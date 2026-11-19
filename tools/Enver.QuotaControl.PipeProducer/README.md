# Productor de credencial CONTROL — solo preparación local

Biblioteca independiente .NET10/BCL para Windows x64. No es un CLI, generador de
passwords, cliente AWS, broker de CreateUser ni supervisor de revocación. **No se
ha invocado con credenciales reales ni se ha iniciado CONTROL operativo.**

## Contrato de seguridad

- Entrada: `ReadOnlyMemory<byte>` del proceso creador. No acepta rutas de secretos,
  argv, ENV, clipboard, stdin, archivos, configuración o funciones de logging.
- El creador mantiene la memoria válida hasta terminar y borra su copia después.
  El productor no genera ni convierte la contraseña de entrada a `string`.
- Frame compatible: uint32 big-endian + 16..128 bytes ASCII compatibles con RBAC
  + EOF. Cierra el escritor tras transferir; borra el frame propio en éxito/error.
- `AnonymousPipeServerStream` inicialmente **no heredable**. Duplica solo el
  extremo lector para el hijo; `CreateProcessW` usa `STARTUPINFOEX` con
  `PROC_THREAD_ATTRIBUTE_HANDLE_LIST`. Nunca herencia indiscriminada.
- El hijo se crea suspendido, se asigna a job privado con `KILL_ON_JOB_CLOSE`, se
  cierran en el padre los extremos/duplicados no usados y después se reanuda.
  La lista explícita contiene credencial, stdout privado y los NUL de stdio;
  ningún otro handle del padre se entrega al hijo.
- Ejecutable absoluto `Program Files/dotnet/dotnet.exe`, sin shell. API pública
  fija DLL/descriptor y sus SHA256 de §118, solo `inventory`, túnel16379 y handle.
  Rechaza reparse points en esas rutas y presencia de los bytes del secreto en
  argumentos/entorno antes de crear el proceso. La comprobación no sustituye
  permisos de integridad del host ni elimina TOCTOU frente a un administrador.
- Entorno nuevo mínimo: SystemRoot/WINDIR, diagnósticos .NET deshabilitados y
  telemetría CLI deshabilitada. No hereda AWS/GitHub/configuración del padre.
- Stdin y stderr a NUL. Stdout de inventory se recibe en pipe privado, máximo
  64KiB, profundidad JSON8; nunca se registra/devuelve crudo. La proyección
  `InventoryEvidence` verifica schema/endpoint/DB/exit, campos tipados y presencia
  del secreto también en strings JSON decodificados; rechaza duplicados,
  inconsistencias PASS, truncación, salida extra o excesiva. Solo devuelve
  métricas/tiempos/conexión saneados; no keys, HGETALL ni mensajes del servidor.
- Presupuesto de ejecución45s (incluye arranque/transferencia y los30s del
  consumidor), más hasta10s de esperas de cleanup (5sproceso +5slector): reservar
  **55s** en la ventana. Cancelación/error/deadline cierran pipe y job propios;
  no reconexiones. Cleanup no acreditado produce error `Cleanup`. No es garantía
  de tiempo real ante bloqueo/fallo del kernel ni sustituye supervisión externa.
- Excepciones solo enumeraciones saneadas, sin inner exceptions ni payloads.

**Modelo de confianza:** padre único/revisado, sin código concurrente ajeno que
cree procesos con herencia general durante el corto intervalo de duplicación.
Todos los lanzamientos del productor usan lista explícita. Los tests prueban un
hermano no autorizado y un handle heredable ajeno excluido; esto no es sandbox
frente a root/administrador, mismo usuario con derechos de duplicar handles,
inyección, debugger, dumps o código malicioso dentro del padre/hijo. Deben
acreditarse host, ACL de ejecutables, dumps/EDR/swap y custodia. Borrado de buffers
propios es best-effort; no garantiza borrar copias internas SO/CLR/JSON.

## Pruebas locales

```powershell
dotnet restore tools/Enver.QuotaControl.PipeProducer.Tests/Enver.QuotaControl.PipeProducer.Tests.csproj --configfile tools/Enver.QuotaControl/NuGet.Config -p:NuGetAudit=false
dotnet run --project tools/Enver.QuotaControl.PipeProducer.Tests/Enver.QuotaControl.PipeProducer.Tests.csproj --no-restore --configuration Release
```

Solo biblioteca nueva y runner BCL, sin paquetes. El consumidor aceptado se
referencia **como DLL existente**, no como ProjectReference: no se recompila ni
modifica CONTROL, API o solución. Hijos reales Windows locales con fixtures
sintéticas prueban frame/EOF, consumidor existente, longitudes, handle-list,
hermano excluido, cierre, crash, cancelación, deadline, entorno mínimo, argv,
salida saneada/rechazada, errores y estabilidad de handles. Ninguna prueba abre
red, TLS, AWS, Redis o túnel ni ejecuta inventory operativo.

## Antes de uso real

`RunInventoryAsync(workspace, passwordEnMemoria, cancellationToken)` es una API,
**no una orden autorizada ahora**. Faltan freeze/exclusión/inflight, plugin/ruta,
creador/custodio operativo, integración con CreateUser/revocación supervisados y
aceptación de evidencia. No se puede copiar un handle entre consolas.

No generar password, instalar plugin, crear usuario/asociación, abrir sesión ni
invocar esta API con el workspace válido hasta autorización posterior delimitada.
El exit/evidencia de esta biblioteca jamás acreditan exclusión o G ni permiten
apply/initialize/fence/maintain/activate/deployment.
