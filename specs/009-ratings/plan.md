# Plan 009 — Calificación de entrega

**v0.1.1 · Ejecución local autorizada para V1 · Desarrollador B.** Fuentes: [spec.md](spec.md), [constitución canónica](../../.specify/memory/constitution.md).

## 1. Objetivo
Registrar una calificación por relación permitida tras completar un pedido; no calcular reputación en esta escritura.

## 2. Arquitectura reutilizada
MVC, DI, Bearer normal y cuenta activa de [005](../005-users-profile/spec.md); lectura de participantes y estado de [006](../006-orders/spec.md), finalización terminal de [007](../007-delivery/spec.md). Sin Redis nuevo.

## 3. Estructura propuesta
`RatingsController`, contratos, caso de uso y modelo Rating; puerto específico de escritura/lectura por relación y adaptador Mongo. Reutilizar una lectura mínima de Order, no invocar sus endpoints HTTP.

## 4. Persistencia
UUID propio, `orderId`, `authorId`, `recipientId`, `score`, `comment` y `createdAt` UTC. Índice único `(orderId, authorId, recipientId)`. Sin agregado ni actualización síncrona de Users/Order.

## 5. Flujo de endpoint
`POST /api/v1/orders/{id}/rating`: validar → verificar visibilidad, propietario, asignado y `Completado` → insertar → `201` con la proyección de spec §4; reenvío idéntico confirmado `200` con el mismo registro y fecha.

## 6. Autorización
Solo solicitante propietario → rescatista asignado, según la decisión aprobada. Derivar autor de `sub` y destinatario de Order; nunca del JSON ni de `activeRole`. Devolver la calificación solo al autor autenticado.

## 7. Validación
UUID en ruta; score entero estricto 1–5, comentario opcional/null de hasta 500 caracteres tras trim y vacío normalizado a null. Rechazar campos ajenos, incluidos actor y destinatario.

## 8. Concurrencia e idempotencia
**ALTO RIESGO:** el índice único, no la lectura previa, decide el ganador. En colisión de relación, leer el registro confirmado y comparar datos normalizados; idéntico devuelve el existente, distinto no lo sobrescribe. No se requiere transacción con Order si completado y asignación son terminales. ACK incierto no es éxito; reconciliar solo contra un registro confirmado.

## 9. Errores
Base de 005 y spec §8: 404 no visible/inexistente; 403 visible pero sin permiso; 409 `order_state_conflict` si el pedido propio está incompleto, `rating_already_exists` si la calificación difiere; 503 para resultado no verificable. No exponer errores Mongo.

## 10. Pruebas mínimas
HTTP: pedido propio completado, incompleto, ajeno y no participante, score/límite de texto. Mongo: carrera de inserción y reenvío idéntico/distinto; un único registro y conservación del original.

## 11. Secuencia
Confirmar dirección y contrato de participantes/Completado → DTO y store → caso de uso y endpoint → prueba de unicidad. Trabajar con fixtures del contrato estable sin esperar todos los endpoints 007.

## 12. Riesgos y decisiones abiertas
La dirección está confirmada. Verificar el formulario Android en un gate posterior. Revisar el diseño si se permite reabrir o reasignar pedidos completados. Home/Perfil pueden mantener rating null sin bloquear 009.

## 13. Definición de terminado
Ruta y AC-RAT-01/02 aceptados, con evidencia de autorización, conflicto y unicidad concurrente; sin motor de reputación ni cambios Auth/CI.
