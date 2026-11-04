# ENVER Backend

Proyecto académico de backend cuya solución es `Enver.Backend` y cuyo proyecto inicial es `Enver.API`, basado en C#, ASP.NET Core y .NET 10.

## Estado actual

La **Fase 0 de gobierno documental SDD está completada en su elaboración**. Después se crearon, mediante solicitudes separadas, 5 subagentes, 5 skills y los documentos de arquitectura base. La **fase funcional activa es `002-authentication`; su spec está APROBADO PARA PLANIFICACIÓN**, habilitada y no iniciada.

`001-backend-foundation` tuvo una **implementación técnica parcial/inicial integrada mediante PR #1** a `develop` (merge `bef06b6`). El responsable confirma que el backend compila en .NET 10, funciona `GET /health` y se actualizó la dependencia vulnerable `Microsoft.OpenApi`; la lectura confirma `net10.0`, el health check básico del host y la referencia 2.13.0. No se repitieron esas verificaciones en esta entrega ni se acredita readiness de dependencias.

**Foundation no está completada integralmente.** Sus tres documentos se conservan intactos como registro histórico: 49 FND, 28 PLD, 38 TBF (con estado de origen PENDIENTE / NO EJECUTADA) y 17 DEC. Ni el PR ni esta actualización los dan por cumplidos, aprobados o cerrados. La presencia de Dockerfile y artefactos generados no acredita pruebas, seguridad o despliegues.

El [spec de autenticación](specs/002-authentication/spec.md), versión 0.2.0, contiene 48 AUTH y queda **APROBADO PARA PLANIFICACIÓN** por decisión expresa del responsable. Permite elaborar `plan.md` en una entrega posterior, no implementar código. En esta intervención no se generan plan, tareas ni código funcional.

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
| [`specs/002-authentication/spec.md`](specs/002-authentication/spec.md) | Registro, login y logout; 48 AUTH; v0.2.0 **APROBADO PARA PLANIFICACIÓN**. |

El historial de Fase 0, la corrección A-01/A-02, la apertura formal de 002 y su aprobación posterior se conservan en [`memoria.md`](memoria.md), §§7–13. La constitución canónica y su referencia raíz no se modifican. Sus menciones al alcance inicial y los límites históricos de 001 no se reforman ni se interpretan como aprobación de implementación.

## Base confirmada

- C#, ASP.NET Core y .NET 10.
- MongoDB Atlas como NoSQL principal, inicialmente para usuarios, y Redis para caché distribuida, cuotas y revocación JWT.
- AWS como nube; GitHub Actions para CI/CD y Terraform para IaC en incrementos posteriores.
- Identidad mediante correo institucional UTTT; endpoints objetivo `POST /api/v1/auth/register`, `/login` y `/logout`.
- JWT emitido por ENVER con HS256, `iss=enver-api`, `aud=enver-mobile`, duración inicial de 15 minutos y skew máximo de 30 segundos; secretos externos.
- AD-01 a AD-08 aprobadas: contratos/normalización/límites, Problem Details RFC 9457, estados `Active`/`Disabled`, `PasswordHasher<TUser>` obligatorio, modelo mínimo `users` con UUIDv4 e índice único, claims/rotación JWT, política de revocación Redis y cuotas distribuidas configurables. REGISTER inicial: 3/15 min por email y 20/15 min por origen; cuotas completas en el spec §3.5.
- Alcance de esta intervención: actualizar el spec 002 y los tres documentos de estado para registrar su aprobación para planificación, sin generarla ni ejecutar trabajo técnico.

La rúbrica también exige, para fases futuras, dos instancias en nube, balanceador, JWT, Redis distribuido, caché, paginación, índices, limitación de solicitudes (rate limiting), registro de eventos (logging), observabilidad y versionamiento. **No se declaran implementados ni verificados.**

## Propuestas y pendientes

Se mantiene como propuesta una arquitectura REST modular con separación de responsabilidades; las rutas de autenticación v1 sí están expresamente aprobadas. No se adopta un patrón definitivo de capas ni una organización adicional de proyectos.

AD-01 a AD-08 quedan cerradas como decisiones de especificación, no como implementación. Sus mecanismos técnicos —BSON/driver/read-write concerns, actualización segura de hash, rotación operativa, integridad/recuperación Redis, margen TTL y coordinación/pseudonimización de cuotas— se concretarán en el plan y fases aplicables. AD-09 difiere calibración de percentiles candidatos (no SLA ni mediciones); AD-10 ratifica privacidad/secretos y difiere detalles/retención antes del despliegue productivo; AD-11 difiere infraestructura y AD-12 herramientas/gates/evidencia a plan/DevSecOps. Los 12 IDs AD se conservan en el spec §10.

AWS, Terraform, GitHub Actions/CI-CD, dos instancias, Load Balancer, HTTPS publicado, trust de proxies, observabilidad/CloudWatch, Redis administrado y construcción/publicación/despliegue Docker son requisitos futuros no implementados ni verificados; no se seleccionan recursos concretos ahora. La garantía Redis ante pérdida de revocaciones exige demostración técnica posterior, no basta persistencia/ping. Build, unit/integration/security tests, SAST y dependency/secret/container scanning siguen obligatorios futuros: ningún gate se considera satisfecho por documentarlo.

## Restricciones vigentes

- No desarrollar funcionalidades específicas de ENVER ni crear controladores, DTOs, entidades, servicios, CRUD o endpoints de negocio.
- Solo se actualizan `specs/002-authentication/spec.md`, `AGENTS.md`, `memoria.md` y `README.md`. No crear `plan.md`, `task.md` ni equivalentes en esta intervención; planificación documental posterior habilitada, no iniciada.
- No modificar constitución canónica, referencia raíz, los tres documentos de 001, agentes o skills ni ampliar permisos.
- No modificar código, configuración ni archivos existentes de Visual Studio.
- No ejecutar compilaciones, la aplicación, pruebas, instalaciones, migraciones ni despliegues.
- No interpretar requisitos futuros o propuestas como autorización para ejecutarlos.

No se incluyen instrucciones de ejecución: no se instala ningún paquete, se modifica `Program.cs`, se conecta MongoDB/Redis ni se implementan pruebas. El trabajo se detiene tras esta actualización documental. La aprobación para planificación está registrada; elaborar/aprobar el plan y autorizar tareas o implementación siguen siendo actos separados. No se acredita aceptación técnica ni cumplimiento integral de la rúbrica.
