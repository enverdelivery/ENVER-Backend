---
name: backend-architecture
description: Guía documental para revisar y proponer arquitectura base de ENVER con ASP.NET Core y .NET 10, sin implementar ni adoptar decisiones pendientes.
metadata:
  opencode/autoinvoke: false
---

# Arquitectura base de backend

## Propósito

Orientar la revisión y el diseño conceptual de la arquitectura base de `Enver.Backend` y `Enver.API`, con C#, ASP.NET Core y .NET 10. El resultado es una propuesta documental, nunca una implementación ni una aprobación arquitectónica implícita.

## Cuándo utilizarla

Solo cuando se solicite explícitamente una revisión arquitectónica documental o la comparación conceptual de alternativas. No usarla para generar una solución, reorganizar proyectos, seleccionar paquetes o iniciar funcionalidades de ENVER.

## Relación con constitution.md

La [constitución canónica](../../../.specify/memory/constitution.md), especialmente sus apartados de decisiones, separación de responsabilidades, seguridad y resiliencia, gobierna esta guía. El [constitution.md de la raíz](../../../constitution.md) solo referencia esa fuente. La skill no convierte la propuesta de API REST modular en una decisión definitiva ni fija capas o bibliotecas.

Las rutas textuales se entienden desde la raíz del proyecto; los enlaces Markdown son relativos a esta carpeta.

## Procedimiento documental

Procedimiento definido para un uso posterior autorizado; no se ejecuta durante la creación de esta skill.

1. Precisar autorización, preguntas de diseño y archivos que pueden leerse. Leer primero `.specify/memory/constitution.md`, `AGENTS.md`, `memoria.md` y `README.md`.
2. Si el alcance y los permisos lo permiten, consultar por lectura `Enver.Backend.slnx`, `Enver.API/Enver.API.csproj`, `Enver.API/Program.cs` y `Enver.API/Dockerfile`. Separar observaciones del estado de cualquier conclusión no verificada.
3. Describir responsabilidades conceptuales de transporte HTTP, aplicación o negocio e infraestructura. Evaluar SOLID, dependencias y testabilidad sin imponer una estructura física de carpetas o proyectos.
4. Mantener la API REST modular como propuesta. Comparar solo las alternativas solicitadas y explicar compromisos, sin decidir proveedor cloud, motor NoSQL, capas, librerías o versiones complementarias.
5. Relacionar el diseño conceptual con NoSQL, dos instancias en nube, balanceador, JWT, Redis distribuido, caché, paginación, índices, rate limiting, logging, observabilidad y versionamiento. Señalar lo pendiente sin inventar casos de negocio.
6. Examinar, como preguntas de diseño, estado compartido, consistencia, concurrencia, idempotencia, tiempos de espera y tratamiento de fallos. No prometer garantías ni prestaciones sin evidencia.
7. Entregar en la respuesta observaciones, alternativas propuestas, riesgos, decisiones pendientes y condiciones documentales para una revisión posterior autorizada.

## Reglas y restricciones

- Solo lectura y elaboración conceptual en la respuesta; no crear archivos ni modificar la documentación existente.
- No generar código, controladores, DTOs, entidades, servicios, CRUD, endpoints ni contratos de negocio específicos de ENVER.
- No crear `spec.md`, `plan.md`, `task.md` ni documentos equivalentes para adelantar fases.
- No modificar `Program.cs`, `Enver.API.csproj`, `Dockerfile`, dependencias, configuración, archivos de Visual Studio o permisos de agentes.
- No ejecutar comandos, la aplicación, compilaciones, pruebas, instalaciones, migraciones, análisis automatizados ni despliegues.
- No invocar agentes u otras skills ni eludir permisos de lectura. Si falta acceso o la solicitud contradice la constitución, detener la acción afectada y comunicarlo.
- No presentar la plantilla, un middleware, una referencia de paquete o un Dockerfile como prueba de una arquitectura validada o de un despliegue realizado.

## Criterios de aceptación documental

- El resultado mantiene C#, ASP.NET Core y .NET 10 como base confirmada y distingue claramente estado observado y diseño propuesto.
- La separación de responsabilidades y los compromisos se explican sin adoptar capas ni herramientas pendientes.
- Los requisitos docentes están relacionados con necesidades de diseño futuras, no con componentes declarados implementados.
- Los riesgos de distribución, concurrencia y resiliencia se expresan como aspectos por concretar o verificar.
- Cada observación relevante cita archivo y línea o sección; lo no verificable se declara como tal.
- No contiene implementación, decisiones aprobadas ficticias ni afirmaciones de pruebas ejecutadas.

## Relación con agentes y compatibilidad

Afinidad temática con `foundation-architect`, sin delegación automática. Los cinco perfiles actuales deniegan cargar skills; se mantienen intactos. La carga explícita requiere un agente con permiso `skill` y autorización para la revisión.

Formato contrastado con la [guía de skills de OpenCode V2](https://opencode.ai/v2/docs/skills) y la versión instalada previamente comprobada `2.0.23`. `metadata.opencode/autoinvoke: false` oculta la skill del catálogo del modelo, pero no bloquea la carga explícita autorizada. El ID es `backend-architecture`. No se prueba su carga ni se ejecuta el procedimiento.
