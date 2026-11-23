import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const load = name => JSON.parse(fs.readFileSync(path.join(root, 'scripts/deploy/iam', name + '.json'), 'utf8'));
const provider = 'arn:aws:iam::926925605666:oidc-provider/token.actions.githubusercontent.com';
const repository = 'arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api';
const prefix = 'repo:enverdelivery@350055774/ENVER-Backend@1445184810:environment:development:workflow_ref:enverdelivery/ENVER-Backend/.github/workflows/';
const subjects = kind => [].concat(load(kind + '-trust').Statement[0].Condition.StringEquals['token.actions.githubusercontent.com:sub']);

test('OIDC exact immutable subjects separate publisher and manual deploy, never PR or fork', () => {
  assert.deepEqual(load('github-oidc-subject'), {use_default: false, include_claim_keys: ['repo', 'context', 'workflow_ref', 'event_name']});
  for (const kind of ['publisher', 'deploy']) {
    const trust = load(kind + '-trust');
    assert.equal(trust.Statement.length, 1);
    assert.equal(trust.Statement[0].Effect, 'Allow');
    assert.deepEqual(trust.Statement[0].Principal, {Federated: provider});
    assert.equal(trust.Statement[0].Action, 'sts:AssumeRoleWithWebIdentity');
    assert.deepEqual(Object.keys(trust.Statement[0].Condition), ['StringEquals']);
    assert.equal(trust.Statement[0].Condition.StringEquals['token.actions.githubusercontent.com:aud'], 'sts.amazonaws.com');
    assert.doesNotMatch(JSON.stringify(trust), /\*/);
  }
  assert.deepEqual(subjects('publisher'), ['push', 'workflow_dispatch'].map(event => prefix + 'hardening.yml@refs/heads/develop:event_name:' + event));
  assert.deepEqual(subjects('deploy'), [prefix + 'deploy-dev.yml@refs/heads/develop:event_name:workflow_dispatch']);
  const good = subjects('deploy')[0];
  for (const wrong of [good.replace('enverdelivery@350055774', 'fork@1'), good.replace('1445184810', '1'), good.replace(':development:', ':production:'), good.replace('heads/develop', 'heads/main'), good.replace('heads/develop', 'pull/1/merge'), good.replace('deploy-dev.yml', 'ci.yml'), good.replace(':workflow_dispatch', ':pull_request'), good.replace(':workflow_dispatch', ':pull_request_target')]) {
    assert.equal([...subjects('deploy'), ...subjects('publisher')].includes(wrong), false);
  }
});

test('OIDC publisher permissions are exactly regional ECR auth and one repository publication', () => {
  const p = load('publisher-permissions');
  assert.equal(p.Statement.length, 2);
  const [auth, repo] = p.Statement;
  assert.equal(auth.Action, 'ecr:GetAuthorizationToken'); assert.equal(auth.Resource, '*');
  assert.deepEqual(auth.Condition, {StringEquals: {'aws:RequestedRegion': 'us-east-1'}});
  assert.equal(repo.Resource, repository);
  assert.deepEqual([...repo.Action].sort(), ['ecr:BatchCheckLayerAvailability', 'ecr:GetDownloadUrlForLayer', 'ecr:BatchGetImage', 'ecr:InitiateLayerUpload', 'ecr:UploadLayerPart', 'ecr:CompleteLayerUpload', 'ecr:PutImage', 'ecr:DescribeImages'].sort());
  assert.ok(p.Statement.every(s => s.Effect === 'Allow' && !s.NotAction && !s.NotResource));
});

test('OIDC deploy permissions permit only fixed SSM document on two hosts plus necessary reads', () => {
  const p = load('deploy-permissions');
  assert.equal(p.Statement.length, 5);
  assert.deepEqual(p.Statement.flatMap(s => s.Action).sort(), ['ecr:DescribeImages', 'ssm:SendCommand', 'ssm:GetDocument', 'ssm:GetCommandInvocation', 'elasticloadbalancing:DescribeTargetHealth'].sort());
  assert.ok(p.Statement.every(s => s.Effect === 'Allow' && !s.NotAction && !s.NotResource));
  assert.equal(p.Statement[0].Resource, repository);
  const doc = 'arn:aws:ssm:us-east-1:926925605666:document/ENVER-Dev-ImageDeployment-v1';
  assert.deepEqual(p.Statement[1].Resource, [doc, ...['i-0a66085b7b2411393', 'i-00de11a3f3c1a8aa5'].map(id => 'arn:aws:ec2:us-east-1:926925605666:instance/' + id)]);
  assert.equal(p.Statement[2].Resource, doc);
  for (const s of p.Statement.slice(3)) { assert.equal(s.Resource, '*'); assert.deepEqual(s.Condition, {StringEquals: {'aws:RequestedRegion': 'us-east-1'}}); }
});

test('OIDC workflows reference real dedicated roles while CI stays AWS-free and flags remain closed by default', () => {
  const text = file => fs.readFileSync(path.join(root, file), 'utf8');
  const ci = text('.github/workflows/ci.yml'), hardening = text('.github/workflows/hardening.yml'), deploy = text('.github/workflows/deploy-dev.yml');
  assert.doesNotMatch(ci, /id-token:|configure-aws-credentials|AWS_ACCESS_KEY_ID|AWS_SECRET_ACCESS_KEY/);
  assert.match(hardening, /ENVER_PUBLISHER_ROLE: arn:aws:iam::926925605666:role\/enver-github-ecr-publisher/);
  assert.match(hardening, /role-to-assume: \$\{\{ env.ENVER_PUBLISHER_ROLE \}\}/);
  assert.match(deploy, /ENVER_DEPLOY_ROLE: arn:aws:iam::926925605666:role\/enver-github-deploy-dev/);
  assert.match(hardening, /ENVER_PUBLISH_ENABLED: \$\{\{ vars.ENVER_PUBLISH_ENABLED \}\}/);
  assert.match(deploy, /ENVER_DEPLOY_ENABLED: \$\{\{ vars.ENVER_DEPLOY_ENABLED \}\}/);
  assert.match(text('scripts/ci/hardening.mjs'), /role\/enver-github-ecr-publisher/);
  assert.match(text('scripts/deploy/deploy.mjs'), /role\/enver-github-deploy-dev/);
});
