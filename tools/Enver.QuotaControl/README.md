# ENVER Quota CONTROL — transporte implementado, no ejecutado contra Valkey

Baselines aceptados: [003 §§111–113](../../specs/003-infrastructure/task.md); transporte local en §114. Source de interpretación de cuota `078130627bc50a9dc2feee5fb699cedbea87dbbb`.

**CONTROL TOOL: IMPLEMENTED / LOCALLY VERIFIED / NOT EXECUTED.** Transporte INVENTORY implementado con BCL y probado solo con streams/dobles en memoria. **Ningún socket/TLS real, AWS/Redis real, password operativa, usuario/group o túnel creado.** Esta entrega no autoriza usar su capacidad de conexión. No iniciar Enver.API/Docker ni generar G. C/I siguen **APPROVED / NOT APPLIED** y DEPLOY-CONFIG-01 bloqueado por G/CONTROL; no prueba drenaje/continuidad.

## Arquitectura y archivos

Aplicación de consola .NET 10, SDK fijado por el `global.json` existente. Sin paquetes externos ni referencia a Enver.API, solución o imagen aceptada. No host ASP.NET, DI de API, initializer Mongo, AWS SDK, clientes de Secrets Manager, HMAC ni configuración ENV.

| Archivo | Responsabilidad |
| --- | --- |
| `Enver.QuotaControl.csproj` | Ejecutable independiente; recurso Lua embebido; cero PackageReference |
| `NuGet.Config` | Sources vacíos para restore offline; auditoría NuGet deshabilitada en ambos proyectos |
| `Contract.cs` | Descriptor exacto, snapshot, G/tiempo, plan de12campos, readback estricto |
| `descriptor.approved.json` | Descriptor no secreto aceptado; no G ni secretos |
| `RedisProtocol.cs` | Lista cerrada de comandos/respuestas, categorías de error saneadas e interfaz |
| `Inventory.cs` | Cursor completo SCAN, deduplicación, tipos/TTL/expiración/zsets y reporte saneado |
| `RecoverClosed.lua` | Nuevo CAS; TYPE M/R, TIME y único HSET M.closed |
| `ControlService.cs` | Inventario/precheck/readback; apply protegido para simulaciones; sin retry |
| `Program.cs` | CLI estricto; opt-in de transporte solo para inventory, handle de pipe sin secreto |
| `RemotePolicy.cs` | Allowlist por operación, validación de argumentos y forma de respuestas |
| `PipeCredential.cs` | Frame binario limitado, secreto en bytes, EOF y borrado de buffers propios |
| `RespCodec.cs` | RESP2 acotado/estricto y encoding; AUTH sin convertir password a string |
| `RemoteTransport.cs` | TCP/TLS endpoint fijo, SNI, AUTH único, timeouts, sesión sin reconnect/retry |
| `InventoryExecution.cs` | Evidencia saneada, clasificación de resultados y exit codes de inventory |
| `../Enver.QuotaControl.Tests/Enver.QuotaControl.Tests.csproj` | Runner de pruebas unitarias independiente, solo ProjectReference al CONTROL |
| `../Enver.QuotaControl.Tests/FakeRedis.cs` | Doble Redis totalmente en memoria; modelo CAS, no intérprete Lua |
| `../Enver.QuotaControl.Tests/Program.cs` | 50 casos base, adaptados a nuevos exit codes; G fixture no operativa |
| `../Enver.QuotaControl.Tests/RemoteTests.cs` | Pruebas de transporte/RESP/política/cancelación/salida sin red |
| `../Enver.QuotaControl.Tests/RemoteDoubles.cs` | Sesión/duplex/pipe simulados en memoria, sin sockets ni TLS handshake |

Los `bin/obj` generados están ignorados por las reglas existentes. No se modifica la solución de ENVER ni se compila la API.

## CLI y protección

Comandos reconocidos: **precheck, inventory, plan-closed, apply-closed, readback**. No activate, maintain, fence, initialize ni comandos arbitrarios. Opciones permitidas:

- `--descriptor <archivo-json-no-secreto>`: obligatorio; parser rechaza propiedades desconocidas, duplicadas, faltantes y contenido distinto al descriptor aceptado. Límite16KiB.
- `--generation <G-aprobada-posteriormente>`: obligatoria exclusivamente para plan/apply/readback. 32hex minúsculas, no todo ceros. **Nunca se genera**.
- `--server-time-us <entero>`: obligatorio exclusivamente para plan-closed. Positivo y ≤9007199254740991−900000000. Es entrada explícita, no reloj local ni tiempo de servidor ya acreditado.
- **Solo inventory:** `--transport direct` o `--transport ssm-tunnel`; obligatoriamente acompañado de `--credential-pipe <handle-heredado>`. Para ssm-tunnel también `--tunnel-port <1024..65535>`. Direct fija6379 y rechaza tunnel-port. No opción host/endpoint/username/database/URI/password/token/config secreta ni override de TLS.

**Sin --transport, inventory sigue RemoteTransportDisabled (exit20); precheck/readback siguen deshabilitados remotamente y apply sigue ApplyDisabled.** Solo inventory puede optar por transporte y consumir el pipe. `plan-closed` permanece puro. No fallback a runtime ni a credenciales de máquina/ENV. El CLI no abre red hasta descriptor/opciones/frame de credencial válidos; no se ha ejecutado ese camino con un transporte real.

**Apply remoto tiene dos barreras:** CLI bloqueado y ControlService exige IsLocalSimulation=true; RemoteTransport devuelvefalse y su allowlist además rechaza EVAL. No aceptar un booleano CLI como prueba de autorización/drenaje/exclusión. La marca de los dobles no es una sandbox contra implementaciones maliciosas.

Se puede planificar posteriormente con G y tiempo suministrados expresamente por el responsable. No se incluyen ejemplos con una G que pueda confundirse con una época aprobada. Las pruebas usan una fixture constante identificada como no operativa, sin RNG, generación de credenciales o procedimiento de época.

## Contrato y comportamiento

Destinos inmutables: DB0/development, metadata `enver:{development:auth:ratelimit}:metadata`, registry `enver:{development:auth:ratelimit}:registry`, MATCH `enver:{development:auth:ratelimit}:bucket:*` (sin la tilde ACL).

Snapshot exacto:

```text
development|quota-development-academic-initial-v1|hmac-development-fecf274e-v1|academic-initial|5000|67108864|3|900000000|20|900000000|10|60000000|60|60000000|5|60000000|300|60000000|30|60000000
```

Plan: protocol1, snapshot exacto, G explícita, stateclosed, count0, horizon0, dirty0, version1, fence=t, last=t, drain=t+900000000, maximumWindow900000000. UNKNOWN es procedencia externa, **no campo13 ni G ficticia**. Version1 no significa never-used. El plan devuelve los12campos; no acredita cese, tiempo ni permiso de escritura.

Inventario:

- SCAN de cursor0 hasta cursor devuelto0, **MATCH exacto, COUNT100 sin TYPE filter**; páginas vacías no finalizan; deduplicación de keys. No KEYS/fallback, notificaciones ni solo registry como inventario.
- TYPE/PTTL/PEXPIRETIME de cada key única; HGETALL de metadata hash; ZCARD y ZRANGE rank-paginado WITHSCORES de registry/buckets zset; contraste de miembros, formas, scores, horizontes/expiración y huérfanos.
- Relectura final de M/R; se conserva cualquier presencia observada inicial o final. Token de sesión fijo; pérdida/error de conexión o incoherencia invalida token/cierra canal. Respuesta inválida/cursor repetido/cotas ⇒ abortar, no truncar/limpiar. No reconnect, ni seguimiento MOVED/ASK. Esto observa una conexión, **no acredita cambios de primary silenciosos fuera del canal**.
- Límites:5000buckets,5000miembrosregistry,300eventos/bucket,10000páginasSCAN, páginasZRANGE≤100, hash≤256pares con cotas de cadenas. COUNT es hint y no se interpreta como límite de respuesta. TimeoutCLI30s/cancelación, sin retries. No es un inventariador de topología multi-primary.
- Bucket observado aunque desaparezca antes de leerlo sigue bloqueando el precheck; requiere otra pasada completa. Residuos de cualquier tipo impiden precheck/apply. Vacío no prueba primer uso.

Apply simulado realiza precheck completo, después **un EVAL**, y readback final. Lua solo permite dos keys exactas y snapshot/G exactos; comprueba M/R ausentes dentro del punto atómico, lee TIME y crea M con un HSET de12campos. No sobrescribe, crea ready, modifica buckets/registry, cambia TTL ni limpia residuos. No DEL/ZREM/PERSIST. No declara un inventario global atómico: **un bucket que aparece tras el precheck puede coexistir con el M.closed creado**; el readback detecta la discrepancia y conserva cierre. La exclusión externa es imprescindible antes de cualquier uso real.

Respuesta Lua: `["enver.auth.quota.recovery-closed", "1", status, category, timeμs]`; éxito solo `created_closed/ok`, abortos reconocidos `metadata_exists/registry_exists/invalid/time`. Otra forma/status/tiempo o error desde despacho ⇒ resultado incierto, sin retry. Un EVAL confirmado no acepta metadata existente ni con la mismaG. Readback valida TYPEhash/HGETALL exactamente12campos/PTTL-1, snapshot/C/I/G, todos los valores del plan, y registry/buckets ausentes. Si metadata se pierde después de éxito/resultado incierto, no se recrea automáticamente.

## Matriz exacta de comandos

Son comandos del núcleo, no permisos concedidos. **Solo inventory tiene transporte remoto implementado**; el resto de la matriz describe flujos puros/simulados. En la conexión de inventory se añade exclusivamente AUTH de handshake, nunca probes ni scripting-cache.

| Subcomando | Redis commands required | read/write | scope | riesgo |
| --- | --- | --- | --- | --- |
| precheck | TYPE, PTTL, PEXPIRETIME, SCAN, TIME; HGETALL si M.hash; ZCARD/ZRANGE WITHSCORES si hay zset | Lectura | M/R/B exactos, DB0 | SCAN enumeraDB aunque MATCH filtre; lecturas no prueban never-used/cese |
| inventory | TYPE, PTTL, PEXPIRETIME, SCAN, TIME; HGETALL si M.hash; ZCARD/ZRANGE WITHSCORES si hay zset | Lectura | M/R/B exactos, DB0 | Nombres/valores opacos solo en memoria; cambios concurrentes limitan cobertura |
| plan-closed | **Ninguno** | Puro/local | Descriptor/G/tiempo explícitos | Un plan válido no prueba tiempo, G aprobada ni autorización |
| apply-closed | Precheck + **EVAL**, dentro **TYPE/TIME/HSET** + comandos readback | Lectura + único HSET | Escritura solo M; TYPE M/R dentro Lua | CAS no cubre todosB; resultado perdido/OOM puede ser incierto; CLI bloqueado |
| readback | TYPE, HGETALL, PTTL, PEXPIRETIME, SCAN, TIME; ZCARD/ZRANGE WITHSCORES si aparece zset | Lectura | M/R/B exactos, DB0 | Nunca corregir discrepancias; unknown/ready/residuo ⇒ rechazo |

**No HLEN/HMGET:** se usa HGETALL. Allowlist exhaustiva de wire INVENTORY: **AUTH, SCAN, TYPE, PTTL, PEXPIRETIME, HGETALL, ZCARD, ZRANGE WITHSCORES, TIME**. AUTH solo al inicio, exactamente +OK; no se expone como comando arbitrario. Sin HELLO/PING/INFO/CLIENT/SELECT/KEYS/COMMAND/MONITOR/EVAL/EVALSHA/SCRIPT/HSET/DEL/ZREM/PERSIST/FLUSH*/CONFIG ni familia revoked/dev. Todo otro subcomando remoto se deniega antes de encoding/I/O; no hay Execute(rawstring).

ACL no restringe EVAL a un SHA/argumentos concretos; HSET no restringe el valor state. No promesa de separación absoluta de autoridad. Ni SCAN MATCH ni el host administrativo restringen por sí mismos la enumeración al prefijo. No alterar runtimeACL/IAM/grupo/red a partir de esta tabla.

## Salida y seguridad

Una línea JSON; plan y flujos locales conservan `schema=enver.quota-control/1`, `mode=local-only`. Plan `planned_not_applied`, UNKNOWNexterno/operationallyApprovedfalse/serverTimeVerifiedfalse/admissionOpenedfalse. **Inventory con transporte usa schema=enver.quota-control/2, mode=inventory-remote** y el mapa siguiente; no confundir con el resultado del plan. Errores antes de abrir transporte pueden conservar el envelope local/1, pero inventory ya usa sus nuevos códigos. No eco de argumentos, errores de servidor o excepciones.

Inventory proyecta presencias/cantidades/discrepancias y `execution`: runId local no sensible (fecha+contador, no G), inicio/fin UTC, duración monotónica, scanStartedUtc/scanFinishedUtc, initialCursor/finalCursor, páginas, uniqueKeys, duplicateKeys, completed y serverTimeMicroseconds. `connection`: endpoint lógico, IP:puerto TCP elegido (en túnel es loopback, no IP remota de Valkey), protocolo/cipher TLS y scope de topología. Datos parciales en fallos se marcan como incompletos; no inferirceros de camposnull. Sin nombresB/miembrosR/eventos/metadata cruda ni secretos. No serializar observaciones internas; usar proyección saneada. Sin AWS SDK, ENV, static keys o secretstores. No rota/genera credenciales ni escribe logs/evidencia a disco por defecto.

| Exit inventory | Significado |
| --- | --- |
| 0 | PASS: recorrido completo, M/R ausentes, 0buckets/discrepancias, cursor0→0, sin duplicados y conexión coherente |
| 10 | RESIDUE_FOUND: metadata/registry/buckets presentes, incluso vacíos o solo presentes en la relectura final |
| 20 | INCOMPLETE: timeout/cancelación, SCAN incompleto, duplicados, sesión perdida/incoherente, MOVED/ASK/estado topológico no utilizable; incluye inventory sin transporte |
| 30 | AUTH_TLS_FAILURE: TLS/certificado/hostname o autenticación fallida |
| 40 | PROTOCOL_FAILURE: RESP inválido, EOF en frame, tipos/forma incompatibles, TIME inválido |
| 50 | POLICY/ALLOWLIST_FAILURE: NOPERM, comando/opciones/descriptor no permitidos |
| 60 | CREDENTIAL_INPUT_FAILURE: handle/frame/longitud/EOF/formato de credencial inválido |
| 70 | LOCAL_FAILURE: fallo local no clasificable, siempre saneado |

Incompletitud de sesión/recorrido o duplicados tiene prioridad sobre residuo al clasificar una pasada ambigua. Residuos conocidos no autorizan retry automático para buscar un vacío. Duplicados SCAN pueden ser legítimos: se deduplican para contar, pero **por conservadurismo toda duplicación produce20**, nunca PASS. Exit0 ya no significa simplemente «terminó».

Conclusión emitida solo con exit0: **CURRENT INVENTORY EMPTY**. `exclusionAccreditedByTool=false`, `operationallyApproved=false`, `admissionOpened=false`. Jamás afirma primeruso ni exclusión acreditada. La orquestación externa solo podrá ampliar la conclusión según §113 si acredita exclusión, inflight y topología/primary en la ventana; un JSON con exit0 no prueba esas condiciones.

## Transporte TLS/SNI, sesión y límites

- Direct: DNS del endpoint fijo→primera IPv4→un TCP6379. Una dirección/un intento; no resolver otras IP como failover, pool, multiplexación, retries o reconnect.
- SSM: TCP127.0.0.1:puerto explícito, pero **SslClientAuthenticationOptions.TargetHost siempre es el hostname real Valkey**. SNI y validación de hostname no usan localhost. La herramienta no abre SSM ni verifica quién abrió el listener; el responsable debe acreditar destino/túnel/sesión.
- TLS1.2/1.3 exclusivamente, sin resumption TLS, cadena del sistema con revocación Online; callback solo endurece: rechaza SslPolicyErrors, certificado ausente/autofirmado leaf y cadena inválida. No roots/clientcerts personalizadas, TrustServerCertificate o bypass de hostname/chain. OS trust/CRL-egress y compatibilidad real quedan por validar bajo autorización posterior.
- Operación completa≤30s; DNS+TCP+TLS≤5s; AUTH y cada roundtrip≤5s. Cancelación/Ctrl+C, sin retry. Un fallo cierra el canal e invalida sesión; un solo AUTH por conexión. Recursos cerrados en finally/awaitusing también en fallos.
- Se comprueba conexión TCP/remote endpoint estable y cierre del socket antes/después de comandos; se rechazan redirecciones/errores topológicos. Scope explícito: **SINGLE_PINNED_TLS_CONNECTION_NOT_INDEPENDENT_PRIMARY_ATTESTATION**. No INFO/ROLE, detector AWS ni prueba de ausencia de cambios silenciosos detrás de un túnel; esa acreditación sigue externa. Ante señales de incoherencia devuelve20. No usarlo como inventario multishard/replicas.
- RESP2: simple/error/integer/bulk/array/null; UTF8estricto, CRLF, enteros canónicos con límites. Max4096bytes/bulk,1024bytes/línea,20000elementos/array,22000nodos,8MiB/respuesta y dos niveles de contenedor (SCAN externo+keys). Rechaza RESP3, tamaños/profundidad extra, EOF/truncación y nulls donde el comando no los admite. Valida forma por comando antes de entregar respuesta al dominio; no pipeline.
- Límites existentes de dominio5000buckets/registry,300eventos/bucket/10000páginas continúan. Sobrecota aborta, no trunca ni crea excepción de cleanup. No se solicita cambiar memoria, TTL o datos de Valkey.

## Credenciales por handle de pipe anónimo

Única entrada del CLI: `--credential-pipe <handle-numérico-heredado>`, mediante `AnonymousPipeClientStream(PipeDirection.In, handle)`. .NET valida que el handle sea un pipe, no un archivo regular. El handle es identificador no secreto, **no una ruta a archivo/config ni password**; la custodia del padre debe garantizar pipe anónimo privado de un solo uso e herencia solo al hijo. No stdin/TTY, ENV, passwordargs, secretfiles/appsettings o lectura de Secrets Manager en este binario.

Contrato del productor futuro: frame `uint32 big-endian N` + Nbytes de password ASCII imprimible compatibleRBAC, **16≤N≤128**, sin espacio/coma/comillas/barra/@; después cerrar extremo escritor para EOF. No username recibido del pipe: usuario fijo `enver-dev-quota-control`. Bytes extra/EOFfaltante, longitud inválida o cancelación abortan antes de abrir conexión; lectura espera como máximo el presupuesto total30s. No usar shell echo, redirección de archivo o argumentos interpolados para producir el secreto.

La password se mantiene como byte[] sin conversión a string, se codifica AUTH en memoria, se borra la copia/frame y se dispone el secreto después de AUTH; ToStringdevuelve[REDACTED]. Payloads de error del servidor no se guardan en mensajes/salida. Borrado **best-effort de buffers propios**: no promesa de zeroization de copias internas de OS/SslStream/CLR, swap/dumps/EDR. El host administrativo debe controlar dumps/transcripts/debug/otrosprocesos. No logs de protocolo/connectionstrings/AUTH/pipe ni ecos de material recibido.

El productor/broker con custodia de una password operativa y generación/API CreateUser **no se implementan aquí**. No se genera una contraseña real. Se puede construir bajo autorización posterior un padre .NET con AnonymousPipeServerStream, herencia restringida, spawn de este CLI con su clienthandle, DisposeLocalCopyOfClientHandle en el padre, escritura del frame y cierre del escritor. No copiar handles entre sesiones/consolas ni tratarlos como tokens recuperables. El pipe/destino debe existir por procedimiento aprobado; el CLI no lo crea ni recibe secretos del shell history.

## Plantillas de ejecución futura — NO EJECUTADAS

`$ControlPipeHandle` representa únicamente el handle heredado que entregará el padre aprobado, no la password. No funciona como copiar/pegar desde otra consola. Tras autorizar identidad/grupo/ruta/custodia y acreditar exclusión/primary, el padre podría iniciar:

```powershell
dotnet tools/Enver.QuotaControl/bin/Release/net10.0/Enver.QuotaControl.dll inventory --descriptor tools/Enver.QuotaControl/descriptor.approved.json --transport ssm-tunnel --tunnel-port 16379 --credential-pipe $ControlPipeHandle
```

O, desde un host privado directamente autorizado, con otra selección explícita de ruta:

```powershell
dotnet tools/Enver.QuotaControl/bin/Release/net10.0/Enver.QuotaControl.dll inventory --descriptor tools/Enver.QuotaControl/descriptor.approved.json --transport direct --credential-pipe $ControlPipeHandle
```

No ejecutar StartSession/crear listener/user/group/password como parte de estos comandos. No apply/remotewrites ni G. Capacidad técnica **no es autorización de ejecución**.

## Pruebas locales reproducibles

Desde la raíz, exclusivamente estos proyectos (no solución/API):

```powershell
dotnet restore tools/Enver.QuotaControl.Tests/Enver.QuotaControl.Tests.csproj --configfile tools/Enver.QuotaControl/NuGet.Config -p:NuGetAudit=false --verbosity minimal
dotnet run --project tools/Enver.QuotaControl.Tests/Enver.QuotaControl.Tests.csproj --no-restore --configuration Release
```

Runner unitario BCL, sin xUnit/MSTest/testSDK ni instalaciones. Suite base50pruebas de §112 conservada con adaptación de exitcodes;40casos de transporte local: **90/90 PASS, failed0, exit0**. Cobertura: parser/descriptor/snapshot/tiempos/G, residuos, SCAN/duplicados/sesión/cotas, CASsimulado/readback, RESP válido/inválido/tipos/nesting/tamaño/UTF8, TLS/SNIconfig, allowlist y prohibidos, AUTHfailure/NOPERM, cancelación/recursos, reporte/evidencia/saneamiento y códigos. Dobles duplex/streams **sin DNS, sockets, handshakeTLS, AWS, Redis real, Docker ni productor de credenciales reales**. Las fixtures no operativas no son una época ni password aprobadas. Esto verifica configuración/orquestación local, **no handshake/certificado/Valkey reales**.

**Lua no fue ejecutado en un intérprete ni en Valkey.** Pruebas estructurales verifican el recurso realmente embebido: snapshot, secuencia exacta TYPE/TYPE/TIME/HSET,12pares HSET y ausencia de ready/otros comandos; el modelo CAS en FakeRedis prueba la orquestación/respuestas, no equivalencia de ejecución Lua. No presentar esta evidencia como integración/atomicidad remota/ACL/TLS/durabilidad validadas. Un error inicial local de compilación por nombre Array se corrigió; el resultado final anterior es el exitoso. Sin Redis/Docker/AWS ni G operativa generada.

## Antes de inventory real y de D-REC-EPOCH

1. Autorizar un incremento **read-only** concreto: actor/host/cuenta-región/destinoDB0/primary/artefacto/hash/comandos/ventana y alcanceDB de SCAN; no permission-change implícito.
2. Revisar/aprobar el transporte y mecanismo de pipe implementados localmente, hash/versión del artefacto futuro, productor/estación con custodia segura y sin secretos CLI/ENV/logs. Transporte implementado no significa TLS/RESP reales validados. Acreditar sesión/primary en la ventana sin probes invisibles; no sustituir falta de evidencia por un flag.
3. Crear/asociar/revocar la identidadCONTROL temporal solo con autorización AWS separada según §113, excepción temporal de membresía poseída por Terraform y freeze/restituciónbaseline. Acreditar ACL efectiva, red/túnel autorizado, OS trust/revocación, TLS/auth/timeout/RESP reales y custodia del informe, únicamente en ejecución futura autorizada. No usuario runtime/staticAWSkeys ni abrirSG por defecto.
4. Para inventario que sirva a recuperación: universo completo de escritores, exclusión e inflight0 verificables, custodia de evidencia y vigencia del corte. Una pasada read-only sola no prueba continuidad ni un vacío simultáneo global.
5. Para D-REC-EPOCH además: Tlast/Wsafehistórica/reloj/cese continuo, inventario final vacío por expiración natural, gates D-REC-MAINT/PATH/LOSS/CONTROL y controlCAS verificado en entornos posteriores autorizados. **900s solo acredita Wtarget**, no Wsafe. Residuos persistentes ⇒STOP, no cleanup.
6. Generación/custodia/aprobación de G, escritura closed y apertura/deployment son autorizaciones futuras distintas. No activate/maintain/fence implementados por este incremento; no cambios de imagen/API.

**STOP después de implementación y pruebas locales.**
