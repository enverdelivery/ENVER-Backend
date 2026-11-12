# Especificación 004 — CI automático y deployment manual de ENVER

## 1. Identificación, autoridad y estado

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.1.0 / 2026-11-12 |
| Solicitud | SDD — CREAR Y MATERIALIZAR 004-CI-CD; BUILD DOCUMENTAL ÚNICAMENTE |
| Estado documental | MATERIALIZADO PARA REVISIÓN Y APROBACIÓN; decisiones propuestas no ejecutadas |
| Estado técnico | NO IMPLEMENTADO; sin workflows, CI GitHub o deployment acreditados por esta entrega |
| Baseline | `feature/authentication`, `c58ea86e3eddff4befa8f61398dd58d55c4ef985`; tres Markdown de 003 existentes sin commit |
| Artefactos de esta solicitud | Este spec, [plan.md](plan.md) y [task.md](task.md) |

Autoridad: [constitución canónica](../../.specify/memory/constitution.md), [referencia raíz](../../constitution.md), [AGENTS](../../AGENTS.md), [memoria](../../memoria.md), [README](../../README.md). Esta solicitud posterior autoriza **solo los tres Markdown de 004**. No reforma la constitución ni sincroniza documentos de gobierno; la autorización documental no concede implementación, comandos técnicos, recursos, cargos o cambios de fase automáticos.

Clasificación:

- **REQUISITO:** obligación futura confirmada por la solicitud; no significa implementada ni satisfecha.
- **RECOMENDACIÓN / DECISIÓN PROPUESTA:** solución justificada sujeta a aprobación antes de implementarla.
- **DECISIÓN PENDIENTE:** dato, política o selección que necesita resolución explícita; no se completa silenciosamente.
- **ESTADO OBSERVADO:** lectura de archivos/evidencia previa, no una ejecución nueva.

Los requisitos usan CI-001–012, CD-001–012, SEC-001–012 y REL-001–012: **48 requisitos**, todos pendientes de implementación/verificación. La trazabilidad continúa en PLD-CICD, T-CICD, E(T-CICD) y AC-CICD.

## 2. Objetivo, alcance y fuera de alcance

Separar **Continuous Integration automático** de **Controlled Manual Deployment**:

```text
PR hacia main / push aprobado → CI automático → checks + imagen + seguridad
                                             → CI GREEN → NO DEPLOYMENT

Usuario autorizado → GitHub → Actions → Deploy → Run workflow
                   → workflow_dispatch → verificar SHA/CI/digest
                   → AWS → dos instancias/health → smoke → éxito o fallo
```

El primer release público abarca exclusivamente el backend terminado en alcance local/test:

- `POST /api/v1/auth/register`.
- `POST /api/v1/auth/login`.
- `POST /api/v1/auth/logout`.

**Orders queda fuera de esta primera release y de esta entrega.** Se diseñan CI, gates, Testcontainers, seguridad, imagen/ECR, identidad GitHub→AWS, validación Terraform, elegibilidad, operación manual, smoke, rollback, concurrencia y evidencia. No se implementan ahora.

Fuera de alcance: crear `.github/workflows`, scripts ejecutables, tests, `infra/` o Terraform; modificar `Program.cs`, Authentication, Dockerfile, dependencias/SDK/lockfiles o 001/002/003; configurar branch protection, environments, IAM/OIDC/ECR o recursos cloud; ejecutar restore/build/tests/scanners/Docker/Terraform/AWS, publicar imágenes o desplegar. Sin stage, commit ni push.

## 3. Baseline y coherencia con 001/002/003

Fuentes: [001 spec](../001-backend-foundation/spec.md), [001 plan](../001-backend-foundation/plan.md), [002 spec](../002-authentication/spec.md), [002 plan](../002-authentication/plan.md), [002 task](../002-authentication/task.md), [003 spec](../003-infrastructure/spec.md), [003 plan](../003-infrastructure/plan.md) y [003 task](../003-infrastructure/task.md).

**ESTADO OBSERVADO:** Authentication TERMINADA — **LOCAL/TEST REPRESENTATIVO**. E058/GATE-E y memoria §55 conservan UT 684/684, IT 310/310 y ST 223/223, con cero fallidas/omitidas en su evidencia aceptada. Son antecedentes, **no runs GitHub ni mínimos eternos**; CI futuro ejecutará todas las pruebas existentes del SHA. No se repiten ahora. Foundation no está completada integralmente; GATE-G/H de 002 siguen pendientes y AUTH-027 **NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**. Ningún gate 004 modifica esos estados.

[global.json](../../global.json) fija SDK **10.0.403**, `allowPrerelease=false`, `rollForward=disable`. Los proyectos son `net10.0` y usan lockfiles. No se observó `TreatWarningsAsErrors` explícito en los cuatro csproj leídos; eso no elimina warnings/errors impuestos por SDK/analyzers/configuración efectiva. Debe registrarse la política efectiva sin añadir supresiones ni convertir todos los warnings en errores arbitrariamente.

La [fixture compartida](../../tests/Shared/AuthenticationDependenciesFixture.cs) usa Mongo con replica set, Redis `noeviction`, DynamoDB Local y Ryuk; imágenes identificadas por digest y secretos sintéticos por ejecución. DynamoDB Local no prueba IAM/servicio AWS ni Atlas/ElastiCache reales. En esta baseline no se observaron workflows bajo `.github/` ni `infra/` creada por 003.

004 consume, **sin redefinir**, EC2 Linux + Docker en dos AZ, ALB, ACM, Atlas, Redis compartido, DynamoDB authority, CloudWatch, ECR y secretos externos de 003. Readiness/forwarding, superficie protegida de prueba, dominio y recursos siguen pendientes. Los requisitos de seguridad/JWT/cuotas/revocación/errores/no-store de 002 permanecen intactos.

## 4. Requisitos CI automático

| ID | REQUISITO futuro |
| --- | --- |
| CI-001 | CI automático como mínimo en `pull_request` hacia `main`; ningún job CI despliega, aplica Terraform o altera producción. |
| CI-002 | Validar el SHA exacto candidato a deployment. Distinguir SHA de PR head, merge sintético, SHA integrado en main y revisión del workflow; no sustituir su equivalencia por nombre de rama o último run verde. |
| CI-003 | Resolver SDK desde global.json, acreditar 10.0.403 y rollForward disable; fallar ante SDK ausente/incompatible, sin fallback/actualización automática. |
| CI-004 | `dotnet restore --locked-mode` en el alcance completo aprobado; fallar ante lockfile discrepante, error o dependencia no reproducible. No actualizar paquetes/lockfiles automáticamente. |
| CI-005 | `dotnet build --no-restore` con configuración/SDK declarados, cero errores y warnings según política efectiva documentada; sin supresión o cambio arbitrario. |
| CI-006 | Ejecutar todos los tests existentes de Enver.API.UnitTests para el SHA, no fijar 684 como total contractual ni filtrar para obtener verde. |
| CI-007 | Ejecutar todos los tests existentes de Enver.API.IntegrationTests con Docker/Testcontainers y dependencias representativas requeridas, incluidas Mongo, Redis y DynamoDB Local cuando corresponda; no sustituir por mocks como acreditación. |
| CI-008 | Ejecutar todos los tests existentes de Enver.API.SecurityTests; no fijar 223 como total contractual ni confundir estos tests con scans independientes. |
| CI-009 | Gate de pruebas con exitcode nativo cero, cero failed y cero skipped bajo contrato actual; comprobar resultados legibles/completos y descubrimiento no vacío. Error, timeout, cancelación, suite ausente o resultado desconocido no es verde. |
| CI-010 | Aislar recursos por run/job/intento, readiness de fixtures, lifecycle, cleanup, timeouts y diagnóstico saneado; no modificar presupuestos SUT o debilitar tests para acomodar runner lento. |
| CI-011 | Agregar explícitamente todos los jobs obligatorios; ninguno puede omitirse silenciosamente, usar continue-on-error o generar éxito solo por un job resumen. Un control no aplicable necesita condición de alcance acreditada, no un skip arbitrario. |
| CI-012 | Cuando exista `infra/`, incorporar fmt-check, validación Terraform y análisis IaC; plan controlado y ningún apply en CI. Si no existe infra, registrar no aplicabilidad esperada sin aparentar IaC validada. |

### Modelo de eventos y ejecución recomendado

**DECISIÓN PROPUESTA:** PR a main para feedback e integración, y `push` a **main** para validar el commit realmente integrado y producir el candidato publicable. Push a develop u otras ramas relevantes queda pendiente; no habilitar todas por defecto. No cambiar branch protection en esta entrega. La política de deployment inicial propuesta usa commits alcanzables desde main, con run exitoso del **mismo SHA** en CI confiable de main.

Secuencia conceptual: checkout del SHA → SDK fijado → restore locked → build no-restore → UT → IT → ST → controles de código/dependencias/secretos/IaC aplicables → build de imagen → scan/integridad → publicación confiable ECR → **CI GREEN elegible**. El checkout/contenido, resultados y artefactos deben corresponder al mismo SHA.

"Después de CI verde construir imagen" se concreta sin circularidad: **CHECKS DE CÓDIGO VERDES** autoriza construir la imagen; **CI GREEN FINAL** exige además build/scan/integridad y, para un candidato de main, publicación/verificación ECR. Un PR puede terminar CI verde sin publicación por política de confianza, pero su estado es **NO ELEGIBLE PARA DEPLOYMENT**. El run de main debe acreditar el pipeline completo. No recompilar en CD lo ya aprobado.

Un merge sintético verde de PR no prueba el nuevo SHA de merge/squash/rebase; main requiere su propia validación. No usar filtros de rutas para dejar sin checks un SHA que se pretenda desplegar; cualquier optimización debe demostrar mismo contrato completo y aprobarse aparte.

## 5. UT, IT, ST y Testcontainers en GitHub

Proyectos futuros a ejecutar:

| Suite | Proyecto existente | Antecedente local, no número fijo |
| --- | --- | --- |
| UT | [Enver.API.UnitTests](../../tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj) | 684/684 |
| IT | [Enver.API.IntegrationTests](../../tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj) | 310/310 |
| ST | [Enver.API.SecurityTests](../../tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj) | 223/223 |

**DECISIÓN PROPUESTA:** runner Linux GitHub-hosted efímero con Docker Engine accesible, recursos y versión de imagen de runner aprobados. No usar daemon de un entorno productivo ni self-hosted compartido con producción. La etiqueta/version runner y viabilidad del SDK exacto quedan pendientes; no confiar sin prueba en `latest` del runner. No se instala ni ejecuta Docker aquí.

- Preflight futuro: plataforma/arquitectura, SDK, daemon/permisos Docker, disco/memoria/CPU, acceso a registries/imágenes por digest y capacidad de iniciar la topología requerida. Error de infraestructura falla el job, no omite IT/ST.
- Inicialización mediante fixtures existentes: Mongo replica set/ACK/índices, Redis TIME/Lua/noeviction y autoridad local, con namespaces/DB/tablas/puertos aislados. No añadir servicios paralelos que dupliquen sus contenedores o alteren contratos.
- Propuesta inicial de una secuencia UT→IT→ST; si se separan jobs, cada uno recibe exactamente los binarios verificados o recompila declaradamente el mismo SHA/configuración antes de test. `--no-build --no-restore` solo si existe ese build acreditado para el mismo runner/artefacto. No reutilizar binarios locales de evidencia histórica.
- Mantener cleanup de fixture/Ryuk y recoger resultado/diagnóstico seguro incluso al fallar. Runner efímero es última barrera de limpieza, no permiso para volcar logs/credenciales. No desactivar Ryuk por comodidad ni borrar recursos ajenos.
- Timeout de job distinto de límites de dependencia/SUT/fixtures. Aprobar márgenes basados en medición; fixture actual incluye presupuesto de vida de diez minutos, no autorización para aumentarlo. No sleeps/retries de suites para producir verde, ni reclasificar incidentes no determinados como flakes.
- TRX/metadatos/salidas nativas y manifiesto de suites por run, con saneamiento y retención. Verificar todas las suites, total descubierto/ejecutado y cero omisiones; un archivo TRX vacío, truncado o de otro SHA no sirve. La comparación de descubrimiento/ejecución debe contemplar las theories reales sin fijar el conteo histórico.

## 6. Security checks y supply chain

| ID | REQUISITO futuro |
| --- | --- |
| SEC-001 | Scan de vulnerabilidades de dependencias directas/transitivas y estado de soporte con salida procesable; fallo, datos incompletos o hallazgo bloqueante impiden elegibilidad. |
| SEC-002 | Detección de secretos en alcance aprobado de fuente/historia/cambios y artefactos sin publicar el valor encontrado; no depender solo del masking automático. |
| SEC-003 | Análisis estático/source con configuración compatible .NET 10 y revisión de hallazgos; no presentarlo como seguridad universal. |
| SEC-004 | Scan de la imagen final por digest, incluyendo base/paquetes; no aprobar una imagen distinta de la escaneada. IaC scan obligatorio cuando exista infra. |
| SEC-005 | Política de severidad, bloqueos, disponibilidad de feeds, falsos positivos y excepciones formales aprobada antes de declarar security verde; nunca continue-on-error para ocultar fallos. |
| SEC-006 | PR/forks sin AWS/OIDC productivo, secretos runtime o permisos de publicación/deploy; no ejecutar código no confiable con privilegios mediante pull_request_target, workflow_run o artefactos/cachés de origen dudoso. |
| SEC-007 | Credenciales AWS temporales vía OIDC como recomendación principal, trust acotado al repo/contexto real y aud correcto; evitar claves AWS duraderas en GitHub. La implementación requiere aprobación IAM de 003. |
| SEC-008 | Separar rol publisher CI/ECR, rol deployment, provisión Terraform y bootstrap/runtime. CI publisher no puede desplegar, ejecutar SSM productivo ni aplicar Terraform; deployment normal no es administrador AWS. |
| SEC-009 | Permisos GITHUB_TOKEN mínimos por job: contents read; id-token write solo donde OIDC lo requiere; actions read para consultas de CI justificadas; ningún packages write por usar ECR. Otros permisos solo con necesidad documentada. |
| SEC-010 | Runtime JWT/Atlas/Redis/HMAC en AWS Secrets Manager o SSM según 003; GitHub solo referencias/credenciales mínimas del pipeline. Sin secretos en imagen, logs, state, planes públicos, outputs o pruebas publicadas. |
| SEC-011 | Acciones y herramientas con versiones aprobadas, preferencia de pinning a SHA completo en acciones críticas, integridad de artefactos/cachés y entradas validadas sin interpolación shell insegura. No descargar scripts mutables sin verificación. |
| SEC-012 | Evidencia/retención/acceso y privilegios de producción revisados; no ejecutar scripts o workflow desde un ref arbitrario seleccionado por el usuario de deployment. Proteger cambios al workflow y registrar actor/revisión. |

**RECOMENDACIONES, no herramientas adoptadas:** aprovechar analyzers/SDK y auditoría de paquetes compatibles con .NET 10; evaluar un detector de secretos mantenido, un SAST compatible, un scanner de contenedor y uno IaC (posible herramienta compartida). Justificar alcance, licencia, costo, límites de repo privado, reportes y mantenimiento antes de seleccionar productos/versiones. Evitar pagar o añadir servicios duplicados sin necesidad.

**DECISIONES PENDIENTES:** selección concreta, severidades bloqueantes, antigüedad máxima de análisis/feeds y forma de verificar excepciones con responsable, justificación, alcance y vencimiento. Una excepción no convierte el finding en inexistente ni exime restore/build/tests/integridad/manualidad. Hasta aprobar política/controles no existe security verde acreditable.

## 7. Imagen, ECR e identidad de release

| ID | REQUISITO futuro |
| --- | --- |
| REL-001 | Imagen .NET 10 reproducible, mismo SHA/configuración/lockfiles aprobados y base identificable; no secretos en capas/contexto, runtime non-root y revisión del Dockerfile existente sin cambios arbitrarios. |
| REL-002 | Tag de trazabilidad por SHA completo e identidad de deployment por digest inmutable; `latest` no es identidad aceptable. No sobrescribir silenciosamente el mapeo SHA→digest. |
| REL-003 | Construir tras checks de código verdes; escanear/verificar imagen antes de publicación elegible. Publicación ECR solo desde contexto CI confiable y con mínimo privilegio; PR no publica en ECR productivo. |
| REL-004 | Cadena exacta repo+Git SHA↔workflow/revisión+CI run/intento↔imagen↔digest↔deployment/actor/instancias. Un input digest, etiqueta o label OCI por sí solo no acredita esa cadena. |
| REL-005 | Confirmar integridad del manifiesto/artefactos y digest consultado en ECR tras push; imagen desplegada igual a la validada. No rebuild en CD ni resolver de nuevo un tag mutable después del preflight. |
| REL-006 | Consumir inputs/outputs y permisos de 003; no duplicar AWS ni cambiar EC2/Docker, ALB, ACM, Atlas, Redis, authority, CloudWatch/ECR. Incompatibilidades requieren revisión explícita. |
| REL-007 | Terraform y app deployment tienen un único ownership por campo; la versión de aplicación no puede ser sobrescrita por reconcile/arranque/replace de infraestructura. |
| REL-008 | Preparar rollback manual por digest anterior conocido/aprobado y configuración compatible; sin rollback ciego de Redis/revocaciones/authority/material JWT ni cambio de namespace que reinicie cuotas. |
| REL-009 | Serializar deployments/rollback y mutaciones relacionadas de production, sin cancelar a mitad una operación activa. Manejar ejecuciones pendientes/obsoletas y fallo del runner. |
| REL-010 | Registrar evidencia académica CI/build/tests/security/imagen/digest/deploy manual/actor/fecha/SHA/targets/HTTPS/smoke, saneada y accesible con retención aprobada. |
| REL-011 | Controlar costos/límites de Actions, runners, almacenamiento/retención de artifacts, ECR, scanners, red/transferencia y AWS; no afirmar gratuidad sin cuenta/plan/consumo validados. |
| REL-012 | Gates existentes de 001/002/003 no se satisfacen por CI verde o por crear documentos; aceptar solo el alcance probado, mantener AUTH-027 residual/diferidos y pedir autorización por incremento técnico. |

**DECISIÓN PROPUESTA:** usar ECR definido por 003, imagen identificada por digest y tag SHA, labels mínimos de revisión/fuente/versión de build, manifiesto de release protegido con IDs de run/intento, hashes de resultados, digest, controles y policy vigente. Verificar procedencia desde APIs/metadatos confiables, no aceptar un JSON editado por un PR como prueba. Retener anterior para rollback. SBOM/provenance/firmas son opciones posteriores a evaluar, no capacidades ya existentes ni requisitos de herramientas específicas.

Puede existir más de un build del mismo SHA por rerun o base/toolchain distinta: no asumir digest idéntico solo por el SHA. Aprobar un único candidato identificado por run/intento/digest y no mutar su tag inmutable; ante conflicto rechazar/revisar, no reemplazar el candidato en silencio. Reproducibilidad exige registrar inputs completos, no prometer igualdad bit a bit no demostrada.

ECR/roles deben existir por trabajo autorizado de 003 antes de publicar. El **bootstrap inicial de registry/roles/state** necesita coordinación explícita con 003: no exigir imagen ya publicada para crear su propio repositorio, ni cerrar aceptación completa de 003 antes de probar CD de 004. Si la secuencia de autorizaciones de 003 requiere ajuste, registrarlo como dependencia pendiente (DP-CICD-12), no editar 003 ni provisionar desde CI de 004.

## 8. Requisitos CD manual y política de ramas

| ID | REQUISITO futuro |
| --- | --- |
| CD-001 | Workflow de deployment separado de CI, único trigger productivo `workflow_dispatch`. Ningún push, PR, merge, release/tag, schedule, workflow_run o llamada automatizada puede iniciar producción. |
| CD-002 | Responsable autorizado inicia en GitHub → Actions → Deploy → **Run workflow**; registrar actor, motivo, SHA/run/digest/ambiente. No bots llamando dispatch para eludir manualidad. |
| CD-003 | Antes de mutaciones verificar repositorio, SHA elegible, workflow confiable, CI verde completo del mismo SHA/run/intento, política de gates/security, ECR/digest e integridad; ausente, pendiente, fallido, cancelado, skipped o incierto → **DEPLOYMENT RECHAZADO**. |
| CD-004 | Workflow ejecutado desde ref de control confiable (main propuesto), con implementación en default branch para habilitar botón; no ejecutar versión del workflow/script arbitraria enviada como input. Verificar main/default branch reales antes de configurar. |
| CD-005 | Usar GitHub Environment `production`, historial y variables/referencias mínimas; protección/branch rules y reviewers cuando plan/visibilidad lo permitan. workflow_dispatch es la acción manual necesaria; no depender obligatoriamente de segunda aprobación no disponible. |
| CD-006 | Resolver inputs a SHA completo y digest antes de mutar, validar formatos/ownership/region/cuenta/destino y congelar la selección. Revalidar elegibilidad tras adquirir exclusión mutua y antes de activar; no confiar en un preflight obsoleto. |
| CD-007 | Desplegar exactamente el digest aprobado usando permisos mínimos y mecanismo EC2 acordado con 003; esperar estabilidad acotada y comprobar mínimo dos instancias con mismo digest y targets saludables. |
| CD-008 | Ejecutar smoke públicos HTTPS con identidad de prueba controlada, nunca credenciales reales de usuarios; TLS, liveness/readiness según contrato, ALB y los tres comandos de Authentication. |
| CD-009 | Acreditar estado compartido A/B y cuotas/revocación cuando exista superficie autorizada; vincular a GATE-I-I de 003. Falta de prueba requerida es pendiente/bloqueo, no éxito inferido por Logout repetido o round-robin supuesto. |
| CD-010 | Fallo de deploy, AWS API, imagen, Terraform, dependencia, target o smoke → estado fallido/incierto explícito y evidencia, no éxito. Sin retries ciegos de POST/no idempotentes o apertura de seguridad para recuperar disponibilidad. |
| CD-011 | Rollback también manual/controlado por workflow_dispatch y digest conocido, con las comprobaciones aplicables y registro propio; no rollback automático de producción ni uso de latest. |
| CD-012 | Terraform apply nunca por push/PR/merge ni CI; si se incorpora, solo en flujo manual autorizado con plan revisado, costo, state remoto/lock y rol separado. Cambiar aplicación no autoriza cambios estructurales implícitos. |

**DECISIÓN PROPUESTA inicial — main, no tags:** permitir exclusivamente SHA completos alcanzables desde `main` del repositorio esperado, validados por CI confiable de main para ese SHA, conservando candidato por digest. Esto facilita una cadena de custodia única y evita tags movibles/eventos release ambiguos. Tags/releases aprobados se dejan como ampliación pendiente, nunca trigger automático de CD. `feature/authentication` actual no se declara elegible ni integrada a main.

El dropdown Branch de Run workflow debe quedar limitado/validado a la revisión confiable de main; el **SHA a desplegar** es input separado y puede ser un commit previo elegible de main. Que el ref del workflow sea main no demuestra que el SHA candidato pertenezca a main. La ruta manual no concede permiso al actor para eludir gates, sobreescribir digest o elegir cuenta/destino arbitrarios.

GitHub también permite técnicamente dispatch vía CLI/API; el procedimiento aprobado aquí es el botón Run workflow de una persona autorizada. No afirmar que el evento distingue UI de API por sí mismo. Evitar tokens/apps/roles que automaticen ese dispatch, limitar acceso y auditar actor; controles concretos de autorización/plan quedan pendientes.

## 9. Preflight, OIDC, permissions y secretos

Preflight futuro, sin mutaciones: validar actor/repositorio/ref de control → resolver SHA de main → localizar run confiable esperado (workflow ID/revisión, event push main, head_sha, run ID/intento y conclusiones de cada gate) → comprobar artifact/manifiesto/digest ECR de cuenta/región/repositorio aprobados → verificar vigencia de scans/policy → congelar selección. No buscar solo "último success" ni confiar en un check de igual nombre creado por otro origen. Si GitHub/ECR no pueden acreditarlo, denegar.

Para lectura de ECR puede requerirse OIDC antes de concluir el preflight: obtener sesión de **solo lectura** o rol de verificación sin mutación; no ejecutar despliegue ni asumir privilegios de escritura hasta validar. Consulta GitHub con permisos read apropiados. Adquirir exclusión de production y revalidar antes de mutar. Evitar TOCTOU de tags/ramas/artifact y no permitir inputs que introduzcan comandos.

**RECOMENDACIÓN:** GitHub OIDC→IAM roles temporales definidos/provisionados en 003, no claves AWS largas. Trust comprueba emisor, `aud` previsto para STS y `sub` exacto del repo/contexto; validar el formato real de subjects (incluidos IDs inmutables si aplican), sin wildcard que habilite forks/PR/otros repos.

- Publisher: contexto confiable main; solo ECR necesario, sin production/SSM/Terraform.
- Deployment: environment production, mínimo EC2/SSM/ELB/ECR lectura para operación aprobada; no permisos globales de secretos/DB/authority/bootstrap.
- Terraform apply: rol de provisión separado, solo en tramo manual autorizado. State remoto/locking de 003 obligatorio antes de runner GitHub.
- Runtime: rol de instancia obtiene secretos AWS de 003; pipeline no necesita leer material JWT/Atlas/Redis/HMAC. Smoke usa secreto sintético efímero, tratado como sensible aunque no sea real.

Con environment, el `sub` OIDC referencia el environment y no necesariamente codifica rama; complementar con restricciones de environment disponibles, control de workflow/ref confiable y protección de cambios. No asumir que sub de environment por sí solo prueba main, ni que id-token write concede acceso AWS. Si el plan no permite una protección prevista, documentar alternativa equivalente y aprobarla, o bloquear; no relajar trust ni exigir comprar segunda aprobación por defecto.

| Contexto | Permisos GitHub propuestos | AWS |
| --- | --- | --- |
| PR / checks de código | contents: read; sin id-token ni writes por defecto | Ninguno |
| Publisher confiable main | contents: read; id-token: write solo job OIDC | ECR acotado |
| Preflight manual | contents: read; actions: read para verificar runs; id-token solo si lectura ECR lo necesita | Lectura/verificación acotada |
| Deployment manual production | contents: read; actions: read si consulta CI; id-token: write en job AWS | Rol deploy limitado |
| Plan/apply autorizado | read GitHub y OIDC mínimo necesario según operación | Rol Terraform separado y alcance aprobado |

ECR no usa el permiso GitHub `packages`; dejarlo sin write salvo otro registry adoptado explícitamente. Uso de API para deployment history puede requerir `deployments: write` justificado; uploads/attestations/checks pueden tener requisitos propios a resolver, no conceder `write-all`. Credenciales de checkout no persistidas innecesariamente; cachés no son fuente confiable de resultados y no comparten datos sensibles entre PR/publicación.

## 10. Terraform y ownership de versión

**CI futuro cuando exista infra:** `terraform fmt -check`, inicialización de validación sin backend/credenciales productivos cuando sea viable (por ejemplo backend deshabilitado), `terraform validate` e IaC scan. Sin inventar directorio ausente ni ejecutar ahora; no fallar el CI actual por infraestructura que aún no forma parte del alcance, pero sí fallar si existe y su validación requerida se omite.

`terraform plan` puede leer datos sensibles y requiere provider/estado; mantenerlo en flujo controlado, no con credenciales productivas en PR. Estado/plan cifrados y restringidos, sin subir plan crudo a comentario/artifact público. Freshness, aprobación, mismas entradas/lock y relación plan→apply por resolver con 003. Un resultado detailed-exitcode que indica cambios no debe confundirse con fallo; errores/desconocidos bloquean según contrato de comandos aprobado.

**DECISIÓN PROPUESTA de ownership:** Terraform/003 administra infraestructura estructural (VPC/SG/IAM/EC2/ALB/ECR/telemetría y referencias), **deployment manual es el único escritor de la versión/digest activa**. La definición estructural no contendrá un digest por defecto que vuelva a imponer versión antigua en refresh/apply/reemplazo. El deploy no editará state ni usará ignore_changes como solución meramente nominal sin demostrar ownership real.

Contrato requerido antes de implementar: fuente persistente/no secreta de versión activa, quién la escribe, cómo arranca/reemplaza una EC2 con ese digest, cómo ambas instancias convergen, rollback e interacción con maintenance/apply. Su ubicación/mecanismo (por ejemplo referencia operacional administrada fuera de Terraform) queda pendiente y debe aprobarse con 003; no crear un servicio nuevo ni simular resolución escribiendo un tfvars desde CD. Prueba de reconciliación/reemplazo no puede revertir la versión aprobada. Hasta entonces DP-I-14 de 003 no se declara resuelta, aunque la **estrategia conceptual** queda definida aquí.

Apply, si se adopta, solo en tramo manual con plan revisado/rol propio y exclusión mutua con deployment/rollback; no aplicar infraestructura para cada release de aplicación sin necesidad. No destruir datos, recrear authority o resetear Redis como parte de pipeline.

## 11. Deployment, smoke y verificación distribuida

Flujo manual futuro:

1. Persona autorizada usa Run workflow, elige SHA elegible/run y motivo; production fijo, sin destinos libres.
2. Verificar CI completo verde, repo/ref/SHA/policy y resolver digest verificado; lectura AWS solo con permisos mínimos.
3. Adquirir exclusión production y revalidar candidato/estado anterior; obtener OIDC de deployment.
4. Registrar digest anterior/configuración compatible y desplegar digest exacto en EC2 por mecanismo acordado (SSM es candidato coherente con 003, no implementación aprobada).
5. Esperar estabilidad con timeout aprobado, comprobar inventario mínimo dos, digest efectivo A/B y dos targets saludables; no basta que el comando AWS haya sido aceptado.
6. Ejecutar smoke HTTPS, comprobar distribución/seguridad según contrato y recoger telemetría saneada.
7. Solo con todo acreditado registrar éxito; si falla, declarar fallo/incierto, conservar evidencia y esperar recuperación/rollback manual autorizado.

### Smoke mínimo propuesto

- TLS válido sin ignorar validación de certificados, DNS/ALB y liveness `/health`; readiness solo tras implementación/evidencia separadas de 003, no ruta ficticia.
- Register con identidad institucional sintética controlada y única bajo dominio aprobado de 002 → 201 conforme contrato, sin token automático; Login → 200 y JWT en memoria protegida; Logout → 204 y repetición conforme contrato. Nunca registrar cuerpo/token/password/Authorization. Verificar no-store y errores mínimos seguros seleccionados.
- Usuario/cuenta de prueba autorizado, sin usar buzón/persona reales ni agregar un endpoint de borrado. Aprobar retención/limpieza por operación de mantenimiento separada de runtime/deploy; cuotas/origen compartido pueden afectar la prueba, no deshabilitarlas. Evitar retries ciegos de Register/Login y acumulación infinita de cuentas.
- Evidencia de mínimo dos targets saludables, mismo digest y solicitudes atendidas por A/B mediante telemetría interna autorizada, no cabecera pública de diagnóstico. Varias requests al ALB no garantizan por sí solas cambio de instancia.

### Cross-instance y límites de aceptación

Login vía ALB, token aceptado en política protegida normal A/B, Logout en una instancia y rechazo normal posterior en otra, cuotas agregadas y fail-closed conforme 002/003. **Logout admite reconfirmación de token revocado auténtico y vigente**: su 204 repetido no es prueba de rechazo normal. Los tres endpoints iniciales no proporcionan por sí solos una operación protegida normal de negocio para demostrar ese rechazo.

Usar únicamente superficie protegida ya autorizada o un incremento de prueba separado; no agregar `/me`, Orders ni endpoint productivo de diagnóstico. Si falta superficie/atribución fiable, registrar evidencia parcial y mantener pendientes CD-009, criterio aplicable, GATE-I-I de 003 y aceptación final correspondiente; no cerrar por "cuando sea posible" sin prueba. Smoke básico puede pasar sin acreditar todas las garantías distribuidas, pero **no permite declarar exitosa la primera release aceptada ni satisfecho GATE-CD-C** hasta resolver el contrato requerido. Tampoco una prueba finita elimina el riesgo silencioso residual de AUTH-027.

## 12. Fallos, rollback y concurrencia

| Situación | Comportamiento exigido |
| --- | --- |
| CI no verde/missing/otro SHA o digest sustituido | Rechazar antes de mutación; registrar motivo seguro |
| ECR/AWS/GitHub no verificable | Fallar preflight; no degradar a tag/latest o credencial alternativa permanente |
| API AWS falla tras despacho/timeout | Estado incierto: reconciliar inventario/versión real con lectura acotada; no repetir mutación ciegamente ni declarar éxito |
| Una instancia unhealthy/versión mixta | No éxito; drenaje/contención segura según 003 y recuperación/rollback manual |
| Dependencia o autoridad no disponible/incierta | Conservar fail-closed/503; no abrir trust, reemplazar por fake o resetear state |
| Smoke falla/no se puede acreditar | Deployment fallido o pendiente de verificación, nunca success aunque proceso de deploy salga cero |
| Terraform falla | No continuar aplicación como si infraestructura estuviera válida; reconciliar state/plan seguro, sin apply/destroy automático |
| Runner cancelado/falla a mitad | No asumir rollback ni estabilidad; registrar ejecución interrumpida, exigir reconciliación antes de nueva mutación |

**Rollback propuesto:** otra ejecución manual del workflow confiable con operación rollback, digest anterior conocido, origen CI y compatibilidad aprobados, revisión vigente de seguridad y referencias runtime. Si evidencia anterior caducó/falta, resolver/revalidar antes de rollback, sin exención silenciosa. Repetir health/smoke/atribución y registrar actor/resultado. No recompilar antiguo SHA para fingir el mismo artefacto ni restaurar Redis/authority/claves JWT por defecto.

**Concurrencia propuesta:** grupo conceptual `production-deploy`, compartido por deploy, rollback y cualquier apply manual que pueda afectar el mismo destino; `cancel-in-progress: false` para mutaciones. Un grupo Actions no es garantía de FIFO ni cola durable; comprobar comportamiento de pendientes en versión vigente y no garantizar ejecución de cada solicitud. No cancelar un deploy en curso por otro push. Revalidar SHA/digest cuando llega su turno, y reconciliar estado incierto al perder runner; el lock de state no sustituye exclusión de releases. Resolver coordinación entre workflows/repos y mantenimiento fuera de Actions antes de habilitarlos.

## 13. Evidencia académica y costo

Paquete futuro: URL/run de CI con SHA/revisión de workflow, SDK/locked restore/build/warnings, UT/IT/ST completos con exitcodes/TRX y saneamiento, reportes security/policy, imagen/build/scan/ECR/digest, manifiesto íntegro, evento manual/actor/fecha/inputs no sensibles, historial environment, evidencia dos instancias/targets/digest/HTTPS/health/smoke/estado distribuido y rollback/fallos controlados. Vincular E(T-CICD)→AC→gate y evidencia compartida de 003, sin contabilizar dos veces ni cerrar gates por asociación.

Retención/ubicación/accesos/costos pendientes de aprobación. Logs Actions, artifacts, TRX y capturas pueden contener datos sensibles: revisar antes de upload, restringir y no publicar JWT ni credenciales de prueba. La evidencia histórica temporal local de 002 no es sustituto de CI/ECR/cloud. Reportes perdidos o inaccesibles bloquean la acreditación que depende de ellos.

GitHub Actions/minutos/runners, artifact/cache storage, retención/egreso, ECR, scans/feeds/licencias, OIDC/servicios invocados y transferencia pueden tener límites o cargos. Validar plan/visibilidad/cuenta/región al implementar; no prometer gratuidad de GitHub-hosted, herramientas o ECR. Limitar ejecuciones/concurrencia/retención sin omitir seguridad o suites.

## 14. Criterios de aceptación futuros

Todos **PENDIENTES / NO VERIFICADOS**. Gates en [task.md §3](task.md#3-gates-formales).

| Criterio | Resultado exigido | Requisitos | Gates |
| --- | --- | --- | --- |
| AC-CICD-01 | PR a main y push main propuesto activan CI; ningún evento automático inicia deployment/apply | CI-001, 002, CD-001, 002, 012 | GATE-CI-A, GATE-CD-B |
| AC-CICD-02 | SDK exacto, restore locked y build no-restore/cero errores, warnings efectivos registrados | CI-003–005 | GATE-CI-B |
| AC-CICD-03 | UT/IT/ST completos del SHA, resultados nativos, cero failed/skipped y no suites vacías/omitidas | CI-006–011 | GATE-CI-C |
| AC-CICD-04 | Runner Docker/fixtures aislados y cleanup/timeouts/diagnóstico saneado acreditados | CI-007, 008, 010, SEC-006 | GATE-CI-C |
| AC-CICD-05 | Scans requeridos y política aprobada, negativos bloqueantes sin false green; IaC cuando exista | CI-012, SEC-001–005 | GATE-CI-D |
| AC-CICD-06 | Imagen reproducible/rastreable, escaneada, publicada ECR e integridad/digest verificados | SEC-004, 011, REL-001–005 | GATE-CI-E |
| AC-CICD-07 | OIDC/trust/contexto/permisos mínimos por rol sin secretos runtime ni acceso AWS desde PR | SEC-006–010, 012 | GATE-CD-A |
| AC-CICD-08 | Actor manual/ref confiable/main/environment, mismo SHA/run/CI/digest; rechazos de candidatos inválidos | CD-001–006, SEC-012, REL-004, 005 | GATE-CD-B |
| AC-CICD-09 | Terraform CI sin apply, plan/state protegidos; apply solo manual autorizado y ownership sin dos escritores | CI-012, CD-012, REL-006, 007 | GATE-CI-D, GATE-CD-B |
| AC-CICD-10 | Deploy digest exacto, mínimo dos EC2/targets saludables, espera acotada y no éxito por simple ACK | CD-007, 010, REL-004–007 | GATE-CD-C |
| AC-CICD-11 | TLS/health/ALB y Register/Login/Logout públicos con identidad sintética, sin secretos ni usuarios reales | CD-008, SEC-010, REL-006, 010 | GATE-CD-C |
| AC-CICD-12 | Estado A/B, token/revocación normal/cuotas acreditados por superficie autorizada o bloqueo explícito de aceptación | CD-009, REL-006, 012 | GATE-CD-C |
| AC-CICD-13 | Fallos API/imagen/targets/smoke/dependencias/Terraform no producen deployment exitoso ni bypass | CD-003, 010, 012, REL-012 | GATE-CD-B, GATE-CD-C |
| AC-CICD-14 | Rollback manual por digest anterior compatible; exclusión deploy/rollback/apply sin reset de seguridad | CD-011, 012, REL-007–009 | GATE-CD-D |
| AC-CICD-15 | Evidencia íntegra/saneada de CI a deploy/actor/SHA/digest/targets/smoke, retención y costo aprobados | SEC-011, 012, REL-004, 010, 011 | GATE-CD-D |
| AC-CICD-16 | Revisión final requisito→decisión→tarea→evidencia→gate; primera release solo Authentication y límites 001/002/003 preservados | REL-006, 012, CD-008, 009 | Todos; cierre final GATE-CD-D |

## 15. Decisiones pendientes y riesgos

| ID | DECISIÓN PENDIENTE | Bloquea |
| --- | --- | --- |
| DP-CICD-01 | Repo/owner/default branch reales, protección main, actores autorizados y aprobación política main/push | Configuración de eventos/elegibilidad/dispatch |
| DP-CICD-02 | Plan/visibilidad GitHub, environments/protecciones disponibles y presupuesto/minutos/retención | production/permisos/evidencia |
| DP-CICD-03 | Runner Linux/arquitectura/version/capacidad, Docker/red, SDK exacto disponible y timeouts | Restore/build/UT/IT/ST |
| DP-CICD-04 | Warnings efectivos, configuración build, descubrimiento/aggregación de tests y evidencias | GATE-CI-B/C |
| DP-CICD-05 | Herramientas/versiones/licencias scans y pinning; severidad, feeds, vigencia/excepciones | Security verde y elegibilidad |
| DP-CICD-06 | Formato/custodia manifiesto, integridad/provenance opcional y retención/rerun/revocación de candidato | Imagen/digest/CI proof |
| DP-CICD-07 | Trust OIDC/sub real/roles/ARN/cuenta/región, scopes y coordinación IAM con 003 | ECR y deployment |
| DP-CICD-08 | Ownership operativo: fuente persistente de versión activa y mecanismo EC2/SSM/drenaje/reemplazo | Deploy sin drift con Terraform |
| DP-CICD-09 | Plan/apply/state remoto/locking y exclusión con mutaciones manuales | Integración Terraform privilegiada |
| DP-CICD-10 | Identidad sintética institucional, secreto efímero, cuotas/retención/limpieza; smoke/readiness | Pruebas públicas seguras |
| DP-CICD-11 | Superficie protegida y atribución A/B autorizadas, fallos/rollback/timeout/reconciliación | Prueba distribuida y GATE-CD-C/D |
| DP-CICD-12 | Bootstrap registry/roles/state de 003 y secuencia compartida sin dependencia circular de primeras imágenes/deploy | Primera publicación y deployment |
| DP-CICD-13 | Retención/acceso de evidencia, destino environment y respuesta operativa/alertas | Evidencia académica/operación |
| DP-CICD-14 | Versiones de workflows/actions y protección de cambios, política de cancelación/pendientes | Supply chain y concurrencia |

Riesgos principales: confundir SHA de PR con main; verde parcial/skip; SDK ausente; runner insuficiente y fallos de fixture; acciones/inputs/artifacts no confiables con OIDC; trust de environment demasiado amplio; tag mutable/TOCTOU o rerun que sustituye digest; comandos SSM privilegiados; state/plan/TRX/logs sensibles; cambios automáticos de versión por Terraform; falsa prueba de revocación usando Logout; smoke que crea usuarios indefinidamente; cancelación a mitad de deploy; expiración de evidencia; cargos/licencias inesperados. Cada uno requiere mitigación comprobada en plan/task, no promesas de seguridad absoluta.

No resolver DP por ejecución. Si hay incompatibilidad con 003 o faltan recursos/readiness/privilegios/presupuesto, registrar bloqueo y solicitar revisión explícita. Esta entrega no modifica 003.

## 16. Referencias y revisión documental

Referencias oficiales consultadas para el diseño; revalidar capacidades/plan/versions al implementar: [Run workflow y default branch](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow), [OIDC AWS y trust](https://docs.github.com/en/actions/how-tos/secure-your-work/security-harden-deployments/oidc-in-aws), [environments/protecciones](https://docs.github.com/en/actions/reference/workflows-and-actions/deployments-and-environments).

Revisión de alcance: coherencia con 001 sin completarlo; Authentication local/test conservada; 003 intacta y no implementada; CI automático sin deploy; workflow_dispatch obligatorio y persona autorizada; apply no automático; OIDC recomendado; sin secretos; Orders fuera y primera release Authentication. Todos los gates/tareas pendientes. Solo se materializan estos tres Markdown; sin workflows, AWS, Terraform, ejecución técnica, stage, commit o push.
