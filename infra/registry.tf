# INFRA-4: definición para revisión/plan; no autoriza apply ni publicación.
# Un solo repositorio privado por ambiente, sin nuevas variables/providers.
resource "aws_ecr_repository" "api" {
  name                 = "${local.name_prefix}-api"
  image_tag_mutability = "IMMUTABLE"
  force_delete         = false

  encryption_configuration {
    encryption_type = "AES256"
  }

  # Verificar BASIC/configuración efectiva del registry antes de apply/push.
  # No habilita Inspector ni modifica scanning a nivel cuenta.
  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "${local.name_prefix}-api"
  }
}

resource "aws_ecr_lifecycle_policy" "api" {
  repository = aws_ecr_repository.api.name

  # No expirar imágenes etiquetadas: conservar tag inmutable en cada digest
  # activo/de rollback. 14 días es edad desde push, NO desde retirar el tag.
  # La limpieza futura de versiones etiquetadas exige revisión/autorización.
  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Expire only untagged images pushed more than 14 days ago"
      selection = {
        tagStatus   = "untagged"
        countType   = "sinceImagePushed"
        countUnit   = "days"
        countNumber = 14
      }
      action = {
        type = "expire"
      }
    }]
  })
}
