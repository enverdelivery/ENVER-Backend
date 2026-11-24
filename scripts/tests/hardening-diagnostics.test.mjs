import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {spawnSync} from 'node:child_process';
import {trivyDiagnostic, observeTrivyVersion, codeqlFindings, scannerLimits, trivyImageArguments} from '../ci/hardening-diagnostics.mjs';
import {assessTrivy} from '../ci/pipeline.mjs';
import {root, policy} from '../ci/lib.mjs';

function temporary(body) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'enver-scanner-evidence-'));
  try { return body(dir); } finally { fs.rmSync(dir, {recursive: true, force: true}); }
}
function diagnostic(dir, result = {status: 0}, data) {
  const reportPath = path.join(dir, 'report.json');
  if (data !== undefined) fs.writeFileSync(reportPath, JSON.stringify(data));
  return trivyDiagnostic(result, {reportPath, targetDigest: 'sha256:' + 'a'.repeat(64),
    toolImage: policy.images.trivy, declaredVersion: policy.toolVersions.trivy, env: {}});
}
function sarif(results, rules = []) { return {runs: [{tool: {driver: {rules}}, results}]}; }
const findings = documents => codeqlFindings(documents, {repository: root, env: {}});

test('Trivy execution failure is not inferred to be vulnerability findings', () => temporary(dir => {
  for (const status of [1, 125, 126, 127, null]) {
    const d = diagnostic(dir, {status, error: {code: 'ETIMEDOUT'}, signal: 'SIGTERM'});
    assert.equal(d.exitCode, status); assert.equal(d.exitCodeSource, 'DOCKER_RUN');
    assert.equal(d.nativeTrivyExitCode, status === 1 ? 1 : null);
    assert.equal(d.stage, 'TRIVY_EXECUTION_FAILURE'); assert.equal(d.policyEvaluation, 'NOT_REACHED');
    assert.equal(d.highCount, null); assert.equal(d.criticalCount, null);
    assert.equal(d.reportGenerated, false); assert.equal(d.processError, 'ETIMEDOUT');
  }
}));
test('Trivy valid reports retain separate HIGH/CRITICAL counts including unfixed issues and SHA256', () => temporary(dir => {
  const data = {Results: [{Vulnerabilities: [{Severity: 'HIGH'}, {Severity: 'CRITICAL', FixedVersion: ''}, {Severity: 'LOW'}]}]};
  const d = diagnostic(dir, {status: 0}, data);
  assert.equal(d.reportGenerated, true); assert.equal(d.reportState, 'VALID_JSON');
  assert.equal(d.highCount, 1); assert.equal(d.criticalCount, 1); assert.match(d.reportHash, /^[a-f0-9]{64}$/);
  assert.equal(d.reportBytes, Buffer.byteLength(JSON.stringify(data)));
  assert.equal(d.stage, 'TRIVY_PROCESS_COMPLETED');
  assert.throws(() => assessTrivy(data), {code: 'TRIVY_HIGH_CRITICAL'});
  assert.equal(assessTrivy({Results: []}).highCritical, 0);
}));
test('Trivy partial report cannot turn a nonzero execution into successful evaluation', () => temporary(dir => {
  const d = diagnostic(dir, {status: 2}, {Results: [{Vulnerabilities: [{Severity: 'HIGH'}]}]});
  assert.equal(d.highCount, 1); assert.equal(d.stage, 'TRIVY_EXECUTION_FAILURE');
  assert.equal(d.policyEvaluation, 'NOT_REACHED');
}));
test('Trivy missing invalid malformed and oversized reports never invent zero findings', () => temporary(dir => {
  for (const data of [{}, {Results: [null]}, {Results: [{Vulnerabilities: {}}]}, {Results: [{Vulnerabilities: [null]}]}]) {
    assert.equal(diagnostic(dir, {status: 0}, data).highCount, null);
  }
  fs.writeFileSync(path.join(dir, 'report.json'), '{broken');
  assert.equal(diagnostic(dir).reportState, 'UNREADABLE_OR_INVALID_JSON');
  fs.writeFileSync(path.join(dir, 'report.json'), ' '.repeat(scannerLimits.inputBytes));
  const d = diagnostic(dir);
  assert.equal(d.reportState, 'NOT_REGULAR_OR_OVERSIZED'); assert.equal(d.reportHash, null);
}));
test('Trivy excerpts are redacted before aggressive truncation and do not export arbitrary process fields', () => temporary(dir => {
  const secret = 'scanner-canary-sensitive-value';
  const env = {GH_TOKEN: secret};
  const d = trivyDiagnostic({status: 1, env, stdout: 'x'.repeat(9000) + secret,
    stderr: `token=${secret}\nhttps://user:pass@example.invalid/private?q=${secret}\n${Buffer.from(secret).toString('base64')}`},
  {reportPath: path.join(dir, 'absent'), env, targetDigest: 'sha256:' + 'a'.repeat(64),
    toolImage: policy.images.trivy, declaredVersion: '0.61.1'});
  const json = JSON.stringify(d);
  assert.doesNotMatch(json, new RegExp(secret)); assert.ok(!json.includes(Buffer.from(secret).toString('base64')));
  assert.ok(!json.includes('user:pass')); assert.equal(d.env, undefined);
  assert.ok(d.stdoutExcerpt.length <= 2048); assert.ok(d.stderrExcerpt.length <= 2048);
  assert.deepEqual(d.command.slice(-trivyImageArguments.length), trivyImageArguments);
}));
test('observed version is distinguished from policy version and never overwrites scan exit/stage', () => temporary(dir => {
  const d = diagnostic(dir, {status: 125});
  observeTrivyVersion(d, {status: 0, stdout: 'Version: 0.61.1\n'});
  assert.equal(d.toolVersion, '0.61.1'); assert.equal(d.toolVersionSource, 'SAME_DIGEST_VERSION_COMMAND');
  assert.equal(d.exitCode, 125); assert.equal(d.stage, 'TRIVY_EXECUTION_FAILURE');
  observeTrivyVersion(d, {status: null, error: {code: 'ETIMEDOUT'}});
  assert.equal(d.toolVersion, null); assert.equal(d.versionProbeError, 'ETIMEDOUT');
}));
test('scanner-specific auth assignments and known secrets are removed before clipping', () => temporary(dir => {
  const secret = 'prefix-token=synthetic-canary-fragment';
  const d = trivyDiagnostic({status: 1, stderr: `start ${secret}\nregistry_auth=synthetic-private-material\nclient_secret=synthetic-private-material`},
    {reportPath: path.join(dir, 'absent'), env: {GH_TOKEN: secret}});
  assert.ok(!JSON.stringify(d).includes('synthetic-canary-fragment'));
  assert.ok(!JSON.stringify(d).includes('synthetic-private-material'));
}));
test('SARIF derives rule severity relative location range and message without snippets', () => {
  const d = findings([sarif([{ruleId: 'cs/example', ruleIndex: 0, message: {text: 'Investigate this flow'},
    locations: [{physicalLocation: {artifactLocation: {uri: 'Enver.API/Example.cs'}, region: {startLine: 4, endLine: 8, snippet: {text: 'DO_NOT_EXPORT'}}}}]}],
  [{id: 'cs/example', properties: {'security-severity': '8.1'}, defaultConfiguration: {level: 'error'}}])]);
  assert.equal(d.totalResults, 1); assert.equal(d.blockingResultCount, 1); assert.equal(d.sarifGenerated, true);
  assert.deepEqual(d.findings[0], {index: 0, resultIndex: 0, ruleId: 'cs/example', level: 'error', securitySeverity: '8.1',
    file: 'Enver.API/Example.cs', startLine: 4, endLine: 8, message: 'Investigate this flow', baselineState: 'UNKNOWN', truePositive: 'NEEDS_REVIEW'});
  assert.ok(!JSON.stringify(d).includes('DO_NOT_EXPORT'));
});
test('SARIF preserves zero-result policy including informational or suppressed results', () => {
  const d = findings([sarif([{level: 'none', suppressions: [{kind: 'inSource'}]}, {level: 'note'}]), sarif([{}])]);
  assert.equal(d.totalResults, 3); assert.equal(d.blockingResultCount, 3);
  assert.deepEqual(d.findings.map(f => f.index), [0, 1, 2]);
  assert.deepEqual(d.findings.map(f => f.resultIndex), [0, 1, 0]);
  assert.equal(findings([sarif([])]).blockingResultCount, 0);
});
test('SARIF resolves artifact index and file URI but omits external absolute traversal and URL paths', () => {
  const document = sarif(['file://' + root.replaceAll('\\', '/') + '/Enver.API/A.cs', '../private.cs', '/elsewhere/private.cs',
    'https://user:pass@host/private?q=secret', '%2e%2e/private.cs', 'C:\\elsewhere\\private.cs', '%00secret', 'file://external/private.cs'].map(uri =>
    ({locations: [{physicalLocation: {artifactLocation: {uri}}}]})));
  document.runs[0].artifacts = [{location: {uri: 'Enver.API/Indexed.cs'}}];
  document.runs[0].results.push({locations: [{physicalLocation: {artifactLocation: {index: 0}}}]});
  const d = findings([document]);
  assert.equal(d.findings[0].file, 'Enver.API/A.cs');
  for (const row of d.findings.slice(1, -1)) assert.equal(row.file, null);
  assert.equal(d.findings.at(-1).file, 'Enver.API/Indexed.cs');
});
test('SARIF output is bounded with exact totals and explicit omissions; messages sanitized', () => {
  const secret = 'secret-canary-for-sarif';
  const d = codeqlFindings([sarif(Array.from({length: 150}, () => ({ruleId: 'r'.repeat(1000),
    message: {text: 'x'.repeat(4000) + secret}})))], {repository: root, env: {GH_TOKEN: secret}});
  assert.equal(d.totalResults, 150); assert.equal(d.blockingResultCount, 150);
  assert.equal(d.findings.length, 100); assert.equal(d.omittedResults, 50); assert.equal(d.truncated, true);
  assert.ok(Buffer.byteLength(JSON.stringify(d)) < scannerLimits.artifactBytes);
  assert.ok(!JSON.stringify(d).includes(secret));
});
test('real CodeQL wrapper preserves derived evidence before failing, and empty SARIF still passes', () => temporary(dir => {
  const privateDir = path.join(dir, 'enver-ci-private', 'codeql');
  const evidenceDir = path.join(dir, 'evidence');
  fs.mkdirSync(privateDir, {recursive: true});
  const env = {...process.env, RUNNER_TEMP: dir, ENVER_EVIDENCE_DIR: evidenceDir, GITHUB_SHA: 'a'.repeat(40), ENVER_SOURCE_SHA: 'a'.repeat(40)};
  delete env.GITHUB_OUTPUT; delete env.GITHUB_STEP_SUMMARY;
  for (const results of [[{ruleId: 'cs/test', message: {text: 'Synthetic only'}}], []]) {
    fs.writeFileSync(path.join(privateDir, 'csharp.sarif'), JSON.stringify(sarif(results)));
    const run = spawnSync(process.execPath, ['scripts/ci/hardening.mjs', 'codeql'], {cwd: root, env, encoding: 'utf8'});
    assert.equal(run.status, results.length ? 1 : 0, run.stderr);
    const evidence = JSON.parse(fs.readFileSync(path.join(evidenceDir, 'codeql-findings.json')));
    const control = JSON.parse(fs.readFileSync(path.join(evidenceDir, 'codeql.json')));
    assert.equal(evidence.parseState, 'PASS'); assert.equal(evidence.blockingResultCount, results.length);
    assert.equal(control.state, results.length ? 'FAIL' : 'PASS');
    assert.equal(control.reason, results.length ? 'CODEQL_FINDINGS' : 'VERIFIED');
  }
  fs.rmSync(path.join(privateDir, 'csharp.sarif'));
  const missing = spawnSync(process.execPath, ['scripts/ci/hardening.mjs', 'codeql'], {cwd: root, env, encoding: 'utf8'});
  assert.equal(missing.status, 1);
  assert.equal(JSON.parse(fs.readFileSync(path.join(evidenceDir, 'codeql-findings.json'))).sarifGenerated, false);
  assert.equal(JSON.parse(fs.readFileSync(path.join(evidenceDir, 'codeql.json'))).reason, 'CODEQL_NO_SARIF');
}));
test('workflow always publishes only derived scanner artifacts and never enables SARIF upload', () => {
  const yaml = fs.readFileSync(path.join(root, '.github/workflows/hardening.yml'), 'utf8');
  assert.match(yaml, /upload: never/);
  for (const name of ['trivy-diagnostic.json', 'codeql-findings.json']) assert.ok(yaml.includes('/enver-evidence/' + name));
  for (const section of [yaml.split('  container_scan:')[1].split('  codeql:')[0], yaml.split('  codeql:')[1].split('  mutation:')[0]]) {
    assert.match(section, /if: always\(\)/); assert.doesNotMatch(section, /continue-on-error|\.sarif/);
  }
  const source = fs.readFileSync(path.join(root, 'scripts/ci/hardening.mjs'), 'utf8');
  assert.match(source, /'--pull=never'/); // La consulta de versión no reintenta pull ni scan.
});
