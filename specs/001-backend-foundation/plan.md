# Plan documental 001 — Arquitectura base de ENVER Backend

## 1. Objetivo, alcance y estado

| Elemento | Valor |
| --- | --- |
| Solución y proyecto | `Enver.Backend` / `Enver.API` |
| Plataforma confirmada | C#, ASP.NET Core 10 y .NET 10 |
| Fase de esta solicitud | Planificación documental de arquitectura base |
| Versión del plan | 0.1.0 |
| Estado | Borrador, pendiente de revisión y aprobación |
| Especificación de entrada | [`spec.md`](spec.md), versión 0.1.0, también en borrador |
| Autoridad documental | [Constitución canónica](../../.specify/memory/constitution.md), versión 1.0.0 |
| Autorización vigente para esta intervención | Crear únicamente este `plan.md`, sin ejecutar tareas |
| Estado técnico | Ningún componente, control o requisito se declara implementado ni verificado por este plan |

El objetivo es describir **cómo construir posteriormente** una base modular, segura, verificable y operable, relacionando decisiones propuestas con requisitos reales. No se definen funcionalidades de ENVER, no se generan tareas ejecutables y no se implementa la arquitectura.

La constitución conserva su autoridad. [`AGENTS.md`](../../AGENTS.md), [`memoria.md`](../../memoria.md) y [`README.md`](../../README.md) aún registran la Fase 0; no se modifican. La solicitud actual autoriza específicamente este artefacto de planificación, no una transición automática a ejecución ni una reforma de las reglas. La sincronización futura de documentos de estado requiere autorización independiente.

La prohibición de crear un plan durante la intervención que produjo `spec.md`, incluido **FND-049**, se conserva como límite histórico de aquella entrega. No se cambia su redacción ni se transforma ese requisito en una autorización genérica: este plan se crea por una solicitud posterior y delimitada. La especificación no se considera aprobada por el hecho de planificarla.

### Convenciones

- **C §n:** apartado de la constitución canónica. [`constitution.md`](../../constitution.md) de la raíz es solo su referencia y no otra constitución.
- **FND-001 a FND-049:** requisitos existentes de `spec.md`; se conservan sus identificadores y estados de origen sin renumerarlos.
- **DEC-01 a DEC-17:** asuntos pendientes ya registrados en `spec.md` §20; no son decisiones aprobadas.
- **PLD-nnn:** decisiones de diseño y planificación de este plan. No reemplazan requisitos ni códigos DEC. Cada una identifica requisitos, alternativas, propuesta justificada, riesgos y estado de aprobación.
- **Confirmado en origen** identifica una obligación de la fuente, no la aprobación de este plan ni del detalle propuesto. Salvo indicación expresa sobre la base tecnológica ya confirmada, todas las propuestas de diseño permanecen pendientes.

### PLD-001 — Construcción futura por incrementos condicionados

- **Requisitos relacionados:** FND-001, FND-034, FND-035, FND-037, FND-038, FND-048, FND-049.
- **Alternativas:** Construcción integral sin puertas de revisión; incrementos con aprobación de alcance y evidencia; creación automática de tareas al terminar el plan.
- **Propuesta justificada:** Usar incrementos condicionados por decisiones resueltas y criterios de aceptación. Así se reduce el riesgo de implementar supuestos mientras `spec.md` sigue en borrador. La secuencia conceptual siguiente no constituye un backlog ni autoriza su ejecución.
- **Riesgos:** Interpretar el plan como permiso de trabajo; avanzar con supuestos abiertos; confundir documentación con evidencias técnicas.
- **Estado de aprobación:** Propuesta pendiente. Solo está autorizada la creación del presente archivo.

| Incremento futuro conceptual | Condición de entrada | Enfoque de construcción posterior | Evidencia esperada antes de continuar |
| --- | --- | --- | --- |
| Gobierno y diseño | Revisión de `spec.md`, este plan y autorización expresa | Resolver decisiones bloqueantes y delimitar el cambio autorizado | Aprobación con alcance y pendientes visibles |
| Base y límites | Plataforma y organización aprobadas | Establecer composición, responsabilidades y dependencias mínimas | Revisión de límites sin lógica de negocio |
| Capacidades transversales | Contratos, seguridad y configuración aprobados | Incorporar protección HTTP, errores, correlación y configuración validada | Casos de aceptación de límites y seguridad |
| Integraciones y distribución | Motor, Redis y garantías aprobados | Incorporar adaptadores y comportamiento compartido entre instancias | Evidencia de aislamiento, coherencia y fallos |
| Verificación | Entornos y procedimientos de ensayo autorizados | Relacionar casos unitarios, integración, seguridad y rendimiento con los FND | Resultados reproducibles y limitaciones declaradas |
| Entrega reproducible | Cloud, IaC, CI/CD y presupuesto aprobados | Preparar artefacto, recursos, dos instancias, balanceo y promoción controlada | Evidencias de topología, salud, reversión y recuperación |

La definición de casos documentales puede preceder al código; ejecutarlos exige otra autorización. Ningún incremento habilita por sí mismo el siguiente ni permite crear `task.md`.

## 2. Trazabilidad con constitution.md y spec.md

### PLD-002 — Una sola cadena de autoridad y evidencia

- **Requisitos relacionados:** FND-001, FND-048, FND-049.
- **Alternativas:** Duplicar la especificación en el plan; cambiar estados de requisitos al redactar decisiones; referenciar la fuente y registrar explícitamente la relación requisito–decisión–evidencia.
- **Propuesta justificada:** Mantener la cadena constitución canónica → requisito FND → decisión PLD → cambio futuro autorizado → caso y evidencia. La matriz del §23 relaciona los 49 requisitos sin modificar su definición. C §§2–3 y 11 rige la clasificación y evolución documental.
- **Riesgos:** Fuentes divergentes, requisitos omitidos, atribución de aprobación inexistente o evidencia sin relación con el alcance.
- **Estado de aprobación:** Trazabilidad exigida en origen; mecanismo de mantenimiento propuesto, pendiente de aprobación.

Para una modificación futura se identificará primero el FND afectado, después la decisión y su aprobación, y finalmente el cambio y la evidencia. Si la propuesta altera el qué o un criterio de aceptación de `spec.md`, se solicitará autorización para revisar esa fuente; no se alterará mediante este plan. La ausencia de evidencia significa pendiente, no cumplimiento.

## 3. Plataforma ASP.NET Core y .NET 10

### PLD-003 — Plataforma y dependencias mínimas

- **Requisitos relacionados:** FND-004, FND-036, FND-041.
- **Alternativas:** Conservar la plataforma confirmada con dependencias mínimas; añadir una plantilla extensa y paquetes preventivos; cambiar plataforma.
- **Propuesta justificada:** Conservar C#, ASP.NET Core 10 y .NET 10; evaluar primero capacidades de plataforma antes de proponer paquetes. Antes de incorporar una dependencia, documentar necesidad, compatibilidad, licencia, mantenimiento, versión y aprobación. Los paquetes presentes en la plantilla son observaciones, no aprobación automática de futuras selecciones.
- **Riesgos:** Versiones incompatibles, parches sin mantenimiento, paquetes innecesarios o fijación tácita de la versión del lenguaje.
- **Estado de aprobación:** Plataforma confirmada por C §2.1 y FND-004; parches, versión concreta de C# y dependencias complementarias pendientes. La política de incorporación es propuesta.

La futura construcción deberá hacer reproducibles los metadatos y versiones aprobados y distinguir SDK de construcción, runtime y versión de aplicación. No se instala, actualiza ni fija ninguno desde este documento.

## 4. Propuesta de API REST modular

### PLD-004 — Estilo de exposición y modularidad

- **Requisitos relacionados:** FND-002, FND-003, FND-005, FND-030, FND-031, FND-040.
- **Alternativas:** API REST modular en un único despliegue; API sin límites modulares explícitos; servicios distribuidos independientes.
- **Propuesta justificada:** Evaluar como primera opción una API REST modular con un único artefacto desplegable en varias instancias. Separar responsabilidades lógicas, no introducir microservicios ni comunicación interna de red sin necesidad. Mantener pendientes los mecanismos de exposición HTTP y la aprobación de REST, conforme a C §2.3.
- **Riesgos:** Convertir módulos en capas artificiales, anticipar límites de negocio o confundir modularidad con múltiples servicios.
- **Estado de aprobación:** Propuesta pendiente; FND-005 continúa en estado P.

La construcción posterior partiría de capacidades técnicas transversales y contratos aprobados. Los límites funcionales se definirían solo mediante especificaciones futuras de negocio, no en este plan.

## 5. Comparación de arquitectura por capas y Clean Architecture

| Criterio | Capas convencionales | Clean Architecture proporcionada |
| --- | --- | --- |
| Organización | Presentación, aplicación o negocio y datos | Responsabilidades internas independientes de adaptadores y transporte |
| Dependencias | Puede ser descendente; debe impedir que las reglas dependan de detalles de datos | Las dependencias de código apuntan a políticas y contratos interiores |
| Coste inicial | Menor si los límites son pequeños y explícitos | Mayor si se crean proyectos e interfaces sin necesidad |
| Testabilidad | Adecuada si hay inversión de dependencias | Favorece aislamiento de políticas y sustitución de adaptadores |
| NoSQL y Redis | Riesgo de filtrar modelos y clientes desde la capa de datos | Detalles externos encapsulados detrás de límites justificados |
| Evolución | Sencilla para poco alcance, pero puede degradarse en acoplamiento | Requiere disciplina y evitar abstracciones especulativas |
| Cumplimiento SOLID | Depende del diseño, no del nombre del patrón | Depende del diseño, no de crear cuatro proyectos |
| Riesgo académico | Presentar carpetas como prueba de arquitectura | Sobrediseñar la plantilla o inventar un dominio todavía no especificado |

### PLD-005 — Patrón arquitectónico propuesto

- **Requisitos relacionados:** FND-002, FND-003, FND-005, FND-034.
- **Alternativas:** Capas con inversión de dependencias; Clean Architecture proporcionada; organización sin límites explícitos.
- **Propuesta justificada:** Recomendar para evaluación una Clean Architecture ligera centrada en la dirección de dependencias, no en un número obligatorio de proyectos. Mantener capas con inversión de dependencias como alternativa válida si satisface los mismos criterios con menor coste. La comparación y aceptación final corresponden al responsable, no al nombre del rol solicitado.
- **Riesgos:** Adoptar Clean Architecture como decisión implícita, crear interfaces sin consumidor o inventar entidades para llenar un proyecto de dominio.
- **Estado de aprobación:** Propuesta pendiente, vinculada a DEC-02. No hay patrón definitivo aprobado.

## 6. Separación de responsabilidades y dependencias

### PLD-006 — Dirección de dependencias y composición

- **Requisitos relacionados:** FND-002, FND-003, FND-006, FND-015, FND-016, FND-030, FND-034.
- **Alternativas:** Reglas acopladas directamente al host y clientes externos; contratos de aplicación con adaptadores externos; abstracciones genéricas universales.
- **Propuesta justificada:** Si se aprueba el enfoque ligero, ubicar políticas y contratos interiores sin referencias a HTTP ni clientes NoSQL o Redis. Los adaptadores dependerían de esos contratos; el host compondría implementaciones aprobadas. Evitar repositorios genéricos o mediación obligatoria sin un caso que los justifique. C §4 establece el principio, no una biblioteca.
- **Riesgos:** Dependencias circulares, modelos de datos filtrados al exterior, host convertido en contenedor de reglas o abstracciones que oculten garantías importantes del motor.
- **Estado de aprobación:** Separación confirmada en origen; organización y mecanismos concretos propuestos, pendientes.

Flujo conceptual posterior: transporte recibe y valida el contexto HTTP; aplicación coordina una capacidad técnica autorizada; infraestructura adapta el acceso externo. La dirección del flujo de ejecución no invierte la dirección de las referencias de código. Solo la composición del host podría conocer adaptadores concretos; ello no permite introducir reglas en el host.

## 7. Organización conceptual de proyectos y carpetas

### PLD-007 — Estructura mínima y ampliable

- **Requisitos relacionados:** FND-002, FND-003, FND-004, FND-005, FND-034, FND-035.
- **Alternativas:** Un proyecto con límites internos; separación en host, aplicación e infraestructura; cuatro proyectos obligatorios más proyectos por módulo.
- **Propuesta justificada:** Evaluar host existente y separación de aplicación e infraestructura cuando el tamaño y los límites la justifiquen. Un dominio separado se incorporaría solo si futuras reglas de negocio aprobadas lo necesitan; no se crea vacío para cumplir una etiqueta. Mantener la solución de Visual Studio y el proyecto `Enver.API` existentes.
- **Riesgos:** Crear estructura prematura, migrar archivos sin permiso, acoplar nombres de proyectos a decisiones no aprobadas o aumentar el coste de mantenimiento sin beneficio.
- **Estado de aprobación:** Propuesta conceptual pendiente; ningún proyecto o carpeta se crea por este plan.

| Nombre conceptual, no creado | Responsabilidad futura posible | Referencias conceptuales permitidas si se aprueba |
| --- | --- | --- |
| `Enver.API`, existente | Host, transporte y composición; configuración transversal | Aplicación y adaptadores solo para composición |
| `Enver.Application`, propuesto | Políticas técnicas y contratos interiores que estén justificados | Dominio únicamente si después existe y se aprueba |
| `Enver.Infrastructure`, propuesto | Adaptadores de NoSQL, Redis y servicios externos aprobados | Contratos interiores; no dependencia hacia el host |
| `Enver.Domain`, opcional futuro | Reglas de dominio aún no especificadas | Sin dependencias de HTTP o infraestructura; excluido de construcción ahora |
| Proyectos o agrupaciones de pruebas, por decidir | Verificación unitaria, integración y seguridad aisladas | Componentes bajo prueba según el nivel de verificación |

Si se mantiene un único proyecto, los mismos límites se expresarían en carpetas conceptuales de transporte, aplicación e infraestructura y se revisarían explícitamente. En ninguna opción se crean aquí controladores, servicios, entidades, DTOs o carpetas de módulos de negocio.

## 8. Configuración y ambientes

### PLD-008 — Configuración tipada, validada y aislada

- **Requisitos relacionados:** FND-004, FND-008, FND-012, FND-016, FND-019, FND-032, FND-041.
- **Alternativas:** Valores embebidos; archivos por ambiente con secretos; configuración no secreta versionada y valores externos protegidos, con validación antes de admitir tráfico.
- **Propuesta justificada:** Separar desarrollo, pruebas y entorno publicado o equivalente académico; definir un catálogo de configuración con obligatoriedad, origen, precedencia y sensibilidad. Validar coherencia y límites al iniciar, y aislar persistencia, Redis, telemetría y credenciales por ambiente. Preferir capacidades de configuración de plataforma cuando cubran la necesidad.
- **Riesgos:** Precedencia inesperada, conexión accidental a otro ambiente, diferencias entre instancias o diagnósticos que revelen secretos.
- **Estado de aprobación:** Aislamiento exigido; catálogo, precedencia, proveedores de configuración y reglas concretas pendientes de DEC-14.

La construcción futura partiría del catálogo aprobado, no de valores inventados. Los valores imprescindibles inválidos impedirían admitir tráfico. La validación de configuración no deberá convertirse en dependencia indiscriminada de red durante cada comprobación de salud. No se genera archivo de configuración ni se modifica uno existente.

## 9. Seguridad, HTTPS, JWT, autorización y secretos

### PLD-009 — Transporte y fronteras de confianza

- **Requisitos relacionados:** FND-008, FND-013, FND-020, FND-030, FND-041, FND-045.
- **Alternativas:** Terminación TLS solo en la API; terminación en balanceador y reencriptación interna; terminación en balanceador con tramo interno protegido mediante una decisión de riesgo explícita.
- **Propuesta justificada:** Preferir HTTPS público y evaluar reencriptación hacia la API. Si se propone otra protección interna, documentar frontera, aislamiento y riesgo antes de aceptarla. Limitar la confianza en cabeceras reenviadas a proxies aprobados y evitar que influyan en seguridad o cuotas desde un origen no confiable.
- **Riesgos:** Bucles de redirección, cabeceras falsificadas, certificado inválido o exposición del tramo interno.
- **Estado de aprobación:** HTTPS confirmado; topología TLS, política y terminación propuestas, pendientes de DEC-04.

### PLD-010 — Identidad JWT y autorización diferenciadas

- **Requisitos relacionados:** FND-009, FND-010, FND-012, FND-013, FND-030, FND-035, FND-045.
- **Alternativas:** JWT emitido por un proveedor de identidad aprobado; emisión propia expresamente diseñada y aprobada; autenticación asumida suficiente sin políticas de acceso.
- **Propuesta justificada:** Evaluar primero un emisor de confianza con capacidades de rotación, sin seleccionarlo. La API actuaría como validador de identidad y aplicaría políticas independientes de autorización con denegación por defecto. Documentar firma, algoritmos, emisor, audiencia, vigencia, tolerancia temporal, claves y comportamiento ante fallos de actualización de claves. No se propone crear endpoints de emisión ni elegir roles funcionales.
- **Riesgos:** JWT válido usado fuera de su ámbito, confianza en claims no controlados, rotación que degrade seguridad o emisión propia con complejidad innecesaria.
- **Estado de aprobación:** JWT y mínimo privilegio requeridos; emisor, políticas y parámetros pendientes de DEC-06.

### PLD-011 — Validación, secretos y revisión OWASP

- **Requisitos relacionados:** FND-011, FND-012, FND-013, FND-025, FND-031, FND-035, FND-036, FND-044, FND-045.
- **Alternativas:** Validación dispersa y secretos en archivos; controles en fronteras de confianza y gestión externa; delegar toda seguridad a un balanceador.
- **Propuesta justificada:** Aplicar validación estructural y límites al entrar, sin confundirla con reglas de negocio. Excluir secretos del repositorio y artefactos; evaluar identidades de ejecución y permisos mínimos para recuperarlos y rotarlos. Relacionar cada categoría de la edición OWASP aprobada con riesgo, control, prueba o justificación de no aplicabilidad. Reservar la denegación segura para fallos de controles que no pueden relajarse.
- **Riesgos:** Propiedades no permitidas, tamaños ilimitados, secretos en telemetría o falsa cobertura OWASP por una lista de herramientas.
- **Estado de aprobación:** Obligaciones confirmadas; límites, almacén, edición OWASP y controles concretos pendientes de DEC-08, DEC-12 y DEC-14.

La construcción posterior incorporaría primero los límites aprobados y sus casos negativos, después comprobaría que también cubren errores y rechazos. Ningún valor de credencial se incluye en ejemplos, registros o evidencias.

## 10. Estrategia NoSQL, índices y consistencia

### PLD-012 — Selección guiada por garantías y acceso

- **Requisitos relacionados:** FND-003, FND-014, FND-015, FND-023, FND-024, FND-028, FND-034, FND-047.
- **Alternativas:** Familias documentales, clave–valor o de columnas anchas según necesidades aprobadas; servicio administrado o autogestionado; seleccionar un producto antes de conocer garantías y patrones.
- **Propuesta justificada:** Comparar motores NoSQL mediante una ficha de capacidades: consultas, índices, particionamiento, atomicidad, control de concurrencia, consistencia, respaldo, coste, seguridad y compatibilidad .NET. Elegir solo tras aprobación. Construir después el adaptador mínimo y sus verificaciones; diseñar índices únicamente para patrones autorizados, evitando modelos funcionales inventados o una abstracción que oculte restricciones del motor.
- **Riesgos:** Presuponer transacciones o unicidad global, exploraciones costosas, índices excesivos, dependencia del proveedor o recuperación imposible con el presupuesto previsto.
- **Estado de aprobación:** Uso de NoSQL confirmado; motor, servicio, modelo, índices y garantías pendientes de DEC-03. No se elige MongoDB ni otro motor.

La ficha se completaría antes del adaptador. Para cada patrón futuro se relacionarían consulta, orden estable, cardinalidad, índice, coste y garantía de consistencia. Las mediciones de acceso y restauración se realizarían solo después de autorizar entornos y procedimientos.

## 11. Redis y caché distribuida

### PLD-013 — Caché compartida con coherencia explícita

- **Requisitos relacionados:** FND-006, FND-016, FND-017, FND-018, FND-021, FND-022, FND-028, FND-034, FND-044, FND-045.
- **Alternativas:** Redis compartido con lectura de origen ante ausencia; actualización anticipada de caché; invalidación por versiones o generaciones; memoria local como única caché o estado compartido implícito.
- **Propuesta justificada:** Evaluar como punto de partida lectura desde caché con recurso al origen y poblamiento posterior, solo para categorías elegibles. Combinar expiración, invalidación y control de versiones o mecanismo equivalente cuando sea necesario para evitar repoblación obsoleta. Definir claves con ambiente, ámbito de acceso y versión contractual. No asumir que invalidar después de escribir elimina todas las carreras; si la garantía no puede demostrarse, excluir esa categoría de caché.
- **Riesgos:** Obsolescencia, avalancha de lecturas al origen, datos de otra identidad, Redis convertido en fuente duradera o cuotas desactivadas durante fallos.
- **Estado de aprobación:** Redis distribuido requerido; servicio, cliente, elegibilidad, expiración, obsolescencia máxima, mecanismo de coherencia y política de fallo pendientes de DEC-07 y DEC-08.

La futura construcción separaría usos de optimización de los controles distribuidos de seguridad. Para caché de rendimiento podría admitirse degradación acotada al origen si su capacidad lo permite; un control de autorización o cuota no perdería su garantía silenciosamente. Las carreras entre escritura, invalidación y repoblación se incluirían como casos de aceptación desde dos instancias. No se proponen TTL numéricos sin aprobación.

## 12. Dos instancias cloud con balanceador

### PLD-014 — Topología sin afinidad obligatoria

- **Requisitos relacionados:** FND-006, FND-007, FND-008, FND-019, FND-020, FND-027, FND-039, FND-043.
- **Alternativas:** Dos instancias equivalentes tras balanceador; procesos duplicados dependientes de sesión local; topología multirregional.
- **Propuesta justificada:** Planificar al menos dos instancias independientes del host, un balanceador y dependencias compartidas protegidas. Mantener fuera de cada instancia el estado imprescindible y no depender de afinidad de sesión. Definir señales distintas para proceso vivo y aptitud de tráfico, retirada controlada y capacidad de la instancia superviviente. No inferir independencia regional o de zona por duplicar procesos.
- **Riesgos:** Punto único de fallo en dependencias, capacidad insuficiente al perder una instancia o retirada simultánea por una política de salud demasiado agresiva.
- **Estado de aprobación:** Topología mínima requerida; servicio, dominios de fallo, algoritmo, salud, drenaje y temporización pendientes de DEC-04 y DEC-15.

La construcción posterior distinguiría caída del proceso, incapacidad local y fallo común de dependencia; no reiniciaría todas las instancias indiscriminadamente por un fallo compartido. Se recogería identidad de instancia en la telemetría para comprobar distribución y exclusión. No se define una ruta técnica ni se configura el balanceador ahora.

## 13. Evaluación conceptual de AWS, Azure y GCP

Esta comparación es conceptual: no certifica soporte vigente de servicios concretos para .NET 10 ni incluye precios, cuotas, regiones o niveles de servicio verificados. Esos datos requerirán consulta posterior autorizada antes de elegir.

| Aspecto a evaluar | AWS | Azure | GCP |
| --- | --- | --- | --- |
| Ejecución de la API | Comparar máquinas virtuales y servicios de contenedores administrados | Comparar máquinas virtuales, servicios de aplicaciones y contenedores administrados | Comparar máquinas virtuales y servicios de contenedores administrados |
| Dos instancias y balanceo | Confirmar control del mínimo de instancias, salud y retirada en el servicio candidato | Confirmar equivalencia entre instancias declaradas, unidades de escalado y distribución real | Confirmar mínimo de instancias activas, comportamiento de escalado y balanceo real |
| .NET 10 | Verificar runtime o imagen aprobada, ciclo de soporte y restricciones del servicio | Verificar runtime o imagen aprobada; afinidad con ecosistema .NET no demuestra superioridad técnica | Verificar runtime o imagen aprobada, ciclo de soporte y restricciones del servicio |
| Dependencias | Evaluar NoSQL aprobado, Redis compatible, redes e identidad disponibles | Evaluar NoSQL aprobado, Redis compatible, redes e identidad disponibles | Evaluar NoSQL aprobado, Redis compatible, redes e identidad disponibles |
| Coste académico | Estimar cómputo mínimo, balanceo, datos, Redis, registros, transferencia y recuperación | Aplicar el mismo perfil y no asumir créditos o gratuidad | Aplicar el mismo perfil y no asumir créditos o gratuidad |
| Operación e IaC | Revisar permisos, observabilidad, despliegue, estado y reproducibilidad | Revisar los mismos criterios con recursos equivalentes | Revisar los mismos criterios con recursos equivalentes |
| Riesgo | Dependencia de servicios y complejidad de red o identidad | Dependencia de servicios y coste operativo de la configuración elegida | Dependencia de servicios y restricciones del modelo de ejecución elegido |

### PLD-015 — Selección cloud por evidencia comparable

- **Requisitos relacionados:** FND-004, FND-008, FND-012, FND-014, FND-016, FND-019, FND-020, FND-033, FND-038, FND-041, FND-042, FND-043, FND-047.
- **Alternativas:** AWS, Azure o GCP; dentro de cada uno, ejecución administrada o máquinas virtuales según control y coste.
- **Propuesta justificada:** No declarar ganador ahora. Aprobar primero criterios y ponderaciones de coste total, capacidades .NET 10, dos instancias efectivas, seguridad, dependencias y facilidad de recuperación. Comparar el mismo perfil en los tres proveedores y registrar evidencia de capacidades y restricciones antes de elevar una recomendación. Un modelo que escale a cero o no permita demostrar la topología mínima no se considerará equivalente sin justificación aprobada.
- **Riesgos:** Comparación con perfiles distintos, confundir soporte de contenedor con soporte de un runtime administrado, créditos asumidos o selección por familiaridad sin capacidad suficiente.
- **Estado de aprobación:** Evaluación propuesta pendiente; proveedor, región, servicios, presupuesto y ponderaciones sin aprobar, conforme a DEC-04.

## 14. Resiliencia, concurrencia e idempotencia

### PLD-016 — Presupuestos y aislamiento de fallos

- **Requisitos relacionados:** FND-007, FND-018, FND-020, FND-027, FND-039, FND-042, FND-043, FND-044, FND-047.
- **Alternativas:** Esperas y reintentos sin límite; presupuestos globales y por dependencia con degradación controlada; protección e aislamiento adicionales cuando el riesgo lo justifique.
- **Propuesta justificada:** Derivar del presupuesto total los tiempos de dependencia, intentos y márgenes; propagar cancelación segura y reintentar solo errores transitorios de operaciones repetibles. Evaluar protección de circuitos, aislamiento y límites de concurrencia según los fallos esperados, no como paquetes obligatorios. Diferenciar caída de instancia, Redis y persistencia, y la recuperación de datos de la simple sustitución de un proceso.
- **Riesgos:** Reintentos multiplicados entre capas, tormentas de carga, bloqueo de recursos o aceptación de objetivos de recuperación no costeados.
- **Estado de aprobación:** Resiliencia requerida; presupuestos, mecanismos, respaldo y objetivos cuantitativos pendientes de DEC-10, DEC-15 y DEC-16.

### PLD-017 — Control de conflictos y repetición segura

- **Requisitos relacionados:** FND-015, FND-017, FND-027, FND-028, FND-029, FND-035, FND-045.
- **Alternativas:** Control optimista basado en versión si el motor lo soporta; exclusión coordinada con garantías demostradas cuando sea necesaria; sobrescritura sin detección de conflicto; deduplicación solo en memoria local.
- **Propuesta justificada:** Evaluar primero concurrencia optimista sobre capacidades reales del motor y rechazo explícito de conflictos. Para operaciones que después requieran idempotencia, definir identidad, alcance, vigencia y resultado repetido, y vincular el registro de deduplicación al efecto con una garantía verificable. No usar un bloqueo Redis aislado como prueba suficiente de atomicidad con NoSQL ni prometer exactamente una vez.
- **Riesgos:** Actualización perdida, bloqueo caducado mientras sigue el efecto, duplicados entre instancias o caída entre efecto y registro del resultado.
- **Estado de aprobación:** Tratamiento requerido; mecanismos, operaciones aplicables, consistencia y ventanas pendientes de DEC-03 y DEC-10.

La construcción futura partiría de una tabla por operación técnica autorizada: garantía de lectura, límite de atomicidad, conflicto esperado, posibilidad de reintento y obligación de idempotencia. Si no puede garantizarse un resultado, se explicitaría la limitación y se revisaría el criterio antes de implementar; no se introduciría una solución aparente que oculte el riesgo.

## 15. Contratos HTTP, errores, versionamiento y paginación

### PLD-018 — Contratos previsibles y acotados

- **Requisitos relacionados:** FND-003, FND-005, FND-011, FND-023, FND-025, FND-030, FND-031, FND-040, FND-044.
- **Alternativas:** Formatos ad hoc; representación de error interoperable compatible con estándares HTTP; envoltorio uniforme para toda respuesta sin considerar semántica.
- **Propuesta justificada:** Evaluar un formato de error HTTP estándar, con código estable, mensaje seguro y correlación; no imponer un envoltorio global de éxito ni cuerpos en respuestas que no los admiten. Definir convenciones JSON y tamaños antes del transporte concreto. Evaluar paginación por cursor para recorridos grandes o cambiantes y por desplazamiento cuando su coste y estabilidad sean aceptables; decidir tras patrones e índices aprobados. La política de versión se resuelve en §21, sin fijar rutas de negocio.
- **Riesgos:** Convertir errores en éxito HTTP, exponer modelos internos, perder precisión JSON, paginación inestable o compatibilidad rota.
- **Estado de aprobación:** Contratos y límites requeridos; formato, serializador, paginación, tamaños y versionado concretos pendientes de DEC-09.

### PLD-019 — Procesamiento transversal y rendimiento seguro

- **Requisitos relacionados:** FND-008, FND-009, FND-010, FND-011, FND-017, FND-018, FND-021, FND-022, FND-025, FND-026, FND-030, FND-031, FND-042, FND-044, FND-045.
- **Alternativas:** Controles locales dispersos; política transversal con límites previos a autenticación y cuotas por identidad; limitación global en balanceador o coordinada entre instancias; compresión indiscriminada o condicionada.
- **Propuesta justificada:** Diseñar primero las dependencias de orden: aceptar información de proxy solo tras validar confianza; proteger tamaños y abuso antes de trabajo costoso; autenticar antes de autorizar y de aplicar cuotas ligadas a identidad; cubrir con correlación y errores los rechazos gestionados por la API. Evaluar coordinación atómica de cuotas globales en un punto aprobado, no multiplicarlas por instancia. Aplicar caché y compresión solo donde elegibilidad, negociación y aislamiento estén probados.
- **Riesgos:** Omitir controles por orden incorrecto, falsificar identidad de consumo, usar un limitador local como cuota global, comprimir secretos o prometer contratos de errores para fallos generados fuera de la API.
- **Estado de aprobación:** Capacidades requeridas; orden final, punto de cuotas, tolerancia, algoritmos y umbrales pendientes de DEC-07, DEC-08 y DEC-09.

En la futura construcción se revisarían diagramas de procesamiento y casos de rechazo antes de elegir componentes. Los errores emitidos por balanceador o plataforma se distinguirían de los emitidos por la API y tendrían una política de compatibilidad explícita. No se prescribe aquí código, llamadas de middleware o secuencia de configuración ejecutable.

## 16. Logging, métricas y observabilidad

### PLD-020 — Telemetría correlacionada y minimizada

- **Requisitos relacionados:** FND-007, FND-020, FND-021, FND-022, FND-027, FND-032, FND-033, FND-041, FND-042, FND-043, FND-046.
- **Alternativas:** Registros locales sin estructura; registros estructurados y métricas centralizadas con trazas distribuidas; captura exhaustiva de cuerpos y etiquetas sin límites.
- **Propuesta justificada:** Definir un vocabulario estable de eventos y dimensiones acotadas: ambiente, versión, instancia y correlación. Medir tasa, latencia, errores, saturación, salud y dependencias, distinguiendo caché, cuotas y fallos. Evaluar convenciones interoperables para propagación y exportación, sin seleccionar librería o plataforma. Separar cobertura de correlación del muestreo de trazas y tratar identificadores entrantes como datos no confiables.
- **Riesgos:** Costes de cardinalidad o retención, datos sensibles en eventos, pérdida durante caídas o una métrica agregada que oculte una instancia defectuosa.
- **Estado de aprobación:** Observabilidad requerida; plataforma, esquema final, acceso, retención, muestreo y cobertura propuesta pendientes de DEC-11 y DEC-15.

La construcción posterior incorporaría correlación antes de integrar dependencias y definiría alertas con contexto y responsable. La evidencia de FND-046 explicitaría pérdidas ante caída abrupta y no las ocultaría como cobertura total.

## 17. Pruebas unitarias, integración y seguridad

### PLD-021 — Verificación por requisito y riesgo

- **Requisitos relacionados:** FND-002, FND-003, FND-007, FND-009, FND-010, FND-013, FND-017, FND-018, FND-022, FND-027, FND-028, FND-029, FND-034, FND-035, FND-042, FND-043, FND-044, FND-045, FND-046, FND-047, FND-048.
- **Alternativas:** Solo pruebas unitarias; solo pruebas integrales; niveles complementarios con evidencia vinculada a los FND y objetivos autorizados.
- **Propuesta justificada:** Diseñar casos antes o junto al componente futuro: unitarios para políticas aislables; integración para HTTP y adaptadores con dependencias compatibles; seguridad para casos negativos y aislamiento; escenarios distribuidos para coherencia, cuotas, fallos, concurrencia e idempotencia. Reservar carga y restauración a entornos y presupuestos aprobados. Una simulación unitaria no demostraría garantías del motor ni del balanceo.
- **Riesgos:** Falsa confianza por sustitutos, pruebas sobre producción o datos sensibles, casos no trazados y cobertura numérica sin relevancia del riesgo.
- **Estado de aprobación:** Niveles de verificación requeridos; herramientas, entornos, umbrales, datos y ejecución pendientes de DEC-12, DEC-15 y DEC-16.

| Nivel futuro | Enfoque conceptual | Evidencia prevista, no generada |
| --- | --- | --- |
| Unitario | Aislar políticas y conversiones técnicas justificadas | Caso, FND, resultado y limitaciones del aislamiento |
| Integración | Verificar transporte, contratos y adaptadores reales o equivalentes justificados | Versiones de dependencias, ambiente y resultados |
| Seguridad | Rechazos JWT, permisos, validación, secretos, aislamiento y abuso | Casos negativos y tratamiento de hallazgos |
| Distribuido y fallos | Alternar instancias, perder una dependencia, provocar conflictos y duplicados | Tiempos, efectos, consistencia y garantías observadas |
| Rendimiento y recuperación | Medir perfil aprobado y restaurar datos en ambiente aislado | Muestra, recursos, percentiles, RTO, RPO y alcance real |

Este plan no crea proyectos de pruebas, casos ejecutables ni scripts y no realiza ensayos.

## 18. DevSecOps y análisis estático

### PLD-022 — Controles de calidad y cadena de suministro

- **Requisitos relacionados:** FND-004, FND-012, FND-013, FND-034, FND-035, FND-036, FND-037, FND-045, FND-048.
- **Alternativas:** Revisiones manuales sin criterios de bloqueo; controles automatizados complementados por revisión; incorporar muchas herramientas sin política de resultados.
- **Propuesta justificada:** Definir controles por riesgo: análisis estático, dependencias directas y transitivas, licencias, secretos y revisión de artefactos o contenedores. Aprobar primero severidades bloqueantes, tratamiento de falsos positivos y excepciones con responsable y vencimiento. Relacionar herramienta, versión, fuente analizada y evidencia; evitar que la presencia del control se declare equivalente a seguridad acreditada.
- **Riesgos:** Herramientas desactualizadas, ruido ignorado, excepciones permanentes, secretos en informes o conclusiones de vulnerabilidad sin evidencia suficiente.
- **Estado de aprobación:** Controles previstos en origen; herramientas, licencias, política de bloqueo y excepciones pendientes de DEC-05 y DEC-12.

La construcción posterior empezaría por la política de calidad y evidencia antes de automatizarla. No se añaden paquetes, analizadores o configuraciones en esta intervención.

## 19. CI/CD y controles de aprobación

### PLD-023 — Promoción de artefacto y autorización operativa

- **Requisitos relacionados:** FND-001, FND-004, FND-012, FND-019, FND-020, FND-034, FND-035, FND-036, FND-037, FND-039, FND-041, FND-048.
- **Alternativas:** Despliegue manual no trazado; automatización sin puertas; promoción controlada del mismo artefacto con aprobación por ambiente.
- **Propuesta justificada:** Diseñar una cadena futura de revisión de fuente, verificaciones autorizadas, generación de artefacto identificable, evaluación de controles y promoción aprobada. Preferir el mismo artefacto verificado entre ambientes, inyectando configuración y secretos sin reconstruirlo para cada uno. Identificar quién aprueba, qué credenciales mínimas usa cada etapa y cómo se detiene o revierte una entrega fallida.
- **Riesgos:** Ejecutar código no confiable con secretos, conceder permisos excesivos, confundir una aprobación documental con permiso de despliegue o promover un artefacto distinto del verificado.
- **Estado de aprobación:** Entrega controlada requerida; plataforma, etapas concretas, identidades y permisos pendientes de DEC-13.

Las puertas futuras separarían revisión de código y dependencias, aceptación en entorno de ensayo y publicación. La estrategia de retirada de instancias y coexistencia de versiones se coordinaría con PLD-014 y PLD-025. La reversión de aplicación no se consideraría automáticamente reversión segura de datos. No se crea ni ejecuta una canalización.

## 20. Infraestructura como código

### PLD-024 — Recursos reproducibles y estado protegido

- **Requisitos relacionados:** FND-008, FND-012, FND-016, FND-019, FND-020, FND-037, FND-038, FND-039, FND-041, FND-047.
- **Alternativas:** Configuración manual como única fuente; herramienta declarativa ligada al proveedor; herramienta declarativa multiproveedor con alcance explícito.
- **Propuesta justificada:** Seleccionar después de DEC-04 una herramienta acorde con recursos, conocimientos y presupuesto aprobados. Representar la topología mínima y diferencias no secretas entre ambientes en definiciones revisables; proteger estado, identidades y salidas sensibles cuando existan. Revisar el cambio previsto antes de aplicar y registrar desviaciones, sin asumir que una herramienta multiproveedor elimina dependencia del proveedor.
- **Riesgos:** Secretos en estado o salidas, recursos destruidos accidentalmente, deriva manual, costes no controlados o una reversión de infraestructura incompatible con persistencia.
- **Estado de aprobación:** IaC contemplada por FND-038; herramienta, recursos, almacenamiento de estado y permisos pendientes de DEC-13 y DEC-14.

La construcción futura relacionaría cada recurso con un requisito y una decisión cloud aprobada. La reproducción se comprobaría solo en un entorno aislado autorizado y con controles de coste. No se generan archivos IaC ni se configura o aprovisiona infraestructura.

## 21. Versionamiento

### PLD-025 — Versiones de contrato, artefacto y documento

- **Requisitos relacionados:** FND-003, FND-005, FND-017, FND-025, FND-031, FND-037, FND-039, FND-040, FND-041, FND-048.
- **Alternativas:** Versión de API por ruta, cabecera o negociación; cambios incompatibles sin versión; versión única confundida entre API, despliegue y documento.
- **Propuesta justificada:** Aprobar explícitamente un mecanismo de versión pública tras evaluar consumidores y contratos; mientras tanto, no fijar rutas ni paquetes. Distinguir versión de API, identificador inmutable del artefacto, compatibilidad de datos y versión documental. Clasificar cambios incompatibles, período de convivencia y retirada, y considerar su impacto sobre claves de caché y reversión.
- **Riesgos:** Clientes rotos, caché reutilizada entre contratos incompatibles, despliegue mixto no probado o reversión que requiera un esquema de datos ya irreversible.
- **Estado de aprobación:** Versionamiento requerido; mecanismo, política y períodos pendientes de DEC-09 y DEC-13. La versión 0.1.0 de este plan no aprueba una versión de API.

La construcción posterior verificaría compatibilidad antes de promover versiones coexistentes. Ningún consumidor, contrato o período de soporte se inventa para cerrar la decisión.

## 22. Riesgos y decisiones pendientes

### PLD-026 — Ratificación de objetivos y parámetros

- **Requisitos relacionados:** FND-007, FND-011, FND-017, FND-022, FND-027, FND-033, FND-041, FND-042, FND-043, FND-044, FND-045, FND-046, FND-047.
- **Alternativas:** Adoptar los números propuestos como SLA; ajustarlos por intuición al redactar el plan; conservarlos como candidatos y ratificarlos con perfil, presupuesto y ambiente.
- **Propuesta justificada:** Preservar literalmente los candidatos de `spec.md` y sus estados de propuesta. Someterlos a aprobación conjunta con carga, recursos, datos sintéticos, tiempos de dependencia y costes. Los límites aún no definidos siguen abiertos; no se considera aceptado un requisito hasta disponer de parámetros aprobados y evidencia autorizada.
- **Riesgos:** Prometer capacidad sin entorno definido, ocultar pérdidas de telemetría, confundir fallo de instancia con pérdida de datos o modificar criterios de la fuente de forma implícita.
- **Estado de aprobación:** Ratificación pendiente de DEC-08, DEC-10, DEC-11, DEC-15 y DEC-16. No hay mediciones ni objetivos nuevos aprobados.

| Requisito | Valor o condición conservada de `spec.md` | Estado y condición previa |
| --- | --- | --- |
| FND-042 | 50 solicitudes válidas/s; 10 minutos tras 2 de estabilización; p95 ≤ 500 ms; p99 ≤ 1.000 ms; fallos ≤ 1 %; cuerpos de hasta 16 KiB; aciertos/fallos de caché 50/50 cuando corresponda | Propuesta; definir y aprobar mezcla, operaciones de ensayo, recursos, región, volumen y cuotas. Medir desde cliente a través del balanceador y mantener la definición de fallos de la fuente. |
| FND-043 | Exclusión de una instancia no apta ≤ 30 s; después, 100 % de nuevas solicitudes admitidas a instancias aptas durante 10 minutos | Propuesta; aprobar temporización y capacidad de supervivencia. Registrar fallos en curso; no implica SLA mensual. |
| FND-044 | 100 % de los casos de exceso reciben tratamiento acordado; límites y tolerancia todavía sin números | Obligación confirmada; cuantificar tamaños, cuotas, concurrencia y presupuesto total antes de aceptar técnicamente. |
| FND-045 | Cero accesos no autorizados y cero exposición de secretos en el conjunto aprobado | Obligación confirmada; definir conjunto de seguridad, entorno y alcance. No prueba ausencia universal de vulnerabilidades. |
| FND-046 | 100 % de correlación en el conjunto de ensayo, separada del muestreo; pérdidas abruptas declaradas | Concreción propuesta; ratificar cobertura y garantía de pérdida antes de medir. |
| FND-047 | RTO ≤ 30 minutos; RPO ≤ 15 minutos ante escenario aprobado de pérdida de persistencia académica | Propuesta; aprobar respaldo, integridad, alcance del incidente y costes. No extrapolar a pérdida regional. |

### Registro de riesgos del plan

Los códigos RPL identifican riesgos, no requisitos adicionales, tareas o incidentes demostrados.

| Riesgo | Decisiones y requisitos afectados | Tratamiento propuesto y bloqueo |
| --- | --- | --- |
| RPL-01: gobierno aún registrado como Fase 0 y especificación no aprobada | PLD-001, PLD-002, PLD-027; FND-001, FND-048 | Delimitar este plan como autorización documental posterior. Bloquear implementación y solicitar aprobación y sincronización documental independiente. |
| RPL-02: sobrediseño o adopción implícita de Clean Architecture | PLD-004–PLD-007; FND-002–FND-005 | Comparar coste y límites antes de seleccionar; no crear un dominio o proyectos sin necesidad aprobada. |
| RPL-03: garantías de NoSQL incompatibles con concurrencia e idempotencia | PLD-012, PLD-017; FND-014, FND-015, FND-028, FND-029 | Bloquear adaptadores y garantías concretas hasta aprobar motor y alcance de atomicidad. |
| RPL-04: Redis como punto común de fallo o filtración | PLD-013, PLD-019; FND-016–FND-018, FND-021, FND-022, FND-045 | Separar optimización de controles obligatorios, aprobar coherencia, aislamiento y fallo seguro. |
| RPL-05: dos instancias sin alta disponibilidad real | PLD-014–PLD-016; FND-007, FND-019, FND-020, FND-043 | Declarar dominios de fallo, dependencias comunes y capacidad de una instancia; no prometer tolerancia regional. |
| RPL-06: confundir identidad y permisos | PLD-009–PLD-011; FND-008–FND-013, FND-045 | Exigir políticas y casos negativos por ámbito; no habilitar acceso por un JWT válido solamente. |
| RPL-07: límites o procesamiento HTTP incoherentes | PLD-018, PLD-019; FND-011, FND-022–FND-026, FND-030, FND-031, FND-044 | Revisar orden, cuotas agregadas, semántica HTTP y elegibilidad de compresión antes de implementar. |
| RPL-08: observabilidad sensible, costosa o incompleta | PLD-020, PLD-026; FND-032, FND-033, FND-046 | Aprobar campos, cardinalidad, retención y tratamiento de pérdidas; no registrar secretos. |
| RPL-09: controles o evidencia no reproducibles | PLD-021–PLD-024; FND-034–FND-039, FND-048 | Relacionar resultado con fuente, artefacto y ambiente; no atribuir verificación por presencia de herramientas. |
| RPL-10: cuotas cloud, coste y soporte no contrastados | PLD-015, PLD-024, PLD-026; FND-004, FND-019, FND-038, FND-042, FND-047 | Consultar posteriormente fuentes vigentes y aprobar presupuesto; no elegir proveedor por supuestos. |
| RPL-11: usar el plan como autorización de ejecución | PLD-001, PLD-027, PLD-028; FND-001, FND-049 | Mantener puertas separadas de aprobación y de autorización técnica; no crear `task.md` ni ejecutar nada. |

### Correspondencia con los pendientes existentes

| Pendiente de `spec.md` | Decisiones de este plan que lo desarrollan sin resolverlo | Resultado de aprobación todavía necesario |
| --- | --- | --- |
| DEC-01 | PLD-001, PLD-002, PLD-027, PLD-028 | Aprobación de fuentes, alcance y sincronización documental autorizada |
| DEC-02 | PLD-004, PLD-005, PLD-006, PLD-007 | Estilo de API, patrón, límites y organización final |
| DEC-03 | PLD-012, PLD-017 | Motor, capacidades, consultas, consistencia e índices |
| DEC-04 | PLD-009, PLD-014, PLD-015 | Proveedor, servicios, región, topología, red y TLS |
| DEC-05 | PLD-003, PLD-022 | Dependencias y versiones justificadas y aprobadas |
| DEC-06 | PLD-010, PLD-011 | Emisor, claves, parámetros y políticas de acceso |
| DEC-07 | PLD-013, PLD-019 | Redis, aislamiento, coherencia y degradación |
| DEC-08 | PLD-011, PLD-013, PLD-019, PLD-026 | Límites, cuotas globales y tolerancia aprobados |
| DEC-09 | PLD-018, PLD-019, PLD-025 | Contratos, JSON, errores, paginación, compresión y versión |
| DEC-10 | PLD-012, PLD-016, PLD-017, PLD-026 | Presupuestos y garantías de consistencia e idempotencia |
| DEC-11 | PLD-020, PLD-026 | Telemetría, retención, muestreo y correlación |
| DEC-12 | PLD-011, PLD-021, PLD-022 | Pruebas, herramientas, OWASP y criterios de calidad |
| DEC-13 | PLD-023, PLD-024, PLD-025 | CI/CD, IaC, promoción, compatibilidad y reversión |
| DEC-14 | PLD-008, PLD-011, PLD-024 | Ambientes, configuración, secretos y protección del estado |
| DEC-15 | PLD-014, PLD-020, PLD-021, PLD-026 | Perfil y objetivos cuantitativos ratificados |
| DEC-16 | PLD-012, PLD-016, PLD-021, PLD-024, PLD-026 | RTO, RPO, escenario y recuperación costeada |
| DEC-17 | PLD-001, PLD-002, PLD-027, PLD-028 | Autorización eventual de acceso mínimo para revisiones, si se solicita; sin cambios aquí |

## 23. Matriz de trazabilidad de requisitos

La matriz conserva individualmente los **49 IDs reales** y señala sus decisiones principales; las relaciones adicionales se detallan en el campo de requisitos de cada PLD. La columna C/P reproduce el estado de origen del requisito en `spec.md`, no una aprobación de la decisión PLD. **C** significa confirmado en origen con detalle pendiente; **P**, propuesta. Todas las evidencias técnicas de esta tabla son futuras y no ejecutadas.

| Requisito y tema en `spec.md` | Estado de origen | Decisiones principales relacionadas | Constitución canónica | Evidencia futura o tratamiento del alcance |
| --- | --- | --- | --- | --- |
| FND-001 — Gobierno de la especificación | C | PLD-001, PLD-002, PLD-027, PLD-028 | §§1–3, 9–11 | Alcance, decisiones y autorizaciones sin saltos de fase |
| FND-002 — Responsabilidades y SOLID | C | PLD-004, PLD-005, PLD-006, PLD-007, PLD-021 | §4 | Revisión de responsabilidades y dependencias |
| FND-003 — Límites y contratos internos | C | PLD-004, PLD-005, PLD-006, PLD-007, PLD-018, PLD-025 | §§4, 5.2 | Contratos sin dependencia de detalles externos |
| FND-004 — Plataforma tecnológica | C | PLD-003, PLD-007, PLD-008, PLD-015, PLD-022, PLD-023 | §§2.1, 2.4 | Compatibilidad y versiones aprobadas |
| FND-005 — Estilo REST modular | P | PLD-004, PLD-005, PLD-007, PLD-018, PLD-025 | §§2.3, 4 | Aceptación o rechazo explícito del estilo |
| FND-006 — Ausencia de estado local imprescindible | C | PLD-006, PLD-013, PLD-014 | §§2.2, 8 | Alternancia y reinicio de instancias sin pérdida de estado duradero |
| FND-007 — Escalabilidad horizontal | C | PLD-014, PLD-016, PLD-020, PLD-021, PLD-026 | §§2.2, 8 | Comparación de capacidad sin asumir escala lineal |
| FND-008 — Transporte HTTPS | C | PLD-008, PLD-009, PLD-014, PLD-015, PLD-019, PLD-024 | §5.1 | Certificados y fronteras de confianza verificadas |
| FND-009 — Autenticación JWT | C | PLD-010, PLD-019, PLD-021 | §§2.2, 5.2 | Casos positivos y negativos de validación |
| FND-010 — Autorización de mínimo privilegio | C | PLD-010, PLD-019, PLD-021 | §§5, 5.2 | Matriz de permisos y rechazos sin acceso indebido |
| FND-011 — Validación y límites de entrada | C | PLD-011, PLD-018, PLD-019, PLD-026 | §§5, 5.2 | Casos de entradas inválidas y límites aprobados |
| FND-012 — Gestión de secretos | C | PLD-008, PLD-010, PLD-011, PLD-015, PLD-022, PLD-023, PLD-024 | §5.1 | Ausencia de exposición, mínimo acceso y rotación |
| FND-013 — Evaluación OWASP API Security | C | PLD-009, PLD-010, PLD-011, PLD-021, PLD-022 | §5 | Cobertura de riesgos, controles y excepciones |
| FND-014 — Persistencia NoSQL | C | PLD-012, PLD-015 | §§2.1, 2.2, 2.4 | Decisión de motor e integración autorizada |
| FND-015 — Modelo de acceso y garantías de datos | C | PLD-006, PLD-012, PLD-017 | §§2.2, 2.4, 4, 8 | Patrones, atomicidad y capacidades reales |
| FND-016 — Redis compartido | C | PLD-006, PLD-008, PLD-013, PLD-015, PLD-024 | §§2.2, 5, 8 | Acceso compartido e aislamiento entre ámbitos |
| FND-017 — Coherencia e invalidación | C | PLD-013, PLD-017, PLD-019, PLD-021, PLD-025, PLD-026 | §§2.2, 8 | Ventanas y carreras de invalidación y repoblación |
| FND-018 — Fallos y presión de caché | C | PLD-013, PLD-016, PLD-019, PLD-021 | §§5, 8 | Degradación acotada sin relajar controles |
| FND-019 — Topología mínima objetivo | C | PLD-008, PLD-014, PLD-015, PLD-023, PLD-024 | §§2.2, 2.4 | Dos instancias efectivas con versión identificable |
| FND-020 — Balanceo y salud | C | PLD-009, PLD-014, PLD-015, PLD-016, PLD-020, PLD-023, PLD-024 | §§2.2, 5, 8 | Distribución, salud y retirada controlada |
| FND-021 — Caché de respuestas y eficiencia | C | PLD-013, PLD-019, PLD-020 | §§2.2, 5.2, 8 | Medición de aciertos y coste con aislamiento |
| FND-022 — Rate limiting entre instancias | C | PLD-013, PLD-019, PLD-020, PLD-021, PLD-026 | §§2.2, 5, 8 | Cuotas agregadas, tolerancia y rechazo HTTP 429 |
| FND-023 — Paginación acotada | C | PLD-012, PLD-018 | §§2.2, 8 | Límites y recorrido estable según garantía |
| FND-024 — Indexación basada en consultas | C | PLD-012 | §§2.2, 8 | Acceso medido y coste de los índices |
| FND-025 — Serialización JSON | C | PLD-011, PLD-018, PLD-019, PLD-025 | §§4, 5.2; ampliación SOL de `spec.md` | Conformidad de tipos, límites y datos expuestos |
| FND-026 — Compresión negociada y segura | C | PLD-019 | §§5, 8; ampliación SOL de `spec.md` | Negociación, elegibilidad y comparación de coste |
| FND-027 — Fallos acotados y cancelación | C | PLD-014, PLD-016, PLD-017, PLD-020, PLD-021, PLD-026 | §8 | Presupuestos y reintentos seguros bajo fallos |
| FND-028 — Consistencia y concurrencia | C | PLD-012, PLD-013, PLD-017, PLD-021 | §§2.4, 8 | Conflictos visibles y ausencia de pérdidas silenciosas |
| FND-029 — Idempotencia cuando corresponda | C | PLD-017, PLD-021 | §8 | Repeticiones entre instancias y ventanas de fallo |
| FND-030 — Comportamiento transversal del procesamiento HTTP | C | PLD-004, PLD-006, PLD-009, PLD-010, PLD-018, PLD-019 | §§4–5 | Orden y cobertura de controles aprobados |
| FND-031 — Contratos de respuesta y errores seguros | C | PLD-004, PLD-011, PLD-018, PLD-019, PLD-025 | §§4, 5.2 | Estados, contenido y correlación sin exposición |
| FND-032 — Registros estructurados y seguros | C | PLD-008, PLD-020 | §§2.2, 5.2 | Eventos minimizados, correlacionados y accesibles según política |
| FND-033 — Métricas y trazas distribuidas | C | PLD-015, PLD-020, PLD-026 | §§2.2, 5, 8 | Señales por ambiente, versión e instancia con cardinalidad acotada |
| FND-034 — Verificación unitaria y de integración | C | PLD-001, PLD-005, PLD-006, PLD-007, PLD-012, PLD-013, PLD-021, PLD-022, PLD-023 | §§3, 6 | Casos y resultados trazados a criterios |
| FND-035 — Pruebas de seguridad y escenarios adversos | C | PLD-001, PLD-007, PLD-010, PLD-011, PLD-017, PLD-021, PLD-022, PLD-023 | §§5–8 | Casos negativos y riesgos bloqueantes tratados |
| FND-036 — Controles de calidad y cadena de suministro | C | PLD-003, PLD-011, PLD-022, PLD-023 | §§6–7 | Análisis, dependencias, secretos y artefactos con umbrales |
| FND-037 — Entrega controlada y trazable | C | PLD-001, PLD-022, PLD-023, PLD-024, PLD-025 | §7 | Relación fuente, controles, artefacto, ambiente y aprobación |
| FND-038 — Infraestructura reproducible y revisable | C | PLD-001, PLD-015, PLD-024 | §7; ampliación SOL de `spec.md` | Recursos versionados, estado protegido y desviaciones |
| FND-039 — Despliegue y reversión seguros | C | PLD-014, PLD-016, PLD-023, PLD-024, PLD-025 | §§2.2, 7–8 | Coexistencia, retirada, entrega fallida y reversión compatible |
| FND-040 — Evolución de contratos | C | PLD-004, PLD-018, PLD-025 | §2.2 | Política y pruebas de compatibilidad y retirada |
| FND-041 — Configuración y aislamiento de ambientes | C | PLD-003, PLD-008, PLD-009, PLD-015, PLD-020, PLD-023, PLD-024, PLD-025, PLD-026 | §§5.1, 7–8 | Catálogo, precedencia y validación sin mezcla de ambientes |
| FND-042 — Presupuesto de rendimiento propuesto | P | PLD-015, PLD-016, PLD-019, PLD-020, PLD-021, PLD-026 | §§2.2, 6, 8 como fundamento | Ratificación del perfil y mediciones de extremo a extremo |
| FND-043 — Recuperación de una instancia propuesta | P | PLD-014, PLD-015, PLD-016, PLD-020, PLD-021, PLD-026 | §§2.2, 8 como fundamento | Ratificación y medición de exclusión y supervivencia |
| FND-044 — Consumo acotado bajo abuso | C | PLD-011, PLD-013, PLD-016, PLD-018, PLD-019, PLD-021, PLD-026 | §§2.2, 5, 8 | Límites cuantificados y casos de exceso |
| FND-045 — Invariantes de seguridad | C | PLD-009, PLD-010, PLD-011, PLD-013, PLD-017, PLD-019, PLD-021, PLD-022, PLD-026 | §§5–6 | Cero accesos indebidos y filtraciones en el conjunto aprobado |
| FND-046 — Cobertura de correlación operativa | P | PLD-020, PLD-021, PLD-026 | §§2.2, 5.2 como fundamento | Ratificación, cobertura y pérdida abrupta declarada |
| FND-047 — Recuperación de datos propuesta | P | PLD-012, PLD-015, PLD-016, PLD-021, PLD-024, PLD-026 | §§7–8 como fundamento | Ratificación y ensayo de restauración con alcance definido |
| FND-048 — Trazabilidad documental y de aceptación | C | PLD-001, PLD-002, PLD-021, PLD-022, PLD-023, PLD-025, PLD-027 | §§2–3, 11 | Cobertura de requisitos, decisiones y evidencia sin aprobación inventada |
| FND-049 — Delimitación absoluta de esta entrega | C | PLD-001, PLD-002, PLD-027, PLD-028 | §§1, 3–4, 9–10 | Límite histórico de la entrega de `spec.md`, preservado; esta intervención separada solo crea `plan.md` |

Las decisiones pueden cubrir aspectos transversales de varios FND sin fusionarlos. Una referencia a C como fundamento no convierte los objetivos numéricos o ampliaciones solicitadas en requisitos constitucionales previamente aprobados.

## 24. Criterios de aprobación

### PLD-027 — Puertas separadas de revisión y ejecución

- **Requisitos relacionados:** FND-001, FND-004, FND-005, FND-014, FND-019, FND-034, FND-035, FND-036, FND-037, FND-038, FND-042, FND-043, FND-044, FND-045, FND-046, FND-047, FND-048, FND-049.
- **Alternativas:** Aprobación implícita al crear documentos; aprobación global que habilite cualquier tarea; aprobación documental delimitada y autorización técnica independiente por alcance.
- **Propuesta justificada:** Mantener puertas distintas para consistencia documental, decisiones bloqueantes, alcance técnico y operación. La aprobación debe indicar versión, decisiones aceptadas, pendientes, responsable, alcance y restricciones. Un plan puede aprobarse con pendientes visibles para revisión conceptual, pero no habilitar la construcción de componentes afectados por esos pendientes.
- **Riesgos:** Considerar aprobados todos los FND o PLD por la aceptación del archivo, ejecutar con motor o proveedor no elegido o atribuir una revisión a agentes sin acceso autorizado.
- **Estado de aprobación:** Propuesta pendiente; no hay aprobación de contenido ni autorización técnica registrada en este plan.

| Puerta futura | Condiciones documentales de aceptación | Consecuencia permitida únicamente con autorización específica |
| --- | --- | --- |
| Revisión de este plan | Un único plan; 49 FND conservados; todas las PLD con alternativas, justificación, riesgos y estado; enlaces canónicos; sin decisiones aprobadas ficticias | Aprobar o devolver observaciones del documento; no ejecutar |
| Aprobación de diseño | Revisar `spec.md` y este plan, aceptar o rechazar REST y patrón, resolver decisiones relevantes y conservar los objetivos P como tales si no se ratifican | Autorizar otro artefacto documental si se solicita; no crear automáticamente `task.md` |
| Autorización de construcción | Alcance explícito; fuentes y criterios aplicables aprobados; motor, dependencias y organización resueltos cuando afecten al cambio; gobierno sincronizado o discrepancia aclarada formalmente | Solo cambios técnicos enumerados en esa nueva autorización |
| Autorización de verificaciones | Entorno, datos, herramientas, objetivos, límites de coste y procedimientos aprobados | Solo verificaciones autorizadas; conservar resultados y limitaciones |
| Autorización de entrega | Cloud, IaC, dos instancias, balanceo, secretos, permisos, controles y reversión aprobados; criterios bloqueantes satisfechos o excepción formal válida | Solo entrega y ambiente expresamente autorizados |

Para registrar una aprobación posterior se necesitará permiso para actualizar los documentos afectados; no se anticipa ese registro. Los perfiles de agentes y skills no se ejecutan ni se modifican para suplir la revisión. Este plan no acredita ninguno de los criterios técnicos anteriores.

## 25. Exclusiones del alcance

### PLD-028 — Contención de esta intervención

- **Requisitos relacionados:** FND-001, FND-048, FND-049 como antecedentes de gobierno y delimitación documental.
- **Alternativas:** Crear solo el plan solicitado; actualizar además gobierno, especificación y permisos; iniciar tareas o implementación al terminar.
- **Propuesta justificada:** Limitar la intervención a este archivo y conservar todos los existentes. Aplicar el principio constitucional de autorización delimitada y respetar FND-049 como restricción histórica de la entrega de especificación, sin reescribirla para ampliar permisos.
- **Riesgos:** Alteración de trabajo existente, paso de fase no autorizado, generación de tareas o selección técnica presentada como aprobada.
- **Estado de aprobación:** La creación exclusiva de `plan.md` está autorizada por la solicitud actual; no se ha aprobado el diseño propuesto ni ninguna ejecución posterior.

Quedan fuera de esta entrega:

- Funcionalidades específicas de ENVER, incluidos usuarios, pedidos, repartidores, pagos o entregas; no se definen endpoints, modelos ni reglas de esos ámbitos.
- Creación de código, entidades, DTOs, controladores, servicios, CRUD, proyectos o carpetas técnicas. Los nombres del §7 son solo alternativas conceptuales.
- Modificación de la solución, `Program.cs`, `.csproj`, Dockerfile, archivos de Visual Studio, configuración, constitución canónica, referencia raíz, `AGENTS.md`, `memoria.md`, `README.md` o `spec.md`.
- Selección aprobada de Clean Architecture, capas, proveedor cloud, motor NoSQL, paquetes, versiones complementarias o herramientas por el solo hecho de proponerlas.
- Creación de `task.md`, un backlog ejecutable, scripts, archivos IaC, canalizaciones, infraestructura o configuraciones.
- Instalaciones, ejecución de la aplicación, comandos, compilaciones, pruebas, migraciones, análisis automatizados, despliegues o verificaciones operativas.
- Invocación de subagentes, carga o ejecución de skills del proyecto, modificación de sus permisos o atribución de revisiones no realizadas.
- Afirmaciones de implementación, seguridad, rendimiento, disponibilidad, coherencia o recuperación ya demostradas.

El trabajo se detiene al completar `specs/001-backend-foundation/plan.md`. No se crea `task.md` ni se ejecuta ninguna parte de este plan.
