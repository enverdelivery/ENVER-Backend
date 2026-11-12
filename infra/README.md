# ENVER — Baseline Terraform local (T-INFRA-001 / T-INFRA-002)

## Propósito y alcance

Baseline de [003-infrastructure](../specs/003-infrastructure/spec.md), conforme a su [plan](../specs/003-infrastructure/plan.md) y [tareas](../specs/003-infrastructure/task.md). Este incremento prepara naming, variables, provider AWS y state local, **sin recursos, data sources, módulos, comandos remotos o infraestructura desplegada**. No implementa las fases posteriores de 003 ni CI/CD de [004](../specs/004-ci-cd/spec.md).

Authentication permanece TERMINADA — LOCAL/TEST REPRESENTATIVO. Nada aquí acredita cloud, readiness, IAM o dependencias productivas. No ejecutar `plan` ni `apply` en este bloque.

## Requisitos y versiones

- Terraform instalado por el usuario: **1.17.1, windows_amd64**, detectado el 2026-11-12; no se actualizó. `required_version = ">= 1.17.1, < 1.18.0"` permite parches de la misma minor; ampliar minor requiere revisión.
- Provider único: `hashicorp/aws`, restricción **`~> 6.0`** (6.x, no 7.x). La API oficial del Registry reportó **6.73.0**, publicada el 2026-11-11, al revisar esta baseline: [fuente](https://registry.terraform.io/v1/providers/hashicorp/aws). La restricción mantiene la serie mayor observada; la selección exacta y sus checksums quedan en `.terraform.lock.hcl` generado por init, no se inventan ni editan manualmente.
- Acceso HTTPS al Registry/releases para descargar provider. `init -backend=false` instala/verifica provider y genera lock, no requiere autenticación AWS ni crea recursos.
- El primer `terraform -version` no resolvió el comando en el PATH heredado. Se encontró el ejecutable ya instalado en el PATH persistido del usuario/WinGet. Las ejecuciones de este bloque lo usarán mediante PATH temporal del proceso; no se modifica PATH persistente, no se reinstala ni actualiza Terraform.
- Autenticación AWS: **pendiente de acreditar**. CLI/identidad y resultados reales se registran en task.md; no confundir presencia de una configuración con cuenta/región aprobadas.

Resultado local del BUILD: init seleccionó **AWS 6.73.0 firmado por HashiCorp**; `.terraform.lock.hcl` generado y conservado para el checkpoint posterior. Fmt/check y validate finalizaron con exit 0. AWS CLI no disponible e identidad AWS no acreditada. T-INFRA-001/002 y GATE-I-A conservan los pendientes de aceptación descritos en task §6.

## Cuenta, región, presupuesto y configuración

| Dato | Estado / tratamiento |
| --- | --- |
| Proyecto | ENVER; identificador técnico `enver` |
| Cuenta AWS / responsable operativo | PENDIENTES; `aws_account_id` obligatorio sin default, guard `allowed_account_ids` al configurar provider |
| Región/AZ y ambiente cloud final | PENDIENTES; `aws_region` y `environment` obligatorios sin default, no se selecciona una región |
| Presupuesto máximo, duración, precios/créditos | PENDIENTES; no hay aprobación de cargos ni alarma/recurso de presupuesto creada |
| Dominio, tamaños EC2, Redis final, Atlas tier | PENDIENTES para sus tareas específicas, sin defaults especulativos |

`terraform.tfvars.example` es versionable y **no operativo**: contiene ejemplos no sensibles y placeholders de cuenta/región que fallan las validaciones de destino si se intentan usar. Formato válido de una región/cuenta no significa aprobación. Antes de una ejecución futura que use inputs, el responsable debe aprobar destino/ambiente/presupuesto y preparar `terraform.tfvars` local ignorado, sin secretos.

Las únicas variables actuales son `project_name`, `environment`, `aws_region`, `aws_account_id` y `common_tags`. Naming común: `<project_name>-<environment>`. Tags canónicos: Project, Environment, ManagedBy=Terraform; tags adicionales no pueden sobrescribirlos ni usar prefijo aws:. Tags son metadatos potencialmente visibles: nunca datos personales, credenciales o secretos. No hay ALB URL, IDs de instancia, ECR URL u outputs inventados.

Provider usa la cadena estándar AWS externa (perfil/SSO/rol/entorno configurados mediante un flujo autorizado), nunca access_key/secret_key/session_token en HCL. No crear usuarios IAM/access keys ni habilitar flags para saltar comprobaciones. AWS CLI/STS no son requisito de `validate`; la cuenta real se verificará por permiso posterior antes de acceso cloud. No registrar IDs completos innecesarios ni contenido de archivos de credenciales.

El presupuesto no se convierte en una protección por declararlo: no existe tope automático. Cómputo, EBS, IPv4, ALB, NAT/endpoints, Redis, Atlas, ECR, logs, secretos, DynamoDB, state remoto, DNS y transferencia pueden generar cargos. Validar precios regionales/cuenta y aprobar estimación antes de provisionar; no se afirma Free Tier. En este bloque no se definen ni crean tales recursos.

## Comandos autorizados en este bloque

Desde la raíz del repositorio, con el ejecutable instalado disponible en PATH del proceso:

```text
terraform -version
terraform fmt -recursive infra
terraform fmt -check -recursive infra
terraform -chdir=infra init -backend=false
terraform -chdir=infra validate
git diff --check
git status
git diff --stat
```

`validate` comprueba la configuración sin resolver valores de inputs ni configurar el acceso real AWS: éxito no aprueba los placeholders, autentica al operador, estima costos o acredita cloud. Los comandos, provider realmente seleccionado, lock y resultados observados se registran en [task.md](../specs/003-infrastructure/task.md). No usar `init -upgrade` ni actualizar lock por iniciativa propia. `fmt` no comprueba existencia de recursos.

**No autorizados ahora:** plan, apply, destroy, import, refresh, operaciones state, backends remotos, infraestructura, Docker/build/tests, GitHub workflows o deployment. Ningún comando permitido autoriza avanzar a T-INFRA-003/T-INFRA-025 ni cerrar sus gates. No stage/commit/push.

## State local: custodia, respaldo y recuperación

`main.tf` declara backend **local**, ruta `terraform.tfstate` relativa a `infra/`. La inicialización autorizada usa `-backend=false`: no inicializa backend remoto ni demuestra protección/cifrado de un state. No crear un state artificial para aparentar evidencia ni ejecutar apply para generarlo.

- Uso inicial de **un operador**; el directorio y cualquier state/backup/plan requieren acceso local restringido. Gitignore no cifra ni aplica ACL; no afirmar que existen esos controles solo porque el archivo está ignorado.
- `.terraform/`, `*.tfstate`, `*.tfstate.*`, `*.tfvars`, `*.tfvars.json`, planes/crash logs/overrides están excluidos por el `.gitignore` raíz. `.terraform.lock.hcl` y `terraform.tfvars.example` deben poder versionarse. Provider binario local no se versiona.
- **Antes de crear o custodiar state real:** designar operador/destino aprobado, verificar ACL y cifrado en reposo; aprobar almacenamiento de backup cifrado externo a Git y su política de retención/acceso. No asumir que el state local trae cifrado incorporado ni que un `.backup` es respaldo cifrado.
- **Procedimiento de respaldo futuro:** detener mutaciones concurrentes, identificar ambiente/lineage/serial y versión del state sin publicar valores; copiar por mecanismo de cifrado aprobado a destino restringido; registrar hash/fecha/versión de forma protegida y comprobar legibilidad/recuperación de la copia con el operador autorizado. No copiar state a artifacts/temporales abiertos o docs.
- **Recuperación futura:** mantener exclusión, verificar integridad/cifrado/ambiente/lineage/serial de la copia aprobada, restaurar solo con autorización y reconciliar frente a recursos reales antes de mutar. No ejecutar restore/push de state ni sobrescribir datos por esta entrega.
- No cambiar `environment`/cuenta/región sobre el mismo state para representar otro entorno. Separación de directorios/state/ambientes y migración requieren aprobación explícita.
- **Antes de colaboración o GitHub:** backend remoto protegido/cifrado/versionado con locking compatible, bootstrap/migración/recuperación aprobados y probados conforme a 003; no state local efímero de runner. S3 es propuesta futura, no creado. No crear DynamoDB para lock: la Authentication Authority DynamoDB es otra responsabilidad.

El destino/mecanismo de cifrado, ACL y prueba de restauración de un state real siguen pendientes mientras no exista autorización/evidencia. Esta baseline técnica no basta para declarar custodia integral comprobada o GATE-I-A cerrado.

## Estructura actual y evolución futura

```text
infra/
├── main.tf
├── versions.tf
├── providers.tf
├── variables.tf
├── outputs.tf
├── locals.tf
├── terraform.tfvars.example
├── README.md
└── .terraform.lock.hcl       # generado correctamente por init
```

`.terraform/` contiene descargas locales ignoradas. `networking.tf`, `security.tf`, `compute.tf`, `load-balancer.tf`, `registry.tf`, `redis.tf`, `authority.tf`, `secrets.tf` y `observability.tf` son futuros de 003: **no creados ni autorizados en este bloque**. No definir recursos para adelantar esas fases.

004 conserva CI automático sin deployment y CD exclusivamente manual por workflow_dispatch tras CI verde del SHA/digest exactos. El ownership de digest Terraform/deploy y bootstrap de ECR/IAM siguen pendientes; esta baseline no los resuelve ni crea pipelines. El resultado y límites del incremento se registran en task.md y memoria.md, sin reescribir spec/plan.
