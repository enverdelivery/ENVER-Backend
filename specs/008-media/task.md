# Tareas 008 — Carga directa de imágenes

**Catálogo propuesto · B · 8 tareas pendientes/no ejecutadas.** Fuentes: [spec](spec.md), [plan](plan.md). Coordinar [006](../006-orders/spec.md) y política [005](../005-users-profile/spec.md). Cada incremento requiere autorización y `E(T-MED-nnn)`; ninguna tarea autoriza provisionar AWS.

- [ ] **T-MED-001 — Contrato y validación.** Implementar DTO y límites de metadata; probar nombres/rutas, tamaño de solicitud y tipos permitidos/no permitidos (AC-MED-01).
- [ ] **T-MED-002 — Reserva persistida.** Crear metadata Mongo y clave staging generada por servidor; probar propiedad, expiración y ausencia de URLs/campos firmados persistidos (AC-MED-01/02).
- [ ] **T-MED-003 — Firma de carga.** Adapter POST multipart una vez confirmado; verificar clave exacta, MIME, condición 1 byte–5 MiB y 300 s con entradas controladas, sin AWS real (AC-MED-01).
- [ ] **T-MED-004 — Endpoint protegido.** Conectar presign, Bearer, `201` con `imageUrl` null y errores tipados. Probar sin token, éxito y fallo de firma sin fuga de campos (AC-MED-01/02).
- [ ] **T-MED-005 — Verificación fuera de API.** Una vez aprobado el ejecutor, validar formato/dimensiones y preparar copia inmutable sin metadata sensible; probar que un objeto inválido y un replay nunca sustituyen la copia entregable (AC-MED-02).
- [ ] **T-MED-006 — Vínculo y lectura.** Proveer lectura de estado/propiedad y firma CloudFront para 006; verificar solo medio propio listo, acceso visible, unicidad de referencia y null sin foto (AC-MED-02/03).
- [ ] **T-MED-007 — Huérfanos y límites.** Tras las decisiones de retención y autorización pertinente, verificar política staging, límites y reconciliación que no borre referencias, incluidas asociaciones concurrentes (AC-MED-03).
- [ ] **T-MED-008 — Verificación local autorizada.** Ejecutar cobertura dirigida con adaptadores simulados, revisar diff y guardar evidencia; señalar por separado validaciones Android/AWS que no estén acreditadas (AC-MED-01–03).
