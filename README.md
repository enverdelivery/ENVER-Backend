# ENVER Backend

Proyecto académico de backend cuya solución es `Enver.Backend` y cuyo proyecto inicial es `Enver.API`, basado en C#, ASP.NET Core y .NET 10.

## Estado actual

La **Fase 0 de gobierno documental SDD está completada en su elaboración**. Después se crearon, mediante solicitudes separadas, 5 subagentes, 5 skills y los documentos de arquitectura base. La **fase funcional activa es `002-authentication`; spec APROBADO PARA PLANIFICACIÓN, plan APROBADO PARA DESCOMPOSICIÓN EN TAREAS y task APROBADO PARA EJECUCIÓN INCREMENTAL**. Las 79 tareas están PENDIENTE / NO EJECUTADA; sin implementación ni ejecución técnica autorizadas por esta entrega.

`001-backend-foundation` tuvo una **implementación técnica parcial/inicial integrada mediante PR #1** a `develop` (merge `bef06b6`). El responsable confirma que el backend compila en .NET 10, funciona `GET /health` y se actualizó la dependencia vulnerable `Microsoft.OpenApi`; la lectura confirma `net10.0`, el health check básico del host y la referencia 2.13.0. No se repitieron esas verificaciones en esta entrega ni se acredita readiness de dependencias.

**Foundation no está completada integralmente.** Sus tres documentos se conservan intactos como registro histórico: 49 FND, 28 PLD, 38 TBF (con estado de origen PENDIENTE / NO EJECUTADA) y 17 DEC. Ni el PR ni esta actualización los dan por cumplidos, aprobados o cerrados. La presencia de Dockerfile y artefactos generados no acredita pruebas, seguridad o despliegues.

El [spec de autenticación](specs/002-authentication/spec.md), versión 0.3.0, conserva 48 AUTH. El [plan técnico](specs/002-authentication/plan.md), versión 0.2.0, contiene 25 decisiones PLD-AUTH y cobertura de diseño 48/48. APR-03 resuelve la discrepancia normativa AUTH-027 / PLD-AUTH-08 sin cambiar arquitectura; el historial permanece en plan §23.1 y memoria §15. Aquella entrega habilitó crear posteriormente tareas, no implementarlas.

La autorización documental posterior SOL-TASK-002 crea [task.md](specs/002-authentication/task.md) v0.1.0: **79 tareas**, cobertura documental **AUTH 48/48, PLD-AUTH 25/25, AC 21/21** y **8 gates pendientes**. **APROBADO PARA EJECUCIÓN INCREMENTAL** permite solicitar ejecución posterior de una tarea/incremento con autorización delimitada, no ejecutarla automáticamente, satisfacer gates, resolver diferidos o autorizar AWS. Sin implementación; **AUTH-027 sigue NO IMPLEMENTADO / NO SATISFECHO**. Registro en memoria §16; spec/plan intactos.

## Documentación

La **[constitución canónica](.specify/memory/constitution.md)** es la autoridad principal de la documentación del proyecto.

| Archivo | Propósito |
| --- | --- |
| [`.specify/memory/constitution.md`](.specify/memory/constitution.md) | Constitución: alcance, SDD, diseño, seguridad, calidad y gobierno. |
| [`constitution.md`](constitution.md) | Acceso a la constitución canónica, sin duplicarla. |
| [`AGENTS.md`](AGENTS.md) | Instrucciones y restricciones para agentes. |
| [`memoria.md`](memoria.md) | Estado observado, decisiones, propuestas y pendientes. |
| [`README.md`](README.md) | Presentación del proyecto y orientación documental. |
| [`.opencode/agents/`](.opencode/agents/) | 5 subagentes creados; definiciones y permisos conservados, sin autorización de invocación por su existencia. |
| [`.opencode/skills/`](.opencode/skills/) | 5 skills creadas; su existencia no autoriza carga o ejecución. |
| [`specs/001-backend-foundation/spec.md`](specs/001-backend-foundation/spec.md) | Arquitectura base: 49 requisitos FND y 17 pendientes DEC; borrador pendiente de aprobación. |
| [`specs/001-backend-foundation/plan.md`](specs/001-backend-foundation/plan.md) | 28 decisiones PLD propuestas o con detalles pendientes; borrador pendiente de aprobación. |
| [`specs/001-backend-foundation/task.md`](specs/001-backend-foundation/task.md) | 38 tareas TBF; todas PENDIENTE / NO EJECUTADA; borrador pendiente de aprobación. |
| [`specs/002-authentication/spec.md`](specs/002-authentication/spec.md) | Registro, login y logout; 48 AUTH; v0.3.0 **APROBADO PARA PLANIFICACIÓN**. |
| [`specs/002-authentication/plan.md`](specs/002-authentication/plan.md) | 25 decisiones PLD-AUTH; v0.2.0 **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**, sin aceptación técnica ni implementación. |
| [`specs/002-authentication/task.md`](specs/002-authentication/task.md) | 79 tareas T-AUTH-001 a T-AUTH-079; v0.1.0 **APROBADO PARA EJECUCIÓN INCREMENTAL**; todas pendientes/no ejecutadas, sin gates satisfechos. |

El historial de Fase 0, A-01/A-02, apertura/aprobación de 002, higiene Git y resolución normativa se conserva en [`memoria.md`](memoria.md), §§7–15; §16 añade la creación documental del catálogo. La constitución canónica y su referencia raíz no se modifican. Sus menciones al alcance inicial y los límites históricos de 001 no se reforman ni se interpretan como aprobación de implementación.

## Base confirmada

- C#, ASP.NET Core y .NET 10.
- MongoDB Atlas como NoSQL principal, inicialmente para usuarios, y Redis para caché distribuida, cuotas y revocación JWT.
- AWS como nube; GitHub Actions para CI/CD y Terraform para IaC en incrementos posteriores.
- Identidad mediante correo institucional UTTT; endpoints objetivo `POST /api/v1/auth/register`, `/login` y `/logout`.
- JWT emitido por ENVER con HS256, `iss=enver-api`, `aud=enver-mobile`, duración inicial de 15 minutos y skew máximo de 30 segundos; secretos externos.
- AD-01 a AD-08 aprobadas: contratos/normalización/límites, Problem Details RFC 9457, estados `Active`/`Disabled`, `PasswordHasher<TUser>` obligatorio, modelo mínimo `users` con UUIDv4 e índice único, claims/rotación JWT, política de revocación Redis y cuotas distribuidas configurables. REGISTER inicial: 3/15 min por email y 20/15 min por origen; cuotas completas en el spec §3.5.
- Antecedente APR-03: ajustó únicamente AUTH-022/027/043 y referencias Redis, sincronizó el plan y los tres documentos de estado, sin tareas ni trabajo técnico en aquella entrega. SOL-TASK-002 crea ahora el catálogo y sincroniza el estado documental, sin modificar spec/plan ni implementar; AUTH-023/024/025/026/038/039 conservan su contenido.

La rúbrica también exige, para fases futuras, dos instancias en nube, balanceador, JWT, Redis distribuido, caché, paginación, índices, limitación de solicitudes (rate limiting), registro de eventos (logging), observabilidad y versionamiento. **No se declaran implementados ni verificados.**

## Propuestas y pendientes

Para 002 se aprueba un único Enver.API con separación lógica, sin proyectos separados de aplicación/dominio/infraestructura y con tres proyectos futuros de pruebas. Redis compartido para revocación/cuotas, sin ledger MongoDB, segundo almacén duradero obligatorio o transacciones distribuidas. No se exigen dos Redis físicos ni se agregan patrones innecesarios.

AD-01 a AD-08 son decisiones de especificación, no implementación; AD-07 se precisa por APR-03. Logout confirmado exige rechazo distribuido con continuidad confiable; pérdida detectada/integridad incierta exige fail-closed/503 y recuperación validada o cuarentena. La pérdida catastrófica totalmente silenciosa no observable permanece residual de infraestructura, no eliminada. AUTH-027 está aprobado documentalmente, NO IMPLEMENTADO / NO SATISFECHO. No se cambian TTL, JWT, idempotencia, sesiones o cuotas.

Plan §23.2 conserva detalles diferidos de versiones/grafo, MongoDB/Redis y ACK/detección/recuperación, cardinalidad/HMAC, calibración temporal, privacidad/retención, TLS/proxies/AWS/readiness, herramientas/gates y Docker/publish antes de build sensible. AD-09/11/12 y detalles de AD-10 siguen pendientes de sus puertas; los 12 IDs AD se conservan. La aprobación del plan no acredita estas capacidades ni convierte los candidatos en decisiones técnicas finales.

AWS, Terraform, GitHub Actions/CI-CD, dos instancias, Load Balancer, HTTPS publicado, trust de proxies, observabilidad/CloudWatch, Redis administrado y construcción/publicación/despliegue Docker son requisitos futuros no implementados ni verificados; no se seleccionan recursos concretos ahora. La garantía Redis ante pérdida de revocaciones exige demostración técnica posterior, no basta persistencia/ping. Build, unit/integration/security tests, SAST y dependency/secret/container scanning siguen obligatorios futuros: ningún gate se considera satisfecho por documentarlo.

## Restricciones vigentes

- No desarrollar funcionalidades específicas de ENVER ni crear controladores, DTOs, entidades, servicios, CRUD o endpoints de negocio.
- SOL-TASK-002 solo crea `specs/002-authentication/task.md` y sincroniza `AGENTS.md`, `memoria.md` y `README.md`. No modificar spec/plan ni crear otros artefactos; el catálogo no autoriza ejecutar sus tareas.
- No modificar constitución canónica, referencia raíz, los tres documentos de 001, agentes o skills ni ampliar permisos.
- No modificar código, configuración ni archivos existentes de Visual Studio.
- No ejecutar compilaciones, la aplicación, pruebas, instalaciones, migraciones ni despliegues.
- No interpretar requisitos futuros o propuestas como autorización para ejecutarlos.

La entrega es exclusivamente documental: se crea el catálogo futuro, no se instala ningún paquete, modifica `Program.cs`, conecta MongoDB/Redis, implementa código/pruebas ni ejecuta builds/pruebas/scanners. El trabajo se detiene tras task.md y la sincronización de estado, sin workflows, Terraform, commit o push. Cada tarea/incremento requiere autorización técnica posterior independiente. No se acredita aceptación técnica, AUTH-027 satisfecho ni cumplimiento integral de la rúbrica.
