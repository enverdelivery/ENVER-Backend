import test from 'node:test';
import assert from 'node:assert/strict';
import crypto from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import {redact, safeText, commandDiagnostic, trxDiagnostic, githubDiagnostic, diagnosticLimits} from '../ci/diagnostics.mjs';
import {GitHub, Closed, recordDiagnostic, diagnosticDir, command} from '../ci/lib.mjs';

const canary = () => crypto.randomBytes(24).toString('hex');
const noLeaks = (value, secrets) => { const text = JSON.stringify(value); for (const s of secrets) assert.ok(!text.includes(s), 'Synthetic credential must be absent'); };
test('redacts Authorization, Bearer, AWS-like keys, GitHub tokens and JWT without printing canaries', () => {
  const bearer = canary(), basic = canary(), aws = ['AK', 'IA'].join('') + 'Z'.repeat(16), gh = ['gh', 'p_'].join('') + canary();
  const jwt = ['eyJ' + canary(), canary(), canary()].join('.');
  const out = redact(`Authorization: Basic ${basic}\nBearer ${bearer}\n${aws}\n${gh}\n${jwt}`, {});
  noLeaks(out, [basic, bearer, aws, gh, jwt]); assert.match(out, /REDACTED/);
});
test('known environment secrets are redacted raw, JSON escaped, URL encoded and base64', () => {
  const secret = 'synthetic/' + canary() + '\n"end';
  const representations = [secret, encodeURIComponent(secret), JSON.stringify(secret).slice(1, -1), Buffer.from(secret).toString('base64')];
  noLeaks(redact(representations.join('\n'), {GITHUB_TOKEN: secret, NOT_SENSITIVE: 'keep'}), representations);
  noLeaks(redact('value abc', {JWT_SECRET: 'abc'}), ['abc']);
});
test('redacts runtime secret assignments, cookies and connection credentials; keeps feed hostname', () => {
  const secret = canary(), user = 'synthetic-user-' + canary();
  const out = redact(`OriginSecret="${secret}"\nAWS_SESSION_TOKEN=${secret}\nSigningKey='${secret}'\nSet-Cookie: session=${secret}\nerror NU1301 https://${user}:${secret}@feed.example.invalid/v3/index.json?token=${secret}\nmongodb://${user}:${secret}@db.example.invalid/store\nrediss://${user}:${secret}@cache.example.invalid:6379/0`, {});
  noLeaks(out, [secret, user]); assert.ok(out.includes('feed.example.invalid')); assert.ok(!out.includes('?token='));
});
test('redaction precedes line/character truncation, removes ANSI/control characters and bounds output', () => {
  const secret = canary().repeat(5), value = '\x1b[31merror NU1301\x1b[0m\n' + Array.from({length: 200}, (_, i) => `error ${i} ${secret}`).join('\n');
  const out = safeText(value, {env: {OriginSecret: secret}, characters: 1024, lines: 100});
  noLeaks(out, [secret, secret.slice(-30)]); assert.ok(out.length <= 1024); assert.ok(!out.includes('\x1b'));
});
test('quoted opaque signing material is redacted without masking long source test identifiers', () => {
  const secret = crypto.randomBytes(32).toString('base64'), identifier = 'Enver.Tests.' + 'Rejects_unavailable_shared_authority_without_emission_or_mutation_'.repeat(3);
  noLeaks(redact(`Actual: "${secret}"`, {}), [secret]); assert.equal(redact(identifier, {}), identifier);
});
test('command diagnostics preserve restore/build error codes, project, source line and safe arguments', () => {
  for (const operation of ['restore', 'build']) {
    const args = [operation, 'tools/Synthetic.Tests/Synthetic.Tests.csproj', '--verbosity', 'quiet'];
    const d = commandDiagnostic('dotnet', args, {status: 1, stderr: 'error NU1301: feed https://feed.example.invalid/v3/index.json', stdout: 'Noise\nSynthetic.cs(12,3): error CS0123: mismatch [Synthetic.Tests.csproj]'}, {});
    assert.equal(d.exitCode, 1); assert.equal(d.operation, operation); assert.deepEqual(d.arguments, args);
    assert.match(d.stderr, /NU1301/); assert.match(d.stdout, /Synthetic.cs\(12,3\): error CS0123/); assert.ok(!d.stdout.includes('Noise'));
  }
});
test('secret-like or unapproved arguments are omitted; output still redacted', () => {
  const secret = canary(), args = ['restore', 'Example.csproj', '--source', `https://user:${secret}@feed.example.invalid/v3`];
  const d = commandDiagnostic('dotnet', args, {status: 1, stderr: `error NU1301 ${args.at(-1)}`}, {});
  assert.equal(d.arguments, undefined); assert.equal(d.argumentsOmitted, 'UNAPPROVED_OR_SECRET_LIKE_ARGUMENT'); noLeaks(d, [secret]);
  assert.equal(commandDiagnostic('dotnet', ['restore'], {status: 0}, {}), null);
  for (const [exe, a] of [['aws', ['sts', 'get-caller-identity']], ['docker', ['run']], ['terraform', ['plan']], ['terraform', ['apply']], ['dotnet', ['run']]])
    assert.equal(commandDiagnostic(exe, a, {status: 1, stderr: secret}, {}), null);
});
test('terraform validate preserves exact multiline diagnostic without state/variable commands', () => {
  const stderr = 'Error: Invalid function argument\n\n  on compute.tf line 68, in resource "example" "host":\n  68: value = file("bootstrap/example.sh")\n\nInvalid value for path parameter.';
  const d = commandDiagnostic('terraform', ['-chdir=/tmp/enver-ci-private/iac-source', 'validate', '-no-color'], {status: 1, stderr}, {});
  assert.equal(d.stderr, stderr); assert.equal(d.operation, 'validate'); assert.equal(d.exitCode, 1);
});
test('only the fixed Node helper test command preserves bounded sanitized assertions and stack', () => {
  const secret = canary(), args = ['--test', '/repo/scripts/tests/pipeline.test.mjs'];
  const d = commandDiagnostic('node', args, {status: 1, stdout: `not ok 1 - workflow assertion\nAssertionError [ERR_ASSERTION]\n at TestContext (/repo/scripts/tests/pipeline.test.mjs:110:10)\nPassword=${secret}`, stderr: ''}, {});
  assert.equal(d.operation, '--test'); assert.equal(d.exitCode, 1); assert.deepEqual(d.arguments, args);
  assert.match(d.stdout, /ERR_ASSERTION/); assert.match(d.stdout, /pipeline.test.mjs:110/); noLeaks(d, [secret]);
  for (const other of [['-e', 'untrusted'], ['--test', '/repo/tests/business.test.mjs'], ['--test', '/repo/scripts/tests/token.test.mjs']])
    assert.equal(commandDiagnostic('node', other, {status: 1, stdout: secret}, {}), null);
  assert.equal(commandDiagnostic('node', args, {status: 0}, {}), null);
});
test('dotnet test command diagnostic excludes arbitrary application logs; TRX carries assertions', () => {
  const secret = canary(), d = commandDiagnostic('dotnet', ['test', 'Example.csproj'], {status: 1, stdout: `application error: ${secret}\nExample.cs(1): error CS0001: fixture`, stderr: 'arbitrary application stderr'}, {});
  noLeaks(d, [secret]); assert.equal(d.stderr, ''); assert.match(d.stdout, /CS0001/);
});
const trx = (message, stack, extra = '') => `<TestRun><Results><UnitTestResult testId="1" testName="Synthetic.Test(secret: untrusted)" outcome="Failed"><Output><ErrorInfo><Message>${message}</Message><StackTrace>${stack}</StackTrace></ErrorInfo><StdOut>${extra}</StdOut></Output></UnitTestResult></Results><TestDefinitions><UnitTest id="1"><TestMethod className="Enver.Synthetic.Tests" name="Fails" /></UnitTest></TestDefinitions><ResultSummary><Counters total="2" passed="1" failed="1" notExecuted="0" /></ResultSummary></TestRun>`;
test('TRX extracts fully qualified failure, message, stack, source and counts, not application StdOut', () => {
  const secret = canary(), xml = trx('Assert.Equal() Failure\nExpected: &quot;A&quot;\nActual: &quot;B&quot;', 'at Enver.Synthetic.Tests.Fails() in /repo/tests/Synthetic.cs:line 42', secret);
  const d = trxDiagnostic(xml, {}); assert.equal(d.available, true); assert.deepEqual(d.counts, {total: 2, passed: 1, failed: 1, skipped: 0});
  assert.equal(d.failedTests[0].name, 'Enver.Synthetic.Tests.Fails'); assert.match(d.failedTests[0].message, /Expected: "A"/);
  assert.deepEqual(d.failedTests[0].source, [{file: '/repo/tests/Synthetic.cs', line: 42}]); noLeaks(d, [secret, 'untrusted']);
});
test('TRX decodes XML/numeric entities and CDATA before redaction, including stack secrets', () => {
  const secret = canary(), encoded = [...secret].map(c => '&#' + c.charCodeAt(0) + ';').join('');
  const d = trxDiagnostic(trx(`<![CDATA[Expected failure]]>\nOriginSecret=&quot;${encoded}&quot;`, `Bearer ${secret}`), {});
  noLeaks(d, [secret]); assert.match(d.failedTests[0].message, /Expected failure/);
});
test('TRX rejects external entities/truncation, supports namespaces and caps failure/message counts', () => {
  assert.equal(trxDiagnostic('<!DOCTYPE x [<!ENTITY y SYSTEM "file:///private">]>' + trx('x', ''), {}).available, false);
  assert.equal(trxDiagnostic(trx('x', '').slice(0, -10), {}).available, false);
  const namespaced = trx('x', '').replace(/<(\/)?(TestRun|UnitTestResult|Counters|ErrorInfo|Message|StackTrace)(?=[\s>])/g, '<$1t:$2');
  assert.equal(trxDiagnostic(namespaced, {}).failedTests.length, 1);
  const unit = '<UnitTestResult testName="Synthetic.Fails" outcome="Failed"><Output><ErrorInfo><Message>' + 'bounded message '.repeat(1000) + '</Message></ErrorInfo></Output></UnitTestResult>';
  const d = trxDiagnostic('<TestRun>' + unit.repeat(25) + '<Counters total="25" passed="0" failed="25" notExecuted="0" /></TestRun>', {});
  assert.equal(d.failedTests.length, diagnosticLimits.failedTests); assert.equal(d.omittedFailures, 5); assert.ok(d.failedTests[0].message.length <= 6000);
});
test('GitHub failure diagnostics whitelist safe headers/body fields and redact credential values', async () => {
  const secret = canary(), response = new Response(JSON.stringify({message: `Forbidden Bearer ${secret}`, documentation_url: 'https://docs.github.com/rest', arbitrary: secret}), {status: 403, headers: {'x-github-request-id': 'request-123', 'x-accepted-github-permissions': 'contents=read', 'x-ratelimit-remaining': '20', authorization: secret, 'set-cookie': secret}});
  const d = await githubDiagnostic(response, '/repos/owner/repo/dependency-graph/compare/a...b?token=' + secret, {}, secret);
  assert.equal(d.status, 403); assert.equal(d.headers['x-github-request-id'], 'request-123'); assert.equal(d.headers.authorization, undefined);
  assert.equal(d.headers['set-cookie'], undefined); assert.equal(d.arbitrary, undefined); assert.equal(d.endpoint.includes('?'), false); noLeaks(d, [secret]);
});
test('GitHub body capture is bounded, ignores non-JSON and non-string fields', async () => {
  const large = await githubDiagnostic(new Response('x'.repeat(diagnosticLimits.httpBytes + 1), {status: 403}), '/repos/x/y', {});
  assert.equal(large.bodyState, 'OVERSIZED'); assert.equal(large.message, undefined);
  const invalid = await githubDiagnostic(new Response('not json', {status: 403}), '/repos/x/y', {}); assert.equal(invalid.message, undefined);
  const structured = await githubDiagnostic(new Response('{"message":{"private":"value"}}', {status: 403}), '/repos/x/y', {}); assert.equal(structured.message, undefined);
});
test('custom GitHub request remains BLOCKED on 403; safe evidence persists without changing permissions', async t => {
  t.mock.method(globalThis, 'fetch', async () => new Response('{"message":"Forbidden","documentation_url":"https://docs.github.com/rest"}', {status: 403, headers: {'x-github-request-id': 'synthetic-id'}}));
  await assert.rejects(() => new GitHub('synthetic-token').request('/repos/owner/repo/dependency-graph/compare/a...b'), e => {
    assert.ok(e instanceof Closed); assert.equal(e.state, 'BLOCKED'); assert.equal(e.code, 'GITHUB_API_403');
    assert.equal(e.details.diagnostic.message, 'Forbidden'); return true;
  });
});
test('central persistence redacts before writing only dedicated diagnostic JSON; original command still fails', () => {
  const secret = canary(); process.env.ENVER_DIAGNOSTIC_TOKEN = secret;
  try {
    recordDiagnostic('test', {message: 'value ' + secret});
    for (const f of fs.readdirSync(diagnosticDir)) noLeaks(fs.readFileSync(path.join(diagnosticDir, f), 'utf8'), [secret]);
    assert.throws(() => recordDiagnostic('../private', {}));
    assert.equal(recordDiagnostic('test', {messages: Array(100).fill('bounded text '.repeat(2000))}).reason, 'DIAGNOSTIC_RECORD_SIZE_LIMIT');
    assert.throws(() => command(process.execPath, ['-e', 'process.exit(7)']), e => e.code.startsWith('COMMAND_FAILED_') && e.details.exitCode === 7);
  } finally { delete process.env.ENVER_DIAGNOSTIC_TOKEN; }
});
