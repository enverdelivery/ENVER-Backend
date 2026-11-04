---
name: sdd-governance
description: Guía documental de gobierno SDD de ENVER para revisar alcance, autoridad, estados de decisión y autorizaciones, solo por solicitud explícita.
metadata:
  opencode/autoinvoke: false
---

# Gobierno y metodología SDD

## Propósito

Definir un procedimiento documental reutilizable para preservar el gobierno SDD de `Enver.Backend`, proyecto `Enver.API`, basado en C#, ASP.NET Core y .NET 10, sin implementar funcionalidades ni ejecutar tareas técnicas.

## Cuándo utilizarla

Solo ante una solicitud explícita y autorizada de revisión del alcance, las decisiones o la coherencia metodológica. La existencia o carga de esta skill no autoriza ejecutar su procedimiento, pasar de fase ni modificar documentos.

## Relación con constitution.md

La [constitución canónica](../../../.specify/memory/constitution.md) es la autoridad principal del gobierno documental. El [constitution.md de la raíz](../../../constitution.md) es únicamente su referencia, sin contenido normativo duplicado. Esta skill desarrolla una pauta de revisión subordinada a los apartados de alcance, SDD, agentes, autorización y coherencia de la constitución; no la sustituye ni modifica.

Las rutas textuales del procedimiento se refieren a la raíz del proyecto; los enlaces Markdown se resuelven desde esta carpeta de skill.

## Procedimiento documental

Procedimiento previsto para una futura solicitud autorizada; no se ejecuta al crear este archivo.

1. Confirmar la autorización, la fase, el objeto de revisión y los documentos permitidos. Si faltan autorización o acceso, detenerse e informar.
2. Leer, en orden, `.specify/memory/constitution.md`, `AGENTS.md`, `memoria.md` y `README.md`, respetando los permisos del agente activo.
3. Clasificar cada afirmación relevante como requisito o decisión confirmada, propuesta, pendiente o estado observado. Asociar su fuente con archivo y línea o sección.
4. Comprobar que C#, ASP.NET Core, .NET 10 y NoSQL se mantienen como base confirmada; que la API REST modular sigue siendo propuesta; y que proveedor cloud, motor NoSQL, capas, librerías y versiones complementarias no se deciden implícitamente.
5. Revisar que los requisitos docentes futuros no aparezcan como implementados y que no se confundan observaciones de la plantilla con evidencias de funcionamiento.
6. Contrastar el alcance solicitado con la constitución. Si hay contradicción, comunicarla y detener la acción afectada sin cambiar las reglas ni asumir una excepción permanente.
7. Presentar en la respuesta hallazgos, aclaraciones necesarias y criterios de continuidad. Una transición futura requiere aprobación expresa de su alcance y criterios de aceptación; no se inicia automáticamente.

## Reglas y restricciones

- Aplicar solo revisión por lectura y respuesta en español. Esta skill no autoriza escritura ni registro automático en `memoria.md`.
- No crear `spec.md`, `plan.md`, `task.md` ni artefactos equivalentes para eludir la fase vigente.
- No generar código, funcionalidades, controladores, DTOs, entidades, servicios, CRUD ni endpoints de ENVER.
- No modificar código, configuración, archivos de Visual Studio, permisos de agentes ni esta skill por iniciativa propia.
- No ejecutar comandos, compilaciones, pruebas, instalaciones, migraciones, análisis automatizados, despliegues ni procedimientos de otras skills.
- No invocar subagentes, buscar permisos alternativos, acceder a rutas no autorizadas ni reproducir secretos.
- Un requisito, una propuesta o una autorización documental no constituyen permiso para implementar.

## Criterios de aceptación documental

- El informe identifica la fase y el alcance autorizado, sin atribuir aprobaciones inexistentes.
- Las confirmaciones, propuestas, pendientes y observaciones están diferenciadas y vinculadas con sus fuentes.
- La constitución mantiene su autoridad canónica y el archivo raíz sigue siendo referencia.
- Los hallazgos citan evidencia y distinguen contradicción de información insuficiente.
- Se indican las limitaciones de acceso y que no hubo ejecución técnica ni cambios de archivos.
- Las acciones futuras se presentan como pendientes de autorización, no como tareas iniciadas.

## Relación con agentes y compatibilidad

Existe afinidad temática con `sdd-reviewer`; no es una instrucción para invocarlo. Los cinco subagentes actuales tienen denegada la carga de skills y no se cambian sus permisos. Esta guía solo podrá cargarse explícitamente por un agente cuyo permiso `skill` lo permita y con autorización de alcance.

Frontmatter basado en la [guía de skills de OpenCode V2](https://opencode.ai/v2/docs/skills), para la versión instalada previamente comprobada `2.0.23`. `metadata.opencode/autoinvoke: false` excluye esta skill de la lista disponible para el modelo; no es un control de permisos ni impide su carga explícita por ID. El ID es `sdd-governance`, determinado por la carpeta. Su creación no incluye una prueba de carga o ejecución.
