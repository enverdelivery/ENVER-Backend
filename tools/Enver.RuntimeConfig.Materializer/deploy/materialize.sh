#!/bin/sh
set -eu
ulimit -c 0
# Entorno vacío: ningún perfil, access key o session token heredado.
exec /usr/bin/env -i PATH=/usr/bin:/bin HOME=/nonexistent \
    AWS_CONFIG_FILE=/dev/null AWS_SHARED_CREDENTIALS_FILE=/dev/null \
    AWS_EC2_METADATA_DISABLED=false AWS_EC2_METADATA_V1_DISABLED=true \
    AWS_REGION=us-east-1 AWS_DEFAULT_REGION=us-east-1 AWS_MAX_ATTEMPTS=1 \
    DOTNET_EnableDiagnostics=0 \
    /opt/enver/runtime-config/current/Enver.RuntimeConfig.Materializer --materialize
