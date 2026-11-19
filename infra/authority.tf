# T-INFRA-020: opción B tras ListTables vacío en la cuenta/región aprobadas.
# Solo infraestructura y permisos candidatos. NO apply ni bootstrap de datos.
locals {
  authentication_authority_table_name = "enver-dev-authentication-authority"
  authentication_authority_table_arn  = "arn:aws:dynamodb:${var.aws_region}:${var.aws_account_id}:table/${local.authentication_authority_table_name}"
}

resource "aws_dynamodb_table" "authentication_authority" {
  name                        = local.authentication_authority_table_name
  billing_mode                = "PAY_PER_REQUEST"
  hash_key                    = "Id"
  deletion_protection_enabled = true
  stream_enabled              = false

  attribute {
    name = "Id"
    type = "S"
  }

  # Sin sort key, GSI/LSI, TTL, réplicas, importación, restore ni items.
  # Sin server_side_encryption: cifrado predeterminado con clave AWS-owned,
  # NO alias/aws/dynamodb ni una nueva KMS key o grants KMS runtime.
  # PITR omitido: default false del provider; costo/recuperación diferidos.
  # La tabla vacía NO habilita confianza. Bootstrap separado y autorizado.
  tags = merge(local.common_tags, { Name = local.authentication_authority_table_name })

  lifecycle {
    precondition {
      condition = (
        var.project_name == "enver" && var.environment == "dev" &&
        var.aws_region == "us-east-1" && var.aws_account_id == "926925605666"
      )
      error_message = "T020 solo cubre enver/dev en us-east-1/cuenta926925605666. Repetir discovery antes de cualquier provisión autorizada; no crear una autoridad duplicada."
    }
  }
}

resource "aws_iam_role_policy" "api_dynamodb_auth_authority" {
  name = "enver-dev-dynamodb-auth-authority"
  # Rol existente aceptado. Referencia nominal deliberada para no incorporar
  # el refresh del rol y su snapshot calculado de otras policies al target T020.
  # No crea otro rol, cambia ownership ni administra T019/SSM/ECR/boundary.
  role = "enver-dev-api-runtime"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Sid    = "ReadAndCasOnlyAuthenticationAuthority"
      Effect = "Allow"
      Action = [
        "dynamodb:GetItem",
        "dynamodb:UpdateItem"
      ]
      # ARN determinístico exacto, revisable antes de apply. El depends_on
      # conserva orden tabla→policy sin depender de recursos aceptados ajenos.
      Resource = local.authentication_authority_table_arn
      Condition = {
        "ForAllValues:StringEquals" = {
          "dynamodb:LeadingKeys" = ["authentication"]
        }
        Null = { "dynamodb:LeadingKeys" = "false" }
      }
    }]
  })

  depends_on = [aws_dynamodb_table.authentication_authority]
}
