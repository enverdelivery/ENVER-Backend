# Tareas 009 — Calificación de entrega

**Ratings V1 · B · 7/8 tareas acreditadas localmente.** Fuentes: [spec](spec.md), [plan](plan.md). Se reutilizan los participantes de [006](../006-orders/spec.md) y la terminalidad de [007](../007-delivery/spec.md). No se acredita Android ni entorno remoto.

- [x] **T-RAT-001 — Contratos y validación.** Definir DTOs y normalización de §4; probar score, comentario y rechazo de actores enviados (AC-RAT-01).
- [x] **T-RAT-002 — Modelo y adaptador Mongo.** Persistir la relación con índice único y proyección segura; verificar BSON e índice (AC-RAT-02).
- [x] **T-RAT-003 — Lectura de pedido y permiso.** Derivar actores desde cuenta activa y participantes de 006/007 bajo la dirección aprobada; comprobar propio completado, incompleto y ajeno (AC-RAT-01).
- [x] **T-RAT-004 — Escritura y reenvío.** Insertar, recuperar ganador por relación y comparar datos normalizados; probar idéntico/distinto y resultado incierto (AC-RAT-02).
- [x] **T-RAT-005 — Ruta HTTP.** Cablear POST con Bearer normal, JSON estricto y errores tipados; verificar `201`/`200`, estados de error y no exposición de destinatario (AC-RAT-01/02).
- [x] **T-RAT-006 — Unicidad concurrente.** Ejecutar solicitudes simultáneas en Mongo de pruebas; comprobar un registro y que la original no se sobrescribe (AC-RAT-02).
- [ ] **T-RAT-007 — Contrato de entrega móvil.** Comprobar JSON del formulario confirmado y respuesta contra fixtures del contrato aprobado; sin implementar agregados (AC-RAT-01/02).
- [x] **T-RAT-008 — Verificación local autorizada.** Ejecutar pruebas dirigidas, revisar diff y registrar evidencia y límites (AC-RAT-01/02).

## Evidencia local

- **E(T-RAT-001):** [19 casos de validación](../../tests/Enver.API.UnitTests/RatingInputTests.cs).
- **E(T-RAT-002–006):** [HTTP + Mongo Testcontainers](../../tests/Enver.API.IntegrationTests/RatingsIntegrationTests.cs) verifica BSON, índice, ciclo real 007 con elegibilidad de prueba, privacidad, respuesta, concurrencia y pérdida simulada del ACK. [Seguridad HTTP](../../tests/Enver.API.SecurityTests/RatingsAuthenticationTests.cs) verifica Bearer ausente, inválido y revocado, y revocación indisponible.
- **E(T-RAT-008):** SDK 10.0.403, restore bloqueado y Release PASS (0 advertencias/errores); unitarias 38/38 (19 Ratings y 19 regresión Orders/Delivery), seguridad dirigida 41/41, integración 6/6 (1 Ratings y 5 regresión Orders/Delivery), `git diff --check` PASS.
