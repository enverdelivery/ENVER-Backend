# Tareas 003 — Infraestructura de ENVER Backend

## 1. Control, autorización y estado inicial

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.13.0 / 2026-11-12 |
| Solicitud inicial / incremento actual | Antecedentes §§6–16; INFRA-4 ECR — IAM VALIDATION + REPLAN PRE-APPLY, VALIDACIÓN CONTROLADA / NO APPLY (§17) |
| Fuentes | [spec.md](spec.md) y [plan.md](plan.md), propuesta aprobada como base documental |
| Catálogo | 30 tareas, T-INFRA-001 a T-INFRA-030 |
| Estado vigente de tareas | 001/002 conservan estado parcial; 003–007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE; 008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE; 025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE; otras 21 PENDIENTE / NO EJECUTADA |
| Estado de todos los gates | **PENDIENTE / NO SATISFECHO** |
| Evidencia de ejecución de 003 | §14 conserva apply red 14/0/0 y post-plan sin cambios; §15 build local sin push. §17: ECR policy adjunta manualmente según responsable; STS/scanning BASIC exit 0, listado general denegado y describe por nombre NotFound; fmt-check/validate exit 0, replan exacto 2/0/0 y red14 no-op, sin apply. Escritura/scanning de imagen y aceptación integral pendientes |
| Baseline documental original | `feature/authentication`, `c58ea86e3eddff4befa8f61398dd58d55c4ef985` |
| Baseline del BUILD actual | `feature/infrastructure`, creada desde `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`, árbol inicialmente limpio |
| Checkpoint previo a INFRA-2/3 | `feature/infrastructure`, `611071f0ecb0d6cb67993f4e9bffd00d328a2f90`, árbol limpio al iniciar §9 |
| Checkpoint previo a preparación IAM apply | `feature/infrastructure`, `f1a98a01e15c67297287133c27dc23279a649d09`, árbol limpio al iniciar §12 |
| Checkpoint pre-apply | `feature/infrastructure`, `0958e83b887a73e1bdae2ff8b25753f5a52d0f0c`, árbol limpio/0 staged al iniciar §14 |

Crear este catálogo **no autoriza ejecutar tareas**, cambiar código/configuración/Docker/tests, crear `infra/`, recursos AWS/Atlas, workflows o secretos, ni correr Terraform. No stage/commit/push. Los únicos archivos de esta entrega son los tres Markdown de 003. La autorización previa de la propuesta es documental, no habilita cargos ni credenciales.

El párrafo anterior conserva el alcance de la materialización inicial. La solicitud posterior explícita **BUILD T-INFRA-001 + T-INFRA-002 — BASELINE TERRAFORM** autoriza solo preparar estos dos incrementos, crear la baseline `infra/` y su lock, comprobar herramientas/credenciales sin revelar secretos, ejecutar version/fmt/init -backend=false/validate y registrar evidencia en este archivo y memoria.md. `.gitignore` solo si faltan reglas; ya existen. Autoriza crear rama, **no stage/commit/push, plan/apply, recursos AWS ni tareas 003–030**. No cambia los criterios originales de aceptación ni cierra decisiones pendientes por inferencia.

La solicitud posterior **CIERRE BASELINE AWS + TERRAFORM** autoriza validar perfil/identidad/región e inspeccionar permisos sin modificarlos, repetir version/fmt-check/validate y actualizar exclusivamente task/memoria (§7). No autoriza cerrar sin criterios satisfechos, modificar `infra/`, registrar credenciales, crear recursos/políticas, ejecutar plan/apply ni stage/commit/push. §6 conserva los resultados históricos; §7 es el estado de identidad/región vigente.

**CIERRE DE PRESUPUESTO + PREPARACIÓN INFRA-2/3** aprueba el límite operativo inicial de USD 50 con reserva objetivo mínima de USD 50 sobre USD 100 de crédito informado por el responsable (§8). Autoriza únicamente registro en task/memoria, reevaluación de criterios, recomendación documental y version/fmt-check/validate/Git de lectura. No autoriza ejecutar 003–006/025, modificar infraestructura/IAM, plan/apply, stage/commit/push o consumo AWS. §§6–7 permanecen como antecedentes; el presupuesto pendiente allí ya no es el estado vigente.

La solicitud posterior **INFRA-2 + INFRA-3 / T-INFRA-003 A T-INFRA-006 / BUILD CONTROLADO NO APPLY** autoriza expresamente preparar definiciones candidatas y bootstrap revisable, aunque 001/002/gates sigan abiertos, y ejecutar solo la parte posible de 025 sin recursos (§9). Esto permite preparación parcial, **no aceptación de dependencias incumplidas**, cierre de decisiones o provisión. Plan tiene condiciones expresas de permisos/inputs/decisiones: no se cumplen, no se ejecuta. No IAM aplicado, apply/destroy/import, pruebas backend, CI/CD ni stage/commit/push.

La solicitud posterior **INFRA-2/3 — REVALIDACIÓN IAM + TERRAFORM PLAN / VALIDACIÓN CONTROLADA NO APPLY** autoriza STS/AZ, fmt/check/validate y un intento de plan con inputs documentados; solo task/memoria como edición documental (§10). El responsable informa bootstrap aplicada; su contenido/asignación efectivos no se enumeran. Esta autorización no acepta DP-I-03 ni implica init/migración de backend, asignación IAM, apply o cierre de tareas/gates. §9 conserva la denegación y ausencia de plan de aquel turno como historial, no como resultado vigente.

La solicitud posterior **INICIALIZAR BACKEND LOCAL + PLAN INFRA-2/3 / VALIDACIÓN CONTROLADA NO APPLY** autoriza init exclusivamente local y **aprueba opción A para DISEÑO Y PLAN**, con los inputs y límites de §11. `network_design_approved=true` solo mediante input temporal; no modificación del ejemplo ni aceptación de provisión. No backend remoto/migración, IAM modificado, apply/destroy/import o cierre implícito de tareas/gates. §§9–10 conservan sus bloqueos como historial ya superado en consulta AZ/backend, no como estado vigente.

**PREPARAR IAM MÍNIMO PARA PRIMER APPLY INFRA-2/3** autoriza únicamente un JSON write separado, revisión de fuentes provider/IAM y actualización task/memoria, más fmt-check/validate. No modifica la read policy existente ni concede permisos, acepta gates o autoriza apply/destroy. El checkpoint Git anterior fue una autorización separada ya completada; en §12 no hay stage/commit/push ni llamadas a APIs de la cuenta AWS.

**VALIDAR IAM NETWORK APPLY + REPLAN PRE-APPLY** autoriza STS, DescribeNetworkAcls/DescribeNetworkInterfaces, revisión local de write policy, fmt-check/validate y un replan con los mismos inputs aprobados; solo task/memoria se editan (§13). El responsable informa asignación manual de Read/Apply. No se autorizan cambios IAM, dry-run de escritura, simulación/enumeración de políticas, init, apply/destroy/import, recursos manuales o stage/commit/push. La evidencia histórica de §12 conserva su condición de propuesta no aplicada en aquel momento; §13 registra el nuevo estado informado y las llamadas realmente verificadas.

**PRIMER APPLY CONTROLADO — NETWORK BASELINE INFRA-2/3** autoriza posteriormente **solo** generar/revisar un plan guardado de 14 creaciones y aplicar ese mismo archivo, post-plan y lecturas AWS del inventario (§14). Permite materializar red base/SG aunque la aceptación integral de tareas/gates siga pendiente: no aprueba retrospectivamente sus dependencias ni constituye despliegue de aplicación. No workloads, IAM ampliado, destroy/import, cambios de HCL o ejecución integral de 026–028; la evidencia puntual se registra aquí y en 003–006/025, sin declarar completados esos alcances. Únicas ediciones versionables autorizadas: task/memoria; state/plan locales ignorados, sin stage/commit/push. El checkpoint Git previo fue otra autorización completada; las prohibiciones de apply en §§6–13 conservan su alcance histórico.

**INFRA-4 — ECR / CONTAINER REGISTRY** autoriza posteriormente el alcance acotado de §15: registry.tf, tres outputs, propuesta IAM ECR separada, revisión Docker y build local sin push, fmt/check/validate/plan y registro task/memoria. No aplicar/adjuntar políticas, cambiar Dockerfile/producto, crear workloads/OIDC/workflows ni stage/commit/push. Preparación no acepta dependencias ni resuelve DP-I-14 integralmente; spec/plan e historial de red intactos.

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

Notación abreviada: E(001) = E(T-INFRA-001); E(003–006) exige cada una de las cuatro evidencias. §§6–17 registran solo los resultados y límites efectivamente acreditados; ninguna E parcial satisface por sí sola un gate. I-H/I-I no implican SLA, producción lista ni cierre automático de AUTH-027 o gates de 002.

## 4. Catálogo de tareas

### T-INFRA-001 — Cuenta, región, alcance y presupuesto

- **Estado:** PARCIAL / BLOQUEADA POR DECISIONES.
- **Fase:** INFRA-1 Baseline.
- **Objetivo:** fijar límites del ambiente cloud académico y responsables antes de cargos.
- **Dependencias:** autorización posterior del responsable; DP-I-01 y DP-I-13.
- **Trazabilidad:** INFRA-001–003, 033, 038; PLD-INFRA-001/020; AC-I-01, 15.
- **Archivos esperados:** decisiones autorizadas en `specs/003-infrastructure/spec.md` y `plan.md`; evidencia protegida externa.
- **Acciones futuras:** confirmar cuenta/región/AZ, acceso, nomenclatura, duración, presupuesto máximo, precios/créditos vigentes y propiedad de recursos. Estimar todos los servicios, sin asumir Free Tier.
- **Aceptación:** alcance, presupuesto y responsables aprobados por escrito; lista de cargos y recursos fuera de presupuesto, sin secretos.
- **Evidencia:** E(T-INFRA-001), aprobación y estimación fechada/regional saneada.
- **Resultado actual:** identidad/perfil/región, crédito informado USD 100, presupuesto USD 50/reserva objetivo USD 50 acreditados (§§7–8); §9 confirma proyecto enver/ambiente dev/us-east-1 y estimaciones parciales de red/IP; §10 acredita seis AZ reales disponibles. Faltan estimación integral por duración/uso, cobertura/vigencia del crédito, responsables/ownership/custodia, calendario y aceptación de decisiones restantes aplicables. No se satisface la aceptación completa ni se vuelve a marcar pendiente el monto aprobado.
- **Riesgo:** cargos o exposición de datos de cuenta; una alarma no limita gasto.
- **Gate asociado:** GATE-I-A; soporte de GATE-I-H/GATE-I-I.

### T-INFRA-002 — Baseline Terraform, versiones y state

- **Estado:** PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-1 Baseline.
- **Objetivo:** definir base IaC reproducible y custodia del state.
- **Dependencias:** T-INFRA-001; resolver DP-I-02, DP-I-12/13 aplicables.
- **Trazabilidad:** INFRA-029–033, 038; PLD-INFRA-015/016; AC-I-01, 12.
- **Archivos esperados:** futuros `infra/main.tf`, `providers.tf`, `versions.tf`, `variables.tf`, `outputs.tf`; configuración backend según decisión aprobada. State nunca versionado.
- **Acciones futuras:** aprobar versiones/provider AWS, validar naming/tags/inputs/outputs y definir state local protegido, backup cifrado/operador único. Planear backend remoto S3/locking compatible antes de GitHub/colaboración.
- **Aceptación:** baseline revisada, versiones y manejo de state aprobados; ningún secreto en tfvars/output; no se crea bucket/tabla automáticamente.
- **Evidencia:** E(T-INFRA-002), revisión de baseline y procedimiento de respaldo/migración/recuperación.
- **Resultado actual:** baseline Terraform 1.17.1/provider 6.73.0 conservada; §14 confirma backend local ya inicializado y primer apply acotado. State local creado/actualizado, serial 15, 14 instancias gestionadas, ignorado; backup .backup ausente. T-INFRA-001 continúa sin aceptación; no evidencia integral de custodia/backup/recuperación del state ni aceptación DP-I-12/13. No se marca COMPLETADA.
- **Riesgo:** pérdida o filtración state, lock incompatible y confusión con la authority DynamoDB.
- **Gate asociado:** GATE-I-A, GATE-I-H.

### T-INFRA-003 — VPC, subnets y rutas

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-2 Networking.
- **Objetivo:** expresar topología simple en dos AZ con exposición deliberada.
- **Dependencias:** T-INFRA-001, T-INFRA-002, T-INFRA-025; DP-I-03.
- **Trazabilidad:** INFRA-007, 033; PLD-INFRA-004/015; AC-I-02.
- **Archivos esperados:** futuros `infra/networking.tf`, `variables.tf`, `outputs.tf`.
- **Acciones futuras:** definir CIDR, subnets públicas/privadas, route tables e Internet Gateway; distinguir ALB, hosts y Redis. No aplicar todavía.
- **Aceptación:** topología revisada en dos AZ, sin solapamientos y con rutas/propósitos verificables; funcionamiento real queda para 027–028.
- **Evidencia:** E(T-INFRA-003), diagrama y revisión IaC/red.
- **Resultado actual:** §14 acredita autorización posterior y creación/verificación AWS de VPC 10.20.0.0/16, subnets .0/.1 /24 en use1-az1/use1-az2, IGW, tabla, default route y dos asociaciones; post-plan sin cambios. Avance real cloud, no solo definido/planificado. Falta aceptación de dependencias 001/002/025 y revisión de solapamientos externos; DP-I-03 no se cierra integralmente por este apply acotado. Estado PARCIAL conservado.
- **Riesgo:** subnets privadas sin salida o exposición accidental por rutas.
- **Gate asociado:** GATE-I-B.

### T-INFRA-004 — Salida estable y acceso Atlas/AWS

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-2 Networking.
- **Objetivo:** habilitar egreso seguro y reproducible sin Atlas abierto globalmente.
- **Dependencias:** T-INFRA-003; DP-I-03/06.
- **Trazabilidad:** INFRA-008, 009, 014; PLD-INFRA-004/008; AC-I-02, 06.
- **Archivos esperados:** futuros `infra/networking.tf`, `security.tf`, `variables.tf`, `outputs.tf`, solo referencias no secretas.
- **Acciones futuras:** comparar EC2 con salida fija frente a privada/NAT/endpoints y sus costos; definir DNS y salidas autorizadas para Atlas, ECR, SSM, logs, secretos y authority.
- **Aceptación:** perfil aprobado, rutas/salidas y allowlist especificadas sin `0.0.0.0/0` en Atlas; prueba de conectividad real reservada a 028.
- **Evidencia:** E(T-INFRA-004), comparación de costo/red y reglas propuestas saneadas.
- **Resultado actual:** §14 materializa IGW/ruta y egreso SG provisional sin EIP/NAT/compute. Dos EIP futuras conservan solo aprobación de diseño/plan; EIP y allowlist Atlas /32 no implementadas ni autorizadas para apply. Faltan costo/egreso integral, decisiones restantes DP-I-03/06 y prueba de conectividad externa estable; red creada no acredita Atlas accesible. Estado PARCIAL conservado.
- **Riesgo:** IP cambiante, NAT/IPv4 costosos o dependencia sin conectividad.
- **Gate asociado:** GATE-I-B, soporte GATE-I-F.

### T-INFRA-005 — Roles y permisos mínimos

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-3 Security/IAM.
- **Objetivo:** separar provisión, deployment, runtime, administración y bootstrap.
- **Dependencias:** T-INFRA-003, T-INFRA-004; DP-I-08/09 y contrato preliminar 004.
- **Trazabilidad:** INFRA-008, 021, 022, 024; PLD-INFRA-005/011/012; AC-I-02, 08, 09.
- **Archivos esperados:** futuros `infra/security.tf`, `authority.tf`, `secrets.tf`, referencias de registry/compute.
- **Acciones futuras:** inventariar llamadas reales; diseñar IAM por recurso/acción, roles temporales/SSM y límites OIDC para futuro deployment. No crear trust/workflows ahora.
- **Aceptación:** revisión de mínimo privilegio y separación de roles; runtime sin permisos de creación/borrado de tabla ni administración global.
- **Evidencia:** E(T-INFRA-005), matriz rol→acción→recurso y revisión.
- **Resultado actual:** §14 acredita apply de red y post-plan sin AccessDenied con IAM Read/Apply configuradas previamente por el responsable; permisos efectivos suficientes para las operaciones ejercitadas, no auditoría global ni prueba de destroy/rollback. Policies locales/remotas no modificadas por el agente. IAM runtime/instance profile/SSM y separación deployment/authority/secretos/004 pendientes. Estado PARCIAL conservado.
- **Riesgo:** privilegio excesivo, trust abierto o secretos de cuenta persistentes.
- **Gate asociado:** GATE-I-B, GATE-I-F.

### T-INFRA-006 — SG, administración y transporte interno

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-3 Security/IAM.
- **Objetivo:** evitar bypass de ALB y administración pública.
- **Dependencias:** T-INFRA-004, T-INFRA-005; DP-I-03/05/09.
- **Trazabilidad:** INFRA-008, 009, 013, 024; PLD-INFRA-004/005/007; AC-I-02, 05, 09.
- **Archivos esperados:** futuros `infra/security.tf`, `networking.tf`, `load-balancer.tf`.
- **Acciones futuras:** definir ingreso backend solo SG ALB, Redis privado/solo clientes aprobados, egreso y acceso SSM; resolver TLS interno y puertos con criterio de riesgo.
- **Aceptación:** reglas revisadas sin SSH público ni administración Docker; matriz de pruebas de acceso directo/denegación prevista para 028.
- **Evidencia:** E(T-INFRA-006), revisión SG/egreso y decisión TLS interno.
- **Resultado actual:** §14 acredita dos SG y cuatro reglas reales/verificados con AWS CLI: ALB solo ingress TCP443 IPv4 público y egress TCP8080 a SG backend; backend solo ingress TCP8080 desde SG ALB, sin SSH/ingress público de aplicación. Egreso backend IPv4 amplio provisional. Sin hosts/ALB o pruebas activas de bypass, SSM runtime, TLS interno o egreso fino aceptado. DP-I-05/09 y aceptación de dependencias pendientes; estado PARCIAL conservado.
- **Riesgo:** API directa, Redis público, egress indiscriminado o TLS asumido por EXPOSE.
- **Gate asociado:** GATE-I-B, GATE-I-E.

### T-INFRA-007 — Registry ECR

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-4 Registry.
- **Objetivo:** definir repositorio de artefactos inmutables sin introducir pipeline.
- **Dependencias:** T-INFRA-002, T-INFRA-005, T-INFRA-025; DP-I-14.
- **Trazabilidad:** INFRA-005, 024, 031; PLD-INFRA-003/015; AC-I-03, 12, 13.
- **Archivos esperados:** futuros `infra/registry.tf`, `variables.tf`, `outputs.tf`, políticas en `security.tf`.
- **Acciones futuras:** definir ECR, acceso push/pull, retención compatible con rollback y referencias por digest; distinguir rol build/deploy/runtime.
- **Aceptación:** configuración revisada y output no secreto; no imágenes publicadas ni registry creado por la preparación.
- **Evidencia:** E(T-INFRA-007), revisión IaC y contrato de permisos/retención.
- **Resultado actual:** §§15–17 definen ECR privado enver-dev-api, IMMUTABLE/AES256/scan_on_push=true, lifecycle solo untagged >14 días y tres outputs. §17: policy separada adjunta manualmente según responsable, scanning global BASIC/rules=[] leído exit 0; listado general denegado, describe por nombre RepositoryNotFound. Fmt-check/validate exit 0, replan exacto 2/0/0 y red14 no-op, sin apply. Sin permisos de escritura/destroy ejercitados, repository o scanning de imagen reales. Dependencias 002/005/025 y aceptación de retención/contrato/DP-I-14 pendientes; PARCIAL conservado.
- **Riesgo:** borrado del digest de rollback, tags mutables o permisos amplios.
- **Gate asociado:** GATE-I-C, GATE-I-H.

### T-INFRA-008 — Contrato de imagen y ownership del digest

- **Estado:** PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-4 Registry.
- **Objetivo:** garantizar mismo artefacto reproducible y promoverlo manualmente desde 004.
- **Dependencias:** T-INFRA-007; DP-I-02/14, contrato aprobado con futura 004.
- **Trazabilidad:** INFRA-005, 006, 034, 035; PLD-INFRA-003/017/018; AC-I-03, 13.
- **Archivos esperados:** contrato futuro en `specs/003-infrastructure/plan.md` y referencias `infra/compute.tf`/`variables.tf`. Dockerfile y workflows fuera de esta tarea.
- **Acciones futuras:** revisar Dockerfile existente, .NET 10/non-root/configuración; definir commit→digest→CI verde, writer único del digest y retención previa. Solicitar aparte cualquier cambio Docker necesario.
- **Aceptación:** contrato aprobado de artefacto inmutable idéntico en A/B y deploy workflow_dispatch tras CI verde; sin build, push o YAML implícitos.
- **Evidencia:** E(T-INFRA-008), contrato y revisión documental de imagen.
- **Resultado actual:** §15 registra Dockerfile intacto, build local autorizado exit 0, metadatos linux/amd64, .NET/ASP.NET 10.0.12, USER 1654 non-root, HTTP_PORTS=8080 y ENTRYPOINT dotnet Enver.API.dll. Sin arranque, push/digest ECR ni CI acreditados. Contrato URI@digest y propuesta de escritor único 004 documentados aquí sin modificar plan; aprobación DP-I-02/14/004, bases fijadas por digest, evidencia de seguridad y aceptación pendientes. No COMPLETADA.
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

- **Estado:** PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-13 Terraform, intervención temprana transversal.
- **Objetivo:** acreditar baseline antes de extender IaC.
- **Dependencias:** T-INFRA-001, T-INFRA-002; autorización explícita de comandos/entorno, DP-I-12.
- **Trazabilidad:** INFRA-029, 032, 038; PLD-INFRA-015/016/019; AC-I-01.
- **Archivos esperados:** futura baseline `infra/*.tf` de 002 y lockfile de providers si se autoriza; state/planes nunca Git.
- **Acciones futuras:** revisar formato, inicialización/validación y versiones/locking mediante comandos autorizados; registrar salida nativa saneada. No apply ni provisión; cualquier descarga/init requerirá permiso futuro.
- **Aceptación:** baseline validada realmente, state/backup protegidos y versiones acreditadas; validación local no prueba cloud ni toda la IaC futura.
- **Evidencia:** E(T-INFRA-025), comandos/entorno/versiones/resultados y revisión.
- **Resultado actual:** §14 acredita fmt-check/validate exit 0, plan guardado 14/0/0 exit 2, apply de ese archivo exit 0 (14 añadidos), post-plan 0/0/0 exit 0 y verificación AWS; hashes infra/HCL/lock/policies intactos. State local ignorado serial 15, sin .backup observado; custodia/backup/recuperación, dependencias 001/002 y aceptación E025 integral pendientes. No COMPLETADA ni gate cerrado.
- **Extensión actual:** §15 acredita fmt/check/validate exit 0 y plan ECR 2/0/0 exit 2, con 14 recursos de red no-op. Conserva intento CLI previo fallido por argumentos PowerShell; sin init/actualización/apply ni modificación de state persistido. Custodia/backup/recovery y aceptación integral continúan pendientes.
- **Revalidación posterior:** §17 registra fmt-check/validate exit 0 y un nuevo plan guardado 2/0/0 exit 2, sin AccessDenied de Terraform; networking14 no-op, HCL/policy/lock/state persistido intactos. La lectura CLI de listado general denegada se conserva aparte; no cierre de dependencias/custodia/backup/recovery/E025 integral.
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

Cobertura documental: 40 requisitos INFRA, 20 decisiones propuestas PLD-INFRA, 15 criterios AC-I, 15 fases, 30 tareas y 9 gates. **Cobertura no equivale a implementación, prueba o satisfacción.** Tras §9 hay siete tareas parciales (001–006/025), otras 23 PENDIENTE / NO EJECUTADA; todos los gates PENDIENTE / NO SATISFECHO. Identidad STS y presupuesto de baseline acreditados, sin infraestructura cloud desplegada.

Se revisa coherencia con 001 sin completarlo, con 002 sin cambiar Authentication/semántica Redis/JWT ni su autoridad DynamoDB, y con el límite entre infraestructura y 004. Readiness/forwarding siguen pendientes de incremento separado; dominio/precios/versiones/retención/umbrales y demás DP permanecen abiertos. No se declara cloud existente ni se incorporan secretos. Futuro deploy manual workflow_dispatch tras CI verde, nunca automático por push/merge.

El cierre original de materialización fue exclusivamente documental, sin Terraform/infra. La autorización posterior y sus límites se registran a continuación; no amplía permisos por sí sola a fases posteriores.

## 6. BUILD controlado T-INFRA-001 + T-INFRA-002 — 2026-11-12

### 6.1. Autorización, baseline y alcance real

Responsable solicita **ENVER — 003 INFRASTRUCTURE / BUILD T-INFRA-001 + T-INFRA-002 / BASELINE TERRAFORM**. Precheck: rama `feature/authentication`, HEAD `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`, árbol limpio. Se crea `feature/infrastructure` desde ese mismo HEAD, sin cambiar commit. Checkpoint anterior incluye únicamente los seis Markdown 003/004; Authentication sigue TERMINADA exclusivamente LOCAL/TEST REPRESENTATIVO.

Se crean `infra/main.tf`, `versions.tf`, `providers.tf`, `variables.tf`, `outputs.tf`, `locals.tf`, `terraform.tfvars.example`, `README.md` y el lock generado `infra/.terraform.lock.hcl`. Solo se actualizan este task y `memoria.md`. `.gitignore` ya contiene exclusiones equivalentes para descargas, state/backups, tfvars, planes, crashes y overrides; no se modifica ni duplica. Lock y ejemplo siguen versionables, pendientes de checkpoint posterior: **sin stage/commit/push**.

### 6.2. E(T-INFRA-001) — preparación parcial, no aprobación de destino/costo

- **Trazabilidad:** INFRA-001–003/033/038; PLD-INFRA-001/020; AC-I-01/15; GATE-I-A, soporte I-H/I-I.
- **Acreditado:** límites sin recursos/cargos, proyecto técnico `enver`, inputs de destino sin defaults y [README infra](../../infra/README.md) con lista de potenciales cargos, decisiones abiertas y procedimiento de operación local.
- **Pendiente:** cuenta/identidad, región/AZ, ambiente final, responsables, duración, presupuesto máximo, elegibilidad/créditos y precios regionales. Dominio, tamaños EC2, Redis final y tier Atlas no se seleccionan.
- **Detección segura:** AWS CLI ausente; sin variables habituales de credenciales/perfil/roles/web identity/contenedor, sin rutas alternativas de configuración indicadas ni archivos estándar `.aws/config`/`.aws/credentials`. Solo se comprobó presencia, no contenido. No se ejecutó STS. Esto **no demuestra ausencia universal de una identidad AWS**: identidad no acreditada en esta sesión; acceso pendiente. No se crean usuarios IAM/access keys.
- **Resultado:** PARCIAL / BLOQUEADA POR DECISIONES. No existe estimación regional ni aprobación escrita de presupuesto/responsables; E(001) no satisface su criterio de aceptación. DP-I-01/13 permanecen pendientes.

### 6.3. E(T-INFRA-002) — baseline técnica local validada

- **Trazabilidad:** INFRA-029–033/038; PLD-INFRA-015/016; AC-I-01/12; GATE-I-A/I-H.
- **Entorno:** Windows, Terraform **1.17.1**, plataforma **windows_amd64**. Primera llamada `terraform -version`: error de resolución (exit 1) por PATH heredado. Búsqueda acotada en PATH proceso/usuario/máquina y ubicaciones comunes encontró instalación WinGet existente del usuario; ejecución por su ruta: exit 0. Se antepone esa ruta solo al PATH del proceso de los comandos posteriores. Sin instalar/actualizar Terraform ni alterar PATH persistente; aviso de versión más nueva no seguido.
- **Versiones:** Terraform `>= 1.17.1, < 1.18.0`, compatible con la instalación observada y limitada a parches de esa minor. Provider único `hashicorp/aws ~> 6.0`, serie mayor acreditada por API oficial Registry (6.73.0, publicación 2026-11-11). `init` resolvió **6.73.0**, instalado **signed by HashiCorp**, y generó lock con versión/restricción/checksums; no se editó manualmente. Selección técnica justificada para revisión, no aprobación tácita de DP-I-12 completa.
- **Configuración revisada:** provider usa `var.aws_region`, guard `allowed_account_ids` con `var.aws_account_id` y cadena estándar externa de credenciales; sin claves ni flags que omitan verificación. Inputs tipados/validados; cuenta/región/ambiente sin defaults; ejemplo con placeholders no operativos. Naming y tags comunes; outputs documentado sin inventar recursos. Sin resource/data/module/provisioner ni acceso AWS.
- **State:** backend local explícito, ruta `terraform.tfstate`, nunca Git. `init -backend=false` no inicializa backend ni genera state de recursos. No backend S3 ni tabla de locking; DynamoDB Authentication Authority conserva su finalidad separada. README define un operador, respaldo cifrado, recuperación y migración remota antes de colaboración/GitHub. **Operador/destino, ACL/cifrado/retención y recuperación real no acreditados**: no se simulan ni se aprueban por documentación. No hay state real que respaldar en este bloque.

| Comando ejecutado | Resultado nativo |
| --- | --- |
| `terraform fmt -recursive infra` | Exit 0, sin errores |
| `terraform fmt -check -recursive infra` | Exit 0, sin archivos fuera de formato |
| `terraform -chdir=infra init -backend=false` | Exit 0; successfully initialized; AWS 6.73.0 firmado por HashiCorp; lock generado |
| `terraform -chdir=infra validate` | Exit 0; `Success! The configuration is valid.` |
| `git check-ignore -v -- …` (rutas baseline) | Exclusiones existentes confirmadas; excepción de lock y ejemplo versionable comprobados |

Las salidas nativas constan en la sesión de ejecución; este registro saneado documenta resultados, no inventa un paquete externo archivado ni retención duradera. `validate` no configura la identidad/cuenta AWS ni aprueba valores de inputs; no acredita plan, costos, infraestructura o recuperación. No se ejecutó `plan`, `apply`, refresh, import, destroy, init remoto o `init -upgrade`.

**Resultado contractual:** baseline técnica implementada/validada localmente; **T-INFRA-002 PARCIAL / ACEPTACIÓN PENDIENTE**, no COMPLETADA. T-INFRA-001 sigue sin aceptación; quedan revisión/aprobación de versiones/manejo de state y decisiones aplicables DP-I-12/13. DP-I-02 de compute no se resuelve ni se adelanta. No se rebaja el criterio original de T-INFRA-002 por obtener validate verde.

### 6.4. Gates, límites y cierre del bloque

**GATE-I-A PENDIENTE / NO SATISFECHO**: exige presupuesto, versiones, ambiente y state aprobados, E(001), E(002) y E(025), además de validación real. Solo se acredita la validación local descrita; no hay aprobación de presupuesto/destino/custodia integral ni E(T-INFRA-025). No se ejecuta ni marca T-INFRA-025 por coincidencia de comandos. AC-I-01 y los otros ocho gates permanecen pendientes; ningún AC cloud satisfecho.

Validación final repetida: `terraform -version` confirma 1.17.1/windows_amd64; `terraform fmt -check -recursive infra` y `terraform -chdir=infra validate` exit 0. `git diff --check` exit 0, sin errores de whitespace (solo aviso de normalización LF/CRLF por configuración existente). `git diff --stat` muestra únicamente task/memoria como cambios rastreados; `git status --untracked-files=all` añade los nueve archivos infra esperados. Sin state/plan real, sin secretos detectados en la baseline/lock y sin stage. Las descargas locales `.terraform/` permanecen ignoradas; no se versionan binarios.

No se crean recursos AWS, workflows, networking/compute/registry/authority/secrets/observability ni otras fases; no se modifica código productivo, Authentication, tests, Dockerfile, SDK, 003 spec/plan o 004. Primera release Auth-only y deploy exclusivamente manual workflow_dispatch tras CI verde exacto se conservan. Orders fuera de alcance; AUTH-027 integral y gates de 002 no cambian. Se detiene tras este bloque, sin stage/commit/push.

## 7. Validación AWS + Terraform y revisión de cierre — 2026-11-12

### 7.1. Autorización y evidencia saneada de identidad/perfil

Solicitud **ENVER — 003 INFRASTRUCTURE / CIERRE BASELINE AWS + TERRAFORM / BUILD-VALIDACIÓN CONTROLADA**. Rama `feature/infrastructure`, HEAD `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`; preexistían task/memoria modificados y nueve archivos infra nuevos, 0 staged. Se preservan esos cambios. Solo se actualizan este archivo y memoria.md; no se modifica configuración Terraform, lock, ejemplo, README infra o configuración local AWS.

El responsable informa instalación AWS CLI, usuario IAM sin acceso de consola y credenciales configuradas localmente. No se crean usuarios, claves, políticas ni recursos en este turno. El sondeo inicial no encuentra CLI en PATH heredado/ubicación global; la búsqueda acotada localiza instalación existente del usuario en `AppData/Local/Programs/Amazon/AWSCLIV2`. `aws --version`: **aws-cli/2.40.1**, Python 3.14.8, Windows/10, AMD64, exit 0; sin instalación/actualización ni cambios persistentes de PATH.

Las salidas completas de STS/configuración se capturan en memoria del proceso y se reducen antes de mostrarse. No se imprimen ni archivan Account ID/ARN completos, access keys (ni sufijos), secret keys o tokens. No se leen directamente archivos de credenciales ni se escriben salidas crudas en Git/temporales.

| Consulta real | Resultado saneado |
| --- | --- |
| `aws sts get-caller-identity --profile enver-infra` (JSON, sin pager) | Exit 0; autenticación exitosa; Account presente; ARN coincide con usuario IAM `user/enver-terraform-local` |
| `aws configure get region --profile enver-infra` | Exit 0; `us-east-1` |
| `aws configure list --profile enver-infra` | Exit 0; perfil esperado, access_key y secret_key presentes, origen `shared-credentials-file`; región `us-east-1`, origen `config-file` |

**Región: us-east-1 — APROBADA PARA BASELINE**, por selección explícita del responsable y verificación real. No es irreversible ni aprueba región/AZ final para todos los servicios. La cuenta autenticada está presente, sin publicar ID; eso no aprueba costos, permisos de provisión o recursos.

### 7.2. IAM: verificación limitada, sin ampliación de acceso

El responsable declara que `enver-terraform-local` no tiene permisos administrativos ni amplios de infraestructura; es intencional. Para comprobarlo se intentan solo lecturas IAM con ese perfil: `list-attached-user-policies`, `list-user-policies` y `list-groups-for-user`, limitadas al mismo usuario. Las tres devuelven **AccessDenied, exit 254**. No se reintenta con otro principal ni se agregan permisos para eludir la denegación.

**IAM permissions: PENDIENTES PARA INFRA-3.** Se acredita denegación de esas consultas, **no ausencia integral de privilegios**: STS valida identidad, no enumera permisos efectivos; AccessDenied en lecturas tampoco demuestra que no existan otros permisos amplios. La declaración del responsable queda separada de lo observado. Auditoría de políticas/grupos/roles/condiciones requiere evidencia de un operador autorizado; no se marca T-INFRA-005 ejecutada ni se inspeccionan otras identidades.

### 7.3. E(T-INFRA-002) complementaria: versiones, configuración y state

| Comprobación | Resultado |
| --- | --- |
| `terraform -version` | Exit 0; **1.17.1 / windows_amd64**; sin actualizar |
| `terraform fmt -check -recursive infra` | Exit 0; formato correcto, sin editar archivos |
| `terraform -chdir=infra validate` | Exit 0; **Success! The configuration is valid.** |
| Lock AWS | **hashicorp/aws 6.73.0**, restricción `~> 6.0`, checksums presentes; sin actualizar/init nuevo |
| Provider | `region = var.aws_region`, cuenta permitida desde variable; sin access_key/secret_key/token en HCL |
| State | Backend **local** declarado, no remoto; state/backups/planes ignorados y ninguno rastreado ni generado |

Se conserva la estrategia inicial local, no Git, sin S3 o DynamoDB locking. No se inicializa backend ni se crea state artificial; no hay evidencia nueva de ACL/cifrado, respaldo/recuperación, operador/custodia aceptados o migración remota. Autenticación exitosa del CLI **no prueba** autenticación Terraform ni autorización de provisión: no se ejecuta plan ni se configura un destino real mediante tfvars. Los nueve archivos infra se conservan byte por byte durante este turno.

### 7.4. Dictamen por criterios originales y detención

- **T-INFRA-001: PARCIAL / BLOQUEADA POR DECISIONES.** Identidad/perfil y región de baseline acreditados; **presupuesto: DECISIÓN PENDIENTE**, sin monto inventado. El contrato también exige presupuesto/responsables aprobados por escrito y estimación fechada/regional: no existen esas nuevas aprobaciones. Duración, ambiente/AZ y ownership formal tampoco se cierran por la identidad IAM. No corresponde “bloqueada solo por presupuesto”. E(001) se amplía, no se acepta integralmente.
- **T-INFRA-002: PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE.** Versiones observadas, lock, formato/configuración y estrategia local comprobados en su alcance; dependencia T-INFRA-001 sigue sin aceptación. No se sustituyen aprobación/custodia de state y decisiones aplicables por validate verde. No COMPLETADA.
- **GATE-I-A: PENDIENTE / NO SATISFECHO.** Task §3/spec AC-I-01 exigen presupuesto/versiones/ambiente/state aprobados y E(001), E(002), **E(025)**. Identidad/región no eliminan esos requisitos; presupuesto y aprobaciones/evidencias faltantes bloquean cierre. T-INFRA-025 no ejecutada; otros ocho gates, DP y AC no cerrados.

Trazabilidad de este complemento: E(001) → INFRA-001–003/033/038, PLD-INFRA-001/020, AC-I-01/15; E(002) → INFRA-029–033/038, PLD-INFRA-015/016, AC-I-01/12; GATE-I-A y soporte I-H/I-I. Resultados saneados en esta sesión/documento; sin afirmar archivo externo o retención duradera de salidas nativas.

No recursos creados por el bloque, no políticas IAM agregadas, sin plan/apply, sin cambios de Authentication/Program.cs/tests/Dockerfile/004/spec/plan ni tareas posteriores. Acceso de consola deshabilitado es declaración del responsable, no verificación IAM acreditada. Estado Git esperado conserva dos documentos modificados y nueve archivos infra nuevos, 0 staged; HEAD sin cambios. Sin stage/commit/push. Se detiene tras validación y registro.

## 8. Presupuesto aprobado y preparación documental INFRA-2/3 — 2026-11-12

### 8.1. Decisión del responsable y alcance de la autorización

Solicitud **CIERRE DE PRESUPUESTO + PREPARACIÓN INFRA-2/3 / BUILD DOCUMENTAL-VALIDACIÓN CONTROLADA**, rama `feature/infrastructure`, HEAD `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`. Se conservan los dos documentos modificados y nueve archivos infra nuevos preexistentes, sin stage. Solo se editan task/memoria; no se reescriben spec/plan ni se implementa networking/IAM.

| Concepto | Decisión registrada |
| --- | --- |
| Crédito total disponible informado por el responsable | **USD 100**; no consulta de Billing ni verificación independiente del saldo/condiciones en este bloque |
| Presupuesto operativo inicial ENVER | **USD 50 — APROBADO PARA BASELINE** |
| Reserva mínima objetivo | **USD 50**; preservar al menos ese importe del crédito informado |
| Costo estimado real | **PENDIENTE**; el crédito y el presupuesto no son una cotización ni garantía de cobertura |

Límite inicial para el conjunto de desarrollo, validación, despliegue y entrega académica; **no se interpreta como USD 50 mensuales ni por recurso**. La duración/ventanas de uso siguen por aprobar. La reserva es objetivo operativo, no control técnico de AWS: no se crean AWS Budgets/alarma ni tope automático. Si saldo/consumo/condiciones o estimación total pueden comprometerla, detener y someter revisión previa; esta aprobación no autoriza recursos ni cargos en este turno.

### 8.2. Control de costo y estrategia económica inicial

Antes de cualquier bloque que pueda generar cargos, exigir estimación fechada para `us-east-1`, horas/duración, cantidades/capacidad, retención/transferencia y margen, indicando partidas cubiertas o no por crédito. Revisar acumulado y saldo/reserva antes de mutar; no asumir Free Tier ni gratuidad de un servicio o de una cuenta con crédito.

Partidas obligatorias: **EC2, EBS/snapshots, IPv4/EIP, ALB/LCU, Redis, CloudWatch, ECR, Secrets/SSM/KMS, transferencia y Atlas si aplica**; además DynamoDB, endpoints, DNS y cualquier dependencia necesaria. Costos Atlas externos no se descuentan automáticamente del crédito AWS: registrar su cobertura/fuente por separado y revisar cualquier gasto adicional. La lista de servicios potenciales no es aprobación de su creación.

Estrategia inicial confirmada, sin implementación ni tipos/precios elegidos: **dos EC2 pequeñas en dos AZ**, ALB requerido por rúbrica, **evitar NAT Gateway inicialmente**, MongoDB Atlas externo, evaluar costo/compatibilidad de Redis antes de elegir ElastiCache, logs/retención mínimos necesarios para evidencia y seguridad, y apagado/eliminación controlada cuando no se use. Se conserva la comparación Redis de PLD-INFRA-009: no se selecciona una alternativa final ni se rebajan continuidad/revocación/fail-closed para ahorrar.

No NAT no equivale a salida gratis: resolver egreso para Atlas/AWS/SSM, costo de IPv4/EIP y eventual uso de endpoints antes de aprobar DP-I-03. EC2 detenida no elimina cargos de EBS, IPv4/EIP, ALB, Redis, logs u otros servicios persistentes. Procedimiento futuro: inventario/costos → autorización de parada/limpieza → protección de datos/continuidad de seguridad → verificación posterior de recursos/cargos residuales. **No destroy, borrado de datos, snapshots o reset de revocación/authority implícitos.**

### 8.3. Reevaluación contractual E(001)/E(002) y GATE-I-A

- **T-INFRA-001 — PARCIAL / BLOQUEADA POR DECISIONES.** Presupuesto USD 50 y reserva objetivo aprobados; identidad, perfil y región de baseline ya acreditados. Falta la **estimación regional fechada y lista cuantificada de cargos/recursos fuera del límite**, duración/horas, elegibilidad/vigencia/cobertura del crédito, responsables y ownership aprobados por escrito, ambiente/AZ y naming/tags aplicables. El criterio original de aceptación no se modifica: monto aprobado no equivale a estimación ni aceptación de DP-I-01/13 completas.
- **T-INFRA-002 — PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE.** Se reconocen Terraform 1.17.1, AWS lock 6.73.0 y estrategia inicial local como baseline técnico. **T-INFRA-001 es dependencia formal no aceptada.** El responsable incluye “state local protegido” en su baseline; lectura y evidencia previa confirman backend local/no Git, procedimiento documentado y ausencia de secretos/state real, pero no verifican ACL/cifrado, destino/operador, backup cifrado/recuperación y aprobación integral del manejo del state. No se inventa prueba de protección ni se genera state para completarla.
- **GATE-I-A — PENDIENTE / NO SATISFECHO.** El presupuesto ya no bloquea por falta de monto. Faltan aceptación integral de E(001)/E(002), ambiente/custodia aprobados y **E(T-INFRA-025)**. T-INFRA-025 está en INFRA-13 como **intervención temprana transversal**, no como validación posterior a construir toda la infraestructura: plan §§4–5 y task 025 exigen `001 → 002 → 025 → 003`. No hay circularidad ni autorización para omitirla; la validación final del conjunto es T-INFRA-026. Los comandos repetidos aquí no ejecutan ni aceptan automáticamente T-INFRA-025. Requiere autorización y evidencia propias antes de extender IaC.

Trazabilidad: E(001) → INFRA-001–003/033/038, PLD-INFRA-001/020, AC-I-01/15; E(002) → INFRA-029–033/038, PLD-INFRA-015/016, AC-I-01/12; GATE-I-A y soporte I-H/I-I. DP-I-01 queda **parcialmente resuelta** respecto al monto/región/crédito informado; no se cierra su resto ni otras DP por inferencia.

### 8.4. Recomendación del siguiente bloque — NO EJECUTADO / NO AUTORIZADO AHORA

Primero resolver los pendientes concretos de 001/002 y solicitar **validación inicial T-INFRA-025** con alcance/comandos/entorno/custodia/evidencia delimitados. Después solicitar un BUILD de **preparación de configuración**, sin plan/apply/provisión ni asignación de políticas al usuario, para 003–006:

| Tarea futura | Preparación recomendada / decisiones previas |
| --- | --- |
| T-INFRA-003 — Networking | VPC, dos subnets en AZ distintas, route tables e Internet Gateway; aprobar CIDR sin solapamientos, AZ y perfil red/egreso. Dos subnets públicas son opción inicial a evaluar para futuro ALB/hosts sin NAT, no topología definitiva ni alta disponibilidad integral |
| T-INFRA-004 — Salida Atlas/AWS | Aprobar egreso estable/allowlist Atlas, DNS y conectividad hacia AWS/SSM; contabilizar IPv4/EIP/endpoints y comparar con alternativas, sin Atlas `0.0.0.0/0`. Resolver DP-I-03/06 aplicables antes de aceptar |
| T-INFRA-005 — Security/IAM | Matriz llamada→acción→recurso/condición, roles separados runtime/Terraform/deploy/bootstrap. Runtime mínimo según integraciones reales, Terraform delimitado; no AdministratorAccess/PowerUserAccess, privilegios globales de infraestructura o PassRole irrestricto. Acciones que técnicamente requieran `Resource=*` deben justificarse/restringirse, no generalizarse |
| T-INFRA-006 — SG/administración | SG de ALB y backend, ingreso backend solo desde SG ALB; administración mediante SSM, sin SSH público ni Docker remoto. Resolver puerto/TLS interno/egreso de DP-I-05/09; SG ALB puede prepararse sin crear el ALB |

Archivos candidatos a autorizar después: `infra/networking.tf`, `infra/security.tf` y ajustes mínimos de variables/locals/outputs, más evidencia task/memoria. El IAM Terraform del operador se **propone/revisa**, no se autoasigna ni se incorpora al runtime; cualquier otorgamiento real requiere autorización específica y principal autorizado. **IAM permissions PENDIENTES PARA INFRA-3**; `enver-terraform-local` está autenticado según §7, pero STS y AccessDenied de lecturas no prueban privilegios efectivos ni autorizan provisión.

No adelantar `compute.tf`, ALB/listeners/targets, Redis, ECR, ACM, DynamoDB, Secrets ni recursos de esos servicios. Solo contratos/referencias no secretas para IAM futuro cuando sean necesarios, sin crearlos ni inventar ARNs/cuenta. No definir credenciales, puertos/CIDR finales, tamaños, Atlas tier o servicio Redis por iniciativa propia. La recomendación no ejecuta T-INFRA-003–006 ni modifica sus dependencias/criterios; siguen PENDIENTE / NO EJECUTADA.

### 8.5. Validación local y detención

En este turno: `terraform -version` confirma **1.17.1/windows_amd64**, exit 0, sin actualización; `terraform fmt -check -recursive infra` **exit 0**; `terraform -chdir=infra validate` **exit 0 / Success! The configuration is valid.** Sin init nuevo, plan/apply o llamadas AWS; identidad/región y CLI 2.40.1 se conservan como evidencia previa de §7, no se afirma una nueva consulta STS/Billing.

No recursos/políticas creados ni modificaciones en infra, Authentication/Program.cs/tests/Dockerfile/004/spec/plan. Los ocho gates restantes y T-INFRA-025 permanecen pendientes; presupuesto y esta recomendación no abren ejecución automática. Sin stage/commit/push; HEAD conservado. Detener al registrar decisión, pendientes y propuesta.

## 9. INFRA-2/3 — Definiciones candidatas, validación parcial y bloqueo de plan

### 9.1. Autorización, baseline y decisiones — 2026-11-12

El responsable solicita BUILD CONTROLADO NO APPLY de T-INFRA-003–006 y parte posible de T-INFRA-025. Lectura completa previa de spec/plan/task; revisión de Dockerfile, launchSettings, Program y appsettings sin editarlos. Baseline verificada: **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**, árbol limpio. Checkpoint Git previo autorizado preserva once archivos del baseline; este turno no hace stage/commit/push.

Confirmados para este bloque: **project enver, environment dev, us-east-1**, primera release Register/Login/Logout; Orders excluido. Crédito informado USD 100, presupuesto total inicial USD 50/reserva objetivo USD 50 conservados, no gratuidad ni aprobación de gasto. Autorización actual permite preparar candidatos pese a dependencias pendientes, no aceptarlas ni reformar spec/plan.

Archivos nuevos: `infra/networking.tf`, `infra/security.tf`, `infra/bootstrap/terraform-local-network-policy.json`. Modificados: variables/locals/outputs/terraform.tfvars.example/README de infra, este task y memoria. Main/backend, provider, versiones y lock permanecen intactos; no dependencias/providers nuevos.

### 9.2. E(T-INFRA-003) / E(T-INFRA-004): red y Atlas

- **Trazabilidad:** 003 → INFRA-007/033, PLD-INFRA-004/015, AC-I-02; 004 → INFRA-008/009/014, PLD-INFRA-004/008, AC-I-02/06; GATE-I-B y soporte I-F.
- Spec §6/PLD-INFRA-004 permiten **proponer A: ALB/EC2 en públicas, ingreso backend solo SG ALB**; la opción privada B no se elimina. Se define candidato A con VPC propuesta `10.20.0.0/16`, DNS support/hostnames, dos subnets, IGW y route table/ruta/asociaciones. No defaults de red: CIDR y bits adicionales son inputs obligatorios; ejemplo `/16 + 8` produce `10.20.0.0/24` y `10.20.1.0/24`. Contención/no solapamiento interno por derivación; redes externas aún no verificadas.
- AZ por `aws_availability_zones`, filtro available/availability-zone/opt-in habilitado; ordenar **Zone IDs**, tomar dos con claves a/b y precondición al menos dos. Sin nombres de AZ inferidos; IDs reales/aptitud de cómputo no acreditados por denegación. Revisar reemplazos si cambia el conjunto elegible.
- Ruta IPv4 `0.0.0.0/0` vía IGW por tabla compartida. `map_public_ip_on_launch=false`; sin IP/EIP no habrá salida IPv4 de hosts futuros aunque la subnet sea pública. **Sin NAT Gateway/instance/endpoints/EIP**. Redis privado sigue fase propia.
- DP-I-03 sigue abierta: `network_design_approved` obligatorio sin default y precondición impiden planear sin aceptación expresa de candidato/CIDR/egreso/costo. Ejemplo deja false; guard no sustituye autorización o gates. No se declara A aceptada por escribirla.
- Recomendación concreta: **dos EIP futuras, una por EC2**, IPs /32 explícitas en Atlas y reasociación controlada por reemplazo. Comparación detallada en [README](../../infra/README.md#atlas-e-ips-recomendacióncosto-no-aprovisionamiento): IP automática cambia con stop/start, NAT agrega costo/GB y alternativas privadas requieren componentes/capacidad/tier/operación. Sin aprobar/asignar EIP o tocar allowlist. Prohibido /0 Atlas. Runbook previo a admisión exige autorización, ambas salidas y TLS; no reset de seguridad ni suponer cierre inmediato de conexiones al retirar allowlist.
- **Costo fechado 2026-11-12:** [VPC pricing](https://aws.amazon.com/vpc/pricing/) acredita **USD 0.005/IP-hora**, in-use/idle, aplicable us-east-1. Dos IPv4 o dos EIP: **USD 0.01/h, USD 0.24/24 h, USD 7.30/730 h**, solo IP. Proyección mínima futura 2 backend + 2 ALB: **USD 14.60/730 h solo IP**, ALB puede usar más. No EC2/EBS, ALB horas/LCU, Redis/Atlas/logs/transferencia incluidos. VPC/subnets/rutas/IGW/SG definidos sin workloads/IPs: **USD 0 de cargo recurrente directo estimado**, no factura medida ni toda plataforma gratis. NAT no elegido; tarifa exacta actual Virginia/GB pendiente, no extrapolada de ejemplo Ohio. Total/calendario/crédito/ownership siguen pendientes antes de apply. EIP retenida con instancia detenida sigue costando; no limpiarla/restaurar datos sin permiso.

### 9.3. E(T-INFRA-005) / E(T-INFRA-006): permisos y SG

- **Trazabilidad:** 005 → INFRA-008/021/022/024, PLD-INFRA-005/011/012, AC-I-02/08/09; 006 → INFRA-008/009/013/024, PLD-INFRA-004/005/007, AC-I-02/05/09; GATE-I-B y soporte I-E/I-F.
- SG ALB: ingreso TCP 443 IPv4 público; **sin 80** hasta listener redirect aprobado; egreso solo SG backend/8080. SG backend: ingreso únicamente SG ALB/8080, sin 22/SSH, Docker remoto o ingress general de aplicación. SG no crea ALB/certificado ni demuestra bloqueo real; evitar otros SG permisivos/default en hosts futuros.
- **Puerto declarado 8080**, no inferido solo de EXPOSE: Dockerfile líneas 7–8 + `Properties/launchSettings.json` Container HTTP_PORTS=8080; Program/appsettings sin otro listener. Referencia oficial .NET confirma HTTP_PORTS=8080 en imagen base de referencia, pero no se inspecciona/ejecuta imagen de producción. 8081/useSSL local no prueba HTTPS; **TLS interno DP-I-05 pendiente**, mapeo host→contenedor por confirmar en compute. SG TCP candidato no decide HTTP frente a HTTPS.
- Egreso backend **IPv4 amplio provisional** (`0.0.0.0/0`, todos los protocolos), no ingress ni /0 Atlas: evita imponer una lista incompleta que rompa AWS/SSM/ECR/CloudWatch, Atlas, OCSP/TLS, bootstrap y Redis. Superficie de salida/exfiltración mayor, sin filtro FQDN ni aislamiento fino; reducir solo con diseño completo de destinos/endpoints/red, puertos/SG Redis y requisitos AMI/certificados. Sin IPv6. Resolver VPC DNS y conectividad SSM; sin agente/role/profile/Session Manager funcionales todavía. Egreso no aceptado como diseño final.
- Bootstrap **solo lectura**, JSON revisado localmente: nueve Describe de red/AZ/atributos/SG/rutas/tags, región us-east-1, usuario enver-terraform-local. `Resource=*` de lecturas regionales, sin ID de cuenta; consulta a [referencia EC2](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ec2.html) confirma operaciones. Propuesta para revisión/asignación manual por administrador autorizado, **no aplicada ni simulada en IAM**. Sin AdministratorAccess/PowerUserAccess, CreateTags/escrituras de red, RunInstances, EIP, ALB/ECR/ACM/Secrets/Redis/DynamoDB/CloudWatch, IAM/PassRole. Tagging existe solo en HCL; permiso de escritura no necesario para definir/validar sin apply. La futura provisión exige otra política lifecycle con condiciones/recursos revisados, no ampliar esta a ciegas.
- T005 exige separación/diseño y dice no crear trust aún: **sin rol/profile runtime Terraform en este bloque**. Matriz documental: operador Terraform (lecturas de esta fase), runtime EC2 (rol/profile futuro, no claves, no creación/borrado de tabla), operador SSM (sesiones delimitadas), publisher/deploy OIDC 004 y bootstrap de secretos/authority separados. No permisos prematuros SSM/ECR/logs/Secrets/DynamoDB; contratos DP-I-08/09/004 siguen pendientes.

### 9.4. Preflight AWS y E(T-INFRA-025) parcial

Con AWS CLI existente/perfil externo enver-infra, sin pager: STS **exit 0**, ARN coincide con user/enver-terraform-local, Account presente; región configurada us-east-1. Se capturan salidas en memoria y solo se muestran presencia/coincidencia; sin Account ID/ARN completo, claves/tokens o errores crudos en documentos/archivos.

`aws ec2 describe-availability-zones --profile enver-infra --region us-east-1 …` **exit 254 / UnauthorizedOperation**. Acción faltante **ec2:DescribeAvailabilityZones**, recurso conjunto regional de AZ (**Resource=* para esa lectura**), fase INFRA-2/T003 preflight. No se amplían permisos, cambia principal o reintenta contra otras cuentas. Políticas efectivas no auditadas integralmente; identidad no demuestra permisos suficientes.

**Plan NO ejecutado:** falla prerrequisito de permiso AZ y faltan decisiones críticas/inputs aceptados DP-I-03; no se usa cuenta ficticia, guard true o flags para saltar validación. Backend inicial tampoco se reinicializa por iniciativa propia. No hay planfile/salida de plan ni recuento de cambios observado. Lectura estática del HCL expande **14 instancias de recursos** (8 red, 2 SG y 4 reglas), un data source; **no es resultado de plan** ni infraestructura existente.

| Comprobación real | Resultado |
| --- | --- |
| `terraform -version` | **1.17.1 / windows_amd64**, exit 0; sin actualización |
| Lock/provider | **6.73.0**, sin modificación; no init/descargas nuevos |
| `terraform fmt -recursive infra` | Exit 0; normalizó security.tf |
| `terraform fmt -check -recursive infra` | Exit 0 |
| `terraform -chdir=infra validate` | Exit 0, **Success! The configuration is valid.** |
| JSON bootstrap parse/revisión local | Nueve acciones Describe exactas, región/usuario delimitados; no prueba de aceptación IAM |
| State/planes | Sin state/plan real ni rastreado; `.terraform/`/tfvars reales siguen ignorados |
| `git diff --check` | Exit 0; solo avisos LF→CRLF de configuración existente, sin errores de whitespace |
| `git status` / `git diff --stat` | Rama feature/infrastructure; siete archivos modificados sin stage y tres nuevos sin seguimiento. Stat rastreado no incluye esos tres nuevos |

**E(T-INFRA-025) PARCIAL:** INFRA-029/032/038, PLD-INFRA-015/016/019, AC-I-01/GATE-I-A; fecha/entorno/commit arriba. Se ejecuta ahora solo la porción autorizada posible, no se aprueban 001/002/custodia/backup ni se declara completada. Resultados saneados en sesión/task, sin inventar paquete externo o retención duradera.

### 9.5. Dictamen y límites

T003/004/005/006 quedan **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: configuración/propuestas no satisfacen decisiones, dependencias ni pruebas cloud. **GATE-I-A PENDIENTE / NO SATISFECHO** por 001/002/custodia/costo integral/ownership y aceptación completa E025. **GATE-I-B PENDIENTE / NO SATISFECHO**: exige GATE-I-A, E(003–006), E(019–020), E(027–028), VPC/rutas/SG/SSM/IAM/salidas Atlas/bypass realmente verificados. Validate verde no sustituye esos criterios. Ningún otro gate/AC cerrado.

No recursos AWS ni IAM policies aplicados; no apply/destroy/import/plan, EC2/ALB/EIP/NAT/Redis/ECR/ACM/Secrets/DynamoDB/CloudWatch, backend remoto, workflows, pruebas UT/IT/ST, producto/Authentication/Program.cs/Dockerfile o 004 modificados. Spec/plan permanecen intactos. Reversión futura solo de cambios propios bajo permiso, sin operaciones AWS/Git destructivas. Sin stage/commit/push; HEAD conservado. Detener tras este bloque.

## 10. INFRA-2/3 — Revalidación IAM/AZ e intento de plan

### 10.1. Alcance y baseline — 2026-11-12

Autorización **VALIDACIÓN CONTROLADA / NO APPLY**: revalidar STS/AZ, fmt/check/validate, intentar plan, registrar solo hechos en task/memoria. Rama verificada **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**. Se preservan los siete archivos modificados y tres nuevos de §9; 0 staged. No se edita HCL, provider, lock, política bootstrap, README de infra o spec/plan en este turno. El responsable informa **bootstrap IAM aplicada**; no se consulta IAM para certificar asignaciones/contenido/ausencia de privilegios amplios.

### 10.2. STS/AZ: evidencia real complementaria E(003)/E(005)

`aws sts get-caller-identity --profile enver-infra` (salida JSON capturada en memoria, sin pager): **exit 0**, ARN coincide con `user/enver-terraform-local`, Account presente y válido; se suprimen ID y ARN completos. La identidad se verifica nuevamente antes del intento de plan.

`aws ec2 describe-availability-zones --region us-east-1 --profile enver-infra` (JSON, sin pager): **exit 0**, seis AZ `available`, tipo `availability-zone`, opt-in `opt-in-not-required`. Mapeo observado **para esta cuenta**, no nombres inferidos:

| AZ | Zone ID |
| --- | --- |
| us-east-1a | use1-az1 |
| us-east-1b | use1-az2 |
| us-east-1e | use1-az3 |
| us-east-1c | use1-az4 |
| us-east-1f | use1-az5 |
| us-east-1d | use1-az6 |

El algoritmo existente de Zone IDs ordenados propone **a → use1-az1/us-east-1a**, **b → use1-az2/us-east-1b**. Queda acreditado mínimo dos AZ disponibles, no capacidad/compatibilidad de un futuro tipo EC2 ni aceptación definitiva del perfil de red. La denegación AZ de §9 es histórica y no se repite. Sin AccessDenied/UnauthorizedOperation nuevos en estas llamadas; **no se certifica aún suficiencia IAM para todo Terraform**.

### 10.3. E(T-INFRA-025): validaciones e intento fallido de plan

Terraform observado **1.17.1/windows_amd64**; provider/lock **6.73.0** conservados, sin actualización/descarga/init nuevos. Comandos `terraform fmt -recursive infra`, `terraform fmt -check -recursive infra` y `terraform -chdir=infra validate`: **exit 0**; validate: **Success! The configuration is valid.** Fmt no requirió modificaciones.

Para plan: perfil externo `AWS_PROFILE=enver-infra`, región us-east-1; `TF_VAR_*` solo en proceso: project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8 (deriva /24 .0 y .1), common_tags de ejemplo existente; aws_account_id obtenido de STS, no ficticio ni publicado. `network_design_approved=false` conserva DP-I-03 pendiente, sin inventar aceptación para lograr verde. Sin inputs automáticos/state inesperados; inspección previa limita resource/data types a red/SG/AZ existentes. Sin credenciales ambientales que sustituyan el perfil. Variables del proceso restauradas; sin tfvars real o planfile guardado, sin logging Terraform a archivo.

Intento ejecutado **una vez**:

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 1
Error: Backend initialization required, please run "terraform init"
Reason: Initial configuration of the requested backend "local"
```

Fallo **anterior a evaluación del provider/data source y recursos**, no IAM. La baseline inicial usó `init -backend=false`; no se ejecuta init/reconfigure/migrate-state por inferencia. **No hay resumen válido add/change/destroy ni data sources leídos por Terraform**; no se registra 0 destroy como resultado observado. La consulta AZ anterior fue AWS CLI, no un plan. Ningún permiso adicional propuesto: no hubo nueva acción denegada que lo justifique. Una política aplicada informada y AZ exit 0 no acreditan todas las llamadas futuras.

Lectura estática: **14 instancias de recursos definidos**, 8 red (VPC, dos subnets, IGW, route table, ruta IGW, dos asociaciones), 2 SG y 4 reglas; un data source AZ. No tipos fuera del alcance en el HCL revisado; **sin inventario de plan exitoso**. Sin EC2/ALB/ECR/Redis/DynamoDB/ACM/Secrets/CloudWatch/EIP/NAT definidos o creados. Costo conocido del diseño actual sin workloads/IP: **USD 0 recurrente directo estimado** de red básica/SG, conforme a fuentes de §9; sin nueva consulta Billing/precios ni gratuidad de plataforma, sin recursos facturables añadidos.

### 10.4. Reevaluación y detención

- **T-INFRA-003/004/005/006: PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, sin cambios de estado. AZ ahora acreditadas, no topología/egreso/allowlist Atlas, contratos IAM/runtime, SSM o TLS interno aceptados. E(004)/E(006) sin nueva prueba cloud.
- **T-INFRA-025: PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. Agrega intento fallido de plan y causa exacta; no completa state/custodia/backup o dependencias 001/002.
- **GATE-I-A PENDIENTE / NO SATISFECHO**: aceptación 001/002/025, costo integral, ownership y custodia/state aún incompletos. **GATE-I-B PENDIENTE / NO SATISFECHO**: exige I-A, E(003–006), E(019–020), E(027–028) y verificaciones cloud. Nueve gates permanecen pendientes.
- Antes de reintentar plan: autorización específica de inicialización del backend local, revisión de custodia/state/backup y decisiones/inputs pendientes; no tratar `network_design_approved=true` como mero arreglo técnico. Un init futuro no autoriza apply, migración, permisos extra o provisión.

Comprobación Git: `git diff --check` **exit 0**, solo avisos de normalización LF→CRLF; `git status` confirma siete modificados/tres nuevos y **0 staged**, `git diff --stat` revisado. Lock/provider/versiones/backend/spec/plan sin cambios respecto a HEAD; no state, planfile, tfvars real o lock de state en infra. No se agregan archivos al índice para revisar los nuevos.

Sin mutaciones AWS/IAM, apply/destroy/import, init, pruebas backend, workflows, producto o 004. Solo se actualizan este task y memoria, manteniendo §9 como historial. Sin stage/commit/push; HEAD conservado. Detener al registrar esta validación.

## 11. Backend local inicializado y plan INFRA-2/3 válido

### 11.1. Autorización y decisión acotada — 2026-11-12

Solicitud **INICIALIZAR BACKEND LOCAL + PLAN INFRA-2/3 / VALIDACIÓN CONTROLADA NO APPLY**. Baseline verificada: **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**, siete modificados/tres nuevos preexistentes, 0 staged. Solo se editan task/memoria; HCL, ejemplo tfvars, README de infra, política bootstrap, spec/plan, backend fuente, versiones y lock se conservan.

**Network design APROBADO PARA DISEÑO Y PLAN, NO PARA APPLY**: opción A, ALB y dos EC2 futuros en públicas, backend solo SG ALB, SSH público prohibido, SSM futuro, sin NAT Gateway; dos EIP futuras, una por host para salida Atlas; TLS público futuro ALB+ACM. Inputs enver/dev/us-east-1, VPC **10.20.0.0/16**, subnets **10.20.0.0/24** y **10.20.1.0/24**. La aprobación habilita `network_design_approved=true` en este plan, no cargos/provisión, EIP implementadas, TLS interno decidido, segmentación fina de egreso aceptada o aceptación integral DP-I-03/otras decisiones.

### 11.2. Init y validación: E(002)/E(025) parciales

Un prechequeo de hashes falló porque el cmdlet Get-FileHash no estaba disponible, **antes de invocar Terraform**; se sustituyó por SHA-256 de .NET en memoria, sin instalar herramientas. Init se ejecutó una sola vez, no interactivo para detenerse en lugar de aceptar migraciones inesperadas:

```text
terraform -chdir=infra init -input=false -lockfile=readonly -no-color
Exit: 0
Successfully configured the backend "local"!
Reusing version 6.73.0 of hashicorp/aws from the dependency lock file
Using previously-installed hashicorp/aws v6.73.0
Terraform has been successfully initialized!
```

Metadata local comprobada en `infra/.terraform/terraform.tfstate`: tipo local, path terraform.tfstate; **no es un state de recursos aplicados**. Sin reconfigure/migrate-state, S3, backend remoto o DynamoDB locking. Hashes de fuentes infra/bootstrap/lock idénticos antes/después de init y fmt; no upgrade/descarga de provider nuevo. `terraform fmt -recursive infra`, `terraform fmt -check -recursive infra` y `terraform -chdir=infra validate`: **exit 0**, validate **Success! The configuration is valid.**

`git check-ignore --no-index` confirma exclusiones de terraform.tfstate, .backup y sufijos, `.terraform/` y tfvars real; lock y tfvars.example siguen versionables. Ninguno de esos artefactos sensibles rastreado. Tras plan: **sin infra/terraform.tfstate de recursos, planfile ni tfvars real**; solo metadata de backend bajo `.terraform/` ignorada. No se demuestra cifrado/ACL/custodia/backup/restauración por inicializar o ignorar archivos.

### 11.3. Plan real: E(003–006)/E(025) parciales

STS previo al plan, perfil externo **enver-infra**, **exit 0**, usuario esperado enver-terraform-local y Account válido sin publicarlos completos. Inputs no secretos solo en proceso: enver/dev/us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8, common_tags={} (default existente), cuenta guard obtenida de STS. CLI para **network_design_approved=true**; sin alterar ejemplo o escribir tfvars. Sin credenciales ambientales que sustituyan perfil, TF_CLI_ARGS o logging a archivo; variables restauradas al terminar. Un único plan, sin -out/planfile:

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode -var=network_design_approved=true
Exit: 2
data.aws_availability_zones.available: Read complete after 0s [id=us-east-1]
Plan: 14 to add, 0 to change, 0 to destroy.
```

**Exit 2 es éxito con cambios previstos** usando detailed-exitcode, no fallo. Inventario contrastado exactamente contra direcciones autorizadas:

| Tipo/dirección | Add |
| --- | --- |
| aws_vpc.enver | 1 |
| aws_subnet.public["a"], ["b"] | 2 |
| aws_internet_gateway.enver | 1 |
| aws_route_table.public | 1 |
| aws_route.public_internet | 1 |
| aws_route_table_association.public["a"], ["b"] | 2 |
| aws_security_group.sg_alb, sg_backend | 2 |
| aws_vpc_security_group_ingress_rule.alb_https, backend_from_alb | 2 |
| aws_vpc_security_group_egress_rule.alb_to_backend, backend_initial | 2 |
| **Total recursos / change / destroy** | **14 / 0 / 0** |

Data source **aws_availability_zones.available leído**, no recurso a crear. Plan confirma **a: use1-az1 / 10.20.0.0/24**, **b: use1-az2 / 10.20.1.0/24** (mapeo nombres us-east-1a/us-east-1b acreditado por CLI en §10). VPC /16, DNS support/hostnames true, IP automática de subnets false y ruta default IPv4 vía IGW. SG/reglas: 443 público solo ALB, backend/8080 solo SG ALB, egreso ALB a backend/8080, egreso backend IPv4 amplio provisional. Sin 22/80, SG no acredita escucha, SSM o TLS.

**Sin recursos inesperados**: no instancias EC2, EIP, ALB/target group, ECR, ACM, Redis/ElastiCache, DynamoDB, Secrets, CloudWatch, Route53 o NAT Gateway. **Sin AccessDenied** en este plan; **sin permiso incremental requerido observado**, ni políticas modificadas. Este plan inicial no certifica permisos de apply/refresh futuro, todas las acciones bootstrap o ausencia de privilegios amplios. Sin eludir allowed_account_ids ni crear state/recursos para probar permisos.

Costo directo recurrente estimado de **este inventario sin workloads/IP asignadas: USD 0** por VPC/subnets/IGW/rutas/SG, según fuentes previamente acreditadas (§9), sin nueva consulta Billing ni gratuidad integral. Ningún componente facturable de workloads/IP/NAT añadido por este plan. **Dos EIP futuras ≈ USD 7.30 por 730 h (~mes)** a USD 0.005/IP-h previamente acreditado: fuera del inventario y **NO autorizadas para apply**. EC2/ALB futuros tampoco se cuentan aquí; costo integral/duración/crédito/reserva siguen sus pendientes.

### 11.4. Reevaluación y detención

**DEFINIDO**: HCL existente. **PLANIFICADO**: 14 creaciones de red/SG confirmadas por plan válido. **CREADO EN AWS**: ninguno por este bloque. E(003–006)/E025 amplían la trazabilidad de §§9–10 con plan real; no se inicia T026/revisión integral de todos los servicios ni se crea evidencia cloud.

T003/004/005/006 conservan **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, ahora con plan acreditado de sus componentes presentes. T004 no tiene EIP/allowlist/conectividad; T005 no completa contratos IAM/runtime ni permisos de provisión; T006 no tiene pruebas bypass/SSM/TLS interno/egreso final. T025 conserva **PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: init/plan no sustituyen custodia/backup/recuperación, dependencias y aceptación explícita. No se completan tareas por comandos verdes.

**GATE-I-A PENDIENTE / NO SATISFECHO** por 001/002, custodia/recuperación state, ownership y E025 integral. **GATE-I-B PENDIENTE / NO SATISFECHO**: exige I-A y verificaciones cloud E003–006/E019–020/E027–028. Nueve gates pendientes; spec/plan originales intactos, decisiones para apply todavía no aceptadas integralmente.

Comprobación Git: **git diff --check exit 0**, solo avisos LF→CRLF; status siete modificados/tres nuevos preexistentes, **0 staged**; stat revisado (no incluye nuevos sin seguimiento). HEAD y baseline protegida conservados; sin patrones básicos de secretos detectados en task/memoria (no análisis exhaustivo).

Sin apply/destroy/import, IAM mutado, recursos AWS creados, workloads, CI/CD, pruebas backend o producto modificado. Sin stage/commit/push; HEAD conservado. Detener tras documentar init/plan y pendientes, sin autorización implícita para siguiente tarea o apply.

## 12. IAM write mínima candidata para INFRA-2/3 — PREPARADA / NO APLICADA

### 12.1. Alcance y evidencia base — 2026-11-12

Baseline verificada **feature/infrastructure**, HEAD **f1a98a01e15c67297287133c27dc23279a649d09**, árbol limpio y 0 staged. Checkpoint previo `feat(infra): define networking and security baseline` integra los diez archivos revisados de INFRA-2/3; no acredita apply. Plan de §11 **14 add / 0 change / 0 destroy** conservado, no repetido porque ningún `.tf` cambia.

Se crea únicamente [terraform-local-network-apply-policy.json](../../infra/bootstrap/terraform-local-network-apply-policy.json); se actualizan task/memoria. [Read policy](../../infra/bootstrap/terraform-local-network-policy.json), HCL/variables/locals/outputs, lock, README, ejemplo tfvars, spec/plan y producto **intactos**. La read policy sigue informada aplicada; la write propuesta **NO está aplicada, adjunta, simulada ni validada por el servicio IAM**. No se consultan/modifican APIs de la cuenta AWS.

### 12.2. Derivación provider 6.73.0 y acciones Allow

Fuentes leídas del tag **v6.73.0**: [VPC](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_.go), [subnet](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_subnet.go), [IGW](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_internet_gateway.go), [tabla](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_route_table.go), [ruta](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_route.go), [asociación](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_route_table_association.go), [SG](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_security_group.go), [ingress/framework](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_security_group_ingress_rule.go), [egress](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_security_group_egress_rule.go) y [tags](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/tags_gen.go). Revisión pública por lectura, no ejecución de esas fuentes.

**22 acciones write únicas**, sin Describe duplicado:

| Recurso/operación | Allow ec2: incluido | Motivo observado |
| --- | --- | --- |
| VPC | CreateVpc, DeleteVpc, ModifyVpcAttribute | Crear/eliminar VPC; habilitar DNS. Modify limitado a **EnableDnsSupport/EnableDnsHostnames**, no métricas/tenancy |
| Dos subnets | CreateSubnet, DeleteSubnet | Creación IPv4 dedicada y eliminación |
| IGW | CreateInternetGateway, AttachInternetGateway, DetachInternetGateway, DeleteInternetGateway | Ciclo attach/detach y creación/eliminación |
| Tabla/asociaciones | CreateRouteTable, DeleteRouteTable, AssociateRouteTable, DisassociateRouteTable | Tabla pública y asociaciones de las dos subnets |
| Ruta | CreateRoute, DeleteRoute | Default IPv4 a IGW, no tag propio |
| SG | CreateSecurityGroup, DeleteSecurityGroup | Dos grupos propios |
| Reglas SG | AuthorizeSecurityGroupIngress, AuthorizeSecurityGroupEgress, RevokeSecurityGroupIngress, RevokeSecurityGroupEgress | Crear/eliminar cuatro reglas; **RevokeSecurityGroupEgress se usa también en Create del SG para quitar egreso IPv4/IPv6 predeterminado**, aun sin IPv6 desplegado |
| Tags al crear | CreateTags | Autorización adicional exigida para TagSpecifications atómicas en los seis tipos etiquetables |

**Excluidos por no pertenecer al create/delete del plan actual:** ReplaceRoute (Update), ReplaceRouteTableAssociation (Update), ModifySecurityGroupRules (Update), DeleteTags y CreateTags independiente (actualización de tags, no requerido para borrar recursos). ModifySubnetAttribute no se concede: con subnets IPv4 nuevas/no default y atributos del HCL/defaults false, el helper de creación modifica solo si difieren; map_public_ip_on_launch=false coincide con el default de creación. No se extiende por intuición a IPv6, DNS64, Outposts u otros atributos. Una divergencia real futura exige revisión incremental, no ampliar preventivamente. No IPAM, tenancy, endpoints, ENI create/delete/detach o limpieza de dependencias ajenas. No adoptar esta policy para un plan con updates/configuración distinta sin nueva revisión.

### 12.3. ARN, condiciones y tags

Contraste con [EC2 Service Authorization Reference](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ec2.html), [tag-on-create](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/supported-iam-actions-tagging.html), [variables IAM](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_variables.html) y [claves globales](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_condition-keys.html). Las mayúsculas de **ec2:Attribute = EnableDnsSupport/EnableDnsHostnames** se contrastan con el [ejemplo oficial AWS](https://docs.aws.amazon.com/aws-managed-policy/latest/reference/AWS-SSM-RemediationAutomation-ExecutionRolePolicy.html); solo se toma ese detalle técnico, **no se propone adjuntar esa política amplia**.

- **Ningún Resource literal `"*"` en la write policy**; todos son ARN `arn:aws:ec2:us-east-1:*:<tipo>/*`. Tipos: vpc, subnet, internet-gateway, route-table, security-group y security-group-rule. Cada `/*` es necesario porque sus IDs todavía no existen; no wildcard de servicio/región/tipo/acción. Rutas y asociaciones no tienen ARN/tag propio en estas autorizaciones: se controlan por tabla/subnet existentes.
- Wildcard **de cuenta en ARN** evita publicar/hardcodear Account ID; **en cada statement** `aws:ResourceAccount = ${aws:PrincipalAccount}` lo limita al dueño de la cuenta del usuario. Las acciones seleccionadas no están en las excepciones EC2 documentadas para ResourceAccount. La variable se usa en **Condition**, no en el campo cuenta del ARN: IAM no permite variables antes del quinto `:`. Si falta la clave requerida se deniega; no StringEqualsIfExists que abra el control.
- Cada statement exige **aws:RequestedRegion=us-east-1** y **aws:username=enver-terraform-local**. RequestedRegion está disponible para estos endpoints EC2 regionales; username lo está para IAM users, no roles/root/SSO: no se supone portabilidad de esta policy a roles futuros.
- Creación: **RequestTag Project=enver, Environment=dev, ManagedBy=Terraform**; recursos principales además exigen Name dentro de los nombres existentes en locals/HCL. Mutación/eliminación y referencias a padres: **ResourceTag** de control y Name existente de VPC/IGW/tabla/subnets/SG. CreateSubnet/CreateRouteTable/CreateSecurityGroup autorizan por separado el ARN nuevo con RequestTag y la VPC padre con ResourceTag; attach/detach IGW cubren ambos recursos. Asociaciones cubren tabla y subnets. No se exige ResourceTag sobre recursos aún inexistentes ni RequestTag en rutas/revokes/delete.
- Provider pasa **TagSpecifications en creación** para VPC/subnet/IGW/tabla/SG y reglas standalone: no hay un intervalo sin tags que haga imposible satisfacer el create normal. Reglas SG usan ARN del SG con ResourceTag y ARN nuevo de regla con RequestTag. AWS solo evalúa el segundo nivel cuando se envía TagSpecifications; **esto no es garantía universal de tags en llamadas manuales sin TagSpecifications**. Revokes se autorizan contra SG padre etiquetado, no contra tags de la regla predeterminada sin etiquetar.
- **CreateTags únicamente con ec2:CreateAction** en las siete APIs de creación/authorize usadas; no permite retaggear/adoptar recursos existentes. TagKeys se limita a **Project, Environment, ManagedBy, Name** (las tres primeras obligatorias por RequestTag). Las reglas no tienen Name en HCL y no se les exige. Cualquier common_tags futuro fuera de esas claves exige revisión; el plan acreditado usó common_tags={}.

**Límites explícitos:** tags/nombres no son IDs exclusivos ni un límite de cardinalidad; esta policy no impone exactamente 14 recursos, CIDR/puertos/destinos exactos ni demuestra quién creó cada recurso preexistente con iguales tags/nombres. La autorización operativa sigue limitada al plan revisado; antes de asignación/apply debe revisarse ausencia de colisiones/adopción, otras políticas/boundaries/SCP y, cuando haya IDs, evaluar pinning para cleanup. No permite retagging por sí misma, pero no neutraliza grants amplios de otras políticas ni cambios de tags por otros principales; es Allow acotado, **no permissions boundary**. No se finge una restricción IAM sobre campos sin condición soportada.

### 12.4. Describe adicionales y rollback pendiente

Read policy **sin modificar**; se documenta propuesta mínima separada para revisión/autorización posterior:

| Describe adicional | Evidencia en 6.73.0 / necesidad |
| --- | --- |
| **ec2:DescribeNetworkAcls** | VPC Read llama findVPCDefaultNetworkACL para atributo computado del NACL automático; un error allí se registra como warning, pero impide lectura íntegra. No autoriza crear/modificar/borrar NACL |
| **ec2:DescribeNetworkInterfaces** | [deleteLingeringENIs](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_network_interface.go#L1738) se invoca antes de Delete de subnet y SG, incluso cuando no hay ENIs. Su error sí bloquea eliminación |

Ambas lecturas requieren **Resource `"*"`**: SAR no ofrece tipos ARN para esas acciones; conservar región us-east-1 y usuario enver-terraform-local de la read policy. **No incluidas en la write ni concedidas**, no se prueba IAM por llamadas AWS. Se deberá autorizar explícitamente la ampliación read aparte; la policy write más la read actual **no se declaran suficientes para rollback integral** mientras falte DescribeNetworkInterfaces. No se agregan DescribeVpcEndpoints o permisos GuardDuty/ENI para caminos de limpieza de servicios no desplegados; ante dependencias inesperadas, detener y revisar sin borrar componentes ajenos.

Los deletes/detach/disassociate/revokes preparados solo alcanzan el cohort de red etiquetado/nombres de esta fase, sujeto a las limitaciones de §12.3. AWS crea defaults implícitos al crear una VPC; no se añaden recursos Terraform o permisos de gestión de NACL/SG/tabla default. DeleteVpc elimina sus defaults en el ciclo normal sin workloads. Preparar eliminación **NO autoriza terraform destroy ni rollback**; requieren permiso propio, plan/inventario/estado y costo revisados, orden inverso de dependencias y custodia recuperable.

### 12.5. Validación, costo y estado

JSON parseado localmente: **válido**, 9 statements Allow, **22 acciones exactas**, todas EC2 específicas; assertions locales de ARN/condiciones/tags/ec2:Attribute/CreateAction correctas. **5318 caracteres compactados**, por debajo de 6144 de una customer-managed policy; excede 2048 agregados de inline policies de IAM user: no proponer pegarla como inline del usuario. Esto no crea ni asigna una managed policy; formato/asignación requieren revisión del responsable administrativo. **No hay validación semántica IAM/simulación o eficacia acreditada**. AWS CLI no aporta un validador semántico IAM puramente local: Access Analyzer validate-policy es una API remota; no se llama ni se usa create/put/attach policy para validar.

`terraform fmt -check -recursive infra` **exit 0**; `terraform -chdir=infra validate` **exit 0 / Success! The configuration is valid.** HCL/read policy/lock intactos, plan **14/0/0** previo conservado, no repetido. JSON no está referenciado por HCL: no modifica el inventario ni despliega permisos. Sin permisos RunInstances/TerminateInstances, EIP/NAT, ELB/ECR/ElastiCache/DynamoDB/Secrets/ACM/Route53/CloudWatch/IAM/SSM/KMS/S3/AutoScaling; sin ec2:* o acciones wildcard, AdministratorAccess/PowerUserAccess.

Sin recursos/cargos nuevos por preparar el archivo. Inventario de red básica/SG sin workloads/IP sigue con **USD 0 recurrente directo estimado** según §11; no se habilitan EC2/ALB/EIP/NAT, no aprobación de gasto integral ni cierre de presupuesto/custodia/ownership. **T-INFRA-005 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. **GATE-I-A e I-B PENDIENTE / NO SATISFECHO**, nueve gates pendientes; ninguna tarea completada por este documento. T003/004/006 y 001/002 conservan sus pendientes.

Sin apply/destroy/import/init/plan, AWS/IAM modificado, pruebas backend, workflows o producto alterado. Solo nuevo JSON y task/memoria; sin stage/commit/push, HEAD conservado. Detener tras preparar propuesta y registrar sus límites, sin aplicar políticas o infraestructura.

## 13. IAM Network Read/Apply informadas aplicadas y replan pre-apply — NO APPLY

### 13.1. Autorización, fuentes y límites — 2026-11-12

Solicitud **VALIDAR IAM NETWORK APPLY + REPLAN PRE-APPLY / VALIDACIÓN CONTROLADA / NO APPLY**. Baseline comprobada **feature/infrastructure**, HEAD **f1a98a01e15c67297287133c27dc23279a649d09**. Árbol inicial: task/memoria modificados y write JSON sin seguimiento, del incremento §12; **0 staged**, se conserva ese trabajo. Solo task/memoria editados ahora; archivos infra, JSON read/write, HCL, lock, ejemplo, spec/plan/004 y producto intactos.

**Informado por el responsable, no enumerado remotamente:**

- **IAM Network Read: APLICADA**, inline **ENVER-Terraform-Network-Read**, ahora con DescribeNetworkAcls/DescribeNetworkInterfaces.
- **IAM Network Apply: APLICADA**, customer-managed **ENVER-Terraform-Network-Apply**, adjunta directamente a enver-terraform-local desde el JSON local de §12.
- Responsable declara sin AdministratorAccess/PowerUserAccess/ec2:*. No se auditan otras policies, contenido remoto, versiones, boundaries o SCP; esa ausencia global no queda independientemente acreditada.
- **Terraform apply: TODAVÍA NO AUTORIZADO.** Asignación IAM no sustituye autorización operativa, gates o revisión de costos/state. Ningún cambio AWS/IAM por el agente.

El JSON read del repositorio conserva las nueve acciones históricas y **no representa íntegramente la ampliación remota informada**; no se sincroniza porque esta solicitud solo autoriza editar task/memoria. §12 conserva su estado cronológico de policy preparada/no aplicada; este registro supera aquel pendiente de asignación **según confirmación del responsable**, no reescribe su historia ni acredita rollback integral.

### 13.2. Preflight y lecturas reales

Primer prechequeo local terminó con exit 1 antes de ejecutar AWS/Terraform: AWS CLI no estaba en la ruta supuesta bajo Program Files. Se localizó la instalación existente en `%LOCALAPPDATA%/Programs/Amazon/AWSCLIV2/aws.exe`; Terraform en su directorio WinGet existente. Sin instalación, cambio de PATH/configuración o reintento oculto de plan. Después, una ejecución de cada STS/Describe/fmt/validate/plan.

Sin overrides ambientales de credenciales, TF_VAR, TF_CLI_ARGS o TF_LOG; sin tfvars reales/autoload o state de recursos preexistente. Perfil **enver-infra**, región **us-east-1**; valores e identificadores sensibles no publicados. Salidas capturadas en memoria de proceso, sin archivos crudos/planfiles/tfvars reales guardados.

| Comando | Resultado observado |
| --- | --- |
| aws sts get-caller-identity --profile enver-infra | **exit 0**, ARN de IAM user enver-terraform-local y cuenta válida; cuenta usada solo para allowed_account_ids, no publicada |
| aws ec2 describe-network-acls --region us-east-1 --profile enver-infra | **exit 0**, 1 ACL existente devuelta; IDs/reglas suprimidos; no creada por este turno |
| aws ec2 describe-network-interfaces --region us-east-1 --profile enver-infra | **exit 0**, 0 interfaces devueltas |
| terraform fmt -check -recursive infra | **exit 0** |
| terraform -chdir=infra validate -no-color | **exit 0**, Success! The configuration is valid. |

Read permissions **suficientes para las llamadas efectivamente ejecutadas**, incluidas las dos adicionales. No acredita todas las lecturas de recursos futuros: sin recursos gestionados existentes, el replan no ejercita cada Read del provider. El anterior bloqueo documentado por falta de esas dos lecturas está superado para las llamadas directas; no se extrapola a writes/rollback.

Revisión local write JSON: parseo válido, **9 statements Allow / 22 acciones únicas** idénticas al catálogo §12; sin NotAction, acciones wildcard o fuera de allowlist. **Sin RunInstances/TerminateInstances/AllocateAddress/AssociateAddress/CreateNatGateway ni ELB/IAM/ECR/ElastiCache/DynamoDB/Secrets/ACM/Route53/CloudWatch/SSM/KMS/S3/AutoScaling**. ARN/condiciones/tags se conservan, así como limitaciones de §12. No auditoría del JSON remoto ni prueba de permisos efectivos de escritura por el éxito del plan.

### 13.3. Único replan, inventario y comparación

Inputs temporales de proceso: **project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8, network_design_approved=true, common_tags={}**, y aws_account_id tomado del STS verificado como en §11. `AWS_PROFILE=enver-infra`; variables restauradas al terminar. Sin modificar ejemplos o escribir tfvars; no init ni plan -out.

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 2 (éxito con cambios previstos)
data.aws_availability_zones.available: Read complete after 1s [id=us-east-1]
Plan: 14 to add, 0 to change, 0 to destroy.
```

Se comparan **las 14 direcciones exactas** con §11, no solo la suma; coinciden, sin recurso añadido/retirado, update o destroy:

| Direcciones reales de creación | Add |
| --- | --- |
| aws_vpc.enver | 1 |
| aws_subnet.public["a"], public["b"] | 2 |
| aws_internet_gateway.enver | 1 |
| aws_route_table.public | 1 |
| aws_route.public_internet | 1 |
| aws_route_table_association.public["a"], public["b"] | 2 |
| aws_security_group.sg_alb, sg_backend | 2 |
| aws_vpc_security_group_ingress_rule.alb_https, backend_from_alb | 2 |
| aws_vpc_security_group_egress_rule.alb_to_backend, backend_initial | 2 |
| **Total add / change / destroy** | **14 / 0 / 0** |

Data source AZ leído, no creación. Subnets **10.20.0.0/24 / use1-az1** y **10.20.1.0/24 / use1-az2**; VPC /16, DNS support/hostnames true, map_public_ip_on_launch false, ruta 0.0.0.0/0 a IGW. Reglas sin diferencia contra baseline: ALB HTTPS 443 público, backend 8080 exclusivamente desde SG ALB, egreso ALB a backend 8080, backend IPv4 amplio **provisional**; no SSH/80. No se certifican puertos reales, TLS/SSM o bypass.

**Recursos inesperados: ninguno.** No instancias EC2, ALB, EIP, NAT Gateway, Redis/ElastiCache, IAM, ECR, DynamoDB o secretos. **AccessDenied/UnauthorizedOperation: ninguno** en STS, dos Describe y plan. **Permiso mínimo adicional observado: ninguno**; no se propone ampliar IAM preventivamente. No hay acción/recurso/fase denegada que registrar. Esto **no prueba Create/Delete/Attach/Modify/Authorize/Revoke/CreateTags**, no ejecutados; sin simulación IAM o dry-run write.

Hashes SHA-256 antes/después de fuentes infra/policies/lock/README/ejemplo **sin cambios (0)**; comandos fmt/validate/plan no alteran esos archivos. Sin planfile guardado: esta revisión no garantiza el resultado de un apply futuro; requiere nuevo alcance explícito y revisión vigente antes de provisión.

### 13.4. Costos, reevaluación y detención

**Ningún costo directo nuevo identificado en el inventario contra baseline.** Se conserva la estimación previa **USD 0 recurrente directo para VPC/subnets/IGW/rutas/SG sin workloads/IP**, no costo cero absoluto, factura consultada o gratuidad de plataforma. No tráfico/workloads/IP/NAT incluidos, sin nueva consulta precios/Billing. EC2/ALB/EIP/Redis siguen fuera de este plan; costo integral, duración, cobertura/vigencia del crédito, reserva y ownership pendientes.

- **T-INFRA-003 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** inventario repetido y coincidente; dependencias 001/002/025, solapamientos externos y aceptación restantes pendientes.
- **T-INFRA-004 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** ruta planificada; sin EIP/allowlist Atlas /32 o conectividad real, ni costo/egreso integral aceptados.
- **T-INFRA-005 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** asignación Read/Apply informada, dos lecturas acreditadas; writes/rollback no probados, separación runtime/deployment/SSM/secretos/authority, políticas efectivas y aceptación integral pendientes.
- **T-INFRA-006 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** SG/reglas planificados sin cambios; faltan cloud/bypass/SSM/TLS interno/egreso final.
- **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE:** fmt/validate/replan reales; custodia/backup/recuperación/state, dependencias y E025 integral pendientes.
- **GATE-I-A PENDIENTE / NO SATISFECHO:** ownership/costos/state y aceptación E001/E002/E025 pendientes. **GATE-I-B PENDIENTE / NO SATISFECHO:** I-A y evidencias cloud E003–006/E019–020/E027–028 pendientes. Nueve gates pendientes, ninguna tarea completada ni salto automático a 026/027.

**DEFINIDO:** HCL. **PLANIFICADO:** 14 creaciones. **CREADO EN AWS por este turno:** ninguno. IAM configurado previamente por el responsable se registra aparte, no como provisión Terraform. Sin apply/destroy/import, recursos manuales, IAM mutado por el agente, backend tests o CI/CD; sin stage/commit/push. HEAD conservado, solo task/memoria editados; detener sin siguiente incremento automático.

## 14. Primer apply controlado — red base INFRA-2/3 CREADA Y VERIFICADA

### 14.1. Autorización y gate operativo — 2026-11-12

Solicitud **PRIMER APPLY CONTROLADO — NETWORK BASELINE INFRA-2/3 / BUILD CLOUD CONTROLADO**: autorización expresa solo de 14 recursos gestionados de red/SG, condicionada a plan guardado revisado, sin workloads, cambios IAM o destroy/import. No acepta tareas/gates integralmente. HEAD de infraestructura aplicado **0958e83b887a73e1bdae2ff8b25753f5a52d0f0c**, rama **feature/infrastructure**, working tree **limpio / 0 staged** antes del plan y nuevamente antes del apply. Commit previo `chore(infra): prepare network apply permissions` contiene exactamente JSON write/task/memoria; no se hace commit ahora.

STS **exit 0**, perfil **enver-infra**, IAM user **enver-terraform-local** confirmado; cuenta validada y usada para allowed_account_ids sin publicarla. Terraform **1.17.1** / provider **6.73.0** comprobados localmente con version -json. Backend **local**, path terraform.tfstate, ya inicializado: **init no necesario/no ejecutado**, sin migración/reconfigure/backend remoto. No state de recursos ni archivo enver-network.tfplan preexistentes; no se sobrescribe trabajo ajeno. Sin overrides de credenciales/TF_VAR/TF_CLI_ARGS/TF_LOG ni tfvars reales/autoload; inputs temporales y entorno restaurados al terminar.

**Inputs exactos:** project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8 → subnets 10.20.0.0/24 y 10.20.1.0/24; network_design_approved=true; common_tags={} y account guard de STS como baseline previa. No nuevos destinos/valores de arquitectura. `terraform fmt -check -recursive infra` **exit 0** y `terraform -chdir=infra validate -no-color` **exit 0 / Success! The configuration is valid.**

### 14.2. Plan guardado revisado y trazabilidad

```text
terraform -chdir=infra plan -input=false -no-color -out=enver-network.tfplan -detailed-exitcode
Exit: 2
Plan: 14 to add, 0 to change, 0 to destroy.
terraform -chdir=infra show -no-color enver-network.tfplan
Exit: 0; Plan: 14 to add, 0 to change, 0 to destroy.
```

Un solo plan pre-apply generado, timestamp interno **2026-11-12T19:04:08Z**. Se revisan salida humana y `show -json` en memoria: **14 direcciones exactas de §13**, todas con acción create, 0 update/delete, ningún tipo prohibido; tags, VPC/CIDR, subnets/ZoneIds, DNS, IP automática false y cuatro reglas/relaciones SG coinciden con HCL. Data source AZ leído, no recurso creado; checks del plan **pass**.

La primera comprobación auxiliar JSON se detuvo antes del apply por supuestos incorrectos del lector: `@($null).Count=1` para child_modules ausente y expectativa de data AZ en planned_values (ya leído, ausente allí; presente en configuration). Lectura diagnóstica del **mismo archivo** confirmó cero módulos hijos y ninguna dirección inesperada. Se corrigió solo la comprobación en memoria: no hubo cambio HCL, nuevo plan, error Terraform o recurso prohibido; la revisión del archivo existente terminó **PASS** antes de aplicar. No se oculta este fallo local ni se trata como plan diferente.

- SHA-256 del plan revisado/aplicado: **0c323dfcd5c28ec219e69b1b6b5db5b3fd496abb4bf4426aa82fd76849b2d53f**.
- Fingerprint SHA-256 agregado de fuentes versionadas infra (rutas ordenadas + hash de contenido): **263fd6088ae9f7c3da63f8c47d72aed48819109608c93a981e748bbeaf0c45d2**.
- Ambos verificados iguales justo antes del apply y después; ninguna regeneración de plan, edición HCL/provider/lock/JSON o uso de -target/-replace/-refresh-only. Gate operativo pre-apply **APROBADO / SATISFECHO PARA ESTE COMANDO**, distinto de GATE-I-A/I-B, que no se cierran.

### 14.3. Apply exacto y post-plan

**Inicio apply UTC: 2026-11-12T19:06:47.1094767+00:00. Fin UTC: 2026-11-12T19:07:04.6559714+00:00.**

```text
terraform -chdir=infra apply -input=false -no-color enver-network.tfplan
Exit: 0
Apply complete! Resources: 14 added, 0 changed, 0 destroyed.

terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 0
No changes. Your infrastructure matches the configuration.
```

**Apply completo, no parcial; sin errores/AccessDenied/UnauthorizedOperation**, sin reintentos, ampliación IAM o cleanup. Post-plan único con exactamente los mismos inputs/perfil: **0 add / 0 change / 0 destroy**, refresca los 14 recursos sin denegaciones. Solo el plan inicial guardado fue aplicado; el post-plan no tiene -out ni se aplica.

| Dirección Terraform creada | ID observado (no secreto) |
| --- | --- |
| aws_vpc.enver | vpc-060f67e600affd569 |
| aws_subnet.public["a"] | subnet-0120d869dd659e2a1 |
| aws_subnet.public["b"] | subnet-01934348f5d6985aa |
| aws_internet_gateway.enver | igw-06e8461bf7317dc7d |
| aws_route_table.public | rtb-0a80f643c062696f9 |
| aws_route.public_internet | r-rtb-0a80f643c062696f91080289494 |
| aws_route_table_association.public["a"] | rtbassoc-09d224d0d751f3de7 |
| aws_route_table_association.public["b"] | rtbassoc-02e7f934b3ac07704 |
| aws_security_group.sg_alb | sg-0dacf0df5da30b4f7 |
| aws_security_group.sg_backend | sg-070d06cfebe866427 |
| aws_vpc_security_group_ingress_rule.alb_https | sgr-0049d608b24aa0375 |
| aws_vpc_security_group_ingress_rule.backend_from_alb | sgr-0ab23de9320e08c1c |
| aws_vpc_security_group_egress_rule.alb_to_backend | sgr-076c7db58a1da7c31 |
| aws_vpc_security_group_egress_rule.backend_initial | sgr-03b3c6fa937265c8e |

14 son **recursos gestionados Terraform**, no recuento de todo objeto automático AWS: al crear VPC AWS añade sus defaults implícitos (NACL/tabla/SG/default route local), no gestionados como recursos adicionales de este HCL. Ningún servicio/workload prohibido creado por este apply según plan guardado, salida apply y state. No se enumeran todas las cuentas/servicios ni se afirma inexistencia global de recursos ajenos.

### 14.4. Verificaciones AWS de lectura — 2026-11-12T19:09:09.1221599+00:00

Ocho comandos EC2 Describe con **--region us-east-1 --profile enver-infra**, todos **exit 0**, sin APIs de modificación:

| Lectura por IDs del apply | Evidencia real |
| --- | --- |
| describe-vpcs | VPC anterior available, CIDR **10.20.0.0/16**, tags Project=enver/Environment=dev/ManagedBy=Terraform/Name=enver-dev-vpc |
| describe-vpc-attribute (enableDnsSupport) | **true** |
| describe-vpc-attribute (enableDnsHostnames) | **true** |
| describe-subnets | Dos subnets available, **10.20.0.0/24 → use1-az1 / us-east-1a**, **10.20.1.0/24 → use1-az2 / us-east-1b**; ambas en VPC ENVER, **MapPublicIpOnLaunch=false**, tags/nombres correctos |
| describe-internet-gateways | IGW con **una** asociación available exclusivamente a VPC ENVER, tags correctos |
| describe-route-tables | Tabla ENVER, default **0.0.0.0/0 → IGW ENVER active**, ruta local /16; exactamente dos asociaciones explícitas associated, una por subnet, Main=false, tags correctos |
| describe-security-groups | Dos SG del apply en VPC ENVER, tags/nombres correctos |
| describe-security-group-rules | Exactamente cuatro reglas/IDs/tags esperados: ALB ingress **TCP443 desde 0.0.0.0/0**, egress solo **TCP8080 a SG backend**; backend ingress solo **TCP8080 desde SG ALB**, egress **todos protocolos IPv4 0.0.0.0/0 provisional** |

**Sin 22 público, sin 8080 público del backend ni ingress adicionales** en los SG gestionados; 443 público únicamente en SG ALB entre esos grupos. Son reglas verificadas, **no un ALB/servidor/HTTPS operativos o prueba activa de bypass**: todavía no existen hosts, target groups/listeners o SSM runtime en esta fase. Egreso amplio provisional no se convierte en diseño fino aceptado, TLS interno decidido o conectividad Atlas acreditada.

### 14.5. State, costos, tareas y gates

`infra/terraform.tfstate` creado/actualizado, última escritura **2026-11-12T19:07:04.5633341Z**, **serial 15**, **14 instancias managed / 0 tainted**. Lectura selectiva en memoria, **sin publicar state completo**. **terraform.tfstate.backup no existe** en la revisión posterior: no se presenta como respaldo creado. `.terraform/` sigue metadata/downloads locales. Plan/state/.backup y otros sufijos/tfvars reales/.terraform permanecen ignorados; **0 artefactos sensibles rastreados**, lock y ejemplo siguen versionables. Ignorar no acredita ACL/cifrado/custodia/backup/recuperación; state es ahora indispensable para recursos reales, conservarlo y no repetir el plan guardado ya aplicado.

Solo red base y SG materializados, **sin EC2/ALB/EIP/NAT/Redis/ElastiCache/target groups/ECR/ACM/Route53/DynamoDB/Secrets/CloudWatch/IAM/SSM** gestionados por este apply. **Ningún costo directo recurrente nuevo distinto del inventario previsto identificado**: se conserva estimación previa de red básica/SG sin workloads/IP **USD 0 recurrente directo**, no factura medida ni costo cero absoluto; tráfico y costo integral de plataforma/duración/crédito/reserva/ownership siguen pendientes, sin nueva consulta Billing/precios. No recursos facturables de workloads/IP/NAT habilitados por inferencia.

Reevaluación: **T-INFRA-003/004/005/006 continúan PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, ahora 003/006 con creación y verificación real cloud. 003 conserva dependencias/solapamientos externos; 004 sin EIP/allowlist Atlas/conectividad estable; 005 acredita las operaciones de provisión ejercitadas, no todos los permisos, mínimos globales o destroy/rollback, y falta IAM runtime/instance profile/SSM/servicios posteriores; 006 sin SSM/TLS interno/egreso fino/pruebas activas. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, suma evidencia plan/apply/post-plan, pero faltan custodia/backup/recovery, 001/002 y E025 integral. Los rótulos conservan su estado parcial; no reducen el avance cloud registrado a mera definición.

**GATE-I-A PENDIENTE / NO SATISFECHO:** 001/002, custodia/backup/recovery, costos/ownership y aceptación completa E025. **GATE-I-B PENDIENTE / NO SATISFECHO:** I-A, IAM runtime/SSM, restricciones Atlas, TLS interno/egreso y otras evidencias E019–020/E027–028 integrales. Nueve gates pendientes; no cierre automático, cumplimiento de dos instancias/producción o AUTH-027 integral. Este incremento no despliega Authentication, no implementa 004 ni ejecuta el alcance integral de 026–028; la operación puntual expresamente autorizada queda trazada en §14.

**DEFINIDO = PLANIFICADO = CREADO EN AWS** para estos 14 recursos, con verificación de propiedades enumeradas; no equivalencia con plataforma lista. Sin otras mutaciones, destroy/import, ampliación IAM, cambio HCL, pruebas backend o CI/CD. Solo task/memoria como cambios versionables; HEAD conservado, sin stage/commit/push. Detener para revisión humana, sin siguiente incremento o cleanup automático.

## 15. INFRA-4 — ECR / contrato de imagen: definición y plan, sin apply

### 15.1. Autorización, baseline y archivos — 2026-11-12

Solicitud expresa **BUILD CONTROLADO / PLAN PRIMERO / NO APPLY AUTOMÁTICO**, T-INFRA-007/008 y registro transversal 025. Lectura de spec/plan/task y Dockerfile/configuración, sin reinterpretar requisitos ni aceptar dependencias pendientes. Baseline verificada: **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, `docs(infra): record network baseline deployment`, árbol inicialmente **limpio / 0 staged**. Este checkpoint posterior registra el apply histórico §14; no fue otro apply.

Nuevos [registry.tf](../../infra/registry.tf) y [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json); modificados [outputs.tf](../../infra/outputs.tf), este task y [memoria §65](../../memoria.md). Sin nuevas variables/providers ni modificaciones networking/security, policies previas, lock, Dockerfile/producto/tests, spec/plan o 004. STS exit 0, perfil **enver-infra**, IAM user **enver-terraform-local**, región **us-east-1**, cuenta contrastada con owner del VPC en state, sin publicarla. Terraform **1.17.1/windows_amd64**, provider **6.73.0**, backend local inicializado; sin init/instalación/actualización.

Evidencia local: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-20261112\` (logs build/plan, resúmenes, hashes). El JSON completo de plan contiene state/identificadores internos: no imprimir/versionar/publicar. Carpeta temporal no garantiza conservación ni acredita custodia/backup/recovery del state. Prechecks auxiliares fallidos **Get-FileHash no disponible / AWS CLI no resuelto en PATH heredado**, ambos antes de AWS/Terraform; corregidos mediante SHA256 .NET y binarios existentes del usuario, sin instalar o cambiar PATH persistente.

### 15.2. Contenedor y build local, sin cambiar Dockerfile

- Dockerfile **existe**: runtime `mcr.microsoft.com/dotnet/aspnet:10.0`, SDK `mcr.microsoft.com/dotnet/sdk:10.0.403`; restore **--locked-mode**, build **--no-restore**, publish **--no-restore** con UseAppHost=false. global.json conserva **10.0.403 / rollForward:disable**.
- Stage **final FROM base**, hereda **USER $APP_UID**, WORKDIR `/app`, ENTRYPOINT **["dotnet", "Enver.API.dll"]**, EXPOSE **8080/8081**. launchSettings configura HTTP_PORTS=8080/HTTPS_PORTS=8081 solo para desarrollo Docker; EXPOSE no prueba listeners/TLS.
- Docker cliente/servidor **29.6.1**, Linux/amd64 disponible. Una ejecución `docker build --file Enver.API/Dockerfile --tag enver-api:infra-validation .`, **19:24:39.6050132Z–19:28:54.0723541Z**, **exit 0**; build informa **0 Warning(s), 0 Error(s)**. Tag local no existía antes. Sin push/login ECR, arranque de contenedor/aplicación o pruebas. Imagen local conservada, sin cleanup automático.
- `docker image inspect`: **Config.User=1654 / APP_UID=1654, non-root configurado en imagen final**; **DOTNET_VERSION=10.0.12 / ASPNET_VERSION=10.0.12**, **ASPNETCORE_HTTP_PORTS=8080**, linux/amd64, ENTRYPOINT indicado. **8080 acreditado como configuración/metadatos, no escucha probada ni conectividad cloud**.
- ID/RepoDigest **local** `sha256:32d2d2a952c4f1c4c9bb51c620a452b8acbee0ee591cc9f1828479f20fd10c33`: **no es digest acreditado de imagen publicada en ECR ni artefacto productivo elegido**. No variable de digest inventado.
- `.dockerignore` excluye state/backups/tfvars/planes/.terraform, configuración local/credenciales/material sensible usual. No secretos pasados como build args; esto no es auditoría de todas las capas, SBOM, scanning ni CI verde.
- **Gaps/límites:** bases por tags mutables, sin digest fijado; SDK fijo/locked restore no garantizan reproducción byte a byte. Pinning/revisión de vulnerabilidades/versiones soportadas necesitan evaluación y autorización separadas. .NET10.0.12 es resolución observada, no aprobación del parche cloud. No gap non-root observado en metadatos; endurecimiento runtime/secretos/readiness/forwarding/arquitectura hosts pendientes. Dockerfile intacto; build no extrapolado a UT/IT/ST o Authentication cloud.

### 15.3. ECR, scanning, cifrado y lifecycle propuestos

Un solo `aws_ecr_repository.api`, **enver-dev-api**, derivado de `${local.name_prefix}-api`; privado, tags comunes Project=enver/Environment=dev/ManagedBy=Terraform y Name=enver-dev-api. **IMMUTABLE**, sin excepciones latest; **force_delete=false** evita vaciar/borrar imágenes por el flujo Terraform normal. **AES256 administrado por ECR**, sin custom KMS/recursos o permisos KMS. Outputs no sensibles **ecr_repository_name**, **ecr_repository_url**, **ecr_repository_arn**; URL/ARN calculados, no existencia acreditada.

`image_scanning_configuration.scan_on_push=true` soportado por provider6.73.0 y presente en plan; no recurso de scanning global ni Inspector habilitado. La [documentación actual](https://docs.aws.amazon.com/AmazonECR/latest/userguide/image-scanning-basic-enabling.html) describe BASIC por defecto/filtros globales; [AWS](https://aws.amazon.com/blogs/containers/container-scanning-updates-in-amazon-ecr-private-registries-using-amazon-inspector/) explica la transición desde configuración por repositorio. **No afirmar scanning efectivo/BASIC real sin modo/filtros de cuenta y resultado de imagen**.

Lectura `aws ecr get-registry-scanning-configuration --profile enver-infra --region us-east-1 --output json --no-cli-pager`: **exit254 / AccessDeniedException**, acción **ecr:GetRegistryScanningConfiguration**, recurso **`*`**, fase **preflight read-only**; ninguna identity-based policy permite la acción. No IAM ampliado, simulación ni AdministratorAccess/PowerUserAccess. Pendiente **antes de apply/push**: permiso read si responsable lo aprueba, verificar modo/filtros/costo y resolver incompatibilidades en incremento propio, no cambiar toda la cuenta desde registry.tf.

Un `aws_ecr_lifecycle_policy.api`, regla **untagged / sinceImagePushed / days / 14 / expire**. Elección conservadora: expirar solo imágenes sin etiqueta con push de hace **más de14 días**, no versiones etiquetadas antiguas por conteo ciego que podría borrar rollback. Expiración ECR asíncrona, normalmente dentro de24h tras elegibilidad. **Edad desde push, no desde quitar el tag**: retirar último tag de un digest antiguo puede hacerlo elegible inmediatamente.

Contrato para revisión: conservar etiqueta inmutable en **cada digest activo y al menos el previo compatible para rollback**, junto con manifiestos/índices referenciados; no retirar sus tags/borrarlos mientras sean necesarios. **Todas las etiquetadas se conservan** por esta regla. Limpieza adicional requiere inventario de referencias, autorización y preview de lifecycle antes de cambiarlo. No hay máximo global de imágenes/GB ni tope de costo; se acotan residuos untagged antiguos, **no se garantiza número pequeño total**. Retención final/volumen DP-I-14 pendientes; registry aún no creado no prueba retención real de rollback.

### 15.4. IAM mínimo candidato, separado del publisher

Policy nueva **PROPUESTA LOCAL / NO APLICADA**: **4 Allow / 9 acciones exactas / 1631 caracteres compactos**, JSON/allowlist/scope verificados localmente. Ocho acciones limitadas a `arn:aws:ecr:us-east-1:*:repository/enver-dev-api`, **aws:ResourceAccount=${aws:PrincipalAccount}**, región/username exactos. Create/TagResource exigen cuatro request tags; lifecycle/delete cuatro resource tags. Lecturas sin tags para diagnóstico/refresh, conservando nombre/cuenta/región/usuario.

| Acciones | Necesidad y límites |
| --- | --- |
| ecr:CreateRepository, ecr:TagResource | Create con tags/settings IMMUTABLE/AES256/scanning en una llamada; TagResource es permiso dependiente de create con tags |
| ecr:DescribeRepositories, ecr:ListTagsForResource, ecr:GetLifecyclePolicy | Read/refresh de repository y lifecycle |
| ecr:PutLifecyclePolicy, ecr:DeleteLifecyclePolicy | Crear/retirar policy; provider reemplaza recurso policy al cambiar contenido, no repository |
| ecr:DeleteRepository | Rollback **solo con autorización futura**, no ejecutado/probado |
| ecr:GetRegistryScanningConfiguration | Preflight/costo denegado ahora; statement separado **Resource=* para acción sin ARN**, región/username, sin write global |

Derivación fijada al provider [repository.go](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ecr/repository.go), [lifecycle_policy.go](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ecr/lifecycle_policy.go), [tags_gen.go](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ecr/tags_gen.go), y [IAM ECR](https://docs.aws.amazon.com/AmazonECR/latest/userguide/security_iam_service-with-iam.html). **Sin ecr:***, otras familias de servicios, token/uploads/PutImage/BatchDeleteImage, repository/registry policy, replication o IAM/OIDC. No PutImageScanningConfiguration/PutImageTagMutability/UntagResource: settings iniciales van en CreateRepository; updates no usados en este plan necesitan revisión/autorización adicional.

Allow/condiciones **no son boundary**, auditoría de grants remotos o límite absoluto de settings/lifecycle si hay otros permisos. `force_delete=false` protege el HCL, **no es condición IAM contra borrado forzado por otro cliente**; Sid no impone aprobación humana. Adjuntar concedería capacidad delete, pero no su uso sin permiso. Policies de red intactas; no destroy probado.

**Publisher futuro separado, conceptual/no concedido:** [AWS push IAM](https://docs.aws.amazon.com/AmazonECR/latest/userguide/image-push-iam.html): `ecr:GetAuthorizationToken` Resource=*; en ARN exacto `ecr:BatchCheckLayerAvailability`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`, `ecr:PutImage`, `ecr:BatchGetImage`. Lectura de evidencia futura DescribeImages/DescribeImageScanFindings requerirá autorización propia. Runtime pull separado: token, BatchCheckLayerAvailability/BatchGetImage/GetDownloadUrlForLayer, sin push/provisión. Futuro **GitHub→AWS OIDC**, credenciales temporales/roles separados, sin access keys persistentes de CI; no trust/OIDC/workflow creados. Ninguno de esos permisos publisher/runtime está en el JSON Terraform.

### 15.5. Contrato digest/CI/escritor único — pendiente de aceptación

Referencia desplegable futura **`<repository-uri>@sha256:<digest>`**, **no latest/tag como identidad**. Vincular SHA Git completo, build/run/attempt/arquitectura, digest registrado por ECR y **CI verde del mismo SHA/artefacto**. Tag inmutable único por publicación (SHA+run+attempt), no reutilizar con rebuild ni reconstruir al promover; distinguir índice OCI/manifiesto de plataforma y conservar dependencias. RepoDigest/ID local no sustituye ECR DescribeImages ni elige artefacto productivo.

Propuesta DP-I-14/004: **Terraform administra registry/lifecycle/outputs, no el digest seleccionado runtime; 004 sería su único escritor**. Sin variable de producción ficticia, dos escritores o ignore_changes para ocultar drift. Mecanismo EC2/ownership/contrato integral pendientes de aprobación; sin modificar spec/plan/compute/workflows.

CI futuro **automático**; CD producción **separado/exclusivamente humano**, GitHub Actions → Deploy → **Run workflow** (`workflow_dispatch`) tras CI verde exacto. Push/PR/merge/release/schedules/dispatch automático no despliegan producción; **terraform apply nunca automático**. API/CLI pueden emitir workflow_dispatch: actor/acceso/auditoría deben diseñarse en004, no asumir que el evento prueba intervención humana. Futuro deploy exige mismo digest A/B, dos instancias sanas, smoke tests y evidencia; rollback manual a imagen/config previas compatibles, sin restaurar state de seguridad antiguo/resetear cuotas/authority o retirar claves vigentes. Ninguna capacidad implementada por este contrato.

### 15.6. Validación/plan sin apply y state intacto

`terraform fmt -recursive infra`, `terraform fmt -check -recursive infra`, `terraform -chdir=infra validate`: **exit0**, **Success! The configuration is valid.** Sin init/actualización; fmt no alteró HCL previo salvo outputs añadidos autorizados.

Primer intento de plan **exit1 / Error: Too many command line arguments**, argumentos con `=`/PowerShell antes de plan/refresh; log conservado, no fallo IAM. Segundo intento con argumentos literales entre comillas, mismos inputs/HCL/permisos:

`terraform -chdir=infra plan -input=false -no-color -out=infra4-ecr.tfplan -detailed-exitcode`

**19:27:34.4802448Z–19:27:55.6869202Z**, **exit2 / Plan: 2 to add, 0 to change, 0 to destroy.** Plan guardado ignorado `infra/infra4-ecr.tfplan`, SHA256 **75c8ac132a58658681605ae6dde02386c362ff16c6d89fc12a675d54b0f8d4e1**. `terraform -chdir=infra show -json infra4-ecr.tfplan` exit0, revisión sin imprimir JSON completo. **No autoriza apply**; revalidar vigencia/plan refrescado ante cambios/permisos antes de pedir autorización. Plan de red consumido no reutilizado.

| Address | Acción prevista |
| --- | --- |
| aws_ecr_repository.api | create: enver-dev-api, IMMUTABLE, AES256, scan_on_push=true, force_delete=false, tags aprobados |
| aws_ecr_lifecycle_policy.api | create: única regla untagged >14 días |
| Los14 recursos de red/SG existentes | **no-op**, sin update/replace/delete |

Inputs verificados en JSON: enver/dev/us-east-1, VPC10.20.0.0/16, public_subnet_newbits=8, common_tags={}, network_design_approved=true y account guard STS; subnets existentes preservadas. Tres outputs ECR nuevos, no secretos. **Sin AccessDenied de plan**; repository nuevo fuera de state: éxito **no ejercita permisos de create/delete/scanning ni prueba ausencia de colisión con ECR no gestionado**.

JSON conserva **3 entradas resource_drift de refresh** en route table/ambos SG: vistas calculadas route/ingress/egress antes vacías ahora reflejan ruta/reglas de recursos independientes del apply§14. Contrastadas con standalone route/four rules, coinciden; los14 **resource_changes=no-op**, no mutación propuesta. No afirmar ausencia absoluta de drift. Auxiliar asumió inicialmente un address de ruta incorrecto y se detuvo; corregida solo inspección del mismo JSON por tipo/address reales, sin replan/HCL cambiado.

State persistido **serial15 / 14 instancias gestionadas**, **SHA256 idéntico antes/después**, .backup no acreditado. State/backups/planes/tfvars/.terraform ignorados/no versionados. Hashes de HCL previo (salvo outputs autorizado), policies previas, lock, Dockerfile/.dockerignore/global.json iguales. Sin apply/destroy/import, refresh-only apply, mutaciones AWS/IAM o restauración de state.

### 15.7. Costo consultado, sin asumir Free Tier

Consulta **2026-11-12**, [AWS Price List AmazonECR us-east-1](https://pricing.us-east-1.amazonaws.com/offers/v1.0/aws/AmazonECR/current/us-east-1/index.json), publicationDate **2026-11-11T15:36:26Z**, effectiveDate **2026-11-01T00:00:00Z**, SKU **KJC988R4J65BDMTQ**, TimedStorage-ByteHrs/S3: **USD0.10/GB-mes**, almacenamiento privado estándar. No archivo/replicación/pull-through cache configurados.

[ECR pricing oficial](https://aws.amazon.com/ecr/pricing/): entrada y transferencia a servicios AWS **en la misma región USD0/GB**; Internet/inter-región puede facturarse, con tarifas agregadas por volumen/ruta y cargos del otro extremo. **Tarifa neta exacta de salida/volumen/costo integral pendientes**; no asumir Free Tier/franquicias/créditos. Sin NAT/endpoints nuevos; si se añaden después, sus cargos se suman.

[AWS scanning](https://aws.amazon.com/blogs/containers/container-scanning-updates-in-amazon-ecr-private-registries-using-amazon-inspector/) describe BASIC **sin cargo adicional**, ENHANCED/Inspector **facturable**; fuente no acredita modo/factura actual de esta cuenta: lectura denegada, costo efectivo pendiente. No usar tarifa histórica Inspector como vigente. AES256 evita custom KMS, no implica ECR total gratis.

Estimación futura **0.10×GB-mes almacenados + transferencia facturable + scanning si aplica**; no inferir consumo/compresión/deduplicación o factura desde tamaño Docker local. Lifecycle no es cap; etiquetadas pueden crecer. Presupuesto agregado baseline USD50/reserva≥USD50 de crédito reportado USD100 intacto, no saldo validado/presupuesto mensual/tope duro; T001 costo/duración/ownership abiertos. Sin apply/push no recursos ECR ni bytes subidos por este bloque; red previa permanece.

### 15.8. Estados/gates y detención

- **T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: HCL/outputs/plan, no registry creado; IAM solo propuesto, scanning efectivo/retención/costo/contrato/dependencias y aprobación pendientes.
- **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: build/non-root/configuración y contrato propuesto; bases reproducibles/seguridad, ECR/digest/CI/ownership aprobado/rollback pendientes. Ausencia de push no elimina criterio contractual propio ni acredita su aprobación.
- **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: suma extensión fmt/check/validate/plan; custodia/cifrado/backup/recovery, E025 integral y001/002 sin aceptar.
- **T-INFRA-003/004/005/006 conservan PARCIAL**; no aceptar EIP/Atlas/egreso/SSM/runtime IAM/TLS por plan ECR. **GATE-I-A / GATE-I-B PENDIENTE / NO SATISFECHO**.
- Gate relacionado registry/container **GATE-I-C PENDIENTE / NO SATISFECHO**: faltan009/010/027/028, dos EC2/AZ/mismo digest/capacidad/recuperación y prerrequisitos A/B. **GATE-I-H / GATE-I-I** pendientes por state/reproducción/rollback/004/aceptación cloud. **Nueve gates pendientes**.
- Nueve tareas parciales001–008/025; otras **21 PENDIENTE / NO EJECUTADA**. Sin ejecutar compute/026–028, alterar Authentication o cerrar002 GATE-G/H/AUTH-027 integral.

**DEFINIDO/PLANIFICADO:** un repository/una lifecycle policy, tres outputs y candidato IAM separado, build local válido. **CREADO EN AWS EN ESTE BLOQUE: NINGUNO. APPLY/PUSH: NO EJECUTADOS.** Cinco archivos versionables autorizados, sin stage/commit/push. Detener para revisión humana HCL/IAM/plan/costo y nueva autorización delimitada, sin cleanup/permisos/siguiente incremento automático.

## 16. INFRA-4 ECR — revisión pre-apply IAM y cuotas, sin AWS ni checkpoint Git

### 16.1. Alcance y revisión local — 2026-11-12

Autorización **PRE-APPLY IAM + CHECKPOINT / VALIDACIÓN CONTROLADA / NO APPLY**: inspeccionar policy existente, acciones/scope/tamaño, preparar evidencia para aplicación manual posterior, ejecutar solo fmt-check/validate y actualizar task/memoria si hay evidencia nueva. El título no autoriza stage/commit/push: se prohíben expresamente. Baseline confirmada **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, cinco archivos pendientes de §15 y **0 staged**. Sin cambios de HCL/JSON/lock, plan repetido, AWS CLI/STS/IAM/Access Analyzer/simulación, Docker/producto o instalación/init/actualización.

JSON [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json) **válido por parseo y revisión local**, 4 Allow y nueve acciones exactas, sin duplicados ni comodines de acción:

`ecr:CreateRepository`, `ecr:TagResource`, `ecr:DescribeRepositories`, `ecr:ListTagsForResource`, `ecr:GetLifecyclePolicy`, `ecr:PutLifecyclePolicy`, `ecr:DeleteLifecyclePolicy`, `ecr:DeleteRepository`, `ecr:GetRegistryScanningConfiguration`.

Ausencia confirmada de **ecr:***, AdministratorAccess/PowerUserAccess y cualquier acción de **EC2, ELB, IAM, S3, KMS, DynamoDB, ElastiCache, Secrets Manager, ACM, Route53, CloudWatch o SSM**; también sin permisos publisher. Settings iniciales se transmiten con CreateRepository, no requieren ampliar updates de scanning/mutabilidad para este plan; lecturas/tags/lifecycle/delete corresponden al flujo provider6.73.0 revisado en §15.4. Delete apoya únicamente rollback posterior autorizado, no ejecución actual ni mínimo efectivo global auditado.

Contrastado con [referencia oficial ECR vigente](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ecr.html), consultada 2026-11-12: **ocho acciones admiten resource repository**, y permanecen restringidas a **enver-dev-api/us-east-1**, ARN `arn:aws:ecr:us-east-1:*:repository/enver-dev-api`. El wildcard de cuenta se acota con **aws:ResourceAccount=${aws:PrincipalAccount}**, región/username; create/tag usa request tags, lifecycle/delete resource tags, sin exigir tags aún inexistentes al crear.

**Único Resource="*": ecr:GetRegistryScanningConfiguration**, operación de registry sin resource type ARN en la referencia AWS. No se puede sustituir por ARN repository ni exigir tags de ese repository sin romper esa lectura. Statement separado, región us-east-1/username enver-terraform-local, solo read; no PutRegistryScanningConfiguration, Inspector o administración global concedidos. Permanece por el **AccessDenied observado en §15.3** para inspección de modo/filtros/costo; no porque Terraform repository lo invoque al planificar un create. No se repitió la llamada: acceso efectivo/scanning siguen pendientes.

### 16.2. Tamaño y propuesta de aplicación manual, no ejecutada

Archivo existente **2269 bytes / 2269 caracteres** (ASCII); documento compacto **1631 caracteres sin whitespace**, confirmado tanto por serialización JSON como por conteo sin espacios. SHA256 exacto del archivo **393b731f012d6be6bc285b068793f52dda91099661717f2f3b991c437955cbe9**. No se reformatea ni modifica policy.

[Cuotas oficiales IAM](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_iam-quotas.html), consultadas 2026-11-12: inline de **usuario: 2048 caracteres agregados entre todas sus inline policies**, no por archivo; customer-managed: **6144 caracteres por policy**, ambos sin contar whitespace. **Cabe aisladamente inline**, pero deja solo417 caracteres para las restantes inline del usuario; su suma remota/cuotas de adjuntos no se consultaron. No afirmar que puede añadirse inline al usuario actual por medir solo este JSON.

**Recomendación pendiente de aprobación: customer-managed policy independiente ENVER-Terraform-ECR**. No exigida por tamaño individual; recomendada por separación clara respecto de networking y para no consumir el agregado inline compartido. Preparación para responsable autorizado: revisar el JSON exacto/hash/nueve acciones/cuatro statements; si aprueba posteriormente, crear la customer-managed con ese nombre y adjuntarla **solo al usuario previsto enver-terraform-local**, comprobando cuenta y cuota disponible. No integrar con networking ni conceder al usuario Terraform permisos IAM para crear/adjuntar policies. Estos son pasos propuestos, **ninguno ejecutado ni acreditado**; no se crea ARN/version remota ficticio.

Al adjuntarse, la policy sí otorgaría capacidad delete; Sid/aprobación humana no son guard IAM y force_delete=false solo protege el flujo HCL. No permissions boundary ni auditoría de otras policies/boundaries/SCP. La validación local **no es IAM Access Analyzer ni prueba efectiva de create/delete/rollback**. Después de cualquier aplicación manual reportada harán falta autorización de las lecturas pertinentes, verificación de identidad/modo/filtros/costo y revisión de vigencia del plan antes de solicitar apply; nada de ello se ejecuta aquí.

### 16.3. Validación local, baseline conservada y estados

Terraform **1.17.1/windows_amd64** / provider **6.73.0** comprobados con `terraform -chdir=infra version -json`. Un auxiliar se detuvo inicialmente al buscar selección provider desde la raíz sin configuración Terraform (`provider_selections={}`); comprobación corregida a infra, sin cambiar versión/archivos o ejecutar plan. **19:43:50.0174285Z–19:44:01.7004193Z**:

- `terraform fmt -check -recursive infra`: **exit0**, sin modificaciones.
- `terraform -chdir=infra validate -no-color`: **exit0 / Success! The configuration is valid.**
- **Plan no repetido** porque HCL no cambió. Se conserva como **evidencia previa**, no refresh actual, el plan §15: **2 add / 0 change / 0 destroy**, únicamente aws_ecr_repository.api/aws_ecr_lifecycle_policy.api, networking14 no-op; SHA256 **75c8ac132a58658681605ae6dde02386c362ff16c6d89fc12a675d54b0f8d4e1** comprobado idéntico. No se aplica ni se presume vigencia indefinida.

Evidencia local nueva: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-preapply-review-20261112-194320\`, resúmenes de parseo/scope/tamaño/hash y validación, hashes de fuentes y state/plan. No contiene una aprobación IAM ni reemplaza custodia/backup/recovery. Hashes previos preservan HCL/policy/lock/state/plan y trabajo existente; state/planes/.terraform/tfvars siguen fuera de Git. Únicas ediciones **de este turno**: task/memoria, sobre los cinco cambios pendientes previos.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. Sin aceptación de dependencias, registry/imagen ECR, scanning efectivo, IAM aplicado, state custody/backup/recovery o contrato004 integral. **T-INFRA-003–006 siguen PARCIAL; los nueve gates PENDIENTE / NO SATISFECHO**, incluidos I-A/I-B/I-C/I-H/I-I. Authentication y estados002 sin cambios.

**AWS MODIFICADO: NO. APPLY/DESTROY/IMPORT/REPLAN: NO. POLICY REMOTA CREADA/ADJUNTADA: NO. STAGE/COMMIT/PUSH: NO.** Detener para revisión humana; no ejecución manual implícita, siguiente incremento o checkpoint Git.

## 17. INFRA-4 ECR — IAM informado aplicado, lecturas y replan pre-apply

### 17.1. Autorización, fuentes y baseline — 2026-11-12

Solicitud expresa **IAM VALIDATION + REPLAN PRE-APPLY / VALIDACIÓN CONTROLADA / NO APPLY**: STS, GetRegistryScanningConfiguration, DescribeRepositories, revisión local de policy, fmt-check/validate y un replan con inputs aprobados; solo task/memoria editados. **Terraform apply: NO AUTORIZADO TODAVÍA**. Sin mutaciones IAM/AWS, repositorio manual, apply/destroy/import, Docker/producto o stage/commit/push. Baseline confirmada **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, cinco pendientes previos de INFRA-4 y **0 staged**.

**INFORMADO POR EL RESPONSABLE:** creó y adjuntó manualmente **ENVER-Terraform-ECR**, **customer-managed**, fuente [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json), sin AdministratorAccess/PowerUserAccess/ecr:*. Se registra **APLICADA MANUALMENTE SEGÚN RESPONSABLE**, no una mutación del agente. No se enumeran policy remota/versiones/adjuntos/boundaries/SCP; las llamadas verifican únicamente sus respuestas efectivas, no identidad exacta de la policy causante ni ausencia global de otros grants. §§15/16 conservan sus estados históricos de propuesta/no aplicada en aquellos turnos.

JSON local intacto, hash **393b731f012d6be6bc285b068793f52dda91099661717f2f3b991c437955cbe9**, 4 Allow/9 acciones aprobadas/1631 caracteres compactos: CreateRepository, TagResource, DescribeRepositories, ListTagsForResource, GetLifecyclePolicy, PutLifecyclePolicy, DeleteLifecyclePolicy, DeleteRepository y GetRegistryScanningConfiguration, todas **ecr:**. Sin wildcard de acción ni familias prohibidas/publisher. Scope por repository enver-dev-api/us-east-1/misma cuenta/username/tags y único Resource=* para lectura scanning conservados; no ampliación local/remota.

### 17.2. Lecturas efectivas y AccessDenied delimitado

Perfil **enver-infra**, región **us-east-1**, sin pager; salidas capturadas sin credenciales/ID de cuenta completo en documentación. Secuencia terminada **2026-11-12T19:50:50.6800817Z**:

| Llamada AWS CLI | Resultado real / alcance |
| --- | --- |
| sts get-caller-identity --profile enver-infra --region us-east-1 | **exit 0**, IAM user **enver-terraform-local**, cuenta coincide con owner VPC del state aprobado |
| ecr get-registry-scanning-configuration --region us-east-1 --profile enver-infra | **exit 0**, **scanType=BASIC / rules=[]**; lectura antes denegada ahora disponible |
| ecr describe-repositories --region us-east-1 --profile enver-infra | **exit 254 / AccessDeniedException**; listado general, no lista vacía exit 0 |
| Misma lectura con --repository-names enver-dev-api | Diagnóstico acotado, **exit 254 / RepositoryNotFoundException**, sin AccessDenied: repository indicado no existe; no lectura exit 0 de un repository existente |

**AccessDenied observado:** acción **ecr:DescribeRepositories**, recurso **arn:aws:ecr:us-east-1:<account>:repository/*** (listado general), fase **AWS CLI/read ECR previa al plan**; ninguna identity-based policy permite ese alcance según respuesta. **Operación Terraform asociada: ninguna**, no es un fallo del replan/provider. La policy permite un nombre exacto, no el inventario de toda la cuenta; NotFound de la consulta acotada no se oculta ni convierte en exit 0. Sin IAM modificado/reintento con privilegios superiores.

**Permiso mínimo si se exigiera ese listado general:** Allow únicamente **ecr:DescribeRepositories** sobre **arn:aws:ecr:us-east-1:<cuenta-aprobada>:repository/***, con región/username/misma cuenta; ampliaría recursos visibles, no acciones ni escritura. **Solo propuesta explicativa, NO aplicada ni necesaria para este plan**. Preferir consulta por nombre, alineada con DescribeRepositories(RepositoryNames=[nombre]) del provider revisado; no ecr:*, AdministratorAccess/PowerUserAccess o falsa condición por tags para una lectura sin repository concreto. No ampliar IAM solo para obtener inventario general innecesario.

**Efectividad acreditada: parcial**, GetRegistryScanningConfiguration exit 0 y respuesta NotFound por nombre sin AccessDenied; **no create/tag/lifecycle/delete efectivos probados**. Plan de repository nuevo tampoco los ejercita. BASIC/rules=[] acredita modo global en ese instante; `scan_on_push=true` sigue siendo solicitud HCL/plan, no prueba de scanning efectivo o findings de imagen inexistente. No se modifican filtros globales/Inspector ni se interpreta ausencia de reglas como imagen ya escaneada.

### 17.3. Terraform y plan exacto conservando networking

Terraform **1.17.1/windows_amd64**, provider **6.73.0**, comprobados desde infra, lock sin cambios. `terraform fmt -check -recursive infra` **exit 0**, `terraform -chdir=infra validate -no-color` **exit 0 / Success! The configuration is valid.** Sin init/instalaciones/actualizaciones.

Una ejecución de `terraform -chdir=infra plan -input=false -no-color -out=infra4-ecr-replan-20261112-195037.tfplan -detailed-exitcode`, **19:51:57.0468069Z–19:52:18.1128719Z**, **exit 2 / Plan: 2 to add, 0 to change, 0 to destroy.** **Sin AccessDenied en Terraform**. Show-json **exit 0** revisado selectivamente, sin publicar JSON/state completo.

| Address / inventario | Acción |
| --- | --- |
| aws_ecr_repository.api | **create**, nombre **enver-dev-api**, **IMMUTABLE**, **scan_on_push=true**, **AES256**, **force_delete=false**, tags aprobados |
| aws_ecr_lifecycle_policy.api | **create**, única regla **untagged/sinceImagePushed/days/14/expire** |
| Los14 recursos de networking/SG del state | **no-op**, 0 update/replace/delete |

Inputs contrastados en JSON: project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8, common_tags={}, network_design_approved=true, account guard igual a cuenta STS/state; subnets existentes preservadas. Tres outputs ECR no sensibles. No recursos fuera de los dos ECR, destrucciones ni replacements. Lifecycle conserva todas las imágenes etiquetadas, incluidas activas/rollback bajo contrato§15; 14 días desde push, no desde retirar tag, sin tope global de almacenamiento.

El nuevo JSON conserva las **tres entradas calculadas de refresh** de route table/SG (route/ingress/egress antes vacíos). Se contrastaron con standalone route/cuatro reglas y coinciden; **14 resource_changes=no-op**. No confundirlas con cambios de red propuestos ni afirmar cero drift absoluto.

Plan nuevo ignorado **infra/infra4-ecr-replan-20261112-195037.tfplan**, SHA256 **669e2550dced81aa3bd1bf3c07aff210305057e61c17d2cb503abe55c72a1640**; plan previo§15/hash intacto, no sobrescrito/reaplicado. State persistido **serial15/14 instancias gestionadas/hash idéntico**, sin backup/recovery nuevo acreditado; HCL/policy/lock y demás fuentes no modificados por lecturas/validación/plan. Plan no autoriza apply y requiere revisión de vigencia antes de una autorización posterior.

### 17.4. Costo, estados y detención

Se conserva evidencia regional previa§15.7 de almacenamiento ECR **USD0.10/GB-mes**, **sin nueva consulta tarifaria/Billing**. Sin Free Tier asumido ni total calculado a partir del tamaño Docker local. La lectura confirma modo BASIC actual, no ejecución/resultados de scan ni factura/ausencia de otros cargos Inspector globales; costo integral/transferencia/volumen/duración y presupuesto/reserva siguen su aceptación pendiente. Ningún repository/imagen/byte ECR creado/subido por este bloque.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: aplicación IAM informada y lecturas/replan acreditados, no repository real, escritura/scan de imagen/retención/contrato/dependencias aceptados. **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: sin nueva imagen/digest ECR/CI/ownership aprobado ni rollback. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: nueva validación/plan no suple custodia/cifrado/backup/recovery/001/002/E025 integral.

**GATE-I-A PENDIENTE / NO SATISFECHO** según §3/E001/E002/E025: faltan aceptación de baseline/costo/state/ownership y protección/recuperación. **GATE-I-B PENDIENTE / NO SATISFECHO**: I-A, IAM runtime/SSM, Atlas/egreso/TLS, E019–020/E027–028 integrales sin resolver. 003–006 siguen parciales; I-C/H/I y **los nueve gates pendientes**. Authentication/002/004 intactos; no ejecutar integralmente026–028 ni cerrar AUTH-027/002 GATE-G/H.

Evidencia local privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-iam-replan-20261112-195037\`: lecturas saneadas, logs/JSON de plan y resúmenes/hashes; no publicar JSON completo ni confundir ubicación temporal con custodia/backup. Solo task/memoria editados en este turno; conjunto pendiente conserva cinco archivos/0staged, artefactos Terraform sensibles ignorados/no versionados.

**RECURSOS AWS CREADOS POR EL AGENTE: NINGUNO. IAM REMOTO: APLICACIÓN MANUAL INFORMADA POR RESPONSABLE, NO MODIFICADO POR EL AGENTE. TERRAFORM APPLY: NO AUTORIZADO TODAVÍA / NO EJECUTADO. SIN DESTROY/IMPORT/STAGE/COMMIT/PUSH.** Detener para revisión humana, sin siguiente incremento automático.
