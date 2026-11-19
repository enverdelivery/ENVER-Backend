# Plan 004 — Cinco fases para CI/CD y deployment DEV manual

## 1. Control de versión y autorización

| Campo | Estado vigente |
| --- | --- |
| Versión | 0.2.0 |
| Fuente | [spec.md](spec.md), decisiones finales aprobadas por el responsable |
| Estado | DISEÑO APROBADO; implementación y verificaciones técnicas NO EJECUTADAS |
| Plan | Exactamente cinco fases; diez tareas grandes |
| Catálogo único | [tasks.md](tasks.md), T-CICD-101–110, todas PENDIENTE / NO EJECUTADA |
| Gates | GATE-CI-A–E y GATE-CD-A–D, todos PENDIENTE / NO SATISFECHO |
| Próximo gate | IMPLEMENT COMPLETE CI/CD PIPELINE, no iniciado automáticamente |

Este gate modifica solo spec.md, plan.md y tasks.md. No crea los workflows que describe ni scripts/tests/IAM/infra, no ejecuta comandos de aplicación, pruebas, scans, AWS o deployment, ni realiza commit/push. Las decisiones de ramas, herramientas condicionales, OIDC, manualidad, imagen única y compensación ya están aprobadas; solo sus datos operativos y evidencias permanecen pendientes.

El catálogo singular task.md preexistente queda sustituido como fuente operativa por tasks.md; se conserva sin editar, mover o borrar fuera de este alcance. No usar sus treinta tareas ni sus reglas antiguas para dirigir el siguiente gate. Los IDs T-CICD-101–110 evitan confundir el catálogo nuevo con evidencias anteriores. Alcance/registro de aprobación en spec §1; constituciones y 001/002/003, memoria/AGENTS/README intactos.

## 2. Decisiones aprobadas y trazabilidad

| Decisión | Contrato vigente | Requisitos principales | Tareas |
| --- | --- | --- | --- |
| PLD-CICD-001 | Tres workflows ci.yml, hardening.yml y deploy-dev.yml; jobs separados y deploy solo dispatch humano | CI-001/011; CD-001/002 | 101, 104, 107 |
| PLD-CICD-002 | feature/*→PR→develop→PR→main; develop integra y origina candidatas DEV, main estable. Validación por SHA integrado exacto | CI-001/002; CD-003/004 | 101, 106, 107 |
| PLD-CICD-003 | SDK10.0.403/disable, restore locked, build Release y warnings efectivos sin supresión | CI-003–005 | 101, 102 |
| PLD-CICD-004 | UT/IT/ST/runtime completos aplicables y contratos, resultados nativos íntegros, cero failed/skipped, sin retries de suites | CI-006–009/011 | 101, 102, 110 |
| PLD-CICD-005 | Runner efímero/Docker/fixtures existentes Mongo replica set/Redis noeviction/Dynamo Local/Ryuk; aislamiento, cleanup y presupuesto SUT intacto | CI-007/008/010 | 102 |
| PLD-CICD-006 | Semgrep, Dependency Review+NuGet audit, Gitleaks, Hadolint, Trivy, CodeQL y Stryker/ZAP/RESTler según compatibilidad/política | CI-012; SEC-001–005 | 103, 104 |
| PLD-CICD-007 | CI sin AWS; no privilegios PR/forks ni ejecución de artefactos no confiables en jobs privilegiados | SEC-006/011/012 | 101, 103–107, 110 |
| PLD-CICD-008 | Una construcción de imagen linux/amd64 por candidata; mismo OCI/manifiesto para hardening, ECR y deploy; cero push PR | REL-001–005 | 104, 106, 108 |
| PLD-CICD-009 | ECR existente; registro verificable SHA/run/intento/policy/digest/evidencia, retención anterior y rechazo de sustitución/rerun ambiguo | REL-002–006; SEC-008/011 | 104, 106, 107 |
| PLD-CICD-010 | OIDC obligatorio; CI sin AWS, Publisher ECR, Deploy ECR-read/SSM-A-B/target-health-read, runtime actual IMDSv2 | SEC-007–009/012 | 105, 106, 107 |
| PLD-CICD-011 | Environment development, workflow disponible en default branch para botón y ejecutado desde develop confiable; actores/ref protegidos | CD-001–005; SEC-012 | 101, 105, 107 |
| PLD-CICD-012 | Selección automática última candidata aprobada develop, sin digest manual; prueba de CI/hardening exactos, congelación y revalidación bajo exclusión | CD-003/006; REL-004/005 | 106, 107 |
| PLD-CICD-013 | Preservar secretos/runtime; pruebas sintéticas en memoria, reportes redactados sin confiar solo en masking | SEC-002/010–012; CD-008/012 | 102–110 |
| PLD-CICD-014 | IaC existente con fmt/validate/scan sin AWS/state remoto; nunca apply en pipeline de aplicación | CI-012; CD-012; REL-006 | 103, 107 |
| PLD-CICD-015 | Versión activa persistente por mecanismo existente; deploy único escritor; no revertir digest por Terraform, wrappers antiguos o reinicialización | REL-006/007; CD-012 | 107, 108, 109 |
| PLD-CICD-016 | Rolling secuencial A/readiness/target healthy antes de B; digest/binding/servicio/health efectivos, no ACK como éxito | CD-007/010; REL-005/006 | 108, 109 |
| PLD-CICD-017 | HTTPS health obligatorio, smoke AUTH según input; liveness no readiness integral ni prueba distribuida por logout repetido | CD-008/009; REL-012 | 102, 109, 110 |
| PLD-CICD-018 | Compensación automática MISMO workflow manual: fallo A→solo A; fallo B→B y A si cambió; original FAILED incluso si rollback PASS | CD-010/011; REL-008 | 108, 109, 110 |
| PLD-CICD-019 | Exclusión enver-development-deploy para todo run/compensación; cancel-in-progress=false, reconciliación SSM/runner y coordinación externa | CD-006/010; REL-009 | 107–110 |
| PLD-CICD-020 | Gates fail-closed, cinco estados, reportes saneados, pinning/coste/retención y aceptación por evidencia sin cerrar otros specs por asociación | CI-011; SEC-005/011/012; REL-010–012 | 101–110 |

Números de tarea abreviados: 101 = T-CICD-101. Estas veinte decisiones sustituyen el texto previo incompatible; no hay quince fases activas ni despliegue exclusivo de main/production.

## 3. Cinco fases y capacidades completas

| Fase | Tareas grandes | Capacidad entregable futura | Condición para avanzar |
| --- | --- | --- | --- |
| **PHASE 1 — CI foundation** | 101, 102 | Grafo CI/eventos/SDK/Release/UT y suites IT/ST/runtime/contratos con resultados completos | CI confiable sin AWS, fixtures aisladas y agregación de tests fail-closed |
| **PHASE 2 — DevSecOps gates** | 103, 104 | Fast gate de scanners/IaC y build once/hardening selectivo con todos sus jobs/reportes | Evidencia source/imagen y política aplicable, sin privilegios de publicación ni target inseguro |
| **PHASE 3 — OIDC + ECR promotion** | 105, 106 | Identidades mínimas y promoción de la misma imagen develop a ECR con registro verificable | OIDC/IAM disponibles por autorización propia; digest remoto igual y sin deploy automático |
| **PHASE 4 — Manual rolling deployment** | 107, 108 | Botón/preflight/candidata/lock y orquestación A→B por digest en baseline existente | Capacidad implementada/revisada; ninguna mutación real antes de verificación/compensación de fase5 listas |
| **PHASE 5 — Verification + rollback** | 109, 110 | Health/smoke/compensación integrada y validación final positiva/negativa con evidencia | Original FAILED ante fallo, retorno validado o recuperación explícita; cierre de gates solo con evidencia |

Las fases agrupan capacidades, no conceden ejecución automática al completarlas. 108 construye el flujo rolling con contrato de verificación/compensación; 109 completa y verifica su integración antes de habilitar un run mutador. No exigir gates de cierre cloud antes de poder realizar su prueba, ni usar esa ausencia para omitir precondiciones de seguridad. 110 necesita autorización de runs/objetivos/operaciones AWS específica además de implementación revisada.

## 4. Diseño de CI y hardening

### 4.1. Grafo FAST PR GATE

Los IDs siguientes son futuros IDs de jobs, no YAML creado:

| Job ID / nombre visible | needs / condiciones |
| --- | --- |
| detect_changes / Detect Changes | Sin dependencias; siempre calcula alcance y deja gate visible |
| build_test / Build & Test | detect_changes; restore/build Release/UT del SHA |
| integration_validation / Mongo / Redis / Dynamo Integration Validation | build_test; IT y ST completas; binarios transferidos verificados o compilación declarada del mismo SHA |
| runtime_tool_tests / Runtime Tool Tests | detect_changes; inventario/config/dependencias propio fuera de slnx |
| semgrep / Semgrep SAST | detect_changes |
| dependency_review / Dependency Review | detect_changes; delta PR y NuGet audit completo; sin confundir disponibilidad/licencia con éxito |
| secret_scan / Secret Scan | detect_changes; detector/redacción sin revelar findings sensibles |
| container_lint / Dockerfile / Container Lint | detect_changes |
| iac_validation / IaC Validation | detect_changes; infra existente, sin backend ni AWS |
| contract_tests / Contract Tests | integration_validation; verificar evidencia de aserciones contractuales del mismo SHA/run, no solo presencia de TRX |
| docker_build / Docker Build | build_test, integration_validation, runtime_tool_tests, semgrep, dependency_review, secret_scan, container_lint, iac_validation, contract_tests; solo resultados aplicables acreditados |
| pr_quality_gate / PR Quality Gate | detect_changes y todos los jobs anteriores; condición de finalización que evalúe también failure/cancelled/skipped |

Detect Changes no suprime a nivel de evento los checks requeridos. Condiciones internas y documento de alcance distinguen NOT APPLICABLE de skip imprevisto. Ante diff no calculable, ejecutar controles. Un job bloqueado por dependencia fallida no se trata como no aplicable. Un PR docs-only puede terminar con no aplicabilidades demostradas y sin candidata; no se vuelve desplegable por eso.

Las tres suites de API se mantienen completas cuando cambia su ámbito; tests omitidos dentro de suite fallan. Los tests runtime ejecutables no se consideran cubiertos por dotnet test de la solución. No desactivar Ryuk ni duplicar servicios de Testcontainers; solo DynamoDB Local con credenciales sintéticas, nunca el servicio AWS real. Timeout de job y presupuesto de fixture/SUT son distintos; no aumentar el segundo para esconder runner insuficiente.

### 4.2. Grafo HARDENING GATE

| Job ID / nombre visible | needs / condiciones |
| --- | --- |
| verify_candidate / Verify Candidate | Verificar CI exacto terminado, origen develop confiable y artefacto OCI; sin rebuild |
| container_scan / Container Vulnerability Scan | verify_candidate; analizar el manifest candidato exacto |
| codeql / CodeQL SAST | verify_candidate; compilación instrumentada propia si precisa, sin reconstruir imagen |
| mutation / Mutation Testing | verify_candidate; política manual/periódica/candidata y alcance medido |
| dynamic_preflight / Dynamic Target Preflight | verify_candidate; laboratorio/OpenAPI/límites/destinos aprobados |
| dast / DAST OWASP ZAP | dynamic_preflight; objetivo efímero propio realmente accesible al job |
| fuzzing / REST API Fuzzing | dynamic_preflight; contrato exacto, compile/test antes de fuzz |
| hardening_report / Hardening Report | verify_candidate y todos los análisis anteriores; siempre consolidar sin borrar fallos |
| hardening_gate / Hardening Gate | hardening_report y estados reales de controles; aprobar política completa del contexto |
| publish_candidate / Publish Approved Candidate | verify_candidate, hardening_gate; solo develop confiable, OIDC Publisher y sin ejecución de código del artifact |

En implementaciones con runners distintos, un servidor iniciado por Dynamic Target Preflight no está mágicamente disponible en DAST/fuzzing. El preflight certifica la receta y el contrato seguros; cada consumidor crea su laboratorio aislado y verifica readiness/destino. No transferir secretos por job outputs. Puede consolidarse infraestructura dentro de cada job consumidor, sin fusionar los jobs visibles ni tocar A/B.

Para mantener **ci.yml sin AWS**, hardening se ejecuta como workflow independiente sobre push develop relevante, dispatch autorizado o calendario aprobado; no mediante workflow_run privilegiado ni llamada de deploy. Verifica el run CI del SHA/evento exacto con espera acotada y rechaza CI fallido/no verificable. No existe needs entre workflows; la unión de evidencia se valida por APIs/metadatos confiables. Los dos grafos visibles son coherentes, no una dependencia gráfica inexistente.

Solo push/dispatch desde develop confiable con candidata completa puede publicar. Schedule reanaliza artefactos existentes vigentes y no publica/despliega por sí mismo; pérdida/expiración del OCI requerido bloquea sin rebuild oculto. Los jobs de análisis no tienen AWS; solo publish_candidate recibe id-token y permisos ECR. Proteger el workflow/versiones de control y verificar autenticidad de todos los inputs antes de ese job.

CodeQL e imagen se exigen para promoción según spec §6; mutation/DAST/RESTler tienen calendario/aplicabilidad versionados, sin PASS cuando no corrieron. Si un control requerido no puede ejecutarse por licencia/SDK/target/feed, bloquear; no degradarlo a opcional silenciosamente. Cada reporte incluye los cinco estados y la matriz de controles requeridos por ese contexto. Coste y latencia se miden; no metas SLA inventadas.

## 5. Imagen, OIDC y candidata aprobada

Cadena única: Docker Build linux/amd64 → OCI íntegro con manifest digest/hash de transporte → hardening misma imagen → ECR mismos blobs/manifiesto → verificación digest remoto → registro aprobado. Usar [Enver.API/Dockerfile](../../Enver.API/Dockerfile) y contexto sellado/sin secretos; cambios de Dockerfile/SDK/producto requieren alcance autorizado, no reparación implícita de este gate.

PR no publica, hardening no construye otra imagen y deploy no compila. Un intento de build nuevo debe tener nueva identidad declarada; nunca afirmar que reconstrucción recupera un digest anterior. El proceso de copia al registry debe preservar media types/manifiesto; si no mantiene digest, bloquear en vez de aceptar equivalencia de config ID.

Registro de candidata en custodia confiable, no JSON editado en un PR: repo, develop, SHA integrado, revisiones de workflows, run IDs/intentos, SDK/config/base, OCI hash/manifest digest, resultados/hashes de evidencia, versión de política/fecha/vigencia. Consultar datos GitHub/ECR confiables y retener el artefacto anterior. Versiones/revocaciones de aprobación no mutan el mapeo de una candidata ya emitida.

OIDC requerido con aud/sub exactos. Repo/owner/environment/branch reales y formato con IDs si aplica se verifican en 105; environment subject no acredita develop por sí solo. Roles dedicados conforme spec §9: CI sin AWS, Publisher ECR, Deploy ECR-read+SSM-A-B+health-read, Runtime EC2 actual IMDSv2. No IAM/Terraform/secret-read global del deploy, claves permanentes, profile local enver-infra ni tokens entre jobs. Roles/ARN/configuración GitHub necesitan autorización independiente; no se crean ni prueban aquí.

## 6. Deployment DEV y compensación dentro del mismo run

### 6.1. Botón, control de ref y preflight

Solo deploy-dev.yml con nombre Deploy ENVER Dev y trigger workflow_dispatch. Inputs environment=development y run_smoke_tests booleano true por defecto; sin digest, comando, role/host/URL arbitrarios. Workflow disponible en default branch para descubrimiento; ejecutar desde develop confiable. No obligar a integrar la candidata en main para desplegar DEV; main sigue estable/entrega.

Validar actor/repo/ref/revisión → resolver automáticamente última candidata aprobada/vigente alcanzable de develop → comprobar CI/hardening exactos/estados/policy/evidencias → OIDC temporal y ECR read → congelar SHA/run/intento/digest → exclusión del run/revalidación → comprobar A/B/baseline y digest previo compatible retenido. Si el head actual no está aprobado, el resumen identifica claramente qué candidata anterior elegible se eligió; no usar un CI verde genérico. Ante cero candidatas válidas, rechazar sin tocar AWS runtime.

La identidad previa actual tiene evidencia de deployment manual, no aprobación CI nueva. Incorporarla como referencia de retorno exige validar/registrar su aprobación/compatibilidad sin inventar un run GitHub histórico. Sin digest anterior aprobado/usable, denegar antes de A. Mismo digest ya activo y validado puede ser no-op, con evidencia real y sin reinicios gratuitos.

### 6.2. Rolling y preservación

Consumir exactamente EC2 A/B, ECR, TG, ALB y CloudFront de spec §3. Comprobar primero servicio/contenedor/imagen/binding/configuración preservada. Actualizar referencia de imagen por mecanismo existente mediante SSM restringido/versionado; runtime obtiene permisos de pull por su rol actual, sin credenciales pipeline persistidas en EC2.

Por A: despacho único rastreado → materialización ordinaria/startup → readiness acotada tolerando carrera → digest/binding/health/target healthy. **Solo después B**, con el mismo contrato. No modificar estructura systemd/wrappers/overlay de OriginSecret, JWT, autoridad, cuotas, ALB/CloudFront o Terraform. Ningún initialize ni recreación de infraestructura.

Después de ambos: A/B mismo manifest aprobado, targets healthy y CloudFront HTTPS /health200/Healthy. Si se solicita smoke, flujo AUTH sintético seguro; run_smoke_tests=false solo omite AUTH y reporta NOT EXECUTED. No inventar readiness endpoint; liveness no prueba dependencias. Smoke no demuestra por sí solo rechazo distribuido post-logout.

### 6.3. Máquina de fallos y compensación

- Antes de cada mutación persistir metadatos no secretos, candidato/previo, fase, instance ID y CommandId/estado disponible; no guardar credenciales. No asumir fallo de llamada significa ausencia de efectos.
- Fallo A: parar avance, **B intacta**, reconciliar A y restaurar A al anterior aprobado si cambió; validar A/target. Si ya está anterior, no restart redundante. Original FAILED.
- Fallo B con A nueva: parar avance, reconciliar, **restaurar B y luego A si cambió**; validar ambos, targets y HTTPS CloudFront health. Original FAILED.
- Fallo en validación final/smoke solicitado tras ambos cambios: mismo retorno B→A, sin retries de POST que creen cuentas ni compensaciones de datos.
- Compensación como jobs/ramas condicionales del mismo run manual, ejecutables pese a needs fallido. No lanzar otro workflow, ni exigir segundo botón para esta compensación ya aprobada.
- Un intento forward y, si procede, un intento de retorno por nodo; consultas de estado acotadas. Esperar/reconciliar SSM en curso antes de otra orden; si no es seguro, rollback BLOCKED/FAIL con intervención requerida, no bucle de retries.
- Restauración B fallida no justifica mentir sobre éxito: intentar restauración A únicamente si el estado reconciliado lo permite; informar ambos outcomes y detenerse sin tocar seguridad.
- `deploymentOutcome=FAILED` es irreversible para el intento original; `rollbackOutcome=PASS/FAIL/NOT EXECUTED/BLOCKED` va separado. El summary/cleanup no neutraliza el exitcode del fallo.

El grupo enver-development-deploy y cancel-in-progress=false cubren el run entero y compensación; no es FIFO/cola durable ni garantiza que un runner muerto ejecute finally. Pérdida de runner/cancelación deja reconciliación obligatoria antes de nueva mutación, sin otro workflow automático. Coordinar mantenimiento externo y comandos SSM huérfanos. No ampliar rol Deploy con cambios TG/ALB/drenaje no autorizados.

## 7. Verificación, evidencias y alcance de aceptación

Gates definidos en [tasks.md §3](tasks.md#3-gates-formales). Cada capacidad debe tener negativos: CI ajeno/otro SHA, artifact alterado, digest sustituido, feed/control ausente, skipped/missing/timeout, ref no develop, PR con privilegios, rol fuera de alcance y fallo A/B/final/compensación. Pruebas de fallos reales solo en objetivo/entorno autorizado; los tests locales del orquestador no se presentan como AWS PASS.

Reportes siempre consolidan build/UT/IT/ST/runtime/contratos/SAST/dependencias/secretos/contenedor/IaC/mutation/DAST/fuzzing y, para CD, actor/candidata/digests/A-B/targets/HTTPS/smoke/compensación. Cinco estados, evidencias firmes por run y saneamiento antes de upload. Excepciones y retención/coste visibles; sin tokens/password/secretos/runtime JSON/state/plan en logs/artifacts.

Una ejecución operacional DEV puede pasar sus verificaciones aplicables sin cerrar aceptación distribuida integral. Si falta superficie protegida normal/atribución A-B, informar pendientes CD-009/AUTH-027/GATE-I-I; no atribuir rechazo del token a logout repetido, ni añadir endpoint nuevo. Ningún gate 004 cierra 001/002/003 retrospectivamente.

**Implementación posterior:** resolver DP operativos de spec §13 en las diez capacidades, sin reabrir decisiones finales ni provisionar recursos por iniciativa. Crear/configurar archivos y ejecutar CI, configurar OIDC/IAM, publicar ECR y despachar deployment son acciones diferentes que necesitan alcance explícito en el siguiente gate. Esta entrega se detiene con tres documentos actualizados y cero workflows, pruebas, llamadas AWS o deployments nuevos.
