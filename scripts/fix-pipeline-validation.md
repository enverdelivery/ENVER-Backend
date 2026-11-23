# FIX PIPELINE — diagnóstico y evidencia local

Fecha: 2026-11-19. Rama `feature/ci-cd`; base `2e89e3c`/`origin/develop`, contiene `bab1f17`. La solicitud autoriza corregir blockers locales de pruebas/harness/IaC y revisar los tres workflows existentes. No rediseña004 ni autoriza configurar GitHub/OIDC/AWS, publicar ECR, desplegar, initialize, stage, commit, push o PR. Se preservaron los37 archivos nuevos/modificados del intento anterior y su [evidencia original](validation.md).

**Último cierre:** §11, AWS provider OIDC y dos roles dedicados creados/read-back PASS; ARNs reales en workflows. **NEXT GATE=REMOVE TEMP IAM BOOTSTRAP PERMISSIONS**, no retirados aquí. GitHub subject/environment y SSM/STS runtime pendientes; cero deploy/publicación/initialize. IT311/311 y ST223/223 PASS previos conservados, no repetidos. Workflows READY FOR GITHUB en código, no operativos para publicar/desplegar todavía; anteriores fallos/diagnósticos intactos.

## 1. Diez fallos originales, por caso

Todos los nombres IT se encuentran en `tests/Enver.API.IntegrationTests/`; los ST en `tests/Enver.API.SecurityTests/`. Los valores de configuración indicados son fixtures negativas, no secretos.

| Caso / archivo | Contrato esperado | Resultado original | Diagnóstico |
|---|---|---|---|
| IT `AuthenticationDependencyCompositionTests.Missing_control_configuration_is_not_replaced_with_defaults`, `AuthenticationComposition:QuotaGeneration=""`; `AuthenticationDependencyCompositionTests.cs` | Rechazar config ausente con validador AuthenticationComposition, sin defaults ni secretos | ObjectDisposedException sin mensaje del validador | B/C: harness anterior a la frontera saneada de proceso; causa común descrita abajo |
| IT mismo método, `AuthenticationComposition:QuotaIdentityReference=""`; mismo archivo | Igual: referencia requerida, startup rechazado | Igual | Misma causa, una sola corrección compartida |
| IT mismo método, `RateLimiting:MaxActivePartitions=4999`; mismo archivo | Rechazar capacidad incompatible con AuthenticationComposition | Igual | Misma causa |
| IT `PasswordRehashIntegrationTests.Real_store_preserves_CAS_contract_and_bounded_conflict_resolution(scenario: updated, expected: Verified)`; `PasswordRehashIntegrationTests.cs` | CAS real actualizado una vez; hash válido, campos/fecha preservados, sin lectura/retry adicional | MongoConnectionException saneada; sin fase/stack Mongo original | B/F del harness de conexión/lifecycle, **operación exacta histórica no demostrable** con el TRX disponible; no se inventa una regresión ni un punto exacto de timeout |
| IT `MongoUserStoreIntegrationTests.Disabled_is_returned_and_own_databases_and_cleanup_are_isolated`; `MongoUserStoreIntegrationTests.cs` | Disabled leído, DBs aisladas, solo cleanup propio, otra DB preservada | MongoConnectionException; warm-up de ambos clientes observado, sin fin de setup del segundo | Mismo límite de diagnóstico: fallo en fase de preparación o cleanup que pudo enmascararlo; no hay evidencia de fallo Redis/Dynamo, puertos fijos ni cambio de persistencia |
| ST `AuthenticationStartupConfigurationTests.Invalid_synthetic_configuration_is_rejected_during_test_host_startup`, `MongoDb:DatabaseName=other_environment_database`; `AuthenticationStartupConfigurationTests.cs` | Rechazar database fuera del entorno; mensaje MongoDb seguro | ObjectDisposedException en vez del validador | Misma causa común de harness que los tres IT de config |
| ST mismo método, `Redis:NamespacePrefix=other:namespace`; mismo archivo | Rechazar namespace de otro entorno | Igual | Misma causa |
| ST mismo método, `Jwt:Algorithm=none`; mismo archivo | Solo HS256 aceptado; rechazar none antes de servir | Igual | Misma causa |
| ST mismo método, `Authentication:InstitutionalDomain=example.invalid`; mismo archivo | Rechazar dominio no aprobado | Igual | Misma causa |
| ST mismo método, `RateLimiting:PolicyRevision=""`; mismo archivo | Revisión requerida, sin default inseguro | Igual | Misma causa |

### Causa común y corrección de configuración

`Program.cs` valida `IStartupValidator` antes de resolver IHostedService/clientes; la frontera `SafeStartup` captura cualquier fallo, escribe solo categorías seguras y retorna exit1, disponiendo el host. Esto es **correcto para el proceso runtime**. El `DeferredHost` de WebApplicationFactory puede finalizar Start normalmente al terminar el entrypoint; después intenta resolver TestServer del provider ya dispuesto. Las pruebas históricas esperaban que el error Options se propagara desde el entrypoint, una suposición que ya no corresponde a esa frontera.

`tests/Shared/AuthenticationApiFactory.cs` ahora decora exclusivamente el IStartupValidator real en el harness y observa sus OptionsValidationException/AggregateException de Options; vuelve a propagarlas intactas después de `CreateHost`, tanto si Start falla como si retorna con el host dispuesto. No sustituye validadores, no ejecuta un segundo conjunto de validaciones, no inserta valores válidos ni cambia Program/SafeStartup. Las pruebas conservan sus asserts de rechazo/mensaje/no-secret y **añaden** comprobación de que es un fallo Options real, no cualquier excepción. La ejecución afectada3IT+6ST (incluye startup válido) pasó antes de la suite completa.

### Mongo: alcance real de la corrección

La ejecución diagnóstica de los10 casos Mongo afectados/relacionados pasó sin retry de suite, por lo que **no reprodujo** los dos fallos históricos. El reporte original había suprimido también la ubicación; no puede recuperarse de él el inner exception/operación exactos. Se conserva esa limitación: un nuevo PASS no prueba por sí solo una causa histórica.

Sí se identificaron límites incorrectamente acoplados en el harness: el setup DDL tenía token/socket5s pero heredaba connect/selection500ms, y el cleanup usaba el socket SUT500ms pese a tener token10s. Se separan connect/selection/socket de los clientes **exclusivos de setup/cleanup**, manteniendo endpoint/DB propia/concerns y una sola escritura DDL/drop. El cliente SUT, wtimeout500ms, concerns majority/journal/primary, retryWrites=false y presupuestos de operación/request quedan intactos. El warm-up del cliente SUT hace solo ping de readiness acotado a5s, fuera del request, con polling100ms únicamente ante fallo transitorio de conexión/timeout; no repite inserts/CAS/login ni tests. Primary y respuesta ok siguen siendo requisitos.

Ambos scopes generan diagnóstico saneado de comando/fase/tipo; `SafelyAsync` conserva ubicación fuente/línea sin documentos, claves, URI o mensajes sensibles. PasswordRehash observa esos diagnósticos mediante ITestOutputHelper. No se abren puertos, sustituyen dependencias ni usan recursos cloud. No se ha demostrado una regresión de la aplicación, y no se modifica su semántica para conseguir verde.

## 2. Orchestrator: dos fronteras explícitas

- **PORTABLE CI TEST:** control de secuencia, compensación, límites/ambigüedad, XML/AWS wire sintético, no-secret y rechazo de precheck antes de leer el control plane/generar secreto/mutar.28 casos, conteo calculado desde las ejecuciones, no mínimo histórico de aceptación.
- **WORKSTATION / HISTORICAL VALIDATION:** antiguo assert positivo de `new PipeInventory().Precheck()` contra ruta Windows y hashes inmutables. Se conserva en el mismo ejecutable, modo exacto `--historical-artifact-validation`; args desconocidos dan STOP. Nunca se activa por variables de entorno ni fallback de CI.
- El modo histórico no corre aquí y se informa **NOT EXECUTED**, no skipped/PASS. No se cambian `ControlSession.cs`, `PipeInventory`, `FixedResources`, hashes, descriptor ni binarios aprobados. Las comprobaciones históricas siguen exigibles en el gate operativo que use esos artefactos.
- El CI no excluye toda la herramienta: Windows ejecuta Orchestrator portable y PipeProducer; Linux las otras tres suites. Los tests locales portables se observan en Windows; los runs Linux .NET siguen pendientes de GitHub, no se afirman equivalentes por inferencia.

## 3. IaC y aprobación posterior específica

Finding válido `AVD-AWS-0104` en `aws_vpc_security_group_egress_rule.backend_initial`, archivo `infra/security.tf`, `ip_protocol=-1`, destino `0.0.0.0/0`. No se necesitaban todos los protocolos/puertos.

El responsable aprobó expresamente la excepción acotada consultada durante este gate: se elimina esa regla en **fuente**, se permiten TCP443 públicos para servicios AWS/ECR/SSM y TCP27017 públicos para Atlas SRV, más TCP6379 solo al SG cache. No se inventan CIDRs públicos fijos ni abre DNS público: AmazonProvidedDNS no se filtra mediante SG. Los únicos ignores inline son AVD-AWS-0104 en `backend_https` y `backend_atlas`, owner responsable ENVER, caducidad2026-12-19 y revisión antes de apply. [Justificación, riesgo y revisión](../infra/EGRESS-REVIEW.md). AWS conserva sus reglas actuales: HCL corregido **no significa apply ni conectividad operativa probada**.

Se detectó contaminación del scan por planes antiguos ignorados en el workspace: Trivy reportaba `backend_initial` aun después de cambiar HCL. La validación usa ahora solo fuentes publicables git-listed (`.tf`, lock y bootstrap), sin state, planes, tfvars, cache o secretos locales; scanner limitado a Terraform. Se agregaron tests de esa selección. Terraform valida esa copia aislada sin backend/credenciales/plan cloud.

Un intento de init aislado falló por disco insuficiente al descargar otra copia del provider. Se recuperó con una junction **solo** al directorio del provider instalado6.73.0, manteniendo lockfile readonly y sin vincular state/backend. `fmt -check -recursive`, `init -backend=false -input=false -lockfile=readonly` y `validate` dieron exit0. Esa optimización local no se incorpora como dependencia de una estación en el workflow: el runner efímero descarga/verifica su provider normalmente.

Un intento posterior del helper completo en `iac-final-helper`, usando `TF_PLUGIN_CACHE_DIR` local, volvió a fallar en **init** al agotarse el disco. No sustituye el PASS de los comandos y scan aislados por un PASS ficticio del helper: ese intento queda FAIL y conserva su recibo. No se hacen más descargas de provider para buscar verde.

Trivy de fuente vigente: PASS según la política High/Critical, **cuatro findings LOW/MEDIUM permanecen**, dos excepciones declaradas, ninguna otra. Trivy0.61.1 no emite los ignores inline como resultados EXCEPTION; se informa observedExceptions=0, no se inventa un conteo del scanner. Las excepciones declaradas permanecen en policy/reporte; el helper falla por vencimiento UTC. Prueba negativa en copia temporal sin las dos anotaciones: **PASS**, reaparecen exactamente dos CRITICAL AVD-AWS-0104, uno por recurso aprobado, y el agregador los rechaza. Cualquier EXCEPTION emitida debe coincidir exactamente con regla/recurso/aprobación/vigencia; FAIL/estado desconocido no se acepta.

## 4. Entorno y secuencia de validación

Después de corregir el harness, un intento afectado tuvo13/13 fallos **en setup**, antes del SUT: faltaba el pipe `dockerDesktopLinuxEngine`. No se contó como un nuevo defecto funcional ni se relanzó ciegamente. Se informó y el responsable autorizó **arranque local de Docker Desktop**, sin reset/prune/borrado de volúmenes ni reinicio EC2. Motor Linux listo tras14 consultas acotadas. El intento fallido y las ejecuciones diagnósticas se conservan en temporal; luego se permiten las pruebas dirigidas y completas al haberse corregido el impedimento de entorno.

Tras pasar las dirigidas13/13IT y6/6ST, el intento completo `full-suites-validated` no produjo una suite válida: IT agotó el timeout30min sin TRX; ST ejecutó223 casos,114PASS/109FAIL/0skipped; los109 fallos comparten DockerUnavailableException durante construcción/setup Testcontainers, antes del SUT. Semgrep quedó BLOCKED/SEMGREP_NOT_EXECUTED, con reporte vacío. La lectura de entorno confirmó **C:0MB** y Docker info sin respuesta. No se afirma que estos fallos sean pruebas funcionales aprobadas ni se borran los recibos.

El responsable autorizó borrar **únicamente** el ZIP verificado recuperable `enver-cicd-sdk-10.0.403.zip` y las descargas fallidas `terraform-provider2682758850` / `terraform-provider3706898909`; se eliminaron esos tres archivos regulares, conservando SDK extraído, evidencia, state, volúmenes y demás temporales. Se recuperaron~583MB. Persistiendo Docker sin respuesta, aprobó **un único reinicio local de Docker Desktop**, sin reset/prune ni restart EC2; su readiness y cualquier prueba posterior se registran separadamente. No se crea un bucle de reintentos de suites.

**Resultado del único reinicio autorizado:** comando exit1;12 consultas acotadas posteriores no obtuvieron motor Linux listo. **BOUNDED_READINESS_EXPIRED.** No hubo un segundo reinicio ni otro intento de IT/ST o scans Docker. El gate queda bloqueado por entorno local; requiere recuperar almacenamiento/motor y después una ejecución completa válida. No se presenta IT311/311 o ST223/223 como logrado.

Para cerrar la revisión de secretos sin depender de Docker se usa Gitleaks **8.24.2 nativo Windows**, misma versión aprobada, desde release oficial y ZIP verificado SHA256 `cc47fdc0364964e2d346fbbcbe4cc87f34d490b2647508fb05930d0ec2fbff07`. No es una ejecución del contenedor del workflow ni acredita su disponibilidad. Calibración sintética: reglas generic-api-key y enver-runtime-secret detectadas, valores completamente redactados en informe/stdout/stderr. Historia Git completa PASS/0findings; revisión final del working tree se registra con los47 archivos publicables. No se leen blobs DPAPI, JSON runtime ni secretos reales para esta calibración.

| Verificación vigente | Resultado |
|---|---|
| Pipeline Node |29/29 PASS; cero failed/skipped |
| Agente Python Linux aislado |7/7 PASS; sin red, servicios ni AWS |
| SDK/restore locked/Release/UT |SDK10.0.403,844/844 PASS |
| Afectadas IT después de Docker recuperado |13/13 PASS,0skipped; recibo post-docker-recovery |
| Afectadas ST después de Docker recuperado |6/6 PASS,0skipped; recibo post-docker-recovery |
| IntegrationTests completas |Intento full-suites-validated BLOCKED/TEST_TIMEOUT sin TRX; no PASS completo |
| SecurityTests completas |Intento full-suites-validated FAIL:114/223PASS,109fallos DockerUnavailable en setup,0skipped |
| Runtime Windows |PipeProducer29/29 y Orchestrator portable28/28 PASS; histórico NOT EXECUTED |
| Otras tres suites portables |PASS: QuotaControl90/90; Bootstrap94/94; Materializer47/47, observabilidad70/70 y1005/1005 assertions, parse13/13 y78/78, identidad30/30 y268/268; sin cloud |
| Terraform fmt/init/validate aislados |PASS, sin backend/plan/apply |
| Trivy config |PASS con excepción explícita acotada;4LOW/MEDIUM preservados |
| Semgrep / Hadolint / NuGet audit |Semgrep BLOCKED por ejecución Docker sin reporte válido; Hadolint no alcanzado en ese intento. NuGet audit PASS; Dependency Review GitHub pendiente |
| YAML / Actionlint |PASS: parser YAML1.2/unique keys, needs/referencias, permisos, eventos,12/10/10jobs; Actionlint1.7.12 PASS (shellcheck/pyflakes externos no acreditados) |
| Secret scan / canary |Gitleaks8.24.2 Windows verificado: historia PASS/0findings, canarios generic/runtime detectados y totalmente redactados;47archivos finales comprobados al cierre. Ejecución Docker final pendiente |

## 5. Revisión de los tres grafos

- **CI12 jobs:** Detect Changes sin needs ni filtro global de evento; fallback técnico ante diff incierto. Gate con `always()` tanto a nivel job como paso agregador, incluso si falla descarga de evidencia. Scoped NOT APPLICABLE se emite, no es un skip implícito. Resultado faltante/ajeno/skipped falla; matriz runtime requiere ambos sistemas cuando aplica; ninguna credencial/id-token AWS en CI/PR. Inputs/build/contract/OCI siguen vinculados al mismo SHA/run/intento.
- **Hardening10 jobs:** implementables ahora verify/scan/CodeQL/mutation/report/gate, con ejecución real de CodeQL/Stryker/imagen pendiente de GitHub/candidata; Publisher CONFIGURATION PENDING. Mutation no solicitada y DAST/fuzzing sin solicitud = NOT EXECUTED explícito; si se solicitan sin target/OpenAPI = BLOCKED y gate falla. Se endurece el gate para rechazar también estado desconocido/NOT APPLICABLE/fallo de opcionales; no se acepta ausencia por conveniencia. Ningún ataque a DEV compartido.
- **Deploy10 jobs:** solo workflow_dispatch humano, develop/actor/triggering actor/intento1, concurrencia `enver-development-deploy` cancel-in-progress=false. Todos los pasos AWS están detrás de sesión OIDC temporal y validación/flags, no un profile local o claves estáticas. A y B usan selección congelada del mismo manifest, retorno usa previo aprobado; histórico manual sigue approved=false. No initialize, apply, shell libre, secreto hardcodeado ni dispatch automático. Orquestación no ejecutada aquí, CONFIGURATION/RUNTIME VALIDATION PENDING.

READY FOR GITHUB significa estructura/correcciones locales listas para un primer run autorizado, **no** CI cloud ya verde, publicación ni deployment habilitados. Los flags de publicación/despliegue y trust/configuración siguen sin activar.

**Readiness de cierre:** CI **LOCAL VALIDATION BLOCKED / NOT READY** (sin IT/ST completas válidas); Hardening **IMPLEMENTED / CONFIGURATION PENDING / RUNTIME VALIDATION PENDING**, sin aprobación para publicar; Deploy **IMPLEMENTED / CONFIGURATION PENDING / NOT EXECUTED**, no habilitado. Las tres estructuras pasan YAML/Actionlint y conservan12/10/10jobs, pero **no se declaran los tres workflows listos para ejecución integral en GitHub** con los blockers actuales. Recuperar primero el entorno local no requiere AWS/OIDC; CI/PR continúa diseñado sin credenciales AWS.

## 6. Evidencia y cierre

Recibos privados en `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-fix-pipeline\`: `affected-it`, `affected-validation-fixed`, `affected-final` (setup fallido), `post-docker-recovery`, `full-unit`, `full-suites-validated`, `runtime-validated`, `iac-source-only`, `iac-exception-proof`, `iac-final-helper`, `scanners`, `nuget`, `native-secret-proof`. No subir TRX/stdout/findings crudos, canarios o datos de runtime a artifacts. Las cifras anteriores provienen de resultados nativos, no de constantes de aceptación del pipeline. Los recibos locales identifican el HEAD base pero validan el **working tree no comprometido**, no una attestation de que el commit base contenga estos cambios.

**Cierre: CORRECCIONES LOCALES CONSERVADAS / VALIDACIÓN INTEGRAL BLOQUEADA. NEXT GATE=FIX REMAINING PIPELINE BLOCKERS.** Recuperar espacio suficiente y motor Docker local, después validar IT/ST completas y controles Docker sobre una única ejecución nueva por recuperación demostrada; no blind retries. Sigue pendiente demostrar la causa/operación exacta de los2Mongo históricos si vuelven a fallar, con el diagnóstico seguro añadido. Luego, con gate local satisfecho y autorización separada, podrán solicitarse GitHub/OIDC/IAM; no se adelantan aquí. CodeQL/Stryker/OCI/Linux .NET requieren evidencia real posterior; laboratorio OpenAPI/DAST/fuzzing y smoke AUTH conservan sus condiciones. Ninguna tarea004 COMPLETE integralmente, nueve gates cloud pendientes. **AWS MODIFIED=NO; OIDC CONFIGURED=NO; DEPLOY EXECUTED=NO; INITIALIZE EXECUTED=NO; STAGE/COMMIT/PUSH/PR=NO.** Historical initialize total1 y AUTH-027 no se cierran por estos resultados.

## 7. Archivos intervenidos en FIX PIPELINE

Estos20 archivos son el delta de esta corrección, no una sustitución de los37 archivos previos. El working tree completo contiene47 archivos publicables nuevos/modificados; se conservan los demás.

- `.github/workflows/ci.yml`
- `infra/security.tf`
- `infra/EGRESS-REVIEW.md` (nuevo)
- `tests/Shared/AuthenticationApiFactory.cs`
- `tests/Enver.API.IntegrationTests/AuthenticationDependencyCompositionTests.cs`
- `tests/Enver.API.IntegrationTests/MongoUserStoreIntegrationTests.cs`
- `tests/Enver.API.IntegrationTests/MongoUserStoreCredentialCasIntegrationTests.cs`
- `tests/Enver.API.IntegrationTests/PasswordRehashIntegrationTests.cs`
- `tests/Enver.API.SecurityTests/AuthenticationStartupConfigurationTests.cs`
- `tools/Enver.QuotaControl.Orchestrator.Tests/Program.cs`
- `scripts/ci/lib.mjs`
- `scripts/ci/pipeline.mjs`
- `scripts/ci/hardening.mjs`
- `scripts/ci/policy.json`
- `scripts/tests/pipeline.test.mjs`
- `scripts/README.md`
- `scripts/validation.md`
- `scripts/fix-pipeline-validation.md` (nuevo)
- `specs/004-ci-cd/tasks.md`
- `memoria.md`

Producto API completo, fixture Testcontainers/imágenes/límites, proyectos/locks, contratos runtime productivos, constituciones, AGENTS y spec/plan004 permanecen sin diff. No se configura AWS/OIDC ni se modifica el baseline operativo por corregir HCL local.

## 8. FIX REMAINING PIPELINE BLOCKERS — precheck posterior

2026-11-19, rama `feature/ci-cd`, mismo HEAD y working tree conservado. Se autoriza diagnóstico/validación local, no AWS/IAM/deploy/initialize/Git write. **LOCAL ENVIRONMENT BLOCKER**, antes de nuevas IT/ST/scanners Docker.

| Comprobación nueva | Evidencia / resultado |
|---|---|
| Disco C: al precheck |173.019.136 bytes libres (165MiB;0,161GiB); faltan1.974.464.512 bytes para2GiB. No se interpreta2GiB como garantía de capacidad de suites |
| Datos Docker |`%LOCALAPPDATA%/Docker/wsl/disk/docker_data.vhdx`: longitud40.856.715.264 bytes (~38,05GiB); main/ext4.vhdx100.663.296 bytes. No se conoce uso interno/espacio recuperable con el motor bloqueado; ningún VHD se modifica |
| Temporales candidatos, no borrados |`%TEMP%/terraform-provider3195792105`178.313.013 bytes; `terraform-provider1038921446`25.076.722 bytes; `.tmpv0.2026.11.11.08.17.stable_01-WarpSetup.exe`136.448.968 bytes. Total339.838.703 bytes, insuficiente. Requieren comprobar propietario/uso/recuperabilidad y autorización antes de eliminar; no se declaran seguros por su nombre |
| Docker version / info |Una consulta de cada comando; ambas expiran a12s sin salida. Se termina solo cada CLI vencida |
| Docker context ls |exit0; `desktop-linux` activo, pipe esperado dockerDesktopLinuxEngine |
| Desktop/servicio |Procesos Desktop/backend activos; com.docker.service Stopped/Manual. No se diagnostica la causa del motor únicamente por el servicio auxiliar |
| Recuperación/readiness |0 start/restart,0 contenedores; se respeta umbral de disco. No polling ni hello-world después del bloqueo de almacenamiento; Docker BLOCKED, no READY |
| IT / ST nuevas |BLOCKED / NOT EXECUTED,0 casos nuevos. Evidencia previa13/13IT y6/6ST dirigidas PASS; IT completa timeout sin TRX y ST114/223PASS/109setup DockerUnavailable se conservan |
| Semgrep / Hadolint nuevos |BLOCKED / NOT EXECUTED; no findings inventados ni supresiones nuevas |
| IaC nueva |fmt -check -recursive -no-color PASS. validate y Trivy config no repetidos: final BLOCKED. No init/descarga de provider; los PASS aislados y el init fallido previos se conservan separados |
| YAML / Actionlint nuevos |PASS; YAML1.2/unique keys/needs,12/10/10jobs; deploy solo dispatch y concurrencia preservados. Actionlint instalado, sin validación externa ShellCheck/Pyflakes |

No se rediseñan workflows, cambian pruebas/semántica ni toca AWS/IAM. Solo se actualizan este informe, tasks.md y memoria.md. La comprobación nativa final de secretos/diff y el estado final de disco se registran por separado en el recibo nuevo; no se sobreescriben los recibos anteriores.

**Acción manual requerida:** liberar/ampliar almacenamiento fuera del repositorio, sin borrar Docker volumes/VHD ni evidencia, hasta al menos2GiB libres; volver a medir antes de una recuperación controlada del motor. Después exigir docker info exitoso y un contenedor mínimo cacheado exitoso, y solo entonces ejecutar cada suite completa una vez y los controles pendientes. Sin disponibilidad de disco no se consume un nuevo restart para buscar verde.

**CI / HARDENING / DEPLOY = LOCAL VALIDATION BLOCKED.** GitHub runtime y AWS runtime siguen pendientes por separado; ZAP/RESTler no ejecutados sin laboratorio no se reportan como PASS ni bloquean la revisión sintáctica. Ninguna tarea COMPLETE ni gate cloud satisfecho. **AWS MODIFIED=NO; OIDC CONFIGURED=NO; DEPLOY EXECUTED=NO; INITIALIZE EXECUTED=NO; STAGE/COMMIT/PUSH/PR=NO; NEXT GATE=RESOLVE LOCAL DOCKER ENVIRONMENT.**

## 9. Recuperación y continuación después de USAGE LIMIT

2026-11-19. Autorizaciones posteriores delimitadas para completar validación local en `feature/ci-cd`, preservando todos los cambios, sin rediseño ni AWS/IAM/OIDC/deploy/initialize/stage/commit/push/PR. **USAGE LIMIT ONLY** interrumpió la conversación, no acredita un fallo de código ni de pipeline. La recuperación confirma proceso `validate-suites.cjs` y testhost IT originales activos; no se inicia otra suite sobre ellos.

| Control / checkpoint | Evidencia nueva |
|---|---|
| Disco |Inicial2.483.691.520bytes /2.368,633MiB /2,313118GiB; al recuperar2.459.844.608bytes /2,290909GiB. Sin borrados/prune/reset |
| Docker |Context desktop-linux; version/info PASS; ejecución mínima de Hadolint2.12.0 cacheada PASS. READY,0start/restart |
| IntegrationTests completas |Runner original recuperado, SDK10.0.403/build Release sin restore; **311/311PASS/0FAIL/0skipped**, exit0,683,100s. TRX completo verificado; sin relanzamiento |
| SecurityTests completas |Mismo runner secuencial; **222/223PASS/1FAIL/0skipped**, exit1,123,112s. Fallo de aserción durante ejecución funcional, no Docker/setup; sin retry |
| Semgrep |PASS;137archivos,0findings; pipeline helper vigente, limitación text patterns C# preservada, CodeQL GitHub requerido |
| Hadolint |PASS;0findings, imagen digest aprobado cacheada |
| Terraform |fmt/validate PASS; snapshot publicable26archivos idénticos a fuente vigente, provider instalado;0init/descargas,0plan/apply |
| Trivy config |PASS;4LOW/MEDIUM,0HIGH/CRITICAL; únicamente las dos excepciones exactas aprobadas, observedExceptions=0 real. Imagen digest aprobado recuperada del registro público por faltar en caché, con headroom previo comprobado |
| YAML / Actionlint |PASS;12/10/10jobs, needs/unique keys, deploy solo workflow_dispatch y concurrencia requerida; no ShellCheck/Pyflakes externos |
| Secret scan / diff finales |PASS previo conservado y47archivos idénticos al manifest escaneado al recuperar. El recibo final registra scan nativo/hashes del working tree tras cambios documentales y diff/index; no implica ejecución del contenedor GitHub ni nueva historia cloud |

Los cuatro findings IaC permanecen visibles: AVD-AWS-0024 MEDIUM y AVD-AWS-0025 LOW en authority DynamoDB, AVD-AWS-0178 MEDIUM en VPC y AVD-AWS-0033 LOW en ECR. No se reparan ni suprimen por iniciativa. Solo las dos excepciones AVD-AWS-0104 de §3 están aprobadas, hasta2026-12-19.

### Único fallo restante: evidencia y límite diagnóstico

`Enver.API.SecurityTests.LoginSecurityTests.Security_rehash_race_has_one_winner_no_loop_and_only_closed_observability` falla en `tests/Enver.API.SecurityTests/LoginSecurityTests.cs:60`: **Expected OK / Actual ServiceUnavailable**. Duración nativa del caso **2,2761003s**; la suite termina con un único fallo y0skip. **FUNCTIONAL TEST ASSERTION FAILURE**, no fallo de disponibilidad Docker/construcción de fixture; una regresión de producto o su causa exacta todavía no quedan demostradas. No se reejecuta para cambiar el resultado ni se debilita la aceptación200.

El caso sincroniza dos hosts antes de CAS y exige dos respuestas200, dos escrituras, una relectura, dos emisiones y eventos cerrados. El assert HTTP falla antes de comprobar los contadores. El TRX no incluye stdout/probes/eventos de ese caso, por lo que no permite identificar cuál solicitud/fase produjo503.

La inspección confirma un candidato concreto: `LoginTestScope.CreateAsync(fixture)` sin override hereda **LoginBudget1000ms**, `tests/Shared/AuthenticationDependenciesFixture.cs:165`; el caso concurrente análogo de IT (`LoginTests.cs:87–109`) configura explícitamente **4000ms de test**. Esa diferencia no demuestra que expirase el presupuesto: la duración nativa incluye setup/cleanup, y cuotas/admisión/Mongo/CAS/relectura/rehash/emisión también pueden cerrar con503. No se cambian budgets de producto/tests, ports, asserts o fixtures. No se atribuye el503 a Redis/Dynamo/Mongo ni al coste KDF sin evidencia de fase.

**Próximo diagnóstico propuesto, requiere autorización propia:** una ejecución dirigida instrumentada del caso fallido con categorías/fase, causa de presupuesto y estados/counters ya disponibles de los probes, sin secretos ni cuerpos/tokens/identidades; conservar las aserciones originales, sin retries ni ampliar tiempos para obtener verde. Decidir una corrección de harness o producto solo después de acreditar la causa y su alcance. No se inicia aquí.

Recibos privados nuevos: `%LOCALAPPDATA%/Temp/opencode/enver-fix-pipeline/docker-resume-2026-11-19T18-14-43-567Z/`. initial-docker/recovered-readiness, integration/security TRX e intent/resúmenes, suites-summary, hadolint/result, semgrep-evidence/semgrep, iac-final, workflows y cierre final de secretos/hashes/diff. La evidencia anterior permanece sin sobrescribir. Código de producto, pruebas, workflows, HCL y configuración no se cambian en esta continuación; solo este informe/tasks/memoria. Los scans PASS corresponden a bytes de código/configuración conservados; el recibo final exige comprobar también los tres Markdown finales.

**CI / HARDENING / DEPLOY = LOCAL VALIDATION BLOCKED**, por1ST restante, no por entorno Docker ni laboratorio ZAP/RESTler. Se separan local code validation, GitHub runtime y AWS runtime: runs GitHub/Linux .NET/CodeQL/mutation/OCI y configuración/publicación/deploy AWS no acreditados. ZAP/RESTler sin lab no son PASS ni impiden revisión local de sintaxis. Ninguna tarea COMPLETE ni gate cloud cerrado. **AWS MODIFIED=NO; OIDC CONFIGURED=NO; DEPLOY EXECUTED=NO; INITIALIZE EXECUTED=NO; STAGE/COMMIT/PUSH/PR=NO; NEXT GATE=FIX REMAINING PIPELINE BLOCKERS**. Requiere diagnóstico acotado autorizado del fallo ST; no otro restart ni repetición de IT/Semgrep/controles ya PASS.

## 10. FIX FINAL SECURITY TEST BLOCKER — TEST BUDGET confirmado

2026-11-19, `feature/ci-cd`. Autorización exclusiva para reproducir con diagnóstico el caso de carrera, corregir la causa acreditada, verificar objetivo/grupo/suite y regresión pertinente. No cambian producto/workflows/dependencias/Dockerfile/IaC/AWS/IAM/OIDC ni se publica/deploy/initialize operativo/commit/push/PR.

### Intención y reproducción única

El caso conserva usuario Active/credencial válida en dos hosts. AUTH-013/021/039 y AC-06 de002 exigen rehash seguro y sesiones válidas: un CAS gana; el otro pierde, relee una vez y autentica contra el hash nuevo. Por tanto ambos deben terminar200 si controles/presupuesto permiten completar el trabajo acotado. No se permite un bucle CAS ni convertir503 a200 cuando la confirmación es incierta. Se mantienen todas las aserciones originales de status, escrituras, relecturas, emisiones y eventos.

La instrumentación **opt-in solo test** decoró los controles/emisión reales y observó probes de usuario/password con enums/resultados/presupuesto. Sin respuestas favorables simuladas, operaciones repetidas, cuerpos/identidades/contraseñas/JWT/hash/URIs/excepciones ni logging productivo. El primer build falló por un using Quotas ausente, corregido antes de ejecutar pruebas y con logs conservados. Después hubo **una sola reproducción real** con presupuesto original1000ms,1FAIL/0skip:

| Evidencia segura por solicitud | A | B |
|---|---|---|
| HTTP / ProblemDetails |503 / temporarily_unavailable |503 / temporarily_unavailable |
| Fixture |Index Ready, DummyReady true, hashing1 |Igual |
| Admisión / cuota |Verifiable / Admitted |Verifiable / Admitted |
| Lookup / rehash |Found / Completed |Found / Completed |
| CAS / relectura |Conflict / Found una vez |Updated / sin relectura |
| Última fase |Reverificación:139,240ms antes del KDF; Unavailable/RequestBudget/0ms al salir |Emisión protegida:139,918ms al entrar; null/RequestBudget/0ms al salir |
| Firmante JWT invocado |0 |0 |

**Clasificación única E — TEST BUDGET.** El trabajo real de KDF, CAS/relectura y emisión excede el perfil sintético1s del caso, no un PRODUCT BUG/contrato obsoleto ni setup Docker. El KDF de A se ejecutó, por tanto su Unavailable no se atribuye a falta de cupo; admisión/cuota/Mongo fueron confirmados. Esto prueba la causa de la reproducción; el TRX histórico anterior sigue sin detalle de fase, no se reescribe.

### Productores503 exactos

- **A:** `Enver.API/Security/PasswordService.cs`, `Verify:43`: después del KDF `budget.Cause != None` devuelve Unavailable. `PasswordRehashCoordinator.ResolveAsync:48–49` rechaza esa reverificación y `AuthenticationService.LoginCoreAsync:65–66` devuelve LoginStatus.Unavailable.
- **B:** `Enver.API/Composition/LoginControls.cs`, `LoginTokenIssuer.IssueAsync`, retorna null durante la emisión protegida, RequestBudget agotado y0firmas; `AuthenticationService.LoginCoreAsync:69–71` rechaza emisión nula/presupuesto vencido. El probe no distingue el subpaso interno seguridad/lease que consumió139ms: no se inventa un fallo Dynamo específico.
- **Ambos:** `AuthenticationController.Login:46–52` mapea Unavailable a503; ApiProblemDetails produce temporarily_unavailable. No handler global, SafeStartup, CloudFront trust, cuota rechazada o falta de readiness explican estos503 observados.

### Corrección solo del test/harness y resultados

`tests/Enver.API.SecurityTests/LoginSecurityTests.cs` aplica **4000ms solo al caso de carrera**, igual que el IT concurrente preexistente (`LoginTests.cs:87–109`), no una ampliación productiva/SLA. Asserts nuevos prueban base fixture1000ms intacta, override4000 en ambos hosts, Mongo500ms, Redis250ms y hashing1. Se conservan dos200, dos escrituras, una relectura, dos emisiones y eventos cerrados.

`tests/Shared/LoginTestScope.cs` añade captura desactivada por defecto, únicamente enums de fase/resultado/cancelación y tiempo. Delega una vez a los controles/emisor/probes reales, sin cambiar setup, barrera, stores, hasher, CAS o presupuestos de otros tests. No sleeps, skips, retries, cuotas locales sustitutas ni modificaciones operativas de authority/credenciales.

| Verificación, una ejecución por paso | Resultado |
|---|---|
| Objetivo tras corrección |1/1PASS/0failed/skipped;70,989s incluyendo fixture |
| LoginSecurityTests completo |62/62PASS/0failed/skipped;110,290s |
| SecurityTests completo después del grupo |223/223PASS/0failed/skipped;141,479s |
| IT Login por harness compartido |25/25PASS/0failed/skipped;95,098s |
| IT completas previas |311/311PASS preservado, no se repiten311 por cambio solo test/harness |
| UT/runtime/Node/Python previos |PASS conservados, producto/tools/helpers intactos, no repetidos |

Tras el fix el objetivo registra dos200, CAS Updated/Conflict, una relectura/dos emisiones y Cause=None/~2991ms restantes en ambas salidas. No se reduce KDF ni se fija qué host gana; A/B puede intercambiarse. Diagnóstico fallido y original222/223 permanecen; ejecuciones posteriores son gates autorizados tras causa corregida, no retries ciegos. SDK10.0.403/build Release sin restore con assets locked previos; sin paquetes nuevos.

### Cierre local y límites externos

Semgrep137/0findings, Hadolint e IaC PASS **conservados por bytes**: Enver.API/tools/Dockerfile/fuentes IaC no cambian; no se extiende cobertura Semgrep a tests. Solo dos excepciones AVD-AWS-0104 precisas vigentes y4LOW/MEDIUM IaC. YAML/Actionlint y scan final nativo de49archivos publicables/hashes/diff se registran en el recibo final; historia Git PASS previa conservada, sin commits/fetch nuevos.

**CI.YML / HARDENING.YML / DEPLOY-DEV.YML = READY FOR GITHUB** para primera ejecución autorizada, no runtime cloud verde. Conservan12/10/10jobs y deploy solo dispatch humano. Hardening mantiene configuración/controles externos pendientes, CodeQL obligatorio; Deploy requiere OIDC/IAM/environment/actores/SSM, flags cerrados. Linux .NET/CodeQL/mutation/OCI, runs GitHub y AWS runtime no acreditados. ZAP/RESTler sin laboratorio NOT EXECUTED/BLOCKED/RUNTIME VALIDATION PENDING, no PASS; controles solicitados/aplicables siguen bloqueantes si faltan.

Delta exclusivo: dos archivos tests anteriores y tasks/informe/memoria;49archivos publicables totales, previos preservados. Recibos privados `%LOCALAPPDATA%/Temp/opencode/enver-fix-pipeline/final-security-2026-11-19T18-40-16-326Z/`: diagnostic/target-fixed/login-group/security-full/integration-login, diagnosis.json, scope-proof y cierre final. Build inicial sin tests en final-security-2026-11-19T18-39-44-196Z conservado. No subir TRX/stdout crudos o datos de runtime.

**REMAINING LOCAL BLOCKERS=NONE; NEXT GATE=CONFIGURE GITHUB OIDC AND IAM**, requiere autorización posterior, no se ejecuta ahora. Nueve gates pendientes, ninguna tarea004 COMPLETE integralmente ni AUTH-027 cerrado. **AWS MODIFIED=NO; OIDC CONFIGURED=NO; DEPLOY EXECUTED=NO; INITIALIZE OPERATIVO EXECUTED=NO; STAGE/COMMIT/PUSH/PR=NO.**

## 11. FIX OIDC/IAM CONFIGURATION — AWS configurado, GitHub pendiente

2026-11-19. Autorizaciones CONFIGURE/CONTINUE/FIX OIDC/IAM del responsable. En esta continuación solo profile **enver-infra**, identidad STS `arn:aws:iam::926925605666:user/enver-terraform-local`/cuenta926925605666. Prechecks STS, ListPolicies, ListOpenIDConnectProviders y ListRoles PASS; descubrimiento completo13policies Local/0providers/0roles GitHub. Se preservan errores AccessDenied y sesión administrativa expirada históricos; no se reintenta perfil admin ni se autoeleva el usuario. Bootstrap ENVER-Terraform-IAM-Runtime v3 contiene los cuatro statements GitHub autorizados; se lee y conserva hash antes/después.

### Cambios AWS realmente aplicados y verificados

| Recurso | Acción / ARN |
|---|---|
| Provider |CREATED `arn:aws:iam::926925605666:oidc-provider/token.actions.githubusercontent.com`; URL https://token.actions.githubusercontent.com; audience único sts.amazonaws.com |
| Publisher |CREATED `arn:aws:iam::926925605666:role/enver-github-ecr-publisher`; inline ENVER-GitHub-EcrPublisher |
| Deploy |CREATED `arn:aws:iam::926925605666:role/enver-github-deploy-dev`; inline ENVER-GitHub-DeployDev |

Exactamente5escrituras: un provider, dos roles, dos inline; sin policies managed nuevas/adjuntas, duplicados, instance profiles o modificación del rol runtime. IAM obtuvo certificado/thumbprint al crear provider sin valor inventado. Lectura GetOpenIDConnectProvider/GetRole/ListAttachedRolePolicies/ListRolePolicies/GetRolePolicy valida exactamente las políticas fuente: **PASS configuración AWS**, una inline/cero managed por rol y MaxSessionDuration3600s. No IAM simulation obligatoria ni prueba destructiva o autenticación STS fabricada.

### Trust y least privilege

- Solo Principal Federated del provider GitHub y AssumeRoleWithWebIdentity; StringEquals aud sts.amazonaws.com y subjects exactos, sin wildcard.
- Repo inmutable `repo:enverdelivery@350055774/ENVER-Backend@1445184810`, environment development, workflow_ref exacto de cada workflow `@refs/heads/develop` y evento. Publisher permite solo push/workflow_dispatch de hardening.yml; Deploy solo workflow_dispatch de deploy-dev.yml. Separación también en trust, no solo permissions.
- GitHub REST autenticado confirmó repo público/id1445184810/owner350055774/creación2026-11-04/default main. Según [referencia OIDC GitHub](https://docs.github.com/en/actions/reference/security/oidc#immutable-subject-claims), los repos posteriores a2026-07-15 mantienen IDs también al personalizar subject. Se corrige el borrador names-only; template ordenado repo/context/workflow_ref/event_name. Configuración actual `use_default:true`; environment development404. **No GitHub writes**, no acceso a secretos ni publicación de tokens/credenciales. Subject final/configuración administrativa y sesión real deben validarse después; no quitar IDs o extender trust para lograr verde.
- Publisher: GetAuthorizationToken solo us-east-1;8acciones ECR de upload/read/DescribeImages exclusivamente repositorio enver-dev-api. Sin SendCommand, EC2, IAM, ALB o CloudFront.
- Deploy: DescribeImages del único repo; SendCommand únicamente documento fijo ENVER-Dev-ImageDeployment-v1 y dos instancias; GetDocument del mismo documento; GetCommandInvocation/DescribeTargetHealth regionales. Sin EC2 DescribeInstances/CloudFront read innecesarios, IAM, secret-read, mutation infra o publicación ECR. Los únicos Resource:* son APIs sin recurso granular, con región; DescribeTargetHealth no admite scope ARN según [AWS SAR](https://docs.aws.amazon.com/service-authorization/latest/reference/list_elasticloadbalancingv2.html). El TG fijo es control del workflow, no granularidad IAM inexistente.

### SSM y GitHub pendientes, fail-closed

SSM GetDocument al documento exacto fue AccessDenied: el perfil de bootstrap carece de ese permiso SSM; **existencia desconocida**, no se afirma que falte ni que exista. No se creó documento ni envió comandos. Documento local versionado y fuente Python revisados: parámetros enums/digests/run validados con ENV_VAR, sin shell/host/rol arbitrario ni secretos embebidos; versión/hash se fijan por el workflow, no por una condición IAM inventada. Requiere provisionamiento/lectura SSM delimitado posterior; no usar AWS-RunShellScript ni mantener bootstrap IAM por ese pendiente.

GitHub **CONFIGURATION PENDING**: crear development/restricciones develop/protecciones autorizadas y configurar el subject de `scripts/deploy/iam/github-oidc-subject.json`; publicación/despliegue siguen cerrados, actores no inventados, flags no habilitados. Main default exige integrar el workflow allí para mostrar botón dispatch; no se hace commit/push/PR. No se ejecuta workflow para probar STS. Endpoint health CloudFront no consultado en este gate. GitHub→STS real y rechazos reales aún NOT EXECUTED; los tests negativos nuevos son contrato local, no prueba cloud.

### Configuración local y verificación acotada

ARNs reales fijos en hardening.yml/deploy-dev.yml; configure-aws-credentials pin/region/cuenta y sesiones temporales se conservan. Helpers publisher/deploy verifican los nuevos nombres exactos. CI no tiene AWS ni id-token.12/10/10jobs y deploy solo workflow_dispatch/concurrency cancel-in-progress=false intactos; id-token solo1job Publisher/7jobs AWS Deploy. No se cambian custodia digest, selección/retorno aprobado, políticas scanner ni código/pruebas productivas.

Antes de aplicar,3pruebas nuevas de contrato trust/permisos PASS. Después,1nueva de wiring workflow y1existente IAM modificada PASS: **5/5dirigidas,0failed/skipped**, sin repetir suite29 ni validaciones pesadas. YAML1.2 unique keys/needs/roles/permisos/composites y Actionlint **PASS**; ShellCheck/Pyflakes externos no ejecutados. Secret scan final nativo y source manifest/diff/index se registran tras los Markdown. IT311/ST223/UT844/Semgrep137/Hadolint/IaC previos no repetidos: sus fuentes técnicas relevantes intactas, no se atribuyen como nuevos resultados. Registros finales privados en `%LOCALAPPDATA%/Temp/opencode/enver-oidc-2026-11-19T19-29-17-411Z/` (inventory, applied, verification, llamadas IAM, local-workflows, escaneo y cierre).

### Retirada de bootstrap: ya procede, no ejecutada

**SÍ pueden retirarse ahora los cuatro statements GithubOidcBootstrapRead/Provider/PublisherRole/DeployRole de ENVER-Terraform-IAM-Runtime**, mediante autorización separada. Provider/roles/inline no dependen en runtime del permiso administrativo de creación; GitHub/SSM/STS pendientes no justifican conservarlo. Preservar los9statements base anteriores, boundary y permisos runtime. v3/hash permanecen intactos aquí; la policy separada ENVER-GitHub-OIDC-Bootstrap-Temporary inventariada tiene0attachments y no se modifica por asociación. No se retira ni modifica automáticamente ningún permiso.

**NEXT GATE=REMOVE TEMP IAM BOOTSTRAP PERMISSIONS. AWS IAM/OIDC MODIFIED=YES; EC2/CLOUDFRONT/ALB/VPC/runtime/secretos/authority/generación MODIFIED=NO; BUILD/PUBLISH/DEPLOY/INITIALIZE/STAGE/COMMIT/PUSH/PR=NO.** Historical initialize total1; ninguna tarea004 COMPLETE/gate integral satisfecho, AUTH-027 no cerrado. Código workflows READY FOR GITHUB, configuración operativa incompleta y runtime pendiente, no habilitación de deployment.
