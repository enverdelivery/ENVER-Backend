terraform {
  # Compatible con Terraform 1.17.1 observado; cambios de minor requieren revisión.
  required_version = ">= 1.17.1, < 1.18.0"

  required_providers {
    aws = {
      source = "hashicorp/aws"
      # Serie mayor 6 acreditada en Registry; init fija la versión en el lockfile.
      version = "~> 6.0"
    }
  }
}
