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
| [`002-authentication/spec.md`](specs/002-authentication/spec.md) | Versión 0.3.0; 48 requisitos, AUTH-001 a AUTH-048; **APROBADO PARA PLANIFICACIÓN**; ajuste mínimo Redis aprobado por APR-03, sin implementación acreditada. |
| [`002-authentication/plan.md`](specs/002-authentication/plan.md) | Versión 0.2.0; 25 decisiones PLD-AUTH; **APROBADO PARA DESCOMPOSICIÓN EN TAREAS**; sin implementación autorizada. |
| [`002-authentication/task.md`](specs/002-authentication/task.md) | Versión 0.1.0; 79 tareas T-AUTH-001 a T-AUTH-079, todas PENDIENTE / NO EJECUTADA; **APROBADO PARA EJECUCIÓN INCREMENTAL**; AUTH 48/48, PLD-AUTH 25/25, AC 21/21 y 8 gates pendientes. |

`001-backend-foundation` tuvo una **implementación técnica parcial/inicial**, integrada a `develop` mediante PR #1, merge `bef06b6` del commit `dcf04a4`. El responsable informa compilación correcta en .NET 10, funcionamiento de `GET /health` y actualización de la dependencia vulnerable `Microsoft.OpenApi`; la lectura confirma `net10.0`, el health check básico del host y la referencia 2.13.0. No se repiten verificaciones técnicas en esta entrega. Esto **no completa integralmente foundation**, no acredita readiness de dependencias ni seguridad validada y no marca completados los 49 FND, 28 PLD, 38 TBF o 17 DEC. Sus documentos y estados de origen se conservan como registro histórico.

La **fase funcional activa es `002-authentication`; spec APROBADO PARA PLANIFICACIÓN y plan APROBADO PARA DESCOMPOSICIÓN EN TAREAS**. APR-02 aprobó el spec v0.2.0; SOL-PLAN-002 dio lugar al plan v0.1.0, bloqueado inicialmente por la discrepancia AUTH-027 / PLD-AUTH-08. APR-03 aprueba y aplica el ajuste mínimo de AUTH-022/027/043 y referencias Redis, resuelve ese bloqueo y conserva su historial en plan §23.1 y memoria §15. Aquella intervención solo actualizó spec/plan y sincronizó AGENTS/memoria/README; no creó tareas ni implementó nada. AD-09/11/12 y detalles de AD-10 conservan sus diferidos; los controles técnicos siguen pendientes de evidencia.

**Entrega documental posterior SOL-TASK-002:** el responsable aprueba el catálogo compactado y autoriza crear únicamente `specs/002-authentication/task.md` y sincronizar AGENTS/memoria/README. Se registran 79 tareas y estado **APROBADO PARA EJECUCIÓN INCREMENTAL**, sin ejecutar ninguna ni implementar autenticación. Ese estado solo permite solicitar ejecución posterior de una tarea/incremento con autorización delimitada: no satisface gates, resuelve diferidos, autoriza AWS ni acredita implementación. AUTH-027 permanece **NO IMPLEMENTADO / NO SATISFECHO**. Registro en memoria §16; spec/plan y constituciones/001 permanecen intactos.

Arquitectura de 002 conservada: un Enver.API con separación lógica y tres proyectos futuros de pruebas; Redis compartido para revocación/cuotas, sin ledger MongoDB de revocaciones, segundo almacén duradero obligatorio o transacciones distribuidas. Logout confirmado exige rechazo distribuido con continuidad confiable; pérdida detectada o integridad incierta exige fail-closed/503 y recuperación validada o cuarentena. La pérdida catastrófica totalmente silenciosa no observable es riesgo residual de infraestructura, no eliminado. AUTH-027 está aprobado documentalmente, **NO IMPLEMENTADO / NO SATISFECHO**.

MongoDB Atlas, Redis para caché, cuotas y revocación, AWS, GitHub Actions y Terraform son selecciones confirmadas por el responsable para el desarrollo incremental futuro; no capacidades ya integradas. MongoDB/Redis para autenticación se especifican en 002; las integraciones, pruebas, infraestructura, CI/CD, observabilidad y demás capacidades pendientes requerirán incrementos y specs posteriores según su alcance autorizado. No se seleccionan por iniciativa del agente servicios AWS, capas, paquetes o versiones adicionales.

La constitución canónica versión 1.0.0, su referencia raíz y los tres documentos de `specs/001-backend-foundation/` permanecen **intactos**. Sus referencias a Fase 0, restricciones de entrega y pendientes se conservan como antecedentes; esta apertura posterior expresamente autorizada no reforma la constitución ni aprueba retrospectivamente foundation. El historial, las fuentes de evidencia y los límites actuales se registran en [`memoria.md`](memoria.md).

## Prohibiciones sin autorización explícita

- No implementar funcionalidades específicas de ENVER.
- No crear controladores, DTOs, entidades, servicios, CRUD ni endpoints de negocio.
- No crear archivos ni ampliar documentos fuera de la entrega SOL-TASK-002: crear `specs/002-authentication/task.md` y sincronizar únicamente `AGENTS.md`, `memoria.md` y `README.md`. No modificar spec/plan, constituciones o 001, ni crear otros artefactos. La aprobación del catálogo no inicia su ejecución automáticamente.
- No modificar código, dependencias o configuración existente, incluidos `Program.cs`, `Enver.API.csproj` y `Dockerfile`.
- No ejecutar compilaciones, la aplicación, pruebas, instalaciones, migraciones, despliegues, análisis técnicos ni canalizaciones.
- No crear, modificar ni invocar subagentes, ni crear, modificar, incorporar, cargar o ejecutar skills sin autorización específica; los perfiles y permisos existentes se conservan.
- No borrar, mover o regenerar archivos de Visual Studio, solución, proyecto o artefactos existentes.

La inspección de archivos mediante lectura está permitida para conocer el estado y conservar el trabajo existente. No equivale a autorización para ejecutar el proyecto o intervenir técnicamente en él.

## Autorización para trabajo posterior

Antes de implementar o ejecutar tareas técnicas, solicitar autorización explícita al responsable, indicando alcance, archivos o componentes afectados y acciones previstas. Si la autorización es ambigua, aclararla antes de actuar.

La aprobación de un requisito, una propuesta documental o una lista de necesidades futuras no autoriza su implementación. El catálogo 002 **APROBADO PARA EJECUCIÓN INCREMENTAL** contiene 79 tareas no ejecutadas y requiere autorización posterior específica antes de cada tarea/incremento técnico: alcance, archivos/componentes, comandos y entorno. Mantener los diferidos y sus puertas antes de construir o aceptar cada componente; los gates y E(T-AUTH-nnn) siguen pendientes. Cualquier fase posterior deberá respetar el flujo SDD y los criterios de aprobación definidos en la constitución. No hacer commit ni push en esta entrega.

## Coherencia y registro

- Redactar la documentación en español y mantener enlaces relativos válidos.
- Separar requisitos confirmados, propuestas, decisiones pendientes y estado observado.
- No seleccionar proveedor cloud, motor NoSQL, capas, librerías o versiones complementarias por iniciativa propia.
- No presentar dependencias de la plantilla como decisiones definitivas.
- Registrar cambios y decisiones autorizados en `memoria.md`, manteniendo coherentes los documentos afectados.
- No afirmar que una funcionalidad, una prueba, una medida de seguridad o un despliegue existe o funciona sin evidencia.
- No introducir secretos ni datos sensibles en documentos, ejemplos o registros.
- Detenerse al completar SOL-TASK-002 y comunicar el task.md creado y los tres documentos sincronizados, 79 tareas no ejecutadas, cobertura AUTH 48/48, PLD-AUTH 25/25, AC 21/21, 8 gates pendientes y AUTH-027 NO IMPLEMENTADO / NO SATISFECHO. No ejecutar TBF/T-AUTH, implementar AUTH ni generar trabajo adicional; conservar spec/plan y el historial APR-03.

## Agentes y skills existentes y futuros

Los cinco subagentes y las cinco skills ya creados conservan sus definiciones y restricciones. Su invocación, carga, ejecución, modificación o ampliación de acceso requiere autorización expresa y respeto de los permisos reales. Cualquier incorporación futura deberá respetar la constitución y declarar su alcance, sin habilitar acciones técnicas o cambios de fase por sí misma. Este archivo no crea ni habilita ningún agente o skill.
