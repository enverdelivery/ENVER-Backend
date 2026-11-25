# Tareas 010 — Rescuer Enrollment & Eligibility

**v0.1.0 · Seguimiento de implementación local autorizada bajo CONDITIONAL PASS.** Fuentes: [spec v0.2.0](spec.md), [plan](plan.md). `PASS` significa evidencia local, no aprobación para integrar, activar o desplegar. Abreviaturas: **UT** `tests/Enver.API.UnitTests/`, **ST** `tests/Enver.API.SecurityTests/`, **IT** `tests/Enver.API.IntegrationTests/`.

### T-ENR-001 — Contrato y validación

- [x] **PASS local.** Objetivo: fijar DTO, versión sintáctica, validador estricto y reutilización de `RescuerStatusResponse`.
- **Dependencias:** G-ENR-01, PLD-ENR-01/02. **Archivos:** `Enver.API/Features/Users/Validation/RescuerEnrollmentInput.cs`, `Enver.API/Features/Users/Core/RescuerEnrollment.cs` y UT de contrato.
- **Aceptación:** valida tipos, longitud y sintaxis; rechaza aceptación falsa, duplicados, claves prohibidas y datos extra. No acepta teléfono, actor ni privilegios.
- **Evidencia mínima:** tabla de casos positivos/negativos y resultados UT (AC-ENR-01/03).

### T-ENR-002 — Modelo BSON e índices

- [x] **PASS local.** Objetivo: mapear elegibilidad y auditoría, con esquema cerrado e índices únicos.
- **Dependencias:** G-ENR-01, T-ENR-001, PLD-ENR-04. **Archivos:** `Enver.API/Infrastructure/MongoDB/{RescuerEligibilityDocument,RescuerEligibilityEventDocument,MongoRescuerEligibilityStore}.cs`, IT Mongo.
- **Aceptación:** UUID subtipo 4, Int64, UTC, campos/tipos cerrados e índices según spec §7. Sin migración de concesiones ni modificación de los BSON existentes.
- **Evidencia mínima:** fixture BSON, inspección de ambos índices y rechazo de estado/tipo desconocidos (AC-ENR-02).

### T-ENR-003 — Escritura transaccional, auditoría y reconciliación

- [x] **PASS local tras FIX-02.** Objetivo: acoplar estado y evento, preservar unicidad y modelar transiciones condicionadas sin publicar un canal operativo.
- **Dependencias:** T-ENR-002, PLD-ENR-05. **Archivos:** `Enver.API/Features/Users/Application/IRescuerEnrollmentStore.cs`, `Enver.API/Infrastructure/MongoDB/MongoRescuerEligibilityStore.cs`, IT Mongo.
- **Aceptación:** no hay *upsert* de alta; CAS por estado/revisión para cambios autorizados; commit mayoritario, evidencia de operación y reconciliación acotada. Pruebas de transición con actor sintético; sin cambios parciales, restauración por reenvío ni doble evento.
- **Evidencia mínima:** transacciones confirmadas/abortadas, duplicado y ACK incierto con resultados verificados; conteos y revisiones (AC-ENR-02).

### T-ENR-004 — Adaptador de lectura y comportamiento fail closed

- [x] **PASS local.** Objetivo: proyectar el contrato trivaluado de 005 sin modificar el puerto.
- **Dependencias:** T-ENR-002, PLD-ENR-06. **Archivos:** `Enver.API/Infrastructure/MongoDB/{MongoRescuerEligibility,MongoRescuerEligibilityStore}.cs`, UT/IT de lectura.
- **Aceptación:** verificado `enabled` → true; ausente/revocado → false; incertidumbre → null. Lectura exacta desde primario con concern linealizable y presupuesto probado, sin fallback operativo.
- **Evidencia mínima:** matriz de resultados y evidencia de configuración/comando de lectura en replica set local, sin secretos (AC-ENR-02/03/04).

### T-ENR-005 — Servicio de incorporación y pruebas unitarias

- [x] **PASS local.** Objetivo: aplicar política automática a cuenta propia, requisitos y consentimiento; separar alta de reenvío y modo.
- **Dependencias:** T-ENR-001/003/004, PLD-ENR-03. **Archivos:** `Enver.API/Features/Users/Application/RescuerEnrollmentService.cs`, UT del servicio.
- **Aceptación:** utiliza Auth, perfil y catálogo controlado; no escribe perfil, Auth ni modo; respeta revocación, versión, reenvío y cancelación. El reenvío exitoso no reacepta ni rehace requisitos exclusivos del alta.
- **Evidencia mínima:** resultados UT para cuenta inicial/inactiva, correo, teléfono, reglas, revocación, reenvíos e incertidumbre (AC-ENR-01/02/03).

### T-ENR-006 — Endpoint, composición y seguridad HTTP

- [x] **PASS local tras FIX-01.** Objetivo: exponer una sola ruta en pruebas, reutilizar contratos y dejar la composición operativa apagada.
- **Dependencias:** T-ENR-005; resultado de cuota controlado, sin esperar T-ENR-007; PLD-ENR-02/03/06. **Archivos:** `Enver.API/Controllers/UsersController.cs`, `Enver.API/Composition/UsersServiceCollectionExtensions.cs`, `Enver.API/Http/ApiProblemDetails.cs` solo si es necesario; ST/IT HTTP.
- **Aceptación:** `Authentication.Normal`, `sub` propio y cuenta activa; códigos, límites, `no-store`, challenge y composición de spec §6. Token revocado no muta; PATCH/PUT no escalan. Registro predeterminado `DisabledRescuerEligibility` y ruta inactiva sin escrituras; proveedor de pruebas aislado.
- **Evidencia mínima:** matriz HTTP/seguridad, inventario de rutas y prueba DI por defecto (AC-ENR-01/03/05).

### T-ENR-007 — Cuota separada y regresión de Auth

- [ ] **BLOCKED por D-ENR-04.** Objetivo: concretar admisión por cuenta con aislamiento, validar capacidad y separar política y contadores. Implementado solo el puerto; `429`/`503` probados con cuota controlada. No se cambió Auth, Redis ni configuraciones protegidas.
- **Dependencias:** T-ENR-006, aprobación del alcance transversal G-ENR-02, PLD-ENR-08. **Archivos candidatos sujetos a aprobación:** puerto en `Enver.API/Features/Users/Application/`, `Enver.API/Configuration/RateLimitingOptions.cs`, descriptores Auth y protocolo Redis en rutas de plan §1/5; ST/IT Redis de pruebas.
- **Aceptación:** política independiente; `429` y `Retry-After` cuando proceda; `503` sin fallback al fallar cuota requerida; no cambiar los contadores/comportamiento de Auth. No tocar Redis operativo ni configuración protegida sin aprobación.
- **Evidencia mínima:** aceptación de diseño/configuración, pruebas aisladas entre operaciones y réplicas del host de prueba, regresión de Auth (AC-ENR-03/05).

### T-ENR-008 — Integración Mongo y carreras

- [x] **PASS local tras FIX-03.** Objetivo: demostrar garantías críticas con Mongo real mediante Testcontainers y coordinación determinista.
- **Dependencias:** T-ENR-003/004/006, PLD-ENR-04/05/09. **Archivos:** IT de enrollment y `tests/Shared/AuthenticationDependenciesFixture.cs` solo si requiere adaptación autorizada.
- **Aceptación:** varias altas generan un estado/evento; primero alta o primero observación de ausencia por revocación, sin acreditar retirada inexistente; revocación efectiva frente a reenvío no se pierde. Incluir BSON inválido, fallos y conciliación sin éxito prematuro.
- **Evidencia mínima:** resultados de carreras por barreras, configuración de replica set, estados, revisiones y conteo de eventos (AC-ENR-02/03).

### T-ENR-009 — Delivery y regresión dirigida

- [x] **PASS local.** Objetivo: probar el permiso persistente con 007 y la continuidad de entregas, conservando contratos previos. Flujo integrado y regresión dirigida acreditados; se conserva el historial de fallos no reproducidos.
- **Dependencias:** T-ENR-006/008, PLD-ENR-07/09. **Archivos:** IT/ST para Auth, Users, Orders y Delivery; sin cambio previsto a `OrderDeliveryService` ni documentos BSON previos.
- **Aceptación:** habilitado acepta; ausente/revocado/incierto no despacha CAS; un ganador frente a otro `accept` o `cancel`; reenvío válido y denegación posterior a revocación. Entregas asignadas solo por participación y Auth. Probar orden temporal candidato con barreras; la prueba no lo ratifica.
- **Evidencia mínima:** resultados dirigidos, carreras con Mongo y regresiones del contrato 002/005/006/007 (AC-ENR-03/04/05).

### T-ENR-010 — Evidencia, coordinación y gates

- [ ] **PARTIAL.** Objetivo: consolidar trazabilidad y distinguir integración de activación, despliegue y Delivery operativo. Evidencia local registrada; faltan cuota aprobada y decisiones operativas.
- **Dependencias:** T-ENR-007/008/009 para evidencia completa y resoluciones de puertas según spec §11. **Archivos:** `specs/010-rescuer-eligibility/{spec,plan,task}.md` y registro de evidencia futuro solo con autorización.
- **Aceptación:** lista de AC y evidencias; decisiones de autoridad, reglas/publicación/Android, consistencia, cuotas y temporalidad con dueño y estado; sin compatibilidad Android acreditada sin prueba real. Mantener 007 BSON/rollback como bloqueo independiente y no conmutar el proveedor compartido mientras no haya barrera acreditada.
- **Evidencia mínima:** matriz requisito → decisión → tarea → evidencia, revisión SDD y actas para cada gate que llegue a cerrarse; ningún cierre se infiere por merge (AC-ENR-01–05).

## Cobertura prevista

| Requisitos | Tareas | Criterios |
|---|---|---|
| ENR-001/002/003/004 | 001, 005, 006, 009 | AC-ENR-01/03/05 |
| ENR-005 | 001, 002, 003, 005, 006, 010 | AC-ENR-01/02/05 |
| ENR-006/007 | 002, 003, 004, 005, 007, 008, 010 | AC-ENR-01/02/03 |
| ENR-008 | 004, 006, 009, 010 | AC-ENR-03/04/05 |

## Revisión documental — CONDITIONAL PASS local

Base `588c2565673e0bbd77dba25621cd41ce9ce35df3`, sin divergencia frente a `origin/develop` y sin cambios ajenos al inicio. La autorización de este incremento permite implementar y probar con composición aislada (G-ENR-01 solo para el alcance local identificado); no aprueba promoción, publicación de reglas ni activación.

| Severidad | Hallazgo y tratamiento |
|---|---|
| CRITICAL | G-ENR-05: lector BSON 006/007 de flota mixta y rollback no acreditados. Sin escrituras reales de Delivery; no se modifica el agregado ni se cambia la DI operativa de elegibilidad. |
| HIGH | D-ENR-04 / T-ENR-007: el catálogo, el protocolo y la configuración vigentes son cerrados para register/login/logout. Una extensión transversal no está autorizada; solo puerto y resultados de cuota controlados. |
| HIGH | D-ENR-02/03: sin autoridad ni recuperación operativa aprobadas, no se publica canal de revocación/rehabilitación. La semántica de `accept` concurrente y la topología objetivo requieren ratificación y evidencia; el fixture no las sustituye. |
| MEDIUM | D-ENR-01: `v1` es ilustrativo; no hay reglas aprobadas/publicadas ni prueba Android. La revalidación de cuenta/perfil y el permiso de Delivery no comparten transacción con el alta. |
| LOW | T-ENR-010 recopila evidencia parcial aun si T-ENR-007 está bloqueada; no cierra las puertas por recopilarla. |
| INFO | 010 añade una ruta fuera del alcance inicial de 005, sin cambiar sus rutas, DTO ni permisos. Auth conserva Bearer Normal y estado de cuenta; 009 sigue dependiendo de participación y terminalidad, no del permiso actual. |

## Evidencia por criterio y límites

| Criterio | Tareas | Evidencia local |
|---|---|---|
| AC-ENR-01 | 001, 005, 006 | `RescuerEnrollmentTests`, prueba de seguridad de ruta y flujo HTTP en `RescuerEnrollmentIntegrationTests`: validación, cuenta, requisitos, consentimiento y DTO. |
| AC-ENR-02 | 002, 003, 004, 008 | Testcontainers replica set: BSON, índices, un estado/un evento, abort sin estado parcial, cancelación del llamador tras commit confirmado y reconciliación por evento. No equivale a simular todas las pérdidas de red o a certificar la topología operativa. |
| AC-ENR-03 | 001, 005, 006, 007, 008 | Bearer, identidad, no escalamiento y fail closed; `429`/`503` con resultados de cuota de prueba. **Parcial:** no se acredita aislamiento del adaptador distribuido, que no se implementó. |
| AC-ENR-04 | 004, 008, 009 | Flujo de Delivery con adaptador Mongo: permiso antes del CAS, reenvío denegado tras revocar, continuidad y `accept` ya autorizado en la carrera candidata. No ratifica la semántica operativa ni sustituye las pruebas de regresión previas. |
| AC-ENR-05 | 006, 007, 009, 010 | Regresión backend dirigida acreditada, con historial de fallos no reproducidos. Compatibilidad Android no acreditada. |

**Ejecución final local:** Release SDK `10.0.403`, 0 errores/avisos; UT 38/38 (31 de 010 y 7 de Delivery); ST 42/42 (`TokenValidationTests`, incluida la nueva ruta); IT 19/19 (3 nuevas de 010 y 16 de `AuthenticationPipelineTests`). MongoDB `7.0.48`, replica set local de Testcontainers, driver `3.13.0`: comandos `find` de elegibilidad observados con `_id` UUID estándar, `readConcern.level = linearizable` y `maxTimeMS` positivo; cliente en primario, sin fallback a `majority`. Los commits observados exigieron mayoría y journaling.

**Tanda complementaria de regresión:** 6/10: pasaron los tres casos 010, Perfil Mongo, Ratings y el ciclo HTTP Delivery. Fallaron dos casos de Orders y las dos variantes de carrera previa de Delivery, en la creación inicial/confirmación de pedidos antes de las carreras. Los cuatro pasaron en el SHA base aislado y pasaron 4/4 al repetir únicamente esos casos en la rama con los binarios Release finales. No se cambió Orders ni se determinó causa raíz.

**Estado reportado en la primera implementación:** 8 PASS local, 1 BLOCKED y 1 PARTIAL. La revisión posterior acreditó 5 PASS, 4 PARTIAL y 1 BLOCKED: T-ENR-003 no distinguía pérdida de ACK de cancelación posterior al ACK; T-ENR-006 no respetaba la precedencia de cuerpo ausente; T-ENR-008 no fijaba el orden crítico. Dos repeticiones adicionales de la regresión dirigida pasaron 4/4 y 4/4, sin determinar la causa de los fallos históricos.

## Corrección acotada y revalidación local

- **FIX-01 / T-ENR-006:** la validación comprueba si el cuerpo está vacío antes de evaluar el tipo, solo en enrollment. Seguridad `42/42`: `400` sin cuerpo con/sin JSON, `415` con cuerpo de tipo incompatible, `400` para JSON inválido/campos extra/duplicados, `413` para tamaño excedido, Bearer y errores con `no-store`. Regresión PATCH/PUT incluida. La composición predeterminada retiene los dos proveedores deshabilitados y la solicitud válida retorna `503` sin crear colecciones.
- **FIX-02 / T-ENR-003:** `2/2` con MongoDB 7.0.48 real. El relé TCP de pruebas recibió y descartó respuestas exitosas de `commitTransaction` sin `writeConcernError`, cerrando el socket antes de que el driver recibiera el ACK. El test verificó fallos de commit del lado cliente, ningún ACK de commit recibido y una única identidad sesión/transacción para los despachos del driver. En la misma solicitud, la aplicación consultó el evento persistido y luego la concesión vigente para responder `200`; al descartar también las respuestas de lectura del evento, respondió `503`. Un cliente independiente confirmó una concesión y un evento de alta en ambos casos, con un solo despacho de cada insert. Se conserva la prueba previa de cancelación posterior al ACK, que no equivale a pérdida del ACK. No se cambió lógica de producción de Mongo, el presupuesto ni los reintentos. El primer intento falló en el observador de prueba por conservar un `RawBsonDocument` ya liberado; se corrigió capturando el UUID de sesión por valor.
- **FIX-03 / T-ENR-008:** una barrera en el puerto de escritura, respaldado por Mongo real, pausa la primera solicitud después de observar ausencia. La segunda confirma el alta en revisión 1; la prueba confirma la revocación en revisión 2 antes de liberar la primera. Esta recibe `403`; la concesión queda `revoked` en revisión 2, con la misma aceptación y exactamente dos eventos, alta y revocación. Sin *sleeps*, cambios a la política o rutas administrativas.
- **Regresión dirigida:** las seis pruebas existentes de Perfil Mongo, Orders y Delivery pasaron; los cuatro fallos históricos no reaparecieron y su causa sigue sin determinarse. Unitarias `38/38` (31 de 010 y 7 de Delivery).

**Resultado acumulado de la revalidación:** Release `10.0.403` con 0 errores/avisos; seguridad `42/42`; Mongo `6/6` casos de 010 (cuatro casos sin cambios tras la primera tanda y dos de ACK en la ejecución dirigida posterior). **8 PASS local, 1 BLOCKED (007), 1 PARTIAL (010)**. Siguiente gate recomendado: autorizar por separado commit y PR de revisión de la composición deshabilitada.

Siguen abiertos G-ENR-02/03/04/05. Conservar **CRITICAL BSON 006/007 y rollback**; confianza y continuidad del historial tras pérdida o restauración; cuota distribuida `enrollment/account` con dominio HMAC y estado propios; autoridad/canal de revocación y rehabilitación; ratificación temporal de `accept`; reglas reales/Android, validación de la topología objetivo y telemetría agregada sin PII. Los binarios y las pruebas locales no constituyen integración, activación, despliegue ni disponibilidad operativa. Histórico de initialize: **1**; no se ejecutó en esta entrega.
