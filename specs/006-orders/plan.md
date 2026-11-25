# Plan 006 — Pedidos

**v0.1.0 · Borrador para revisión · Desarrollador A.** Fuentes: [spec.md](spec.md), [constitución canónica](../../.specify/memory/constitution.md). No autoriza ejecución técnica.

## 1. Objetivo
Persistir pedidos y entregar feed, detalle, creación, propios y cancelación, con JSON compatible con Android.

## 2. Arquitectura reutilizada
MVC, DI, Bearer normal, cuenta activa, JSON estricto y errores de [005](../005-users-profile/spec.md). Driver Mongo y operaciones acotadas; sin Redis adicional.

## 3. Estructura propuesta
`OrdersController`, `Features/Orders/{Contracts,Application,Core}` y puerto/repositorio Mongo específico. Compartir estado, participantes, lecturas/operaciones de Order y proyecciones autorizadas con [007](../007-delivery/spec.md) y [009](../009-ratings/spec.md), no detalles BSON ni controladores.

## 4. Persistencia
Un documento por UUID: propietario, asignado nullable, nombre efectivo como snapshot, categoría, título, instrucciones, importe Decimal128 y moneda ISO acordada, ubicación estructurada, preferencia de horario, `mediaId` nullable, estado y tiempos UTC. Códigos internos mapean las etiquetas del spec. Estados: Publicado → Aceptado → En camino → Llegué → Esperando confirmación → Completado; Cancelado es terminal alternativo. Preparar índices de feed, propios y asignados por orden de página, único propietario/clave idempotente con huella y único parcial para `mediaId` no nulo. No guardar `cost`, `locationSearch`, rutas Android ni URLs firmadas.

## 5. Flujo de endpoints
- Feed: solo publicados, con proyección Home. Mine: filtrar por propietario y proyectar resumen en todos los estados. Cursor por `(createdAt,id)` descendente, ligado a consulta/ámbito, sin offset.
- Detalle: aplicar visibilidad antes de proyectar; no participantes de un publicado reciben instrucciones, ubicación y `mediaId` null.
- Crear: validar → buscar repetición por propietario/clave y huella → para nueva creación resolver nombre y validar media opcional → insertar `Publicado` → proyectar. Confirmado nuevo: `201` y `Location`; repetición válida: `200` con estado vigente.
- Cancelar: actualización condicional sobre propio publicado; `200` tras confirmar cambio o repetición propia ya cancelada.

## 6. Autorización
Cuenta activa para feed/creación. Propietario para mine/cancel; propietario o asignado para detalle no publicado y datos privados. `activeRole` no decide acceso. Verificar pertenencia y preparación de media antes de asociarla; firmar lectura solo tras visibilidad.

## 7. Validación
DTOs estrictos de spec §4: categorías/horarios exactos, campos y longitudes, `tarifa` como string decimal con hasta dos decimales, coordenadas completas y acotadas, etiqueta o detalle, IDs UUID, `Idempotency-Key` y `limit` 1–50 (20 por defecto). No persistir la búsqueda de ubicación ni admitir `photoPath` o `imageResId`.

## 8. Concurrencia e idempotencia
**ALTO RIESGO:** cancelar y aceptar compiten por el estado esperado en el mismo documento; condición atómica por ID, propietario y `Publicado` para cancel. No reemplazar documentos ni requerir una versión adicional en MVP. Índices únicos resuelven carreras de creación y asociación; ante colisión de clave, comprobar primero si es reenvío del mismo pedido antes de clasificar conflicto de media. No revalidar ni reclamar media para un reenvío confirmado. Timeout incierto no prueba ausencia de efectos.

## 9. Errores
Mapear base 005 y códigos de spec §8: `order_state_conflict`, `idempotency_conflict`, `media_not_ready`, `media_rejected`, `media_already_attached`; `404` para recursos/medios ajenos no visibles y `503` si no se confirma el resultado. No filtrar errores Mongo.

## 10. Pruebas mínimas
Creación, JSON inválido, round trip y reenvío idéntico/distinto; propio, feed paginado estable, detalle reducido/no visible; cancelar propio/ajeno y estado inválido. Mongo: clave única y, con 007, carrera cancel/accept. Contrato con 008: medio ajeno, pendiente, rechazado y duplicado; sin foto no requiere AWS.

## 11. Secuencia
Fijar modelo, estados, proyecciones y consultas compartidas → persistencia + creación/lecturas → cancelación → asociación con 008. B puede usar el contrato estable para 007 antes de cerrar las cinco rutas; entrega inicial sin foto posible como incremento, no cierre de toda la integración Media.

## 12. Riesgos y decisiones abiertas
Moneda (MXN es propuesta), semántica de `is100PercentYours`, mapping de importe, ubicación y horario requieren confirmación para cerrar contrato. Confirmar cancelación limitada a publicado. No inferir permisos ni booleanos de negocio desde Android. Coordinar con 008 la vida de un medio listo ante asociación y limpieza.

## 13. Definición de terminado
Cinco rutas, AC-ORD-01–03 y dependencias de aceptación comprobadas; distinguir evidencia sin foto de validación con 008 y de carrera con 007. Contratos Android y gates aplicables aprobados sin cambios a Auth.
