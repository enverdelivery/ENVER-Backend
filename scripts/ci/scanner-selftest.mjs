import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {command, root, privateDir, policy, requireThat, readJson} from './lib.mjs';
// A fresh NON-OPERATIVE canary. Never printed, uploaded, committed or supplied to ENVER/AWS.
try {
  fs.mkdirSync(privateDir, {recursive: true, mode: 0o700});
  const dir = fs.mkdtempSync(path.join(privateDir, 'scanner-canary-'));
  const fixture = path.join(dir, 'synthetic-canary.txt');
  fs.writeFileSync(fixture, 'OriginSecret = "' + crypto.randomBytes(48).toString('base64') + '"\napi_key = "' + crypto.randomBytes(48).toString('base64') + '"\n', {mode: 0o600});
  try {
    const r = command('docker', ['run', '--rm', '--network', 'none', '--mount', `type=bind,src=${dir},dst=/scan,readonly`, '--mount', `type=bind,src=${root}/.gitleaks.toml,dst=/gitleaks.toml,readonly`, '--mount', `type=bind,src=${privateDir},dst=/private`, policy.images.gitleaks,
      'dir', '/scan', '--config', '/gitleaks.toml', '--redact=100', '--no-banner', '--report-format', 'json', '--report-path', '/private/scanner-selftest.json'], {allowFailure: true});
    const data = readJson(path.join(privateDir, 'scanner-selftest.json'));
    requireThat(r.status === 1 && ['enver-runtime-secret', 'generic-api-key'].every(id => data.some(f => f.RuleID === id)), 'SCANNER_CANARY_NOT_DETECTED');
    requireThat(data.every(f => !f.Secret || /^REDACTED$/i.test(f.Secret)), 'SCANNER_REDACTION_FAILED');
    console.log('SCANNER_SELFTEST: PASS; canary detected and redacted; no real secrets used');
  } finally { fs.unlinkSync(fixture); fs.rmdirSync(dir); }
} catch (e) { console.log('SCANNER_SELFTEST: FAIL (' + (e.code || 'SCANNER_ERROR') + ')'); process.exitCode = 1; }
