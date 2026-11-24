import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {approvalTimestamps} from '../ci/hardening.mjs';
import {validateApproval} from '../deploy/candidates.mjs';
import {policy, policyHash, root} from '../ci/lib.mjs';

const issuedAtMs = Date.parse('2026-11-24T18:00:00.123Z');
const maxAgeMs = policy.candidateMaxAgeHours * 3600000;
function approval(t, createdAtMs = issuedAtMs) {
  t.mock.method(Date, 'now', () => createdAtMs);
  return {schema: 1, repository: policy.repository, branch: policy.branch,
    sha: 'a'.repeat(40), workflowSha: 'b'.repeat(40), manifestDigest: 'sha256:' + 'c'.repeat(64),
    policy: policy.version, policyHash, artifactHash: 'd'.repeat(64),
    ecr: `${policy.aws.registry}/${policy.aws.repository}`,
    ciRun: '1', ciAttempt: '1', hardeningRun: '2', hardeningAttempt: '1',
    evidenceHashes: {ci: 'e'.repeat(64), hardening: 'f'.repeat(64)},
    securityResults: ['verify_candidate', ...policy.requiredHardening].map(control => ({control, state: 'PASS'})),
    ...approvalTimestamps()};
}

test('approval timestamps capture one clock instant even when subsequent reads advance', t => {
  let reads = 0;
  const clock = t.mock.method(Date, 'now', () => issuedAtMs + 43 * reads++);
  const timestamps = approvalTimestamps();
  assert.equal(clock.mock.callCount(), 1);
  assert.equal(timestamps.createdAt, new Date(issuedAtMs).toISOString());
  assert.equal(timestamps.expiresAt, new Date(issuedAtMs + maxAgeMs).toISOString());
  assert.equal(Date.parse(timestamps.expiresAt) - Date.parse(timestamps.createdAt), maxAgeMs);
  assert.equal(Date.now(), issuedAtMs + 43); // A second read would reproduce the historical excess.
});

test('generated approval duration is exactly the configured 168 hours and the strict validator accepts it', t => {
  const a = approval(t);
  assert.equal(maxAgeMs, 604800000);
  assert.equal(Date.parse(a.expiresAt) - Date.parse(a.createdAt), maxAgeMs);
  assert.equal(validateApproval(a, issuedAtMs), a);
  assert.equal(validateApproval(a, issuedAtMs + maxAgeMs - 1), a);
});

test('publisher uses the tested timestamp helper without independent clock reads or timestamp overrides', () => {
  const source = fs.readFileSync(path.join(root, 'scripts/ci/hardening.mjs'), 'utf8');
  const publisher = source.slice(source.indexOf('async function publish()'), source.indexOf('const handlers ='));
  assert.equal((publisher.match(/\.\.\.approvalTimestamps\(\)/g) || []).length, 1);
  assert.doesNotMatch(publisher, /\b(?:createdAt|expiresAt)\s*:|Date\.now\(|new Date\(/);
});

test('strict validator rejects even one millisecond over max age, including observed historical excesses', t => {
  const a = approval(t);
  for (const excess of [1, 6, 43, 120]) {
    const oversized = {...a, expiresAt: new Date(issuedAtMs + maxAgeMs + excess).toISOString()};
    assert.throws(() => validateApproval(oversized, issuedAtMs), {code: 'APPROVAL_EXPIRED'});
  }
});

test('strict validator rejects expiresAt equal to or earlier than validation time', t => {
  const a = approval(t);
  for (const now of [issuedAtMs + maxAgeMs, issuedAtMs + maxAgeMs + 1])
    assert.throws(() => validateApproval(a, now), {code: 'APPROVAL_EXPIRED'});
});

test('strict validator still rejects an approval older than the configured max age', t => {
  const old = approval(t, issuedAtMs - maxAgeMs - 1);
  assert.equal(Date.parse(old.expiresAt), issuedAtMs - 1);
  assert.throws(() => validateApproval(old, issuedAtMs), {code: 'APPROVAL_EXPIRED'});
});

test('strict validator still rejects invalid or future createdAt values', t => {
  const a = approval(t);
  for (const createdAt of ['invalid', '', null, new Date(issuedAtMs + 1).toISOString()])
    assert.throws(() => validateApproval({...a, createdAt}, issuedAtMs), {code: 'APPROVAL_EXPIRED'});
});

test('strict validator still rejects invalid expiresAt values', t => {
  const a = approval(t);
  for (const expiresAt of ['invalid', '', null])
    assert.throws(() => validateApproval({...a, expiresAt}, issuedAtMs), {code: 'APPROVAL_EXPIRED'});
});
