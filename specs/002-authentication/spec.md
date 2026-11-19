# Especificación 002 — Autenticación de ENVER Backend

## 1. Identificación, objetivo y autoridad

| Elemento | Valor |
| --- | --- |
| Solución / proyecto | `Enver.Backend` / `Enver.API` |
| Fase funcional activa | `002-authentication`; spec aprobado y plan aprobado para descomposición documental posterior; sin tareas ni implementación |
| Versión documental | 0.3.0 |
| Estado del documento | **APROBADO PARA PLANIFICACIÓN** |
| Plataforma | C#, ASP.NET Core y .NET 10 |
| Requisitos | 48, AUTH-001 a AUTH-048 |
| Estado técnico de autenticación | NO IMPLEMENTADA / NO VERIFICADA por esta entrega |
| Autoridad principal | [Constitución canónica 1.0.0](../../.specify/memory/constitution.md) |
| Gobierno y registro | [AGENTS.md](../../AGENTS.md), [memoria.md](../../memoria.md), [README.md](../../README.md) |
| Antecedentes preservados | [Spec 001](../001-backend-foundation/spec.md), [plan 001](../001-backend-foundation/plan.md), [tareas 001](../001-backend-foundation/task.md) |

### 1.1. Objetivo

Definir qué deberá cumplir el módulo de autenticación: registro de una identidad institucional UTTT, login con emisión de JWT de acceso y logout mediante revocación distribuida. Establecer contratos, garantías de seguridad, límites, criterios de aceptación y trazabilidad sin implementar controladores, servicios, repositorios, entidades, DTOs, pruebas ni infraestructura.

La apertura formal de 002 autorizó originalmente crear este spec como borrador y actualizar los tres documentos de estado enlazados. La solicitud posterior de aprobación autoriza actualizar exclusivamente esos cuatro documentos, incorporar AD-01 a AD-08 con sus precisiones, registrar AD-09 a AD-12 como diferidas total o parcialmente y pasar a **APROBADO PARA PLANIFICACIÓN**. Este estado habilita elaborar `plan.md` en una entrega posterior, pero **esta intervención no lo genera**, no crea `task.md`, no autoriza implementación ni ejecución técnica y no reforma la constitución. Su referencia raíz [constitution.md](../../constitution.md), la fuente canónica y los tres documentos de 001 permanecen intactos. Los límites históricos de cada entrega no se reinterpretan como permisos técnicos ni como prohibición permanente de planificación posterior autorizada.

El párrafo anterior registra la entrega APR-02 (v0.2.0). **APR-03**, acto posterior expresamente autorizado, aprueba el ajuste mínimo de AUTH-022/027/043 y sus referencias Redis para resolver la discrepancia con PLD-AUTH-08 del [plan](plan.md). Esta versión 0.3.0 conserva **APROBADO PARA PLANIFICACIÓN**; el plan v0.2.0 queda **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**, sin aceptación técnica ni autorización de implementación. Solo se actualizan spec, plan y los tres documentos de estado; no se genera `task.md`. La arquitectura no incorpora ledger MongoDB, segundo almacén duradero obligatorio ni transacciones distribuidas. El historial y la resolución constan en memoria §15 y plan §23.1.

### 1.2. Fuentes y estados

- **C §n:** apartado de la constitución canónica. La referencia raíz no es una segunda constitución.
- **SOL-02 §n:** bloque numerado de decisiones aprobadas en la solicitud del responsable que abre formalmente 002 (identidad, autenticación, JWT, contraseñas, logout/Redis, MongoDB, cuotas, versionamiento, HTTP, logs, seguridad, DevSecOps, exclusiones, trazabilidad y estado).
- **APR-02 / AD-nn:** solicitud posterior del responsable que aprueba AD-01 a AD-08, ajusta las cuotas de registro, ratifica parcialmente AD-10 y difiere los detalles de AD-09 a AD-12. Prevalece sobre propuestas previas de este spec; su trazabilidad y alcance están en §10 y en `memoria.md` §13.
- **APR-03:** aprobación posterior del responsable del ajuste normativo mínimo Redis: continuidad confiable, reacción a pérdida detectada y riesgo residual no observable; modifica solo AUTH-022/027/043 y referencias directamente dependientes, sin reabrir arquitectura ni autorizar ejecución. No sustituye las fuentes históricas APR-02; registro en `memoria.md` §15.
- **CTX:** contexto comunicado por el responsable: MongoDB Atlas, Redis, AWS, Terraform, GitHub Actions, dos instancias, balanceador, HTTPS y observabilidad. Selección no equivale a despliegue.
- **FND-nnn:** requisito real del spec 001, referenciado sin cambiar su estado. PLD/TBF son antecedentes de diseño/preparación, no evidencia de ejecución ni tareas de este módulo.
- **C — Confirmado en origen:** obligación constitucional o decisión expresa del responsable. APR-02 aprueba este spec para planificación, no para aceptación técnica ni implementación.
- **P — Propuesto / candidato:** concreción pendiente de calibración o ratificación técnica; en particular los objetivos AD-09 no son SLA ni resultados.
- **AD-nn:** identificador estable de decisión en §10. AD-01 a AD-08 están aprobadas; AD-09, AD-11 y AD-12 diferidas; AD-10 parcialmente aprobada y con detalle diferido. No son tareas ni evidencias de ejecución.
- **Observado / informado:** lectura de archivos / declaración del responsable, respectivamente; no se confunden con una verificación nueva.

Prioridades: **P0**, seguridad, integridad o gobierno esenciales; **P1**, capacidad funcional u operativa necesaria. No son secuencia de ejecución. Se conservan los 48 identificadores AUTH. Todos permanecen **sin implementación ni verificación acreditadas por esta entrega**. La aprobación documental permite planificar con detalles técnicos explícitamente diferidos; un criterio dependiente de ellos no se declara técnicamente satisfecho hasta resolverlos y aportar evidencia.

### 1.3. Línea base y discrepancias aclaradas

- Git observa integración parcial de foundation mediante PR #1, commit `dcf04a4` y merge `bef06b6` en `develop`; `feature/authentication` parte de ese merge. Esto no completa integralmente 001.
- El responsable confirma compilación correcta en .NET 10, funcionamiento de `GET /health` y corrección de una vulnerabilidad High de `Microsoft.OpenApi`. La lectura confirma `net10.0`, registro/mapeo del health check básico y referencia 2.13.0. No se repitieron build, peticiones o escaneos; falta adjuntar evidencia verificable del aviso corregido en una fase autorizada.
- `/health` representa salud básica del host, no readiness MongoDB/Redis. No se observa autenticación JWT ni integración de esas dependencias en el host actual.
- 001 conserva 49 FND, 28 PLD, 38 TBF PENDIENTE / NO EJECUTADA y 17 DEC con sus estados históricos. La selección posterior de productos y parámetros no cierra íntegramente esos asuntos ni aprueba retrospectivamente los borradores.
- La autorización actual aclara la desalineación de fase en los documentos de estado; no reforma los textos constitucionales históricos. Las rutas preliminares `/api/auth/*` se sustituyen en este alcance por `/api/v1/auth/*`, expresamente aprobadas; no se crean alias.
- `.vs/`, `bin/`, `obj/` y `.csproj.user` están versionados sin `.gitignore`. La higiene Git queda fuera de esta entrega. No se dispone de la rúbrica docente original: la trazabilidad académica utiliza lo transcrito en C §2.2 y lo comunicado en SOL-02.

## 2. Alcance, actores y precondiciones

### 2.1. Incluido

Una sola cuenta ENVER con nombre completo, correo institucional y contraseña; unicidad de correo normalizado; login propio del backend; JWT HS256 de acceso; sesiones concurrentes; logout del token presentado; diseño de persistencia `users` en MongoDB Atlas, revocación y cuotas compartidas Redis; seguridad, errores, privacidad, auditoría y criterios de verificación futura.

El teléfono no es requisito para crear la cuenta base. Un futuro perfil de repartidor se habilitará sobre la misma identidad y solicitará teléfono en su propia especificación, sin otro registro de autenticación. No se modelan aquí sus datos.

### 2.2. Actores

| Actor | Responsabilidad o capacidad |
| --- | --- |
| Persona solicitante no autenticada | Solicitar registro e intentar login con correo institucional y contraseña. |
| Persona autenticada / cliente móvil ENVER | Presentar su access token y solicitar logout de ese token. No se presupone interfaz implementada. |
| Backend ENVER | Validar solicitudes, proteger credenciales, emitir/validar JWT y aplicar revocación, límites y auditoría. |
| MongoDB Atlas | Persistencia principal de usuarios y garantía de unicidad del correo normalizado. |
| Redis compartido | Estado de seguridad de revocación y coordinación de cuotas; no autoridad de identidad ni almacenamiento de contraseñas. |
| Responsable del proyecto / operación autorizada | Aprobar decisiones, configurar ambientes, custodiar secretos y autorizar verificación/entrega futura; no implica un rol administrativo de la API. |

### 2.3. Precondiciones futuras de operación

Estos son requisitos de entrada para operar, no infraestructura existente:

1. Spec y diseño aplicables aprobados, autorización técnica independiente y configuración válida por ambiente.
2. MongoDB Atlas accesible con credencial mínima, colección lógica e índice único listos antes de admitir registros.
3. Redis compartido con garantías de revocación y cuotas aprobadas, estado confiable y controles de pérdida/expulsión definidos.
4. Clave HS256 externa, aleatoria y suficientemente entrópica; issuer/audience y parámetros temporales coherentes entre instancias.
5. HTTPS en producción y entornos publicados, confianza explícita en proxies, relojes sincronizados y límites de entrada vigentes.
6. No se exige JWT para register/login. Logout requiere un JWT auténtico dentro de vigencia; el caso ya revocado solo se admite bajo la excepción idempotente de AUTH-023.

## 3. Contratos funcionales HTTP

### 3.1. Rutas y estados confirmados

| Operación | Acceso | Entrada funcional | Éxito | Efecto |
| --- | --- | --- | --- | --- |
| `POST /api/v1/auth/register` | Anónimo, sujeto a límites | Nombre completo, correo institucional y contraseña | `201 Created` | Crear una sola identidad persistente. |
| `POST /api/v1/auth/login` | Anónimo, sujeto a límites | Correo institucional y contraseña | `200 OK` | Entregar un JWT de acceso tras autenticar correctamente. |
| `POST /api/v1/auth/logout` | Bearer, sujeto a límites | Token presentado mediante `Authorization: Bearer` | `204 No Content` | Confirmar revocación del token presentado; sin cuerpo de respuesta. |

No se define `GET /users/{id}`, `GET /me`, listado de usuarios ni una ruta `Location` ficticia para justificar el 201. No se modifica `/health` y no se asigna alcance de autorización de negocio por el mero login.

### 3.2. Contratos conceptuales aprobados — AD-01/AD-02

APR-02 aprueba los siguientes contratos conceptuales. Son especificación, no DTOs implementados:

| Mensaje | Campos aprobados | Restricciones |
| --- | --- | --- |
| Registro: solicitud | `fullName`, `email`, `password`, todos strings obligatorios | Sin teléfono, rol, estado, identificador asignado por cliente ni datos de repartidor. `confirmPassword` pertenece principalmente al cliente y nunca se persiste. |
| Registro: respuesta | `id` como UUID canónico string, `fullName`, `email` normalizado | Sin hash ni token. No realizar login automático tras el registro; el flujo de login es independiente. |
| Login: solicitud | `email`, `password`, strings obligatorios | No aceptar filtros, documentos de consulta ni operadores MongoDB. |
| Login: respuesta | `accessToken`, `tokenType` con valor `Bearer`, `expiresIn` en segundos | `expiresIn` inicial 900; el vencimiento autoritativo está en `exp`. Sin refresh token. |
| Logout: solicitud | Sin cuerpo funcional; Bearer en cabecera | Nunca token en URL, query string, logs o un segundo campo ambiguo. |

Los mensajes con cuerpo usan `application/json` UTF-8 y los nombres exactos camelCase del contrato; propiedades desconocidas o duplicadas, tipos incorrectos y null en campos obligatorios producen 400. No se convierten arrays/objetos a strings. `confirmPassword`, teléfono, rol, estado o identificador enviados como propiedades desconocidas se rechazan y no se persisten. Logout no admite cuerpo funcional; un cuerpo no vacío se rechaza con 400. Los tokens solo se entregan por el canal autorizado, nunca en URLs o redirecciones. La configuración concreta del serializador corresponde al plan sin cambiar estos resultados observables.

### 3.3. Validación y límites

| Dato | Regla aprobada | Límites y tratamiento |
| --- | --- | --- |
| Nombre completo | Obligatorio y no vacío tras recortar únicamente espacios externos; sin imponer número de palabras o apellidos. | Máximo 200 valores escalares Unicode; conservar mayúsculas, acentos y espacios internos; rechazar caracteres de control. |
| Correo institucional | Trim externo y minúsculas invariantes; dirección ASCII con dominio exacto `uttt.edu.mx`; sin espacios internos ni controles. | Máximo general 254 bytes ASCII, local-part máximo 64 bytes. Sintaxis ASCII no entrecomillada, sin nombres de presentación, comentarios, listas ni puntos iniciales/finales/consecutivos. No eliminar puntos ni sufijos `+`. |
| Contraseña de registro | Inicialmente 12 a 128 valores escalares Unicode; reglas centralizadas/configurables. | Máximo 512 bytes UTF-8; no trim, normalización Unicode, cambio de case ni truncamiento. No se imponen composición o caducidad periódica no aprobadas. |
| Contraseña de login | Verificar el secreto recibido sin transformarlo y mantener límites de entrada. | No reaplicar automáticamente una futura política mínima de creación que impida autenticar cuentas existentes. La política de creación y la verificación son responsabilidades distintas. |
| Mensaje HTTP | Validar estructura y límites antes de trabajo costoso. | Cuerpo máximo 16 KiB; profundidad JSON máxima inicial 4; exceso de cuerpo 413, contenido no soportado 415. Timeouts concretos al plan, sin esperas ilimitadas. |
| JWT presentado | Bearer solo en `Authorization`, estructura/tamaño validados antes de confiar en claims. | Máximo inicial 4 KiB; no admitir JWT más grande por presentarse en cabecera en lugar del cuerpo. |

Debe validarse primero un límite de transporte para no procesar entradas desmesuradas antes de medir campos. La misma normalización aprobada se utiliza en registro, login, unicidad y ámbito de cuotas de identidad. El índice único no sustituye la validación de dominio. `usuario@uttt.edu.mx` es un ejemplo de formato, no una cuenta real ni prueba de titularidad.

El correo es insensible a mayúsculas por política de identidad ENVER; no se afirma equivalencia de buzones según un proveedor UTTT no contrastado. Nombre conserva case y contraseña es sensible a case. El formato institucional **no verifica la posesión del buzón ni la pertenencia académica**. AD-03 aprueba no implementar verificación de correo en 002; no se añade envío de correos, directorio UTTT ni un flag de verificación acreditada.

### 3.4. Estados de error y confidencialidad

| Situación | Respuesta objetivo | Garantía |
| --- | --- | --- |
| Campos ausentes/incorrectos, dominio no permitido, JSON inválido | `400 Bad Request` | Sin escritura funcional ni emisión de token; detalles por campo permitidos solo si no revelan secretos o existencia de cuentas. |
| Login con correo inexistente, contraseña incorrecta o cuenta `Disabled` | `401 Unauthorized` | Mismo contrato y mensaje genérico de autenticación incorrecta. No reflejar valores ni distinguir los motivos. |
| Token ausente, manipulado, expirado, emisor/audiencia/algoritmo inválido, revocado | `401 Unauthorized` en recursos protegidos | Rechazo seguro; desafío Bearer cuando corresponda, sin causas internas sensibles. |
| Registro no permitido o correo normalizado en conflicto | `409 Conflict` | Mensaje genérico de conflicto de registro, sin nombre, estado de cuenta, identificador o datos del registro existente. |
| Cuota excedida | `429 Too Many Requests` | `Retry-After` cuando sea calculable; no prometer que reintentar garantiza éxito. |
| Dependencia necesaria indisponible o estado de seguridad no confiable | `503 Service Unavailable` | No aparentar credenciales inválidas ni éxito; sin nombrar la dependencia o revelar detalles de conexión; sin relajar controles. |
| Error inesperado | `500 Internal Server Error` | Representación segura y correlacionada, nunca stack trace, excepción del driver o secreto. |
| Cuerpo excesivo / tipo de contenido no soportado | `413 Content Too Large` / `415 Unsupported Media Type` | Rechazo temprano; distinguir respuestas del borde de las emitidas por la API. |

AD-01/AD-02 aprueban los estados de esta sección y **Problem Details compatible con RFC 9457**, con `Content-Type: application/problem+json`. El cuerpo conceptual incluye `type`, `title`, `status`, `detail`, `code`, `traceId` y `errors` únicamente cuando exista validación por campo. No incluir inicialmente `instance`; `status` coincide con el estado HTTP real. Se adopta inicialmente `type=about:blank`, sin inventar URLs de problemas. `traceId` será generado o validado por servidor y coherente con la correlación operativa; no se confía ciegamente en identificadores externos.

| HTTP | `code` aprobado | Mensaje genérico seguro |
| --- | --- | --- |
| 400 | `validation_failed` | La solicitud contiene datos inválidos. |
| 401 | `authentication_failed` | No fue posible autenticar la solicitud. |
| 409 | `registration_conflict` | No fue posible completar el registro. |
| 429 | `rate_limited` | Se superó temporalmente el límite de solicitudes. |
| 500 | `internal_error` | Ocurrió un error al procesar la solicitud. |
| 503 | `temporarily_unavailable` | El servicio no está disponible temporalmente. |

No exponer existencia o estado de cuenta en detalles, ni nombres MongoDB/Redis, stack traces, infraestructura, claves, secretos o valores sensibles. `errors` solo identifica campos y mensajes seguros, nunca copia los valores recibidos. Errores de autenticación y demás respuestas sensibles usan `Cache-Control: no-store`. Para 429 se envía `Retry-After` cuando corresponda y sea calculable, redondeado conservadoramente; no implica garantía de éxito al reintentar. Los códigos de detalle de 413/415 y la integración con rechazos del borde son detalles del plan; no alterarán sus estados, confidencialidad ni compatibilidad. No se prescribe un envoltorio global de éxitos.

Logout repetido con token auténtico ya revocado pero aún dentro de su ventana temporal: 204. Expirado, falsificado o ausente: 401. Estado Redis no verificable: 503. Solo logout permite confirmar el efecto con un token ya revocado; no permite usarlo en otro recurso ni reducir el TTL existente.

El 409 genérico reduce la exposición adicional, pero el contraste 201/409 **permite inferir existencia o indisponibilidad del registro**. Se documenta el riesgo residual de enumeración de registro; no se promete eliminarlo conservando ese contrato. En login se protegen también canales laterales de tiempos, cuotas, cabeceras y logs públicos, no solo el texto.

### 3.5. Cuotas académicas iniciales aprobadas — AD-08

Todas las cuotas son configurables, distribuidas y compartidas por todas las instancias, finalmente coordinadas mediante Redis. Se aplican conjuntamente, no como alternativas que permitan evadir otro límite.

| Operación | Ámbito | Máximo inicial / ventana |
| --- | --- | --- |
| REGISTER | Email normalizado | 3 intentos / 15 minutos |
| REGISTER | Origen | 20 intentos / 15 minutos |
| LOGIN | Origen + email normalizado | 5 intentos / minuto |
| LOGIN | Email normalizado | 10 intentos / minuto |
| LOGIN | Origen | 60 intentos / minuto |
| LOGOUT | Cuenta autenticada | 30 intentos / minuto |
| LOGOUT | Origen | 300 intentos / minuto |

La aprobación adopta ventanas móviles y consumo coordinado; el algoritmo y su evidencia pertenecen al plan/AD-12. Solicitudes admitidas a procesamiento consumen cuota aunque fallen o la cuenta no exista; los rechazos 429 no prolongan indefinidamente una penalización. No se convierte la limitación en bloqueo permanente, `Disabled` o sanción. Exceso: 429 con `Retry-After` cuando corresponda. Los contadores por email se pseudonimizan adecuadamente mediante un mecanismo con clave por concretar, no email en claro ni hash simple enumerable; su vida y cardinalidad son acotadas.

El techo REGISTER por origen se incrementa expresamente de la propuesta anterior de 3 a **20/15 minutos** para reducir bloqueo bajo NAT universitario. Combinar identidad y origen **no elimina todas las restricciones de NAT**; las cuotas son configuración académica inicial, ajustable sin reescribir lógica, no calibración productiva acreditada. No se acepta un fallback local como cuota global cuando Redis falla.

### 3.6. Objetivos candidatos, no SLA — AD-09 diferida

| Operación | p95 candidato | p99 candidato |
| --- | --- | --- |
| Login | ≤ 1.000 ms | ≤ 2.000 ms |
| Registro | ≤ 1.500 ms | ≤ 3.000 ms |
| Logout | ≤ 250 ms | ≤ 500 ms |

Estos valores solo son candidatos de evaluación para calibración durante pruebas de rendimiento; no son SLA, gates medidos ni capacidades verificadas. El plan definirá perfil, concurrencia, carga, volumen, recursos, tiempos de dependencia, timeouts, unidades/muestras y tratamiento de rechazos. No se adoptan automáticamente otros números sugeridos en la revisión (concurrencia, presupuestos o disponibilidad) ni las metas propuestas de 001. **Nunca reducir la seguridad del hashing para cumplir estos candidatos.** La respuesta fail-closed ante una dependencia caída preserva seguridad, no demuestra disponibilidad.

## 4. Flujos funcionales y límites de atomicidad

### 4.1. Registro

1. Recibir solicitud por el canal permitido; aplicar límites de transporte y las cuotas de identidad/origen de §3.5 antes del hashing cuando corresponda.
2. Validar estructura, nombre, dominio institucional y contraseña, conforme a la configuración aprobada; normalizar correo de forma consistente.
3. Evaluar únicamente condiciones de registro expresamente aprobadas. No inventar roles o requisitos de repartidor.
4. Obtener hash mediante `PasswordHasher<TUser>` y persistir el documento mínimo con UUIDv4 generado por backend, estado `Active` y timestamps UTC del servidor; en creación `createdAt = updatedAt`.
5. Resolver la unicidad en MongoDB mediante índice único, no solo con una consulta previa. Dos instancias no pueden crear dos cuentas con el mismo correo normalizado.
6. Confirmar creación mediante 201 y `id`, `fullName`, `email` normalizado; registrar evento seguro. No dar éxito antes de conocer el resultado persistente según garantías técnicas a definir en plan. No iniciar login ni emitir JWT automáticamente.
7. Duplicado/conflicto: 409 genérico. Si la escritura tiene resultado incierto por fallo de red, devolver error seguro sin afirmar que no hubo creación ni borrar/reintentar ciegamente. Una repetición no puede crear otro usuario con el mismo correo.

### 4.2. Login

1. Aplicar límites de transporte y controles de abuso compartidos para origen/identidad, también cuando la cuenta no exista.
2. Validar y normalizar correo conforme a la misma política del registro.
3. Consultar usuario por igualdad sobre el email normalizado. No convertir objetos del cliente en filtros NoSQL.
4. Verificar mediante `PasswordHasher<TUser>`. Para cuenta inexistente, verificar contra un hash ficticio válido preparado previamente con el mismo hasher; no generarlo por solicitud ni usar sleeps/jitter como defensa principal. No omitir trabajo comparable solo por detectar `Disabled`; no se prometen tiempos idénticos en toda red. No reaplicar una futura política mínima de creación que bloquee credenciales existentes.
5. `Active` puede autenticarse con credenciales/controles válidos; `Disabled` y credenciales incorrectas reciben el mismo 401 genérico. No convertir una dependencia caída en usuario inexistente. Un login normal no modifica `updatedAt`; si aparece `SuccessRehashNeeded`, contemplar actualización segura del hash y de `updatedAt`, sin sobrescritura concurrente indebida.
6. Tras autenticar y superar controles necesarios, emitir JWT con `sub` UUID canónico, `jti` de 32 bytes aleatorios criptográficos codificados Base64url sin padding, `iss=enver-api`, `aud=enver-mobile`, `iat` NumericDate UTC y `exp=iat+900` inicialmente. Usar HS256 y `kid` de un conjunto aprobado por ambiente. No emitir si no se garantiza el estado de seguridad requerido.
7. Entregar 200 con token y metadatos mínimos; auditar sin almacenar el secreto. Cada login exitoso puede crear otra sesión: no invalida automáticamente las anteriores.

### 4.3. Logout

1. Aplicar límites pertinentes y obtener exclusivamente el Bearer presentado.
2. Validar tamaño/formato, firma HS256, claims y vigencia antes de confiar en `jti`/`exp` o escribir Redis.
3. Consultar/registrar revocación de forma coordinada. Si el mismo token auténtico ya está revocado y sigue en su ventana temporal, confirmar idempotentemente la revocación según AUTH-023.
4. Mantener la marca al menos hasta `exp + skew + margen conservador`, conforme AUTH-024; no reducirla en llamadas repetidas ni usar una caché local de “no revocado”.
5. Devolver 204 únicamente tras confirmar estado de revocación con la garantía aprobada. Indisponibilidad o resultado incierto: error controlado, nunca éxito supuesto.
6. Las comprobaciones protegidas posteriores a la confirmación de revocación, en cualquier instancia, rechazan ese token mientras exista continuidad confiable del estado compartido, conforme AUTH-022. Ante pérdida o discontinuidad detectada, integridad incierta o estado no verificable, se aplica fail-closed según AUTH-026/027; una marca ausente no autoriza si el estado no es confiable. No se promete cancelar retroactivamente solicitudes que ya superaron el control; su ventana se documentará.

Las escrituras de usuario y las marcas Redis no se presentan como una transacción distribuida. No se necesita revocar otros JWT de la cuenta ni guardar el token completo para revocar el presentado. Expirar un token naturalmente no requiere insertar una marca con TTL cero/negativo.

## 5. Requisitos identificados

Las cláusulas de entrega de AUTH-001/047/048 se conservan como registro del alcance APR-02, sin reescribir esos requisitos: no niegan la creación posterior autorizada del plan ni su sincronización APR-03. El estado vigente y los límites de esta entrega están en §1.1 y en el cierre APR-03 de §11; no se crean tareas ni se autoriza implementación.

### 5.1. Gobierno, identidad y registro

### AUTH-001 — Alcance documental y aprobación

- **Descripción:** Mantener la separación entre decisiones confirmadas, propuestas, aprobación del spec y autorizaciones de planificación, implementación y verificación.
- **Prioridad:** P0.
- **Fuente:** C §§1–3, 9–11; SOL-02 §§14–15; APR-02 (estado y límites).
- **Referencia FND:** FND-001, FND-048, FND-049 (límite histórico, no ampliado).
- **Criterio de aceptación:** Cada AUTH identifica fuente, estado, prioridad y criterio; los únicos cambios de esta entrega son este spec y los tres documentos autorizados. Constitución y 001 intactos; sin plan/tareas ni ejecución técnica.
- **Estado:** C; documento APROBADO PARA PLANIFICACIÓN; sin aceptación técnica ni autorización de implementación.

### AUTH-002 — Una identidad institucional extensible

- **Descripción:** Crear una cuenta base con nombre, correo institucional y contraseña. No exigir teléfono ni crear otra identidad al habilitar un futuro perfil de repartidor.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §1; C §4.
- **Referencia FND:** FND-002, FND-003, FND-014.
- **Criterio de aceptación:** Un registro sin teléfono puede ser válido; los contratos/documento base no incluyen CLABE o datos de repartidor. No hay endpoint de registro de repartidor ni asignación de roles funcionales por campos del cliente.
- **Estado:** C; habilitación de repartidor fuera de alcance actual.

### AUTH-003 — Contratos versionados por ruta

- **Descripción:** Exponer únicamente las tres operaciones de §3.1 bajo `/api/v1/auth/`, conservando los estados de éxito aprobados.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §§2, 5, 8–9; APR-02 / AD-01.
- **Referencia FND:** FND-005 (sin aprobar todo su estilo), FND-025, FND-031, FND-040.
- **Criterio de aceptación:** Registro/login/logout producen 201/200/204 en sus casos exitosos y respetan §3.2: registro recibe `fullName`, `email`, `password` y devuelve `id`, `fullName`, `email` normalizado; login recibe `email`, `password` y devuelve `accessToken`, `tokenType=Bearer`, `expiresIn=900` inicialmente. Logout solo recibe el JWT por `Authorization: Bearer`, sin cuerpo funcional ni cuerpo en 204. No se añaden aliases sin versión ni endpoint de consulta de usuario.
- **Estado:** C; contratos y proyecciones aprobados en AD-01.

### AUTH-004 — Registro y resultado persistente

- **Descripción:** Registrar una sola identidad válida y responder solo después de conocer la creación persistente, con conflicto genérico ante duplicado o registro no permitido.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§1, 6, 9; C §8; APR-02 / AD-01, AD-05.
- **Referencia FND:** FND-014, FND-028, FND-029, FND-031.
- **Criterio de aceptación:** Registro válido crea un documento y produce 201 únicamente tras confirmación según garantías que se definirán en el plan; conflicto produce 409 sin datos existentes. Fallo o resultado incierto no produce éxito ficticio ni borrado compensatorio del usuario creado por otra solicitud. La respuesta contiene solo la proyección de §3.2, sin token ni login automático.
- **Estado:** C; ausencia de login automático aprobada; garantías físicas de confirmación al plan.

### AUTH-005 — Dominio y normalización coherentes

- **Descripción:** Usar correo institucional válido del dominio exacto `uttt.edu.mx` y una representación normalizada consistente en acceso, índices y cuotas.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§1–2, 6; C §5.2; APR-02 / AD-01, AD-03.
- **Referencia FND:** FND-011, FND-015, FND-024, FND-028.
- **Criterio de aceptación:** Aplicar trim externo y minúsculas invariantes, sin quitar puntos ni sufijos `+`. Rechazar espacios internos, controles, sintaxis no admitida, dominio distinto de `uttt.edu.mx`, subdominios y sufijos engañosos. Cumplir 254 bytes ASCII generales y 64 en local-part según §3.3. Variantes que solo difieren en case/espacios externos identifican una cuenta; acceso, índice y cuotas usan la misma normalización. No acreditar posesión del buzón.
- **Estado:** C; normalización, sintaxis y límites aprobados en AD-01; limitación institucional aceptada en AD-03.

### AUTH-006 — Política inicial de contraseña

- **Descripción:** Admitir en registro inicialmente de 12 a 128 valores escalares Unicode y hasta 512 bytes UTF-8, con política centralizada/configurable y sin transformación del secreto.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §4; C §§5.2, 8; APR-02 / AD-01.
- **Referencia FND:** FND-011, FND-041, FND-044.
- **Criterio de aceptación:** Fronteras 11/12/128/129 escalares y 512 bytes respetan §3.3. No trim, normalización, conversión de case ni truncamiento silencioso; el secreto completo se verifica. Login mantiene límites de entrada pero no reaplica automáticamente una futura política mínima de creación que bloquee cuentas existentes.
- **Estado:** C; unidad Unicode y límite en bytes aprobados en AD-01.

### AUTH-007 — Nombre y datos mínimos

- **Descripción:** Exigir nombre completo como dato funcional, sin requisitos culturales inventados, y no persistir confirmación de contraseña ni datos ajenos al registro base.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §§1, 6, 10; C §§4, 5.2; APR-02 / AD-01.
- **Referencia FND:** FND-003, FND-011, FND-015.
- **Criterio de aceptación:** Nombre obligatorio y no vacío tras recortar solo espacios externos; máximo 200 valores escalares Unicode, sin controles. Conservar mayúsculas, acentos y espacios internos, sin exigir dos apellidos. Propiedades ajenas al contrato, incluida `confirmPassword`, teléfono o datos de repartidor, se rechazan conforme AUTH-008 y no se persisten.
- **Estado:** C; reglas de nombre aprobadas en AD-01.

### AUTH-008 — Entradas acotadas y resistencia a NoSQL injection

- **Descripción:** Validar estructura y tipos, no aceptar filtros/operadores del cliente, impedir asignación de propiedades sensibles y limitar recursos antes de procesar/hashar entradas.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§4, 11; C §5.2; APR-02 / AD-01, AD-02.
- **Referencia FND:** FND-011, FND-025, FND-030, FND-044.
- **Criterio de aceptación:** Exigir `application/json` UTF-8 para cuerpos funcionales. Desconocidas, duplicadas, tipos incorrectos, null obligatorio, objetos/arrays/operadores en strings y profundidad superior a 4 producen 400 seguro, sin alterar consultas/estado. Cuerpo >16 KiB: 413; contenido no soportado: 415. Logout no admite cuerpo no vacío. Límites previos al trabajo costoso y errores conforme §3.4; no se coercionan arrays/objetos a strings ni se confía en filtros del cliente.
- **Estado:** C; límites y política JSON/HTTP aprobados en AD-01/AD-02.

### AUTH-009 — Unicidad ante registros concurrentes

- **Descripción:** Garantizar unicidad de email normalizado mediante un índice único en MongoDB Atlas, también con solicitudes concurrentes desde varias instancias.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§1, 6, 11; C §8; APR-02 / AD-05.
- **Referencia FND:** FND-015, FND-024, FND-028, FND-029.
- **Criterio de aceptación:** En una carrera de registros equivalentes se crea como máximo una cuenta; un ganador confirmado puede producir 201 y los duplicados 409 genérico. Consultar antes de insertar no se considera garantía suficiente. No se admite registro si falta la condición de unicidad requerida.
- **Estado:** C; índice único obligatorio aprobado en AD-05; preparación, consistencia física y comprobación de readiness al plan, no aceptación acreditada.

### 5.2. Login, hash y persistencia

### AUTH-010 — Login propio del backend

- **Descripción:** Autenticar por correo institucional y contraseña y emitir access token únicamente tras verificación válida y controles necesarios disponibles.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§2–3, 9; C §5.2; APR-02 / AD-01, AD-03, AD-04.
- **Referencia FND:** FND-009, FND-011, FND-014, FND-030.
- **Criterio de aceptación:** `Active` con credenciales válidas verificadas mediante `PasswordHasher<TUser>` y controles disponibles recibe 200 con §3.2. `Disabled`, cuenta inexistente o contraseña incorrecta no emiten token y reciben el mismo 401 genérico. No reaplicar automáticamente una futura política mínima de creación a credenciales existentes. No se usa proveedor externo, refresh token/MFA ni Identity completo por la sola adopción del hasher. Redis no disponible no habilita emisión sin controles.
- **Estado:** C; contratos, estados y hasher aprobados en AD-01/AD-03/AD-04.

### AUTH-011 — Error genérico y mitigación de enumeración

- **Descripción:** No distinguir públicamente correo inexistente de contraseña incorrecta; reducir diferencias observables y documentar el riesgo residual de registro.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§1–2, 11; C §5.2; APR-02 / AD-02, AD-04.
- **Referencia FND:** FND-013, FND-031, FND-035, FND-045.
- **Criterio de aceptación:** Cuenta inexistente, contraseña incorrecta y `Disabled` comparten 401 `authentication_failed`, contrato Problem Details y mensaje genérico; cuotas no dependen de que exista la cuenta. Para inexistente verificar contra hash ficticio válido preparado previamente con el mismo hasher, sin generarlo por solicitud ni sleeps/jitter como defensa principal; no omitir trabajo comparable por estado. Aplicar rate limiting antes del trabajo criptográfico cuando corresponda. Evaluar diferencias temporales sin prometer igualdad absoluta. Registro 201/409 conserva riesgo residual sin datos adicionales.
- **Estado:** C; mitigación aprobada en AD-04 y errores en AD-02; perfil/tolerancias de evaluación al plan/AD-09.

### AUTH-012 — Estado de cuenta controlado por servidor

- **Descripción:** Persistir un estado de usuario y definir qué estados permiten login, sin aceptar ese estado desde registro ni inventar un sistema de administración o sanciones.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §6; C §5.2; APR-02 / AD-03.
- **Referencia FND:** FND-010, FND-011, FND-015.
- **Criterio de aceptación:** Solo estados `Active` y `Disabled`; registro inicial `Active` asignado por servidor. `Active` puede autenticar con controles/credenciales válidos, pero no implica correo verificado; `Disabled` recibe el mismo 401 genérico. Sin operación pública de desactivación, administración, sanciones o estados/suspensión de repartidor. Cuenta ENVER separada de perfil de repartidor; no se infiere revocación global inmediata de JWT emitidos por un cambio de estado.
- **Estado:** C; mínimo e inicial aprobados en AD-03; no se implementa verificación institucional en 002.

### AUTH-013 — Hash especializado y mantenido

- **Descripción:** Usar obligatoriamente `ASP.NET Core PasswordHasher<TUser>`, sin criptografía personalizada ni cifrado reversible; no incorporar Identity completo solo por este uso.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §4; C §§5–6; APR-02 / AD-04.
- **Referencia FND:** FND-004, FND-012, FND-035, FND-036.
- **Criterio de aceptación:** El framework administra formato, salt, parámetros internos y versionamiento; este spec no fija manualmente iteraciones. Verificar el secreto completo por su API mantenida; contemplar `SuccessRehashNeeded` y actualización segura de hash/`updatedAt` sin sobrescrituras concurrentes. El mismo secreto en dos cuentas no produce una representación determinista común. Documentar versión/configuración efectiva y evaluar coste sin reducir seguridad por latencia. Nunca persistir ni registrar el original.
- **Estado:** C; selección obligatoria y política aprobadas en AD-04; versión, configuración efectiva y mecanismo de actualización al plan/framework.

### AUTH-014 — Documento conceptual de usuario

- **Descripción:** Mantener en MongoDB Atlas `users` únicamente con `_id`, `fullName`, `normalizedEmail`, `passwordHash`, `status`, `createdAt` y `updatedAt` según §6.1.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§1, 6, 10; C §§4, 5.2; APR-02 / AD-01, AD-03, AD-05.
- **Referencia FND:** FND-003, FND-014, FND-015, FND-041.
- **Criterio de aceptación:** `_id` UUIDv4 generado por backend, estable y público como UUID canónico string; no email. `status` según AUTH-012; timestamps UTC del servidor, iguales al crear. Login normal no cambia `updatedAt`; rehash persistido sí. No añadir `lastLoginAt`, roles, CLABE, historial, ratings, datos de repartidor ni campos sin requisito. Sin contraseña original, JWT completo, claves o conexiones en el documento; API solo proyecta §3.2.
- **Estado:** C; modelo aprobado en AD-05; BSON, driver y representación física al plan.

### AUTH-015 — Acceso e índices justificados

- **Descripción:** Consultar usuarios por email normalizado y referenciar identidad por identificador estable; aislar ambientes y justificar los índices en esos patrones.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §6; C §§2.2, 8; APR-02 / AD-05.
- **Referencia FND:** FND-014, FND-015, FND-024, FND-041.
- **Criterio de aceptación:** Igualdad por `normalizedEmail` usa índice único obligatorio; la búsqueda por `_id` usa su índice propio sin duplicarlo innecesariamente. No depender de scans o índices de repartidor. Plan definirá inicialización, confirmación de escritura/lectura y alcance de unicidad según topología; una creación confirmada debe poder observarse en login conforme a esas garantías. No presuponer unicidad global en sharding no diseñado.
- **Estado:** C; patrones/índice aprobados en AD-05; topología y read/write concerns al plan.

### 5.3. Emisión y validación JWT

### AUTH-016 — Emisión HS256 por ENVER

- **Descripción:** Emitir access tokens firmados HS256 con `iss=enver-api` y `aud=enver-mobile` desde el backend ENVER.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§2–3; APR-02 / AD-06.
- **Referencia FND:** FND-009, FND-012, FND-041.
- **Criterio de aceptación:** Solo una autenticación exitosa permite emitir el token; la firma usa clave externa aprobada y el validador admite únicamente HS256. Se rechazan `none`, algoritmos asimétricos y selección de algoritmo sin control en la cabecera. Emitir `kid` correspondiente al conjunto aprobado del ambiente, no resolverlo mediante búsquedas arbitrarias ni fallback inseguro.
- **Estado:** C; no selecciona todavía paquete JWT ni versión.

### AUTH-017 — Claims mínimos y jti único

- **Descripción:** Incluir `sub`, `jti`, `iss`, `aud`, `iat` y `exp`; subject estable de usuario y jti nuevo, no predecible y único por token dentro del emisor/ambiente.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§3, 5; C §5.2; APR-02 / AD-06.
- **Referencia FND:** FND-009, FND-011, FND-045.
- **Criterio de aceptación:** `sub` UUID canónico de usuario; `jti` nuevo de 32 bytes aleatorios criptográficos, Base64url sin padding (43 caracteres). `iat` NumericDate UTC entero en segundos Unix y `exp=iat+900` inicialmente; issuer/audience exactos. Dos logins comparten subject pero no jti; claims acotados y sin contraseña, hash, nombre/email innecesarios, CLABE, teléfono o permisos autoasignados. JWT firmado es legible, no cifrado.
- **Estado:** C; contenido/generación aprobados en AD-06; APIs concretas al plan.

### AUTH-018 — Validación completa antes de confiar en claims

- **Descripción:** Rechazar JWT ausente/alterado/inválido, algoritmo no permitido, firma incorrecta, issuer/audience ajenos, claims obligatorios ausentes o inválidos y vigencia no válida; luego comprobar revocación.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§3, 5, 11; C §5.2; APR-02 / AD-01, AD-06.
- **Referencia FND:** FND-009, FND-010, FND-011, FND-030, FND-045.
- **Criterio de aceptación:** JWT únicamente en `Authorization: Bearer`, máximo inicial 4 KiB. Validar firma, HS256, clave/`kid` aprobados, issuer, audience, `sub`, `jti`, `iat`, `exp`, tipos, longitudes, tiempos y revocación. `kid` desconocido se rechaza sin fallback; `nbf` se valida si existe, no se exige. Rechazar `iat` futuro fuera de tolerancia, `exp <= iat` y duración incompatible con la política. No usar jti no autenticado como identidad confiable. Revocado no autoriza recursos; solo logout tiene la excepción idempotente de AUTH-023.
- **Estado:** C; validaciones/límite aprobados en AD-01/AD-06; precisión y pruebas de frontera al plan.

### AUTH-019 — Entropía, custodia y rotación de clave

- **Descripción:** Usar para HS256 una clave aleatoria de al menos 256 bits de entropía (mínimo 32 bytes aleatorios), externa y aislada por ambiente; compartirla solo con emisores/validadores autorizados.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §3; C §5.1; APR-02 / AD-06, AD-10; requisito de seguridad HS256 como concreción técnica revisable.
- **Referencia FND:** FND-009, FND-012, FND-041.
- **Criterio de aceptación:** Sin contraseñas humanas, claves de ejemplo ni longitud Base64 como prueba de entropía. Claves independientes por ambiente; desarrollo User Secrets/variables externas, producción futura gestor seguro AWS con mínimo privilegio. Rotación: distribuir nueva a validadores, emitir con nueva, conservar anterior solo para validar tokens vigentes y retirarla al concluir su ventana. Usar `kid` del conjunto aprobado; desconocido se rechaza. Ante compromiso definir retiro extraordinario y reautenticación de afectados; todo poseedor HS256 puede emitir tokens. No confundirlo con logout ordinario individual.
- **Estado:** C; rotación conceptual aprobada AD-06 y gestión general AD-10; periodicidad, automatización y gestor AWS concreto diferidos.

### AUTH-020 — Duración y tolerancia temporal configurables

- **Descripción:** Configurar inicialmente `exp - iat = 900` segundos y skew máximo 30 segundos, coherentes entre instancias, sin valores mágicos dispersos.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§3, 5; C §8; APR-02 / AD-06, AD-07.
- **Referencia FND:** FND-009, FND-027, FND-041.
- **Criterio de aceptación:** Configuración inválida impide admitir tráfico dependiente; skew efectivo máximo 30 s (inicial 30 s), vida inicial 900 s y reloj UTC. Probar antes/en/después de `exp` y fin de aceptación. Revocación cubre `exp + skew + margen conservador`; no aumentar retroactivamente ventanas si marcas previas caducan antes. Cambios y recuperación se coordinan entre instancias según el plan.
- **Estado:** C; política aprobada AD-06/AD-07; precisión/fronteras y margen numérico al plan.

### AUTH-021 — Sesiones concurrentes y límites del Bearer

- **Descripción:** Permitir varios JWT de acceso simultáneos para una cuenta; cada logout afecta solo al presentado, sin refresh tokens ni cierre global implícito.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §§2, 5, 13; APR-02 / AD-06.
- **Referencia FND:** FND-006, FND-009, FND-010.
- **Criterio de aceptación:** Revocar A no revoca B del mismo `sub` UUID canónico; ambos tienen `jti` aleatorio distinto según AUTH-017. Bearer robado puede reutilizarse mientras sea válido y no revocado; jti no impide por sí solo replay. Retiro extraordinario de clave comprometida o recuperación de integridad no son logout ordinario ni promesa de cierre global público.
- **Estado:** C; mecanismos de vinculación de token o cierre global no incluidos.

### 5.4. Logout y Redis como estado de seguridad

### AUTH-022 — Logout distribuido confirmado

- **Descripción:** Invalidar el token presentado mediante una marca Redis compartida, confirmando revocación antes de responder 204.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§5, 9; C §8; APR-02 / AD-07; APR-03 (delimitación del modelo de fallos).
- **Referencia FND:** FND-006, FND-009, FND-016, FND-018, FND-028.
- **Criterio de aceptación:** Tras un logout 204 confirmado en una instancia, todas las instancias ENVER deben rechazar ese JWT en comprobaciones protegidas posteriores mientras exista continuidad confiable del estado compartido de revocación. Ante pérdida o discontinuidad detectada, integridad incierta o estado Redis no verificable, los accesos dependientes fallan de forma cerrada conforme AUTH-026/027; una marca ausente no se interpreta como autorización cuando el estado no es confiable. No bastan borrado del cliente, memoria local, réplica obsoleta ni caché local de “no revocado”. Clave conceptual y TTL de AUTH-024/025; ACK y lecturas deben sostener la garantía dentro del modelo de fallos de AUTH-027. Se mantiene el límite para solicitudes que ya superaron el control antes de la revocación.
- **Estado:** C; garantía delimitada aprobada documentalmente por APR-03 / AD-07; sin implementación ni aceptación técnica. Mecanismo y evidencia siguen pendientes para aceptación, no para aprobación documental del plan.

### AUTH-023 — Idempotencia segura de logout

- **Descripción:** Repetir logout del mismo token auténtico ya revocado puede confirmar 204 sin volver a habilitarlo, extender indebidamente su vida ni relajar validaciones.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §5; C §8; APR-02 / AD-02, AD-07.
- **Referencia FND:** FND-009, FND-028, FND-029, FND-030.
- **Criterio de aceptación:** Token auténtico ya revocado y todavía dentro de su ventana temporal: 204, también ante repetición/concurrencia; nunca reducir TTL existente. Token expirado, falsificado o ausente: 401 `authentication_failed`. Redis no verificable: 503 `temporarily_unavailable`, nunca 204 supuesto. Solo logout permite esta confirmación con token revocado; firma/claims/vigencia siguen obligatorios y no se autoriza otro recurso.
- **Estado:** C; semántica aprobada AD-02/AD-07; coordinación concreta al plan.

### AUTH-024 — Ventana de revocación y TTL

- **Descripción:** Conservar revocación al menos hasta `exp + clock skew + margen conservador`, sin almacenar el JWT completo ni PII innecesaria.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§3, 5; APR-02 / AD-07.
- **Referencia FND:** FND-009, FND-017, FND-028, FND-041.
- **Criterio de aceptación:** `finRetención = exp + skew + margenConservador` y `TTL >= finRetención - ahoraUTC` cuando sea positivo. Margen numérico al plan, cubriendo relojes/precisión entre validadores; no redondear hacia abajo ni reducir TTL en logout repetido. Probar fronteras, reloj, precisión Redis y expiración natural, sin marcas permanentes accidentales. El margen de retención no extiende validez JWT; cambios de tolerancia no dejan ventanas aceptables sin marca.
- **Estado:** C; mínimo aprobado AD-07; margen conservador y precisión exactos al plan.

### AUTH-025 — Aislamiento y no expulsión de revocaciones

- **Descripción:** Tratar las revocaciones como estado de seguridad, aislado por ambiente/emisor/propósito y protegido contra expulsión arbitraria por presión de caché.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §5; C §§5, 8; APR-02 / AD-07.
- **Referencia FND:** FND-016, FND-017, FND-018, FND-041, FND-045.
- **Criterio de aceptación:** Clave conceptual `enver:{environment}:auth:revoked:{issuer}:{jti}` con jti autenticado; sin JWT completo ni PII innecesaria. Revocaciones vigentes no se expulsan por políticas de memoria ni caché de rendimiento; limpiezas no mezclan ambientes. Nombres/bases lógicas separados no garantizan por sí solos aislamiento de expulsión. Falta de capacidad deniega/controla fallo sin omitir escritura; acceso mínimo y sin cachés locales de “no revocado”.
- **Estado:** C; formato/garantías aprobados AD-07; aislamiento físico/política al plan e infraestructura, sin seleccionar servicio AWS.

### AUTH-026 — Fail-closed de controles compartidos

- **Descripción:** Si Redis necesario no puede comprobar revocación, denegar acceso protegido; si no puede confirmar logout o cuotas requeridas, no afirmar éxito ni continuar desprotegido.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§5, 7, 11; C §§5, 8; APR-02 / AD-02, AD-07.
- **Referencia FND:** FND-018, FND-022, FND-027, FND-030, FND-045.
- **Criterio de aceptación:** Con Redis indisponible/no verificable, token válido no obtiene acceso protegido, logout no devuelve 204 y register/login no eluden cuotas obligatorias. Responder 503 `temporarily_unavailable` sin identificar dependencia; no 401 por falsas credenciales ni fallback local que multiplique cuotas o acepte tokens sin control. Pérdida/integridad incierta activa AUTH-027.
- **Estado:** C; fail-closed/503 aprobados AD-02/AD-07; presupuestos concretos al plan/AD-09.

### AUTH-027 — Pérdida detectada de confianza, recuperación y riesgo residual

- **Descripción:** Tratar como no confiable el estado compartido de revocación ante cualquier pérdida, inconsistencia o discontinuidad detectada, o cuando su integridad no pueda verificarse según los controles definidos. Mientras persista esa condición, impedir los accesos protegidos dependientes de dicho estado. Conexión disponible, ping exitoso o Redis restaurado vacío no son prueba suficiente de integridad. Reconocer explícitamente como riesgo residual de infraestructura la pérdida catastrófica completamente silenciosa no observable mediante los controles disponibles.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§5, 11; C §8; APR-02 / AD-07; APR-03 (ajuste normativo mínimo aprobado, sin sustituir las fuentes históricas).
- **Referencia FND:** FND-018, FND-027, FND-028, FND-039, FND-047 (sin adoptar sus RTO/RPO propuestos).
- **Criterio de aceptación:** Se exigen los diez puntos siguientes, dentro del modelo de fallos documentado; la recuperación extraordinaria no es logout global ordinario. Este requisito no exige ledger MongoDB, segundo almacén duradero obligatorio ni transacciones distribuidas.
  1. Definir el modelo de fallos, señales disponibles, límites de detección y condiciones de confianza, cierre y reapertura. Una señal disponible ignorada no se clasifica como evento no observable.
  2. Pérdida detectada o integridad incierta activa no admisión compartida de accesos protegidos dependientes, con 503.
  3. No confirmar logout ni emitir tokens cuando no puedan mantenerse los controles de seguridad requeridos.
  4. Mantener no admisión hasta restaurar estado confiable validado o completar una cuarentena suficiente para que ningún token potencialmente afectado permanezca dentro de su ventana de aceptación.
  5. Cuando corresponda, coordinar detención de emisión considerando última emisión posible, clock skew, margen de retención y solicitudes en curso; no iniciar automáticamente la cuarentena desde la primera alerta.
  6. Evitar políticas conocidas que puedan expulsar revocaciones vigentes; definir capacidad, acceso, aislamiento y monitoreo posteriores de las condiciones operativas necesarias, sin exigir dos Redis físicos.
  7. Probar escenarios reproducibles de indisponibilidad, timeout, operaciones inciertas, flush controlado, eliminación detectable de marca, recuperación y cuarentena; incluir reinicio, restauración antigua y failover cuando hagan detectable o incierta la continuidad.
  8. Documentar qué componente detecta cada evento, propagación del cierre y qué demuestra cada prueba. Inyectar solo la señal de estado no confiable verifica la reacción, no demuestra por sí solo el detector.
  9. Documentar la pérdida completamente silenciosa no observable como riesgo residual de infraestructura, con tratamiento y mitigaciones posteriores; no declararlo eliminado.
  10. No presentar ping, persistencia, marcador almacenado en el mismo Redis o una prueba finita como demostración absoluta de que esa pérdida nunca puede ocurrir.
- **Estado:** C; aprobado documentalmente como requisito por APR-03 / AD-07, pero **NO IMPLEMENTADO / NO SATISFECHO**. Mecanismos y evidencias de detección, coordinación, recuperación y operación siguen pendientes para la aceptación técnica correspondiente; no se exige demostrar eliminación absoluta del riesgo no observable ni se bloquea por ello la aprobación documental del plan.

### 5.5. Abuso, HTTP y protección transversal

### AUTH-028 — Cuotas globales entre instancias

- **Descripción:** Diseñar rate limiting distribuido mediante Redis con consumo coordinado, ventanas y tolerancia explícitas, sin multiplicar la cuota al añadir instancias.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §7; C §§2.2, 5, 8; APR-02 / AD-08.
- **Referencia FND:** FND-006, FND-018, FND-022, FND-044.
- **Criterio de aceptación:** Tráfico alternado/concurrente entre al menos dos instancias respeta conjuntamente cuotas configurables y ventanas móviles de §3.5, coordinadas mediante Redis sin multiplicarlas por instancia. Reiniciar una API no reinicia contadores. Algoritmo, atomicidad y recuperación ante pérdida se demostrarán en el plan; Redis caído no habilita cuotas solo locales presentadas como globales.
- **Estado:** C; política/valores aprobados AD-08; mecanismo técnico y evidencia al plan.

### AUTH-029 — Valores iniciales y defensa contra ataques distribuidos

- **Descripción:** Configurar las cuotas académicas iniciales aprobadas de §3.5, combinando identidad/origen, con controles agregados contra credential stuffing y brute force.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§7, 11; APR-02 / AD-08 (sustituye techo previo de registro por origen).
- **Referencia FND:** FND-011, FND-013, FND-022, FND-041, FND-044.
- **Criterio de aceptación:** REGISTER: email 3/15 min y origen 20/15 min. LOGIN: origen+email 5/min, email 10/min y origen 60/min. LOGOUT: cuenta autenticada 30/min y origen 300/min. Intentos admitidos, incluidos fallidos/cuentas inexistentes, consumen cuota sin revelar existencia; probar fronteras y combinación de límites entre instancias. Exceso: 429 `rate_limited`, `Retry-After` cuando corresponda; rechazos no prolongan indefinidamente penalización. Sin bloqueo permanente, `Disabled` ni sanción. Valores modificables por configuración sin reescribir lógica.
- **Estado:** C; AD-08 aprobada con techo REGISTER por origen elevado a 20/15 min; calibración productiva no acreditada.

### AUTH-030 — Origen confiable y cardinalidad acotada

- **Descripción:** Derivar origen solo del contexto de red/proxies aprobados; minimizar exposición de identidad en claves de cuotas y limitar cardinalidad/coste de contadores y hashing.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§7, 10–11; C §§5, 8; APR-02 / AD-08.
- **Referencia FND:** FND-008, FND-011, FND-022, FND-030, FND-044.
- **Criterio de aceptación:** `Forwarded`/`X-Forwarded-For` no confiables no evaden cuotas. Probar NAT, IPv6, identidades aleatorias y saturación del hasher; combinar email+origen no elimina restricciones de NAT. Email para contadores se pseudonimiza adecuadamente con mecanismo con clave por concretar, no se almacena en claro ni mediante hash simple enumerable como supuesta anonimización. Misma política para cuentas existentes/inexistentes y cardinalidad/vida acotadas; cuotas antes de criptografía costosa cuando corresponda.
- **Estado:** C; política aprobada AD-08; mecanismo de pseudonimización, confianza de proxies y límites operativos al plan/infraestructura/AD-09.

### AUTH-031 — Errores seguros y semántica HTTP

- **Descripción:** Respetar estados de §3.4, mantener errores estables/correlacionados y no filtrar persistencia, criptografía, secretos o identidad existente.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §9; C §5.2; APR-02 / AD-02.
- **Referencia FND:** FND-025, FND-030, FND-031, FND-045.
- **Criterio de aceptación:** Respetar §3.4 y RFC 9457: `application/problem+json`, `type`, `title`, `status`, `detail`, `code`, `traceId`; `errors` solo si hay validación por campo y sin valores sensibles; inicialmente sin `instance`. Códigos 400/401/409/429/500/503 según tabla y errores sensibles `no-store`. Sin stack traces, infraestructura, MongoDB/Redis, secretos ni detalle de existencia; mantener riesgo residual 201/409. Fallos no se transforman en 200, 204 no lleva cuerpo. Desafío Bearer cuando corresponda e integración con errores de borde al plan, sin relajar el contrato API.
- **Estado:** C; contrato aprobado AD-02; configuración concreta al plan.

### AUTH-032 — Privacidad y no almacenamiento en cachés de respuestas

- **Descripción:** Minimizar datos personales y evitar almacenar respuestas de autenticación, tokens o credenciales en cachés HTTP/compartidas; restringir acceso a datos persistentes y auditoría.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§3–6, 10; C §§4, 5.2; APR-02 / AD-10 (ratificación parcial).
- **Referencia FND:** FND-003, FND-017, FND-021, FND-026, FND-032, FND-045.
- **Criterio de aceptación:** Respuestas sensibles llevan política `Cache-Control: no-store`; no se guarda JWT/password hash en Redis de rendimiento ni se comparte respuesta entre identidades. No se habilita compresión de secretos sin evaluación. Acceso/retención de PII e IP requiere política; no se afirma cumplimiento legal sin contexto jurisdiccional aprobado.
- **Estado:** C para minimización/protección ratificadas; AD-10 parcialmente aprobada, retención definitiva y tratamiento operativo diferidos, a resolver antes de despliegue productivo.

### AUTH-033 — HTTPS y confianza de transporte

- **Descripción:** Exigir HTTPS en producción y entornos publicados, definir TLS interno y confianza limitada en proxies antes de operar tras balanceador.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §11; CTX; C §5.1.
- **Referencia FND:** FND-008, FND-019, FND-020, FND-030.
- **Criterio de aceptación:** El canal publicado no sirve autenticación sensible en HTTP; certificados y terminación tienen diseño aprobado. Redirección de plantilla, EXPOSE 8081 o perfil local SSL no constituyen evidencia de TLS efectivo, ALB o dos instancias.
- **Estado:** C; diseño/implementación de infraestructura posterior, AD-11.

### AUTH-034 — Secretos externos y ambientes aislados

- **Descripción:** Mantener fuera del repositorio, appsettings versionados, imágenes, logs y ejemplos las claves JWT, connection strings MongoDB y credenciales Redis; separar ambientes.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§3, 6, 10; CTX; C §§5.1, 7; APR-02 / AD-10.
- **Referencia FND:** FND-012, FND-036, FND-041, FND-045.
- **Criterio de aceptación:** Local usa User Secrets o variables externas sin versionar; User Secrets no se presenta como bóveda productiva ni garantía de cifrado local. Producción futura usa gestor seguro de secretos AWS con mínimo privilegio, sin elegir servicio aquí. Claves por ambiente y rotación contempladas; diagnósticos/fallos no muestran valores y no hay secretos en imágenes/logs. No se accede a secretos reales para elaborar este spec.
- **Estado:** C; política ratificada parcialmente AD-10; gestor AWS concreto y operación diferidos a AD-10/AD-11.

### AUTH-035 — Autenticación distinta de autorización

- **Descripción:** Un JWT válido solo establece identidad; recursos protegidos deniegan por defecto y verifican permisos aplicables. Register/login son públicos de forma explícita, no un bypass general.
- **Prioridad:** P0.
- **Fuente:** C §§4, 5.2; SOL-02 §§11, 13.
- **Referencia FND:** FND-002, FND-009, FND-010, FND-013, FND-030.
- **Criterio de aceptación:** No se conceden privilegios de repartidor/administrador por registrarse o enviar claims/campos; logout se limita al token presentado. Nuevos recursos requieren políticas de su propia especificación. La excepción idempotente de logout no autoriza un token revocado en otra ruta.
- **Estado:** C; roles administrativos completos fuera de alcance.

### 5.6. Operación, resiliencia y requisitos no funcionales

### AUTH-036 — Logs estructurados y auditoría minimizada

- **Descripción:** Registrar eventos de registro exitoso/rechazado, login exitoso/fallido, cuota excedida, logout, rechazo por revocación y fallo de dependencia con contexto seguro y correlación.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §10; C §5.2; APR-02 / AD-10.
- **Referencia FND:** FND-012, FND-032, FND-041, FND-046.
- **Criterio de aceptación:** Eventos incluyen instante UTC, tipo, resultado/categoría segura, ambiente, versión, instancia y correlación coherente con `traceId`. Nunca registrar contraseña, password hash, JWT completo, cabecera `Authorization`, signing keys, connection strings, secretos, cuerpos sensibles completos ni valores sensibles en errores. Identidad minimizada/pseudonimizada; no volcar email/IP/jti automáticamente ni enviar al cliente motivos privados de login. Retención finita, justificada y con acceso/eliminación definidos antes de despliegue productivo, sin fijar aquí plazo CloudWatch ni afirmar cumplimiento legal.
- **Estado:** C; prohibiciones/minimización ratificadas AD-10; esquema operativo, plazo definitivo y garantías de pérdida diferidos.

### AUTH-037 — Observabilidad sin PII de alta cardinalidad

- **Descripción:** Definir señales de éxito/error/latencia por operación e instancia, fallos MongoDB/Redis, cuotas, revocación y saturación del hashing, con trazas correlacionadas y dimensiones acotadas.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §10; CTX; C §2.2.
- **Referencia FND:** FND-007, FND-020, FND-033, FND-046.
- **Criterio de aceptación:** Métricas y trazas distinguen fallo de credenciales de fallo operativo sin usar tokens, emails, subject/jti o IP como etiquetas de métricas. Correlaciones externas son no confiables y acotadas. Health básico no se usa como prueba de integridad Redis; muestreo y pérdidas se declaran.
- **Estado:** C; alertas, umbrales, cobertura y plataforma/retención CloudWatch en fase posterior, AD-09/AD-10/AD-11.

### AUTH-038 — Fallos de dependencias acotados

- **Descripción:** Definir timeouts, cancelación y reintentos limitados por operación; MongoDB indisponible no produce registros fantasma exitosos ni login basado en caché de credenciales.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§5, 11; C §8; APR-02 / AD-05, AD-07, AD-09.
- **Referencia FND:** FND-018, FND-027, FND-028, FND-031.
- **Criterio de aceptación:** Fallos MongoDB en register/login generan error operativo seguro sin inventar inexistencia; 201 requiere creación confirmada y una escritura incierta no prueba ausencia de efecto. Redis caído/no confiable aplica AUTH-026/027 y recuperación segura. Espera, concurrencia y reintentos acotados se definirán técnicamente; candidatos §3.6 no son timeouts ni SLA. No reintentar indiscriminadamente insert/emisión; logout no depende de MongoDB si puede validar/revocar con garantías aprobadas.
- **Estado:** C; garantías aprobadas AD-05/AD-07; presupuestos/reintentos al plan y calibración AD-09 diferida sin bloquear aprobación documental.

### AUTH-039 — Concurrencia y repetición sin promesas falsas

- **Descripción:** Declarar garantías por operación, incluyendo registro concurrente, login repetido, rehash y logout con respuesta perdida; no prometer exactamente una vez.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§1, 5, 11; C §8; APR-02 / AD-04, AD-05, AD-07.
- **Referencia FND:** FND-015, FND-028, FND-029, FND-035.
- **Criterio de aceptación:** Índice único evita duplicados; 201 solo tras creación confirmada. Login repetido puede emitir otro jti; `SuccessRehashNeeded` exige actualización segura sin sobrescritura concurrente y cambia `updatedAt` solo si persiste el rehash. Logout repetido conserva revocación/TTL; pérdida de confianza activa AUTH-027. Respuesta perdida no prueba ausencia de efecto; no se inventa clave de idempotencia, lock Redis o transacción MongoDB/Redis no diseñada.
- **Estado:** C; garantías aprobadas AD-04/AD-05/AD-07; atomicidad/mecanismos al plan.

### AUTH-040 — Calidad verificable sin SLA inventado

- **Descripción:** Acotar cargas, tiempos y concurrencia; mantener cero aceptación indebida y cero exposición de secretos en el conjunto de seguridad aprobado, sin afirmar seguridad universal o rendimiento no medido.
- **Prioridad:** P0.
- **Fuente:** C §§5–6, 8; SOL-02 §§4, 11–12; APR-02 / AD-09.
- **Referencia FND:** FND-042, FND-043, FND-044, FND-045, FND-046, FND-047.
- **Criterio de aceptación:** Casos negativos aprobados son rechazados y un incumplimiento bloquea aceptación técnica; exceso de payload/cuota no evade límites. Evaluar candidatos §3.6: login p95/p99 ≤1.000/2.000 ms, registro ≤1.500/3.000 ms, logout ≤250/500 ms. Son candidatos, no SLA/gates medidos; calibrar durante pruebas con perfil, volumen, concurrencia, recursos y dependencias aprobados. Nunca reducir seguridad del hashing para cumplirlos ni trasladar automáticamente 50 req/s, RTO/RPO o metas propuestas de 001.
- **Estado:** C para invariantes; P/candidatos numéricos de AD-09 diferida, sin bloquear aprobación documental ni acreditar rendimiento.

### 5.7. Verificación futura, DevSecOps y trazabilidad

### AUTH-041 — Criterios unitarios

- **Descripción:** Poder comprobar aisladamente normalización, dominio, campos/límites, política de contraseña, decisiones de estado, claims, TTL y clasificación segura de errores.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §12; C §6; APR-02 / AD-12.
- **Referencia FND:** FND-002, FND-003, FND-034, FND-048.
- **Criterio de aceptación:** Unit tests serán control futuro obligatorio: casos vinculados a AUTH/criterio, fronteras, Unicode, tiempo simulado y configuración inválida. Ejecución con evidencia de commit, ejecución, herramienta, configuración, resultado y tratamiento de hallazgos. Sustitutos no prueban índice MongoDB, persistencia Redis o TLS real. No se escriben ni ejecutan pruebas aquí ni se fija cobertura arbitraria.
- **Estado:** C para obligación futura; herramientas/datos/gate y evidencia diferidos a plan/DevSecOps, AD-12.

### AUTH-042 — Integración y pruebas distribuidas

- **Descripción:** Poder verificar HTTP, MongoDB Atlas compatible con el entorno autorizado, Redis y alternancia entre al menos dos instancias con datos sintéticos.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§5–7, 12; C §§6, 8; APR-02 / AD-12.
- **Referencia FND:** FND-006, FND-014, FND-016, FND-019, FND-022, FND-028, FND-034.
- **Criterio de aceptación:** Integration tests serán control futuro obligatorio: carrera de registro, lectura por índice, emisión/validación, revocación cruzada, cuotas §3.5, caída/recuperación y ACK de logout. Identificar versiones/topología y evidencia de commit, ejecución, herramienta, configuración, resultado y tratamiento. Declarar representatividad; pruebas locales no acreditan despliegue AWS tras balanceador ni evidencia aún no producida.
- **Estado:** C; procedimientos, entornos y ejecución futura requieren autorización, AD-12.

### AUTH-043 — Verificación de seguridad por amenazas

- **Descripción:** Cubrir las amenazas de §7 con casos positivos/negativos, riesgo residual y evidencia prevista, aplicando OWASP API Security de forma proporcionada.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§11–12; C §§5–6; APR-02 / AD-12.
- **Referencia FND:** FND-013, FND-035, FND-045.
- **Criterio de aceptación:** Security tests serán control futuro obligatorio: cada amenaza tiene control/caso, incluyendo replay antes/después de revocación, pérdida Redis detectable, reacción fail-closed, recuperación conforme AUTH-027 y evaluación documentada del riesgo residual de pérdida no observable, sin afirmar detección universal. Evidencia ligada a commit, ejecución, herramienta, configuración, resultado y tratamiento. Hallazgos bloqueantes requieren corrección o excepción formal con responsable/vencimiento según política que se aprobará en plan/DevSecOps. Sin escaneos, ataques ni pruebas activas en esta entrega o sobre objetivos no autorizados. Alcance de pérdida Redis precisado por APR-03.
- **Estado:** C; edición OWASP, herramientas y severidades pendientes en AD-12.

### AUTH-044 — SAST, dependencias y secretos

- **Descripción:** Exigir verificaciones futuras de SAST, dependencias directas/transitivas, licencias y secretos, con política de bloqueo y evidencias asociadas al cambio.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §12; C §7; APR-02 / AD-12.
- **Referencia FND:** FND-004, FND-012, FND-035, FND-036, FND-037.
- **Criterio de aceptación:** SAST, dependency scanning y secret scanning serán controles futuros obligatorios, no satisfechos por documentarlos. Evidencias con commit, ejecución, herramienta/versión, configuración, alcance, resultado, tratamiento y umbral por aprobar; dependencias directas/transitivas y licencias, hallazgos/excepciones caducables. Verificar corrección OpenApi informada con aviso/informe, sin asumir seguridad por versión o falta de mensajes en caché NuGet. Un secreto expuesto requiere tratamiento/rotación, no solo eliminarlo del archivo.
- **Estado:** C; elección de herramientas, paquetes y criterios de gate en AD-12.

### AUTH-045 — Build reproducible y revisión del contenedor

- **Descripción:** Verificar posteriormente construcción reproducible y revisión de imágenes/artefactos, manteniendo trazabilidad de dependencias e identidad del artefacto entregado.
- **Prioridad:** P1.
- **Fuente:** SOL-02 §12; C §7; APR-02 / AD-12.
- **Referencia FND:** FND-004, FND-036, FND-037, FND-039.
- **Criterio de aceptación:** Build y container scanning serán controles futuros obligatorios; construcción autorizada documenta SDK/runtime/dependencias, commit y entorno, escaneo cubre imagen final/base. Evidencias con ejecución, herramienta, configuración, resultado y tratamiento, asociadas al commit y artefacto. Comprobar ausencia de secretos y privilegios mínimos. No afirmar gate satisfecho sin ejecución; reproducibilidad operacional no es identidad binaria bit a bit sin definir/probar ese criterio.
- **Estado:** C; procedimiento, herramientas y definición de reproducibilidad pendientes en AD-12.

### AUTH-046 — Integración incremental de plataforma

- **Descripción:** Mantener como destino AWS, CI/CD GitHub Actions, IaC Terraform, HTTPS, observabilidad y al menos dos instancias tras balanceador, sin implementar esas capacidades en el spec.
- **Prioridad:** P1.
- **Fuente:** CTX; SOL-02 §§5, 10–12; APR-02 / AD-11; C §§2.2, 7–8.
- **Referencia FND:** FND-007, FND-008, FND-019, FND-020, FND-033, FND-037, FND-038, FND-039, FND-041.
- **Criterio de aceptación:** AWS, Terraform, GitHub Actions, CI/CD, dos instancias, Load Balancer, HTTPS publicado, trust de proxies, observabilidad centralizada/CloudWatch, Redis administrado y construcción/publicación/despliegue Docker quedan como requisitos futuros, no implementados ni verificados. No elegir recursos concretos aquí; Dockerfile no acredita entrega. MongoDB/Redis, pruebas y capacidades restantes se incorporan bajo incrementos autorizados, sin sustituir garantía distribuida por demostración de una instancia.
- **Estado:** C para selecciones/objetivo; servicios, despliegue y fases posteriores pendientes en AD-11.

### AUTH-047 — Trazabilidad sin cierre retroactivo

- **Descripción:** Conservar fuente → AUTH → criterio → decisión aprobada → trabajo autorizado → cambio → evidencia, distinguiendo aprobación documental, detalles técnicos diferidos y antecedentes FND/PLD/TBF de cumplimiento.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §14; C §§2–3, 11; APR-02 (aprobación y AD-12).
- **Referencia FND:** FND-001, FND-048.
- **Criterio de aceptación:** Los 48 AUTH se cubren en §9 y cada AD identifica requisitos afectados; fuentes APR-02 y estados están en §10 y memoria §13. Sin tareas ni IDs ficticios. Plan/tareas/evidencia aún ausentes no se presentan como realizados, ni se cierra una TBF por relación documental. Cada control futuro exige evidencia de commit, ejecución, herramienta, configuración, resultado y tratamiento; aprobación para planificación no satisface gates.
- **Estado:** C; spec APROBADO PARA PLANIFICACIÓN; aceptación técnica y gates pendientes AD-12.

### AUTH-048 — Exclusiones y condición de continuidad

- **Descripción:** Conservar las exclusiones de §11 como fuera de alcance actual, no funcionalidades eliminadas; detenerse tras entregar documentación.
- **Prioridad:** P0.
- **Fuente:** SOL-02 §§13–15; APR-02 (nuevo estado y límites de intervención); C §§3, 9–10.
- **Referencia FND:** FND-001, FND-049 (restricción histórica preservada).
- **Criterio de aceptación:** Esta intervención solo modifica este spec y los tres documentos autorizados: sin código funcional, paquetes, conexiones, pruebas ejecutables, recursos, workflows, plan ni tareas nuevos. El documento queda APROBADO PARA PLANIFICACIÓN: permite una entrega posterior de `plan.md`, no autoriza implementación. No iniciar aquí planificación ni trabajo adicional; tareas y ejecución técnica requieren autorización separada.
- **Estado:** C para límite de entrega; no acredita aceptación de los demás AUTH.

## 6. Modelo conceptual de persistencia y estado distribuido

### 6.1. `users` en MongoDB Atlas

Contrato conceptual aprobado por AD-05/AD-03, no esquema ejecutable ni selección de clases/driver. Contiene únicamente estos campos; BSON, driver y representación física se definirán en un plan posterior sin cambiar el contrato lógico aprobado.

| Concepto | Campo aprobado | Regla / patrón de acceso |
| --- | --- | --- |
| Identificador | `_id` persistente; `id` público UUID canónico string | UUIDv4 generado por backend y estable; referencia de `sub`, nunca email. |
| Nombre | `fullName` | Solo nombre necesario para cuenta base; validar según §3.3. |
| Correo normalizado | `normalizedEmail` | Dominio exacto UTTT, búsqueda por igualdad e índice único. No duplicar correo original sin justificación. |
| Hash de contraseña | `passwordHash` | Salida completa de `PasswordHasher<TUser>`, con formato/versionamiento del framework; nunca expuesta en contratos/telemetría. |
| Estado | `status` | `Active` o `Disabled`; inicial `Active`, cliente no lo asigna; no implica correo verificado ni estado de repartidor. |
| Timestamps | `createdAt`, `updatedAt` | UTC del servidor, iguales al crear; login normal no modifica `updatedAt`, rehash persistido sí. |

Índice único obligatorio en `normalizedEmail`, además del índice propio de `_id`. Consultar existencia antes de insertar no garantiza concurrencia. 201 solo tras creación confirmada según garantías que se definirán en el plan; inicialización segura, read/write concerns, representación UUID/BSON, topología y permisos son detalles técnicos diferidos de AD-05. Rehash seguro sin sobrescrituras concurrentes ni campos nuevos sin requisito; no duplicar versión del hasher automáticamente.

No incluir `lastLoginAt`, roles, CLABE, datos/perfil de repartidor, historial, ratings, teléfono, vehículos, ubicación, flags de verificación falsamente acreditados, tokens ni datos sin requerimiento. No hay acceso a connection strings reales.

### 6.2. Redis

- Revocación: formato conceptual aprobado `enver:{environment}:auth:revoked:{issuer}:{jti}`; jti autenticado, sin JWT completo, credenciales ni PII innecesaria. No usar cachés locales de “no revocado”; confirmación/lectura conforme AUTH-022.
- Retener como mínimo hasta `exp + clock skew + margen conservador` a definir numéricamente en plan; no reducir TTL al repetir logout ni redondear hacia abajo. Revocaciones vigentes no expulsables por memoria; la expiración natural posterior no es expulsión arbitraria.
- Cuotas: ámbitos y valores de §3.5, finalmente mediante Redis, vida/cardinalidad acotadas y email pseudonimizado adecuadamente; sin correo en claro o hash simple enumerable ni limitador local como supuesto control global.
- Caché de rendimiento: uso futuro separado de revocación/cuotas; usuarios persistidos siguen teniendo MongoDB como fuente principal. No se cachean secretos ni respuestas de login.
- No se seleccionan cluster, servicio AWS/ElastiCache, política de persistencia, mecanismo de recuperación o biblioteca desde este spec. ACK y lecturas posteriores deben sostener la garantía de AUTH-022 dentro del modelo de fallos de AUTH-027: continuidad confiable y fail-closed ante pérdida detectada o integridad incierta. No se exige ledger MongoDB, segundo almacén duradero obligatorio ni transacciones distribuidas.
- La disponibilidad de conexión no acredita integridad. Un borrado silencioso puede no ser detectable con un marcador en el mismo estado perdido; la pérdida catastrófica completamente silenciosa no observable permanece como riesgo residual de infraestructura a documentar y mitigar posteriormente, no como riesgo eliminado. Ping, persistencia, marcador colocalizado o pruebas finitas no demuestran su ausencia universal.
- Si Redis no está disponible/no verificable, 503 y fail-closed según AUTH-026; pérdida detectada o integridad incierta exige no admisión compartida y restauración confiable validada o cuarentena cubriendo máxima vida restante de tokens afectados con emisión coordinadamente detenida cuando sea necesario. Política aprobada, mecanismo técnico pendiente al plan/infraestructura; “Redis persistente” no cierra la garantía.

## 7. Amenazas, controles y riesgo residual

Los códigos AM son referencias de riesgo, no tareas ni incidentes observados.

| Riesgo | Controles AUTH | Verificación futura / limitación |
| --- | --- | --- |
| AM-01: NoSQL injection y asignación de propiedades | 005, 007–009, 014–015 | Objetos/operadores en campos no alteran filtros; estado/rol no asignable; cargas acotadas. |
| AM-02: credential stuffing y brute force | 006, 010–011, 013, 028–030, 040 | Controles de origen/combinación e identidad agregada, hashing acotado; no basta cuota por par frente a muchos orígenes. |
| AM-03: user enumeration | 004, 011, 029, 031, 036 | Login genérico y evaluación temporal; 409 de registro conserva inferencia residual explícita. |
| AM-04: JWT tampering/algoritmo/emisor/audiencia erróneos | 016–020, 035, 043 | Rechazo de firma alterada, `none`, algoritmo distinto, claims ausentes y token ajeno. |
| AM-05: replay, robo y expiración | 017–024, 032–034 | Token válido robado puede reutilizarse hasta revocación/expiración; jti no es prueba de posesión ni token de un solo uso. |
| AM-06: Redis indisponible | 022–026, 028, 038 | Denegación segura en control requerido; error operativo, sin fallback inseguro. |
| AM-07: pérdida silenciosa/evicción/failover Redis | 024–027, 039, 042–043 | AD-07 precisada por APR-03: verificar prevención de expulsión, fallos detectables, no admisión y recuperación conforme AUTH-027. Documentar límites de detección y riesgo residual no observable; aceptación técnica requiere evidencia de controles exigibles, no ausencia universal de pérdida silenciosa. |
| AM-08: MongoDB indisponible o escritura incierta | 004, 009, 014–015, 038–039 | No éxito ficticio ni login con credenciales cacheadas; unicidad limita duplicados. |
| AM-09: logs sensibles/secretos expuestos | 019, 032, 034, 036–037, 044–045 | Revisar respuestas, logs, trazas, artefactos e imágenes; mínimo acceso y rotación definidos. |
| AM-10: payload excesivo, JSON malicioso y agotamiento de hashing | 006–008, 028–030, 038, 040 | Rechazo temprano, profundidad/tamaños, concurrencia y cardinalidad acotadas. |
| AM-11: carreras de registro, rehash y logout | 009, 013, 022–024, 039, 042 | Índice único y garantías por operación, repetición segura, sin falsa transacción distribuida. |
| AM-12: proxy no confiable y transporte inseguro | 030, 033–034, 046 | Cabeceras falsas no evaden cuotas; TLS publicado e interno deben verificarse en infraestructura. |
| AM-13: dirección UTTT sin prueba de titularidad | 002, 005, 012 | Dominio no demuestra identidad institucional; no marcar email verificado por validación sintáctica, AD-03. |
| AM-14: clave HS256 comprometida/compartida en exceso | 019–020, 034, 044 | Todo poseedor puede emitir tokens; limitar custodia y definir retiro/rotación ante compromiso. |

La edición OWASP y su matriz completa por categorías se acordarán en AD-12. Esta relación no afirma cobertura total ni ausencia de vulnerabilidades. Cambiar contratos para eliminar riesgos residuales requiere aprobación del responsable; no se cambia 201/409 silenciosamente.

## 8. Escenarios de aceptación y casos límite

Son casos documentales del spec aprobado para futura verificación autorizada, no pruebas implementadas ni ejecutadas. El diseño técnico, entorno y evidencia siguen pendientes; objetivos candidatos AD-09 no se confunden con criterios de seguridad ya aprobados.

| Caso | Dado / cuando | Resultado esperado | AUTH |
| --- | --- | --- | --- |
| AC-01 | Registro válido sin teléfono | Cuenta `Active`, UUIDv4 y timestamps UTC iguales al crear; hash seguro, 201 tras creación confirmada y `id`/`fullName`/`email` normalizado. Sin token ni login automático. | 002–007, 013–014 |
| AC-02 | Email de otro dominio, subdominio o sufijo engañoso | 400, sin cuenta/token. | 005, 008, 031 |
| AC-03 | Variantes equivalentes y registros simultáneos en A/B | Máximo una cuenta, duplicados 409 genérico; índice único determinante. | 004–005, 009, 015, 039 |
| AC-04 | Contraseña de 11/12/128/129 escalares; Unicode/espacios y bytes | Registro 12–128 escalares y máximo 512 bytes UTF-8; sin transformar/truncar. Login no reaplica futuros mínimos de creación que bloqueen cuentas existentes. | 006, 008, 010, 013 |
| AC-05 | `confirmPassword`, teléfono, rol, estado, desconocidas/duplicadas, null obligatorio, objetos/arrays/operadores | 400 según contrato JSON; sin persistencia ni asignación indebida. Nombre conserva case/acentos/espacios internos, ≤200 escalares y sin controles. | 002, 007–008, 012, 035 |
| AC-06 | Login válido dos veces; hasher indica `SuccessRehashNeeded` | Dos JWT con sub UUID común y jti distinto de 32 bytes aleatorios Base64url; 200 con tokenType Bearer y expiresIn 900. Login normal no cambia updatedAt; rehash persistido lo actualiza sin sobrescritura concurrente. | 010, 013–014, 016–021, 039 |
| AC-07 | Cuenta inexistente frente a contraseña incorrecta | Mismo 401 `authentication_failed` y Problem Details; hash ficticio preparado previamente con el mismo hasher para inexistente, cuotas antes de coste cuando corresponda, sin sleeps/jitter principal ni promesa de tiempo idéntico. | 011, 029, 031 |
| AC-08 | Cuenta `Disabled` | Mismo 401 genérico, sin JWT ni estado revelado. `Active` no acredita correo verificado; sin operación pública de desactivación. | 010–012 |
| AC-09 | Firma, algoritmo, issuer, audience o claims alterados/ausentes; `kid` desconocido | 401 sin acceso ni escritura basada en claims no confiables, búsquedas arbitrarias o fallback inseguro de clave. | 016–019, 035 |
| AC-10 | Fronteras de `iat`, `exp`, skew y `nbf` opcional | NumericDate UTC, exp=iat+900 inicial, skew ≤30 s, nbf validado solo si existe. Retención cubre además margen conservador a definir, sin extender validez JWT. | 018, 020, 024 |
| AC-11 | Logout de token A en instancia 1 y uso posterior en 2 | 204 tras confirmar marca; A rechazado, token B de la misma cuenta no revocado. | 021–025 |
| AC-12 | Dos logout concurrentes o repetición tras perder respuesta | 204 con token auténtico ya revocado aún en su ventana temporal; nunca reducir TTL ni autorizar otro recurso. Estado no verificable: 503. | 022–024, 039 |
| AC-13 | Logout con token expirado, ausente o falsificado | 401 según contrato; no crear marca eterna ni confiar en jti falso. | 018, 023–024, 031 |
| AC-14 | Redis caído durante validación, cuota o logout | 503 seguro y fail-closed: sin acceso protegido, sin register/login eludiendo cuota obligatoria ni éxito ficticio de logout. | 026, 028, 038 |
| AC-15 | **Pérdida detectable de confianza y recuperación Redis.** Token auténtico revocado mediante logout confirmado; al menos dos instancias ENVER compartiendo Redis. Provocar de forma controlada y observable indisponibilidad, timeout, error de escritura, flush, eliminación de marca, restauración antigua o failover con continuidad incierta. | Operaciones inciertas no producen éxito supuesto; logout no devuelve 204 sin confirmación. Pérdida detectada o integridad incierta activa no admisión compartida y accesos protegidos dependientes reciben 503; detener emisión cuando corresponda. Ping o ausencia de marca no justifican reapertura, solo recuperación confiable validada o cuarentena suficiente. Tras recuperación se respeta la revocación; tras cuarentena los tokens afectados ya no son temporalmente válidos. Documentar componente/momento de detección, propagación, recuperación y límites; una señal inyectada prueba reacción, no el detector. La evidencia no afirma ausencia universal de pérdida silenciosa. Complementa AC-14 sin cambiar su contrato. | 022–027, 038–039, 042–043 |
| AC-16 | Cuotas distribuidas, fronteras, IP falsa, NAT/IPv6 y múltiples orígenes | REGISTER email 3/origen 20 cada 15 min; LOGIN par 5/email 10/origen 60 por min; LOGOUT cuenta 30/origen 300 por min. 429/Retry-After cuando corresponda, pseudonimización y cuota conjunta sin Disabled/sanción; NAT sigue siendo limitación residual. | 028–030 |
| AC-17 | MongoDB caído o respuesta de escritura perdida | Error seguro sin existencia falsa ni duplicados en reintento; resultado incierto explícito. | 004, 009, 038–039 |
| AC-18 | Cuerpo >16 KiB, JSON profundidad >4, contenido no soportado, JWT >4 KiB e identidades aleatorias | 413 cuerpo excesivo, 415 contenido no soportado, 400 JSON profundo; JWT excesivo no llega a trabajo criptográfico. Recursos acotados y sin scans/crecimiento ilimitado; contrato exacto de rechazo por cabecera excesiva al plan/borde. | 008, 018, 030, 040 |
| AC-19 | Revisar respuesta, logs, trazas, métricas y persistencia | Sin secretos/hash/JWT completo fuera del canal autorizado; PII minimizada y correlación segura. | 014, 017, 031–032, 034, 036–037 |
| AC-20 | Rotar clave/cambiar configuración o perder instancia | Política de coexistencia/retiro aprobada, sin romper revocaciones ni depender de afinidad local. | 019–020, 024, 027, 046 |
| AC-21 | Inspeccionar CI/artefacto en etapa autorizada | Build, unit/integration/security tests, SAST, dependency/secret/container scanning obligatorios futuros, con commit/ejecución/herramienta/configuración/resultado/tratamiento; gates no satisfechos por documentación. | 041–048 |

No se extrapola un conjunto finito de casos a seguridad universal. Los casos que exigen MongoDB/Redis, dos instancias, infraestructura o escáneres quedan pendientes hasta disponer de componentes y autorización; no se simula su cumplimiento con documentación.

## 9. Trazabilidad académica y con foundation

### 9.1. Cobertura de AUTH y antecedentes

Cada rango incluye todos sus IDs. La columna FND complementa las referencias individuales, no modifica sus criterios. Las relaciones con PLD/TBF son antecedentes reales del módulo transversal: **no significan aprobación, implementación, ejecución o cierre**. No se crean tareas de autenticación ni se reutilizan TBF como si ya las implementaran.

| AUTH | Relación FND principal | PLD de origen como antecedente | TBF de origen como antecedente | Evidencia futura prevista |
| --- | --- | --- | --- | --- |
| 001–003 | 001–005, 025, 031, 040, 048–049 | 001–004, 018, 025, 027–028 | 001–005, 022–023, 038 | Gobierno, contrato y decisión aprobados. |
| 004–009 | 011, 014–015, 024–025, 028–029, 031, 041, 044 | 011–012, 017–019 | 014–016, 021–022 | Validación, persistencia y unicidad concurrente. |
| 010–015 | 003–004, 009–015, 024, 030–031, 035–036, 041, 045 | 003, 008, 010–012, 017–018, 021–022 | 004, 006, 009–010, 012–016, 030–031, 034 | Login, hasher, estado y patrones de datos. |
| 016–021 | 006, 009–012, 027, 041, 045 | 008, 010–011, 016, 019 | 006, 010, 012–014, 020, 031 | Claims, firma, vigencia, claves y sesiones. |
| 022–027 | 006, 009, 016–018, 027–030, 039, 041, 045, 047 | 010, 013, 016–017, 019, 021, 025–026 | 012, 017–018, 020–021, 030–033 | Revocación cruzada, TTL y fallos/pérdida de estado. |
| 028–035 | 002–003, 006, 008–013, 017–022, 025–026, 030–032, 036, 041, 044–045 | 008–011, 013–014, 018–019, 022 | 006, 009–014, 017–019, 022, 025–027, 031, 034 | Cuotas globales, contratos seguros, transporte y privacidad. |
| 036–040 | 007, 012, 015, 018, 020, 027–029, 031–035, 041–048 | 008, 016–017, 020–021, 026 | 006, 020–021, 027–033 | Auditoría, observabilidad, límites y resiliencia. |
| 041–046 | 002–004, 006–008, 012–014, 016, 019–020, 022, 028, 033–039, 041, 045, 048 | 003, 011, 014, 020–024 | 004, 009–011, 019, 028–037 | Unit/integration/security, análisis, build y entrega posterior. |
| 047–048 | 001, 048–049 | 001–002, 027–028 | 001–002, 038 | Cadena de evidencia y límites de entrega. |

**Cobertura documental: 48/48 AUTH.** No es un porcentaje de implementación, pruebas o cumplimiento académico. Los IDs completos de referencia son FND-nnn, PLD-nnn y TBF-nnn en sus respectivos enlaces de §1; los números abreviados de esta tabla no crean otros identificadores.

### 9.2. Requisitos académicos comunicados

| Exigencia comunicada | Fuente | Relación AUTH / tratamiento |
| --- | --- | --- |
| Usuario / registro | SOL-02 §§1, 8 | 002–009, 012–015; cuenta base, sin datos de repartidor. |
| Login | SOL-02 §§2, 8 | 010–021; credenciales propias y JWT. |
| Logout | SOL-02 §§5, 8 | 021–027; revocación del token distribuida. |
| NoSQL e índices | C §2.2; SOL-02 §6 | 009, 014–015, 042; MongoDB Atlas e índice único, sin integración acreditada. |
| JWT | C §2.2; SOL-02 §3 | 016–024; emisión/validación, jti, expiración, revocación. |
| Redis distribuido y caché | C §2.2; SOL-02 §§5, 7; CTX | 022–030, 032; controles de seguridad compartidos. Blacklist/cuotas no demuestran por sí mismas caché de rendimiento; caché funcional posterior. |
| Rate limiting | C §2.2; SOL-02 §7 | 028–030, 040, 042; agregación y casos de abuso. |
| Dos instancias y balanceador | C §2.2; CTX | 022, 028, 033, 042, 046; requisitos y verificación futura, no despliegue declarado. |
| Logging y observabilidad | C §2.2; SOL-02 §10 | 036–037; minimización, correlación y señales. |
| Versionamiento | C §2.2; SOL-02 §8 | 003, 031; rutas v1 aprobadas, evolución futura por decidir. |
| Paginación | C §2.2; FND-023 | No aplicable a estos tres comandos sin listados. No se inventa listado de usuarios para acreditar el requisito; pendiente en otra spec aplicable. |
| Seguridad, pruebas y entrega | C §§5–7; SOL-02 §§11–12; CTX | 032–048; qué verificar, sin ejecución ni workflows/IaC. |

La rúbrica original no está disponible en el repositorio inspeccionado; no se inventan numeraciones docentes, ponderaciones o evidencias. Su contraste posterior queda en AD-12.

### 9.3. Continuidad de decisiones históricas

SOL-02 concreta para 002 aspectos de DEC-01 (alcance documental), DEC-03 (MongoDB Atlas), DEC-04 (AWS), DEC-05 (preferencia del hasher, no versiones adicionales), DEC-06 (emisión/JWT), DEC-07 (usos/fallo Redis), DEC-08 (cuotas iniciales), DEC-09 (rutas/estados), DEC-13 (GitHub Actions/Terraform) y DEC-14 (principios de secretos/configuración). No cierra esos DEC íntegramente ni altera los documentos de origen. DEC-02, DEC-10 a DEC-12 y DEC-15 a DEC-17 conservan asimismo asuntos abiertos; esta entrega no adopta Clean Architecture, servicios concretos, metas de carga o RTO/RPO.

APR-02 ratifica contratos y decisiones AD-01 a AD-08 para 002: sustituye la preferencia del hasher por selección obligatoria, las propuestas de estado por `Active`/`Disabled`, fija UUIDv4/claims/rotación y garantías Redis y eleva el techo REGISTER de origen a 20/15 min. AD-09 a AD-12 conservan el tratamiento diferido de §10. No se cambian DEC/FND/PLD/TBF de 001 ni sus estados; se conserva la apertura original v0.1.0 como antecedente en memoria §12 y la aprobación v0.2.0 como nuevo acto en §13.

La futura planificación deberá asignar sus propios identificadores de decisión y, solo con autorización posterior, tareas relacionadas con AUTH, AC, decisión y evidencia. No se generan esos artefactos ni IDs de ejecución anticipados aquí.

## 10. Decisiones aprobadas, detalles diferidos y puertas técnicas

APR-02 cierra las decisiones de especificación AD-01 a AD-08 y aprueba el documento para planificación. Los IDs AD se conservan como registro de decisiones, no plan, backlog o catálogo de ejecución. Los detalles técnicos indicados no bloquean esta aprobación documental; sí deben resolverse antes de construir o aceptar técnicamente el componente afectado. Cambiar el qué o una garantía aprobada requiere autorización para actualizar el spec.

APR-03 autoriza y aprueba expresamente la delimitación normativa de AD-07 / AUTH-022/027/043 y sus referencias, resolviendo la discrepancia con PLD-AUTH-08 sin reabrir Redis ni añadir almacenamiento. Se conserva APR-02 como antecedente; no se atribuye este nuevo alcance retroactivamente a aquella aprobación.

| ID | Estado y decisión del responsable | AUTH afectados / relacionados | Detalle diferido y puerta correspondiente |
| --- | --- | --- | --- |
| AD-01 | **APROBADA.** Contratos §3.2, normalización/unidades/límites §3.3, sin login automático; JSON estricto, 413/415 y JWT ≤4 KiB. | 003–008, 010, 014, 018 | Configurar serializador/límites y casos de frontera en plan; no reabrir las reglas funcionales aprobadas. |
| AD-02 | **APROBADA.** RFC 9457, campos/códigos/traceId/no-store §3.4; logout repetido 204 vigente, 401 inválido/expirado/ausente, 503 estado incierto; TTL no decreciente. | 008, 011, 023, 026, 031 | Desafío Bearer, detalle 413/415 e integración de errores API/borde al plan. Preservar riesgo residual 201/409. |
| AD-03 | **APROBADA.** `Active`/`Disabled`, inicial `Active`; rechazo genérico de Disabled y sin correo verificado; cuenta distinta de perfil de repartidor. | 005, 010, 012, 014; contexto 002 | No hay desactivación pública, administración, suspensión/estados de repartidor ni cierre global implícito. |
| AD-04 | **APROBADA.** `PasswordHasher<TUser>` obligatorio, framework administra formato/salt/parámetros/versionado sin iteraciones manuales; `SuccessRehashNeeded`, actualización concurrente segura y hash ficticio previo para inexistente. | 010, 011, 013, 039 | APIs/versiones/configuración efectiva y mecanismo de actualización al plan; evaluación temporal con AD-09. No Identity completo por este uso. |
| AD-05 | **APROBADA.** Siete campos §6.1, UUIDv4 público canónico, índice único, timestamps UTC y 201 solo tras creación confirmada; rehash sí modifica updatedAt, login normal no. | 009, 014, 015, 038, 039; contexto 004/013 | BSON, driver, representación física, topología/read-write concerns e inicialización al plan; garantía lógica no se difiere. |
| AD-06 | **APROBADA.** HS256/issuer/audience/15 min/skew ≤30 s; sub UUID, jti 32 bytes aleatorios Base64url, iat UTC y exp=iat+900; validaciones, kid aprobado y rotación coordinada por ambiente. | 016–021 | APIs, precisión/fronteras, periodicidad y automatización de rotación al plan; no refresh tokens ni fallback de kid. |
| AD-07 | **APROBADA; PRECISADA POR APR-03. A. Garantía exigible:** clave conceptual §6.2, TTL hasta exp+skew+margen, no decreciente, confirmación antes de 204 y sin caché local negativa. Continuidad confiable → rechazo en todas las instancias; Redis no verificable, pérdida detectada o integridad incierta → fail-closed/503, no admisión y recuperación segura o cuarentena, deteniendo emisión cuando corresponda. **B. Prevención:** noeviction, capacidad suficiente, monitoreo futuro, acceso mínimo y aislamiento apropiado; no exigir dos Redis físicos. **C. Riesgo residual:** pérdida catastrófica totalmente silenciosa no observable, a documentar y mitigar posteriormente en infraestructura, sin declararla eliminada. | 020, 022–027, 038, 039; verificación 042–043 | Margen numérico, confirmación/lecturas, señales/límites de detección, coordinación, recuperación, capacidad/monitoreo y evidencias a plan/infraestructura. Bloquean aceptación técnica afectada, no aprobación documental del plan. Ping/persistencia/marcador colocalizado/prueba finita no demuestran ausencia universal. Sin ledger MongoDB, segundo almacén duradero obligatorio o transacciones distribuidas. |
| AD-08 | **APROBADA CON AJUSTE.** Cuotas exactas de §3.5: REGISTER email 3/origen 20 cada 15 min; LOGIN par 5/email 10/origen 60 por min; LOGOUT cuenta 30/origen 300 por min; configurables/distribuidas Redis, 429 sin sanciones. | 028–030; verificación 040/042 | Algoritmo/atomicidad, pseudonimización con clave, proxies, cardinalidad y pérdida de contadores al plan; NAT residual reconocido. |
| AD-09 | **DIFERIDA, NO BLOQUEA APROBACIÓN DOCUMENTAL.** Percentiles §3.6 solo candidatos de evaluación, nunca SLA ni resultados; no reducir hashing para cumplirlos. | 038, 040; relacionados 011, 026, 030, 037, 041–043 | Perfil de carga/concurrencia, presupuestos/timeouts/reintentos, entorno y calibración en pruebas de rendimiento antes de aceptación correspondiente. |
| AD-10 | **APROBACIÓN PARCIAL / DETALLE DIFERIDO.** Prohibiciones de secretos en logs, minimización PII, local User Secrets/variables externas y futuro gestor seguro AWS con mínimo privilegio ratificados. | 032, 034, 036; relacionados 019, 030, 037 | Esquema/accesos, servicio AWS concreto y retención definitiva de logs antes del despliegue productivo, sin fijar plazo CloudWatch aquí. |
| AD-11 | **DIFERIDA A INFRAESTRUCTURA.** AWS/Terraform/GitHub Actions/CI-CD, dos instancias/Load Balancer, HTTPS/proxies, observabilidad/CloudWatch, Redis administrado y entrega Docker son requisitos futuros no implementados. | 033, 037, 046; relacionados 025, 034 | Servicios/región/red, recursos, permisos, coste, recuperación y evidencia de despliegue en fase/incremento autorizado; no seleccionar recursos aquí. |
| AD-12 | **DIFERIDA A PLAN / DEVSECOPS.** Build, unit/integration/security tests, SAST, dependency/secret/container scanning obligatorios futuros; documento no satisface gate. | 041–045, 047; relacionados 040, 046, 048 | Herramientas/configuración/entornos, severidades/excepciones, rúbrica y evidencia OpenApi. Evidencias ligadas a commit, ejecución, herramienta, configuración, resultado y tratamiento; bloquean aceptación técnica, no esta aprobación. |

La aprobación original v0.2.0 y del tratamiento de los AD queda registrada por APR-02; APR-03 aprueba esta versión v0.3.0 y la precisión de AD-07, manteniendo **APROBADO PARA PLANIFICACIÓN**. El plan ya existente queda **APROBADO PARA DESCOMPOSICIÓN EN TAREAS** tras su sincronización expresa. Los detalles diferidos deben concretarse antes de la construcción/aceptación afectada, sin inventar capacidades o evidencias. Solo se habilita crear task.md en una entrega posterior delimitada, no generarlo aquí ni implementar código o desplegar. No se cierra la aceptación técnica de AUTH-027 por aprobar su política.

## 11. Exclusiones y cierre de entrega

Fuera de alcance actual, **no eliminados del producto por este documento**:

- MFA, refresh tokens, recuperación y cambio de contraseña.
- OAuth externo, Google login, Supabase Auth y otros proveedores externos de autenticación.
- Roles administrativos completos, chat, pagos, pedidos y notificaciones push.
- Perfil de repartidor completo, CLABE, teléfono obligatorio para cuenta base y un registro independiente de repartidores.
- Verificación de correo/directorio institucional como flujo nuevo, cierre global de sesiones, administración de usuarios o endpoints de consulta no solicitados; requieren decisión/ampliación expresa si se necesitan.
- Implementación de MongoDB/Redis, infraestructura AWS/ElastiCache/ALB, TLS publicado, dos instancias, CloudWatch, Terraform, GitHub Actions y demás capacidades transversales; solo se expresan dependencias y requisitos aplicables.
- Modificar `Program.cs`, `.csproj`, appsettings, Dockerfile, solución, archivos Visual Studio, higiene Git o configuración existente; crear C#, paquetes, scripts, pruebas ejecutables, workflows, IaC, plan o catálogo de tareas.
- Modificar `constitution.md`, `.specify/memory/constitution.md` o cualquier archivo de `specs/001-backend-foundation/`.
- Invocar/modificar subagentes, cargar/ejecutar skills, compilar, ejecutar aplicación, instalar, conectar dependencias, migrar, escanear o desplegar.

**Cierre histórico APR-02 (v0.2.0):** spec actualizado a v0.2.0, **APROBADO PARA PLANIFICACIÓN**, con 48 AUTH consecutivos, 21 escenarios AC, 14 riesgos AM y 12 IDs AD conservados: AD-01 a AD-08 aprobadas (AD-08 con ajuste), AD-09/AD-11/AD-12 diferidas y AD-10 parcialmente aprobada con detalle diferido. Solo se actualizan además los tres documentos de estado autorizados. No hay plan, tareas, código funcional, paquetes ni ejecución técnica nuevos. No se declara completada foundation, satisfecha una garantía técnica ni cumplida la rúbrica. Se detiene esta intervención sin iniciar planificación.

**Cierre vigente APR-03 (v0.3.0):** se ajustan únicamente AUTH-022/027/043 y se sincronizan §4.3 paso 6, §6.2, AM-07, AC-15, AD-07 y metadatos/registro de esta resolución. Los otros 45 AUTH, incluidos AUTH-023/024/025/026/038/039, se conservan íntegros. Spec **APROBADO PARA PLANIFICACIÓN** y plan v0.2.0 **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**; 48 AUTH, 21 AC, 14 AM y 12 AD sin renumeración. Esta entrega solo actualiza los cinco documentos autorizados; las restricciones anteriores conservan su alcance histórico y no impiden esta sincronización expresa del plan ya existente. No se crea task.md ni se implementa/ejecuta nada; AUTH-027 sigue NO IMPLEMENTADO / NO SATISFECHO. Se conserva el riesgo residual y se detiene la intervención sin tareas, conexiones, instalaciones, workflows, commit o push.
