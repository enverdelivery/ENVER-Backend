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
