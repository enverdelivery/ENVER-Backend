# Tareas 003 — Infraestructura de ENVER Backend

## 1. Control, autorización y estado inicial

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.1.0 / 2026-11-12 |
| Solicitud | SDD — MATERIALIZAR 003-INFRASTRUCTURE; BUILD DOCUMENTAL ÚNICAMENTE |
| Fuentes | [spec.md](spec.md) y [plan.md](plan.md), propuesta aprobada como base documental |
| Catálogo | 30 tareas, T-INFRA-001 a T-INFRA-030 |
| Estado de todas las tareas | **PENDIENTE / NO EJECUTADA** |
| Estado de todos los gates | **PENDIENTE / NO SATISFECHO** |
| Evidencia de ejecución de 003 | Ninguna registrada; todas las E(T-INFRA-nnn) están pendientes |
| Baseline | `feature/authentication`, `c58ea86e3eddff4befa8f61398dd58d55c4ef985` |

Crear este catálogo **no autoriza ejecutar tareas**, cambiar código/configuración/Docker/tests, crear `infra/`, recursos AWS/Atlas, workflows o secretos, ni correr Terraform. No stage/commit/push. Los únicos archivos de esta entrega son los tres Markdown de 003. La autorización previa de la propuesta es documental, no habilita cargos ni credenciales.

Aplican [constitución](../../.specify/memory/constitution.md), [001](../001-backend-foundation/spec.md), [002](../002-authentication/spec.md) y el cierre local de [002 task §§82–84](../002-authentication/task.md). No se cierran GATE-G/H de 002 ni AUTH-027 integralmente.

Antes de cada ejecución futura: confirmar DP bloqueantes, autorización de alcance/archivos/comandos/entorno/costo, evidencia prevista y reversión. Los **archivos esperados** de cada tarea son destinos futuros sujetos a esa autorización, no archivos creados ahora. Las evidencias se almacenarán en ubicación protegida aprobada, no como secretos/state/logs crudos en Git.

## 2. Reglas de dependencia y evidencia

- Una dependencia exige su resultado aceptado; una referencia a otra fase no concede permiso para ejecutarla.
- T-INFRA-001–024 preparan decisiones, contratos y configuración; T-INFRA-025 valida baseline temprana y T-INFRA-026 el conjunto final. No se provisiona infraestructura hasta T-INFRA-027 con autorización posterior específica.
- T-INFRA-024 exige evidencia de readiness/forwarding de un incremento de aplicación **autorizado por separado**. Esta tarea integra/revisa evidencia; no implementa código o tests bajo autorización de 003.
- La aceptación de una configuración no demuestra que funcione en cloud. Pruebas reales, bootstrap y aceptación runtime de las fases se realizan en T-INFRA-027–030. Gates B–G son aceptación cloud, no prerrequisitos circulares que exijan cloud antes de revisar el plan de apply.
- 004 es dependencia externa futura: CI verde del mismo commit/digest, deployment manual mediante `workflow_dispatch`, permisos y smoke tests aprobados. No se crea ni ejecuta 004 aquí; su ausencia bloquea deployment, no se elude con un despliegue automático o un comando ad hoc.
- `E(T-INFRA-nnn)` identifica evidencia **futura**, no un archivo ya existente. Cada registro incluirá requisito/PLD/AC/gate, autorización, commit/digest, fecha/entorno, acciones/comandos realmente realizados, resultados/salida nativa cuando aplique, referencias saneadas, limitaciones y revisión del responsable. Credenciales, JWT, PII y state no son evidencia publicable.
- Fallos, resultados inciertos y bloqueos se conservan; no cerrar gate por reintentos ocultos ni extrapolar UT/IT/ST locales a cloud. Fallos inducidos, reprovisión, migración state y limpieza requieren permiso propio.

## 3. Gates formales

Todos los gates de esta tabla están **PENDIENTE / NO SATISFECHO**. Los prerrequisitos se exigen para su **cierre**, no autorizan las tareas. Los criterios AC se detallan en spec §18. Un gate solo puede cerrarse con evidencia y aceptación explícita del responsable.

| Gate | Objeto / criterio de cierre | Tareas y evidencias necesarias | Prerrequisitos de cierre | AC |
| --- | --- | --- | --- | --- |
| GATE-I-A | Baseline Terraform validada: presupuesto, versiones, ambiente y state aprobados; validación inicial real | E(001), E(002), E(025) | Autorización técnica de baseline; sin apply implícito | AC-I-01 |
| GATE-I-B | Networking/security verificados: VPC/rutas/SG/SSM/IAM, salidas Atlas y no acceso directo público al backend/Redis | E(003–006), E(019–020), E(027–028) | GATE-I-A | AC-I-02, 09 |
| GATE-I-C | Mínimo dos EC2 reales, dos AZ, mismo digest y capacidad/recuperación verificadas | E(007–010), E(027–028) | GATE-I-A, GATE-I-B | AC-I-03 |
| GATE-I-D | ALB distribuye entre ambas, health/readiness, drenaje y unhealthy/todos-unhealthy seguros | E(011–012), E(023–024), E(027–028) | GATE-I-B, GATE-I-C | AC-I-04, 11 |
| GATE-I-E | HTTPS público válido, DNS/ACM/listeners y transporte/proxy resueltos sin spoofing | E(013–014), E(023–024), E(027–028) | GATE-I-B, GATE-I-C, GATE-I-D | AC-I-05 |
| GATE-I-F | Atlas/Redis/secrets/authority integrados, índices/ACK, continuidad/fail-closed y bootstrap/rotación probados | E(015–020), E(023–024), E(027–028) | GATE-I-B, GATE-I-C | AC-I-06–09, 11 |
| GATE-I-G | Observabilidad operativa: señales, privacidad, capacidad Redis, alarma/destinatario y retención | E(021–022), E(027–028) | GATE-I-C, GATE-I-F | AC-I-10 |
| GATE-I-H | Infra cloud reproducible: IaC revisada, state protegido, drift/reproducción/rollback y evidencia verificable | E(002), E(008), E(025–030) | GATE-I-A a GATE-I-G | AC-I-12, 13, 15 |
| GATE-I-I | URL HTTPS pública sirve Register/Login/Logout desde dos instancias, controles distribuidos y negativos verificados; deploy manual trazable | E(027–030) y contrato aprobado 004 | GATE-I-A a GATE-I-H | AC-I-13–15 |

Notación abreviada: E(001) = E(T-INFRA-001); E(003–006) exige cada una de las cuatro evidencias. Ninguna está registrada por esta entrega. I-H/I-I no implican SLA, producción lista ni cierre automático de AUTH-027 o gates de 002.

## 4. Catálogo de tareas

### T-INFRA-001 — Cuenta, región, alcance y presupuesto

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-1 Baseline.
- **Objetivo:** fijar límites del ambiente cloud académico y responsables antes de cargos.
- **Dependencias:** autorización posterior del responsable; DP-I-01 y DP-I-13.
- **Trazabilidad:** INFRA-001–003, 033, 038; PLD-INFRA-001/020; AC-I-01, 15.
- **Archivos esperados:** decisiones autorizadas en `specs/003-infrastructure/spec.md` y `plan.md`; evidencia protegida externa.
- **Acciones futuras:** confirmar cuenta/región/AZ, acceso, nomenclatura, duración, presupuesto máximo, precios/créditos vigentes y propiedad de recursos. Estimar todos los servicios, sin asumir Free Tier.
- **Aceptación:** alcance, presupuesto y responsables aprobados por escrito; lista de cargos y recursos fuera de presupuesto, sin secretos.
- **Evidencia:** E(T-INFRA-001), aprobación y estimación fechada/regional saneada.
- **Riesgo:** cargos o exposición de datos de cuenta; una alarma no limita gasto.
- **Gate asociado:** GATE-I-A; soporte de GATE-I-H/GATE-I-I.

### T-INFRA-002 — Baseline Terraform, versiones y state

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-1 Baseline.
- **Objetivo:** definir base IaC reproducible y custodia del state.
- **Dependencias:** T-INFRA-001; resolver DP-I-02, DP-I-12/13 aplicables.
- **Trazabilidad:** INFRA-029–033, 038; PLD-INFRA-015/016; AC-I-01, 12.
- **Archivos esperados:** futuros `infra/main.tf`, `providers.tf`, `versions.tf`, `variables.tf`, `outputs.tf`; configuración backend según decisión aprobada. State nunca versionado.
- **Acciones futuras:** aprobar versiones/provider AWS, validar naming/tags/inputs/outputs y definir state local protegido, backup cifrado/operador único. Planear backend remoto S3/locking compatible antes de GitHub/colaboración.
- **Aceptación:** baseline revisada, versiones y manejo de state aprobados; ningún secreto en tfvars/output; no se crea bucket/tabla automáticamente.
- **Evidencia:** E(T-INFRA-002), revisión de baseline y procedimiento de respaldo/migración/recuperación.
- **Riesgo:** pérdida o filtración state, lock incompatible y confusión con la authority DynamoDB.
- **Gate asociado:** GATE-I-A, GATE-I-H.

### T-INFRA-003 — VPC, subnets y rutas

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-2 Networking.
- **Objetivo:** expresar topología simple en dos AZ con exposición deliberada.
- **Dependencias:** T-INFRA-001, T-INFRA-002, T-INFRA-025; DP-I-03.
- **Trazabilidad:** INFRA-007, 033; PLD-INFRA-004/015; AC-I-02.
- **Archivos esperados:** futuros `infra/networking.tf`, `variables.tf`, `outputs.tf`.
- **Acciones futuras:** definir CIDR, subnets públicas/privadas, route tables e Internet Gateway; distinguir ALB, hosts y Redis. No aplicar todavía.
- **Aceptación:** topología revisada en dos AZ, sin solapamientos y con rutas/propósitos verificables; funcionamiento real queda para 027–028.
- **Evidencia:** E(T-INFRA-003), diagrama y revisión IaC/red.
- **Riesgo:** subnets privadas sin salida o exposición accidental por rutas.
- **Gate asociado:** GATE-I-B.

### T-INFRA-004 — Salida estable y acceso Atlas/AWS

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-2 Networking.
- **Objetivo:** habilitar egreso seguro y reproducible sin Atlas abierto globalmente.
- **Dependencias:** T-INFRA-003; DP-I-03/06.
- **Trazabilidad:** INFRA-008, 009, 014; PLD-INFRA-004/008; AC-I-02, 06.
- **Archivos esperados:** futuros `infra/networking.tf`, `security.tf`, `variables.tf`, `outputs.tf`, solo referencias no secretas.
- **Acciones futuras:** comparar EC2 con salida fija frente a privada/NAT/endpoints y sus costos; definir DNS y salidas autorizadas para Atlas, ECR, SSM, logs, secretos y authority.
- **Aceptación:** perfil aprobado, rutas/salidas y allowlist especificadas sin `0.0.0.0/0` en Atlas; prueba de conectividad real reservada a 028.
- **Evidencia:** E(T-INFRA-004), comparación de costo/red y reglas propuestas saneadas.
- **Riesgo:** IP cambiante, NAT/IPv4 costosos o dependencia sin conectividad.
- **Gate asociado:** GATE-I-B, soporte GATE-I-F.

### T-INFRA-005 — Roles y permisos mínimos

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-3 Security/IAM.
- **Objetivo:** separar provisión, deployment, runtime, administración y bootstrap.
- **Dependencias:** T-INFRA-003, T-INFRA-004; DP-I-08/09 y contrato preliminar 004.
- **Trazabilidad:** INFRA-008, 021, 022, 024; PLD-INFRA-005/011/012; AC-I-02, 08, 09.
- **Archivos esperados:** futuros `infra/security.tf`, `authority.tf`, `secrets.tf`, referencias de registry/compute.
- **Acciones futuras:** inventariar llamadas reales; diseñar IAM por recurso/acción, roles temporales/SSM y límites OIDC para futuro deployment. No crear trust/workflows ahora.
- **Aceptación:** revisión de mínimo privilegio y separación de roles; runtime sin permisos de creación/borrado de tabla ni administración global.
- **Evidencia:** E(T-INFRA-005), matriz rol→acción→recurso y revisión.
- **Riesgo:** privilegio excesivo, trust abierto o secretos de cuenta persistentes.
- **Gate asociado:** GATE-I-B, GATE-I-F.

### T-INFRA-006 — SG, administración y transporte interno

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-3 Security/IAM.
- **Objetivo:** evitar bypass de ALB y administración pública.
- **Dependencias:** T-INFRA-004, T-INFRA-005; DP-I-03/05/09.
- **Trazabilidad:** INFRA-008, 009, 013, 024; PLD-INFRA-004/005/007; AC-I-02, 05, 09.
- **Archivos esperados:** futuros `infra/security.tf`, `networking.tf`, `load-balancer.tf`.
- **Acciones futuras:** definir ingreso backend solo SG ALB, Redis privado/solo clientes aprobados, egreso y acceso SSM; resolver TLS interno y puertos con criterio de riesgo.
- **Aceptación:** reglas revisadas sin SSH público ni administración Docker; matriz de pruebas de acceso directo/denegación prevista para 028.
- **Evidencia:** E(T-INFRA-006), revisión SG/egreso y decisión TLS interno.
- **Riesgo:** API directa, Redis público, egress indiscriminado o TLS asumido por EXPOSE.
- **Gate asociado:** GATE-I-B, GATE-I-E.

### T-INFRA-007 — Registry ECR

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-4 Registry.
- **Objetivo:** definir repositorio de artefactos inmutables sin introducir pipeline.
- **Dependencias:** T-INFRA-002, T-INFRA-005, T-INFRA-025; DP-I-14.
- **Trazabilidad:** INFRA-005, 024, 031; PLD-INFRA-003/015; AC-I-03, 12, 13.
- **Archivos esperados:** futuros `infra/registry.tf`, `variables.tf`, `outputs.tf`, políticas en `security.tf`.
- **Acciones futuras:** definir ECR, acceso push/pull, retención compatible con rollback y referencias por digest; distinguir rol build/deploy/runtime.
- **Aceptación:** configuración revisada y output no secreto; no imágenes publicadas ni registry creado por la preparación.
- **Evidencia:** E(T-INFRA-007), revisión IaC y contrato de permisos/retención.
- **Riesgo:** borrado del digest de rollback, tags mutables o permisos amplios.
- **Gate asociado:** GATE-I-C, GATE-I-H.

### T-INFRA-008 — Contrato de imagen y ownership del digest

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-4 Registry.
- **Objetivo:** garantizar mismo artefacto reproducible y promoverlo manualmente desde 004.
- **Dependencias:** T-INFRA-007; DP-I-02/14, contrato aprobado con futura 004.
- **Trazabilidad:** INFRA-005, 006, 034, 035; PLD-INFRA-003/017/018; AC-I-03, 13.
- **Archivos esperados:** contrato futuro en `specs/003-infrastructure/plan.md` y referencias `infra/compute.tf`/`variables.tf`. Dockerfile y workflows fuera de esta tarea.
- **Acciones futuras:** revisar Dockerfile existente, .NET 10/non-root/configuración; definir commit→digest→CI verde, writer único del digest y retención previa. Solicitar aparte cualquier cambio Docker necesario.
- **Aceptación:** contrato aprobado de artefacto inmutable idéntico en A/B y deploy workflow_dispatch tras CI verde; sin build, push o YAML implícitos.
- **Evidencia:** E(T-INFRA-008), contrato y revisión documental de imagen.
- **Riesgo:** drift Terraform/CD, imagen no rastreable o secretos en capas.
- **Gate asociado:** GATE-I-C, GATE-I-H, GATE-I-I.

### T-INFRA-009 — Configuración de dos instancias

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-5 Compute.
- **Objetivo:** definir dos hosts Linux Docker en AZ distintas con capacidad suficiente.
- **Dependencias:** T-INFRA-006, T-INFRA-008; DP-I-02/03.
- **Trazabilidad:** INFRA-002, 004–006, 039; PLD-INFRA-002/003; AC-I-03.
- **Archivos esperados:** futuros `infra/compute.tf`, `variables.tf`, `outputs.tf`, referencias IAM/red.
- **Acciones futuras:** configurar cómputo/volúmenes/roles y exposición de contenedor, mismo digest y capacidad mínima dos; bootstrap sin secretos en user-data. No crear instancias hasta 027.
- **Aceptación:** IaC expresa dos EC2/dos AZ, sin puertos abiertos al público ni secreto; capacidad elegida justificada sin debilitar hasher. Existencia real exigida por gate en 027–028.
- **Evidencia:** E(T-INFRA-009), revisión topología/capacidad y configuración.
- **Riesgo:** CPU/memoria insuficientes, una sola máquina disfrazada de dos instancias o bootstrap inseguro.
- **Gate asociado:** GATE-I-C.

### T-INFRA-010 — Recuperación, mantenimiento y escalado

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-5 Compute.
- **Objetivo:** definir recuperación y escalado sin perder estado de seguridad compartido.
- **Dependencias:** T-INFRA-009; DP-I-02/07/15.
- **Trazabilidad:** INFRA-009, 019, 035, 039; PLD-INFRA-002/010/018; AC-I-03, 07, 13.
- **Archivos esperados:** futuros `infra/compute.tf`, `observability.tf` y procedimiento en `specs/003-infrastructure/plan.md`.
- **Acciones futuras:** diseñar reemplazo y drenaje, capacidad durante mantenimiento, escalado manual inicial y actualización de salidas Atlas; no reiniciar cuotas por host ni activar autoscaling sin decisión.
- **Aceptación:** runbook con fallos/rollback y límites de disponibilidad explícitos; escenarios de recuperación definidos para prueba 028, no promesa de cero downtime.
- **Evidencia:** E(T-INFRA-010), procedimiento y revisión de escenarios/costo.
- **Riesgo:** quedar con una instancia, nueva IP bloqueada en Atlas o reabrir seguridad por reinicio.
- **Gate asociado:** GATE-I-C, GATE-I-H.

### T-INFRA-011 — ALB y target group

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-6 Balanceador.
- **Objetivo:** expresar balanceo público hacia dos targets privados por SG.
- **Dependencias:** T-INFRA-003, T-INFRA-006, T-INFRA-009; DP-I-05.
- **Trazabilidad:** INFRA-010, 011, 031; PLD-INFRA-006; AC-I-04.
- **Archivos esperados:** futuros `infra/load-balancer.tf`, `variables.tf`, `outputs.tf`.
- **Acciones futuras:** definir ALB/subnets/target group `instance`, registro A/B, puerto/protocolo y drenaje; no requerir stickiness ni admisión pública previa a TLS/dependencias.
- **Aceptación:** configuración revisada consistente con dos AZ y reglas SG; la distribución real se acredita solo en 028.
- **Evidencia:** E(T-INFRA-011), revisión ALB/targets/puertos.
- **Riesgo:** target incorrecto, ALB vacío o afinidad que oculte cuotas no distribuidas.
- **Gate asociado:** GATE-I-D.

### T-INFRA-012 — Distribución, drenaje y fallos de targets

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-6 Balanceador.
- **Objetivo:** definir comportamiento seguro de health y cambios de instancia.
- **Dependencias:** T-INFRA-010, T-INFRA-011; DP-I-05/15 para el diseño inicial. El contrato health definitivo se integra después en 023–024 y su verificación runtime en 028, sin dependencia circular.
- **Trazabilidad:** INFRA-010, 011, 019, 027, 028; PLD-INFRA-006/014; AC-I-04, 11.
- **Archivos esperados:** futuros `infra/load-balancer.tf` y contrato/escenarios en `specs/003-infrastructure/plan.md`.
- **Acciones futuras:** fijar timeouts/intervalos/matcher/draining, escenarios un target unhealthy y todos unhealthy, y atribución interna A/B para pruebas. No reintentos automáticos de POST.
- **Aceptación:** parámetros y matriz aprobados, incluyendo ALB fail-open y fail-closed de aplicación; no usar solo liveness como readiness. Ejecución real en 028.
- **Evidencia:** E(T-INFRA-012), contrato de checks/fallos y criterios de resultado.
- **Riesgo:** asumir que ALB bloquea todo tráfico cuando todos están unhealthy.
- **Gate asociado:** GATE-I-D.

### T-INFRA-013 — Dominio, DNS y ACM

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-7 HTTPS.
- **Objetivo:** resolver identidad HTTPS pública verificable.
- **Dependencias:** T-INFRA-001, T-INFRA-011; DP-I-04.
- **Trazabilidad:** INFRA-012, 031; PLD-INFRA-007; AC-I-05.
- **Archivos esperados:** futura configuración ACM/DNS aprobada en `infra/load-balancer.tf` o `main.tf`, inputs/outputs no secretos.
- **Acciones futuras:** obtener decisión de dominio/subdominio y proveedor DNS, titular/permisos y validación/renovación ACM regional. Definir registros hacia ALB; no inventar dominio.
- **Aceptación:** ownership/autorización y configuración DNS/ACM revisadas; no afirmar certificado emitido antes de 027–028.
- **Evidencia:** E(T-INFRA-013), decisión DNS y revisión saneada de validación/renovación.
- **Riesgo:** falta de control DNS, región errónea, costo de dominio o certificado sin renovar.
- **Gate asociado:** GATE-I-E.

### T-INFRA-014 — Listeners, TLS y forwarding

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-7 HTTPS.
- **Objetivo:** evitar publicación insegura o identidad de origen falsificable.
- **Dependencias:** T-INFRA-006, T-INFRA-012, T-INFRA-013; DP-I-05/11.
- **Trazabilidad:** INFRA-012, 013, 028; PLD-INFRA-007/014; AC-I-05, 11.
- **Archivos esperados:** futuros `infra/load-balancer.tf`, `security.tf`, `variables.tf`; contrato en `plan.md`, sin middleware modificado aquí.
- **Acciones futuras:** especificar 443 con ACM, 80 solo redirect, TLS interno y proxies conocidos; definir pruebas de cadena/hostname/expiración, spoofing y bucles HTTPS para 028.
- **Aceptación:** política TLS/transporte/proxy aprobada y compatible con cuotas por origen; falta de implementación forwarding bloquea publicación hasta 024.
- **Evidencia:** E(T-INFRA-014), revisión listeners y plan de verificación de transporte/origen.
- **Riesgo:** confiar en X-Forwarded-For del cliente o atribuir HTTPS a EXPOSE.
- **Gate asociado:** GATE-I-E.

### T-INFRA-015 — Atlas: red, usuario y TLS

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-8 Atlas.
- **Objetivo:** definir acceso NoSQL seguro por ambiente.
- **Dependencias:** T-INFRA-004, T-INFRA-005; DP-I-06/09.
- **Trazabilidad:** INFRA-009, 014, 022, 033; PLD-INFRA-008/012; AC-I-06, 09.
- **Archivos esperados:** futuras referencias en `infra/networking.tf`, `secrets.tf`, `variables.tf`; decisión Atlas en `spec.md`.
- **Acciones futuras:** aprobar cluster/tier/región y ownership, DB/usuario mínimo y salidas permitidas, TLS validado y connection string externa. Provider Atlas adicional solo por autorización explícita.
- **Aceptación:** contrato/red/permisos/costo revisados sin credenciales ni supuesta gratuidad; creación/ingreso de secretos se reserva a 027 con permiso específico.
- **Evidencia:** E(T-INFRA-015), revisión Atlas y decisiones de provisión/backup.
- **Riesgo:** tier incompatible, TLS deshabilitado, allowlist abierta o usuario excesivo.
- **Gate asociado:** GATE-I-F.

### T-INFRA-016 — Atlas: índices, ACK y concurrencia

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-8 Atlas.
- **Objetivo:** preservar contratos de persistencia de 002 en la configuración cloud.
- **Dependencias:** T-INFRA-015; contrato de secretos coordinado con T-INFRA-019 antes de 026.
- **Trazabilidad:** INFRA-015, 040; PLD-INFRA-008/020; AC-I-06, 14.
- **Archivos esperados:** referencias/configuración futura en `infra/compute.tf` y plan de verificación en `specs/003-infrastructure/plan.md`; sin cambios de repositorio Mongo o tests.
- **Acciones futuras:** revisar índices/BSON/read-write concerns/ACK e inicialización existente, permisos separados y escenarios duplicado concurrente/lectura A-B. No relajar garantías por tier.
- **Aceptación:** configuración y matriz de pruebas compatibles con 002 revisadas; prueba runtime se registra en 028, no se infiere de tests locales.
- **Evidencia:** E(T-INFRA-016), checklist de contratos e inicialización y plan cross-instance.
- **Riesgo:** admitir antes del índice, duplicados o éxito sin ACK acreditado.
- **Gate asociado:** GATE-I-F, GATE-I-I.

### T-INFRA-017 — Opción Redis y compatibilidad

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-9 Redis.
- **Objetivo:** seleccionar opción viable sin alterar semántica de seguridad.
- **Dependencias:** T-INFRA-001, T-INFRA-004, T-INFRA-005; DP-I-07.
- **Trazabilidad:** INFRA-003, 016–018; PLD-INFRA-009; AC-I-07.
- **Archivos esperados:** futuros `infra/redis.tf`, `variables.tf` y decisión en `spec.md`.
- **Acciones futuras:** comparar ElastiCache nodos frente a Redis dedicado EC2 privada; evaluar costo total, versión/topología, TLS/ACL, TIME, Lua, atomicidad/noeviction y restricciones de comandos administrativos.
- **Aceptación:** selección aprobada con matriz de compatibilidad a comprobar en 028; no Serverless/Valkey/Cluster por equivalencia asumida y sin afirmar HA del nodo único.
- **Evidencia:** E(T-INFRA-017), decisión/costo y matriz de comandos/parámetros/adapter.
- **Riesgo:** servicio "compatible" que no permita scripts/reloj/política requeridos.
- **Gate asociado:** GATE-I-F.

### T-INFRA-018 — Redis: capacidad, continuidad y recuperación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-9 Redis.
- **Objetivo:** dimensionar y definir seguridad ante pérdida o resultado incierto.
- **Dependencias:** T-INFRA-010, T-INFRA-017; contratos de authority T-INFRA-020 antes de 026; DP-I-07/15.
- **Trazabilidad:** INFRA-016–019, 026, 035, 040; PLD-INFRA-009/010/018; AC-I-07, 11, 14.
- **Archivos esperados:** futuros `infra/redis.tf`, `observability.tf` y runbook en `plan.md`.
- **Acciones futuras:** definir memoria total/reserva/noeviction/topes, persistencia y señales/fencing/cuarentena; separar recuperación de cuotas/revocaciones, sin ledger externo. Diseñar pruebas autorizadas de pérdida/failover para 028.
- **Aceptación:** capacidad, RTO/RPO/límites y recuperación aprobados; 64 MiB de cuotas no usados como total. Riesgo silencioso residual y AUTH-027 no se declaran resueltos.
- **Evidencia:** E(T-INFRA-018), dimensionamiento y modelo de fallos/runbook con límites de detección.
- **Riesgo:** pérdida de marca vigente, cuotas reiniciadas, apertura prematura o promesa falsa de durabilidad.
- **Gate asociado:** GATE-I-F, GATE-I-H.

### T-INFRA-019 — Secretos externos, entrega y rotación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-10 Configuración.
- **Objetivo:** mantener secretos runtime fuera de Git/imagen/state y distribuirlos de forma coordinada.
- **Dependencias:** T-INFRA-005, T-INFRA-008; DP-I-09/14.
- **Trazabilidad:** INFRA-006, 022–024, 032, 033; PLD-INFRA-012/016/018; AC-I-09, 12, 13.
- **Archivos esperados:** futuros `infra/secrets.tf`, `security.tf`, `compute.tf`, inputs solo de referencias.
- **Acciones futuras:** evaluar Secrets Manager preferido frente a Parameter Store; definir JWT, Atlas, Redis, HMAC y otros secretos, KMS/IAM, entrega segura, actualización/rotación y separación por ambiente. Valores por flujo externo aprobado, nunca Terraform/user-data.
- **Aceptación:** contrato aprobado sin valores secretos; JWT mantiene verificadoras vigentes y HMAC no resetea ventanas. GitHub solo deployment/OIDC, no fuente runtime.
- **Evidencia:** E(T-INFRA-019), inventario por finalidad sin valores, revisión IAM/entrega/rotación.
- **Riesgo:** secreto en state/logs/disco, clave compartida entre ambientes o rotación inconsistente A/B.
- **Gate asociado:** GATE-I-B, GATE-I-F, GATE-I-H.

### T-INFRA-020 — Tabla authority y bootstrap controlado

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-10 Configuración.
- **Objetivo:** contemplar DynamoDB authority existente, no un sustituto.
- **Dependencias:** T-INFRA-005, T-INFRA-019; DP-I-08/09.
- **Trazabilidad:** INFRA-020, 021, 024, 026; PLD-INFRA-005/011; AC-I-08, 09.
- **Archivos esperados:** futuros `infra/authority.tf`, `security.tf`, `variables.tf`, `outputs.tf`; procedimiento en `plan.md`.
- **Acciones futuras:** verificar esquema/clave del adapter, nombre/región/capacidad, lecturas consistentes/condicionales, roles runtime/bootstrap, protección y observabilidad. Definir bootstrap y recuperación; ejecutarlos solo en 027 bajo permiso explícito.
- **Aceptación:** configuración y procedimiento revisados sin initializer alternativo, ledger de tokens ni reapertura por arranque. Ausencia/recreación de tabla no se considera estado confiable.
- **Evidencia:** E(T-INFRA-020), mapping adapter→tabla/permisos y aprobación de bootstrap.
- **Riesgo:** autoridad vacía/antigua, privilegios de borrado runtime o confusión con lock Terraform.
- **Gate asociado:** GATE-I-B, GATE-I-F.

### T-INFRA-021 — Logs y métricas CloudWatch

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-11 Observabilidad.
- **Objetivo:** definir telemetría útil, saneada y con costos controlados.
- **Dependencias:** T-INFRA-009, T-INFRA-011, T-INFRA-017, T-INFRA-019, T-INFRA-020; DP-I-10.
- **Trazabilidad:** INFRA-025, 026, 037; PLD-INFRA-013/020; AC-I-10, 15.
- **Archivos esperados:** futuros `infra/observability.tf`, referencias en `compute.tf`/`security.tf`; agentes/config complementaria requieren alcance explícito.
- **Acciones futuras:** especificar entrega logs, CPU/memoria/estado, targets, errores/latencia, Redis/authority; correlación A/B interna y etiquetas acotadas. No añadir Serilog u otro stack por iniciativa propia.
- **Aceptación:** cobertura/privacidad/permisos revisados, requisitos de agente/exportación identificados; señal operativa real solo en 028.
- **Evidencia:** E(T-INFRA-021), matriz señal→origen→destino→acceso/costo.
- **Riesgo:** logs de credenciales/PII, cardinalidad elevada o asumir memoria disponible sin agente.
- **Gate asociado:** GATE-I-G.

### T-INFRA-022 — Alarmas, retención y operación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-11 Observabilidad.
- **Objetivo:** fijar alertas accionables y retención antes de publicar.
- **Dependencias:** T-INFRA-018, T-INFRA-021; DP-I-10/13.
- **Trazabilidad:** INFRA-003, 025, 026, 037; PLD-INFRA-001/013/020; AC-I-10, 15.
- **Archivos esperados:** futuros `infra/observability.tf`, `variables.tf`; procedimiento autorizado en `plan.md`.
- **Acciones futuras:** aprobar retención/umbrales/destinatario, alarmas capacidad Redis/targets/errores/authority, acceso y límites de costo; preparar prueba controlada y recuperación de alarma para 028.
- **Aceptación:** valores/responsables aprobados, sin datos sensibles en notificaciones; ninguna alarma se considera operativa sin prueba posterior.
- **Evidencia:** E(T-INFRA-022), aprobación de umbrales/retención y matriz de respuesta.
- **Riesgo:** alertas sin destinatario, fatiga/omisiones, costo o retención indebida.
- **Gate asociado:** GATE-I-G.

### T-INFRA-023 — Contrato liveness/readiness y proxy

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-12 Health.
- **Objetivo:** concretar requisitos de aplicación faltantes sin implementarlos en 003.
- **Dependencias:** T-INFRA-012, T-INFRA-014, T-INFRA-016, T-INFRA-018, T-INFRA-020, T-INFRA-022; DP-I-05/11/15.
- **Trazabilidad:** INFRA-011, 013, 019, 027, 028; PLD-INFRA-007/010/014; AC-I-04, 05, 11.
- **Archivos esperados:** contrato y solicitud futura delimitada en `specs/003-infrastructure/spec.md`/`plan.md`; no código ni tests.
- **Acciones futuras:** definir ruta candidata readiness, salida mínima, índices/dependencias/inicialización/admisión segura, confianza proxy/origen y uso ALB. Mantener `/health` liveness y Logout sin dependencia Mongo añadida.
- **Aceptación:** contrato revisado con 002 y alcance de implementación separado aprobado; ningún ping o retirada local sustituye continuidad/fencing compartidos.
- **Evidencia:** E(T-INFRA-023), contrato, revisión de seguridad y autorización separada propuesta/otorgada según realidad.
- **Riesgo:** declarar listo un endpoint inexistente o falsear origen/cuotas mediante forwarding.
- **Gate asociado:** GATE-I-D, GATE-I-E, GATE-I-F.

### T-INFRA-024 — Evidencia de readiness/forwarding antes de cloud

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-12 Health.
- **Objetivo:** aceptar la dependencia de aplicación solo con evidencia propia autorizada.
- **Dependencias:** T-INFRA-023; incremento externo de aplicación con autorización explícita y evidencia aceptada. Sin este incremento, BLOQUEADA al intentar ejecutarla.
- **Trazabilidad:** INFRA-011, 013, 027, 028, 038; PLD-INFRA-014/020; AC-I-04, 05, 11.
- **Archivos esperados:** referencias futuras de evidencia en `specs/003-infrastructure/task.md`; ruta aprobada en `infra/load-balancer.tf`. Ningún archivo productivo/tests modificado por esta tarea.
- **Acciones futuras:** revisar commit/artefacto y pruebas del incremento separado para readiness/forwarding, fallos/spoofing y contratos de 002; alinear configuración ALB sin inventar éxito.
- **Aceptación:** evidencia aceptada de capacidad implementada y compatible, sin declarar todavía validación cloud. A falta de autorización/implementación, no avanzar a 026/027.
- **Evidencia:** E(T-INFRA-024), referencia íntegra y saneada al incremento, resultados y revisión de integración.
- **Riesgo:** eludir prohibición de modificar Authentication/tests o desplegar readiness solo nominal.
- **Gate asociado:** GATE-I-D, GATE-I-E, GATE-I-F.

### T-INFRA-025 — Validación inicial de baseline Terraform

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-13 Terraform, intervención temprana transversal.
- **Objetivo:** acreditar baseline antes de extender IaC.
- **Dependencias:** T-INFRA-001, T-INFRA-002; autorización explícita de comandos/entorno, DP-I-12.
- **Trazabilidad:** INFRA-029, 032, 038; PLD-INFRA-015/016/019; AC-I-01.
- **Archivos esperados:** futura baseline `infra/*.tf` de 002 y lockfile de providers si se autoriza; state/planes nunca Git.
- **Acciones futuras:** revisar formato, inicialización/validación y versiones/locking mediante comandos autorizados; registrar salida nativa saneada. No apply ni provisión; cualquier descarga/init requerirá permiso futuro.
- **Aceptación:** baseline validada realmente, state/backup protegidos y versiones acreditadas; validación local no prueba cloud ni toda la IaC futura.
- **Evidencia:** E(T-INFRA-025), comandos/entorno/versiones/resultados y revisión.
- **Riesgo:** inicialización con backend equivocado, descarga no aprobada o secretos en salida.
- **Gate asociado:** GATE-I-A.

### T-INFRA-026 — Revisión final, plan, costo y autorización de provisión

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-13 Terraform.
- **Objetivo:** impedir apply sin revisión integral de configuración, seguridad y costo.
- **Dependencias:** T-INFRA-001 a T-INFRA-025 aceptadas; DP bloqueantes resueltas; autorización de validación/plan. No depende de recursos cloud todavía.
- **Trazabilidad:** INFRA-003, 029–036, 038; PLD-INFRA-015–019; AC-I-01, 12, 13.
- **Archivos esperados:** futuros `infra/*.tf`, contrato de inputs/outputs y revisión en `plan.md`; plan/state protegidos fuera de Git.
- **Acciones futuras:** validar conjunto completo, revisar plan y cambios/destrucciones, drift/importación, costos y ownership digest. Antes de GitHub/uso compartido exigir backend remoto/versionado/locking y migración autorizada, aunque el baseline fuera local.
- **Aceptación:** plan/costo/versiones/red/secretos/health revisados y aprobación específica de apply, bootstrap y permisos registrados. Ninguna destrucción ni recurso fuera del alcance aprobado.
- **Evidencia:** E(T-INFRA-026), validación, resumen saneado de plan, estimación y autorización; conservar plan real de forma protegida.
- **Riesgo:** cargos/destrucciones imprevistos, dos escritores de digest, state local en runner o plan público con secretos.
- **Gate asociado:** GATE-I-H; revalidación de GATE-I-A.

### T-INFRA-027 — Provisión y despliegue manual autorizados

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-14 Cloud.
- **Objetivo:** materializar recursos y desplegar el artefacto sin automatizar promoción por push.
- **Dependencias:** T-INFRA-026; dominio/certificado y permisos aprobados; integración futura 004 aprobada/operativa, CI verde del mismo commit/digest, workflow_dispatch manual y state remoto protegido; autorización AWS/Atlas/apply/bootstrap/secretos delimitada.
- **Trazabilidad:** INFRA-001, 002, 004–024, 027–035, 038, 039; PLD-INFRA-001–019; AC-I-02–09, 11–13.
- **Archivos esperados:** futuros `infra/*.tf` revisados y referencias de ejecución/evidencia en `task.md`; sin YAML, código, Docker o tests nuevos en esta tarea.
- **Acciones futuras:** bajo permisos específicos aplicar solo lo revisado, habilitar Atlas/secretos por flujo seguro, bootstrap authority controlado y desplegar manualmente desde GitHub. Verificar inventario dos EC2/AZ, mismo digest, TLS, targets/config y señales iniciales. No reabrir confianza por deploy.
- **Aceptación:** provisión/deployment manual reales trazables y saneados, sin recursos no autorizados; existencia de dos hosts acreditada. No declarar aceptación pública integral antes de 028–030.
- **Evidencia:** E(T-INFRA-027), autorizaciones, apply/recursos, referencia workflow_dispatch, CI/commit/digest y controles iniciales sin secretos.
- **Riesgo:** gasto real, bootstrap inseguro, exposición prematura o deployment automático/no revisado. Detener admisión ante incertidumbre.
- **Gate asociado:** GATE-I-B a GATE-I-I; no cierre automático.

### T-INFRA-028 — Verificación pública, distribuida y rollback

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-14 Cloud.
- **Objetivo:** demostrar el comportamiento cloud, no extrapolar evidencia local.
- **Dependencias:** T-INFRA-027; autorización de objetivos/ventana/casos negativos y fallos; DP-I-15; superficie protegida autorizada para prueba de revocación normal.
- **Trazabilidad:** INFRA-001–028, 034–040; PLD-INFRA-002–014/017–020; AC-I-02–15.
- **Archivos esperados:** referencias futuras en `task.md`/`plan.md`; resultados/capturas saneados en ubicación protegida aprobada. Sin endpoint, test productivo o workflow nuevo.
- **Acciones futuras:** probar URL HTTPS, A/B sin stickiness, SG/SSM/bypass/origen, health y todos unhealthy, Atlas/índices/ACK/concurrencia, Redis TIME/Lua/noeviction/capacidad, authority/secretos y alarmas. Validar Register 201, Login 200, Logout 204 y negativos/no-store/challenge/cuotas/Retry-After/503 conforme 002, revocación cruzada y rollback seguro. Fallos/reapertura solo bajo modelo aprobado.
- **Aceptación:** evidencia real de todos los casos, dos instancias y mismas garantías; ningún secreto en salida. Revocación se prueba por política normal, no por excepción Logout. Fallos sin bypass, sin restaurar state antiguo; límites silenciosos explícitos.
- **Evidencia:** E(T-INFRA-028), matriz caso→AC→resultado con atribución interna A/B, logs/métricas/alarma y rollback saneados; registrar cualquier bloqueo/fallo sin ocultarlo.
- **Riesgo:** datos/usuarios reales, cuotas afectadas, diagnóstico público, falsa garantía de continuidad o prueba destructiva no autorizada.
- **Gate asociado:** GATE-I-B a GATE-I-I.

### T-INFRA-029 — Paquete de evidencia académica

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-15 Evidencia.
- **Objetivo:** permitir verificación de rúbrica, seguridad y costos por un tercero autorizado.
- **Dependencias:** T-INFRA-028; DP-I-13 y política de retención/acceso aprobada.
- **Trazabilidad:** INFRA-002, 003, 025, 026, 037, 038, 040; PLD-INFRA-001/013/020; AC-I-10, 12–15.
- **Archivos esperados:** índice de referencias futuro en `specs/003-infrastructure/task.md`; paquete externo protegido, no state/secretos/volcados en Git.
- **Acciones futuras:** reunir diagrama, mapping IaC/recursos, dos instancias/AZ/digest/targets, HTTPS, NoSQL/Redis/authority, pruebas, logs/alarma y costos; relacionar cada E con requisito/PLD/AC/gate y separar local/cloud.
- **Aceptación:** paquete accesible al evaluador autorizado, saneado, fechado/trazable y con ubicación/retención comprobadas; ningún screenshot aislado sustituye prueba distribuida.
- **Evidencia:** E(T-INFRA-029), índice/manifiesto y revisión de privacidad/completitud.
- **Riesgo:** secretos/PII/identificadores innecesarios, referencias temporales perdidas o evidencia cloud no reproducible.
- **Gate asociado:** GATE-I-H, GATE-I-I.

### T-INFRA-030 — Reproducibilidad y revisión final de aceptación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-15 Evidencia.
- **Objetivo:** revisar aceptación de infraestructura sin aprobar retrospectivamente otros alcances.
- **Dependencias:** T-INFRA-026, T-INFRA-028, T-INFRA-029; autorización específica de reproducción/drift/recuperación, presupuesto y protección de datos.
- **Trazabilidad:** INFRA-002, 003, 029–040; PLD-INFRA-015–020; AC-I-01, 12–15.
- **Archivos esperados:** actualización futura autorizada de `spec.md`, `plan.md`, `task.md`; evidencia externa protegida. Otros documentos solo mediante permiso específico.
- **Acciones futuras:** demostrar reproducción controlada con IaC y referencias externas/secretas aprobadas, drift y recuperación/rollback sin borrar datos ni reiniciar seguridad; revisar AC y gates A–G, después H/I con el responsable. No destroy ni segunda infraestructura facturable implícitos.
- **Aceptación:** evidencia real de reproducción y revisión integral registrada, costos/límites claros y cierre solo de gates efectivamente acreditados/aprobados. 002 GATE-G/H y AUTH-027 conservan su proceso propio.
- **Evidencia:** E(T-INFRA-030), demostración autorizada, revisión final requisito→PLD→tarea→evidencia→AC→gate y decisión del responsable.
- **Riesgo:** destrucción de datos, cargos duplicados o declarar producción/seguridad integral por cerrar una demostración académica.
- **Gate asociado:** GATE-I-H, GATE-I-I.

## 5. Cobertura y revisión documental

| Requisitos | Tareas responsables principales |
| --- | --- |
| INFRA-001–003 | 001, 009, 026–030 |
| INFRA-004–006 | 008–010, 019, 027–028 |
| INFRA-007–009 | 003–006, 015, 027–028 |
| INFRA-010–013 | 006, 011–014, 023–024, 027–028 |
| INFRA-014–015 | 015–016, 027–028 |
| INFRA-016–019 | 017–018, 020, 023–024, 027–028 |
| INFRA-020–021 | 005, 020, 027–028 |
| INFRA-022–024 | 005–006, 019–020, 027–028 |
| INFRA-025–026 | 018, 020–022, 027–029 |
| INFRA-027–028 | 012, 014, 023–024, 027–028 |
| INFRA-029–033 | 001–002, 025–027, 030 |
| INFRA-034–036 | 008, 010, 018–019, 026–028, 030 |
| INFRA-037–040 | 001, 009–010, 016, 018, 021–030 |

Cobertura documental: 40 requisitos INFRA, 20 decisiones propuestas PLD-INFRA, 15 criterios AC-I, 15 fases, 30 tareas y 9 gates. **Cobertura no equivale a implementación, prueba o satisfacción.** Todas las tareas siguen PENDIENTE / NO EJECUTADA y todos los gates PENDIENTE / NO SATISFECHO. No hay E de ejecución registrada.

Se revisa coherencia con 001 sin completarlo, con 002 sin cambiar Authentication/semántica Redis/JWT ni su autoridad DynamoDB, y con el límite entre infraestructura y 004. Readiness/forwarding siguen pendientes de incremento separado; dominio/precios/versiones/retención/umbrales y demás DP permanecen abiertos. No se declara cloud existente ni se incorporan secretos. Futuro deploy manual workflow_dispatch tras CI verde, nunca automático por push/merge.

Esta entrega termina al crear spec/plan/task y validar documentalmente su alcance. No ejecutar T-INFRA, Terraform, AWS, builds, pruebas o workflows; no crear `infra/`, Orders ni cambios Docker; sin stage, commit o push.
