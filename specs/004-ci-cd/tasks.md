# Tareas 004 — Diez capacidades completas de CI/CD

## 1. Control, catálogo vigente y límite de autorización

| Campo | Estado |
| --- | --- |
| Versión | 0.4.13 |
| Fuentes | [spec.md](spec.md) y [plan.md](plan.md), decisiones finales aprobadas |
| Catálogo | **10 tareas grandes: T-CICD-101–110** |
| Fases | Cinco, PHASE 1–5 según plan §3 |
| Estado de tareas | IMPLEMENTED en alcance local / AWS IAM CONFIGURED / TEMP BOOTSTRAP IAM PERMISSIONS REMOVED / GITHUB ENVIRONMENT Y OIDC SUBJECT CONFIGURED / RUNTIME VALIDATION PENDING según §4; historial §§7–20. Ninguna COMPLETE integralmente |
| Evidencias técnicas nuevas | §20: run44847408590FAIL, Build844/844-LinuxRuntime-IaC-DependencyReviewPASS. WindowsCRLF reproduce55→54PASS; lectura normalizada56/56PASS LF/CRLF. Diagnóstico Node acotado/fixtures sintéticos aislados. Mongo reproducePASS local Windows/Linux, causa GitHub desconocida; únicamente diagnóstico del contenedor fallido, no fix acreditado. Publicación diagnóstica incremental expresamente aprobada, sujeta a validación |
| Gates | Nueve, todos PENDIENTE / NO SATISFECHO |
| Próximo gate | **FIX REMAINING ACCREDITED FAILURE**: observar CI automático del incremento diagnóstico autorizado; si todos los controles requeridos pasan, VALIDATE ENVER CI GREEN. Sin AWS/deploy/initialize/merge |

Este tasks.md es el único catálogo vigente de 004. Sustituye operativamente el task.md singular preexistente, que se conserva intacto como antecedente no normativo fuera del alcance de edición. Los IDs 001–030 no se reutilizan; 101–110 evitan aparentar continuidad de tareas/evidencias que cambiaron de alcance. No ejecutar el catálogo anterior ni sus reglas de rama/rollback sustituidas.

**Antecedente documental:** el gate de diseño únicamente actualizó spec.md, plan.md y tasks.md, sin ejecutar el catálogo. **Autorizaciones posteriores:** IMPLEMENT COMPLETE CI/CD PIPELINE y CONTINUE IMPLEMENTATION FROM CURRENT STATE, seguidas de **FIX PIPELINE — RESOLVE LOCAL BLOCKERS**, que autoriza corregir harness/pruebas/IaC fuente, revisar workflows, validar localmente y actualizar estados. Durante FIX PIPELINE el responsable aprueba dos excepciones de egreso precisas y el arranque local de Docker Desktop para recuperar validación; no AWS/OIDC, deploy, initialize, stage/commit/push/PR. Registros §§7–8; spec/plan, constituciones y specs001/002/003 se conservan.

## 2. Contrato de ejecución y evidencia

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

- **Estado:** IMPLEMENTED localmente; RUNTIME VALIDATION PENDING en GitHub. YAML/Actionlint y tests de agregación PASS; gate conserva fail-closed incluso ante descarga fallida. No COMPLETE.
- **Fase:** PHASE 1 — CI foundation.
- **Capacidad:** CI automático de ramas aprobadas con jobs visibles, SDK/restore/build Release/UT y agregación fail-closed; sin AWS.
- **Dependencias:** autorización técnica posterior; datos de repo/default branch/actores/runner y política de permisos de DP-01/02/03/04/14.
- **Archivos/componentes futuros:** `.github/workflows/ci.yml` y helpers de alcance/resultados estrictamente necesarios; global.json/proyectos/locks como fuentes existentes sin modificación implícita.
- **Entrega completa:** triggers PR develop/main y push develop/main, checkout del SHA correcto, Detect Changes y contratos de los doce jobs, Build & Test con SDK10.0.403/disable, restore locked/build Release/UT, PR Quality Gate que evalúa dependencias reales. Cambios globales invalidan controles afectados; docs-only no genera candidata; diff incierto ejecuta checks. CI no recibe OIDC AWS/secretos ni publica.
- **Aceptación:** grafo/condiciones revisados; build/UT del SHA con resultados completos; negativos de suite vacía, skip inesperado, cancelación, lock/SDK discrepantes y diff fallido no producen verde falso. No presentar jobs aún no implementados por otras capacidades como PASS; gate completo depende de102–104.
- **Trazabilidad:** CI-001–006/009/011; CD-001/004; SEC-006/009/011/012; REL-010/011; PLD-001–004/007/011/020; AC-01/02/03/07/16; GATE-CI-A/B/C.
- **Evidencia futura:** E(T-CICD-101), eventos/needs/permissions, SHA/SDK/restore/build/UT/TRX, estados y negativos saneados.

### T-CICD-102 — Integración segura, runtime y contrato real de API

- **Estado:** IMPLEMENTED; validación local aplicable PASS / RUNTIME VALIDATION PENDING en GitHub. UT844/844, cinco suites runtime portables, dirigidas IT13/13 y ST6/6 e IT completas311/311 PASS conservadas. §11 resuelve el único ST por presupuesto sintético probado, solo tests/harness: objetivo1/1, LoginSecurity62/62, **ST completa223/223**, regresión IT Login25/25 PASS;0failed/skipped finales. Precheck histórico separado/NOT EXECUTED, hashes intactos. Fallos originales y reproducción diagnóstica preservados, sin retries ciegos.
- **Fase:** PHASE 1 — CI foundation.
- **Capacidad:** jobs Mongo / Redis / Dynamo Integration Validation, Runtime Tool Tests y Contract Tests con todas las suites aplicables y sin duplicación innecesaria.
- **Dependencias:**101; inventario real de proyectos/ejecutables y runner Docker disponible, DP-03/04/10/11 según alcance local.
- **Archivos/componentes futuros:** ci.yml, recolectores/reportes y pruebas de contrato/harness solo en alcance autorizado; fixtures/test projects existentes como referencia. No agregar endpoints al producto ni Pact.
- **Entrega completa:** IT/ST completas del SHA usando Mongo replica set, Redis noeviction, DynamoDB Local/Ryuk existentes, namespaces/secretos sintéticos y cleanup; inventariar tests de tools fuera de slnx y ejecutables no cubiertos por dotnet test. Acreditar binarios/SDK/config antes de no-build/no-restore. Verificar contractualmente register201 sin token, login200/Bearer/expiresIn900, logout204/cuerpo vacío, health200/texto, ProblemDetails/JSON estricto/no-store/challenge. Reutilizar evidencia contractual identificada del mismo run si evita repetir IT pesadas, sin aceptar solo un TRX presente.
- **Aceptación:** descubrimiento/ejecución íntegros, cero failed/skipped y tests no vacíos por suite; aislamiento/cleanup probado, ausencia de acceso real AWS/Atlas/Redis desplegado y sin alterar límites SUT. Log de credenciales sintéticas o fuga de canary falla. Falta de atribución distribuida/superficie normal se informa y no inventa rechazo post-logout.
- **Trazabilidad:** CI-003–011; SEC-006/010; CD-009; REL-010/012; PLD-003–005/013/017/020; AC-02/03/04/12/16; GATE-CI-B/C.
- **Evidencia futura:** E(T-CICD-102), resultados UT/IT/ST/runtime/contratos correspondientes, manifiesto de suites/binarios y recursos/cleanup/privacidad.

### T-CICD-103 — Fast DevSecOps gate e IaC sin privilegios

- **Estado:** IMPLEMENTED; RUNTIME VALIDATION PENDING en GitHub y CONFIGURATION PENDING para Dependency Review. Reanudación §10: Terraform fmt/validate y Trivy config PASS con dos excepciones exactas aprobadas hasta2026-12-19 y4LOW/MEDIUM visibles; sin init/descargas de provider. Semgrep137archivos/0findings y Hadolint0findings PASS. NuGet y secret scan nativo Windows previos PASS; el recibo final registra el escaneo del working tree y sus hashes después de estos cambios documentales. Los intentos fallidos de §§8–9 se conservan. Sin AWS/apply.
- **Fase:** PHASE 2 — DevSecOps gates.
- **Capacidad:** Semgrep SAST, Dependency Review, Secret Scan, Dockerfile / Container Lint e IaC Validation, con decisiones de gate verificables.
- **Dependencias:**101/102 para pruebas integradas; versiones/compatibilidad/licencias/coste/pinning y política procesable de DP-02/05/14.
- **Archivos/componentes futuros:** ci.yml, configuración/reglas Semgrep/Gitleaks/Hadolint/Trivy/dependencias y helpers revisados; infra/Dockerfile/producto no se reparan por iniciativa.
- **Entrega completa:** Semgrep rápido C#, Dependency Review del delta cuando disponible más NuGet audit directo/transitivo completo, Gitleaks sin valor en logs, Hadolint/contexto y Terraform fmt/init de validación sin backend/validate/Trivy IaC para infra existente. Sin credenciales AWS/state remoto/plan privilegiado/apply. Evaluar findings reales, no solo éxito de upload. Estados y aplicabilidad por rutas auditables.
- **Aceptación:** secretos y High/Critical bloqueantes según política, excepciones formales con vencimiento; error/scanner/feed/licencia/resultado requerido ausente bloquea. No convertir herramienta no ejecutada en PASS ni licencias ausentes en NOT APPLICABLE. Negativos controlados con datos sintéticos, sin secretos reales; CI sin AWS y sin continue-on-error que oculte fallo.
- **Trazabilidad:** CI-011/012; SEC-001–006/009–012; CD-012; REL-006/010/011; PLD-006/007/013/014/020; AC-05/07/09/15; GATE-CI-D.
- **Evidencia futura:** E(T-CICD-103), versiones/policy/reglas, reportes/exitcodes/feeds/alcance y matriz de bloqueos saneada.

### T-CICD-104 — Imagen única y hardening selectivo completo

- **Estado:** IMPLEMENTED según política selectiva; RUNTIME VALIDATION PENDING. Diez jobs y custodia/gates preparados; imagen real/CodeQL/Stryker no acreditados. ZAP/RESTler NOT EXECUTED o BLOCKED sin laboratorio/OpenAPI; solicitud requerida bloquea. El gate rechaza estados desconocidos/fallos incluso en opcionales, sin PASS ficticio.
- **Fase:** PHASE 2 — DevSecOps gates.
- **Capacidad:** Docker Build build-once más workflow hardening con los diez jobs visibles, análisis y gate de candidata; sin publicación AWS todavía.
- **Dependencias:**101–103; contratos de artefacto/custodia y política de DP-05/06/11/14; laboratorio seguro disponible antes de ejecutar ZAP/RESTler.
- **Archivos/componentes futuros:** ci.yml y `.github/workflows/hardening.yml`, helpers de OCI/identidad/gates y configuración CodeQL/Trivy/Stryker/ZAP/RESTler en alcance posterior autorizado.
- **Entrega completa:** una imagen linux/amd64 por candidata, OCI/manifest digest/hash de transporte; Verify Candidate enlaza CI exacto terminado. Container Vulnerability Scan, CodeQL SAST, Mutation Testing, Dynamic Target Preflight, DAST OWASP ZAP y REST API Fuzzing según política; Hardening Report y Hardening Gate conservan todos los estados. Definir contrato de Publish Approved Candidate para106, sin privilegios en análisis. Stryker sobre lógica pura/validadores con calendario/umbral medidos. ZAP/RESTler en servidor HTTP real desechable, destino allowlist y presupuestos; validar OpenAPI contra DTOs/respuestas manuales, compile/test antes de fuzz. Cada runner consumidor tiene laboratorio propio, no servidor supuesto de otro job.
- **Aceptación:** misma imagen entre build y scan, cero rebuild en hardening y cero PR push; CodeQL/imagen requeridos para promoción con compatibilidad acreditada. Frecuencias selectivas y NOT EXECUTED/BLOCKED explícitos para controles no corridos; informe generado no produce aprobación automática. Nunca tráfico ZAP/RESTler a CloudFront/ALB/A-B compartidos. Artefacto perdido/alterado, falta de evidencia o controles requeridos no ejecutados bloquean, sin reemplazo por reconstrucción.
- **Trazabilidad:** CI-002/010/011; SEC-002–006/009–012; REL-001–005/010–012; PLD-001/006–009/013/020; AC-01/05/06/07/15/16; GATE-CI-A/D/E.
- **Evidencia futura:** E(T-CICD-104), Docker Build/run/intento/OCI/manifiesto, ejecución/no ejecución motivada de controles, límites/cobertura dinámica y negativos de gates.

### T-CICD-105 — Identidad OIDC y separación efectiva de roles

- **Estado:** AWS IAM CONFIGURED/read-back previo PASS (§12), bootstrap REMOVED/verificado (§14); GitHub development/subject personalizado CONFIGURED/read-back PASS (§§15–16). RUNTIME VALIDATION PENDING: GitHub→STS PENDING UNTIL GITHUB EXECUTION tras publicar, denegaciones reales no probadas y documento SSM pendiente. ARNs reales y permisos OIDC de workflows verificados localmente; sin claves AWS estáticas. No COMPLETE ni GATE-CD-A satisfecho.
- **Fase:** PHASE 3 — OIDC + ECR promotion.
- **Capacidad:** contrato e integración GitHub→OIDC→roles temporales mínimos, con contextos/denegaciones acreditados sin claves permanentes.
- **Dependencias:**101/103/104; DP-01/02/07/12. Configuración IAM/GitHub requiere autorización propia; este catálogo no la provisiona por sí solo.
- **Archivos/componentes futuros:** configuración de permisos/contextos de hardening.yml y deploy-dev.yml, referencias de roles/environment; trust/roles AWS solo bajo alcance IAM expresamente autorizado, sin Terraform apply en application pipeline.
- **Entrega completa:** CI sin AWS; Publisher solo ECR; Deploy ECR read+SSM A/B+lecturas target/estado necesarias; runtime conserva rol enver-dev-api-runtime/IMDSv2. Aud/sub/emisor reales y formato IDs si aplica, environment development con restricciones develop/ref/revisión. Cada job OIDC obtiene sesión propia, sin transferir credenciales. SSM restringido/versionado, sin parámetros shell/host/rol arbitrarios ni secretos leídos por deploy.
- **Aceptación:** evidencia real autorizada de identidad/scopes; rechazos PR/fork/ref/repo no elegibles, Publisher sin deploy y Deploy sin IAM/Terraform/bootstrap/secret-read global. No inferir PASS de IAM a partir de un diseño o mock; no crear recursos de prueba ni exigir simulación. Si no hay provisión/permiso, registrar BLOCKED sin fallback a claves permanentes.
- **Trazabilidad:** SEC-006–012; CD-004/005/012; REL-006/010/011; PLD-007/010/011/013/020; AC-07/08/09/15; GATE-CD-A.
- **Evidencia futura:** E(T-CICD-105), matriz acción/recurso/contexto/trust real y resultados saneados, sin tokens OIDC/STS.

### T-CICD-106 — Promoción ECR del mismo artefacto y registro aprobado

- **Estado:** IMPLEMENTED en código; CONFIGURATION PENDING y RUNTIME VALIDATION PENDING. Sin publicación ECR ni prueba OCI→ECR; flags cerrados por defecto.
- **Fase:** PHASE 3 — OIDC + ECR promotion.
- **Capacidad:** Publish Approved Candidate, exclusivamente develop confiable, con custodia verificable y mismo manifest digest.
- **Dependencias:**103/104/105; ECR existente y autorización específica de publicación; DP-05/06/07/12/13.
- **Archivos/componentes futuros:** hardening.yml, publicador/recolectores de evidencia y esquema de registro; ECR existente de spec §3, sin recrearlo.
- **Entrega completa:** verificar CI/hardening exactos del commit integrado develop y los controles requeridos; consumir OCI sin ejecutar código del artifact; copiar mismos blobs/manifiesto a ECR, verificar digest remoto y emitir registro repo/branch/SHA/workflows/runs/intentos/policy/evidencias/digest/vigencia. Retener candidato previo; rechazar sustitución de etiquetas/reruns ambiguos. Schedules reanalizan, no publican automáticamente ni despliegan.
- **Aceptación:** una imagen construida, analizada y publicada con manifest idéntico; diferencia de media type/manifiesto bloquea en vez de aceptar config ID. PR/main/feature sin contexto DEV elegible no publican. Ninguna aprobación ECR invoca deploy. Rechazar CI ajeno, cancelado, incompleto, artifacts/manifiestos alterados y política caducada. Imagen manual existente no obtiene CI GREEN retrospectivo.
- **Trazabilidad:** CI-002/011; SEC-004–012; REL-001–006/008/010/011; CD-003; PLD-002/007–010/012/013/020; AC-06/07/08/14/15; GATE-CI-E/CD-A/CD-B.
- **Evidencia futura:** E(T-CICD-106), run confiable, aprobación y manifest digest local/remoto iguales, pruebas de no publicación PR y de no auto-deployment.

### T-CICD-107 — Botón DEV manual, candidata automática y preflight seguro

- **Estado:** IMPLEMENTED en código; CONFIGURATION PENDING y RUNTIME VALIDATION PENDING. Resolver/dispatch/preflight/lock preparados; sin botón GitHub acreditado, candidata aprobada ni retorno manual incorporado.
- **Fase:** PHASE 4 — Manual rolling deployment.
- **Capacidad:** deploy-dev.yml con botón manual y selección/preflight/lock que rechaza antes de mutar cualquier candidata o baseline inseguro.
- **Dependencias:**101–106 aceptadas en su alcance; DP-01/02/06–09/13/14. La activación cloud depende además de108/109 listas y autorización de110.
- **Archivos/componentes futuros:** `.github/workflows/deploy-dev.yml`, helpers de elegibilidad/registro/SSM en alcance autorizado y environment development; sin cuarto workflow.
- **Entrega completa:** nombre Deploy ENVER Dev y solo workflow_dispatch; disponibilidad en default branch y ejecución desde develop confiable. Inputs environment=development y run_smoke_tests true por defecto; no digest manual ni destinos/comandos libres. Elegir última candidata aprobada/vigente alcanzable develop, comprobar CI/hardening exactos/registro/ECR, congelar selección, exclusión enver-development-deploy/cancel-in-progress=false y revalidación. Guardar por nodo digest previo aprobado/compatible/retendido y estado inicial homogéneo; comprobar invariantes de baseline y return path antes de A. Lecturas AWS temporales OIDC no equivalen a mutación autorizada.
- **Aceptación:** botón/ref/actor/contexto verificables; push/PR/workflow_run/schedule/release/workflow_call jamás despliegan ni llaman dispatch. Ausencia de candidata/previo seguro, API incierta, SHA/ref/policy incorrectos o mutación remota no reconciliada bloquean sin tocar A/B. Default branch main no obliga a desplegar main; summary identifica si candidata elegida es anterior al head develop. Preflight prepara metadatos de recuperación, no secretos ni state Terraform.
- **Trazabilidad:** CI-002/011; CD-001–006/010/012; SEC-005–012; REL-002/004–012; PLD-001/002/007/009–015/019/020; AC-01/07/08/09/13/14/15/16; GATE-CD-A/B/D.
- **Evidencia futura:** E(T-CICD-107), dispatch/ref/candidata y negativos de elegibilidad/concurrencia con resultado nativo, primero sin mutaciones reales.

### T-CICD-108 — Rolling A→B por digest con preservación runtime

- **Estado:** IMPLEMENTED en código; RUNTIME VALIDATION PENDING. Orquestación y documento SSM fijo preparados; ninguna mutación/restart A/B ni validación AWS ejecutada.
- **Fase:** PHASE 4 — Manual rolling deployment.
- **Capacidad:** orquestación secuencial del despliegue con hooks obligatorios de verificación/compensación y registro de mutaciones, sin cambiar infraestructura.
- **Dependencias:**107; contrato de verificación/compensación de109 diseñado y acordado. No habilitar un run mutador hasta109 integrada/verificada y110 específicamente autorizada.
- **Archivos/componentes futuros:** deploy-dev.yml y ejecutor SSM restringido/handlers de estado; referencia activa por mecanismo existente. Program.cs/Dockerfile/systemd/overlay/secretos/IaC no se alteran implícitamente.
- **Entrega completa:** A primero, despacho rastreado y cambio de referencia al digest aprobado, pull de esa imagen por runtime EC2 y restart/materialización ordinaria; polling bounded de servicio/container/digest/binding/health/target. B solo después de A PASS y target healthy. Preservar OriginSecret/JWT/autoridad/generación cuotas/AllowLocalHttp=false, ALB/CloudFront, unidad/wrappers/overlay/journald/montaje/permisos existentes. Registrar CommandId y estado antes/después, congelar imagen y activar señal de compensación ante fallo real, no seguir B por simple ACK.
- **Aceptación:** en pruebas seguras de orquestador, ninguna llamada mutadora B antes de A validada; fallo A nunca toca B; versiones/destinos fuera del baseline se rechazan. No rebuild/repush de deployment, initialize, Terraform apply ni recreación de recursos. Lecturas acotadas toleran carrera inicial sin segundo restart forward. Timeout no autoriza retry ciego ni solapar órdenes. Compensación/resultado final integrados en109 antes de operación AWS.
- **Trazabilidad:** CD-006/007/010–012; SEC-008/010; REL-002/004–010; PLD-008/013/015/016/018/019/020; AC-06/10/13/14/15; GATE-CD-B/C/D.
- **Evidencia futura:** E(T-CICD-108), máquina de estados/contratos/readiness e invariantes verificadas en entorno autorizado; pruebas AWS reales solo cuando se autoricen, sin presentarlas anticipadamente como PASS.

### T-CICD-109 — Verificación HTTPS y rollback compensatorio integrado

- **Estado:** IMPLEMENTED en código; RUNTIME VALIDATION PENDING. Pruebas sintéticas de orden/compensación/rechazos PASS, no evidencia de rollback real. AUTH smoke no destructivo limitado a health; AUTH NOT EXECUTED sin identidad reutilizable.
- **Fase:** PHASE 5 — Verification + rollback.
- **Capacidad:** verificaciones finales y retorno seguro dentro del MISMO run manual, con fallo original irreversible.
- **Dependencias:**107/108; imagen previa aprobada/compatible y contrato runtime; DP-09/10/11/13/14; objetivos/credenciales sintéticas autorizados antes de tráfico público.
- **Archivos/componentes futuros:** deploy-dev.yml, helpers de readiness/health/smoke/compensación/summary y pruebas seguras del orquestador en alcance posterior.
- **Entrega completa:** CloudFront HTTPS /health200 obligatorio; targets/digests A/B; smoke AUTH según input, sin body/token/password en evidencias ni retries POST. Fallo A: no B, reconciliar/restaurar/validar solo A. Fallo B: detener avance, reconciliar y restaurar B primero, después A si quedó nueva; comprobar A/B/targets/CloudFront health. Fallo final/smoke solicitado: mismo retorno B→A. No restaurar datos/secretos/Redis/autoridad/cuotas, no lanzar otro workflow ni pedir segundo botón para compensar.
- **Aceptación:** flujo de fallo no queda skipped por needs; presupuesto de una activación y un retorno por nodo cuando necesario, evitando restart si ya está anterior. Deployment original FAILED aun con rollback PASS. Si retorno parcial/fallido/incierto, reportarlo y continuar únicamente pasos seguros tras reconciliación; sin bucle de retries ni éxito falso. Runner perdido no promete rollback ejecutado: recuperación BLOCKED/pendiente y próxima operación reconcilia. Smoke false = AUTH NOT EXECUTED, nunca omite health/targets/readiness. Logout204 repetido no prueba rechazo distribuido.
- **Trazabilidad:** CD-006–012; SEC-010/012; REL-002/004–010/012; PLD-013/015–020; AC-10–16; GATE-CD-B/C/D.
- **Evidencia futura:** E(T-CICD-109), escenarios A/B/final/rollback parcial/interrupción, outcomes separados y prueba de sanitización; cloud solo con autorización delimitada.

### T-CICD-110 — Verificación completa, negativos y evidencia académica

- **Estado:** RUNTIME VALIDATION PENDING; verificaciones locales aplicables PASS según §§10–16 y fallos previos conservados. Tres workflows READY FOR GITHUB en código; corte para commit/push autorizado, sin runs acreditados. Environment/subject configurados, configuración operativa restante/SSM pendientes. Provider/roles AWS creados/read-back previo PASS; end-to-end GitHub→OIDC→STS/ECR/deploy NO EJECUTADO, STS PENDING UNTIL GITHUB EXECUTION tras publicar. Ningún gate cerrado por mocks/YAML.
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

Las veinte PLD enlazan tareas en plan §2; los dieciséis AC enlazan requisitos/gates en spec §12 y tareas en §4. **48/48 requisitos con cobertura documental,20/20 decisiones enlazadas,16/16 AC enlazados,5 fases,10 capacidades y9 gates pendientes.** La implementación local posterior de §7 no equivale a cumplimiento técnico integral.

## 6. Cierre del gate documental anterior (histórico)

SPEC UPDATED / PLAN UPDATED / TASKS UPDATED. DEPLOY BRANCH=develop; MAIN ROLE=stable; AWS DEPLOY=MANUAL workflow_dispatch; ROLLBACK=AUTOMATIC COMPENSATING WITHIN MANUAL DEPLOYMENT; WORKFLOWS=3; OIDC=REQUIRED; BUILD STRATEGY=BUILD ONCE / DEPLOY SAME DIGEST; AWS MODIFIED=NO.

**NEXT GATE=IMPLEMENT COMPLETE CI/CD PIPELINE.** Este catálogo no lo inicia: STOP tras la actualización de los tres Markdown y comprobación documental de coherencia/enlaces. Sin workflows, AWS/IAM, pruebas, deployment, initialize, stage, commit o push nuevos.

## 7. Ejecución local posterior autorizada — 2026-11-19

**Antecedente de la primera implementación; corrección posterior y estado vigente en §8.** Los fallos originales se conservan, no se borran por un resultado posterior.

Se recuperan y conservan los archivos del intento interrumpido por límite de uso; un timeout local no se convierte en diagnóstico funcional ni autoriza repetir mutaciones. `feature/ci-cd` parte de `origin/develop` `2e89e3c`, merge PR#2 que contiene `bab1f17`. Working tree inicial limpio antes de crear la rama; recuperación con 27 archivos nuevos, sin staged/commit/push. No se resetean cambios.

Tres workflows creados, **12 jobs CI / 10 hardening / 10 deploy** (CI runtime usa matriz Linux/Windows), helpers, tests y policies locales. Scripts con estados y agregación fail-closed; CI no recibe AWS; build-once/digest OCI→promoción→deploy; deployment únicamente dispatch humano con compensación mismo run. No se han construido/publicado candidatas ni invocado fases AWS para esta validación. [Runbook](../../scripts/README.md), [evidencia local y bloqueos](../../scripts/validation.md).

E(T-CICD-101/102/103/104/107/108/109/110) es **parcial/local**, no un run Actions: YAML/Actionlint/Node/Python, restore/build Release/UT, suites IT/ST con fallos, cinco ejecutables runtime observados, Semgrep/Hadolint/Gitleaks/NuGet e IaC. Los resultados fallidos/no ejecutados permanecen explícitos. `RuntimeToolRestore.targets` respeta los RIDs ya fijados en locks, sin regenerarlos ni modificar proyectos operativos. Semgrep declara su alcance de texto C#; no presume AST completo. Los 11 fingerprints Gitleaks revisados corresponden a prosa/fixtures no operativas, no a secretos aceptados.

E(T-CICD-105/106) **no acredita infraestructura OIDC, asunción real ni publicación**. Policies/claims son archivos preparatorios; flags Publisher/Deploy permanecen sin habilitar; retorno manual conocido sigue `approved=false`. La prueba Orchestrator conserva su validación de estación/hashes y bloquea: no se cambia para fabricar PASS. Trivy detecta egress irrestricto CRITICAL del baseline; no se edita SG/HCL ni se aprueba excepción por iniciativa.

**Cierre de aquel intento (histórico):** implementación local conservada para revisión; ninguna tarea COMPLETE integralmente, nueve gates PENDIENTE / NO SATISFECHO. NEXT GATE de entonces=**FIX PIPELINE**; estado vigente en §8. **AWS MODIFIED=NO; DEPLOY EXECUTED=NO; INITIALIZE EXECUTED=NO; COMMIT/PUSH/PR=NO.** Historical initialize total=1. AUTH-027 y aceptación distribuida no se cierran por estas verificaciones.

## 8. FIX PIPELINE — corrección local posterior autorizada

La solicitud expresa del responsable autoriza diagnosticar y corregir los5IT/5ST, separar la validación histórica del Orchestrator, corregir HCL y revisar los tres grafos sin rediseñarlos. Se preserva `feature/ci-cd`,37 archivos previos, cero stage/commit/push/PR. Informe vigente con diez casos por nombre/archivo/contrato/resultado/causa y lista exacta de archivos: [fix-pipeline-validation.md](../../scripts/fix-pipeline-validation.md).

Ocho fallos de config compartían una suposición obsoleta de WebApplicationFactory: SafeStartup sanea el fallo Options y dispone el host, por lo que el harness veía ObjectDisposedException. Se observa/propaga el IStartupValidator real **solo en tests**, sin cambiar Program, validadores o saneamiento productivo; se añaden asserts de tipo Options conservando rechazo/mensaje/no-secret. Los2Mongo no se reprodujeron en diagnóstico; la evidencia histórica no permite asignar operación exacta. Se corrige el acoplamiento de plazos de setup/cleanup a sockets de request, con readiness ping acotada, sin aumentar budgets/concerns/retries SUT y con diagnósticos seguros.

Orchestrator portable28/28 y PipeProducer29/29 PASS; otras tres suites runtime PASS. La prueba positiva de hashes/ruta de estación se conserva en modo separado explícito `--historical-artifact-validation`, NO EJECUTADO en CI; producción/hashes/descriptor intactos. No se omite toda la herramienta ni se inventan artefactos.

**Aprobación adicional consultada:** el responsable acepta dos excepciones exactas AVD-AWS-0104, owner responsable ENVER, caducidad2026-12-19 y revisión antes de apply. Se retira all-protocol en HCL y separa TCP443/27017 a destinos públicos dinámicos y TCP6379 al SG cache. [EGRESS-REVIEW.md](../../infra/EGRESS-REVIEW.md) registra justificación/riesgo/condiciones. Sin excepción global ni cambio de AWS. Terraform fmt/init sin backend/validate PASS; Trivy fuente PASS con4LOW/MEDIUM conservados; prueba negativa sin anotaciones bloquea exactamente los dos recursos. Scanner/validate usan snapshot publicable sin state/planes/tfvars/cache locales, evitando planes históricos que contaminaban el scan.

Un intento local de init falló por disco insuficiente; se valida con provider instalado/verificado y lock readonly, sin vincular state. Docker Linux dejó de estar disponible; un intento afectado falló13/13 en setup, no en el SUT. El responsable autoriza arrancar Docker Desktop local, motor listo con polling acotado, sin reset/prune ni restart EC2. Estos fallos se conservan y no se repiten suites ciegamente.

**Resultados de cierre:** dirigidas13/13IT y6/6ST PASS. El intento completo posterior IT agotó30min sin TRX; ST114/223PASS y109fallos DockerUnavailable en setup,0skipped. Semgrep BLOCKED sin reporte válido; Hadolint no alcanzado en ese intento. Docker no se recuperó tras liberar~583MB y el único reinicio adicional expresamente autorizado: exit1 y readiness acotada expirada, sin reset/prune ni segundo reinicio. No se repiten suites con motor no disponible. UT844/844, Node29/29, Python7/7, runtime portable, Terraform/Trivy fuente aislados, NuGet y YAML/Actionlint PASS con límites registrados. Secret scan final se realiza con Gitleaks8.24.2 Windows oficial/SHA256 verificado, independiente de Docker: historia y47archivos publicables sin findings, canarios genérico/runtime detectados y redactados; no acredita ejecución Docker.

CI conserva LOCAL VALIDATION BLOCKED / NOT READY; Hardening y Deploy IMPLEMENTED / CONFIGURATION PENDING / RUNTIME VALIDATION PENDING, sin publicación ni deployment habilitados. **NEXT GATE=FIX REMAINING PIPELINE BLOCKERS:** recuperar espacio/motor local, acreditar IT/ST completas y controles Docker; después solicitar GitHub/OIDC/IAM por separado. No se presentan311/311IT o223/223ST como logrados. La causa/operación exacta de los2Mongo históricos sigue limitada por el TRX original; el diagnóstico nuevo permitirá precisarla si reaparece, sin cambiar semántica productiva. Cada ejecución valida working tree local, no un run GitHub/attestation del commit base. E(T-CICD-101/102/103/104/110) parcial; E105/106 sin OIDC/ECR. Ninguna tarea COMPLETE ni gate satisfecho por inferencia. **AWS/OIDC/DEPLOY/INITIALIZE/STAGE/COMMIT/PUSH/PR NO EJECUTADOS.** Historical initialize total1; AUTH-027 no acreditado.

## 9. FIX REMAINING PIPELINE BLOCKERS — precheck posterior 2026-11-19

Solicitud autorizada: comprobar almacenamiento/Docker, ejecutar una vez IT/ST completas únicamente después de readiness real y repetir controles locales sin rediseñar workflows ni intervenir AWS/IAM. **Resultado: LOCAL ENVIRONMENT BLOCKER; no se alcanzan las precondiciones de ejecución.** Se preservan los resultados y recibos históricos de §§7–8; no son ejecuciones nuevas de este gate.

- **Disco al precheck:** C: libre **173.019.136 bytes (165 MiB; 0,161 GiB)**. Para alcanzar el mínimo conservador de **2 GiB (2.147.483.648 bytes)** faltaban exactamente **1.974.464.512 bytes**. Ese mínimo solo permite reconsiderar readiness, no garantiza capacidad para todas las suites/scanners. No se borran datos, temporales, volúmenes ni evidencia; no reset/prune.
- **Presión identificada:** `docker_data.vhdx` de **40.856.715.264 bytes** (longitud del archivo, no medición de espacio recuperable ni uso interno). Los dos temporales `terraform-provider3195792105` / `terraform-provider1038921446` y el instalador temporal Warp suman **339.838.703 bytes**; son candidatos a revisar con su propietario, no archivos acreditados como descartables. Aun eliminándolos no bastaría. No se leen secretos ni se examina su contenido.
- **Docker:** contexto activo esperado `desktop-linux`, endpoint `npipe:////./pipe/dockerDesktopLinuxEngine`. Una consulta `docker version` y una `docker info`, cada una limitada a12s, expiran sin respuesta. `docker context ls` exit0. Desktop/backend tienen procesos activos; `com.docker.service` Stopped/Manual no basta por sí solo para diagnosticar el motor. Se terminan únicamente los clientes CLI vencidos, no el daemon. **0 arranques/reinicios, 0 contenedores y 0 reintentos de suite** en este gate por el umbral de disco; no se afirma readiness ni ejecución mínima PASS.
- **Validación ligera nueva:** YAML1.2/unique keys, referencias needs y conteos **12/10/10 PASS**; deploy mantiene solo workflow_dispatch y concurrencia requerida. Actionlint instalado PASS, sin ShellCheck/Pyflakes externos. `terraform fmt -check -recursive -no-color` PASS, sin init/descargas. Sin cambios a workflows/HCL/Dockerfile/pruebas.
- **Validación bloqueada nueva:** IT/ST completas **BLOCKED / NOT EXECUTED**; conservan como antecedentes IT timeout sin TRX y ST114/223PASS/109fallos de setup DockerUnavailable, no regresiones funcionales acreditadas. Semgrep y Hadolint **BLOCKED / NOT EXECUTED**. IaC final **BLOCKED**: fmt PASS, validate/Trivy no repetidos por entorno; se conservan sus PASS aislados anteriores y el fallo posterior de init, sin convertirlos en PASS nuevo. Solo las dos excepciones exactas AVD-AWS-0104 siguen aprobadas. ZAP/RESTler sin laboratorio permanecen NOT EXECUTED/BLOCKED según aplicabilidad, sin PASS ni nuevo bloqueo de sintaxis.

**CI.YML / HARDENING.YML / DEPLOY-DEV.YML = LOCAL VALIDATION BLOCKED.** Código implementado no equivale a validación local completa, GitHub runtime ni AWS runtime. Estados de capacidades §4 se conservan; ninguna COMPLETE, OIDC/deployment no configurados ni ejecutados y nueve gates pendientes. **NEXT GATE=RESOLVE LOCAL DOCKER ENVIRONMENT.** Acción manual: liberar/ampliar almacenamiento fuera del repositorio y sin tocar datos Docker/evidencias hasta al menos2GiB libres, volver a medir, recuperar controladamente el motor y exigir `docker info` + contenedor mínimo ya cacheado antes de las suites únicas. No configurar OIDC/IAM todavía. Detalle y recibos nuevos en [informe §8](../../scripts/fix-pipeline-validation.md#8-fix-remaining-pipeline-blockers--precheck-posterior).

## 10. Recuperación y continuación del runner original — 2026-11-19

Solicitudes RESOLVE LOCAL DOCKER ENVIRONMENT AND COMPLETE PIPELINE VALIDATION y CONTINUE PIPELINE VALIDATION FROM CURRENT STATE: preservar rama/cambios/evidencias y completar únicamente validaciones pendientes. La interrupción de conversación por **USAGE LIMIT ONLY** no fue un fallo del pipeline ni terminó el proceso original: al reanudar se encontró `validate-suites.cjs` activo con su testhost de IntegrationTests; **no se relanza ni duplica**. El runner mantiene una ejecución por suite, SDK10.0.403, build Release sin restore, resultados nuevos y timeout45min por suite, comprobando disco/Docker antes de cada una.

- Precheck inicial C: **2.483.691.520 bytes / 2.368,633MiB / 2,313118GiB**; lectura de recuperación2.459.844.608 bytes /2,290909GiB. Sin borrados ni reset/prune. Docker `desktop-linux`, version/info PASS, contenedor mínimo Hadolint2.12.0 cacheado PASS; **DOCKER READY**, sin start/restart.
- Hadolint **PASS /0findings**. Semgrep **PASS /137archivos /0findings**, modo C# text patterns declarado; no AST/interprocedural y CodeQL sigue obligatorio en GitHub. YAML1.2/unique keys/needs y Actionlint **PASS**,12/10/10jobs; deploy solo workflow_dispatch/concurrencia preservados, sin ShellCheck/Pyflakes externos acreditados.
- Terraform **fmt/validate PASS** en snapshot publicable existente cuyos26archivos coinciden byte por byte con las fuentes vigentes; provider ya disponible, sin init/descarga ni backend/state/plan/apply. Trivy config con digest aprobado **PASS**,4LOW/MEDIUM,0HIGH/CRITICAL; únicamente dos excepciones precisas AVD-AWS-0104 vigentes, observedExceptions=0 según salida real. La imagen local faltante se recupera del registro público con presupuesto de espacio comprobado, no desde ECR ni EC2.
- **Resultados finales del mismo runner, sin relanzarlo:** IntegrationTests **311total/311PASS/0FAIL/0skipped**, exit0, **683,100s (11min23s)**. SecurityTests **223total/222PASS/1FAIL/0skipped**, exit1, **123,112s (2min3s)**. Ambos TRX completos se conservan. No se repiten UT/dirigidas/runtime/Node/Python ni las suites completas para buscar verde.
- **Fallo ST exacto:** `LoginSecurityTests.Security_rehash_race_has_one_winner_no_loop_and_only_closed_observability`, `tests/Enver.API.SecurityTests/LoginSecurityTests.cs:60`: esperaba200/OK y recibió503/ServiceUnavailable; duración del caso2,2761003s. Clasificación **FUNCTIONAL TEST ASSERTION FAILURE / NO SETUP FAILURE**; no DockerUnavailable ni skip. No acredita por sí solo una regresión productiva ni una causa raíz concreta: el TRX no captura la fase/probes/eventos de esa solicitud.
- **Inspección sin cambios:** el caso usa LoginTestScope sin override y hereda LoginBudget1000ms de AuthenticationDependenciesFixture; el caso concurrente análogo de IT usa un override de test4000ms. Es un **candidato de sensibilidad al presupuesto**, no un timeout demostrado: el tiempo del caso incluye setup/cleanup y el503 también puede salir de cuotas/admisión/Mongo/CAS/relectura/rehash/emisión. No se amplían budgets, debilitan asserts, modifican producto/harness ni reejecutan tests bajo esta autorización de continuación. Próximo incremento: diagnóstico acotado autorizado de ese caso, conservando conteos CAS/relectura/emisión y no-secret, antes de decidir corrección.

Recibos nuevos privados: `%LOCALAPPDATA%/Temp/opencode/enver-fix-pipeline/docker-resume-2026-11-19T18-14-43-567Z/`, con initial-docker/recovered-readiness, integration/security TRX y resúmenes, suites-summary, hadolint, semgrep-evidence, iac-final, workflows y cierre final de secretos/hashes/diff. Son evidencia del working tree no comprometido, no attestation de implementación en HEAD ni ejecución GitHub. El recibo final debe comprobar los bytes documentales finales; la historia Gitleaks PASS anterior se conserva, sin nuevos commits/fetch.

**CI.YML / HARDENING.YML / DEPLOY-DEV.YML = LOCAL VALIDATION BLOCKED** por el único fallo ST restante, no por Docker ni por ausencia de laboratorio ZAP/RESTler. Local code validation ≠ GitHub runtime validation ≠ AWS runtime validation: GitHub/Linux .NET/CodeQL/mutation/OCI y AWS/OIDC/publicación/deploy siguen pendientes donde aplican; no se inventan PASS ni se adelanta configuración. Nueve gates pendientes, ninguna tarea COMPLETE; **AWS/OIDC/DEPLOY/INITIALIZE/STAGE/COMMIT/PUSH/PR NO EJECUTADOS**. ZAP/RESTler sin lab conservan NOT EXECUTED/BLOCKED/RUNTIME VALIDATION PENDING, no PASS ni bloqueo de sintaxis local. **NEXT GATE=FIX REMAINING PIPELINE BLOCKERS** con diagnóstico delimitado del nuevo fallo ST antes de otra ejecución; Docker no requiere otro reset/restart.

## 11. FIX FINAL SECURITY TEST BLOCKER — cierre local 2026-11-19

Autorización exclusiva para reproducir/instrumentar el único caso ST, corregir la causa acreditada, verificar objetivo/grupo/suite y regresión pertinente. Diagnóstico completo y productores503 en [informe §10](../../scripts/fix-pipeline-validation.md#10-fix-final-security-test-blocker--test-budget-confirmado). No rediseño/workflows/AWS/IAM/OIDC/deploy/initialize operativo/stage/commit/push/PR.

**Clasificación única E — TEST BUDGET.** Una reproducción con1000ms confirmó ambas admisiones Verifiable/cuotas Admitted, índice Ready/dummy inicializado, CAS Updated/Conflict y una relectura Found. A agotó RequestBudget durante reverificación (139,240ms→0), B durante emisión protegida (139,918ms→0),0firmas y ambos503temporarily_unavailable. El fail-closed productivo es correcto. La causa de la reproducción queda probada; el TRX histórico anterior sin fases se conserva intacto.

**Corrección solo test/harness:** el caso de carrera usa4000ms como su IT análogo; nuevos asserts comprueban base fixture1000, Mongo500/Redis250/hashing1 y override solo en ambos hosts de ese caso. Se mantienen dos200, dosCAS, una relectura, dos emisiones y allowlist cerrada de eventos. No cambian appsettings, password hashing, CAS, JWT, trust o cuotas. Diagnóstico opt-in desactivado por defecto en LoginTestScope, solo enums/tiempos/contadores, sin identidad/payload/password/JWT/hash/excepciones. Sin sleeps, skips, retries ni simulación favorable de dependencias. Contrato002 AUTH-013/021/039 y AC-06: ambas credenciales válidas pueden emitir si controles/presupuesto verificables; no cambiar200 a503.

| Verificación autorizada, una ejecución por paso | Resultado |
|---|---|
| Reproducción original1000ms |1FAIL/0skip, RequestBudget probado; preservada |
| Objetivo corregido4000ms |1/1PASS/0failed/skipped;70,989s incluyendo fixture |
| Grupo LoginSecurityTests |62/62PASS/0failed/skipped;110,290s |
| SecurityTests completa, después del grupo |223/223PASS/0failed/skipped;141,479s |
| Regresión IT Login por harness compartido |25/25PASS/0failed/skipped;95,098s; IT completa311/311 previa no repetida |

SDK10.0.403/build Release sin restore y assets locked previos. Un primer build de instrumentación falló por using Quotas ausente, corregido antes de ejecutar pruebas; logs preservados. UT844/runtime/Node/Python PASS previos no repetidos, producto/tools/helpers intactos. Semgrep137/0findings, Hadolint e IaC PASS conservados por bytes de su alcance intactos; YAML/Actionlint/secretos/hashes/diff finales en recibo de cierre, sin extender cobertura Semgrep a los tests.

**CI.YML / HARDENING.YML / DEPLOY-DEV.YML = READY FOR GITHUB**, solo código local listo para primera ejecución autorizada, no CI cloud verde. Hardening conserva configuración/controles externos pendientes y CodeQL obligatorio; Deploy requiere OIDC/IAM/environment/actores/SSM, flags cerrados. Linux .NET/CodeQL/mutation/OCI, runs GitHub y AWS runtime no acreditados. ZAP/RESTler sin lab NOT EXECUTED/BLOCKED/RUNTIME VALIDATION PENDING, no PASS; controles solicitados/aplicables siguen fail-closed.12/10/10jobs y deploy solo workflow_dispatch intactos.

Delta exclusivo: `tests/Shared/LoginTestScope.cs`, `tests/Enver.API.SecurityTests/LoginSecurityTests.cs` y tasks/informe/memoria;49archivos publicables totales, previos preservados. Recibos privados `%LOCALAPPDATA%/Temp/opencode/enver-fix-pipeline/final-security-2026-11-19T18-40-16-326Z/`: diagnostic/target-fixed/login-group/security-full/integration-login, diagnosis.json, scope-proof y cierre final. **REMAINING LOCAL BLOCKERS=NONE; NEXT GATE=CONFIGURE GITHUB OIDC AND IAM**, solo como solicitud posterior. Nueve gates pendientes, ninguna tarea004 COMPLETE integralmente, AUTH-027 no cerrado. **AWS MODIFIED=NO; OIDC CONFIGURED=NO; DEPLOY EXECUTED=NO; INITIALIZE OPERATIVO EXECUTED=NO; STAGE/COMMIT/PUSH/PR=NO.**

## 12. FIX OIDC/IAM CONFIGURATION — AWS creado/verificado, 2026-11-19

Autorizaciones sucesivas CONFIGURE/CONTINUE/FIX OIDC/IAM permiten provider/roles/policies, ARNs locales y validación pertinente; no deploy, publicación ECR de prueba, EC2 restart, CloudFront/ALB/VPC/secretos/runtime/authority/generación, apply general, initialize o Git de escritura. Solo `--profile enver-infra`; identidad verificada `arn:aws:iam::926925605666:user/enver-terraform-local`. No se usa enver-admin-temp en esta continuación. Prechecks STS/ListPolicies/ListOpenIDConnectProviders/ListRoles PASS; inventario paginado13customer policies,0providers/0roles GitHub existentes.

| Recurso creado (sin duplicados) | ARN real |
|---|---|
| GitHub OIDC, audience único sts.amazonaws.com | `arn:aws:iam::926925605666:oidc-provider/token.actions.githubusercontent.com` |
| Publisher | `arn:aws:iam::926925605666:role/enver-github-ecr-publisher` |
| Deploy Dev | `arn:aws:iam::926925605666:role/enver-github-deploy-dev` |

Cinco escrituras AWS:1CreateOpenIDConnectProvider,2CreateRole,2PutRolePolicy. Read-back GetOpenIDConnectProvider/GetRole/ListAttachedRolePolicies/ListRolePolicies/GetRolePolicy **PASS**: configuración exacta, máximo3600s, una inline y cero managed adjuntas por rol. Provider certificado obtenido por IAM, sin thumbprint inventado. Políticas regionales mínimas en `scripts/deploy/iam/`; no adjuntar las policies locales amplias de infraestructura.

Trust exacta StringEquals: aud sts.amazonaws.com; repo inmutable `enverdelivery@350055774/ENVER-Backend@1445184810`; environment development; workflow hardening/deploy-dev correspondiente en refs/heads/develop; evento Publisher push/workflow_dispatch, Deploy solo workflow_dispatch. Sin wildcard, forks, PR, main o feature. GitHub REST autenticado confirmó ownerId/repoId/fecha2026-11-04/default main/repositorio público, subject `use_default:true` y environment404. Se corrige el borrador anterior sin IDs y se incluye event_name en el template. **No se modificó GitHub**: configurar subject de `github-oidc-subject.json` y environment development/protecciones develop requiere intervención administrativa posterior; tokens por defecto no satisfacen trust. Condiciones inspeccionadas, no prueba real STS/denegaciones.

Publisher solo GetAuthorizationToken regional y8acciones ECR en enver-dev-api; sin SSM/IAM/EC2. Deploy solo DescribeImages, SendCommand al documento ENVER-Dev-ImageDeployment-v1 y dos EC2 autorizadas, GetDocument, GetCommandInvocation y DescribeTargetHealth. Resource:* solo donde API no admite granularidad, con región; no mutation infra/CloudFront/EC2, ni lectura de secretos. SSM GetDocument fue **AccessDenied** para el profile de bootstrap: existencia desconocida, no crear/invocar ni sustituir documento libre; provisionamiento/revisión SSM es pendiente separado. Versión1/hash se comprueban en workflow; no se atribuye esa restricción de versión a IAM.

Workflows llevan ARNs reales; helpers actualizan comprobaciones de configuración/identidad a esos nombres. CI AWS-free intacto, id-token solo Publisher y7jobs AWS Deploy, sin permisos globales. Se conservan12/10/10jobs, deploy solo dispatch/concurrencia/no cancel, custodia build-once/mismo digest/retorno aprobado y flags sin habilitar. YAML1.2/unique keys/needs/permissions/composites y Actionlint **PASS**; sin ShellCheck/Pyflakes externos nuevos. Pruebas dirigidas de esta configuración:3nuevas de trust/least-privilege antes de aplicar y2después (workflow y único contrato IAM modificado) **5/5PASS,0skip**, sin repetir29helpers ni311/223/844 ni scanners previos innecesarios. Secret scan final/diff/manifiesto se conservan en recibo final; producto/IaC/Dockerfile/pruebas API intactos.

Recibos privados `%LOCALAPPDATA%/Temp/opencode/enver-oidc-2026-11-19T19-29-17-411Z/`: inventory/applied/verification, llamadas IAM saneadas, local-workflows, escaneo y cierre final. Fallos previos de permisos quedan históricos, no se ocultan ni se presentan como aplicación parcial.

**¿Ya pueden retirarse los permisos IAM temporales? SÍ**, la creación y lectura de verificación AWS finalizaron; runtime GitHub/SSM no requiere conservar privilegios de bootstrap. Retirar en gate separado únicamente GithubOidcBootstrapRead/Provider/PublisherRole/DeployRole de ENVER-Terraform-IAM-Runtime, preservando sus9statements base. v3/hash permanecen intactos aquí. La policy ENVER-GitHub-OIDC-Bootstrap-Temporary inventariada tiene0attachments y no se usa/edita. **NEXT GATE=REMOVE TEMP IAM BOOTSTRAP PERMISSIONS**, no ejecutado. Configuración GitHub, documento SSM, sesión real STS y runtime siguen pendientes; no deployment listo operativamente ni GATE-CD-A cerrado. Nueve gates pendientes, ninguna tarea COMPLETE; AWS IAM/OIDC MODIFIED=YES, EC2/CloudFront/ALB/DEPLOY/INITIALIZE/STAGE/COMMIT/PUSH/PR=NO. Historical initialize total1 y AUTH-027 inalterados.

## 13. REMOVE TEMP IAM BOOTSTRAP PERMISSIONS — FAILED/BLOCKED, 2026-11-19

Autorización exclusiva para retirar GithubOidcBootstrapRead, GithubOidcBootstrapProvider, GithubOidcBootstrapPublisherRole y GithubOidcBootstrapDeployRole de ENVER-Terraform-IAM-Runtime, conservando exactamente sus9statements originales y todos los recursos OIDC/roles/trust/permisos/runtime. Se usa únicamente profile enver-infra e identidad STS `arn:aws:iam::926925605666:user/enver-terraform-local`; branch feature/ci-cd y trabajo anterior preservados.

Inspección PASS: default v3 tiene13statements, cuatro temporales y nueve originales; estos últimos coinciden estructuralmente con el snapshot acreditado del bootstrap. Se prepara fuera de Git un documento limpio de9statements, eliminando solo esos cuatro SID y conservando orden/contenido/metadatos restantes. Había2slots de versiones libres; no se elimina ninguna versión histórica.

**Resultado real: NO RETIRADOS.** Único intento `iam CreatePolicyVersion --set-as-default` sobre la política exacta, profile enver-infra, exit254/AccessDenied: falta autorización `iam:CreatePolicyVersion`. Cero escrituras AWS exitosas, cero retries, sin conceder permisos a sí mismo, usar otro perfil, reañadir bootstrap o alterar policies alternativas. No se afirma que la retirada esté completada.

Read-back posterior confirma default v3 y hash del documento completo **idénticos**: nueve originales intactos y los cuatro temporales todavía activos. Provider y ambos roles siguen presentes; GetOpenIDConnectProvider/GetRole/GetRolePolicy antes/después confirman provider/trust/permisos sin cambios y mismos RoleId. No borrados/modificaciones OIDC, EC2, CloudFront, ALB, runtime, secretos, authority/generación, deployment o initialize. Historical initialize total1.

Recibos privados `%LOCALAPPDATA%/Temp/opencode/enver-remove-bootstrap-2026-11-19T19-49-07-481Z/`: policy-before, policy-clean, versions-before, resources-before, plan, call-013 (denegación) y verification. Un administrador autorizado debe aplicar el documento limpio limitado a esos9statements y verificar la retirada; no se necesita otorgar administración general de policies al usuario local. No se comparte ningún secreto.

Solo tasks.md/memoria.md se actualizan en este gate, registrando el bloqueo; código/workflows/policies fuente y evidencia previa permanecen intactos. No se repiten suites ni scans previos ni se hace stage/commit/push/PR. **NEXT GATE=FIX TEMP IAM BOOTSTRAP REMOVAL**; únicamente tras confirmar la retirada procede **CONFIGURE GITHUB ENVIRONMENT AND OIDC SUBJECT**. Los pendientes GitHub/SSM/STS y nueve gates integrales se conservan; ninguna tarea COMPLETE ni AUTH-027 cerrado.

## 14. Retirada administrativa de bootstrap — verificación de solo lectura, 2026-11-19

El responsable informa retirada manual desde AWS Console de los cuatro statements temporales de ENVER-Terraform-IAM-Runtime y prohíbe volver a modificar esa política. La autorización actual permite exclusivamente lecturas disponibles y sincronizar tasks.md/memoria.md; no se ejecuta de nuevo el script de retirada ni se conceden permisos de inspección.

**TEMP BOOTSTRAP IAM PERMISSIONS = REMOVED.** STS con profile enver-infra confirma cuenta926925605666/usuario enver-terraform-local. GetPolicy/GetPolicyVersion leen default **v4** y confirman exactamente9statements,0SID GithubOidcBootstrap*. Comparación por SID de cada uno de los nueve objetos con el documento limpio acreditado PASS; metadatos de política intactos. Quedan preservados CreateOnlyTaggedRuntimeRoleWithApprovedBoundary, CreateOnlyTaggedRuntimeProfile, ExactRuntimeRoleLifecycleAndInlinePolicies, WriteInlineOnlyOnRuntimeRoleWithApprovedBoundary, ExactRuntimeProfileLifecycle, PassOnlyRuntimeRoleToEc2, DenyRoleCreationOrInlineWriteWithoutApprovedBoundary, DenyRuntimeBoundaryReplacementOrRemoval y DenyAdministrationOfExternalRuntimeBoundary. La retirada la realizó el administrador, no este agente; el intento denegado de §13 se conserva como antecedente.

Provider `arn:aws:iam::926925605666:oidc-provider/token.actions.githubusercontent.com`, Publisher `arn:aws:iam::926925605666:role/enver-github-ecr-publisher` y Deploy `arn:aws:iam::926925605666:role/enver-github-deploy-dev` **PRESERVED según confirmación administrativa y evidencia previa acreditada**. GetOpenIDConnectProvider/GetRole actuales devuelven AccessDenied; no se afirma read-back nuevo de existencia/trust/permisos, no se reañade bootstrap ni se usa otro perfil. No hay hallazgo de borrado ni modificación por el agente.

Recibos privados `%LOCALAPPDATA%/Temp/opencode/enver-bootstrap-readonly-2026-11-19T19-56-05-033Z/`: llamadas exclusivamente STS/IAM read-only y verification.json; lectura vigente de v4/nueve originales/cero temporales y denegaciones explícitas. Cero escrituras AWS y cero nuevos intentos de modificar ENVER-Terraform-IAM-Runtime. No provider/roles/trust/permisos, EC2, CloudFront, ALB, runtime, SSM SendCommand, Terraform apply, deployment o initialize; histórico initialize total1. Solo estos dos Markdown actualizados, sin repetir validaciones locales ni stage/commit/push/PR.

**NEXT GATE=CONFIGURE GITHUB ENVIRONMENT AND OIDC SUBJECT**, no ejecutado aquí. GitHub development/subject personalizado, documento SSM y sesión GitHub→STS real siguen pendientes; ninguna habilitación de publicación/deploy, tarea integral COMPLETE, gate cloud satisfecho ni AUTH-027 cerrado. No volver a modificar la política runtime para ese próximo gate.

## 15. CONFIGURE GITHUB ENVIRONMENT AND OIDC SUBJECT — configuración verificada, 2026-11-19

Autorización para inspeccionar/configurar únicamente GitHub development y el subject que requiere la trust AWS aprobada; comprobar wiring local/credenciales sin valores y documentar. No autoriza deploy/publicación/SSM SendCommand/EC2/CloudFront/ALB/apply/initialize, modificar la política runtime o reañadir bootstrap. Rama local feature/ci-cd y trabajo anterior preservados; sin stage/commit/push/PR.

Acceso administrativo confirmado con GitHub CLI y REST (`admin:true`, repositoryId1445184810/ownerId350055774); las credenciales previas sin admin no hicieron cambios remotos. Antes: development404, subject use_default:true/use_immutable_subject:true y prefijo inmutable esperado. Repositorio público de usuario, default main, ninguna workflow remota y feature/ci-cd404: no se dispone todavía del código publicado para una prueba STS-only segura.

**GitHub environment development = CREATED / protection CONFIGURED**, read-back PASS: deployment_branch_policy protected_branches:false/custom_branch_policies:true; una única regla **name develop/type branch**, sin tags ni wildcard. Sin espera ni reviewers obligatorios, para mantener usable el flujo académico. Esta regla restringe el uso del environment, no dispara deploy; el workflow conserva exclusivamente workflow_dispatch humano y sus demás validaciones. No se crea production ni otros environments.

**OIDC CUSTOM SUBJECT = CONFIGURED**, read-back PASS: use_default:false, include_claim_keys ordenadas **repo/context/workflow_ref/event_name**, use_immutable_subject:true, sub_claim_prefix `repo:enverdelivery@350055774/ENVER-Backend@1445184810`. El subject se alinea con los JSON de trust aprobados sin modificarlos: environment development, refs/heads/develop, workflow hardening.yml con push/workflow_dispatch para Publisher y deploy-dev.yml con workflow_dispatch para Deploy. Cero llamadas AWS o ampliación de trust; no se afirma que configurar claims pruebe por sí solo una sesión STS.

Exactamente3escrituras GitHub: PUT development, POST regla branch develop y PUT subject. El adaptador temporal se detuvo tras crear el environment al esperar un nombre de campo incorrecto en la respuesta de listado; se corrigió a branch_policies y continuó comprobando el recibo/ID del mismo environment, sin recrearlo ni duplicar escrituras. Recibos conservados. Una vez completado, GET environment/regla/subject verifica todo lo anterior.

**PUBLISHER OIDC / DEPLOY OIDC = PASS en configuración local**, no runtime. ARNs reales comprobados: `arn:aws:iam::926925605666:role/enver-github-ecr-publisher` y `arn:aws:iam::926925605666:role/enver-github-deploy-dev`; configure-aws-credentials con pin inmutable/us-east-1/rol correcto. CI sin AWS ni id-token; solo publish_candidate y7jobs AWS Deploy tienen contents:read/id-token:write/environment development, sin permisos globales. Grafos12/10/10, trigger dispatch exclusivo, concurrencia cancel-in-progress:false, custodia build-once/digest y flags de habilitación inalterados. No se editaron workflows/helpers ni código productivo en este gate.

**STATIC AWS KEYS = NOT PRESENT:** listados de nombres de secrets de repositorio/environment vacíos; organization secrets no aplican por ser owner de tipo User. No se leen valores ni se crean AWS_ACCESS_KEY_ID/AWS_SECRET_ACCESS_KEY/AWS_SESSION_TOKEN. Autenticación GitHub utilizada solo en memoria, no tokens en fuentes/evidencias.

**OIDC→STS RUNTIME TEST = PENDING UNTIL PUSH. SSM DOCUMENT provisioning/verification = PENDING.** Sin workflow dispatch, sesión AWS, ECR mutation, SSM o tráfico CloudFront. No crear un cuarto workflow ni disparar Publisher/Deploy para sustituir la prueba mínima; la prueba segura requerirá código publicado y autorización posterior, limitada a asumir rol/get-caller-identity. No se intenta resolver SSM por este gate.

Validaciones nuevas: YAML1.2/unique keys/needs/composites/roles/permisos **PASS** y Actionlint1.7.12 **PASS**, sin ShellCheck/Pyflakes externos. El ejecutable cacheado no estaba disponible; se recuperó únicamente actionlint.exe del ZIP retenido, tras verificar SHA256 contra checksums oficiales, sin instalar paquetes ni cambiar fuentes. Escaneo final de secretos/hashes/diff/index se registra tras estos Markdown; suites311/223/844 y scanners técnicos previos no repetidos. Recibos privados `%LOCALAPPDATA%/Temp/opencode/enver-github-oidc-2026-11-19T23-53-57-399Z/`: inventory/applied/verification, llamadas REST sin tokens, validator-proof/local-workflows y cierre final.

**NEXT GATE=REVIEW AND COMMIT CI-CD BRANCH**, solo próximo paso solicitado, no ejecutado. TEMP BOOTSTRAP IAM PERMISSIONS=REMOVED; política runtime/proveedor/roles/trust/permisos AWS intactos por este gate. EC2/CloudFront/ALB/SSM SendCommand/apply/publicación/deploy/initialize/stage/commit/push/PR=NO, historical initialize total1. Nueve gates integrales pendientes, ninguna tarea COMPLETE ni AUTH-027 cerrado; configuración GitHub verificada no equivale a runtime AWS acreditado o deployment habilitado.

## 16. REVIEW AND COMMIT CI-CD BRANCH — corte revisado y publicación autorizada, 2026-11-20 UTC

Autorización expresa para revisión final, YAML/Actionlint/secret scan/diff, stage de archivos intencionados, **un único commit nuevo** `feat(ci-cd): add secure GitHub Actions pipeline` y push únicamente `feature/ci-cd` a origin. No amend/force/tags, main/develop push, PR, merge, dispatch, AWS, publicación ECR, SSM provision/SendCommand, EC2/CloudFront/ALB mutation, Terraform apply ni initialize. El resultado de commit/push y discovery se comprueba después mediante Git/REST y recibos privados; no se presupone por registrar esta autorización antes del commit.

Preflight: origin fetch/push exacto `https://github.com/enverdelivery/ENVER-Backend.git`, sin credenciales en URL; rama correcta/HEAD padre `2e89e3c2830a2aa21e573e8c9c4526e4dcd753ca`, índice vacío, feature remota inicialmente ausente.50archivos publicables **idénticos byte por byte al manifiesto del último cierre acreditado**. No producto/pruebas cambiados después de sus ejecuciones: no repetir IT311/311, ST223/223, UT844/844, LoginSecurity62/62, Semgrep/Hadolint/IaC ni suites costosas. Se revisa el diff histórico de harness, test de carrera, separación Orchestrator y dos excepciones HCL aprobadas; no son cambios posteriores ni nuevos permisos para apply.

Revisión funcional/de seguridad de fuente PASS: CI únicamente pull_request/push de integración develop/main, sin AWS/OIDC; hardening preserva Publisher exacto, gate previo/candidata build-once y copia preserve-digests. Deploy solo workflow_dispatch humano/develop, rol exacto, environment development, A→readiness/target→B→readiness/target→HTTPS y compensación B→A dentro del mismo run; resultado original FAILED irreversible. Grafos12/10/10, id-token solo Publisher/7jobs AWS Deploy, concurrencia enver-development-deploy/cancel-in-progress:false. No rebuild de candidata aprobada, initialize o apply normal. `AllowLocalHttp=false`, modo CloudFront/fingerprints/authority/generación/rol runtime/custodia/retorno approved=false y flags cerrados preservados. Sin cambios productivos nuevos.

GitHub releído con admin:true/repoId1445184810/ownerId350055774: development existe, solo regla develop/type branch, subject custom con cuatro claims e IDs inmutables exactos. Secrets repo/environment0/0; variables de publicación/deploy no activadas. Sin tokens estáticos AWS ni OriginSecret literal, archivos temporales de credenciales, state/planes/bin/obj o generados ajenos en el conjunto intencionado. Trust JSON exactas StringEquals sin wildcard/PR/forks/main/feature; sin llamadas ni escrituras AWS para esta revisión. Provider y roles creados/read-back previo §12; runtime IAM v4/nueve originales/cero bootstrap verificados §14, sin reañadir permisos o afirmar nueva lectura IAM.

Se sincronizan **solo tres Markdown** en este gate: tasks.md v0.4.9, memoria.md y scripts/README.md. Este último contenía instrucciones operativas obsoletas de environment ausente/subject default/bootstrap aún por retirar; se corrigen contra §§14–15 sin borrar los informes históricos. El resto de los50archivos y todos los bytes de workflows/helpers/pruebas/HCL se conservan. YAML/Actionlint1.7.12/diff nuevos PASS; ShellCheck/Pyflakes externos no ejecutados. Gitleaks8.24.2 nativo/checksum verificado debe escanear los50archivos finales con0findings antes del stage; SHA256 posterior y comparación del índice con blobs normalizados garantizan que el commit usa exactamente las fuentes revisadas. No se publican temporales/recibos.

**OIDC→STS RUNTIME TEST=PENDING UNTIL PUSH antes de publicar; PENDING UNTIL GITHUB EXECUTION tras confirmar el push. SSM DOCUMENT=PENDING. AWS DEPLOY=NO. INITIALIZE EXECUTED=NO; historical initialize total1.** No se ejecuta STS mediante Publisher/Deploy, no se configura un cuarto workflow y no se habilitan flags. El push de feature por sí solo no coincide con los triggers CI/hardening: primera CI real requiere el PR autorizado posteriormente a develop. La presencia de YAML en la rama no prueba discovery global de Actions ni botón manual: GitHub requiere deploy-dev en la default branch para Run workflow, y no se modifica main en este gate. Reportar NOT YET OBSERVABLE si la API todavía no los descubre.

Recibos privados `%LOCALAPPDATA%/Temp/opencode/enver-cicd-review-2026-11-20/`: inspection/validation/scan/commit/push/final-closeout, manifiestos, tres refs antes/después y lectura REST de workflows en el SHA publicado. Se exige índice/worktree limpios y main/develop remotas intactas al terminar. **NEXT GATE=CREATE PR feature/ci-cd → develop AND VALIDATE REAL GITHUB CI**, no ejecutado automáticamente. Implementación local lista no equivale a diez capacidades COMPLETE, nueve gates satisfechos, OIDC runtime PASS o AUTH-027 acreditado.

## 17. Instrumentación diagnóstica segura — corte autorizado 2026-11-20 UTC

Publicación de §16 acreditada: commit `5af5f78876066da39724c44137769bf8914ad671`, rama feature/ci-cd remota, main/develop intactas. Solicitud posterior creó exactamente [PR#3](https://github.com/enverdelivery/ENVER-Backend/pull/3) hacia develop, sin merge. Primer [run44644574494](https://github.com/enverdelivery/ENVER-Backend/actions/runs/44644574494), evento pull_request/attempt1, **FAIL**:4jobs PASS/6FAIL/3skipped,13ejecuciones por matriz Linux/Windows de12jobs declarados. Build PASS/UT843de844PASS/1FAIL; runtime Linux restore/Windows build exit1, Terraform validate exit1, Dependency Review403. Integración omitida por build_test fallido; gate rechaza correctamente su resultado ausente. Hardening/deploy no descubiertos ni ejecutados en aquel corte; STS/SSM pendientes. No se atribuye la causa del403 a visibilidad o permisos sin evidencia.

La extracción del ZIP original completo confirma que los mensajes UT/comandos nunca se publicaron; `spawnSync` retuvo stdout/stderr en memoria y el fallo solo serializó herramienta/operación/exit. TRX privado no subido, JSON solo conteos. El gate EXTRACT ORIGINAL JOB LOGS se detuvo con **BLOCKED — INSUFFICIENT EVIDENCE**, sin cambios/commits. Recibos privados enver-pr-ci-2026-11-20 y enver-original-ci-logs-2026-11-20; no sustituir ese fallo por PASS local histórico.

Autorización nueva **ADD SAFE CI DIAGNOSTIC INSTRUMENTATION AND RE-RUN REAL GITHUB CI**: corregir solo pérdida de evidencia, pruebas focalizadas, validaciones ligeras, un commit nuevo `fix(ci): preserve sanitized failure diagnostics` y push exclusivo feature/ci-cd para CI automático del PR. No corregir UT/restore/build/validate/403, ampliar permisos, usar PAT/claves AWS, cambiar Quality Gate o producto. No AWS calls/SSM/EC2/CloudFront/ALB/apply/initialize/main o develop push/merge/dispatch. Histórico initialize1 preservado.

Cambios: nuevo diagnostics.mjs con redacción centralizada antes de persistir y recortar, allowlist de comandos/argumentos y cabeceras/campos REST, parser TRX diagnóstico sin outputs de aplicación. lib.mjs registra solo fallos aprobados y mantiene throw/BLOCKED/exit originales. pipeline.mjs añade diagnóstico de suite desde TRX fallido sin tocar assertions, comandos o lógica del gate. Composite ci-control añade subida exclusivamente JSON saneado de directorio dedicado, solo fallo/retención7días; controles requeridos y grafos12/10/10 intactos. Nuevos tests sintéticos de redacción/parser/HTTP/persistencia sin AWS; README operativo y estos dos Markdown sincronizados. No paquetes, librerías, políticas, tests C# o infraestructura modificados.

Límites:160líneas/16000caracteres por stream aprobado; test console solo códigos tooling, assertions/expected/actual/stack/source desde ErrorInfo del TRX; hasta20fallos/mensaje y stack6000caracteres, sin StdOut/StdErr del test, DTD/entidades o archivo mayor32MiB. JSON REST máximo64KiB, solo status/path y headers whitelist/message/documentation_url saneados, nunca respuesta arbitraria. Argumentos omitidos si desconocidos o si redacción detecta material sensible. JSON diagnóstico máximo512KiB. Se redactan valores sensibles conocidos del entorno sin imprimirlo, Authorization/cookies/Bearer, tokens GitHub/JWT, IDs AWS y asignaciones runtime/credenciales/query URI. Solo `enver-ci-diagnostics/*.json` entra al artefacto; fuentes privadas/raw TRX/env/state nunca se suben. No nuevos permisos ni workflows.

Pruebas focalizadas iniciales **48/48PASS/0FAIL/0skip**:15nuevas y33contratos helpers/OIDC existentes. La revisión agrega un caso de material opaco entre comillas sin ocultar identificadores de tests largos: catálogo final **49casos/16nuevos**, cuya ejecución final y YAML/Actionlint/secrets/diff se acreditan en `%LOCALAPPDATA%/Temp/opencode/enver-safe-ci-diagnostics-2026-11-20/`. No suites producto completas locales ni Terraform/AWS ejecutados en esta instrumentación; CI real hará sus controles autorizados. El commit/push y diagnóstico nuevo se verifican después mediante SHA, ref, URL/run y artefactos: este texto previo al commit no presume resultado ni corrige causas. **QUALITY GATE=UNCHANGED; OIDC→STS=PENDING; SSM DOCUMENT=PENDING; DEPLOY=NO; INITIALIZE=NO**. Siguiente gate solo con diagnóstico acreditado; ninguna capacidad integral COMPLETE, gate cloud o AUTH-027 cerrado por instrumentar.

## 18. Correcciones acreditadas de CI — reanudación 2026-11-20 UTC

Solicitud FIX ACCREDITED CI FAILURES, reanudada explícitamente tras límite de uso. Inicio limpio feature/ci-cd/448f8e246a5bfb0bfbf00999d5eb1bb7a952129b, sin diff/parciales que reutilizar, reset o checkout destructivo. Se reutilizan copias/evidencias/probe privados; patch temporal fallido no se presume aplicado ni se incorpora a Git. Run44647671094 FAIL/5jobsPASS/6FAIL/2skipped: UT844/844PASS, LinuxNU1100/fuentes vacías Bootstrap, WindowsCS0103/PipeCredential129/DLLCONTROL ausente, TerraformAWS6.73.0/checksum, IT279FAIL y ST109FAIL por shell Mongo durante setup. Diagnóstico saneado de448f8e2 conservado; Quality Gate correcto.

Cambios delimitados: restoreArguments limita la fuente pública NuGet existente a Bootstrap/Tests, sin modificar NuGet.Config offline de CONTROL, paquetes/locks, credenciales, integridad o ignore-failed-sources. Windows compila primero CONTROL del checkout para el HintPath existente de PipeProducer.Tests, sin inventar símbolos ni cambiar Program/tests/artefactos históricos. Mongo realiza setup previo a suites con pull de la misma imagen aprobada del fixture por digest y command-v/mongosh--version en contenedor sin red, exige2.14.0/fail-closed. Testcontainers4.16 busca shell dentro del contenedor, no del host: no se instala inútilmente en el host ni se presenta el preflight como prueba suficiente de corrección cloud. Assertions/fixtures/producto sin cambios.

Validación local en copia completa aislada/SDK instalado404; pin403 de repo/GitHub intacto. Restore/build CONTROL/PipeProducer.Tests y restore Bootstrap.Tests PASS, PipeProducer29/29PASS. UT844/844PASS/0skip: primer843/844 retenido fue FileNotFoundException de .dockerignore omitido por el preparador local; se completó la copia desde tracked y se repitió tras corregir ese impedimento comprobado, sin tocar tests. Terraform/smokeIT-ST/helpers/YAML/Actionlint/secretos/diff y manifiesto final en recibos privados enver-accredited-ci-fixes-2026-11-20. No se presume Linux/SDK403 por equivalencia Windows/404.

GitHub solo lectura: repo public/ADMIN, Actions enabled, dependencyGraphManifests accesible/1manifiesto; compare administrativo base2e89e3c/head448f8e2 devuelve200. Esto no acredita que el GITHUB_TOKEN del PR deje de fallar ni justifica ampliar permisos, usar PAT/static token o alterar seguridad. Dependency Review intacto/sin solución especulativa.

Autorizado un commit nuevo `fix(ci): resolve accredited pipeline failures` y push solo feature/ci-cd si las validaciones pasan, sin amend448f8e2, seguido de observación CI automático del PR#3. SHA/push/run en recibos posteriores al corte, sin segundo commit para insertar su propio SHA. NEXT GATE=VALIDATE REAL GITHUB CI RERUN; si persiste fallo acreditado, FIX REMAINING ACCREDITED FAILURE; si solo403, FIX DEPENDENCY REVIEW CONFIGURATION. Quality Gate/redacción/artifacts/permissions/OIDC intactos.0AWS calls/SSM/deploy/apply/initialize/merge/dispatch/main o develop push; histórico initialize1, STS/SSM/gates004/AUTH-027 pendientes.

**Cierre bloqueado local:** Terraform NO CORREGIDO/NO VALIDADO. Probe inicial sin init por chmod en montaje readonly; modificación aplicada solo al temporal para ejecutar copia del CLI en /tmp. Siguiente init directo Linux seguro/backendfalse/lockreadonly agotó240s durante descarga6.73.0, sin llegar a validate; contenedor propio detenido tras verificar su montaje, sin tocar otros contenedores. Descarga oficial independiente también agotó240s con ZIP parcial116833060bytes, no verificado/no usado. Disco cayó por debajo de1GiB; no se continúa descargas/extracciones ni se borra trabajo/evidencia/volúmenes para forzar validación. Lockfile/infra/CI Terraform intactos: no se inventa que evitar cache ya soluciona el problema ni se regenera lock sin prueba. Restore equivalente/build Windows/Pipe29/UT844/smokeIT1-ST1 y helpers53 pasan localmente, no acreditan los jobs Linux/SDK403. Validaciones ligeras finales en recibo privado; binaries de validators se recuperan de ZIPs retenidos verificando SHA, sin cambiar versiones. Commit=NONE, push=NOT EXECUTED, nuevoCI=NONE porque no se cumple condición de validación completa. NEXT GATE=FIX REMAINING ACCREDITED FAILURE; resolver margen de disco y completar diagnóstico/corrección Terraform, luego validar/publicar/observar. Gates004/AUTH-027/STS/SSM no cerrados.

## 19. Validación Terraform retomada — 2026-11-21 UTC

Responsable autoriza continuar solo Terraform pendiente y después revalidar/publicar un nuevo commit/observar CI, preservando los seis archivos preparados. Inicio HEAD448f8e2/feature/ci-cd, hashes de los seis idénticos al cierre§18; status/diff/diff-check inspeccionados, sin reset/amend/regeneración. Espacio observado1,12GiB, no se presume aumento comunicado: limpieza autorizada de archivos providerWindows generados/no tracked de infra/.terraform/providers y helper temporal, más descarga incompleta; sin junctions, fuentes, locks, state/backend/volúmenes o AWS. Recupera aproximadamente1,07GiB, margen2,21GiB antes de descarga. Retirada local no cambia infraestructura remota.

Descarga Linux AWS6.73.0 reanudada206 con plazo20min;176734675bytes/SHA256 coincide con zh ya comprometido y metadatos oficiales. Probe original sin ejecución por imagen auxiliar ausente; se usa imagen Mongo ya aprobada/cacheada como base Linux, entrypoint shell sin iniciar Mongo, sin red/credenciales/backend. Primer mirror tenía layout incorrecto, corregido solo temporalmente; evidencias de ambos intentos conservadas. Mirror local no se incorpora al CI; advertencia unauthenticated corresponde a mirror, cuya integridad fue comprobada contra el zh comprometido antes de usarlo. CI conserva instalación directa/verificación Terraform ordinaria, sin flags para omitir integridad.

**Causa precisa ahora reproducida:** init backendfalse/inputfalse/lockreadonly con paquete ZIP correcto/caché vacía da0, avisa que no guarda selecciones; validate da1 con el mismo mismatch observado en GitHub. Init normal en la copia aislada agrega únicamente h1Linux y validate da0. El lock original contiene zhLinux correcto, pero no h1Linux requerido para validar el paquete extraído; no se concluye que el archive ni el lock fuente deban reemplazarse. Guard `verifyValidationLock` preserva estructura/versión/constraints/providers y TODOS los hashes existentes; solo admite h1 adicionales bien formados, rechaza zh nuevos, eliminaciones, duplicados o modificaciones ajenas. Se comprueba además que lock del repositorio conserva sus bytes. El pipeline inicializa su snapshot privado fresco sin readonly y valida después del guard; nunca modifica el lock fuente ni reusa cache de estación/state/variables. Pruebas positivas/negativas del guard agregadas a ci-bootstrap.test.mjs; no se cambian gates, permisos o diagnósticos saneados.

**Terraform local PASS:** Linux_amd64, CLI1.17.1/provider6.73.0, fmt-check/init backendfalse/validate exit0, sin red y0AWS calls. Único cambio al lock **privado**: un h1Linux añadido; lock repo/infra intactos. Revalidación dirigida posterior de restore/build/UT/smokeIT-ST/shell y helpers/YAML/Actionlint/secretos/diff antes de publicar; SDK local404 equivalente, pin GitHub403 conservado. Resultados finales/manifiestos/SHA/push/URL/run se registran tras este corte en recibos externos enver-accredited-ci-fixes-2026-11-20, sin un segundo commit para insertar su SHA.

Revalidación posterior a Terraform acreditada: restore Bootstrap equivalente/Windows prerequisite-build/PipeProducer.Tests build-run exit0, UT844/844PASS/0skip, smokeIT1/1 y ST1/1PASS/0skip, mongosh2.14.0. Helpers55/55PASS/0skip (incluye redacción previa y2casos del guard con negativas), YAML12/10/10+composites/Actionlint1.7.12/secretos0findings/diff-checkPASS. ShellCheck/Pyflakes externos no ejecutados; no se repiten suites completas IT/ST no afectadas. Quality Gate byte por byte y diagnósticos/workflows/permissions/OIDC/C# tests/producto/infra sin cambios. Manifiesto final y validaciones tras esta sincronización documental en recibo privado; único commit/push aún condicional a esos mismos bytes.

Dependency Review **STILL BLOCKED / SEPARATE FOLLOW-UP**, sin solución especulativa, PAT/static token/ampliación de permisos o cambios de settings; no se bloquea publicación de fixes acreditados únicamente por ese403. Autorizado un commit nuevo `fix(ci): resolve accredited runtime and validation failures`, stage solo seis archivos previstos y push exclusivamente feature/ci-cd, PR#3 abierto/sin merge. NEXT GATE=VALIDATE REAL GITHUB CI RERUN; si solo403, FIX DEPENDENCY REVIEW CONFIGURATION; otros fallos se reportan sin corregirlos automáticamente en este incremento.0AWS API/SSM/deploy/apply/initialize/merge/dispatch/push main-develop; histórico initialize exactamente1. STS/SSM/gates004/AUTH-027 permanecen pendientes.

## 20. Windows Node y Mongo — incremento diagnóstico autorizado

**Fuente y autorización:** solicitud FIX REMAINING ACCREDITED FAILURE sobre run44847408590/PR3; inicio limpio feature/ci-cd/7db8789. Ese run terminóFAIL con Build&Test844/844PASS, LinuxRuntimePASS, IaCPASS, DependencyReviewPASS y WindowsNode/IT/ST/QualityGateFAIL. GitHub CLI recupera log Windows original: COMMAND_FAILED_node, sin test/assertion/stack; artifacts Windows anteriores contienen fixtures sintéticos de tests, no evidencia nativa. Tras explicar que Mongo no se reproduce y no cabe inventar fix, el responsable aprueba explícitamente **«Sí, diagnóstico incremental»**: publicar tras validación Windows reproducido+diagnóstico seguro Mongo, solo feature/ci-cd/CI automático, sin afirmar solución Mongo ni autorizar correcciones posteriores especulativas.

**Windows local:** mismo comando `node --test` y cuatro archivos scripts/tests en copias aisladas del checkout, WindowsX64/Node24.16.0: LF55/55PASS, CRLF54/55. Falla `workflows count names triggers and immutable action pins`, pipeline.test.mjs110, AssertionErrorERR_ASSERTION, regex `/group: enver-development-deploy\n  cancel-in-progress: false/` frente a YAMLCRLF. Stack/cwd/command en recibo saneado; no se presenta esa reproducción como stdout histórico ausente. Se normaliza únicamente la lectura de ese YAML; todas las assertions/gates/workflows se conservan. Resultado56/56PASS LF y CRLF (un test adicional del diagnóstico). Se habilita diagnóstico nativo acotado/redactado SOLO para comando Node--test de helpers con argumentos aprobados; contexto whitelist, nunca env completo. RUNNER_TEMP/evidence de helpers aislados impide subir sus fixtures sintéticos como fallos reales. Sonda negativa con YAML incorrecto SOLO en copia temporal confirma assertion nativa en command-1.json y fixtures sintéticos confinados al directorio hijo.

**Mongo investigado, NO CORREGIDO:** Testcontainers4.16.0 ejecuta `[/bin/sh,-c,command -v mongosh || command -v mongo]` mediante DockerExecAsync dentro de su contenedor y guarda el resultado una vez en Lazy. No usa child_process ni PATH del host; el preflight anterior era otro contenedor con entrypoint distinto, aunque mismo digest. Probes réplica-set reales con mismo módulo/imagen desde Windows y LinuxUbuntu24.04X64 confirman `/usr/bin/mongosh`,2.14.0/exit0 en tres execs del mismo contenedor y startupPASS. HostWindows local tiene mongosh2.3.8; su presencia no interviene en la llamada interna; hostPATH de GitHub desconocido. No hay evidencia para instalar host tooling, cambiar PATH del runner, sustituir shell, reintentar startup ni cambiar wait/assertions.

Diagnóstico agregado SOLO tras la excepción exacta, antes de eliminar el contenedor fallido: contexto fixture-container, plataforma/arch host, cwdIsRooted, exit del descubrimiento POSTERIOR, ruta únicamente de lista fija, booleano /usr/bin en PATH y exit/booleano versión2.14.0 al invocar shell explícito. Timeout5s/catch de diagnóstico; no stdout/stderr arbitrario, env/args con contraseña, conexión, containerlogs o credenciales. ErrorInfo conserva excepción original como inner y startupFAIL; si el exec posterior pasa, NO significa que el original haya pasado ni modifica el fallo. `originalDiscoveryExit=NOT_EXPOSED_BY_MODULE` explícito. Causa Mongo GitHub todavía NO DETERMINADA, fix NO ACREDITADO; el siguiente CI debe aportar evidencia, no se promete IT/ST verdes.

Validaciones locales/manifest/commit/push/run final en recibos externos `enver-node-mongo-failures-2026-11-21`. Se conserva limitación SDK404 local vs403 GitHub, sin cambiar pin/dependencias/lock ni productos/infra/permissions/OIDC/QualityGate. Un commit nuevo de Windows+diagnóstico, sin amend, branch feature/ci-cd; mensaje no afirmará que Mongo fue corregido. Publicación condicionada a controles relevantes; luego solo observación/readback de CI, sin otra corrección automática.0AWSAPI/SSM/deploy/apply/initialize/dispatch/merge/push main-develop; histórico initialize1, STS/SSM/gates004/AUTH-027 pendientes. NEXT GATE=FIX REMAINING ACCREDITED FAILURE o VALIDATE ENVER CI GREEN si CI real acredita todos los controles.

**Evidencia local al corte:** baseline completo anterior al cambio diagnóstico del fixture: UT844/844, IT311/311, ST223/223PASS/0skip; Orchestrator restore/build/portable-runPASS, PipeProducer runPASS. No se confunden esos resultados con un fixMongo o un PASSGitHub. Diagnóstico nuevo compilado en harness externo con fuente compartida actual, contrato positivo y PATH negativo sintéticoPASS: sinPATH, discovery127/versión explícita0/2.14.0, metadata sigueFAILED; no demuestra esa causa en CI. El primer harness temporal omitía FrameworkReference y suponía que versión explícita fallaría sinPATH; ambas correcciones solo en temporales, sin alterar producto/tests para hacerlos pasar. Helpers56/56 LF/CRLFPASS, capturaNode negativa e aislamientoPASS; YAML/Actionlint/secretos0findings/diff-checkPASS, ShellCheck/Pyflakes externos no ejecutados. Pruebas dirigidas con fixture modificado y manifiesto final, previos a commit, en recibos posteriores; resultados CI/SHA en recibos sin un segundo commit para insertar su propio SHA.

**Prueba dirigida y control adicional:** restore/build del fixture modificadoPASS, ITdirigido1/1PASS usando Mongo real. STdirigido `LoginSecurityTests.Issued_JWT_has_only_approved_claims_header_and_no_PII_or_secret_in_events` daFAIL503 frente a200, después de startup exitoso; se conserva ese recibo, NO se debilita assertion ni amplía presupuesto ni se afirma causa sin prueba. Se compara el mismo método con fixture anterior en la misma copia y se exige suite completa223/223 con diagnóstico nuevo antes de publicación; no hay retry automático ni cambio de orden/suite del CI para hacerlo pasar. Códigos de exec nulos o inesperados se etiquetanUNKNOWN, nunca se interpreta null como PATH ausente. Resultado diferencial/final y manifiesto en recibos externos, que condicionan el commit.

**Cierre local de la comparación:** fixture anterior en la misma copia/mismo método aislado tambiénFAIL503/200; no se atribuye ese resultado al diagnóstico nuevo ni se afirma su causa temporal. Suite completa con fixture diagnóstico223/223PASS/0skip, incluido ese método, sin cambios de test/presupuesto ni reducción de suite. ITdirigido con fixture modificado1/1PASS; baseline completo IT311/311 y UT844/844PASS. Refinamiento final limitado a metadatos de fallo (UNKNOWN explícito/estado enum del contenedor) compilado y validado con sondas positivas/negativas del archivo exacto cuyo SHA se conserva. La publicación diagnóstica exige manifiesto y controles ligeros finales PASS; el fallo dirigido previo permanece en recibos y en este historial.
