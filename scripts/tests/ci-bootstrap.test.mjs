import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {root, policy} from '../ci/lib.mjs';
import {prepareMongoShell, restoreArguments, verifyValidationLock} from '../ci/pipeline.mjs';

test('private validation lock may add platform h1 while preserving every committed selection and checksum', () => {
  const before = fs.readFileSync(path.join(root, 'infra/.terraform.lock.hcl'), 'utf8');
  const h1 = 'h1:' + Buffer.alloc(32, 7).toString('base64');
  const after = before.replace('hashes = [', `hashes = [\n    "${h1}",`);
  assert.deepEqual(verifyValidationLock(before, before), {repositoryLock: 'UNCHANGED', addedPlatformHashes: 0});
  assert.deepEqual(verifyValidationLock(before, after), {repositoryLock: 'UNCHANGED', addedPlatformHashes: 1});
  assert.equal(fs.readFileSync(path.join(root, 'infra/.terraform.lock.hcl'), 'utf8'), before);
});
test('private validation lock rejects version, constraints, provider, removed hashes and new archive hashes', () => {
  const before = fs.readFileSync(path.join(root, 'infra/.terraform.lock.hcl'), 'utf8');
  for (const after of [before.replace('6.73.0', '6.74.0'), before.replace('~> 6.0', '>= 6.0'),
    before.replace('hashicorp/aws', 'unapproved/aws'), before.replace(/^\s*"zh:[^"\r\n]+",\r?\n/m, ''),
    before.replace('hashes = [', `hashes = [\n    "zh:${'a'.repeat(64)}",`),
    before.replace('hashes = [', 'hashes = [\n    "h1:invalid",')])
    assert.throws(() => verifyValidationLock(before, after));
});

test('only the package-dependent Bootstrap restores override the empty CONTROL feed list', () => {
  for (const project of ['Enver.QuotaControl.Bootstrap', 'Enver.QuotaControl.Bootstrap.Tests']) {
    const args = restoreArguments(`tools/${project}/${project}.csproj`);
    assert.equal(args[args.indexOf('--source') + 1], 'https://api.nuget.org/v3/index.json');
    assert.ok(!args.includes('--ignore-failed-sources'));
    assert.ok(args.some(a => a.endsWith('RuntimeToolRestore.targets')));
  }
  for (const project of ['Enver.QuotaControl', 'Enver.QuotaControl.PipeProducer.Tests'])
    assert.ok(!restoreArguments(`tools/${project}/${project}.csproj`).includes('--source'));
  assert.ok(restoreArguments('Enver.Backend.slnx').includes('--locked-mode'));
});

test('Mongo shell preparation uses exact fixture digest and checks container-local pinned version without credentials', () => {
  const calls = [];
  const report = prepareMongoShell((exe, args) => { calls.push({exe, args}); return {status: 0, stdout: policy.mongoShell.version + '\n'}; });
  assert.equal(report.state, 'PASS'); assert.equal(report.location, 'TESTCONTAINERS_IMAGE');
  assert.deepEqual(calls[0], {exe: 'docker', args: ['pull', policy.mongoShell.image]});
  assert.deepEqual(calls[1], {exe: 'docker', args: ['run', '--rm', '--network', 'none', '--entrypoint', '/bin/sh', policy.mongoShell.image,
    '-c', 'command -v mongosh >/dev/null && mongosh --version']});
  assert.ok(!JSON.stringify(calls).match(/password|username|connection|AWS/i));
});
test('Mongo shell missing, mismatched or failed setup is fail-closed before tests', () => {
  for (const result of [{status: 1, stdout: ''}, {status: 0, stdout: '0.0.0'}, {status: 0, stdout: ''}])
    assert.throws(() => prepareMongoShell(() => result), /MONGO_SHELL_UNAVAILABLE_OR_VERSION_MISMATCH/);
  assert.throws(() => prepareMongoShell(() => { throw Error('installation failed'); }), /installation failed/);
});
test('Windows runtime builds the checkout CONTROL prerequisite before existing suites without changing historical artifacts', () => {
  const source = fs.readFileSync(path.join(root, 'scripts/ci/pipeline.mjs'), 'utf8');
  const runtime = source.slice(source.indexOf('async function runtime()'), source.indexOf('async function semgrep()'));
  assert.ok(runtime.indexOf("'tools/Enver.QuotaControl/Enver.QuotaControl.csproj'") < runtime.indexOf('for (const project of list)'));
  assert.match(runtime, /process.platform === 'win32'/);
  assert.match(runtime, /restore\(prerequisite\); build\(prerequisite\)/);
  assert.match(runtime, /--no-build/);
});
