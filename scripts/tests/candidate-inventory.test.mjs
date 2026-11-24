import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {execFileSync} from 'node:child_process';
import {Closed, GitHub, policy, policyHash, sha256} from '../ci/lib.mjs';
import {approvedCandidates, resolve, validateApproval, verifyApproval} from '../deploy/candidates.mjs';

const now = Date.parse('2026-11-24T19:00:00.000Z');
const maxAgeMs = policy.candidateMaxAgeHours * 3600000;
const base = `/repos/${policy.repository}/actions`;

// A real single-file ZIP exercises production archive size/hash/content checks.
// Only the GitHub transport is mocked; no approval verifier is replaced.
function zip(filename, text) {
  const name = Buffer.from(filename), content = Buffer.from(text);
  let crc = 0xffffffff;
  for (const byte of content) {
    crc ^= byte;
    for (let bit = 0; bit < 8; bit++) crc = (crc >>> 1) ^ (crc & 1 ? 0xedb88320 : 0);
  }
  crc = (crc ^ 0xffffffff) >>> 0;
  const local = Buffer.alloc(30), central = Buffer.alloc(46), end = Buffer.alloc(22);
  local.writeUInt32LE(0x04034b50); local.writeUInt16LE(20, 4);
  local.writeUInt32LE(crc, 14); local.writeUInt32LE(content.length, 18);
  local.writeUInt32LE(content.length, 22); local.writeUInt16LE(name.length, 26);
  central.writeUInt32LE(0x02014b50); central.writeUInt16LE(20, 4); central.writeUInt16LE(20, 6);
  central.writeUInt32LE(crc, 16); central.writeUInt32LE(content.length, 20);
  central.writeUInt32LE(content.length, 24); central.writeUInt16LE(name.length, 28);
  end.writeUInt32LE(0x06054b50); end.writeUInt16LE(1, 8); end.writeUInt16LE(1, 10);
  end.writeUInt32LE(central.length + name.length, 12);
  end.writeUInt32LE(local.length + name.length + content.length, 16);
  return Buffer.concat([local, name, content, central, name, end]);
}
const jsonText = value => JSON.stringify(value) + '\n';

class FixtureGitHub extends GitHub {
  constructor() {
    super('synthetic-fixture');
    this.responses = new Map(); this.errors = new Map(); this.calls = [];
    this.runs = []; this.approvalDownloads = []; this.artifacts = new Map(); this.nextArtifact = 1;
    this.responses.set(`${base}/workflows/hardening.yml/runs?branch=develop&status=success`, {workflow_runs: this.runs});
  }
  async request(endpoint, {binary = false} = {}) {
    this.calls.push(endpoint);
    if (this.errors.has(endpoint)) throw this.errors.get(endpoint);
    assert.ok(this.responses.has(endpoint), `Unexpected fixture request: ${endpoint}`);
    if (binary) {
      const id = Number(endpoint.split('/').at(-2));
      const artifact = this.artifacts.get(id);
      if (artifact.name.startsWith('approved-candidate-')) this.approvalDownloads.push(artifact.name);
    }
    return this.responses.get(endpoint);
  }
  async pages(endpoint, key) { return (await this.request(endpoint))[key]; }
  putArtifact(run, name, filename, value) {
    const text = typeof value === 'string' ? value : jsonText(value), bytes = zip(filename, text);
    const artifact = {id: this.nextArtifact++, name, expired: false, size_in_bytes: bytes.length, digest: 'sha256:' + sha256(bytes)};
    const endpoint = `${base}/runs/${run}/artifacts`;
    if (!this.responses.has(endpoint)) this.responses.set(endpoint, {artifacts: []});
    this.responses.get(endpoint).artifacts.push(artifact);
    this.responses.set(`${base}/artifacts/${artifact.id}/zip`, bytes);
    this.artifacts.set(artifact.id, artifact);
    return artifact;
  }
}

function fixture(t) {
  const dir = fs.mkdtempSync(path.join(process.env.RUNNER_TEMP || os.tmpdir(), 'enver-inventory-fixture-'));
  const history = path.join(dir, 'history.git');
  const keys = ['GIT_DIR', 'GIT_WORK_TREE', 'PATH'];
  const original = Object.fromEntries(keys.map(key => [key, process.env[key]]));
  t.after(() => {
    for (const key of keys) if (original[key] === undefined) delete process.env[key]; else process.env[key] = original[key];
    fs.rmSync(dir, {recursive: true, force: true});
  });
  // Git for Windows supplies the same unzip dependency used by the resolver.
  if (process.platform === 'win32') {
    const git = execFileSync('where.exe', ['git'], {encoding: 'utf8'}).trim().split(/\r?\n/)[0];
    const bin = path.resolve(path.dirname(git), '../usr/bin');
    if (fs.existsSync(path.join(bin, 'unzip.exe'))) process.env.PATH = bin + path.delimiter + process.env.PATH;
  }
  const env = {...process.env}; delete env.GIT_DIR; delete env.GIT_WORK_TREE;
  execFileSync('git', ['init', '--bare', '--object-format=sha1', history], {env, stdio: 'pipe'});
  const git = (args, input) => execFileSync('git', ['--git-dir=' + history, ...args], {env, input, encoding: 'utf8'}).trim();
  const tree = git(['hash-object', '-t', 'tree', '--stdin', '-w'], '');
  const commits = [];
  for (let i = 0; i < 3; i++) commits.push(git(['hash-object', '-t', 'commit', '--stdin', '-w'],
    `tree ${tree}\n${i ? `parent ${commits[i - 1]}\n` : ''}author Fixture <fixture@example.invalid> 1700000000 +0000\ncommitter Fixture <fixture@example.invalid> 1700000000 +0000\n\nfixture-${i}\n`));
  git(['update-ref', 'refs/remotes/origin/develop', commits[2]]);
  process.env.GIT_DIR = history; delete process.env.GIT_WORK_TREE;
  t.mock.method(Date, 'now', () => now);
  const warnings = []; t.mock.method(console, 'warn', message => warnings.push(message));
  const api = new FixtureGitHub();
  function add({run, sha = commits[2], durationMs = maxAgeMs, createdAtMs = now - 60000, patch = {}, ciPatch = {}, hardPatch = {}}) {
    const ciRun = String(run + 100000), hardeningRun = String(run);
    const results = ['verify_candidate', ...policy.requiredHardening].map(control => ({control, state: 'PASS'}));
    const candidate = {manifestDigest: 'sha256:' + sha256(`fixture-manifest-${run}`), artifactHash: sha256(`fixture-archive-${run}`)};
    const ci = {approved: true, sha, policyHash, run: ciRun, attempt: '1', ...ciPatch};
    const hard = {sha, workflowSha: sha, policyHash, run: hardeningRun, attempt: '1', candidate, results, ...hardPatch};
    const approval = {schema: 1, repository: policy.repository, branch: policy.branch, sha, workflowSha: sha,
      ...candidate, policy: policy.version, policyHash, ecr: `${policy.aws.registry}/${policy.aws.repository}`,
      ciRun, ciAttempt: '1', hardeningRun, hardeningAttempt: '1',
      createdAt: new Date(createdAtMs).toISOString(), expiresAt: new Date(createdAtMs + durationMs).toISOString(),
      evidenceHashes: {ci: sha256(jsonText(ci)), hardening: sha256(jsonText(hard))}, securityResults: results};
    Object.assign(approval, typeof patch === 'function' ? patch(approval) : patch);
    const runMetadata = (id, workflow) => ({id: Number(id), head_sha: sha, head_branch: policy.branch,
      repository: {full_name: policy.repository}, head_repository: {full_name: policy.repository},
      path: `.github/workflows/${workflow}.yml`, event: 'push', conclusion: 'success', status: 'completed', run_attempt: 1});
    const ciRunMetadata = runMetadata(ciRun, 'ci'), hardRunMetadata = runMetadata(hardeningRun, 'hardening');
    api.responses.set(`${base}/runs/${ciRun}`, ciRunMetadata);
    api.responses.set(`${base}/runs/${hardeningRun}`, hardRunMetadata);
    api.responses.set(`${base}/runs/${hardeningRun}/attempts/1/jobs`, {jobs: ['Hardening Gate', 'Publish Approved Candidate'].map(name => ({name, conclusion: 'success'}))});
    const ciArtifact = api.putArtifact(ciRun, `ci-report-${ciRun}-1`, 'ci-report.json', ci);
    const hardArtifact = api.putArtifact(hardeningRun, `hardening-report-${hardeningRun}-1`, 'hardening-report.json', hard);
    const artifact = api.putArtifact(hardeningRun, `approved-candidate-${hardeningRun}-1`, 'approved.json', approval);
    api.runs.push(hardRunMetadata);
    return {approval, artifact, ciArtifact, hardArtifact, ciRunMetadata, hardRunMetadata};
  }
  return {api, add, warnings, commits};
}

test('valid candidate survives expired and overlong historical approvals with individual rejection diagnostics', async t => {
  const f = fixture(t), valid = f.add({run: 31});
  const expired = f.add({run: 21, sha: f.commits[1], createdAtMs: now - maxAgeMs});
  const overlong = f.add({run: 11, sha: f.commits[0], durationMs: maxAgeMs + 43});
  const result = await resolve(f.api);
  assert.equal(result.selected.manifestDigest, valid.approval.manifestDigest);
  assert.deepEqual(result.rollbackInventory.map(a => a.hardeningRun), ['31']);
  assert.deepEqual(f.api.approvalDownloads, [valid, expired, overlong].map(v => v.artifact.name));
  assert.equal(f.warnings.length, 2);
  for (const v of [expired, overlong]) assert.ok(f.warnings.some(w => w.includes(v.artifact.name) && w.includes('APPROVAL_EXPIRED')));
});

test('multiple temporal-invalid candidates cannot poison discovery of a later independent valid approval', async t => {
  const f = fixture(t);
  for (const [i, excess] of [43, 6, 120, 1].entries()) f.add({run: 50 + i, sha: f.commits[0], durationMs: maxAgeMs + excess});
  f.add({run: 60, patch: {createdAt: 'invalid'}});
  f.add({run: 61, patch: {createdAt: new Date(now + 1).toISOString()}});
  f.add({run: 62, patch: {expiresAt: 'invalid'}});
  const valid = f.add({run: 7});
  assert.equal((await resolve(f.api)).selected.manifestDigest, valid.approval.manifestDigest);
  assert.equal(f.warnings.length, 7);
});

test('resolver is BLOCKED when every independent approval is temporally invalid', async t => {
  const f = fixture(t);
  f.add({run: 15, durationMs: maxAgeMs + 43});
  f.add({run: 16, createdAtMs: now - maxAgeMs - 1});
  assert.deepEqual(await approvedCandidates(f.api), []);
  await assert.rejects(() => resolve(f.api), {code: 'NO_APPROVED_DEVELOP_CANDIDATE', state: 'BLOCKED'});
});

test('inventory preserves the exact max-age boundary: 604800000 accepted, 604800001 rejected', async t => {
  const f = fixture(t);
  assert.equal(maxAgeMs, 604800000);
  const valid = f.add({run: 8}), invalid = f.add({run: 9, durationMs: maxAgeMs + 1});
  assert.equal(validateApproval(valid.approval), valid.approval);
  assert.throws(() => validateApproval(invalid.approval), {code: 'APPROVAL_EXPIRED'});
  assert.deepEqual((await approvedCandidates(f.api)).map(a => a.manifestDigest), [valid.approval.manifestDigest]);
});

test('selection remains first-parent integration order, then descending run ID only for the same SHA', async t => {
  const f = fixture(t);
  const older = f.add({run: 900, sha: f.commits[0]});
  const newest = f.add({run: 17});
  const middle = f.add({run: 1000, sha: f.commits[1]});
  const sameShaLaterRun = f.add({run: 18});
  const result = await resolve(f.api);
  assert.deepEqual(result.rollbackInventory.map(a => a.manifestDigest), [sameShaLaterRun, newest, middle, older].map(v => v.approval.manifestDigest));
  assert.equal(result.selected.hardeningRun, '18');
});

test('public verifyApproval still rejects temporal-invalid approvals instead of accepting a partial verification', async t => {
  const f = fixture(t), invalid = f.add({run: 42, durationMs: maxAgeMs + 43});
  await assert.rejects(() => verifyApproval(invalid.approval, f.api), {code: 'APPROVAL_EXPIRED'});
  assert.deepEqual(f.api.calls, []);
});

for (const kind of ['approval ZIP', 'CI report ZIP', 'Hardening report ZIP']) test(`${kind} hash mismatch after a valid approval still aborts inventory`, async t => {
  const f = fixture(t), valid = f.add({run: 71}), bad = f.add({run: 72, sha: f.commits[1]});
  const artifact = kind === 'approval ZIP' ? bad.artifact : kind === 'CI report ZIP' ? bad.ciArtifact : bad.hardArtifact;
  artifact.digest = 'sha256:' + '0'.repeat(64);
  await assert.rejects(() => resolve(f.api), {code: 'ARTIFACT_HASH'});
  assert.ok(f.api.calls.includes(`${base}/runs/${valid.approval.ciRun}`));
  assert.equal(f.warnings.length, 0);
});

for (const key of ['ci', 'hardening']) test(`${key} report evidence-hash mismatch after a valid approval still aborts inventory`, async t => {
  const f = fixture(t); f.add({run: 81});
  f.add({run: 82, patch: a => ({evidenceHashes: {...a.evidenceHashes, [key]: '0'.repeat(64)}})});
  await assert.rejects(() => resolve(f.api), {code: 'APPROVAL_EVIDENCE_CHANGED'});
  assert.equal(f.warnings.length, 0);
});

const critical = [
  ['policy hash', {policyHash: '0'.repeat(64)}, 'APPROVAL_POLICY'],
  ['policy version', {policy: 'untrusted-policy'}, 'APPROVAL_POLICY'],
  ['repository', {repository: 'other/repository'}, 'APPROVAL_SOURCE'],
  ['branch', {branch: 'main'}, 'APPROVAL_SOURCE'],
  ['schema', {schema: 2}, 'APPROVAL_SOURCE'],
  ['registry', {ecr: 'other.invalid/other'}, 'APPROVAL_REGISTRY'],
  ['artifact run identity', {hardeningRun: '9999'}, 'APPROVAL_ARTIFACT_RUN'],
  ['workflow SHA', {workflowSha: 'b'.repeat(40)}, 'UNTRUSTED_RUN'],
  ['manifest binding', {manifestDigest: 'sha256:' + '0'.repeat(64)}, 'APPROVAL_EVIDENCE_CONTENT'],
  ['candidate artifact binding', {artifactHash: '0'.repeat(64)}, 'APPROVAL_EVIDENCE_CONTENT'],
];
for (const [name, patch, code] of critical) test(`${name} mismatch after a valid approval remains fail-closed`, async t => {
  const f = fixture(t); f.add({run: 91}); f.add({run: 92, patch});
  await assert.rejects(() => resolve(f.api), {code});
  assert.equal(f.warnings.length, 0);
});

for (const field of ['head_sha', 'path', 'head_branch']) test(`CI provenance ${field} mismatch after a valid approval remains fail-closed`, async t => {
  const f = fixture(t); f.add({run: 101}); const bad = f.add({run: 102});
  bad.ciRunMetadata[field] = field === 'head_sha' ? f.commits[0] : 'untrusted';
  await assert.rejects(() => resolve(f.api), {code: 'UNTRUSTED_RUN'});
  assert.equal(f.warnings.length, 0);
});

test('a temporal defect cannot hide malformed local evidence hashes', async t => {
  const f = fixture(t); f.add({run: 111});
  f.add({run: 112, durationMs: maxAgeMs + 43, patch: {evidenceHashes: {ci: 'invalid', hardening: 'invalid'}}});
  await assert.rejects(() => resolve(f.api), {code: 'APPROVAL_EVIDENCE_HASH'});
  assert.equal(f.warnings.length, 0);
});

test('a temporal defect cannot hide non-PASS security findings', async t => {
  const f = fixture(t); f.add({run: 117});
  f.add({run: 118, durationMs: maxAgeMs + 43,
    patch: a => ({securityResults: [...a.securityResults, {control: 'extra_check', state: 'FAIL'}]})});
  await assert.rejects(() => resolve(f.api), {code: 'APPROVAL_FINDINGS'});
  assert.equal(f.warnings.length, 0);
});

test('a temporal defect cannot hide a policy hash mismatch', async t => {
  const f = fixture(t); f.add({run: 119});
  f.add({run: 120, durationMs: maxAgeMs + 43, patch: {policyHash: '0'.repeat(64)}});
  await assert.rejects(() => resolve(f.api), {code: 'APPROVAL_POLICY'});
  assert.equal(f.warnings.length, 0);
});

for (const key of ['ci', 'hardening']) test(`a temporal defect cannot hide a ${key} report hash mismatch`, async t => {
  const f = fixture(t); f.add({run: 113});
  f.add({run: 114, durationMs: maxAgeMs + 43,
    patch: a => ({evidenceHashes: {...a.evidenceHashes, [key]: '0'.repeat(64)}})});
  await assert.rejects(() => resolve(f.api), {code: 'APPROVAL_EVIDENCE_CHANGED'});
  assert.equal(f.warnings.length, 0);
});

test('a temporal defect cannot hide a run provenance mismatch', async t => {
  const f = fixture(t); f.add({run: 115});
  const bad = f.add({run: 116, durationMs: maxAgeMs + 43});
  bad.ciRunMetadata.head_sha = f.commits[0];
  await assert.rejects(() => resolve(f.api), {code: 'UNTRUSTED_RUN'});
  assert.equal(f.warnings.length, 0);
});

test('malformed security results are not reclassified as temporal rejection', async t => {
  const f = fixture(t); f.add({run: 121});
  f.add({run: 122, durationMs: maxAgeMs + 43, patch: {securityResults: null}});
  await assert.rejects(() => resolve(f.api), TypeError);
  assert.equal(f.warnings.length, 0);
});

for (const error of [new Error('unexpected fixture API error'), Object.assign(new Error('not a validation error'), {code: 'APPROVAL_EXPIRED'}), new Closed('APPROVAL_EXPIRED')])
  test(`unexpected API exception propagates without temporal suppression: ${error.message} / ${error.constructor.name}`, async t => {
    const f = fixture(t); f.add({run: 131}); const bad = f.add({run: 132});
    f.api.errors.set(`${base}/runs/${bad.approval.ciRun}`, error);
    await assert.rejects(() => resolve(f.api), actual => actual === error);
    assert.equal(f.warnings.length, 0);
  });
