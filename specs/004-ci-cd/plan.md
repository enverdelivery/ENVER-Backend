# Plan 004 — CI automático y Controlled Manual Deployment

## 1. Control y alcance autorizado

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.1.0 / 2026-11-12 |
| Fuente | [spec.md](spec.md), solicitud SDD — CREAR Y MATERIALIZAR 004-CI-CD |
| Estado | PLAN PROPUESTO MATERIALIZADO PARA REVISIÓN; SIN EJECUCIÓN AUTORIZADA |
| Catálogo | [task.md](task.md): 30 tareas T-CICD-001–030, PENDIENTE / NO EJECUTADA |
| Gates | Cinco GATE-CI y cuatro GATE-CD, PENDIENTES / NO SATISFECHOS |

Aplican [constitución](../../.specify/memory/constitution.md), [001](../001-backend-foundation/spec.md), [002](../002-authentication/spec.md) y [003](../003-infrastructure/spec.md). La solicitud solo crea spec/plan/task de 004. No implementa workflows, scripts, Terraform, Docker o tests, no cambia configuraciones/seguridad del producto, no configura GitHub/AWS y no autoriza comandos técnicos, cargos, stage/commit/push. 003 y su trabajo no versionado se conservan intactos.

Las siguientes PLD son **DECISIONES PROPUESTAS**, no implementadas. Se distinguen de los **REQUISITOS** de spec y de DP-CICD-01–14 **PENDIENTES**. Ninguna decisión de este plan se adopta silenciosamente por ejecutar una tarea.

## 2. Decisiones propuestas y trazabilidad

| ID | Decisión propuesta / justificación | Requisitos principales | DP |
| --- | --- | --- | --- |
| PLD-CICD-001 | Dos workflows conceptuales separados: CI automático y Deploy manual, solo workflow_dispatch/Run workflow. Sin producción por push, PR, merge, workflow_run o release | CI-001, CD-001, 002 | 01, 14 |
| PLD-CICD-002 | PR a main más push main recomendado; deployment solo SHA alcanzable de main con CI de main del SHA exacto. PR merge verde no basta; tags posteriores no habilitados | CI-002, CD-003, 004, 006 | 01, 06 |
| PLD-CICD-003 | SDK de global.json 10.0.403/disable, restore locked y build no-restore; conservar warnings efectivos y fallar con errores sin actualizar paquetes | CI-003–005 | 03, 04 |
| PLD-CICD-004 | UT→IT→ST, sin filtros/omisiones, TRX/exitcode y agregación fail-closed; conteos históricos no contractuales | CI-006–009, 011 | 03, 04 |
| PLD-CICD-005 | Runner Linux efímero con Docker, fixtures Testcontainers existentes (Mongo replica set, Redis, DynamoDB Local, Ryuk), aislamiento/cleanup y timeout de job separado del SUT | CI-007, 008, 010 | 03, 04 |
| PLD-CICD-006 | Scans dependencias/secretos/source/imagen e IaC cuando exista; seleccionar herramientas por compatibilidad/licencia/costo. Sin falso verde ante fallo de control | CI-012, SEC-001–005 | 05 |
| PLD-CICD-007 | PR sin privilegios AWS ni secretos; jobs confiables no consumen scripts/cachés/artefactos no verificados de PR, evitando escalamiento por pull_request_target/workflow_run | SEC-006, 011, 012 | 01, 14 |
| PLD-CICD-008 | Checks de código verdes antes de build de imagen; CI GREEN final exige scan/integridad y publicación main. Tag SHA/digest, no latest y no rebuild en CD | REL-001–005 | 05, 06 |
| PLD-CICD-009 | ECR de 003, publisher OIDC separado, misma imagen escaneada/publicada; manifiesto de release protegido une SHA/run/intento/digest y no se reemplaza silenciosamente en rerun | REL-002–006, SEC-008 | 06, 07, 12 |
| PLD-CICD-010 | OIDC con aud/sub/contexto exactos y formatos reales; roles publisher/verificación/deploy/provisión/instancia separados, sin claves largas ni IAM creado por 004 | SEC-007–009, CD-005 | 02, 07 |
| PLD-CICD-011 | production con historial y protección disponible; main/ref confiable, workflow en default branch. Segunda aprobación opcional según plan; persona inicia Run workflow siempre | CD-001–006, SEC-012 | 01, 02, 14 |
| PLD-CICD-012 | Preflight prueba SHA/CI completo/run-intento/policy/ECR/digest, permisos read antes de mutación y revalidación bajo lock; fail-closed si APIs o evidencia faltan | CD-003, 006, SEC-009, REL-004, 005 | 05, 06, 07 |
| PLD-CICD-013 | Runtime secrets quedan en AWS de 003; GitHub solo lo necesario al pipeline; secretos sintéticos temporales para smoke, sin logs/cuerpos sensibles | SEC-010, REL-010, CD-008 | 07, 10, 13 |
| PLD-CICD-014 | Terraform fmt/validate/IaC condicionado a infra; plan protegido/controlado y apply solo manual/rol separado. State remoto/lock antes de runner | CI-012, CD-012, SEC-004, REL-006 | 09, 12 |
| PLD-CICD-015 | Infra estructural de Terraform; versión/digest activo solo del deploy. Fuente persistente no secreta y reemplazo/reconcile acordados con 003 antes de implementar | REL-007, CD-007, 012 | 08, 09, 12 |
| PLD-CICD-016 | Deploy manual por mecanismo EC2 acordado (SSM candidato), mismo digest A/B, estabilidad/targets y luego smoke; ACK no significa éxito | CD-007, 010, REL-006 | 08, 10, 11 |
| PLD-CICD-017 | Smoke TLS/health/Auth sintético y evidencia A/B; revocación normal requiere superficie autorizada distinta de Logout, sin endpoint nuevo implícito | CD-008, 009, REL-012 | 10, 11 |
| PLD-CICD-018 | Rollback también manual a digest previo aprobado y compatible; sin restaurar estado Redis/authority/JWT. Fallos e incertidumbre requieren reconciliación, no retries ciegos | CD-010, 011, REL-008 | 08, 11 |
| PLD-CICD-019 | Grupo production-deploy compartido para deploy/rollback/apply, sin cancelar mutación activa; no prometer FIFO/cola durable ni ignorar mantenimiento externo | REL-009, CD-006, 012 | 09, 14 |
| PLD-CICD-020 | Acciones críticas fijadas por SHA, evidencias saneadas/retención/costo y revisión negativa/final; no cerrar 002/003 por green CI ni afirmar cloud existente | SEC-011, 012, REL-010–012 | 02, 05, 06, 13, 14 |

## 3. Fases, dependencias y criterios de avance

Las fases son dominios de trabajo, no orden numérico rígido: OIDC precede a publicación y los contratos smoke/rollback/concurrencia preceden al primer deployment aceptable. La implementación de archivos y las verificaciones runtime son avances distintos; escribir un workflow no acredita que corrió.

| Fase | Tareas / resultado futuro | Dependencias | Criterio de avance | Gates |
| --- | --- | --- | --- | --- |
| **CICD-1 Baseline GitHub Actions** | 001 política/repo/costo; 002 estructura CI/Deploy y permisos | Autorización técnica y DP-01/02/14 | Contratos/eventos/refs separados revisados, sin producción automática. Skeleton validado antes de habilitar credenciales | GATE-CI-A |
| **CICD-2 CI restore/build** | 003 SDK/restore; 004 build/warnings | CICD-1 | SDK exacto, restore locked y build no-restore con salida real/errores bloqueantes | GATE-CI-B |
| **CICD-3 UT** | 005 suite UT; 006 resultados nativos/TRX | CICD-2 | Todos UT presentes pasan, cero failed/skipped; falta/archivo inválido falla, no mínimo fijo 684 | GATE-CI-C |
| **CICD-4 IT/Testcontainers** | 007 runner/fixtures; 008 IT | CICD-2/3; runner/capacidad aprobados | Docker e imágenes aisladas, dependencias reales/locales representativas, IT completa/cleanup sin retocar SUT | GATE-CI-C |
| **CICD-5 ST** | 009 ST; 010 agregación UT/IT/ST | CICD-3/4 | ST completa y agregación que falla ante rojo, cancelado, vacío, skip o suite ausente | GATE-CI-C |
| **CICD-6 Security scanning** | 011 herramientas/policy; 012 controles fuente/deps/secretos y contrato de scan imagen/IaC | CICD-1/2; decisiones tooling/licencia | Controles/configuración aprobados; todas las ejecuciones requeridas/negativos acreditados en 029 | GATE-CI-D |
| **CICD-7 Container build** | 013 build imagen; 014 manifiesto/integridad/procedencia | CICD-2–6 | Mismo SHA/config, scan de imagen e integridad; no secretos ni latest ni falsa equivalencia de reruns | GATE-CI-E |
| **CICD-8 ECR publishing** | 015 contrato registry/publicación; 016 push confiable y verificación digest | CICD-7 y 9; ECR/roles de 003 bajo permiso propio | Solo main confiable publica, PR no; identidad ECR coincide con build/scan. No deployment ni creación ECR | GATE-CI-E |
| **CICD-9 OIDC AWS** | 017 trust/roles por contrato; 018 permisos y prueba acceso/denegación | CICD-1 y contrato 015; provisión IAM de 003 independiente | Contexto/aud/sub reales verificados, separación roles y PR denegado; no claves largas ni runtime secrets en GitHub | GATE-CD-A |
| **CICD-10 Terraform validation** | 019 fmt/validate/IaC condicional; 020 plan/apply/ownership | CICD-1/6/8/9 para contratos, sin exigir publicación para diseñar | Ausencia de infra explícita; si existe, validar; plan protegido, apply manual y versión única sin drift acordados con 003 | GATE-CI-D, GATE-CD-B |
| **CICD-11 Manual deployment workflow** | 021 preflight de candidato; 022 despliegue EC2 manual | CICD-1–10 para capacidades necesarias; 003 recursos/readiness, no su aceptación final | workflow_dispatch/ref main/production y checks sin bypass. Operación de despliegue real solo con permiso posterior y smoke/rollback listos | GATE-CD-B |
| **CICD-12 Smoke tests** | 023 HTTPS/health/Auth sintético; 024 cross-instance | CICD-11; 003 capacidad operativa/superficie autorizada | Pruebas diseñadas seguras; éxito público y estado distribuido solo con ejecución real en 030, sin inferirlo de Logout repetido | GATE-CD-C |
| **CICD-13 Rollback/concurrency** | 025 rollback/fallos; 026 exclusión de mutaciones | CICD-10/11/12 para contratos | Digest previo/manualidad, reconciliación y lock compartido probados en entorno autorizado; no rollback de seguridad | GATE-CD-B, GATE-CD-D |
| **CICD-14 Observability/evidence** | 027 logs/manifest/retención; 028 evidencia académica/costo | Resultados/contratos CICD-2–13 | Índice/política saneados y verificables; completar evidencia real en 029/030, sin datos personales/secretos | GATE-CD-D |
| **CICD-15 Final validation** | 029 matriz CI/security/negativos; 030 prueba manual, smoke/rollback y cierre | Capacidades 1–14 autorizadas; 003 operativa y permisos específicos | Gates solo con evidencia real y revisión; sin deployment por evento automático, aceptación limitada a primera release Authentication | Todos |

T-CICD-001–028 implementarán/revisarán contratos y capacidades **solo cuando se autoricen**; algunas pruebas necesitan servicios/runner y permiso propio. T-CICD-029 valida el CI completo/negativos y T-CICD-030 la entrega manual/cloud. Todas están sin ejecutar ahora. No se requiere cerrar GATE-CD-C antes de poder realizar la prueba que lo acredita.

## 4. Modelo de CI propuesto

### 4.1. Eventos, SHA y confianza

CI `ci.yml` es solo nombre conceptual futuro, no archivo creado. PR main comprueba integración; push main comprueba commit integrado y genera candidato elegible. PR head/merge SHA y main SHA registrados por separado. No usar resultado local ni PR de SHA distinto para publicar/desplegar.

Flujo: checkout exacto → global.json/SDK → restore locked → build no-restore → UT → IT → ST → controles fuente/deps/secretos/IaC aplicables → build imagen → scan/integridad → publisher main → agregador **CI GREEN final**. Todos los pasos obligatorios acreditados; security no se da por verde por publicar reportes.

CI PR aislado, sin OIDC AWS/production/secretos; puede construir/analizar imagen pero no publicar ECR productivo. Publisher confiable es job separado de tests/código no confiable, con OIDC/ECR mínimo y artefactos verificados del mismo run/SHA. Ningún workflow_run privilegiado que consuma salida de PR para desplegar o publicar sin cadena de custodia. No `continue-on-error` ni resumen verde si un job fue cancelado/skipped.

### 4.2. SDK, build, tests y timeouts

Respetar SDK 10.0.403/rollForward disable y lockfiles existentes. Configuración Release se propone para artefacto futuro, pero aprobación de configuración/build/tests y warnings efectivos es DP-04. No agregar TreatWarningsAsErrors o suppressions arbitrarios; registrar warnings y política de security por separado.

Primera implementación recomendada secuencial para simplificar identidad de binarios y evitar presión de fixtures: UT→IT→ST. Si se distribuye en jobs, transferir/verificar build o reconstruir explícitamente mismo SHA/SDK/config; no asumir que checkout incluye assemblies. Reusar caché solo como optimización con claves/validación, nunca evidencia de test.

Runner Linux efímero, Docker accesible y recursos medidos; Mongo replica set, Redis y DynamoDB Local/Ryuk según fixture existente. Inicialización/readiness y cleanup de fixtures, namespace por run, timeouts de job aprobados y salida nativa preservada antes de cleanup. No modificar los límites SUT por problemas del runner ni desactivar tests. Cero skipped bajo contrato vigente; conteos dinámicos y no cero, results completos por proyecto.

### 4.3. Controles security y artefactos

Herramientas/versiones/costo/policy antes de implementar. Selección mínima justificada para dependencias, secretos, source, imagen e IaC cuando exista; no imponer marcas/licencias sin aprobación. Fallos del scanner/feed/control, resultados desconocidos o findings bloqueantes niegan elegibilidad. Excepciones solo formales con alcance/responsable/vencimiento y visibles, sin omitir gate ni alterar tests.

Acciones críticas fijadas por SHA aprobado y actualización revisada; imágenes base/fixtures identificables. Build de imagen después de checks de código, no después del green final que también la exige. Manifiesto enlaza SHA/run/intento, toolchain/base, resultados/hash y digest; verificar digest publicado ECR. Un rerun no sustituye candidato automáticamente; la misma etiqueta SHA no se reescribe ante diferente digest.

## 5. Deployment manual y contrato con 003

### 5.1. Procedimiento humano y elegibilidad

`deploy.yml`/nombre Deploy son propuestas conceptuales. Workflow en default branch real, ejecutado solo desde revisión confiable main. Persona autorizada: Actions → Deploy → Run workflow → SHA/run/motivo → confirmación manual. Production fijo. Ningún evento push/PR/merge/release/cron/workflow_run ni bot dispatch inicia producción.

Antes de mutar: confirmar actor/repo/ref; SHA alcanzable main; run/intento de CI confiable para ese SHA; cada gate obligatorio verde según policy vigente; digest ECR esperado y no sustituido. Si falta prueba, se rechaza. No usar "último run success", input de URL/ARN libre, nombre de check no confiable o etiqueta latest. Obtener lectura AWS temporal si verificar ECR lo exige, antes de rol mutador. Bajo exclusión, revalidar y congelar digest/destino.

Production usa historial/environment y protecciones disponibles. Segunda aprobación es opcional según plan, no condición académica obligatoria: workflow_dispatch ya es acción humana. No afirmar que UI/API originan eventos distinguibles por sí solos; limitar actores/tokens, no automatizar llamadas dispatch y auditar manualidad. Si no existe environment/control requerido en el plan, resolver alternativa explícita o bloquear, no simularlo.

### 5.2. OIDC y permissions

003 proveerá IAM/roles/ECR/red/destino; 004 especifica necesidad/consumo. OIDC aud/sub exactos según repo/subject real; environment sub no garantiza rama por sí solo. Publisher main/ECR distinto de deploy/SSM, provisión/apply y runtime/bootstrap. No wildcard amplio ni claves duraderas, no credenciales de producción en PR.

GITHUB_TOKEN `contents: read`; `actions: read` solo para consultas CI; `id-token: write` solo en jobs OIDC. `packages` no requiere write por ECR. Historial vía API/attestations/otras funciones necesitan permiso adicional solo si se adoptan y justifican. Runtime obtiene secretos en AWS; pipeline no imprime/lee innecesariamente JWT keys, Atlas, Redis o HMAC. Prueba sintética sensible permanece efímera.

### 5.3. Inputs/outputs consumidos

| De 003 hacia 004 | Uso / restricciones |
| --- | --- |
| Cuenta/región/environment y roles aprobados | Destino fijo/validado, no entrada arbitraria que amplíe trust |
| ECR URI y repositorio | Publicar/leer digest en registry existente; no crearlo desde pipeline |
| Instancias/SG/target group/ALB | Operación EC2 y comprobación dos targets, sin SSH público ni bypass |
| URL HTTPS, TLS/DNS, liveness/readiness aprobados | Smoke y estabilidad; readiness no resuelta es bloqueo de aceptación |
| Referencias configuración/secretos/authority | Conservar semántica, runtime no se abastece de GitHub Secrets |
| Logs/métricas/alarma | Atribución interna A/B, errores y evidencia, sin PII ni JWT |
| Backend state/locking y plan aprobado si aplica | Solo tramo manual autorizado, sin state local efímero en runner |

004 devuelve a operación/003: commit/digest elegido, run/intento/policy, actor/hora, versión activa/previa, outcome/targets/health/smoke/rollback y referencia de evidencia saneada. No devuelve secretos ni redefine arquitectura.

### 5.4. Ownership Terraform/deploy y bootstrap

Terraform administra estructura; workflow manual es único escritor de versión activa. Deben acordarse con 003 almacenamiento persistente no secreto del digest activo, inicialización, reemplazo EC2, reconcile y rollback. No resolver con dos tfvars writers o ignore_changes sin protocolo real. Prueba futura: aplicar cambio estructural/reemplazar host no impone digest anterior. Ubicación/mecanismo concreta DP-08; 003 no se modifica aquí ni DP-I-14 se declara cerrada.

ECR/roles/state deben estar disponibles antes de primera publicación; 003 aún no los tiene implementados. Solicitar bootstrap acotado a 003 y revisar la secuencia de su autorización para no exigir imagen ECR antes de crear el registry. La preparación de 004 no exige aceptación final cloud de 003. La operación compartida 003/004 aportará la misma evidencia a los respectivos gates, sin esperar que ambos estén cerrados antes de empezar sus pruebas. Si se requiere ajuste de 003, pedirlo; no ejecutar ni editar por esta entrega (DP-12).

### 5.5. Aplicación, humo y rollback

Desplegar digest exacto en EC2 por mecanismo acordado (SSM candidato); no comando arbitrario del input. Esperar estabilidad acotada, mínimo dos instancias/AZ/digest y dos targets saludables; verificar vía HTTPS y telemetría. No exponer identidad de instancia por nuevo endpoint/cabecera pública. No prometer que ALB bloquea todo si todos unhealthy; fail-closed permanece responsabilidad de aplicación/authority.

Smoke básico: TLS válido sin bypass, `/health`, readiness aprobada, Register 201/Login 200/Logout 204 con identidad sintética institucional, no-store y errores seguros. Controlar cuotas/origen, creación/retención/limpieza de cuenta sin borrar datos con permisos de deploy. Tokens solo en memoria y nunca artifacts/logs. No retry ciego de POST.

Prueba distribuida: solicitudes atribuidas a A/B, token en política protegida normal, revocación cruzada y cuota compartida. Logout repetido tiene excepción y no prueba rechazo; falta de superficie protegida autorizada deja bloqueada aceptación de GATE-CD-C/GATE-I-I, aunque smoke básico pase. Resolver por incremento separado, no agregando Orders o `/me` para mostrar la rúbrica.

Rollback solo otro Run workflow autorizado, digest anterior conocido y revisión compatible/vigente, sin revertir Redis/authority/JWT por defecto. Fallo/API incierta/unhealthy/smoke rojo no es éxito ni trigger automático de rollback; reconciliar y esperar operación humana. Grupo production-deploy común con cancel-in-progress false, coordinación apply/maintenance y revalidación al obtener turno; no prometer FIFO.

## 6. Estrategia de validación y gates

Definición canónica de gates 004 en [task.md §3](task.md#3-gates-formales), todos pendientes:

- GATE-CI-A/B/C/D/E: estructura, restore/build, suites, security e imagen/ECR.
- GATE-CD-A/B/C/D: OIDC/permisos, manualidad protegida, smoke público/distribuido y rollback/evidencia.

Validación negativa futura debe comprobar: PR no recibe credenciales/publicación; SHA no main o CI ajeno/rojo/cancelado/skip se rechaza; digest/tag sustituido se rechaza; falta scanner/resultado o workflow errado no produce verde; automático nunca deploy/apply; colisiones no cancelan mutación; target/API/dependencia/smoke fallidos no declaran éxito. Sin modificar producto/tests para simular verde; probes de workflow y fallos solo con autorización de objetivo/entorno.

T-CICD-029 acredita CI completo/negativos; T-CICD-030 acredita operación manual real, smoke, fallos/rollback y evidencia. AC-CICD-01–16 enlazan requisitos/gates. Las tareas de configuración previa no cierran gates cloud por sí solas. Retener fallos y nativas; no reruns ocultos ni fakes como demostración cloud. GATE-G/H de 002 y AUTH-027 conservan su proceso propio.

## 7. Riesgos, costos y autorización posterior

DP-CICD-01–14 de spec siguen abiertos. Antes de implementar especificar archivos, comandos, entorno, permisos, secretos/referencias, costo, reversión y evidencia. Proteger GitHub/AWS/SSM, state/planes y artifacts; calibrar runner antes de cambiar presupuestos. Validar plan/visibilidad/licencias/retención; Actions/ECR/transferencia/scanners pueden cobrar. No resolver carencias de plan con trust más amplio o segunda aprobación obligatoria no soportada.

No se instalan herramientas ni seleccionan versiones adicionales en esta entrega. No se crea CI/CD, resources, infra, ECR, pruebas u Orders. El resultado actual son tres documentos; detenerse sin stage/commit/push.
