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
| [`002-authentication/spec.md`](specs/002-authentication/spec.md) | Versión 0.2.0; 48 requisitos, AUTH-001 a AUTH-048; **APROBADO PARA PLANIFICACIÓN**; sin plan ni tareas generados y sin implementación autorizada. |

`001-backend-foundation` tuvo una **implementación técnica parcial/inicial**, integrada a `develop` mediante PR #1, merge `bef06b6` del commit `dcf04a4`. El responsable informa compilación correcta en .NET 10, funcionamiento de `GET /health` y actualización de la dependencia vulnerable `Microsoft.OpenApi`; la lectura confirma `net10.0`, el health check básico del host y la referencia 2.13.0. No se repiten verificaciones técnicas en esta entrega. Esto **no completa integralmente foundation**, no acredita readiness de dependencias ni seguridad validada y no marca completados los 49 FND, 28 PLD, 38 TBF o 17 DEC. Sus documentos y estados de origen se conservan como registro histórico.

La **fase funcional activa es `002-authentication`; su spec está APROBADO PARA PLANIFICACIÓN**, habilitada y no iniciada. La aprobación posterior expresa del responsable cierra AD-01 a AD-08 (AD-08 con ajuste), ratifica parcialmente AD-10 y difiere AD-09, AD-11, AD-12 y los detalles de AD-10. Esta intervención permite únicamente actualizar `specs/002-authentication/spec.md`, `AGENTS.md`, `memoria.md` y `README.md`. El nuevo estado permite elaborar `plan.md` en una entrega posterior, pero **no se genera en esta intervención**, no habilita código ni acredita aceptación técnica. Tampoco se generan `task.md` ni artefactos equivalentes.

MongoDB Atlas, Redis para caché, cuotas y revocación, AWS, GitHub Actions y Terraform son selecciones confirmadas por el responsable para el desarrollo incremental futuro; no capacidades ya integradas. MongoDB/Redis para autenticación se especifican en 002; las integraciones, pruebas, infraestructura, CI/CD, observabilidad y demás capacidades pendientes requerirán incrementos y specs posteriores según su alcance autorizado. No se seleccionan por iniciativa del agente servicios AWS, capas, paquetes o versiones adicionales.

La constitución canónica versión 1.0.0, su referencia raíz y los tres documentos de `specs/001-backend-foundation/` permanecen **intactos**. Sus referencias a Fase 0, restricciones de entrega y pendientes se conservan como antecedentes; esta apertura posterior expresamente autorizada no reforma la constitución ni aprueba retrospectivamente foundation. El historial, las fuentes de evidencia y los límites actuales se registran en [`memoria.md`](memoria.md).

## Prohibiciones sin autorización explícita

- No implementar funcionalidades específicas de ENVER.
- No crear controladores, DTOs, entidades, servicios, CRUD ni endpoints de negocio.
- No crear archivos ni ampliar documentos fuera de los cuatro artefactos autorizados para esta actualización de 002. No modificar la constitución, su referencia raíz ni ningún documento de 001; no crear plan, tareas o equivalentes en esta intervención. La planificación queda habilitada para una entrega posterior delimitada, no iniciada automáticamente.
- No modificar código, dependencias o configuración existente, incluidos `Program.cs`, `Enver.API.csproj` y `Dockerfile`.
- No ejecutar compilaciones, la aplicación, pruebas, instalaciones, migraciones, despliegues, análisis técnicos ni canalizaciones.
- No crear, modificar ni invocar subagentes, ni crear, modificar, incorporar, cargar o ejecutar skills sin autorización específica; los perfiles y permisos existentes se conservan.
- No borrar, mover o regenerar archivos de Visual Studio, solución, proyecto o artefactos existentes.

La inspección de archivos mediante lectura está permitida para conocer el estado y conservar el trabajo existente. No equivale a autorización para ejecutar el proyecto o intervenir técnicamente en él.

## Autorización para trabajo posterior

Antes de implementar o ejecutar tareas técnicas, solicitar autorización explícita al responsable, indicando alcance, archivos o componentes afectados y acciones previstas. Si la autorización es ambigua, aclararla antes de actuar.

La aprobación de un requisito, una propuesta documental o una lista de necesidades futuras no autoriza su implementación. El spec 002 aprobado permite planificación documental posterior, no tareas ni ejecución técnica; su plan requerirá revisión/aprobación propia. Cualquier fase posterior deberá respetar el flujo SDD y los criterios de aprobación definidos en la constitución.

## Coherencia y registro

- Redactar la documentación en español y mantener enlaces relativos válidos.
- Separar requisitos confirmados, propuestas, decisiones pendientes y estado observado.
- No seleccionar proveedor cloud, motor NoSQL, capas, librerías o versiones complementarias por iniciativa propia.
- No presentar dependencias de la plantilla como decisiones definitivas.
- Registrar cambios y decisiones autorizados en `memoria.md`, manteniendo coherentes los documentos afectados.
- No afirmar que una funcionalidad, una prueba, una medida de seguridad o un despliegue existe o funciona sin evidencia.
- No introducir secretos ni datos sensibles en documentos, ejemplos o registros.
- Detenerse al completar esta actualización documental de aprobación de 002 y comunicar archivos afectados, los 48 AUTH conservados, trazabilidad, contradicciones y decisiones diferidas. No ejecutar TBF, implementar AUTH ni iniciar aquí la planificación o trabajo adicional.

## Agentes y skills existentes y futuros

Los cinco subagentes y las cinco skills ya creados conservan sus definiciones y restricciones. Su invocación, carga, ejecución, modificación o ampliación de acceso requiere autorización expresa y respeto de los permisos reales. Cualquier incorporación futura deberá respetar la constitución y declarar su alcance, sin habilitar acciones técnicas o cambios de fase por sí misma. Este archivo no crea ni habilita ningún agente o skill.
