# Tareas 007 — Entrega y asignación rescatista

**Desarrollador B · 10/10 tareas acreditadas localmente.** Fuentes: [spec](spec.md), [plan](plan.md), [Order 006](../006-orders/spec.md) y [elegibilidad 005](../005-users-profile/spec.md). La evidencia local no habilita accepts reales ni autoriza publicación o despliegue.

**REVIEW SPEC 007 IMPLEMENTATION: CONDITIONAL PASS.** Listo para revisión de código mediante PR, **no aprobado para despliegue ni habilitación operativa**.

- [x] **T-DLV-001 — Contratos compartidos.** Consumir estados/DTOs de 006; probar cursor de asignado y UUID/ausencia de cuerpo (AC-DEL-01).
- [x] **T-DLV-002 — Actores y elegibilidad.** Aplicar cuenta activa y consulta 005 para accept; probar no elegible, autoprestación y ausencia de escalamiento por modo (AC-DEL-01).
- [x] **T-DLV-003 — Listado propio.** Implementar `assigned-to-me` con índice y paginación de 006; verificar aislamiento e inclusión de completados (AC-DEL-01).
- [x] **T-DLV-004 — Asignación atómica.** Implementar accept, marca del paso y filtro no asignado; probar asignación, estado y accept tras cancel (AC-DEL-01/02).
- [x] **T-DLV-005 — Avance por asignado.** Implementar start, arrive y request-completion con precondiciones persistidas; probar actor equivocado y pasos fuera de orden (AC-DEL-01/03).
- [x] **T-DLV-006 — Confirmación del propietario.** Aplicar cierre atómico, probar dueño frente a otros actores y preservar el estado terminal para 009 (AC-DEL-01).
- [x] **T-DLV-007 — Reenvíos y resultados inciertos.** Reconocer marca por paso/actor, revalidar permisos y devolver estado vigente; probar repetición avanzada y ausencia de éxito falso (AC-DEL-03).
- [x] **T-DLV-008 — HTTP y errores tipados.** Cablear rutas, detalle autorizado y códigos 403/404/409/503; verificar que el perdedor no recibe datos privados (AC-DEL-01/03).
- [x] **T-DLV-009 — Carreras críticas.** Con Mongo de pruebas y cancel de 006, verificar un ganador en accept/accept y accept/cancel; no duplicar cobertura (AC-DEL-02).
- [x] **T-DLV-010 — Verificación local autorizada.** Ejecutar pruebas dirigidas, revisar diff y registrar evidencia/límites sin sustituir la aprobación de elegibilidad real (AC-DEL-01–03).

## Evidencia local

- **E(T-DLV-001–008):** [flujo HTTP y BSON](../../tests/Enver.API.IntegrationTests/DeliveryIntegrationTests.cs), [reconciliación e incertidumbre](../../tests/Enver.API.UnitTests/OrderDeliveryTests.cs) y [frontera Bearer](../../tests/Enver.API.SecurityTests/OrderAuthenticationTests.cs). Verificados el aislamiento del cursor, las marcas UTC, los errores y el reenvío sin mutación incluso tras `Completado`.
- **E(T-DLV-009):** dos carreras reales con MongoDB 7.0.48/Testcontainers y clientes independientes, ambas PASS. Una sola mutación confirmada por carrera; la perdedora no hizo match. El estado persistido coincide con el ganador y no mezcla aceptación y cancelación.
- **E(T-DLV-010):** Release con SDK fijado 10.0.403, 0 errores y 0 avisos; unitarias dirigidas 19/19 (7 Delivery, 12 Orders); seguridad dirigida 39/39; integración final 22/22 (Delivery 3, Orders 2, Users 1, Auth 16); cero omitidas. Primera integración: 21/22, fallo en la preparación de perfil del test HTTP de Orders. Control del SHA inicial: Orders 2/2, integración 18/19 por un fallo distinto de disponibilidad en Auth. Sin cambios entre las dos ejecuciones de la implementación ni causa atribuida al primer fallo.
- **Confirmación de revisión:** tres ejecuciones dirigidas de cuatro casos (Orders HTTP, Delivery HTTP, primera escritura de perfil y disponibilidad Auth): 4/4 cada una; no se reprodujo el fallo inicial de Orders.
- **Preparación del commit:** Release SDK 10.0.403, 0 errores y 0 avisos; unitarias Delivery/Orders 19/19. Se reutilizan integración y seguridad anteriores, sin ejecutarlas de nuevo: las huellas de fuentes C# coincidieron con los PDB y los cuatro ensamblados Release conservaron su SHA-256.

## Hallazgos abiertos de la revisión

- **CRITICAL · Compatibilidad BSON y rollback:** el lector estricto de 006 no reconoce `delivery`. Bloquea despliegue y habilitación de escrituras de Delivery hasta aprobar y verificar un lector compatible y una versión mínima de reversión. No se implementó el lector puente.
- **HIGH · Elegibilidad:** `DisabledRescuerEligibility` sigue registrado por defecto. Bloquea `accept` real hasta aprobar e integrar una fuente autorizada; el modo activo sigue sin conceder permisos.
- **MEDIUM · Estabilidad:** fallo histórico del primer *upsert* del perfil en Orders, no reproducido y de causa no determinada. Se conserva el resultado inicial; no se cambiaron pruebas ni presupuestos.
- **LOW · Contrato HTTP:** queda por aclarar la precedencia entre visibilidad pública y participación para los pasos distintos de `accept`. No se amplió el acceso ni se cambió este comportamiento.

**Siguiente gate:** AUTHORIZE SPEC 007 COMMIT AND PR. No autoriza merge, despliegue, escrituras de Delivery ni initialize; histórico 1.
