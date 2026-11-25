# Tareas 005 — Perfil propio y modo activo

**Catálogo propuesto · A · 9 tareas pendientes/no ejecutadas.** Fuentes: [spec](spec.md), [plan](plan.md). Orden por dependencia; cada incremento futuro requiere autorización y evidencia `E(T-USR-nnn)`. No autoriza ejecución.

- [ ] **T-USR-001 — Contratos y validación.** DTOs de §4, distinción omitido/null, allowlist y modos exactos; comprobar JSON estricto y propiedades prohibidas (AC-USR-02/03).
- [ ] **T-USR-002 — Cuenta activa y acceso propio.** Usar `sub` y Bearer normal con revocación, leer solo la cuenta correspondiente. Verificar 401 sin token, inválido y revocado; no exponer credenciales (AC-USR-01/03).
- [ ] **T-USR-003 — Perfil Mongo.** Adaptador y mapeo separados, UUID único y operaciones parciales; comprobar creación simultánea sin perder campos y BSON Auth intacto (AC-USR-02).
- [ ] **T-USR-004 — Leer perfil propio.** Conectar `GET /me`, defaults sin escritura y nombre efectivo consumible por 006; verificar ambos casos de perfil (AC-USR-01).
- [ ] **T-USR-005 — Editar perfil.** Conectar PATCH y verificar campos permitidos, borrado de teléfono y rechazo íntegro de campos no editables (AC-USR-02).
- [ ] **T-USR-006 — Seleccionar modo.** Conectar PUT y verificar repetición, conservación de campos y ausencia de cambios en permisos (AC-USR-02/03).
- [ ] **T-USR-007 — Consultar elegibilidad.** Conectar `rescuer-status` y lectura interna para 007; probar default false y reflejo de una fuente de prueba. No habilitar cuentas reales sin decisión aprobada (AC-USR-03).
- [ ] **T-USR-008 — Contrato HTTP seguro.** Cablear resultados/errores y comprobar proyecciones, challenge, denegación y `no-store`; preservar contratos Auth (AC-USR-01–03).
- [ ] **T-USR-009 — Verificación local autorizada.** Ejecutar pruebas de 005 y regresión relevante de Auth, revisar diff y registrar resultados, fallos y límites; liberar solo conforme al plan §13 (AC-USR-01–03).
