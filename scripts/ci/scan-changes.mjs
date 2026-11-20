import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {command, root, privateDir, policy, requireThat, readJson} from './lib.mjs';
// Includes untracked source without staging. No generated files or broad recursive filesystem scan.
try {
  fs.mkdirSync(privateDir, {recursive: true, mode: 0o700});
  const dir = fs.mkdtempSync(path.join(privateDir, 'publicable-'));
  const files = [...new Set([
    ...command('git', ['diff', '--name-only', '-z', 'HEAD', '--']).stdout.split('\0'),
    ...command('git', ['ls-files', '--others', '--exclude-standard', '-z']).stdout.split('\0'),
  ].filter(Boolean))];
  let count = 0;
  for (const file of files) {
    const source = path.join(root, file);
    if (!fs.existsSync(source)) continue;
    requireThat(!fs.lstatSync(source).isSymbolicLink() && fs.lstatSync(source).isFile(), 'SCAN_NONREGULAR_PATH');
    const dest = path.join(dir, file); requireThat(dest.startsWith(dir + path.sep), 'SCAN_PATH');
    fs.mkdirSync(path.dirname(dest), {recursive: true}); fs.copyFileSync(source, dest); count++;
  }
  const r = command('docker', ['run', '--rm', '--network', 'none', '--mount', `type=bind,src=${dir},dst=/scan,readonly`, '--mount', `type=bind,src=${root}/.gitleaks.toml,dst=/gitleaks.toml,readonly`, '--mount', `type=bind,src=${privateDir},dst=/private`, policy.images.gitleaks,
    'dir', '/scan', '--config', '/gitleaks.toml', '--redact=100', '--no-banner', '--report-format', 'json', '--report-path', '/private/changes-gitleaks.json'], {allowFailure: true});
  requireThat(r.status === 0 || r.status === 1, 'SECRET_SCANNER_NOT_EXECUTED');
  const data = readJson(path.join(privateDir, 'changes-gitleaks.json'));
  requireThat(r.status === 0 && data.length === 0, 'SECRET_FINDING_REDACTED_REPORT_LOCAL');
  console.log(JSON.stringify({secretScan: 'PASS', files: count, findings: 0, scanner: policy.toolVersions.gitleaks}));
} catch (e) { console.log('SECRET_SCAN_FAIL: ' + (e.code || 'SCANNER_ERROR')); process.exitCode = 1; }
