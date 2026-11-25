# Plan 008 — Carga directa de imágenes

**v0.1.0 · Borrador para revisión · Desarrollador B.** Fuentes: [spec.md](spec.md), [constitución canónica](../../.specify/memory/constitution.md). No autoriza ejecución ni cambios cloud.

## 1. Objetivo
Una ruta de presign para carga directa Android → S3 privado y lectura temporal por CloudFront, sin bytes en ENVER API.

## 2. Arquitectura reutilizada
MVC, DI, Bearer normal, errores de [005](../005-users-profile/spec.md) y Mongo. Options tipadas para parámetros de entorno y adaptadores específicos de firma; sin Redis añadido.

## 3. Estructura propuesta
`MediaController`, contratos y aplicación Media, store de metadata y adaptadores de firma. Aislar el verificador fuera del proceso API; no seleccionar aún un servicio nuevo. [006](../006-orders/spec.md) consume estado/propiedad por `mediaId` y solicita URL de lectura solo tras autorizar el pedido.

## 4. Persistencia
UUID por reserva; dueño, clave staging, clave final cuando esté lista, tipo, tamaño verificado, fechas y estado `pending/ready/rejected`. 006 es única fuente de asociación y aplica índice único de medio. No guardar URLs, campos firmados ni bytes.

## 5. Flujo de endpoint
`POST /api/v1/media/presign`: validar → reservar para `sub` → firmar → `201` con `mediaId`, `uploadUrl`, `uploadFields`, `expiresIn: 300`, `imageUrl: null`. Propuesta: Android manda POST multipart a S3 sin Bearer; política fija clave, MIME declarado y rango real de archivo 1 byte–5 MiB. El verificador externo valida el objeto, retira metadata sensible y prepara copia inmutable. Sin endpoint de confirmación adicional.

## 6. Autorización
**ALTO RIESGO:** clave/propietario derivados del servidor; firma de carga solo para staging. S3 sin lectura pública; CloudFront con acceso privado al origen y firma temporal de lectura para objetos validados de pedidos visibles. No devolver credenciales permanentes ni registrar material de firma.

## 7. Validación
JSON estricto y cuerpo acotado; nombre simple hasta 128 caracteres, sin rutas, MIME JPEG/PNG. No confiar en nombre, extensión ni tipo declarado. La API limita metadata; S3 aplica límite de bytes y el verificador comprueba tamaño, decodificación y dimensiones reales antes de declarar `ready`.

## 8. Concurrencia e idempotencia
Presign crea una reserva acotada, no confirma carga. Procesar validación repetida sin cambiar la copia final ya publicada, verificando la misma versión del objeto cuyos bytes se procesan. Un reenvío a staging no altera la clave final. Lifecycle solo en staging; la reconciliación de huérfanos finales debe impedir carrera con asociación, sin borrar referencias vigentes. No habilitar limpieza final hasta aprobar esa coordinación.

## 9. Errores
400 nombre inválido, 415 tipo no permitido, 503 si reserva o firma no se confirman. S3 tiene contrato de errores propio. Para 006 distinguir ajeno/no visible, pending, rejected y media asociado según 006 §8.

## 10. Pruebas mínimas
HTTP para autorización, respuesta y errores, metadata excesiva, clave no controlada, dueño y expiración. Con adaptadores simulados verificar condiciones firmadas, rechazo/validación, replay de staging, ausencia de filtraciones y lectura según visibilidad; integración AWS real solo con autorización separada.

## 11. Secuencia
Contrato con Android y A → metadata y firma aislada → validar/copia y proyección con 006 → lifecycle/reconciliación y aceptación. Se puede trabajar en paralelo con Orders sin foto.

## 12. Riesgos y decisiones abiertas
Confirmar multipart/compatibilidad SDK y Android antes de fijar firma; alternativa requiere revisión del contrato. Definir ejecutor externo, límites de dimensiones/reservas, vigencia de lectura y retención/limpieza. S3 privado, CloudFront, permisos y configuración se autorizan por separado; no se presumen existentes.

## 13. Definición de terminado
AC-MED-01–03 acreditados, incluidos tamaño impuesto, aislamiento y vínculo único con 006; simulaciones no acreditan AWS real. Sin proxy de bytes, DELETE ni exposición del origen.
