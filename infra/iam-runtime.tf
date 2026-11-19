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

# T-INFRA-019: core3 (mongodb/jwt/hmac) o core4 (+Redis real aprobado).
# api_secret_arns conserva ese orden y valida los cuatro ARNs exactos.
# Lista vacía mantiene la policy INERTE. HCL/plan no conceden permisos:
# boundary v2/core3 es evidencia del owner, no export live acreditado.
# Redis requiere propuesta v3/core4 cotejada/publicada por el owner y una
# actualización IAM futura autorizada; policy importada en state52 sin apply.
# Delta core4 aprobado: solo añadir Redis, preservando Sid y contrato KMS.
# Sin ListSecrets, DescribeSecret, escritura, rotación ni comodines de recurso.
# Preparación/plan no publican la boundary ni aplican esta policy al rol.
resource "aws_iam_role_policy" "api_secrets_read" {
  count = length(var.api_secret_arns) == 0 ? 0 : 1
  name  = "${local.name_prefix}-secrets-read"
  role  = aws_iam_role.api_runtime.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid      = "ReadExactCoreSecrets"
        Effect   = "Allow"
        Action   = "secretsmanager:GetSecretValue"
        Resource = var.api_secret_arns
      },
      {
        Sid      = "DecryptOnlyThroughSecretsManager"
        Effect   = "Allow"
        Action   = "kms:Decrypt"
        Resource = var.api_secrets_kms_key_arn
        Condition = {
          StringEquals = {
            "kms:ViaService"    = "secretsmanager.${var.aws_region}.amazonaws.com"
            "kms:CallerAccount" = var.aws_account_id
          }
        }
      }
    ]
  })

  lifecycle {
    precondition {
      condition     = var.api_secrets_kms_key_arn != null
      error_message = "Con api_secret_arns definido se exige el ARN de la clave KMS que cifra los secretos; no se asume la clave gestionada por AWS sin declararla."
    }
  }
}

resource "aws_iam_instance_profile" "api_runtime" {
  name = "${local.name_prefix}-api-runtime"
  role = aws_iam_role.api_runtime.name
  tags = merge(local.common_tags, { Name = "${local.name_prefix}-api-runtime" })
}
