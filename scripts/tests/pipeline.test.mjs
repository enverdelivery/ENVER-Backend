import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {classify, parseTrx, aggregate, policy, policyHash, digest, validateRun, root, GitHub} from '../ci/lib.mjs';
import {assessTrivy, verifyContract, contractMethods, iacSourcePaths} from '../ci/pipeline.mjs';
import {hardeningDecision} from '../ci/hardening.mjs';
import {validateApproval} from '../deploy/candidates.mjs';
import {compensate, rollbackOrder, AWSPlane, validateDispatch} from '../deploy/deploy.mjs';

test('uncertain diff runs every technical control', () => {
  const s = classify([], true);
  for (const k of ['build_test', 'integration_validation', 'runtime_tool_tests', 'semgrep', 'dependency_review', 'secret_scan', 'container_lint', 'iac_validation', 'contract_tests', 'docker_build']) assert.equal(s[k], true, k);
});
test('docs-only scope cannot skip secret scanning', () => { const s = classify(['README.md']); assert.equal(s.secret_scan, true); assert.equal(s.docker_build, false); });
test('unknown changes and workflow changes fail safe', () => { for (const p of ['unknown.file', '.github/workflows/ci.yml', 'scripts/ci/lib.mjs']) assert.equal(classify([p]).integration_validation, true); });
test('develop source/runtime/infra candidates run all controls', () => { for (const p of ['Enver.API/Program.cs', 'tools/x.cs', 'infra/main.tf']) assert.equal(classify([p], false, 'push', 'develop').docker_build, true); });
test('PR source test dependency docker classifications', () => {
  for (const p of ['Enver.API/a.cs', 'tests/test.cs', 'Enver.API/packages.lock.json', 'Enver.API/Dockerfile']) assert.equal(classify([p]).contract_tests, true);
  assert.equal(classify(['infra/main.tf']).iac_validation, true);
  assert.equal(classify(['tools/a.cs']).runtime_tool_tests, true);
});
const trx = (c = '', result = 'Passed') => `<TestRun><Results><UnitTestResult testName="Example.test(value: 1)" outcome="${result}" /></Results><ResultSummary><Counters total="1" executed="1" passed="1" ${c} /></ResultSummary></TestRun>`;
test('TRX successful complete suite is accepted without historical counts', () => { assert.equal(parseTrx(trx()).passed, 1); });
test('TRX empty truncated failed skipped and entity payloads fail closed', () => {
  for (const xml of [trx().replace('total="1"', 'total="0"'), trx().slice(0, -10), trx('notExecuted="1"'), trx('', 'NotExecuted'), '<!ENTITY x "x">' + trx()]) assert.throws(() => parseTrx(xml));
});
const id = {repository: policy.repository, sha: 'a'.repeat(40), run: '1', attempt: '1'};
const good = {control: 'build_test', ...id, policyHash, state: 'PASS', reason: 'VERIFIED'};
test('gate rejects missing result, skipped job, foreign attempt, false NA and blocked', () => {
  assert.equal(aggregate(['build_test'], [good], {build_test: {result: 'success'}}, {}, id).length, 1);
  for (const [rows, needs] of [[[], {result: 'success'}], [[good], {result: 'skipped'}], [[{...good, attempt: '2'}], {result: 'success'}], [[{...good, state: 'NOT APPLICABLE', reason: 'PATH_SCOPE'}], {result: 'success'}], [[{...good, state: 'BLOCKED'}], {result: 'success'}]]) assert.throws(() => aggregate(['build_test'], rows, {build_test: needs}, {}, id));
});
test('explicit path-scoped NA is distinct from PASS', () => { assert.equal(aggregate(['build_test'], [{...good, state: 'NOT APPLICABLE', reason: 'PATH_SCOPE'}], {build_test: {result: 'success'}}, {build_test: false}, id)[0].state, 'NOT APPLICABLE'); });
test('container scanner errors and unfixed criticals never pass', () => {
  assert.throws(() => assessTrivy({})); assert.throws(() => assessTrivy({Results: [{Vulnerabilities: [{Severity: 'CRITICAL', FixedVersion: ''}]}]}));
  assert.equal(assessTrivy({Results: []}).highCritical, 0);
});
test('IaC successes are not findings and exceptions require exact resource rule and unexpired approval', () => {
  const exception = policy.iacExceptions[0];
  const f = {Severity: 'CRITICAL', Status: 'EXCEPTION', AVDID: exception.rule, CauseMetadata: {Resource: exception.resource}};
  const data = value => ({Results: [{Misconfigurations: [value]}]});
  assert.equal(assessTrivy(data({...f, Status: 'PASS'})).findings, 0);
  assert.throws(() => assessTrivy(data(f)));
  assert.equal(assessTrivy(data(f), [exception], Date.parse('2026-11-19')).observedExceptions, 1);
  assert.throws(() => assessTrivy(data(f), [exception], Date.parse('2026-12-19')));
  assert.throws(() => assessTrivy(data({...f, CauseMetadata: {Resource: 'different-resource'}}), [exception]));
  assert.throws(() => assessTrivy(data({...f, Status: 'FAIL'}), [exception]));
  assert.throws(() => assessTrivy(data({...f, Status: 'UNKNOWN'}), [exception]));
});
test('contract control requires named real assertions and both suites', () => {
  const r = {state: 'PASS', details: {integration: {total: 20, tests: contractMethods.map(n => 'Namespace.' + n)}, security: {total: 10}}};
  assert.equal(verifyContract(r).methods.length, contractMethods.length);
  assert.throws(() => verifyContract({...r, details: {...r.details, integration: {total: 20, tests: []}}}));
});
const names = ['verify_candidate', 'container_scan', 'codeql', 'mutation', 'dynamic_preflight', 'dast', 'fuzzing'];
test('hardening mandatory scanners cannot be skipped or replaced by optional controls', () => {
  const rows = names.map(control => ({control, state: ['mutation', 'dynamic_preflight', 'dast', 'fuzzing'].includes(control) ? 'NOT EXECUTED' : 'PASS', reason: 'MANUAL_HEAVY_CONTROL_NOT_REQUESTED'}));
  const needs = Object.fromEntries(names.map(n => [n, {result: 'success'}]));
  assert.equal(hardeningDecision(rows, needs).length, 3);
  assert.throws(() => hardeningDecision(rows, {...needs, codeql: {result: 'skipped'}}));
  assert.throws(() => hardeningDecision(rows, needs, {dynamic: true}));
  assert.throws(() => hardeningDecision(rows.filter(r => r.control !== 'mutation'), needs));
  for (const state of ['FAIL', 'BLOCKED', 'SKIPPED', 'NOT APPLICABLE']) assert.throws(() => hardeningDecision(rows.map(r => r.control === 'mutation' ? {...r, state} : r), needs));
});
test('IaC snapshot permits publicable source only, never local plans state variables or caches', () => {
  const allowed = ['infra/security.tf', 'infra/.terraform.lock.hcl', 'infra/bootstrap/compute-host.sh', 'infra/secrets.tf', 'infra/environment.tf.json'];
  const privatePaths = ['infra/terraform.tfstate', 'infra/terraform.tfvars', 'infra/t019-core4.tfvars.json', 'infra/plan.tfplan', 'infra/.terraform/hidden.tf', 'infra/../secret.tf', 'outside/main.tf'];
  assert.deepEqual(iacSourcePaths([...allowed, ...privatePaths, allowed[0]]), allowed);
});
test('egress exceptions are exact approved resources with expiry, no all-protocol public rule', () => {
  const source = fs.readFileSync(path.join(root, 'infra/security.tf'), 'utf8');
  assert.doesNotMatch(source, /ip_protocol\s*=\s*"-1"/);
  const annotated = [...source.matchAll(/#trivy:ignore:(AVD-AWS-\d+):exp:(\d{4}-\d{2}-\d{2})\s*resource "aws_vpc_security_group_egress_rule" "([^"]+)"/g)];
  assert.equal(annotated.length, 2); assert.equal(policy.iacExceptions.length, 2);
  for (const [_, rule, expires, resource] of annotated) {
    const exception = policy.iacExceptions.find(e => e.resource === 'aws_vpc_security_group_egress_rule.' + resource);
    assert.equal(exception?.rule, rule); assert.equal(exception.expires, expires);
    assert.equal(exception.approval, 'EXPLICIT_FIX_PIPELINE'); assert.ok(exception.owner);
    assert.match(source, new RegExp('"' + resource + '" \\{[^}]*from_port\\s*=\\s*' + exception.port + '[^}]*to_port\\s*=\\s*' + exception.port));
  }
});
test('approval must bind repository branch run attempts policy freshness and digest', () => {
  const a = {schema: 1, repository: policy.repository, branch: 'develop', sha: 'a'.repeat(40), workflowSha: 'b'.repeat(40), manifestDigest: 'sha256:' + 'c'.repeat(64), policyHash, policy: policy.version, artifactHash: 'd'.repeat(64), ecr: `${policy.aws.registry}/${policy.aws.repository}`, ciRun: '1', ciAttempt: '1', hardeningRun: '2', hardeningAttempt: '1', evidenceHashes: {ci: 'a'.repeat(64), hardening: 'b'.repeat(64)}, createdAt: new Date(Date.now() - 10000).toISOString(), expiresAt: new Date(Date.now() + 100000).toISOString(), securityResults: ['verify_candidate', ...policy.requiredHardening].map(control => ({control, state: 'PASS'}))};
  assert.equal(validateApproval(a), a);
  for (const patch of [{branch: 'main'}, {policyHash: 'e'.repeat(64)}, {expiresAt: '2000-01-01'}, {manifestDigest: 'latest'}, {ciAttempt: null}, {securityResults: []}]) assert.throws(() => validateApproval({...a, ...patch}));
});
test('manifest authority is not a mutable tag or config/image id', () => { assert.throws(() => digest('latest')); assert.throws(() => digest('sha256:abc')); assert.equal(digest('sha256:' + 'a'.repeat(64)).length, 71); });
test('foreign run, PR event and different workflow are rejected', () => {
  const run = {repository: {full_name: policy.repository}, head_repository: {full_name: policy.repository}, head_branch: 'develop', head_sha: 'a'.repeat(40), path: '.github/workflows/ci.yml', event: 'push'};
  validateRun(run, run.head_sha, 'ci');
  for (const patch of [{head_branch: 'main'}, {event: 'pull_request'}, {path: '.github/workflows/other.yml'}, {head_repository: {full_name: 'fork/repo'}}]) assert.throws(() => validateRun({...run, ...patch}, run.head_sha, 'ci'));
});
function state(nodes) { return {nodes, previousDigest: 'sha256:' + 'a'.repeat(64), deploymentOutcome: 'FAILED'}; }
function plane(calls, fail = '') { return {
  async reconcile(n) { calls.push('reconcile ' + n); if (fail === 'reconcile ' + n) throw Error('uncertain'); },
  async restore(n) { calls.push('restore ' + n); if (fail === 'restore ' + n) throw Error('fixture'); },
  async ready(n) { calls.push('ready ' + n); }, async finalHealth() { calls.push('final health'); },
}; }
test('A failure only compensates A and remains FAILED', async () => { const calls = [], s = state({A: {attempted: true}, B: {}}); await compensate(s, plane(calls)); assert.deepEqual(calls, ['restore A', 'ready A', 'final health']); assert.equal(s.deploymentOutcome, 'FAILED'); assert.equal(s.rollbackOutcome, 'PASS'); });
test('B and final-health failures compensate B then A', async () => { const calls = [], s = state({A: {attempted: true}, B: {attempted: true}}); await compensate(s, plane(calls)); assert.deepEqual(rollbackOrder(s), ['B', 'A']); assert.deepEqual(calls, ['restore B', 'ready B', 'restore A', 'ready A', 'final health']); assert.equal(s.deploymentOutcome, 'FAILED'); });
test('uncertain SSM state is reconciled before restore; no blind retry', async () => { const calls = [], s = state({A: {attempted: true}, B: {attempted: true, uncertain: true}}); await compensate(s, plane(calls, 'reconcile B')); assert.equal(calls.includes('restore B'), false); assert.equal(calls.includes('restore A'), false); assert.equal(s.rollbackOutcome, 'BLOCKED'); assert.equal(s.deploymentOutcome, 'FAILED'); });
test('workflows count names triggers and immutable action pins', () => {
  const dir = path.join(root, '.github/workflows');
  assert.deepEqual(fs.readdirSync(dir).sort(), ['ci.yml', 'deploy-dev.yml', 'hardening.yml']);
  const yaml = fs.readFileSync(path.join(dir, 'deploy-dev.yml'), 'utf8').replaceAll('\r\n', '\n');
  const triggers = yaml.split('permissions:')[0];
  assert.match(yaml, /^name: Deploy ENVER Dev$/m); assert.match(triggers, /^  workflow_dispatch:$/m);
  assert.doesNotMatch(triggers, /^  (?:push|pull_request|schedule|workflow_run|workflow_call):/m);
  assert.match(yaml, /group: enver-development-deploy\n  cancel-in-progress: false/);
  const ci = fs.readFileSync(path.join(dir, 'ci.yml'), 'utf8');
  const gate = ci.slice(ci.indexOf('\n  pr_quality_gate:'));
  assert.match(gate, /if: always\(\)/);
  assert.match(gate, /- if: always\(\)\s+run: node scripts\/ci\/pipeline\.mjs pr_quality_gate/);
  assert.doesNotMatch(ci, /id-token:|configure-aws-credentials|AWS_ACCESS_KEY_ID|AWS_SECRET_ACCESS_KEY/);
  const detect = ci.slice(ci.indexOf('\n  detect_changes:'), ci.indexOf('\n  build_test:'));
  assert.doesNotMatch(detect, /^    (?:needs|if):/m);
  assert.doesNotMatch(ci.split('\njobs:')[0], /paths(?:-ignore)?:/);
  assert.doesNotMatch(fs.readFileSync(path.join(dir, 'hardening.yml'), 'utf8'), /^  workflow_run:/m);
  for (const file of fs.readdirSync(dir)) {
    const text = fs.readFileSync(path.join(dir, file), 'utf8');
    for (const [, action] of text.matchAll(/uses: ([^\s#]+)/g)) assert.ok(action.startsWith('./') || /@[a-f0-9]{40}$/.test(action), action);
    const count = [...text.matchAll(/^  [a-z_]+:\s*$/gm)].filter(m => m.index > text.indexOf('\njobs:')).length;
    assert.equal(count, file === 'ci.yml' ? 12 : 10);
  }
});
test('SSM document is deterministic fixed source without shell input interpolation', () => {
  const agent = fs.readFileSync(path.join(root, 'scripts/deploy/remote_agent.py'), 'utf8').replaceAll('\r\n', '\n');
  const document = JSON.parse(fs.readFileSync(path.join(root, 'scripts/deploy/ssm-document.json')));
  assert.ok(document.mainSteps[0].inputs.runCommand[0].includes(agent));
  assert.ok(Object.values(document.parameters).every(p => p.interpolationType === 'ENV_VAR'));
  assert.equal(document.mainSteps[0].inputs.runCommand[0].includes('{{'), false);
});
test('IAM roles have distinct workflow-bound subjects and no admin or mutation outside fixed SSM', () => {
  const iam = n => JSON.parse(fs.readFileSync(path.join(root, 'scripts/deploy/iam/' + n + '.json')));
  assert.notEqual(iam('publisher-trust').Statement[0].Condition.StringEquals['token.actions.githubusercontent.com:sub'], iam('deploy-trust').Statement[0].Condition.StringEquals['token.actions.githubusercontent.com:sub']);
  const actions = iam('deploy-permissions').Statement.flatMap(s => s.Action);
  assert.ok(!actions.includes('iam:*') && !actions.includes('ssm:*') && !actions.includes('ec2:*'));
  assert.deepEqual(iam('github-oidc-subject').include_claim_keys, ['repo', 'context', 'workflow_ref', 'event_name']);
});
test('official dependency comparison endpoint is allowed, path traversal is not', async t => {
  const request = t.mock.method(globalThis, 'fetch', async () => new Response('[]', {status: 200, headers: {'content-type': 'application/json'}}));
  const api = new GitHub('synthetic');
  await api.request(`/repos/${policy.repository}/dependency-graph/compare/${'a'.repeat(40)}...${'b'.repeat(40)}`);
  assert.equal(request.mock.callCount(), 1);
  await assert.rejects(() => api.request('/repos/../other'));
});
test('SSM destination/operation/digest rejection precedes any AWS call', async () => {
  const s = {...state({A: {}, B: {}}), run: '123', attempt: '1', candidate: {manifestDigest: 'sha256:' + 'a'.repeat(64)}};
  const p = new AWSPlane(s); p.aws = () => { throw Error('AWS must not be reached'); };
  await assert.rejects(() => p.remote('C', 'deploy'));
  await assert.rejects(() => p.remote('A', 'shell'));
  await assert.rejects(() => p.remote('A', 'deploy', 'latest'));
});
test('lost SendCommand response is not retried and retains uncertain state', async () => {
  const s = {...state({A: {attempted: true}, B: {}}), run: '123', attempt: '1', candidate: {manifestDigest: 'sha256:' + 'a'.repeat(64)}};
  const p = new AWSPlane(s), calls = [];
  p.aws = (service, operation) => { calls.push(operation); throw Error('synthetic transport uncertainty'); };
  await assert.rejects(() => p.remote('A', 'deploy'));
  assert.deepEqual(calls, ['send-command']); assert.equal(s.nodes.A.uncertain, true);
  await assert.rejects(() => p.reconcile('A', s.nodes.A.mutationCommandId));
  assert.deepEqual(calls, ['send-command']);
});
test('failed B compensation does not endanger known healthy A', async () => {
  const calls = [], s = state({A: {attempted: true}, B: {attempted: true}});
  await compensate(s, plane(calls, 'restore B'));
  assert.equal(calls.includes('restore A'), false); assert.equal(s.rollbackOutcome, 'BLOCKED'); assert.equal(s.deploymentOutcome, 'FAILED');
});
test('dispatch validates human triggering actor, develop, first attempt and explicit allowlist', () => {
  const env = {GITHUB_REPOSITORY: policy.repository, GITHUB_EVENT_NAME: 'workflow_dispatch', GITHUB_REF: 'refs/heads/develop', GITHUB_ACTOR: 'operator', GITHUB_TRIGGERING_ACTOR: 'operator', GITHUB_RUN_ATTEMPT: '1', ENVER_DEPLOY_ACTORS: 'operator', ENVER_ENVIRONMENT: 'development'};
  validateDispatch(env);
  for (const patch of [{GITHUB_REF: 'refs/heads/main'}, {GITHUB_EVENT_NAME: 'push'}, {GITHUB_RUN_ATTEMPT: '2'}, {GITHUB_TRIGGERING_ACTOR: 'automation[bot]'}, {ENVER_DEPLOY_ACTORS: ''}]) assert.throws(() => validateDispatch({...env, ...patch}));
});
