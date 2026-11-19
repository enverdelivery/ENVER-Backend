# NEW CLEAN VALKEY (v2): aditivo y aislado del cluster histórico `enver-dev-valkey`.
# No restaura snapshots, no migra datos, no reutiliza el endpoint antiguo y no modifica
# el replication group, user group, parameter group, subnet group, SG ni usuario runtime existentes.
# Crear este recurso NO acredita por sí mismo un ambiente jamás utilizado: esa evidencia
# se captura antes de cualquier `initialize` y se conserva fuera de este archivo.

locals {
  cache_v2_name = "${local.name_prefix}-valkey-v2"
}

resource "aws_elasticache_user_group" "cache_v2" {
  engine        = "valkey"
  user_group_id = local.cache_v2_name
  user_ids      = var.valkey_runtime_user_id == null ? [] : [var.valkey_runtime_user_id]
  tags          = merge(local.cache_tags, { Name = local.cache_v2_name })

  # Mismo contrato RBAC externo que el grupo histórico, pero con ID nuevo.
  # No se modifica ni se reutiliza `aws_elasticache_user_group.cache`.
  lifecycle {
    precondition {
      condition = try(
        var.valkey_runtime_user_id != null && length(trimspace(var.valkey_runtime_user_id)) > 0,
        false
      )
      error_message = "NEW VALKEY v2 exige el usuario runtime externo enver-dev-runtime; no aplicar grupo vacío ni cache sin RBAC."
    }
  }
}

resource "aws_elasticache_replication_group" "cache_v2" {
  replication_group_id = local.cache_v2_name
  description          = "ENVER development private Valkey v2; new clean environment, never-used accreditation pending"
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
  user_group_ids       = [aws_elasticache_user_group.cache_v2.user_group_id]

  transit_encryption_enabled = true
  transit_encryption_mode    = "required"
  at_rest_encryption_enabled = true
  # Sin kms_key_id: cifrado en reposo predeterminado gestionado por el servicio AWS.

  auto_minor_version_upgrade = false
  snapshot_retention_limit   = 0
  # Sin snapshot restore, snapshot ARN, global replication group ni primary_cluster_id.
  # Sin backup automático: este cluster nace vacío y no es recuperación del histórico.

  tags = merge(local.cache_tags, { Name = local.cache_v2_name })

  depends_on = [aws_vpc_security_group_ingress_rule.cache_from_backend]
}
