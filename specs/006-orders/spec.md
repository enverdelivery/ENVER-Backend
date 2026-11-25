# Especificación 006 — Pedidos

**v0.1.0 · Borrador para revisión · Responsable de implementación: A.** Rige la [constitución canónica](../../.specify/memory/constitution.md). Las rutas y el ciclo solicitado son requisitos; los detalles de MVP necesitan aprobación. No autoriza implementación.

## 1. Propósito

Reemplazar `SampleArticles` y `PedidosLocalState` con pedidos persistidos y una proyección utilizable por Home.

## 2. Alcance

Publicar, consultar feed/detalle/mis pedidos y cancelar. 006 es dueño del modelo y vocabulario de estados; [007](../007-delivery/spec.md) define las transiciones de entrega.

## 3. Endpoints

| Método y ruta | Éxito |
|---|---|
| `GET /api/v1/orders` | `200` página del feed publicado |
| `GET /api/v1/orders/{id}` | `200` detalle según visibilidad |
| `POST /api/v1/orders` | `201` detalle y `Location` al recurso; repetición válida `200` |
| `GET /api/v1/orders/mine` | `200` página de pedidos propios en cualquier estado |
| `POST /api/v1/orders/{id}/cancel` | `200` detalle en estado `Cancelado`, sin cuerpo de solicitud |

## 4. Contratos de solicitud y respuesta

- Creación: `selectedCategory`, `orderTitle` (1–120 caracteres tras trim), `tarifa` (string decimal positivo, punto decimal, máximo dos decimales) y `deliveryTime` obligatorios. `specialInstructions` (hasta 500), `locationSearch`, `locationDetail`, `selectedAddressLabel`, `selectedHabitualPlace` (hasta 200 cada uno), par numérico `selectedLat`/`selectedLng` y `mediaId` (UUID) son opcionales o null. Exigir etiqueta de dirección o detalle; coordenadas completas, finitas y en rangos ±90/±180 si se envían. `locationSearch` solo sirve como entrada de UI, no se persiste.
- Feed: `id` (UUID string), `category`, `title`, `cost` (importe como string decimal canónico sin símbolo), `is100PercentYours` (boolean, semántica pendiente en §11), `requesterName`, `rating` (número o null), `faculty` (string o null), `imageUrl` (URL HTTPS o null). `rating` y `faculty` quedan null sin fuente aprobada. No exponer `imageResId` ni `photoPath`.
- Resumen de seguimiento: campos del feed más `status`, `deliveryTime`, `createdAt` y `updatedAt` (UTC ISO 8601). `mine` y `assigned-to-me` usan ese resumen. Detalle: resumen más `specialInstructions`, `location` (`detail`, `lat`, `lng`, `addressLabel`, `habitualPlaceLabel`) y `mediaId`; para no participantes, los tres campos adicionales son null.
- Listados: `{ "items": [], "nextCursor": null }`; `limit` opcional de 1–50 (20 por defecto), `cursor` opaco vinculado al listado y a su ámbito, orden `createdAt` descendente y desempate por `id` descendente. `mine` y `assigned-to-me` son rutas literales, no IDs.

## 5. Reglas de negocio

- Estado inicial `Publicado`; secuencia única `Publicado` → `Aceptado` → `En camino` → `Llegué` → `Esperando confirmación` → `Completado`. MVP propuesto: solo `Publicado` → `Cancelado`, por su propietario; ambos estados finales son terminales. Repetir cancelación propia confirmada devuelve el estado actual, sin nueva escritura.
- Categorías: `Papelería`, `Comida`, `Tecnología`, `Otros`. `deliveryTime` preserva `Ahora`, `En 1 hora`, `Hoy más tarde`; en este MVP es una preferencia, no una hora prometida ni un temporizador.
- Creación requiere `Idempotency-Key` (UUID generado por el cliente y reutilizado en retries). Misma cuenta, clave y datos normalizados devuelven el pedido ya creado (estado actual); misma clave con datos distintos produce conflicto. La repetición no vuelve a reclamar media ni vuelve a publicar.
- `mediaId` puede ser null. Si se envía, debe estar listo, pertenecer al solicitante y no estar asociado a otro pedido. `imageUrl` se resuelve para lectura a partir de media; no persistir URL temporal ni aceptar URL del cliente.

## 6. Autorización

Aplicar [005 §§6, 8](../005-users-profile/spec.md). Una cuenta activa puede consultar el feed y publicar como solicitante; el modo de UI no autoriza ni restringe. `mine` y cancelación se acotan a `sub` propietario. Detalle de un pedido publicado visible a cuentas activas con proyección reducida; en otros estados solo propietario y asignado. Solo ellos ven instrucciones y ubicación detallada.

## 7. Persistencia / requisitos de datos

Un documento Mongo Order con UUID del servidor, UUID de propietario y asignado nullable, códigos internos estables de categoría/estado/preferencia con mapping a las etiquetas anteriores, título/instrucciones, ubicación estructurada (pares numéricos, etiquetas de texto), `mediaId` nullable y tiempos UTC. `selectedHabitualPlace` es una etiqueta, no un ID ni un catálogo. Conservar el nombre público efectivo del solicitante como snapshot al crear. Importe BSON Decimal128, no string ni float, con moneda ISO 4217 acordada. `cost` se proyecta, no se almacena. Indexar feed, propietario y asignado según orden de página; unicidad `(ownerId, idempotencyKey)` con huella de solicitud normalizada y unicidad de `mediaId` solo para referencias no nulas. Cancelar con filtro atómico por ID, propietario y estado esperado; no sobrescribir asignaciones ni estados concurrentes.

## 8. Casos de error

Base de 005. `404 not_found` si no hay visibilidad (incluida cancelación ajena). `409 order_state_conflict` si ya no está publicado y `409 idempotency_conflict` para una clave con otro payload. Para medios propios: `409 media_not_ready` si está pendiente, `409 media_rejected` si se rechazó y `409 media_already_attached` si ya está asociado; `404 not_found` si no existe o es ajeno. Ante escritura incierta, no afirmar éxito ni repetir con otra clave.

## 9. Criterios de aceptación

- **AC-ORD-01:** un pedido válido nace `Publicado`; las etiquetas Android y el importe hacen round trip sin pérdida; repetir con la misma clave no duplica el pedido.
- **AC-ORD-02:** feed paginado y detalle respetan visibilidad; `mine` nunca trae pedidos ajenos; sin imagen, `imageUrl` es null.
- **AC-ORD-03:** solo el propietario puede cancelar un publicado; ante carrera cancel/accept solo una mutación gana; media ajena, pendiente o ya usada no se asocia.

## 10. Fuera de alcance

Pagos, geocodificación, catálogo de lugares habituales, edición de pedido, cancelación tras asignación o seguimiento en tiempo real.

## 11. Dependencias / decisiones

[005](../005-users-profile/spec.md) aporta identidad y nombre efectivo; 006 publica el contrato compartido por 007 y [009](../009-ratings/spec.md). [008](../008-media/spec.md) es necesario solo para pedidos con foto. **Antes de cerrar contrato:** confirmar moneda (propuesta MXN), semántica booleana de `is100PercentYours` (no inferir propiedad, tarifa ni permisos) y mapping de importe, ubicación y horarios con Android.
