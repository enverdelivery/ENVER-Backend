# Especificación 005 — Perfil propio y modo activo

**v0.1.0 · Borrador para revisión · Responsable de implementación: A.** Rigen la [constitución canónica](../../.specify/memory/constitution.md) y el contrato [002 Authentication](../002-authentication/spec.md). Las rutas y la separación de permisos son requisitos; las concreciones de MVP de este borrador se someten a aprobación. No autoriza implementación.

## 1. Propósito

Dar a Home y Perfil una vista de la cuenta autenticada y permitir seleccionar el modo de uso sin cambiar los permisos de la cuenta.

## 2. Alcance

Cuatro operaciones sobre la identidad propia. Este spec define también las convenciones HTTP comunes de 005–009.

## 3. Endpoints

| Método y ruta | Éxito |
|---|---|
| `GET /api/v1/users/me` | `200` perfil |
| `PATCH /api/v1/users/me` | `200` perfil actualizado |
| `PUT /api/v1/users/me/active-role` | `200` perfil actualizado |
| `GET /api/v1/users/me/rescuer-status` | `200` estado rescatista |

## 4. Contratos de solicitud y respuesta

- Perfil: `id` (UUID de la cuenta como string), `fullName`, `email`, `phone` (string o null), `activeRole` (`Solicitante` o `Rescatista`), `rescuerEnabled` (boolean), `rescuerLevel` (string o null) y `rating` (número o null). Sin fuente aprobada para nivel o rating, devolver null.
- PATCH: JSON parcial con `fullName` y/o `phone`. Omitido significa sin cambio; `phone: null` borra el teléfono. `fullName` no admite null y conserva las restricciones de nombre de 002. Teléfono en formato E.164 si se proporciona; no implica verificación. Rechazar un PATCH vacío.
- PUT: `{"activeRole":"Rescatista"}` o `{"activeRole":"Solicitante"}`. Estado rescatista: `{ "rescuerEnabled": false, "rescuerLevel": null }` como ejemplo sin habilitación.
- Común a 005–009: cuerpos JSON UTF-8 camelCase de tipos explícitos, sin claves desconocidas o duplicadas ni coerciones. Las operaciones sin cuerpo no aceptan datos de negocio. No exponer datos sensibles ni cachear respuestas autenticadas compartidamente.

## 5. Reglas de negocio

- Sin perfil persistido, GET proyecta el nombre de la cuenta y los valores `phone=null`, `activeRole=Solicitante`, `rescuerEnabled=false`, sin escribir. Un `fullName` editado se guarda como nombre de perfil, no en Auth; Home utiliza ese nombre efectivo.
- `activeRole` es solo modo de UI. Ambos valores son seleccionables sin modificar `rescuerEnabled`. Solo una fuente de servidor aprobada puede habilitar operaciones de rescatista; ni PATCH ni PUT modifican la habilitación. Si no hay habilitación, nadie obtiene acceso por seleccionar el modo.
- Email, identidad, permisos, estado de cuenta, nivel y rating no son editables. Repetir el mismo PUT no agrega efectos.

## 6. Autorización

Común a 005–009: Bearer con política normal de 002 (no la excepción de Logout), identidad tomada de `sub` validado y cuenta activa para operar. Invalidación/revocación se comprueba antes del acceso; si no puede verificarse la seguridad, se deniega el acceso. No aceptar identidad o permisos declarados por clientes. En 005, solo acceso propio.

## 7. Persistencia / requisitos de datos

Preferir un perfil Mongo separado con clave única igual al UUID de la cuenta. Preservar el `UserDocument` BSON cerrado y el JWT de Auth; cualquier alternativa exige revisión de compatibilidad. Primera escritura crea el perfil; actualizaciones parciales atómicas cambian solo los campos solicitados y no sobrescriben otros datos, incluso si PATCH y PUT coinciden.

## 8. Casos de error

Común a 005–009: `application/problem+json` con `status`, `code` estable y `traceId`; errores por campo seguros cuando proceda. Base: `400 validation_failed` (incluye campos prohibidos), `401 authentication_failed` con challenge Bearer para token ausente, inválido o revocado, `403 forbidden` por falta de permiso de un usuario autenticado, `404 not_found` para recursos no visibles o inexistentes, `413 payload_too_large`, `415 unsupported_media_type`, `429 rate_limited` si aplica y `503 temporarily_unavailable` si no puede verificarse seguridad o persistencia. Los conflictos de negocio tienen código propio, nunca `registration_conflict`. No se cambia el contrato de errores de Auth.

## 9. Criterios de aceptación

- **AC-USR-01:** la cuenta activa lee solo su perfil; sin documento, GET devuelve defaults y no escribe.
- **AC-USR-02:** PATCH aplica solo campos permitidos; rechaza campos protegidos sin cambios. PUT concurrente no pierde otros campos.
- **AC-USR-03:** seleccionar `Rescatista` no cambia la elegibilidad; el estado de consulta refleja la fuente de servidor. Un token inválido o revocado no permite leer ni escribir.

## 10. Fuera de alcance

Cambios de registro, JWT, email o contraseña; alta/administración pública de rescatistas; advertencias, sanciones y motor de niveles.

## 11. Dependencias / decisiones

002 provee `sub` y validación. [006](../006-orders/spec.md) consume el nombre efectivo y [007](../007-delivery/spec.md) la habilitación. **Pendiente para habilitar accept real:** aprobar quién y cómo asigna `rescuerEnabled` sin endpoint adicional; mientras tanto, el valor seguro es false. Confirmar formato de teléfono y campos con Android antes de fijar el contrato.
