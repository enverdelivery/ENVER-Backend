"""Supervisor de DEPLOY-CONFIG-03. Se ejecuta solo en A tras transferir código no secreto.

SETTINGS se inyecta con manifest/artefactos públicos, nunca payloads de Secrets Manager.
No importa ni ejecuta Bootstrap o Enver.API; solo el helper y su modo de binding.
"""
import base64
import hashlib
import json
import os
import resource
import shutil
import stat
import subprocess
import sys
import zlib

BASE = '/opt/enver/bootstrap/24c2eaad518f2586/publish'
IMAGE = 'mcr.microsoft.com/dotnet/aspnet@sha256:bfd912c83295ce66d4b706ebb8541b9f14259b0e76fbb88b2751f58ce2399d2e'
RUNTIME = '/run/enver/appsettings.Runtime.json'
ENV = {'PATH': '/usr/local/bin:/usr/bin:/bin:/usr/local/sbin:/usr/sbin:/sbin', 'HOME': '/nonexistent',
       'LANG': 'C.UTF-8', 'AWS_CONFIG_FILE': '/dev/null', 'AWS_SHARED_CREDENTIALS_FILE': '/dev/null',
       'AWS_EC2_METADATA_DISABLED': 'false', 'AWS_EC2_METADATA_V1_DISABLED': 'true',
       'AWS_REGION': 'us-east-1', 'AWS_DEFAULT_REGION': 'us-east-1', 'AWS_MAX_ATTEMPTS': '1',
       'DOTNET_EnableDiagnostics': '0', 'DOTNET_CLI_TELEMETRY_OPTOUT': '1'}
REPORT = dict(event='FINAL', status='FAIL', deployConfig03='OPEN', deployConfig04='CLOSED',
              apiStarted=False, initializeHistoricalTotal=1, gStatus='APPLIED', nextGate='BLOCKED',
              secretValuesLogged=False, secretValuesInEnv=False, secretValuesInArgv=False,
              secretValuesInImage=False, redisWrites=0, dynamoWrites=0)
class Stop(Exception):
    pass
def require(value, code):
    if not value: raise Stop(code)
def run(args, timeout=30):
    return subprocess.run(args, env=ENV, capture_output=True, text=True, timeout=timeout)
def digest(path):
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for block in iter(lambda: f.read(1048576), b''): h.update(block)
    return h.hexdigest()
def safe_json(result):
    require(not result.stderr, 'CHILD_STDERR_SUPPRESSED')
    try: value = json.loads(result.stdout)
    except Exception: raise Stop('CHILD_OUTPUT_SUPPRESSED')
    require(isinstance(value, dict) and value.get('status') == 'PASS' and result.returncode == 0, 'CHILD_FAILURE_SUPPRESSED')
    # Only fixed output field values, never arbitrary application/SDK/exception output.
    for key, value in value.items():
        require(value is True or value is False or value in ('PASS', '0600', '1654:1654'), 'CHILD_OUTPUT_NOT_ALLOWLISTED')
    return value
def service_inactive():
    value = run(['systemctl', 'is-active', 'enver-api.service'])
    return value.stdout.strip() in ('inactive', 'unknown', 'failed')

resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
os.umask(0o077)
container = None
made_runtime = False
stage = 'prechecks'
try:
    require(os.geteuid() == 0 and os.uname().machine == 'x86_64', 'HOST_MISMATCH')
    require(service_inactive(), 'API_SERVICE_NOT_INACTIVE')
    require(not os.path.lexists(RUNTIME), 'EXISTING_RUNTIME_FILE_STOP')
    require(not os.path.lexists('/etc/systemd/system/enver-api.service'), 'EXISTING_SERVICE_FILE_STOP')
    require(not os.path.lexists('/etc/enver/api-start.authorized'), 'UNEXPECTED_API_START_AUTHORIZATION')
    require(not os.path.lexists('/etc/enver/api-image.env'), 'UNEXPECTED_IMAGE_AUTHORIZATION')
    require(not os.path.lexists('/opt/enver/runtime-config/current'), 'EXISTING_RUNTIME_LINK_STOP')
    with open('/proc/swaps') as f: require(not any(line.strip() for line in list(f)[1:]), 'SWAP_PRESENT')
    docker = shutil.which('docker', path=ENV['PATH'])
    require(docker is not None, 'DOCKER_UNAVAILABLE')
    active = run([docker, 'ps', '-q'])
    require(active.returncode == 0 and not active.stdout.strip(), 'UNEXPECTED_ACTIVE_CONTAINERS')
    stage = 'artifact verification and installation (nonsecret only)'
    directory = SETTINGS['directory']
    require(directory.startswith('/opt/enver/runtime-config/') and os.path.realpath(directory) == directory, 'ARTIFACT_DIRECTORY_INVALID')
    packed = b''.join(open(directory + '/part-' + str(i), 'rb').read() for i in range(SETTINGS['parts']))
    require(hashlib.sha256(packed).hexdigest() == SETTINGS['packedSha256'], 'TRANSFER_HASH_MISMATCH')
    bundle = json.loads(zlib.decompress(base64.b64decode(packed, validate=True)))
    require(bundle['directory'] == directory, 'BUNDLE_DIRECTORY_MISMATCH')
    manifest = bundle['files']
    for item in manifest:
        relative = item['path']
        require(not relative.startswith('/') and '..' not in relative.split('/'), 'ARTIFACT_PATH_INVALID')
        target = directory + '/' + relative
        require(not os.path.lexists(target), 'ARTIFACT_FILE_EXISTS')
        os.makedirs(os.path.dirname(target), mode=0o755, exist_ok=True)
        os.chmod(os.path.dirname(target), 0o755)
        if 'body' in item:
            body = base64.b64decode(item['body'], validate=True)
            require(hashlib.sha256(body).hexdigest() == item['sha256'], 'NEW_FILE_HASH_MISMATCH')
            with open(target, 'xb') as f: f.write(body)
        else:
            require(digest(BASE + '/' + relative) == item['sha256'], 'BASELINE_FILE_HASH_MISMATCH')
            shutil.copyfile(BASE + '/' + relative, target)
        require(digest(target) == item['sha256'], 'INSTALLED_FILE_HASH_MISMATCH')
        os.chmod(target, 0o755 if relative in ('Enver.RuntimeConfig.Materializer', 'materialize.sh', 'start-api.sh') else 0o644)
    REPORT['artifactVerified'] = True
    REPORT['artifactDirectory'] = directory
    REPORT['artifactManifestSha256'] = bundle['manifestSha256']
    stage = 'first real pinned SDK materialization'
    made_runtime = True
    first = safe_json(run([directory + '/Enver.RuntimeConfig.Materializer', '--materialize'], timeout=110))
    REPORT['materializer'] = first
    require(first.get('pins') is True and first.get('instanceRole') is True and first.get('binding') is True, 'MATERIALIZER_NOT_READY')
    initial = os.stat(RUNTIME, follow_symlinks=False)
    require(stat.S_ISREG(initial.st_mode) and stat.S_IMODE(initial.st_mode) == 0o600 and initial.st_uid == initial.st_gid == 1654, 'RUNTIME_PERMISSIONS_INVALID')
    mount = run(['findmnt', '-n', '-o', 'FSTYPE', '-T', RUNTIME])
    require(mount.returncode == 0 and mount.stdout.strip() == 'tmpfs', 'RUNTIME_NOT_TMPFS')
    REPORT.update(tmpfs=True, path=RUNTIME, permissions='0600', ownerGroup='1654:1654')
    stage = 'restart rematerialization without API'
    # Hold the old inode open so reuse is impossible; never read or hash its contents.
    with open(RUNTIME, 'rb') as held:
        second = safe_json(run([directory + '/Enver.RuntimeConfig.Materializer', '--materialize'], timeout=110))
        refreshed = os.stat(RUNTIME, follow_symlinks=False)
        require(refreshed.st_ino != os.fstat(held.fileno()).st_ino, 'RUNTIME_NOT_REPLACED')
    require(second == first, 'REMATERIALIZATION_VALIDATION_MISMATCH')
    REPORT.update(materializerExecutions=2, restartPinnedRematerialization=True, atomicReplacement=True)
    stage = 'network-isolated non-API binding and read-only mount diagnostic'
    command = [docker, 'create', '--name', 'enver-runtime03-binding', '--network', 'none', '--read-only',
               '--cap-drop', 'ALL', '--security-opt', 'no-new-privileges', '--user', '1654:1654',
               '--ulimit', 'core=0:0', '--log-driver', 'none', '--pids-limit', '64', '--memory', '256m',
               '--tmpfs', '/tmp:rw,nosuid,nodev,noexec,size=16m,mode=1777',
               '--mount', 'type=bind,src=' + directory + ',dst=/probe,readonly',
               '--mount', 'type=bind,src=' + RUNTIME + ',dst=' + RUNTIME + ',readonly',
               '--env', 'DOTNET_EnableDiagnostics=0', '--env', 'HOME=/nonexistent',
               '--entrypoint', 'dotnet', IMAGE, '/probe/Enver.RuntimeConfig.Materializer.dll', '--validate-runtime']
    created = run(command)
    require(created.returncode == 0, 'DIAGNOSTIC_CREATE_FAILED')
    container = created.stdout.strip()
    inspected = run([docker, 'inspect', container])
    require(inspected.returncode == 0, 'DIAGNOSTIC_INSPECT_FAILED')
    actual = json.loads(inspected.stdout)[0]
    require(actual['HostConfig']['NetworkMode'] == 'none' and not actual['HostConfig'].get('PortBindings'), 'DIAGNOSTIC_NETWORK_MISMATCH')
    require(actual['Config']['Cmd'] == ['/probe/Enver.RuntimeConfig.Materializer.dll', '--validate-runtime'], 'DIAGNOSTIC_ENTRYPOINT_MISMATCH')
    require(actual['HostConfig']['LogConfig']['Type'] == 'none', 'DIAGNOSTIC_LOG_DRIVER_MISMATCH')
    runtime_mount = [m for m in actual['Mounts'] if m['Destination'] == RUNTIME]
    require(len(runtime_mount) == 1 and runtime_mount[0]['Source'] == RUNTIME and not runtime_mount[0]['RW'], 'MOUNT_NOT_READ_ONLY')
    diagnostic = safe_json(run([docker, 'start', '--attach', container], timeout=60))
    require(diagnostic.get('binding') is True and diagnostic.get('jsonParse') is True and diagnostic.get('requiredSections') is True and diagnostic.get('externalConnections') is False, 'BINDING_FAILED')
    removed = run([docker, 'rm', container])
    require(removed.returncode == 0, 'DIAGNOSTIC_REMOVE_FAILED')
    container = None
    REPORT.update(containerParse='PASS', binding='PASS', runtimeMount='READ-ONLY', diagnosticNetwork='none', diagnosticContainerRemoved=True)
    stage = 'systemd preparation without activation'
    unit = '/etc/systemd/system/enver-api.service'
    for script in ('materialize.sh', 'start-api.sh'):
        require(run(['/bin/sh', '-n', directory + '/' + script]).returncode == 0, 'SHELL_SYNTAX_INVALID')
    os.symlink(directory, '/opt/enver/runtime-config/current')
    shutil.copyfile(directory + '/enver-api.service', unit)
    os.chmod(unit, 0o644)
    checked = run(['systemd-analyze', 'verify', unit])
    require(checked.returncode == 0, 'SYSTEMD_VERIFY_FAILED')
    require(run(['systemctl', 'daemon-reload']).returncode == 0, 'SYSTEMD_RELOAD_FAILED')
    require(service_inactive(), 'API_SERVICE_NOT_INACTIVE_POSTCHECK')
    enabled = run(['systemctl', 'is-enabled', 'enver-api.service'])
    require(enabled.stdout.strip() == 'disabled', 'API_SERVICE_UNEXPECTEDLY_ENABLED')
    require(not os.path.lexists('/etc/enver/api-start.authorized'), 'START_GATE_NOT_CLOSED')
    active = run([docker, 'ps', '-q'])
    require(active.returncode == 0 and not active.stdout.strip(), 'UNEXPECTED_ACTIVE_CONTAINERS_POSTCHECK')
    REPORT.update(systemdUnit='PREPARED', systemdVerified=True, serviceActive=False, serviceEnabled=False,
                  startupAuthorizationAbsent=True, configPrecedence='CORRECT', configConsumedByIConfiguration=True,
                  sections=['Redis', 'MongoDb', 'RateLimiting', 'Jwt', 'AuthenticationSecurityAuthority', 'AuthenticationComposition'],
                  status='PASS', deployConfig03='CLOSED', nextGate='EC2 A API CONTAINER START AUTHORIZATION')
except Exception as error:
    REPORT.update(stage=stage, error=str(error) if isinstance(error, Stop) else type(error).__name__)
finally:
    if container is not None:
        try:
            run([docker, 'stop', '--time', '2', container], timeout=15)
            REPORT['diagnosticContainerRemoved'] = run([docker, 'rm', container]).returncode == 0
        except Exception: REPORT['diagnosticContainerRemoved'] = False
    if REPORT['status'] != 'PASS' and made_runtime and os.path.isfile(RUNTIME):
        try: os.unlink(RUNTIME); REPORT['failedRuntimeRemoved'] = True
        except Exception: REPORT['failedRuntimeRemoved'] = False
    print(json.dumps(REPORT), flush=True)
sys.exit(0 if REPORT['status'] == 'PASS' else 1)
