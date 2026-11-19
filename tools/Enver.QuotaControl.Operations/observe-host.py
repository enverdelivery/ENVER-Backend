"""Observación Linux por SSM. Sin Redis, secretos, cambios de servicios o deployments.

Solo cuenta indicadores. Ausencia de indicadores NO cierra el universo de escritores.
No exportar argv, environment, cron/ExecStart, logs ni contenido de scripts.
"""
import datetime
import glob
import json
import os
import re
import subprocess

PATTERN = re.compile(r"enver|quota.?control|redis-cli|valkey-cli|dotnet", re.I)


def run(args):
    try:
        p = subprocess.run(args, capture_output=True, text=True, timeout=8,
                           env={"PATH": "/usr/sbin:/usr/bin:/sbin:/bin", "LC_ALL": "C"})
        return {"available": True, "ok": p.returncode == 0, "code": p.returncode}, p.stdout
    except FileNotFoundError:
        return {"available": False, "ok": False}, ""
    except Exception:
        return {"available": True, "ok": False, "error": "SANITIZED_OBSERVATION_FAILURE"}, ""


def count_command(args):
    status, text = run(args)
    return dict(status, count=len(text.splitlines()) if status["ok"] else None)


result = {"schema": "enver.host-writer-observation/1",
          "startedUtc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
          "readOnly": True, "redisConnectionAttempted": False,
          "universeClosed": False, "barrierEstablished": False, "inflight": "UNKNOWN"}
status, text = run(["docker", "-H", "unix:///var/run/docker.sock", "ps", "-a", "--format", "{{.ID}} {{.Image}} {{.Names}}"])
result["containers"] = dict(status, allCount=len(text.splitlines()) if status["ok"] else None,
                            enverIndicators=sum(bool(PATTERN.search(x)) for x in text.splitlines()) if status["ok"] else None)
result["runningContainers"] = count_command(["docker", "-H", "unix:///var/run/docker.sock", "ps", "-q"])
ancestors = set()
current = os.getpid()
while current > 1 and current not in ancestors:
    ancestors.add(current)
    try:
        with open("/proc/%s/status" % current, encoding="utf-8") as f:
            current = int(re.search(r"^PPid:\s+(\d+)", f.read(), re.M).group(1))
    except Exception:
        break
direct, wrappers, uncertain = [], [], 0
for directory in glob.glob("/proc/[0-9]*"):
    pid = int(os.path.basename(directory))
    if pid in ancestors:
        continue
    try:
        with open(directory + "/comm", encoding="utf-8") as f:
            comm = f.read().strip()
        with open(directory + "/cmdline", "rb") as f:
            args = f.read(65536).replace(b"\0", b" ").decode("utf-8", "replace")
        if PATTERN.search(comm):
            direct.append({"pid": pid, "category": "dotnet-enver-or-redis-admin"})
        elif PATTERN.search(args):
            wrappers.append({"pid": pid, "category": "wrapper-with-writer-indicator"})
    except FileNotFoundError:
        pass  # Carrera de snapshot; no barrera ni prueba de drenaje.
    except Exception:
        uncertain += 1
result["processes"] = {"directIndicators": direct, "wrapperIndicators": wrappers,
                       "unreadable": uncertain, "observerAncestorsExcluded": len(ancestors)}
result["listener8080"] = count_command(["ss", "-H", "-ltn", "sport = :8080"])
result["established6379"] = count_command(["ss", "-H", "-tn", "state", "established",
                                          "( dport = :6379 or sport = :6379 )"])
for key, args in [("serviceFiles", ["systemctl", "list-unit-files", "--type=service", "--no-legend", "--no-pager"]),
                  ("services", ["systemctl", "list-units", "--all", "--type=service", "--no-legend", "--no-pager"]),
                  ("systemdJobs", ["systemctl", "list-jobs", "--no-legend", "--no-pager"])]:
    status, text = run(args)
    # Solo nombres/estados de unidades, no sus descripciones o comandos.
    matching = [x.split()[:4] for x in text.splitlines() if PATTERN.search(x.split()[0] if x.split() else "")]
    result[key] = dict(status, totalLines=len(text.splitlines()) if status["ok"] else None, matching=matching)
status, text = run(["systemctl", "list-timers", "--all", "--no-legend", "--no-pager"])
timers = []
for timer in sorted(set(re.findall(r"[A-Za-z0-9_.@:-]+\.timer", text))):
    trigger_status, trigger = run(["systemctl", "show", timer, "--property=Triggers", "--value"])
    targets = trigger.strip().split()
    indicators, inspected = False, trigger_status["ok"]
    for target in targets:
        target_status, command = run(["systemctl", "show", target, "--property=ExecStart", "--value"])
        inspected = inspected and target_status["ok"]
        indicators = indicators or bool(PATTERN.search(command))
    timers.append({"timer": timer, "targets": targets, "execStartRead": inspected,
                   "writerIndicator": indicators, "transitiveScriptsAudited": False})
result["timers"] = dict(status, entries=timers)
files = []
for pattern in ["/etc/crontab", "/etc/anacrontab", "/etc/cron.d/*", "/etc/cron.hourly/*",
                "/etc/cron.daily/*", "/etc/cron.weekly/*", "/etc/cron.monthly/*", "/var/spool/cron/*",
                "/opt/enver/**/*.sh", "/opt/enver/**/*.py", "/opt/enver/**/*.cjs",
                "/opt/enver/**/*.service", "/opt/enver/**/*.timer"]:
    files.extend(glob.glob(pattern, recursive=True))
files = sorted(set(f for f in files if os.path.isfile(f)))
unreadable, indicators = 0, []
for file in files[:256]:
    try:
        with open(file, "rb") as f:
            data = f.read(65537)
        if len(data) > 65536:
            unreadable += 1
        elif PATTERN.search(data.decode("utf-8", "replace")):
            indicators.append(file)
    except Exception:
        unreadable += 1
result["cronAndScripts"] = {"filesFound": len(files), "filesReadLimit": 256,
                            "unreadableOrOversized": unreadable, "truncated": len(files) > 256,
                            "writerIndicatorPaths": indicators, "transitiveScriptsAudited": False}
result["atJobs"] = count_command(["atq"])
result["finishedUtc"] = datetime.datetime.now(datetime.timezone.utc).isoformat()
print(json.dumps(result, separators=(",", ":")))
