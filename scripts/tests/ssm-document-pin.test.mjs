import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {Closed, policy, root, sha256} from '../ci/lib.mjs';
import {AWSPlane} from '../deploy/deploy.mjs';

const expected = JSON.parse(fs.readFileSync(path.join(root, 'scripts/deploy/ssm-document.json'), 'utf8'));
const awsHash = 'b'.repeat(64); // Deliberately not the local source hash.
const blocked = {code: 'SSM_DOCUMENT_NOT_PINNED', state: 'BLOCKED'};

function fixture(t) {
  const responses = {
    'get-document': {Name: policy.aws.ssmDocument, DocumentVersion: policy.aws.ssmVersion,
      Status: 'Active', DocumentType: 'Command', DocumentFormat: 'JSON', Content: JSON.stringify(expected)},
    'describe-document': {Document: {Name: policy.aws.ssmDocument, DocumentVersion: policy.aws.ssmVersion,
      Owner: policy.aws.account, Status: 'Active', DocumentType: 'Command', SchemaVersion: expected.schemaVersion,
      Hash: awsHash, HashType: 'Sha256', DefaultVersion: '9', LatestVersion: '11'}},
  };
  const state = {run: '1234', attempt: '1', nodes: {A: {}, B: {}},
    candidate: {manifestDigest: 'sha256:' + 'c'.repeat(64)}};
  const plane = new AWSPlane(state), calls = [], errors = new Map();
  t.mock.method(plane, 'aws', (service, operation, args) => {
    calls.push({service, operation, args: [...args]});
    assert.equal(service, 'ssm');
    assert.ok(Object.hasOwn(responses, operation), 'Unexpected mocked operation; live AWS is never called');
    if (errors.has(operation)) throw errors.get(operation);
    return structuredClone(responses[operation]);
  });
  return {plane, state, calls, errors, responses, content: responses['get-document'], metadata: responses['describe-document'].Document};
}

test('valid content and DescribeDocument metadata accept only the explicit configured name/version, not default/latest', async t => {
  const f = fixture(t);
  assert.equal(policy.aws.ssmDocument, 'ENVER-Dev-ImageDeployment-v1');
  assert.equal(policy.aws.ssmVersion, '1');
  assert.equal(policy.aws.account, '926925605666');
  assert.equal(policy.aws.region, 'us-east-1');
  assert.equal(expected.schemaVersion, '2.2');
  assert.equal(Object.hasOwn(f.content, 'Hash'), false);
  assert.equal(Object.hasOwn(f.content, 'HashType'), false);
  await f.plane.document();
  const pinned = ['--name', policy.aws.ssmDocument, '--document-version', '1'];
  assert.deepEqual(f.calls, [
    {service: 'ssm', operation: 'get-document', args: [...pinned, '--document-format', 'JSON']},
    {service: 'ssm', operation: 'describe-document', args: pinned},
  ]);
  assert.equal(f.state.documentHash, awsHash);
  assert.notEqual(f.state.documentHash, sha256(JSON.stringify(expected)));
});

const mutations = [
  ['missing Hash', f => { delete f.metadata.Hash; }],
  ['null Hash', f => { f.metadata.Hash = null; }],
  ['short hash', f => { f.metadata.Hash = 'b'.repeat(63); }],
  ['long hash', f => { f.metadata.Hash = 'b'.repeat(65); }],
  ['uppercase hash', f => { f.metadata.Hash = 'B'.repeat(64); }],
  ['non-hex hash', f => { f.metadata.Hash = 'g'.repeat(64); }],
  ['hash with trailing newline', f => { f.metadata.Hash = awsHash + '\n'; }],
  ['non-string hash', f => { f.metadata.Hash = [awsHash]; }],
  ['wrong HashType', f => { f.metadata.HashType = 'Sha1'; }],
  ['missing HashType', f => { delete f.metadata.HashType; }],
  ['GetDocument version differs', f => { f.content.DocumentVersion = '2'; }],
  ['DescribeDocument version differs', f => { f.metadata.DocumentVersion = '2'; }],
  ['both responses agree on an unpinned version', f => { f.content.DocumentVersion = f.metadata.DocumentVersion = '2'; }],
  ['implicit default version', f => { f.content.DocumentVersion = f.metadata.DocumentVersion = '$DEFAULT'; }],
  ['implicit latest version', f => { f.content.DocumentVersion = f.metadata.DocumentVersion = '$LATEST'; }],
  ['missing GetDocument version', f => { delete f.content.DocumentVersion; }],
  ['missing DescribeDocument version', f => { delete f.metadata.DocumentVersion; }],
  ['GetDocument name mismatch', f => { f.content.Name = 'OtherDocument'; }],
  ['DescribeDocument name mismatch', f => { f.metadata.Name = 'OtherDocument'; }],
  ['both responses agree on another name', f => { f.content.Name = f.metadata.Name = 'OtherDocument'; }],
  ['owner/account mismatch', f => { f.metadata.Owner = '000000000000'; }],
  ['missing owner', f => { delete f.metadata.Owner; }],
  ['inactive GetDocument', f => { f.content.Status = 'Updating'; }],
  ['inactive DescribeDocument', f => { f.metadata.Status = 'Failed'; }],
  ['missing GetDocument status', f => { delete f.content.Status; }],
  ['missing DescribeDocument status', f => { delete f.metadata.Status; }],
  ['GetDocument type mismatch', f => { f.content.DocumentType = 'Automation'; }],
  ['DescribeDocument type mismatch', f => { f.metadata.DocumentType = 'Automation'; }],
  ['schema mismatch', f => { f.metadata.SchemaVersion = '1.2'; }],
  ['missing schema', f => { delete f.metadata.SchemaVersion; }],
  ['GetDocument format mismatch', f => { f.content.DocumentFormat = 'YAML'; }],
  ['missing content', f => { delete f.content.Content; }],
  ['non-string content', f => { f.content.Content = expected; }],
  ['content mismatch', f => { f.content.Content = JSON.stringify({...expected, description: 'different content'}); }],
  ['embedded command differs', f => {
    const changed = structuredClone(expected); changed.mainSteps[0].inputs.runCommand[0] += '\n# different command';
    f.content.Content = JSON.stringify(changed);
  }],
  ['content schema differs despite valid metadata', f => { f.content.Content = JSON.stringify({...expected, schemaVersion: '1.2'}); }],
  ['content parameters differ', f => { f.content.Content = JSON.stringify({...expected, parameters: {}}); }],
  ['null GetDocument response', f => { f.responses['get-document'] = null; }],
  ['unexpected GetDocument response', f => { f.responses['get-document'] = []; }],
  ['null DescribeDocument response', f => { f.responses['describe-document'] = null; }],
  ['missing Document metadata envelope', f => { f.responses['describe-document'] = {}; }],
  ['null Document metadata', f => { f.responses['describe-document'].Document = null; }],
  ['hash incorrectly supplied only by GetDocument', f => {
    f.content.Hash = awsHash; f.content.HashType = 'Sha256'; delete f.metadata.Hash;
  }],
];
for (const [name, mutate] of mutations) test(`${name}: BLOCKED, no documentHash retained and no execution`, async t => {
  const f = fixture(t); mutate(f);
  await assert.rejects(() => f.plane.document(), blocked);
  assert.equal(Object.hasOwn(f.state, 'documentHash'), false);
  assert.deepEqual(f.calls.map(c => c.operation), ['get-document', 'describe-document']);
});

test('malformed JSON content propagates failure without retaining a document hash', async t => {
  const f = fixture(t); f.content.Content = '{invalid json';
  await assert.rejects(() => f.plane.document(), SyntaxError);
  assert.equal(Object.hasOwn(f.state, 'documentHash'), false);
});

test('canonical equality allows JSON formatting/key order, but not changed strings or commands', async t => {
  const f = fixture(t);
  f.content.Content = JSON.stringify(Object.fromEntries(Object.entries(expected).reverse()), null, 4) + '\n';
  await f.plane.document();
  assert.equal(f.state.documentHash, awsHash);
});

for (const operation of ['get-document', 'describe-document']) {
  for (const error of [new Closed('SYNTHETIC_AWS_READ_ERROR', 'BLOCKED'), new Error('unexpected synthetic AWS error')])
    test(`${operation} ${error.constructor.name} propagates unchanged, with no hash or execution`, async t => {
      const f = fixture(t); f.errors.set(operation, error);
      await assert.rejects(() => f.plane.document(), actual => actual === error);
      assert.equal(Object.hasOwn(f.state, 'documentHash'), false);
      assert.deepEqual(f.calls.map(c => c.operation), operation === 'get-document' ? ['get-document'] : ['get-document', 'describe-document']);
    });
}

test('DescribeDocument hash survives state serialization and is used unchanged by the later mocked SSM execution path', async t => {
  const f = fixture(t); await f.plane.document();
  const persisted = JSON.parse(JSON.stringify(f.state));
  assert.equal(persisted.documentHash, awsHash);
  const plane = new AWSPlane(persisted), calls = [];
  // A fully mocked transport/reconcile: no AWS CLI, SSM command or remote operation occurs.
  t.mock.method(plane, 'aws', (service, operation, args) => {
    calls.push({service, operation, args});
    return {Command: {CommandId: '00000000-0000-0000-0000-000000000001'}};
  });
  t.mock.method(plane, 'reconcile', async () => ({Status: 'Success', ResponseCode: 0, StandardErrorContent: '',
    StandardOutputContent: JSON.stringify({schema: 1, instance: policy.aws.instances.A, operation: 'snapshot', status: 'PASS'})}));
  await plane.remote('A', 'snapshot');
  assert.equal(calls.length, 1);
  assert.equal(calls[0].service, 'ssm'); assert.equal(calls[0].operation, 'send-command');
  const value = flag => calls[0].args[calls[0].args.indexOf(flag) + 1];
  assert.equal(value('--document-name'), policy.aws.ssmDocument);
  assert.equal(value('--document-version'), policy.aws.ssmVersion);
  assert.equal(value('--document-hash'), awsHash);
  assert.equal(value('--document-hash-type'), 'Sha256');
});
