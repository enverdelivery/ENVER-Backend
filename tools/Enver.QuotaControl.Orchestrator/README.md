# Creador / revocador CONTROL — preparación local

**IMPLEMENTADO / VERIFICADO LOCALMENTE / NO EJECUTADO CONTRA AWS.**
No acredita Fase 1, no inicia ventana y no autoriza Fase 2. Véase
[operaciones §17](../Enver.QuotaControl.Operations/README.md#17-cierre-acotado-de-los-cinco-pendientes-124-aceptado).

## Alcance fijo

Ejecutable .NET 10/BCL, sin paquetes nuevos, SDK, AWS CLI, credenciales por ENV,
cadena de proveedores, renovación ni fallback. El adaptador AWS usa HTTPS Query
regional con SigV4; **solo fue probado con HTTP simulado**, no contra AWS.
Lee exclusivamente el perfil manual `enver-admin-temp` de la ubicación estándar
del usuario, una vez por proceso, sin imprimir su contenido.

Cuenta `926925605666`, región `us-east-1`, principal
`arn:aws:iam::926925605666:user/enver-terraform-local`; recursos exactos en
`FixedResources`. No admite IDs, endpoints, ACL ni acciones arbitrarios.
Solo crea/elimina `enver-dev-quota-control` y añade/retira ese miembro del grupo
`enver-dev-valkey`. Nunca llama ModifyUser del runtime ni comandos de datos.
El ACL CONTROL es el literal read-only de task §113.3.

## Máquina de estados y custodia

Comandos admitidos **en un mismo proceso**, sin persistir/reanudar propiedad:

| Comando | Comportamiento |
| --- | --- |
| `precheck` | Plataforma/artefactos y perfil/identidad; metadata runtime, CONTROL, grupo, RG y cluster fijos; baseline estable, membresía exactamente runtime y CONTROL ausente |
| `create-control` | Revalidar baseline/artefactos; generar 256 bits en RAM, codificación Base64URL directa en bytes; CreateUser con cuerpo HTTP en RAM, nunca argv/ENV/archivo/string de password |
| `associate-control` | Añadir exclusivamente CONTROL y verificar membresía/propagación dentro del presupuesto; todo fallo entra en cleanup |
| `run-inventory` | Única invocación de PipeProducer existente; hijo CONTROL/descriptor/puerto16379 fijados por ese productor; siempre terminar en cleanup, tanto PASS como STOP |
| `revoke-control` | Converge en retirada, eliminación propia y verificación completa |
| `delete-control` | Igual convergencia; no omite retirada ni adopta un usuario ajeno |
| `verify-baseline` | Cleanup si falta; después de cleanup hace una lectura nueva, no reutiliza el recibo anterior |
| `abort` / EOF / Ctrl+C / watchdog | Cleanup de lo poseído; sin extensión implícita del plazo |

No se conserva password fuera de RAM ni se pide por consola. La entrada de consola
solo admite los comandos de la tabla. El secreto llega al consumidor fijo por el
pipe anónimo heredado restringido del productor aceptado. Sin clipboard, history,
stdout crudo de CONTROL ni logs de request/response/password/fingerprint.
Los recibos exportan solo enums, estado, causa saneada y duración monotónica.

Create ambiguo: **INCIDENT**, sin repetir/adoptar/borrar a ciegas. Asociación
intentada incierta exige retirada, incluso si una primera lectura omite el miembro.
Delete requiere recibo propio: una ausencia Describe anterior no lo sustituye.
Colisión, drift, errores de cleanup o sobrepasar el plazo: **INCIDENT/STOP**, no
revocación presumida. Expiración: sin nuevas llamadas de red, renovación o fallback;
si queda un delta propio no resuelto, incidente y escalado a Kevin.

El arranque sin los argumentos de autorización operativa explícita, remanente de
una ventana **ya autorizada** y expiración de credenciales devuelve STOP antes de
leer el perfil o usar red. El flag es una declaración humana, **no prueba técnica
de freeze/T0**. No se documenta una orden de lanzamiento operativo para esta entrega.
Un proceso nuevo no recupera ni elimina CONTROL preexistente.

Remanente máximo900s; reserva de cleanup485s, hijo45s+cleanup10s. Son presupuestos
de STOP, no SLA AWS ni cotas de EVAL. Debe descontarse el tiempo ya gastado por la
ventana; no reiniciar900s al lanzar el programa. El watchdog impide dejar la consola
esperando indefinidamente antes de cleanup. El túnel16379, freeze y supervisión de
sesiones pertenecen al procedimiento autorizado externo: este programa no abre
StartSession ni instala esos controles. Hard-kill/caída del host no permiten
garantizar `finally`: Kevin debe tratar la falta de recibos como incidente.
Zeroización best-effort, sin promesa de sandbox frente a un usuario privilegiado.

## Pruebas autorizadas, exclusivamente locales

```powershell
dotnet run --project tools/Enver.QuotaControl.Orchestrator.Tests/Enver.QuotaControl.Orchestrator.Tests.csproj --configuration Release
```

**28/28 PASS**. Éxito completo; Create fail; association fail; inventory STOP;
sesión expirada; revoke/delete fail; baseline mismatch; cleanup parcial;
secreto sintético ausente de recibos/argv/ENV y buffer borrado; colisiones,
idempotencia, dispose, comandos inválidos, deadline, errores ambiguos y snapshots
rezagados; cuerpo HTTP RAM/firma/campos fijos/XML/endpoint; expiración sin reintento;
verificación fresca; default STOP e integridad real de artefactos sin lanzar hijo.
AWS real0, Redis0, passwords reales0, consumidores CONTROL lanzados0.

Referencias binarias al productor aceptado; no ProjectReference al API, CONTROL
original o productor. No recompila ni repite sus suites aceptadas. El estado
operativo/remoto del adaptador y los permisos de mutación **no están acreditados
por estas pruebas**; su validación futura requiere autorización separada.
