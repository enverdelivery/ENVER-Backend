# Plan 005 — Perfil propio y modo activo

**v0.1.0 · Borrador para revisión · Desarrollador A.** Diseño propuesto a partir de [spec.md](spec.md), bajo la [constitución canónica](../../.specify/memory/constitution.md). No autoriza ejecución técnica.

## 1. Objetivo
Entregar las cuatro rutas propias de §3 sin cambiar la identidad ni los contratos de Authentication.

## 2. Arquitectura reutilizada
MVC, DI, `IUserStore.FindByIdAsync` como lectura de cuenta, Bearer normal y revocación de 002. Mantener JSON estricto, `no-store`, presupuestos acotados y resultados tipados. Sin Redis nuevo.

## 3. Estructura propuesta
`UsersController` y `Features/Users/{Contracts,Application,Core}`; puerto específico para perfil y adaptador Mongo. Exponer a 006 el nombre efectivo y a 007 solo la consulta de elegibilidad; no compartir un DTO HTTP ni el documento de credenciales con esos módulos.

## 4. Persistencia
Perfil separado con `_id` igual al UUID de Auth. Auth conserva ID, correo, estado de cuenta y nombre original; el perfil puede sobreescribir el nombre de presentación y almacenar teléfono y modo. `rescuerEnabled` procede exclusivamente de fuente de servidor aprobada; la ausencia de habilitación da `false`. No almacenar agregados o niveles sin fuente aprobada.

## 5. Flujo de endpoints
- `GET /me`: leer cuenta y perfil; proyectar defaults y nombre de Auth sin insertar si falta perfil.
- `PATCH /me`: validar presencia de campos, aplicar solo los permitidos, devolver perfil efectivo.
- `PUT /me/active-role`: persistir únicamente el modo y devolver perfil efectivo.
- `GET /me/rescuer-status`: proyectar habilitación y nivel según la fuente; nivel y rating no disponibles permanecen null donde correspondan.

## 6. Autorización
Aplicar `Authentication.Normal` y leer cuenta activa desde `sub`; no aceptar ID del cliente ni claims de rol nuevos. **ALTO RIESGO:** elegir `Rescatista` no otorga elegibilidad ni cambia permisos, incluso al crear el primer perfil.

## 7. Validación
PATCH admite solo `fullName` y/o `phone`: omitido conserva, teléfono null borra, nombre null o PATCH vacío fallan. Reusar las reglas de nombre de 002, validar E.164 según la propuesta y aceptar solo los dos valores exactos de `activeRole`. Preservar las reglas JSON de §4.

## 8. Concurrencia e idempotencia
Upsert parcial por UUID, no reemplazo completo; conservar campos ajenos incluso ante la primera escritura simultánea. Reconciliar el conflicto de inserción inicial con actualización acotada de los campos solicitados, no repetir ciegamente una escritura incierta. PUT al mismo valor no añade efectos.

## 9. Errores
Aplicar spec §§6 y 8: errores tipados, 401 con challenge, 403 por falta de permiso, 503 ante seguridad o persistencia no verificable. Si el escritor actual no cubre un estado requerido, añadir adaptación acotada al nuevo alcance sin cambiar errores de Auth.

## 10. Pruebas mínimas
HTTP: propio, sin token, inválido y revocado; contrato de perfil y estado rescatista. Mongo: GET sin escritura, PATCH permitido/prohibido, PATCH/PUT simultáneos. Probar selección de modo con y sin elegibilidad de servidor, sin escalamiento.

## 11. Secuencia
DTOs y lectura propia → perfil Mongo y escrituras parciales → modo/consulta de elegibilidad → regresión. A puede publicar la interfaz lógica de perfil a 006/007 antes de terminar todo 005.

## 12. Riesgos y decisiones abiertas
Confirmar campos y teléfono con Android antes de fijar contrato. Aprobar origen y administración de elegibilidad antes de habilitar accepts reales; no bloquea lectura de perfil ni desarrollo con default seguro. Evitar mezclar datos Auth con proyecciones del perfil.

## 13. Definición de terminado
Cuatro rutas y AC-USR-01–03 aceptados con evidencia de las tareas; regresión de Auth y controles CI aplicables sin relajar. Ninguna habilitación real concedida por la entrega de perfil.
