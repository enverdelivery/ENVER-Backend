# Tareas 001 — Arquitectura base de ENVER Backend

## 1. Identificación, autoridad y alcance

| Elemento | Valor |
| --- | --- |
| Solución / proyecto | `Enver.Backend` / `Enver.API` |
| Plataforma confirmada en origen | C#, ASP.NET Core 10 y .NET 10 |
| Fase de esta solicitud | Arquitectura base documental; descomposición de trabajo futuro |
| Versión documental | 0.1.0 |
| Estado del documento | Borrador para revisión; no constituye autorización operativa |
| Especificación de entrada | [`spec.md`](spec.md), versión 0.1.0, borrador |
| Plan de entrada | [`plan.md`](plan.md), versión 0.1.0, borrador |
| Autoridad principal | [Constitución canónica](../../.specify/memory/constitution.md), versión 1.0.0 |
| Referencia raíz | [`constitution.md`](../../constitution.md), sin duplicar autoridad normativa |
| Cantidad de tareas | 38, identificadas como TBF-001 a TBF-038 |
| Estado de todas las tareas | PENDIENTE / NO EJECUTADA |
| Cobertura documental prevista | 49/49 requisitos FND y 28/28 decisiones PLD |

Este archivo documenta trabajo futuro de preparación, definición de condiciones y verificación de la arquitectura base. **No ejecuta ninguna tarea**, no implementa componentes, no aprueba las propuestas de las fuentes y no presenta resultados técnicos.

La constitución, `AGENTS.md` y `memoria.md` se consultaron íntegramente, al igual que `spec.md` y `plan.md`; también se consultó `README.md` conforme a las instrucciones de agentes. Los documentos de gobierno siguen registrando la Fase 0. La solicitud posterior y explícita autoriza **solo crear este `task.md`**, sin modificar esas fuentes ni pasar automáticamente a implementación. C §§3, 9–11 sigue exigiendo autorización delimitada para actuaciones posteriores.

FND-049 corresponde al límite histórico de la entrega que creó `spec.md`. PLD-028 delimita la intervención que creó `plan.md`. Se conservan sus significados: sus prohibiciones de generar otros artefactos no se reescriben ni se interpretan como permisos generales. Este catálogo responde a una autorización documental posterior, no a ejecución o aprobación de las entregas anteriores.

### Convenciones de trazabilidad y estado

- **FND-001 a FND-049:** los 49 identificadores reales de requisitos de `spec.md`; no se crean otros FND ni se cambian sus estados.
- **PLD-001 a PLD-028:** las 28 decisiones reales de `plan.md`; las propuestas y sus detalles pendientes no se convierten en aprobados al descomponerlas.
- **DEC-01 a DEC-17:** los asuntos pendientes existentes en `spec.md` §20, desarrollados en `plan.md` §22; no son nuevas decisiones ni tareas adicionales.
- **TBF-nnn:** identificador y título de cada tarea en su encabezado. Los ocho campos posteriores completan descripción, resultado esperado, prioridad, dependencias, FND, PLD, aceptación y estado.
- **P0:** esencial para gobierno, seguridad o integridad. **P1:** necesario para la arquitectura objetivo. **P2:** optimización por ratificar. Las prioridades son propuestas y no habilitan ejecución ni sustituyen las dependencias.
- Toda dependencia entre TBF expresa precedencia futura, no que una tarea esté realizada. Las aprobaciones externas, permisos y parámetros pendientes son puertas adicionales, aunque no sean otras tareas.
- Estudiar una decisión no exige que ya esté resuelta. Su aprobación sí será necesaria para cerrar la definición dependiente o autorizar un cambio técnico. La aprobación de una tarea documental no aprueba automáticamente las PLD ni los requisitos relacionados.
- Los resultados de las tareas de pruebas, IaC, CI/CD y despliegue son **preparación documental de una actuación futura**. No se crean scripts, pruebas ejecutables, recursos o canalizaciones con este catálogo. La construcción y la obtención de evidencias reales requieren autorización técnica independiente.
- Las lecturas y comprobaciones de estructura necesarias para redactar este archivo no significan que se hayan ejecutado TBF-001, TBF-002 o TBF-038. Ninguna tarea se marca completada por existir su definición.

## 2. Preparación arquitectónica y dependencias

### TBF-001 — Preparar revisión de alcance y autorizaciones

- **Descripción:** Preparar una futura revisión del alcance, de las fuentes en borrador y de las puertas de aprobación, identificando la desalineación de fase sin corregir documentos por iniciativa propia.
- **Resultado esperado:** Propuesta de alcance autorizado, responsable de aprobación y tratamiento explícito de la sincronización documental pendiente.
- **Prioridad:** P0.
- **Dependencias:** Sin predecesoras TBF. Requiere una nueva autorización para realizar la revisión; DEC-01 y DEC-17 se estudian sin asumir su resolución.
- **Requisitos FND:** FND-001, FND-048, FND-049.
- **Decisiones del plan:** PLD-001, PLD-002, PLD-027, PLD-028.
- **Criterios de aceptación:** La propuesta distingue autorización documental, aprobación de diseño y permiso de ejecución; identifica documentos que necesitarían actualización autorizada y conserva FND-049 y PLD-028 como límites históricos. No atribuye aprobación o revisión a agentes no invocados.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-002 — Preparar línea base de trazabilidad

- **Descripción:** Preparar el mecanismo futuro de mantenimiento de la relación constitución–FND–PLD–tarea–evidencia, usando los identificadores de las fuentes.
- **Resultado esperado:** Criterio de control de cambios y revisión de cobertura, sin alterar los requisitos ni crear fuentes normativas alternativas.
- **Prioridad:** P0.
- **Dependencias:** TBF-001; autorización específica para revisar o registrar cambios en los documentos afectados.
- **Requisitos FND:** FND-001, FND-048.
- **Decisiones del plan:** PLD-002, PLD-027.
- **Criterios de aceptación:** Se preservan los 49 FND y las 28 PLD; toda relación nueva tiene fuente, alcance y estado. Evidencia ausente se identifica como pendiente y una propuesta no se presenta como decisión aprobada.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-003 — Preparar decisión sobre REST, capas y Clean Architecture

- **Descripción:** Preparar la revisión de alternativas REST y organización por capas o Clean Architecture proporcionada, considerando coste, límites y testabilidad.
- **Resultado esperado:** Comparación y recomendación para decisión del responsable, sin seleccionar automáticamente el patrón ni inventar módulos funcionales.
- **Prioridad:** P1.
- **Dependencias:** TBF-001, TBF-002; DEC-02 se debe resolver mediante aprobación explícita para adoptar una alternativa.
- **Requisitos FND:** FND-002, FND-003, FND-005, FND-034.
- **Decisiones del plan:** PLD-004, PLD-005, PLD-006, PLD-007.
- **Criterios de aceptación:** Se comparan al menos las alternativas de `plan.md` §§4–7 y sus compromisos; FND-005 permanece propuesto hasta aprobación, y no se exige un número de proyectos o un dominio vacío para declarar conformidad.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-004 — Preparar política de plataforma y dependencias

- **Descripción:** Preparar los criterios de compatibilidad, mantenimiento y aprobación de versiones sobre la base confirmada C#, ASP.NET Core 10 y .NET 10.
- **Resultado esperado:** Propuesta de control de SDK, runtime, parches y dependencias complementarias, sin instalaciones o selecciones implícitas.
- **Prioridad:** P0.
- **Dependencias:** TBF-001; DEC-05 pendiente para versiones y paquetes concretos.
- **Requisitos FND:** FND-004, FND-036, FND-041.
- **Decisiones del plan:** PLD-003, PLD-022.
- **Criterios de aceptación:** Se diferencian SDK, runtime, lenguaje y aplicación; cada incorporación futura requiere necesidad, compatibilidad, licencia, versión y aprobación. Las referencias existentes de la plantilla no se consideran nuevas decisiones aprobadas.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-005 — Preparar separación de responsabilidades y organización

- **Descripción:** Preparar límites conceptuales entre host, aplicación e infraestructura según el patrón que finalmente se apruebe, sin crear proyectos, clases o carpetas técnicas.
- **Resultado esperado:** Propuesta de responsabilidades, composición y referencias permitidas, con justificación de cada abstracción necesaria.
- **Prioridad:** P1.
- **Dependencias:** TBF-003, TBF-004; DEC-02 y aprobación de la organización definitiva.
- **Requisitos FND:** FND-002, FND-003, FND-004, FND-005, FND-006, FND-015, FND-016, FND-030, FND-034, FND-035.
- **Decisiones del plan:** PLD-005, PLD-006, PLD-007.
- **Criterios de aceptación:** Las reglas interiores no dependen de HTTP o clientes externos; no hay referencias circulares o modelos de persistencia expuestos. Se distingue flujo de ejecución de dirección de dependencias; no se crean reglas o entidades de negocio para llenar la estructura.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-006 — Preparar catálogo de configuración y ambientes

- **Descripción:** Preparar una definición de ambientes, configuración no secreta, obligatoriedad, origen, precedencia y validación previa a admitir tráfico.
- **Resultado esperado:** Catálogo conceptual de configuración y aislamiento entre desarrollo, pruebas y entorno publicado o equivalente académico.
- **Prioridad:** P0.
- **Dependencias:** TBF-004, TBF-005; DEC-14 y parámetros concretos de los componentes correspondientes.
- **Requisitos FND:** FND-004, FND-008, FND-012, FND-016, FND-019, FND-032, FND-041.
- **Decisiones del plan:** PLD-008.
- **Criterios de aceptación:** Se definen precedencia, validación y respuesta ante valores imprescindibles inválidos; se evita mezclar persistencia, Redis, secretos y telemetría entre ambientes. No se incluyen credenciales ni se modifican archivos de configuración.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-007 — Preparar evaluación cloud comparable

- **Descripción:** Preparar la comparación de AWS, Azure y GCP con un perfil equivalente, incluyendo .NET 10, dos instancias, balanceo, dependencias, red, recuperación y coste total.
- **Resultado esperado:** Criterios y ponderaciones para una recomendación posterior sustentada, sin declarar proveedor ganador.
- **Prioridad:** P1.
- **Dependencias:** TBF-001, TBF-004; DEC-04 a resolver con capacidades verificadas posteriormente y presupuesto aprobado. La compatibilidad con DEC-03 y DEC-07 condiciona la recomendación final.
- **Requisitos FND:** FND-004, FND-008, FND-012, FND-014, FND-016, FND-019, FND-020, FND-033, FND-038, FND-041, FND-042, FND-043, FND-047.
- **Decisiones del plan:** PLD-015.
- **Criterios de aceptación:** Se compara el mismo perfil en los tres proveedores, sin asumir gratuidad, cuotas, soporte vigente o independencia regional. La capacidad de mantener dos instancias activas y un balanceador se evalúa explícitamente antes de una selección aprobada.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-008 — Preparar ratificación de parámetros y objetivos

- **Descripción:** Preparar la revisión conjunta de límites, carga, recuperación y correlación definidos o propuestos por las fuentes.
- **Resultado esperado:** Relación de parámetros a ratificar, condiciones de medición y decisiones abiertas, sin nuevas metas aprobadas.
- **Prioridad:** P0.
- **Dependencias:** TBF-001, TBF-007; DEC-08, DEC-10, DEC-11, DEC-15 y DEC-16. Las capacidades finales de NoSQL y Redis condicionan la ratificación, no la preparación de la lista de parámetros.
- **Requisitos FND:** FND-007, FND-011, FND-017, FND-022, FND-027, FND-033, FND-041, FND-042, FND-043, FND-044, FND-045, FND-046, FND-047.
- **Decisiones del plan:** PLD-026.
- **Criterios de aceptación:** Se conservan los valores candidatos y condiciones de `spec.md` §18 y `plan.md` §22, resumidos en §4 de este catálogo. No se acepta técnicamente un límite sin cuantificar ni se convierte un objetivo P en SLA o decisión confirmada.
- **Estado:** PENDIENTE / NO EJECUTADA.

## 3. Catálogo de preparación y verificación futura

### TBF-009 — Preparar evaluación de riesgos OWASP API Security

- **Descripción:** Preparar la relación de categorías OWASP con riesgos de la base, controles y casos de verificación futuros.
- **Resultado esperado:** Propuesta de cobertura de riesgos, no aplicabilidades justificadas y tratamiento de riesgos residuales.
- **Prioridad:** P0.
- **Dependencias:** TBF-003, TBF-007; DEC-12 para edición OWASP, criterios y objetivos de revisión autorizados.
- **Requisitos FND:** FND-013, FND-035, FND-036, FND-045.
- **Decisiones del plan:** PLD-011, PLD-021, PLD-022.
- **Criterios de aceptación:** Cada categoría de la edición que se apruebe tiene riesgo, control y evidencia prevista o justificación de no aplicabilidad; no se declara seguridad por usar HTTPS o JWT solamente. No se realizan escaneos ni se inventan incidentes.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-010 — Preparar gestión de secretos e identidades de ejecución

- **Descripción:** Preparar las reglas de almacenamiento externo, aislamiento, acceso mínimo y rotación de secretos de cada ambiente.
- **Resultado esperado:** Política propuesta de acceso y ciclo de vida, sin valores reales de credenciales.
- **Prioridad:** P0.
- **Dependencias:** TBF-006, TBF-009; DEC-14 y capacidades del proveedor que se apruebe en DEC-04.
- **Requisitos FND:** FND-012, FND-041, FND-045.
- **Decisiones del plan:** PLD-008, PLD-011, PLD-022.
- **Criterios de aceptación:** Se delimitan identidades, permisos, rotación y respuesta a exposición; código, documentación, imágenes, salidas IaC y registros quedan excluidos como depósitos de secretos. No se accede a almacenes ni se configura ninguno.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-011 — Preparar HTTPS y confianza en proxies

- **Descripción:** Preparar alternativas de terminación TLS, protección del tramo interno y tratamiento de cabeceras reenviadas.
- **Resultado esperado:** Fronteras de confianza y criterios futuros de comprobación de transporte.
- **Prioridad:** P0.
- **Dependencias:** TBF-006, TBF-007, TBF-009, TBF-010; DEC-04 para topología TLS, certificados y proxies aprobados.
- **Requisitos FND:** FND-008, FND-013, FND-020, FND-030, FND-041, FND-045.
- **Decisiones del plan:** PLD-009.
- **Criterios de aceptación:** HTTPS público, certificados, protección interna y confianza limitada a proxies aprobados están contemplados; se definen casos de HTTP no protegido, cabeceras falsas y bucles de redirección. No se emiten certificados ni se modifica el host o balanceador.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-012 — Preparar autenticación JWT

- **Descripción:** Preparar requisitos y alternativas de emisor, firma, algoritmo permitido, audiencia, vigencia, tolerancia temporal y rotación de claves.
- **Resultado esperado:** Propuesta de validación JWT y comportamiento ante fallo de actualización de claves, sin elegir emisor ni generar emisión de tokens.
- **Prioridad:** P0.
- **Dependencias:** TBF-006, TBF-009, TBF-010; DEC-06 y versiones compatibles aprobadas mediante DEC-05 cuando correspondan.
- **Requisitos FND:** FND-009, FND-010, FND-012, FND-013, FND-030, FND-035, FND-045.
- **Decisiones del plan:** PLD-010.
- **Criterios de aceptación:** Se especifican resultados esperados para token ausente, firma alterada, algoritmo no permitido, emisor o audiencia incorrectos y vigencia inválida. Un token válido solo establece identidad; no se crean endpoints de autenticación ni permisos funcionales.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-013 — Preparar autorización y casos negativos

- **Descripción:** Preparar la política de denegación por defecto y separación entre autenticación y autorización para capacidades técnicas aprobadas.
- **Resultado esperado:** Criterios para una matriz futura de acceso por objeto, propiedad y función cuando aplique, sin definir roles de negocio.
- **Prioridad:** P0.
- **Dependencias:** TBF-009, TBF-012; DEC-06 y aprobación de cualquier excepción pública o ámbito protegido.
- **Requisitos FND:** FND-010, FND-013, FND-035, FND-045.
- **Decisiones del plan:** PLD-010, PLD-021.
- **Criterios de aceptación:** Los casos distinguen identidad inválida de permiso insuficiente y prevén cero accesos indebidos en el conjunto aprobado. Las excepciones anónimas requieren justificación; no se inventan recursos de usuarios, pedidos u otras funcionalidades.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-014 — Preparar validación y límites de entrada

- **Descripción:** Preparar la política de formatos, tamaños, profundidad, rangos y propiedades admitidas en las fronteras HTTP.
- **Resultado esperado:** Relación de límites por ratificar y casos negativos para evitar consumo o asignación de propiedades indebidos.
- **Prioridad:** P0.
- **Dependencias:** TBF-006, TBF-009; DEC-08 y DEC-09 para límites y convenciones JSON.
- **Requisitos FND:** FND-011, FND-025, FND-031, FND-035, FND-044, FND-045.
- **Decisiones del plan:** PLD-011, PLD-018, PLD-019.
- **Criterios de aceptación:** Cada límite tiene unidad, tratamiento del exceso y resultado esperado sin efectos persistentes o detalles internos. Los valores no definidos quedan pendientes, sin generar DTOs, modelos o validadores ejecutables.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-015 — Preparar decisión de motor NoSQL y garantías

- **Descripción:** Preparar una ficha comparativa de capacidades NoSQL para revisión, sin seleccionar MongoDB u otro motor.
- **Resultado esperado:** Criterios de elección sobre consultas, atomicidad, consistencia, concurrencia, seguridad, compatibilidad, respaldo y coste.
- **Prioridad:** P0.
- **Dependencias:** TBF-004, TBF-007, TBF-009; DEC-03 a resolver antes de adoptar motor o cliente; compatibilidad con DEC-04 y DEC-05.
- **Requisitos FND:** FND-003, FND-014, FND-015, FND-023, FND-024, FND-028, FND-034, FND-047.
- **Decisiones del plan:** PLD-012.
- **Criterios de aceptación:** La ficha no presupone transacciones, índices o unicidad globales; relaciona capacidades y límites con los FND. No se crean colecciones, entidades, adaptadores o conexiones ni se incorpora un paquete específico.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-016 — Preparar patrones de acceso e indexación

- **Descripción:** Preparar el método para relacionar patrones de consulta autorizados, ordenamiento, cardinalidad, índices y coste del motor aprobado.
- **Resultado esperado:** Plantilla conceptual de revisión de acceso y garantías, sin modelado específico de ENVER.
- **Prioridad:** P1.
- **Dependencias:** TBF-008, TBF-015; DEC-03 y patrones técnicos de ensayo expresamente autorizados. Las consultas funcionales permanecen fuera del alcance.
- **Requisitos FND:** FND-015, FND-023, FND-024, FND-028.
- **Decisiones del plan:** PLD-012.
- **Criterios de aceptación:** Cada patrón que se autorice podrá asociarse a índices, alcance de atomicidad y coste de lectura, escritura y almacenamiento. Se prevé justificar exploraciones completas y garantías de orden sin crear índices reales o datos de negocio.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-017 — Preparar Redis compartido y aislamiento

- **Descripción:** Preparar criterios de servicio, cliente, conectividad protegida y separación de ambientes y ámbitos de caché.
- **Resultado esperado:** Propuesta de integración distribuida compatible con dos instancias y sin dependencia de memoria local imprescindible.
- **Prioridad:** P1.
- **Dependencias:** TBF-006, TBF-007, TBF-010, TBF-015; DEC-07 y aprobaciones de compatibilidad y servicio según DEC-04 y DEC-05.
- **Requisitos FND:** FND-006, FND-016, FND-041, FND-045.
- **Decisiones del plan:** PLD-006, PLD-008, PLD-013.
- **Criterios de aceptación:** Se separan ambientes y ámbitos de acceso; Redis no se presenta como única copia duradera ni como producto ya configurado. No se instala cliente, inicia Redis o define una credencial real.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-018 — Preparar coherencia, invalidación y fallos de caché

- **Descripción:** Preparar elegibilidad, claves, expiración, obsolescencia máxima, invalidación y respuesta a fallos, contemplando carreras de repoblación.
- **Resultado esperado:** Política propuesta por categoría técnica autorizada y escenarios de verificación entre instancias.
- **Prioridad:** P0.
- **Dependencias:** TBF-008, TBF-016, TBF-017; DEC-07, DEC-08 y garantías relevantes de DEC-10.
- **Requisitos FND:** FND-017, FND-018, FND-021, FND-022, FND-028, FND-034, FND-044, FND-045.
- **Decisiones del plan:** PLD-013, PLD-019, PLD-026.
- **Criterios de aceptación:** Se explicitan ventanas y límites pendientes; invalidar tras escribir no se asume suficiente para eliminar carreras. Si no puede garantizarse frescura o aislamiento, se prevé excluir la categoría; el fallo de Redis no relaja silenciosamente cuotas o seguridad.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-019 — Preparar topología sin estado, balanceo y salud

- **Descripción:** Preparar el diseño conceptual de al menos dos instancias equivalentes, balanceador, estado compartido y señales de salud y aptitud.
- **Resultado esperado:** Topología propuesta y protocolo futuro de alternancia, retirada y sustitución de instancias.
- **Prioridad:** P1.
- **Dependencias:** TBF-005, TBF-006, TBF-007, TBF-011, TBF-017; DEC-04 y DEC-15 para dominios de fallo, capacidad y temporización.
- **Requisitos FND:** FND-006, FND-007, FND-008, FND-019, FND-020, FND-027, FND-039, FND-043.
- **Decisiones del plan:** PLD-014, PLD-015.
- **Criterios de aceptación:** Se distinguen instancia viva, aptitud de tráfico y fallo común de dependencia; no se requiere afinidad de sesión y no se promete tolerancia regional. No se definen endpoints de negocio, recursos cloud o un balanceador configurado.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-020 — Preparar presupuestos y tolerancia a fallos

- **Descripción:** Preparar presupuestos globales y por dependencia, cancelación segura, intentos limitados y criterios de degradación o aislamiento.
- **Resultado esperado:** Política propuesta de resiliencia que diferencie caída de instancia, caché y persistencia.
- **Prioridad:** P0.
- **Dependencias:** TBF-008, TBF-015, TBF-017, TBF-019; DEC-10, DEC-15 y DEC-16.
- **Requisitos FND:** FND-007, FND-018, FND-020, FND-027, FND-039, FND-042, FND-043, FND-044, FND-047.
- **Decisiones del plan:** PLD-016.
- **Criterios de aceptación:** Los intentos y tiempos se relacionan con el presupuesto total, sin reintentos ilimitados o multiplicados entre capas. Se identifica qué operaciones pueden repetirse y qué mecanismos necesitan evaluación sin elegir biblioteca ni provocar fallos reales.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-021 — Preparar concurrencia e idempotencia

- **Descripción:** Preparar garantías por operación técnica autorizada, conflictos, repetición y tratamiento de caídas entre efecto y registro del resultado.
- **Resultado esperado:** Tabla propuesta de atomicidad, consistencia, control de versión, identidad de repetición y vigencia, sin contratos de negocio.
- **Prioridad:** P0.
- **Dependencias:** TBF-015, TBF-018, TBF-020; DEC-03 y DEC-10.
- **Requisitos FND:** FND-015, FND-017, FND-027, FND-028, FND-029, FND-035, FND-045.
- **Decisiones del plan:** PLD-017.
- **Criterios de aceptación:** Se prevén conflictos visibles y no sobrescrituras silenciosas; los duplicados desde dos instancias tienen comportamiento esperado. Un bloqueo Redis aislado no prueba atomicidad con NoSQL ni exactamente una vez; los límites no resueltos siguen abiertos.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-022 — Preparar contratos HTTP, JSON, errores y middleware

- **Descripción:** Preparar semántica de estados, representación segura de errores, convenciones JSON y dependencias de orden del procesamiento transversal.
- **Resultado esperado:** Propuesta contractual técnica y relación de casos de éxito, rechazo, error y cancelación, sin DTOs o endpoints concretos.
- **Prioridad:** P0.
- **Dependencias:** TBF-005, TBF-008, TBF-011, TBF-012, TBF-013, TBF-014; DEC-09 y aprobación de la composición transversal.
- **Requisitos FND:** FND-003, FND-005, FND-011, FND-023, FND-025, FND-030, FND-031, FND-040, FND-044.
- **Decisiones del plan:** PLD-004, PLD-006, PLD-018, PLD-019.
- **Criterios de aceptación:** La propuesta no transforma errores en HTTP 200 ni exige cuerpos donde no corresponden; no expone secretos, trazas o persistencia. Distingue fallos del borde de los emitidos por la API y contempla fechas, nulos, números y correlación, sin generar código de middleware.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-023 — Preparar versionamiento y compatibilidad

- **Descripción:** Preparar alternativas de versión pública, cambios incompatibles, convivencia, retirada y relación con artefactos, datos, caché y documentos.
- **Resultado esperado:** Política propuesta de evolución y compatibilidad, sin seleccionar mecanismo por defecto.
- **Prioridad:** P1.
- **Dependencias:** TBF-003, TBF-022; DEC-09 y DEC-13.
- **Requisitos FND:** FND-003, FND-005, FND-017, FND-025, FND-031, FND-037, FND-039, FND-040, FND-041, FND-048.
- **Decisiones del plan:** PLD-025.
- **Criterios de aceptación:** Se distinguen versión de API, artefacto, datos y documento; los períodos y mecanismo quedan sujetos a aprobación. Se contemplan versiones mixtas y reversión sin inferir que revertir aplicación revierte datos; no se crean rutas o paquetes de versionado.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-024 — Preparar paginación e interacción con índices

- **Descripción:** Preparar comparación de cursor y desplazamiento según patrones permitidos, límites, orden estable y cambios concurrentes.
- **Resultado esperado:** Criterios de selección y aceptación de paginación consistentes con índices y garantías NoSQL.
- **Prioridad:** P1.
- **Dependencias:** TBF-016, TBF-022, TBF-023; DEC-03, DEC-08 y DEC-09.
- **Requisitos FND:** FND-015, FND-023, FND-024, FND-028, FND-044.
- **Decisiones del plan:** PLD-012, PLD-018.
- **Criterios de aceptación:** Se prevén límites inválidos o excesivos y recorridos sin omisiones o duplicados sobre datos sin cambios. La semántica ante cambios concurrentes y el coste de consulta se declaran, sin crear consultas, índices o colecciones funcionales.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-025 — Preparar cuotas globales y límites de consumo

- **Descripción:** Preparar ámbito, identidad, cuota, ventana, tolerancia y coordinación entre instancias para limitar solicitudes y recursos.
- **Resultado esperado:** Política propuesta de rate limiting y comportamiento de rechazo y fallo compartido.
- **Prioridad:** P0.
- **Dependencias:** TBF-008, TBF-012, TBF-017, TBF-019, TBF-020, TBF-022; DEC-08 y decisiones de coordinación de DEC-07 y DEC-10.
- **Requisitos FND:** FND-009, FND-010, FND-011, FND-018, FND-022, FND-030, FND-044, FND-045.
- **Decisiones del plan:** PLD-011, PLD-013, PLD-019.
- **Criterios de aceptación:** Se evalúa consumo agregado, no una cuota multiplicada por instancia; se prevé HTTP 429 y reintento cuando corresponda. Se considera identidad no falsificable y protección previa al trabajo costoso; el fallo compartido no desactiva la garantía sin aprobación.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-026 — Preparar compresión y eficiencia de respuestas

- **Descripción:** Preparar elegibilidad, negociación, variantes de caché y comparación de bytes, CPU y latencia, considerando exposición de datos.
- **Resultado esperado:** Política propuesta de compresión selectiva y protocolo futuro de comparación.
- **Prioridad:** P2.
- **Dependencias:** TBF-018, TBF-022, TBF-023; DEC-09 y criterios de seguridad y carga aprobados.
- **Requisitos FND:** FND-021, FND-025, FND-026, FND-031, FND-042, FND-045.
- **Decisiones del plan:** PLD-019.
- **Criterios de aceptación:** Se respetan capacidades del cliente y variantes de respuesta; los casos sensibles no se comprimen sin evaluación explícita. Algoritmos, umbrales y beneficios quedan pendientes de aprobación y medición; no se habilita compresión ni se generan respuestas.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-027 — Preparar logging estructurado y seguro

- **Descripción:** Preparar eventos, severidades, campos mínimos, correlación, acceso y retención evitando secretos y datos innecesarios.
- **Resultado esperado:** Propuesta de esquema operativo y política de minimización, no registros técnicos generados.
- **Prioridad:** P1.
- **Dependencias:** TBF-006, TBF-010, TBF-022; DEC-11 y DEC-14.
- **Requisitos FND:** FND-012, FND-032, FND-041, FND-046.
- **Decisiones del plan:** PLD-008, PLD-020.
- **Criterios de aceptación:** Se contemplan instante, ambiente, versión, instancia y correlación; se excluyen credenciales, tokens y cuerpos sensibles no autorizados. Los límites de retención, acceso y pérdida se distinguen de la presencia de una biblioteca de logging.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-028 — Preparar métricas, trazas y alertas

- **Descripción:** Preparar señales de tasa, latencia, errores, saturación, salud, caché, cuotas y dependencias con dimensiones acotadas.
- **Resultado esperado:** Catálogo propuesto de observabilidad y correlación distribuida con política de muestreo y pérdidas explícitas.
- **Prioridad:** P1.
- **Dependencias:** TBF-019, TBF-020, TBF-027; DEC-11 y DEC-15.
- **Requisitos FND:** FND-007, FND-020, FND-021, FND-022, FND-027, FND-032, FND-033, FND-041, FND-042, FND-043, FND-046.
- **Decisiones del plan:** PLD-020, PLD-026.
- **Criterios de aceptación:** Las señales distinguen instancias y ambientes sin cardinalidad o datos sensibles incontrolados; los identificadores entrantes no se aceptan ciegamente. Se separa correlación de muestreo y se prevé declarar pérdidas abruptas, sin configurar exportadores, tableros o alertas.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-029 — Preparar verificación unitaria

- **Descripción:** Preparar los casos aislables de políticas técnicas, límites y transformaciones que después se aprueben, sin sustituir integración por simulaciones.
- **Resultado esperado:** Catálogo propuesto de casos unitarios con FND, entradas, resultado esperado y límites de aislamiento.
- **Prioridad:** P1.
- **Dependencias:** TBF-005, TBF-014, TBF-020, TBF-021, TBF-022; DEC-12 y autorización separada antes de escribir o ejecutar pruebas.
- **Requisitos FND:** FND-002, FND-003, FND-034, FND-048.
- **Decisiones del plan:** PLD-005, PLD-006, PLD-021.
- **Criterios de aceptación:** Cada caso tiene criterio de origen y no afirma comprobar capacidades de NoSQL, Redis o balanceo mediante sustitutos. No se fija cobertura o framework no aprobado ni se crean proyectos o pruebas ejecutables.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-030 — Preparar integración y escenarios distribuidos

- **Descripción:** Preparar casos de HTTP, NoSQL, Redis y operación entre dos instancias, incluidos coherencia, cuotas, conflictos, contratos y fallos.
- **Resultado esperado:** Catálogo y protocolo propuestos con ambiente, versiones, datos sintéticos, garantías y evidencias futuras.
- **Prioridad:** P1.
- **Dependencias:** TBF-015, TBF-018, TBF-019, TBF-021, TBF-022, TBF-023, TBF-024, TBF-025, TBF-026, TBF-027, TBF-028, TBF-029; DEC-03, DEC-07 y DEC-12 resueltos según cada caso.
- **Requisitos FND:** FND-006, FND-014, FND-015, FND-016, FND-017, FND-018, FND-019, FND-020, FND-021, FND-022, FND-023, FND-024, FND-025, FND-027, FND-028, FND-029, FND-030, FND-031, FND-034, FND-035, FND-040, FND-041, FND-048.
- **Decisiones del plan:** PLD-012, PLD-013, PLD-014, PLD-017, PLD-018, PLD-021, PLD-025.
- **Criterios de aceptación:** Se prevén dependencias reales compatibles o equivalencia justificada, aislamiento y ausencia de datos de producción. Se incluyen carreras de caché y duplicados entre instancias sin generar datos, iniciar contenedores o ejecutar casos.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-031 — Preparar verificación de seguridad

- **Descripción:** Preparar casos positivos y negativos de transporte, JWT, autorización, validación, aislamiento, secretos, abuso y exposición.
- **Resultado esperado:** Procedimiento propuesto y clasificación de hallazgos y excepciones, limitado a objetivos autorizados.
- **Prioridad:** P0.
- **Dependencias:** TBF-009, TBF-010, TBF-011, TBF-012, TBF-013, TBF-014, TBF-018, TBF-025, TBF-027, TBF-030; DEC-12 y conjunto de seguridad aprobado.
- **Requisitos FND:** FND-008, FND-009, FND-010, FND-011, FND-012, FND-013, FND-017, FND-022, FND-026, FND-031, FND-035, FND-036, FND-044, FND-045.
- **Decisiones del plan:** PLD-009, PLD-010, PLD-011, PLD-019, PLD-021, PLD-022.
- **Criterios de aceptación:** Se conserva el criterio de cero accesos indebidos y filtraciones en el conjunto aprobado, sin extrapolar seguridad universal. Los fallos bloqueantes tienen tratamiento formal; no se ejecutan peticiones, escáneres o pruebas activas ni se exponen secretos en evidencias.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-032 — Preparar ensayos de rendimiento y fallo de instancia

- **Descripción:** Preparar procedimientos para capacidad, latencia, abuso, caché vacía o no disponible y retirada de una instancia, conservando los objetivos propuestos.
- **Resultado esperado:** Protocolos de medición y comparación con muestra, recursos, carga, tiempos y resultados esperados, sin mediciones realizadas.
- **Prioridad:** P1.
- **Dependencias:** TBF-008, TBF-016, TBF-018, TBF-019, TBF-020, TBF-025, TBF-026, TBF-028, TBF-030; DEC-08, DEC-12 y DEC-15, más autorización técnica y presupuesto para una ejecución futura.
- **Requisitos FND:** FND-007, FND-017, FND-018, FND-020, FND-021, FND-022, FND-024, FND-026, FND-027, FND-033, FND-042, FND-043, FND-044, FND-046.
- **Decisiones del plan:** PLD-014, PLD-016, PLD-019, PLD-020, PLD-021, PLD-026.
- **Criterios de aceptación:** Se mantienen las condiciones de FND-042 y FND-043, no se prometen prestaciones y se registran fallos en curso como parte del procedimiento. La mezcla sintética no define endpoints o funciones de ENVER y el protocolo no equivale a una prueba ejecutada.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-033 — Preparar respaldo y verificación de recuperación

- **Descripción:** Preparar el alcance de pérdida de persistencia, respaldo, restauración e integridad para el ambiente académico aprobado.
- **Resultado esperado:** Procedimiento propuesto que permita medir RTO y RPO en un ensayo posterior y diferencie recuperación de datos de reconstrucción de caché.
- **Prioridad:** P1.
- **Dependencias:** TBF-008, TBF-015, TBF-020, TBF-030; DEC-03, DEC-12 y DEC-16, con autorización independiente del ambiente y coste del ensayo.
- **Requisitos FND:** FND-014, FND-015, FND-027, FND-038, FND-039, FND-047.
- **Decisiones del plan:** PLD-012, PLD-016, PLD-021, PLD-024, PLD-026.
- **Criterios de aceptación:** Se preservan RTO y RPO como propuestas, se define inicio y fin de medición y comprobación de integridad y se limita la garantía al escenario aprobado. No se crean copias ni se restaura almacenamiento o se afirma tolerancia regional.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-034 — Preparar política de análisis estático y DevSecOps

- **Descripción:** Preparar controles de código, dependencias directas y transitivas, licencias, secretos y artefactos o contenedores.
- **Resultado esperado:** Política propuesta de herramientas por aprobar, severidades, bloqueos, falsos positivos y excepciones con vigencia.
- **Prioridad:** P1.
- **Dependencias:** TBF-004, TBF-009, TBF-010, TBF-029, TBF-030, TBF-031; DEC-05 y DEC-12.
- **Requisitos FND:** FND-004, FND-012, FND-013, FND-034, FND-035, FND-036, FND-037, FND-045, FND-048.
- **Decisiones del plan:** PLD-003, PLD-011, PLD-022.
- **Criterios de aceptación:** Cada control previsto tiene alcance, criterio de aprobación y evidencia requerida; una excepción necesita responsable, motivo y caducidad. No se instala ni ejecuta herramienta alguna o se atribuye vulnerabilidad o seguridad a un paquete sin evidencia.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-035 — Preparar CI/CD y puertas de promoción

- **Descripción:** Preparar la cadena futura de fuente revisada, verificaciones, artefacto inmutable, ambiente y autorización, separando aprobación documental y operación.
- **Resultado esperado:** Diseño propuesto de etapas, identidades, permisos mínimos, controles y evidencias de promoción y reversión.
- **Prioridad:** P1.
- **Dependencias:** TBF-023, TBF-029, TBF-030, TBF-031, TBF-034; DEC-13 y aprobación de plataforma, permisos y uso de secretos.
- **Requisitos FND:** FND-001, FND-004, FND-012, FND-019, FND-020, FND-034, FND-035, FND-036, FND-037, FND-039, FND-041, FND-048.
- **Decisiones del plan:** PLD-023, PLD-027.
- **Criterios de aceptación:** Se prevé promover el mismo artefacto verificado con configuración externa, sin recompilarlo por ambiente, y separar código no confiable de credenciales de entrega. No se crean workflows, identidades, artefactos o ejecuciones de canalización.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-036 — Preparar infraestructura como código

- **Descripción:** Preparar alcance, alternativas de herramienta y gestión protegida del estado para representar recursos y diferencias autorizadas entre ambientes.
- **Resultado esperado:** Criterios propuestos de reproducibilidad, revisión de cambios, detección de deriva y control de costes.
- **Prioridad:** P1.
- **Dependencias:** TBF-006, TBF-007, TBF-010, TBF-015, TBF-017, TBF-019, TBF-020, TBF-033, TBF-035; DEC-04, DEC-13 y DEC-14.
- **Requisitos FND:** FND-008, FND-012, FND-016, FND-019, FND-020, FND-037, FND-038, FND-039, FND-041, FND-047.
- **Decisiones del plan:** PLD-024.
- **Criterios de aceptación:** Cada recurso previsto se relaciona con requisito y decisión aprobada; se contemplan secretos en estado o salidas, deriva, acciones destructivas y coste. No se da por aprobada una herramienta ni se genera IaC o se aplica configuración o infraestructura.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-037 — Preparar despliegue futuro y verificación operativa

- **Descripción:** Preparar condiciones de publicación, convivencia de versiones, retirada controlada, fallo de entrega, reversión y obtención posterior de evidencias en dos instancias.
- **Resultado esperado:** Procedimiento propuesto de entrega y verificación con alcance, responsables, límites y puertas operativas, sin despliegue realizado.
- **Prioridad:** P0.
- **Dependencias:** TBF-019, TBF-023, TBF-028, TBF-030, TBF-031, TBF-032, TBF-033, TBF-034, TBF-035, TBF-036; DEC-04, DEC-13, DEC-15 y DEC-16, criterios bloqueantes resueltos y autorización técnica independiente. Ejecutar ensayos requiere además componentes e infraestructura previamente construidos bajo autorizaciones ajenas a este catálogo.
- **Requisitos FND:** FND-006, FND-007, FND-008, FND-019, FND-020, FND-027, FND-033, FND-037, FND-038, FND-039, FND-040, FND-041, FND-042, FND-043, FND-044, FND-045, FND-046, FND-047, FND-048.
- **Decisiones del plan:** PLD-014, PLD-015, PLD-016, PLD-020, PLD-021, PLD-023, PLD-024, PLD-025, PLD-026, PLD-027.
- **Criterios de aceptación:** El procedimiento identifica artefacto, ambiente e instancias; separa preparación de resultados reales y exige evidencia de salud, distribución y reversión compatible. No se promete revertir datos automáticamente ni se ejecutan comandos, se despliegan componentes o se declaran cumplidos los RNF.
- **Estado:** PENDIENTE / NO EJECUTADA.

### TBF-038 — Preparar revisión integral de cobertura y límites

- **Descripción:** Preparar una revisión final futura de alcance, decisiones, dependencias, criterios y evidencia, sin confundir existencia del catálogo con realización de tareas.
- **Resultado esperado:** Dictamen futuro de consistencia documental con pendientes y límites de aceptación visibles.
- **Prioridad:** P0.
- **Dependencias:** TBF-001 a TBF-037 para un cierre integral; las revisiones parciales deberán declarar su subconjunto y no atribuir cierre global. DEC-01; DEC-17 solo si se solicita participación de agentes o skills, sin eludir sus permisos.
- **Requisitos FND:** FND-001, FND-048, FND-049.
- **Decisiones del plan:** PLD-001, PLD-002, PLD-027, PLD-028.
- **Criterios de aceptación:** Se contabilizan 38 tareas, 49 FND y 28 PLD conservados; toda afirmación de avance exige evidencia y autorización. FND-049 y PLD-028 conservan su alcance histórico y los archivos anteriores permanecen intactos en esta intervención. Ninguna tarea se marca realizada como efecto de esta revisión de definición.
- **Estado:** PENDIENTE / NO EJECUTADA.

## 4. Parámetros conservados, dependencias y decisiones bloqueantes

### 4.1. Objetivos de origen, no resultados ni aprobaciones

| Requisito | Condiciones de origen conservadas | Tareas de preparación relacionadas |
| --- | --- | --- |
| FND-042, propuesta | 50 solicitudes válidas/s durante 10 minutos tras 2 de estabilización; p95 ≤ 500 ms, p99 ≤ 1.000 ms y fallos ≤ 1 %; cuerpos de hasta 16 KiB y proporción de aciertos/fallos de caché 50/50 cuando aplique. Medición desde cliente atravesando balanceador; mezcla, datos, región, recursos y cuotas por aprobar. Se cuentan 5xx, desconexiones y tiempos agotados; rechazos esperados por separado. | TBF-008, TBF-032, TBF-037 |
| FND-043, propuesta | Exclusión de instancia no apta ≤ 30 s y, después del límite ratificado, 100 % de nuevas solicitudes admitidas hacia instancias aptas durante 10 minutos. Capacidad suficiente de supervivencia y fallos en curso explícitos; no implica SLA mensual. | TBF-008, TBF-019, TBF-032, TBF-037 |
| FND-044, confirmado en origen con valores pendientes | 100 % de casos de exceso con tratamiento documentado; tamaños, paginación, concurrencia, cuotas, tolerancia y tiempo total deben cuantificarse y aprobarse. | TBF-008, TBF-014, TBF-024, TBF-025, TBF-031, TBF-032 |
| FND-045, confirmado en origen | Cero accesos no autorizados y cero exposición de secretos en el conjunto aprobado. Un caso fallido invalida el criterio; no demuestra ausencia universal de vulnerabilidades. | TBF-009, TBF-010, TBF-012, TBF-013, TBF-031 |
| FND-046, propuesta | 100 % de correlación en el conjunto de ensayo aprobado, separada del muestreo de trazas; pérdidas ante caída abrupta cuantificadas conforme a la garantía que se ratifique. | TBF-008, TBF-027, TBF-028, TBF-032, TBF-037 |
| FND-047, propuesta | RTO ≤ 30 minutos desde declaración de incidente hasta recuperación validada; RPO (Recovery Point Objective): máxima pérdida de datos tolerable expresada en tiempo entre el incidente y el punto consistente recuperable más reciente, no antigüedad de los registros individuales. RPO ≤ 15 minutos permanece PROPUESTA, pendiente de aprobación y sin cumplimiento demostrado, para el escenario académico de pérdida de persistencia que se apruebe. Integridad, copias, alcance y costes pendientes; no se extrapola a pérdida regional. | TBF-008, TBF-033, TBF-037 |

Los valores C y P de las fuentes se mantienen sin elevarlos de estado. Cualquier modificación futura del qué o de un criterio requiere autorización para revisar su fuente; no se introduce mediante esta lista de tareas.

### 4.2. Dependencias de trabajo y puertas externas

Las dependencias TBF son relaciones entre **resultados documentales futuros**, no infraestructura existente ni trabajos ejecutados. Los IDs no son órdenes de ejecución. Los campos de dependencias definen las entradas principales; las siguientes puertas son obligatorias adicionalmente cuando corresponda:

| Puerta externa | Condición antes de la actuación futura afectada |
| --- | --- |
| Revisión documental | Nueva autorización para ejecutar la tarea de revisión o preparación y para guardar cualquier resultado fuera de este archivo, si se solicita. |
| Selección de diseño | Aprobación expresa de las alternativas y parámetros pertinentes; crear este catálogo no resuelve decisiones. |
| Construcción técnica | Especificación y alcance aplicables aprobados, decisiones bloqueantes resueltas y permiso explícito de archivos, componentes y acciones. No se incluyen tareas de construcción autorizadas en esta entrega. |
| Ejecución de verificaciones | Componentes disponibles bajo autorización independiente, entorno aislado, herramientas, datos sintéticos, objetivos, presupuesto y procedimiento aprobados. Preparar un protocolo no ejecuta sus casos. |
| Entrega o restauración | Identidades, secretos, topología, controles, recuperación y ambiente aprobados; permiso operativo separado y evidencia previa exigible. |

No hay progreso automático por cumplir una dependencia. Un bloqueo se comunica sin resolverlo eligiendo proveedor, motor, paquete o política por iniciativa del agente. Para un cierre parcial se declara qué queda fuera; no se declara cobertura técnica integral.

### 4.3. Registro de decisiones pendientes y bloqueos

Los DEC conservan sus significados de `spec.md` §20. Esta tabla señala trabajo afectado y la aprobación requerida, sin registrarla como obtenida. Los códigos PLD relacionados se detallan también en la matriz del §6.

| Pendiente existente | Decisiones del plan relacionadas | Tareas principalmente afectadas | Qué bloquea mientras siga abierto |
| --- | --- | --- | --- |
| DEC-01 — Aprobación de fuentes y sincronización documental | PLD-001, PLD-002, PLD-027, PLD-028 | TBF-001, TBF-002, TBF-038; transversal al resto | Aprobar el diseño o habilitar actuaciones posteriores sin alcance expreso y tratamiento de la fase documental. |
| DEC-02 — REST, patrón y organización | PLD-004, PLD-005, PLD-006, PLD-007 | TBF-003, TBF-005, TBF-022 | Adoptar Clean Architecture, capas, modularidad o estructura definitiva. |
| DEC-03 — Motor NoSQL y garantías | PLD-012, PLD-017 | TBF-015, TBF-016, TBF-018, TBF-021, TBF-024, TBF-030, TBF-033 | Seleccionar motor o paquetes y afirmar atomicidad, índices, concurrencia o recuperación soportadas. |
| DEC-04 — Cloud, servicios, red y balanceador | PLD-009, PLD-014, PLD-015 | TBF-007, TBF-011, TBF-019, TBF-036, TBF-037 | Elegir proveedor y servicios, región, dominios de fallo o implementar topología y TLS. |
| DEC-05 — Dependencias y versiones | PLD-003, PLD-022 | TBF-004, TBF-012, TBF-015, TBF-017, TBF-034 | Incorporar paquetes, herramientas o versiones complementarias sin aprobación de compatibilidad y necesidad. |
| DEC-06 — JWT y políticas de acceso | PLD-010, PLD-011 | TBF-012, TBF-013, TBF-031 | Elegir emisor, parámetros, claves o reglas concretas de autorización. |
| DEC-07 — Redis, coherencia y fallos | PLD-013, PLD-019 | TBF-017, TBF-018, TBF-025, TBF-030 | Adoptar servicio, cliente, TTL, obsolescencia o degradación y afirmar garantías de caché. |
| DEC-08 — Límites y cuotas | PLD-011, PLD-013, PLD-019, PLD-026 | TBF-008, TBF-014, TBF-024, TBF-025, TBF-032 | Aceptar límites sin valores o demostrar cuotas globales sin tolerancia y ámbito aprobados. |
| DEC-09 — Contratos y evolución HTTP | PLD-018, PLD-019, PLD-025 | TBF-022, TBF-023, TBF-024, TBF-026 | Adoptar formato de error, JSON, paginación, compresión o mecanismo y períodos de versión. |
| DEC-10 — Consistencia, reintentos e idempotencia | PLD-012, PLD-016, PLD-017, PLD-026 | TBF-008, TBF-018, TBF-020, TBF-021, TBF-030 | Prometer ventanas, efectos repetibles, control de conflictos y presupuestos de dependencia. |
| DEC-11 — Observabilidad y pérdidas | PLD-020, PLD-026 | TBF-008, TBF-027, TBF-028, TBF-032 | Adoptar plataforma, retención, muestreo, alertas o garantía de correlación. |
| DEC-12 — Pruebas, calidad y OWASP | PLD-011, PLD-021, PLD-022 | TBF-009, TBF-029, TBF-030, TBF-031, TBF-032, TBF-033, TBF-034 | Seleccionar herramientas y umbrales o ejecutar verificaciones sin objetivos autorizados. |
| DEC-13 — CI/CD, IaC, entrega y reversión | PLD-023, PLD-024, PLD-025 | TBF-023, TBF-035, TBF-036, TBF-037 | Crear o activar automatización, definir permisos operativos o realizar entregas y reversión. |
| DEC-14 — Configuración, ambientes y secretos | PLD-008, PLD-011, PLD-024 | TBF-006, TBF-010, TBF-027, TBF-036 | Adoptar precedencia, configuración, almacenamiento de secretos o gestión del estado de IaC. |
| DEC-15 — Objetivos de carga, fallo y correlación | PLD-014, PLD-020, PLD-021, PLD-026 | TBF-008, TBF-019, TBF-028, TBF-032, TBF-037 | Declarar aceptados FND-042, FND-043 o FND-046 sin ratificación y evidencia. |
| DEC-16 — RTO, RPO y restauración | PLD-012, PLD-016, PLD-021, PLD-024, PLD-026 | TBF-008, TBF-020, TBF-033, TBF-036, TBF-037 | Aprobar recuperación y costes o extrapolar garantías no ensayadas. |
| DEC-17 — Acceso eventual de agentes o skills | PLD-001, PLD-002, PLD-027, PLD-028 | TBF-001, TBF-002, TBF-038, solo si se solicita delegación | Atribuir revisión a perfiles sin permisos o invocarlos eludiendo restricciones. No impide redactar este archivo por el agente principal autorizado. |

## 5. Matriz de cobertura de los 49 requisitos

Cada fila identifica el requisito real y al menos una tarea que lo aborda. **C** y **P** son estados de aprobación de origen en `spec.md`, no estados de ejecución. Todas las tareas siguen **PENDIENTE / NO EJECUTADA**. Las tareas indicadas son las principales; sus campos detallan relaciones adicionales. La cobertura es documental prevista, no implementación ni cumplimiento técnico.

| Requisito de `spec.md` | Estado de origen | Tareas principales que lo abordan |
| --- | --- | --- |
| FND-001 — Gobierno de la especificación | C | TBF-001, TBF-002, TBF-035, TBF-038 |
| FND-002 — Responsabilidades y SOLID | C | TBF-003, TBF-005, TBF-029 |
| FND-003 — Límites y contratos internos | C | TBF-003, TBF-005, TBF-015, TBF-022, TBF-023, TBF-029 |
| FND-004 — Plataforma tecnológica | C | TBF-004, TBF-005, TBF-006, TBF-007, TBF-034, TBF-035 |
| FND-005 — Estilo REST modular | P | TBF-003, TBF-005, TBF-022, TBF-023 |
| FND-006 — Ausencia de estado local imprescindible | C | TBF-005, TBF-017, TBF-019, TBF-030, TBF-037 |
| FND-007 — Escalabilidad horizontal | C | TBF-008, TBF-019, TBF-020, TBF-028, TBF-032, TBF-037 |
| FND-008 — Transporte HTTPS | C | TBF-006, TBF-007, TBF-011, TBF-019, TBF-031, TBF-036, TBF-037 |
| FND-009 — Autenticación JWT | C | TBF-012, TBF-025, TBF-031 |
| FND-010 — Autorización de mínimo privilegio | C | TBF-012, TBF-013, TBF-025, TBF-031 |
| FND-011 — Validación y límites de entrada | C | TBF-008, TBF-014, TBF-022, TBF-025, TBF-031 |
| FND-012 — Gestión de secretos | C | TBF-006, TBF-010, TBF-012, TBF-027, TBF-031, TBF-034, TBF-035, TBF-036 |
| FND-013 — Evaluación OWASP API Security | C | TBF-009, TBF-011, TBF-012, TBF-013, TBF-031, TBF-034 |
| FND-014 — Persistencia NoSQL | C | TBF-007, TBF-015, TBF-030, TBF-033 |
| FND-015 — Modelo de acceso y garantías de datos | C | TBF-005, TBF-015, TBF-016, TBF-021, TBF-024, TBF-030, TBF-033 |
| FND-016 — Redis compartido | C | TBF-006, TBF-007, TBF-017, TBF-030, TBF-036 |
| FND-017 — Coherencia e invalidación | C | TBF-008, TBF-018, TBF-021, TBF-023, TBF-030, TBF-031, TBF-032 |
| FND-018 — Fallos y presión de caché | C | TBF-018, TBF-020, TBF-025, TBF-030, TBF-032 |
| FND-019 — Topología mínima objetivo | C | TBF-006, TBF-007, TBF-019, TBF-030, TBF-035, TBF-036, TBF-037 |
| FND-020 — Balanceo y salud | C | TBF-007, TBF-011, TBF-019, TBF-020, TBF-028, TBF-030, TBF-032, TBF-037 |
| FND-021 — Caché de respuestas y eficiencia | C | TBF-018, TBF-026, TBF-028, TBF-030, TBF-032 |
| FND-022 — Rate limiting entre instancias | C | TBF-008, TBF-018, TBF-025, TBF-028, TBF-030, TBF-031, TBF-032 |
| FND-023 — Paginación acotada | C | TBF-015, TBF-016, TBF-022, TBF-024, TBF-030 |
| FND-024 — Indexación basada en consultas | C | TBF-015, TBF-016, TBF-024, TBF-030, TBF-032 |
| FND-025 — Serialización JSON | C | TBF-014, TBF-022, TBF-023, TBF-026, TBF-030 |
| FND-026 — Compresión negociada y segura | C | TBF-026, TBF-031, TBF-032 |
| FND-027 — Fallos acotados y cancelación | C | TBF-008, TBF-019, TBF-020, TBF-021, TBF-028, TBF-030, TBF-032, TBF-033, TBF-037 |
| FND-028 — Consistencia y concurrencia | C | TBF-015, TBF-016, TBF-018, TBF-021, TBF-024, TBF-030 |
| FND-029 — Idempotencia cuando corresponda | C | TBF-021, TBF-030 |
| FND-030 — Comportamiento transversal del procesamiento HTTP | C | TBF-005, TBF-011, TBF-012, TBF-022, TBF-025, TBF-030 |
| FND-031 — Contratos de respuesta y errores seguros | C | TBF-014, TBF-022, TBF-023, TBF-026, TBF-030, TBF-031 |
| FND-032 — Registros estructurados y seguros | C | TBF-006, TBF-027, TBF-028 |
| FND-033 — Métricas y trazas distribuidas | C | TBF-007, TBF-008, TBF-028, TBF-032, TBF-037 |
| FND-034 — Verificación unitaria y de integración | C | TBF-003, TBF-005, TBF-015, TBF-018, TBF-029, TBF-030, TBF-034, TBF-035 |
| FND-035 — Pruebas de seguridad y escenarios adversos | C | TBF-005, TBF-009, TBF-012, TBF-013, TBF-014, TBF-021, TBF-030, TBF-031, TBF-034, TBF-035 |
| FND-036 — Controles de calidad y cadena de suministro | C | TBF-004, TBF-009, TBF-031, TBF-034, TBF-035 |
| FND-037 — Entrega controlada y trazable | C | TBF-023, TBF-034, TBF-035, TBF-036, TBF-037 |
| FND-038 — Infraestructura reproducible y revisable | C | TBF-007, TBF-033, TBF-036, TBF-037 |
| FND-039 — Despliegue y reversión seguros | C | TBF-019, TBF-020, TBF-023, TBF-033, TBF-035, TBF-036, TBF-037 |
| FND-040 — Evolución de contratos | C | TBF-022, TBF-023, TBF-030, TBF-037 |
| FND-041 — Configuración y aislamiento de ambientes | C | TBF-004, TBF-006, TBF-007, TBF-008, TBF-010, TBF-011, TBF-017, TBF-023, TBF-027, TBF-028, TBF-030, TBF-035, TBF-036, TBF-037 |
| FND-042 — Presupuesto de rendimiento propuesto | P | TBF-007, TBF-008, TBF-020, TBF-026, TBF-028, TBF-032, TBF-037 |
| FND-043 — Recuperación de una instancia propuesta | P | TBF-007, TBF-008, TBF-019, TBF-020, TBF-028, TBF-032, TBF-037 |
| FND-044 — Consumo acotado bajo abuso | C | TBF-008, TBF-014, TBF-018, TBF-020, TBF-022, TBF-024, TBF-025, TBF-031, TBF-032, TBF-037 |
| FND-045 — Invariantes de seguridad | C | TBF-008, TBF-009, TBF-010, TBF-011, TBF-012, TBF-013, TBF-014, TBF-017, TBF-018, TBF-021, TBF-025, TBF-026, TBF-031, TBF-034, TBF-037 |
| FND-046 — Cobertura de correlación operativa | P | TBF-008, TBF-027, TBF-028, TBF-032, TBF-037 |
| FND-047 — Recuperación de datos propuesta | P | TBF-007, TBF-008, TBF-015, TBF-020, TBF-033, TBF-036, TBF-037 |
| FND-048 — Trazabilidad documental y de aceptación | C | TBF-001, TBF-002, TBF-023, TBF-029, TBF-030, TBF-034, TBF-035, TBF-037, TBF-038 |
| FND-049 — Delimitación absoluta de esta entrega | C | TBF-001, TBF-038: preservación del límite histórico de la entrega de `spec.md`, no extensión de su autorización |

**Cobertura documental de requisitos: 49/49 (100 %).** Los cinco requisitos propuestos FND-005, FND-042, FND-043, FND-046 y FND-047 conservan P. Los otros 44 conservan C con los matices y detalles pendientes de la fuente. Ninguno se declara implementado o técnicamente aceptado.

## 6. Matriz de cobertura de las 28 decisiones del plan

Las denominaciones reproducen las decisiones reales de `plan.md`. Las tareas asociadas son preparación o revisión futura, no resolución de la decisión. La plataforma y los límites de creación expresamente autorizados se distinguen de las propuestas de diseño todavía pendientes.

| Decisión real de `plan.md` | Tareas principales relacionadas | Situación conservada, sin nueva aprobación |
| --- | --- | --- |
| PLD-001 — Construcción futura por incrementos condicionados | TBF-001, TBF-038 | Propuesta pendiente; no autoriza incrementos técnicos |
| PLD-002 — Una sola cadena de autoridad y evidencia | TBF-001, TBF-002, TBF-038 | Trazabilidad exigida; mecanismo propuesto pendiente |
| PLD-003 — Plataforma y dependencias mínimas | TBF-004, TBF-034 | Base confirmada; parches, paquetes y política pendientes |
| PLD-004 — Estilo de exposición y modularidad | TBF-003, TBF-022 | REST modular propuesto, no adoptado |
| PLD-005 — Patrón arquitectónico propuesto | TBF-003, TBF-005, TBF-029 | Clean Architecture ligera frente a capas, pendiente |
| PLD-006 — Dirección de dependencias y composición | TBF-003, TBF-005, TBF-017, TBF-022, TBF-029 | Principio confirmado; organización y mecanismos pendientes |
| PLD-007 — Estructura mínima y ampliable | TBF-003, TBF-005 | Propuesta conceptual pendiente, sin proyectos creados |
| PLD-008 — Configuración tipada, validada y aislada | TBF-006, TBF-010, TBF-017, TBF-027 | Aislamiento requerido; catálogo y precedencia pendientes |
| PLD-009 — Transporte y fronteras de confianza | TBF-011, TBF-031 | HTTPS requerido; topología y TLS pendientes |
| PLD-010 — Identidad JWT y autorización diferenciadas | TBF-012, TBF-013, TBF-031 | JWT y mínimo privilegio requeridos; detalles pendientes |
| PLD-011 — Validación, secretos y revisión OWASP | TBF-009, TBF-010, TBF-014, TBF-025, TBF-031, TBF-034 | Obligaciones confirmadas; límites, almacén y edición pendientes |
| PLD-012 — Selección guiada por garantías y acceso | TBF-015, TBF-016, TBF-024, TBF-030, TBF-033 | NoSQL obligatorio; motor y garantías pendientes |
| PLD-013 — Caché compartida con coherencia explícita | TBF-017, TBF-018, TBF-025, TBF-030 | Redis requerido; servicio, coherencia y fallo pendientes |
| PLD-014 — Topología sin afinidad obligatoria | TBF-019, TBF-030, TBF-032, TBF-037 | Dos instancias requeridas; topología y tiempos pendientes |
| PLD-015 — Selección cloud por evidencia comparable | TBF-007, TBF-019, TBF-037 | Sin proveedor elegido ni comparación técnica ejecutada |
| PLD-016 — Presupuestos y aislamiento de fallos | TBF-020, TBF-032, TBF-033, TBF-037 | Resiliencia requerida; mecanismos y objetivos pendientes |
| PLD-017 — Control de conflictos y repetición segura | TBF-021, TBF-030 | Tratamiento requerido; garantías y mecanismos pendientes |
| PLD-018 — Contratos previsibles y acotados | TBF-014, TBF-022, TBF-024, TBF-030 | Contratos requeridos; formato y parámetros pendientes |
| PLD-019 — Procesamiento transversal y rendimiento seguro | TBF-014, TBF-018, TBF-022, TBF-025, TBF-026, TBF-031, TBF-032 | Capacidades requeridas; orden, cuotas y compresión pendientes |
| PLD-020 — Telemetría correlacionada y minimizada | TBF-027, TBF-028, TBF-032, TBF-037 | Observabilidad requerida; herramienta y cobertura pendientes |
| PLD-021 — Verificación por requisito y riesgo | TBF-009, TBF-013, TBF-029, TBF-030, TBF-031, TBF-032, TBF-033, TBF-037 | Tipos requeridos; casos, entornos, umbrales y ejecución pendientes |
| PLD-022 — Controles de calidad y cadena de suministro | TBF-004, TBF-009, TBF-010, TBF-031, TBF-034 | Controles previstos; herramientas y bloqueos pendientes |
| PLD-023 — Promoción de artefacto y autorización operativa | TBF-035, TBF-037 | Entrega controlada requerida; plataforma y permisos pendientes |
| PLD-024 — Recursos reproducibles y estado protegido | TBF-033, TBF-036, TBF-037 | IaC contemplada; herramienta, alcance y permisos pendientes |
| PLD-025 — Versiones de contrato, artefacto y documento | TBF-023, TBF-030, TBF-037 | Versionamiento requerido; mecanismo y períodos pendientes |
| PLD-026 — Ratificación de objetivos y parámetros | TBF-008, TBF-018, TBF-028, TBF-032, TBF-033, TBF-037 | Ratificación pendiente; sin métricas ni objetivos nuevos aprobados |
| PLD-027 — Puertas separadas de revisión y ejecución | TBF-001, TBF-002, TBF-035, TBF-037, TBF-038 | Puertas propuestas; sin aprobación técnica registrada |
| PLD-028 — Contención de esta intervención | TBF-001, TBF-038 | Delimita históricamente la creación de `plan.md`; no autoriza este catálogo ni ejecución por sí sola |

**Cobertura documental de decisiones: 28/28 (100 %).** Se mantienen abiertos los detalles y propuestas de origen; la existencia de una tarea no los resuelve. Cada tarea tiene al menos un FND y una PLD, y las dos matrices permiten localizar trabajo previsto sin cambiar las fuentes.

## 7. Restricciones y condición de continuidad

- Esta entrega crea únicamente `specs/001-backend-foundation/task.md`. No modifica archivos preexistentes ni crea nuevas especificaciones, planes u otros documentos.
- No desarrolla funcionalidades específicas de ENVER ni define endpoints, reglas o modelos de usuarios, pedidos, repartidores, pagos o entregas.
- No crea código, controladores, servicios, entidades, DTOs, modelos, proyectos técnicos, paquetes, scripts o configuraciones.
- No instala paquetes, inicia servicios, configura Redis o NoSQL, aprovisiona infraestructura, crea canalizaciones ni realiza despliegues.
- No ejecuta comandos, tareas de este catálogo, pruebas, compilaciones, instalaciones, migraciones, análisis automatizados o procedimientos de recuperación.
- No selecciona motor NoSQL, proveedor cloud, estructura definitiva, librerías o versiones complementarias sin aprobación.
- No invoca agentes, ejecuta skills ni altera permisos para simular una revisión. Una revisión futura debe respetar el acceso realmente autorizado.
- No declara implementados requisitos, completadas decisiones o realizadas verificaciones. Preparar documentación y disponer de un protocolo no equivalen a evidencia técnica.

**Resumen:** 38 tareas definidas; las 38 con estado **PENDIENTE / NO EJECUTADA**. Cobertura documental prevista de **49/49 FND** y **28/28 PLD**, con los **17 asuntos DEC** existentes visibles como pendientes y con las autorizaciones técnicas separadas.

El trabajo se detiene tras crear este archivo. No se ejecuta ninguna tarea ni se modifica el estado de las fuentes.
