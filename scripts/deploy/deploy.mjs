import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {Closed, requireThat, command, root, policy, evidence, readJson, save, sha256, identity, digest, filesBelow, output, GitHub} from '../ci/lib.mjs';
import {resolve, verifyApproval} from './candidates.mjs';

export const phases = ['resolve', 'preflight', 'deploy_a', 'ready_a', 'deploy_b', 'ready_b', 'post', 'compensate'];
export function latestState(dir = evidence) {
  const states = filesBelow(dir).filter(p => path.basename(p) === 'state.json').map(readJson);
  requireThat(states.length > 0, 'DEPLOYMENT_STATE_MISSING');
  requireThat(states.every(s => s.run === identity().run && s.attempt === identity().attempt), 'FOREIGN_DEPLOYMENT_STATE');
  return states.sort((a, b) => phases.indexOf(b.phase) - phases.indexOf(a.phase))[0];
}
export function rollbackOrder(s) {
  return ['B', 'A'].filter(n => s.nodes?.[n]?.attempted);
}
export async function compensate(s, plane) {
  s.deploymentOutcome = 'FAILED'; s.rollbackAttempted = true; s.rollbackOutcome = 'PASS';
  for (const node of rollbackOrder(s)) {
    try {
      if (s.nodes[node].uncertain) await plane.reconcile(node, s.nodes[node].mutationCommandId);
      await plane.restore(node, s.previousDigest); await plane.ready(node, s.previousDigest);
      s.nodes[node].rollback = 'PASS';
    } catch {
      s.nodes[node].rollback = 'BLOCKED'; s.rollbackOutcome = 'BLOCKED';
      // Keep the other healthy target serving if this node cannot be reconciled/restored safely.
      break;
    }
  }
  try { await plane.finalHealth(); } catch { s.rollbackOutcome = 'FAIL'; }
  return s;
}
export function validateDispatch(env = process.env) {
  requireThat(env.GITHUB_REPOSITORY === policy.repository && env.GITHUB_EVENT_NAME === 'workflow_dispatch' && env.GITHUB_REF === 'refs/heads/develop', 'UNTRUSTED_DISPATCH');
  requireThat(!env.GITHUB_ACTOR?.endsWith('[bot]') && env.GITHUB_ACTOR, 'HUMAN_DISPATCH_REQUIRED');
  const triggering = env.GITHUB_TRIGGERING_ACTOR || env.GITHUB_ACTOR;
  requireThat(!triggering.endsWith('[bot]') && env.GITHUB_RUN_ATTEMPT === '1', 'DEPLOY_RERUN_OR_BOT_REQUIRES_RECONCILIATION', 'BLOCKED');
  const actors = (env.ENVER_DEPLOY_ACTORS || '').split(',').map(s => s.trim()).filter(Boolean);
  requireThat(actors.length > 0 && actors.includes(env.GITHUB_ACTOR) && actors.includes(triggering), 'AUTHORIZED_DEPLOY_ACTORS_REQUIRED', 'BLOCKED');
  requireThat(env.ENVER_ENVIRONMENT === policy.environment, 'DEPLOY_ENVIRONMENT');
}
function configured() {
  validateDispatch();
  requireThat(process.env.ENVER_DEPLOY_ENABLED === 'true', 'DEPLOY_CONFIGURATION_PENDING', 'BLOCKED');
  requireThat(process.env.ENVER_DEPLOY_ROLE === `arn:aws:iam::${policy.aws.account}:role/enver-github-deploy-dev`, 'DEPLOY_ROLE_CONFIGURATION_PENDING', 'BLOCKED');
}
export class AWSPlane {
  constructor(s, persist = () => {}) { this.s = s; this.persist = persist; }
  aws(service, operation, args = []) {
    return JSON.parse(command('aws', [service, operation, '--region', policy.aws.region, '--output', 'json', ...args], {timeout: 45000, env: {...process.env, AWS_MAX_ATTEMPTS: '1', AWS_RETRY_MODE: 'standard', AWS_PAGER: ''}}).stdout || '{}');
  }
  async document() {
    const expected = readJson(path.join(root, 'scripts/deploy/ssm-document.json'));
    const live = this.aws('ssm', 'get-document', ['--name', policy.aws.ssmDocument, '--document-version', policy.aws.ssmVersion, '--document-format', 'JSON']);
    const canonical = v => JSON.stringify(v, (key, value) => value && typeof value === 'object' && !Array.isArray(value) ? Object.fromEntries(Object.entries(value).sort(([a], [b]) => a.localeCompare(b))) : value);
    requireThat(canonical(JSON.parse(live.Content)) === canonical(expected) && live.HashType === 'Sha256' && /^[a-f0-9]{64}$/.test(live.Hash), 'SSM_DOCUMENT_NOT_PINNED', 'BLOCKED');
    this.s.documentHash = live.Hash;
  }
  async ecr(manifestDigest) {
    digest(manifestDigest);
    const r = this.aws('ecr', 'describe-images', ['--repository-name', policy.aws.repository, '--image-ids', 'imageDigest=' + manifestDigest]);
    requireThat(r.imageDetails?.length === 1 && r.imageDetails[0].imageDigest === manifestDigest, 'ECR_CANDIDATE_ABSENT');
  }
  async reconcile(node, commandId) {
    requireThat(/^[0-9a-f-]{36}$/.test(commandId || ''), 'SSM_COMMAND_ID_UNCERTAIN', 'BLOCKED');
    const until = Date.now() + 660000;
    for (let n = 0; n < 132 && Date.now() < until; n++) {
      let r;
      try { r = this.aws('ssm', 'get-command-invocation', ['--command-id', commandId, '--instance-id', policy.aws.instances[node]]); }
      catch { await new Promise(r => setTimeout(r, 5000)); continue; }
      if (['Pending', 'InProgress', 'Delayed', 'Cancelling'].includes(r.Status)) { await new Promise(r => setTimeout(r, 5000)); continue; }
      requireThat(['Success', 'Failed', 'Cancelled', 'TimedOut'].includes(r.Status), 'SSM_UNKNOWN_TERMINAL', 'BLOCKED');
      this.s.nodes[node].uncertain = false; this.persist(this.s);
      return r;
    }
    throw new Closed('SSM_EXECUTION_STILL_UNCERTAIN', 'BLOCKED');
  }
  async remote(node, operation, manifestDigest = this.s.candidate.manifestDigest) {
    requireThat(Object.hasOwn(policy.aws.instances, node) && ['snapshot', 'deploy', 'restore', 'verify', 'finalize'].includes(operation), 'REMOTE_OPERATION');
    const params = {Operation: [operation], DesiredDigest: [digest(manifestDigest)], PreviousDigest: [digest(this.s.previousDigest || manifestDigest)], DeploymentId: [`${this.s.run}-${this.s.attempt}`]};
    const mutates = ['deploy', 'restore', 'finalize'].includes(operation);
    if (mutates) { this.s.nodes[node].uncertain = true; this.persist(this.s); }
    // Never resend send-command after timeout. Missing CommandId remains BLOCKED until operator reconciliation.
    const r = this.aws('ssm', 'send-command', ['--document-name', policy.aws.ssmDocument, '--document-version', policy.aws.ssmVersion,
      '--document-hash', this.s.documentHash, '--document-hash-type', 'Sha256', '--instance-ids', policy.aws.instances[node],
      '--parameters', JSON.stringify(params), '--timeout-seconds', '60', '--comment', `ENVER-${this.s.run}-${this.s.attempt}-${node}-${operation}`]);
    this.s.nodes[node].commandId = r.Command.CommandId;
    if (mutates) this.s.nodes[node].mutationCommandId = r.Command.CommandId;
    this.persist(this.s);
    const result = await this.reconcile(node, r.Command.CommandId);
    requireThat(result.Status === 'Success' && result.ResponseCode === 0, 'REMOTE_OPERATION_FAILED');
    requireThat(!result.StandardErrorContent?.trim(), 'REMOTE_UNEXPECTED_STDERR');
    const receipt = JSON.parse(result.StandardOutputContent.trim());
    requireThat(receipt.schema === 1 && receipt.instance === policy.aws.instances[node] && receipt.operation === operation && receipt.status === 'PASS', 'REMOTE_RECEIPT');
    return receipt;
  }
  async targets(nodes = ['A', 'B']) {
    const until = Date.now() + 180000;
    for (let n = 0; n < 24 && Date.now() < until; n++) {
      const r = this.aws('elbv2', 'describe-target-health', ['--target-group-arn', policy.aws.targetGroup]);
      if (nodes.every(node => r.TargetHealthDescriptions?.some(t => t.Target.Id === policy.aws.instances[node] && t.TargetHealth.State === 'healthy'))) return 'PASS';
      await new Promise(r => setTimeout(r, 5000));
    }
    throw new Closed('TARGET_HEALTH_TIMEOUT');
  }
  async ready(node, manifestDigest) { const r = await this.remote(node, 'verify', manifestDigest); requireThat(r.digest === manifestDigest, 'READINESS_DIGEST'); await this.targets([node]); }
  async restore(node, manifestDigest) { return this.remote(node, 'restore', manifestDigest); }
  async finalHealth() {
    await this.targets();
    const r = await fetch(policy.aws.healthUrl, {redirect: 'error', signal: AbortSignal.timeout(10000)});
    requireThat(r.status === 200 && await r.text() === 'Healthy', 'CLOUDFRONT_HEALTH'); return 'PASS';
  }
}
async function execute(phase) {
  if (phase !== 'summary') validateDispatch();
  if (phase === 'validate') { configured(); return; }
  let s;
  const persist = state => save(path.join(evidence, 'current', 'state.json'), state);
  if (phase === 'resolve') {
    configured(); const {selected, rollbackInventory} = await resolve();
    s = {...identity(), phase, candidate: selected, rollbackInventory, nodes: {A: {}, B: {}}, deploymentOutcome: 'IN PROGRESS', rollbackAttempted: false, rollbackOutcome: 'NOT EXECUTED'};
    persist(s); return;
  }
  try { s = latestState(); }
  catch (e) {
    if (phase !== 'summary') throw e;
    s = {...identity(), candidate: {sha: 'NOT RESOLVED', manifestDigest: 'NOT RESOLVED'}, nodes: {A: {}, B: {}}, deploymentOutcome: 'FAILED', rollbackAttempted: false, rollbackOutcome: 'NOT EXECUTED'};
  }
  s.phase = phase; persist(s);
  if (phase === 'summary') {
    const needs = JSON.parse(process.env.NEEDS_JSON || '{}');
    if (!Object.values(needs).every(n => ['success', 'skipped'].includes(n.result)) || s.deploymentOutcome !== 'SUCCEEDED') s.deploymentOutcome = 'FAILED';
    const text = `# Deployment Summary\n\n| Campo | Resultado |\n|---|---|\n| Environment | development |\n| Actor | ${process.env.GITHUB_ACTOR} |\n| Commit | ${s.candidate.sha} |\n| Manifest digest | ${s.candidate.manifestDigest} |\n| Previous digest | ${s.previousDigest || 'NOT RESOLVED'} |\n| EC2 A | ${s.nodes.A.ready || 'NOT EXECUTED'} |\n| EC2 B | ${s.nodes.B.ready || 'NOT EXECUTED'} |\n| ALB targets | ${s.targets || 'NOT EXECUTED'} |\n| CloudFront health | ${s.health || 'NOT EXECUTED'} |\n| AUTH smoke | ${s.smoke || 'NOT EXECUTED'} |\n| Rollback attempted | ${s.rollbackAttempted} |\n| Rollback result | ${s.rollbackOutcome} |\n| Final deployment | ${s.deploymentOutcome} |\n\nNo se acredita AUTH-027 distribuido. Un rollback exitoso no convierte el deployment original en éxito.\n`;
    if (process.env.GITHUB_STEP_SUMMARY) fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, text);
    persist(s); requireThat(s.deploymentOutcome === 'SUCCEEDED', 'DEPLOYMENT_FAILED'); return;
  }
  configured(); const plane = new AWSPlane(s, persist);
  try {
    if (phase === 'preflight') {
      const who = plane.aws('sts', 'get-caller-identity');
      requireThat(who.Account === policy.aws.account && who.Arn.includes(':assumed-role/enver-github-deploy-dev/'), 'DEPLOY_IDENTITY');
      await verifyApproval(s.candidate); await plane.ecr(s.candidate.manifestDigest); await plane.document();
      const a = await plane.remote('A', 'snapshot'), b = await plane.remote('B', 'snapshot');
      requireThat(a.digest === b.digest && !a.recoveryRequired && !b.recoveryRequired, 'BASELINE_UNRECONCILED', 'BLOCKED');
      s.previousDigest = digest(a.digest);
      const previous = s.rollbackInventory.find(c => c.manifestDigest === s.previousDigest);
      const manual = readJson(path.join(root, 'scripts/deploy/rollback-baseline.json'));
      requireThat(previous || manual.approved === true && manual.manifestDigest === s.previousDigest && manual.environment === policy.environment && manual.repository === policy.repository && /^[a-f0-9]{64}$/.test(manual.evidenceHash || '') && typeof manual.approvedBy === 'string' && Date.parse(manual.expiresAt) > Date.now(), 'PREVIOUS_APPROVED_DIGEST_REQUIRED', 'BLOCKED');
      await plane.ecr(s.previousDigest); await plane.targets(); s.targets = 'PASS';
    } else if (phase === 'deploy_a' || phase === 'deploy_b') {
      const node = phase.endsWith('_a') ? 'A' : 'B';
      requireThat(node !== 'B' || s.nodes.A.ready === 'PASS', 'A_MUST_PASS_BEFORE_B');
      s.nodes[node].attempted = true; persist(s);
      await plane.remote(node, 'deploy'); s.nodes[node].deployed = 'PASS';
    } else if (phase === 'ready_a' || phase === 'ready_b') {
      const node = phase.endsWith('_a') ? 'A' : 'B'; await plane.ready(node, s.candidate.manifestDigest); s.nodes[node].ready = 'PASS';
    } else if (phase === 'post') {
      requireThat(s.nodes.A.ready === 'PASS' && s.nodes.B.ready === 'PASS', 'BOTH_NODES_REQUIRED');
      await plane.finalHealth(); s.targets = 'PASS'; s.health = 'PASS';
      // No unbounded registration and no credential intake in this gate. Required health is never optional.
      s.smoke = process.env.ENVER_SMOKE === 'true' ? 'NOT EXECUTED: REUSABLE_SYNTHETIC_IDENTITY_NOT_CONFIGURED; NONDESTRUCTIVE_HEALTH_ONLY' : 'NOT EXECUTED: DISABLED_BY_INPUT';
      for (const node of ['A', 'B']) await plane.remote(node, 'finalize');
      s.deploymentOutcome = 'SUCCEEDED';
    } else if (phase === 'compensate') {
      const needs = JSON.parse(process.env.NEEDS_JSON || '{}');
      for (const node of ['A', 'B']) {
        const job = 'deploy_' + node.toLowerCase();
        if (needs[job]?.result === 'failure' && !s.nodes[node].attempted) {
          s.nodes[node].attempted = true; s.nodes[node].uncertain = true; s.nodes[node].mutationCommandId = null;
        }
      }
      if (!rollbackOrder(s).length) { s.rollbackOutcome = 'NOT EXECUTED'; }
      else await compensate(s, plane);
      persist(s);
      requireThat(['PASS', 'NOT EXECUTED'].includes(s.rollbackOutcome), 'COMPENSATION_NOT_COMPLETE', 'BLOCKED');
    } else throw new Closed('UNKNOWN_DEPLOY_PHASE');
  } catch (e) {
    s.deploymentOutcome = 'FAILED';
    const code = e instanceof Closed ? e.code : 'DEPLOYMENT_OPERATION_ERROR';
    s.failure ||= code; s.phaseFailure = {phase, code}; persist(s); throw e;
  }
  persist(s);
}
if (process.argv[1] && fileURLToPath(import.meta.url) === path.resolve(process.argv[1])) {
  try { await execute(process.argv[2]); console.log(`${process.argv[2]}: VERIFIED`); }
  catch (e) {
    if (process.argv[2] === 'summary' && process.env.GITHUB_STEP_SUMMARY && !fs.existsSync(path.join(evidence, 'current/state.json'))) fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, '# Deployment Summary\nFAILED / BLOCKED before a verified state. No deployment success is accredited.\n');
    console.log(e instanceof Closed ? `${e.state}: ${e.code}` : 'BLOCKED: DEPLOYMENT_OPERATION_ERROR'); process.exitCode = 1;
  }
}
