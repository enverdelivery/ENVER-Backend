# Entrega runtime DEPLOY-CONFIG-03

**Estado vigente: DEPLOY-CONFIG-03 CLOSED por retest real único aceptado; API no iniciada.**
El historial de bloqueos y correcciones se conserva debajo; evidencia de cierre en
`memoria.md`. Posterior [START CONTRACT FIX — source y pruebas](deploy/START-CONTRACT.md):
source listo para gate separado de nueva imagen, sin construirla ni desplegarla.
Plantillas locales ahora con logging saneado/journal y preservación del runtime;
no se reemplazan las unidades o wrappers preparados en A. No arrancar Enver.API.

## Corrección local config-parse-validation (2026-11-18 UTC)

El defecto reproducido **no es una lectura precommit del runtime final**. La primera
validación (`Program.MapAndValidate` → `BindingDiagnostic.InMemory`) serializa el JSON
generado y usa `AddJsonStream`, además de dos defaults obligatorios del paquete.
La segunda (`AtomicRuntimeFile.Write` → `LinuxAttempt.ValidateConfiguration` →
`BindingDiagnostic.FileBinding`) se ejecuta después de cerrar/flush del candidato
`/run/enver/.runtime-<GUID>.tmp`, antes de publicar el final.

**Causa a nivel de código:** antes del fix, `FileBinding` usaba
`RuntimeConfiguration.AddRequiredFile(Defaults(), temp).Build()` también para ese candidato.
`AddJsonFile` resuelve un `PhysicalFileProvider` con filtros predeterminados que excluyen
nombres iniciados por punto. Por tanto, el archivo temporal existe físicamente, pero el
provider lo presenta como inexistente y el JSON obligatorio lanza `FileNotFoundException`.
La regresión local reproduce la cadena anterior real, contrasta `File.Exists=true` con
`GetFileInfo.Exists=false` y comprueba visibilidad con `ExclusionFilters.None`.
No depende de secretos, AWS ni de un archivo final existente.

**Fix:** solo el binding del temporal abre explícitamente el candidato mediante
`File.OpenRead(temp)` → `Defaults().AddJsonStream(stream).Build()` → validadores existentes.
El archivo sigue siendo obligatorio: no hay fallback, validación omitida, cambio de nombre,
permisos ni relajación de filtros globales. Los defaults, mappings, pins y contratos se
conservan. La memoria sigue validándose sin runtime físico; el rename sigue siendo la
última E/S productiva, con todos los controles precommit conservados.

Fases distinguibles: `config-parse-validation` / `config-binding-validation` (memory),
`temp-config-parse-validation` / `temp-config-binding-validation` (temporary-file),
`final-config-parse-validation` / `final-config-binding-validation` (final-file, únicamente
validación explícita separada). El fallo añade **`validation-target`** a los seis campos
saneados anteriores; sus valores son `memory`, `temporary-file`, `final-file` o
`not-applicable`. Códigos/tipos/inner y prohibiciones de payload/Message/stack permanecen.
Un supervisor futuro debe autorizar/entender este contrato de siete campos antes de usarlo;
los supervisores históricos no se reejecutan ni modifican en este gate.

**Corrección de la inferencia histórica:** la fase compartida del diagnóstico remoto
`ae717ce6-381c-4bc8-9aa2-5729ff747eff` no distinguía primera/segunda validación. Por eso
las afirmaciones anteriores «temp/write no alcanzados» y «parse en memoria falló» no
estaban acreditadas por ese recibo. El defecto del temporal está reproducido localmente;
no se dispone del nombre GUID histórico ni se realizó una nueva inspección/ejecución remota.
La ausencia final y de nuevos temporales residuales sí procedía del postcheck del supervisor.

Las regresiones usan exclusivamente payloads sintéticos, fixtures propios bajo el temporal
local aprobado y el binding real; no acreditan chmod/chown/fsync/rename Linux en A.
No añaden validación fallible postcommit al flujo productivo. El test opcional del final
valida solo después del rename de su fixture. No se modifica `global.json`: el responsable
autoriza explícitamente SDK instalado **10.0.404** para este gate, pues el pin **10.0.403**
con rollForward deshabilitado no está instalado. Sin restore/instalación ni cambio de paquetes.

**Resultado local:** Release **PASS, 0 warnings / 0 errores**; suite completa **PASS**:
47/47 aserciones históricas, 70/70 escenarios de observabilidad (1005/1005 aserciones)
y 13/13 regresiones config-parse (78/78 aserciones), **1130/1130 aserciones en total**.
Las regresiones incluyen memoria/final ausente, reproducción provider/dot-prefix,
temporal correcto/final ausente, final explícito tras rename, cero lecturas solicitadas
del final antes del rename en el adaptador de prueba, defaults obligatorios faltantes,
cuatro nuevas fases con target seguro, binding inválido fail-closed, JSON inválido y
flujo completo con dependencias sintéticas. Runtime local 10.0.15; no es una ejecución
del artefacto remoto Linux 10.0.14. No se ejecutan más verificaciones técnicas tras este gate.

Build local autorizado, sin restore:
`dotnet "C:/Program Files/dotnet/sdk/10.0.404/MSBuild.dll" tools/Enver.RuntimeConfig.Materializer.Tests/Enver.RuntimeConfig.Materializer.Tests.csproj -target:Build -property:Configuration=Release -property:RestoreLockedMode=true`.
Suite ejecutada desde su DLL de pruebas Release, capturando salida y suprimiendo detalles
no allowlisted en el runner local; salida final sin stderr ni valores sintéticos filtrados.

**DEPLOY-CONFIG-03 OPEN / DEPLOY-CONFIG-04 CLOSED**. No publish, transfer, AWS,
materializer real, escrituras `/run/enver`, API, IAM, Terraform, initialize, commit o push.
El artefacto remoto conserva su versión anterior; cualquier publicación/ejecución exige
autorización separada. Los apartados de evidencia anteriores se conservan como historial.

## Alcance

Helper .NET 10 independiente. No ejecuta el entry point de Enver.API, servicios hospedados,
Bootstrap, inicialización de cuotas ni operaciones Redis/Dynamo/MongoDB. La referencia a API
reutiliza exclusivamente opciones, validadores y configuración. AWS SDK SecretsManager
4.0.100.18 es dependencia exclusiva del helper; Core 4.0.102.18 coincide con API. No se cambian
dependencias/lockfile de API. Los lockfiles del helper y pruebas son propios.

- `--materialize`: exige Linux x64/root, tmpfs sin swap, aislamiento de credenciales e IMDSv2
  de A con role `enver-dev-api-runtime`. Usa `InstanceProfileAWSCredentials` explícito;
  no admite fallback a profile, credenciales estáticas, ECS o web identity.
- Las cuatro referencias ARN+VersionId están fijadas en `PinnedSecrets.cs`. Se verifica
  también ARN/VersionId de respuesta. No usa AWSCURRENT como selector ni reintenta requests.
- Schemas exactos observados en memoria: Redis `{username,password}`, MongoDB
  `{username,password,host,database}`, HMAC `{keyBase64}`, JWT
  `{activeKid,keys:[{kid,keyBase64}]}`. No admite campos extra/duplicados ni aliases.
- Redis usa endpoint contractual v2,6379,TLS,SNI,DB0 y AllowAdmin=false; el roundtrip del
  parser debe conservar credenciales/settings. MongoDB usa host Atlas validado, credenciales
  escapadas y database `enver_development`; no resuelve DNS ni conecta durante binding.
- HMAC/JWT se validan con las opciones reales: Base64 canónica, ring completo, kid único y
  activo existente, sin reutilización HMAC/JWT. Ningún valor, longitud ni hash se reporta.
- Conserva defaults no secretos de API y configura tabla/timeout2000,C/I/G/capacidad5000.
  El timeout procede del contrato Bootstrap ya existente y satisface el validador API.

## Archivo y providers

Solo `/run/enver/appsettings.Runtime.json`. Directorio root:root0711; archivo1654:1654,
0600, accesible al UID del contenedor únicamente mediante bind mount read-only. Temp
CreateNew/0600 en el mismo tmpfs, escritura/Flush(true), binding del temporal, chown,
validación de permisos y fsync del directorio **antes** del rename atómico final.
No hay fallback persistente ni promesa de durabilidad en disco. No se borra el JSON
histórico al preparar/fallar un intento, ni se barren otros temporales. En caso de fallo
solo se elimina el temporal CreateNew de ese intento. La publicación es la última E/S;
no hay validaciones fallibles posteriores que obliguen a borrar un final ya publicado.
Aunque se conserve un histórico, un exit no cero del helper impide ExecStart; nunca se
utiliza ese histórico como fallback ni como autorización para arrancar API.

`Program.cs` añade el provider compartido de `RuntimeConfiguration` **después** de los
providers de WebApplication.CreateBuilder: appsettings, Development, user secrets si aplica,
env y CLI permanecen, pero no sobrescriben el runtime final. En contenedor es obligatorio;
reloadOnChange=false. El helper genera solo seis secciones allowlisted, no providers
genéricos desde el payload. Fuera de Docker se conserva la configuración local existente.
No se construyó ni arrancó una imagen operacional API en esta entrega.

`--validate-runtime` reutiliza ese mismo provider y resuelve únicamente IOptions de Redis,
MongoDB, RateLimiting, Jwt, AuthenticationSecurityAuthority, AuthenticationComposition,
Authentication y AuthenticationTransport. No construye/inicia un host ni resuelve clientes.

## Preparación systemd (NO instalada en A)

`deploy/enver-api.service`, `materialize.sh` y `start-api.sh` son plantillas preparadas,
no habilitadas ni ejecutadas. El supervisor instala la unidad solo después de validar la
entrega real, que falló antes de alcanzar esa etapa.

Flujo futuro: autorización externa ausente por defecto + imagen por digest aprobada en
archivo **no secreto**, ExecStartPre materializer, después Docker. Exit no cero impide
ExecStart; Restart=no. Cada restart explícito vuelve a materializar los VersionId fijados.
El bind del archivo se establece después del rename. La unidad no publica puertos.
No crear `/etc/enver/api-start.authorized`, no fijar imagen ni iniciar servicio en este gate.

Stop: detener/retirar contenedor antes de borrar JSON; ExecStopPost elimina archivo y
RuntimeDirectoryPreserve=no retira el directorio. Reboot descarta tmpfs. No copiar JSON a
disco, repositorio, imágenes, variables, argv o logs. No garantizar zeroización de strings
administrados: permanecen solo en RAM hasta su recolección/salida. Core dumps/diagnósticos
están deshabilitados en lanzadores; el helper rechaza hosts con swap habilitado.

## Evidencia y bloqueo (2026-11-18 UTC)

- Build Release PASS,0warnings/errors; **47 aserciones fake-only PASS**, sin AWS/red/API.
  Incluye schemas, ring/HMAC, pinning request/response, precedencia y errores/logs canario
  sin payload ni inner exceptions. La CLI solo admite dos switches fijos, no secretos.
- Publicación Linux self-contained PASS. Artefacto no secreto transferido por SSM y
  verificado en A: `/opt/enver/runtime-config/7d479a7a383d6246`.
- Primera ejecución real `--materialize`: fallo saneado. No hay recibo runtime READY.
  Causa exacta **NO ESTABLECIDA**; no atribuir a IAM/schema/credenciales sin evidencia.
  No se ejecutaron la segunda materialización, diagnóstico de mount/binding ni instalación
  systemd. No hay autorización para reintentar automáticamente.
- SSM de entrega: `34abf371-5647-4e53-ad82-b88195465e00`. Recibo saneado externo:
  `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-runtime03-delivery-2026-11-18T08-18-10-316Z\result.json`.
- **DEPLOY-CONFIG-03 OPEN / NEXT GATE BLOCKED**. DEPLOY-CONFIG-04 CLOSED conservado.
  API NO iniciada; sin escrituras Redis/Dynamo, initialize adicional, Terraform, IAM,
  EC2 B, ALB, HTTPS, commit o push. Initialize histórico1/G APPLIED son baseline aceptado,
  no una nueva lectura o inicialización en esta entrega.

Se requiere continuación autorizada para diagnosticar el fallo con códigos/fases seguros,
sin payloads, y completar los gates restantes. No volver a despachar supervisores históricos
de transferencia/entrega: tienen marcadores de una sola ejecución y checks anti-overwrite.

## Fix de observabilidad local (posterior; pendiente de aprobación/publicación)

La causa del fallo real anterior sigue **NO ESTABLECIDA**. Este gate añade observabilidad
y tests; **no ejecuta el materializador real**, AWS, Secrets Manager, Docker ni `/run/enver`.
El artefacto remoto anterior no fue actualizado. El supervisor histórico de delivery sigue
siendo un procedimiento operacional, no un runner de diagnóstico: no reutilizarlo para
diagnosticar, pues descarta detalles del hijo e incluye etapas ajenas al gate actual.
Un futuro diagnóstico requiere un nuevo alcance/script aprobado que conserve solo los
seis campos saneados del helper, sin repetir automáticamente la entrega operacional.

### Orden inspeccionado antes del fix

Main → Prepare (Linux/root/swap/tmpfs/directorio, borrado de histórico y barrido de temps)
→ identidad IMDSv2/creación cliente (credenciales SDK diferidas)
→ GetSecretValue Redis, MongoDB, HMAC, JWT
→ parse/map Redis, MongoDB, HMAC, JWT → config no secreta/JSON
→ binding en memoria → temp CreateNew0600 → write/flush/fsync → binding del temp
→ chown → rename → fsync directorio → stat/permisos del final.
Los catches descartaban la causa, y el cleanup podía borrar el final incluso si era histórico.

### Orden instrumentado actual

`arguments` → `tmpfs-check` → `aws-credentials` (identidad y resolución SDK explícita)
→ `redis-secret-read` → `mongodb-secret-read` → `hmac-secret-read` → `jwt-secret-read`
→ `redis-parse` → `redis-map` → `mongodb-parse` → `mongodb-map`
→ `hmac-parse` → `hmac-map` → `jwt-parse` → `jwt-map`
→ `nonsecret-config` → `json-build`
→ `config-parse-validation` → `config-binding-validation` (memoria)
→ `temp-file-create` → `file-write` → `file-flush`
→ `config-parse-validation` → `config-binding-validation` (temporal)
→ `file-permissions` → `final-file-check` (candidato completo, antes de publicar)
→ `directory-flush` → `atomic-rename` (commit).
`temp-cleanup` identifica exclusivamente un error al cerrar/eliminar el temporal propio.
Los hooks de fallos son internos, inyectados en tests; no existen flags/env de producción
para saltarse controles, suministrar secretos o elegir dependencias fake.

### Contrato del fallo

Exit1 significa **MATERIALIZATION = FAILED**, sin retry/fallback/startup. Exit2 es uso inválido.
El catch exterior emite solamente `phase`, `exception`, `inner`, `cancelled`, `category`,
`code`. Las etiquetas y los tipos/códigos son allowlists; `inner` es una lista acotada de
tipos permitidos, no excepciones serializadas. AWS ErrorCode conocido se conserva; código
desconocido se convierte en `aws-error-unclassified`. No se publican Message, ToString,
StackTrace, RequestId, Data, valores, versiones de payload ni nombres de tipos desconocidos.
Las excepciones intermedias retienen solo esa instantánea segura, no el objeto original
ni un InnerException con contenido. Se conservan los cuatro ARN/VersionId y MaxErrorRetry=0.

**Cambio funcional delimitado: SÍ**, para cumplir el cleanup autorizado y validar antes
del commit; la resolución explícita permite atribuir fallos de credenciales a su fase.
Sin cambios en mappings, validadores, secretos/pins, C/I/G, API o infraestructura en este fix.

Build Release **PASS**,0warnings/errors, sin restore/publicación. Suite completa **PASS**:
**47/47 aserciones históricas + 70/70 escenarios nuevos (1005/1005 aserciones nuevas)**.
Se conservan las 47 aserciones anteriores; se usa el mismo runner con dependencias
inyectadas en RAM, sin invocar Main productivo. Incluye fallo por cada fase con/sin histórico,
JSON inválido real de los cuatro schemas, AWS simulado, pin mismatch, cancelación, tipos/códigos
no admitidos, inner acotado, write parcial, permisos, rename, validación final y cleanup.
Se comprueban stdout/stderr sin canarios, ausencia de final parcial, preservación de histórico
y temporal ajeno, cero commits en fallo y operaciones sin retries. El éxito simulado comprueba
el orden precommit y un único JSON completo. Las pruebas no acreditan el adaptador Linux en A.

**NEXT GATE = OBSERVABILITY BUILD/PUBLISH AUTHORIZATION**; DEPLOY-CONFIG-03 sigue OPEN,
DEPLOY-CONFIG-04 CLOSED, initialize histórico1/G APPLIED y API NO iniciada.
