// Evidencia derivada únicamente: sin red/procesos, snippets, SARIF crudo ni cambios de política.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {redact, safeText} from './diagnostics.mjs';

export const scannerLimits = {inputBytes: 10 * 1024 * 1024, results: 100, artifactBytes: 256 * 1024};
export const trivyImageArguments = ['image', '--input', '/image/image.tar', '--scanners', 'vuln', '--format', 'json', '--output', '/private/image-scan.json'];
function text(value, env, characters = 512, lines = 4) {
  // Extensión local del saneamiento compartido para asignaciones propias de scanners.
  const assignments = redact(value, env).replace(/\b(?:registry[_-]?auth|api[_-]?key|client[_-]?secret|credentials?|password|passwd|token|secret)["']?\s*[:=]\s*(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|[^\r\n]*)/gi, '[REDACTED_SCANNER_ASSIGNMENT]');
  return safeText(assignments, {env, characters, lines});
}

export function trivyDiagnostic(result, {reportPath, targetDigest, toolImage, declaredVersion, env = process.env}) {
  const diagnostic = {schema: 1, executable: 'docker', tool: 'trivy', toolImage,
    toolVersion: null, declaredToolVersion: declaredVersion, toolVersionSource: 'NOT_OBSERVED',
    targetType: 'OCI_ARCHIVE', targetPath: '/image/image.tar', targetDigest,
    command: ['docker', 'run', '--rm', '--platform', 'linux/amd64', '--network', 'bridge',
      '--cap-drop', 'ALL', '--security-opt', 'no-new-privileges', '--workdir', '/repo',
      '<repository-readonly-mount>', '<private-output-mount>', '<image-mount>', '<runner-uid-gid>',
      '--env', 'HOME=/tmp', '--env', 'TRIVY_CACHE_DIR=/tmp/trivy-cache', toolImage, ...trivyImageArguments],
    exitCode: Number.isInteger(result.status) ? result.status : null, exitCodeSource: 'DOCKER_RUN',
    nativeTrivyExitCode: Number.isInteger(result.status) && result.status >= 0 && result.status < 125 ? result.status : null,
    nativeExitCodeCaveat: 'DOCKER_RUN_PROPAGATES_CONTAINER_EXIT; 125/126/127_OR_SIGNAL_MAY_PRECEDE_TRIVY',
    processError: text(result.error?.code, env, 128, 1) || null,
    signal: text(result.signal, env, 128, 1) || null,
    stage: result.status === 0 ? 'TRIVY_PROCESS_COMPLETED' : 'TRIVY_EXECUTION_FAILURE',
    policyEvaluation: 'NOT_REACHED', reportGenerated: false, reportBytes: null, reportHash: null,
    reportState: 'MISSING', highCount: null, criticalCount: null,
    stdoutExcerpt: text(result.stdout, env, 2048, 20), stderrExcerpt: text(result.stderr, env, 2048, 20)};
  try {
    const stat = fs.statSync(reportPath);
    diagnostic.reportGenerated = stat.isFile(); diagnostic.reportBytes = stat.size;
    if (!stat.isFile() || stat.size >= scannerLimits.inputBytes) {
      diagnostic.reportState = 'NOT_REGULAR_OR_OVERSIZED'; return diagnostic;
    }
    const bytes = fs.readFileSync(reportPath);
    diagnostic.reportHash = crypto.createHash('sha256').update(bytes).digest('hex');
    const data = JSON.parse(bytes.toString('utf8'));
    if (!Array.isArray(data.Results) || data.Results.some(r => !r ||
      r.Vulnerabilities !== undefined && !Array.isArray(r.Vulnerabilities))) {
      diagnostic.reportState = 'INVALID_SCHEMA'; return diagnostic;
    }
    const vulnerabilities = data.Results.flatMap(r => r.Vulnerabilities || []);
    if (vulnerabilities.some(v => !v || typeof v.Severity !== 'string')) {
      diagnostic.reportState = 'INVALID_SCHEMA'; return diagnostic;
    }
    diagnostic.reportState = 'VALID_JSON';
    diagnostic.highCount = vulnerabilities.filter(v => v.Severity === 'HIGH').length;
    diagnostic.criticalCount = vulnerabilities.filter(v => v.Severity === 'CRITICAL').length;
  } catch { diagnostic.reportState = diagnostic.reportGenerated ? 'UNREADABLE_OR_INVALID_JSON' : 'MISSING_OR_UNREADABLE'; }
  return diagnostic;
}

export function observeTrivyVersion(diagnostic, result, env = process.env) {
  const version = result.status === 0 ? String(result.stdout ?? '').match(/^Version:\s*(\d+\.\d+\.\d+(?:[-+][\w.-]+)?)\s*$/m)?.[1] : null;
  diagnostic.toolVersion = version || null;
  diagnostic.toolVersionSource = version ? 'SAME_DIGEST_VERSION_COMMAND' : 'NOT_OBSERVED';
  diagnostic.versionProbeExitCode = Number.isInteger(result.status) ? result.status : null;
  diagnostic.versionProbeError = text(result.error?.code, env, 128, 1) || null;
}

function relativeFile(uri, repository, env) {
  if (typeof uri !== 'string') return null;
  let file;
  try { file = decodeURIComponent(uri).replaceAll('\\', '/'); } catch { return null; }
  if (/[\x00-\x1f?#]/.test(file)) return null;
  const base = path.resolve(repository).replaceAll('\\', '/').replace(/\/$/, '') + '/';
  if (file.startsWith('file://')) {
    try {
      const url = new URL(file);
      if (url.host && url.host !== 'localhost') return null;
      file = fileURLToPath(url).replaceAll('\\', '/');
    } catch { return null; }
  }
  if (file.startsWith(base)) file = file.slice(base.length);
  if (/^(?:\/|[A-Za-z]:|[a-z][a-z0-9+.-]*:)/i.test(file) || file.split('/').includes('..')) return null;
  file = path.posix.normalize(file);
  return file === '.' ? null : text(file, env, 512, 1);
}

export function codeqlFindings(documents, {repository, env = process.env}) {
  let totalResults = 0;
  const findings = [];
  for (const data of documents) for (const run of data.runs) {
    for (let resultIndex = 0; resultIndex < run.results.length; resultIndex++) {
      const result = run.results[resultIndex] || {};
      const index = totalResults++;
      if (findings.length >= scannerLimits.results) continue;
      const rule = run.tool?.driver?.rules?.[result.ruleIndex] ||
        run.tool?.driver?.rules?.find(r => r.id === result.ruleId);
      const location = result.locations?.[0]?.physicalLocation;
      const artifact = location?.artifactLocation;
      const uri = artifact?.uri ?? run.artifacts?.[artifact?.index]?.location?.uri;
      const severity = rule?.properties?.['security-severity'];
      const line = value => Number.isSafeInteger(value) && value > 0 ? value : null;
      findings.push({index, resultIndex, ruleId: text(result.ruleId ?? rule?.id, env, 128, 1) || null,
        level: text(result.level ?? rule?.defaultConfiguration?.level ?? 'warning', env, 32, 1),
        securitySeverity: typeof severity === 'number' || typeof severity === 'string' ? text(severity, env, 32, 1) : null,
        file: relativeFile(uri, repository, env), startLine: line(location?.region?.startLine), endLine: line(location?.region?.endLine),
        message: text(result.message?.text ?? result.message?.markdown, env),
        baselineState: ['new', 'unchanged', 'updated', 'absent'].includes(result.baselineState) ? result.baselineState : 'UNKNOWN',
        truePositive: 'NEEDS_REVIEW'});
    }
  }
  return {schema: 1, sarifGenerated: documents.length > 0, sarifFiles: documents.length,
    totalResults, blockingResultCount: totalResults, blockingPolicy: 'ALL_RESULTS_UNCHANGED',
    findings, omittedResults: totalResults - findings.length, truncated: totalResults > findings.length};
}
