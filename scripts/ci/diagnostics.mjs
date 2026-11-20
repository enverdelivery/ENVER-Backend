// Solo evidencia diagnóstica: sin procesos, red, entorno completo ni logs de aplicación.
import path from 'node:path';

export const diagnosticLimits = {lines: 160, characters: 16000, failedTests: 20, trxCharacters: 32 * 1024 * 1024, httpBytes: 65536};
const sensitiveName = /TOKEN|SECRET|PASSWORD|PASSWD|CREDENTIAL|CONNECTIONSTRING|PRIVATE.?KEY|SIGNING.?KEY|KEYBASE64|PSEUDONYMIZATION|AWS_ACCESS_KEY_ID|JWT|MONGO|REDIS|DATABASE_URL/i;
function opaque(value) {
  if (/^[a-f0-9]{32,}$/i.test(value)) return true;
  const counts = new Map(); for (const c of value) counts.set(c, (counts.get(c) || 0) + 1);
  const entropy = [...counts.values()].reduce((sum, n) => sum - n / value.length * Math.log2(n / value.length), 0);
  return entropy > 4.5 && /[0-9+/=]/.test(value);
}

export function redact(value, env = process.env, extraSecrets = []) {
  let text = String(value ?? '').replace(/\x1b\[[0-?]*[ -/]*[@-~]/g, '').replace(/[\u0000-\u0008\u000b\u000c\u000e-\u001f\u007f\u202a-\u202e\u2066-\u2069]/g, '');
  const values = [...Object.entries(env).filter(([name]) => sensitiveName.test(name)).map(([, v]) => v), ...extraSecrets];
  const secrets = [...new Set(values.filter(v => typeof v === 'string' && v.length > 0).flatMap(v =>
    [v, encodeURIComponent(v), JSON.stringify(v).slice(1, -1), Buffer.from(v).toString('base64')]))].sort((a, b) => b.length - a.length);
  for (const secret of secrets) text = text.split(secret).join('[REDACTED]');
  text = text.replace(/\b(?:proxy-authorization|authorization|set-cookie|cookie)["']?\s*[:=]\s*[^\r\n]*/gi, '[REDACTED_HEADER]');
  text = text.replace(/\bBearer\s+[^\s"'<>]+/gi, 'Bearer [REDACTED]');
  text = text.replace(/\b(?:gh[pousr]_[A-Za-z0-9_]{10,}|github_pat_[A-Za-z0-9_]{10,})\b/g, '[REDACTED_GITHUB_TOKEN]');
  text = text.replace(/\b(?:AKIA|ASIA|AIDA|AROA|AIPA|ANPA|ANVA|ASCA)[A-Z0-9]{16}\b/g, '[REDACTED_AWS_KEY]');
  text = text.replace(/\beyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+(?:\.[A-Za-z0-9_-]*)?/g, '[REDACTED_JWT]');
  text = text.replace(/\b(?:AWS_ACCESS_KEY_ID|AWS_SECRET_ACCESS_KEY|AWS_SESSION_TOKEN|GH_TOKEN|GITHUB_TOKEN|OriginSecret|JWT[_:]?(?:SECRET|SIGNING[_:]?KEY)|PseudonymizationKeyBase64|KeyBase64|Password|Passwd|ClientSecret|ConnectionString|SigningKey|SigningSecret|SecretAccessKey|SessionToken|AccessToken|IdToken|RefreshToken)["']?\s*[:=]\s*(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|[^\r\n]*)/gi, '[REDACTED_SECRET_ASSIGNMENT]');
  text = text.replace(/\b[a-z][a-z0-9+.-]*:\/\/[^\s<>"']+/gi, raw => {
    try {
      const url = new URL(raw);
      return `${url.protocol}//${url.host}${url.username || url.password ? '/[REDACTED_CREDENTIAL_URI]' : url.pathname}${url.search || url.hash ? '?[REDACTED_QUERY]' : ''}`;
    } catch { return '[REDACTED_URL]'; }
  });
  text = text.replace(/(["'])([A-Za-z0-9+/_=-]{32,})\1/g, (all, quote, value) => opaque(value) ? quote + '[REDACTED_OPAQUE_VALUE]' + quote : all);
  return text.replace(/[A-Za-z0-9+/_=-]{100,}/g, value => opaque(value) ? '[REDACTED_OPAQUE_VALUE]' : value);
}

export function safeText(value, {env = process.env, extraSecrets = [], characters = diagnosticLimits.characters, lines = diagnosticLimits.lines} = {}) {
  // Redactar ANTES de recortar: nunca publicar el fragmento cortado de un secreto.
  const clean = redact(value, env, extraSecrets).split(/\r?\n/).slice(-lines).join('\n');
  return clean.length > characters ? '[TRUNCATED]\n' + clean.slice(-(characters - 12)) : clean;
}

export function sanitizeDiagnostic(value) {
  if (typeof value === 'string') return safeText(value);
  if (Array.isArray(value)) return value.map(sanitizeDiagnostic);
  if (value && typeof value === 'object') return Object.fromEntries(Object.entries(value).map(([k, v]) => [k, sanitizeDiagnostic(v)]));
  return value;
}

function approvedArguments(tool, args) {
  const simplePath = v => /^[\w .:/\\-]+$/.test(v) && !/secret|password|token|credential/i.test(v);
  if (args.length > 40) return false;
  if (tool === 'terraform') return args.length === 3 && /^-chdir=/.test(args[0]) && simplePath(args[0].slice(7)) && args[1] === 'validate' && args[2] === '-no-color';
  for (let i = 0; i < args.length; i++) {
    const a = args[i];
    if (i === 0 && ['restore', 'build', 'test'].includes(a) || ['--locked-mode', '--no-build', '--no-restore'].includes(a)) continue;
    if (/\.(?:csproj|slnx)$/.test(a) && simplePath(a)) continue;
    if (a.startsWith('-p:CustomAfterMicrosoftCommonTargets=') && simplePath(a.slice(a.indexOf('=') + 1)) && a.endsWith('RuntimeToolRestore.targets')) continue;
    if (a === '-c' && args[++i] === 'Release' || a === '--verbosity' && args[++i] === 'quiet') continue;
    if (a === '--logger' && /^trx;LogFileName=(?:unit|integration|security)\.trx$/.test(args[++i])) continue;
    if (a === '--results-directory' && typeof args[i + 1] === 'string' && simplePath(args[++i])) continue;
    return false;
  }
  return true;
}

export function commandDiagnostic(executable, args, result, env = process.env) {
  const tool = path.basename(executable).replace(/\.exe$/i, '').toLowerCase();
  const operation = tool === 'dotnet' ? args[0] : args.find(a => a === 'validate');
  if (!(tool === 'dotnet' && ['restore', 'build', 'test'].includes(operation) || tool === 'terraform' && operation === 'validate')) return null;
  if (result.status === 0 && !result.error) return null;
  const cleanArgs = args.map(a => safeText(a, {env, characters: 1024, lines: 1}));
  const argumentsSafe = approvedArguments(tool, args) && args.every((a, i) => a === cleanArgs[i]);
  // Test stdout puede incluir logs de la aplicación. Para test solo códigos del tooling;
  // los fallos de assertions proceden del TRX sin StdOut/StdErr del test.
  const useful = value => {
    const clean = redact(value ?? '', env);
    if (tool === 'terraform') return clean;
    return clean.split(/\r?\n/).filter(l => operation === 'test'
      ? /\b(?:NU|MSB|CS|NETSDK)\d{4}\b/.test(l)
      : /\b(?:error|warning)\b|\b(?:NU|MSB|CS|NETSDK)\d{4}\b|error MSBuild|Build FAILED/i.test(l)).join('\n');
  };
  return {tool, operation, exitCode: result.status, processError: result.error?.code || null,
    arguments: argumentsSafe ? cleanArgs : undefined, argumentsOmitted: argumentsSafe ? undefined : 'UNAPPROVED_OR_SECRET_LIKE_ARGUMENT',
    stderr: safeText(useful(result.stderr), {env}), stdout: safeText(useful(result.stdout), {env})};
}

function xmlText(value = '') {
  return value.replace(/<!\[CDATA\[([\s\S]*?)\]\]>/g, '$1').replace(/&(?:#x([0-9a-f]+)|#([0-9]+)|([a-z]+));/gi, (all, hex, decimal, name) => {
    if (name) return ({quot: '"', apos: "'", lt: '<', gt: '>', amp: '&'})[name] ?? all;
    const n = parseInt(hex || decimal, hex ? 16 : 10); return n > 0 && n <= 0x10ffff ? String.fromCodePoint(n) : '';
  });
}
function attributes(value) {
  return Object.fromEntries([...value.matchAll(/([\w:]+)\s*=\s*(["'])([\s\S]*?)\2/g)].map(m => [m[1], xmlText(m[3])]));
}
function element(value, name) {
  return xmlText(value.match(new RegExp('<(?:\\w+:)?' + name + '(?:\\s[^>]*)?>([\\s\\S]*?)</(?:\\w+:)?' + name + '>'))?.[1] || '');
}

export function trxDiagnostic(xml, env = process.env) {
  if (xml.length > diagnosticLimits.trxCharacters || /<!DOCTYPE|<!ENTITY/i.test(xml) || !/<\/(?:\w+:)?TestRun>\s*$/.test(xml))
    return {format: 'TRX', available: false, reason: 'UNSAFE_OVERSIZED_OR_INCOMPLETE_TRX'};
  const counters = attributes(xml.match(/<(?:\w+:)?Counters\s+([^>]+)>/)?.[1] || '');
  const counts = Object.fromEntries(['total', 'passed', 'failed', 'notExecuted'].map(k => [k === 'notExecuted' ? 'skipped' : k, /^\d+$/.test(counters[k] || '') ? Number(counters[k]) : null]));
  const definitions = new Map();
  for (const m of xml.matchAll(/<(?:\w+:)?UnitTest\s+([^>]+)>([\s\S]*?)<\/(?:\w+:)?UnitTest>/g)) {
    const id = attributes(m[1]).id, method = attributes(m[2].match(/<(?:\w+:)?TestMethod\s+([^>]+)>/)?.[1] || '');
    if (id && method.className && method.name) definitions.set(id, method.className + '.' + method.name);
  }
  const failedTests = []; let failures = 0;
  for (const m of xml.matchAll(/<(?:\w+:)?UnitTestResult\b([^>]*?)(?:\/>|>([\s\S]*?)<\/(?:\w+:)?UnitTestResult>)/g)) {
    const a = attributes(m[1]); if (a.outcome !== 'Failed') continue;
    failures++; if (failedTests.length >= diagnosticLimits.failedTests) continue;
    const info = m[2]?.match(/<(?:\w+:)?ErrorInfo>([\s\S]*?)<\/(?:\w+:)?ErrorInfo>/)?.[1] || '';
    const clean = (v, characters = 6000) => safeText(v, {env, characters});
    const stackTrace = clean(element(info, 'StackTrace'));
    failedTests.push({name: clean((definitions.get(a.testId) || a.testName || 'UNKNOWN').split('(')[0], 512),
      message: clean(element(info, 'Message')), stackTrace,
      source: [...stackTrace.matchAll(/\bin ([^\r\n]+?):line (\d+)\b/g)].slice(0, 8).map(s => ({file: s[1], line: Number(s[2])}))});
  }
  return {format: 'TRX', available: true, counts, failedTests, omittedFailures: Math.max(0, failures - failedTests.length)};
}

export async function githubDiagnostic(response, endpoint, env = process.env, token = '') {
  const headers = {};
  for (const name of ['x-github-request-id', 'x-accepted-github-permissions', 'x-oauth-scopes', 'x-ratelimit-limit', 'x-ratelimit-remaining', 'x-ratelimit-reset', 'x-ratelimit-resource', 'x-ratelimit-used']) {
    const value = response.headers.get(name); if (value !== null) headers[name] = safeText(value, {env, extraSecrets: [token], characters: 512, lines: 1});
  }
  let body = {}, bodyState = 'UNAVAILABLE';
  const reader = response.body?.getReader();
  if (reader) {
    const chunks = []; let size = 0;
    try {
      while (true) {
        const {value, done} = await reader.read(); if (done) break;
        size += value.length;
        if (size > diagnosticLimits.httpBytes) { bodyState = 'OVERSIZED'; await reader.cancel(); break; }
        chunks.push(Buffer.from(value));
      }
      if (bodyState !== 'OVERSIZED') { body = JSON.parse(Buffer.concat(chunks).toString('utf8')); bodyState = 'JSON_FIELDS_ONLY'; }
    } catch { bodyState = 'UNAVAILABLE_OR_NON_JSON'; }
  }
  const clean = v => typeof v === 'string' ? safeText(v, {env, extraSecrets: [token], characters: 2048, lines: 10}) : undefined;
  return {status: response.status, endpoint: clean(endpoint.split('?')[0]), headers, bodyState,
    message: clean(body?.message), documentation_url: clean(body?.documentation_url)};
}
