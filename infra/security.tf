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

resource "aws_vpc_security_group_egress_rule" "backend_initial" {
  security_group_id = aws_security_group.sg_backend.id
  description       = "Provisional IPv4 outbound access pending complete dependency egress design"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1"
  tags              = local.common_tags
}

# Egreso IPv4 amplio provisional para no inventar puertos/destinos completos.
# No abre ingress ni reemplaza allowlist Atlas/TLS/IAM. Afinar con dependencias.
# No ingress 22/2375/2376, no ingress público de aplicación y no regla 80 aún.
# SSM requerirá rol/profile, agente y conectividad probados en otra autorización.
# T-INFRA-005 pide diseño/separación, no crear trust todavía: no rol/profile IAM
# ni attachments a enver-terraform-local. Bootstrap revisable en bootstrap/.
