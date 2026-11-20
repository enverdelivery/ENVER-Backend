import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {Closed, requireThat, command, root, policy, policyHash, privateDir, evidence, readJson, save, sha256, identity, output, emit, exactSDK, classify, parseTrx, aggregate, filesBelow, container, GitHub} from './lib.mjs';

export const controls = ['build_test', 'integration_validation', 'runtime_tool_tests', 'semgrep', 'dependency_review', 'secret_scan', 'container_lint', 'iac_validation', 'contract_tests', 'docker_build'];
export const projects = {
  unit: 'tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj',
  integration: 'tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj',
  security: 'tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj',
};
function scope() { return JSON.parse(process.env.ENVER_SCOPE || '{}'); }
function projectArgs(project) {
  return project.replaceAll('\\', '/').startsWith('tools/')
    ? ['-p:CustomAfterMicrosoftCommonTargets=' + path.join(root, 'scripts/ci/RuntimeToolRestore.targets')] : [];
}
function restore(project) {
  const locked = project.endsWith('.slnx') || fs.existsSync(path.join(root, path.dirname(project), 'packages.lock.json'));
  command('dotnet', ['restore', project, ...(locked ? ['--locked-mode'] : []), ...projectArgs(project), '--verbosity', 'quiet']);
}
function build(project) { command('dotnet', ['build', project, '-c', 'Release', '--no-restore', ...projectArgs(project), '--verbosity', 'quiet']); }
function suite(name) {
  const dir = path.join(privateDir, name); fs.mkdirSync(dir, {recursive: true});
  const file = path.join(dir, name + '.trx');
  requireThat(!fs.existsSync(file), 'NO_AUTOMATIC_SUITE_RERUN_' + name.toUpperCase());
  const r = command('dotnet', ['test', projects[name], '-c', 'Release', '--no-build', '--no-restore', '--logger', `trx;LogFileName=${name}.trx`, '--results-directory', dir], {allowFailure: true});
  if (!fs.existsSync(file)) return {state: 'BLOCKED', reason: r.error?.code === 'ETIMEDOUT' ? 'TEST_TIMEOUT' : 'MISSING_NATIVE_RESULT'};
  const xml = fs.readFileSync(file, 'utf8');
  try {
    const counts = parseTrx(xml);
    requireThat(r.status === 0, 'TEST_PROCESS_NONZERO'); return {state: 'PASS', ...counts};
  } catch {
    const counters = xml.match(/<Counters\s+([^>]+)>/)?.[1] || '';
    const number = name => Number(counters.match(new RegExp('\\b' + name + '="(\\d+)"'))?.[1] || 0);
    return {state: 'FAIL', reason: 'FAILED_SKIPPED_OR_INCOMPLETE_SUITE', total: number('total'), passed: number('passed'), failed: number('failed'), skipped: number('notExecuted')};
  }
}
function suitesMustPass(details, names) {
  if (!names.every(n => details[n].state === 'PASS')) {
    const error = new Closed('REQUIRED_SUITE_NOT_PASS'); error.details = details; throw error;
  }
  return details;
}
async function detect() {
  const event = process.env.GITHUB_EVENT_PATH ? readJson(process.env.GITHUB_EVENT_PATH) : {};
  const base = event.pull_request?.base?.sha || event.before;
  const head = process.env.GITHUB_SHA;
  let paths = [], uncertain = true;
  if (/^[a-f0-9]{40}$/.test(base || '') && !/^0+$/.test(base) && /^[a-f0-9]{40}$/.test(head || '')) {
    const r = command('git', ['diff', '--name-only', '-z', base, head, '--'], {allowFailure: true});
    if (r.status === 0) { paths = r.stdout.split('\0').filter(Boolean); uncertain = false; }
  }
  const value = classify(paths, uncertain, process.env.GITHUB_EVENT_NAME, process.env.GITHUB_REF_NAME);
  output('scope', JSON.stringify(value));
  save(path.join(evidence, 'scope.json'), {...identity(), scope: value});
  return {scope: value, paths: paths.length, fallback: uncertain};
}
async function buildTest() {
  exactSDK(); restore('Enver.Backend.slnx'); build('Enver.Backend.slnx');
  return suitesMustPass({build: 'PASS', configuration: 'Release', sdk: policy.sdk, unit: suite('unit')}, ['unit']);
}
async function integration() {
  exactSDK(); command('docker', ['info', '--format', '{{.ServerVersion}}']);
  // Separate runner: restore/build this exact SHA explicitly; no reliance on another runner's cache.
  restore('Enver.Backend.slnx'); build('Enver.Backend.slnx');
  const details = {integration: suite('integration'), security: suite('security'), dependencies: 'EXISTING_TESTCONTAINERS_FIXTURES_ONLY'};
  return suitesMustPass(details, ['integration', 'security']);
}
async function runtime() {
  exactSDK(); const list = policy.runtimeProjects[process.platform];
  requireThat(list?.length, 'RUNTIME_PLATFORM', 'BLOCKED');
  const results = [];
  for (const project of list) {
    const file = `tools/${project}/${project}.csproj`;
    restore(file); build(file);
    const r = command('dotnet', ['run', '--project', file, '-c', 'Release', '--no-build', '--no-restore'], {allowFailure: true});
    const json = r.stdout.split(/\r?\n/).flatMap(line => { try { return [JSON.parse(line)]; } catch { return []; } });
    const native = json.findLast(x => Number.isInteger(x.total));
    const markers = (r.stdout.match(/^PASS /gm) || []).length;
    const assertions = [...r.stdout.matchAll(/(\d+)\/(\d+) (?:assertions )?PASS/g)];
    if (r.status !== 0 || /^FAIL /m.test(r.stdout)) {
      const error = new Closed('RUNTIME_EXECUTABLE_FAILED');
      error.details = {platform: process.platform, failedProject: project, previousSuites: results,
        failedCases: r.stdout.split(/\r?\n/).filter(l => /^FAIL [a-zA-Z0-9 _-]+$/.test(l)).map(l => l.slice(5)),
        historicalPrecheck: project === 'Enver.QuotaControl.Orchestrator.Tests' ? 'SEPARATE_EXPLICIT_WORKSTATION_MODE_NOT_PART_OF_PORTABLE_SUITE' : undefined};
      throw error;
    }
    if (native) requireThat(native.total > 0 && native.passed === native.total && native.failed === 0, 'RUNTIME_INCOMPLETE');
    else requireThat((markers > 0 || assertions.length > 0) && assertions.every(m => Number(m[1]) > 0 && m[1] === m[2]), 'RUNTIME_NO_RESULTS');
    results.push({project, state: 'PASS', count: native?.total || markers || Number(assertions[0][1]),
      ...(project === 'Enver.QuotaControl.Orchestrator.Tests' ? {historicalArtifactValidation: 'NOT EXECUTED; SEPARATE_WORKSTATION_EVIDENCE'} : {})});
  }
  const tests = filesBelow(path.join(root, 'scripts/tests')).filter(f => f.endsWith('.test.mjs'));
  requireThat(tests.length > 0, 'PIPELINE_TESTS_MISSING'); command('node', ['--test', ...tests]);
  if (process.platform === 'linux') command('python3', ['-B', '-m', 'unittest', 'discover', '-s', 'scripts/tests', '-p', '*_test.py']);
  return {platform: process.platform, suites: results, pipelineTests: 'PASS', realAWS: false};
}
async function semgrep() {
  const report = path.join(privateDir, 'semgrep.json');
  const r = container('semgrep', ['semgrep', 'scan', '--config', '/repo/scripts/ci/semgrep.yml', '--metrics=off', '--disable-version-check', '--exclude', '**/bin/**', '--exclude', '**/obj/**', '--include', '*.cs', '--json', '--output', '/private/semgrep.json', '/repo/Enver.API', '/repo/tools']);
  requireThat(r.status === 0 && fs.existsSync(report), 'SEMGREP_NOT_EXECUTED', 'BLOCKED');
  const data = readJson(report);
  requireThat(data.paths?.scanned?.length > 0 && Array.isArray(data.results) && !data.errors?.length, 'SEMGREP_INCOMPLETE');
  requireThat(data.results.length === 0, 'SEMGREP_FINDINGS'); return {scanned: data.paths.scanned.length, findings: 0, mode: 'C_SHARP_TEXT_SECURITY_PATTERNS', limitation: 'NO_AST_OR_INTERPROCEDURAL_COVERAGE; CODEQL_REQUIRED'};
}
async function dependencies() {
  exactSDK(); restore('Enver.Backend.slnx');
  const toolProjects = filesBelow(path.join(root, 'tools')).filter(p => p.endsWith('.csproj') && !/[\\/](?:obj|bin)[\\/]/.test(p));
  for (const p of toolProjects) restore(path.relative(root, p));
  const audits = [];
  for (const p of ['Enver.Backend.slnx', ...toolProjects.map(p => path.relative(root, p))]) {
    const r = command('dotnet', ['list', p, 'package', '--vulnerable', '--include-transitive', '--format', 'json', '--no-restore']);
    const data = JSON.parse(r.stdout); requireThat(data.projects?.length > 0, 'NUGET_AUDIT_INCOMPLETE');
    requireThat(!data.problems?.length && !data.errors?.length && !(data.logs || []).some(l => /warning|error/i.test(l.level || '')), 'NUGET_AUDIT_FEED_OR_REPORT_ERROR', 'BLOCKED');
    const vulns = data.projects.flatMap(p => (p.frameworks || []).flatMap(f => [...(f.topLevelPackages || []), ...(f.transitivePackages || [])].flatMap(p => p.vulnerabilities || [])));
    requireThat(!vulns.some(v => policy.vulnerabilities.block.includes(v.severity?.toUpperCase())), 'NUGET_HIGH_CRITICAL');
    audits.push({project: p, findings: vulns.length});
  }
  let review = {state: 'NOT APPLICABLE', reason: 'PR_COMPARISON_ONLY'};
  if (process.env.GITHUB_EVENT_NAME === 'pull_request') {
    const event = readJson(process.env.GITHUB_EVENT_PATH), base = event.pull_request?.base?.sha, head = event.pull_request?.head?.sha;
    requireThat(/^[a-f0-9]{40}$/.test(base || '') && /^[a-f0-9]{40}$/.test(head || ''), 'DEPENDENCY_REVIEW_REFS');
    const changes = await new GitHub().request(`/repos/${policy.repository}/dependency-graph/compare/${base}...${head}`);
    requireThat(Array.isArray(changes), 'DEPENDENCY_REVIEW_RESPONSE');
    const blocking = changes.filter(c => c.change_type === 'added').flatMap(c => c.vulnerabilities || []).filter(v => policy.vulnerabilities.block.includes(v.severity?.toUpperCase()));
    requireThat(blocking.length === 0, 'DEPENDENCY_REVIEW_FINDINGS'); review = {state: 'PASS', changes: changes.length};
  }
  return {nugetAudit: {state: 'PASS', audits}, dependencyReview: review};
}
export async function secrets() {
  const r = container('gitleaks', ['git', '/repo', '--config', '/repo/.gitleaks.toml', '--gitleaks-ignore-path', '/repo/.gitleaksignore', '--redact=100', '--no-banner', '--log-opts=--all', '--report-format', 'json', '--report-path', '/private/gitleaks.json'], {
    extraEnv: {GIT_CONFIG_COUNT: '1', GIT_CONFIG_KEY_0: 'safe.directory', GIT_CONFIG_VALUE_0: '/repo'},
  });
  requireThat(r.status === 0 || r.status === 1, 'GITLEAKS_EXECUTION', 'BLOCKED');
  requireThat(fs.existsSync(path.join(privateDir, 'gitleaks.json')), 'GITLEAKS_REPORT', 'BLOCKED');
  const findings = readJson(path.join(privateDir, 'gitleaks.json'));
  requireThat(Array.isArray(findings), 'GITLEAKS_REPORT_SCHEMA');
  requireThat(r.status === 0 && findings.length === 0, 'SECRET_FINDING');
  return {findings: 0, history: 'ALL_FETCHED_REFS', redacted: true};
}
async function lint() {
  const r = container('hadolint', ['hadolint', '--format', 'json', '/repo/Enver.API/Dockerfile']);
  requireThat(r.status === 0 || r.status === 1, 'HADOLINT_EXECUTION', 'BLOCKED');
  const data = JSON.parse(r.stdout); requireThat(Array.isArray(data), 'HADOLINT_SCHEMA');
  requireThat(r.status === 0 && !data.some(f => ['error', 'warning'].includes(f.level)), 'HADOLINT_FINDINGS');
  return {findings: data.length};
}
export function assessTrivy(data, exceptions = [], now = Date.now()) {
  requireThat(Array.isArray(data.Results), 'TRIVY_INCOMPLETE');
  const configurations = data.Results.flatMap(r => r.Misconfigurations || []);
  requireThat(configurations.every(c => c.Status === undefined || ['FAIL', 'PASS', 'EXCEPTION'].includes(c.Status)), 'TRIVY_UNKNOWN_MISCONFIG_STATE');
  const ignored = configurations.filter(c => c.Status === 'EXCEPTION');
  for (const finding of ignored) requireThat(exceptions.some(e => e.rule === finding.AVDID && e.resource === finding.CauseMetadata?.Resource &&
    now < Date.parse(e.expires + 'T00:00:00Z')), 'TRIVY_UNAPPROVED_OR_EXPIRED_EXCEPTION');
  const issues = [...data.Results.flatMap(r => r.Vulnerabilities || []), ...configurations.filter(c => c.Status === undefined || c.Status === 'FAIL')];
  requireThat(!issues.some(v => policy.vulnerabilities.block.includes(v.Severity)), 'TRIVY_HIGH_CRITICAL');
  return {findings: issues.length, highCritical: 0, observedExceptions: ignored.length, ignoreUnfixed: false};
}
export function iacSourcePaths(paths) {
  return [...new Set(paths)].filter(p => p.startsWith('infra/') && !p.split('/').some(s => s === '..' || s === '.terraform') &&
    (/\.tf(?:\.json)?$/.test(p) || p === 'infra/.terraform.lock.hcl' || /^infra\/bootstrap\/[\w./-]+\.(?:json|sh)$/.test(p)) &&
    !/\.tfstate|\.tfvars|\.tfplan|(?:^|\/)tfplan/i.test(p));
}
async function iac() {
  requireThat(policy.iacExceptions.every(e => Date.now() < Date.parse(e.expires + 'T00:00:00Z')), 'IAC_EXCEPTION_EXPIRED', 'BLOCKED');
  // Only publicable source enters validation/scanning: never scan workstation plans,
  // tfstate, tfvars or cached .terraform (which can describe the old infrastructure).
  const source = path.join(privateDir, 'iac-source');
  requireThat(!fs.existsSync(source), 'IAC_FRESH_SOURCE_REQUIRED');
  const paths = iacSourcePaths(command('git', ['ls-files', '--cached', '--others', '--exclude-standard', '-z', '--', 'infra']).stdout.split('\0').filter(Boolean));
  requireThat(paths.includes('infra/security.tf') && paths.includes('infra/.terraform.lock.hcl'), 'IAC_SOURCE_INCOMPLETE');
  for (const file of paths) {
    requireThat(!fs.lstatSync(path.join(root, file)).isSymbolicLink(), 'IAC_SYMLINK');
    const dest = path.join(source, file.slice('infra/'.length)); fs.mkdirSync(path.dirname(dest), {recursive: true});
    fs.copyFileSync(path.join(root, file), dest);
  }
  command('terraform', [`-chdir=${source}`, 'fmt', '-check', '-recursive']);
  command('terraform', [`-chdir=${source}`, 'init', '-backend=false', '-input=false', '-lockfile=readonly']);
  command('terraform', [`-chdir=${source}`, 'validate', '-no-color']);
  const r = container('trivy', ['config', '--misconfig-scanners', 'terraform', '--skip-dirs', '/repo/.terraform', '--include-non-failures', '--format', 'json', '--output', '/private/iac.json', '/repo'], {network: 'bridge', repository: source});
  requireThat(r.status === 0, 'TRIVY_IAC_EXECUTION', 'BLOCKED');
  return {terraform: 'PASS', sourceFiles: paths.length, localStatePlansVariables: 'EXCLUDED', ...assessTrivy(readJson(path.join(privateDir, 'iac.json')), policy.iacExceptions),
    exceptions: policy.iacExceptions};
}
export const contractMethods = [
  'Confirmed_201_has_exact_projection_real_seven_field_document_and_real_hash_without_auto_login',
  'Anonymous_Register_Login_skip_optional_Bearer_and_charge_once',
  'Logout_requires_Bearer_revokes_and_only_exact_Logout_can_reconfirm',
  'Fallback_rejects_missing_or_invalid_identity_with_exact_safe_challenge',
  'Early_boundary_rejects_without_JWT_or_quota',
  'Health_has_exact_plaintext_contract',
];
export function verifyContract(result) {
  requireThat(result.state === 'PASS' && result.details.integration?.total > 0 && result.details.security?.total > 0, 'CONTRACT_SUITE_ABSENT');
  const executed = result.details.integration.tests;
  for (const method of contractMethods) requireThat(executed.some(t => t.endsWith('.' + method) || t === method), 'CONTRACT_ASSERTION_MISSING_' + method);
  return {methods: contractMethods, integration: result.details.integration.total, security: result.details.security.total};
}
async function contract() {
  const candidates = filesBelow(evidence).filter(p => path.basename(p) === 'integration_validation.json');
  requireThat(candidates.length === 1, 'CONTRACT_RESULTS_MISSING');
  const result = readJson(candidates[0]), id = identity();
  requireThat(['repository', 'sha', 'run', 'attempt'].every(k => result[k] === id[k]), 'CONTRACT_FOREIGN_RESULTS');
  return verifyContract(result);
}
export function inspectOCI(archive) {
  const entries = command('tar', ['-tf', archive]).stdout.split(/\r?\n/).filter(Boolean);
  requireThat(entries.length > 2 && entries.every(n => /^(?:\.\/)?(?:index\.json|oci-layout|blobs\/?|blobs\/sha256\/?|blobs\/sha256\/[a-f0-9]{64})\/?$/.test(n)), 'OCI_PATHS');
  function blob(name) { return command('tar', ['-xOf', archive, name], {encoding: null, maxBuffer: 512 * 1024 * 1024}).stdout; }
  const index = JSON.parse(blob('index.json'));
  requireThat(index.schemaVersion === 2 && index.manifests?.length === 1, 'OCI_SINGLE_MANIFEST');
  const manifestDigest = index.manifests[0].digest;
  requireThat(/^sha256:[a-f0-9]{64}$/.test(manifestDigest), 'OCI_DIGEST');
  const raw = blob('blobs/sha256/' + manifestDigest.slice(7)); requireThat('sha256:' + sha256(raw) === manifestDigest, 'OCI_MANIFEST_HASH');
  const manifest = JSON.parse(raw);
  requireThat(manifest.mediaType === 'application/vnd.oci.image.manifest.v1+json' && /^sha256:[a-f0-9]{64}$/.test(manifest.config.digest), 'OCI_MANIFEST_TYPE');
  const configBytes = blob('blobs/sha256/' + manifest.config.digest.slice(7)); requireThat('sha256:' + sha256(configBytes) === manifest.config.digest, 'OCI_CONFIG_HASH');
  const config = JSON.parse(configBytes);
  requireThat(config.os === 'linux' && config.architecture === 'amd64' && config.config?.User === '1654:1654', 'OCI_PLATFORM_OR_ROOT');
  for (const layer of manifest.layers) {
    requireThat(/^sha256:[a-f0-9]{64}$/.test(layer.digest), 'OCI_LAYER_DIGEST');
    requireThat('sha256:' + sha256(blob('blobs/sha256/' + layer.digest.slice(7))) === layer.digest, 'OCI_LAYER_HASH');
  }
  return {manifestDigest, artifactHash: sha256(fs.readFileSync(archive)), platform: 'linux/amd64'};
}
async function dockerBuild() {
  const needs = JSON.parse(process.env.NEEDS_JSON || '{}');
  for (const [key, value] of Object.entries(needs)) requireThat(key === 'detect_changes' || value.result === 'success', 'PREBUILD_GATE_' + key);
  const dir = path.join(evidence, 'image'); fs.mkdirSync(dir, {recursive: true});
  const archive = path.join(dir, 'image.tar');
  command('docker', ['buildx', 'build', '--platform', 'linux/amd64', '--provenance=false', '--sbom=false', '--file', 'Enver.API/Dockerfile', '--tag', 'sha-' + identity().sha, '--output', `type=oci,dest=${archive}`, '.']);
  const oci = inspectOCI(archive), record = {...identity(), ...oci, policy: policy.version, policyHash, artifactName: `oci-${identity().run}-${identity().attempt}`};
  save(path.join(dir, 'candidate.json'), record);
  output('digest', oci.manifestDigest); return record;
}
async function gate() {
  const id = identity(), s = scope(), needs = JSON.parse(process.env.NEEDS_JSON || '{}');
  const results = filesBelow(evidence).filter(p => p.endsWith('.json') && !p.includes(path.sep + 'image' + path.sep)).map(readJson).filter(r => r.control);
  requireThat(results.every(r => ['repository', 'sha', 'run', 'attempt'].every(k => r[k] === id[k]) && r.policyHash === policyHash), 'FOREIGN_CI_RESULT');
  for (const control of controls) requireThat(results.filter(r => r.control === control).length === (control === 'runtime_tool_tests' ? 2 : 1), 'MISSING_OR_DUPLICATE_CI_RESULT_' + control);
  // Matrix results retain independent platform identities; neither runner may silently disappear.
  if (s.runtime_tool_tests) {
    const runtime = results.filter(r => r.control === 'runtime_tool_tests');
    requireThat(runtime.length === 2 && ['linux', 'win32'].every(p => runtime.some(r => r.details.platform === p)) && runtime.every(r => r.state === 'PASS'), 'RUNTIME_MATRIX_INCOMPLETE');
  }
  const rows = aggregate(controls, results, needs, s, id);
  requireThat(needs.detect_changes?.result === 'success', 'DETECT_CHANGES_FAILED');
  save(path.join(evidence, 'ci-report.json'), {...id, policy: policy.version, policyHash, scope: s, results: results.filter(r => controls.includes(r.control)), controls: rows, approved: true});
  if (process.env.GITHUB_STEP_SUMMARY) fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, '# PR Quality Gate\n' + rows.map(r => `- ${r.control}: ${r.state}`).join('\n') + '\n');
  return {controls: rows};
}
const handlers = {detect_changes: detect, build_test: buildTest, integration_validation: integration, runtime_tool_tests: runtime, semgrep, dependency_review: dependencies, secret_scan: secrets, container_lint: lint, iac_validation: iac, contract_tests: contract, docker_build: dockerBuild, pr_quality_gate: gate};
export async function main(control) {
  requireThat(Object.hasOwn(handlers, control), 'UNKNOWN_CONTROL');
  try {
    if (controls.includes(control) && scope()[control] === false) { emit(control, 'NOT APPLICABLE', 'PATH_SCOPE'); return; }
    emit(control, 'PASS', 'VERIFIED', await handlers[control]());
  } catch (e) {
    emit(control, e instanceof Closed ? e.state : 'BLOCKED', e instanceof Closed ? e.code : 'UNEXPECTED_TOOL_OR_RESULT_ERROR', e.details || {});
    process.exitCode = 1;
  }
}
if (process.argv[1] && fileURLToPath(import.meta.url) === path.resolve(process.argv[1])) await main(process.argv[2]);
