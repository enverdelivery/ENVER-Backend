# Spec 010 — Rescuer Enrollment & Eligibility

**v0.2.0 · Borrador para revisión · CONDITIONAL PASS para preparación documental.** Política funcional A seleccionada. Este estado no autoriza implementar, integrar, activar ni desplegar. Rigen la [constitución canónica](../../.specify/memory/constitution.md) y los contratos de [002 Authentication](../002-authentication/spec.md), [005 Users/Profile](../005-users-profile/spec.md), [006 Orders](../006-orders/spec.md) y [007 Delivery](../007-delivery/spec.md). Véanse [plan](plan.md) y [tareas](task.md).

## 1. Objetivo y alcance

Permitir que una cuenta activa se incorpore voluntariamente como Rescatista, sin aprobación individual. La política de servidor valida requisitos y concede un permiso persistente, auditable y revocable. El permiso no es un rol JWT ni el modo de interfaz. La alternativa C (concesión administrada como vía principal de alta) queda descartada.

010 define una sola ruta nueva, dos colecciones y una fuente para la interfaz existente de elegibilidad. Define las transiciones de revocación y rehabilitación, pero no designa quién las ejecuta ni por qué canal. Extiende el alcance de 005 con una operación independiente; no convierte las rutas de perfil en altas ni modifica 005.

## 2. Historias de usuario

- Como Solicitante, quiero incorporarme con mi cuenta y teléfono existentes y aceptar las reglas sin esperar una decisión individual.
- Como usuario habilitado, quiero ver el permiso en Perfil y elegir por separado si cambio al modo Rescatista.
- Como rescatista revocado, quiero seguir usando la app como Solicitante y terminar las entregas ya asignadas si conservo cuenta activa y autorización de participación.
- Como responsable de la política, quiero poder acreditar qué autorizó cada cambio sin exponer datos de auditoría al cliente.

## 3. Requisitos funcionales

| ID | Requisito | Fuente o estado |
|---|---|---|
| ENR-001 | Toda cuenta nueva comienza como `Solicitante` y sin habilitación. Solo una incorporación expresa, independiente de `activeRole`, puede iniciar el alta. | Política seleccionada; proyección 005. |
| ENR-002 | Identificar exclusivamente por `sub` validado y comprobar existencia y estado activo de la cuenta. | `Authentication.Normal` y consulta de cuenta de 002. |
| ENR-003 | Validar el correo institucional persistido con la política vigente de Auth, incluido su dominio exacto. | Reusar normalizador de 002. No acredita posesión del buzón, matrícula ni identidad estudiantil. |
| ENR-004 | Comprobar teléfono registrado y válido en el perfil. | Reusar la validación E.164 de 005. No acredita titularidad ni verificación por SMS. |
| ENR-005 | Exigir aceptación expresa y versión aplicable, con evidencia persistente. | Requisito seleccionado; texto, identificador y publicación pendientes. |
| ENR-006 | Denegar si existe revocación vigente y no conceder si no se puede comprobar la fuente. Solo la rehabilitación autorizada puede salir de `revoked`. | Política seleccionada; autoridad y canal pendientes. |
| ENR-007 | Habilitar automáticamente por política de servidor; preservar unicidad, auditoría, idempotencia y fail closed. | Objetivos seleccionados; diseño Mongo y cuotas en revisión. |
| ENR-008 | Reutilizar `IRescuerEligibility` y las rutas 005/007, sin ampliar autorización por `activeRole`; conservar continuidad por participación. | Política seleccionada; semántica temporal de `accept` pendiente de ratificación. |

**Semántica V1 propuesta:** correo y teléfono se comprueban al evaluar la primera incorporación; no sustituyen el permiso ni se revalidan en cada `accept`. Cambiar o borrar el teléfono no revoca automáticamente. Una nueva versión de reglas aplica a altas nuevas, no invalida concesiones existentes ni implica reaceptación. No se promete atomicidad de las lecturas de cuenta y perfil con su modificación concurrente; todo requisito más fuerte necesita nueva aprobación. Auth sigue comprobando cuenta activa por operación protegida.

## 4. Reglas iniciales y publicación

**Texto propuesto, no publicado ni aprobado operativamente:**

1. Aceptar únicamente pedidos que el Rescatista pueda realizar.
2. Cumplir las instrucciones del Solicitante.
3. Tratar respetuosamente a los usuarios.
4. Utilizar los datos personales únicamente para realizar la entrega.
5. Informar oportunamente sobre inconvenientes durante el pedido.
6. No realizar actividades fraudulentas ni utilizar indebidamente ENVER.

Se propone un identificador opaco e inmutable por texto aprobado: cualquier cambio de contenido exige un identificador nuevo. El catálogo de publicaciones debe vincular texto exacto, identificador y revisión de política. Antes de activar, aprobar un canal de publicación y coordinar la distribución del mismo texto/ID en Android y la versión vigente en todas las réplicas. Una app antigua no debe enviar aceptación de un texto que no mostró; ante conflicto debe informar y obtener el texto por el canal aprobado antes de pedir una nueva aceptación. No habrá endpoint de descarga. `v1` es solo un ejemplo.

## 5. Estados y reglas de negocio

| Estado anterior | Operación | Resultado |
|---|---|---|
| Ausencia confirmada | Incorporación válida | Inserción `enabled` con auditoría. |
| `enabled` | Reenvío válido con la versión registrada | `enabled`, sin escritura ni nuevo consentimiento. |
| `enabled` | Revocación autorizada | `revoked`, con auditoría; se puede revocar con cuenta desactivada. |
| `revoked` | Incorporación por autoservicio | Denegación, sin escritura. |
| `revoked` | Rehabilitación explícita autorizada | `enabled`, con cuenta activa y nueva decisión auditada; no inventa una aceptación. |

No hay estados `Pending` o `Rejected`, suspensiones temporizadas ni rehabilitación automática. La revocación no caduca ni borra el registro, no cambia modo o token y no reasigna pedidos. No se propone bloquear preventivamente una cuenta aún no incorporada: la revocación ante ausencia no puede darse por efectiva.

## 6. Contrato HTTP

**Única ruta nueva:** `POST /api/v1/users/me/rescuer-enrollment`. Política `Authentication.Normal`, cuenta activa y UUID extraído exclusivamente de `sub` validado. Sin parámetros de negocio en query.

Solicitud `application/json` UTF-8:

```json
{
  "acceptedRules": true,
  "rulesVersion": "v1"
}
```

Cuerpo objeto único; `acceptedRules` obligatorio, booleano exactamente `true`; `rulesVersion` obligatorio, string de 1–64 caracteres ASCII, primer carácter alfanumérico, resto alfanuméricos o `.`, `_`, `-`; sensible a mayúsculas y sin normalizar. Rechazar JSON inválido, claves desconocidas o duplicadas, coerciones y `null`; en particular `userId`, `rescuerId`, `rescuerEnabled`, `status`, `decisionBy`, `permissions` y `phone`. Límite propuesto de 4 KiB, alineado con 005. El teléfono se registra previamente mediante `PATCH /api/v1/users/me`.

Respuesta `200 OK`, primera alta confirmada o reenvío válido, con el `RescuerStatusResponse` de 005; sin `Location`:

```json
{
  "rescuerEnabled": true,
  "rescuerLevel": null
}
```

| HTTP | `code` | Condición |
|---|---|---|
| 400 | `validation_failed` | Query no permitida, entrada ausente o mal formada, aceptación distinta de `true` o campos prohibidos. |
| 401 | `authentication_failed` | Token ausente, inválido o revocado; challenge Bearer de 002. |
| 403 | `forbidden` | Cuenta ausente/inactiva, correo legible que incumple la política al evaluar el alta o revocación confirmada. |
| 409 | `rescuer_phone_required` | Primera alta sin teléfono o con valor legible que no cumple 005. |
| 409 | `rescuer_rules_version_conflict` | Primera alta con versión distinta de la vigente; o `enabled` con versión solicitada distinta de la registrada. |
| 413 / 415 | `payload_too_large` / `unsupported_media_type` | Límite excedido o representación no soportada. |
| 429 | `rate_limited` | Cuota excedida; `Retry-After` cuando sea calculable. |
| 503 | `temporarily_unavailable` | Control requerido no verificable, BSON ilegible, resultado no confirmado o ruta registrada sin activación autorizada. |

Errores con `application/problem+json`, `status`, `code` estable y `traceId`; no exponer autoridad ni motivo interno. Respuestas `Cache-Control: no-store`. Para una petición bien formada y cuenta activa, `revoked` confirmado impide el éxito incluso si coincide la versión histórica. Si se confirma `enabled`, el reenvío con la versión registrada no vuelve a comprobar teléfono, correo ni vigencia de esa versión; sí se aplican Auth y la cuota de la ruta.

| Ruta existente | Contrato conservado |
|---|---|
| `GET /api/v1/users/me` | Perfil propio intacto, incluido modo y proyección de elegibilidad. |
| `GET /api/v1/users/me/rescuer-status` | Estado propio; `false` si ausencia o revocación confirmadas, `503` ante incertidumbre del proveedor operativo. |
| `PATCH /api/v1/users/me` | Solo campos aprobados por 005; permite registrar teléfono, no habilitar. |
| `PUT /api/v1/users/me/active-role` | Selección independiente de modo, sin conceder permisos. Enrollment tampoco cambia el modo. |
| `POST /api/v1/orders/{id}/accept` | Bodyless, política Normal, elegibilidad por solicitud y CAS de 007. |

Con proveedor operativo incierto, las rutas de perfil que lo consultan conservan `503`, también antes de las escrituras de PATCH/PUT; no se inventa `false`.

## 7. Persistencia y auditoría — propuesta técnica

`rescuerEligibility`: `_id` UUID BSON estándar (subtipo 4), igual al de Auth e índice único; `status` string cerrado `enabled`/`revoked`; `revision` Int64 positiva (empieza en 1); `updatedAt` fecha BSON UTC; `acceptedRulesVersion` string y `rulesAcceptedAt` fecha BSON UTC, ambos inmutables para la aceptación registrada en V1.

`rescuerEligibilityEvents`: `_id` UUID estándar único de operación; `userId` UUID objetivo y `revision` Int64 con índice único compuesto; transición anterior/nueva, resultado de cambio confirmado y fecha BSON UTC; iniciador, autoridad, versión de política y, para el alta, versión de reglas y códigos cerrados de requisitos comprobados. El UUID autenticado identifica al iniciador; la política versionada de servidor es la autoridad de concesión. Para revocación/rehabilitación: identidad opaca del actor autorizado y referencia de decisión o código de motivo aprobado. No aceptar operador declarado por el cliente ni texto libre. No inventar reaceptación. Denegaciones y reenvíos no crean eventos de transición; distinguirlos en telemetría agregada sin valores personales.

Mapeos BSON cerrados y validación de tipos/invariantes. No duplicar nombre, correo, teléfono ni datos bancarios. No modificar `UserDocument`, `UserProfileDocument` ni `OrderDocument`; no precargar concesiones ni eliminar o expirar registros de revocación. Proponer transacción Mongo para estado y evento con commit reconocido por mayoría; verificar topología y permisos antes de aceptar la garantía. Auditoría de solo anexado para el escritor y acceso/retención sujetos a aprobación.

## 8. Seguridad, concurrencia e integración

- **Prevención y presupuesto:** deny by default, validación estricta, cuenta activa, cuota distribuida independiente por `sub` validado y pseudonimizado, presupuesto acotado desde admisión hasta reconciliación; sin secretos, JWT ni PII innecesaria en logs. Auth mantiene registro, login, logout y revocación sin cambios de contrato. No reutilizar contadores de Auth; cuota necesaria no verificable → `503`.
- **Alta e idempotencia:** insertar solo ante ausencia, sin *upsert* que sobrescriba. Solicitudes simultáneas producen un estado y un evento. El reenvío se identifica por cuenta y versión aceptada; no exige `Idempotency-Key`. Si pierde una carrera, reconciliar contra el estado vigente verificado: `200` solo para `enabled` y misma versión, `403` para `revoked`, `409` por versión distinta, `503` si no se acredita el resultado.
- **Cambios autorizados:** CAS por UUID, estado y revisión esperados; incrementar revisión solo en transición efectiva. Duplicado con mismo ID y decisión confirmada no repite escritura; mismo ID con otra decisión se rechaza. Si una revocación observa ausencia concurrente con la primera alta, no puede confirmar la retirada y requiere resolución contra el estado confirmado. Tras una revocación confirmada, ningún reenvío de enrollment restaura el permiso. Ante commit incierto, reconciliar estado y evidencia de operación dentro del presupuesto; si no se puede, `503` sin afirmar que no hubo escritura.
- **Fuente de servidor:** `IRescuerEligibility.ReadAsync(Guid, RequestBudget)` conserva `ValueTask<RescuerEligibility?>`: `enabled` → `(true, null)`; ausencia o revocación confirmada → `(false, null)`; fallo/incertidumbre → `null`. Se propone lectura por `_id` en primario con `ReadConcern.Linearizable`, acotada y validada en la topología objetivo; `Majority` no equivale a esa garantía. Sin caché positiva, fallback más débil ni mocks operativos. Ante pérdida detectada o restauración no validada, bloquear hasta reconciliar el registro; la pérdida silenciosa no observable es riesgo residual.
- **Delivery:** la lectura de permiso antecede al CAS de `orders` y se repite en cada `accept`, incluidos reenvíos. Una lectura iniciada después de una revocación confirmada por mayoría deniega sin despachar el CAS (salvo posterior rehabilitación autorizada); si no se puede verificar, `503`. Un `accept` cuyo permiso se leyó antes, y que se solapa con la revocación, puede completar después. **Esta semántica requiere ratificación**; no hay atomicidad entre documentos ni la logra una segunda lectura. Preservar no autoaceptación, un ganador en `accept/accept` y `accept/cancel` y marcas 007.
- **Continuidad seleccionada:** revocado puede actuar como Solicitante, consultar entregas asignadas y progresarlas conforme a la participación de 007; solo el dueño confirma el cierre. No toma nuevos pedidos ni obtiene `200` repitiendo `accept` después de revocar. Si la cuenta está inactiva o el token revocado, prevalece Auth.

## 9. Integración con Android

El snapshot inspeccionado no muestra un flujo completo; no acredita compatibilidad. Flujo futuro: autenticarse → «Convertirse en Rescatista» → comprobar o registrar teléfono mediante 005 → mostrar el texto versionado y recabar aceptación → enviar el DTO de §6 → refrescar Perfil → elegir voluntariamente modo Rescatista → ofrecer Aceptar si está habilitado. Distinguir los dos `409`, `403`, `429` y `503`. El booleano orienta la interfaz, pero no autoriza al cliente. No modificar Android en esta entrega.

## 10. Criterios de aceptación

| ID | Evidencia futura exigida |
|---|---|
| AC-ENR-01 | Contrato HTTP y reglas: cuenta nueva sin permiso; alta válida; cuenta inactiva; correo no conforme; teléfono ausente/inválido; aceptación falsa o versión inválida; campos desconocidos, duplicados o prohibidos. Las solicitudes rechazadas por validación o autorización no despachan escrituras. |
| AC-ENR-02 | Mongo real mediante Testcontainers: BSON cerrado, unicidad, repetición y simultaneidad (un estado/evento), atomicidad y reconciliación de confirmación incierta; reenvío revocado y carrera con revocación sin rehabilitación. |
| AC-ENR-03 | Seguridad: token revocado, `sub` propio, autoridad estrictamente de servidor, PATCH/PUT sin escalamiento, `503` si fuente/cuota no verificable, `429` con aislamiento de los contadores existentes. |
| AC-ENR-04 | Delivery: habilitado acepta; no habilitado o revocado no toma pedidos; permisos o resultados inciertos no se tratan como éxito; carreras 007 y carrera con revocación coordinada según política ratificada; se conserva continuidad. |
| AC-ENR-05 | Regresión dirigida de Auth, Users, Orders y Delivery. Verificar Android real antes de declarar compatibilidad; no confundir evidencia móvil con aceptación local del backend. |

## 11. Dependencias, decisiones y gates

| ID | Puerta pendiente |
|---|---|
| G-ENR-01 · Implementación local | Aprobar spec/plan/task y autorizar cada incremento. Contratos y pruebas controladas pueden diseñarse sin autoridad real ni texto operativo, usando datos y versiones sintéticos. |
| G-ENR-02 · Integración | Revisar evidencias de persistencia, seguridad, regresión y composición apagada. Toda extensión transversal de cuotas requiere aprobación y regresión específicas antes de integrar esa parte. |
| G-ENR-03 · Activación | Aprobar reglas/publicación, autoridad y canal de revocación/rehabilitación, retención y recuperación, topología/presupuestos, configuración protegida de cuotas y ratificación temporal de `accept`. Evidenciar comportamiento entre réplicas. Por defecto `DisabledRescuerEligibility`; pruebas controladas no habilitan la ruta operativa. |
| G-ENR-04 · Despliegue | Autorización separada de integración y activación; fuera de esta entrega. |
| G-ENR-05 · Delivery operativo | **CRITICAL: compatibilidad BSON 006/007 y rollback**, independiente de 010. No habilitar escrituras reales de Delivery. Si no se acredita una barrera independiente para esas escrituras, mantener desactivados tanto el proveedor compartido como enrollment operativo. |

002, 005, 006 y 007 son dependencias; Media y Ratings no otorgan elegibilidad. La integración de #21 no acredita que Delivery esté operativo. El plan detalla decisiones aún abiertas.

## 12. Fuera de alcance y Definition of Done

Sin panel ni API administrativa, aprobaciones manuales de alta, sanciones completas, chat, notificaciones, GPS, pagos, banca, SMS, identidad externa, roles JWT, refactorizaciones generales, AWS, Terraform, SSM, GitHub Actions, despliegues o initialize.

**Especificación:** alcance, contrato, criterios y pendientes revisados. **Implementación futura:** criterios aplicables acreditados por evidencia trazable y contratos existentes intactos. **Operación:** no se presume por completar código o pruebas; requiere cierre separado de gates, incluidas las condiciones de activación y BSON de 007.
