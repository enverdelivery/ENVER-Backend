# Plan 010 — Rescuer Enrollment & Eligibility

**v0.1.0 · Borrador para revisión · No autoriza ejecución.** Fuentes: [spec v0.2.0](spec.md), [tareas](task.md), [constitución](../../.specify/memory/constitution.md) y [memoria](../../memoria.md). Base documental `588c2565673e0bbd77dba25621cd41ce9ce35df3` (`develop`, incluye 007 #21 y 009 #22).

## 1. Arquitectura y archivos previstos

**PLD-ENR-01.** Extender el módulo Users del monolito ASP.NET Core .NET 10: transporte → aplicación → puertos mínimos → Mongo. Sin proyectos, paquetes o servicios externos nuevos propuestos. Los nombres siguientes son orientativos; solo los tres Markdown pertenecen a esta entrega.

| Área | Ruta prevista y propósito |
|---|---|
| Contrato/validación | `Enver.API/Features/Users/Validation/RescuerEnrollmentInput.cs` y `Features/Users/Core/RescuerEnrollment.cs` bajo `Enver.API`: DTO de entrada y valores internos. Reutilizar `RescuerStatusResponse`, `ProfileInput`/su regla de teléfono y `InstitutionalEmailNormalizer`, sin duplicar reglas. |
| Aplicación | `Enver.API/Features/Users/Application/{RescuerEnrollmentService,IRescuerEnrollmentStore}.cs`: orquestación y puerto transaccional; `IUserStore`, `IUserProfileStore`, `TimeProvider` y `RequestBudget` existentes. |
| Mongo y adaptador | `Enver.API/Infrastructure/MongoDB/{RescuerEligibilityDocument,RescuerEligibilityEventDocument,MongoRescuerEligibilityStore,MongoRescuerEligibility}.cs`: contratos cerrados, escritor/consulta y adaptador del puerto existente. Reusar `IMongoDatabase` y cliente existentes con política de lectura acotada a elegibilidad. |
| HTTP/DI | `Enver.API/Controllers/UsersController.cs`, `Enver.API/Composition/UsersServiceCollectionExtensions.cs` y helper de `Enver.API/Http/ApiProblemDetails.cs` solo si requiere extensión compatible: acción, composición y errores. No modificar los mappings de errores existentes. |
| Cuota | Puerto de admisión local a Users en `Enver.API/Features/Users/Application/`; candidatas a evaluación (no autorizadas): `Enver.API/Configuration/RateLimitingOptions.cs`, `Enver.API/Features/Authentication/Application/Quotas/`, `Enver.API/Infrastructure/Redis/` y validación de descriptores/configuración protegida. |
| Pruebas | `tests/Enver.API.UnitTests/`, `tests/Enver.API.SecurityTests/`, `tests/Enver.API.IntegrationTests/`; reutilizar convenciones existentes de fixtures, Testcontainers y host de pruebas; revisar `tests/Shared/AuthenticationDependenciesFixture.cs` para Mongo replica set. |

## 2. Contrato, flujo y errores

**PLD-ENR-02.** Usar lectura estricta y acotada de JSON UTF-8 siguiendo UsersController. Distinguir ausencia, `false`, duplicados y variantes de tipos/nombres; no aceptar objeto parcial ni versión normalizada. No aceptar teléfono en el DTO. Aplicar exclusivamente a 010 los códigos de negocio nuevos; preservar challenge, `no-store` y catálogo compartido (spec §6).

**PLD-ENR-03.** Autenticar y derivar UUID de `sub`; comprobar admisión de la funcionalidad y transporte antes del trabajo costoso. Crear un `RequestBudget` compartido, con cancelación cliente y límites por dependencia; usar presupuesto de prueba separado del dimensionamiento operativo pendiente. Consumir cuota dentro de ese presupuesto, por identidad autenticada y antes de las lecturas de negocio, incluso para reenvíos válidos. El servicio:

1. Consulta cuenta y exige que esté activa; no toma el correo de claims ni del cliente.
2. Consulta elegibilidad autoritativa: `revoked` deniega; `enabled` compara aceptación registrada y no escribe; dato incierto devuelve indisponibilidad.
3. Para ausencia confirmada, comprueba versión publicada configurada en servidor, correo persistido conforme a Auth y teléfono persistido conforme a 005. Distingue ausencia verificable de fallo de lectura. Lee solo perfil necesario; no llama al flujo HTTP de PATCH ni a la proyección de perfil que vuelve a consultar elegibilidad.
4. Fija UUID interno de operación, instante de servidor y evidencia de política/consentimiento. Persiste alta y evento de forma atómica o reconcilia dentro del mismo presupuesto. No escribe modo, perfil ni Auth.

Validar composición con versiones y autoridades **sintéticas únicamente en el host de pruebas**; no agregar reglas reales por defecto. Si la ruta está registrada pero deshabilitada, no despacha escritura y responde `503` a la solicitud autenticada. No crear una ruta de publicación de reglas.

## 3. Persistencia y concurrencia

**PLD-ENR-04.** Dos colecciones nuevas y estrictas, índices de spec §7. Preparar índices y validación antes de admitir escrituras; no precargar concesiones, usar TTL o reconstruirlas desde perfil, ni alterar documentos existentes. Probar con replica set Mongo real; acreditar por separado la topología efectiva, permisos para transacciones, confirmación mayoritaria y lectura linealizable. No introducir otro almacén ni transacción distribuida.

**PLD-ENR-05.** Transacción de escritura local Mongo: insertar únicamente ante ausencia (unicidad de `_id`) o CAS por estado/revisión para el modelo de transiciones autorizadas, más evento de solo anexado en la misma sesión. Commit con reconocimiento mayoritario. La lectura linealizable no se hace dentro de la transacción; no reemplazar el filtro de escritura con una lectura transaccional. Acotar cualquier reintento permitido; conservar ID interno del intento al resolver un commit. Para un resultado incierto, exigir evidencia de evento/commit y estado actual confirmado cuando corresponda; para duplicado de alta, verificar ganador y aceptación sin atribuirse su escritura. Una ausencia tras timeout no prueba falta de efecto remoto. No comunicar éxito con un evento histórico si el estado de alta está ahora revocado. Nunca repetir una transición con una identidad nueva a ciegas.

Probar ambos órdenes de la carrera: revocación observa ausencia y no confirma retirada; alta confirma y revocación posterior condicionada puede retirarla. Si la revocación efectiva gana frente a un reenvío u otra resolución de alta, el autoservicio no puede volver a escribir `enabled`. El CAS de revocación/rehabilitación puede probarse con una autoridad sintética; no se planifica CLI, API, acceso directo operativo ni permisos administrativos improvisados.

## 4. Lectura, DI y Delivery

**PLD-ENR-06.** `MongoRescuerEligibility` implementa la interfaz de 005 sin cambiar su firma ni inducir certeza artificial. Usar consulta exacta por `_id` en primario con `ReadConcern.Linearizable`, límite servidor y presupuesto cliente. Diferenciar documento ausente de fallo/integridad incierta; no usar `Majority` como fallback, ni caché positiva. Si no se acredita alcance del concern, soporte de topología o recuperación del registro tras pérdida/restauración, no activar. Mantener `DisabledRescuerEligibility` como proveedor operativo predeterminado; el escritor de enrollment se mantiene apagado junto a ese proveedor, no se permite un `200` que prometa una habilitación que los consumidores todavía no usan.

**PLD-ENR-07.** Sin cambios previstos a Order ni a `OrderDeliveryService`: ya consume el puerto antes del CAS y cada nueva solicitud vuelve a consultarlo. Verificar orden temporal con barreras de prueba, sin sleeps: revocación confirmada antes de leer → denegar sin despachar CAS; lectura autorizada previa con operaciones solapadas → el CAS puede completar, sujeto a ratificación; fuente incierta → no despachar. Continuidad por cuenta activa y participación, no por grant. Si se exige barrera al commit, replantear en otro diseño; no encubrir la ventana con una relectura. **CRITICAL BSON 006/007 y rollback**: mientras no se resuelva, no abrir escrituras reales; sin barrera independiente acreditada, no conmutar el proveedor compartido ni activar enrollment.

## 5. Rate limiting — propuesta mínima

**PLD-ENR-08.** Proponer una operación de cuota de enrollment separada y un único ámbito por cuenta (`sub` ya validado, pseudonimizado del lado servidor y aislado por operación/entorno). No reutilizar buckets ni contadores de register/login/logout. Sin decisión sobre ventana, límite, volumen de particiones, origen adicional ni parámetros protegidos. Cuota distribuida no disponible o configuración no acreditada → `503`; exceso confirmado → `429` y `Retry-After` cuando pueda calcularse. En local usar puerto y resultados controlados para verificar contrato, no como implementación operativa.

**Hallazgo:** `AuthenticationOperation` (`Http/RequestBudget.cs`) admite tres operaciones; `AuthenticationQuotaPolicyDescriptor`, `RateLimitingOptions`, constructor de claves y protocolo/script Redis tienen contratos cerrados, revisión y límites de capacidad. No basta con añadir un atributo a la acción. Evaluar reutilizar el motor distribuido y su admisión/semántica de recuperación con una identidad de política/partición nueva, sin alterar semántica de 002. La extensión puede afectar descriptores, validadores, serialización, protocolo Lua, identidad/generación, coexistencia de réplicas, capacidad y configuración protegida; inventariar esos puntos y someterlos a aprobación antes de cambiar contratos transversales. Si no se acredita aislamiento y compatibilidad, el adaptador operativo permanece bloqueado; no elegir un fallback local ni cambiar Redis operativo. Cierre futuro: evidencia con Redis de pruebas, aislamiento, indisponibilidad, rechazo y regresión de Auth; sin initialize.

## 6. Pruebas y secuencia

**PLD-ENR-09.** Primero contrato y dominio → persistencia/auditoría y lectura → servicio → endpoint/DI controlados → cuotas y regresiones previa aprobación de su alcance. Usar unitarias para decisiones y validación, pruebas HTTP/seguridad para límites y escalamiento y Mongo real para índices, BSON, atomicidad, reconciliación y carreras. Coordinar carreras por barreras y preservar evidencia de fixtures y topología. La prueba local no acredita capacidad operativa, recuperación de datos ni compatibilidad móvil; coordinar texto/ID, errores y flujo con Android sin editarlo.

## 7. Decisiones y puertas

| ID | Decisión pendiente | Bloquea |
|---|---|---|
| D-ENR-01 | Ratificar contrato técnico y punto de comprobación de requisitos; aprobar texto/ID y canal de publicación coordinado. | G-ENR-01 para contrato; G-ENR-03 para uso de texto real. |
| D-ENR-02 | Designar autoridad, permisos y canal de revocación/rehabilitación, motivos, retención y política de recuperación/integridad. | Canal real y G-ENR-03; no bloquea modelo/pruebas sintéticas autorizadas. |
| D-ENR-03 | Ratificar lectura frente a `accept` en curso; acreditar consistencia, transacciones, límites y configuración homogénea. | G-ENR-03. Si cambia la semántica, revisar diseño y pruebas antes de integración del comportamiento afectado. |
| D-ENR-04 | Aprobar alcance de adaptación de cuotas, identidad, capacidad, ventana, límites y configuración protegida. | Cambios transversales de G-ENR-02 y G-ENR-03 operativo; no bloquea contrato con doble de prueba. |
| D-ENR-05 | Aprobar por separado promoción/despliegue y acreditar BSON 006/007, convivencia y rollback. | G-ENR-04 y G-ENR-05; no hay `accept` operativo por integrar 010. |

Ninguna puerta está cerrada. Antes de iniciar trabajo local, obtener aprobación SDD y autorización de tarea con archivos, comandos y entorno. Después de ello pueden empezar validación, servicio, almacenamiento y pruebas controladas sin autoridad real, publicación ni configuración operativa. No iniciar mecanismos operativos por inferencia.
