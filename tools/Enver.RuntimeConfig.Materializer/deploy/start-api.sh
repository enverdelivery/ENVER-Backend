#!/bin/sh
set -eu
ulimit -c 0
# Preparación únicamente. No se crea api-start.authorized ni se fija imagen en este gate.
test -f /etc/enver/api-start.authorized
test -f /run/enver/appsettings.Runtime.json
printf '%s' "${ENVER_API_IMAGE:-}" | /usr/bin/grep -Eq '^[a-zA-Z0-9./:_-]+@sha256:[a-f0-9]{64}$'
# El archivo se monta después del rename del ExecStartPre, nunca docker cp ni env secreto.
exec /usr/bin/docker run --rm --name enver-api --network bridge --platform linux/amd64 \
    --publish 127.0.0.1:8080:8080 \
    --user 1654:1654 --read-only --cap-drop ALL --security-opt no-new-privileges \
    --ulimit core=0:0 --log-driver journald \
    --tmpfs /tmp:rw,nosuid,nodev,noexec,size=16m,mode=1777 \
    --mount type=bind,src=/run/enver/appsettings.Runtime.json,dst=/run/enver/appsettings.Runtime.json,readonly \
    --env ASPNETCORE_ENVIRONMENT=Development --env HOME=/nonexistent \
    --env AWS_CONFIG_FILE=/dev/null --env AWS_SHARED_CREDENTIALS_FILE=/dev/null \
    --env AWS_EC2_METADATA_DISABLED=false --env AWS_EC2_METADATA_V1_DISABLED=true \
    --env AWS_REGION=us-east-1 --env DOTNET_EnableDiagnostics=0 \
    "${ENVER_API_IMAGE}"
