# Tareas 004 — Diez capacidades completas de CI/CD

## 1. Control, catálogo vigente y límite de autorización

| Campo | Estado |
| --- | --- |
| Versión | 0.2.0 |
| Fuentes | [spec.md](spec.md) y [plan.md](plan.md), decisiones finales aprobadas |
| Catálogo | **10 tareas grandes: T-CICD-101–110** |
| Fases | Cinco, PHASE 1–5 según plan §3 |
| Estado de todas las tareas | **PENDIENTE / NO EJECUTADA** |
| Evidencias técnicas nuevas | Ninguna; referencias E(T-CICD-nnn) son futuras |
| Gates | Nueve, todos PENDIENTE / NO SATISFECHO |
| Próximo gate | **IMPLEMENT COMPLETE CI/CD PIPELINE**, sujeto a autorización posterior |

Este tasks.md es el único catálogo vigente de 004. Sustituye operativamente el task.md singular preexistente, que se conserva intacto como antecedente no normativo fuera del alcance de edición. Los IDs 001–030 no se reutilizan; 101–110 evitan aparentar continuidad de tareas/evidencias que cambiaron de alcance. No ejecutar el catálogo anterior ni sus reglas de rama/rollback sustituidas.

**Este gate únicamente actualiza spec.md, plan.md y tasks.md.** No crea workflows, scripts, tests o infra; no modifica GitHub/AWS/IAM, no ejecuta restore/build/tests/scans/Docker/Terraform/initialize/deploy ni hace stage/commit/push. La aprobación del diseño no inicia estas tareas automáticamente. Constituciones, documentos de gobierno y specs001/002/003 se conservan sin editar; la autorización de esta entrega se registra en spec §1.

## 2. Contrato de ejecución futura y evidencia

- Cada tarea entrega una capacidad funcional completa con pruebas positivas/negativas, dependencias y evidencia. No se descompone aquí en microtareas ni se considera terminada por producir un skeleton o un reporte vacío.
- El próximo gate debe autorizar archivos/componentes, comandos, entorno, presupuesto y permisos reales. Implementar workflows no concede por sí solo configurar IAM, publicar ECR, generar tráfico dinámico o despachar un deployment AWS.
- Decisiones ya aprobadas: develop integra/despliega DEV; main estable; tres workflows; OIDC obligatorio; build image once; deploy solo dispatch humano y compensación automática dentro del mismo run. No tratarlas como propuestas pendientes otra vez.
- DP-CICD-01–14 de spec §13 son datos/configuraciones/evidencias operativas a resolver con autorización, no permiso para inventar recursos o herramientas incompatibles.
- E(T-CICD-nnn) contendrá fuente de autorización, requisito/PLD/AC/gate, commit/revisión/run/intento, comandos/exitcodes/resultados reales, artifacts/digest cuando aplique, findings/estados/limitaciones y aprobación. Un mock local no acredita permisos AWS ni un deployment real.
- Estados de controles: PASS, FAIL, NOT APPLICABLE, NOT EXECUTED, BLOCKED. Un control obligatorio ausente/skip/cancelado/timeout/fallido o resultado desconocido impide aprobar. NOT APPLICABLE necesita alcance acreditado y no se cuenta como PASS.
- Resultados de suites no vacíos, cero failed/skipped internos y conteo dinámico; sin retries para esconder rojos. Preservar fallos originales, redacción previa al upload y retención aprobada, sin passwords/tokens/OriginSecret/runtime JSON/state/planes.
- Baseline AWS y configuración de spec §3 son invariantes, no recursos por recrear. A falta de permiso/capacidad, BLOCKED; no IAM simulation obligatoria ni creación de recursos para probar acceso.

## 3. Gates formales

Todos **PENDIENTE / NO SATISFECHO**. Un gate se cierra solo con evidencia y revisión; los prerrequisitos no autorizan ejecución. AC completos en spec §12.

| Gate | Criterio de aceptación de capacidad | Evidencia de tareas | Prerrequisitos de cierre | AC |
| --- | --- | --- | --- | --- |
| GATE-CI-A | Tres workflows/eventos/ramas y jobs visibles correctos, CI sin AWS, deploy solo dispatch humano | 101, 104, 107, 110 | Implementación autorizada | 01, 16 |
| GATE-CI-B | SDK exacto/restore/build Release reproducible y warnings efectivos | 101, 102, 110 | CI-A | 02 |
| GATE-CI-C | UT/IT/ST/runtime/contratos aplicables completos y aislados, resultados nativos íntegros/cleanup | 101, 102, 110 | CI-B | 03, 04 |
| GATE-CI-D | Semgrep/deps/audit/Gitleaks/lint/IaC/hardening según política, cinco estados/negativos y laboratorio seguro | 103, 104, 110 | CI-B/C para verificaciones dependientes | 05, 09 |
| GATE-CI-E | Build image once y manifest digest preservado OCI→scan→ECR; registro aprobado develop y cero PR push | 104, 106, 110 | CI-A–D y CD-A para publicación | 06 |
| GATE-CD-A | OIDC/roles mínimos/trust/contextos y separación CI/Publisher/Deploy/Runtime acreditados | 105, 106, 107, 110 | CI-A; IAM autorizado por separado | 07 |
| GATE-CD-B | Botón develop/DEV, candidata automática verificada/congelada, lock/preflight rechazos y baseline/retorno seguros | 106, 107, 109, 110 | CI-A–E, CD-A; capacidades rolling/compensación revisadas | 01, 08, 09, 13 |
| GATE-CD-C | Rolling A→B con readiness/targets, misma imagen, HTTPS health y smoke según input con límites de aceptación explícitos | 108, 109, 110 | CD-B; capacidad de compensación lista y ejecución AWS autorizada | 10, 11, 12 |
| GATE-CD-D | Fallos A/B/final y compensación mismo run acreditados, original FAILED, interrupción/concurrencia y evidencia saneada | 107–110 | CD-B y pruebas operativas aplicables autorizadas, incluida evidencia CD-C | 12–16 |

Números abreviados en tablas = T-CICD-nnn / E(T-CICD-nnn) y AC-CICD-nn. Un gate no necesita estar cerrado para poder realizar su propia prueba autorizada, pero sí todas las precondiciones de seguridad de esa operación. CI-E no dispara deployment. CD-C/CD-D no cierran por asociación AUTH-027, GATE-I-I de003 ni otros gates de001/002.

## 4. Catálogo de capacidades

### T-CICD-101 — CI foundation operativo y grafo rápido

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 1 — CI foundation.
- **Capacidad:** CI automático de ramas aprobadas con jobs visibles, SDK/restore/build Release/UT y agregación fail-closed; sin AWS.
- **Dependencias:** autorización técnica posterior; datos de repo/default branch/actores/runner y política de permisos de DP-01/02/03/04/14.
- **Archivos/componentes futuros:** `.github/workflows/ci.yml` y helpers de alcance/resultados estrictamente necesarios; global.json/proyectos/locks como fuentes existentes sin modificación implícita.
- **Entrega completa:** triggers PR develop/main y push develop/main, checkout del SHA correcto, Detect Changes y contratos de los doce jobs, Build & Test con SDK10.0.403/disable, restore locked/build Release/UT, PR Quality Gate que evalúa dependencias reales. Cambios globales invalidan controles afectados; docs-only no genera candidata; diff incierto ejecuta checks. CI no recibe OIDC AWS/secretos ni publica.
- **Aceptación:** grafo/condiciones revisados; build/UT del SHA con resultados completos; negativos de suite vacía, skip inesperado, cancelación, lock/SDK discrepantes y diff fallido no producen verde falso. No presentar jobs aún no implementados por otras capacidades como PASS; gate completo depende de102–104.
- **Trazabilidad:** CI-001–006/009/011; CD-001/004; SEC-006/009/011/012; REL-010/011; PLD-001–004/007/011/020; AC-01/02/03/07/16; GATE-CI-A/B/C.
- **Evidencia futura:** E(T-CICD-101), eventos/needs/permissions, SHA/SDK/restore/build/UT/TRX, estados y negativos saneados.

### T-CICD-102 — Integración segura, runtime y contrato real de API

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 1 — CI foundation.
- **Capacidad:** jobs Mongo / Redis / Dynamo Integration Validation, Runtime Tool Tests y Contract Tests con todas las suites aplicables y sin duplicación innecesaria.
- **Dependencias:**101; inventario real de proyectos/ejecutables y runner Docker disponible, DP-03/04/10/11 según alcance local.
- **Archivos/componentes futuros:** ci.yml, recolectores/reportes y pruebas de contrato/harness solo en alcance autorizado; fixtures/test projects existentes como referencia. No agregar endpoints al producto ni Pact.
- **Entrega completa:** IT/ST completas del SHA usando Mongo replica set, Redis noeviction, DynamoDB Local/Ryuk existentes, namespaces/secretos sintéticos y cleanup; inventariar tests de tools fuera de slnx y ejecutables no cubiertos por dotnet test. Acreditar binarios/SDK/config antes de no-build/no-restore. Verificar contractualmente register201 sin token, login200/Bearer/expiresIn900, logout204/cuerpo vacío, health200/texto, ProblemDetails/JSON estricto/no-store/challenge. Reutilizar evidencia contractual identificada del mismo run si evita repetir IT pesadas, sin aceptar solo un TRX presente.
- **Aceptación:** descubrimiento/ejecución íntegros, cero failed/skipped y tests no vacíos por suite; aislamiento/cleanup probado, ausencia de acceso real AWS/Atlas/Redis desplegado y sin alterar límites SUT. Log de credenciales sintéticas o fuga de canary falla. Falta de atribución distribuida/superficie normal se informa y no inventa rechazo post-logout.
- **Trazabilidad:** CI-003–011; SEC-006/010; CD-009; REL-010/012; PLD-003–005/013/017/020; AC-02/03/04/12/16; GATE-CI-B/C.
- **Evidencia futura:** E(T-CICD-102), resultados UT/IT/ST/runtime/contratos correspondientes, manifiesto de suites/binarios y recursos/cleanup/privacidad.

### T-CICD-103 — Fast DevSecOps gate e IaC sin privilegios

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 2 — DevSecOps gates.
- **Capacidad:** Semgrep SAST, Dependency Review, Secret Scan, Dockerfile / Container Lint e IaC Validation, con decisiones de gate verificables.
- **Dependencias:**101/102 para pruebas integradas; versiones/compatibilidad/licencias/coste/pinning y política procesable de DP-02/05/14.
- **Archivos/componentes futuros:** ci.yml, configuración/reglas Semgrep/Gitleaks/Hadolint/Trivy/dependencias y helpers revisados; infra/Dockerfile/producto no se reparan por iniciativa.
- **Entrega completa:** Semgrep rápido C#, Dependency Review del delta cuando disponible más NuGet audit directo/transitivo completo, Gitleaks sin valor en logs, Hadolint/contexto y Terraform fmt/init de validación sin backend/validate/Trivy IaC para infra existente. Sin credenciales AWS/state remoto/plan privilegiado/apply. Evaluar findings reales, no solo éxito de upload. Estados y aplicabilidad por rutas auditables.
- **Aceptación:** secretos y High/Critical bloqueantes según política, excepciones formales con vencimiento; error/scanner/feed/licencia/resultado requerido ausente bloquea. No convertir herramienta no ejecutada en PASS ni licencias ausentes en NOT APPLICABLE. Negativos controlados con datos sintéticos, sin secretos reales; CI sin AWS y sin continue-on-error que oculte fallo.
- **Trazabilidad:** CI-011/012; SEC-001–006/009–012; CD-012; REL-006/010/011; PLD-006/007/013/014/020; AC-05/07/09/15; GATE-CI-D.
- **Evidencia futura:** E(T-CICD-103), versiones/policy/reglas, reportes/exitcodes/feeds/alcance y matriz de bloqueos saneada.

### T-CICD-104 — Imagen única y hardening selectivo completo

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 2 — DevSecOps gates.
- **Capacidad:** Docker Build build-once más workflow hardening con los diez jobs visibles, análisis y gate de candidata; sin publicación AWS todavía.
- **Dependencias:**101–103; contratos de artefacto/custodia y política de DP-05/06/11/14; laboratorio seguro disponible antes de ejecutar ZAP/RESTler.
- **Archivos/componentes futuros:** ci.yml y `.github/workflows/hardening.yml`, helpers de OCI/identidad/gates y configuración CodeQL/Trivy/Stryker/ZAP/RESTler en alcance posterior autorizado.
- **Entrega completa:** una imagen linux/amd64 por candidata, OCI/manifest digest/hash de transporte; Verify Candidate enlaza CI exacto terminado. Container Vulnerability Scan, CodeQL SAST, Mutation Testing, Dynamic Target Preflight, DAST OWASP ZAP y REST API Fuzzing según política; Hardening Report y Hardening Gate conservan todos los estados. Definir contrato de Publish Approved Candidate para106, sin privilegios en análisis. Stryker sobre lógica pura/validadores con calendario/umbral medidos. ZAP/RESTler en servidor HTTP real desechable, destino allowlist y presupuestos; validar OpenAPI contra DTOs/respuestas manuales, compile/test antes de fuzz. Cada runner consumidor tiene laboratorio propio, no servidor supuesto de otro job.
- **Aceptación:** misma imagen entre build y scan, cero rebuild en hardening y cero PR push; CodeQL/imagen requeridos para promoción con compatibilidad acreditada. Frecuencias selectivas y NOT EXECUTED/BLOCKED explícitos para controles no corridos; informe generado no produce aprobación automática. Nunca tráfico ZAP/RESTler a CloudFront/ALB/A-B compartidos. Artefacto perdido/alterado, falta de evidencia o controles requeridos no ejecutados bloquean, sin reemplazo por reconstrucción.
- **Trazabilidad:** CI-002/010/011; SEC-002–006/009–012; REL-001–005/010–012; PLD-001/006–009/013/020; AC-01/05/06/07/15/16; GATE-CI-A/D/E.
- **Evidencia futura:** E(T-CICD-104), Docker Build/run/intento/OCI/manifiesto, ejecución/no ejecución motivada de controles, límites/cobertura dinámica y negativos de gates.

### T-CICD-105 — Identidad OIDC y separación efectiva de roles

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 3 — OIDC + ECR promotion.
- **Capacidad:** contrato e integración GitHub→OIDC→roles temporales mínimos, con contextos/denegaciones acreditados sin claves permanentes.
- **Dependencias:**101/103/104; DP-01/02/07/12. Configuración IAM/GitHub requiere autorización propia; este catálogo no la provisiona por sí solo.
- **Archivos/componentes futuros:** configuración de permisos/contextos de hardening.yml y deploy-dev.yml, referencias de roles/environment; trust/roles AWS solo bajo alcance IAM expresamente autorizado, sin Terraform apply en application pipeline.
- **Entrega completa:** CI sin AWS; Publisher solo ECR; Deploy ECR read+SSM A/B+lecturas target/estado necesarias; runtime conserva rol enver-dev-api-runtime/IMDSv2. Aud/sub/emisor reales y formato IDs si aplica, environment development con restricciones develop/ref/revisión. Cada job OIDC obtiene sesión propia, sin transferir credenciales. SSM restringido/versionado, sin parámetros shell/host/rol arbitrarios ni secretos leídos por deploy.
- **Aceptación:** evidencia real autorizada de identidad/scopes; rechazos PR/fork/ref/repo no elegibles, Publisher sin deploy y Deploy sin IAM/Terraform/bootstrap/secret-read global. No inferir PASS de IAM a partir de un diseño o mock; no crear recursos de prueba ni exigir simulación. Si no hay provisión/permiso, registrar BLOCKED sin fallback a claves permanentes.
- **Trazabilidad:** SEC-006–012; CD-004/005/012; REL-006/010/011; PLD-007/010/011/013/020; AC-07/08/09/15; GATE-CD-A.
- **Evidencia futura:** E(T-CICD-105), matriz acción/recurso/contexto/trust real y resultados saneados, sin tokens OIDC/STS.

### T-CICD-106 — Promoción ECR del mismo artefacto y registro aprobado

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 3 — OIDC + ECR promotion.
- **Capacidad:** Publish Approved Candidate, exclusivamente develop confiable, con custodia verificable y mismo manifest digest.
- **Dependencias:**103/104/105; ECR existente y autorización específica de publicación; DP-05/06/07/12/13.
- **Archivos/componentes futuros:** hardening.yml, publicador/recolectores de evidencia y esquema de registro; ECR existente de spec §3, sin recrearlo.
- **Entrega completa:** verificar CI/hardening exactos del commit integrado develop y los controles requeridos; consumir OCI sin ejecutar código del artifact; copiar mismos blobs/manifiesto a ECR, verificar digest remoto y emitir registro repo/branch/SHA/workflows/runs/intentos/policy/evidencias/digest/vigencia. Retener candidato previo; rechazar sustitución de etiquetas/reruns ambiguos. Schedules reanalizan, no publican automáticamente ni despliegan.
- **Aceptación:** una imagen construida, analizada y publicada con manifest idéntico; diferencia de media type/manifiesto bloquea en vez de aceptar config ID. PR/main/feature sin contexto DEV elegible no publican. Ninguna aprobación ECR invoca deploy. Rechazar CI ajeno, cancelado, incompleto, artifacts/manifiestos alterados y política caducada. Imagen manual existente no obtiene CI GREEN retrospectivo.
- **Trazabilidad:** CI-002/011; SEC-004–012; REL-001–006/008/010/011; CD-003; PLD-002/007–010/012/013/020; AC-06/07/08/14/15; GATE-CI-E/CD-A/CD-B.
- **Evidencia futura:** E(T-CICD-106), run confiable, aprobación y manifest digest local/remoto iguales, pruebas de no publicación PR y de no auto-deployment.

### T-CICD-107 — Botón DEV manual, candidata automática y preflight seguro

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 4 — Manual rolling deployment.
- **Capacidad:** deploy-dev.yml con botón manual y selección/preflight/lock que rechaza antes de mutar cualquier candidata o baseline inseguro.
- **Dependencias:**101–106 aceptadas en su alcance; DP-01/02/06–09/13/14. La activación cloud depende además de108/109 listas y autorización de110.
- **Archivos/componentes futuros:** `.github/workflows/deploy-dev.yml`, helpers de elegibilidad/registro/SSM en alcance autorizado y environment development; sin cuarto workflow.
- **Entrega completa:** nombre Deploy ENVER Dev y solo workflow_dispatch; disponibilidad en default branch y ejecución desde develop confiable. Inputs environment=development y run_smoke_tests true por defecto; no digest manual ni destinos/comandos libres. Elegir última candidata aprobada/vigente alcanzable develop, comprobar CI/hardening exactos/registro/ECR, congelar selección, exclusión enver-development-deploy/cancel-in-progress=false y revalidación. Guardar por nodo digest previo aprobado/compatible/retendido y estado inicial homogéneo; comprobar invariantes de baseline y return path antes de A. Lecturas AWS temporales OIDC no equivalen a mutación autorizada.
- **Aceptación:** botón/ref/actor/contexto verificables; push/PR/workflow_run/schedule/release/workflow_call jamás despliegan ni llaman dispatch. Ausencia de candidata/previo seguro, API incierta, SHA/ref/policy incorrectos o mutación remota no reconciliada bloquean sin tocar A/B. Default branch main no obliga a desplegar main; summary identifica si candidata elegida es anterior al head develop. Preflight prepara metadatos de recuperación, no secretos ni state Terraform.
- **Trazabilidad:** CI-002/011; CD-001–006/010/012; SEC-005–012; REL-002/004–012; PLD-001/002/007/009–015/019/020; AC-01/07/08/09/13/14/15/16; GATE-CD-A/B/D.
- **Evidencia futura:** E(T-CICD-107), dispatch/ref/candidata y negativos de elegibilidad/concurrencia con resultado nativo, primero sin mutaciones reales.

### T-CICD-108 — Rolling A→B por digest con preservación runtime

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 4 — Manual rolling deployment.
- **Capacidad:** orquestación secuencial del despliegue con hooks obligatorios de verificación/compensación y registro de mutaciones, sin cambiar infraestructura.
- **Dependencias:**107; contrato de verificación/compensación de109 diseñado y acordado. No habilitar un run mutador hasta109 integrada/verificada y110 específicamente autorizada.
- **Archivos/componentes futuros:** deploy-dev.yml y ejecutor SSM restringido/handlers de estado; referencia activa por mecanismo existente. Program.cs/Dockerfile/systemd/overlay/secretos/IaC no se alteran implícitamente.
- **Entrega completa:** A primero, despacho rastreado y cambio de referencia al digest aprobado, pull de esa imagen por runtime EC2 y restart/materialización ordinaria; polling bounded de servicio/container/digest/binding/health/target. B solo después de A PASS y target healthy. Preservar OriginSecret/JWT/autoridad/generación cuotas/AllowLocalHttp=false, ALB/CloudFront, unidad/wrappers/overlay/journald/montaje/permisos existentes. Registrar CommandId y estado antes/después, congelar imagen y activar señal de compensación ante fallo real, no seguir B por simple ACK.
- **Aceptación:** en pruebas seguras de orquestador, ninguna llamada mutadora B antes de A validada; fallo A nunca toca B; versiones/destinos fuera del baseline se rechazan. No rebuild/repush de deployment, initialize, Terraform apply ni recreación de recursos. Lecturas acotadas toleran carrera inicial sin segundo restart forward. Timeout no autoriza retry ciego ni solapar órdenes. Compensación/resultado final integrados en109 antes de operación AWS.
- **Trazabilidad:** CD-006/007/010–012; SEC-008/010; REL-002/004–010; PLD-008/013/015/016/018/019/020; AC-06/10/13/14/15; GATE-CD-B/C/D.
- **Evidencia futura:** E(T-CICD-108), máquina de estados/contratos/readiness e invariantes verificadas en entorno autorizado; pruebas AWS reales solo cuando se autoricen, sin presentarlas anticipadamente como PASS.

### T-CICD-109 — Verificación HTTPS y rollback compensatorio integrado

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 5 — Verification + rollback.
- **Capacidad:** verificaciones finales y retorno seguro dentro del MISMO run manual, con fallo original irreversible.
- **Dependencias:**107/108; imagen previa aprobada/compatible y contrato runtime; DP-09/10/11/13/14; objetivos/credenciales sintéticas autorizados antes de tráfico público.
- **Archivos/componentes futuros:** deploy-dev.yml, helpers de readiness/health/smoke/compensación/summary y pruebas seguras del orquestador en alcance posterior.
- **Entrega completa:** CloudFront HTTPS /health200 obligatorio; targets/digests A/B; smoke AUTH según input, sin body/token/password en evidencias ni retries POST. Fallo A: no B, reconciliar/restaurar/validar solo A. Fallo B: detener avance, reconciliar y restaurar B primero, después A si quedó nueva; comprobar A/B/targets/CloudFront health. Fallo final/smoke solicitado: mismo retorno B→A. No restaurar datos/secretos/Redis/autoridad/cuotas, no lanzar otro workflow ni pedir segundo botón para compensar.
- **Aceptación:** flujo de fallo no queda skipped por needs; presupuesto de una activación y un retorno por nodo cuando necesario, evitando restart si ya está anterior. Deployment original FAILED aun con rollback PASS. Si retorno parcial/fallido/incierto, reportarlo y continuar únicamente pasos seguros tras reconciliación; sin bucle de retries ni éxito falso. Runner perdido no promete rollback ejecutado: recuperación BLOCKED/pendiente y próxima operación reconcilia. Smoke false = AUTH NOT EXECUTED, nunca omite health/targets/readiness. Logout204 repetido no prueba rechazo distribuido.
- **Trazabilidad:** CD-006–012; SEC-010/012; REL-002/004–010/012; PLD-013/015–020; AC-10–16; GATE-CD-B/C/D.
- **Evidencia futura:** E(T-CICD-109), escenarios A/B/final/rollback parcial/interrupción, outcomes separados y prueba de sanitización; cloud solo con autorización delimitada.

### T-CICD-110 — Verificación completa, negativos y evidencia académica

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** PHASE 5 — Verification + rollback.
- **Capacidad:** demostrar end-to-end CI→hardening→misma imagen ECR→deployment humano→verificación/compensación, con revisión final de gates y límites.
- **Dependencias:**101–109 implementadas/revisadas, datos operativos resueltos para cada operación; autorización explícita de integración Git/run CI, IAM/OIDC, publicación, entorno dinámico y operaciones AWS/fallos/smoke/rollback según lo que efectivamente se vaya a ejecutar. No se concede ahora ninguna de ellas.
- **Archivos/componentes futuros:** evidencias/reportes/índice y referencias de gate autorizadas; máximo los tres workflows definidos. Cambios a otros specs/gobierno requieren permiso aparte.
- **Entrega completa:** ejecutar y verificar runs de PR y develop reales del SHA, UT/IT/ST/runtime/contratos/scans/IaC/imagen; validar digest idéntico hasta ECR/deploy. Negativos de missing/skipped/timeout/findings/policy/artifact/ref/CI ajeno/credenciales PR y no auto-deploy. Persona usa botón; observar A/B y health, smoke solicitado, fallo A sin B, fallo B con retorno B→A, fallo final/compensación/interrupción/concurrencia en entorno expresamente autorizado. Consolidar Hardening Report/deployment summary, evidencia de privacidad/coste/retención y trazabilidad requisito→PLD→tarea→E→AC→gate.
- **Aceptación:** gates solo con evidencia real aceptada; ningún contador histórico ni YAML existente equivale a PASS. Reportar ejecución/no aplicabilidad/bloqueos sin borrar rojos. El retorno exitoso mantiene deployment FAILED; ningún automático inicia otro workflow o infraestructura. Distinguir éxito operacional DEV de aceptación distribuida pendiente de002/003; no cerrar AUTH-027 integralmente. Fallos reales no se inyectan en DEV compartido sin autorización específica.
- **Trazabilidad:** CI-001–012; SEC-001–012; CD-001–012; REL-001–012; PLD-CICD-001–020; AC-CICD-01–16; todos los gates, solo en su alcance acreditado.
- **Evidencia futura:** E(T-CICD-110), runs/CommandId/resultados/digests/actor/targets/TLS/smoke/rollback/reportes saneados y decisión del responsable, con pendientes expresos.

## 5. Cobertura documental de los 48 requisitos

Referencias abreviadas a tareas vigentes;110 es verificación transversal y no reemplaza la capacidad concreta. Cobertura es trazabilidad de diseño, no cumplimiento técnico.

| Requisito | Capacidades principales | Verificación |
| --- | --- | --- |
| CI-001 | 101, 107 | 110 |
| CI-002 | 101, 104, 106, 107 | 110 |
| CI-003 | 101, 102 | 110 |
| CI-004 | 101, 102 | 110 |
| CI-005 | 101, 102 | 110 |
| CI-006 | 101, 102 | 110 |
| CI-007 | 102 | 110 |
| CI-008 | 102 | 110 |
| CI-009 | 101, 102 | 110 |
| CI-010 | 102, 104 | 110 |
| CI-011 | 101, 102, 103, 104, 107 | 110 |
| CI-012 | 103 | 110 |
| SEC-001 | 103 | 110 |
| SEC-002 | 103, 104 | 110 |
| SEC-003 | 103, 104 | 110 |
| SEC-004 | 103, 104, 106 | 110 |
| SEC-005 | 103, 104, 106, 107 | 110 |
| SEC-006 | 101, 103, 104, 105, 106, 107 | 110 |
| SEC-007 | 105, 106, 107 | 110 |
| SEC-008 | 105, 106, 107, 108 | 110 |
| SEC-009 | 101, 104, 105, 106, 107 | 110 |
| SEC-010 | 102, 103, 104, 105, 106, 107, 108, 109 | 110 |
| SEC-011 | 101, 103, 104, 105, 106, 107 | 110 |
| SEC-012 | 101, 103, 104, 105, 106, 107, 109 | 110 |
| REL-001 | 104 | 110 |
| REL-002 | 104, 106, 107, 108, 109 | 110 |
| REL-003 | 103, 104, 106 | 110 |
| REL-004 | 104, 106, 107, 108, 109 | 110 |
| REL-005 | 104, 106, 107, 108, 109 | 110 |
| REL-006 | 103, 105, 106, 107, 108, 109 | 110 |
| REL-007 | 107, 108, 109 | 110 |
| REL-008 | 106, 107, 109 | 110 |
| REL-009 | 107, 108, 109 | 110 |
| REL-010 | 101, 102, 103, 104, 105, 106, 107, 108, 109 | 110 |
| REL-011 | 101, 103, 104, 105, 106 | 110 |
| REL-012 | 102, 104, 107, 109 | 110 |
| CD-001 | 101, 107 | 110 |
| CD-002 | 107 | 110 |
| CD-003 | 106, 107 | 110 |
| CD-004 | 101, 105, 107 | 110 |
| CD-005 | 105, 107 | 110 |
| CD-006 | 107, 108, 109 | 110 |
| CD-007 | 108, 109 | 110 |
| CD-008 | 109 | 110 |
| CD-009 | 102, 109 | 110 |
| CD-010 | 107, 108, 109 | 110 |
| CD-011 | 108, 109 | 110 |
| CD-012 | 103, 105, 107, 108, 109 | 110 |

Las veinte PLD enlazan tareas en plan §2; los dieciséis AC enlazan requisitos/gates en spec §12 y tareas en §4. **48/48 requisitos con cobertura documental,20/20 decisiones enlazadas,16/16 AC enlazados,5 fases,10 tareas no ejecutadas y9 gates pendientes.** No existe evidencia de implementación 004 en esta entrega.

## 6. Cierre de este gate

SPEC UPDATED / PLAN UPDATED / TASKS UPDATED. DEPLOY BRANCH=develop; MAIN ROLE=stable; AWS DEPLOY=MANUAL workflow_dispatch; ROLLBACK=AUTOMATIC COMPENSATING WITHIN MANUAL DEPLOYMENT; WORKFLOWS=3; OIDC=REQUIRED; BUILD STRATEGY=BUILD ONCE / DEPLOY SAME DIGEST; AWS MODIFIED=NO.

**NEXT GATE=IMPLEMENT COMPLETE CI/CD PIPELINE.** Este catálogo no lo inicia: STOP tras la actualización de los tres Markdown y comprobación documental de coherencia/enlaces. Sin workflows, AWS/IAM, pruebas, deployment, initialize, stage, commit o push nuevos.
