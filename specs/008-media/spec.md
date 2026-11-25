# Especificación 008 — Carga directa de imágenes

**v0.1.0 · Borrador para revisión · Responsable de implementación: B.** Rige la [constitución canónica](../../.specify/memory/constitution.md). Carga directa, S3 privado y entrega CloudFront son la dirección aprobada; las concreciones de MVP necesitan aprobación. No autoriza provisionar recursos ni implementar.

## 1. Propósito

Permitir que Android cargue una imagen de pedido directamente a S3 sin pasar los bytes por ENVER API ni recibir credenciales permanentes.

## 2. Alcance

Crear autorización temporal de carga, validar el resultado y entregar la imagen mediante CloudFront al asociarla a un pedido. No añadir otro endpoint ENVER de confirmación.

## 3. Endpoints

`POST /api/v1/media/presign` → `201 Created` con datos de carga temporal.

## 4. Contratos de solicitud y respuesta

- Solicitud JSON: `{"fileName":"pedido.jpg","contentType":"image/jpeg"}`. `fileName` es un nombre simple de hasta 128 caracteres, sin rutas; no determina la clave. Tipos iniciales `image/jpeg` y `image/png`.
- Respuesta: `mediaId` (UUID string), `uploadUrl` (HTTPS), `uploadFields` (mapa string → string, copiar literalmente a la carga), `expiresIn: 300` (segundos), `imageUrl: null` mientras no esté validada y asociada. Los nombres de `uploadFields` son los exigidos por S3, no DTOs ENVER.
- Propuesta: carga S3 mediante POST multipart con los campos devueltos y el archivo; política firmada limita clave exacta, tipo declarado y tamaño de archivo entre 1 byte y 5 MiB. Android no manda Bearer ENVER a S3. Tras la carga, envía `mediaId` a [006](../006-orders/spec.md); solo las proyecciones de un pedido visible devuelven la URL de lectura temporal de CloudFront.

## 5. Reglas de negocio

- S3 privado; clave generada por el backend, por ejemplo `staging/orders/{userId}/{uuid}.jpg` (extensión deducida del tipo permitido). Bloquear acceso público y separación entre objetos de carga y objetos entregables.
- Presign no acredita carga ni contenido válido. Verificación fuera del proceso API sobre el objeto persistido: comprobar tamaño y que los bytes decodifican como JPEG/PNG admitido, con límites de dimensiones; retirar metadatos sensibles antes de publicar. Solo una copia validada a clave final inmutable (no escribible con la firma de carga) queda lista para asociación y lectura. Un replay de la carga no modifica la imagen publicada.
- 006 puede asociar una sola vez un medio propio listo. Si sigue pendiente, Android reintenta crear con la misma clave de idempotencia. La URL de lectura se emite solo si el pedido asociado es visible al llamante; es de corta vigencia, no prueba de propiedad ni valor persistido en Order.
- Huérfanos MVP: expirar por lifecycle objetos de carga en `staging`; limitar reservas/volumen no asociado. Reconciliar metadatos y objetos finales no asociados por separado; no borrar imágenes referenciadas ni hacer limpieza por edad sobre el prefijo de entrega.

## 6. Autorización

Aplicar [005 §§6, 8](../005-users-profile/spec.md). `sub` es el propietario de la reserva. Permiso de carga limitado a una clave temporal; acceso de CloudFront al origen privado y URL firmada para lectura. No registrar URLs/campos firmados ni entregar credenciales AWS permanentes.

## 7. Persistencia / requisitos de datos

Metadata Mongo por `mediaId`: propietario, claves de staging y final (la segunda cuando esté lista), tipo, tamaño verificado, estado (`pending`, `ready` o `rejected`), fecha de reserva/expiración de carga. Order conserva la referencia y aplica la unicidad de asociación; no duplicar la fuente de verdad de la relación. No persistir rutas Android, bytes ni URLs firmadas en Mongo.

## 8. Casos de error

Base de 005; `400 validation_failed` para nombre inválido, `415 unsupported_media_type` para tipo no admitido, `503 temporarily_unavailable` si no se puede reservar o firmar. S3 rechaza cargas fuera de política o expiradas; su respuesta no es Problem Details de ENVER. En 006, `409 media_not_ready` permite reintentar; `409 media_rejected` requiere una nueva autorización de carga.

## 9. Criterios de aceptación

- **AC-MED-01:** Android carga JPEG/PNG sin pasar bytes por la API; clave, vencimiento y tamaño se imponen en S3; el origen no es público.
- **AC-MED-02:** un archivo inválido o ajeno no es asociable; sobrescribir staging no cambia el objeto entregable, y CloudFront solo sirve copias validadas con acceso firmado.
- **AC-MED-03:** solo una orden puede asociar un `mediaId` propio listo; sin foto, `imageUrl` es null. La limpieza no elimina objetos asociados.

## 10. Fuera de alcance

DELETE, galería, vídeo, avatar, búsqueda remota de URLs y entrega pública del bucket.

## 11. Dependencias / decisiones

[006](../006-orders/spec.md) consume `mediaId` y `imageUrl`; pedidos sin foto no requieren 008. **Bloqueantes para fotos:** confirmar POST multipart y límites con Android, definir verificador/copia fuera de la API, límites de reservas, vigencia de lectura y retención/reconciliación de huérfanos. Provisionamiento privado, permisos y firma CloudFront requieren autorización separada; no se asume infraestructura existente para imágenes.
