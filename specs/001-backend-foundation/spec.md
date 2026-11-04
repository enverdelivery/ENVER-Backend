# Especificación 001 — Arquitectura base de ENVER Backend

## 1. Información general, objetivo y alcance

| Elemento | Valor |
| --- | --- |
| Solución | `Enver.Backend` |
| Proyecto | `Enver.API` |
| Base tecnológica | C#, ASP.NET Core 10 y .NET 10 |
| Fase de esta solicitud | Especificación de arquitectura base, sin implementación |
| Versión documental | 0.1.0 |
| Estado del documento | Borrador para revisión y aprobación del responsable del proyecto |
| Constitución de referencia | [Constitución canónica, versión 1.0.0](../../.specify/memory/constitution.md) |
| Referencia raíz | [`constitution.md`](../../constitution.md), sin autoridad normativa independiente |
| Estado técnico | No evaluado mediante ejecución; ningún requisito se declara implementado o verificado |

### Objetivo

Definir **qué debe cumplir** la arquitectura base: responsabilidades, capacidades transversales, atributos de calidad y condiciones de aceptación. Esta especificación no contiene pasos de implementación, una planificación ni funcionalidades de negocio de ENVER.

### Autoridad y autorización delimitada

La constitución canónica sigue siendo la autoridad principal del gobierno documental. `AGENTS.md`, `memoria.md` y `README.md` conservan el registro de la Fase 0 y sus restricciones; no se actualizan con esta intervención. La solicitud explícita del responsable autoriza únicamente crear este archivo de especificación y consultar las fuentes indicadas. No constituye una reforma de la constitución, una aprobación integral del borrador ni autorización para planificar, implementar o ejecutar tareas técnicas.

La restricción constitucional de no crear especificaciones durante la Fase 0 no se interpreta como una autorización automática: este artefacto corresponde a la fase de especificación expresamente solicitada. La sincronización posterior de los documentos de estado queda pendiente de autorización independiente. Si aparece otra contradicción, deberá aclararse antes de la acción afectada.

### Fuentes y convención de requisitos

- **C §n:** apartado de la [constitución canónica](../../.specify/memory/constitution.md), fuente normativa principal. Las referencias a `constitution.md` en esta especificación remiten siempre a esa fuente, no al archivo raíz como constitución alternativa.
- **SOL:** solicitud actual del responsable: contenido y restricciones para esta especificación. Es fuente explícita de las ampliaciones documentales sobre JSON, compresión e infraestructura como código, no permiso de ejecución.
- **GOB:** [`AGENTS.md`](../../AGENTS.md), [`memoria.md`](../../memoria.md) y [`README.md`](../../README.md), consultados íntegramente como contexto subordinado.
- Las definiciones de agentes y skills son pautas auxiliares, no fuentes de requisitos de negocio ni autorizaciones nuevas.

Cada requisito tiene un ID estable `FND-nnn`, descripción, justificación, prioridad, criterio de aceptación, fuente y estado de aprobación. Las prioridades propuestas son **P0** (esencial para seguridad, integridad o gobierno), **P1** (necesario para la arquitectura objetivo) y **P2** (optimización cuya prioridad debe ratificarse). La prioridad no determina un orden de ejecución.

Estados de aprobación:

- **C — Confirmado en origen:** obligación establecida en la constitución o solicitada explícitamente. La formulación y los criterios de este borrador todavía requieren revisión formal; no implica aprobación de detalles pendientes.
- **P — Propuesto, pendiente de aprobación:** alternativa o concreción no decidida por el responsable. No puede tratarse como decisión adoptada.

**Para todos los requisitos:** la comprobación documental futura no equivale a aceptación técnica. Los criterios que requieran medición, pruebas o despliegue se evaluarán únicamente en una fase expresamente autorizada. Aquí solo quedan definidos; no se ejecutan. Un criterio con parámetros pendientes no puede declararse satisfecho hasta aprobarlos y aportar evidencia.

### Estado observado por lectura

- `Enver.Backend.slnx`, líneas 1–3, referencia un único proyecto: `Enver.API/Enver.API.csproj`.
- `Enver.API.csproj`, líneas 1–9, usa el SDK web y `net10.0`; las referencias de las líneas 11–14 son estado existente, no selección definitiva de paquetes complementarios.
- `Program.cs`, líneas 1–23, contiene registro y mapeo inicial de controladores, OpenAPI, redirección HTTPS y autorización. Esto no acredita autenticación JWT, políticas efectivas o funcionalidades implementadas.
- Existe Dockerfile con imágenes .NET 10 y archivos de Visual Studio, configuración y artefactos generados. Su presencia no demuestra compilaciones, pruebas, imágenes construidas o despliegues satisfactorios.
- La inspección no encontró controladores implementados ni especificaciones previas en `specs/`. No se inspeccionaron credenciales ni se ejecutaron comandos.

Se leyeron íntegramente los cinco perfiles en `.opencode/agents/` y las cinco skills en `.opencode/skills/`. Los cuatro revisores son de solo lectura; `documentation-keeper` solicita aprobación para editar exclusivamente los cinco Markdown de gobierno. Ninguno tiene habilitada la carga de skills ni acceso de lectura a esta nueva ruta. Las skills desactivan su anuncio automático mediante `metadata.opencode/autoinvoke: false`. No se invocaron agentes, no se cargaron esas skills ni se cambiaron permisos para elaborar este documento; tampoco se atribuye una revisión a esos perfiles.

### FND-001 — Gobierno de la especificación

- **Descripción:** La arquitectura deberá conservar trazabilidad y autorización explícita entre requisitos, decisiones, cambios y evidencias, sin permitir que este borrador habilite implementación.
- **Justificación:** Evitar saltos de fase y decisiones implícitas contrarios a SDD.
- **Prioridad:** P0.
- **Criterio de aceptación:** La revisión documental identifica fuente y estado para el 100 % de los requisitos; ninguna decisión pendiente figura adoptada y cualquier trabajo posterior tiene alcance y autorización independientes documentados antes de iniciarse.
- **Fuente:** C §§1–3, 9–11; SOL; GOB.
- **Estado de aprobación:** C — Confirmado en origen; borrador y revisión formal pendientes.

## 2. Principios de arquitectura y separación de responsabilidades

### FND-002 — Responsabilidades y SOLID

- **Descripción:** La arquitectura deberá distinguir transporte HTTP, reglas de aplicación o negocio e infraestructura, aplicando SOLID de forma proporcionada y evitando acoplar las reglas a persistencia o caché.
- **Justificación:** Favorecer comprensión, cambios controlados y testabilidad sin complejidad especulativa.
- **Prioridad:** P1.
- **Criterio de aceptación:** Una revisión futura de responsabilidades y dependencias no encuentra reglas dependientes de detalles de HTTP, NoSQL o Redis; toda dependencia entre responsabilidades tiene justificación. La revisión no exige aún nombres de capas, carpetas o proyectos.
- **Fuente:** C §4; SOL.
- **Estado de aprobación:** C — Principio confirmado; estructura definitiva pendiente.

### FND-003 — Límites y contratos internos

- **Descripción:** Los límites entre responsabilidades deberán ser explícitos y permitir sustituir infraestructura sin modificar contratos públicos ni exponer detalles internos o información sensible.
- **Justificación:** Reducir dependencia accidental de herramientas y del motor de datos aún no elegido.
- **Prioridad:** P1.
- **Criterio de aceptación:** La revisión de los contratos futuros identifica responsabilidades, dependencias y datos expuestos; no hay filtración de modelos de persistencia ni secretos a los contratos públicos y las abstracciones tienen una necesidad verificable.
- **Fuente:** C §§4, 5.2; SOL.
- **Estado de aprobación:** C — Principio confirmado; contratos concretos pendientes y no definidos en esta especificación de arquitectura base.

## 3. ASP.NET Core 10, C# y .NET 10

### FND-004 — Plataforma tecnológica

- **Descripción:** La base deberá utilizar C#, ASP.NET Core 10 y .NET 10, con componentes compatibles y versiones complementarias aprobadas antes de incorporarlas.
- **Justificación:** Mantener la plataforma confirmada y prevenir selecciones tácitas de dependencias.
- **Prioridad:** P0.
- **Criterio de aceptación:** En una verificación futura autorizada, los metadatos del proyecto indican `net10.0`, la aplicación se ejecuta con ASP.NET Core 10 y cada dependencia adicional tiene compatibilidad y aprobación registradas. La versión concreta del lenguaje y los parches de plataforma se documentan sin fijarlos aquí.
- **Fuente:** C §§2.1, 2.4; SOL. La lectura del proyecto es solo contexto observado.
- **Estado de aprobación:** C — Plataforma confirmada; parches y versiones complementarias pendientes.

## 4. API REST modular como propuesta

### FND-005 — Estilo REST modular

- **Descripción:** Se propone una API REST modular con semántica HTTP coherente, límites lógicos claros y contratos evolutivos, sin decidir todavía patrón de capas, cantidad de proyectos o mecanismo de exposición HTTP.
- **Justificación:** Evaluar una base comprensible y extensible sin adoptar arquitectura definitiva prematuramente.
- **Prioridad:** P1.
- **Criterio de aceptación:** La revisión arquitectónica documenta la aceptación o rechazo explícitos de la propuesta y sus compromisos; si se aprueba, los futuros contratos respetan métodos, estados HTTP y límites modulares definidos. Este criterio no requiere diseñar recursos de negocio ahora.
- **Fuente:** C §§2.3, 4; SOL.
- **Estado de aprobación:** P — API REST modular propuesta, no adoptada.

## 5. Arquitectura escalable y sin estado

### FND-006 — Ausencia de estado local imprescindible

- **Descripción:** La atención de una solicitud no deberá depender de sesión, archivos o memoria exclusiva de una instancia para conservar estado duradero o aplicar controles globales de seguridad y consistencia.
- **Justificación:** Permitir balanceo, reinicio y sustitución de instancias sin afinidad obligatoria de sesión.
- **Prioridad:** P0.
- **Criterio de aceptación:** En un ensayo futuro autorizado, solicitudes equivalentes pueden alternar entre dos instancias sin perder estado duradero ni alterar permisos; reiniciar una instancia no requiere reconstruir sesión local. Se permite estado efímero local únicamente si su pérdida no afecta esas garantías.
- **Fuente:** C §§2.2, 8; SOL.
- **Estado de aprobación:** C — Capacidad requerida; tratamiento concreto del estado compartido pendiente.

### FND-007 — Escalabilidad horizontal

- **Descripción:** La arquitectura deberá admitir añadir o retirar instancias compatibles sin cambios en los contratos públicos ni dependencia de una instancia concreta, manteniendo límites de capacidad conocidos en las dependencias compartidas.
- **Justificación:** Evitar que el número de procesos determine la corrección o provoque sobrecarga incontrolada.
- **Prioridad:** P1.
- **Criterio de aceptación:** Una evaluación futura compara una y dos instancias bajo el perfil aprobado, registra capacidad y saturación de API, almacenamiento y Redis, y no identifica un bloqueo funcional por afinidad de instancia. No se exige duplicar linealmente el rendimiento.
- **Fuente:** C §§2.2, 8; SOL.
- **Estado de aprobación:** C — Escalabilidad solicitada; capacidad y perfil de carga pendientes.

## 6. Seguridad: HTTPS, JWT, autorización, validación y secretos

### FND-008 — Transporte HTTPS

- **Descripción:** Los entornos publicados deberán proteger el tráfico de clientes mediante HTTPS y definir las fronteras de confianza y protección de los enlaces internos cuando haya balanceador o proxy.
- **Justificación:** Evitar exposición de credenciales y confianza indebida en cabeceras reenviadas.
- **Prioridad:** P0.
- **Criterio de aceptación:** Una revisión y prueba futuras confirman certificado válido, ausencia de contenido protegido servido por HTTP y aceptación de información reenviada solo desde proxies aprobados. El punto de terminación TLS y la protección del tramo interno tienen decisión explícita y justificación de riesgo.
- **Fuente:** C §5.1; SOL.
- **Estado de aprobación:** C — HTTPS confirmado; terminación y política TLS concretas pendientes.

### FND-009 — Autenticación JWT

- **Descripción:** La autenticación de recursos protegidos deberá validar JWT con firma, algoritmos permitidos, emisor, audiencia y vigencia explícitos, con gestión segura de claves.
- **Justificación:** Impedir acceso mediante tokens falsificados o emitidos para otro destinatario.
- **Prioridad:** P0.
- **Criterio de aceptación:** Los casos futuros de token ausente, firma alterada, algoritmo no permitido, emisor o audiencia incorrectos, expiración y uso antes de vigencia son rechazados sin filtrar datos; un token válido solo establece la identidad. La rotación de claves conserva el comportamiento aprobado.
- **Fuente:** C §§2.2, 5.2; SOL.
- **Estado de aprobación:** C — JWT requerido; emisor, emisión, duración, tolerancia temporal y política de rotación pendientes. No se especifican endpoints de autenticación.

### FND-010 — Autorización de mínimo privilegio

- **Descripción:** Todo acceso protegido deberá denegarse por defecto salvo permiso explícito, considerando objeto, propiedad y función cuando correspondan; autenticación y autorización serán condiciones distintas.
- **Justificación:** Un token válido no debe permitir acceder a recursos o campos ajenos a su ámbito.
- **Prioridad:** P0.
- **Criterio de aceptación:** La matriz futura de políticas y casos negativos acredita cero accesos no autorizados; se distinguen falta de autenticación y falta de permisos mediante estados HTTP coherentes y una política de no revelación aprobada. Solo las capacidades públicas expresamente justificadas admiten acceso anónimo.
- **Fuente:** C §§5, 5.2; SOL.
- **Estado de aprobación:** C — Principio confirmado; políticas y ámbitos concretos pendientes de especificaciones posteriores.

### FND-011 — Validación y límites de entrada

- **Descripción:** Toda entrada deberá validarse en su frontera de confianza según formato, tamaño, rango y estructura, sin confiar en el cliente ni en la naturaleza NoSQL de la persistencia.
- **Justificación:** Reducir inyección, asignación indebida de propiedades y consumo abusivo de recursos.
- **Prioridad:** P0.
- **Criterio de aceptación:** Los casos futuros de datos inválidos, cargas sobredimensionadas, profundidad excesiva y propiedades no permitidas se rechazan con respuestas documentadas, sin efectos persistentes ni detalles internos. Los límites numéricos y la política de propiedades desconocidas constan aprobados antes de probarlos.
- **Fuente:** C §§5, 5.2; SOL.
- **Estado de aprobación:** C — Validación confirmada; esquemas y límites específicos pendientes.

### FND-012 — Gestión de secretos

- **Descripción:** Los secretos deberán mantenerse fuera de código, documentación, imágenes y registros, mediante mecanismos externos con mínimo privilegio, separación de ambientes y rotación.
- **Justificación:** Evitar filtraciones y reducir el alcance de una credencial comprometida.
- **Prioridad:** P0.
- **Criterio de aceptación:** La revisión y los controles futuros autorizados no detectan secretos expuestos; la identidad de ejecución solo accede a los secretos de su ambiente y existe evidencia controlada de rotación sin revelar valores. No se considera un identificador de configuración como evidencia de secretos gestionados correctamente.
- **Fuente:** C §5.1; SOL.
- **Estado de aprobación:** C — Obligación confirmada; solución de almacenamiento y rotación pendiente.

### FND-013 — Evaluación OWASP API Security

- **Descripción:** La arquitectura deberá relacionar los riesgos de OWASP API Security con controles, criterios de aceptación y riesgos residuales, incluidos flujos sensibles, consumo de recursos, inventario y comunicación con servicios externos.
- **Justificación:** Evitar que HTTPS y JWT se consideren suficientes para asegurar toda la API.
- **Prioridad:** P0.
- **Criterio de aceptación:** La revisión futura contiene una relación de cada categoría de la edición OWASP adoptada con riesgo aplicable, control y evidencia prevista, o una justificación de no aplicabilidad. Incluye autorización por objeto, propiedad y función, autenticación, configuración, solicitudes a servicios externos y confianza en respuestas de terceros; los riesgos residuales tienen responsable y aceptación explícita.
- **Fuente:** C §5; SOL.
- **Estado de aprobación:** C — Referencia OWASP confirmada; edición y evaluación concreta pendientes.

## 7. Base NoSQL obligatoria y motor pendiente

### FND-014 — Persistencia NoSQL

- **Descripción:** La persistencia principal que requiera la arquitectura deberá utilizar un motor NoSQL aprobado conforme a la rúbrica, sin imponer MongoDB ni otro producto en esta especificación.
- **Justificación:** Cumplir la exigencia docente sin anticipar selección tecnológica o modelado de negocio.
- **Prioridad:** P0.
- **Criterio de aceptación:** Una decisión posterior aprobada identifica motor NoSQL, compatibilidad, capacidades y justificación; las evidencias de integración autorizadas utilizan ese motor. Mientras falte la decisión, no se acredita cumplimiento técnico ni se incorporan paquetes específicos por este requisito.
- **Fuente:** C §§2.1, 2.2, 2.4; SOL.
- **Estado de aprobación:** C — NoSQL obligatorio; motor pendiente.

### FND-015 — Modelo de acceso y garantías de datos

- **Descripción:** Los futuros patrones de acceso, particionamiento si aplica, índices y garantías de consistencia deberán justificarse según capacidades del motor aprobado y necesidades especificadas, sin presuponer transacciones o unicidad globales.
- **Justificación:** Evitar modelados y garantías incompatibles con el almacenamiento elegido.
- **Prioridad:** P1.
- **Criterio de aceptación:** Cada patrón futuro tiene declarados alcance de atomicidad, consistencia, controles de concurrencia y coste de consulta; ninguna garantía depende de una capacidad inexistente del motor. Este documento no define colecciones, entidades ni relaciones de negocio.
- **Fuente:** C §§2.2, 2.4, 4, 8; SOL.
- **Estado de aprobación:** C — Justificación requerida; modelado y garantías concretas pendientes.

## 8. Caché distribuida Redis y coherencia

### FND-016 — Redis compartido

- **Descripción:** La arquitectura deberá disponer de caché distribuida Redis compartida por las instancias, con aislamiento por ambiente y ámbito de acceso, sin utilizar la caché como única copia de datos duraderos.
- **Justificación:** Compartir aceleración de lectura sin perder información ni mezclar contextos.
- **Prioridad:** P1.
- **Criterio de aceptación:** Las pruebas futuras entre dos instancias observan datos de caché compartidos conforme al ámbito aprobado; no hay reutilización entre ambientes o identidades sin permiso y la pérdida de caché no elimina la fuente persistente. La conectividad y las credenciales están protegidas.
- **Fuente:** C §§2.2, 5, 8; SOL.
- **Estado de aprobación:** C — Redis distribuido requerido; servicio, topología, cliente y política de aislamiento pendientes.

### FND-017 — Coherencia e invalidación

- **Descripción:** Cada dato cacheable deberá tener una política explícita de claves, expiración, invalidación y obsolescencia máxima; las operaciones que requieran datos actuales no podrán depender de una copia más antigua que su garantía aceptada.
- **Justificación:** Evitar respuestas incoherentes entre instancias y resurrección de valores obsoletos por carreras entre lectura e invalidación.
- **Prioridad:** P0.
- **Criterio de aceptación:** Para cada categoría futura cacheable hay una política aprobada con duración y obsolescencia máxima cuantificadas. Los escenarios autorizados de actualización y lecturas concurrentes desde ambas instancias cumplen esa ventana, incluso ante repoblación concurrente; cuando no pueda garantizarse, se excluye la categoría de caché o se revisa la garantía antes de aprobarla.
- **Fuente:** C §§2.2, 8; SOL.
- **Estado de aprobación:** C — Coherencia requerida; políticas, ventanas y mecanismo concreto pendientes.

### FND-018 — Fallos y presión de caché

- **Descripción:** La indisponibilidad, expulsión o expiración simultánea de caché deberá tener un comportamiento declarado que limite la presión sobre NoSQL y no relaje autorización, cuotas o consistencia exigidas.
- **Justificación:** Redis no debe convertir una optimización en un fallo en cascada ni en una vía de evasión de controles.
- **Prioridad:** P0.
- **Criterio de aceptación:** En escenarios futuros de Redis no disponible y caché vacía, la aplicación cumple los presupuestos de tiempo y carga aprobados o devuelve un error controlado. Los controles de seguridad mantienen su garantía o deniegan la operación; no se asume continuidad automática de todos los servicios.
- **Fuente:** C §§5, 8; SOL.
- **Estado de aprobación:** C — Tratamiento de fallos requerido; comportamiento por capacidad y límites pendientes.

## 9. Dos instancias en AWS, Azure o GCP y balanceador

### FND-019 — Topología mínima objetivo

- **Descripción:** El entorno académico objetivo deberá ejecutar al menos dos instancias de la API en un proveedor aprobado entre AWS, Azure o GCP, con versiones compatibles, configuración equivalente y dependencias compartidas seguras.
- **Justificación:** Cumplir la rúbrica y demostrar operación distribuida sin seleccionar ahora un proveedor.
- **Prioridad:** P1.
- **Criterio de aceptación:** La evidencia futura identifica dos instancias activas e independientes y su versión, ambiente y conexión a dependencias. No se considera el Dockerfile local prueba de esta topología ni se infiere independencia de zona o región sin decisión explícita.
- **Fuente:** C §§2.2, 2.4; SOL.
- **Estado de aprobación:** C — Dos instancias requeridas y alternativas solicitadas; proveedor, servicio y dominios de fallo pendientes.

### FND-020 — Balanceo y salud

- **Descripción:** El tráfico deberá distribuirse mediante un balanceador que excluya instancias no aptas, permita retirada controlada y considere señales de salud sin exponer información sensible.
- **Justificación:** Mantener servicio ante fallos o sustituciones sin dirigir nuevas solicitudes a instancias incapaces de atenderlas.
- **Prioridad:** P0.
- **Criterio de aceptación:** Ensayos posteriores muestran tráfico atendido por ambas instancias y exclusión de una no apta según tiempos aprobados. La política distingue disponibilidad del proceso y aptitud para recibir tráfico, y define qué fallos de dependencias impiden atenderlo. No se define aquí ninguna ruta técnica ni endpoint de negocio.
- **Fuente:** C §§2.2, 5, 8; SOL.
- **Estado de aprobación:** C — Balanceo requerido; producto, algoritmo, señales y temporización pendientes.

## 10. Rendimiento

### FND-021 — Caché de respuestas y eficiencia

- **Descripción:** El uso de caché deberá justificarse por reutilización y coste medido, sin servir respuestas sensibles entre ámbitos distintos ni ocultar invalidaciones o permisos.
- **Justificación:** Mejorar latencia y consumo de dependencias preservando aislamiento y frescura.
- **Prioridad:** P1.
- **Criterio de aceptación:** La evaluación futura compara aciertos y fallos de caché bajo el mismo perfil y registra latencia, carga y tasa de aciertos. Las respuestas protegidas no aparecen en cachés compartidas sin una política segura aprobada; no se imponen porcentajes de acierto sin conocer la carga.
- **Fuente:** C §§2.2, 5.2, 8; SOL.
- **Estado de aprobación:** C — Caché requerida; elegibilidad y políticas de caché HTTP o de aplicación pendientes.

### FND-022 — Rate limiting entre instancias

- **Descripción:** La limitación de solicitudes deberá definir identidad o ámbito, cuota, ventana y comportamiento de rechazo, manteniendo una garantía global explícita cuando el tráfico se reparta entre instancias.
- **Justificación:** Evitar que añadir instancias multiplique inadvertidamente la cuota y permita abuso de recursos.
- **Prioridad:** P0.
- **Criterio de aceptación:** Con tráfico alternado entre dos instancias, el consumo agregado no supera la cuota y tolerancia aprobadas; los excesos reciben HTTP 429 y una indicación de reintento cuando corresponda. Ante fallo del mecanismo compartido se aplica la política aprobada, sin desactivar silenciosamente la protección.
- **Fuente:** C §§2.2, 5, 8; SOL.
- **Estado de aprobación:** C — Rate limiting requerido; cuotas, tolerancia, ámbito y coordinación pendientes.

### FND-023 — Paginación acotada

- **Descripción:** Las futuras consultas de colecciones deberán ser paginadas, con tamaño máximo y orden estable; la semántica bajo cambios concurrentes deberá declararse.
- **Justificación:** Evitar respuestas y consultas sin límite y expectativas falsas de instantánea consistente.
- **Prioridad:** P1.
- **Criterio de aceptación:** Los tamaños negativos, inválidos o mayores al máximo tienen tratamiento definido y verificable; sobre un conjunto sin cambios, recorrer páginas no repite ni omite elementos por un orden ambiguo. Con cambios concurrentes, el comportamiento coincide con la garantía aprobada.
- **Fuente:** C §§2.2, 8; SOL.
- **Estado de aprobación:** C — Paginación requerida; técnica, tamaños y ordenamiento concretos pendientes.

### FND-024 — Indexación basada en consultas

- **Descripción:** Los índices deberán derivarse de patrones de consulta autorizados y justificarse por coste de lectura, escritura y almacenamiento.
- **Justificación:** Evitar degradación por exploraciones costosas o índices especulativos.
- **Prioridad:** P1.
- **Criterio de aceptación:** Para cada consulta crítica futura existe evidencia de su patrón de acceso e índices utilizados sobre un volumen representativo; cualquier exploración completa queda identificada y justificada. El análisis incluye impacto de escritura y no presupone una característica de un motor pendiente.
- **Fuente:** C §§2.2, 8; SOL.
- **Estado de aprobación:** C — Índices requeridos; índices concretos pendientes del motor y las consultas futuras.

### FND-025 — Serialización JSON

- **Descripción:** Los contratos HTTP que representen datos deberán definir una serialización JSON interoperable, estable y acotada, con política de nombres, nulos, fechas, números y propiedades no expuestas.
- **Justificación:** Evitar ambigüedad contractual, filtración de datos y consumo excesivo de memoria o CPU.
- **Prioridad:** P1.
- **Criterio de aceptación:** Un conjunto de conformidad futuro verifica tipos de contenido, codificación, fechas no ambiguas, precisión numérica acordada y ausencia de propiedades sensibles, ciclos o crecimiento no acotado. Las excepciones de respuesta sin cuerpo o no JSON quedan expresamente documentadas.
- **Fuente:** SOL; C §§4, 5.2. La obligación JSON procede de SOL, no se atribuye como decisión previa de la constitución.
- **Estado de aprobación:** C — JSON solicitado; convenciones, límites y serializador concretos pendientes.

### FND-026 — Compresión negociada y segura

- **Descripción:** La arquitectura deberá contemplar compresión de respuestas elegibles mediante negociación de contenido, excluyendo los casos donde suponga riesgo de exposición o sobrecoste injustificado.
- **Justificación:** Reducir transferencia sin comprometer seguridad ni consumir recursos sin beneficio.
- **Prioridad:** P2.
- **Criterio de aceptación:** Las comprobaciones futuras respetan las capacidades del cliente y las variantes de caché, preservan el contenido y comparan bytes, CPU y latencia. Las respuestas con secretos o datos sensibles mezclados con entradas controlables no se comprimen sin evaluación explícita del riesgo; algoritmos, umbral y elegibilidad tienen aprobación.
- **Fuente:** SOL; C §§5, 8. Compresión es una ampliación solicitada, no un componente constitucional previamente seleccionado.
- **Estado de aprobación:** C — Consideración de compresión solicitada; política y adopción por categoría pendientes.

## 11. Resiliencia, tolerancia a fallos, consistencia, concurrencia e idempotencia

### FND-027 — Fallos acotados y cancelación

- **Descripción:** Cada dependencia deberá tener tiempos de espera y comportamiento de fallo explícitos; la cancelación deberá propagarse cuando sea seguro, y los reintentos no podrán amplificar carga o repetir efectos no idempotentes.
- **Justificación:** Evitar bloqueo indefinido, agotamiento de recursos y fallos en cascada.
- **Prioridad:** P0.
- **Criterio de aceptación:** Bajo fallos futuros autorizados de almacenamiento, Redis o una instancia, las solicitudes terminan dentro del presupuesto aprobado o con error controlado, sin reintentos ilimitados. El número de intentos, su coste total y la seguridad de repetir están justificados; los mecanismos de aislamiento o protección se evalúan sin imponer una biblioteca.
- **Fuente:** C §8; SOL.
- **Estado de aprobación:** C — Resiliencia requerida; presupuestos y mecanismos concretos pendientes.

### FND-028 — Consistencia y concurrencia

- **Descripción:** La arquitectura deberá declarar las garantías de lectura, escritura y resolución de conflictos y evitar pérdida silenciosa de actualizaciones entre instancias dentro del alcance de atomicidad aprobado.
- **Justificación:** NoSQL y escalado horizontal no garantizan por sí solos consistencia fuerte ni exclusión de carreras.
- **Prioridad:** P0.
- **Criterio de aceptación:** Escenarios futuros de escrituras concurrentes producen el resultado o conflicto documentados, sin sobrescritura inadvertida. Se identifican garantías por operación y límites del motor; si la consistencia es eventual, se aprueban convergencia y obsolescencia tolerables, también respecto de la caché.
- **Fuente:** C §§2.4, 8; SOL.
- **Estado de aprobación:** C — Tratamiento requerido; nivel de consistencia y control de conflictos pendientes.

### FND-029 — Idempotencia cuando corresponda

- **Descripción:** Las operaciones futuras que lo requieran deberán tratar solicitudes repetidas de modo que no dupliquen efectos, con una garantía que abarque ambas instancias y las ventanas de fallo declaradas.
- **Justificación:** Los reintentos y pérdidas de respuesta no deben producir efectos duplicados ni promesas falsas de ejecución exactamente una vez.
- **Prioridad:** P0.
- **Criterio de aceptación:** Para cada operación futura clasificada como idempotente se documentan alcance, identidad de repetición, vigencia y semántica de respuesta. Las repeticiones simultáneas y posteriores a una respuesta perdida no duplican efectos dentro de esa garantía; si se usan claves, la reutilización con contenido diferente tiene un rechazo definido. Los fallos entre efecto y registro de resultado están contemplados.
- **Fuente:** C §8; SOL.
- **Estado de aprobación:** C — Análisis de idempotencia requerido; operaciones, ventanas y mecanismo pendientes.

## 12. Middleware, gestión de errores y contratos de respuesta

### FND-030 — Comportamiento transversal del procesamiento HTTP

- **Descripción:** El procesamiento mediante middleware deberá aplicar coherentemente seguridad, límites, correlación y tratamiento de errores, sin depender de reglas de negocio ni permitir su omisión accidental.
- **Justificación:** El orden y la cobertura de controles transversales afectan autorización, trazabilidad y respuestas de fallo.
- **Prioridad:** P0.
- **Criterio de aceptación:** La revisión futura justifica las dependencias de orden y cubre respuestas exitosas, rechazos, errores y cancelaciones. Ninguna ruta protegida evade controles por su posición de procesamiento y el manejo de cabeceras de proxy respeta la frontera de confianza. No se prescribe aquí una secuencia de llamadas o código.
- **Fuente:** C §§4–5; SOL.
- **Estado de aprobación:** C — Capacidad transversal solicitada; composición y orden concretos pendientes.

### FND-031 — Contratos de respuesta y errores seguros

- **Descripción:** Las respuestas deberán respetar semántica HTTP y contratos documentados; los errores tendrán una representación estable con identificación segura del problema y correlación, sin exponer trazas, secretos o detalles de persistencia.
- **Justificación:** Hacer predecible el consumo de la API y permitir diagnóstico sin filtrar información interna.
- **Prioridad:** P0.
- **Criterio de aceptación:** Casos futuros de validación, autenticación, autorización, limitación de solicitudes, dependencia no disponible y error inesperado producen estados y contenido acordados. No se transforma todo error en HTTP 200 ni se exige un cuerpo para respuestas que no lo admiten. El esquema de error, localización y cualquier envoltorio de éxito requieren aprobación y no se generan DTOs en esta especificación.
- **Fuente:** C §§4, 5.2; SOL.
- **Estado de aprobación:** C — Coherencia y confidencialidad requeridas; formato concreto pendiente.

## 13. Logging, observabilidad, métricas y trazabilidad

### FND-032 — Registros estructurados y seguros

- **Descripción:** Los eventos operativos deberán registrarse de forma estructurada con severidad, instante, ambiente, versión, instancia y correlación, minimizando datos personales y evitando secretos o tokens.
- **Justificación:** Permitir diagnóstico distribuido y auditoría operativa sin crear otra fuente de exposición.
- **Prioridad:** P1.
- **Criterio de aceptación:** La revisión futura de eventos representativos permite relacionar una solicitud y sus fallos entre instancias y dependencias, sin contener credenciales ni cuerpos sensibles no autorizados. Están aprobadas retención, acceso, tratamiento de datos y límites de volumen.
- **Fuente:** C §§2.2, 5.2; SOL.
- **Estado de aprobación:** C — Logging requerido; herramienta, campos finales y política de retención pendientes.

### FND-033 — Métricas y trazas distribuidas

- **Descripción:** La arquitectura deberá observar tasa de solicitudes, latencia, errores, saturación, dependencias, caché, rate limiting y salud por instancia, y mantener correlación de solicitudes y trazas sin confiar ciegamente en identificadores externos.
- **Justificación:** Distinguir síntomas de causas y diagnosticar el comportamiento de dos instancias.
- **Prioridad:** P1.
- **Criterio de aceptación:** En una observación futura autorizada se distinguen métricas por ambiente, versión e instancia y se relacionan solicitudes con dependencias según la política de muestreo. Se han definido cardinalidad acotada, tratamiento de correlaciones entrantes, alertas y ausencia de etiquetas sensibles. No se presupone un proveedor de telemetría.
- **Fuente:** C §§2.2, 5, 8; SOL.
- **Estado de aprobación:** C — Observabilidad requerida; señales finales, muestreo, alertas y plataforma pendientes.

## 14. Pruebas unitarias, integración, seguridad y calidad

### FND-034 — Verificación unitaria y de integración

- **Descripción:** La arquitectura deberá permitir pruebas unitarias de comportamientos aislados y de integración de HTTP, persistencia NoSQL, Redis y límites entre instancias según los riesgos y requisitos aprobados.
- **Justificación:** Comprobar contratos y garantías reales sin confundir aislamiento unitario con integración efectiva.
- **Prioridad:** P1.
- **Criterio de aceptación:** En una fase autorizada, cada criterio aplicable tiene casos, resultados y evidencia vinculados al ID; las integraciones críticas incluyen escenarios con dependencias compatibles con las seleccionadas. Los datos de ensayo están aislados y no usan secretos de producción. No se fija una biblioteca o porcentaje de cobertura todavía.
- **Fuente:** C §§3, 6; SOL.
- **Estado de aprobación:** C — Tipos de pruebas confirmados; herramientas, alcance y umbrales de calidad pendientes.

### FND-035 — Pruebas de seguridad y escenarios adversos

- **Descripción:** La verificación futura deberá incluir casos positivos y negativos de autenticación, autorización, validación, aislamiento, cuotas, concurrencia, idempotencia y fallos, con objetivos autorizados y controlados.
- **Justificación:** Detectar incumplimientos que no aparecen en el flujo exitoso.
- **Prioridad:** P0.
- **Criterio de aceptación:** La relación de riesgos y controles posee casos verificables y evidencia; los hallazgos bloqueantes no se aceptan sin corrección o excepción formal justificada. No se realizan pruebas activas sobre objetivos ajenos o no autorizados y no se registra una verificación como aprobada solo por su diseño documental.
- **Fuente:** C §§5–8; SOL.
- **Estado de aprobación:** C — Seguridad y calidad requeridas; ejecución, entorno y clasificación de bloqueos pendientes.

## 15. Análisis estático, dependencias, secretos y CI/CD

### FND-036 — Controles de calidad y cadena de suministro

- **Descripción:** La estrategia futura deberá contemplar análisis estático, dependencias y vulnerabilidades, detección de secretos y revisión de artefactos o contenedores, con criterios explícitos de bloqueo y excepciones.
- **Justificación:** Reducir defectos y riesgos introducidos por código, componentes y entrega.
- **Prioridad:** P1.
- **Criterio de aceptación:** Una definición aprobada relaciona cada control con su alcance, versión de herramienta, evidencia y umbral de aprobación; cada excepción tiene responsable, motivo y vigencia. No se presume que un paquete es seguro o vulnerable por su mera presencia y no se incorporan herramientas desde este documento.
- **Fuente:** C §§6–7; SOL.
- **Estado de aprobación:** C — Controles solicitados conforme a la estrategia constitucional; herramientas y umbrales pendientes.

### FND-037 — Entrega controlada y trazable

- **Descripción:** La futura estrategia CI/CD deberá conservar la relación entre fuente aprobada, verificaciones, artefacto y ambiente, con mínimo privilegio, secretos externos y aprobación para promocionar o revertir.
- **Justificación:** Evitar despliegues no autorizados y cambios no identificables entre ambientes.
- **Prioridad:** P1.
- **Criterio de aceptación:** Una entrega futura autorizada puede relacionarse con la revisión de origen, resultados de controles, identificador inmutable de artefacto y aprobación del ambiente. Se documentan responsables, permisos y criterios de promoción y reversión, sin que crear o aprobar esta especificación habilite una canalización.
- **Fuente:** C §7; SOL.
- **Estado de aprobación:** C — Estrategia solicitada; plataforma, automatización y reglas finales pendientes.

## 16. Infraestructura como código y despliegue

### FND-038 — Infraestructura reproducible y revisable

- **Descripción:** El diseño futuro deberá contemplar infraestructura como código para representar los recursos aprobados, su configuración no secreta y las diferencias autorizadas entre ambientes, sin depender exclusivamente de cambios manuales no registrados.
- **Justificación:** Facilitar reproducibilidad, revisión y control de desviaciones.
- **Prioridad:** P1.
- **Criterio de aceptación:** En una fase futura autorizada existe una definición versionada y revisada de la topología aprobada, sin secretos, con gestión protegida de su estado cuando corresponda; su reproducción en un ambiente aislado y la identificación de desviaciones tienen evidencia. No se elige herramienta ni se crea infraestructura en esta fase.
- **Fuente:** SOL; C §7 como contexto de entrega. IaC es una ampliación solicitada, no una tecnología seleccionada por la constitución.
- **Estado de aprobación:** C — Consideración de IaC solicitada; herramienta, alcance de recursos y proceso de aplicación pendientes.

### FND-039 — Despliegue y reversión seguros

- **Descripción:** La futura publicación deberá permitir introducir y retirar versiones compatibles sin perder datos ni enviar tráfico a instancias no aptas, y disponer de una reversión explícita y sus límites de compatibilidad.
- **Justificación:** Conservar integridad y disponibilidad ante cambios o fallos de una entrega.
- **Prioridad:** P0.
- **Criterio de aceptación:** Un ensayo autorizado con dos instancias y balanceador evidencia versión identificable, retirada controlada, validación de salud y recuperación ante una entrega fallida. La compatibilidad de contratos y datos durante coexistencia y reversión está aprobada; no se promete revertir datos de forma automática ni sin pérdida si no hay evidencia.
- **Fuente:** C §§2.2, 7–8; SOL.
- **Estado de aprobación:** C — Despliegue controlado requerido; estrategia concreta, convivencia de versiones y recuperación pendientes.

## 17. Versionamiento y configuración de ambientes

### FND-040 — Evolución de contratos

- **Descripción:** La API deberá tener una política explícita de versionamiento, compatibilidad, cambios incompatibles y retirada, independiente de la numeración de este documento y de la versión del artefacto.
- **Justificación:** Evitar ruptura silenciosa de consumidores durante la evolución.
- **Prioridad:** P1.
- **Criterio de aceptación:** La política futura define qué constituye un cambio incompatible, cómo se identifica la versión y cómo se comunica la retirada; los ensayos de compatibilidad preservan contratos vigentes durante el período aprobado. No se elige aquí versionamiento por ruta, cabecera u otro mecanismo.
- **Fuente:** C §2.2; SOL.
- **Estado de aprobación:** C — Versionamiento requerido; mecanismo y períodos de soporte pendientes.

### FND-041 — Configuración y aislamiento de ambientes

- **Descripción:** Desarrollo, pruebas y producción o su equivalente académico deberán tener configuración y secretos separados, coherentes entre instancias del mismo ambiente, sin fijar un proveedor ni incluir valores sensibles.
- **Justificación:** Evitar conexiones cruzadas, configuraciones inseguras y diferencias ocultas entre instancias.
- **Prioridad:** P0.
- **Criterio de aceptación:** La revisión futura identifica configuración necesaria, origen, precedencia, valores obligatorios y validación; una configuración inválida para operar con seguridad impide admitir tráfico. Redis, persistencia, secretos y telemetría no mezclan ambientes. La exposición de información diagnóstica está limitada según el ambiente y permisos aprobados.
- **Fuente:** C §§5.1, 7–8; SOL.
- **Estado de aprobación:** C — Separación de ambientes solicitada; catálogo, precedencia y controles concretos pendientes.

## 18. Requisitos no funcionales verificables

Los valores de esta sección son **propuestas cuantitativas**, no metas docentes confirmadas ni prestaciones actuales. Se requiere ratificación del responsable y un entorno de evaluación aprobado antes de medir. Los resultados deberán identificar versiones, recursos asignados, datos sintéticos, latencia de dependencias, condiciones de caché, límites de carga, reloj de medición y tamaño de muestra. No se inventan resultados ni endpoints para evaluarlos ahora.

### FND-042 — Presupuesto de rendimiento propuesto

- **Descripción:** Se propone que la base soporte 50 solicitudes válidas por segundo durante 10 minutos, tras 2 minutos de estabilización, con p95 de latencia de extremo a extremo no mayor de 500 ms, p99 no mayor de 1.000 ms y tasa de fallos no mayor del 1 %.
- **Justificación:** Ofrecer un objetivo medible inicial que pueda ratificarse o ajustarse antes de ejecutar ensayos.
- **Prioridad:** P1.
- **Criterio de aceptación:** En un ensayo futuro aprobado se cumplen conjuntamente los valores ratificados para una mezcla sintética sin negocio, con cuerpos de solicitud y respuesta de hasta 16 KiB y proporción 50/50 de aciertos y fallos de caché cuando corresponda. La mezcla, los recursos cloud, región, volumen persistente y las operaciones de ensayo requieren definición previa aprobada. Se miden desde el cliente atravesando el balanceador y se cuentan como fallos los 5xx, desconexiones y tiempos agotados; los rechazos esperados se evalúan por separado y las cuotas de este ensayo no quedan por debajo de la carga ofrecida.
- **Fuente:** SOL §18; C §§2.2, 6, 8 como contexto. Los números son propuesta de esta especificación, no valores extraídos de la constitución.
- **Estado de aprobación:** P — Todos los objetivos y condiciones de medición pendientes de ratificación; no existe evidencia de rendimiento.

### FND-043 — Recuperación de una instancia propuesta

- **Descripción:** Se propone detectar y excluir del balanceo una instancia no apta en un máximo de 30 segundos, conservando la capacidad de recibir nuevas solicitudes en la instancia sana sin intervención manual.
- **Justificación:** Hacer verificable el objetivo de tolerar un fallo de instancia en la topología mínima.
- **Prioridad:** P0.
- **Criterio de aceptación:** En un ensayo futuro con capacidad suficiente en la instancia sana, el tiempo entre el fallo y la exclusión es como máximo el ratificado; después de ese límite, el 100 % de las nuevas solicitudes admitidas se dirige a instancias aptas durante una observación de 10 minutos. Los fallos de solicitudes en curso se registran y se evalúan según las garantías de resiliencia e idempotencia; no se promete ausencia total de interrupción ni un SLA mensual.
- **Fuente:** C §§2.2, 8; SOL. Los 30 segundos y 10 minutos son propuesta de esta especificación.
- **Estado de aprobación:** P — Umbral, condiciones y compromiso operativo pendientes de aprobación.

### FND-044 — Consumo acotado bajo abuso

- **Descripción:** Los límites aprobados de tamaño, paginación, concurrencia admitida, cuotas y tiempo de espera deberán seguir siendo efectivos con tráfico distribuido y entradas que intenten excederlos.
- **Justificación:** Expresar la protección de recursos como un comportamiento verificable y no solo como presencia de middleware.
- **Prioridad:** P0.
- **Criterio de aceptación:** El 100 % de los casos de exceso del conjunto de ensayo autorizado recibe el tratamiento documentado sin admitir cargas o tamaños de página superiores al máximo. Las cuotas globales respetan su tolerancia aprobada y las operaciones no exceden el presupuesto total de tiempo admitido. Los máximos y la tolerancia deben estar cuantificados y aprobados antes de aceptar el resultado.
- **Fuente:** C §§2.2, 5, 8; SOL.
- **Estado de aprobación:** C — Acotación requerida; valores numéricos concretos pendientes, sin aceptación técnica posible hasta definirlos.

### FND-045 — Invariantes de seguridad

- **Descripción:** La arquitectura deberá satisfacer cero accesos no autorizados y cero exposición de secretos en el conjunto de aceptación de seguridad aprobado, incluyendo cruces entre instancias y ámbitos de caché.
- **Justificación:** Establecer un resultado inequívoco para controles críticos sin afirmar que un conjunto finito de pruebas demuestra seguridad absoluta.
- **Prioridad:** P0.
- **Criterio de aceptación:** El 100 % de los casos negativos aprobados de JWT, permisos, aislamiento y filtración cumple el rechazo o la ausencia de exposición previstos. Un solo incumplimiento invalida este criterio; el informe especifica alcance y limitaciones y no extrapola ausencia universal de vulnerabilidades.
- **Fuente:** C §§5–6; SOL.
- **Estado de aprobación:** C — Invariantes derivadas de seguridad confirmada; conjunto de casos y verificación pendientes.

### FND-046 — Cobertura de correlación operativa

- **Descripción:** Toda solicitud admitida deberá poder correlacionarse con su resultado y eventos operativos cuando se generen, incluso si la traza distribuida está sujeta a muestreo.
- **Justificación:** Diagnosticar errores y respuestas entre dos instancias sin depender de trazas siempre completas.
- **Prioridad:** P1.
- **Criterio de aceptación:** En el conjunto futuro de ensayo, el 100 % de las solicitudes admitidas tiene un identificador de correlación válido asociado a su resultado y a los registros de solicitud previstos, incluidos rechazos y errores gestionados. Las trazas muestreadas se distinguen de la cobertura de correlación y no se registran datos sensibles. Las pérdidas ante caída abrupta se cuantifican y se contrastan con la garantía operativa previamente aprobada, sin ocultarlas como cobertura completa.
- **Fuente:** C §§2.2, 5.2; SOL. La cobertura del 100 % es una concreción propuesta.
- **Estado de aprobación:** P — Cobertura y garantía ante pérdida abrupta pendientes de aprobación.

### FND-047 — Recuperación de datos propuesta

- **Descripción:** Se propone un objetivo de tiempo de recuperación (RTO) no mayor de 30 minutos y un objetivo de punto de recuperación (RPO, Recovery Point Objective) no mayor de 15 minutos para un escenario aprobado de pérdida de persistencia dentro del ambiente académico. RPO es la máxima pérdida de datos tolerable expresada en tiempo entre el incidente y el punto consistente recuperable más reciente. RPO ≤ 15 minutos permanece como PROPUESTA, no como requisito aprobado ni cumplido.
- **Justificación:** Diferenciar la recuperación de datos de la recuperación de una instancia o de una caché reconstruible.
- **Prioridad:** P1.
- **Criterio de aceptación:** Solo tras ratificar los objetivos, un ensayo de restauración posterior y autorizado verifica el RTO desde la declaración del incidente hasta recuperar el servicio validado, y contrasta con el RPO ratificado el intervalo entre el instante del incidente y el punto consistente recuperable más reciente. Se identifican ambos instantes y las escrituras perdidas dentro de ese intervalo; no se mide la antigüedad de los registros individuales. El alcance del fallo, la validez de copias y la comprobación de integridad constan aprobados; el resultado no se extrapola a pérdida regional ni a escenarios no ensayados. Esta definición no aporta evidencia de cumplimiento ni aprueba la propuesta de RPO ≤ 15 minutos.
- **Fuente:** SOL sobre tolerancia a fallos y despliegue; C §§7–8 como fundamento. Los objetivos numéricos de recuperación son propuesta de esta especificación.
- **Estado de aprobación:** P — RTO, RPO, escenario, mecanismo de respaldo y coste pendientes de aprobación.

## 19. Matriz de trazabilidad con constitution.md

### FND-048 — Trazabilidad documental y de aceptación

- **Descripción:** Cada requisito deberá mantener relación con su fuente canónica o solicitud explícita, decisiones pendientes, criterios de aceptación y evidencias futuras, sin inventar aprobación o resultados.
- **Justificación:** Permitir revisar cobertura y evolución SDD sin convertir una ampliación solicitada en una decisión constitucional previa.
- **Prioridad:** P0.
- **Criterio de aceptación:** La matriz cubre todos los IDs de esta versión y distingue los requisitos derivados de C, los añadidos por SOL y las propuestas. En una revisión futura autorizada, cada evidencia tiene ID, alcance y resultado o figura expresamente pendiente; los cambios de fuente y aprobación son identificables.
- **Fuente:** C §§2–3, 11; SOL.
- **Estado de aprobación:** C — Trazabilidad confirmada; aprobación del borrador y evidencias técnicas pendientes.

En la tabla, **C** enlaza conceptualmente a la [constitución canónica](../../.specify/memory/constitution.md); el [archivo raíz](../../constitution.md) conserva su función de acceso. Las referencias por apartado evitan copiar el contenido normativo. Los rangos de IDs incluyen todos sus elementos y no asignan aprobación por agrupación.

| Requisitos | Fuente constitucional | Fuente complementaria y relación | Evidencia futura esperada, no ejecutada |
| --- | --- | --- | --- |
| FND-001 | C §§1–3, 9–11 | SOL y GOB: alcance y autorización | Revisión de estados, fuentes y autorizaciones |
| FND-002, FND-003 | C §§4, 5.2 | SOL: separación y contratos | Revisión de responsabilidades, límites y dependencias |
| FND-004 | C §§2.1, 2.4 | SOL: plataforma 10 | Metadatos y compatibilidad aprobada |
| FND-005 | C §§2.3, 4 | SOL: conservar REST como propuesta | Decisión explícita sobre el estilo arquitectónico |
| FND-006, FND-007 | C §§2.2, 8 | SOL: escalabilidad sin estado | Ensayos entre instancias y evaluación de capacidad |
| FND-008 | C §5.1 | SOL: HTTPS | Evaluación de transporte y frontera de confianza |
| FND-009, FND-010, FND-011 | C §§2.2, 5, 5.2 | SOL: JWT, autorización y validación | Casos positivos, negativos y de límites |
| FND-012 | C §5.1 | SOL: secretos | Controles de exposición, acceso y rotación |
| FND-013 | C §5 | SOL: OWASP API Security | Relación riesgos, controles y riesgos residuales |
| FND-014, FND-015 | C §§2.1, 2.2, 2.4, 4, 8 | SOL: NoSQL sin seleccionar motor | Decisión de motor y garantías de acceso |
| FND-016, FND-017, FND-018 | C §§2.2, 5, 8 | SOL: Redis y coherencia | Ensayos de aislamiento, invalidación y fallos |
| FND-019, FND-020 | C §§2.2, 2.4, 5, 8 | SOL: AWS, Azure o GCP sin elección | Evidencia de topología y comportamiento de balanceo |
| FND-021, FND-022 | C §§2.2, 5, 8 | SOL: caché y rate limiting | Mediciones y cuotas agregadas entre instancias |
| FND-023, FND-024 | C §§2.2, 8 | SOL: paginación e indexación | Casos de límites, estabilidad y acceso a datos |
| FND-025 | C §§4, 5.2 como principios | SOL añade JSON explícitamente | Conformidad de serialización y exposición |
| FND-026 | C §§5, 8 como principios | SOL añade compresión explícitamente | Negociación, seguridad y comparación de recursos |
| FND-027, FND-028, FND-029 | C §8 | SOL: resiliencia, consistencia, concurrencia e idempotencia | Casos de fallos, conflictos y duplicados |
| FND-030, FND-031 | C §§4–5 | SOL: middleware y respuestas | Revisión transversal y conformidad de errores |
| FND-032, FND-033 | C §§2.2, 5, 8 | SOL: observabilidad | Registros seguros, métricas y correlación |
| FND-034, FND-035 | C §§3, 5–8 | SOL: pruebas y calidad | Relación de casos y evidencias autorizadas |
| FND-036, FND-037 | C §§6–7 | SOL: controles y CI/CD | Resultados de controles y trazabilidad de entrega |
| FND-038 | C §7 como fundamento | SOL añade IaC sin elegir herramienta | Definición reproducible y revisión de desviaciones |
| FND-039 | C §§2.2, 7–8 | SOL: despliegue | Ensayo autorizado de entrega y reversión |
| FND-040, FND-041 | C §§2.2, 5.1, 7–8 | SOL: versiones y ambientes | Política de evolución y validación de configuración |
| FND-042, FND-043 | C §§2.2, 6, 8 como fundamento | SOL pide RNF verificables; números propuestos aquí | Ratificación y mediciones de carga y fallo de instancia |
| FND-044, FND-045 | C §§2.2, 5–6, 8 | SOL: límites y seguridad verificables | Casos de abuso y ausencia de accesos indebidos |
| FND-046 | C §§2.2, 5.2 | SOL: trazabilidad operativa; porcentaje propuesto aquí | Cobertura y pérdida de correlación medidas |
| FND-047 | C §§7–8 como fundamento | SOL: tolerancia a fallos; RTO y RPO propuestos aquí | Ratificación y ensayo de restauración |
| FND-048 | C §§2–3, 11 | SOL: matriz de trazabilidad | Revisión de cobertura de IDs, estados y evidencias |
| FND-049 | C §§1, 3–4, 9–10 | SOL: exclusiones absolutas | Comprobación documental de alcance y ausencia de cambios ajenos |

## 20. Riesgos, restricciones y decisiones pendientes

### Riesgos identificados, no incidentes demostrados

Los siguientes elementos describen riesgos de esta especificación, no nuevos requisitos independientes ni hallazgos técnicos verificados.

| Riesgo | Requisitos relacionados | Consecuencia potencial y tratamiento documental |
| --- | --- | --- |
| Desalineación entre la fase solicitada y los documentos que aún registran Fase 0 | FND-001, FND-048, FND-049 | Interpretar autorización más amplia de la recibida. Conservar el límite de esta solicitud y solicitar autorización separada para sincronizar gobierno antes de nuevas acciones afectadas. |
| Motor NoSQL no elegido | FND-014, FND-015, FND-024, FND-028 | Presuponer atomicidad, índices o consistencia inexistentes. Mantener garantías concretas pendientes y exigir justificación al seleccionar. |
| Caché obsoleta, repoblación concurrente o aislamiento incorrecto | FND-016–FND-018, FND-021, FND-028 | Respuestas inconsistentes o filtración. Exigir políticas y ventanas aprobadas antes de aceptación. |
| Redis como dependencia común | FND-018, FND-022, FND-027 | Caída en cascada o evasión de cuotas. Distinguir caché opcional de controles que no pueden relajarse. |
| Dos instancias con dependencias o dominio de fallo compartidos | FND-019, FND-020, FND-043, FND-047 | Confundir duplicación de procesos con alta disponibilidad completa. No afirmar independencia de zonas, región o almacenamiento sin decisión y evidencia. |
| JWT sin autorización suficiente o claves gestionadas incorrectamente | FND-009, FND-010, FND-012, FND-045 | Acceso indebido. Mantener separados identidad, permisos, rotación y validación. |
| Reintentos y duplicados durante fallos | FND-027–FND-029 | Repetición de efectos o pérdida de actualizaciones. Exigir garantías y ventanas explícitas por operación futura. |
| Compresión o telemetría con información sensible | FND-026, FND-031–FND-033 | Exposición de secretos o datos. Exigir elegibilidad, minimización y evaluación de riesgo. |
| Umbrales cuantitativos sin contexto o presupuesto aprobado | FND-042–FND-047 | Prometer rendimiento o recuperación irreales. Mantener propuestas pendientes hasta ratificación con entorno y costes. |
| IaC y CI/CD entendidos como permiso para ejecutar | FND-036–FND-039, FND-049 | Despliegue o modificación no autorizados. Separar especificación, aprobación de diseño y autorización operativa. |
| Revisión atribuida a perfiles sin acceso a esta especificación | FND-001, FND-048 | Evidencia documental falsa o intento de eludir permisos. No atribuir revisiones ni modificar accesos sin autorización. |

### Decisiones pendientes

Los códigos `DEC` identifican asuntos por resolver, no decisiones adoptadas ni tareas autorizadas. Su aprobación corresponde al responsable del proyecto.

| Código | Asunto pendiente | Impacto principal |
| --- | --- | --- |
| DEC-01 | Revisión y aprobación de este borrador, sus prioridades y autorización separada para sincronizar documentos de fase | Gobierno, alcance y continuidad |
| DEC-02 | Aceptación o rechazo de API REST modular y estructura definitiva de capas o módulos | FND-002–FND-005 |
| DEC-03 | Motor NoSQL, modelo futuro, garantías, particionamiento e índices | FND-014, FND-015, FND-024, FND-028 |
| DEC-04 | Proveedor entre AWS, Azure o GCP, servicio de ejecución, balanceador, red y dominios de fallo | FND-008, FND-019, FND-020, FND-043 |
| DEC-05 | Librerías, versiones complementarias y parches aprobados, sin asumir nuevas selecciones por la plantilla | FND-004 y componentes futuros |
| DEC-06 | Emisor y emisión JWT, claves, vigencia, rotación y políticas de autorización | FND-009, FND-010, FND-012 |
| DEC-07 | Redis, aislamiento, elegibilidad, expiración, invalidación, obsolescencia y comportamiento ante fallos | FND-016–FND-018, FND-021 |
| DEC-08 | Cuotas globales, tolerancia, ventanas, identidad de consumo y límites de entradas o concurrencia | FND-011, FND-022, FND-044 |
| DEC-09 | Paginación, contratos JSON, errores, compresión y estrategia de versionamiento | FND-023, FND-025, FND-026, FND-031, FND-040 |
| DEC-10 | Presupuestos de dependencia, reintentos, consistencia, control de conflictos e idempotencia | FND-027–FND-029 |
| DEC-11 | Plataforma de observabilidad, retención, datos permitidos, muestreo, correlación y alertas | FND-032, FND-033, FND-046 |
| DEC-12 | Herramientas y entornos de pruebas, cobertura, criterios de calidad, edición OWASP y controles de seguridad | FND-013, FND-034–FND-036, FND-045 |
| DEC-13 | CI/CD, IaC, aprobación de entrega, permisos, coexistencia de versiones y reversión | FND-037–FND-039 |
| DEC-14 | Catálogo y precedencia de configuración, ambientes y gestión externa de secretos | FND-012, FND-041 |
| DEC-15 | Ratificación de objetivos de carga, latencia, errores, recuperación de instancia y cobertura de correlación | FND-042, FND-043, FND-046 |
| DEC-16 | Ratificación de RTO, RPO, escenarios de pérdida, respaldo, restauración y costes | FND-047 |
| DEC-17 | Autorización eventual y mínima de lectura o carga para que agentes o skills revisen este artefacto, si se solicita | FND-001, FND-048; no se cambian permisos aquí |

### Restricciones de aceptación y continuidad

Un requisito marcado C no prueba su implementación. Uno marcado P no se transforma en C por aparecer en este archivo. Ni aprobar este borrador ni resolver un pendiente autoriza automáticamente a crear planificación, modificar código o ejecutar verificaciones. La indisponibilidad de un detalle obligatorio deberá mantenerse visible y bloquear la aceptación que dependa de él, no ocultarse como cumplimiento.

## 21. Exclusiones del alcance

### FND-049 — Delimitación absoluta de esta entrega

- **Descripción:** Esta entrega deberá limitarse a crear `specs/001-backend-foundation/spec.md`, sin implementación, modificaciones de archivos existentes o generación de artefactos adicionales.
- **Justificación:** Conservar el alcance autorizado y separar especificación de planificación, implementación y operación.
- **Prioridad:** P0.
- **Criterio de aceptación:** El único archivo creado por esta intervención es esta especificación. No se han generado `plan.md`, `task.md`, artefactos equivalentes, código o infraestructura, ni se han modificado archivos existentes. No se ejecutaron comandos, compilaciones, pruebas, instalaciones, migraciones o despliegues; no se atribuye ejecución o revisión a agentes o skills no invocados.
- **Fuente:** SOL, restricciones absolutas; C §§1, 3–4, 9–10.
- **Estado de aprobación:** C — Límite de entrega expresamente confirmado; no implica aceptación técnica de ningún otro requisito.

Quedan expresamente fuera del contenido funcional y de las acciones de esta entrega:

- Funcionalidades específicas de ENVER, incluidos usuarios, pedidos, repartidores, pagos y entregas; no se definen sus endpoints, flujos, modelos o reglas de negocio.
- Entidades, DTOs, controladores, servicios, CRUD, esquemas de datos o contratos específicos de esas funcionalidades.
- Selección de MongoDB u otro motor NoSQL, de un proveedor cloud o de paquetes adicionales sin aprobación.
- Pasos de implementación, cronogramas, planificación técnica o listas de tareas de ejecución.
- Implementación de infraestructura, contenedores, balanceadores, Redis, almacenamiento, JWT, middleware, observabilidad, canalizaciones o IaC.
- Modificación de `Program.cs`, `.csproj`, Dockerfile, archivos de Visual Studio, configuración, `AGENTS.md`, `memoria.md`, `README.md`, constitución canónica o referencia raíz.
- Modificación de agentes, sus permisos, skills o configuraciones de OpenCode; invocación de subagentes o ejecución de los procedimientos de las skills.
- Cualquier comando, prueba, compilación, instalación, migración, análisis automatizado o despliegue, así como declaraciones de cumplimiento no respaldadas por evidencias futuras autorizadas.

El trabajo se detiene al completar esta especificación. No se genera `plan.md` ni `task.md` y no se inicia la implementación.
