# ENVER Backend

Proyecto académico de backend cuya solución es `Enver.Backend` y cuyo proyecto inicial es `Enver.API`, basado en C#, ASP.NET Core y .NET 10.

## Estado actual

La **Fase 0 de gobierno documental SDD está completada en su elaboración**. Después se crearon, mediante solicitudes documentales separadas, 5 subagentes, 5 skills y los documentos de arquitectura base. La fase actual es **corrección de auditoría documental, limitada a A-01 y A-02**.

La arquitectura base está **documentada, no implementada**, y su documentación permanece pendiente de aprobación. `spec.md` contiene 49 requisitos FND, `plan.md` contiene 28 decisiones PLD y `task.md` contiene 38 tareas TBF, todas **PENDIENTE / NO EJECUTADA**. Se conservan los 17 pendientes DEC y los estados de aprobación de origen. No hay especificaciones de funcionalidades específicas de ENVER.

Existe una estructura inicial de ASP.NET Core con archivos de Visual Studio y Dockerfile. Su presencia no implica que se hayan verificado funcionalidades, seguridad, pruebas, contenedores o despliegues. Esta corrección es exclusivamente documental y no aprueba el diseño ni autoriza ejecutar las tareas.

## Documentación

La **[constitución canónica](.specify/memory/constitution.md)** es la autoridad principal de la documentación del proyecto.

| Archivo | Propósito |
| --- | --- |
| [`.specify/memory/constitution.md`](.specify/memory/constitution.md) | Constitución: alcance, SDD, diseño, seguridad, calidad y gobierno. |
| [`constitution.md`](constitution.md) | Acceso a la constitución canónica, sin duplicarla. |
| [`AGENTS.md`](AGENTS.md) | Instrucciones y restricciones para agentes. |
| [`memoria.md`](memoria.md) | Estado observado, decisiones, propuestas y pendientes. |
| [`README.md`](README.md) | Presentación del proyecto y orientación documental. |
| [`.opencode/agents/`](.opencode/agents/) | 5 subagentes creados; definiciones y permisos conservados, sin autorización de invocación por su existencia. |
| [`.opencode/skills/`](.opencode/skills/) | 5 skills creadas; su existencia no autoriza carga o ejecución. |
| [`specs/001-backend-foundation/spec.md`](specs/001-backend-foundation/spec.md) | Arquitectura base: 49 requisitos FND y 17 pendientes DEC; borrador pendiente de aprobación. |
| [`specs/001-backend-foundation/plan.md`](specs/001-backend-foundation/plan.md) | 28 decisiones PLD propuestas o con detalles pendientes; borrador pendiente de aprobación. |
| [`specs/001-backend-foundation/task.md`](specs/001-backend-foundation/task.md) | 38 tareas TBF; todas PENDIENTE / NO EJECUTADA; borrador pendiente de aprobación. |

El historial de Fase 0, las autorizaciones documentales posteriores y la corrección actual se conservan en [`memoria.md`](memoria.md). La constitución canónica no se modifica: mantiene el texto del alcance inicial, sin que esta actualización reforme sus reglas o habilite trabajo técnico. Las referencias a Fase 0 en las entregas anteriores describen su contexto histórico, no el inventario actualizado.

## Base confirmada

- C#, ASP.NET Core y .NET 10.
- Uso de NoSQL según la rúbrica docente, sin motor seleccionado.
- Fase 0 completada documentalmente; alcance actual limitado a la corrección expresamente autorizada de A-01 y A-02, sin implementación ni aprobación de decisiones pendientes.

La rúbrica también exige, para fases futuras, dos instancias en nube, balanceador, JWT, Redis distribuido, caché, paginación, índices, limitación de solicitudes (rate limiting), registro de eventos (logging), observabilidad y versionamiento. **No se declaran implementados ni verificados.**

## Propuestas y pendientes

Se propone una API REST modular y una estrategia futura de DevSecOps y CI/CD. No son una arquitectura definitiva ni una selección de herramientas.

Permanecen pendientes el proveedor cloud, el motor NoSQL, la estructura definitiva de capas, las librerías y sus versiones complementarias, así como los detalles de diseño y operación de los requisitos docentes.

## Restricciones vigentes

- No desarrollar funcionalidades específicas de ENVER ni crear controladores, DTOs, entidades, servicios, CRUD o endpoints de negocio.
- No crear nuevos archivos, especificaciones, planes, tareas, subagentes o skills ni ampliar el contenido existente fuera de esta corrección autorizada.
- No modificar la constitución canónica, su referencia raíz, agentes o skills ni ampliar permisos.
- No modificar código, configuración ni archivos existentes de Visual Studio.
- No ejecutar compilaciones, la aplicación, pruebas, instalaciones, migraciones ni despliegues.
- No interpretar requisitos futuros o propuestas como autorización para ejecutarlos.

No se incluyen instrucciones de ejecución porque las tareas técnicas están fuera del alcance actual. Tras corregir A-01 y A-02, el trabajo se detiene hasta recibir autorización explícita para continuar. Completar la elaboración documental no equivale a aprobación ni a cumplimiento técnico de la rúbrica.
