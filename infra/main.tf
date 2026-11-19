# Baseline de T-INFRA-002: no contiene recursos, data sources ni módulos.
# Backend local inicial de un solo operador; el state nunca se versiona.
# init -backend=false no inicializa este backend ni crea infraestructura.
terraform {
  backend "local" {
    path = "terraform.tfstate"
  }
}
