locals {
  name_prefix = "${var.project_name}-${var.environment}"

  # Evidencia de configuración: Container (Dockerfile) HTTP_PORTS=8080;
  # Dockerfile EXPOSE 8080. No es prueba de escucha cloud ni de TLS interno.
  backend_container_port = 8080

  # IDs ordenados para evitar inferir us-east-1a/b y depender del orden API.
  # Si cambia la elegibilidad, revisar reemplazos de subnet antes de cualquier apply.
  selected_zone_ids = slice(
    sort(data.aws_availability_zones.available.zone_ids),
    0,
    min(2, length(data.aws_availability_zones.available.zone_ids))
  )
  public_subnet_cidrs = cidrsubnets(var.vpc_cidr, var.public_subnet_newbits, var.public_subnet_newbits)
  public_subnets = {
    a = { cidr = local.public_subnet_cidrs[0], zone_id = try(local.selected_zone_ids[0], null) }
    b = { cidr = local.public_subnet_cidrs[1], zone_id = try(local.selected_zone_ids[1], null) }
  }

  common_tags = merge(var.common_tags, {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "Terraform"
  })
}
