import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {Closed, requireThat, command, root, policy, policyHash, privateDir, evidence, readJson, save, sha256, identity, output, emit, exactSDK, filesBelow, container, GitHub, validateRun} from './lib.mjs';
import {inspectOCI, assessTrivy} from './pipeline.mjs';
import {trivyDiagnostic, observeTrivyVersion, trivyImageArguments, codeqlFindings} from './hardening-diagnostics.mjs';

const hardControls = ['verify_candidate', 'container_scan', 'codeql', 'mutation', 'dynamic_preflight', 'dast', 'fuzzing'];
function trustedRef() {
  requireThat(process.env.GITHUB_REPOSITORY === policy.repository && process.env.GITHUB_REF === 'refs/heads/develop' && ['push', 'workflow_dispatch'].includes(process.env.GITHUB_EVENT_NAME), 'UNTRUSTED_HARDENING_CONTEXT');
}
function oneFile(name) {
  const found = filesBelow(evidence).filter(p => path.basename(p) === name);
  requireThat(found.length === 1, 'MISSING_OR_AMBIGUOUS_' + name.replace(/\W/g, '_')); return found[0];
}
export async function locate(api = new GitHub()) {
  trustedRef();
  let runId = process.env.ENVER_CI_RUN;
  requireThat(!runId || /^\d+$/.test(runId), 'CI_RUN_INPUT');
  for (let n = 0; n < 90; n++) {
    let run;
    if (runId) run = await api.request(`/repos/${policy.repository}/actions/runs/${runId}`);
    else {
      const response = await api.request(`/repos/${policy.repository}/actions/workflows/ci.yml/runs?branch=develop&event=push&head_sha=${process.env.GITHUB_SHA}&per_page=100`);
      const eligible = response.workflow_runs.filter(r => r.head_sha === process.env.GITHUB_SHA && r.event === 'push');
      requireThat(eligible.length <= 1, 'AMBIGUOUS_CI_RUN'); run = eligible[0];
    }
    if (run) {
      validateRun(run, run.head_sha, 'ci');
      requireThat(run.event === 'push', 'CI_MUST_BE_TRUSTED_PUSH');
      const reachable = command('git', ['merge-base', '--is-ancestor', run.head_sha, 'origin/develop'], {allowFailure: true});
      requireThat(reachable.status === 0, 'CI_NOT_REACHABLE_DEVELOP');
      if (run.status === 'completed') {
        requireThat(run.conclusion === 'success', 'CI_NOT_SUCCESS');
        const artifact = await api.artifact(run.id, `oci-${run.id}-${run.run_attempt}`);
        requireThat(artifact.workflow_run?.head_sha === run.head_sha, 'OCI_ARTIFACT_SOURCE');
        output('ci_run', run.id); output('ci_attempt', run.run_attempt); output('sha', run.head_sha);
        output('oci_name', artifact.name); output('ci_report_name', `ci-report-${run.id}-${run.run_attempt}`);
        return run;
      }
    }
    await new Promise(resolve => setTimeout(resolve, 20000));
  }
  throw new Closed('CI_WAIT_EXPIRED', 'BLOCKED');
}
export async function verify() {
  trustedRef(); const api = new GitHub();
  const candidate = readJson(oneFile('candidate.json')), ci = readJson(oneFile('ci-report.json'));
  const run = await api.request(`/repos/${policy.repository}/actions/runs/${candidate.run}`);
  validateRun(run, candidate.sha, 'ci');
  requireThat(run.status === 'completed' && run.conclusion === 'success' && String(run.run_attempt) === candidate.attempt, 'CI_ATTEMPT_OR_RESULT');
  const artifact = await api.artifact(candidate.run, `oci-${candidate.run}-${candidate.attempt}`);
  requireThat(artifact.workflow_run?.head_sha === candidate.sha && candidate.repository === policy.repository && candidate.policyHash === policyHash, 'CANDIDATE_SOURCE');
  requireThat(ci.approved === true && ['sha', 'run', 'attempt', 'repository', 'policyHash'].every(k => ci[k] === candidate[k]), 'CI_REPORT_IDENTITY');
  const build = ci.results?.filter(r => r.control === 'docker_build');
  requireThat(build?.length === 1 && build[0].state === 'PASS' && ['manifestDigest', 'artifactHash', 'sha', 'run', 'attempt'].every(k => build[0].details[k] === candidate[k]), 'CANDIDATE_NOT_BOUND_TO_CI_GATE');
  const actual = inspectOCI(oneFile('image.tar'));
  requireThat(actual.manifestDigest === candidate.manifestDigest && actual.artifactHash === candidate.artifactHash, 'OCI_IDENTITY_MISMATCH');
  requireThat(candidate.sha === process.env.ENVER_SOURCE_SHA, 'CANDIDATE_INPUT_SHA');
  return {candidate, ci, artifactId: artifact.id, ciReportHash: sha256(fs.readFileSync(oneFile('ci-report.json')))};
}
async function scanImage() {
  const actual = inspectOCI(oneFile('image.tar'), path.join(privateDir, 'trivy-image'));
  const candidate = readJson(oneFile('candidate.json'));
  requireThat(actual.manifestDigest === candidate.manifestDigest && actual.artifactHash === candidate.artifactHash, 'SCAN_IMAGE_CHANGED');
  const r = container('trivy', trivyImageArguments, {network: 'bridge'});
  const diagnostic = trivyDiagnostic(r, {reportPath: path.join(privateDir, 'image-scan.json'),
    targetDigest: actual.manifestDigest, toolImage: policy.images.trivy, declaredVersion: policy.toolVersions.trivy});
  diagnostic.sourceArchiveHash = actual.artifactHash;
  diagnostic.layoutManifestDigest = actual.manifestDigest;
  const diagnosticPath = path.join(evidence, 'trivy-diagnostic.json');
  save(diagnosticPath, diagnostic);
  // Sondeo posterior del digest cacheado: --pull=never evita reintentar una descarga fallida.
  // Sin red, mounts ni influencia en el resultado del scan; no cambia su presupuesto.
  try {
    observeTrivyVersion(diagnostic, command('docker', ['run', '--rm', '--pull=never', '--platform', 'linux/amd64',
      '--network', 'none', '--read-only', '--cap-drop', 'ALL', '--security-opt', 'no-new-privileges',
      ...(typeof process.getuid === 'function' ? ['--user', `${process.getuid()}:${process.getgid()}`] : []),
      '--env', 'HOME=/tmp', policy.images.trivy, '--version'], {timeout: 15000, allowFailure: true}));
  } catch { diagnostic.toolVersionSource = 'VERSION_PROBE_UNAVAILABLE'; }
  save(diagnosticPath, diagnostic);
  requireThat(r.status === 0, 'TRIVY_IMAGE_EXECUTION', 'BLOCKED');
  try {
    const assessment = assessTrivy(readJson(path.join(privateDir, 'image-scan.json')));
    diagnostic.stage = 'TRIVY_POLICY_PASS'; diagnostic.policyEvaluation = 'PASS';
    return {...assessment, manifestDigest: actual.manifestDigest};
  } catch (error) {
    diagnostic.stage = error.code === 'TRIVY_HIGH_CRITICAL' ? 'TRIVY_FINDINGS' : 'TRIVY_REPORT_FAILURE';
    diagnostic.policyEvaluation = 'FAIL'; throw error;
  } finally { save(diagnosticPath, diagnostic); }
}
async function codeqlPreflight() {
  const repo = await new GitHub().request(`/repos/${policy.repository}`);
  const enabled = repo.visibility === 'public' || repo.security_and_analysis?.advanced_security?.status === 'enabled' || process.env.ENVER_CODEQL_ENABLED === 'true';
  output('enabled', enabled ? 'true' : 'false');
  requireThat(enabled, 'CODEQL_SETTINGS_OR_LICENSE_PENDING', 'BLOCKED'); return {enabled};
}
async function codeqlResult() {
  const files = filesBelow(path.join(privateDir, 'codeql')).filter(f => f.endsWith('.sarif'));
  const diagnosticPath = path.join(evidence, 'codeql-findings.json');
  save(diagnosticPath, {schema: 1, sarifGenerated: files.length > 0, sarifFiles: files.length,
    parseState: 'NOT_COMPLETED', totalResults: null, blockingResultCount: null, findings: []});
  requireThat(files.length > 0, 'CODEQL_NO_SARIF', 'BLOCKED');
  let count = 0;
  const documents = [];
  for (const file of files) {
    const data = readJson(file); requireThat(data.runs?.length > 0 && data.runs.every(r => Array.isArray(r.results)), 'CODEQL_INCOMPLETE');
    count += data.runs.reduce((n, r) => n + r.results.length, 0);
    documents.push(data);
  }
  save(diagnosticPath, {...codeqlFindings(documents, {repository: root}), parseState: 'PASS'});
  requireThat(count === 0, 'CODEQL_FINDINGS'); return {sarifRuns: files.length, findings: count};
}
async function mutation() {
  if (process.env.ENVER_MUTATION !== 'true') return {state: 'NOT EXECUTED', reason: 'MANUAL_HEAVY_CONTROL_NOT_REQUESTED'};
  exactSDK(); command('dotnet', ['tool', 'restore']); command('dotnet', ['restore', 'Enver.Backend.slnx', '--locked-mode']);
  command('dotnet', ['stryker', '--config-file', 'scripts/ci/stryker-config.json', '--output', path.join(privateDir, 'mutation')], {timeout: 45 * 60 * 1000});
  const reports = filesBelow(path.join(privateDir, 'mutation')).filter(p => p.endsWith('mutation-report.json'));
  requireThat(reports.length === 1, 'MUTATION_REPORT_MISSING');
  const data = readJson(reports[0]); const mutants = Object.values(data.files || {}).flatMap(f => f.mutants || []);
  requireThat(mutants.length > 0, 'MUTATION_EMPTY');
  const valid = mutants.filter(m => !['CompileError', 'Ignored'].includes(m.status));
  requireThat(valid.length > 0 && mutants.every(m => ['Killed', 'Timeout', 'Survived', 'NoCoverage', 'CompileError', 'Ignored'].includes(m.status)), 'MUTATION_INCOMPLETE');
  const score = valid.filter(m => ['Killed', 'Timeout'].includes(m.status)).length / valid.length * 100;
  requireThat(score >= policy.mutationThreshold, 'MUTATION_THRESHOLD'); return {mutants: mutants.length, score, threshold: policy.mutationThreshold};
}
function dynamic() {
  // No HTTP lab/OpenAPI-completeness evidence exists yet. Never fall back to shared DEV.
  return process.env.ENVER_DYNAMIC === 'true'
    ? {state: 'BLOCKED', reason: 'ISOLATED_HTTP_TARGET_AND_VERIFIED_OPENAPI_REQUIRED'}
    : {state: 'NOT EXECUTED', reason: 'MANUAL_HEAVY_CONTROL_NOT_REQUESTED'};
}
function dynamicConsumer() {
  return {state: process.env.ENVER_DYNAMIC === 'true' ? 'BLOCKED' : 'NOT EXECUTED', reason: 'SAFE_DYNAMIC_PREFLIGHT_NOT_SATISFIED'};
}
export function hardeningDecision(results, needs, {mutation = false, dynamic = false} = {}) {
  const required = ['verify_candidate', ...policy.requiredHardening, ...(mutation ? ['mutation'] : []), ...(dynamic ? ['dynamic_preflight', 'dast', 'fuzzing'] : [])];
  for (const name of hardControls) {
    requireThat(results.filter(r => r.control === name).length === 1, 'HARDENING_EVIDENCE_MISSING_' + name);
    requireThat(needs[name]?.result === 'success', 'UNEXPECTED_HARDENING_JOB_' + name);
  }
  for (const name of required) requireThat(results.find(r => r.control === name)?.state === 'PASS' && needs[name]?.result === 'success', 'HARDENING_REQUIRED_' + name);
  // Optional controls may be unrequested, not failed/skipped/unknown. A declared
  // non-execution is never promoted to PASS and becomes blocking if requested.
  for (const r of results) requireThat(r.state === 'PASS' || !required.includes(r.control) && r.state === 'NOT EXECUTED' &&
    ['MANUAL_HEAVY_CONTROL_NOT_REQUESTED', 'SAFE_DYNAMIC_PREFLIGHT_NOT_SATISFIED'].includes(r.reason), 'HARDENING_FAILURE_' + r.control);
  return required;
}
async function report() {
  const data = filesBelow(evidence).filter(f => f.endsWith('.json')).map(readJson);
  const results = data.filter(r => hardControls.includes(r.control));
  const ciFiles = filesBelow(evidence).filter(p => path.basename(p) === 'ci-report.json');
  const candidateFiles = filesBelow(evidence).filter(p => path.basename(p) === 'candidate.json');
  const ci = ciFiles.length === 1 ? readJson(ciFiles[0]) : {results: [], approved: false};
  const candidate = candidateFiles.length === 1 ? readJson(candidateFiles[0]) : results.find(r => r.control === 'verify_candidate' && r.state === 'PASS')?.details.candidate || {sha: 'NOT RESOLVED', manifestDigest: 'NOT RESOLVED'};
  const id = identity();
  requireThat(results.every(r => ['repository', 'sha', 'run', 'attempt'].every(k => r[k] === id[k]) && r.policyHash === policyHash), 'FOREIGN_HARDENING_RESULT');
  const needs = JSON.parse(process.env.NEEDS_JSON || '{}');
  for (const control of hardControls) {
    if (!results.some(r => r.control === control)) results.push({...id, control, state: 'NOT EXECUTED', reason: 'MISSING_EVIDENCE_JOB_' + (needs[control]?.result || 'UNKNOWN').toUpperCase()});
  }
  const summary = {...id, workflowSha: process.env.GITHUB_SHA, policy: policy.version, policyHash,
    candidate, ciApproved: ci.approved === true, ciReportHash: ciFiles.length === 1 ? sha256(fs.readFileSync(ciFiles[0])) : null, ci: ci.results, results,
    requested: {mutation: process.env.ENVER_MUTATION === 'true', dynamic: process.env.ENVER_DYNAMIC === 'true'}};
  save(path.join(evidence, 'hardening-report.json'), summary);
  output('report_hash', sha256(fs.readFileSync(path.join(evidence, 'hardening-report.json'))));
  if (process.env.GITHUB_STEP_SUMMARY) {
    const lines = [...(ci.results || []), ...results].map(r => `| ${r.control} | ${r.state} | ${r.reason} |`);
    fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, `# Hardening Report\nCommit: ${candidate.sha}\n\nManifest: ${candidate.manifestDigest}\n\n| Control | Estado | Motivo |\n|---|---|---|\n${lines.join('\n')}\n\nAUTH-027 y aceptación distribuida: NO ACREDITADOS por este reporte.\n`);
  }
  return {states: results.map(r => ({control: r.control, state: r.state}))};
}
async function gate() {
  const summary = readJson(oneFile('hardening-report.json'));
  requireThat(summary.policyHash === policyHash && summary.sha === process.env.ENVER_SOURCE_SHA, 'HARDENING_POLICY_OR_SHA');
  requireThat(summary.ciApproved === true && summary.candidate.sha === summary.sha, 'HARDENING_CI_EVIDENCE_REQUIRED');
  requireThat(JSON.parse(process.env.NEEDS_JSON || '{}').hardening_report?.result === 'success', 'HARDENING_REPORT_JOB_FAILED');
  const required = hardeningDecision(summary.results, JSON.parse(process.env.NEEDS_JSON || '{}'), summary.requested);
  return {required, reportHash: sha256(fs.readFileSync(oneFile('hardening-report.json')))};
}
function publisherPreflight() {
  trustedRef();
  requireThat(process.env.ENVER_PUBLISH_ENABLED === 'true', 'PUBLISH_CONFIGURATION_PENDING', 'BLOCKED');
  requireThat(process.env.ENVER_PUBLISHER_ROLE === `arn:aws:iam::${policy.aws.account}:role/enver-github-ecr-publisher`, 'PUBLISHER_ROLE_CONFIGURATION_PENDING', 'BLOCKED');
  return {configured: true};
}
async function publish() {
  publisherPreflight(); const verified = await verify();
  const summaryPath = oneFile('hardening-report.json'), summary = readJson(summaryPath);
  requireThat(sha256(fs.readFileSync(summaryPath)) === process.env.ENVER_HARDENING_REPORT_HASH && summary.policyHash === policyHash, 'APPROVAL_REPORT_HASH');
  requireThat(summary.ciApproved === true && ['verify_candidate', ...policy.requiredHardening].every(c => summary.results.filter(r => r.control === c).length === 1 && summary.results.find(r => r.control === c).state === 'PASS') && summary.candidate.manifestDigest === verified.candidate.manifestDigest, 'PUBLISH_GATE_MISMATCH');
  const account = JSON.parse(command('aws', ['sts', 'get-caller-identity', '--output', 'json']).stdout);
  requireThat(account.Account === policy.aws.account && account.Arn.includes(':assumed-role/enver-github-ecr-publisher/'), 'PUBLISH_IDENTITY');
  const password = command('aws', ['ecr', 'get-login-password', '--region', policy.aws.region]).stdout.trim();
  requireThat(password.length > 0, 'ECR_TOKEN_EMPTY');
  const authPath = path.join(privateDir, 'ecr-auth.json');
  save(authPath, {auths: {[policy.aws.registry]: {auth: Buffer.from('AWS:' + password).toString('base64')}}});
  const candidate = verified.candidate, archive = oneFile('image.tar');
  const tag = `candidate-${candidate.sha}-${candidate.run}-${candidate.attempt}`;
  try {
    const r = container('skopeo', ['copy', '--preserve-digests', '--authfile', '/private/ecr-auth.json', 'oci-archive:/image/image.tar', `docker://${policy.aws.registry}/${policy.aws.repository}:${tag}`], {network: 'bridge', writable: [[path.dirname(archive), '/image']]});
    requireThat(r.status === 0, 'ECR_SAME_MANIFEST_COPY');
  } finally { fs.rmSync(authPath, {force: true}); }
  const remote = JSON.parse(command('aws', ['ecr', 'describe-images', '--region', policy.aws.region, '--repository-name', policy.aws.repository, '--image-ids', `imageTag=${tag}`, '--output', 'json']).stdout);
  requireThat(remote.imageDetails?.length === 1 && remote.imageDetails[0].imageDigest === candidate.manifestDigest, 'ECR_MANIFEST_MISMATCH');
  const approval = {schema: 1, repository: policy.repository, branch: policy.branch, sha: candidate.sha,
    ciRun: candidate.run, ciAttempt: candidate.attempt, hardeningRun: identity().run, hardeningAttempt: identity().attempt,
    workflowSha: process.env.GITHUB_SHA, manifestDigest: candidate.manifestDigest, artifactHash: candidate.artifactHash,
    policy: policy.version, policyHash, evidenceHashes: {ci: verified.ciReportHash, hardening: sha256(fs.readFileSync(summaryPath))},
    securityResults: summary.results.map(r => ({control: r.control, state: r.state})), createdAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + policy.candidateMaxAgeHours * 3600000).toISOString(), ecr: `${policy.aws.registry}/${policy.aws.repository}`, tag};
  save(path.join(evidence, 'approval', 'approved.json'), approval);
  return {manifestDigest: approval.manifestDigest, candidateSha: approval.sha, registryVerified: true};
}
const handlers = {locate, verify_candidate: verify, container_scan: scanImage, codeql_preflight: codeqlPreflight, codeql: codeqlResult, mutation, dynamic_preflight: dynamic, dast: dynamicConsumer, fuzzing: dynamicConsumer, hardening_report: report, hardening_gate: gate, publisher_preflight: publisherPreflight, publish_candidate: publish};
async function main(control) {
  requireThat(Object.hasOwn(handlers, control), 'UNKNOWN_HARDENING_CONTROL');
  try {
    const data = await handlers[control]();
    if (control === 'locate' || control === 'publisher_preflight' || control === 'codeql_preflight') return;
    const state = data.state || 'PASS', reason = data.reason || 'VERIFIED';
    emit(control, state, reason, data);
    if (['FAIL', 'BLOCKED'].includes(state)) process.exitCode = 1;
  } catch (e) {
    emit(control === 'codeql_preflight' ? 'codeql' : control, e instanceof Closed ? e.state : 'BLOCKED', e instanceof Closed ? e.code : 'HARDENING_TOOL_OR_RESULT_ERROR');
    process.exitCode = 1;
  }
}
if (process.argv[1] && fileURLToPath(import.meta.url) === path.resolve(process.argv[1])) await main(process.argv[2]);
