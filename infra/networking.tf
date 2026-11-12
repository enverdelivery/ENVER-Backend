# T-INFRA-003/004: candidato A de spec §6, no topología aceptada ni aplicada.
# Dos subnets públicas futuras para ALB/hosts; Redis privado pertenece a otra fase.
data "aws_availability_zones" "available" {
  state                  = "available"
  all_availability_zones = false

  filter {
    name   = "zone-type"
    values = ["availability-zone"]
  }

  filter {
    name   = "opt-in-status"
    values = ["opt-in-not-required", "opted-in"]
  }
}

resource "aws_vpc" "enver" {
  cidr_block           = var.vpc_cidr
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = merge(local.common_tags, { Name = "${local.name_prefix}-vpc" })

  lifecycle {
    precondition {
      condition     = var.network_design_approved
      error_message = "DP-I-03 sigue pendiente: aprobar expresamente topología A, CIDR/no solapamientos, egreso y costos antes de planear. Esta variable no autoriza apply ni cierra gates."
    }

    precondition {
      condition     = length(local.selected_zone_ids) == 2
      error_message = "Se requieren dos AZ estándar disponibles y habilitadas para esta cuenta/región. No usar Local/Wavelength Zones ni nombres inferidos."
    }
  }
}

resource "aws_subnet" "public" {
  for_each = local.public_subnets

  vpc_id                  = aws_vpc.enver.id
  cidr_block              = each.value.cidr
  availability_zone_id    = each.value.zone_id
  map_public_ip_on_launch = false

  tags = merge(local.common_tags, { Name = "${local.name_prefix}-public-${each.key}" })
}

resource "aws_internet_gateway" "enver" {
  vpc_id = aws_vpc.enver.id
  tags   = merge(local.common_tags, { Name = "${local.name_prefix}-igw" })
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.enver.id
  tags   = merge(local.common_tags, { Name = "${local.name_prefix}-public-rt" })
}

resource "aws_route" "public_internet" {
  route_table_id         = aws_route_table.public.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.enver.id
}

resource "aws_route_table_association" "public" {
  for_each = local.public_subnets

  subnet_id      = aws_subnet.public[each.key].id
  route_table_id = aws_route_table.public.id
}
