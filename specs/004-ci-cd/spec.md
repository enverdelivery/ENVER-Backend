# Especificación 004 — CI/CD de ENVER: integración automática y deployment DEV manual

## 1. Autoridad, aprobación y alcance de esta entrega

| Campo | Estado vigente |
| --- | --- |
| Versión | 0.2.0 |
| Fuente de aprobación | Decisiones finales del responsable: APRUEBO LA PROPUESTA DE 004-CI-CD |
| Estado documental | DISEÑO APROBADO PARA PREPARAR IMPLEMENTACIÓN; actualización documental autorizada |
| Estado técnico 004 | NO IMPLEMENTADO / NO VERIFICADO en GitHub Actions |
| Artefactos vigentes | Este spec, [plan.md](plan.md) y [tasks.md](tasks.md) |
| Catálogo vigente | 10 tareas de capacidad completa, T-CICD-101–110; todas PENDIENTE / NO EJECUTADA |
| Próximo gate | IMPLEMENT COMPLETE CI/CD PIPELINE; requiere autorización técnica posterior |

Aplican la [constitución canónica](../../.specify/memory/constitution.md), su [referencia](../../constitution.md), [AGENTS](../../AGENTS.md), [memoria](../../memoria.md) y [README](../../README.md). Esta aprobación posterior sustituye las decisiones incompatibles de 004 v0.1.0, sin reformar constituciones ni aprobar retrospectivamente otros specs.

**Este gate solo autoriza actualizar spec.md, plan.md y tasks.md de 004.** No autoriza crear workflows/scripts/tests, modificar código/configuración/dependencias, configurar GitHub/AWS/IAM, publicar imágenes, ejecutar pruebas/scanners/builds, initialize, Terraform, deployment, stage, commit o push. No se sincronizan memoria/README/AGENTS porque están fuera de los tres archivos autorizados; el registro de aprobación y alcance queda aquí.

**Catálogo único vigente:** tasks.md sustituye el catálogo singular task.md. El archivo singular preexistente se conserva intacto fuera del alcance de edición, únicamente como antecedente no normativo; sus tareas T-CICD-001–030, fases y reglas sustituidas no gobiernan trabajo nuevo. Los identificadores nuevos evitan reasignar evidencias o significados a IDs antiguos.

Clasificación: **APROBADO** expresa diseño exigible, no implementación; **OBSERVADO** expresa lectura/evidencia anterior, no una nueva verificación; **PENDIENTE OPERATIVO** expresa un dato o precondición de ejecución que no reabre una decisión aprobada. Se conservan 48 IDs de requisitos, 20 PLD y 16 AC, con texto vigente y nueva trazabilidad.

## 2. Objetivo, ramas y separación CI/CD

Flujo aprobado:

```text
feature/* → Pull Request → develop → Pull Request → main
                             │                     │
                     integración / DEV       estable / entrega
                             │
                    CI + hardening aplicable
                             │
                    candidata aprobada ECR
                             │
                       NO AUTO DEPLOY

Persona autorizada → Actions → Deploy ENVER Dev → Run workflow
                  → workflow_dispatch → candidata develop verificada
                  → A → readiness/target A → B → readiness/target B
                  → HTTPS health / smoke → resultado
                  → ante fallo: compensación dentro del MISMO run manual
```

- `develop` es la rama de integración y **único origen de candidatas desplegables a DEV** en este alcance.
- `main` es la rama estable/de entrega; no es requisito que una candidata DEV ya esté integrada a main.
- PR hacia develop/main y push a develop/main ejecutan CI según alcance. Solo un contexto confiable de develop puede publicar una candidata DEV. PR, forks y feature/* no publican ni despliegan.
- Un merge/push a develop puede ejecutar CI/hardening y publicar ECR; **jamás inicia deployment**, ni invocando automáticamente dispatch.
- El SHA de PR head, merge sintético, commit integrado en develop, revisión del workflow y posterior SHA de main son identidades distintas.

Alcance de API: register/login/logout bajo `/api/v1/auth/` y GET `/health`. No incorporar Orders, `/me`, administración, endpoints de diagnóstico o nuevas funcionalidades para satisfacer herramientas.

## 3. Baseline AWS y repositorio que deben preservarse

Baseline recibido del responsable y evidencia anterior en memoria, **sin nueva consulta AWS en este gate**:

| Componente | Referencia existente |
| --- | --- |
| EC2 A | `i-0a66085b7b2411393` |
| EC2 B | `i-00de11a3f3c1a8aa5` |
| Cuenta / región | `926925605666` / `us-east-1` |
| URL HTTPS | `https://d3htt8nzgp6ksn.cloudfront.net` |
| CloudFront distribution | `E2ZL0CKKMP8PIV` |
| ALB | `enver-dev-alb-1073601258.us-east-1.elb.amazonaws.com` |
| ECR | `926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api` |
| Target Group | `arn:aws:elasticloadbalancing:us-east-1:926925605666:targetgroup/enver-dev-api-tg/025d00fc6b95df06` |
| Manifest digest actualmente acreditado | `sha256:41483906445df7b1ad53de32dfc8b6c416bfd53c17cf043cd5a33476c357a856` |
| Runtime EC2 | Rol `enver-dev-api-runtime` mediante IMDSv2 |

Preservar OriginSecret existente, JWT secrets, autoridad Dynamo, generación de cuotas, CloudFront mode habilitado, AllowLocalHttp=false, ALB, CloudFront, red y contratos runtime. El deploy normal cambia únicamente la referencia de imagen mediante el mecanismo existente, materializa configuración ordinaria y reinicia controladamente; no recrea infraestructura ni ejecuta Terraform apply o initialize de autoridad/cuotas. Materialización ordinaria no equivale a otro initialize. INITIALIZE HISTORICAL TOTAL=1 y G APPLIED son antecedentes a conservar, no contadores reiniciables por pipeline.

Se conservan binding `0.0.0.0:8080`, unidad systemd, wrappers/overlay de configuración existentes, journald, identidad non-root y montaje runtime read-only desde tmpfs con permisos vigentes. No reinyectar/regenerar secretos ni sobrescribir el wrapper remoto con una plantilla antigua sin overlay. No copiar runtime JSON ni secretos a artifacts.

La URL pública tiene HTTPS; la aceptación anterior de saltos de origen HTTP es **solo DEV/académico y NO END-TO-END TLS**. No añadir ACM/dominio/Route53 como prerrequisito inventado ni presentar esa aceptación como seguridad de producción.

Estado local observado: SDK [global.json](../../global.json) 10.0.403/rollForward disable; solución net10.0 con UT/IT/ST; [fixture](../../tests/Shared/AuthenticationDependenciesFixture.cs) Mongo replica set, Redis noeviction, DynamoDB Local y Ryuk por digest; herramientas runtime con tests propios fuera de la solución; `infra/` existente; workflows aún ausentes. No se repiten pruebas anteriores ni se fijan sus conteos como mínimos perpetuos. La imagen actual y sus evidencias manuales **no adquieren automáticamente aprobación CI de 004**.

## 4. Workflows, eventos y jobs visibles

Máximo **tres workflows**, nombres y responsabilidades aprobados:

| Archivo futuro | Nombre | Eventos / límites |
| --- | --- | --- |
| `.github/workflows/ci.yml` | ENVER CI | pull_request hacia develop/main y push a develop/main; todos sus jobs sin AWS |
| `.github/workflows/hardening.yml` | ENVER Hardening | Candidata publicable de develop, manual o periódico según aplicabilidad/coste; solo Publisher puede usar AWS ECR |
| `.github/workflows/deploy-dev.yml` | Deploy ENVER Dev | **Únicamente workflow_dispatch humano** |

No push, pull_request, workflow_run, schedule, release/tag o workflow_call como trigger de deploy; ningún otro workflow lo invoca automáticamente. El rollback no dispara otro workflow. Configuración del botón requiere que deploy-dev.yml exista en la default branch real; la ejecución DEV se selecciona desde `develop` confiable. La default branch puede ser main y su función de descubrimiento **no cambia develop como rama de deployment**. Rechazar refs arbitrarios antes de AWS; verificar estas condiciones al implementar, sin asumir la configuración real de GitHub.

### FAST PR GATE: 12 jobs separados

| Job visible | Función exigida |
| --- | --- |
| Detect Changes | Clasificar API/tests/shared/dependencias/runtime/container/IaC/workflows; diff incierto ejecuta controles |
| Build & Test | SDK exacto, restore locked, build Release, UT completas y resultados nativos |
| Mongo / Redis / Dynamo Integration Validation | IT y ST completas mediante fixtures seguras existentes; no SQL Server |
| Runtime Tool Tests | Cubrir herramientas de materialización/cuotas según alcance y sus comandos reales |
| Semgrep SAST | Reglas rápidas C# y específicas de ENVER |
| Dependency Review | Cambios de dependencias y NuGet audit del grafo completo aplicable |
| Secret Scan | Gitleaks, resultados redactados y alcance explícito |
| Dockerfile / Container Lint | Hadolint y revisión del contexto/configuración de contenedor |
| IaC Validation | fmt-check, validate y Trivy IaC para infra existente; sin state remoto ni apply |
| Contract Tests | Contrato real API/Auth; pruebas/aserciones verificables del mismo SHA/run, sin Pact inventado |
| Docker Build | Una imagen linux/amd64; artefacto OCI íntegro; **no push en PR** |
| PR Quality Gate | Agregador fail-closed de todos los controles aplicables, siempre visible |

### HARDENING GATE: 10 jobs separados

| Job visible | Función exigida |
| --- | --- |
| Verify Candidate | Verificar origen CI, SHA/run/intento, artefacto y digest exactos |
| Container Vulnerability Scan | Trivy sobre la misma imagen candidata; config non-root/contexto e inventario |
| CodeQL SAST | Análisis profundo compatible C#/.NET 10; findings evaluados, no solo upload |
| Mutation Testing | Stryker.NET; alcance inicialmente lógica pura/validadores; ejecución manual/periódica o candidata según coste |
| Dynamic Target Preflight | Autorizar/validar laboratorio desechable, contrato y presupuesto antes de tráfico dinámico |
| DAST OWASP ZAP | Objetivo seguro aislado y límites explícitos |
| REST API Fuzzing | RESTler con OpenAPI real validado, primero compile/test y luego fuzz acotado |
| Hardening Report | Consolidar CI/tests/SAST/dependencias/secretos/contenedor/IaC/mutation/DAST/fuzzing, incluso ante fallos |
| Hardening Gate | Evaluar política y estados; generar reporte no implica aprobar |
| Publish Approved Candidate | Solo candidata develop acreditada y gates requeridos aprobados; OIDC/ECR separado |

No concentrar todo en un job. `needs:` expresa dependencias dentro de cada workflow. No fingir un `needs` entre ejecuciones independientes. El enlace entre CI y hardening verifica workflow ID/revisión, evento, rama, SHA, run/intento y artefactos exactos; no consume arbitrariamente el último artifact disponible.

## 5. Requisitos CI-001–012

| ID | Requisito aprobado |
| --- | --- |
| CI-001 | CI automático en PR develop/main y push develop/main; nunca deployment/apply automático. Mantener feature/*→PR→develop→PR→main. |
| CI-002 | Validar SHA exacto integrado en develop para candidatas DEV; no heredar el verde de otro SHA, de PR o main. |
| CI-003 | SDK 10.0.403 desde global.json, allowPrerelease=false y rollForward=disable; sin fallback silencioso. |
| CI-004 | Restore locked para proyectos con lockfile; discrepancias fallan, no regenerar lockfiles ni actualizar paquetes para obtener verde. Inventariar restauración explícita de herramientas externas a la solución según su contrato real. |
| CI-005 | Build Release sin restore implícito posterior; registrar warnings efectivos sin nuevas supresiones o cambios arbitrarios de política. |
| CI-006 | Ejecutar UT existentes completas del SHA cuando aplican; suite no vacía y conteo dinámico, sin mínimos históricos fijos. |
| CI-007 | Ejecutar IT completas con Mongo/Redis/DynamoDB Local/Testcontainers existentes, sin usar infraestructura desplegada ni mocks como acreditación de integración. |
| CI-008 | Ejecutar ST completas y tests runtime pertinentes, incluidos ejecutables fuera de slnx con su comando real; no confundir ST con scanners. |
| CI-009 | Exitcode nativo cero, cero failed/skipped y resultados completos por suite; timeout/cancelación/suite ausente/resultado vacío no son PASS. |
| CI-010 | Runner efímero aislado con Docker, readiness/cleanup/Ryuk y namespaces propios; no ampliar presupuestos SUT o repetir suites para esconder fallos. |
| CI-011 | Jobs y gates visibles separados, condiciones de alcance acreditadas y agregación fail-closed. Path filtering nunca deja un SHA desplegable sin prueba completa aplicable. |
| CI-012 | Validación de infra existente: fmt-check, init de validación sin backend/credenciales AWS cuando viable, validate y scan IaC; sin plan privilegiado/apply/state expuesto. |

No duplicar contenedores mediante servicios paralelos a las fixtures. Cada runner acredita binarios del mismo SHA/SDK/configuración antes de usar no-build/no-restore. Cache es optimización, no evidencia. Preservar límites de las fixtures y recoger diagnósticos saneados antes del cleanup.

Contract Tests cubre únicamente register201, login200/Bearer/expiresIn900, logout204/cuerpo vacío, health200/texto Healthy, JSON estricto, ProblemDetails, no-store y challenge. Puede reutilizar evidencia TRX contractual íntegra del mismo run sin repetir IT pesadas, pero debe comprobar que las aserciones identificadas se ejecutaron; un archivo existente o resumen global no basta. No alterar DTOs ni introducir Pact sin una necesidad nueva aprobada.

## 6. Seguridad SEC-001–012 y política de ejecución

| ID | Requisito aprobado |
| --- | --- |
| SEC-001 | Dependency Review más NuGet audit directo/transitivo; revisar vulnerabilidades, soporte y alcance de licencias según capacidades verificadas. |
| SEC-002 | Gitleaks sobre fuente/cambios/historia y artefactos según política; nunca imprimir el secreto encontrado ni confiar solo en masking. |
| SEC-003 | Semgrep rápido y CodeQL profundo compatibles con .NET 10; no añadir SonarCloud redundante. |
| SEC-004 | Hadolint para Dockerfile, Trivy para imagen final e IaC; comprobar que el manifest digest analizado es el publicado/desplegado. |
| SEC-005 | Política procesable de severidades, findings, feeds, antigüedad y excepciones; fallo de herramienta/resultado desconocido bloquea el control requerido, sin continue-on-error para aparentar éxito. |
| SEC-006 | CI/PR sin AWS, id-token AWS, secretos runtime ni publicación; no elevar código de PR mediante pull_request_target, workflow_run o artefactos/cachés no confiables. |
| SEC-007 | **OIDC obligatorio**, IAM roles dedicados y credenciales temporales; no estrategia de claves AWS permanentes ni fallback a ellas. |
| SEC-008 | Separar CI sin AWS, Publisher ECR, Deploy ECR-read/SSM-A-B/target-health-read y Runtime EC2 actual; sin rol administrador, Terraform ni bootstrap desde deploy. |
| SEC-009 | GITHUB_TOKEN mínimo por job: contents read, actions read donde consulta evidencia, id-token write solo en jobs OIDC. Writes para SARIF/historial solo si justificados; no packages write por usar ECR. |
| SEC-010 | Runtime conserva gestión existente de secretos; pipeline no lee/modifica OriginSecret/JWT/Atlas/Redis/HMAC ni los empaqueta. Credenciales sintéticas y tokens de smoke solo en memoria, fuera de logs/artifacts. |
| SEC-011 | Acciones críticas fijadas a SHA completo, herramientas/imágenes a versiones verificadas; validar inputs y procedencia/integridad de artefactos; sin interpolación shell de datos no confiables. |
| SEC-012 | Trust limitado a repo/contexto/environment/branch autorizados, formato real aud/sub y control de workflow/actor; proteger evidencia, refs y revisiones del pipeline. |

Herramientas aprobadas **solo cuando compatibles/aplicables**: Semgrep, CodeQL, Dependency Review, NuGet audit, Gitleaks, Hadolint, Trivy, Stryker.NET, OWASP ZAP y RESTler. Versiones concretas, disponibilidad SDK/runner y licencias se verifican antes de habilitarlas; una licencia ausente o incompatibilidad de control requerido es BLOCKED, no NOT APPLICABLE ni PASS por conveniencia. No imponer compras ni sustituciones silenciosas.

Política aprobada de base: secretos detectados y findings High/Critical bloqueantes; demás severidades, reglas equivalentes entre herramientas, licencias permitidas, vigencia de feeds/análisis y umbral mutation se concretan en configuración revisada antes de declarar candidata aprobada. Excepciones requieren responsable, alcance, justificación y vencimiento; no borran el finding ni eximen identidad/manualidad/tests.

### Estados obligatorios y dos niveles

Cada control informa **PASS / FAIL / NOT APPLICABLE / NOT EXECUTED / BLOCKED**. NOT APPLICABLE requiere condición demostrada y política versionada; NOT EXECUTED es ausencia de ejecución; BLOCKED identifica impedimento; ninguno se convierte en PASS. Fallo, cancelación, skip inesperado, timeout o informe ausente de un control obligatorio impiden aprobar.

FAST PR GATE contiene los 12 jobs rápidos de §4. HARDENING GATE ejecuta controles pesados solo manual/periódicamente o para candidatas publicables, según coste/aplicabilidad. La política declara qué controles requiere cada evento/candidata y cuáles son periódicos; no desactiva silenciosamente obligatorios. CodeQL y scan de imagen son requeridos para promoción cuando su compatibilidad/disponibilidad esté acreditada; si no, candidata BLOCKED hasta resolución aprobada. Mutation/DAST/RESTler pueden tener calendario propio, conservando estado/alcance explícitos y sin afirmar hardening completo cuando falten.

Filtros internos, no supresión total del workflow/gate requerido: cambios global.json/csproj/locks/config/workflows/scripts activan controles dependientes; API/shared activa UT/IT/ST/contratos; tools activa sus tests; Docker activa build/lint/scan; infra activa validación IaC. Diff incierto activa controles. Cambios documentales pueden ser NOT APPLICABLE con evidencia y no generar candidata. Feed nuevo exige rescans periódicos aunque no cambie fuente. No prometer tiempos CI sin medición ni omitir pruebas para cumplir una meta arbitraria.

### Laboratorio dinámico

ZAP/RESTler nunca atacan producción ni el DEV compartido `https://d3htt8nzgp6ksn.cloudfront.net`, ALB o A/B. Requieren servidor HTTP real en laboratorio desechable, dependencias locales aisladas, datos/secretos sintéticos propios, allowlist de destinos, redirects confinados, límites de tiempo/volumen/concurrencia y cleanup. No debilitar seguridad o cuotas para producir cobertura aparente. No exponer IMDS/AWS/red interna a los scanners.

MapOpenApi no acredita un contrato completo: validar schemas/respuestas/Bearer/logout contra controladores que escriben respuestas manualmente antes de RESTler. Primero compile/test, luego fuzz acotado. Objetivo o OpenAPI inadecuado producen BLOCKED/NOT EXECUTED y nunca PASS; su impacto en promoción depende de la política explícita, sin inventar endpoints.

## 7. Imagen y promoción REL-001–012

| ID | Requisito aprobado |
| --- | --- |
| REL-001 | **Build image once**, linux/amd64, mismo SHA/config/SDK/locks, contexto sin secretos y runtime non-root. |
| REL-002 | Identidad por manifest digest inmutable, tag trazable SHA/run/intento; no latest ni tags mutables como prueba. |
| REL-003 | Checks aplicables antes del build; hardening sobre la misma imagen; solo Publisher confiable de develop publica ECR. PR no hace push. |
| REL-004 | Cadena repo/SHA/workflow/revisión/CI-run/intento/artefacto/digest/policy/evidencias/actor/instancias verificable, no un JSON autodeclarado por PR. |
| REL-005 | Preservar/verificar manifest digest entre artefacto OCI, scan, ECR y deployment. Prohibido rebuild entre CI, hardening, ECR y deploy; discrepancia bloquea. |
| REL-006 | Reutilizar exclusivamente baseline §3; no recrear infraestructura, ALB, CloudFront, ECR o autoridad desde pipeline de aplicación. |
| REL-007 | Deploy es único escritor de referencia de versión activa; Terraform no debe revertirla por reconcile. No editar state/tfvars como atajo. |
| REL-008 | Registrar/retener digest anterior aprobado y compatible antes de mutar para rollback compensatorio en el mismo run manual; no revertir estado de seguridad. |
| REL-009 | Serializar todo el run deploy/compensación DEV, cancel-in-progress=false, coordinación con mantenimiento y reconciliación tras interrupción. |
| REL-010 | Evidencia íntegra/saneada de build/tests/scans/aprobación/ECR/deploy/rollback; reportes y fallo original conservados. |
| REL-011 | Controlar coste/minutos/retención/egreso/licencias y ejecución selectiva; no prometer gratuidad ni usar coste como bypass. |
| REL-012 | CI/deploy no cierra por inferencia gates 001/002/003 ni AUTH-027; separar capacidad CI/CD, éxito operacional DEV y aceptación distribuida. |

Flujo: construir una vez → exportar OCI y hash de transporte → analizar misma imagen → publicar mismos blobs/manifiesto → verificar ECR → emitir registro de candidata aprobada. Una compilación de instrumentación CodeQL o de tests no es otra construcción de imagen. Pérdida de artefacto no autoriza rebuild oculto para fingir identidad anterior.

Registro obligatorio: repository/branch/commit SHA, workflow ID/revisión, CI/hardening run IDs e intentos, manifest digest, hash de artefacto y evidencias, policy version, estado por control y vigencia. Publicación no ejecuta scripts contenidos en artefactos no confiables. Un rerun no sustituye silenciosamente SHA→digest: nueva identidad explícita o rechazo. Guardar versión previa; firmas/SBOM adicionales no se afirman existentes ni son requisito de herramienta añadido por esta entrega.

## 8. Deployment CD-001–012

| ID | Requisito aprobado |
| --- | --- |
| CD-001 | deploy-dev.yml separado, nombre Deploy ENVER Dev, único trigger workflow_dispatch humano; nunca deploy por merge/push/PR/workflow_run/schedule. |
| CD-002 | Botón Actions→Deploy ENVER Dev→Run workflow; registrar actor/ref/SHA/run/digest/environment, sin bots automatizando dispatch. |
| CD-003 | Seleccionar automáticamente última candidata develop aprobada y vigente; verificar CI/hardening exactos, custodia y ECR; no pedir digest manual ni usar último run success genérico. |
| CD-004 | Workflow presente en default branch para botón y ejecución desde develop confiable; comprobar repo/ref/revisión/actor antes de AWS; main conserva función estable, no origen DEV. |
| CD-005 | GitHub Environment development y protecciones/branch rules disponibles; no destinations/roles/URLs arbitrarios ni segunda aprobación obligatoria no soportada por el plan. |
| CD-006 | Exclusión del run completo, selección congelada y revalidación previa a mutación; guardar digest anterior compatible/validado. |
| CD-007 | Deploy A→readiness A→target A healthy→deploy B→readiness B→target B healthy; digest y binding efectivos, servicio/contenedor y polling acotado. ACK de SSM no es éxito. |
| CD-008 | CloudFront HTTPS /health obligatorio; run_smoke_tests controla únicamente smoke AUTH sintético, no preflight/readiness/targets/health. |
| CD-009 | Mantener exigencia de evidencia distribuida de 002/003 separada: no dar PASS a revocación cruzada por logout repetido, health o round-robin supuesto. |
| CD-010 | Fallo real de activación/verificación/smoke tras mutación marca deployment FAILED e inicia la compensación aplicable; estado AWS incierto requiere reconciliación antes de otra mutación. |
| CD-011 | **AUTOMATIC COMPENSATING ROLLBACK dentro del MISMO workflow manual**: fallo A restaura solo A; fallo B restaura B y luego A si cambió, con validación. Original siempre FAILED aunque rollback pase. |
| CD-012 | Sin Terraform apply, initialize o recreación de infraestructura en deployment/compensación; sin cambios OriginSecret/JWT/autoridad/cuotas/ALB/CloudFront. |

Inputs simples: environment limitado a `development` y run_smoke_tests booleano (true por defecto). Sin digest/ARN/comandos/host libres. La última candidata se ordena por commit integrado elegible alcanzable desde develop, no por un run ajeno más reciente. Congelar commit/digest/run/intento; no cambiar de candidata a mitad aunque avance develop. Si ninguna cumple, rechazar sin tocar A/B. El SHA sintético de PR nunca es candidato automático.

Preflight: validar dispatch/contexto → localizar CI/hardening confiables del mismo SHA → verificar artefacto/registro/policy → OIDC temporal para lectura ECR → comprobar cuenta/región/URI/digest → exclusión/revalidación → comprobar A/B y referencia anterior homogénea/compatible → mutaciones. La imagen previa manual requiere incorporación verificada al inventario de rollback; no inventar evidencia CI para ella. Si no existe retorno aprobado/usable, bloquear antes de A.

## 9. Identidades, permisos y ejecución remota

| Contexto | GitHub | AWS permitido |
| --- | --- | --- |
| CI/tests/scanners/PR | contents read y permisos específicos de reporte justificados; sin OIDC AWS | Ninguno |
| Publish Approved Candidate | contents/actions read según necesidad, id-token write solo en ese job | Rol Publisher: ECR necesario, sin SSM/deploy |
| Preflight/deploy/compensación | contents/actions read, id-token write solo en jobs AWS, environment development | Rol Deploy: ECR read, SSM A/B y lecturas de estado/target health estrictamente necesarias |
| EC2 runtime | No obtiene secretos desde GitHub | Rol actual enver-dev-api-runtime / IMDSv2 |

OIDC emisor `https://token.actions.githubusercontent.com`, aud `sts.amazonaws.com`, sub exacto del repo/contexto real, incluidos IDs inmutables si aplican. Con environment, sub no demuestra develop por sí solo: complementarlo con branch restrictions, control de revisión del workflow y protección de cambios. Sin wildcards de repo/ref amplios, write-all, claves AWS permanentes o privilegios Terraform/IAM/secret-read del deploy. Tokens OIDC/STS no se registran ni transfieren entre jobs; cada job obtiene su sesión propia.

SSM necesita contrato restringido/versionado para A/B, digest validado y script confiable, no parámetros shell arbitrarios. No usar el rol local enver-infra en runners ni inferir permisos PASS por diseño. Roles/ARN/trust/subject/configuración real quedan pendientes de comprobación y autorización IAM aparte; este gate no los crea. Si falta permiso o evidencia, BLOCKED, nunca prueba mediante creación de recursos ni simulación obligatoria de IAM.

## 10. Estado operacional y compensación

Estados del run: preflight rechazado sin mutaciones; avance A; A verificado; avance B; B verificado; verificaciones finales; SUCCEEDED o FAILED. Registrar por nodo despacho/CommandId, confirmación, versión anterior/actual y mutación intentada/observada; persistir metadatos no secretos antes de mutar. No asumir que timeout significa comando no ejecutado.

| Fallo | Estrategia obligatoria dentro del mismo run iniciado manualmente |
| --- | --- |
| Preflight sin mutaciones | Rechazar; no tocar A/B ni ejecutar rollback innecesario |
| Activación/readiness/target A | **No tocar B**. Reconciliar A; intentar restaurar A al digest anterior aprobado, validar servicio/contenedor/digest/binding/health/target. Si ya permanece anterior, validar sin restart redundante. Original FAILED |
| A pasó, activación/readiness/target B falla | Detener avance; reconciliar; **restaurar B primero**, luego **A si quedó nueva**. Validar A/B, digest previo, targets y CloudFront /health. Original FAILED |
| Health final o smoke AUTH solicitado falla tras actualizar ambas | Mismo retorno B→A y validaciones; no reintentar POST para fabricar éxito. Original FAILED |
| Compensación falla o no puede reconciliarse con seguridad | Registrar rollback FAIL/BLOCKED y nodos afectados, intentar pasos restantes solo si son seguros; detener y exigir recuperación autorizada. No reset de seguridad ni bucle de restarts |
| Runner perdido/cancelado o SSM aún en curso | No prometer que GitHub siempre ejecuta cleanup. Estado de recuperación incierto, sin workflow automático nuevo; siguiente intervención exige reconciliar comandos/instancias antes de mutar |

La compensación se programa mediante dependencias/condiciones de fallo y finalización que no queden omitidas por needs fallido. Reconciliar hasta terminalidad o acotar incertidumbre antes de enviar órdenes que puedan solaparse. Presupuesto acotado por nodo: una activación y, si corresponde, un intento de restauración; polling de lectura no es retry de mutación. No reejecutar ciegamente comandos consumidos al reanudar un run.

Validar al terminar rollback: versión anterior, configuración preservada, A/B cuando proceda, target health y HTTPS CloudFront /health. `deploymentOutcome=FAILED` se mantiene irreversible para ese intento; `rollbackOutcome` se informa por separado. Summary/cleanup verdes no cambian exitcode final. Un éxito de rollback no borra registros de fallo.

Grupo fijo `enver-development-deploy`, cancel-in-progress=false para el workflow entero, incluido rollback. No prometer FIFO/cola durable; solicitudes pendientes pueden ser reemplazadas según semántica de Actions. Coordinar mantenimiento externo, detectar estado no reconciliado y revalidar cuando llega turno; una exclusión Actions no impide por sí sola un SSM anterior huérfano. No añadir permisos de drenaje/modificación del TG/ALB para simular rolling: la estrategia inicial secuencial usa readiness/health existentes; cualquier cambio de topología requiere autorización separada.

## 11. Readiness, smoke y límites de aceptación

Readiness por nodo combina systemd active, contenedor RUNNING, manifest/config identity cuando corresponda, binding esperado, health local/privado200 y target healthy, con límites de espera y tolerancia de carreras iniciales. `/health` es liveness y **no acredita dependencias listas**. No inventar `/ready` ni afirmar una readiness endpoint inexistente.

Tras A/B, CloudFront /health200/Healthy con TLS validado es obligatorio. Si run_smoke_tests=true: un flujo register201/login200/logout204 con identidad sintética no personal, cuotas respetadas, password/token solo en memoria, sin impresión de cuerpos sensibles/retries POST. Aprobar política de retención/limpieza sin añadir endpoint de borrado o permisos de escritura DB al deploy. Si false, registrar AUTH NOT EXECUTED y no afirmar smoke validado.

Ausencia de superficie protegida normal/atribución A-B impide afirmar rechazo distribuido post-logout o cerrar AUTH-027/GATE-I-I. No añadir `/me`/Orders ni usar logout204 repetido como sustituto. Éxito operacional del deployment DEV y gates de capacidad CI/CD se registran separadamente de la aceptación distribuida integral de 002/003, que continúa pendiente hasta su propia evidencia y aprobación.

## 12. Criterios de aceptación y gates

Los AC son diseño aprobado, **todos pendientes de ejecución/verificación**. Gates canónicos en [tasks.md](tasks.md#3-gates-formales), también pendientes.

| AC | Resultado exigido | Requisitos | Gate |
| --- | --- | --- | --- |
| AC-CICD-01 | Tres workflows, ramas aprobadas, jobs visibles y ningún automático despliega | CI-001/002/011; CD-001/002 | CI-A |
| AC-CICD-02 | SDK exacto, restore/build Release y política efectiva de warnings | CI-003–005 | CI-B |
| AC-CICD-03 | UT/IT/ST/runtime/contratos completos aplicables, resultados íntegros y cero omisiones internas | CI-006–011 | CI-C |
| AC-CICD-04 | Fixtures Mongo/Redis/Dynamo Local aisladas, cleanup y diagnósticos seguros | CI-007/008/010; SEC-006 | CI-C |
| AC-CICD-05 | Scanners/policy/estados correctos, hardening selectivo y laboratorios seguros | SEC-001–005/011; CI-011 | CI-D |
| AC-CICD-06 | Imagen una vez; mismo manifest digest scan→ECR→deploy, sin PR push ni rebuild | REL-001–005; SEC-004 | CI-E |
| AC-CICD-07 | OIDC obligatorio, separación de roles, CI sin AWS y trust/contexto mínimos | SEC-006–012 | CD-A |
| AC-CICD-08 | Botón manual, develop/DEV, selección automática acreditada y congelada | CD-001–006; REL-004/005 | CD-B |
| AC-CICD-09 | IaC estática sin apply ni dos escritores de versión, baseline preservado | CI-012; CD-012; REL-006/007 | CI-D / CD-B |
| AC-CICD-10 | A primero y B solo tras A PASS/healthy; polling y digest efectivos | CD-007/010; REL-005/006 | CD-C |
| AC-CICD-11 | HTTPS health final y smoke AUTH según input, privacidad y estados explícitos | CD-008; SEC-010; REL-010 | CD-C |
| AC-CICD-12 | Límites de prueba distribuida explícitos; sin falso PASS ni cierre de AUTH-027 | CD-009; REL-012 | CD-C / CD-D |
| AC-CICD-13 | Fallos/preflight/runner/API/smoke nunca producen deployment exitoso | CD-003/010/012; SEC-005 | CD-B / CD-D |
| AC-CICD-14 | Compensación A o B→A en mismo run manual, validación y original FAILED siempre | CD-011; REL-007–009 | CD-D |
| AC-CICD-15 | Evidencia y reportes íntegros/saneados, retención/coste y actor/run/digest | SEC-011/012; REL-004/010/011 | CD-D |
| AC-CICD-16 | Trazabilidad completa y separación entre documentación, implementación y aceptación real | REL-012; CI-011; CD-009 | Todos |

CI-A etc. abrevia GATE-CI-A, y CD-A etc. GATE-CD-A. Ningún gate requiere haber cerrado la prueba que él mismo debe permitir realizar; las capacidades de seguridad sí deben estar disponibles antes de una mutación autorizada.

## 13. Pendientes operativos y cierre documental

No quedan pendientes las decisiones develop/main, manualidad, tres workflows, herramientas seleccionadas condicionalmente, OIDC obligatorio, build once o compensación automática en el mismo run. Pendientes **operativos**, no autorización para resolverlos en este gate:

| ID | Dato/configuración/evidencia pendiente | Impacto |
| --- | --- | --- |
| DP-CICD-01 | Owner/repo/default branch reales, actores y protecciones de develop/main/workflows | Contextos y botón |
| DP-CICD-02 | Plan/visibilidad GitHub, environment development, costes y retención | Disponibilidad de funciones/evidencia |
| DP-CICD-03 | Runner/SDK exacto/Docker/recursos y tiempos medidos | Ejecución fiable |
| DP-CICD-04 | Inventario real de proyectos/ejecutables/resultados y cobertura contractual | Suites completas |
| DP-CICD-05 | Versiones/pinning/licencias, feeds/vigencia, umbrales específicos y calendario hardening | Gates de seguridad efectivos |
| DP-CICD-06 | Esquema/custodia/versionado de registro de candidata y validación por APIs | Identidad build once |
| DP-CICD-07 | Proveedor OIDC/ARN roles/aud/sub reales, scopes/documento SSM restringido | Publicación/deploy; IAM requiere permiso aparte |
| DP-CICD-08 | Compatibilidad de referencia activa/overlay runtime y contrato de reconciliación de 003 | Preservación sin drift |
| DP-CICD-09 | Coordinación con mantenimiento externo, exclusión y comandos remotos en curso | No concurrencia/recuperación |
| DP-CICD-10 | Identidad sintética, cuotas, retención/limpieza y presupuesto smoke | Smoke AUTH seguro |
| DP-CICD-11 | Laboratorio HTTP/OpenAPI para ZAP/RESTler y superficie/atribución distribuida autorizadas | DAST/fuzzing y aceptación distribuida |
| DP-CICD-12 | Disponibilidad y prueba de permisos ECR/OIDC existentes sin recreación ni recursos de prueba | Primera promoción |
| DP-CICD-13 | Retención/aprobación del digest anterior y compatibilidad para compensación, alertas/evidencia | Retorno seguro antes de A |
| DP-CICD-14 | Verificación de grafo/condiciones/estados negativos/cancelación y revisión de supply chain | Ausencia de falso verde |

Riesgos a verificar: SHA confundido, privilegios PR, artifacts alterados, feeds caducados, secretos en TRX, scanner no ejecutado presentado verde, falsos positivos de revocación, SSM incierto/repetido, runner perdido durante rollback y versión activa sobrescrita por mantenimiento. Ninguno se resuelve solo escribiendo YAML.

Referencias de diseño: [dispatch/default branch](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow), [OIDC AWS](https://docs.github.com/en/actions/how-tos/secure-your-work/security-harden-deployments/oidc-in-aws), [CodeQL compatibilidad](https://codeql.github.com/docs/codeql-overview/supported-languages-and-frameworks/), [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/getting-started/), [RESTler](https://github.com/microsoft/restler-fuzzer). No se consulta ni ejecuta infraestructura para acreditarlas en esta entrega.

**Resultado documental:** SPEC UPDATED; PLAN UPDATED; TASKS UPDATED; deploy branch develop; main stable; AWS deployment MANUAL workflow_dispatch; rollback AUTOMATIC COMPENSATING WITHIN MANUAL DEPLOYMENT; workflows3; OIDC REQUIRED; BUILD ONCE / DEPLOY SAME DIGEST; AWS MODIFIED NO. **NEXT GATE: IMPLEMENT COMPLETE CI/CD PIPELINE. STOP documental; no implementación técnica iniciada.**
