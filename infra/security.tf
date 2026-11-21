# T-INFRA-005/006: SG para recursos futuros; NO ALB, EC2 o IAM aplicados.
# Reglas separadas: no mezclar reglas inline con aws_vpc_security_group_*_rule.
resource "aws_security_group" "sg_alb" {
  name        = "${local.name_prefix}-alb"
  description = "Future public ALB: HTTPS only; no HTTP listener provisioned"
  vpc_id      = aws_vpc.enver.id
  tags        = merge(local.common_tags, { Name = "${local.name_prefix}-alb-sg" })
}

resource "aws_security_group" "sg_backend" {
  name        = "${local.name_prefix}-backend"
  description = "Future backend: application ingress from ALB security group only"
  vpc_id      = aws_vpc.enver.id
  tags        = merge(local.common_tags, { Name = "${local.name_prefix}-backend-sg" })
}

resource "aws_vpc_security_group_ingress_rule" "alb_https" {
  security_group_id = aws_security_group.sg_alb.id
  description       = "Public HTTPS to future ALB; ACM and listener are not defined"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 443
  to_port           = 443
  tags              = local.common_tags
}

resource "aws_vpc_security_group_ingress_rule" "backend_from_alb" {
  security_group_id            = aws_security_group.sg_backend.id
  referenced_security_group_id = aws_security_group.sg_alb.id
  description                  = "Declared container port only from future ALB; internal TLS unresolved"
  ip_protocol                  = "tcp"
  from_port                    = local.backend_container_port
  to_port                      = local.backend_container_port
  tags                         = local.common_tags
}

resource "aws_vpc_security_group_egress_rule" "alb_to_backend" {
  security_group_id            = aws_security_group.sg_alb.id
  referenced_security_group_id = aws_security_group.sg_backend.id
  description                  = "ALB may initiate connections only to backend application port"
  ip_protocol                  = "tcp"
  from_port                    = local.backend_container_port
  to_port                      = local.backend_container_port
  tags                         = local.common_tags
}

# Excepción aprobada explícitamente en FIX PIPELINE, solo este recurso/regla.
# Owner: responsable ENVER. Caduca 2026-12-19; revisar antes de cualquier apply.
# Endpoints públicos AWS/ECR/SSM/Secrets/Dynamo/S3: HTTPS, sin CIDRs fijos aprobados.
#trivy:ignore:AVD-AWS-0104:exp:2026-12-19
resource "aws_vpc_security_group_egress_rule" "backend_https" {
  security_group_id = aws_security_group.sg_backend.id
  description       = "HTTPS to public AWS service endpoints; temporary reviewed destination exception"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 443
  to_port           = 443
  tags              = local.common_tags
}

# Atlas SRV usa destinos públicos dinámicos, no direcciones privadas inventadas.
# Excepción exacta aprobada; owner/caducidad/condiciones en EGRESS-REVIEW.md.
#trivy:ignore:AVD-AWS-0104:exp:2026-12-19
resource "aws_vpc_security_group_egress_rule" "backend_atlas" {
  security_group_id = aws_security_group.sg_backend.id
  description       = "MongoDB Atlas TLS 27017; temporary reviewed destination exception"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 27017
  to_port           = 27017
  tags              = local.common_tags
}

resource "aws_vpc_security_group_egress_rule" "backend_cache" {
  security_group_id            = aws_security_group.sg_backend.id
  referenced_security_group_id = aws_security_group.cache.id
  description                  = "Valkey TLS only to the existing cache security group"
  ip_protocol                  = "tcp"
  from_port                    = 6379
  to_port                      = 6379
  tags                         = local.common_tags
}

# AmazonProvidedDNS (resolver VPC) no se filtra mediante SG: no abrir DNS público.
# No UDP/TCP53 genérico, HTTP80, puertos SSH, Redis público ni all-protocol.
# No abre ingress ni reemplaza allowlist Atlas/TLS/IAM. Sin apply en este gate.
# No ingress 22/2375/2376, no ingress público de aplicación y no regla 80 aún.
# SSM requerirá rol/profile, agente y conectividad probados en otra autorización.
# T-INFRA-005 pide diseño/separación, no crear trust todavía: no rol/profile IAM
# ni attachments a enver-terraform-local. Bootstrap revisable en bootstrap/.
