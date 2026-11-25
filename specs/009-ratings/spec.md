# Especificación 009 — Calificación de entrega

**v0.1.0 · Borrador para revisión · Responsable de implementación: B.** Rige la [constitución canónica](../../.specify/memory/constitution.md). Elegibilidad tras completar y unicidad son requisitos; las concreciones de MVP necesitan aprobación. No autoriza implementación.

## 1. Propósito

Registrar una calificación de una entrega completada, sin introducir un motor de reputación.

## 2. Alcance

Una escritura de calificación por relación permitida y pedido. MVP propuesto: el solicitante propietario califica al rescatista asignado, no a la inversa.

## 3. Endpoints

`POST /api/v1/orders/{id}/rating` → `201` al crear, `200` al repetir la misma calificación ya confirmada.

## 4. Contratos de solicitud y respuesta

- Solicitud: `{"score":5,"comment":"Muy rápido"}`. `score` entero de 1 a 5. `comment` opcional/null, texto plano de hasta 500 caracteres tras trim; vacío se normaliza a null.
- Respuesta: `id`, `orderId` (UUID string), `score`, `comment` y `createdAt` (UTC ISO 8601). No aceptar ni exponer identidad de terceros que no se necesite para esta operación.

## 5. Reglas de negocio

- Solo `Completado` con asignado válido. El servidor determina autor y destinatario a partir de `sub` y el pedido; no se admiten en el cuerpo.
- Para la misma relación/pedido: datos normalizados idénticos devuelven la calificación existente; datos distintos no la reemplazan. No se recalcula un promedio en esta escritura.

## 6. Autorización

Aplicar [005 §§6, 8](../005-users-profile/spec.md). Solo el propietario del pedido puede calificar al asignado bajo la dirección propuesta. No depende del modo activo. La respuesta solo se entrega al autor autenticado.

## 7. Persistencia / requisitos de datos

Registro Mongo con UUID del servidor, pedido, autor, destinatario, puntuación, comentario y timestamp. Índice único `(orderId, authorId, recipientId)` como garantía ante carreras. En duplicado, leer el registro ganador y comparar el contenido normalizado. No se requiere transacción entre pedido y calificación: `Completado` y la asignación son terminales en 006/007; si se cambia esa garantía, revisar este contrato.

## 8. Casos de error

Base de 005. `404 not_found` si el pedido no existe o no es visible; `403 forbidden` si es visible pero el llamante no puede calificar; `409 order_state_conflict` si el pedido propio no está completado; `409 rating_already_exists` si el payload difiere del ya registrado. En escritura incierta no inventar respuesta de éxito; una repetición idéntica puede reconciliarse por la clave única.

## 9. Criterios de aceptación

- **AC-RAT-01:** solo el autor permitido puede calificar un pedido completado; score entero fuera de 1–5 o comentario demasiado largo se rechazan sin escritura.
- **AC-RAT-02:** solicitudes simultáneas o repetidas generan un único registro; reenvío idéntico devuelve ese registro, otro contenido no lo sobrescribe.

## 10. Fuera de alcance

Edición/borrado, listado, agregados Home/Perfil, moderación avanzada, penalizaciones o niveles.

## 11. Dependencias / decisiones

[006](../006-orders/spec.md) aporta identidad y participantes del pedido; [007](../007-delivery/spec.md) completa el pedido y preserva asignación terminal. **Confirmar** dirección solicitante → rescatista y disponibilidad del formulario Android. Los campos `rating` de 005/006 pueden seguir null hasta que se apruebe su proyección.
