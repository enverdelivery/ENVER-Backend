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

variable "compute_plan_approved" {
  description = "Autoriza solo preparación/plan INFRA-5, máximo48h y guardrail USD10 de ventana. No apply, timer o tope técnico AWS."
  type        = bool
  default     = false
  nullable    = false
}

variable "compute_ami_id" {
  description = "AMI Amazon Linux2023 x86_64 fijada para reproducción. null resuelve alias público SSM; registrar ID y revisar antes de apply. No secreto."
  type        = string
  default     = null
  validation {
    condition     = var.compute_ami_id == null ? true : can(regex("^ami-[0-9a-f]{8,17}$", var.compute_ami_id))
    error_message = "compute_ami_id debe ser null o un AMI ID válido; owner/OS/arquitectura se verifican por data source."
  }
}

variable "api_secret_arns" {
  description = "T-INFRA-019: lista vacía (policy inerte), core3 o core4 aprobado. Orden obligatorio mongodb, jwt, hmac, redis; solo los cuatro ARNs exactos autorizados. Metadata, nunca valores ni VersionIds. Poblar inputs no aplica IAM ni publica boundary v3."
  type        = list(string)
  default     = []
  nullable    = false

  validation {
    condition = length(var.api_secret_arns) == 0 || try(
      contains([3, 4], length(var.api_secret_arns)) &&
      length(distinct(var.api_secret_arns)) == length(var.api_secret_arns) &&
      alltrue([
        for arn in var.api_secret_arns : contains([
          "arn:aws:secretsmanager:us-east-1:926925605666:secret:enver/dev/api/mongodb-Iuqeur",
          "arn:aws:secretsmanager:us-east-1:926925605666:secret:enver/dev/api/jwt-AweYNs",
          "arn:aws:secretsmanager:us-east-1:926925605666:secret:enver/dev/api/hmac-s9TSqb",
          "arn:aws:secretsmanager:us-east-1:926925605666:secret:enver/dev/api/redis-FlY8aP"
        ], arn)
      ]) &&
      tolist([
        for arn in var.api_secret_arns :
        regex("^arn:aws:secretsmanager:us-east-1:926925605666:secret:enver/dev/api/(mongodb|jwt|hmac|redis)-[A-Za-z0-9]{6}$", arn)[0]
      ]) == tolist(slice(["mongodb", "jwt", "hmac", "redis"], 0, length(var.api_secret_arns))),
      false
    )
    error_message = "api_secret_arns debe contener 0, los tres core aprobados o los cuatro ARNs exactos aprobados, en orden mongodb/jwt/hmac[/redis], de us-east-1/cuenta926925605666/enver/dev/api. Sin otros sufijos, comodines ni placeholders."
  }
}

variable "api_secrets_kms_key_arn" {
  description = "T-INFRA-019: ARN exacto aprobado de aws/secretsmanager (evidencia previa/owner), obligatorio con api_secret_arns no vacío. Solo metadata; null conserva el contrato inerte, sin otra clave ni material criptográfico."
  type        = string
  default     = null

  validation {
    condition     = var.api_secrets_kms_key_arn == null ? true : var.api_secrets_kms_key_arn == "arn:aws:kms:us-east-1:926925605666:key/51e1d79c-e924-4330-ac41-c9b263ee4ce3"
    error_message = "api_secrets_kms_key_arn debe ser null o el ARN exacto aprobado de aws/secretsmanager: key/51e1d79c-e924-4330-ac41-c9b263ee4ce3 en cuenta926925605666/us-east-1. Sin alias, comodines u otras claves."
  }
}

variable "runtime_permissions_boundary_arn" {
  description = "ARN real aprobado de boundary externa ENVER, creada por administrador humano y no gestionada por Terraform. Obligatorio; verificar existencia/contenido/version/custodia antes de plan. Sin secreto ni default."
  type        = string
  nullable    = false

  validation {
    # Referencia determinística de cuenta/nombre aprobados, NO prueba de existencia.
    condition     = var.runtime_permissions_boundary_arn == "arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"
    error_message = "Se requiere el ARN real aprobado, path /, de ENVER-Runtime-Permissions-Boundary en la cuenta baseline. No usar placeholder/otra policy; verificar creación manual y custodia independiente."
  }
}

variable "cache_node_type" {
  description = "T017: candidato development. AWS documenta T4g micro con Valkey >=7.2; oferta us-east-1 y capacidad real pendientes. Cambiar tamaño requiere revisión de plan/costo, no autoriza apply."
  type        = string
  default     = "cache.t4g.micro"
  nullable    = false

  validation {
    condition     = contains(["cache.t4g.micro", "cache.t4g.small", "cache.t4g.medium"], var.cache_node_type)
    error_message = "El diseño T017 solo contempla la familia T4g development (micro/small/medium); otro nodo requiere revisión explícita."
  }
}

variable "valkey_runtime_user_id" {
  description = "T017: ID no secreto de usuario VALKEY EXTERNALLY MANAGED, verificado por evidencia del owner, sin lookup AWS. null bloquea grupo/cache; no password, ARN ni ID ficticio."
  type        = string
  default     = null
  nullable    = true

  validation {
    condition = var.valkey_runtime_user_id == null ? true : (
      length(trimspace(var.valkey_runtime_user_id)) > 0 &&
      can(regex("^[a-z][a-z0-9-]{0,127}$", var.valkey_runtime_user_id)) &&
      var.valkey_runtime_user_id == "enver-dev-runtime"
    )
    error_message = "T017 solo admite el ID no vacío/formato válido enver-dev-runtime, provisionado y verificado por el owner externo; no se consulta su existencia por API."
  }
}
