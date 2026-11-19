# Enver.QuotaControl.Bootstrap

**Modo `--preflight-only`: IMPLEMENTADO / VERIFICACIÓN LOCAL CON DOBLES / NO EJECUTADO CONTRA DEPENDENCIAS REALES.**

Herramienta administrativa separada con preflight diagnóstico no mutante y una ruta operacional existente para:

`RedisAuthenticationQuotaLifecycle.InitializeVerifiedNewEnvironmentAsync`

No modifica `Enver.API`, la imagen aceptada ni el cluster histórico. No implementa
`fence`, `maintain` ni `activate`. No genera G, no aplica Terraform, no despliega y no
acredita por sí sola que el ambiente sea jamás utilizado.

## Entrada

La credencial Redis llega **solo por stdin**, como JSON no persistido:

```json
{"username":"<usuario>","password":"<secreto>"}
```

No se aceptan `--password`, `--username`, `--secret`, `--connection-string`, `--token`
ni variables de entorno para la credencial. El endpoint, G, tabla y región se pasan
explícitamente; ninguno se descubre ni se genera. El comando exige
exactamente uno de `--execute-initialize` o `--preflight-only`, más `--secret-stdin`.

```text
--execute-initialize
--secret-stdin
--endpoint master.enver-dev-valkey-v2.<...>.cache.amazonaws.com
--generation <G-32-hex-externa>
--authority-table enver-dev-authentication-authority
--region us-east-1
[--profile <perfil-aws-no-secreto>]
[--timeout-seconds 5..300]
```

El endpoint debe ser el primary de `enver-dev-valkey-v2`; se rechazan el endpoint
histórico y cualquier otro replication group. La tabla y la región son exactas.
La G se valida pero nunca se crea.

## Modo diagnóstico `--preflight-only`

Usa los mismos argumentos anteriores, sustituyendo **únicamente** `--execute-initialize`
por `--preflight-only`. Los dos flags son excluyentes: combinarlos produce
`execution-mode-conflict` y exit 3; duplicar el flag también se rechaza. No hay fallback
al modo operacional si el diagnóstico falla.

La entrada valida argumentos y lee la credencial por stdin como antes, construye el
mismo `RedisBootstrapBackend` y retorna desde el runner diagnóstico **antes** de
`BootstrapService.ExecuteAsync`. El runner recibe solo un delegado a
`backend.PreflightAsync`, no el backend ni métodos de initialize/readback.
Ejecuta una sola vez el preflight real, con fases `local-guard`, `authority-guard`,
`redis-database`, `metadata-type`, `registry-type`, `keyspace`, sin cambiar guards,
timeouts, DB0, TLS ni selección de primary. No aplica G, no crea metadata/registry
y no muta DynamoDB. No inicializa, no hace readback operacional ni reintenta.

Salida: `PREFLIGHT-ONLY RESULT: AVAILABLE` si `ProbeSucceeded=true`, o `UNAVAILABLE`
en caso contrario, más únicamente `ProbeSucceeded`, `LocalReady`, `GlobalReady`,
`MetadataType`, `RegistryType`, `KeyspaceTotal`. `AVAILABLE` indica que se completó
la sonda, **no** que sea admisible: el exit code también exige guards abiertos,
metadata/registry `none` y keyspace total 0. Los tipos desconocidos se imprimen como
`unknown`, nunca como payload arbitrario.

Las excepciones internas conservan el diagnóstico saneado aprobado en stderr:
`phase`, `exception`, `inner`, `cancelled`, `category`, `code`. No se registran
mensajes, stack traces, conexión, credenciales, stdin ni respuestas completas.

### Exit codes

| Código | Significado |
| --- | --- |
| 0 | Diagnóstico completado y admisible; en modo operacional, initialize/readback satisfactorios. |
| 2 | Diagnóstico no disponible/no admisible; en modo operacional, `BOOTSTRAP STOPPED`. |
| 3 | Argumentos CLI inválidos o modos incompatibles. |
| 4 | Validación de entrada/configuración Bootstrap fallida. |
| 5 | Error inesperado de arranque/ejecución no convertido por el backend; salida genérica sin detalles sensibles. |

El modo diagnóstico solo puede contactar dependencias en una ejecución posterior
expresamente autorizada. Este incremento no autoriza publish, transferencia, EC2,
Redis/DynamoDB reales, lectura de secretos ni initialize. G operativa permanece
**APPROVED / NOT APPLIED**.

## Secuencia

1. Guard local Redis y authority Dynamo deben estar abiertos.
2. `TYPE metadata` y `TYPE registry` deben ser `none`.
3. `INFO keyspace` debe indicar cero keys.
4. `initialize` con C, I, policy `academic-initial`, siete reglas, `5000` y
   `67108864` exactos, guards reales y authority real; sin overrides.
5. Readback exacto: hash de 12 campos, snapshot, `generation=G`, `state=ready`,
   `count=0`, `horizon=0`, `dirty=0`, `version=1`, `fence=0`, `drain=0`,
   `last>0`, `maximumWindow=900000000`, `PTTL=-1`; registry `none`/`PTTL=-2`;
   keyspace exactamente una key.

Ante respuesta incierta, `dirty=1`, diferencia de readback o error de guard/authority:
**STOP**. No se limpia, corrige, reintenta ni repite initialize automáticamente.

## Límites

- No es recuperación del cluster histórico ni lo usa como rollback.
- No cierra DEPLOY-CONFIG-01, no aplica C/I/G y no satisface gates.
- Las pruebas locales usan dobles: no hay Redis, AWS, G real, Docker ni deployment.
- La suite conserva los 48 casos anteriores y añade cobertura del modo diagnóstico,
  despacho exclusivo, ausencia de initialize, redacción y grafo de llamadas del runner.
  Sus resultados locales no acreditan handshake TLS, certificado, ACL, Dynamo ni Valkey reales.
