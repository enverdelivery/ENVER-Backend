import fs from 'node:fs';
import path from 'node:path';
import {root, save} from '../ci/lib.mjs';
// Local deterministic generation only. Never creates/updates an AWS document.
const agent = fs.readFileSync(path.join(root, 'scripts/deploy/remote_agent.py'), 'utf8').replaceAll('\r\n', '\n');
const document = {
  schemaVersion: '2.2', description: 'ENVER fixed image-only DEV operations; no arbitrary shell, infrastructure, secrets or initialize.',
  parameters: {
    Operation: {type: 'String', allowedValues: ['snapshot', 'deploy', 'verify', 'restore', 'finalize'], interpolationType: 'ENV_VAR'},
    DesiredDigest: {type: 'String', allowedPattern: '^sha256:[a-f0-9]{64}$', interpolationType: 'ENV_VAR'},
    PreviousDigest: {type: 'String', allowedPattern: '^sha256:[a-f0-9]{64}$', interpolationType: 'ENV_VAR'},
    DeploymentId: {type: 'String', allowedPattern: '^[0-9]{1,20}-[0-9]{1,4}$', interpolationType: 'ENV_VAR'},
  },
  mainSteps: [{action: 'aws:runShellScript', name: 'EnverFixedImageOperation', inputs: {
    timeoutSeconds: '600', runCommand: ["set -eu\numask 077\nulimit -c 0\nexec /usr/bin/python3 - <<'ENVER_FIXED_AGENT'\n" + agent + '\nENVER_FIXED_AGENT'],
  }}],
};
save(path.join(root, 'scripts/deploy/ssm-document.json'), document);
