# T-INFRA-017: diseño development, NO provisión ni compatibilidad runtime acreditada.
# Valkey 7.2 / familia valkey7: verificación regional administrativa; sin apply.
locals {
  cache_tags = merge(local.common_tags, {
    Project = "ENVER"
    Purpose = "RuntimeCache"
  })
}

resource "aws_security_group" "cache" {
  name        = "${local.name_prefix}-cache"
  description = "Private Valkey: TLS from ENVER backend only"
  vpc_id      = aws_vpc.enver.id
  tags        = merge(local.cache_tags, { Name = "${local.name_prefix}-cache" })

  # Sin egress: al crear el SG, el provider revoca el allow-all de AWS.
  # Las respuestas a conexiones autorizadas usan seguimiento stateful del SG.
  # No reglas inline ni excepción de egress genérico para el nodo único.
  lifecycle {
    precondition {
      condition = (
        var.project_name == "enver" && var.environment == "dev" &&
        var.aws_region == "us-east-1" && var.aws_account_id == "926925605666" &&
        aws_vpc.enver.id == "vpc-060f67e600affd569" &&
        aws_security_group.sg_backend.id == "sg-070d06cfebe866427" &&
        aws_security_group.sg_backend.vpc_id == aws_vpc.enver.id
      )
      error_message = "T017 solo cubre enver/dev en la cuenta, región, VPC y SG backend autorizados. Detener ante discrepancias; no reemplazar la red."
    }
  }
}

resource "aws_vpc_security_group_ingress_rule" "cache_from_backend" {
  security_group_id            = aws_security_group.cache.id
  referenced_security_group_id = aws_security_group.sg_backend.id
  description                  = "Valkey TLS 6379 from backend security group only"
  ip_protocol                  = "tcp"
  from_port                    = 6379
  to_port                      = 6379
  tags                         = local.cache_tags
}

resource "aws_elasticache_subnet_group" "cache" {
  name        = "${local.name_prefix}-cache-subnets"
  description = "ENVER Valkey in the two approved existing subnets"
  subnet_ids  = [aws_subnet.public["a"].id, aws_subnet.public["b"].id]
  tags        = merge(local.cache_tags, { Name = "${local.name_prefix}-cache-subnets" })

  # Las subnets existentes tienen ruta IGW; eso NO asigna IP pública al cache.
  # No se crean subnets privadas nuevas ni se cambian rutas de los hosts.
  lifecycle {
    precondition {
      condition = (
        aws_subnet.public["a"].id == "subnet-0120d869dd659e2a1" &&
        aws_subnet.public["b"].id == "subnet-01934348f5d6985aa" &&
        aws_subnet.public["a"].vpc_id == aws_vpc.enver.id &&
        aws_subnet.public["b"].vpc_id == aws_vpc.enver.id &&
        aws_subnet.public["a"].cidr_block == "10.20.0.0/24" &&
        aws_subnet.public["b"].cidr_block == "10.20.1.0/24" &&
        aws_subnet.public["a"].availability_zone == "us-east-1a" &&
        aws_subnet.public["b"].availability_zone == "us-east-1b"
      )
      error_message = "T017 exige las subnets A/B exactas del baseline administrativo corregido. No sustituir IDs, CIDR o AZ ni aceptar reemplazos de red."
    }
  }
}

resource "aws_elasticache_parameter_group" "cache" {
  name        = "${local.name_prefix}-valkey"
  family      = "valkey7"
  description = "ENVER Valkey 7.2: no eviction of security state"
  tags        = merge(local.cache_tags, { Name = "${local.name_prefix}-valkey" })

  parameter {
    name  = "maxmemory-policy"
    value = "noeviction"
  }
}

# Usuario y credenciales EXTERNALLY MANAGED: evidencia administrativa aceptada.
# Sin lookup DescribeUsers/Secrets Manager, recurso de usuario ni importación.
# Existencia/estado/engine/ACL/password son responsabilidad del owner externo;
# el plan solo valida el contrato local del ID, no vuelve a verificarlos en AWS.

resource "aws_elasticache_user_group" "cache" {
  engine        = "valkey"
  user_group_id = "${local.name_prefix}-valkey"
  user_ids      = var.valkey_runtime_user_id == null ? [] : [var.valkey_runtime_user_id]
  tags          = merge(local.cache_tags, { Name = "${local.name_prefix}-valkey" })

  # null NO es una configuración aplicable: la precondición bloquea el grupo
  # y su replication group dependiente. No count=0 ni fallback sin RBAC.
  # Terraform posee membresía; administración externa posee usuario/ACL/rotación.
  # VALKEY no requiere default user; el grupo asociado lo deshabilita.
  # Owner acredita usuario activo/VALKEY/password/ACL y secreto custodiado.
  # Pruebas negativas y compatibilidad runtime requieren evidencia aparte.
  # No gestionar membresía externa concurrente ni usar ignore_changes.
  lifecycle {
    precondition {
      condition = try(
        var.valkey_runtime_user_id != null && length(trimspace(var.valkey_runtime_user_id)) > 0,
        false
      )
      error_message = "EXTERNAL RBAC USER ID REQUIRED: proporcionar el ID real no secreto respaldado por evidencia administrativa. No aplicar grupo vacío ni cache sin RBAC; Terraform no verifica el usuario externo por API."
    }
  }
}

resource "aws_elasticache_replication_group" "cache" {
  replication_group_id = "${local.name_prefix}-valkey"
  description          = "ENVER development private Valkey; authentication activation pending"
  engine               = "valkey"
  engine_version       = "7.2"
  node_type            = var.cache_node_type
  port                 = 6379

  cluster_mode                = "disabled"
  num_cache_clusters          = 1
  automatic_failover_enabled  = false
  multi_az_enabled            = false
  preferred_cache_cluster_azs = [aws_subnet.public["a"].availability_zone]
  network_type                = "ipv4"

  subnet_group_name    = aws_elasticache_subnet_group.cache.name
  security_group_ids   = [aws_security_group.cache.id]
  parameter_group_name = aws_elasticache_parameter_group.cache.name
  user_group_ids       = [aws_elasticache_user_group.cache.user_group_id]

  transit_encryption_enabled = true
  transit_encryption_mode    = "required"
  at_rest_encryption_enabled = true
  # Sin kms_key_id: cifrado predeterminado gestionado por el servicio AWS.
  # Sin auth_token/passwords, recursos SecretVersion ni generación de claves.

  auto_minor_version_upgrade = false
  snapshot_retention_limit   = 0
  # Sin backup automático/HA en este candidato mínimo; NO durabilidad aceptada.
  # T018/T020 deben definir capacidad, continuidad y recuperación antes de uso.
  tags = merge(local.cache_tags, { Name = "${local.name_prefix}-valkey" })

  depends_on = [aws_vpc_security_group_ingress_rule.cache_from_backend]
}
