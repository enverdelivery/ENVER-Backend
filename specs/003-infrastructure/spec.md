# Especificación 003 — Infraestructura de ENVER Backend

## 1. Identificación, autoridad y autorización

| Campo | Valor |
| --- | --- |
| Versión documental | 0.1.0 |
| Fecha | 2026-11-12 |
| Solicitud | SDD — MATERIALIZAR 003-INFRASTRUCTURE; BUILD DOCUMENTAL ÚNICAMENTE |
| Estado documental | Propuesta previamente aprobada, materializada para revisión de fidelidad; no autoriza ejecución técnica |
| Estado técnico de 003 | NO IMPLEMENTADA / SIN EVIDENCIA CLOUD REGISTRADA |
| Plataforma | ENVER Backend; ASP.NET Core / .NET 10; AWS y Terraform |
| Baseline de lectura | `feature/authentication`, commit `c58ea86e3eddff4befa8f61398dd58d55c4ef985` |
| Documentos de 003 | Este spec, [plan.md](plan.md) y [task.md](task.md) |

Autoridad: [constitución canónica](../../.specify/memory/constitution.md), [referencia raíz](../../constitution.md), [AGENTS](../../AGENTS.md), [memoria](../../memoria.md) y [README](../../README.md). La solicitud posterior explícita autoriza crear **únicamente los tres Markdown de 003**; no reforma la constitución, no reescribe los estados históricos de 001/002 ni sincroniza otros documentos en esta entrega. La aprobación de la propuesta habilita su materialización documental, no `apply`, pruebas, despliegues ni cargos.

Clasificación utilizada:

- **REQUISITO:** resultado o restricción exigida para la futura aceptación; no significa satisfecho.
- **DECISIÓN PROPUESTA:** solución de la propuesta aprobada como base documental; su ejecución requiere autorización y resolución de sus parámetros pendientes.
- **DECISIÓN PENDIENTE:** selección o dato todavía no aprobado. No se completa por iniciativa del implementador.
- **ESTADO OBSERVADO:** hecho leído en la baseline o evidencia histórica referenciada; no verificación nueva.

## 2. Objetivo, alcance y límites

Resultado futuro: una **URL HTTPS pública** de ENVER que sirva inicialmente:

- `POST /api/v1/auth/register`.
- `POST /api/v1/auth/login`.
- `POST /api/v1/auth/logout`.

El tráfico deberá distribuirse entre **mínimo dos instancias reales del backend**, detrás de un Load Balancer, con estado de seguridad compartido y sin cambiar los contratos de Authentication. Esta URL **no se declara existente**.

Incluye diseño de cómputo, contenedores, red, TLS/DNS, Atlas, Redis, autoridad DynamoDB, IAM, secretos, observabilidad, salud, IaC/state, operación, rollback y evidencia académica. Define interfaces para 004, no su pipeline.

Fuera de alcance de esta entrega: implementación de infraestructura o aplicación; Orders; creación de `infra/`; recursos AWS/Atlas/DNS; imágenes o cambios Docker; workflows; cambios en `Program.cs`, Authentication, tests, configuración o dependencias; ejecuciones Terraform, AWS CLI, builds o pruebas. No stage, commit ni push. Cada tarea futura requerirá autorización delimitada por archivos, comandos, entorno, costo y objetivos.

## 3. Baseline y coherencia con 001/002

Fuentes: [001 spec](../001-backend-foundation/spec.md), [001 plan](../001-backend-foundation/plan.md), [001 task](../001-backend-foundation/task.md), [002 spec](../002-authentication/spec.md), [002 plan](../002-authentication/plan.md) y [002 task](../002-authentication/task.md).

**ESTADO OBSERVADO:** Authentication terminó en alcance **LOCAL/TEST REPRESENTATIVO**; E058 y GATE-E se registran en 002 task §§82–84 y memoria §55. Se conservan UT 684/684, IT 310/310 y ST 223/223 como evidencia histórica, sin repetirlas ni convertirlas en pruebas cloud. GATE-G/H y diferidos de 002 no se cierran aquí; AUTH-027 permanece **NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**. Foundation no se declara completada integralmente.

El wiring de [AuthenticationServiceCollectionExtensions](../../Enver.API/Composition/AuthenticationServiceCollectionExtensions.cs) ya usa [DynamoDbAuthenticationSecurityAuthority](../../Enver.API/Infrastructure/Aws/DynamoDbAuthenticationSecurityAuthority.cs). Su existencia no acredita una tabla AWS provisionada ni bootstrap productivo. [Program.cs](../../Enver.API/Program.cs) expone `/health` como liveness; readiness sigue pendiente. El [Dockerfile](../../Enver.API/Dockerfile) ya existe: no se crea ni modifica ni se da por certificado para producción.

Las selecciones posteriores AWS/Atlas/Redis/Terraform desarrollan los pendientes históricos de 001 sin modificarlo ni aprobar retrospectivamente sus requisitos. 003 conserva los contratos HTTP, JWT, validación, cuotas, revocación, errores, `no-store`, presupuestos y separación lógica de 002. No introduce otro emisor, ledger MongoDB de revocaciones ni transacciones distribuidas.

## 4. Requisitos generales y restricciones académicas/costo

Todos los `INFRA-nnn` son **REQUISITOS futuros**; ninguno está acreditado por esta entrega.

| ID | Requisito verificable |
| --- | --- |
| INFRA-001 | Publicar en AWS el mismo backend ASP.NET Core / .NET 10 con los tres endpoints de §2, sin modificar su semántica. |
| INFRA-002 | Demostrar backend cloud, mínimo dos instancias, balanceo, HTTPS, NoSQL, seguridad, observabilidad, Terraform/IaC reproducible y evidencia verificable. Dos contenedores en una sola EC2 no satisfacen la propuesta. |
| INFRA-003 | Aprobar presupuesto y estimación regional antes de recursos con cargo; identificar costos recurrentes, variables y de recursos detenidos. No afirmar gratuidad ni elegibilidad Free Tier. |
| INFRA-038 | Exigir autorización técnica por incremento, evidencia trazable y gates pendientes hasta verificación; materializar documentos no ejecuta tareas. |

**Restricción académica:** elegir el mínimo diseño suficiente, no microservicios, Kubernetes, multi-región ni componentes especulativos. La rúbrica requiere evidencia del resultado, no solamente diagramas o una salida de Terraform.

## 5. Arquitectura objetivo y cómputo

**DECISIÓN PROPUESTA — base documental aprobada:**

```text
Internet
  │ HTTPS
  ▼
Application Load Balancer + certificado ACM
  ├── EC2 Linux + Docker — instancia A / AZ A
  └── EC2 Linux + Docker — instancia B / AZ B
          ├── MongoDB Atlas: datos de usuarios
          ├── Redis compartido: cuotas, revocación y estado de seguridad
          └── DynamoDB authority: autoridad operacional compartida
```

ECR alojaría las imágenes; CloudWatch recibiría telemetría; IAM/SSM permitirían administración sin SSH público. Los componentes son **objetivo futuro**, no inventario de recursos existentes.

| Alternativa evaluada | Ventajas | Costos y limitaciones | Conclusión documental |
| --- | --- | --- | --- |
| EC2 Linux + Docker | Dos máquinas demostrables, Terraform directo, control explícito, salida estable para Atlas, rúbrica sencilla de evidenciar | Parches, operación, recuperación y capacidad del host a cargo del proyecto | Recomendada inicialmente; no se afirma ser siempre la más barata |
| ECS/Fargate | Contenedores administrados y menos operación del host | Costos de tareas/red/salida; confirmar interpretación docente de dos tasks como instancias | Alternativa, no seleccionada para este baseline |
| ECS sobre EC2 | Orquestación conservando hosts demostrables | Más piezas operativas para este alcance | No recomendada inicialmente por simplicidad académica |

| ID | Requisito verificable |
| --- | --- |
| INFRA-004 | Desplegar el artefacto backend como contenedor Docker en al menos dos EC2 Linux, en dos AZ; validar capacidad real para .NET 10 y hashing sin bajar controles para ahorrar recursos. |
| INFRA-005 | Utilizar imagen reproducible y rastreable por commit y digest inmutable, mismo digest en ambas instancias, runtime non-root y sin secretos en capas; evaluar el Dockerfile existente antes de su uso futuro. |
| INFRA-006 | Separar configuración por ambiente y obtener secretos fuera de la imagen; inicio seguro ante configuración inválida y sin volcar Options/entorno. |
| INFRA-039 | Documentar recuperación de una instancia, mantenimiento, escalado y límites; mantener mínimo dos instancias en aceptación y evitar afinidad de sesión necesaria para seguridad. Dos AZ de cómputo no acreditan HA de todas las dependencias ni SLA. |

**DECISIONES PENDIENTES:** región/AZ, arquitectura CPU, AMI Linux, versiones aprobadas, tamaño EC2/EBS, digest, política de parches/reemplazo y escalado. No asignar tamaños por intuición. Escalado inicial manual propuesto, sujeto a autorización y compatibilidad de IP de salida con Atlas; autoscaling posterior no implícito.

## 6. Networking y seguridad de red

| ID | Requisito verificable |
| --- | --- |
| INFRA-007 | Definir VPC propia, CIDR sin solapamiento, subnets en dos AZ, route tables e Internet Gateway, con separación de exposición pública/privada. |
| INFRA-008 | ALB público; puerto backend accesible solo desde SG del ALB; sin SSH ni puerto Docker administrativo públicos. SSM mediante rol de instancia y conectividad aprobada. Redis privado. Egreso acotado y justificado. |
| INFRA-009 | Resolver salida estable, DNS y conectividad TLS hacia Atlas/servicios AWS; Atlas restringido a salidas explícitamente aprobadas, sin `0.0.0.0/0` como atajo. Demostrar que una llamada directa a EC2 no elude ALB ni cuotas. |

**DECISIÓN PROPUESTA:** perfil académico económico con ALB en subnets públicas y EC2 con salida IPv4 estable; los hosts pueden estar en subnets públicas, pero SG impide todo ingreso de aplicación salvo desde ALB. IPv4/EIP tiene costo posible. Redis permanece en subnets privadas, sin dirección pública. Administración exclusivamente SSM con IAM temporal.

**Alternativa más aislada:** EC2 privadas con salida NAT o mecanismo equivalente aprobado y direcciones autorizadas en Atlas; considerar costo NAT, transferencia inter-AZ y endpoints privados. SSM, ECR, logs, secretos y DynamoDB también necesitan salida o endpoints. Un Internet Gateway no da salida directa a una instancia privada sin el mecanismo adecuado.

**DECISIONES PENDIENTES:** perfil de red definitivo, IP de salida por AZ, estrategia DNS, endpoints privados frente a salida pública TLS y reglas de egreso. No sobrearquitecturar ni afirmar que "subnet privada" equivale a seguridad sin revisar rutas y SG.

## 7. Load Balancer y comportamiento de fallos

| ID | Requisito verificable |
| --- | --- |
| INFRA-010 | ALB con target group que distribuya solicitudes entre mínimo dos instancias, por defecto sin stickiness; puerto/protocolo/registro explícitos y drenaje para cambios controlados. |
| INFRA-011 | Health checks con ruta, matcher, intervalos, umbrales y timeouts aprobados; demostrar retirada de target no saludable y recuperación. Documentar el caso de todos unhealthy y la respuesta segura de la aplicación. |

**DECISIÓN PROPUESTA:** target group de tipo `instance`, puerto del contenedor publicado solo en red autorizada; listener 443 a targets y 80 solo redirección. Puerto interno, TLS interno y parámetros quedan pendientes.

ALB puede **fail-open hacia targets unhealthy si todos están unhealthy**. No garantizar bloqueo global por health check. El fail-closed de Authentication/autoridad compartida debe mantenerse aun si llega tráfico. Readiness no sustituye fencing ni continuidad Redis. Distinguir respuestas de borde (por ejemplo falta de targets) de Problem Details emitido por API; no reintentar automáticamente POST ni convertir errores en éxito.

## 8. HTTPS, ACM y DNS

| ID | Requisito verificable |
| --- | --- |
| INFRA-012 | Certificado ACM válido en región del ALB y hostname controlado; listener 443, política TLS aprobada y 80 solo redirección a HTTPS. Verificar cadena, nombre, expiración y DNS; no certificado autofirmado para la URL pública final. |
| INFRA-013 | Resolver explícitamente cifrado ALB→backend y confianza de proxies/origen/esquema antes de aceptación. Aceptar forwarded headers solo de proxies autorizados; evitar spoofing de IP, bucles HTTPS y debilitamiento de cuotas. |

**DECISIÓN PROPUESTA:** terminar TLS público en ALB con ACM, validar propiedad por DNS y publicar dominio/subdominio del responsable. No hay dominio ni zona DNS declarados existentes; Route 53 es opción, no obligación frente a otro DNS autorizado. El DNS asignado al ALB no equivale a tener un dominio propio con certificado válido.

**DECISIONES PENDIENTES:** dominio, titular, proveedor DNS, permisos de validación/renovación, política TLS, HSTS y TLS interno. `EXPOSE 8081` no prueba HTTPS. Si forwarding/transportes requieren cambios de aplicación, abrir incremento autorizado aparte; no modificar middleware como parte silenciosa de IaC.

## 9. MongoDB Atlas

| ID | Requisito verificable |
| --- | --- |
| INFRA-014 | Atlas como NoSQL externo, conexión TLS con validación de certificado, usuario de mínimo privilegio y DB por ambiente; connection string externa y acceso de red restringido. |
| INFRA-015 | Conservar índices únicos, BSON, read/write concerns y ACK de 002; inicialización controlada, duplicado concurrente y lectura/credenciales cross-instance verificables. No degradar consistencia para acomodar un tier. |

**DECISIÓN PROPUESTA:** acceso desde salidas autorizadas de §6 con secretos externos. Separar permisos para índices/inicialización y runtime según operaciones reales. No crear cluster, usuarios ni reglas ahora. Backups/recuperación dependen del tier y deben validarse; no asumir capa gratuita, private endpoint o disponibilidad incluidos.

**DECISIONES PENDIENTES:** proyecto/cluster/región/tier, capacidad, cobertura de respaldo, responsable Atlas y gestión Terraform frente a preprovisión externa aprobada. AWS es el provider previsto; cualquier provider Atlas adicional requiere decisión explícita, no incorporación automática.

## 10. Redis productivo y continuidad de seguridad

| ID | Requisito verificable |
| --- | --- |
| INFRA-016 | Redis compartido por todas las instancias y separado por ambiente; cuotas, JWT revocation, logical clock y security state conservan semántica de 002. Sin fallback local o fail-open. |
| INFRA-017 | Acreditar Redis TIME, Lua y atomicidad de las claves involucradas, TLS/autenticación y comandos realmente usados; no elegir Redis Cluster, Serverless, Valkey u otro compatible sin prueba previa. |
| INFRA-018 | Política `noeviction`, memoria total y reserva operativa aprobadas, TTL/cardinalidad/topes conservados y alarmas de capacidad. No tratar 64 MiB del presupuesto de cuotas como dimensionamiento total del servidor. |
| INFRA-019 | Ante pérdida detectada, estado no verificable o confianza incierta: fail-closed/503 según contrato, cierre coordinado y recuperación validada o cuarentena. Conservar el riesgo residual de pérdida catastrófica totalmente silenciosa no observable. |

**DECISIÓN PROPUESTA preferida / objetivo administrado:** ElastiCache **basado en nodos**, con parameter group `noeviction`, red privada, TLS y autenticación. Evaluar réplica/Multi-AZ, costo y modo no particionado compatible con scripts; no aprobar topología por el nombre del servicio. Replicación/failover no garantizan conservación de todas las escrituras confirmadas.

**DECISIÓN PROPUESTA económica:** Redis dedicado en EC2 privada, distinto de los hosts API, con persistencia y runbook aprobados, capacidad, patching y recuperación. Un único nodo es SPOF; no es equivalente a HA. No abaratar exponiéndolo a Internet ni alojando un Redis independiente por backend.

ElastiCache restringe comandos administrativos (por ejemplo `CONFIG`); verificar parameter groups y APIs soportadas en lugar de copiar el harness local. La prueba de compatibilidad debe cubrir el adapter real, scripts, reconexión, relojes y resultados inciertos. `PING`, un marcador, `WAIT`, persistencia o backups no acreditan continuidad por sí solos. No añadir ledger de revocaciones en MongoDB ni otro almacén de revocaciones duradero.

La recuperación debe preservar revocaciones y cuotas; restaurar una copia antigua o cambiar namespaces no permite reapertura automática. El horizonte de cuarentena de 002 se calcula desde la última emisión posible con márgenes efectivos; no es un RTO aprobado. La detección y fencing del servicio elegido requieren evidencia cloud específica. AUTH-027 no se satisface con este texto.

**DECISIONES PENDIENTES:** opción final, motor/versión, topología, tamaño, ACL/comandos, parámetros, persistencia, respaldo, señales de pérdida, RTO/RPO y responsables.

## 11. DynamoDB authority

| ID | Requisito verificable |
| --- | --- |
| INFRA-020 | Contemplar tabla regional para `DynamoDbAuthenticationSecurityAuthority` conforme al esquema/clave del adapter existente, lecturas consistentes y actualizaciones condicionales; nombre/región externos. No eliminar ni sustituir la autoridad. |
| INFRA-021 | Separar creación de tabla, bootstrap y runtime; definir IAM mínimo, disponibilidad, observabilidad y recuperación. No inicializar o reabrir confianza por el mero arranque, deploy o recreación de tabla. |

Esta autoridad es **operacional compartida**, no almacén de revocaciones individuales, ni la tabla de locking Terraform. La tabla sola no establece un estado de confianza válido. El adapter observado usa clave `Id` y estado de autenticación compartido; esquema, payload inicial y transiciones se validarán contra el código vigente antes de provisionar. La aplicación runtime no recibirá permisos de creación o borrado de tabla por comodidad.

**DECISIONES PENDIENTES:** nombre, capacidad/facturación, respaldo/protección, procedimiento y rol de bootstrap, señales de fallo y alarmas. No se crea un inicializador alternativo ni una segunda autoridad.

## 12. Secretos, configuración e IAM

| ID | Requisito verificable |
| --- | --- |
| INFRA-022 | Mantener material JWT, conexión Atlas, credenciales Redis, clave HMAC/pseudonimización y otros secretos runtime en gestor externo por ambiente. Sin hardcode, valores en Git, imágenes, user-data, planes compartidos, logs u outputs. |
| INFRA-023 | Coordinar rotación y distribución a ambas instancias: conservar claves JWT verificadoras durante vigencia y no resetear cuotas al rotar HMAC. Material independiente por ambiente y finalidad. |
| INFRA-024 | Mínimo privilegio y credenciales temporales; separar operador Terraform, deployment, instancia/runtime y bootstrap. Delimitar recursos y acciones según llamadas reales, sin permisos administrativos globales en la API. |

**DECISIÓN PROPUESTA:** Secrets Manager como preferencia; SSM Parameter Store SecureString como alternativa a evaluar por costo, límites, rotación e IAM/KMS. Configuración no sensible versionable por ambiente. Definir entrega segura al proceso (sin imprimir, persistir en disco sin protección o exponer a otros usuarios), actualización y revocación antes del primer despliegue.

GitHub Secrets solo participa en deployment si resulta necesario; no es fuente runtime. OIDC preferido para credenciales temporales de deployment, sin asumir que necesita claves AWS persistentes. Terraform puede gestionar metadatos/referencias; ingresar valores secretos mediante un flujo seguro separado para evitar almacenarlos en state. `sensitive=true` solo oculta presentación, no elimina secretos del state.

## 13. Observabilidad y logging

| ID | Requisito verificable |
| --- | --- |
| INFRA-025 | Centralizar logs estructurados/saneados con correlación, operación, resultado, versión e instancia; excluir contraseñas/hashes, JWT/Authorization, secretos, cuerpos y datos personales innecesarios. Restringir acceso, retención y exportación. |
| INFRA-026 | CloudWatch con CPU/memoria, estado EC2, target health, errores/latencia, fallos de dependencias/autoridad, capacidad Redis y alarmas con destinatario; medir por categorías acotadas, no identidades como etiquetas. |

**DECISIÓN PROPUESTA:** CloudWatch Logs, métricas, alarmas y vista mínima de operación. Memoria del host y métricas de aplicación pueden requerir agente/exportación futuros autorizados; no son capacidades demostradas por escoger CloudWatch. La alternativa Redis EC2 requiere su propia recolección segura. Separar fallos ALB y aplicación para no prometer un contrato único de errores fuera de la API.

**DECISIONES PENDIENTES:** retención, umbrales, destinatarios, muestreo, métricas custom/agente, control de acceso y costo. La evidencia mínima debe mostrar dos instancias, tráfico, health, error/alarma controlados y capacidad sin datos sensibles. Access logs de ALB u otra instrumentación adicional solo si se aprueban por utilidad, privacidad y costo.

## 14. Liveness y readiness

| ID | Requisito verificable |
| --- | --- |
| INFRA-027 | Conservar `/health` como liveness del proceso, sin presentarlo como prueba de Atlas/Redis/autoridad ni integridad del estado. |
| INFRA-028 | Definir e implementar en incremento aparte readiness sanitizado, dependencias/índices/inicialización/admisión segura y su relación con ALB; validar forwarding confiable antes de aceptación pública. |

**ESTADO OBSERVADO:** readiness no está resuelto; `/health/ready` es solo ruta candidata. Un gate no puede cerrarse con un endpoint inexistente. Hasta su autorización/implementación, se bloquea la aceptación cloud que depende de él. Health checks temporales de liveness no son aceptación de readiness.

La readiness global y la dependencia efectiva de cada operación son distintas: Mongo caído no debe introducir consulta Mongo en Logout. Una política de retirada global debe documentar su impacto operativo sin cambiar el contrato interno. La aplicación debe seguir rechazando de forma segura cuando ALB haga fail-open. No exponer componentes, conexiones, contadores o estado privado en respuestas health.

## 15. Terraform: estructura, variables, outputs y entornos

| ID | Requisito verificable |
| --- | --- |
| INFRA-029 | Infraestructura AWS declarativa en Terraform, versiones/providers aprobados y bloqueados, validación/revisión antes de aplicar. No generar `infra/` ni ejecutar Terraform en esta entrega. |
| INFRA-030 | Inputs tipados/validados para región, ambiente, naming/tags, CIDR/subnets, capacidad, imagen/digest, dominio/certificado, referencias a secretos, Redis, tabla authority y límites/etiquetas de costo. Sin secretos en tfvars versionados. |
| INFRA-031 | Outputs no secretos para URL HTTPS, ALB/target group, instancias, ECR, logs y referencias de integración; no tokens, credenciales, connection strings ni claves. Mantener identificadores internos fuera de evidencia pública no necesaria. |
| INFRA-032 | Proteger state, backups, planes y locking; separar ambientes y prohibir commits de state, secretos o planes sensibles. Migración remota aprobada antes de uso compartido/GitHub. |
| INFRA-033 | Naming y tags consistentes (proyecto, ambiente, responsable, propósito/costo); separar configuración y datos de ambientes, sin clonar secretos ni afirmar que existe producción/staging. |

**DECISIÓN PROPUESTA — estructura futura, NO creada:**

```text
infra/
├── main.tf
├── providers.tf
├── versions.tf
├── variables.tf
├── outputs.tf
├── networking.tf
├── security.tf
├── compute.tf
├── registry.tf
├── load-balancer.tf
├── secrets.tf
├── redis.tf
├── authority.tf
└── observability.tf
```

El provider AWS es el objetivo; versiones y providers complementarios quedan pendientes. Evitar módulos complejos sin necesidad. La estructura es contrato de responsabilidades, no archivos autorizados en esta entrega. Elementos externos preexistentes requerirán referencia/importación deliberada, no recreación automática.

Entornos conceptuales: local/test existente, cloud académico objetivo y producción futura solo si se autoriza como tal. Un despliegue académico público debe cumplir seguridad aunque no implique SLA productivo. Seleccionar un ambiente cloud inicial y estado aislado; no provisionar duplicados para aparentar separación.

## 16. Estrategia de state

**DECISIÓN PROPUESTA inicial académica:** state local protegido, un operador, almacenamiento con acceso restringido y respaldo cifrado comprobable; nunca Git. Requiere procedimiento de recuperación, no solo nombrar una carpeta local. No usar state local efímero de un runner GitHub.

**Antes de uso compartido o GitHub:** backend remoto protegido, cifrado, versionado, locking, IAM mínimo y segregación por ambiente; sin publicación de state/plan en artefactos abiertos.

**Propuesta futura:** S3 con locking nativo compatible con la versión Terraform aprobada. No crear bucket ni tabla ahora. El locking con DynamoDB de Terraform está deprecado en la documentación de referencia; no confundirlo con la authority que sí usa Authentication. Bootstrap y migración del backend tendrán su propio permiso, revisión, respaldo y prueba de exclusión mutua.

## 17. Despliegue, rollback y reproducibilidad

| ID | Requisito verificable |
| --- | --- |
| INFRA-034 | 003 expone inputs/outputs y permisos para 004; deployment futuro manual por `workflow_dispatch`, únicamente después de CI verde del mismo commit/artefacto. No deployment automático por push/merge ni workflows en 003. |
| INFRA-035 | Rollback documentado a digest/configuración compatibles previos, con drenaje, salud y verificación; no rollback ciego de state de seguridad, borrado de cuotas o retiro prematuro de claves JWT. |
| INFRA-036 | Demostrar reproducción del ambiente mediante IaC, cambios/drift revisados y recuperación controlada, sin recrear datos o autoridad para "limpiar" un fallo. Ningún destroy sin autorización y protección de datos. |
| INFRA-037 | Evidencia académica trazable por requisito/tarea/gate, commit/digest, entorno, fecha, resultado y almacenamiento protegido; excluir secretos y distinguir resultados locales/cloud. |
| INFRA-040 | Validar Authentication pública y estado compartido entre instancias, casos negativos y fallos operativos autorizados; aceptación de 003 no cierra automáticamente diferidos o gates de 002. |

004 definirá **CI automático, quality gates, workflow_dispatch, deploy manual y smoke tests**. 003 solo define contrato de artefacto inmutable, ambiente, referencias, acceso, salida URL/health y rollback. La ejecución cloud de pruebas de aceptación de 003 no diseña el workflow de smoke tests de 004.

**DECISIONES PENDIENTES:** ownership del digest (Terraform frente a CD, sin dos escritores), mecanismo de actualización en EC2, aprobación de cambios IaC, drenaje y concurrencia, retención de versiones y rollback. Mantener dos instancias en la aceptación; pérdida temporal de capacidad durante mantenimiento debe quedar explícita, no prometer cero downtime sin prueba. Primer deployment público requiere integración aprobada de 004, con CI verde y promoción manual mediante `workflow_dispatch`; ninguna se ejecuta aquí.

## 18. Criterios de aceptación futuros

Todos **PENDIENTES / NO VERIFICADOS**. Evidencia `E(T-INFRA-nnn)` y gates se definen en [task.md](task.md); la tabla no declara cumplimiento.

| Criterio | Resultado exigido | Requisitos | Gate |
| --- | --- | --- | --- |
| AC-I-01 | Baseline/costo/versiones/state aprobados; validación Terraform acreditada | INFRA-003, 029, 032, 033, 038 | GATE-I-A |
| AC-I-02 | VPC/rutas/SG/SSM y salida Atlas verificadas; sin bypass público al backend/Redis | INFRA-007–009, 024 | GATE-I-B |
| AC-I-03 | Dos EC2 reales en dos AZ, mismo digest .NET 10, capacidad y recuperación demostradas | INFRA-002, 004–006, 039 | GATE-I-C |
| AC-I-04 | ALB distribuye a ambos targets; drenaje, unhealthy y todos-unhealthy probados con seguridad | INFRA-010, 011, 027, 028 | GATE-I-D |
| AC-I-05 | URL/dominio HTTPS válidos, redirección y forwarding/origen sin spoofing; TLS interno resuelto | INFRA-012, 013 | GATE-I-E |
| AC-I-06 | Atlas cumple red/TLS/permisos, índices/ACK/concerns y concurrencia cross-instance | INFRA-014, 015 | GATE-I-F |
| AC-I-07 | Redis acredita comandos/scripts/reloj, noeviction, capacidad, cuotas/revocación compartidas y recuperación segura | INFRA-016–019 | GATE-I-F |
| AC-I-08 | Authority AWS conforme al adapter, IAM/bootstrap y fallos verificados sin reapertura automática | INFRA-020, 021 | GATE-I-F |
| AC-I-09 | Secretos externos/rotación, IAM mínimo y separación por ambiente sin filtraciones | INFRA-006, 022–024, 033 | GATE-I-B, GATE-I-F |
| AC-I-10 | Logs/métricas/alarma controlada disponibles y saneados; retención/umbrales aprobados | INFRA-025, 026 | GATE-I-G |
| AC-I-11 | Liveness/readiness diferenciados, implementados y probados; no sustituyen fail-closed | INFRA-011, 019, 027, 028 | GATE-I-D, GATE-I-F |
| AC-I-12 | IaC revisada/reproducible; inputs/outputs/state protegidos; evidencia de drift y recuperación | INFRA-029–033, 036, 037 | GATE-I-H |
| AC-I-13 | Integración 004 conserva deploy manual tras CI verde, digest trazable y rollback seguro | INFRA-005, 034, 035 | GATE-I-H, GATE-I-I |
| AC-I-14 | Register 201, Login 200 y Logout 204 conforme 002 por URL HTTPS; duplicados/errores/cuotas/revocación/fallos verificados entre ambas instancias | INFRA-001, 015–021, 040 | GATE-I-I |
| AC-I-15 | Paquete académico permite verificar rúbrica, costos y límites sin declarar satisfecho AUTH-027 integralmente ni gates de 002 | INFRA-002, 003, 037–040 | GATE-I-H, GATE-I-I |

Pruebas cross-instance: identificar A/B en telemetría interna saneada, no agregar endpoint o cabecera pública de diagnóstico. Revocar en A y comprobar rechazo en B mediante superficie protegida ya autorizada o prueba autorizada aparte; **no inventar `/me`, Orders o endpoint productivo para probar**. Repetir Logout no sirve como prueba de rechazo normal de un token revocado, dada su excepción deliberada. Cuotas deben comprobarse sin afinidad ni reinicio entre hosts. Fallos inducidos requieren entorno/ventana autorizados y protección de datos.

## 19. Decisiones pendientes

| ID | Decisión pendiente | Resolución antes de |
| --- | --- | --- |
| DP-I-01 | Cuenta, región/AZ, presupuesto máximo y elegibilidad/créditos/precios vigentes | T-INFRA-001, cualquier cargo |
| DP-I-02 | AMI, CPU, tamaños, almacenamiento, versiones/parches y política de recuperación | T-INFRA-002, 009–010 |
| DP-I-03 | Perfil red pública restringida frente a privada/NAT/endpoints; salidas estáticas y DNS | T-INFRA-003–006 |
| DP-I-04 | Dominio/subdominio, titular, proveedor DNS, ACM y renovación | T-INFRA-013–014 |
| DP-I-05 | TLS interno, puerto, forwarding confiable, timeouts/health/draining | T-INFRA-006, 011–014, 023–024 |
| DP-I-06 | Atlas región/tier/respaldo/capacidad; ownership y eventual provider | T-INFRA-015–016 |
| DP-I-07 | Redis opción, motor/topología/tamaño/comandos/ACL; RTO/RPO, señales/recuperación | T-INFRA-017–018 |
| DP-I-08 | Authority nombre/capacidad/protección/bootstrap/operador/recuperación | T-INFRA-020 |
| DP-I-09 | Gestor/entrega/rotación de secretos, KMS y políticas IAM concretas | T-INFRA-005, 019–020 |
| DP-I-10 | Logs/agentes/métricas/umbrales/retención/destinatarios y privacidad | T-INFRA-021–022 |
| DP-I-11 | Readiness y forwarding: autorización separada de aplicación, ruta y semántica | T-INFRA-023–024 |
| DP-I-12 | Versiones Terraform/providers, estado local/remoto, backend/bootstrap/migración | T-INFRA-002, 025–027 |
| DP-I-13 | Ambiente cloud inicial, naming/tags, acceso, evidencia y retención | T-INFRA-001–002, 029–030 |
| DP-I-14 | Contrato con 004, ownership del digest, operación manual, rollback y revisión de cambios IaC | T-INFRA-008, 026–028 |
| DP-I-15 | Ventana/objetivos de prueba, superficie protegida autorizada para revocación, límites de mantenimiento/escalado | T-INFRA-010, 012, 024, 028 |

No resolver una DP mediante ejecución implícita. Si bloquea un criterio, el gate permanece abierto.

## 20. Riesgos, costo y evidencia académica

| Riesgo | Tratamiento requerido |
| --- | --- |
| Cargos inesperados | Estimación regional y aprobación previa, tags, seguimiento y plan de apagado/limpieza autorizado; una alarma de presupuesto no es tope duro |
| EC2 detenida pero otros cargos activos | Contabilizar EBS/snapshots, IPv4/EIP, ALB, NAT, logs, secretos y datos persistentes; no asumir cero costo por detener compute |
| Redis económico SPOF o failover con pérdida | Declarar disponibilidad real y límites; cierre coordinado y recuperación/cuarentena probados; no afirmar AUTH-027 resuelto |
| Dos AZ de API con dependencia única | No prometer HA integral; decidir disponibilidad de Atlas/Redis/authority y objetivos RTO/RPO |
| Red pública restringida o mala regla de egreso | Revisar SG/rutas/salidas; pruebas de bypass; no exponer SSH/Redis ni abrir Atlas a todo Internet |
| Robo de state/secretos/logs | Cifrado, acceso mínimo, saneamiento, backup y rotación; evitar valores secretos en Terraform |
| Health engañoso/ALB fail-open | Readiness real más fail-closed de aplicación; observabilidad separada y pruebas de todos unhealthy |
| Proxy/origen falso | Confianza explícita, pruebas spoofing y orden middleware aprobado; no usar headers arbitrarios como identidad |
| Drift/dos escritores o rollback inseguro | Ownership de imagen/config, revisión de cambios, digest previo, no restaurar estado de seguridad antiguo |
| Evidencia solo local/temporal | Recopilar evidencia cloud controlada con metadatos y retención aprobada; no convertir TRX local en validación AWS |

Recursos potencialmente facturables: EC2/EBS/snapshots, IPv4/EIP, ALB/LCU, NAT/endpoints, tráfico inter-AZ/Internet, ElastiCache o Redis EC2, Atlas, ECR, CloudWatch/logs/alarmas/métricas, Secrets Manager/SSM/KMS, DynamoDB, S3/state y DNS/registro de dominio. No hay precios ni gratuidad acreditados; validar cuenta, región, créditos, duración y volumen al implementar. Comparar **costo total**, no solo compute.

Evidencia mínima futura: diagrama y mapping Terraform→recursos, plan/aplicación revisados, dos instancias/AZ/digest y targets, HTTPS/certificado/DNS, pruebas públicas saneadas de Authentication y controles distribuidos, alarmas/logs/métricas, costos y reproducción/rollback. Cada pieza llevará referencia `E(T-INFRA-nnn)` y vínculo a AC/gate; su registro y almacenamiento se autorizan después. Ninguna evidencia de ejecución 003 existe por crear estos archivos.

## 21. Referencias técnicas y cierre documental

Referencias para revalidar capacidades/versiones en la implementación (no acreditación de precios): [ALB health/fail-open](https://docs.aws.amazon.com/elasticloadbalancing/latest/application/target-group-health-checks.html), [parámetros ElastiCache](https://docs.aws.amazon.com/AmazonElastiCache/latest/dg/ParameterGroups.Engine.html), [comandos ElastiCache](https://docs.aws.amazon.com/AmazonElastiCache/latest/dg/SupportedCommands.html) y [backend S3 Terraform](https://developer.hashicorp.com/terraform/language/backend/s3).

Revisión documental: se conserva 001 sin reescribir su historia, 002 sin cambios de seguridad ni cierres cloud implícitos, DynamoDB authority existente, liveness/readiness separados y deployment manual futuro de 004. No se incluyen credenciales ni dominios inventados. Solo se materializan spec/plan/task de 003; no hay implementación, gates satisfechos, stage, commit o push autorizados en esta entrega.
