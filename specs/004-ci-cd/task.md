# Tareas 004 — CI automático y deployment manual

## 1. Identificación, alcance y estado

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.1.0 / 2026-11-12 |
| Solicitud | SDD — CREAR Y MATERIALIZAR 004-CI-CD; BUILD DOCUMENTAL ÚNICAMENTE |
| Fuentes | [spec.md](spec.md) y [plan.md](plan.md) |
| Estado documental | CATÁLOGO MATERIALIZADO PARA REVISIÓN Y APROBACIÓN; no autoriza ejecución |
| Catálogo | T-CICD-001 a T-CICD-030: **30 tareas PENDIENTE / NO EJECUTADA** |
| Gates | GATE-CI-A–E y GATE-CD-A–D: **PENDIENTES / NO SATISFECHOS** |
| Evidencias de ejecución 004 | Ninguna; todas las E(T-CICD-nnn) están pendientes |
| Baseline | `feature/authentication`, `c58ea86e3eddff4befa8f61398dd58d55c4ef985`; conservar los tres Markdown existentes de 003 |

La autorización actual solo crea spec/plan/task de 004. No crea workflows/scripts, recursos AWS/IAM/ECR, Terraform/infra, cambios Docker/producto/tests, configuración GitHub ni Orders. No ejecuta compilaciones, pruebas, scans, pipelines, Terraform, AWS o deployment. Sin stage/commit/push. Aplican [constitución](../../.specify/memory/constitution.md), [001](../001-backend-foundation/spec.md), [002](../002-authentication/spec.md) y [003](../003-infrastructure/spec.md), intactos.

## 2. Reglas de ejecución futura y evidencia

- Antes de cada tarea: autorización explícita de alcance/archivos/comandos/entorno/permisos/costo/reversión y resolución de DP bloqueantes. **Archivos esperados** son futuros, no creados por esta entrega.
- Dependencias exigen resultados aceptados, no permiso automático. Preparar capacidades no equivale a desplegar: implementación de workflows y pruebas de configuración se distinguen de verificación final real en 029/030.
- ECR/IAM/state/readiness/targets/secrets de 003 son dependencias externas sujetas a autorización de 003, no recursos que estas tareas puedan duplicar. Bootstrap inicial se resolverá explícitamente para evitar exigir una imagen antes de crear su registry (DP-CICD-12).
- La capacidad manual puede aprobarse y probarse sin producción antes de la aceptación cloud conjunta. No exigir GATE-I-I final de 003 antes de realizar la prueba que lo acredita; usar sus condiciones de seguridad/capacidad operativa y compartir evidencia, sin cerrar otro spec automáticamente.
- E(T-CICD-nnn) es referencia futura, no reporte existente. Cada E contendrá autorización, requisito/PLD/AC/gate, repo/SHA/revisión de workflow/run/intento/digest cuando aplique, actor/fecha/entorno, comandos realmente ejecutados y salidas nativas/resultados, integridad/referencias saneadas, límites y aprobación.
- Resultados sensibles o planes/state no se publican; artifacts/TRX/logs se revisan antes de upload y tienen acceso/retención aprobados. Mantener incidentes/rojos sin reintentos ocultos ni fakes como prueba cloud.
- Ningún gate verde local de 002 sustituye un run CI del SHA. No fijar para siempre 684/310/223 ni aceptar suites vacías/filtros/omitidas; preservar cero failed/skipped del contrato vigente.
- Primera release solo Authentication; revocación normal requiere superficie protegida autorizada distinta de Logout, sin agregar endpoints productivos. Si falta, no cerrar aceptación distribuida/CD-C.

## 3. Gates formales

**Todos: PENDIENTE / NO SATISFECHO.** Los prerrequisitos son de cierre; no conceden ejecución ni mutaciones cloud. AC-CICD-01–16 están en spec §14. La aceptación final requiere revisión del responsable con evidencia real, no solo YAML válido.

| Gate | Objeto / criterio futuro de cierre | Evidencias de tareas | Prerrequisitos de cierre | AC |
| --- | --- | --- | --- | --- |
| GATE-CI-A | CI estructuralmente válido, eventos/ref/permisos y separación de Deploy sin producción automática | E(001–002), E(029) | Autorización de implementación/validación | AC-CICD-01 |
| GATE-CI-B | SDK exacto, restore locked reproducible y build no-restore/cero errores, warnings acreditados | E(003–004), E(029) | GATE-CI-A | AC-CICD-02 |
| GATE-CI-C | UT/IT/ST completas verdes del SHA, cero failed/skipped, fixtures Docker y resultados/cleanup íntegros | E(005–010), E(029) | GATE-CI-B | AC-CICD-03, 04 |
| GATE-CI-D | Scans y policy verdes/negativos bloqueantes; IaC aplicable controlada sin apply automático | E(011–012), E(019–020), E(029) | GATE-CI-B, GATE-CI-C | AC-CICD-05, 09 |
| GATE-CI-E | Imagen del mismo SHA, scan/integridad y publicación ECR confiable con digest acreditado | E(013–016), E(029) | GATE-CI-B/C/D y GATE-CD-A para publisher OIDC | AC-CICD-06 |
| GATE-CD-A | OIDC/trust/permisos por contexto/rol válidos, PR denegado y sin runtime secrets en GitHub | E(017–018), E(029) | GATE-CI-A; provisión IAM externa de 003 aprobada | AC-CICD-07 |
| GATE-CD-B | Workflow separado, solo dispatch humano/ref confiable/production, elegibilidad exacta y exclusión; rechazos seguros | E(020–022), E(025–026), E(029) | GATE-CI-A a GATE-CI-E, GATE-CD-A | AC-CICD-01, 08, 09, 13 |
| GATE-CD-C | Deployment exacto, dos instancias/targets, HTTPS/health y smoke Auth/distribuido reales verdes, fallos sin éxito falso | E(022–024), E(030) | GATE-CD-B, capacidad operativa segura de 003 y autorización cloud; no su cierre final previo | AC-CICD-10–13 |
| GATE-CD-D | Rollback manual y concurrencia/fallos acreditados; evidencia académica íntegra/saneada y revisión final | E(025–030) | GATE-CD-C y gates anteriores | AC-CICD-14–16 |

Notación E(001) = E(T-CICD-001); un rango exige cada evidencia. GATE-CI-E no es deployment; su verde nunca inicia CD. GATE-CD-D no cierra AUTH-027 integralmente, GATE-G/H de 002 ni GATE-I-I de 003 por asociación.

## 4. Catálogo de tareas

### T-CICD-001 — Baseline GitHub, política y costo

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-1 Baseline GitHub Actions.
- **Objetivo:** confirmar repositorio, default branch, actores y límites antes de configurar CI/CD.
- **Dependencias:** autorización técnica posterior; DP-CICD-01/02/14.
- **Trazabilidad:** CI-001, 002; CD-001, 002, 004, 005; REL-011, 012; PLD-CICD-001/002/011/020; AC-CICD-01, 08, 15, 16.
- **Archivos esperados:** decisiones futuras autorizadas en `specs/004-ci-cd/spec.md`/`plan.md`; sin cambios GitHub implícitos.
- **Acciones futuras:** confirmar owner/repo/main/default branch, plan/visibilidad, actores Run workflow, eventos PR main/push main propuestos, protección de cambios y presupuesto/retención.
- **Aceptación:** decisiones/roles/costo aprobados; solo main recomendado para release, sin tags habilitados ni producción automática.
- **Evidencia:** E(T-CICD-001), aprobación de política y límites saneados.
- **Riesgo:** workflow desde ref no confiable, plan incompatible o costos no previstos.
- **Gate asociado:** GATE-CI-A, GATE-CD-B, soporte GATE-CD-D.

### T-CICD-002 — Estructura separada CI/Deploy y permisos base

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-1 Baseline GitHub Actions.
- **Objetivo:** definir workflows separados sin ruta automática a producción.
- **Dependencias:** T-CICD-001; autorización de crear/configurar workflows, no otorgada ahora.
- **Trazabilidad:** CI-001, 011; CD-001, 002, 004, 005; SEC-006, 009, 012; PLD-CICD-001/007/011; AC-CICD-01, 07, 08.
- **Archivos esperados:** futuros `.github/workflows/ci.yml` y `deploy.yml`, nombres propuestos; ningún archivo creado ahora.
- **Acciones futuras:** estructurar CI automático PR/push aprobado y Deploy exclusivamente workflow_dispatch, sin jobs mutadores hasta autorización propia; contents read y privilegios por job.
- **Aceptación:** estructura validada, workflow Deploy desde ref confiable/default branch, sin push/PR/merge/workflow_run/release/cron/call automático de producción. Un skeleton no produce CI GREEN por sí solo.
- **Evidencia:** E(T-CICD-002), revisión estructural/eventos/permisos y validación autorizada.
- **Riesgo:** trigger heredado, reusable workflow accesible a eventos automáticos o éxito vacío.
- **Gate asociado:** GATE-CI-A, GATE-CD-B.

### T-CICD-003 — SDK exacto y restore locked

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-2 CI restore/build.
- **Objetivo:** reproducir dependencias del SHA con global.json intacto.
- **Dependencias:** T-CICD-002; DP-CICD-03/04.
- **Trazabilidad:** CI-002–004; PLD-CICD-002/003; AC-CICD-02.
- **Archivos esperados:** futuro `ci.yml`; global.json/lockfiles/csproj solo lectura.
- **Acciones futuras:** checkout SHA completo, resolver SDK 10.0.403 sin rollForward y ejecutar restore locked del alcance completo aprobado; validar versiones/salida sin actualizar paquetes.
- **Aceptación:** SDK exacto y `dotnet restore --locked-mode` exitosos; discrepancia/fallo de lock o SDK bloquea. No downgrade/fallback automático.
- **Evidencia:** E(T-CICD-003), SHA, SDK efectivo, comando/exitcode y lockfiles/hash saneados.
- **Riesgo:** runner sin SDK fijo, restore implícito mutable o lockfile regenerado.
- **Gate asociado:** GATE-CI-B.

### T-CICD-004 — Build y warnings efectivos

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-2 CI restore/build.
- **Objetivo:** compilar sin restore adicional y con política de warnings explícita.
- **Dependencias:** T-CICD-003; DP-CICD-04.
- **Trazabilidad:** CI-005; REL-001; PLD-CICD-003; AC-CICD-02.
- **Archivos esperados:** futuro `ci.yml`; no cambios en código/csproj/analyzers.
- **Acciones futuras:** aprobar configuración build/tests/imagen, ejecutar `dotnet build --no-restore`, registrar warnings y controles efectivos sin suprimirlos ni imponer arbitrariamente warnings-as-errors.
- **Aceptación:** cero errores, salida nativa cero y política efectiva acreditada; warnings tratados según configuración aprobada. Un exitcode oculto por wrapper no sirve.
- **Evidencia:** E(T-CICD-004), comando/configuración/SDK/resultados y manifiesto de binarios si se transfieren.
- **Riesgo:** testear binarios distintos de los compilados, restore implícito o errores ocultos.
- **Gate asociado:** GATE-CI-B.

### T-CICD-005 — Suite UT completa

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-3 UT.
- **Objetivo:** ejecutar todos los unit tests existentes del SHA.
- **Dependencias:** T-CICD-004; configuración de resultados y entorno aprobada.
- **Trazabilidad:** CI-006, 009; PLD-CICD-004; AC-CICD-03.
- **Archivos esperados:** futuro `ci.yml`; proyecto UnitTests solo lectura.
- **Acciones futuras:** ejecutar Enver.API.UnitTests sin filtros; usar no-build/no-restore solo con build idéntico acreditado; recoger TRX/salida nativa.
- **Aceptación:** todos los UT descubiertos pasan, cero failed/skipped y suite no vacía; 684 solo antecedente, no conteo fijo. Sin reutilizar ejecución local como run nuevo.
- **Evidencia:** E(T-CICD-005), run/SHA, descubrimiento/ejecución y TRX saneado.
- **Riesgo:** cero tests con exitcode cero, filtros parciales o assembly obsoleto.
- **Gate asociado:** GATE-CI-C.

### T-CICD-006 — Captura e integridad de resultados de tests

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-3 UT.
- **Objetivo:** que resultados ausentes/cancelados/omitidos nunca produzcan verde.
- **Dependencias:** T-CICD-005; DP-CICD-04/13.
- **Trazabilidad:** CI-009–011; REL-010; PLD-CICD-004/020; AC-CICD-03, 15.
- **Archivos esperados:** futuro `ci.yml`; reportes protegidos de run, no TRX crudo versionado.
- **Acciones futuras:** definir captura de exitcode, TRX completo, conteos por suite, SHA/run/intento y saneamiento; recoger evidencia de fallo sin convertir uploads/cleanup en éxito del test.
- **Aceptación:** pruebas autorizadas de resultado faltante/vacío/skipped/error fallan agregación; conteo dinámico y cero omitidas. Captura no imprime datos sensibles.
- **Evidencia:** E(T-CICD-006), contratos/reportes saneados y resultados negativos del recolector.
- **Riesgo:** summary exitoso con runner rojo, TRX truncado o evidencia de otro SHA.
- **Gate asociado:** GATE-CI-C, soporte GATE-CD-D.

### T-CICD-007 — Runner Docker y lifecycle Testcontainers

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-4 IT/Testcontainers.
- **Objetivo:** disponer de dependencias representativas aisladas y recursos medidos.
- **Dependencias:** T-CICD-001, T-CICD-004; DP-CICD-03.
- **Trazabilidad:** CI-007, 008, 010; SEC-006; PLD-CICD-005/007; AC-CICD-04.
- **Archivos esperados:** futuro `ci.yml`; fixture compartida solo lectura, sin cambios de imagen/test/presupuesto.
- **Acciones futuras:** validar runner Linux efímero, SDK/daemon Docker/capacidad/red/imágenes por digest; lifecycle Mongo replica set/Redis/DynamoDB Local/Ryuk de fixture, readiness, namespaces, cleanup y timeout job separado del SUT.
- **Aceptación:** preflight/arranque/cleanup probados bajo autorización, sin puertos/recursos compartidos ajenos ni desactivar Ryuk; fallo Docker bloquea, no omite suite.
- **Evidencia:** E(T-CICD-007), plataforma/recursos/fixture/diagnóstico saneado y cleanup.
- **Riesgo:** runner insuficiente, timeout oculto, imágenes no disponibles o desecho de recursos ajenos.
- **Gate asociado:** GATE-CI-C.

### T-CICD-008 — Suite IT completa

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-4 IT/Testcontainers.
- **Objetivo:** validar fronteras HTTP y dependencias reales/locales representativas del SHA.
- **Dependencias:** T-CICD-006, T-CICD-007.
- **Trazabilidad:** CI-007, 009, 010; PLD-CICD-004/005; AC-CICD-03, 04.
- **Archivos esperados:** futuro `ci.yml`; IntegrationTests/fixtures sin modificación.
- **Acciones futuras:** ejecutar todas las IT secuencialmente después de UT según propuesta; preservar Mongo/Redis/authority reales del harness, salida nativa/cleanup y evidencia de fallos. No retries automáticos de suite tras rojo.
- **Aceptación:** todas las IT del SHA pasan, cero failed/skipped, 310 solo antecedente; no mocks como prueba de dependencia ni extrapolación AWS/Atlas productivo.
- **Evidencia:** E(T-CICD-008), TRX/exitcode/run/SHA y contexto fixture saneados.
- **Riesgo:** estado entre runs, diagnóstico sensible o modificar tests/SUT para acomodar CI.
- **Gate asociado:** GATE-CI-C.

### T-CICD-009 — Suite ST completa

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-5 ST.
- **Objetivo:** conservar controles de Authentication y pruebas de seguridad existentes.
- **Dependencias:** T-CICD-008; build/fixtures/resultados del mismo SHA disponibles.
- **Trazabilidad:** CI-008–010; REL-012; PLD-CICD-004/005/020; AC-CICD-03, 04, 16.
- **Archivos esperados:** futuro `ci.yml`; SecurityTests sin cambios.
- **Acciones futuras:** ejecutar todos los ST después de IT verde bajo propuesta, registrar resultados y fallos sin filtros/supresión; no confundir suite ST con scanner de imagen/source.
- **Aceptación:** todos los ST del SHA pasan, cero failed/skipped, 223 no es total permanente; controles 002 no se rebajan y cleanup conservado.
- **Evidencia:** E(T-CICD-009), TRX/exitcode y relación run/SHA con suite completa.
- **Riesgo:** saltar ST por condicional/costo o asumir pruebas locales como CI.
- **Gate asociado:** GATE-CI-C.

### T-CICD-010 — Agregación fail-closed de UT/IT/ST

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-5 ST.
- **Objetivo:** exigir todas las suites verdes antes de checks de código aprobados.
- **Dependencias:** T-CICD-006, T-CICD-008, T-CICD-009.
- **Trazabilidad:** CI-006–011; PLD-CICD-004; AC-CICD-03.
- **Archivos esperados:** futuro `ci.yml`; summaries/reportes protegidos.
- **Acciones futuras:** agregar estados reales de jobs/suites, validar resultados/total/descubrimiento y distinguir skip de job por fallo de una no aplicabilidad aprobada; ninguna de UT/IT/ST es opcional.
- **Aceptación:** rojo/cancelado/timeout/desconocido/skip/resultado ausente bloquea; no continue-on-error ni success por summary que no revisa dependencias.
- **Evidencia:** E(T-CICD-010), matriz de estados negativos/positivos y salida del agregador.
- **Riesgo:** dependencias `needs`/condiciones que convierten skip en green o omiten una suite.
- **Gate asociado:** GATE-CI-C.

### T-CICD-011 — Herramientas y política de security

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-6 Security scanning.
- **Objetivo:** seleccionar controles compatibles/costeables y criterios bloqueantes explícitos.
- **Dependencias:** T-CICD-001, T-CICD-002; DP-CICD-05/14.
- **Trazabilidad:** SEC-001–005, 011; REL-011; PLD-CICD-006/020; AC-CICD-05, 15.
- **Archivos esperados:** decisiones autorizadas futuras en spec/plan de 004; sin herramienta/paquete instalado por esta tarea documental.
- **Acciones futuras:** evaluar dependencias, secretos, source, imagen e IaC; justificar licencia/plan privado/costo/reportes/.NET 10 y pinning. Aprobar severidad/feeds/vigencia/excepciones/responsables/vencimiento.
- **Aceptación:** herramientas/versiones/policy aprobadas con bloqueos y tratamiento de falsos positivos; ninguna herramienta adoptada por default ni hallazgo oculto por excepción informal.
- **Evidencia:** E(T-CICD-011), matriz de opciones/costo/compatibilidad y aprobación.
- **Riesgo:** scanner sin soporte/licencia, umbral indefinido o falso verde con feed ausente.
- **Gate asociado:** GATE-CI-D.

### T-CICD-012 — Controles automáticos y contrato de security gate

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-6 Security scanning.
- **Objetivo:** integrar controles aprobados y bloqueo ante fallos sin dar privilegios a PR.
- **Dependencias:** T-CICD-004, T-CICD-011.
- **Trazabilidad:** SEC-001–006, 011; CI-011, 012; PLD-CICD-006/007/020; AC-CICD-05, 07.
- **Archivos esperados:** futuro `ci.yml` y configuración propia de scanners solo si se autoriza expresamente; no cambios productivos/tests.
- **Acciones futuras:** configurar deps/secretos/source y puntos de scan imagen/IaC, reportes saneados/exitcodes/policy; verificar origen de acciones/artifacts y negar credenciales PR.
- **Aceptación:** contratos de gates y negativos aprobados; fallo/scanner sin datos/findings bloqueantes impiden elegibilidad. Ejecución sobre imagen final se completa en 013–016/029; no afirmar verde integral antes.
- **Evidencia:** E(T-CICD-012), revisión de configuración y pruebas autorizadas de bloqueo/privacidad.
- **Riesgo:** uploads verdes que ocultan findings, secrets en reporte o código PR con token elevado.
- **Gate asociado:** GATE-CI-D.

### T-CICD-013 — Build de imagen después de checks

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-7 Container build.
- **Objetivo:** construir el artefacto .NET 10 aprobado sin secretos.
- **Dependencias:** T-CICD-010, T-CICD-012; DP-CICD-04/06; contrato imagen de 003.
- **Trazabilidad:** REL-001–003, 005; SEC-004, 010, 011; PLD-CICD-003/008; AC-CICD-06.
- **Archivos esperados:** futuro `ci.yml`; Dockerfile existente solo lectura. Cualquier cambio Docker requiere incremento separado.
- **Acciones futuras:** build del SHA/lock/config exactos tras checks de código, base/toolchain rastreables, non-root/contexto limpio, tag SHA/digest y scan de imagen final aprobado.
- **Aceptación:** build/scan válidos, digest local identificado, sin latest como identidad ni runtime secrets en capas; discrepancia con Dockerfile/SDK bloquea y solicita cambio aparte.
- **Evidencia:** E(T-CICD-013), SHA/config/base/build/scan/digest y revisión de contexto saneados.
- **Riesgo:** build mutable de otro commit, cache no confiable o publicar una imagen distinta de la escaneada.
- **Gate asociado:** GATE-CI-E.

### T-CICD-014 — Manifiesto de release e integridad

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-7 Container build.
- **Objetivo:** enlazar exactamente fuente, CI y artefacto sin confiar solo en labels.
- **Dependencias:** T-CICD-013; DP-CICD-06/13.
- **Trazabilidad:** REL-002, 004, 005, 010; SEC-011, 012; PLD-CICD-008/009/020; AC-CICD-06, 08, 15.
- **Archivos esperados:** futuro `ci.yml`; manifiesto/reportes protegidos de run, esquema en plan de 004 si se autoriza. No manifiesto con secretos en Git.
- **Acciones futuras:** definir repo/SHA/workflow/revisión/run/intento, hashes de resultados/policy, base/imagen/digest y custodia; distinguir reruns/candidatos. Evaluar provenance/SBOM/firmas solo por aprobación.
- **Aceptación:** integridad/autoría/origen verificables mediante run/APIs confiables; rechazo de manifiesto alterado/de PR ajeno/digest diferente. No sobrescritura silenciosa de SHA tag.
- **Evidencia:** E(T-CICD-014), esquema y pruebas autorizadas de integridad/alteración.
- **Riesgo:** manifiesto autodicho por código malicioso, expiración o SHA→digest ambiguo.
- **Gate asociado:** GATE-CI-E, GATE-CD-B, soporte GATE-CD-D.

### T-CICD-015 — Contrato de registry ECR y publicación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-8 ECR publishing.
- **Objetivo:** consumir ECR/003 sin duplicarlo y definir acceso/retención por digest.
- **Dependencias:** T-CICD-014; DP-CICD-07/12, contrato de registry existente/autorizado de 003.
- **Trazabilidad:** REL-002–006, 008; SEC-008; PLD-CICD-009/010; AC-CICD-06, 07, 14.
- **Archivos esperados:** contrato en futuro plan de 004 y configuración `ci.yml`; sin Terraform/recurso ECR creado por esta tarea.
- **Acciones futuras:** acordar cuenta/región/URI, tag SHA inmutable, publisher/pull/verify, retención del digest previo y bootstrap inicial con 003; no permitir PR push.
- **Aceptación:** contrato/roles/outputs y secuencia bootstrap aprobados, sin exigir imagen antes de crear registry ni alterar 003 silenciosamente.
- **Evidencia:** E(T-CICD-015), revisión de integración y permisos/retención sin credenciales.
- **Riesgo:** registry/cuenta equivocados, ciclos de bootstrap o eliminación de artefacto de rollback.
- **Gate asociado:** GATE-CI-E, soporte GATE-CD-A/D.

### T-CICD-016 — Publicación confiable y CI GREEN final

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-8 ECR publishing.
- **Objetivo:** hacer elegible solo el digest construido/validado por CI confiable de main.
- **Dependencias:** T-CICD-015, T-CICD-018; ECR/IAM provisionados bajo autorización de 003; permiso específico de publicar imagen.
- **Trazabilidad:** CI-002, 011; REL-001–005; SEC-006, 008, 009; PLD-CICD-008/009/010; AC-CICD-06, 07, 08.
- **Archivos esperados:** futuro `ci.yml`; imagen/manifiesto en destinos ECR/artifacts autorizados, no repositorio ECR nuevo.
- **Acciones futuras:** job publisher main con OIDC mínimo, verificar origen/scan/artifact, push y digest consultado ECR, finalizar manifiesto y agregar todos los gates. PR queda no elegible aunque su CI sea verde.
- **Aceptación:** SHA/run/intento/digest exactos y ECR verificados; conflicto tag/rerun/missing scan rechaza. CI GREEN final incluye publicación íntegra para main, **no deployment**.
- **Evidencia:** E(T-CICD-016), run confiable/policy/scan/digest ECR y prueba de PR sin publicación.
- **Riesgo:** automatic deploy acoplado al publish, credenciales expuestas o digest sustituido.
- **Gate asociado:** GATE-CI-E.

### T-CICD-017 — Contrato OIDC/trust y separación de roles

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-9 OIDC AWS.
- **Objetivo:** acotar identidad GitHub→AWS sin claves largas.
- **Dependencias:** T-CICD-001, T-CICD-015; DP-CICD-02/07/12.
- **Trazabilidad:** SEC-007–010, 012; CD-005; PLD-CICD-010/011/013; AC-CICD-07, 08.
- **Archivos esperados:** contrato/roles/trust propuestos en plan de 004; implementación IAM de 003 por permiso separado, no archivos Terraform en esta tarea.
- **Acciones futuras:** definir issuer/aud/sub reales y formato con IDs si aplica, contexto main/publisher frente a environment production/deploy; scopes mínimos read/publish/deploy/provisión/runtime/bootstrap y protección de ref.
- **Aceptación:** matriz trust/contexto/acción/recurso aprobada, sin wildcard amplio ni asumir rama por sub environment. Si protección no existe en plan, resolver equivalente explícito o bloquear.
- **Evidencia:** E(T-CICD-017), revisión de trust/roles y referencias de provisión 003 saneadas.
- **Riesgo:** otro repo/PR/ref asume rol o id-token confundido con permiso AWS.
- **Gate asociado:** GATE-CD-A.

### T-CICD-018 — Permissions y validación OIDC autorizada

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-9 OIDC AWS.
- **Objetivo:** comprobar acceso mínimo y denegaciones antes de mutar producción.
- **Dependencias:** T-CICD-002, T-CICD-017; IAM/roles de 003 disponibles y autorización de pruebas AWS acotadas.
- **Trazabilidad:** SEC-006–010, 012; PLD-CICD-007/010/011/013; AC-CICD-07.
- **Archivos esperados:** futuros `ci.yml`/`deploy.yml`; configuración environment/roles solo por permisos explícitos correspondientes, sin crear IAM desde 004.
- **Acciones futuras:** contents read, actions read donde verifica CI, id-token write solo jobs OIDC; no packages write por ECR. Probar identidad/scopes permitidos y rechazo PR/otros refs/repo, sin imprimir tokens ni leer secretos runtime.
- **Aceptación:** permisos/trust verificados, publisher sin deploy/apply, deploy sin administración global/bootstrap; lectura ECR posible sin mutación en preflight. Ninguna credencial larga añadida.
- **Evidencia:** E(T-CICD-018), matriz acceso/denegación/resultados y sesión/actor saneados.
- **Riesgo:** permisos write-all, comandos SSM arbitrarios o falsas denegaciones probadas solo con mocks.
- **Gate asociado:** GATE-CD-A.

### T-CICD-019 — Terraform CI condicional sin apply

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-10 Terraform validation.
- **Objetivo:** validar IaC cuando forme parte del repo sin crearla desde pipeline.
- **Dependencias:** T-CICD-002, T-CICD-011; DP-CICD-09; versiones/lock/provider de 003 cuando existan.
- **Trazabilidad:** CI-012; SEC-004, 005; CD-012; PLD-CICD-006/014; AC-CICD-05, 09.
- **Archivos esperados:** futuro `ci.yml`; `infra/` solo lectura si existe, no creada aquí.
- **Acciones futuras:** condición explícita de alcance infra, fmt-check/validate/IaC scan y init de validación sin backend/credenciales prod si viable; registrar no aplicabilidad si aún no existe.
- **Aceptación:** ausencia infra no simula IaC verde; si existe, controles obligatorios no omitibles y fallos bloqueantes. Sin plan privilegiado/apply automático o credenciales productivas en PR.
- **Evidencia:** E(T-CICD-019), contrato de presencia/ausencia y pruebas autorizadas de validación/fallo.
- **Riesgo:** skip general que evade IaC existente, init a state prod o providers mutables.
- **Gate asociado:** GATE-CI-D, GATE-CD-B.

### T-CICD-020 — Plan/apply controlado y ownership de versión

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-10 Terraform validation.
- **Objetivo:** evitar dos escritores del digest y apply automático.
- **Dependencias:** T-CICD-015, T-CICD-017, T-CICD-019; DP-CICD-08/09/12 y coordinación expresa DP-I-14 de 003.
- **Trazabilidad:** CI-012; CD-012; REL-006–009; PLD-CICD-014/015/019; AC-CICD-09, 14.
- **Archivos esperados:** contrato futuro en plan de 004 y `deploy.yml` si se adopta tramo manual IaC; sin crear/editar Terraform/003 por esta tarea.
- **Acciones futuras:** definir fuente persistente de digest activo solo deploy, arranque/reemplazo/reconcile, plan protegido/rol/state remoto/locking y revisión antes de apply manual. No resolver escribiendo tfvars concurrentes ni ignore_changes nominal.
- **Aceptación:** ownership/protocolo aprobados; CI nunca apply. Plan/adopción de apply y coordinación mutaciones explícitos, sin afirmar DP-I-14 cerrada por documento unilateral. Probar reconciliación real en 030 si aplicable.
- **Evidencia:** E(T-CICD-020), contrato y prueba autorizada del control sin drift; state/plan no publicados.
- **Riesgo:** Terraform vuelve a digest anterior, state expuesto, destrucción/costo o lock que no cubre deploy.
- **Gate asociado:** GATE-CD-B, GATE-CD-D; soporte GATE-CI-D.

### T-CICD-021 — Preflight de deployment manual

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-11 Manual deployment workflow.
- **Objetivo:** rechazar cualquier candidato no acreditado antes de mutaciones.
- **Dependencias:** T-CICD-002, T-CICD-010, T-CICD-012, T-CICD-014, T-CICD-016, T-CICD-018, T-CICD-020; DP-CICD-01/05/06.
- **Trazabilidad:** CD-001–006; SEC-009, 011, 012; REL-004, 005; PLD-CICD-002/011/012; AC-CICD-01, 08, 09, 13.
- **Archivos esperados:** futuro `deploy.yml`, environment production y protección disponible solo bajo permiso específico.
- **Acciones futuras:** validar actor/repo/ref main/default branch/SHA alcanzable, CI workflow/run/intento/head_sha/gates/policy y digest ECR íntegro. Solo lectura OIDC antes de rol mutador si hace falta; congelar/revalidar inputs tras lock.
- **Aceptación:** negativos CI ajeno/rojo/cancelado/skip/incompleto, SHA no main, ref workflow errado, imagen/manifiesto alterado o APIs inciertas se rechazan sin mutación; ninguna segunda aprobación indisponible obligatoria.
- **Evidencia:** E(T-CICD-021), matriz de elegibilidad/negativos y prueba Run workflow autorizada sin deployment productivo implícito.
- **Riesgo:** último verde de otro SHA, TOCTOU, bot dispatch o shell injection por inputs.
- **Gate asociado:** GATE-CD-B.

### T-CICD-022 — Operación manual EC2 por digest

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-11 Manual deployment workflow.
- **Objetivo:** actualizar la versión controladamente sin duplicar recursos 003.
- **Dependencias:** T-CICD-018, T-CICD-020, T-CICD-021; DP-CICD-08/11, contrato operativo/capacidad segura de 003. Primera operación pública exige además 023–026 listos y autorización cloud específica de 030.
- **Trazabilidad:** CD-007, 010; REL-004–007; PLD-CICD-015/016/018; AC-CICD-10, 13.
- **Archivos esperados:** futuro `deploy.yml`; mecanismo/runbook EC2 aprobado, SSM candidato. Sin scripts productivos arbitrarios ni cambios app/Docker.
- **Acciones futuras:** implementar operación manual autorizada con digest congelado/destino fijo, drenaje, versión anterior, espera acotada y comprobación de inventario/digest A/B/targets. Probar configuración en entorno autorizado no productivo antes de 030.
- **Aceptación:** capacidad verificada sin bypass de preflight, mínimo dos instancias en aceptación y control de fallos/incertidumbre; ACK AWS no es éxito, éxito depende de smoke/distribución posteriores.
- **Evidencia:** E(T-CICD-022), revisión de operación/roles y prueba autorizada; evidencia pública se agrega en 030.
- **Riesgo:** commands SSM libres, caída de capacidad, versión mixta o declaración de éxito prematura.
- **Gate asociado:** GATE-CD-B, GATE-CD-C.

### T-CICD-023 — Smoke HTTPS/health/Authentication sintético

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-12 Smoke tests.
- **Objetivo:** definir comprobación pública mínima segura de la primera release.
- **Dependencias:** T-CICD-021, T-CICD-022; DP-CICD-10/11 y readiness/TLS de 003 implementados/autorizados aparte.
- **Trazabilidad:** CD-008, 010; SEC-010; REL-006, 010; PLD-CICD-013/017; AC-CICD-10, 11, 13.
- **Archivos esperados:** futuro `deploy.yml` y helpers de smoke solo si se delimitan/autorizan después; no tests/producto modificados por esta entrega.
- **Acciones futuras:** TLS validado, ALB/liveness/readiness aprobada y Register 201/Login 200/Logout 204/no-store con identidad institucional sintética, token/password en memoria; política de cuotas/retención/limpieza sin permisos DB de deploy.
- **Aceptación:** contrato/implementación de smoke revisados sin credenciales reales/logs sensibles, bypass TLS o retries POST ciegos; ejecución pública verde solo en 030. Fallo smoke impide éxito.
- **Evidencia:** E(T-CICD-023), matriz smoke y revisión de privacidad/cuenta/limpieza; evidencia real enlazada después.
- **Riesgo:** PII/JWT en logs, usuarios de prueba acumulados, cuota bloqueada o readiness ficticia.
- **Gate asociado:** GATE-CD-C.

### T-CICD-024 — Cross-instance y revocación normal

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-12 Smoke tests.
- **Objetivo:** enlazar pruebas A/B y seguridad distribuida con GATE-I-I de 003.
- **Dependencias:** T-CICD-023; DP-CICD-11, superficie protegida/atribución A-B autorizadas y evidencia de su capacidad. Si faltan, bloquear al intentar ejecutar, sin inventar endpoint.
- **Trazabilidad:** CD-009; REL-006, 012; PLD-CICD-017/020; AC-CICD-12, 16.
- **Archivos esperados:** futuro `deploy.yml` y referencias de evidencia en task de 004; superficie de prueba externa autorizada por incremento separado, sin Orders ni endpoint productivo nuevo aquí.
- **Acciones futuras:** probar token normal A/B, Logout/revocación cruzada y cuotas agregadas mediante ALB y telemetría interna; distinguir excepción Logout y no inferir cambio de host por requests repetidas.
- **Aceptación:** contrato/superficie autorizados permiten evidencia normal de rechazo del revocado y cuotas; runtime se acredita en 030. Si solo smoke básico pasa, primera release no se acepta todavía; no cerrar GATE-CD-C/I-I.
- **Evidencia:** E(T-CICD-024), autorización de superficie/matriz A-B y resultados reales cuando se ejecuten, con límites de AUTH-027.
- **Riesgo:** falso positivo con Logout 204 repetido, un único host o nueva superficie pública de diagnóstico.
- **Gate asociado:** GATE-CD-C; aporta evidencia a 003 sin cerrar su gate automáticamente.

### T-CICD-025 — Fallos y rollback manual

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-13 Rollback/concurrency.
- **Objetivo:** recuperar versión compatible sin revertir controles de seguridad.
- **Dependencias:** T-CICD-020, T-CICD-022, T-CICD-023; DP-CICD-08/11.
- **Trazabilidad:** CD-010–012; REL-007, 008; PLD-CICD-015/018; AC-CICD-13, 14.
- **Archivos esperados:** futuro `deploy.yml` y runbook autorizado en plan de 004; sin restauraciones Redis/JWT/authority.
- **Acciones futuras:** operación rollback por nuevo Run workflow/digest previo conocido y aprobado, CI/origen/compatibilidad/security vigentes, health/smoke; manejar AWS API incierta/imagen/dependencia/target/Terraform fallidos con reconciliación.
- **Aceptación:** contrato/control probados en entorno autorizado, sin latest/rebuild/rollback automático, sin reset Redis/cuotas/authority o retirada de claves vigentes. Verificación real final en 030.
- **Evidencia:** E(T-CICD-025), escenarios/decisiones y prueba de control; registrar rojos/estado incierto.
- **Riesgo:** rollback a vulnerable incompatible, pérdida de revocaciones o retry ciego de mutación.
- **Gate asociado:** GATE-CD-B, GATE-CD-D.

### T-CICD-026 — Exclusión de deployments y mutaciones

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-13 Rollback/concurrency.
- **Objetivo:** impedir dos cambios simultáneos en production sin interrumpir el activo.
- **Dependencias:** T-CICD-020, T-CICD-022, T-CICD-025; DP-CICD-09/14.
- **Trazabilidad:** CD-006, 012; REL-009; PLD-CICD-012/019; AC-CICD-08, 14.
- **Archivos esperados:** futuro `deploy.yml` y contrato compartido con flujo manual IaC si se adopta.
- **Acciones futuras:** grupo production-deploy y cancel-in-progress false, coordinación rollback/apply/maintenance, revalidación al obtener turno y procedimiento tras runner perdido. Documentar semántica de pendientes sin prometer FIFO/cola durable.
- **Aceptación:** colisión no produce mutaciones concurrentes ni cancela activo; nueva ejecución reconcilia estado incierto. Lock state no se usa como único lock de release.
- **Evidencia:** E(T-CICD-026), prueba autorizada de colisiones/cancelación/turno y revisión de coordinación.
- **Riesgo:** grupo distinto por workflow, mutación fuera Actions, pendientes reemplazados o runner muerto con deploy aún activo.
- **Gate asociado:** GATE-CD-B, GATE-CD-D.

### T-CICD-027 — Logs, manifiestos y retención saneados

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-14 Observability/evidence.
- **Objetivo:** preservar evidencia útil sin convertir pipeline/artifacts en fuga de datos.
- **Dependencias:** T-CICD-010, T-CICD-012, T-CICD-014, T-CICD-018, T-CICD-021, T-CICD-023, T-CICD-026; DP-CICD-02/06/13.
- **Trazabilidad:** SEC-010–012; REL-004, 010, 011; PLD-CICD-013/020; AC-CICD-15.
- **Archivos esperados:** futuros `ci.yml`/`deploy.yml`, artifacts/manifiestos/reportes protegidos; no state/plan/logs crudos sensibles en Git.
- **Acciones futuras:** identificar run/intento/SHA/digest/actor/outcome, recoger nativas/targets/health y correlación CloudWatch de 003; sanear antes de upload, aprobar acceso/retención/costo y controlar permisos extra necesarios.
- **Aceptación:** pruebas de saneamiento/captura e integridad y revisión de política, sin confiar solo en masking. Resultado del upload/summary no borra fallo original.
- **Evidencia:** E(T-CICD-027), política y ejemplos saneados/controles de acceso e integridad.
- **Riesgo:** TRX/capturas/JWT/planes públicos, cardinalidad/retención costosas o falta de reporte de fallo.
- **Gate asociado:** GATE-CD-D.

### T-CICD-028 — Índice académico y presupuesto de evidencia

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-14 Observability/evidence.
- **Objetivo:** preparar verificación externa trazable de CI a release manual.
- **Dependencias:** T-CICD-027; DP-CICD-02/13.
- **Trazabilidad:** REL-010–012; SEC-012; PLD-CICD-020; AC-CICD-15, 16.
- **Archivos esperados:** índice futuro autorizado en `specs/004-ci-cd/task.md`; paquete externo protegido, completado tras 029/030.
- **Acciones futuras:** preparar matriz requisito/PLD/tarea/AC/gate y entradas CI/build/tests/security/imagen/ECR/digest/Run workflow/actor/targets/TLS/smoke/rollback; plan de retención y costo Actions/artifacts/scanners/transferencia.
- **Aceptación:** estructura/política accesibles al evaluador autorizado, huecos explícitos hasta pruebas reales y sin evidencia local presentada como cloud. No declarar paquete final completo todavía.
- **Evidencia:** E(T-CICD-028), índice modelo/revisión de privacidad/costo y ubicación aprobada.
- **Riesgo:** enlaces temporales rotos, información de cuenta innecesaria o capturas que no acreditan el SHA desplegado.
- **Gate asociado:** GATE-CD-D.

### T-CICD-029 — Validación final CI, security y preflight negativo

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-15 Final validation.
- **Objetivo:** acreditar CI automático completo y rechazo de rutas no elegibles sin desplegar producción automáticamente.
- **Dependencias:** T-CICD-001 a T-CICD-020, T-CICD-021, T-CICD-025, T-CICD-026, T-CICD-027, T-CICD-028; autorización de runs/objetivos/costo y main/workflows integrados mediante proceso Git autorizado aparte.
- **Trazabilidad:** CI-001–012; SEC-001–012; CD-001–006, 012; REL-001–007, 009–012; PLD-CICD-001–015/019/020; AC-CICD-01–09, 13, 15, 16.
- **Archivos esperados:** referencias futuras en task de 004; workflows revisados; reportes en ubicación protegida. Sin modificar producto/tests para inyectar falso verde.
- **Acciones futuras:** runs reales PR/main del SHA, SDK/restore/build/UT/IT/ST/scans/imagen/ECR e integridad; negativos de skip/missing/fallo/scanner/digest/ref/CI ajeno y PR sin OIDC. Verificar ningún automático despliega/apply y preflight rechaza antes de mutar; probes de fallos solo en contexto autorizado seguro.
- **Aceptación:** gates CI y OIDC/manualidad acreditados según evidencia real, no éxito por archivos o conteos históricos. Todas suites/controles requeridos verdes en candidato, negativos rechazan. No deployment por CI ni apertura productiva.
- **Evidencia:** E(T-CICD-029), matriz de runs/negativos/estados, TRX/reportes/manifest/digest y revisión.
- **Riesgo:** pruebas destructivas accidentalmente en producción, rerun oculto, elevar PR o verde parcial de otro SHA.
- **Gate asociado:** GATE-CI-A a GATE-CI-E, GATE-CD-A, GATE-CD-B; soporte GATE-CD-D.

### T-CICD-030 — Validación manual pública, rollback y revisión final

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** CICD-15 Final validation.
- **Objetivo:** acreditar primera release pública Authentication mediante acción humana, evidencia distribuida y recuperación controlada.
- **Dependencias:** T-CICD-021 a T-CICD-029 aceptadas; GATE-CD-B acreditado, 003 operativa/segura, presupuesto y autorización cloud/fallos/rollback/smoke con identidad/superficie de prueba aprobadas. No exigir cierre final GATE-I-I antes de su prueba compartida.
- **Trazabilidad:** CD-001–012; SEC-007–012; REL-004–012; PLD-CICD-010–020; AC-CICD-08–16.
- **Archivos esperados:** actualización futura autorizada de spec/plan/task de 004, referencias a evidencia protegida. Cierre de 003/documentos de gobierno requiere autorización específica aparte.
- **Acciones futuras:** persona usa Actions→Deploy→Run workflow, validar candidato/lock/OIDC y digest real A/B/dos targets/HTTPS/smoke/cross-instance, probar fallos/colisión/rollback manual autorizados y reconcile sin drift; completar índice, revisar AC/gates con responsable.
- **Aceptación:** evidencia real del SHA/digest/actor/flujo manual y todos los checks; fallo smoke o prueba distribuida ausente no es éxito. Rollback no revierte seguridad; ningún recurso/Orders/endpoint no autorizado. Gates solo cerrados explícitamente si acreditados, sin cerrar AUTH-027 integralmente ni 002/003 por inferencia.
- **Evidencia:** E(T-CICD-030), deployment history/CI/digest/targets/TLS/smoke/A-B/rollback/negativos saneados y decisión final con límites/costo.
- **Riesgo:** outage, exposición de credenciales, estado de seguridad revertido, falsa atribución distribuida o despliegue fuera del botón/manualidad.
- **Gate asociado:** GATE-CD-C, GATE-CD-D; aporta evidencia compartida a 003 sin modificarla automáticamente.

## 5. Cobertura documental y cierre de esta entrega

| Requisitos | Tareas principales |
| --- | --- |
| CI-001–002 | 001–003, 016, 021, 029 |
| CI-003–005 | 003–004, 029 |
| CI-006–011 | 005–010, 016, 029 |
| CI-012 | 011–012, 019–020, 029 |
| SEC-001–005 | 011–013, 019, 029 |
| SEC-006–009 | 002, 007, 012, 016–018, 021, 029 |
| SEC-010–012 | 012–014, 017–018, 021, 023, 027–030 |
| CD-001–006 | 001–002, 017, 021, 026, 029–030 |
| CD-007–010 | 022–025, 030 |
| CD-011–012 | 019–020, 025–026, 029–030 |
| REL-001–005 | 013–016, 021–022, 027, 029–030 |
| REL-006–009 | 015, 020, 022–026, 029–030 |
| REL-010–012 | 001, 006, 009, 011, 024, 027–030 |

Cobertura de diseño: **48 requisitos, 20 decisiones propuestas, 15 fases, 16 criterios de aceptación, 30 tareas y 9 gates**. Cobertura no es satisfacción ni evidencia de ejecución. Todas las tareas están PENDIENTE / NO EJECUTADA; todos los gates PENDIENTE / NO SATISFECHO; ninguna E de ejecución registrada.

Revisión documental: 001 no completado retrospectivamente; Authentication local/test intacta con sus gates/diferidos; arquitectura/003 sin cambios; CI automático sin deployment; workflow_dispatch/Run workflow obligatorio; apply no automático; OIDC recomendado con permisos mínimos; secretos fuera de documentos; Orders excluido y primera release solo Register/Login/Logout. DP-CICD-01–14 permanecen abiertos.

Esta entrega termina con los tres Markdown de 004 y validación documental. No ejecutar T-CICD/T-INFRA/T-AUTH, crear workflows/infra/recursos o hacer stage/commit/push. Detenerse y esperar nueva autorización.
