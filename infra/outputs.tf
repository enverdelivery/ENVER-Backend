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
