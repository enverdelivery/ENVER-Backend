# Tareas 007 — Entrega y asignación rescatista

**Catálogo propuesto · B · 10 tareas pendientes/no ejecutadas.** Fuentes: [spec](spec.md), [plan](plan.md). Usar [Order 006](../006-orders/spec.md) y [elegibilidad 005](../005-users-profile/spec.md); cada incremento requiere autorización y `E(T-DLV-nnn)`.

- [ ] **T-DLV-001 — Contratos compartidos.** Consumir estados/DTOs de 006; probar cursor de asignado y UUID/ausencia de cuerpo (AC-DEL-01).
- [ ] **T-DLV-002 — Actores y elegibilidad.** Aplicar cuenta activa y consulta 005 para accept; probar no elegible, autoprestación y ausencia de escalamiento por modo (AC-DEL-01).
- [ ] **T-DLV-003 — Listado propio.** Implementar `assigned-to-me` con índice y paginación de 006; verificar aislamiento e inclusión de completados (AC-DEL-01).
- [ ] **T-DLV-004 — Asignación atómica.** Implementar accept, marca del paso y filtro no asignado; probar asignación, estado y accept tras cancel (AC-DEL-01/02).
- [ ] **T-DLV-005 — Avance por asignado.** Implementar start, arrive y request-completion con precondiciones persistidas; probar actor equivocado y pasos fuera de orden (AC-DEL-01/03).
- [ ] **T-DLV-006 — Confirmación del propietario.** Aplicar cierre atómico, probar dueño frente a otros actores y preservar el estado terminal para 009 (AC-DEL-01).
- [ ] **T-DLV-007 — Reenvíos y resultados inciertos.** Reconocer marca por paso/actor, revalidar permisos y devolver estado vigente; probar repetición avanzada y ausencia de éxito falso (AC-DEL-03).
- [ ] **T-DLV-008 — HTTP y errores tipados.** Cablear rutas, detalle autorizado y códigos 403/404/409/503; verificar que el perdedor no recibe datos privados (AC-DEL-01/03).
- [ ] **T-DLV-009 — Carreras críticas.** Con Mongo de pruebas y cancel de 006, verificar un ganador en accept/accept y accept/cancel; no duplicar cobertura (AC-DEL-02).
- [ ] **T-DLV-010 — Verificación local autorizada.** Ejecutar pruebas dirigidas, revisar diff y registrar evidencia/límites sin sustituir la aprobación de elegibilidad real (AC-DEL-01–03).
