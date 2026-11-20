import fs from 'node:fs';
import path from 'node:path';
import {requireThat, command, policy, policyHash, privateDir, sha256, digest, GitHub, validateRun, states} from '../ci/lib.mjs';

export function validateApproval(a, now = Date.now()) {
  requireThat(a.schema === 1 && a.repository === policy.repository && a.branch === policy.branch && /^[a-f0-9]{40}$/.test(a.sha) && /^[a-f0-9]{40}$/.test(a.workflowSha), 'APPROVAL_SOURCE');
  digest(a.manifestDigest);
  requireThat(a.policyHash === policyHash && a.policy === policy.version && /^[a-f0-9]{64}$/.test(a.artifactHash), 'APPROVAL_POLICY');
  requireThat(a.ecr === `${policy.aws.registry}/${policy.aws.repository}`, 'APPROVAL_REGISTRY');
  for (const key of ['ciRun', 'ciAttempt', 'hardeningRun', 'hardeningAttempt']) requireThat(/^\d+$/.test(String(a[key])), 'APPROVAL_RUN');
  requireThat(Number.isFinite(Date.parse(a.createdAt)) && Date.parse(a.createdAt) <= now && Date.parse(a.expiresAt) > now && Date.parse(a.expiresAt) - Date.parse(a.createdAt) <= policy.candidateMaxAgeHours * 3600000, 'APPROVAL_EXPIRED');
  for (const key of ['ci', 'hardening']) requireThat(/^[a-f0-9]{64}$/.test(a.evidenceHashes?.[key]), 'APPROVAL_EVIDENCE_HASH');
  for (const name of ['verify_candidate', ...policy.requiredHardening]) requireThat(a.securityResults.filter(r => r.control === name).length === 1 && a.securityResults.find(r => r.control === name).state === 'PASS', 'APPROVAL_CONTROL_' + name);
  requireThat(a.securityResults.every(r => states.includes(r.state)), 'APPROVAL_UNKNOWN_STATE');
  requireThat(!a.securityResults.some(r => ['FAIL', 'BLOCKED'].includes(r.state)), 'APPROVAL_FINDINGS'); return a;
}
async function jsonFromArtifact(api, artifact, filename) {
  const bytes = await api.download(artifact, 8 * 1024 * 1024);
  fs.mkdirSync(privateDir, {recursive: true, mode: 0o700});
  const file = path.join(privateDir, 'artifact-' + artifact.id + '.zip');
  fs.writeFileSync(file, bytes, {mode: 0o600});
  try {
    const names = command('unzip', ['-Z1', file]).stdout.trim().split('\n');
    requireThat(names.length === 1 && names[0] === filename, 'APPROVAL_ARTIFACT_CONTENTS');
    const text = command('unzip', ['-p', file, filename]).stdout;
    return {value: JSON.parse(text), hash: sha256(text)};
  } finally { fs.rmSync(file, {force: true}); }
}
export async function verifyApproval(a, api = new GitHub()) {
  validateApproval(a);
  const ci = await api.request(`/repos/${policy.repository}/actions/runs/${a.ciRun}`);
  validateRun(ci, a.sha, 'ci');
  requireThat(ci.event === 'push' && ci.conclusion === 'success' && ci.status === 'completed' && String(ci.run_attempt) === String(a.ciAttempt), 'APPROVAL_CI_STATE');
  const hard = await api.request(`/repos/${policy.repository}/actions/runs/${a.hardeningRun}`);
  validateRun(hard, a.workflowSha, 'hardening');
  requireThat(hard.conclusion === 'success' && hard.status === 'completed' && String(hard.run_attempt) === String(a.hardeningAttempt), 'APPROVAL_HARDENING_STATE');
  const jobs = await api.pages(`/repos/${policy.repository}/actions/runs/${a.hardeningRun}/attempts/${a.hardeningAttempt}/jobs`, 'jobs');
  for (const name of ['Hardening Gate', 'Publish Approved Candidate']) requireThat(jobs.filter(j => j.name === name).length === 1 && jobs.find(j => j.name === name).conclusion === 'success', 'APPROVAL_JOB_' + name.replaceAll(' ', '_'));
  const ciEvidence = await jsonFromArtifact(api, await api.artifact(a.ciRun, `ci-report-${a.ciRun}-${a.ciAttempt}`), 'ci-report.json');
  const hardEvidence = await jsonFromArtifact(api, await api.artifact(a.hardeningRun, `hardening-report-${a.hardeningRun}-${a.hardeningAttempt}`), 'hardening-report.json');
  requireThat(ciEvidence.hash === a.evidenceHashes.ci && hardEvidence.hash === a.evidenceHashes.hardening, 'APPROVAL_EVIDENCE_CHANGED');
  requireThat(ciEvidence.value.approved === true && ciEvidence.value.sha === a.sha && ciEvidence.value.policyHash === policyHash && hardEvidence.value.candidate.manifestDigest === a.manifestDigest && hardEvidence.value.candidate.artifactHash === a.artifactHash && hardEvidence.value.policyHash === policyHash, 'APPROVAL_EVIDENCE_CONTENT');
  requireThat(String(ciEvidence.value.run) === String(a.ciRun) && String(ciEvidence.value.attempt) === String(a.ciAttempt) && String(hardEvidence.value.run) === String(a.hardeningRun) && String(hardEvidence.value.attempt) === String(a.hardeningAttempt) && hardEvidence.value.sha === a.sha && hardEvidence.value.workflowSha === a.workflowSha, 'APPROVAL_EVIDENCE_RUN');
  requireThat(JSON.stringify(hardEvidence.value.results.map(r => ({control: r.control, state: r.state}))) === JSON.stringify(a.securityResults), 'APPROVAL_SECURITY_RESULTS_CHANGED');
  return a;
}
export async function approvedCandidates(api = new GitHub()) {
  // Integration order, not last successful workflow, timestamp, tag or mutable latest.
  const order = command('git', ['rev-list', '--first-parent', 'origin/develop']).stdout.trim().split('\n');
  const ranks = new Map(order.map((sha, n) => [sha, n]));
  const runs = await api.pages(`/repos/${policy.repository}/actions/workflows/hardening.yml/runs?branch=develop&status=success`, 'workflow_runs');
  const candidates = [];
  for (const run of runs) {
    if (!['push', 'workflow_dispatch'].includes(run.event) || run.head_repository?.full_name !== policy.repository) continue;
    const name = `approved-candidate-${run.id}-${run.run_attempt}`;
    const artifacts = await api.pages(`/repos/${policy.repository}/actions/runs/${run.id}/artifacts`, 'artifacts');
    const artifact = artifacts.find(a => a.name === name && !a.expired);
    if (!artifact) continue;
    const {value} = await jsonFromArtifact(api, artifact, 'approved.json');
    if (!ranks.has(value.sha) || Date.parse(value.expiresAt) <= Date.now()) continue;
    requireThat(String(value.hardeningRun) === String(run.id) && String(value.hardeningAttempt) === String(run.run_attempt), 'APPROVAL_ARTIFACT_RUN');
    candidates.push(await verifyApproval(value, api));
  }
  return candidates.sort((a, b) => ranks.get(a.sha) - ranks.get(b.sha) || Number(b.hardeningRun) - Number(a.hardeningRun));
}
export async function resolve(api = new GitHub()) {
  const candidates = await approvedCandidates(api);
  requireThat(candidates.length > 0, 'NO_APPROVED_DEVELOP_CANDIDATE', 'BLOCKED');
  return {selected: candidates[0], rollbackInventory: candidates};
}
