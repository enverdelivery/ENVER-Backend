# Constitución de ENVER Backend

Versión documental: 1.0.0.  
Estado: gobierno documental inicial de la Fase 0.  
Solución: `Enver.Backend`. Proyecto: `Enver.API`.

## 1. Propósito, autoridad y alcance

Esta constitución establece las reglas de gobierno de ENVER Backend, proyecto académico basado en C#, ASP.NET Core y .NET 10, mediante desarrollo guiado por especificaciones (SDD).

Este archivo, `.specify/memory/constitution.md`, es la fuente canónica y la autoridad principal de la documentación del proyecto. El archivo `constitution.md` de la raíz solo debe referenciarlo. Los demás documentos deben ser coherentes con esta constitución y no pueden modificarla implícitamente.

La Fase 0 comprende exclusivamente el establecimiento del gobierno documental mediante estos cinco archivos:

- `.specify/memory/constitution.md`.
- `constitution.md`.
- `AGENTS.md`.
- `memoria.md`.
- `README.md`.

Esta fase no autoriza desarrollar funcionalidades específicas de ENVER, adoptar una arquitectura definitiva ni ejecutar tareas técnicas. La existencia de un requisito futuro no constituye permiso para implementarlo.

## 2. Clasificación de requisitos y decisiones

Toda documentación debe distinguir explícitamente:

- **Confirmado:** requisito o decisión establecido por el responsable del proyecto.
- **Propuesta:** alternativa sujeta a evaluación y aprobación; no debe presentarse como adoptada.
- **Pendiente:** decisión no resuelta que requiere información o autorización.
- **Estado observado:** hecho obtenido de la inspección de archivos; no equivale a implementación validada ni a decisión aprobada.

### 2.1. Base confirmada

- Lenguaje C#.
- ASP.NET Core y .NET 10.
- Uso de una base de datos NoSQL conforme a la rúbrica docente, sin motor seleccionado.
- Alcance actual limitado al gobierno documental de la Fase 0.

### 2.2. Requisitos docentes para fases futuras

Los siguientes requisitos forman parte del alcance académico comunicado. Su cumplimiento deberá especificarse y verificarse en fases autorizadas; ninguno se declara implementado por este documento.

| Requisito | Criterio que deberá concretarse posteriormente |
| --- | --- |
| NoSQL | Seleccionar y justificar el motor, el modelo de datos y los patrones de acceso. |
| Dos instancias en nube | Definir el despliegue de la API en dos instancias y el tratamiento del estado compartido. |
| Balanceador | Distribuir tráfico entre las instancias y definir comprobaciones de salud. |
| JWT | Especificar emisión o proveedor, validación, vigencia y gestión de claves de los tokens. |
| Redis distribuido | Precisar su uso compartido entre instancias y su política de disponibilidad y seguridad. |
| Caché | Definir claves, aislamiento, expiración, invalidación y comportamiento ante fallos. |
| Paginación | Establecer límites y ordenamiento estable según los casos de consulta. |
| Índices | Diseñarlos a partir de consultas y validar su impacto y coste. |
| Limitación de solicitudes (rate limiting) | Definir cuotas, alcance entre instancias, respuestas y protección de recursos. |
| Registro de eventos (logging) | Producir registros estructurados, correlacionados y sin información sensible. |
| Observabilidad | Definir métricas, trazas, señales de salud y criterios de seguimiento. |
| Versionamiento | Definir compatibilidad, evolución y retirada de versiones de la API. |

### 2.3. Propuesta arquitectónica

Se propone una API REST modular con responsabilidades claramente delimitadas. Esta propuesta no decide un patrón definitivo de capas, una cantidad de proyectos, una biblioteca ni una organización física de carpetas.

### 2.4. Decisiones pendientes

- Proveedor cloud y servicios concretos.
- Motor NoSQL y estrategia de modelado.
- Estructura definitiva de capas y módulos.
- Librerías y versiones complementarias.
- Detalles de autenticación, Redis, operación, pruebas y entrega que requieran concretar los requisitos anteriores.

Las dependencias y configuraciones ya presentes en la plantilla se consideran estado observado, no aprobación de todas las decisiones futuras.

## 3. SDD y trazabilidad

- Antes de implementar, se deberán definir y aprobar el alcance, los requisitos y los criterios de aceptación de la fase correspondiente.
- La trazabilidad futura deberá relacionar requisito, especificación, decisión, trabajo autorizado, cambio y evidencia de verificación.
- Los requisitos y decisiones deberán disponer de identificadores cuando se elaboren las especificaciones futuras.
- Las propuestas no pueden convertirse en requisitos mediante su implementación silenciosa.
- Los cambios de alcance deben documentarse y someterse a autorización antes de ejecutarse.
- En la Fase 0 no se crearán `spec.md`, `plan.md` ni `task.md`, ni se iniciará su desarrollo bajo otros nombres para eludir esta restricción.
- No se afirmará cumplimiento, ejecución o avance sin evidencia. La presencia de archivos generados no demuestra una compilación, una prueba o un despliegue satisfactorio.

## 4. Separación de responsabilidades y SOLID

El diseño futuro deberá separar transporte HTTP, reglas de aplicación o negocio y acceso a infraestructura, sin imponer todavía una estructura definitiva de capas.

Se aplicarán los principios SOLID de forma proporcionada: responsabilidad única, extensión sin modificaciones innecesarias, sustitución correcta, interfaces específicas e inversión de dependencias. Las abstracciones deberán responder a necesidades justificadas y no a complejidad especulativa.

Los detalles de persistencia, caché y servicios externos no deberán determinar las reglas de negocio. Los contratos públicos no deberán exponer accidentalmente detalles internos o datos sensibles.

No se crearán ahora controladores, DTOs, entidades, servicios, operaciones CRUD ni endpoints de negocio.

## 5. Seguridad desde el diseño

La seguridad deberá incorporarse a los requisitos y criterios de aceptación de las fases futuras, tomando OWASP API Security como referencia. Se deberán evaluar amenazas y riesgos de autorización por objeto, propiedad y función, autenticación, consumo de recursos, flujos sensibles, solicitudes a servicios externos, configuración, inventario de API y consumo de API de terceros.

### 5.1. Transporte y secretos

- Exigir HTTPS en los entornos publicados y definir correctamente su terminación cuando exista un balanceador o proxy de confianza.
- No almacenar credenciales, tokens, claves privadas ni secretos en el repositorio, ejemplos o registros.
- Seleccionar posteriormente mecanismos externos de gestión de secretos, con acceso mínimo, rotación y separación por entorno.
- No considerar la redirección HTTPS de la plantilla como evidencia suficiente de una configuración de transporte segura.

### 5.2. Entradas y autorización

- Validar datos, formatos, tamaños, parámetros y límites en las fronteras de entrada; no confiar en datos externos ni en la naturaleza NoSQL del almacenamiento.
- Diferenciar autenticación de autorización y aplicar mínimo privilegio y denegación por defecto a los recursos protegidos.
- Verificar permisos sobre cada objeto, propiedad y operación que lo requiera; un JWT válido no concede por sí solo acceso a todos los recursos.
- Especificar la validación de firma, algoritmo permitido, emisor, audiencia y vigencia de JWT antes de implementarla.
- Evitar exposición de detalles internos en errores y de datos sensibles en respuestas, cachés y telemetría.

La presencia de componentes de autorización en la plantilla no demuestra que existan políticas o protección efectiva de recursos.

## 6. Estrategia futura de pruebas

Las fases autorizadas deberán contemplar:

- Pruebas unitarias de reglas y comportamientos aislados.
- Pruebas de integración de los límites HTTP y de las integraciones relevantes con NoSQL, Redis y otros servicios aprobados.
- Pruebas de seguridad para autenticación, autorización, validación, exposición de datos y límites de consumo, según los riesgos identificados.
- Verificaciones de concurrencia, idempotencia y resiliencia cuando el comportamiento de la operación las requiera.

Las evidencias deberán vincularse con criterios de aceptación. Las herramientas, umbrales y entornos de prueba permanecen pendientes. Durante la Fase 0 no se crearán ni ejecutarán pruebas.

## 7. Estrategia futura de DevSecOps y CI/CD

Se propone una entrega automatizada con revisiones y controles de calidad y seguridad. Su diseño deberá considerar análisis estático, análisis de dependencias y vulnerabilidades, detección de secretos, revisión de contenedores, pruebas y trazabilidad de artefactos.

Antes de habilitar despliegues se deberán definir entornos, permisos mínimos, gestión de secretos, controles de aprobación, criterios de promoción y reversión. Las pruebas dinámicas de seguridad deberán realizarse únicamente sobre entornos y objetivos autorizados.

No se seleccionan ahora proveedores, herramientas ni versiones. No se crearán canalizaciones de CI/CD ni se ejecutarán compilaciones, análisis, instalaciones o despliegues en esta fase. El Dockerfile existente no demuestra una estrategia DevSecOps implantada.

## 8. Resiliencia, concurrencia e idempotencia

- Definir tiempos de espera, cancelación y tratamiento de fallos de las dependencias según cada caso.
- Justificar reintentos limitados y evitar que multipliquen efectos no idempotentes; evaluar mecanismos de aislamiento y circuitos de protección cuando proceda.
- Especificar consistencia y control de concurrencia con base en las capacidades del motor NoSQL seleccionado, evitando pérdidas de actualizaciones.
- Analizar el estado compartido entre instancias, incluidos caché y controles distribuidos que deban ser coherentes.
- Definir idempotencia para las operaciones que la requieran, con alcance, duración y tratamiento de duplicados cuando se utilicen claves de idempotencia.
- Documentar el comportamiento ante fallos de Redis, del almacenamiento o de una instancia, sin prometer garantías no verificadas.

Estos son criterios para el diseño futuro, no autorización para crear componentes técnicos.

## 9. Reglas para agentes y futuras habilidades (skills)

- Consultar esta constitución antes de actuar y leer `AGENTS.md` y el estado registrado en `memoria.md`.
- Respetar el alcance de la fase y las instrucciones explícitas del responsable.
- Solicitar autorización antes de implementar funcionalidades, modificar código o configuración, o ejecutar tareas técnicas. La autorización documental no habilita esas acciones.
- No crear subagentes ni skills en la Fase 0. Su incorporación futura requiere autorización, alcance explícito y compatibilidad con esta constitución.
- Las futuras instrucciones de agentes o skills no podrán ampliar implícitamente el alcance ni convertir propuestas en decisiones.
- Ante contradicciones documentales, detener la acción afectada y solicitar aclaración; no resolverlas mediante cambios técnicos no autorizados.
- Conservar los archivos existentes de Visual Studio, código y configuración, sin borrarlos, reorganizarlos o corregirlos fuera del alcance autorizado.

## 10. Autorización y cierre de fase

La autorización actual se limita a crear los cinco documentos de gobierno. Quedan prohibidos, sin autorización posterior explícita, el desarrollo funcional, las modificaciones técnicas, las compilaciones, la ejecución de la aplicación, las pruebas, las instalaciones, las migraciones y los despliegues.

No se cambiará de fase automáticamente al completar los documentos. El trabajo deberá detenerse y comunicar el resultado y las decisiones pendientes.

## 11. Evolución y coherencia documental

Los cambios de esta constitución deberán autorizarse y registrar su motivo, su impacto y la actualización de la versión documental. Su historial de decisiones se reflejará en `memoria.md` sin atribuir aprobaciones no otorgadas.

Los documentos dependientes deberán actualizarse de forma coherente cuando cambie una regla. `constitution.md` conservará únicamente la referencia a la fuente canónica para evitar dos constituciones divergentes.

`memoria.md` registrará estado y decisiones; `README.md` facilitará la orientación; `AGENTS.md` concretará las reglas de actuación de los agentes. Ninguno sustituye esta constitución.
