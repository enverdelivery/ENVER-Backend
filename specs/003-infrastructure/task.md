# Tareas 003 — Infraestructura de ENVER Backend

## 1. Control, autorización y estado inicial

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.4.0 / 2026-11-12 |
| Solicitud inicial / incremento actual | Materialización; baseline local (§6); validación AWS (§7); aprobación de presupuesto y preparación documental INFRA-2/3 (§8) |
| Fuentes | [spec.md](spec.md) y [plan.md](plan.md), propuesta aprobada como base documental |
| Catálogo | 30 tareas, T-INFRA-001 a T-INFRA-030 |
| Estado vigente de tareas | T-INFRA-001 PARCIAL / BLOQUEADA POR DECISIONES; T-INFRA-002 PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE; 003–030 PENDIENTE / NO EJECUTADA |
| Estado de todos los gates | **PENDIENTE / NO SATISFECHO** |
| Evidencia de ejecución de 003 | E(T-INFRA-001) parcial y E(T-INFRA-002) local en §§6–8; identidad STS acreditada y presupuesto aprobado; sin infraestructura cloud desplegada |
| Baseline documental original | `feature/authentication`, `c58ea86e3eddff4befa8f61398dd58d55c4ef985` |
| Baseline del BUILD actual | `feature/infrastructure`, creada desde `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`, árbol inicialmente limpio |

Crear este catálogo **no autoriza ejecutar tareas**, cambiar código/configuración/Docker/tests, crear `infra/`, recursos AWS/Atlas, workflows o secretos, ni correr Terraform. No stage/commit/push. Los únicos archivos de esta entrega son los tres Markdown de 003. La autorización previa de la propuesta es documental, no habilita cargos ni credenciales.

El párrafo anterior conserva el alcance de la materialización inicial. La solicitud posterior explícita **BUILD T-INFRA-001 + T-INFRA-002 — BASELINE TERRAFORM** autoriza solo preparar estos dos incrementos, crear la baseline `infra/` y su lock, comprobar herramientas/credenciales sin revelar secretos, ejecutar version/fmt/init -backend=false/validate y registrar evidencia en este archivo y memoria.md. `.gitignore` solo si faltan reglas; ya existen. Autoriza crear rama, **no stage/commit/push, plan/apply, recursos AWS ni tareas 003–030**. No cambia los criterios originales de aceptación ni cierra decisiones pendientes por inferencia.

La solicitud posterior **CIERRE BASELINE AWS + TERRAFORM** autoriza validar perfil/identidad/región e inspeccionar permisos sin modificarlos, repetir version/fmt-check/validate y actualizar exclusivamente task/memoria (§7). No autoriza cerrar sin criterios satisfechos, modificar `infra/`, registrar credenciales, crear recursos/políticas, ejecutar plan/apply ni stage/commit/push. §6 conserva los resultados históricos; §7 es el estado de identidad/región vigente.

**CIERRE DE PRESUPUESTO + PREPARACIÓN INFRA-2/3** aprueba el límite operativo inicial de USD 50 con reserva objetivo mínima de USD 50 sobre USD 100 de crédito informado por el responsable (§8). Autoriza únicamente registro en task/memoria, reevaluación de criterios, recomendación documental y version/fmt-check/validate/Git de lectura. No autoriza ejecutar 003–006/025, modificar infraestructura/IAM, plan/apply, stage/commit/push o consumo AWS. §§6–7 permanecen como antecedentes; el presupuesto pendiente allí ya no es el estado vigente.

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

Notación abreviada: E(001) = E(T-INFRA-001); E(003–006) exige cada una de las cuatro evidencias. La materialización inicial no registró ejecución; §§6–8 incorporan evidencia parcial de 001 y validación local de 002, sin satisfacer gates. I-H/I-I no implican SLA, producción lista ni cierre automático de AUTH-027 o gates de 002.

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
- **Resultado actual:** identidad/perfil y región de baseline acreditados (§7); crédito informado USD 100, presupuesto operativo inicial USD 50 y reserva objetivo mínima USD 50, APROBADO PARA BASELINE (§8). El monto ya no está pendiente. Faltan estimación regional fechada por duración/uso, elegibilidad/vigencia del crédito para los servicios, aprobación de responsables/ownership, duración y definiciones de ambiente/AZ/naming-tags aplicables a DP-I-01/13. No se satisface todavía la aceptación completa.
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
- **Resultado actual:** §§6.3/7/8 acreditan baseline Terraform 1.17.1, provider/lock 6.73.0, estrategia local y fmt/validate correctos. Versiones reconocidas por el responsable como baseline técnico, sin actualización. T-INFRA-001 continúa sin aceptación; presupuesto aprobado no resuelve esa dependencia ni aporta evidencia de custodia/backup/recuperación del state y aceptación integral aplicable a DP-I-12/13. No se marca COMPLETADA.
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

Cobertura documental: 40 requisitos INFRA, 20 decisiones propuestas PLD-INFRA, 15 criterios AC-I, 15 fases, 30 tareas y 9 gates. **Cobertura no equivale a implementación, prueba o satisfacción.** Estado tras §8: 001 parcial/bloqueada por aprobaciones/evidencia restantes (no por monto presupuestario), 002 baseline técnica validada con aceptación pendiente, otras 28 tareas PENDIENTE / NO EJECUTADA; todos los gates PENDIENTE / NO SATISFECHO. Identidad STS y presupuesto de baseline acreditados, sin infraestructura cloud desplegada.

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
