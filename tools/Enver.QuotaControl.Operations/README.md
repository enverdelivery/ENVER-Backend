# Acta operativa PRE-CONTROL — auditoría y diseño, ventana NO INICIADA

**FASE 1 = NOT READY; NOT ACCREDITED.** Cierre acotado vigente en §16, posterior a la aceptación de §123. Este documento prepara una ventana; no la inicia.
Ni el checklist ni las lecturas puntuales establecen por sí mismos un freeze,
una barrera de datos o inflight0. No crear CONTROL/password, asociar, abrir
túnel/Redis o ejecutar inventory en esta entrega.

## 1. Acta obligatoria de ventana máxima15min

Designaciones recibidas del responsable en §9. Los campos temporales y las
acreditaciones técnicas pendientes deben completarse y aceptarse antes de pedir
Fase2; las designaciones no aceptan automáticamente todas las casillas siguientes.

| Campo | Valor actual |
| --- | --- |
| Responsable único humano de ventana | **Kevin Alexander Vega Zarza**, designación expresa; ventana no iniciada |
| Autoriza creación futura / aborto | **Kevin Alexander Vega Zarza**; no autorización de creación efectiva en esta entrega |
| Creador CONTROL futuro | **Kevin Alexander Vega Zarza**; proceso creador técnico aún no acreditado |
| Custodio de credencial temporal | **Kevin Alexander Vega Zarza**; no generar password real |
| Operador inventory futuro | **Kevin Alexander Vega Zarza**; no ejecutar inventory |
| Supervisor | **Kevin Alexander Vega Zarza** |
| Revocador | **Kevin Alexander Vega Zarza** |
| Verificador del baseline final | **Kevin Alexander Vega Zarza** |
| Responsable de escalado si expira sesión / falla revocación | **Kevin Alexander Vega Zarza**; protocolo de STOP/incidente conservado, sin fallback automático |
| Inicio UTC de freeze, Tfreeze | **NO INICIADO** |
| Cierre máximo UTC | **Tfreeze+15min**, pendiente de fecha real; vigilar también reloj monotónico |
| Fuente temporal / sincronización e incertidumbre | **PENDIENTE de acreditación**; no usar la hora de edición como Tfreeze/T0 |
| Cuenta / región / perfil | 926925605666 / us-east-1 / enver-admin-temp; revalidar STS justo antes |
| Recursos | RG y usergroup enver-dev-valkey; usuario temporal soloenver-dev-quota-control |
| Membresía baseline | **UserIds=[enver-dev-runtime]**, mismo grupo asociado al RG |
| Hashes CONTROL/descriptor/productor | Revalidar según task§119 y §118; no reconstruir CONTROL |
| Cobertura humana declarada | Kevin declara que **no existen otros operadores humanos autorizados concurrentes durante la ventana**; no prueba inexistencia de sesiones/credenciales o automatizaciones capaces |
| Cobertura de automatizaciones/estaciones/credenciales | **PENDIENTE inventario técnico y cierre verificable**; no inferir otros operadores a partir de pendientes técnicos |
| BarreraT0 y recibos de drenaje | **NO ESTABLECIDOS** |

Lista para aceptación por el responsable:

- [ ] Soy el responsable único designado y puedo mantener/escalar toda la ventana.
- [ ] Identifiqué todos los operadores humanos, sesiones administrativas, roles,
  scripts, estaciones y automatizaciones con capacidad de modificar ElastiCache
  o lanzar infraestructura. Constan cobertura y acuses de no concurrencia.
- [ ] No hay `terraform apply` en esta estación, otras estaciones, WSL, runners,
  workspaces remotos u otros repos relevantes; no hay ejecución encolada capaz de
  empezar durante la ventana. Ningún actor los iniciará hasta el cierre verificado.
- [ ] No hay cambios manuales/SDK/CLI ElastiCache concurrentes; se prohíben durante
  la ventana excepto el delta CONTROL expresamente autorizado posteriormente.
- [ ] No hay pipelines/workflows de infraestructura activos, queued, pending,
  waiting o programados; se impide su nuevo despacho durante la ventana por el
  procedimiento aceptado. No se modifica ningún workflow en esta preparación.
- [ ] Verifiqué GitHub y cualquier CI externo/workspace que el inventario haya
  identificado, incluidos eventos de otros repos. Adjunté evidencia actual.
- [ ] Apruebo la excepción **temporal**, sin cambio HCL/state/ignore_changes:
  agregar exclusivamente CONTROL → uninventory → retirar exclusivamenteCONTROL →
  eliminar usuario creado y comprobar baselineexacto. No sustituir UserIds completos.
- [ ] Runtime ACL/authmode/asociaciones y configuración RG no se modificarán.
- [ ] Barrera de escritores y pruebas de inflight están acreditadas aparte: freeze
  IaC no impide consumos Redis de una API/admin y no equivale a barreraT0.
- [ ] Plugin/ruta/TLS, productor revisado, custodia, hashes, supervisión de topología
  y permisos de alta/baja efectivos están listos antes de crear el usuario.
- [ ] Sesión administrativa vigente con margen≥20min y cierre disponible; no
  fallback ni renovación automática. Caducidad⇒STOP/escalado humano.
- [ ] Cualquier cambio/pending/timeout/error⇒STOP y cleanup del delta propio.
- [ ] El freeze termina solo tras ausenciaCONTROL y baselineverificado. Si la
  revocación falla o se exceden15min: **incidente abierto**, no reapertura automática
  de otros cambios ni afirmación falsa de cierre. No se garantiza TTL de usuario.
- [ ] Acepto exposición de enumeración DB por SCAN y custodia del informe saneado.
- [ ] Solicitaré autorización explícitaFase2: esta aceptación no crea recursos.

Formato de aceptación humana: responsable, inicioUTC, cierreUTC≤inicio+900s,
referencias de evidencia/cobertura/acuses, custodios, barrera/drenaje acreditados,
baseline/hash y aceptación de cada casilla. **Designaciones y declaración parcial
registradas en §9; acta final y ventana aún NO aceptadas/iniciadas.**

## 2. Qué se verificó realmente

- Backend HCL local; ningún workflow en checkout/refs locales de §118.
- GitHubAPI, repositorio **enverdelivery/ENVER-Backend**, defaultbranchmain:
  **0 workflows** y **0 runs** en estados queued/in_progress/waiting/pending/requested,
  2026-11-17T15:59:32.087Z–15:59:34.134Z. No existe un workflow registrado concreto
  que identificar como reconciliador en esa lectura. No demuestra ausencia de CI
  externo, otros repos ni futuros pushes/dispatches. Si aparece uno, registrar ID,
  path, commit/ref, triggers/schedules, reusableworkflows, scripts invocados y
  credenciales/runners; exigir evidencia de runs noactivos/noencolados y mecanismo
  de no despacho para toda la ventana. No basta la ausencia de una ejecución hoy.
- SnapshotWindows16:10:50.2823354Z: Terraformprocesses0/applyobservado0. No global.
- A/B se observaron mediante `observe-host.py` por SSM, sin conexiónRedis ni
  cambios de servicios. task§119 registraCommandIds/horas. El script no exporta
  argv/env/cron/ExecStart/logs; devuelve métricas e indicadores, **no una prueba
  transitiva de que todos los scripts son inocuos**.
- `ssm:DescribeSessions` paraA fue **AccessDenied**16:02:55.412Z; se detuvieron las
  llamadasAWS, no se consultaron sesionesB ni se amplióIAM. Falta evidencia de
  sesiones/otrosoperadores; no se solicita un permiso nuevo por inferencia.

## 3. Universo de escritores: cobertura y control mínimo faltante

Inventario obligatorio con dueño, vía de acceso, capacidad, disparadores,
autorreinicio, colas/conexiones y prueba de cierre para **cada** entrada:

1. A/B: contenedores (también detenidos con restart policy), dotnet/Enver,
   systemd/transientunits, listener8080, cron/anacron/at, timers y scripts transitivos.
2. Scripts y tooling local/WSL/otras estaciones: redis-cli/valkey-cli, helpers
   anteriores, procesos SDK, pruebas integración/seguridad/capacidad y CONTROL.
3. Runners/CI/workspaces remotos, otros repos y operadores/roles/sesiones
   administrativas o túneles que posean credenciales y ruta a Valkey.
4. Cualquier otro cliente identificado por propietarios de credenciales/red.
   La ausencia de SCAN o cliente visible no elimina la capacidad de escribir.

A/B: ceros observados de contenedores/procesos/listener/conexiones6379/jobs, pero
7timers porhost y1archivo cron/script sin indicador. Sus scripts transitivos y
otros hosts/holders no están auditados exhaustivamente. **UNIVERSE NOT CLOSED.**

**BarreraT0 futura:** punto de confirmación de un plano externo de mantenimiento
que controle **todos** los dispatchers y reinicios/colas/admins del universo,
impida nuevas escrituras y conserve el cierre durante inventory/revocación.
T0 se establece después del último acuse de bloqueo de despacho, no desde el
snapshot Docker, retiroALB, freezeTerraform o lecturaTIME. No existe ese plano
acreditado hoy. Cerrar el universo es requisito para probar su cobertura.

**Consumers=0** requiere observaciones actuales más cierre antirreinicio y acuses
de todos los dueños. **New writes afterT0 impossible under freeze** exige que la
barrera cubra también administradores/clientes con credencial, no solo pipelines.
**Inflightprevio resuelto/acotado** requiere respuestas terminales/recibos por
operación y cola/conexión, o cota exhaustiva demostrada de despacho/ejecución
servidor seguida de drenaje verificable. Incluir buffers/red/colas/retries y
comandos ya aceptados. TCP6379establecidas0 no excluye trabajo ya aceptado.

**Control mínimo faltante:** inventario cerrado + mecanismo externo de bloqueo
de todos los dispatchers/autorreinicios + protocolo de acuses/drenaje que
demuestre finalización de lo ya despachado. Si se necesita enforcement adicional
de infraestructura o coordinación, diseñarlo y autorizarlo por separado; no
implementar ACL/red/fence/lifecycle aquí ni considerar900s/timeoutcliente prueba.
**Inflight sigue UNKNOWN. T0 no tiene timestamp real.**

## 4. Plugin SSM y procedimiento futuro, sin ejecución

Windows10/AMD64, OS/procesoX64; AWSCLI **2.40.1**. Se requieren binariosWindowsx64
con procedencia/firma/versión/hash revisados. No hay plugin enPATH, registro de
desinstalación HKLM/HKCU ni cinco ubicaciones estándar inspeccionadas (§119).
Por tanto AWSCLI **no puede invocarlo desdePATH actual**. Puede existir fuera de
esas ubicaciones: aportar ruta y verificarla, o autorizar instalación local
posterior desde distribución oficialAWS. **No se descargó/instaló nada.**

Después de preparar y autorizar ruta/plugin, sin secretos en parámetros:

```powershell
aws ssm start-session --target i-0a66085b7b2411393 --document-name AWS-StartPortForwardingSessionToRemoteHost --parameters "host=master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com,portNumber=6379,localPortNumber=16379" --profile enver-admin-temp --region us-east-1 --no-cli-pager
```

**No ejecutado.** Supervisor futuro debe comprobar listener127.0.0.1/::1 únicamente,
registrarSessionId/PIDpropios, mantener topología/exclusión y terminar su sesión
al cerrar. TLS sigue extremo a extremo hasta Valkey; `TargetHost`/SNI fijo
**master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com**, sin bypass de cadena,
hostname o revocación. No SSH/B/directo/puerto público por fallback.

El [productor](../Enver.QuotaControl.PipeProducer/README.md) tiene presupuesto45s
de ejecución más10s de cleanup local, dentro de la ventana900s. Ajuste conservador
respecto§118:120screar/active +180sasociar +60stúnel +55sproductor/inventory
+**485scleanup** (215sretirar/propagar +180seliminar/ausencia +90scompararbaseline). Ninguna ventana
comenzó. Ante expiración/túnelroto/error/topologíacambiada/pending/cleanupfallido:
STOP, cerrar recursos propios, revocar delta propio si existiera y escalar; no
retry de inventory, renovaciónautomática, G, writesRedis o deployment.

## 5. Revalidación posterior a aceptación de §119 — sin iniciar ventana

Véase [task §120](../../specs/003-infrastructure/task.md) y [memoria §181](../../memoria.md).
**FASE 1 = NOT ACCREDITED.** Las secciones anteriores conservan la evidencia de §119;
esta actualización no firma las casillas ni inicia freeze, T0 o Fase 2.

| Condición | Estado | Evidencia UTC 2026-11-17 | Qué falta |
| --- | --- | --- | --- |
| Freeze | NO INICIADO / NO ACREDITADO | Terraform local0 a18:31:49; GitHub0workflows/0runsqueued-in_progress-waiting-pending-requested a18:34:01–02 | Responsable/custodios y aceptación; inicio/fin≤900s; cobertura/acuses y no despacho mantenido de todos los operadores/workspaces/automatizaciones |
| Universo | NO CERRADO | Registro writer/non-writer/unknown en task§120.4;45archivoslocales inspeccionados estáticamente, no ejecutados | Cuerpos/grafo transitivo efectivo de7timersycron/scripts porhost, otros clientes/holders/sesiones; observación actual más antirreinicio |
| T0/inflight | NO ESTABLECIDO / UNKNOWN | Sin fuente temporal acreditada ni recibos de barrera/drenaje | Barrera cubriendo registro cerrado, recibos terminales/cota servidor exhaustiva y verificación final mantenida |
| Perfil/SSMOnline | OBSERVADOS | STS18:31:47 identidad esperada; A/BOnline/agent3.3.5828.0 a18:36:42 | No sustituye freeze, plugin o permisos de sesión |
| Plugin/documento | NO UTILIZABLE / NO ACREDITADO | Sin plugin/ruta a18:39:07; DescribeDocument18:36:46exit254AccessDenied ⇒STOPAWS | Ruta instalada real, versión/AMD64/resoluciónCLI y evidencia de documento por medio autorizado |
| Integridad | BASELINE LOCAL CONSERVADO |20CONTROL/9files§119 y4hashesartefactosiguales a18:38:34;29/29PASS históricos | Uso real requiere creador/custodia/supervisión revisados; no repetir suite aceptada como sustituto |

No se repitió DescribeSessions denegado, no se amplió IAM ni se hicieron llamadas
AWS después de DescribeDocument AccessDenied. No nuevos RunCommand/StartSession;
los ceros de procesos/containers/conexiones/timers de A/B permanecen históricos.

## 6. Plugin: procedimiento concreto y discrepancia pendiente

La continuación interrumpida descargó el instalador oficial AWS:
`https://s3.amazonaws.com/session-manager-downloads/plugin/latest/windows/SessionManagerPluginSetup.exe`.
Ruta preparada:
`C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-pre-control-ssm-20261117\SessionManagerPluginSetup.exe`.
Paquete8 871 560bytes/FileVersion1.2.896.0; hash público y validación nativa
WinVerifyTrustPASS/firmanteAmazonWebServices,Inc. registrados en task§120.3.
La validación fue alternativa al cmdlet Authenticode fallido por TypeData duplicado;
no se cambió PowerShell/policy ni se ejecutó el instalador o elevación por el agente.

El responsable informó acciones manuales realizadas, pero la instalación no es
observable en PATH, registro HKLM/HKCU ni cinco raíces comprobadas; tampoco hay
entrada SessionManagerPlugin en PATH persistente. No se afirma ausencia global.
Windows/procesoX64 y AWSCLI2.40.1AMD64 no acreditan arquitectura de un plugin ausente.

Para Windowsx64, el responsable debe ejecutar el paquete verificado **como
administrador** y completar el instalador; ruta estándar
`C:\Program Files\Amazon\SessionManagerPlugin\bin`. Abrir una consola nueva para
recoger PATH; una ruta personalizada debe aportarse y revisarse. Después verificar
solo `session-manager-plugin --version`, arquitectura PEAMD64 y que el mismo entorno
de AWSCLI lo resuelve. No usar StartSession como prueba, no copiar/extraer binarios
para eludir elevación ni modificar PATH persistente unilateralmente.

La consulta del documento requerido fue AccessDenied, no «documento inexistente».
Falta evidencia de disponibilidad por un mecanismo ya autorizado; no inferir
permiso de StartSession ni solicitar/ampliar IAM automáticamente. El comando de §4
sigue siendo una plantilla futura NO ejecutada, con TLS/SNI/validación intactos.

## 7. Recibo mínimo de barrera y drenaje — propuesta no aplicada

Requiere primero universo cerrado; cada entrada UNKNOWN capaz de escribir bloquea.
El registro por candidato debe incluir dueño, ejecutable/configuración efectivos,
vía de acceso, triggers/reinicios/colas/retries y mecanismo de bloqueo verificable.
No basta una declaración humana sin cobertura y evidencia del mecanismo.

1. Bajo ventana aceptada, obtener recibo de **cada** dispatcher/admin/harness:
   identidad, bloqueo de nuevos despachos/autorreinicios, evidencia, última operación
   emitida y colas pendientes. Mantener bloqueo hasta cierre verificado.
2. Coordinador único registra T0 **después del último recibo** de todo el registro,
   no desde snapshotDocker/socket/freezeIaC. FuenteUTC con sincronía/incertidumbre
   acreditadas más monotónico para duración≤900s; no comparar relojes A/B/cliente
   sin relación acreditada.
3. Reconciliar operaciones previas con respuestas terminales/recibos exhaustivos
   o demostrar cota completa servidor/colas/red/buffers/retries y verificar drenaje.
   Respuesta perdida tras despacho = unresolved/STOP, no cancelación acreditada.
4. Recibo final: horaUTC/monotónica, evidenciasreferenciadas, consumidores0,
   despachosbloqueados, pendientes0, bloqueo mantenido y comprobación de cobertura.

No hay hoy fuente temporal acreditada, T0 real, recibos/cota ni verificación final.
El protocolo de cuotas devuelve uncertain tras fallo posterior a despacho:
timeoutcliente/espera900s/procesos-conexiones0 no son prueba de inflight0.
Este contrato no implementa ni autoriza barreras nuevas de IAM/red/ACL/lifecycle.
**STOP: inflight UNKNOWN; no CONTROL/password/membresía/túnel/Redis/inventory/G.**

## 8. Revalidación SSM posterior — §120 aceptado, sin sesión

[Task§121](../../specs/003-infrastructure/task.md)/[memoria§182](../../memoria.md),
2026-11-17UTC18:54:plugin1.2.896.0 instalado en ruta estándar/PEAMD64/versionexit0;
PATHpersistente ya contiene bin. Solo se actualiza PATH del proceso verificador
con esa carpeta para resolver hijo nativo por nombre/--version/exit0; no instalación
ni cambio persistente por agente. Consolas antiguas deben renovar su entorno.
AWSCLI2.40.1AMD64;perfilSTSesperadoPASS/documentoAmazonActiveSessionPASS/AOnlinePASS.
DescribeDocument deja de ser bloqueo observado; no se acreditan por ello permisos
StartSession/TerminateSession ni conectividad/TLS real. No sesiones/túneles abiertos.

Terraformlocal0/GitHubrepo0workflows-0runs en cinco estados e integridad278protegidos/
4artefactosPASS;29/29PASS históricos conservados sinretest/rebuild. El resto de este
checklist sigue sin completar: responsable/custodios/ventana≤900s/acuses mantenidos,
universo cerrado de timers/scripts/clientes/holders/sesiones, bloqueo verificable
global/T0/fuente temporal y recibos/cotaexhaustiva de drenaje/verificaciónfinal.
No nuevos RunCommand/DescribeSessions; snapshotsA/B anteriores no son actuales ni
barrera. **FASE 1 = NOT ACCREDITED; writers/inflight UNKNOWN; STOP antes de Fase 2.**

## 9. Acta preparada con designaciones y declaración expresa de Kevin

**Fuente:** mensaje del responsable en esta conversación, posterior al STOP por
falta de datos humanos. **Naturaleza:** declaración y designación explícitas, no
observación técnica, acta de inicio, autorización Fase2 ni evidencia de drenaje.
Véase [task§122](../../specs/003-infrastructure/task.md) y
[memoria§183](../../memoria.md). **FASE 1 = NOT ACCREDITED.**

Kevin Alexander Vega Zarza asume expresamente todos los roles enumerados en §1.
La acumulación de roles está declarada; no se exige aquí otra persona por iniciativa
del agente ni se afirma supervisión independiente. La designación de quien crea o
revoca no demuestra que el proceso técnico correspondiente esté listo o ejecutado.

### 9.1. Declaración recibida, sin extender su alcance

- No existen otros operadores humanos **autorizados concurrentes durante la ventana**.
  Se registra como acuse del único responsable declarado; no se esperan acuses de
  personas cuya existencia no está identificada. No equivale a inventario de todas
  las sesiones, credenciales, accesos técnicos o posibles actores no autorizados.
- No se autorizan Terraform apply, cambios manuales ElastiCache, deployments ni
  reinicios de servicios escritores.
- GitHub Actions/workflows activos/en cola:0 **según la evidencia ya registrada**;
  no es una lectura nueva ni un mecanismo contra futuros despachos.
- A/B **se mantendrán** sin runtime ENVER activo: compromiso para la ventana, no
  nueva observación ni prueba de bloqueo de arranques/despachos.
- Ventana máxima15min; instrucción expresa **NO INICIAR TODAVÍA**.
- Sin CONTROL/password real/asociación/túnel/Redis/inventory/G/deployment. Tampoco
  modificaciones IAM/ACL/red/lifecycle/Terraform/CI por esta preparación.

Las prohibiciones previas de cambios IAM relacionados, pipelines de infraestructura,
scripts capaces de tocar Redis y sesiones concurrentes no autorizadas siguen siendo
condiciones del freeze completo. No se presume que su cobertura técnica ni todas
las casillas de §1 estén acreditadas por esta declaración parcial.

### 9.2. Campos temporales y barrera

| Campo | Estado preparado |
| --- | --- |
| Inicio UTC / cierre máximo UTC | NO INICIADO / NO FIJADO; al autorizarse, cierre≤inicio+900s |
| Fuente UTC / sincronización / incertidumbre / monotónico | PENDIENTES; no escoger unilateralmente una fuente como acreditada |
| T0 | NO ESTABLECIDO; solo después de cobertura completa y último recibo de bloqueo verificable |
| Tf | NO REGISTRADO; exige verificación terminal de drenaje bajo barrera mantenida |
| consumers / writers activos / inflight | UNKNOWN; no reemplazar por0 a partir del compromiso humano |

### 9.3. Evidencia técnica restante

| Condición | Evidencia que falta | Estado | Responsable |
| --- | --- | --- | --- |
| Universo A/B | Resolver en ambos hosts unidad/servicio, ruta, usuario, comandos, configuración/includes/hooks y dependencias transitivas efectivas de fstrim, logrotate, refresh-policy-routes@ens5, sysstat-collect, sysstat-summary, systemd-tmpfiles-clean y update-motd; clasificar con prueba de acceso/capacidad, no por nombre | NO CERRADO; siete timers siguen UNKNOWN | Kevin Alexander Vega Zarza |
| Resto del universo | Cron/anacron/run-parts/at/jobs/unidades transitorias/servicios/contenedores/restart policies, scripts locales/WSL/otras estaciones, tests/helpers/admin SDK, otros CI/repos/workspaces/clientes y holders: registro completo, destinos y vías de acceso, disparadores y prueba de cierre | NO CERRADO | Kevin Alexander Vega Zarza |
| Freeze mantenido / consumers=0 | Observación actual más mecanismo verificable contra nuevos despachos/reinicios de todos los candidatos; cobertura de sesiones y automatizaciones, prohibiciones completas y futura ventana aceptada/supervisada | NO INICIADO / NO ACREDITADO | Kevin Alexander Vega Zarza |
| T0 / inflight | Fuente temporal acreditada, recibos de barrera sobre registro cerrado, respuestas terminales de operaciones previas o cota exhaustiva servidor/colas/red/retries, y recibo final con Tf y pendientes0 | NO ESTABLECIDO / UNKNOWN | Kevin Alexander Vega Zarza |
| Custodia técnica futura | Revisión del creador en RAM, transferencia exclusivamente por pipe restringido y supervisión real de abort/revocación/baseline; confirmar disponibilidad del procedimiento de incidente | ROLES DESIGNADOS; operación no acreditada | Kevin Alexander Vega Zarza |

Se conserva el protocolo previo: expiración AWS⇒STOP y escalado a Kevin sin
fallback/renovación automática; fallo de revocación, baseline no verificable o
exceso de plazo⇒incidente abierto, nunca revocación/cierre presumidos. En una
operación futura autorizada, Kevin será quien confirme revocación y baseline con
evidencia; la designación no sustituye esa evidencia ni autoriza alta ahora.

Esta entrega solo prepara el acta y reevalúa documentos. No llamadas AWS, auditoría
remota, nuevas pruebas, servicios detenidos o barreras aplicadas. No inicia el reloj
de900s ni produce T0/Tf. La auditoría técnica y cualquier enforcement adicional
requieren su ejecución posterior expresamente delimitada; no se infieren de la
solicitud documental actual. **STOP antes de iniciar ventana y antes de Fase2.**

## 10. Auditoría técnica autorizada después de aceptar §122

La solicitud posterior autoriza lecturas transitivas A/B/locales y preparar freeze,
T0/inflight, custodia y un checklist; **no aplicar controles ni iniciar ventana**.
§§2–9 conservan su evidencia histórica y sus límites originales. Kevin conserva
todos los roles; no falta nombrar otra persona. Registro: [task§123](../../specs/003-infrastructure/task.md)
y [memoria§184](../../memoria.md).

### 10.1. Recibos y límites reales de la inspección

Lectores independientes en el temporal aprobado
`C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-writer-transitive-20261117\`:
`audit-host.py`, `audit-followup.py`, `audit-leaves.py` y `run-audit.cjs`.
No se ejecutan los scripts inspeccionados: se ejecuta exclusivamente el lector
mediante `AWS-RunShellScript`; no StartSession, Redis, payloads de secretos,
argv/env generales ni logs del servidor. Los ficheros JSON de evidencia saneada
son `<instance-id>.json`, `<instance-id>-followup.json`, `<instance-id>-leaves.json`.
Se exportan líneas de código OS acotadas, con procedencia RPM y redacción; una
línea redactada, referencia dinámica o límite no resuelto sigue siendo una laguna.

| Lectura | Host | CommandId | Intervalo UTC del cliente, 2026-11-17 |
| --- | --- | --- | --- |
| Dependencias iniciales, 31 registros de archivo | A | 05e3f696-6d90-4bca-9cb6-83b60673deee | 19:08:00.486–19:08:12.118 |
| Dependencias iniciales, 31 registros de archivo | B | 60f6f3ab-377e-4a0a-a325-566c53f73cf4 | 19:08:12.121–19:08:23.362 |
| Configuración/hooks, 21 registros de archivo | A | 70d15ffb-a7e5-4d19-878a-09db422032b7 | 19:10:51.769–19:11:03.050 |
| Configuración/hooks, 21 registros de archivo | B | 1386afcc-9042-4aa4-964e-837722785164 | 19:11:03.054–19:11:14.739 |
| Hojas/configuración, 15 registros más 30 reglas tmpfiles | A | 5118c367-9777-4077-a729-eac706c1acc6 | 19:13:30.891–19:13:43.595 |
| Hojas/configuración, 15 registros más 30 reglas tmpfiles | B | 42a60968-a622-4de4-9716-99bfd86c4a55 | 19:13:43.598–19:13:55.087 |

Todos los recibos anteriores Success/ResponseCode0. Los conteos se solapan y
contienen referencias/ausencias/binarios: **no son una cuenta de archivos auditados
semánticamente hasta cierre**. El primer follow-up A,
`7fdca158-4b20-4ede-8d47-ecd062b07ce3`, devolvió OUTPUT_LIMIT; el controlador detuvo
esa colección sin consultar B. Se corrigió solo la presentación del lector:
compresión de evidencia ya saneada, límite descomprimido128KiB, nuevos recibos
arriba. No se considera aquel intento una auditoría completa ni un fallo de permisos.

STS 19:05:24.881–19:05:30.216 confirmó cuenta/ARN esperados; cada colección volvió
a comprobarlos con perfil exclusivo, intentos AWS1, sin fallback/renovación.
La primera lectura A/B observó contenedores Docker incluidos detenidos0,
jobs systemd0, atq0, listener8080=0 y TCP6379establecidas0. Son snapshots, no
consumers0 continuo, ausencia de trabajo servidor ni barrera.

`timedatectl show` devolvió NTP=yes/NTPSynchronized=yes en ambos hosts. Aun así,
el primer lector A informa 19:07:17.770–19:07:18.440 mientras su llamada cliente
ocurre 19:08:00.486–19:08:12.118: **relojes no acreditados como comparables**.
No convertir esta diferencia en un offset exacto ni Tlast/T0; falta incertidumbre
y procedencia efectiva del reloj del coordinador y de los hosts.

EC2 control-plane, sin probar conexión Redis:
- 19:17:38.351–19:17:42.716: SG backend `sg-070d06cfebe866427` asociado únicamente
  a ENI A `eni-01ecdecf22b187764` y B `eni-049a982368fb2bca8` en esa consulta regional.
- 19:17:42.720–19:17:46.967: SG cache `sg-0af65898948bd0578`, VPC
  `vpc-060f67e600affd569`, ingress TCP6379 solo desde SG backend, sin CIDR IPv4/IPv6.
- No prueba asociación actual exhaustiva del RG a todos sus SG, todos los caminos
  de red, otros recursos/regiones ni imposibilidad de adjuntar otra ENI después.
  No basta para concluir que un holder externo no pueda usar A/B como salto.

Windows, snapshot19:13:31.988: ningún proceso Terraform/dotnet/cliente Redis/plugin
en la proyección seleccionada; un aws.exe de la auditoría concurrente. Tareas
programadas: lectura correcta y0candidatas por filtro de acción; **no revisión
transitiva de todas las tareas, servicios Windows o wrappers de nombre neutro**.
WSL no arrancado/inspeccionado. GitHub19:22:55.581–19:22:55.596: relectura fallida
antes de obtener workflows; STOP de consultas GitHub, sin fallback. No atribuir
causa ni ceros actuales; los0workflows/0runs de §8 siguen siendo históricos.
No repetir DescribeSessions anteriormente denegado ni inferir permisos nuevos.

### 10.2. Registro de los siete timers en ambos hosts

Las siete unidades timer están activas. Servicios efectivos bajo **root por defecto**,
Restart=no, sin drop-ins ni Environment/EnvironmentFiles configurados en la lectura.
La tabla aplica separadamente a A y B según sus recibos; no se deduce del nombre.
SI/NO expresan capacidad acreditada del camino exacto; UNKNOWN no es NO.

| Nombre → qué ejecuta / ruta | Usuario | Dependencias/configuración reales seguidas | Alcanza Redis | Escribe cuotas | Clasificación / evidencia faltante |
| --- | --- | --- | --- | --- | --- |
| fstrim.timer → fstrim.service → `/usr/sbin/fstrim --listed-in /etc/fstab:/proc/self/mountinfo --verbose --quiet-unsupported` | root | Unidad y ELF util-linux; entradas de mounts, bibliotecas y comportamiento efectivo no cerrados | UNKNOWN | UNKNOWN | **UNKNOWN**. rpm -Vf del paquete no limpio; no atribuir ese resultado al binario individual ni llamarlo manipulado. Resolver las discrepancias y frontera de confianza/binarios/configuración |
| logrotate.timer → logrotate.service → `/usr/sbin/logrotate /etc/logrotate.conf` | root | include `/etc/logrotate.d`: btmp, chrony, dnf, psacct, sssd, wtmp. Hooks reales: chronyc cyclelogs; accton; lectura de pidfile y señal a sssd. RPM logrotate limpio | UNKNOWN | UNKNOWN | **UNKNOWN**. Seguir comportamiento de auxiliares/daemon receptores/configuración y carga efectiva; seis entradas no equivalen a cero hooks |
| refresh-policy-routes@ens5.timer → servicio homónimo → `/usr/bin/setup-policy-routes ens5 refresh` | root | `LIBDIR_OVERRIDE` o `/usr/share/amazon-ec2-net-utils/lib.sh`; se resolvió esta ruta real tras observar que `/lib/lib.sh` no existía. RPM amazon-ec2-net-utils limpio. Biblioteca consulta IMDS por curl, escribe configuración networkd, modifica rutas/altnames y puede ejecutar networkctl reload | UNKNOWN | UNKNOWN | **UNKNOWN** para capacidad total. Falta cerrar referencias dinámicas, configuración/rutas y auxiliares; no ausencia de acceso por falta de texto redis. Desactivarlo indiscriminadamente puede afectar SSM |
| sysstat-collect.timer → sysstat-collect.service → `/usr/lib64/sa/sa1 1 1` | root | sa1 carga `/etc/sysconfig/sysstat`, SA_DIR=/var/log/sa, SADC_OPTIONS=-S DISK, llama `/usr/lib64/sa/sadc`; RPM sysstat limpio | UNKNOWN | UNKNOWN | **UNKNOWN** hasta cerrar binario/loader/auxiliares y acceso efectivo. Se acredita ruta de métricas local, no arbitrariedad de configuración futura |
| sysstat-summary.timer → sysstat-summary.service → `/usr/lib64/sa/sa2 -A` | root | Mismo sysconfig; `/usr/bin/sar`, find/grep/xargs/rm, compresor xz; DELAY_RANGE=0, historial28d | UNKNOWN | UNKNOWN | **UNKNOWN**; seguir compresor/auxiliares/resolución efectiva. Es un script con subprocesos, no solo un nombre de métricas |
| systemd-tmpfiles-clean.timer → servicio → `systemd-tmpfiles --clean` | root | Ejecutable relativo efectivo; `/usr/bin/systemd-tmpfiles` RPM systemd limpio. 30 `.conf` en /run y /usr/lib; /etc sin entradas observadas. Proyección de tipos/path, sin argumento/dato | UNKNOWN | UNKNOWN | **UNKNOWN**. Falta resolución final del ejecutable, precedencia/overrides/reglas/directorios/symlinks; no exportar contenido que pueda ser secreto |
| update-motd.timer → update-motd.service → `/usr/sbin/update-motd` | root | Ejecuta scripts de `/etc/update-motd.d`; único observado `70-available-updates`: `/usr/bin/dnf updateinfo` y, condicionalmente, `dnf check-release-update`. Config dnf observada y nombres de plugins; timeout30s por subprocesso | UNKNOWN | UNKNOWN | **UNKNOWN**. Falta cerrar plugins Python/repos/helpers/carga efectiva. Ningún dnf ni hook fue ejecutado por esta auditoría; timeout no prueba inocuidad |

Lectura de texto/paquetes y grafo parcial mejora §9.3, pero no acredita toda la
semántica ni todos los accesos. No es obligatorio recompilar paquetes OS para
aceptarlos: se necesita una frontera de confianza explícita y evidencia verificable
de integridad, invocación, configuración y dependencias efectivas; hoy no está cerrada.

### 10.3. Resto del universo: capaces, no capaces y lagunas

| Elemento → ruta/usuario/ejecución | Dependencias | Alcanza Redis | Escribe cuotas | Clasificación / evidencia |
| --- | --- | --- | --- | --- |
| API / identidad runtime futura → Login/Register/Logout → `RedisAuthenticationQuotaStore` → `RedisAuthenticationQuotaScriptProtocol.ExecuteAsync` | RedisConnections, opciones, credencial/namespace, Lua aceptado | SI como capacidad de cliente; conectividad real no probada aquí | SI | **WRITER**; código local. Sin ejecución API en esta entrega; root A/B y holders del secreto runtime siguen siendo candidatos |
| Lifecycle → `Enver.API/Infrastructure/Redis/RedisAuthenticationQuotaLifecycle.cs` | Conexión y scripts de initialize/fence/maintain/activate | SI como capacidad | SI | **WRITER**, no ejecutar; ni su ausencia de proceso ni freeze IaC lo bloquean por sí solos |
| Harness → `tests/Shared/AuthenticationDependenciesFixture.cs` y scopes de integración/seguridad | Testcontainers.Redis; conexión del contenedor, namespace de RunId; CreateConfiguration permite overrides | SI al Redis de fixture | SI a su namespace; destino development fuera del camino fijo UNKNOWN | **WRITER** de fixture. Debe congelarse la ejecución y cerrarse cualquier override/helper alternativo, sin correr tests |
| CONTROL original → DLL fijada, modo remoto inventory | Allowlist RESP de lectura, apply remoto rechazado; identidad CONTROL futura | SI por túnel futuro | NO en camino auditado | **NON-WRITER de cuotas**; no confundir enumeración DB con aislamiento ACL ni permitir otros binarios |
| Productor → `CredentialProducer.RunInventoryAsync`, padre Windows | DLL/descriptor fijados, hijo inventory/pipe, no broker AWS | SI solo mediante hijo futuro | NO en camino auditado | **NON-WRITER de cuotas**; 29PASS históricos. No administra usuarios ni prueba custodia operativa |
| Bootstrap → `infra/bootstrap/compute-host.sh`, root al boot | dnf install docker/SSM; configuración Docker; enable docker/SSM | UNKNOWN por dependencias administrativas | UNKNOWN para universo completo | No inicia API ni lee secretos en su cuerpo; **UNKNOWN transitivo**, no NON-WRITER global. Prohibir reboot/rebootstrap/replacement durante freeze |
| Cron A/B → `/etc/crontab`, shell bash/root | Solo SHELL/PATH/MAILTO, sin líneas job observadas; directorios cron/spool sin entradas en lectura acotada | UNKNOWN para universo continuo | UNKNOWN | Archivo exacto sin despacho actual; **UNKNOWN** para inserciones, usuarios/directorios inaccesibles, anacron/run-parts y jobs futuros |
| Servicios/procesos persistentes A/B | 183 service unit files por host enumerados; procesos comm/uid, sin argv/env; docker/SSM/cloud-init/admins capaces de lanzar código | UNKNOWN | UNKNOWN | **UNKNOWN**: no revisión efectiva de todos los 183 servicios/activaciones, namespaces/red y cadenas de procesos. Un proceso Python/SSM observado puede ser el propio lector |
| Scripts administrativos/deployment temporales, usuario Windows | Familias `enver-acl-controlled-change-20261117`/`resume` → helpers/common.cjs y SDK/CLI; escaneo45archivos §120 | UNKNOWN | UNKNOWN; mutación ACL administrativa identificada | **UNKNOWN** hasta revisión semántica completa de todos los includes/destinos/ejecutables. No se reejecutaron helpers ni se convirtieron sus indicadores en evidencia de ejecución |
| Credenciales/holders y sesiones | Rol host con contrato CORE4/GetSecretValue, perfil administrativo y posibles sesiones/credenciales ya emitidas | UNKNOWN como cobertura total | UNKNOWN | **UNKNOWN**: registro efectivo de holders/roles/policies/estaciones/sesiones incompleto. No leer payloads ni tomar listado IAM como prueba de ausencia de copias |
| CI/repos/workspaces/Windows/WSL/otras estaciones | Snapshot local acotado y GitHub histórico; relectura GH fallida | UNKNOWN | UNKNOWN | **UNKNOWN**, no hay recibos de no despacho mantenido ni cobertura de otros repos/workspaces/runners |

**UNIVERSO NO CERRADO**. No se obtuvo una prueba exhaustiva de todos los holders
ni una auditoría efectiva de todos los servicios/jobs/wrappers. Ningún UNKNOWN
potencialmente escritor se excluye para declarar READY.

## 11. Freeze técnico: diseño reversible, NO aplicado ni finalizado

No hay hoy un mecanismo global listo para aplicar. La tabla especifica controles
candidatos y sus verificaciones/reversión; los identificadores/listas no conocidos
deben cerrarse antes de aceptar una receta ejecutable. No ejecutar ejemplos con
listas vacías, ampliar IAM unilateralmente ni usar prohibición humana como enforcement.

| Control | Recurso | Aplicación futura, solo con autorización | Verificación | Reversión | Riesgo / pendiente |
| --- | --- | --- | --- | --- | --- |
| FZ-1, exclusión de despachadores infra/admin | Todos los principals/sesiones/workspaces del registro cerrado | Puerta de despacho efectiva en cada CI/workspace; retirar temporalmente capacidad de mutación de ejecutores no exceptuados mediante control de acceso revisado. Debe cubrir ElastiCache, EC2/SSM/deployment, IAM/assume-role y cambios de la propia puerta; no solo terraform.exe | Export de política efectiva/puerta, ejecutores y colas vacíos, recibo por emisor, sin nuevo despacho; comprobaciones de políticas sin ejecutar mutaciones como prueba | Restaurar exactamente las políticas/estados anteriores solo tras cierre aceptado; no sobrescribir cambios ajenos | Inventario/principals/políticas/alcance y mecanismo de autoridad aún no cerrados. No existe una acción IAM «deny terraform apply»: hay que controlar sus APIs y otros caminos. Una identidad admin capaz de quitar el deny invalida una promesa de imposibilidad |
| FZ-2, antiactivación systemd | Lista cerrada de unidades writer y sus .timer/.socket/.path/servicios/transitorios | Tras guardar estado, `systemctl mask --runtime --now` solo unidades aprobadas; detener árbol/cgroup propio y resolver cada disparador | `systemctl show` LoadState/ActiveState/SubState/UnitFileState/ControlGroup; jobs0, árbol0 y sin activadores/aliases restantes. Mask no detiene script lanzado directamente | Unmask solo máscaras creadas en esta ventana; restaurar enable/active/restart exactamente según baseline y autorización, nunca arrancar API automáticamente | Lista exacta pendiente. No mask general de los7timers, docker, SSM, red, clock o ssh; preservar canal de observación/aborto. Root puede eludir masks: requiere FZ-1/FZ-5 |
| FZ-3, colas/jobs/scripts/harness | Cron/at/workers/runners/scheduled tasks identificados | Suspender despacho en el scheduler real y autorreinicio, con recibo por cola/runner y preservación de su estado; no borrar jobs ni matar procesos ajenos | Configuración y colas/retries/pending verificadas, receipt de bloqueo; todos los lanzadores cubiertos | Restaurar únicamente delta propio; liberar pendientes solo tras cierre explícito | Aún sin lista exhaustiva/mecanismo por ejecutor. Detener cron no cubre shells/SDK/tests manuales; stop cliente no cancela servidor |
| FZ-4, barrera de red de respaldo, candidata | Egress local y forward de A/B, todos los netns/puentes/rutas y otros clientes admitidos | Regla temporal dedicada que rechace TCP destino6379, IPv4/IPv6, tráfico nuevo y established, con precedencia efectiva; no flush global ni cambio SG/ACL runtime. Sintaxis concreta/backend/namespace/rollback deben revisarse antes | Ruleset/orden y cobertura OUTPUT/FORWARD/netns, counters y monitor de integridad; ninguna prueba conectando a Redis en esta preparación. No afirmar drenaje con counters0 | Retirar solo regla/tabla propia tras cierre seguro, comparar baseline; nunca una tarea que reabra red automáticamente al vencer900s | No instalada/verificada; nft/iptables/backend no acreditados. Puede afectar clientes y cualquier futuro túnel. No controla root, tráfico ya pasado, comandos aceptados ni otros puertos/proxies; requiere FZ-1/2/3/5. **No puede levantarse para Fase2 conservando una acreditación basada en ella**: requiere diseño/autorización posterior con aislamiento equivalente y nueva verificación |
| FZ-5, exclusividad administrativa/credenciales | Sesiones SSM/SSH/console, roles, estaciones/WSL y holders conocidos | Cubrir y cerrar/bloquear lanzamientos concurrentes no autorizados mediante mecanismo revisado; conservar solo observación/aborto de Kevin y evitar nuevas sesiones de mutación | Cobertura efectiva/recibos e inventario de sesiones por medio autorizado; perfil vigente y capacidad de reversión disponible. La denegación DescribeSessions previa sigue sin resolver | Restaurar exclusivamente acceso suspendido propio al cierre aceptado, sin renovar credenciales automáticamente | Permisos/sesiones/capacidad de bypass no acreditados. No leer secretos, renovar sesión ni terminar sesiones ajenas por inferencia |
| FZ-6, mantenimiento y reloj | Coordinador y recibos FZ-1..5 | Supervisor registra estado/cambios/deadline monotónico; ante pérdida de observabilidad STOP, no «mantener PASS» con snapshot viejo | Mismos controles activos hasta cierre; cambio de política/ruta/servicio/clock/holder ⇒ invalidar acreditación | Solo cierre documentado permite deshacer freeze; exceso de plazo ⇒ incidente y decisión humana | Watchdog/observabilidad no implementados/acreditados. Expiración de sesión no revoca recursos ni reabre cambios por sí sola |

FZ-4 no sustituye el bloqueo de nuevos **despachos** en el origen: una aplicación
puede encolar aunque su paquete sea rechazado. Tampoco SG ingress modificado,
retiro ALB, masks o ausencia de procesos aislados bastan. El diseño final debe
demostrar la conjunción de todos los controles aplicables al universo cerrado.
Sin listas/autoridad/recibos verificables, esta tabla es **diseño parcial, no freeze listo**.

## 12. T0, operaciones anteriores e instante Tf

**Método propuesto, no ejecutado ni completamente acreditable hoy.**

1. **Tiempo:** coordinador único Windows; UTC con fuente/última sincronía/estado,
   incertidumbre máxima verificable y relación con la fuente aceptadas por Kevin;
   más `Stopwatch.GetTimestamp/Frequency` del mismo proceso para duración. Lecturas
   candidatas previas: `w32tm /query /source`, `/status /verbose`, `/peers`; en hosts
   `chronyc tracking`, `chronyc sources -n` y estado del servicio. Sin ejecutarlas
   ahora ni cambiar clocks/NTP. Last offset o NTPSynchronized=yes solos no acotan
   error: hacen falta procedencia, dispersión/root-distance, antigüedad y cobertura
   de incertidumbre. No se fija unilateralmente una tolerancia como aprobada.
2. **Tfreeze:** justo antes de la primera acción efectiva del freeze, nunca al acabar
   de aplicarlo; registrar UTC±incertidumbre y ticks. Deadline monotónico=Tfreeze+900s,
   cierreUTC conservador≤inicio+900s incluyendo incertidumbre. Sin tiempo acreditado
   o sin autorización de inicio, STOP previo. No reiniciar el plazo por fallos.
3. **Acuses de bloqueo:** por cada entrada del registro cerrado: candidato/host/
   ejecutable/usuario, disparadores/reinicios bloqueados, configuración/evidencia,
   cola/retry/último ordinal de despacho, y hora de recepción del coordinador.
   Todo UNKNOWN o ausencia de recibo impide el siguiente paso. Mantener controles
   contra ejecución administrativa directa y modificación de las barreras.
4. **Consumers=0:** todos los procesos/árboles/cgroups/contenedores emisores ausentes
   o detenidos, jobs/colas no despachables, activadores/restarts bloqueados y
   holders/admins cubiertos. Corroborar con `docker ps -aq`, `systemctl list-jobs`,
   estado de cada unidad/cola y observación de procesos/sockets saneada. Ningún
   conteo puntual aislado satisface esta condición; cero colas locales tampoco
   prueba cero aceptado en servidor.
5. **T0:** acción del coordinador `registrar BarrierCommitted` **después** de recibir
   y validar el último acuse y volver a comprobar mantenimiento/cobertura y
   consumers0. Recibo: id de ventana/registro, conjunto completo de acuses,
   UTC±incertidumbre, ticks/frecuencia y resultado. No hay comando existente que
   implemente hoy ese recibo: debe prepararse/revisarse el coordinador o un
   procedimiento de registro verificable antes de READY. Es un instante posterior
   al cual ningún writer de ese universo puede despachar; no es hora de Docker,
   SCAN, Redis TIME, último request HTTP ni timestamp de un host sin sincronía.
6. **Drenaje de lo anterior:** para cada emisor, enumeración exhaustiva de operaciones
   despachadas hasta T0 y de buffers/queues/retries; reconciliar cada una con respuesta
   terminal atribuible de finalización o prueba de no despacho. Un error del cliente,
   cancelación, kill o cierre TCP no es respuesta terminal del servidor. «uncertain»
   mantiene pendiente; cualquier retry debe incluirse en el conjunto y bloquearse.
7. **Condición exacta de inflight=0:** cobertura exhaustiva del universo y todos sus
   despachos previos, ningún despacho posible desdeT0, buffers/retries/queues sin
   trabajo capaz de llegar después, y conjunto de operaciones sin finalización
   reconciliada **vacío**. Alternativa: cota contractual exhaustiva de última llegada,
   colas, red/buffers y ejecución servidor, más prueba de que se cumplen sus
   hipótesis y de agotamiento; documentar fórmula y procedencia, no inventar número.
8. **Tf:** tras esa reconciliación y nueva verificación del freeze/consumers0, el
   coordinador registra `DrainVerified` con UTC±incertidumbre/ticks, recibos terminales
   referenciados, pendientes0 y controles mantenidos. Tf≥T0. Solo entonces puede
   evaluarse FASE1 ACCREDITED y **STOP antes de Fase2**.

**No existe cota contractual demostrada en lo inspeccionado.** El código
`RedisAuthenticationQuotaScriptProtocol.cs:96–114` marca dispatched antes de
`ScriptEvaluateAsync(...).WaitAsync(...)` y devuelve uncertain si falla después.
OperationTimeout250ms u otro valor configura el cliente, no confirma cancelación
ni plazo final del servidor. lua-time-limit, socket timeout, ausencia de procesos,
espera900s o TTLs de cuota no son cota de red+buffers+colas+ejecución.

Falta localizar registros terminales exhaustivos existentes (sin secretos) y probar
que cubren también despachos históricos, o aportar contrato servidor/transporte
y evidencia de sus hipótesis. Una futura instrumentación solo hacia delante no
reconcilia retroactivamente lo desconocido. Sin una de esas vías, **STOP por
inflight UNKNOWN incluso si el resto del freeze queda preparado**. Este trabajo
no conecta a Redis para buscar evidencia ni autoriza comandos administrativos allí.

## 13. Custodia operativa futura — procedimiento de diseño, no broker implementado

Kevin es creador/custodio/operador/supervisor/revocador/verificador. La biblioteca
actual **no contiene generador, CreateUser, ModifyUserGroup, DeleteUser, cliente AWS,
supervisor global ni baseline verifier**. No hay un ejecutable creador/revocador
exacto acreditado; no inventar un comando CLI ni presentar el procedimiento como
implementado. Revisión estática confirma el transporte aceptado;29PASS siguen
siendo sintéticos, sin repetir suites/rebuilds.

### 13.1. Secuencia técnica requerida bajo autorización posterior separada

1. Revisar el proceso creador único Windowsx64, identidad AWS exclusiva, expiración
   acreditada con margen≥20min, integridad de binarios/host/permisos y custodia
   frente a debugger/dumps/EDR/pagefile/same-user/admin. Sin sandbox contra admin
   ni promesa de borrado absoluto; no cambiar esas configuraciones por esta lectura.
   Revisar antes la integración AWS **en el mismo proceso**, sin CLI para la password,
   logging de request, debug, trazas de SDK con cuerpo o serialización persistente.
2. Solo tras Fase2 autorizada y baseline actual: generación propuesta de32bytes
   criptográficos y codificación base64url sin padding (43ASCII válidos), en RAM
   del creador. No usar fixture ni generar ahora. Validar16..128bytes compatibles
   con el productor. La elección debe quedar revisada junto al creador real.
3. CreateUser una sola vez, ID/nombre exactos `enver-dev-quota-control`, engine
   y ACL read-only literal aprobados en task§113; no default, +@all o nuevos grants.
   Contraseña via petición HTTPS en memoria del proceso con credencial AWS explícita,
   sin fallback ni autorenovación. Si la integración obliga a strings/copies SDK,
   documentar ciclo de vida y borrado best-effort; nunca prometer que el CLR las borra.
   Guardar requestId saneado; si resultado es ambiguo, **no repetir CreateUser**:
   STOP y reconciliar existencia/propiedad del delta por lecturas autorizadas.
4. Esperar estadoactive con polling acotado al presupuesto de alta; verificar ID/
   nombre/engine/ACL/authmode sin imprimir password. Asociar solo mediante delta
   UserIdsToAdd=[CONTROL], grupo fijo, no reemplazo completo; verificar active,
   UserIds exactos runtime+CONTROL, RG/asociaciones/topología/runtime inalterados.
5. Solo con ruta autorizada posteriormente y primary/TLS supervisados, el mismo
   padre invoca `CredentialProducer.RunInventoryAsync(workspace, memory, token)`.
   Hijo fijado: `C:\Program Files\dotnet\dotnet.exe` →
   `<workspace>\tools\Enver.QuotaControl\bin\Release\net10.0\Enver.QuotaControl.dll`
   `inventory --descriptor <workspace>\tools\Enver.QuotaControl\descriptor.approved.json`
   `--transport ssm-tunnel --tunnel-port 16379 --credential-pipe <handle-real-heredado>`.
   Solo handle como argumento, nunca password. No ejecutar ahora ni incluirlo como
   paso del checklist Fase1. DLL/descriptor/productor hashes aceptados revalidados
   y reparse points rechazados; no recompilar CONTROL para «recuperar baseline».
6. Productor crea pipe anónimo no heredable, duplica únicamente lector y handles
   stdio necesarios en handle-list; crea child suspendido, job kill-on-close, cierra
   duplicados paternos/no usados antes de ResumeThread. Frame uint32 big-endian+
   bytes, escribe≤132bytes, borra frame y cierra escritor/EOF. Retiene memoria de
   entrada válida hasta completar; el creador borra su copia en finally. Stdout
   privado acotado/proyectado, stdin/stderr NUL, entorno mínimo sin AWS/secretos.
7. Cancelación/crash/child failure/deadline/inventory STOP: cerrar pipe/job propios,
   comprobar child terminado/handles cerrados, nunca ejecutar inventory otra vez
   ni pasar credencial a otra consola. Presupuesto45s+hasta10s de cleanup local;
   no garantiza tiempos de kernel ni revoca usuario AWS. InvalidOutput/Cleanup
   impiden declarar inventory completado. Guardar solo enums/receipt tipado.
8. En **PASS o STOP**, cerrar child y sesión/túnel propios si los hubiera, iniciar
   retirada del delta propio UserIdsToRemove=[CONTROL], confirmar propagación/estado
   y runtime exacto; después DeleteUser solo del usuario propio y comprobar ausencia
   mediante respuesta UserNotFound del recurso exacto. No borrar usuario preexistente
   de propiedad incierta ni tocar contraseña/ACL runtime.
9. Verificar baseline final contra recibo previo: RG/topología/SG/TLS/pending sin
   cambio, grupoactive/UserIds=[enver-dev-runtime], runtime mismo ACL/authmode/grupos/
   passwordcount y demás metadata no secreta, CONTROL ausente, procesos/handles/
   sesiones propios cerrados. No comparar ni leer payload/hash/fingerprint de secretos.
   Restaurar freeze solo por cierre autorizado, nunca por timeout ciego.
10. Expiración AWS: STOP/escalado a Kevin, sin fallback ni renovación automática;
    registrar qué recibos faltan. No afirmar revocación por borrar RAM o matar child.
    Si no se puede ejecutar/verificar retiro/eliminación/baseline con la autoridad
    disponible, queda **INCIDENTE ABIERTO**, igual que deadline excedido o cleanup
    fallido. Kevin decide recuperación bajo autorización posterior; no renombrar
    el incidente a «revocado por expiración» ni ampliar IAM automáticamente.

El presupuesto anterior de120+180+60+55+485=900s es una **reserva de diseño**,
no SLA AWS y no tiempo adicional a Fase1. Aplicar/verificar freeze y reconciliar
antes también consume la misma ventana. Debe recalcularse el itinerario real y
demostrarse margen de cierre **antes de READY**; no usar900s completos para la
preparación y luego iniciar otro reloj de cleanup.

### 13.2. Recibos saneados a conservar, nunca credenciales

Todos: id de ventana/paso, responsable, cuenta/región/recurso, UTC±incertidumbre y
monotónico, requestId/CommandId/SessionId/PID propios cuando corresponda, estado,
referencia a lectura/evidencia y enum de fallo. Sin cuerpos brutos ni keys/secretos.

| Hito | Recibo que lo acredita | Qué NO basta |
| --- | --- | --- |
| CREATED | CreateUser aceptado más DescribeUsers exacto active, engine/ACL/authmode aprobados, procedencia del delta propio | HTTP200 sin esperar estado; password en RAM; timeout de resultado ambiguo |
| ASSOCIATED | ModifyUserGroup aditivo aceptado + grupoactive con runtime+CONTROL exactos, RG asociado y sin pending/cambio inesperado | Solicitud enviada o pending |
| INVENTORY COMPLETED / STOPPED | Receipt tipado productor+exit+evidencia saneada de un único intento; en STOP conservar causa/terminación/cleanup y resultado desconocido cuando corresponda | Stdout crudo, éxito del proceso creador, child no confirmado terminado |
| REVOKED | Retiro exclusivo confirmado propagado y cierre verificado de todos los consumidores/conexiones propios; semántica efectiva de revocación de sesiones autenticadas acreditada por mecanismo documentado/validado separadamente | ModifyUserGroup devuelto/estadoactive solo acredita control-plane, no invalidación universal de sockets autenticados. Sin evidencia suficiente: REVOKED CONTROL-PLANE / DATA-PLANE NO ACREDITADO, no cierre final |
| DELETED | DeleteUser propio aceptado, seguido de UserNotFound para ID exacto y ninguna asociación residual observada | Borrar memoria, expiry AWS, usuariooff o petición pendiente |
| BASELINE RESTORED | Comparación explícita no secreta antes/después RG/grupo/runtime, CONTROLausente y recursos propios cerrados; recibos anteriores completos y plazo respetado | UserIds correctos aislados, ausencia de password local, reapertura automática de writers |

Hoy todos esos hitos operativos son **NO EJECUTADOS**; no hay delta que retirar.
Falta preparar/revisar creador-revocador/supervisor real y demostrar su custodia,
abort/autoridad vigente y baseline verification. No implementar esa integración
por inferencia de una solicitud de revisión y procedimiento.

## 14. Checklist secuencial único de Fase1 — bloqueado, NO ejecutar hoy

Es una receta condicionada para la futura ventana, no un script desplegado ni
un runbook listo mientras haya UNKNOWN/listas/mecanismos sin cerrar. Ningún paso
crea CONTROL/password/túnel ni toca Redis. Acciones de Fase2 de §13 son únicamente
revisión de diseño y **no forman parte de esta secuencia**.

| Paso | Comando / acción futura | Resultado esperado | Evidencia | STOP |
| --- | --- | --- | --- | --- |
| P1 PRE-FREEZE | Aceptar registro de §10 completo por candidato, reemplazar UNKNOWN y fijar listas/control exactos FZ-1..6, sus comandos y rollback; comprobar recibos históricos/drain contract y creador revisado sin ejecutarlo | Universo cerrado; método de drenaje y custodia listos | Registro/autoridad/mecanismos/versiones y procedencia completos | **Hoy STOP aquí**: universo incompleto, cota/terminales ausentes, broker/controles no preparados |
| P2 PRE-FREEZE | Revalidar `aws sts get-caller-identity --profile enver-admin-temp --region us-east-1 --no-cli-pager`; expiración por metadata no secreta; plugin --version/PE/resolución; DescribeDocument/SSMOnline y baseline AWS de solo lectura, integridad CONTROL/productor/descriptor sin rebuild | Identidad exacta, margen≥20min y medios de observar/abortar disponibles | Recibos saneados actuales, expiración acreditada y hashes de artefactos | Error/expired/denied/mismatch/sesión de cierre no disponible; no fallback/renovación ni StartSession como prueba |
| P3 PRE-FREEZE | Verificar reloj fuente/incertidumbre/monotónico según §12 y aceptar itinerario≤900s con reserva real de cierre; Kevin autoriza explícitamente iniciar | Inicio aún no realizado; acta final delimitada y tiempo verificable | Aceptación y recibos de tiempo/margen | Fuente incierta, falta autorización, presupuesto inviable; nunca backdate |
| F1 FREEZE | Registrar Tfreeze y deadline antes de primer control; aplicar únicamente FZ-1..6 previamente aprobados, en orden: puertas admin/CI → dispatchers/activadores/jobs → respaldo red si autorizado → verificación total | Nuevas ejecuciones/despachos impedidos, supervisión/aborto disponibles | Un recibo por control/candidato, baseline y delta reversible | Control fallido, nuevo actor/job/cambio, pérdida de observación o deadline; STOP y rollback seguro del delta autorizado, incidente si no puede cerrarse |
| W1 VERIFICAR WRITERS | `docker ps -aq`, `systemctl list-jobs --no-legend --no-pager`, `atq`, estado de cada unidad/cgroup/cola/runner y procesos/sockets saneados; contrastar con registro y barreras mantenidas | consumers0 y ninguna ruta de nuevo despacho/restart | Evidencia exhaustiva por emisor, no solo contadores | Cualquier consumer, UNKNOWN, acuse ausente, barrera evadible/no mantenida |
| B1 FIJAR T0 | Coordinador valida último acuse y registra BarrierCommitted como §12.5 | T0 posterior al cierre de todos los despachos | UTC±incertidumbre/ticks y conjunto íntegro de recibos | Tiempo o cobertura incompletos; no inventar timestamp ni usar snapshots |
| D1 ACREDITAR INFLIGHT0 | Reconciliar por emisor todo despacho previo y sus retries/buffers con terminales; alternativa solo cota exhaustiva acreditada y verificación de hipótesis | Pendientes sin reconciliar0 | Índice completo de despachos/terminales o contrato+cálculo+observación verificables | Un uncertain, operación sin historial, retry/cola abierta o falta de cota; esperar900s no cambia resultado |
| V1 FIJAR Tf | Volver a verificar controles/consumers0/pendientes0 y registrar DrainVerified en el coordinador | Tf≥T0, freeze mantenido, plazo vigente | Recibo §12.8 y estado actual de todos los controles | Cualquier cambio/laguna/plazo sin margen; invalidar acreditación |
| A1 FASE1 ACCREDITED → STOP | Solo si todos los pasos previos PASAN, Kevin acepta evidencia del incremento y se registra resultado real | Acreditación de esa ventana, **no autorización Fase2** | Acta/tiempos/recibos; seguimiento del deadline hasta cierre autorizado | Nunca continuar creando CONTROL; preservar el cierre seguro. Si Fase2 no se autoriza dentro del presupuesto, cerrar/revertir solo delta propio conforme plan aprobado; no mantener ventana indefinida ni reabrir con incidente pendiente |

Ni Tfreeze ni T0/Tf existen hoy. **READY TO START WINDOW** exige P1 completo y
capacidad acreditable para P2/P3/F1..A1, sin UNKNOWN operativos. Ejecutar controles
y generar recibos reales corresponde a una autorización posterior, no a este diseño.

## 15. Resultado de preparación

| Condición | Estado | Evidencia | Qué falta |
| --- | --- | --- | --- |
| Roles / autorización actual | FORMALIZADOS; §122 aceptado | Kevin único en los roles; autoriza auditoría/diseño, prohíbe inicio | No otro nombre; autorización final de ventana solo después de resolver técnicos |
| Universo writers | **NO CERRADO** | Seis lecturas A/B, rutas/usuarios/hooks reales §10; writers API/lifecycle/tests; SG/ENI observados | Cierre transitivo efectivo timers/servicios/auxiliares/dynamic refs, otros holders/sesiones/estaciones/CI; resolver todos los UNKNOWN |
| Freeze efectivo | **DISEÑO PARCIAL / NO APLICADO** | FZ-1..6 con verificación/reversión/riesgos | Registro cerrado, listas/comandos/principals/puertas y bypass bajo control; autorización separada para enforcement; compatibilidad futura de red/inventory no asumida |
| T0 / inflight | **MÉTODO CONDICIONAL; NO ACREDITABLE AÚN** | §12 y uncertain del código real; sin cota contractual demostrada | Fuente/uncertainty/registro verificables; terminales exhaustivos previos o cota servidor-red-colas-retries con hipótesis probadas |
| Custodia / revocación | **PRODUCTOR REVISADO; OPERACIÓN NO PREPARADA** | RAM→pipe/child fijado/cleanup estático; contrato de recibos §13 | Creador-revocador/supervisor real revisado y disponible, host/credenciales/abort/expiración/cierre/baseline acreditables, presupuesto viable |
| Checklist | PREPARADO / BLOQUEADO EN P1 | Secuencia única §14 termina en acreditación futura y STOP | Completar prerequisitos antes de convertir diseño a receta ejecutable autorizable |

**FASE 1 = NOT READY. NO ACCREDITED. Ventana NO INICIADA.** No solicitar inicio
como si bastara otra aceptación documental. CONTROL original intacto y no ejecutado;
productor sintético intacto; sin password real/usuario/membresía/túnel/Redis/inventory/
G/deployment/freeze/servicios deshabilitados/IAM-ACL-red-Terraform-workflows modificados/
commit/push. Estados funcionales y diferidos aceptados conservados en task§123.

## 16. §123 aceptado: cierre acotado, sin ampliar arquitectura ni iniciar ventana

### A. Clasificación y evidencia final disponible

**EVIDENCIA ACTUAL local, 2026-11-17UTC:**19:35:33 sin `.github`/workflows ni
otra configuración CI identificada; ninguna de las9refs locales tiene workflows;
un único worktree y backendTerraformlocal. No fetch ni consultas GitHub nuevas:
el fallo§123 no es bloqueo independiente y los0workflows/0runs de§121 son
**EVIDENCIA HISTÓRICA**. Se usa la declaración de cobertura humana de§122 sin
inventar otros repos, responsables o automatizaciones.

Windows19:36:09: sin procesos Terraform/dotnet/redis-cli/valkey-cli/plugin en la
proyección, tareas ENVERpor nombre0; hay procesos WSL. `wsl --list --running --quiet`
19:36:40 devuelve solo **docker-desktop**; Ubuntu registrado, no en ejecución.
No iniciar Ubuntu. EngineDockerlocal por namedpipe: intento por nombre no completó;
reintento con ruta instalada absoluta a19:38:26 terminó ETIMEDOUT tras20s. No
iniciar/reiniciar/parar Docker ni inferir contenedores0. Es una laguna concreta
del cliente local conocido, no evidencia de que esté escribiendo en AWS.

**AWS actual:**STS19:34:18.871–19:34:22.295 **ExpiredToken⇒STOPAWS**; sin llamadas
posteriores, renovación/fallback ni nuevas lecturas remotas. Los recibos A/B,
SG/ENI y sus comandos/configuraciones de§123 siguen **históricos**, no actualizados.
Se consultaron esos JSON como datos y se compararon las proyecciones A/B19:41:43;
coinciden scripts/configuración/hooks/RPM reseñados y reglas tmpfiles. La igualdad
de una proyección o de campos ausentes no completa una auditoría omitida.

Para NO/NO se clasifica el **camino de ejecución registrado**, no todos los programas
que root podría ejecutar después. Se acepta la semántica del componente OS de
procedencia conocida con invocación/configuración revisadas; no se pide demostrar
todo el SO ni se convierte privilegio root en escritura de cuotas. Un futuro
cliente arbitrario de root/holder se clasifica por separado. No/No no afirma
aislamiento de red del host. Se conservan las clasificaciones aceptadas de API,
lifecycle, CONTROL y productor; no se repiten suites ni auditorías de sus permisos.

| Elemento | Origen | Proceso/script final | Alcanza Redis | Escribe cuotas | Evidencia | Clasificación final disponible |
| --- | --- | --- | --- | --- | --- | --- |
| fstrim.timer A/B | systemd/root | fstrim.service→/usr/sbin/fstrim, argumentos de mounts§10.2 | NO en función estándar; binario efectivo no cerrado | NO en función estándar; binario efectivo no cerrado | HIST§123: unidad sin mismatch específico; rpm -Vf util-linux no limpio y sin detalle específico del ELF | **PENDIENTE**, no afirmar manipulación ni promover a NON-WRITER efectivo sin resolver la verificación del archivo |
| logrotate.timer A/B | systemd/root | logrotate.conf→6includes; hooks chronyc cyclelogs, accton /var/account/pacct, HUP a sssd/sssd_kcm; compresión de logs | NO | NO | HIST§123: código/config/hooks reales, paquetes logrotate/chrony/psacct/sssd-common verificados, sin hook de Redis ni arranque de ENVER | **NON-WRITER** en el camino registrado; no desactivar timer |
| refresh-policy-routes@ens5.timer A/B | systemd/root | setup-policy-routes ens5 refresh→amazon-ec2-net-utils/lib.sh→IMDS, networkd/rutas/altnames | NO; HTTP a IMDS no es Redis | NO | HIST§123: unidad/config sin override observado, biblioteca/paquete verificados y funciones reales | **NON-WRITER de cuotas**, administrativo de red; no desactivarlo |
| sysstat-collect.timer A/B | systemd/root | sa1 1 1→/etc/sysconfig/sysstat→sadc -S DISK /var/log/sa | NO | NO | HIST§123: script/sysconfig/paquete sysstat verificados; colecta local de métricas | **NON-WRITER** del camino registrado |
| sysstat-summary.timer A/B | systemd/root | sa2 -A→mismo sysconfig→sar/find/grep/xargs/rm/xz sobre /var/log/sa | NO | NO | HIST§123: cuerpo/config reales y paquete sysstat verificado; no cliente ni destino Redis | **NON-WRITER** del camino registrado; no exigir revisar todo código de compresores del SO |
| systemd-tmpfiles-clean.timer A/B | systemd/root | systemd-tmpfiles --clean con30reglas proyectadas /run y /usr/lib, sin override observado /etc | NO | NO | HIST§123: unidad y paquete systemd verificados; --clean actúa sobre filesystem, no ejecuta un cliente de cuotas | **NON-WRITER** del camino registrado, no implica que pueda borrarse cualquier ruta sin riesgo |
| update-motd.timer A/B | systemd/root | update-motd→70-available-updates→dnf updateinfo/check-release-update→plugins | NO en wrapper; plugins no cerrados | NO en wrapper; plugins no cerrados | HIST§123: wrapper y dnf.conf reales; solo nombres de copr/debuginfo-install/release-notification/supportinfo, no cuerpos/carga efectiva de plugins | **PENDIENTE** concreto: configuración habilitada y código/entrypoints efectivos de esos plugins |
| Cron/jobs A/B | crontab/root y schedulers observados | /etc/crontab soloSHELL/PATH/MAILTO; sin entradas cron/spool, atq/jobs0 | NO para el archivo sinjobs | NO para el archivo sinjobs | HIST§123 | **NON-WRITER del archivo/colas observados**, no permiso para insertar un job; no congelar schedulers vacíos por hipótesis |
| Harnesses integración/seguridad del checkout | tests/Shared y scopes | Testcontainers.Redis.GetConnectionString→RedisConnections; cambios/restarts usan de nuevo fixture.RedisConnectionString | SI al Redis de fixture; NO al destino AWS en los llamadores revisados | SI en fixture; NO en cuotas de developmentAWS | ACTUALsource: DistributedQuotaTestScope:82–92; RedisAuthenticationQuotaTestScope:32–40/86–94 y llamadores; SecurityScope:210–260; factory/fixture overrides locales, sin lectura de credencialAWS de Redis | **WRITER DE FIXTURE / NON-WRITER DEL DESTINO OPERATIVO** para este checkout; no descartar la capacidad de un código modificado que no se auditó |
| Helpers históricos de observación | temporales acl/quotas/readiness | validate-redis.py, observe-metadata.py, observe-first-use.py, precheck-host.py; wrappers SSM correspondientes | SI, mediante rolhost/credencialruntime en los observadores que autentican | NO en estos caminos | ACTUALlectura estática: allowlists AUTH/lecturas; precheck no arranca API; no ejecutarhelpers/payloadsahora | **NON-WRITER DE CUOTAS**, holders temporales de credencial con camino acotado |
| Helpers ACL change/resume | temporales administrativos | apply.cjs→common.cjs→ModifyUser y SSMvalidate-redis.py; preflight→observer | SI mediante hijo remoto | NO de datos por el camino revisado; SI mutaciónACL | ACTUALlectura de cuerpos/exports/allowlist y scripts finales; exclusiónfilewx no es permiso de rerun | **ADMINISTRATIVO MUTADOR**, congelar su lanzamiento; no inventar EVAL de cuotas dentro de estos helpers |
| Script de deployment existente | infra/bootstrap/compute-host.sh y readinessprecheck | bootstraphost instala/habilita Docker/SSM; precheck lee metadata/ECR/TLS, sin docker run | NO en bootstrap; SI en sondeoTLSreadiness | NO por sus cuerpos revisados | Source/HIST§123; no se encontró launcher de deployment API ya implementado que deba detenerse | **NO WRITER DE CUOTAS POR ESOS CAMINOS**; bootstrap/admin sigue prohibido durante ventana |
| API aceptada | Enver.API/composición | RedisAuthenticationQuotaStore→ScriptEvaluateAsync(evaluate) | SI | SI | Clasificación§120/123 conservada | **W1 WRITER**, sin runtime A/B observado en§123 |
| Lifecycle | Enver.API/herramientas que lo invoquen | initialize/fence/maintain/activate→mismo protocolo | SI | SI | Clasificación§120/123 conservada; no se inicia por reconnect | **W2 WRITER**, no ejecutarlo |
| Kevin/estaciónWindows y roleshostA/B | identidades/holders conocidos | SDK/CLI/código que use credencialruntime; SSM puede lanzar código en hosts | SI como capacidad conocida; perfil actual expirado | SI si despachan fuera de helpersreadonly | §122 y fuentes que obtienen credencial enRAM; inventario de sesionesA anteriormentedenegado, Bsinverificar | **W3 EMISORES ADMINISTRATIVOS CAPACES**; falta cierre de sesiones/procesos conocidos, no se presume otro humano |
| DockerDesktop local | estación/WSL ya activo | engine y contenedores no enumerables por timeoutlocal | NO ACREDITADO | NO ACREDITADO | ACTUAL19:36–38 | **PENDIENTE**; no sustituir por0 ni atribuirle uso de credencialesAWS sin evidencia |
| GitHub/CI/workspace conocido | repo local y declaraciónKevin | Sin workflow/configCI registrado en checkout/9refs; backendlocal, un worktree | NO hay emisor definido | NO hay emisor definido | ACTUALlocal19:35 y HISTGitHub§121 separados | **SIN WRITER CI IDENTIFICADO**; no bloqueo por refrescoGitHub ni infraestructura hipotética |
| CONTROL/productor exactos | artefactos aceptados | inventory fijado/RAM→pipe | SI solo en uso posterior | NO | Clasificación previa conservada, sin reauditar/rebuild | **NON-WRITER DE CUOTAS**, operativo no ejecutado |

No se obtiene todavía la lista exhaustiva libre de UNKNOWN: los pendientes se
exponen, no se renombran «SI conservador» para aparentar cierre.

### B. Lista de acciones futuras del freeze, limitada a los emisores identificados

Ninguna acción aplicada. Se retiran del itinerario actual los controles hipotéticos
de§11: denyIAM genérico, firewall6379, máscaras indiscriminadas y deshabilitar CI/
schedulers no existentes. No se propone un servicio `enver.service` inventado ni
se mata todo dotnet/node/python. **No hay aún una lista completa de comandos de
detención: no están identificados los procesos/sesiones pendientes.**

| Acción exacta futura | Recurso | Resultado esperado / verificación | Reversión |
| --- | --- | --- | --- |
| Kevin suspende toda invocación de `terraform apply`, SDK/CLI de mutaciónElastiCache/IAM, deployment/bootstrap/reboot, `node .../enver-acl-controlled-{change,resume}-20261117/apply.cjs`, launchers API/lifecycle y nuevos RunCommand/StartSession de ejecución ajenos al checklist | Único operador declarado, estaciónWindows y rolesA/B bajo su control | Acuse nominal ya designado más recibos actuales de no ejecución/colas/sesiones. Es una acción humana concreta, **no un deny técnico ya instalado** | Liberar solo tras cierre/rollback aprobado y baselineverificado; no iniciar API/tests automáticamente |
| `Get-CimInstance Win32_Process` proyectado sin argv/env; `wsl --list --running --quiet`; `C:\Program Files\Docker\Docker\resources\bin\docker.exe --host npipe:////./pipe/docker_engine ps --all --quiet` | Estación/WSL/Docker local | Sin emisorW1/W2/W3 activo ni contenedor con credencial/destino de cuotas; Ubuntu no iniciado. Si aparece candidato o timeout, STOP antes de fijarT0; no matar proceso ajeno | Lecturas sin delta que revertir |
| SSM `SendCommand` a A=i-0a66085b7b2411393 y B=i-00de11a3f3c1a8aa5, DocumentName=AWS-RunShellScript, comando `python3` con cuerpo exacto del lector `tools/Enver.QuotaControl.Operations/observe-host.py`; recoger GetCommandInvocation de cada CommandId | A/B; observación autorizada separadamente del inicio | ResponseCode0, contenedores0/procesosindicadores0/jobs0/atq0/listener8080=0/TCP6379=0 y sin lecturasfallidas. Esto corrobora ausencia, **no sustituye el cierre transitivo o las sesiones pendientes** | Solo lectores, no delta de servicios/dataset |
| Comprobar ausencia de sesiones/RunCommands administrativos concurrentes mediante medio de lectura autorizado y vigente; asociar cada sesión conocida a Kevin/propósito/terminación | W3/A/B | Recibo de cobertura; el AccessDenied histórico no equivale a0. No se proporciona un TerminateSession sin SessionId/propiedad acreditados | Lectura sin reversión; cualquier terminación futura requiere ID propio y alcance autorizados |
| Si aparece un proceso/contenedor/unidadwriter real, **STOP de preparación y registrar ID, propietario y activador**, antes de aprobar su acción de parada/reinicio | Candidato concreto, no nombre supuesto | Identificador y mecanismo de detención/reversión verificables | Se define con el ID real, no ejecutar kill/stop hipotético ahora |

La lista disponible contiene acciones exactas y puntos STOP, pero **no acredita
freeze ejecutable completo** mientras falten esos identificadores/cobertura. No
se afirma bloqueo técnico de todos los despachos a partir de la sola declaración.

### C. W_inflight: datos reales por writer, no Wsafe

`W_inflight=max(W1,W2,W3,otros emisores de cuotas que se confirmen)` solo puede
calcularse si cada término pertinente está acreditado o excluido por evidencia.
No se sustituye un término desconocido por0 ni se le asigna «infinito» como hecho.

| Writer | Última operación potencial | Timeout/retry/queue reales | Cota después del freeze | Falta exacta para cerrar |
| --- | --- | --- | --- | --- |
| W1 API/store | EVAL de evaluate ya enviado; lectura ZRANGE previa no consume cuotas | appsettingsbase: operación750ms/conexión2000ms, budgetsHTTP8000/8000/4000ms; runtimefinal no aplicado. RedisConnections: Sync/AsyncTimeout=opción, BacklogPolicy.FailFast, AbortOnConnectFail=false, ExponentialRetry de conexión. Store sin retry mutador de aplicación; protocoloWaitAsync/uncertain postdispatch | **NO EXISTE COTA MÁXIMA VERIFICABLE EN ESTE SOURCE/CONFIG** | Para cualquier despacho de este emisor que pudiera seguir pendiente: respuesta terminal/no despacho atribuible o cota de buffers/transporte/aceptación/cola/ejecución servidor. Los parámetros de cliente no la proporcionan. Si se acredita que nunca despachó contra este destino, excluirlo del conjunto real sin inventar espera |
| W2 lifecycle | EVAL initialize/fence/maintain/activate; activate puede encadenar lecturas/lotesmaintain antes del último EVAL | Mismo multiplexer/protocolo y presupuesto por dependencia; bucles/lotes tienen límites de cantidad, no SLA servidor; sin retry automático del EVAL en wrapper | **SIN COTA VERIFICABLE** | Identificar si hubo invocación contra este destino y su terminal; si la hubo y es uncertain, misma evidencia de finalización o contrato servidor/transporte. Un límite5000elementos no da segundos máximos |
| W3 holder/admin genérico | Último comando de escritura de su cliente concreto; no se inventa que emitió alguno | No existe un timeout/retry/queue universal de un holder; los helpersreadonly revisados no emiten cuotas y los helpersACL solo mutan control-plane | **NO CALCULABLE SIN IDENTIFICAR EMISOR/COMANDO** | Inventario de procesos/sesiones conocidos y del último despacho mutador, o recibo verificable de ningún despacho de cuotas por los caminos usados. No leer history/secretos ni presumir efectos por matar cliente |
| Harnesses/CONTROL/helpersreadonly del camino revisado | Ninguna escritura al destinooperativo | Presupuestos de tests 250/2000ms, Budget10000ms y harness5min son locales/fixture; productor45+10s no es plazo servidor | **NO APLICAN AL W DEL DESTINO**, no se agregan como una espera750ms/55s/5min | Clasificación de destino/allowlist anterior; no reclamar drenaje del fixture como drenajeAWS |
| fstrim efectivo, pluginsdnf y DockerDesktop pendientes | Aún no hay un comando de cuotas atribuido ni exclusión efectiva completa | Falta la parte precisa indicada enA | **NO ASIGNAR W** antes de cerrar capacidad | Resolver verificaciónfstrim, plugins habilitados/código y contenedores/procesos del enginelocal respectivamente; si no son writers, excluirlos, no inventar cotas |

**W_inflight = NO ACREDITADO.** Por tanto no se habilita ahora la secuencia
«esperar W_inflight→Tf→inflight0». Cuando cada término esté acreditado, el método
solicitado es: freeze completo y mantenido→writers0→T0 del coordinador después
del último recibo→espera monotónica de W_inflight demostrada desde eseT0→verificación
de hipótesis/colas/consumidores/conexiones/procesos y controles→Tf→inflight0.
Una cola/operaciónaceptada no se descarta solo con las observaciones finales.
Wsafe/horizontes/TTLs de cuota permanecen separados; no usar900s como W_inflight.

Tiempo actual19:42:55: `w32tm /query /status /verbose` devolvió
**0x80070426 / servicioWindowsTime no iniciado**. No se inició ni cambió reloj.
No hay fuenteUTC/incertidumbre acreditadas del coordinador; el monotónico sirve
para duración, no acredita por sí solo UTC o relación con los relojesA/B históricos.

### D. Checklist operativo CONTROL, acciones exactas, NO ejecutadas

Kevin conserva todos los roles. Esta lista **no autoriza Fase2**. No añadir pasos
de G/recuperación/lifecycle/deployment. No usar comandos CLI que transporten password.

| Paso | Acción futura exacta | Recibo / STOP |
| --- | --- | --- |
| C0 | Fase1acreditada en ventana vigente, autorizaciónFase2 separada; GetCallerIdentity perfilenver-admin-temp/us-east-1, metadata de expiración/margen, DescribeUsers/runtimeCONTROL, DescribeUserGroups/enver-dev-valkey, DescribeReplicationGroups/enver-dev-valkey, DescribeCacheClusters/enver-dev-valkey-001; integridad de artefactos y baseline actual | Sin autorización/identidad/margen, colisiónCONTROL/cambio/pending o freeze levantado: STOP antes de password. El STS actual expirado no satisfaceC0 |
| C1 | **En el proceso creador revisado**, generar la password futuraRAM según§123 y construir peticiónAPI CreateUser: UserId=UserName=enver-dev-quota-control, Engine=valkey, AccessStringliteral§113.3, AuthenticationMode.Type=password/Passwords=[RAM]. No CLI/ENV/file/clipboard/logging | CREATED requiere active+metadata exacta. Resultado ambiguo⇒reconciliar antes de cualquier repetición; no adoptar usuarioajeno. **El ejecutable creador todavía no existe: STOP de preparación aquí**, no fingir que RunInventoryAsync lo implementa |
| C2 | API ModifyUserGroup: UserGroupId=enver-dev-valkey, UserIdsToAdd=[enver-dev-quota-control], sin UserIdsToRemove/cambioengine; pollingDescribeUserGroups/Users/RG dentro del presupuesto | ASSOCIATED solo active, miembrosruntime+CONTROL exactos, RGtopología/TLS/pending sin cambio. Fallo⇒C5–C7 sobre delta propio |
| C3 | Mismo creador invoca `CredentialProducer.RunInventoryAsync(workspace, passwordRAM, token)` una sola vez; hijo/argumentos/handle-list fijados de§13.1; rutaSSM únicamente con autorizaciónposterior y sin alterar el freeze para eludirlo | INVENTORY COMPLETED/STOPPED: receipt tipado/exit/evidenciasaneada;45s+10scleanup reservados, no retry. Nada de stdoutcrudo/credenciales/keys |
| C4 ABORT | Cancelar token del hijo; disponer pipe/job propios; confirmar child terminado/handlescerrados; borrar bufferspropios enfinally; cerrar solo túnel/sesión propios si existieran | No equivale a revocarAWS. ExpiredToken⇒STOP/escaladoKevin sin fallback/renovación. Cleanupfallido⇒incidente, conservar causa saneada y continuar retirada solo con autoridad válida |
| C5 PASS o STOP | API ModifyUserGroup: UserGroupId=enver-dev-valkey, UserIdsToRemove=[enver-dev-quota-control], sin UserIdsToAdd/cambioengine; esperar grupoactive/sinpending/membresíaruntimeexacta | REVOKEDcontrol-plane con lectura de propagación; semántica de sesionesautenticadas/cierrepropio verificadas, no asumir revocacióndataplane solo poractive |
| C6 | API DeleteUser: UserId=enver-dev-quota-control **solo si creado en esta operación**; DescribeUsers exacto hasta UserNotFound dentro del plazo | DELETED y ausencia verificadas; permisoexpirado/fallo/propiedadincierta⇒incidenteabierto, no borrarotro usuario |
| C7 | Comparar baseline no secreto RG/grupo/runtime antes/después, UserIds=[enver-dev-runtime], mismo ACL/authmode/passwordcount/asociaciones, CONTROLausente, procesos/sesiones/handlespropios cerrados, plazo≤900s | BASELINE RESTORED exige todos los recibos. Fallo/excesoplazo⇒incidenteabierto; no liberarfreeze por timeout. Ningún payload/hash/fingerprint de secreto en evidencia |

Faltan el creador-revocador ejecutable y su disponibilidad de abort/cleanup;
las llamadasAPI exactas y la biblioteca revisada no los materializan. No implementar
ese programa ni elegir SDK nuevo bajo esta solicitud de convertir un checklist.

### E. Estado final y únicamente bloqueos concretos restantes

**FASE 1 = NOT READY. Ventana NO INICIADA. No ACCREDITED.**

1. **AWS ExpiredToken:** no se puede completar ahora la lectura específica de
   fstrim ni de pluginsdnf de A/B ni la cobertura autorizada de sesiones conocida
   pendiente. fstrim: detalle de verificación del ELF, no sospecha inventada de
   manipulación; dnf: código/entrypoints/configuración habilitada de los plugins
   ya enumerados. Reanudación remota requiere sesión válida proporcionada manualmente,
   no renovación/fallback del agente; la denegación anterior de sesiones no se
   presume resuelta por renovar credenciales.
2. **DockerDesktoplocal activo sin inventario:** el namedpipe no responde al
   `ps --all`; no se pueden atribuir/excluir procesos/contenedores del cliente
   conocido ni preparar su eventual parada exacta sin ese inventario. No autoriza
   pararDocker/Ubuntu ni otras cargas del usuario.
3. **W1/W2/W3 sin cota acreditada o exclusión/terminal completo**, uno por uno como
   tablaC; no hay W_inflight numérico legítimo. Los0snapshots no lo corrigen.
4. **Relojdelcoordinador:** WindowsTime no iniciado y sin fuenteUTC/incertidumbre
   equivalente acreditadas; no cambiar servicio/configuración por inferencia.
5. **Creador-revocador real ausente:** falta programa revisado que haga C1–C7 con
   custodiaRAM y aborto/revocación/baseline, no otra persona. La solicitud actual
   no lo implementa; perfil AWS hoy expirado tampoco acredita cierre disponible.

No se agrega como bloqueo refrescarGitHub, otra persona, CI hipotético, auditar
todos los183servicios por nombre, verificar formalmente todo el SO ni desplegar
nueva arquitectura de IAM/red. La lista de freeze **no se declara completa**
mientras los emisores concretos pendientes y su cierre no puedan identificarse.
Sin freeze/CONTROL/password/túnel/Redis/G/deployment/commit/push; sin cambios de
servicios/reloj/IAM/ACL/red/CI/Terraform ni código/artefactos aceptados.

## 17. Cierre acotado de los cinco pendientes; §124 aceptado

**Estado vigente: FASE 1 = NOT READY. Ventana NO INICIADA; no T0/Tf ni freeze.**
§16 es la entrega histórica aceptada; sus ExpiredToken y ausencia de ejecutable
no se presentan como hechos actuales. La presente autorización solo habilita
las lecturas y el programa local/sus simulaciones, no su ejecución contra AWS.

### A. fstrim: NON-WRITER de cuotas, rutas efectivas A/B

- `fstrim.timer`→`fstrim.service`; fragmentos en `/usr/lib/systemd/system/`;
  único drop-in del timer `fstrim.timer.d/99-amazon-fstrim-schedule.conf`:
  weekly/Persistent/RandomizedDelay6d/Accuracy1h, no programa adicional.
- Service sin drop-ins/ExecStartPre/ExecStartPost; usuario root implícito,
  Restart=no, inactivo en ambos. Ejecutable real `/usr/sbin/fstrim`:
  `--listed-in /etc/fstab:/proc/self/mountinfo --verbose --quiet-unsupported`.
- ELF util-linux **2.37.4-1.amzn2023.0.7 x86_64**,44824bytes, SHA256
  **d061b94101497e046ae4034d0709f8e08a5cbcc8cec088635d8364c31f2dff63**;
  digest **específico del archivo** RPM coincide en A/B. No deducir alteración
  de un fallo de verificación del paquete completo.
- Camino nativo de listado de montajes/FITRIM: no cliente de red/Redis,
  adquisición de su password ni ejecución de scripts desde fstab/mountinfo.
  No escribe cuotas. **No es un sandbox:** root conserva facultades ambientales
  de red/lectura privilegiada; no se afirma una prohibición kernel de esas acciones.

### B. update-motd: NON-WRITER de cuotas, rutas efectivas A/B

Timer→service→`/usr/sbin/update-motd`, root implícito, Restart=no, sin drop-ins
ni pre/post adicionales, inactivo en ambos. Se conserva el inventario de hooks
aceptado; no se ejecutó update-motd/DNF/plugin para auditarlo.

Camino seguido: wrapper (filtro ejecutables/no rpmnew)→hook efectivo
`/etc/update-motd.d/70-available-updates`→`/usr/bin/dnf --debuglevel 2 updateinfo`
y, si el RPM está presente, `dnf check-release-update`→Python→`dnf.cli.main`.
Los `timeout30s` limitan clientes del hook, **no se usan como cota Redis**.
MOTD head/tail se copian como datos, no se evalúan.

Config `/etc/dnf/dnf.conf` sin override de plugins/pluginpath/pluginconfpath;
directorio Python3.9 `dnf-plugins` y `/etc/dnf/plugins` inspeccionados. Se siguieron
`dnf/plugin.py` (`_get_plugins_files`, imports, `_check_enabled`, `_run_init`,
callbacks y `register_command`), los17módulos enumerados y la importación
`al_supportinfo`→`dnf_support_info_plugin.package_support`/su paquete.

| Código efectivo | Acción alcanzable en estas dos invocaciones |
| --- | --- |
| `debuginfo-install` | Registro de comando; enabled1/autoupdate0, no instalación |
| `generate_completion_cache` | Hook sack/transaction: SQLite `/var/cache/dnf/packages.db` y consultas de paquetes |
| `release_notification` | pre_config y comando check-release-update: sustituciones de URL, lectura XML de metadata vía base.urlopen/libdnf; texto de aviso. URL configurada HTTPS S3 AL2023/core/releasemd.xml; no usuario/query en URL; no ejecución del texto descargado |
| debug/repoclosure/repodiff/repograph/repomanage | Constructores registran comandos, no ejecutan sus acciones de administración |
| builddep/changelog/config_manager/copr/download/groups_manager/needs_restarting/reposync | Decoradores de registro; no se seleccionan esos comandos. Sus imports no son una orden de ejecutar builddep/reposync/etc. |
| al_supportinfo y paquete | Registro SupportInfoCommand y tipos/handlers de formato/archivo/HTTPS; ese comando no se selecciona |

Las rutas actuales sí pueden usar **red de repositorios/metadata** y ejecutar
los scripts Python descritos, escribir cachés/SQLite/texto MOTD y leer metadata
OS/RPM. No contienen en esas rutas llamadas Redis, EVAL ni recuperación de su
password. No se le atribuye ausencia de facultades root/IMDS por esta revisión;
la autoridad ambiental no cambia el propósito/código efectivo auditado. Tampoco
se convierte la confianza en librerías empaquetadas en verificación formal de todo
el SO o en garantía frente a reemplazar/modificar programas posteriormente.

### C. Docker Desktop local: inventario resuelto sin iniciar ni parar cargas

Proxy Windows bloqueado y socketLinux `/var/run/docker.sock` HTTP500 **no son
inventarios vacíos**. Se resolvió el mount namespace del dockerd activo
(PID149 observado) y su **`/run/docker.raw.sock`**, GET de API1.47 vía socat.
La distro docker-desktop ya estaba activa; Ubuntu no se inició.

Lecturas directas20:19:25–20:20:27, sellos del relojlocal no acreditado:
**33contenedores,0running,33exited, todosPID0.** GET inspect de los33 registra
imagen/digest/entrypoint/ensamblado, reinicio, redes, montajes y solo nombres de
variables/indicadores booleanos, sin exportar valores secretos. Sin ENVER,
endpoint operativo o credenciales AWS detectados en esas configuraciones/montajes.
No se leyó usersecrets ni tokens/certificados Kubernetes.

| Contenedores | Camino configurado / clasificación respecto de cuotas operativas ENVER |
| --- | --- |
| tickets.api / orders.api | ticketsapi→Tickets.API.dll / ordersapi→Orders.API.dll; proyecto eshop-services, restart unless-stopped; **NON-WRITER en el camino observado**, detenidos |
| auth.api / basket.api / catalog.api | authapi→Auth.API.dll / basketapi→Basket.API.dll / catalogapi→Catalog.API.dll; mismo proyecto, restartno; **NON-WRITER en el camino observado**, detenidos. Config Basket.ConnectionStrings__Redis apunta al servicio Docker `distributedcache`, comprobado por predicado sin exportar el valor, no al endpointAWS |
| sgum-api | sgum-backend-sgum-api→SGUM.Api.dll, proyecto sgum-backend/restartno; **NON-WRITER en el camino observado**, detenido |
| ordersdb/ticketsdb/authdb/basketdb/catalogdb | postgres15, datos locales, restartalways; **NON-WRITER** del destino operativo |
| sgum-postgres | postgres16, datos locales/restartno; **NON-WRITER** del destino operativo |
| distributedcache | redis:latest del proyecto eshop-services, servidor local/restartalways, no cliente ENVER; **NON-WRITER** del destino operativo |
| firebird5-engine/db-dessert | firebirdsql/firebird5.0.3-noble, montajes Firebird propios/restartalways; **NON-WRITER** del destino operativo |
| 18contenedores Kubernetes/infra local | Imágenes/entrypoints y montajes de pause, DNS, proxy, vpnkit, provisioner, control-plane y etcd; todos restartno/PID0, sin ENVER configurado; **NON-WRITER en el camino observado** |

Redes de aplicaciones `eshop-services_default`, `sgum-backend_default`,
`firebird5-engine_default`; infra también none/host. Montajes usersecrets/HTTPS de
otras apps y serviceaccounts/PKI Kubernetes no se confunden con credenciales ENVER
ni se abren sus payloads. Restart policies totales: always8/unless-stopped2/no23;
**no se cambiaron**. Ningún proceso de contenedor actual puede emitir: PID0.

Resultado del alcance conocido: **writers operativos identificados0 / UNKNOWN
activos0**, no exclusión eterna de contenedores modificados/arrancados después.
Se leyeron además, por GET archive sin arrancar ni exportar los binarios,
los seis DLL de entrada reales de `/app` (48640–123904bytes): hashes custodiados
en `docker-binaries125.json`, sin indicadores UTF8/UTF16 de Enver.API,
RedisAuthenticationQuotaStore/Lifecycle, namespace de cuotas o endpoint operativo.
Esto respalda la procedencia/configuración junto al estado detenido; **no es una
decompilación exhaustiva ni prueba aislada de incapacidad de todo código futuro**.
La barrera futura debe verificar de nuevo estos estados y conservar la prohibición
de arranques/restarts; no hace falta parar/deshabilitar otras cargas ya detenidas.
Esta clasificación no acredita drenaje de una operación histórica en otro servidor.

### D. W1/W2/W3: terminalidad real y barrera futura, no aplicada

Los snapshots A/B del lector actual muestran0containers,0jobs,0atq,
0listener8080 y0TCP6379 establecido. Windows20:20:31:0procesos seleccionados
(dotnet/API/CONTROL/lifecycle/Redis-cli/Valkey-cli/AWS/plugin/Terraform) y
0listener16379/conexión6379 pertinentes; no argv/ENV/history exportados.
Los workers SSM del lector no son evidencia de una sesión interactiva cerrada;
los RunCommand propios tienen recibos Success/exit0.

| Writer | Último intento real identificado / terminalidad |
| --- | --- |
| W1 API/store | No proceso/servicio operativo ENVER ni contenedor API en los caminos conocidos observados. Llamadores de tests ya trazados al fixture. **Ningún EVAL operacional pendiente identificado; último despacho/no-despacho operacional aún sin recibo atribuible completo.** No inventar un EVAL existente. Si se acredita exclusión de despacho, no exigir una espera artificial |
| W2 lifecycle | Invocadores revisados siguen siendo herramientas/tests fixture, sin proceso operativo encontrado. **Ningún intento initialize/fence/maintain/activate contra AWS identificado con resultado uncertain; falta cerrar la atribución operacional/no-despacho**, no fabricar una cola pendiente |
| W3 admin | Sin proceso administrativo local pertinente; helpers conocidos revisados son readonly o control-plane ACL, no mutación de cuotas. **DescribeSessions Active actual20:17:24 AccessDenied** impide la cobertura conocida de sesiones A/B/región. No se inventaron SessionIds ni se cerraron sesiones ajenas o desconocidas; sin nueva prueba de facultad de terminación |

Barrera preparada: **emisores capaces identificados detenidos + todos los
clientes/sesiones conocidos cerrados con recibo + ningún socket atribuible activo**,
mantenida durante la ventana futura. No se instaló ahora.

**¿Elimina por sí sola una cota temporal pendiente? No.** Una operación `EVAL`
ya aceptada (`evaluate` para W1; initialize/fence/maintain/activate para W2) puede
seguir ejecutándose en Valkey después de que cierre/cancele el cliente. Un comando
previamente en buffer/cola tampoco adquiere recibo terminal por observar sockets0.
Para W3 ni siquiera hay comando mutador concreto atribuido: no se presume uno.

Dos cierres legítimos, sin inventar un timeout: (1) evidencia de no-despacho/exclusión
de cada emisor real al destino o (2) respuesta terminal atribuible de cada intento.
Si realmente hay uncertain sin respuesta, hace falta observación externa autorizada
del servidor/cliente que pruebe finalización y ausencia de trabajo/buffers/colas de
esos emisores. Un TIME aislado o CloudWatch CurrConnections no identifica todo eso.
No se añaden ACL/grants/client commands, esperas900s, TTLs o controles hipotéticos
para fingirlo. **W_inflight sigue NO ACREDITADO**, distinto de Wsafe.

### E. Fuente temporal principal única seleccionada: A/chrony

**Amazon Time Sync169.254.169.123 vía chrony de A**, no WindowsTime ni AWS Date
sin incertidumbre. Se leyó tracking/sources, no se cambió configuración.
Calibración específica, **NO T0**, comandoSSM
`2afeda5a-64c1-4843-991a-b7bc80af81d0`:

- Fuente seleccionada `^*`,reach377,stratum4,leapNormal; último sample2s.
- SampleAUTC **2026-11-17T20:25:41.476977+00:00**, monotónicoA400918037914337ns.
- Systemoffset5.040µs, rootdelay419.184µs, dispersion303.468µs;
  offset+rootdelay/2+dispersion≈518.1µs. Margen razonable fuenteA **1ms** bajo
  estas condiciones observadas, no SLA formal NTP ni fuente criptográfica nueva.
- Bracket monotónico del coordinador alrededor de send→reciboSSM:
 1089058078029700..1089069413178400ns, duración **11.3351487s**. Sample dentro
 de ese intervalo; al asignarlo al punto medio, incertidumbre half-bracket5.66757435s
 +1ms +span de colección remoto15.563776ms: **≤±5.685s** conservadores.

Esto calibra UTC con un intervalo **sin suponer latencia simétrica**. No afirmar
precisión1ms del coordinador por la precisión aproximada de A. El reloj de pared
Windows muestra desplazamiento respecto de A y queda fuera de la autoridadT0;
sus sellos previos/locales solo ordenan registros en su dominio.

Reproducible: misma lectura dirigida con brackets monotónicos antes/después,
tracking/sources frescos, sampleUTC/monotónicoA y reciboSSM; rechazar falta de sync,
salto/suspensión, cambio de fuente o bracket que no quepa en el presupuesto. Para
la futura ventana se renueva la calibración y se usa el intervalo UTC conservador
y duración monotónica; no trasladar el sample histórico como T0, prolongar900s o
usar un reloj no acreditado para ignorar deadlines. FuenteA ausente/incierta⇒STOP,
sin fallback automático ni iniciar servicio WindowsTime.

### F. Creador / revocador CONTROL implementado localmente

[Programa y contrato](../Enver.QuotaControl.Orchestrator/README.md), con
`precheck/create-control/associate-control/run-inventory/revoke-control/delete-control/verify-baseline`
en una única sesiónRAM. .NET10/BCL, sin SDK/paquetes nuevos, AWS Query HTTPS/SigV4
fijo; perfil manual exclusivo sin cadena de credenciales ni renovación. No APIs
genéricas ni IDs/endpoint/ACL arbitrarios; no ModifyUser runtime ni datos Redis.

Artefactos originales/descriptor y productor cargado verificados antes del secreto;
password futura256bits/RAM→HTTPbodyRAM→pipe anónimo existente hacia consumidorfijo.
No string de password/argv/ENV/archivo/clipboard/log/history; recibos tipados saneados.
Colisión sin adopción; Create ambiguo⇒incidente; asociación fallida/inventorySTOP
obligan cleanup de lo propio; retirada/eliminación/baseline/plazo inciertos⇒incidente,
no revocación presumida. Watchdog/EOF/Ctrl+C/dispose usan cleanup; hard-kill/hostcaído
no garantizan finally y requieren supervisión/incidente de Kevin, no otro rol.

**28/28 PASS**, suite nueva exclusivamente local. Incluye los10casos exigidos,
idempotencia/colisión/ambiguos/snapshotrezagado/deadline, contratoHTTP/XML fijo,
caducidad sin nueva llamada, hashpreflight sin hijo y defaultSTOP sin leerperfil.
AWS real0/Redis0/passwordreal0/hijoCONTROL0 en suite. Referencias binarias: **no
recompilaron CONTROL/productor/API ni repitieron sus suites**. No prueba de IAM,
propagación/SLA AWS o revocación dataplane reales. Ejecución operativa prohibida
en esta entrega; el flag futuro no acredita freeze/T0/autorización por sí mismo.

### G. Cierre y únicos blockers restantes

**FASE 1 = NOT READY / NO ACCREDITED. Ventana NO INICIADA.**

1. **Cobertura administrativa actual:** DescribeSessions Active AccessDenied;
   faltan las sesiones A/B/región conocidas y sus cierres/terminales atribuibles
   mediante un mecanismo autorizado, sin cambiar permisos por iniciativa del agente.
2. **Terminalidad/no-despacho W1/W2/W3:** falta la atribución completa descrita
   enD; ningún EVAL operacional pendiente ha sido identificado, pero los snapshots
   y cierres de clientes no son sustitutos de esa evidencia. Resolver por exclusión
   verificable/terminal real, no por timeout o hipótesis de600/900s.

No hay nuevo bloqueo por renovar sesión, WindowsTime, Dockerproxy, creador ausente,
otro humano, refreshGitHub, CI desconocido o verificación formal de todoSO.
No freeze/CONTROLreal/passwordreal/Redis/G/deployment/commit-push; sin cambios de
servicios/IAM/ACL/red/workflows/Terraform ni arquitectura general nueva.

### Recibos y procedencia de §17

Directorio local aprobado `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-writer-transitive-20261117\`:

| Recibo | Identificación |
| --- | --- |
| A/B `*-close125.json` | SSM `c6bff81a-711f-4de5-951f-6209bccf4b84` / `2b39b508-9bed-4df3-a439-2042f1f6ccaa`; Success0, rutas/timers/ELF/procesos/sockets/chrony |
| A/B `*-plugins125.json` | `add5a682-2d12-429b-9539-0c65e6c8d620` / `7c02bb80-f4d2-48a6-a086-0b4c30a95850`; Success0, config/código/dispatch |
| A/B `*-hooks125.json` | `41cb569c-055e-4568-b5eb-d7c5e4516971` / `830c76e9-0041-4961-ad31-f2b0b98cf46a`; Success0, callbacks/imports efectivos |
| A `*-clock125.json` | `2afeda5a-64c1-4843-991a-b7bc80af81d0`; Success0, calibración, no T0 |
| `sessions125.json` / `windows125.json` | Denegación actual de DescribeSessions / snapshot procesos-sockets Windows |
| `docker125.json` / `docker-inspect125.json` | Lista rawsocket / metadata de los33contenedores |
| `docker-basket-target125.json` / `docker-binaries125.json` | Predicado destino Docker local / hashes e indicadores de seis DLL reales, sin contenido crudo |

Los intentos previos que devolvieron `OUTPUT_LIMIT` no son evidencia de cierre;
se redujo la salida y solo se aceptaron los recibos Success válidos de la tabla.
No confundir sellos del reloj local, UTC sincronizado A y evidencia histórica.
Las lecturas remotas no iniciaron timers/DNF/fstrim/Redis ni aplicaron cambios;
SSM transmite el script observador y deja sus recibos normales de auditoría.

## 18. SSM0 manual y distinción emisores/inflight

**Vigente: FASE 1 = NOT READY, único bloqueo de inflight operacional W1/W2.**
Esta continuación sustituye solo el pendiente SSM y afina la atribución W1/W2/W3;
§17 y su denegación se conservan como evidencia histórica, no bloqueo actual.

### Evidencia manual recibida y cobertura cerrada

El responsable informa STS PASS con perfil `enver-admin-temp` e identidad
`arn:aws:iam::926925605666:user/enver-terraform-local`, y la operación real
DescribeSessions Active PASS con **Sessions=[]**. Se registra la observación
manual del entorno del proyecto, **no una ejecución adicional del agente**;
sin timestamp exacto adjunto, no se fabrica uno. **Cobertura administrativa SSM
SATISFECHA PARA ESA OBSERVACIÓN**; no repetir DescribeSessions salvo contradicción
nueva. SimulatePrincipalPolicy/InvalidClientTokenId informado no es bloqueo ni
invalida el éxito de la API real. No ampliar IAM ni reabrir otros cierres.

### Dos afirmaciones distintas

- **NO ACTIVE EMITTERS: SATISFECHO en el alcance observado.** La evidencia
  acumulada identifica ausencia de API/lifecycle activos, contenedores detenidos
  con PID0, sockets pertinentes ausentes y ahora sesiones SSM0. Se conserva la
  procedencia/fecha de cada snapshot, sin fingir simultaneidad o monitoreo continuo.
  No se requiere investigar emisores hipotéticos o repetir auditorías cerradas.
- **NO COMMAND IN FLIGHT: NO ACREDITADO.** Ausencia actual de emisores no registra
  qué se despachó previamente ni su resultado. Un EVAL ya aceptado puede continuar
  después de cerrar su cliente. No se afirma que exista un comando pendiente real:
  **no hay ninguno identificado; falta evidencia de exclusión/terminalidad**.

### Evidencia mínima por camino, sin nuevos blockers

| Camino identificado | Disposición / mínimo que falta |
| --- | --- |
| W1 API→RedisAuthenticationQuotaStore→EVAL evaluate | **BLOCKED solo para inflight operacional.** Registro atribuible de ejecuciones/despliegues y destino efectivo que excluya despachos desde el API al endpoint operativo en el tramo no cubierto; **o** registro de sus despachos mutadores reales y terminal de cada intento relevante, sin uncertain restante. Un snapshot API0 no es ese registro. No volver a auditar tests fixture excluidos |
| W2 lifecycle→initialize/fence/maintain/activate | **BLOCKED solo para inflight operacional.** Registro atribuible de invocaciones y destinos con cero despachos operacionales en el tramo no cubierto; **o** terminales de los EVAL mutadores realmente enviados, incluidos lotes despachados, sin uncertain restante. Invocador0 no es prueba retrospectiva de invocaciones0 |
| W3 administrativo/control | **EXCLUIDO del inflight mutador de cuotas en los caminos conocidos:** helpers readonly/control-plane ACL ya revisados; CONTROL no ejecutado e inventory read-only. Con SSM0, su pendiente de cobertura queda cerrado. No inventar una operación Redis desconocida ni pedir una auditoría de otros holders hipotéticos. Solo un emisor/despacho contradictorio concreto reabriría esta atribución |

El tramo a cubrir es el posterior a la última frontera acreditada de inflight0;
si no existe, las ejecuciones posibles de estos caminos conocidos contra este
destino. Basta evidencia atribuible de esas ejecuciones/destinos/resultados, no
leer history con secretos ni verificar formalmente todo el SO. Los caminos
fixture/readonly/ACL ya excluidos no reciben una exigencia nueva de terminal Redis.

Si un registro identifica un despacho uncertain real, falta su terminal atribuible
o evidencia externa autorizada equivalente de que concluyó y no queda su trabajo
en buffers/colas. No lo sustituyen timeout, cancelación, cierre del cliente,
sesiones0/sockets0, TIME aislado, TTL, Wsafe o esperar900s. No se pide ahora conectar
Redis ni crear CONTROL para fabricar evidencia, y no se asigna una cota temporal
a un comando que ni siquiera se ha identificado como pendiente.

### Resultado

**FASE 1 = NOT READY. Único blocker técnico: acreditar inflight0/no-despacho
operacional W1/W2 mediante los registros/terminales anteriores.**
Sin ventana, freeze, T0/Tf, CONTROL/password real, Redis, G, deployment o commit/push.
En esta continuación solo se registra/reanaliza documentación; sin llamadas
AWS/SSM/IAM, cambio de código, compilaciones o pruebas. Se conservan el resto de
clasificaciones y prerrequisitos cerrados, sin ampliar el diseño.
