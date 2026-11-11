# Tareas 002 — Autenticación de ENVER Backend

## 1. Identificación, fuentes y autorización

| Elemento | Valor |
| --- | --- |
| Solución / proyecto | `Enver.Backend` / `Enver.API` |
| Versión documental | 0.1.0 |
| Estado del documento | **APROBADO PARA EJECUCIÓN INCREMENTAL** |
| Catálogo vigente | 79 tareas: T-AUTH-001 a T-AUTH-079 |
| Estado inicial de todas las tareas | PENDIENTE / NO EJECUTADA |
| Estado tras el cierre de T-AUTH-001 | 1 COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA; 78 PENDIENTE / NO EJECUTADA; evidencia en §10 |
| Estado tras el cierre de T-AUTH-002 | 2 COMPLETADAS / EVIDENCIA DOCUMENTAL REGISTRADA: 001 y 002; 77 PENDIENTE / NO EJECUTADA: 003–079; evidencia de 002 en §11 |
| Estado tras el cierre de T-AUTH-003 | 001/002 COMPLETADAS / EVIDENCIA DOCUMENTAL REGISTRADA; 003 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA; 76 PENDIENTE / NO EJECUTADA: 004–079; evidencia de 003 en §12 |
| Estado tras el cierre de T-AUTH-004 | 001/002/004 COMPLETADAS / EVIDENCIA DOCUMENTAL REGISTRADA; 003 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA; 75 PENDIENTE / NO EJECUTADA: 005–079; evidencia de 004 en §13 |
| Estado tras el cierre de T-AUTH-005 | 001/002/004 COMPLETADAS / EVIDENCIA DOCUMENTAL REGISTRADA; 003/005 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA; 74 PENDIENTE / NO EJECUTADA: 006–079; evidencia de 005 en §14 |
| Estado del checkpoint T-AUTH-006 | 001–005 conservan sus cierres; 006 COMPLETADA / EVIDENCIA TÉCNICA Y CHECKPOINT GIT REGISTRADOS, sujeto a commit único correcto y verificación posterior limpia según §15; 73 PENDIENTE / NO EJECUTADA: 007–079 |
| Estado posterior del bloque 007/008 | 001–006 conservan sus cierres; 007 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§16); 008 IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE (§17); 009–079: 71 PENDIENTE / NO EJECUTADA |
| Estado posterior del bloque 009–012 | 001–007 conservan sus cierres; 008 mantiene verificación de startup pendiente; 009–012 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA como componentes aislables (§§18–21); 013–079: 67 PENDIENTE / NO EJECUTADA |
| Estado posterior de 013–014 | 001–007 y 009–012 conservan sus cierres; 008 mantiene verificación de startup pendiente; 013/014 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA (§22); 015–079: 65 PENDIENTE / NO EJECUTADA |
| Estado posterior de 015 | 001–007 y 009–015 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA; 008 mantiene IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE; 016–079: 64 PENDIENTE / NO EJECUTADA; 8 gates pendientes / no satisfechos |
| Estado vigente tras cierre formal de GATE-A | 001–007 y 009–015 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA; 008 mantiene IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; 016–079: 64 PENDIENTE / NO EJECUTADA; GATE-A SATISFECHO / CERRADO; GATE-B a GATE-H PENDIENTES / NO SATISFECHOS |
| Estado posterior de 016 | 001–007 y 009–016 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA; 008 mantiene IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; 017–079: 63 PENDIENTE / NO EJECUTADA; GATE-A SATISFECHO / CERRADO; GATE-B a GATE-H PENDIENTES / NO SATISFECHOS |
| Estado posterior de 018 | 001–007 y 009–016/018 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA; 008 mantiene IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; 017 y 019–079: 62 PENDIENTE / NO EJECUTADA; GATE-A SATISFECHO / CERRADO; GATE-B a GATE-H PENDIENTES / NO SATISFECHOS |
| Estado posterior de 019 | 001–007 y 009–016/018–019 COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA; 008 mantiene IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; 017 y 020–079: 61 PENDIENTE / NO EJECUTADA; GATE-A SATISFECHO / CERRADO; GATE-B a GATE-H PENDIENTES / NO SATISFECHOS |
| Estado posterior de 020-B | 001–007, 009–017 y 018–019 conservan sus cierres; 008 mantiene IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; **020-A COMPLETADA / EVIDENCIA LOCAL REGISTRADA, 020-B COMPLETADA / EVIDENCIA DYNAMODB LOCAL REGISTRADA y T-AUTH-020 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**; 021–079: 59 PENDIENTE / NO EJECUTADA; GATE-A SATISFECHO / CERRADO; GATE-B a GATE-H PENDIENTES / NO SATISFECHOS |
| Estado posterior de 052 | Cierres anteriores y estado de 008 conservados; **052 COMPLETADA / EVIDENCIA DE CONTRATO REGISTRADA** (§32); 021–051 y 053–079: 58 tareas no ejecutadas, incluida **021 BLOQUEADA POR 053 Y 054**; 053/054 pendientes; GATE-B a GATE-H pendientes/no satisfechos; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE |
| Estado posterior de 053 | Cierres anteriores conservados; **053 COMPLETADA / EVIDENCIA AISLABLE REGISTRADA** (§33); 008 mantiene startup productivo pendiente; 021–051 y 054–079: 57 tareas no ejecutadas, incluida **021 BLOQUEADA POR 054**; 054 pendiente; GATE-B a GATE-H pendientes/no satisfechos; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE |
| Estado posterior del incremento contractual 054 | **054 COMPLETADA EN SU INCREMENTO CONTRACTUAL / PRERREQUISITO DE DISEÑO SATISFECHO**, con **EVIDENCIA IT DISTRIBUIDA PENDIENTE EN T-AUTH-021/T-AUTH-055 PARA CIERRE DE GATE-B** (§34); **021 DESBLOQUEADA RESPECTO A 054 / NO INICIADA**; 021–051 y 055–079: 56 tareas no ejecutadas; 008 conserva startup productivo pendiente; cierres anteriores intactos, GATE-B a GATE-H pendientes/no satisfechos y AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE |
| Estado posterior del cierre de 021 | **021 COMPLETADA / EVIDENCIA REDIS DISTRIBUIDA REGISTRADA** (§35); **054: EVIDENCIA IT ESENCIAL ASIGNADA A T-AUTH-021 SATISFECHA EN EL ALCANCE REGISTRADO** (§34.1), sin cerrar su ampliación en 055. 022–051 y 055–079: 55 tareas no ejecutadas; **055 PENDIENTE; 022 NO INICIADA**. 008 conserva startup productivo pendiente; cierres anteriores intactos; **GATE-B PENDIENTE / NO SATISFECHO; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE** |
| Estado posterior del cierre de 055 | **055 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA** (§36): presión distribuida P1/P2, capacidad 5000, lazy cleanup, siete cuotas, timeout/Uncertain, OOM real, restart/no-reset/fence/recovery, autoridad 020, revisión/snapshot divergente y ST de seguridad 6/6; suites UT **333/333**, IT **97/97**, ST **69/69**. **054: AMPLIACIÓN DE EVIDENCIA ASIGNADA A T-AUTH-055 SATISFECHA EN EL ALCANCE REGISTRADO** (§34.2). 022–051 y 056–079: 54 tareas no ejecutadas; **022 NO INICIADA**. 008 conserva startup productivo pendiente; cierres anteriores intactos; **GATE-B PENDIENTE / NO SATISFECHO; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE** |
| Estado posterior del cierre de 022 | **022 COMPLETADA / EVIDENCIA OPERATIVA REDIS REGISTRADA** (§37): dos hosts lógicos de test con Redis real compartido, estado local independiente, revocación A→B, TTL sin reducción, ausencia de caché negativa, `noeviction`, memoria/headroom medidos, separación ambiente/propósito, cierre compartido sin bypass y ST de cierre/privacidad 2/2; suites UT **333/333**, IT **100/100**, ST **71/71**. 023–051 y 056–079: 53 tareas no ejecutadas; **023/056/057/058 NO INICIADAS**. 008 conserva startup productivo pendiente; cierres anteriores intactos; **GATE-B PENDIENTE / NO SATISFECHO; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE** |
| Cobertura documental prevista | AUTH 48/48; PLD-AUTH 25/25; AC 21/21 |
| Estado posterior del cierre de 023 | **023 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA** ([§39](#e-t-auth-023)): BSON mínimo, cliente aislable, concerns/ACK/journaling locales y privacidad del harness; UT **371/371**, IT **104/104**, ST **71/71**. 024–051 y 056–079: 52 tareas no ejecutadas. 008 conserva startup productivo pendiente; GATE-A/B SATISFECHOS / CERRADOS; GATE-C–H PENDIENTES / NO SATISFECHOS; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. |
| Estado posterior del cierre de 024/025 | **024 y 025 COMPLETADAS / EVIDENCIA REGISTRADA** ([§40](#e-t-auth-024), [§41](#e-t-auth-025)): UserStore e inicializador aislables; UT **414/414**, IT **120/120**, ST **71/71**. 026–051 y 056–079: 50 tareas no ejecutadas; **026/027 NO INICIADAS**. 008 conserva startup productivo pendiente; **GATE-A/B SATISFECHOS / CERRADOS; GATE-C–H PENDIENTES / NO SATISFECHOS**; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. |
| Estado posterior del cierre de 026/027 y revisión de GATE-C | **026 y 027 COMPLETADAS / EVIDENCIA REGISTRADA** ([§42](#e-t-auth-026), [§43](#e-t-auth-027)); UT **436/436**, IT **129/129**, ST **71/71**. 028–051 y 056–079: 48 tareas no ejecutadas. **GATE-C SATISFECHO / CERRADO como checkpoint local** ([§44](#cierre-gate-c)); no integración productiva ni Atlas. 008 mantiene startup productivo pendiente; 056/057/058 NO INICIADAS; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. |
| Estado posterior de BUILD-1 | **056/029/028/030/031/033/057 COMPLETADAS / EVIDENCIA REGISTRADA**, separadamente en §§46–52; BUILD-1 aceptado tras la regresión final de §45: build **0/0**, UT **493/493**, IT **154/154**, ST **79/79**, cero fallidas/omitidas. 032, 034–051 y 058–079: 41 tareas no ejecutadas; 008 conserva su estado documental previo. **GATE-A/B/C SATISFECHOS / CERRADOS; GATE-D–H PENDIENTES / NO SATISFECHOS**. Sin Register ni autenticación funcional publicada; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. |
| Estado posterior de BUILD-2 | **034/035/036/037 COMPLETADAS / EVIDENCIA REGISTRADA** en §§55–58; **BUILD-2 ACEPTADO** según §54: UT **506/506**, IT **186/186**, revalidación post-limpieza origin **1/1** y Register/composición **39/39**, ST completas **84/84**; cero fallidas/omitidas. **GATE-D SATISFECHO / CERRADO, LOCAL/TEST REPRESENTATIVO** (§59). 032, 038–051 y 058–079: 37 tareas no ejecutadas; 008 conserva su estado documental previo. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. |
| Estado posterior del checkpoint E-JWT | **038–042 COMPLETADAS / EVIDENCIA REGISTRADA**, separadas en §§61–65; regresión actual UT **586/586**, IT **191/191**, ST **120/120**, 0 fallidas/omitidas (§60). **E-JWT SATISFECHO / CERRADO, LOCAL/TEST REPRESENTATIVO** (§66); **GATE-E completo y GATE-F PENDIENTES**. 032, 043–051 y 058–079: 32 tareas no ejecutadas; 008 conserva startup productivo pendiente. Sin Login/Logout ni 058 integral; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. |
| Estado posterior de Logout / GATE-F | **043–046 COMPLETADAS / EVIDENCIA REGISTRADA** en §§69–72; **GATE-F SATISFECHO / CERRADO, LOCAL/TEST REPRESENTATIVO** (§73). UT previas **633/633** conservadas; IT actuales **259/259**, ST finales **152/152**, 0 fallidas/omitidas (§67). **GATE-B CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA** (§68). 032, 047–051 y 058–079: 28 tareas no ejecutadas; 008 conserva su estado previo. Register y JWT conservan aceptación local/test; Logout completo local/test; Login pendiente y autenticación NO TERMINADA. |
| Gates | 8; GATE-A/B/C/D/F SATISFECHOS / CERRADOS en sus alcances registrados; GATE-E/G/H PENDIENTES / NO SATISFECHOS |
| Fuente de aprobación | SOL-TASK-002: aprobación expresa del catálogo compactado y autorización documental del responsable |
| Estado técnico | Autenticación NO IMPLEMENTADA / NO VERIFICADA por esta entrega; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO |

Fuentes, en su función normativa:

1. [Constitución canónica](../../.specify/memory/constitution.md), v1.0.0, y su [referencia raíz](../../constitution.md), sin autoridad independiente.
2. [AGENTS.md](../../AGENTS.md), [memoria.md](../../memoria.md) y [README.md](../../README.md): actuación, estado y antecedentes.
3. [Spec 002](spec.md), v0.3.0, **APROBADO PARA PLANIFICACIÓN**: fuente funcional; especialmente §§3–8 y 10.
4. [Plan 002](plan.md), v0.2.0, **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**: arquitectura, decisiones PLD-AUTH-01 a PLD-AUTH-25, incrementos §19 y puertas §23.2.
5. SOL-TASK-002 identifica únicamente la solicitud conversacional que aprueba estas 79 tareas y autoriza crear este archivo y sincronizar los tres documentos de estado. No es evidencia técnica, commit ni aprobación de los detalles diferidos.

La entrega de creación SOL-TASK-002 fue exclusivamente documental y no ejecutó ninguna tarea del catálogo. Se conservan intactos spec/plan, ambas referencias constitucionales y los [tres antecedentes de foundation](../001-backend-foundation/). Las restricciones de entregas anteriores se interpretan en su contexto histórico; esta autorización posterior no reforma la constitución, aprueba retrospectivamente 001 ni cierra FND/PLD/TBF/DEC. Memoria §16 registra aquella entrega, sin borrar §§7–15. El cierre posterior autorizado únicamente de T-AUTH-001 se registra en §10 de este documento y memoria §17; no implica implementación ni cierre de gates.

Otro acto posterior autoriza cerrar únicamente T-AUTH-002, cuya evaluación previa fue dictaminada LISTA PARA CIERRE DOCUMENTAL; registro en §11 y memoria §18. No altera E(T-AUTH-001), aprueba versiones definitivas, inicia T-AUTH-003 ni satisface gates.

La autorización BUILD posterior de T-AUTH-003 permite únicamente fijar la baseline existente, generar/verificar lockfile, restaurar, compilar y auditar; tras éxito técnico, registrar E(T-AUTH-003) en §12 y memoria §19. Se ejecuta y cierra solo 003, sin implementar autenticación, iniciar 004 ni cerrar gates; los cierres e historiales anteriores se conservan.

Otro acto posterior aprueba el dictamen LISTA PARA CIERRE DOCUMENTAL de la inspección de solo lectura de T-AUTH-004 y autoriza registrar únicamente su cierre en este documento (§13) y memoria (§20). No repite la inspección, modifica reglas/configuración, inicia T-AUTH-005 ni satisface gates; las evidencias y estados de 001–003 se conservan.

La autorización BUILD posterior de T-AUTH-005 permite corregir exclusivamente .dockerignore, Enver.API/Dockerfile y Enver.API.csproj después del diagnóstico REQUIERE CORRECCIÓN; solo tras éxito de las verificaciones locales autorizadas, registrar su cierre en §14 y memoria §21. No autoriza Docker, publish real, implementación de autenticación, inicio de T-AUTH-006 ni satisfacción de gates; conserva las evidencias e historia anteriores.

El acto BUILD CONTROLADO posterior de T-AUTH-006 autoriza consolidar ownership/integración en este documento y memoria, preparar E(T-AUTH-006) antes de un único commit y confirmar después un working tree limpio. El responsable amplía expresamente el alcance del commit a los cierres históricos válidos 001/002, intactos, además de 003–006, únicamente en las siete rutas de §15. No permite push, iniciar 007, crear componentes futuros o repetir restore/build sin inconsistencia técnica.

Una autorización BUILD CONTROLADO posterior permite exclusivamente 007/008: seis archivos Configuration, ambos appsettings no sensibles, restore bloqueado/build y comprobaciones controladas; después, evidencia en §§16–17 y memoria §23. No autoriza Program.cs, F2, conexiones o autenticación funcional, dependencias nuevas, commit o push. El mecanismo ValidateOnStart queda preparado, no integrado en el host; 008 no se declara completada.

La autorización posterior para 009–012 exige ejecución secuencial y comprobación antes de avanzar, preserva los cambios 007/008 y prohíbe integración de Program/F2/endpoints/clientes/criptografía/cuotas distribuidas/paquetes nuevos/commit/push. Se permite código aislable, probes temporales eliminables y evidencia individual después de verificar. El responsable aprueba explícitamente la matriz mínima D012 durante esta ejecución; registro en §21. Los componentes quedan PREPARADOS / AISLABLES / NO INTEGRADOS AL HOST, sin cambiar el estado de 008 ni cerrar gates.

La autorización BUILD CONTROLADO posterior permite exclusivamente 013/014: crear los tres proyectos bajo `tests/`, actualizar sin regenerar `Enver.Backend.slnx`, añadir las seis referencias aprobadas únicamente donde corresponden, generar/verificar lockfiles, materializar pruebas F1 y auditar el grafo. Acepta expresamente xUnit 2.9.3 como Legacy por coherencia con el plan, sin sustituir su familia/major. No autoriza Program, host, endpoints, Docker/Testcontainers, T-AUTH-015, dependencias fuera de la lista, commit o push. Tras validación, la evidencia se registra en §22 y memoria §29.

La autorización BUILD CONTROLADO posterior delimita exclusivamente T-AUTH-015: Docker/Testcontainers con las imágenes y digests aprobados, fixture/factory y pruebas IT/ST de soporte; restore bloqueado, build y suites bajo los comandos indicados. Autoriza correcciones mínimas de fixture/factory/tests pertenecientes a 015, incluido presupuesto finito de inicialización máximo de 10 minutos, ambiente sintético efectivo y namespace Redis derivado del contrato F1. No autoriza `Program.cs`, producción, paquetes, imágenes/digests, spec/plan, T-AUTH-016, commit ni push. El registro de éxito queda en §23 y memoria §30.

La autorización BUILD CONTROLADO posterior delimita exclusivamente T-AUTH-016: `StackExchange.Redis` 3.4.31 como única referencia directa nueva, lockfiles derivados, conectividad Redis interna, namespaces/constructor de claves y pruebas UT/IT con el Redis real de la fixture existente. Aprueba `BacklogPolicy.FailFast`, `AbortOnConnectFail=false`, `ExponentialRetry` basado en `RedisOptions.ConnectionTimeoutMilliseconds` y el mapeo explícito de los dos timeouts existentes. No autoriza `Program.cs`, endpoints, revocación/TTL, cuotas/HMAC, MongoDB, JWT funcional, T-AUTH-017+, commit ni push. La evidencia queda en §26 y memoria §33.

La autorización BUILD CONTROLADO posterior delimita exclusivamente T-AUTH-018: puerto de admisión de estado de seguridad, guard Redis fail-closed y pruebas UT/IT. Reutiliza `RedisConnections`, `RequestBudget`/`DependencyBudget` y Redis Testcontainer existente; no agrega paquetes ni modifica lockfiles, `Program.cs`, endpoints, revocación/TTL, cuotas/HMAC, MongoDB, JWT funcional, detección real 019, recuperación/coordinación 020 o T-AUTH-017. La evidencia queda en §27 y memoria §34.

La autorización BUILD CONTROLADO posterior delimita exclusivamente T-AUTH-019: detector local de señales Redis reales, cierre conservador, medición local T0/T1/T2 y casos IT/ST. Reutiliza el multiplexer de `RedisConnections`, sin recrearlo, sin polling, paquetes, `Program.cs`, revocación/TTL, cuotas, JWT funcional, recuperación/reapertura coordinada 020 ni T-AUTH-017. La evidencia queda en §28 y memoria §35.

Una autorización posterior independiente ejecuta exclusivamente el subpaso local T-AUTH-020-A y registra evidencia parcial en §30 y memoria §37, sin cerrar T-AUTH-020 ni incorporar DynamoDB.

La autorización BUILD CONTROLADO posterior de T-AUTH-020-B permite exclusivamente el paquete DynamoDB aprobado, el adapter de autoridad, mutación potencial compartida, fixture DynamoDB Local, pruebas y evidencia de cierre en §31 y memoria §38. No autoriza `Program.cs`, endpoints, login/JWT, Terraform, IAM/AWS real, T-AUTH-021, stage, commit ni push.

La autorización BUILD CONTROLADO posterior de T-AUTH-052 aprueba su decisión técnica previa y permite únicamente modelos/descriptores, puerto abstracto, validaciones, contrato de transición/atomicidad/script, cálculo puro de RetryAfter y UT. Tras detenerse por la contradicción entre prohibición de Docker y regresión completa, el responsable autoriza expresamente Docker/Testcontainers **solo para ejecutar IT/ST ya existentes**, sin modificar fixtures ni crear IT/ST de 052. El cierre requiere restore bloqueado, build y las tres suites verdes, sin cambios de lockfiles. Evidencia en §32 y memoria §39; sin store Redis, Lua ejecutable, HMAC/origen, cardinalidad, coordinación runtime, Program, paquetes nuevos, ejecución de 021/053/054, stage, commit o push.

La autorización BUILD CONTROLADO posterior de T-AUTH-053 aprueba su contrato pre-BUILD y permite exclusivamente canonicalización de representación, HMAC-SHA-256, composición inequívoca, puertos/modelos, proveedor aislable de origen, endurecimiento de la clave HMAC, contrato de rotación y UT/ST focales. Se inspecciona antes la activación por colección de las fixtures y se ejecuta solo la nueva clase ST mediante filtro, sin Docker/Testcontainers, IntegrationTests ni fixture compartida. No autoriza Program/DI productivo, forwarding real, normalizador institucional completo de 028, JWT, Redis/Lua, 021/054, AWS/Terraform, paquetes nuevos, stage, commit ni push. 008 conserva su verificación productiva pendiente. Evidencia en §33 y memoria §40 tras restore bloqueado/build/UT/ST focales verdes y lockfiles intactos.

El responsable aprueba posteriormente el dimensionamiento inicial de 054: **MaxActivePartitions=5000 globales por ambiente/finalidad y presupuesto de diseño 64 MiB=67108864 bytes para cuotas**, sin derivarlo de la fixture ni acreditar memoria real/convivencia con revocaciones. Aprueba expresamente distribuir la evidencia: BUILD contractual 054 con UT, sin Redis/Lua/Docker; IT reales obligatorias en 021/055. La autorización BUILD CONTRACTUAL CONTROLADO ejecuta solo ese incremento; tras verde, registra §34 y memoria §41 y satisface el prerrequisito de diseño para 021, sin iniciarla ni cerrar GATE-B. No permite Program/DI productivo, AWS/Terraform, paquetes, stage/commit/push. 054 no queda completada integralmente: **EVIDENCIA IT DISTRIBUIDA PENDIENTE EN T-AUTH-021/T-AUTH-055 PARA CIERRE DE GATE-B**. La memoria empírica insuficiente exige reducir capacidad o aprobar mayor presupuesto antes de despliegue, nunca degradar revocaciones.

**APROBADO PARA EJECUCIÓN INCREMENTAL** significa exclusivamente que el catálogo podrá ejecutarse una tarea o incremento a la vez mediante **autorización posterior** de alcance, archivos, acciones y entorno. No autoriza ejecución inmediata, instalaciones, conexiones, builds, pruebas, scanners, workflows, AWS, Terraform, commit o push; no satisface gates ni resuelve decisiones diferidas.

La autorización posterior de implementación y correcciones focalizadas de 021 produjo la validación técnica verde registrada en §35. El responsable autoriza después **BUILD DOCUMENTAL CONTROLADO** exclusivamente para cerrar E(T-AUTH-021), actualizar la evidencia diferida de 054 y añadir memoria §42, modificando solo este documento y `memoria.md`. No se repiten pruebas ni se modifican implementación/tests, contratos 052/053/054, paquetes, `Program.cs`, DI productivo, AWS/Terraform o gates; sin stage/commit/push. Los estados anteriores se conservan como historial y el estado posterior de 021 rige este cierre.

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

**E(T-AUTH-021)** registra el store Redis/Lua real, evidencia distribuida, capacidad, memoria, recovery y límites en [§35](#e-t-auth-021); actualiza únicamente las IT esenciales de 054 asignadas a 021 en §34.1. Las evidencias históricas siguientes conservan el alcance de sus respectivas entregas.

**E(T-AUTH-022)** registra la verificación operativa Redis de dos hosts lógicos de test, revocación A→B, TTL sin reducción, `noeviction`, memoria/headroom, separación ambiente/propósito, cierre sin bypass y privacidad diagnóstica en [§37](#e-t-auth-022). No crea componente productivo, no ejecuta endpoints/JWT/logout y no cierra GATE-B ni AUTH-027.

**E(T-AUTH-023)** registra BSON/UUID/status, MongoClient aislable reutilizable, topología local rs0, concerns/ACK/journaling, aislamiento y redacción de diagnósticos del harness en [§39](#e-t-auth-023). No implementa store, índices, CAS ni composición productiva; no cierra GATE-C.

**E(T-AUTH-024)** registra el UserStore, resultados seguros, duplicados específicos, ACK perdido y precondición del índice fuera del presupuesto del store en [§40](#e-t-auth-024). **E(T-AUTH-025)** registra creación/verificación del índice, idempotencia, concurrencia, incompatibilidad, duplicados y preservación en [§41](#e-t-auth-025). Ambos cierres son locales y aislables; no integran autenticación productiva ni cierran GATE-C. Las limitaciones e incidencias históricas se conservan explícitamente.

**E(T-AUTH-026)** registra CAS de tres condiciones/dos campos, resultados, presupuestos, concurrencia y ACK suprimido en [§42](#e-t-auth-026). **E(T-AUTH-027)** registra integración MongoDB real 023–026, dos clientes, unicidad/CAS, lectura, incertidumbre y aislamiento en [§43](#e-t-auth-027). La revisión posterior de E023–E027 cierra formalmente **GATE-C local** en [§44](#cierre-gate-c), sin modificar los estados históricos de cada entrega ni iniciar Register/composición.

**BUILD-1** registra la autorización y regresión final en [§45](#build-1-validacion-final), y sus evidencias individuales **E(T-AUTH-056), E(T-AUTH-029), E(T-AUTH-028), E(T-AUTH-030), E(T-AUTH-031), E(T-AUTH-033) y E(T-AUTH-057)** en §§46–52. [§53](#build-1-aceptacion) acepta solo ese incremento, sin cerrar GATE-D ni iniciar 034–037. Los estados y límites de entregas anteriores se conservan como antecedentes.

**BUILD-2** registra su validación autorizada e historial en [§54](#build-2-validacion-final), **E(T-AUTH-034), E(T-AUTH-035), E(T-AUTH-036) y E(T-AUTH-037)** separadas en §§55–58 y la revisión formal de **GATE-D LOCAL/TEST REPRESENTATIVO** en [§59](#cierre-gate-d). Este estado posterior no reescribe las entregas históricas ni inicia JWT/Login/Logout u otro gate.

**E-JWT** registra autorización, historial del cleanup y regresión completa en [§60](#e-jwt-validacion-final), **E(T-AUTH-038) a E(T-AUTH-042)** separadas en §§61–65 y cierre exclusivamente del checkpoint local/test en [§66](#cierre-e-jwt). No cierra GATE-E completo ni GATE-F, no publica Login/Logout y no activa JWT en el pipeline productivo de 058.

**Logout / GATE-F**, autorizado posteriormente, registra validación e incidentes en [§67](#logout-validacion-final), correctivo de GATE-B en [§68](#gate-b-correctivo-logical-clock), **E(T-AUTH-043) a E(T-AUTH-046)** en §§69–72 y revisión formal en [§73](#cierre-gate-f). Solo ese acto posterior cierra F local/test; GATE-E completo, Login/032/058 y aceptación productiva siguen pendientes.

Cada ficha reserva su identificador **E(T-AUTH-nnn)**. **E(T-AUTH-001)** conserva la inspección de solo lectura y su cierre documental autorizado (§10); **E(T-AUTH-002)** registra la evaluación previa de SDK/dependencias/riesgos y su cierre documental autorizado (§11); **E(T-AUTH-003)** registra la baseline técnica y sus verificaciones reales (§12); **E(T-AUTH-004)** registra la inspección acotada de higiene Git y su cierre documental autorizado (§13); **E(T-AUTH-005)** registra las correcciones de contexto/publish y verificaciones locales sin Docker ni publish real (§14); **E(T-AUTH-006)** registra estructura, ownership y condiciones de confirmación del checkpoint Git único (§15). **E(T-AUTH-007)** registra configuración declarada y verificaciones controladas (§16); **E(T-AUTH-008)** registra implementación preparada con startup real pendiente (§17). **E(T-AUTH-009)** registra presupuestos/cancelación (§18); **E(T-AUTH-010)** errores/correlación/no-store (§19); **E(T-AUTH-011)** frontera HTTP (§20); **E(T-AUTH-012)** decisión aprobada y coordinación aislable (§21); **E(T-AUTH-013/014)** registra proyectos, dependencias, lockfiles, pruebas y auditoría (§22); **E(T-AUTH-015)** registra Docker/Testcontainers, hosts, aislamiento, cleanup y startup real de Options (§23); **E(T-AUTH-016)** registra cliente Redis, key builder, grafo, pruebas y límites (§26); **E(T-AUTH-018)** registra admisión fail-closed, presupuestos, incertidumbre y pruebas Redis (§27); **E(T-AUTH-019)** registra señales reales observables, detector y cierre local (§28); **E(T-AUTH-017)** registra el store de revocación, script Lua, TTL y pruebas Redis reales (§29); **E(T-AUTH-020)** registra la autoridad DynamoDB Local, CAS, emisión potencial y la integración Redis/DynamoDB de dos coordinadores (§31); **E(T-AUTH-052)** registra el contrato aislado de cuotas, PolicyRevision, RetryAfter y UT (§32); **E(T-AUTH-053)** registra pseudonimización/origen aislables, configuración, drenaje y UT/ST focales sin Docker (§33); **E(T-AUTH-054)** registra exclusivamente el incremento contractual de capacidad/recuperación y UT, con evidencia IT distribuida pendiente en 021/055 (§34). **E(T-AUTH-055)** registra la evidencia distribuida ampliada de cuotas, capacidad, limpieza, fallos, OOM y seguridad de origen (§36) y la ampliación de 054 asignada a 055 (§34.2). Los resultados de ejecución de T-AUTH-022 a T-AUTH-051 y T-AUTH-056 a T-AUTH-079 siguen **PENDIENTES / SIN EVIDENCIA TÉCNICA GENERADA**. La definición de una prueba o salida esperada no constituye resultado obtenido; los cierres registrados no acreditan autenticación integrada ni aceptación global de seguridad. El cierre de 006 solo es efectivo si el commit y la verificación Git posterior de §15 son correctos.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-001)](#e-t-auth-001): revisión de autorización y estado Git de solo lectura realizada; evidencia documental registrada en §10, sin pruebas del backend.
- **Riesgos/notas:** No stash/reset, descarte, commit o push implícitos. Esta tarea tampoco se ejecuta por crear el catálogo.
- **Estado:** COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-002)](#e-t-auth-002): inventario/auditoría expresamente autorizados realizados; herramientas, fecha, resultados, tratamiento y límites registrados en §11, sin build ni pruebas del backend.
- **Riesgos/notas:** Restauraciones o accesos de red implícitos requieren alcance autorizado; dependencias de plantilla no son selecciones definitivas.
- **Estado:** COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-003)](#e-t-auth-003): SDK/grafo fijados, lockfile generado, restore normal/bloqueado, build y auditoría NuGet realizados (§12); cada capacidad futura debe actualizar/verificar sus propios lockfiles bajo autorización.
- **Riesgos/notas:** Sin referencias redundantes, versiones transitivas forzadas sin justificación ni incorporación tácita de herramientas candidatas.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-004)](#e-t-auth-004): inspección de solo lectura y 44 comprobaciones de exclusión/versionabilidad realizadas; resultados, alcance y límites registrados en §13, sin auditoría exhaustiva del historial ni scanner adicional en este cierre.
- **Riesgos/notas:** Ignorar no retira secretos del historial; una exposición requiere tratamiento/rotación. No certificar historial o binarios no revisados.
- **Estado:** COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-005)](#e-t-auth-005): inspección estática de exclusiones/Dockerfile, evaluación MSBuild con 42 archivos sintéticos vacíos, restore bloqueado y dos builds locales correctos; alcance y límites en §14. Sin Docker ni publish real; verificación efectiva de publicación con sentinelas y revisión final en 078 pendientes de autorización posterior.
- **Riesgos/notas:** `.gitignore` no protege Docker ni publish. No diferir esta protección hasta después de construir imágenes.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-006)](#e-t-auth-006): revisión de estructura/baseline registrada y ownership por rol en §15; checkpoint único autorizado, preparado con la evidencia antes de commit y confirmado únicamente tras verificar working tree limpio. Sin regenerar solución/artefactos existentes.
- **Riesgos/notas:** Limpieza no se obtiene descartando trabajo. No exige un commit por tarea ni incorpora patrones/proyectos excluidos.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA Y CHECKPOINT GIT REGISTRADOS.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-007)](#e-t-auth-007): casos de valores ausentes/incoherentes y aislamiento; materialización con F2/008 y regresión 060.
- **Riesgos/notas:** Declarar Options no demuestra disponibilidad, integridad ni aprobación de topología/ACK; se preservan puertas de §7.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-008)](#e-t-auth-008): configuración inválida impide inicio/admisión; User Secrets/variables sintéticas sin volcado de valores; pruebas Options en UT y 060.
- **Riesgos/notas:** Longitud Base64 no acredita entropía; no recargar claves/skew/cuotas descoordinadamente ni inventar topes pendientes.
- **Estado:** IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-009)](#e-t-auth-009): casos de agotamiento, cancelación del cliente, operación incierta y ausencia de reintento/espera ilimitados.
- **Riesgos/notas:** Timeout no prueba ausencia de efecto; cancelar no interrumpe mágicamente PasswordHasher síncrono. Calibración AD-09 en 067.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-010)](#e-t-auth-010): 400/401/409/413/415/429/500/503, cancelación y respuesta iniciada; 204 vacío y sin segunda escritura.
- **Riesgos/notas:** Handler de excepciones no cubre por sí solo MVC/challenge/cuotas; no exponer infraestructura, stack traces, secretos ni existencia/estado.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-011)](#e-t-auth-011): fronteras, charset/encoding, duplicados y cuerpos; JWT excesivo 401 API, distinto del posible rechazo de headers del borde.
- **Riesgos/notas:** Si la API nativa de duplicados no sirve, comprobación equivalente acotada, nunca aceptar duplicados; no confiar solo en Content-Length.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-012)](#e-t-auth-012): casos con colaboradores controlados; 400 automático MVC no omite un control asíncrono requerido; integración final en 058.
- **Riesgos/notas:** No requiere el normalizador funcional posterior para definir el contrato; no cambiar estados ni cuotas aprobados.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§22).

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§22).

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§23).

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§26).

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§29).

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§27).

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§28).

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§31): 020-A conserva evidencia local y 020-B acredita DynamoDB Local. AWS real, IAM, Terraform, continuidad/failover y riesgo silencioso residual permanecen fuera de esta evidencia; GATE-B y AUTH-027 no quedan satisfechos.

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
- **Pruebas/evidencia requerida:** E(T-AUTH-021): script versionado valida tipos/argumentos antes de mutar, concurrencia y timeout incierto; sin reintento ciego. Asume además las IT de 054 asignadas en §34: atomicidad conjunta del registro/capacidad/cuotas, límite global 5000, creación concurrente sin sobrepasarlo, TTL/liberación, reconciliación/pérdida/recuperación sin reset y medición física de memoria/convivencia básica con revocaciones. No cerrar 021 trasladando fallos u obligaciones esenciales sin acreditar a 055.
- **Riesgos/notas:** F11 se adelanta por dependencia; no asumir atomicidad cross-slot, crear claves ilimitadas o usar claims no autenticados para logout.
- **Estado:** COMPLETADA / EVIDENCIA REDIS DISTRIBUIDA REGISTRADA ([E(T-AUTH-021), §35](#e-t-auth-021)). IT esenciales de 054 asignadas a 021 satisfechas en el alcance registrado (§34.1); ampliación 055 pendiente, 022 no iniciada. GATE-B PENDIENTE / NO SATISFECHO; sin autorización automática de otra tarea.

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
- **Estado:** COMPLETADA / EVIDENCIA OPERATIVA REDIS REGISTRADA ([E(T-AUTH-022), §37](#e-t-auth-022)). Acredita dos hosts lógicos de test con Redis real compartido, estado local independiente, revocación A→B, TTL sin reducción, ausencia de caché negativa, `noeviction`, memoria/headroom, separación ambiente/propósito, cierre compartido sin bypass y privacidad diagnóstica. No acredita TLS/ACL productivos, hosts físicos, AWS, LB, dimensionamiento productivo ni cierre de GATE-B; AUTH-027 conserva NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.

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
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA ([E(T-AUTH-023), §39](#e-t-auth-023)). Alcance de componente y aceptación local, sin integración productiva; GATE-C permanece PENDIENTE / NO SATISFECHO.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-024), §40](#e-t-auth-024)); componente aislable, sin composición productiva ni cierre de GATE-C.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-025), §41](#e-t-auth-025)); inicialización aislable, sin composición productiva ni cierre de GATE-C.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-026), §42](#e-t-auth-026)); CAS aislable, sin rehash/login funcional ni composición productiva.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-027), §43](#e-t-auth-027)); integración local de componentes 023–026, no Atlas/HA/failover productivo.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-028), §48](#e-t-auth-028)); normalizador/validador verificados, sin ejecutar Register/Login ni integrar consumo funcional de cuotas.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-029), §47](#e-t-auth-029)); política y medición estricta verificadas, sin endpoint de registro o acceso.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-030), §49](#e-t-auth-030)); fronteras y entradas maliciosas en validadores puros, sin acreditar el conjunto HTTP funcional futuro de 065.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-031), §50](#e-t-auth-031)); PasswordHasher<User>, verificación y dummy singleton preinicializado; sin persistencia de rehash/CAS, Login o emisión.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-033), §51](#e-t-auth-033)); límite configurable, saturación y ownership del trabajo real verificados; calibración productiva de 067 pendiente.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-034), §55](#e-t-auth-034)); contratos y flujo D012 verificados en el alcance local/test de BUILD-2.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-035), §56](#e-t-auth-035)); endpoint/persistencia y errores verificados localmente, sin acreditar publicación externa.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-036), §57](#e-t-auth-036)); cuotas conjuntas y observabilidad allowlist verificadas, sin dimensionamiento productivo.

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
- **Pruebas/evidencia requerida:** E(T-AUTH-037): suites unit/integration/security del incremento con Mongo/Redis reales y composición 056/057; resultados registrados en §§54/58.
- **Riesgos/notas:** Fase numérica no impide adelantar composición; factory no acredita pipeline final ni infraestructura publicada.
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-037), §58](#e-t-auth-037)); Register OPERATIVO LOCAL/TEST y revisión formal del gate en §59.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-038)](#e-t-auth-038), §61: claims mínimos/jti distinto, claves inválidas y frontera temporal explícita antes/en/después de exp+skew coordinada con retención; validación común en §60.
- **Riesgos/notas:** Sin PII/roles/refresh ni emisión pública sin controles; precisión/margen requieren revisión, no defaults accidentales ni longitud Base64 como entropía.
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-039)](#e-t-auth-039), §62: firma/algoritmos, claims ambiguos, tiempos y límites; ningún jku/x5u remoto, fallback inseguro o uso confiable de sub/jti antes de validar.
- **Riesgos/notas:** Parseo temprano acotado no autentica; tolerancia temporal no alarga exp emitido ni deja tokens aceptados sin retención.
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-040)](#e-t-auth-040), §63: denegación por defecto, excepción no reutilizable, revocado →401 fuera de logout y Redis incierto →503/no-store; historial del cleanup en §60.2.
- **Riesgos/notas:** JWT válido no concede permisos de negocio; futuros recursos requieren su spec/política y no se crean para probar esta capacidad.
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-041)](#e-t-auth-041), §64: claves sintéticas, coexistencia/retiro, configuración incompatible y retención suficiente; procedimiento manual aprobado y reloj controlado.
- **Riesgos/notas:** Sin automatización/periodicidad o gestor AWS seleccionado; todo poseedor HS256 puede emitir. No nuevo endpoint de cierre global.
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA.

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
- **Pruebas/evidencia requerida:** [E(T-AUTH-042)](#e-t-auth-042), §65: unit/integration/security de JWT con Redis real cuando aplique; sin identidad falsa derivada de claims no autenticados; checkpoint revisado en §66.
- **Riesgos/notas:** No depende de login público para producir fixtures JWT; no anticipa aceptación de endpoints ni detección universal Redis.
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA; [E(T-AUTH-043), §69](#e-t-auth-043), alcance local/test.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA; [E(T-AUTH-044), §70](#e-t-auth-044), alcance local/test.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA; [E(T-AUTH-045), §71](#e-t-auth-045), alcance local/test.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA; [E(T-AUTH-046), §72](#e-t-auth-046), alcance local/test.

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
- **Estado:** COMPLETADA / EVIDENCIA DE CONTRATO REGISTRADA (§32).

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
- **Estado:** COMPLETADA / EVIDENCIA AISLABLE REGISTRADA (§33); forwarding y rotación runtime no implementados.

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
- **Pruebas/evidencia requerida:** E(T-AUTH-054): identidades aleatorias, capacidad, pérdida/recuperación y ausencia de reset; garantías distintas de recuperación de revocaciones. Distribución posterior expresamente aprobada: modelos/reglas puras y UT en incremento contractual 054; implementación/evidencia Redis real obligatoria en 021/055 (§34), sin eliminar UT/IT ni introducir circularidad. Topes aprobados: 5000 particiones y 64 MiB=67108864 bytes como presupuesto inicial de diseño, no medición ni garantía empírica.
- **Riesgos/notas:** TTL solo no acota cardinalidad; noeviction no da memoria ilimitada; sin degradar seguridad por presión de caché.
- **Estado:** COMPLETADA EN SU INCREMENTO CONTRACTUAL / PRERREQUISITO DE DISEÑO SATISFECHO (§34). **EVIDENCIA IT ESENCIAL ASIGNADA A T-AUTH-021: SATISFECHA EN EL ALCANCE REGISTRADO** (§34.1 y E(T-AUTH-021), §35), incluidos Redis real, memoria medida, convivencia básica y recuperación ensayada. **AMPLIACIÓN DE EVIDENCIA ASIGNADA A T-AUTH-055: SATISFECHA EN EL ALCANCE REGISTRADO** (§34.2 y E(T-AUTH-055), §36): presión sostenida, siete cuotas extendidas, capacidad 5000, limpieza, OOM/timeout, restart/fence/recovery y convivencia ampliada con revocaciones. No declara satisfacción integral de 054/GATE-B ni garantías productivas.

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
- **Obligación IT de 054:** Ampliar/regresar evidencia de 021 entre hosts y las siete cuotas bajo carga extendida, presión sostenida, perfiles prolongados, límite global, limpieza y mediciones representativas de memoria/convivencia con revocaciones bajo presión ampliada (§34.1); sin presentar UT contractuales como prueba distribuida ni liberar GATE-B con evidencias pendientes. El cierre de 021 no satisface 055.
- **Riesgos/notas:** No sanción/Disabled por cuota; aislar recursos de pruebas de pérdida y concurrencia. No acredita red/LB productivos.
- **Estado:** COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA (§36 y E(T-AUTH-055)). Acredita presión distribuida, capacidad, limpieza, fallos, OOM, restart/fence/recovery y seguridad de origen en el entorno de test descrito; no declara red/LB productivos, forwarding productivo, hosts físicos independientes, AWS validado, benchmark empresarial ni cierre de GATE-B. **022 NO INICIADA; GATE-B PENDIENTE / NO SATISFECHO; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.**

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-056), §46](#e-t-auth-056)); composición y transporte verificados localmente en host/test, sin acreditar TLS publicado o infraestructura cloud.

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
- **Estado:** COMPLETADA / EVIDENCIA REGISTRADA ([E(T-AUTH-057), §52](#e-t-auth-057)); DI/admisión interna y dependencias reales locales verificadas, sin endpoints funcionales o bootstrap productivo implícito.

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

El catálogo se creó con todos los gates **PENDIENTE / NO SATISFECHO**. Tras los cierres formales de §25, §38, §44, §59 y §73, el estado vigente es: **GATE-A/B/C/D/F SATISFECHOS / CERRADOS en sus alcances registrados**; **GATE-E/G/H PENDIENTES / NO SATISFECHOS**. GATE-D y GATE-F corresponden exclusivamente a Register y Logout LOCAL/TEST REPRESENTATIVO. Los gates revisan capacidades; no reemplazan autorización por tarea/incremento. No se impone terminar el gate completo para construir sus propias tareas integrantes: sus entradas y dependencias indican qué parte puede iniciarse.

El cierre de [§66](#cierre-e-jwt) satisfizo únicamente **checkpoint E-JWT, LOCAL/TEST REPRESENTATIVO**, sin cerrar entonces E completo ni F. El acto posterior de [§73](#cierre-gate-f) cierra F tras revisar sus propias evidencias; **GATE-E completo y G/H siguen pendientes**.

**Un gate superado localmente no demuestra AWS/Atlas/infraestructura productiva.** Cada aceptación identifica entorno/topología y alcance. Un checkpoint local puede permitir trabajo posterior independiente autorizado, pero no cerrar la aceptación externa pendiente de 068, simularla o afirmar cumplimiento integral. El cierre 079 depende de 068; mientras falte, permanece pendiente aunque existan checkpoints locales.

### GATE-A

- **Nombre:** Base/configuración.
- **Estado vigente:** **SATISFECHO / CERRADO**; registro formal y límites en §25.
- **Tareas de entrada:** 001–006; componentes 007–015.
- **Condiciones de entrada:** Solicitud técnica posterior delimitada, fuentes/baseline identificadas, conservación de cambios existentes y entornos aprobados para las acciones de preparación.
- **Condiciones de salida:** 001–015 revisadas/verificadas: SDK/grafo/versiones, higiene/contexto/publish, Options inválidas rechazadas, presupuestos/errores/HTTP probados y tres suites/fixtures disponibles. Resultados reales, no mera presencia de clases.
- **Bloqueos conocidos:** Git pendiente de consolidación por responsable, versiones/licencias/vulnerabilidades y Docker/runner no aprobados; no ejecutar un build sensible sin 005. Las fuentes de configuración se pueden comprobar con valores sintéticos sin servicio cloud.
- **Evidencia necesaria:** E(T-AUTH-001) a E(T-AUTH-015), revisión de alcance/configuración y resultados de pruebas de componentes. 056 integra después el tramo base y no es entrada de A: depende de 053, evitando ciclo A→B→A.

### GATE-B

- **Nombre:** Redis.
- **Estado vigente:** **CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA**, checkpoint local de componentes Redis para el entorno declarado de test; cierre original y límites intactos en §38, correctivo logical clock en [§68](#gate-b-correctivo-logical-clock). No se reabre ni rehace el gate.
- **Tareas de entrada:** GATE-A; 016 y condiciones previas de 019/052–054; componentes 017–022 y 055.
- **Condiciones de entrada:** Versiones/configuración/entorno Redis aprobados; modelo de fallos, ACK/lecturas y señales antes de 017/019; precedencia/cardinalidad/HMAC/recuperación antes de cuotas.
- **Condiciones de salida:** 016–022 y 052–055 con evidencia: revocación/retención no decreciente, noeviction/capacidad, confirmación/lecturas, cuotas conjuntas y fail-closed distribuidos; detección/cierre/recuperación verificados para entorno declarado. Casos JWT/endpoints finales se complementan en F/G, no se anticipan aquí.
- **Bloqueos conocidos:** Fuentes de detección/coordinación/ACK/topología, topes y protocolos no resueltos; prueba local no resuelve operación del servicio administrado. Sin prueba de reapertura segura mantener cierre.
- **Evidencia necesaria:** E(T-AUTH-016) a E(T-AUTH-022), E(T-AUTH-052) a E(T-AUTH-055); dos hosts/dependencias reales, propagación y límites. AUTH-027 sigue NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE; este cierre no lo certifica por checkpoint de componentes.

### GATE-C

- **Nombre:** MongoDB.
- **Estado vigente:** **SATISFECHO / CERRADO como checkpoint local de componentes MongoDB**; revisión formal de E023–E027, entorno y límites en [§44](#cierre-gate-c). No acredita composición productiva/Atlas/HA/failover cloud.
- **Tareas de entrada:** GATE-B, 023; componentes 024–027.
- **Condiciones de entrada:** Driver/grafo y topología/journaling/ACK/concerns aprobados; fixture representativa autorizada.
- **Condiciones de salida:** 023–027 verificadas: siete campos/BSON estándar, lectura tras creación confirmada, índice exacto/inicializador, unicidad concurrente, CAS y resultados inciertos seguros.
- **Bloqueos conocidos:** Standalone insuficiente para majority/failover; datos/índices incompatibles no se reparan automáticamente; Atlas externo sigue pendiente donde aplique.
- **Evidencia necesaria:** E(T-AUTH-023) a E(T-AUTH-027), inspección real de BSON/índice y pruebas de carrera/ACK/CAS con versión/topología.

### GATE-D

- **Nombre:** Register.
- **Estado vigente:** **SATISFECHO / CERRADO, LOCAL/TEST REPRESENTATIVO**; revisión formal de E028–E031/E033–E037/E056/E057 y límites en [§59](#cierre-gate-d). No acredita despliegue ni titularidad del buzón institucional.
- **Tareas de entrada:** GATE-B/C; 028–031/033 y composición 056–057; componentes 034–037. 032 se verifica para rehash antes de Login, no es condición funcional de crear una cuenta nueva.
- **Condiciones de entrada:** Normalización/password/coste y controles compartidos disponibles; factories y adaptadores reales integrables sin publicar rutas incompletas.
- **Condiciones de salida:** 034–037 verificadas: 201 confirmado, conflicto 409 genérico determinado por índice, sin login/token automático; validación/cuotas antes del coste, no-store/logging seguro y pruebas positivas/negativas/concurrentes.
- **Bloqueos conocidos:** Índice/confianza/cuotas no aptos impiden aceptación; 201/409 conserva inferencia residual y dominio no demuestra titularidad.
- **Evidencia necesaria:** E(T-AUTH-028) a E(T-AUTH-031), E(T-AUTH-033) a E(T-AUTH-037), E(T-AUTH-056) y E(T-AUTH-057); casos AC-01 a AC-05, 16/17/19 según ficha.

### GATE-E

- **Nombre:** JWT/Login.
- **Estado vigente:** **PENDIENTE / NO SATISFECHO como gate completo**. Checkpoint **E-JWT SATISFECHO / CERRADO, LOCAL/TEST REPRESENTATIVO**, revisión en [§66](#cierre-e-jwt); Login/032/058 pendientes y posteriores a GATE-F.
- **Tareas de entrada:** GATE-D; 038–042 para checkpoint JWT. Después de GATE-F, 032 y 047–051; integración final 058.
- **Condiciones de entrada:** Claves/configuración/temporalidad/retención aprobadas y controles Redis confiables; CAS/dummy/coste disponibles antes de Login. Emisión de fixtures de test no requiere login público.
- **Condiciones de salida:** Checkpoint **E-JWT**: 038–042 probadas habilitan F. Cierre **E completo**: tras F, 047–051 y 058 comprobadas, con dummy previo, Disabled comparable, CAS seguro, 401 uniforme, cuotas y JWT solo con controles válidos.
- **Bloqueos conocidos:** Precisión/margen/rotación, estado no confiable, dummy no preparado, rehash incierto o cuota no verificable bloquean aceptación/emisión afectada. No cerrar E completo antes de Logout.
- **Evidencia necesaria:** E(T-AUTH-032), E(T-AUTH-038) a E(T-AUTH-042), E(T-AUTH-047) a E(T-AUTH-051), E(T-AUTH-058); fronteras temporales, claims/algoritmos/políticas y login real. E038–E042 registradas en §§61–65; E032, E047–E051 y E058 siguen pendientes. El estado inicial sin evidencia se conserva en el historial de creación.

### GATE-F

- **Nombre:** Logout.
- **Estado vigente:** **SATISFECHO / CERRADO — LOCAL/TEST REPRESENTATIVO**; revisión formal de E043–E046 en [§73](#cierre-gate-f), tras validación de §67 y correctivo de §68. No acredita 058 integral ni despliegue productivo.
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
| Cardinalidad | Entrada contractual 054 aprobada: 5000 particiones globales y presupuesto inicial de cuotas 64 MiB=67108864 bytes (§34). TTL solo no basta, capacidad insuficiente →503. Validación empírica de memoria/convivencia y comportamiento distribuido obligatoria en 021/055 antes de aceptación correspondiente/despliegue; sin debilitar revocaciones |
| Transición HMAC/políticas | Protocolo aprobado en 053 antes de construir/aceptar control y antes de cualquier rotación/cambio real; no resetear ventanas |
| Recuperación de contadores | Política contractual 054 aprobada/UT verdes (§34): reconstrucción confiable o cierre durante ventana restante, distinta de revocaciones. 021 desbloqueada respecto del diseño 054; demostración real obligatoria en 021/055, sin reset ni reapertura por ping/restart |
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

La validación de la entrega de creación SOL-TASK-002 se limitó a estructura documental, IDs/campos/estados, referencias, matrices, dependencias/gates, coherencia con fuentes y formato Git. No fue ejecución de E(T-AUTH-nnn), build, prueba del backend, scanner, conexión o pipeline. Tras registrar el resultado documental y sincronizar AGENTS/memoria/README se detuvo aquel trabajo. La autorización posterior de §10 cierra únicamente T-AUTH-001; cualquier tarea siguiente necesita delimitar su propia intervención, sin ejecución automática.

La solicitud de cierre posterior de §11 registra únicamente la evaluación ya realizada de T-AUTH-002. T-AUTH-001 conserva su cierre; 003–079 y los ocho gates continúan pendientes. La deuda de parches identificada se traslada a la selección autorizada posterior, sin instalar, restaurar ni fijar versiones en este cierre.

El acto técnico posterior de §12 cierra exclusivamente T-AUTH-003 tras sus verificaciones autorizadas. 004–079 y los ocho gates siguen pendientes; no se transforma la evidencia de baseline en aceptación integral de AUTH/AC, foundation o infraestructura.

El cierre documental posterior de §13 registra exclusivamente la inspección ya realizada de T-AUTH-004. 001–003 conservan sus estados y evidencias; 005–079 y los ocho gates siguen pendientes. R-008 se reduce, sin resolverse ni borrar su historial; no se inicia la protección Docker/publish de T-AUTH-005 automáticamente.

La ejecución posterior de §14 corrige y valida localmente solo T-AUTH-005, sin Docker ni publish real. 001–004 conservan sus estados y evidencias; 006–079 y los ocho gates siguen pendientes. R-006 queda REDUCIDO y R-007 REDUCIDO — NO RESUELTO; no se inicia T-AUTH-006 ni se transforma la evaluación de elementos MSBuild en una publicación validada.

El checkpoint posterior de §15 consolida ownership y versiona en un solo commit autorizado los cambios acumulados válidos 001–006. Sus estados históricos/evidencias no se reescriben; la confirmación de 006 exige commit correcto y estado Git posterior limpio. Eso completa únicamente F0 — Baseline de 002, no GATE-A: 007–079 y los ocho gates permanecen pendientes, sin inicio automático de F1.

## 10. Cierre documental autorizado de T-AUTH-001

<a id="e-t-auth-001"></a>

### E(T-AUTH-001)

#### Autorización, resultado y trazabilidad

El responsable autorizó revisar únicamente T-AUTH-001 en modo PLAN mediante inspección de solo lectura. El dictamen fue **LISTA PARA CIERRE DOCUMENTAL**. Una solicitud posterior, en modo BUILD DOCUMENTAL, autoriza registrar esa evidencia y cerrar únicamente esta tarea en este archivo, con un registro breve al final de memoria. No se repite la inspección técnica ni se generan resultados nuevos de SDK, dependencias o servicios por registrar el cierre.

| Campo | Evidencia registrada |
| --- | --- |
| Tarea | T-AUTH-001 — Delimitar ejecución y baseline documental/Git |
| Estado de cierre | **COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA** |
| AUTH relacionados | AUTH-001, AUTH-047, AUTH-048; no se declaran satisfechos globalmente por este cierre |
| PLD-AUTH relacionados | PLD-AUTH-01, PLD-AUTH-25 |
| Gate | GATE-A — **PENDIENTE**; no se cierra por completar 001 |
| AC | Sin escenario funcional directo |
| Resultado esperado | Identificar baseline SDD/Git, alcance, entorno, operaciones, protecciones, dependencias y riesgos sin modificar archivos durante la inspección |
| Resultado obtenido | Inspección y delimitación realizadas en solo lectura; baseline identificada y riesgos trasladados, sin implementación |
| Archivos modificados durante la inspección | Ninguno; este cierre posterior modifica únicamente task.md y añade el registro histórico en memoria.md |
| Pruebas ejecutadas | Ninguna; los comandos de consulta no constituyen pruebas del backend |
| Commit del cierre / workflow run / captura académica | No generados; el commit de baseline siguiente es previo, no un commit de esta intervención |

#### Baseline Git y entorno observados en la inspección anterior

| Elemento | Dato observado |
| --- | --- |
| Rama | `feature/authentication` |
| HEAD | `d1d29298f86e25070f7bd1835acecd25d88d3e35` |
| Commit de baseline | `docs(auth): finalize authentication planning and tasks` |
| Working tree | Limpio al inicio y al final de aquella inspección; no describe el árbol tras escribir este cierre |
| Seguimiento observado | HEAD coincidía con la referencia **local** `origin/feature/authentication`; no se consultó nuevamente el remoto |
| Repositorio configurado | `https://github.com/enverdelivery/ENVER-Backend.git` |
| Sistema | Windows x64 |
| Git | `2.55.0.windows.3` |
| SDK seleccionado | .NET `10.0.300` |
| SDK instalados | `9.0.205` y `10.0.300` |
| ASP.NET Core Runtime | `10.0.8` |
| TargetFramework | `net10.0` |
| Spec de baseline | spec.md v0.3.0 — **APROBADO PARA PLANIFICACIÓN** |
| Plan de baseline | plan.md v0.2.0 — **APROBADO PARA DESCOMPOSICIÓN EN TAREAS** |
| Task de baseline | task.md v0.1.0 — **APROBADO PARA EJECUCIÓN INCREMENTAL** |
| Catálogo / gates | 79 tareas; 8 gates pendientes. Tras este cierre: solo 001 completada documentalmente, 002–079 pendientes |

Estos datos proceden únicamente de las salidas y lecturas ya obtenidas en la inspección autorizada, ratificadas en la solicitud de cierre. No certifican versiones aprobadas definitivas, compilación, seguridad de dependencias ni infraestructura. Las comprobaciones documentales del cierre no sustituyen ni actualizan aquella evidencia.

#### Alcance F0 y dependencias siguientes

F0 — Baseline de autenticación comprende las seis tareas siguientes; no reabre la Fase 0 histórica de gobierno. La autorización del bloque no significa ejecutar automáticamente todas sus tareas:

| Tarea | Alcance | Dependencias | Estado tras este cierre |
| --- | --- | --- | --- |
| T-AUTH-001 | Delimitar ejecución y baseline documental/Git | Sin tarea previa | COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA |
| T-AUTH-002 | Evaluar SDK, dependencias y riesgos actuales | T-AUTH-001 | PENDIENTE / NO EJECUTADA |
| T-AUTH-003 | Fijar versiones y restore reproducible | T-AUTH-002 | PENDIENTE / NO EJECUTADA |
| T-AUTH-004 | Verificar higiene Git y secretos versionados | T-AUTH-001 | PENDIENTE / NO EJECUTADA |
| T-AUTH-005 | Proteger contexto Docker y publicación | T-AUTH-003, T-AUTH-004 | PENDIENTE / NO EJECUTADA |
| T-AUTH-006 | Consolidar estructura y checkpoint técnico | T-AUTH-003, T-AUTH-004, T-AUTH-005 | PENDIENTE / NO EJECUTADA |

Únicamente T-AUTH-001 queda cerrada aquí. T-AUTH-002 no se ha iniciado: antes de sus acciones deben delimitarse comandos, archivos, entorno, escrituras o accesos a red y autorizaciones aplicables. Los ocho gates, incluido GATE-A, continúan **PENDIENTES / NO SATISFECHOS**; F0 no incluye todos los componentes requeridos para superar GATE-A.

#### Operaciones permitidas y no autorizadas en T-AUTH-001

La inspección permitió únicamente lectura de las fuentes y comandos locales de consulta efectivamente ejecutados:

```text
git status
git branch --show-current
git log --oneline --decorate -n 10
git log -1 --format=%H
git remote -v
git --version
dotnet --info
dotnet --list-sdks
```

T-AUTH-001 **no autorizó** modificaciones de código, instalaciones, restore, builds, pruebas, scanners, Docker, conexiones o integración funcional MongoDB/Redis, endpoints, JWT, PasswordHasher, rate limiting, workflows, AWS, Terraform, despliegues, commit o push. Consultar `git remote -v` solo mostró configuración local. La autorización posterior para escribir este cierre no altera retrospectivamente los límites de la inspección ni inicia ninguna otra tarea.

**Archivos protegidos:** `constitution.md`, `.specify/memory/constitution.md` y `specs/001-backend-foundation/*`; se conservan los estados de FND/PLD/TBF/DEC, sin cierre retrospectivo. Durante la inspección no se autorizó modificar archivo alguno. Durante este cierre también permanecen intactos spec/plan de 002, código/configuración de Enver.API, Dockerfile, `.dockerignore`, `.gitignore`, AGENTS.md y README.md.

#### Riesgos trasladados, pendientes y sin corregir

Los IDs R-001 a R-008 identifican exclusivamente las observaciones de esta evidencia; no sustituyen riesgos o requisitos de las fuentes.

| ID | Observación y limitación de la inspección | Traslado para tratamiento autorizado |
| --- | --- | --- |
| R-001 | No existe `global.json` observado | T-AUTH-002 evalúa SDK; T-AUTH-003 define fijación aprobada |
| R-002 | No existen lockfiles observados | T-AUTH-003, tras evaluación de T-AUTH-002 |
| R-003 | Dependencias actuales sin evaluación vigente completa de grafo, vulnerabilidades y licencias | T-AUTH-002; decisiones posteriores en T-AUTH-003 |
| R-004 | Corrección OpenAPI informada requiere evidencia verificable, no inferencia únicamente por versión | T-AUTH-002; contraste DevSecOps posterior según catálogo |
| R-005 | Algunos comandos NuGet pueden provocar restore, escritura o acceso a red | Autorizar explícitamente esas acciones en T-AUTH-002/T-AUTH-003 antes de ejecutarlas |
| R-006 | `.dockerignore` requiere endurecimiento porque Dockerfile utiliza `COPY . .`; reintroduce metadatos Git y no cubre todas las variantes sensibles previstas | T-AUTH-005, sin construir antes con contexto sensible desprotegido |
| R-007 | Protección de publish pendiente | T-AUTH-005 |
| R-008 | Working tree limpio no demuestra ausencia de secretos en historial, archivos ignorados, binarios o artefactos locales | T-AUTH-004 con alcance explícito; no certificar lo no revisado |

No se corrige ninguno de estos riesgos en esta intervención. No se consultaron feeds ni se ejecutaron auditorías/scanners, restore, builds, pruebas, Docker, conexiones MongoDB/Redis o acciones cloud en la inspección; no hay evidencia de seguridad, licencias, rendimiento o despliegue. La coincidencia con una referencia remota local no acredita una consulta actual al servidor. No se afirma ausencia universal de secretos o vulnerabilidades.

**AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO.** T-AUTH-001 no aporta evidencia técnica para cerrarlo y no intenta resolverlo en F0. Se conserva el riesgo residual de pérdida Redis completamente silenciosa no observable, sin afirmar detección universal.

El cierre termina al registrar esta evidencia y el breve historial en memoria. No cambia el estado documental global APROBADO PARA EJECUCIÓN INCREMENTAL, no satisface globalmente AUTH-001/047/048, no cierra gates ni autoriza iniciar T-AUTH-002 automáticamente.

## 11. Cierre documental autorizado de T-AUTH-002

<a id="e-t-auth-002"></a>

### E(T-AUTH-002)

#### Autorización, resultado y trazabilidad

El responsable autorizó únicamente T-AUTH-002 — Evaluar SDK, dependencias y riesgos actuales, en modo PLAN / INSPECCIÓN TÉCNICA. La dependencia T-AUTH-001 estaba COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA. Se realizó inventario local y contraste de fuentes oficiales, sin modificar el repositorio. El dictamen **LISTA PARA CIERRE DOCUMENTAL** fue aprobado; una solicitud posterior BUILD DOCUMENTAL autoriza escribir este registro y añadir un historial breve en memoria, sin repetir comandos técnicos ni consultas externas. Esta aprobación no aprueba las versiones observadas como baseline definitiva.

| Campo | Evidencia registrada |
| --- | --- |
| Tarea / cierre | T-AUTH-002 — **COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA** |
| AUTH relacionados | AUTH-013, AUTH-018, AUTH-041, AUTH-044, AUTH-045 |
| PLD-AUTH relacionados | PLD-AUTH-03, PLD-AUTH-21 |
| AC relacionado | AC-21; sin acreditar su aceptación integral |
| Gates | GATE-A **PENDIENTE**; soporte futuro de GATE-H, también pendiente |
| Resultado esperado / obtenido | Inventariar plataforma/grafo/licencias, contrastar OpenAPI y riesgos antes de seleccionar versiones / evaluación realizada con los resultados y límites siguientes |
| Archivos de la evaluación | Lectura de csproj, Program.cs, documentos SDD/gobierno, assets/nuspecs y componentes locales; ninguno modificado durante la evaluación |
| Pruebas | Ninguna; consultas de inventario/auditoría NuGet, no build ni pruebas del backend |
| Commit del cierre / workflow run / captura académica | No generados; HEAD corresponde a una baseline previa |

La trazabilidad no declara satisfechos estos AUTH, PLD-AUTH o AC únicamente por cerrar la tarea. Los ocho gates siguen **PENDIENTES / NO SATISFECHOS**; T-AUTH-003 no se inicia.

#### Baseline y entorno observados el 2026-11-04

| Elemento | Observación de la evaluación anterior |
| --- | --- |
| Rama / HEAD | `feature/authentication` / `d1d29298f86e25070f7bd1835acecd25d88d3e35` |
| Commit de baseline | `docs(auth): finalize authentication planning and tasks` |
| Working tree antes/después | Mismos cambios documentales preexistentes: `memoria.md` y `specs/002-authentication/task.md`; no estaba limpio, no hubo cambios nuevos durante la evaluación |
| Remoto Git | No consultado nuevamente; la referencia local origin no acredita una consulta al servidor |
| Sistema | Windows x64, versión `10.0.19045` |
| SDK seleccionado / instalados | `10.0.300` / `9.0.205` y `10.0.300` |
| MSBuild / host .NET | `18.6.3` / `10.0.8` |
| Runtimes relevantes | `Microsoft.AspNetCore.App`, `Microsoft.NETCore.App` y `Microsoft.WindowsDesktop.App`: `8.0.27`, `9.0.6`, `9.0.16` y `10.0.8`, x64 |
| TargetFramework | `net10.0` |
| global.json / lockfiles | No encontrados |
| Conservación comprobada durante la evaluación | 31 archivos versionados con contenido SHA-256 idéntico antes/después; project.assets.json y los otros cuatro archivos NuGet del primer nivel de obj conservaron contenido y fecha de modificación |

No apareció incompatibilidad evidente entre SDK, runtime, TFM y assets seleccionados. No se validó mediante compilación o ejecución; compatibilidad aparente no equivale a mantenimiento o seguridad actualizados.

#### Comandos, restore y consultas de red

La ayuda del SDK y [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-package-list) confirmaron que .NET 10 puede restaurar implícitamente al listar paquetes. Antes de las consultas se identificó el riesgo de escritura de obj/project.assets.json y otros artefactos; se usó **--no-restore en las cuatro consultas de paquetes**. No se realizó restore nuevo ni instalación, y se revisó Git después de cada consulta. Las consultas de vulnerabilidades/outdated y fuentes oficiales sí emplearon acceso de red autorizado; no se afirma que todas fueran offline.

```text
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
dotnet package list --help
dotnet package list --project Enver.API/Enver.API.csproj --no-restore
dotnet package list --project Enver.API/Enver.API.csproj --no-restore --include-transitive
dotnet package list --project Enver.API/Enver.API.csproj --no-restore --vulnerable --include-transitive
dotnet package list --project Enver.API/Enver.API.csproj --no-restore --outdated
git status --short --branch --untracked-files=all
git log -1 --format="%H %s"
git log --oneline --all -- Enver.API/Enver.API.csproj
git show dcf04a4 -- Enver.API/Enver.API.csproj
```

También se realizaron lecturas/búsquedas y consultas PowerShell de assets, nuspecs, versiones de DLL y hashes, sin escribir informes en archivos. Dos intentos auxiliares fallaron por disponibilidad de Get-FileHash y sintaxis de canalización; se corrigieron mediante consultas de lectura. Una URL de release OpenAPI sin prefijo v devolvió 404; se consultó el tag oficial correcto v2.13.0. No se ejecutaron scanners adicionales a la consulta NuGet autorizada.

#### Grafo, dependencias directas y licencias

| Paquete | Versión declarada y resuelta | Uso observado | Licencia declarada y fuente oficial consultada |
| --- | --- | --- | --- |
| Microsoft.AspNetCore.OpenApi | 10.0.8 | Generación OpenAPI mediante AddOpenApi/MapOpenApi | MIT; [NuGet 10.0.8](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/10.0.8) |
| Microsoft.OpenApi | 2.13.0 | Modelo/serialización OpenAPI; selección directa de la dependencia | MIT; [NuGet 2.13.0](https://www.nuget.org/packages/Microsoft.OpenApi/2.13.0) |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 | Targets MSBuild para tooling de contenedores de Visual Studio | Microsoft Software License Terms — Visual Studio Container Tools Targets; [NuGet](https://www.nuget.org/packages/Microsoft.VisualStudio.Azure.Containers.Tools.Targets/1.23.0) y [licencia](https://www.nuget.org/packages/Microsoft.VisualStudio.Azure.Containers.Tools.Targets/1.23.0/License) |

No se observaron paquetes exclusivamente transitivos adicionales. Microsoft.AspNetCore.OpenApi 10.0.8 declara Microsoft.OpenApi >= 2.0.0; **Microsoft.OpenApi se resuelve una sola vez como 2.13.0**, no como otra copia transitiva 2.0.0. El asset seleccionado de OpenAPI.NET es lib/net8.0, cuyo grupo no añade dependencias. Los frameworks compartidos no son paquetes transitivos adicionales del listado. No existen todavía JwtBearer, IdentityModel, MongoDB.Driver ni StackExchange.Redis en el grafo del proyecto; no se presentan las dependencias futuras como instaladas.

Container Tools incorpora DLL embebidas fuera de las dependencias NuGet separadas: se observaron Newtonsoft.Json.dll (producto 13.0.1) y System.Security.Principal.Windows.dll (producto 5.0.0). Su ThirdPartyNotices.txt declara MIT para Json.NET y Apache-2.0 para material BindingAddress. No se hizo auditoría binaria exhaustiva. La licencia principal del tooling no es MIT; permite desarrollo/pruebas bajo sus términos y requiere revisar distribución/uso en build. No se identificó impedimento evidente para desarrollo/pruebas en la lectura acotada; no es dictamen legal ni aprobación general de redistribución. Conservar avisos aplicables de las licencias.

#### Vulnerabilidades, alcance y deuda de plataforma

**Resultado observado:** dotnet package list --no-restore --vulnerable --include-transitive **no reportó paquetes vulnerables en el grafo NuGet actual**. Herramienta: SDK 10.0.300. Fuentes mostradas: https://api.nuget.org/v3/index.json y la carpeta local Microsoft SDKs/NuGetPackages. Se contrastó además el [índice oficial de vulnerabilidades NuGet](https://api.nuget.org/v3/vulnerabilities/index.json), cuyos datos base/update consultados indicaban actualización 2026-11-04T11:49:31.6454276Z. Todo el contraste corresponde a la fecha de consulta 2026-11-04, no a una nueva auditoría durante este cierre.

Ese resultado **no cubre automáticamente SDK, runtime, frameworks compartidos, DLL embebidas, imágenes ni vulnerabilidades no publicadas**. No certifica seguridad global del proyecto. El feed consultado no contenía todos los avisos presentes en el repositorio oficial OpenAPI.NET; por ello se contrastaron también fuentes del mantenedor.

| Componente / tipo | Aviso oficial y severidad contrastados | Corrección relevante y evaluación para ENVER |
| --- | --- | --- |
| Microsoft.OpenApi, directo y dependencia del paquete ASP.NET OpenAPI | [GHSA-v5pm-xwqc-g5wc / CVE-2026-49451](https://github.com/microsoft/OpenAPI.NET/security/advisories/GHSA-v5pm-xwqc-g5wc), HIGH 7.5 | Rama 2.x corregida en 2.7.5; 2.13.0 queda fuera del rango afectado. Terminación al procesar documentos maliciosos en versiones anteriores; conservar resolución parcheada |
| Mismo paquete core | [GHSA-m2pf-rgqv-ggvv](https://github.com/microsoft/OpenAPI.NET/security/advisories/GHSA-m2pf-rgqv-ggvv), HIGH 7.5 | Rama 2.x: frontera conservadora 2.12.2; 2.13.0 queda fuera del rango afectado. Riesgo de recursión/agotamiento de recursos en operaciones afectadas; no actualizar automáticamente a 3.x |
| Readers/YamlReader, ausentes del grafo | [GHSA-7pxr-59rr-hqj2 / CVE-2026-72923](https://github.com/microsoft/OpenAPI.NET/security/advisories/GHSA-7pxr-59rr-hqj2), HIGH 7.5 | YamlReader 2.x corregido en 2.12.0, con endurecimiento posterior 2.12.2; no se identificaron estos paquetes instalados en el proyecto |
| SDK Windows 10.0.300, no PackageReference | [CVE-2026-45490](https://github.com/dotnet/announcements/issues/403), MODERATE 6.0 | Corregido para ese aviso en SDK 10.0.301; elevación local mediante workload. Seleccionar SDK mantenido, no congelar 10.0.300 sin evaluación |
| Runtime 10.0.8, Brotli, no PackageReference | [CVE-2026-111497](https://github.com/dotnet/announcements/issues/494), HIGH 8.1 según Microsoft/.NET | Runtime 10.0.14; exposición requiere descomprimir datos Brotli controlados por atacante, ruta no observada en Program.cs. Actualización pendiente |
| Runtime Windows 10.0.8, sockets, no PackageReference | [CVE-2026-70197](https://github.com/dotnet/announcements/issues/493), HIGH 8.1 | Runtime 10.0.14; requiere patrón concreto de reutilización UDP IPv4/IPv6, no demostrado en ENVER. Actualización pendiente |
| Runtime 10.0.8, System.Formats.Tar | CVE-2026-45491, MODERATE 6.8, [notas oficiales 10.0.9](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.9/10.0.9.md) | Corregido para ese aviso en 10.0.9; extracción TAR/enlaces simbólicos no observados como funcionalidad ENVER; mantenimiento pendiente |
| ASP.NET Core 10.0.8, SignalR/Blazor | CVE-2026-45591, HIGH 7.5, mismas notas oficiales 10.0.9 | Corregido para ese aviso en 10.0.9; ruta MessagePack/SignalR no observada, sin afirmar explotación de ENVER |

La tabla no es una auditoría exhaustiva de todas las CVE del entorno ni prueba explotación. No se identificaron casos CRITICAL/LOW aplicables en el contraste efectuado; no es garantía universal. **La plataforma instalada SDK 10.0.300 / runtime 10.0.8 está atrasada respecto a parches de seguridad actuales.** La [metadata oficial .NET 10](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json) y las [notas 10.0.14](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.14/10.0.14.md) publicaban SDK 10.0.403 y runtime 10.0.14: candidatos a evaluar en T-AUTH-003, no versiones adoptadas o instaladas aquí.

#### Outdated y estado específico OpenAPI

| Paquete | Actual | Última estable mostrada por outdated | Tratamiento propuesto, no ejecutado |
| --- | --- | --- | --- |
| Microsoft.AspNetCore.OpenApi | 10.0.8 | 10.0.14 | Evaluar revisión compatible de mantenimiento; cambiar el paquete no corrige por sí solo el runtime instalado |
| Microsoft.OpenApi | 2.13.0 | 3.11.0 | Major distinto: evaluación separada, no actualización automática a latest; la rama 2.x parcheada cubre los avisos contrastados |
| Container Tools | 1.23.0 | No se mostró actualización | Tooling no funcional de autenticación; revisar necesidad, aislamiento y licencia |

No se observó conflicto de resolución de Microsoft.OpenApi. La referencia directa se incorporó en dcf04a4, coherente con la corrección informada; no se observó uso explícito de APIs del modelo en el código consultado. **La referencia directa 2.13.0 se conserva por ahora**: con ASP.NET OpenAPI 10.0.8, quitarla podría permitir resolver una versión mínima vulnerable. Los advisories core revisados quedan corregidos antes de 2.13.0; el informe High histórico original no se identificó inequívocamente y no se atribuye retrospectivamente un ID concreto a aquel informe.

Se recomienda evaluar Microsoft.AspNetCore.OpenApi **10.0.8 → 10.0.14** y después reevaluar la necesidad del pin directo. El [nuspec oficial 10.0.14 consultado](https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.openapi/10.0.14/microsoft.aspnetcore.openapi.nuspec) exige Microsoft.OpenApi **[2.12.2, 3.0.0)**; la decisión futura deberá verificar y bloquear el grafo resultante. No se elimina la referencia ni se adopta 3.x en esta tarea.

#### Riesgos heredados: actualización documental solo en E(T-AUTH-002)

Se conservan las observaciones históricas R-001 a R-008 de E(T-AUTH-001). La siguiente evaluación posterior no elimina riesgos ni corrige configuración:

| ID | Estado documental en esta evidencia | Motivo y continuidad |
| --- | --- | --- |
| R-001 | **PENDIENTE** | Sin global.json observado; selección y fijación en T-AUTH-003 |
| R-002 | **PENDIENTE** | Sin lockfiles observados; política reproducible en T-AUTH-003 |
| R-003 | **REDUCIDO** | Inventario/grafo/licencias/auditoría actual realizados, con límites; persisten deuda SDK/runtime, componentes embebidos y necesidad de repetir auditoría del grafo definitivo |
| R-004 | **REDUCIDO** | OpenAPI 2.13.0 contrastado con advisories oficiales; informe histórico original no identificado inequívocamente |
| R-005 | **REDUCIDO** | Restore evitado mediante --no-restore; hubo consultas de red autorizadas y comprobación Git posterior. Mantener autorización expresa para acciones futuras |

#### Recomendaciones para T-AUTH-003, sin iniciarla

1. Seleccionar versión mantenida del SDK .NET 10, verificando compatibilidad con IDE/runner.
2. Evaluar SDK 10.0.403.
3. Evaluar runtime 10.0.14.
4. Evaluar Microsoft.AspNetCore.OpenApi 10.0.14.
5. Mantener Microsoft.OpenApi en rama 2.x compatible y parcheada; reevaluar justificadamente el pin directo después de evaluar el paquete padre.
6. No adoptar 3.x automáticamente.
7. Definir global.json.
8. Definir política de rollForward.
9. Definir lockfiles.
10. Definir restore reproducible/bloqueado, delimitando escrituras y red antes de ejecutarlo.
11. Repetir auditoría después de fijar el grafo definitivo.
12. Mantener evaluación de futuras dependencias MongoDB/Redis/JWT para sus tareas correspondientes; no incorporarlas como consecuencia automática de este cierre.

**Límites y cierre:** no se modificaron archivos durante la evaluación previa ni se instalaron paquetes, ejecutó restore nuevo, build, pruebas, Docker, conexiones MongoDB/Redis, workflows, commit o push. No se hicieron scanners adicionales a la consulta NuGet expresamente autorizada. No se garantiza frescura permanente de feeds/advisories, seguridad de DLL embebidas/imágenes o compatibilidad productiva; los resultados no acreditan aceptación de AC-21. Este cierre posterior modifica únicamente task.md y añade memoria §18, sin nueva auditoría, cambio de versiones o creación de global.json/lockfiles. T-AUTH-001 permanece completada; T-AUTH-003..079 siguen **PENDIENTE / NO EJECUTADA** y los ocho gates siguen pendientes, incluido GATE-A. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Se detiene el trabajo sin iniciar T-AUTH-003.

## 12. Ejecución y cierre técnico autorizado de T-AUTH-003

<a id="e-t-auth-003"></a>

### E(T-AUTH-003)

#### Autorización, trazabilidad y baseline

El responsable autoriza en modo BUILD exclusivamente T-AUTH-003 — Fijar versiones y restore reproducible. Autoriza crear global.json, actualizar solo Microsoft.AspNetCore.OpenApi, habilitar/generar packages.lock.json y ejecutar restore normal/bloqueado, build sin restore, auditoría NuGet y outdated diagnóstico. Solo después del éxito técnico permite modificar este documento y memoria para registrar la evidencia y el estado **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. T-AUTH-001/002 estaban completadas documentalmente. Los intentos previos se detuvieron sin cambios porque 10.0.403 no aparecía; el responsable realizó la instalación manual y autorizó esta reanudación. El agente no instaló el SDK.

| Campo | Resultado observado el 2026-11-04 |
| --- | --- |
| AUTH relacionados | AUTH-013, AUTH-018, AUTH-041–045; preparación técnica, no satisfacción integral de esos requisitos |
| PLD-AUTH / AC | PLD-AUTH-03/20/21; AC-21, cuya aceptación integral sigue pendiente |
| Gates | GATE-A **PENDIENTE**; soporte futuro de GATE-H, también pendiente; los ocho gates no se cierran |
| Rama / HEAD inicial | `feature/authentication` / `d1d29298f86e25070f7bd1835acecd25d88d3e35` |
| Estado Git inicial | Modificaciones preexistentes en `memoria.md` y `specs/002-authentication/task.md`, conservadas; no era un árbol limpio |
| Remoto Git | No consultado nuevamente; origin en el estado es una referencia local |
| Entorno | Windows x64, versión 10.0.19045; RID win-x64; raíz `C:\API_SDD_ENVER\Enver.Backend` |
| SDK anterior observado | 10.0.300, MSBuild 18.6.3, host/runtime 10.0.8, según E(T-AUTH-002) e intentos bloqueados |
| SDK al reanudar y tras global.json | **10.0.403**, commit 5a2162bea6; instalación y selección confirmadas antes de modificar archivos; dotnet --version desde raíz devuelve 10.0.403 después del pin |
| SDK instalados | 9.0.205, 10.0.300 y 10.0.403, en `C:\Program Files\dotnet\sdk` |
| MSBuild / host finales observados | **18.9.15+5a2162bea** / **10.0.14**, commit 0427b9abbe; Microsoft.NETCore.App y Microsoft.AspNetCore.App 10.0.14 instalados |
| Framework | **net10.0**, sin cambio; salida de compilación Debug/net10.0/Enver.API.dll |
| Commit del cambio / workflow / captura | No generados; HEAD identifica la baseline anterior, no un commit de esta implementación |

#### Configuración y dependencias finales

Se crea en la raíz el siguiente global.json, confirmado mediante dotnet --version:

```json
{
  "sdk": {
    "version": "10.0.403",
    "allowPrerelease": false,
    "rollForward": "disable"
  }
}
```

La política exacta disable fue aprobada expresamente: evita cambios implícitos de SDK/NuGet y de grafo entre máquinas. A diferencia de latestPatch, feature y minor, no permite avanzar automáticamente ni a otro parche, banda o minor. Los parches de seguridad futuros requieren actualizar deliberadamente el pin y revalidar restore/grafo/build; no significa congelar indefinidamente esta versión. La [guía oficial de lockfiles y SDK](https://learn.microsoft.com/en-us/dotnet/core/install/upgrade#package-lock-files) consultada en la evaluación previa recomienda deshabilitar roll-forward. SDK 10.0.403/runtime 10.0.14 fueron contrastados previamente con [metadata oficial .NET 10](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json); esta ejecución confirma su disponibilidad local, no instala herramientas.

Cambios exactos en Enver.API.csproj: añadir `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>` al PropertyGroup existente y cambiar Microsoft.AspNetCore.OpenApi de 10.0.8 a 10.0.14. No se cambia TFM, código, otras referencias, metadatos del tooling ni estructura de solución.

| Dependencia directa | Antes | Declarada/resuelta final | Licencia / tratamiento |
| --- | --- | --- | --- |
| Microsoft.AspNetCore.OpenApi | 10.0.8 | **10.0.14** | MIT; nuspec oficial previamente consultado declara net10.0 y dependencia Microsoft.OpenApi [2.12.2, 3.0.0), confirmada ahora en lockfile |
| Microsoft.OpenApi | 2.13.0 | **2.13.0** | MIT; referencia directa conservada, sin adopción de 3.x |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 | **1.23.0** | Microsoft Software License Terms — Visual Studio Container Tools Targets; se conserva tooling de Visual Studio, no funcionalidad de autenticación |

El restore generó **Enver.API/packages.lock.json**, formato 1, sección net10.0, tres paquetes Direct con versiones resueltas y contentHash. Los rangos requested del lockfile son mínimos NuGet; las versiones exactas resueltas se verifican con locked-mode. Assets y lockfile contienen únicamente estas tres bibliotecas, sin paquetes exclusivamente transitivos adicionales ni MongoDB.Driver, StackExchange.Redis, JwtBearer, IdentityModel u otra dependencia funcional nueva. Microsoft.OpenApi se resuelve una sola vez como 2.13.0 y satisface el rango del paquete padre. Su pin podría reevaluarse en otro cambio autorizado; no se elimina aquí ni se reconstruye el informe High histórico no identificado.

SHA-256 del lockfile tras generarlo: `6B6E8F6A214D7850EDB9BD00A19CA83FDF0321B27606CF929F6561A218562EBC`; permaneció idéntico después de restore bloqueado, build y consultas de auditoría/outdated. No se editó manualmente. Container Tools no impidió restore/build; esto no valida su flujo Docker/depuración ni audita sus DLL embebidas.

#### Comandos y resultados reales

Ejecutados desde la raíz; sintaxis de una línea compatible con Windows. No se ejecutaron pruebas ni la aplicación.

| Comando | Resultado obtenido |
| --- | --- |
| `dotnet --list-sdks` | 9.0.205, 10.0.300 y 10.0.403; verificaciones inicial/final |
| `dotnet --info` | Selecciona 10.0.403 antes de cambiar archivos; MSBuild/host/entorno de la tabla anterior |
| `dotnet --version` | **10.0.403** desde la raíz tras crear global.json; comprobación final conservada |
| `dotnet restore Enver.API/Enver.API.csproj` | Correcto; informa restauración en 2.39 s; genera lockfile y actualiza artefactos NuGet locales |
| `dotnet restore Enver.API/Enver.API.csproj --locked-mode` | Correcto; informa restauración en 775 ms; lockfile sin cambios |
| `dotnet build Enver.API/Enver.API.csproj --no-restore` | **Compilación correcta, 0 advertencias, 0 errores**, 20.92 s; Enver.API/bin/Debug/net10.0/Enver.API.dll |
| `dotnet package list --project Enver.API/Enver.API.csproj --no-restore --vulnerable --include-transitive` | No reportó paquetes vulnerables en el grafo consultado; ningún paquete/severidad reportados, sin supresiones añadidas |
| `dotnet package list --project Enver.API/Enver.API.csproj --no-restore --outdated` | Solo Microsoft.OpenApi: solicitado/resuelto 2.13.0, más reciente 3.11.0; sin actualización automática |

Herramienta de auditoría: CLI NuGet incluida en **SDK 10.0.403**. La consulta vulnerable y outdated muestran **https://api.nuget.org/v3/index.json** y **C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\**. Los assets registran además **C:\Program Files\dotnet\library-packs** como origen de restore. Son consultas con red/cachés autorizadas, no una certificación de frescura absoluta del feed ni auditoría universal.

La auditoría cubre paquetes NuGet directos y transitivos presentes en el grafo restaurado para net10.0 y los avisos disponibles en esos orígenes. **No cubre automáticamente SDK/runtime/frameworks compartidos, DLL embebidas, imágenes, vulnerabilidades no publicadas ni seguridad funcional de la aplicación.** El contraste oficial previo OpenAPI de E(T-AUTH-002) sigue siendo un antecedente separado; aquí se revalida la resolución compatible mediante lockfile/assets, build y consulta NuGet, sin afirmar nueva revisión exhaustiva de advisories.

Outdated no mostró actualización para Microsoft.AspNetCore.OpenApi 10.0.14 o Container Tools 1.23.0. La versión 3.11.0 de OpenAPI.NET es otra major y no se adopta. No se ignoraron advertencias/errores de seguridad ni se añadieron NoWarn/excepciones.

Se revisaron también estado Git, HEAD, diff, XML del proyecto, JSON de global/lock/assets y hashes mediante lecturas/comandos de solo inspección. Task/memoria conservaron exactamente su contenido previo hasta completar restore normal, locked, build y revisión de auditoría. El primer restore con este SDK mostró mensajes de primera ejecución/telemetría y un anuncio de instalación de certificado HTTPS de desarrollo; no se ejecutó dev-certs --trust ni se auditó el almacén de certificados. No se afirma ausencia de escrituras en cachés/perfil/almacenes externos al repositorio. No se corrigió el estado global de workloads ni se ejecutó workload restore.

#### Evaluación posterior de riesgos, sin borrar el historial

| ID | Estado en E(T-AUTH-003) | Evidencia y límites |
| --- | --- | --- |
| R-001 | **RESUELTO** | global.json válido selecciona exactamente SDK instalado 10.0.403, allowPrerelease=false y rollForward=disable. Mantener actualizaciones explícitas de seguridad; no fija por sí solo todo runtime de despliegue |
| R-002 | **RESUELTO** | Lockfile del proyecto generado por restore, consistente con assets y verificado por locked-mode sin alterarlo; proyectos/dependencias futuros necesitan sus propios lockfiles y validaciones |
| R-003 | **REDUCIDO / REVALIDADO** | Grafo definitivo actual verificado, licencias de las tres dependencias conservadas según evaluación/nuspecs previos, build y consulta NuGet sin hallazgos reportados; límites de plataforma, DLL embebidas e imágenes siguen explícitos |
| R-004 | **REDUCIDO / REVALIDADO** | OpenAPI ASP.NET 10.0.14 + core 2.13.0 compatibles en grafo real/build; pin conservado, sin conflicto observado ni hallazgo NuGet. El informe High histórico original sigue sin identificación inequívoca |
| R-005 | **REDUCIDO / COMPORTAMIENTO DE RESTORE DOCUMENTADO** | Restore normal y bloqueado autorizados actualizan obj/caché; generación automática de lockfile y consultas de red permitidas. Build y list usan --no-restore; lockfile estable y fuentes/escrituras/primera ejecución documentadas |

Los estados anteriores en E(T-AUTH-001/002) permanecen intactos como historial; tampoco se cierran R-006..008 u otros riesgos por esta intervención. Un restore normal futuro puede actualizar el lockfile si cambian entradas; **esta tarea valida el comando explícito --locked-mode, no activa RestoreLockedMode global ni crea CI**. La reproducibilidad aquí es operacional del SDK/grafo actual, no identidad binaria bit a bit, restauración probada con caché vacía o validación de otros runners/sistemas/IDE. Los SDK/runtimes antiguos siguen instalados; no se desinstalan ni se certifica todo el equipo. No se ejecutó la aplicación para demostrar qué runtime cargaría en producción.

#### Archivos, Git final y cierre

Archivos técnicos: **global.json** creado; **Enver.API/Enver.API.csproj** modificado solo como se indicó; **Enver.API/packages.lock.json** generado. Después del éxito técnico, se modifica únicamente **specs/002-authentication/task.md** para esta evidencia/estado/resúmenes y se añade **memoria.md §19**, preservando su historia. Bin/obj son artefactos locales generados/ignorados; no se añaden al índice. Los otros 28 archivos versionados conservaron su contenido comprobado por SHA-256; spec/plan, constituciones, 001, Program.cs, appsettings, Dockerfile, .dockerignore, .gitignore, solución y workflows no se modifican.

Estado Git de cierre, conservando los cambios documentales anteriores y sin preparar archivos:

```text
## feature/authentication...origin/feature/authentication
 M Enver.API/Enver.API.csproj
 M memoria.md
 M specs/002-authentication/task.md
?? Enver.API/packages.lock.json
?? global.json
```

**Resultado: T-AUTH-003 — COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, limitada a la baseline aprobada. T-AUTH-001/002 y sus evidencias se conservan; **T-AUTH-004..079 (76 tareas) siguen PENDIENTE / NO EJECUTADA**. Los **8 gates siguen PENDIENTES / NO SATISFECHOS**, incluido GATE-A; no se acredita AC-21 integral ni se cierra foundation retrospectivamente. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin autenticación implementada, pruebas, ejecución de la API, conexiones MongoDB/Redis, Docker, workflows, AWS, commit o push. Se detiene el trabajo sin iniciar T-AUTH-004.

## 13. Cierre documental autorizado de T-AUTH-004

<a id="e-t-auth-004"></a>

### E(T-AUTH-004)

#### Autorización, trazabilidad y alcance de la evidencia

El responsable autorizó únicamente T-AUTH-004 — Verificar higiene Git y secretos versionados, en modo PLAN / INSPECCIÓN TÉCNICA de solo lectura. Aprobó el dictamen **LISTA PARA CIERRE DOCUMENTAL** y posteriormente autorizó, en modo BUILD DOCUMENTAL, modificar solo este documento y añadir un registro breve en memoria para cerrar la tarea como **COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA**. Este cierre registra los resultados ya observados; no repite búsquedas de secretos, comprobaciones check-ignore o revisión del historial, ni ejecuta scanners.

| Campo | Evidencia de la inspección previa |
| --- | --- |
| Fecha | **2026-11-04** |
| Rama / HEAD | `feature/authentication` / `d1d29298f86e25070f7bd1835acecd25d88d3e35` |
| AUTH relacionados | **AUTH-034, AUTH-044, AUTH-045** |
| PLD-AUTH relacionados | **PLD-AUTH-21, PLD-AUTH-22** |
| AC relacionados | **AC-19, AC-21**; sin aceptación integral por cerrar esta tarea |
| Gate / soporte | **GATE-A PENDIENTE**; soporte futuro de **GATE-H**, también pendiente |
| Entorno / herramientas | Repositorio local en Windows; Git **2.55.0.windows.3** y lecturas/comprobaciones PowerShell, sin instalar herramientas |
| Resultado esperado / obtenido | Comprobar higiene actual, reglas, versionabilidad y candidatos sensibles sin modificar archivos / inspección acotada y 44 comprobaciones correctas, sin correcciones requeridas dentro del alcance |
| Commit del cierre / workflow / captura | No generados; HEAD identifica la baseline anterior, no un commit de este cierre |

La trazabilidad no declara cumplidos globalmente estos AUTH, PLD-AUTH o AC. Los ocho gates permanecen **PENDIENTES / NO SATISFECHOS** y la tarea no acredita un secret scanning exhaustivo ni seguridad de autenticación.

#### Estado Git inicial/final de la inspección y conservación

El estado fue **idéntico antes y después de la inspección**, con cambios válidos preexistentes de T-AUTH-003 y cierres anteriores. No era un working tree limpio; no hubo cambios nuevos ni archivos preparados en el índice:

```text
## feature/authentication...origin/feature/authentication
 M Enver.API/Enver.API.csproj
 M memoria.md
 M specs/002-authentication/task.md
?? Enver.API/packages.lock.json
?? global.json
```

No se consultó nuevamente el remoto Git; origin representa una referencia local, no evidencia de sincronización nueva con el servidor. Se revisaron **31 archivos versionados** en sus copias de trabajo y sus versiones del índice, y los **2 archivos nuevos no ignorados** (global.json y packages.lock.json). Los 33 archivos actuales conservaron el manifiesto SHA-256 inicial/final `DCC73948B2E91DC86F10117F79A78E846CDE64887444BF748026753146FFC8E1`. Esta identidad de contenido corresponde a aquella inspección, no niega las ediciones documentales expresamente autorizadas para este cierre posterior.

Entre los archivos versionados relevantes estaban .gitignore, .dockerignore, Enver.API.csproj, Program.cs, appsettings.json, appsettings.Development.json, Properties/launchSettings.json, Enver.API.http, Dockerfile, solución y documentos de gobierno/specs. No se observaron artefactos generados actualmente versionados ni archivos seguidos que contradijeran las reglas de exclusión comprobadas.

#### Comandos y método ya realizados

La inspección utilizó `git --no-optional-locks` para evitar refrescos opcionales del índice. Se ejecutaron status normal y corto, status --ignored, ls-files, listados de ignorados/no versionados, lectura de blobs del índice con cat-file, revisión histórica por nombres con log y check-ignore sobre rutas sintéticas. Las llamadas parametrizadas por ruta no volcaron valores sensibles; no se crearon archivos sintéticos ni temporales persistentes.

```text
git --no-optional-locks status
git --no-optional-locks status --short --branch --untracked-files=all
git --no-optional-locks status --ignored --short --untracked-files=normal
git --no-optional-locks ls-files
git --no-optional-locks ls-files --others --exclude-standard
git --no-optional-locks ls-files --others --ignored --exclude-standard
git --no-optional-locks log -1 --format="%H %s"
git --no-optional-locks log --all --format= --name-only --no-renames
git --no-optional-locks log --all --diff-filter=D --format="%h %ad" --date=short -- .vs Enver.API/bin Enver.API/obj Enver.API/Enver.API.csproj.user
git --no-optional-locks diff --cached --name-only
git --version
```

Para las 44 rutas siguientes se usó `check-ignore --no-index -v` (con core.quotePath=false en la matriz), interpretando las reglas negadas `!` como versionables; también se contrastaron las rutas seguidas con las reglas. La búsqueda PowerShell en textos/índice comprobó patrones y asignaciones con salida limitada a metadatos redactados; no fue Gitleaks. Se revisaron estructura y valores base de ambos appsettings y únicamente la presencia de UserSecretsId en csproj, sin abrir su almacén. Un intento auxiliar inicial de cálculo de hashes falló por sintaxis; se corrigió mediante lectura, sin escrituras ni cambios del estado inspeccionado.

#### Cobertura .gitignore y pruebas check-ignore

La lectura y las comprobaciones confirmaron cobertura de **.vs/**, **bin/**, **obj/**, **TestResults/**, **coverage/**, **\*.trx**, **\*.log**, **.env**, **.env.\*** con excepción **!.env.example**, **appsettings.Local.json**, **appsettings.\*.Local.json**, **secrets.json**, **\*.pfx**, **\*.p12**, **\*.pem**, **\*.key**, **.terraform/**, **\*.tfstate**, **\*.tfstate.\***, **\*.tfvars** y **\*.tfvars.json**. Se conserva versionable **.terraform.lock.hcl** mediante su excepción; también los appsettings base, global.json y Enver.API/packages.lock.json. No se modifica .gitignore.

**Resultado: 44 comprobaciones correctas; 28/28 rutas esperadas como ignoradas y 16/16 esperadas como versionables.** Las rutas de la tabla fueron comprobadas sin crearlas:

| Rutas comprobadas | Resultado | Cantidad |
| --- | --- | --- |
| `.vs/probe.suo`; `Enver.API/bin/probe.dll`; `Enver.API/obj/probe.json` | Ignoradas | 3 |
| `TestResults/probe.txt`; `tests/TestResults/probe.txt`; `coverage/probe.xml`; `tests/coverage/probe.xml` | Ignoradas | 4 |
| `probe.trx`; `probe.log` | Ignoradas | 2 |
| `.env`; `.env.local`; `Enver.API/.env.production` | Ignoradas | 3 |
| `appsettings.Local.json`; `Enver.API/appsettings.Local.json`; `Enver.API/appsettings.Development.Local.json` | Ignoradas | 3 |
| `secrets.json`; `Enver.API/secrets.json` | Ignoradas | 2 |
| `secret.key`; `test.pfx`; `test.p12`; `test.pem` | Ignoradas | 4 |
| `.terraform/probe`; `infra/.terraform/probe` | Ignoradas | 2 |
| `terraform.tfstate`; `terraform.tfstate.backup`; `infra/terraform.tfstate`; `terraform.tfvars`; `infra/probe.tfvars.json` | Ignoradas | 5 |
| `.env.example`; `Enver.API/.env.example` | Versionables | 2 |
| `.terraform.lock.hcl`; `infra/.terraform.lock.hcl` | Versionables | 2 |
| `Enver.API/appsettings.json`; `Enver.API/appsettings.Development.json` | Versionables | 2 |
| `global.json`; `Enver.API/packages.lock.json` | Versionables; existentes, aún no versionados | 2 |
| `Enver.API/Enver.API.csproj`; `Enver.API/Program.cs`; `Enver.API/Dockerfile`; `.dockerignore`; `.gitignore`; `.github/workflows/ci.yml`; `specs/002-authentication/task.md`; `memoria.md` | Versionables; prueba de ruta, no creación de workflow | 8 |

El lockfile NuGet no se clasificó como secreto; que global.json y packages.lock.json sean versionables no significa que ya se hayan incorporado al índice o comprometido. .gitignore no protege por sí solo Docker/publish ni impide una adición forzada; esa protección adicional permanece fuera de este cierre.

#### Revisión acotada de candidatos sensibles y configuración

**No se detectaron candidatos de secretos literales dentro del alcance revisado.** Los patrones incluyeron password assignments, claves, tokens, connection strings, URI MongoDB, credenciales Redis/AWS, Bearer literal, JWT y cabeceras de claves privadas, además de asignaciones genéricas key/secret/token. Se revisaron archivos actuales versionados, sus versiones del índice y los dos nuevos no ignorados; las menciones documentales y nombres de API no se confundieron con secretos. No se mostraron valores sensibles completos ni se detectó un secreto confirmado al que asignar severidad o tratamiento de rotación. Esto no garantiza ausencia universal de secretos.

| Archivo / componente | Resultado observado |
| --- | --- |
| `Enver.API/appsettings.json` | Solo configuración base de logging con niveles estándar y AllowedHosts de plantilla |
| `Enver.API/appsettings.Development.json` | Solo configuración base de logging con niveles estándar |
| Ambos appsettings | No se detectaron credenciales, connection strings reales, signing keys, tokens o claves AWS; tampoco placeholders sensibles |
| UserSecretsId | **CONFIGURADO**; solo se verificó presencia, sin registrar su valor ni inspeccionar el contenido de User Secrets |

Se enumeraron **57 archivos ignorados** por nombre/ruta. No aparecieron nombres candidatos de .env, secrets.json, configuración local sensible, claves/certificados, estados/variables Terraform o almacenes .aws/.ssh/.azure en ese listado. Sí existían archivos locales potencialmente sensibles por su función: Enver.API/Enver.API.csproj.user, .vs/Enver.Backend.slnx/v18/.suo y DocumentLayout*.json de Visual Studio. **Sus contenidos no se abrieron**; no se afirma que contengan secretos ni que estén libres de ellos.

#### Historial por rutas y evaluación posterior de R-008

La revisión de nombres/rutas en referencias locales (`log --all`) identificó 88 rutas históricas. Se observaron estos artefactos antes versionados y retirados del seguimiento en **04ff903**:

| Ruta histórica | Archivos |
| --- | --- |
| `.vs/` | **10** |
| `Enver.API/bin/` | **10** |
| `Enver.API/obj/` | **36** |
| `Enver.API/Enver.API.csproj.user` | **1** |

No aparecieron nombres candidatos de secretos en esa búsqueda histórica acotada, pero **no se auditó exhaustivamente el contenido de los blobs**. La retirada del seguimiento y las reglas actuales no borran versiones históricas ni acreditan que su contenido esté libre de secretos. No se vuelcan commits completos, se reescribe historial o se declara el historial limpio.

| Riesgo heredado | Estado posterior únicamente en esta evidencia | Motivo y continuidad |
| --- | --- | --- |
| **R-008** | **REDUCIDO — NO RESUELTO** | Higiene actual, versionabilidad, appsettings y patrones acotados comprobados; siguen fuera del alcance blobs históricos no auditados exhaustivamente, contenido de archivos ignorados, binarios, artefactos locales, secretos ofuscados o fragmentados, User Secrets y almacenes externos. Auditoría completa del historial pendiente para DevSecOps/Gitleaks bajo autorización propia |

No se crea otro riesgo ni se elimina/reformula la observación histórica de E(T-AUTH-001); las evidencias y evaluaciones previas de 001–003 se conservan intactas. Los patrones pueden omitir formatos no reconocidos, contenido generado o material fuera de las referencias locales; no hubo fetch ni certificación de repositorios remotos. Cualquier exposición futura requerirá evaluación y tratamiento/rotación, no solo ignorar o borrar el archivo.

#### Cierre documental y límites

Este acto posterior modifica únicamente **specs/002-authentication/task.md** para estado/evidencia/resúmenes y añade **memoria.md §20**, sin reescribir su historial. Conserva las modificaciones técnicas y archivos nuevos de T-AUTH-003; spec/plan, constituciones, foundation, Program.cs, csproj, global.json, lockfile, appsettings, Dockerfile, .dockerignore, .gitignore y workflows permanecen intactos en este cierre. No se repite la inspección técnica; las verificaciones de esta entrega se limitan a integridad documental, estados, conservación de archivos y formato Git.

**T-AUTH-004 — COMPLETADA / EVIDENCIA DOCUMENTAL REGISTRADA**. T-AUTH-001/002 conservan su cierre documental y T-AUTH-003 su cierre técnico; **T-AUTH-005..079 (75 tareas) siguen PENDIENTE / NO EJECUTADA**. Los **8 gates siguen PENDIENTES / NO SATISFECHOS**, incluido **GATE-A**; no se acredita cumplimiento global AUTH/AC ni cierre retrospectivo de foundation. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin implementación, instalaciones, restore/build/tests/scanners nuevos, conexiones MongoDB/Redis, Docker, commit o push. Trabajo detenido sin iniciar T-AUTH-005.

## 14. Ejecución y cierre técnico autorizado de T-AUTH-005

<a id="e-t-auth-005"></a>

### E(T-AUTH-005)

#### Diagnóstico, autorización y trazabilidad

La inspección previa de T-AUTH-005, del **2026-11-04**, concluyó **REQUIERE CORRECCIÓN**: .dockerignore reintroducía metadatos Git y no cubría variantes locales sensibles; el primer restore Docker precedía a la copia de global.json/lockfile; el Web SDK incluía JSON del proyecto para output/publish sin exclusiones locales propias. Aquella inspección no modificó archivos ni ejecutó Docker/publish; la propuesta conversacional no se presentó como evidencia persistida o tarea cerrada.

El responsable autoriza reanudar exclusivamente T-AUTH-005 en BUILD, corregir solo **.dockerignore**, **Enver.API/Dockerfile** y **Enver.API/Enver.API.csproj**, y validar estáticamente, evaluar MSBuild con material sintético, restaurar en modo bloqueado y compilar localmente. Solo después de éxito técnico se autoriza registrar el cierre en este documento y memoria. **No se autoriza ni ejecuta Docker o publish real**.

| Campo | Evidencia |
| --- | --- |
| Fecha de ejecución | **2026-11-04** |
| Rama / HEAD | `feature/authentication` / `d1d29298f86e25070f7bd1835acecd25d88d3e35` |
| Trazabilidad | **AUTH-032, AUTH-034, AUTH-044, AUTH-045**; **PLD-AUTH-22**; **AC-19, AC-21** |
| Gates | **GATE-A PENDIENTE**; condición previa de GATE-H, también pendiente; los ocho gates siguen no satisfechos |
| Entorno / herramientas | Windows, repositorio local; SDK **10.0.403**, PowerShell y Git; sin instalaciones o nuevas dependencias NuGet |
| Resultado esperado / obtenido | Exclusiones preventivas y Dockerfile alineados, evaluación MSBuild/restore/build correctos / comprobaciones locales satisfactorias con límites explícitos de contexto e imagen no construidos y publish no ejecutado |
| Commit / workflow / captura | No generados; HEAD identifica la baseline anterior, no un commit de esta entrega |

No se acredita cumplimiento global AUTH/PLD/AC, cierre de foundation ni aceptación de autenticación. La tarea se cierra en el alcance local expresamente autorizado, conservando las comprobaciones reales de Docker/publish como pendientes posteriores.

#### Cambios técnicos aplicados

**.dockerignore:** se conservan las exclusiones útiles de la plantilla, incluidas .git, .vs, .vscode, bin, obj, node_modules, .env y archivos de herramientas. Se eliminan las cinco excepciones `!**/.gitignore`, `!.git/HEAD`, `!.git/config`, `!.git/packed-refs` y `!.git/refs/heads/**`; no queda ninguna regla negada que reintroduzca Git. Se añaden patrones recursivos para:

- .github, TestResults, coverage/coverage.*, archivos coverage/coveragexml/trx/log.
- .env.*, appsettings.Local.json, appsettings.*.Local.json, secrets.json y certificados/claves pfx/p12/pem/key.
- .terraform, tfstate/tfstate.*, tfvars/tfvars.json y tfplan.
- dist, build, artifacts, BenchmarkDotNet.Artifacts; .aws/.azure/.ssh/.docker/.idea.
- Preferencias IDE/user/suo, perfiles de publicación personales, temporales y archivos OS; estado local de runners/act.
- Documentación/gobierno raíz innecesarios para construir la API: memoria.md, specs/, .opencode/, .specify/, AGENTS.md y constitution.md. Se excluyen del contexto, no del repositorio.

No se excluyen global.json, Enver.API/packages.lock.json, csproj, appsettings.json o appsettings.Development.json. No se añade exclusión genérica JSON. .env.example queda fuera del contexto junto con .env.* por no existir una necesidad de build para conservarlo; su versionabilidad Git no se modifica. .terraform.lock.hcl tampoco queda excluido.

**Enver.API/Dockerfile:** se conserva la arquitectura base/build/publish/final, runtime **mcr.microsoft.com/dotnet/aspnet:10.0**, USER $APP_UID, puertos, directorios y ENTRYPOINT. La etapa build usa **mcr.microsoft.com/dotnet/sdk:10.0.403**, alineada con global.json. Antes del restore se copian global.json a /src y csproj/packages.lock.json a /src/Enver.API; el restore lleva **--locked-mode**. COPY . . se conserva después de restaurar, sujeto a .dockerignore. Build y publish conservan Release por defecto y añaden **--no-restore**. La etapa final copia exclusivamente **/app/publish** desde publish. No se introducen secretos ARG/ENV ni credenciales; EXPOSE no se interpreta como HTTPS validado. La sintaxis admite la etiqueta SDK fijada; disponibilidad, digest y contenido de las imágenes no se comprobaron mediante pull/build.

**Enver.API/Enver.API.csproj:** se añade la propiedad interna `_LocalSensitiveFilePatterns` con 14 patrones recursivos: appsettings.Local.json, appsettings.*.Local.json, secrets.json, *.pfx, *.p12, *.pem, *.key, .env, .env.*, *.tfstate, *.tfstate.*, *.tfvars, *.tfvars.json y *.tfplan. Dos actualizaciones de elementos **Content** y **None** fijan **CopyToOutputDirectory="Never"** y **CopyToPublishDirectory="Never"** para dichos patrones. No se cambia TFM, UserSecretsId, configuración Docker de proyecto ni paquetes/versiones; se conserva íntegra la baseline de T-AUTH-003, incluido RestorePackagesWithLockFile. Los appsettings base no coinciden con esas exclusiones.

#### Validaciones realmente ejecutadas

| Orden / comando o comprobación | Resultado obtenido |
| --- | --- |
| Lectura/validación XML del csproj y comparación de dependencias | XML válido; mismas tres referencias/versiones de T-AUTH-003; 14 patrones sensibles |
| Inspección estática de .dockerignore con evaluador independiente de sus patrones simples, sin Docker/Moby | **142/142 rutas esperadas excluidas**, incluidas variantes raíz/anidadas, Git y documentación; **8/8 entradas esperadas disponibles**; cero negaciones; no se crean archivos para esta matriz |
| Inspección estática de Dockerfile | SDK alineado, tres COPY anteriores al restore bloqueado, COPY amplio posterior, build/publish sin restore, runtime y COPY final conservados; sin secretos ARG/ENV añadidos |
| `dotnet msbuild Enver.API/Enver.API.csproj -nologo -getItem:Content,None` | Salida 0; evaluación de **42 archivos sintéticos vacíos**, sin ejecutar targets build/publish: **42/42 con Never para output y publish**, 22 Content y 20 None |
| Limpieza controlada de las pruebas sintéticas | Los 42 archivos propios y todos sus directorios propios retirados; sin archivos sintéticos persistidos ni secretos reales |
| `dotnet build Enver.API/Enver.API.csproj --no-restore` | Salida 0; **0 advertencias, 0 errores**; Debug/net10.0/Enver.API.dll; 10,29 s |
| `dotnet restore Enver.API/Enver.API.csproj --locked-mode` desde raíz | Salida 0; informa que todos los proyectos están actualizados para la restauración; lockfile inalterado |
| `dotnet build Enver.API/Enver.API.csproj --no-restore` después del restore bloqueado | Salida 0; **0 advertencias, 0 errores**; Debug/net10.0/Enver.API.dll; 2,98 s |
| `dotnet --version` | **10.0.403** |

Las ocho rutas disponibles comprobadas estáticamente fueron global.json, Enver.API/packages.lock.json, Enver.API/Enver.API.csproj, Enver.API/appsettings.json, Enver.API/appsettings.Development.json, Enver.API/Program.cs, Enver.Backend.slnx y .terraform.lock.hcl. La matriz de exclusiones cubrió nombres sintéticos Git, IDE, build/test, entorno, configuración local, certificados, Terraform y temporales, tanto en raíz como anidados; **no es un empaquetado real del contexto ni ejecución del matcher oficial de Docker**.

Para MSBuild se crearon 14 nombres sensibles por ubicación: un subárbol único de proyecto, otro nivel anidado y un subárbol único bajo wwwroot. Los nombres fueron appsettings.Local.json, appsettings.Development.Local.json, secrets.json, probe.pfx/p12/pem/key, .env, .env.local, terraform.tfstate, terraform.tfstate.backup, probe.tfvars, probe.tfvars.json y probe.tfplan. Se usaron archivos de cero bytes, se evaluaron sus metadatos y se retiraron en finally antes de los builds. No se construyó ni publicó con esos archivos. Los dos appsettings base conservaron **PreserveNewest** para output/publish; packages.lock.json permaneció fuera de Content/None según el SDK.

global.json conserva SHA-256 `0A568475B41B131EF308328AE8EED9DF85C42C776505089E3153C0A13220B6FB`; packages.lock.json conserva `6B6E8F6A214D7850EDB9BD00A19CA83FDF0321B27606CF929F6561A218562EBC`. No se edita o regenera manualmente el lockfile. El restore fue bloqueado, no forzado con caché vacía; su éxito no implica un nuevo análisis de vulnerabilidades o identidad binaria bit a bit. Los builds escriben bin/obj locales ignorados; no son builds Release/Linux ni pruebas de imagen. Una comprobación auxiliar inicial de hashes falló porque Get-FileHash no estaba disponible en la sesión; se sustituyó por SHA-256 de .NET, sin cambios en las entradas protegidas.

Antes de registrar este cierre se verificó que task.md y memoria.md seguían intactos respecto al inicio; los cinco cambios/archivos preexistentes de T-AUTH-003 y cierres anteriores no se descartaron. Solo tras los resultados correctos anteriores se modifican estos dos documentos.

#### Riesgos posteriores y límites

| Riesgo heredado | Estado posterior en esta evidencia | Fundamento y pendiente |
| --- | --- | --- |
| **R-006** | **REDUCIDO** | Se corrigen las brechas conocidas de .dockerignore y las excepciones Git; 142 exclusiones, 8 entradas disponibles y Dockerfile comprobados estáticamente. No se ejecutó el builder/matcher oficial, inspección de contexto real, pull o análisis de capas; no se certifica imagen/contexto efectivo |
| **R-007** | **REDUCIDO — NO RESUELTO** | Exclusiones preventivas MSBuild añadidas y metadatos verificados con 42 archivos vacíos; sigue pendiente ejecutar y verificar un **publish real con sentinelas controladas** bajo autorización posterior |

No se reescriben los estados históricos de E(T-AUTH-001..004), ni se resuelve R-008 por estas correcciones. .gitignore no sustituye .dockerignore o MSBuild. La actualización de metadatos evaluada no certifica la salida efectiva de todos los targets de publish/static web assets, imports o inclusiones futuras; la revisión posterior deberá inspeccionar los artefactos reales. Tampoco se inspeccionan User Secrets ni almacenes externos. No se acredita HTTPS, runtime no privilegiado efectivo, seguridad de bases, ausencia universal de secretos o cumplimiento integral AC-21.

#### Git, archivos y cierre de alcance

Se conserva rama/HEAD y no se consulta nuevamente el remoto. Al inicio estaban modificados Enver.API.csproj, memoria.md y task.md, y sin seguimiento global.json/packages.lock.json. Esta entrega interviene únicamente **.dockerignore**, **Enver.API/Dockerfile**, **Enver.API/Enver.API.csproj**, y, después del éxito técnico, **specs/002-authentication/task.md** y **memoria.md**. Los otros 28 archivos versionados/nuevos no ignorados, incluidos global.json/lockfile, conservan contenido; spec/plan, constituciones, foundation, Program.cs, appsettings, .gitignore y workflows no se modifican.

Estado Git final observado, sin staged, commit o push:

```text
## feature/authentication...origin/feature/authentication
 M .dockerignore
 M Enver.API/Dockerfile
 M Enver.API/Enver.API.csproj
 M memoria.md
 M specs/002-authentication/task.md
?? Enver.API/packages.lock.json
?? global.json
```

La validación final mediante **git diff --check** resulta sin errores de whitespace, con advertencias LF→CRLF en las cinco rutas modificadas; **git status** confirma el listado anterior y ningún staged. La comprobación de hashes/estructura confirma conservación de los 28 archivos protegidos, historia de memoria, fichas/evidencias 001–004 y bloque 006–matrices/gates; solo 005 cambia de estado. **T-AUTH-005 — COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA** en el alcance autorizado sin Docker/publish. 001–004 conservan sus cierres; **T-AUTH-006..079 (74 tareas) siguen PENDIENTE / NO EJECUTADA**. Los **8 gates siguen PENDIENTES / NO SATISFECHOS**, incluido **GATE-A**. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin autenticación implementada, instalaciones, tests/scanners, ejecución de API, conexiones MongoDB/Redis/AWS, Docker, publish real, workflows, commit o push. Se detiene el trabajo sin iniciar T-AUTH-006.

## 15. Consolidación de estructura y checkpoint Git de T-AUTH-006

<a id="e-t-auth-006"></a>

### E(T-AUTH-006)

#### Definición, autorización y fuentes

**T-AUTH-006 — Consolidar estructura y checkpoint técnico**, F0 / GATE-A, tiene por objetivo delimitar responsabilidades y una baseline limpia para incrementos posteriores; depende de **003–005**. Su salida exige un API/tres proyectos futuros de tests, dependencias interiores y edición serializada definidas, además de working tree limpio tras commits expresamente autorizados. La ficha no exige crear componentes funcionales, proyectos vacíos o un commit por tarea.

El responsable autoriza BUILD CONTROLADO para resolver ownership y checkpoint mediante **un único commit**, sin push. La aclaración posterior permite incluir también los cierres históricos 001/002 que seguían sin versionar, **sin reescribir evidencia, trazabilidad o estados**. Este registro se prepara antes del commit para no ensuciar después el checkpoint; la declaración de cierre solo es efectiva si se verifican las condiciones Git finales descritas abajo. No se redefine la tarea con el título/trazabilidad incorrectos de la solicitud descartada.

| Campo | Evidencia / fuente |
| --- | --- |
| Fecha / entorno | **2026-11-04**; repositorio local Windows, Git **2.55.0.windows.3**; sin consulta nueva al remoto |
| Rama / HEAD previo | `feature/authentication` / `d1d29298f86e25070f7bd1835acecd25d88d3e35` |
| Trazabilidad oficial | **AUTH-001, AUTH-002, AUTH-035, AUTH-041, AUTH-047, AUTH-048**; **PLD-AUTH-01, PLD-AUTH-02, PLD-AUTH-20, PLD-AUTH-25**; **AC-21** |
| Fuentes | Ficha 006 y task §6; plan §§3–4, 16, 19; AGENTS y memoria; E(T-AUTH-001..005) |
| Resultado de la inspección previa | Estructura suficiente; **REQUIERE CORRECCIÓN / CONSOLIDACIÓN** por ownership explícito y checkpoint Git pendientes |
| Cambios propios de 006 | Solo este documento y memoria §22: ownership, registro del checkpoint y estado; sin código/configuración nuevos |
| Identificador del checkpoint | El commit que incorpora esta sección, hijo del HEAD previo, con mensaje `chore(auth): consolidate authentication baseline checkpoint`; su SHA se obtiene y comunica después del commit, no se inventa ni se inserta recursivamente en su propio contenido |

AUTH-001/047/048 aportan separación de autorizaciones, trazabilidad, historia y límites; AUTH-002 preserva una identidad base sin adelantar repartidor; AUTH-035 conserva autenticación distinta de permisos; AUTH-041 exige mantener lógica aislable para pruebas posteriores. No se declara implementación funcional ni satisfacción global de esos requisitos o AC-21. PLD-AUTH-01/02 determinan proyecto único y dependencias interiores; PLD-AUTH-20 reserva tres suites futuras; PLD-AUTH-25 mantiene incrementos autorizados individualmente.

#### Ownership e integración por responsabilidad técnica

Se reutilizan las áreas y dependencias del plan §3 y la serialización de task §6. No existía una asignación explícita por archivo: se fija aquí por **roles técnicos**, sin inventar personas, crear CODEOWNERS, agentes o permisos adicionales. El **integrador técnico del backend** es el rol responsable de la revisión/consolidación final de todos los archivos compartidos; para este checkpoint lo ejerce el ejecutor expresamente autorizado, bajo las aprobaciones del responsable del proyecto. El ejecutor de cada incremento posterior asume el rol de su componente solo durante el alcance autorizado; el rol no habilita por sí mismo su ejecución.

| Archivo/componente | Rol propietario técnico | Incrementos que pueden proponer cambios, con autorización propia | Integración final |
| --- | --- | --- | --- |
| `Enver.API/Program.cs` | Composición/pipeline del backend | **056 → 057 → 058**: base HTTP, dependencias/admisión y autenticación/controllers, respectivamente; 059 solo ante ajuste de integración expresamente autorizado | Integrador técnico del backend; edición serializada, sin lógica funcional en Program |
| `Enver.API/Enver.API.csproj` | Baseline SDK/dependencias y protección de artefactos | 003/005 ya ejecutadas; 016, 023, 038 u otra tarea cuyo alcance aprobado requiera modificar el proyecto; sin paquetes funcionales incorporados por 006 | Integrador técnico del backend; serializar cambios de proyecto/grafo |
| `Enver.API/packages.lock.json` y lockfiles futuros | Grafo reproducible del proyecto correspondiente | Solo el incremento autorizado que cambie sus dependencias/entradas; generar mediante restore y verificar locked-mode, nunca editar manualmente | Integrador técnico del backend; coordinar con csproj/SDK y verificar diff |
| `Enver.Backend.slnx` | Estructura de solución y pruebas | 013 incorpora los tres proyectos de tests; otros cambios solo con alcance explícito, sin regenerar la solución | Integrador técnico del backend; edición serializada |
| Futuro `Features/Authentication/Application/AuthenticationService.cs` | Aplicación de autenticación y puertos | 034/035 registro; 043 logout; 047–050 login según responsabilidad; 032 solo si su coordinación corresponde realmente al servicio conforme ficha | Integrador técnico del backend; conservar dependencias/orden del catálogo, sin ediciones concurrentes |
| Futuro `Controllers/AuthenticationController.cs` | Transporte HTTP y proyección de contratos | 035 registro, 044 logout, 049 login; integración final del host en 058 | Integrador técnico del backend; endpoints incorporados de forma serializada |
| Futuros `AuthenticationInputs.cs` / `AuthenticationResults.cs` | Contratos interiores de aplicación | 034 registro, 043 logout y 047 login; otros consumidores solo dentro de su alcance aprobado | Integrador técnico del backend; revisar conjuntamente productores/consumidores |
| Futuro `tests/Shared/*` | Pruebas/fixtures y aislamiento de entornos | 014 utilidades, 015 factories/fixtures y tareas de pruebas posteriores que necesiten extenderlas bajo autorización | Integrador técnico del backend; serializar cambios comunes y conservar aislamiento |
| Futuro `.github/workflows/ci.yml` | DevSecOps/CI | 070 build/unit, 071 integration/security, 072 SAST/auditoría y 073 secret/container scanning, conforme cada ficha | Integrador técnico del backend; integración serializada; sin workflow creado por 006 |

Los IDs anteriores describen ownership de los incrementos previstos, **no amplían archivos ni alteran dependencias, gates o autorizaciones del catálogo**. Antes de editar un archivo compartido, el ejecutor declara tarea/rol/ruta, coordina un único turno con el integrador y revisa el diff acumulado; el integrador valida coherencia con el incremento anterior antes de aceptar el siguiente. Compartir responsabilidad funcional no permite ediciones concurrentes del mismo archivo. Cambios adicionales requieren autorización delimitada; no se crean ahora ninguno de los componentes futuros.

#### Mapa y baseline consolidados sin materialización anticipada

Se observa **Enver.Backend.slnx → Enver.API/Enver.API.csproj**, único proyecto de aplicación **net10.0**, Controllers vacío, configuración base, host inicial con OpenAPI solo en Development y /health básico, sin readiness MongoDB/Redis. El mapa futuro del plan conserva Controllers/Contracts → Application → Core/Ports; Infrastructure/Security implementan puertos y Composition conecta implementaciones. Configuration, Http y Observability mantienen sus responsabilidades transversales, sin proyectos extra de aplicación.

**tests/Enver.API.UnitTests**, **tests/Enver.API.IntegrationTests** y **tests/Enver.API.SecurityTests** son las tres ubicaciones futuras; tests/Shared comparte fuentes sin cuarto proyecto. No existen todavía esos proyectos ni tests/; su creación pertenece a **013**, utilidades/dependencias a **014** y factories/fixtures a **015**. Options/HTTP pertenecen a 007–012; Program se integra después en 056–058. Ninguna ausencia de esas capacidades futuras se convierte en bloqueo artificial de 006.

E001 conserva alcance y baseline inicial; E002 evaluación/grafo/licencias/riesgos; E003 fija SDK **10.0.403**, lockfile y restore reproducible; E004 registra higiene Git y revisión sensible acotada; E005 protege contexto/publish hasta su nivel actual. La lectura confirma global.json exacto, RestorePackagesWithLockFile, tres paquetes baseline (Microsoft.AspNetCore.OpenApi **10.0.14**, Microsoft.OpenApi **2.13.0**, Container Tools **1.23.0**) y ausencia de nuevas referencias funcionales MongoDB/Redis/JWT. Dockerfile conserva SDK alineado, restore bloqueado y build/publish sin restores implícitos. No se detecta inconsistencia que justifique repetir comandos .NET.

Se reutilizan **como evidencia anterior, no como ejecuciones de 006**, el restore bloqueado y los builds Debug/net10.0 con **0 advertencias y 0 errores** de E005, las 44 pruebas de higiene de E004 y las 142 exclusiones/8 entradas y evaluación de 42 archivos sintéticos de E005. No se repiten restore/build, scanners, pruebas, Docker o publish; no se certifica runtime/imagen, publicación efectiva, seguridad funcional o ausencia universal de secretos por este checkpoint.

#### Estado Git, alcance exacto y protocolo del único commit

El estado inicial contiene cinco rutas modificadas y dos sin seguimiento: .dockerignore, Enver.API/Dockerfile, Enver.API/Enver.API.csproj, memoria.md, specs/002-authentication/task.md, Enver.API/packages.lock.json y global.json; no había staged. Se revisó que el diff técnico acumulado corresponde a 003/005, y que task/memoria conservan los cierres 001–005; los cierres 001/002 aún no estaban en HEAD. El responsable autoriza expresamente incluirlos intactos para no descartarlos ni dejar el árbol sucio.

**Únicas siete rutas del commit:**

```text
.dockerignore
Enver.API/Dockerfile
Enver.API/Enver.API.csproj
Enver.API/packages.lock.json
global.json
specs/002-authentication/task.md
memoria.md
```

Antes de commit se revisan git status, git diff --check y diff del índice limitado a esas siete rutas; la validación documental comprueba 79 fichas, conservación de las fichas/evidencias 001–005 y del historial de memoria, 007–079 pendientes, ocho gates pendientes y archivos protegidos intactos. Las lecturas/checks previos no informan errores de whitespace, solo advertencias LF→CRLF. Se versiona la baseline acumulada en un checkpoint, no un commit por tarea; no hay descartes, reset, checkout, clean, reescritura de historial ni archivos ajenos incluidos.

**Verificación posterior obligatoria, no adelantada como resultado al redactar:** ejecutar `git status`, `git log -1 --oneline`, `git rev-parse HEAD` y comprobar que no haya modificaciones, staged o archivos no versionados no ignorados. El commit debe tener como padre el HEAD previo, el mensaje autorizado y exactamente las siete rutas anteriores. El SHA y resultado efectivo se comunicarán en la entrega posterior al commit. Si falla el commit o el estado final no está limpio, **no se declara completada 006**, se informa la causa y no se crea otro commit/amend sin autorización. No se modifica este registro después del commit exitoso.

#### Continuidad y riesgos sin cambios

| Riesgo | Estado conservado | Límite |
| --- | --- | --- |
| **R-006** | **REDUCIDO** | Contexto/builder e imagen reales no verificados |
| **R-007** | **REDUCIDO — NO RESUELTO** | Publish real con sentinelas pendiente de autorización/verificación |
| **R-008** | **REDUCIDO — NO RESUELTO** | Historial/ignorados/binarios/artefactos/almacenes externos fuera de auditoría exhaustiva; árbol limpio no prueba ausencia de secretos |

Tras commit correcto y verificación limpia, **T-AUTH-006 — COMPLETADA / EVIDENCIA TÉCNICA Y CHECKPOINT GIT REGISTRADOS**. Se completa únicamente **F0 — Baseline de 002**, sin reformar la Fase 0 documental original ni cerrar foundation retrospectivamente. **GATE-A PENDIENTE**: requiere además 007–015 y sus verificaciones según task §4; los otros siete gates siguen pendientes. **T-AUTH-007..079 (73 tareas) PENDIENTE / NO EJECUTADA**. **AUTH-027 — NO IMPLEMENTADO / NO SATISFECHO**. Sin autenticación, dependencias nuevas, carpetas/proyectos ficticios, instalaciones, conexiones MongoDB/Redis/AWS, Docker, publish real o push. Trabajo detenido sin iniciar 007.

## 16. Evidencia técnica de T-AUTH-007

<a id="e-t-auth-007"></a>

### E(T-AUTH-007)

El responsable autoriza exclusivamente 007/008 en BUILD CONTROLADO el **2026-11-04**, con estado inicial limpio en `feature/authentication`, HEAD **a4e5cc2a5dc9a36e8b3f383af05389799d1aa559** (checkpoint F0), sin consulta nueva al remoto. La implementación de configuración y los comandos técnicos preceden a este registro. No se alteran título, fase/gate, dependencias o relaciones de las fichas.

| Campo | Evidencia real |
| --- | --- |
| Trazabilidad oficial 007 | AUTH-015/025/026/027/034/038; PLD-AUTH-04/05/07/08; AC-14/15/17/20; depende de 006; F1 / GATE-A |
| Archivos nuevos 007 | `Enver.API/Configuration/MongoDbOptions.cs`, `Enver.API/Configuration/RedisOptions.cs` |
| Archivos existentes | `Enver.API/appsettings.json`, `Enver.API/appsettings.Development.json`; después de éxito técnico, este documento y memoria §23 |
| MongoDB declarado | Base por ambiente, colección `users`, operación/conexión 2000 ms, candidatos write/read concern `majority` y preferencia `primary`; sin cliente, topología, journaling o política retry efectiva |
| Redis declarado | Prefijo por ambiente, propósitos `auth:revocation`/`auth:ratelimit`, operación 750 ms, conexión 2000 ms, margen candidato 60 s, admisión fail-closed y requisito futuro `noeviction`; no un TTL fijo ni configuración de servidor |
| Aislamiento local | Convención explícita de implementación: base `enver_<ambiente>` con sufijo opcional `_...` para ejecución; namespace `enver:<ambiente>`, ambos cotejados contra IHostEnvironment. Development declara `enver_development` y `enver:development`; la base común deja ambos vacíos para exigir selección externa por ambiente |
| Secretos | ConnectionString existe solo como propiedad inicialmente vacía; se omite de ambos JSON y debe suministrarse por User Secrets/variables. No se abrió el almacén real, no se mostraron conexiones ni credenciales |
| Validadores | MongoDbOptionsValidator/RedisOptionsValidator en el único archivo autorizado `AuthenticationOptionsValidation.cs`; presencia de conexión, ambiente, colección/propósitos, timeouts, candidatos, margen y requisitos seguros |
| Herramientas/entorno | Windows local; SDK fijado 10.0.403, net10.0, compilador del SDK y referencias/runtime 10.0.14; PowerShell 5.1/.NET Framework 4.8 para orquestación; sin infraestructura externa |
| Restore | `dotnet restore Enver.API/Enver.API.csproj --locked-mode`: correcto, restaurado en 899 ms; grafo sin paquetes nuevos |
| Build | `dotnet build Enver.API/Enver.API.csproj --no-restore`: correcto en 11,36 s; **0 advertencias / 0 errores** |
| Comprobación compartida 007/008 | **71/71 casos** en DI aislada con IStartupValidator; detalle en §17. No se arrancó la API ni un host, no se instanciaron clientes MongoDB/Redis |
| Inspección JSON/grafo | Cinco secciones presentes, siete cuotas coincidentes 7/7, conexiones/claves ausentes de JSON, cardinalidad null; packages.lock.json conserva SHA-256 `6B6E8F6A214D7850EDB9BD00A19CA83FDF0321B27606CF929F6561A218562EBC` |
| Commit/workflow/captura | No generados; HEAD conserva el checkpoint F0; cambios sin commit |

La convención de nombres no demuestra separación efectiva de permisos/datos, disponibilidad o seguridad de transporte. Los concerns son candidatos conforme al plan §7, no selección final de topología/ACK. `noeviction` es un requisito declarado, no verificado; el margen 60 s sigue candidato de calibración. TLS/ACL/conexiones efectivas se validarán con el cliente y entorno autorizados. No se acredita AUTH-027 ni garantías Redis por declarar Options.

**Casos reservados para F2 y regresión 060:** sección ausente individualmente; conexión ausente; base/namespace de otro ambiente; colección/propósitos incorrectos; timeouts cero/negativos o superiores al presupuesto; concerns candidatos incompatibles; margen inválido; fail-closed desactivado; política de expulsión incompatible; aislamiento de dos ejecuciones sin tocar datos ajenos. Los casos ejecutados en la comprobación temporal no se presentan como suites unitarias/integración existentes.

**Estado final 007: COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, para su salida de declaración de configuración, sin inferir integración ni aceptación operativa. La incorporación al host y suites siguen pendientes; 008 conserva la limitación específica de startup.

## 17. Evidencia parcial de T-AUTH-008

<a id="e-t-auth-008"></a>

### E(T-AUTH-008)

Misma autorización, baseline, entorno y restore/build compartidos de §16, ejecutados **una vez** en este bloque. Trazabilidad oficial: **AUTH-005/006/007/008/019/020/029/034/038; PLD-AUTH-04/10/13/15/18; AC-04/10/16/18/20**; depende de 007; F1 / GATE-A. No se declara satisfacción global de esos requisitos o AC.

**Archivos nuevos:** `Enver.API/Configuration/JwtOptions.cs`, `AuthenticationOptions.cs`, `RateLimitingOptions.cs`, `AuthenticationOptionsValidation.cs`. Los dos appsettings comparten alcance con 007. Ningún archivo adicional de aplicación/pruebas, dependencia o proyecto creado.

### Reglas implementadas y límites

- **JWT:** HS256, issuer `enver-api`, audience `enver-mobile`, vida 900 s, skew obligatorio 0–30 s; conjunto local no vacío, kids presentes/únicos y activo perteneciente al conjunto; Base64 estándar canónico y al menos 32 bytes decodificados. No firma, parseo de JWT real, generación de claves ni prueba de entropía. Los buffers decodificados de la comprobación estructural se limpian; las Options no se imprimen.
- **Authentication:** dominio `uttt.edu.mx`; límites iniciales del spec: nombre 200 escalares, email 254/64 bytes ASCII, creación de password 12–128 escalares/512 bytes, cuerpo 16384 bytes, JSON profundidad 4, JWT 4096 bytes. La mínima de creación permanece separada conceptualmente de verificación de login. Cambiar estos valores fuera de la política inicial requiere decisión posterior, no defaults silenciosos.
- **Presupuestos:** candidatos register/login 8000 ms, logout 4000 ms y concurrencia de hashing 2, configurables positivos; los timeouts de dependencias no pueden exceder el total mínimo. No se ejecuta cancelación, hashing ni presupuesto restante (009); no se afirma SLA o calibración.
- **RateLimiting:** siete estructuras configurables exactamente por los ámbitos del spec, con máximo/ventana positivos: REGISTER email 3/900 s y origen 20/900 s; LOGIN origen-email 5/60 s, email 10/60 s, origen 60/60 s; LOGOUT cuenta 30/60 s y origen 300/60 s. No algoritmo Redis, buckets, middleware ni consumo.
- **Política/HMAC:** etiqueta local no sensible `academic-initial`; no protocolo de transición aprobado. Clave HMAC externa en Base64 canónico no vacío, sin inventar tamaño mínimo criptográfico no fijado para HMAC; se rechaza reutilización de una clave JWT configurada. La exclusividad real por ambiente y calidad/entropía deben acreditarse posteriormente. `MaxActivePartitions = null` conserva explícitamente la puerta de cardinalidad; si se proporciona un valor, solo se comprueba positividad, no aprobación del tope. No habilita construir cuotas con topes sin resolver.
- **Registro preparado:** método `AddValidatedAuthenticationOptions(IServiceCollection, IConfiguration, IHostEnvironment)` en el archivo autorizado. Cinco implementaciones pequeñas de IValidateOptions, una por tipo, para separar responsabilidades; secciones obligatorias mediante GetRequiredSection, cinco llamadas ValidateOnStart. No hace falta crear cinco archivos adicionales. Errores fijos sin interpolar valores sensibles.
- **Sin recarga automática:** Configure/Bind al crear Options, sin suscribir cambios; el consumo previsto es IOptions, no una recarga independiente por instancia. Esto no demuestra coordinación de un cambio/reinicio entre instancias ni sustituye las puertas de rotación/política.

### Comprobación técnica temporal real, no suite F2

Se compiló un ejecutable temporal mediante `dotnet exec <SDK 10.0.403>/Roslyn/bincore/csc.dll @arguments`, contra la DLL ya construida y las referencias instaladas net10.0/ASP.NET Core 10.0.14; sin csproj, NuGet, tests/ ni cambios de solución. Se ejecutó con `dotnet exec <temporal>/auth-options-007008-20261104.dll`. El código construía IConfiguration desde los dos JSON autorizados más un proveedor **en memoria**, ServiceCollection y un IHostEnvironment sintético Development. Nunca usó configuración predeterminada de host, User Secrets reales, variables reales o servicios de red. Las cadenas de conexión sintéticas no eran conexiones válidas; el material de claves consistía en bytes repetidos creados solo en memoria, deliberadamente sin atribuir entropía.

**Resultado: Controlled DI checks: 71/71.** Se invocó IStartupValidator de ese contenedor aislado, no el startup de Enver.API. Se comprobaron configuración sintética válida, rechazo de base sin secretos/secciones ausentes, conexiones vacías, aislamiento, colección/concerns/propósitos, timeouts/margen/admisión, algoritmo/emisor/audiencia/vida/skew, kids ausentes/desconocidos/duplicados, Base64 inválido/no canónico/corto, límites/presupuestos/concurrencia, máximo/ventana inválidos en cada una de las siete cuotas, revisión/cardinalidad/HMAC inválidos y reutilización HMAC-JWT. Casos positivos adicionales: skew cero, base con sufijo de ejecución y ausencia de invalidación automática de IOptionsMonitor tras Reload. No se imprimieron valores de claves/conexiones.

Los primeros intentos de invocar el compilador temporal fallaron por longitud de línea de comandos y construcción del argumento /out en PowerShell; se corrigió la invocación con referencias específicas y argumentos delimitados, sin modificar la implementación de aplicación. La compilación/ejecución final del probe fue correcta. Una comprobación de hash falló porque Get-FileHash no estaba disponible; se repitió con SHA256 de .NET y confirmó el lockfile intacto. No son errores/warnings del build del proyecto.

Los **cuatro artefactos temporales** (fuente, runtimeconfig, ejecutable y copia de Enver.API.dll) en el directorio temporal aprobado se eliminaron y se comprobó **4/4 ausentes**. No se conservaron valores sintéticos ni se crearon suites unitarias/integración; el probe descartable no sustituye casos mantenibles y repetibles de F2/060.

### Casos que debe materializar F2

1. Cada sección/valor obligatorio ausente por separado; fuentes externas sintéticas sin volcado y mensajes sin reflejar datos.
2. Matriz MongoDB/Redis e aislamiento de §16; presupuestos negativos/cero/incoherentes y fronteras con dependencias válidas.
3. JWT: HS256/issuer/audience exactos; vida distinta de 900; skew ausente/negativo/0/30/31; kid vacío/desconocido/duplicado; clave ausente, Base64 malformado/no canónico y 31/32 bytes. Aceptar longitud no debe acreditarse como entropía.
4. Límites iniciales y distinción creación/verificación de password; errores con datos de configuración inválidos sin secretos.
5. Las siete cuotas: ausencia individual, máximo/ventana cero/negativos, valores positivos; cardinalidad pendiente no convertida en tope aprobado; clave HMAC ausente/malformada/reutilizada y revisión de política ausente.
6. ValidateOnStart en factory/host de prueba, pruebas de no admisión del host real al integrarlo y regresión 060. La comprobación DI actual no demuestra este resultado en el host real.

**Estado final 008: IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE.** Program.cs no se modificó ni se invocó el método de registro desde la aplicación: **PREPARADO / NO INTEGRADO EN HOST**. No se afirma que Enver.API rechace hoy una configuración inválida antes de admitir tráfico. La integración continúa reservada a 056 → 057 → 058 bajo autorización propia; F2 y 060 pendientes.

Estado agregado: **7 tareas completadas (001–007), 1 preparada no completada (008), 71 pendientes/no ejecutadas (009–079)**. Fichas 009–079 y E001–E006/historial preservados. **8 gates pendientes**, incluido **GATE-A**; cobertura documental AUTH 48/48, PLD-AUTH 25/25, AC 21/21 sin acreditar cumplimiento técnico. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO**. R-006 REDUCIDO; R-007 y R-008 REDUCIDO — NO RESUELTO. Sin API ejecutada, conexiones MongoDB/Redis, servicios/endpoints funcionales, F2, dependencias nuevas, Docker/publish, commit o push. Trabajo detenido sin iniciar 009.

La comparación final contra HEAD leyendo Git explícitamente como UTF-8 confirmó **71/71 fichas 009–079 intactas**, secciones E001–E006 idénticas e historial previo de memoria conservado como prefijo. El primer intento de comparación por salida de PowerShell tuvo diferencias de decodificación, no cambios de esas fichas; se corrigió la lectura. El inventario Git confirma exactamente seis archivos nuevos Configuration y cuatro modificados autorizados, sin staged ni cambios en archivos protegidos. Las advertencias Git LF→CRLF son de normalización, distintas de los 0 warnings del build; el diff/check y status finales se comunican al cerrar la entrega.

## 18. Evidencia técnica de T-AUTH-009

<a id="e-t-auth-009"></a>

### E(T-AUTH-009)

Autorización conversacional BUILD CONTROLADO del **2026-11-04**, exclusivamente 009 → 010 → 011 → 012, en secuencia y con parada si no se verifica el componente anterior. Inicio en feature/authentication, HEAD **a4e5cc2a5dc9a36e8b3f383af05389799d1aa559**; se preservan los diez archivos cambiados/no versionados válidos de 007/008. Sin nueva consulta al remoto. La revisión anterior permite preparar consumidores de 008 sin acreditar startup real; 008 mantiene **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN DE STARTUP PENDIENTE**. No se altera su estado ni se adelanta Program.

| Campo | Evidencia real 009 |
| --- | --- |
| Trazabilidad | AUTH-008/026/038/040; PLD-AUTH-18; AC-14/17/18; depende de 007/008; F1 / GATE-A, transversal |
| Archivo nuevo | `Enver.API/Http/RequestBudget.cs`; tras verificación, este documento y memoria §24 |
| Implementación | RequestBudget/DependencyBudget con TimeProvider, tiempo monotónico, saldo no negativo, CTS enlazados y ámbito de dependencia limitado por el saldo; sin colas/reintentos ni operaciones de infraestructura |
| Configuración consumida | AuthenticationOptions para 8000/8000/4000 ms; BeginDependency acepta los candidatos de Options Mongo/Redis (2000/750 ms) y conexión (2000 ms); ningún valor se declara SLA ni se duplica como constante funcional |
| Semántica | Client, RequestBudget y DependencyTimeout diferenciados; prioridad determinista cliente → total → dependencia si coinciden señales, sin afirmar cronología. MarkDispatched antes de enviar conserva ResultUncertain ante cancelación/timeout; el llamador no debe inferir ausencia de efecto ni reintentar ciegamente |
| Restore | `dotnet restore Enver.API/Enver.API.csproj --locked-mode`: correcto, proyecto actualizado |
| Build | `dotnet build Enver.API/Enver.API.csproj --no-restore`: correcto, **0 advertencias / 0 errores**, 11,27 s |
| Entorno/herramientas | Windows local, SDK 10.0.403, net10.0; probe compilado con csc del SDK contra referencias ASP.NET/Core 10.0.14; sin proyecto de pruebas/NuGet nuevo |
| Comprobación real | **19/19 aserciones aisladas**, tiempo manual sin sleeps: candidatos por operación, cero/negativo rechazado, saldo/saturación a cero, clamp al saldo, tokens enlazados, expiración, prioridad, fallo tipado, incertidumbre antes/después del envío y cancelación del cliente. Ningún host/API iniciado |
| Comando probe | Script temporal Run.ps1: `dotnet exec .../Roslyn/bincore/csc.dll @Compile.rsp`, seguido de `dotnet exec .../Probe.dll`; sin credenciales ni conexiones |
| Resultado/estado | **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, para auxiliares aislables; no demuestra presupuesto efectivo en pipeline/operaciones reales |

**Casos F2 pendientes:** trasladar estas fronteras a UT mantenibles, cancelación simultánea, disposición, presupuesto restante entre fases, timeout con efecto incierto y clasificación segura; regresión 060/calibración 067 posteriores. Cancelar no interrumpe un PasswordHasher síncrono ni revierte efectos remotos. Los artefactos del probe se mantienen solo durante esta ejecución secuencial y deben eliminarse al cierre. No se crearon tests/, endpoints o clientes; Program intacto, sin dependencias nuevas, commit/push. GATE-A y los otros siete gates pendientes; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. Se verifica 009 antes de iniciar 010.

## 19. Evidencia técnica de T-AUTH-010

<a id="e-t-auth-010"></a>

### E(T-AUTH-010)

Misma autorización, baseline y entorno de §18; se inicia solo después de verificar 009. **Trazabilidad oficial:** AUTH-011/023/026/031/032/036/037/038; PLD-AUTH-14/19/23; AC-07/13/14/19; depende de 009, F1 / GATE-A transversal.

| Campo | Evidencia real 010 |
| --- | --- |
| Nuevos archivos | `Enver.API/Http/ApiProblemDetails.cs`, `ApiExceptionHandler.cs`, `RequestContextMiddleware.cs`; después, este registro y memoria §25 |
| Contrato | RFC 9457, about:blank/title/status/detail/code/traceId, sin instance, errors solo para 400 y por campos permitidos; mensajes constantes, nunca valores suministrados. Los ocho estados/códigos del plan §12 se preservan |
| Writer/registro | IProblemDetailsWriter propio sobre AddProblemDetails/IProblemDetailsService nativos; AddSafeApiErrors prepara registros, sin invocarlos en Program. El writer reconstruye un problema seguro incluso si otro productor aporta detail/instance/extensions arbitrarias |
| Respuestas | 204 vacío, no-store en errores y éxitos al componer RequestContext; Bearer genérico en 401, Retry-After calculado/redondeado hacia arriba en 429 si se suministra; guardas HasStarted/cancelación y marca de escritura para evitar duplicar documento |
| Excepciones | IExceptionHandler: cliente cancelado no recibe documento; respuesta iniciada no se reescribe y se aborta; OCE/TimeoutException no cliente → 503; inesperada → 500. Nunca se serializa ni registra el texto/stack de excepción |
| Correlación | Concreción técnica local: X-Correlation-ID único de 32 hex minúsculas no todo cero; otro formato/múltiples valores genera ActivityTraceId aleatorio. TraceIdentifier y cabecera consistentes; no es identidad ni se interpreta como claims confiables |
| Build | `dotnet build Enver.API/Enver.API.csproj --no-restore`: correcto, **0 advertencias / 0 errores**, 6,26 s; reutiliza restore bloqueado de §18, sin cambio de paquetes |
| Probe ejecutado | Run.ps1/csc/dotnet exec temporales: **65/65 aserciones nuevas**; se repiten 19 de 009, acumulado **84/84**; DI nativa y DefaultHttpContext/streams de memoria, feature de respuesta iniciada controlada, sin host/API ni endpoints |
| Cobertura real | Ocho estados/códigos y forma JSON, errores condicionales, no instance ni segunda escritura, 204, challenge, Retry-After, saneamiento de productor, excepciones/cancelación, respuesta iniciada, correlación válida/inválida/múltiple/CRLF y no-store de éxito |
| Resultado/estado | **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, componentes PREPARADOS / AISLABLES / NO INTEGRADOS AL HOST |

**F2/056/058 pendientes:** materializar estos casos en UT/IT, ejecución de OnStarting y escrituras bajo servidor real, respuestas de MVC/challenge/autorización/cuotas y cancelación durante escritura. Un handler aislado no garantiza comportamiento del ExceptionHandlerMiddleware ante una respuesta ya iniciada; servidor/borde pueden rechazar antes y quedan fuera del writer. No se acredita integración ni no-store efectivo de la API actual. Sin pruebas UT/IT/ST existentes, nuevos paquetes o conexiones; gates pendientes, AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. Se verifica 010 antes de iniciar 011.

## 20. Evidencia técnica de T-AUTH-011

<a id="e-t-auth-011"></a>

### E(T-AUTH-011)

Se inicia tras verificar 010, bajo autorización/baseline de §18. **Trazabilidad oficial:** AUTH-003/008/018/023/030/035/040; PLD-AUTH-13/14/18/19; AC-05/09/13/18; dependencias 008–010; F1 / GATE-A. 008 mantiene verificación diferida, no se declara cerrado su startup.

| Campo | Evidencia real 011 |
| --- | --- |
| Nuevo archivo | `Enver.API/Http/AuthenticationRequestLimitsMiddleware.cs`, con metadata/resultado de frontera, contrato de continuación y configuración System.Text.Json; después este registro y memoria §26 |
| Alcance | Solo metadata AuthenticationRequestMetadata de operación; sin rutas/controllers/DTOs funcionales creados. Sin metadata pasa al siguiente. La composición futura debe asignarla después de routing, no se afirma protección de endpoints actuales |
| Cuerpo | Alquiler de buffer para límite + 1, lectura real hasta 16385 bytes aunque falte Content-Length, stream no seekable/chunks; longitud declarada excesiva también rechaza. Replay acotado para el consumidor, restauración del stream y devolución del buffer con limpieza; sin disco ni logging del cuerpo |
| Presupuesto | Reutiliza RequestBudget del contexto o crea uno desde Options que cubre lectura y continuación, sin reinicio por dependencia. ReadAsync usa token enlazado; cancelación cliente no escribe, agotamiento total 503. No es límite global de recepción de servidor/borde |
| JSON | application/json UTF-8 estricto, charset opcional UTF-8, Content-Encoding ausente/identity, sin descompresión; nombre exacto camelCase, objeto con strings obligatorios fullName/email/password en registro o email/password en login; desconocidas/null/tipos/ausentes rechazados sin validar aún reglas funcionales de dominio/password |
| Duplicados/Unicode | APIs .NET 10 JsonDocumentOptions/JsonSerializerOptions.AllowDuplicateProperties=false disponibles y usadas; duplicados incluso por nombre escapado rechazados, profundidad 4, sin comentarios/trailing commas/coerción; decoder UTF-8 estricto y control de UTF-16/surrogates |
| Logout/Bearer | Cuerpo no vacío (whitespace incluido) 400 dentro de límite general; vacío no exige Content-Type. Authorization única, esquema Bearer y forma compacta ASCII de tres segmentos, token ≤4096 bytes antes de criptografía. Nunca obtiene token de query/cookie/cuerpo. Sin firmas ni claims confiables |
| Register/login públicos | Bearer opcional no inicia validación/revocación ni altera contrato, conforme al plan §9; límite global de cabecera y posible 431 pertenecen al servidor/borde posterior |
| Coordinación preparada | IAuthenticationBoundaryContinuation recibe resultados admisibles/400 si se registra; sin continuación, rechazo seguro 400 por defecto. Permite a 012 ejecutar controles asíncronos antes del 400; 413/415/401/503 permanecen terminales de frontera |
| Build | `dotnet build Enver.API/Enver.API.csproj --no-restore`: correcto, **0 advertencias / 0 errores**, 5,54 s; restore/grafo de §18 conservados |
| Probe real | **149/149 aserciones nuevas**, acumulado **233/233** al repetir 009/010; streams de memoria/no seekable de 3 bytes por lectura y ReadAsync cancelable controlado, sin host/API |
| Casos verificados | JSON válido register/login, ausente/ilegible/array/null/tipos/operadores/case/unknown/missing/duplicados simples y escapados/comentarios/comas/surrogates/UTF-8 inválido/profundidad; 16384/16385 bytes y longitud declarada engañosa; media types/charset/encoding; logout vacío/whitespace/objeto; Bearer ausente/múltiple/malformado/no ASCII/4096/4097 bytes/solo canales alternativos; configuración serializer y timeout de lectura |
| Estado | **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, frontera PREPARADA / AISLABLE / NO INTEGRADA AL HOST |

**Pendientes F2/056/058:** materializar casos en UT/IT/ST y servidor de prueba, framing chunked real, límites de servidor/proxy y headers, OnStarting, deserialización MVC real con contratos futuros y continuidad asíncrona. Los chunks del probe son un stream controlado, no evidencia de Kestrel/LB. No hay normalizador funcional, hashing, clientes, JWT criptográfico ni cuotas. Gates pendientes, AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. Se verifica 011 antes de comenzar 012; la precedencia de resultados 400 y controles obligatorios se decide separadamente en 012.

## 21. Evidencia técnica y decisión aprobada de T-AUTH-012

<a id="e-t-auth-012"></a>

### E(T-AUTH-012)

Se inicia después de verificar 011, con la autorización/baseline de §18. **Trazabilidad oficial:** AUTH-008/028/029/030/031/035/040; PLD-AUTH-02/13/14/15/18/19; AC-05/14/16/18; depende de 010/011; F1 / GATE-A, entrada GATE-B. No se modifica esta trazabilidad ni las cuotas/estados aprobados.

### D012 — Precedencia y consumo, APROBADA EXPRESAMENTE

Spec/plan no imponían una precedencia única: plan §§13/15 y task §7 conservaban esta puerta antes de cuotas/MVC. Tras verificar 011, el agente presenta la matriz mínima mediante pregunta explícita. El responsable responde **«Aprobar matriz propuesta»** el **2026-11-04**, autorizando registrarla y comprobar sus componentes con colaboradores controlados, manteniendo integración real diferida a F2/058. D012 identifica esta decisión local de ejecución de 012; no crea otro AUTH/PLD ni reforma spec/plan.

| Situación/orden | Resultado | Consumo y controles |
| --- | --- | --- |
| 1. Exceso real/declarado de cuerpo | 413 | Terminal antes de parseo, preflight y cuota; lectura acotada |
| 2. Media type/charset/encoding no soportado en cuerpos funcionales | 415 | Terminal de frontera, sin cuota |
| 3. Bearer requerido ausente/ambiguo/excesivo/malformado | 401 | Terminal; nunca derivar buckets de claims no autenticados. Logout no vacío es 400 de estructura dentro del límite general; no se usa su cuerpo como token |
| 4. JSON ilegible/email no normalizable/defectos de validación, con control obligatorio no verificable | 503 | Ejecutar preflight asíncrono antes de responder 400; sin fallback local ni consultar existencia de usuario |
| 5. Control verificable, JSON ilegible o sin identidad válida para los ámbitos | 400 | Sin cuota/particiones arbitrarias ni cargo parcial de origen; no admitido a procesamiento funcional |
| 6. Identidad válida, evaluación conjunta indisponible/incierta | 503 | No continuar/reintentar; si el comando se envió, pudo haber consumo: no afirmar ausencia de efecto |
| 7. Identidad válida, algún ámbito excedido | 429 | Cero consumo según contrato del colaborador, sin prolongar penalización; Retry-After calculado cuando disponible |
| 8. Todos los ámbitos admiten, quedan defectos de campos/MVC | 400 | Consumo conjunto único: intentos admitidos consumen aunque fallen después; sin hashing/servicio funcional |
| 9. Todos admiten, sin defectos | Continuar | Un solo consumo conjunto por request; no implica éxito funcional ni emisión |

No se inventa una cuota de JSON inválido ni un bucket de email arbitrario. No se consulta usuario para decidir identidad/cuota. Para register/login, identidad debe proceder del normalizador compartido futuro antes del consumo; nombre/password inválidos no eluden cuotas si la identidad es válida. En logout, identidad de cuenta autenticada debe existir antes de cuota; una identidad criptográfica no confiable no se acepta por un flag del cliente. Los defectos múltiples se agregan mediante campos permitidos, nunca valores/mensajes del request.

### Componentes y verificación

| Campo | Evidencia real 012 |
| --- | --- |
| Nuevos archivos | `Enver.API/Features/Authentication/Validation/AuthenticationAdmissionResults.cs`, `Enver.API/Http/AuthenticationValidationCoordinator.cs`, `Enver.API/Http/AuthenticationValidationFilter.cs`; después task/memoria §27 |
| Contratos | Resultados propios de assessment/cuota/coordinación, sin DTOs funcionales o valores sensibles; tres colaboradores obligatorios para preflight, evaluación de identidad/campos y consumo conjunto. No hay adaptadores reales, fakes productivos ni fallback disponible/no-op |
| Coordinación | Continuación de frontera intercepta 400 antes de MVC. Frontera válida continúa hasta autenticación/binding y filtro MVC; preflight precede al 400, una única identidad preparada precede a cuota, no hay lectura de usuarios |
| Evitar doble cargo | Lazy<Task> compartida en Items por HttpContext, acceso protegido y resultado de control reutilizado por todas las invocaciones del request; ID de intento interno generado una vez al evaluar cuota. No idempotencia pública, entre requests, ni promesa de exactly-once distribuido |
| MVC preparado | IAsyncActionFilter/IOrderedFilter con orden -3000, anterior al ModelStateInvalidFilter nativo (-2000), sin deshabilitar globalmente validación de otros controllers. Await de controles antes de resolver ModelState; no copia RawValue/AttemptedValue/mensajes. Registro y JsonOptions preparados, no invocados desde Program |
| Fail-closed | Colaborador no verificable/timeout/incierto → 503 sin texto interno ni reintento; cancelación cliente aborta sin respuesta; falta de frontera en filtro → 503; cuenta logout no autenticada → 401 sin cuota |
| Build | `dotnet build Enver.API/Enver.API.csproj --no-restore`: correcto, **0 advertencias / 0 errores**, 4,44 s; grafo/restore de §18 sin cambios |
| Probe real | **53/53 aserciones nuevas**, acumulado **286/286** con 009/010/011 repetidas; colaboradores controlados, HttpContext/ModelState/filtros MVC nativos en memoria, TCS para espera asíncrona y ocho invocaciones concurrentes de coordinación |
| Casos ejecutados | JSON ilegible/identidad inválida y disponibilidad; matriz 400/429/503 con defectos; consumo único/reutilización de error MVC posterior; timeout incierto no reintentado; coordinación concurrente; frontera 400 con guard; frontera válida difiere a MVC; orden frente al filtro automático nativo, espera obligatoria y no ejecución de su 400 anticipado; resultados seguros; guard 503 frente a ModelState inválido; frontera ausente; cuenta no autenticada; cliente cancelado; registros scoped/JsonOptions; composición frontera → continuación |
| Estado | **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**, para decisión expresamente aprobada y coordinación aislable verificada; sin declarar integración real |

**Diferidos obligatorios:** F2 materializa suites mantenibles y factories; 021 implementa cuota real con sus puertas de cardinalidad/HMAC, atomicidad y coordinación, sin obtener permiso de este cierre; normalizador/validadores funcionales en sus tareas. 056 conecta tramo HTTP/Options, 057 dependencias/admisión, **058 integra MVC/cuotas/autenticación y prueba el orden completo**, 060 regresa el conjunto. Aquí se invocaron componentes/filtros reales con colaboradores controlados, no se ejecutó un pipeline MVC/servidor real, modelo funcional, Redis ni control distribuido. No se acredita rechazo/admisión de la API actual ni que Redis haya consumido exactamente una vez.

**Casos F2:** matriz D012 en suites y factory incremental, 400 automático con errores de binding/ModelState reales, guard cancelado/no verificable, identidad inválida sin buckets, datos completos con defectos múltiples, atomicidad de cuotas reales y doble invocación/reentradas, ninguna consulta de existencia, timeout con consumo incierto, orden autenticación → cuenta/logout → cuota y no dependencia de Bearer opcional en register/login. Las 286 aserciones del probe no son UT/IT/ST mantenibles ni cierre de GATE-A.

Estado agregado: **11 completadas (001–007,009–012), 008 preparada con startup pendiente, 67 PENDIENTE / NO EJECUTADA (013–079)**. Los cierres previos y 008 se preservan; **8 gates pendientes**, incluido **GATE-A**, faltan 013–015 y materialización F2. Cobertura documental AUTH 48/48, PLD-AUTH 25/25, AC 21/21 no equivale a satisfacción. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO**; riesgos R-006/007/008 sin cambios. Program, Options/appsettings de 007/008, spec/plan, constituciones/foundation, dependencias/lockfile/solución y Docker/workflows no se modifican por este bloque. Sin endpoints, conexiones, JWT criptográfico, cuotas distribuidas, commit o push. Se detiene el trabajo al completar 012, sin iniciar F2.

**Verificación de cierre del bloque:** eliminados los **10/10 artefactos temporales** (cinco fuentes del probe, script, runtimeconfig, response file del compilador y dos DLL) y el directorio temporal propio vacío; ausencia comprobada. Inventario Git: ocho archivos nuevos de este bloque, seis Configuration previos preservados y cuatro tracked modificados (dos JSON previos más task/memoria), sin staged. Comparación UTF-8 con HEAD confirma 67/67 fichas 013–079 intactas, definiciones oficiales 001–012 sin alteración salvo estados/enlaces de evidencia y E001–E006 íntegros. Archivos protegidos fuera de esas rutas sin diff; hash del lockfile idéntico a F0; comprobación de whitespace en los 14 archivos nuevos correcta. Git diff/check y status finales se comunican al entregar, sin commit/push.

## 22. Evidencia técnica de T-AUTH-013 y T-AUTH-014

<a id="e-t-auth-013"></a>

### E(T-AUTH-013)

Autorización BUILD CONTROLADO delimitada a F2, con baseline F1 recuperada y SDK 10.0.403 / `net10.0`. **Trazabilidad oficial:** AUTH-041/042/043; PLD-AUTH-01/03/20; AC-21; depende de 003/006; F2 / GATE-A. No se modifica `Program.cs`, no se integra host ni se inicia 015.

| Campo | Evidencia real 013 |
| --- | --- |
| Proyectos | Se crean exactamente `tests/Enver.API.UnitTests`, `tests/Enver.API.IntegrationTests` y `tests/Enver.API.SecurityTests`, cada uno net10.0, no empaquetable y con referencia a `Enver.API`; no se crea cuarto proyecto. |
| Solución | `Enver.Backend.slnx` conserva `Enver.API` y añade los tres proyectos mediante entradas `Project Path`; no se regenera la solución. |
| Descubrimiento/ejecución | `dotnet test ...UnitTests... --no-build`: 4/4; `...IntegrationTests...`: 2/2; `...SecurityTests...`: 2/2. El adaptador real xUnit VSTest 4.0.1 descubre cada suite; no hay falso verde por cero tests. |
| Pruebas materializadas | UT: Options sintéticas válidas, duplicate kid, cancelación cliente y resultado incierto tras despacho. IT: Problem Details 503 seguro/no-store y D012 con admisión conjunta única bajo concurrencia. ST: JSON duplicado y Bearer de 4097 bytes rechazados sin eco de valores. No se inicia host, endpoints, MVC pipeline real, MongoDB o Redis. |
| Build | `dotnet build Enver.Backend.slnx --no-restore`: correcto, **0 advertencias / 0 errores**, 11,26 s tras corregir exclusivamente dos detalles de las nuevas pruebas (API de NullFileProvider y colisión local del nombre Options). |
| Estado | **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**; tres suites reales disponibles, sin afirmar integración completa. |

<a id="e-t-auth-014"></a>

### E(T-AUTH-014)

**Trazabilidad oficial:** AUTH-039/041/042/043/044/045; PLD-AUTH-03/20; AC-10/21; depende de 013; F2 / GATE-A. La aceptación explícita conserva **xunit 2.9.3 Legacy**, únicamente por coherencia con el plan que exige xUnit v2; no se sustituye familia/major ni se interpreta como mantenimiento funcional futuro.

| Campo | Evidencia real 014 |
| --- | --- |
| Directos aprobados | En los tres proyectos: Microsoft.NET.Test.Sdk 18.11.0, xunit 2.9.3 y xunit.runner.visualstudio 4.0.1. Solo IT/ST añaden Microsoft.AspNetCore.Mvc.Testing 10.0.14, Testcontainers.MongoDb 4.16.0 y Testcontainers.Redis 4.16.0. Todas las referencias tienen `PrivateAssets="all"`. |
| Lockfiles | Cada proyecto declara `RestorePackagesWithLockFile=true`; restore autorizado genera sus tres `packages.lock.json`, sin edición manual. `dotnet restore Enver.Backend.slnx --locked-mode` correcto para API y tres suites. |
| Utilidades | Se crea solo el directorio físico `tests/Shared/`, sin proyecto ni fuente vacía: ningún auxiliar no-Docker compartido era necesario. Las pruebas usan TimeProvider nativo; no hay sleeps, Moq, coverlet, FluentAssertions, Microsoft.Extensions.TimeProvider.Testing, Testcontainers base directo ni transitivas forzadas. |
| Restore/build | `dotnet restore Enver.Backend.slnx` correcto; dos descargas iniciales truncadas de Microsoft.CodeCoverage 18.11.0 y Docker.DotNet.Enhanced 4.3.3 fueron reintentadas por NuGet y los tres restores concluyeron correctamente. Build final y los tres tests se ejecutan después sin advertencias/errores. |
| Grafo/auditoría | Directos y versiones coinciden con el aprobado. Transitivos relevantes: TestHost/ObjectModel/CodeCoverage 18.11.0; xunit core/assert/extensibility 2.9.3 y analyzers 1.18.0; TestHost/Hosting/DependencyModel 10.0.14; Testcontainers 4.16.0, Docker.DotNet.Enhanced/X509 4.3.3, SSH.NET 2026.0.0, SharpZipLib 1.4.2 y BouncyCastle.Cryptography 2.7.0. `dotnet list Enver.Backend.slnx package --include-transitive --vulnerable --no-restore` no informa paquetes vulnerables en los orígenes actuales `https://api.nuget.org/v3/index.json` y la caché SDK local. |
| Límites | La auditoría depende de avisos/fuentes actuales y no certifica ausencia universal de vulnerabilidades, imágenes, DLL embebidas o seguridad funcional. PrivateAssets evita propagación NuGet, pero no sustituye una verificación publish futura. Los módulos Testcontainers se restauran pero no se instancian: no hubo Docker, contenedores, imágenes, MongoDB, Redis ni Testcontainers ejecutados. |
| Estado | **COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA**. |

Estado agregado: **13 completadas (001–007,009–014), 008 preparada con startup pendiente, 65 PENDIENTE / NO EJECUTADA (015–079)**. Los ocho gates, incluido **GATE-A**, continúan PENDIENTE / NO SATISFECHO porque falta 015 y sus fixtures/arranque, entre otras condiciones. **AUTH-027 permanece NO IMPLEMENTADO / NO SATISFECHO**. Sin commit/push; se detiene el incremento sin iniciar 015.

## 23. Evidencia técnica de T-AUTH-015

<a id="e-t-auth-015"></a>

### E(T-AUTH-015)

Autorización BUILD CONTROLADO limitada a F2/T-AUTH-015. **Trazabilidad oficial:** AUTH-009/022/025–028/038/042/043; PLD-AUTH-05/07/08/20; AC-03/11/14–17; depende de 005/007/008/014; soporte GATE-A/B/C/G. No modifica `Program.cs`, producción, dependencias, imágenes/digests, lockfiles, spec/plan ni inicia 016.

| Campo | Evidencia real 015 |
| --- | --- |
| Fixture y plazo | `tests/Shared/AuthenticationDependenciesFixture.cs` usa un único presupuesto finito de inicialización de `TimeSpan.FromMinutes(10)` para pull/create/start de MongoDB y Redis, bootstrap del replica set y probes iniciales. Sustituye el presupuesto original de 2 minutos que canceló el primer pull de Mongo; cleanup y probes públicos conservan sus tokens de llamada. La primera descarga/inicialización efectiva de Mongo superó 2 minutos (aprox. 2 min 30 s de suite) y completó dentro del máximo autorizado. |
| Contenedores | Testcontainers inició MongoDB `mongo:8.0.34-noble@sha256:3c64937121b9437095921c4bb2569911084c05f7b88e4a263d62a8959d2b2399`, Redis `redis:8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba` y Ryuk `testcontainers/ryuk:0.14.0@sha256:7c1a8a9a47c780ed0f983770a662f80deb115d95cce3e2daa3d12115b8cd28f0`. |
| MongoDB | Contenedor iniciado con replica set `rs0`; `hello` confirmó primary escribible. La sonda realizó `updateOne`/lectura sintéticas en la base por ejecución y las pruebas comprobaron escritura/lectura compartida entre hosts. |
| Redis | Contenedor iniciado; `PING` devolvió `PONG`, `CONFIG GET maxmemory-policy` devolvió `noeviction` y `CONFIG GET maxmemory` confirmó 64 MiB (`67108864`). Se comprobó `SET`/`GET` y la eliminación selectiva de claves propias. |
| Aislamiento sintético | Una ejecución comparte el mismo `RunId`: `EnvironmentName=T015_<RunId>`, `MongoDatabase=enver_t015_<RunId>` y `RedisNamespace=enver:t015_<RunId>`. El guion bajo del namespace es la forma derivada del contrato F1 `enver:{environment.ToLowerInvariant()}`; no modifica el validador. |
| Factories | `AuthenticationApiFactory` aplica el ambiente sintético al `IHostBuilder` antes de construir el host y conserva la configuración in-memory. Dos factories A/B arrancaron con `/health` 200, mismo ambiente/base/namespace y la misma fixture externa; `IServiceProvider`, `TestServer`, probes y `TestHostLocalState` son distintos. El estado local de A no apareció en B; una escritura Mongo/Redis de A fue observable desde B mediante las dependencias compartidas. |
| Startup de Options | La corrección de factory evita que `WebApplicationFactory` conserve `Development`: `IHostEnvironment.EnvironmentName` es exactamente `T015_<RunId>` durante `ValidateOnStart`. SecurityTests acreditó un startup válido y el rechazo durante startup de cinco configuraciones inválidas (MongoDb, Redis, Jwt, Authentication y RateLimiting), sin secretos sintéticos en los mensajes comprobados. |
| Limpieza | Cada prueba elimina solo la base MongoDB propia y las claves Redis registradas; se comprobó su ausencia. Ante fallo parcial se disponen los contenedores creados; al final Testcontainers/Ryuk eliminó los contenedores. No se ejecutaron `FLUSHALL`, `FLUSHDB` ni `docker system prune`. |
| Restore y build | `dotnet restore Enver.Backend.slnx --locked-mode` correcto; los cuatro lockfiles no cambiaron. `dotnet build Enver.Backend.slnx --no-restore` correcto, **0 advertencias / 0 errores**. |
| Suites | `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build`: 4 descubiertas/ejecutadas, 4 correctas, 0 fallidas, 0 omitidas. `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build`: 8/8 correctas, 0 fallidas/omitidas, 46,1764 s. `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build`: 4/4 correctas, 0 fallidas/omitidas, 3,1385 s. |
| Limitación Scout | Docker Scout requiere autenticación en este entorno. No se inició sesión ni se solicitaron credenciales; la coincidencia de digests no afirma ausencia de vulnerabilidades de imágenes. |
| Estado | **T-AUTH-015 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.** T-AUTH-008 conserva **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE**. **GATE-A** y los otros siete gates siguen pendientes/no satisfechos; **AUTH-027 permanece NO IMPLEMENTADO / NO SATISFECHO**. |

## 24. Evidencia complementaria para reevaluación de GATE-A

Tras autorización BUILD CONTROLADO para materializar exclusivamente las brechas de evidencia de componentes, se crean/actualizan pruebas mantenibles de F1 sin modificar `Program.cs`, componentes productivos, paquetes, lockfiles, imágenes, digests efectivos, spec ni plan. Los dos primeros resultados de SecurityTests mostraron que los casos sintéticos de Bearer omitían el esquema `Bearer `; se corrigieron solo esos casos positivos/de frontera y su generador, no el middleware. El token sintético usa tres segmentos Base64url sin padding; las aserciones del mismo caso verifican longitud total de token y tres segmentos para 4096/4097, antes de invocar la frontera. Esto no implementa autenticación criptográfica.

| Verificación | Resultado real |
| --- | --- |
| T-AUTH-008 | `AuthenticationOptionsValidationTests` cubre secciones/valores obligatorios, MongoDB/Redis y aislamiento, issuer/audience/lifetime/skew/ActiveKid/kids/claves JWT, límites/presupuestos Authentication, las siete cuotas RateLimiting y HMAC sintético sin exponer secretos. Conserva **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE**; no integra F1 en `Program.cs`. |
| T-AUTH-009 / 014 | `RequestBudgetTests` usa `ManualTimeProvider` propio, sin sleeps, para saldo monotónico/no negativo, agotamiento, clamp, timeout de dependencia, cancelación enlazada y de cliente, prioridad, `MarkDispatched` e incertidumbre posterior al despacho. |
| T-AUTH-010 | `ProblemDetailsTests` cubre los códigos F1, forma y Content-Type, `no-store`, correlación, saneamiento y ausencia de sentinelas/eco, Retry-After, challenge Bearer, 204 vacío, segunda escritura, respuesta iniciada y cancelación. La evidencia de OnStarting usa `ControlledResponseFeature`: registra el callback, lo ejecuta explícitamente y observa correlación/no-store antes de marcar `HasStarted` y antes de que retorne el delegado; por ello no puede quedar verde por el fallback `finally` del middleware. |
| T-AUTH-011 | `AuthenticationBoundarySecurityTests` cubre 16384/16385, stream fragmentado no seekable, cancelación, JSON ambiguo/UTF-8/surrogates, media type/charset/encoding, logout vacío/no vacío y Authorization ausente/múltiple/malformado/compacto. Un token de **4096** caracteres con `Authorization: Bearer <token>` continúa la frontera; **4097** recibe 401 antes de criptografía. No acredita framing Kestrel, TLS, ALB/LB ni request smuggling. |
| T-AUTH-012 | `AuthenticationValidationCompositionTests` cubre D012 con colaboradores controlados: preflight 503 antes del 400, JSON ilegible o identidad inválida sin cuota, 429, resultado incierto 503 sin reintento, espera asíncrona MVC, ModelState saneado, admisión única/reentrada/concurrencia y logout no autenticado sin cuota ni buckets arbitrarios. |
| Restore/build/suites | `dotnet restore Enver.Backend.slnx --locked-mode` correcto, cuatro lockfiles sin cambios; `dotnet build Enver.Backend.slnx --no-restore` correcto con **0 advertencias / 0 errores**. UnitTests **44/44**, IntegrationTests **29/29** y SecurityTests **32/32** correctas, sin fallidas ni omitidas. |
| Publish | `dotnet publish Enver.API/Enver.API.csproj --no-restore -c Release` en directorio temporal exclusivo correcto. Archivos: configuración, `Enver.API`/PDB/deps/runtimeconfig/staticwebassets, `Microsoft.AspNetCore.OpenApi.dll`, `Microsoft.OpenApi.dll` y `web.config`. `Enver.API.deps.json` no contiene xunit, Microsoft.NET.Test.Sdk/TestPlatform, MVC Testing/TestHost, Testcontainers, Docker.DotNet, SSH.NET ni BouncyCastle. El directorio temporal se eliminó tras inspección. |
| Digest MongoDB | Se corrige solo la transcripción documental previa `...c05f7f88e4...` por el digest efectivo de la fixture `...c05f7b88e4...`; no cambia la fixture, imagen ni digest ejecutado. |

**BRECHAS TÉCNICAS PARA REEVALUACIÓN DE GATE-A: COMPLETADAS.** La corrección posterior de la prueba confirma además: **BRECHA ONSTARTING PARA GATE-A: COMPLETADA.** Esta evidencia complementaria no cierra automáticamente GATE-A: queda **PENDIENTE, A FALTA DE CONSOLIDACIÓN GIT Y CIERRE FORMAL**. T-AUTH-009 a T-AUTH-015 conservan **COMPLETADAS / EVIDENCIA TÉCNICA REGISTRADA**; T-AUTH-016 permanece **PENDIENTE / NO EJECUTADA** y AUTH-027 **NO IMPLEMENTADO / NO SATISFECHO**. Sin stage, commit ni push.

## 25. Cierre formal de GATE-A — Base/configuración

El **5 de noviembre de 2026**, tras la reevaluación formal final y la consolidación autorizada, **GATE-A — Base/configuración queda SATISFECHO / CERRADO**. El cierre se limita a las capacidades de baseline, configuración, componentes F1 y soporte de pruebas F2 descritos en este catálogo; no altera el estado ni la evidencia histórica de sus tareas integrantes.

| Campo | Registro de cierre |
| --- | --- |
| Checkpoint técnico F1/F2 | `1658c57478c8866ae5abe9aaabefe7fcba088508` — `test(auth): complete base configuration gate evidence` |
| Evidencia | E(T-AUTH-001) a E(T-AUTH-015), más la evidencia complementaria de §24 y la brecha OnStarting completada |
| Restore y build | `restore --locked-mode` correcto; build con **0 advertencias / 0 errores** |
| Suites | UnitTests **44/44**, IntegrationTests **30/30** y SecurityTests **32/32**; **0 fallidas / 0 omitidas** |
| Publish | Publish de `Enver.API` inspeccionado correctamente; `Enver.API.deps.json` sin dependencias exclusivas de testing |
| OnStarting | Ejecución efectiva acreditada mediante `ControlledResponseFeature`: callback registrado y ejecutado, con correlación y `no-store` observados antes de iniciar la respuesta y sin depender del fallback `finally` |
| Git | Consolidación completada en el checkpoint indicado; working tree limpio al evaluar el gate |

El cierre de GATE-A **no significa** autenticación productiva terminada, endpoints Register/Login/Logout implementados, Redis/MongoDB/JWT productivos integrados, despliegue AWS o infraestructura cloud validada. **T-AUTH-008** conserva **IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE**; su integración de host permanece reservada a 056. **T-AUTH-016** permanece **PENDIENTE / NO EJECUTADA**, y GATE-B a GATE-H permanecen **PENDIENTES / NO SATISFECHOS**. **AUTH-027** sigue **NO IMPLEMENTADO / NO SATISFECHO**. **R-007** permanece **REDUCIDO — NO RESUELTO**; la inspección completa posterior de artefactos/sentinelas pertenece a 078 / GATE-H. Este cierre no declara aceptación externa, infraestructura productiva ni cumplimiento integral de la rúbrica.

## 26. Evidencia técnica de T-AUTH-016

<a id="e-t-auth-016"></a>

### E(T-AUTH-016)

Autorización BUILD CONTROLADO limitada a F3/T-AUTH-016. **Trazabilidad oficial:** AUTH-025/026/028/032/034/038; PLD-AUTH-03/07/16/18; AC-14/19/21; depende de 007/009/015; GATE-B. No modifica `Program.cs`, endpoints, revocación real, TTL, cuotas/HMAC, MongoDB, JWT funcional ni inicia 017.

| Campo | Evidencia real 016 |
| --- | --- |
| Paquete y grafo | `Enver.API.csproj` incorpora únicamente `StackExchange.Redis` **3.4.31** fijo. El grafo efectivo de API resuelve `RESPite` 3.4.31 y `System.IO.Hashing` 10.0.13; los lockfiles de pruebas también resuelven `Microsoft.Extensions.Logging.Abstractions` y `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.13 cuando el framework no los satisface. No se agregan referencias directas redundantes. |
| Lockfiles y auditoría | Restore normal y `dotnet restore Enver.Backend.slnx --locked-mode` correctos; se regeneran solo `Enver.API/packages.lock.json` y los tres lockfiles de pruebas por el grafo de API. `dotnet list Enver.Backend.slnx package --include-transitive --vulnerable --no-restore` no informó paquetes vulnerables en NuGet.org/caché SDK actuales; no certifica ausencia universal de vulnerabilidades, imágenes o plataforma. |
| Conexión | `RedisConnections` es propietario de larga vida por destino lógico; comparte una inicialización `ConnectAsync` y devuelve un único `IConnectionMultiplexer` reutilizable a callers concurrentes. No crea conexiones por request, no conserva ni registra connection strings/contraseñas/claves, y dispone una vez el multiplexer bajo su ownership. No se integra aún en `Program.cs`. |
| Opciones efectivas | Construye `ConfigurationOptions` desde `RedisOptions`: `ConnectTimeout=ConnectionTimeoutMilliseconds`, `SyncTimeout=AsyncTimeout=OperationTimeoutMilliseconds`, `BacklogPolicy.FailFast`, `AbortOnConnectFail=false`, `ReconnectRetryPolicy=ExponentialRetry(ConnectionTimeoutMilliseconds)` e `IncludeDetailInExceptions=false`. Las pruebas reales usan los valores sintéticos ya aprobados de 250 ms; no se fijan valores productivos nuevos. |
| Eventos y cancelación | Expone solo señales saneadas `ConnectionFailed`, `ConnectionRestored`, `ErrorMessage` e `InternalError`, sin detalles de endpoint, claves o secretos. `GetMultiplexerAsync` permite cancelar la espera del caller con `WaitAsync`; esto no cancela la conexión u operación Redis ya despachada, no habilita reintentos ciegos y no dispone el singleton por una cancelación individual. |
| Namespace/clave | `RedisKeyBuilder` construye exclusivamente `enver:{environment}:auth:revoked:{issuer}:{jti}`; ambiente canónico validado, issuer fijo `enver-api` y `jti` Base64url canónico sin padding de 32 bytes/43 caracteres. Rechaza vacíos, espacios, caracteres inválidos, issuer ajeno, JWT compacto y correo detectable; la validación de formato no autentica el jti. No implementa `SET`/`GET`/`DEL` de revocación productiva. |
| Pruebas UT | 7 métodos nuevos: formato/aislamiento, rechazo de segmentos y jti inválidos, opciones explícitas, señales saneadas y cancelación previa a conexión. |
| Pruebas IT / Redis real | Redis Testcontainer real acreditó 16 callers concurrentes con el mismo multiplexer, `GetDatabase` reutilizado, `SET`/`GET`/`DEL` sintéticos, aislamiento de dos namespaces y cleanup selectivo. La parada controlada produjo fallo finito sin backlog; tras reiniciar, la fixture confirmó Redis sano. La cancelación de caller conservó el multiplexer reutilizable. El cierre con 128 lecturas sintéticas en vuelo devolvió el valor propio o fallo, nunca el valor de otra clave; no se afirma reproducir exactamente el incidente upstream. No se usaron `FLUSHALL`, `FLUSHDB` ni `docker system prune`. |
| Límite de recuperación | La misma instancia no reconectó dentro de la ventana acotada tras el ciclo de parada/reinicio de Testcontainers, aunque la fixture confirmó la disponibilidad del Redis reiniciado. 016 no introduce recreación ni loops propios; recuperación, detección, continuidad y fail-closed integral continúan en 017+ y gates posteriores. |
| Resultados | Restore normal correcto; restore locked correcto; build **0 advertencias / 0 errores**. UnitTests **56/56**, IntegrationTests **35/35** y SecurityTests **32/32**, sin fallidas ni omitidas. |
| Estado | **T-AUTH-016 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.** **GATE-B** permanece PENDIENTE / NO SATISFECHO. **AUTH-027** permanece NO IMPLEMENTADO / NO SATISFECHO. |

## 27. Evidencia técnica de T-AUTH-018

<a id="e-t-auth-018"></a>

### E(T-AUTH-018)

Autorización BUILD CONTROLADO limitada a F3/T-AUTH-018. **Trazabilidad oficial:** AUTH-026/027/038; PLD-AUTH-08/14/18/24; AC-14/15; depende de 009/016; GATE-B. No modifica `Program.cs`, endpoints, JWT funcional, revocación/TTL, cuotas/HMAC, MongoDB, detectores reales 019, recuperación/coordinación 020 ni inicia 017.

| Campo | Evidencia real 018 |
| --- | --- |
| Contrato y decisiones | Se crean `IAuthenticationSecurityState.cs` y `RedisSecurityStateGuard.cs`. El puerto devuelve `AuthenticationSecurityDecision` con admisión mínima `Verifiable`, `Unavailable` o `Uncertain`, causa de cancelación cuando existe, `CanContinue` y `ResultUncertain`; no expone HTTP, tipos Redis, JWT o repositorios. La traducción futura de no admisión a 503 `temporarily_unavailable` queda disponible por decisión, sin escribir respuesta ni endpoint. |
| Guard y conexión | El guard reutiliza `RedisConnections`, su único multiplexer reutilizable y un `DependencyBudget` limitado por `RedisOptions.OperationTimeoutMilliseconds`. Una sonda `PING` acotada acredita únicamente disponibilidad puntual para el guard; no se presenta como prueba de integridad, continuidad o reapertura segura. No usa `IsConnected` como prueba ni crea conexión/request, fallback local, loop o retry de aplicación. |
| Fail-closed e incertidumbre | Redis no disponible, error de conexión o fallo previo al despacho → `Unavailable` y no admisión. Antes de `MarkDispatched`, timeout/cancelación conserva `ResultUncertain=false`; después del despacho, timeout/cancelación o error Redis se traduce a `Uncertain`, cierra admisión y nunca se interpreta como no revocado, éxito ni credencial inválida. La cancelación del cliente no admite ni reintenta y no dispone `RedisConnections`. |
| Señales y recuperación | `ConnectionFailed`, `ErrorMessage` e `InternalError` cierran admisión. `ConnectionRestored` no la reabre. Tras un fallo el guard sigue conservador aunque Redis responda; la señal simulada prueba reacción, no detector. No se implementa fuente de confianza, detector real, propagación, coordinación ni recuperación segura: corresponden a 019/020. |
| Pruebas UT | 9 casos nuevos, sin Moq ni sleeps: admisión verificable; Redis no verificable sin fallback; señal `ConnectionFailed`; `ConnectionRestored` sin reapertura; timeout antes/después de despacho; cancelación cliente; agotamiento de presupuesto y límite de dependencia. Comprueban ausencia de retry mediante un único verificador controlado. |
| Pruebas IT / Redis real | Redis Testcontainer acredita disponibilidad, dos consumidores independientes fallando cerrados durante parada controlada, no reapertura automática tras `Start`, timeout real de operación despachada mediante `CLIENT PAUSE`, resultado incierto y cancelación del caller sin disponer la conexión. No se crean claves de revocación ni se ejecutan `SET`/`GET`/`DEL` productivos, `FLUSHALL`, `FLUSHDB` o `docker system prune`. Dos consumidores no sustituyen integración productiva de hosts, reservada a 057 y verificaciones posteriores. |
| Restore, build y suites | `dotnet restore Enver.Backend.slnx --locked-mode` correcto y sin cambios de lockfiles. Build **0 advertencias / 0 errores**. UnitTests **65/65**, IntegrationTests **39/39** y SecurityTests **32/32**, sin fallidas ni omitidas. |
| Estado al cierre de T-AUTH-018 | **T-AUTH-018 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.** T-AUTH-017 y T-AUTH-019 permanecían PENDIENTES / NO EJECUTADAS en ese cierre. **GATE-B** permanece PENDIENTE / NO SATISFECHO. **AUTH-027** permanece NO IMPLEMENTADO / NO SATISFECHO. |

## 28. Evidencia técnica de T-AUTH-019

<a id="e-t-auth-019"></a>

### E(T-AUTH-019)

Autorización BUILD CONTROLADO limitada a F3/T-AUTH-019. **Trazabilidad oficial:** AUTH-022/025–027/038/043; PLD-AUTH-08/20/24; AC-15; depende de 018; GATE-B. No modifica `Program.cs`, paquetes o lockfiles, endpoints, JWT funcional, revocación/TTL, cuotas, MongoDB, failover, flush, coordinación/reapertura 020 ni inicia 017.

| Campo | Evidencia real 019 |
| --- | --- |
| Detector y flujo | Se crea `RedisSecurityStateDetector.cs`. Se suscribe una sola vez a `RedisConnections.ConnectivityChanged`, toma T1 con `TimeProvider.GetTimestamp()` y propaga la señal saneada al `RedisSecurityStateGuard`. El guard deja de suscribirse directamente, evitando notificación duplicada. `ConnectionFailed`, `ErrorMessage` e `InternalError` cierran; `ConnectionRestored` solo se observa y nunca reabre. No se usa `IsConnected`, `PING` ni una operación puntual como criterio de confianza global. |
| Modelo y cierre | La IT provoca T0 antes de `StopRedisAsync`, espera `ConnectionFailed` real de cada consumidor, obtiene T1 desde el detector y evalúa el guard en T2. Calcula `DetectionLatency = T1 - T0` y `PropagationLatency = T2 - T1` con reloj monotónico y comprueba que ambas mediciones son no negativas; no establece SLA/RTO. Ambos guards independientes quedan no admisibles y no existe fallback ni retry de aplicación. |
| Redis real y reinicio limitado | Redis Testcontainer real acredita pérdida observable, flujo `RedisConnections → Detector → Guard`, cierre y propagación para dos consumidores. Tras `StartRedisAsync`, la fixture confirma Redis sano por sus recursos propios; `ConnectionRestored` se espera de forma finita y opcional, sin `GET` del multiplexer anterior ni condición de éxito. No se declara que el mismo multiplexer se haya recuperado ni que exista continuidad, misma instancia lógica, integridad o reapertura segura. |
| Timeout y seguridad | Una operación Redis real despachada y pausada mediante `CLIENT PAUSE` conserva `ResultUncertain`, cierra el guard y no habilita bypass. SecurityTests acredita señal adversa real, no admisión persistente después del reinicio y que disponibilidad puntual externa no equivale a integridad. No hay lectura/escritura de revocación, TTL, scripts Lua ni interpretación permisiva de ausencia de marca. |
| Límites explícitos | No se ejecutan `FLUSHALL`/`FLUSHDB`, eliminación de estado, restauración antigua ni failover; no se implementan polling, recreación de multiplexer, recuperación global, cuarentena, coordinación distribuida ni composición productiva. Esos asuntos permanecen en 020, 062, 063 y/o 068. Una pérdida silenciosa no observable sigue siendo riesgo residual. |
| Restore, build y suites | `dotnet restore Enver.Backend.slnx --locked-mode` correcto y sin cambios de lockfiles. Build **0 advertencias / 0 errores**. UnitTests **65/65**, IntegrationTests **39/39** y SecurityTests **33/33**, sin fallidas ni omitidas. |
| Estado | **T-AUTH-019 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.** T-AUTH-017 y T-AUTH-020 permanecen PENDIENTES / NO EJECUTADAS. **GATE-B** permanece PENDIENTE / NO SATISFECHO. **AUTH-027** permanece NO IMPLEMENTADO / NO SATISFECHO. |

## 29. Evidencia técnica de T-AUTH-017

<a id="e-t-auth-017"></a>

### E(T-AUTH-017)

Autorización BUILD CONTROLADO limitada a F3/T-AUTH-017. **Trazabilidad oficial:** AUTH-020/022–026/039/042; PLD-AUTH-02/07–09/20; AC-10–14; depende de 016/019; GATE-B. No modifica `Program.cs`, endpoints, JWT funcional, logout, cuotas, MongoDB, `CheckRevocation.lua`, read-back, `WAIT`, recuperación/coordinación 020 ni agrega paquetes.

| Campo | Evidencia real 017 |
| --- | --- |
| Puerto y resultados | Se crean `ITokenRevocationStore`, requests mínimos de escritura (`issuer`, `jti`, `exp`, skew efectivo y margen operativo) y lectura (`issuer`, `jti`), y resultados tipados. Escritura: `Confirmed`, `NoOperation`, `Unavailable`, `Uncertain`; lectura: `Revoked`, `NotRevoked`, `Unavailable`, `Uncertain`. Conservan causa de cancelación cuando aplica y derivan `ResultUncertain`; no exponen HTTP, JWT completo, claims, PII ni tipos Redis en el puerto. |
| Store y clave | `RedisTokenRevocationStore` recibe `TimeProvider`, `RedisConnections`, `IAuthenticationSecurityState` y `RedisOptions`. Deriva el ambiente de `Redis:NamespacePrefix` y delega exclusivamente la clave canónica a `RedisKeyBuilder`: `enver:{environment}:auth:revoked:enver-api:{jti}`. No escribe JWT, PII, secretos, issuer duplicado ni jti en el valor. |
| TTL y no operación | Calcula `retainUntil = exp + skew efectivo + margen operativo` con UTC; skew/margen negativos, offset no UTC u overflow dan `Unavailable`. Redondea la fecha absoluta hacia arriba a milisegundos. Si `retainUntil <= now` obtenido del `TimeProvider`, devuelve `NoOperation` sin Redis; no crea TTL cero/negativo ni clave permanente. |
| Script y ACK | `RevokeToken.lua`, empaquetado como recurso, recibe la clave, el valor fijo `"1"` y la expiración Unix absoluta en milisegundos. En una única ejecución atómica: crea la clave con `PXAT` si no existe; conserva la expiración existente si es igual/mayor; usa `PEXPIREAT ... GT` solo para extender; y devuelve `0` si detecta una marca sin expiración. Solo retorno `1` es ACK `Confirmed`: comando despachado, terminado dentro del presupuesto, respuesta esperada y retención suficiente confirmada por la misma operación. No afirma persistencia universal, réplica confirmada ni seguridad ante failover. |
| Primario y lectura | StackExchange.Redis 3.4.31 no contiene `DemandPrimary`; sus metadatos locales confirman `CommandFlags.DemandMaster`, documentado como operación solo en primary. `ScriptEvaluateAsync` y `StringGetAsync` usan `DemandMaster`; no se usa `FireAndForget`, réplica, caché negativa, `CheckRevocation.lua`, read-back o `WAIT`. |
| Fail-closed e incertidumbre | Escritura y lectura evalúan primero `IAuthenticationSecurityState`; estado no verificable no envía Redis y produce `Unavailable` o `Uncertain`. Tras crear `DependencyBudget` con el timeout Redis, `MarkDispatched` ocurre inmediatamente antes del comando. Pre-despacho agotado/cancelado produce `Unavailable` sin incertidumbre; timeout, cancelación o fallo sin ACK tras despacho producen `Uncertain`/`ResultUncertain=true`. No hay retry, loop, Polly ni fallback de aplicación. |
| Pruebas UT | 11 casos nuevos, sin Moq: TTL normal con redondeo, `NoOperation`, skew/margen negativos, overflow, no envío ante security state cerrado/incierto, timeout antes/después de despacho, no retry, clave exacta, inputs mínimos y mapeo `Revoked`/`NotRevoked`. |
| Pruebas IT / Redis real | Redis Testcontainer ejecutó el Lua real: primera marca, lectura por segundo consumidor, valor `"1"`, TTL positivo, repetición con mismo/menor/mayor fin, no reducción, extensión y concurrencia con expiraciones distintas cuyo fin final es al menos el máximo solicitado. También acreditó clave ausente solo con admisión verificable, aislamiento por namespace, estado cerrado no permisivo, parada Redis conservadora y timeout real despachado mediante `CLIENT PAUSE` como incierto. Cada caso elimina exclusivamente sus propias claves; no se usaron `FLUSHALL`, `FLUSHDB` ni `docker system prune`. |
| Límites explícitos | Dos consumidores y un contenedor local no prueban continuidad, persistencia, ACK de réplica, failover, pérdida/restauración, integridad ni coordinación distribuida. `AUTH-027` sigue no implementado/no satisfecho; cuarentena, recuperación y reapertura confiable permanecen en T-AUTH-020 y contrastes posteriores. |
| Restore, build y suites | `dotnet restore Enver.Backend.slnx --locked-mode` correcto, lockfiles sin cambios; build **0 advertencias / 0 errores**. UnitTests **76/76**, IntegrationTests **46/46** y SecurityTests **33/33**, sin fallidas ni omitidas. |
| Estado | **T-AUTH-017 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA.** **T-AUTH-020** permanece PENDIENTE / NO EJECUTADA. **GATE-B** permanece PENDIENTE / NO SATISFECHO. **AUTH-027** permanece NO IMPLEMENTADO / NO SATISFECHO. |

## 30. Evidencia local parcial de T-AUTH-020-A

<a id="e-t-auth-020-a"></a>

### E(T-AUTH-020-A)

Autorización BUILD CONTROLADO limitada al subpaso local 020-A de F3/T-AUTH-020. **Trazabilidad oficial:** AUTH-022/024/026–028/038/039; PLD-AUTH-08/09; AC-15/20; depende de 017/019; GATE-B. No modifica `Program.cs`, endpoints, login/JWT, Terraform, AWS, IAM, DynamoDB Local/real, paquetes ni lockfiles; no crea el adapter DynamoDB ni inicia 021.

| Campo | Evidencia real 020-A |
| --- | --- |
| Puerto y modelos | Se crea `IAuthenticationSecurityAuthority` con `ReadAsync`, `MergeClosedAsync` y `TryReopenAsync`, sin tipos AWS, DynamoDB, Redis, HTTP, JWT ni PII. Sus modelos distinguen `Open`/`Closed`, `Available`/`Conflict`/`Unavailable`/`Uncertain`, causa de cancelación cuando corresponde, `Generation`, `Revision`, última emisión posible y metadatos de cuarentena. `Closed` requiere metadatos UTC válidos; ausencia de registro no equivale a `Open`. |
| Coordinador local | `AuthenticationSecurityCoordinator` implementa sin cambios `IAuthenticationSecurityState`. Consulta la autoridad antes de admitir; ausencia, fallo, resultado incierto o `Closed` cierran el latch local. `CloseAsync` cierra el latch antes de E/S y no lo abre por Redis sano, `PING`, reconexión, `ConnectionRestored` ni Redis vacío. Solo una transición explícita de autoridad `Closed → Open` con revisión posterior puede ser observada como reapertura. |
| Emisión, horizonte y cuarentena | `BeginEmissionAsync` exige autoridad `Open` verificable y registra una lease local; `CompleteEmission`/`FailEmission` liberan solo el seguimiento local y nunca reducen el máximo conservado. El cierre usa el máximo local y el instante UTC actual; el horizonte se deriva de `JwtOptions.LifetimeSeconds + JwtOptions.ClockSkewSeconds + RedisOptions.RevocationOperationalMarginSeconds`, sin literal 990. El fake conserva máximos de emisión, cuarentena y horizonte durante el cierre. La persistencia CAS distribuida por emisión no se afirma: requiere el adapter 020-B. |
| CAS y reapertura | `Generation` aumenta exclusivamente en `Open → Closed`; `Revision` aumenta en los cambios compartidos realizados por el fake. Ante un conflicto hay un reread/recomputación y un único segundo intento; otro conflicto sigue cerrado. La operación explícita de control solo reabre tras cuarentena completa, sin leases locales en vuelo y con CAS de generación/revisión; errores, timeout/incierto o autoridad no disponible no asumen `Open`. Restauración confiable validada queda fuera de 020-A. |
| Pruebas locales | Fake compartido thread-safe cubre estado ausente, Open/Closed, CAS, conflictos y resultados no disponibles/inciertos. UT cubren admisión inicial, latch, emisiones, máximo, horizonte, cuarentena, cambios de configuración, generación/revisión, conflicto y reloj. IT usa Redis Testcontainer real, dos coordinadores con fake compartido, parada/arranque Redis, clave ausente, cierre observado por B y reapertura explícita tras cuarentena. ST cubre ping, Redis vacío/reinicio, autoridad ausente/no disponible, generación atrasada, cuarentena incompleta y que una clave de revocación ausente no autoriza cuando la autoridad está cerrada. |
| Restore, build y suites | `dotnet restore Enver.Backend.slnx --locked-mode` correcto y lockfiles sin cambios. Build **0 advertencias / 0 errores**. UnitTests **88/88**, IntegrationTests **48/48** y SecurityTests **37/37**, sin fallidas ni omitidas. |
| Límites y estado | Un fake compartido y Redis local no acreditan DynamoDB, lectura fuertemente consistente, `ConditionExpression`, timeout real de SDK, despliegue AWS, IAM, bootstrap del registro, continuidad/failover ni coordinación distribuida de emisiones. **T-AUTH-020-A COMPLETADA / EVIDENCIA LOCAL REGISTRADA. T-AUTH-020 IMPLEMENTACIÓN PARCIAL / ADAPTER DYNAMODB PENDIENTE. GATE-B PENDIENTE / NO SATISFECHO. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO.** |

## 31. Evidencia DynamoDB Local de T-AUTH-020-B y cierre de T-AUTH-020

<a id="e-t-auth-020"></a>

### E(T-AUTH-020)

Autorización BUILD CONTROLADO limitada a T-AUTH-020-B. **Trazabilidad oficial:** AUTH-022/024/026–028/038/039; PLD-AUTH-08/09; AC-15/20; depende de 017/019; GATE-B. Sin cambios a `Program.cs`, endpoints, login/JWT, Terraform, IAM/AWS real, T-AUTH-021, stage, commit o push.

| Campo | Evidencia real 020-B |
| --- | --- |
| Paquete y grafo | `Enver.API` referencia únicamente `AWSSDK.DynamoDBv2` **4.0.110.1** (Apache-2.0); restore resuelve `AWSSDK.Core` **4.0.102.18**, dentro de `[4.0.102.18, 5.0.0)`. No se agregaron paquetes directos de pruebas. Los tres lockfiles de prueba reflejan exclusivamente ambas dependencias transitivas del `ProjectReference` a `Enver.API`; no hay dependencias ajenas. La consulta de vulnerabilidades de NuGet no reportó paquetes vulnerables. |
| Puerto, emisión y coordinador | El puerto añade `RegisterPotentialIssuanceAsync`, request con CAS `Generation`/`Revision` y resultado con los cuatro estados. `Open` permite solo `LastPossibleIssuanceUtc` UTC opcional y no metadata de cierre. `BeginEmissionAsync` lee `Open`, calcula `now + RequestBudget.Remaining`, registra antes de crear lease y, ante un conflicto, hace un único reread/recomputación/segundo intento. Ausencia, `Closed`, segundo conflicto, indisponibilidad o incertidumbre niegan emisión y cierran el latch. `CompleteEmission`/`FailEmission` solo liberan la lease local; no reducen ni compensan el máximo compartido. |
| Adapter y semántica DynamoDB | `DynamoDbAuthenticationSecurityAuthority` usa únicamente `IAmazonDynamoDB` de bajo nivel dentro de Infrastructure. `GetItemAsync` usa `ConsistentRead=true`; ausencia se devuelve sin estado y nunca como `Open`. El mapping valida `Id`, estado, números, timestamps Unix en milisegundos, rangos, overflow y metadata; datos inválidos son no disponibles. Las mutaciones usan `UpdateItem` condicionado por estado, generación y revisión; `ConditionalCheckFailedException` se mapea solo a `Conflict`. `Register` conserva `Open`, generación y máximo de timestamp, e incrementa revisión. `MergeClosed` relee consistentemente, conserva máximos y solo incrementa generación en `Open → Closed`; `TryReopen` exige cuarentena completa y elimina metadata. |
| Presupuestos e incertidumbre | Cada operación AWS usa `RequestBudget.BeginDependency` con máximo 500 ms y marca despacho inmediatamente antes de la llamada. Cancelación/timeout antes de despacho es no disponible; después de despacho sin respuesta semántica de mutación es incierto y no se reintenta. Errores de recurso/autorización/validación/throttling se tratan de modo conservador. `MaxErrorRetry=0` se aplica al cliente DynamoDB Local; el adapter no añade retries. |
| DynamoDB Local y bootstrap | La fixture usa Testcontainers genérico con `amazon/dynamodb-local:3.1.0@sha256:7ef4a2c45b58c2901e70a4f28e0953a422c2c631baaaf5e2c15e0805740c7752`, puerto interno 8000/publicado dinámicamente, readiness por puerto y `ListTablesAsync` acotado, cliente HTTP local con región y credenciales sintéticas no secretas y tabla aislada por ejecución. Crea solo su tabla con key `Id` string y throughput 1/1; bootstrap condicionado crea explícitamente `authentication/Open/0/0`. |
| Pruebas | UT cubren CAS/recomputación de emisión. IT con DynamoDB Local cubren ausencia, bootstrap, lectura inmediata consistente por adapter, monotonicidad de timestamp, generación/revisión, cierre/reapertura, dos clientes/escritores, writer atrasado y carreras register/cierre. La IT de coordinadores usa dos clientes/autoridades, dos coordinadores, DynamoDB Local y Redis Testcontainer real: A registra, cierra, B observa `Closed`; parada/arranque Redis y clave ausente no habilitan; tras cuarentena y control explícito B reabre y vuelve a admitir. ST cubre autoridad DynamoDB ausente/no disponible, conflicto/incertidumbre de register, generación atrasada, cuarentena incompleta y Redis vacío sin bypass. |
| Restore, build y suites | Restore normal actualizó `Enver.API/packages.lock.json` y sus proyecciones transitivas en los tres lockfiles de pruebas; `dotnet restore Enver.Backend.slnx --locked-mode` fue correcto. Build **0 advertencias / 0 errores**. UnitTests **89/89**, IntegrationTests **52/52** y SecurityTests **39/39**, sin fallidas ni omitidas. |
| Límites y estado | DynamoDB Local acredita semántica de adapter, no AWS regional, IAM, disponibilidad, continuidad/failover, recuperación ante pérdida, despliegue Terraform ni riesgo de pérdida catastrófica silenciosa no observable. **T-AUTH-020-A COMPLETADA / EVIDENCIA LOCAL REGISTRADA; T-AUTH-020-B COMPLETADA / EVIDENCIA DYNAMODB LOCAL REGISTRADA; T-AUTH-020 COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA. GATE-B PENDIENTE / NO SATISFECHO. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** |

## 32. Evidencia de contrato de T-AUTH-052

<a id="e-t-auth-052"></a>

### E(T-AUTH-052)

Registro de cierre del **10 de noviembre de 2026**. El responsable aprueba la decisión técnica pre-BUILD y autoriza exclusivamente el contrato aislable de T-AUTH-052. **Trazabilidad oficial:** AUTH-028–030/031/039/040; PLD-AUTH-14/15; AC-16/18; dependencias 012/017 completadas; GATE-B. Baseline `feature/authentication`, HEAD `f9f455191e3d14a691403c13f459ba952b9eb770`, working tree limpio. La excepción posterior autoriza Docker/Testcontainers únicamente para regresión IT/ST existente. No hay commit de esta entrega ni consulta al remoto.

| Campo | Evidencia real 052 |
| --- | --- |
| Archivos de aplicación | Se crean `FA/Application/Ports/IAuthenticationQuotaStore.cs` y cuatro archivos en `FA/Application/Quotas/`: `AuthenticationQuotaModels.cs`, `AuthenticationQuotaPolicyDescriptor.cs`, `AuthenticationQuotaEvaluationContract.cs`, `AuthenticationQuotaRetryAfter.cs`. El puerto es abstracto, sin implementación, registro DI ni modificación de `IAuthenticationQuotaAdmission`. |
| Modelos e invariantes | Request inmutable con operación, InternalAttemptId, revisión y copia de ámbitos; discriminador cerrado Email/Origin/OriginEmail/Account; definición con PermitLimit y Window positivos. Se rechazan operación inválida, ámbitos ausentes/extra/duplicados/incompatibles, identificadores vacíos/con espacios/controles y definiciones nulas. No se inventa un límite de longitud. OpaqueId debe venir seguro y derivado por 053: la validación sintáctica no prueba privacidad ni origen confiable. Sin Cost/burst/weight/tokens, HttpContext, PII raw, JWT, RedisKey/RedisValue o tipos de driver. Se reutilizan los enums existentes de operación y causa de presupuesto, no transporte HTTP. |
| Resultados y frontera | Cuatro estados: Admitted, Rejected, Unavailable y Uncertain. RetryAfter positivo obligatorio solo para Rejected; null para los demás. ResultUncertain se deriva exclusivamente del status. Se conserva BudgetCancellationCause; enums inválidos y cancelación en resultados confirmados se rechazan. Pre-despacho sin resultado: Unavailable; post-despacho sin resultado semántico: Uncertain, incluso sin cancelación. Unavailable/Uncertain son compatibles con HTTP Unavailable/503; una UT con adapter solo de prueba invoca el coordinador existente y verifica 503 sin retry. No se implementa adapter HTTP productivo. |
| Siete cuotas | Descriptor AcademicInitial inmutable, revisión `academic-initial`: REGISTER email 3/900 s y origen 20/900 s; LOGIN par 5/60 s, email 10/60 s y origen 60/60 s; LOGOUT cuenta 30/60 s y origen 300/60 s. El descriptor exige las siete reglas únicas. El request debe coincidir con la revisión y definiciones del descriptor activo confiable; revisión vacía/desconocida/inactiva/incompatible o topología no apta devuelve Unavailable en preflight, sin despacho/consumo/fallback ni 400/429 público. |
| PolicyRevision e identidad | La revisión identifica un descriptor inmutable y no integra `AuthenticationQuotaPartitionIdentity`, que contiene operación, ámbito e identificador opaco dentro del contexto de ambiente/finalidad. No se genera un formato de key ni namespace físico. La UT conserva estado sintético bajo las mismas identidades al cambiar revisión; no demuestra coordinación runtime ni migración Redis. |
| Transición y drenaje | `AuthenticationQuotaPolicyTransition` conserva descriptor anterior/nuevo y ordena preparar, detener admisiones entre instancias, resolver operaciones en vuelo, establecer último consumo posible, preservar descriptor/estado anterior, drenar, activar coordinadamente y reabrir solo con continuidad verificable. DrainWindow es max(MaximumWindow anterior, MaximumWindow nueva), derivado sin literal 15 min. Una sola revisión admite; no PreviousPolicyRevision(s) por request ni lectura dual. El drenaje parte del último consumo posible, no del aviso; operaciones inciertas sin resolver o falta de prueba de cese impiden reabrir. No se implementan coordinador, temporizador, borrado/reset ni reapertura runtime. |
| Atomicidad y topología | `DescribeConfirmedInsertions` describe un evento por ámbito, todos con el mismo InternalAttemptId, para Admitted y cero para Rejected; no aplica efectos. Uncertain/Unavailable no se interpretan como inserciones confirmadas. Todas las claves deberán ser accesibles atómicamente en una única ejecución Lua en el destino efectivo; no se selecciona Cluster, hash slot universal, clave agregada, prohibición de Cluster ni cross-slot. Un booleano de capacidad en el preflight expresa la precondición, no demuestra la topología. Sin rollback general Lua: errores posteriores a escrituras pueden dejar efectos desconocidos/parciales; no son admisión/rechazo confirmados ni habilitan compensación, retry ciego o fallback local. |
| Contrato futuro de script | Descriptor ordenado verificable: validar request/descriptor; topología antes del despacho; cantidad/tipos/rangos de argumentos; operación/ámbitos/cuotas/ventanas/revisión; estado existente/precondiciones previsibles; Redis TIME una vez; evaluar ventanas con el mismo now; si bloquea, retornar máximo RetryAfter sin insertar; si todos admiten, depurar físicamente/insertar mismo ID en todos/aplicar retención futura. La depuración inicial es lógica. No se ejecuta Redis TIME ni existe Lua de cuotas. |
| Ventanas y RetryAfter | Cálculo puro sobre instantes suministrados, sin TimeProvider/UTC local: ventana (now - Window, now], excluye eventos en o antes del límite inferior y rechaza eventos futuros. Con n >= limit usa posición n - limit + 1 (base 1); con n == limit, el más antiguo. Retorna TimeSpan positivo redondeado hacia arriba a segundos completos y máximo entre bloqueantes; no bloqueante devuelve null. Ordena una copia, admite timestamps iguales para eventos distintos y falla explícitamente si el redondeo desborda, sin acortar el tiempo. No es limitador local ni fuente de admisión distribuida; la serialización HTTP queda fuera. |
| InternalAttemptId y efectos | Un solo ID suministrado por evaluación, compartido por todos sus ámbitos; no se genera otro tras incertidumbre. No es clave pública de idempotencia ni garantía exactly-once. Cada admisión representa un evento por ámbito, sin coste configurable; fallos posteriores de procesamiento no revierten consumo conforme D012. |
| UT nuevas | `UT/AuthenticationQuotaContractTests.cs` y `UT/AuthenticationQuotaRetryAfterTests.cs`: **70 casos nuevos**, sin Moq. Cubren estados/invariantes/cancelación/despacho, siete cuotas, ámbitos, inmutabilidad, validación de revisión/definiciones/topología, identidad estable y pasos de drenaje, todos-o-ninguno descriptivo/ID compartido, contrato de tiempo/script, frontera HTTP compatible, fronteras temporales, n == limit/n > limit, máximo/redondeo/overflow y ausencia de Cost/tipos de transporte o Redis en modelos. No acreditan ejecución atómica ni recuperación real. |
| Comandos y resultados | `dotnet restore Enver.Backend.slnx --locked-mode` correcto; `dotnet build Enver.Backend.slnx --no-restore` correcto, SDK fijado 10.0.403/net10.0, **0 advertencias / 0 errores** (14,17 s en build final). El build inicial detectó una advertencia xUnit2029 en una aserción nueva; se corrigió solo la UT antes de las suites. `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build`: **159/159**. `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build`: **52/52** (1 min 41 s). `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build`: **39/39** (51 s). Sin fallidas ni omitidas. |
| Docker y regresión | IT/ST fueron exclusivamente regresión existente con la fixture Testcontainers sin modificar, sus imágenes/digests existentes y dependencias MongoDB/Redis/DynamoDB Local. Docker se utilizó solo por esa infraestructura preexistente; no se añadieron IT/ST de 052 ni acreditan Redis/Lua de cuotas. |
| Conservación y diferidos | Los cuatro lockfiles y Program.cs conservan exactamente sus SHA-256 de partida; csproj/solución/configuración, fixtures e IT/ST sin diff. Sin paquetes nuevos, store Redis, Lua ejecutable de cuotas, HMAC/origen (053), MaxActivePartitions/capacidad/recuperación (054), implementación de 021, AWS, stage, commit ni push. Spec/plan, constituciones, foundation, AGENTS y README intactos; solo se actualizan este documento y memoria tras las verificaciones verdes. |
| Estado final | **T-AUTH-052 COMPLETADA / EVIDENCIA DE CONTRATO REGISTRADA. T-AUTH-053 y T-AUTH-054 PENDIENTES / NO EJECUTADAS. T-AUTH-021 BLOQUEADA POR 053 Y 054 / NO EJECUTADA. GATE-B PENDIENTE / NO SATISFECHO. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** Se detiene sin iniciar otra tarea. |

## 33. Evidencia aislable de T-AUTH-053

<a id="e-t-auth-053"></a>

### E(T-AUTH-053)

Registro del **10 de noviembre de 2026**. El responsable aprueba la decisión técnica pre-BUILD y autoriza exclusivamente T-AUTH-053 en BUILD CONTROLADO aislable, sin Docker ni componentes productivos. **Trazabilidad oficial:** AUTH-028/030/032–034/036; PLD-AUTH-16/17; AC-16/19/20; dependencias 008/012/016; GATE-B y soporte GATE-D/E/F. 012/016 completadas; 008 preparada con startup productivo pendiente, no se declara completada. Baseline limpia en `feature/authentication`, HEAD `6ef97b54b24416505dec2babf7bc205d02749990`, sin consulta al remoto ni commit de esta entrega.

| Campo | Evidencia real 053 |
| --- | --- |
| Archivos | Se crean `FA/Application/Ports/IAuthenticationScopePseudonymizer.cs`, `FA/Application/Quotas/AuthenticationScopeIdentity.cs`, `AuthenticationPseudonymizationTransition.cs`, `HTTP/AuthenticationOriginProvider.cs` y `Enver.API/Infrastructure/Security/HmacAuthenticationScopePseudonymizer.cs`. Se modifica únicamente el validador HMAC en `CFG/AuthenticationOptionsValidation.cs` y su auxiliar de comparación. Pruebas: dos archivos UT nuevos, ampliación de `UT/AuthenticationOptionsValidationTests.cs` y nueva clase `ST/AuthenticationScopePseudonymizationSecurityTests.cs`; documentación solo task/memoria. |
| Email | `AuthenticationCanonicalEmail.FromValidatedEmail` recorta exteriores, rechaza contenido no ASCII/espacios/controles y representaciones sin un único @ con partes no vacías; aplica minúsculas invariantes sin quitar puntos/+ ni normalización Unicode/IDNA. Su precondición es correo ya validado por la capa anterior: NO demuestra dominio/sintaxis/límites institucionales completos ni implementa 028. La integración futura deberá acreditar esa validación antes de crear ámbitos; no hay endpoints nuevos ni buckets para entradas externas en 053. |
| Origen canónico | `AuthenticationCanonicalOrigin` captura bytes sin referencia mutable: `ipv4:` + 8 hex minúsculos o `ipv6:` + 32 hex minúsculos. IPv4-mapped IPv6 se convierte a IPv4; ScopeId no cero se rechaza antes de convertir, sin eliminación silenciosa ni agrupación de prefijos IPv6. La representación canónica sigue siendo dato sensible reversible, no pseudónimo; solo el HMAC se entrega como OpaqueId. |
| Cuenta | `AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal` exige exactamente una identidad autenticada y un único sub en ella, UUID D no vacío, convertido a minúsculas. No toma claims de identidades anónimas ni body/query/header. El principal debe proceder de autenticación previamente acreditada; IsAuthenticated y parsear UUID no prueban firma/autorización. No se valida JWT en 053. |
| Identidad y puerto | `AuthenticationScopeIdentity` solo permite componentes canónicos tipados y combinaciones operación/ámbito de 052; reutiliza AuthenticationQuotaScopeType. OriginEmail fija login/origen primero/email después; Account fija logout. `IAuthenticationScopePseudonymizer` es local y síncrono, sin HttpContext/headers/JWT/tipos Redis, E/S, budget de dependencia, retry ni fallback. |
| HMAC y encoding | HMAC-SHA-256 BCL con digest completo de 32 bytes y Base64url sin padding, exactamente 43 caracteres [A-Za-z0-9_-]. Frame: LP(enver.auth.quota), LP(1), LP(ambiente), LP(operación), LP(ámbito), LP(componente1) y opcional LP(componente2); LP usa uint32 big-endian de longitud UTF-8 estricta sin BOM y bytes del campo. Etiquetas cerradas register/login/logout y email/origin/origin-email/account. Ambiente exclusivamente del IHostEnvironment confiable, minúsculas invariantes y representación UTF-8 válida; no request. Sin PolicyRevision/InternalAttemptId/timestamps/process IDs/key física. Un vector fijo fue calculado independientemente con BCL y datos sintéticos; las UT comprueban su salida exacta y el framing compuesto. |
| Clave y configuración | Se endurece mínimo de 1 a 32 bytes decodificados y se mantiene Base64 estándar canónica por roundtrip exacto, sin trim ni Base64url. Se rechazan null/vacío, formato inválido, whitespace, padding ausente/no canónico y tamaño insuficiente. Comparación de material decodificado contra claves JWT mediante CryptographicOperations.FixedTimeEquals, con limpieza de buffers. El componente también valida al recibir Options.Create sin validators; configuración inválida no genera clave alternativa. Longitud no prueba entropía. No se agregan opciones ni se activa ValidateOnStart en Program. |
| Secretos y estabilidad | Snapshot propio de clave/ambiente, sin hot reload de Options ni variación por instancia/cultura/proceso. Misma entrada canónica/contexto/clave produce el mismo pseudónimo; vectores con clave/contexto distintos difieren, sin prometer ausencia matemática de colisiones. ToString de componentes sensibles devuelve solo el tipo, errores fijos sin datos raw y no hay logs/respuestas de claves o entradas HMAC. Se limpian buffers temporales y clave al disponer, con exclusión para uso concurrente/dispose; no se afirma borrado de strings administrados ni anonimización absoluta. |
| Provider de origen | `AuthenticationOriginProvider` exige `AuthenticationOriginTrustContext` como feature creada explícitamente por servidor. Directo aprobado usa RemoteIpAddress; proxy requerido usa exclusivamente origen resuelto acreditado por transporte; sin feature/prueba/dirección o con zona no válida devuelve Unavailable. No lee/parsea Forwarded/X-Forwarded-For/X-Original-For y no vuelve al peer del proxy cuando falta resolución verificada. No hay unknown/random. La feature expresa una precondición confiable, no demuestra por sí misma la confianza del proxy. Su productor/integración y forwarding real quedan en 056/infraestructura; las ST inyectan contexto sintético, no acreditan cadena real de proxies. |
| Rotación/drenaje | `AuthenticationPseudonymizationTransition` modela cambio de clave o representación canónica, incluidos dominio/versión, sin contener secretos ni implementar coordinación. Reutiliza las fases de 052: preparar, detener admisiones globales, resolver in-flight, acreditar último consumo posible, preservar configuración/estado anterior, drenar, activar coordinadamente, reabrir con continuidad verificable. DrainWindow=max(oldPolicy.MaximumWindow,newPolicy.MaximumWindow); admite el mismo descriptor/revisión para rotar solo HMAC. HasDrainWindowElapsed solo calcula horizonte desde último consumo posible con instantes suministrados de confianza; false si faltan datos o no venció. true NO acredita cese global ni autoriza activación. Sin dual-read/consume, activación runtime, reset ni temporizador local presentado como coordinación. |
| Interop y diferidos | OpaqueId alimenta AuthenticationQuotaScope de 052 con ScopeType/Definition. No cambia PolicyRevision/identidad de partición ni las siete cuotas. 021 recibirá esos ámbitos, no datos raw/clave/HttpContext. 054 conserva cardinalidad/MaxActivePartitions/capacidad/lifecycle/recuperación; 021 conserva Redis/Lua/TIME/TTL/claves físicas/argumentos/topología. No se ejecutan ni implementan 021/054. |
| UT | **54 casos nuevos**, sin Moq: vector/formato/determinismo, separación de dominios/operaciones/ambientes, framing compuesto, cultura, email, IPv4/IPv6/mapped/zonas, cuenta/claims, configuración Base64/min32/JWT reutilizado, snapshot/dispose/concurrencia, interop 052 y drenaje sin activación anticipada. Suite completa **213/213**, sin fallidas ni omitidas. |
| ST focales sin Docker | **16/16**, sin fallidas ni omitidas: spoofing, ausencia de origen/confianza, directo que ignora headers, proxy requerido sin fallback, origen acreditado por feature de servidor, representaciones IP equivalentes, zonas, salida/diagnósticos sin datos raw, cuenta no acreditada y configuración inválida. La clase no tiene Collection/IClassFixture/factory ni usa AuthenticationDependenciesFixture. Inspección confirma fixture Docker por colección, no global; filtro ejecuta exclusivamente esta clase. No se ejecuta la suite ST completa ni IT; no se iniciaron Docker/Testcontainers. |
| Comandos | `dotnet restore Enver.Backend.slnx --locked-mode` correcto; cuatro lockfiles intactos. `dotnet build Enver.Backend.slnx --no-restore` correcto, **0 advertencias / 0 errores** (13,42 s final), SDK fijado 10.0.403/net10.0. `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build`: **213/213**. `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build --filter "FullyQualifiedName~Enver.API.SecurityTests.AuthenticationScopePseudonymizationSecurityTests"`: **16/16**. |
| Incidencia de pruebas | Primera UT: 210 correctas/3 fallidas nuevas porque el test de IStartupValidator omitía la sección sintética AuthenticationSecurityAuthority. Se completó únicamente la configuración local de esos tres casos, sin tocar fixture global ni producción adicional; build y UT completas repetidos correctamente antes de ST/documentación. No fue fallo de pseudonimización ni se ocultaron/omitieron pruebas. |
| Conservación y límites | SHA-256 de Program.cs y los cuatro lockfiles idénticos a baseline. Sin cambios a csproj/solución/paquetes, config JSON, fixtures, IT, spec/plan, constituciones, foundation, AGENTS/README o contrato 052. Sin DI productivo, JWT funcional, endpoints, forwarding/LB/TLS productivos, rotación runtime, Redis/Lua/TIME de 053, AWS/Terraform, stage, commit o push. No se presentan Options aisladas como startup productivo ni ST de feature como validación de proxies reales. |
| Estado | **T-AUTH-053 COMPLETADA / EVIDENCIA AISLABLE REGISTRADA. T-AUTH-008 IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE. T-AUTH-054 PENDIENTE / NO EJECUTADA. T-AUTH-021 BLOQUEADA POR 054 / NO EJECUTADA. GATE-B PENDIENTE / NO SATISFECHO. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** Trabajo detenido sin iniciar otra tarea. |

## 34. Evidencia del incremento contractual de T-AUTH-054

<a id="e-t-auth-054"></a>

### E(T-AUTH-054)

Registro del **10 de noviembre de 2026**. El responsable aprueba el contrato pre-BUILD, resuelve expresamente sus dos brechas de dimensionamiento/distribución UT/IT y autoriza exclusivamente BUILD CONTRACTUAL CONTROLADO de 054. Baseline limpia en `feature/authentication`, HEAD `c10d0c2e742ce57ac7bc6e788799820bf791b01b`, sin consulta al remoto. **Trazabilidad:** AUTH-025–028/030/038/040; PLD-AUTH-07/08/15/18; AC-14–16/18; dependencias formales 008/020/052. 020/052 conservan sus cierres y 053 aporta ámbitos opacos compatibles; 008 mantiene startup productivo pendiente, no se declara completada.

| Campo | Evidencia real y límites de 054 |
| --- | --- |
| Archivos | Cinco archivos nuevos en `Enver.API/Features/Authentication/Application/Quotas/`: `AuthenticationQuotaCapacityDescriptor.cs`, `AuthenticationQuotaPartitionState.cs`, `AuthenticationQuotaCapacityContract.cs`, `AuthenticationQuotaConfigurationSnapshot.cs`, `AuthenticationQuotaRecoveryContract.cs`. Tres archivos UT nuevos: `AuthenticationQuotaCapacityTests.cs`, `AuthenticationQuotaConfigurationTests.cs`, `AuthenticationQuotaRecoveryContractTests.cs`. `CFG/RateLimitingOptions.cs` solo recibe aclaración XML de null/activación, sin modificar su forma o binding. Documentación solo task/memoria. Modelos puros en Application por responsabilidad; ningún store ni código Redis en RDS. |
| Dimensionamiento aprobado | **MaxActivePartitions=5000**, global por ambiente/finalidad de cuotas, compartido entre siete cuotas/instancias, no por operación/ámbito/revisión ni equivalente a usuarios. **AuthenticationQuotaMemoryBudgetBytes=67108864=64 MiB**, presupuesto inicial académico de diseño que incluye buckets, eventos, registro, metadatos y housekeeping; no garantía del proceso Redis completo/OS/API/otros subsistemas ni selección AWS. No se deriva de la fixture ni se afirma capacidad empresarial/productiva definitiva. |
| Activación y Options | `AuthenticationQuotaCapacityDescriptor.ForAcademicActivation` exige 5000 explícito y devuelve snapshot inmutable con presupuesto 67108864; null/0/negativos/otro valor no activan esa configuración, sin default correctivo. El constructor general positivo permite modelar transiciones futuras/sintéticas, NO aprueba otros topes. Las Options/JSON preparados se conservan compatibles: el JSON continúa null=NO DIMENSIONADO/no apto para activar cuotas; no se confunde implementar el contrato con activar el host. Validador estructural 008 intacto. |
| Identidad | Se reutiliza `AuthenticationQuotaPartitionIdentity(Operation, ScopeType, OpaqueId)` de 052. Misma identidad cuenta una vez; operaciones/ámbitos distintos cuentan distinto aun si coinciden sus strings opacos. Sin PolicyRevision, presupuesto/capacidad, clave HMAC, PII raw ni RedisKey en identidad. Validar sintaxis no acredita que OpaqueId proceda de 053; esa procedencia sigue siendo precondición del store futuro. |
| Actividad y retención | `AuthenticationQuotaPartitionState` captura definición, máximo timestamp de eventos, versión técnica y verificabilidad. Activa si existe evento en (sharedNow-Window,sharedNow]; en borde inferior es inactiva; ausencia solo cuenta como inactiva con estado acreditado; evento futuro o estado incierto produce actividad desconocida, nunca plaza libre. Horizonte finito último evento+ventana, sin margen nuevo/de revocación; overflow se rechaza. Rechazos no mutan/refrescan estado. StateVersion no es PolicyRevision ni identidad. |
| Conteo y capacidad | `AuthenticationQuotaCapacityState` distingue actividad global y plazas residuales pendientes de liberación; ambas siguen cargadas hasta limpieza segura. `Evaluate` deduplica requisitos/observaciones coherentes y calcula existentes/nuevos; contradicciones, subconteo observable o estado no verificable fallan cerrados. Requiere observaciones/ausencias acreditadas en el mismo contexto autoritativo, no un contador/cache local. Con N nuevos exige ocupación contabilizada+N<=máximo, evitando overflow; con N=0 permite evaluación de existentes incluso tras reducción por debajo de ocupación. Ningún cálculo puro acredita el contador real completo. |
| Interop y orden | `EvaluateRequest` deriva identidades desde el request 052 y contrasta definiciones observadas. La comparación de snapshots y preflight conservan revisión/definiciones/topología de 052. Con estado verificable, `GetFailureAfterQuotaEvaluation` devuelve Rejected con RetryAfter positivo si cuota bloquea, antes de falta de capacidad; si cuota permite y capacidad no, Unavailable/503 sin RetryAfter. null solo significa ausencia de bloqueo contractual, nunca Admitted ni autorización HTTP. |
| Sin capacidad parcial | Evaluación insuficiente devuelve Unavailable y una intención de nuevas particiones vacía; no crea/reserva/consume ningún ámbito ni muta inputs. Cuando hay capacidad, la lista completa solo describe intención y NO puede persistirse si cuota/snapshot/confianza bloquean. En 021, validar, observar, comprobar cuotas/capacidad, registrar todas las nuevas particiones, consumir y retener deberán ser una única operación atómica incluyendo registro global. Sin rollback general Lua, compensación/retry ciego ni contador distribuido separado no atómico. Falta de resultado semántico tras despacho conserva Uncertain de 052; antes, Unavailable. Cuatro estados, sin quinto. |
| Recuperación y cleanup | `CanReleaseCapacity` exige estado definitivamente inactivo y verificable, identidad/versión/definición/timestamp sin cambios y serialización con consumos. No libera activo, incierto o renovado concurrentemente. El mantenimiento físico lazy/batch futuro será acotado, retirará residuos antes de liberar su cargo y no dependerá solo de notifications. No reset global, LRU/LFU, eviction de seguridad, compensación por timeout ni sacrificio de revocaciones. Los predicados NO ejecutan cleanup ni prueban por sí mismos serialización. |
| Pérdida/reconciliación | Contrato de pasos: detener admisiones globales, resolver in-flight, acreditar último consumo posible, reconstruir/validar o esperar ventana aplicable, reconciliar y reabrir solo con continuidad verificable. `CanReopenAfterLostCounters` deriva máximo de ventanas de descriptores afectados desde último consumo posible, no alerta/restart ni cuarentena JWT; falla si faltan tiempos/cese/in-flight/reconciliación. Reconstrucción requiere validación expresa y reconciliación. La autoridad 020 registra emisiones de tokens, NO prueba consumos de cuotas. Restart/registro vacío/PING no autorizan capacidad vacía. Riesgo totalmente silencioso no observable no eliminado. |
| Tiempo y privacidad | Las funciones reciben tiempo sintético/compartido, no consultan reloj local. 021 deberá usar el mismo Redis TIME para ventanas/actividad/capacidad/liberación y acreditar continuidad temporal. No ejecución TIME/TTL real en 054. Datos de capacidad: identidad opaca y metadatos técnicos; sin entradas raw ni secretos, logs, métricas o ToString con OpaqueIds. Observabilidad futura solo agregados/dimensiones acotadas de ocupación/máximo/agotamiento/cleanup/reconciliación; no telemetría implementada. |
| Snapshot/transición | `AuthenticationQuotaConfigurationSnapshot` vincula ambiente confiable, versión de configuración, referencia no secreta de identidad (no clave/hash HMAC), política y capacidad separada. `Matches` compara contenido completo, no solo versiones; divergencia/ausencia del activo causa Unavailable. `AuthenticationQuotaCapacityTransition` solo permite cambio operacional compatible con versión nueva; exige pasos de presupuesto aprobado, cese/fencing del snapshot viejo, resolver in-flight, conservar estado, activar snapshot único y continuidad. No requiere drenaje de ventanas para capacidad sola; no implementa coordinación ni acredita aprobación de presupuestos futuros. |
| Reducción/aumento/HMAC | Reducción conserva ocupación/particiones: existentes continúan y nuevas solo cuando quepa el conjunto. Aumento requiere memoria aprobada/configuración coordinada sin admitir snapshots divergentes; casos 4000/6000 son sintéticos, no nuevos topes aprobados. Capacidad es operacional, no parte de PolicyRevision ni de identidad; revisión no reinicia conteo. Cambios de cuotas/identidad no pueden usar transición de capacidad como atajo. HMAC/representación mantiene drenaje 053 sin current/previous activos y reconciliación de capacidad antes de reabrir. |
| Memoria pendiente | Lista inmutable `RequiredIntegrationEvidence` explicita ocho obligaciones, incluidas memoria medida con housekeeping y convivencia sin debilitar revocaciones; NO declara que ya se cumplan. 5000 por sí solo no prueba límite en bytes. 64 MiB de cuotas no acredita 64 MiB totales para Redis+revocaciones+overhead. Medir ocupación/picos reales; si no cabe, reducir MaxActivePartitions o aprobar más presupuesto/infraestructura antes de despliegue. Nunca eviction de revocaciones ni aceptación productiva sin acreditar presupuesto. |
| UT | **78 casos nuevos**, objetos puros y sin Moq: descriptor/5000/64 MiB/activación/null y límites, identidad/revisión/deduplicación, capacidad exacta/agotada/existing/multi-scope/sin efectos parciales, actividad/fronteras, residuos, overflow, observaciones contradictorias, prioridad 429/503, fail-closed/Uncertain, privacidad, snapshots completos, transiciones, recuperación/drenaje y obligaciones de memoria/revocaciones. Suite completa **291/291**, cero fallidas/omitidas. Las UT no prueban Redis/atomicidad, consumo real de memoria, cese global ni detector de pérdida. |
| Validación ejecutada | `dotnet restore Enver.Backend.slnx --locked-mode`: correcto, proyectos actualizados. `dotnet build Enver.Backend.slnx --no-restore`: correcto, **0 advertencias / 0 errores**, 43,86 s. `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build`: **291/291**. No ejecución IT/ST ni Docker/Testcontainers; build compila proyectos de pruebas existentes sin ejecutar sus fixtures. |
| Conservación | Program.cs y cuatro lockfiles con SHA-256 idénticos a baseline. csproj/solución, config JSON, fixtures/IT/ST, Infrastructure/Redis/Lua, contratos 052/053, spec/plan, constituciones/foundation, AGENTS/README intactos. Sin paquetes nuevos, DI productivo, Redis físico/TIME/TTL/Lua de cuotas, AWS/Terraform, ejecución de 021/055, stage/commit/push. Evidencia documental solo tras restore/build/UT verdes. |

**Distribución IT obligatoria aprobada (no eliminada):**

- **021:** Redis/Lua real, atomicidad conjunta de registro/capacidad/cuotas, carreras entre consumidores/instancias, límite global 5000 y creación concurrente sin sobrepasarlo, TTL/expiración/liberación serializada, reconciliación, timeout post-dispatch, pérdida/recuperación y ausencia de reset; medición física de buckets/eventos/registro/limpieza y convivencia básica con revocaciones. No declarar 021 completa trasladando obligaciones esenciales o fallos conocidos a 055.
- **055:** ampliación/regresión entre hosts y las siete cuotas, presión sostenida, capacidad global, limpieza y mediciones representativas de memoria/convivencia. La evidencia debe contrastar 64 MiB como presupuesto de cuotas y dimensionar la convivencia total antes de despliegue, sin asumir suficiencia del tamaño sintético de fixture.
- **Sin circularidad:** 054 contrato/UT satisface prerrequisito de diseño de 021; 021 implementa/prueba el store y 055 amplía evidencia distribuida; GATE-B no cierra sin las IT pendientes y sus demás condiciones. No se exige implementar el store dentro de 054 ni se concede autorización automática a 021/055. 063 conserva sus ensayos transversales de recuperación posteriores.

**Estado final: T-AUTH-054 COMPLETADA EN SU INCREMENTO CONTRACTUAL / PRERREQUISITO DE DISEÑO SATISFECHO. EVIDENCIA IT DISTRIBUIDA PENDIENTE EN T-AUTH-021/T-AUTH-055 PARA CIERRE DE GATE-B. T-AUTH-021 DESBLOQUEADA RESPECTO A 054 / NO INICIADA. T-AUTH-008 conserva startup productivo pendiente. GATE-B PENDIENTE / NO SATISFECHO. AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** Trabajo detenido sin iniciar otro incremento.

### 34.1. Actualización posterior de evidencia diferida tras el cierre de 021

El cierre documental autorizado del **10 de noviembre de 2026**, [E(T-AUTH-021)](#e-t-auth-021), actualiza el estado posterior sin sustituir el historial contractual anterior: **T-AUTH-054 — EVIDENCIA IT ESENCIAL ASIGNADA A T-AUTH-021: SATISFECHA EN EL ALCANCE REGISTRADO**.

| Obligación asignada a 021 | Evidencia acreditada y límite |
| --- | --- |
| Atomicidad y concurrencia reales | Una evaluación Lua conjunta de registro/capacidad/cuotas, consumidores independientes y carreras de creación sin sobrepasar 5000; sin consumo/creación parcial ante rechazo o capacidad insuficiente. |
| TTL/liberación/reconciliación | Expiración con Redis TIME, rechazo sin refrescar TTL, limpieza lazy con revalidación de candidatos y reconciliación/inventario cerrado; pérdida/marker incompleto fallan cerrados y se recuperan explícitamente. |
| Resultado desconocido y no-reset | Timeout real post-dispatch conserva Uncertain sin retry ciego ni inferencia por read-back; restart conserva eventos vigentes, requiere fence explícito y no reabre el guard original. |
| Memoria y revocaciones | Perfil sintético de **5000 particiones / 272700 eventos**, **24757168 bytes atribuibles** frente a **67108864 bytes** aprobados: **DENTRO DEL PRESUPUESTO EN EL PERFIL MEDIDO**. Coexistencia básica con store real de revocaciones y expiración absoluta preservada. |

**T-AUTH-055 permanece PENDIENTE / NO EJECUTADA:** presión sostenida, regresión distribuida más amplia entre hosts, siete cuotas bajo carga extendida, perfiles prolongados y convivencia bajo presión ampliada. No se trasladan fallos esenciales conocidos de 021; lo pendiente es ampliación del alcance ensayado, no evidencia ya declarada. No hay suficiencia absoluta futura, benchmark productivo, capacidad empresarial ni dimensionamiento definitivo del servicio Redis. GATE-B permanece **PENDIENTE / NO SATISFECHO**.

### 34.2. Actualización posterior de evidencia diferida tras el cierre de 055

El cierre documental autorizado del **10 de noviembre de 2026**, [E(T-AUTH-055)](#e-t-auth-055), actualiza el estado posterior sin sustituir el historial contractual anterior: **T-AUTH-054 — AMPLIACIÓN DE EVIDENCIA ASIGNADA A T-AUTH-055: SATISFECHA EN EL ALCANCE REGISTRADO**.

| Obligación ampliada asignada a 055 | Evidencia acreditada y límite |
| --- | --- |
| Presión distribuida sostenida | P1: 3000 intentos REGISTER repartidos 1500/1500 entre dos hosts, tres tramos de 30 s a concurrencia 8, 18 filas de fase sin `Unavailable`/`Uncertain`; P2: 5000 particiones, **101993** eventos sembrados y **102000** de pico, tres ciclos de 20 s con 256 intentos/ciclo y revocaciones concurrentes. |
| Siete cuotas bajo carga extendida | Matriz **10/10** de límites, ventanas, RetryAfter y operación fallida admitida; sin sanción ni bloqueo perpetuo por rechazo. |
| Capacidad global 5000 | Tres rondas con **4998 → carrera de 16 intentos (1 admitido / 15 no disponibles) → 5000**, `two-new` no disponible, ámbitos existentes evaluables, expiración real y reasignación persistente entre rondas; nunca se superó 5000. |
| Limpieza y TTL | Lazy cleanup acotado a 3 candidatos por evaluación, 5 residuos liberados en dos evaluaciones, candidato renovado preservado y consumo concurrente sin liberar eventos vigentes. |
| Fallos, OOM y tiempo | Timeout real post-dispatch → `Uncertain` sin retry; OOM real de Redis → `Uncertain`, sin admisión ni mutación parcial (`DIRTY=0`) y admisión tras liberar memoria; reinicio sin reset, fence explícito y recuperación validada. |
| Memoria y revocaciones | Pico atribuible **9376224 bytes** frente a **67108864 bytes** aprobados (14 %) en el perfil P2, frontera de capacidad **1969384 bytes** y `noeviction`; **768** revocaciones creadas (384 largas preservadas / 384 cortas expiradas) coexistiendo con la presión. |
| Seguridad de origen y configuración | ST **6/6**: forwarded falsificado no crea cupo, NAT/IPv4-mapped/IPv6 comparten partición física, privacidad de claves/miembros/diagnósticos, estados cerrados sin despacho, revisión forjada y snapshot divergente fail-closed (`DIRTY=0`, sin particiones). |

Límites: entorno de test con Redis real y dos consumidores lógicos; no acredita red/LB productivos, forwarding productivo, hosts físicos independientes, AWS, Terraform, failover universal, benchmark empresarial ni dimensionamiento definitivo. GATE-B permanece **PENDIENTE / NO SATISFECHO** y AUTH-027 **NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**.

## 35. Evidencia Redis distribuida y cierre de T-AUTH-021

<a id="e-t-auth-021"></a>

### E(T-AUTH-021)

Registro del **10 de noviembre de 2026** mediante BUILD DOCUMENTAL CONTROLADO expresamente autorizado tras validación técnica verde. **Trazabilidad oficial:** AUTH-028–030/038–040; PLD-AUTH-02/15/16/18; AC-14/16/18; dependencias 012/016/052–054; F3/GATE-B. Se acredita además la evidencia IT esencial de 054 asignada a 021 (§34.1), sin cambiar contratos 052/053/054, spec/plan ni aceptar integralmente el gate. Rama `feature/authentication`, HEAD **`123503c1d2a80f7e0bde2a66c8142a17c6b6604b`**; implementación/tests de 021 presentes como cambios no staged, no como commit nuevo. La presente entrega solo edita `task.md` y `memoria.md`, sin repetir pruebas.

| Campo | Evidencia técnica previa acreditada y alcance |
| --- | --- |
| Implementación | `Enver.API/Infrastructure/Redis/RedisAuthenticationQuotaStore.cs`, `RedisAuthenticationQuotaConfiguration.cs`, `RedisAuthenticationQuotaKeyBuilder.cs`, `RedisAuthenticationQuotaScriptProtocol.cs`, `RedisAuthenticationQuotaLifecycle.cs` y `Scripts/ConsumeAuthenticationQuota.lua`. `Enver.API/Enver.API.csproj` cambia únicamente para incluir Lua como EmbeddedResource. Se conserva el puerto `IAuthenticationQuotaStore`; adapter aislable, sin DI/host productivo ni endpoints. |
| Pruebas y harness | Cuatro archivos nuevos `RedisAuthenticationQuota*Tests.cs` en UnitTests (KeyBuilder, ScriptProtocol, Store, Lifecycle); cinco archivos en IntegrationTests (Store, Capacity, Memory, Recovery y TestScope); `RedisAuthenticationQuotaSecurityTests.cs` en SecurityTests. Fixture compartida añade únicamente la propiedad read-only `RedisContainerId` para diagnóstico seguro de restart. |
| Entorno | .NET 10 y Redis real Docker/Testcontainers **`redis:8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba`**, imagen/digest conservados. Consumidores independientes comparten Redis aislado por ambiente de test; no equivale a AWS, Cluster desplegado ni failover universal. La regresión completa usa además las dependencias locales existentes de la fixture. |
| Lua y tiempo | Protocolo/script versionado **`enver.auth.quota / 1`**; validaciones de tipos/argumentos/snapshot/generación antes de mutar. Una evaluación Lua de consumo y **un Redis TIME**, microsegundos `seconds*1000000+microseconds`; aritmética entera exacta hasta `2^53−1`. Sorted sets implementan sliding windows **(nowUs-windowUs, nowUs]**, sin reloj local para decidir ventanas. |
| Evaluación conjunta | Todas las cuotas/ámbitos requeridos por la operación se evalúan conjuntamente con el mismo now y el mismo **InternalAttemptId UUID-N**, un evento por ámbito al admitir. Rechazo o falta de capacidad no insertan eventos ni crean particiones parciales. No se ofrece exactly-once ni rollback general Lua ante errores tras escritura; un marker de mutación incompleta falla cerrado. |
| Identidad y hash slot | Claves `enver:{<environment>:auth:ratelimit}:bucket:<operation>:<scope>:<opaqueId>`, `registry` y `metadata` comparten el hash tag `{<environment>:auth:ratelimit}` y se declaran en KEYS. Esto acredita diseño de slot común, no selecciona infraestructura Cluster. Identidad estable sin PolicyRevision/generación; recibe OpaqueIds de 053, nunca PII raw ni claves HMAC. Cuotas, revisiones y contratos aprobados no cambian. |
| RetryAfter | Resultado Rejected conserva RetryAfter positivo derivado de Redis TIME, evento relevante n−limit+1 y máximo entre ámbitos bloqueantes, redondeado hacia arriba a segundos. Rechazo verificable precede agotamiento de capacidad. Sin RetryAfter artificial para Unavailable/Uncertain. |
| Capacidad/concurrencia | Tope global **5000** por ambiente/finalidad, no por proceso/operación/revisión. Registro sorted set y metadata de snapshot/generación/conteo/horizonte/estado/versión/marker participan en la misma evaluación Lua. IT directas acreditan existentes al máximo, insuficiencia conjunta sin creación parcial, 2500 admisiones reales que crean 5000 particiones y carreras de creación entre consumidores sin excederlo. |
| TTL/cleanup | Horizonte último evento+ventana; TTL absoluto **PEXPIREAT**, sin refresh por rechazo. Lazy cleanup toma hasta tres pistas y revalida actividad/estado dentro de Lua; candidatos renovados no se liberan por pistas stale. Control explícito mantiene reconciliación por lotes e inventario cerrado acotado antes de activación, incluidos residuos sin registro. |
| Lifecycle/fencing | Inicialización explícita solo de ambiente verificado como nuevo; Redis vacío no lo demuestra. Fence cambia generación y cierra admisión; drenaje desde último consumo posible, reconciliación y activación explícita. Generación anterior queda invalidada; activación temprana falla. Restart/PING/consumo no reabren guard ni autoridad 020. Las IT acreditan pérdida detectada de bucket/registro/metadata, marker incompleto, residuos y recovery ensayado, no detección universal de pérdidas silenciosas. |
| Unavailable/Uncertain | Pre-dispatch no verificable →Unavailable; falta de respuesta semántica confiable post-dispatch →Uncertain, incluido timeout real. Sin retry ciego, compensación ni lectura posterior para inferir el resultado. Se conservan Admitted/Rejected/Unavailable/Uncertain de 052 y las fronteras local/global fail-closed. |
| Memoria medida | Perfil **sintético** de **5000 particiones / 272700 eventos**, ventana de preparación de 15 minutos solo en test. Buckets **24027944 B**, registro **728112 B**, metadata **1112 B**; total atribuible **24757168 bytes**, presupuesto aprobado **67108864 bytes**: **DENTRO DEL PRESUPUESTO EN EL PERFIL MEDIDO**. Medición con MEMORY USAGE; INFO MEMORY diagnóstico de antes/durante/después. No garantiza producción, presupuesto total del proceso Redis, picos futuros, suficiencia absoluta ni capacidad empresarial. |
| Revocation coexistence | IT real mantiene revocación observable por segundo consumidor y su expiración absoluta durante preparación/medición/limpieza selectiva de cuotas; conserva noeviction. Coexistencia básica acreditada, no presión ampliada/sostenida ni dimensionamiento definitivo. |

### 35.1. Incidencias de validación y corrección exclusiva del harness/test

- Se corrigió el ownership de conexiones: `RedisAuthenticationQuotaTestScope` es único propietario, limpia keys con conexión viva antes de disponer conexiones, cleanup idempotente y preservación conjunta de error de prueba/cleanup mediante AggregateException; no se ocultan fallos.
- Se observó **mismo Testcontainer** tras StopAsync/StartAsync con **puerto publicado diferente** (diagnóstico inicial `127.0.0.1:62864` → `127.0.0.1:62879`). El harness reconstruye configuración de test desde el endpoint actual, crea multiplexer nuevo, comprueba coincidencia de endpoint, conexión y PING acotado antes del fence. PING solo acredita readiness de test, no recuperación de confianza productiva ni sustitución del detector.
- La aserción fija de dos eventos usaba ventana de 1 s, menor que el restart. Se separa expiración natural (IT `Window_both_sides_and_expiry_release_capacity_with_real_TIME`) de no-reset: la focal usa **30 s sintéticos**, observa TIME y scores read-only antes de consumir y exige preservación de miembros/scores aún vigentes. Las siete cuotas productivas permanecen intactas; no se cambió `fenced` a `uncertain`.
- Evidencia focal verde: score inicial **1794298278050177 µs**; Redis TIME antes **1794298278076815**, después **1794298283343009**; intervalo desde evento **5292832 µs**. En Email/Origin: **raw=1/vigente=1/expirado=0** después del restart; tras consumo, **raw=2/vigentes=2/expirados=0**, nuevo score **1794298283444554**, TIME **1794298283460322**. Con 1 s el inicial habría expirado; no se atribuye retrospectivamente reset sin evidencia. No-reset **OK**, fence **`fenced`** semántico/categoría `ok`, recovery explícito **OK**, guard original cerrado y cleanup **OK**. Sin cambios de semántica productiva para corregir estas incidencias.

### 35.2. Validación ejecutada antes del cierre documental

| Comando / verificación | Resultado acreditado |
| --- | --- |
| `dotnet restore Enver.Backend.slnx --locked-mode` (implementación inicial) | Correcto; cuatro lockfiles conservados. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName=Enver.API.IntegrationTests.RedisAuthenticationQuotaRecoveryTests.Controlled_restart_does_not_reopen_guard_or_reset_quota_and_explicit_fence_recovers" --logger "console;verbosity=detailed"` | **1/1** tras corrección; readiness/PING, no-reset, fence, recovery y cleanup verdes. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~Enver.API.IntegrationTests.RedisAuthenticationQuotaRecoveryTests" --logger "console;verbosity=normal"` | Clase recovery **10/10**. |
| `dotnet build Enver.Backend.slnx --no-restore` | Correcto, **0 advertencias / 0 errores**. |
| `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build` | **333/333**, sin fallidas/omitidas; 42 casos nuevos de 021. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --logger "console;verbosity=normal"` | **78/78**, sin fallidas/omitidas; incluye 26 casos nuevos de 021, recovery, capacidad 5000, memoria y coexistencia. |
| `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build` | **63/63**, sin fallidas/omitidas; incluye ocho casos preparados de 021 sobre identidades/diagnósticos, fronteras cerradas y configuración/revisión forjadas. |
| Conservación | SHA-256 previo al cierre confirma intactos los seis archivos implementación/Lua durante correcciones focales, `Program.cs` y cuatro lockfiles; sin paquetes nuevos. No se repiten verificaciones técnicas por esta edición documental. |

### 35.3. Estado y límites del cierre

**T-AUTH-021: COMPLETADA / EVIDENCIA REDIS DISTRIBUIDA REGISTRADA. T-AUTH-054: EVIDENCIA IT ESENCIAL ASIGNADA A T-AUTH-021 SATISFECHA EN EL ALCANCE REGISTRADO. T-AUTH-055: PENDIENTE. T-AUTH-022: NO INICIADA. GATE-B: PENDIENTE / NO SATISFECHO. AUTH-027: NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** 008 conserva verificación productiva de startup pendiente.

Pendientes de 055: regresión distribuida más amplia, siete cuotas bajo carga extendida, presión sostenida, perfiles prolongados y coexistencia bajo presión ampliada. Pendientes de 022 y del pipeline/infraestructura: controles operativos y aceptación integral aplicables; este adapter no integra DI/host productivo, JWT/endpoints ni recuperación operacional automática. No se acredita AWS/Terraform, TLS/IAM productivos, Redis productivo dimensionado definitivamente, failover universal o eliminación de pérdida catastrófica totalmente silenciosa no observable. Ninguna tarea posterior se inicia por este cierre.

Registro documental exclusivo en este archivo y [memoria §42](../../memoria.md#42-cierre-de-t-auth-021-y-evidencia-redis-distribuida); implementación/tests actuales preservados sin cambios adicionales, sin stage, commit ni push. Trabajo detenido.

## 36. Evidencia distribuida ampliada y cierre de T-AUTH-055

<a id="e-t-auth-055"></a>

### E(T-AUTH-055)

Registro del **10 de noviembre de 2026** mediante BUILD FOCALIZADO DE TEST autorizado en incrementos delimitados. Se crearon cinco archivos de prueba y se corrigieron únicamente modelos/expectativas del harness; el código productivo, `Program.cs`, paquetes, lockfiles e imágenes/digests permanecieron intactos. Rama `feature/authentication`, HEAD `d6e764460bf5e163a067f073c937e9b16eb036f0`; cambios de test no staged, sin commit ni push.

**Trazabilidad oficial:** AUTH-026,028–030,038–040,042,043; PLD-AUTH-08,15–18,20; AC-14–16,18; dependencias 015/021/052–054; F11/GATE-B, regresión GATE-G.

| Campo | Evidencia acreditada y alcance |
| --- | --- |
| Archivos de prueba | `tests/Enver.API.IntegrationTests/DistributedQuotaTestScope.cs`, `DistributedQuotaTests.cs`, `DistributedQuotaPressureTests.cs`, `DistributedQuotaFailureTests.cs` y `tests/Enver.API.SecurityTests/DistributedQuotaSecurityTests.cs`. Sin cambios en producto, configuración, cuotas, ventanas, store, Lua o lifecycle. |
| Entorno | .NET 10, Redis real Docker/Testcontainers conservado (`redis:8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba`), dos consumidores lógicos sobre el mismo Redis aislado por ambiente de test; `noeviction` y presupuesto de cuotas 67108864 bytes. |
| P1 — ventanas académicas | **3000** intentos REGISTER repartidos **1500/1500** entre hosts A/B, tres tramos de **30 s** a concurrencia 8 (`ELAPSED_MS=30008/30011/30003`), 18 filas de fase con `Unavailable=0` y `Uncertain=0`; `ACCOUNTED=590→624→624`, `RESIDUES=0`; memoria atribuible máxima **430744 bytes**. Duración 1 m 30 s–1 m 33 s. |
| P2 — presión mixta | **5000** particiones; **101993** eventos sembrados + **7** reservados para spares = **102000** de pico, `ACCOUNTED=5000`, `ACTIVE=5000`, `RESIDUES=0`; tres ciclos de 20 s con **256** intentos/ciclo y **768** revocaciones (384 largas con TTL preservado, 384 cortas expiradas); `P2-after-selective-cleanup ATTRIBUTED=0`; `noeviction`; ninguna fase `Unavailable`/`Uncertain` inesperada. Duración 1 m 18 s–1 m 31 s. |
| Capacidad global 5000 | Tres rondas: `ACCOUNTED=2→4998` (ronda 1) y `4→4998` (rondas 2/3 por reasignación persistente), carrera de 16 intentos con **1 admitido / 15 no disponibles**, `MAX_ACCOUNTED=5000`, `two-new=Unavailable`, ámbitos existentes `Admitted`, expiración real y `ACCOUNTED=2` reasignadas por ronda; nunca se superó 5000. Memoria en frontera **1969384 bytes**. Duración 49–50 s. |
| Limpieza | Lazy cleanup con **3** candidatos por evaluación, **5** residuos liberados en dos evaluaciones, candidato renovado preservado (`RENEWED_CANDIDATE_PRESERVED=true`), consumo concurrente sin liberar eventos vigentes. Duración 235 ms–2 s. |
| Siete cuotas | Matriz **10/10**: límites por operación/ámbito, ventanas, RetryAfter y continuación funcional tras intento fallido admitido; sin sanción ni rechazo perpetuo. |
| Timeout/Uncertain | Timeout real post-dispatch → `Uncertain`, sin retry ciego ni inferencia por read-back; pre-dispatch no verificable → `Unavailable`. |
| OOM real | Redis bajo `maxmemory` real: `USED_MEMORY=34555992 > MAXMEMORY=16777216`, `noeviction`; resultado `Uncertain`, `DISPATCHES=1`, `DIRTY=0`, sin admisión ni mutación parcial; admisión correcta tras liberar memoria. |
| Restart/fence/recovery | Reinicio sin reset de eventos vigentes, guard original cerrado, fence explícito con generación nueva y recuperación validada; sin reapertura por PING/restart. |
| Autoridad 020 | Integración de dos coordinadores conservada; cuarentena/activación explícita sin apertura implícita de capacidad. |
| Revisión/snapshot | `PolicyRevision` forjada → `Unavailable` sin despacho (`DISPATCHES=0`, `DIRTY=0`, `REGISTRY=0`); snapshot divergente enlazado realmente al store → `Unavailable` tras **1** despacho rechazado por Lua antes de mutar (`DIRTY=0`, `REGISTRY=0`, sin particiones). |
| Seguridad de origen y privacidad | ST **6/6**: forwarded falsificado no crea cupo (`REGISTRY=2` solo para los ámbitos admitidos, `DISPATCHES=1`, `DIRTY=0`), NAT/IPv4-mapped/IPv6 comparten partición física, claves/miembros/diagnósticos sin identidad ni material de clave, estados local/global cerrados sin despacho. |
| Memoria y revocaciones | Pico atribuible P2 **9376224 bytes** / **67108864** (14 %); frontera de capacidad **1969384 bytes**; `used_memory` Redis ~11,8 MB, pico ~11,9 MB; 768 revocaciones coexistentes y expiración absoluta preservada. No es benchmark ni dimensionamiento definitivo. |

### 36.1. Correcciones exclusivas del harness (sin cambios productivos)

- `DistributedQuotaTestScope.RunAsync` admite presupuesto opcional de espera; P1 180 s, P2 300 s, capacidad 240 s, limpieza 90 s y casos de fallo 180 s. Un `TimeoutException` falla la prueba; no se captura para simular éxito. Perfiles y ventanas no se redujeron.
- P2 reserva **7** eventos (2 REGISTER + 3 LOGIN + 2 LOGOUT) para admitir spares en ámbitos existentes: `SEEDED_EVENTS=101993`, `PEAK_EVENTS=102000`.
- El JTI sintético de revocaciones se genera como Base64url canónico de 32 bytes (43 caracteres, sin padding), conforme a `RedisKeyBuilder.ValidateJti`; antes se usaba un GUID hexadecimal rechazado correctamente por el producto.
- Capacidad calcula el faltante real hasta 4998 (`missing = 4998 - accounted`), reconociendo que las 2 particiones reasignadas de la ronda anterior siguen vivas; la persistencia entre rondas se conserva como evidencia positiva.
- ST de spoofing compara la identidad opaca de cuota (el tipo canónico no implementa igualdad por valor) y el snapshot divergente se inyecta mediante `NewStore(..., configuration)`; se añade `ITestOutputHelper` solo para diagnósticos agregados sin identidades ni OpaqueIds.

### 36.2. Validación ejecutada

| Comando / verificación | Resultado acreditado |
| --- | --- |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; todos los proyectos actualizados, lockfiles conservados. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build` | **333/333**, sin fallidas ni omitidas. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build` | **97/97**, sin fallidas ni omitidas. |
| `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build` | **69/69**, sin fallidas ni omitidas. |
| Focales previas | P1, P2, capacidad, limpieza, clase pressure **4/4** y ST focal **6/6** verdes antes de la regresión completa. |
| Conservación | Producto, `Program.cs`, lockfiles, paquetes e imágenes/digests intactos; sin stage, commit ni push. |

### 36.3. Estado y límites del cierre

**T-AUTH-055: COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA. T-AUTH-054: AMPLIACIÓN DE EVIDENCIA ASIGNADA A T-AUTH-055 SATISFECHA EN EL ALCANCE REGISTRADO (§34.2). T-AUTH-022: NO INICIADA. GATE-B: PENDIENTE / NO SATISFECHO. AUTH-027: NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.** 008 conserva verificación productiva de startup pendiente.

No se acredita red/LB productivos, forwarding productivo, hosts físicos independientes, AWS/Terraform, Redis productivo dimensionado definitivamente, failover universal, benchmark empresarial ni eliminación del riesgo residual de pérdida catastrófica totalmente silenciosa no observable. Ninguna tarea posterior se inicia por este cierre.

Registro documental exclusivo en este archivo y [memoria §43](../../memoria.md#43-cierre-de-t-auth-055-y-evidencia-de-t-auth-054); implementación/tests preservados sin cambios adicionales, sin stage, commit ni push. Trabajo detenido.

## 37. Evidencia operativa Redis y cierre de T-AUTH-022

<a id="e-t-auth-022"></a>

### E(T-AUTH-022)

Registro del **10 de noviembre de 2026** mediante **BUILD CONTROLADO DE VERIFICACIÓN** expresamente autorizado. Se crearon únicamente tres archivos de prueba y, tras la validación verde, se actualizan este documento y `memoria.md`. No se modificó producto, `Program.cs`, `Enver.API.csproj`, `Enver.Backend.slnx`, `tests/Shared/**`, lockfiles, paquetes, configuración productiva ni imágenes/digests. Rama `feature/authentication`, HEAD base **`3ae6114178258f70a749adc304e6b5ba15df942e`**; cambios nuevos no staged, sin commit ni push.

**Trazabilidad oficial:** AUTH-022–030/038/039/042/043; PLD-AUTH-07–09/15/20; AC-11/12/14–16/18; dependencias 015/017/020/021/055; F3/GATE-B, soporte GATE-G.

| Campo | Evidencia acreditada y alcance |
| --- | --- |
| Archivos nuevos | `tests/Enver.API.IntegrationTests/RedisOperationalScope.cs` (harness de dos hosts lógicos), `tests/Enver.API.IntegrationTests/RedisOperationalScopeTests.cs` (3 IT) y `tests/Enver.API.SecurityTests/RedisOperationalScopeSecurityTests.cs` (2 ST). Sin cambios en producto, `Program.cs`, csproj/slnx, lockfiles, paquetes, fixture compartida ni imágenes/digests. |
| Entorno | .NET 10 y Redis real Docker/Testcontainers conservado: `redis:8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba`; Mongo/DynamoDB Local de la fixture reutilizados sin actualización deliberada. Entorno de test aislado por sufijo único; no representa hosts físicos, AWS, LB ni despliegue administrado. |
| Dos hosts de test | Dos `WebApplicationFactory<AuthenticationOptions>` con `ServiceProvider`, `RedisConnections`/multiplexer, `RedisSecurityStateGuard` y `RedisTokenRevocationStore` independientes y no compartidos por referencia; ambos usan la misma cadena Redis real y el mismo namespace de ambiente. Se acredita independencia lógica, no procesos/máquinas distintas. |
| Revocación A→B | JTI sintético Base64url de 32 bytes (43 caracteres, sin padding). B observa `NotRevoked` antes de la escritura; A confirma `Confirmed`; B observa `Revoked` con su propio store/conexión. Una segunda revocación desde A con horizonte menor conserva la expiración previa y B sigue observando `Revoked`; no hay caché negativa local, `WAIT`, read-back productivo, retry ni endpoint logout/JWT real. |
| TTL/retención | Se verifica valor `1`, expiración absoluta `PEXPIREAT` y TTL positivo; la repetición con menor horizonte **no reduce** la retención (`expireAfterLower >= expireAfterFirst`). La evidencia no sustituye la batería de 017 ni introduce extensión productiva. |
| Perfil operativo | `CONFIG GET maxmemory-policy` y `INFO MEMORY` sobre Redis real: **`noeviction`**; `MAXMEMORY_TOTAL_BYTES=67108864`; `USED_MEMORY_BYTES=1397640`; `USED_MEMORY_PEAK_BYTES=1397640`; `HEADROOM_BYTES=65711224`; `INFO maxmemory_policy=noeviction`. Medición de una ejecución focal previa a esta edición documental, no benchmark ni sizing. |
| Presupuesto de cuotas | Se cita por separado el presupuesto aprobado de cuotas **67108864 bytes** (054), con `QUOTA_BUDGET_SCOPE=CUOTAS_ONLY`; no se confunde con `MAXMEMORY` total de la fixture (`MAXMEMORY_SCOPE=FIXTURE_TOTAL`) ni se presenta como memoria medida de cuotas en esta tarea. |
| Separación ambiente/propósito | Claves reales de revocación `enver:{environment}:auth:revoked` y namespace conceptual de cuotas `enver:{environment}:auth:ratelimit`; configuración `RevocationPurpose=auth:revocation` y `RateLimitingPurpose=auth:ratelimit`. Se comprueba prefijo de ambiente común, propósitos distintos y que una clave de otro ambiente no queda bajo el namespace actual. No se afirma aislamiento físico. |
| Permisos/administración | `allowAdmin=true` se usa únicamente en la conexión administrativa del harness para `CONFIG GET`/`INFO`; las conexiones de los hosts no lo reciben. No se modifica configuración productiva ni se declara que una conexión productiva deba exponer permisos administrativos. |
| Cierre compartido/no bypass | La señal `ConnectionFailed` se inyecta en ambos guards. Con una marca de revocación previamente confirmada y Redis respondiendo a `PING`, ambos hosts deniegan lectura (`Unavailable`) y escritura (`Unavailable`), sin `NotRevoked`, fallback local, caché negativa permisiva ni éxito ficticio. La prueba acredita **reacción al cierre inyectado**, no detección automática del evento físico; el detector real conserva la evidencia de 019/055. |
| Privacidad diagnóstica | El reporte agregado solo contiene estados, booleanos, conteos, memoria agregada, host lógico A/B y patrones conceptuales. Verifica ausencia de JTI, OpaqueId, email, IP, `sub`, JWT, clave HMAC, contraseña/connection string Redis y secretos sintéticos de la fixture (`ContainsSyntheticSecret=false`). |
| TLS/ACL | Testcontainers local no acredita TLS ni ACL productivos. No se implementan TLS/ACL locales artificiales; su aplicabilidad futura queda para infraestructura/tareas correspondientes (AUTH-033/046/AD-11/068). No se marcan como satisfechos. |
| Límites/no-claims | Sin endpoints, JWT funcional final, logout HTTP, composición productiva 056/057, pipeline 058, AWS/Terraform, hosts físicos, LB, failover cloud, dimensionamiento definitivo ni garantías productivas. No se repiten P1/P2, OOM, capacidad 5000, siete cuotas ni las baterías completas de 017–021/053/055; se citan como evidencia previa. AUTH-027 no se certifica. |

### 37.1. Validación ejecutada

| Comando / verificación | Resultado acreditado |
| --- | --- |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; todos los proyectos actualizados, lockfiles conservados. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RedisOperationalScopeTests"` | **3/3**, sin fallidas ni omitidas. |
| `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build --filter "FullyQualifiedName~RedisOperationalScopeSecurityTests"` | **2/2**, sin fallidas ni omitidas. |
| `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build` | **333/333**, sin fallidas ni omitidas; cero UT nuevas. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build` | **100/100**, sin fallidas ni omitidas. |
| `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build` | **71/71**, sin fallidas ni omitidas. |
| Conservación | Producto, `Program.cs`, csproj/slnx, lockfiles, paquetes, fixture compartida e imágenes/digests intactos; sin stage, commit ni push. |

### 37.2. Estado y límites del cierre

**T-AUTH-022: COMPLETADA / EVIDENCIA OPERATIVA REDIS REGISTRADA. GATE-B: PENDIENTE / NO SATISFECHO. AUTH-027: NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. T-AUTH-023: NO INICIADA. T-AUTH-056: NO INICIADA. T-AUTH-057: NO INICIADA. T-AUTH-058: NO INICIADA.** 008 conserva verificación productiva de startup pendiente.

La evidencia acredita el alcance operativo local declarado de 022, no la aceptación integral de GATE-B. No se cierra GATE-B automáticamente, no se satisface AUTH-027, no se autoriza 023/056/057/058 y no se presentan TLS/ACL productivos, hosts físicos, AWS, LB ni operación del servicio administrado como verificados. Cualquier tarea posterior requiere autorización delimitada propia.

Registro documental exclusivo en este archivo y [memoria §44](../../memoria.md#44-cierre-de-t-auth-022-y-evidencia-operativa-redis); sin stage, commit ni push. Trabajo detenido.

<a id="cierre-gate-b"></a>

## 38. Cierre formal de GATE-B — Redis

El **10 de noviembre de 2026**, tras la evaluación formal de evidencia y la regresión vigente registrada en T-AUTH-022, **GATE-B — Redis queda SATISFECHO / CERRADO** como checkpoint local de componentes Redis para el entorno declarado de test. El cierre no declara aceptación productiva, no certifica AUTH-027 y no autoriza tareas posteriores.

| Campo | Registro de cierre |
| --- | --- |
| Fecha | 10 de noviembre de 2026. |
| Branch | `feature/authentication`. |
| HEAD base del cierre | `6e3fed42d2813bd8fdd71ab928dbb046339bee4b`. |
| Working tree al evaluar | Limpio. |
| Gate | GATE-B — Redis. |
| Propósito | Verificar el estado compartido Redis, la revocación/retención, las cuotas conjuntas, el fail-closed distribuido, la detección/cierre/recuperación y los límites operativos del entorno declarado. |
| Decisión formal | **GATE-B — Redis: SATISFECHO / CERRADO** en el entorno local declarado. |
| Alcance | Checkpoint local de componentes Redis; entorno declarado de test con hosts lógicos independientes y dependencias reales de Testcontainers. No es aceptación productiva. |

### 38.1. Entradas requeridas y satisfechas

Se registran como satisfechas, sin alterar sus estados individuales:

- **GATE-A** como base, ya **SATISFECHO / CERRADO** en §25.
- **T-AUTH-016:** conexión Redis y namespaces seguros.
- **T-AUTH-017:** revocación atómica y retención.
- **T-AUTH-018:** admisión confiable y fail-closed.
- **T-AUTH-019:** detección según modelo de fallos.
- **T-AUTH-020:** coordinación de cierre, emisión y recuperación segura.
- **T-AUTH-021:** almacén distribuido de cuotas.
- **T-AUTH-022:** verificación Redis distribuido y protección operativa.
- **T-AUTH-052:** contrato de cuotas, PolicyRevision y Retry-After.
- **T-AUTH-053:** pseudonimización y origen confiable.
- **T-AUTH-054:** cardinalidad y recuperación de cuotas.
- **T-AUTH-055:** verificación completa de cuotas entre hosts.

### 38.2. Evidencias utilizadas

No se duplican contenidos; se referencian las evidencias registradas:

- [E(T-AUTH-016)](#e-t-auth-016): conexión, reutilización y namespace.
- [E(T-AUTH-017)](#e-t-auth-017): revocación, Lua, TTL y lectura.
- [E(T-AUTH-018)](#e-t-auth-018): fail-closed, incertidumbre y ausencia de bypass.
- [E(T-AUTH-019)](#e-t-auth-019): señales reales, detector y cierre local.
- [E(T-AUTH-020)](#e-t-auth-020): autoridad, CAS, coordinación y reapertura segura.
- [E(T-AUTH-021)](#e-t-auth-021): Redis/Lua de cuotas, atomicidad, capacidad, memoria y recovery.
- [E(T-AUTH-022)](#e-t-auth-022): dos hosts lógicos, revocación A→B, `noeviction`, memoria/headroom, cierre sin bypass y privacidad.
- [E(T-AUTH-052)](#e-t-auth-052): contrato de siete cuotas, Redis TIME, Retry-After y transición.
- [E(T-AUTH-053)](#e-t-auth-053): HMAC, OpaqueIds, origen canónico y transición.
- [E(T-AUTH-054)](#e-t-auth-054): MaxActivePartitions, presupuesto de cuotas y distribución de evidencia.
- [E(T-AUTH-055)](#e-t-auth-055): presión distribuida, siete cuotas, capacidad, limpieza, OOM y recovery.

### 38.3. Matriz de condiciones del gate

| Condición oficial | Evidencia | Estado | Resultado |
| --- | --- | --- | --- |
| Entradas GATE-A, 016 y condiciones previas de 019/052–054; componentes 017–022 y 055. | §4, §25 y fichas completadas. | SATISFECHA. | PASA. |
| Versiones/configuración/entorno Redis aprobados; modelo de fallos, ACK/lecturas y señales antes de 017/019; precedencia/cardinalidad/HMAC/recuperación antes de cuotas. | E016, E017, E019, E052–E054. | SATISFECHA en entorno local declarado. | PASA. |
| Revocación/retención no decreciente. | E017, E022. | SATISFECHA. | PASA. |
| `noeviction`, capacidad y memoria. | E021, E022, E055 y E054. | SATISFECHA. | PASA. |
| Confirmación/lecturas por segundo host. | E017, E022. | SATISFECHA. | PASA. |
| Cuotas conjuntas entre hosts. | E021, E052, E055. | SATISFECHA. | PASA. |
| Fail-closed distribuido. | E018–E022, E055. | SATISFECHA. | PASA. |
| Detección/cierre/recuperación para el entorno declarado. | E019, E020, E021, E055. | SATISFECHA local. | PASA. |
| Dos hosts/dependencias reales, propagación y límites. | E021, E022, E055. | SATISFECHA como hosts lógicos. | PASA. |
| Prueba de reapertura segura. | E020, E021, E055. | SATISFECHA. | PASA. |
| AUTH-027 no certificado por checkpoint de componentes. | Estado de AUTH-027 y tareas relacionadas. | PARCIAL PERMITIDA como residual explícito. | NO BLOQUEA. |
| Casos JWT/endpoints finales. | §4; F/G posteriores. | NO APLICA a GATE-B. | FUERA DEL GATE. |

### 38.4. Condiciones satisfechas en el entorno local declarado

Quedaron satisfechas, con la evidencia citada:

- Redis real compartido; dos hosts lógicos independientes; conexiones, stores y guards independientes.
- Revocación A→B, TTL absoluto, retención no decreciente y ausencia de reducción.
- Confirmación/lecturas, fail-closed, no fallback local y no caché negativa permisiva.
- Siete cuotas distribuidas, Redis TIME, Lua atómico, RetryAfter, capacidad global 5000, no partial consumption, cleanup y no reset.
- Fencing/recovery, propagación de estado compartido, reapertura segura explícita y coordinación de cierre.
- OOM controlado, `noeviction` efectivo y memoria/capacidad medidas.
- PolicyRevision/snapshot fail-closed, HMAC/pseudonimización y privacidad diagnóstica.

### 38.5. Capacidad y memoria registradas

| Concepto | Valor registrado |
| --- | --- |
| `MaxActivePartitions` | **5000**. |
| Presupuesto de cuotas | **67108864 bytes**; `QUOTA_BUDGET_SCOPE=CUOTAS_ONLY`. |
| Evidencia medida en 021 | **24757168 bytes** atribuibles para **5000 particiones / 272700 eventos**. |
| Pico registrado en 055 | **9376224 bytes** atribuibles. |
| Perfil 022 | `maxmemory` de fixture **67108864 bytes**, `used_memory` **1397640 bytes**, `headroom` **65711224 bytes**; `MAXMEMORY_SCOPE=FIXTURE_TOTAL`. |

`QUOTA_BUDGET_SCOPE=CUOTAS_ONLY` y `MAXMEMORY_SCOPE=FIXTURE_TOTAL` son ámbitos distintos. Estos valores no constituyen dimensionamiento productivo ni garantía de capacidad empresarial.

### 38.6. Regresión vigente registrada

Evidencia de regresión utilizada para esta evaluación documental:

- Build: **0 advertencias / 0 errores**.
- UnitTests: **333/333**.
- IntegrationTests: **100/100**.
- SecurityTests: **71/71**.
- Focal IT de T-AUTH-022: **3/3**.
- Focal ST de T-AUTH-022: **2/2**.

No se repitieron pruebas en este cierre documental. La regresión registrada no se convierte en una nueva certificación técnica: se reutiliza la evidencia ya acreditada.

### 38.7. Límites residuales y no-claims

- **AUTH-027:** NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE. **No bloquea GATE-B**, no se certifica mediante este checkpoint y continúa como requisito/riesgo posterior.
- **Pérdida completamente silenciosa/no observable:** permanece como riesgo residual de infraestructura. No se declara eliminada, detectada universalmente ni resuelta; su tratamiento posterior corresponde a tareas/gates/infraestructura posteriores.
- **T-AUTH-008:** IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE. No fue entrada de GATE-B, no bloquea este cierre y su integración productiva sigue pendiente.
- **T-AUTH-023, T-AUTH-056, T-AUTH-057 y T-AUTH-058:** NO INICIADAS. GATE-B no las satisface ni las autoriza.
- **GATE-C y gates posteriores:** permanecen **PENDIENTES / NO SATISFECHOS** sin cambios.
- **Infraestructura no acreditada:** GATE-B no acredita TLS productivo, ACL productivas, AWS, Terraform, Load Balancer, hosts físicos independientes, failover cloud, sizing definitivo de Redis, deployment productivo, forwarding/LB productivo ni operación del servicio administrado. Son límites aceptados del cierre local, no defectos ocultos.
- **Alcance:** el cierre es un checkpoint local de componentes Redis para el entorno declarado de test; no es aceptación productiva ni cumplimiento integral de la rúbrica externa.

El estado **GATE-B PENDIENTE / NO SATISFECHO** registrado en §37.2 y en [memoria §44](../../memoria.md#44-cierre-de-t-auth-022-y-evidencia-operativa-redis) corresponde al cierre de T-AUTH-022 y se conserva como historial; este §38 fija el estado vigente del gate. Este cierre no inicia automáticamente T-AUTH-023/056/057/058, no cierra GATE-C ni modifica la evidencia de las tareas de entrada.

**Adenda posterior del 11 de noviembre de 2026:** [§68](#gate-b-correctivo-logical-clock) consolida el incidente real de retroceso de Redis TIME y su correctivo autorizado. GATE-B permanece **CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA**, sin reescribir este cierre original ni sus límites.

## 39. Evidencia técnica y cierre de T-AUTH-023

<a id="e-t-auth-023"></a>

### E(T-AUTH-023)

Registro del **10 de noviembre de 2026**. Autorización posterior **BUILD CONTROLADO** exclusivamente para T-AUTH-023, seguida de autorización **TEST-ONLY** para corregir la expectativa de excepción de siete casos y de autorización **SEGURIDAD TEST-ONLY** para sanear el logging del harness. **Trazabilidad oficial:** AUTH-002/004/007/012/014/015/034/038; PLD-AUTH-03/04/05/20; AC-01/05/08/17/21; dependencia 022 completada; F4/GATE-C. Acredita únicamente la contribución de 023 a esos requisitos/criterios, no sus flujos funcionales completos.

**Baseline:** rama `feature/authentication`, HEAD `833abf362f3c534b3417c2f724a769153dc633ac`; árbol limpio al iniciar el BUILD original. Las reanudaciones conservaron sus cinco archivos tracked modificados y siete nuevos; no se descartó trabajo. No hay commit de esta entrega ni consulta al remoto.

### 39.1. Paquetes, archivos y decisiones aprobadas

- Referencia directa única añadida: `<PackageReference Include="MongoDB.Driver" Version="3.13.0" />` en `Enver.API/Enver.API.csproj`. `MongoDB.Bson` no tiene referencia directa redundante.
- Grafo efectivo añadido: Driver **3.13.0**, Bson **3.13.0**, DnsClient **1.6.1**, SharpCompress **0.48.1**, Snappier **1.3.1** y ZstdSharp.Port **0.7.3**. Sin downgrade ni cambio de versiones ajenas; assets `lib/net6.0` compatibles y compilados con `net10.0`. El grafo efectivo no se confunde con todos los mínimos de grupos de frameworks del catálogo NuGet; el framework satisface/pruna dependencias y las abstracciones de logging existentes de tests no se degradan.
- Licencias comprobadas en metadatos del paquete: Driver/Bson/DnsClient **Apache-2.0**, SharpCompress/ZstdSharp.Port **MIT**, Snappier **BSD-3-Clause**. En el BUILD original, `dotnet list Enver.Backend.slnx package --vulnerable --include-transitive --no-restore` no reportó paquetes vulnerables en los cuatro proyectos. No es certificación universal ni auditoría de imágenes/plataforma.
- Restore normal autorizado actualizó exclusivamente `Enver.API/packages.lock.json`, `tests/Enver.API.UnitTests/packages.lock.json`, `tests/Enver.API.IntegrationTests/packages.lock.json` y `tests/Enver.API.SecurityTests/packages.lock.json`. Las validaciones posteriores usaron `--locked-mode`; la corrección de logging no cambió paquetes, csproj ni lockfiles.
- Producto creado: `Enver.API/Features/Authentication/Core/User.cs`, `UserStatus.cs`, `Enver.API/Infrastructure/MongoDB/UserDocument.cs` y `MongoClientFactory.cs`.
- Pruebas creadas: `tests/Enver.API.UnitTests/MongoBsonModelTests.cs`, `MongoClientFactoryTests.cs`, `tests/Enver.API.IntegrationTests/MongoBsonIntegrationTests.cs` y `TestcontainersLoggingPrivacyTests.cs`. Se modifica `tests/Shared/AuthenticationDependenciesFixture.cs` exclusivamente para conectar y alojar el logger redactor test-only; sin modificar su funcionamiento, imágenes/digests o topología.

| Decisión aprobada | Valor materializado y comprobado |
| --- | --- |
| `APPROVED_MONGODB_DRIVER` | MongoDB.Driver 3.13.0 estable. |
| `APPROVED_MONGOCLIENT_LIFETIME` | Un cliente reutilizable por propietario singleton de proceso/destino; composición aislada, sin registro productivo. |
| `APPROVED_TOPOLOGY` | MongoDB 8.0.34-noble, replica set rs0 de un nodo, solo test local. Imagen/digest de la fixture intactos: `mongo:8.0.34-noble@sha256:3c64937121b9437095921c4bb2569911084c05f7b88e4a263d62a8959d2b2399`. |
| `APPROVED_WRITE_CONCERN` / `APPROVED_JOURNALING` | `w:majority`, `j:true` explícitos. |
| `APPROVED_ACK_TIMEOUT` | WTimeout derivado de `MongoDb:OperationTimeoutMilliseconds`: base 2000 ms comprobada en UT; fixture 500 ms comprobados en UT y comando real. No era un requisito previo independiente: su derivación se aprobó en la resolución pre-BUILD. |
| `APPROVED_READ_CONCERN` / `APPROVED_READ_PREFERENCE` | Majority / Primary. |
| `APPROVED_STATUS_BSON` | String exacto Active o Disabled; sin representación ordinal ni default permisivo. |
| `APPROVED_RETRY_WRITES_023` | false. |

### 39.2. Modelo, configuración y evidencia real

| Capacidad | Resultado observado |
| --- | --- |
| User/BSON | Exactamente siete propiedades y campos: `Id`→`_id`, `FullName`→`fullName`, `NormalizedEmail`→`normalizedEmail`, `PasswordHash`→`passwordHash`, `Status`→`status`, `CreatedAt`→`createdAt`, `UpdatedAt`→`updatedAt`. Strings para nombre/correo/hash/status; ningún campo auxiliar ni DTO HTTP. |
| Identidad/estado/tiempo | `User.Create` genera UUIDv4 backend, Active inicial y un mismo instante UTC para ambos timestamps, normalizado a milisegundos. BSON UUID con `GuidRepresentation.Standard`/Binary subtype 4 y bytes estándar; fechas BSON DateTime UTC. El round-trip conserva los siete valores y la precisión, incluso antes del epoch en UT. |
| Rechazos | Serializador cerrado de status rechaza desconocidos, strings ordinales, números y null. Faltantes y campos extra fallan, sin modelo parcial ni fallback a Active. Rechazo de UUID legacy, identidad no UUIDv4 y timestamps de modelo no UTC. No anticipa validación institucional, hashing o reglas de registro de tareas posteriores. |
| Factory | Snapshot congelado, acceso concurrente reutilizable y disposal del propietario; UT de composición singleton aislada y reutilización real en IT. La composición debe conservar una factory por destino; no hay cache global de autoridad ni conexión por request. No modifica `Program.cs`. |
| Settings | Majority/Primary, `RetryWrites=false`, WriteConcern majority+j:true+WTimeout; ConnectTimeout/ServerSelectionTimeout derivados de ConnectionTimeoutMilliseconds, SocketTimeout/WaitQueueTimeout de OperationTimeoutMilliseconds, todos positivos y finitos. Conserva TLS/validaciones externas, sin excepciones para tests. |
| API/timeouts | El primer build intentó usar `MongoClientSettings.Timeout`; el miembro es interno en 3.13.0 y se retiró ese uso. No se activa CSOT, que retiraría wtimeout del comando; se rechaza `timeoutMS` por URI. Los límites por fase no son un presupuesto total de flujo, pendiente en 024+ con las abstracciones existentes. |
| Topología | `hello` confirmó `setName=rs0`, primary escribible y un solo host; serverStatus confirmó WiredTiger persistente y buildInfo versión 8.0.34. El cliente observó ReplicaSetPrimary. |
| Round-trip/concerns/ACK | Inserción tipada de usuario sintético, inspección BSON bruto y lectura tipada equivalentes. Monitor de comandos conserva solo booleanos: insert con `w=majority`, `j=true`, `wtimeout=500`, sin txnNumber; respuesta ok/n=1 y sin writeErrors/writeConcernError; dos find con ReadConcern majority. Colección Primary y servidor primary acreditados. El éxito de la operación y su respuesta acreditan ACK, no solo `IsAcknowledged` de configuración. |
| Journaling local | WiredTiger persistente y escritura crítica con `j:true` aceptada en MongoDB 8.0.34. Sin flags obsoletos ni inferencia solo desde defaults; no es prueba de supervivencia ante fallo físico. |
| Aislamiento/cleanup | RunId/EnvironmentName/base de fixture comprobados; dos bases propias con sufijo `_t023_` y discriminador por prueba, documentos sintéticos distintos y ausencia de contaminación cruzada. Borrado solo de bases propias, colección ausente tras limpieza y segunda base conservada al limpiar la primera; disposición final de dependencias. |

Se conserva la semántica de fallos sin implementar IUserStore: timeout/error de WriteConcern significa escritura no confirmada que puede haber ocurrido, no rollback; fallo de lectura no significa usuario inexistente; topología no apta exige cierre conservador en flujos futuros; sin retry aplicativo ciego ni compensación destructiva. La traducción de resultados corresponde a 024.

### 39.3. Incidencias y privacidad del harness

El primer focal UT tuvo **31/38**: los siete casos de campo obligatorio ausente esperaban FormatException y el driver produjo **MongoDB.Bson.BsonSerializationException**. Se detuvo el incremento. La autorización posterior corrigió solo la aserción exacta de `Every_field_is_required`, conservando los siete casos y el rechazo obligatorio; UT **38/38**. No cambió producto para forzar verde.

Las primeras IT pasaron **3/3**, pero su output expuso una credencial efímera de la fixture mediante logging automático de Testcontainers; se detuvo antes de regresiones/cierre documental. Fuente: `DockerContainerOperations.ExecAsync` → `Logger.ExecuteCommandInDockerContainer` → ConsoleLogger, que formatea el comando mongosh completo. No se transcribe el valor. Los contenedores de dependencias terminaron eliminados; la redacción futura no reescribe el output histórico.

La autorización posterior permite solo dos archivos test-only para resolverlo: fixture compartida y `TestcontainersLoggingPrivacyTests.cs`. `SensitiveTestcontainersLogger` se conecta mediante `WithLogger` a MongoDB, Redis y DynamoDB Local, **sin modificar/deshabilitar el logger global ni cambiar niveles**. Sustituye por `[REDACTED]` los secretos conocidos de la fixture, su representación URI-encoded, URIs MongoDB/Redis completas, pares de credenciales, argumentos/campos sensibles y Bearer. Formatea y sanea antes del sink: no reenvía estado estructurado ni Exception originales; mantiene texto saneado de tipos, mensajes, inner exceptions y stack, niveles, EventId, scopes y eventos de lifecycle/readiness/cleanup. No captura ni traga excepciones funcionales.

La prueba sin Docker verifica secretos sintéticos en comandos, URIs, campos, scopes, estado estructurado y excepciones; usa aserciones booleanas para no imprimir secretos ante un fallo. Acredita que warning/error, tipos de excepción, mensaje de healthcheck, servicio, puerto y container id sintético permanecen visibles. Un aviso xUnit2008 inicial se corrigió solo en esa prueba sin imprimir el contenido; su reejecución y build final quedaron sin warnings.

Se capturó el output de IT focal y regresiones IT/ST antes de publicarlo: comprobación explícita sin URIs MongoDB/Redis ni pares de credenciales, y sin argumento `--password` no redactado. La captura focal muestra `[REDACTED]` conservando el comando/readiness/cleanup. Artefactos locales fuera del repositorio, bajo `%LOCALAPPDATA%/Temp/opencode/`: `t-auth-023-it-focal-sanitized.log`, `t-auth-023-it-regression-sanitized.log` y `t-auth-023-st-regression-sanitized.log`. La garantía registrada corresponde a esos canales, valores conocidos y patrones ensayados; no es una certificación universal de todos los loggers o de producción.

### 39.4. Comandos y resultados finales

| Comando / verificación | Resultado real tras sanitización |
| --- | --- |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~TestcontainersLoggingPrivacyTests"` | **1/1**, sin Docker; reejecución sin warnings. |
| `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build --filter "FullyQualifiedName~MongoBsonModelTests\|FullyQualifiedName~MongoClientFactoryTests"` | **38/38**. El separador del filtro es el carácter pipe, escapado aquí únicamente por la tabla Markdown. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~MongoBsonIntegrationTests" --logger "console;verbosity=normal"` | **3/3**; captura de output comprobada. |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto, todos los proyectos actualizados; sin cambios nuevos de lockfiles. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build` | **371/371**, 0 fallidas/omitidas. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build` | **104/104**, 0 fallidas/omitidas; captura sin patrones de conexión/credencial. |
| `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build` | **71/71**, 0 fallidas/omitidas; captura sin patrones de conexión/credencial. |

Las suites añaden 38 UT de 023, 3 IT MongoDB y 1 regresión de privacidad del harness; no hay nueva ST focal. La autorización original y las reanudaciones se registran juntas, sin ocultar los intentos detenidos ni atribuirles resultados finales verdes.

### 39.5. Estado formal y límites

**T-AUTH-023: COMPLETADA / EVIDENCIA TÉCNICA REGISTRADA. GATE-C: PENDIENTE / NO SATISFECHO. T-AUTH-024/025/026/027: NO INICIADAS.** GATE-A/B conservan SATISFECHO / CERRADO. 008 conserva IMPLEMENTACIÓN PREPARADA / VERIFICACIÓN PRODUCTIVA DE STARTUP PENDIENTE; 056/057/058 NO INICIADAS; AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.

El estado de 023 NO INICIADA de §§37–38 y memoria §§44–45 se conserva como antecedente; esta autorización posterior fija el cierre vigente de 023 sin cambiar los cierres anteriores. No se implementan store productivo, índice unique/inicializador, CAS, hasher, endpoints, register/login/logout, Orders, AWS o Terraform. `Program.cs`, configuración productiva existente, spec/plan, constituciones, foundation, AGENTS/README, solución y proyectos de tests permanecen intactos. La fixture conserva funcionalidad, imágenes/digests, topología, credenciales, puertos, readiness, timeouts y cleanup: solo cambia su frontera de logging.

Un replica set de un nodo no acredita quorum multinodo, failover, HA, Atlas, cloud, TLS/ACL productivos, dos instancias desplegadas ni producción. La validación de infraestructura/topología productiva y 068 quedan pendientes. No se cierran GATE-C–H ni se satisface AUTH-027. Sin stage, commit ni push; no se inicia ninguna tarea posterior.

## 40. Evidencia técnica y cierre de T-AUTH-024

<a id="e-t-auth-024"></a>

### E(T-AUTH-024)

Registro del **10 de noviembre de 2026**, tras autorizaciones posteriores de BUILD, ajustes/diagnósticos TEST-ONLY y **REGRESIÓN FINAL DEFINITIVA Y CIERRE** de 024/025. Rama `feature/authentication`, HEAD base y conservado `64db47e347647e76ffb3588663a2a5e4616295ca`. Se conservó el trabajo preexistente, incluida la fixture modificada y los archivos nuevos sin seguimiento. No se hizo stage, commit ni push.

**Trazabilidad de la ficha:** AUTH-004/005/009/010/015/031/038/039; PLD-AUTH-02/05/14/18; AC-01–03/07/08/17; dependencias 009/023; F4/GATE-C. Esta evidencia acredita únicamente el componente y su contribución, no los flujos HTTP completos ni la satisfacción integral de esos requisitos.

### 40.1. Contrato, implementación y pruebas

- `Enver.API/Features/Authentication/Application/Ports/IUserStore.cs`: `FindByNormalizedEmailAsync`, `FindByIdAsync` e `InsertAsync`, con RequestBudget explícito; lecturas **Found / NotFound / Unavailable**, inserciones **Confirmed / Conflict / Unavailable / Uncertain**, consistencia de payload/causa y representación textual sin datos del usuario.
- `Enver.API/Infrastructure/MongoDB/MongoUserStore.cs`: igualdad construida por backend y collation simple; ID UUID tipado. Solo una consulta completada vacía acredita NotFound; errores, BSON inválido y multiplicidad no se convierten en inexistencia. Se devuelve también Disabled, sin ocultarlo ni anticipar reglas de login.
- Insert preserva identidad/modelo y especifica WriteConcern en el comando real: majority, j:true y wtimeout=500 en IT. Lecturas majority/primary; `RetryWrites=false`. Confirmed exige ACK concluyente; **Conflict requiere duplicate key 11000 y keyPattern estructurado exclusivo de normalizedEmail ascendente**. Duplicate `_id`, clave compuesta/ausente u otro rechazo no se convierten en conflicto de email. WriteConcern ambiguo, respuesta perdida o cancelación post-dispatch conservan Uncertain; no implican rollback ni ausencia de escritura.
- Cero retry aplicativo, nueva identidad, read-back automático, borrado compensatorio o caché de credenciales. RequestBudget no se reinicia entre operaciones; límite de dependencia finito mediante las abstracciones existentes.
- Pruebas: `tests/Enver.API.UnitTests/MongoUserStoreTests.cs` (**22/22** en focal previa, incluidas en UT completas finales); `tests/Enver.API.IntegrationTests/MongoUserStoreIntegrationTests.cs` (**5/5** focal previa y **5/5** dentro de la regresión final). Cubren lectura por email/ID tras insert confirmado, concerns efectivos, duplicados email frente a ID, deserialización real fallida, Disabled, aislamiento de bases y ACK-loss.
- **Límite de ACK-loss:** inserción real confirmada en MongoDB cuya entrega de ACK se suprime mediante decorador test-only en la frontera del adapter; resultado Uncertain y un solo intento, sin read-back automático ni compensación. No acredita corte TCP, failover ni pérdida física de ACK en una red productiva.

### 40.2. Índice como precondición test-only

Las IT 024 conservan el warm-up existente: ping a admin con ReadPreference.Primary, CTS independiente de **5 s**, respuesta válida y primary obligatorio. Después, `PrepareIndexAsync` crea el índice usando **la misma `MongoUsersInitializer.CreateIndexDefinition()`**, bajo un RequestBudget **test-only independiente de 5000 ms**. Verifica mediante el initializer y exige **Ready**, manteniendo sus **500 ms de dependencia**. Solo después se crea el RequestBudget normal del UserStore de **8000 ms**, con **500 ms por dependencia MongoDB**.

El setup no altera Options, cliente, concerns, RetryWrites ni plazos productivos. No hay retry ni aserción flexibilizada. La creación/verificación repetida, idempotencia y carreras del índice corresponden a **025**, no a las operaciones del store medidas por 024. En esta validación definitiva no se añadieron warm-ups ni instrumentación ni se modificó código.

### 40.3. Validación final conjunta y fuentes de evidencia

Suites ejecutadas **secuencialmente**, sin modificar archivos entre ellas; IntegrationTests **una sola vez**, con límite externo de **900 s**, sin corte ni reejecución:

| Comando / verificación | Resultado real |
| --- | --- |
| `docker info` | Docker disponible, Engine **29.6.1**. |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; proyectos actualizados, lockfiles intactos. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build` | **414/414**, 0 fallidas/omitidas. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --logger "console;verbosity=detailed"` | **120/120**, 0 fallidas/omitidas; **7.9706 minutos** (478.236 s). Incluye Mongo024 **5/5**, Mongo025 **11/11** y capacidad Redis verde. |
| `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build` | **71/71**, 0 fallidas/omitidas; ejecutada después de IT verde. |

Fuente primaria: salidas de los comandos de esta sesión. Captura local completa de IT retenida por el harness: `sh_1c8fe724e001giycNqYiAqvgdo.out`, fuera del repositorio; no es artefacto versionado. No se transcriben documentos, comandos completos sensibles, HMACs, claves ni credenciales. Las ST son la suite existente, no nuevas ST específicas de 024/025 ni certificación integral de seguridad.

La regresión final volvió a acreditar **2500 admissions reales / 5000 particiones** Redis, metadata=registry=5000, namespace propio=5002 keys, carrera 1 Admitted/7 Unavailable esperados y ausencia de admisión parcial según las aserciones. Snapshot final: PING disponible, master sin réplicas/cluster, used_memory=4409872 bytes, maxmemory=67108864, noeviction, connected_clients=4, blocked_clients/rejected_connections/evicted_keys=0. No se observó OOM en ese caso de capacidad; la prueba de OOM deliberado usa su contenedor dedicado. **GATE-B permaneció SATISFECHO / CERRADO**, sin reinterpretar esta regresión como reapertura o nueva garantía productiva.

### 40.4. Incidencias históricas y alcance conservado

Se conservan como incidencias de validación **no reproducidas en la ejecución final**, sin presentarlas como defectos productivos demostrados:

1. Una regresión anterior **119/120** falló en la IT positiva Mongo024: initializer Unavailable pese a comandos exitosos. El aislado **1/1** y subconjunto **7/7** posteriores pasaron. Causa histórica no determinada. Se autorizó separar la precondición del índice fuera del presupuesto del store, sin cambiar producto.
2. Una focal posterior de 025 obtuvo **10/11**: con duplicados, Unavailable/cancelación y un segundo índice listado rompieron la igualdad de índices. El diagnóstico aislado posterior **1/1** observó 11000/DuplicateKey, Incompatible dentro de presupuesto, documentos preservados y solo `_id_`; no identificó el índice histórico ni demostró su estado. Ver §41.
3. Otra regresión **119/120**, con Mongo024 **5/5** y Mongo025 **11/11**, falló en `RedisAuthenticationQuotaCapacityTests.Exactly_2500_real_admissions_create_5000_and_consumers_race_without_exceeding_limit`: Admitted esperado / Unavailable durante el llenado. Aislado **1/1** y precedentes inmediatos + capacidad **3/3** pasaron, con 2500 admissions/5000 particiones. **Clasificación E: causa no determinada**; no se infiere timeout, OOM, defecto de fixture o producto. No reabrió GATE-B.

La instrumentación test-only existente de Mongo024/Mongo025/capacidad Redis permanece; observa reloj/timers reales, eventos y métricas agregadas y puede influir en tiempos, por lo que no es benchmark. **No se modificó producto, expectativas, timeouts, memoria ni capacidad para forzar verde** durante estos diagnósticos y la validación final. No se ejecutó nuevamente IT tras el verde final.

### 40.5. Estado formal y no-claims

**T-AUTH-024: COMPLETADA / EVIDENCIA REGISTRADA. T-AUTH-025: COMPLETADA / EVIDENCIA REGISTRADA** por su evidencia separada de §41. **T-AUTH-026/027: NO INICIADAS. GATE-C: PENDIENTE / NO SATISFECHO. GATE-B: SATISFECHO / CERRADO**, al igual que GATE-A; GATE-D–H pendientes. AUTH-027 continúa NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE; 008 conserva startup productivo pendiente; 056/057/058 no iniciadas. Los estados de 024/025 no iniciadas de §39 son antecedentes, no el estado vigente tras este cierre autorizado.

Componentes aislables, sin registro DI ni composición productiva de `Program.cs`, endpoints, register/login/logout, CAS, Orders, AWS o Terraform. MongoDB local de un nodo no acredita Atlas, quorum multinodo, failover, HA, TLS/ACL/IAM productivos, dos hosts físicos o sizing productivo. No se cierran requisitos íntegros ni gates posteriores. Solo se actualizan este documento y `memoria.md` tras todas las suites verdes; sin stage, commit ni push ni inicio de otra tarea.

## 41. Evidencia técnica y cierre de T-AUTH-025

<a id="e-t-auth-025"></a>

### E(T-AUTH-025)

Registro separado del **10 de noviembre de 2026**, bajo la misma autorización final, rama y HEAD de §40. **Trazabilidad de la ficha:** AUTH-004/005/009/015/038; PLD-AUTH-05/06/24; AC-03/17; dependencia 023; F4/GATE-C. Acredita índice/inicialización aislables, no admisión de registros productivos ni satisfacción integral de AUTH-009.

### 41.1. Definición y semántica acreditadas

- `Enver.API/Infrastructure/MongoDB/MongoUsersInitializer.cs`: índice propuesto `normalizedEmail_1`, key **normalizedEmail ascending**, **unique=true**, **collation simple/binaria**, sin sparse, partial, hidden ni TTL. La inspección compara clave/unique/collation/opciones, rechaza buildUUID y no confía solo en el nombre. Acepta nombre equivalente distinto sin renombrar/recrear; conserva `_id_` y otros índices independientes.
- Una dependencia MongoDB de **500 ms** cubre inspección, un único intento de creación cuando procede y reinspección. El RequestBudget total de las IT es **8000 ms**; se conservan RetryWrites=false y concerns aprobados. No hay lock local usado como autoridad distribuida, retry aplicativo, reparación, borrado ni reconciliación automática de datos/índices.
- **Ready** requiere verificación efectiva dentro de presupuesto. **Incompatible** expresa evidencia concluyente de definición incompatible o duplicados observados y verificación completada dentro del presupuesto. **Unavailable** mantiene fail-closed cuando no puede confirmarse el estado dentro de ese presupuesto; no se convierte retroactivamente en Ready por una observación posterior. Cancelar la espera no garantiza detener el trabajo remoto.
- Reinspeccionar después de creación/carrera o respuesta perdida no es repetir creación. No se afirma que CommandSucceeded por sí solo pruebe finalización de toda la operación ni unicidad efectiva.

### 41.2. Evidencia de pruebas y preservación

`tests/Enver.API.UnitTests/MongoUsersInitializerTests.cs`: **21/21** en focal previa, incluidas en UT finales; definición exacta, equivalencia semántica, opciones incorrectas (incluidos TTL/building), idempotencia, respuesta perdida, colisión de nombre, ausencia/incompatibilidad y cancelación/fallo de inspección.

`tests/Enver.API.IntegrationTests/MongoUsersInitializerIntegrationTests.cs`: **11/11** focal previa y **11/11** en la regresión final. Creación efectiva y segunda inicialización Ready sin cambios; duplicado rechazado realmente; incompatibilidades reales de key, descending, nonunique, partial, sparse, collation y hidden preservan datos/índices; nombre equivalente aceptado sin mutación. Dos hosts lógicos con clientes independientes inicializaron concurrentemente la misma base: **HostA=Ready / HostB=Ready**, un solo índice compatible, sin retry ni reparación. No son dos hosts físicos desplegados.

En el escenario concluyente con dos documentos duplicados, la ejecución final capturó **createIndexes CommandFailed, Code 11000 / CodeName DuplicateKey**, duración **202.584 ms**, evento a **207.584 ms** del dependency. Retornó **Incompatible** con token no cancelado; documentos originales preservados. Antes, inmediatamente después y tras **300 ms** se observó solo `_id_`, cero índices compatibles, sin build activo observado; `hint normalizedEmail_1` fue rechazado. El tercer duplicado diagnóstico fue aceptado, conservando los dos originales y dejando tres documentos sintéticos: no había unicidad de email efectiva y nunca se declaró Ready. Esta inserción adicional es exclusivamente una sonda de test, no reparación ni comportamiento del initializer.

La incidencia histórica de segundo índice tras Unavailable se conserva en §40.4: **su identidad, estabilidad/usabilidad y causa siguen sin determinarse**, no se afirma que MongoDB construyera indebidamente un índice único. El contrato de task/plan prohíbe reparar/borrar/reconciliar **datos** automáticamente; no se añade por interpretación una prohibición nueva de todo intento de creación con datos duplicados ni se relaja la aserción existente de preservación.

### 41.3. Regresión final, estado y límites

Comparte la ejecución real y los comandos de §40.3, sin ejecutar suites adicionales: restore locked correcto, build **0 advertencias / 0 errores**, **UT 414/414, IT 120/120 en 7.9706 minutos, ST 71/71**, sin fallidas/omitidas. Focales históricas 024 **5/5** y 025 **11/11** confirmadas también por sus clases dentro de la regresión final. No reaparecieron los fallos históricos Redis/MongoDB; no se modificó producto para obtener verde y GATE-B permaneció cerrado.

**T-AUTH-025: COMPLETADA / EVIDENCIA REGISTRADA. T-AUTH-024: COMPLETADA / EVIDENCIA REGISTRADA (§40). T-AUTH-026/027: NO INICIADAS. GATE-C: PENDIENTE / NO SATISFECHO. GATE-A/B: SATISFECHOS / CERRADOS.** Se conservan los no-claims de §40.5: sin startup/DI productivo, register/login/logout, Atlas/HA/failover, AWS/Terraform ni cierre integral de AUTH-027. La identidad de inicialización de mínimo privilegio y el contraste de infraestructura productiva siguen pendientes. Constituciones, spec/plan, foundation, AGENTS/README, producto, tests, fixture, proyectos, paquetes e imágenes no se modifican en esta validación definitiva; solo se registra evidencia en task/memoria. Sin stage, commit ni push; trabajo detenido tras el cierre autorizado.

## 42. Evidencia técnica y cierre de T-AUTH-026

<a id="e-t-auth-026"></a>

### E(T-AUTH-026)

Registro del **10 de noviembre de 2026**. Autorización posterior **BUILD AGRUPADO Y CIERRE DE GATE-C**: implementar 026, validar focalmente, después crear/verificar 027, ejecutar regresión secuencial y consolidar evidencias/revisión formal. Rama `feature/authentication`, HEAD base y conservado `255140461911e7121dba73f46abbd24f4d226eb1`, árbol limpio al inicio. Sin stage, commit ni push.

**Trazabilidad oficial:** AUTH-012/013/014/039; PLD-AUTH-05/12; AC-06/08; dependencias 023/024; F4/GATE-C, soporte GATE-E. Acredita únicamente la persistencia CAS, no PasswordHasher, rehash funcional, decisión de credenciales, JWT o login.

### 42.1. Contrato aprobado e implementación

Modificados exclusivamente los dos archivos productivos autorizados: `Enver.API/Features/Authentication/Application/Ports/IUserStore.cs` y `Enver.API/Infrastructure/MongoDB/MongoUserStore.cs`. Único método CAS añadido:

```csharp
ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(
    Guid id,
    string expectedPasswordHash,
    UserStatus expectedStatus,
    string newPasswordHash,
    DateTime updatedAtUtc,
    RequestBudget budget);
```

`UserCredentialUpdateStatus` contiene **Updated / Conflict / Unavailable / Uncertain**. El resultado valida enum/causa, expone incertidumbre y su ToString solo contiene categoría, nunca hash/filtro/usuario. Sin proyección HTTP ni excepciones sensibles expuestas por el adapter.

`UpdateOneAsync` tipado, filtro de igualdad backend **_id AND passwordHash anterior AND status esperado**, collation simple/binaria, **IsUpsert=false**, `$set` exclusivamente **passwordHash/updatedAt**. No reemplaza el documento ni modifica status, createdAt u otros campos. Los siete campos y los archivos User/BSON de 023 permanecen intactos; no hashVersion, roles, tokens ni campos auxiliares. Active será la precondición del futuro login, que no se implementa aquí.

El instante procede del backend que llama al puerto: se exige DateTime UTC y se trunca a milisegundos BSON antes del update. No exige mayor que el timestamp anterior, no añade +1 ms ni lo usa como token CAS; las pruebas emplean instantes deterministas distintos. Identidad/estado inválidos, hashes null o instante no UTC no despachan.

### 42.2. Confirmación, incertidumbre y presupuesto

| Condición concluyente dentro de presupuesto | Resultado |
| --- | --- |
| ACK válido, matched=1, modified=1, sin error de write concern | **Updated**. |
| ACK válido, matched=0, modified=0 | **Conflict**, sin distinguir inexistencia, estado o hash obsoleto. |
| ACK válido, matched=1, modified=0 | **Unavailable**, no-op confirmado según concreción expresamente aprobada; no éxito de rehash ni incertidumbre sobre una modificación. |
| Rechazo concluyente sin escritura o fallo previo al posible despacho | **Unavailable**. |
| ACK ausente, conteos no disponibles/incoherentes, respuesta perdida, write concern incierto o timeout/cancelación tras posible despacho | **Uncertain**, nunca Conflict. |

Reutiliza RequestBudget/DependencyBudget sin reiniciarlos: **8000 ms total IT / 500 ms dependencia MongoDB**, además del restante efectivo. Marca posible despacho conservadoramente antes de invocar al driver; esto no acredita envío físico. Retorno positivo o conflicto requiere comprobación final del presupuesto. Cancelar espera no deshace trabajo remoto. Cero retry aplicativo, segundo update, read-back automático, rollback o compensación. Relectura/credenciales/emisión pertenecen a **032/048**.

Concerns heredados y comprobados en el comando update real: **majority+j:true+wtimeout=500**, ReadConcern majority, ReadPreference primary y **RetryWrites=false**, sin txnNumber. WTimeout deriva de OperationTimeoutMilliseconds; no se modifican Options, cliente/factory, configuración ni plazos.

### 42.3. Pruebas, incidencia y secuencia ejecutada

- Nuevo `tests/Enver.API.UnitTests/MongoUserStoreCredentialCasTests.cs`: **22/22** focal. Firma exacta, resultado seguro, filtro de tres términos, update de dos campos, no-upsert/collation, ACK/conteos/no-op, rechazos frente a incertidumbre, UTC/milisegundos, presupuesto agotado sin reinicio, cancelación antes/después de posible despacho, dependency timeout y cero retry/read-back.
- Nuevo `tests/Enver.API.IntegrationTests/MongoUserStoreCredentialCasIntegrationTests.cs`: **6/6** focal. Update real con campos preservados, no-op, hash incorrecto/status mismatch/ID ausente sin mutación, dos clientes independientes con **un Updated/un Conflict**, timestamp/hash del ganador preservados y hash anterior nuevamente rechazado. Comando update con concerns efectivos y ACK válido.
- ACK-loss: update real confirmado por MongoDB, entrega de respuesta suprimida por decorador test-only de IMongoCollection en la frontera del adapter. Retorno **Uncertain**, **un intento**, cero lecturas u otras escrituras automáticas; observación posterior exclusiva del test acredita efecto y no cambia el resultado original. No simula corte TCP ni failover físico, y no promete exactly-once.
- Reutiliza proxies/scope de 024 y añade helpers locales en el nuevo IT 026 para 026/027; no modifica otros tests ni `tests/Shared/**`. Setup conserva warm-up independiente de 5 s y preparación del índice con definición exacta de 025/presupuesto separado 5000 ms, seguida de Ready obligatorio; no amplía el presupuesto del store.
- **Una corrección test-only de las dos máximas autorizadas:** el primer build dio CS1729 por constructor no público de WriteError en el doble UT. Se corrigió solo su construcción sintética mediante reflexión en el nuevo UT; build posterior **0 advertencias/0 errores** y focales verdes. No fue defecto productivo, no se cambió producto para forzar verde ni se cambiaron expectativas, plazos o retries. 027 se creó únicamente después de ambos focales 026 verdes.

Comandos focales: restore locked y build no-restore de la solución; `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build --filter "FullyQualifiedName~MongoUserStoreCredentialCasTests"`; `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~MongoUserStoreCredentialCasIntegrationTests" --logger "console;verbosity=detailed"`. Focal IT, con fixture: **1.2815 minutos**.

### 42.4. Regresión completa compartida con E027

Tras focal 027 verde, se ejecutaron secuencialmente, sin suites concurrentes:

| Comando | Resultado real |
| --- | --- |
| `dotnet restore Enver.Backend.slnx --locked-mode` | Correcto; lockfiles intactos. |
| `dotnet build Enver.Backend.slnx --no-restore` | **0 advertencias / 0 errores**. |
| `dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build` | **436/436**, 0 fallidas/omitidas. |
| `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --logger "console;verbosity=detailed"` | **129/129**, 0 fallidas/omitidas; **7.4388 minutos**, una ejecución completa, sin corte externo de 900 s. |
| `dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build` | **71/71**, 0 fallidas/omitidas; después de IT verde. |

La regresión incluye Mongo023 **3/3**, Mongo024 **5/5**, Mongo025 **11/11**, Mongo026 **6/6**, Mongo027 **3/3**. Redis capacity volvió a acreditar 2500 admissions/5000 particiones; GATE-A/B permanecen cerrados. No reaparecieron las incidencias históricas de §40.4, que conservan su causa no determinada. Nuevas pruebas: 22 UT + 9 IT; ninguna ST artificial nueva, se ejecuta la suite de seguridad existente sin atribuirle cobertura específica que no tiene.

Fuente primaria: salidas reales de esta sesión; captura IT completa retenida localmente por el harness como `sh_1c91a95d2001bPcOIcdH9wpWrV.out` fuera del repositorio, no artefacto versionado. Datos sintéticos, diagnóstico agregado y log Testcontainers con password redactado; sin imprimir hashes, keyValue, documentos ni conexiones sensibles. Garantía limitada a los canales/casos inspeccionados, no privacidad universal.

**T-AUTH-026: COMPLETADA / EVIDENCIA REGISTRADA.** Sin composición productiva, hashing o flujo de autenticación; revisión de gate separada en §44.

## 43. Evidencia técnica y cierre de T-AUTH-027

<a id="e-t-auth-027"></a>

### E(T-AUTH-027)

Registro separado del **10 de noviembre de 2026**, misma autorización/baseline de §42. **Trazabilidad oficial:** AUTH-004/009/013/014/015/038/039/042; PLD-AUTH-05/06/12/20; AC-01/03/06/17; dependencias 015/024/025/026, con modelo/cliente 023 transitivos; F4/GATE-C. No acredita login, normalización HTTP, hashing ni criterios funcionales completos por asociación.

Nuevo `tests/Enver.API.IntegrationTests/MongoPersistenceIntegrationTests.cs`, creado después del verde focal de 026. Reutiliza helpers existentes y los nuevos de 026 sin alterar la fixture. Build posterior **0/0** y focal `dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~MongoPersistenceIntegrationTests" --logger "console;verbosity=detailed"`: **3/3**, 0 fallidas/omitidas, **56.7199 segundos** incluida fixture.

### 43.1. Matriz integrada real

| Escenario | Evidencia ejecutada |
| --- | --- |
| Creación observable A→B | A inserta Confirmed, otro cliente lee Found por ID y email; ID coincide, concerns majority/primary y RetryWrites=false. |
| Modelo real 023 | BSON inspeccionado con exactamente siete nombres; igualdad con documento esperado, UUID Standard/subtype 4, status y timestamps DateTime iguales al crear. |
| Índice/inicializador 025 | Inspección real: un índice semánticamente compatible y un `_id_`; inicializador desde el otro cliente devuelve Ready. |
| Unicidad concurrente 024/025 | Dos clientes, mismo normalizedEmail ya normalizado y UUID distintos; **un Confirmed/un Conflict**, un documento final y lectura cruzada del ganador. No se implementa normalización 028 ni endpoint. |
| CAS integrado 026 | Sobre la cuenta ganadora, dos clientes actualizan desde mismo hash/estado: **un Updated/un Conflict**, hash y timestamp del ganador, otros cinco campos preservados; viejo hash nuevamente rechazado. |
| Estado obsoleto | CAS con estado incorrecto devuelve Conflict, documento sin mutación; no se cambia status. E026 complementa usuario realmente Disabled e ID ausente. |
| Escritura incierta | Update real con entrega de ACK suprimida retorna Uncertain; un intento, cero read-back/retry/compensación automáticos. Observación posterior del test no repara el resultado. |
| Lectura fallida | Documento sintético realmente incompatible con BSON mínimo: Unavailable, no NotFound; se preserva el dato. No se presenta como caída TCP del servidor. |
| Aislamiento y cleanup | Mismo ID en dos bases propias sin contaminación; cleanup de primera conserva documento original y conteo de segunda. |
| Entorno | Consulta real hello/buildInfo: MongoDB **8.0.34**, **rs0 de un nodo**, primary escribible. E023/regresión complementan WiredTiger persistente y concerns/journaling. |

Reutiliza E023/E024/E025 y sus pruebas en vez de duplicar baterías de BSON/opciones de índice/fallos previos. Un único runtime compartido con clientes independientes, no dos servidores o hosts físicos. La preparación del índice es setup separado, no evidencia nueva de latencia de creación; creación/idempotencia del initializer conserva su prueba en 025.

### 43.2. Estado y límites

Regresión completa compartida, sin segunda ejecución: **UT 436/436, IT 129/129, ST 71/71**, restore locked y build **0 advertencias/0 errores** (§42.4). Los tres escenarios de 027 también pasaron dentro de IT completas. **T-AUTH-027: COMPLETADA / EVIDENCIA REGISTRADA** en ese entorno.

No exactly-once, ACK físico perdido en red, quorum multinodo, HA/failover productivos, Atlas, TLS/ACL/IAM de producción, sizing cloud, AWS o Terraform. No modifica Program.cs, factory, Options, User/BSON, csproj/lockfiles, imagen/digest, rs0, puertos, Redis/DynamoDB o tests/Shared. Sin paquetes nuevos ni flujo de autenticación; autorización de producción/contraste Atlas 068 sigue pendiente. T-AUTH-027 (pruebas MongoDB) no es AUTH-027 (continuidad Redis), que permanece NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE.

## 44. Revisión formal y cierre local de GATE-C — MongoDB

<a id="cierre-gate-c"></a>

La autorización de BUILD agrupado incluye expresamente revisar y cerrar GATE-C si E023–E027 y todas las validaciones lo soportan. Esta revisión es un acto posterior a las pruebas y **no un cierre automático por existencia de archivos o por completar 026/027**. Rama/HEAD de §42; sin commit de esta entrega.

### 44.1. Consolidación contra el criterio oficial

Criterio de §4: **023–027 verificadas: siete campos/BSON estándar, lectura tras creación confirmada, índice exacto/inicializador, unicidad concurrente, CAS y resultados inciertos seguros.**

| Evidencia | Revisión y soporte observado |
| --- | --- |
| [E(T-AUTH-023)](#e-t-auth-023), §39 | Driver/grafo aprobado, UUID Standard/subtype 4, siete campos, cliente/concerns y journaling; IT BSON/topología **3/3** en regresión actual, WiredTiger persistente/rs0 de un nodo/MongoDB 8.0.34. |
| [E(T-AUTH-024)](#e-t-auth-024), §40 | Store con lectura Found/NotFound/Unavailable e inserción confirmada/conflicto específico/incierta; **5/5** actuales, creación observable y ACK de insert suprimido sin retry/compensación. |
| [E(T-AUTH-025)](#e-t-auth-025), §41 | Índice exacto semántico, idempotencia/concurrencia, incompatibilidad y duplicados fail-closed sin reparación; **11/11** actuales. |
| [E(T-AUTH-026)](#e-t-auth-026), §42 | CAS atómico de tres condiciones/dos campos, ACK/concerns/plazos, status/hash conflict y carrera sin sobrescritura; **22 UT/6 IT** focales y verdes dentro de regresión. |
| [E(T-AUTH-027)](#e-t-auth-027), §43 | **3/3** focales y actuales: dos clientes, BSON/índice, lectura cruzada, unicidad/CAS y sus ganadores, incertidumbre/lectura fallida/aislamiento integrados. |

Condiciones de entrada comprobadas: GATE-B cerrado, 023 completada, driver/grafo/concerns/journaling/ACK y fixture rs0 previamente aprobados; no se adoptan nuevas versiones/topologías. Pruebas nuevas reusan exactamente esas selecciones. Restore locked correcto, **build 0/0, UT 436/436, IT 129/129 y ST 71/71**, sin omitidas. Evidencias de carrera y respuesta perdida delimitan el efecto observado y la incertidumbre, sin atribuir durabilidad cloud.

### 44.2. Decisión formal, pendientes y no-claims

**GATE-C: SATISFECHO / CERRADO como checkpoint local/test representativo de componentes MongoDB. T-AUTH-026 y T-AUTH-027: COMPLETADAS / EVIDENCIA REGISTRADA. GATE-A/B: SATISFECHOS / CERRADOS, sin reapertura. GATE-D–H: PENDIENTES / NO SATISFECHOS.**

El cierre no acredita Atlas, HA productiva, failover cloud, AWS o infraestructura productiva; **068** y la identidad de inicialización de mínimo privilegio en infraestructura siguen pendientes. Tampoco acredita startup/DI/admisión productivos, hashing, validación/Register/Login/Logout/JWT o cierre integral de AUTH-027. **008** conserva startup productivo pendiente; **028–051 y 056–079** no se ejecutan por este cierre; **056/057/058 NO INICIADAS**. Relectura/credenciales y emisión siguen en 032/048. No se modifican requisitos/decisiones de spec/plan ni las constituciones, foundation, AGENTS/README.

Los estados previos GATE-C pendiente y 026/027 no iniciadas de §§39–41/memoria §47 se conservan como antecedentes; §§42–44 fijan el estado posterior autorizado. Las incidencias históricas de §40.4 permanecen registradas sin causa productiva demostrada; no reaparecieron en la regresión actual y no se borran por el cierre. La corrección de esta entrega se limitó al constructor del doble UT (§42.3); no hubo corrección productiva posterior a focales para forzar verde. Solo task/memoria se actualizan documentalmente tras validación verde. Sin stage, commit ni push; detener sin iniciar otra tarea ni Register.

## 45. BUILD-1 — Autorización, historial y regresión final

<a id="build-1-validacion-final"></a>

Registro del **11 de noviembre de 2026**. El responsable autoriza BUILD-1 en orden **056 → 029 → 028 → 030 → 031 → 033 → 057**, las correcciones posteriores expresamente delimitadas del harness y los diagnósticos test-only; finalmente autoriza **REGRESIÓN FINAL Y ACEPTACIÓN**, sin cambios técnicos previos y sin correcciones automáticas si falla. Rama `feature/authentication`, HEAD base y conservado `8db9b6082ad3e826169d16da0df0b38d0e0ef442`. Baseline de esta validación: **6 modificados + 20 nuevos, sin stage**. Solo tras todas las suites verdes se permite actualizar este documento y `memoria.md`; sin stage/commit/push.

### 45.1. Archivos del incremento y límites

- Nuevos productivos: `Enver.API/Composition/AuthenticationServiceCollectionExtensions.cs`, `AuthenticationTransport.cs`, `AuthenticationDependencyOptions.cs`, `AuthenticationDependencyAdmission.cs`; `Enver.API/Features/Authentication/Validation/UnicodeInput.cs`, `PasswordPolicy.cs`, `InstitutionalEmailNormalizer.cs`, `AuthenticationInputValidator.cs`; `Enver.API/Features/Authentication/Application/Ports/IPasswordService.cs`; `Enver.API/Security/PasswordService.cs`, `DummyPasswordHash.cs`, `HashingConcurrencyGate.cs`.
- Modificado productivo: `Enver.API/Program.cs`, exclusivamente wiring de servicios/pipeline. No incorpora lógica funcional ni endpoints de autenticación.
- Nuevos tests: `tests/Enver.API.UnitTests/PasswordPolicyTests.cs`, `ValidationTests.cs`, `PasswordServiceTests.cs`, `HashingConcurrencyGateTests.cs`; `tests/Enver.API.IntegrationTests/AuthenticationFoundationCompositionTests.cs`, `AuthenticationDependencyCompositionTests.cs`, `HashingConcurrencyIntegrationTests.cs`; `tests/Enver.API.SecurityTests/MaliciousInputTests.cs`.
- Harness modificado: `tests/Shared/AuthenticationApiFactory.cs`; `tests/Enver.API.IntegrationTests/DistributedQuotaTestScope.cs`, `RedisOperationalScope.cs`, `RedisAuthenticationQuotaCapacityTests.cs`, `RedisAuthenticationQuotaTestScope.cs`. La fixture `tests/Shared/AuthenticationDependenciesFixture.cs` permanece intacta. Las entradas temporales que invocaban secuencias diagnósticas fueron retiradas, sin agregar casos artificiales a la regresión final.
- No cambian User/BSON de siete campos, contratos/implementación MongoDB existente, stores/protocolo/Lua Redis, capacidad 5000, timeouts, csproj, lockfiles, paquetes, imágenes/digests o topología. No se implementan Register/034–037, Login, Logout, JWT, rehash/032, Orders, AWS o Terraform. La referencia al adapter de autoridad existente no crea ni despliega recursos AWS; tests usan DynamoDB Local y credenciales sintéticas.

### 45.2. Historial preservado; no atribución retrospectiva de causas

La escritura inicial y las focales no bastaron para aceptar BUILD-1. Hubo una corrección de namespace del wiring y ajustes test-only de configuración de proxy, Generic Host y aislamiento/longitud del nombre de base sintética. La primera regresión IT terminó **129/154**: **21** fallos de factories históricas por referencias obligatorias ausentes de `AuthenticationComposition`, **3** por tabla de autoridad local compartida eliminada por cleanup, y **1** de 025 (`Unavailable` al agotar 500 ms). No se registraron cierres con esa regresión roja.

Una autorización correctiva posterior ajustó únicamente `OperationalHost`/`QuotaHost` con configuración sintética y colaborador de autoridad test-only para escenarios que no prueban 020, conservando validaciones y aserciones. Los casos de 057 preparan tablas de autoridad exclusivas mediante `OwnedCompositionAuthority`, vacías o explícitamente abiertas según escenario, y eliminan solo lo propio. No se desactivó cleanup global ni se convirtió ausencia en Open productivo. Focal correctivo **24/24** y Mongo 025 **11/11**. La siguiente regresión quedó **153/154**, por `Direct_5000_boundary_existing_only_and_no_partial_capacity` durante `FillRegisterAsync(2499)`, sin causa determinada; ST completas no se ejecutaron entonces.

El diagnóstico posterior tuvo objetivo aislado verde y síntoma equivalente en el caso hermano `Exactly_2500_real_admissions_create_5000_and_consumers_race_without_exceeding_limit`: intento **332**, **331** admisiones previas, **662/662** particiones, sin buckets propios parciales, sin cancelación observada; su categoría Lua no quedó capturada. Otro intento se bloqueó antes de ejecutar tests por `java.lang.ClassFormatError` de DynamoDB Local: incidencia ambiental separada, no explicación de Redis, sin modificar Java/imagen/fixture.

La captura semántica test-only posterior ejecutó **tres focales verdes**, una aislada y dos con ambos recovery/restart anteriores. En las tres, el intento 332 fue Admitted y se alcanzaron **2500 admisiones / 5000 particiones**. Los únicos `Unavailable` capturados fueron los siete rechazos esperados de la carrera final: EVAL real despachado, respuesta de seis campos válida, `unavailable/capacity`, `retry=0`, Parse semántico y `ToConsumeResult.unavailable` coincidentes con el resultado público. Esa rama retorna antes de escribir; sin eventos/buckets de los intentos rechazados. Los incrementos de metadata/registry concurrentes correspondieron al ganador, no a admisión parcial de los rechazados. No se traslada esa categoría al fallo prematuro histórico, cuya causa sigue sin determinar. Los hooks observan comandos/respuestas reales, sin retry; snapshots EVAL_RO de solo lectura quedan fuera del RequestBudget y añaden coste diagnóstico, no constituyen benchmark. No se justificó ni aplicó corrección productiva Redis.

### 45.3. Comandos y resultados de esta única regresión final

Ejecutados **secuencialmente**, sin otras suites en paralelo, cambios técnicos previos ni correcciones durante esta validación:

```text
docker info
dotnet restore Enver.Backend.slnx --locked-mode
dotnet build Enver.Backend.slnx --no-restore
dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build
```

| Verificación | Resultado real |
| --- | --- |
| Docker | Cliente/servidor 29.6.1 disponibles, Docker Desktop Linux/WSL2, 4 CPU, 7.698 GiB; sin modificar su configuración. |
| Restore locked | Correcto; proyectos actualizados, sin cambios de lockfiles. |
| Build | **0 warnings / 0 errors**, duración 34,20 s. |
| UnitTests completas | **493/493**, 0 fallidas, 0 omitidas, duración 4 s. |
| IntegrationTests completas | **154/154**, 0 fallidas, 0 omitidas, duración **8 min 58 s**; una sola ejecución, límite externo 900 s. |
| SecurityTests completas | **79/79**, 0 fallidas, 0 omitidas, duración 50 s; ejecutadas solo después de IT verdes. |

Fuente primaria: salidas reales de los seis comandos en la sesión autorizada; sin filtrar tests ni omitir casos. No reaparecieron el fallo prematuro Redis, el timeout histórico Mongo 025 o `ClassFormatError`. No se ejecuta otra regresión, auditoría, benchmark o despliegue para este cierre. Las cifras focales de §§46–52 identifican ejecuciones previas acreditadas y sus clases, ahora incluidas en estas suites completas; no se repitieron focales durante la validación final ni se suman suites compartidas dos veces.

## 46. E(T-AUTH-056) — Composición transversal y transporte

<a id="e-t-auth-056"></a>

**Trazabilidad:** AUTH-008/030–034/036/038; PLD-AUTH-04/13/14/17–19/23; AC-05/14/16/18–20. Archivos: `Program.cs`, `Composition/AuthenticationServiceCollectionExtensions.cs`, `AuthenticationTransport.cs`; pruebas `AuthenticationFoundationCompositionTests.cs` y factory compartida. Autorización/resultados comunes en §45.

- `Program.cs` invoca `AddAuthenticationFoundation`, `AddAuthenticationDependencies` y `UseAuthenticationFoundation`; no contiene reglas funcionales. Options con `ValidateOnStart` mantienen las validaciones JWT/HMAC/Mongo/Redis y del límite de hashing. Configuración inválida impide startup; no se deshabilitan validadores para obtener verde.
- Contexto/correlación, `ProblemDetails` saneados, excepción segura y `Cache-Control: no-store` conectados. Pruebas de 500/traceId y logs verifican que no se vuelque el plaintext sintético de la excepción. Metadata de rutas **solo de test** activa JSON/content-type/tamaño y RequestBudget antes del handler.
- Forwarding nativo únicamente con proxies explícitos, un salto y simetría; se eliminan confianzas de red predeterminadas y no se toma un header arbitrario como autoridad. Se prueba peer confiable/no confiable, headers múltiples/ausentes, esquema y origen no disponible cuando no es verificable. Conexión directa requiere aprobación; HTTP local controlado se limita por ambiente/política.
- HTTPS redirection y HSTS según ambiente/política; las pruebas verifican cabeceras/esquema y ausencia de bucle en el hop autorizado, no TLS externo publicado.
- **Evidencia:** focal previa **15/15 IT** de `AuthenticationFoundationCompositionTests`, íntegra dentro de **154/154 IT** finales; regresión completa y 0 omitidas en §45.3. Probes `/__test/*` residen exclusivamente en tests, no son endpoints productivos. No acredita LB, certificados, AWS o red cloud; no cierra automáticamente el pendiente histórico de 008 fuera de este alcance.

**T-AUTH-056: COMPLETADA / EVIDENCIA REGISTRADA.**

## 47. E(T-AUTH-029) — Contraseña y Unicode estricto

<a id="e-t-auth-029"></a>

**Trazabilidad:** AUTH-006/008/010/040/041; PLD-AUTH-04/12/13; AC-04/05/18. Archivos: `Features/Authentication/Validation/UnicodeInput.cs`, `PasswordPolicy.cs`; pruebas `PasswordPolicyTests.cs`. Autorización/resultados comunes en §45.

- Creación: **12–128 valores escalares Unicode y ≤512 bytes UTF-8**. `Rune.DecodeFromUtf16` rechaza Unicode inválido sin sustitución; no confunde longitud UTF-16 con escalares. Validación de verificación separa límites superiores del mínimo futuro de creación, sin implementar Login.
- Casos 11/12/128/129, suplementarios, surrogates aislados/invertidos y secreto íntegro. **128 suplementarios =512 bytes y 256 unidades UTF-16**; con el tope 128, llegar a 513 bytes exige superar también el número de escalares. Por ello la frontera de bytes 512/513 se contrasta además aisladamente con `UnicodeInput.Fits`, sin afirmar una contraseña válida de 128 escalares/513 bytes.
- No trim, normalización Unicode, cambio de case, truncamiento o composición adicional de password. Pruebas de hashing de 031 contrastan el secreto original con variantes sin espacios, distinto case o normalización compuesta, que no son equivalentes. Passwords sintéticos con espacios/caracteres especiales no son rechazados por reglas de composición inventadas.
- **Evidencia:** focal previa **13/13 UT** de `PasswordPolicyTests`, incluida en **493/493 UT** finales; soporte de preservación exacta en E031 y Unicode malicioso en E030. No se persiste ni registra plaintext por estos validadores.

**T-AUTH-029: COMPLETADA / EVIDENCIA REGISTRADA.**

## 48. E(T-AUTH-028) — Identidad institucional y nombre

<a id="e-t-auth-028"></a>

**Trazabilidad:** AUTH-002/005/007/008/031; PLD-AUTH-02/13/14; AC-01–03/05. Archivos: `Features/Authentication/Validation/InstitutionalEmailNormalizer.cs`, `AuthenticationInputValidator.cs`; pruebas `ValidationTests.cs`. Autorización/resultados comunes en §45.

- Email: trim externo, lowercase invariant, **ASCII**, dominio exacto **uttt.edu.mx**, límites **254 bytes total / 64 local-part**, sintaxis dot-atom no entrecomillada. Rechaza controles, listas, comentarios, display names, comillas, puntos inicial/final/dobles, subdominios y sufijos; sin usar MailAddress como aceptación permisiva.
- La normalización es idempotente; case/espacios externos producen identidad equivalente. No elimina puntos ni `+tag`: esos valores siguen siendo identidades diferentes. No acredita titularidad/verificación de buzón.
- El dominio fijo y local-part ≤64 hacen inalcanzable un email institucional válido de 254 bytes: no se inventa un positivo imposible. Se prueba 64/65 local-part, rechazos de total 254/255 y la restricción de dominio, manteniendo ambos topes.
- Nombre: trim externo, no vacío, **≤200 escalares**, controles rechazados incluso en extremos antes de trim, Unicode inválido rechazado; conserva case, acentos y espacios internos. Casos 200/201 suplementarios y caracteres de control.
- **Evidencia compartida con 030:** focal previa `ValidationTests` **35/35 UT**, incluida en **493/493 UT** finales; no se cuenta dos veces. El assessment separa identidad utilizable para cuotas de campos inválidos sin devolver valores sensibles. La integración funcional de D012/consumo queda en 034–036, no se declara implementada aquí.

**T-AUTH-028: COMPLETADA / EVIDENCIA REGISTRADA.**

## 49. E(T-AUTH-030) — Fronteras y entradas maliciosas

<a id="e-t-auth-030"></a>

**Trazabilidad:** AUTH-005–008/040/041/043; PLD-AUTH-13/20; AC-02/04/05/18. Archivos: `tests/Enver.API.UnitTests/ValidationTests.cs`, `PasswordPolicyTests.cs`; `tests/Enver.API.SecurityTests/MaliciousInputTests.cs`. Implementación contrastada: validadores de E028/E029.

- Fronteras de nombre/email/password, escalares suplementarios, UTF-8, Unicode inválido, ASCII y sintaxis institucional; casos y limitaciones de fronteras en E028/E029.
- **8 ST** sintéticas cubren intento de operador NoSQL como identidad, CRLF, sufijo de dominio, carácter invisible, punto Unicode, lista de direcciones, campos sobredimensionados/controles/surrogate inválido y aceptación íntegra de caracteres especiales como secreto, sin equipararlos a inyección.
- Rechazo sin efectos: validadores puros sin puertos de escritura, hashing, cuotas o emisión; assessment por campo sin incluir los valores rechazados. Esto no demuestra todavía rechazo funcional HTTP sin efectos en Register, no implementado; las pruebas de frontera HTTP transversal corresponden a 056 y la suite funcional final ampliada a 065.
- **Evidencia:** UT compartidas de E028 **35/35** y E029 **13/13**, ST focal previa **8/8**, ahora incluidas en **493/493 UT y 79/79 ST** finales, sin omisiones. Autorización y comandos §45.

**T-AUTH-030: COMPLETADA / EVIDENCIA REGISTRADA.**

## 50. E(T-AUTH-031) — PasswordHasher y dummy preinicializado

<a id="e-t-auth-031"></a>

**Trazabilidad:** AUTH-006/010/011/013/014/041; PLD-AUTH-03/12/20; AC-04/06/07. Archivos: `Features/Authentication/Application/Ports/IPasswordService.cs`, `Security/PasswordService.cs`, `DummyPasswordHash.cs`, wiring en Composition; pruebas `PasswordServiceTests.cs` y composición 057.

- `PasswordHasher<User>` del framework mantiene salt/formato/parámetros/versionado. Configuración efectiva de composición: `PasswordHasherOptions` predeterminadas del framework .NET 10, sin overrides ni iteraciones manuales elegidas para el incremento; no Identity completo, criptografía propia, cifrado reversible o workarounds de latencia.
- Verificación mapea **Success / Failed / SuccessRehashNeeded**; secretos iguales producen hashes con salts distintos, secretos distintos fallan y se preserva plaintext exacto. El vector legado lo genera el propio framework en modo IdentityV2 solo en test; no altera la configuración productiva ni persiste un rehash.
- `DummyPasswordHash` es singleton de DI con `Lazy` ExecutionAndPublication, una inicialización por contenedor de proceso/host; el hosted service lo prepara antes de atender requests. Mismo hasher/configuración que las verificaciones reales, sin generar hash dummy por request. No equivale a compartir un dummy estático entre varios hosts de test independientes.
- El adapter no depende de stores ni escribe usuarios, no conserva plaintext como estado ni lo registra; resultados/ToString no exponen el hash. Logs/diagnósticos del incremento no publican secretos ni hashes. No se afirma ausencia universal de filtraciones de componentes futuros.
- **Evidencia:** focal previa **7/7 UT**, incluida en **493/493 UT** finales; dummy preinicializado y servicio real por DI en E057, dentro de **154/154 IT**. No implementa CAS/rehash 032, Login, JWT ni igualación exacta de tiempos.

**T-AUTH-031: COMPLETADA / EVIDENCIA REGISTRADA.**

## 51. E(T-AUTH-033) — Concurrencia y ownership del hashing

<a id="e-t-auth-033"></a>

**Trazabilidad:** AUTH-008/011/026/031/038/040/041; PLD-AUTH-14/18/20; AC-07/18. Archivos: `Security/HashingConcurrencyGate.cs`, integración en `PasswordService.cs`; `HashingConcurrencyGateTests.cs`, `HashingConcurrencyIntegrationTests.cs` y composición 057.

- Gate configurable por `Authentication:HashingConcurrencyLimit`, **límite efectivo 2 en composición verificada**; `SemaphoreSlim.Wait(0)` admite inmediatamente o devuelve saturación, sin cola de espera ilimitada ni `Task.Run`. El servicio representa saturación como `Unavailable`, disponible para futura respuesta 503 segura, sin publicar todavía un endpoint funcional.
- El lease pertenece al trabajo síncrono real y solo se libera al salir, también ante excepción; liberación idempotente. Cancelar al consumidor mientras el hasher sigue bloqueado no devuelve el permiso anticipadamente ni permite otro hash. Presupuesto ya cancelado no adquiere cupo.
- **Evidencia:** focal previa **2/2 UT** (incluye saturación al tercer intento con límite 2) y **3/3 IT** de trabajo real/bloqueo, cancelación y error. Esas IT usan límite 1 para aislar ownership y un thread controlado de test, no `Task.Run` productivo; no se presentan como medición de dos trabajos simultáneos. Integración de DI confirma límite 2. Incluidas en **493/493 UT y 154/154 IT** finales.
- No se reduce la seguridad del hasher; calibración/perfil 067 y dimensionamiento/SLA productivos siguen pendientes.

**T-AUTH-033: COMPLETADA / EVIDENCIA REGISTRADA.**

## 52. E(T-AUTH-057) — Dependencias reales y admisión interna

<a id="e-t-auth-057"></a>

**Trazabilidad:** AUTH-009/015/025–028/038; PLD-AUTH-05–08/19/24; AC-03/14/15/17. Archivos: Composition `AuthenticationServiceCollectionExtensions.cs`, `AuthenticationDependencyOptions.cs`, `AuthenticationDependencyAdmission.cs`, wiring `Program.cs`, factory compartida y `AuthenticationDependencyCompositionTests.cs`.

- MongoClient/RedisConnections registrados singleton y reutilizados; DI resuelve `MongoUserStore`, `RedisAuthenticationQuotaStore`, pseudonimizador HMAC, servicio de password y gate reales. Se reutilizan los adapters/contratos ya aprobados, no se cambia BSON ni Lua. Referencias externas de configuración/identidad/generación y capacidad deben ser explícitas; no defaults correctivos ni generaciones generadas automáticamente al arrancar.
- Hosted service activa detector, preinicializa dummy y ejecuta inicializador Mongo. Admisión interna comprueba índice para la futura operación Register y confianza local/compartida; **Ready** permite continuar solo si los demás controles son aptos. **Incompatible** y **Unavailable** cierran admisión, preservando liveness; no reparan índices/datos ni relajan presupuestos. La evaluación de estado compartido/futura Logout no consulta Mongo de usuarios artificialmente.
- Pruebas con MongoDB/Redis reales locales: índice incompatible conservado y 503 en probe; dependencia Mongo temporalmente no disponible con liveness 200 y admisión dependiente 503; índice Ready con confianza apta; cierre global conservado y diagnóstico saneado. Clientes reutilizables/dummy listo comprobados por DI.
- Se conserva **GET /health como liveness**, sin crear `/health/ready`. Probes de admisión solo en assembly IT. Las rutas productivas register/login/logout siguen 404; no implementan operaciones de negocio.
- Arranque no inicializa cuotas/autoridad ni reabre seguridad. Ausencia de autoridad queda fail-closed; cuota sin metadata no se admite. Solo el escenario de test prepara explícitamente su autoridad propia y ejecuta el lifecycle de un ambiente nuevo verificado para probar el adapter real; no es bootstrap productivo implícito. La composición de autoridad conserva adapter existente y región/credenciales externas, acreditándose únicamente DynamoDB Local sintético, no AWS real.
- **Evidencia:** focal previa **7/7 IT** de `AuthenticationDependencyCompositionTests`, incluidas en **154/154 IT** finales. Junto con E056 (15) y E033 (3), forman las **25 IT** nuevas de composición/concurrencia/admisión, sin doble conteo. El correctivo independiente de 24 casos y los 11 casos focales de 025 son antecedentes de §45.2, no sustituyen la regresión final.

**T-AUTH-057: COMPLETADA / EVIDENCIA REGISTRADA.**

## 53. Aceptación de BUILD-1 y checkpoint preparado

<a id="build-1-aceptacion"></a>

El responsable condiciona aceptación a restore locked, build y UT/IT/ST completas verdes, sin fallidas u omitidas. **Condición satisfecha en §45.3: BUILD-1 ACEPTADO**, con las siete tareas **056/029/028/030/031/033/057 COMPLETADAS / EVIDENCIA REGISTRADA**, individualmente en §§46–52. No se aceptó ninguna de las regresiones rojas anteriores ni se reclasificó como capacidad real el fallo prematuro no explicado.

**GATE-A/B/C SATISFECHOS / CERRADOS**, en sus alcances previos intactos. **GATE-D PENDIENTE / NO SATISFECHO**, porque **034,035,036,037 siguen PENDIENTE / NO EJECUTADA**. No se autoriza iniciarlas por este cierre. GATE-E–H siguen pendientes, **032/058** no se ejecutan, **008** conserva su estado documental previo sin cierre automático; **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**. No se acreditan Atlas, TLS publicado, HA/failover cloud, LB, AWS/IAM, Terraform o despliegue.

Checkpoint preparado únicamente en código/tests ya existentes y estos dos documentos: **sin stage, commit o push**, HEAD base conservado. La validación final no modificó producto/tests/instrumentación/fixture, parámetros o dependencias; solo después de todas las suites verdes se actualizan `task.md` y `memoria.md`. Las restricciones/historias de §§1–44 y memoria §§1–48 se conservan en su contexto. No se modifican spec/plan, constituciones, foundation, AGENTS/README o perfiles de agentes/skills. Detener sin generar trabajo adicional ni repetir pruebas.

### 53.1. Solicitud posterior de checkpoint Git — bloqueo tras limpieza test-only

El responsable autoriza posteriormente **AUTH BUILD-1 — CHECKPOINT GIT**, con revisión completa de los 8 modificados/20 nuevos y retirada de instrumentación temporal antes de stage. Su punto 5 permite validación posterior si la limpieza cambia lógica ejecutable del test. Esta autorización posterior sustituye exclusivamente la prohibición anterior de stage/commit para un checkpoint coherente; no autoriza push, correcciones productivas ni BUILD-2.

Se revisaron completos los 20 nuevos y todos los diffs. Se conservaron como cambios estables la configuración sintética de factories históricas, los dobles explícitos test-only y `OwnedCompositionAuthority` de 057 con su cleanup exclusivo. Se retiró de `RedisAuthenticationQuotaCapacityTests.cs` la clase temporal `RedisCapacityDiagnostic`, incluidos hooks/stores observados, snapshots EVAL_RO, profiler, suscripciones de excepciones/conexión y reloj/timers diagnósticos. Los dos casos conservan requests, escenarios y aserciones, usando `FillRegisterAsync` y los dos stores reales del scope sin observadores. `RedisAuthenticationQuotaTestScope.cs` pierde únicamente el parámetro/ruta diagnóstica opcional y queda sin diferencias respecto al HEAD base. Presupuestos Redis **10000/2000 ms**, capacidad **5000**, producto, fixture y contratos intactos. Esto **no fue solo eliminación de logging**: cambia el recorrido ejecutable de los tests y elimina la carga temporal de las observaciones; no demuestra causa de las incidencias históricas.

Por esa excepción se ejecutaron una vez y en secuencia:

```text
dotnet build Enver.Backend.slnx --no-restore
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build
```

Build **0 warnings / 0 errors**, **33,70 s**. IT **153/154**, **1 fallida, 0 omitidas**, **8 min 22 s**, límite externo 900 s. Falló `MongoUsersInitializerIntegrationTests.Existing_duplicate_data_blocks_readiness_and_is_not_reconciled`. La instrumentación existente de 025 registra `createIndexes` iniciado, cancelación del token de dependencia al agotarse el presupuesto de **500 ms**, `INITIALIZER_RETURN=Unavailable`, RequestBudget inicial **8000 ms** con **7503,303 ms** restantes, y `TaskCanceledException` del comando. La aparición inmediata de metadata de índice no acreditó readiness: hint rechazado, build aún observable y desaparición posterior del índice; se conservaron los dos documentos originales. No se obtuvo respuesta de servidor que acreditara el resultado esperado dentro del presupuesto. Se registra lo observado, no una causa raíz ambiental/productiva demostrada ni una relación causal con la limpieza Redis. No hubo otro fallo de test Redis ni `ClassFormatError` en esta ejecución.

**CHECKPOINT GIT BLOQUEADO / NO CREADO**: no stage ni commit ante validación posterior roja; sin retry, nuevas pruebas, cambios de timeout, reparación o relajación de aserciones. UT/ST no se repiten: **493/493 y 79/79** pertenecen a la aceptación histórica de §45.3, no a una regresión íntegra posterior a esta limpieza. Esa aceptación y E056/E029/E028/E030/E031/E033/E057 se conservan como historia, pero no acreditan verde del árbol posterior a la limpieza. El árbol pendiente pasa a **7 modificados + 20 nuevos (27 archivos)** porque el scope Redis queda idéntico al base, no por pérdida de archivos. HEAD sigue `8db9b6082ad3e826169d16da0df0b38d0e0ef442`; GATE-A/B/C cerrados y GATE-D pendiente, sin iniciar 034–037, sin push. Se conserva el trabajo y se detiene a la espera de autorización posterior delimitada.

**Estado posterior — revalidación del árbol post-limpieza autorizada y satisfecha.** La solicitud independiente **AUTH BUILD-1 — REVALIDACIÓN DEL ÁRBOL LIMPIO Y CHECKPOINT** autoriza primero el focal 025 y, solo si pasa, una regresión completa secuencial y el checkpoint de los **27 archivos**. Rama/base comprobadas, **7 modificados + 20 nuevos / 0 staged** al inicio. Sin cambios de producto, tests, expectativas, fixture, presupuestos o capacidad; sin retries ni reintroducción de instrumentación temporal. Se ejecutaron una vez, en este orden:

```text
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~MongoUsersInitializerIntegrationTests"
dotnet restore Enver.Backend.slnx --locked-mode
dotnet build Enver.Backend.slnx --no-restore
dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build
```

Fuente primaria: salidas reales de esta sesión autorizada. Focal **025 11/11**, 0 fallidas/omitidas, **5 s**; restore locked correcto; build **0 warnings / 0 errores**, **18,44 s**; UT completas **493/493**, **4 s**; IT completas **154/154**, **7 min 44 s**, una sola ejecución con límite externo **900 s**; ST completas **79/79**, **49 s**, solo después de IT verdes. **0 fallidas / 0 omitidas** en todas las suites. No reaparecieron el timeout 025, el fallo prematuro Redis ni `ClassFormatError`. El diagnóstico temporal Redis sigue ausente; se conservan los diagnósticos estables y contratos anteriores.

**ÁRBOL POST-LIMPIEZA ACEPTADO / REVALIDADO**; queda satisfecho el bloqueo de validación del intento de checkpoint anterior, cuyo fallo y límites se conservan arriba sin atribuirle una causa raíz no demostrada. Se mantienen intactas **E056 §46, E029 §47, E028 §48, E030 §49, E031 §50, E033 §51 y E057 §52** y los siete estados COMPLETADA / EVIDENCIA REGISTRADA. La autorización permite ahora stage exclusivo de los **27 archivos revisados** y un commit con mensaje exacto `feat(auth): add validation hashing and dependency composition`, condicionado a la revisión del índice; el hash resultante y estado final se informan en la salida Git de la sesión. Únicas ediciones de esta revalidación: esta nota y memoria §49.5, después del verde. **GATE-A/B/C cerrados; GATE-D PENDIENTE / NO SATISFECHO**, sin iniciar 034–037/BUILD-2. Sin push, pull, fetch, merge o rebase; sin implementar Register/Login/Logout/JWT, Orders o AWS/Terraform. Detener después de verificar el checkpoint local.

## 54. BUILD-2 — Validación final, historial y aceptación de Register

<a id="build-2-validacion-final"></a>

### 54.1. Autorización, baseline y entorno

Registro del **11 de noviembre de 2026**. Las solicitudes posteriores **AUTH BUILD-2** autorizan implementar 034–037, diagnosticar/corregir exclusivamente el harness y, por último, **VALIDACIÓN FINAL, EVIDENCIAS Y CIERRE GATE-D**. Esta última permite solo ST completas y, si pasan sin fallidas/omitidas, actualizar este documento y `memoria.md`; prohíbe repetir UT/IT completas, modificar producto, iniciar otra capacidad o hacer stage/commit/push. Las solicitudes prematuras de checkpoint no produjeron commit: faltaban ST completas y cierre documental. Se conserva esa separación entre implementación, evidencia, aceptación y checkpoint posterior.

Rama **`feature/authentication`**, HEAD base y conservado **`fe7858dc054a21a68fc051f95db6cb2e7383f8a4`**; baseline **2 modificados + 14 nuevos, nada staged**. Ese HEAD corresponde al checkpoint posterior de BUILD-1; no se reescribe su historial de §53.1. Entorno .NET 10, runtime de pruebas **10.0.14**, fixtures/imágenes/digests existentes, Docker Desktop/WSL2, MongoDB local real y Redis local real. Autoridad propia explícita en DynamoDB Local, sin recursos AWS reales. Los dos hosts de la carrera son factories/clientes y estados locales independientes, no dos máquinas físicas ni un balanceador publicado.

### 54.2. Historial correctivo y limpieza, sin borrar intentos rojos

- Primer focal IT agrupado: **7/39** verdes; los **32** casos nuevos no alcanzaron sus aserciones por un nombre de base sintética ajeno al prefijo obligatorio del ambiente. Corrección test-only: derivarlo de `fixture.MongoDatabase`, sin relajar el validador ni tocar producto.
- Focal posterior: **38/39**, con un `EqualException` en el caso origin 20/900; la captura inicial saneada no identificaba la comparación. No se imputó un defecto productivo sin evidencia. Dos ejecuciones diagnósticas aisladas localizaron el fallo en **setup**, antes del request 1: expected `initialized`, actual `unavailable`, categoría `closed`, causa `DependencyTimeout`; **0 requests, 0 llamadas de cuota, 0 EVAL de inicialización**. La comprobación local observada duró **330,7 ms** frente a **250 ms** de presupuesto de dependencia; no es una medición aislada del tiempo físico de conexión ni prueba de un fallo en Lua/conteo/Retry-After.
- Corrección autorizada únicamente en `RegistrationTestScope.cs`: abrir su `RedisConnections` real y completar `PING` bajo el token de setup, **antes** del lifecycle. Preparación **TEST-ONLY**, sin cuota, Lua, buckets, mutación de autoridad, retries ni ampliación de los **250 ms**. Tras ella: origin **1/1**, Register/composición **39/39** y ST focales **5/5** verdes.
- El diagnóstico temporal no era solo logging: agregaba snapshots before/after y un `executeOverride` cuya obtención de conexión ocurría después de `MarkDispatched`, a diferencia de la ruta normal. Se detuvo el cierre al identificar ese cambio de recorrido. La solicitud independiente **LIMPIEZA COORDINADA DEL DIAGNÓSTICO Y REVALIDACIÓN FOCAL** autorizó limpiar ambos archivos IT y revalidar focalmente, sin repetir IT completas.
- Se retiraron íntegramente `OriginQuotaDiagnostic`, output, snapshots, hooks/`executeOverride`, wrappers de autoridad/seguridad, estructuras y captura temporal de comparaciones/stack; se conservó el saneamiento de fallos. `RegistrationTests.cs` vuelve a `SendAsync`; `QuotaProbe` cuenta/delega al store real resuelto de la composición, sin sustituir su frontera de despacho. Requests, escenarios, aserciones funcionales, cuotas, `Retry-After`, stores reales y pre-warm estable se conservan. **No queda instrumentación diagnóstica temporal de este incremento.**

### 54.3. Comandos y resultados reales

Fuentes primarias: salidas de las ejecuciones autorizadas de esta conversación, incluida la notificación final de IT completas; no estimaciones ni cifras de BUILD-1.

```text
dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build --filter "FullyQualifiedName~AuthenticationServiceTests"
dotnet restore Enver.Backend.slnx --locked-mode
dotnet build Enver.Backend.slnx --no-restore
dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build
```

El focal UT pertenece a la implementación inicial, anterior a la regresión. Después, la limpieza autorizada ejecutó, en orden:

```text
dotnet build Enver.Backend.slnx --no-restore
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RegistrationTests.Origin_20_per_900_joint_with_email_3_per_900_has_one_charge_and_conservative_retry_after"
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~RegistrationTests|FullyQualifiedName~AuthenticationDependencyCompositionTests"
```

Finalmente, y sin repetir UT/IT en este cierre:

```text
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build
```

| Verificación | Resultado y relación temporal |
| --- | --- |
| Focal `AuthenticationServiceTests` | **13/13 UT**, 0 fallidas/omitidas; incluido en UT completas. |
| Restore locked | Correcto, sin cambios de dependencias/lockfiles. No repetido en el cierre documental. |
| Build previo a regresión | **0 warnings / 0 errores**, **18,97 s**. |
| UT completas | **506/506**, 0 fallidas/omitidas, **4 s**; anteriores a la limpieza exclusivamente IT, producto/UT intactos después. |
| IT completas | **186/186**, 0 fallidas/omitidas, **8 min 20 s**, una ejecución con límite externo 900 s; anteriores a retirar el diagnóstico del caso origin. |
| Build post-limpieza | **0 warnings / 0 errores**, **32,41 s**; solución completa y fuentes sin diagnóstico. |
| Origin post-limpieza | **1/1**, 0 fallidas/omitidas, **5 s**; request 21 →429, `Retry-After` 1–900 s. |
| Register/composición post-limpieza | **39/39 IT**, 0 fallidas/omitidas, **32 s**: 32 casos Register + 7 de composición. Incluye origin; no se suma como un caso distinto a las 186 IT. |
| ST focales Register | **5/5**, 0 fallidas/omitidas, evidencia previa conservada: el proyecto/clase ST no referencia el scope IT eliminado; ahora también incluidas en ST completas. |
| ST completas finales | **84/84**, **0 fallidas / 0 omitidas**, **1 min 14 s**, sin filtro y sin build, sobre los binarios del build post-limpieza. |

**No se presenta 186/186 como una nueva ejecución post-limpieza.** El responsable acepta expresamente para este cierre UT/IT completas ya acreditadas más build y focales post-limpieza, sin repetirlas; el cambio posterior se limita al harness IT y restaura la ruta productiva real. Las ST completas actuales satisfacen la condición restante. No hubo corrección productiva, cambio de aserciones funcionales, límites, presupuestos o paquetes después de esas evidencias. Las pruebas locales no acreditan SLA/rendimiento productivo ni eliminan las incidencias históricas de §45/53.

### 54.4. Archivos y decisión

Archivos nuevos del incremento, bajo `Enver.API/`: `Features/Authentication/Contracts/RegisterRequest.cs`, `RegisterResponse.cs`; `Features/Authentication/Application/AuthenticationInputs.cs`, `AuthenticationResults.cs`, `AuthenticationService.cs`, `Ports/IRegisterControls.cs`; `Controllers/AuthenticationController.cs`; `Composition/RegisterControls.cs`; `Http/RegisterBoundaryContinuation.cs`; `Observability/AuthenticationEvents.cs`. Wiring previo modificado: `Composition/AuthenticationServiceCollectionExtensions.cs`.

Pruebas nuevas: `tests/Enver.API.UnitTests/AuthenticationServiceTests.cs`, `tests/Enver.API.IntegrationTests/RegistrationTests.cs`, `RegistrationTestScope.cs` y `tests/Enver.API.SecurityTests/RegistrationSecurityTests.cs`. Ajuste estable del test histórico `AuthenticationDependencyCompositionTests.cs`: Register ya no espera 404, exige 503 con autoridad no apta; Login/Logout conservan 404. No relaja fail-closed ni inicializa seguridad desde producto.

**BUILD-2 ACEPTADO. T-AUTH-034/035/036/037 COMPLETADAS / EVIDENCIA REGISTRADA**, separadamente en §§55–58; **Register OPERATIVO LOCAL/TEST**. La revisión y decisión del gate se registran como acto explícito en §59, no por mera existencia de clases. Solo después del verde final se modifican este documento y `memoria.md`; **sin stage/commit/push** y sin autorización automática del siguiente incremento.

## 55. E(T-AUTH-034) — Contratos y flujo de Register

<a id="e-t-auth-034"></a>

**Trazabilidad:** AUTH-002–009/012–015/038; PLD-AUTH-02/05/12/13; AC-01–05. Archivos: contratos Register, `AuthenticationInputs.cs`, `AuthenticationResults.cs`, `AuthenticationService.cs`, `Ports/IRegisterControls.cs`; prueba `AuthenticationServiceTests.cs`. Autorización, entorno y comandos en §54.

- Request exacto **`fullName/email/password`**, obligatorios; response exacta **`id/fullName/email`**. JSON camelCase estricto mediante la frontera/serializer existentes: sin desconocidos, duplicados, null, tipos indebidos, coerción ni mass assignment. Inputs/results internos no dependen de MVC/BSON/Redis y sus `ToString` no vuelcan datos sensibles.
- D012: **413/415 terminales sin cuota**; preflight obligatorio antes del 400; JSON ilegible/identidad inválida no crean buckets. Email institucional ASCII normalizado por `InstitutionalEmailNormalizer`; identidad apta → una cuota conjunta antes de evaluar defectos secundarios y antes de hashing. Se reutilizan `AuthenticationInputValidator`, `PasswordPolicy`, `IPasswordService`, `HashingConcurrencyGate` e `IUserStore`; misma identidad normalizada en cuota, persistencia y response.
- Nombre/password válidos antes del hash; UUIDv4, **Active**, instante **UTC backend** y **`createdAt == updatedAt`** creados por servidor. El documento final usa el hash obtenido; plaintext no se persiste. No reintento con otra identidad, consulta previa, token, cookie o auto-login.
- **13/13 UT** verifican orden `admission → quota → hash → insert`, ausencia de hash/escritura en rechazos correspondientes, saturación, identidad/timestamps, proyección y mapeo Confirmed/Conflict/Unavailable/Uncertain. Controles fallidos no se convierten en éxito. Los rechazos posteriores a admisión conservan su consumo legítimo, no se afirma ausencia universal de efectos.
- Regresión aplicable: **506/506 UT**, focal IT post-limpieza **39/39**, **186/186 IT** acreditadas y **84/84 ST** finales, sin doble conteo; relación temporal en §54.3.

**T-AUTH-034: COMPLETADA / EVIDENCIA REGISTRADA.**

## 56. E(T-AUTH-035) — Endpoint, persistencia y contrato HTTP

<a id="e-t-auth-035"></a>

**Trazabilidad:** AUTH-003/004/009/031/032/038/039; PLD-AUTH-05/14/19; AC-01/03/17/19. Archivos: `Enver.API/Controllers/AuthenticationController.cs`, flujo/proyecciones de 034, wiring y continuación de frontera; `RegistrationTests.cs` y `RegistrationTestScope.cs`.

- **`POST /api/v1/auth/register`**, anónimo (`AllowAnonymous`); sin binding MVC permisivo del cuerpo. Parseo limitado por la frontera existente y extracción sin coerción cuando hay defectos secundarios, conservando la precedencia D012.
- **201 solo con `UserInsertStatus.Confirmed`**. Mongo real contiene un documento de **siete campos exactos**: `_id/fullName/normalizedEmail/passwordHash/status/createdAt/updatedAt`; UUIDv4, Active y timestamps iguales. Hash verificado con el hasher real del framework. Response únicamente `id/fullName/email`, email normalizado, sin password/hash/status/accessToken/refreshToken ni otro campo, sin `Set-Cookie`, login automático o `Location` ficticio.
- Índice único de `normalizedEmail` determina **409 genérico `registration_conflict`** para emails equivalentes; no read-before-write. El store previo clasifica el duplicado específico del índice/email, no cualquier error Mongo como conflicto. **Unavailable/Uncertain →503**. Una inserción por intento, sin compensating delete, retry o nuevo UUID tras incertidumbre.
- La prueba Uncertain usa un decorador **test-only**: ejecuta la inserción real confirmada y oculta el resultado; observa 503 y un documento conservado. Es evidencia del tratamiento del resultado incierto y ausencia de compensación, **no pérdida física de ACK Mongo**. Unavailable sin envío se modela por el decorador y la admisión ante Mongo no disponible se contrasta además con destino real inaccesible. `/health` conserva liveness 200.
- Problem Details seguro, correlación cabecera/cuerpo y **`Cache-Control: no-store`** verificados en `AssertProblemAsync`; no se exponen dependencias, cuerpos ni valores sensibles.

### Contrato HTTP final de Register

| Elemento | Contrato / evidencia |
| --- | --- |
| Request | Objeto con exactamente `fullName: string`, `email: string`, `password: string`; campos obligatorios y validación estricta de 034. |
| Éxito | **201**; objeto exacto `id: UUID`, `fullName: string`, `email: normalizedEmail`. |
| Validación | **400 `validation_failed`**. |
| Conflicto | **409 `registration_conflict`** genérico. |
| Cuerpo excesivo | **413 `payload_too_large`**, terminal sin cuota. |
| Media/charset/encoding | **415 `unsupported_media_type`**, terminal sin cuota. |
| Cuota excedida | **429 `rate_limited`**, `Retry-After` conservador cuando calculable. |
| Dependencia/confianza/saturación/resultado incierto | **503 `temporarily_unavailable`**, fail-closed. |
| Inesperado | **500 `internal_error`**, solo manejo transversal de excepción inesperada; soporte de E056/errores existentes y regresiones, no un nuevo escenario de caída física introducido en Register. |

**Sin 422** ni login/JWT implícitos. Casos HTTP/persistencia dentro de los **32 IT Register**, revalidados con composición **39/39** post-limpieza; suites completas y límites en §54.3.

**T-AUTH-035: COMPLETADA / EVIDENCIA REGISTRADA.**

## 57. E(T-AUTH-036) — Cuotas conjuntas y observabilidad segura

<a id="e-t-auth-036"></a>

**Trazabilidad:** AUTH-028–030/032/036–038; PLD-AUTH-15/16/19/23; AC-16/19. Archivos: `Composition/RegisterControls.cs`, `AuthenticationService.cs`, `Http/RegisterBoundaryContinuation.cs`, `Observability/AuthenticationEvents.cs`; UT/IT/ST del incremento.

- Email **3/900 s** y origen **20/900 s**, Redis compartido real. El adapter envía ambos ámbitos en **una evaluación conjunta por request**; no hay consumo adicional en filtro, mapping o error, ni retry/devolución. Se reutiliza sin cambios el store/protocolo/Lua de cuotas y su InternalAttemptId común a los ámbitos; no se declara idempotencia pública ni exactly-once distribuido ante fallos.
- Caso origin post-limpieza: **20 identidades válidas distintas** reciben cuota admitida y HTTP 400 por nombre inválido; la **21.ª identidad →429**. **21 llamadas para 21 requests**, **21 particiones** (20 emails + un origen), cero inserciones; el email rechazado no crea una partición parcial. Máximo esperado por email admitido en este escenario: un intento, sin alcanzar 3/4; el límite email no bloquea prematuramente el origen. El caso separado de defectos secundarios admite tres consumos del mismo email y rechaza el cuarto.
- La aserción permanente comprueba `Retry-After` entre **1 y 900 s**, no igualdad fija con 900 ni un reloj local artificial. La fórmula existente usa **Redis TIME**, evento relevante de cada ámbito bloqueante y `ceil(max(reset − now)/1_000_000)`; HTTP conserva el redondeo superior en segundos. El diagnóstico previo observó 900 s para 899124337 µs restantes, sin cargo parcial del rechazo; se conserva como evidencia histórica, **no como snapshot de la ejecución post-limpieza**, que ya no instrumenta Redis. Soporte adicional: pruebas de cuotas/calculador existentes incluidas en las regresiones y lectura del Lua intacto. El rechazo ocurre antes de la fase de escritura, sin extender penalización.
- Cuota **Unavailable/Uncertain →503 antes de defectos secundarios/hash**, sin retry. El decorador Uncertain test-only ejecuta el consumo real y oculta su admisión confirmada; no representa pérdida física de ACK. Consumo admitido permanece aunque después haya 400 de campos, 409 Mongo o 503 por saturación de hashing; casos específicos lo contrastan con el rechazo del siguiente intento.
- `AuthenticationEvents` acepta enums cerrados y registra únicamente **Operation/Kind/Outcome** (más el campo técnico de plantilla del logger); sin cuerpo, email, password, hash, tokens, HMAC, claves, BSON o conexiones en los eventos. IT comprueba ausencia de secretos/identidades sintéticas y ST la allowlist/tipos; no es una afirmación universal sobre telemetría futura.
- **Pre-warm Redis exclusivamente TEST-ONLY**: conecta y PING antes del lifecycle, no consume cuota/Lua ni cambia autoridad. Conserva los **250 ms**, cuotas, capacidad **5000**, configuración real y fail-closed; no es una reparación productiva ni benchmark.

**Evidencia:** origin **1/1** y Register/composición **39/39** post-limpieza; ST focal **5/5** y ST completas **84/84**; UT/IT acreditadas según §54.3. **T-AUTH-036: COMPLETADA / EVIDENCIA REGISTRADA.**

## 58. E(T-AUTH-037) — Register integrado y seguridad

<a id="e-t-auth-037"></a>

**Trazabilidad:** AUTH-002–009/012–015/031/032/039/042/043; PLD-AUTH-05/06/13/14/20; AC-01–05/17/19. Archivos: `tests/Enver.API.UnitTests/AuthenticationServiceTests.cs`, `tests/Enver.API.IntegrationTests/RegistrationTests.cs`, `RegistrationTestScope.cs`, `AuthenticationDependencyCompositionTests.cs`, `tests/Enver.API.SecurityTests/RegistrationSecurityTests.cs`.

- **Composición real 056/057**, controlador/continuación y stores Mongo/Redis reales; setup exclusivo por caso con base/namespace y autoridad propia explícita, cleanup solo de recursos propios. No bootstrap, reapertura ni fallback de seguridad productivos.
- 201 válido, documento de siete campos, hash real verificable y ausencia de plaintext; no token/JWT/auto-login. En carrera de **dos hosts/clientes independientes**, email equivalente produce **un 201 + un 409 + una cuenta final**, sustentada por índice único, no por consulta de existencia.
- Entradas inválidas sin escritura, campos secundarios inválidos con consumo legítimo único, JSON malformado, null/tipos incorrectos, duplicados/case indebido, **mass assignment** y **payload NoSQL-like** rechazados sin buckets arbitrarios de identidad. Nombre/email/password y límites puros de E028–E030 se conservan; no se convierte un operador recibido en consulta Mongo.
- Frontera **413/415**, cuota **429**, dependencia/autoridad no apta/cuota/persistencia inciertas/saturación **503**. No-store, correlación, Problem Details saneado y ausencia de secretos en respuestas/logs; Login y Logout siguen sin implementarse (404 en los casos de ausencia de rutas).
- **13/13 UT** focales iniciales; **32 IT Register + 7 IT composición =39/39 post-limpieza**; **5 ST focales** verifican contratos/ToString, campos desconocidos/objeto/duplicado y eventos estructurados de allowlist. Regresiones **506/506 UT**, **186/186 IT** acreditadas y **84/84 ST finales**, todas sin fallidas/omitidas; ejecución y representatividad temporal detalladas en §54.3, sin sumar focales como suites adicionales.

**T-AUTH-037: COMPLETADA / EVIDENCIA REGISTRADA. Register OPERATIVO LOCAL/TEST.**

## 59. Revisión formal y cierre local/test de GATE-D

<a id="cierre-gate-d"></a>

Acto expresamente autorizado por el responsable, posterior a las **84/84 ST** completas verdes. Se revisan conjuntamente las evidencias, no se deduce aceptación de un gate por el cierre de una tarea aislada:

| Evidencia revisada | Capacidad / contraste actual |
| --- | --- |
| **E028 §48** | Identidad institucional compartida y normalización idempotente, nombre; se usa la misma identidad en cuota/documento/response. |
| **E029 §47** | Política password/Unicode/bytes y secreto íntegro, sin rebajar seguridad ni inventar composición. |
| **E030 §49** | Fronteras y entradas maliciosas; ampliación HTTP funcional en E034/E037 sin escritura indebida. |
| **E031 §50** | PasswordHasher del framework y dummy preinicializado; hash persistido verificable, sin plaintext ni token. No rehash 032. |
| **E033 §51** | Gate de hashing, saturación y ownership; E036 verifica 503 posterior sin devolución de cuota. |
| **E034 §55** | Contratos exactos y orden D012, validación/cuota antes del coste, UUIDv4/Active/UTC backend. |
| **E035 §56** | 201 solo Confirmed; 409 genérico por índice específico; 503 seguro, sin lectura previa/retry/compensación. |
| **E036 §57** | Cuota Redis conjunta única 3/900 y 20/900, 429/Retry-After, fail-closed y eventos allowlist. |
| **E037 §58** | Integración positiva, negativa, maliciosa y concurrente: un 201/un 409/una cuenta, sin token/auto-login, no-store y privacidad. |
| **E056 §46** | Frontera/errores/correlación/presupuestos/transporte conectados; continuación Register mantiene 413/415 terminales y preflight antes del 400. No TLS público. |
| **E057 §52** | Clientes/stores reales, índice y admisión fail-closed, sin bootstrap de autoridad/cuotas desde host. El estado histórico Register404 de BUILD-1 queda sucedido por Register503 con autoridad no apta y Register201 solo con controles aptos; Login/Logout404 se conserva. |

**Criterios de salida acreditados:** 034–037 verificadas; **201 confirmado**, **409 genérico determinado por índice**, sin login/token automático; validación/cuota antes del coste y hashing seguro; **Mongo/Redis reales**, composición apta y admisión fail-closed; **no-store/logging seguro**, pruebas positivas/negativas/concurrentes y maliciosas. Resultados finales aceptados bajo la secuencia y límites de §54.3, sin afirmar una repetición de las 186 IT post-limpieza.

**GATE-D: SATISFECHO / CERRADO — LOCAL / TEST REPRESENTATIVO. BUILD-2 ACEPTADO.** GATE-A/B/C permanecen SATISFECHOS / CERRADOS en sus alcances; **GATE-E/F/G/H pendientes**. 032, JWT/Login/Logout, integración final 058, Orders y siguientes incrementos no se inician. 008 conserva su pendiente documental de startup productivo; **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**, distinto de T-AUTH-027 Mongo.

**Límites y riesgos conservados:** no acredita AWS, deployment cloud, Load Balancer, HTTPS público/TLS publicado, Atlas HA, Redis administrado productivo, failover real/productivo ni posesión del correo institucional. El contraste 201/409 mantiene **enumeración residual**; **NAT/origen compartido** conserva limitaciones; el dominio **uttt.edu.mx no prueba titularidad**. Tampoco cierra la seguridad integral, observabilidad/CI/CD o aceptación externa de 068/079.

**Integridad y entrega:** Program.cs mantiene solo composición, sin lógica de negocio; User/BSON de siete campos y contratos Mongo intactos, store/protocolo/Lua Redis intactos; cuotas **3/900 y 20/900**, capacidad **5000** y timeouts (incluidos **250 ms** del harness) intactos; csproj/lockfiles/fixture/imágenes/digests sin cambios, **0 paquetes nuevos**. Huella SHA-256 del árbol productivo fuente **idéntica al inicio y al final** del cierre: `07FE3DB35CC38C8AA02D4A3507D35A66524715109F6E2D5EBA714D65BFF695A5`. La lectura de diferencias confirma intactos Program/Core/Mongo/Redis/Configuration/fixture, spec/plan, csproj y lockfiles respecto a HEAD. En este cierre solo se escriben `task.md` y `memoria.md`, sin modificar spec/plan, constituciones, foundation, AGENTS/README, agentes/skills ni código. Se deja listo para **solicitar** checkpoint posterior, **sin stage/commit/push**; HEAD base conservado y detener sin iniciar otro gate.

## 60. E-JWT — Autorización, historial y validación final

<a id="e-jwt-validacion-final"></a>

### 60.1. Alcance, baseline y entorno

El **11 de noviembre de 2026**, las solicitudes **AUTH — PRE-BUILD JWT 038–042**, **BUILD AGRUPADO JWT 038–042 / CHECKPOINT E-JWT**, **DIAGNÓSTICO DEL CLEANUP IT040 SIN TOCAR PRODUCTO** y **CONTINUACIÓN E-JWT DESDE IT040 VERDE** delimitan y autorizan este incremento. El pre-BUILD aprueba concreciones; el BUILD posterior autoriza sus dos paquetes/versiones, componentes, tests, comandos y registro tras verde. La autorización de diagnóstico amplía expresamente el soporte test-only a `RegistrationTestScope.cs`; la continuación no autoriza cambiar producto ni repetir correcciones sin fallo nuevo demostrado.

Rama **`feature/authentication`**, HEAD base y conservado **`5742c6ea0d24ed44005ac3793104d413b40aa810`**, checkpoint local de BUILD-2/Register. Inicio original limpio; baseline de esta continuación **6 modificados + 11 nuevos / nada staged**. No se interpreta E-JWT como un nuevo commit Git. Entorno .NET 10, runtime previamente observado **10.0.14**, proyectos `net10.0`, Docker Desktop/WSL2 e imágenes/digests existentes; MongoDB/Redis reales locales y autoridad propia en DynamoDB Local. Sin AWS real. Los dos hosts son hosts lógicos/TestServer con rings/clientes propios dentro del proceso de pruebas, no dos máquinas físicas ni un LB publicado.

Secuencia conservada: **038 → 039 → 040 → 041 → 042 → E-JWT → posteriormente 043–046/GATE-F → posteriormente 032 y Login 047–051 → 058**. No se cambia la dependencia **047 → 046**. La emisión de fixtures llama al primitivo directamente sin Login público; no acredita la futura orquestación de credenciales/cuotas/autoridad de emisión de 049.

### 60.2. Historial correctivo conservado

1. El restore inicial locked informó **NU1004** por las dos nuevas referencias y el ProjectReference derivado. `dotnet restore Enver.Backend.slnx --force-evaluate` actualizó solo los cuatro lockfiles autorizados; el segundo `--locked-mode` fue correcto, sin advertencias de grafo. No se sustituyó ninguna versión. No se presenta ese restore como auditoría independiente de dependencias/imágenes.
2. La primera compilación detectó dos errores de nombres exclusivamente en los nuevos tests: tipo logger `Log` con método homónimo y `AuthenticationOptions` ambiguo. Primera corrección de harness: nombres/alias y hosting genérico en lugar de API obsoleta. Se corrigió también la anotación nullable de `MetadataAddress = null!` en el configurador, sin alterar comportamiento. Build siguiente **0 warnings / 0 errores**. No hubo fallo de focal productivo ni relajación de controles.
3. Focales iniciales **UT038 16/16, UT039 51/51, ST039 28/28, UT040 12/12** verdes. Primer **IT040 2/3**, 1 fallida/0 omitidas: `Stopped_real_Redis_is_fail_closed_for_a_cryptographically_valid_identity`, error `SCAN / SocketClosed` en `RegistrationTestScope.DisposeAsync`, línea original 153. Segundo ajuste test-only: espera acotada del cliente anterior antes del cleanup. Build **0/0**; segundo **IT040 2/3**, mismo punto reportado. Se detuvo al agotar las dos correcciones autorizadas, sin ejecutar 041/042 completo/regresión ni escribir evidencias.
4. La solicitud diagnóstica independiente permitió instrumentación mínima saneada y **una corrección estructural**. El diagnóstico reprodujo **1 fallida/1 total**, sin omisión, y separó el resultado funcional del teardown: **200** con el mismo JWT antes del stop; después **503**, **no-store**, Problem Details `temporarily_unavailable`, sin challenge Bearer; todas las aserciones funcionales pasaron. Tras `StartRedisAsync`, la fixture verificó Redis sano: **mismo contenedor, endpoint publicado distinto, cliente anterior desconectado**. La espera terminó con `TaskCanceledException`; después `SCAN` ocultó ese fallo de teardown. **COMPORTAMIENTO PRODUCTIVO: PASÓ; TEARDOWN: FALLÓ. Causa E demostrada: endpoint de cleanup obsoleto**, no "flake" ni fallo de revocación atribuido por intuición. No se traslada esta causa a incidentes históricos de otros incrementos sin prueba.
5. Ownership: la fixture posee contenedor/Stop/Start; el scope posee hosts, namespace de cuotas, base Mongo y autoridad de prueba; el host JWT posee sus clientes. Orden real: infraestructura → scope → host JWT → 200 → Stop Redis → request 503/no-store → Start/verificación Redis → disposición host JWT → cleanup del scope. Por tanto **Redis no seguía detenido al ejecutar SCAN**: el cliente estaba fijado al endpoint anterior. `KeysAsync(config.Keys.Prefix + "*")` busca metadata/buckets de cuotas propios; su error impedía llegar a `KeyDeleteAsync` y `DropDatabaseAsync`, aunque el `finally` disponía hosts/autoridad.
6. Corrección estructural final, solo en `JwtCheckpointTests.cs` y `RegistrationTestScope.cs`: conservar Stop/Start en `try/finally`; después crear **conexión exclusiva de cleanup** con el endpoint actual de la fixture y los mismos timeouts. `DisposeWithRedisCleanupAsync` opt-in comparte el mismo SCAN, borrado, DropDatabase y disposición; `DisposeAsync()` habitual conserva el cliente anterior del scope. El llamador dispone la conexión prestada. No se omite limpieza, captura globalmente RedisConnectionException, modifica fixture, reabre admisión ni recrea clientes de seguridad. **Se retiran instrumentación y bucle de reconnect anteriores; cero sleeps/retries añadidos**.
7. Ya sin diagnóstico: caso aislado **1/1** (13 s), tres repeticiones secuenciales **1/1** cada una (8 s cada una), **IT040 3/3** (12 s), origin quota **1/1** (4 s) y Register/composición **39/39** (35 s), todos con 0 fallidas/omitidas. Son **4/4 ejecuciones aisladas** del caso; el grupo también lo incluye. El grupo de 39 incluye origin; no se suma como caso distinto. No se cambió nuevamente la corrección en esta continuación.

Fuente primaria: salidas reales de comandos y diagnóstico saneado de esta conversación; sin guardar tokens, secretos, endpoints o cuerpos sensibles en este registro. Los intentos rojos son antecedentes reales, no eliminados por los verdes posteriores.

### 60.3. Comandos y resultados de la continuación

Ejecutados secuencialmente, sin corrección técnica ni repetición de suites completas:

```text
dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build --filter "FullyQualifiedName~JwtPolicyTests&Task=041"
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~JwtCheckpointTests&Task=041"
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~JwtCheckpointTests"
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build --filter "FullyQualifiedName~TokenValidationTests"
dotnet restore Enver.Backend.slnx --locked-mode
dotnet build Enver.Backend.slnx --no-restore
dotnet test tests/Enver.API.UnitTests/Enver.API.UnitTests.csproj --no-build
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build
```

| Verificación | Total / passed / failed / skipped | Duración informada |
| --- | --- | --- |
| UT041, rotación con reloj controlado | **1 / 1 / 0 / 0** | 206 ms |
| IT041, JwtBearer real/rotación | **1 / 1 / 0 / 0** | 4 s |
| JwtCheckpointTests completo | **5 / 5 / 0 / 0** | 13 s |
| TokenValidationTests completo | **36 / 36 / 0 / 0** | 4 s |
| Restore locked final | Correcto, proyectos actualizados, sin cambiar lockfiles en esta continuación | No se atribuye duración no informada |
| Build final sin restore | **0 warnings / 0 errores** | **16,58 s** |
| UnitTests completas | **586 / 586 / 0 / 0** | **4 s** |
| IntegrationTests completas | **191 / 191 / 0 / 0** | **8 min 57 s** |
| SecurityTests completas, solo tras IT verdes | **120 / 120 / 0 / 0** | **1 min 11 s** |

IT completas con límite externo **900 s**. Los focales forman parte de las suites, no aumentan sus conteos. Frente al baseline anterior, hay **80 UT + 5 IT + 36 ST** nuevas; las pruebas de configuración incompatible de 038 también soportan 041, sin duplicar su conteo. Esta vez las tres suites completas corresponden al **árbol posterior a la corrección final de cleanup**, no al diagnóstico instrumentado. La clasificación/corrección adicional permitida para una regresión fallida no fue necesaria.

### 60.4. Archivos e integridad

Nuevos: `Enver.API/Features/Authentication/Application/Ports/IAccessTokenIssuer.cs`; en `Enver.API/Security/`, `JwtTokenIssuer.cs`, `JwtKeyRing.cs`, `JwtBearerConfiguration.cs`, `JwtClaimsValidator.cs`, `AuthenticationPolicies.cs`, `RevocationAuthorizationHandler.cs`, `AuthenticationAuthorizationResultHandler.cs`; `tests/Enver.API.UnitTests/JwtPolicyTests.cs`, `tests/Enver.API.IntegrationTests/JwtCheckpointTests.cs`, `tests/Enver.API.SecurityTests/TokenValidationTests.cs`.

Modificados acumulados: `Enver.API/Enver.API.csproj`, `Enver.API/packages.lock.json`, los tres `tests/Enver.API.{UnitTests,IntegrationTests,SecurityTests}/packages.lock.json`, `tests/Enver.API.IntegrationTests/RegistrationTestScope.cs` y, únicamente tras todas las suites verdes, `specs/002-authentication/task.md` y `memoria.md`. **19 archivos acumulados: 8 modificados + 11 nuevos**; el archivo adicional sobre los 18 inicialmente previstos fue autorizado expresamente para el diagnóstico/cleanup.

En esta continuación **solo se editan task/memoria**. Huella SHA-256 agregada de fuentes/configuración/paquetes de `Enver.API` y `tests` (`.cs/.csproj/.json/.lua`, excluyendo bin/obj), idéntica antes y después de las pruebas: **`8466A66EE9006B1BE97706D7E6B4927C3BD0EFB00BEF76A1BFD8D537F941C8A2`**. No se presenta como hash de commit o auditoría de secretos. La lectura de diferencias respecto a HEAD confirma intactos Program, composición productiva, Register/controller/contratos, Core/BSON, Mongo, Redis/protocolo/Lua, Options/Http y `tests/Shared`, spec/plan. Cuotas, capacidad 5000 y timeouts productivos intactos. Claves de fixtures generadas mediante RNG, sin secretos reales hardcoded. Sin stage/commit/push.

## 61. E(T-AUTH-038) — Emisión JWT y key ring

<a id="e-t-auth-038"></a>

**Trazabilidad oficial:** AUTH-016/017/019–021/034; PLD-AUTH-03/04/09/10; AC-06/09/10/20/21; dependencias 008/017/037. 008 conserva su estado de startup productivo pendiente; no se declara completada por esta evidencia. Autorización, entorno, archivos y validación común en §60.

- Dos referencias directas aprobadas: **Microsoft.AspNetCore.Authentication.JwtBearer 10.0.14** y **Microsoft.IdentityModel.JsonWebTokens 8.19.2**; único stack Microsoft, **JsonWebTokenHandler**. Grafo locked: Protocols.OpenIdConnect/Protocols/Tokens/Logging/Abstractions **8.19.2**, System.IdentityModel.Tokens.Jwt **8.19.2** solo transitivo y sin usar su handler legacy; Microsoft.Bcl.Cryptography **10.0.2** transitivo. No nuevas referencias directas en proyectos de pruebas ni alternativa instalada.
- `IAccessTokenIssuer`: **`AccessTokenIssueResult Issue(Guid subject)`**. Resultado tipado `Issued`/`Unavailable`, token y tiempos solo en éxito, `ToString()` solo nombre de tipo; puerto sin HTTP/MVC/Redis/Mongo. No decide credenciales, cuotas o Login.
- `JwtKeyRing` captura opciones en snapshot local inmutable, lookup ordinal exacto por kid, sin trim/fallback/discovery/reload; firmas internas `TryGetSigningCredentials`/`TryGetValidationKey`. Bloquea snapshot inválido con error saneado: HS256/issuer/audience/vida/skew, ActiveKid existente, kids no vacíos/únicos, Base64 canónica ≥32 bytes y no reutilización de HMAC de cuotas. Provisión externa aleatoria por ambiente; la longitud no demuestra entropía. Opciones existentes y sus validadores se conservan, sin activar wiring productivo.
- `JwtTokenIssuer` toma **una lectura UTC** de TimeProvider; **iat=floor Unix seconds**, NumericDate JSON entero; **exp=iat+900**. Firma HS256 con kid activo. Payload emitido **exactamente sub/jti/iss/aud/iat/exp**, issuer `enver-api`, audience `enver-mobile`; sin nbf automático/PII/email/fullName/roles/phone/refresh. No persiste tokens ni JTI, consulta usuarios o almacenes.
- Subject **UUIDv4 D minúsculo, 36 caracteres**, round-trip exacto/variante RFC; JTI **RNG criptográfico 32 bytes**, Base64url canónica sin padding **43 caracteres**, nuevo por emisión. Unicidad probabilística, sin ledger por token ni garantía anti-replay implícita.
- **UT038 16/16** acreditadas: claims exactos, jti distinto, una lectura/floor/exp900, rechazo de opciones/material/kid inválidos, HMAC reutilizada y snapshot independiente de mutaciones; incluidas en **586/586 UT** actuales. Fronteras temporales de 039/041 y ST mínimas respaldan el perfil. El primitivo emite fixtures de test: la autorización de emisión pública con controles/lease020 corresponde al futuro flujo049, no está acreditada aquí.

**T-AUTH-038: COMPLETADA / EVIDENCIA REGISTRADA.**

## 62. E(T-AUTH-039) — Validación JWT estricta

<a id="e-t-auth-039"></a>

**Trazabilidad oficial:** AUTH-017–020; PLD-AUTH-10/13; AC-09/10/18; dependencias 011/038. Archivos: `JwtBearerConfiguration.cs`, `JwtClaimsValidator.cs`, `JwtPolicyTests.cs`, `TokenValidationTests.cs`.

- Configurador preparado: **MapInboundClaims=false, SaveToken=false, IncludeErrorDetails=false, UseSecurityTokenValidators=false, RefreshOnIssuerKeyNotFound=false**; una ruta JsonWebTokenHandler, sin Authority/metadata/configuration manager remotos. Firma obligatoria, HS256 allowlist, issuer/audience exactos, expiración obligatoria, resolución local exacta de kid y `TryAllIssuerSigningKeys=false`; no fallback ni búsqueda remota.
- Compacto ASCII **≤4096 bytes**, sin contar `Bearer `, comprobado antes de decode/criptografía/Redis, tanto en frontera Bearer como validación directa. JSON/UTF-8 estrictos y acotados; se rechazan nombres duplicados incluso equivalentes por escapes, campos adicionales, tipos ambiguos y obligatorios ausentes. Payload allowlist de los seis claims y `nbf` opcional; aud string única exacta, no array. Header solo alg/kid/typ, typ opcional `JWT`; se rechazan none/otro alg, kid ausente/duplicado/tipo incorrecto/desconocido, jku/x5u/crit y otros campos no soportados. UUID/JTI canónicos con round-trip.
- Preparse **solo rechaza**; no autentica ni consulta Redis. Firma y validaciones completas antes de producir `ValidatedJwtIdentity`, vinculada al mismo principal autenticado. El wrapper devuelve fallo genérico sin token/exception original sensible; `IncludeTokenOnFailedValidation=false`, `LogTokenId=false`, `LogValidationExceptions=false`.
- **LifetimeValidator explícito**, TimeProvider UTC sin truncar el instante de comparación: `exp > iat`, `exp-iat == 900`, `iat <= now+S`, **`now < exp+S`**; S entero0–30 s, inicial30. Si nbf existe: entero, `nbf <= now+S` y `nbf <= exp`. `now==exp` pasa con skew30 y falla con skew0; `now==exp+S` siempre falla. Frontera a un tick antes/en/después probada, sin defaults accidentales.
- **UT039 51/51** y **ST039 focal 28/28**, incluidas en regresiones actuales **UT586/ST120**. `TokenValidationTests` completo **36/36** comprueba HTTP401 genérico, challenge seguro, no-store, rechazo antes de trabajo de estado/Redis y ausencia de token/claves/sentinela sensible en body/logs de aplicación/framework capturados. Alcance de los casos probados, no garantía universal de telemetría o vulnerabilidades.

**T-AUTH-039: COMPLETADA / EVIDENCIA REGISTRADA.**

## 63. E(T-AUTH-040) — Políticas, revocación y resultados HTTP

<a id="e-t-auth-040"></a>

**Trazabilidad oficial:** AUTH-003/018/021–023/026/031/032/035; PLD-AUTH-11/14/19; AC-09/11–14/19; dependencias 010/018/020/039. Archivos: `AuthenticationPolicies.cs`, `RevocationAuthorizationHandler.cs`, `AuthenticationAuthorizationResultHandler.cs`; UT/IT/ST JWT y soporte cleanup de §60.2.

- DefaultPolicy y FallbackPolicy exigen esquema Bearer/identidad autenticada y requisito de revocación. El handler exige feature de identidad ya validada del mismo principal, estado verificable y lectura concluyente del store; sin caché negativa ni fallback. **JWT válido por sí solo no concede permisos de negocio**; se conservan los demás requisitos del recurso.
- NotRevoked con controles aptos puede satisfacer el requisito; **Revoked →401** fuera de la excepción. Estado o lectura **Unavailable/Uncertain →503**, **no-store**, código `temporarily_unavailable`; no convierte un fallo operativo de identidad criptográfica válida en401. JWT ausente/inválido o revocado produce401 genérico con challenge `Bearer` seguro, no redirección. El result handler no deja continuar ante requisito no satisfecho.
- Excepción futura: solo política y metadata Logout, **POST exacto `/api/v1/auth/logout`**, JWT auténtico vigente y estado verificable; puede admitir Revoked/NotRevoked para la futura confirmación funcional. No se reutiliza por otra ruta/método/política. **No endpoint, escritura nueva, respuesta204 ni servicio Logout**; pertenecerán a043–046. Las UT construyen contexto/metadata, no publican esa ruta.
- **UT040 12/12**, **IT040 3/3** tras el fix y ahora incluidos en **191/191 IT**. Redis real acredita marca017 escrita solo desde setup, rechazo por dos hosts; Redis detenido da503/no-store y la lectura real ocultada por decorador test-only modela Uncertain→503. No prueba pérdida física de ACK. ST comprueba además autoridad no verificable sin lectura, misuse de excepción, fallback y privacidad. Los dos rojos previos, diagnóstico de endpoint obsoleto y validaciones4/4 del cleanup se conservan en §60.2, sin atribuirlos a producto.

**T-AUTH-040: COMPLETADA / EVIDENCIA REGISTRADA.**

## 64. E(T-AUTH-041) — Rotación manual y retención coordinada

<a id="e-t-auth-041"></a>

**Trazabilidad oficial:** AUTH-019/020/024/034; PLD-AUTH-09/10; AC-10/20; dependencias 017/038. Soporte autorizado: este procedimiento y pruebas `JwtPolicyTests`/`JwtCheckpointTests`; puerta pura `JwtKeyRing.CanRetirePreviousKey`.

**Procedimiento aprobado:** (1) distribuir A+B a todos los validadores; (2) confirmar reconocimiento de B; (3) cambiar ActiveKid a B en emisores; (4) acreditar cese de toda emisión A, incluidas operaciones en vuelo; (5) coexistir validando A+B; (6) retirar A únicamente después del límite seguro. Si no puede establecerse la última emisión posible, no se autoriza retirar. Compromiso requiere retiro extraordinario/reautenticación, no un Logout global nuevo.

**Retención mínima:** última emisión posible A **+900 s de vida +30 s de skew máximo =930 s**, desde el instante conservador de cese real, nunca desde el primer aviso. No se agrega el margen Redis candidato/configurado de60 s ni se transforma en990. Las revocaciones conservan por separado `exp+skew+margen operativo` y sus Options/Lua/TTL intactos. Vida o skew incompatibles bloquean el snapshot; todo cambio futuro que deje aceptación sin retención suficiente exige revisión/aprobación coordinada.

- **UT041 1/1**, 206 ms: A activa; A+B reconocidas; B activa emite token validable por el ring distribuido; A sigue válida durante coexistencia. A **930 s menos un tick** la puerta de retiro devuelve false y A aún valida; **a930 s** devuelve true y A ya está expirada. Tras retirar A, token A rechazado y nueva emisión B válida. Sin sleeps reales930 s.
- **IT041 1/1**, 4 s: mismo flujo con **JwtBearer real**, hosts lógicos/Redis real y TimeProvider controlado; 401/no-store al borde/tras retiro, B nueva aceptada. Incluida en JwtCheckpointTests5/5 y regresión191/191.
- ActiveKid inexistente, vida901 y skew fuera0–30/null se rechazan en UT038 (reutilizadas y verificadas dentro de586/586 UT), no se contabilizan como nuevos casos041.
- La puerta de retiro calcula el horizonte con instantes UTC confiables suministrados; **no acredita por sí sola el cese global ni impide que un operador elimine una clave prematuramente fuera del procedimiento**. No hot reload/rotación runtime, temporizador, periodicidad inventada, AWS manager ni coordinación productiva de despliegue acreditada. Todo poseedor HS256 puede emitir; riesgo conservado.

**T-AUTH-041: COMPLETADA / EVIDENCIA REGISTRADA.**

## 65. E(T-AUTH-042) — Checkpoint integrado JWT sin Login público

<a id="e-t-auth-042"></a>

**Trazabilidad oficial:** AUTH-016–024/026/035/041–043; PLD-AUTH-10/11/20; AC-06/09–14/20; dependencias 015/038–041. Pruebas: `JwtCheckpointTests.cs`, `JwtPolicyTests.cs`, `TokenValidationTests.cs`; comandos/resultados comunes en §60.3.

- **JwtCheckpointTests completo5/5**: emisor real y JwtBearer real, dos hosts con rings/RedisConnections propios y configuración compartida; mismo sub, jti distinto por emisión y validación cruzada. Rotación A→B/coexistencia/retiro con reloj controlado. Marca de revocación por store017 existente desde setup, leída por ambos hosts y rechazada401; Redis real detenido→503/no-store; incertidumbre después de lectura real modelada conservadoramente→503. No nueva escritura funcional Logout.
- **TokenValidationTests completo36/36**: malformed,4097, firma alterada/tampering, wrong issuer/audience, expired/borde exp+skew, future iat/nbf, missing/unknown/duplicate kid, none/otro alg, claims ausentes/duplicados/equivalentes por escapes/ambiguos/tipos incorrectos, sub/JTI inválidos, jku/x5u/crit, UTF-8 inválido y PII adicional. Positivo del emisor con payload mínimo; revoked401; estado/revocación no verificables503; abuso de excepción rechazado; default/fallback, no-store/challenge seguro y logs capturados sin token/key/sentinela password/hash. Fixtures adversariales usan HMAC BCL solo en tests para preservar JSON malicioso, no un segundo stack JWT productivo.
- UT038/039/040/041 suman **80 casos** incluidos en586/586: canonicalidad/aleatoriedad/configuración/key ring, precisión/tiempos y reglas estrictas, políticas/aislamiento y rotación. No se finge que cada vector sea una IT separada ni que todos impliquen Redis real.
- El test integrado verifica Register **201** mediante el host original/Mongo real y ausencia de rutas Login/Logout; regresión191/191 conserva además Register32/composición7 y la suite completa previa. Superficies JWT `/_jwt`, `/_fallback` y `/_misused` existen **solo en hosts de test**; no se publican Login/Logout para generar fixtures.
- `Program.cs` y composición productiva **intactos**, sin AddAuthentication/JwtBearer o UseAuthentication productivos nuevos. Los hosts de test registran componentes y pipeline real únicamente para verificar042. La selección por endpoint, Bearer opcional anónimo y wiring/controllers/pipeline completo siguen en058; no se acredita seguridad JWT del host productivo por estos tests.

Regresión íntegra del árbol final: restore locked correcto, build **0/0**, **UT 586/586, IT 191/191, ST 120/120**, todas con **0 fallidas / 0 omitidas**. No infraestructura cloud, readiness externa, replay prevention, failover productivo ni eliminación universal de pérdida Redis.

**T-AUTH-042: COMPLETADA / EVIDENCIA REGISTRADA.**

## 66. Revisión y cierre del checkpoint E-JWT

<a id="cierre-e-jwt"></a>

Acto autorizado por **CONTINUACIÓN E-JWT DESDE IT040 VERDE**, realizado únicamente después de focales y las tres suites completas verdes del árbol post-fix. Se revisan separadamente:

| Evidencia | Capacidad acreditada |
| --- | --- |
| E038 §61 | Stack/versiones aprobados, emisión mínima, snapshot de claves, UUID/JTI, tiempo y rechazo de claves/configuración inválida. |
| E039 §62 | Firma/HS256/kid/issuer/audience, perfil estricto/tamaño/duplicados/tiempo exacto, rechazo antes de identidad confiable. |
| E040 §63 | Default/fallback, revocado401, no verificable503/no-store, excepción Logout aislada, Redis real y cleanup corregido sin ocultar errores. |
| E041 §64 | Procedimiento manual, distribución/activación/coexistencia/retiro y retención930, reloj controlado/configuración incompatible. |
| E042 §65 | Emisión/validación real cruzada, dos hosts lógicos, revocación/fail-closed, seguridad y Register sin nuevas rutas productivas. |

**T-AUTH-038, T-AUTH-039, T-AUTH-040, T-AUTH-041 y T-AUTH-042: COMPLETADAS / EVIDENCIA REGISTRADA. CHECKPOINT E-JWT: SATISFECHO / CERRADO — LOCAL/TEST REPRESENTATIVO.** Esto satisface el prerrequisito de capacidad para solicitar043–046, **no autoriza su ejecución automática**.

**GATE-E completo: PENDIENTE / NO SATISFECHO. GATE-F: PENDIENTE / NO SATISFECHO.** GATE-A/B/C/D conservan cierres, G/H pendientes. 032 (incluida su brecha de hashing antiguo/CAS), Login047–051, Logout043–046 y058 integral **no implementados por este incremento**; 008 conserva verificación productiva de startup pendiente. **AUTH-027 NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**, sin confundirlo con T-AUTH-027 Mongo ya completada. No Orders, refresh tokens, AWS/Terraform, CI/CD ni siguiente incremento.

No acredita dos instancias físicas, Load Balancer, TLS público, Atlas HA, Redis administrado, failover/continuidad productivos o titularidad de correo. Los estados anteriores de ausencia JWT en §59 y entregas previas quedan como historial de aquellas autorizaciones, no como estado posterior de este cierre. Solo se actualizan task/memoria tras verde; spec/plan y dependencias intactos, sin stage/commit/push. Detener aquí, sin checkpoint Git nuevo ni trabajo adicional.

## 67. Logout — Autorización, incidentes y validación final

<a id="logout-validacion-final"></a>

### 67.1. Autorización, baseline y fuentes

El **11 de noviembre de 2026**, el responsable autoriza sucesivamente la validación del correctivo Redis logical clock, la terminación individual de capacity R9–R10, **AUTH GATE-F — REGRESIÓN COMPLETA FINAL Y CIERRE**, diagnóstico focal Register Uncertain y **AUTH GATE-F — REINTENTO DE IT COMPLETAS TRAS DIAGNÓSTICO REGISTER UNCERTAIN**. La última autorización conserva UT completas previas, exige build/IT/ST y solo con IT+ST verdes permite consolidar GATE-B, registrar E043–E046 y revisar/cerrar F. Permite cambios de tests únicamente ante una nueva causa demostrada; prohíbe iniciar 032/047–051/058, stage/commit/push.

Baseline comprobada: SDK **10.0.403**, rama **`feature/authentication`**, HEAD **`f2335b0f95bed02edfd2360939fc66954c1a9497`**, **19 modificados + 7 nuevos / 0 staged**, diff tracked **605 inserciones / 47 eliminaciones**. El árbol conserva las implementaciones/focales autorizadas de Logout y el único correctivo productivo Redis; no se rehace ese trabajo. Fuente primaria de este cierre: salidas reales de sesión y TRX; los antecedentes no presentes como capturas en esta sesión se atribuyen expresamente al informe del responsable, no a nuevas ejecuciones.

### 67.2. Incidentes históricos — no son el estado final

1. **CS0104 en UT043 y alias test-only:** incidente previo comunicado por el responsable; el archivo actual usa `ApplicationAuthenticationService` para desambiguar. No se modifica producción por ese error de nombres.
2. **Dos fallos de setup IT043:** antecedentes informados por el responsable; no se inventan comparaciones o valores no disponibles. Se conservan separados de los resultados funcionales actuales.
3. **WWW-Authenticate faltante:** el diagnóstico previo informado identificó selección de `DefaultApiProblemDetailsWriter` en MVC, que podía omitir la ruta del writer propio. El fix central preparado en `ApiProblemDetails` aplica el challenge antes de delegar y también en su writer: asignación única `Bearer`, sin append ni parámetros privados. No se altera la validación JWT para forzar el header.
4. **Logout404 histórico:** las expectativas de ausencia del endpoint de incrementos previos se sustituyeron por el contrato actual de Logout401/Bearer sin autenticación. Composition conserva Register/Login y controles independientes; no afirma integración 058 terminada.
5. **Origin20/900:** historial de setup de §54 y memoria §50; conectar/PING antes del lifecycle fue preparación test-only, no consumo ni retry funcional. Cuotas y dependencia Redis de 250 ms permanecen intactas.
6. **Mongo025 y metadata transitoria:** la cancelación local no demuestra desaparición instantánea de un build remoto ni un índice utilizable. El test preexistente acredita `Incompatible` con rechazo concluyente 11000 o `Unavailable` exclusivamente con cancelación del presupuesto observada; nunca Ready retroactivo. La preservación estable exige ausencia de índice compatible/build activo y originales íntegros. Polling test-only condicionado: máximo 2 s, condición quieta de al menos 100 ms y muestras de 25 ms; la carrera tiene observación separada acotada a 5 s. No es sleep productivo, retry del initializer o ampliación de su dependencia de 500 ms. La segunda inicialización del caso de duplicados es verificación independiente explícita, no recuperación automática.
7. **Redis capacity `Unavailable/state`:** incidente real, captura de retroceso **−7887 µs**, condición anterior `now < meta.last`, fix y pruebas conservados en §68. No se atribuye esta causa retrospectivamente a todos los fallos Redis históricos sin captura.
8. **Capacity agrupada:** R1–R8 terminaron verdes; el límite externo de 900 s interrumpió R9 sin TRX, sin resultado clasificable Redis. R9 y R10 se ejecutaron después individualmente y pasaron. El intento interrumpido no cuenta entre las diez rondas completas ni altera RequestBudget/timeouts del producto.
9. **Primera regresión final:** restore locked correcto, cuatro lockfiles intactos, build 0/0 en 23,17 s, UT **633/633** en 5 s; IT **258/259**, una fallida/0 omitidas, 10 min 16 s. Falló `Persistence_outcomes_map_to_503_without_retry_or_compensating_delete(Uncertain,1)` con `EqualException` saneada. Se detuvo sin ST ni documentación de cierre. El wrapper impidió identificar la comparación exacta; la causa histórica sigue **NO DETERMINADA**.
10. **Diagnóstico Register Uncertain:** solo se seleccionó ese caso mediante `FullyQualifiedName` y `DisplayName~Uncertain`. Primer intento bloqueado antes del cuerpo por startup Mongo (exit code 48/ADDRESS_IN_USE; Docker indicó contenedor no ejecutándose), incidencia separada, sin atribución al fallo histórico. Siguiente ejecución instrumentada **1/1**, 0 fallidas/omitidas: HTTP503, `temporarily_unavailable`, no-store, documentos 0→1, una llamada al puerto y una inserción interna; inner Confirmed → probe Uncertain. Documento coincidente con el ID de esa única solicitud. Retry aplicativo=0; ausencia de delete acreditada por puerto/call-sites, no captura wire. Todas las aserciones etiquetadas pasaron; no se demostró expectativa obsoleta ni defecto productivo. Observaciones pueden alterar tiempos: el verde instrumentado no explica retrospectivamente el rojo. Instrumentación retirada, 206/206 hashes restaurados y build IT 0/0 en 13,47 s; no fix permanente en Register/Scope.
11. **Primera ST de este reintento:** **145/146**, una fallida/0 omitidas, 1 min 51 s. `RegistrationSecurityTests.Structured_events_only_accept_closed_enums_and_emit_no_request_labels` recorría el producto cartesiano de enums, incluyendo Logout/Admitted, y exigía el formato de Register. Logout admite solo Confirmed/Rejected/RateLimited/Unavailable/Uncertain y emite esquema propio; el rechazo `ArgumentOutOfRangeException(outcome)` era correcto. Causa test-only demostrada por código, enum/diff y stack, distinta de Register Uncertain. Bajo la excepción autorizada se modifica **solo `tests/Enver.API.SecurityTests/RegistrationSecurityTests.cs`**: conserva todas las combinaciones de Register, valida sus claves/tipos y enums inválidos; añade cinco casos de etiquetas Logout exactas y uno de rechazo de otros resultados/enums sin emitir eventos. Mantiene formato/Operation/Outcome/EventId/mensaje cerrados y ausencia de campos de solicitud. No se relaja producto ni se elimina cobertura. Build ST 0/0 en 17,44 s; focal posterior **11/11** y ST completas posteriores **152/152**. Se conservan ambos resultados, sin presentar 145/146 como verde.

### 67.3. Resultado final y representatividad

| Validación | Total / passed / failed / skipped | Duración informada |
| --- | --- | --- |
| SDK seleccionado | **10.0.403** | No aplica. |
| Restore locked previo del mismo árbol de paquetes | Correcto, cuatro lockfiles intactos; no repetido en el reintento. | No se inventa duración. |
| Build de solución `--no-restore` del reintento | **0 warnings / 0 errores** | **19,37 s**. |
| UT completas previas, conservadas y no repetidas | **633 / 633 / 0 / 0**, incluidos 37 casos UT Logout y 10 logical clock. | **5 s**. |
| IT completas de este reintento, sin filtro/StopOnFail | **259 / 259 / 0 / 0** | **10 min 30 s**; TRX start→finish **636,779 s**. |
| Build del único fix ST test-only | **0 warnings / 0 errores** | **17,44 s**. |
| Focal `RegistrationSecurityTests` post-fix | **11 / 11 / 0 / 0** | **172 ms**. |
| ST completas post-fix, sin filtro | **152 / 152 / 0 / 0** | **1 min 44 s**; TRX start→finish **113,546 s**. |

Orden real: build solución → IT completas verdes → ST completas rojas → causa nueva demostrada/fix exclusivamente ST → build ST → focal ST → ST completas verdes. **No se repiten IT, UT completas ni rondas focales de capacidad en este reintento**. IT y UT corresponden al mismo producto/UT/IT final; el único cambio posterior es el archivo ST, cuya suite final se ejecuta completa. No se presenta todo como una única corrida íntegra verde inicial. xUnit2031 se conserva como warning histórico no corregido; estos builds incrementales informaron 0 warnings.

| Caso/grupo vigilado en IT completas | Resultado |
| --- | --- |
| Persistence `Unavailable/0` | **Passed**, 1,383684 s. |
| Persistence `Uncertain/1` | **Passed**, 1,292674 s; no reaparece la EqualException histórica. |
| `RedisAuthenticationQuotaCapacityTests` | **2/2**; Exactly_2500... **Passed**, 11,745811 s. |
| `MongoUsersInitializerIntegrationTests` | **11/11**; Existing_duplicate_data... **Passed**, 0,907639 s. Resultado original Incompatible, dependencia no cancelada; segunda verificación Incompatible/11000, originales preservados. |
| Origin20/900 | **Passed**, 4,724051 s. |
| Register201/proyección/documento/hash/sin auto-login | **Passed**, 1,430343 s. |
| `AuthenticationDependencyCompositionTests` | **7/7**, incluye Logout401/Bearer corregido. |
| `LogoutTests` | **42/42**. |
| `RedisAuthenticationQuotaClockTests` | **26/26**. |
| `LogoutSecurityTests` en ST finales | **26/26**. |

No se observa `unavailable/state` **inesperado**. Los casos de corrupción estructural que esperan ese resultado siguen siendo positivos de fail-closed; no se confunden con regresión del reloj. Entorno local Windows/.NET10, runner xUnit/VSTest e imágenes/digests/fixture existentes; MongoDB y Redis reales y autoridad DynamoDB Local. Dos hosts son lógicos de TestServer con recursos locales independientes, no máquinas/cloud/LB.

### 67.4. Comandos, capturas e integridad

Comandos ejecutados (cada suite como proceso independiente; opciones TRX no alteran pruebas):

```text
dotnet --version
dotnet build Enver.Backend.slnx --no-restore
dotnet test tests/Enver.API.IntegrationTests/Enver.API.IntegrationTests.csproj --no-build --logger "trx;LogFileName=it-complete.trx" --results-directory "<directorio externo de esta ejecución>"
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build --logger "trx;LogFileName=st-complete.trx" --results-directory "<directorio externo de esta ejecución>"
dotnet build tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-restore
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build --filter "FullyQualifiedName~RegistrationSecurityTests" --logger "trx;LogFileName=st-registration-focal-post-fix.trx" --results-directory "<directorio externo de esta ejecución>"
dotnet test tests/Enver.API.SecurityTests/Enver.API.SecurityTests.csproj --no-build --logger "trx;LogFileName=st-complete-post-test-fix.trx" --results-directory "<directorio externo de esta ejecución>"
```

Directorio externo real: `%LOCALAPPDATA%\Temp\opencode\gate-f-it-retry-20261111-153827`, con logs, cuatro TRX y manifiestos `source-before.sha256`/`source-before-docs.sha256`. Fuentes previas retenidas: `gate-f-final-20261111-150706` (UT633/IT258), `register-uncertain-diagnostic-20261111-153017` (setup rojo y caso instrumentado verde) y `redis-clock-validation-20261111-141608` (focales y diez rondas). Son capturas locales, no artefactos de CI ni garantía de retención externa.

Antes de documentar se comparan **206 archivos fuente/configuración/documentación no ignorados** contra baseline: solo cambia el archivo ST autorizado. Producto, Register, JWT038–042, Mongo/BSON, `Program.cs`, protocolo/store Redis, Lua logical clock ya preparado, capacidad5000, timeouts, RequestBudget, connection policy, global.json/rollForward, csproj y cuatro lockfiles permanecen intactos durante el reintento; **0 paquetes nuevos**. El diff Redis contra HEAD se limita al correctivo autorizado de §68; no se modifica otra vez. `tests/Shared` y demás cambios de Logout son trabajo previo conservado, no nuevos cambios de este reintento. Después del verde solo se actualizan este documento y `memoria.md`, además del único fix ST ya validado. No stage/commit/push.

## 68. GATE-B — Evidencia correctiva consolidada de Redis logical clock

<a id="gate-b-correctivo-logical-clock"></a>

**GATE-B: CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA.** Adenda al cierre histórico de §38, sin reabrirlo ni rehacer su evaluación desde cero. El responsable acreditó previamente captura real Lua `['enver.auth.quota','1','unavailable','state','0','0']`, retroceso **−7887 microsegundos** y rama anterior `now < meta.last`. Redis TIME es una **fuente temporal distribuida de pared, no estrictamente monotónica**. No se inventan valores absolutos de la captura que no están disponibles en esta sesión.

Único fix productivo Redis preparado: `Enver.API/Infrastructure/Redis/Scripts/ConsumeAuthenticationQuota.lua`. Tras validar Redis TIME y metadata: **effectiveNow = max(redisNow, meta.last)**; sin metadata, **effectiveNow = redisNow**. El Lua conserva nombre local `now` para ese instante efectivo, un TIME por evaluación y protocolo de seis campos/versionado existente. Valida rango exacto/overflow y corrupción antes de mutar. Eventos, actividad, horizontes y expiraciones usan effectiveNow; Retry-After convierte el horizonte lógico a espera de pared con `relevantScore + window - redisNow`, incluyendo el rollback pendiente, sin intermedio fuera de rango. No cambia cuota, capacity5000, timeout, presupuesto, conexión, store o protocolo C#.

| Evidencia focal ya ejecutada | Passed / failed / skipped |
| --- | --- |
| UT Redis | **90 / 0 / 0**, incluye 10 logical clock. |
| IT Redis | **51 / 0 / 0**. |
| IT rollback determinista | **26 / 0 / 0**. |
| IT distribuida GATE-B | **19 / 0 / 0**. |
| ST focal Redis/distribuida GATE-B | **46 / 0 / 0**. |
| Register quota focal | **6 / 0 / 0**. |
| Logout quota focal | **8 / 0 / 0**. |
| Capacidad R1–R10 completas | **10/10 rondas**, dos pruebas por ronda, **20 / 0 / 0**. |

R9 individual: runner **36 s**, TRX **106,610 s**; R10 individual: runner **29 s**, TRX **94,368 s**. No se repitieron R1–R8 para completarlas. `capacity-09-continuation.trx` y `capacity-10.trx` mantienen separado el intento R9 interrumpido. Las dos capacity incluidas ahora en IT259 son regresión ordinaria, no otra ronda focal ni aumento artificial del conteo 10/10.

Propiedades acreditadas dentro del modelo ensayado: **meta.last no decrece**, horizon no se reduce **por rollback**, TTL no se acorta, cuota no se abre por retroceso, Retry-After conservador, rechazo conjunto sin mutación, capacidad5000 intacta y corrupción estructural fail-closed. Las IT deterministas concatenan el cuerpo Lua productivo con un shim test-only de TIME y traslación de época de expiraciones, sin reemplazar condiciones del cuerpo ni cambiar el reloj del servidor; incluyen −1 µs, −7887 µs, retrocesos grandes, concurrencia/capacidad y 16 corrupciones. No prueban todas las patologías temporales ni disponibilidad/continuidad productiva. No se añaden retries/sleeps productivos ni se cambian infraestructura o garantías residuales de AUTH-027.

La regresión final de §67 conserva **26/26 clock y 2/2 capacity**, sin `state` inesperado; las ST finales son verdes. Los incidentes Redis anteriores sin raw response permanecen con su clasificación histórica, sin extrapolarles esta causa demostrada.

## 69. E(T-AUTH-043) — Logout confirmado e idempotente

<a id="e-t-auth-043"></a>

**Estado: COMPLETADA / EVIDENCIA REGISTRADA — LOCAL/TEST.** Trazabilidad: AUTH-018/022–024/026/038/039; PLD-AUTH-02/08/09/11; AC-11–14; dependencias017/020/040, conservadas.

- Archivos del incremento previo: `AuthenticationInputs.cs`, `AuthenticationResults.cs`, `AuthenticationService.cs`, `Ports/ILogoutControls.cs` y `Composition/LogoutControls.cs`; pruebas `tests/Enver.API.UnitTests/LogoutTests.cs`, `tests/Enver.API.IntegrationTests/LogoutTests.cs` y soporte de `tests/Shared/AuthenticationApiFactory.cs`.
- `LogoutInput` solo desde principal/feature JWT autenticados y coincidentes; sub/jti canónicos, issuer único y **exp autenticado único entero UTC**, nunca binding de cuerpo/query. No JWT completo en resultado/ToString.
- `LogoutAsync`: admisión confiable → cuota conjunta → cuerpo vacío → **una escritura de revocación por intento**, con el mismo RequestBudget, exp/skew30/margen60 efectivos; **Confirmed es el único éxito** y requiere presupuesto vigente. NoOperation/Unavailable/Uncertain →Unavailable/HTTP503; nunca success supuesto. Sin consulta MongoDB, lectura de confirmación, read-back, retry o compensación.
- Repeat y concurrencia requieren confirmación propia, preservan valor Redis **"1"**, TTL positivo y expiración absoluta no decreciente hasta exp+skew+margen. Cancelación previa/presupuesto agotado no despachan; cancelación posterior al ACK no se convierte en éxito. JWT falso/expirado/ausente se rechaza antes de escribir.
- Evidencia final compartida §67: **10 UT de trait043 y 6 IT de trait043** incluidas en UT633/IT259; primera/repetida/concurrente, no-confirmación, presupuesto y Mongo inaccesible. E046 añade rechazo cross-host. La respuesta perdida se modela ocultando una confirmación real en probe y exigiendo otro intento independiente; no se prueba pérdida física TCP/ACK ni exactly-once.

## 70. E(T-AUTH-044) — Endpoint, política y challenge seguro

<a id="e-t-auth-044"></a>

**Estado: COMPLETADA / EVIDENCIA REGISTRADA — LOCAL/TEST.** Trazabilidad: AUTH-003/023/031/032/035; PLD-AUTH-11/13/14/19; AC-12/13/19; dependencias011/040/043.

- `AuthenticationController.Logout`, convención/metadata Logout y puerto/input internos. **POST /api/v1/auth/logout**, política exacta **Authentication.Logout**, token exclusivamente desde **Authorization: Bearer único**. El JWT de fixtures usa emisor/validador reales de E038–E042.
- **204 vacío/no-store**, sin content-type, token/cookie/auto-login y solo Confirmed. Cuerpo vacío permitido incluso con content-type; cualquier cuerpo no vacío, incluido whitespace/JSON/bytes inválidos, **400 validation_failed** tras cuota; **>16384 bytes →413 payload_too_large** antes de identidad/cuota. No existe LogoutRequest funcional ni coerción del cuerpo.
- Missing/malformed/firma/claims/vigencia inválidos →**401 authentication_failed**, header **WWW-Authenticate exactamente Bearer**, sin error_description/error_uri/detalles internos y con Problem Details seguro/correlación/no-store. Lectura/admisión no verificable →**503 temporarily_unavailable**, no challenge401.
- Excepción de revocado aún vigente solo para metadata+política+método+ruta Logout exactos. GET, ruta copiada o recurso protegido conservan401; token expirado nunca obtiene excepción. No se relaja política default/fallback.
- Fix central `Enver.API/Http/ApiProblemDetails.cs` aplica challenge idempotente antes de delegar al servicio y en writer, saneando duplicados/parámetros previos. Revalida 400/409/413/415/429/500/503 sin Bearer y evita doble escritura. No cambia algoritmo/claims/tiempo JWT ni guarda token.
- Evidencia: **8 UT trait044**, **13 IT trait044**, más **15 UT y 5 IT de regresión Challenge401** en las suites de §67; E046 aporta ST adversariales. La integración JWT completa se compone en hosts de test; **058/pipeline productivo final sigue pendiente**. La ruta/controller implementados no acreditan publicación cloud.

## 71. E(T-AUTH-045) — Cuotas conjuntas y eventos allowlist

<a id="e-t-auth-045"></a>

**Estado: COMPLETADA / EVIDENCIA REGISTRADA — LOCAL/TEST.** Trazabilidad: AUTH-028–030/032/036/037; PLD-AUTH-15/16/19/23; AC-16/19; dependencias021/036/044/053.

- `LogoutControls` consume el store real de cuotas con HMAC/identidades tipadas desde **cuenta previamente autenticada y origen confiable**. Descriptor existente: **account 30/60 s**, **origin 300/60 s**. Una evaluación conjunta y un cargo por scope; cambiar jti no crea otra cuenta, un sub de cuerpo no es fuente de identidad.
- Intento31/cuenta e intento301/origen →**429 rate_limited**, Retry-After entero positivo y conservador (máximo entre ámbitos, no promesa de éxito). Rechazo no agrega cargo parcial ni crea bucket de cuenta nueva; snapshots y expiraciones conservados. Unavailable/Uncertain →**503**, sin revocación ni devolución ficticia del consumo ocultado.
- Fallos posteriores de cuerpo/revocación conservan el único cargo ya realizado. No rollback, refund, retry o cuotas locales alternativas. `NoOperation` posterior no produce204.
- `AuthenticationEvents`: allowlist Logout **confirmed/rejected/rate_limited/dependency_unavailable/uncertain**, EventId2045/Logout, campos Operation/Outcome/{OriginalFormat}, sin JWT/sub/jti/IP raw, clave Redis, identidades, excepción o cuerpo. Captura de seguridad comprueba ausencia de sentinelas/secretos dentro de los canales ensayados; no certifica todos los proveedores productivos.
- Evidencia: **4 UT trait045 y 8 IT trait045**, incluidos en §67, más abuso/logs de ST Logout. Único fix nuevo del cierre: RegistrationSecurityTests distingue esquema Register y Logout sin reducir cobertura; **11/11 focal y 152/152 ST finales**, incluidos seis casos nuevos de contrato/rechazo de eventos. El producto de eventos no se modifica para aceptar combinaciones inválidas.

## 72. E(T-AUTH-046) — Logout distribuido, fallos y seguridad

<a id="e-t-auth-046"></a>

**Estado: COMPLETADA / EVIDENCIA REGISTRADA — LOCAL/TEST.** Trazabilidad: AUTH-018/021–024/026/035/038/039/042/043; PLD-AUTH-08/09/11/20/24; AC-11–14/17; dependencias015/042–045.

- Dos hosts lógicos con DI/conexiones/stores locales independientes y Redis/autoridad externa de test compartidos. JWT A y B del mismo subject con jti distintos: A válido en host2 antes; Logout A en host1 →204; A rechazado401 en host2, B sigue válido y sin marca.
- Repeat desde host2 y dos logout concurrentes →confirmaciones propias, valor Redis **"1"**, cuatro escrituras totales y expiración absoluta no decreciente. Excepción de política no reutilizable en otras rutas/métodos; no afecta B.
- Escritura NoOperation/Unavailable/Uncertain →503, jamás204 supuesto; lectura no verificable→503. Probe Uncertain oculta confirmación real, no pérdida física de red; nuevo intento independiente puede confirmar sin retry aplicativo automático ni read-back.
- Autoridad closed/missing/unavailable/uncertain mantiene503 y latch cerrado aun con PING. Parada real Redis bloquea logout; Start en finally y endpoint actual solo para cleanup, sin reabrir estado de seguridad. Mongo inaccesible no impide identidad/política/cuota/revocación confirmada cuando sus controles están aptos.
- **10 IT trait046**, **26 ST LogoutSecurityTests**, incluidas en **42/42 IT Logout** y suites finales de §67. ST cubren firma/alg/iss/aud/kid/claims/duplicados/tamaño/vigencia, Authorization ambiguo y fuentes falsas query/cookie/body/header, cuerpos adversariales, >16KiB, policy isolation, no-store, challenge mínimo, abuso de cuenta y logs seguros.
- Límites: dos hosts **lógicos**, no físicos; Redis/Mongo/DynamoDB Local, no AWS real. No acredita TLS público, Atlas HA, failover/continuidad productiva, detector universal de pérdida silenciosa, IAM/ACL cloud o dimensionamiento empresarial. Recuperación integral/aceptación externa y AUTH-027 permanecen pendientes en su alcance posterior; 046 no cierra GATE-G/H ni la autenticación completa.

## 73. Revisión formal y cierre de GATE-F — Logout

<a id="cierre-gate-f"></a>

Acto posterior explícitamente autorizado, realizado **solo tras IT259/259 y ST152/152 finales verdes**, con UT633/633 previas conservadas. Revisión de las cuatro evidencias, no aceptación por presencia de archivos ni por focales aisladas:

| Criterio | Evidencia revisada | Dictamen local/test |
| --- | --- | --- |
| Entrada GATE-B y checkpoint E-JWT | §38/§68, E038–E042/§66 | Satisfecho; E completo no es prerrequisito circular de F. |
| 204 exclusivamente Confirmed, sin read-back/retry | E043/E044, pruebas de no-confirmación/presupuesto | Satisfecho. |
| Repetición/concurrencia y TTL no decreciente | E043/E046, Redis real/value1/PEXPIRETIME | Satisfecho. |
| JWT inválido401 + Bearer único; dependencia incierta503 | E044/E046 y fix central de challenge | Satisfecho. |
| A revocado cross-host, B intacto y excepción exclusiva | E044/E046, hosts independientes/política adversarial | Satisfecho. |
| Mongo no requerido para Logout | E043/E046, índice no Ready/Mongo inaccesible | Satisfecho. |
| account30/60, origin300/60, joint/no partial/Retry-After | E045, ocho IT focales incluidas en IT259 | Satisfecho. |
| Eventos/logs seguros y esquema cerrado | E045/E046, ST Logout y fix test-only demostrado | Satisfecho en canales/perfil ensayados. |
| Redis logical clock y capacidad | §68, 26/26 clock, 10/10 rondas previas y capacity2/2 en IT | Satisfecho; sin state inesperado. |
| Regresión exigida | §67: UT633 previas, IT259 actuales, ST152 finales; 0 fallidas/omitidas finales | Satisfecho con relación temporal explícita y rojos históricos conservados. |

**T-AUTH-043/044/045/046: COMPLETADAS / EVIDENCIA REGISTRADA. GATE-F: SATISFECHO / CERRADO — LOCAL/TEST REPRESENTATIVO. GATE-B: CERRADO / EVIDENCIA CORRECTIVA CONSOLIDADA.** GATE-A/C/D y checkpoint E-JWT conservan sus cierres. **GATE-E completo PENDIENTE / NO SATISFECHO:** faltan032/047/048/049/050/051/058. GATE-G/H pendientes; 008 conserva su estado documental previo, sin cierre automático.

**Register completo en su aceptación local/test previa; JWT038–042 completo como checkpoint local/test; Logout completo local/test; Login pendiente; Authentication NO TERMINADA.** AUTH-027 sigue **NO IMPLEMENTADO / NO SATISFECHO INTEGRALMENTE**; pérdida catastrófica silenciosa no observable es residual no eliminado. No se certifican despliegue, dos máquinas/LB, TLS público, AWS/Atlas/Redis administrado, HA/failover, titularidad de correo, seguridad universal ni 058 integral.

La preparación para checkpoint Git no es un commit: conservar todo trabajo anterior y el único fix ST adicional; **sin stage/commit/push** y HEAD base intacto. Solo task/memoria se actualizan después del verde. **Detener sin iniciar032/047–051/058, otro bloque o checkpoint Git automático.**
