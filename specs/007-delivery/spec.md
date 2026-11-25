# Especificación 007 — Entrega y asignación rescatista

**v0.1.0 · Borrador para revisión · Responsable de implementación: B.** Rige la [constitución canónica](../../.specify/memory/constitution.md). Las rutas, transiciones, actores y atomicidad solicitados son requisitos; los detalles de MVP necesitan aprobación. No autoriza implementación.

## 1. Propósito

Permitir que un rescatista tome un pedido y avance su entrega hasta que el solicitante la confirme.

## 2. Alcance

Listado por asignación y cinco transiciones sobre el mismo Order de [006](../006-orders/spec.md).

## 3. Endpoints

| Método y ruta | De → a / resultado | Actor |
|---|---|---|
| `GET /api/v1/orders/assigned-to-me` | `200` página de pedidos asignados, incluidos completados | Rescatista asignado (`sub`) |
| `POST /api/v1/orders/{id}/accept` | `Publicado` → `Aceptado` | Rescatista elegible, distinto del propietario |
| `POST /api/v1/orders/{id}/start-delivery` | `Aceptado` → `En camino` | Rescatista asignado |
| `POST /api/v1/orders/{id}/arrive` | `En camino` → `Llegué` | Rescatista asignado |
| `POST /api/v1/orders/{id}/request-completion` | `Llegué` → `Esperando confirmación` | Rescatista asignado |
| `POST /api/v1/orders/{id}/confirm-completion` | `Esperando confirmación` → `Completado` | Solicitante propietario |

## 4. Contratos de solicitud y respuesta

Lista: `limit`/`cursor` y página de resúmenes de 006, con cursor para el ámbito asignado. Transiciones: UUID de pedido en ruta, sin cuerpo; `200` con detalle de 006 y estado vigente, incluida repetición válida. No aceptar `assignedTo`, posición, timestamps, ni estados enviados por el cliente.

## 5. Reglas de negocio

- `accept` fija asignado y estado en la misma escritura; un solo ganador aun frente a otros accepts o cancelación. No saltar pasos, reasignar ni salir de un estado terminal.
- Los pasos ocurren como máximo una vez por pedido en este ciclo sin retrocesos. Si el mismo actor autorizado reenvía un paso que ya aplicó, devolver el detalle vigente sin escribir, incluso si el estado avanzó. No retornar éxito a quien perdió el accept ni a un actor distinto. Sin clave adicional para estas transiciones.
- Tras un resultado incierto, el cliente reenvía la misma acción; si no se puede confirmar el resultado, devolver `503`, no afirmar que la transición no ocurrió. `arrive` es confirmación manual en MVP, sin verificación GPS.

## 6. Autorización

Aplicar [005 §§6, 8](../005-users-profile/spec.md). En `accept` comprobar elegibilidad de servidor (`rescuerEnabled`), también en una repetición; la asignación se obtiene del `sub`. Para los demás pasos y el listado manda la participación persistida, no `activeRole`. Propuesta: deshabilitar elegibilidad impide nuevos accepts, no elimina acceso de una cuenta aún activa a entregas ya asignadas. Solo el propietario confirma el cierre.

## 7. Persistencia / requisitos de datos

Sin colección Delivery. Actualización Mongo condicional atómica por ID y estado esperado, con precondiciones de asignación/propiedad, y asignado nulo al aceptar. Registrar actor e instante UTC del paso en el mismo documento y escritura; usar esa marca para reconocer la repetición tras confirmar datos persistidos. Actualizar `updatedAt` solo al mutar. Prohibido leer y guardar incondicionalmente; no usar locks distribuidos. Índice de asignado conforme a 006.

## 8. Casos de error

Base de 005. `403 forbidden` por falta de elegibilidad o autoprestación sobre pedido visible; `404 not_found` para pedido inexistente/no visible (incluido actor ajeno a una entrega), `409 order_state_conflict` para un estado no válido sobre un pedido visible. `503 temporarily_unavailable` ante resultado incierto no resuelto.

## 9. Criterios de aceptación

- **AC-DEL-01:** solo el actor indicado ejecuta cada transición, en orden; el listado nunca incluye asignaciones ajenas.
- **AC-DEL-02:** dos accepts o accept/cancel concurrentes no producen doble asignación ni estados contradictorios.
- **AC-DEL-03:** repetir una acción aplicada por el mismo actor no vuelve a mutar; el perdedor no obtiene éxito y una acción fuera de orden no altera el pedido.

## 10. Fuera de alcance

Reasignación, múltiples rescatistas por pedido, autoprestación, geofencing, pago, notificaciones, disputas y cierres automáticos.

## 11. Dependencias / decisiones

[006](../006-orders/spec.md) aporta agregado, estados, cancelación y contrato de respuesta. [005](../005-users-profile/spec.md) aporta elegibilidad; sin fuente aprobada no se habilitan accepts reales. Confirmar la regla de entregas en curso al retirar elegibilidad. [009](../009-ratings/spec.md) consume el estado terminal `Completado` y los participantes.
