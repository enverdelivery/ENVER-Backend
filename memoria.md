# Memoria de ENVER Backend

## 1. Estado actual

- **Actualización posterior vigente del 12 de noviembre de 2026:** **T-AUTH-058 COMPLETADA / EVIDENCIA REGISTRADA; GATE-E SATISFECHO / CERRADO; Authentication TERMINADA — exclusivamente LOCAL/TEST REPRESENTATIVO**. Una IT completa **310/310** y después una ST completa **223/223**, ambas0fallidas/0omitidas,exit0; UT **684/684** previas conservadas sin repetir, producto/UT intactos. E058/revisión formal e incidentes en [§55](#55-revalidación-final-e058-y-cierre-localtest-de-gate-e) y task§§82–84. GATE-G/H pendientes,059–079no iniciadas,008conserva su estado previo yAUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. Sin aceptación productiva/cloud/despliegue ni cierre integral de002. Solo task/memoria actualizados; los estados anteriores siguientes se conservan como antecedentes.

- **Actualización posterior vigente del 11 de noviembre de 2026:** **T-AUTH-043/044/045/046 COMPLETADAS / EVIDENCIA REGISTRADA; GATE-F SATISFECHO / CERRADO, LOCAL/TEST REPRESENTATIVO**. **GATE-B CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA** por logical clock Redis. UT previas **633/633** conservadas, IT actuales **259/259**, ST finales **152/152**, 0 fallidas/omitidas finales; historial y relación temporal en [§52](#52-logout-cierre-localtest-de-gate-f-y-correctivo-gate-b). GATE-E completo pendiente: 032/047–051/058 no iniciadas. Register y JWT conservan aceptación local/test; Logout completo local/test, Login pendiente y **Authentication NO TERMINADA**. Los estados anteriores se conservan como antecedentes.

- **Actualización posterior vigente del 10 de noviembre de 2026:** T-AUTH-026/027 **COMPLETADAS / EVIDENCIA REGISTRADA**; revisión formal de E023–E027: **GATE-C SATISFECHO / CERRADO como checkpoint local/test de componentes**, sin Atlas/HA/failover cloud ni composición productiva. UT **436/436**, IT **129/129**, ST **71/71**; [§48](#48-cierre-de-t-auth-026-y-t-auth-027-y-revisión-formal-de-gate-c). GATE-A/B cerrados; **GATE-D–H PENDIENTES / NO SATISFECHOS**; Register y 056/057/058 no iniciados. Los estados previos siguientes se conservan como antecedentes.

- **Actualización posterior vigente del 10 de noviembre de 2026:** T-AUTH-024 y T-AUTH-025 **COMPLETADAS / EVIDENCIA REGISTRADA**, con UT **414/414**, IT **120/120**, ST **71/71**; detalle y límites en [§47](#47-cierre-de-t-auth-024-y-t-auth-025-tras-regresión-final). **026/027 NO INICIADAS; GATE-C PENDIENTE / NO SATISFECHO; GATE-A/B SATISFECHOS / CERRADOS**. Los estados de creación documental y entregas anteriores siguientes se conservan como antecedentes, no como autorización técnica adicional.

- **Fase funcional activa:** `002-authentication`; spec **APROBADO PARA PLANIFICACIÓN**, plan **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**, task v0.1.0 **APROBADO PARA EJECUCIÓN INCREMENTAL**. Catálogo de 79 tareas no ejecutadas, sin implementación; creación documental en §16. Resolución APR-03 en §15 e higiene previa en §14.
- **Fase 0:** completada en su elaboración documental; no habilitó automáticamente entregas posteriores ni implementación.
- **Solución:** `Enver.Backend`.
- **Proyecto:** `Enver.API`.
- **Alcance vigente:** SOL-TASK-002 autoriza crear únicamente `specs/002-authentication/task.md` y sincronizar AGENTS/memoria/README para registrar las 79 tareas aprobadas, sin ejecutarlas. Sin modificar código/configuración, instalar, conectar dependencias, ejecutar builds/pruebas/scanners, crear workflows, commit o push. Spec/plan, constituciones y 001 permanecen intactos; los §§7–15 conservan el historial de sus entregas.
- **Fuente de autoridad:** [constitución canónica](.specify/memory/constitution.md), versión documental inicial 1.0.0.
- **Estado observado:** 5 subagentes y 5 skills conservados; foundation documentada mediante 49 FND, 28 PLD y 38 TBF; implementación técnica parcial/inicial integrada mediante PR #1. Detalle y límites de evidencia en §12.
- **Aprobación:** 001 conserva sus borradores y estados de origen. Spec 002 v0.3.0 **APROBADO PARA PLANIFICACIÓN** y plan v0.2.0 **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**, por APR-03; task v0.1.0 **APROBADO PARA EJECUCIÓN INCREMENTAL**, por SOL-TASK-002 (§16). Creación original y APR-02 permanecen en §§12–13; creación del plan, bloqueo inicial y resolución posterior en §15 y plan §23.1. No equivalen a aceptación técnica ni autorización inmediata de tareas.
- **Ejecución:** no se marca completada ninguna TBF ni cerrado íntegramente ningún DEC; la implementación parcial no acredita cumplimiento integral de FND/PLD/TBF/DEC. No se implementa autenticación en esta entrega.
- **Especificación funcional:** [002-authentication](specs/002-authentication/spec.md), 48 requisitos AUTH-001 a AUTH-048 para registro, login y logout versionados por ruta.
- **Plan técnico:** [plan 002](specs/002-authentication/plan.md), 25 decisiones PLD-AUTH-01 a PLD-AUTH-25, trazabilidad de diseño 48/48 y diferidos con puertas; conservado intacto en esta entrega.
- **Catálogo de tareas:** [task 002](specs/002-authentication/task.md), 79 T-AUTH-001 a T-AUTH-079, todas PENDIENTE / NO EJECUTADA; cobertura documental AUTH 48/48, PLD-AUTH 25/25 y AC 21/21; 8 gates pendientes.

La creación documental no acredita cumplimiento técnico de la rúbrica. El estado APROBADO PARA EJECUCIÓN INCREMENTAL solo permite solicitar ejecución posterior de una tarea/incremento con autorización propia; no satisface gates, resuelve diferidos ni autoriza AWS. No se implementa autenticación: AUTH-027 permanece **NO IMPLEMENTADO / NO SATISFECHO**.

## 2. Estado inicial observado

La inspección por lectura permitió identificar:

- `Enver.Backend.slnx`, que referencia `Enver.API/Enver.API.csproj`.
- Un proyecto con SDK web y destino `net10.0`.
- `Program.cs` con configuración inicial de controladores, OpenAPI, redirección HTTPS y autorización. Esto no demuestra la existencia de políticas de acceso ni seguridad validada.
- Referencias existentes a `Microsoft.AspNetCore.OpenApi` 10.0.8 y `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` 1.23.0. Se registran como observación, no como aprobación definitiva de dependencias.
- Dockerfile, archivos de configuración y perfiles de ejecución de Visual Studio.
- Archivos locales y generados en `.vs` y `obj`; su presencia no acredita ejecuciones satisfactorias.
- No se encontraron documentos Markdown ni controladores implementados en la inspección inicial.

Los archivos de código, configuración y Visual Studio se conservaron sin cambios. No se verificó el comportamiento de la aplicación.

## 3. Decisiones y requisitos confirmados

| Elemento | Estado |
| --- | --- |
| C# | Lenguaje de la base tecnológica. |
| ASP.NET Core y .NET 10 | Plataforma confirmada. |
| NoSQL | Uso requerido por la rúbrica; MongoDB Atlas seleccionado posteriormente por el responsable, inicialmente para usuarios en 002; sin conexión implementada por esta entrega. |
| Redis | Seleccionado para caché distribuida, rate limiting y revocación JWT; integración y garantías operativas pendientes. |
| Cloud y entrega | AWS, GitHub Actions y Terraform seleccionados para incrementos posteriores; servicios, infraestructura y automatización no implementados por esta entrega. |
| Fase 0 | Alcance original: solo gobierno documental SDD; elaboración documental completada, sin autorización técnica derivada. |
| Constitución canónica | `.specify/memory/constitution.md`. |
| `constitution.md` de la raíz | Referencia a la constitución, sin duplicar su contenido. |

## 4. Requisitos docentes para fases futuras

El alcance académico comunicado incluye NoSQL, dos instancias en nube, balanceador, JWT, Redis distribuido, caché, paginación, índices, limitación de solicitudes (rate limiting), registro de eventos (logging), observabilidad y versionamiento.

Estos requisitos deberán concretarse y verificarse en fases autorizadas. No se registra ninguno como implementado o validado.

## 5. Propuestas

- API REST modular, con separación de responsabilidades y aplicación proporcionada de SOLID.
- Estrategia futura de DevSecOps y CI/CD con controles de calidad, seguridad y aprobación.

Las propuestas originales no establecieron una arquitectura de capas. La aprobación posterior del plan de 002 adopta un único Enver.API con separación lógica y tres proyectos futuros de pruebas; registro en §15. Las selecciones expresas no son adopciones por iniciativa del agente ni evidencia de implementación.

## 6. Decisiones pendientes

- Servicios concretos AWS, región, red, topología, TLS y presupuesto; proveedor ya seleccionado.
- Garantías, configuración y modelado detallado de MongoDB Atlas; motor ya seleccionado y contrato conceptual de usuarios definido en 002.
- Detalles de estructura de 002 dentro de los límites aprobados de un Enver.API con separación lógica; no proyectos adicionales de aplicación ni patrones excluidos.
- Versiones/grafo/licencias/vulnerabilidades de las librerías seleccionadas en el plan y dependencias complementarias que requieran autorización.
- Mecanismos técnicos de rotación HS256, configuración efectiva del hasher administrada por framework, actualización concurrente segura, BSON/read-write concerns, integridad/recuperación Redis y margen TTL, coordinación/pseudonimización de cuotas, presupuestos y observabilidad. Contratos, normalización, estados, selección obligatoria del hasher, modelo y cuotas quedan aprobados en §13 y 002; no se fijan iteraciones criptográficas manuales.
- Herramientas, entornos y criterios de pruebas, controles y CI/CD; plataforma GitHub Actions y herramienta IaC Terraform seleccionadas, no configuradas.
- Catálogo task.md creado documentalmente por SOL-TASK-002 (§16), con 79 tareas pendientes; implementación/verificaciones técnicas requieren autorización separada. AD-07 precisada por APR-03: pérdida detectada → cierre/recuperación; silenciosa no observable → riesgo residual de infraestructura, sin ledger. AD-09/AD-11/AD-12 diferidas y AD-10 parcial con detalle diferido, según spec §10, plan §23.2 y registro §15.

## 7. Registro documental de la Fase 0

Registro histórico de la intervención inicial; no es el inventario de las entregas posteriores. Se conserva su alcance y no se le atribuyen autorizaciones adicionales.

| Documento creado | Función |
| --- | --- |
| [`.specify/memory/constitution.md`](.specify/memory/constitution.md) | Reglas canónicas, alcance, principios y gobierno del proyecto. |
| [`constitution.md`](constitution.md) | Referencia a la fuente canónica. |
| [`AGENTS.md`](AGENTS.md) | Obligaciones, autorizaciones y límites de actuación de agentes. |
| [`memoria.md`](memoria.md) | Estado observado, decisiones, propuestas y pendientes. |
| [`README.md`](README.md) | Presentación y orientación documental. |

Este registro describe únicamente el trabajo documental autorizado. No se crearon `spec.md`, `plan.md`, `task.md`, subagentes ni skills. No se desarrollaron funcionalidades ni se ejecutaron compilaciones, pruebas, instalaciones, migraciones o despliegues como parte de esta intervención.

## 8. Condición de continuidad al cierre de la Fase 0 — registro histórico

El trabajo se detiene tras completar los cinco documentos. Cualquier nueva fase, implementación o tarea técnica requiere autorización explícita; los pendientes no constituyen una lista de ejecución autorizada.

## 9. Entregas documentales posteriores — historial conservado

Las solicitudes posteriores fueron independientes y delimitadas: crear perfiles no autorizó invocarlos; crear skills no autorizó ejecutarlas; crear la especificación no autorizó el plan ni las tareas; crear el plan no autorizó las tareas; crear `task.md` no autorizó ejecutar su catálogo. Se conserva el registro original de Fase 0 de los §§7–8.

| Entrega o intervención | Alcance y resultado documental | Aprobación y límites conservados |
| --- | --- | --- |
| Subagentes | 5 perfiles creados en [`.opencode/agents/`](.opencode/agents/): `sdd-reviewer`, `foundation-architect`, `security-reviewer`, `traceability-reviewer` y `documentation-keeper`. | Cuatro revisores de solo lectura; keeper limitado a cinco Markdown de gobierno con aprobación de edición. Su creación no autorizó invocación ni ampliación de permisos. |
| Skills | 5 definiciones creadas en [`.opencode/skills/`](.opencode/skills/): `sdd-governance`, `backend-architecture`, `devsecops-review`, `traceability` y `documentation-quality`. | Procedimientos documentales subordinados a la constitución; su creación no autorizó carga o ejecución ni cambios de fase. |
| Especificación de arquitectura base | [`spec.md`](specs/001-backend-foundation/spec.md), versión 0.1.0: 49 requisitos FND-001 a FND-049 y 17 pendientes DEC-01 a DEC-17. | Borrador pendiente de aprobación; FND-049 conserva el límite histórico de crear exclusivamente la especificación durante aquella intervención. |
| Plan de arquitectura base | [`plan.md`](specs/001-backend-foundation/plan.md), versión 0.1.0: 28 decisiones PLD-001 a PLD-028 y matriz de los 49 FND. | Borrador pendiente de aprobación; PLD-028 conserva el límite histórico de crear exclusivamente el plan durante aquella intervención. |
| Catálogo de tareas | [`task.md`](specs/001-backend-foundation/task.md), versión 0.1.0: 38 tareas TBF-001 a TBF-038, vinculadas a los 49 FND y las 28 PLD. | Las 38 mantienen PENDIENTE / NO EJECUTADA; borrador pendiente de aprobación, sin construcción ni verificación técnica realizadas por el catálogo. |
| Auditoría documental | Informe mostrado en la conversación, sin archivo de informe; dictamen APTO CON OBSERVACIONES exclusivamente documental. | No aprueba arquitectura, requisitos, decisiones o tareas ni acredita cumplimiento técnico de la rúbrica. |
| Solicitud de corrección A-01/A-02 — histórica | Autorización explícita del responsable para corregir únicamente A-01 y A-02 en los documentos indicados. | No autorizó reformar la constitución, modificar perfiles o skills, crear archivos, implementar ni ejecutar tareas. |

Al cierre de estas entregas documentales, la arquitectura base estaba documentada, pero no implementada por ellas, y no se habían elaborado especificaciones funcionales. Ese estado histórico no describe la integración técnica posterior ni la apertura de 002, registradas en §12. La aprobación documental y cualquier autorización técnica posterior siguen siendo actos separados.

## 10. Registro de la corrección de auditoría A-01 y A-02

- **A-01:** se sincronizan el estado, inventario y límites en `AGENTS.md`, `memoria.md` y `README.md`. Se distingue la elaboración completada de Fase 0 de la aprobación pendiente de la documentación de arquitectura base. El historial original no se sustituye ni se interpreta como autorización de entregas posteriores.
- **A-02:** se aclara FND-047 de `spec.md` y su resumen en `task.md`: RPO (Recovery Point Objective) es la máxima pérdida de datos tolerable expresada en tiempo entre el incidente y el punto consistente recuperable más reciente, no la antigüedad de los registros individuales. RPO ≤ 15 minutos permanece PROPUESTA, pendiente de aprobación y sin evidencia de cumplimiento.
- **Plan revisado:** `plan.md` conserva RPO ≤ 15 minutos como propuesta y remite a la especificación; no contiene la expresión ambigua sobre antigüedad de datos. Se mantiene sin modificaciones.
- **Trazabilidad conservada:** 49 FND, 28 PLD, 38 TBF y 17 DEC, con todos sus identificadores, relaciones y estados de aprobación de origen. DEC-01 permanece pendiente de aprobación de las fuentes; esta autorización de sincronización no equivale a resolverlo íntegramente. Ninguna TBF se declara ejecutada por esta corrección.
- **Límites:** solo se modifican `AGENTS.md`, `memoria.md`, `README.md`, `specs/001-backend-foundation/spec.md` y `specs/001-backend-foundation/task.md`. No se crean archivos ni se modifican la constitución canónica, su referencia raíz, agentes, skills, código o configuración. No se ejecutan tareas, instalaciones, compilaciones, pruebas, migraciones o despliegues.

## 11. Referencias históricas, pendientes y continuidad al cierre de A-01/A-02

La constitución canónica 1.0.0 conserva sus expresiones de alcance inicial de Fase 0, incluidos §§2.1 y 10; no se modifica ni se reforma implícitamente. Los preámbulos, riesgos y restricciones de las entregas originales de `spec.md`, `plan.md` y `task.md` también conservan el estado y las autorizaciones de aquellas intervenciones. Para conocer el estado actualizado se consulta este registro; aquellas menciones no acreditan una aprobación actual ni habilitan ejecución. Permanece la desalineación literal con las expresiones de estado inicial de la constitución; cualquier ajuste de esa fuente requeriría autorización independiente.

Al cierre de aquella corrección, los 17 DEC conservaron sus identificadores y condiciones pendientes, sin aprobar decisiones. Los otros hallazgos quedaron fuera: A-03, contraste con la rúbrica docente original no disponible en la revisión; A-04, acceso limitado de los perfiles a los nuevos artefactos. No se incorporó la rúbrica ni se ampliaron permisos. Las confirmaciones posteriores de §12 no reescriben estos documentos ni cierran íntegramente los DEC.

La condición de cierre de aquella entrega fue detenerse tras A-01/A-02, manteniendo los borradores y las 38 tareas PENDIENTE / NO EJECUTADA. La apertura de 002 que sigue responde a una autorización posterior explícita, no a una continuación automática.

## 12. Apertura formal de 002-authentication e integración parcial de foundation

### 12.1. Autorización y estado de partida

El responsable confirma expresamente la apertura documental de 002 y autoriza los cuatro archivos del §1. Prohíbe modificar ambas referencias constitucionales y los tres documentos de 001, crear plan/tareas, implementar C#, instalar paquetes o conectar MongoDB/Redis. No se invocan agentes ni se cargan skills.

| Elemento | Fuente y estado | Límite de la afirmación |
| --- | --- | --- |
| Integración inicial de foundation | Lectura Git: commit `dcf04a4`, PR #1 y merge `bef06b6` en `develop`; `feature/authentication` parte de ese merge y estaba limpia al iniciar esta entrega. | Integración parcial, no aprobación ni aceptación integral de 001. |
| Compilación .NET 10 | Confirmada por el responsable; lectura de `Enver.API.csproj` confirma `net10.0`. | No se ejecutó compilación ni se generó evidencia nueva en esta intervención. |
| `GET /health` | Responsable informa funcionamiento; `Program.cs` registra health checks y mapea `/health`. | Health check básico del host; sin comprobaciones explícitas MongoDB/Redis ni evidencia de readiness distribuida. |
| Corrección High de Microsoft.OpenApi | Responsable informa corrección; lectura del proyecto y assets observa versión 2.13.0. | No se ejecutó dependency scanning ni se certifica ausencia de vulnerabilidades; aviso e informe verificable quedan pendientes. |
| Protección de `main` y `develop` | Informada por el responsable. | No se consultó la configuración remota para verificarla. |
| Higiene Git | Lectura observa `.vs/`, `bin/`, `obj/` y `.csproj.user` versionados, sin `.gitignore`. | Hallazgo abierto; no se eliminan archivos ni se modifica el índice o historial Git. |

### 12.2. Decisiones confirmadas para elaborar el spec

- Una identidad ENVER con nombre completo, correo institucional `@uttt.edu.mx` y contraseña; email normalizado único. Teléfono no obligatorio; futura habilitación de repartidor sobre la misma cuenta, sin registro independiente.
- Rutas `POST /api/v1/auth/register`, `/login` y `/logout`; éxitos 201, 200 y 204 respectivamente. Errores 400, 401, 409 y 429 según contrato; fallos internos seguros.
- Backend ENVER como emisor JWT, `iss=enver-api`, `aud=enver-mobile`, HS256, duración inicial 15 minutos y skew máximo 30 segundos; claims mínimos `sub`, `jti`, `iss`, `aud`, `iat`, `exp`. Clave externa de alta entropía, nunca versionada.
- Preferencia aprobada por `ASP.NET Core PasswordHasher<TUser>`, sin criptografía personalizada. Contraseñas de 12 a 128 caracteres, sin truncamiento, con reglas centralizadas/configurables.
- Sesiones concurrentes permitidas; logout revoca solo el token presentado. Redis es estado de seguridad compartido, TTL hasta `exp + skew`, sin expulsión arbitraria de revocaciones vigentes; validación protegida fail-closed y riesgo de pérdida silenciosa explícito.
- MongoDB Atlas para colección conceptual `users`; sin contraseñas originales, JWT completos ni connection strings versionadas. Redis también para cuotas distribuidas y caché futura; credenciales y respuestas de autenticación no se consideran datos cacheables.
- Configuración inicial de rate limiting: registro aproximadamente 3 intentos/15 minutos/origen; login aproximadamente 5/minuto por combinación origen/identidad más defensa distribuida; logout con cuota superior pendiente de ratificación. Algoritmo, ventanas y tolerancia exactos por concretar.
- AWS, GitHub Actions y Terraform confirmados; HTTPS, observabilidad y al menos dos instancias detrás de balanceador son requisitos futuros, no despliegue acreditado. Las integraciones y demás capacidades pendientes se implementarán incrementalmente mediante specs/incrementos posteriores expresamente autorizados; esta entrega no las ejecuta.
- MFA, refresh tokens, recuperación/cambio de contraseña, OAuth externo y demás exclusiones del spec quedan fuera del alcance actual, no eliminados del producto por esta decisión.

### 12.3. Resultado documental y trazabilidad conservada

Se crea [specs/002-authentication/spec.md](specs/002-authentication/spec.md), versión 0.1.0, **BORRADOR / PENDIENTE DE APROBACIÓN**, con 48 AUTH y 12 pendientes AD. Se actualizan exclusivamente `AGENTS.md`, `memoria.md` y `README.md`. Se mantienen intactos constitución raíz, constitución canónica y los tres documentos de 001; no se generan plan ni tareas.

Las decisiones de esta solicitud concretan parcialmente DEC-01, DEC-03 a DEC-09, DEC-13 y DEC-14, solo en los aspectos explícitos y para el alcance de 002. No aprueban retrospectivamente los borradores, no cierran íntegramente los 17 DEC ni alteran los estados históricos de FND/PLD/TBF. La trazabilidad del spec distingue fuente aprobada, concreción propuesta, aceptación técnica pendiente y evidencia futura.

### 12.4. Discrepancias aclaradas y riesgos conservados

- La afirmación histórica de foundation no implementada se contextualiza con su integración técnica parcial posterior; no se reescriben 001 ni la constitución.
- Las antiguas selecciones pendientes se concretan por decisión expresa del responsable, no por reforma constitucional implícita. Servicios, garantías, versiones y controles restantes continúan abiertos.
- Las rutas preliminares `/api/auth/*` quedan sustituidas para 002 por las rutas explícitamente aprobadas `/api/v1/auth/*`; no existen alias implementados ni se presupone compatibilidad con consumidores inexistentes.
- Un correo con dominio UTTT no demuestra titularidad ni condición académica. El 409 genérico de registro reduce detalles, pero no elimina la inferencia de existencia frente al 201; riesgo residual explícito, sin prometer ausencia total de enumeración.
- Redis fail-closed ante indisponibilidad no detecta por sí solo pérdida silenciosa de revocaciones. El diseño de continuidad/recuperación es bloqueante para garantizar logout distribuido; no se selecciona infraestructura concreta.
- A-03 (rúbrica original ausente), A-04 (permisos de perfiles), higiene Git y evidencia verificable de la corrección de dependencia siguen abiertos. No se amplía el alcance para resolverlos.

El trabajo se detiene tras esta especificación y actualización documental. La aprobación del documento, la autorización para planificar y la autorización técnica posterior son actos separados. No se ejecutan pruebas, compilaciones, análisis técnicos, instalaciones, conexiones, migraciones ni despliegues.

## 13. Aprobación de 002-authentication para planificación — acto posterior

### 13.1. Autorización y nuevo estado

Tras una revisión crítica de solo lectura, el responsable aprueba expresamente AD-01 a AD-08 con precisiones y eleva el techo de registro por origen. Esta solicitud se identifica como **APR-02** en el spec; autoriza actualizar exclusivamente `specs/002-authentication/spec.md`, `AGENTS.md`, `memoria.md` y `README.md`, sin reformar constituciones, intervenir en 001 o crear archivos técnicos. No se invocan subagentes ni se cargan skills.

El spec pasa de v0.1.0, BORRADOR / PENDIENTE DE APROBACIÓN, a **v0.2.0, APROBADO PARA PLANIFICACIÓN**. Este estado permite generar `plan.md` en una entrega posterior, pero el responsable prohíbe generarlo en esta intervención. Tampoco se generan `task.md`, equivalentes ni implementación; el plan tendrá revisión y aprobación propias y no habilita automáticamente ejecución. Los §§7–12 se conservan como historial de sus respectivas entregas; sus preferencias, propuestas y restricciones se interpretan en su contexto temporal, no como estado vigente que contradiga este acto posterior.

### 13.2. Decisiones aprobadas y trazabilidad

| ID | Decisión aprobada por el responsable | AUTH actualizados / vinculados |
| --- | --- | --- |
| AD-01 | Registro `fullName/email/password` → `id/fullName/email` normalizado, sin login automático; login `email/password` → `accessToken/tokenType=Bearer/expiresIn=900`; logout Bearer sin cuerpo funcional. Nombre trim externo, conserva case/acentos/espacios internos, sin controles, ≤200 escalares; email trim/minúsculas invariantes, dominio exacto `uttt.edu.mx`, ASCII ≤254 bytes y local-part ≤64, sin eliminar puntos/`+`; contraseña registro 12–128 escalares/≤512 bytes UTF-8 sin transformar. Login no reaplica futuros mínimos de creación. JSON estricto UTF-8, cuerpo ≤16 KiB, profundidad ≤4, JWT ≤4 KiB; 400/413/415 según contrato. | 003–008, 010, 014, 018 |
| AD-02 | RFC 9457 con type/title/status/detail/code/traceId, errors solo validación por campo, sin instance inicial; application/problem+json y no-store. Códigos seguros 400/401/409/429/500/503; Retry-After cuando corresponda. Logout auténtico ya revocado aún vigente 204; expirado/falso/ausente 401; Redis incierto 503; nunca reducir TTL. | 008, 011, 023, 026, 031 |
| AD-03 | Estados `Active`/`Disabled`, inicial `Active`; Disabled mismo 401. Active no significa correo verificado. Cuenta ENVER separada del futuro perfil de repartidor; sin desactivación pública, administración o estados/suspensión de repartidor. | 010, 012, 014; contexto 005 |
| AD-04 | `PasswordHasher<TUser>` obligatorio, no preferencia; framework administra formato/salt/parámetros/versionado, sin iteraciones manuales. `SuccessRehashNeeded` y actualización segura sin sobrescritura concurrente; hash ficticio válido previo con mismo hasher para inexistente, no por solicitud ni sleeps/jitter principal; cuotas antes de coste cuando corresponda. No Identity completo por este uso. | 010, 011, 013, 039 |
| AD-05 | users solo `_id/fullName/normalizedEmail/passwordHash/status/createdAt/updatedAt`; UUIDv4 backend y público canónico string, índice único email, timestamps UTC iguales al crear. Login normal no cambia updatedAt, rehash persistido sí. 201 tras creación confirmada, no consulta previa como única garantía; sin campos extra no requeridos. | 009, 014, 015, 038, 039; contexto 004/013 |
| AD-06 | HS256, issuer enver-api, audience enver-mobile, 15 min y skew ≤30 s; sub UUID, jti 32 bytes aleatorios criptográficos Base64url sin padding, iat NumericDate UTC y exp=iat+900. Validaciones completas, nbf si existe, kid aprobado/desconocido rechazado sin fallback; claves independientes por ambiente y rotación coordinada con coexistencia limitada. | 016–021 |
| AD-07 | Redis estado de seguridad, clave conceptual `enver:{environment}:auth:revoked:{issuer}:{jti}`, sin JWT/PII innecesaria; retener hasta exp+skew+margen, sin disminuir TTL ni expulsar revocaciones/caché local negativa. Tras logout confirmado rechazo en cualquier instancia; fail-closed/503 y no admisión si pérdida/integridad incierta. Recuperación por restauración validada o cuarentena cubriendo vida restante con emisión coordinadamente detenida cuando proceda. Persistencia no demuestra garantía; mecanismo técnico al plan/infraestructura. | 020, 022–027, 038, 039; verificación 042 |
| AD-08 | Cuotas configurables distribuidas Redis: REGISTER email 3/15 min y origen **20/15 min**; LOGIN par origen+email 5/min, email 10/min, origen 60/min; LOGOUT cuenta 30/min y origen 300/min. Ventanas móviles, consumo coordinado, 429/Retry-After cuando corresponda, pseudonimización adecuada de email. Sin bloqueo permanente, Disabled o sanción; combinación de claves no elimina NAT. | 028–030 |

Los valores previos de §12.2 se conservan como origen histórico: el techo REGISTER de 3/15 min/origen se sustituye por **20/15 min/origen**, conservando 3/15 min/email como límite distinto. La preferencia previa del hasher pasa a selección obligatoria. Se conservan los 48 AUTH-001 a AUTH-048 sin renumeración; AUTH-001/047/048 se sincronizan con la aprobación y el límite documental, sin crear requisitos ni tareas nuevos.

### 13.3. Decisiones diferidas y puertas técnicas

- **AD-09 — DIFERIDA:** candidatos login p95/p99 ≤1.000/2.000 ms, registro ≤1.500/3.000 ms y logout ≤250/500 ms. No son SLA ni capacidades verificadas; calibración real en pruebas, sin reducir seguridad del hashing. Perfil, concurrencia, presupuestos y timeouts al plan. AUTH-038/040 actualizados; no bloquea aprobación documental.
- **AD-10 — APROBACIÓN PARCIAL / DETALLE DIFERIDO:** ratificadas prohibiciones de logs de contraseña/hash/JWT completo/Authorization/signing keys/connection strings/secretos/cuerpos sensibles/valores sensibles en errores y minimización PII. Local User Secrets/variables externas; producción futura gestor seguro AWS con mínimo privilegio. Servicio/operación y retención definitiva antes de despliegue productivo, sin fijar plazo CloudWatch. AUTH-032/034/036 actualizados.
- **AD-11 — DIFERIDA A INFRAESTRUCTURA:** AWS, Terraform, GitHub Actions/CI-CD, dos instancias, Load Balancer, HTTPS publicado, trust de proxies, observabilidad centralizada/CloudWatch, Redis administrado y construcción/publicación/despliegue Docker son requisitos futuros no implementados/verificados. AUTH-033/037/046 se conservan como requisitos futuros; sin recursos concretos seleccionados.
- **AD-12 — DIFERIDA A PLAN / DEVSECOPS:** build, unit/integration/security tests, SAST y dependency/secret/container scanning obligatorios futuros. Evidencia debe asociar commit, ejecución, herramienta, configuración, resultado y tratamiento; ningún gate satisfecho por documentación. Herramientas, severidades/excepciones, entornos y evidencias al plan/fase autorizada. AUTH-041–045/047 actualizados.
- **Detalles técnicos de AD-01 a AD-08:** serialización, BSON/driver/read-write concerns, índices físicos, configuración efectiva del framework, rehash concurrente, precisión temporal/rotación operativa, margen TTL/integridad/recuperación Redis, atomicidad/pseudonimización/proxies de cuotas. No reabren las políticas aprobadas, pero deben resolverse antes de construir/aceptar el componente afectado.

### 13.4. Límites, contradicciones y cierre

La aprobación documental no resuelve técnicamente la pérdida silenciosa Redis: AUTH-027 sigue exigiendo mecanismo demostrable y evidencia antes de afirmar revocación distribuida segura. 201/409 conserva inferencia residual de registro; el dominio no prueba titularidad; NAT sigue siendo restricción aun con el techo elevado; jti no impide por sí solo replay de un Bearer válido. Son límites/riesgos explícitos, no controles cumplidos.

No se reescribe historial, se aprueba retrospectivamente foundation ni se cierran FND/PLD/TBF/DEC. Las desalineaciones literales históricas ya registradas en §11 se conservan con su contexto; no se reforma constitución ni se interpreta como prohibición permanente de esta aprobación expresa. Rúbrica original, permisos de perfiles, higiene Git y evidencia OpenApi continúan pendientes.

La comprobación exclusivamente documental confirma 48 AUTH consecutivos con sus seis campos obligatorios, matriz 48/48, 12 AD, 21 AC, 14 AM y 43 enlaces locales existentes; rangos abreviados y referencias de secciones revisados sin errores. `git diff --check` no encuentra errores, solo advertencias LF→CRLF en los tres documentos de estado. La lectura del diff confirma constituciones, 001 y `Enver.API` sin cambios; 002 solo contiene `spec.md`. Esto no es evidencia técnica de cumplimiento de los AUTH.

Se actualizan exclusivamente los cuatro documentos autorizados. No se crean archivos de implementación, `plan.md`, `task.md` o equivalentes; no se instalan paquetes ni se ejecutan build, aplicación, pruebas, análisis técnicos, conexiones, migraciones o despliegues. La intervención termina con **APROBADO PARA PLANIFICACIÓN**, sin iniciar el plan ni trabajo adicional.

## 14. Higiene del repositorio — intervención separada autorizada

El responsable autoriza establecer `.gitignore` y dejar de versionar artefactos locales/build mediante retirada exclusiva del índice, sin borrar archivos locales, código fuente o documentación. No autoriza autenticación, plan/tareas, instalaciones, commit o push. Esta intervención no cambia el estado **APROBADO PARA PLANIFICACIÓN** de 002 ni ejecuta sus requisitos o los de foundation.

### 14.1. Auditoría previa y conservación del trabajo

- Estado inicial en `feature/authentication`: `AGENTS.md`, `README.md` y `memoria.md` modificados; `specs/002-authentication/spec.md` aún sin seguimiento; sin cambios preparados en el índice. Se conserva ese trabajo y no se añade automáticamente al índice.
- Artefactos ya versionados: 10 archivos en `.vs/`, 10 en `Enver.API/bin/`, 36 en `Enver.API/obj/` y `Enver.API/Enver.API.csproj.user`: **57 en total**, todos presentes localmente. No se encontraron otros artefactos versionados de IDE, TestResults, cobertura o temporales en la clasificación auditada.
- Configuración revisada: `Enver.API/appsettings.json` contiene solo Logging/AllowedHosts y `appsettings.Development.json` solo Logging; sus copias de build tienen el mismo tipo de contenido. Los dos originales siguen versionados y no se ignoran.
- No se encontraron archivos `.env`, secretos locales, claves/certificados ni estado/variables Terraform en el árbol inspeccionado. La inspección acotada de 48 archivos de texto, sin imprimir valores sensibles, no encontró coincidencias de credenciales literales en los patrones revisados. No es un secret scanning certificado, no revisa historial Git, binarios ni almacenes de secretos externos; no garantiza ausencia universal de secretos.

### 14.2. Política y retirada del índice

Se crea [`.gitignore`](.gitignore) para .NET/Visual Studio, estado local de VS Code/Rider, tests/cobertura/logs/temporales, secretos/configuración local, overrides Docker locales, estado local de runners/act y Terraform futuro. `.env.example` solo admite placeholders; `.terraform.lock.hcl`, configuración base, Dockerfile, `.dockerignore`, workflows y documentación SDD permanecen versionables. VS Code permite configuración compartida seleccionada, sujeta a revisión de secretos.

Se retiran **solo del índice Git** los 57 archivos auditados mediante `git rm -r --cached -- .vs Enver.API/bin Enver.API/obj Enver.API/Enver.API.csproj.user`, sin forzar ni borrar copias locales. Se comprueba que las 57 copias siguen presentes. `.gitignore` queda añadido al índice; no se prepara el trabajo documental previo ni se crea commit.

La política se comprueba mediante `git check-ignore --no-index` sobre 36 rutas ignorables y 22 rutas que deben permanecer versionables, incluidas `.env.example`, `.terraform.lock.hcl`, appsettings base, Dockerfile, `.dockerignore`, workflows, fuente y specs. Son comprobaciones de higiene Git, no pruebas de autenticación ni gates DevSecOps satisfechos. La salida final de Git mostrará las retiradas como eliminaciones preparadas para el próximo commit, aunque los archivos continúan en disco; ignorarlos no borra su historial.

Se modifica únicamente esta memoria además de crear `.gitignore` y actualizar el índice para los artefactos indicados. No se modifican README, AGENTS, ambas constituciones, 001, spec 002, código, configuración de aplicación o Docker. No se crean archivos de autenticación, `plan.md` o `task.md`, ni se ejecutan builds/pruebas/instalaciones/despliegues. Sin commit ni push; la intervención termina sin iniciar planificación.

## 15. Planificación 002 y resolución normativa Redis — APR-03

### 15.1. Antecedente y autorización posterior

SOL-PLAN-002 aprobó las 25 decisiones técnicas y autorizó crear exclusivamente plan 002 v0.1.0. Se documentó la contradicción entre AUTH-027 de spec v0.2.0 y PLD-AUTH-08: el criterio exigía preservar revocaciones ante pérdida silenciosa más allá de las garantías de la arquitectura Redis sin testigo duradero. El plan quedó **PENDIENTE DE REVISIÓN NORMATIVA DE AUTH-027 — NO HABILITADO PARA DESCOMPOSICIÓN EN TAREAS**, sin generar tareas ni código. La propuesta y evidencia de aquel bloqueo se conservan íntegros como historial en plan §23.1; no se borran por su resolución.

Tras revisión conversacional de solo lectura, el responsable aprueba el ajuste mínimo. **APR-03** identifica esta nueva autorización: modificar spec/plan y AGENTS/memoria/README solo para la sincronización necesaria. No autoriza implementación, task.md, paquetes, Program.cs, conexiones, workflows, Terraform, commit o push. No se invocan subagentes ni se cargan skills.

### 15.2. Cambios aprobados y garantía resultante

- Spec pasa a v0.3.0 conservando **APROBADO PARA PLANIFICACIÓN**. Solo cambian de contenido AUTH-022, AUTH-027 y AUTH-043. Los otros 45 requisitos, incluidos AUTH-023/024/025/026/038/039, permanecen íntegros. Se conservan los 48 IDs consecutivos, 21 AC, 14 AM y 12 AD.
- AUTH-022: tras 204 confirmado, rechazo en todas las instancias con continuidad confiable del estado compartido; pérdida/discontinuidad detectada, integridad incierta o estado no verificable → fail-closed/503, sin autorizar por marca ausente. Mantiene el límite de solicitudes ya autorizadas antes de revocar.
- AUTH-027: modelo de fallos/señales/límites, no admisión compartida, sin emisión/204 sin controles, recuperación validada o cuarentena suficiente desde última emisión posible considerando skew/margen/solicitudes en curso. Prevención de evicción, capacidad/monitoreo y pruebas detectables; pérdida catastrófica silenciosa no observable es residual, no eliminada. Aprobado documentalmente, **NO IMPLEMENTADO / NO SATISFECHO**.
- AUTH-043: probar pérdida detectable, reacción y recuperación; evaluar el riesgo no observable sin afirmar detección universal. Inyectar una señal solo demuestra reacción, no el detector.
- Se sincronizan AC-15, AD-07 (garantía/prevención/residual), §4.3 paso 6, §6.2 y AM-07. No cambian JWT, TTL, idempotencia, sesiones ni cuotas.
- Arquitectura inalterada: un Enver.API con separación lógica, tres proyectos futuros de pruebas y Redis compartido para revocaciones/cuotas. Sin MongoDB revocation ledger, segundo almacén duradero obligatorio o transacciones distribuidas; no se exigen dos Redis físicos.

El plan pasa a v0.2.0 **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**: PLD-AUTH-08 alineada, matrices de diseño 48/48 y resolución histórica/vigente diferenciadas. Se resuelve la contradicción normativa; la pérdida no observable permanece como riesgo técnico residual. La aceptación técnica aún exige evidencia de los controles definidos; ping, persistencia, marcador colocalizado o prueba finita no prueban ausencia universal.

### 15.3. Diferidos, continuidad y cierre

Permanecen diferidos: versiones/grafo/licencias/vulnerabilidades; detalles MongoDB/Redis, ACK, topología/detección/recuperación/capacidad; cardinalidad y rotación HMAC; calibración de presupuestos/margen/latencias; TLS/proxies/AWS/readiness; privacidad/retención; herramientas/gates/OWASP/rúbrica y evidencia OpenAPI; endurecimiento Docker/publish antes de un build sensible. Plan §23.2 conserva sus puertas. No bloquean esta aprobación documental ni quedan automáticamente aprobados/ejecutados.

La sincronización actualiza solo los cinco documentos autorizados, mantiene constituciones/001/código/configuración intactos y preserva §§7–14 como historial. La validación documental revisa IDs, campos, matrices directa/inversa, enlaces, diferencias de AUTH y estado; no es build, prueba de seguridad ni escaneo de dependencias. No se declara cumplida la rúbrica o completada foundation.

Resultado de la validación documental interna: 48 AUTH consecutivos con sus seis campos; solo 022/027/043 difieren frente a la línea base y los otros 45 permanecen íntegros. 25 PLD consecutivas, cobertura 48/48 y 188 relaciones directas/inversas coincidentes; 21 AC, 14 AM y 12 AD conservados. Los 66 enlaces locales de los cinco documentos existen; sin espacios finales ni errores en `git diff --check` (solo advertencias LF→CRLF). Los 25 archivos versionados fuera del alcance conservan el manifiesto SHA256 de partida; §§7–14 de memoria permanecen íntegros y no existe task.md de 002. No quedan contradicciones normativas vigentes identificadas; las desalineaciones literales históricas ya contextualizadas no reforman la constitución ni son bloqueos nuevos.

La aprobación del plan solo permite crear posteriormente `specs/002-authentication/task.md` en otra entrega delimitada; no autoriza implementar ni ejecutar tareas. La intervención termina sin task.md, código, paquetes, conexiones, workflows, cambios Terraform, commit o push.

## 16. Catálogo aprobado de tareas 002 — SOL-TASK-002

### 16.1. Autorización y alcance documental

El responsable aprueba expresamente el catálogo compactado de **79 tareas**, T-AUTH-001 a T-AUTH-079, y autoriza crear [specs/002-authentication/task.md](specs/002-authentication/task.md) y sincronizar exclusivamente AGENTS/memoria/README. SOL-TASK-002 identifica este acto conversacional posterior, no un commit ni evidencia de ejecución. Se conservan íntegros los antecedentes de §§7–15 y el historial APR-03; no se modifica spec v0.3.0, plan v0.2.0, constituciones o 001.

### 16.2. Resultado y significado del estado

Task v0.1.0 queda **APROBADO PARA EJECUCIÓN INCREMENTAL**, con las 79 tareas en **PENDIENTE / NO EJECUTADA**, fases F0–F15, matrices AUTH 48/48, PLD-AUTH 25/25 y AC 21/21; los 8 gates permanecen pendientes/no satisfechos. El documento reserva E(T-AUTH-nnn) sin resultados técnicos inventados; conserva dependencias, orden JWT → Logout → Login, paralelismo condicionado, ediciones serializadas, recomendaciones de commit y puertas de los diferidos.

El estado solo significa que el catálogo puede comenzar a ejecutarse una tarea o incremento a la vez **mediante autorización posterior** de alcance, archivos, acciones y entorno. No permite ejecutar todas las tareas inmediatamente, satisface gates, resuelve diferidos, autoriza AWS/Terraform/despliegues o acredita requisitos implementados. La asociación documental no demuestra cumplimiento.

**AUTH-027 continúa NO IMPLEMENTADO / NO SATISFECHO.** Detección, coordinación, recuperación y operación requieren evidencia futura; pérdida completamente silenciosa no observable sigue como riesgo residual, sin detección universal, ledger MongoDB, segundo almacén duradero obligatorio o transacciones distribuidas. Las puertas técnicas de AD-09/10/11/12 y demás detalles conservan su vigencia.

### 16.3. Validación documental y límite de entrega

La comprobación documental identifica 79 IDs consecutivos y fichas con campos obligatorios/estado/evidencia prevista, matrices 48/25/21 con referencias concordantes con las fichas, 8 gates y grafo de dependencias sin ciclos ni IDs inexistentes. Se revisan fases, enlaces y coherencia con spec/plan; no se genera evidencia de ejecución de las tareas. Las referencias principales de las matrices no pretenden enumerar todas las relaciones de cada ficha.

Se crea solo task.md y se modifican solo AGENTS/memoria/README para registrar su creación/estado y ausencia de implementación. No se implementa código, instala paquetes, ejecuta builds/pruebas/scanners, conecta MongoDB/Redis, modifica Program.cs, crea workflows o realiza commit/push. Finalizada la validación documental, se detiene el trabajo; la siguiente ejecución requiere nueva autorización.

## 17. Cierre documental posterior de T-AUTH-001

El responsable autoriza registrar el cierre de **T-AUTH-001 — Delimitar ejecución y baseline documental/Git**, ejecutada previamente mediante inspección de solo lectura y dictaminada LISTA PARA CIERRE DOCUMENTAL. Se identifica la baseline en `feature/authentication`, HEAD `d1d29298f86e25070f7bd1835acecd25d88d3e35` (`docs(auth): finalize authentication planning and tasks`), con working tree limpio al inicio/final de aquella inspección y coincidencia con la referencia local origin; sin nueva consulta al remoto.

Se registra [E(T-AUTH-001)](specs/002-authentication/task.md#e-t-auth-001) en task.md §10 y solo esa tarea pasa a **COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA**. T-AUTH-002 todavía no iniciada; T-AUTH-002 a T-AUTH-079 siguen **PENDIENTE / NO EJECUTADA**. Los ocho gates permanecen pendientes, incluido **GATE-A**, que depende de tareas posteriores. Las referencias anteriores a 79 tareas pendientes describen la baseline de creación SOL-TASK-002; este acto posterior cambia exclusivamente 001, sin reescribir ese historial ni cambiar las aprobaciones documentales globales.

Ninguna implementación realizada ni riesgo corregido: R-001 a R-008 se trasladan como pendientes en la evidencia. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**; el cierre no aporta evidencia técnica para satisfacerlo ni declara globalmente satisfechos AUTH-001/047/048. Esta entrega modifica solo task.md y añade este registro; sin código/configuración, paquetes, restore, build, pruebas, scanners, Docker, conexiones MongoDB/Redis, workflows, commit o push. Se detiene el trabajo sin iniciar T-AUTH-002.

## 18. Cierre documental posterior de T-AUTH-002

El responsable aprueba el dictamen LISTA PARA CIERRE DOCUMENTAL de **T-AUTH-002 — Evaluar SDK, dependencias y riesgos actuales**. La evaluación del 2026-11-04 inventarió el grafo NuGet actual en feature/authentication, HEAD `d1d29298f86e25070f7bd1835acecd25d88d3e35`, conservando los mismos cambios documentales preexistentes en memoria/task, sin nueva consulta al remoto Git. La consulta NuGet con --no-restore --vulnerable --include-transitive no reportó paquetes vulnerables dentro de su alcance; no certifica seguridad global. Se contrastó OpenAPI 2.13.0 con advisories oficiales y se identificó deuda de parches en SDK 10.0.300/runtime 10.0.8, sin instalar ni fijar versiones.

Se registra [E(T-AUTH-002)](specs/002-authentication/task.md#e-t-auth-002) en task.md §11 y únicamente 002 pasa a **COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA**; T-AUTH-001 conserva su cierre. T-AUTH-003 no iniciada; 003–079 continúan pendientes, al igual que los ocho gates, incluido **GATE-A**. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Este acto posterior no reescribe el historial, declara satisfechos globalmente los requisitos relacionados ni realiza implementación; modifica solo task.md y añade este registro, sin restore nuevo, build/pruebas, scanners adicionales, conexiones, Docker, workflows, commit o push. Se detiene el trabajo.

## 19. Ejecución y cierre técnico posterior de T-AUTH-003

El responsable autoriza exclusivamente **T-AUTH-003 — Fijar versiones y restore reproducible**, en modo BUILD, y el registro documental solo después de éxito técnico. Los intentos previos se detuvieron porque el SDK no aparecía; tras instalación manual informada por el responsable, la reanudación del 2026-11-04 confirma SDK **10.0.403**, MSBuild **18.9.15** y host **10.0.14**. El agente no instala el SDK. Se conserva feature/authentication y HEAD `d1d29298f86e25070f7bd1835acecd25d88d3e35`, sin nueva consulta al remoto y sin descartar los cambios documentales preexistentes.

Se crea global.json con versión exacta 10.0.403, allowPrerelease=false y rollForward=disable; en Enver.API.csproj se habilita RestorePackagesWithLockFile y solo Microsoft.AspNetCore.OpenApi cambia **10.0.8 → 10.0.14**. Microsoft.OpenApi **2.13.0** y Container Tools **1.23.0** se conservan. Restore genera Enver.API/packages.lock.json; grafo de tres paquetes autorizado, restore normal y locked correctos, lockfile estable y build net10.0 correcto con **0 advertencias / 0 errores**. La auditoría NuGet directa/transitiva con --no-restore no reporta paquetes vulnerables dentro de su alcance; no certifica toda la aplicación/plataforma. Outdated solo ofrece Microsoft.OpenApi 3.11.0, no adoptado. Riesgos posteriores en E(T-AUTH-003): R-001/002 RESUELTOS para el proyecto actual; R-003/004 REDUCIDOS / REVALIDADOS; R-005 REDUCIDO / COMPORTAMIENTO DE RESTORE DOCUMENTADO. Se conserva todo el historial previo y sus límites.

Tras validar técnicamente, se registra [E(T-AUTH-003)](specs/002-authentication/task.md#e-t-auth-003) en task §12 y únicamente 003 pasa a **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. Archivos intervenidos: global.json, Enver.API.csproj, packages.lock.json, task.md y este registro añadido a memoria. 001/002 conservan su cierre; **004–079 pendientes**, los **8 gates pendientes**, incluido **GATE-A**. Spec/plan, constituciones, 001, código de aplicación y configuración restante intactos. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin implementación de autenticación, tests, ejecución de API, conexiones MongoDB/Redis, Docker, workflows, AWS, commit o push. Trabajo detenido sin iniciar T-AUTH-004.

## 20. Cierre documental posterior de T-AUTH-004

El responsable aprueba LISTA PARA CIERRE DOCUMENTAL de **T-AUTH-004 — Verificar higiene Git y secretos versionados**, inspeccionada en solo lectura el 2026-11-04 en feature/authentication, HEAD `d1d29298f86e25070f7bd1835acecd25d88d3e35`. Se validó .gitignore con **44 comprobaciones check-ignore correctas (28/28 ignoradas y 16/16 versionables)**, se revisaron ambos appsettings de configuración base y se verificó únicamente **UserSecretsId CONFIGURADO**, sin abrir su almacén ni registrar su valor. No se detectaron candidatos de secretos literales dentro del alcance revisado; no es garantía universal. Se conservaron los cinco cambios preexistentes de csproj/memoria/task y los nuevos global.json/packages.lock.json, con estado Git idéntico antes/después de aquella inspección y sin nueva consulta al remoto.

Se registra [E(T-AUTH-004)](specs/002-authentication/task.md#e-t-auth-004) en task §13 y solo 004 pasa a **COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA**. R-008 queda **REDUCIDO — NO RESUELTO**, sin crear otro riesgo ni borrar antecedentes: blobs históricos, contenido de ignorados, binarios/artefactos locales, secretos ofuscados/fragmentados, User Secrets y almacenes externos no fueron auditados exhaustivamente; historial completo pendiente de DevSecOps/Gitleaks autorizado. Las rutas históricas de 57 artefactos retirados en 04ff903 no acreditan ausencia de secretos en sus contenidos.

Se añade únicamente este registro y se actualiza task.md, sin reescribir la historia anterior ni tocar código/configuración o repetir inspecciones/scanners. 001–003 conservan sus cierres; **005–079 (75 tareas) siguen pendientes**, al igual que los **8 gates**, incluido **GATE-A**. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin instalaciones, restore/build/tests, conexiones MongoDB/Redis, Docker, commit o push. Trabajo detenido; **T-AUTH-005 no iniciada**.

## 21. Ejecución y cierre técnico posterior de T-AUTH-005

El responsable autoriza reanudar exclusivamente **T-AUTH-005 — Proteger contexto Docker y publicación**, tras el diagnóstico previo **REQUIERE CORRECCIÓN**, el 2026-11-04 en feature/authentication, HEAD `d1d29298f86e25070f7bd1835acecd25d88d3e35`, sin nueva consulta al remoto. Se endurece .dockerignore conservando exclusiones útiles, retirando las cinco excepciones que reintroducían Git y cubriendo variantes sensibles/IDE/build/test/Terraform y documentación ajena al build. Dockerfile conserva runtime/multietapa/puertos/usuario/copia final, fija SDK **10.0.403**, copia global.json/csproj/lockfile antes del restore **--locked-mode** y añade **--no-restore** a build/publish. En Enver.API.csproj se añaden 14 patrones sensibles con CopyToOutputDirectory/CopyToPublishDirectory **Never** para Content/None, sin cambiar dependencias o configuración base.

La comprobación estática independiente da **142/142 exclusiones y 8/8 entradas disponibles**; no equivale al matcher/contexto real de Docker. La evaluación MSBuild con **42 archivos sintéticos vacíos** confirma **42/42 Never**, appsettings base PreserveNewest y limpieza completa antes de compilar. Dos builds locales Debug/net10.0 con **--no-restore** resultan correctos (**0 advertencias, 0 errores**, 10,29 s y 2,98 s), con **restore --locked-mode correcto** entre ambos. SDK 10.0.403; global.json y packages.lock.json conservan sus hashes. No se ejecuta Docker ni publish real, no se prueba Release/Linux o imagen y no se instalan paquetes.

Solo tras éxito técnico se registra [E(T-AUTH-005)](specs/002-authentication/task.md#e-t-auth-005) en task §14 y solo 005 pasa a **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. **R-006 REDUCIDO** por hardening y validación estática, sin contexto/imagen real comprobados; **R-007 REDUCIDO — NO RESUELTO**, pendiente de publish real con sentinelas controladas y autorización posterior. Se conservan los riesgos/evidencias anteriores y no se declara seguridad universal.

Archivos intervenidos: .dockerignore, Enver.API/Dockerfile, Enver.API/Enver.API.csproj y, después de validar, task.md y este registro añadido a memoria. Se conservan los cambios previos, sin reescribir historia ni modificar los demás archivos protegidos. 001–004 conservan sus cierres; **006–079 (74 tareas) siguen pendientes**, los **8 gates pendientes**, incluido **GATE-A**; **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin autenticación, ejecución de API, tests/scanners, conexiones MongoDB/Redis/AWS, Docker, publish real, workflows, commit o push. Trabajo detenido; **T-AUTH-006 no iniciada**.

## 22. Ownership y checkpoint Git autorizado de T-AUTH-006

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-006 — Consolidar estructura y checkpoint técnico** el 2026-11-04, en feature/authentication, HEAD previo `d1d29298f86e25070f7bd1835acecd25d88d3e35`, sin nueva consulta al remoto. La inspección había identificado solo dos pendientes reales: ownership explícito y baseline Git limpia. La aclaración posterior autoriza **un único commit** que incluya los cierres históricos válidos 001/002, **intactos**, junto con los cambios acumulados 003–006, exclusivamente en .dockerignore, Enver.API/Dockerfile, Enver.API/Enver.API.csproj, Enver.API/packages.lock.json, global.json, task.md y memoria.md. No se reescriben §§17–18 ni E001/E002, su trazabilidad o estados.

Se reutilizan las responsabilidades del plan y la edición serializada de task §6. La tabla de [E(T-AUTH-006)](specs/002-authentication/task.md#e-t-auth-006), task §15, fija ownership por roles de composición, baseline/grafo, estructura de solución/pruebas, aplicación de autenticación, transporte/contratos y DevSecOps; el **integrador técnico del backend** asume la revisión/consolidación final, ejercida en este checkpoint por el ejecutor autorizado bajo aprobación del responsable. Program conserva **056 → 057 → 058**; csproj/lockfiles/solución, futuros Service/Controller/Inputs/Results, tests/Shared y workflow requieren turnos serializados, revisión del diff y autorización propia de cada incremento. No se inventan personas ni se crea CODEOWNERS o componentes futuros.

La baseline física de un API net10.0 es suficiente; los tres proyectos bajo tests/ son futuros de 013–015. E001–E005 conservan sus evidencias y limitaciones: SDK 10.0.403, lockfile, restore/build previos y protecciones de higiene/contexto/publish, sin repetir comandos .NET, pruebas/scanners, Docker o publish. R-006 sigue **REDUCIDO**, R-007 y R-008 **REDUCIDO — NO RESUELTO**. Solo se actualiza task.md y se añade este registro; los otros archivos autorizados se incorporan al commit con sus cambios anteriores, sin nueva modificación técnica por 006.

E006 y el estado **COMPLETADA / EVIDENCIA TÉCNICA Y CHECKPOINT GIT REGISTRADOS** se preparan antes del commit para no ensuciar después el checkpoint: son efectivos **solo tras commit correcto y verificación posterior de working tree limpio**, según protocolo de task §15. El checkpoint se identifica por el commit que incorpora esta sección, con padre HEAD previo y mensaje `chore(auth): consolidate authentication baseline checkpoint`; su SHA y salida final de git status se comunican tras ejecutar git log/rev-parse, sin inventar el hash ni añadirlo recursivamente al propio commit. Si falla la verificación, no se declara 006 completada ni se ejecuta otro commit/amend automáticamente.

El cierre confirmado completa únicamente F0 — Baseline de 002: 001–005 conservan sus cierres y **007–079 (73 tareas) permanecen PENDIENTE / NO EJECUTADA**. Los **8 gates**, incluido **GATE-A**, siguen pendientes; **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin cambios a spec/plan, constituciones, foundation, Program/appsettings/workflows; sin autenticación, dependencias nuevas, proyectos/carpetas, instalaciones, conexiones MongoDB/Redis/AWS, Docker, publish real o push. Trabajo detenido sin iniciar T-AUTH-007.

## 23. Configuración tipada de T-AUTH-007 y preparación de T-AUTH-008

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-007 — Configurar Options de persistencia y Redis** y **T-AUTH-008 — Configurar y validar Options de autenticación**, el **2026-11-04**. Se confirma inicio limpio en feature/authentication, HEAD **a4e5cc2a5dc9a36e8b3f383af05389799d1aa559**, checkpoint F0; sin nueva consulta al remoto. No se amplía la autorización a Program.cs: si se necesitara modificarlo, habría que detener esa acción; aquí no se modifica ni se integra el host.

Se crean exactamente seis archivos bajo Enver.API/Configuration: MongoDbOptions.cs, RedisOptions.cs, JwtOptions.cs, AuthenticationOptions.cs, RateLimitingOptions.cs y AuthenticationOptionsValidation.cs. Se modifican ambos appsettings solo con configuración no sensible. MongoDB declara base/colección, timeouts y concerns **candidatos**, sin escoger topología/journaling/ACK/retries; Redis declara namespace/propósitos, timeouts, margen candidato de retención, fail-closed y requisito noeviction sin aplicarlos a infraestructura. Convención local explícita: base enver_<ambiente> con sufijo opcional por ejecución y namespace enver:<ambiente>, cotejados con el ambiente del host. No acredita aislamiento real por permisos ni disponibilidad/integridad.

JWT representa HS256/enver-api/enver-mobile/900 s/skew 0–30, kids locales únicos/activo y claves Base64 canónicas de al menos 32 bytes; no acredita entropía ni valida tokens. Authentication representa límites iniciales del spec y presupuestos/concurrencia candidatos del plan. RateLimiting conserva las siete cuotas exactas y configurables del spec; cardinalidad queda null, etiqueta academic-initial no es protocolo distribuido, clave HMAC externa distinta de JWT y sin inventar un tamaño criptográfico no fijado. Los secretos se omiten de ambos JSON: conexiones, material JWT y HMAC se suministrarán externamente; no se abren User Secrets reales, generan claves reales ni imprimen datos sensibles.

El único archivo de validación contiene cinco implementaciones IValidateOptions separadas por responsabilidad y un método de registro con secciones obligatorias y cinco ValidateOnStart; no se invoca desde Program. Configure/Bind no suscribe recarga automática. Errores fijos sin valores sensibles. Se preservan puertas de cardinalidad/HMAC, calibración AD-09, rotación/coordinación, TLS y topologías del plan; no se construyen controles funcionales.

**Verificación técnica previa al registro documental:** restore `dotnet restore Enver.API/Enver.API.csproj --locked-mode` correcto (899 ms); build `dotnet build Enver.API/Enver.API.csproj --no-restore` correcto (11,36 s), **0 advertencias / 0 errores**, SDK fijado 10.0.403/net10.0. Comprobación temporal contra la DLL construida, con compilador/referencias/runtime 10.0.14 instalados y configuración sintética en memoria: **71/71 casos** en DI aislada mediante IStartupValidator, sin iniciar API/host ni conectar dependencias. No es suite F2 ni prueba del startup real de Enver.API. Se corrigieron incidencias de la invocación temporal del compilador y se reemplazó Get-FileHash no disponible por SHA256 de .NET; no afectaron al build del proyecto. Los cuatro artefactos temporales se eliminaron, 4/4 ausentes. Inspección JSON confirma cinco secciones, cuotas 7/7, ausencia de conexiones/claves persistidas y cardinalidad null; hash del lockfile idéntico a F0. Casos y límites completos en task §§16–17.

Solo después del éxito técnico se registran [E(T-AUTH-007)](specs/002-authentication/task.md#e-t-auth-007) y [E(T-AUTH-008)](specs/002-authentication/task.md#e-t-auth-008). **007: COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, limitada a declarar configuración; **008: IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE**, no completada artificialmente. **PREPARADO / NO INTEGRADO EN HOST**: falta verificar no inicio/admisión real al integrar bajo 056 → 057 → 058; también suites F2 y regresión 060. No se afirma rechazo actual de configuración inválida por Enver.API.

001–006 conservan íntegros sus cierres/evidencias e historial. Estado actual: **7 completadas, 1 preparada no completada y 71 PENDIENTE / NO EJECUTADA (009–079)**; fichas posteriores intactas. **8 gates pendientes**, incluido **GATE-A**; AUTH-027 **NO IMPLEMENTADO / NO SATISFECHO**. Cobertura documental AUTH 48/48, PLD-AUTH 25/25, AC 21/21 no equivale a cumplimiento. R-006 REDUCIDO; R-007/008 REDUCIDO — NO RESUELTO, sin cambios. Spec/plan, constituciones, foundation, Program, csproj/lockfile/global.json/solución, Docker y workflows intactos. Sin dependencias nuevas, conexiones MongoDB/Redis/AWS, Register/Login/Logout/JWT funcional, rate limiting distribuido, endpoints, tests/, Docker/publish, commit o push. Trabajo detenido sin iniciar 009.

## 24. Ejecución aislable y secuencial de T-AUTH-009

El responsable autoriza el **2026-11-04** exclusivamente 009 → 010 → 011 → 012 en BUILD CONTROLADO, con verificación antes de avanzar. Baseline feature/authentication, HEAD **a4e5cc2a5dc9a36e8b3f383af05389799d1aa559**, diez archivos cambiados/no versionados válidos de 007/008 preservados; sin consulta al remoto. La preparación de consumidores no cierra 008 ni acredita startup real. Program.cs no se modifica.

Se crea `Enver.API/Http/RequestBudget.cs`: saldo monotónico mediante TimeProvider, cancelación enlazada cliente/total/dependencia, timeout acotado por saldo, sin reintentos/colas; causas diferenciadas y MarkDispatched/ResultUncertain sin inferir ausencia de efectos. Consume candidatos de Options, no SLA. Restore bloqueado correcto y build --no-restore correcto (**0 advertencias / 0 errores**, 11,27 s). Comprobación temporal determinista, sin sleeps ni host/API: **19/19 aserciones**, usando csc/referencias instaladas, sin NuGet/proyectos de pruebas nuevos. Solo después se registra [E(T-AUTH-009)](specs/002-authentication/task.md#e-t-auth-009), con límites y casos F2, y 009 pasa a **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA** dentro del alcance aislable. 008 conserva su estado; 010 aún no iniciada al registrar este punto de control. Sin conexiones, endpoints, dependencias nuevas ni commit/push; gates pendientes, AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. Se permite continuar secuencialmente con 010, no otras tareas.

## 25. Verificación aislada de T-AUTH-010

Tras verificar 009, se crean exclusivamente ApiProblemDetails.cs, ApiExceptionHandler.cs y RequestContextMiddleware.cs bajo Enver.API/Http. Contrato RFC 9457 cerrado, sin instance ni valores recibidos, errores por campos permitidos; ocho estados/códigos del plan, no-store, challenge seguro, Retry-After calculado y guardas contra segunda escritura. Correlación única de 32 hex minúsculas no cero o generada por servidor, sin confianza de identidad. Writer/servicio y handler nativos preparados, no integrados en Program. Cancelación del cliente no inventa respuesta; timeout/cancelación interna 503; inesperada 500 sin serializar excepción.

Build --no-restore correcto, **0 advertencias / 0 errores**, 6,26 s. **65/65 aserciones nuevas**, acumulado 84/84 con 009, mediante DI/HttpContext/streams controlados, sin iniciar host/API. Solo tras éxito se registra [E(T-AUTH-010)](specs/002-authentication/task.md#e-t-auth-010), con pruebas F2 y límites de servidor/MVC/challenge pendientes; 010 queda **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, exclusivamente como componente aislable. Program intacto, sin endpoints, infraestructura, dependencias nuevas ni commit/push. GATE-A y restantes pendientes; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. Se continúa únicamente con 011.

## 26. Frontera aislable de T-AUTH-011

Tras verificar 010, se crea AuthenticationRequestLimitsMiddleware.cs bajo Enver.API/Http, sin rutas/DTOs/endpoints ni cambios al host. Metadata de operación y resultados sin valores sensibles, lectura real acotada a 16385 bytes para límite de 16384, replay en memoria y limpieza del buffer; no confía solo en Content-Length. Usa presupuesto/token enlazado de 009. JSON UTF-8 estricto y configuración System.Text.Json; APIs nativas .NET 10 de rechazo de duplicados disponibles y comprobadas; nombres exactos, strings obligatorios, unknown/null/tipos/profundidad/comentarios/comas/Unicode inválido rechazados. Logout no vacío 400; Bearer único/ASCII compacto ≤4096 bytes solo desde Authorization, sin criptografía. Register/login no procesan Bearer opcional conforme al plan. Contrato de continuación permite a 012 coordinar 400 con controles asíncronos; fallback seguro si no hay coordinador.

Build --no-restore correcto, **0 advertencias / 0 errores**, 5,54 s. **149/149 aserciones nuevas**, acumulado 233/233 con 009/010. Probe de streams/HttpContext controlados, sin host/API ni tráfico de infraestructura; no prueba Kestrel/chunked de red/LB ni MVC real. Solo tras verificar se registra [E(T-AUTH-011)](specs/002-authentication/task.md#e-t-auth-011), con casos F2/056/058 pendientes; 011 queda **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA** dentro del alcance aislable. Sin conexiones, paquetes nuevos, Program, commit/push. 008 y gates mantienen sus pendientes, AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. Solo entonces se comienza 012.

## 27. Decisión aprobada y coordinación aislable de T-AUTH-012

Tras verificar 011 se identifica que spec/plan no fijaban una precedencia única. El agente presenta explícitamente la matriz mínima y el responsable responde **«Aprobar matriz propuesta»** el 2026-11-04: terminales de frontera primero; preflight asíncrono antes del 400; sin identidad válida/JSON ilegible no hay cuotas ni buckets arbitrarios; con identidad válida se evalúan conjuntamente los ámbitos una vez; indisponible/incierto 503, excedido 429 sin consumo, admitido consume y luego 400 por defectos o continuación. Un 503 incierto no prueba ausencia de consumo. **D012**, en task §21, es una decisión local expresamente aprobada para 012, no otro PLD ni modificación de spec/plan. No consulta existencia de usuario ni introduce cuota nueva o cambio de estado funcional.

Se crean AuthenticationAdmissionResults.cs bajo Features/Authentication/Validation y AuthenticationValidationCoordinator.cs/AuthenticationValidationFilter.cs bajo Http. Contratos propios sin valores sensibles, colaboradores obligatorios sin implementaciones reales ni fallback; cache Lazy<Task> por request e ID interno para evitar doble invocación/cargo, no idempotencia entre requests. Continuación de frontera y filtro asíncrono orden -3000 anterior al automático MVC (-2000), sin copiar valores/mensajes de ModelState; registro/JsonOptions preparados pero no aplicados al host. La identidad para cuotas requiere normalización compartida futura y cuenta autenticada para logout. No se adelantan normalizador, usuarios, cuotas Redis o servicios funcionales.

Build --no-restore correcto, **0 advertencias / 0 errores**, 4,44 s. **53/53 aserciones nuevas**, acumulado final **286/286** de 009 → 010 → 011 → 012, repetidas sobre el código final: matriz, async guard antes del 400, filtros MVC nativos invocados aisladamente, incertidumbre sin reintento, concurrencia/reutilización y registro/composición. No se inició host/API ni servidor, no son suites F2. Solo tras verificar se registra [E(T-AUTH-012)](specs/002-authentication/task.md#e-t-auth-012); estado **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, limitado a decisión aprobada y coordinación aislable. Integración/pruebas reales de MVC/cuotas quedan expresamente para F2/058 y regresión 060, sin cierre artificial del host o control distribuido. Los artefactos temporales deben eliminarse y comprobarse antes de entrega.

Resultado del bloque: **009–012 completadas como componentes aislables**, cada una con evidencia propia; 008 conserva **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE**. Total **11 completadas, 1 preparada y 67 pendientes/no ejecutadas (013–079)**. GATE-A y otros siete gates pendientes; faltan 013–015 y materialización F2. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO, riesgos R-006/007/008 y cobertura documental sin cambios. Historial previo preservado. Sin modificar Program, Options/appsettings válidos de 007/008, spec/plan/constituciones/foundation, paquetes/lockfile/global/solución o Docker/workflows; sin endpoints, conexiones MongoDB/Redis, JWT criptográfico, cuotas distribuidas, commit o push. Trabajo detenido sin iniciar 013.

Verificación final: **10/10 artefactos temporales eliminados** y directorio propio vacío retirado, ausencia comprobada. Inventario: ocho nuevos propios del bloque y seis Configuration previos, cuatro tracked modificados (dos JSON previos, task/memoria), sin staged. Comprobación UTF-8: 67/67 fichas posteriores intactas, definiciones oficiales sin redefinir y E001–E006 preservadas; archivos protegidos sin diff y lockfile con hash F0. Se mantiene el trabajo no versionado, sin descartes, commit o push. El diff/check y status finales se comunican en conversación.

## 28. Recuperación semántica de baseline F1 tras limpieza manual

Tras una ejecución manual posterior de `git clean -fd`, se observa que las fuentes **no versionadas** de F1 registradas en §§23–27 ya no existen, mientras las evidencias y estados documentales permanecen. También se observan cambios manuales posteriores en Program, el grafo de paquetes y lockfile que adelantaban MongoDB, Redis, JWT, servicios y autenticación fuera de 007–012. La recuperación autorizada devuelve Program al host F0 y retira exclusivamente esas dependencias funcionales; conserva configuración no sensible 007/008 y la evidencia histórica íntegra.

Se reconstruyen los catorce archivos registrados de Configuration, Http y Features/Authentication/Validation desde spec/plan/task y E007–E012. Es una reconstrucción **semánticamente equivalente**, no una afirmación de recuperación byte a byte. No crea endpoints, controllers, contratos, servicios, usuarios, repositorios, clientes, conexiones, JWT criptográfico, cuotas distribuidas, tests o proyectos F2; no integra componentes al host. Los comandos de restore/build y la revisión final determinarán si se mantienen los estados históricos; hasta entonces no se reescriben E001–E012 ni se inicia 013. Sin commit/push.

La validación posterior completa correctamente: `dotnet restore Enver.API/Enver.API.csproj` (2,94 s), restore bloqueado (706 ms) y `dotnet build Enver.API/Enver.API.csproj --no-restore` (14,04 s), con **0 advertencias / 0 errores**. El lockfile se regenera automáticamente al retirar el grafo manual y recupera los tres directos baseline (AspNetCore.OpenApi 10.0.14, OpenApi 2.13.0, Container Tools 1.23.0) con SHA-256 `6B6E8F6A214D7850EDB9BD00A19CA83FDF0321B27606CF929F6561A218562EBC`. Program es equivalente al F0 de HEAD; appsettings conserva cinco secciones no sensibles, sin conexiones, HMAC ni claves persistidas. Se comprueba existencia 14/14 de fuentes F1 y ausencia 14/14 de componentes manuales posteriores, ninguno versionado en HEAD. Los estados históricos de 007–012 se conservan; 008 sigue preparada sin integración de startup; 013 permanece pendiente. Sin Docker, conexiones, endpoints, dependencias nuevas, commit o push.

## 29. Ejecución y cierre técnico de T-AUTH-013 y T-AUTH-014

El responsable autoriza BUILD CONTROLADO exclusivamente para 013/014, tras aprobar Microsoft.NET.Test.Sdk 18.11.0, xunit 2.9.3, xunit.runner.visualstudio 4.0.1, Microsoft.AspNetCore.Mvc.Testing 10.0.14 y Testcontainers.MongoDb/Redis 4.16.0. Acepta expresamente xUnit 2.9.3 marcado Legacy, limitado a la coherencia con xUnit v2 requerido por el plan; no se migra de familia/major. No se autorizan paquetes adicionales, `Program.cs`, endpoints, host, Docker/Testcontainers, 015, commit o push.

Se crean exactamente tres proyectos net10.0 no empaquetables con lockfiles propios y `PrivateAssets=all`: UnitTests contiene los tres paquetes base; IntegrationTests y SecurityTests contienen además MVC Testing y los dos módulos Testcontainers. Los tres referencian Enver.API. `Enver.Backend.slnx` conserva API y añade los tres proyectos sin regenerarse. `tests/Shared/` se crea solo como directorio, sin `.csproj` ni fuentes vacías porque no hizo falta soporte compartido no-Docker. No se añaden coverlet, FluentAssertions, Moq, Microsoft.Extensions.TimeProvider.Testing, mocking, Testcontainers base directo ni transitivas forzadas.

Restore de solución genera los tres lockfiles; registra dos respuestas truncadas iniciales de NuGet al descargar Microsoft.CodeCoverage 18.11.0 y Docker.DotNet.Enhanced 4.3.3, reintentadas correctamente antes de completar el restore. Restore bloqueado correcto. El build final de solución es correcto con **0 advertencias / 0 errores** (11,26 s). Tras una corrección limitada a las pruebas nuevas por NullFileProvider y colisión local `Options`, UnitTests ejecuta 4/4, IntegrationTests 2/2 y SecurityTests 2/2, todos descubiertos por el adaptador xUnit VSTest 4.0.1. Cubren Options/kids/cuotas y presupuesto/cancelación/incertidumbre; Problem Details/no-store y consumo único D012; JSON duplicado y Authorization demasiado largo sin eco. Son componentes/control de composición en memoria: no arrancan host, endpoint productivo, MVC pipeline real, MongoDB, Redis ni Docker.

La auditoría autorizada `dotnet list Enver.Backend.slnx package --include-transitive --vulnerable --no-restore` no reporta paquetes vulnerables para API ni las tres suites en NuGet.org y caché SDK local. Transitivos relevantes incluyen TestHost/ObjectModel/CodeCoverage 18.11.0, xUnit core/assert/extensibility 2.9.3, ASP.NET TestHost/Hosting/DependencyModel 10.0.14 y Testcontainers 4.16.0 con Docker.DotNet.Enhanced/X509 4.3.3, SSH.NET 2026.0.0, SharpZipLib 1.4.2 y BouncyCastle.Cryptography 2.7.0. Esto depende de los avisos/feed actuales y no acredita ausencia universal de vulnerabilidades, seguridad funcional, imágenes o DLL embebidas; PrivateAssets tampoco sustituye comprobación de publish futura.

Tras las evidencias E013/E014 en task §22, **013 y 014 quedan COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA**. 008 mantiene **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE**. **015–079 permanecen PENDIENTE / NO EJECUTADA**, GATE-A y los otros siete gates siguen pendientes, y AUTH-027 permanece **NO IMPLEMENTADO / NO SATISFECHO**. No se ejecuta Docker/Testcontainers, crea factory/fixture de 015, conecta MongoDB/Redis, modifica Program, hace commit ni push.

## 30. Ejecución y cierre técnico de T-AUTH-015

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-015 — Preparar contenedores y factories de hosts**. La ejecución conserva `Program.cs`, producción, dependencias/lockfiles, imágenes/digests, solución, spec/plan y constituciones; no inicia 016, no hace commit/push y no ejecuta operaciones globales Redis/Docker.

La fixture de `tests/Shared/` inicia MongoDB y Redis Testcontainers con los digests autorizados, replica set `rs0`, usuario/secretos sintéticos y cleanup selectivo. El presupuesto global de inicialización se ajusta de 2 a **10 minutos finitos**: cubre pull/create/start, bootstrap/probes iniciales y dispone recursos parciales ante excepción; no cubre cleanup ni introduce cancelación infinita, sleeps o reintentos ilimitados. El primer pull/inicio real de Mongo excedió los 2 minutos originales (aprox. 2 min 30 s de suite) y concluyó dentro del máximo. Mongo confirma primary y escritura/lectura sintéticas; Redis confirma `PONG`, `noeviction`, `maxmemory=67108864` y `SET`/`GET`/`DEL` sobre claves propias.

La factory corrige el ambiente efectivo de `WebApplicationFactory` antes de construir el host: ambos hosts usan exactamente `T015_<RunId>`, con `enver_t015_<RunId>` y `enver:t015_<RunId>`. El namespace final usa guion bajo por coherencia con F1 (`enver:{environment.ToLowerInvariant()}`), sin cambiar el validador. IT acredita hosts A/B con dependencias externas compartidas y `IServiceProvider`, `TestServer` y estado local distintos; una escritura externa de A fue visible para B. Cada cleanup elimina solo la base/keys propias y comprueba ausencia; Testcontainers/Ryuk retira contenedores. No se ejecutan `FLUSHALL`, `FLUSHDB` ni `docker system prune`.

`dotnet restore Enver.Backend.slnx --locked-mode` y `dotnet build Enver.Backend.slnx --no-restore` son correctos; lockfiles sin cambios y build con **0 advertencias / 0 errores**. IntegrationTests: **4/4** correctas; SecurityTests: **8/8** correctas, incluido startup válido, `ValidateOnStart` real, cinco configuraciones inválidas rechazadas y mensajes comprobados sin secretos; UnitTests: **4/4** correctas. Docker Scout sigue no disponible sin autenticación: no se inició sesión y no se afirma ausencia de vulnerabilidades de imágenes.

Tras registrar [E(T-AUTH-015)](specs/002-authentication/task.md#e-t-auth-015), **T-AUTH-015 queda COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. T-AUTH-008 conserva **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE**. **GATE-A** y los otros siete gates permanecen pendientes/no satisfechos; **AUTH-027 permanece NO IMPLEMENTADO / NO SATISFECHO**. T-AUTH-016 no se inicia.

## 31. Evidencia complementaria de componentes para reevaluación de GATE-A

El responsable autoriza BUILD CONTROLADO limitado a completar las brechas mantenibles de evidencia de componentes de 008–012 y la inspección publish de 014, sin iniciar 016 ni modificar producción, `Program.cs`, paquetes, lockfiles, imágenes/digests, spec o plan. Tras una corrección únicamente en `AuthenticationBoundarySecurityTests`, los casos positivos/de frontera envían explícitamente `Authorization: Bearer <token>`; el helper genera un JWT-like sintético de tres segmentos Base64url sin padding y comprueba dentro de los casos de 4096/4097 la longitud solicitada y los tres segmentos. No se firma ni autentica un JWT y no se cambia `AuthenticationRequestLimitsMiddleware`: el fallo previo era del helper de prueba, que omitía el esquema Bearer y era rechazado antes de evaluar longitud.

Restore bloqueado de solución correcto, con los cuatro lockfiles sin cambios; build de solución correcto con **0 advertencias / 0 errores**. Las suites completas finalizan correctas: UnitTests **44/44**, IntegrationTests **29/29** y SecurityTests **32/32**, sin fallidas ni omitidas. Las pruebas materializan Options/aislamiento/JWT/cuotas/HMAC sintéticos de 008; presupuestos con `ManualTimeProvider` sin sleeps de 009; Problem Details/correlación/saneamiento/escritura de 010; fronteras de cuerpo/JSON/Unicode/HTTP/Authorization de 011; y D012 con colaboradores controlados, espera MVC, concurrencia/reentrada y no-consumo de cuota en casos no admisibles de 012. La evidencia no acredita Kestrel, TLS, ALB/LB, request smuggling, endpoints funcionales, criptografía JWT, clientes productivos o cuotas Redis reales.

Publish Release de `Enver.API` en un directorio temporal exclusivo resulta correcto. El contenido publicado se limita a la aplicación, configuración, runtime/deps/PDB/staticwebassets, `Microsoft.AspNetCore.OpenApi.dll`, `Microsoft.OpenApi.dll` y `web.config`; `Enver.API.deps.json` no contiene xunit, Microsoft.NET.Test.Sdk/TestPlatform, MVC Testing/TestHost, Testcontainers, Docker.DotNet, SSH.NET ni BouncyCastle. El directorio temporal se eliminó después de inspeccionarlo. La comprobación también confirma la errata de transcripción del digest MongoDB en task §23: la fixture efectiva usa `...3c64937121b9437095921c4bb2569911084c05f7b88e4a263d62a8959d2b2399`; se corrige solo esa evidencia documental, sin cambiar imagen, fixture ni digest efectivo.

**BRECHAS TÉCNICAS PARA REEVALUACIÓN DE GATE-A: COMPLETADAS.** Este registro no cierra el gate: **GATE-A permanece PENDIENTE PARA REEVALUACIÓN FORMAL**. T-AUTH-008 conserva **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE**; T-AUTH-009 a T-AUTH-015 conservan **COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA**; T-AUTH-016 permanece **PENDIENTE / NO EJECUTADA** y AUTH-027 **NO IMPLEMENTADO / NO SATISFECHO**. Sin stage, commit ni push.

La revisión posterior detecta que la aserción inicial de OnStarting podía quedar verde por el `finally` de `RequestContextMiddleware` tras retornar el delegado. Se corrige exclusivamente `ProblemDetailsTests`: `ControlledResponseFeature` recibe y cuenta el callback de `OnStarting`, lo ejecuta explícitamente y, antes de marcar `HasStarted`, observa que la correlación válida y `Cache-Control: no-store` fueron restaurados. El delegado aún no había retornado en esa observación, por lo que el fallback `finally` no puede ser la causa. La feature controla solo la respuesta de prueba; se añade `HttpRequestFeature` estándar para las cabeceras de solicitud. No se modifica middleware, `Program.cs` ni otro componente productivo.

Restore bloqueado y build de solución vuelven a ser correctos, con lockfiles sin cambios y **0 advertencias / 0 errores**. IntegrationTests finaliza **30/30**, UnitTests **44/44** y SecurityTests **32/32**, sin fallidas ni omitidas. **BRECHA ONSTARTING PARA GATE-A: COMPLETADA.** GATE-A sigue **PENDIENTE, A FALTA DE CONSOLIDACIÓN GIT Y CIERRE FORMAL**; T-AUTH-016 no se inicia, AUTH-027 permanece **NO IMPLEMENTADO / NO SATISFECHO**, y no hay stage, commit ni push.

## 32. Cierre formal de GATE-A — Base/configuración

El **5 de noviembre de 2026**, la reevaluación formal final declara **GATE-A — Base/configuración SATISFECHO / CERRADO**, con alcance limitado a baseline/configuración y soporte de pruebas F2. El checkpoint técnico consolidado es `1658c57478c8866ae5abe9aaabefe7fcba088508` (`test(auth): complete base configuration gate evidence`); el working tree estaba limpio al evaluar el gate. La evidencia asociada E(T-AUTH-001) a E(T-AUTH-015), incluida la complementaria de §31, acredita restore bloqueado correcto, build **0 advertencias / 0 errores**, UnitTests **44/44**, IntegrationTests **30/30** y SecurityTests **32/32**, sin fallidas ni omitidas. El publish inspeccionado de `Enver.API` no contenía dependencias exclusivas de testing, y la ejecución efectiva de OnStarting quedó acreditada sin depender del fallback `finally`.

Este cierre no implementa autenticación productiva, endpoints Register/Login/Logout, integraciones productivas Redis/MongoDB/JWT, AWS ni infraestructura cloud. T-AUTH-008 conserva **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE**; T-AUTH-016 no se inicia y permanece **PENDIENTE / NO EJECUTADA**. AUTH-027 continúa **NO IMPLEMENTADO / NO SATISFECHO** y R-007 **REDUCIDO — NO RESUELTO**, con su verificación integral reservada a 078 / GATE-H. GATE-B a GATE-H continúan pendientes/no satisfechos.

## 33. Ejecución y cierre técnico de T-AUTH-016

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-016 — Integrar conexión Redis y namespaces seguros**, con `StackExchange.Redis` **3.4.31** como única referencia directa nueva. Se conserva `Program.cs`, endpoints, MongoDB, JWT funcional, revocación/TTL, cuotas/HMAC y T-AUTH-017+ fuera de alcance; no hay stage, commit ni push.

`RedisConnections` implementa ownership de larga vida por destino lógico, inicialización async compartida y reutilización de un `IConnectionMultiplexer`, sin creación por request. Construye opciones desde los valores existentes de `RedisOptions`: `ConnectTimeout` desde `ConnectionTimeoutMilliseconds`, `SyncTimeout`/`AsyncTimeout` desde `OperationTimeoutMilliseconds`, `BacklogPolicy.FailFast`, `AbortOnConnectFail=false`, `ExponentialRetry` basado en el timeout de conexión e `IncludeDetailInExceptions=false`. Las señales expuestas son saneadas y la cancelación solo interrumpe la espera del caller; no se afirma cancelación remota, reintento ciego ni disponibilidad por conservar una instancia desconectada.

`RedisKeyBuilder` construye únicamente `enver:{environment}:auth:revoked:{issuer}:{jti}`, con ambiente canónico, issuer fijo `enver-api` y jti Base64url canónico de 32 bytes/43 caracteres. No persiste revocaciones ni usa JWT completo, PII o secretos. El grafo restaurado contiene StackExchange.Redis 3.4.31, RESPite 3.4.31 y System.IO.Hashing 10.0.13; los lockfiles de pruebas resuelven además las abstracciones Microsoft 10.0.13 necesarias, sin referencias directas redundantes. La auditoría NuGet directa/transitiva no informa paquetes vulnerables en sus fuentes actuales, sin constituir garantía universal.

Restore normal y bloqueado son correctos; build con **0 advertencias / 0 errores**. UnitTests finaliza **56/56**, IntegrationTests con Redis Testcontainer real **35/35** y SecurityTests **32/32**, sin fallidas ni omitidas. Las IT acreditan reutilización concurrente, operaciones sintéticas, aislamiento y limpieza selectiva de namespaces, fallo finito durante parada controlada y ausencia de resultados cruzados al disponer con lecturas en vuelo. Redis reiniciado volvió a estar sano según la fixture, pero el mismo multiplexer no reconectó dentro de la ventana acotada: no se añade recreación ni loop propio y la recuperación/continuidad/fail-closed integral sigue reservada a 017+.

Se registra [E(T-AUTH-016)](specs/002-authentication/task.md#e-t-auth-016): **T-AUTH-016 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. **GATE-B** sigue PENDIENTE / NO SATISFECHO y **AUTH-027** sigue NO IMPLEMENTADO / NO SATISFECHO. T-AUTH-017 no se inicia.

## 34. Ejecución y cierre técnico de T-AUTH-018

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-018 — Implementar admisión confiable y fail-closed**. Se crean el puerto `IAuthenticationSecurityState` y `RedisSecurityStateGuard`, sin modificar `Program.cs`, crear endpoints, implementar revocación/TTL, cuotas/HMAC, JWT funcional, MongoDB, detector real 019, recuperación 020 o T-AUTH-017. No se agregan paquetes y los cuatro lockfiles permanecen sin cambios; no hay stage, commit ni push.

El contrato devuelve una decisión mínima `Verifiable`, `Unavailable` o `Uncertain`, con causa de cancelación cuando aplica, sin exponer HTTP, tipos Redis o JWT. El guard reutiliza `RedisConnections` y `RequestBudget`/`DependencyBudget`; una sonda Redis efectiva y acotada sirve solo para disponibilidad puntual del guard, nunca como prueba de integridad/continuidad. Redis no verificable deniega; antes de `MarkDispatched`, timeout/cancelación mantiene resultado no incierto; después, timeout/cancelación/error Redis es incierto y no permite retry ni fallback. `ConnectionFailed`, `ErrorMessage` e `InternalError` cierran admisión; `ConnectionRestored` no la reabre.

Restore bloqueado correcto y build con **0 advertencias / 0 errores**. UnitTests finaliza **65/65**, IntegrationTests con Redis Testcontainer real **39/39** y SecurityTests **32/32**, sin fallidas ni omitidas. Las IT acreditan disponibilidad, caída con dos consumidores independientes, no reapertura tras reinicio, timeout de operación despachada y cancelación sin disposición de la conexión. La señal controlada acredita reacción, no detector real; detección, fuentes de confianza, coordinación y recuperación segura permanecen en 019/020.

Se registra [E(T-AUTH-018)](specs/002-authentication/task.md#e-t-auth-018): **T-AUTH-018 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. T-AUTH-017 y T-AUTH-019 siguen PENDIENTES / NO EJECUTADAS; **GATE-B** sigue PENDIENTE / NO SATISFECHO y **AUTH-027** sigue NO IMPLEMENTADO / NO SATISFECHO.

## 35. Ejecución y cierre técnico de T-AUTH-019

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-019 — Implementar detección según modelo de fallos aprobado**, limitado a señales Redis reales observables, cierre conservador, T0/T1/T2, propagación local y límites explícitos. Se crea `RedisSecurityStateDetector`, sin crear multiplexers/conexiones por request, polling, endpoints, health checks, paquetes, `Program.cs`, revocación/TTL, cuotas, JWT funcional, MongoDB, failover, flush, coordinación/reapertura 020 ni T-AUTH-017. No hay stage, commit ni push.

El detector se suscribe a las señales saneadas existentes de `RedisConnections`, registra T1 con reloj monotónico y las transmite al guard. El guard deja de suscribirse directamente para evitar duplicación y deja de usar `PING` como criterio de admisión. `ConnectionFailed`, `ErrorMessage` e `InternalError` cierran; `ConnectionRestored` únicamente representa conectividad recuperada y no reabre. Ni `IsConnected`, `PING` ni una operación puntual exitosa demuestran integridad o continuidad.

Las IT con Redis Testcontainer real acreditan para dos consumidores independientes: T0 antes de parada, `ConnectionFailed` real, T1 observado por detector, T2 de no admisión y latencias locales no negativas de detección/propagación. Tras `StartRedisAsync`, la fixture verifica Redis mediante sus recursos propios y la espera de `ConnectionRestored` es finita y opcional; no se reclama recuperación del mismo multiplexer, continuidad de datos ni reapertura. SecurityTests acreditan que la señal adversa mantiene cierre y que disponibilidad puntual externa no permite bypass. No se ejecutan flush, eliminación de estado, restauración antigua o failover; recuperación segura, cuarentena, coordinación y reapertura quedan diferidas a 020 y contrastes posteriores.

Restore bloqueado correcto y lockfiles sin cambios; build con **0 advertencias / 0 errores**. UnitTests finaliza **65/65**, IntegrationTests **39/39** y SecurityTests **33/33**, sin fallidas ni omitidas. Se registra [E(T-AUTH-019)](specs/002-authentication/task.md#e-t-auth-019): **T-AUTH-019 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. T-AUTH-017 y T-AUTH-020 siguen PENDIENTES / NO EJECUTADAS; **GATE-B** sigue PENDIENTE / NO SATISFECHO y **AUTH-027** sigue NO IMPLEMENTADO / NO SATISFECHO.

## 36. Ejecución y cierre técnico de T-AUTH-017

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-017 — Implementar revocación atómica y retención**, incluyendo el recurso Lua embebido mínimo en `Enver.API.csproj`, sin paquetes, `Program.cs`, endpoints/logout, JWT funcional, cuotas, `CheckRevocation.lua`, read-back, `WAIT`, recuperación/coordinación 020, stage, commit o push. Durante el build inicial se detecta que StackExchange.Redis 3.4.31 no contiene `CommandFlags.DemandPrimary`; la inspección de sus metadatos locales confirma `CommandFlags.DemandMaster`, documentado como operación solo en primary. La corrección aprobada sustituye exclusivamente las dos referencias de script y lectura; no cambia el diseño de revocación.

Se crean el puerto `ITokenRevocationStore` y requests mínimos de escritura/lectura, resultados sin HTTP/JWT/Redis con estados confirmados, no operativos, no disponibles o inciertos y causa de cancelación cuando aplica. `RedisTokenRevocationStore` usa `TimeProvider`, `IAuthenticationSecurityState`, `RequestBudget`/`DependencyBudget`, `RedisConnections` y `RedisKeyBuilder`; evalúa primero la admisión fail-closed, no comunica Redis cuando no es verificable y marca despacho justo antes de cada comando. Antes de despacho timeout/cancelación devuelve no disponible sin incertidumbre; después de despacho, timeout/cancelación/fallo sin ACK devuelve incierto sin reintento de aplicación.

`RevokeToken.lua` usa una operación Redis atómica y el valor fijo no sensible `"1"`: crea con `PXAT` si no existe, conserva expiración igual/mayor y extiende solo con `PEXPIREAT ... GT`; una marca preexistente sin expiración devuelve fallo seguro. La retención se calcula como `exp + skew efectivo + margen operativo`, se redondea conservadoramente hacia arriba a milisegundos y no envía Redis si ya no es positiva. Skew/margen negativos, fecha no UTC u overflow se tratan de forma no disponible. `ScriptEvaluateAsync` y `StringGetAsync` usan `DemandMaster`; no se usa `FireAndForget`, réplica, caché negativa, read-back, `WAIT`, `CheckRevocation.lua` ni retries/loops/Polly. El ACK semántico solo confirma ejecución dentro de presupuesto con retorno esperado y retención suficiente por la misma operación, no persistencia universal, ACK de réplica ni seguridad ante failover.

Las UT nuevas, sin Moq, cubren TTL/redondeo, no operación, entradas/rangos inválidos, admisión cerrada/incierta sin Redis, fronteras de despacho e incertidumbre, ausencia de retry, clave e inputs mínimos y mapeo de lectura. Las IT con Redis Testcontainer ejecutan el Lua real: primera marca, lectura cruzada por dos consumidores, valor/TTL, idempotencia, no reducción/extensión, concurrencia con expiraciones distintas, namespace aislado, estado cerrado, parada Redis y timeout real despachado. Cada prueba elimina solo sus propias claves; no se emplean `FLUSHALL`, `FLUSHDB` ni `docker system prune`.

Restore bloqueado correcto y lockfiles sin cambios; build con **0 advertencias / 0 errores**. UnitTests finaliza **76/76**, IntegrationTests **46/46** y SecurityTests **33/33**, sin fallidas ni omitidas. Se registra [E(T-AUTH-017)](specs/002-authentication/task.md#e-t-auth-017): **T-AUTH-017 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. **T-AUTH-020** permanece PENDIENTE / NO EJECUTADA; **GATE-B** permanece PENDIENTE / NO SATISFECHO; **AUTH-027** permanece **NO IMPLEMENTADO / NO SATISFECHO**. Esto no acredita continuidad, persistencia, failover, integridad, recuperación, cuarentena ni coordinación distribuida.

## 37. Implementación local parcial de T-AUTH-020-A

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-020-A — coordinador local y contrato de autoridad**. Se crean el puerto abstracto `IAuthenticationSecurityAuthority`, modelos sin dependencias AWS/DynamoDB/Redis/HTTP/JWT/PII, `AuthenticationSecurityCoordinator` y fake thread-safe compartido para pruebas. `IAuthenticationSecurityState` permanece intacta; `Program.cs`, endpoints, login/JWT, Terraform, AWS/IAM, DynamoDB Local/real, paquetes, lockfiles y T-AUTH-021 quedan fuera de alcance.

El coordinador mantiene latch local fail-closed, exige lectura verificable de la autoridad para admitir o iniciar una emisión potencial, y cierra antes de intentar propagar el cierre. Ausencia, indisponibilidad, incertidumbre o estado global `Closed` niegan admisión. No se reabre por conectividad Redis, `PING`, reconexión ni Redis vacío. El cierre calcula el máximo conservador local y deriva cuarentena desde lifetime, skew y margen efectivos; el fake modela `Generation`/`Revision`, CAS y un solo reread/reintento ante conflicto. La reapertura es una operación de control explícita tras cuarentena y sin emisiones locales en vuelo; restauración confiable validada se mantiene diferida.

La persistencia atómica distribuida de cada emisión potencial no se afirma en 020-A porque el puerto autorizado no añade esa mutación y el adapter DynamoDB no forma parte de este subpaso. La evidencia solo acredita lógica local con fake; no demuestra semántica DynamoDB, lecturas fuertemente consistentes, condiciones de escritura reales, SDK, AWS/IAM, bootstrap, failover ni continuidad distribuida.

Restore bloqueado correcto y lockfiles sin cambios; build con **0 advertencias / 0 errores**. UnitTests finaliza **88/88**, IntegrationTests **48/48** y SecurityTests **37/37**, sin fallidas ni omitidas. Las IT usan Redis Testcontainer real con dos coordinadores y fake compartido: A cierra, B observa `Closed`; parada/arranque y clave Redis ausente no abren; después de cuarentena, la operación explícita CAS permite que B observe `Open`. Las ST cubren ping, reinicio/Redis vacío, autoridad ausente/no disponible, generación atrasada, cuarentena incompleta y ausencia de revocación no permisiva. Se registra [E(T-AUTH-020-A)](specs/002-authentication/task.md#e-t-auth-020-a): **T-AUTH-020-A COMPLETADA / EVIDENCIA LOCAL REGISTRADA**; **T-AUTH-020 IMPLEMENTACIÓN PARCIAL / ADAPTER DYNAMODB PENDIENTE**. **GATE-B** permanece **PENDIENTE / NO SATISFECHO** y **AUTH-027** permanece **NO IMPLEMENTADO / NO SATISFECHO**. Sin stage, commit ni push.

## 38. Cierre técnico de T-AUTH-020-B con DynamoDB Local

El responsable autoriza BUILD CONTROLADO exclusivamente para **T-AUTH-020-B — adapter DynamoDB, registro compartido de emisión potencial y cierre de T-AUTH-020**. Se añade a `Enver.API` únicamente `AWSSDK.DynamoDBv2` 4.0.110.1; el restore resuelve `AWSSDK.Core` 4.0.102.18 dentro del rango aprobado y la auditoría disponible no reporta vulnerabilidades. Los lockfiles de los tres proyectos de pruebas cambian solo por la proyección transitiva de estas dependencias desde su `ProjectReference`, sin paquetes directos nuevos de test. El restore bloqueado es correcto.

El puerto añade `RegisterPotentialIssuanceAsync` con CAS de generación/revisión. El coordinador calcula conservadoramente `TimeProvider.GetUtcNow() + RequestBudget.Remaining`, confirma el registro compartido antes de crear una lease y admite un único reread/recomputación/segundo intento tras conflicto. Ausencia, estado cerrado, segundo conflicto, indisponibilidad o incertidumbre no habilitan emisión; completar/fallar libera solo la lease local y nunca rebaja el máximo. La autoridad DynamoDB usa `GetItemAsync` fuertemente consistente, `UpdateItem` condicional, bootstrap explícito y mapping validado; los errores conservan fail-closed, y `MaxErrorRetry=0` se usa en el cliente local.

La fixture existente se amplía con Testcontainers genérico para `amazon/dynamodb-local:3.1.0@sha256:7ef4a2c45b58c2901e70a4f28e0953a422c2c631baaaf5e2c15e0805740c7752`, puerto 8000 publicado dinámicamente, `ListTablesAsync` acotado, tabla aislada y credenciales sintéticas no reales. Las IT acreditan adapter real, lectura consistente inmediata, bootstrap, monotonicidad, Generation/Revision, cierre/reapertura, dos clientes y carreras register/cierre. La integración real de dos coordinadores comparte DynamoDB Local y Redis Testcontainer: registro de A, cierre, observación de B, parada/arranque Redis y estado vacío sin bypass, y reapertura explícita después de cuarentena. ST añade ausencia/no disponibilidad de autoridad DynamoDB y register conflict/uncertain sin emisión.

Restore bloqueado correcto; build **0 advertencias / 0 errores**. UnitTests **89/89**, IntegrationTests **52/52** y SecurityTests **39/39**, sin fallidas ni omitidas. Se registra [E(T-AUTH-020)](specs/002-authentication/task.md#e-t-auth-020): **T-AUTH-020-A COMPLETADA / EVIDENCIA LOCAL REGISTRADA; T-AUTH-020-B COMPLETADA / EVIDENCIA DYNAMODB LOCAL REGISTRADA; T-AUTH-020 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. Esto no acredita AWS real, IAM, Terraform, disponibilidad regional, continuidad/failover ni elimina el riesgo de pérdida silenciosa catastrófica no observable. **GATE-B permanece PENDIENTE / NO SATISFECHO; AUTH-027 permanece NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** `Program.cs` y Terraform permanecen intactos; no se hicieron stage, commit ni push y no se incluyen secretos reales.

## 39. Cierre de contrato aislado de T-AUTH-052

Registro de cierre del **10 de noviembre de 2026**. El responsable aprueba la decisión técnica pre-BUILD y autoriza exclusivamente **T-AUTH-052 — contrato de cuotas, PolicyRevision y RetryAfter**. Baseline limpia en `feature/authentication`, HEAD `f9f455191e3d14a691403c13f459ba952b9eb770`, sin consulta al remoto. La ejecución se detuvo inicialmente antes de modificar archivos por la contradicción entre prohibición de Docker y regresión IT/ST completa; el responsable autoriza después la excepción **solo para la regresión existente**, sin nuevos IT/ST ni modificaciones de fixtures.

Se crean `IAuthenticationQuotaStore` como puerto abstracto sin implementación/DI, cuatro archivos en `Enver.API/Features/Authentication/Application/Quotas/` y dos archivos UT. Los modelos inmutables representan operación, ID interno, revisión, ámbitos cerrados/identificador opaco y definición positiva; sin transporte HTTP, tipos Redis, PII raw o Cost configurable. Se reutilizan los enums existentes de operación y causa de presupuesto. Resultado con **Admitted/Rejected/Unavailable/Uncertain**, RetryAfter positivo exclusivo de Rejected y ResultUncertain derivado; antes de despacho sin resultado es Unavailable y después sin resultado semántico es Uncertain. El contrato HTTP existente permanece intacto; una UT con adapter solo de prueba acredita compatibilidad 503 para ambos resultados no verificables, no un adapter productivo.

El descriptor `academic-initial` conserva las siete cuotas exactas: REGISTER 3/email y 20/origen por 15 minutos; LOGIN 5/par, 10/email y 60/origen por minuto; LOGOUT 30/cuenta y 300/origen por minuto. La validación contra descriptor activo devuelve Unavailable ante revisión vacía/desconocida/inactiva, definición incompatible o topología no apta, sin despacho/consumo/fallback. PolicyRevision **no integra la identidad conceptual de partición ni crea otro namespace**; no se fija una key física ni el formato HMAC de 053.

El contrato de transición conserva descriptor/estado anterior y exige preparar, cerrar admisiones globalmente, resolver operaciones en vuelo, establecer último consumo posible, drenar, activar coordinadamente y reabrir solo con continuidad verificable. **DrainWindow = max(ventanas anteriores y nuevas)**, derivado de descriptores; no se implementa coordinación runtime ni se acepta esperar mientras sigue entrando consumo. Solo una revisión admite, sin arrays PreviousPolicyRevision(s) por request ni lectura dual; la identidad estable y los pasos se prueban sintéticamente, no como migración Redis real.

El contrato de atomicidad exige una sola ejecución Lua capaz de operar todas las claves en el destino efectivo, sin seleccionar Cluster/hash slot universal/clave agregada/cross-slot. Describe un evento por ámbito con el mismo InternalAttemptId al admitir y cero al rechazar; no aplica efectos ni ofrece rollback general, compensación o exactly-once. Un error tras escritura puede dejar efectos desconocidos/parciales y debe mantenerse incierto, sin retry ciego ni fallback local. El descriptor del script futuro ordena validaciones antes de mutar, **Redis TIME una vez** y now compartido; no se ejecuta TIME ni existe Lua de cuotas en 052. El cálculo puro implementa ventana **(now - Window, now]**, evento relevante **n - limit + 1**, máximo bloqueante y redondeo superior a segundos en TimeSpan, con controles de eventos futuros/overflow; no es un limitador local presentado como distribuido.

Restore bloqueado correcto y cuatro lockfiles sin cambios de contenido/SHA-256. Build final correcto con **0 advertencias / 0 errores**; una advertencia inicial xUnit2029 se corrigió exclusivamente en una aserción UT nueva. **70 UT nuevas**, sin Moq; suite UnitTests **159/159**, IntegrationTests **52/52**, SecurityTests **39/39**, sin fallidas ni omitidas. IT/ST fueron solo regresión existente: Docker/Testcontainers se usó únicamente para su fixture preexistente MongoDB/Redis/DynamoDB Local; no acredita implementación ni atomicidad Redis/Lua de 052.

Solo tras todas las verificaciones verdes se registra [E(T-AUTH-052)](specs/002-authentication/task.md#e-t-auth-052) en task §32 y se añade este registro. **T-AUTH-052 COMPLETADA / EVIDENCIA DE CONTRATO REGISTRADA; T-AUTH-053 y T-AUTH-054 PENDIENTES / NO EJECUTADAS; T-AUTH-021 BLOQUEADA POR 053 Y 054 / NO EJECUTADA. GATE-B PENDIENTE / NO SATISFECHO; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** Program.cs, csproj/lockfiles/solución/configuración, fixtures, IT/ST, spec/plan, constituciones, foundation, AGENTS y README intactos. Sin paquetes nuevos, store Redis/Lua ejecutable de cuotas, HMAC/origen, MaxActivePartitions, implementación de 021/053/054, AWS, stage, commit o push. Se conserva el historial y se detiene el trabajo.

## 40. Pseudonimización y origen confiable aislables de T-AUTH-053

El **10 de noviembre de 2026**, el responsable aprueba la decisión técnica pre-BUILD y autoriza exclusivamente T-AUTH-053: componentes locales, validación de configuración, contrato de rotación y UT/ST focales sin Docker. Baseline limpia en `feature/authentication`, HEAD `6ef97b54b24416505dec2babf7bc205d02749990`, sin consulta al remoto. Dependencias formales 008/012/016: se conserva 008 preparada con startup productivo pendiente, no se declara completada por consumir sus Options ni por validar configuración sintética.

Se crean `IAuthenticationScopePseudonymizer`, `AuthenticationScopeIdentity` con representaciones tipadas de email/origen/cuenta, `HmacAuthenticationScopePseudonymizer`, `AuthenticationOriginProvider` con modelos de confianza/resultado y `AuthenticationPseudonymizationTransition`. Email aplica trim externo, ASCII y minúsculas invariantes conservando puntos/+; rechaza representaciones incompatibles pero exige validación institucional previa por la capa anterior, sin implementar 028 completo ni endpoints. Origen captura bytes IPv4/IPv6 con prefijo de familia y hex minúsculo, convierte IPv4-mapped a IPv4 y rechaza ScopeId no cero sin agrupación IPv6. Cuenta requiere principal previamente autenticado/acreditado, una identidad autenticada y un único sub UUID D no vacío; no lee body/query/header, no valida JWT y no afirma que IsAuthenticated por sí solo pruebe firma.

El HMAC usa SHA-256 y digest completo de 32 bytes, salida Base64url sin padding de **43 caracteres**. Composición inequívoca LP con longitud UTF-8 uint32 big-endian y bytes estrictos sin BOM para contexto `enver.auth.quota`, versión `1`, ambiente confiable del host, operación, ámbito y componentes; en par, origen antes de email. Reutiliza AuthenticationQuotaScopeType y separa dominios de email/origin/origin-email/account. No incluye PolicyRevision, InternalAttemptId, tiempo, proceso ni RedisKey. La clave/ambiente se capturan en snapshot sin recarga dinámica; misma entrada/contexto/clave produce el mismo OpaqueId entre instancias. La salida alimenta los ámbitos de 052 sin datos raw/clave/HttpContext para 021.

Se endurece AuthenticationOptionsValidation para clave HMAC Base64 estándar canónica de **al menos 32 bytes**, sin trim ni Base64url; comparación de material JWT decodificado mediante FixedTimeEquals, sin valores sensibles en errores. El componente también rechaza configuración inválida con Options.Create, sin clave alternativa. No se agregan opciones, paquetes o DI productivo ni se integra ValidateOnStart en Program. Los componentes no registran claves/email/IP/cuenta/input HMAC; ToString no imprime sus valores. Se limpian buffers y clave al disponer sin prometer borrado de strings administrados; pseudonimización no equivale a anonimización absoluta y longitud no acredita entropía.

El origen requiere feature de confianza explícitamente creada por código del servidor: directo aprobado usa RemoteIpAddress; proxy requerido usa solo origen resuelto acreditado. Sin confianza/dirección o con zona incompatible devuelve Unavailable; no lee/parsea headers de forwarding ni usa unknown/random o peer del proxy como fallback. El productor de esa confianza, validación real de cadena de proxies y forwarding productivo permanecen pendientes de 056/infraestructura; las ST inyectan contexto sintético y no los acreditan.

El descriptor de rotación modela drenaje coordinado para clave o cambios de canonicalización/dominio/versión, sin coordinador runtime. Deriva **max(oldPolicy.MaximumWindow,newPolicy.MaximumWindow)** y admite el mismo descriptor/PolicyRevision cuando solo cambia HMAC. Exige detener admisiones globalmente, resolver in-flight, acreditar último consumo posible, preservar estado/configuración anterior, drenar y activar/reabrir coordinadamente con continuidad verificable. El método puro de horizonte no autoriza activación ni acredita cese global; no hay dual-read/consume ni reset. No se modifica el contrato 052 ni se adelantan cardinalidad/capacidad/recuperación 054 o Redis/Lua/TIME/TTL/claves físicas/topología 021.

Restore bloqueado correcto y cuatro lockfiles con SHA-256 idénticos a baseline; build final **0 advertencias / 0 errores**. **54 UT nuevas**, suite completa **213/213**; **16 ST focales nuevas, 16/16**, sin fallidas ni omitidas. Primera ejecución UT: tres casos nuevos de IStartupValidator fallaron por falta de sección sintética AuthenticationSecurityAuthority; se corrigió únicamente su preparación local, sin modificar fixture global, y se repitieron build/UT con éxito antes de ST. Se inspeccionó que Testcontainers se inicia por colección, no globalmente; la clase ST nueva no usa esa colección/fixture y se ejecutó solo mediante filtro FullyQualifiedName. **Sin Docker/Testcontainers ni ejecución de IntegrationTests o suite ST completa**; esta evidencia no acredita Redis ni forwarding real.

Tras verde se registra [E(T-AUTH-053)](specs/002-authentication/task.md#e-t-auth-053) en task §33 y se añade únicamente este historial documental. **T-AUTH-053 COMPLETADA / EVIDENCIA AISLABLE REGISTRADA; T-AUTH-008 IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; T-AUTH-054 PENDIENTE / NO EJECUTADA; T-AUTH-021 BLOQUEADA POR 054 / NO EJECUTADA; GATE-B PENDIENTE / NO SATISFECHO; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** Program.cs, csproj/lockfiles/solución/config JSON, fixtures/IT, spec/plan, constituciones/foundation, AGENTS/README y contrato 052 intactos. Sin paquetes nuevos, implementación Redis/Lua, JWT funcional, endpoints, forwarding/rotación runtime, AWS/Terraform, stage, commit o push. Historial previo conservado; trabajo detenido sin iniciar otra tarea.

## 41. Incremento contractual de capacidad y recuperación de T-AUTH-054

El **10 de noviembre de 2026**, el responsable resuelve expresamente las dos brechas de la decisión pre-BUILD de 054 y autoriza exclusivamente su BUILD CONTRACTUAL CONTROLADO. Aprueba **MaxActivePartitions=5000**, global por ambiente/finalidad de cuotas, compartido por siete cuotas/todas las instancias, y **presupuesto inicial de diseño de cuotas 64 MiB=67108864 bytes**. No equivale a usuarios/capacidad empresarial ni se deriva de los 64 MiB de la fixture. Incluye buckets, eventos, registro, metadatos y housekeeping, no garantía sobre Redis total, SO/API u otros subsistemas ni selección de tamaño/servicio AWS. Aprueba además la distribución UT en 054 e IT obligatoria real en 021/055. Baseline limpia en `feature/authentication`, HEAD `c10d0c2e742ce57ac7bc6e788799820bf791b01b`, sin consulta al remoto.

Se crean cinco archivos puros en `Enver.API/Features/Authentication/Application/Quotas/`: descriptor de capacidad, estado observado de partición, contrato de evaluación, snapshot/transición de configuración y recuperación. Tres archivos UT nuevos bajo `tests/Enver.API.UnitTests/`. `RateLimitingOptions.cs` solo aclara por XML la semántica de null; binding/validador estructural/config JSON permanecen compatibles. `ForAcademicActivation` exige **5000 explícito** y captura el presupuesto aprobado; null no dimensionado, cero/negativos/otro valor no activan esta configuración. El constructor general positivo permite modelar transiciones futuras/sintéticas, no aprobarlas. JSON continúa null; no hay activación operacional ni registro DI. 008 mantiene startup productivo pendiente.

Se reutiliza la identidad 052 **Operation+ScopeType+OpaqueId**, sin revisión, capacidad/presupuesto, PII raw o clave HMAC. La observación de partición captura definición, último evento máximo, versión técnica y verificabilidad. Activa en **(sharedNow-Window,sharedNow]**; estado incierto/evento futuro no equivale a inactivo; key residual no demuestra actividad. Retención finita último evento+ventana sin margen adicional/de revocación ni refresh por rechazo; no se ejecuta TTL. La ocupación contabiliza conservadoramente plazas residuales pendientes de liberación; capacidad deduplica requisitos, distingue existentes/nuevos y exige que quepan todos, con cálculo sin overflow. Existentes pueden evaluarse al máximo o tras reducción por debajo de ocupación si sigue verificable. Insuficiencia →Unavailable/503 sin RetryAfter, creación ni consumo parcial; cuota bloqueante verificable conserva Rejected/429 prioritario. null del cálculo solo significa ausencia de bloqueo, nunca admisión/consumo real. Resultado desconocido post-dispatch conserva Uncertain de 052, sin quinto estado.

El snapshot inmutable vincula ambiente confiable, versión de configuración, referencia de identidad no secreta, política y capacidad operacional separada. Compara contenido completo; divergencia falla cerrada. Cambiar PolicyRevision no cambia identidad ni resetea ocupación. Transición solo de capacidad/memoria exige presupuesto aprobado y snapshot único coordinado, preserva estado y no necesita drenar ventanas; reducción no expulsa y aumento no acepta límites divergentes. Valores 4000/6000 de UT son sintéticos, no aprobaciones operacionales. Cambios de política/HMAC no pueden usar este atajo: se conserva drenaje 052/053 sin current/previous simultáneos y reconciliación antes de reabrir.

Recuperación exige inactividad acreditada y liberación serializada con consumos/versiones sin renovación; mantenimiento lazy/batch futuro acotado y retiro de residuos antes de liberar plazas. Sin reset, LRU/LFU, eviction activa, sacrificio de revocaciones ni liberación por timeout. Pérdida/desalineación: reconstrucción confiable o cierre durante máximo de ventanas afectadas desde **último consumo posible**, con cese global, in-flight resueltos y capacidad reconciliada. Los bool/instantes en UT expresan precondiciones, no prueban coordinación/continuidad reales. La autoridad 020 registra emisiones JWT, no acredita ese consumo de cuotas; cuarentena JWT/PING/restart/registro vacío no abren capacidad. Actividad/ventanas/liberación usarán la misma autoridad temporal Redis TIME en 021; 054 no lee reloj local ni Redis. Privacidad: identidades opacas/metadatos mínimos, sin logs/ToString de OpaqueIds o secretos; observabilidad futura agregada, no implementada.

**Memoria no acreditada empíricamente:** 5000 particiones no garantizan caber en 64 MiB. Las obligaciones de integración quedan también enumeradas en el descriptor, sin resultados ficticios. 021/055 deben medir buckets, eventos por bucket, registro, TTL, limpieza, picos y convivencia real con revocaciones; los 64 MiB de cuotas no prueban suficiencia de una instancia Redis total de ese tamaño. Si no cabe, reducir MaxActivePartitions o aprobar mayor presupuesto/infraestructura antes de despliegue, nunca debilitar revocaciones ni cerrar productivamente sin evidencia.

Restore bloqueado correcto, build **0 advertencias / 0 errores** (43,86 s), **78 UT nuevas** y suite completa **291/291**, cero fallidas/omitidas. Se ejecutaron únicamente `dotnet restore Enver.Backend.slnx --locked-mode`, `dotnet build Enver.Backend.slnx --no-restore` y `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build` como validación técnica. SHA-256 de Program.cs y cuatro lockfiles idénticos a baseline. **Sin Docker/Testcontainers, IT/ST ejecutadas, Redis físico, Lua, TIME/TTL real, paquetes nuevos, DI productivo, AWS/Terraform ni ejecución de 021/055.** Se conserva csproj/solución/config JSON, Infrastructure/Redis, fixtures/IT/ST, contratos 052/053, spec/plan, constituciones/foundation, AGENTS/README. No stage/commit/push.

**IT diferidas obligatorias:** 021 implementará y acreditará atomicidad conjunta Redis real de registro/capacidad/cuotas, carreras multi-instance, límite global 5000, creación concurrente sin sobrepasarlo, TTL/liberación, reconciliación, timeout post-dispatch, pérdida/recuperación sin reset y medición física de memoria/convivencia básica con revocaciones. No declarar 021 completa trasladando obligaciones esenciales o fallos conocidos a 055. 055 ampliará/regresará entre hosts y siete cuotas, presión sostenida, limpieza y mediciones representativas. 054 contrato/UT →prerrequisito de diseño de 021 satisfecho →021 implementa/prueba →055 amplía IT →GATE-B solo podrá cerrar con toda evidencia exigida verde; sin dependencia circular ni eliminación de IT.

Solo después de verde se registra [E(T-AUTH-054)](specs/002-authentication/task.md#e-t-auth-054) en task §34 y este historial. **T-AUTH-054 COMPLETADA EN SU INCREMENTO CONTRACTUAL / PRERREQUISITO DE DISEÑO SATISFECHO. EVIDENCIA IT DISTRIBUIDA PENDIENTE EN T-AUTH-021/T-AUTH-055 PARA CIERRE DE GATE-B. T-AUTH-021 DESBLOQUEADA RESPECTO A 054 / NO INICIADA. T-AUTH-008 mantiene verificación productiva de startup pendiente. GATE-B PENDIENTE / NO SATISFECHO; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** No se presenta Redis, memoria/convivencia real, recuperación distribuida o seguridad integral como acreditados. Historial conservado; trabajo detenido sin otro incremento.

## 42. Cierre de T-AUTH-021 y evidencia Redis distribuida

El **10 de noviembre de 2026**, tras BUILD de 021 y correcciones focalizadas autorizadas de infraestructura/semántica de test, el responsable autoriza **BUILD DOCUMENTAL CONTROLADO** exclusivamente para registrar [E(T-AUTH-021)](specs/002-authentication/task.md#e-t-auth-021), actualizar evidencia diferida de 054 y añadir este cierre. Rama **`feature/authentication`**, HEAD **`123503c1d2a80f7e0bde2a66c8142a17c6b6604b`**, working tree con cambios no staged de 021. Esta entrega solo modifica `specs/002-authentication/task.md` y `memoria.md`; no repite Docker/build/tests, no modifica implementación/tests ni inicia 022/055. Los estados de §§39–41 conservan su valor histórico; este registro establece el estado posterior autorizado.

### Implementación y decisiones acreditadas

Se implementa **RedisAuthenticationQuotaStore** real conservando `IAuthenticationQuotaStore` de 052, junto con configuración/snapshot confiable, key builder, protocolo y lifecycle aislable en `Enver.API/Infrastructure/Redis/`, y `Scripts/ConsumeAuthenticationQuota.lua`. `Enver.API.csproj` únicamente incorpora el Lua como EmbeddedResource. Se añaden cuatro archivos UT, cinco archivos IT (incluido TestScope) y un archivo ST; la fixture compartida añade solo diagnóstico read-only de ID de contenedor. No se añade paquete, DI productivo ni endpoint; contratos 052/053/054, cuotas productivas y `Program.cs` permanecen intactos.

Script/protocolo **enver.auth.quota / 1**, una evaluación Lua conjunta por consumo, **un Redis TIME en microsegundos**, rango entero exacto hasta `2^53−1`, sorted sets y sliding windows **(nowUs-windowUs,nowUs]**. Misma marca temporal y mismo **InternalAttemptId UUID-N** en todos los ámbitos requeridos. Validaciones antes de escribir, sin inserciones/creaciones parciales ante rechazo o capacidad insuficiente; no se promete rollback general tras un error de escritura. RetryAfter positivo solo en Rejected, evento n−limit+1, máximo bloqueante entre ámbitos y redondeo superior a segundos. Unavailable antes de despacho no verificable y Uncertain ante resultado semántico desconocido post-dispatch; timeout real acreditado, sin retry ciego, compensación ni read-back para inferir resultado.

Claves `enver:{<environment>:auth:ratelimit}:bucket:<operation>:<scope>:<opaqueId>`, más `registry` y `metadata`, con **hash tag común** y KEYS declaradas; no selecciona Cluster ni prueba despliegue cross-slot. Identidad estable sin PolicyRevision/generación ni PII raw/claves HMAC. Registro global y metadata de snapshot/generación/conteo/horizonte/estado/versión/marker participan en la misma operación; **MaxActivePartitions=5000** global por ambiente/finalidad. IT reales acreditan existentes al máximo, insuficiencia conjunta, 2500 admisiones que crean 5000 particiones y carreras entre consumidores sin superar el límite ni crear parcialmente.

Retención absoluta **PEXPIREAT**, último evento+ventana sin refresh al rechazar. Lazy cleanup acotado a tres candidatos revalidados dentro de Lua; no libera renovados por pistas stale. Lifecycle explícito: inicialización solo con ambiente nuevo verificado, generación/fence, drenaje desde último consumo posible, reconciliación por lotes e inventario cerrado acotado y activación explícita. Pérdidas detectadas/marker incompleto fallan cerrados, sin bootstrap por Redis vacío ni reset permisivo. La generación anterior se invalida, activar antes del drenaje falla y el guard original no reabre por PING/restart. No se reabre la autoridad 020 ni se acredita detección universal de pérdida silenciosa.

### Incidencias de validación resueltas sin cambios productivos

El ObjectDisposedException original procedía del orden de disposición: se hizo al TestScope único owner de conexiones originales y post-restart, con limpieza antes de disponer, cleanup idempotente y AggregateException para preservar ambos errores si fallan prueba y cleanup. Después se demostró que **StopAsync/StartAsync del mismo Testcontainer puede cambiar el puerto publicado** (`127.0.0.1:62864` → `127.0.0.1:62879` en el diagnóstico inicial). El harness usa el endpoint actual, multiplexer nuevo y readiness acotada por conexión/PING, solo disponibilidad de infraestructura de test y nunca recuperación de confianza productiva.

La aserción fija de dos eventos mezclaba no-reset con expiración natural bajo ventana de 1 s. La focal usa ahora **30 s sintéticos** y observa Redis TIME/scores read-only antes del consumo; exige preservar cada evento todavía vigente, y después los vigentes previos más el nuevo. Expiración se acredita separadamente en la IT existente de ventana. En la focal verde, transcurrieron **5292832 µs** desde el evento inicial hasta la observación post-restart: el evento habría expirado con 1 s. Con 30 s se observó en Email/Origin raw=1/vigente=1/expirado=0 tras restart y raw=2/vigentes=2/expirados=0 tras consumo. **No-reset OK; fence `fenced` semántico/categoría `ok`; recovery explícito y cleanup OK; guard original cerrado.** Las siete cuotas productivas no cambiaron; no se sustituyó la expectativa `fenced` por `uncertain` ni se alteró Lua/lifecycle/store para corregir el test.

### Memoria y validación previa registrada

**Perfil sintético: 5000 particiones y 272700 eventos**, ventana ampliada de preparación de 15 minutos solo en test. Huella atribuible medida mediante MEMORY USAGE: buckets **24027944 bytes**, registro **728112 bytes**, metadata **1112 bytes**; total **24757168 bytes** frente al presupuesto aprobado **67108864 bytes**. Resultado: **DENTRO DEL PRESUPUESTO EN EL PERFIL MEDIDO**. Se obtuvo INFO MEMORY diagnóstico antes/durante/después. La coexistencia básica real preservó revocación visible por segundo consumidor, expiración absoluta y marca durante limpieza selectiva de cuotas. Esto no garantiza producción, no es benchmark productivo ni capacidad empresarial y no acredita suficiencia absoluta futura, picos sostenidos o tamaño definitivo del servicio Redis completo.

Validación técnica **ya ejecutada**, sin repetirla para este registro documental:

| Verificación | Resultado |
| --- | --- |
| Restore inicial bloqueado | Correcto; lockfiles conservados. |
| Recovery focal tras correcciones | **1/1**. |
| Clase RedisAuthenticationQuotaRecoveryTests | **10/10**. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| UnitTests `--no-build` | **333/333**, incluidos 42 casos nuevos de 021. |
| IntegrationTests `--no-build` | **78/78**, incluidos 26 casos nuevos de 021, capacidad 5000, memoria y revocaciones. |
| SecurityTests `--no-build` | **63/63**, incluidos ocho casos de 021. |

Sin fallidas ni omitidas en las suites finales. Comandos focales/filtros y trazabilidad AUTH/PLD/AC registrados en task §35. Redis real de la fixture conserva **8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba**. SHA-256 previo al cierre confirmó intactos seis archivos implementación/Lua durante las correcciones focales, `Program.cs` y cuatro lockfiles; sin paquetes nuevos, cambios de imagen/digest o ejecución AWS/Terraform.

### Estado formal y límites residuales

**T-AUTH-021: COMPLETADA / EVIDENCIA REDIS DISTRIBUIDA REGISTRADA. T-AUTH-054: EVIDENCIA IT ESENCIAL ASIGNADA A T-AUTH-021 SATISFECHA EN EL ALCANCE REGISTRADO** (task §34.1): atomicidad Redis real, concurrencia, capacidad global 5000, creation races, TTL/liberación, reconciliación, resultado desconocido, no-reset, memoria medida y coexistencia básica con revocaciones. Se conserva el cierre contractual de 054 sin declararla satisfecha integralmente para GATE-B.

**T-AUTH-055 PENDIENTE:** presión sostenida, regresión distribuida más amplia entre hosts, siete cuotas bajo carga extendida, perfiles prolongados y convivencia bajo presión ampliada. **T-AUTH-022 NO INICIADA**; no se acreditan sus controles operativos ni se autoriza su ejecución. 008 mantiene verificación productiva de startup pendiente. **GATE-B PENDIENTE / NO SATISFECHO. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.**

No hay DI/host/JWT/endpoints productivos nuevos, AWS validada, dimensionamiento definitivo de Redis productivo, failover universal, presión sostenida acreditada ni eliminación del riesgo residual de pérdida catastrófica totalmente silenciosa no observable. Se preservan spec/plan, constituciones/foundation, AGENTS/README y todo trabajo técnico existente. Cierre exclusivamente documental; sin stage, commit ni push y sin iniciar otra tarea. Trabajo detenido.

## 43. Cierre de T-AUTH-055 y evidencia de T-AUTH-054

El **10 de noviembre de 2026**, tras BUILD FOCALIZADO DE TEST autorizado en incrementos delimitados, el responsable autoriza el registro documental de [E(T-AUTH-055)](specs/002-authentication/task.md#e-t-auth-055) y de la ampliación de evidencia de T-AUTH-054. Rama **`feature/authentication`**, HEAD **`d6e764460bf5e163a067f073c937e9b16eb036f0`**; cinco archivos de prueba nuevos sin stage; producto, `Program.cs`, paquetes, lockfiles e imágenes/digests intactos. Esta entrega solo modifica `specs/002-authentication/task.md` y `memoria.md`, sin repetir pruebas ni iniciar 022.

### Evidencia acreditada

Se crean `DistributedQuotaTestScope.cs`, `DistributedQuotaTests.cs`, `DistributedQuotaPressureTests.cs` y `DistributedQuotaFailureTests.cs` en IntegrationTests, y `DistributedQuotaSecurityTests.cs` en SecurityTests, sobre el Redis real de la fixture (`redis:8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba`) con dos consumidores lógicos y `noeviction`.

P1 reparte **1500/1500** intentos REGISTER entre dos hosts, tres tramos de **30 s** a concurrencia 8, con `Unavailable=0` y `Uncertain=0` en las 18 filas de fase, `ACCOUNTED=590→624→624`, `RESIDUES=0` y memoria atribuible máxima **430744 bytes**. P2 sostiene **5000** particiones, **101993** eventos sembrados y **102000** de pico, tres ciclos de 20 s con **256** intentos/ciclo y **768** revocaciones (384 largas preservadas, 384 cortas expiradas), pico atribuible **9376224 bytes** frente a **67108864** aprobados y limpieza selectiva hasta **0** bytes atribuibles. Capacidad acredita tres rondas `4998 → carrera 16 intentos (1 admitido / 15 no disponibles) → 5000`, `two-new=Unavailable`, ámbitos existentes evaluables, expiración real y `ACCOUNTED=2` reasignadas por ronda, con memoria en frontera **1969384 bytes**. Lazy cleanup libera **5** residuos en dos evaluaciones acotadas a 3 candidatos y preserva el candidato renovado.

La matriz de siete cuotas queda **10/10**, timeout real post-dispatch → `Uncertain` sin retry, OOM real de Redis (`USED_MEMORY=34555992 > MAXMEMORY=16777216`) → `Uncertain` con `DIRTY=0` y admisión tras liberar memoria, restart sin reset con fence y recuperación validada, autoridad 020 conservada, revisión forjada `Unavailable` sin despacho y snapshot divergente `Unavailable` tras **1** despacho rechazado antes de mutar. Los ST de seguridad quedan **6/6**: forwarded falsificado no crea cupo, NAT/IPv4-mapped/IPv6 comparten partición física, privacidad de claves/miembros/diagnósticos y estados cerrados sin despacho.

### Correcciones exclusivas del harness

El presupuesto de espera se hizo opcional en `DistributedQuotaTestScope.RunAsync` (P1 180 s, P2 300 s, capacidad 240 s, limpieza 90 s, fallos 180 s) sin reducir perfiles; un timeout falla la prueba. P2 reserva **7** eventos para admitir spares (`SEEDED_EVENTS=101993`, `PEAK_EVENTS=102000`) y genera JTIs Base64url canónicos de 32 bytes (43 caracteres) conforme a `RedisKeyBuilder.ValidateJti`. Capacidad calcula el faltante real hasta 4998, reconociendo que las particiones reasignadas de la ronda anterior siguen vivas. En ST, el spoofing compara identidad opaca de cuota porque el tipo canónico no implementa igualdad por valor, y el snapshot divergente se enlaza realmente al store mediante `NewStore(..., configuration)`; `ITestOutputHelper` solo emite diagnósticos agregados sin identidades ni OpaqueIds. Ninguna corrección alteró producto, cuotas, ventanas, store, Lua, lifecycle, protocolo, `Program.cs`, paquetes ni lockfiles.

### Validación ejecutada

| Verificación | Resultado |
| --- | --- |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; lockfiles conservados. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| UnitTests `--no-build` | **333/333**, sin fallidas ni omitidas. |
| IntegrationTests `--no-build` | **97/97**, sin fallidas ni omitidas. |
| SecurityTests `--no-build` | **69/69**, sin fallidas ni omitidas. |
| Focales previas | P1, P2, capacidad, limpieza, clase pressure **4/4** y ST focal **6/6** verdes. |

### Estado formal y límites

**T-AUTH-055: COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA. T-AUTH-054: AMPLIACIÓN DE EVIDENCIA ASIGNADA A T-AUTH-055 SATISFECHA EN EL ALCANCE REGISTRADO** (task §34.2). **T-AUTH-022: NO INICIADA. GATE-B: PENDIENTE / NO SATISFECHO. AUTH-027: NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** 008 mantiene verificación productiva de startup pendiente.

No se acredita red/LB productivos, forwarding productivo, hosts físicos independientes, AWS/Terraform, dimensionamiento definitivo de Redis productivo, failover universal, benchmark empresarial ni eliminación del riesgo residual de pérdida catastrófica totalmente silenciosa no observable. Cierre exclusivamente documental; sin stage, commit ni push y sin iniciar otra tarea. Trabajo detenido.

## 44. Cierre de T-AUTH-022 y evidencia operativa Redis

El **10 de noviembre de 2026**, tras **BUILD CONTROLADO DE VERIFICACIÓN** expresamente autorizado, se registra [E(T-AUTH-022)](specs/002-authentication/task.md#e-t-auth-022). Rama **`feature/authentication`**, HEAD base **`3ae6114178258f70a749adc304e6b5ba15df942e`**; tres archivos de prueba nuevos sin stage. Producto, `Program.cs`, `Enver.API.csproj`, `Enver.Backend.slnx`, `tests/Shared/**`, lockfiles, paquetes, configuración productiva e imágenes/digests permanecen intactos. Esta entrega solo modifica `specs/002-authentication/task.md` y `memoria.md`.

### Evidencia acreditada

Se crean `RedisOperationalScope.cs` (harness de dos hosts lógicos), `RedisOperationalScopeTests.cs` (3 IT) y `RedisOperationalScopeSecurityTests.cs` (2 ST). Dos `WebApplicationFactory<AuthenticationOptions>` mantienen `ServiceProvider`, `RedisConnections`/multiplexer, `RedisSecurityStateGuard` y `RedisTokenRevocationStore` independientes, sin compartir estado local por referencia, sobre el mismo Redis real de la fixture (`redis:8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba`) y el mismo namespace de ambiente. Se acredita independencia lógica, no hosts físicos, AWS ni LB.

La revocación cruzada usa un JTI sintético Base64url de 32 bytes (43 caracteres, sin padding): B observa `NotRevoked` antes de la escritura, A confirma `Confirmed`, B observa `Revoked` con su propio store/conexión; una segunda revocación desde A con horizonte menor conserva la expiración previa y B continúa observando `Revoked`. Se verifica valor `1`, TTL positivo, expiración absoluta `PEXPIREAT` y no reducción de retención; no se añade `WAIT`, read-back productivo, retry, caché negativa, logout HTTP ni JWT real.

El perfil operativo real confirma `CONFIG GET maxmemory-policy` y `INFO maxmemory_policy` en **`noeviction`**, `MAXMEMORY_TOTAL_BYTES=67108864`, `USED_MEMORY_BYTES=1397640`, `USED_MEMORY_PEAK_BYTES=1397640` y `HEADROOM_BYTES=65711224`. El presupuesto de cuotas **67108864 bytes** se cita por separado (054, `QUOTA_BUDGET_SCOPE=CUOTAS_ONLY`) y no se confunde con el `maxmemory` total de la fixture (`MAXMEMORY_SCOPE=FIXTURE_TOTAL`); no es sizing productivo. `allowAdmin=true` se usa solo en la conexión administrativa del harness para `CONFIG GET`/`INFO`; las conexiones de los hosts no lo reciben.

La separación se verifica con claves de revocación `enver:{environment}:auth:revoked`, namespace conceptual de cuotas `enver:{environment}:auth:ratelimit`, propósitos `auth:revocation`/`auth:ratelimit` y una clave de otro ambiente fuera del namespace actual; no se afirma aislamiento físico. La señal `ConnectionFailed` se inyecta en ambos guards: con una marca de revocación previa y Redis respondiendo a `PING`, ambos hosts deniegan lectura y escritura (`Unavailable`), sin `NotRevoked`, fallback local, caché negativa permisiva ni éxito ficticio. La prueba acredita reacción al cierre inyectado, no detección automática del evento físico; el detector real conserva la evidencia de 019/055.

El reporte agregado de privacidad solo contiene estados, booleanos, conteos, memoria agregada, host lógico A/B y patrones conceptuales. Se verifica ausencia de JTI, OpaqueId, email, IP, `sub`, JWT, clave HMAC, contraseña/connection string Redis y secretos sintéticos de la fixture (`ContainsSyntheticSecret=false`). Testcontainers local no acredita TLS ni ACL productivos; no se implementan localmente ni se marcan como satisfechos, y su aplicabilidad futura queda para infraestructura/tareas correspondientes (AUTH-033/046/AD-11/068).

### Validación ejecutada

| Verificación | Resultado |
| --- | --- |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; lockfiles conservados. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| Focal IntegrationTests `~RedisOperationalScopeTests` | **3/3**, sin fallidas ni omitidas. |
| Focal SecurityTests `~RedisOperationalScopeSecurityTests` | **2/2**, sin fallidas ni omitidas. |
| UnitTests `--no-build` | **333/333**, sin fallidas ni omitidas; cero UT nuevas. |
| IntegrationTests `--no-build` | **100/100**, sin fallidas ni omitidas. |
| SecurityTests `--no-build` | **71/71**, sin fallidas ni omitidas. |
| Conservación | Producto, `Program.cs`, csproj/slnx, lockfiles, paquetes, fixture compartida e imágenes/digests intactos; sin stage, commit ni push. |

### Estado formal y límites

**T-AUTH-022: COMPLETADA / EVIDENCIA OPERATIVA REDIS REGISTRADA. GATE-B: PENDIENTE / NO SATISFECHO. AUTH-027: NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. T-AUTH-023: NO INICIADA. T-AUTH-056: NO INICIADA. T-AUTH-057: NO INICIADA. T-AUTH-058: NO INICIADA.** 008 mantiene verificación productiva de startup pendiente.

La evidencia acredita el alcance operativo local de 022, no la aceptación integral de GATE-B. No se cierra GATE-B automáticamente, no se satisface AUTH-027 y no se autoriza 023/056/057/058. No se acreditan endpoints, composición productiva, pipeline, hosts físicos, AWS/Terraform, LB, TLS/ACL productivos, dimensionamiento definitivo ni operación del servicio administrado. Cierre exclusivamente documental; sin stage, commit ni push y sin iniciar otra tarea. Trabajo detenido.

## 45. Cierre formal de GATE-B — Redis

El **10 de noviembre de 2026** se registra el [cierre formal de GATE-B en task.md §38](specs/002-authentication/task.md#cierre-gate-b). **GATE-B — Redis queda SATISFECHO / CERRADO** como checkpoint local de componentes Redis para el entorno declarado de test, con hosts lógicos independientes y dependencias reales de Testcontainers.

- **Branch:** `feature/authentication`.
- **HEAD base del cierre:** `6e3fed42d2813bd8fdd71ab928dbb046339bee4b`.
- **Working tree al evaluar:** limpio.
- **Decisión formal:** GATE-B — Redis: **SATISFECHO / CERRADO** en el entorno local declarado.
- **Evidencias utilizadas:** [E(T-AUTH-016)](specs/002-authentication/task.md#e-t-auth-016) a [E(T-AUTH-022)](specs/002-authentication/task.md#e-t-auth-022) y [E(T-AUTH-052)](specs/002-authentication/task.md#e-t-auth-052) a [E(T-AUTH-055)](specs/002-authentication/task.md#e-t-auth-055); no se duplican sus contenidos.
- **Regresión vigente:** Build **0 advertencias / 0 errores**; UnitTests **333/333**; IntegrationTests **100/100**; SecurityTests **71/71**; focal IT **3/3** y focal ST **2/2** de T-AUTH-022. No se repitieron pruebas.
- **Controles satisfechos:** Redis real compartido, dos hosts lógicos con conexiones/stores/guards independientes, revocación A→B, TTL absoluto y retención no decreciente, confirmación/lecturas, fail-closed sin fallback local ni caché negativa permisiva, siete cuotas distribuidas con Redis TIME/Lua/RetryAfter, capacidad global 5000, no partial consumption, cleanup, no reset, fencing/recovery, propagación de estado compartido, `noeviction`, memoria/capacidad, PolicyRevision/snapshot, HMAC/pseudonimización, privacidad y reapertura segura explícita.
- **Capacidad/memoria:** `MaxActivePartitions=5000`; presupuesto de cuotas **67108864 bytes** (`QUOTA_BUDGET_SCOPE=CUOTAS_ONLY`); medición 021 **24757168 bytes** atribuibles para 5000 particiones/272700 eventos; pico 055 **9376224 bytes**; perfil 022 con `maxmemory` de fixture **67108864 bytes**, `used_memory` **1397640 bytes** y `headroom` **65711224 bytes** (`MAXMEMORY_SCOPE=FIXTURE_TOTAL`). No es dimensionamiento productivo.
- **Alcance:** checkpoint local de componentes Redis para el entorno declarado de test; no es aceptación productiva.

**Límites residuales:** AUTH-027 permanece **NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**, no bloquea GATE-B y no se certifica por este checkpoint. La pérdida completamente silenciosa/no observable sigue como riesgo residual de infraestructura, no eliminada ni detectada universalmente; su tratamiento corresponde a tareas/gates/infraestructura posteriores. T-AUTH-008 mantiene **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE** y no fue entrada de GATE-B. T-AUTH-023, T-AUTH-056, T-AUTH-057 y T-AUTH-058 siguen **NO INICIADAS**. GATE-C y posteriores permanecen **PENDIENTES / NO SATISFECHOS**. GATE-B no acredita TLS/ACL productivos, AWS, Terraform, LB, hosts físicos independientes, failover cloud, sizing definitivo, deployment productivo, forwarding/LB productivo ni operación del servicio administrado.

El estado **GATE-B PENDIENTE / NO SATISFECHO** de §44 se conserva como historial del cierre de T-AUTH-022; este §45 fija el estado vigente. El cierre no inicia automáticamente tareas posteriores ni autoriza 023/056/057/058. Sin stage, commit ni push.

## 46. Cierre de T-AUTH-023 y sanitización del harness

El **10 de noviembre de 2026**, tras BUILD CONTROLADO y dos reanudaciones focalizadas expresamente autorizadas, se registra [E(T-AUTH-023)](specs/002-authentication/task.md#e-t-auth-023), task.md §39. Rama `feature/authentication`, HEAD base y conservado `833abf362f3c534b3417c2f724a769153dc633ac`. El BUILD original partió de árbol limpio; las reanudaciones conservaron su implementación parcial sin descartarla. No se hizo stage, commit ni push.

### Decisiones e implementación acreditadas

- **Driver:** única referencia directa MongoDB.Driver **3.13.0**, estable; MongoDB.Bson **3.13.0** transitivo, sin referencia redundante. Grafo añadido efectivo: DnsClient **1.6.1**, SharpCompress **0.48.1**, Snappier **1.3.1**, ZstdSharp.Port **0.7.3**. Sin downgrade/cambio de paquetes ajenos; licencias Apache-2.0, MIT y BSD-3-Clause según tabla de E023. La auditoría NuGet del BUILD original no reportó paquetes vulnerables; no es garantía universal ni análisis de imágenes.
- **Archivos de producto del incremento:** `Enver.API/Features/Authentication/Core/User.cs`, `UserStatus.cs`, `Enver.API/Infrastructure/MongoDB/UserDocument.cs`, `MongoClientFactory.cs` y referencia de paquete en csproj. Restore normal autorizado actualizó solo los cuatro lockfiles API/UT/IT/ST; restore locked posterior correcto. Las correcciones test-only no cambiaron producto/grafo.
- **BSON mínimo:** exactamente `_id/fullName/normalizedEmail/passwordHash/status/createdAt/updatedAt`; UUIDv4 backend, Standard/subtype 4, status String Active o Disabled, Active inicial, timestamps UTC BSON DateTime con precisión de milisegundos e iguales al crear. Valores desconocidos/números, faltantes y campos extra se rechazan sin fallback. Sin roles, tokens, campos auxiliares o validación funcional anticipada de 028+.
- **Cliente:** propietario aislable con snapshot congelado, composición singleton por proceso/destino y reutilización concurrente acreditada; sin DI productiva ni cambios en `Program.cs`. ReadConcern Majority, ReadPreference Primary, RetryWrites=false y WriteConcern majority+j:true. WTimeout deriva de OperationTimeoutMilliseconds: **2000 ms base** en UT y **500 ms fixture** en UT/comando real. Timeouts de conexión/selección/socket/pool finitos; sin activar CSOT interno de 3.13.0 ni admitir timeoutMS por URI que retiraría wtimeout. Presupuesto total del flujo y resultados del store quedan en 024+.
- **MongoDB real:** imagen/digest existentes de MongoDB **8.0.34-noble**, rs0 de **un nodo**, primary escribible, WiredTiger persistente y versión 8.0.34 acreditados en runtime. Insert con majority+j:true+wtimeout=500, ACK sin errores y lecturas majority desde primary; BSON bruto y round-trip tipado equivalentes. Esto acredita soporte local de escritura con journaling, no supervivencia física/failover.
- **Aislamiento:** RunId/ambiente/base verificados, dos bases propias por prueba, sin contaminación cruzada y limpieza selectiva conservando la otra base. Datos enteramente sintéticos. Sin store, índice unique, CAS, hasher ni endpoints.

### Incidencias y correcciones autorizadas

El primer BUILD tuvo una referencia a `MongoClientSettings.Timeout`, miembro interno del driver; se corrigió antes del focal, sin cambiar versión ni contrato. El primer focal UT obtuvo 31/38: siete faltantes BSON rechazados correctamente con **MongoDB.Bson.BsonSerializationException**, mientras la teoría esperaba FormatException. Se detuvo y, con autorización TEST-ONLY, se corrigió únicamente la expectativa exacta en `MongoBsonModelTests.cs`; 38/38 verdes, sin modificar producto.

Las primeras IT pasaron 3/3 pero expusieron una credencial efímera de MongoDB en logging automático de Testcontainers. Se detuvo antes de regresiones/cierre; no se transcribe el secreto y los contenedores de dependencias terminaron eliminados. La fuente exacta fue `DockerContainerOperations.ExecAsync` → logging del comando mongosh completo → ConsoleLogger; no un output helper propio. La corrección no reescribe los logs históricos.

La autorización de seguridad modifica solo `tests/Shared/AuthenticationDependenciesFixture.cs` y crea `tests/Enver.API.IntegrationTests/TestcontainersLoggingPrivacyTests.cs` antes del cierre documental. `SensitiveTestcontainersLogger`, conectado por WithLogger a los tres contenedores, redacta secretos conocidos/raw/URI-encoded, URIs MongoDB/Redis completas, patrones de credenciales, argumentos/campos sensibles y Bearer como `[REDACTED]`. No reenvía objetos originales de estado/excepción al sink. Conserva niveles, EventId, scopes saneados, tipos/mensajes/stack de errores y eventos de readiness/lifecycle/cleanup. No deshabilita logging global, no traga errores ni cambia funcionamiento/topología de la fixture.

La prueba de privacidad sin Docker comprueba mensajes, comandos, URIs, estado estructurado, scopes y excepciones con secretos sintéticos conocidos; aserciones booleanas no imprimen los valores en fallos. También acredita que warnings/errores y diagnóstico de healthcheck siguen visibles. Se corrigió un aviso xUnit2008 únicamente en esa prueba; reejecución y build final sin warnings. Capturas locales de IT focal y regresiones IT/ST bajo `%LOCALAPPDATA%/Temp/opencode/`, nombres `t-auth-023-it-focal-sanitized.log`, `t-auth-023-it-regression-sanitized.log` y `t-auth-023-st-regression-sanitized.log`, inspeccionadas antes de publicarlas: sin URIs/patrones de credencial y sin argumento de password no redactado. La captura focal muestra `[REDACTED]` y conserva evidencia útil; garantía acotada a esos canales/valores/patrones ensayados, no certificación de todos los loggers o de producción.

### Validación final ejecutada

| Verificación | Resultado |
| --- | --- |
| Regresión de privacidad `~TestcontainersLoggingPrivacyTests`, sin Docker | **1/1**, sin warnings en reejecución. |
| UT focales `~MongoBsonModelTests` o `~MongoClientFactoryTests` | **38/38**. |
| IT focales `~MongoBsonIntegrationTests` | **3/3**, BSON/topología/concerns/journaling/aislamiento reales y captura saneada. |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; sin cambios nuevos de lockfiles. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| UnitTests `--no-build` | **371/371**, 0 fallidas/omitidas. |
| IntegrationTests `--no-build` | **104/104**, 0 fallidas/omitidas; captura comprobada. |
| SecurityTests `--no-build` | **71/71**, 0 fallidas/omitidas; captura comprobada. |

Comandos/rutas exactos y grafo/licencias en E023. No hay nueva ST focal; se ejecutó la suite existente. El incremento añade 38 UT y 4 IT, incluida la prueba sin Docker de privacidad.

### Estado formal y no-claims

**T-AUTH-023: COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA. GATE-C: PENDIENTE / NO SATISFECHO. T-AUTH-024/025/026/027: NO INICIADAS.** GATE-A/B conservan SATISFECHO / CERRADO; GATE-D–H pendientes. AUTH-027 continúa NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE; 008 mantiene IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; 056/057/058 NO INICIADAS. El estado previo de 023 en §§44–45 se conserva como historial; este §46 fija su cierre posterior autorizado.

`Program.cs`, Options/appsettings, spec/plan, constituciones, foundation, AGENTS/README, solución y proyectos de tests intactos; fuera de 023 solo se interviene la frontera de logging test-only expresamente autorizada. Funcionalidad de fixture, imágenes/digests, rs0, credenciales, puertos, readiness, timeouts y cleanup conservados. No se implementan 024+, register/login/logout, Orders, AWS o Terraform. No se acredita quorum multinodo, failover, HA, Atlas, cloud, TLS/ACL productivos, dos instancias desplegadas ni producción; contraste de infraestructura/topología productiva y 068 pendientes. Se detiene sin stage, commit ni push ni iniciar otra tarea.

## 47. Cierre de T-AUTH-024 y T-AUTH-025 tras regresión final

El **10 de noviembre de 2026**, la autorización **REGRESIÓN FINAL DEFINITIVA Y CIERRE** permite validar sin nuevas correcciones previas y, únicamente tras build/UT/IT/ST verdes, actualizar `specs/002-authentication/task.md` y esta memoria. Rama `feature/authentication`, HEAD base y conservado `64db47e347647e76ffb3588663a2a5e4616295ca`; cambios preexistentes conservados, sin stage/commit/push. Se mantuvo la instrumentación test-only ya existente en las tres clases IT Mongo024/Mongo025/capacidad Redis; no se modificaron producto, tests, fixture, expectativas, warm-ups, retries o plazos durante esta validación final.

### E(T-AUTH-024) — UserStore aislable

Registrada separadamente en [task.md §40](specs/002-authentication/task.md#e-t-auth-024): `IUserStore.cs` y `MongoUserStore.cs`, con FindByNormalizedEmailAsync/FindByIdAsync/InsertAsync. Igualdad construida por backend; Found/NotFound/Unavailable y Confirmed/Conflict/Unavailable/Uncertain; lectura fallida nunca es inexistencia. Conflict exige evidencia estructurada de duplicate normalizedEmail; duplicate `_id` distinto no es conflicto de email. Conserva concerns majority/primary, escritura majority+j:true+wtimeout=500 y RetryWrites=false; cero retry, read-back automático, compensación, caché o cambio de identidad. UT focal previa **22/22**, IT focal previa **5/5** y **5/5** en la regresión final: lectura tras confirmación, duplicados específicos, errores BSON, Disabled, aislamiento y ACK-loss. ACK-loss usa escritura real con entrega de confirmación suprimida en frontera de adapter test-only; no prueba corte TCP/failover real.

El índice es **precondición test-only de 024**: warm-up existente ping/admin/Primary con CTS independiente de 5 s y primary obligatorio; preparación mediante la definición exacta de 025 con RequestBudget independiente de **5000 ms** y verificación obligatoria **Ready** por el initializer, que mantiene dependencia de **500 ms**. Solo después se crea el RequestBudget del store de **8000 ms**, dependencia MongoDB **500 ms**. Creación/idempotencia/carreras del índice se acreditan en 025, no se cargan al presupuesto medido del store. Sin Options alternativos ni timeout productivo aumentado.

### E(T-AUTH-025) — Índice e inicialización

Registrada separadamente en [task.md §41](specs/002-authentication/task.md#e-t-auth-025): `MongoUsersInitializer.cs`, normalizedEmail ascending, unique=true, collation simple/binaria, sin sparse/partial/hidden/TTL; inspección semántica y rechazo de buildUUID, no solo nombre. Creación/verificación, idempotencia, índice equivalente con otro nombre sin rename, dos hosts lógicos concurrentes, incompatibilidad y duplicados, fail-closed, preservación y no reparación. Dependencia única **500 ms**, RequestBudget IT **8000 ms**, RetryWrites=false, sin retry aplicativo ni reconciliación/borrado automático. Ready solo con verificación dentro de presupuesto; Incompatible con evidencia concluyente y verificación completada; Unavailable si no hay confirmación dentro del límite, sin conversión retroactiva a Ready.

UT focal previa **21/21**, IT focal previa **11/11** y **11/11** en la regresión final. HostA y HostB retornaron Ready con clientes independientes y un único índice compatible. El caso concluyente de duplicados observó **11000 / DuplicateKey** en createIndexes, duración **202.584 ms**, antes del timeout; resultado Incompatible y token no cancelado. Dos documentos originales preservados; solo `_id_` antes/después/tras 300 ms, sin índice normalizedEmail utilizable. El tercer duplicado diagnóstico fue aceptado, conservando los dos originales y sin declarar unicidad/Ready ni reparar datos. Detalle de pruebas, trazabilidad y límites en E025.

### Validación final ejecutada

| Verificación | Resultado real |
| --- | --- |
| `docker info` | Disponible, Engine **29.6.1**. |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; lockfiles sin cambios. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| UnitTests completas `--no-build` | **414/414**, 0 fallidas/omitidas. |
| IntegrationTests completas `--no-build --logger "console;verbosity=detailed"` | **120/120**, 0 fallidas/omitidas, **7.9706 minutos**; una sola ejecución, límite externo 900 s no alcanzado. |
| SecurityTests completas `--no-build` | **71/71**, 0 fallidas/omitidas; ejecutadas después de IT verde. |

Comandos/rutas completos y captura local de IT en E024 §40.3. Suites secuenciales, sin otra ejecución de IT después del verde. Redis capacity volvió a acreditar **2500 admissions reales / 5000 particiones**, metadata=registry=5000, un ganador y siete Unavailable esperados en carrera, sin admisión parcial según aserciones. Snapshot final de ese caso: PING disponible, used_memory=4409872 bytes, maxmemory=67108864 (64 MiB), policy=noeviction, connected_clients=4 y blocked_clients/rejected_connections/evicted_keys=0. Sin OOM observado en ese caso; la prueba de OOM intencional usa otro contenedor dedicado. **GATE-B permaneció SATISFECHO / CERRADO**, no se reabrió.

### Incidencias históricas conservadas

- Regresión anterior **119/120**: IT positiva Mongo024 retornó initializer Unavailable pese a comandos exitosos; aislado **1/1** y subconjunto **7/7** pasaron. Causa no determinada. Ajuste posterior autorizado exclusivamente test-only separó la precondición del índice del presupuesto del store; no cambió producto.
- Focal 025 anterior **10/11**: duplicados, Unavailable/cancelación y un segundo índice listado rompieron preservación de índices. Diagnóstico aislado **1/1** observó 11000/DuplicateKey e Incompatible dentro de presupuesto, sin segundo índice. Su identidad/estado histórico no quedó demostrado; no se infiere defecto de initializer ni índice único construido indebidamente.
- Otra regresión **119/120**, con Mongo024 **5/5** y Mongo025 **11/11**, falló en el llenado de capacidad Redis: Admitted esperado / Unavailable. Aislado **1/1** y subconjunto inmediato memoria+capacidad **3/3** verdes, con 2500 admissions/5000 particiones. **Clasificación E: causa no determinada**, sin atribuir OOM, performance, fixture o defecto productivo.

Estas incidencias de test **no reaparecieron en la ejecución final**; no se ocultan ni se declaran resueltas por una causa raíz no demostrada. Se conserva instrumentación diagnóstica test-only, que puede afectar timings y no constituye benchmark. **No se modificó producto para forzar verde**, no se relajaron aserciones, no se añadieron retries ni se aumentaron timeouts internos.

### Estado formal y límites

**T-AUTH-024: COMPLETADA / EVIDENCIA REGISTRADA. T-AUTH-025: COMPLETADA / EVIDENCIA REGISTRADA. T-AUTH-026: NO INICIADA. T-AUTH-027: NO INICIADA. GATE-C: PENDIENTE / NO SATISFECHO. GATE-A/B: SATISFECHOS / CERRADOS.** Los estados de 024/025 de §46 se conservan como historial; este §47 registra su cierre posterior autorizado. GATE-D–H siguen pendientes; AUTH-027 sigue NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE; 008 mantiene verificación productiva de startup pendiente y 056/057/058 no se inician.

Solo aceptación de componentes aislables locales: sin DI/startup productivo, CAS, register/login/logout, Orders, AWS/Terraform, Atlas/HA/quorum multinodo/failover, TLS/ACL/IAM productivos, dos hosts físicos ni sizing productivo. No se cierran requisitos funcionales íntegros ni GATE-C. Constituciones, spec/plan, foundation y AGENTS/README permanecen intactos; no se añaden paquetes ni cambian configuración, imágenes/digests o topología. Documentación final limitada a task/memoria; sin stage, commit ni push. Trabajo detenido sin iniciar otra tarea.

## 48. Cierre de T-AUTH-026 y T-AUTH-027 y revisión formal de GATE-C

El **10 de noviembre de 2026**, autorización **BUILD AGRUPADO Y CIERRE DE GATE-C** para implementar y validar secuencialmente 026 → 027, ejecutar regresión completa y registrar sus evidencias separadas antes de revisar formalmente el gate. Rama `feature/authentication`, HEAD base y conservado `255140461911e7121dba73f46abbd24f4d226eb1`, árbol limpio de partida. Sin stage, commit ni push.

### E(T-AUTH-026)

Registrada en [task.md §42](specs/002-authentication/task.md#e-t-auth-026). Únicos archivos productivos modificados: `Enver.API/Features/Authentication/Application/Ports/IUserStore.cs` y `Enver.API/Infrastructure/MongoDB/MongoUserStore.cs`. Firma exacta aprobada `CompareExchangePasswordHashAsync(Guid id, string expectedPasswordHash, UserStatus expectedStatus, string newPasswordHash, DateTime updatedAtUtc, RequestBudget budget)`, retorno `ValueTask<UserCredentialUpdateResult>`; sin otro método CAS.

- UpdateOneAsync tipado, filtro backend **_id/hash anterior/status esperado**, collation simple/binaria, no upsert, `$set` únicamente **passwordHash/updatedAt**. No reemplazo completo ni modificación de status/createdAt/otros campos. User/BSON de siete campos intactos, sin hashVersion ni campos extra.
- **Updated:** ACK válido y matched=1/modified=1 dentro del presupuesto; **Conflict:** ACK válido 0/0 sin distinguir ID ausente/Disabled/hash obsoleto; **Unavailable:** predispatch/rechazo concluyente o no-op confirmado 1/0; **Uncertain:** posible despacho sin confirmación, write concern incierto, ACK perdido o timeout/cancelación posterior. Sin traducir a HTTP ni reclasificar incertidumbre como conflicto.
- UTC del backend obligatorio, normalización a milisegundos; no exigir >anterior ni inventar +1 ms. RequestBudget IT **8000 ms**, dependencia **500 ms**, sin reinicio ni ampliación. Comando update real con **majority+j:true+wtimeout=500**, lecturas majority/primary, RetryWrites=false y sin txnNumber. No retry, read-back automático, rollback, compensación o segundo update. 032/048 deciden después relectura/credenciales/emisión.
- Nuevos `MongoUserStoreCredentialCasTests.cs` (**22/22** focal) y `MongoUserStoreCredentialCasIntegrationTests.cs` (**6/6** focal): contrato/filtro/update/ACK/no-op/plazos/privacidad, campos preservados, hash/status/ID mismatch y dos clientes independientes con **un Updated/un Conflict**, hash/timestamp del ganador y rechazo del hash anterior. ACK de update real suprimido en frontera test-only: **Uncertain**, un intento, cero lecturas/escrituras compensatorias automáticas; no corte TCP/failover ni exactly-once.
- **Una corrección test-only, de máximo dos autorizadas:** primer build CS1729 por constructor no público de WriteError usado en doble UT; corregida exclusivamente su construcción sintética mediante reflexión. Build posterior 0/0 y focales verdes. No cambió producto, contrato, expectativas, plazos o fixture. Solo después se creó 027.

### E(T-AUTH-027)

Registrada separadamente en [task.md §43](specs/002-authentication/task.md#e-t-auth-027). Nuevo `tests/Enver.API.IntegrationTests/MongoPersistenceIntegrationTests.cs`, **3/3** focales; integra 023–026 mediante helpers existentes y helpers nuevos de 026, sin modificar otros tests/Shared.

- A inserta confirmado y B lee por ID/email; inspección real de exactamente siete campos, UUID Standard/subtype 4 y timestamps BSON; inicializador Ready e índice unique semánticamente compatible, un `_id_`.
- Dos clientes independientes sobre la misma base: altas con mismo normalizedEmail ya normalizado y UUID distintos producen **un Confirmed/un Conflict**, un documento final. No implementa validación/normalización HTTP 028.
- Sobre la cuenta ganadora, CAS concurrente **Updated+Conflict**, hash/timestamp ganador preservados; hash/status obsoletos sin mutación. Lectura fallida de BSON real incompatible produce **Unavailable != NotFound**.
- ACK de update suprimido retorna Uncertain sin retry/read-back/compensación; la observación posterior del test no corrige el resultado original. Bases propias aisladas y cleanup de una conserva la otra.
- Consultas reales hello/buildInfo confirman **MongoDB 8.0.34, rs0 de un nodo, primary escribible**; E023 y regresión complementan WiredTiger persistente/concerns/journaling. Dos clientes/hosts lógicos, no dos hosts físicos ni prueba de Atlas/HA/failover cloud.

### Validación ejecutada

| Verificación | Resultado real |
| --- | --- |
| Restore locked antes de focales y antes de regresión | Correctos, sin cambios de lockfiles. |
| Build después de corrección UT, después de crear 027 y final | **0 advertencias / 0 errores**. |
| UT focal 026 | **22/22**, 0 fallidas/omitidas. |
| IT focal 026 | **6/6**, 0 fallidas/omitidas; 1.2815 minutos incluida fixture. |
| IT focal 027 | **3/3**, 0 fallidas/omitidas; 56.7199 segundos incluida fixture. |
| UnitTests completas `--no-build` | **436/436**, 0 fallidas/omitidas. |
| IntegrationTests completas `--no-build --logger "console;verbosity=detailed"` | **129/129**, 0 fallidas/omitidas, **7.4388 minutos**; una ejecución completa. |
| SecurityTests completas `--no-build` | **71/71**, 0 fallidas/omitidas; después de IT verde. |

Comandos/rutas y fuente primaria en E026 §42.4: salidas reales de sesión y captura local completa IT `sh_1c91a95d2001bPcOIcdH9wpWrV.out`, retenida por el harness fuera del repositorio; no artefacto versionado. Nuevas pruebas: 22 UT y 9 IT; suite ST existente sin nuevos casos artificiales. Diagnósticos agregados/datos sintéticos, sin hashes, conexiones, keyValue o documentos sensibles en output; Testcontainers conserva redacción de password.

### Revisión formal E023–E027 y decisión GATE-C

La autorización incluye expresamente este acto, distinto del cierre de tareas. [Task.md §44](specs/002-authentication/task.md#cierre-gate-c) consolida E023 (§39), E024 (§40), E025 (§41), E026 (§42) y E027 (§43) contra **siete campos/BSON estándar, creación confirmada observable, índice exacto/inicializador, unicidad concurrente, CAS y resultados inciertos seguros**. En regresión actual: Mongo023 **3/3**, Mongo024 **5/5**, Mongo025 **11/11**, Mongo026 **6/6**, Mongo027 **3/3**; driver/grafo/concerns/journaling/ACK y fixture rs0 previamente aprobados se conservan. No se concluye el gate solo por la existencia de clases o aprobación documental.

**T-AUTH-026 y T-AUTH-027: COMPLETADAS / EVIDENCIA REGISTRADA. GATE-C: SATISFECHO / CERRADO como checkpoint local/test representativo de componentes MongoDB. GATE-A/B: SATISFECHOS / CERRADOS, intactos. GATE-D–H: PENDIENTES / NO SATISFECHOS.** Redis capacity pasó nuevamente 2500 admissions/5000 particiones; las incidencias históricas Mongo/Redis de §47/E024 no reaparecieron y permanecen sin causa productiva demostrada. El estado de 026/027/GATE-C de §47 se conserva como historial; este §48 fija el estado posterior autorizado.

Sin acreditar Atlas, HA productiva, failover cloud, AWS/infraestructura productiva, TLS/ACL/IAM, dos hosts físicos o sizing cloud. 068 y mínimo privilegio de inicialización productiva pendientes. **008** mantiene startup productivo pendiente; **056/057/058 NO INICIADAS**. Register, Login, Logout, emisión JWT, PasswordHasher, validación/rehash funcionales, Orders, AWS y Terraform no se implementan ni inician. AUTH-027 (continuidad Redis) permanece NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE; no confundir con T-AUTH-027 de MongoDB.

Únicamente siete archivos afectados: dos productivos autorizados, tres tests nuevos y task/memoria tras verde. Program.cs, User/BSON, factory, Options, csproj/lockfiles, tests/Shared, imágenes/digests, topología/puertos, Redis/DynamoDB, spec/plan, constituciones, foundation y AGENTS/README intactos. Cero paquetes nuevos. Sin stage, commit ni push; detener sin iniciar siguiente tarea.

## 49. BUILD-1 de autenticación — Regresión final, evidencias y aceptación

El **11 de noviembre de 2026**, el responsable autoriza **REGRESIÓN FINAL Y ACEPTACIÓN** de BUILD-1, escrito en orden **056 → 029 → 028 → 030 → 031 → 033 → 057**, después de los incrementos correctivos/diagnósticos test-only expresamente autorizados. Rama `feature/authentication`, HEAD base y conservado **`8db9b6082ad3e826169d16da0df0b38d0e0ef442`**; baseline de la validación **6 modificados + 20 nuevos, sin stage**. No se modifica nada antes de ejecutar ni se aplica corrección durante la validación; solo tras todas las suites verdes se actualizan este archivo y `task.md`. Sin stage/commit/push.

### 49.1. Resultado final y fuentes

Registro común, comandos y archivos en [task.md §45](specs/002-authentication/task.md#build-1-validacion-final); fuente primaria: salidas reales de esta sesión. Se ejecutan secuencialmente `docker info`, `dotnet restore Enver.Backend.slnx --locked-mode`, `dotnet build Enver.Backend.slnx --no-restore` y las tres suites mediante `dotnet test tests/Enver.API.{UnitTests,IntegrationTests,SecurityTests}/Enver.API.{UnitTests,IntegrationTests,SecurityTests}.csproj --no-build`, cada ruta por separado y sin filtro. SecurityTests se ejecuta únicamente después de IT completas verdes. No se repite ninguna suite.

- Docker cliente/servidor **29.6.1**, Linux/WSL2, 4 CPU, 7.698 GiB, disponible y sin cambios de configuración.
- Restore locked correcto; build **0 warnings / 0 errors**, **34,20 s**.
- **UT 493/493**, 0 fallidas/omitidas, **4 s**.
- **IT 154/154**, 0 fallidas/omitidas, **8 min 58 s**, una sola ejecución y límite externo **900 s**.
- **ST 79/79**, 0 fallidas/omitidas, **50 s**.
- No reaparecen el rechazo prematuro Redis, el timeout Mongo 025 ni DynamoDB `ClassFormatError`.

### 49.2. Siete evidencias individuales

| Tarea / evidencia | Registro separado y alcance acreditado |
| --- | --- |
| **056** | [E(T-AUTH-056), §46](specs/002-authentication/task.md#e-t-auth-056): Program/Composition, Options/ValidateOnStart, errores/ProblemDetails/correlación/no-store, frontera/RequestBudget, proxies explícitos, HTTPS/HSTS local por ambiente y startup inválido. Focal previa **15/15 IT**, incluida en regresión final; sin claim de TLS publicado/AWS. |
| **029** | [E(T-AUTH-029), §47](specs/002-authentication/task.md#e-t-auth-029): creación 12–128 escalares/≤512 bytes UTF-8, Unicode estricto/suplementarios/surrogates, secreto íntegro sin trim/normalize/case/truncate o composición adicional. Focal previa **13/13 UT**, incluida en regresión. 513 bytes exige superar también 128 escalares; el límite de bytes se contrasta además aisladamente. |
| **028** | [E(T-AUTH-028), §48](specs/002-authentication/task.md#e-t-auth-028): email ASCII/trim/lowercase invariant/dominio exacto uttt.edu.mx/topes 254 y 64/sintaxis/equivalencia; nombre ≤200 escalares, sin controles, conserva case/acentos/espacios internos. UT compartidas **35/35** con 030, no duplicadas. No existe positivo institucional de 254 bytes compatible con dominio fijo/local-part ≤64. |
| **030** | [E(T-AUTH-030), §49](specs/002-authentication/task.md#e-t-auth-030): fronteras Unicode/bytes/identidad y entradas maliciosas, rechazo en validadores puros sin escritura/emisión. **8/8 ST** focales y UT compartidas, incluidas en regresión. No acredita por sí sola transporte o Register funcional, pendientes de su integración posterior. |
| **031** | [E(T-AUTH-031), §50](specs/002-authentication/task.md#e-t-auth-031): PasswordHasher<User>/Options predeterminadas del framework sin iteraciones manuales, Success/Failed/SuccessRehashNeeded, salts distintos, preservación exacta y dummy Lazy singleton preinicializado por host con mismo hasher, no por request. **7/7 UT** focales; sin criptografía propia ni plaintext persistido/logueado por el adapter; sin rehash/CAS 032. |
| **033** | [E(T-AUTH-033), §51](specs/002-authentication/task.md#e-t-auth-033): gate configurable, límite efectivo 2 comprobado, admisión inmediata/saturación sin cola, cancelación/errores y permiso retenido hasta fin real, sin Task.Run productivo. **2/2 UT + 3/3 IT** focales incluidas; las IT de ownership usan límite 1 de forma explícita. Sin calibración productiva 067. |
| **057** | [E(T-AUTH-057), §52](specs/002-authentication/task.md#e-t-auth-057): clientes singleton/stores reales, inicializador Mongo y admisión interna Ready/Incompatible/Unavailable/fail-closed, Mongo/Redis locales reales, DI y liveness `/health`, sin `/health/ready` o bootstrap productivo implícito. **7/7 IT** focales incluidas. Autoridad DynamoDB Local sintética, sin recursos AWS reales. |

Los focales previos **15+3+7=25 IT** corresponden a composición/concurrencia/admisión; se incluyen íntegramente en la regresión de 154 casos. No se repiten focales en esta validación final ni se suman suites compartidas como evidencias independientes de mayor tamaño.

### 49.3. Historial correctivo y diagnóstico conservado

No se borran las regresiones rojas por el cierre verde. La primera IT **129/154** tuvo **21** fallos de referencias obligatorias de Composition ausentes en factories históricas, **3** de autoridad local eliminada por cleanup compartido, y **1** de 025 por 500 ms. Con autorización posterior se corrigieron solo factories/configuración sintética y aislamiento de tablas propias de 057; **24/24** focales correctivos y **11/11** focales de 025. Se conservaron validaciones, aserciones, fixture y fail-closed productivo; los dobles de autoridad son exclusivamente test-only para escenarios que no prueban 020.

La regresión posterior **153/154** falló en capacidad Redis durante `FillRegisterAsync(2499)`; no se ejecutaron ST completas ni se aceptó BUILD-1 entonces. Un diagnóstico posterior reprodujo el síntoma en el caso hermano, intento **332**, 331 admisiones/662 particiones, sin causa/categoría Lua capturada; otro intento se bloqueó en startup por `java.lang.ClassFormatError` de DynamoDB Local, incidencia ambiental separada, sin cambios Java/imagen/fixture. Las tres focales semánticas siguientes fueron verdes: intento 332 Admitted, 2500 admisiones/5000 particiones, y únicamente `unavailable/capacity` esperado en siete perdedores de la carrera. La captura confirma esa rama concreta y ausencia de escritura parcial del rechazado, **no explica retrospectivamente el fallo prematuro**. No se justificó cambio productivo Redis. Historial, instrumentación y límites en task §45.2; los snapshots de diagnóstico añaden coste y no acreditan rendimiento productivo.

### 49.4. Decisión, integridad y checkpoint

**BUILD-1 ACEPTADO** al satisfacer la condición expresa de restore/build/UT/IT/ST completos verdes, sin fallidas/omitidas. **T-AUTH-056/029/028/030/031/033/057: COMPLETADAS / EVIDENCIA REGISTRADA**, con fichas actualizadas y registros separados. [Task §53](specs/002-authentication/task.md#build-1-aceptacion) prepara el checkpoint documental; **no se hace stage, commit o push**. HEAD permanece intacto.

**GATE-A/B/C SATISFECHOS / CERRADOS; GATE-D PENDIENTE / NO SATISFECHO**, porque faltan **034–037**, no iniciadas. GATE-E–H pendientes; 032 y 058 no ejecutadas. No se cambia automáticamente el estado documental de 008. Los estados de no inicio de 056/057 y ausencia de composición/hashing en §48 y task §§1–44 son antecedentes de aquellas entregas, no el estado posterior de este §49.

User/BSON de siete campos, contratos/código Mongo, stores/protocolo/Lua Redis, capacidad **5000**, timeouts, csproj/lockfiles, fixture, imágenes/digests y topología intactos; **cero paquetes nuevos**. La huella del árbol productivo fuente antes/después de la validación coincide. No se implementan Register/Login/Logout/JWT, Orders, AWS/Terraform o endpoints funcionales. `/health` sigue siendo liveness, sin readiness nuevo ni claim de Atlas, HA/failover cloud, TLS publicado o AWS/IAM. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**. Spec/plan, constituciones, foundation, AGENTS/README y agentes/skills no se modifican. Tras el verde solo se escriben `task.md` y `memoria.md`; se detiene el trabajo sin iniciar BUILD-2, generar artefactos adicionales o repetir pruebas.

### 49.5. Solicitud posterior de checkpoint Git: limpieza y validación bloqueante

La autorización posterior **AUTH BUILD-1 — CHECKPOINT GIT** exige revisar todos los diffs y los **20 archivos nuevos completos**, retirar diagnóstico temporal y validar si esa limpieza cambia lógica ejecutable. Se revisa la baseline correcta de **8 modificados + 20 nuevos**, sin stage, rama `feature/authentication`, HEAD `8db9b6082ad3e826169d16da0df0b38d0e0ef442`; ningún archivo ajeno al incremento ni secreto real identificado en la revisión manual. Se conservan factories/configuración sintética, aislamiento de tablas y sus aserciones históricas.

Se retira íntegramente `RedisCapacityDiagnostic` de `RedisAuthenticationQuotaCapacityTests.cs`: hooks/stores observados, EVAL_RO/snapshots, profiler, reloj/timers y suscripciones globales temporales. Los casos mantienen requests, escenarios, dos stores reales y aserciones. Se elimina solo la ruta diagnóstica opcional de `RedisAuthenticationQuotaTestScope.cs`, que queda **idéntico a HEAD**. El total pendiente pasa por ello a **7 modificados + 20 nuevos =27**, no 28. No se cambian producto, fixture, budgets **Redis 10000/2000 ms**, capacidad 5000, BSON/Mongo, csproj/lockfiles o paquetes. La limpieza cambia recorrido ejecutable/coste temporal del test, no es solo logging y no prueba causa del rechazo prematuro histórico.

Aplicando exclusivamente la excepción del punto 5, se ejecutan una vez `dotnet build Enver.Backend.slnx --no-restore` (**0 warnings/0 errors**, 33,70 s) y después IT completas `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build` (**153/154**, **1 fallida/0 omitidas**, **8 min 22 s**, límite externo 900 s). Fallo: `MongoUsersInitializerIntegrationTests.Existing_duplicate_data_blocks_readiness_and_is_not_reconciled`. Captura existente: `createIndexes` iniciado, dependencia de **500 ms** cancelada, `Unavailable` con **7503,303 ms** restantes de RequestBudget **8000 ms**, `TaskCanceledException`; no respuesta de servidor confirmada para el resultado esperado. Metadata transitoria no acredita índice utilizable ni readiness, originales preservados. Detalles en [task §53.1](specs/002-authentication/task.md#531-solicitud-posterior-de-checkpoint-git--bloqueo-tras-limpieza-test-only). Sin otro fallo Redis ni `ClassFormatError`; no se atribuye causa raíz ni causalidad a la limpieza.

**CHECKPOINT BLOQUEADO / NO CREADO**. No stage, commit o push; sin correcciones, retry o más pruebas. UT **493/493**, IT **154/154**, ST **79/79**, BUILD-1 aceptado y siete evidencias de §§49.1–49.4 son antecedentes reales previos a la limpieza, **no una regresión íntegra verde del árbol posterior**. Se conserva esa historia y el estado de las siete tareas; la entrega actual no declara checkpoint validado. **GATE-A/B/C cerrados; GATE-D pendiente/no satisfecho**, 034–037 no iniciadas, AUTH-027 no satisfecho integralmente. Se preservan limpieza y trabajo existente sin confirmar, y se detiene para nueva autorización delimitada.

**Estado posterior — árbol post-limpieza revalidado.** La solicitud independiente **AUTH BUILD-1 — REVALIDACIÓN DEL ÁRBOL LIMPIO Y CHECKPOINT** autoriza el focal 025, una regresión completa condicionada a ese verde y, únicamente con todo verde, stage de los **27 archivos** y commit local. Se comprueban rama/base esperadas y **7 modificados + 20 nuevos / 0 staged**. Sin cambios técnicos, expectativas, timeouts, retries o instrumentación temporal. Comandos exactos y fuente primaria en [task §53.1](specs/002-authentication/task.md#531-solicitud-posterior-de-checkpoint-git--bloqueo-tras-limpieza-test-only): focal `FullyQualifiedName~MongoUsersInitializerIntegrationTests` **11/11**, **5 s**; restore locked correcto; build sin restore **0 warnings / 0 errores**, **18,44 s**; UT completas sin build **493/493**, **4 s**; IT completas sin build **154/154**, **7 min 44 s**, una sola ejecución, límite externo **900 s**; ST completas sin build **79/79**, **49 s**, solo tras IT verdes. Suites ejecutadas secuencialmente, **0 fallidas / 0 omitidas**. No reaparecieron timeout 025, fallo prematuro Redis o `ClassFormatError`.

**ÁRBOL POST-LIMPIEZA ACEPTADO / REVALIDADO**: se satisface el bloqueo de validación posterior sin borrar el intento rojo anterior, inferir su causa o reescribir la aceptación histórica. Se mantienen las siete evidencias **E056 §46/E029 §47/E028 §48/E030 §49/E031 §50/E033 §51/E057 §52** y estados COMPLETADA / EVIDENCIA REGISTRADA. Solo se amplían esta nota y task §53.1 después del verde. Se autoriza ahora checkpoint de los 27 archivos revisados con mensaje exacto `feat(auth): add validation hashing and dependency composition`, previa verificación del índice; hash y estado final se informan mediante la salida Git de la sesión. Producto/BSON/Mongo/Redis/Lua, capacidad 5000, presupuestos, fixture, csproj/lockfiles y paquetes intactos durante esta revalidación. **GATE-A/B/C cerrados; GATE-D pendiente/no satisfecho**, 034–037/BUILD-2 no iniciadas; AUTH-027 no satisfecho integralmente. Sin push/pull/fetch/merge/rebase o nuevas funcionalidades; detener después del checkpoint local.

## 50. BUILD-2 — Register aceptado y cierre formal local/test de GATE-D

### 50.1. Autorización y fuentes

El **11 de noviembre de 2026**, la solicitud **AUTH BUILD-2 — VALIDACIÓN FINAL, EVIDENCIAS Y CIERRE GATE-D** autoriza exclusivamente ejecutar ST completas y, tras verde sin fallidas/omitidas, registrar E034–E037 y revisar formalmente GATE-D en `specs/002-authentication/task.md` y este archivo. No repetir UT/IT completas, modificar producto, iniciar otro gate ni hacer stage/commit/push. Baseline comprobada: rama **`feature/authentication`**, HEAD **`fe7858dc054a21a68fc051f95db6cb2e7383f8a4`**, **2 modificados + 14 nuevos / 0 staged**. El checkpoint posterior de BUILD-1 ya está representado por ese HEAD; §49.5 se conserva como historial, no como prohibición o ausencia actual de aquel commit.

Fuente primaria: salidas reales de comandos de esta conversación y notificación final de IT completas. Autorizaciones, archivos, comandos, historia y representatividad en [task §54](specs/002-authentication/task.md#build-2-validacion-final). No se deduce aceptación de una baseline declarada: las solicitudes anteriores de checkpoint se detuvieron porque faltaban ST completas y cierre documental.

### 50.2. Incidencias y correcciones test-only conservadas

El primer focal agrupado dio **7/39**: 32 casos nuevos bloqueados por el nombre de base sintética fuera del prefijo de ambiente. Corrección limitada al scope, derivando el nombre de `fixture.MongoDatabase`, sin relajar validadores/producto; después **38/39**. El caso origin 20/900 seguía con `EqualException`; la captura saneada inicial no identificaba la comparación y no se atribuyó causa productiva.

Dos ejecuciones diagnósticas aisladas posteriores identificaron la aserción de setup `initialized` frente a `unavailable/closed`, **DependencyTimeout** con presupuesto Redis **250 ms**; comprobación local observada **330,7 ms**, no duración aislada medida de la conexión física. **0 requests HTTP, 0 llamadas de cuota, 0 EVAL de inicialización**: no era evidencia de fallo en Register, conteo o Retry-After. Corrección expresamente autorizada: conectar el Redis real propio y completar **PING bajo el token de setup**, antes del lifecycle. **Pre-warm exclusivamente TEST-ONLY**, sin consumir cuota/Lua, crear buckets o modificar autoridad; sin cambiar los 250 ms ni retries.

Después: origin **1/1**, Register/composición **39/39**, ST focal **5/5**. La instrumentación temporal posterior no era solo logging: añadía snapshots y un `executeOverride` con obtención de conexión después de MarkDispatched. Se detuvo el cierre al detectarlo y se solicitó autorización. **LIMPIEZA COORDINADA DEL DIAGNÓSTICO Y REVALIDACIÓN FOCAL** autorizó limpiar `RegistrationTestScope.cs` y `RegistrationTests.cs`, restaurar el store real compuesto y validar focalmente, sin repetir IT completas. Se retiraron completamente `OriginQuotaDiagnostic`, snapshots, hooks/override, wrappers, output y captura temporal de comparaciones/stack; saneamiento de errores, pre-warm, requests/escenarios y aserciones funcionales conservados. No se ocultaron los rojos, cambió producto o se reutilizó el resultado instrumentado como si fuera un run post-limpieza.

### 50.3. Validación aceptada, con relación temporal explícita

| Evidencia | Resultado real |
| --- | --- |
| Focal UT del servicio | **13/13**, incluido en UT completas. |
| Restore locked previo | Correcto, sin cambios de paquetes/lockfiles. |
| Build previo a regresión | **0 warnings / 0 errores**, **18,97 s**. |
| UT completas acreditadas | **506/506**, 0 fallidas/omitidas, **4 s**. |
| IT completas acreditadas | **186/186**, 0 fallidas/omitidas, **8 min 20 s**, una ejecución con límite externo 900 s; anteriores a la limpieza del diagnóstico origin. |
| Build post-limpieza | **0 warnings / 0 errores**, **32,41 s**. |
| Origin post-limpieza | **1/1**, 0 fallidas/omitidas, **5 s**; 20 admitidos a cuota/400 por nombre inválido, request 21 →429, Retry-After 1–900 s. |
| Register/composición post-limpieza | **39/39**, 0 fallidas/omitidas, **32 s**; 32 Register + 7 composición, incluido origin. |
| ST focales | **5/5**, evidencia previa conservada; no dependen del scope IT limpiado y ahora están incluidas en ST completas. |
| ST completas de este cierre | **84/84**, **0 fallidas / 0 omitidas**, **1 min 14 s**, sin filtro ni build. |

El único comando de pruebas de este cierre final es `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build`, tras build/focales post-limpieza verdes. **No se repiten UT ni las 186 IT.** La aceptación expresa del responsable combina las suites completas ya acreditadas con la revalidación focal post-limpieza y las ST completas actuales; no declara 186/186 como una nueva regresión post-limpieza. Producto y UT/ST permanecieron intactos; la limpieza afectó únicamente los dos archivos IT y restauró la ruta real. No se suman focales como casos nuevos de las suites ni se acredita rendimiento productivo.

### 50.4. Cuatro evidencias registradas y contrato HTTP

| Tarea | Evidencia separada y alcance |
| --- | --- |
| **034** | [E(T-AUTH-034), §55](specs/002-authentication/task.md#e-t-auth-034): request exacto fullName/email/password y response id/fullName/email; inputs/results saneados y AuthenticationService; D012, normalizador institucional compartido, cuota conjunta antes de defectos secundarios/hash, hasher/gate reales, UUIDv4/Active/UTC backend y timestamps iguales, sin plaintext persistido/token/auto-login; 13 UT y regresiones de §50.3. |
| **035** | [E(T-AUTH-035), §56](specs/002-authentication/task.md#e-t-auth-035): POST anónimo, 201 solo Confirmed con Mongo real/documento siete campos/hash verificable, email normalizado, 409 genérico por índice normalizedEmail, 503 Unavailable/Uncertain; sin read-before-write/retry con UUID nuevo/compensación. Problem Details seguro, correlación y no-store; sin token/cookie/Location ficticio. Incertidumbre modelada ocultando resultado de inserción real, no pérdida física de ACK. |
| **036** | [E(T-AUTH-036), §57](specs/002-authentication/task.md#e-t-auth-036): email **3/900 s**, origin **20/900 s**, una evaluación conjunta por request; 20 emails admitidos y 21.º429, 21 llamadas/21 particiones/cero inserciones; Retry-After 1–900 s y redondeo superior existente, sin penalización extendida/cargo parcial. 503 ante cuota no apta/incierta; sin devolver consumo por validación secundaria, conflicto o saturación. Logging allowlist sin email/password/hash/token/secretos. Pre-warm solo de infraestructura test-only, 250 ms intactos. |
| **037** | [E(T-AUTH-037), §58](specs/002-authentication/task.md#e-t-auth-037): composición 056/057 y Mongo/Redis reales, positivo 201/documento/hash, dos hosts/clientes independientes con email equivalente →un201/un409/una cuenta, inválidos sin escritura, mass assignment/NoSQL-like/413/415/429/503, fail-closed, no auto-login/JWT, no-store y privacidad; ST completas 84/84. |

Contrato consolidado en task §56: **POST /api/v1/auth/register**, request exactamente `fullName/email/password` strings obligatorios, success **201** con objeto exacto `id` UUID / `fullName` string / `email` normalizado. Errores **400 validation_failed**, **409 registration_conflict**, **413 payload_too_large**, **415 unsupported_media_type**, **429 rate_limited**, **503 temporarily_unavailable**; **500 internal_error solo inesperado**, sustentado por el manejo transversal existente/E056, no un fallo físico nuevo simulado para Register. **Sin 422**, accessToken o refreshToken. Los escenarios Uncertain de cuota/persistencia usan decoradores test-only con operación real y resultado confirmado ocultado; no acreditan pérdida física de ACK.

### 50.5. Revisión formal GATE-D, decisión e integridad

[Task §59](specs/002-authentication/task.md#cierre-gate-d) revisa conjuntamente **E028/E029/E030/E031/E033/E034/E035/E036/E037/E056/E057**, contra 201 confirmado, 409 genérico por índice, no auto-login/token, validación/cuotas antes del coste y hashing seguro, persistencia/cuota reales, composición/admisión fail-closed, no-store/logging seguro y pruebas positivas/negativas/maliciosas/concurrentes. No es cierre automático por asociación ni reforma los registros de BUILD-1: su Register404 histórico queda sucedido por Register503 si la autoridad no es apta y Register201 solo con controles aptos; Login/Logout siguen sin implementarse.

**BUILD-2 ACEPTADO. T-AUTH-034, T-AUTH-035, T-AUTH-036 y T-AUTH-037: COMPLETADAS / EVIDENCIA REGISTRADA. GATE-D: SATISFECHO / CERRADO, LOCAL / TEST REPRESENTATIVO. Register OPERATIVO LOCAL/TEST.** GATE-A/B/C conservan sus cierres; GATE-E/F/G/H pendientes. 008 mantiene su estado documental previo; 032/058 y JWT/Login/Logout, refresh tokens, Orders, AWS/Terraform no se inician. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**, distinto de T-AUTH-027 Mongo.

No acredita AWS/cloud deployment, Load Balancer, HTTPS público/TLS publicado, Atlas HA, Redis administrado productivo, failover real/productivo o posesión del correo institucional. Persisten enumeración residual 201/409, limitaciones NAT/origen compartido y que **uttt.edu.mx no prueba titularidad**; no se cierra seguridad integral ni aceptación externa 068/079.

En este cierre se modifican **solo task.md y memoria.md**, después del verde ST; producto intacto durante limpieza/revalidación/cierre (la limpieza técnica anterior afectó solo los dos archivos IT identificados). Program.cs solo composición y sin dominio, User/BSON/contratos Mongo/store/protocolo/Lua Redis intactos, cuotas **3/900 y 20/900**, capacidad **5000**, timeouts y **250 ms** intactos; csproj/lockfiles/fixture/imágenes/digests sin cambios, **0 paquetes nuevos**. Huella productiva fuente **idéntica al inicio y al final**: `07FE3DB35CC38C8AA02D4A3507D35A66524715109F6E2D5EBA714D65BFF695A5`. La lectura de diferencias de Program/Core/Mongo/Redis/Configuration/fixture, spec/plan, csproj y lockfiles respecto a HEAD no devuelve cambios. Spec/plan, constituciones, foundation, AGENTS/README, agentes/skills intactos. **Sin stage/commit/push**, HEAD base conservado; listo para solicitar checkpoint posterior, no realizarlo ahora. Detener sin iniciar otro gate ni generar trabajo adicional.

## 51. JWT 038–042 — Validación y cierre del checkpoint E-JWT local/test

### 51.1. Autorización y baseline

El **11 de noviembre de 2026**, el responsable autoriza sucesivamente **PRE-BUILD JWT 038–042**, **BUILD AGRUPADO JWT 038–042 / CHECKPOINT E-JWT**, **DIAGNÓSTICO DEL CLEANUP IT040 SIN TOCAR PRODUCTO** y **CONTINUACIÓN E-JWT DESDE IT040 VERDE**. El pre-BUILD concreta el diseño y el BUILD posterior aprueba exactamente paquetes/versiones, archivos, reglas temporales, políticas y composición solo-test. La autorización diagnóstica permite intervenir además en `RegistrationTestScope.cs` si se demuestra necesidad; la continuación permite041/042, regresión completa y, solo tras verde, cinco evidencias y cierre exclusivamente E-JWT. No autoriza Login/Logout/032/058 integral ni ejecución futura automática.

Baseline comprobada de esta continuación: rama **`feature/authentication`**, HEAD **`5742c6ea0d24ed44005ac3793104d413b40aa810`**, **6 modificados + 11 nuevos / nada staged**. Ese HEAD ya representa el checkpoint local posterior de BUILD-2/Register; §50 conserva el historial de la entrega anterior a aquel commit. El HEAD se mantiene y no se crea otro commit ahora. Fuente primaria: comandos/salidas reales de esta conversación; autorización, archivos, historial y comandos completos en [task §60](specs/002-authentication/task.md#e-jwt-validacion-final).

### 51.2. Historial de bloqueos y corrección final del cleanup

- Restore locked inicial: **NU1004** por las nuevas referencias; regeneración controlada únicamente de cuatro lockfiles y segundo restore locked correctos, sin alternativa instalada. Primer build: dos errores de nombres de los nuevos tests (`Log` y `AuthenticationOptions` ambiguo), corregidos en el harness; hosting de test actualizado y anotación nullable del configurador corregida sin cambio funcional. Build posterior0warnings/0errores. Se conservan estos antecedentes, no se presentan como verificaciones verdes iniciales.
- Focales iniciales **UT03816/16, UT03951/51, ST03928/28, UT04012/12** verdes. Primer **IT0402/3**, fallo del caso `Stopped_real_Redis_is_fail_closed_for_a_cryptographically_valid_identity`, **SCAN / SocketClosed** en `RegistrationTestScope.DisposeAsync`, línea original153. La segunda corrección test-only añadió espera acotada de reconexión; siguiente **IT0402/3** con el mismo fallo de teardown. Se detuvo el BUILD sin041/042 completo, regresión o cierre documental al agotar las dos correcciones autorizadas.
- Diagnóstico posterior separado: **1 caso fallido**, instrumentación temporal saneada. Confirma **200** con el JWT antes del stop; después **503**, **Cache-Control:no-store**, Problem Details `temporarily_unavailable`, sin challenge401, y **todas las aserciones funcionales pasadas**. Tras Start, la fixture verificó Redis sano: **mismo contenedor / endpoint publicado distinto / cliente anterior desconectado**. La espera venció con `TaskCanceledException`; el SCAN posterior ocultó ese error del teardown. **COMPORTAMIENTO PRODUCTIVO: PASÓ. TEARDOWN: FALLÓ. Causa E demostrada: endpoint de cleanup obsoleto**, no un "flake" ni Redis aún detenido al ejecutar SCAN. No se atribuye esa causa retrospectivamente a incidentes históricos diferentes.
- Ownership: fixture dueña del contenedor/Stop/Start; scope dueño de hosts/baseMongo/namespace de cuotas/autoridad de prueba; hostJWT dueño de sus clientes. Secuencia real: crear →200→Stop→503/no-store→Start/verificación→disponer hostJWT→SCAN del scope. El SCAN busca metadata/buckets bajo el prefijo propio de cuotas, luego borra keys y baseMongo; no es una lectura de revocación del request.
- **Una corrección estructural final**, solo `JwtCheckpointTests.cs` y `RegistrationTestScope.cs`: Stop/Start en `try/finally`; cleanup con conexión propia al endpoint actual y los **mismos timeouts**. Vía opt-in `DisposeWithRedisCleanupAsync`; disposición habitual del scope intacta. Se mantienen SCAN/borrado/DropDatabase y propagación de errores, sin catch global, omisión de limpieza o cambios de fixture; el caller dispone el cliente de cleanup. No modifica/recrea clientes de seguridad ni reabre admisión.
- Instrumentación y bucle de reconnect **retirados antes de validar**. Sin sleeps/retries nuevos. Verdes posteriores: caso aislado **1/1** (13s), tres repeticiones secuenciales **1/1** cada una (8s cada una), total **4/4 ejecuciones aisladas**; **IT0403/3** (12s), origin **1/1** (4s), Register/composición **39/39** (35s), sin fallidas/omitidas.39incluye origin y el grupoIT040 incluye el caso; no se suman como pruebas nuevas. La continuación actual no modifica nuevamente el fix.

### 51.3. Continuación y regresión íntegra del árbol final

Comandos exactos y fuente en task §60.3. Ejecución secuencial: focalUT041, focalIT041, JwtCheckpointTests completo, TokenValidationTests completo; solo tras esos verdes, restore locked/build/UT/IT/ST completas. **Ninguna corrección técnica en este turno**; no se usa la corrección adicional permitida para una hipotética regresión de harness.

| Verificación | Total / passed / failed / skipped | Tiempo informado |
| --- | --- | --- |
| UT041 | **1 / 1 / 0 / 0** | 206 ms |
| IT041 | **1 / 1 / 0 / 0** | 4 s |
| JwtCheckpointTests completo | **5 / 5 / 0 / 0** | 13 s |
| TokenValidationTests completo | **36 / 36 / 0 / 0** | 4 s |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; proyectos actualizados, lockfiles intactos durante la continuación | No se atribuye tiempo no informado |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 warnings / 0 errores** | **16,58 s** |
| UnitTests completas `--no-build` | **586 / 586 / 0 / 0** | **4 s** |
| IntegrationTests completas `--no-build` | **191 / 191 / 0 / 0** | **8 min 57 s** |
| SecurityTests completas `--no-build` | **120 / 120 / 0 / 0** | **1 min 11 s** |

Una ejecución de cada suite completa, IT con límite externo **900 s** y ST solo después de IT verdes. Las tres suites actuales corresponden al **árbol post-fix sin diagnóstico**. Focales contenidos en las suites, no conteos adicionales. Incremento: **80 UT / 5 IT / 36 ST**. Entorno local/test con .NET 10 y fixtures/imágenes/digests existentes, Redis/Mongo reales y autoridad propia en DynamoDB Local; sin recursos AWS reales ni cambios de infraestructura.

### 51.4. Cinco evidencias individuales

| Tarea / registro | Alcance acreditado |
| --- | --- |
| **038 — [E038, task §61](specs/002-authentication/task.md#e-t-auth-038)** | JwtBearer **10.0.14** + JsonWebTokens **8.19.2**, dos referencias directas del mismo stack JsonWebTokenHandler, cuatro lockfiles; System.IdentityModel.Tokens.Jwt8.19.2 solo transitivo y sin handler legacy. `IAccessTokenIssuer.Issue(Guid)`/resultado Issued-Unavailable sin token en ToString; JwtTokenIssuer y JwtKeyRing snapshot/lookup exacto/sin fallback remoto. Claves externas Base64 canónica≥32bytes, kids/activa válidos, no HMAC reutilizada. Payload exacto sub/jti/iss/aud/iat/exp; UUIDv4D minúsculo, JTI RNG32bytes/Base64url43 sin padding y nuevo por token. Una lectura de reloj, iat entero floor, exp=iat+900, sin PII/nbf automático/refresh ni persistencia token/JTI. **16/16 UT** previas, incluidas en586/586actuales. Emisión de fixtures, no autorización pública/flujo049. |
| **039 — [E039, task §62](specs/002-authentication/task.md#e-t-auth-039)** | JwtBearerConfiguration/JwtClaimsValidator; MapInboundClaims=false/SaveToken=false, sin metadata/discovery/keys remotas, HS256/kid/firma/issuer/audience estrictos.4096bytes previos a decode/cripto/Redis; JSON/UTF-8/tipos/duplicados incluidos escapes/allowlist/canonicalidad. Preparse solo rechaza; feature confiable solo tras validación completa. LifetimeValidator explícito:exp>iat, diferencia900, iat≤now+S, **now<exp+S**; nbf opcional entero≤now+S y≤exp. Fronteras exactas con skew0/30. **51/51 UT +28/28 ST focales**, y36/36ST JWT completas actuales. |
| **040 — [E040, task §63](specs/002-authentication/task.md#e-t-auth-040)** | Default/fallback, RevocationAuthorizationHandler y AuthenticationAuthorizationResultHandler; identidad autenticada del mismo principal, estado verificable y no revocación, sin conceder permisos de negocio automáticamente. Revoked→401; no verificable/Unavailable/Uncertain→503/no-store. Excepción solo política/metadata/POST/ruta exacta Logout, sin endpoint/escritura nueva/204. **12/12 UT, IT0403/3**, actuales191/191IT y STJWT; historial del cleanup conservado en §51.2, no imputado a producto. |
| **041 — [E041, task §64](specs/002-authentication/task.md#e-t-auth-041)** | Procedimiento manual: distribuir/reconocer A+B antes de B activa, acreditar cese real A, coexistir y retirar. Retención **900+30=930s**, sin sumar margenRedis60ni convertir en990; TTLRedis intacto. A930menos un tick:puerta false/tokenA válido; a930:puerta true/tokenA expirado; B nueva válida tras retiro. **1/1UT+1/1IT**, TimeProvider controlado/sin espera930s. Kid activo inexistente/vida901/skew incompatible rechazados en UT038 reutilizadas. La puerta no acredita cese global ni impide una eliminación administrativa fuera del procedimiento. Sin rotación automática, periodicidad o AWSmanager. |
| **042 — [E042, task §65](specs/002-authentication/task.md#e-t-auth-042)** | **5/5IT**, emisor/JwtBearer reales, dos hosts lógicos con rings/clientes propios, validación cruzada y sesiones/JTI distintos; rotación/retiro, Redis real, marca017desde setup→401, parada→503 y decorador test-only tras lectura real→Uncertain/503 (no pérdida físicaACK). **36/36ST**, malformed/tamaño/tampering/alg/kid/claims/tiempo/header adversarial, PII ausente, safe challenge/no-store, logs capturados sin token/keys/sentinela password/hash y misuseLogout rechazado. Register201real y ausencia de Login/Logout productivos; regresiónRegister completa incluida en191/191. PipelineJWT exclusivamente test-only, no058productivo. |

### 51.5. Decisión e integridad

Revisión explícita de las cinco evidencias y las suites actuales en [task §66](specs/002-authentication/task.md#cierre-e-jwt), no aceptación por presencia de archivos. **T-AUTH-038/039/040/041/042 COMPLETADAS / EVIDENCIA REGISTRADA. CHECKPOINT E-JWT SATISFECHO / CERRADO — LOCAL/TEST REPRESENTATIVO. GATE-E completo PENDIENTE / NO SATISFECHO. GATE-F PENDIENTE / NO SATISFECHO.** A/B/C/D conservan cierres; G/H pendientes. El checkpoint habilita solicitar el siguiente incremento, no implementarlo automáticamente ni crear un commitGit.

**Sin 032, Login 047–051, Logout 043–046, 058 integral, refresh tokens, Orders, AWS/Terraform.** La brecha 032 de hashing antiguo/CAS sigue pendiente antes de Login posterior a GATE-F; 047 depende de 046 sin cambios. 008 mantiene startup productivo pendiente. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**, distinto de T-AUTH-027 Mongo. No acreditación de dos máquinas/LB, TLS público, Atlas HA, Redis administrado, failover/continuidad productivos, titularidad de correo ni seguridad universal. Poseedores HS256 pueden emitir; JTI no evita replay por sí solo. Los estados de ausencia JWT de §50 son antecedentes, no el estado posterior de §51.

**Program.cs, composición productiva, Register, BSON/User, Mongo, Redis/protocolo/Lua, cuotas/capacidad 5000/timeouts (incluidos 250 ms) intactos respecto a HEAD**, sin UseAuthentication/JwtBearer productivos nuevos; 058 conserva wiring/selección por endpoint/pipeline integral. Ningún secreto real hardcoded. Durante esta continuación no se alteran fuentes/tests/paquetes/configuración: huella agregada SHA-256 antes/después de pruebas **`8466A66EE9006B1BE97706D7E6B4927C3BD0EFB00BEF76A1BFD8D537F941C8A2`**, método/alcance en task §60.4; no hash de commit ni escaneo de secretos. Fixtures/shared, imágenes/digests, spec/plan, constituciones, 001, AGENTS/README y agentes/skills conservados.

Entrega acumulada: **8 modificados + 11 nuevos = 19 archivos**, incluidos los 18 iniciales y `RegistrationTestScope.cs` autorizado después; este turno solo actualiza task/memoria tras verde. **Sin stage/commit/push**, HEAD intacto. Detener sin iniciar Logout, Login, otro gate o checkpoint Git.

## 52. Logout: cierre local/test de GATE-F y correctivo GATE-B

### 52.1. Autorización y baseline

El **11 de noviembre de 2026**, tras focales autorizadas de Logout043–046 y del correctivo Redis, el responsable autoriza regresión final/cierre, diagnóstico focal de Register Uncertain y posteriormente **AUTH GATE-F — REINTENTO DE IT COMPLETAS TRAS DIAGNÓSTICO REGISTER UNCERTAIN**. La última solicitud conserva UT633/633, exige build→IT completas→ST completas y solo tras verde permite registrar E043–E046/consolidar GATE-B/revisar F. Admite cambios test-only únicamente ante nueva causa demostrada. No autoriza Login/032/047–051/058, stage/commit/push.

Baseline comprobada: SDK **10.0.403**, rama **`feature/authentication`**, HEAD **`f2335b0f95bed02edfd2360939fc66954c1a9497`**, **19 modificados + 7 nuevos / 0 staged**, diff tracked **+605/−47**. Se conserva ese trabajo técnico anterior; no se reimplementa ni se descarta. Fuentes/archivos/comandos/resultados e incidentes separados en [task §§67–73](specs/002-authentication/task.md#logout-validacion-final). Los estados de no inicio de Logout de §51 corresponden a aquel incremento E-JWT, no al cierre posterior de este §52.

### 52.2. Historial preservado — distinto del estado final

- **Antecedentes comunicados por el responsable:** CS0104 UT043 resuelto mediante alias test-only; dos fallos de setup IT043; challenge401 faltante diagnosticado como selección de `DefaultApiProblemDetailsWriter`, con fix central en ApiProblemDetails antes de delegar y en writer; antiguas expectativas Logout404 sustituidas por Logout401/Bearer sin autenticación. No se inventan valores de los dos setup ni se atribuyen a producto sin captura. El fix central mantiene un Bearer sin parámetros, no relaja JWT ni agrega Bearer a otros errores.
- **Origin20/900:** historial de §50 conservado, pre-warm Redis test-only previo al lifecycle, sin cuota/EVAL/retry funcional ni cambios de 250 ms.
- **Mongo025:** observación de metadata transitoria tras cancelación no equivale a índice utilizable. El test preexistente exige Incompatible con11000 o Unavailable con cancelación acreditada, nunca Ready retrospectivo; originales preservados, hint rechazado y ausencia estable de build/índice compatible. Polling read-only condicionado de2s/quiet100ms/muestras25ms y observación de carrera5s son del harness, no nuevo timeout/retry del initializer; la segunda inicialización es verificación independiente explícita. Producto/500ms intactos.
- **Redis capacity/state:** captura real previa acreditada por el responsable: raw `['enver.auth.quota','1','unavailable','state','0','0']`, Redis TIME retrocedió **−7887µs**, condición anterior `now < meta.last`. Único fix Redis: `effectiveNow=max(redisNow,meta.last)` o redisNow sin metadata; detalle en §52.3. Incidentes antiguos sin captura no se reclasifican retrospectivamente.
- **Capacity R9 interrumpida:** timeout externo900s del agrupador después de ocho rondas verdes; no es RequestBudget/dependencia/timeoutRedis/falloLua. R9 y R10 posteriores individuales verdes completan10/10,20/20; el intento sin TRX no cuenta como ronda completa y su historia permanece.
- **Primera validación final:** restore locked correcto/4 lockfiles intactos, build0/0(23,17s), UT633/633(5s), IT258/259(10min16s), una fallida/0 omitidas: Register `Uncertain/documents=1`, EqualException saneada. Se detuvo sin ST/docs; comparación/causa histórica **NO DETERMINADAS**.
- **Diagnóstico separado Register:** primer intento falló antes del cuerpo por startupMongo exit48/ADDRESS_IN_USE/contenedor no ejecutándose, sin atribuirlo al rojo histórico. Siguiente caso instrumentado1/1: HTTP503/code temporarily_unavailable/no-store, documentos0→1, único ID de solicitud coincidente, una llamada al puerto y una inserción inner Confirmed→probe Uncertain, sin retry aplicativo. Delete ausente por puerto/call-sites, no medición wire. Todas las etiquetas pasaron; no expectativa obsoleta o defecto productivo demostrado. La observación puede alterar tiempos: no prueba causa del rojo. Se retiró la instrumentación y restauraron206/206 hashes; buildIT0/0(13,47s), sin fix permanente Register/Scope.
- **Nueva ST roja y causa demostrada en este reintento:**145/146,1fallida/0omitidas(1min51s). `RegistrationSecurityTests.Structured_events_only_accept_closed_enums_and_emit_no_request_labels` asumía todo el producto cartesiano de enums y formato único Register; Logout/Admitted se rechaza correctamente por la allowlist específica. Bajo la excepción autorizada se corrige **solo `tests/Enver.API.SecurityTests/RegistrationSecurityTests.cs`**: cobertura Register intacta, cinco etiquetas Logout exactas y un caso de rechazo de otras combinaciones/enums sin emitir eventos, validando claves/mensaje/EventId cerrados. Producto intacto. Focal11/11 y STfinal152/152; se conservan los dos runs, no se esconde145/146.

### 52.3. Consolidación correctiva de GATE-B

[Task §68](specs/002-authentication/task.md#gate-b-correctivo-logical-clock) añade evidencia al cierre original de §45 de esta memoria y task§38, **sin reabrir/rehacer GATE-B**. Redis TIME es distribuido pero de pared, **no estrictamente monotónico**. El Lua `ConsumeAuthenticationQuota.lua` validado usa **effectiveNow=max(redisNow,meta.last)**, o redisNow sin metadata, para ventanas/eventos/horizontes/expiraciones. Retry-After conserva espera de pared incluyendo rollback pendiente; protocolo de6campos, una lecturaTIME, rangos/overflow y fail-closed estructural intactos.

Focales ya acreditadas: **UTRedis90/90, ITRedis51/51, rollbackIT26/26, distribuidaGATE-B19/19, STGATE-B46/46, Registerquota6/6, Logoutquota8/8**. Capacity: **R1–R10 completas10/10,20/20 pruebas,0failed/0skipped**; R9individual36s(TRX106,610s),R10individual29s(TRX94,368s), sin repetirR1–R8. Evidencias en `%LOCALAPPDATA%\Temp\opencode\redis-clock-validation-20261111-141608`; R9sinresultado se conserva aparte.

Se acredita, en el modelo ensayado: meta.last no decrece; horizon/TTL no se reducen **por rollback**; cuota no se abre; Retry-After conservador; capacidad5000 intacta; rechazo sin mutación y corrupción estructural fail-closed. Los26casos deterministas ejecutan cuerpoLua productivo con shimTIME/traslación de época de expiraciones solo-test, sin cambiar condiciones ni reloj del servidor. La regresión actual confirma clock26/26/capacity2/2, sinstateinesperado. No es garantía universal temporal/infraestructura ni solución a pérdida silenciosa. **GATE-B: CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA**.

### 52.4. Validación final y evidencias individuales

| Validación | Resultado final / duración informada |
| --- | --- |
| SDK | **10.0.403**. |
| Build solución `--no-restore` | **0warnings/0errores,19,37s**. |
| UT completas previas conservadas | **633/633,0failed/0skipped,5s**; no se repiten. |
| IT completas actuales, sin filtro/StopOnFail | **259/259,0failed/0skipped,10min30s**; TRX636,779s. |
| Build tras único fix ST test-only | **0warnings/0errores,17,44s**. |
| Focal RegistrationSecurityTests post-fix | **11/11,0failed/0skipped,172ms**. |
| ST completas finales post-fix | **152/152,0failed/0skipped,1min44s**; TRX113,546s. |

Relación temporal explícita: build→ITverdes→STrojas→fix de un solo archivoST→build/focalST→STcompletasverdes. No se repitenIT niUT completas tras el fix porque producto/UT/IT quedan intactos; la suiteSTfinal sí corresponde a su árbol corregido completo. xUnit2031 no se corrige; warninghistórico conservado, builds actuales incrementales0warnings. No se ejecutan más rondas focales de capacidad.

Casos vigilados: Persistence **Unavailable/0 y Uncertain/1 Passed**, respectivamente1,383684s/1,292674s; Redis capacity **2/2** (Exactly_2500...11,745811s), Mongo025 **11/11** (duplicados0,907639s, Incompatible original sin cancelación y segunda verificación11000), Origin20/900 **Passed**4,724051s, Registerpositivo **Passed**1,430343s, Composition **7/7**, LogoutIT **42/42**, ClockIT **26/26**. LogoutST **26/26** y RegistrationSecurityTests **11/11** en ST152. No reaparecieron fallo RegisterUncertain ni `unavailable/state` inesperado; corrupción intencional sigue verificando fail-closed.

| Evidencia separada | Capacidad acreditada |
| --- | --- |
| [E043, task§69](specs/002-authentication/task.md#e-t-auth-043) | Identidad/exp autenticados, LogoutAsync, Confirmed único éxito, una escritura por intento sin read-back/retry/Mongo; repeat/concurrent, Redisvalue1/TTLexp+skew30+margen60 no decreciente; NoOperation/Unavailable/Uncertain503, presupuesto/cancelación.10UT+6ITtrait043. |
| [E044, task§70](specs/002-authentication/task.md#e-t-auth-044) | POST/api/v1/auth/logout, Authentication.Logout, AuthorizationBearerúnico,204vacío/no-store, cuerpo no vacío400/>16KiB413, inválido401/Bearer exacto sinerror_description, noverificable503, excepción de revocado solo método/ruta/política exactos y fixcentralApiProblemDetails.8UT+13ITtrait044 y regresiones Challenge401. |
| [E045, task§71](specs/002-authentication/task.md#e-t-auth-045) | Account30/60s/origin300/60s, evaluación conjunta/cargoúnico/no parcial/429/Retry-After, Unavailable-Uncertain503, consumo preservado ante fallos posteriores; eventos/logs allowlist sinJWT/sub/jti/IPraw.4UT+8ITtrait045 y ST. |
| [E046, task§72](specs/002-authentication/task.md#e-t-auth-046) | Dos hosts lógicos, A revocado en1→rechazo en2/Bintacto; repeat/concurrent, Rediscompartido/value1/expiración no decreciente; no-confirmación503, autoridadfail-closed, paradaRedis, Mongoindependiente, policyisolation y STadversariales.10ITtrait046+26STLogout. |

Fuente primaria: salidas de sesión y directorio externo `%LOCALAPPDATA%\Temp\opencode\gate-f-it-retry-20261111-153827`, logs/TRX/manifiestos. Comandos exactos y fuentes previas en task§67.4. No son artefactosCI ni se inventa garantía de retención. La respuesta perdida se modela mediante probe que oculta confirmación real; no prueba pérdida físicaACK/TCP.

### 52.5. Revisión formal, estado e integridad

[Task§73](specs/002-authentication/task.md#cierre-gate-f) revisa explícitamente E043–E046 frente a204soloConfirmed, idempotencia/TTL, inválido401+Bearer/noverificable503, A revocado/Bintacto cross-host, excepción exclusiva, Mongoindependiente, cuotas/logs seguros, ausencia de read-back/retry, logicalclockvalidado y suitesfinalesverdes.

**T-AUTH-043/044/045/046: COMPLETADAS / EVIDENCIA REGISTRADA. GATE-F: SATISFECHO / CERRADO — LOCAL/TEST REPRESENTATIVO. GATE-B: CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA.** GATE-A/C/D y checkpointE-JWT conservan cierres. **GATE-E completo PENDIENTE:**032/047/048/049/050/051/058 no iniciadas. GATE-G/Hpendientes;008mantiene su pendiente de startup productivo. **Register completo en su aceptación local/test previa; JWT038–042 completo como checkpoint local/test; Logout completo local/test; Login pendiente; Authentication NO TERMINADA.**

Antes de documentar, comparación de206archivos confirma que durante el reintento solo cambió RegistrationSecurityTests.cs. **Program.cs, JWT038–042, Register productivo, BSON/User/Mongo, Redis protocol/store/Lua ya corregido, capacity5000, timeouts, RequestBudget, connectionpolicy, global.json/rollForward, csproj/cuatro lockfiles intactos;0paquetes nuevos.** El único cambio Redis contraHEAD sigue siendo logicalclock autorizado; no hubo otro fix productivo. Tests/Shared y modificacionesLogout previas se conservan. Tras verde se escriben únicamente task/memoria además del fixST ya validado. Spec/plan, constituciones/foundation, AGENTS/README, agentes/skills intactos; sininstrumentación temporal nueva.

No se acredita058integral/pipelineJWTproductivo final, despliegue, dos máquinas/LB, TLS público, AtlasHA, Redisadministrado, AWS/IAM/ACL/Terraform, failovercontinuidadproductivos ni seguridaduniversal. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**; pérdida totalmente silenciosa no observable residual no eliminada. **Sin stage/commit/push**, HEADbaseconservado. Detener para solicitar checkpointGit posterior, sin iniciarlo ni ejecutar032/Login047–051/058 u otro bloque.

## 53. BUILD autorizado 032 +047–051 — Login y rehash, GATE-E pendiente

### 53.1. Autorización, baseline y decisiones

Solicitud **AUTH FINAL — BUILD 032 +047–051**, secuencia obligatoria032→047→048→049→050→051, focales por fase y regresión completa antes de escribir evidencias. Baseline comprobada: **feature/authentication**, HEAD **597619c0708290dcc749db687905de70171b9555**, árbol limpio, SDK **10.0.403**, Windows local. El checkpointGit de Logout ya existe; el presente bloque no lo modifica ni autoriza otro. **058 excluida, Program.cs intacto, sin stage/commit/push.** Alcance y comandos en [task§74](specs/002-authentication/task.md#login-validacion-final).

Decisiones expresamente aprobadas: PasswordHasher<User> existente, **no BCrypt**; rehash con límites de verificación sin cambiar mínimos Register; **un CAS**, solo Conflict permite **una relectura y una reverificación**, otro SuccessRehashNeeded→503 sin segundo CAS. Disabled verifica hash real,401 genérico, sin rehash/CAS/JWT; NotFound verifica dummy previo una vez, sin segunda lectura. Cuotas conjuntasLogin5/10/60 por60s antes de lookup/KDF segúnD012. Contrato real de IUserStore/MongoUserStore confirmado compatible antes de047, sin modificación productiva silenciosa.

**Excepción explícita posterior:** se pregunta por la inclusión del nuevo archivo Shared en proyectos que enumeran enlaces Compile. El responsable autoriza solo esos enlaces: **una línea en IntegrationTests.csproj y una en SecurityTests.csproj**, sin propiedades, paquetes, versiones o lockfiles; UnitTests.csproj intacto. No se interpreta como permiso general para editar proyectos.

### 53.2. Implementación y evidencias separadas

| Evidencia / estado COMPLETADA LOCAL/TEST | Capacidad acreditada y enlace |
| --- | --- |
|**E032**|[Task§75](specs/002-authentication/task.md#e-t-auth-032): PasswordRehashCoordinator, Rehash específico, snapshotActive/proof previo/mismo budget, un CAS _id/hash/status sin upsert y solo hash/updatedAt UTC milisegundos; Conflict acotado, Unavailable/Uncertain503 sin retry/read-back,18UT/10ITRehash y carrerasHTTP.|
|**E047**|[Task§76](specs/002-authentication/task.md#e-t-auth-047): LoginRequest/Response exactos, LoginInput/Result tipados/LoginAsync, normalizador existente, passwordcorto/Unicode/espacios sin transformaciones, D012 y separación de errores;15UT focales iniciales, ampliadas en32UTLogin.|
|**E048**|[Task§77](specs/002-authentication/task.md#e-t-auth-048): dummy preinicializado una vez/NotFound jamás autentica, Disabledverificareal sinCAS/JWT,401uniforme/challengeBearerúnico, crypto/readunavailable503 y rehash032;11UTtrait048 y pruebas conKDFreal. No constant-time ni umbral arbitrario.|
|**E049**|[Task§78](specs/002-authentication/task.md#e-t-auth-049): POST/api/v1/auth/login/AllowAnonymous/metadataLogin,200exacto/no-store, BeginEmissionlease→issuerexistenteuna vez→checksfinales→Complete/Fail; no devuelve token si falla control posterior ni compensa horizonte;6UTemisión yJWT/sesionesA/B reales.050 integrada antes de aceptación.|
|**E050**|[Task§79](specs/002-authentication/task.md#e-t-auth-050): origin+email5/60,email10/60,origin60/60; evaluación/cargo conjunto único/sinparcial/refund,429yRetryAftermáximo conservador,503si noverificable;6ITy9STesquemas. Un terminalLogin porintento incluidos413/415 sin duplicar aplicación; labels cerrados sin secretos/PII/motivos privados.|
|**E051**|[Task§80](specs/002-authentication/task.md#e-t-auth-051): LoginTestScope/UT/IT/ST, Mongo/Redis/hasher/dummy/JWT/autoridad existentes reales/locales, contratos/races/dependencias/D012/sesionesLogoutA/Bintacto/seguridad.25ITLogin,62STLogin+9esquemas; regresión completa verde.|

### 53.3. Secuencia, incidencias y regresión

Focales iniciales:032UT25/25 eIT10/10;047UT15/15;048UT36/36;049Login/JWT112/112;050IT5/5 yST20/20.051ITinicial33/33. Se conservan incidencias, sin ocultar runs rojos:

- 049: fallo de compilación del nuevo test por tres argumentos omitidos del estado de autoridad; solo se corrige constructor del test, no el contrato.
- 051UT57/58: nuevo test descubre inicializaciónLazy por invocación directa de VerifyDummy. Corrección autorizada en PasswordService comprueba IsInitialized antes de acceder; devuelveUnavailable sin generar dummy por request. Preflight ya lo exige. DummyPasswordHash/HashRegister intactos. UTfocal posterior58/58.
- 051ST65/66: vector surrogateaislado\\uD800 esperaba cargo secundario; JSONestricto no lo materializa y lo declara ilegible. Se corrige solo expectativa a cerocargo, sin relajar Unicode/frontera/producto. STampliada71/71.
- FocalITampliado34/35: MongoConnectionException en rehashConflict+Success, sin operación original identificada ni defecto productivo acreditado. Se conservan rojo y límites. Marcadores constantes de fase solo en el nuevo test, sin datos sensibles; diagnóstico10/10 y focalITaislado35/35. **Causa histórica no acreditada**; no se atribuye a reloj/carga ni se cambian timeouts/retries/sleeps.
- xUnit2031histórico de LogoutTests:166 aparece al recompilarUT; no se corrige. Buildfinalincremental0warnings no afirma eliminación de ese warning en rebuild.

| Regresión completa final secuencial | Resultado |
| --- | --- |
|Restore solución --locked-mode|Correcto;cuatro lockfiles idénticos a baseline.|
|Build solución --no-restore|**0warnings/0errores,13,29s**.|
|UT completas|**684/684,0failed/0skipped,6s**.|
|IT completas|**294/294,0failed/0skipped,11min6s**.|
|ST completas|**223/223,0failed/0skipped,2min51s**.|

Fuente primaria: `%LOCALAPPDATA%\Temp\opencode\login-build-20261111-173108`, logs/TRX de todas las etapas y manifiestos. **219/219 archivos idénticos durante restore/build/regresión** (206baseline+13nuevos). Desglose y comandos en task§74.3. Regresiones verdes: Register32IT/13UT/RegistrationSecurity11, Composition7, JWTCheckpoint5/JwtPolicy80/TokenValidation36; **Logout42IT/37UT/26ST; Redisclock26/capacity2IT**. RegisterUnavailable/Uncertain,201yOrigin20/900 pasan; no se explica retrospectivamente el EqualException previo. No se repiten/aumentan las diez rondas históricas de capacidad de§52/task§68.

### 53.4. Integridad, límites y detención

**13 archivos creados,18 técnicos modificados y solo task/memoria documentados tras verde:33 archivos en total**, todos sin staging. Inventario porcomponente en task§74.4 y evidencias. Los dos csproj cambian exclusivamente +1/-0 por enlace autorizado; **0paquetes nuevos**. Tests históricos de composición/JWT reconocen ruta preparada sin acreditar058; RegistrationSecurity mantiene familias cerradas, no un cartesiano universal.

Verificación final: únicamente task/memoria cambian tras regresión, otros217 archivos idénticos; **git diff --check exit0** (solo avisos LF→CRLF), nuevos archivos sin whitespace final. **git status20M/13??,0staged**, rama/HEADconservados; capturas `final-diff-check.log`/`final-git-status.txt` en el directorio externo indicado. Sin commit/push.

**Program.cs, spec.md, plan.md, constituciones/001, AGENTS/README, global.json, solución, Enver.API.csproj/UnitTests.csproj, cuatro lockfiles, opciones/appsettings, timeouts, RequestBudget, hashingconcurrency, capacity5000, MongoUserStore/IUserStore/User/BSON/índices, Redisstore/protocolo/Lua(logicalclockincluido), JWT038–042 productivo, Docker/CI/AWS/Terraform intactos.** No subagentes/skills nuevos/invocados ni cambios fuera del bloque. Un observador de eventos Login en composición cubre rechazos de frontera; no activa autenticación JWT productiva ni modifica Program.

Los hosts de TestServer componen JWT explícitamente solo-test y comparten Mongo/Redis/DynamoDBLocal con estado local independiente. MapControllers descubre Login preparado: no se promete invisibilidad ni despliegue autorizado. Incertidumbre porprobe oculta confirmación real, no pérdida físicaACK; privacidad probada solo en canalescapturados. No dosmáquinas/LB/TLSpúblico/AtlasHA/Redisadministrado/AWS/IAM/ACL/Terraform/failoverproductivo ni garantía universal temporal o de seguridad.

**032/047/048/049/050/051 COMPLETADAS / EVIDENCIA REGISTRADA — LOCAL/TEST REPRESENTATIVO. GATE-E PENDIENTE / NO SATISFECHO por058;058 PENDIENTE / NO EJECUTADA, NO IMPLEMENTADA.** GATE-B **CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA**; GATE-D/F y checkpointE-JWT conservan cierres; A/C conservados, G/Hpendientes,008mantiene startupproductivopendiente y058–079son22tareasnoejecutadas. **Authentication NO TERMINADA; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**, pérdida totalmente silenciosa noobservable residual no eliminada.

**Sin stage/commit/push**, HEAD **597619c0708290dcc749db687905de70171b9555** conservado; árbol con cambios autorizados sin staging. **Detener**, sin058, otro incremento, pipeline productivo final o checkpointGit automático.

## 54. BUILD 058 — Pipeline productivo activado; cierre bloqueado por regresión IT

### 54.1. Autorización y baseline

El **11 de noviembre de 2026**, **AUTH FINAL — BUILD T-AUTH-058 / CIERRE GATE-E** autoriza únicamente058, focales y regresión completa, con E058/cierre de GATE-E **condicionados a toda la regresión verde**. Baseline verificada: **feature/authentication**, HEAD **995fa9c1c604f20e2cb941af196829ecbc35c8e4**, árbol limpio, SDK **10.0.403**, Windows local. Ese HEAD ya contiene el checkpoint posterior de Login/rehash; las ausencias/prohibiciones de058 de§53 son antecedentes, no el alcance de esta solicitud posterior. Sin stage/commit/push. Fuentes, archivos, comandos y resultados detallados en [task§81](specs/002-authentication/task.md#pipeline-058-bloqueado).

### 54.2. Implementación técnica acotada

- **Program.cs ahora modificado:** `AddAuthenticationPipeline()` reutiliza componentes existentes; `UseAuthentication()` precede al único `UseAuthorization()` y al único `MapControllers()`. Foundation mantiene routing/límites/presupuesto antes de autenticación. `/health` es anónimo explícito, conservando liveness, sin readiness nuevo.
- Composición registra **un solo Bearer**, configurador/validador existentes, default/fallback normal segura, handler de revocación y writer de autorización existentes; Logout protegido por `Authentication.Logout`. No otro issuer/keyring/quota store/password service/authority.
- ConfiguradorBearer omite parsing/criptografía/revocación del Bearer opcional **solo con IAllowAnonymous + metadata Register/Login**; no bypass de Logout/protegidos. HS256/issuer/audience/kid/claims/900s/skew30/tamaño4096/duplicados intactos. `AuthenticationPolicies.cs` cambia solo comentario, no reglas.
- Inicialización existente de dummy/Mongo/índice/detectorRedis y Options/ValidateOnStart/materialJWT reutilizada. El host no crea metadata de cuotas/authority ni otro inicializador; estado no apto mantiene fail-closed. Alta de autoridad/cuotas es preparación explícita del test, no bootstrap productivo.
- Nuevo **AuthenticationPipelineTests.cs**,16casos sobre **Program real con WebApplicationFactory**, Mongo/Redis reales y DynamoDBLocal. Superficie protegida solo del assemblyIT/factory058, sin endpoint productivo nuevo. Cubre públicos anónimos y Bearer opcional sin trabajoJWT/lecturaRevocación, cargo único, tokenLogin válido, Logout401/204/reconfirmaciónrevocado, fallback/default/normal y excepción no copiable, JWTinválido/revocado401, revocación/authorityUnavailable-Uncertain503, challengeBearerexacto/no-store/413/415, singleton/scheme e inicialización sin bootstrap. Captura logs sin password/hash/token/email/sub/jti/claves/conexiones en los canales comprobados. Probes ocultan respuestas de adapters reales, no acreditan pérdida físicaACK.
- Actualizados tres IT históricos: **AuthenticationDependencyCompositionTests.cs**, **JwtCheckpointTests.cs**, helper invocado en **RegistrationTests.cs**. Ahora exigen Bearer único y fallback segura. Solo el probeIT057 de dependencias es anónimo explícito, sin relajar políticas productivas ni expectativas de dependencia. Requests nuevos058 con budgets originales fixture1000/1000/500; setupLifecycle8000 fuera del request. No cambiar timeouts productivos ni harnessLogin previamente aprobado.

### 54.3. Validación y antecedentes rojos conservados

1. Build inicial con CS1061/nullable del nuevo test; corregidos solo usingReflection/anotaciónnullable. xUnit2031histórico LogoutTests:166 conservado; buildsincrementales posteriores0warnings no prueban su eliminación en rebuild.
2. Focal058 **16/16**. Primer grupoauth **0/106** por setupMongo: mongo-start/exit48/ADDRESS_IN_USE, sin cuerpos de tests; un reintento sin cambios **103/106**, tres probes057 reciben401 por fallback antes de su admisión. Se hace explícito AllowAnonymous solo en ese controladorIT.
3. Grupo post-fix **121/122**:05816/Register32/Login25/composición7 verdes; Logout41/42, una MongoConnectionException/TimeoutException en **DropDatabaseAsync del teardown**, no falloHTTP identificado. Sin cambiar scope/timeouts, revalidación Logout completa **42/42**; no se atribuye causa raíz del timeout.
4. JWT/dependencias **51/51** (JwtCheckpoint5/revocación7/clock26/capacity2/Mongo02511); UTpolicies/revocación **91/91**; TokenValidationST **36/36**. Origin20/900 pasa dentro deRegister32 y posteriormente en la suiteIT.

| Regresión completa secuencial | Resultado actual |
| --- | --- |
| Restore locked | Correcto, cuatro lockfiles intactos. |
| Build solución sin restore | **0warnings/0errores,6,26s**. |
| UT completas | **684/684,0fallidas/0omitidas,5sCLI**. |
| IT completas | **304/310,6fallidas/0omitidas,10min16sCLI**,623,378sTRX; una ejecución, límite externo900s. |
| ST completas | **NO EJECUTADAS por IT rojas**.223/223previo no se reutiliza como resultado del árbol actual. |

Seis fallos: dosCASMongo (`Real_cas_updates_only_hash_and_millisecond_timestamp_with_effective_concerns`, `Independent_clients_race_one_winner_and_one_conflict_without_overwrite`) y cuatroMongo025 (`Creation_effective_inspection_second_initialization_and_duplicate_rejection`, incompatibleindex **partial/wrong-key/descending**). CAS informa MongoConnectionException saneada sin identificar operación original; creation captura initializerUnavailable tras createIndexes cancelado/TaskCanceledException con **7476ms de RequestBudget8000 restantes/CauseNone**. Metadata compatible posterior no prueba readiness retrospectiva. En tres incompatibles, MongoConnectionException/TimeoutException en setup/create-incompatible-index. No se acredita causa común, defectoCAS/productivo, agotamientoRequestBudget o relación causal con058; no reinterpretar errores históricos.

En IT completas pasan05816/16/Register32/32/Login25/25/Logout42/42/composición7/7/JwtCheckpoint5/5/revocaciónRedis7/7/clock26/26/capacity2/2; Mongo0257/11 yCAS4/6 bloquean aceptación. No otra corrección, instrumentación, repetición de IT completas o ST completas tras el rojo.

Fuente primaria externa: **`%LOCALAPPDATA%\Temp\opencode\auth058-20261111-210837`**, logs/TRX/filtros/manifiestos; no garantía de retención ni evidenciaCI. **220/220 archivos idénticos durante restore/build/suites**; solo después se escriben task§81/estado058 y esta nota, **no E058 ni cierre**.

### 54.4. Decisión, integridad y detención

**T-AUTH-058 EN CURSO / IMPLEMENTACIÓN TÉCNICA PREPARADA / VALIDACIÓN BLOQUEADA; NO COMPLETADA. E058 NO REGISTRADA. GATE-E PENDIENTE / NO SATISFECHO. Authentication NO TERMINADA.** No aceptar el incremento por focales verdes mientras la regresión completa esté roja. Se conservan E032/E038–E042/E043–E046/E047–E051, GATE-B/D/F y checkpointE-JWT históricos; A/C conservados, G/H pendientes. 008 conserva su estado previo;059–079 no iniciadas.

Entrega **10 archivos:1ITnuevo+9modificados**, cuatro productivos descritos, tresIThistóricos y dosdocumentos. **Cero paquetes nuevos**; csproj/lockfiles/fixture/imágenes/digests/solución/global.json intactos. No modificación de AuthenticationService/PasswordRehashCoordinator/PasswordService/MongoUserStore/User/BSON, stores/protocolo/LuaRedis/logicalclock/capacidad5000, contratosJWTclaims/keyring, Options/appsettings/timeouts/RequestBudget/hashingconcurrency. Sin retries/sleeps/diagnósticos temporales nuevos ni secretos reales añadidos.

Spec/plan, constituciones/001, AGENTS/README, agentes/skills intactos. No Orders,059, infraestructura/Terraform/CI/CD/AWS/despliegue. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**, sin claims de cloudproductionready/dosmáquinas/LB/TLSpúblico/AtlasHA/Redisadministrado/failoverproductivo o eliminación de pérdida silenciosa residual. **Sin stage/commit/push**, HEAD **995fa9c1c604f20e2cb941af196829ecbc35c8e4** conservado. Detener para decisión delimitada sobre validaciónMongo, sin fixes ni nuevas ejecuciones automáticas.

## 55. Revalidación final, E058 y cierre local/test de GATE-E

### 55.1. Autorización, baseline y entorno

El **12 de noviembre de 2026**, **AUTH FINAL — REVALIDACIÓN COMPLETA PARA CIERRE DE GATE-E** autoriza una ITcompleta `--no-build`, después una STcompleta solo conIT310/310 y cierre documental únicamente tras verde. **Sin cambios de código/tests/configuración/dependencias**, sin repetirUT, sin suites paralelas, sin repetición deITtrasrojo ni fixes. Solo este archivo y `specs/002-authentication/task.md` se actualizan después de confirmar la regresión. El bloqueo de§54 ytask§81 permanece como antecedente, no estado vigente.

Baseline **feature/authentication**, HEAD **995fa9c1c604f20e2cb941af196829ecbc35c8e4**,12modificados+1nuevo/0staged.220archivos y5assemblies inventariados: idénticos antes/después de las dos suites. Producto/UTcoinciden con la evidenciaUTaceptada **684/684,0fallidas/0omitidas**, que se conserva sin nueva invocación. Los assembliesEnver.API existentes enhost/IT/STcoinciden; no restore/build.

Docker **29.6.1** disponible, imágenes de fixture existentes,0otrosrunners, backendDocker previo al run y alimentaciónAC/batería100%. Inhibición temporal automática con PowerRequestSystemRequired+ExecutionRequired durante cada proceso hijo, sin cambiar plan energético. MongoDBprimary/ACK/lectura yRedisPING/noeviction/escritura/lectura verificados por la fixture existente antes de los cuerpos. DynamoDBLocal, noAWS. **0eventos observados de suspensión/reanudación/cambios de reloj≥2s**, backendDocker sin reinicio observado, Docker disponible al final; inhibición liberada,0runnersrestantes. No garantía universal contra suspensión manual/forzada o fallos externos futuros.

### 55.2. Incidentes y correcciones anteriores preservados

Detalle/fuentes en [task§82.2](specs/002-authentication/task.md#auth-validacion-final), sin borrar rojos:

- **Mongo/harness:** helperDDL sintético compartido con cliente clonado exclusivo/socket-token máximo5000ms, sin modificar límites500ms delSUT de fixture, producto, concerns/WTimeout o initializer real.40mediciones originalesACK, luego60invocaciones ysubsetMongo22/22 verdes; no causa física universal acreditada. TresarchivosMongoITya modificados antes de esta validación.
- **F1duplicados:** defecto demostrado de observaciónCTS deltest: deadline monotónico vencido puede preceder callbackcancelación. Fixsolo-test observa token odeadline conservador vencido alretorno, sin aceptarUnavailableantesdeplazo niReady/pérdida de datos. Prueba determinista y10invocaciones originales verdes. F2passwordnull no reprodujo la igualdad histórica:4filas/teoría4/4 yRegister32+F1=33/33; **sinfixF2/causa no determinada**.
- **P1 histórico:** suspensión/reanudaciónWindows acreditada dentro deltest, salto51min40,154s,161intentos/153Admitted/8Unavailable; no carga3000continua. Rama interna exactaUnavailable no recuperada. P1aislado1/1 con3000intentos/2420Admitted/580Rejected y0Unavailable-Uncertain, replay sin discrepancias; **sinfixP1 ni defecto deLua/pacing/reloj demostrado**. La siguiente presión4/4 yP1enITtambién verdes.
- **Origin20/900 histórico:** ITposterior309/310 falla otrocaso, EqualException saneada sinassertion/expected/actual; no se atribuye a suspensión anterior. Aislado1/1 yRegistrationTests32/32 verdes,sinfix:20cargosorigin,20emailscon1cadauno,21ª429sinbucketemailparcial, RetryAfter900/900s. **Causa histórica no determinada**, no «flake» ni defecto productivo/harness demostrado. Instrumentación retirada byteabyte.
- **Orquestación histórica:** algunos wrappers perdieronexitcode nativo por timeout o NativeCommandErroralrecibirstderr; se conservanTRXfinales yno se inventaexitcode. Elscript externo de esta entrega usaStart-Process/redirección nativa/espera ycapturaexitcode real antes de liberarPowerRequest, sin alterar producto/tests o repetir suites.

Las correcciones demostradas corresponden a preparación/medición delharness; los incidentes no localizados no se reclasifican retroactivamente como defectos delproducto ni se borran por elverde nuevo.

### 55.3. Regresión completa final y evidencias

Fuente primaria externa **`%LOCALAPPDATA%\Temp\opencode\auth058-final-validation-20261112-091730`** (`R`), conTRX/stdout/stderr/exitcode/environment porfase, comandos, manifiestos yresumen. Evidencia localtemporal, sin garantía de retención ni workflowCI. Los comandos `dotnet test <csproj> --no-build` no usan filtros; solo añaden loggerTRX ydirectorioexterno. Detalle en [task§82](specs/002-authentication/task.md#auth-validacion-final).

| Suite secuencial | Passed / total | Failed / Skipped | Duración runTRX | Wall CLI+fixture | Exit real | TRX |
| --- | --- | --- | --- | --- | --- | --- |
| Una ITcompleta | **310/310** | **0/0** |**555,3886689s**|559,143731s|**0**|`R/it-once/result.trx`|
| Una STcompleta, después deITverde | **223/223** | **0/0** |**182,5251329s**|187,101272s|**0**|`R/st-once/result.trx`|
| UTbaseline conservada, no repetida | **684/684** | **0/0** |Sin nueva invocación|—|Sin nuevo exitcode|`auth058-20261111-210837/full-ut.trx`, fuera deR|

ITTRX **09:18:58.8586326→09:28:14.2473015**, STTRX **09:28:36.0587600→09:31:38.5838929**, ambas2026-11-12/-06:00. Dentro de la mismaIT:05816/16,Register32/32,Login25/25,Logout42/42,Rehash10/10,composición7/7,JWT5/5,presión4/4,clock26/26,storecuotas13/13,revocación7/7,Mongo02511/11,CAS6/6. P1Passed91,2646811s;OriginPassed3,9666207s;F1Passed0,7562571s. ST223incluyeTokenValidation36,Login62+eventos9,Logout26,Register11 yrestantesfamilias existentes, todasverdes. No nuevas focales ni recuento duplicado.

**E(T-AUTH-058)** se registra solo ahora en [task§83](specs/002-authentication/task.md#e-t-auth-058): Programreal, ordenfoundation→Authentication→Authorization→MVC, DI/schemeúnicos, públicos explícitos conBeareropcional sin trabajoJWT/revocación, cargo único, Logoutprotegido/excepciónexacta, fallbacksegura,401challengeBearer/no-store y503noverificable, errores413/415tempranos e inicialización sinbootstrap. Reutiliza componentesexistentes, no creaissuer/validator/cuotas/passwordservice/autoridadalternativos. Los16casoshostrealpasaron dentrode310/310. Privacidad solo en canales capturados, probes no prueban pérdida físicaACK.

### 55.4. Revisión formal, decisión y límites

La revisión explícita de [task§84](specs/002-authentication/task.md#cierre-gate-e) contrasta **Register, Login, Logout, JWT, Rehash, Revocation, Quotas, Fail-closed, Pipeline058 yregresión final** conE034–E037/GATE-D,E038–E042/checkpointE-JWT,E043–E046/GATE-F,E032/E047–E051,E017–E022/E052–E055/E057 yE058. Las entradas ysalidas deGATE-E quedan acreditadas en el alcance localtrasLogout, no por mera presencia dearchivos.

**T-AUTH-058: COMPLETADA / EVIDENCIA REGISTRADA. GATE-E: SATISFECHO / CERRADO. Authentication: TERMINADA — exclusivamente LOCAL/TEST REPRESENTATIVO.** A/B/C/D/F conservan cierres ylímites anteriores; **GATE-G/H PENDIENTES / NO SATISFECHOS**,059–079no iniciadas,008conservaestado documental previo. Esto no completa integralmente002ni aprueba despliegue, productionready/cloudready, dosmáquinas/LB/TLS/AtlasHA/Redisadministrado/AWS/IAM/failoverproductivo. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**; pérdida totalmente silenciosa noobservable residual noeliminada.

Solo task/memoria editados en esta entrega después deverde; losotros218archivos y5assembliesconservados. Árbolfinalmantiene12modificados+1nuevoanteriores,0staged; se preserva todoeltrabajopendiente. Spec/plan, constituciones/001, AGENTS/README, agentes/skills intactos, sininvocacionesnuevas. No Orders,Infrastructure/Terraform/AWS/CI-CD,059 ni despliegue. **Sin stage/commit/push**, HEAD **995fa9c1c604f20e2cb941af196829ecbc35c8e4** conservado. Trabajo detenido tras cierre; ningún incremento ocheckpointGit automático.

## 56. 003 — BUILD controlado T-INFRA-001 + T-INFRA-002: baseline Terraform local

### 56.1. Autorización y baseline posterior

El **12 de noviembre de 2026**, el responsable autoriza **ENVER — 003 INFRASTRUCTURE / BUILD T-INFRA-001 + T-INFRA-002 / BASELINE TERRAFORM**. Es un permiso técnico posterior delimitado, no una ejecución implícita de los catálogos. Se leen completamente spec/plan/task de 003 y referencias de 002/004 para conservar sus contratos. No se reescriben 003 spec/plan ni las constituciones; AGENTS/README raíz conservan su historial sin sincronización no autorizada.

Baseline verificada: rama `feature/authentication`, HEAD **3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932**, árbol limpio. Este checkpoint documental autorizado previamente contiene los seis Markdown de 003/004; Authentication mantiene su checkpoint y cierre **TERMINADA — exclusivamente LOCAL/TEST REPRESENTATIVO** (§55). Se crea **feature/infrastructure** desde ese HEAD y se comprueban rama, status limpio y HEAD idéntico. No commit nuevo en este BUILD.

### 56.2. Archivos y decisiones técnicas revisables

Se crean únicamente nueve archivos versionables en `infra/`: `main.tf`, `versions.tf`, `providers.tf`, `variables.tf`, `outputs.tf`, `locals.tf`, `terraform.tfvars.example`, `README.md` y `.terraform.lock.hcl` generado por init. Se actualizan solo [003 task](specs/003-infrastructure/task.md), con E(001)/E(002) en §6, y este archivo. `.gitignore` **no modificado**: ya excluye `.terraform/`, state/backups, tfvars reales, planes, crashes/overrides, conservando lock y ejemplo versionables; se comprueba con `git check-ignore`.

- Terraform ya instalado: **1.17.1 / windows_amd64**. Primera ejecución falla por PATH heredado (exit 1); se localiza el binario existente en PATH persistido del usuario/WinGet y se usa añadiendo su directorio solo al PATH del proceso. Ejecución de versión correcta exit 0. No reinstalación/actualización ni modificación persistente de PATH; no se sigue el aviso de versión nueva.
- Restricción Terraform `>= 1.17.1, < 1.18.0`: compatible con la instalación observada, parches de la misma minor; ampliación sujeta a revisión. Provider único **hashicorp/aws ~> 6.0**, justificado con consulta a [Registry oficial](https://registry.terraform.io/v1/providers/hashicorp/aws): versión publicada 6.73.0 el 2026-11-11. Init selecciona **6.73.0, signed by HashiCorp**, y genera lock con checksums, sin edición manual. No se atribuye aprobación integral a DP-I-12.
- Provider sin credenciales: `var.aws_region` y guard `allowed_account_ids` desde `var.aws_account_id`; cadena estándar AWS externa. Cuenta/región/ambiente obligatorios sin defaults; ejemplo contiene placeholders no operativos y no secretos. Inputs tipados/validados, naming/tags comunes ENVER, outputs solo documentado sin inventar endpoints/IDs. Sin recursos/data sources/módulos/provisioners.
- State **local**, no Git; no backend S3 ni DynamoDB para lock. `init -backend=false` instala provider sin inicializar backend. No se crea state/plan ni se simulan recursos. README registra operador único, custodia, respaldo cifrado, recuperación y migración remota antes de colaboración/GitHub; operador/destino, ACL/cifrado, retención y restauración real quedan pendientes, sin afirmar protección comprobada. La authority DynamoDB de Authentication no es locking Terraform.

### 56.3. Evidencia local, pendientes y estados

`terraform fmt -recursive infra`, `terraform fmt -check -recursive infra`, `terraform -chdir=infra init -backend=false` y `terraform -chdir=infra validate` terminan **exit 0**; validate: **Success! The configuration is valid.** Salidas nativas en la sesión, síntesis saneada en task §6; no se inventa archivo externo de evidencia/retención. Se revisa lock: solo versión/restricción/checksums del provider, sin secretos detectados. Revisión de nueve archivos infra: sin secretos detectados, state/plan, recursos o módulos; lock/ejemplo no ignorados, enlaces relativos válidos y sin espacios finales/conflictos.

AWS CLI **ausente**. Detección solo de presencia de variables habituales de credenciales/perfil/roles/web identity/contenedor, rutas alternativas y archivos estándar `.aws/config`/`.aws/credentials`: no presentes. No se leen/imprimen secretos ni se llama STS; **identidad AWS no acreditada**, no afirmación universal de inexistencia de credenciales. Cuenta, región/AZ/ambiente final, responsables, presupuesto/duración, precios/créditos siguen pendientes. No se seleccionan dominio, tamaños EC2, Redis final o Atlas tier; no se crean usuarios IAM/access keys.

| Elemento | Estado tras este BUILD |
| --- | --- |
| T-INFRA-001 | **PARCIAL / BLOQUEADA POR DECISIONES**; preparación sin aprobación de destino, costo y responsables |
| T-INFRA-002 | **PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE**; no se rebaja el contrato de dependencia T-INFRA-001 ni la aceptación de versiones/custodia de state |
| T-INFRA-003–030 | **PENDIENTE / NO EJECUTADA**; T-INFRA-025 no ejecutada ni cerrada por coincidencia de comandos |
| GATE-I-A | **PENDIENTE / NO SATISFECHO**; faltan aprobaciones y E(025), además de aceptación integral E(001)/E(002) |
| Otros ocho gates / AC cloud | Pendientes; no infraestructura ni aceptación cloud |

Validación local no configura identidad real, valida inputs concretos de destino ni estima costos. README/task separan selección técnica de aprobación de DP-I-12/13 y custodia efectiva; DP-I-01/02 y demás decisiones no resueltas por inferencia. No se inicia fase posterior por obtener validate verde.

### 56.4. Límites y detención

Sin `plan`, `apply`, destroy, refresh, import, backend remoto ni recursos AWS; sin workflows GitHub, Orders, producto/Authentication/Program.cs, tests, Dockerfile, dependencias de aplicación o SDK. 003 spec/plan, 004 y el resto del trabajo existente se conservan. Primera release Register/Login/Logout y CD exclusivamente manual workflow_dispatch tras CI verde del SHA/digest exactos se mantienen; ningún deploy. AUTH-027 integral y gates pendientes de 002 no cambian.

**Sin stage/commit/push**; HEAD del checkpoint conservado. Los nueve archivos infra quedan nuevos/no rastreados, task/memoria modificados para revisión posterior. El bloque se detiene sin generar tareas adicionales ni checkpoint automático.

Comprobación final: `terraform -version` confirma 1.17.1/windows_amd64; fmt/check y validate nuevamente exit 0. `git diff --check` exit 0 (solo avisos LF/CRLF existentes); `git diff --stat` corresponde únicamente a task/memoria rastreados. Status: esos dos modificados y nueve nuevos infra, 0 staged, sin otros cambios. No se interpreta el working tree como limpio: los cambios quedan pendientes para revisión.

## 57. 003 — Validación AWS + Terraform; cierre formal aún pendiente

### 57.1. Autorización y contexto preservado

El **12 de noviembre de 2026**, **CIERRE BASELINE AWS + TERRAFORM / BUILD-VALIDACIÓN CONTROLADA** autoriza validar identidad/perfil/región, revisar IAM sin cambios, repetir version/fmt-check/validate y actualizar únicamente [003 task](specs/003-infrastructure/task.md) y este archivo. Baseline real: `feature/infrastructure`, HEAD **3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932**, dos documentos modificados y nueve archivos infra nuevos del BUILD anterior, 0 staged. Se preserva ese trabajo, sin exigir árbol limpio ni generar checkpoint.

### 57.2. AWS: evidencia observada y límites

AWS CLI **2.40.1** disponible en instalación del usuario, Windows/AMD64. Sondeo inicial no resuelve PATH heredado/ubicación global; búsqueda acotada localiza binario existente. Sin instalar/actualizar herramientas ni cambiar PATH persistente. Se ejecutan consultas con perfil **enver-infra**, sin pager y con salida capturada/reducida en memoria antes de mostrarse:

- `aws sts get-caller-identity --profile enver-infra`: **exit 0**, autenticación exitosa; ARN coincide con **user/enver-terraform-local**, Account presente. No se publica ID/ARN completo ni contenido de credenciales.
- `aws configure get region --profile enver-infra`: **exit 0**, **us-east-1 — APROBADA PARA BASELINE** por selección explícita del responsable; no decisión irreversible para toda la infraestructura.
- `aws configure list --profile enver-infra`: **exit 0**, perfil esperado; access_key/secret_key presentes, origen **shared-credentials-file**; región us-east-1, origen **config-file**. No valores ni sufijos de claves, tokens o archivos de credenciales mostrados/archivados en repo.

El responsable informa creación previa de usuario IAM sin consola y access key local, sin permisos administrativos/amplios; no los crea este bloque. Las lecturas `iam list-attached-user-policies`, `list-user-policies`, `list-groups-for-user` del mismo usuario devuelven las tres **AccessDenied / exit 254**. **No se puede certificar ausencia integral de privilegios amplios** con esa denegación; declaración del responsable y evidencia observada quedan separadas. IAM permissions **PENDIENTES PARA INFRA-3**; no políticas nuevas, no elevación ni intento con otro principal. Auditoría efectiva/consola no acreditadas; T-INFRA-005 no ejecutada.

### 57.3. Terraform, criterios y estados vigentes

`terraform -version`: **1.17.1 / windows_amd64**, exit 0. `terraform fmt -check -recursive infra`: **exit 0**. `terraform -chdir=infra validate`: **exit 0, Success! The configuration is valid.** Lock **hashicorp/aws 6.73.0** observado, sin actualizaciones/init. Provider usa `var.aws_region`, sin claves/tokens en HCL; los nueve archivos infra permanecen byte por byte intactos en este turno. Backend **local**, no Git, no remoto; state/planes excluidos y ninguno rastreado/generado. No S3 ni tabla de locking; authority DynamoDB sigue separada. STS exitoso no equivale a plan/provisión Terraform o custodia/cifrado/backup de state verificados.

| Elemento | Dictamen vigente |
| --- | --- |
| T-INFRA-001 | **PARCIAL / BLOQUEADA POR DECISIONES**; identidad/perfil y región de baseline acreditados, **presupuesto DECISIÓN PENDIENTE**. También faltan estimación regional, aprobación de responsables/ownership/duración y ambiente/AZ final; no “solo presupuesto” |
| T-INFRA-002 | **PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE**; T-INFRA-001 es dependencia formal sin aceptar; no se infiere aprobación integral de versiones/custodia/state |
| GATE-I-A | **PENDIENTE / NO SATISFECHO**; exige presupuesto/versiones/ambiente/state aprobados y E(001)/E(002)/E(025); E(025) ausente |
| Otras 28 tareas / ocho gates | Sin ejecución ni cierre; no continuación automática |

E(001)/E(002) complementarias y trazabilidad detalladas en task §7, conservando §6/§56 como antecedentes: su antigua ausencia de CLI/identidad no es el estado actual. El README infra se conserva por instrucción de editar solo task/memoria; sus observaciones pertenecen al BUILD anterior. No se modifica spec/plan ni se rebajan sus criterios. Los resultados se registran saneados, sin inventar archivo externo/retención duradera de evidencia.

### 57.4. Límites de la entrega

No recursos AWS creados por este bloque, no plan/apply ni IAM policies nuevas; sin cambios de Authentication/Program.cs/tests/Dockerfile/004, configuración infra o credenciales locales. No se estima ni inventa presupuesto. Baseline regional no autoriza cargos. Authentication mantiene cierre exclusivamente LOCAL/TEST REPRESENTATIVO, gates pendientes y AUTH-027 integral sin cambios; sin Orders/despliegues.

**Sin stage/commit/push**, HEAD conservado. Se detiene tras validación documental; el checkpoint y cualquier siguiente incremento requieren autorización propia.

## 58. 003 — Presupuesto aprobado y preparación documental INFRA-2/3

### 58.1. Decisión presupuestaria y alcance — 2026-11-12

Solicitud **CIERRE DE PRESUPUESTO + PREPARACIÓN INFRA-2/3 / BUILD DOCUMENTAL-VALIDACIÓN CONTROLADA**. Rama `feature/infrastructure`, HEAD **3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932**; dos documentos modificados y nueve archivos infra nuevos preexistentes, 0 staged. Solo se actualizan este archivo y [003 task §8](specs/003-infrastructure/task.md); sin nuevos artefactos ni cambios de spec/plan/infra/IAM.

**Decisión explícita del responsable — APROBADO PARA BASELINE:** crédito total disponible informado **USD 100**, **presupuesto operativo inicial ENVER USD 50**, **reserva mínima objetivo USD 50**. Preservar al menos esa reserva; no recursos cuyo costo estimado pueda comprometerla sin revisión previa. El límite es para el conjunto del período de desarrollo/validación/despliegue/entrega, **no USD 50 mensuales ni por componente**; duración/horas pendientes. Crédito informado no es estimación real ni saldo/cobertura verificados por Billing; **estimación regional aún pendiente**. No se crea alarma, AWS Budgets o límite técnico automático; no se autorizan cargos por este turno.

### 58.2. Costos y estrategia inicial conservada

Antes de un bloque facturable: estimación fechada para us-east-1 con cantidades, capacidad, horas/duración, retención/transferencia, cargos acumulados/residuales y margen; comprobar saldo, vigencia/elegibilidad/cobertura del crédito y reserva. Revisar **EC2, EBS/snapshots, IPv4/EIP, ALB/LCU, Redis, CloudWatch, ECR, Secrets/SSM/KMS, transferencia, Atlas si aplica** y dependencias como DynamoDB/endpoints/DNS. No asumir gratuidad/Free Tier. Atlas externo requiere fuente/cobertura separada: no asumir que el crédito AWS paga sus cargos.

Estrategia inicial confirmada, **sin implementar**: dos EC2 pequeñas en dos AZ (sin tipo elegido), ALB por rúbrica, evitar NAT Gateway inicialmente, Atlas externo, evaluar Redis por costo/compatibilidad antes de elegir ElastiCache, logs/retención mínimos necesarios para seguridad/evidencia y parada/eliminación controladas cuando no se use. No se cambia la preferencia/comparación Redis ni su semántica de continuidad/fail-closed. Sin NAT aún hay costos/decisiones de IPv4/EIP, egreso y endpoints/SSM por resolver.

EC2 detenida no implica costo cero: EBS, IP, ALB, Redis, logs y otros persistentes pueden seguir facturando. Limpieza futura exige inventario, autorización delimitada, protección de datos/estado de seguridad y comprobación de residuos/cargos; no destroy, borrado de datos o reset de revocación/authority implícitos.

### 58.3. Reevaluación sin cierre artificial

| Elemento | Estado vigente y pendientes exactos |
| --- | --- |
| T-INFRA-001 | **PARCIAL / BLOQUEADA POR DECISIONES**. Monto aprobado; identidad/perfil/región previamente acreditados. Faltan estimación regional fechada/lista cuantificada de cargos y recursos fuera del límite, horas/duración, condiciones/vigencia/cobertura del crédito, responsables/ownership aprobados por escrito y ambiente/AZ/naming-tags aplicables a DP-I-01/13 |
| T-INFRA-002 | **PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE**. Versiones/lock y estrategia local reconocidos como baseline; dependencia 001 sin aceptar. “State local protegido” es parte de la baseline declarada por el responsable; no aporta por sí solo evidencia de ACL/cifrado, operador/destino, backup/recuperación y aceptación integral del manejo del state exigido por contrato |
| GATE-I-A | **PENDIENTE / NO SATISFECHO**. Presupuesto ya aprobado; faltan aceptación completa E(001)/E(002), ambiente/custodia y **E(025)**. T-INFRA-025 es validación **temprana transversal**, aunque figure en INFRA-13: secuencia original `001 → 002 → 025 → 003`; no fase tardía dispensable ni tarea ejecutada por repetir fmt/validate |

DP-I-01 parcialmente resuelta en monto/región/crédito informado, no cerrada integralmente. No se modifican dependencias/criterios, ni se elimina E(025), ni se cierran otros ocho gates o AC. §§56–57/task §§6–7 conservan la ausencia de aprobación presupuestaria como antecedente, no como estado actual. No se inventan resultados de custodia o una estimación para completar tareas.

### 58.4. Próximo bloque recomendado, no ejecutado

Resolver pendientes 001/002 y autorizar explícitamente **T-INFRA-025** antes de extender IaC. Después solicitar preparación de configuración **T-INFRA-003/004/005/006**, no provisión: VPC, dos subnets en distintas AZ, route tables/Internet Gateway, salida estable/allowlist Atlas y DNS, SG backend solo desde SG ALB, administración SSM sin SSH público, roles runtime/Terraform mínimos y separados. Evaluar dos subnets públicas como opción económica sin NAT, sin fijarlas todavía ni confundir SG ALB con creación de un ALB.

Archivos candidatos futuros: networking.tf/security.tf y cambios mínimos variables/locals/outputs, evidencia task/memoria. Aprobar CIDR/AZ/egreso, costos, puertos/TLS y contratos de recursos/acciones IAM aplicables antes de aceptar. No AdministratorAccess/PowerUserAccess, permisos amplios de infraestructura o PassRole irrestricto; justificar acciones que requieran Resource=* con alcance/condiciones compatibles. No autoasignar permisos al usuario local: cualquier otorgamiento requiere autorización/principal propios. **enver-terraform-local autenticado según §57; IAM permissions PENDIENTES PARA INFRA-3**. La denegación de lecturas IAM previa no certifica ausencia integral de privilegios.

No EC2, ALB, Redis, ECR, ACM, DynamoDB o Secrets en ese siguiente bloque delimitado; solo referencias/contratos no secretos cuando procedan. Sin implementación en este turno: 003–006 siguen PENDIENTE / NO EJECUTADA y la recomendación no concede autorización.

### 58.5. Validación y límites

`terraform -version`: **1.17.1/windows_amd64, exit 0**; fmt-check recursivo infra **exit 0**; validate **exit 0 / Success! The configuration is valid.** Provider AWS baseline **6.73.0**, sin actualización/init nuevo. No se repiten STS/config/IAM ni se consulta Billing: identidad/región/CLI 2.40.1 se conservan de §57, diferenciadas de evidencia nueva. Sin plan/apply, recursos AWS, políticas nuevas ni cambios Authentication/Program.cs/tests/Dockerfile/004/spec/plan/infra. No secretos ni IDs de cuenta completos registrados.

**Sin stage/commit/push**, HEAD conservado. Se preserva el trabajo pendiente y se detiene tras esta entrega documental y validación local; sin siguiente incremento automático.

## 59. 003 — INFRA-2/3: definiciones Terraform, bootstrap de lectura y E025 parcial

### 59.1. Autorización y baseline — 2026-11-12

Solicitud **INFRA-2 + INFRA-3 / T-INFRA-003 A T-INFRA-006 / BUILD CONTROLADO NO APPLY** autoriza definiciones candidatas y la porción posible de validación transversal 025. No acepta dependencias incompletas ni permite provisión. Lectura completa de 003 spec/plan/task y revisión de puerto Docker/configuración antes de editar. Baseline verificada **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**, árbol limpio; checkpoint previo autorizado de once archivos. Se conserva HEAD, sin nuevo stage/commit/push.

Confirmados: proyecto **enver**, ambiente **dev**, región **us-east-1**, release inicial Authentication, no Orders; presupuesto inicial USD 50 y reserva objetivo USD 50 sobre crédito informado USD 100, no aprobación de cargos/estimación integral. Se crean `infra/networking.tf`, `security.tf`, `bootstrap/terraform-local-network-policy.json`; se modifican variables/locals/outputs/tfvars.example/README de infra y únicamente task/memoria de gobierno/SDD. No spec/plan, providers/versions/main/backend o lock modificados.

### 59.2. Candidato de red, salida y costo

Spec §6 contempla A (ALB/EC2 en públicas con SG restrictivo) como propuesta económica; DP-I-03 sigue sin aceptación. Se materializa **candidato A**, no elección definitiva: VPC propuesta `10.20.0.0/16`, DNS support/hostnames, dos subnets derivadas (/24 de ejemplo), tabla pública compartida/asociaciones/ruta IPv4 vía IGW. CIDR/newbits obligatorios sin defaults; confirmación explícita `network_design_approved` requerida antes de plan, ejemplo false. Contención/no solapamiento interno por derivación; redes externas y AZ reales aún no acreditadas.

AZ por data source disponible/estándar/opt-in, Zone IDs ordenados y primeras dos con claves a/b; sin inferir us-east-1a/b, precondición dos elegibles. Aptitud/capacidad de futuro EC2 requiere revisión propia. `map_public_ip_on_launch=false`: IGW/subnet pública sin IP pública/EIP no da salida IPv4 a una instancia. **Sin NAT Gateway/instance, endpoints, EIP, EC2 o ALB definidos**; Redis privado se mantiene para otra fase.

Recomendación concreta Atlas: **dos EIP futuras, una por backend**, dos /32 en allowlist, reasociación autorizada por reemplazo y comprobación de ambas salidas/TLS antes de admisión. Sin aprobar costo/asignación ni modificar Atlas. IP automática cambia al detener/reemplazar; NAT añade cargos/GB/IP y alternativas privadas requieren aprobación de red/operación/capacidad/tier. No /0 en Atlas ni asumir que quitar una regla corta inmediatamente conexiones existentes.

Precio público consultado **2026-11-12**, [AWS VPC](https://aws.amazon.com/vpc/pricing/), aplicable us-east-1: **USD 0.005/IP-hora**, activa o inactiva. Dos IP/EIP: **USD 0.01/h, USD 0.24/24 h, USD 7.30/730 h**, solo IP; proyección futura mínima 2 backend + 2 ALB **USD 14.60/730 h solo IP**, escalado puede aumentar IP ALB. VPC/subnets/rutas/IGW/SG sin workloads/IP: **USD 0 recurrente directo estimado**, no factura real ni gratuidad de plataforma. Tarifa exacta actual NAT Virginia/GB pendiente (no usada); no extrapolar ejemplo Ohio. Total EC2/EBS/ALB/LCU/Redis/Atlas/logs/secretos/transferencia, calendario/crédito/cobertura y ownership siguen pendientes antes de apply. EIP retenida sigue costando con EC2 parada. Comparación y runbook en [infra README](infra/README.md).

### 59.3. Security/IAM y puerto acreditado por lectura

SG ALB candidato: ingreso TCP 443 público IPv4, egreso solo SG backend/8080; no 80 hasta redirect aprobado. Backend: ingreso solo SG ALB/8080, sin ingress público general/SSH/Docker remoto. Puerto **declarado 8080** por launchSettings Container HTTP_PORTS, Dockerfile y referencia .NET; no se ejecuta contenedor ni se certifica escucha/mapeo cloud. 8081/EXPOSE/useSSL local no prueban HTTPS; **TLS interno sigue pendiente**. SSM exige rol/profile/agente/conectividad posteriores, no administración funcional acreditada.

Egreso backend **IPv4 amplio provisional** (`0.0.0.0/0`, todos los protocolos), sin ingress público o /0 Atlas: evita romper dependencias con una lista parcial de puertos antes del diseño completo. Mayor superficie de salida/exfiltración reconocida, sin filtro FQDN o segmentación fina prometidos; destinos/endpoints/red, puertos/SG Redis, TLS/OCSP, bootstrap/AMI y DNS deben revisarse para reducirlo antes de aceptación final. Sin IPv6. No adjuntar SG default/permisivos a futuros hosts; pruebas bypass reales reservadas a cloud.

Bootstrap JSON revisable **solo lectura**: nueve Describe de AZ/VPC/atributos/subnets/rutas/IGW/SG/reglas/tags, región us-east-1 y usuario enver-terraform-local; Resource=* para lecturas regionales, sin cuenta hardcodeada. Fuente de operaciones [EC2](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ec2.html). JSON parseado/revisado, no IAM simulado/aplicado. Propuesta para revisión/asignación manual por administrador autorizado; **sin políticas/roles adjuntos por este agente**. Escrituras de tags/red no son necesarias para definir/validar sin apply; provisión exige nueva política lifecycle acotada. Sin AdministratorAccess/PowerUserAccess, RunInstances, EIP, ALB/ECR/ACM/Secrets/Redis/DynamoDB/CloudWatch o IAM/PassRole.

T005 pide separación/diseño, sin crear trust aún: **no runtime role/instance profile en Terraform ahora**. Matriz de roles futuros: Terraform, runtime EC2 sin claves/crear-borrar tabla, operador SSM, publisher/deploy OIDC 004 y bootstrap secretos/authority separados. No adelantar permisos runtime de servicios aún inexistentes; DP-I-08/09/004 sin cerrar.

### 59.4. Preflight real y evidencia parcial E025

STS con perfil externo enver-infra: **exit 0**, usuario esperado y Account presente; región perfil us-east-1. Solo coincidencia/presencia mostradas, no ID/ARN completos o credenciales. Lectura AZ falla **UnauthorizedOperation / exit 254**, acción **ec2:DescribeAvailabilityZones**, recurso conjunto regional AZ (**Resource=* de esa lectura**), INFRA-2/T003 preflight. Sin ampliar permisos, cambiar identidad o llamar apply. Permisos actuales insuficientes para resolver el data source; no prueba de ausencia de otros privilegios.

**Terraform plan NO ejecutado:** faltan permiso y aceptación DP-I-03/inputs críticos; no se usa cuenta falsa, guard true o bypass. Sin planfile, state real, backend reinicializado o outputs efectivos. Recuento estático del HCL expandido: **14 recursos definidos** (8 red, 2 SG, 4 reglas) y un data source; no resultados de plan ni recursos existentes.

Terraform **1.17.1/windows_amd64**, provider/lock **6.73.0** intactos. `fmt -recursive infra` exit 0 (normalizó security.tf), `fmt -check -recursive infra` exit 0 y `validate` **exit 0 / Success! The configuration is valid.** Sin init/actualizaciones/descargas nuevas. State/planes ausentes y no rastreados, exclusiones conservadas; JSON de bootstrap con nueve lecturas/condiciones verificadas localmente.

`git diff --check` **exit 0**, sin errores de whitespace (solo avisos de normalización LF→CRLF existente). `git status` confirma feature/infrastructure, siete archivos modificados sin stage y tres nuevos sin seguimiento; `git diff --stat` revisado, sin contar automáticamente los tres nuevos. No se añaden al índice para obtener estadísticas.

| Tarea/gate | Estado vigente |
| --- | --- |
| T-INFRA-001/002 | Conservan parciales/pendientes de aceptación; dev/proyecto confirmados y costos parciales no cierran costo integral/custodia/ownership |
| T-INFRA-003/004/005/006 | **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; decisiones/dependencias y aceptación real sin satisfacer |
| T-INFRA-025 | **PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**; ahora sí porción explícitamente autorizada/ejecutada, no COMPLETADA |
| GATE-I-A | **PENDIENTE / NO SATISFECHO**, faltan 001/002, custodia/costo/ownership y aceptación completa E025 |
| GATE-I-B | **PENDIENTE / NO SATISFECHO**; exige I-A, E(003–006), E(019–020), E(027–028) y networking/IAM/SSM/Atlas/bypass reales |

Siete tareas parciales, otras 23 no ejecutadas, nueve gates pendientes. Trazabilidad E003–006/E025 y límites en [task §9](specs/003-infrastructure/task.md); salidas saneadas en sesión/documento, sin afirmar archivo externo/retención duradera. No se extrapola validate a cloud ni se aceptan dependencias por la autorización de preparar candidatos.

### 59.5. Detención

No recursos AWS creados, IAM aplicado, plan/apply/destroy/import, EC2/ALB/EIP/NAT/Redis/ECR/ACM/Secrets/DynamoDB/CloudWatch ni backend remoto. Sin workflows/004, UT/IT/ST, producto/Authentication/Program.cs/Dockerfile, agentes/skills o specs de otras fases modificados. Cierre Authentication exclusivamente LOCAL/TEST REPRESENTATIVO y AUTH-027 integral sin cambios. Sin stage/commit/push; HEAD conservado; detener tras este bloque, sin trabajo posterior automático.

## 60. 003 — Revalidación IAM/AZ e intento de Terraform plan

### 60.1. Autorización y evidencia AWS — 2026-11-12

Solicitud **INFRA-2/3 — REVALIDACIÓN IAM + TERRAFORM PLAN / VALIDACIÓN CONTROLADA / NO APPLY**. Baseline verificada: **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**, siete modificados y tres nuevos de §59, 0 staged. Solo se editan [003 task §10](specs/003-infrastructure/task.md) y esta memoria; se preservan infra/spec/plan/004/Authentication y trabajo pendiente. No se cambia la autorización de provisión ni las decisiones pendientes.

El responsable informa **bootstrap IAM aplicada**. Nueva evidencia real: `aws sts get-caller-identity --profile enver-infra` **exit 0**, usuario esperado `user/enver-terraform-local`, Account válido presente; ID/ARN completos y credenciales no publicados. STS repetido antes del plan. No se enumeran políticas IAM, por lo que no se certifica su contenido/asignación efectivos ni ausencia de privilegios amplios.

`aws ec2 describe-availability-zones --region us-east-1 --profile enver-infra` **exit 0**, seis AZ estándar disponibles/habilitadas. Mapeo observado de cuenta por Zone ID: **use1-az1/us-east-1a**, **use1-az2/us-east-1b**, **use1-az3/us-east-1e**, **use1-az4/us-east-1c**, **use1-az5/us-east-1f**, **use1-az6/us-east-1d**. Candidato del algoritmo existente: primeras dos, use1-az1/use1-az2. No AZ hardcodeadas ni capacidad futura EC2 acreditada. La denegación AZ de §59 queda como antecedente; ninguna nueva denegación IAM observada en estas llamadas. No se equipara AZ exit 0 con permisos suficientes para todo plan.

### 60.2. Validación local e intento de plan fallido

Terraform **1.17.1/windows_amd64**, provider/lock **6.73.0** preservados. `fmt -recursive infra`, `fmt -check -recursive infra`, `validate`: **exit 0**, sin cambios de formato; validate **Success! The configuration is valid.** Sin instalación/actualización/init nuevos.

Plan intentado **una vez**, con perfil externo enver-infra y variables solo en proceso: enver/dev/us-east-1, CIDR propuesto 10.20.0.0/16, newbits=8 (dos /24 propuestas), tags existentes; cuenta real obtenida de STS sin publicarla. **network_design_approved=false**, sin convertir autorización de intento en aceptación DP-I-03. Comprobación previa sin inputs automáticos/state inesperados, resource types limitados a red/SG y data source AZ; sin credenciales ambientales que sustituyan perfil. Variables restauradas, sin tfvars real/planfile/log Terraform guardados.

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 1
Error: Backend initialization required, please run "terraform init"
Reason: Initial configuration of the requested backend "local"
```

Falla **antes del provider**, no AccessDenied ni permiso faltante identificado. Baseline inicial usó `init -backend=false`; **no se ejecuta init/reconfigure/migrate-state automáticamente**. No hay conteos add/change/destroy válidos, cambios previstos confirmados ni data source resuelto por Terraform (las AZ las consultó CLI). No se afirma 0 destroy observado. No permiso IAM adicional propuesto sin una denegación real; permisos restantes no acreditados por este intento.

HCL revisado mantiene **14 instancias definidas** (8 red, 2 SG, 4 reglas) y un data source, no resultado de plan. Sin EC2/ALB/ECR/Redis/DynamoDB/ACM/Secrets/CloudWatch/EIP/NAT definidos ni creados en este bloque. Solo costo conocido del diseño actual: **USD 0 recurrente directo estimado** para red básica/SG sin workloads/IP asignadas, conforme a fuentes previas; sin nueva consulta precios/Billing ni estimación integral o gasto autorizado. Presupuesto/reserva conservados.

### 60.3. Reevaluación y límites

T-INFRA-003/004/005/006 conservan **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**. T-INFRA-025 conserva **PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**; se agrega resultado fallido de plan, no cierre por comandos verdes. AZ acreditadas no resuelven dependencias 001/002, custodia/backup, costo/ownership, DP-I-03, Atlas/egreso, SSM, contratos runtime o TLS interno.

**GATE-I-A PENDIENTE / NO SATISFECHO**, sin aceptación completa E001/E002/E025 y state/costo/ownership. **GATE-I-B PENDIENTE / NO SATISFECHO**, requiere I-A, E003–006, E019–020, E027–028 y verificaciones cloud. Nueve gates pendientes; no estados nuevos de tareas completadas.

Comprobación Git: `git diff --check` **exit 0** (solo avisos LF→CRLF); status siete modificados/tres nuevos, **0 staged**; stat revisado. Lock/provider/versiones/backend/spec/plan conservados respecto a HEAD; sin state/planfile/tfvars real o lock de state en infra.

Reintento requiere autorización específica para inicialización local, revisión custodia/state/backup y resolución/alcance de decisiones pendientes; no cambiar guard de aprobación a true por conveniencia. No recursos AWS creados/mutados, IAM aplicado por el agente, apply/destroy/import/init, pruebas backend, workflows, producto/Authentication o 004. Sin stage/commit/push, HEAD conservado. Detener tras documentar los resultados; sin siguiente incremento automático.

## 61. 003 — Backend local inicializado y plan INFRA-2/3 válido

### 61.1. Decisión y autorización — 2026-11-12

Solicitud **INICIALIZAR BACKEND LOCAL + PLAN INFRA-2/3 / VALIDACIÓN CONTROLADA NO APPLY**. Rama **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**, siete modificados/tres nuevos preexistentes y 0 staged. Solo se editan esta memoria y [003 task §11](specs/003-infrastructure/task.md); infra/spec/plan/004/producto permanecen como estaban.

**Network design APROBADO PARA DISEÑO Y PLAN, NO PARA APPLY**: opción A, ALB y dos EC2 futuros en públicas, backend exclusivamente SG ALB, no SSH público, administración futura SSM, sin NAT Gateway, dos EIP futuras para salida estable Atlas (una por EC2), TLS público futuro ALB+ACM. Inputs confirmados enver/dev/us-east-1, VPC **10.20.0.0/16** y subnets **10.20.0.0/24**, **10.20.1.0/24**. `network_design_approved=true` autorizado solo como input del plan; no costos/provisión aprobados, EIP implementadas, TLS interno decidido ni cierre integral DP-I-03/otras decisiones.

### 61.2. Init local, lock y exclusiones

Un prechequeo de hashes no encontró Get-FileHash y terminó antes de llamar Terraform; se sustituyó por SHA-256 .NET en memoria, sin instalar herramientas. Única ejecución de `terraform -chdir=infra init -input=false -lockfile=readonly -no-color`: **exit 0**, **Successfully configured the backend "local"!**, reutilizando **hashicorp/aws 6.73.0** ya instalado y fijado en lock. Sin upgrade, provider nuevo, prompts de migración, reconfigure/migrate-state, S3/backend remoto o DynamoDB locking.

Metadata `infra/.terraform/terraform.tfstate` comprobada: tipo local, path terraform.tfstate; no state de recursos aplicados. Hashes de archivos infra/bootstrap/lock antes/después idénticos. Fmt/check recursivos y validate **exit 0**, validate **Success! The configuration is valid.**, sin cambios de formato. Terraform 1.17.1 preservado.

Exclusiones Git verificadas para state/backup/sufijos, `.terraform/` y tfvars real; lock y tfvars.example siguen versionables. No state/plan/tfvars real rastreados. Tras plan: sin infra/terraform.tfstate de recursos, planfile o tfvars real; metadata local queda bajo `.terraform/` ignorada. Esto no certifica ACL/cifrado, custodia, backup o recuperación: pendientes de 001/002/025 conservados.

### 61.3. Plan observado, inventario y costos

STS previo: **exit 0**, perfil externo enver-infra, usuario esperado/cuenta válida, identificadores completos suprimidos. Inputs de proceso: enver/dev/us-east-1, CIDR /16, newbits=8, common_tags={} default existente y cuenta guard desde STS. CLI `-var=network_design_approved=true`; sin cambiar ejemplo ni escribir tfvars. Variables restauradas, sin credenciales ambientales que sustituyan perfil/log Terraform a archivo. Un solo plan, sin planfile guardado:

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode -var=network_design_approved=true
Exit: 2 (éxito con cambios previstos)
data.aws_availability_zones.available: Read complete after 0s [id=us-east-1]
Plan: 14 to add, 0 to change, 0 to destroy.
```

Inventario exacto contrastado: **1 VPC, 2 subnets, 1 IGW, 1 route table, 1 default route, 2 asociaciones, 2 SG, 4 reglas SG**; un data source AZ leído, no recurso a crear. Plan confirma **use1-az1/10.20.0.0/24** y **use1-az2/10.20.1.0/24**; nombres us-east-1a/us-east-1b según mapeo CLI previo §60. DNS support/hostnames true, map_public_ip_on_launch false; ruta default IGW, SG ALB 443 y backend solo SG ALB/8080, egreso ALB a backend y backend IPv4 amplio provisional. Sin 22/80; sin afirmar SSM/puerto/TLS/bypass reales.

**Sin tipos inesperados o prohibidos:** EC2 instances/EIP/ALB/target groups/ECR/ACM/Redis/ElastiCache/DynamoDB/Secrets/CloudWatch/Route53/NAT ausentes del inventario. **Sin AccessDenied, sin permiso adicional requerido observado**, sin mutar IAM; esto acredita llamadas de este plan inicial, no permisos para apply/refresh futuro o auditoría completa de políticas efectivas.

Costo directo recurrente estimado **del inventario actual sin workloads/IP: USD 0** de red básica/SG según fuentes anteriores, no factura medida ni plataforma gratuita. Ningún workload/IP/NAT facturable incluido. **Dos EIP futuras ≈ USD 7.30/730 h (~mes)** a USD 0.005/IP-h previamente acreditado, **fuera del plan y NO autorizadas para apply**, como EC2/ALB futuros. Sin nueva consulta Billing/precios ni aprobación de costo integral; presupuesto/reserva/cobertura/horas pendientes preservados.

### 61.4. Estados y límites

**DEFINIDO** = HCL existente; **PLANIFICADO** = 14 creaciones confirmadas; **CREADO EN AWS** = ninguno por este bloque. T-INFRA-003/004/005/006 conservan **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, ahora con plan real registrado para componentes presentes. Faltan aceptación de dependencias, salida/allowlist Atlas real, contratos IAM/runtime, egreso final, SSM y TLS interno. T-INFRA-025 conserva **PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE** por custodia/backup/recuperación, 001/002 y aceptación integral E025.

**GATE-I-A PENDIENTE / NO SATISFECHO**: 001/002, state/custodia/recuperación, ownership y E025 completa aún pendientes. **GATE-I-B PENDIENTE / NO SATISFECHO**: I-A y verificación cloud E003–006/E019–020/E027–028 faltantes. Nueve gates pendientes; no cierre artificial o inicio de T026 por plan verde.

Git: **diff --check exit 0**, solo avisos LF→CRLF; status siete modificados/tres nuevos preexistentes y **0 staged**; stat revisado sin añadir archivos al índice. HEAD y baseline protegida conservados; revisión básica de patrones de secretos en task/memoria sin coincidencias, no exhaustiva.

No apply/destroy/import, recursos AWS creados, IAM modificado, CI/CD, pruebas backend o producto/Authentication/004 alterados. Ejemplo tfvars, HCL/provider/lock/spec/plan intactos en este turno. Sin stage/commit/push; HEAD conservado. Detener tras registrar esta validación; autorización de diseño/plan no es autorización de apply o siguiente incremento.

## 62. 003 — Write policy mínima candidata, NO aplicada

### 62.1. Autorización y baseline — 2026-11-12

Solicitud **PREPARAR IAM MÍNIMO PARA PRIMER APPLY INFRA-2/3 / BUILD DOCUMENTAL / IAM POLICY PREPARATION / NO APPLY, NO CAMBIOS AWS**. Baseline verificada **feature/infrastructure**, HEAD **f1a98a01e15c67297287133c27dc23279a649d09**, árbol limpio/0 staged. El checkpoint previo autorizado `feat(infra): define networking and security baseline` conserva diez archivos de INFRA-2/3, no recursos cloud. Se crea únicamente `infra/bootstrap/terraform-local-network-apply-policy.json`, se actualizan [003 task §12](specs/003-infrastructure/task.md) y esta memoria. Read policy, HCL, lock, variables/locals/outputs, README, ejemplo tfvars, spec/plan, producto y 004 **intactos**.

Plan acreditado **14 add / 0 change / 0 destroy** conservado sin repetirlo: la policy JSON no está referenciada por Terraform. Diseño aprobado para plan, **no apply**. Policy de escritura **PREPARADA / NO APLICADA / SIN EFICACIA AWS ACREDITADA**; no llamadas a APIs de la cuenta AWS, asignación IAM o simulación.

### 62.2. Evidencia de fuente y permisos

Revisión de código público del tag **AWS provider v6.73.0**, no de una versión distinta: [directorio EC2](https://github.com/hashicorp/terraform-provider-aws/tree/v6.73.0/internal/service/ec2), archivos vpc_.go, vpc_subnet.go, vpc_internet_gateway.go, vpc_route_table.go, vpc_route.go, vpc_route_table_association.go, vpc_security_group.go, ingress/egress rule y tags_gen.go. Matriz y enlaces por archivo en task §12. Contraste con [SAR EC2](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ec2.html), [tag-on-create](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/supported-iam-actions-tagging.html), [variables IAM](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_variables.html) y [claves globales](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_condition-keys.html).

**22 acciones write únicas / 9 statements**, sin Describe: CreateVpc/DeleteVpc/ModifyVpcAttribute (solo **EnableDnsSupport/EnableDnsHostnames**, casing contrastado con [ejemplo oficial AWS](https://docs.aws.amazon.com/aws-managed-policy/latest/reference/AWS-SSM-RemediationAutomation-ExecutionRolePolicy.html), sin proponer adjuntarlo); CreateSubnet/DeleteSubnet; CreateInternetGateway/AttachInternetGateway/DetachInternetGateway/DeleteInternetGateway; CreateRouteTable/DeleteRouteTable/AssociateRouteTable/DisassociateRouteTable; CreateRoute/DeleteRoute; CreateSecurityGroup/DeleteSecurityGroup; Authorize/Revoke SecurityGroupIngress/Egress; CreateTags solo durante creación.

RevokeSecurityGroupEgress es necesario también en creación del SG: el provider retira egreso predeterminado IPv4/IPv6. ReplaceRoute, ReplaceRouteTableAssociation, ModifySecurityGroupRules y DeleteTags son update de configuración/tags, no create/delete de este plan: excluidos, como CreateTags independiente. ModifySubnetAttribute no concedido con atributos/defaults false de nuevas subnets IPv4 actuales; su helper solo modifica diferencias. No permisos para IPAM/tenancy/IPv6/endpoints, crear/borrar/desadjuntar ENIs o limpiar dependencias ajenas. Cambios de configuración/defaults/camino real exigirían revisión, no ampliación preventiva.

### 62.3. Alcance ARN/tags e IAM pendiente

Sin **Resource literal "*"** en write; ARN por seis tipos: vpc/subnet/internet-gateway/route-table/security-group/security-group-rule, región fija us-east-1, ID /* porque todavía no existen. Cuenta ARN * compensada **en cada statement** por `aws:ResourceAccount=${aws:PrincipalAccount}`; la variable se usa en Condition (no cuenta del ARN, donde IAM no permite interpolarla). Acciones seleccionadas no figuran entre excepciones EC2 de ResourceAccount. Cada statement exige `aws:RequestedRegion=us-east-1`, `aws:username=enver-terraform-local` (IAM user, no roles/root/SSO).

Tags de creación Project=enver, Environment=dev, ManagedBy=Terraform por RequestTag; nombres existentes de HCL acotados para recursos principales. Mutación/eliminación y padres por ResourceTag/Name; RequestTag no se exige a rutas/associations/revokes/delete. Crear hijos verifica VPC padre por separado; attach/detach cubren VPC/IGW; asociaciones tabla/subnet. TagSpecifications atómicas del provider permiten crear recursos/reglas con tags sin inventar fase sin tags. SG rules: autorizar sobre padre etiquetado y ARN nuevo de regla con RequestTag; AWS solo evalúa la segunda autorización si hay TagSpecifications, no garantía universal para API manual sin ellos.

CreateTags restringido por **ec2:CreateAction** a las siete APIs usadas; claves Project/Environment/ManagedBy/Name. No retagging/adopción mediante este documento; reglas HCL sin Name no lo requieren. Tags adicionales futuros fuera de esas cuatro claves necesitan revisión (plan baseline common_tags={}). **No límite duro de 14 recursos o de CIDR/puertos/destinos**, ni prueba de autoría de recursos preexistentes con los mismos tags/nombres. Antes de asignar/apply: revisar colisiones/otras políticas/SCP/boundaries y futura fijación de IDs para cleanup. Allow acotado, no permissions boundary; otros grants/identidades podrían eludir sus condiciones. No se afirma scope exacto por IDs aún desconocidos.

**Read policy conservada sin modificar.** Nuevas necesidades documentadas, NO concedidas: **ec2:DescribeNetworkAcls** en VPC Read (NACL computado; fallo produce warning) y **ec2:DescribeNetworkInterfaces** antes de Delete subnet/SG incluso sin ENIs (fallo bloquea eliminación). Esta última sale de [deleteLingeringENIs](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_network_interface.go#L1738). Para ambas lecturas SAR exige **Resource "*"**, sin ARN soportado; proponer ampliación read posterior conservando región/usuario, no mezclarlas en write. Mientras falte esa autorización no se declara rollback integral habilitado. No dar permisos GuardDuty/ENI/DescribeVpcEndpoints por componentes ausentes; detener ante dependencias inesperadas. DeleteVpc elimina defaults automáticos en flujo normal sin workloads, sin gestionar defaults como recursos Terraform nuevos.

Permisos delete/detach/disassociate/revoke preparados para el cohort de esta fase, sujetos a límites de tags/nombres; **NO autorización de destroy o rollback**. Sin RunInstances/TerminateInstances, EIP/NAT, ELB/ECR/Redis/ElastiCache/DynamoDB/Secrets/ACM/Route53/CloudWatch/IAM/SSM/KMS/S3/AutoScaling, ec2:* o grants administrativos.

### 62.4. Validación y estados

JSON **sintácticamente válido** por parseo local; assertions de 9 statements/22 acciones exactas, ARN, región/usuario/propietario, tags y CreateAction/Attribute superadas. **5318 caracteres compactados** (<6144 customer-managed policy, >2048 agregados inline IAM user): formato/asignación administrativa pendientes, no pegar como inline del usuario ni crear/adjuntar automáticamente. Sin validación semántica IAM, simulación o aceptación AWS: AWS CLI no incorpora un validador IAM puramente local y Access Analyzer validate-policy es remoto; no se invoca ni se usan create/put/attach-policy.

`terraform fmt -check -recursive infra` y `terraform -chdir=infra validate` **exit 0**, validate **Success! The configuration is valid.** Read policy/HCL/lock idénticos a HEAD; plan 14/0/0 no repetido. Sin recursos/cargos nuevos por preparar JSON. Red básica/SG del inventario sin workloads/IP mantiene USD 0 recurrente directo estimado previo, sin habilitar EC2/ALB/EIP/NAT o aprobar gasto integral.

**T-INFRA-005 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. **GATE-I-A/GATE-I-B PENDIENTE / NO SATISFECHO**, nueve gates pendientes; custodia/state/recuperación, ownership/costo, dependencias, contratos runtime y pruebas cloud no se sustituyen por JSON válido. Ninguna tarea completada. Sin apply/destroy/import/init/plan, AWS/IAM modificado, pruebas backend o workflows. Sin stage/commit/push; HEAD conservado. Detener tras propuesta y evidencia, sin siguiente incremento automático.

## 63. 003 — IAM informada aplicada, lecturas acreditadas y replan pre-apply 14/0/0

### 63.1. Autorización y nuevo estado informado — 2026-11-12

Solicitud **VALIDAR IAM NETWORK APPLY + REPLAN PRE-APPLY / VALIDACIÓN CONTROLADA / NO APPLY**. Baseline verificada **feature/infrastructure**, HEAD **f1a98a01e15c67297287133c27dc23279a649d09**; task/memoria modificados y write JSON sin seguimiento del turno anterior, **0 staged**. Ese trabajo se conserva. Solo se actualizan task/memoria; JSON read/write, HCL/lock/ejemplo/README/spec/plan, producto y 004 intactos. Evidencia detallada en [003 task §13](specs/003-infrastructure/task.md).

**IAM Network Read: APLICADA** manualmente según responsable, inline **ENVER-Terraform-Network-Read**, con DescribeNetworkAcls/DescribeNetworkInterfaces adicionales. **IAM Network Apply: APLICADA** manualmente según responsable, customer-managed **ENVER-Terraform-Network-Apply**, adjunta directamente a enver-terraform-local desde el JSON §62. Responsable declara sin AdministratorAccess/PowerUserAccess/ec2:*. No se enumeran policies remotas/versiones/otros grants/boundaries/SCP ni se certifica su ausencia global. **Terraform apply: TODAVÍA NO AUTORIZADO.** Permiso IAM no equivale a autorización de ejecutar, cerrar gates o aprobar costos/state.

§62 conserva la condición histórica de policy preparada/no aplicada; el pendiente de asignación allí se supera por esta confirmación, no por auditoría remota. El JSON read del repositorio conserva nueve acciones anteriores y no refleja los dos Describe agregados remotamente; **no se sincroniza fuera del alcance task/memoria**. Write JSON intacto, sin evidencia independiente de igualdad con versión remota adjunta.

### 63.2. Ejecución real y límites

Primer prechequeo local **exit 1 antes de AWS/Terraform** por ruta supuesta inexistente de AWS CLI en Program Files. Se localiza instalación existente `%LOCALAPPDATA%/Programs/Amazon/AWSCLIV2/aws.exe`, sin instalar/configurar/PATH. Terraform del directorio WinGet existente; provider lock **6.73.0** intacto, baseline Terraform **1.17.1** conservada. **Una ejecución** de STS/cada Describe/fmt/validate/plan después del prechequeo, sin plan fallido u oculto.

- `aws sts get-caller-identity --profile enver-infra`: **exit 0**, IAM user **enver-terraform-local**, cuenta validada y no publicada; se usa como guard allowed_account_ids.
- `aws ec2 describe-network-acls --region us-east-1 --profile enver-infra`: **exit 0**, 1 ACL existente devuelta; IDs/reglas suprimidos, no creada aquí.
- `aws ec2 describe-network-interfaces --region us-east-1 --profile enver-infra`: **exit 0**, 0 interfaces.
- Write JSON: parseo válido; **9 statements Allow / 22 acciones** exactas de §62, sin NotAction/acciones wildcard/prohibidas. Sin EC2 instances/EIP/NAT ni ELB/IAM/ECR/ElastiCache/DynamoDB/Secrets/ACM/Route53/CloudWatch/SSM/KMS/S3/AutoScaling. No se modifica JSON/read policy o IAM.
- `terraform fmt -check -recursive infra`: **exit 0**.
- `terraform -chdir=infra validate -no-color`: **exit 0**, **Success! The configuration is valid.**

Único replan con `AWS_PROFILE=enver-infra`, inputs temporales aprobados **enver/dev/us-east-1, VPC 10.20.0.0/16, newbits=8, network_design_approved=true, common_tags={}**, cuenta del STS. Sin overrides de credenciales/TF_CLI_ARGS/TF_LOG/TF_VAR preexistentes, tfvars reales/autoload ni state de recursos previo; entorno restaurado, sin logs crudos o planfile guardados:

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 2 (éxito con cambios previstos)
data.aws_availability_zones.available: Read complete after 1s [id=us-east-1]
Plan: 14 to add, 0 to change, 0 to destroy.
```

Coincidencia de **14 direcciones exactas** contra §61/task §11: **1 VPC, 2 subnets, 1 IGW, 1 route table, 1 ruta default, 2 asociaciones, 2 SG y 4 reglas SG** (2 ingress/2 egress). Ningún recurso distinto, update/destroy o tipo inesperado. AZ **use1-az1/use1-az2**, subnets **10.20.0.0/24 y 10.20.1.0/24**, DNS VPC true, IP automática subnet false, ruta default IGW; SG ALB443/backend8080/egreso sin cambios, backend IPv4 amplio provisional, no SSH/80. No EC2/ALB/EIP/NAT/Redis/ElastiCache u otros workloads. No readiness/SSM/TLS/bypass reales certificados.

**AccessDenied/UnauthorizedOperation: ninguno** en llamadas ejecutadas. **Read suficiente para esas llamadas: sí**; no lectura integral de todos los recursos futuros, inexistentes en el state. **Permiso adicional mínimo observado: ninguno**, sin ampliar IAM preventivamente. Las dos lecturas requeridas en §62 ahora están acreditadas directamente; eso **no demuestra permisos efectivos write/destroy/rollback**. No Create/Delete/Attach/Modify/Authorize/Revoke/CreateTags, simulación o dry-run write. Tampoco se prueba que los Allow locales sean la única fuente de permisos efectivos. Hashes SHA-256 de fuentes infra/policies/lock/README/ejemplo **sin cambios** antes/después.

### 63.3. Costo, estados y detención

**Ningún costo directo nuevo identificado respecto al inventario baseline**. Se mantiene estimación previa **USD 0 recurrente directo de red básica/SG sin workloads/IP**, no costo cero absoluto ni factura/gratuidad verificada; sin nueva consulta precios/Billing. Tráfico/workloads/IP y plataforma futura no cuantificados por este plan. Costos integrales, duración/créditos/reserva/ownership conservan pendientes.

**T-INFRA-003/004/005/006: PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**. 003 conserva dependencias/solapamientos/aprobaciones; 004 sin EIP/allowlist/conectividad/egreso integral; 005 tiene asignación IAM informada y reads observados, no eficacia write/rollback o separación runtime/004 aceptada; 006 sin pruebas cloud/bypass/SSM/TLS interno/egreso final. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, faltan custodia/state/backup/recuperación y aceptación de dependencias/E025.

**GATE-I-A PENDIENTE / NO SATISFECHO** por E001/E002/E025, costos/ownership/state; **GATE-I-B PENDIENTE / NO SATISFECHO** por I-A y evidencias cloud E003–006/E019–020/E027–028. Nueve gates pendientes, ninguna tarea completada. HCL **DEFINIDO**, 14 recursos **PLANIFICADOS**, ninguno **CREADO EN AWS por este turno**; IAM manual previo se registra separado de provisión Terraform.

Sin apply/destroy/import/init, recursos AWS manuales, mutación IAM por el agente, backend tests, CI/CD, stage/commit/push. HEAD conservado y 0 staged; solo task/memoria editados, write JSON preexistente sin seguimiento conservado. Sin planfile guardado: el plan no garantiza acciones exactas de un apply futuro. Detener; antes de provisión se requieren autorización explícita, plan vigente, permisos de ciclo de vida revisados, costos/reserva, protección de state y rollback autorizado, sin aceptación implícita por plan verde.

## 64. 003 — Primer apply autorizado: 14 recursos de red/SG creados y verificados

### 64.1. Autorización, checkpoint e inputs — 2026-11-12

Solicitud **PRIMER APPLY CONTROLADO — NETWORK BASELINE INFRA-2/3 / BUILD CLOUD CONTROLADO**, autorización expresa únicamente de 14 recursos de red/SG tras generar y revisar un plan guardado. No aprobación de plataforma completa, workloads, IAM adicional, destroy/import o cierre de gates/dependencias. El checkpoint Git previo autorizado **0958e83b887a73e1bdae2ff8b25753f5a52d0f0c**, `chore(infra): prepare network apply permissions`, integra exactamente write JSON/task/memoria. Rama **feature/infrastructure**, HEAD y working tree **limpio / 0 staged** verificados al iniciar y justo antes del apply. No stage/commit/push ahora; solo task/memoria como ediciones versionables, detalle en [003 task §14](specs/003-infrastructure/task.md).

STS **exit 0**, perfil **enver-infra**, IAM user **enver-terraform-local** confirmado; cuenta guard validada desde STS, no publicada. Terraform **1.17.1** / provider **6.73.0** comprobados; backend **local** ya inicializado, path terraform.tfstate; **init no necesario/no ejecutado**, sin migración o remoto. IAM Read/Apply configuradas previamente por el responsable; ninguna policy modificada por el agente.

Inputs temporales exactamente **enver/dev/us-east-1, VPC 10.20.0.0/16, newbits=8 → 10.20.0.0/24 y 10.20.1.0/24, network_design_approved=true, common_tags={}**, account guard desde STS igual a baseline. Sin overrides de credenciales/TF_CLI_ARGS/TF_LOG/TF_VAR preexistentes, tfvars reales/autoload o state/plan preexistente; entorno restaurado al terminar. HCL/provider/lock/JSON/README/ejemplo/spec/plan, Authentication y 004 intactos.

### 64.2. Plan exacto, verificación auxiliar y apply

Fmt-check y validate **exit 0**; **un único plan pre-apply**:

```text
terraform -chdir=infra plan -input=false -no-color -out=enver-network.tfplan -detailed-exitcode
Exit: 2
Plan: 14 to add, 0 to change, 0 to destroy.
terraform -chdir=infra show -no-color enver-network.tfplan
Exit: 0; Plan: 14 to add, 0 to change, 0 to destroy.
```

Timestamp interno del plan **2026-11-12T19:04:08Z**, archivo **infra/enver-network.tfplan ignorado por Git**. Revisión humana + JSON en memoria confirma exactamente las 14 direcciones previstas, create exclusivamente, tags/CIDR/ZoneIds/DNS/SG coincidentes, sin recursos prohibidos. Data AZ leído, no creado. La comprobación auxiliar inicial paró por supuestos incorrectos de lectura JSON (`@($null).Count=1` para módulos ausentes y data AZ no presente en planned_values tras ser leído); diagnóstico del **mismo archivo** confirmó cero módulos/recursos extra y AZ presente en configuration. Corregida solo la aserción en memoria, gate operativo **PASS** antes del apply, sin regenerar plan/editar HCL ni ocultar esa detención local. No fue un plan Terraform fallido o inventario divergente.

**SHA-256 plan:** `0c323dfcd5c28ec219e69b1b6b5db5b3fd496abb4bf4426aa82fd76849b2d53f`. **Fingerprint agregado fuentes infra versionadas:** `263fd6088ae9f7c3da63f8c47d72aed48819109608c93a981e748bbeaf0c45d2`. Revalidados justo antes y después del apply, sin cambios. Gate del comando no es GATE-I-A/I-B ni su aceptación.

**Inicio apply UTC 2026-11-12T19:06:47.1094767+00:00; fin UTC 2026-11-12T19:07:04.6559714+00:00**; HEAD aplicado **0958e83b887a73e1bdae2ff8b25753f5a52d0f0c**:

```text
terraform -chdir=infra apply -input=false -no-color enver-network.tfplan
Exit: 0
Apply complete! Resources: 14 added, 0 changed, 0 destroyed.
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 0
No changes. Your infrastructure matches the configuration.
```

**Apply completo; post-plan 0/0/0**, mismos inputs/perfil, refresh de los 14 recursos correcto. Sin AccessDenied/UnauthorizedOperation/error de apply/post-plan, reintentos, -target/-replace/-refresh-only, destroy/import o cambio IAM. Solo ese archivo fue aplicado; post-plan sin -out/no aplicado. Create efectivo acreditado para operaciones ejercitadas, **no destroy/rollback ni auditoría integral IAM**.

### 64.3. Inventario real y verificaciones AWS

**14 recursos gestionados:** 1 VPC, 2 subnets, 1 IGW, 1 tabla, 1 default route, 2 asociaciones, 2 SG, 4 reglas. IDs completos no secretos por dirección en task §14.3. Principales:

- VPC **vpc-060f67e600affd569**; IGW **igw-06e8461bf7317dc7d**; tabla **rtb-0a80f643c062696f9**.
- Subnet a **subnet-0120d869dd659e2a1**; subnet b **subnet-01934348f5d6985aa**.
- SG ALB **sg-0dacf0df5da30b4f7**; SG backend **sg-070d06cfebe866427**.

**Ocho Describe EC2 exit 0**, región us-east-1/perfil enver-infra, verificación finalizada **2026-11-12T19:09:09.1221599+00:00**: describe-vpcs, describe-vpc-attribute dos veces, describe-subnets, describe-internet-gateways, describe-route-tables, describe-security-groups y describe-security-group-rules. Evidencia:

- VPC available, **10.20.0.0/16**, DNS support/hostnames **true**.
- Subnets available **10.20.0.0/24 — use1-az1/us-east-1a**, **10.20.1.0/24 — use1-az2/us-east-1b**; ambas **MapPublicIpOnLaunch=false**.
- IGW con una asociación available exclusivamente a VPC ENVER. Tabla ENVER con ruta **0.0.0.0/0 → IGW ENVER active**, ruta local /16 y **dos asociaciones explícitas associated**, una por subnet.
- SG ALB únicamente ingress **TCP443 IPv4 público** y egress **TCP8080 a SG backend**; SG backend únicamente ingress **TCP8080 desde SG ALB**, egress todos protocolos IPv4 0.0.0.0/0 **provisional**. Cuatro reglas/IDs exactos, sin ingress extra/22 público o backend8080 público.
- Tags Project=enver/Environment=dev/ManagedBy=Terraform y Name cuando corresponde, correctos en todos los recursos etiquetables inspeccionados. No prueba activa de conectividad/bypass/SSM/TLS ni ALB/API en funcionamiento.

Sin EC2/EIP/NAT/ALB/target groups/ECR/ACM/Route53/Redis/ElastiCache/DynamoDB/Secrets/CloudWatch/IAM/SSM creados **por este apply**, según plan, apply y state; no inventario global de la cuenta. VPC añade defaults AWS implícitos (tabla/SG/NACL/ruta local), no recursos adicionales gestionados del HCL. Se distingue inventario Terraform de objetos automáticos AWS.

### 64.4. State, costo, pendientes y detención

State local **infra/terraform.tfstate** creado/actualizado: **serial 15, 14 instancias managed, 0 tainted**, última escritura **2026-11-12T19:07:04.5633341Z**. **Sin terraform.tfstate.backup observado**, no respaldo acreditado. Lectura selectiva en memoria sin mostrar state completo; .terraform/plan/state/sufijos/tfvars reales ignorados, **0 artefactos sensibles rastreados**. Exclusión Git no acredita ACL/cifrado/custodia/backup/recovery: ahora state representa recursos reales y es indispensable conservarlo; no reaplicar el plan guardado consumido.

Se materializó únicamente red base/SG; no costo directo nuevo fuera de lo previsto identificado. Estimación previa **USD 0 recurrente directo para esos componentes sin workloads/IP**, **no costo cero absoluto**, factura o tráfico medidos; sin nueva consulta Billing/precios. Plataforma futura, duración, crédito/reserva/cobertura y cost ownership mantienen pendientes. No se infiere autorización de recursos de costo relevante EC2/ALB/EIP/NAT/Redis.

**T-INFRA-003/004/005/006 siguen PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; 003/006 avanzan con red/SG reales creados y verificados, sin fingir aceptación integral. 003 conserva dependencias/solapamientos externos; 004 EIP/allowlist Atlas/conectividad estable pendientes; 005 IAM Terraform usado no satisface runtime/instance profile/SSM/servicios posteriores ni acredita destroy/rollback; 006 sin SSM/TLS interno/egreso fino o pruebas activas. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, registra fmt/validate/plan/apply/post-plan pero conserva custodia/backup/recovery, 001/002 y E025 integral pendientes.

**GATE-I-A PENDIENTE / NO SATISFECHO** por 001/002, state custody/backup/recovery, cost ownership y E025 integral. **GATE-I-B PENDIENTE / NO SATISFECHO** por I-A, IAM runtime/SSM, Atlas, TLS interno/egreso y otras evidencias integrales. Nueve gates pendientes. Autorización puntual de red no reforma la constitución, acepta retrospectivamente dependencias, ejecuta integralmente 026–028 o habilita despliegue/004. AUTH-027 integral y demás límites previos intactos.

Solo task/memoria editados; sin HCL/Authentication/004 alterados, otras mutaciones, ampliación IAM, destroy/import, stage/commit/push. **DEFINIDO/PLANIFICADO/CREADO EN AWS** coinciden para estos 14 recursos y propiedades verificadas, no para plataforma completa. Detener para revisión humana; ningún siguiente apply, cleanup o incremento automático autorizado.

## 65. 003 — INFRA-4 ECR / contrato de imagen: definición y plan sin apply

### 65.1. Autorización, baseline y archivos — 2026-11-12

Solicitud **INFRA-4 — ECR / CONTAINER REGISTRY / T-INFRA-007 + T-INFRA-008**, **BUILD CONTROLADO / PLAN PRIMERO / NO APPLY AUTOMÁTICO**. Autoriza ECR Terraform/outputs, candidato IAM mínimo separado, lectura Dockerfile/configuración y build local sin push, fmt/check/validate/plan y registro task/memoria. No adjuntar políticas, cambiar Dockerfile/producto/tests/004, crear OIDC/workloads, apply/destroy/import ni stage/commit/push. Evidencia detallada: [003 task §15](specs/003-infrastructure/task.md).

Baseline verificada **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, `docs(infra): record network baseline deployment`, árbol inicialmente **limpio / 0 staged**. Ese checkpoint posterior integra task/memoria del apply§64, no otro apply. Red real permanece, state local **serial15/14 instancias gestionadas/hash intacto**. Terraform **1.17.1/windows_amd64**, provider **6.73.0**, perfil **enver-infra**, región **us-east-1**, STS exit0/user **enver-terraform-local**, cuenta comparada con owner VPC sin publicar ID. Backend local inicializado; sin init/instalaciones/actualizaciones.

Nuevos **infra/registry.tf** y **infra/bootstrap/terraform-local-ecr-policy.json**; modificados **infra/outputs.tf**, **specs/003-infrastructure/task.md v0.11.0** y **memoria.md**. Solo cinco archivos versionables; red/security/policies previas/lock/providers/variables, Dockerfile/.dockerignore/global.json, producto/tests, spec/plan, constituciones y004 intactos. Evidencia local privada en `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-20261112\`; JSON completo de plan no publicado/versionado, retención temporal no garantizada, no custodia/backup/recovery acreditados.

### 65.2. Dockerfile y build local

Dockerfile existente: runtime **aspnet:10.0**, SDK **sdk:10.0.403**, restore locked/build sin restore/publish sin restore con UseAppHost=false; final **FROM base**, hereda **USER $APP_UID**, ENTRYPOINT **dotnet Enver.API.dll**, WORKDIR `/app`, EXPOSE8080/8081. SDK global **10.0.403 / rollForward:disable** intacto. Docker cliente/servidor **29.6.1**, Linux/amd64; tag local enver-api:infra-validation ausente antes.

Una ejecución `docker build --file Enver.API/Dockerfile --tag enver-api:infra-validation .`, **19:24:39.6050132Z–19:28:54.0723541Z**, **exit0**, build informa **0 warnings/0 errors**. Inspect: **USER1654/APP_UID=1654 non-root configurado**, **.NET/ASP.NET10.0.12**, **HTTP_PORTS=8080**, linux/amd64 y ENTRYPOINT esperado. Sin contenedor iniciado/aplicación/pruebas ejecutados, escucha HTTP/TLS o readiness acreditados. ID/RepoDigest **local** `sha256:32d2d2a952c4f1c4c9bb51c620a452b8acbee0ee591cc9f1828479f20fd10c33` **no es digest publicado ECR/artefacto productivo elegido**. Sin login/push o variable ficticia; imagen local conservada, no cleanup.

Gaps: bases por tags mutables, no digest; SDK fijo/restore locked no garantizan reproducción byte a byte. Pinning/vulnerabilidades/versión de parche requieren evaluación/autorización separada, no cambio silencioso Dockerfile. No gap non-root observado en metadatos. .dockerignore excluye state/backups/tfvars/planes/.terraform/credenciales/material sensible usual; sin secretos como build args, pero no auditoría integral de capas/scanning/SBOM/CI verde. Authentication no revalidada ni desplegada.

### 65.3. ECR, scanning y retención

Un registry privado **enver-dev-api** derivado de project/environment/local.name_prefix, **IMMUTABLE** sin excepción latest, **AES256 ECR administrado**, **force_delete=false**, sin custom KMS. Tres outputs no secretos **ecr_repository_name/url/arn**; no existencia acreditada. **scan_on_push=true** definido/planificado, no scanning real probado, Inspector habilitado o recurso de scanning global creado.

Lectura de configuración global **exit254/AccessDeniedException**, **ecr:GetRegistryScanningConfiguration**, recurso **`*`**, fase preflight read-only, sin identity-based Allow. **No AccessDenied de plan** ni ampliación IAM automática. Modo/filtros BASIC/ENHANCED y costo efectivo pendientes antes de apply/push; documentación por defecto no demuestra configuración de la cuenta. No se cambia toda la cuenta desde registry.tf.

Lifecycle único **untagged/sinceImagePushed/14days/expire**: expira solo imágenes sin etiqueta con push de hace más de14d, asíncrono normalmente dentro de24h tras elegibilidad; edad **no se cuenta desde retirar tag**. Conservar etiqueta inmutable/manifiestos/índices en cada digest activo y al menos el previo compatible para rollback. **Todas las etiquetadas conservadas**, evitando borrado por conteo/edad ciegos de un rollback antiguo. No máximo global de imágenes/GB/tope de costo; etiquetadas pueden crecer. Limpieza adicional requiere inventario/preview/autorización; retención final DP-I-14 y evidencia real pendientes.

### 65.4. IAM separado: propuesta no aplicada

JSON nuevo **4 Allow/9acciones/1631caracteres compactos**, validado localmente:

`ecr:CreateRepository`, `ecr:TagResource`, `ecr:DescribeRepositories`, `ecr:ListTagsForResource`, `ecr:GetLifecyclePolicy`, `ecr:PutLifecyclePolicy`, `ecr:DeleteLifecyclePolicy`, `ecr:DeleteRepository`, **`ecr:GetRegistryScanningConfiguration`**.

Ocho acciones en repository/enver-dev-api/us-east-1, cuenta igual a aws:PrincipalAccount, username/región; create/tag con cuatro request tags, lifecycle/delete con cuatro resource tags. Lectura global scanning separada Resource=* porque no admite ARN, username/región, sin write global. Derivación del código provider6.73.0 referenciada en task§15.4: settings iniciales van en CreateRepository; no PutImageScanningConfiguration/PutImageTagMutability/UntagResource para updates no usados.

**Sin ecr:*** ni otras familias de servicios, token/uploads/PutImage/BatchDeleteImage, account/repository policies, replication o IAM/OIDC. Policies de red intactas. Condiciones Allow **no son boundary**, auditoría de grants remotos o tope absoluto ante otras policies. DeleteRepository solo para rollback posteriormente autorizado, no probado/ejecutado; Sid no impone aprobación humana, y force_delete=false protege HCL pero no fuerza condición IAM contra otro cliente.

Publisher **conceptual/no concedido** separado: GetAuthorizationToken en Resource=*; BatchCheckLayerAvailability/InitiateLayerUpload/UploadLayerPart/CompleteLayerUpload/PutImage/BatchGetImage en ARN exacto. Lecturas de digest/scan y runtime pull (incluido GetDownloadUrlForLayer) bajo permisos/autorización propios. Futuro **GitHub→AWS OIDC/credenciales temporales**, sin access keys persistentes de CI; OIDC/trust/workflows no creados.

### 65.5. Contrato digest y deployment futuro

Identidad **`<repository-uri>@sha256:<digest>`**, no latest/tag. Vincular SHA completo/run/attempt/arquitectura/CI verde exacto al digest real ECR; no inferirlo del ID/RepoDigest local. Tag inmutable único por publicación, promover mismo artefacto sin rebuild, conservar dependencias OCI y rollback compatible. **Propuesta pendiente DP-I-14/004:** Terraform administra registry/lifecycle/outputs; **004 único escritor de selección digest runtime**, sin dos escritores/ignore_changes/variable ficticia. Mecanismo EC2/ownership/aprobación contractual pendientes, sin modificar plan.

CI **automático**; CD producción **separado/exclusivamente humano**, GitHub Actions → Deploy → **Run workflow/workflow_dispatch** tras CI verde del SHA/artefacto elegido. Push/PR/merge/release/schedule/dispatch automático no despliegan producción; **terraform apply nunca automático**. Actor/acceso/auditoría deben contemplar dispatch vía API/CLI. Futuro deploy: mismo digest A/B, dos instancias sanas, smoke tests, rollback manual y evidencia; no restaurar state de seguridad antiguo/resetear cuotas/authority. No capacidad implementada aquí.

### 65.6. Validación/plan, costo y límites

Fmt/check recursivos y validate **exit0/Success! The configuration is valid.** Primer intento de plan **exit1/Too many command line arguments**, paso de `=` por PowerShell antes de refresh/plan, log conservado. Segundo con argumentos literales entre comillas/mismos inputs/HCL/permisos: **exit2/Plan: 2 to add, 0 to change, 0 to destroy.** Sin ocultar fallos auxiliares previos Get-FileHash/PATH, corregidos con herramientas existentes sin cambio persistente.

Plan ignorado **infra/infra4-ecr.tfplan**, **19:27:34.4802448Z–19:27:55.6869202Z**, SHA256 **75c8ac132a58658681605ae6dde02386c362ff16c6d89fc12a675d54b0f8d4e1**. Show-json exit0: **aws_ecr_repository.api** y **aws_ecr_lifecycle_policy.api** solo create; **14 recursos de red no-op**, ningún update/replace/delete. Inputs verificados enver/dev/us-east-1,VPC10.20.0.0/16,newbits8,common_tags={},network_design_approved=true, account guard STS; subnets preservadas. Plan nuevo sin repository en state **no prueba permisos de escritura/scanning ni ausencia de colisión no gestionada**.

Tres resource_drift de refresh en route table/SG incorporan vistas calculadas antes vacías; coinciden con ruta/four rules standalone, **no-op final en los14**, no cero drift absoluto. Auxiliar apuntó inicialmente a address supuesto incorrecto y se detuvo; corregida solo inspección del mismo JSON por tipo/address real, sin replan/HCL modificado. State persistido/hash/serial15/14instancias intactos, backup no acreditado; lock/policies previas/Dockerfile/global.json/.dockerignore y HCL previo salvo outputs autorizado intactos. State/backups/planes/tfvars/.terraform ignorados/no versionados. Plan de red consumido no reutilizado; plan ECR no autoriza apply y requiere revisar vigencia ante cambios/permisos.

Costo consultado **2026-11-12**, [AWS Price List regional](https://pricing.us-east-1.amazonaws.com/offers/v1.0/aws/AmazonECR/current/us-east-1/index.json), publicationDate2026-11-11T15:36:26Z/effectiveDate2026-11-01, SKU KJC988R4J65BDMTQ: privado estándar **USD0.10/GB-mes**. [ECR pricing](https://aws.amazon.com/ecr/pricing/): entrada y transferencia a servicios AWS en misma región **USD0/GB**; Internet/inter-región facturable según ruta/volumen, neto exacto pendiente. Sin asumir Free Tier/créditos/franquicias/consumo mensual o GB ECR desde tamaño Docker local.

[AWS scanning](https://aws.amazon.com/blogs/containers/container-scanning-updates-in-amazon-ecr-private-registries-using-amazon-inspector/) describe BASIC sin cargo adicional, ENHANCED/Inspector facturable; no prueba modo/factura actual de cuenta, lectura denegada. No usar tarifa histórica Inspector como vigente. Estimación integral pendiente **0.10×GB-mes+transferencia facturable+scanning si aplica**; lifecycle no es cap. Presupuesto agregado baseline USD50/reserva≥USD50 de crédito reportado USD100 intacto, no saldo validado/mes/tope duro/aceptación T001. Sin nuevo NAT/endpoints/custom KMS ni almacenamiento ECR subido.

### 65.7. Estados y detención

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. Sin imagen/digest ECR/CI, contrato/ownership/retención aprobados, scanning/costo efectivo, IAM aplicado ni dependencias/custodia/backup/recovery aceptados.

**T-INFRA-003/004/005/006 PARCIAL** conservado. **GATE-I-A / GATE-I-B PENDIENTE / NO SATISFECHO**; relacionado registry/container **GATE-I-C** pendiente por dos EC2/AZ/mismo digest/capacidad/recuperación/prerrequisitos; **I-H/I-I** por reproducción/state/rollback/004/aceptación cloud. **Nueve gates pendientes**, nueve tareas parciales001–008/025, otras21 PENDIENTE/NO EJECUTADA. Authentication exclusivamente LOCAL/TEST REPRESENTATIVO sin cambios; 002 GATE-G/H/AUTH-027 integral pendientes.

**CREADO EN AWS EN ESTE BLOQUE: NINGUNO. SIN APPLY/DESTROY/IMPORT, MUTACIONES AWS/IAM, PUSH DE IMAGEN, OIDC O WORKFLOWS.** Solo preparación/build local; cinco archivos versionables autorizados, sin stage/commit/push Git. Detener para revisión humana HCL/IAM/plan/costo, sin cleanup/nuevos permisos/siguiente incremento automático.

## 66. 003 — INFRA-4 ECR: revisión IAM/cuotas pre-apply, sin AWS ni checkpoint Git

**2026-11-12**, solicitud **PRE-APPLY IAM + CHECKPOINT / VALIDACIÓN CONTROLADA / NO APPLY**: revisión local del JSON ECR, scope/cuotas/tipo recomendado, fmt-check/validate y evidencia nueva solo en task/memoria. Sin stage/commit/push aunque el título mencione checkpoint. Baseline confirmada **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, cinco pendientes previos INFRA-4/0staged. Detalle [003 task §16](specs/003-infrastructure/task.md), versión actual **0.12.0**; §65/task§15 conservan su evidencia histórica.

Policy **localmente válida**, parseo/revisión/allowlist correctos, 4 Allow y **nueve acciones exactas**: ecr:CreateRepository, ecr:TagResource, ecr:DescribeRepositories, ecr:ListTagsForResource, ecr:GetLifecyclePolicy, ecr:PutLifecyclePolicy, ecr:DeleteLifecyclePolicy, ecr:DeleteRepository y **ecr:GetRegistryScanningConfiguration**. Sin ecr:*, AdministratorAccess/PowerUserAccess, otras familias de servicios prohibidas ni permisos publisher; settings iniciales cubiertos por CreateRepository, no ampliar updates no utilizados. No validación IAM remota ni prueba de create/delete/rollback.

[Referencia oficial ECR](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ecr.html) consultada: ocho acciones admiten ARN repository y siguen restringidas a **enver-dev-api/us-east-1**, misma cuenta mediante aws:ResourceAccount=${aws:PrincipalAccount}, username/región/tags pertinentes. **Solo GetRegistryScanningConfiguration usa Resource="*"**, por ser lectura de registry sin ARN soportado; statement separado y limitado por username/región, sin escritura global. Debe conservarse por AccessDenied§65/task§15 para inspección de scanning/costo, **no por una llamada de Terraform create/plan**. Sin lectura AWS nueva ni afirmar permiso efectivo resuelto.

Tamaño **2269 bytes/caracteres de archivo ASCII / 1631 caracteres compactos sin whitespace**; SHA256 **393b731f012d6be6bc285b068793f52dda91099661717f2f3b991c437955cbe9**, JSON intacto. [IAM quotas oficiales](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_iam-quotas.html): usuario inline **2048 caracteres agregados entre todas sus inline**, managed **6144 por policy**, sin whitespace. Cabe aisladamente inline pero deja solo417 para el resto; agregado remoto/adjuntos no auditados. **Recomendación pendiente: customer-managed separada ENVER-Terraform-ECR**, no obligatoria por tamaño individual, sí conveniente para separación de networking/cuota inline compartida.

Evidencia preparada para responsable: JSON exacto/hash/acciones/scope y nombre/tipo; si aprueba posteriormente, creación/adjunto manual solo al usuario previsto **enver-terraform-local**, cuenta/cuota verificadas por quien corresponda. **No crear/adjuntar ahora ni dar permisos IAM al usuario Terraform**, mezclar networking o inventar ARN/version aplicada. Delete concede capacidad si se adjunta; Sid no impone aprobación humana, force_delete=false limita HCL, Allow no es boundary ni revisión de grants/SCP. Futuras lecturas de identidad/scanning/costo y revisión de vigencia/plan previo al apply requieren autorización propia.

Terraform **1.17.1/windows_amd64** / provider **6.73.0** acreditados desde infra. Auxiliar inicial buscó selección provider desde raíz y se detuvo por provider_selections vacío; corregido con `-chdir=infra`, sin instalaciones/cambios/plan. **19:43:50.0174285Z–19:44:01.7004193Z**, `terraform fmt -check -recursive infra` **exit0** y `terraform -chdir=infra validate -no-color` **exit0 / Success! The configuration is valid.** Sin repetir plan: baseline §65/task§15 **2/0/0**, solo repository/lifecycle, networking14 no-op, **evidencia previa no refresh actual**. Hash del plan guardado **75c8ac132a58658681605ae6dde02386c362ff16c6d89fc12a675d54b0f8d4e1** preservado; no vigencia indefinida/autorización apply.

Evidencia nueva local privada: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-preapply-review-20261112-194320\`, resúmenes/tamaños/hashes/validación. No evidencia de custodia/backup/recovery state. HCL/policy/lock/state/plan y trabajo previo preservados por hashes; artefactos Terraform sensibles excluidos. Solo task/memoria editados **en este turno**; conjunto pendiente sigue tres modificados/dos nuevos/0staged.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**; 003–006 parciales y **nueve gates pendientes**, incluidos I-A/I-B/I-C/I-H/I-I. Sin registry/imagen ECR, scanning efectivo/IAM aplicado/contratos/dependencias aceptados; Authentication/002 intactos. **Sin AWS CLI/STS/IAM/Access Analyzer/simulación, mutaciones AWS, replan/apply/destroy/import, Docker/producto o stage/commit/push.** Detener para revisión humana, sin siguiente incremento ni checkpoint automático.

## 67. 003 — INFRA-4 IAM informado aplicado y replan pre-apply 2/0/0

**2026-11-12**, solicitud **IAM VALIDATION + REPLAN PRE-APPLY / VALIDACIÓN CONTROLADA / NO APPLY**. Autoriza STS/lecturas ECR, revisión local, fmt-check/validate/un replan con inputs existentes y solo task/memoria editados. **Terraform apply: NO AUTORIZADO TODAVÍA**. Baseline **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, cinco pendientes previos INFRA-4/0staged. Detalle en [003 task §17](specs/003-infrastructure/task.md), v**0.13.0**.

**INFORMADO POR RESPONSABLE:** creó/adjuntó manualmente **ENVER-Terraform-ECR**, **customer-managed**, fuente JSON local, sin AdministratorAccess/PowerUserAccess/ecr:*. Se registra **APLICADA MANUALMENTE SEGÚN RESPONSABLE**, no acción del agente ni enumeración/auditoría del documento/versión/adjuntos remotos/grants/boundaries/SCP. Historial de propuesta/no aplicada§65/66 se conserva. JSON local hash **393b731f012d6be6bc285b068793f52dda91099661717f2f3b991c437955cbe9** intacto, 4Allow/9acciones ECR aprobadas/1631caracteres compactos, sin servicios prohibidos/publisher o wildcard de acción; scope repository/misma cuenta/región/username/tags y único Resource=* de lectura scanning intactos.

Lecturas (perfil **enver-infra**, **us-east-1**) terminadas **19:50:50.6800817Z**:

- **STS exit0**, IAM user **enver-terraform-local**, cuenta igual a owner VPC en state, sin publicar credenciales/ID de cuenta completo.
- **GetRegistryScanningConfiguration exit0**, **scanType=BASIC / rules=[]**. Lectura antes denegada ahora efectiva; no filtro global cambiado ni scan de imagen/findings acreditados. scan_on_push=true sigue HCL/plan para repository futuro.
- **DescribeRepositories sin nombres exit254/AccessDeniedException**, acción **ecr:DescribeRepositories**, recurso **arn:aws:ecr:us-east-1:<account>:repository/***, fase **listado AWS CLI previo**, no operación Terraform. No lista vacía exit0 ni permiso de inventario general demostrado.
- Misma lectura **--repository-names enver-dev-api**, diagnóstico acotado: **exit254/RepositoryNotFoundException**, sin AccessDenied, repository inexistente; no éxito de lectura de repository existente. Se conserva el fallo original, sin ampliar IAM.

Para listar toda la cuenta, el Allow mínimo adicional sería **solo ecr:DescribeRepositories sobre repository/*** de cuenta/región aprobadas con username/región/misma cuenta; ampliaría visibilidad a otros repositorios. **Propuesta explicativa NO APLICADA / NO NECESARIA para Terraform**: preferir consulta por nombre como provider. No ecr:*/AdministratorAccess/PowerUserAccess ni restricciones falsas por tags en listado sin nombre. **Permisos efectivos acreditados parcialmente**, lectura scanning/respuesta acotada; create/tag/lifecycle/delete/destroy no ejercitados ni auditados globalmente.

Terraform **1.17.1/windows_amd64**, AWS provider **6.73.0**, lock intacto; **fmt-check exit0**, **validate exit0 / Success! The configuration is valid.**, sin init/instalaciones/updates. Un nuevo plan guardado, **19:51:57.0468069Z–19:52:18.1128719Z**, **exit2 / Plan: 2 to add, 0 to change, 0 to destroy.**, **sin AccessDenied Terraform**. Show-json exit0: únicamente **aws_ecr_repository.api** y **aws_ecr_lifecycle_policy.api** create, **networking14 no-op**, ningún update/replace/delete.

Inputs verificados: enver/dev/us-east-1,VPC10.20.0.0/16,newbits8,common_tags={},network_design_approved=true, account guard STS/state; subnets preservadas. Config plan **enver-dev-api / IMMUTABLE / scan_on_push=true / AES256 / force_delete=false**, lifecycle único **untagged/sinceImagePushed/14days/expire**, todas las etiquetadas conservadas para contrato activo/rollback§65. Tres outputs ECR no sensibles; no digest/imagen ECR ni variables ficticias.

Plan nuevo ignorado **infra/infra4-ecr-replan-20261112-195037.tfplan**, SHA256 **669e2550dced81aa3bd1bf3c07aff210305057e61c17d2cb503abe55c72a1640**. Tres entradas calculadas de refresh route/ingress/egress coinciden con standalone route/cuatro reglas; no-op final en los14, no afirmar cero drift absoluto. State persistido **serial15/14managed/hash intacto**, plan previo§65/hash intacto, fuentes/HCL/policy/lock preservados; no custodia/backup/recovery nuevo acreditado. Plan no autoriza apply ni tiene vigencia indefinida.

Costo conservado de evidencia previa: **almacenamiento USD0.10/GB-mes**, sin nueva consulta precios/Billing, asumir Free Tier o calcular total desde tamaño Docker local. Modo BASIC confirmado no prueba ejecución de scanning ni factura global/ausencia de otros cargos Inspector; volumen/transferencia/duración/total/reserva y aceptación costo pendientes. Sin recursos ECR/imagen/bytes creados/subidos por el agente.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. Sin registry real/digest/CI/retención/contrato/dependencias/state custody/backup/recovery aceptados. **GATE-I-A PENDIENTE / NO SATISFECHO**, falta E001/E002/E025 integral/costo/state/ownership; **GATE-I-B PENDIENTE / NO SATISFECHO**, I-A/SSM/runtime IAM/Atlas/egreso/TLS/E019–020/E027–028. 003–006 parciales; **nueve gates pendientes**, Authentication/002/004 sin cambios, AUTH-027 integral/GATE-G/H conservados.

Evidencia local privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-iam-replan-20261112-195037\`: lecturas saneadas/resúmenes/logs/JSON privado de plan/hashes, no retención/custodia acreditadas ni JSON completo publicable. Solo task/memoria editados en este turno, cinco pendientes previos/0staged; state/backups/planes/.terraform/tfvars excluidos de Git. **SIN MUTACIONES AWS/IAM POR EL AGENTE, APPLY/DESTROY/IMPORT, REPOSITORY MANUAL, DOCKER/PRODUCTO O STAGE/COMMIT/PUSH.** Detener para revisión humana; ningún siguiente incremento automático.

## 68. 003 — INFRA-4 ECR creado por apply exacto; post-plan0/0/0, inventario de imágenes pendiente

### 68.1. Autorización, checkpoint y gate — 2026-11-12

Solicitud **APPLY CONTROLADO / BUILD CLOUD CONTROLADO**, autorización expresa únicamente de aws_ecr_repository.api y aws_ecr_lifecycle_policy.api con plan fresco guardado/revisado2/0/0 y networking14 no-op; no HCL/IAM modificado, workloads, push de imagen, destroy/import o stage/commit/push. Checkpoint previo **2626fb10eb9b7c128c6422e92598ca9f0e78e72c**, `feat(infra): prepare ecr registry baseline`, rama **feature/infrastructure**, árbol **limpio/0staged** al inicio y justo antes de apply. Commit de cinco archivos exactos de preparación, no state/planes. Detalle [003 task §18](specs/003-infrastructure/task.md), v**0.14.0**; historial§65–67 intacto.

STS **exit0**, perfil **enver-infra**, IAM user **enver-terraform-local**, región **us-east-1**, cuenta contrastada con owner VPC del state. Terraform **1.17.1/windows_amd64**, provider **6.73.0**, backend local inicializado/path terraform.tfstate, sin init/updates. Fmt-check/validate **exit0**, validate **Success! The configuration is valid.** State inicial serial15/14managed sin.backup; ENVER-Terraform-ECR customer-managed aplicada manualmente según responsable§67, sin enumeración/mutación IAM por agente.

Plan fresco `terraform -chdir=infra plan -input=false -no-color -out=enver-ecr.tfplan -detailed-exitcode`, **20:04:51.9375136Z–20:05:11.6015211Z**, **exit2/Plan: 2 to add, 0 to change, 0 to destroy.** Guardado **infra/enver-ecr.tfplan**, ignorado, SHA256 **f91058f8f89ecea0b00e7a5ae27d27602b75f21ef3a6311a479694abdbe5c3ea**. Show humano/no-color y JSON **exit0**, inspeccionados sin publicar state/JSON completo. Solo dos create ECR, red14no-op, ningún recurso nuevo prohibido ni replace/delete.

Inputs en JSON idénticos enver/dev/us-east-1,VPC10.20.0.0/16,newbits8,common_tags={},network_design_approved=true,account guard STS/state; subnets preservadas. Config **enver-dev-api/IMMUTABLE/scan_on_push=true/AES256/force_delete=false**, lifecycle único untagged/sinceImagePushed/days/14/expire, sin tagged. Gate operativo pre-apply **PASA**, no GATE-I-A/B satisfechos. HEAD/árbol/fuentes/state/hash revisados intactos, sin AccessDenied bloqueante; no regenerar plan ni editar HCL entre inspección y apply.

### 68.2. Apply completo y post-plan

Una ejecución **`terraform -chdir=infra apply -input=false -no-color enver-ecr.tfplan`**, **20:06:15.2025475Z–20:06:33.7890082Z**, **exit0/Apply complete! Resources: 2 added, 0 changed, 0 destroyed.** Ambos ID **enver-dev-api**. Sin fallo/parcial/reintento/AccessDenied Terraform, sin destroy/rollback/IAM adicional. Plan hash idéntico antes/después; fuentes intactas. **No reaplicar el archivo consumido**; planes anteriores conservados/no usados.

Post-plan mismos inputs/perfil, **20:07:16.2036319Z–20:07:34.3744522Z**, **exit0/No changes. Your infrastructure matches the configuration. / 0/0/0**. Guardado ignorado infra/enver-ecr-post.tfplan para inspección JSON exit0, no aplicado. **Networking14no-op + ECR2no-op =16managed**, ningún apply correctivo.

### 68.3. Propiedades reales, URI/ARN y denegación de inventario

Lecturas AWS CLI específicas **exit0**, terminadas **20:08:30.6302407Z**: DescribeRepositories por nombre verifica repository existente **enver-dev-api**, **IMMUTABLE**, repository **scanOnPush=true**, cifrado **AES256**; GetLifecyclePolicy verifica única regla **untagged/sinceImagePushed/days/14/expire**, sin tagged; GetRegistryScanningConfiguration **BASIC/rules=[]**, configuración global distinta del flag por repository. URI/ARN coinciden con state:

- **URI real:** `926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api`.
- **ARN real:** `arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api`.

Identificadores no secretos solicitados para futuro push/digest, no prueba de imagen o scan/findings. Lifecycle no expira etiquetadas;14d desde push, no desde quitar tag. Contrato activo/previo rollback/manifiestos/retención§65 pendiente de aceptación/evidencia real, sin limpieza autorizada.

Última verificación read-only **ListImages** por repository exacto, **20:08:52.9293504Z–20:08:56.3479929Z**, **exit254/AccessDeniedException**, acción **ecr:ListImages**, recurso **arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api**, fase **inventario final de imágenes**, **ninguna operación/recurso Terraform fallidos**. Policy provisión no incluye esa acción; falta identity-based Allow según respuesta. **Detenidas las llamadas AWS/Terraform**, sin reintentar/ampliar IAM/rollback; solo documentación y comprobaciones locales después. No confundir con listado DescribeRepositories general histórico§67, ni convertir denegación en cero imágenes.

**0 imágenes publicadas por el agente; total/digests/ausencia global de digest NO ACREDITADOS**. No docker push/login/tag de imagen ni publicación de enver-api:infra-validation. Una comprobación futura necesitaría **ecr:ListImages solo en ese ARN** bajo autorización separada; no modificar JSON/IAM en este bloque. Apply/post-plan completados y configuración verificada no se degradan artificialmente a apply parcial por esta limitación de lectura.

### 68.4. State, costo, pendientes y detención

State local **infra/terraform.tfstate** actualizado: **serial18/16managed/0tainted**, última escritura **2026-11-12T20:06:33.7251925Z**, SHA256 **b4497de9fb6fc28e819970bad70cca2bddbf093743745f9522ef4332d4c94e02**. **.backup ahora observado**, serial15/14managed, escritura **20:06:33.3403478Z**, snapshot local anterior, no custodia/cifrado/recuperación aceptados. No restaurarlo/reaplicar state previo para gestionar16recursos actuales. State/backup/sufijos/.terraform/tfvars reales/planes ignorados/no versionados; lock/ejemplo siguen versionables. State esencial para recursos reales, protección/backup/recovery y remoto antes de colaboración/GitHub pendientes.

Tarifa almacenamiento conservada **USD0.10/GB-mes** de evidencia previa§65, sin nueva consulta Billing/precios, Free Tier o costo mensual inventados. Repository recién creado sin imágenes subidas por agente: almacenamiento esperado prácticamente vacío, **no GB/factura medidos ni inventario vacío probado** por ListImages denegado. BASIC/scanOnPush no prueban factura global ni scanning de imágenes; volumen/transferencia/duración/reserva/costo integral pendientes. Sin custom KMS/Inspector/replicación/workloads/red adicional creados por este bloque.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, ahora repository/lifecycle reales/verificados, no mero HCL; faltan dependencias002/005/025/contrato/retención/DP-I-14/revisión integral. **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, imagen/digest/validación ECR/CI/rollback/aprobación pendientes. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, suma fmt/validate/plan/apply/post-plan; .backup local no satisface custody/cifrado/recovery/E001/E002/E025 integral. T002 parcial, metadata actualizada sin aceptación nueva.

**GATE-I-A PENDIENTE / NO SATISFECHO** por baseline/costo/ownership/state/backup protegido/recovery/E001/E002/E025; **GATE-I-B PENDIENTE / NO SATISFECHO** por I-A/runtime IAM/SSM/Atlas/egreso/TLS/E019–020/E027–028. **Nueve gates pendientes**, nueve tareas parciales001–008/025, otras21 PENDIENTE/NO EJECUTADA, 003–006 conservadas. Sin ejecución integral026–028, Authentication cloud/004 o cierre002 GATE-G/H/AUTH-027 integral.

Evidencia local privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-apply-20261112-200342\`: prechecks/gate/hashes, plan humano/JSON privados/apply/post-plan, lecturas/denegación/metadata. No custodia/retención acreditadas por carpeta temporal; no publicar state/planes/JSON completo. **HCL/policies/lock/producto/Dockerfile/tests/004 intactos; solo task/memoria editados**, HEAD2626fb1 y0staged. **APPLY COMPLETO / POST-PLAN0/0/0; LISTIMAGES DENEGADO / DETENCIÓN SIN IAM NUEVO.** Sin más cloud/terraform, destroy/import/cleanup, push de imagen, stage/commit/push Git o siguiente incremento automático. Detener para revisión humana.

## 69. 003 — ListImages resuelto, repository vacío y checkpoint post-apply autorizado

**2026-11-12**, solicitud **VALIDAR LISTIMAGES + CHECKPOINT POST-APPLY / VALIDACIÓN + GIT CHECKPOINT / NO APPLY / NO DOCKER PUSH**. Baseline comprobada **feature/infrastructure**, HEAD **2626fb10eb9b7c128c6422e92598ca9f0e78e72c**, dos documentos de§68 modificados/0staged/0untracked, conservados para el checkpoint. Autoriza sincronizar ListImages en JSON local, actualizar task/memoria y stage/commit exclusivamente de esos tres archivos, sin push. Detalle [003 task §19](specs/003-infrastructure/task.md), v**0.15.0**; §68 y su denegación se conservan como historial.

**INFORMADO POR RESPONSABLE:** actualizó manualmente ENVER-Terraform-ECR agregando solo **ecr:ListImages**, sin otros permisos nuevos. No modificación IAM remota por agente ni enumeración/auditoría de documento/versiones/adjuntos/grants. Lecturas **20:22:34.9357668Z–20:22:47.1485716Z**, perfil **enver-infra/us-east-1**, todas **exit0**: STS confirma **enver-terraform-local/cuenta esperada**; ListImages devuelve **imageIds=[] / 0 imágenes / 0 digests**, sin paginación limitada; DescribeRepositories reconfirma **enver-dev-api/IMMUTABLE/scanOnPush=true/AES256**; GetLifecyclePolicy verifica una regla **untagged/sinceImagePushed/days/14/expire**, ninguna tagged. Sin AccessDenied/reintento. **Ningún digest existente en ese repository al consultar**, no extrapolado a otros repositorios o fechas.

URI real **926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api**, ARN **arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api**, iguales a§68. Repository/lifecycle creados por apply anterior, no en este turno. ListImages histórico denegado queda superado por evidencia nueva, no se borra. Sin imagen publicada por agente ni digest inventado; BASIC/rules=[] observado§68 no consultado otra vez ni prueba de scanning/findings de imagen.

JSON local **infra/bootstrap/terraform-local-ecr-policy.json** sincronizado agregando **únicamente ecr:ListImages** al statement read existente: **4Allow/10acciones exactas únicas/1648caracteres compactos**, SHA256 **68ab0b17e25e03378ad801357f980e44146bc909873678a2921b3df5a59866b1**. Comparación semántica contra HEAD sin esa acción confirma resto intacto. Sin ecr:*/AdministratorAccess/PowerUserAccess o permisos publisher/runtime nuevos. ListImages limitado al repository/us-east-1/misma cuenta/username mediante ARN/condiciones existentes; solo GetRegistryScanningConfiguration conserva Resource=*. Hashes/tamaños previos§65–67 históricos, no policy actual; JSON local no prueba todo IAM remoto o permissions boundary.

**20:23:22.8813360Z–20:23:35.5318584Z**, fmt-check/validate **exit0**, validate **Success! The configuration is valid.** Sin nuevo plan/apply/init. Post-plan§68 preservado: **20:07:16.2036319Z–20:07:34.3744522Z**, exit0/**0/0/0**, red14+ECR2no-op, no replan fresco. Hashes state/backup/planes iguales antes/después; state serial18/16managed y.backup serial15/14managed conservados, no restaurados. State/sufijos/.terraform/tfvars reales/planes excluidos, lock/ejemplo versionables; custodia/cifrado/backup protegido/recovery pendientes. Tarifa previa USD0.10/GB-mes intacta; inventario vacío no factura mensual cero, GB-mes medidos o Free Tier.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, ahora inventario vacío acreditado, pero dependencias002/005/025/contrato/retención/DP-I-14 pendientes. **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, faltan publicación/digest/validación ECR/scanning de imagen/CI/rollback y ownership004/reproducibilidad. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, protección/recovery/dependencias/E025 integral pendientes. Nueve tareas parciales001–008/025, otras21 pendientes; **nueve gates PENDIENTE / NO SATISFECHO**, incluidos I-A/B. Sin aceptar002 GATE-G/H/AUTH-027 integral, producción o004.

Checkpoint autorizado de **solo JSON ECR/task/memoria**, incluyendo evidencia post-apply§68 previamente pendiente; mensaje **docs(infra): record ecr deployment and list permission**. Revisión Git/hash/exclusiones antes de stage, verificación commit/árbol al terminar; hash y resultado del commit comunicados al cierre, no autorreferencia dentro del propio commit. No push/merge/rebase/amend/squash o stage de state/planes/secretos. HCL/lock/Dockerfile/producto/tests/spec/plan/004 intactos.

Evidencia privada local `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-list-checkpoint-20261112-202233\`, no custodia/retención garantizadas por carpeta temporal. **Sin apply/plan nuevo, Docker push/login/tag/build, IAM remoto modificado, recursos nuevos o cleanup**. Detener tras validar checkpoint, sin siguiente incremento automático.

## 70. 003 — T-INFRA-008: build local por SHA y publisher separado, sin publicación

**2026-11-12**, solicitud **PREPARAR PUBLICACIÓN CONTROLADA DE IMAGEN ECR / BUILD DOCUMENTAL + VALIDACIÓN LOCAL**, sin Docker push/Terraform apply/cambios AWS automáticos. Baseline comprobada **feature/infrastructure**, HEAD **3312d220e3ff68851befc59e9a7cfa516185bdc8**, `docs(infra): record ecr deployment and list permission`, **árbol limpio/0staged** antes y durante build. Confirma checkpoint§69 ya creado con exactamente provisioning JSON/task/memoria, sin push. Lectura spec/plan/task sin reinterpretar aceptación ni modificar esos spec/plan; detalle [003 task §20](specs/003-infrastructure/task.md), v**0.16.0**.

Tag seleccionado **git-3312d220e3ff**, primeros12hex del SHA Git completo, no timestamp/latest. Tag local **enver-api:git-3312d220e3ff**, destino propuesto **926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api:git-3312d220e3ff**, no creado en ECR. Relación documentada externamente **SHA→tag→imageDigest ECR futuro**; ID local no es digest ECR/atestación de procedencia. Repository/config y **0 imágenes/ningún digest** son evidencia fechada§69, no nueva consulta en§70. Antes de push autorizado, revalidar identidad/tag ausente mediante DescribeImages: si existe, **DETENER**; AccessDenied no significa ausencia. No sobrescribir/reutilizar tag inmutable o rebuild silencioso al promover; prefijo corto siempre vinculado al SHA completo.

Dockerfile **intacto**, SHA256 **6a9c69f5b5517129d046997c72a84d80631eb8dce377641afae65e3107e442a0**; base aspnet10.0/SDK10.0.403, global.json10.0.403/rollForward:disable, locked restore/no-restore conservados; stage final FROM base/hereda USER$APP_UID, ENTRYPOINT dotnet Enver.API.dll. Contexto build **raíz del repo**, `.dockerignore` existente excluye secretos locales usuales/state/planes/tfvars/.terraform/bin/obj/gobierno. Sin secretos en build args o cambios producto/tests/004.

Docker cliente/servidor **29.6.1** disponible. Una ejecución real **docker build --file Enver.API/Dockerfile --tag enver-api:git-3312d220e3ff .**, **20:31:13.1959806Z–20:32:05.8396958Z**, **exit0 / 0warnings / 0errors**, con árbol limpio y fuentes versionadas iguales por hash antes/después. No se reutiliza infra-validation como evidencia suficiente ni se afirma no-cache. Inspect exit0: **image ID local sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda**, **linux/amd64**, **101036277bytes ≈96.36MiB** según Docker, **USER1654/non-root**, .NET/ASP.NET **10.0.12**, **HTTP_PORTS8080** y ENTRYPOINT previsto. BuildKit exporta manifiesto/índice locales: no digest ECR desplegable ni GB facturados. Puerto solo configurado, sin arranque/escucha/TLS/pruebas acreditados; bases por tags no fijadas por digest, reproducibilidad/seguridad/SBOM/CI pendientes. Imagen conservada sin cleanup. Auxiliar PowerShell inicial tuvo ParserError/exit1 por paréntesis en función hash **antes de ejecutar comandos**, corregido solo lector, no fallo/reintento Docker.

Nuevo **infra/bootstrap/local-ecr-publisher-policy.json**, **PROPUESTA LOCAL / NO APLICADA**, **2Allow/9acciones exactas/827caracteres compactos**, SHA256 **4a4feedc8cdd2057b396a40efcde5fb8e8bcdb42dc37df178e911dc6511ffbcd**. Incluye GetAuthorizationToken(Resource=*), BatchCheckLayerAvailability/InitiateLayerUpload/UploadLayerPart/CompleteLayerUpload/PutImage/BatchGetImage/DescribeImages/DescribeImageScanFindings. Fuentes oficiales AWS push y referencia list_ecr.html consultadas2026-11-12: token sin resource type ARN, resto admite repository. Solo JSON/allowlist/scoping revisados localmente, no simulación IAM/permisos efectivos.

Token restringido a regiónus-east-1/userenver-terraform-local/PrincipalAccount926925605666; ocho acciones repository restringidas a **arn:aws:ecr:us-east-1:*:repository/enver-dev-api**, mismos guards y **ResourceAccount=${aws:PrincipalAccount}**. No ResourceAccount de repository en token. Sin ecr:*, otros servicios, AdministratorAccess/PowerUserAccess/DeleteRepository/PutLifecyclePolicy u otros permisos provisioning/borrado/config global. **terraform-local-ecr-policy.json intacto**; separar archivos no aísla identidades/otros grants, no boundary ni auditoría remota.

BatchGetImage corresponde a receta oficial push/lectura de manifiesto; DescribeImages verifica colisión/digest; DescribeImageScanFindings necesario para resultado scanning posterior, no inferirlo de flag/metadata. **ListImages omitido** por redundancia con flujo DescribeImages y provisión existente; **GetDownloadUrlForLayer omitido** porque pull de capas no es push/verificación de metadata y requerirá identidad/permiso/alcance propio. Sin StartImageScan/Inspector/ENHANCED automático. Scanning real/seguridad de imagen pendientes, no se declara limpia.

Flujo futuro detallado§20.4, **NO EJECUTADO**: responsable aprueba/aplica policy separadamente; identidad/config/tag ausente y artefacto local exacto revisados; get-login-password en memoria/exit verificado→docker login password-stdin al registry; tag destino→push; DescribeImages por imageTag=git-3312d220e3ff obtiene **imageDigest real**; DescribeImageScanFindings por ese digest comprueba estado/findings; futuro pull/uso por digest/rollback autorizado aparte. No loguear token ni inferir digest de image ID; Docker credential helper/config requiere custodia antes de login. No automatizar cleanup/rollback ante publicación parcial. Retener etiquetas/manifiestos activos/previos, lifecycle14d es edad desde push, no retiro del tag.

Publisher exclusivamente bootstrap/prueba local controlada. Futuro definitivo **GitHub Actions→AWS OIDC→roles temporales separados publicación/deployment**, runtime pull separado, **sin access keys de largo plazo en GitHub**; aws:username local no se traslada directamente a role OIDC. No OIDC/roles/workflows creados. CI automático/CDproducción separado y exclusivamente humano Run workflow/workflow_dispatch, SHA/artefacto CI verde/digest exactos, sin Terraform apply automático; todo futuro, no implementado§70.

Fmt-check/validate **20:33:01.9841793Z–20:33:15.3162181Z**, **exit0 / Success! The configuration is valid.** Sin HCL/provider/lock cambiado, **sin plan/apply/init/AWS CLI/APIs de cuenta**. State/backup/planes conservados por hash, Docker config preservado; post-plan§68 **0/0/0/red14+ECR2no-op** histórico, no nuevo cloud check. Ningún byte ECR publicado ni recurso/IAM/Atlas modificado. Tarifa previa USD0.10/GB-mes conservada, no factura actual/Free Tier/costo mensual inventados.

**T-INFRA-007 conserva PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, falta policy aplicada/login/push/digest ECR/scanning/pull por digest/rollback/contrato/ownership004/reproducibilidad/seguridad; **T-INFRA-025 conserva PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, custodia/cifrado/backup protegido/recovery/dependencias/E025 integral pendientes. **Nueve gates pendientes**, nueve tareas parciales001–008/025 y otras21 pendientes; sin cambios002 GATE-G/H/AUTH-027 integral/004 o producción.

Solo publisher JSON nuevo y task/memoria editados, **sin stage/commit/push**, HEAD3312d220 conservado. Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-publisher-prep-20261112-203110\`, carpeta temporal no acredita custodia/retención. **BUILD LOCAL COMPLETO / PUBLISHER NO APLICADO / LOGIN-PUSH-DIGEST-SCANNING ECR PENDIENTES**. Detener sin siguiente incremento/publicación/cleanup automáticos.

## 71. 003 — primera publicación ECR autorizada, bloqueada en GetAuthorizationToken

**2026-11-12**, solicitud **T-INFRA-008 — VALIDAR PUBLISHER + PUBLICAR PRIMERA IMAGEN ECR / BUILD CLOUD CONTROLADO**, permite identidad/ListImages, validar JSON/imagen exacta, login/tag/un push autorizado, leer digest/scanning y pull por digest condicional a permisos, sin arrancar contenedor. Prohíbe rebuild silencioso/latest/segundo tag/push, Terraform apply/destroy/import, IAM modificado, workloads/OIDC y Git stage/commit/push. Baseline comprobada **feature/infrastructure**, HEAD **3312d220e3ff68851befc59e9a7cfa516185bdc8**, task/memoria pendientes + publisher JSON no rastreado/0staged; trabajo§70 conservado. Detalle [003 task §21](specs/003-infrastructure/task.md), v**0.17.0**.

**INFORMADO POR RESPONSABLE:** creó/adjuntó manualmente **ENVER-Local-ECR-Publisher** desde JSON local, sin AdministratorAccess/PowerUserAccess/ecr:*. No IAM creado/adjuntado/modificado por agente ni enumeración/auditoría de contenido/versión/adjuntos/grants remotos; §70 conserva estado histórico propuesto/no aplicado. JSON publisher validado localmente, **2Allow/9acciones**, hash **4a4feedc8cdd2057b396a40efcde5fb8e8bcdb42dc37df178e911dc6511ffbcd** y guards intactos; provisioning JSON separado/intacto. No simulación IAM/prueba de permisos por parsear archivo.

Prechecks **20:40:30.0861799Z–20:40:36.8246558Z**, perfil **enver-infra/us-east-1**: STS **exit0**, **enver-terraform-local/cuenta926925605666** correctos; ListImages **exit0/imageIds=[]/0 imágenes**, sin truncar paginación. Docker inspect **exit0**, **enver-api:git-3312d220e3ff**, ID exacto **sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda**, **linux/amd64**, fuentes del build§70 coinciden salvo documentación autorizada; sin rebuild. Credential helper Docker **desktop** existente/disponible, login no ejecutado todavía. Config repository/lifecycle§69 no consultada nuevamente.

Una llamada **aws ecr get-login-password --region us-east-1 --profile enver-infra**, **20:41:09.7233779Z–20:41:12.6294320Z**, **exit254 / AccessDeniedException**: **ecr:GetAuthorizationToken**, **Resource="*"**, principal **arn:aws:iam::926925605666:user/enver-terraform-local**, mensaje **no identity-based policy allows the ecr:GetAuthorizationToken action**. Fase token/login; **ninguna operación/recurso Terraform fallidos**. AWS_MAX_ATTEMPTS=1/retries0, sin reintento. No token válido obtenido/impreso/guardado; stdout solo memoria, variable retirada, **Docker login no invocado/exit NO APLICA**. Lector dedicado no abrió stderr UTF-16 por tratarlo como binario; Get-Content leyó solo error local, no repetición AWS/token.

**DETENCIÓN TOTAL DE OPERACIONES AWS/DOCKER DE PUBLICACIÓN**: sin tag registry local, **sin docker push/exit NO APLICA**, sin capas/manifiesto o digest Docker de push. **Sin imageDigest ECR/imagePushedAt/imageSizeInBytes nuevos**, ni post-ListImages: cantidad0 es pre-push fechada, no verificación posterior inventada. Sin scanning/findings consultados, no IN_PROGRESS/imagen segura afirmados; **sin pull por digest**, no digest publicado acreditado que descargar. Referencia repository@digest **NO DISPONIBLE**, no sha256 inventado. **Ni primer digest ECR ni previo compatible acreditados**, rollback real no probado/pendiente.

La adjunción informada no acredita Allow efectivo; el mensaje no identifica por sí solo si falta adjunto/versión/condiciones/propagación u otro detalle. Responsable debe revisar **cuenta/user/policy adjunta/versión activa/condiciones del permiso ya previsto** antes de autorizar una revalidación. No JSON/IAM ampliado/modificado automáticamente o espera/reintento a ciegas. GetAuthorizationToken ya usa Resource=* por no admitir ARN repository; no ensanchar actions repository para resolverlo. Separación provisioning/publisher y futuro GitHub OIDC/roles temporales§70 conservados, sin access keys de largo plazo en GitHub.

Solo validación local tras denegación: **20:42:01.9723825Z–20:42:15.2236512Z**, fmt-check/validate **exit0/Success! The configuration is valid.** **Sin plan/apply/destroy/import/init**, post-plan§68 **0/0/0/red14+ECR2no-op** histórico, no replan fresco. **242fuentes existentes/publisher** verificadas antes de editar docs; HCL/lock/provisioning/publisher/Dockerfile/producto/tests/spec/plan/004 intactos; state/backup/planes y Docker credentials config idénticos por hash. No imagen/byte ECR/recurso/IAM/Atlas modificado por agente, costo/factura no calculados; tarifa previa USD0.10/GB-mes conservada, sin Free Tier asumido.

**T-INFRA-007 conserva PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: evidencia real/config previas e inventario vacío nuevo, dependencias/retención/contrato pendientes. **T-INFRA-008 conserva PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, bloqueo operativo GetAuthorizationToken denegado; permisos efectivos/login/push/digest/scanning/pull/rollback/contrato/ownership004/reproducibilidad/seguridad pendientes. **T-INFRA-025 conserva PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, custody/cifrado/backup protegido/recovery/dependencias/E025 integral pendientes. **Nueve gates pendientes**, nueve tareas parciales001–008/025, otras21 pendientes; 002 GATE-G/H/AUTH-027 integral/004/producción no aceptados.

Solo task/memoria editados ahora; publisher JSON previo conservado sin cambios/no rastreado, **0staged/sin commit/push**, HEAD3312d220 intacto. Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-first-push-20261112-204027\`, sin password/token ni custodia/retención garantizadas por carpeta temporal. **PUBLICACIÓN BLOQUEADA ANTES DE LOGIN / SIN TAG-PUSH-DIGEST-SCANNING-PULL**. Detener para revisión humana, sin AWS adicional/reintentos/IAM/cleanup o siguiente incremento automático.

## 72. 003 — checkpoint primera imagen ECR publicada, evidencia informada por responsable

**2026-11-12**, solicitud **GIT CHECKPOINT CONTROLADO / NO NUEVAS OPERACIONES AWS**. Baseline comprobada **feature/infrastructure**, HEAD **3312d220e3ff68851befc59e9a7cfa516185bdc8**, publisher JSON nuevo/task y memoria modificados, **0staged**, sin archivos inesperados. Prechecks git status/diff --check/diff --stat/rev-parse correctos, diff --check exit0. Documentos todavía reflejaban bloqueo§71; se añade publicación posterior informada, sin borrar ni reinterpretar aquel intento fallido. Detalle [003 task §22](specs/003-infrastructure/task.md), v**0.18.0**.

**FUENTE: responsable, prompt CHECKPOINT PRIMERA IMAGEN ECR PUBLICADA**, no ejecución/revalidación independiente nueva del agente ni logs nativos adicionales inspeccionados en este checkpoint. Según esa evidencia comunicada, primera publicación real **Git SHA3312d220e3ff68851befc59e9a7cfa516185bdc8 → tag git-3312d220e3ff → ECR digest sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda**, repository **enver-dev-api/us-east-1**, **Docker push exit0 / pull de la misma imagen por digest exit0**. **imagePushedAt2026-11-12T21:36:45.185000Z**, **imageSizeInBytes101033417bytes**, no Size local101036277bytes§70. No se inventa actor, corrección IAM/otros permisos u horario adicional de ejecución.

**Referencia desplegable registrada:** `926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api@sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda`. El digest comunicado de ECR coincide numéricamente con ID local§70, pero **no se infiere como prueba desde Docker local**. No nuevo deploy/arranque/runtime A/B/CI por registrarlo.

**1 tag publicado / 3 objetos OCI observados según responsable:** índice, manifiesto **linux/amd64** y atestación de la misma publicación, **no3versiones distintas ni3tags/pushes**. No digests hijos inventados. **Scanning linux/amd64 IN_PROGRESS**, no estado final/imagen limpia; **findings/severidades PENDIENTES / NO DISPONIBLES**, **no findings=0**. No seguimiento cloud nuevo. **Rollback real PENDIENTE**: primer digest conocido, sin digest/version anterior publicada. Retener tag/manifiestos dependientes; sin cleanup/retag/mutabilidad relajada. Pull exit0 informado no prueba descarga sin caché o GetDownloadUrlForLayer efectivo; no se añade ese permiso al JSON ni se declara runtime IAM resuelto.

Publisher **infra/bootstrap/local-ecr-publisher-policy.json** intacto desde§70, SHA256 **4a4feedc8cdd2057b396a40efcde5fb8e8bcdb42dc37df178e911dc6511ffbcd**, parseo local válido **2Allow/9acciones exactas**: GetAuthorizationToken/BatchCheckLayerAvailability/InitiateLayerUpload/UploadLayerPart/CompleteLayerUpload/PutImage/BatchGetImage/DescribeImages/DescribeImageScanFindings. Sin secretos detectados en el archivo, AdministratorAccess/PowerUserAccess/ecr:*/ec2:*/iam:* o permisos provisioning/borrado/lifecycle. Token Resource=* por no admitir ARN, resto repository/guards cuenta/región/user conservados. Sin auditoría/modificación IAM remoto. Provisioning/HCL/lock/Dockerfile/producto/tests/spec/plan/004 intactos.

State/sufijos/backups/.terraform/planes/tfvars reales/credenciales Docker excluidos/no versionados, lock/ejemplo versionables; state/planes preservados por hash. **Ningún AWS/Docker/Terraform ejecutado en este checkpoint**: fmt/validate§71 exit0 y post-plan§68 0/0/0 se conservan fechados, no revalidación fresca. Tarifa previa USD0.10/GB-mes intacta; bytes informados no consumo/factura mensual total ni Free Tier supuesto.

**T-INFRA-007 PARCIAL / aceptación pendiente**. **T-INFRA-008 PARCIAL / publicación, digest y pull acreditados según responsable; scanning final, rollback y aceptación pendientes**, no COMPLETADA; reproducibilidad/seguridad/contrato/ownership004/CI/deployment restantes. **T-INFRA-025 PARCIAL / aceptación pendiente**, custodia/cifrado/backup protegido/recovery/dependencias/E025 integral sin resolver. **GATE-I-A y GATE-I-B PENDIENTE / NO SATISFECHO**, nueve gates abiertos, nueve tareas parciales001–008/025 y otras21 pendientes. Sin cierre002 GATE-G/H/AUTH-027 integral/004/producción.

Autorizado stage/commit de **solo publisher JSON/task/memoria**, incluyendo preparación§70/bloqueo§71 e informe posterior§72, mensaje **feat(infra): publish first ecr image by digest**. Verificar commit/árbol limpio/0staged después; hash final comunicado al cierre, sin autorreferencia en el propio commit. No amend/squash/merge/rebase/push Git. Evidencia privada local de revisión `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-image-checkpoint-20261112-215724\`, resumen de fuente informada/hashes/exclusiones/revisión JSON/Git, no logs nuevos de publicación ni custodia/retención garantizadas por carpeta temporal. **Sin AWS/IAM/Docker/Terraform modificado/ejecutado por agente en este turno.** Detener tras checkpoint, sin siguiente incremento/scanning/push/workloads/cleanup automáticos.
