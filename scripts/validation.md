# Evidencia local 004 — 2026-11-19

**Registro histórico de la primera implementación.** La corrección posterior autorizada FIX PIPELINE y su evidencia vigente están en [fix-pipeline-validation.md](fix-pipeline-validation.md). Los fallos de esta tabla se conservan como antecedente, no se borran ni se afirman vigentes sin contrastar el informe posterior.

## Alcance y procedencia

Autorizaciones del responsable: IMPLEMENT COMPLETE CI/CD PIPELINE y CONTINUE IMPLEMENTATION FROM CURRENT STATE. Rama `feature/ci-cd`; HEAD/base `2e89e3c2830a2aa21e573e8c9c4526e4dcd753ca`, merge PR#2 que contiene `bab1f17728a4212471f4964d994fe42927bb6170`. Son cambios **sin commit**: los resultados locales corresponden al working tree inspeccionado, no a una attestation GitHub del commit base.

La interrupción fue un límite de uso; hubo además un timeout local de 30 s que no prueba un fallo funcional. Al recuperar se ejecutaron status/branch/diff/stat/cached-stat: 27 archivos nuevos conservados, sin staged ni diff de archivos tracked en aquel punto. No se hizo reset, descarte, cambio de rama, commit, push o PR durante la recuperación.

Esta entrega no accedió a AWS para validar identidad/infra, no publicó ECR, no construyó candidata de aplicación, no desplegó, no reinició A/B y no ejecutó initialize. Las llamadas AWS de tests usan dobles o DynamoDB Local, no la cuenta real. **Historical initialize total=1** se conserva como antecedente, no una medición repetida. OriginSecret/JWT/Mongo/Redis/DPAPI y el entorno compartido permanecen intactos.

## Componentes y aceptación

| Componente | Estado de revisión | Límite |
|---|---|---|
| CI, 12 jobs declarados | PARTIAL / IMPLEMENTED | YAML y helpers comprobados; tests/hallazgo IaC bloquean; sin run GitHub. La matriz runtime materializa 13 ejecuciones de job en total. |
| Hardening, 10 jobs | PARTIAL / IMPLEMENTED | Contrato/custodia/estados listos; imagen real/CodeQL/Stryker pendientes; laboratorio ZAP/RESTler inexistente, sin PASS ficticio. |
| Deploy, 10 jobs | PARTIAL / IMPLEMENTED | Manualidad/orden/compensación probados solo con mocks; CONFIGURATION PENDING y RUNTIME VALIDATION PENDING. |
| OIDC/IAM/SSM/environment | CONFIGURATION PENDING | Policies/documento preparados; no creados/asumidos/verificados en AWS/GitHub. |
| Catálogo 004 | Actualizado | 10 capacidades con estado individual; ninguna COMPLETE integralmente, nueve gates pendientes. |

Los checks no se presentan verdes cuando falta ejecución aplicable. Las implementaciones de gates fallan ante skip, falta de evidencia, contexto/policy/intento ajeno o findings bloqueantes. Las pruebas sintéticas no cierran gates cloud ni AUTH-027.

## Validaciones efectivamente ejecutadas

| Verificación | Resultado observado |
|---|---|
| Parser YAML 1.2, unique keys | PASS: tres workflows, dos composite actions y reglas Semgrep. |
| Actionlint 1.7.12 | PASS: tres workflows, needs/contexts/estructura. Binario oficial, SHA256 de release verificado; shellcheck/pyflakes externos no disponibles, no se acreditan. |
| Sintaxis Node | PASS: nueve módulos `.mjs`, incluido test y generador. |
| Tests del pipeline | PASS: **26/26**, cero skips; alcance/diff incierto, TRX incompleto/vacío/fallido, gates, identidad/freshness, manualidad/pinning, actor/triggering actor/rerun, compensación A/B, comando incierto y rechazo antes de AWS. |
| Tests Python del agente SSM | PASS: **7/7**, en contenedor Linux aislado, red deshabilitada, source RO y tmpfs; mocks, sin servicios remotos. Incluye retorno sin restart cuando el forward falló antes de cambiar imagen/journal. |
| SDK / restore / build Release | PASS: **SDK10.0.403 exacto**, restore locked de la solución y build sin restore implícito. SDK externo recuperado de ZIP con SHA512 oficial verificado; no se cambia global.json ni SDK del sistema. |
| UnitTests | PASS: **844/844**, cero skips. |
| IntegrationTests | **FAIL: 306/311 PASS, 5 FAIL**, cero skips; fixtures Mongo replica set/Redis noeviction/DynamoDB Local/Ryuk existentes. Resultado original conservado; no retry para verde. |
| SecurityTests | **FAIL: 218/223 PASS, 5 FAIL**, cero skips; primera ejecución separada porque el intento anterior de IT detuvo el helper antes de ST. Sin rerun de ST. |
| Contrato | Assertions existentes y health exacto añadido; control agregado NO ACREDITADO mientras IT/ST no pasen. No se hace smoke contra DEV. |
| PipeProducer.Tests | PASS: **29/29**, Windows, fixtures no operativas. |
| QuotaControl.Tests | PASS: **90/90**, Windows local, dobles. |
| Bootstrap.Tests | PASS: **94/94**, Windows local, dobles; no initialize operativo. |
| Orchestrator.Tests | **FAIL, exit1**. Prueba positiva final depende de estación Windows/hashes de binarios aprobados; no se debilita ni omite. Se asigna al runner Windows, no a Linux. |
| Materializer.Tests | PASS: 47/47 assertions iniciales; observabilidad70/70 escenarios y1005/1005 assertions; parse13/13 y78/78; identidad30/30 y268/268. Sin AWS/network/secret-read/runtime-write reales. |
| Linux runtime .NET completo | RUNTIME VALIDATION PENDING; los ejecutables anteriores se observaron localmente en Windows. Python sí se validó en Linux. |
| NuGet audit | PASS: solución más diez proyectos tools, directas/transitivas, sin High/Critical. Se respetan los RIDs existentes mediante targets auxiliar y locks sin regeneración. |
| Dependency Review API en PR | CONFIGURATION/RUNTIME VALIDATION PENDING; fuera de PR la comparación es NOT APPLICABLE, no se simula aprobación de un PR. Endpoint oficial probado con mock. |
| Semgrep1.186.0 | PASS: patrones de seguridad de texto C# sobre API/tools; cero findings/errores. Intentos previos AST registraron PartialParsing; no se ignora ese error ni se declara cobertura AST. CodeQL sigue requerido. |
| Hadolint2.12.0 | PASS sobre Dockerfile real; sin cambiarlo. |
| Gitleaks8.24.2 histórico | PASS después de revisar 11 falsos positivos precisos de prosa/fixtures. No secretos reales aceptados; sin allowlist global por directorio. |
| Gitleaks sobre cambios nuevos | **PASS: 37 archivos publicables modificados/nuevos, cero findings.** El escaneo inicial detectó tres términos de prosa no secretos en Markdown completos; se comparan exactamente en la regla generic-api-key, sin excluir archivos ni credenciales. |
| Canary del scanner | PASS: material aleatorio NO OPERATIVO detectado por regla OriginSecret y redacción comprobada, nunca impreso/subido. |
| Terraform fmt/init sin backend/validate | PASS. Sin plan cloud, credenciales ni apply. |
| Trivy config | **FAIL: cinco findings totales, uno CRITICAL bloqueante**, `AVD-AWS-0104` / `aws-vpc-no-public-egress-sgr`, `infra/security.tf`: egress irrestricto. Sin cambiar infraestructura ni aprobar excepción. |
| Trivy imagen / OCI real / ECR / CodeQL / Stryker | NOT EXECUTED localmente; requieren candidata/contexto/herramientas y gate de ejecución correspondiente. |
| ZAP / RESTler | NOT EXECUTED; solicitud dinámica bloquea hasta tener laboratorio HTTP aislado/OpenAPI verificado. Ningún tráfico destructivo a CloudFront/ALB/A-B. |
| Deployment / compensación AWS / AUTH smoke real | NOT EXECUTED. Solo modelos locales del orquestador; retorno manual aún `approved=false`. |

Las cifras son resultados observados de esta ejecución, **no constantes incorporadas al pipeline**. Cambios posteriores de tests se contabilizan desde resultados nativos. El fallo original no se borra por correcciones posteriores de un helper, SDK temporal, parser, permisos de contenedor o comando de validación.

## Fallos y siguiente gate

IT: dos fallos MongoConnectionException saneados (`PasswordRehashIntegrationTests`, `MongoUserStoreIntegrationTests`) y tres pruebas de configuración que observaron ObjectDisposedException en lugar del mensaje esperado. ST: cinco variantes de `AuthenticationStartupConfigurationTests.Invalid_synthetic_configuration_is_rejected_during_test_host_startup`. No se deduce un único origen ni se alteran límites SUT/fixtures para esconderlos. El pipeline conserva FAIL y ejecuta ambas suites sin retries.

El Orchestrator exige identidad de estación y binarios históricos en `PipeInventory.Precheck`; una prueba positiva de ese entorno no es portable por moverla a un runner. Adaptar su fixture exige conservar el contrato productivo y demostrar casos negativos, no reescribir hashes/guards ni esperar PASS de un fallo. IaC requiere revisión del egress existente y decisión autorizada sobre remediación/excepción, sin apply aquí.

**NEXT GATE: FIX PIPELINE**, delimitado a esas pruebas/portabilidad y decisión de IaC. Después: autorización de commit/push/PR y TEST CI ON GITHUB; configurar OIDC/GitHub/IAM y promoción ECR solo en gates posteriores. No habilitar flags para saltarse estos bloqueos. Default branch/licencia/protecciones/actores/retención y aprobación del retorno también requieren resolución real.

## Evidencia local privada (fuera de Git)

Bajo `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\`:

- `enver-cicd-local-validation\enver-evidence\build_test.json` y `enver-ci-private\{unit,integration}\*.trx`: build/UT y fallo IT original.
- `enver-cicd-security-once\security.trx`: primera ejecución ST.
- `enver-cicd-local-runtime\enver-evidence\runtime_tool_tests.json`: PipeProducer.
- `enver-cicd-recovery-scanners\enver-evidence\secret_scan.json`: historial y revisión de falsos positivos.
- `enver-cicd-final-scanners\enver-evidence\semgrep.json`: alcance C# texto y limitación; reporte privado del canary.
- `enver-cicd-final-audit\enver-evidence\dependency_review.json`: auditoría NuGet (comparación PR no ejecutada).
- `enver-cicd-final-iac\enver-evidence\iac_validation.json` y reporte Trivy privado: FAIL, Terraform válido y finding CRITICAL.
- `enver-cicd-final-changes-scan\enver-ci-private\changes-gitleaks.json`: revisión local redaccionada; no incluir ese reporte en artifacts.

No copiar TRX/stdout/reportes crudos a GitHub artifacts o a documentos. Solo resultados saneados; ningún secret runtime, JWT, contraseña, token AWS/OIDC o DPAPI forma parte de esta evidencia publicable.

## Archivos

Modificados:

- `.dockerignore`
- `.gitignore`
- `memoria.md`
- `specs/004-ci-cd/tasks.md`

Nuevos:

- `.config/dotnet-tools.json`
- `.github/actions/ci-control/action.yml`
- `.github/actions/deploy-phase/action.yml`
- `.github/workflows/ci.yml`
- `.github/workflows/hardening.yml`
- `.github/workflows/deploy-dev.yml`
- `.gitleaks.toml`
- `.gitleaksignore`
- `scripts/README.md`
- `scripts/validation.md`
- `scripts/ci/RuntimeToolRestore.targets`
- `scripts/ci/hardening.mjs`
- `scripts/ci/lib.mjs`
- `scripts/ci/pipeline.mjs`
- `scripts/ci/policy.json`
- `scripts/ci/scan-changes.mjs`
- `scripts/ci/scanner-selftest.mjs`
- `scripts/ci/semgrep.yml`
- `scripts/ci/stryker-config.json`
- `scripts/deploy/build-ssm-document.mjs`
- `scripts/deploy/candidates.mjs`
- `scripts/deploy/deploy.mjs`
- `scripts/deploy/iam/deploy-permissions.json`
- `scripts/deploy/iam/deploy-trust.json`
- `scripts/deploy/iam/github-oidc-subject.json`
- `scripts/deploy/iam/publisher-permissions.json`
- `scripts/deploy/iam/publisher-trust.json`
- `scripts/deploy/remote_agent.py`
- `scripts/deploy/rollback-baseline.json`
- `scripts/deploy/ssm-document.json`
- `scripts/tests/pipeline.test.mjs`
- `scripts/tests/remote_agent_test.py`
- `tests/Enver.API.IntegrationTests/HealthContractTests.cs`
