# Memoria de ENVER Backend

## 1. Estado actual

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
