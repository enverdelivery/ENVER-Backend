# Plan técnico 002 — Autenticación de ENVER Backend

## 1. Metadatos, estado y autorización

| Elemento | Valor |
| --- | --- |
| Solución / aplicación | `Enver.Backend` / `Enver.API` |
| Módulo | `002-authentication` |
| Versión documental | 0.2.0 |
| Especificación de entrada | [spec.md](spec.md), v0.3.0, **APROBADO PARA PLANIFICACIÓN** |
| Estado del plan | **APROBADO PARA DESCOMPOSICIÓN EN TAREAS** |
| Decisiones de planificación registradas | 25: PLD-AUTH-01 a PLD-AUTH-25 |
| Cobertura de diseño | AUTH-001 a AUTH-048; no equivale a implementación o cumplimiento |
| Autorización de esta entrega | APR-03: ajustar spec/plan y sincronizar AGENTS/memoria/README; sin tareas ni implementación |
| Estado técnico | Autenticación NO IMPLEMENTADA / NO VERIFICADA por esta entrega |

La fuente **SOL-PLAN-002** identifica en este documento la solicitud posterior del responsable que aprueba las 25 decisiones PLD-AUTH y autoriza crear este archivo. Es una fuente conversacional de aprobación documental, no un commit, una tarea ni evidencia de ejecución. Sus ajustes sustituyen las alternativas de la conversación de diseño solo en los términos expresamente indicados; no reforman implícitamente el spec.

La condición del responsable para el estado **APROBADO PARA DESCOMPOSICIÓN EN TAREAS** es ausencia de contradicciones con el spec. En v0.1.0 no se cumplía: AUTH-027 exigía una garantía de pérdida silenciosa superior al alcance de PLD-AUTH-08. **APR-03** aprueba y aplica el ajuste normativo mínimo al spec v0.3.0 y sincroniza este plan, resolviendo esa discrepancia sin cambiar arquitectura. El antecedente y la propuesta original se conservan íntegros como historial en §23.1. No se declara resuelto técnicamente el riesgo de infraestructura.

Las 25 decisiones quedan alineadas con la fuente funcional; candidatos y detalles diferidos conservan sus puertas de revisión/calibración. La aprobación documental de AUTH-027 no lo declara implementado o satisfecho ni atribuye aceptación técnica al conjunto. La pérdida completamente silenciosa no observable permanece como riesgo residual explícito; las señales disponibles no pueden ignorarse para clasificar un fallo como no observable.

No se genera `task.md`, código, proyectos de pruebas, paquetes, workflows o IaC. No se modifican `Program.cs`, configuración, Docker, constituciones, 001, perfiles de agentes o skills. No se realizan conexiones a MongoDB Atlas/Redis, compilaciones, pruebas, escaneos, instalaciones, commit o push. Esta aprobación solo permite crear documentalmente tareas en una entrega posterior delimitada; no autoriza implementación ni ejecución de esas tareas.

## 2. Fuentes normativas, antecedentes y estado observado

Orden de autoridad y función:

1. [Constitución canónica](../../.specify/memory/constitution.md), v1.0.0: gobierno, SDD, responsabilidades, seguridad y autorizaciones.
2. [Referencia raíz](../../constitution.md): acceso a la misma constitución, sin autoridad independiente.
3. [AGENTS.md](../../AGENTS.md): actuación de agentes; [memoria.md](../../memoria.md) y [README.md](../../README.md): estado e historial de entregas.
4. [Spec 002](spec.md), especialmente §§3–8 y 10: fuente normativa funcional; 48 AUTH, 21 AC y 14 AM conservados.
5. SOL-PLAN-002: decisiones de diseño de §21, subordinadas al spec vigente. APR-03: acto posterior del responsable que aprueba/aplica el ajuste normativo Redis y resuelve la discrepancia con PLD-AUTH-08. No sustituye retrospectivamente las fuentes anteriores; registro en memoria §15.

Antecedentes preservados: [spec 001](../001-backend-foundation/spec.md), [plan 001](../001-backend-foundation/plan.md) y [task 001](../001-backend-foundation/task.md). Sus 49 FND, 28 PLD, 38 TBF y 17 DEC conservan sus estados históricos. La implementación parcial de foundation no implica aceptación integral; ninguna relación con este plan cierra una TBF o aprueba retrospectivamente 001.

Base inspeccionada en el diseño: [Program.cs](../../Enver.API/Program.cs), [proyecto](../../Enver.API/Enver.API.csproj), [appsettings base](../../Enver.API/appsettings.json), [appsettings de desarrollo](../../Enver.API/appsettings.Development.json), [Dockerfile](../../Enver.API/Dockerfile), [.gitignore](../../.gitignore) y [.dockerignore](../../.dockerignore). Se observa `net10.0`, host inicial y `/health` básico; no integración de autenticación acreditada. Logging/AllowedHosts en appsettings no son secretos; el proyecto dispone de `UserSecretsId`, que no es una credencial. Dependencias de plantilla no son selección definitiva.

La higiene Git posterior consta en memoria §14. Las menciones anteriores del spec a artefactos versionados son antecedentes de su entrega, no un motivo para alterarlas aquí. En la creación v0.1.0 solo se autorizó este archivo y no se sincronizaron los documentos de estado. APR-03 autoriza ahora esa sincronización en AGENTS/memoria/README junto con spec/plan; las restricciones históricas no se reinterpretan como permisos de implementación ni impiden este acto documental posterior.

Las consultas de NuGet y documentación de la conversación anterior fueron investigación de solo lectura. No son instalación, auditoría de vulnerabilidades ni validación del grafo. No se renuevan verificaciones técnicas en esta entrega.

## 3. Arquitectura y responsabilidades

**PLD-AUTH-01/02.** Un único proyecto de aplicación `Enver.API`, con carpetas y namespaces delimitados. No crear `Enver.Application`, `Enver.Domain` o `Enver.Infrastructure` en esta fase. Crear posteriormente solo tres proyectos independientes de pruebas bajo `tests/`.

| Área | Responsabilidad | Restricción |
| --- | --- | --- |
| Controllers / Contracts | Transporte HTTP, contratos públicos y proyección de resultados | Sin drivers ni reglas de persistencia |
| Application | Coordinar register/login/logout y definir puertos específicos | Sin referencias a MVC, BSON o clientes Redis |
| Core | Identidad y estados mínimos | Sin infraestructura ni permisos funcionales inventados |
| Infrastructure | Adaptar MongoDB y Redis | Implementa puertos interiores |
| Security | Hashing, JWT, claves y controles de revocación/autorización | Sin criptografía propia ni Identity completo |
| Http | Límites, errores, correlación y coordinación HTTP | Sin duplicar la lógica funcional |
| Configuration | Options y validación | Nunca volcar secretos |
| Observability | Eventos y métricas acotados | Sin etiquetas de alta cardinalidad/PII |
| Composition | DI y configuración del pipeline | `Program.cs` no contiene lógica funcional |

Dependencias conceptuales: `Controllers → Application → Core / Ports`; Infrastructure y Security implementan los puertos. Composition conoce implementaciones para conectarlas. El flujo de ejecución hacia un adaptador no implica que Application dependa de su driver.

Se excluyen microservicios, CQRS, MediatR, AutoMapper, repositorios genéricos, UnitOfWork artificial y ASP.NET Core Identity completo. Las interfaces se crean solo para una frontera real o sustitución de pruebas. Separar ensamblados después requiere crecimiento/beneficio demostrado y autorización, no rellenar proyectos vacíos por patrón. Pruebas/revisión de dependencias comprobarán los límites internos.

## 4. Estructura objetivo

Árbol orientativo de archivos futuros; no creado por esta entrega. Puede simplificarse si una clase/interfaz no aporta valor, conservando responsabilidades y pruebas. No se añaden endpoints para llenar la estructura.

```text
Enver.API/
  Program.cs
  Enver.API.csproj
  appsettings.json
  appsettings.Development.json
  Dockerfile
  Controllers/
    AuthenticationController.cs
  Features/Authentication/
    Contracts/
      RegisterRequest.cs
      RegisterResponse.cs
      LoginRequest.cs
      LoginResponse.cs
    Application/
      AuthenticationService.cs
      AuthenticationInputs.cs
      AuthenticationResults.cs
      Ports/
        IUserStore.cs
        IPasswordService.cs
        IAccessTokenIssuer.cs
        ITokenRevocationStore.cs
        IAuthenticationQuotaStore.cs
        IAuthenticationSecurityState.cs
    Core/
      User.cs
      UserStatus.cs
    Validation/
      AuthenticationInputValidator.cs
      InstitutionalEmailNormalizer.cs
  Infrastructure/
    MongoDB/
      UserDocument.cs
      MongoUserStore.cs
      MongoUsersInitializer.cs
      MongoReadinessHealthCheck.cs
    Redis/
      RedisConnections.cs
      RedisTokenRevocationStore.cs
      RedisAuthenticationQuotaStore.cs
      RedisSecurityStateGuard.cs
      RedisReadinessHealthCheck.cs
      Scripts/
        RevokeToken.lua
        CheckRevocation.lua
        ConsumeAuthenticationQuota.lua
  Security/
    PasswordService.cs
    DummyPasswordHash.cs
    HashingConcurrencyGate.cs
    JwtTokenIssuer.cs
    JwtKeyRing.cs
    JwtBearerConfiguration.cs
    JwtClaimsValidator.cs
    RevocationAuthorizationHandler.cs
    AuthenticationPolicies.cs
    AuthenticationAuthorizationResultHandler.cs
  Http/
    RequestContextMiddleware.cs
    AuthenticationRequestLimitsMiddleware.cs
    AuthenticationValidationAndQuotaFilter.cs
    ApiProblemDetails.cs
    ApiExceptionHandler.cs
  Configuration/
    MongoDbOptions.cs
    RedisOptions.cs
    JwtOptions.cs
    AuthenticationOptions.cs
    RateLimitingOptions.cs
    AuthenticationOptionsValidation.cs
  Observability/
    AuthenticationEvents.cs
    AuthenticationMetrics.cs
  Composition/
    AuthenticationServiceCollectionExtensions.cs
tests/
  Shared/
    AuthenticationApiFactory.cs
    AuthenticationDependenciesFixture.cs
  Enver.API.UnitTests/
    Enver.API.UnitTests.csproj
    ValidationTests.cs
    AuthenticationServiceTests.cs
    PasswordServiceTests.cs
    JwtPolicyTests.cs
    RevocationRetentionTests.cs
    OptionsValidationTests.cs
  Enver.API.IntegrationTests/
    Enver.API.IntegrationTests.csproj
    RegistrationTests.cs
    LoginTests.cs
    LogoutTests.cs
    DistributedQuotaTests.cs
    DependencyFailureTests.cs
    ProblemDetailsTests.cs
  Enver.API.SecurityTests/
    Enver.API.SecurityTests.csproj
    MaliciousInputTests.cs
    TokenValidationTests.cs
    EnumerationResistanceTests.cs
    SensitiveDataExposureTests.cs
    RedisStateLossTests.cs
```

No se necesita `LogoutRequest`: no hay cuerpo funcional. Los modelos persistentes no se devuelven por HTTP. Evitar representaciones automáticas como `ToString()` que expongan contraseñas en contratos/inputs. Los auxiliares de pruebas pueden compartirse como fuentes sin crear un cuarto proyecto. Los scripts pequeños quedarían embebidos/versionados, no descargados dinámicamente.

No se incluyen colecciones, archivos ni puertos para un testigo MongoDB duplicado de revocaciones. `IAuthenticationSecurityState`/guard expresa la admisión y recuperación pendientes; su nombre no demuestra detectar pérdida silenciosa.

## 5. Dependencias y política de versiones

**PLD-AUTH-03.** Selección de paquetes aprobada; instalación y versiones efectivas pendientes de verificación/autorización técnica.

| Paquete | Tipo previsto | Uso | Riesgo / alternativa nativa |
| --- | --- | --- | --- |
| `MongoDB.Driver` | Directo, aplicación | Persistencia, BSON e índices | Revisar grafo DNS/compresión y compatibilidad; no cliente nativo ASP.NET equivalente |
| `StackExchange.Redis` | Directo, aplicación | Conexión reutilizable, comandos y Lua | Reconexión/colas/ACK incierto; `IDistributedCache` no cubre atomicidad requerida |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Directo, aplicación | Integración JWT mantenida | Alinear familia ASP.NET Core 10 e IdentityModel |
| `Microsoft.IdentityModel.JsonWebTokens` | Directo solo si el emisor usa explícitamente su API | Emisión JWT | No segundo stack; alinear con el grafo JwtBearer |
| `Microsoft.NET.Test.Sdk` | Directo, tres proyectos de pruebas | Descubrimiento/ejecución | Compatibilidad con SDK/runner por comprobar |
| `xunit` | Directo, tres proyectos de pruebas | Tests | Familia/versiones estables compatibles por comprobar |
| `xunit.runner.visualstudio` | Directo, pruebas; privado para consumidores | Runner | No llega al artefacto productivo |
| `Microsoft.AspNetCore.Mvc.Testing` | Directo, integración/seguridad | WebApplicationFactory | Alinear con ASP.NET Core de aplicación |
| `Testcontainers.MongoDb` | Directo, integración/seguridad | MongoDB real reproducible | Requiere Docker autorizado e imágenes revisadas |
| `Testcontainers.Redis` | Directo, integración/seguridad | Redis real reproducible | No acredita semántica del futuro servicio administrado |

Transitivas relevantes: `MongoDB.Bson` mediante driver; `Microsoft.IdentityModel.Tokens`/protocolos/JsonWebTokens mediante JwtBearer según versión; `Microsoft.AspNetCore.TestHost` mediante MVC Testing; `Testcontainers` mediante módulos y componentes xUnit. No añadir referencias redundantes ni forzar versiones transitivas sin justificar compatibilidad.

Las versiones consultadas anteriormente son únicamente candidatas de investigación, no aprobaciones ni certificados de seguridad. Antes de instalar: verificar compatibilidad .NET 10, estabilidad apropiada, vulnerabilidades directas/transitivas, licencias y grafo efectivo. Fijar después SDK/dependencias y restore reproducible con lockfiles bajo autorización. Las dependencias existentes de plantilla se conservan sin actualizarlas aquí; la corrección OpenAPI informada necesita aviso/informe verificable, no inferencia por número de versión.

Preferir capacidades nativas: PasswordHasher, Options, System.Text.Json, Problem Details, ILogger, LoggerMessage, Activity/Meter, TimeProvider y health checks. No añadir inicialmente FluentValidation, Serilog, Polly, AutoMapper, MediatR, Moq ni paquetes externos de Problem Details. No incorporar simultáneamente dos stacks de emisión JWT.

## 6. Configuración, secretos y presupuestos

**PLD-AUTH-04/18.** Usar `IOptions`, `IValidateOptions<T>` y `ValidateOnStart`.

| Options | Configuración no sensible | Configuración externa sensible |
| --- | --- | --- |
| `MongoDbOptions` | Base por ambiente, `users`, timeouts y política de índices/concerns | Connection string/credenciales |
| `RedisOptions` | Namespace, propósitos, timeouts, retención y política de admisión | Credenciales/conexión sensible y material TLS |
| `JwtOptions` | Issuer, audience, HS256, duración, skew, `kid` activo/permitidos | Material de signing keys por ambiente |
| `AuthenticationOptions` | Dominio, reglas de creación/verificación, límites HTTP/Unicode y concurrencia | No necesita secretos funcionales |
| `RateLimitingOptions` | Cuotas, ventanas, revisión de política y límites de cardinalidad | Clave HMAC exclusiva de pseudonimización |

`appsettings.json` y su variante Development solo contienen configuración no sensible. Desarrollo: User Secrets o variables de entorno. CI: secretos sintéticos/efímeros. Producción futura: gestor seguro AWS por seleccionar con mínimo privilegio. No crear `.env` con credenciales ni imprimir Options/conexiones completos. User Secrets no es una bóveda productiva ni prueba de cifrado local.

Validar antes de admitir tráfico: secciones obligatorias; issuer/audience/algoritmo; duración y skew aprobados; `kid` únicos y clave activa existente; claves HS256 aleatorias de al menos 32 bytes decodificados; ventanas y cuotas positivas; límites de payload/campos coherentes; aislamiento de ambiente; presupuestos finitos y configuración TLS aplicable. Longitud Base64 no demuestra entropía. Fallo de configuración impide inicio/admisión dependiente; fallo temporal de dependencia produce estado no apto/503, no credenciales falsas.

| Control | Candidato inicial configurable | Tratamiento |
| --- | --- | --- |
| Register/login total | 8 s | Presupuesto total, no SLA |
| Logout total | 4 s | Sin esperas ilimitadas |
| MongoDB, operación crítica | Hasta 2 s | Acotado además por presupuesto restante |
| Redis, operación crítica | Hasta 750 ms | Sin considerar timeout como ausencia de efecto |
| Inicialización/conexión | Hasta 2 s cuando corresponda | Distinguir operaciones lógicas y recuperación; calibrar |
| Hashing simultáneo por instancia | 2 | Sin cola ilimitada; saturación 503 |
| Margen operativo de revocación | 60 s | Candidato, no extensión de vida JWT |

Usar cancelación del request y presupuestos enlazados. PasswordHasher es síncrono: cancelar no interrumpe mágicamente un hash iniciado; limitar admisión/concurrencia y no lanzar `Task.Run` ilimitados. Los percentiles candidatos del spec §3.6 no son estos timeouts, SLA o gates medidos. Calibrar sin reducir seguridad del hash.

No recargar claves, skew o cuotas independientemente por instancia. Versionar/coordinarlas para no reiniciar límites o dejar tokens aceptables sin revocación retenida; protocolo operativo y límites numéricos de cardinalidad pendientes de §23.

## 7. Persistencia MongoDB e inicialización

**PLD-AUTH-05/06.** MongoClient singleton. Colección lógica `users`; sin datos de repartidor ni atributos extra:

| Campo | Representación / regla |
| --- | --- |
| `_id` | UUIDv4 backend; BSON Binary subtype 4, `GuidRepresentation.Standard`; API/JWT UUID canónico |
| `fullName` | String validado; conserva case/acentos/espacios internos |
| `normalizedEmail` | String normalizado único; sin duplicar correo original |
| `passwordHash` | Salida íntegra de PasswordHasher; nunca contrato público |
| `status` | `Active` / `Disabled`; inicial Active, asignado por servidor |
| `createdAt` | UTC del servidor |
| `updatedAt` | UTC; igual a createdAt al crear; login normal no escribe, rehash persistido sí |

BSON DateTime conserva precisión de milisegundos; usar el mismo instante para ambos timestamps al crear. No introducir `hashVersion`, `lastLoginAt`, roles, tokens, flags de correo verificado o campos de control no requeridos.

Índice `normalizedEmail` ascendente, único, collation simple/binaria, no sparse ni parcial. `_id` usa su índice propio, sin duplicarlo. La representación UUID estándar evita legacy; el detalle de serializador se adapta a la API efectiva sin cambiar la garantía.

Inicializador idempotente: comprobar colección e índice, crearlo cuando proceda y verificar clave/unique/collation/opciones, no solo su nombre. Duplicados o configuración incompatible bloquean registro; no borrar, reparar o reconciliar datos automáticamente. La identidad de inicialización separada de la aplicación de mínimo privilegio se define en infraestructura. No seleccionar sharding ni presuponer unicidad global en una topología no diseñada.

WriteConcern `majority` propuesto; ReadPreference `primary` para autenticación y ReadConcern `majority` cuando corresponda a la garantía de lectura/creación confirmada. Concretar journaling, confirmación física, topología y APIs/timeouts efectivos antes de aceptar persistencia; no reemplazar por escritura sin ACK o lectura obsoleta. Una creación confirmada debe ser observable por login según esas garantías.

Registro: validar/cuotas antes del hash; generar UUID y un hash; insertar; 201 solo tras creación confirmada. El índice, no consultar-antes-de-insertar, garantiza concurrencia. Duplicate key identificado sobre normalizedEmail → 409 `registration_conflict` genérico; no convertir cualquier duplicate key/error en conflicto de email. Escritura incierta → error operativo seguro, sin afirmar ausencia de creación, borrar compensatoriamente o reintentar con otra identidad.

Timeout/cancelación por operación y presupuesto restante; no reintentos de aplicación ciegos. El retryable write nativo, si se utiliza, se evalúa y acota según versión/topología; no se superponen capas de reintentos ni se promete exactamente una vez. Login consulta con filtro de igualdad construido por backend, sin documentos/operadores del cliente y sin caché de credenciales.

## 8. Redis, revocación, confianza y recuperación

**PLD-AUTH-07/08/09.** Redis compartido entre instancias ENVER; ConnectionMultiplexer reutilizable/singleton por destino necesario. No abrir conexiones por request ni exigir dos instancias físicas Redis. Separación conceptual por ambiente y propósito: `auth:revocation`, `auth:ratelimit`, `cache` futuro. Esos propósitos no cambian la clave conceptual aprobada de revocación:

```text
enver:{environment}:auth:revoked:{issuer}:{jti}
```

Las llaves son placeholders de componentes, no una decisión de hash slots/Redis Cluster. El jti debe estar autenticado antes de derivar la clave. No almacenar JWT completo ni PII innecesaria.

El almacenamiento de revocaciones debe aplicar `noeviction`; namespaces y bases lógicas no aíslan por sí solos memoria/expulsión. Si comparten proceso, la política de memoria protege también las revocaciones de presión de cuotas/caché; la falta de capacidad se traduce en fallo seguro, nunca omisión de marca. Monitoreo/capacidad, separación física y servicio administrado se evalúan después según coste, disponibilidad, Free Tier/créditos y evidencia. No se habilita aquí caché descartable que compita con seguridad.

Diseñar ACL mínimas, TLS aplicable, lecturas del primario y acceso administrativo restringido. No fire-and-forget, réplicas obsoletas ni cachés locales negativas. El backlog durante desconexión se deshabilita o limita para evitar efectos tardíos fuera del presupuesto; el detalle depende del cliente instalado. Un timeout de espera no garantiza que un comando enviado no haya ejecutado.

### 8.1. Confirmación y TTL

Retención candidata: `retainUntil = exp + skewEfectivo + margenOperativo`; inicial skew 30 s y margen configurable 60 s. TTL positivo suficiente hasta esa fecha, precisión en milisegundos y redondeo conservador; la retención adicional no extiende la vigencia JWT.

Operación atómica pequeña para crear/verificar marca y conservar la mayor expiración absoluta entre la existente y la requerida. No reducir TTL al repetir logout, crear marcas eternas accidentalmente ni confiar en exp/jti de un token no validado. Verificar la respuesta/resultado de la operación antes de 204. Un read-back o ACK no demuestra por sí mismo durabilidad frente a pérdida/failover; `WAIT` tampoco convierte Redis en un almacén de consistencia fuerte.

Primero y repetido: JWT auténtico temporalmente válido, marca confirmada y cuota disponible → 204. Ausente/falso/expirado → 401. Lectura/escritura incierta o estado requerido no verificable → 503. Repetición tras respuesta perdida puede confirmar efecto si vuelve a comprobarse. No se cancelan retroactivamente solicitudes que ya superaron el control de autorización.

### 8.2. Fallos y admisión

| Situación | Respuesta / garantía exigida |
| --- | --- |
| Redis indisponible o lectura incierta | 503, no asumir ausencia de revocación |
| Escritura incierta de logout | No 204; puede haber ocurrido efecto, sin afirmarlo como confirmado |
| Cuotas no verificables | Sin register/login/logout eludiendo controles; 503, no fallback local |
| Falta de memoria/capacidad | Rechazo operativo seguro; no expulsar marca vigente ni saltar escritura |
| Pérdida detectada o confianza incierta | No admisión compartida de accesos dependientes y freno de emisión cuando corresponda |
| Pérdida catastrófica silenciosa no observable | Riesgo residual explícito conforme AUTH-027 / APR-03; no eliminado ni convertido en aceptación técnica |

No introducir almacenamiento duradero duplicado de revocaciones en MongoDB ni transacciones distribuidas. Logout no depende de MongoDB para mantener un testigo. La alternativa del diseño conversacional con testigo externo queda expresamente descartada para 002, no diferida como tarea implícita.

### 8.3. Recuperación diseñada y límites no resueltos

Ante pérdida DETECTADA de confianza: activar no admisión compartida; detener coordinadamente emisión cuando sea necesario; restaurar estado confiable validado o aplicar cuarentena que cubra máxima vida restante. Identificar el último instante posible de emisión, instancias en vuelo y cambios de reloj/configuración. Con candidatos iniciales, un horizonte conservador de 900 + 30 + 60 = 990 s desde la última emisión posible puede servir para dimensionar; no empieza automáticamente en la primera alerta ni es RTO aprobado.

Si no se puede demostrar cese de emisión/seguridad de reapertura, mantener no admisión. Retirar readiness o cerrar un flag local no demuestra coordinación global. No guardar el único indicador de confianza en el mismo estado cuya pérdida pretende detectar. Definir posteriormente fuentes de señales y control operativo confiable; no afirmar que se detectan todos los borrados mediante eventos de conexión, un ping, persistencia o un marcador.

Reinicio, flush, restauración antigua y failover con pérdida deben evaluarse con el modelo de fallos aprobado. Las pérdidas detectables se prueban con cierre y recuperación; documentar qué componente detecta cada evento, sus límites y propagación. Inyectar solo una señal de estado no confiable prueba la reacción, no demuestra por sí solo el detector. El caso completamente silencioso no observable se evalúa/documenta como riesgo residual, sin afirmar detección universal o aprobarlo por omisión. Detección/fencing/continuidad y ACK/lecturas del servicio elegido siguen pendientes de infraestructura para aceptación técnica; no se afirma AUTH-022/027 satisfecho.

Para contadores perdidos: reconstrucción confiable o no admisión durante la ventana restante aplicable; no resetear cuotas silenciosamente. Recuperación de revocaciones y de cuotas son garantías distintas. APR-03 resuelve el ajuste normativo de §23.1 y permite descomposición documental, no demuestra una solución técnica ni amplía el alcance para añadir almacenamiento duplicado.

## 9. JWT, claves y autorización

**PLD-AUTH-10/11.** Backend ENVER emite HS256, issuer `enver-api`, audience `enver-mobile`, vida inicial 900 s y skew inicial 30 s, máximo 30 s. Solo emitir tras credenciales válidas y todos los controles requeridos disponibles.

| Claim/header | Tratamiento |
| --- | --- |
| `sub` | UUID canónico del usuario, sin email |
| `jti` | 32 bytes de RNG criptográfico; Base64url sin padding, 43 caracteres; nuevo por token |
| `iss` / `aud` | Valores exactos aprobados |
| `iat` | NumericDate UTC entero |
| `exp` | `iat + 900` inicialmente |
| `kid` | Obligatorio al emitir y al resolver validación conforme spec; conjunto local/configurado aprobado |
| `nbf` | No obligatorio; validar si existe |

No refresh tokens, roles autoasignados, PII innecesaria o criptografía propia. Usar un solo stack mantenido. `MapInboundClaims=false`, `SaveToken=false`. Firma obligatoria, algoritmo HS256 en allowlist, clave simétrica aprobada, issuer/audience/lifetime y claims validados antes de confiar en jti/sub. Rechazar `none`, algoritmos inesperados, firma alterada, `kid` ausente/desconocido y fallbacks de probar claves arbitrarias. No resolver jku/x5u u otras referencias remotas del token.

Comprobar tipos, unicidad, longitudes y codificaciones canónicas de claims/header; UUID/jti válidos, iat no futuro fuera de tolerancia, exp mayor a iat y duración compatible con política; nbf válido cuando exista. Rechazar duplicados/estructuras ambiguas, cabeceras críticas no soportadas y entradas mayores a 4 KiB antes de criptografía. Bearer solo en Authorization, un token sin canal alternativo. Limitar parseo previo sin tratar claims sin firma como confiables.

La implementación fijará y probará una frontera temporal consistente antes/en/después de exp y fin del skew; propuesta de precisión: fin exclusivo `ahora < exp + skew`, iat/nbf no posteriores a `ahora + skew`. No adoptar accidentalmente otro borde por un default del paquete; la tolerancia no cambia el exp emitido. La configuración efectiva y pruebas deben documentarse.

Claves aleatorias externas de al menos 256 bits de entropía, independientes por ambiente. Conjunto local limitado por kid, con una clave emisora activa. Rotar: distribuir a validadores, confirmar reconocimiento, cambiar emisión, mantener anterior solo para tokens vigentes y retirarla tras su ventana; no alargar aceptación si las revocaciones previas caducan antes. Compromiso exige retiro extraordinario y reautenticación, no un endpoint de cierre global ordinario. Periodicidad/automatización/gestor AWS quedan diferidos. Todo poseedor de HS256 puede emitir tokens.

Autenticación JWT comprueba firma, claims y vigencia. Autorización normal agrega requisito de NO revocación; default/fallback debe ser seguro y futuros recursos requieren sus permisos propios. Tras logout confirmado, todas las instancias rechazan ese token mientras exista continuidad confiable del estado compartido; ante pérdida o discontinuidad detectada, integridad incierta o estado no verificable, accesos dependientes fallan cerrados y una marca ausente no autoriza. Se conserva el límite de solicitudes que ya superaron el control. Una política exclusiva para `POST /api/v1/auth/logout` permite identidad criptográficamente válida aunque ya revocada para confirmar el mismo efecto; no habilita otras rutas. El servicio verifica/conserva la marca antes de 204. Probar que esa excepción no puede usarse con políticas generales ni nuevas rutas. Redis no verificable se traduce en 503 por el resultado de autorización, no 401 de credenciales falsas.

Register/login son públicos explícitos sin JWT requerido; un Bearer opcional no debe introducir trabajo de autenticación/revocación innecesario o alterar sus contratos. No se agregan recursos de usuario, privilegios de repartidor o administración. Cada login válido puede emitir otro jti sin revocar sesiones anteriores.

## 10. Password hashing y rehash concurrente

**PLD-AUTH-12.** Adaptador pequeño de `PasswordHasher<User>`; framework administra salt, formato, parámetros y versionado, sin iteraciones manuales ni Identity completo. Registrar configuración/versión efectiva en evidencia futura, no contraseñas/hashes. Misma contraseña en cuentas distintas no debe producir un hash determinista común.

Registro: validar y aplicar controles antes de generar un hash y persistirlo. Login: hash real si existe usuario; dummy hash si no existe. Dummy creado una vez durante inicialización del proceso, mismo hasher/configuración y contraseña sintética; listo antes de admitir login, nunca generado/rehashado por request. Disabled recibe trabajo comparable y luego el mismo 401 genérico; un fallo MongoDB no se trata como usuario inexistente. No usar sleeps/jitter como defensa principal ni prometer tiempos idénticos.

`SuccessRehashNeeded`: generar hash actualizado y escribir atómicamente con condición `_id`, `passwordHash` anterior y estado correspondiente (`Active` para login). Actualizar solo passwordHash y updatedAt. Si no coincide, releer y comprobar estado/credenciales dentro de un presupuesto acotado antes de decidir; no sobrescribir concurrentemente ni emitir basándose en información cuya validez ya no puede sostenerse. Resultado incierto → respuesta operativa segura. No agregar `hashVersion` ni otro campo de concurrencia al contrato de siete campos. Login sin rehash no actualiza updatedAt.

La validación del secreto de login no reutiliza mínimos futuros de creación que bloqueen credenciales existentes. Nunca trim, normalización Unicode, case change o truncamiento de contraseña.

## 11. Validación y contratos HTTP

**PLD-AUTH-13.** Validadores propios pequeños con System.Text.Json, sin FluentValidation. Reglas funcionales exactas del spec §§3.2–3.3, no sustituidas por defaults de DTO/DataAnnotations.

| Operación | Request | Éxito y respuesta |
| --- | --- | --- |
| `POST /api/v1/auth/register` | fullName, email, password strings obligatorios | 201: id UUID canónico, fullName, email normalizado; sin token/login automático |
| `POST /api/v1/auth/login` | email, password strings obligatorios | 200: accessToken, tokenType `Bearer`, expiresIn `900` inicialmente; sin refresh token |
| `POST /api/v1/auth/logout` | Authorization Bearer; sin cuerpo funcional | 204 sin cuerpo; repetición segura conforme §9 |

No alias sin v1, GET de usuario/me, Location ficticio o envoltorio global de éxitos. Unknown fields como confirmPassword, teléfono, rol, id o status del cliente se rechazan; no se ignoran/persisten.

| Dato | Validación normativa |
| --- | --- |
| Nombre | Trim externo; no vacío; hasta 200 escalares Unicode usando Rune; sin controles; conserva case, acentos y espacios internos; sin número de apellidos inventado |
| Email | Trim externo, minúsculas invariantes, ASCII, dominio exacto uttt.edu.mx; no espacios internos/controles, nombres de presentación, listas, comentarios o forma entrecomillada; local-part no vacía, sin puntos iniciales/finales/consecutivos; máximo 254 bytes totales/64 local-part; no quitar puntos ni `+` |
| Password registro | Inicial 12–128 escalares, máximo 512 bytes UTF-8; secreto completo sin transformaciones ni composición/caducidad inventadas |
| Password login | String obligatorio, sin transformación y con límites superiores de entrada; no aplicar automáticamente futuros mínimos de creación |

No usar `MailAddress` como única validación. Normalización compartida para registro, búsqueda, índice y cuotas. Dominio válido no prueba posesión del buzón ni condición académica; Active no significa verificado.

Transporte/middleware: cuerpo real máximo 16 KiB, incluido chunked; no confiar solo en Content-Length. JSON funcional application/json UTF-8 estricto; charset/Content-Encoding no soportados → 415. No activar descompresión que permita eludir límites. Cuerpo excesivo → 413. Logout con cuerpo no vacío, incluido whitespace, → 400 dentro del límite general de transporte; cuerpo vacío no exige Content-Type.

Serialización: nombres camelCase exactos, case-insensitive deshabilitado, unmapped members rechazados, profundidad 4, sin comentarios/comas finales/coerción objeto-array-string. Rechazar duplicados con `AllowDuplicateProperties=false` cuando lo permita la API efectiva .NET 10; si hay impedimento, implementar comprobación equivalente acotada, no aceptar duplicados. Null obligatorio, campos ausentes, JSON/Unicode inválido y profundidad excesiva → 400 seguro. Contar escalares y bytes estrictos sin sustitución silenciosa de Unicode inválido.

JWT presentado máximo 4 KiB antes de trabajo criptográfico; Authorization única y sin query/cookie/cuerpo alternativo. Rechazo API por JWT excesivo: 401 seguro como concreción técnica, distinto de rechazo de cabecera completa por servidor/borde, potencialmente 431. Definir límites globales de headers y recepción en infraestructura. Lo producido fuera de la API no puede garantizarlo su writer Problem Details.

## 12. Problem Details y clasificación de errores

**PLD-AUTH-14.** AddProblemDetails, IProblemDetailsService e IExceptionHandler nativos. Una política común integra errores de MVC, validación, challenge Bearer, autorización y cuotas; capturar excepciones no cubre por sí solo esos caminos.

RFC 9457, `application/problem+json`, type inicial `about:blank`, title/status/detail/code/traceId; errors solo para validación por campo, sin copiar valores. Sin instance inicial; status igual al HTTP real, mensajes genéricos del spec §3.4. TraceId generado o validado por servidor y coherente con logs. Cache-Control `no-store` en respuestas sensibles, incluidas fallidas y exitosas de autenticación.

| HTTP | code | Origen / tratamiento |
| --- | --- | --- |
| 400 | `validation_failed` | Entrada inválida, sin detalles sensibles |
| 401 | `authentication_failed` | Inexistente, contraseña incorrecta, Disabled, token inválido o revocado fuera de la excepción logout |
| 409 | `registration_conflict` | Conflicto de registro genérico |
| 413 | `payload_too_large` | Concreción técnica propuesta por SOL-PLAN-002, no nuevo requisito funcional |
| 415 | `unsupported_media_type` | Concreción técnica propuesta por SOL-PLAN-002 |
| 429 | `rate_limited` | Retry-After cuando sea calculable, redondeo conservador |
| 500 | `internal_error` | Error inesperado seguro |
| 503 | `temporarily_unavailable` | Dependencia/control necesario no disponible o estado incierto |

No nombres MongoDB/Redis, infraestructura, stack traces, driver exceptions, conexiones o secretos en respuesta. No motivos privados de login ni existencia/estado de cuenta. Challenge Bearer cuando corresponda sin detalles internos; evitar redirecciones de autenticación y respuestas 200 por error. 204 nunca tiene cuerpo.

Validación, credenciales incorrectas, cuota y conflictos esperados usan resultados tipados donde sea posible. Adaptadores traducen excepciones reales de driver; no excepciones como flujo normal. Fallos operativos no se convierten en credenciales inválidas. Cancelación del cliente no inventa respuesta; presupuesto interno agotado se clasifica de forma segura, sin convertir indiscriminadamente todo cancellation en 500. Si la respuesta ya comenzó, no escribir un segundo documento.

Contrato de errores del proxy/servidor y precedencia para peticiones con múltiples defectos se concretan sin cambiar los estados funcionales aprobados; no reejecutar register/login/logout mediante páginas de error.

## 13. Rate limiting distribuido y pseudonimización

**PLD-AUTH-15/16/17.** RateLimiter nativo en memoria NO acredita distribución. Redis coordina ventanas móviles y evaluación conjunta de ámbitos configurables:

| Operación | Ámbito | Máximo / ventana |
| --- | --- | --- |
| REGISTER | normalizedEmail | 3 / 15 min |
| REGISTER | origin | 20 / 15 min |
| LOGIN | origin + email | 5 / min |
| LOGIN | email | 10 / min |
| LOGIN | origin | 60 / min |
| LOGOUT | cuenta autenticada | 30 / min |
| LOGOUT | origin | 300 / min |

Diseño preferido: sorted sets de eventos admitidos por ámbito y un script Lua pequeño atómico. Tiempo de Redis común; eliminar eventos fuera de ventana, comprobar todos los ámbitos aplicables, insertar un mismo ID interno de intento en todos solo si se admite y fijar expiración acotada. Si cualquiera bloquea, 429 sin nuevos eventos que prolonguen indefinidamente penalización. Intentos admitidos consumen aunque fallen credenciales o no exista cuenta. No Disabled, bloqueo permanente o sanciones.

Retry-After corresponde al máximo tiempo necesario para liberar los ámbitos bloqueantes, redondeado hacia arriba; sin publicar email/contador/causa individual ni prometer éxito al reintentar. Un ID interno puede evitar doble contabilización de la misma ejecución, sin crear una clave pública de idempotencia no aprobada. Timeout no autoriza reintento ciego de scripts con resultado incierto.

Scripts pequeños, versionados y probados; verificar tipos/argumentos antes de mutar porque Lua no da rollback general tras un error. Evaluación conjunta requiere claves accesibles atómicamente en la topología elegida; no se fija Redis Cluster ni se presume atomicidad cross-slot. Un diseño cluster futuro debe demostrar compatibilidad o revisarse antes de uso.

HMAC-SHA-256 con clave exclusiva por ambiente/finalidad para email y par origen-email, componentes codificados inequívocamente y pseudónimos acotados. No email en claro/hash simple, signing key JWT, password ni secreto reutilizado. Si se pseudonimiza origen/cuenta, separar contexto de finalidad. Rotación coordinada preserva ventanas previas; no cambiar clave/prefijo para reiniciar cuotas. El protocolo de transición y revisión de política se definirá antes de construirlo.

Origen: RemoteIpAddress tras procesar Forwarded Headers exclusivamente de proxies conocidos/autorizados. Cabeceras directas X-Forwarded-For/Forwarded del cliente no son autoridad. Canonicalizar IPv4/IPv6; no agrupar prefijos IPv6 silenciosamente. Detalles LB/proxies/TLS se resuelven en AWS. NAT universitario sigue afectando usuarios legítimos.

Aplicar límites de transporte y cuotas antes del hashing/trabajo costoso cuando corresponda. No usar sub/jti sin autenticar para cuenta/logout. La coordinación entre protección temprana de origen y consumo conjunto debe evitar dobles cargos o admisión parcial; falta concretar tratamiento de JSON ilegible, email no normalizable y prioridades 400/429/503. No crear buckets arbitrarios de email inválido ni hacer depender cuotas de existencia de usuario. Una defensa local de concurrencia/origen es suplementaria, nunca la cuota global.

Cardinalidad: claves de tamaño fijo, eventos por bucket limitados, TTL, registro/presupuesto de particiones activas y no creación de nuevas particiones al rechazar. Definir topes/memoria y tratamiento 503 al no poder mantener control, no evicción de seguridad ni admisión sin límites. TTL por sí solo no acota crecimiento frente a identidades aleatorias. Pérdida/recuperación de contadores conforme §8.3; no presentar reinicio API o fallback local como continuidad distribuida.

## 14. Logging, métricas y privacidad

**PLD-AUTH-23.** ILogger nativo, LoggerMessage si aporta valor; sin Serilog inicial. Eventos estructurados: registro exitoso/rechazado, login exitoso/fallido, logout confirmado, rechazo por revocación, cuota excedida, dependencia no disponible, confianza perdida y error inesperado.

Campos acotados: instante UTC, ID/tipo de evento, operación, resultado/categoría segura, ambiente, versión, instancia, duración y correlación. No registrar contraseña, passwordHash, JWT completo, Authorization, claves, conexiones, secretos, cuerpos sensibles o valores sensibles de errores. Tampoco volcar excepción de driver u objeto request/options completos. Plantillas constantes, sin interpolar texto arbitrario de cliente; controlar inyección de logs.

No incluir email/IP/sub/jti automáticamente ni usarlos como etiquetas de métricas. Si una correlación de identidad se justifica, pseudónimo separado por finalidad y acceso, con política de retención. TraceIdentifier generado por servidor se correlaciona con Activity de forma validada/acotada; no aceptar ciegamente correlation IDs externos.

Métricas por operación/resultado/instancia y categorías acotadas: latencia, fallos de dependencia, cuotas, revocaciones, saturación del hasher y no admisión. No declarar disponibilidad, integridad o cobertura por tener logs. Exportación/CloudWatch, alertas, pérdidas/muestreo y retención/accesos/eliminación definitivos quedan diferidos antes de producción. No prometer cumplimiento legal sin contexto aprobado.

Respuestas/token/credenciales no se cachean en HTTP/Redis de rendimiento; `no-store`, sin compresión de secretos no evaluada. Blacklist y cuotas no acreditan por sí mismas caché funcional de rendimiento; paginación no aplica a estos tres comandos sin listados y queda para otra spec.

## 15. Pipeline HTTP y health/readiness

**PLD-AUTH-19/24.** Orden conceptual, a adaptar a APIs efectivas de ASP.NET Core 10 sin cambiar garantías:

1. Contexto/correlación segura y política de respuesta sensible.
2. Exception handling centralizado.
3. Forwarded Headers de proxies explícitamente confiables.
4. HTTPS/HSTS según ambiente y transporte aprobado.
5. Routing y metadata de endpoints.
6. Límites HTTP/transporte y presupuestos antes de lectura costosa.
7. Protección temprana de recursos; sin sustituir cuota distribuida.
8. Authentication: firma, claims y vigencia.
9. Authorization/revocation; excepción logout exclusivamente acotada.
10. MVC: deserialización estricta, validación, cuotas conjuntas y servicio.
11. Respuesta/eventos seguros y correlacionados, incluidos rechazos tempranos.

Forwarding antes de origen/esquema; routing antes de decisiones por endpoint; autenticidad antes de claims confiables/revocación; límites antes de hashing. Conectar writers de challenge/autorización y errores MVC para que no escapen del contrato común. Evitar que el 400 automático de MVC impida un control asíncrono requerido o contabilice dos veces: concretar filtro/orden al implementar con pruebas. Si se usa RateLimiter nativo auxiliar por endpoint, ubicarlo después de routing; no puede por sí solo obtener email de un cuerpo aún no validado.

HTTPS publicado debe imponerse por diseño de transporte, no por EXPOSE 8081, perfil local o redirección de plantilla. Confianza proxy y TLS interno requieren infraestructura aprobada; no seleccionarla aquí.

Conservar `/health` como liveness básico. Readiness independiente futura, potencial `/health/ready`, sin implementarla aquí: acceso MongoDB, índice requerido, Redis y admisión segura. Ping no es prueba de integridad Redis; caída MongoDB no debe obligar a que la lógica de logout consulte usuarios. Diferenciar readiness del conjunto y dependencia efectiva de cada operación. Respuesta pública sin detalles internos; uso por LB/control de admisión a definir después. No crear un endpoint de negocio protegido para demostrar pruebas; usar superficie de test no productiva cuando corresponda.

## 16. Testing y evidencia prevista

**PLD-AUTH-20.** Tres proyectos independientes con xUnit. Integración/seguridad con WebApplicationFactory + Testcontainers.MongoDb + Testcontainers.Redis; dependencias reales, no mocks como demostración de MongoDB/Redis. Unitarios pueden usar fakes pequeños de puertos y TimeProvider sin Moq.

| Suite | Casos previstos y relación |
| --- | --- |
| Unit | Normalización/dominio, nombre/password Unicode y bytes, fronteras, estado, configuración inválida, dummy, decisiones rehash, claims, TTL y clasificación segura; AUTH-005–008, 010–020, 024, 031, 039, 041 |
| Integration | Register válido, duplicado concurrente A/B, índice/BSON/lectura, login correcto/incorrecto, inexistente/Disabled, dummy/rehash persistido, JWT, logout/repetición, sesiones A/B, revocación cruzada, cuotas conjuntas, caídas/resultados inciertos y Problem Details; AUTH-004–039, 042 |
| Security | NoSQL injection/mass assignment, JSON inválido/duplicados/null/tipos, límites y Unicode, JWT manipulado/expirado/issuer/audience/algoritmo/kid/claims, enumeración/tiempos, replay, pérdida Redis y filtraciones; AUTH-008, 011, 018–030, 032–040, 043 |

Debe existir cobertura explícita de cada AC-01 a AC-21 del spec, incluidos fronteras iat/exp/skew/nbf y retención; mismo 401 para inexistente/contraseña/Disabled; dummy preparado una vez; SuccessRehashNeeded concurrente; token A revocado entre instancias sin afectar B; Redis caído produce 503 y ningún bypass; MongoDB caído no produce login ficticio ni registro exitoso; no-store y absence de secretos en respuestas/logs/trazas/persistencia. Probar excepción logout contra políticas normales.

Datos sintéticos, namespace/base por ejecución y al menos dos hosts compartiendo dependencias. Fijar imágenes/versiones/digests después de revisión; readiness reproducible, sin sleeps arbitrarios. MongoDB replica set en escenarios de majority/failover; standalone no los prueba. Fallos de red/ACK perdido, presión de memoria, pérdida/flush/restauración antigua/failover y reapertura se ensayan sobre objetivos autorizados. Conforme AUTH-027/043 y AC-15 ajustados por APR-03, verificar pérdidas detectables, reacción fail-closed y recuperación/cuarentena; evaluar el riesgo no observable sin afirmar detección universal ni convertir un ping exitoso en prueba de integridad.

Pruebas de tiempos con perfiles/muestras/tolerancias aprobados, sin prometer igualdad absoluta ni debilitar hashing. Pruebas de saturación/cardinalidad no permiten colas ilimitadas ni etiquetas de métricas sensibles.

Testcontainers local no demuestra MongoDB Atlas, TLS/ACL del servicio, AWS, LB o infraestructura productiva. Requerir integración autorizada posterior con Atlas de pruebas y evaluación del Redis/infraestructura elegidos. No convertir dos factories en evidencia de dos instancias desplegadas tras balanceador.

Cada evidencia futura: AUTH/AC/amenaza, commit, ejecución, herramienta/versión, configuración, entorno/topología, resultado y tratamiento. No fijar porcentaje arbitrario de cobertura; una documentación/test escrito sin ejecución no satisface gate. Construcción y pruebas se incorporan a cada incremento autorizado, no al final.

## 17. DevSecOps, Docker y gates futuros

**PLD-AUTH-21/22.** GitHub Actions futuro; no se crea workflow ni recurso. Jobs previstos:

| Job | Responsabilidad futura |
| --- | --- |
| `build` | Restore reproducible, build Release, identidad de SDK/runtime/dependencias y artefacto |
| `unit-tests` | Suite unitaria y resultados trazables |
| `integration-tests` | Integración y suite SecurityTests explícitamente ejecutadas con contenedores autorizados |
| `security-scan` | SAST, dependencias/licencias, secretos y contenedor; no sustituye SecurityTests |

Herramientas candidatas: CodeQL + analizadores .NET; NuGet Audit incluyendo transitivas + Dependabot; Gitleaks; Trivy sobre imagen final/base. Registrar licencias/inventario/SBOM según herramienta a concretar. No son controles ejecutados ni versiones aprobadas.

Política candidata: High/Critical aplicable bloquea salvo excepción documentada con responsable/vencimiento; secretos activos bloquean y requieren rotación/tratamiento, no solo borrado; SecurityTests fallidos bloquean; fallo de scanner/resultado ausente nunca equivale a aprobación. Concretar aplicabilidad, otras severidades, licencias, excepciones y evidencias antes de gates. Verificar aviso OpenAPI informado con evidencia real futura.

Required checks solo después de jobs reales confiables: nombres estables, triggers apropiados sin filtros que omitan checks, permisos mínimos, acciones fijadas a commit, PR no confiables sin secretos productivos, sin ejecutar código hostil con privilegios vía pull_request_target. Branch protection/rulesets requiere autorización propia; no se cambia aquí. No despliegues automáticos ni selección de recursos AWS.

### 17.1. Contexto y publicación Docker

Antes de construir imágenes en un entorno con secretos locales, endurecer `.dockerignore` como trabajo explícito posterior. El Dockerfile actual usa COPY del contexto; `.gitignore` no lo protege. Excluir al menos `.env`, `.env.*`, `appsettings.Local.json`, `appsettings.*.Local.json`, `secrets.json`, `*.pfx`, `*.p12`, `*.pem`, `*.key`, `.git`, `.vs`, `bin`, `obj`, `TestResults`, `coverage`, `.terraform`, `*.tfstate` y `*.tfvars`, con alcance de rutas y variantes sensibles pertinentes. Eliminar excepciones que reintroduzcan metadatos Git sensibles en el contexto; no alterar esos archivos en esta entrega.

Revisar además exclusiones de contenido/publish MSBuild, variables/argumentos de build, logs/capas y artefactos finales. No basta ocultar un archivo de Git. Base/SDK fijados y revisados, usuario no privilegiado y scan de imagen, asociados a commit/digest/ejecución; reproducibilidad operacional no afirma identidad bit a bit sin criterio específico. Contexto/exportación de secretos debe comprobarse antes de autorizar esa construcción, no confiar solo en un scan posterior.

## 18. Seguridad y threat model

Se reutilizan AM-01 a AM-14 del spec, sin crear requisitos de negocio. La edición/matriz OWASP API Security completa queda para ratificación posterior; no afirmar cobertura universal.

| Amenaza | Controles previstos | AUTH / casos |
| --- | --- | --- |
| Credential stuffing / brute force | Cuotas conjuntas distribuidas, hasher y concurrencia acotada | AUTH-006, 010–013, 028–030, 040; AC-07/08/16/18 |
| NoSQL injection / mass assignment | Contratos cerrados, tipos estrictos y filtros propios | AUTH-005, 007–009, 014–015; AC-02/03/05 |
| Enumeración | 401 uniforme, dummy hash, cuota independiente de existencia; residual 201/409 | AUTH-004, 011, 029, 031, 036; AC-07/08/19 |
| JWT tampering / algorithm confusion | HS256 allowlist, firma, issuer/audience, kid cerrado y claims | AUTH-016–020, 035, 043; AC-09/10 |
| Replay / token robado | Vida acotada, TLS/no-store, logout distribuido; jti no da proof-of-possession | AUTH-017–024, 032–034; AC-10–13 |
| Redis indisponible | Fail-closed y 503, sin caché local negativa/fallback cuota | AUTH-022–028, 038; AC-14 |
| Redis pérdida/evicción/failover | noeviction, confirmación, pérdida detectada cierra y recuperación segura; silenciosa no observable es residual conforme APR-03 | AUTH-025–027, 039, 042–043; AC-15/20 |
| MongoDB caído / resultado incierto | Índice, concerns, timeouts y ausencia de éxito ficticio | AUTH-004, 009, 014–015, 038–039; AC-03/17 |
| Secret leakage / log injection | Secretos externos, eventos por allowlist, no cuerpos, contexto/imagen revisados | AUTH-019, 032, 034, 036–037, 044–045; AC-19/21 |
| Payload/hashing/cardinalidad | Límites previos, Rune/UTF-8, JSON estricto, cuotas y concurrencia | AUTH-006–008, 028–030, 038, 040; AC-04/05/18 |
| Race conditions | Índice único, rehash condicionado, operaciones Redis atómicas | AUTH-009, 013, 022–024, 039, 042; AC-03/06/12 |
| Proxy falso / HTTP | Forwarding confiable y TLS publicado a verificar | AUTH-030, 033–034, 046; AC-16/20 |
| Dominio sin titularidad | No marcar correo verificado ni inventar verificación | AUTH-002, 005, 012; AC-01/02/08 |
| HS256 comprometido | Custodia mínima, ambientes y rotación/retiro | AUTH-019–020, 034, 044; AC-20/21 |

No nuevos roles/privilegios por registro; recursos futuros requieren autorización por objeto/función en sus specs. MFA, refresh, recuperación/cambio de contraseña, OAuth externo, administración completa, repartidor, CLABE, chat, pagos, pedidos y push siguen excluidos. No crear funcionalidades adicionales para acreditar requisitos académicos no aplicables al módulo.

## 19. Implementación incremental futura

**PLD-AUTH-25.** Secuencia conceptual, no tareas ejecutables ni autorización de ejecución. Cada incremento necesita alcance/autorización propia, incluye pruebas desde el inicio y no habilita automáticamente el siguiente.

| Incremento | Contenido futuro | Evidencia/puerta antes de aceptación |
| --- | --- | --- |
| A. Base transversal segura | Estructura, Options, Problem Details, límites y pruebas base | Configuración inválida/entradas/rechazos seguros |
| B. Redis y cuotas | Conexiones, namespaces, ventanas móviles, consumo conjunto y fail-closed | Dos instancias sin multiplicar cuota; fallos sin bypass |
| C. MongoDB/users | BSON, persistencia, inicializador, índices y concurrencia | Unicidad/lectura confirmada y resultados inciertos |
| D. Register | Validación, PasswordHasher y creación confirmada | 201/409 seguros, sin token automático |
| E. JWT y autorización | Key ring, emisión/validación y políticas | Algoritmos/claims/temporalidad y protección normal; no emitir públicamente sin controles completos |
| F. Logout | Revocación Redis e idempotencia | ACK/TTL, repetición, rechazo cruzado y recuperación aprobada |
| G. Login | Dummy preparado, Disabled, rehash y emisión | 401 genérico, CAS seguro y controles disponibles |
| H. Seguridad/resiliencia | Fallos, saturación, pérdida detectada, recuperación y leakage tests | Evidencia de riesgos/limitaciones, sin cerrar pérdida silenciosa por documentación |
| I. DevSecOps | CI, scanners, Docker y gates | Endurecimiento contexto ANTES del primer build sensible; ejecuciones trazables |

No esperar hasta I para proteger un build sensible ni hasta H para probar validación/seguridad. No publicar endpoints parcialmente protegidos. APR-03 resuelve la revisión normativa AUTH-027 antes de descomposición; las tareas futuras deberán incluir las puertas de definición/verificación operativa pendientes, no ocultarlas ni presentarlas como ejecutadas.

## 20. Riesgos residuales y límites conservados

| Riesgo | Tratamiento / límite |
| --- | --- |
| Contraste 201/409 | Permite inferencia parcial de existencia; no cambiar contrato silenciosamente |
| Dominio uttt.edu.mx | No demuestra posesión del correo o condición académica |
| NAT universitario | Cuotas por origen pueden afectar usuarios legítimos; no afirmar eliminación del problema |
| Bearer robado | Replay posible antes de logout/expiración; jti no lo impide solo |
| HS256 | Todo poseedor de clave puede emitir; custodia, aislamiento y retiro esenciales |
| Redis catastrófico/silencioso no observable | Riesgo residual aprobado documentalmente por APR-03; discrepancia normativa resuelta, riesgo no eliminado y mitigación/verificación de infraestructura pendientes |
| Namespaces sin aislamiento físico | No previenen presión compartida; noeviction protege expulsión, no capacidad ilimitada |
| ACK/failover/topología | Confirmar operación no demuestra pérdida cero; validar servicio antes de aceptación |
| Resultados MongoDB/Redis inciertos | Respuestas conservadoras; error/respuesta perdida no demuestra ausencia de efecto |
| Cuotas/cardinalidad/HMAC | Rotación/reset/cambio de política puede abrir límites; coordinación y dimensionamiento pendientes |
| Pruebas locales | No acreditan Atlas, AWS, LB, TLS/proxies o operación real |
| Latencias/presupuestos | Candidatos calibrables, no SLA/capacidad medida; no reducir hashing |
| Contexto Docker | .gitignore no protege contexto/publish; endurecimiento antes de build sensible |
| Observabilidad/secretos | Retención/accesos y gestor concreto pendientes; logs/scans no garantizan ausencia universal de fuga |

La aceptación de un riesgo documental no demuestra control operativo ni modifica por sí sola el criterio vigente del spec. No se usan porcentajes de implementación, seguridad universal o cumplimiento académico sin evidencia/rúbrica.

## 21. Registro de decisiones PLD-AUTH

Fuente de las 25 filas: SOL-PLAN-002, con resolución normativa posterior APR-03 para PLD-AUTH-08. **A** = aprobado en origen como decisión de diseño compatible; **C** = aprobado como candidato/concreción, requiere calibración/revisión indicada. El estado histórico **R** de PLD-AUTH-08 (discrepancia normativa) queda resuelto por APR-03 y conservado en §23.1. Ninguna fila equivale a implementación o aceptación técnica; aprobación documental global según §§1 y 24.

| ID | Decisión / límites y estado | Desarrollo | AUTH relacionados |
| --- | --- | --- | --- |
| PLD-AUTH-01 | A: único Enver.API, tres proyectos de pruebas; sin proyectos de aplicación extra ni patrones excluidos | §§3–4, 16 | AUTH-001, AUTH-002, AUTH-035, AUTH-041, AUTH-042, AUTH-043, AUTH-048 |
| PLD-AUTH-02 | A: separación lógica y dependencias interiores; interfaces solo con valor | §§3–4 | AUTH-001, AUTH-002, AUTH-014, AUTH-015, AUTH-035, AUTH-041, AUTH-047 |
| PLD-AUTH-03 | A para paquetes; versiones y JsonWebTokens directo condicionales, sin segundo stack | §5 | AUTH-013, AUTH-016, AUTH-018, AUTH-041, AUTH-042, AUTH-043, AUTH-044, AUTH-045 |
| PLD-AUTH-04 | A: Options tipadas, validación inicial y secretos externos | §6 | AUTH-006, AUTH-008, AUTH-019, AUTH-020, AUTH-029, AUTH-034, AUTH-038 |
| PLD-AUTH-05 | A: siete campos, UUID estándar, índice único, cliente singleton; concerns propuestos según garantía | §7 | AUTH-002, AUTH-004, AUTH-005, AUTH-007, AUTH-009, AUTH-012, AUTH-014, AUTH-015, AUTH-038, AUTH-039 |
| PLD-AUTH-06 | A: inicializador idempotente; bloquear registro sin índice, sin reparar datos | §7 | AUTH-004, AUTH-009, AUTH-015, AUTH-038, AUTH-042 |
| PLD-AUTH-07 | A: Redis compartido y namespaces, no exigir dos instancias físicas; no expulsión y sin aislamiento ficticio por DB | §8 | AUTH-022, AUTH-024, AUTH-025, AUTH-026, AUTH-028, AUTH-032, AUTH-046 |
| PLD-AUTH-08 | A, alineada por APR-03: sin testigo MongoDB/segundo almacén duradero obligatorio/transacciones; continuidad confiable, fail-closed ante pérdida detectada/integridad incierta y recuperación segura; silenciosa no observable es residual, no eliminada | §§8, 23.1 | AUTH-022, AUTH-025, AUTH-026, AUTH-027, AUTH-028, AUTH-038, AUTH-039, AUTH-042, AUTH-043 |
| PLD-AUTH-09 | C: exp + skew 30 s + margen configurable 60 s candidato; nunca reducir TTL ni extender JWT | §§6, 8.1 | AUTH-020, AUTH-023, AUTH-024, AUTH-039 |
| PLD-AUTH-10 | A: HS256/issuer/audience/900 s/skew, claims y kid; claves locales aprobadas y validación explícita | §9 | AUTH-003, AUTH-010, AUTH-016, AUTH-017, AUTH-018, AUTH-019, AUTH-020, AUTH-021, AUTH-034 |
| PLD-AUTH-11 | A: firma/claims separados de revocación, excepción solo POST logout para idempotencia | §§8.1, 9 | AUTH-003, AUTH-018, AUTH-021, AUTH-022, AUTH-023, AUTH-026, AUTH-035, AUTH-039 |
| PLD-AUTH-12 | A: PasswordHasher<User>, dummy previo, Disabled comparable y CAS de rehash sin hashVersion | §10 | AUTH-006, AUTH-010, AUTH-011, AUTH-012, AUTH-013, AUTH-014, AUTH-039 |
| PLD-AUTH-13 | A: validadores propios/JSON estricto, Unicode/bytes y límites; sin aceptar duplicados por limitación de API | §11 | AUTH-003, AUTH-005, AUTH-006, AUTH-007, AUTH-008, AUTH-018, AUTH-030, AUTH-040 |
| PLD-AUTH-14 | A para mecanismo/contrato; C para concreciones 413/415; no-store y errores seguros | §12 | AUTH-003, AUTH-004, AUTH-008, AUTH-011, AUTH-023, AUTH-026, AUTH-029, AUTH-031, AUTH-032, AUTH-038 |
| PLD-AUTH-15 | A: Redis distribuido, cuotas aprobadas, ventanas móviles y atomicidad; Lua pequeño/probado, sin fijar Cluster | §13 | AUTH-028, AUTH-029, AUTH-030, AUTH-038, AUTH-040, AUTH-042 |
| PLD-AUTH-16 | A: HMAC-SHA-256 por ambiente/finalidad, no email claro/reutilización; rotación sin reset | §13 | AUTH-028, AUTH-030, AUTH-032, AUTH-034, AUTH-036 |
| PLD-AUTH-17 | A: origen tras forwarding confiable, detalle LB diferido y NAT residual | §§13, 15 | AUTH-028, AUTH-030, AUTH-033, AUTH-046 |
| PLD-AUTH-18 | C: presupuestos 8/4 s, Mongo 2 s, Redis 750 ms, conexión 2 s, hash 2; saturación 503 y calibración | §6 | AUTH-008, AUTH-011, AUTH-026, AUTH-030, AUTH-038, AUTH-040, AUTH-041 |
| PLD-AUTH-19 | A: pipeline conceptual aprobado, adaptación a APIs reales sin modificar host aquí | §15 | AUTH-003, AUTH-008, AUTH-018, AUTH-023, AUTH-026, AUTH-030, AUTH-031, AUTH-033, AUTH-035 |
| PLD-AUTH-20 | A: tres suites, WebApplicationFactory y contenedores reales; replica set donde aplique, sin acreditar cloud localmente | §16 | AUTH-009, AUTH-013, AUTH-022, AUTH-027, AUTH-028, AUTH-039, AUTH-040, AUTH-041, AUTH-042, AUTH-043, AUTH-047 |
| PLD-AUTH-21 | A para diseño de jobs; C para herramientas/gates/política, sin workflows ni ejecuciones | §17 | AUTH-034, AUTH-040, AUTH-041, AUTH-042, AUTH-043, AUTH-044, AUTH-045, AUTH-046, AUTH-047, AUTH-048 |
| PLD-AUTH-22 | A: endurecimiento Docker/contexto/publish necesario antes de build sensible, no realizado | §17.1 | AUTH-032, AUTH-034, AUTH-044, AUTH-045, AUTH-046 |
| PLD-AUTH-23 | A: ILogger/LoggerMessage, eventos mínimos sin secretos ni PII en métricas | §14 | AUTH-011, AUTH-031, AUTH-032, AUTH-034, AUTH-036, AUTH-037 |
| PLD-AUTH-24 | A: conservar /health básico y diseñar readiness independiente futuro, sin detalles públicos | §15 | AUTH-009, AUTH-015, AUTH-026, AUTH-027, AUTH-037, AUTH-038, AUTH-046 |
| PLD-AUTH-25 | A: incrementos A–I con pruebas continuas; no tareas ni ejecución automática | §§19, 24 | AUTH-001, AUTH-038, AUTH-039, AUTH-040, AUTH-041, AUTH-042, AUTH-043, AUTH-044, AUTH-045, AUTH-046, AUTH-047, AUTH-048 |

## 22. Trazabilidad inversa y cobertura de los 48 AUTH

La tabla de §21 expresa PLD-AUTH → AUTH; esta matriz permite verificar cobertura inversa individual. Los números PLD abreviados remiten exclusivamente a PLD-AUTH-nn, no a PLD de 001. Cobertura significa relación de diseño/documentación, no requisito cumplido; APR-03 resuelve el bloqueo normativo de AUTH-027, sin satisfacer sus controles técnicos. Los requisitos de gobierno no se presentan como código a implementar. Los AC del spec mantienen sus IDs y sirven como puente hacia evidencia futura.

| AUTH | PLD-AUTH relacionados | Evidencia/puerta futura prevista |
| --- | --- | --- |
| AUTH-001 | 01, 02, 25 | Autorizaciones, fuente normativa y límites de entrega |
| AUTH-002 | 01, 02, 05 | Cuenta mínima sin teléfono/repartidor |
| AUTH-003 | 10, 11, 13, 14, 19 | Tres rutas v1, contratos/201/200/204 |
| AUTH-004 | 05, 06, 14 | Creación confirmada, conflicto y resultado incierto |
| AUTH-005 | 05, 13 | Normalización exacta, dominio y cuotas coherentes |
| AUTH-006 | 04, 12, 13 | Escalares/bytes y política de creación distinta de login |
| AUTH-007 | 05, 13 | Nombre sin transformación indebida/datos extra |
| AUTH-008 | 04, 13, 14, 18, 19 | Límites reales, JSON/tipos/inyección y errores |
| AUTH-009 | 05, 06, 20, 24 | Índice listo y carrera entre hosts |
| AUTH-010 | 10, 12 | Credenciales válidas y emisión solo con controles |
| AUTH-011 | 12, 14, 18, 23 | 401 uniforme/dummy/tiempos/logs seguros |
| AUTH-012 | 05, 12 | Active/Disabled, servidor controla estado |
| AUTH-013 | 03, 12, 20 | Hasher mantenido, dummy y rehash concurrente |
| AUTH-014 | 02, 05, 12 | Siete campos, UUID/timestamps y sin hashVersion |
| AUTH-015 | 02, 05, 06, 24 | Patrón por email, índices y lectura confirmada |
| AUTH-016 | 03, 10 | Emisión HS256 con conjunto aprobado |
| AUTH-017 | 10 | Claims mínimos y jti aleatorio canónico |
| AUTH-018 | 03, 10, 11, 13, 19 | Firma/claims/límites antes de revocación |
| AUTH-019 | 04, 10 | Entropía, custodia y rotación con coexistencia |
| AUTH-020 | 04, 09, 10 | Vida/skew/fronteras y retención coordinada |
| AUTH-021 | 10, 11 | Sesiones independientes y riesgo replay |
| AUTH-022 | 07, 08, 11, 20 | ACK/lecturas, rechazo cruzado con continuidad confiable y fail-closed ante pérdida detectada/integridad incierta |
| AUTH-023 | 09, 11, 14, 19 | Logout repetido 204 vigente, 401 inválido, 503 incierto |
| AUTH-024 | 07, 09 | TTL suficiente/no decreciente y fronteras |
| AUTH-025 | 07, 08 | noeviction, namespaces y presión compartida |
| AUTH-026 | 07, 08, 11, 14, 18, 19, 24 | Redis no verificable → 503 sin bypass |
| AUTH-027 | 08, 20, 24 | Ajuste normativo aprobado/aplicado por APR-03; detección, cierre, recuperación y riesgo residual requieren evidencia; NO implementado/satisfecho |
| AUTH-028 | 07, 08, 15, 16, 17, 20 | Cuota conjunta distribuida y recuperación |
| AUTH-029 | 04, 14, 15 | Valores aprobados/Retry-After sin sanciones |
| AUTH-030 | 13, 15, 16, 17, 18, 19 | Origen confiable/HMAC/cardinalidad/saturación |
| AUTH-031 | 14, 19, 23 | Problem Details estable y challenges seguros |
| AUTH-032 | 07, 14, 16, 22, 23 | Minimización/no-store/sin secretos cacheados |
| AUTH-033 | 17, 19 | TLS/proxies verificados en infraestructura |
| AUTH-034 | 04, 10, 16, 21, 22, 23 | Secretos externos y sin filtraciones en artefactos |
| AUTH-035 | 01, 02, 11, 19 | Denegación por defecto/excepción logout acotada |
| AUTH-036 | 16, 23 | Eventos seguros, identidad mínima y retención definida |
| AUTH-037 | 23, 24 | Métricas acotadas/health distinto de integridad |
| AUTH-038 | 04, 05, 06, 08, 14, 15, 18, 24, 25 | Presupuestos/fallos/resultado incierto sin ficción de éxito |
| AUTH-039 | 05, 08, 09, 11, 12, 20, 25 | Carrera registro/rehash/logout sin exactly-once |
| AUTH-040 | 13, 15, 18, 20, 21, 25 | Perfil/carga/seguridad y candidatos sin SLA |
| AUTH-041 | 01, 02, 03, 18, 20, 21, 25 | Unit tests trazables con tiempo/datos sintéticos |
| AUTH-042 | 01, 03, 06, 08, 15, 20, 21, 25 | Integración real/dos hosts/fallos con representatividad |
| AUTH-043 | 01, 03, 08, 20, 21, 25 | Amenazas/casos negativos, pérdida detectable y recuperación; evaluar riesgo no observable sin detección universal |
| AUTH-044 | 03, 21, 22, 25 | SAST/dependencias/licencias/secretos con evidencia |
| AUTH-045 | 03, 21, 22, 25 | Build/imagen reproducibles y revisados |
| AUTH-046 | 07, 17, 21, 22, 24, 25 | Cloud/CI/Terraform/topología posteriores, sin recursos elegidos |
| AUTH-047 | 02, 20, 21, 25 | Fuente → decisión → autorización → cambio → evidencia |
| AUTH-048 | 01, 21, 25 | Exclusiones, cierre y no ejecución automática |

Cobertura documental de diseño: **48/48 AUTH**, con 25 decisiones y la discrepancia normativa resuelta por APR-03. Tareas/commits/evidencias de autenticación aún ausentes no se inventan; no se asignan IDs de tareas anticipados ni se cierra aceptación técnica.

## 23. Historial de discrepancia, resolución normativa y decisiones diferidas

### 23.1. AUTH-027: antecedente conservado y resolución por APR-03

<details>
<summary>Registro íntegro de la propuesta v0.1.0 — histórico, superado por la resolución APR-03 que figura después</summary>

**Estado: PROPUESTA NO APLICADA AL SPEC; REVISIÓN NORMATIVA BLOQUEANTE.**

El criterio actual de AUTH-027 exige que la revocación confirmada no resucite ante reinicio, flush, restauración antigua y failover con pérdida; además indica que reaccionar solo a fallos detectados no demuestra sostener la garantía frente a pérdida silenciosa. SOL-PLAN-002 / PLD-AUTH-08 limita 002 a Redis compartido/noeviction/TTL/fail-closed y acepta pérdida catastrófica silenciosa como riesgo residual de infraestructura, sin testigo duradero externo. Esto no equivale al cumplimiento del criterio vigente. No se presenta un algoritmo que detecte lo indetectable ni se afirma una garantía absoluta.

Propuesta de descripción a revisar por el responsable, no texto sustitutorio vigente:

> Tratar Redis como estado de seguridad compartido. Evitar expulsión de revocaciones vigentes, validar las operaciones y cerrar admisión cuando Redis esté indisponible o exista pérdida detectada/integridad incierta. Diseñar recuperación y documentar el modelo de fallos y sus límites. No interpretar conexión/ping exitoso o restauración vacía como evidencia suficiente de integridad. Reconocer que una pérdida catastrófica silenciosa fuera de los mecanismos verificables de detección es riesgo residual que exige mitigación y verificación en infraestructura; no declarar protección absoluta ante ese escenario.

Propuesta de criterios verificables para esa revisión:

1. Demostrar Redis compartido, TTL/noeviction, ausencia de cachés negativas y ACL/configuración aplicable; no inferirlos por nombre de recurso.
2. Timeout/desconexión/operación incierta → 503 fail-closed; nunca 204 no confirmado ni emisión/acceso/cuotas sin control.
3. Ante reinicio, flush, restauración antigua o failover cuya pérdida se detecte o vuelva incierta la continuidad, activar no admisión compartida y detener emisión cuando corresponda antes de reabrir.
4. Recuperar solo con restauración confiable validada o cuarentena desde última emisión posible cubriendo vida restante/tolerancias; probar coordinación y reapertura, no solo flag local.
5. Describir qué pérdidas se detectan, fuentes de confianza, latencia/ventana de detección y casos no cubiertos. Si no se detecta un caso ensayado, registrarlo como limitación/riesgo pendiente, nunca test aprobado por omisión.
6. Antes de aceptar infraestructura, evaluar ACK/persistencia/failover/restauración/capacidad y mitigaciones del servicio elegido, con evidencia y responsable del riesgo residual. Un ping, marcador colocalizado o WAIT no cierra esa evaluación.

La revisión debe evaluar impacto en AUTH-022 y su garantía de cualquier control posterior, AUTH-025/026/028/039/042/043, spec §§2.3, 4.3, 6.2, AM-07, AC-15 y AD-07. No basta suavizar AUTH-027 dejando contradictorios los demás textos. No se crean nuevas versiones/IDs funcionales ni se modifica ningún criterio en esta entrega. Las partes compatibles (noeviction, TTL, fail-closed, sin testigo) permanecen documentadas, pero no autorizan implementar un requisito debilitado.

Solicitar autorización posterior delimitada para revisar/coherenciar el spec y aprobar el resultado, o aportar un diseño alternativo compatible con el criterio vigente. Hasta entonces, el spec prevalece y el plan no pasa a APROBADO PARA DESCOMPOSICIÓN EN TAREAS. Tampoco declarar finalizada aceptación técnica de la revocación por aprobar el ajuste documental.

</details>

**Resolución vigente APR-03 — AJUSTE APROBADO Y APLICADO; DISCREPANCIA NORMATIVA RESUELTA.** El responsable autoriza modificar solo spec/plan y los tres documentos de estado necesarios. Spec v0.3.0 ajusta AUTH-022/027/043, AC-15, AD-07, §4.3 paso 6, §6.2 y AM-07. Los otros 45 AUTH, incluidos AUTH-023/024/025/026/038/039, conservan íntegro su contenido. Los 48 AUTH, 21 AC, 14 AM y 12 AD mantienen IDs.

Se delimita la garantía: logout confirmado → rechazo en todas las instancias mientras exista continuidad confiable; pérdida/discontinuidad detectada, integridad incierta o Redis no verificable → fail-closed/503 y no admisión hasta recuperación validada o cuarentena suficiente. No emisión/204 sin controles verificables; considerar última emisión posible, skew, margen y solicitudes en curso. Noeviction/capacidad/monitoreo/acceso mínimo previenen pérdidas conocidas, sin exigir dos Redis físicos. La pérdida catastrófica completamente silenciosa no observable es riesgo residual de infraestructura a mitigar/evaluar, no eliminado.

No se agrega ledger MongoDB, segundo almacén duradero obligatorio o transacción distribuida. No interpretar un ping/marcador/persistencia/prueba finita como garantía absoluta ni ignorar señales disponibles. Pruebas futuras separan detección real y reacción a señal inyectada, con propagación y límites documentados.

La resolución permite **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**, no declara AUTH-027 implementado/satisfecho ni seguridad/infraestructura verificadas. Se conservan diferidos y sus puertas; memoria §15 registra el acto. La propuesta anterior permanece como evidencia histórica, no como bloqueo vigente ni requisito alternativo.

### 23.2. Registro de pendientes y puertas

Estas entradas son asuntos de decisión, no tareas ni autorizaciones. AD-09 a AD-12 del spec mantienen sus IDs/estados; este plan concreta selecciones sin declararlos cerrados integralmente.

| Asunto diferido | Puerta para resolverlo | PLD / AUTH |
| --- | --- | --- |
| Resolución normativa de pérdida silenciosa y textos relacionados — RESUELTA, registro de continuidad | APR-03 aplicada en spec v0.3.0 y este plan v0.2.0; no es bloqueo de descomposición. Controles/evidencias de infraestructura permanecen pendientes en la fila correspondiente | PLD-AUTH-08; AUTH-022/027/043 y relaciones de §23.1 |
| Versiones SDK/runtime/paquetes, grafo, licencias/vulnerabilidades | Antes de instalar/fijar dependencias; autorización técnica propia | PLD-AUTH-03; AUTH-013/018/044/045 |
| Topología MongoDB, journaling/ACK, concerns y APIs concretas | Antes de construir/aceptar persistencia y escoger entorno de prueba representativo | PLD-AUTH-05/06/20; AUTH-004/009/015/038/042 |
| Redis administrado, ACK/lecturas, continuidad, detección/fencing, recuperación, memoria/monitoreo y separación física | Diseño de infraestructura antes de aceptar garantías/operar; sin seleccionar recursos AWS aquí | PLD-AUTH-07/08/15/24; AUTH-022–030/037/046 |
| Precisión temporal, margen 60 s y coordinación de cambios/rotación JWT | Antes de aceptar validación/revocación; calibrar y demostrar fronteras | PLD-AUTH-09/10; AUTH-019/020/024 |
| Consumo de cuotas para entradas malformadas y precedencias de errores; topes de cardinalidad | Antes de construir cuota/MVC; sin modificar valores funcionales aprobados | PLD-AUTH-13/15/18/19; AUTH-008/028–031/040 |
| Rotación HMAC y revisión distribuida de políticas sin reset | Antes de rotar/cambiar política y de aceptar el control correspondiente | PLD-AUTH-16; AUTH-028/030/034 |
| Timeouts/concurrencia/perfil de carga, medición de percentiles/timing de login | Calibración en entorno autorizado antes de aceptación correspondiente; AD-09 | PLD-AUTH-18/20; AUTH-011/038/040 |
| Gestor AWS, retención/accesos/eliminación de PII/logs, exportación/alertas | Antes de despliegue productivo; AD-10/11, sin plazo/servicio inventado | PLD-AUTH-04/23; AUTH-019/032/034/036/037 |
| TLS/proxies/LB, redes, coste, readiness, Terraform y despliegue Docker en AWS | Incremento de infraestructura autorizado; AD-11 | PLD-AUTH-17/21/22/24; AUTH-033/037/046 |
| Tool versions, OWASP/rúbrica, política final de severidades/licencias/excepciones y evidencia OpenAPI | Antes de aceptar gates/evidencias y contrastar cumplimiento académico; AD-12 | PLD-AUTH-20/21; AUTH-040–045/047 |
| Endurecimiento .dockerignore y publish; identidad de artefacto/base | Antes del primer build sensible; trabajo posterior explícito, no diferible hasta después del build | PLD-AUTH-22; AUTH-034/044/045 |
| Sincronización de AGENTS/memoria/README y estado del plan — RESUELTA | Autorizada y aplicada por APR-03; historial anterior conservado | AUTH-001/047/048 |

No quedan contradicciones normativas identificadas tras APR-03. La separación física Redis no es requisito impuesto por el spec; namespaces + noeviction no se presentan como aislamiento completo. La ausencia de testigo MongoDB es coherente con AUTH-027 revisado y PLD-AUTH-08; no elimina el riesgo no observable ni demuestra controles ya ejecutados. La condicionalidad de la API de duplicados JSON no permite aceptar duplicados; la representación UUID/read-write concerns no cambia los siete campos ni los contratos.

## 24. Condiciones para generar task.md y cierre

Antes de generar `task.md` deben concurrir:

1. Resolución normativa de §23.1: **cumplida documentalmente por APR-03**, con spec como fuente funcional y referencias sincronizadas; no equivale a aceptación técnica.
2. Revisión coherente de este plan tras esa resolución: **cumplida documentalmente**; se registra **APROBADO PARA DESCOMPOSICIÓN EN TAREAS** por aprobación expresa condicionada a coherencia, no por terminar de escribirlo.
3. Tener una solicitud posterior delimitada para crear tareas; no generarlas en esta entrega ni bajo otro nombre.
4. Mantener visibles los demás diferidos con su puerta: no convertir candidatos/versiones/metas en decisiones finales tácitas. La descomposición futura identificará validaciones/revisiones previas al componente afectado, no autorizará omitirlas.
5. Cada futura tarea deberá enlazar AUTH/AC, PLD-AUTH, alcance/archivos, criterio/evidencia, dependencia y autorización; conservar no ejecución inicial. No reutilizar TBF como tareas de autenticación ni marcar foundation terminada.
6. Diferenciar crear/aprobar tareas de ejecutarlas: código, instalaciones, conexiones, build/pruebas/scans, workflows, infraestructura y despliegue requieren autorización técnica separada y entornos/objetivos aprobados.
7. Convertir endurecimiento Docker en trabajo explícito ANTES de un build con secretos locales, no en una nota opcional de cierre.

**Registro histórico de creación v0.1.0:** en aquella entrega solo se autorizó crear `specs/002-authentication/plan.md`, sin permiso de editar memoria. Se conservaron spec 002, constituciones, 001, documentos de estado, código/configuración e infraestructura; sin paquetes, tareas, código funcional, verificaciones técnicas, commit o push. El plan quedó PENDIENTE DE REVISIÓN NORMATIVA DE AUTH-027 — NO HABILITADO PARA DESCOMPOSICIÓN EN TAREAS. El antecedente y propuesta originales se conservan en §23.1; APR-03 resuelve ese bloqueo, no borra la entrega previa.

**Cierre vigente APR-03:** 25 decisiones PLD-AUTH registradas y trazabilidad documental de 48/48 AUTH; discrepancia normativa resuelta. Spec v0.3.0 conserva **APROBADO PARA PLANIFICACIÓN** y este plan v0.2.0 queda **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**. Se sincronizan exclusivamente los cinco documentos autorizados, manteniendo historial y decisiones diferidas. No se declara ningún requisito implementado ni satisfecho; no se agrega almacenamiento ni complejidad. Se detiene la intervención sin task.md, código, instalaciones, conexiones, workflows, commit o push. Esta aprobación solo permite crear posteriormente task.md en una entrega delimitada, no autoriza implementación.
