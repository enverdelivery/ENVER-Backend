"""Prueba LOCAL Linux, aislada por mount/network namespace; SOLO fixture sintético.

Ejecutar como root: unshare --mount --net --fork python3 <este archivo>
No lee el runtime real. No inicia API, Docker, AWS ni conexiones a dependencias.
"""
import json
import os
import pathlib
import stat
import subprocess

RUNTIME = '/run/enver/appsettings.Runtime.json'
def command(*args):
    subprocess.run(args, check=True, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

assert os.geteuid() == 0
command('mount', '--make-rprivate', '/')
# Montar encima de /run SOLO dentro del namespace privado; no tocar contenido del host.
command('mount', '-t', 'tmpfs', '-o', 'size=1m,nosuid,nodev,noexec,mode=0755', 'tmpfs', '/run')
os.mkdir('/run/enver', 0o711)
fd = os.open(RUNTIME, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600)
os.write(fd, b'{"fixture":"synthetic-local-only"}')
os.fchown(fd, 1654, 1654)
os.close(fd)
command('mount', '--bind', '/run/enver', '/run/enver')
command('mount', '-o', 'remount,bind,ro,nosuid,nodev,noexec', '/run/enver')
mode = os.stat(RUNTIME)
assert stat.S_ISREG(mode.st_mode) and stat.S_IMODE(mode.st_mode) == 0o600 and mode.st_uid == mode.st_gid == 1654
os.setgroups([])
os.setgid(1654)
os.setuid(1654)
with open(RUNTIME, 'rb') as stream:
    assert json.load(stream) == {'fixture': 'synthetic-local-only'}
try:
    fd = os.open(RUNTIME, os.O_WRONLY)  # Sin truncate/write; comprobar rechazo del mount RO.
except OSError as error:
    import errno
    assert error.errno == errno.EROFS
else:
    os.close(fd)
    raise AssertionError('Write unexpectedly allowed')
print(json.dumps({'localLinuxFixture': 'PASS', 'path': RUNTIME, 'mode': '0600', 'ownerGroup': '1654:1654',
                  'processUidGid': '1654:1654', 'read': 'PASS', 'writeOpen': 'DENIED_READ_ONLY_FS',
                  'realRuntimeAccess': False, 'apiStarted': False, 'networkNamespace': 'ISOLATED'}))
