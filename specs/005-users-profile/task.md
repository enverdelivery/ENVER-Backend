# Tareas 005 — Perfil propio y modo activo

**Desarrollador A · 9/9 tareas completadas en validación local.** Fuentes: [spec](spec.md), [plan](plan.md). La evidencia local no acredita CI ni autoriza publicación o despliegue.

- [x] **T-USR-001 — Contratos y validación.** DTOs de §4, distinción omitido/null, allowlist y modos exactos; comprobar JSON estricto y propiedades prohibidas (AC-USR-02/03).
- [x] **T-USR-002 — Cuenta activa y acceso propio.** Usar `sub` y Bearer normal con revocación, leer solo la cuenta correspondiente. Verificar 401 sin token, inválido y revocado; no exponer credenciales (AC-USR-01/03).
- [x] **T-USR-003 — Perfil Mongo.** Adaptador y mapeo separados, UUID único y operaciones parciales; comprobar creación simultánea sin perder campos y BSON Auth intacto (AC-USR-02).
- [x] **T-USR-004 — Leer perfil propio.** Conectar `GET /me`, defaults sin escritura y nombre efectivo consumible por 006; verificar ambos casos de perfil (AC-USR-01).
- [x] **T-USR-005 — Editar perfil.** Conectar PATCH y verificar campos permitidos, borrado de teléfono y rechazo íntegro de campos no editables (AC-USR-02).
- [x] **T-USR-006 — Seleccionar modo.** Conectar PUT y verificar repetición, conservación de campos y ausencia de cambios en permisos (AC-USR-02/03).
- [x] **T-USR-007 — Consultar elegibilidad.** Conectar `rescuer-status` y lectura interna para 007; probar default false y reflejo de una fuente de prueba. No habilitar cuentas reales sin decisión aprobada (AC-USR-03).
- [x] **T-USR-008 — Contrato HTTP seguro.** Cablear resultados/errores y comprobar proyecciones, challenge, denegación y `no-store`; preservar contratos Auth (AC-USR-01–03).
- [x] **T-USR-009 — Verificación local autorizada.** Ejecutar pruebas de 005 y regresión relevante de Auth, revisar diff y registrar resultados, fallos y límites; liberar solo conforme al plan §13 (AC-USR-01–03).

## Evidencia local

- **E(T-USR-001/002/004–008):** [`UsersProfileSecurityTests.cs`](../../tests/Enver.API.SecurityTests/UsersProfileSecurityTests.cs) y revisión del código 005. La prueba HTTP cubre acceso propio, defaults, edición y campos protegidos, ambos modos, elegibilidad y `no-store`; el PUT persiste mediante `$set` del modo únicamente.
- **E(T-USR-003) — PASS:** [`MongoUserProfileStoreTests`](../../tests/Enver.API.IntegrationTests/MongoUserProfileStoreTests.cs) 1/1 con Testcontainers. Verifica lectura inicial sin crear la colección, primeras escrituras concurrentes y parciales, relectura desde otro cliente y un único documento con `_id` UUID. `MongoBsonIntegrationTests` 3/3 confirma el contrato BSON de Auth.
- **E(T-USR-009) — PASS local:** Release 0 errores/advertencias con SDK local 10.0.404 (pin intacto); se conserva evidencia previa de unitarias 844/844 y seguridad dirigida 37/37. Integración final 140/140 (1 perfil y 139 regresiones Auth), sin omitidos. Véase [memoria.md](../../memoria.md#incremento-005--perfil-propio-y-modo-activo) para los intentos previos y los límites del resultado.
