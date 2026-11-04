---
name: traceability
description: Guía documental para relacionar requisitos, decisiones, autorizaciones y evidencias de ENVER sin inventar avances ni crear artefactos de fases posteriores.
metadata:
  opencode/autoinvoke: false
---

# Trazabilidad de requisitos y decisiones

## Propósito

Definir una pauta documental para relacionar fuentes, requisitos, decisiones, autorizaciones y evidencias de ENVER Backend. La evaluación debe ajustarse a la fase vigente y no producir especificaciones o tareas anticipadas.

## Cuándo utilizarla

Solo cuando se solicite explícitamente revisar la trazabilidad o detectar contradicciones de estado entre documentos autorizados. No utilizarla para crear un catálogo funcional, una planificación o una lista de implementación.

## Relación con constitution.md

La [constitución canónica](../../../.specify/memory/constitution.md), en particular sus reglas de clasificación, trazabilidad y evidencia, determina los criterios de esta guía. El [constitution.md de la raíz](../../../constitution.md) es únicamente una referencia. La skill no crea otra autoridad ni asigna identificadores aprobados por iniciativa propia.

Las rutas textuales parten de la raíz del proyecto; los enlaces Markdown son relativos a esta carpeta.

## Procedimiento documental

Procedimiento reservado a una solicitud futura autorizada; no se ejecuta al crear la skill.

1. Confirmar alcance, autorización, fase y documentos disponibles. Leer primero `.specify/memory/constitution.md`, `AGENTS.md`, `memoria.md` y `README.md`.
2. Identificar en las fuentes permitidas los requisitos, decisiones, propuestas y observaciones existentes. Conservar sus identificadores si los tienen; si no existen, usar citas de archivo y sección sin inventar códigos aprobados.
3. Asociar cada elemento con su fuente, estado declarado, referencia en otros documentos y evidencia disponible. Una autorización comunicada por el responsable debe distinguirse de una inferencia del revisor.
4. Revisar la cobertura documental de C#, ASP.NET Core, .NET 10, NoSQL y los requisitos docentes futuros: dos instancias en nube, balanceador, JWT, Redis distribuido, caché, paginación, índices, rate limiting, logging, observabilidad y versionamiento.
5. Distinguir los pendientes de proveedor cloud, motor NoSQL, capas, librerías y versiones complementarias de las propuestas de arquitectura o DevSecOps.
6. Revisar por lectura los enlaces internos permitidos. Ante un destino inaccesible, informar que no pudo verificarse; no afirmar que no existe ni buscar acceso por otra vía.
7. Detectar contradicciones, cambios de estado sin respaldo y afirmaciones de avance sin evidencia. No exigir en Fase 0 especificaciones, implementaciones o pruebas de fases posteriores.
8. Presentar, únicamente en la respuesta, una matriz con elemento, fuente, estado, autorización o evidencia disponible, referencias y brecha. Separar información ausente, contradictoria y fuera de alcance.

## Reglas y restricciones

- Solo lectura y elaboración de un informe en español; no crear matrices, registros o reportes en archivos.
- No modificar la constitución, `memoria.md`, otros documentos, código, configuración o permisos de agentes.
- No crear `spec.md`, `plan.md`, `task.md` ni equivalentes, ni convertir el informe en una especificación o planificación encubierta.
- No generar funcionalidades, código, controladores, DTOs, entidades, servicios, CRUD o endpoints de ENVER.
- No ejecutar comandos, validadores de enlaces, pruebas, compilaciones, instalaciones, migraciones, análisis automatizados o despliegues.
- No invocar agentes u otras skills, consultar destinos externos ni eludir permisos. Detener la acción afectada ante falta de autorización o conflicto con la constitución.
- No inferir una prueba o despliegue satisfactorio a partir de archivos generados, dependencias o configuraciones presentes.

## Criterios de aceptación documental

- Cada elemento del informe tiene fuente verificable o está marcado como no verificable dentro del alcance.
- Los estados confirmado, propuesta, pendiente y observado están separados y no implican ejecución.
- La matriz no atribuye identificadores, autorizaciones ni evidencias inexistentes.
- Las brechas distinguen contradicción de ausencia de información o limitación de acceso.
- El análisis respeta la fase y no exige artefactos que todavía están prohibidos.
- Se declara que no se ejecutaron verificaciones técnicas ni se modificaron archivos.

## Relación con agentes y compatibilidad

Afinidad temática con `traceability-reviewer`, sin invocación implícita. Los cinco subagentes actuales tienen denegada la carga de skills y no se alteran sus permisos. Su uso explícito requiere un agente con permiso `skill` y autorización de alcance.

Formato basado en la [guía de skills de OpenCode V2](https://opencode.ai/v2/docs/skills), para la versión instalada previamente comprobada `2.0.23`. `metadata.opencode/autoinvoke: false` oculta esta skill de la lista del modelo, pero conserva su registro para carga explícita autorizada. El ID es `traceability`. No se prueba la carga ni se ejecuta el procedimiento.
