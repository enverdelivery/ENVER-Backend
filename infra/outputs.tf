# Expresiones de recursos DEFINIDOS, no IDs existentes ni outputs acreditados.
output "vpc_id" {
  description = "ID futuro de la VPC definida; solo se materializa con provisión autorizada."
  value       = aws_vpc.enver.id
}

output "subnet_ids" {
  description = "IDs futuros por clave lógica a/b; sin subnets reales todavía."
  value       = { for key, subnet in aws_subnet.public : key => subnet.id }
}

output "security_group_ids" {
  description = "SG definidos para ALB/backend futuros, no recursos cloud existentes."
  value = {
    alb     = aws_security_group.sg_alb.id
    backend = aws_security_group.sg_backend.id
  }
}

output "ecr_repository_name" {
  description = "Nombre del único repositorio API definido; existencia pendiente de apply autorizado."
  value       = aws_ecr_repository.api.name
}

output "ecr_repository_url" {
  description = "URI no secreta para el contrato URI@sha256:digest; no acredita imagen publicada."
  value       = aws_ecr_repository.api.repository_url
}

output "ecr_repository_arn" {
  description = "ARN no secreto para permisos acotados; no credencial ni autorización de publicación."
  value       = aws_ecr_repository.api.arn
}

output "compute_ami_id" {
  description = "AMI resuelta para este plan; fijar al reproducir. No acredita instancia ni bootstrap."
  value       = data.aws_ami.api.id
}

output "compute_instances" {
  description = "IDs/AZ futuros de dos hosts, no health, SSM o aplicación acreditados."
  value = { for key, instance in aws_instance.api : key => {
    id         = instance.id
    zone_id    = aws_subnet.public[key].availability_zone_id
    private_ip = aws_network_interface.api[key].private_ip
  } }
}

output "compute_egress_ipv4" {
  description = "EIP futuras para allowlist Atlas /32 por autorización aparte; no crea reglas Atlas."
  value       = { for key, address in aws_eip.api : key => address.public_ip }
}

output "runtime_role_arn" {
  description = "Role EC2 futuro, sin grants Secrets/authority/CloudWatch ni permisos de deployment."
  value       = aws_iam_role.api_runtime.arn
}

# T017: contrato no secreto; no endpoint/SG materializado ni servicio READY.
output "cache_primary_endpoint" {
  description = "DNS privado futuro del primary Valkey. Cliente TLS con validación de certificado; no URL con credenciales."
  value       = aws_elasticache_replication_group.cache.primary_endpoint_address
}

output "cache_port" {
  description = "Puerto del endpoint Valkey; TLS obligatorio, nunca plaintext."
  value       = aws_elasticache_replication_group.cache.port
}

output "cache_security_group_id" {
  description = "SG exclusivo futuro del cache, solo ingress desde backend."
  value       = aws_security_group.cache.id
}

output "cache_subnet_group_name" {
  description = "Subnet group futuro de las dos subnets existentes autorizadas."
  value       = aws_elasticache_subnet_group.cache.name
}

output "cache_engine" {
  description = "Motor del candidato HCL; no prueba de compatibilidad runtime."
  value       = aws_elasticache_replication_group.cache.engine
}

output "cache_tls_enabled" {
  description = "TLS habilitado por diseño junto con modo required; no prueba de handshake cloud."
  value       = aws_elasticache_replication_group.cache.transit_encryption_enabled
}

# T020: metadata futura, no estado del registro ni autorización de bootstrap.
output "authentication_authority_table_name" {
  description = "Nombre de la tabla authority T020; no acredita provisión, bootstrap ni confianza."
  value       = aws_dynamodb_table.authentication_authority.name
}

output "authentication_authority_table_arn" {
  description = "ARN no secreto de la tabla authority T020; no contiene datos operacionales."
  value       = aws_dynamodb_table.authentication_authority.arn
}
