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
  description = "Región obligatoria sin default; baseline aprobada us-east-1. Otro destino requiere revisión."
  type        = string
  nullable    = false

  validation {
    condition     = can(regex("^[a-z][a-z0-9]*(-[a-z0-9]+)+-[0-9]+$", var.aws_region))
    error_message = "aws_region debe tener un identificador regional válido en minúsculas; los placeholders no son regiones aprobadas."
  }
}

variable "vpc_cidr" {
  description = "CIDR IPv4 privado de la VPC candidata; 10.20.0.0/16 es propuesta, no default aprobado ni prueba de ausencia de solapamientos."
  type        = string
  nullable    = false

  validation {
    condition = try(
      can(cidrnetmask(var.vpc_cidr)) &&
      cidrhost(var.vpc_cidr, 0) == split("/", var.vpc_cidr)[0] &&
      tonumber(split("/", var.vpc_cidr)[1]) >= 16 &&
      tonumber(split("/", var.vpc_cidr)[1]) <= 28 &&
      can(regex("^(10\\.|172\\.(1[6-9]|2[0-9]|3[01])\\.|192\\.168\\.)", var.vpc_cidr)),
      false
    )
    error_message = "vpc_cidr debe ser una red IPv4 RFC1918 canónica /16 a /28; revisar además solapamientos externos."
  }
}

variable "public_subnet_newbits" {
  description = "Bits adicionales para derivar dos subnets no solapadas de la VPC; propuesta /16 + 8 = dos /24, sin default."
  type        = number
  nullable    = false

  validation {
    condition = try(
      var.public_subnet_newbits == floor(var.public_subnet_newbits) &&
      var.public_subnet_newbits >= 1 &&
      tonumber(split("/", var.vpc_cidr)[1]) + var.public_subnet_newbits <= 27 &&
      can(cidrsubnets(var.vpc_cidr, var.public_subnet_newbits, var.public_subnet_newbits)),
      false
    )
    error_message = "Se requieren dos subnets contenidas/no solapadas, al menos /27 para futuro ALB; newbits entero positivo."
  }
}

variable "network_design_approved" {
  description = "Confirmación explícita posterior de DP-I-03: topología A, CIDR, egreso y costo revisados. No autoriza apply ni suple gates/dependencias. Sin default."
  type        = bool
  nullable    = false
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
