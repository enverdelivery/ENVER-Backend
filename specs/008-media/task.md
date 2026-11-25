# Tareas 008 — Carga directa de imágenes

**Presign V1 · 5/8 tareas acreditadas localmente.** Fuentes: [spec](spec.md), [plan](plan.md) y [decisiones aprobadas](../../memoria.md#incremento-008--presign-local). No acredita AWS ni integración con Orders.

- [x] **T-MED-001 — Contrato y validación.** Implementar DTO y límites de metadata; probar nombres/rutas, tamaño de solicitud y tipos permitidos/no permitidos (AC-MED-01).
- [x] **T-MED-002 — Reserva persistida.** Crear metadata Mongo y clave staging generada por servidor; probar propiedad, expiración y ausencia de URLs/campos firmados persistidos (AC-MED-01/02).
- [x] **T-MED-003 — Firma de carga.** Adapter POST multipart; verificar clave exacta, MIME, condición 1 byte–5 MiB y 300 s con entradas controladas, sin AWS real (AC-MED-01).
- [x] **T-MED-004 — Endpoint protegido.** Conectar presign, Bearer, `201` con `imageUrl` null y errores tipados. Probar sin token, éxito y fallo de firma sin fuga de campos (AC-MED-01/02).
- [ ] **T-MED-005 — Verificación fuera de API.** Una vez aprobado el ejecutor, validar formato/dimensiones y preparar copia inmutable sin metadata sensible; probar que un objeto inválido y un replay nunca sustituyen la copia entregable (AC-MED-02).
- [ ] **T-MED-006 — Vínculo y lectura.** Proveer lectura de estado/propiedad y firma CloudFront para 006; verificar solo medio propio listo, acceso visible, unicidad de referencia y null sin foto (AC-MED-02/03).
- [ ] **T-MED-007 — Huérfanos y límites.** Tras las decisiones de retención y autorización pertinente, verificar política staging, límites y reconciliación que no borre referencias, incluidas asociaciones concurrentes (AC-MED-03).
- [x] **T-MED-008 — Verificación local autorizada.** Ejecutar cobertura dirigida con adaptadores simulados, revisar diff y guardar evidencia; señalar por separado validaciones Android/AWS que no estén acreditadas (AC-MED-01–03).

Evidencia local: `MediaPresignTests` (HTTP con firmante simulado), `MediaSigningTests` (política SDK con valores sintéticos, sin tráfico S3), `MediaReservationTests` (Mongo/Testcontainers: 10 aceptadas y 2 denegadas en 12 intentos concurrentes). T-MED-007 queda abierta aunque la cuota y el TTL de sus contadores están probados: no se acredita lifecycle de staging ni reconciliación de huérfanos.
