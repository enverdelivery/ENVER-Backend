# EC2 A START CONTRACT FIX — source y pruebas (2026-11-18 UTC)

## Actualización vigente — LOCAL HEALTH CONTRACT FIX

**SOURCE READY FOR NEW IMAGE BUILD = YES. NEW API IMAGE REQUIRED = YES.**
Esta entrega cambia `AuthenticationTransport` y prepara únicamente el futuro
binding Docker; no construye/publica/descarga imágenes ni inicia la API.
La evidencia anterior de este documento permanece como historial, salvo los
ajustes de health/exposición que se describen aquí.

- Opciones exactas: `AuthenticationTransportOptions`; middleware de composición:
  `AuthenticationTransport.UseAuthenticationTransport`. `AllowLocalHttp` continúa
  **false**. No cambian opciones, appsettings, secretos ni autorización de origen.
- Orden: captura/validación de forwarding → contexto de confianza de origen →
  HSTS donde ya aplicaba → redirección HTTPS condicional → routing/autenticación/
  autorización/endpoints del pipeline existente. `Program.cs` no cambia.
- `UseWhen` excluye de la redirección **solo GET con Path ordinal `/health` y
  PathBase vacío**. Otros métodos, `/health/`, `/health/*`, mayúsculas y prefijos
  conservan HTTPS. Una query no cambia el Path. No hay excepción global por
  Development, localhost o `/api`. HTTPS continúa por el mismo pipeline.
- Se conserva el único `MapHealthChecks("/health").AllowAnonymous()` y su respuesta
  existente. **LIVENESS**, sin checks Mongo/Redis/Dynamo. **READINESS ABSENT**.
- Plantilla local y wrapper preparado en A incluyen exactamente un
  `--publish 127.0.0.1:8080:8080`, no un binding wildcard. **HOST EXPOSURE LOOPBACK
  ONLY**; validación estática y de sintaxis, no prueba de un socket ejecutado.
  Se conservan UID/GID1654:1654, runtimeRO, journald, stdout/stderrjournal y la
  variable de imagen digest-pinned. Unidad, materializer y runtime no se modifican.
- **TEMPORARY PRE-ALB CONTRACT**. Un ALB externo al host no puede alcanzar este
  puerto publicado exclusivamente en loopback. Su futuro acceso a otra interfaz,
  controles de red y health checks requieren un cambio explícito posterior; no se
  abre ahora ninguna interfaz ni se implementan ALB/HTTPS.
- `/etc/enver/api-start.authorized` sigue **ausente**. Servicio inactive/disabled,
  contenedor ausente, pull0/start0/daemon-reload0/materializer0.

**Imagen anterior:**
`sha256:e8332c6ab79f28c262737dd2015ef94ead7aa63f3d4eb913641aac8dbfeadbc9`
queda **SUPERSEDED FOR DEPLOYMENT** porque no contiene la excepción binaria.
No se borra, retaggea ni publica otra imagen. `api-image.env` conserva esa
referencia histórica; debe actualizarse únicamente a un nuevo digest aprobado en
un gate posterior, no arrancar con el anterior. Preparar el binding no valida esa
imagen obsoleta ni autoriza startup.

Release API y UnitTests con SDK10.0.404 explícito, sin restore: **0 warnings,
0 errores; 753/753 tests PASS**, incluidos25nuevos de transporte/liveness/binding.
Pipeline ASP.NET real en memoria, sin host/API iniciado, sockets ni dependencias
reales. Las rutas HTTPS usan un terminal sintético, no operaciones de negocio.
`global.json`, los seis lockfiles, Dockerfile y proyecto API permanecen intactos.
Evidencia local y SSM de preparación en
`C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-local-health-source-2026-11-18T15-43-33-998Z\`.

**NEXT GATE: NEW API IMAGE BUILD + LOCAL VALIDATION**, requiere autorización
separada. No Docker build, ECR push/pull, daemon-reload/start, marcador, Mongo real,
Redis real, Dynamo writes, initialize, Terraform/IAM/ALB/HTTPS ni commit/push.

## Evidencia histórica — contrato de startup anterior

**SOURCE READY FOR IMAGE BUILD = YES. NEW IMAGE REQUIRED = YES.** Solo preparación
local autorizada; ninguna imagen construida/publicada, API iniciada ni plantilla
remota reemplazada. No equivale a aceptación de una nueva imagen o del startup real.

## Configuración runtime

`Enver.API/Program.cs` invoca `AddEnverRuntimeConfiguration` antes del registro de
servicios. En contenedor exige `/run/enver/appsettings.Runtime.json`, absoluto,
`Optional=false`, `ReloadOnChange=false`. El provider deriva del provider JSON de
.NET y conserva ese contrato de archivo obligatorio. Fuera de contenedor se conserva
el flujo local anterior cuando no existe el archivo runtime.

No basta con poner JSON al final: .NET fusiona claves y listas de providers
anteriores. El provider específico ahora exige en **su propio archivo** los campos
entregados por el materializador de las seis secciones runtime: Redis, MongoDb, Jwt,
RateLimiting, AuthenticationSecurityAuthority y AuthenticationComposition. Comprueba
campos sensibles no vacíos y cada par Kid/KeyBase64 de un key ring no vacío y contiguo.
No exige que el runtime repita los defaults no sensibles de política.

Bloquea el fallback de Redis:ConnectionString, MongoDb:ConnectionString,
RateLimiting:PseudonymizationKeyBase64, Jwt:ActiveKid y el subárbol Jwt:Keys, incluido
el tail de una lista JWT previa. Los defaults no sensibles siguen combinándose.
Los validadores reales `IStartupValidator` se ejecutan explícitamente después de
`Build`, antes de `RunAsync` y de resolver los servicios hospedados/clientes.
Archivo ausente/malformed, secciones/campos ausentes, opciones JWT/rate inválidas y
fallos de binding abortan con salida no cero; no se abre un listener como fallback.

### Orden de providers

Orden exacto observado en la prueba local con SDK10.0.404/runtime10.0.15, Development,
ApplicationName=Enver.API.UnitTests y argumentos sintéticos; no inicia Host/API:

1. MemoryConfigurationSource (bootstrap).
2. EnvironmentVariablesConfigurationSource, prefijo ASPNETCORE_.
3. EnvironmentVariablesConfigurationSource, prefijo DOTNET_.
4. CommandLineConfigurationSource (hosting).
5. MemoryConfigurationSource (defaults de hosting).
6. JsonConfigurationSource, appsettings.json.
7. JsonConfigurationSource, appsettings.Development.json.
8. JsonConfigurationSource, Enver.API.UnitTests.settings.json.
9. JsonConfigurationSource, Enver.API.UnitTests.settings.Development.json.
10. EnvironmentVariablesConfigurationSource, sin prefijo.
11. CommandLineConfigurationSource (aplicación).
12. ChainedConfigurationSource.
13. RuntimeJsonSource, appsettings.Runtime.json, **último**.

Los settings por ApplicationName son providers adicionales observados en este
entorno de pruebas; el nombre productivo es Enver.API. En Development, el assembly
Enver.API tiene UserSecretsId: si los defaults de hosting incorporan User Secrets,
esa fuente también precede a environment/CLI y runtime. No se leyó un almacén local
real de User Secrets para generar este registro. La enumeración anterior es evidencia
del proceso de prueba, **no una enumeración ejecutada en la futura imagen**.
El orden productivo relativo confirmado por source es: todos los defaults de
CreateBuilder → RuntimeJsonSource → registro/validación/opciones → RunAsync, sin
providers posteriores añadidos por Program.cs. Ni Development ni env/CLI pueden
sobrescribir después los secretos runtime. El inventario exacto del runtime10.0.14
de la futura imagen deberá confirmarse en su gate, sin volcar valores.

## Observabilidad sin secretos

`SafeStartup.Run` contiene toda la creación/configuración/construcción/ejecución del
host y devuelve exit1 con `event=startup status=failed phase=<enum allowlisted>`.
No imprime excepciones, Message, stack, inner, objetos de configuración ni nombres
de tipos arbitrarios. Las fases distinguen builder/runtime-config/services/host-build/host-run.

Se eliminan los providers de logging predeterminados y se instala únicamente
`SafeConsoleLoggerProvider`. No invoca formatters ni serializa state/scopes/exceptions.
Emite categorías finitas y severidad; reconoce lifecycle started/listening/stopping
sin direcciones, rutas ni valores. Conserva resultados auth mediante allowlist y
añade categoría mongodb-initialization con ready/incompatible/unavailable. Mensajes
del framework/SDK desconocidos se proyectan a categoría genérica, no se reproducen.
Se sacrifica detalle crudo deliberadamente; esto no son trazas completas ni un dump
del fallo. No se añaden sinks externos, CloudWatch, request logging ni nueva persistencia.

Plantillas futuras: Docker **journald**, proceso adjunto stdout/stderr; unidad
`StandardOutput=journal`, `StandardError=journal`, para `journalctl -u enver-api.service`.
Journald Docker también identifica el contenedor en su propio metadata. Puede haber
duplicación del evento adjunto; no contiene payload. `Restart=no`, ExecStartPre sin
prefijo de ignorar error, RuntimeDirectoryPreserve=yes y sin borrar el runtime final
en ExecStopPost. No se instala/reload/inicia/habilita/reinicia servicio alguno.

Las plantillas solo pasan nombres/valores no sensibles allowlisted al contenedor:
ambiente Development, HOME inexistente, archivos de credenciales/config AWS anulados,
IMDSv2 habilitado/IMDSv1 deshabilitado, región y diagnósticos desactivados. Ningún
Redis__/MongoDb__/Jwt__/HMAC ni access key/profile es requisito de env/argv.
No se selecciona todavía imagen nueva ni se crea autorización de arranque.

## Inicializador MongoDB — evidencia de source

- Registro: `AuthenticationServiceCollectionExtensions.AddAuthenticationDependencies`
  añade `AuthenticationInitializationService` como IHostedService.
- Invocación: `AuthenticationInitializationService.StartAsync` inicializa el hash
  dummy, establece presupuesto de registro y llama
  `AuthenticationDependencyAdmission.RefreshIndexAsync` →
  `MongoUsersInitializer.InitializeAsync`.
- `EvaluateOperationAsync(Register)` vuelve a verificar este requisito para admitir
  registro. No se elimina ni se separa automáticamente del startup.

| Operación | Comportamiento exacto |
| --- | --- |
| Client ping | No hay ping explícito en el inicializador; no confundir con handshake/monitorización internos del driver. |
| Database access | `GetDatabase` en DI obtiene un handle; el constructor comprueba namespace/settings/concerns. No es prueba de conectividad. |
| Collection lookup | `GetCollection<UserDocument>("users")` obtiene un handle, no ejecuta búsqueda ni crea colección por sí solo. |
| Index lookup | `Indexes.ListAsync` + consumo del cursor; una inspección inicial y, si se intentó crear, una posterior acotada. |
| Collection create | Posible **implícitamente** por createIndexes en namespace ausente; no hay llamada explícita CreateCollection. Puede incluir índice `_id_` automático del servidor. |
| Index create | `Indexes.CreateOneAsync(CreateIndexDefinition())` una vez por invocación cuando no hay índice relacionado incompatible ni uno compatible. |
| Document insert/update/delete | Ninguno en este camino de inicialización. Las operaciones CRUD del store existen para requests pero no se invocan aquí. |
| Index/collection drop, repair | Ninguno. |

Índice requerido: `users`, clave `{normalizedEmail:1}`, `unique=true`, collation
simple/binaria; nombre propuesto `normalizedEmail_1`. Se acepta un equivalente con
otro nombre. Un índice por ese nombre o campo incompatible produce Incompatible,
sin reparación/creación. Code26 en listIndexes se trata como namespace ausente.

Si falta el índice, se intenta crearlo una sola vez y se reinspecciona. Code11000
(datos duplicados) produce Incompatible; no limpia datos. Cancelación/ausencia/fallo
que no demuestre estado compatible produce Unavailable. Una excepción/ACK perdido
no reintenta create: solo reinspección, pudiendo aceptar un índice efectivamente
compatible creado por ese intento u otro actor.

**Clasificación de ambas posibles mutaciones estructurales: REQUIRED FOR APPLICATION
CORRECTNESS**, condicionales a que falte la estructura. La unicidad del correo normalizado
es una invariante de registro, no mantenimiento opcional ni una mutación inesperada.
La colección es el soporte estructural implícito para ese índice; crearla de nuevo no
es necesario si ya existe. No hay API CreateIfNotExists explícita: se implementa
inspeccionar → crear si falta → verificar, no una transacción check/create.

**MONGO STARTUP POLICY = EXPLICIT MUTATION AUTHORIZATION.** Conservar el diseño;
solicitar autorización específica antes de ejecutar un startup que pueda crear
colección/índice reales. Si ya existe índice compatible, llamadas posteriores no
crean nada; si hay incompatibilidad o falla el requisito, el registro no se admite.
El hosted service conserva liveness aunque Mongo esté Unavailable/Incompatible;
no convierte dependencia indisponible en permiso de registro ni en readiness.

**Idempotencia: PARTIAL como acreditación de este gate.** Se prueba con dobles la
lógica de invocación repetida, aceptación de equivalente, cero creates cuando ya
está listo, creación única, ACK perdido y fallos. No se ejecuta servidor real ni
se acredita ahora idempotencia distribuida/concurrencia/durabilidad del servidor.

## HTTP, usuario e imagen

`/health` permanece **LIVENESS**, AddHealthChecks sin checks de dependencias.
**READINESS ENDPOINT = ABSENT.** No se añade ninguno. Contratos sin cambios:

- POST `/api/v1/auth/register`.
- POST `/api/v1/auth/login`.
- POST `/api/v1/auth/logout`.

Dockerfile conserva bases fijadas .NET10 y etapas existentes, ahora declara
`USER 1654:1654`. El wrapper exige `--platform linux/amd64 --user 1654:1654`, runtime
montado en ruta exacta read-only. `.dockerignore` excluye appsettings.Runtime.json;
el proyecto también lo excluye de output/publish. No se copia runtime ni se hornean
secretos por este contrato. El examen de una imagen nueva queda pendiente: no se
afirma un scan de imagen que no se ha construido. Puerto base expuesto8080/8081;
no se ha acreditado listener en este gate.

Prueba Linux local mediante WSL/Ubuntu, namespace privado de mounts y red,
tmpfs propio sobre /run y JSON sintético propio: archivo regular0600/1654:1654 en
la ruta exacta, proceso1654:1654 puede abrir/parsear; bind RO impide apertura de
escritura con EROFS sin truncate/write. No toca runtime del host ni A. No acredita
un contenedor API real; acredita permisos/ruta/UID y RO con kernel Linux.

### Invalidación para deployment

- OLD ROOT/INDEX DIGEST `sha256:5da876d7cd52b36d6f63d84cf9856b7d6130715cd00f93bf7daef45c3e76aa9f`.
- OLD AMD64 DIGEST `sha256:852607824b0b5630567c52b9c067ab72ba290980f577def8a41235b590515f2d`.
- Ambos **SUPERSEDED FOR DEPLOYMENT** para el contrato actual. No se borran imágenes
  ECR, no se usa latest ni digest alternativo. Source anterior no incorpora el provider.

## Evidencia local y límites

Release con SDK **10.0.404**, MSBuild.dll explícito, sin restore, instalaciones,
cambios de dependencias o pin global. API + UnitTests y helper + suite del helper:
**0 warnings / 0 errores**. **728/728 unit tests PASS**, incluidos44casos nuevos
en RuntimeStartupContractTests/MongoStartupContractTests. Suite histórica del helper:
47/47 +1005/1005 +78/78 = **1130/1130 aserciones PASS**. No sumar aserciones del
helper como si fueran casos xUnit. No se ejecutan suites Integration/Security que
puedan levantar servicios reales o Testcontainers.

Recibo/archivos/trx en
`C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-start-contract-local-2026-11-18T14-01-43-640Z\`.
Una primera ronda tuvo build limpio y723/724 tests: expectativa de prueba de providers
omitía los settings por ApplicationName; se corrigió la expectativa, no el orden real.
El recibo final verifica global.json y seis packages.lock.json idénticos antes/después;
lock API mantiene SHA256 `21f03fe61b94ddc32c5e61af45f264da4bb95e4c6971565813d520a0742e49ee`.

**DEPLOY-CONFIG-03/04 CLOSED**, INITIALIZATION CONTRACT SATISFIED,
initialize histórico **1**, G `73041e6288899e1528f3b8f25c39e888` **APPLIED**, API A
NOT RUNNING y START COUNT0: baselines preservados, no verificaciones remotas nuevas.
AWS/SecretsManager/SSM/realMongo/realRedis/DynamoWrites/initialize/buildImagen/push0.
Sin Terraform/IAM/daemon-reload/start/ALB/HTTPS/commit/push. Ningún acceso a B.

**NEXT GATE = NEW ENVER.API IMAGE BUILD + LOCAL VALIDATION**, autorización separada.
No inicia automáticamente construcción, publicación ni despliegue. STOP tras la
verificación local y análisis; solo registro documental de esta entrega.
