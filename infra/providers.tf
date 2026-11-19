# Cadena estándar de credenciales AWS, configurada fuera del repositorio.
# No se omiten comprobaciones de identidad ni se incorporan credenciales.
provider "aws" {
  region              = var.aws_region
  allowed_account_ids = [var.aws_account_id]

  default_tags {
    tags = local.common_tags
  }
}
