# INFRA-5: definiciones para revisión/plan. No autorización de IAM/apply.
resource "aws_iam_role" "api_runtime" {
  name                 = "${local.name_prefix}-api-runtime"
  description          = "ENVER EC2 host: SSM sessions and read-only API image pull"
  max_session_duration = 3600
  # Customer-managed externa, creada/revisada por administrador humano.
  # Terraform solo la referencia en CreateRole; no administra su policy/versiones.
  permissions_boundary = var.runtime_permissions_boundary_arn
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = { Service = "ec2.amazonaws.com" }
      Action    = "sts:AssumeRole"
    }]
  })
  tags = merge(local.common_tags, { Name = "${local.name_prefix}-api-runtime" })
}

# Session Manager únicamente. Sin managed attachment, parámetros, inventario,
# documentos, CloudWatch, secretos o comandos administrativos globales.
# Comprobar agente/registro/sesión reales antes de declarar SSM operativo.
resource "aws_iam_role_policy" "api_ssm_session" {
  name = "${local.name_prefix}-ssm-session"
  role = aws_iam_role.api_runtime.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Action = [
        "ssm:UpdateInstanceInformation",
        "ssmmessages:CreateControlChannel",
        "ssmmessages:CreateDataChannel",
        "ssmmessages:OpenControlChannel",
        "ssmmessages:OpenDataChannel"
      ]
      Resource = "*"
      Condition = {
        StringEquals = { "aws:RequestedRegion" = var.aws_region }
      }
    }]
  })
}

resource "aws_iam_role_policy" "api_ecr_pull" {
  name = "${local.name_prefix}-ecr-pull"
  role = aws_iam_role.api_runtime.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect   = "Allow"
        Action   = "ecr:GetAuthorizationToken"
        Resource = "*"
        Condition = {
          StringEquals = {
            "aws:RequestedRegion"  = var.aws_region
            "aws:PrincipalAccount" = var.aws_account_id
          }
        }
      },
      {
        Effect = "Allow"
        Action = [
          "ecr:BatchCheckLayerAvailability",
          "ecr:GetDownloadUrlForLayer",
          "ecr:BatchGetImage"
        ]
        Resource = aws_ecr_repository.api.arn
        Condition = {
          StringEquals = { "aws:RequestedRegion" = var.aws_region }
        }
      }
    ]
  })
}

resource "aws_iam_instance_profile" "api_runtime" {
  name = "${local.name_prefix}-api-runtime"
  role = aws_iam_role.api_runtime.name
  tags = merge(local.common_tags, { Name = "${local.name_prefix}-api-runtime" })
}
