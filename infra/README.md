# ENVER — Terraform local y candidato networking/security (003–006)

## Propósito y alcance

Baseline de [003-infrastructure](../specs/003-infrastructure/spec.md), conforme a su [plan](../specs/003-infrastructure/plan.md) y [tareas](../specs/003-infrastructure/task.md). El BUILD 003–006 agrega **definiciones**, no recursos creados: VPC/subnets/IGW/rutas, data source de AZ, SG/reglas y bootstrap IAM revisable. Sin EC2, ALB, Redis, EIP, endpoints, NAT, IAM aplicado o CI/CD de [004](../specs/004-ci-cd/spec.md). Todas las dependencias/decisiones y gates no acreditados permanecen abiertos.

Authentication permanece TERMINADA — LOCAL/TEST REPRESENTATIVO. Nada aquí acredita cloud/readiness o dependencias productivas. **No apply/destroy/import**. Plan solo si inputs/decisiones y permisos suficientes están acreditados; en este BUILD no procede por denegación AZ y DP-I-03 pendiente.

## Requisitos y versiones

- Terraform instalado por el usuario: **1.17.1, windows_amd64**, detectado el 2026-11-12; no se actualizó. `required_version = ">= 1.17.1, < 1.18.0"` permite parches de la misma minor; ampliar minor requiere revisión.
- Provider único: `hashicorp/aws`, restricción **`~> 6.0`** (6.x, no 7.x). La API oficial del Registry reportó **6.73.0**, publicada el 2026-11-11, al revisar esta baseline: [fuente](https://registry.terraform.io/v1/providers/hashicorp/aws). La restricción mantiene la serie mayor observada; la selección exacta y sus checksums quedan en `.terraform.lock.hcl` generado por init, no se inventan ni editan manualmente.
- Acceso HTTPS al Registry/releases para descargar provider. `init -backend=false` instala/verifica provider y genera lock, no requiere autenticación AWS ni crea recursos.
- El primer `terraform -version` no resolvió el comando en el PATH heredado. Se encontró el ejecutable ya instalado en el PATH persistido del usuario/WinGet. Las ejecuciones de este bloque lo usarán mediante PATH temporal del proceso; no se modifica PATH persistente, no se reinstala ni actualiza Terraform.
- AWS CLI 2.40.1/perfil `enver-infra`, usuario `enver-terraform-local`: identidad validada en task §7 y repetida en este BUILD. Perfil solo por mecanismo externo, no hardcode en provider; STS no acredita permisos de infraestructura.

Resultado del BUILD inicial (task §6): init seleccionó **AWS 6.73.0 firmado por HashiCorp**; lock conservado sin actualización. El checkpoint posterior `611071f0ecb0d6cb67993f4e9bffd00d328a2f90` contiene esa baseline. Task §9 registra ahora 003–006/E025 sin cerrar T-INFRA-001/002 o gates por validación local.

## Cuenta, región, presupuesto y configuración

| Dato | Estado / tratamiento |
| --- | --- |
| Proyecto | ENVER; identificador técnico `enver` |
| Cuenta AWS / responsable operativo | Identidad acreditada, ID no publicado; variable obligatoria y guard `allowed_account_ids`. Ownership/custodia formal pendientes |
| Región/ambiente | `us-east-1`, `dev`, proyecto `enver`, confirmados para este BUILD; sin defaults de destino ni perfil embebido |
| AZ/topología/CIDR | Selección por data source; sin AZ reales acreditadas por falta de permiso. Candidato A y CIDR siguen por aceptar |
| Presupuesto/crédito | Crédito informado USD 100; presupuesto inicial total USD 50, reserva objetivo USD 50; duración/estimación integral/cobertura pendientes |
| Dominio, tamaños EC2, Redis final, Atlas tier | PENDIENTES para sus tareas específicas, sin defaults especulativos |

`terraform.tfvars.example` es versionable y **no operativo**: cuenta placeholder, valores de red propuestos y `network_design_approved=false`. Región/ambiente son baseline confirmada, no autorización de provisión. No cambiar el guard a true para eludir DP pendientes. Antes de un plan futuro, aprobar inputs y configuración/entorno/state/comandos; preferir `TF_VAR_*` en proceso o tfvars local ignorado, sin secretos. No registrar ID completo innecesario.

Variables: `project_name`, `environment`, `aws_region`, `aws_account_id`, `common_tags`, `vpc_cidr`, `public_subnet_newbits` y `network_design_approved`. Red sin defaults pendientes silenciosos. Naming `<project_name>-<environment>`; tags Project, Environment, ManagedBy=Terraform, Name por recurso; sin datos personales/secretos. Outputs son expresiones para VPC/subnets/SG **definidos**, no IDs reales todavía. No ALB URL, EC2 IDs o ECR URL inventados.

Provider usa la cadena estándar AWS externa (perfil/SSO/rol/entorno configurados mediante un flujo autorizado), nunca access_key/secret_key/session_token en HCL. No crear usuarios IAM/access keys ni habilitar flags para saltar comprobaciones. AWS CLI/STS no son requisito de `validate`; identidad/cuenta verificadas por STS en este bloque, sin demostrar permisos de infraestructura. Revalidar destino antes de un plan futuro. No registrar IDs completos innecesarios ni contenido de archivos de credenciales.

El presupuesto no se convierte en una protección por declararlo: no existe tope automático. Cómputo, EBS, IPv4, ALB, NAT/endpoints, Redis, Atlas, ECR, logs, secretos, DynamoDB, state remoto, DNS y transferencia pueden generar cargos. Validar precios regionales/cuenta y aprobar estimación antes de provisionar; no se afirma Free Tier. En este bloque no se definen ni crean tales recursos.

## Comandos autorizados en este bloque

Desde la raíz del repositorio, con el ejecutable instalado disponible en PATH del proceso:

```text
terraform -version
terraform fmt -recursive infra
terraform fmt -check -recursive infra
terraform -chdir=infra validate
git diff --check
git status
git diff --stat
```

`validate` comprueba la configuración sin resolver valores de inputs ni configurar el acceso real AWS: éxito no aprueba los placeholders, autentica al operador, estima costos o acredita cloud. Los comandos, provider realmente seleccionado, lock y resultados observados se registran en [task.md](../specs/003-infrastructure/task.md). No usar `init -upgrade` ni actualizar lock por iniciativa propia. `fmt` no comprueba existencia de recursos.

Init del BUILD inicial fue `-backend=false`; no se repite ni se descarga otro provider. Plan está autorizado **solo condicionalmente**; no ejecutado ahora. No apply/destroy/import, refresh, operaciones state, backend remoto, provisión, Docker/build/tests, workflows/deployment ni stage/commit/push. E025 parcial registra únicamente comprobaciones posibles; no cierra dependencias o gates.

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
├── networking.tf
├── security.tf
├── bootstrap/
│   └── terraform-local-network-policy.json
├── terraform.tfvars.example
├── README.md
└── .terraform.lock.hcl       # generado correctamente por init
```

`.terraform/` contiene descargas locales ignoradas. `compute.tf`, `load-balancer.tf`, `registry.tf`, `redis.tf`, `authority.tf`, `secrets.tf` y `observability.tf` siguen futuros: no creados en este bloque.

004 conserva CI automático sin deployment y CD exclusivamente manual por workflow_dispatch tras CI verde del SHA/digest exactos. El ownership de digest Terraform/deploy y bootstrap de ECR/IAM siguen pendientes; esta baseline no los resuelve ni crea pipelines. El resultado y límites del incremento se registran en task.md y memoria.md, sin reescribir spec/plan.

## Candidato A: red, puerto y controles

Spec §6 permite proponer ALB y EC2 en subnets públicas con backend accesible **solo por SG ALB**; B (backend privado con salida equivalente) sigue alternativa. DP-I-03 no está aceptada. Se materializa A como candidato revisable, con precondición `network_design_approved` requerida antes de plan; no se ejecuta mientras esté pendiente.

- VPC dedicada propuesta `10.20.0.0/16`, DNS support/hostnames; no prueba de ausencia de solapamientos de cuenta/redes externas.
- Dos subnets derivadas `cidrsubnets(vpc_cidr, newbits, newbits)`: ejemplo `8` → `10.20.0.0/24` y `10.20.1.0/24`. Son propuestas, no defaults. Contenidas/no solapadas por construcción; al menos /27 para futuro ALB. Redis privado se resolverá en su tarea, nunca en estas subnets públicas por ahorro.
- Data source `aws_availability_zones`: disponibles, tipo availability-zone, opt-in habilitado/no requerido; ordenar Zone IDs y tomar dos. Claves Terraform estables a/b. No se infieren `us-east-1a/b`; reevaluar reemplazos si cambia elegibilidad. Falla si hay menos de dos. Aptitud de una futura AMI/tipo/capacidad EC2 requiere validación posterior.
- Subnets → asociaciones → route table pública compartida → ruta `0.0.0.0/0` → IGW. Sin NAT/endpoints. `map_public_ip_on_launch=false` evita IPs automáticas/costos inadvertidos; **una subnet pública/IGW no da salida IPv4 a un host sin IP pública/EIP**. La asociación EIP se autorizará aparte.
- SG ALB: ingreso TCP 443 IPv4 público, egreso solo a SG backend/8080. Sin 80 hasta configurar redirect aprobado; SG no crea listener/ACM ni HTTPS real.
- SG backend: ingreso solo SG ALB/TCP 8080; sin ingress público, SSH 22 o Docker 2375/2376. No adjuntar SG default ni otros SG permisivos al futuro host. No se gestiona NACL/SG default ni se afirma verificación de bypass real.
- Puerto **declarado** 8080 acreditado por `Enver.API/Properties/launchSettings.json` (Container HTTP_PORTS), Dockerfile EXPOSE y referencia oficial .NET de imagen base. `Program.cs`/appsettings no fijan otro listener. No se ejecutó contenedor: no prueba de escucha cloud ni del futuro mapeo host→contenedor. EXPOSE 8081/useSSL local no prueban certificado HTTPS productivo.
- Egreso backend **IPv4 amplio provisional** (`0.0.0.0/0`, todos los protocolos), para no romper Atlas, AWS APIs/ECR/CloudWatch/SSM, validación de certificados, bootstrap y Redis con una lista de puertos incompleta. **No es ingress público ni autoriza /0 en Atlas**. Mayor superficie de salida/exfiltración pendiente de reducir con diseño completo de destinos, endpoints/red, puertos/SG Redis y requisitos de AMI/TLS; no se promete filtro FQDN/segmentación fina. AmazonProvidedDNS no se filtra por SG; usar resolver VPC aprobado. No IPv6 configurado. La aceptación final de egreso sigue pendiente, sin conectividad real acreditada.
- Administración prevista SSM: requiere agente, role/profile y conectividad, no existen todavía. **TLS ALB→backend PENDIENTE** (DP-I-05); 8080 es candidato de configuración, no elección aprobada de HTTP interno. Si se exige HTTPS, revisar puerto/certificado/transporte con su incremento. TLS público obligatorio ALB/ACM, no implementado.

Fuentes: [Atlas puertos 27015–27017](https://www.mongodb.com/docs/atlas/connect-to-database-deployment/), [Atlas allowlist](https://www.mongodb.com/docs/atlas/security/ip-access-list/), [IGW y costo](https://docs.aws.amazon.com/vpc/latest/userguide/VPC_Internet_Gateway.html), [imagen base .NET](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime-deps/10.0/noble/amd64/Dockerfile). Imagen por tag, no digest certificado en este bloque.

## Atlas e IPs: recomendación/costo, NO aprovisionamiento

Estimación consultada **2026-11-12**, región baseline **us-east-1**, USD, sin descuentos/crédito/Free Tier ni impuestos. [Precio oficial VPC](https://aws.amazon.com/vpc/pricing/): IPv4 pública **0.005 USD/IP-hora**, igual asociada/inactiva, aplicable a regiones comerciales; [VPC base](https://docs.aws.amazon.com/vpc/latest/userguide/what-is-amazon-vpc.html#pricing) e IGW no tienen cargo directo adicional. No se habilitan IPAM avanzado, análisis, flow logs o encryption controls facturables.

| Alternativa futura | Estabilidad/operación | Estimación parcial de salida |
| --- | --- | --- |
| Dos IPv4 automáticas EC2 | Cambian al stop/start o reemplazo; allowlist debe actualizarse antes de admitir tráfico. No satisface salida estable por sí sola | `2 × 0.005 × h`; 24 h: USD 0.24; 730 h: USD 7.30 (solo IP, no EC2) |
| **Dos EIP, una por EC2 — recomendada, pendiente aprobación** | Dos /32 explícitos Atlas; reasociar por reemplazo autorizado, conservar allowlist y comprobar ambas salidas. No abrir /0 para recuperar servicio | Misma tarifa base: USD 0.01/h, USD 7.30/730 h; EIP reservada aunque EC2 detenida sigue costando. Sin asignarlas aún |
| NAT Gateway | Salida estable pero costo horario, por GB e IP; uno por AZ para no centralizar salida en una AZ | **No elegido, no definido**. Tarifa exacta actual us-east-1/GB y transferencia pendiente de acreditar; no extrapolar el ejemplo Ohio de pricing a Virginia |
| EC2 privadas + NAT instance/endpoints/peering | Operación/capacidad/HA, IP, endpoints y tier Atlas deben aprobarse; endpoints AWS no dan salida a Atlas por sí solos | Pendiente; ningún componente agregado automáticamente |

**Definiciones INFRA-2/3 actuales: USD 0 de cargo recurrente directo estimado por VPC/subnets/rutas/IGW/SG sin workloads/IPs asignadas.** No equivale a toda la plataforma gratis: no hay apply ni factura medida. Proyección futura mínima de IP con 2 backend + al menos 2 IP ALB: `4 × 0.005 × 730 = USD 14.60`, **solo IPs**, ALB puede usar más. EC2/EBS, ALB horas/LCU, Redis, Atlas, transferencia, logs/secretos/authority siguen fuera de ese subtotal; total/horas/cobertura del crédito **pendientes**, sin aprobación de cargos bajo USD 50. Revalidar tarifa, calendario y costo acumulado antes de cualquier apply; reserva objetivo USD 50 no es tope técnico.

Runbook Atlas futuro: aprobar dos EIP/costo y ownership → asignarlas/asociarlas bajo permiso propio → ingresar solo /32 en proyecto Atlas mediante operador autorizado → comprobar salidas A/B y TLS antes de admisión → reemplazar/reasociar con exclusión/drenaje sin reset de cuotas/revocación → retirar allowlist/IP solo con autorización y comprobación de referencias. Quitar una entrada Atlas no prueba cierre inmediato de conexiones existentes. No hay Atlas real, reglas, EIP o provider Atlas creados.

## IAM bootstrap revisable y separación runtime

`bootstrap/terraform-local-network-policy.json` es propuesta **de solo lectura para plan/preflight**, no política aplicada ni permisos de provisión. Nueve Describe explícitos de red/tags, restringidos a región us-east-1 y usuario enver-terraform-local, sin Account ID. `Resource=*` permite enumeración regional requerida por esas lecturas; no acceso administrativo. Fuente revisada: [acciones EC2](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ec2.html). JSON válido no equivale a política IAM aceptada/probada.

**Bloqueo observado:** `ec2:DescribeAvailabilityZones` denegada con `UnauthorizedOperation`, CLI exit 254; recurso conjunto AZ regional (`*`), fase INFRA-2/preflight. STS fue exitoso, ID/ARN completo suprimidos. Por permiso insuficiente y decisiones abiertas, **plan NO ejecutado** y no existe lista de cambios de plan.

El responsable puede **revisar** la propuesta desde identidad administrativa y autorizar su asignación manual al usuario correcto (no automática por estos archivos); después volver a validar identidad/permisos y aprobar inputs antes de un nuevo plan. No se pide AdministratorAccess/PowerUserAccess ni se usa otro principal para eludir denegación. La política **no permite CreateTags, creación/mutación/borrado de red, RunInstances, EIP, ALB, ECR, ACM, Secrets, Redis, DynamoDB, CloudWatch, IAM/PassRole**: no son necesarios para definir y validar sin apply. Tagging actual solo en HCL; escrituras y lifecycle de red exigirán otra revisión antes de provisión. No se garantiza que nueve lecturas cubran todas las llamadas de futuras versiones/configuración; registrar una denegación nueva, no ampliar permisos a ciegas.

T-INFRA-005 pide diseñar separación y no crear trust aún: **no se define rol/profile/attachment runtime** en este bloque. Roles futuros: Terraform (IaC de fase autorizada), runtime EC2 (trust EC2 + instance profile y solo llamadas requeridas al incorporar servicios), operador SSM (sesiones con alcance), publisher/deploy OIDC (004) y bootstrap de secretos/authority (separado). Sin access keys dentro de EC2; sin crear/borrar tabla en runtime. SSM/ECR/logs/Secrets/DynamoDB se incorporarán únicamente cuando sus tareas/recursos/contratos estén aprobados; la sesión local no es rol runtime ni credencial productiva.
