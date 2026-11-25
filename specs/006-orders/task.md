# Tareas 006 — Pedidos

**Catálogo propuesto · A · 11 tareas pendientes/no ejecutadas.** Fuentes: [spec](spec.md), [plan](plan.md). Cada incremento requiere autorización y evidencia `E(T-ORD-nnn)`.

- [ ] **T-ORD-001 — Contratos Android.** DTOs, validación, etiquetas y proyecciones; verificar payloads y formatos tras cerrar moneda, mapping y `is100PercentYours` (AC-ORD-01/02).
- [ ] **T-ORD-002 — Agregado compartido.** Modelar participantes, estados terminales, UTC, importe y ubicación; comprobar mapeos y dar a 007/009 el contrato de estados/participantes sin esperar el controlador (AC-ORD-01/03).
- [ ] **T-ORD-003 — Persistencia e índices.** Adaptador Mongo, Decimal128, filtros e índices de página y unicidad; verificar BSON, claves duplicadas y `mediaId` nulo (AC-ORD-01–03).
- [ ] **T-ORD-004 — Ámbito y cursores.** Obtener cuenta activa de 005 y validar `limit`/cursor ligados a consulta y ámbito; probar límites y no mezclar páginas ajenas (AC-ORD-02).
- [ ] **T-ORD-005 — Crear e identificar reenvíos.** Conectar POST, snapshot de 005 y huella normalizada; probar 201/Location, reenvío 200, clave conflictiva y ACK incierto sin duplicación (AC-ORD-01).
- [ ] **T-ORD-006 — Leer feed.** Conectar GET con filtro Publicado, proyección y orden estable; probar empates y continuidad (AC-ORD-02).
- [ ] **T-ORD-007 — Leer detalle y propios.** Conectar GET por ID y `/mine`; probar no participantes, asignado, propietario, campos privados y rutas literales (AC-ORD-02).
- [ ] **T-ORD-008 — Cancelar con CAS.** Conectar cancel y probar propietario, estado inválido y repetición; comprobar carrera con 007 al integrar (AC-ORD-03).
- [ ] **T-ORD-009 — Asociar y proyectar media.** Conectar contrato 008, probar medio ajeno/no listo/rechazado/reusado y lectura visible; probar creación sin foto sin dependencia cloud (AC-ORD-02/03).
- [ ] **T-ORD-010 — Frontera HTTP y errores.** Verificar Bearer, JSON estricto, códigos de conflicto y errores sin detalles internos en las cinco rutas (AC-ORD-01–03).
- [ ] **T-ORD-011 — Verificación local autorizada.** Ejecutar suite dirigida y regresión pertinente; registrar evidencia por escenario, incluyendo dependencias aún no verificadas; revisar diff (AC-ORD-01–03).
