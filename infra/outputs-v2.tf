# Salidas no secretas del NEW CLEAN VALKEY v2. No acreditan creación, conectividad,
# inicialización ni estado READY; solo exponen el contrato del recurso definido.

output "cache_v2_primary_endpoint" {
  description = "DNS privado futuro del primary Valkey v2. Cliente TLS con validación; no URL con credenciales."
  value       = aws_elasticache_replication_group.cache_v2.primary_endpoint_address
}

output "cache_v2_port" {
  description = "Puerto del endpoint Valkey v2; TLS obligatorio, nunca plaintext."
  value       = aws_elasticache_replication_group.cache_v2.port
}

output "cache_v2_user_group_id" {
  description = "ID del user group v2; no es usuario, ACL ni credencial."
  value       = aws_elasticache_user_group.cache_v2.user_group_id
}

output "cache_v2_replication_group_arn" {
  description = "ARN no secreto del replication group v2; no acredita provisión ni datos."
  value       = aws_elasticache_replication_group.cache_v2.arn
}

output "cache_v2_replication_group_id" {
  description = "ID del replication group v2; no acredita creación ni estado READY."
  value       = aws_elasticache_replication_group.cache_v2.replication_group_id
}

output "cache_v2_engine" {
  description = "Motor Valkey declarado para v2; no prueba de compatibilidad runtime."
  value       = aws_elasticache_replication_group.cache_v2.engine
}

output "cache_v2_tls_enabled" {
  description = "TLS habilitado por diseño junto con modo required; no prueba de handshake cloud."
  value       = aws_elasticache_replication_group.cache_v2.transit_encryption_enabled
}
