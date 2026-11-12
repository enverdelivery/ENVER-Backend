variable "project_name" {
  description = "Identificador técnico del proyecto; sin datos personales ni secretos."
  type        = string
  default     = "enver"
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,18}[a-z0-9]$", var.project_name))
    error_message = "project_name debe tener 3 a 20 caracteres: minúsculas, dígitos y guiones, sin guion final."
  }
}

variable "environment" {
  description = "Ambiente aprobado por el responsable. No se asume producción ni staging."
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{0,18}[a-z0-9]$", var.environment))
    error_message = "environment debe tener 2 a 20 caracteres: minúsculas, dígitos y guiones, sin guion final."
  }
}

variable "aws_region" {
  description = "Región AWS por aprobar; obligatoria y sin default. Formato no equivale a aprobación o disponibilidad."
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9]*(-[a-z0-9]+)+-[0-9]+$", var.aws_region))
    error_message = "aws_region debe tener un identificador regional válido en minúsculas; los placeholders no son regiones aprobadas."
  }
}

variable "aws_account_id" {
  description = "ID de cuenta AWS aprobada, sin default. Solo limita el destino; no es una credencial ni concede permisos."
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[0-9]{12}$", var.aws_account_id))
    error_message = "aws_account_id debe ser el ID de 12 dígitos aprobado por el responsable."
  }
}

variable "common_tags" {
  description = "Tags adicionales no sensibles; no usar datos personales, credenciales ni claves reservadas."
  type        = map(string)
  default     = {}
  nullable    = false

  validation {
    condition = alltrue([
      for key, value in var.common_tags :
      length(trimspace(key)) > 0 && length(key) <= 128 && length(value) <= 256 &&
      !startswith(lower(key), "aws:") &&
      !contains(["project", "environment", "managedby"], lower(key))
    ])
    error_message = "common_tags debe respetar longitudes de tags y no incluir aws:, Project, Environment o ManagedBy. Nunca incluir secretos."
  }
}
