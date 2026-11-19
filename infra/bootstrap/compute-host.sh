#!/bin/bash
# INFRA-5: bootstrap host-only. No deployment, secretos, token ECR ni API.
set -euo pipefail
umask 027
trap 'echo "ENVER host bootstrap FAILED; API not started" >&2' ERR

install -d -m 0750 -o root -g root /opt/enver /var/lib/enver-bootstrap

# AL2023 bloquea repos al release de la AMI; no dnf upgrade/releasever=latest.
# Reproducción exige conservar AMI/release/paquetes y revisar su soporte.
# EIP/ENI y ruta son dependencias Terraform, con reintento acotado por arranque.
for attempt in 1 2 3; do
  if timeout 300 dnf --setopt=retries=2 --setopt=timeout=30 install -y docker amazon-ssm-agent; then
    break
  fi
  if [ "$attempt" -eq 3 ]; then
    echo "ENVER required packages unavailable; host bootstrap stopped" >&2
    exit 1
  fi
  sleep 15
done

# Acotar logs futuros de contenedores sin lanzar ninguno.
install -d -m 0755 -o root -g root /etc/docker
cat > /etc/docker/daemon.json <<'JSON'
{"log-driver":"local","log-opts":{"max-size":"10m","max-file":"3"}}
JSON
chmod 0644 /etc/docker/daemon.json
systemctl enable --now docker.service
systemctl enable --now amazon-ssm-agent.service
systemctl is-active --quiet docker.service
systemctl is-active --quiet amazon-ssm-agent.service

rpm -q docker amazon-ssm-agent > /var/lib/enver-bootstrap/package-versions.txt
cp /etc/os-release /var/lib/enver-bootstrap/os-release
if [ -f /etc/dnf/vars/releasever ]; then
  cp /etc/dnf/vars/releasever /var/lib/enver-bootstrap/releasever
fi
chmod 0640 /var/lib/enver-bootstrap/*
touch /var/lib/enver-bootstrap/host-prepared
echo "ENVER host prepared; no application deployed or health asserted"
