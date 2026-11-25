# Tareas 006 — Pedidos

**Desarrollador A · 10/11 tareas completas localmente para V1 sin foto.** Fuentes: [spec](spec.md), [plan](plan.md) y [decisiones autorizadas](../../memoria.md#incremento-006--pedidos-v1-sin-foto). No acredita CI ni integración con 007/008.

- [x] **T-ORD-001 — Contratos Android.** DTOs, validación, etiquetas y proyecciones; verificar payloads y formatos tras cerrar moneda, mapping y `is100PercentYours` (AC-ORD-01/02).
- [x] **T-ORD-002 — Agregado compartido.** Modelar participantes, estados terminales, UTC, importe y ubicación; comprobar mapeos y dar a 007/009 el contrato de estados/participantes sin esperar el controlador (AC-ORD-01/03).
- [x] **T-ORD-003 — Persistencia e índices.** Adaptador Mongo, Decimal128, filtros e índices de página y unicidad; verificar BSON, claves duplicadas y `mediaId` nulo (AC-ORD-01–03).
- [x] **T-ORD-004 — Ámbito y cursores.** Obtener cuenta activa de 005 y validar `limit`/cursor ligados a consulta y ámbito; probar límites y no mezclar páginas ajenas (AC-ORD-02).
- [x] **T-ORD-005 — Crear e identificar reenvíos.** Conectar POST, snapshot de 005 y huella normalizada; probar 201/Location, reenvío 200, clave conflictiva y ACK incierto sin duplicación (AC-ORD-01).
- [x] **T-ORD-006 — Leer feed.** Conectar GET con filtro Publicado, proyección y orden estable; probar empates y continuidad (AC-ORD-02).
- [x] **T-ORD-007 — Leer detalle y propios.** Conectar GET por ID y `/mine`; probar no participantes, asignado, propietario, campos privados y rutas literales (AC-ORD-02).
- [x] **T-ORD-008 — Cancelar con CAS.** Conectar cancel y probar propietario, estado inválido y repetición; comprobar carrera con 007 al integrar (AC-ORD-03).
- [ ] **T-ORD-009 — Asociar y proyectar media.** Conectar contrato 008, probar medio ajeno/no listo/rechazado/reusado y lectura visible; probar creación sin foto sin dependencia cloud (AC-ORD-02/03).
- [x] **T-ORD-010 — Frontera HTTP y errores.** Verificar Bearer, JSON estricto, códigos de conflicto y errores sin detalles internos en las cinco rutas (AC-ORD-01–03).
- [x] **T-ORD-011 — Verificación local autorizada.** Ejecutar suite dirigida y regresión pertinente; registrar evidencia por escenario, incluyendo dependencias aún no verificadas; revisar diff (AC-ORD-01–03).

**Evidencia:** [unitarias](../../tests/Enver.API.UnitTests/OrderContractTests.cs) 12/12 (contrato, estados y cursor); [integración Orders](../../tests/Enver.API.IntegrationTests/OrdersIntegrationTests.cs) 2/2 (cinco rutas, visibilidad, precisión, índices, clave única, ACK perdido simulado y dos cancelaciones concurrentes con una sola mutación); [Bearer](../../tests/Enver.API.SecurityTests/OrderAuthenticationTests.cs) incluido en seguridad dirigida 38/38. Release sin errores ni advertencias, regresión de integración Auth/Users 21/21. T-ORD-009 queda pendiente de 008: se verifica creación sin foto y 503 para UUID no verificable. La carrera con `accept` queda para 007.
