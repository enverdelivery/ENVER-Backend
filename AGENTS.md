# Instrucciones para agentes de ENVER Backend

## Lectura obligatoria

Antes de actuar, consultar en este orden:

1. [Constitución canónica](.specify/memory/constitution.md): autoridad principal del gobierno documental del proyecto.
2. Este archivo: reglas de actuación de los agentes.
3. [Memoria del proyecto](memoria.md): estado, decisiones, propuestas y pendientes.
4. [README](README.md): orientación general.

El archivo `constitution.md` de la raíz es una referencia, no otra constitución. Si existe una contradicción documental, no ejecutar la acción afectada: solicitar aclaración y autorización para corregirla.

## Estado documental y alcance vigente

La **Fase 0 está completada en su elaboración documental**. Su alcance original fue establecer el gobierno mediante:

- `.specify/memory/constitution.md`.
- `constitution.md`.
- `AGENTS.md`.
- `memoria.md`.
- `README.md`.

Las solicitudes documentales posteriores, separadas y delimitadas, dieron lugar al siguiente estado observado:

| Artefacto existente | Estado documental |
| --- | --- |
| `.opencode/agents/` | 5 subagentes creados; su existencia no autoriza invocarlos ni amplía sus permisos. |
| `.opencode/skills/` | 5 skills creadas; su existencia no autoriza cargarlas o ejecutar sus procedimientos. |
| [`spec.md`](specs/001-backend-foundation/spec.md) | 49 requisitos, FND-001 a FND-049; borrador pendiente de aprobación. |
| [`plan.md`](specs/001-backend-foundation/plan.md) | 28 decisiones, PLD-001 a PLD-028; borrador pendiente de aprobación. |
| [`task.md`](specs/001-backend-foundation/task.md) | 38 tareas, TBF-001 a TBF-038; todas PENDIENTE / NO EJECUTADA; borrador pendiente de aprobación. |

La arquitectura base está **documentada, no implementada**. Se conservan los 17 pendientes DEC-01 a DEC-17 y los estados de aprobación de origen. No hay especificaciones de funcionalidades específicas de ENVER.

La fase actual es **corrección de auditoría documental**, limitada por la solicitud explícita a A-01 y A-02: sincronizar `AGENTS.md`, `memoria.md` y `README.md`, y aclarar el RPO en `spec.md` y `task.md`; `plan.md` solo puede corregirse si contiene esa misma ambigüedad. No se autoriza crear archivos, reformar la constitución, modificar agentes o skills ni realizar actuaciones técnicas.

La constitución canónica versión 1.0.0 permanece intacta como autoridad principal. Sus referencias al alcance inicial de Fase 0 y las restricciones de cada entrega se conservan como antecedentes; las solicitudes posteriores no constituyen una reforma implícita de sus reglas ni autorización de implementación. El historial y los límites de esta sincronización se registran en [`memoria.md`](memoria.md). Completar o corregir documentos no aprueba su contenido ni autoriza continuar a otra fase.

## Prohibiciones sin autorización explícita

- No implementar funcionalidades específicas de ENVER.
- No crear controladores, DTOs, entidades, servicios, CRUD ni endpoints de negocio.
- No crear nuevas especificaciones, planes, catálogos de tareas ni artefactos equivalentes, ni ampliar o modificar los existentes fuera de la corrección documental expresamente autorizada.
- No modificar código, dependencias o configuración existente, incluidos `Program.cs`, `Enver.API.csproj` y `Dockerfile`.
- No ejecutar compilaciones, la aplicación, pruebas, instalaciones, migraciones, despliegues, análisis técnicos ni canalizaciones.
- No crear, modificar ni invocar subagentes, ni crear, modificar, incorporar, cargar o ejecutar skills sin autorización específica; los perfiles y permisos existentes se conservan.
- No borrar, mover o regenerar archivos de Visual Studio, solución, proyecto o artefactos existentes.

La inspección de archivos mediante lectura está permitida para conocer el estado y conservar el trabajo existente. No equivale a autorización para ejecutar el proyecto o intervenir técnicamente en él.

## Autorización para trabajo posterior

Antes de implementar o ejecutar tareas técnicas, solicitar autorización explícita al responsable, indicando alcance, archivos o componentes afectados y acciones previstas. Si la autorización es ambigua, aclararla antes de actuar.

La aprobación de un requisito, una propuesta documental o una lista de necesidades futuras no autoriza su implementación. Cualquier fase posterior deberá respetar el flujo SDD y los criterios de aprobación definidos en la constitución.

## Coherencia y registro

- Redactar la documentación en español y mantener enlaces relativos válidos.
- Separar requisitos confirmados, propuestas, decisiones pendientes y estado observado.
- No seleccionar proveedor cloud, motor NoSQL, capas, librerías o versiones complementarias por iniciativa propia.
- No presentar dependencias de la plantilla como decisiones definitivas.
- Registrar cambios y decisiones autorizados en `memoria.md`, manteniendo coherentes los documentos afectados.
- No afirmar que una funcionalidad, una prueba, una medida de seguridad o un despliegue existe o funciona sin evidencia.
- No introducir secretos ni datos sensibles en documentos, ejemplos o registros.
- Detenerse al completar la corrección documental autorizada y comunicar los archivos afectados, la trazabilidad conservada y los pendientes, sin ejecutar las tareas TBF ni iniciar trabajo adicional.

## Agentes y skills existentes y futuros

Los cinco subagentes y las cinco skills ya creados conservan sus definiciones y restricciones. Su invocación, carga, ejecución, modificación o ampliación de acceso requiere autorización expresa y respeto de los permisos reales. Cualquier incorporación futura deberá respetar la constitución y declarar su alcance, sin habilitar acciones técnicas o cambios de fase por sí misma. Este archivo no crea ni habilita ningún agente o skill.
