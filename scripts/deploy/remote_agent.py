"""Fixed SSM document payload. No remote installation/execution is performed by this gate."""
import contextlib
import datetime
import fcntl
import hashlib
import hmac
import json
import os
import re
import stat
import subprocess
import tempfile
import time
import urllib.request

ACCOUNT = "926925605666"
REGION = "us-east-1"
REGISTRY = ACCOUNT + ".dkr.ecr." + REGION + ".amazonaws.com"
REPOSITORY = REGISTRY + "/enver-dev-api"
GENERATION = "73041e6288899e1528f3b8f25c39e888"
HOSTS = {"i-0a66085b7b2411393": "10.20.0.63", "i-00de11a3f3c1a8aa5": "10.20.1.99"}
REFERENCE = "/etc/enver/api-image.env"
RUNTIME = "/run/enver/appsettings.Runtime.json"
STATE = "/var/lib/enver-cicd"


def check(ok):
    if not ok:
        raise RuntimeError("REMOTE_CONTRACT_FAILED")


def run(args, data=None, timeout=60):
    result = subprocess.run(args, input=data, stdout=subprocess.PIPE, stderr=subprocess.PIPE, timeout=timeout, check=False)
    check(result.returncode == 0)
    return result.stdout


def canonical(value):
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode()


def protected(path, uid=0, mode=None):
    info = os.lstat(path)
    check(stat.S_ISREG(info.st_mode) and info.st_uid == uid)
    check(mode is None or stat.S_IMODE(info.st_mode) == mode)
    return info


def read(path):
    with open(path, "rb") as source:
        return source.read(1024 * 1024)


def atomic(path, data, mode=0o600, gid=0):
    # No wildcard cleanup; replace only this dedicated file, preserving the image-env contract.
    descriptor, temporary = tempfile.mkstemp(prefix=".enver-cicd-", dir=os.path.dirname(path))
    try:
        os.fchmod(descriptor, mode)
        os.fchown(descriptor, 0, gid)
        with os.fdopen(descriptor, "wb") as target:
            target.write(data)
            target.flush()
            os.fsync(target.fileno())
        os.replace(temporary, path)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)


def metadata():
    token_request = urllib.request.Request("http://169.254.169.254/latest/api/token", method="PUT", headers={"X-aws-ec2-metadata-token-ttl-seconds": "600"})
    with urllib.request.urlopen(token_request, timeout=3) as response:
        token = response.read().decode()

    def get(item):
        request = urllib.request.Request("http://169.254.169.254/latest/meta-data/" + item, headers={"X-aws-ec2-metadata-token": token})
        with urllib.request.urlopen(request, timeout=3) as response:
            return response.read().decode()
    instance = get("instance-id")
    check(instance in HOSTS and get("local-ipv4") == HOSTS[instance])
    check(get("iam/security-credentials/").strip() == "enver-dev-api-runtime")
    return instance, get


def reference():
    protected(REFERENCE)
    match = re.fullmatch(rb"ENVER_API_IMAGE=" + re.escape(REPOSITORY.encode()) + rb"@(sha256:[a-f0-9]{64})\r?\n?", read(REFERENCE))
    check(match is not None)
    return match.group(1).decode()


def configuration():
    protected(RUNTIME, 1654, 0o600)
    data = read(RUNTIME)
    config = json.loads(data)
    transport = config["AuthenticationTransport"]
    check(transport.get("AllowLocalHttp") is False and transport.get("DirectConnectionApproved", False) is False)
    front = transport["CloudFront"]
    check(front.get("Enabled") is True and 32 <= len(front.get("OriginSecret", "")) <= 256)
    check(config["AuthenticationComposition"]["QuotaGeneration"] == GENERATION)
    check(config["AuthenticationSecurityAuthority"]["TableName"] == "enver-dev-authentication-authority")
    protected("/etc/enver/cloudfront-origin.json", 0, 0o600)
    protected("/etc/enver/cloudfront-runtime-overlay.py", 0, 0o700)
    return hashlib.sha256(canonical(config)).hexdigest()


def fingerprints():
    values = {"configuration": configuration()}
    for option in ("ExecStart", "ExecStartPre"):
        text = run(["systemctl", "show", "enver-api.service", "--property=" + option, "--value"]).decode()
        paths = re.findall(r"path=([^ ;}]+)", text)
        check(paths)
        for file in paths:
            protected(file)
            values[file] = hashlib.sha256(read(file)).hexdigest()
    for file in ("/etc/enver/cloudfront-origin.json", "/etc/enver/cloudfront-runtime-overlay.py"):
        values[file] = hashlib.sha256(read(file)).hexdigest()
    values["unit"] = hashlib.sha256(run(["systemctl", "cat", "enver-api.service"])).hexdigest()
    return values


def health(url):
    # Local fixed HTTP only, never credentials. CloudFront TLS health is checked by the caller.
    check(url in ["http://127.0.0.1:8080/health"] + ["http://" + host + ":8080/health" for host in HOSTS.values()])
    with urllib.request.urlopen(url, timeout=3) as response:
        check(response.status == 200 and response.read(32) == b"Healthy")


def ready(instance, expected):
    check(run(["systemctl", "is-active", "enver-api.service"]).strip() == b"active")
    for option, value in (("Restart", b"no"), ("UnitFileState", b"disabled"), ("StandardOutput", b"journal"), ("StandardError", b"journal"), ("LimitCORE", b"0")):
        check(run(["systemctl", "show", "enver-api.service", "--property=" + option, "--value"]).strip() == value)
    container = json.loads(run(["docker", "inspect", "enver-api"]))[0]
    check(container["State"]["Running"] is True and container["Config"]["Image"] == REPOSITORY + "@" + expected)
    check(container["Config"]["User"] == "1654:1654")
    check(container["HostConfig"]["ReadonlyRootfs"] is True and container["HostConfig"]["Privileged"] is False)
    check("ALL" in container["HostConfig"]["CapDrop"] and any(s.startswith("no-new-privileges") for s in container["HostConfig"]["SecurityOpt"]))
    check(run(["findmnt", "--noheadings", "--output", "FSTYPE", "--target", RUNTIME]).strip() == b"tmpfs")
    ports = container["NetworkSettings"]["Ports"].get("8080/tcp") or []
    check(any(p.get("HostIp") == "0.0.0.0" and p.get("HostPort") == "8080" for p in ports))
    mounts = [m for m in container["Mounts"] if m.get("Destination") == RUNTIME]
    check(len(mounts) == 1 and mounts[0]["RW"] is False and mounts[0]["Source"] == RUNTIME)
    check(run(["docker", "exec", "enver-api", "cat", RUNTIME]) == read(RUNTIME))
    image = json.loads(run(["docker", "image", "inspect", REPOSITORY + "@" + expected]))[0]
    check(REPOSITORY + "@" + expected in image["RepoDigests"] and container["Image"] == image["Id"])
    check(image["Architecture"] == "amd64" and image["Os"] == "linux")
    check(reference() == expected)
    configuration()
    health("http://127.0.0.1:8080/health")
    health("http://" + HOSTS[instance] + ":8080/health")


def wait_ready(instance, expected):
    for attempt in range(12):
        try:
            ready(instance, expected)
            return
        except Exception:
            if attempt == 11:
                raise RuntimeError("REMOTE_READINESS_FAILED") from None
            time.sleep(5)


def pull(expected, get):
    # IMDSv2 credentials of the unchanged runtime role; token/password stay in RAM/tmpfs.
    credentials = json.loads(get("iam/security-credentials/enver-dev-api-runtime"))
    body = canonical({"registryIds": [ACCOUNT]})
    now = datetime.datetime.now(datetime.timezone.utc)
    stamp, date = now.strftime("%Y%m%dT%H%M%SZ"), now.strftime("%Y%m%d")
    host = "api.ecr." + REGION + ".amazonaws.com"
    headers = {"content-type": "application/x-amz-json-1.1", "host": host, "x-amz-date": stamp,
               "x-amz-security-token": credentials["Token"], "x-amz-target": "AmazonEC2ContainerRegistry_V20150921.GetAuthorizationToken"}
    names = ";".join(sorted(headers))
    request = "POST\n/\n\n" + "".join(key + ":" + headers[key] + "\n" for key in sorted(headers)) + "\n" + names + "\n" + hashlib.sha256(body).hexdigest()
    scope = date + "/" + REGION + "/ecr/aws4_request"
    to_sign = "AWS4-HMAC-SHA256\n" + stamp + "\n" + scope + "\n" + hashlib.sha256(request.encode()).hexdigest()
    key = ("AWS4" + credentials["SecretAccessKey"]).encode()
    for item in (date, REGION, "ecr", "aws4_request"):
        key = hmac.new(key, item.encode(), hashlib.sha256).digest()
    signature = hmac.new(key, to_sign.encode(), hashlib.sha256).hexdigest()
    headers["Authorization"] = "AWS4-HMAC-SHA256 Credential=" + credentials["AccessKeyId"] + "/" + scope + ", SignedHeaders=" + names + ", Signature=" + signature
    request = urllib.request.Request("https://" + host + "/", data=body, headers=headers, method="POST")
    with urllib.request.urlopen(request, timeout=15) as response:
        payload = json.loads(response.read())
    import base64
    authorization = payload["authorizationData"][0]
    check(authorization["proxyEndpoint"] == "https://" + REGISTRY)
    username, password = base64.b64decode(authorization["authorizationToken"]).split(b":", 1)
    check(username == b"AWS")
    with tempfile.TemporaryDirectory(prefix="enver-cicd-pull-", dir="/run") as directory:
        run(["docker", "--config", directory, "login", "--username", "AWS", "--password-stdin", REGISTRY], data=password)
        run(["docker", "--config", directory, "pull", "--platform", "linux/amd64", REPOSITORY + "@" + expected], timeout=240)


def main():
    check(os.geteuid() == 0)
    operation = os.environ.get("SSM_Operation", "")
    desired = os.environ.get("SSM_DesiredDigest", "")
    previous = os.environ.get("SSM_PreviousDigest", "")
    deployment = os.environ.get("SSM_DeploymentId", "")
    check(operation in ("snapshot", "deploy", "restore", "verify", "finalize"))
    check(re.fullmatch(r"sha256:[a-f0-9]{64}", desired) and re.fullmatch(r"sha256:[a-f0-9]{64}", previous))
    check(re.fullmatch(r"[0-9]{1,20}-[0-9]{1,4}", deployment))
    instance, get = metadata()
    with open("/run/lock/enver-cicd.lock", "a") as lock:
        fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        current = reference()
        record_path = STATE + "/active.json"
        record = json.loads(read(record_path)) if os.path.exists(record_path) else None
        if operation == "snapshot":
            ready(instance, current)
            return {"digest": current, "recoveryRequired": bool(record and record["status"] not in ("committed", "restored"))}, instance, operation
        if operation == "verify":
            wait_ready(instance, desired)
            if record and record.get("deployment") == deployment:
                check(fingerprints() == record["fingerprints"])
            return {"digest": desired}, instance, operation
        if operation == "restore" and current == desired == previous and (record is None or record.get("deployment") != deployment and record.get("status") in ("committed", "restored")):
            # A forward operation that failed before journaling made no image change.
            # Validate the known previous image; do not manufacture a restart/journal.
            wait_ready(instance, desired)
            return {"digest": desired}, instance, operation
        if operation == "deploy":
            check(current == previous)
            check(record is None or record["status"] in ("committed", "restored"))
            os.makedirs(STATE, mode=0o700, exist_ok=True)
            check(os.lstat(STATE).st_uid == 0 and stat.S_IMODE(os.lstat(STATE).st_mode) == 0o700)
            record = {"deployment": deployment, "previous": previous, "desired": desired, "status": "forward-intent", "restoreAttempted": False, "fingerprints": fingerprints()}
            atomic(record_path, canonical(record))
        else:
            check(record and record["deployment"] == deployment and record["previous"] == previous)
        if operation == "finalize":
            ready(instance, desired)
            check(fingerprints() == record["fingerprints"] and record["desired"] == desired)
            record["status"] = "committed"
            atomic(record_path, canonical(record))
            return {"digest": desired}, instance, operation
        if operation == "restore":
            check(not record["restoreAttempted"] and desired == record["previous"])
            record["restoreAttempted"] = True
            record["status"] = "restore-intent"
            atomic(record_path, canonical(record))
        check(fingerprints() == record["fingerprints"])
        if current != desired:
            pull(desired, get)
            check(fingerprints() == record["fingerprints"] and reference() == current)
            info = protected(REFERENCE)
            atomic(REFERENCE, ("ENVER_API_IMAGE=" + REPOSITORY + "@" + desired + "\n").encode(), stat.S_IMODE(info.st_mode), info.st_gid)
            # Existing ExecStartPre performs ordinary materialization + preserved CloudFront overlay.
            run(["systemctl", "restart", "enver-api.service"], timeout=180)
        wait_ready(instance, desired)
        check(fingerprints() == record["fingerprints"])
        record["status"] = "restored" if operation == "restore" else "deployed"
        atomic(record_path, canonical(record))
        return {"digest": desired}, instance, operation


if __name__ == "__main__":
    try:
        result, instance_id, action = main()
        print(json.dumps({"schema": 1, "status": "PASS", "instance": instance_id, "operation": action, **result}))
    except Exception:
        print('{"schema":1,"status":"FAIL","code":"REMOTE_CONTRACT_FAILED"}')
        raise SystemExit(1) from None
