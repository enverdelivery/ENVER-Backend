# Tareas 002 — Autenticación de ENVER Backend

## 1. Identificación, fuentes y autorización

| Elemento | Valor |
| --- | --- |
| Solución / proyecto | `Enver.Backend` / `Enver.API` |
| Versión documental | 0.1.0 |
| Estado del documento | **APROBADO PARA EJECUCIÓN INCREMENTAL** |
| Catálogo vigente | 79 tareas: T-AUTH-001 a T-AUTH-079 |
| Estado inicial de todas las tareas | PENDIENTE / NO EJECUTADA |
| Cobertura documental prevista | AUTH 48/48; PLD-AUTH 25/25; AC 21/21 |
| Gates | 8; todos PENDIENTES / NO SATISFECHOS |
| Fuente de aprobación | SOL-TASK-002: aprobación expresa del catálogo compactado y autorización documental del responsable |
| Estado técnico | Autenticación NO IMPLEMENTADA / NO VERIFICADA por esta entrega; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO |

Fuentes, en su función normativa:

1. [Constitución canónica](../../.specify/memory/constitution.md), v1.0.0, y su [referencia raíz](../../constitution.md), sin autoridad independiente.
2. [AGENTS.md](../../AGENTS.md), [memoria.md](../../memoria.md) y [README.md](../../README.md): actuación, estado y antecedentes.
3. [Spec 002](spec.md), v0.3.0, **APROBADO PARA PLANIFICACIÓN**: fuente funcional; especialmente §§3–8 y 10.
4. [Plan 002](plan.md), v0.2.0, **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**: arquitectura, decisiones PLD-AUTH-01 a PLD-AUTH-25, incrementos §19 y puertas §23.2.
5. SOL-TASK-002 identifica únicamente la solicitud conversacional que aprueba estas 79 tareas y autoriza crear este archivo y sincronizar los tres documentos de estado. No es evidencia técnica, commit ni aprobación de los detalles diferidos.

Esta entrega es exclusivamente documental: no ejecuta ninguna tarea del catálogo. Se conservan intactos spec/plan, ambas referencias constitucionales y los [tres antecedentes de foundation](../001-backend-foundation/). Las restricciones de entregas anteriores se interpretan en su contexto histórico; esta autorización posterior no reforma la constitución, aprueba retrospectivamente 001 ni cierra FND/PLD/TBF/DEC. Memoria §16 registra esta entrega, sin borrar §§7–15.

**APROBADO PARA EJECUCIÓN INCREMENTAL** significa exclusivamente que el catálogo podrá ejecutarse una tarea o incremento a la vez mediante **autorización posterior** de alcance, archivos, acciones y entorno. No autoriza ejecución inmediata, instalaciones, conexiones, builds, pruebas, scanners, workflows, AWS, Terraform, commit o push; no satisface gates ni resuelve decisiones diferidas.

## 2. Reglas comunes y evidencia

Cada ficha es una unidad coherente de implementación/revisión y pruebas, normalmente uno o pocos commits relacionados; no una obligación de crear cada clase del árbol orientativo del plan. Los archivos esperados son alcance futuro, no archivos creados ni permisos otorgados aquí. Se conserva un único `Enver.API`, separación lógica y tres proyectos de pruebas; sin microservicios, proyectos extra de aplicación, CQRS, MediatR, AutoMapper, repositorio genérico, UnitOfWork artificial ni Identity completo.

Convenciones de fichas y matrices:

- Los números en **Dependencias** y en columnas de tareas remiten exclusivamente a T-AUTH-nnn de este catálogo. Los rangos incluyen ambos extremos y todos sus elementos; `—` significa ninguna dependencia de tarea, no ausencia de autorización.
- Los números en **AUTH relacionados**, **PLD-AUTH relacionados** y **AC relacionados** remiten, respectivamente, a AUTH-nnn, PLD-AUTH-nn y AC-nn de las fuentes. `—` en AC identifica preparación/gobierno sin escenario funcional directo.
- Las dependencias son prerrequisitos de tarea; también se exigen las condiciones de entrada, autorizaciones y puertas de §7. Las fases no son un orden numérico de ejecución.
- Las pruebas acompañan cada capacidad. Antes de F2 se especifican sus casos en la revisión de la capacidad y se materializan/ejecutan después con las suites disponibles. No se añade una dependencia circular desde F1 hacia fixtures que consumen sus Options.
- Las factories de F2 permiten composición incremental de módulos bajo prueba. No acreditan el pipeline productivo final ni autorizan publicar endpoints incompletamente protegidos. F12 integra el host en tres tramos serializados; F13 verifica el conjunto.
- No se inventan versiones, topes de cardinalidad, parámetros criptográficos, porcentajes de cobertura, SLA, RTO/RPO o servicios AWS. Los candidatos del plan conservan su necesidad de calibración/aprobación.

### Evidencia E(T-AUTH-nnn)

Cada ficha reserva su identificador **E(T-AUTH-nnn)**. Todos los resultados de ejecución están **PENDIENTES / SIN EVIDENCIA TÉCNICA GENERADA**. La definición de una prueba o salida esperada no constituye resultado obtenido.

| Campo de evidencia futura | Registro requerido cuando corresponda |
| --- | --- |
| Autorización y alcance | Responsable/acto, tarea/incremento, AUTH/PLD/AC y objetivos aprobados |
| Archivos | Rutas modificadas y revisión del cambio |
| Comandos | Comando realmente ejecutado, con secretos omitidos |
| Herramientas/versiones | SDK/runtime/paquetes/scanners/runner e imágenes/digests efectivos |
| Entorno | Ambiente, recursos, topología, aislamiento y representatividad |
| Pruebas ejecutadas | Suite, casos y referencia AC/amenaza; no solo tests escritos |
| Resultado esperado | Criterio normativo aplicable |
| Resultado obtenido | Resultado real, pendiente hasta ejecutar |
| Limitaciones, incidencias y tratamiento | Riesgos residuales, hallazgos, corrección o excepción autorizada con responsable/vencimiento |
| Commit | Identificador real del cambio cuando exista; no se crea aquí |
| Workflow run | URL/ID y artefactos reales cuando exista |
| Captura académica | Solo si la rúbrica lo requiere; segura y complementaria, no sustituto de resultados |

No se registran credenciales, JWT completos, hashes de contraseña, Authorization, signing keys, conexiones o cuerpos sensibles. Las evidencias se ubicarán en el soporte autorizado al ejecutar, sin crear ahora archivos extra ni atribuir a estas fichas resultados técnicos.

### Rutas abreviadas de archivos esperados

| Prefijo | Ruta relativa a la raíz del repositorio |
| --- | --- |
| API/ | `Enver.API/` |
| FA/ | `Enver.API/Features/Authentication/` |
| CFG/ | `Enver.API/Configuration/` |
| HTTP/ | `Enver.API/Http/` |
| SEC/ | `Enver.API/Security/` |
| RDS/ | `Enver.API/Infrastructure/Redis/` |
| MDB/ | `Enver.API/Infrastructure/MongoDB/` |
| OBS/ | `Enver.API/Observability/` |
| COMP/ | `Enver.API/Composition/` |
| UT/ | `tests/Enver.API.UnitTests/` |
| IT/ | `tests/Enver.API.IntegrationTests/` |
| ST/ | `tests/Enver.API.SecurityTests/` |

## 3. Catálogo de tareas

| Fase | Alcance | Tareas | Cantidad |
| --- | --- | --- | --- |
| F0 | Baseline | 001–006 | 6 |
| F1 | Configuración y HTTP | 007–012 | 6 |
| F2 | Infraestructura de pruebas | 013–015 | 3 |
| F3 | Redis | 016–022 | 7 |
| F4 | MongoDB | 023–027 | 5 |
| F5 | Validación | 028–030 | 3 |
| F6 | Password hashing | 031–033 | 3 |
| F7 | Register | 034–037 | 4 |
| F8 | JWT | 038–042 | 5 |
| F9 | Logout | 043–046 | 4 |
| F10 | Login | 047–051 | 5 |
| F11 | Rate limiting transversal | 052–055 | 4 |
| F12 | Pipeline | 056–059 | 4 |
| F13 | Regresión y seguridad integral | 060–068 | 9 |
| F14 | DevSecOps | 069–075 | 7 |
| F15 | Docker y cierre | 076–079 | 4 |
| Total | Catálogo vigente | 001–079 | 79 |

### F0 — Baseline

#### T-AUTH-001

- **Título:** Delimitar ejecución y baseline documental/Git.
- **Fase / gate:** F0 / GATE-A.
- **Objetivo:** Preparar una intervención técnica autorizada sin perder trabajo existente.
- **Dependencias:** —.
- **AUTH relacionados:** 001,047,048.
- **PLD-AUTH relacionados:** 01,25.
- **AC relacionados:** —.
- **Archivos esperados:** Registro de autorización y revisión de `AGENTS.md`, `memoria.md`, `README.md`, spec/plan/task; sin cambios automáticos a esas fuentes.
- **Salida verificable:** Alcance, comandos, componentes, entorno, rama/commit y cambios pendientes identificados; recomendar versionar baseline documental antes del inicio técnico.
- **Pruebas/evidencia requerida:** E(T-AUTH-001): revisión de autorización y estado Git de solo lectura; resultado real pendiente.
- **Riesgos/notas:** No stash/reset, descarte, commit o push implícitos. Esta tarea tampoco se ejecuta por crear el catálogo.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-002

- **Título:** Evaluar SDK, dependencias y riesgos actuales.
- **Fase / gate:** F0 / GATE-A; soporte GATE-H.
- **Objetivo:** Conocer compatibilidad .NET 10 y riesgos directos/transitivos antes de seleccionar versiones.
- **Dependencias:** 001.
- **AUTH relacionados:** 013,018,041,044,045.
- **PLD-AUTH relacionados:** 03,21.
- **AC relacionados:** 21.
- **Archivos esperados:** `API/Enver.API.csproj`, inventario y reporte de evaluación en soporte autorizado.
- **Salida verificable:** SDK/runtime/grafo, vulnerabilidades y licencias inventariados; aviso OpenAPI contrastado con informe verificable, no inferido por versión.
- **Pruebas/evidencia requerida:** E(T-AUTH-002): comandos de inventario/auditoría expresamente autorizados, herramientas/fecha/resultado y tratamiento.
- **Riesgos/notas:** Restauraciones o accesos de red implícitos requieren alcance autorizado; dependencias de plantilla no son selecciones definitivas.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-003

- **Título:** Fijar versiones y restore reproducible.
- **Fase / gate:** F0 / GATE-A; soporte GATE-H.
- **Objetivo:** Establecer una base compatible, revisada y reproducible.
- **Dependencias:** 002.
- **AUTH relacionados:** 013,018,041–045.
- **PLD-AUTH relacionados:** 03,20,21.
- **AC relacionados:** 21.
- **Archivos esperados:** `global.json`, `API/Enver.API.csproj`, política y lockfiles de los proyectos cuando se incorporen dependencias.
- **Salida verificable:** Matriz aprobada SDK/NuGet/runner/imágenes, licencias/grafo y estrategia de versiones exactas/restore bloqueado; paquetes por nombre conforme plan §5.
- **Pruebas/evidencia requerida:** E(T-AUTH-003): revisión de compatibilidad .NET 10 y reproducibilidad al autorizar restore; lockfiles actualizados por cada capacidad consumidora.
- **Riesgos/notas:** Sin referencias redundantes, versiones transitivas forzadas sin justificación ni incorporación tácita de herramientas candidatas.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-004

- **Título:** Verificar higiene Git y secretos versionados.
- **Fase / gate:** F0 / GATE-A; soporte GATE-H.
- **Objetivo:** Evitar versionar configuración sensible y artefactos locales.
- **Dependencias:** 001.
- **AUTH relacionados:** 034,044,045.
- **PLD-AUTH relacionados:** 21,22.
- **AC relacionados:** 19,21.
- **Archivos esperados:** `.gitignore` y registro seguro del alcance versionado.
- **Salida verificable:** Rutas sintéticas sensibles/generadas ignoradas y fuentes/SDD versionables; identificar material ya versionado sin borrarlo ni reescribir historial automáticamente.
- **Pruebas/evidencia requerida:** E(T-AUTH-004): comprobaciones de reglas/rutas y revisión acotada; secret scanning solo bajo autorización y política aplicable.
- **Riesgos/notas:** Ignorar no retira secretos del historial; una exposición requiere tratamiento/rotación. No certificar historial o binarios no revisados.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-005

- **Título:** Proteger contexto Docker y publicación.
- **Fase / gate:** F0 / GATE-A; condición previa de GATE-H.
- **Objetivo:** Impedir que secretos locales entren al contexto, publish o artefactos.
- **Dependencias:** 003,004.
- **AUTH relacionados:** 032,034,044,045.
- **PLD-AUTH relacionados:** 22.
- **AC relacionados:** 19,21.
- **Archivos esperados:** `.dockerignore`, `API/Enver.API.csproj` y sentinelas sintéticas en entorno de prueba autorizado.
- **Salida verificable:** Excluir `.env*`, variantes appsettings locales, secrets, certificados/claves, `.git/.vs`, bin/obj, resultados/cobertura y estado/variables Terraform pertinentes; retirar excepciones que reintroducen `.git`; proteger contenido/publish MSBuild.
- **Pruebas/evidencia requerida:** E(T-AUTH-005): revisión de contexto/contenido y sentinelas sin secretos reales, antes de cualquier build sensible; inspección final en 078.
- **Riesgos/notas:** `.gitignore` no protege Docker ni publish. No diferir esta protección hasta después de construir imágenes.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-006

- **Título:** Consolidar estructura y checkpoint técnico.
- **Fase / gate:** F0 / GATE-A.
- **Objetivo:** Delimitar responsabilidades y una baseline limpia para incrementos posteriores.
- **Dependencias:** 003–005.
- **AUTH relacionados:** 001,002,035,041,047,048.
- **PLD-AUTH relacionados:** 01,02,20,25.
- **AC relacionados:** 21.
- **Archivos esperados:** Mapa de `API/`, `tests/` y propietarios de archivos compartidos; registro de baseline en soporte autorizado.
- **Salida verificable:** Un API/tres proyectos futuros de tests, dependencias interiores y archivos de edición serializada definidos; working tree limpio tras commits expresamente autorizados.
- **Pruebas/evidencia requerida:** E(T-AUTH-006): revisión de estructura, alcance y estado Git; no regenerar solución/artefactos existentes.
- **Riesgos/notas:** Limpieza no se obtiene descartando trabajo. No exige un commit por tarea ni incorpora patrones/proyectos excluidos.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F1 — Configuración y HTTP

#### T-AUTH-007

- **Título:** Configurar Options de persistencia y Redis.
- **Fase / gate:** F1 / GATE-A.
- **Objetivo:** Centralizar configuración tipada de dependencias y aislamiento.
- **Dependencias:** 006.
- **AUTH relacionados:** 015,025–027,034,038.
- **PLD-AUTH relacionados:** 04,05,07,08.
- **AC relacionados:** 14,15,17,20.
- **Archivos esperados:** `CFG/MongoDbOptions.cs`, `CFG/RedisOptions.cs`, `API/appsettings*.json` sin secretos.
- **Salida verificable:** Base/colección, namespaces, concerns candidatos, timeouts, retención y admisión coherentes; conexiones/credenciales exclusivamente externas.
- **Pruebas/evidencia requerida:** E(T-AUTH-007): casos de valores ausentes/incoherentes y aislamiento; materialización con F2/008 y regresión 060.
- **Riesgos/notas:** Declarar Options no demuestra disponibilidad, integridad ni aprobación de topología/ACK; se preservan puertas de §7.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-008

- **Título:** Configurar y validar Options de autenticación.
- **Fase / gate:** F1 / GATE-A.
- **Objetivo:** Rechazar configuración insegura antes de admitir tráfico dependiente.
- **Dependencias:** 007.
- **AUTH relacionados:** 005–008,019,020,029,034,038.
- **PLD-AUTH relacionados:** 04,10,13,15,18.
- **AC relacionados:** 04,10,16,18,20.
- **Archivos esperados:** `CFG/JwtOptions.cs`, `AuthenticationOptions.cs`, `RateLimitingOptions.cs`, `AuthenticationOptionsValidation.cs`, `API/appsettings*.json`.
- **Salida verificable:** IValidateOptions/ValidateOnStart para las cinco Options; HS256/issuer/audience/vida/skew/kids/claves válidos, límites/presupuestos coherentes, siete cuotas configurables y secretos externos.
- **Pruebas/evidencia requerida:** E(T-AUTH-008): configuración inválida impide inicio/admisión; User Secrets/variables sintéticas sin volcado de valores; pruebas Options en UT y 060.
- **Riesgos/notas:** Longitud Base64 no acredita entropía; no recargar claves/skew/cuotas descoordinadamente ni inventar topes pendientes.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-009

- **Título:** Implementar presupuestos y cancelación.
- **Fase / gate:** F1 / GATE-A; transversal.
- **Objetivo:** Acotar trabajo y distinguir cancelación, timeout e incertidumbre.
- **Dependencias:** 007,008.
- **AUTH relacionados:** 008,026,038,040.
- **PLD-AUTH relacionados:** 18.
- **AC relacionados:** 14,17,18.
- **Archivos esperados:** Auxiliares de presupuesto en `HTTP/` o `FA/Application/`, pruebas en `UT/`.
- **Salida verificable:** Presupuesto restante/cancelaciones enlazados; candidatos register/login 8 s, logout 4 s, Mongo 2 s, Redis 750 ms y conexión 2 s configurables, nunca SLA.
- **Pruebas/evidencia requerida:** E(T-AUTH-009): casos de agotamiento, cancelación del cliente, operación incierta y ausencia de reintento/espera ilimitados.
- **Riesgos/notas:** Timeout no prueba ausencia de efecto; cancelar no interrumpe mágicamente PasswordHasher síncrono. Calibración AD-09 en 067.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-010

- **Título:** Unificar errores, correlación y no-store.
- **Fase / gate:** F1 / GATE-A; transversal.
- **Objetivo:** Mantener semántica HTTP segura en todos los caminos de respuesta.
- **Dependencias:** 009.
- **AUTH relacionados:** 011,023,026,031,032,036,037,038.
- **PLD-AUTH relacionados:** 14,19,23.
- **AC relacionados:** 07,13,14,19.
- **Archivos esperados:** `HTTP/ApiProblemDetails.cs`, `ApiExceptionHandler.cs`, `RequestContextMiddleware.cs`, `UT/`, `IT/ProblemDetailsTests.cs`.
- **Salida verificable:** RFC 9457/type about:blank/title/status/detail/code/traceId, errors solo por campo sin valores, sin instance; códigos del plan §12, correlación validada y no-store también en éxitos. Preparar integración MVC/challenge/autorización.
- **Pruebas/evidencia requerida:** E(T-AUTH-010): 400/401/409/413/415/429/500/503, cancelación y respuesta iniciada; 204 vacío y sin segunda escritura.
- **Riesgos/notas:** Handler de excepciones no cubre por sí solo MVC/challenge/cuotas; no exponer infraestructura, stack traces, secretos ni existencia/estado.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-011

- **Título:** Implementar frontera HTTP segura.
- **Fase / gate:** F1 / GATE-A.
- **Objetivo:** Rechazar entradas ambiguas o excesivas antes de trabajo costoso.
- **Dependencias:** 008–010.
- **AUTH relacionados:** 003,008,018,023,030,035,040.
- **PLD-AUTH relacionados:** 13,14,18,19.
- **AC relacionados:** 05,09,13,18.
- **Archivos esperados:** `HTTP/AuthenticationRequestLimitsMiddleware.cs`, configuración System.Text.Json/MVC y pruebas `UT/IT/ST`.
- **Salida verificable:** UTF-8/camelCase exactos, profundidad 4, sin comentarios/coerción/comas finales; desconocidas/duplicadas/null/tipos inválidos rechazados. Cuerpo real ≤16 KiB también chunked; 413/415; logout no vacío, incluso whitespace, 400. Authorization única/JWT ≤4 KiB antes de criptografía y sin canales alternativos.
- **Pruebas/evidencia requerida:** E(T-AUTH-011): fronteras, charset/encoding, duplicados y cuerpos; JWT excesivo 401 API, distinto del posible rechazo de headers del borde.
- **Riesgos/notas:** Si la API nativa de duplicados no sirve, comprobación equivalente acotada, nunca aceptar duplicados; no confiar solo en Content-Length.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-012

- **Título:** Coordinar validación HTTP y precedencia de errores.
- **Fase / gate:** F1 / GATE-A; entrada GATE-B.
- **Objetivo:** Definir rechazo y consumo de cuotas sin omisiones o cargos duplicados.
- **Dependencias:** 010,011.
- **AUTH relacionados:** 008,028–031,035,040.
- **PLD-AUTH relacionados:** 02,13–15,18,19.
- **AC relacionados:** 05,14,16,18.
- **Archivos esperados:** Resultados propios en `FA/Validation/`, coordinación MVC en `HTTP/`, matriz de precedencia en soporte autorizado.
- **Salida verificable:** Decisión aprobada para JSON ilegible/email no normalizable/defectos múltiples y 400–429–503; errores por campo seguros, sin buckets arbitrarios ni dependencia de existencia de usuario.
- **Pruebas/evidencia requerida:** E(T-AUTH-012): casos con colaboradores controlados; 400 automático MVC no omite un control asíncrono requerido; integración final en 058.
- **Riesgos/notas:** No requiere el normalizador funcional posterior para definir el contrato; no cambiar estados ni cuotas aprobados.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F2 — Infraestructura de pruebas

#### T-AUTH-013

- **Título:** Establecer los tres proyectos de pruebas.
- **Fase / gate:** F2 / GATE-A.
- **Objetivo:** Disponer de suites independientes y descubiertas por el runner.
- **Dependencias:** 003,006.
- **AUTH relacionados:** 041–043.
- **PLD-AUTH relacionados:** 01,03,20.
- **AC relacionados:** 21.
- **Archivos esperados:** `tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj`, `tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj`, `tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj`, `Enver.Backend.slnx`.
- **Salida verificable:** Tres proyectos .NET 10 con referencias mínimas, conservando solución y sin cuarto proyecto auxiliar.
- **Pruebas/evidencia requerida:** E(T-AUTH-013): descubrimiento independiente de pruebas mínimas, ejecutado solo en incremento autorizado; no falso verde por cero tests.
- **Riesgos/notas:** No recrear archivos de Visual Studio ni agregar proyectos extra de aplicación.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-014

- **Título:** Configurar dependencias y utilidades de pruebas.
- **Fase / gate:** F2 / GATE-A.
- **Objetivo:** Mantener pruebas deterministas y dependencias fuera de producción.
- **Dependencias:** 013.
- **AUTH relacionados:** 039,041–045.
- **PLD-AUTH relacionados:** 03,20.
- **AC relacionados:** 10,21.
- **Archivos esperados:** Proyectos/lockfiles en `tests/`, fakes pequeños en `UT/`, fuentes auxiliares en `tests/Shared/` cuando aporten valor.
- **Salida verificable:** Test SDK/xUnit/runner, MVC Testing y módulos Testcontainers según suite; assets privados y tiempo controlable con TimeProvider sin Moq.
- **Pruebas/evidencia requerida:** E(T-AUTH-014): grafo/licencias/versiones aprobadas, determinismo temporal y ausencia de dependencias de pruebas en publish.
- **Riesgos/notas:** No fijar porcentaje de cobertura ni sleeps arbitrarios; compartir fuentes no exige otro ensamblado.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-015

- **Título:** Preparar contenedores y factories de hosts.
- **Fase / gate:** F2 / GATE-A; soporte GATE-B/C/G.
- **Objetivo:** Probar dependencias reales y alternancia entre instancias aisladas.
- **Dependencias:** 005,007,008,014.
- **AUTH relacionados:** 009,022,025–028,038,042,043.
- **PLD-AUTH relacionados:** 05,07,08,20.
- **AC relacionados:** 03,11,14–17.
- **Archivos esperados:** `tests/Shared/AuthenticationApiFactory.cs`, `AuthenticationDependenciesFixture.cs`, proyectos IT/ST y lockfiles.
- **Salida verificable:** MongoDB/Redis Testcontainers con imágenes revisadas, replica set donde aplique, noeviction, disponibilidad reproducible, secretos sintéticos y base/namespace por ejecución; dos hosts independientes compartiendo dependencias.
- **Pruebas/evidencia requerida:** E(T-AUTH-015): arranque, aislamiento, limpieza limitada a datos propios y composición incremental de módulos.
- **Riesgos/notas:** Docker autorizado; fixtures son soporte, no prueban aún majority/failover productivos ni dos instancias AWS/LB. No agregar endpoint productivo para tests.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F3 — Redis

#### T-AUTH-016

- **Título:** Integrar conexión Redis y namespaces seguros.
- **Fase / gate:** F3 / GATE-B.
- **Objetivo:** Reutilizar conectividad acotada y separar ambientes/finalidades.
- **Dependencias:** 007,009,015.
- **AUTH relacionados:** 025,026,028,032,034,038.
- **PLD-AUTH relacionados:** 03,07,16,18.
- **AC relacionados:** 14,19,21.
- **Archivos esperados:** `API/Enver.API.csproj`, lockfile, `RDS/RedisConnections.cs`, constructor de claves y pruebas IT/UT.
- **Salida verificable:** StackExchange.Redis aprobado, multiplexer singleton por destino, reconexión/backlog acotados, namespaces por ambiente/propósito; clave de revocación conserva `enver:{environment}:auth:revoked:{issuer}:{jti}`.
- **Pruebas/evidencia requerida:** E(T-AUTH-016): reutilización, caída/timeout y aislamiento; sin conexión por request, JWT completo o PII innecesaria en claves.
- **Riesgos/notas:** Sin exigir dos Redis físicos, fire-and-forget o réplicas obsoletas; namespaces no aíslan memoria/evicción.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-017

- **Título:** Implementar revocación atómica y retención.
- **Fase / gate:** F3 / GATE-B; soporte GATE-F.
- **Objetivo:** Confirmar marcas con TTL suficiente y no decreciente.
- **Dependencias:** 016,019.
- **AUTH relacionados:** 020,022–026,039,042.
- **PLD-AUTH relacionados:** 02,07–09,20.
- **AC relacionados:** 10–14.
- **Archivos esperados:** `FA/Application/Ports/ITokenRevocationStore.cs`, `RDS/RedisTokenRevocationStore.cs`, scripts pequeños versionados `RevokeToken.lua`/`CheckRevocation.lua` cuando corresponda, UT/IT.
- **Salida verificable:** Lectura primaria sin caché negativa; crear/verificar marca atómicamente conservando mayor expiración absoluta. Retención exp+skew+margen, candidato 60 s configurable, redondeo conservador; sin marcas eternas ni extensión de JWT.
- **Pruebas/evidencia requerida:** E(T-AUTH-017): primera/repetida/concurrente, respuesta perdida, fronteras/reloj y resultado incierto; solo entradas autenticadas en uso final.
- **Riesgos/notas:** ACK/lecturas aprobados en 019 antes de construir; ACK/read-back/WAIT no prueban durabilidad universal ni autorizan 204 incierto.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-018

- **Título:** Implementar admisión confiable y fail-closed.
- **Fase / gate:** F3 / GATE-B; soporte GATE-G.
- **Objetivo:** Denegar operaciones dependientes cuando el estado no sea verificable.
- **Dependencias:** 009,016.
- **AUTH relacionados:** 026,027,038.
- **PLD-AUTH relacionados:** 08,14,18,24.
- **AC relacionados:** 14,15.
- **Archivos esperados:** `FA/Application/Ports/IAuthenticationSecurityState.cs`, `RDS/RedisSecurityStateGuard.cs`, UT/IT.
- **Salida verificable:** Estados/transiciones conservadores de admisión, traducción operativa segura y 503; nunca tratar lectura incierta como no revocado o credenciales inválidas.
- **Pruebas/evidencia requerida:** E(T-AUTH-018): transiciones, indisponibilidad, incertidumbre y ausencia de bypass; señal simulada acredita reacción, no detector.
- **Riesgos/notas:** AUTH-027 NO IMPLEMENTADO / NO SATISFECHO; un flag local no acredita coordinación global ni integridad.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-019

- **Título:** Implementar detección según modelo de fallos aprobado.
- **Fase / gate:** F3 / GATE-B; soporte GATE-G.
- **Objetivo:** Conectar señales reales y explicitar sus límites de detección.
- **Dependencias:** 018.
- **AUTH relacionados:** 022,025–027,038,043.
- **PLD-AUTH relacionados:** 08,20,24.
- **AC relacionados:** 15.
- **Archivos esperados:** Adaptadores de señales en `RDS/`, casos IT/ST y registro del modelo en soporte autorizado.
- **Salida verificable:** Entrada obligatoria: modelo/topología de pruebas, ACK/lecturas, fuentes de confianza, límites/propagación aprobados. Detectores implementados para señales reales disponibles, sin ignorarlas para clasificar un fallo como no observable.
- **Pruebas/evidencia requerida:** E(T-AUTH-019): evento provocado, componente/momento de detección, latencia/propagación y limitaciones; contraste integral en 062/068.
- **Riesgos/notas:** AUTH-027 NO IMPLEMENTADO / NO SATISFECHO; ping, persistencia o único marcador colocalizado no acreditan integridad. No detección universal ni ledger MongoDB.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-020

- **Título:** Coordinar cierre, emisión y recuperación segura.
- **Fase / gate:** F3 / GATE-B; soporte GATE-G.
- **Objetivo:** Evitar reapertura insegura tras pérdida detectada o confianza incierta.
- **Dependencias:** 017,019.
- **AUTH relacionados:** 022,024,026–028,038,039.
- **PLD-AUTH relacionados:** 08,09.
- **AC relacionados:** 15,20.
- **Archivos esperados:** Guard/coordinación/recuperación aprobados en `RDS/`, puerto de estado y pruebas UT/IT.
- **Salida verificable:** No admisión compartida y freno de emisión cuando proceda; restauración confiable validada o cuarentena desde última emisión posible, con hosts/solicitudes en vuelo, skew/margen y cambios de reloj/configuración. Sin prueba de cese/reapertura segura, mantener cierre.
- **Pruebas/evidencia requerida:** E(T-AUTH-020): propagación, emisión en vuelo y ausencia de reapertura prematura; 990 s solo horizonte candidato con 900+30+60, no RTO ni temporizador desde primera alerta.
- **Riesgos/notas:** AUTH-027 NO IMPLEMENTADO / NO SATISFECHO; no segundo almacén duradero obligatorio ni transacciones distribuidas; confianza no reside únicamente en el estado perdido.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-021

- **Título:** Implementar el almacén distribuido de cuotas.
- **Fase / gate:** F3 / GATE-B.
- **Objetivo:** Consumir conjuntamente cuotas Redis con recursos acotados.
- **Dependencias:** 012,016,052–054.
- **AUTH relacionados:** 028–030,038–040.
- **PLD-AUTH relacionados:** 02,15,16,18.
- **AC relacionados:** 14,16,18.
- **Archivos esperados:** `FA/Application/Ports/IAuthenticationQuotaStore.cs`, `RDS/RedisAuthenticationQuotaStore.cs`, `Scripts/ConsumeAuthenticationQuota.lua`, UT/IT.
- **Salida verificable:** Una sola implementación del contrato 052 con tiempo Redis/atomicidad, ámbitos HMAC/origen de 053 y topes/recuperación de 054; admitido/rechazado/incierto, sin cargos parciales ni fallback local.
- **Pruebas/evidencia requerida:** E(T-AUTH-021): script versionado valida tipos/argumentos antes de mutar, concurrencia y timeout incierto; sin reintento ciego.
- **Riesgos/notas:** F11 se adelanta por dependencia; no asumir atomicidad cross-slot, crear claves ilimitadas o usar claims no autenticados para logout.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-022

- **Título:** Verificar Redis distribuido y protección operativa.
- **Fase / gate:** F3 / GATE-B; soporte GATE-G.
- **Objetivo:** Contrastar controles compartidos y su alcance operativo real.
- **Dependencias:** 015,017,020,021,055.
- **AUTH relacionados:** 022–030,038,039,042,043.
- **PLD-AUTH relacionados:** 07–09,15,20.
- **AC relacionados:** 11,12,14–16,18.
- **Archivos esperados:** IT/ST Redis, fixture/configuración autorizada y reporte seguro de alcance.
- **Salida verificable:** Dos hosts, noeviction efectivo, memoria/capacidad, permisos/TLS aplicables, fallos sin bypass y propagación verificados; sin afirmar aislamiento físico por namespaces ni garantías productivas por tests locales.
- **Pruebas/evidencia requerida:** E(T-AUTH-022): pruebas reales de cuotas/revocación/operación incierta y presión de memoria; módulos de test sin publicar endpoints; casos JWT finales en 046/062/063.
- **Riesgos/notas:** AUTH-027 NO IMPLEMENTADO / NO SATISFECHO; toda pérdida completamente silenciosa no observable sigue residual; operación del servicio futuro requiere 068/infraestructura.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F4 — MongoDB

#### T-AUTH-023

- **Título:** Integrar MongoDB y modelo BSON.
- **Fase / gate:** F4 / GATE-C.
- **Objetivo:** Establecer persistencia tipada conforme al contrato mínimo aprobado.
- **Dependencias:** 022.
- **AUTH relacionados:** 002,004,007,012,014,015,034,038.
- **PLD-AUTH relacionados:** 03–05,20.
- **AC relacionados:** 01,05,08,17,21.
- **Archivos esperados:** Proyecto/lockfile, `FA/Core/User.cs`, `UserStatus.cs`, `MDB/UserDocument.cs`, configuración MongoClient y pruebas BSON.
- **Salida verificable:** MongoDB.Driver aprobado y cliente singleton; entrada: topología/concerns/journaling/ACK aprobados. Primary y majority según garantía; UUIDv4 BSON subtype 4/GuidRepresentation.Standard. Solo `_id/fullName/normalizedEmail/passwordHash/status/createdAt/updatedAt`, Active inicial, UTC iguales al crear.
- **Pruebas/evidencia requerida:** E(T-AUTH-023): round-trip/inspección BSON, concerns efectivos y aislamiento; replica set donde corresponda.
- **Riesgos/notas:** Sin hashVersion, roles, tokens o correo verificado; no elegir sharding ni inferir garantías Atlas desde standalone.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-024

- **Título:** Implementar UserStore y resultados seguros.
- **Fase / gate:** F4 / GATE-C.
- **Objetivo:** Leer/escribir usuarios con confirmación y fallos explícitos.
- **Dependencias:** 009,023.
- **AUTH relacionados:** 004,005,009,010,015,031,038,039.
- **PLD-AUTH relacionados:** 02,05,14,18.
- **AC relacionados:** 01–03,07,08,17.
- **Archivos esperados:** `FA/Application/Ports/IUserStore.cs`, `MDB/MongoUserStore.cs`, UT/IT.
- **Salida verificable:** Búsqueda por igualdad construida por backend; inserción confirmada/incierta, duplicate key específico normalizedEmail y timeouts/retryable writes acotados según topología/versión.
- **Pruebas/evidencia requerida:** E(T-AUTH-024): errores reales/traducidos, lectura tras creación y ACK perdido; fallo operativo no significa usuario inexistente.
- **Riesgos/notas:** Sin caché de credenciales, consultas del cliente, reintento con otra identidad o borrado compensatorio; no todo duplicate key es conflicto de email.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-025

- **Título:** Garantizar índice e inicialización idempotente.
- **Fase / gate:** F4 / GATE-C.
- **Objetivo:** Impedir registros sin unicidad válida.
- **Dependencias:** 023.
- **AUTH relacionados:** 004,005,009,015,038.
- **PLD-AUTH relacionados:** 05,06,24.
- **AC relacionados:** 03,17.
- **Archivos esperados:** `MDB/MongoUsersInitializer.cs`, definición del índice y IT.
- **Salida verificable:** Índice normalizedEmail ascendente/único/collation simple binaria, no sparse/partial; comprobar opciones, no solo nombre. Inicialización repetible; incompatibilidad/duplicados/falta de índice bloquean registro.
- **Pruebas/evidencia requerida:** E(T-AUTH-025): creación/verificación repetida, índice incorrecto y datos incompatibles; no duplicar índice _id.
- **Riesgos/notas:** No reparar/borrar/reconciliar datos automáticamente; identidad de inicialización de mínimo privilegio a infraestructura.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-026

- **Título:** Implementar actualización CAS de credenciales.
- **Fase / gate:** F4 / GATE-C; soporte GATE-E.
- **Objetivo:** Evitar sobrescritura concurrente de hash o estado.
- **Dependencias:** 023,024.
- **AUTH relacionados:** 012–014,039.
- **PLD-AUTH relacionados:** 05,12.
- **AC relacionados:** 06,08.
- **Archivos esperados:** Puerto IUserStore y `MDB/MongoUserStore.cs`, IT de CAS.
- **Salida verificable:** Actualización atómica condicionada por _id/passwordHash anterior/status correspondiente; solo passwordHash/updatedAt, sin campo extra de versión.
- **Pruebas/evidencia requerida:** E(T-AUTH-026): coincidencia, conflicto, cambio de estado/hash y actualización simultánea; resultado incierto explícito.
- **Riesgos/notas:** Esta capacidad persiste CAS; la decisión de relectura/credenciales y emisión pertenece a 032/048, sin duplicar lógica en el driver.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-027

- **Título:** Verificar persistencia MongoDB real.
- **Fase / gate:** F4 / GATE-C.
- **Objetivo:** Demostrar las garantías de persistencia en topología representativa.
- **Dependencias:** 015,024–026.
- **AUTH relacionados:** 004,009,013–015,038,039,042.
- **PLD-AUTH relacionados:** 05,06,12,20.
- **AC relacionados:** 01,03,06,17.
- **Archivos esperados:** IT de BSON/índice/inicialización, concurrencia y escrituras inciertas; fixture replica set según alcance.
- **Salida verificable:** Máximo una cuenta por email equivalente, creación confirmada observable y CAS sin sobrescrituras; modelo/índice reales inspeccionados.
- **Pruebas/evidencia requerida:** E(T-AUTH-027): MongoDB real, carrera entre hosts/clientes, lectura/escritura/ACK perdido y topología/versiones registradas.
- **Riesgos/notas:** Sin exactly-once; contenedor local no acredita Atlas ni failover productivo. Login funcional se verifica después.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F5 — Validación

#### T-AUTH-028

- **Título:** Implementar normalización y validación de identidad.
- **Fase / gate:** F5 / GATE-D.
- **Objetivo:** Aplicar una identidad normalizada coherente en registro, acceso e índices/cuotas.
- **Dependencias:** 012,027,029.
- **AUTH relacionados:** 002,005,007,008,031.
- **PLD-AUTH relacionados:** 02,13,14.
- **AC relacionados:** 01–03,05.
- **Archivos esperados:** `FA/Validation/InstitutionalEmailNormalizer.cs`, `AuthenticationInputValidator.cs`, UT.
- **Salida verificable:** Email trim externo/minúsculas invariantes/ASCII, dominio exacto uttt.edu.mx, límites 254/64 bytes y sintaxis no entrecomillada aprobada; sin quitar puntos/+. Nombre trim externo/no vacío/≤200 escalares/sin controles, conservando case/acentos/espacios internos.
- **Pruebas/evidencia requerida:** E(T-AUTH-028): fronteras, listas/comentarios/display names, puntos inválidos, subdominios/sufijos y equivalencia de normalización.
- **Riesgos/notas:** No MailAddress como única validación, prueba de titularidad del buzón o flag de verificación; cuotas consumen este mismo valor normalizado.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-029

- **Título:** Implementar políticas de contraseña y límites Unicode.
- **Fase / gate:** F5 / GATE-D.
- **Objetivo:** Validar secretos y escalares sin transformaciones ni Unicode ambiguo.
- **Dependencias:** 008,014,027.
- **AUTH relacionados:** 006,008,010,040,041.
- **PLD-AUTH relacionados:** 04,12,13.
- **AC relacionados:** 04,05,18.
- **Archivos esperados:** Auxiliares Rune/UTF-8 y validadores de password en `FA/Validation/`, UT.
- **Salida verificable:** Unicode inválido rechazado sin sustitución; registro 12–128 escalares/≤512 bytes UTF-8; login obligatorio/con límites superiores separado de futuros mínimos de creación. Controles rechazados según la regla del campo, sin inventar composición de password.
- **Pruebas/evidencia requerida:** E(T-AUTH-029): 11/12/128/129, 512/513 bytes, escalares suplementarios, surrogates inválidos y secreto íntegro.
- **Riesgos/notas:** Nunca trim, normalización Unicode, cambio de case o truncamiento de password; no confundir UTF-16 con valores escalares.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-030

- **Título:** Verificar fronteras e identidad maliciosa.
- **Fase / gate:** F5 / GATE-D; soporte GATE-G.
- **Objetivo:** Contrastar reglas de validación con casos positivos, negativos y abusivos.
- **Dependencias:** 011,015,028,029.
- **AUTH relacionados:** 005–008,040,041,043.
- **PLD-AUTH relacionados:** 13,20.
- **AC relacionados:** 02,04,05,18.
- **Archivos esperados:** `UT/ValidationTests.cs`, `ST/MaliciousInputTests.cs`.
- **Salida verificable:** Matriz completa de fronteras de nombre/email/password, Unicode/bytes y sintaxis; misma normalización sin transformaciones indebidas o aceptación silenciosa de datos inválidos.
- **Pruebas/evidencia requerida:** E(T-AUTH-030): unitarios y entradas maliciosas con datos sintéticos; conjunto HTTP final ampliado en 065, sin duplicar esta suite.
- **Riesgos/notas:** Rechazo debe ocurrir sin escritura/emisión y con coste acotado; tests del validador no acreditan por sí solos límites de transporte.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F6 — Password hashing

#### T-AUTH-031

- **Título:** Implementar hashing y dummy preinicializado.
- **Fase / gate:** F6 / GATE-D; soporte GATE-E.
- **Objetivo:** Proteger/verificar secretos mediante el hasher mantenido del framework.
- **Dependencias:** 023,029.
- **AUTH relacionados:** 006,010,011,013,014,041.
- **PLD-AUTH relacionados:** 03,12,20.
- **AC relacionados:** 04,06,07.
- **Archivos esperados:** `FA/Application/Ports/IPasswordService.cs`, `SEC/PasswordService.cs`, `DummyPasswordHash.cs`, `UT/PasswordServiceTests.cs`.
- **Salida verificable:** PasswordHasher<User> administra salt/formato/parámetros/versionado; dummy generado una vez por proceso con mismo hasher/configuración, listo antes de login, nunca por request.
- **Pruebas/evidencia requerida:** E(T-AUTH-031): éxito/fallo/SuccessRehashNeeded, mismo secreto con salts distintos, preservación exacta y una sola inicialización dummy; registrar configuración efectiva sin hashes.
- **Riesgos/notas:** Sin criptografía propia, iteraciones manuales, cifrado reversible o Identity completo; no sleeps/jitter como defensa principal ni tiempos idénticos prometidos.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-032

- **Título:** Resolver rehash concurrente de forma segura.
- **Fase / gate:** F6 / GATE-E.
- **Objetivo:** Actualizar hashes obsoletos sin perder cambios concurrentes o sostener credenciales inválidas.
- **Dependencias:** 026,031.
- **AUTH relacionados:** 010,012–014,039,042.
- **PLD-AUTH relacionados:** 12,20.
- **AC relacionados:** 06,08.
- **Archivos esperados:** Coordinación en `SEC/PasswordService.cs` o `FA/Application/AuthenticationService.cs` según responsabilidad, pruebas UT/IT.
- **Salida verificable:** SuccessRehashNeeded usa CAS; ante no coincidencia, relectura/estado/credenciales comprobados dentro de presupuesto antes de decidir. Rehash persistido cambia updatedAt; login normal no escribe.
- **Pruebas/evidencia requerida:** E(T-AUTH-032): conflictos hash/status, rehash simultáneo y resultado incierto sin sobrescritura ni emisión basada en información no sostenible.
- **Riesgos/notas:** No hashVersion, bucle/reintento ilimitado o nuevo campo de concurrencia; emisión se conectará en 048/049.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-033

- **Título:** Acotar coste de hashing y probar saturación.
- **Fase / gate:** F6 / GATE-D y GATE-E; pruebas transversales GATE-G.
- **Objetivo:** Controlar consumo criptográfico sin colas ilimitadas o degradación de seguridad.
- **Dependencias:** 009,031.
- **AUTH relacionados:** 008,011,026,031,038,040,041.
- **PLD-AUTH relacionados:** 14,18,20.
- **AC relacionados:** 07,18.
- **Archivos esperados:** `SEC/HashingConcurrencyGate.cs`, integración PasswordService, UT/IT.
- **Salida verificable:** Límite candidato 2 hashes/instancia configurable; admisión saturada produce 503 seguro, sin Task.Run/cola ilimitados; permisos liberados al terminar trabajo real.
- **Pruebas/evidencia requerida:** E(T-AUTH-033): concurrencia, cancelación, errores y saturación; un hash síncrono cancelado externamente no libera cupo anticipadamente mientras sigue ejecutándose.
- **Riesgos/notas:** No reducir seguridad del hash para latencia; calibración/perfil en 067, nunca SLA por default.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F7 — Register

#### T-AUTH-034

- **Título:** Implementar contratos y flujo de registro.
- **Fase / gate:** F7 / GATE-D.
- **Objetivo:** Coordinar una identidad mínima válida antes de persistirla.
- **Dependencias:** 012,025,028,031,033.
- **AUTH relacionados:** 002–009,012–015,038.
- **PLD-AUTH relacionados:** 02,05,12,13.
- **AC relacionados:** 01–05.
- **Archivos esperados:** `FA/Contracts/RegisterRequest.cs`, `RegisterResponse.cs`, `FA/Application/AuthenticationInputs.cs`, `AuthenticationResults.cs`, `AuthenticationService.cs`, UT.
- **Salida verificable:** fullName/email/password y proyección id/fullName/email normalizado; input/result sin MVC/BSON/Redis. Validación/cuota antes de hash, UUIDv4 y Active del servidor; sin token/login automático ni ToString sensible.
- **Pruebas/evidencia requerida:** E(T-AUTH-034): fakes de puertos verifican orden, ausencia de efectos ante rechazo y timestamps de creación coherentes.
- **Riesgos/notas:** No teléfono/rol/status/id del cliente, datos de repartidor o interfaz genérica artificial; adaptadores/persistencia en 035.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-035

- **Título:** Integrar endpoint, persistencia y conflictos.
- **Fase / gate:** F7 / GATE-D.
- **Objetivo:** Exponer registro con confirmación persistente y errores seguros.
- **Dependencias:** 024,034.
- **AUTH relacionados:** 003,004,009,031,032,038,039.
- **PLD-AUTH relacionados:** 05,14,19.
- **AC relacionados:** 01,03,17,19.
- **Archivos esperados:** `API/Controllers/AuthenticationController.cs`, servicio de aplicación/proyecciones y IT.
- **Salida verificable:** POST /api/v1/auth/register produce 201 solo confirmado, respuesta allowlist sin hash/token/Location ficticio; duplicado específico email →409 genérico; incertidumbre →error operativo sin compensación destructiva.
- **Pruebas/evidencia requerida:** E(T-AUTH-035): persistencia confirmada, conflicto, respuesta perdida y Problem Details/no-store correctos.
- **Riesgos/notas:** Índice garantiza unicidad, no consulta previa; contraste 201/409 conserva enumeración residual. Endpoint no se publica antes de controles completos.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-036

- **Título:** Aplicar cuotas y observabilidad segura de register.
- **Fase / gate:** F7 / GATE-D.
- **Objetivo:** Proteger coste de registro y producir eventos mínimos seguros.
- **Dependencias:** 021,035,053.
- **AUTH relacionados:** 028–030,032,036,037,038.
- **PLD-AUTH relacionados:** 15,16,19,23.
- **AC relacionados:** 16,19.
- **Archivos esperados:** Integración filtro/servicio en `HTTP/` y `FA/Application/`, `OBS/AuthenticationEvents.cs`, UT/IT/ST.
- **Salida verificable:** Cuota conjunta email 3/15 min y origen 20/15 min antes del hash; logging estructurado por allowlist y base común para eventos de cuotas/revocación/dependencias/confianza.
- **Pruebas/evidencia requerida:** E(T-AUTH-036): orden, intentos admitidos consumidos, 429/503 seguros y logs sin cuerpo/email/password/hash/secretos ni etiquetas sensibles.
- **Riesgos/notas:** No duplicar consumo entre filtro/servicio; métricas finales y revisión integral en 066; NAT residual no desaparece.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-037

- **Título:** Verificar registro completo.
- **Fase / gate:** F7 / GATE-D.
- **Objetivo:** Aceptar el incremento register mediante pruebas reales y trazables.
- **Dependencias:** 015,030,035,036,056,057.
- **AUTH relacionados:** 002–009,012–015,031,032,039,042,043.
- **PLD-AUTH relacionados:** 05,06,13,14,20.
- **AC relacionados:** 01–05,17,19.
- **Archivos esperados:** `UT/AuthenticationServiceTests.cs`, `IT/RegistrationTests.cs`, ST de registro y fixtures compartidas.
- **Salida verificable:** Contratos/fronteras, dos hosts con duplicados, índice/BSON y creación inspeccionados; sin escrituras por entrada inválida ni token automático/secretos en respuesta.
- **Pruebas/evidencia requerida:** E(T-AUTH-037): suites unit/integration/security del incremento con Mongo/Redis reales y composición 056/057; resultados aún pendientes.
- **Riesgos/notas:** Fase numérica no impide adelantar composición; factory no acredita pipeline final ni infraestructura publicada.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F8 — JWT

#### T-AUTH-038

- **Título:** Implementar emisión JWT y key ring.
- **Fase / gate:** F8 / GATE-E, checkpoint JWT.
- **Objetivo:** Emitir tokens mínimos firmados y temporalmente coherentes.
- **Dependencias:** 008,017,037.
- **AUTH relacionados:** 016,017,019–021,034.
- **PLD-AUTH relacionados:** 03,04,09,10.
- **AC relacionados:** 06,09,10,20,21.
- **Archivos esperados:** Proyecto/lockfile, `FA/Application/Ports/IAccessTokenIssuer.cs`, `SEC/JwtTokenIssuer.cs`, `JwtKeyRing.cs`, UT.
- **Salida verificable:** JwtBearer y stack único compatible; JsonWebTokens directo solo si su API se usa. HS256, una clave activa externa ≥32 bytes aleatorios/kids aprobados; sub UUID, jti RNG 32 bytes Base64url sin padding, iss=enver-api/aud=enver-mobile, iat UTC entero/exp=iat+900, skew ≤30 s.
- **Pruebas/evidencia requerida:** E(T-AUTH-038): claims mínimos/jti distinto, claves inválidas y frontera temporal explícita antes/en/después de exp+skew coordinada con retención.
- **Riesgos/notas:** Sin PII/roles/refresh ni emisión pública sin controles; precisión/margen requieren revisión, no defaults accidentales ni longitud Base64 como entropía.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-039

- **Título:** Implementar validación JwtBearer completa.
- **Fase / gate:** F8 / GATE-E, checkpoint JWT.
- **Objetivo:** No confiar en identidad/claims antes de validación completa.
- **Dependencias:** 011,038.
- **AUTH relacionados:** 017–020.
- **PLD-AUTH relacionados:** 10,13.
- **AC relacionados:** 09,10,18.
- **Archivos esperados:** `SEC/JwtBearerConfiguration.cs`, `JwtClaimsValidator.cs`, UT/ST.
- **Salida verificable:** Firma/HS256 allowlist/issuer/audience/lifetime y tipos/unicidad/canonicalidad/longitud de claims/header; iat/exp válidos, nbf si existe. MapInboundClaims=false, SaveToken=false; rechazar none, kid ausente/desconocido, duplicados/cabeceras críticas no soportadas.
- **Pruebas/evidencia requerida:** E(T-AUTH-039): firma/algoritmos, claims ambiguos, tiempos y límites; ningún jku/x5u remoto, fallback inseguro o uso confiable de sub/jti antes de validar.
- **Riesgos/notas:** Parseo temprano acotado no autentica; tolerancia temporal no alarga exp emitido ni deja tokens aceptados sin retención.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-040

- **Título:** Implementar políticas normal/logout y errores de autorización.
- **Fase / gate:** F8 / GATE-E; entrada GATE-F.
- **Objetivo:** Separar identidad, no revocación y excepción idempotente sin bypass.
- **Dependencias:** 010,018,020,039.
- **AUTH relacionados:** 003,018,021–023,026,031,032,035.
- **PLD-AUTH relacionados:** 11,14,19.
- **AC relacionados:** 09,11–14,19.
- **Archivos esperados:** `SEC/AuthenticationPolicies.cs`, `RevocationAuthorizationHandler.cs`, `AuthenticationAuthorizationResultHandler.cs`, UT/IT/ST.
- **Salida verificable:** Default/fallback seguro; política normal exige no revocación/estado confiable. Solo POST logout admite JWT auténtico vigente revocado para confirmar efecto; 401/challenge seguro frente a 503 operativo, sin redirección ni fuga.
- **Pruebas/evidencia requerida:** E(T-AUTH-040): denegación por defecto, excepción no reutilizable, revocado →401 fuera de logout y Redis incierto →503/no-store.
- **Riesgos/notas:** JWT válido no concede permisos de negocio; futuros recursos requieren su spec/política y no se crean para probar esta capacidad.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-041

- **Título:** Ensayar rotación y coordinación temporal.
- **Fase / gate:** F8 / GATE-E.
- **Objetivo:** Mantener validación/retención coherentes durante cambios de claves o configuración.
- **Dependencias:** 017,038.
- **AUTH relacionados:** 019,020,024,034.
- **PLD-AUTH relacionados:** 09,10.
- **AC relacionados:** 10,20.
- **Archivos esperados:** Procedimiento aprobado en soporte autorizado, pruebas UT/IT de key ring/política temporal.
- **Salida verificable:** Distribuir/reconocer nueva clave, cambiar emisión, coexistir solo por ventana válida y retirar; compromiso exige retiro extraordinario/reautenticación. Skew/margen/cambios coordinados sin aceptación desprotegida.
- **Pruebas/evidencia requerida:** E(T-AUTH-041): claves sintéticas, coexistencia/retiro, configuración incompatible y retención suficiente.
- **Riesgos/notas:** Sin automatización/periodicidad o gestor AWS seleccionado; todo poseedor HS256 puede emitir. No nuevo endpoint de cierre global.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-042

- **Título:** Verificar emisión, validación y políticas JWT.
- **Fase / gate:** F8 / GATE-E, checkpoint JWT.
- **Objetivo:** Acreditar el componente JWT antes de conectar logout y login públicos.
- **Dependencias:** 015,038–041.
- **AUTH relacionados:** 016–024,026,035,041–043.
- **PLD-AUTH relacionados:** 10,11,20.
- **AC relacionados:** 06,09–14,20.
- **Archivos esperados:** `UT/JwtPolicyTests.cs`, `ST/TokenValidationTests.cs`, IT con superficie solo de test.
- **Salida verificable:** Tokens de pruebas emitidos directamente por componente, claims/firma/algoritmo/kid y fronteras temporales comprobados; sesiones distintas y políticas aisladas.
- **Pruebas/evidencia requerida:** E(T-AUTH-042): unit/integration/security de JWT con Redis real cuando aplique; sin identidad falsa derivada de claims no autenticados.
- **Riesgos/notas:** No depende de login público para producir fixtures JWT; no anticipa aceptación de endpoints ni detección universal Redis.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F9 — Logout

#### T-AUTH-043

- **Título:** Implementar flujo de logout confirmado e idempotente.
- **Fase / gate:** F9 / GATE-F.
- **Objetivo:** Revocar solo el token presentado y confirmar efecto con seguridad.
- **Dependencias:** 017,020,040.
- **AUTH relacionados:** 018,022–024,026,038,039.
- **PLD-AUTH relacionados:** 02,08,09,11.
- **AC relacionados:** 11–14.
- **Archivos esperados:** `FA/Application/AuthenticationInputs.cs`, `AuthenticationResults.cs`, `AuthenticationService.cs`, UT/IT.
- **Salida verificable:** Input interno desde JWT autenticado; marca confirmada antes de éxito, repetición vigente conserva mayor expiración; fallo/incierto no produce 204. Sin LogoutRequest/cuerpo ni consulta MongoDB.
- **Pruebas/evidencia requerida:** E(T-AUTH-043): primera/repetida/concurrente, respuesta perdida y presupuesto; falso/expirado/ausente no escribe marca.
- **Riesgos/notas:** No revocar otras sesiones, asumir transacción distribuida o cancelar retroactivamente requests que ya superaron autorización.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-044

- **Título:** Integrar endpoint y política exclusiva logout.
- **Fase / gate:** F9 / GATE-F.
- **Objetivo:** Exponer semántica 204/401/503 sin relajar la política normal.
- **Dependencias:** 011,040,043.
- **AUTH relacionados:** 003,023,031,032,035.
- **PLD-AUTH relacionados:** 11,13,14,19.
- **AC relacionados:** 12,13,19.
- **Archivos esperados:** `API/Controllers/AuthenticationController.cs`, metadata de política y IT.
- **Salida verificable:** POST /api/v1/auth/logout solo Authorization Bearer, sin cuerpo funcional; 204 vacío/no-store solo confirmado, 401 inválido y 503 estado no verificable.
- **Pruebas/evidencia requerida:** E(T-AUTH-044): cuerpo no vacío/whitespace, token ausente/falso/expirado/revocado vigente y challenge sin motivos internos.
- **Riesgos/notas:** La excepción queda limitada a método/ruta logout; integración final del host en 058 sin publicar incrementos incompletos.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-045

- **Título:** Aplicar cuotas y eventos seguros de logout.
- **Fase / gate:** F9 / GATE-F.
- **Objetivo:** Limitar abuso y auditar confirmaciones sin exponer identidad/token.
- **Dependencias:** 021,036,044,053.
- **AUTH relacionados:** 028–030,032,036,037.
- **PLD-AUTH relacionados:** 15,16,19,23.
- **AC relacionados:** 16,19.
- **Archivos esperados:** Filtro/servicio, `OBS/AuthenticationEvents.cs`, IT/ST.
- **Salida verificable:** Cuota conjunta cuenta autenticada 30/min/origen 300/min; eventos mínimos de logout/rechazo sin JWT, sub/jti/IP como etiquetas sensibles.
- **Pruebas/evidencia requerida:** E(T-AUTH-045): ámbitos solo tras autenticar claims, 429/Retry-After y 503 seguros; no doble cargo ni filtración en logs.
- **Riesgos/notas:** Logout repetido también necesita cuota disponible; no usar sub/jti proporcionados sin firma para particionar cuenta.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-046

- **Título:** Verificar logout distribuido y fallos.
- **Fase / gate:** F9 / GATE-F.
- **Objetivo:** Acreditar rechazo cruzado e idempotencia sin bypass.
- **Dependencias:** 015,042–045.
- **AUTH relacionados:** 018,021–024,026,035,038,039,042,043.
- **PLD-AUTH relacionados:** 08,09,11,20,24.
- **AC relacionados:** 11–14,17.
- **Archivos esperados:** `IT/LogoutTests.cs`, IT de fallos y ST de aislamiento de política.
- **Salida verificable:** Revocar A en host 1 rechaza A en 2, B sigue válido; repeticiones/concurrencia no reducen TTL; Redis incierto →503/no 204; MongoDB caído no añade dependencia artificial al logout.
- **Pruebas/evidencia requerida:** E(T-AUTH-046): JWT de fixtures de 042, respuesta perdida, error escritura/lectura y token revocado rechazado fuera de logout.
- **Riesgos/notas:** No necesita login público previo ni endpoint nuevo de negocio; continuidad/fallos integrales y recuperación se contrastan en 062/063/068.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F10 — Login

#### T-AUTH-047

- **Título:** Implementar contratos y flujo de credenciales.
- **Fase / gate:** F10 / GATE-E.
- **Objetivo:** Coordinar validación, búsqueda y verificación con resultados seguros.
- **Dependencias:** 024,028,029,031,046.
- **AUTH relacionados:** 003,008,010,011,013,038.
- **PLD-AUTH relacionados:** 02,12,13,18.
- **AC relacionados:** 04,06,07,17.
- **Archivos esperados:** `FA/Contracts/LoginRequest.cs`, `LoginResponse.cs`, inputs/results y `AuthenticationService.cs`, UT.
- **Salida verificable:** email/password strings obligatorios; resultado público sin motivos privados y flujo dentro de presupuesto. Fallo MongoDB no se representa como usuario inexistente; login no reaplica mínimos futuros de creación.
- **Pruebas/evidencia requerida:** E(T-AUTH-047): verificación correcta/incorrecta, validación sin efectos indebidos y fallo operativo separado con fakes de puertos.
- **Riesgos/notas:** Se ejecuta después de logout conforme al plan; sin ToString sensible ni consultas del cliente.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-048

- **Título:** Integrar dummy, Disabled y rehash seguro.
- **Fase / gate:** F10 / GATE-E.
- **Objetivo:** Unificar rechazo de credenciales sin comprometer concurrencia.
- **Dependencias:** 032,033,047.
- **AUTH relacionados:** 010–014,026,039.
- **PLD-AUTH relacionados:** 08,12,14.
- **AC relacionados:** 06–08,14,17.
- **Archivos esperados:** Servicio de aplicación/PasswordService, UT/IT de login/rehash.
- **Salida verificable:** Inexistente verifica dummy previo, Disabled trabajo comparable y mismo 401 genérico; CAS/relectura acotados ante rehash, sin sobrescribir o sostener estado inválido. Login normal no escribe updatedAt.
- **Pruebas/evidencia requerida:** E(T-AUTH-048): mismo cuerpo/código/cabeceras para inexistente/incorrecto/Disabled, dummy una vez y conflictos reales de hash/estado sin token.
- **Riesgos/notas:** No sleeps/jitter principal ni tiempo idéntico garantizado; error operativo no se convierte en 401 ni obliga a generar dummy por request.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-049

- **Título:** Integrar emisión JWT y endpoint login.
- **Fase / gate:** F10 / GATE-E.
- **Objetivo:** Emitir públicamente solo tras credenciales y controles válidos.
- **Dependencias:** 010,020,038,048.
- **AUTH relacionados:** 003,010,016,017,021,026,031,032.
- **PLD-AUTH relacionados:** 08,10,14,19.
- **AC relacionados:** 06–08,14,15,19.
- **Archivos esperados:** Controller, servicio, `FA/Contracts/LoginResponse.cs`, IT.
- **Salida verificable:** POST /api/v1/auth/login →200 accessToken/tokenType=Bearer/expiresIn=900, nuevo jti sin revocar otras sesiones; no-store y errores comunes. No emitir cuando el guard/controles requeridos no sostengan seguridad.
- **Pruebas/evidencia requerida:** E(T-AUTH-049): claims emitidos, sesiones A/B, rechazo genérico y freno de emisión ante confianza incierta; cuota integrada en 050 antes de aceptación/publicación.
- **Riesgos/notas:** No refresh/MFA ni canales alternativos de token; producción/infraestructura no autorizadas por conectar el endpoint.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-050

- **Título:** Aplicar cuotas y eventos seguros de login.
- **Fase / gate:** F10 / GATE-E.
- **Objetivo:** Prevenir abuso sin revelar existencia/estado de cuenta.
- **Dependencias:** 021,036,049,053.
- **AUTH relacionados:** 011,028–030,032,036,037,038.
- **PLD-AUTH relacionados:** 15,16,19,23.
- **AC relacionados:** 07,08,16,19.
- **Archivos esperados:** Filtro/servicio, eventos en `OBS/`, IT/ST.
- **Salida verificable:** Cuotas conjuntas par origen+email 5/min, email 10/min y origen 60/min antes de hash; admitidos consumen aunque fallen/no exista cuenta; eventos sin motivos privados, secretos o PII innecesaria.
- **Pruebas/evidencia requerida:** E(T-AUTH-050): fronteras combinadas, 429/503, orden de trabajo y logs seguros iguales en casos de credenciales.
- **Riesgos/notas:** Sin sanción/Disabled por cuota, doble consumo ni fallback local. NAT y timing conservan riesgos residuales documentados.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-051

- **Título:** Verificar login completo.
- **Fase / gate:** F10 / GATE-E.
- **Objetivo:** Acreditar flujo de autenticación con dependencias y concurrencia reales.
- **Dependencias:** 015,047–050.
- **AUTH relacionados:** 010–021,026,028–032,039,042,043.
- **PLD-AUTH relacionados:** 10,12,15,20.
- **AC relacionados:** 04,06–08,14,16,17,19.
- **Archivos esperados:** `UT/AuthenticationServiceTests.cs`, `IT/LoginTests.cs`, ST de login.
- **Salida verificable:** Contrato, dummy/Disabled, rehash persistido/concurrente, dos sesiones y fallos Mongo/Redis verificados; sin modificación de updatedAt por login normal ni emisión sin control.
- **Pruebas/evidencia requerida:** E(T-AUTH-051): unit/integration/security con datos sintéticos y factories; reporte de resultados reales y limitaciones.
- **Riesgos/notas:** No duplica pruebas base del hasher; evaluación temporal/carga transversal en 067 y pipeline final en 059.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F11 — Rate limiting transversal

Estas capacidades son prerrequisitos/auxiliares de F3: no existe un segundo almacén de cuotas ni se repite su implementación.

#### T-AUTH-052

- **Título:** Precisar algoritmo, políticas y Retry-After.
- **Fase / gate:** F11 / GATE-B.
- **Objetivo:** Establecer un contrato atómico verificable para las siete cuotas aprobadas.
- **Dependencias:** 012,017.
- **AUTH relacionados:** 028–030,031,039,040.
- **PLD-AUTH relacionados:** 14,15.
- **AC relacionados:** 16,18.
- **Archivos esperados:** Descriptores/cálculo Retry-After en `FA/Application/` o `RDS/` según responsabilidad, UT y contrato de script.
- **Salida verificable:** Redis TIME/ventanas móviles; comprobar todos los ámbitos e insertar mismo ID interno solo si admite; rechazados no prolongan penalización. Retry-After máximo bloqueante redondeado hacia arriba sin revelar ámbito/contador. Valores del spec §3.5 íntegros.
- **Pruebas/evidencia requerida:** E(T-AUTH-052): fronteras, múltiples ámbitos y validación previa a mutar; claves atómicas según topología, sin asumir rollback Lua/cross-slot.
- **Riesgos/notas:** Sin clave pública de idempotencia nueva, Cluster elegido o algoritmo local presentado como distribuido; persistencia del script en 021.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-053

- **Título:** Implementar pseudonimización y origen confiable.
- **Fase / gate:** F11 / GATE-B; soporte GATE-D/E/F.
- **Objetivo:** Derivar ámbitos privados y coherentes sin reiniciar ventanas al cambiar claves.
- **Dependencias:** 008,012,016.
- **AUTH relacionados:** 028,030,032–034,036.
- **PLD-AUTH relacionados:** 16,17.
- **AC relacionados:** 16,19,20.
- **Archivos esperados:** Auxiliares HMAC y origen en `RDS/`/`HTTP/`, UT/ST y procedimiento de transición autorizado.
- **Salida verificable:** HMAC-SHA-256 con clave exclusiva por ambiente/finalidad para email y par inequívoco; no hash simple/email claro/clave JWT reutilizada. Origen RemoteIpAddress tras forwarding de proxies conocidos, IPv4/IPv6 canónico sin agrupación silenciosa. Transición HMAC/política preserva ventanas previas.
- **Pruebas/evidencia requerida:** E(T-AUTH-053): pseudónimos acotados, codificación sin ambigüedad, IP falsa/NAT/IPv6 y transición sintética sin reset; normalizador concreto integrado en endpoints después de 028.
- **Riesgos/notas:** Protocolo debe aprobarse antes de construir el control/rotar; automatización futura no se impone. LB/TLS productivos quedan diferidos.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-054

- **Título:** Acotar cardinalidad y recuperar cuotas con seguridad.
- **Fase / gate:** F11 / GATE-B; soporte GATE-G.
- **Objetivo:** Impedir crecimiento ilimitado y reinicio silencioso de contadores.
- **Dependencias:** 008,020,052.
- **AUTH relacionados:** 025–028,030,038,040.
- **PLD-AUTH relacionados:** 07,08,15,18.
- **AC relacionados:** 14–16,18.
- **Archivos esperados:** Control/presupuesto de particiones en `RDS/`, Options pertinentes, pruebas UT/IT y política de recuperación aprobada.
- **Salida verificable:** Entrada: topes numéricos/memoria aprobados. Claves/eventos/particiones activas limitados, TTL y sin nuevas particiones al rechazar; incapacidad →503/no bypass. Contadores perdidos: reconstrucción confiable o no admisión durante ventana restante aplicable.
- **Pruebas/evidencia requerida:** E(T-AUTH-054): identidades aleatorias, capacidad, pérdida/recuperación y ausencia de reset; garantías distintas de recuperación de revocaciones.
- **Riesgos/notas:** TTL solo no acota cardinalidad; noeviction no da memoria ilimitada; sin degradar seguridad por presión de caché.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-055

- **Título:** Verificar cuotas completas entre hosts.
- **Fase / gate:** F11 / GATE-B; regresión GATE-G.
- **Objetivo:** Acreditar cuotas conjuntas bajo concurrencia y fallos.
- **Dependencias:** 015,021,052–054.
- **AUTH relacionados:** 026,028–030,038–040,042,043.
- **PLD-AUTH relacionados:** 08,15–18,20.
- **AC relacionados:** 14–16,18.
- **Archivos esperados:** `IT/DistributedQuotaTests.cs`, ST de abuso/origen y fixtures aisladas.
- **Salida verificable:** REGISTER email 3/origen 20 por 15 min; LOGIN par 5/email 10/origen 60 por min; LOGOUT cuenta 30/origen 300 por min. Dos hosts/reinicio API no multiplican/resetan cupos; ventanas y Retry-After correctos.
- **Pruebas/evidencia requerida:** E(T-AUTH-055): Redis TIME/atomicidad, intentos admitidos fallidos, rechazos sin penalización perpetua, IP falsa/NAT/IPv6, caída/OOM/timeout y cardinalidad sin fallback local.
- **Riesgos/notas:** No sanción/Disabled por cuota; aislar recursos de pruebas de pérdida y concurrencia. No acredita red/LB productivos.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F12 — Pipeline

#### T-AUTH-056

- **Título:** Integrar configuración, errores y transporte.
- **Fase / gate:** F12 / GATE-A, integración posterior; entrada GATE-D.
- **Objetivo:** Conectar el tramo transversal seguro del host en una intervención coherente.
- **Dependencias:** 007–012,053.
- **AUTH relacionados:** 008,030–034,036,038.
- **PLD-AUTH relacionados:** 04,13,14,17–19,23.
- **AC relacionados:** 05,14,16,18–20.
- **Archivos esperados:** `API/Program.cs`, `COMP/AuthenticationServiceCollectionExtensions.cs`, configuración pública y IT de transporte/errores.
- **Salida verificable:** Options/ValidateOnStart, contexto/errores/no-store y frontera HTTP conectados; forwarding de proxies autorizados antes de origen/esquema, HTTPS/HSTS por ambiente sin bucles ni confianza arbitraria.
- **Pruebas/evidencia requerida:** E(T-AUTH-056): inicio inválido, errores/traceId, rechazos tempranos, cabeceras falsas y esquema/transporte en entorno de pruebas.
- **Riesgos/notas:** Edición serializada de Program/composición; no condición para cerrar GATE-A inicial, pues necesita 053 de Redis. Redirección/EXPOSE no prueban TLS publicado; no seleccionar LB/AWS.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-057

- **Título:** Integrar dependencias y admisión segura.
- **Fase / gate:** F12 / GATE-B/C, integración; entrada GATE-D.
- **Objetivo:** Conectar Redis/MongoDB sin acoplar operaciones a dependencias innecesarias.
- **Dependencias:** 016–020,024,025,056.
- **AUTH relacionados:** 009,015,025–028,038.
- **PLD-AUTH relacionados:** 05–08,19,24.
- **AC relacionados:** 03,14,15,17.
- **Archivos esperados:** `API/Program.cs`, `COMP/AuthenticationServiceCollectionExtensions.cs`, IT de inicialización/DI.
- **Salida verificable:** Clientes singleton, guard y ciclo de inicializador registrados; registro bloqueado sin índice válido, admisión dependiente cerrada ante estado no confiable. Logout no consulta MongoDB artificialmente.
- **Pruebas/evidencia requerida:** E(T-AUTH-057): índice incompatible, fallos de conexión/configuración y estado de seguridad no apto/503; factory incremental con adaptadores reales.
- **Riesgos/notas:** Serializar después de 056; readiness del conjunto no sustituye dependencia efectiva de operación ni exige endpoint público ahora.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-058

- **Título:** Integrar autenticación, autorización y controllers.
- **Fase / gate:** F12 / GATE-E; entrada GATE-G.
- **Objetivo:** Completar el pipeline sin bypass ni trabajo innecesario.
- **Dependencias:** 040,051,055,057.
- **AUTH relacionados:** 003,008,018,023,026,028–032,035.
- **PLD-AUTH relacionados:** 10,11,13–15,19.
- **AC relacionados:** 05,09,11–14,16,18,19.
- **Archivos esperados:** `API/Program.cs`, composición, `HTTP/AuthenticationValidationAndQuotaFilter.cs`, configuración MVC y IT.
- **Salida verificable:** Routing/metadata, límites/presupuestos, autenticidad, autorización/revocación, MVC estricto/validación/cuotas/servicio y respuesta segura en orden compatible con plan §15. Writers MVC/challenge/autorización integrados, sin dobles cargos.
- **Pruebas/evidencia requerida:** E(T-AUTH-058): register/login públicos explícitos sin trabajo JWT innecesario por Bearer opcional; logout exclusivo; errores tempranos y política default/fallback segura.
- **Riesgos/notas:** Serializar después de 057; sin aliases/endpoints de negocio nuevos. Ningún incremento parcial se publica desprotegido.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-059

- **Título:** Verificar pipeline y liveness.
- **Fase / gate:** F12 / GATE-G; consolidación GATE-E/F.
- **Objetivo:** Contrastar el host completo y conservar health básico sin promesas de integridad.
- **Dependencias:** 058.
- **AUTH relacionados:** 009,026,027,033,037,038,046.
- **PLD-AUTH relacionados:** 19,24.
- **AC relacionados:** 14,15,19,20.
- **Archivos esperados:** IT del pipeline/liveness, `API/Program.cs` solo si la integración requiere ajuste autorizado.
- **Salida verificable:** Orden completo probado; `/health` conserva liveness básico sin detalles de dependencias; inicialización/admisión interna no se confunden con health ni con readiness pública.
- **Pruebas/evidencia requerida:** E(T-AUTH-059): contratos/errores y dependencia efectiva de cada operación, liveness bajo fallos y ausencia de detalles internos.
- **Riesgos/notas:** `/health/ready` y uso LB son futuros de infraestructura; un gate local no demuestra AWS/Atlas/infraestructura productiva. AUTH-027 no se satisface por health.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F13 — Regresión y seguridad integral

Las pruebas funcionales acompañan a sus capacidades. Esta fase reutiliza esas suites y agrega escenarios transversales; no duplica su implementación ni posterga toda la seguridad al cierre.

#### T-AUTH-060

- **Título:** Ejecutar regresión transversal trazable.
- **Fase / gate:** F13 / GATE-G.
- **Objetivo:** Contrastar conjuntamente capacidades y cobertura de escenarios sin tests redundantes.
- **Dependencias:** 030,032,033,037,042,046,051,055,059.
- **AUTH relacionados:** 039–043,047.
- **PLD-AUTH relacionados:** 20,25.
- **AC relacionados:** 01–14,16–19.
- **Archivos esperados:** Suites UT/IT/ST existentes y matriz de pruebas/evidencia en soporte autorizado.
- **Salida verificable:** Descubrimiento/ejecución conjunta de validación, hashing, JWT, TTL, Options/errores y endpoints, con AUTH/AC vinculados a casos reales y pendientes identificados.
- **Pruebas/evidencia requerida:** E(T-AUTH-060): comandos de las tres suites, configuración/entorno/resultados y regresiones; sin aceptar cero tests ni porcentaje arbitrario de cobertura.
- **Riesgos/notas:** Esta ejecución reutiliza casos previos; no prueba todavía escenarios externos ni sustituye 062/063/068 o scanners.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-061

- **Título:** Ensayar fallos e incertidumbre de dependencias.
- **Fase / gate:** F13 / GATE-G.
- **Objetivo:** Verificar respuestas conservadoras y consumo acotado ante fallos cruzados.
- **Dependencias:** 060.
- **AUTH relacionados:** 004,026,038,039,042.
- **PLD-AUTH relacionados:** 08,18,20.
- **AC relacionados:** 14,17,18.
- **Archivos esperados:** `IT/DependencyFailureTests.cs`, fixtures/fallos de red autorizados.
- **Salida verificable:** Mongo/Redis caídos, timeout, ACK perdido, cancelación y saturación no generan creación/ausencia/success ficticios ni bypass; no multiplicar efectos al reintentar.
- **Pruebas/evidencia requerida:** E(T-AUTH-061): momento de fallo/operación en vuelo, resultado observable, presupuestos y recuperación; 503 operativo no se convierte en 401.
- **Riesgos/notas:** Objetivos/datos sintéticos autorizados; no ampliar automáticamente a cloud ni cancelar retroactivamente efectos confirmados.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-062

- **Título:** Ensayar pérdida Redis con detectores reales.
- **Fase / gate:** F13 / GATE-G.
- **Objetivo:** Probar detección observable, no solamente reacción a un flag inyectado.
- **Dependencias:** 019,022,046,059.
- **AUTH relacionados:** 022,025–027,038,042,043.
- **PLD-AUTH relacionados:** 08,20,24.
- **AC relacionados:** 15.
- **Archivos esperados:** `ST/RedisStateLossTests.cs`, fixtures de fallos y reporte del modelo.
- **Salida verificable:** Token revocado confirmado/dos hosts; indisponibilidad, timeout/escritura incierta, flush/eliminación detectable/reinicio/restauración antigua/failover según topología ensayados, con señales y propagación de cierre identificadas.
- **Pruebas/evidencia requerida:** E(T-AUTH-062): evento, detector/componente, instante/latencia de detección y propagación, resultados y límites de cada caso; no aprobar por omitir un evento no detectado.
- **Riesgos/notas:** AUTH-027 NO IMPLEMENTADO / NO SATISFECHO; pérdida completamente silenciosa no observable es residual. Señal artificial prueba reacción, nunca detector ni ausencia universal de pérdida.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-063

- **Título:** Ensayar cierre y reapertura coordinados.
- **Fase / gate:** F13 / GATE-G.
- **Objetivo:** Verificar recuperación/cuarentena sin resucitar tokens afectados o cuotas perdidas.
- **Dependencias:** 020,054,062.
- **AUTH relacionados:** 024,026–028,038,039,043.
- **PLD-AUTH relacionados:** 08,09,20.
- **AC relacionados:** 15,20.
- **Archivos esperados:** ST/IT de recuperación Redis y registro seguro de horizonte/instancias.
- **Salida verificable:** Cierre compartido y freno de emisión cuando corresponda, restauración confiable o cuarentena suficiente; última emisión posible/hosts en vuelo/relojes/skew/margen considerados. Recuperación de cuotas mantiene su política propia.
- **Pruebas/evidencia requerida:** E(T-AUTH-063): no reapertura por ping/flag local/primera alerta; tras restauración persiste rechazo, tras cuarentena ningún token afectado permanece temporalmente válido.
- **Riesgos/notas:** AUTH-027 NO IMPLEMENTADO / NO SATISFECHO; si no se demuestra cese/emisión o seguridad de reapertura, conservar no admisión, no inventar RTO.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-064

- **Título:** Completar ataques JWT y replay.
- **Fase / gate:** F13 / GATE-G.
- **Objetivo:** Contrastar amenazas combinadas sobre endpoints/políticas integrados.
- **Dependencias:** 042,046,051,059.
- **AUTH relacionados:** 018–024,035,043.
- **PLD-AUTH relacionados:** 10,11,20.
- **AC relacionados:** 09–13.
- **Archivos esperados:** `ST/TokenValidationTests.cs`, casos de replay y aislamiento de políticas.
- **Salida verificable:** Firma alterada/none/algorithm confusion/issuer/audience/kid/claims ambiguos y fronteras temporales rechazados; replay después de logout denegado con continuidad confiable y sesión B independiente.
- **Pruebas/evidencia requerida:** E(T-AUTH-064): ataques con claves/tokens sintéticos, casos positivos de control y pruebas de bypass de excepción logout.
- **Riesgos/notas:** Bearer robado puede reutilizarse antes de revocación/expiración; jti no es proof-of-possession ni token de un solo uso. No afirmar seguridad universal.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-065

- **Título:** Completar ataques de entrada y consumo.
- **Fase / gate:** F13 / GATE-G.
- **Objetivo:** Probar límites/contratos sobre el pipeline final antes de efectos costosos.
- **Dependencias:** 030,059.
- **AUTH relacionados:** 008,018,030,035,040,043.
- **PLD-AUTH relacionados:** 13,18–20.
- **AC relacionados:** 05,18.
- **Archivos esperados:** `ST/MaliciousInputTests.cs`, IT de transporte y fixtures.
- **Salida verificable:** NoSQL injection/mass assignment, propiedades desconocidas/duplicadas, null/tipos/Unicode, cuerpo chunked excesivo/profundidad/Content-Type y JWT excesivo no eluden límites ni cambian consultas/estado.
- **Pruebas/evidencia requerida:** E(T-AUTH-065): 400/413/415 y 401 JWT API según contrato, ausencia de hashing/escritura/emisión indebidos y recursos acotados.
- **Riesgos/notas:** Rechazos del servidor/proxy fuera de la API tienen alcance distinto, potencial 431; no añadir funcionalidades de negocio ni pruebas sobre objetivos ajenos.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-066

- **Título:** Verificar privacidad y observabilidad integral.
- **Fase / gate:** F13 / GATE-G; soporte GATE-H.
- **Objetivo:** Completar señales operativas acotadas y contrastar ausencia de filtraciones en el alcance probado.
- **Dependencias:** 010,036,045,050,059.
- **AUTH relacionados:** 031,032,034,036,037,043.
- **PLD-AUTH relacionados:** 14,22,23.
- **AC relacionados:** 19.
- **Archivos esperados:** `OBS/AuthenticationEvents.cs`, `AuthenticationMetrics.cs`, `ST/SensitiveDataExposureTests.cs`.
- **Salida verificable:** Eventos de operación/resultado, UTC/ambiente/versión/instancia/duración/correlación; métricas de latencia, dependencias, cuotas, revocación, saturación y no admisión sin email/IP/sub/jti/token como etiquetas. No-store y minimización coherentes.
- **Pruebas/evidencia requerida:** E(T-AUTH-066): revisar respuestas/logs/trazas/métricas/persistencia con secretos sintéticos, log injection y sin volcar requests/options/excepciones sensibles.
- **Riesgos/notas:** Sin caché/compresión de secretos no evaluada; muestreo/pérdidas declarados. Retención/acceso/eliminación/exportación/CloudWatch AD-10/11 antes de producción, no seleccionados aquí.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-067

- **Título:** Calibrar carga, enumeración y saturación.
- **Fase / gate:** F13 / GATE-G.
- **Objetivo:** Evaluar rendimiento candidato y diferencias observables sin debilitar seguridad.
- **Dependencias:** 033,055,061,066.
- **AUTH relacionados:** 011,030,038,040–043.
- **PLD-AUTH relacionados:** 18,20,23.
- **AC relacionados:** 07,08,18.
- **Archivos esperados:** `ST/EnumerationResistanceTests.cs`, perfiles/casos de carga y reporte en soporte autorizado.
- **Salida verificable:** Entrada: perfil/volumen/concurrencia/recursos/muestras/tolerancias aprobados. Medir inexistente/incorrecto/Disabled, hashing/cardinalidad y presupuestos; tratar resultados y rechazos separadamente.
- **Pruebas/evidencia requerida:** E(T-AUTH-067): percentiles y consumo reales del entorno, comparación de candidatos spec §3.6, calibración AD-09 y hallazgos.
- **Riesgos/notas:** Nunca tiempos idénticos/SLA/capacidad no medida, reducción de seguridad del hash ni adopción de metas RTO/RPO/carga de 001; 201/409 y NAT siguen residuales.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-068

- **Título:** Contrastar garantías en entorno externo representativo.
- **Fase / gate:** F13 / GATE-G; aceptación externa pendiente.
- **Objetivo:** Verificar lo que contenedores locales no acreditan del servicio elegido.
- **Dependencias:** 060–067.
- **AUTH relacionados:** 022,025–027,033,034,038,042,043,046.
- **PLD-AUTH relacionados:** 05,07,08,17,20,24.
- **AC relacionados:** 15,17,20.
- **Archivos esperados:** Configuración externa no versionada con secretos, IT/ST externos y reporte seguro del entorno en soporte autorizado.
- **Salida verificable:** Entrada: Atlas de pruebas/Redis y topología/objetivos autorizados disponibles. TLS/ACL/concerns/ACK/lecturas/continuidad/recuperación representativos contrastados y límites de extrapolación documentados.
- **Pruebas/evidencia requerida:** E(T-AUTH-068): comandos, servicio/topología/versiones, pruebas reales autorizadas y resultados/tratamiento; no equivaler dos factories a dos instancias cloud/LB.
- **Riesgos/notas:** AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. No selecciona/despliega AWS; si requiere infraestructura ausente permanece bloqueada, sin impedir checkpoints locales delimitados ni cerrar aceptación integral.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F14 — DevSecOps

#### T-AUTH-069

- **Título:** Aprobar política DevSecOps y evidencia académica.
- **Fase / gate:** F14 / GATE-H.
- **Objetivo:** Concretar controles/gates sin tratar herramientas candidatas como verificadas.
- **Dependencias:** 003.
- **AUTH relacionados:** 040–045,047,048.
- **PLD-AUTH relacionados:** 20,21,25.
- **AC relacionados:** 21.
- **Archivos esperados:** Política/registro de decisiones y rúbrica en soporte autorizado; futuras configuraciones de herramientas.
- **Salida verificable:** Herramientas/versiones/OWASP/rúbrica, severidades/aplicabilidad/licencias y excepciones con responsable/vencimiento aprobados; candidato High/Critical bloqueante ratificado o concretado expresamente. Secretos activos exigen tratamiento/rotación; SecurityTests fallidos bloquean.
- **Pruebas/evidencia requerida:** E(T-AUTH-069): revisión de política, evidencia académica/OpenAPI requerida y comportamiento ante scanner ausente/fallido, nunca aprobación por falta de resultado.
- **Riesgos/notas:** AD-12 no queda resuelta por nombrar CodeQL/Audit/Gitleaks/Trivy; no inventar rúbrica original ni ponderaciones ausentes.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-070

- **Título:** Integrar CI build y unit tests.
- **Fase / gate:** F14 / GATE-H.
- **Objetivo:** Automatizar construcción reproducible y pruebas unitarias con mínimo privilegio.
- **Dependencias:** 060,069.
- **AUTH relacionados:** 034,041,044,045,047.
- **PLD-AUTH relacionados:** 03,20,21.
- **AC relacionados:** 21.
- **Archivos esperados:** `.github/workflows/ci.yml`, configuración de resultados/artefactos no sensibles.
- **Salida verificable:** Jobs estables build/unit-tests, SDK fijado/restore bloqueado/Release, resultados identificados; acciones fijadas a commit y permisos mínimos. PR no confiables sin secretos productivos ni pull_request_target privilegiado ejecutando código hostil.
- **Pruebas/evidencia requerida:** E(T-AUTH-070): runs reales, detección de fallo/ausencia de tests, commit/artefacto/configuración/resultados.
- **Riesgos/notas:** Autorización GitHub propia; ningún workflow creado o ejecutado por este documento. No auto-deploy ni filtros que omitan checks obligatorios.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-071

- **Título:** Integrar CI integration y security tests.
- **Fase / gate:** F14 / GATE-H.
- **Objetivo:** Ejecutar explícitamente ambas suites reales en CI.
- **Dependencias:** 060–067,070.
- **AUTH relacionados:** 042,043,045,047.
- **PLD-AUTH relacionados:** 20,21.
- **AC relacionados:** 21.
- **Archivos esperados:** `.github/workflows/ci.yml`, configuración de fixtures/resultados IT/ST.
- **Salida verificable:** Job integration-tests ejecuta IntegrationTests y SecurityTests con contenedores autorizados/aislados, reportes seguros y versiones/topología identificadas; no verde por cero tests o suite omitida.
- **Pruebas/evidencia requerida:** E(T-AUTH-071): run/commit, ambos descubrimientos/ejecuciones, fallos y artefactos; tests externos 068 no se simulan con secretos productivos en PR.
- **Riesgos/notas:** SecurityTests no sustituido por job security-scan; Docker/runners y cuotas de ejecución requieren permiso/entorno disponibles.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-072

- **Título:** Integrar CodeQL, NuGet Audit e inventario.
- **Fase / gate:** F14 / GATE-H.
- **Objetivo:** Analizar código/dependencias/licencias conforme a la política aprobada.
- **Dependencias:** 069,070.
- **AUTH relacionados:** 043–045,047.
- **PLD-AUTH relacionados:** 03,21.
- **AC relacionados:** 21.
- **Archivos esperados:** Workflow/configuración SAST .NET y auditoría NuGet, inventario/SBOM cuando proceda.
- **Salida verificable:** CodeQL/analizadores y Audit directo/transitivo/licencias integrados con resultados trazables; aviso OpenAPI contrastado con evidencia real y hallazgos tratados.
- **Pruebas/evidencia requerida:** E(T-AUTH-072): runs reales, versiones/alcance, umbrales, fallos de análisis y excepciones vigentes; no inferir seguridad por caché sin avisos.
- **Riesgos/notas:** Permisos/licencias de la plataforma y herramientas ratificados; SAST no reemplaza pruebas dinámicas ni acredita ausencia universal de vulnerabilidades.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-073

- **Título:** Integrar secret/container scanning y security-scan.
- **Fase / gate:** F14 / GATE-H.
- **Objetivo:** Consolidar controles de seguridad CI sin ocultar errores o resultados ausentes.
- **Dependencias:** 070,072,078.
- **AUTH relacionados:** 034,043–045,047.
- **PLD-AUTH relacionados:** 21,22.
- **AC relacionados:** 21.
- **Archivos esperados:** Workflow/configuraciones Gitleaks/Trivy y job agregado `.github/workflows/ci.yml`.
- **Salida verificable:** Gitleaks con alcance/exclusiones justificados y Trivy imagen/base ligado a digest, reutilizando criterios/configuración 078; job estable security-scan agrega SAST/dependencias/secretos/contenedor y falla ante errores/ausencias según política.
- **Pruebas/evidencia requerida:** E(T-AUTH-073): runs reales y fixtures sintéticas de detección/fallo, identidad de artefacto y tratamiento de hallazgos/excepciones vencidas.
- **Riesgos/notas:** Secreto activo requiere rotación/tratamiento, no solo borrado; alcance histórico/binario explícito, sin certificar lo no revisado. Sin logs de secretos.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-074

- **Título:** Configurar actualización segura de dependencias.
- **Fase / gate:** F14 / GATE-H.
- **Objetivo:** Mantener NuGet/Actions revisables sin incorporar cambios automáticamente.
- **Dependencias:** 069,072.
- **AUTH relacionados:** 044,045.
- **PLD-AUTH relacionados:** 03,21.
- **AC relacionados:** 21.
- **Archivos esperados:** `.github/dependabot.yml`.
- **Salida verificable:** Dependabot NuGet/Actions con rutas/alcance y política de revisión aprobados, configuración válida y primera ejecución cuando esté disponible.
- **Pruebas/evidencia requerida:** E(T-AUTH-074): revisión de sintaxis/configuración y run/PR real cuando exista; compatibilidad/grafo/licencias siguen sujetos a controles.
- **Riesgos/notas:** No auto-merge, actualización masiva o excepción de seguridad implícitos; un archivo de configuración no acredita ejecución exitosa.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-075

- **Título:** Configurar required checks tras runs válidos.
- **Fase / gate:** F14 / GATE-H.
- **Objetivo:** Impedir merge que eluda controles reales y confiables.
- **Dependencias:** 070,071,073,074.
- **AUTH relacionados:** 041–048.
- **PLD-AUTH relacionados:** 21,25.
- **AC relacionados:** 21.
- **Archivos esperados:** Configuración remota autorizada de ruleset/branch protection y registro seguro de cambios.
- **Salida verificable:** Solo después de existir/ejecutar correctamente, requerir build, unit-tests, integration-tests y security-scan con nombres/triggers estables y sin filtros que dejen checks ausentes.
- **Pruebas/evidencia requerida:** E(T-AUTH-075): runs exitosos previos, revisión de permisos y PR controlada que no pueda eludir checks; autorización propia del cambio remoto.
- **Riesgos/notas:** No configurar checks imaginarios/antes de tiempo, alterar ramas o desplegar por iniciativa del agente; no confundir branch protection informada con verificada.
- **Estado:** PENDIENTE / NO EJECUTADA.

### F15 — Docker y cierre

#### T-AUTH-076

- **Título:** Endurecer Dockerfile e identificar bases.
- **Fase / gate:** F15 / GATE-H.
- **Objetivo:** Preparar una imagen trazable con contexto/publicación ya protegidos.
- **Dependencias:** 003,005,059,069.
- **AUTH relacionados:** 034,044–046.
- **PLD-AUTH relacionados:** 03,21,22.
- **AC relacionados:** 21.
- **Archivos esperados:** `API/Dockerfile`, revisión de `.dockerignore`/exclusiones publish ya realizadas en 005.
- **Salida verificable:** Etapas build/publish/runtime y rutas revisadas, bases/SDK fijados y procedencia/digests registrados según política; sin secretos por ARG/ENV/COPY.
- **Pruebas/evidencia requerida:** E(T-AUTH-076): revisión de contexto, plataforma, mantenimiento y usuario previsto; comprobación efectiva non-root en 077.
- **Riesgos/notas:** No duplicar/retrasar protección 005 ni afirmar reproducibilidad bit a bit sin criterio; sin despliegue AWS.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-077

- **Título:** Construir y ejecutar imagen local non-root.
- **Fase / gate:** F15 / GATE-H.
- **Objetivo:** Verificar imagen funcional y privilegios mínimos en entorno local autorizado.
- **Dependencias:** 060–067,076.
- **AUTH relacionados:** 003,026,034,038,042,045,047.
- **PLD-AUTH relacionados:** 20–22,24.
- **AC relacionados:** 01,06,11,14,21.
- **Archivos esperados:** Dockerfile si requiere ajuste autorizado, imagen/artefactos locales y pruebas smoke sobre contenedores propios.
- **Salida verificable:** Build/run asociados a baseline/commit/digest; dependencias y secretos efímeros externos, register/login/logout/liveness comprobados; UID efectivo no root y permisos mínimos verificados.
- **Pruebas/evidencia requerida:** E(T-AUTH-077): comandos, SDK/bases/digest, smoke y caída de dependencia; no deducir non-root solo por USER $APP_UID/EXPOSE.
- **Riesgos/notas:** Docker/registro de imágenes autorizados; nunca secretos reales en contexto/capas/logs. No acredita despliegue/HTTPS productivo.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-078

- **Título:** Inspeccionar artefactos, secretos e imagen.
- **Fase / gate:** F15 / GATE-H.
- **Objetivo:** Contrastar protección de datos e imagen final/base antes de aceptación.
- **Dependencias:** 005,069,077.
- **AUTH relacionados:** 032,034,044,045.
- **PLD-AUTH relacionados:** 21,22.
- **AC relacionados:** 19,21.
- **Archivos esperados:** Configuración reutilizable Trivy, reporte seguro de contexto/publish/capas/historial/logs e imagen identificada.
- **Salida verificable:** Sentinelas sintéticas no aparecen en artefactos/capas; Trivy cubre base/imagen final por digest y política aprobada, con hallazgos/incidencias tratados; configuración reutilizada por 073.
- **Pruebas/evidencia requerida:** E(T-AUTH-078): inspección autorizada, ejecución scanner/versión/configuración/resultados y tratamiento; sin duplicar configuración entre local/CI.
- **Riesgos/notas:** No afirmar ausencia universal de secretos o vulnerabilidades; scanner fallido no aprueba ni ocultar secretos del contexto solo mediante .gitignore.
- **Estado:** PENDIENTE / NO EJECUTADA.

#### T-AUTH-079

- **Título:** Consolidar evidencia y aceptación delimitada.
- **Fase / gate:** F15 / GATE-H; consolidación de los ocho gates.
- **Objetivo:** Relacionar requisitos, decisiones, autorizaciones, cambios y resultados reales sin cierre ficticio.
- **Dependencias:** 068,075,078.
- **AUTH relacionados:** 001,040–048.
- **PLD-AUTH relacionados:** 20–25.
- **AC relacionados:** 01–21.
- **Archivos esperados:** Matrices de aceptación/evidencia y documentos de estado solo bajo autorización posterior delimitada.
- **Salida verificable:** AUTH 48/48, PLD 25/25 y AC 21/21 relacionados con tarea/prueba/commit/run/artefacto reales o pendientes explícitos; revisar gates y riesgos/responsables sin aprobar por asociación.
- **Pruebas/evidencia requerida:** E(T-AUTH-079): revisión de evidencias auténticas, alcance/topología y tratamiento; capturas solo si rúbrica exige, nunca evidencia inventada o este catálogo como resultado técnico.
- **Riesgos/notas:** Si 068 u otra puerta está bloqueada, solo checkpoint parcial explícito, no cierre integral. No completa foundation ni autoriza AWS o siguiente incremento automáticamente.
- **Estado:** PENDIENTE / NO EJECUTADA.

## 4. Gates de aceptación

Todos los gates tienen estado **PENDIENTE / NO SATISFECHO**. Las condiciones de salida son requisitos futuros, no resultados observados. Los gates revisan capacidades; no reemplazan autorización por tarea/incremento. No se impone terminar el gate completo para construir sus propias tareas integrantes: sus entradas y dependencias indican qué parte puede iniciarse.

**Un gate superado localmente no demuestra AWS/Atlas/infraestructura productiva.** Cada aceptación identifica entorno/topología y alcance. Un checkpoint local puede permitir trabajo posterior independiente autorizado, pero no cerrar la aceptación externa pendiente de 068, simularla o afirmar cumplimiento integral. El cierre 079 depende de 068; mientras falte, permanece pendiente aunque existan checkpoints locales.

### GATE-A

- **Nombre:** Base/configuración.
- **Tareas de entrada:** 001–006; componentes 007–015.
- **Condiciones de entrada:** Solicitud técnica posterior delimitada, fuentes/baseline identificadas, conservación de cambios existentes y entornos aprobados para las acciones de preparación.
- **Condiciones de salida:** 001–015 revisadas/verificadas: SDK/grafo/versiones, higiene/contexto/publish, Options inválidas rechazadas, presupuestos/errores/HTTP probados y tres suites/fixtures disponibles. Resultados reales, no mera presencia de clases.
- **Bloqueos conocidos:** Git pendiente de consolidación por responsable, versiones/licencias/vulnerabilidades y Docker/runner no aprobados; no ejecutar un build sensible sin 005. Las fuentes de configuración se pueden comprobar con valores sintéticos sin servicio cloud.
- **Evidencia necesaria:** E(T-AUTH-001) a E(T-AUTH-015), revisión de alcance/configuración y resultados de pruebas de componentes. 056 integra después el tramo base y no es entrada de A: depende de 053, evitando ciclo A→B→A.

### GATE-B

- **Nombre:** Redis.
- **Tareas de entrada:** GATE-A; 016 y condiciones previas de 019/052–054; componentes 017–022 y 055.
- **Condiciones de entrada:** Versiones/configuración/entorno Redis aprobados; modelo de fallos, ACK/lecturas y señales antes de 017/019; precedencia/cardinalidad/HMAC/recuperación antes de cuotas.
- **Condiciones de salida:** 016–022 y 052–055 con evidencia: revocación/retención no decreciente, noeviction/capacidad, confirmación/lecturas, cuotas conjuntas y fail-closed distribuidos; detección/cierre/recuperación verificados para entorno declarado. Casos JWT/endpoints finales se complementan en F/G, no se anticipan aquí.
- **Bloqueos conocidos:** Fuentes de detección/coordinación/ACK/topología, topes y protocolos no resueltos; prueba local no resuelve operación del servicio administrado. Sin prueba de reapertura segura mantener cierre.
- **Evidencia necesaria:** E(T-AUTH-016) a E(T-AUTH-022), E(T-AUTH-052) a E(T-AUTH-055); dos hosts/dependencias reales, propagación y límites. AUTH-027 sigue NO IMPLEMENTADO / NO SATISFECHO en esta entrega; no certificarlo por checkpoint de componentes.

### GATE-C

- **Nombre:** MongoDB.
- **Tareas de entrada:** GATE-B, 023; componentes 024–027.
- **Condiciones de entrada:** Driver/grafo y topología/journaling/ACK/concerns aprobados; fixture representativa autorizada.
- **Condiciones de salida:** 023–027 verificadas: siete campos/BSON estándar, lectura tras creación confirmada, índice exacto/inicializador, unicidad concurrente, CAS y resultados inciertos seguros.
- **Bloqueos conocidos:** Standalone insuficiente para majority/failover; datos/índices incompatibles no se reparan automáticamente; Atlas externo sigue pendiente donde aplique.
- **Evidencia necesaria:** E(T-AUTH-023) a E(T-AUTH-027), inspección real de BSON/índice y pruebas de carrera/ACK/CAS con versión/topología.

### GATE-D

- **Nombre:** Register.
- **Tareas de entrada:** GATE-B/C; 028–031/033 y composición 056–057; componentes 034–037. 032 se verifica para rehash antes de Login, no es condición funcional de crear una cuenta nueva.
- **Condiciones de entrada:** Normalización/password/coste y controles compartidos disponibles; factories y adaptadores reales integrables sin publicar rutas incompletas.
- **Condiciones de salida:** 034–037 verificadas: 201 confirmado, conflicto 409 genérico determinado por índice, sin login/token automático; validación/cuotas antes del coste, no-store/logging seguro y pruebas positivas/negativas/concurrentes.
- **Bloqueos conocidos:** Índice/confianza/cuotas no aptos impiden aceptación; 201/409 conserva inferencia residual y dominio no demuestra titularidad.
- **Evidencia necesaria:** E(T-AUTH-028) a E(T-AUTH-031), E(T-AUTH-033) a E(T-AUTH-037), E(T-AUTH-056) y E(T-AUTH-057); casos AC-01 a AC-05, 16/17/19 según ficha.

### GATE-E

- **Nombre:** JWT/Login.
- **Tareas de entrada:** GATE-D; 038–042 para checkpoint JWT. Después de GATE-F, 032 y 047–051; integración final 058.
- **Condiciones de entrada:** Claves/configuración/temporalidad/retención aprobadas y controles Redis confiables; CAS/dummy/coste disponibles antes de Login. Emisión de fixtures de test no requiere login público.
- **Condiciones de salida:** Checkpoint **E-JWT**: 038–042 probadas habilitan F. Cierre **E completo**: tras F, 047–051 y 058 comprobadas, con dummy previo, Disabled comparable, CAS seguro, 401 uniforme, cuotas y JWT solo con controles válidos.
- **Bloqueos conocidos:** Precisión/margen/rotación, estado no confiable, dummy no preparado, rehash incierto o cuota no verificable bloquean aceptación/emisión afectada. No cerrar E completo antes de Logout.
- **Evidencia necesaria:** E(T-AUTH-032), E(T-AUTH-038) a E(T-AUTH-042), E(T-AUTH-047) a E(T-AUTH-051), E(T-AUTH-058); fronteras temporales, claims/algoritmos/políticas y login real. Ninguna evidencia técnica existe todavía.

### GATE-F

- **Nombre:** Logout.
- **Tareas de entrada:** GATE-B y checkpoint E-JWT; componentes 043–046.
- **Condiciones de entrada:** Firma/claims/tiempo y políticas disponibles, marcas/TTL/confirmación/cuotas y admisión confiable; sin necesidad de login público implementado.
- **Condiciones de salida:** 043–046 probadas: 204 solo confirmado, repetido vigente idempotente/TTL no decreciente, 401 inválido, 503 no verificable; token A revocado entre hosts sin afectar B, excepción exclusiva y sin consulta MongoDB artificial.
- **Bloqueos conocidos:** Confirmación/lecturas inciertas o confianza perdida impiden 204; excepción reutilizable fuera de logout bloquea aceptación. Solicitudes ya autorizadas antes de revocar conservan el límite documentado.
- **Evidencia necesaria:** E(T-AUTH-043) a E(T-AUTH-046), AC-11/12/13/14 con dos hosts y fallo MongoDB independiente; pérdida/recuperación integral en G.

### GATE-G

- **Nombre:** Security testing.
- **Tareas de entrada:** GATE-D, E completo y F; pipeline 059; componentes 060–068.
- **Condiciones de entrada:** Suites y pipeline integrados, modelo de amenazas y perfiles/fallos/objetivos autorizados; topología externa disponible antes de 068.
- **Condiciones de salida:** 059–068 con regresión, fallos/ataques/privacy/carga, detectores reales y recuperación/cuarentena demostrados en su alcance; 068 contrasta garantías externas y tratamiento residual. Casos negativos no pueden tener aceptación indebida ni exposición de secretos dentro del conjunto aprobado.
- **Bloqueos conocidos:** Un detector no probado o únicamente un flag inyectado no acredita AC-15; entorno externo ausente bloquea su cierre integral, aunque se registre checkpoint local. AD-09/10/11/12 conservan sus puertas.
- **Evidencia necesaria:** E(T-AUTH-059) a E(T-AUTH-068), resultados/amenazas/AC, latencia/propagación y representatividad; no ausencia universal de pérdida Redis o vulnerabilidades.

### GATE-H

- **Nombre:** DevSecOps.
- **Tareas de entrada:** Política 069 y resultados de G según alcance; componentes 070–078. Preparación local/CI independiente puede avanzar autorizada según dependencias sin simular cierre externo.
- **Condiciones de entrada:** Herramientas/versiones/severidades/licencias/excepciones aprobadas, baseline/artefacto identificables, contexto protegido, Docker/GitHub/permisos disponibles.
- **Condiciones de salida:** Jobs/scanners reales exitosos conforme política, hallazgos tratados, imagen/digest/base revisados, non-root/ausencia de filtración detectada en alcance; required checks solo después de runs válidos. 079 consolida cierre tras todas sus dependencias, incluidas externas.
- **Bloqueos conocidos:** Scanner fallido/ausente, tests fallidos/suites omitidas, secretos activos, excepción vencida, permisos o imagen no disponibles; pendiente 068 no se elimina por CI verde.
- **Evidencia necesaria:** E(T-AUTH-069) a E(T-AUTH-079), commits/runs/reportes/digests reales; checks build/unit-tests/integration-tests/security-scan verificados. No prueba AWS ni autoriza despliegues.

## 5. Orden crítico y dependencias

Orden funcional aprobado:

```text
Base → Redis → MongoDB → Validación/Hashing → Register → JWT
→ Logout → Login → Pipeline final → Seguridad integral
→ DevSecOps/Docker → Evidencia/cierre
```

Las dependencias reales prevalecen sobre número de tarea/fase. F11 se adelanta como dependencia de F3; 056/057 se adelantan para probar Register; E tiene checkpoint JWT antes de F y cierre Login después de F. Secuencia de gates: **A → B → C → D → E-JWT → F → E completo → G → H**.

Ramas de dependencia críticas, sin convertirlas en nuevos IDs/tareas:

- Base 001–015; Redis conecta 016 → 018 → 019 → 017 → 020. El modelo de 019 antecede al almacén de revocación de 017, sin requerir JWT/Login implementados.
- Cuotas: 012/017 → 052; 008/012/016 → 053; 008/020/052 → 054; 052–054 → 021 → 055 → 022. Son capacidades de un único rate limiter.
- Tras 022, MongoDB 023–027; validación usa primero 029 (auxiliares Unicode) y después 028/030. Hashing 031/033 y CAS/rehash 032 completan sus entradas según ficha.
- Register 034–037 requiere composición 056 → 057; JWT 038–042 → Logout 043–046 → Login 047–051 → composición 058 → pipeline 059.
- Seguridad 060–068; configuración política 069 puede prepararse antes por dependencia 003. Docker 076 → 077 → 078 permite 073; no se exige 073 para construir la imagen que ese scanner consumirá, evitando ciclo CI/Docker.
- CI 070/071/072/074 y scan 073 → required checks 075; 068/075/078 → cierre 079. Contexto/publish 005 siempre precede al primer build sensible, no solo a Docker final.

## 6. Paralelismo y commits recomendados

### Paralelismo condicionado

| Capacidades potencialmente independientes | Condiciones |
| --- | --- |
| Higiene 004 frente a inventario/versiones 002/003 | Autorización y archivos distintos; ninguna restauración/instalación implícita fuera de alcance |
| Soporte de tests 013/014 frente a módulos HTTP independientes | Versiones aprobadas; integrar cambios compartidos en orden, 015 espera Options |
| Identidad 028 frente a hashing 031/033 | Auxiliares 029 y modelo 023 estabilizados; no editar simultáneamente mismo servicio/validador |
| Validación JWT 039 y rotación 041 | Emisión/key ring 038 terminados, cambios en archivos separados y pruebas aisladas |
| Ataques JWT 064, entradas 065 y privacidad 066 | Pipeline estable, fixtures/datos aislados y recursos suficientes; ninguna prueba altera Redis compartido con otra |
| Configuraciones separadas de scanners/política | 069 aprobada; integración de workflow/artefactos serializada, sin anticipar checks |

Las pruebas de pérdida/restauración Redis 062/063 y fallos de dependencias no se ejecutan concurrentemente sobre el mismo estado sin aislamiento explícito. No se altera el orden funcional por un supuesto paralelismo ni se invocan subagentes por esta tabla.

**Edición serializada obligatoria** cuando afecte a `Enver.API/Program.cs` (056 → 057 → 058), `Enver.API/Enver.API.csproj`, lockfiles, `Enver.Backend.slnx`, `AuthenticationService.cs`, `AuthenticationController.cs`, `AuthenticationInputs.cs`, `AuthenticationResults.cs`, `tests/Shared/*` y `.github/workflows/ci.yml`. Definir propietario/integrador y revisar diff antes de ampliar un archivo compartido; no asumir independencia por tener IDs distintos.

### Puntos de commit recomendados, no ejecutados

Revisar/versionar la baseline documental pendiente antes de implementación; posteriormente, el catálogo aprobado puede tener su commit separado. No se exige un commit por tarea: una capacidad puede agrupar cambios y pruebas en uno o pocos commits relacionados, sin retrasar seguridad hasta el final. Commit/push necesitan autorización propia.

| Capacidad | Intención recomendada |
| --- | --- |
| Baseline documental previa | `docs(auth): record approved authentication baseline` |
| Este catálogo | `docs(auth): define authentication implementation tasks` |
| Baseline SDK/restore | `chore(auth): establish reproducible development baseline` |
| Contexto/publish | `chore(container): protect build and publish inputs` |
| HTTP/configuración | `feat(auth): add validated configuration and HTTP safeguards` |
| Tests/fixtures | `test(auth): establish authentication test infrastructure` |
| Redis/revocación/admisión | `feat(auth): add revocation and security admission controls` |
| Cuotas | `feat(auth): add distributed authentication rate limiting` |
| MongoDB | `feat(auth): add mongodb user persistence` |
| Validación/hashing | `feat(auth): add identity validation and password hashing` |
| Register | `feat(auth): implement registration` |
| JWT | `feat(auth): implement jwt authentication` |
| Logout | `feat(auth): implement logout` |
| Login | `feat(auth): implement login` |
| Seguridad | `test(auth): verify authentication security and resilience` |
| Docker | `chore(container): harden and verify authentication image` |
| CI | `ci(auth): add authentication quality gates` |
| Evidencia | `docs(auth): record verification evidence and limitations` |

## 7. Puertas de ejecución y decisiones diferidas

Una decisión diferida **NO bloquea escribir este task.md**, pero **sí bloquea ejecutar o aceptar la tarea afectada** en el punto indicado. No se reabren decisiones funcionales aprobadas ni se transforma un candidato en selección final tácita. Cambiar una garantía o alcance exige nueva autorización y actualización de fuente cuando proceda.

| Asunto / fuente | Puerta y tareas afectadas |
| --- | --- |
| Autorización técnica/baseline Git | Antes de cualquier ejecución: 001/006; conservar cambios y obtener autorización por tarea/incremento, archivos/comandos/entorno. El catálogo no la sustituye |
| Versiones SDK/NuGet/runner/imágenes y vulnerabilidades/licencias/grafo | 002/003 antes de instalar/fijar; respetar también en 014/016/023/038/076 y futuras actualizaciones |
| Topología/concerns MongoDB, journaling/ACK y APIs | Antes de construir/aceptar 023–027; replica set representativo cuando corresponda y contraste Atlas 068; sin repair/sharding automático |
| Redis ACK/lecturas, continuidad y fuentes reales de detección | Modelo/entorno aprobado en entrada de 019 antes de 017/019/020; señales/propagación/fencing/límites, no única confianza colocalizada; evidencias 022/062/063/068 |
| Redis capacidad/noeviction/TLS/ACL/monitoreo/recuperación | Verificación de entorno 022, cierre seguro 020; detalles de servicio administrado/operación pendientes 068 e infraestructura, sin imponer separación física |
| Precisión temporal/margen TTL/rotación JWT | 017/038/041 antes de aceptar validación/revocación; 60 s es candidato, skew máximo 30 s y vida inicial 900 s conservados |
| Entradas inválidas y precedencia de errores | 012 antes de construir cuotas/MVC en 021/058; sin cambiar 400/429/503 funcionales ni eludir controles |
| Cardinalidad | Topes numéricos y presupuesto de memoria aprobados en entrada 054 antes de 021; TTL solo no basta, capacidad insuficiente →503 |
| Transición HMAC/políticas | Protocolo aprobado en 053 antes de construir/aceptar control y antes de cualquier rotación/cambio real; no resetear ventanas |
| Recuperación de contadores | Política 054 antes de aceptar 021/055; reconstrucción confiable o cierre durante ventana restante, distinta de revocaciones |
| AD-09, calibración | Perfil/muestras/tolerancias/presupuestos en 067 antes de aceptación correspondiente; candidatos de latencia no son SLA ni justifican reducir hashing |
| AD-10, retención/operación/privacidad | Prohibiciones y minimización desde 008/010/036/066; esquema/accesos/retención/eliminación/exportación/gestor seguro antes de producción, sin plazo/servicio inventado |
| AD-11, AWS/Terraform/TLS/LB/redes/readiness/observabilidad | Incremento de infraestructura separado con recursos/coste/permisos autorizados. 053/056/059/068/076–078 no crean ni despliegan AWS; prueba local no satisface infraestructura |
| AD-12, herramientas/gates/rúbrica/OWASP | 069 antes de aceptar controles/evidencia; versiones, umbrales/licencias/excepciones y evidencia académica; aviso OpenAPI verificable en 002/072 |
| GitHub/runners/registros/objetivos externos | Autorización/conectividad/credenciales mínimas en las tareas consumidoras, especialmente 068/070–075/077/078; sin secretos productivos en PR no confiables |

Docker se requiere para fixtures/pruebas reales consumidoras y build/run/scan de imagen; NuGet/registros/feeds/GitHub y 068 pueden requerir conexión externa autorizada. Esta identificación no autoriza conectarlos aquí. Si un servicio representativo exige AWS todavía no disponible, la tarea afectada permanece bloqueada hasta la entrega de infraestructura correspondiente, no se añade despliegue tácito a 002.

**AUTH-027: NO IMPLEMENTADO / NO SATISFECHO.** Tareas principales: [018](#t-auth-018), [019](#t-auth-019), [020](#t-auth-020), [022](#t-auth-022), [062](#t-auth-062), [063](#t-auth-063), [068](#t-auth-068). APR-03 resolvió la discrepancia normativa con PLD-AUTH-08; no es bloqueo documental vigente. La pérdida completamente silenciosa no observable es riesgo residual de infraestructura, no eliminada ni detectable universalmente. Una señal disponible no se ignora para llamarla no observable. Ping/persistencia/WAIT/marcador colocalizado/pruebas finitas no demuestran ausencia universal; falta evidencia de detección, coordinación, recuperación y operación. Sin ledger MongoDB, segundo almacén duradero obligatorio o transacciones distribuidas.

Riesgos conservados: inferencia parcial 201/409, dominio sin titularidad verificada, NAT universitario, Bearer robado reutilizable antes de revocación/expiración, todo poseedor HS256 puede emitir, resultados inciertos no prueban ausencia de efecto, namespaces no aíslan memoria y pruebas locales no acreditan cloud. No afirmar seguridad universal ni cumplimiento académico sin rúbrica/evidencia.

## 8. Matrices de trazabilidad

Las matrices muestran **referencias principales**, no necesariamente todas las relaciones de cada ficha. Todas las filas deben enlazar tareas existentes que declaren ese AUTH/PLD/AC; las fichas conservan el detalle completo. La asociación **NO significa cumplimiento**, prueba ejecutada, gate satisfecho ni aceptación de infraestructura. Evidencia técnica de las filas: PENDIENTE.

### AUTH → tarea

| AUTH | Tareas principales |
| --- | --- |
| AUTH-001 | 001,006,079 |
| AUTH-002 | 023,028,034,037 |
| AUTH-003 | 011,035,040,044,047,049,058 |
| AUTH-004 | 023–025,035,037,061 |
| AUTH-005 | 024,028,030,037 |
| AUTH-006 | 029–031,034 |
| AUTH-007 | 023,028,030,034 |
| AUTH-008 | 011,012,028–030,065 |
| AUTH-009 | 024,025,027,035,037 |
| AUTH-010 | 031,032,047–049,051 |
| AUTH-011 | 010,031,033,048,050,067 |
| AUTH-012 | 023,026,032,048 |
| AUTH-013 | 026,027,031,032,048 |
| AUTH-014 | 023,026,027,031,032 |
| AUTH-015 | 007,023–025,027,057 |
| AUTH-016 | 038,042,049,051 |
| AUTH-017 | 038,039,042,049 |
| AUTH-018 | 011,039,040,042,064 |
| AUTH-019 | 008,038,039,041 |
| AUTH-020 | 008,017,038,039,041 |
| AUTH-021 | 038,040,046,049,064 |
| AUTH-022 | 017,019,020,043,046,062 |
| AUTH-023 | 017,040,043,044,046 |
| AUTH-024 | 017,020,041,043,063 |
| AUTH-025 | 016,019,022,054,068 |
| AUTH-026 | 018–020,040,046,061 |
| AUTH-027 | 018–020,022,062,063,068 |
| AUTH-028 | 021,036,045,050,052–055 |
| AUTH-029 | 008,021,052,055 |
| AUTH-030 | 011,053–055,065,067 |
| AUTH-031 | 010,012,024,040,066 |
| AUTH-032 | 005,010,036,050,066,078 |
| AUTH-033 | 053,056,059,068 |
| AUTH-034 | 004,005,008,038,053,078 |
| AUTH-035 | 006,011,040,046,058,064 |
| AUTH-036 | 010,036,045,050,053,066 |
| AUTH-037 | 010,036,045,050,059,066 |
| AUTH-038 | 009,018,024,033,061 |
| AUTH-039 | 017,026,027,032,043,061 |
| AUTH-040 | 033,054,065,067,069 |
| AUTH-041 | 013,014,030,031,060,070 |
| AUTH-042 | 015,022,027,037,046,051,071 |
| AUTH-043 | 019,030,062–068,071 |
| AUTH-044 | 002–005,072–074,078 |
| AUTH-045 | 003,005,070,072–078 |
| AUTH-046 | 059,068,075,076,079 |
| AUTH-047 | 001,006,060,069–073,079 |
| AUTH-048 | 001,006,069,075,079 |

Cobertura documental AUTH: **48/48**. Gobierno/continuidad y requisitos de plataforma no se convierten en endpoints o trabajo AWS para aparentar cumplimiento.

### PLD-AUTH → tarea

| PLD-AUTH | Tareas principales |
| --- | --- |
| PLD-AUTH-01 | 006,013 |
| PLD-AUTH-02 | 006,012,017,021,024,034,043,047 |
| PLD-AUTH-03 | 002,003,013,014,016,023,031,038,072 |
| PLD-AUTH-04 | 007,008,029,038,056 |
| PLD-AUTH-05 | 023–027,035,037,057,068 |
| PLD-AUTH-06 | 025,027,037,057 |
| PLD-AUTH-07 | 016,017,022,054,057,068 |
| PLD-AUTH-08 | 018–020,022,054,061–063,068 |
| PLD-AUTH-09 | 017,020,038,041,043,063 |
| PLD-AUTH-10 | 038,039,041,042,049,064 |
| PLD-AUTH-11 | 040,042–044,046,058,064 |
| PLD-AUTH-12 | 026,029,031,032,034,048,051 |
| PLD-AUTH-13 | 011,012,028–030,034,039,065 |
| PLD-AUTH-14 | 010–012,024,035,040,044,049,052 |
| PLD-AUTH-15 | 012,021,036,045,050,052,054,055 |
| PLD-AUTH-16 | 016,021,036,045,050,053 |
| PLD-AUTH-17 | 053,055,056,068 |
| PLD-AUTH-18 | 009,011,016,033,054,055,061,067 |
| PLD-AUTH-19 | 010–012,035,040,044,049,056–059 |
| PLD-AUTH-20 | 013–015,022,027,030,037,042,046,051,060–065,067,068 |
| PLD-AUTH-21 | 002,003,069–078 |
| PLD-AUTH-22 | 004,005,066,073,076–078 |
| PLD-AUTH-23 | 010,036,045,050,056,066,067 |
| PLD-AUTH-24 | 018,019,025,046,057,059,062,068 |
| PLD-AUTH-25 | 001,006,060,069,075,079 |

Cobertura documental PLD-AUTH: **25/25**. Estados de las decisiones y candidatos no se alteran; fuentes completas en plan §21.

### AC → tarea/prueba

| AC | Tareas principales | Prueba/evidencia requerida, todavía no ejecutada |
| --- | --- | --- |
| AC-01 | 023,034,035,037,077 | Registro sin teléfono, Active/UUID/timestamps/hash y 201 confirmado; sin token automático; IT/inspección BSON/smoke |
| AC-02 | 028,030,037 | Dominio exacto, subdominios/sufijos engañosos rechazados 400 sin cuenta/token; UT/ST/IT |
| AC-03 | 025,027,028,037 | Normalización/índice y carreras A/B crean máximo una cuenta; 409 seguro; IT con dos hosts |
| AC-04 | 029–031,037,051 | 11/12/128/129 escalares y bytes/Unicode; login separado de creación; UT/IT |
| AC-05 | 011,028,030,037,065 | Unknown/duplicate/null/tipos/operadores/mass assignment y fullName; rechazo seguro sin persistencia; UT/ST/IT |
| AC-06 | 026,032,038,048,049,051 | Dos sesiones/jti distintos, rehash/CAS/updatedAt y respuesta 200 exacta; UT/IT con Mongo real |
| AC-07 | 031,048,050,051,067 | Dummy previo, mismo 401 inexistente/incorrecto, cuotas/timing sin prometer igualdad; UT/IT/ST |
| AC-08 | 032,048,051,067 | Disabled comparable/401 genérico/sin token ni motivo; CAS/estado y evaluación temporal |
| AC-09 | 039,040,042,064 | Firma/algoritmo/issuer/audience/kid/claims alterados o ausentes; rechazo antes de confiar en identidad; UT/ST/IT |
| AC-10 | 017,038,039,041,042,064 | iat/exp/skew/nbf y retención antes/en/después; tiempo controlado y Redis real |
| AC-11 | 040,043,046,064 | Logout confirmado en host 1 rechaza A en 2, B válido; IT distribuida/ST replay |
| AC-12 | 017,043,044,046 | Logout vigente repetido/concurrente/respuesta perdida 204, TTL no decreciente, incierto 503; IT Redis |
| AC-13 | 011,040,044,046,064 | Ausente/falso/expirado 401, sin marcas indebidas; cuerpo/token/canal correctos; IT/ST |
| AC-14 | 018,021,046,055,061 | Redis no verificable en acceso/logout/cuota →503 y sin bypass; IT de fallos |
| AC-15 | 019,020,022,062,063,068 | Detector real/evento/latencia/propagación, cierre compartido y recuperación/cuarentena en dos hosts; evidencia diferenciada local/externa |
| AC-16 | 021,036,045,050,052–055 | Siete cuotas conjuntas/ventanas, Redis TIME/Retry-After, HMAC/origen/NAT/IPv6; UT/IT/ST |
| AC-17 | 024,027,046,051,061,068 | Mongo caído/ACK perdido: sin éxito o inexistencia falsos, sin duplicados/reintento ciego; IT local/externa |
| AC-18 | 011,029,033,054,055,065,067 | Cuerpo/profundidad/Content-Type/JWT/Unicode/cardinalidad/hashing; rechazos y recursos acotados; UT/IT/ST/carga |
| AC-19 | 010,036,045,050,066,078 | Respuestas/logs/trazas/métricas/persistencia/contexto/capas sin secretos sintéticos filtrados; no-store y minimización; ST/inspección |
| AC-20 | 020,041,053,063,068 | Rotación/configuración/coexistencia/retiro y pérdida de instancia sin afinidad ni ventana sin revocación; UT/IT/externa |
| AC-21 | 069–079 | Runs build/unit/integration/security/SAST/Audit/secret/container, políticas, commits/digests/reportes reales; no gate por documentación |

Cobertura documental AC: **21/21**. Cada referencia usa E(T-AUTH-nnn) de la tarea correspondiente; un escenario puede necesitar varias evidencias. AC-21 en 079 es consolidación de resultados previos, no scanner adicional.

## 9. Continuidad y cierre documental

El catálogo contiene exclusivamente las 79 tareas aprobadas, con fases F0–F15 y estado inicial PENDIENTE / NO EJECUTADA. Los ocho gates conservan evidencia pendiente y las decisiones diferidas sus puertas. No se elimina seguridad al agrupar trabajo, ni se declara implementado/satisfecho ningún AUTH por este documento.

Se mantienen fuera del alcance funcional MFA, refresh tokens, recuperación/cambio de contraseña, OAuth externo, administración completa, repartidor/CLABE, chat, pagos, pedidos y push; tampoco se inventan verificación institucional, endpoints de consulta o cierre global ordinario. AWS/Terraform/despliegue/servicios/readiness pública requieren entrega independiente, no tareas técnicas implícitas en este catálogo.

La validación de esta entrega se limita a estructura documental, IDs/campos/estados, referencias, matrices, dependencias/gates, coherencia con fuentes y formato Git. No es ejecución de E(T-AUTH-nnn), build, prueba del backend, scanner, conexión o pipeline. Tras registrar el resultado documental y sincronizar AGENTS/memoria/README se detiene el trabajo; cualquier tarea necesita autorización posterior independiente.
