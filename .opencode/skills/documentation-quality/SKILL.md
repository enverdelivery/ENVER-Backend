---
name: documentation-quality
description: Guía para revisar claridad, consistencia, enlaces y evidencia del Markdown autorizado de ENVER, sin modificar archivos ni ampliar el alcance.
metadata:
  opencode/autoinvoke: false
---

# Calidad documental

## Propósito

Definir criterios y un procedimiento de revisión documental para que el Markdown de ENVER Backend sea claro, coherente, trazable y fiel al estado real del proyecto, sin duplicar normas ni inventar avances.

## Cuándo utilizarla

Solo por solicitud explícita de revisión de calidad de documentos concretos y accesibles. El procedimiento produce observaciones y propuestas en la respuesta; no autoriza aplicar correcciones ni mantener archivos automáticamente.

## Relación con constitution.md

La [constitución canónica](../../../.specify/memory/constitution.md) es la autoridad principal del gobierno documental. El [constitution.md de la raíz](../../../constitution.md) debe seguir siendo una referencia, sin duplicar el texto normativo. Esta skill aplica los criterios de coherencia y evolución documental de la constitución, sin cambiar su versión, contenido o jerarquía.

Las rutas textuales se refieren a la raíz del proyecto; los enlaces Markdown parten de esta carpeta de skill.

## Procedimiento documental

Los siguientes pasos son una pauta para un uso posterior autorizado, no una revisión ejecutada al crear este archivo.

1. Identificar autorización, documentos a revisar, fase y permisos. Leer primero `.specify/memory/constitution.md`, `AGENTS.md`, `memoria.md` y `README.md`.
2. Verificar que cada documento tenga una finalidad clara: constitución canónica, referencia raíz, reglas de agentes, memoria de estado u orientación general. Para perfiles y skills, revisar únicamente su documentación si el alcance y el acceso lo permiten, sin cargarlos ni invocarlos.
3. Revisar español, terminología consistente, nombres `Enver.Backend` y `Enver.API`, base tecnológica C#, ASP.NET Core y .NET 10, títulos, tablas y legibilidad del Markdown.
4. Comprobar por lectura las rutas y enlaces internos permitidos, interpretando los enlaces de cada skill desde su propia carpeta. No consultar enlaces externos ni ejecutar validadores.
5. Detectar duplicación de reglas, contradicciones de autoridad y confusión entre confirmado, propuesta, pendiente y observado. No convertir diferencias de estilo en cambios de alcance.
6. Contrastar las afirmaciones de avances con las evidencias disponibles. Mantener la Fase 0 documental y los requisitos docentes futuros separados de cualquier estado implementado.
7. Presentar en la respuesta hallazgos y correcciones propuestas, con archivo y línea o sección, motivo e impacto. Si una corrección requeriría modificar la constitución u otros documentos, señalar que requiere autorización independiente y coherencia entre los afectados.

## Reglas y restricciones

- Esta skill es de revisión documental por lectura; no otorga permisos de edición ni autoriza cambios, aunque el agente activo tenga capacidad de escritura.
- No modificar Markdown, código, configuración, archivos de Visual Studio, permisos de agentes o definiciones de skills durante este procedimiento.
- No crear `spec.md`, `plan.md`, `task.md`, equivalentes, nuevos documentos, agentes o skills.
- No generar código, funcionalidades, controladores, DTOs, entidades, servicios, CRUD o endpoints de ENVER.
- No ejecutar comandos, formateadores, linters, validadores, compilaciones, pruebas, instalaciones, migraciones o despliegues.
- No invocar agentes u otras skills ni eludir restricciones de lectura. Si falta acceso, comunicarlo; si hay conflicto con la constitución o el alcance, detener la acción afectada.
- No introducir secretos, reproducir datos sensibles o atribuir decisiones, aprobaciones y verificaciones inexistentes.

## Criterios de aceptación documental

- El informe identifica los documentos revisados y sus límites de acceso, sin afirmar una revisión de fuentes no consultadas.
- Las propuestas mantienen español, terminología uniforme, estructura legible y enlaces relativos coherentes.
- Se preserva una única constitución canónica y el archivo raíz no duplica su contenido.
- Las decisiones y los avances se distinguen de propuestas, pendientes y observaciones con evidencia limitada.
- Cada corrección propuesta indica ubicación, motivo y alcance, sin aplicarse ni ampliar la fase.
- El resultado declara ausencia de ejecución técnica y de modificaciones de archivos.

## Relación con agentes y compatibilidad

Afinidad temática con `documentation-keeper`, sin invocarlo ni concederle permisos adicionales. Sus ediciones futuras siguen limitadas a los cinco Markdown de gobierno, con autorización explícita y aprobación técnica por archivo. Esta skill no activa esa capacidad ni constituye una solicitud de edición.

Los cinco subagentes actuales deniegan cargar skills y permanecen intactos. La carga explícita de esta guía requiere un agente con permiso `skill` y autorización de revisión.

Frontmatter basado en la [guía de skills de OpenCode V2](https://opencode.ai/v2/docs/skills), para la versión instalada previamente comprobada `2.0.23`. `metadata.opencode/autoinvoke: false` excluye la skill del catálogo del modelo; no sustituye los permisos ni bloquea su carga explícita autorizada. El ID es `documentation-quality`. No se realiza prueba de carga o ejecución.
