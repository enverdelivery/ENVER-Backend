# INFRA-5: ventana máxima inicial 48h, guardrail USD10 de la ventana,
# presupuesto proyecto USD50 AGREGADOS. No timer, tope AWS ni apply implícito.
data "aws_ssm_parameter" "al2023" {
  count = var.compute_ami_id == null ? 1 : 0
  name  = "/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64"
}

# Resolver alias mantenible, verificar Amazon/x86_64/HVM/EBS, registrar ID.
# Fijar compute_ami_id al reproducir; actualizar AMI solo con plan autorizado.
data "aws_ami" "api" {
  owners = ["amazon"]
  filter {
    name   = "image-id"
    values = [var.compute_ami_id != null ? var.compute_ami_id : nonsensitive(data.aws_ssm_parameter.al2023[0].value)]
  }
  filter {
    name   = "name"
    values = ["al2023-ami-2023.*-kernel-*-x86_64"]
  }
  filter {
    name   = "architecture"
    values = ["x86_64"]
  }
  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
  filter {
    name   = "root-device-type"
    values = ["ebs"]
  }
  filter {
    name   = "state"
    values = ["available"]
  }
}

# ENI independiente: no IP automática, solo SG backend; conserva EIP y dirección
# privada al reemplazar el host. Sin ingress/routing/subnet nuevos.
resource "aws_network_interface" "api" {
  for_each        = local.public_subnets
  subnet_id       = aws_subnet.public[each.key].id
  security_groups = [aws_security_group.sg_backend.id]
  description     = "ENVER API ${each.key}: stable egress, ALB-only application ingress"
  tags            = merge(local.common_tags, { Name = "${local.name_prefix}-api-${each.key}-eni" })
}

resource "aws_eip" "api" {
  for_each = local.public_subnets
  domain   = "vpc"
  tags     = merge(local.common_tags, { Name = "${local.name_prefix}-api-${each.key}-eip" })
}

resource "aws_eip_association" "api" {
  for_each             = local.public_subnets
  allocation_id        = aws_eip.api[each.key].id
  network_interface_id = aws_network_interface.api[each.key].id
  allow_reassociation  = false
  depends_on           = [aws_internet_gateway.enver]
}

resource "aws_instance" "api" {
  for_each                             = local.public_subnets
  ami                                  = data.aws_ami.api.id
  instance_type                        = "t3a.small"
  availability_zone                    = aws_subnet.public[each.key].availability_zone
  iam_instance_profile                 = aws_iam_instance_profile.api_runtime.name
  user_data                            = replace(file("${path.module}/bootstrap/compute-host.sh"), "\r\n", "\n")
  user_data_replace_on_change          = true
  monitoring                           = false
  instance_initiated_shutdown_behavior = "stop"

  primary_network_interface {
    network_interface_id = aws_network_interface.api[each.key].id
  }

  credit_specification {
    cpu_credits = "standard"
  }
  metadata_options {
    http_endpoint               = "enabled"
    http_tokens                 = "required"
    http_put_response_hop_limit = 1
    instance_metadata_tags      = "disabled"
  }
  root_block_device {
    volume_type           = "gp3"
    volume_size           = 10
    encrypted             = true
    delete_on_termination = true
    iops                  = 3000
    throughput            = 125
  }
  volume_tags = merge(local.common_tags, { Name = "${local.name_prefix}-api-${each.key}-root" })
  tags        = merge(local.common_tags, { Name = "${local.name_prefix}-api-${each.key}" })

  # EIP + permisos + ruta listos antes del boot; no ciclo de dependencia.
  # No se configura key_name, puerto22, contenedor, digest o servicio de API.
  depends_on = [
    aws_eip_association.api,
    aws_route.public_internet,
    aws_route_table_association.public,
    aws_iam_role_policy.api_ssm_session,
    aws_iam_role_policy.api_ecr_pull
  ]
  lifecycle {
    precondition {
      condition     = var.compute_plan_approved
      error_message = "INFRA-5 necesita autorización delimitada de diseño/plan. No concede apply ni gasto."
    }
    precondition {
      condition = (
        var.project_name == "enver" && var.environment == "dev" &&
        var.aws_region == "us-east-1" && var.aws_account_id == "926925605666" &&
        toset(local.selected_zone_ids) == toset(["use1-az1", "use1-az2"])
      )
      error_message = "La aprobación INFRA-5 solo cubre enver/dev/cuenta y dos Zone IDs del baseline. Detener ante drift."
    }
  }
}
