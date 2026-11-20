import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import { spawnSync } from 'node:child_process';

export const root = path.resolve(import.meta.dirname, '../..');
export const policy = JSON.parse(fs.readFileSync(path.join(root, 'scripts/ci/policy.json')));
// Bind the parsed policy, not checkout line endings (Linux and Windows share the gate).
export const policyHash = sha256(JSON.stringify(policy));
export const evidence = process.env.ENVER_EVIDENCE_DIR || path.join(process.env.RUNNER_TEMP || os.tmpdir(), 'enver-evidence');
export const privateDir = path.join(process.env.RUNNER_TEMP || os.tmpdir(), 'enver-ci-private');
export const states = ['PASS', 'FAIL', 'NOT APPLICABLE', 'NOT EXECUTED', 'BLOCKED'];
export class Closed extends Error {
  constructor(code, state = 'FAIL') { super(code); this.code = code; this.state = state; }
}
export function requireThat(ok, code, state = 'FAIL') { if (!ok) throw new Closed(code, state); }
export function sha256(bytes) { return crypto.createHash('sha256').update(bytes).digest('hex'); }
export function readJson(file) {
  requireThat(fs.statSync(file).size < 10 * 1024 * 1024, 'JSON_SIZE');
  return JSON.parse(fs.readFileSync(file, 'utf8'));
}
export function save(file, value) {
  fs.mkdirSync(path.dirname(file), {recursive: true, mode: 0o700});
  fs.writeFileSync(file, JSON.stringify(value, null, 2) + '\n', {mode: 0o600});
}
export function command(executable, args, options = {}) {
  const r = spawnSync(executable, args, {
    cwd: root, encoding: 'utf8', maxBuffer: 64 * 1024 * 1024, timeout: 30 * 60 * 1000,
    env: {...process.env, DOTNET_CLI_TELEMETRY_OPTOUT: '1', AWS_EC2_METADATA_DISABLED: 'true'}, ...options,
  });
  if (!options.allowFailure) {
    const tool = path.basename(executable).replace(/[^a-z0-9]/gi, '_');
    requireThat(!r.error, (r.error?.code === 'ETIMEDOUT' ? 'COMMAND_TIMEOUT_' : 'COMMAND_UNAVAILABLE_') + tool, 'BLOCKED');
    if (r.status !== 0) {
      const failure = new Closed('COMMAND_FAILED_' + tool);
      const operation = args.find(a => ['restore', 'build', 'test', 'run', 'init', 'fmt', 'validate'].includes(a));
      failure.details = {tool, operation: operation || 'OTHER_FIXED_OPERATION', exitCode: r.status};
      throw failure;
    }
  }
  return r;
}
export function identity() {
  return {repository: process.env.GITHUB_REPOSITORY || policy.repository,
    sha: process.env.ENVER_SOURCE_SHA || process.env.GITHUB_SHA || command('git', ['rev-parse', 'HEAD']).stdout.trim(),
    run: String(process.env.GITHUB_RUN_ID || 'local'), attempt: String(process.env.GITHUB_RUN_ATTEMPT || '1')};
}
export function output(name, value) {
  requireThat(/^[a-z_]+$/.test(name) && !String(value).includes('\n'), 'OUTPUT_FORMAT');
  if (process.env.GITHUB_OUTPUT) fs.appendFileSync(process.env.GITHUB_OUTPUT, `${name}=${value}\n`);
}
export function emit(control, state, reason, details = {}) {
  requireThat(/^[a-z_]+$/.test(control) && states.includes(state), 'RESULT_SCHEMA');
  const result = {schema: 1, ...identity(), policy: policy.version, policyHash, control, state, reason, details};
  save(path.join(evidence, `${control}.json`), result);
  console.log(`${control}: ${state} (${reason})`); // No tool stdout, exceptions, request bodies or findings.
  return result;
}
export function digest(value) { requireThat(/^sha256:[a-f0-9]{64}$/.test(value), 'MANIFEST_DIGEST'); return value; }
export function exactSDK() {
  requireThat(readJson(path.join(root, 'global.json')).sdk.version === policy.sdk, 'SDK_POLICY_MISMATCH');
  requireThat(command('dotnet', ['--version']).stdout.trim() === policy.sdk, 'EXACT_SDK_UNAVAILABLE', 'BLOCKED');
}
export function classify(paths, uncertain = false, event = 'pull_request', branch = '') {
  const scope = {api: false, tests: false, dependencies: false, docker: false, infra: false, runtime: false, workflows: false, documentation: false, uncertain};
  for (const file of paths) {
    if (/\.md$/i.test(file)) { scope.documentation = true; continue; }
    if (/^(?:\.github\/|scripts\/|\.config\/)|(?:global\.json|\.slnx|Directory\.[^/]+|NuGet\.[^/]+)$/.test(file)) scope.workflows = true;
    else if (/^(?:Enver\.API\/)/.test(file)) scope.api = true;
    else if (/^tests\//.test(file)) scope.tests = true;
    else if (/^tools\//.test(file)) scope.runtime = true;
    else if (/^infra\//.test(file)) scope.infra = true;
    else scope.workflows = true; // Unknown source is not documentation.
    if (/(?:\.csproj|packages\.lock\.json|NuGet\.Config|global\.json)$/.test(file)) scope.dependencies = true;
    if (/(?:Dockerfile|\.dockerignore)$/.test(file)) scope.docker = true;
  }
  const all = uncertain || scope.workflows || (event === 'push' && branch === policy.branch && Object.entries(scope).some(([k, v]) => v && k !== 'documentation'));
  const source = all || scope.api || scope.tests || scope.dependencies || scope.docker;
  return {...scope, build_test: source, integration_validation: source, runtime_tool_tests: all || source || scope.runtime,
    semgrep: all || source || scope.runtime, dependency_review: all || source || scope.runtime,
    secret_scan: true, container_lint: all || source, iac_validation: all || scope.infra,
    contract_tests: source, docker_build: source};
}
function attrs(text) {
  return Object.fromEntries([...text.matchAll(/([\w:]+)="([^"]*)"/g)].map(m => [m[1], m[2].replace(/&quot;/g, '"').replace(/&amp;/g, '&').replace(/&lt;/g, '<').replace(/&gt;/g, '>')]));
}
export function parseTrx(xml) {
  requireThat(!/<!DOCTYPE|<!ENTITY/i.test(xml), 'TRX_ENTITY');
  const counterTags = [...xml.matchAll(/<(?:\w+:)?Counters\s+([^>]+)\/?\s*>/g)];
  requireThat(counterTags.length === 1 && /<\/(?:\w+:)?TestRun>\s*$/.test(xml), 'TRX_INCOMPLETE');
  const counters = attrs(counterTags[0][1]);
  const results = [...xml.matchAll(/<(?:\w+:)?UnitTestResult\s+([^>]+)>/g)].map(m => attrs(m[1]));
  const total = Number(counters.total), passed = Number(counters.passed);
  requireThat(total > 0 && passed === total && Number(counters.executed) === total && results.length === total, 'TRX_EMPTY_OR_MISSING');
  for (const key of ['failed', 'notExecuted', 'aborted', 'error', 'timeout', 'disconnected', 'warning', 'inconclusive', 'notRunnable']) requireThat(Number(counters[key] || 0) === 0, 'TRX_NONPASS');
  requireThat(results.every(r => r.outcome === 'Passed'), 'TRX_FAILED_OR_SKIPPED');
  return {total, passed, failed: 0, skipped: 0, tests: results.map(r => r.testName.split('(')[0])};
}
export function aggregate(expected, results, needs, scope, id = identity()) {
  const rows = [];
  for (const control of expected) {
    const r = results.find(r => r.control === control);
    requireThat(r && r.repository === id.repository && r.sha === id.sha && r.run === id.run && r.attempt === id.attempt && r.policyHash === policyHash, 'MISSING_OR_FOREIGN_' + control);
    requireThat(needs[control]?.result === 'success', 'UNEXPECTED_JOB_STATE_' + control);
    requireThat(r.state === 'PASS' || (r.state === 'NOT APPLICABLE' && scope[control] === false && r.reason === 'PATH_SCOPE'), 'CONTROL_NOT_ACCEPTED_' + control);
    rows.push({control, state: r.state, hash: sha256(JSON.stringify(r))});
  }
  return rows;
}
export function filesBelow(dir) {
  if (!fs.existsSync(dir)) return [];
  return fs.readdirSync(dir, {withFileTypes: true}).flatMap(e => e.isDirectory() ? filesBelow(path.join(dir, e.name)) : [path.join(dir, e.name)]);
}
export function container(tool, args, {network = 'none', writable = [], input, timeout, extraEnv = {}, repository = root} = {}) {
  fs.mkdirSync(privateDir, {recursive: true, mode: 0o700});
  const mounts = ['--mount', `type=bind,src=${repository},dst=/repo,readonly`, '--mount', `type=bind,src=${privateDir},dst=/private`];
  for (const [host, target] of writable) mounts.push('--mount', `type=bind,src=${host},dst=${target}`);
  return command('docker', ['run', '--rm', '--platform', 'linux/amd64', '--network', network,
    '--cap-drop', 'ALL', '--security-opt', 'no-new-privileges', '--workdir', '/repo', ...mounts,
    ...(typeof process.getuid === 'function' ? ['--user', `${process.getuid()}:${process.getgid()}`] : []),
    '--env', 'HOME=/tmp', '--env', 'TRIVY_CACHE_DIR=/tmp/trivy-cache',
    ...Object.entries(extraEnv).flatMap(([k, v]) => ['--env', `${k}=${v}`]),
    ...(input !== undefined ? ['-i'] : []), policy.images[tool], ...args], {input, ...(timeout ? {timeout} : {}), allowFailure: true});
}
export class GitHub {
  constructor(token = process.env.GH_TOKEN) { this.token = token; }
  async request(endpoint, {binary = false} = {}) {
    requireThat(endpoint.startsWith('/') && !/(?:^|\/)\.\.(?:\/|$)/.test(endpoint), 'GITHUB_ENDPOINT');
    const r = await fetch('https://api.github.com' + endpoint, {headers: {
      Authorization: `Bearer ${this.token}`, Accept: 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28',
    }, signal: AbortSignal.timeout(30000)});
    requireThat(r.ok, 'GITHUB_API_' + r.status, 'BLOCKED');
    return binary ? Buffer.from(await r.arrayBuffer()) : r.json();
  }
  async pages(endpoint, key) {
    const items = [];
    for (let page = 1; page <= 10; page++) {
      const response = await this.request(`${endpoint}${endpoint.includes('?') ? '&' : '?'}per_page=100&page=${page}`);
      const list = key ? response[key] : response;
      requireThat(Array.isArray(list), 'GITHUB_PAGINATION'); items.push(...list);
      if (list.length < 100) return items;
    }
    throw new Closed('GITHUB_PAGE_LIMIT', 'BLOCKED');
  }
  async artifact(run, name) {
    const matches = (await this.pages(`/repos/${policy.repository}/actions/runs/${run}/artifacts`, 'artifacts')).filter(a => a.name === name);
    requireThat(matches.length === 1 && !matches[0].expired, 'ARTIFACT_MISSING_OR_AMBIGUOUS', 'BLOCKED');
    return matches[0];
  }
  async download(artifact, maxBytes = 2 * 1024 * 1024) {
    requireThat(artifact.size_in_bytes <= maxBytes && artifact.digest?.startsWith('sha256:'), 'ARTIFACT_SIZE_OR_HASH');
    const bytes = await this.request(`/repos/${policy.repository}/actions/artifacts/${artifact.id}/zip`, {binary: true});
    requireThat(bytes.length <= maxBytes && 'sha256:' + sha256(bytes) === artifact.digest, 'ARTIFACT_HASH'); return bytes;
  }
}
export function validateRun(run, sha, workflow) {
  requireThat(run.repository?.full_name === policy.repository && run.head_repository?.full_name === policy.repository && run.head_branch === policy.branch && run.head_sha === sha && run.path === `.github/workflows/${workflow}.yml`, 'UNTRUSTED_RUN');
  requireThat(['push', 'workflow_dispatch'].includes(run.event), 'UNTRUSTED_EVENT');
}
