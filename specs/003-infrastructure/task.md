# Tareas 003 — Infraestructura de ENVER Backend

## 1. Control, autorización y estado inicial

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.51.0 / 2026-11-13 UTC |
| Solicitud inicial / incremento actual | CIERRE FORMAL E006 / T-INFRA-006 — ACCEPTANCE / DOCUMENTATION / LOCAL GIT (§42): E006 ACEPTADA / T006 COMPLETA / ACEPTADA para el alcance actual documentado, con excepción temporal de egreso y riesgo HTTP interno aceptados, no runtime ready ni mínimo privilegio final. I-A SATISFECHO; E005 ACEPTADA/T005 COMPLETA / ACEPTADA; I-B PENDIENTE / NO SATISFECHO. Solo task/memoria, validaciones locales y exactamente un commit documental autorizado; sin AWS/IAM/SG/rutas/plan/apply/SendCommand/push |
| Fuentes | [spec.md](spec.md) y [plan.md](plan.md), propuesta aprobada como base documental |
| Catálogo | 30 tareas, T-INFRA-001 a T-INFRA-030 |
| Estado vigente de tareas | 001/002/025 **COMPLETA / ACEPTADA** por aceptación humana explícita (§37); 005 **COMPLETA / ACEPTADA**, E005 **ACEPTADA** para el alcance actual con excepciones operativas temporales, no separación técnica integral (§39); 006 **COMPLETA / ACEPTADA**, E006 **ACEPTADA** para el alcance actual documentado con excepción temporal de egreso (§42); 003/004/007 PARCIAL / aceptación pendiente; 009 PARCIAL / VALIDACIÓN TÉCNICA DE HOSTS ACREDITADA (§32.15), dependencias/decisiones y aceptación integral pendientes; 008 conserva publicación/digest/pull según responsable y pendientes; 010 PARCIAL / recuperación conservadora acreditada, prueba formal pendiente; otras19 PENDIENTE / NO EJECUTADA, incluida026 |
| Estado de todos los gates | I-A **SATISFECHO** por aceptación explícita (§37); B–I **PENDIENTE / NO SATISFECHO** |
| Evidencia de ejecución de 003 | Historial §§18–37 intacto. §32.15 conserva SSM/Docker A/B, sin contenedores ni servicio Enver.API observados entonces. §38 registra auditoría IAM previa, Compute v5 efectiva tras intervención humana y plan NUEVO post-v5 de 2026-11-13T14:37:09.988Z–14:37:27.758Z: exit2, recursos0/0/0/0replacements y +outputcompute_instances; refresh con discrepancias en8recursos, sin acciones correctivas. No zero drift absoluto, no apply ni reutilización de recovery4; ninguna nueva operación AWS/plan en la entrega documental |
| Baseline documental original | `feature/authentication`, `c58ea86e3eddff4befa8f61398dd58d55c4ef985` |
| Baseline del BUILD actual | `feature/infrastructure`, creada desde `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`, árbol inicialmente limpio |
| Checkpoint previo a INFRA-2/3 | `feature/infrastructure`, `611071f0ecb0d6cb67993f4e9bffd00d328a2f90`, árbol limpio al iniciar §9 |
| Checkpoint previo a preparación IAM apply | `feature/infrastructure`, `f1a98a01e15c67297287133c27dc23279a649d09`, árbol limpio al iniciar §12 |
| Checkpoint pre-apply | `feature/infrastructure`, `0958e83b887a73e1bdae2ff8b25753f5a52d0f0c`, árbol limpio/0 staged al iniciar §14 |
| Checkpoint pre-apply ECR | `feature/infrastructure`, `2626fb10eb9b7c128c6422e92598ca9f0e78e72c`, árbol limpio/0 staged al iniciar §18 |
| Checkpoint post-apply ECR / baseline de publicación local | `feature/infrastructure`, `3312d220e3ff68851befc59e9a7cfa516185bdc8`, árbol limpio/0 staged al iniciar §20 |
| Checkpoint primera imagen / baseline INFRA-5 | `feature/infrastructure`, `25daa836c0571d4c0c02486e49ce435a108f0558`, árbol limpio/0 staged al iniciar §23; commit de exactamente publisher JSON/task/memoria |
| Checkpoint compute endurecido / baseline del replan actual | `feature/infrastructure`, `f19ca80ef899436bbe1ad724db950fc0dfa629ae`, padre25daa836, mensaje `feat(infra): prepare hardened compute baseline`, diez archivos; árbol limpio/0 staged al iniciar §30 |
| Checkpoint PRE-APPLY / baseline del apply autorizado | `feature/infrastructure`, `3730ebe2cd0c7851bcd827b1f73e355a673db61a`, padre f19ca80, mensaje `chore(infra): prepare compute pre-apply`, exactamente .gitignore/task/memoria; árbol limpio/0 staged al iniciar §31 |

Crear este catálogo **no autoriza ejecutar tareas**, cambiar código/configuración/Docker/tests, crear `infra/`, recursos AWS/Atlas, workflows o secretos, ni correr Terraform. No stage/commit/push. Los únicos archivos de esta entrega son los tres Markdown de 003. La autorización previa de la propuesta es documental, no habilita cargos ni credenciales.

El párrafo anterior conserva el alcance de la materialización inicial. La solicitud posterior explícita **BUILD T-INFRA-001 + T-INFRA-002 — BASELINE TERRAFORM** autoriza solo preparar estos dos incrementos, crear la baseline `infra/` y su lock, comprobar herramientas/credenciales sin revelar secretos, ejecutar version/fmt/init -backend=false/validate y registrar evidencia en este archivo y memoria.md. `.gitignore` solo si faltan reglas; ya existen. Autoriza crear rama, **no stage/commit/push, plan/apply, recursos AWS ni tareas 003–030**. No cambia los criterios originales de aceptación ni cierra decisiones pendientes por inferencia.

La solicitud posterior **CIERRE BASELINE AWS + TERRAFORM** autoriza validar perfil/identidad/región e inspeccionar permisos sin modificarlos, repetir version/fmt-check/validate y actualizar exclusivamente task/memoria (§7). No autoriza cerrar sin criterios satisfechos, modificar `infra/`, registrar credenciales, crear recursos/políticas, ejecutar plan/apply ni stage/commit/push. §6 conserva los resultados históricos; §7 es el estado de identidad/región vigente.

**CIERRE DE PRESUPUESTO + PREPARACIÓN INFRA-2/3** aprueba el límite operativo inicial de USD 50 con reserva objetivo mínima de USD 50 sobre USD 100 de crédito informado por el responsable (§8). Autoriza únicamente registro en task/memoria, reevaluación de criterios, recomendación documental y version/fmt-check/validate/Git de lectura. No autoriza ejecutar 003–006/025, modificar infraestructura/IAM, plan/apply, stage/commit/push o consumo AWS. §§6–7 permanecen como antecedentes; el presupuesto pendiente allí ya no es el estado vigente.

La solicitud posterior **INFRA-2 + INFRA-3 / T-INFRA-003 A T-INFRA-006 / BUILD CONTROLADO NO APPLY** autoriza expresamente preparar definiciones candidatas y bootstrap revisable, aunque 001/002/gates sigan abiertos, y ejecutar solo la parte posible de 025 sin recursos (§9). Esto permite preparación parcial, **no aceptación de dependencias incumplidas**, cierre de decisiones o provisión. Plan tiene condiciones expresas de permisos/inputs/decisiones: no se cumplen, no se ejecuta. No IAM aplicado, apply/destroy/import, pruebas backend, CI/CD ni stage/commit/push.

La solicitud posterior **INFRA-2/3 — REVALIDACIÓN IAM + TERRAFORM PLAN / VALIDACIÓN CONTROLADA NO APPLY** autoriza STS/AZ, fmt/check/validate y un intento de plan con inputs documentados; solo task/memoria como edición documental (§10). El responsable informa bootstrap aplicada; su contenido/asignación efectivos no se enumeran. Esta autorización no acepta DP-I-03 ni implica init/migración de backend, asignación IAM, apply o cierre de tareas/gates. §9 conserva la denegación y ausencia de plan de aquel turno como historial, no como resultado vigente.

La solicitud posterior **INICIALIZAR BACKEND LOCAL + PLAN INFRA-2/3 / VALIDACIÓN CONTROLADA NO APPLY** autoriza init exclusivamente local y **aprueba opción A para DISEÑO Y PLAN**, con los inputs y límites de §11. `network_design_approved=true` solo mediante input temporal; no modificación del ejemplo ni aceptación de provisión. No backend remoto/migración, IAM modificado, apply/destroy/import o cierre implícito de tareas/gates. §§9–10 conservan sus bloqueos como historial ya superado en consulta AZ/backend, no como estado vigente.

**PREPARAR IAM MÍNIMO PARA PRIMER APPLY INFRA-2/3** autoriza únicamente un JSON write separado, revisión de fuentes provider/IAM y actualización task/memoria, más fmt-check/validate. No modifica la read policy existente ni concede permisos, acepta gates o autoriza apply/destroy. El checkpoint Git anterior fue una autorización separada ya completada; en §12 no hay stage/commit/push ni llamadas a APIs de la cuenta AWS.

**VALIDAR IAM NETWORK APPLY + REPLAN PRE-APPLY** autoriza STS, DescribeNetworkAcls/DescribeNetworkInterfaces, revisión local de write policy, fmt-check/validate y un replan con los mismos inputs aprobados; solo task/memoria se editan (§13). El responsable informa asignación manual de Read/Apply. No se autorizan cambios IAM, dry-run de escritura, simulación/enumeración de políticas, init, apply/destroy/import, recursos manuales o stage/commit/push. La evidencia histórica de §12 conserva su condición de propuesta no aplicada en aquel momento; §13 registra el nuevo estado informado y las llamadas realmente verificadas.

**PRIMER APPLY CONTROLADO — NETWORK BASELINE INFRA-2/3** autoriza posteriormente **solo** generar/revisar un plan guardado de 14 creaciones y aplicar ese mismo archivo, post-plan y lecturas AWS del inventario (§14). Permite materializar red base/SG aunque la aceptación integral de tareas/gates siga pendiente: no aprueba retrospectivamente sus dependencias ni constituye despliegue de aplicación. No workloads, IAM ampliado, destroy/import, cambios de HCL o ejecución integral de 026–028; la evidencia puntual se registra aquí y en 003–006/025, sin declarar completados esos alcances. Únicas ediciones versionables autorizadas: task/memoria; state/plan locales ignorados, sin stage/commit/push. El checkpoint Git previo fue otra autorización completada; las prohibiciones de apply en §§6–13 conservan su alcance histórico.

**INFRA-4 — ECR / CONTAINER REGISTRY** autoriza posteriormente el alcance acotado de §15: registry.tf, tres outputs, propuesta IAM ECR separada, revisión Docker y build local sin push, fmt/check/validate/plan y registro task/memoria. No aplicar/adjuntar políticas, cambiar Dockerfile/producto, crear workloads/OIDC/workflows ni stage/commit/push. Preparación no acepta dependencias ni resuelve DP-I-14 integralmente; spec/plan e historial de red intactos.

**INFRA-4 ECR — APPLY CONTROLADO** autoriza después generar/revisar un plan fresco guardado de exactamente dos altas ECR y aplicarlo sin regenerar/HCL cambiado, seguido de post-plan y lecturas ECR (§18). Red14 debe ser no-op; solo task/memoria como ediciones versionables, sin push de imagen, IAM modificado, workloads, destroy/import o Git stage/commit/push. Autorización puntual no acepta dependencias/gates ni ejecución integral de026–028. Prohibiciones de apply en §§15–17 conservadas como historial; no se reutilizan sus planes ni se reforma spec/plan.

**VALIDAR LISTIMAGES + CHECKPOINT POST-APPLY** autoriza posteriormente STS/ListImages/DescribeRepositories/GetLifecyclePolicy, fmt-check/validate, sincronizar únicamente ListImages en JSON local, actualizar task/memoria y stage/commit de esos tres archivos (§19). Responsable informa IAM remoto actualizado manualmente; el agente no modifica IAM. Sin nuevo plan/apply, Docker push, push Git, workloads o aceptación automática de tareas/gates.

**PREPARAR PUBLICACIÓN CONTROLADA DE IMAGEN ECR** autoriza después revisar SDD/Dockerfile intacto, build local desde HEAD3312d220 con tag por SHA, crear únicamente publisher JSON separado, documentar flujo futuro y ejecutar fmt-check/validate (§20). No modificar provisioning/HCL/producto, AWS/IAM remoto, Docker credentials ni ejecutar login/push, plan/apply, pruebas, CI/OIDC o stage/commit/push. No acepta contratos/dependencias/gates por inferencia.

**VALIDAR PUBLISHER + PUBLICAR PRIMERA IMAGEN ECR** autoriza posteriormente identidad/ListImages, validar policy e imagen exacta, login/tag/un solo push de git-3312d220e3ff, lecturas digest/scanning y pull de la misma imagen por digest solo si permisos permiten, sin contenedor iniciado (§21). No reconstruir, segundo tag/push, IAM modificado, Terraform apply/destroy/import, workloads, OIDC ni Git stage/commit/push. AccessDenied exige detenerse, no ampliar IAM/reintentar. El intento quedó bloqueado en token antes de cualquier publicación; no se consume autorización para repetir a ciegas o pasar al siguiente paso.

**CHECKPOINT PRIMERA IMAGEN ECR PUBLICADA** autoriza después revisar/sincronizar evidencia comunicada por responsable, seguridad/exclusiones y stage/commit únicamente de publisher JSON/task/memoria (§22). No nuevas operaciones AWS/Docker/Terraform ni cambios IAM; sin push Git, amend/squash/merge/rebase o cierre de gates. No atribuir al agente una publicación posterior que esta sesión no ejecutó/verificó; conservar cronología del bloqueo§21.

**INFRA-5 — COMPUTE BASELINE / T-INFRA-009 + T-INFRA-010 y runtime de005** autoriza después preparar HCL/IAM revisables, bootstrap sin API/secretos, fmt/validate y un intento de plan sin apply (§23). EIP solo propuesta, sin NAT ni cambios silenciosos de subnets; IAM no autoasignado, sin otros servicios o Git stage/commit/push. La misma solicitud exige detener/recomendar alternativa si el diseño amenaza USD50. Se activa esa condición en análisis previo: no se crean HCL/policies ni se ejecuta Terraform/plan o AWS CLI. La autorización de preparación no aprueba duración/gasto ni supera el bloqueo para continuar automáticamente.

**INFRA-5 — APROBACIÓN DE VENTANA TEMPORAL + BUILD/PLAN** aprueba después máximo inicial48h, guardrail USD10 acumulado de ventana antes de cualquier apply y mantiene USD50 AGREGADOS/reserva objetivo50. Autoriza retomar HCL de2hosts/2EIP/role/profile/SSM/pull/bootstrap, policies locales revisables, fmt/validate/un plan y una lectura opcional de scanning (§24); **no apply/recursos/IAM remoto/otros servicios/Git stage/commit/push**. Resuelve el bloqueo de preparación§23, no costo integral/aceptación ni autorización de gasto. Plan denegado exige revisión humana, sin ampliación/reintento automático; scanning opcional no ejecutado tras denegación.

**INFRA-5 — REPLAN COMPUTE DESPUÉS DE HABILITAR AMI READ** autoriza STS/GetParameter/DescribeImages, fmt-check/validate y replan con inputs aprobados, revisión local de candidatas y una lectura opcional de scanning (§25). Responsable informa policy mínima ENVER-Terraform-AMI-Read aplicada manualmente; no se enumera IAM. GetParameter ahora exit0, pero DescribeImages denegado activa la detención explícita **antes de plan/scanning**. Solo task/memoria se editan; sin apply, IAM remoto, recursos, cambios HCL/candidatas o Git stage/commit/push.

**INFRA-5 — REPLAN COMPUTE DESPUÉS DE VALIDAR AMI** aporta validación EC2 de la AMI concreta y autoriza STS/fmt-check/validate/un plan completo con inputs anteriores, revisión de candidatas y una consulta scanning (§26). Solo task/memoria como ediciones; sin cambios HCL/IAM, recursos/apply/destroy/import o Git stage/commit/push. Plan completo ahora acreditado, no autorización de provisión/gasto, aceptación de tareas o cierre de gates.

**INFRA-5 — PREPARAR IAM COMPUTE + IAM RUNTIME PARA APPLY** autoriza después revisar las dos candidatas y runtime HCL, preparar instrucciones de aplicación exclusivamente manual, fmt-check/validate y registro de evidencia nueva (§27). **No cambia IAM/AWS/HCL/JSON, no repite plan sin cambios HCL, no apply ni Git stage/commit/push.** Preparación de una futura asignación humana no acredita su realización ni acepta riesgos o gates por inferencia.

**INFRA-5 — HARDENING IAM RUNTIME ANTES DE APPLY / PERMISSIONS BOUNDARY** adopta después límite externo administrado por humano, autoriza crear su JSON y modificar role HCL/variable/candidataIAM, fmt/check/validate y task/memoria (§28). Compute candidata sin cambios. **No acepta riesgo anterior sin límite independiente; guía§27 no habilita adjuntar candidatas todavía.** Sin ARN real aprobado no plan, sinAWS/apply/destroy/import/adjuntos/stage/commit/push. Referencia ARN determinística no acredita existencia de policy.

**INFRA-5 — POST-HARDENING PLAN CON PERMISSIONS BOUNDARY REAL** aporta después ARN real creado por administrador independiente y autoriza usarlo temporalmente, revisar HCL/IAM, fmt/check/validate y un plan (§29). Resuelve bloqueo de input/plan de§28 con fuente informada, no acredita contenido remoto o permisos efectivos por inferencia. Solo task/memoria se editan; prohibidos adjuntos de candidatas, cambios IAM, recursos, apply/destroy/import o Git stage/commit/push.

**INFRA-5 — REPLAN CON PERMISOS DE APPLY REALES** informa después creación/adjunción manual de Compute/IAM-Runtime exclusivamente a enver-terraform-local desde JSON versionados, boundary no adjunta al usuario. Autoriza STS, fmt-check/validate/bash-n y un plan con ARN real (§30), solo task/memoria como ediciones. El checkpoint Git anterior fue autorización separada completada; aquí no stage/commit/push, cambios IAM/recursos/apply/destroy/import. Adjuntos informados y plan no prueban capacidad efectiva de escrituras ni cierran gates.

**INFRA-5 — AUTORIZAR EXCLUSIÓN DE TFPLAN Y CONTINUAR PRE-APPLY** autoriza después agregar únicamente `/infra/tfplan-infra5` a .gitignore, comprobar exclusiones/validaciones, generar e inspeccionar el plan guardado y consolidar task/memoria (§30.5). Permite stage/commit exactamente de esos tres archivos, mensaje `chore(infra): prepare compute pre-apply`; no versionar plan/state/tfvars, push, apply/destroy/import, cambios IAM o recursos AWS. Resuelve solo el bloqueo de ignore del intento anterior, no autoriza provisión ni gasto.

**INFRA-5 — APPLY CONTROLADO DEL PLAN GUARDADO** autoriza posteriormente una única ejecución de infra/tfplan-infra5 con SHA256 exacto (§31), dentro de evaluación48h/guardrailUSD10. Solo tras éxito completo permite inventario/lecturasEC2-EBS-ENI-EIP-IAM/SSM/bootstrap y post-plan, sin API/dependencias/aplicación. Fallo parcial/AccessDenied exige detener sin segundoapply, destroy, IAMmanual o nuevos recursos correctivos; solo task/memoria y evidencia local, sin stage/commit/push. No aprueba retrospectivamente dependencias/gates ni convierte la estimación en factura/cap técnico.

**INFRA-5 — RECUPERACIÓN DE APPLY PARCIAL** autoriza posteriormente lecturas de recursos conocidos, state list e inspección selectiva del taint, fmt-check/validate/bash-n y un nuevo plan guardado solo si su ruta ya está excluida por Git (§32). No reutilizar el plan parcialmente consumido, aplicar recuperación, modificar .gitignore automáticamente, untaint/stateedit/import/destroy/liberar EIP, ampliar IAM o stage/commit/push. AccessDenied o recursos ajenos al alcance detienen la operación; no-op y replacements requieren plan real, no inferencia.

**INFRA-5 — AUTORIZAR EXCLUSIÓN DE RECOVERY PLAN Y CONTINUAR RECOVERY PLAN** permite añadir exclusivamente `/infra/tfplan-infra5-recovery` a .gitignore y después generar/inspeccionar ese nuevo plan, conservando todas las demás reglas y la evidencia previa (§32.5). Autoriza sincronizar task/memoria, no aplicar ni aceptar replacements, untaint/stateedit/import/destroy/liberar EIP, IAM o escrituras AWS, stage/commit/push. La planificación no reinicia la ventana de48h ni cierra gates.

**INFRA-5 — RECUPERACIÓN CONSERVADORA DE EIP TAINTED** autoriza posteriormente solo `terraform untaint` para aws_eip.api["a"]/["b"], condicionado a comprobar en AWS identidad/cuenta/región, existencia, domainvpc, allocation/IP, ausencia de asociaciones inesperadas y coincidencia con state (§32.6). Autoriza nuevo recovery2 solo si ya está ignorado y documentar task/memoria. No apply/destroy/import/liberar/recrear EIP, IAM, edición manual de state, cambio automático de .gitignore, stage/commit/push. Autorizar la corrección de marcas no acepta una recuperación completa o un plan futuro por inferencia.

**INFRA-5 — AUTORIZAR EXCLUSIÓN DE RECOVERY2 Y GENERAR PLAN LIMPIO POST-UNTAINT** permite después añadir exclusivamente `/infra/tfplan-infra5-recovery2` a .gitignore, validar, generar/inspeccionar el tercer plan y sincronizar task/memoria (§32.7). No otras reglas, apply/destroy/import/taint/untaint/stateedit/escriturasAWS/IAM/stage/commit/push. Plan real4/0/0 no es autorización para ejecutarlo ni aceptación de hosts, costos acumulados o gates.

**INFRA-5 — APPLY CONTROLADO RECOVERY2** autoriza posteriormente un único apply de `infra/tfplan-infra5-recovery2` con SHA256 exacto, condicionado a baseline y cuatro altas sin reemplazos (§32.8). Solo tras éxito permite inventario AWS/state, SSM/Docker y post-plan no guardado. Ante fallo parcial exige registrar y detener, sin segundo apply/reintento, cambios de tipo/IAM, plan correctivo, destroy/import/taint/untaint o stage/commit/push. No autoriza desplegar Enver.API ni reiniciar la ventana48h.

**INFRA-5 — CORRECCIÓN INSTANCE TYPE DESPUÉS DE RECHAZO FREE TIER** autoriza después cambiar exclusivamente t3a.small→t3.small en compute operativo y condición exacta de candidata Compute, validar y documentar (§32.9). No modifica IAM remoto. Recovery3 solo después de confirmación de actualización manual de ENVER-Terraform-Compute y comprobar exclusión Git exacta; si falta cualquiera, detener sin plan ni modificar .gitignore automáticamente. No apply/destroy/import/liberar EIP/otros atributos/stage/commit/push, ni reescribir planes/evidencia histórica.

**INFRA-5 — RECOVERY3 DESPUÉS DE CAMBIO A t3.small** confirma después actualización manual exclusiva del tipo en policyreal y permite intentar su lectura sin ampliar permisos, continuar con evidencia informada si IAMdenegado, inventariar state/AWS, validar y planear solo si recovery3 ya está ignorado (§32.10). Si falta exclusión, detener y solicitar únicamente /infra/tfplan-infra5-recovery3; no editar .gitignore automáticamente. Ningún apply o escrituraAWS/IAM, destroy/import/taint/untaint/liberar/desasociarEIP/eliminarENI/stage/commit/push.

**INFRA-5 — AUTORIZAR EXCLUSIÓN DE RECOVERY3 Y GENERAR PLAN FINAL CON t3.small** permite después añadir exclusivamente /infra/tfplan-infra5-recovery3, validar, generar e inspeccionar un único nuevo plan y sincronizar task/memoria (§32.11). GetPolicydenegado histórico no bloquea ni autoriza ampliarIAM. No otras reglas/cambiosHCL/IAM/state/apply/destroy/import/taint/untaint/liberarEIP/stage/commit/push; resultado2/0/0 no concede provisión.

**INFRA-5 — APPLY CONTROLADO RECOVERY3 /CREACIÓN FINAL DE LAS2EC2** autoriza después aplicar únicamente elbinario de§32.11 con SHA256exacto (§32.12). InventarioAWS/state, IAM/SSM/Docker y post-plan solo traséxito; fallo parcial exige registrar ydetener sin reintento/segundoapply/correccionesautomáticas/IAM/destroy/import/taint/untaint/liberarEIP/stage/commit/push. No autoriza API/dependencias o reiniciarventana48h.

**INFRA-5 — RECUPERACIÓN POST-RECOVERY3 /VALIDAR EC2 PARCIALMENTE CREADAS** autoriza después lecturas específicas de los dos IDs, state y recursos asociados, validaciones y recovery4 solo si ya está ignorado (§32.13). El responsable informa permiso ModifyInstanceAttribute añadido manualmente con ARN de instancia y tags ENVER; no habilita escrituras IAM ni prueba ese permiso por inferencia. Si falta exclusión, detener y solicitar solo /infra/tfplan-infra5-recovery4. Sin apply/destroy/import/state rm/untaint/edición state/.gitignore automático/reutilizar planes/stage/commit/push.

**INFRA-5 — RECONCILIACIÓN CONSERVADORA DE EC2 TAINTED + RECOVERY4** autoriza después exclusivamente la regla /infra/tfplan-infra5-recovery4 y untaint de aws_instance.api["a"]/["b"] condicionado a coincidencia material AWS/HCL/state (§32.14). Permite validaciones/plan/show y consulta SSM solo sin replacements; comandos de consulta Docker solo si Online. Divergencia material o acciones create/delete/replace/update de recursos detienen; no apply/destroy/import/terminate/IAMmodificado/edición manual de atributos/state o stage/commit/push. Corregir marcas no concede aceptación integral ni ejecución futura del plan.

**INFRA-5 — VALIDACIÓN FINAL SSM/DOCKER POST-RECONCILIACIÓN** autoriza después DescribeInstanceInformation filtrado a los dos IDs y AWS-RunShellScript solo donde SSM esté Online: docker --version, systemctl is-enabled/is-active docker, docker ps con nombres y listado de servicios running (§32.15). La policy de verificación es creada/adjunta manualmente según responsable, no por el agente. Sin modificar IAM/configuración, iniciar/detener contenedores/servicios o desplegar API. Plan final no guardado es opcional y no se ejecuta; no apply/destroy/stage/commit/push. Reevaluar criterios propios de tareas sin confundir baseline de hosts con aceptación de API/ALB/HTTPS o GATE-I-C.

Aplican [constitución](../../.specify/memory/constitution.md), [001](../001-backend-foundation/spec.md), [002](../002-authentication/spec.md) y el cierre local de [002 task §§82–84](../002-authentication/task.md). No se cierran GATE-G/H de 002 ni AUTH-027 integralmente.

Antes de cada ejecución futura: confirmar DP bloqueantes, autorización de alcance/archivos/comandos/entorno/costo, evidencia prevista y reversión. Los **archivos esperados** de cada tarea son destinos futuros sujetos a esa autorización, no archivos creados ahora. Las evidencias se almacenarán en ubicación protegida aprobada, no como secretos/state/logs crudos en Git.

## 2. Reglas de dependencia y evidencia

- Una dependencia exige su resultado aceptado; una referencia a otra fase no concede permiso para ejecutarla.
- T-INFRA-001–024 preparan decisiones, contratos y configuración; T-INFRA-025 valida baseline temprana y T-INFRA-026 el conjunto final. No se provisiona infraestructura hasta T-INFRA-027 con autorización posterior específica.
- T-INFRA-024 exige evidencia de readiness/forwarding de un incremento de aplicación **autorizado por separado**. Esta tarea integra/revisa evidencia; no implementa código o tests bajo autorización de 003.
- La aceptación de una configuración no demuestra que funcione en cloud. Pruebas reales, bootstrap y aceptación runtime de las fases se realizan en T-INFRA-027–030. Gates B–G son aceptación cloud, no prerrequisitos circulares que exijan cloud antes de revisar el plan de apply.
- 004 es dependencia externa futura: CI verde del mismo commit/digest, deployment manual mediante `workflow_dispatch`, permisos y smoke tests aprobados. No se crea ni ejecuta 004 aquí; su ausencia bloquea deployment, no se elude con un despliegue automático o un comando ad hoc.
- `E(T-INFRA-nnn)` identifica evidencia **futura**, no un archivo ya existente. Cada registro incluirá requisito/PLD/AC/gate, autorización, commit/digest, fecha/entorno, acciones/comandos realmente realizados, resultados/salida nativa cuando aplique, referencias saneadas, limitaciones y revisión del responsable. Credenciales, JWT, PII y state no son evidencia publicable.
- Fallos, resultados inciertos y bloqueos se conservan; no cerrar gate por reintentos ocultos ni extrapolar UT/IT/ST locales a cloud. Fallos inducidos, reprovisión, migración state y limpieza requieren permiso propio.

## 3. Gates formales

**GATE-I-A está SATISFECHO por aceptación humana explícita del responsable (§37); GATE-I-B a GATE-I-I permanecen PENDIENTE / NO SATISFECHO.** Los prerrequisitos se exigen para su **cierre**, no autorizan las tareas. Los criterios AC se detallan en spec §18. Un gate solo puede cerrarse con evidencia y aceptación explícita del responsable.

**Estado actualizado de I-A (§37): SATISFECHO. E001/E002/E025 ACEPTADAS.** La aceptación posterior del responsable completa el acto pendiente en §36, cuyo historial se conserva. No se altera el criterio de la tabla, no se acepta B/C ni se autoriza provisión o apply.

| Gate | Objeto / criterio de cierre | Tareas y evidencias necesarias | Prerrequisitos de cierre | AC |
| --- | --- | --- | --- | --- |
| GATE-I-A — SATISFECHO (§37) | Baseline Terraform validada: presupuesto, versiones, ambiente y state aprobados; validación inicial real | E(001), E(002), E(025) | Autorización técnica de baseline; sin apply implícito | AC-I-01 |
| GATE-I-B | Networking/security verificados: VPC/rutas/SG/SSM/IAM, salidas Atlas y no acceso directo público al backend/Redis | E(003–006), E(019–020), E(027–028) | GATE-I-A — SATISFECHO (§37) | AC-I-02, 09 |
| GATE-I-C | Mínimo dos EC2 reales, dos AZ, mismo digest y capacidad/recuperación verificadas | E(007–010), E(027–028) | GATE-I-A, GATE-I-B | AC-I-03 |
| GATE-I-D | ALB distribuye entre ambas, health/readiness, drenaje y unhealthy/todos-unhealthy seguros | E(011–012), E(023–024), E(027–028) | GATE-I-B, GATE-I-C | AC-I-04, 11 |
| GATE-I-E | HTTPS público válido, DNS/ACM/listeners y transporte/proxy resueltos sin spoofing | E(013–014), E(023–024), E(027–028) | GATE-I-B, GATE-I-C, GATE-I-D | AC-I-05 |
| GATE-I-F | Atlas/Redis/secrets/authority integrados, índices/ACK, continuidad/fail-closed y bootstrap/rotación probados | E(015–020), E(023–024), E(027–028) | GATE-I-B, GATE-I-C | AC-I-06–09, 11 |
| GATE-I-G | Observabilidad operativa: señales, privacidad, capacidad Redis, alarma/destinatario y retención | E(021–022), E(027–028) | GATE-I-C, GATE-I-F | AC-I-10 |
| GATE-I-H | Infra cloud reproducible: IaC revisada, state protegido, drift/reproducción/rollback y evidencia verificable | E(002), E(008), E(025–030) | GATE-I-A a GATE-I-G | AC-I-12, 13, 15 |
| GATE-I-I | URL HTTPS pública sirve Register/Login/Logout desde dos instancias, controles distribuidos y negativos verificados; deploy manual trazable | E(027–030) y contrato aprobado 004 | GATE-I-A a GATE-I-H | AC-I-13–15 |

Notación abreviada: E(001) = E(T-INFRA-001); E(003–006) exige cada una de las cuatro evidencias. §§6–32 registran resultados y límites distinguiendo observación directa/evidencia informada/propuesta; ninguna E parcial satisface por sí sola un gate. I-H/I-I no implican SLA, producción lista ni cierre automático de AUTH-027 o gates de 002.

## 4. Catálogo de tareas

### T-INFRA-001 — Cuenta, región, alcance y presupuesto

- **Estado:** COMPLETA / ACEPTADA por el responsable del proyecto (§37).
- **Fase:** INFRA-1 Baseline.
- **Objetivo:** fijar límites del ambiente cloud académico y responsables antes de cargos.
- **Dependencias:** autorización posterior del responsable; DP-I-01 y DP-I-13.
- **Trazabilidad:** INFRA-001–003, 033, 038; PLD-INFRA-001/020; AC-I-01, 15.
- **Archivos esperados:** decisiones autorizadas en `specs/003-infrastructure/spec.md` y `plan.md`; evidencia protegida externa.
- **Acciones futuras:** confirmar cuenta/región/AZ, acceso, nomenclatura, duración, presupuesto máximo, precios/créditos vigentes y propiedad de recursos. Estimar todos los servicios, sin asumir Free Tier.
- **Aceptación:** alcance, presupuesto y responsables aprobados por escrito; lista de cargos y recursos fuera de presupuesto, sin secretos.
- **Evidencia:** E(T-INFRA-001), aprobación y estimación fechada/regional saneada.
- **Resultado actual:** identidad/perfil/región, crédito informado USD 100, presupuesto USD 50/reserva objetivo USD 50 acreditados (§§7–8); §9 confirma proyecto enver/ambiente dev/us-east-1 y estimaciones parciales de red/IP; §10 acredita seis AZ reales disponibles. Faltan estimación integral por duración/uso, cobertura/vigencia del crédito, responsables/ownership/custodia, calendario y aceptación de decisiones restantes aplicables. No se satisface la aceptación completa ni se vuelve a marcar pendiente el monto aprobado.
- **Riesgo INFRA-5:** §23 estima compute candidato USD36.348/730h y subtotal futuro con ALB/IPv4 USD60.073/730h, aún sin Redis/demás dependencias. No convierte USD50 agregado en mensual ni autoriza730h; sin duración/estimación integral aprobadas, se detiene antes de HCL/plan. Monto aprobado conservado, aceptación integral pendiente.
- **Ventana aprobada posteriormente:** §24 máximo48h y guardrail USD10 de ventana, no presupuesto mensual ni autorización de apply; compute≈USD2.3915, con ALB/1LCU promedio≈4.3355, bolsa provisional otros2+contingencia1→7.3355. Hay margen nominal de preparación, no cotización integral/Billing/reserva garantizada; aceptación001 permanece pendiente.
- **Consolidación §33:** cuenta AWS ENVER, us-east-1/dev/enver, owner operativo Terraform **operador autorizado del proyecto**, USD50 agregados/guardrailUSD10 y ventana original máxima2026-11-15T01:57:56.9337244Z confirmados sin reinicio. Nominal compute vigente≈USD2.5931/48h, sin descuentoFreeTier; lista de exclusiones y costo integral/aceptación restantes explícitos. E001 consolidada **PARCIAL**, no negar presupuesto/owner ya confirmados ni afirmar cotización integral.
- **Delimitación §34:** estimación separa recursos vigentes (compute/EBS/IPv4, ECR y transferencia aplicable) de servicios futuros no implementados ni cargados por inferencia. Costos variables y aceptación siguen pendientes; no se exige presupuestar como consumo actual ALB/Redis/Atlas/servicios futuros para cerrar la baseline local ni se inventan tarifas/volúmenes.
- **Reevaluación §35:** owner/presupuesto/guardrail/ventana original confirmados de nuevo por el responsable. El cierre de cifrado no aporta cotas/estimación aceptada de almacenamiento ECR y transferencia del alcance vigente: E001 sigue **PARCIAL**, no íntegramente acreditada ni bloqueada por desplegar servicios futuros. No sumar etapas futuras como consumo actual.
- **Consolidación posterior §36:** modelo documental comunicado expresamente por el responsable completa ECR storage≈USD0.0007/48h y transferencia ECR→EC2 same-region≈USD0 esperado; total nominal≈USD2.594/48h. Internet VARIABLE/NO ACREDITADO dentro de guardrailUSD10, sin depender de100GBmensuales sin cargo; servicios futuros requieren estimación por etapa. Owner operativo/presupuesto, duración y alcance definidos. Resuelve preparación técnica E001, no aceptación formal ni Billing real.
- **Aceptación explícita §37:** **E001 ACEPTADA / T001 COMPLETA / ACEPTADA** sobre modelo, alcance, presupuesto, owners y duración documentados, incluidos límites y riesgo variable; no Billing real ni autorización de nuevas etapas por margen nominal. Supera el estado de aceptación pendiente de registros anteriores sin borrar su historial.
- **Riesgo:** cargos o exposición de datos de cuenta; una alarma no limita gasto.
- **Gate asociado:** GATE-I-A; soporte de GATE-I-H/GATE-I-I.

### T-INFRA-002 — Baseline Terraform, versiones y state

- **Estado:** COMPLETA / ACEPTADA por el responsable del proyecto (§37), en baseline/custodia local; dependencia001 aceptada.
- **Fase:** INFRA-1 Baseline.
- **Objetivo:** definir base IaC reproducible y custodia del state.
- **Dependencias:** T-INFRA-001; resolver DP-I-02, DP-I-12/13 aplicables.
- **Trazabilidad:** INFRA-029–033, 038; PLD-INFRA-015/016; AC-I-01, 12.
- **Archivos esperados:** futuros `infra/main.tf`, `providers.tf`, `versions.tf`, `variables.tf`, `outputs.tf`; configuración backend según decisión aprobada. State nunca versionado.
- **Acciones futuras:** aprobar versiones/provider AWS, validar naming/tags/inputs/outputs y definir state local protegido, backup cifrado/operador único. Planear backend remoto S3/locking compatible antes de GitHub/colaboración.
- **Aceptación:** baseline revisada, versiones y manejo de state aprobados; ningún secreto en tfvars/output; no se crea bucket/tabla automáticamente.
- **Evidencia:** E(T-INFRA-002), revisión de baseline y procedimiento de respaldo/migración/recuperación.
- **Resultado actual:** baseline Terraform 1.17.1/provider 6.73.0 conservada; §14 creó state serial15/14managed sin .backup entonces. §18 actualiza state local a serial18/16managed tras apply ECR y observa .backup serial15/14managed; ambos ignorados, no custodia/cifrado/recuperación aprobados por su mera existencia. T-INFRA-001 continúa sin aceptación; DP-I-12/13 y evidencia integral de protección/backup/recovery pendientes. No COMPLETADA.
- **Apply parcial §31:** state ahora serial27/24managed,2EIPtainted; backup local serial18/16managed. No restaurar backup anterior al incremento ni usar plan parcialmente consumido; custodia/backup protegido/recovery siguen sin aceptar.
- **Untaint autorizado §32.6:** state serial29/24managed/0tainted; backup nativo serial28/24managed, donde B aún estaba tainted. Son cambios nativos de Terraform, no edición manual; no restaurar un backup que revierta correcciones ni asumir custodia/recuperación integral aceptadas.
- **Custodia local §33:** Terraform1.17.1/AWS6.73.0 y lock/ejemplo versionados verificados; state39/28managed+3data/0tainted y backup38/Btainted conservados. Copia actual serial39 independiente, soloACLoperador/SYSTEM, hash y state-list de copia recuperada válidos. **Sin cifrado acreditado y con ACL amplias del state operativo**, no manejo integral aprobado: PARCIAL. Remoto diferido solo mientras uso local exclusivo; obligatorio antes de compartir/GitHub.
- **Hardening §34:** corrige únicamente DACL de ambos archivos operativos autorizados: dos Allow explícitos OPERADOR/SYSTEM FullControl, sin herencia/grupos amplios, propietarios y hashes originales intactos. Resuelve ese hallazgo de§33; sigue **PARCIAL** por cifrado y recuperación de backup cifrado no ejecutados ante ausencia de canal interactivo seguro. Retención/owner definidos; no borrar copia plaintext antes de validación del .enc.
- **Recuperación cifrada §35:** .enc39/hash esperado existente, ACL explícita OPERADOR+SYSTEM/sin herencia. Operador confirma descifrado/state-list31/28managed+3data; agente comprueba hash/JSON/serial/conteos/lineage del restaurado y elimina únicamente los dos plaintext autorizados. Custodia conserva solo .enc; state39/nativo38 intactos. Bloqueadores de cifrado/recuperación **resueltos**, E002 técnicamente acreditada localmente, sin aceptación integral/dependencia001 por inferencia.
- **Preparación de aceptación §36:** E001 ya preparada técnicamente con el modelo del responsable; E002 mantiene acreditación técnica y aceptación explícita pendiente. No reabrir cifrado/recuperación/ACL resueltos ni repetir operaciones de custodia. Backend remoto sigue diferido hasta antes de compartir/GitHub/runner, sin migración.
- **Aceptación explícita §37:** **E002 ACEPTADA / T002 COMPLETA / ACEPTADA** sobre baseline y custodia local acreditadas. Se acepta dependencia001, sin eliminar obligación de backend remoto protegido/versionado/con locking antes de colaboración/GitHub/runner; state nunca Git ni publicación de contenido. No nueva prueba de cifrado/restore ni migración.
- **Riesgo:** pérdida o filtración state, lock incompatible y confusión con la authority DynamoDB.
- **Gate asociado:** GATE-I-A, GATE-I-H.

### T-INFRA-003 — VPC, subnets y rutas

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-2 Networking.
- **Objetivo:** expresar topología simple en dos AZ con exposición deliberada.
- **Dependencias:** T-INFRA-001, T-INFRA-002, T-INFRA-025; DP-I-03.
- **Trazabilidad:** INFRA-007, 033; PLD-INFRA-004/015; AC-I-02.
- **Archivos esperados:** futuros `infra/networking.tf`, `variables.tf`, `outputs.tf`.
- **Acciones futuras:** definir CIDR, subnets públicas/privadas, route tables e Internet Gateway; distinguir ALB, hosts y Redis. No aplicar todavía.
- **Aceptación:** topología revisada en dos AZ, sin solapamientos y con rutas/propósitos verificables; funcionamiento real queda para 027–028.
- **Evidencia:** E(T-INFRA-003), diagrama y revisión IaC/red.
- **Resultado actual:** §14 acredita autorización posterior y creación/verificación AWS de VPC 10.20.0.0/16, subnets .0/.1 /24 en use1-az1/use1-az2, IGW, tabla, default route y dos asociaciones; post-plan sin cambios. Avance real cloud, no solo definido/planificado. Falta aceptación de dependencias 001/002/025 y revisión de solapamientos externos; DP-I-03 no se cierra integralmente por este apply acotado. Estado PARCIAL conservado.
- **Riesgo:** subnets privadas sin salida o exposición accidental por rutas.
- **Gate asociado:** GATE-I-B.

### T-INFRA-004 — Salida estable y acceso Atlas/AWS

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-2 Networking.
- **Objetivo:** habilitar egreso seguro y reproducible sin Atlas abierto globalmente.
- **Dependencias:** T-INFRA-003; DP-I-03/06.
- **Trazabilidad:** INFRA-008, 009, 014; PLD-INFRA-004/008; AC-I-02, 06.
- **Archivos esperados:** futuros `infra/networking.tf`, `security.tf`, `variables.tf`, `outputs.tf`, solo referencias no secretas.
- **Acciones futuras:** comparar EC2 con salida fija frente a privada/NAT/endpoints y sus costos; definir DNS y salidas autorizadas para Atlas, ECR, SSM, logs, secretos y authority.
- **Aceptación:** perfil aprobado, rutas/salidas y allowlist especificadas sin `0.0.0.0/0` en Atlas; prueba de conectividad real reservada a 028.
- **Evidencia:** E(T-INFRA-004), comparación de costo/red y reglas propuestas saneadas.
- **Resultado actual:** §14 materializa IGW/ruta y egreso SG provisional sin EIP/NAT/compute. Dos EIP futuras conservan solo aprobación de diseño/plan; EIP y allowlist Atlas /32 no implementadas ni autorizadas para apply. Faltan costo/egreso integral, decisiones restantes DP-I-03/06 y prueba de conectividad externa estable; red creada no acredita Atlas accesible. Estado PARCIAL conservado.
- **Diseño/plan posterior:** §24 incorpora2EIP y2ENI persistentes/asociaciones al HCL y salida parcial del plan, sin crearlas ni modificar subnets. /32Atlas y conectividad real siguen pendientes; no autorización de apply o aceptación004 integral.
- **Apply parcial §31:** existen2ENI y2EIP asignadas segúnapply/state, EIPtainted por lectura denegada, sin asociaciones/hosts o egreso probado; Atlas no configurado. No E004 integral ni conectividad aceptada.
- **Riesgo:** IP cambiante, NAT/IPv4 costosos o dependencia sin conectividad.
- **Gate asociado:** GATE-I-B, soporte GATE-I-F.

### T-INFRA-005 — Roles y permisos mínimos

- **Estado:** COMPLETA / ACEPTADA; E(T-INFRA-005) / E005 ACEPTADA por el responsable para el alcance actual con excepciones operativas temporales (§39), no separación técnica integral ni INFRA-024 integralmente satisfecho.
- **Fase:** INFRA-3 Security/IAM.
- **Objetivo:** separar provisión, deployment, runtime, administración y bootstrap.
- **Dependencias:** T-INFRA-003, T-INFRA-004; DP-I-08/09 y contrato preliminar 004.
- **Trazabilidad:** INFRA-008, 021, 022, 024; PLD-INFRA-005/011/012; AC-I-02, 08, 09.
- **Archivos esperados:** futuros `infra/security.tf`, `authority.tf`, `secrets.tf`, referencias de registry/compute.
- **Acciones futuras:** inventariar llamadas reales; diseñar IAM por recurso/acción, roles temporales/SSM y límites OIDC para futuro deployment. No crear trust/workflows ahora.
- **Aceptación:** revisión de mínimo privilegio y separación de roles; runtime sin permisos de creación/borrado de tabla ni administración global.
- **Evidencia:** E(T-INFRA-005), matriz rol→acción→recurso y revisión.
- **Resultado actual:** §14 acredita apply de red y post-plan sin AccessDenied con IAM Read/Apply configuradas previamente por el responsable; permisos efectivos suficientes para las operaciones ejercitadas, no auditoría global ni prueba de destroy/rollback. Policies locales/remotas no modificadas por el agente. IAM runtime/instance profile/SSM y separación deployment/authority/secretos/004 pendientes. Estado PARCIAL conservado.
- **Análisis runtime posterior:** §23 propone trust EC2/profile/ECR pull-only y evalúa SSM managed frente a policy reducida, sin definirlos en Terraform/JSON ni adjuntarlos por detención previa de costo. No SSM funcional, IAM mínimo validado o separación integral aceptada. Conserva PARCIAL / aceptación pendiente.
- **Preparación autorizada posterior:** §24 define role/profile,2inline policies (SSM reducido5acciones/ECR pull4acciones) y JSON Terraform separados; sin attachment managed o IAM remoto. Validación local exit0 no acredita permisos efectivos/SSM funcional/secrets/authority/004. Plan falla en ssm:GetParameter público antes de resolver AMI; PARCIAL / aceptación pendiente.
- **Revalidación §25:** lectura pública SSM ahora acreditada; ec2:DescribeImages denegado antes del replan. Candidatas sin modificar/adjuntar, mínimo faltante de solo lectura documentado. Runtime y separación integral no acreditados; PARCIAL conservado.
- **Replan §26:** role/profile/dosinline policies incluidos en plan completo, candidatas revisadas intactas, PassRole exacto/EC2. Sin AccessDenied de plan, pero no prueba de CreateRole/PutRolePolicy/PassRole/SSM o aislamiento efectivo; PARCIAL / aceptación pendiente.
- **Revisión manual §27:** JSON válidos, scope y acciones concretas inventariados, customer-managed recomendadas por tamaño/separación. Instrucciones manuales condicionadas a revisión humana de escalada PutRolePolicy+PassRole y cuotas; no mínimo privilegio integral certificado, IAM efectivo auditado o políticas adjuntas. PARCIAL / aceptación pendiente.
- **Hardening §28:** boundary9acciones/denegaciones explícitas preparada fuera deTerraform, role con ARN obligatorio, CreateRole/PutRolePolicy condicionados y anti-retiro/edición boundary en candidata. No límite efectivo aplicado/AWS validado; adjunción de candidatas suspendida, PARCIAL / HARDENING IAM PENDIENTE DE APLICACIÓN.
- **Plan §29:** boundary real/custodia según responsable, role con ARN exacto en plan POST-HARDENING12/0/0. Candidatas sin adjuntar, sin role creado o evaluación efectiva de mutaciones/IAM agregado. PARCIAL / HARDENING IAM PENDIENTE DE APLICACIÓN; evidencia de plan no cierre de aceptación.
- **Replan §30:** Compute/IAM-Runtime adjuntas manualmente según responsable, sin boundary en usuario Terraform. STS correcto y nuevo plan12/0/0 con boundary en runtime role; no enumeración remota, prueba de CreateRole/PassRole o aceptación de seguridad efectiva. Runtime todavía no provisionado; PARCIAL conservado, estado de adjunción anterior histórico.
- **Plan guardado §30.5:** role y boundary exactos inspeccionados en binario, inline9acciones conservadas y hardening operador local sin cambios; no auditoría remota ni prueba de escrituras/runtime. PARCIAL / PRE-APPLY ACREDITADO, no cierre de005.
- **Apply parcial §31:** role/profile/dosinline policies completaron creación; boundary exacta registrada por provider enstate. Sin EC2/profile asociado a hosts/SSM, prueba de ECRpull, custodia/admin remota o separación004 integral. Acceso denegado operador a ec2:DescribeAddressesAttribute detiene turno, no ampliarIAM. PARCIAL, criterios de005 no cerrados.
- **Lecturas de recovery §32:** GetRole/GetInstanceProfile/GetRolePolicy confirman role, boundary exacta, profile con role y las dos inline policies existentes. DescribeAddressesAttribute ya termina exit0 tras corrección manual informada por responsable; contenido de policy operadora y custodia de boundary no auditados. Sin EC2/SSM/ECRpull/plan no-op o aceptación integral.
- **Apply recovery2 §32.8:** IAM4 no-op en plan autorizado; no modificaciones IAM ejecutadas. Ambas EC2 rechazadas por elegibilidad Free Tier, sin perfil asociado a un host ni SSM/Docker acreditados. No se repiten consultas IAM tras fallo parcial; verificación remota previa no es evidencia fresca ni aceptación de separación integral/secrets/authority/004. PARCIAL, no cerrar005.
- **Corrección §32.9:** candidata Compute limita ahora ec2:InstanceType exactamente a t3.small, sin ampliar acciones/recursos/tipos. ENVER-Terraform-Compute real requiere actualización manual y confirmación; no consultada/modificada por el agente. Runtime/boundary intactos; validación integral/SSM siguen pendientes, PARCIAL.
- **Retoma §32.10:** responsable confirma actualización manual delSIDexacto a t3.small; GetPolicy exit254/AccessDenied impide verificar versiónactiva. Se conserva fuente informada, sin solicitar permisos. Runtime4/boundary presentes enstate, no nuevo IAMno-op por plan inexistente ni pruebaSSM/permisosefectivos. PARCIAL.
- **Recovery3 §32.11:** IAMruntime4/boundaryexacta no-op en plan real; policyAWS actualizada según responsable, no verificada independientemente. No repetir GetPolicy/ampliar permisos, sin nuevo AccessDenied. Plan no acredita RunInstances/SSM/aislamiento integral efectivos; PARCIAL.
- **Apply recovery3 §32.12:** dosIDEC2 asignados; operadorenver-terraform-local recibe UnauthorizedOperation/ec2:ModifyInstanceAttribute al configurar shutdownbehavior. No ampliaciónIAM/cambioHCL/consultaSSM o perfilesefectivos acreditados; runtime4 era no-op, no aceptaciónintegral. PARCIAL, no cerrar005.
- **Lecturas §32.13:** GetRole/GetInstanceProfile y EC2 confirman runtime role/boundary exacta y profile asociado a ambos hosts; shutdown stop por DescribeInstanceAttribute. Corrección ModifyInstanceAttribute informada, no escritura/simulación/versionado remoto de policy verificado. SSM y separación/aceptación integral pendientes; PARCIAL.
- **Reconciliación §32.14:** role/profile/boundary nuevamente verificados y IAM4 no-op; ssm:DescribeInstanceInformation denegado al usuario Terraform, no demuestra fallo del role runtime ni ausencia de agentes. Sin nueva ampliación IAM, SSM Online/Docker/ECRpull o mínimo privilegio integral acreditados; PARCIAL.
- **Validación final §32.15:** DescribeInstanceInformation/SendCommand/GetCommandInvocation exitosos, SSM Online y ejecución de lectura en ambos hosts acreditados tras policy humana informada. Esto resuelve el bloqueo operativo de §32.14, no audita todas las policies/condiciones ni acepta separación deploy/runtime/authority/secretos o mínimo privilegio integral. PARCIAL.
- **Reevaluación posterior §38:** inventario/agregación IAM, boundary efectiva y auditoría de escalada acreditados en evidencia anterior; Compute v5 exacta y nuevo plan post-v5 resource no-op acreditados. El responsable acepta ahora clasificación ADMINISTRACIÓN REMOTA CONTROLADA, excepción SSM y matriz actual; no acepta automáticamente E005 integral. Separación técnica provisioner/publisher/SSM incompleta con excepción temporal aprobada; runtime separado/boundary acreditados. Aceptación final de E005/tarea pendiente; dependencias originales y alcances futuros Secrets/Authority/004 no se declaran satisfechos. Los estados PARCIAL de líneas anteriores son historia, no bloqueadores IAM/Compute vigentes.
- **Aceptación formal posterior §39:** el responsable acepta explícitamente la evidencia acumulada E005 y el cierre de T005 para el alcance actual, incluidas las excepciones SSM/separación y matriz; **E005 ACEPTADA / T005 COMPLETA / ACEPTADA**. Supera la aceptación pendiente de §38, sin alterar sus fuentes ni aceptar dependencias/alcances futuros por inferencia. GATE-I-B sigue PENDIENTE / NO SATISFECHO; siguiente foco T006, sin ejecución autorizada.
- **Riesgo:** privilegio excesivo, trust abierto o secretos de cuenta persistentes.
- **Gate asociado:** GATE-I-B, GATE-I-F.

### T-INFRA-006 — SG, administración y transporte interno

- **Estado:** COMPLETA / ACEPTADA; E(T-INFRA-006) / E006 ACEPTADA por el responsable para el alcance actual documentado (§42), con excepción temporal de egreso; no egreso final de mínimo privilegio ni GATE-I-B satisfecho.
- **Fase:** INFRA-3 Security/IAM.
- **Objetivo:** evitar bypass de ALB y administración pública.
- **Dependencias:** T-INFRA-004, T-INFRA-005; DP-I-03/05/09.
- **Trazabilidad:** INFRA-008, 009, 013, 024; PLD-INFRA-004/005/007; AC-I-02, 05, 09.
- **Archivos esperados:** futuros `infra/security.tf`, `networking.tf`, `load-balancer.tf`.
- **Acciones futuras:** definir ingreso backend solo SG ALB, Redis privado/solo clientes aprobados, egreso y acceso SSM; resolver TLS interno y puertos con criterio de riesgo.
- **Aceptación:** reglas revisadas sin SSH público ni administración Docker; matriz de pruebas de acceso directo/denegación prevista para 028.
- **Evidencia:** E(T-INFRA-006), revisión SG/egreso y decisión TLS interno.
- **Resultado actual:** §14 acredita dos SG y cuatro reglas reales/verificados con AWS CLI: ALB solo ingress TCP443 IPv4 público y egress TCP8080 a SG backend; backend solo ingress TCP8080 desde SG ALB, sin SSH/ingress público de aplicación. Egreso backend IPv4 amplio provisional. Sin hosts/ALB o pruebas activas de bypass, SSM runtime, TLS interno o egreso fino aceptado. DP-I-05/09 y aceptación de dependencias pendientes; estado PARCIAL conservado.
- **Actualización posterior §40:** auditoría read-only previa confirma ambas EC2/ENI solo con SG backend, denegación de nuevo ingreso público/8080 y lateral8080 por reglas, EIP/rutas y egreso amplio. El responsable aprueba ahora **HTTPS443 externo / HTTP8080 interno**, sin HTTPS interno para este incremento, con riesgo residual aceptado; contratos de destino Atlas y segmentación Redis definidos, matriz T028 prevista. Egreso objetivo propuesto y excepción temporal de egreso **PROPUESTA / NO ACEPTADA**, distinta de las excepciones SSM/separación de E005 ya aceptadas. T004/decisiones aplicables y aceptación E006 no se cierran por inferencia; no se exige desplegar ALB/Atlas/Redis para revisar sus contratos. La línea §14 conserva su estado histórico, no niega la evidencia posterior.
- **Aceptación posterior §41:** responsable acepta explícitamente mantener temporalmente el egreso actual solo para preparación/integración, con ocho controles, plazo original y endurecimiento previo al runtime final. Resuelto el bloqueo de aprobación de egreso de §40; objetivo futuro definido, no implementado. Segmentación/transporte/contratos/matriz acreditados en alcance actual; falta aceptación explícita integral E(T-INFRA-006), no nuevas pruebas o remediaciones IAM/Compute. Dependencias originales y otros criterios de gate conservados, sin exigir servicios futuros completos para acreditar este diseño.
- **Cierre formal §42:** aceptación humana explícita integral de E006 y T006 para ese alcance actual; supera la aceptación pendiente de §41 sin reescribir historia. Incluye concordancia AWS/HCL en alcance auditado, contratos definidos y matriz prevista, no pruebas activas ni servicios futuros operativos. T004/DP integrales y obligaciones posteriores conservan sus tareas; no reapertura E005/Compute, extensión de plazo, gasto adicional ni cierre de I-B.
- **Riesgo:** API directa, Redis público, egress indiscriminado o TLS asumido por EXPOSE.
- **Gate asociado:** GATE-I-B, GATE-I-E.

### T-INFRA-007 — Registry ECR

- **Estado:** PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE.
- **Fase:** INFRA-4 Registry.
- **Objetivo:** definir repositorio de artefactos inmutables sin introducir pipeline.
- **Dependencias:** T-INFRA-002, T-INFRA-005, T-INFRA-025; DP-I-14.
- **Trazabilidad:** INFRA-005, 024, 031; PLD-INFRA-003/015; AC-I-03, 12, 13.
- **Archivos esperados:** futuros `infra/registry.tf`, `variables.tf`, `outputs.tf`, políticas en `security.tf`.
- **Acciones futuras:** definir ECR, acceso push/pull, retención compatible con rollback y referencias por digest; distinguir rol build/deploy/runtime.
- **Aceptación:** configuración revisada y output no secreto; no imágenes publicadas ni registry creado por la preparación.
- **Evidencia:** E(T-INFRA-007), revisión IaC y contrato de permisos/retención.
- **Resultado actual:** §18 acredita repository real enver-dev-api/lifecycle creados con apply exacto 2/0/0 exit 0 y post-plan 0/0/0; red14+ECR2 no-op. §19 reconfirma URI/ARN, IMMUTABLE/AES256/repository scanOnPush=true y regla solo untagged >14 días; ListImages ahora exit 0 / imageIds=[]: 0 imágenes, ningún digest en ese repository al consultar. Registry BASIC/rules=[] observado en§18, no consultado otra vez ni prueba de scanning de imagen. JSON local sincroniza solo ListImages tras actualización manual remota informada. Avance cloud real, sin probar destroy/rollback o imagen escaneada ni aceptar dependencias002/005/025, retención/contrato/DP-I-14. Estado PARCIAL conservado, no mera definición ni COMPLETADA.
- **Revalidación pre-push:** §21 ListImages exit 0 confirma otra vez imageIds=[]/0 imágenes antes del intento de token denegado; sin push ni cambios de configuración. Conserva PARCIAL y aceptación pendiente, no se acredita imagen/digest/scanning.
- **Actualización posterior:** §22 registra primera publicación y pull por digest informados por responsable, 1 tag/3 objetos OCI y scanning linux/amd64 IN_PROGRESS. No verificación cloud nueva de repository/configuración ni aceptación de dependencias/retención/contrato; T-INFRA-007 permanece PARCIAL / aceptación pendiente.
- **Riesgo:** borrado del digest de rollback, tags mutables o permisos amplios.
- **Gate asociado:** GATE-I-C, GATE-I-H.

### T-INFRA-008 — Contrato de imagen y ownership del digest

- **Estado:** PARCIAL / PUBLICACIÓN, DIGEST Y PULL ACREDITADOS SEGÚN RESPONSABLE; SCANNING FINAL, ROLLBACK Y ACEPTACIÓN PENDIENTES (§22).
- **Fase:** INFRA-4 Registry.
- **Objetivo:** garantizar mismo artefacto reproducible y promoverlo manualmente desde 004.
- **Dependencias:** T-INFRA-007; DP-I-02/14, contrato aprobado con futura 004.
- **Trazabilidad:** INFRA-005, 006, 034, 035; PLD-INFRA-003/017/018; AC-I-03, 13.
- **Archivos esperados:** contrato futuro en `specs/003-infrastructure/plan.md` y referencias `infra/compute.tf`/`variables.tf`. Dockerfile y workflows fuera de esta tarea.
- **Acciones futuras:** revisar Dockerfile existente, .NET 10/non-root/configuración; definir commit→digest→CI verde, writer único del digest y retención previa. Solicitar aparte cualquier cambio Docker necesario.
- **Aceptación:** contrato aprobado de artefacto inmutable idéntico en A/B y deploy workflow_dispatch tras CI verde; sin build, push o YAML implícitos.
- **Evidencia:** E(T-INFRA-008), contrato y revisión documental de imagen.
- **Revisión inicial:** §15 registra Dockerfile intacto, build local autorizado exit 0, metadatos linux/amd64, .NET/ASP.NET 10.0.12, USER 1654 non-root, HTTP_PORTS=8080 y ENTRYPOINT dotnet Enver.API.dll. En aquella entrega sin arranque, push/digest ECR ni CI acreditados. Contrato URI@digest y propuesta de escritor único 004 documentados aquí sin modificar plan; aprobación DP-I-02/14/004, bases fijadas por digest, evidencia de seguridad y aceptación pendientes. No COMPLETADA.
- **Extensión observada:** §18 crea repository/lifecycle sin publicar imagen/tag; §19 supera la denegación ListImages con exit 0 y acredita repository vacío: 0 imágenes/ningún digest al consultar. Siguen faltando publicación/digest/validación ECR de imagen/CI/rollback por digest y aceptación contractual. T-INFRA-008 permanece PARCIAL, sin repetir build o modificar Dockerfile/004.
- **Preparación posterior:** §20 acredita otro build local desde HEAD3312d220 limpio, tag **enver-api:git-3312d220e3ff**, exit 0, Linux/amd64/non-root1654/.NET10.0.12, y prepara publisher JSON separado/no aplicado. Flujo futuro login/push/digest/scanning documentado, no ejecutado. Pendientes policy aplicada/login/push/digest/scanning/pull por digest/rollback y contrato/ownership/004; PARCIAL conservado, sin modificar Dockerfile o provisioning.
- **Intento autorizado posterior:** §21 publisher creada/adjunta manualmente según responsable, JSON/hash intactos y local image ID/plataforma coinciden. STS/ListImages exit 0/0 imágenes, pero **GetAuthorizationToken exit 254/AccessDenied** en Resource=*, sin Docker login/tag/push/reintento/IAM modificado. Adjunción informada no acredita permiso efectivo: publicación/digest/scanning/pull/rollback y aceptación integral siguen pendientes. Conserva PARCIAL con bloqueo operativo explícito; no COMPLETADA.
- **Resultado vigente:** §22 registra evidencia posterior **informada por responsable**: push/pull por digest exit 0, tag git-3312d220e3ff y digest ECR **sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda**, pushedAt **2026-11-12T21:36:45.185000Z**, tamaño **101033417bytes**; 1 tag y3objetos OCI, no3versiones. Scanning **IN_PROGRESS** para linux/amd64, sin conteos de findings disponibles; rollback pendiente por ausencia de digest anterior. Publicación/digest/pull acreditados según esa fuente, no ejecutados ni revalidados por el agente en el checkpoint. Scanning final/rollback/seguridad/reproducibilidad/contrato004/aceptación integral pendientes; PARCIAL conservado.
- **INFRA-5:** §23 no consulta scanning; IN_PROGRESS sigue siendo última evidencia informada, no estado actual revalidado. No despliega ni actualiza digest/imagen, no findings=0, conserva pendientes y PARCIAL.
- **Retoma INFRA-5:** §24 tampoco consulta scanning tras AccessDenied del plan. Última evidencia informada IN_PROGRESS, no estado fresco ni findings=0; imagen/digest/pull/rollback/aceptación de§22 conservados, sin nuevo deployment.
- **Revalidación §25:** tampoco se consulta scanning tras denegación DescribeImages; cero llamadas, última evidencia informada IN_PROGRESS, sin findings=0 o aceptación nueva. PARCIAL conservado.
- **Consulta §26:** una llamada DescribeImageScanFindings al digest registrado devuelve exit254/ScanNotFoundException; sin findings/estado final. No se atribuye al manifiesto linux/amd64 de digest no comunicado, cuyo IN_PROGRESS sigue histórico/informado. Publicación/rollback/aceptación conservan sus límites; PARCIAL.
- **Riesgo:** drift Terraform/CD, imagen no rastreable o secretos en capas.
- **Gate asociado:** GATE-I-C, GATE-I-H, GATE-I-I.

### T-INFRA-009 — Configuración de dos instancias

- **Estado:** PARCIAL / VALIDACIÓN TÉCNICA DE HOSTS ACREDITADA; DEPENDENCIAS, DECISIONES Y ACEPTACIÓN INTEGRAL PENDIENTES (§32.15).
- **Fase:** INFRA-5 Compute.
- **Objetivo:** definir dos hosts Linux Docker en AZ distintas con capacidad suficiente.
- **Dependencias:** T-INFRA-006, T-INFRA-008; DP-I-02/03.
- **Trazabilidad:** INFRA-002, 004–006, 039; PLD-INFRA-002/003; AC-I-03.
- **Archivos esperados:** futuros `infra/compute.tf`, `variables.tf`, `outputs.tf`, referencias IAM/red.
- **Acciones futuras:** configurar cómputo/volúmenes/roles y exposición de contenedor, mismo digest y capacidad mínima dos; bootstrap sin secretos en user-data. No crear instancias hasta 027.
- **Aceptación:** IaC expresa dos EC2/dos AZ, sin puertos abiertos al público ni secreto; capacidad elegida justificada sin debilitar hasher. Existencia real exigida por gate en 027–028.
- **Evidencia:** E(T-INFRA-009), revisión topología/capacidad y configuración.
- **Resultado actual:** §23 analiza dos hosts x86_64 en use1-az1/use1-az2, AL2023 vía parámetro público SSM, candidato t3a.small y gp3 10GiB/host. Salida requiere EIP/propuesta o alternativa aprobada; no NAT/IP automática. Estimación pública identifica riesgo agregado USD50 y detiene antes de compute.tf/role/profile/bootstrap/policies/plan. No EC2 creada, AMI ID resuelto, capacidad/SSM/Docker probados ni E009 aceptada; propuestas no son decisiones finales.
- **Resultado posterior:** §24 aprueba ventana48h/guardrail10 para preparación y define2t3a.small/2AZ, gp3cifrado10GiB, ENI/EIP estables, host bootstrap sin API. Fmt/validate exit0. Un plan exit1 muestra10altas parciales/0cambios/0bajas; **2EC2 del HCL no llegan a planearse** por ssm:GetParameter denegado, AMI sin resolver. Sin recursos/capacidad/SSM/bootstrap cloud probados, no E009 integral aceptada.
- **Revalidación §25:** AMI ID **ami-096ef200a877f36af** resuelta realmente por SSM versión202; OS/arquitectura/available no verificados por **ec2:DescribeImages denegado**. Sin nuevo plan ni no-op actual de16existentes. PARCIAL / definición validada, plan completo bloqueado IAM/validación AMI.
- **Replan §26:** validación concreta AMI aportada por responsable y lectura data.aws_ami.api exitosa; plan completo12/0/0 incluye dosEC2t3a.small, dosAZ y gp3cifrado10GiB/host. Bloqueo previo de plan superado; sin EC2 reales/capacidad/bootstrap/SSM/salida/seguridad runtime probados o aceptación integral.
- **Hardening §28:** HCL cambia para exigir boundary externa, sin ARN real no plan nuevo;12/0/0 de§26 pasa a evidencia PRE-HARDENING, no finalapply. Sin recursos reales, PARCIAL conservado.
- **Plan §29:** un plan POST-HARDENING real exit2/12/0/0 incluye boundary obligatoria en role, dosEC2/t3a.small/AMI aprobada/dosAZ/raícesgp3cifradas. Superado bloqueo de plan, no provisión ni pruebas de runtime/aceptación.
- **Replan §30:** exit2/12/0/0 después de adjuntos informados, boundary y compute esperados, NETWORK14/ECR2no-op. Sin dosEC2 reales, bootstrap/SSM/capacidad/aceptación pendientes.
- **Plan guardado §30.5:** infra/tfplan-infra5 inspeccionado,12/0/0/0replacements; dosEC2/AMI/tipo/AZ/raíces/IMDSv2/user_data esperados. Provisión y aceptación siguen pendientes, sin apply.
- **Apply parcial §31:**8recursos nuevos enstate,2ENI/2EIPtainted pero0asociaciones/0EC2/0rootEBS delincremento. FallólecturaEIP previa a dependencias deboot. No doshosts reales, SSM/Docker/bootstrap/capacidad; autorización consumida sin retry/segundoapply. Plan binario conserva hash pero NO REAPLICAR.
- **Lecturas de recovery §32:** EIP2/ENI2 existen en AWS, sin asociación/attachment; EIP siguen tainted enstate. No nuevo plan porque ruta no ignorada, por lo que replacements/conservación de IP, EC2/asociaciones a crear y no-op no están acreditados. Sin recovery apply; no reutilizar plan anterior.
- **Recovery plan §32.5:** bloqueo de ignore resuelto por regla exacta autorizada. Plan real propone reemplazar2EIP por taint y crear2EC2/2asociaciones; ENI2/IAM4/NETWORK14/ECR2 no-op. Replacements no aceptados ni aplicados, sin hosts reales/SSM/Docker; PARCIAL / RECOVERY PENDIENTE.
- **Recuperación conservadora §32.6:** EIP/ENI reales válidas, untaint A/B exit0/0 sin modificar allocations/IP/atributos. Recovery2 no creado por ignoreexit1; aún sin EC2/asociaciones o nuevo plan4/0/0 acreditado. PARCIAL / RECOVERY EN CURSO.
- **Recovery2 §32.7:** nueva exclusión exacta resuelve bloqueo; plan real4/0/0/0replacements crea solo2EC2/2asociaciones sobre EIP/ENI existentes,24no-op. Sin apply/hosts reales o aceptación integral, PARCIAL conservado.
- **Apply recovery2 §32.8:** único apply exit1; dos asociaciones completadas, RunInstances A/B devuelve InvalidParameterCombination por t3a.small no elegible Free Tier. Ningún ID EC2 obtenido; no dos hosts reales verificados, SSM/Docker/capacidad/digest A/B o aceptación. Detenido sin nuevo plan/cambio de tipo/reintento. PARCIAL, no cerrar009.
- **Corrección §32.9:** tipo operativo t3.small autorizado por responsable (x86_64/linux-amd64,2vCPU/2GiB), sin migración ARM ni otros atributos modificados. Cuatro validaciones locales exit0; recovery3 no generado por falta de confirmación de policy real. Elegibilidad específica de cuenta informada por responsable, no nueva prueba RunInstances/capacidad. PARCIAL / RECOVERY PENDIENTE.
- **Retoma §32.10:** HCL/candidata exactos t3.small, state26/0EC2 y AWS0EC2dev más asociaciones/EIP/ENIoriginales verificados. Sin plan nuevo porque ignoreexit1; 2altas siguen conceptuales y no acreditan hosts/capacidad. PARCIAL / RECOVERY3 PENDIENTE.
- **Recovery3 §32.11:** plan real crea únicamente2EC2 t3.small/AMIaprobada/dosAZ/EBS/IMDSv2 esperados;26existentesno-op,0replacements. Ignore resuelto, hash/show acreditados, sin apply/hosts/SSM/Docker/capacidad. PARCIAL / RECOVERY3 PRE-APPLY.
- **Apply recovery3 §32.12:** exit1, AWSIDs A i-0a66085b7b2411393/B i-00de11a3f3c1a8aa5 enerroresdeconfiguraciónposterior; no Creation complete ni doshosts verificados/running/SSM/Docker/capacidad/digest. Detenido sin inventario/retry/segundoapply; PARCIAL, no cerrar009.
- **Lecturas §32.13:** dos EC2 running reales, t3.small/AMI/dosAZ/EBS/IMDSv2/ENI/profile correctos, ambas en state28 pero tainted. Avance de existencia/configuración, no bootstrap Docker/SSM, capacidad, digest o reconciliación aceptados. Recovery4 bloqueado por ignore; PARCIAL.
- **Reconciliación §32.14:** comparación material completa para atributos autorizados, user-data idéntico al bootstrap HCL, untaint A/B exit0/0; ambos EC2 no-op en recovery4, recursos0/0/0. Docker/SSM no comprobados por denegación de lectura al operador; capacidad/mismo digest/aceptación no satisfechos. PARCIAL, no cierre009.
- **Validación final §32.15:** dos running/t3.small/dosAZ y seguridad/EBS/ENI/EIP previos correctos; SSM Online y Docker25.0.14 enabled/active reales en A/B. Checks técnicos de baseline de hosts satisfechos; dimensionamiento aprobado y justificado sin cambiar hasher, no carga medida. No se exige API/mismo digest desplegado/ALB/HTTPS para aceptar estos checks de hosts: pertenecen a integración/gates posteriores. Catálogo009 conserva PARCIAL porque T006/T008 y decisiones DP-I-02/03 no tienen aceptación integral, no por falta de Docker/SSM ni por inventar una prueba de deploy como criterio de hosts. E009 técnica disponible para revisión del responsable.
- **Riesgo:** CPU/memoria insuficientes, una sola máquina disfrazada de dos instancias o bootstrap inseguro.
- **Gate asociado:** GATE-I-C.

### T-INFRA-010 — Recuperación, mantenimiento y escalado

- **Estado:** PARCIAL / RECOVERY DOCUMENTADO; PRUEBA Y ACEPTACIÓN PENDIENTES (§24).
- **Fase:** INFRA-5 Compute.
- **Objetivo:** definir recuperación y escalado sin perder estado de seguridad compartido.
- **Dependencias:** T-INFRA-009; DP-I-02/07/15.
- **Trazabilidad:** INFRA-009, 019, 035, 039; PLD-INFRA-002/010/018; AC-I-03, 07, 13.
- **Archivos esperados:** futuros `infra/compute.tf`, `observability.tf` y procedimiento en `specs/003-infrastructure/plan.md`.
- **Acciones futuras:** diseñar reemplazo y drenaje, capacidad durante mantenimiento, escalado manual inicial y actualización de salidas Atlas; no reiniciar cuotas por host ni activar autoscaling sin decisión.
- **Aceptación:** runbook con fallos/rollback y límites de disponibilidad explícitos; escenarios de recuperación definidos para prueba 028, no promesa de cero downtime.
- **Evidencia:** E(T-INFRA-010), procedimiento y revisión de escenarios/costo.
- **Resultado actual:** §23 documenta propuesta de reemplazo Terraform con AMI/bootstrap controlados, retención/reasociación de salida estable, artefacto igual en A/B y estado crítico externo; sin Auto Scaling ni despliegue. Sin prueba real de fallo/reemplazo/recuperación, rollback anterior, objetivos/duración/capacidad/egreso aceptados o garantía de dos hosts durante mantenimiento. E010 parcial, aceptación pendiente.
- **Preparación posterior:** §24 implementa definiciones de ENI/EIP independientes del host y user_data_replace_on_change, AMI mantenible/fijable y bootstrap común; documenta retención ENI/IP y destrucción previa del host si se reemplaza. No Auto Scaling/create_before_destroy, reemplazo ejecutado, garantía2hosts durante mantenimiento o rollback previo probado. PARCIAL conservado.
- **Revalidación §25:** alias SSM devuelve ID concreto, sin fijarlo/modificar HCL ni verificar atributos. Recovery/reemplazo no ejecutados; PARCIAL / documentación, prueba y aceptación pendientes.
- **Replan §26:** asociaciones/ENI/EIP/EC2 aparecen en plan completo sin reemplazos existentes; no falla/reemplazo/retención real de ENI o recovery ensayados. PARCIAL / documentación, prueba y aceptación pendientes.
- **Hardening §28:** reproducción futura exige misma boundary externa aprobada/versionada y custodia independiente, no retirarla para recuperar un host. Recovery no probado, PARCIAL conservado.
- **Plan §29:** dependencia de boundary real incluida en plan; sin reemplazo/falla/recuperación o mantenimiento real probado, PARCIAL conservado.
- **Replan §30:** inventario de recuperación sin cambios, sin replacements o prueba real; PARCIAL / prueba y aceptación pendientes.
- **Plan guardado §30.5:** conserva ENI/EIP persistentes y dosAZ, sin ejecutar recuperación/reemplazo/mantenimiento. PARCIAL, prueba y aceptación pendientes.
- **Apply parcial §31:** recuperación documentada requiere preservar recursos/estado, revisar lectura denegada y2EIPtainted antes de cualquier replan/reconciliación autorizados por separado. No untaint/stateedit/destroy/reemplazo o rollback probados; PARCIAL conservado.
- **Lecturas de recovery §32:** existencia de ENI/EIP confirmada sin alterar taint/estado. Plan nuevo bloqueado por ignore; no recuperación aplicada ni prueba destructiva. Terraform/AMI/bootstrap/ENI-EIP persistentes/estado externo conservan su carácter de estrategia pendiente de prueba.
- **Recovery plan §32.5:** muestra delete/create de EIP por taint, con pérdida de sus IP actuales si se aplicara. Persistencia de ENI no equivale a conservar EIP al aceptar este plan. Se requiere revisión humana de conservación/replacement; no se ejecutan alternativas, untaint o recuperación. Prueba y aceptación siguen pendientes.
- **Recuperación conservadora §32.6:** solo marcas taint eliminadas tras verificar EIP y coincidencia con state, allocations/IP conservadas. No sustitución de hosts, recuperación integral o prueba destructiva ejecutadas; recovery2 bloqueado por ignore. PARCIAL, sin cierre de010.
- **Recovery2 §32.7:** EIP2/ENI2 no-op y asociaciones usan allocations originales, sin replacement. Acredita planificación conservadora post-untaint, no reprovisión/recuperación de hosts, continuidad o prueba destructiva. PARCIAL, no cerrar010.
- **Apply recovery2 §32.8:** asociaciones creadas sobre EIP/ENI originales, sin reemplazos ejecutados. Recuperación parcial aún NO completada: ambas EC2 rechazadas. Aporta evidencia de fallo/avance conservador, no prueba destructiva, sustitución real de host, recuperación integral o continuidad. PARCIAL, no cerrar010.
- **Retoma §32.10:** EIP/asociaciones/ENIoriginales confirmadas en AWS, ENIavailable/sinattachmenthost; EC2dev0. Conservación real no es recuperación dehosts o prueba destructiva/reemplazo, PARCIAL.
- **Recovery3 §32.11:** EIP/ENI/asociaciones originales no-op y hosts futuros usan ENIexistentes; planificación conservadora no completa recuperación dehosts ni prueba destructiva/reemplazo/continuidad. PARCIAL.
- **Apply recovery3 §32.12:** tercerapplyparcial deINFRA-5, ahora con dosIDEC2 y falloalconfigurarshutdownbehavior. Historialtaintcorregido/cambiodetipo/conservaciónEIP-ENI aporta trazabilidad, pero recuperaciónintegral NO completada ni pruebadestructiva/reemplazo/continuidadprobados. PARCIAL, no cerrar010.
- **Lecturas §32.13:** hosts running y EIP/ENI/asociaciones originales conservadas; ambas EC2 siguen tainted. Falta reconciliación autorizada y prueba/aceptación integral; running no equivale a recovery cerrado, PARCIAL.
- **Reconciliación §32.14:** eliminación controlada de dos marcas, sin sustitución de hosts o pérdida de IP; recovery4 demuestra28no-op/0replacements. Evidencia real de recuperación conservadora del estado, no prueba formal de fallo/reemplazo/drenaje/capacidad/continuidad del runbook. PARCIAL.
- **Validación final §32.15:** recuperación conservadora llega a hosts SSM/Docker operativos, originales y sin contenedores; cronología completa registrada. No fallo inducido, reemplazo real/drenaje/rollback/continuidad o runbook integral aceptados; PARCIAL.
- **Riesgo:** quedar con una instancia, nueva IP bloqueada en Atlas o reabrir seguridad por reinicio.
- **Gate asociado:** GATE-I-C, GATE-I-H.

### T-INFRA-011 — ALB y target group

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-6 Balanceador.
- **Objetivo:** expresar balanceo público hacia dos targets privados por SG.
- **Dependencias:** T-INFRA-003, T-INFRA-006, T-INFRA-009; DP-I-05.
- **Trazabilidad:** INFRA-010, 011, 031; PLD-INFRA-006; AC-I-04.
- **Archivos esperados:** futuros `infra/load-balancer.tf`, `variables.tf`, `outputs.tf`.
- **Acciones futuras:** definir ALB/subnets/target group `instance`, registro A/B, puerto/protocolo y drenaje; no requerir stickiness ni admisión pública previa a TLS/dependencias.
- **Aceptación:** configuración revisada consistente con dos AZ y reglas SG; la distribución real se acredita solo en 028.
- **Evidencia:** E(T-INFRA-011), revisión ALB/targets/puertos.
- **Riesgo:** target incorrecto, ALB vacío o afinidad que oculte cuotas no distribuidas.
- **Gate asociado:** GATE-I-D.

### T-INFRA-012 — Distribución, drenaje y fallos de targets

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-6 Balanceador.
- **Objetivo:** definir comportamiento seguro de health y cambios de instancia.
- **Dependencias:** T-INFRA-010, T-INFRA-011; DP-I-05/15 para el diseño inicial. El contrato health definitivo se integra después en 023–024 y su verificación runtime en 028, sin dependencia circular.
- **Trazabilidad:** INFRA-010, 011, 019, 027, 028; PLD-INFRA-006/014; AC-I-04, 11.
- **Archivos esperados:** futuros `infra/load-balancer.tf` y contrato/escenarios en `specs/003-infrastructure/plan.md`.
- **Acciones futuras:** fijar timeouts/intervalos/matcher/draining, escenarios un target unhealthy y todos unhealthy, y atribución interna A/B para pruebas. No reintentos automáticos de POST.
- **Aceptación:** parámetros y matriz aprobados, incluyendo ALB fail-open y fail-closed de aplicación; no usar solo liveness como readiness. Ejecución real en 028.
- **Evidencia:** E(T-INFRA-012), contrato de checks/fallos y criterios de resultado.
- **Riesgo:** asumir que ALB bloquea todo tráfico cuando todos están unhealthy.
- **Gate asociado:** GATE-I-D.

### T-INFRA-013 — Dominio, DNS y ACM

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-7 HTTPS.
- **Objetivo:** resolver identidad HTTPS pública verificable.
- **Dependencias:** T-INFRA-001, T-INFRA-011; DP-I-04.
- **Trazabilidad:** INFRA-012, 031; PLD-INFRA-007; AC-I-05.
- **Archivos esperados:** futura configuración ACM/DNS aprobada en `infra/load-balancer.tf` o `main.tf`, inputs/outputs no secretos.
- **Acciones futuras:** obtener decisión de dominio/subdominio y proveedor DNS, titular/permisos y validación/renovación ACM regional. Definir registros hacia ALB; no inventar dominio.
- **Aceptación:** ownership/autorización y configuración DNS/ACM revisadas; no afirmar certificado emitido antes de 027–028.
- **Evidencia:** E(T-INFRA-013), decisión DNS y revisión saneada de validación/renovación.
- **Riesgo:** falta de control DNS, región errónea, costo de dominio o certificado sin renovar.
- **Gate asociado:** GATE-I-E.

### T-INFRA-014 — Listeners, TLS y forwarding

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-7 HTTPS.
- **Objetivo:** evitar publicación insegura o identidad de origen falsificable.
- **Dependencias:** T-INFRA-006, T-INFRA-012, T-INFRA-013; DP-I-05/11.
- **Trazabilidad:** INFRA-012, 013, 028; PLD-INFRA-007/014; AC-I-05, 11.
- **Archivos esperados:** futuros `infra/load-balancer.tf`, `security.tf`, `variables.tf`; contrato en `plan.md`, sin middleware modificado aquí.
- **Acciones futuras:** especificar 443 con ACM, 80 solo redirect, TLS interno y proxies conocidos; definir pruebas de cadena/hostname/expiración, spoofing y bucles HTTPS para 028.
- **Aceptación:** política TLS/transporte/proxy aprobada y compatible con cuotas por origen; falta de implementación forwarding bloquea publicación hasta 024.
- **Evidencia:** E(T-INFRA-014), revisión listeners y plan de verificación de transporte/origen.
- **Riesgo:** confiar en X-Forwarded-For del cliente o atribuir HTTPS a EXPOSE.
- **Gate asociado:** GATE-I-E.

### T-INFRA-015 — Atlas: red, usuario y TLS

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-8 Atlas.
- **Objetivo:** definir acceso NoSQL seguro por ambiente.
- **Dependencias:** T-INFRA-004, T-INFRA-005; DP-I-06/09.
- **Trazabilidad:** INFRA-009, 014, 022, 033; PLD-INFRA-008/012; AC-I-06, 09.
- **Archivos esperados:** futuras referencias en `infra/networking.tf`, `secrets.tf`, `variables.tf`; decisión Atlas en `spec.md`.
- **Acciones futuras:** aprobar cluster/tier/región y ownership, DB/usuario mínimo y salidas permitidas, TLS validado y connection string externa. Provider Atlas adicional solo por autorización explícita.
- **Aceptación:** contrato/red/permisos/costo revisados sin credenciales ni supuesta gratuidad; creación/ingreso de secretos se reserva a 027 con permiso específico.
- **Evidencia:** E(T-INFRA-015), revisión Atlas y decisiones de provisión/backup.
- **Riesgo:** tier incompatible, TLS deshabilitado, allowlist abierta o usuario excesivo.
- **Gate asociado:** GATE-I-F.

### T-INFRA-016 — Atlas: índices, ACK y concurrencia

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-8 Atlas.
- **Objetivo:** preservar contratos de persistencia de 002 en la configuración cloud.
- **Dependencias:** T-INFRA-015; contrato de secretos coordinado con T-INFRA-019 antes de 026.
- **Trazabilidad:** INFRA-015, 040; PLD-INFRA-008/020; AC-I-06, 14.
- **Archivos esperados:** referencias/configuración futura en `infra/compute.tf` y plan de verificación en `specs/003-infrastructure/plan.md`; sin cambios de repositorio Mongo o tests.
- **Acciones futuras:** revisar índices/BSON/read-write concerns/ACK e inicialización existente, permisos separados y escenarios duplicado concurrente/lectura A-B. No relajar garantías por tier.
- **Aceptación:** configuración y matriz de pruebas compatibles con 002 revisadas; prueba runtime se registra en 028, no se infiere de tests locales.
- **Evidencia:** E(T-INFRA-016), checklist de contratos e inicialización y plan cross-instance.
- **Riesgo:** admitir antes del índice, duplicados o éxito sin ACK acreditado.
- **Gate asociado:** GATE-I-F, GATE-I-I.

### T-INFRA-017 — Opción Redis y compatibilidad

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-9 Redis.
- **Objetivo:** seleccionar opción viable sin alterar semántica de seguridad.
- **Dependencias:** T-INFRA-001, T-INFRA-004, T-INFRA-005; DP-I-07.
- **Trazabilidad:** INFRA-003, 016–018; PLD-INFRA-009; AC-I-07.
- **Archivos esperados:** futuros `infra/redis.tf`, `variables.tf` y decisión en `spec.md`.
- **Acciones futuras:** comparar ElastiCache nodos frente a Redis dedicado EC2 privada; evaluar costo total, versión/topología, TLS/ACL, TIME, Lua, atomicidad/noeviction y restricciones de comandos administrativos.
- **Aceptación:** selección aprobada con matriz de compatibilidad a comprobar en 028; no Serverless/Valkey/Cluster por equivalencia asumida y sin afirmar HA del nodo único.
- **Evidencia:** E(T-INFRA-017), decisión/costo y matriz de comandos/parámetros/adapter.
- **Riesgo:** servicio "compatible" que no permita scripts/reloj/política requeridos.
- **Gate asociado:** GATE-I-F.

### T-INFRA-018 — Redis: capacidad, continuidad y recuperación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-9 Redis.
- **Objetivo:** dimensionar y definir seguridad ante pérdida o resultado incierto.
- **Dependencias:** T-INFRA-010, T-INFRA-017; contratos de authority T-INFRA-020 antes de 026; DP-I-07/15.
- **Trazabilidad:** INFRA-016–019, 026, 035, 040; PLD-INFRA-009/010/018; AC-I-07, 11, 14.
- **Archivos esperados:** futuros `infra/redis.tf`, `observability.tf` y runbook en `plan.md`.
- **Acciones futuras:** definir memoria total/reserva/noeviction/topes, persistencia y señales/fencing/cuarentena; separar recuperación de cuotas/revocaciones, sin ledger externo. Diseñar pruebas autorizadas de pérdida/failover para 028.
- **Aceptación:** capacidad, RTO/RPO/límites y recuperación aprobados; 64 MiB de cuotas no usados como total. Riesgo silencioso residual y AUTH-027 no se declaran resueltos.
- **Evidencia:** E(T-INFRA-018), dimensionamiento y modelo de fallos/runbook con límites de detección.
- **Riesgo:** pérdida de marca vigente, cuotas reiniciadas, apertura prematura o promesa falsa de durabilidad.
- **Gate asociado:** GATE-I-F, GATE-I-H.

### T-INFRA-019 — Secretos externos, entrega y rotación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-10 Configuración.
- **Objetivo:** mantener secretos runtime fuera de Git/imagen/state y distribuirlos de forma coordinada.
- **Dependencias:** T-INFRA-005, T-INFRA-008; DP-I-09/14.
- **Trazabilidad:** INFRA-006, 022–024, 032, 033; PLD-INFRA-012/016/018; AC-I-09, 12, 13.
- **Archivos esperados:** futuros `infra/secrets.tf`, `security.tf`, `compute.tf`, inputs solo de referencias.
- **Acciones futuras:** evaluar Secrets Manager preferido frente a Parameter Store; definir JWT, Atlas, Redis, HMAC y otros secretos, KMS/IAM, entrega segura, actualización/rotación y separación por ambiente. Valores por flujo externo aprobado, nunca Terraform/user-data.
- **Aceptación:** contrato aprobado sin valores secretos; JWT mantiene verificadoras vigentes y HMAC no resetea ventanas. GitHub solo deployment/OIDC, no fuente runtime.
- **Evidencia:** E(T-INFRA-019), inventario por finalidad sin valores, revisión IAM/entrega/rotación.
- **Riesgo:** secreto en state/logs/disco, clave compartida entre ambientes o rotación inconsistente A/B.
- **Gate asociado:** GATE-I-B, GATE-I-F, GATE-I-H.

### T-INFRA-020 — Tabla authority y bootstrap controlado

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-10 Configuración.
- **Objetivo:** contemplar DynamoDB authority existente, no un sustituto.
- **Dependencias:** T-INFRA-005, T-INFRA-019; DP-I-08/09.
- **Trazabilidad:** INFRA-020, 021, 024, 026; PLD-INFRA-005/011; AC-I-08, 09.
- **Archivos esperados:** futuros `infra/authority.tf`, `security.tf`, `variables.tf`, `outputs.tf`; procedimiento en `plan.md`.
- **Acciones futuras:** verificar esquema/clave del adapter, nombre/región/capacidad, lecturas consistentes/condicionales, roles runtime/bootstrap, protección y observabilidad. Definir bootstrap y recuperación; ejecutarlos solo en 027 bajo permiso explícito.
- **Aceptación:** configuración y procedimiento revisados sin initializer alternativo, ledger de tokens ni reapertura por arranque. Ausencia/recreación de tabla no se considera estado confiable.
- **Evidencia:** E(T-INFRA-020), mapping adapter→tabla/permisos y aprobación de bootstrap.
- **Riesgo:** autoridad vacía/antigua, privilegios de borrado runtime o confusión con lock Terraform.
- **Gate asociado:** GATE-I-B, GATE-I-F.

### T-INFRA-021 — Logs y métricas CloudWatch

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-11 Observabilidad.
- **Objetivo:** definir telemetría útil, saneada y con costos controlados.
- **Dependencias:** T-INFRA-009, T-INFRA-011, T-INFRA-017, T-INFRA-019, T-INFRA-020; DP-I-10.
- **Trazabilidad:** INFRA-025, 026, 037; PLD-INFRA-013/020; AC-I-10, 15.
- **Archivos esperados:** futuros `infra/observability.tf`, referencias en `compute.tf`/`security.tf`; agentes/config complementaria requieren alcance explícito.
- **Acciones futuras:** especificar entrega logs, CPU/memoria/estado, targets, errores/latencia, Redis/authority; correlación A/B interna y etiquetas acotadas. No añadir Serilog u otro stack por iniciativa propia.
- **Aceptación:** cobertura/privacidad/permisos revisados, requisitos de agente/exportación identificados; señal operativa real solo en 028.
- **Evidencia:** E(T-INFRA-021), matriz señal→origen→destino→acceso/costo.
- **Riesgo:** logs de credenciales/PII, cardinalidad elevada o asumir memoria disponible sin agente.
- **Gate asociado:** GATE-I-G.

### T-INFRA-022 — Alarmas, retención y operación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-11 Observabilidad.
- **Objetivo:** fijar alertas accionables y retención antes de publicar.
- **Dependencias:** T-INFRA-018, T-INFRA-021; DP-I-10/13.
- **Trazabilidad:** INFRA-003, 025, 026, 037; PLD-INFRA-001/013/020; AC-I-10, 15.
- **Archivos esperados:** futuros `infra/observability.tf`, `variables.tf`; procedimiento autorizado en `plan.md`.
- **Acciones futuras:** aprobar retención/umbrales/destinatario, alarmas capacidad Redis/targets/errores/authority, acceso y límites de costo; preparar prueba controlada y recuperación de alarma para 028.
- **Aceptación:** valores/responsables aprobados, sin datos sensibles en notificaciones; ninguna alarma se considera operativa sin prueba posterior.
- **Evidencia:** E(T-INFRA-022), aprobación de umbrales/retención y matriz de respuesta.
- **Riesgo:** alertas sin destinatario, fatiga/omisiones, costo o retención indebida.
- **Gate asociado:** GATE-I-G.

### T-INFRA-023 — Contrato liveness/readiness y proxy

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-12 Health.
- **Objetivo:** concretar requisitos de aplicación faltantes sin implementarlos en 003.
- **Dependencias:** T-INFRA-012, T-INFRA-014, T-INFRA-016, T-INFRA-018, T-INFRA-020, T-INFRA-022; DP-I-05/11/15.
- **Trazabilidad:** INFRA-011, 013, 019, 027, 028; PLD-INFRA-007/010/014; AC-I-04, 05, 11.
- **Archivos esperados:** contrato y solicitud futura delimitada en `specs/003-infrastructure/spec.md`/`plan.md`; no código ni tests.
- **Acciones futuras:** definir ruta candidata readiness, salida mínima, índices/dependencias/inicialización/admisión segura, confianza proxy/origen y uso ALB. Mantener `/health` liveness y Logout sin dependencia Mongo añadida.
- **Aceptación:** contrato revisado con 002 y alcance de implementación separado aprobado; ningún ping o retirada local sustituye continuidad/fencing compartidos.
- **Evidencia:** E(T-INFRA-023), contrato, revisión de seguridad y autorización separada propuesta/otorgada según realidad.
- **Riesgo:** declarar listo un endpoint inexistente o falsear origen/cuotas mediante forwarding.
- **Gate asociado:** GATE-I-D, GATE-I-E, GATE-I-F.

### T-INFRA-024 — Evidencia de readiness/forwarding antes de cloud

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-12 Health.
- **Objetivo:** aceptar la dependencia de aplicación solo con evidencia propia autorizada.
- **Dependencias:** T-INFRA-023; incremento externo de aplicación con autorización explícita y evidencia aceptada. Sin este incremento, BLOQUEADA al intentar ejecutarla.
- **Trazabilidad:** INFRA-011, 013, 027, 028, 038; PLD-INFRA-014/020; AC-I-04, 05, 11.
- **Archivos esperados:** referencias futuras de evidencia en `specs/003-infrastructure/task.md`; ruta aprobada en `infra/load-balancer.tf`. Ningún archivo productivo/tests modificado por esta tarea.
- **Acciones futuras:** revisar commit/artefacto y pruebas del incremento separado para readiness/forwarding, fallos/spoofing y contratos de 002; alinear configuración ALB sin inventar éxito.
- **Aceptación:** evidencia aceptada de capacidad implementada y compatible, sin declarar todavía validación cloud. A falta de autorización/implementación, no avanzar a 026/027.
- **Evidencia:** E(T-INFRA-024), referencia íntegra y saneada al incremento, resultados y revisión de integración.
- **Riesgo:** eludir prohibición de modificar Authentication/tests o desplegar readiness solo nominal.
- **Gate asociado:** GATE-I-D, GATE-I-E, GATE-I-F.

### T-INFRA-025 — Validación inicial de baseline Terraform

- **Estado:** COMPLETA / ACEPTADA por el responsable del proyecto (§37), en validación inicial local; dependencias001/002 aceptadas. RECOVERY4/OUTPUT HISTÓRICOS CONSERVADOS, SIN APPLY.
- **Fase:** INFRA-13 Terraform, intervención temprana transversal.
- **Objetivo:** acreditar baseline antes de extender IaC.
- **Dependencias:** T-INFRA-001, T-INFRA-002; autorización explícita de comandos/entorno, DP-I-12.
- **Trazabilidad:** INFRA-029, 032, 038; PLD-INFRA-015/016/019; AC-I-01.
- **Archivos esperados:** futura baseline `infra/*.tf` de 002 y lockfile de providers si se autoriza; state/planes nunca Git.
- **Acciones futuras:** revisar formato, inicialización/validación y versiones/locking mediante comandos autorizados; registrar salida nativa saneada. No apply ni provisión; cualquier descarga/init requerirá permiso futuro.
- **Aceptación:** baseline validada realmente, state/backup protegidos y versiones acreditadas; validación local no prueba cloud ni toda la IaC futura.
- **Evidencia:** E(T-INFRA-025), comandos/entorno/versiones/resultados y revisión.
- **Resultado actual:** §14 acredita fmt-check/validate exit 0, plan guardado 14/0/0 exit 2, apply de ese archivo exit 0 (14 añadidos), post-plan 0/0/0 exit 0 y verificación AWS; hashes infra/HCL/lock/policies intactos. State local ignorado serial 15, sin .backup observado; custodia/backup/recuperación, dependencias 001/002 y aceptación E025 integral pendientes. No COMPLETADA ni gate cerrado.
- **Extensión actual:** §15 acredita fmt/check/validate exit 0 y plan ECR 2/0/0 exit 2, con 14 recursos de red no-op. Conserva intento CLI previo fallido por argumentos PowerShell; sin init/actualización/apply ni modificación de state persistido. Custodia/backup/recovery y aceptación integral continúan pendientes.
- **Revalidación posterior:** §17 registra fmt-check/validate exit 0 y un nuevo plan guardado 2/0/0 exit 2, sin AccessDenied de Terraform; networking14 no-op, HCL/policy/lock/state persistido intactos. La lectura CLI de listado general denegada se conserva aparte; no cierre de dependencias/custodia/backup/recovery/E025 integral.
- **Apply acotado posterior:** §18 registra fmt-check/validate exit 0, plan fresco guardado 2/0/0 exit 2, apply de ese archivo exit 0 y post-plan 0/0/0 exit 0; 16managed no-op. State serial18 y backup local serial15 observados, no protección/recuperación aceptadas. Denegación ListImages posterior no es fallo Terraform; E025 integral/dependencias/custodia/recovery pendientes, estado PARCIAL conservado.
- **Revalidación de checkpoint:** §19 fmt-check/validate exit 0; sin repetir plan/apply. Post-plan§18 conservado como evidencia fechada, state/planes/backup sin cambios por hash. ListImages resuelto no satisface custodia/backup protegido/recovery ni E025 integral; PARCIAL conservado.
- **Preparación de publicación:** §20 fmt-check/validate exit 0, HCL/lock/provisioning/state/planes intactos, sin plan/apply o AWS CLI. Sin aceptación nueva de E025 integral/protección/recovery; estado PARCIAL conservado.
- **Intento de publicación bloqueado:** §21 fmt-check/validate exit 0, state/backup/planes intactos por hash; sin plan/apply/destroy/import. AccessDenied es de ECR token, ninguna operación Terraform fallida; custodia/recovery/dependencias/E025 integral pendientes, estado PARCIAL conservado.
- **Checkpoint de imagen:** §22 conserva validaciones y post-plan históricos; no ejecuta Terraform ni aceptación nueva de custodia/recovery/dependencias/E025 integral. Estado PARCIAL conservado.
- **Análisis INFRA-5:** §23 no ejecuta fmt/validate/plan al detenerse antes de HCL por costo; state/backups/planes conservados por hash. Validaciones anteriores siguen históricas, no resultados actuales; PARCIAL / aceptación pendiente.
- **Retoma INFRA-5:** §24 fmt/check/validate exit0 y plan exit1/AccessDenied público SSM; sin apply o plan completo acreditado. State/backup/planes consumidos conservados; validación local no acepta custodia/backup/recovery ni dependencias, PARCIAL permanece.
- **Revalidación §25:** fmt-check/validate exit0; replan no ejecutado por denegación EC2 previa, sin nuevo resultado add/change/destroy. Hashes de HCL/candidatas/state/backup/planes y demás fuentes preservados; PARCIAL / aceptación pendiente.
- **Replan §26:** fmt-check/validate exit0 y plan completo exit2/12add/0change/0destroy, NETWORK14/ECR2no-op. Validación/plan acreditados no resuelven custodia/cifrado/backup protegido/recovery/dependencias; aceptación integral pendiente.
- **Hardening §28:** fmt/check/validate exit0 del HCL actualizado; plan anterior PRE-HARDENING, nuevo plan bloqueado por ARN real deboundary. Sin prueba de permisos efectivos/custodia/recovery, PARCIAL / aceptación pendiente.
- **Plan §29:** fmt/check/validate exit0 y plan POST-HARDENING exit2/12/0/0, NETWORK14/ECR2no-op. Plan previo sigue PRE-HARDENING; custodia estatal/backup protegido/recovery/eficaciaIAM/dependencias no aceptadas, PARCIAL / aceptación pendiente.
- **Replan §30:** STS correcto, fmt-check/validate/bash-n exit0, plan posterior a adjuntos informados12/0/0; sin prueba de permisos deapply o aceptación de state/backup/recovery/dependencias. No cierre de E025 integral.
- **Plan guardado §30.5:** fmt-check/validate/bash-n exit0, plan guardado/show/show-json y hash acreditados; NETWORK14/ECR2no-op explícitos. Artefactos sensibles ignorados y anteriores preservados porhash; custodia protegida/backup/recovery/aceptación integral pendientes, PARCIAL.
- **Apply parcial §31:** hash/show revisados, únicoapply exit1/denegaciónEC2,24managed locales/backup16. Sin post-plan ni corrección automática; permisos de lectura incompletos y custodia/recovery/dependencias/aceptación pendientes. Registro de fallo no valida integralmente la baseline.
- **Lecturas de recovery §32:** fmt-check/validate/bash-n exit0, permiso de lectura EIP corregido observado; ignoreexit1 impide generar el plan. Sin plan/replacements/no-op nuevos o post-plan0/0/0; state/backup/planes preservados y custodia/recuperación integral pendientes.
- **Recovery plan §32.5:** fmt-check/validate/bash-n exit0, único nuevo plan guardado exit2/6add/0change/2destroy y show humano/JSON exit0, con hash y diferencias de refresh documentadas. Sin apply/untaint, post-plan0/0/0 o aceptación integral; estado local y planes anteriores intactos por hash.
- **Recuperación conservadora §32.6:** untaint A/B exit0/0, state24/serial29/0tainted y comparación semántica sin cambios de atributos. Check de integridad inicialmente sensible al orden de check_results, resuelto por comparación de contenido sin alterar state ni repetir comandos. Recovery2 bloqueado por ignore; no nueva aceptación de baseline o post-plan.
- **Recovery2 §32.7:** fmt-check/validate/bash-n exit0; plan guardado exit2/4add/0change/0destroy/0replacements, show humano/JSONexit0 yhash acreditados. Refresh mantiene3diferencias de snapshot, sin updates propuestos; state/backup/planesanteriores preservados. No post-plan0/0/0, apply o aceptación integral.
- **Apply recovery2 §32.8:** baseline/hash/show humano y JSON correctos, plan4/0/0/0replacements; único apply exit1 con dos asociaciones completadas y ambas EC2 rechazadas. No state list/inventario/post-plan después del fallo, conteo managed actual no verificado. Custodia/backup protegido/recovery/dependencias/E025 integral pendientes; PARCIAL.
- **Corrección §32.9:** fmt/fmt-check/validate/bash-n exit0; comparación exacta HCL/JSON acredita solo cambio de tipo. Sin plan, refresh o escriturasAWS; state/backup/planes intactos porhash. Recovery3 bloqueado por confirmación de policy real; custodia/recovery/dependencias/aceptación pendientes, PARCIAL.
- **Retoma §32.10:** state list exit0/26managed+3data y tresvalidaciones exit0. GetPolicydenegado no impide continuar según autorización, pero check-ignoreexit1 bloquea plan. Custodia/backup/recovery/E025integral pendientes; PARCIAL.
- **Recovery3 §32.11:** ignore/fmt-check/validate/bash-n exit0; único plan exit2/2add/0change/0destroy/0replacements y show/hash reales. State/backup/planesanteriores conservados, dos diferencias de refresh EIP sin acción deupdate. Custodia/backup protegido/recovery/dependencias/aceptación integral pendientes; PARCIAL.
- **Apply recovery3 §32.12:** baseline/hash/show correctos, únicoapplyexit1/ModifyInstanceAttributedenegado. Sin statelist/post-plantrasfallo ni conteo managed actual; custodia/backup/recovery/dependencias y aceptación siguen pendientes. PARCIAL.
- **Lecturas §32.13:** state list exit0/28managed+3data/serial37/ambas EC2 tainted y validaciones exit0. Ignoreexit1 impide recovery4; state/backup/planes preservados, sin post-plan o custodia/recuperación integral aceptadas. PARCIAL.
- **Reconciliación §32.14:** state28/serial39/0tainted, fmt-check/validate/bash-n exit0; final recovery4 exit2 por output compute_instances,28recursosno-op. Dos intentos de plan trazados (primero string null inválido), sin persistir refresh/output mediante apply. Custodia/backup protegido/dependencias/E025 integral pendientes, PARCIAL.
- **Validación final §32.15:** sin comandos Terraform ni nuevo plan, state28+3/serial39/0tainted y hash recovery4 comprobados por lectura local. SSM/Docker no sustituyen custodia/backup protegido/dependencias/aceptación E025. Output pendiente de §32.14 no se guarda; PARCIAL.
- **Validaciones §33:** version/state-list/fmt-check/validate/bash-n/diff-check exit0, inventario y recuperación aislada de copia ACL-restringida acreditados sin refresh/plan/AWS. E025 técnica de validaciones acreditada; protección del state/cifrado-backup/retención/aceptación E001-E002-E025 pendientes, **PARCIAL**, sin ejecutar026.
- **Validaciones §34:** fmt-check/validate/bash-n/diff-check exit0, hashes anteriores/postACL iguales, state/nativo ignorados y no tracked. Técnicamente acreditadas las validaciones, **no** el cifrado/descifrado; aceptación integral/dependencias pendientes, **PARCIAL**, T026 no ejecutada.
- **Validaciones §35:** fmt-check/validate/bash-n/diff-check exit0 actuales, versiones1.17.1/6.73.0 verificadas, state/nativo y .enc protegidos; recuperación cifrada confirmada por operador y limpieza/hash comprobados. E025 **TÉCNICAMENTE ACREDITADA / ACEPTACIÓN EXPLÍCITA PENDIENTE** en validación inicial local; sin afirmar satisfechas E001/dependencias ni cloud/T026.
- **Validaciones §36:** fmt-check/validate/bash-n/diff-check exit0 actuales; E001 completada técnicamente con modelo documental y custodia E002 ya acreditada. E025 **TÉCNICAMENTE ACREDITADA / ACEPTACIÓN EXPLÍCITA PENDIENTE**, sin aceptación formal de dependencias, nuevo plan/apply o ejecuciónT026.
- **Aceptación explícita §37:** **E025 ACEPTADA / T025 COMPLETA / ACEPTADA** para validación inicial local y state/backup protegidos, con dependencias001/002 aceptadas. No revisión final de toda IaC, ejecuciónT026 ni cierre B/C o pruebas runtime; recovery4 histórico recursos0/0/0/0replacements y output pendiente conservados, sin apply para persistirlo.
- **Riesgo:** inicialización con backend equivocado, descarga no aprobada o secretos en salida.
- **Gate asociado:** GATE-I-A.

### T-INFRA-026 — Revisión final, plan, costo y autorización de provisión

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-13 Terraform.
- **Objetivo:** impedir apply sin revisión integral de configuración, seguridad y costo.
- **Dependencias:** T-INFRA-001 a T-INFRA-025 aceptadas; DP bloqueantes resueltas; autorización de validación/plan. No depende de recursos cloud todavía.
- **Trazabilidad:** INFRA-003, 029–036, 038; PLD-INFRA-015–019; AC-I-01, 12, 13.
- **Archivos esperados:** futuros `infra/*.tf`, contrato de inputs/outputs y revisión en `plan.md`; plan/state protegidos fuera de Git.
- **Acciones futuras:** validar conjunto completo, revisar plan y cambios/destrucciones, drift/importación, costos y ownership digest. Antes de GitHub/uso compartido exigir backend remoto/versionado/locking y migración autorizada, aunque el baseline fuera local.
- **Aceptación:** plan/costo/versiones/red/secretos/health revisados y aprobación específica de apply, bootstrap y permisos registrados. Ninguna destrucción ni recurso fuera del alcance aprobado.
- **Evidencia:** E(T-INFRA-026), validación, resumen saneado de plan, estimación y autorización; conservar plan real de forma protegida.
- **Riesgo:** cargos/destrucciones imprevistos, dos escritores de digest, state local en runner o plan público con secretos.
- **Gate asociado:** GATE-I-H; revalidación de GATE-I-A.

### T-INFRA-027 — Provisión y despliegue manual autorizados

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-14 Cloud.
- **Objetivo:** materializar recursos y desplegar el artefacto sin automatizar promoción por push.
- **Dependencias:** T-INFRA-026; dominio/certificado y permisos aprobados; integración futura 004 aprobada/operativa, CI verde del mismo commit/digest, workflow_dispatch manual y state remoto protegido; autorización AWS/Atlas/apply/bootstrap/secretos delimitada.
- **Trazabilidad:** INFRA-001, 002, 004–024, 027–035, 038, 039; PLD-INFRA-001–019; AC-I-02–09, 11–13.
- **Archivos esperados:** futuros `infra/*.tf` revisados y referencias de ejecución/evidencia en `task.md`; sin YAML, código, Docker o tests nuevos en esta tarea.
- **Acciones futuras:** bajo permisos específicos aplicar solo lo revisado, habilitar Atlas/secretos por flujo seguro, bootstrap authority controlado y desplegar manualmente desde GitHub. Verificar inventario dos EC2/AZ, mismo digest, TLS, targets/config y señales iniciales. No reabrir confianza por deploy.
- **Aceptación:** provisión/deployment manual reales trazables y saneados, sin recursos no autorizados; existencia de dos hosts acreditada. No declarar aceptación pública integral antes de 028–030.
- **Evidencia:** E(T-INFRA-027), autorizaciones, apply/recursos, referencia workflow_dispatch, CI/commit/digest y controles iniciales sin secretos.
- **Riesgo:** gasto real, bootstrap inseguro, exposición prematura o deployment automático/no revisado. Detener admisión ante incertidumbre.
- **Gate asociado:** GATE-I-B a GATE-I-I; no cierre automático.

### T-INFRA-028 — Verificación pública, distribuida y rollback

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-14 Cloud.
- **Objetivo:** demostrar el comportamiento cloud, no extrapolar evidencia local.
- **Dependencias:** T-INFRA-027; autorización de objetivos/ventana/casos negativos y fallos; DP-I-15; superficie protegida autorizada para prueba de revocación normal.
- **Trazabilidad:** INFRA-001–028, 034–040; PLD-INFRA-002–014/017–020; AC-I-02–15.
- **Archivos esperados:** referencias futuras en `task.md`/`plan.md`; resultados/capturas saneados en ubicación protegida aprobada. Sin endpoint, test productivo o workflow nuevo.
- **Acciones futuras:** probar URL HTTPS, A/B sin stickiness, SG/SSM/bypass/origen, health y todos unhealthy, Atlas/índices/ACK/concurrencia, Redis TIME/Lua/noeviction/capacidad, authority/secretos y alarmas. Validar Register 201, Login 200, Logout 204 y negativos/no-store/challenge/cuotas/Retry-After/503 conforme 002, revocación cruzada y rollback seguro. Fallos/reapertura solo bajo modelo aprobado.
- **Aceptación:** evidencia real de todos los casos, dos instancias y mismas garantías; ningún secreto en salida. Revocación se prueba por política normal, no por excepción Logout. Fallos sin bypass, sin restaurar state antiguo; límites silenciosos explícitos.
- **Evidencia:** E(T-INFRA-028), matriz caso→AC→resultado con atribución interna A/B, logs/métricas/alarma y rollback saneados; registrar cualquier bloqueo/fallo sin ocultarlo.
- **Riesgo:** datos/usuarios reales, cuotas afectadas, diagnóstico público, falsa garantía de continuidad o prueba destructiva no autorizada.
- **Gate asociado:** GATE-I-B a GATE-I-I.

### T-INFRA-029 — Paquete de evidencia académica

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-15 Evidencia.
- **Objetivo:** permitir verificación de rúbrica, seguridad y costos por un tercero autorizado.
- **Dependencias:** T-INFRA-028; DP-I-13 y política de retención/acceso aprobada.
- **Trazabilidad:** INFRA-002, 003, 025, 026, 037, 038, 040; PLD-INFRA-001/013/020; AC-I-10, 12–15.
- **Archivos esperados:** índice de referencias futuro en `specs/003-infrastructure/task.md`; paquete externo protegido, no state/secretos/volcados en Git.
- **Acciones futuras:** reunir diagrama, mapping IaC/recursos, dos instancias/AZ/digest/targets, HTTPS, NoSQL/Redis/authority, pruebas, logs/alarma y costos; relacionar cada E con requisito/PLD/AC/gate y separar local/cloud.
- **Aceptación:** paquete accesible al evaluador autorizado, saneado, fechado/trazable y con ubicación/retención comprobadas; ningún screenshot aislado sustituye prueba distribuida.
- **Evidencia:** E(T-INFRA-029), índice/manifiesto y revisión de privacidad/completitud.
- **Riesgo:** secretos/PII/identificadores innecesarios, referencias temporales perdidas o evidencia cloud no reproducible.
- **Gate asociado:** GATE-I-H, GATE-I-I.

### T-INFRA-030 — Reproducibilidad y revisión final de aceptación

- **Estado:** PENDIENTE / NO EJECUTADA.
- **Fase:** INFRA-15 Evidencia.
- **Objetivo:** revisar aceptación de infraestructura sin aprobar retrospectivamente otros alcances.
- **Dependencias:** T-INFRA-026, T-INFRA-028, T-INFRA-029; autorización específica de reproducción/drift/recuperación, presupuesto y protección de datos.
- **Trazabilidad:** INFRA-002, 003, 029–040; PLD-INFRA-015–020; AC-I-01, 12–15.
- **Archivos esperados:** actualización futura autorizada de `spec.md`, `plan.md`, `task.md`; evidencia externa protegida. Otros documentos solo mediante permiso específico.
- **Acciones futuras:** demostrar reproducción controlada con IaC y referencias externas/secretas aprobadas, drift y recuperación/rollback sin borrar datos ni reiniciar seguridad; revisar AC y gates A–G, después H/I con el responsable. No destroy ni segunda infraestructura facturable implícitos.
- **Aceptación:** evidencia real de reproducción y revisión integral registrada, costos/límites claros y cierre solo de gates efectivamente acreditados/aprobados. 002 GATE-G/H y AUTH-027 conservan su proceso propio.
- **Evidencia:** E(T-INFRA-030), demostración autorizada, revisión final requisito→PLD→tarea→evidencia→AC→gate y decisión del responsable.
- **Riesgo:** destrucción de datos, cargos duplicados o declarar producción/seguridad integral por cerrar una demostración académica.
- **Gate asociado:** GATE-I-H, GATE-I-I.

## 5. Cobertura y revisión documental

| Requisitos | Tareas responsables principales |
| --- | --- |
| INFRA-001–003 | 001, 009, 026–030 |
| INFRA-004–006 | 008–010, 019, 027–028 |
| INFRA-007–009 | 003–006, 015, 027–028 |
| INFRA-010–013 | 006, 011–014, 023–024, 027–028 |
| INFRA-014–015 | 015–016, 027–028 |
| INFRA-016–019 | 017–018, 020, 023–024, 027–028 |
| INFRA-020–021 | 005, 020, 027–028 |
| INFRA-022–024 | 005–006, 019–020, 027–028 |
| INFRA-025–026 | 018, 020–022, 027–029 |
| INFRA-027–028 | 012, 014, 023–024, 027–028 |
| INFRA-029–033 | 001–002, 025–027, 030 |
| INFRA-034–036 | 008, 010, 018–019, 026–028, 030 |
| INFRA-037–040 | 001, 009–010, 016, 018, 021–030 |

Cobertura documental: 40 requisitos INFRA, 20 decisiones propuestas PLD-INFRA, 15 criterios AC-I, 15 fases, 30 tareas y 9 gates. **Cobertura no equivale a implementación, prueba o satisfacción.** Tras §9 hay siete tareas parciales (001–006/025), otras 23 PENDIENTE / NO EJECUTADA; todos los gates PENDIENTE / NO SATISFECHO. Identidad STS y presupuesto de baseline acreditados, sin infraestructura cloud desplegada.

Se revisa coherencia con 001 sin completarlo, con 002 sin cambiar Authentication/semántica Redis/JWT ni su autoridad DynamoDB, y con el límite entre infraestructura y 004. Readiness/forwarding siguen pendientes de incremento separado; dominio/precios/versiones/retención/umbrales y demás DP permanecen abiertos. No se declara cloud existente ni se incorporan secretos. Futuro deploy manual workflow_dispatch tras CI verde, nunca automático por push/merge.

El cierre original de materialización fue exclusivamente documental, sin Terraform/infra. La autorización posterior y sus límites se registran a continuación; no amplía permisos por sí sola a fases posteriores.

## 6. BUILD controlado T-INFRA-001 + T-INFRA-002 — 2026-11-12

### 6.1. Autorización, baseline y alcance real

Responsable solicita **ENVER — 003 INFRASTRUCTURE / BUILD T-INFRA-001 + T-INFRA-002 / BASELINE TERRAFORM**. Precheck: rama `feature/authentication`, HEAD `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`, árbol limpio. Se crea `feature/infrastructure` desde ese mismo HEAD, sin cambiar commit. Checkpoint anterior incluye únicamente los seis Markdown 003/004; Authentication sigue TERMINADA exclusivamente LOCAL/TEST REPRESENTATIVO.

Se crean `infra/main.tf`, `versions.tf`, `providers.tf`, `variables.tf`, `outputs.tf`, `locals.tf`, `terraform.tfvars.example`, `README.md` y el lock generado `infra/.terraform.lock.hcl`. Solo se actualizan este task y `memoria.md`. `.gitignore` ya contiene exclusiones equivalentes para descargas, state/backups, tfvars, planes, crashes y overrides; no se modifica ni duplica. Lock y ejemplo siguen versionables, pendientes de checkpoint posterior: **sin stage/commit/push**.

### 6.2. E(T-INFRA-001) — preparación parcial, no aprobación de destino/costo

- **Trazabilidad:** INFRA-001–003/033/038; PLD-INFRA-001/020; AC-I-01/15; GATE-I-A, soporte I-H/I-I.
- **Acreditado:** límites sin recursos/cargos, proyecto técnico `enver`, inputs de destino sin defaults y [README infra](../../infra/README.md) con lista de potenciales cargos, decisiones abiertas y procedimiento de operación local.
- **Pendiente:** cuenta/identidad, región/AZ, ambiente final, responsables, duración, presupuesto máximo, elegibilidad/créditos y precios regionales. Dominio, tamaños EC2, Redis final y tier Atlas no se seleccionan.
- **Detección segura:** AWS CLI ausente; sin variables habituales de credenciales/perfil/roles/web identity/contenedor, sin rutas alternativas de configuración indicadas ni archivos estándar `.aws/config`/`.aws/credentials`. Solo se comprobó presencia, no contenido. No se ejecutó STS. Esto **no demuestra ausencia universal de una identidad AWS**: identidad no acreditada en esta sesión; acceso pendiente. No se crean usuarios IAM/access keys.
- **Resultado:** PARCIAL / BLOQUEADA POR DECISIONES. No existe estimación regional ni aprobación escrita de presupuesto/responsables; E(001) no satisface su criterio de aceptación. DP-I-01/13 permanecen pendientes.

### 6.3. E(T-INFRA-002) — baseline técnica local validada

- **Trazabilidad:** INFRA-029–033/038; PLD-INFRA-015/016; AC-I-01/12; GATE-I-A/I-H.
- **Entorno:** Windows, Terraform **1.17.1**, plataforma **windows_amd64**. Primera llamada `terraform -version`: error de resolución (exit 1) por PATH heredado. Búsqueda acotada en PATH proceso/usuario/máquina y ubicaciones comunes encontró instalación WinGet existente del usuario; ejecución por su ruta: exit 0. Se antepone esa ruta solo al PATH del proceso de los comandos posteriores. Sin instalar/actualizar Terraform ni alterar PATH persistente; aviso de versión más nueva no seguido.
- **Versiones:** Terraform `>= 1.17.1, < 1.18.0`, compatible con la instalación observada y limitada a parches de esa minor. Provider único `hashicorp/aws ~> 6.0`, serie mayor acreditada por API oficial Registry (6.73.0, publicación 2026-11-11). `init` resolvió **6.73.0**, instalado **signed by HashiCorp**, y generó lock con versión/restricción/checksums; no se editó manualmente. Selección técnica justificada para revisión, no aprobación tácita de DP-I-12 completa.
- **Configuración revisada:** provider usa `var.aws_region`, guard `allowed_account_ids` con `var.aws_account_id` y cadena estándar externa de credenciales; sin claves ni flags que omitan verificación. Inputs tipados/validados; cuenta/región/ambiente sin defaults; ejemplo con placeholders no operativos. Naming y tags comunes; outputs documentado sin inventar recursos. Sin resource/data/module/provisioner ni acceso AWS.
- **State:** backend local explícito, ruta `terraform.tfstate`, nunca Git. `init -backend=false` no inicializa backend ni genera state de recursos. No backend S3 ni tabla de locking; DynamoDB Authentication Authority conserva su finalidad separada. README define un operador, respaldo cifrado, recuperación y migración remota antes de colaboración/GitHub. **Operador/destino, ACL/cifrado/retención y recuperación real no acreditados**: no se simulan ni se aprueban por documentación. No hay state real que respaldar en este bloque.

| Comando ejecutado | Resultado nativo |
| --- | --- |
| `terraform fmt -recursive infra` | Exit 0, sin errores |
| `terraform fmt -check -recursive infra` | Exit 0, sin archivos fuera de formato |
| `terraform -chdir=infra init -backend=false` | Exit 0; successfully initialized; AWS 6.73.0 firmado por HashiCorp; lock generado |
| `terraform -chdir=infra validate` | Exit 0; `Success! The configuration is valid.` |
| `git check-ignore -v -- …` (rutas baseline) | Exclusiones existentes confirmadas; excepción de lock y ejemplo versionable comprobados |

Las salidas nativas constan en la sesión de ejecución; este registro saneado documenta resultados, no inventa un paquete externo archivado ni retención duradera. `validate` no configura la identidad/cuenta AWS ni aprueba valores de inputs; no acredita plan, costos, infraestructura o recuperación. No se ejecutó `plan`, `apply`, refresh, import, destroy, init remoto o `init -upgrade`.

**Resultado contractual:** baseline técnica implementada/validada localmente; **T-INFRA-002 PARCIAL / ACEPTACIÓN PENDIENTE**, no COMPLETADA. T-INFRA-001 sigue sin aceptación; quedan revisión/aprobación de versiones/manejo de state y decisiones aplicables DP-I-12/13. DP-I-02 de compute no se resuelve ni se adelanta. No se rebaja el criterio original de T-INFRA-002 por obtener validate verde.

### 6.4. Gates, límites y cierre del bloque

**GATE-I-A PENDIENTE / NO SATISFECHO**: exige presupuesto, versiones, ambiente y state aprobados, E(001), E(002) y E(025), además de validación real. Solo se acredita la validación local descrita; no hay aprobación de presupuesto/destino/custodia integral ni E(T-INFRA-025). No se ejecuta ni marca T-INFRA-025 por coincidencia de comandos. AC-I-01 y los otros ocho gates permanecen pendientes; ningún AC cloud satisfecho.

Validación final repetida: `terraform -version` confirma 1.17.1/windows_amd64; `terraform fmt -check -recursive infra` y `terraform -chdir=infra validate` exit 0. `git diff --check` exit 0, sin errores de whitespace (solo aviso de normalización LF/CRLF por configuración existente). `git diff --stat` muestra únicamente task/memoria como cambios rastreados; `git status --untracked-files=all` añade los nueve archivos infra esperados. Sin state/plan real, sin secretos detectados en la baseline/lock y sin stage. Las descargas locales `.terraform/` permanecen ignoradas; no se versionan binarios.

No se crean recursos AWS, workflows, networking/compute/registry/authority/secrets/observability ni otras fases; no se modifica código productivo, Authentication, tests, Dockerfile, SDK, 003 spec/plan o 004. Primera release Auth-only y deploy exclusivamente manual workflow_dispatch tras CI verde exacto se conservan. Orders fuera de alcance; AUTH-027 integral y gates de 002 no cambian. Se detiene tras este bloque, sin stage/commit/push.

## 7. Validación AWS + Terraform y revisión de cierre — 2026-11-12

### 7.1. Autorización y evidencia saneada de identidad/perfil

Solicitud **ENVER — 003 INFRASTRUCTURE / CIERRE BASELINE AWS + TERRAFORM / BUILD-VALIDACIÓN CONTROLADA**. Rama `feature/infrastructure`, HEAD `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`; preexistían task/memoria modificados y nueve archivos infra nuevos, 0 staged. Se preservan esos cambios. Solo se actualizan este archivo y memoria.md; no se modifica configuración Terraform, lock, ejemplo, README infra o configuración local AWS.

El responsable informa instalación AWS CLI, usuario IAM sin acceso de consola y credenciales configuradas localmente. No se crean usuarios, claves, políticas ni recursos en este turno. El sondeo inicial no encuentra CLI en PATH heredado/ubicación global; la búsqueda acotada localiza instalación existente del usuario en `AppData/Local/Programs/Amazon/AWSCLIV2`. `aws --version`: **aws-cli/2.40.1**, Python 3.14.8, Windows/10, AMD64, exit 0; sin instalación/actualización ni cambios persistentes de PATH.

Las salidas completas de STS/configuración se capturan en memoria del proceso y se reducen antes de mostrarse. No se imprimen ni archivan Account ID/ARN completos, access keys (ni sufijos), secret keys o tokens. No se leen directamente archivos de credenciales ni se escriben salidas crudas en Git/temporales.

| Consulta real | Resultado saneado |
| --- | --- |
| `aws sts get-caller-identity --profile enver-infra` (JSON, sin pager) | Exit 0; autenticación exitosa; Account presente; ARN coincide con usuario IAM `user/enver-terraform-local` |
| `aws configure get region --profile enver-infra` | Exit 0; `us-east-1` |
| `aws configure list --profile enver-infra` | Exit 0; perfil esperado, access_key y secret_key presentes, origen `shared-credentials-file`; región `us-east-1`, origen `config-file` |

**Región: us-east-1 — APROBADA PARA BASELINE**, por selección explícita del responsable y verificación real. No es irreversible ni aprueba región/AZ final para todos los servicios. La cuenta autenticada está presente, sin publicar ID; eso no aprueba costos, permisos de provisión o recursos.

### 7.2. IAM: verificación limitada, sin ampliación de acceso

El responsable declara que `enver-terraform-local` no tiene permisos administrativos ni amplios de infraestructura; es intencional. Para comprobarlo se intentan solo lecturas IAM con ese perfil: `list-attached-user-policies`, `list-user-policies` y `list-groups-for-user`, limitadas al mismo usuario. Las tres devuelven **AccessDenied, exit 254**. No se reintenta con otro principal ni se agregan permisos para eludir la denegación.

**IAM permissions: PENDIENTES PARA INFRA-3.** Se acredita denegación de esas consultas, **no ausencia integral de privilegios**: STS valida identidad, no enumera permisos efectivos; AccessDenied en lecturas tampoco demuestra que no existan otros permisos amplios. La declaración del responsable queda separada de lo observado. Auditoría de políticas/grupos/roles/condiciones requiere evidencia de un operador autorizado; no se marca T-INFRA-005 ejecutada ni se inspeccionan otras identidades.

### 7.3. E(T-INFRA-002) complementaria: versiones, configuración y state

| Comprobación | Resultado |
| --- | --- |
| `terraform -version` | Exit 0; **1.17.1 / windows_amd64**; sin actualizar |
| `terraform fmt -check -recursive infra` | Exit 0; formato correcto, sin editar archivos |
| `terraform -chdir=infra validate` | Exit 0; **Success! The configuration is valid.** |
| Lock AWS | **hashicorp/aws 6.73.0**, restricción `~> 6.0`, checksums presentes; sin actualizar/init nuevo |
| Provider | `region = var.aws_region`, cuenta permitida desde variable; sin access_key/secret_key/token en HCL |
| State | Backend **local** declarado, no remoto; state/backups/planes ignorados y ninguno rastreado ni generado |

Se conserva la estrategia inicial local, no Git, sin S3 o DynamoDB locking. No se inicializa backend ni se crea state artificial; no hay evidencia nueva de ACL/cifrado, respaldo/recuperación, operador/custodia aceptados o migración remota. Autenticación exitosa del CLI **no prueba** autenticación Terraform ni autorización de provisión: no se ejecuta plan ni se configura un destino real mediante tfvars. Los nueve archivos infra se conservan byte por byte durante este turno.

### 7.4. Dictamen por criterios originales y detención

- **T-INFRA-001: PARCIAL / BLOQUEADA POR DECISIONES.** Identidad/perfil y región de baseline acreditados; **presupuesto: DECISIÓN PENDIENTE**, sin monto inventado. El contrato también exige presupuesto/responsables aprobados por escrito y estimación fechada/regional: no existen esas nuevas aprobaciones. Duración, ambiente/AZ y ownership formal tampoco se cierran por la identidad IAM. No corresponde “bloqueada solo por presupuesto”. E(001) se amplía, no se acepta integralmente.
- **T-INFRA-002: PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE.** Versiones observadas, lock, formato/configuración y estrategia local comprobados en su alcance; dependencia T-INFRA-001 sigue sin aceptación. No se sustituyen aprobación/custodia de state y decisiones aplicables por validate verde. No COMPLETADA.
- **GATE-I-A: PENDIENTE / NO SATISFECHO.** Task §3/spec AC-I-01 exigen presupuesto/versiones/ambiente/state aprobados y E(001), E(002), **E(025)**. Identidad/región no eliminan esos requisitos; presupuesto y aprobaciones/evidencias faltantes bloquean cierre. T-INFRA-025 no ejecutada; otros ocho gates, DP y AC no cerrados.

Trazabilidad de este complemento: E(001) → INFRA-001–003/033/038, PLD-INFRA-001/020, AC-I-01/15; E(002) → INFRA-029–033/038, PLD-INFRA-015/016, AC-I-01/12; GATE-I-A y soporte I-H/I-I. Resultados saneados en esta sesión/documento; sin afirmar archivo externo o retención duradera de salidas nativas.

No recursos creados por el bloque, no políticas IAM agregadas, sin plan/apply, sin cambios de Authentication/Program.cs/tests/Dockerfile/004/spec/plan ni tareas posteriores. Acceso de consola deshabilitado es declaración del responsable, no verificación IAM acreditada. Estado Git esperado conserva dos documentos modificados y nueve archivos infra nuevos, 0 staged; HEAD sin cambios. Sin stage/commit/push. Se detiene tras validación y registro.

## 8. Presupuesto aprobado y preparación documental INFRA-2/3 — 2026-11-12

### 8.1. Decisión del responsable y alcance de la autorización

Solicitud **CIERRE DE PRESUPUESTO + PREPARACIÓN INFRA-2/3 / BUILD DOCUMENTAL-VALIDACIÓN CONTROLADA**, rama `feature/infrastructure`, HEAD `3b4760ea8387b1cabff56e5f5e0ba5ffe8acb932`. Se conservan los dos documentos modificados y nueve archivos infra nuevos preexistentes, sin stage. Solo se editan task/memoria; no se reescriben spec/plan ni se implementa networking/IAM.

| Concepto | Decisión registrada |
| --- | --- |
| Crédito total disponible informado por el responsable | **USD 100**; no consulta de Billing ni verificación independiente del saldo/condiciones en este bloque |
| Presupuesto operativo inicial ENVER | **USD 50 — APROBADO PARA BASELINE** |
| Reserva mínima objetivo | **USD 50**; preservar al menos ese importe del crédito informado |
| Costo estimado real | **PENDIENTE**; el crédito y el presupuesto no son una cotización ni garantía de cobertura |

Límite inicial para el conjunto de desarrollo, validación, despliegue y entrega académica; **no se interpreta como USD 50 mensuales ni por recurso**. La duración/ventanas de uso siguen por aprobar. La reserva es objetivo operativo, no control técnico de AWS: no se crean AWS Budgets/alarma ni tope automático. Si saldo/consumo/condiciones o estimación total pueden comprometerla, detener y someter revisión previa; esta aprobación no autoriza recursos ni cargos en este turno.

### 8.2. Control de costo y estrategia económica inicial

Antes de cualquier bloque que pueda generar cargos, exigir estimación fechada para `us-east-1`, horas/duración, cantidades/capacidad, retención/transferencia y margen, indicando partidas cubiertas o no por crédito. Revisar acumulado y saldo/reserva antes de mutar; no asumir Free Tier ni gratuidad de un servicio o de una cuenta con crédito.

Partidas obligatorias: **EC2, EBS/snapshots, IPv4/EIP, ALB/LCU, Redis, CloudWatch, ECR, Secrets/SSM/KMS, transferencia y Atlas si aplica**; además DynamoDB, endpoints, DNS y cualquier dependencia necesaria. Costos Atlas externos no se descuentan automáticamente del crédito AWS: registrar su cobertura/fuente por separado y revisar cualquier gasto adicional. La lista de servicios potenciales no es aprobación de su creación.

Estrategia inicial confirmada, sin implementación ni tipos/precios elegidos: **dos EC2 pequeñas en dos AZ**, ALB requerido por rúbrica, **evitar NAT Gateway inicialmente**, MongoDB Atlas externo, evaluar costo/compatibilidad de Redis antes de elegir ElastiCache, logs/retención mínimos necesarios para evidencia y seguridad, y apagado/eliminación controlada cuando no se use. Se conserva la comparación Redis de PLD-INFRA-009: no se selecciona una alternativa final ni se rebajan continuidad/revocación/fail-closed para ahorrar.

No NAT no equivale a salida gratis: resolver egreso para Atlas/AWS/SSM, costo de IPv4/EIP y eventual uso de endpoints antes de aprobar DP-I-03. EC2 detenida no elimina cargos de EBS, IPv4/EIP, ALB, Redis, logs u otros servicios persistentes. Procedimiento futuro: inventario/costos → autorización de parada/limpieza → protección de datos/continuidad de seguridad → verificación posterior de recursos/cargos residuales. **No destroy, borrado de datos, snapshots o reset de revocación/authority implícitos.**

### 8.3. Reevaluación contractual E(001)/E(002) y GATE-I-A

- **T-INFRA-001 — PARCIAL / BLOQUEADA POR DECISIONES.** Presupuesto USD 50 y reserva objetivo aprobados; identidad, perfil y región de baseline ya acreditados. Falta la **estimación regional fechada y lista cuantificada de cargos/recursos fuera del límite**, duración/horas, elegibilidad/vigencia/cobertura del crédito, responsables y ownership aprobados por escrito, ambiente/AZ y naming/tags aplicables. El criterio original de aceptación no se modifica: monto aprobado no equivale a estimación ni aceptación de DP-I-01/13 completas.
- **T-INFRA-002 — PARCIAL / BASELINE TÉCNICA VALIDADA, ACEPTACIÓN PENDIENTE.** Se reconocen Terraform 1.17.1, AWS lock 6.73.0 y estrategia inicial local como baseline técnico. **T-INFRA-001 es dependencia formal no aceptada.** El responsable incluye “state local protegido” en su baseline; lectura y evidencia previa confirman backend local/no Git, procedimiento documentado y ausencia de secretos/state real, pero no verifican ACL/cifrado, destino/operador, backup cifrado/recuperación y aprobación integral del manejo del state. No se inventa prueba de protección ni se genera state para completarla.
- **GATE-I-A — PENDIENTE / NO SATISFECHO.** El presupuesto ya no bloquea por falta de monto. Faltan aceptación integral de E(001)/E(002), ambiente/custodia aprobados y **E(T-INFRA-025)**. T-INFRA-025 está en INFRA-13 como **intervención temprana transversal**, no como validación posterior a construir toda la infraestructura: plan §§4–5 y task 025 exigen `001 → 002 → 025 → 003`. No hay circularidad ni autorización para omitirla; la validación final del conjunto es T-INFRA-026. Los comandos repetidos aquí no ejecutan ni aceptan automáticamente T-INFRA-025. Requiere autorización y evidencia propias antes de extender IaC.

Trazabilidad: E(001) → INFRA-001–003/033/038, PLD-INFRA-001/020, AC-I-01/15; E(002) → INFRA-029–033/038, PLD-INFRA-015/016, AC-I-01/12; GATE-I-A y soporte I-H/I-I. DP-I-01 queda **parcialmente resuelta** respecto al monto/región/crédito informado; no se cierra su resto ni otras DP por inferencia.

### 8.4. Recomendación del siguiente bloque — NO EJECUTADO / NO AUTORIZADO AHORA

Primero resolver los pendientes concretos de 001/002 y solicitar **validación inicial T-INFRA-025** con alcance/comandos/entorno/custodia/evidencia delimitados. Después solicitar un BUILD de **preparación de configuración**, sin plan/apply/provisión ni asignación de políticas al usuario, para 003–006:

| Tarea futura | Preparación recomendada / decisiones previas |
| --- | --- |
| T-INFRA-003 — Networking | VPC, dos subnets en AZ distintas, route tables e Internet Gateway; aprobar CIDR sin solapamientos, AZ y perfil red/egreso. Dos subnets públicas son opción inicial a evaluar para futuro ALB/hosts sin NAT, no topología definitiva ni alta disponibilidad integral |
| T-INFRA-004 — Salida Atlas/AWS | Aprobar egreso estable/allowlist Atlas, DNS y conectividad hacia AWS/SSM; contabilizar IPv4/EIP/endpoints y comparar con alternativas, sin Atlas `0.0.0.0/0`. Resolver DP-I-03/06 aplicables antes de aceptar |
| T-INFRA-005 — Security/IAM | Matriz llamada→acción→recurso/condición, roles separados runtime/Terraform/deploy/bootstrap. Runtime mínimo según integraciones reales, Terraform delimitado; no AdministratorAccess/PowerUserAccess, privilegios globales de infraestructura o PassRole irrestricto. Acciones que técnicamente requieran `Resource=*` deben justificarse/restringirse, no generalizarse |
| T-INFRA-006 — SG/administración | SG de ALB y backend, ingreso backend solo desde SG ALB; administración mediante SSM, sin SSH público ni Docker remoto. Resolver puerto/TLS interno/egreso de DP-I-05/09; SG ALB puede prepararse sin crear el ALB |

Archivos candidatos a autorizar después: `infra/networking.tf`, `infra/security.tf` y ajustes mínimos de variables/locals/outputs, más evidencia task/memoria. El IAM Terraform del operador se **propone/revisa**, no se autoasigna ni se incorpora al runtime; cualquier otorgamiento real requiere autorización específica y principal autorizado. **IAM permissions PENDIENTES PARA INFRA-3**; `enver-terraform-local` está autenticado según §7, pero STS y AccessDenied de lecturas no prueban privilegios efectivos ni autorizan provisión.

No adelantar `compute.tf`, ALB/listeners/targets, Redis, ECR, ACM, DynamoDB, Secrets ni recursos de esos servicios. Solo contratos/referencias no secretas para IAM futuro cuando sean necesarios, sin crearlos ni inventar ARNs/cuenta. No definir credenciales, puertos/CIDR finales, tamaños, Atlas tier o servicio Redis por iniciativa propia. La recomendación no ejecuta T-INFRA-003–006 ni modifica sus dependencias/criterios; siguen PENDIENTE / NO EJECUTADA.

### 8.5. Validación local y detención

En este turno: `terraform -version` confirma **1.17.1/windows_amd64**, exit 0, sin actualización; `terraform fmt -check -recursive infra` **exit 0**; `terraform -chdir=infra validate` **exit 0 / Success! The configuration is valid.** Sin init nuevo, plan/apply o llamadas AWS; identidad/región y CLI 2.40.1 se conservan como evidencia previa de §7, no se afirma una nueva consulta STS/Billing.

No recursos/políticas creados ni modificaciones en infra, Authentication/Program.cs/tests/Dockerfile/004/spec/plan. Los ocho gates restantes y T-INFRA-025 permanecen pendientes; presupuesto y esta recomendación no abren ejecución automática. Sin stage/commit/push; HEAD conservado. Detener al registrar decisión, pendientes y propuesta.

## 9. INFRA-2/3 — Definiciones candidatas, validación parcial y bloqueo de plan

### 9.1. Autorización, baseline y decisiones — 2026-11-12

El responsable solicita BUILD CONTROLADO NO APPLY de T-INFRA-003–006 y parte posible de T-INFRA-025. Lectura completa previa de spec/plan/task; revisión de Dockerfile, launchSettings, Program y appsettings sin editarlos. Baseline verificada: **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**, árbol limpio. Checkpoint Git previo autorizado preserva once archivos del baseline; este turno no hace stage/commit/push.

Confirmados para este bloque: **project enver, environment dev, us-east-1**, primera release Register/Login/Logout; Orders excluido. Crédito informado USD 100, presupuesto total inicial USD 50/reserva objetivo USD 50 conservados, no gratuidad ni aprobación de gasto. Autorización actual permite preparar candidatos pese a dependencias pendientes, no aceptarlas ni reformar spec/plan.

Archivos nuevos: `infra/networking.tf`, `infra/security.tf`, `infra/bootstrap/terraform-local-network-policy.json`. Modificados: variables/locals/outputs/terraform.tfvars.example/README de infra, este task y memoria. Main/backend, provider, versiones y lock permanecen intactos; no dependencias/providers nuevos.

### 9.2. E(T-INFRA-003) / E(T-INFRA-004): red y Atlas

- **Trazabilidad:** 003 → INFRA-007/033, PLD-INFRA-004/015, AC-I-02; 004 → INFRA-008/009/014, PLD-INFRA-004/008, AC-I-02/06; GATE-I-B y soporte I-F.
- Spec §6/PLD-INFRA-004 permiten **proponer A: ALB/EC2 en públicas, ingreso backend solo SG ALB**; la opción privada B no se elimina. Se define candidato A con VPC propuesta `10.20.0.0/16`, DNS support/hostnames, dos subnets, IGW y route table/ruta/asociaciones. No defaults de red: CIDR y bits adicionales son inputs obligatorios; ejemplo `/16 + 8` produce `10.20.0.0/24` y `10.20.1.0/24`. Contención/no solapamiento interno por derivación; redes externas aún no verificadas.
- AZ por `aws_availability_zones`, filtro available/availability-zone/opt-in habilitado; ordenar **Zone IDs**, tomar dos con claves a/b y precondición al menos dos. Sin nombres de AZ inferidos; IDs reales/aptitud de cómputo no acreditados por denegación. Revisar reemplazos si cambia el conjunto elegible.
- Ruta IPv4 `0.0.0.0/0` vía IGW por tabla compartida. `map_public_ip_on_launch=false`; sin IP/EIP no habrá salida IPv4 de hosts futuros aunque la subnet sea pública. **Sin NAT Gateway/instance/endpoints/EIP**. Redis privado sigue fase propia.
- DP-I-03 sigue abierta: `network_design_approved` obligatorio sin default y precondición impiden planear sin aceptación expresa de candidato/CIDR/egreso/costo. Ejemplo deja false; guard no sustituye autorización o gates. No se declara A aceptada por escribirla.
- Recomendación concreta: **dos EIP futuras, una por EC2**, IPs /32 explícitas en Atlas y reasociación controlada por reemplazo. Comparación detallada en [README](../../infra/README.md#atlas-e-ips-recomendacióncosto-no-aprovisionamiento): IP automática cambia con stop/start, NAT agrega costo/GB y alternativas privadas requieren componentes/capacidad/tier/operación. Sin aprobar/asignar EIP o tocar allowlist. Prohibido /0 Atlas. Runbook previo a admisión exige autorización, ambas salidas y TLS; no reset de seguridad ni suponer cierre inmediato de conexiones al retirar allowlist.
- **Costo fechado 2026-11-12:** [VPC pricing](https://aws.amazon.com/vpc/pricing/) acredita **USD 0.005/IP-hora**, in-use/idle, aplicable us-east-1. Dos IPv4 o dos EIP: **USD 0.01/h, USD 0.24/24 h, USD 7.30/730 h**, solo IP. Proyección mínima futura 2 backend + 2 ALB: **USD 14.60/730 h solo IP**, ALB puede usar más. No EC2/EBS, ALB horas/LCU, Redis/Atlas/logs/transferencia incluidos. VPC/subnets/rutas/IGW/SG definidos sin workloads/IPs: **USD 0 de cargo recurrente directo estimado**, no factura medida ni toda plataforma gratis. NAT no elegido; tarifa exacta actual Virginia/GB pendiente, no extrapolada de ejemplo Ohio. Total/calendario/crédito/ownership siguen pendientes antes de apply. EIP retenida con instancia detenida sigue costando; no limpiarla/restaurar datos sin permiso.

### 9.3. E(T-INFRA-005) / E(T-INFRA-006): permisos y SG

- **Trazabilidad:** 005 → INFRA-008/021/022/024, PLD-INFRA-005/011/012, AC-I-02/08/09; 006 → INFRA-008/009/013/024, PLD-INFRA-004/005/007, AC-I-02/05/09; GATE-I-B y soporte I-E/I-F.
- SG ALB: ingreso TCP 443 IPv4 público; **sin 80** hasta listener redirect aprobado; egreso solo SG backend/8080. SG backend: ingreso únicamente SG ALB/8080, sin 22/SSH, Docker remoto o ingress general de aplicación. SG no crea ALB/certificado ni demuestra bloqueo real; evitar otros SG permisivos/default en hosts futuros.
- **Puerto declarado 8080**, no inferido solo de EXPOSE: Dockerfile líneas 7–8 + `Properties/launchSettings.json` Container HTTP_PORTS=8080; Program/appsettings sin otro listener. Referencia oficial .NET confirma HTTP_PORTS=8080 en imagen base de referencia, pero no se inspecciona/ejecuta imagen de producción. 8081/useSSL local no prueba HTTPS; **TLS interno DP-I-05 pendiente**, mapeo host→contenedor por confirmar en compute. SG TCP candidato no decide HTTP frente a HTTPS.
- Egreso backend **IPv4 amplio provisional** (`0.0.0.0/0`, todos los protocolos), no ingress ni /0 Atlas: evita imponer una lista incompleta que rompa AWS/SSM/ECR/CloudWatch, Atlas, OCSP/TLS, bootstrap y Redis. Superficie de salida/exfiltración mayor, sin filtro FQDN ni aislamiento fino; reducir solo con diseño completo de destinos/endpoints/red, puertos/SG Redis y requisitos AMI/certificados. Sin IPv6. Resolver VPC DNS y conectividad SSM; sin agente/role/profile/Session Manager funcionales todavía. Egreso no aceptado como diseño final.
- Bootstrap **solo lectura**, JSON revisado localmente: nueve Describe de red/AZ/atributos/SG/rutas/tags, región us-east-1, usuario enver-terraform-local. `Resource=*` de lecturas regionales, sin ID de cuenta; consulta a [referencia EC2](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ec2.html) confirma operaciones. Propuesta para revisión/asignación manual por administrador autorizado, **no aplicada ni simulada en IAM**. Sin AdministratorAccess/PowerUserAccess, CreateTags/escrituras de red, RunInstances, EIP, ALB/ECR/ACM/Secrets/Redis/DynamoDB/CloudWatch, IAM/PassRole. Tagging existe solo en HCL; permiso de escritura no necesario para definir/validar sin apply. La futura provisión exige otra política lifecycle con condiciones/recursos revisados, no ampliar esta a ciegas.
- T005 exige separación/diseño y dice no crear trust aún: **sin rol/profile runtime Terraform en este bloque**. Matriz documental: operador Terraform (lecturas de esta fase), runtime EC2 (rol/profile futuro, no claves, no creación/borrado de tabla), operador SSM (sesiones delimitadas), publisher/deploy OIDC 004 y bootstrap de secretos/authority separados. No permisos prematuros SSM/ECR/logs/Secrets/DynamoDB; contratos DP-I-08/09/004 siguen pendientes.

### 9.4. Preflight AWS y E(T-INFRA-025) parcial

Con AWS CLI existente/perfil externo enver-infra, sin pager: STS **exit 0**, ARN coincide con user/enver-terraform-local, Account presente; región configurada us-east-1. Se capturan salidas en memoria y solo se muestran presencia/coincidencia; sin Account ID/ARN completo, claves/tokens o errores crudos en documentos/archivos.

`aws ec2 describe-availability-zones --profile enver-infra --region us-east-1 …` **exit 254 / UnauthorizedOperation**. Acción faltante **ec2:DescribeAvailabilityZones**, recurso conjunto regional de AZ (**Resource=* para esa lectura**), fase INFRA-2/T003 preflight. No se amplían permisos, cambia principal o reintenta contra otras cuentas. Políticas efectivas no auditadas integralmente; identidad no demuestra permisos suficientes.

**Plan NO ejecutado:** falla prerrequisito de permiso AZ y faltan decisiones críticas/inputs aceptados DP-I-03; no se usa cuenta ficticia, guard true o flags para saltar validación. Backend inicial tampoco se reinicializa por iniciativa propia. No hay planfile/salida de plan ni recuento de cambios observado. Lectura estática del HCL expande **14 instancias de recursos** (8 red, 2 SG y 4 reglas), un data source; **no es resultado de plan** ni infraestructura existente.

| Comprobación real | Resultado |
| --- | --- |
| `terraform -version` | **1.17.1 / windows_amd64**, exit 0; sin actualización |
| Lock/provider | **6.73.0**, sin modificación; no init/descargas nuevos |
| `terraform fmt -recursive infra` | Exit 0; normalizó security.tf |
| `terraform fmt -check -recursive infra` | Exit 0 |
| `terraform -chdir=infra validate` | Exit 0, **Success! The configuration is valid.** |
| JSON bootstrap parse/revisión local | Nueve acciones Describe exactas, región/usuario delimitados; no prueba de aceptación IAM |
| State/planes | Sin state/plan real ni rastreado; `.terraform/`/tfvars reales siguen ignorados |
| `git diff --check` | Exit 0; solo avisos LF→CRLF de configuración existente, sin errores de whitespace |
| `git status` / `git diff --stat` | Rama feature/infrastructure; siete archivos modificados sin stage y tres nuevos sin seguimiento. Stat rastreado no incluye esos tres nuevos |

**E(T-INFRA-025) PARCIAL:** INFRA-029/032/038, PLD-INFRA-015/016/019, AC-I-01/GATE-I-A; fecha/entorno/commit arriba. Se ejecuta ahora solo la porción autorizada posible, no se aprueban 001/002/custodia/backup ni se declara completada. Resultados saneados en sesión/task, sin inventar paquete externo o retención duradera.

### 9.5. Dictamen y límites

T003/004/005/006 quedan **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: configuración/propuestas no satisfacen decisiones, dependencias ni pruebas cloud. **GATE-I-A PENDIENTE / NO SATISFECHO** por 001/002/custodia/costo integral/ownership y aceptación completa E025. **GATE-I-B PENDIENTE / NO SATISFECHO**: exige GATE-I-A, E(003–006), E(019–020), E(027–028), VPC/rutas/SG/SSM/IAM/salidas Atlas/bypass realmente verificados. Validate verde no sustituye esos criterios. Ningún otro gate/AC cerrado.

No recursos AWS ni IAM policies aplicados; no apply/destroy/import/plan, EC2/ALB/EIP/NAT/Redis/ECR/ACM/Secrets/DynamoDB/CloudWatch, backend remoto, workflows, pruebas UT/IT/ST, producto/Authentication/Program.cs/Dockerfile o 004 modificados. Spec/plan permanecen intactos. Reversión futura solo de cambios propios bajo permiso, sin operaciones AWS/Git destructivas. Sin stage/commit/push; HEAD conservado. Detener tras este bloque.

## 10. INFRA-2/3 — Revalidación IAM/AZ e intento de plan

### 10.1. Alcance y baseline — 2026-11-12

Autorización **VALIDACIÓN CONTROLADA / NO APPLY**: revalidar STS/AZ, fmt/check/validate, intentar plan, registrar solo hechos en task/memoria. Rama verificada **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**. Se preservan los siete archivos modificados y tres nuevos de §9; 0 staged. No se edita HCL, provider, lock, política bootstrap, README de infra o spec/plan en este turno. El responsable informa **bootstrap IAM aplicada**; no se consulta IAM para certificar asignaciones/contenido/ausencia de privilegios amplios.

### 10.2. STS/AZ: evidencia real complementaria E(003)/E(005)

`aws sts get-caller-identity --profile enver-infra` (salida JSON capturada en memoria, sin pager): **exit 0**, ARN coincide con `user/enver-terraform-local`, Account presente y válido; se suprimen ID y ARN completos. La identidad se verifica nuevamente antes del intento de plan.

`aws ec2 describe-availability-zones --region us-east-1 --profile enver-infra` (JSON, sin pager): **exit 0**, seis AZ `available`, tipo `availability-zone`, opt-in `opt-in-not-required`. Mapeo observado **para esta cuenta**, no nombres inferidos:

| AZ | Zone ID |
| --- | --- |
| us-east-1a | use1-az1 |
| us-east-1b | use1-az2 |
| us-east-1e | use1-az3 |
| us-east-1c | use1-az4 |
| us-east-1f | use1-az5 |
| us-east-1d | use1-az6 |

El algoritmo existente de Zone IDs ordenados propone **a → use1-az1/us-east-1a**, **b → use1-az2/us-east-1b**. Queda acreditado mínimo dos AZ disponibles, no capacidad/compatibilidad de un futuro tipo EC2 ni aceptación definitiva del perfil de red. La denegación AZ de §9 es histórica y no se repite. Sin AccessDenied/UnauthorizedOperation nuevos en estas llamadas; **no se certifica aún suficiencia IAM para todo Terraform**.

### 10.3. E(T-INFRA-025): validaciones e intento fallido de plan

Terraform observado **1.17.1/windows_amd64**; provider/lock **6.73.0** conservados, sin actualización/descarga/init nuevos. Comandos `terraform fmt -recursive infra`, `terraform fmt -check -recursive infra` y `terraform -chdir=infra validate`: **exit 0**; validate: **Success! The configuration is valid.** Fmt no requirió modificaciones.

Para plan: perfil externo `AWS_PROFILE=enver-infra`, región us-east-1; `TF_VAR_*` solo en proceso: project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8 (deriva /24 .0 y .1), common_tags de ejemplo existente; aws_account_id obtenido de STS, no ficticio ni publicado. `network_design_approved=false` conserva DP-I-03 pendiente, sin inventar aceptación para lograr verde. Sin inputs automáticos/state inesperados; inspección previa limita resource/data types a red/SG/AZ existentes. Sin credenciales ambientales que sustituyan el perfil. Variables del proceso restauradas; sin tfvars real o planfile guardado, sin logging Terraform a archivo.

Intento ejecutado **una vez**:

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 1
Error: Backend initialization required, please run "terraform init"
Reason: Initial configuration of the requested backend "local"
```

Fallo **anterior a evaluación del provider/data source y recursos**, no IAM. La baseline inicial usó `init -backend=false`; no se ejecuta init/reconfigure/migrate-state por inferencia. **No hay resumen válido add/change/destroy ni data sources leídos por Terraform**; no se registra 0 destroy como resultado observado. La consulta AZ anterior fue AWS CLI, no un plan. Ningún permiso adicional propuesto: no hubo nueva acción denegada que lo justifique. Una política aplicada informada y AZ exit 0 no acreditan todas las llamadas futuras.

Lectura estática: **14 instancias de recursos definidos**, 8 red (VPC, dos subnets, IGW, route table, ruta IGW, dos asociaciones), 2 SG y 4 reglas; un data source AZ. No tipos fuera del alcance en el HCL revisado; **sin inventario de plan exitoso**. Sin EC2/ALB/ECR/Redis/DynamoDB/ACM/Secrets/CloudWatch/EIP/NAT definidos o creados. Costo conocido del diseño actual sin workloads/IP: **USD 0 recurrente directo estimado** de red básica/SG, conforme a fuentes de §9; sin nueva consulta Billing/precios ni gratuidad de plataforma, sin recursos facturables añadidos.

### 10.4. Reevaluación y detención

- **T-INFRA-003/004/005/006: PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, sin cambios de estado. AZ ahora acreditadas, no topología/egreso/allowlist Atlas, contratos IAM/runtime, SSM o TLS interno aceptados. E(004)/E(006) sin nueva prueba cloud.
- **T-INFRA-025: PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. Agrega intento fallido de plan y causa exacta; no completa state/custodia/backup o dependencias 001/002.
- **GATE-I-A PENDIENTE / NO SATISFECHO**: aceptación 001/002/025, costo integral, ownership y custodia/state aún incompletos. **GATE-I-B PENDIENTE / NO SATISFECHO**: exige I-A, E(003–006), E(019–020), E(027–028) y verificaciones cloud. Nueve gates permanecen pendientes.
- Antes de reintentar plan: autorización específica de inicialización del backend local, revisión de custodia/state/backup y decisiones/inputs pendientes; no tratar `network_design_approved=true` como mero arreglo técnico. Un init futuro no autoriza apply, migración, permisos extra o provisión.

Comprobación Git: `git diff --check` **exit 0**, solo avisos de normalización LF→CRLF; `git status` confirma siete modificados/tres nuevos y **0 staged**, `git diff --stat` revisado. Lock/provider/versiones/backend/spec/plan sin cambios respecto a HEAD; no state, planfile, tfvars real o lock de state en infra. No se agregan archivos al índice para revisar los nuevos.

Sin mutaciones AWS/IAM, apply/destroy/import, init, pruebas backend, workflows, producto o 004. Solo se actualizan este task y memoria, manteniendo §9 como historial. Sin stage/commit/push; HEAD conservado. Detener al registrar esta validación.

## 11. Backend local inicializado y plan INFRA-2/3 válido

### 11.1. Autorización y decisión acotada — 2026-11-12

Solicitud **INICIALIZAR BACKEND LOCAL + PLAN INFRA-2/3 / VALIDACIÓN CONTROLADA NO APPLY**. Baseline verificada: **feature/infrastructure**, HEAD **611071f0ecb0d6cb67993f4e9bffd00d328a2f90**, siete modificados/tres nuevos preexistentes, 0 staged. Solo se editan task/memoria; HCL, ejemplo tfvars, README de infra, política bootstrap, spec/plan, backend fuente, versiones y lock se conservan.

**Network design APROBADO PARA DISEÑO Y PLAN, NO PARA APPLY**: opción A, ALB y dos EC2 futuros en públicas, backend solo SG ALB, SSH público prohibido, SSM futuro, sin NAT Gateway; dos EIP futuras, una por host para salida Atlas; TLS público futuro ALB+ACM. Inputs enver/dev/us-east-1, VPC **10.20.0.0/16**, subnets **10.20.0.0/24** y **10.20.1.0/24**. La aprobación habilita `network_design_approved=true` en este plan, no cargos/provisión, EIP implementadas, TLS interno decidido, segmentación fina de egreso aceptada o aceptación integral DP-I-03/otras decisiones.

### 11.2. Init y validación: E(002)/E(025) parciales

Un prechequeo de hashes falló porque el cmdlet Get-FileHash no estaba disponible, **antes de invocar Terraform**; se sustituyó por SHA-256 de .NET en memoria, sin instalar herramientas. Init se ejecutó una sola vez, no interactivo para detenerse en lugar de aceptar migraciones inesperadas:

```text
terraform -chdir=infra init -input=false -lockfile=readonly -no-color
Exit: 0
Successfully configured the backend "local"!
Reusing version 6.73.0 of hashicorp/aws from the dependency lock file
Using previously-installed hashicorp/aws v6.73.0
Terraform has been successfully initialized!
```

Metadata local comprobada en `infra/.terraform/terraform.tfstate`: tipo local, path terraform.tfstate; **no es un state de recursos aplicados**. Sin reconfigure/migrate-state, S3, backend remoto o DynamoDB locking. Hashes de fuentes infra/bootstrap/lock idénticos antes/después de init y fmt; no upgrade/descarga de provider nuevo. `terraform fmt -recursive infra`, `terraform fmt -check -recursive infra` y `terraform -chdir=infra validate`: **exit 0**, validate **Success! The configuration is valid.**

`git check-ignore --no-index` confirma exclusiones de terraform.tfstate, .backup y sufijos, `.terraform/` y tfvars real; lock y tfvars.example siguen versionables. Ninguno de esos artefactos sensibles rastreado. Tras plan: **sin infra/terraform.tfstate de recursos, planfile ni tfvars real**; solo metadata de backend bajo `.terraform/` ignorada. No se demuestra cifrado/ACL/custodia/backup/restauración por inicializar o ignorar archivos.

### 11.3. Plan real: E(003–006)/E(025) parciales

STS previo al plan, perfil externo **enver-infra**, **exit 0**, usuario esperado enver-terraform-local y Account válido sin publicarlos completos. Inputs no secretos solo en proceso: enver/dev/us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8, common_tags={} (default existente), cuenta guard obtenida de STS. CLI para **network_design_approved=true**; sin alterar ejemplo o escribir tfvars. Sin credenciales ambientales que sustituyan perfil, TF_CLI_ARGS o logging a archivo; variables restauradas al terminar. Un único plan, sin -out/planfile:

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode -var=network_design_approved=true
Exit: 2
data.aws_availability_zones.available: Read complete after 0s [id=us-east-1]
Plan: 14 to add, 0 to change, 0 to destroy.
```

**Exit 2 es éxito con cambios previstos** usando detailed-exitcode, no fallo. Inventario contrastado exactamente contra direcciones autorizadas:

| Tipo/dirección | Add |
| --- | --- |
| aws_vpc.enver | 1 |
| aws_subnet.public["a"], ["b"] | 2 |
| aws_internet_gateway.enver | 1 |
| aws_route_table.public | 1 |
| aws_route.public_internet | 1 |
| aws_route_table_association.public["a"], ["b"] | 2 |
| aws_security_group.sg_alb, sg_backend | 2 |
| aws_vpc_security_group_ingress_rule.alb_https, backend_from_alb | 2 |
| aws_vpc_security_group_egress_rule.alb_to_backend, backend_initial | 2 |
| **Total recursos / change / destroy** | **14 / 0 / 0** |

Data source **aws_availability_zones.available leído**, no recurso a crear. Plan confirma **a: use1-az1 / 10.20.0.0/24**, **b: use1-az2 / 10.20.1.0/24** (mapeo nombres us-east-1a/us-east-1b acreditado por CLI en §10). VPC /16, DNS support/hostnames true, IP automática de subnets false y ruta default IPv4 vía IGW. SG/reglas: 443 público solo ALB, backend/8080 solo SG ALB, egreso ALB a backend/8080, egreso backend IPv4 amplio provisional. Sin 22/80, SG no acredita escucha, SSM o TLS.

**Sin recursos inesperados**: no instancias EC2, EIP, ALB/target group, ECR, ACM, Redis/ElastiCache, DynamoDB, Secrets, CloudWatch, Route53 o NAT Gateway. **Sin AccessDenied** en este plan; **sin permiso incremental requerido observado**, ni políticas modificadas. Este plan inicial no certifica permisos de apply/refresh futuro, todas las acciones bootstrap o ausencia de privilegios amplios. Sin eludir allowed_account_ids ni crear state/recursos para probar permisos.

Costo directo recurrente estimado de **este inventario sin workloads/IP asignadas: USD 0** por VPC/subnets/IGW/rutas/SG, según fuentes previamente acreditadas (§9), sin nueva consulta Billing ni gratuidad integral. Ningún componente facturable de workloads/IP/NAT añadido por este plan. **Dos EIP futuras ≈ USD 7.30 por 730 h (~mes)** a USD 0.005/IP-h previamente acreditado: fuera del inventario y **NO autorizadas para apply**. EC2/ALB futuros tampoco se cuentan aquí; costo integral/duración/crédito/reserva siguen sus pendientes.

### 11.4. Reevaluación y detención

**DEFINIDO**: HCL existente. **PLANIFICADO**: 14 creaciones de red/SG confirmadas por plan válido. **CREADO EN AWS**: ninguno por este bloque. E(003–006)/E025 amplían la trazabilidad de §§9–10 con plan real; no se inicia T026/revisión integral de todos los servicios ni se crea evidencia cloud.

T003/004/005/006 conservan **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, ahora con plan acreditado de sus componentes presentes. T004 no tiene EIP/allowlist/conectividad; T005 no completa contratos IAM/runtime ni permisos de provisión; T006 no tiene pruebas bypass/SSM/TLS interno/egreso final. T025 conserva **PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: init/plan no sustituyen custodia/backup/recuperación, dependencias y aceptación explícita. No se completan tareas por comandos verdes.

**GATE-I-A PENDIENTE / NO SATISFECHO** por 001/002, custodia/recuperación state, ownership y E025 integral. **GATE-I-B PENDIENTE / NO SATISFECHO**: exige I-A y verificaciones cloud E003–006/E019–020/E027–028. Nueve gates pendientes; spec/plan originales intactos, decisiones para apply todavía no aceptadas integralmente.

Comprobación Git: **git diff --check exit 0**, solo avisos LF→CRLF; status siete modificados/tres nuevos preexistentes, **0 staged**; stat revisado (no incluye nuevos sin seguimiento). HEAD y baseline protegida conservados; sin patrones básicos de secretos detectados en task/memoria (no análisis exhaustivo).

Sin apply/destroy/import, IAM mutado, recursos AWS creados, workloads, CI/CD, pruebas backend o producto modificado. Sin stage/commit/push; HEAD conservado. Detener tras documentar init/plan y pendientes, sin autorización implícita para siguiente tarea o apply.

## 12. IAM write mínima candidata para INFRA-2/3 — PREPARADA / NO APLICADA

### 12.1. Alcance y evidencia base — 2026-11-12

Baseline verificada **feature/infrastructure**, HEAD **f1a98a01e15c67297287133c27dc23279a649d09**, árbol limpio y 0 staged. Checkpoint previo `feat(infra): define networking and security baseline` integra los diez archivos revisados de INFRA-2/3; no acredita apply. Plan de §11 **14 add / 0 change / 0 destroy** conservado, no repetido porque ningún `.tf` cambia.

Se crea únicamente [terraform-local-network-apply-policy.json](../../infra/bootstrap/terraform-local-network-apply-policy.json); se actualizan task/memoria. [Read policy](../../infra/bootstrap/terraform-local-network-policy.json), HCL/variables/locals/outputs, lock, README, ejemplo tfvars, spec/plan y producto **intactos**. La read policy sigue informada aplicada; la write propuesta **NO está aplicada, adjunta, simulada ni validada por el servicio IAM**. No se consultan/modifican APIs de la cuenta AWS.

### 12.2. Derivación provider 6.73.0 y acciones Allow

Fuentes leídas del tag **v6.73.0**: [VPC](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_.go), [subnet](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_subnet.go), [IGW](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_internet_gateway.go), [tabla](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_route_table.go), [ruta](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_route.go), [asociación](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_route_table_association.go), [SG](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_security_group.go), [ingress/framework](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_security_group_ingress_rule.go), [egress](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_security_group_egress_rule.go) y [tags](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/tags_gen.go). Revisión pública por lectura, no ejecución de esas fuentes.

**22 acciones write únicas**, sin Describe duplicado:

| Recurso/operación | Allow ec2: incluido | Motivo observado |
| --- | --- | --- |
| VPC | CreateVpc, DeleteVpc, ModifyVpcAttribute | Crear/eliminar VPC; habilitar DNS. Modify limitado a **EnableDnsSupport/EnableDnsHostnames**, no métricas/tenancy |
| Dos subnets | CreateSubnet, DeleteSubnet | Creación IPv4 dedicada y eliminación |
| IGW | CreateInternetGateway, AttachInternetGateway, DetachInternetGateway, DeleteInternetGateway | Ciclo attach/detach y creación/eliminación |
| Tabla/asociaciones | CreateRouteTable, DeleteRouteTable, AssociateRouteTable, DisassociateRouteTable | Tabla pública y asociaciones de las dos subnets |
| Ruta | CreateRoute, DeleteRoute | Default IPv4 a IGW, no tag propio |
| SG | CreateSecurityGroup, DeleteSecurityGroup | Dos grupos propios |
| Reglas SG | AuthorizeSecurityGroupIngress, AuthorizeSecurityGroupEgress, RevokeSecurityGroupIngress, RevokeSecurityGroupEgress | Crear/eliminar cuatro reglas; **RevokeSecurityGroupEgress se usa también en Create del SG para quitar egreso IPv4/IPv6 predeterminado**, aun sin IPv6 desplegado |
| Tags al crear | CreateTags | Autorización adicional exigida para TagSpecifications atómicas en los seis tipos etiquetables |

**Excluidos por no pertenecer al create/delete del plan actual:** ReplaceRoute (Update), ReplaceRouteTableAssociation (Update), ModifySecurityGroupRules (Update), DeleteTags y CreateTags independiente (actualización de tags, no requerido para borrar recursos). ModifySubnetAttribute no se concede: con subnets IPv4 nuevas/no default y atributos del HCL/defaults false, el helper de creación modifica solo si difieren; map_public_ip_on_launch=false coincide con el default de creación. No se extiende por intuición a IPv6, DNS64, Outposts u otros atributos. Una divergencia real futura exige revisión incremental, no ampliar preventivamente. No IPAM, tenancy, endpoints, ENI create/delete/detach o limpieza de dependencias ajenas. No adoptar esta policy para un plan con updates/configuración distinta sin nueva revisión.

### 12.3. ARN, condiciones y tags

Contraste con [EC2 Service Authorization Reference](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ec2.html), [tag-on-create](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/supported-iam-actions-tagging.html), [variables IAM](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_variables.html) y [claves globales](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_condition-keys.html). Las mayúsculas de **ec2:Attribute = EnableDnsSupport/EnableDnsHostnames** se contrastan con el [ejemplo oficial AWS](https://docs.aws.amazon.com/aws-managed-policy/latest/reference/AWS-SSM-RemediationAutomation-ExecutionRolePolicy.html); solo se toma ese detalle técnico, **no se propone adjuntar esa política amplia**.

- **Ningún Resource literal `"*"` en la write policy**; todos son ARN `arn:aws:ec2:us-east-1:*:<tipo>/*`. Tipos: vpc, subnet, internet-gateway, route-table, security-group y security-group-rule. Cada `/*` es necesario porque sus IDs todavía no existen; no wildcard de servicio/región/tipo/acción. Rutas y asociaciones no tienen ARN/tag propio en estas autorizaciones: se controlan por tabla/subnet existentes.
- Wildcard **de cuenta en ARN** evita publicar/hardcodear Account ID; **en cada statement** `aws:ResourceAccount = ${aws:PrincipalAccount}` lo limita al dueño de la cuenta del usuario. Las acciones seleccionadas no están en las excepciones EC2 documentadas para ResourceAccount. La variable se usa en **Condition**, no en el campo cuenta del ARN: IAM no permite variables antes del quinto `:`. Si falta la clave requerida se deniega; no StringEqualsIfExists que abra el control.
- Cada statement exige **aws:RequestedRegion=us-east-1** y **aws:username=enver-terraform-local**. RequestedRegion está disponible para estos endpoints EC2 regionales; username lo está para IAM users, no roles/root/SSO: no se supone portabilidad de esta policy a roles futuros.
- Creación: **RequestTag Project=enver, Environment=dev, ManagedBy=Terraform**; recursos principales además exigen Name dentro de los nombres existentes en locals/HCL. Mutación/eliminación y referencias a padres: **ResourceTag** de control y Name existente de VPC/IGW/tabla/subnets/SG. CreateSubnet/CreateRouteTable/CreateSecurityGroup autorizan por separado el ARN nuevo con RequestTag y la VPC padre con ResourceTag; attach/detach IGW cubren ambos recursos. Asociaciones cubren tabla y subnets. No se exige ResourceTag sobre recursos aún inexistentes ni RequestTag en rutas/revokes/delete.
- Provider pasa **TagSpecifications en creación** para VPC/subnet/IGW/tabla/SG y reglas standalone: no hay un intervalo sin tags que haga imposible satisfacer el create normal. Reglas SG usan ARN del SG con ResourceTag y ARN nuevo de regla con RequestTag. AWS solo evalúa el segundo nivel cuando se envía TagSpecifications; **esto no es garantía universal de tags en llamadas manuales sin TagSpecifications**. Revokes se autorizan contra SG padre etiquetado, no contra tags de la regla predeterminada sin etiquetar.
- **CreateTags únicamente con ec2:CreateAction** en las siete APIs de creación/authorize usadas; no permite retaggear/adoptar recursos existentes. TagKeys se limita a **Project, Environment, ManagedBy, Name** (las tres primeras obligatorias por RequestTag). Las reglas no tienen Name en HCL y no se les exige. Cualquier common_tags futuro fuera de esas claves exige revisión; el plan acreditado usó common_tags={}.

**Límites explícitos:** tags/nombres no son IDs exclusivos ni un límite de cardinalidad; esta policy no impone exactamente 14 recursos, CIDR/puertos/destinos exactos ni demuestra quién creó cada recurso preexistente con iguales tags/nombres. La autorización operativa sigue limitada al plan revisado; antes de asignación/apply debe revisarse ausencia de colisiones/adopción, otras políticas/boundaries/SCP y, cuando haya IDs, evaluar pinning para cleanup. No permite retagging por sí misma, pero no neutraliza grants amplios de otras políticas ni cambios de tags por otros principales; es Allow acotado, **no permissions boundary**. No se finge una restricción IAM sobre campos sin condición soportada.

### 12.4. Describe adicionales y rollback pendiente

Read policy **sin modificar**; se documenta propuesta mínima separada para revisión/autorización posterior:

| Describe adicional | Evidencia en 6.73.0 / necesidad |
| --- | --- |
| **ec2:DescribeNetworkAcls** | VPC Read llama findVPCDefaultNetworkACL para atributo computado del NACL automático; un error allí se registra como warning, pero impide lectura íntegra. No autoriza crear/modificar/borrar NACL |
| **ec2:DescribeNetworkInterfaces** | [deleteLingeringENIs](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ec2/vpc_network_interface.go#L1738) se invoca antes de Delete de subnet y SG, incluso cuando no hay ENIs. Su error sí bloquea eliminación |

Ambas lecturas requieren **Resource `"*"`**: SAR no ofrece tipos ARN para esas acciones; conservar región us-east-1 y usuario enver-terraform-local de la read policy. **No incluidas en la write ni concedidas**, no se prueba IAM por llamadas AWS. Se deberá autorizar explícitamente la ampliación read aparte; la policy write más la read actual **no se declaran suficientes para rollback integral** mientras falte DescribeNetworkInterfaces. No se agregan DescribeVpcEndpoints o permisos GuardDuty/ENI para caminos de limpieza de servicios no desplegados; ante dependencias inesperadas, detener y revisar sin borrar componentes ajenos.

Los deletes/detach/disassociate/revokes preparados solo alcanzan el cohort de red etiquetado/nombres de esta fase, sujeto a las limitaciones de §12.3. AWS crea defaults implícitos al crear una VPC; no se añaden recursos Terraform o permisos de gestión de NACL/SG/tabla default. DeleteVpc elimina sus defaults en el ciclo normal sin workloads. Preparar eliminación **NO autoriza terraform destroy ni rollback**; requieren permiso propio, plan/inventario/estado y costo revisados, orden inverso de dependencias y custodia recuperable.

### 12.5. Validación, costo y estado

JSON parseado localmente: **válido**, 9 statements Allow, **22 acciones exactas**, todas EC2 específicas; assertions locales de ARN/condiciones/tags/ec2:Attribute/CreateAction correctas. **5318 caracteres compactados**, por debajo de 6144 de una customer-managed policy; excede 2048 agregados de inline policies de IAM user: no proponer pegarla como inline del usuario. Esto no crea ni asigna una managed policy; formato/asignación requieren revisión del responsable administrativo. **No hay validación semántica IAM/simulación o eficacia acreditada**. AWS CLI no aporta un validador semántico IAM puramente local: Access Analyzer validate-policy es una API remota; no se llama ni se usa create/put/attach policy para validar.

`terraform fmt -check -recursive infra` **exit 0**; `terraform -chdir=infra validate` **exit 0 / Success! The configuration is valid.** HCL/read policy/lock intactos, plan **14/0/0** previo conservado, no repetido. JSON no está referenciado por HCL: no modifica el inventario ni despliega permisos. Sin permisos RunInstances/TerminateInstances, EIP/NAT, ELB/ECR/ElastiCache/DynamoDB/Secrets/ACM/Route53/CloudWatch/IAM/SSM/KMS/S3/AutoScaling; sin ec2:* o acciones wildcard, AdministratorAccess/PowerUserAccess.

Sin recursos/cargos nuevos por preparar el archivo. Inventario de red básica/SG sin workloads/IP sigue con **USD 0 recurrente directo estimado** según §11; no se habilitan EC2/ALB/EIP/NAT, no aprobación de gasto integral ni cierre de presupuesto/custodia/ownership. **T-INFRA-005 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**; **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. **GATE-I-A e I-B PENDIENTE / NO SATISFECHO**, nueve gates pendientes; ninguna tarea completada por este documento. T003/004/006 y 001/002 conservan sus pendientes.

Sin apply/destroy/import/init/plan, AWS/IAM modificado, pruebas backend, workflows o producto alterado. Solo nuevo JSON y task/memoria; sin stage/commit/push, HEAD conservado. Detener tras preparar propuesta y registrar sus límites, sin aplicar políticas o infraestructura.

## 13. IAM Network Read/Apply informadas aplicadas y replan pre-apply — NO APPLY

### 13.1. Autorización, fuentes y límites — 2026-11-12

Solicitud **VALIDAR IAM NETWORK APPLY + REPLAN PRE-APPLY / VALIDACIÓN CONTROLADA / NO APPLY**. Baseline comprobada **feature/infrastructure**, HEAD **f1a98a01e15c67297287133c27dc23279a649d09**. Árbol inicial: task/memoria modificados y write JSON sin seguimiento, del incremento §12; **0 staged**, se conserva ese trabajo. Solo task/memoria editados ahora; archivos infra, JSON read/write, HCL, lock, ejemplo, spec/plan/004 y producto intactos.

**Informado por el responsable, no enumerado remotamente:**

- **IAM Network Read: APLICADA**, inline **ENVER-Terraform-Network-Read**, ahora con DescribeNetworkAcls/DescribeNetworkInterfaces.
- **IAM Network Apply: APLICADA**, customer-managed **ENVER-Terraform-Network-Apply**, adjunta directamente a enver-terraform-local desde el JSON local de §12.
- Responsable declara sin AdministratorAccess/PowerUserAccess/ec2:*. No se auditan otras policies, contenido remoto, versiones, boundaries o SCP; esa ausencia global no queda independientemente acreditada.
- **Terraform apply: TODAVÍA NO AUTORIZADO.** Asignación IAM no sustituye autorización operativa, gates o revisión de costos/state. Ningún cambio AWS/IAM por el agente.

El JSON read del repositorio conserva las nueve acciones históricas y **no representa íntegramente la ampliación remota informada**; no se sincroniza porque esta solicitud solo autoriza editar task/memoria. §12 conserva su estado cronológico de policy preparada/no aplicada; este registro supera aquel pendiente de asignación **según confirmación del responsable**, no reescribe su historia ni acredita rollback integral.

### 13.2. Preflight y lecturas reales

Primer prechequeo local terminó con exit 1 antes de ejecutar AWS/Terraform: AWS CLI no estaba en la ruta supuesta bajo Program Files. Se localizó la instalación existente en `%LOCALAPPDATA%/Programs/Amazon/AWSCLIV2/aws.exe`; Terraform en su directorio WinGet existente. Sin instalación, cambio de PATH/configuración o reintento oculto de plan. Después, una ejecución de cada STS/Describe/fmt/validate/plan.

Sin overrides ambientales de credenciales, TF_VAR, TF_CLI_ARGS o TF_LOG; sin tfvars reales/autoload o state de recursos preexistente. Perfil **enver-infra**, región **us-east-1**; valores e identificadores sensibles no publicados. Salidas capturadas en memoria de proceso, sin archivos crudos/planfiles/tfvars reales guardados.

| Comando | Resultado observado |
| --- | --- |
| aws sts get-caller-identity --profile enver-infra | **exit 0**, ARN de IAM user enver-terraform-local y cuenta válida; cuenta usada solo para allowed_account_ids, no publicada |
| aws ec2 describe-network-acls --region us-east-1 --profile enver-infra | **exit 0**, 1 ACL existente devuelta; IDs/reglas suprimidos; no creada por este turno |
| aws ec2 describe-network-interfaces --region us-east-1 --profile enver-infra | **exit 0**, 0 interfaces devueltas |
| terraform fmt -check -recursive infra | **exit 0** |
| terraform -chdir=infra validate -no-color | **exit 0**, Success! The configuration is valid. |

Read permissions **suficientes para las llamadas efectivamente ejecutadas**, incluidas las dos adicionales. No acredita todas las lecturas de recursos futuros: sin recursos gestionados existentes, el replan no ejercita cada Read del provider. El anterior bloqueo documentado por falta de esas dos lecturas está superado para las llamadas directas; no se extrapola a writes/rollback.

Revisión local write JSON: parseo válido, **9 statements Allow / 22 acciones únicas** idénticas al catálogo §12; sin NotAction, acciones wildcard o fuera de allowlist. **Sin RunInstances/TerminateInstances/AllocateAddress/AssociateAddress/CreateNatGateway ni ELB/IAM/ECR/ElastiCache/DynamoDB/Secrets/ACM/Route53/CloudWatch/SSM/KMS/S3/AutoScaling**. ARN/condiciones/tags se conservan, así como limitaciones de §12. No auditoría del JSON remoto ni prueba de permisos efectivos de escritura por el éxito del plan.

### 13.3. Único replan, inventario y comparación

Inputs temporales de proceso: **project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8, network_design_approved=true, common_tags={}**, y aws_account_id tomado del STS verificado como en §11. `AWS_PROFILE=enver-infra`; variables restauradas al terminar. Sin modificar ejemplos o escribir tfvars; no init ni plan -out.

```text
terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 2 (éxito con cambios previstos)
data.aws_availability_zones.available: Read complete after 1s [id=us-east-1]
Plan: 14 to add, 0 to change, 0 to destroy.
```

Se comparan **las 14 direcciones exactas** con §11, no solo la suma; coinciden, sin recurso añadido/retirado, update o destroy:

| Direcciones reales de creación | Add |
| --- | --- |
| aws_vpc.enver | 1 |
| aws_subnet.public["a"], public["b"] | 2 |
| aws_internet_gateway.enver | 1 |
| aws_route_table.public | 1 |
| aws_route.public_internet | 1 |
| aws_route_table_association.public["a"], public["b"] | 2 |
| aws_security_group.sg_alb, sg_backend | 2 |
| aws_vpc_security_group_ingress_rule.alb_https, backend_from_alb | 2 |
| aws_vpc_security_group_egress_rule.alb_to_backend, backend_initial | 2 |
| **Total add / change / destroy** | **14 / 0 / 0** |

Data source AZ leído, no creación. Subnets **10.20.0.0/24 / use1-az1** y **10.20.1.0/24 / use1-az2**; VPC /16, DNS support/hostnames true, map_public_ip_on_launch false, ruta 0.0.0.0/0 a IGW. Reglas sin diferencia contra baseline: ALB HTTPS 443 público, backend 8080 exclusivamente desde SG ALB, egreso ALB a backend 8080, backend IPv4 amplio **provisional**; no SSH/80. No se certifican puertos reales, TLS/SSM o bypass.

**Recursos inesperados: ninguno.** No instancias EC2, ALB, EIP, NAT Gateway, Redis/ElastiCache, IAM, ECR, DynamoDB o secretos. **AccessDenied/UnauthorizedOperation: ninguno** en STS, dos Describe y plan. **Permiso mínimo adicional observado: ninguno**; no se propone ampliar IAM preventivamente. No hay acción/recurso/fase denegada que registrar. Esto **no prueba Create/Delete/Attach/Modify/Authorize/Revoke/CreateTags**, no ejecutados; sin simulación IAM o dry-run write.

Hashes SHA-256 antes/después de fuentes infra/policies/lock/README/ejemplo **sin cambios (0)**; comandos fmt/validate/plan no alteran esos archivos. Sin planfile guardado: esta revisión no garantiza el resultado de un apply futuro; requiere nuevo alcance explícito y revisión vigente antes de provisión.

### 13.4. Costos, reevaluación y detención

**Ningún costo directo nuevo identificado en el inventario contra baseline.** Se conserva la estimación previa **USD 0 recurrente directo para VPC/subnets/IGW/rutas/SG sin workloads/IP**, no costo cero absoluto, factura consultada o gratuidad de plataforma. No tráfico/workloads/IP/NAT incluidos, sin nueva consulta precios/Billing. EC2/ALB/EIP/Redis siguen fuera de este plan; costo integral, duración, cobertura/vigencia del crédito, reserva y ownership pendientes.

- **T-INFRA-003 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** inventario repetido y coincidente; dependencias 001/002/025, solapamientos externos y aceptación restantes pendientes.
- **T-INFRA-004 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** ruta planificada; sin EIP/allowlist Atlas /32 o conectividad real, ni costo/egreso integral aceptados.
- **T-INFRA-005 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** asignación Read/Apply informada, dos lecturas acreditadas; writes/rollback no probados, separación runtime/deployment/SSM/secretos/authority, políticas efectivas y aceptación integral pendientes.
- **T-INFRA-006 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE:** SG/reglas planificados sin cambios; faltan cloud/bypass/SSM/TLS interno/egreso final.
- **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE:** fmt/validate/replan reales; custodia/backup/recuperación/state, dependencias y E025 integral pendientes.
- **GATE-I-A PENDIENTE / NO SATISFECHO:** ownership/costos/state y aceptación E001/E002/E025 pendientes. **GATE-I-B PENDIENTE / NO SATISFECHO:** I-A y evidencias cloud E003–006/E019–020/E027–028 pendientes. Nueve gates pendientes, ninguna tarea completada ni salto automático a 026/027.

**DEFINIDO:** HCL. **PLANIFICADO:** 14 creaciones. **CREADO EN AWS por este turno:** ninguno. IAM configurado previamente por el responsable se registra aparte, no como provisión Terraform. Sin apply/destroy/import, recursos manuales, IAM mutado por el agente, backend tests o CI/CD; sin stage/commit/push. HEAD conservado, solo task/memoria editados; detener sin siguiente incremento automático.

## 14. Primer apply controlado — red base INFRA-2/3 CREADA Y VERIFICADA

### 14.1. Autorización y gate operativo — 2026-11-12

Solicitud **PRIMER APPLY CONTROLADO — NETWORK BASELINE INFRA-2/3 / BUILD CLOUD CONTROLADO**: autorización expresa solo de 14 recursos gestionados de red/SG, condicionada a plan guardado revisado, sin workloads, cambios IAM o destroy/import. No acepta tareas/gates integralmente. HEAD de infraestructura aplicado **0958e83b887a73e1bdae2ff8b25753f5a52d0f0c**, rama **feature/infrastructure**, working tree **limpio / 0 staged** antes del plan y nuevamente antes del apply. Commit previo `chore(infra): prepare network apply permissions` contiene exactamente JSON write/task/memoria; no se hace commit ahora.

STS **exit 0**, perfil **enver-infra**, IAM user **enver-terraform-local** confirmado; cuenta validada y usada para allowed_account_ids sin publicarla. Terraform **1.17.1** / provider **6.73.0** comprobados localmente con version -json. Backend **local**, path terraform.tfstate, ya inicializado: **init no necesario/no ejecutado**, sin migración/reconfigure/backend remoto. No state de recursos ni archivo enver-network.tfplan preexistentes; no se sobrescribe trabajo ajeno. Sin overrides de credenciales/TF_VAR/TF_CLI_ARGS/TF_LOG ni tfvars reales/autoload; inputs temporales y entorno restaurados al terminar.

**Inputs exactos:** project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8 → subnets 10.20.0.0/24 y 10.20.1.0/24; network_design_approved=true; common_tags={} y account guard de STS como baseline previa. No nuevos destinos/valores de arquitectura. `terraform fmt -check -recursive infra` **exit 0** y `terraform -chdir=infra validate -no-color` **exit 0 / Success! The configuration is valid.**

### 14.2. Plan guardado revisado y trazabilidad

```text
terraform -chdir=infra plan -input=false -no-color -out=enver-network.tfplan -detailed-exitcode
Exit: 2
Plan: 14 to add, 0 to change, 0 to destroy.
terraform -chdir=infra show -no-color enver-network.tfplan
Exit: 0; Plan: 14 to add, 0 to change, 0 to destroy.
```

Un solo plan pre-apply generado, timestamp interno **2026-11-12T19:04:08Z**. Se revisan salida humana y `show -json` en memoria: **14 direcciones exactas de §13**, todas con acción create, 0 update/delete, ningún tipo prohibido; tags, VPC/CIDR, subnets/ZoneIds, DNS, IP automática false y cuatro reglas/relaciones SG coinciden con HCL. Data source AZ leído, no recurso creado; checks del plan **pass**.

La primera comprobación auxiliar JSON se detuvo antes del apply por supuestos incorrectos del lector: `@($null).Count=1` para child_modules ausente y expectativa de data AZ en planned_values (ya leído, ausente allí; presente en configuration). Lectura diagnóstica del **mismo archivo** confirmó cero módulos hijos y ninguna dirección inesperada. Se corrigió solo la comprobación en memoria: no hubo cambio HCL, nuevo plan, error Terraform o recurso prohibido; la revisión del archivo existente terminó **PASS** antes de aplicar. No se oculta este fallo local ni se trata como plan diferente.

- SHA-256 del plan revisado/aplicado: **0c323dfcd5c28ec219e69b1b6b5db5b3fd496abb4bf4426aa82fd76849b2d53f**.
- Fingerprint SHA-256 agregado de fuentes versionadas infra (rutas ordenadas + hash de contenido): **263fd6088ae9f7c3da63f8c47d72aed48819109608c93a981e748bbeaf0c45d2**.
- Ambos verificados iguales justo antes del apply y después; ninguna regeneración de plan, edición HCL/provider/lock/JSON o uso de -target/-replace/-refresh-only. Gate operativo pre-apply **APROBADO / SATISFECHO PARA ESTE COMANDO**, distinto de GATE-I-A/I-B, que no se cierran.

### 14.3. Apply exacto y post-plan

**Inicio apply UTC: 2026-11-12T19:06:47.1094767+00:00. Fin UTC: 2026-11-12T19:07:04.6559714+00:00.**

```text
terraform -chdir=infra apply -input=false -no-color enver-network.tfplan
Exit: 0
Apply complete! Resources: 14 added, 0 changed, 0 destroyed.

terraform -chdir=infra plan -input=false -no-color -detailed-exitcode
Exit: 0
No changes. Your infrastructure matches the configuration.
```

**Apply completo, no parcial; sin errores/AccessDenied/UnauthorizedOperation**, sin reintentos, ampliación IAM o cleanup. Post-plan único con exactamente los mismos inputs/perfil: **0 add / 0 change / 0 destroy**, refresca los 14 recursos sin denegaciones. Solo el plan inicial guardado fue aplicado; el post-plan no tiene -out ni se aplica.

| Dirección Terraform creada | ID observado (no secreto) |
| --- | --- |
| aws_vpc.enver | vpc-060f67e600affd569 |
| aws_subnet.public["a"] | subnet-0120d869dd659e2a1 |
| aws_subnet.public["b"] | subnet-01934348f5d6985aa |
| aws_internet_gateway.enver | igw-06e8461bf7317dc7d |
| aws_route_table.public | rtb-0a80f643c062696f9 |
| aws_route.public_internet | r-rtb-0a80f643c062696f91080289494 |
| aws_route_table_association.public["a"] | rtbassoc-09d224d0d751f3de7 |
| aws_route_table_association.public["b"] | rtbassoc-02e7f934b3ac07704 |
| aws_security_group.sg_alb | sg-0dacf0df5da30b4f7 |
| aws_security_group.sg_backend | sg-070d06cfebe866427 |
| aws_vpc_security_group_ingress_rule.alb_https | sgr-0049d608b24aa0375 |
| aws_vpc_security_group_ingress_rule.backend_from_alb | sgr-0ab23de9320e08c1c |
| aws_vpc_security_group_egress_rule.alb_to_backend | sgr-076c7db58a1da7c31 |
| aws_vpc_security_group_egress_rule.backend_initial | sgr-03b3c6fa937265c8e |

14 son **recursos gestionados Terraform**, no recuento de todo objeto automático AWS: al crear VPC AWS añade sus defaults implícitos (NACL/tabla/SG/default route local), no gestionados como recursos adicionales de este HCL. Ningún servicio/workload prohibido creado por este apply según plan guardado, salida apply y state. No se enumeran todas las cuentas/servicios ni se afirma inexistencia global de recursos ajenos.

### 14.4. Verificaciones AWS de lectura — 2026-11-12T19:09:09.1221599+00:00

Ocho comandos EC2 Describe con **--region us-east-1 --profile enver-infra**, todos **exit 0**, sin APIs de modificación:

| Lectura por IDs del apply | Evidencia real |
| --- | --- |
| describe-vpcs | VPC anterior available, CIDR **10.20.0.0/16**, tags Project=enver/Environment=dev/ManagedBy=Terraform/Name=enver-dev-vpc |
| describe-vpc-attribute (enableDnsSupport) | **true** |
| describe-vpc-attribute (enableDnsHostnames) | **true** |
| describe-subnets | Dos subnets available, **10.20.0.0/24 → use1-az1 / us-east-1a**, **10.20.1.0/24 → use1-az2 / us-east-1b**; ambas en VPC ENVER, **MapPublicIpOnLaunch=false**, tags/nombres correctos |
| describe-internet-gateways | IGW con **una** asociación available exclusivamente a VPC ENVER, tags correctos |
| describe-route-tables | Tabla ENVER, default **0.0.0.0/0 → IGW ENVER active**, ruta local /16; exactamente dos asociaciones explícitas associated, una por subnet, Main=false, tags correctos |
| describe-security-groups | Dos SG del apply en VPC ENVER, tags/nombres correctos |
| describe-security-group-rules | Exactamente cuatro reglas/IDs/tags esperados: ALB ingress **TCP443 desde 0.0.0.0/0**, egress solo **TCP8080 a SG backend**; backend ingress solo **TCP8080 desde SG ALB**, egress **todos protocolos IPv4 0.0.0.0/0 provisional** |

**Sin 22 público, sin 8080 público del backend ni ingress adicionales** en los SG gestionados; 443 público únicamente en SG ALB entre esos grupos. Son reglas verificadas, **no un ALB/servidor/HTTPS operativos o prueba activa de bypass**: todavía no existen hosts, target groups/listeners o SSM runtime en esta fase. Egreso amplio provisional no se convierte en diseño fino aceptado, TLS interno decidido o conectividad Atlas acreditada.

### 14.5. State, costos, tareas y gates

`infra/terraform.tfstate` creado/actualizado, última escritura **2026-11-12T19:07:04.5633341Z**, **serial 15**, **14 instancias managed / 0 tainted**. Lectura selectiva en memoria, **sin publicar state completo**. **terraform.tfstate.backup no existe** en la revisión posterior: no se presenta como respaldo creado. `.terraform/` sigue metadata/downloads locales. Plan/state/.backup y otros sufijos/tfvars reales/.terraform permanecen ignorados; **0 artefactos sensibles rastreados**, lock y ejemplo siguen versionables. Ignorar no acredita ACL/cifrado/custodia/backup/recuperación; state es ahora indispensable para recursos reales, conservarlo y no repetir el plan guardado ya aplicado.

Solo red base y SG materializados, **sin EC2/ALB/EIP/NAT/Redis/ElastiCache/target groups/ECR/ACM/Route53/DynamoDB/Secrets/CloudWatch/IAM/SSM** gestionados por este apply. **Ningún costo directo recurrente nuevo distinto del inventario previsto identificado**: se conserva estimación previa de red básica/SG sin workloads/IP **USD 0 recurrente directo**, no factura medida ni costo cero absoluto; tráfico y costo integral de plataforma/duración/crédito/reserva/ownership siguen pendientes, sin nueva consulta Billing/precios. No recursos facturables de workloads/IP/NAT habilitados por inferencia.

Reevaluación: **T-INFRA-003/004/005/006 continúan PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, ahora 003/006 con creación y verificación real cloud. 003 conserva dependencias/solapamientos externos; 004 sin EIP/allowlist Atlas/conectividad estable; 005 acredita las operaciones de provisión ejercitadas, no todos los permisos, mínimos globales o destroy/rollback, y falta IAM runtime/instance profile/SSM/servicios posteriores; 006 sin SSM/TLS interno/egreso fino/pruebas activas. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, suma evidencia plan/apply/post-plan, pero faltan custodia/backup/recovery, 001/002 y E025 integral. Los rótulos conservan su estado parcial; no reducen el avance cloud registrado a mera definición.

**GATE-I-A PENDIENTE / NO SATISFECHO:** 001/002, custodia/backup/recovery, costos/ownership y aceptación completa E025. **GATE-I-B PENDIENTE / NO SATISFECHO:** I-A, IAM runtime/SSM, restricciones Atlas, TLS interno/egreso y otras evidencias E019–020/E027–028 integrales. Nueve gates pendientes; no cierre automático, cumplimiento de dos instancias/producción o AUTH-027 integral. Este incremento no despliega Authentication, no implementa 004 ni ejecuta el alcance integral de 026–028; la operación puntual expresamente autorizada queda trazada en §14.

**DEFINIDO = PLANIFICADO = CREADO EN AWS** para estos 14 recursos, con verificación de propiedades enumeradas; no equivalencia con plataforma lista. Sin otras mutaciones, destroy/import, ampliación IAM, cambio HCL, pruebas backend o CI/CD. Solo task/memoria como cambios versionables; HEAD conservado, sin stage/commit/push. Detener para revisión humana, sin siguiente incremento o cleanup automático.

## 15. INFRA-4 — ECR / contrato de imagen: definición y plan, sin apply

### 15.1. Autorización, baseline y archivos — 2026-11-12

Solicitud expresa **BUILD CONTROLADO / PLAN PRIMERO / NO APPLY AUTOMÁTICO**, T-INFRA-007/008 y registro transversal 025. Lectura de spec/plan/task y Dockerfile/configuración, sin reinterpretar requisitos ni aceptar dependencias pendientes. Baseline verificada: **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, `docs(infra): record network baseline deployment`, árbol inicialmente **limpio / 0 staged**. Este checkpoint posterior registra el apply histórico §14; no fue otro apply.

Nuevos [registry.tf](../../infra/registry.tf) y [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json); modificados [outputs.tf](../../infra/outputs.tf), este task y [memoria §65](../../memoria.md). Sin nuevas variables/providers ni modificaciones networking/security, policies previas, lock, Dockerfile/producto/tests, spec/plan o 004. STS exit 0, perfil **enver-infra**, IAM user **enver-terraform-local**, región **us-east-1**, cuenta contrastada con owner del VPC en state, sin publicarla. Terraform **1.17.1/windows_amd64**, provider **6.73.0**, backend local inicializado; sin init/instalación/actualización.

Evidencia local: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-20261112\` (logs build/plan, resúmenes, hashes). El JSON completo de plan contiene state/identificadores internos: no imprimir/versionar/publicar. Carpeta temporal no garantiza conservación ni acredita custodia/backup/recovery del state. Prechecks auxiliares fallidos **Get-FileHash no disponible / AWS CLI no resuelto en PATH heredado**, ambos antes de AWS/Terraform; corregidos mediante SHA256 .NET y binarios existentes del usuario, sin instalar o cambiar PATH persistente.

### 15.2. Contenedor y build local, sin cambiar Dockerfile

- Dockerfile **existe**: runtime `mcr.microsoft.com/dotnet/aspnet:10.0`, SDK `mcr.microsoft.com/dotnet/sdk:10.0.403`; restore **--locked-mode**, build **--no-restore**, publish **--no-restore** con UseAppHost=false. global.json conserva **10.0.403 / rollForward:disable**.
- Stage **final FROM base**, hereda **USER $APP_UID**, WORKDIR `/app`, ENTRYPOINT **["dotnet", "Enver.API.dll"]**, EXPOSE **8080/8081**. launchSettings configura HTTP_PORTS=8080/HTTPS_PORTS=8081 solo para desarrollo Docker; EXPOSE no prueba listeners/TLS.
- Docker cliente/servidor **29.6.1**, Linux/amd64 disponible. Una ejecución `docker build --file Enver.API/Dockerfile --tag enver-api:infra-validation .`, **19:24:39.6050132Z–19:28:54.0723541Z**, **exit 0**; build informa **0 Warning(s), 0 Error(s)**. Tag local no existía antes. Sin push/login ECR, arranque de contenedor/aplicación o pruebas. Imagen local conservada, sin cleanup automático.
- `docker image inspect`: **Config.User=1654 / APP_UID=1654, non-root configurado en imagen final**; **DOTNET_VERSION=10.0.12 / ASPNET_VERSION=10.0.12**, **ASPNETCORE_HTTP_PORTS=8080**, linux/amd64, ENTRYPOINT indicado. **8080 acreditado como configuración/metadatos, no escucha probada ni conectividad cloud**.
- ID/RepoDigest **local** `sha256:32d2d2a952c4f1c4c9bb51c620a452b8acbee0ee591cc9f1828479f20fd10c33`: **no es digest acreditado de imagen publicada en ECR ni artefacto productivo elegido**. No variable de digest inventado.
- `.dockerignore` excluye state/backups/tfvars/planes/.terraform, configuración local/credenciales/material sensible usual. No secretos pasados como build args; esto no es auditoría de todas las capas, SBOM, scanning ni CI verde.
- **Gaps/límites:** bases por tags mutables, sin digest fijado; SDK fijo/locked restore no garantizan reproducción byte a byte. Pinning/revisión de vulnerabilidades/versiones soportadas necesitan evaluación y autorización separadas. .NET10.0.12 es resolución observada, no aprobación del parche cloud. No gap non-root observado en metadatos; endurecimiento runtime/secretos/readiness/forwarding/arquitectura hosts pendientes. Dockerfile intacto; build no extrapolado a UT/IT/ST o Authentication cloud.

### 15.3. ECR, scanning, cifrado y lifecycle propuestos

Un solo `aws_ecr_repository.api`, **enver-dev-api**, derivado de `${local.name_prefix}-api`; privado, tags comunes Project=enver/Environment=dev/ManagedBy=Terraform y Name=enver-dev-api. **IMMUTABLE**, sin excepciones latest; **force_delete=false** evita vaciar/borrar imágenes por el flujo Terraform normal. **AES256 administrado por ECR**, sin custom KMS/recursos o permisos KMS. Outputs no sensibles **ecr_repository_name**, **ecr_repository_url**, **ecr_repository_arn**; URL/ARN calculados, no existencia acreditada.

`image_scanning_configuration.scan_on_push=true` soportado por provider6.73.0 y presente en plan; no recurso de scanning global ni Inspector habilitado. La [documentación actual](https://docs.aws.amazon.com/AmazonECR/latest/userguide/image-scanning-basic-enabling.html) describe BASIC por defecto/filtros globales; [AWS](https://aws.amazon.com/blogs/containers/container-scanning-updates-in-amazon-ecr-private-registries-using-amazon-inspector/) explica la transición desde configuración por repositorio. **No afirmar scanning efectivo/BASIC real sin modo/filtros de cuenta y resultado de imagen**.

Lectura `aws ecr get-registry-scanning-configuration --profile enver-infra --region us-east-1 --output json --no-cli-pager`: **exit254 / AccessDeniedException**, acción **ecr:GetRegistryScanningConfiguration**, recurso **`*`**, fase **preflight read-only**; ninguna identity-based policy permite la acción. No IAM ampliado, simulación ni AdministratorAccess/PowerUserAccess. Pendiente **antes de apply/push**: permiso read si responsable lo aprueba, verificar modo/filtros/costo y resolver incompatibilidades en incremento propio, no cambiar toda la cuenta desde registry.tf.

Un `aws_ecr_lifecycle_policy.api`, regla **untagged / sinceImagePushed / days / 14 / expire**. Elección conservadora: expirar solo imágenes sin etiqueta con push de hace **más de14 días**, no versiones etiquetadas antiguas por conteo ciego que podría borrar rollback. Expiración ECR asíncrona, normalmente dentro de24h tras elegibilidad. **Edad desde push, no desde quitar el tag**: retirar último tag de un digest antiguo puede hacerlo elegible inmediatamente.

Contrato para revisión: conservar etiqueta inmutable en **cada digest activo y al menos el previo compatible para rollback**, junto con manifiestos/índices referenciados; no retirar sus tags/borrarlos mientras sean necesarios. **Todas las etiquetadas se conservan** por esta regla. Limpieza adicional requiere inventario de referencias, autorización y preview de lifecycle antes de cambiarlo. No hay máximo global de imágenes/GB ni tope de costo; se acotan residuos untagged antiguos, **no se garantiza número pequeño total**. Retención final/volumen DP-I-14 pendientes; registry aún no creado no prueba retención real de rollback.

### 15.4. IAM mínimo candidato, separado del publisher

Policy nueva **PROPUESTA LOCAL / NO APLICADA**: **4 Allow / 9 acciones exactas / 1631 caracteres compactos**, JSON/allowlist/scope verificados localmente. Ocho acciones limitadas a `arn:aws:ecr:us-east-1:*:repository/enver-dev-api`, **aws:ResourceAccount=${aws:PrincipalAccount}**, región/username exactos. Create/TagResource exigen cuatro request tags; lifecycle/delete cuatro resource tags. Lecturas sin tags para diagnóstico/refresh, conservando nombre/cuenta/región/usuario.

| Acciones | Necesidad y límites |
| --- | --- |
| ecr:CreateRepository, ecr:TagResource | Create con tags/settings IMMUTABLE/AES256/scanning en una llamada; TagResource es permiso dependiente de create con tags |
| ecr:DescribeRepositories, ecr:ListTagsForResource, ecr:GetLifecyclePolicy | Read/refresh de repository y lifecycle |
| ecr:PutLifecyclePolicy, ecr:DeleteLifecyclePolicy | Crear/retirar policy; provider reemplaza recurso policy al cambiar contenido, no repository |
| ecr:DeleteRepository | Rollback **solo con autorización futura**, no ejecutado/probado |
| ecr:GetRegistryScanningConfiguration | Preflight/costo denegado ahora; statement separado **Resource=* para acción sin ARN**, región/username, sin write global |

Derivación fijada al provider [repository.go](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ecr/repository.go), [lifecycle_policy.go](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ecr/lifecycle_policy.go), [tags_gen.go](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/ecr/tags_gen.go), y [IAM ECR](https://docs.aws.amazon.com/AmazonECR/latest/userguide/security_iam_service-with-iam.html). **Sin ecr:***, otras familias de servicios, token/uploads/PutImage/BatchDeleteImage, repository/registry policy, replication o IAM/OIDC. No PutImageScanningConfiguration/PutImageTagMutability/UntagResource: settings iniciales van en CreateRepository; updates no usados en este plan necesitan revisión/autorización adicional.

Allow/condiciones **no son boundary**, auditoría de grants remotos o límite absoluto de settings/lifecycle si hay otros permisos. `force_delete=false` protege el HCL, **no es condición IAM contra borrado forzado por otro cliente**; Sid no impone aprobación humana. Adjuntar concedería capacidad delete, pero no su uso sin permiso. Policies de red intactas; no destroy probado.

**Publisher futuro separado, conceptual/no concedido:** [AWS push IAM](https://docs.aws.amazon.com/AmazonECR/latest/userguide/image-push-iam.html): `ecr:GetAuthorizationToken` Resource=*; en ARN exacto `ecr:BatchCheckLayerAvailability`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`, `ecr:PutImage`, `ecr:BatchGetImage`. Lectura de evidencia futura DescribeImages/DescribeImageScanFindings requerirá autorización propia. Runtime pull separado: token, BatchCheckLayerAvailability/BatchGetImage/GetDownloadUrlForLayer, sin push/provisión. Futuro **GitHub→AWS OIDC**, credenciales temporales/roles separados, sin access keys persistentes de CI; no trust/OIDC/workflow creados. Ninguno de esos permisos publisher/runtime está en el JSON Terraform.

### 15.5. Contrato digest/CI/escritor único — pendiente de aceptación

Referencia desplegable futura **`<repository-uri>@sha256:<digest>`**, **no latest/tag como identidad**. Vincular SHA Git completo, build/run/attempt/arquitectura, digest registrado por ECR y **CI verde del mismo SHA/artefacto**. Tag inmutable único por publicación (SHA+run+attempt), no reutilizar con rebuild ni reconstruir al promover; distinguir índice OCI/manifiesto de plataforma y conservar dependencias. RepoDigest/ID local no sustituye ECR DescribeImages ni elige artefacto productivo.

Propuesta DP-I-14/004: **Terraform administra registry/lifecycle/outputs, no el digest seleccionado runtime; 004 sería su único escritor**. Sin variable de producción ficticia, dos escritores o ignore_changes para ocultar drift. Mecanismo EC2/ownership/contrato integral pendientes de aprobación; sin modificar spec/plan/compute/workflows.

CI futuro **automático**; CD producción **separado/exclusivamente humano**, GitHub Actions → Deploy → **Run workflow** (`workflow_dispatch`) tras CI verde exacto. Push/PR/merge/release/schedules/dispatch automático no despliegan producción; **terraform apply nunca automático**. API/CLI pueden emitir workflow_dispatch: actor/acceso/auditoría deben diseñarse en004, no asumir que el evento prueba intervención humana. Futuro deploy exige mismo digest A/B, dos instancias sanas, smoke tests y evidencia; rollback manual a imagen/config previas compatibles, sin restaurar state de seguridad antiguo/resetear cuotas/authority o retirar claves vigentes. Ninguna capacidad implementada por este contrato.

### 15.6. Validación/plan sin apply y state intacto

`terraform fmt -recursive infra`, `terraform fmt -check -recursive infra`, `terraform -chdir=infra validate`: **exit0**, **Success! The configuration is valid.** Sin init/actualización; fmt no alteró HCL previo salvo outputs añadidos autorizados.

Primer intento de plan **exit1 / Error: Too many command line arguments**, argumentos con `=`/PowerShell antes de plan/refresh; log conservado, no fallo IAM. Segundo intento con argumentos literales entre comillas, mismos inputs/HCL/permisos:

`terraform -chdir=infra plan -input=false -no-color -out=infra4-ecr.tfplan -detailed-exitcode`

**19:27:34.4802448Z–19:27:55.6869202Z**, **exit2 / Plan: 2 to add, 0 to change, 0 to destroy.** Plan guardado ignorado `infra/infra4-ecr.tfplan`, SHA256 **75c8ac132a58658681605ae6dde02386c362ff16c6d89fc12a675d54b0f8d4e1**. `terraform -chdir=infra show -json infra4-ecr.tfplan` exit0, revisión sin imprimir JSON completo. **No autoriza apply**; revalidar vigencia/plan refrescado ante cambios/permisos antes de pedir autorización. Plan de red consumido no reutilizado.

| Address | Acción prevista |
| --- | --- |
| aws_ecr_repository.api | create: enver-dev-api, IMMUTABLE, AES256, scan_on_push=true, force_delete=false, tags aprobados |
| aws_ecr_lifecycle_policy.api | create: única regla untagged >14 días |
| Los14 recursos de red/SG existentes | **no-op**, sin update/replace/delete |

Inputs verificados en JSON: enver/dev/us-east-1, VPC10.20.0.0/16, public_subnet_newbits=8, common_tags={}, network_design_approved=true y account guard STS; subnets existentes preservadas. Tres outputs ECR nuevos, no secretos. **Sin AccessDenied de plan**; repository nuevo fuera de state: éxito **no ejercita permisos de create/delete/scanning ni prueba ausencia de colisión con ECR no gestionado**.

JSON conserva **3 entradas resource_drift de refresh** en route table/ambos SG: vistas calculadas route/ingress/egress antes vacías ahora reflejan ruta/reglas de recursos independientes del apply§14. Contrastadas con standalone route/four rules, coinciden; los14 **resource_changes=no-op**, no mutación propuesta. No afirmar ausencia absoluta de drift. Auxiliar asumió inicialmente un address de ruta incorrecto y se detuvo; corregida solo inspección del mismo JSON por tipo/address reales, sin replan/HCL cambiado.

State persistido **serial15 / 14 instancias gestionadas**, **SHA256 idéntico antes/después**, .backup no acreditado. State/backups/planes/tfvars/.terraform ignorados/no versionados. Hashes de HCL previo (salvo outputs autorizado), policies previas, lock, Dockerfile/.dockerignore/global.json iguales. Sin apply/destroy/import, refresh-only apply, mutaciones AWS/IAM o restauración de state.

### 15.7. Costo consultado, sin asumir Free Tier

Consulta **2026-11-12**, [AWS Price List AmazonECR us-east-1](https://pricing.us-east-1.amazonaws.com/offers/v1.0/aws/AmazonECR/current/us-east-1/index.json), publicationDate **2026-11-11T15:36:26Z**, effectiveDate **2026-11-01T00:00:00Z**, SKU **KJC988R4J65BDMTQ**, TimedStorage-ByteHrs/S3: **USD0.10/GB-mes**, almacenamiento privado estándar. No archivo/replicación/pull-through cache configurados.

[ECR pricing oficial](https://aws.amazon.com/ecr/pricing/): entrada y transferencia a servicios AWS **en la misma región USD0/GB**; Internet/inter-región puede facturarse, con tarifas agregadas por volumen/ruta y cargos del otro extremo. **Tarifa neta exacta de salida/volumen/costo integral pendientes**; no asumir Free Tier/franquicias/créditos. Sin NAT/endpoints nuevos; si se añaden después, sus cargos se suman.

[AWS scanning](https://aws.amazon.com/blogs/containers/container-scanning-updates-in-amazon-ecr-private-registries-using-amazon-inspector/) describe BASIC **sin cargo adicional**, ENHANCED/Inspector **facturable**; fuente no acredita modo/factura actual de esta cuenta: lectura denegada, costo efectivo pendiente. No usar tarifa histórica Inspector como vigente. AES256 evita custom KMS, no implica ECR total gratis.

Estimación futura **0.10×GB-mes almacenados + transferencia facturable + scanning si aplica**; no inferir consumo/compresión/deduplicación o factura desde tamaño Docker local. Lifecycle no es cap; etiquetadas pueden crecer. Presupuesto agregado baseline USD50/reserva≥USD50 de crédito reportado USD100 intacto, no saldo validado/presupuesto mensual/tope duro; T001 costo/duración/ownership abiertos. Sin apply/push no recursos ECR ni bytes subidos por este bloque; red previa permanece.

### 15.8. Estados/gates y detención

- **T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: HCL/outputs/plan, no registry creado; IAM solo propuesto, scanning efectivo/retención/costo/contrato/dependencias y aprobación pendientes.
- **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: build/non-root/configuración y contrato propuesto; bases reproducibles/seguridad, ECR/digest/CI/ownership aprobado/rollback pendientes. Ausencia de push no elimina criterio contractual propio ni acredita su aprobación.
- **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: suma extensión fmt/check/validate/plan; custodia/cifrado/backup/recovery, E025 integral y001/002 sin aceptar.
- **T-INFRA-003/004/005/006 conservan PARCIAL**; no aceptar EIP/Atlas/egreso/SSM/runtime IAM/TLS por plan ECR. **GATE-I-A / GATE-I-B PENDIENTE / NO SATISFECHO**.
- Gate relacionado registry/container **GATE-I-C PENDIENTE / NO SATISFECHO**: faltan009/010/027/028, dos EC2/AZ/mismo digest/capacidad/recuperación y prerrequisitos A/B. **GATE-I-H / GATE-I-I** pendientes por state/reproducción/rollback/004/aceptación cloud. **Nueve gates pendientes**.
- Nueve tareas parciales001–008/025; otras **21 PENDIENTE / NO EJECUTADA**. Sin ejecutar compute/026–028, alterar Authentication o cerrar002 GATE-G/H/AUTH-027 integral.

**DEFINIDO/PLANIFICADO:** un repository/una lifecycle policy, tres outputs y candidato IAM separado, build local válido. **CREADO EN AWS EN ESTE BLOQUE: NINGUNO. APPLY/PUSH: NO EJECUTADOS.** Cinco archivos versionables autorizados, sin stage/commit/push. Detener para revisión humana HCL/IAM/plan/costo y nueva autorización delimitada, sin cleanup/permisos/siguiente incremento automático.

## 16. INFRA-4 ECR — revisión pre-apply IAM y cuotas, sin AWS ni checkpoint Git

### 16.1. Alcance y revisión local — 2026-11-12

Autorización **PRE-APPLY IAM + CHECKPOINT / VALIDACIÓN CONTROLADA / NO APPLY**: inspeccionar policy existente, acciones/scope/tamaño, preparar evidencia para aplicación manual posterior, ejecutar solo fmt-check/validate y actualizar task/memoria si hay evidencia nueva. El título no autoriza stage/commit/push: se prohíben expresamente. Baseline confirmada **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, cinco archivos pendientes de §15 y **0 staged**. Sin cambios de HCL/JSON/lock, plan repetido, AWS CLI/STS/IAM/Access Analyzer/simulación, Docker/producto o instalación/init/actualización.

JSON [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json) **válido por parseo y revisión local**, 4 Allow y nueve acciones exactas, sin duplicados ni comodines de acción:

`ecr:CreateRepository`, `ecr:TagResource`, `ecr:DescribeRepositories`, `ecr:ListTagsForResource`, `ecr:GetLifecyclePolicy`, `ecr:PutLifecyclePolicy`, `ecr:DeleteLifecyclePolicy`, `ecr:DeleteRepository`, `ecr:GetRegistryScanningConfiguration`.

Ausencia confirmada de **ecr:***, AdministratorAccess/PowerUserAccess y cualquier acción de **EC2, ELB, IAM, S3, KMS, DynamoDB, ElastiCache, Secrets Manager, ACM, Route53, CloudWatch o SSM**; también sin permisos publisher. Settings iniciales se transmiten con CreateRepository, no requieren ampliar updates de scanning/mutabilidad para este plan; lecturas/tags/lifecycle/delete corresponden al flujo provider6.73.0 revisado en §15.4. Delete apoya únicamente rollback posterior autorizado, no ejecución actual ni mínimo efectivo global auditado.

Contrastado con [referencia oficial ECR vigente](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ecr.html), consultada 2026-11-12: **ocho acciones admiten resource repository**, y permanecen restringidas a **enver-dev-api/us-east-1**, ARN `arn:aws:ecr:us-east-1:*:repository/enver-dev-api`. El wildcard de cuenta se acota con **aws:ResourceAccount=${aws:PrincipalAccount}**, región/username; create/tag usa request tags, lifecycle/delete resource tags, sin exigir tags aún inexistentes al crear.

**Único Resource="*": ecr:GetRegistryScanningConfiguration**, operación de registry sin resource type ARN en la referencia AWS. No se puede sustituir por ARN repository ni exigir tags de ese repository sin romper esa lectura. Statement separado, región us-east-1/username enver-terraform-local, solo read; no PutRegistryScanningConfiguration, Inspector o administración global concedidos. Permanece por el **AccessDenied observado en §15.3** para inspección de modo/filtros/costo; no porque Terraform repository lo invoque al planificar un create. No se repitió la llamada: acceso efectivo/scanning siguen pendientes.

### 16.2. Tamaño y propuesta de aplicación manual, no ejecutada

Archivo existente **2269 bytes / 2269 caracteres** (ASCII); documento compacto **1631 caracteres sin whitespace**, confirmado tanto por serialización JSON como por conteo sin espacios. SHA256 exacto del archivo **393b731f012d6be6bc285b068793f52dda91099661717f2f3b991c437955cbe9**. No se reformatea ni modifica policy.

[Cuotas oficiales IAM](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_iam-quotas.html), consultadas 2026-11-12: inline de **usuario: 2048 caracteres agregados entre todas sus inline policies**, no por archivo; customer-managed: **6144 caracteres por policy**, ambos sin contar whitespace. **Cabe aisladamente inline**, pero deja solo417 caracteres para las restantes inline del usuario; su suma remota/cuotas de adjuntos no se consultaron. No afirmar que puede añadirse inline al usuario actual por medir solo este JSON.

**Recomendación pendiente de aprobación: customer-managed policy independiente ENVER-Terraform-ECR**. No exigida por tamaño individual; recomendada por separación clara respecto de networking y para no consumir el agregado inline compartido. Preparación para responsable autorizado: revisar el JSON exacto/hash/nueve acciones/cuatro statements; si aprueba posteriormente, crear la customer-managed con ese nombre y adjuntarla **solo al usuario previsto enver-terraform-local**, comprobando cuenta y cuota disponible. No integrar con networking ni conceder al usuario Terraform permisos IAM para crear/adjuntar policies. Estos son pasos propuestos, **ninguno ejecutado ni acreditado**; no se crea ARN/version remota ficticio.

Al adjuntarse, la policy sí otorgaría capacidad delete; Sid/aprobación humana no son guard IAM y force_delete=false solo protege el flujo HCL. No permissions boundary ni auditoría de otras policies/boundaries/SCP. La validación local **no es IAM Access Analyzer ni prueba efectiva de create/delete/rollback**. Después de cualquier aplicación manual reportada harán falta autorización de las lecturas pertinentes, verificación de identidad/modo/filtros/costo y revisión de vigencia del plan antes de solicitar apply; nada de ello se ejecuta aquí.

### 16.3. Validación local, baseline conservada y estados

Terraform **1.17.1/windows_amd64** / provider **6.73.0** comprobados con `terraform -chdir=infra version -json`. Un auxiliar se detuvo inicialmente al buscar selección provider desde la raíz sin configuración Terraform (`provider_selections={}`); comprobación corregida a infra, sin cambiar versión/archivos o ejecutar plan. **19:43:50.0174285Z–19:44:01.7004193Z**:

- `terraform fmt -check -recursive infra`: **exit0**, sin modificaciones.
- `terraform -chdir=infra validate -no-color`: **exit0 / Success! The configuration is valid.**
- **Plan no repetido** porque HCL no cambió. Se conserva como **evidencia previa**, no refresh actual, el plan §15: **2 add / 0 change / 0 destroy**, únicamente aws_ecr_repository.api/aws_ecr_lifecycle_policy.api, networking14 no-op; SHA256 **75c8ac132a58658681605ae6dde02386c362ff16c6d89fc12a675d54b0f8d4e1** comprobado idéntico. No se aplica ni se presume vigencia indefinida.

Evidencia local nueva: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-preapply-review-20261112-194320\`, resúmenes de parseo/scope/tamaño/hash y validación, hashes de fuentes y state/plan. No contiene una aprobación IAM ni reemplaza custodia/backup/recovery. Hashes previos preservan HCL/policy/lock/state/plan y trabajo existente; state/planes/.terraform/tfvars siguen fuera de Git. Únicas ediciones **de este turno**: task/memoria, sobre los cinco cambios pendientes previos.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**. Sin aceptación de dependencias, registry/imagen ECR, scanning efectivo, IAM aplicado, state custody/backup/recovery o contrato004 integral. **T-INFRA-003–006 siguen PARCIAL; los nueve gates PENDIENTE / NO SATISFECHO**, incluidos I-A/I-B/I-C/I-H/I-I. Authentication y estados002 sin cambios.

**AWS MODIFICADO: NO. APPLY/DESTROY/IMPORT/REPLAN: NO. POLICY REMOTA CREADA/ADJUNTADA: NO. STAGE/COMMIT/PUSH: NO.** Detener para revisión humana; no ejecución manual implícita, siguiente incremento o checkpoint Git.

## 17. INFRA-4 ECR — IAM informado aplicado, lecturas y replan pre-apply

### 17.1. Autorización, fuentes y baseline — 2026-11-12

Solicitud expresa **IAM VALIDATION + REPLAN PRE-APPLY / VALIDACIÓN CONTROLADA / NO APPLY**: STS, GetRegistryScanningConfiguration, DescribeRepositories, revisión local de policy, fmt-check/validate y un replan con inputs aprobados; solo task/memoria editados. **Terraform apply: NO AUTORIZADO TODAVÍA**. Sin mutaciones IAM/AWS, repositorio manual, apply/destroy/import, Docker/producto o stage/commit/push. Baseline confirmada **feature/infrastructure**, HEAD **0abab1a445d7ea76b922971442cd535ad0418233**, cinco pendientes previos de INFRA-4 y **0 staged**.

**INFORMADO POR EL RESPONSABLE:** creó y adjuntó manualmente **ENVER-Terraform-ECR**, **customer-managed**, fuente [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json), sin AdministratorAccess/PowerUserAccess/ecr:*. Se registra **APLICADA MANUALMENTE SEGÚN RESPONSABLE**, no una mutación del agente. No se enumeran policy remota/versiones/adjuntos/boundaries/SCP; las llamadas verifican únicamente sus respuestas efectivas, no identidad exacta de la policy causante ni ausencia global de otros grants. §§15/16 conservan sus estados históricos de propuesta/no aplicada en aquellos turnos.

JSON local intacto, hash **393b731f012d6be6bc285b068793f52dda91099661717f2f3b991c437955cbe9**, 4 Allow/9 acciones aprobadas/1631 caracteres compactos: CreateRepository, TagResource, DescribeRepositories, ListTagsForResource, GetLifecyclePolicy, PutLifecyclePolicy, DeleteLifecyclePolicy, DeleteRepository y GetRegistryScanningConfiguration, todas **ecr:**. Sin wildcard de acción ni familias prohibidas/publisher. Scope por repository enver-dev-api/us-east-1/misma cuenta/username/tags y único Resource=* para lectura scanning conservados; no ampliación local/remota.

### 17.2. Lecturas efectivas y AccessDenied delimitado

Perfil **enver-infra**, región **us-east-1**, sin pager; salidas capturadas sin credenciales/ID de cuenta completo en documentación. Secuencia terminada **2026-11-12T19:50:50.6800817Z**:

| Llamada AWS CLI | Resultado real / alcance |
| --- | --- |
| sts get-caller-identity --profile enver-infra --region us-east-1 | **exit 0**, IAM user **enver-terraform-local**, cuenta coincide con owner VPC del state aprobado |
| ecr get-registry-scanning-configuration --region us-east-1 --profile enver-infra | **exit 0**, **scanType=BASIC / rules=[]**; lectura antes denegada ahora disponible |
| ecr describe-repositories --region us-east-1 --profile enver-infra | **exit 254 / AccessDeniedException**; listado general, no lista vacía exit 0 |
| Misma lectura con --repository-names enver-dev-api | Diagnóstico acotado, **exit 254 / RepositoryNotFoundException**, sin AccessDenied: repository indicado no existe; no lectura exit 0 de un repository existente |

**AccessDenied observado:** acción **ecr:DescribeRepositories**, recurso **arn:aws:ecr:us-east-1:<account>:repository/*** (listado general), fase **AWS CLI/read ECR previa al plan**; ninguna identity-based policy permite ese alcance según respuesta. **Operación Terraform asociada: ninguna**, no es un fallo del replan/provider. La policy permite un nombre exacto, no el inventario de toda la cuenta; NotFound de la consulta acotada no se oculta ni convierte en exit 0. Sin IAM modificado/reintento con privilegios superiores.

**Permiso mínimo si se exigiera ese listado general:** Allow únicamente **ecr:DescribeRepositories** sobre **arn:aws:ecr:us-east-1:<cuenta-aprobada>:repository/***, con región/username/misma cuenta; ampliaría recursos visibles, no acciones ni escritura. **Solo propuesta explicativa, NO aplicada ni necesaria para este plan**. Preferir consulta por nombre, alineada con DescribeRepositories(RepositoryNames=[nombre]) del provider revisado; no ecr:*, AdministratorAccess/PowerUserAccess o falsa condición por tags para una lectura sin repository concreto. No ampliar IAM solo para obtener inventario general innecesario.

**Efectividad acreditada: parcial**, GetRegistryScanningConfiguration exit 0 y respuesta NotFound por nombre sin AccessDenied; **no create/tag/lifecycle/delete efectivos probados**. Plan de repository nuevo tampoco los ejercita. BASIC/rules=[] acredita modo global en ese instante; `scan_on_push=true` sigue siendo solicitud HCL/plan, no prueba de scanning efectivo o findings de imagen inexistente. No se modifican filtros globales/Inspector ni se interpreta ausencia de reglas como imagen ya escaneada.

### 17.3. Terraform y plan exacto conservando networking

Terraform **1.17.1/windows_amd64**, provider **6.73.0**, comprobados desde infra, lock sin cambios. `terraform fmt -check -recursive infra` **exit 0**, `terraform -chdir=infra validate -no-color` **exit 0 / Success! The configuration is valid.** Sin init/instalaciones/actualizaciones.

Una ejecución de `terraform -chdir=infra plan -input=false -no-color -out=infra4-ecr-replan-20261112-195037.tfplan -detailed-exitcode`, **19:51:57.0468069Z–19:52:18.1128719Z**, **exit 2 / Plan: 2 to add, 0 to change, 0 to destroy.** **Sin AccessDenied en Terraform**. Show-json **exit 0** revisado selectivamente, sin publicar JSON/state completo.

| Address / inventario | Acción |
| --- | --- |
| aws_ecr_repository.api | **create**, nombre **enver-dev-api**, **IMMUTABLE**, **scan_on_push=true**, **AES256**, **force_delete=false**, tags aprobados |
| aws_ecr_lifecycle_policy.api | **create**, única regla **untagged/sinceImagePushed/days/14/expire** |
| Los14 recursos de networking/SG del state | **no-op**, 0 update/replace/delete |

Inputs contrastados en JSON: project_name=enver, environment=dev, aws_region=us-east-1, vpc_cidr=10.20.0.0/16, public_subnet_newbits=8, common_tags={}, network_design_approved=true, account guard igual a cuenta STS/state; subnets existentes preservadas. Tres outputs ECR no sensibles. No recursos fuera de los dos ECR, destrucciones ni replacements. Lifecycle conserva todas las imágenes etiquetadas, incluidas activas/rollback bajo contrato§15; 14 días desde push, no desde retirar tag, sin tope global de almacenamiento.

El nuevo JSON conserva las **tres entradas calculadas de refresh** de route table/SG (route/ingress/egress antes vacíos). Se contrastaron con standalone route/cuatro reglas y coinciden; **14 resource_changes=no-op**. No confundirlas con cambios de red propuestos ni afirmar cero drift absoluto.

Plan nuevo ignorado **infra/infra4-ecr-replan-20261112-195037.tfplan**, SHA256 **669e2550dced81aa3bd1bf3c07aff210305057e61c17d2cb503abe55c72a1640**; plan previo§15/hash intacto, no sobrescrito/reaplicado. State persistido **serial15/14 instancias gestionadas/hash idéntico**, sin backup/recovery nuevo acreditado; HCL/policy/lock y demás fuentes no modificados por lecturas/validación/plan. Plan no autoriza apply y requiere revisión de vigencia antes de una autorización posterior.

### 17.4. Costo, estados y detención

Se conserva evidencia regional previa§15.7 de almacenamiento ECR **USD0.10/GB-mes**, **sin nueva consulta tarifaria/Billing**. Sin Free Tier asumido ni total calculado a partir del tamaño Docker local. La lectura confirma modo BASIC actual, no ejecución/resultados de scan ni factura/ausencia de otros cargos Inspector globales; costo integral/transferencia/volumen/duración y presupuesto/reserva siguen su aceptación pendiente. Ningún repository/imagen/byte ECR creado/subido por este bloque.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: aplicación IAM informada y lecturas/replan acreditados, no repository real, escritura/scan de imagen/retención/contrato/dependencias aceptados. **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: sin nueva imagen/digest ECR/CI/ownership aprobado ni rollback. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: nueva validación/plan no suple custodia/cifrado/backup/recovery/001/002/E025 integral.

**GATE-I-A PENDIENTE / NO SATISFECHO** según §3/E001/E002/E025: faltan aceptación de baseline/costo/state/ownership y protección/recuperación. **GATE-I-B PENDIENTE / NO SATISFECHO**: I-A, IAM runtime/SSM, Atlas/egreso/TLS, E019–020/E027–028 integrales sin resolver. 003–006 siguen parciales; I-C/H/I y **los nueve gates pendientes**. Authentication/002/004 intactos; no ejecutar integralmente026–028 ni cerrar AUTH-027/002 GATE-G/H.

Evidencia local privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-iam-replan-20261112-195037\`: lecturas saneadas, logs/JSON de plan y resúmenes/hashes; no publicar JSON completo ni confundir ubicación temporal con custodia/backup. Solo task/memoria editados en este turno; conjunto pendiente conserva cinco archivos/0staged, artefactos Terraform sensibles ignorados/no versionados.

**RECURSOS AWS CREADOS POR EL AGENTE: NINGUNO. IAM REMOTO: APLICACIÓN MANUAL INFORMADA POR RESPONSABLE, NO MODIFICADO POR EL AGENTE. TERRAFORM APPLY: NO AUTORIZADO TODAVÍA / NO EJECUTADO. SIN DESTROY/IMPORT/STAGE/COMMIT/PUSH.** Detener para revisión humana, sin siguiente incremento automático.

## 18. INFRA-4 ECR — apply exacto completado, post-plan sin cambios, inventario de imágenes pendiente

### 18.1. Autorización, checkpoint y prechecks — 2026-11-12

Solicitud **APPLY CONTROLADO / BUILD CLOUD CONTROLADO**, autorización explícita únicamente de **aws_ecr_repository.api** y **aws_ecr_lifecycle_policy.api**, condicionada a plan fresco guardado/revisado 2/0/0 y networking14 no-op. Sin apply implícito, -target/-replace/-refresh-only, cambios HCL, IAM mutado, destroy/import, workloads, docker push o CI/CD/OIDC. Solo task/memoria como cambios versionables; sin stage/commit/push.

Checkpoint previo verificado **2626fb10eb9b7c128c6422e92598ca9f0e78e72c**, `feat(infra): prepare ecr registry baseline`, rama **feature/infrastructure**, árbol **limpio / 0 staged** al inicio, tras inspeccionar plan y justo antes de apply. Ese commit contiene exactamente registry.tf/policy ECR/outputs/task/memoria, no artefactos Terraform sensibles.

STS **exit 0**, perfil **enver-infra**, IAM user **enver-terraform-local**, cuenta coincide con owner VPC del state; región **us-east-1**. Terraform **1.17.1/windows_amd64**, provider **6.73.0**, backend **local** inicializado/path terraform.tfstate; sin init/updates. State inicial **serial15 / 14managed**, sin .backup entonces. `terraform fmt -check -recursive infra` **exit 0**, `terraform -chdir=infra validate -no-color` **exit 0 / Success! The configuration is valid.**

ENVER-Terraform-ECR customer-managed adjunta manualmente según responsable (§17); no se modifican/enumera IAM remoto o otros grants. Hashes de todas las fuentes versionadas tomados antes y contrastados hasta terminar verificación: HCL/lock/policies/Dockerfile/producto/tests intactos. No STS/credenciales completos ni state/JSON del plan publicados.

### 18.2. Plan fresco guardado e inspección

Mismos inputs verificados en JSON: **enver/dev/us-east-1**, VPC **10.20.0.0/16**, newbits **8** (subnets existentes .0/.1 /24), common_tags={}, network_design_approved=true, account guard igual a STS/state. Perfil externo enver-infra; sin secretos en argumentos/outputs. Una ejecución:

`terraform -chdir=infra plan -input=false -no-color -out=enver-ecr.tfplan -detailed-exitcode`

**20:04:51.9375136Z–20:05:11.6015211Z**, **exit 2 / Plan: 2 to add, 0 to change, 0 to destroy.** Plan local **infra/enver-ecr.tfplan**, ignorado/no versionado. **SHA256 f91058f8f89ecea0b00e7a5ae27d27602b75f21ef3a6311a479694abdbe5c3ea**. `terraform -chdir=infra show -no-color enver-ecr.tfplan` y `show -json` **exit 0**, salida humana/JSON revisadas, sin volcar state completo a documentación.

| Recurso / configuración | Plan revisado |
| --- | --- |
| aws_ecr_repository.api | **create**; **enver-dev-api**, **IMMUTABLE** sin excepciones, **scan_on_push=true**, **AES256**, **force_delete=false** |
| aws_ecr_lifecycle_policy.api | **create**; una regla **untagged / sinceImagePushed / days / 14 / expire**, ninguna tagged |
| Los14 recursos de networking/SG | **no-op**, sin update/replace/delete |

Tags Project=enver/Environment=dev/ManagedBy=Terraform/Name=enver-dev-api y tres outputs ECR aprobados. Únicamente dos altas gestionadas ECR, sin nuevos recursos prohibidos/VPC/subnets/SG/compute/IAM/SSM/servicios adicionales. **Gate operativo pre-apply PASA**: HEAD/árbol/fmt/validate/plan exacto/inputs/configuración/hash/HCL/state intactos, sin AccessDenied bloqueante. **No equivale a cerrar GATE-I-A/B ni aceptación integral**. No replan ni HCL modificado entre revisión y apply; planes pre-apply anteriores conservados, no usados.

### 18.3. Apply del archivo exacto y post-plan

Una ejecución **`terraform -chdir=infra apply -input=false -no-color enver-ecr.tfplan`**, **20:06:15.2025475Z–20:06:33.7890082Z**, **exit 0**:

**Apply complete! Resources: 2 added, 0 changed, 0 destroyed.**

Ambos recursos completaron con ID **enver-dev-api**. **Apply COMPLETO, no parcial**, sin AccessDenied/error/reintento, sin destroy/rollback/IAM adicional. Hash del plan antes/después idéntico **f91058f8…be5c3ea**; HEAD/fuentes intactos. El archivo ya aplicado se conserva como evidencia privada, **no volver a aplicarlo**.

Post-plan con mismos inputs/perfil, **20:07:16.2036319Z–20:07:34.3744522Z**, `terraform -chdir=infra plan -input=false -no-color -out=enver-ecr-post.tfplan -detailed-exitcode`: **exit 0 / No changes. Your infrastructure matches the configuration.** **0 add / 0 change / 0 destroy**. Show-json exit 0 verifica **networking14 no-op + ECR2 no-op**, total16managed. Archivo post-plan ignorado, inspeccionado/no aplicado. No apply correctivo ni mutaciones de red.

### 18.4. Verificación AWS real y limitación de inventario

Tres lecturas AWS CLI con perfil/región aprobados, sin cambios IAM, terminadas **20:08:30.6302407Z**, todas **exit 0**:

| Operación | Evidencia observada |
| --- | --- |
| describe-repositories --repository-names enver-dev-api | Repository existente; **imageTagMutability=IMMUTABLE**, **imageScanningConfiguration.scanOnPush=true**, **encryptionConfiguration.encryptionType=AES256**; URI/ARN coinciden con state |
| get-lifecycle-policy --repository-name enver-dev-api | Única regla **tagStatus=untagged**, **countType=sinceImagePushed**, **countUnit=days**, **countNumber=14**, **action.type=expire**; sin tagged |
| get-registry-scanning-configuration | **scanType=BASIC / rules=[]**; estado del registry separado del flag scanOnPush del repository |

**URI real:** `926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api`.

**ARN real:** `arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api`.

Identificadores no secretos solicitados para futura publicación/digest, no credenciales ni prueba de imagen publicada. Flags y lifecycle verificados son configuración real; **no prueba de scanning de una imagen/findings, retención efectiva de un digest de rollback ni lectura del inventario**. La regla no expira tagged; edad de14d desde push, no desde quitar tag. Contrato de conservar etiquetas/manifiestos activos/rollback§15 y revisión previa a cualquier limpieza permanece.

Para acreditar cantidad de imágenes/digests se intentó una lectura **`aws ecr list-images --repository-name enver-dev-api --region us-east-1 --profile enver-infra --output json --no-cli-pager`**, **20:08:52.9293504Z–20:08:56.3479929Z**: **exit 254 / AccessDeniedException**. Acción **ecr:ListImages**, recurso **arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api**, fase **verificación read-only final de inventario**; **operación/recurso Terraform fallidos: ninguno**. La policy de provisión no incluye ListImages; respuesta indica falta de identity-based Allow. No confundir con el AccessDenied histórico de listado general DescribeRepositories§17.

**Detenidas las llamadas AWS/ejecuciones Terraform tras esa denegación**, sin reintento, ampliación de IAM o rollback; solo registro documental e inspección local posterior. Apply y post-plan exitosos se conservan como tales. Una verificación futura de inventario necesitaría permiso explícito **solo ecr:ListImages en ese ARN**, aprobado/aplicado aparte por responsable, no añadirlo automáticamente al JSON. **0 imágenes publicadas por el agente; conteo total/ausencia global de digest NO ACREDITADOS**, no convertir AccessDenied en lista vacía. No digest ECR inventado ni docker push/tag/login realizado; imagen local enver-api:infra-validation no publicada por este bloque.

### 18.5. State, exclusiones y costo

State local actualizado **infra/terraform.tfstate**, **serial18 / 16managed / 0tainted**, última escritura **2026-11-12T20:06:33.7251925Z**. SHA256 **b4497de9fb6fc28e819970bad70cca2bddbf093743745f9522ef4332d4c94e02**. Solo metadata/recursos pertinentes inspeccionados, nunca state completo publicado. **terraform.tfstate.backup ahora existe**, **serial15 / 14managed**, última escritura **20:06:33.3403478Z**: snapshot local previo, **no copia cifrada/controlada ni recuperación probada**. No restaurarlo para gestionar recursos ECR actuales ni presentarlo como aceptación de custody/recovery.

State/.backup/sufijos/.terraform/tfvars reales/todos los tfplan ignorados y no versionados, lock/ejemplo conservan versionabilidad. State es indispensable para16 recursos reales; custodia/ACL/cifrado/backup protegido/recovery y backend remoto antes de GitHub/colaboración siguen pendientes, no resueltos por ignorar archivos. HCL/policies/lock/Dockerfile/Authentication/tests/004 intactos; solo task/memoria modificados después de verificación.

Costo regional conservado de§15.7: **almacenamiento ECR USD0.10/GB-mes**, sin nueva consulta precios/Billing, Free Tier asumido o total mensual inventado. Repository recién creado, **sin imágenes publicadas por el agente**, se espera almacenamiento prácticamente vacío hasta publicar, **no tamaño/factura medidos ni cero imágenes global acreditado** por ListImages denegado. No custom KMS/replicación/Inspector/compute/red facturable adicional creado por este bloque. BASIC y scanOnPush no prueban factura global o cero cargos; volumen/transferencia/consumo/duración/aceptación de costo siguen pendientes. Presupuesto baseline/reserva§8 no es tope duro ni autorización de siguientes servicios.

### 18.6. Tareas, gates y cierre del incremento

- **T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: repository/lifecycle reales y propiedades AWS acreditados, avance cloud explícito; faltan aceptación de dependencias002/005/025, contrato/ownership/retención DP-I-14 y revisión integral. No marcar COMPLETADA por dos recursos creados ni inventar imagen/digest/scan/rollback.
- **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: falta imagen ECR/digest/validación de seguridad/CI/rollback y contrato aprobado; conteo/digests tampoco acreditados por permisos. Sin repetir build o alterar producto.
- **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: fmt/validate/plan/apply/post-plan reales; backup local no satisface custodia/cifrado/recovery ni E001/E002/E025 integral. 002 conserva estado parcial con metadata state actualizada, sin aceptar sus criterios.
- **GATE-I-A PENDIENTE / NO SATISFECHO**: baseline/costo/ownership/state/backup protegido/recovery y aceptación integral E001/E002/E025 pendientes. **GATE-I-B PENDIENTE / NO SATISFECHO**: I-A, IAM runtime/SSM/Atlas/egreso/TLS/E019–020/E027–028. **Los nueve gates pendientes**, 003–006 parciales e I-C/H/I sin aceptar registry como dos hosts/digest/producción/rollback.
- Nueve tareas parciales001–008/025, otras21 PENDIENTE/NO EJECUTADA. Apply acotado autorizado no ejecución integral026–028 ni deployment de Authentication/004; 002 GATE-G/H/AUTH-027 integral conservados.

Evidencia local privada en `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-apply-20261112-200342\`: prechecks/hashes, plan humano/JSON privados, gate/apply/post-plan, lecturas y denegación saneadas para documentación, metadata state. No retención/custodia acreditadas por carpeta temporal; no publicar JSON/state/planes.

**ECR DEFINIDO = PLANIFICADO = CREADO Y CONFIGURACIÓN VERIFICADA** para repository/lifecycle, no para imágenes/digests/seguridad integral. **APPLY COMPLETO / POST-PLAN0/0/0; LISTIMAGES DENEGADO / DETENCIÓN SIN IAM NUEVO.** Solo task/memoria como cambios versionables; HEAD2626fb1 conservado, **0staged / sin commit/push**. No más apply, cloud read, IAM, cleanup/destroy o siguiente incremento automático; detener para revisión humana.

## 19. INFRA-4 ECR — ListImages validado e inventario vacío; checkpoint post-apply

**2026-11-12**, autorización **VALIDACIÓN + GIT CHECKPOINT / NO APPLY / NO DOCKER PUSH**. Baseline comprobada **feature/infrastructure**, HEAD **2626fb10eb9b7c128c6422e92598ca9f0e78e72c**, con los dos documentos post-apply§18 pendientes y **0 staged / 0 untracked**; se conserva su evidencia y se incluye en el checkpoint solicitado. Solo se permite añadir al JSON local la acción ListImages, actualizar task/memoria y crear un commit de esos tres archivos; sin push Git, cloud writes, nuevos recursos o ejecución de otro incremento.

**INFORMADO POR RESPONSABLE:** ENVER-Terraform-ECR fue actualizada manualmente para incluir **ecr:ListImages**, sin otros permisos agregados. No se enumera/audita contenido, versión, adjuntos o grants remotos ni el agente modifica IAM. El éxito de las lecturas acredita solo las operaciones ejercitadas, no auditoría de privilegios globales o permisos de publicación/runtime.

Lecturas **20:22:34.9357668Z–20:22:47.1485716Z**, perfil **enver-infra**, región **us-east-1**, todas **exit 0**, sin AccessDenied/reintentos:

| Operación | Resultado real |
| --- | --- |
| STS GetCallerIdentity | IAM user **enver-terraform-local**, cuenta926925605666 esperada; sin credenciales publicadas |
| ECR ListImages --repository-name enver-dev-api | **imageIds: []**, **0 imágenes / 0 digests**, ningún digest existente en ese repository al momento de la consulta; sin limitar paginación |
| ECR DescribeRepositories --repository-names enver-dev-api | Repository real **enver-dev-api**, **IMMUTABLE**, **scanOnPush=true**, **AES256**, URI/ARN coinciden con§18 |
| ECR GetLifecyclePolicy --repository-name enver-dev-api | Única regla **untagged / sinceImagePushed / days / 14 / expire**, sin reglas tagged |

URI **926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api**, ARN **arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api**. La denegación ListImages§18 queda superada para esta lectura, **no borrada del historial**. No se publica enver-api:infra-validation ni se genera digest ficticio. Repository vacío no acredita scanning/findings de una imagen o rollback; registry BASIC/rules=[] conserva su fecha§18, no se vuelve a consultar.

**Policy local sincronizada:** [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json) añade **únicamente ecr:ListImages** a ReadSingleApiRepository. Comparación JSON contra HEAD retirando esa acción confirma igualdad semántica del resto. **4 Allow / 10 acciones únicas exactas / 1648 caracteres compactos**, sin ecr:*, AdministratorAccess, PowerUserAccess ni nuevos permisos publisher/runtime/otros servicios. ListImages hereda ARN enver-dev-api/us-east-1, misma cuenta mediante aws:ResourceAccount=${aws:PrincipalAccount}, username y región. Solo GetRegistryScanningConfiguration conserva Resource=* por no admitir ARN; no ensanchar el listado a todos los repositories. Hash local SHA256 **68ab0b17e25e03378ad801357f980e44146bc909873678a2921b3df5a59866b1**; hash anterior§16/17 histórico, no descripción actual. Política local no es boundary o auditoría de IAM remoto.

Validación local **20:23:22.8813360Z–20:23:35.5318584Z**: `terraform fmt -check -recursive infra` **exit 0**, `terraform -chdir=infra validate -no-color` **exit 0 / Success! The configuration is valid.** **No nuevo plan/apply/init**. Se conserva post-plan§18 **2026-11-12T20:07:16.2036319Z–20:07:34.3744522Z**, exit0/**0/0/0**, networking14+ECR2 no-op; evidencia histórica, no replan fresco en§19. State/backup/planes con hashes iguales antes/después; state serial18/16managed y backup serial15/14managed conservados sin restauración. Exclusión de state/sufijos/.terraform/tfvars reales/tfplan requerida también en revisión Git; lock y ejemplo siguen versionables.

**T-INFRA-007 PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: repository/lifecycle reales y ahora inventario vacío acreditado; dependencias002/005/025, contrato/retención/DP-I-14 y revisión integral pendientes. **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: todavía sin imagen publicada/digest real/validación ECR/scanning de imagen/CI/rollback por digest; reproducibilidad/bases por digest y ownership004 pendientes. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**: fmt/validate y evidencia anterior conservados; custody/cifrado/backup protegido/recovery/E001/E002/E025 integral pendientes. **Nueve gates PENDIENTE / NO SATISFECHO**, incluidos I-A/B; nueve tareas parciales001–008/025, otras21 pendientes. Sin cambios002/004, AUTH-027 integral/GATE-G/H o aceptación de producción.

Tarifa previa **USD0.10/GB-mes** conservada; inventario vacío observado, no medición Billing/GB-mes acumulados ni costo mensual cero/Free Tier asumido. No imagen subida o recurso nuevo creado en§19.

Checkpoint autorizado: stage únicamente **infra/bootstrap/terraform-local-ecr-policy.json**, **specs/003-infrastructure/task.md**, **memoria.md** y commit **docs(infra): record ecr deployment and list permission**. Revisión Git/hash/exclusiones antes de stage y verificación de commit/árbol después; resultado y hash del commit se comunican al cierre, sin autorreferencia del hash dentro de su propio contenido. No push/merge/rebase/amend/squash ni stage de artefactos privados.

Evidencia privada local en `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-ecr-list-checkpoint-20261112-202233\`: respuestas/resúmenes de lectura, comparación policy, validación/hashes y revisión Git. Carpeta temporal no acredita retención/custodia. **Sin apply, plan nuevo, Docker push/login/tag/build, IAM remoto modificado, workloads o cleanup; detener tras checkpoint, sin siguiente incremento automático.**

## 20. T-INFRA-008 — primera publicación ECR preparada, build local por SHA, sin push

### 20.1. Autorización, baseline y trazabilidad

**2026-11-12**, solicitud **BUILD DOCUMENTAL + VALIDACIÓN LOCAL / NO DOCKER PUSH / NO TERRAFORM APPLY / NO CAMBIOS AWS AUTOMÁTICOS**. Se revisan spec/plan/task sin reinterpretar criterios. Baseline verificada **feature/infrastructure**, HEAD **3312d220e3ff68851befc59e9a7cfa516185bdc8**, `docs(infra): record ecr deployment and list permission`, árbol **limpio / 0 staged** antes y durante build. Este checkpoint§19 incluyó exactamente provisioning JSON/task/memoria; no push Git. Autorización actual permite solo publisher JSON nuevo y task/memoria editados, build/inspección local y fmt-check/validate; sin stage/commit/push o ejecución AWS CLI/IAM.

**Tag seleccionado: git-3312d220e3ff**, primeros12 hex del SHA completo anterior, identidad primaria determinística por commit, no timestamp/latest. Tag local **enver-api:git-3312d220e3ff**; destino propuesto **926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api:git-3312d220e3ff**. Relación externa de evidencia **SHA Git completo → tag local/ECR por SHA → imageDigest leído en ECR después del push**; actualmente solo primeros dos identificadores locales preparados, destino/tag ECR no creado. No se añade label OCI/archivo productivo ni se declara atestación criptográfica de procedencia por nombrar el tag.

Repository real/configuración e inventario **imageIds=[] / 0 imágenes / ningún digest** corresponden a lectura§19 **20:22:34.9357668Z–20:22:47.1485716Z**, no se consultan otra vez en§20. Antes de publicar se deberá revalidar identidad/región/repository/IMMUTABLE/tag ausente con permiso efectivo y autorización nueva. Si tag ya existe, **DETENER**, incluso si parece mismo digest; no sobrescribir, borrar, volver mutable ni sustituir tag con latest. AccessDenied/error no significa tag ausente. Una segunda construcción del mismo commit puede resolver bases distintas: no reutilizar tag publicado, decidir nueva identidad aprobada con SHA como base y evidencia propia. Verificar SHA completo frente al prefijo corto para evitar colisiones.

### 20.2. Dockerfile intacto y build real desde HEAD limpio

- **Enver.API/Dockerfile intacto**, SHA256 **6a9c69f5b5517129d046997c72a84d80631eb8dce377641afae65e3107e442a0**. Runtime base aspnet:10.0, SDK10.0.403 y global.json10.0.403/rollForward:disable; restore locked-mode/build y publish no-restore. Stage final **FROM base AS final**, hereda **USER $APP_UID**, WORKDIR/app, ENTRYPOINT **["dotnet","Enver.API.dll"]**, EXPOSE8080/8081. Contexto correcto: **raíz del repositorio**, por COPY global.json/Enver.API/…; `.dockerignore` excluye gobierno/specs/.git/secretos locales habituales/state/planes/tfvars/.terraform/bin/obj. No pasar credenciales/build args secretos.
- Docker cliente/servidor **29.6.1**, disponible. Tag local seleccionado no existía; no se sobrescribe imagen previa. Una invocación real **`docker build --file Enver.API/Dockerfile --tag enver-api:git-3312d220e3ff .`**, **20:31:13.1959806Z–20:32:05.8396958Z**, **exit 0**, build informa **0 Warning(s) / 0 Error(s)**. Se ejecutó antes de crear publisher JSON o editar documentación; HEAD/árbol/hash de todas las fuentes versionadas iguales antes/después. No se reutiliza infra-validation como evidencia suficiente; el builder puede reutilizar caché normal, no se afirma no-cache.
- `docker image inspect` exit0: **image ID local reportado sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda**, **linux/amd64**, **Size101036277bytes ≈96.36MiB** según Docker, no GB facturados/comprimidos ECR. **Config.User=1654/non-root**, .NET/ASP.NET **10.0.12**, **ASPNETCORE_HTTP_PORTS=8080**, ENTRYPOINT indicado. Puerto8080 acreditado como configuración, no escucha/arranque/red/TLS probado; EXPOSE8081 no certifica TLS.
- BuildKit exporta manifiesto e índice local; **image ID/manifest/índice locales NO acreditan digest ECR desplegable**. Al publicar, distinguir índice OCI y manifiesto de plataforma, conservar ambos y dependencias cuando corresponda; referencia A/B debe usar el digest elegido/verificado en ECR, no copiar este ID a compute.
- Bases siguen tags no fijados por digest; reproducibilidad byte a byte, secretos en capas/SBOM/vulnerabilidades/hardening runtime y CI verde no acreditados. Build correcto no declara imagen segura ni app funcional; sin app/container startup, pruebas UT/IT/ST o Docker push/login/tag remoto. Imagen local conservada, sin cleanup. Primer script auxiliar falló por paréntesis en función SHA256 de PowerShell, **ParserError/exit1 antes de ejecutar comandos**; corregido solo lector local, sin fallo/reintento de Docker build ni cambio de fuentes.

### 20.3. Publisher mínimo separado — PROPUESTA LOCAL / NO APLICADA

Nuevo [local-ecr-publisher-policy.json](../../infra/bootstrap/local-ecr-publisher-policy.json), **2Allow / 9 acciones exactas / 827caracteres compactos**, SHA256 **4a4feedc8cdd2057b396a40efcde5fb8e8bcdb42dc37df178e911dc6511ffbcd**. Parseo JSON/allowlist/ARN/condiciones verificados localmente, **no IAM simulation/Access Analyzer ni permisos efectivos probados**. Provisioning [terraform-local-ecr-policy.json](../../infra/bootstrap/terraform-local-ecr-policy.json) **intacto**, separado; no policy remota creada/adjunta ni grants acumulados auditados.

| Acción incluida | Necesidad en el flujo futuro |
| --- | --- |
| ecr:GetAuthorizationToken | Autenticación ECR de Docker vía get-login-password; **Resource="*"**, sin resource type ARN según referencia AWS, no puede restringirse al ARN repository |
| ecr:BatchCheckLayerAvailability | Comprobar capas ya presentes antes de subida |
| ecr:InitiateLayerUpload / UploadLayerPart / CompleteLayerUpload | Subida de capas |
| ecr:PutImage | Publicar manifiesto/tag inmutable |
| ecr:BatchGetImage | Lectura de manifiesto/comprobaciones Docker incluidas en receta oficial AWS de push; no añadir descarga de capas por ello |
| ecr:DescribeImages | Preflight de colisión por tag y digest/metadata post-push desde ECR |
| ecr:DescribeImageScanFindings | Verificar estado/findings reales de scanning tras push, no inferirlos de DescribeImages o flag scan_on_push |

**Ocho acciones de repository** limitadas a **arn:aws:ecr:us-east-1:*:repository/enver-dev-api**, región **us-east-1**, username **enver-terraform-local**, **aws:PrincipalAccount=926925605666**, **aws:ResourceAccount=${aws:PrincipalAccount}**. Token solo región/username/PrincipalAccount, no ResourceAccount/tag de un repository inexistente en esa operación. Wildcard de cuenta ARN queda acotado por guard de misma cuenta; wildcard Resource del token no concede por sí mismo push/pull en otros repositories. Estas son Allow, **no permissions boundary** ni límite de otros grants; provisioning y publishing separados en archivos no crean aislamiento efectivo de identidades si se adjuntan ambos al mismo usuario.

**Omitidos tras evaluar:** ListImages no necesario para flujo por tag/digest con DescribeImages y ya disponible en provisioning para inventario; no duplicarlo por comodidad. **GetDownloadUrlForLayer** se necesita para descargar capas/pull completo, no para push/metadata: omitido, futura prueba de pull/uso por digest requiere rol/permiso/alcance propio. Sin DeleteRepository/BatchDeleteImage/PutLifecyclePolicy/CreateRepository/PutImageTagMutability/PutImageScanningConfiguration/StartImageScan/TagResource ni cambios globales. Sin ecr:* o acciones de otros servicios, AdministratorAccess/PowerUserAccess. No KMS/S3 directos por almacenamiento AES256 gestionado de ECR.

Fuentes AWS consultadas **2026-11-12**: [permisos push a repository específico](https://docs.aws.amazon.com/AmazonECR/latest/userguide/image-push-iam.html), [acciones/resource types ECR](https://docs.aws.amazon.com/service-authorization/latest/reference/list_ecr.html). GetAuthorizationToken no tiene resource type; las ocho acciones restantes admiten repository. Una primera URL alternativa de referencia no devolvió contenido útil; se usó list_ecr.html, sin APIs de cuenta AWS.

Policy **solo bootstrap/prueba local controlada**, credenciales del perfil externo, no recomendación de identidad productiva. Futuro definitivo **GitHub Actions → AWS OIDC → roles temporales separados de publicación/deployment**, runtime pull distinto; **NO access keys de largo plazo en GitHub**. Condición aws:username de este JSON es para IAM user local, **no reutilizar sin rediseño como policy de role OIDC**. No OIDC/trust/roles/workflows creados. CI futuro automático y CD producción separado/exclusivamente humano por Run workflow/workflow_dispatch, CI verde exacto y digest inmutable; no aplicar Terraform automáticamente.

### 20.4. Flujo FUTURO, NO EJECUTADO — exige nueva autorización

1. Aprobar/aplicar manualmente publisher por responsable y verificar cuenta/región/identidad/permisos; no acción realizada aquí. Revalidar repository/config/costo/retención y preflight de tag con **DescribeImages --image-ids imageTag=git-3312d220e3ff**: éxito con imagen implica **DETENER**; solo ImageNotFound específico bajo identidad/acceso acreditados permite proseguir. Ante AccessDenied/cualquier otro error, detener, no ampliar IAM o reintentar a ciegas. Inventario vacío histórico no reserva el tag.
2. Vincular exactamente SHA completo3312d220… y **el artefacto local registrado** mediante image inspect; si ID/tag/fuentes cambian o imagen desaparece, detener/revisar. No reconstruir silenciosamente al promover ni atribuir un rebuild posterior al artefacto ya revisado. Cambios documentales actuales no se incluyen retroactivamente en el build anterior.
3. Solo tras autorización de login/publicación, usar password-stdin sin imprimir/token en logs/argumentos/archivos. Preparación PowerShell con comprobación de ambos exit codes; **NO ejecutada**:

```powershell
try {
    $password = aws ecr get-login-password --region us-east-1 --profile enver-infra
    if ($LASTEXITCODE -ne 0 -or -not $password) { throw 'ECR token falló; detener' }
    $password | docker login --username AWS --password-stdin 926925605666.dkr.ecr.us-east-1.amazonaws.com
    if ($LASTEXITCODE -ne 0) { throw 'Docker login falló; detener' }
} finally {
    Remove-Variable password -ErrorAction SilentlyContinue
}
```

Login puede persistir token en credential helper/config Docker: aprobar custodia antes de ejecutarlo, no hacerlo como mera validación en§20. Sin trazado/transcript que revele password; no loguear token. Token temporal no reemplaza revisión de permisos del principal.

4. Comandos **futuros / NO ejecutados**, detener ante cualquier exit distinto de0 y comprobar ID local antes/después del tag:

```text
docker tag enver-api:git-3312d220e3ff 926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api:git-3312d220e3ff
docker push 926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api:git-3312d220e3ff
aws ecr describe-images --repository-name enver-dev-api --image-ids imageTag=git-3312d220e3ff --region us-east-1 --profile enver-infra
```

5. Registrar **imageDigest real de ECR**, media type/arquitectura/índice-plataforma, tag, SHA Git completo, fecha/operador y resultado. Solo entonces formar **repository-uri@imageDigest** y contrastar con manifiesto publicado; no inventar sha256. Futuro CI añadirá run/attempt/artefacto/CI verde exacto, no existente en esta preparación local. Mismo digest en A/B, promoción sin rebuild, no latest como identidad de deployment.
6. Verificar **scanning real** usando `aws ecr describe-image-scan-findings --repository-name enver-dev-api --image-id imageDigest=<DIGEST_REAL_ECR> --region us-east-1 --profile enver-infra` después del push; marcador no ejecutable, sustituir solo por evidencia real. Revisar estado terminal soportado/findings/errores y política de aceptación de vulnerabilidades **antes de declarar apta**. BASIC/rules=[] y scan_on_push=true históricos no prueban scan completado/imagen limpia. Si scan pendiente/no soportado/denegado/falla, registrar y bloquear aceptación; no StartImageScan/Inspector/ENHANCED automático ni nuevos costos sin permiso.
7. Pull/uso por digest y rollback quedan para otro alcance/identidad autorizado; publisher actual no permite descargar capas. Retener etiquetas inmutables/manifiestos activo y previo compatible, lifecycle solo untagged >14d desde push no protege un digest si pierde último tag. Nunca borrar/reutilizar tag o restaurar state de seguridad antiguo por fallo del primer push; si hay capas/manifiesto parcialmente publicados, inspección autorizada y decisión humana, sin cleanup automático.

### 20.5. Validación, límites y detención

**20:33:01.9841793Z–20:33:15.3162181Z**, `terraform fmt -check -recursive infra` **exit0**, `terraform -chdir=infra validate -no-color` **exit0 / Success! The configuration is valid.** Sin cambios HCL/provider/lock, **sin plan/apply/init**. Post-plan§18 **0/0/0/red14+ECR2no-op** conservado como evidencia fechada, no revalidación cloud actual. State/backup/planes conservados por hash; Docker config preservado, sin login. Ninguna AWS CLI/API de cuenta, recurso/IAM/Atlas modificado, tag ECR creado o byte subido. Build local no incurre almacenamiento ECR por este bloque; tarifa previa USD0.10/GB-mes no es total/factura ni se asume Free Tier.

**T-INFRA-007** conserva **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**, evidencia real§§18–19 y dependencias/contrato/retención pendientes. **T-INFRA-008 PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, falta publisher aplicada/permisos efectivos, login, push, digest real ECR, scanning real, pull/uso por digest/rollback y aceptación contractual/ownership004/reproducibilidad/seguridad. **T-INFRA-025 PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, custody/cifrado/backup protegido/recovery/E025 integral/dependencias pendientes. **Nueve gates PENDIENTE / NO SATISFECHO**, nueve tareas parciales001–008/025 y otras21 pendientes; Authentication local/002 GATE-G/H/AUTH-027 integral/004 intactos.

Solo **local-ecr-publisher-policy.json nuevo + task/memoria modificados**, sin stage/commit/push, HEAD3312d220 conservado. Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-publisher-prep-20261112-203110\` (build/metadata/hashes/validación/revisión local), sin retención/custodia acreditadas por carpeta temporal. **BUILD LOCAL COMPLETO / PUBLISHER PROPUESTO NO APLICADO / LOGIN-PUSH-DIGEST-SCANNING ECR PENDIENTES.** Detener para revisión humana, sin publicación/siguiente incremento/cleanup automático.

## 21. T-INFRA-008 — publicación autorizada bloqueada en token ECR; sin login/tag/push

### 21.1. Autorización, IAM informado y prechecks — 2026-11-12

Solicitud **BUILD CLOUD CONTROLADO / VALIDAR PUBLISHER + PUBLICAR PRIMERA IMAGEN ECR** autoriza solo el artefacto exacto de HEAD **3312d220e3ff68851befc59e9a7cfa516185bdc8**, tag **git-3312d220e3ff**, repository **enver-dev-api**, región **us-east-1**, perfil **enver-infra**; identidad/inventario, login/tag/un push y lecturas digest/scanning, pull por digest condicional a permisos. No build nuevo, latest/segundo tag/push, AWS workloads, IAM modificado, plan/apply/destroy/import, OIDC, arranque de contenedor o Git stage/commit/push.

Rama **feature/infrastructure**, HEAD anterior confirmado, árbol inicial con **task/memoria modificados + publisher JSON sin seguimiento, 0 staged**, trabajo§20 conservado. **INFORMADO POR RESPONSABLE:** creó/adjuntó manualmente **ENVER-Local-ECR-Publisher** desde JSON local, sin AdministratorAccess/PowerUserAccess/ecr:*. No se enumera/audita documento/versión/adjuntos/grants/boundaries/SCP remoto ni se presenta como IAM modificado por el agente. El estado previo de propuesta/no aplicada§20 se conserva como historia.

Validación local del publisher: mismo SHA256 **4a4feedc8cdd2057b396a40efcde5fb8e8bcdb42dc37df178e911dc6511ffbcd**, JSON válido **2Allow/9acciones exactas**, token Resource=* y acciones repository/condiciones/región/cuenta/usuario iguales a§20; provisioning JSON intacto. No simulación IAM, prueba de todos los permisos o auditoría remota por parsear JSON.

Gate pre-login **20:40:30.0861799Z–20:40:36.8246558Z**:

| Comprobación real | Resultado |
| --- | --- |
| STS get-caller-identity --profile enver-infra | **exit0**, IAM user **enver-terraform-local**, cuenta926925605666 esperada |
| ECR list-images --repository-name enver-dev-api --region us-east-1 --profile enver-infra | **exit0 / imageIds=[] / 0 imágenes**, sin tag existente; sin truncar paginación |
| Docker image inspect enver-api:git-3312d220e3ff | **exit0**, ID **sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda**, **linux/amd64**, coincide exactamente con baseline; no rebuild |
| Procedencia local | Fuentes versionadas del build§20 iguales por hash salvo task/memoria autorizados; HEAD íntegro, publisher separado; imagen no regenerada |
| Credenciales Docker | Credential helper existente **desktop** disponible, sin leer/publicar secretos; login aún no ejecutado |

Configuración repository IMMUTABLE/scan_on_push/AES256 y lifecycle de§19 no consultada otra vez en este intento. Vacío anterior acreditado en la hora indicada, no reserva eterna del tag ni permiso para saltar revalidación futura.

### 21.2. AccessDenied antes de Docker login — detención

Una llamada **`aws ecr get-login-password --region us-east-1 --profile enver-infra`**, **20:41:09.7233779Z–20:41:12.6294320Z**, **exit254 / AccessDeniedException**:

- **Acción IAM:** **ecr:GetAuthorizationToken**.
- **Recurso denegado:** **Resource="*"**.
- **Principal:** **arn:aws:iam::926925605666:user/enver-terraform-local**.
- **Fase:** token necesario para login Docker; **Docker login no invocado**, su exit code **NO APLICA**, no fallo de password-stdin/Docker.
- **Mensaje AWS:** **no identity-based policy allows the ecr:GetAuthorizationToken action**.
- **Operación/recurso Terraform afectados:** **NINGUNO**, no Terraform cloud ejecutado.

Token stdout capturado únicamente en memoria para password-stdin, nunca impreso/guardado; no token válido obtenido. Al comprobar exit254 se eliminó la variable y no se invocó docker login. `AWS_MAX_ATTEMPTS=1`, mensaje CLI reached max retries:0; **sin reintento** ni token alternativo/login manual. Primer lector de stderr lo detectó como archivo binario por redirección UTF-16 de PowerShell; se leyó solo ese error local con Get-Content, **no se repitió ninguna llamada AWS ni se leyó password/token**.

**Detenidas todas las operaciones AWS/Docker de publicación/verificación.** No local tag con formato registry, **no docker push**, no capas/manifiesto subidos, push exit **NO APLICA**, sin digest Docker de push/imageDigest ECR/imagePushedAt/imageSizeInBytes nuevos. **0 imágenes** corresponde a ListImages **pre-push**; no inventar post-list o digest. Sin consulta scanning/findings, no etiquetar como IN_PROGRESS/imagen limpia; sin pull por digest porque no hay digest publicado acreditado. No se intenta descargar capas para provocar otra denegación. **Referencia repository@digest no construible**, no sha256 ficticio. Ni primer digest ECR ni digest previo de rollback acreditados; rollback real **NO PROBADO / PENDIENTE**.

JSON local incluye la acción token pero adjunción/manual informada no acredita Allow efectivo; el error no prueba por sí solo si causa es documento/versión/adjunto/condición/propagación u otro detalle. Responsable debe revisar **cuenta/user/adjunto/versión activa/condiciones** del permiso exacto ya previsto. **No ampliar permisos ni modificar JSON/IAM automáticamente**, ni asumir que esperar resolverá el fallo; nueva validación/reanudación requiere autorización explícita, nunca reintentar a ciegas. GetAuthorizationToken ya usa Resource=* porque no admite repository ARN; no ensanchar las acciones repository para resolverlo. Separación provisioning/publishing y futuro OIDC/roles temporales§20 conservados, sin access keys de largo plazo en GitHub.

### 21.3. Validación local, estados y cierre

Después del bloqueo, solo validación local/documentación: **20:42:01.9723825Z–20:42:15.2236512Z**, `terraform fmt -check -recursive infra` **exit0**, `terraform -chdir=infra validate -no-color` **exit0 / Success! The configuration is valid.** Sin plan/apply/destroy/import/init. Post-plan§18 **0/0/0**, red14+ECR2no-op, conserva su fecha histórica; no replan actual. State/backup/planes y Docker credentials config idénticos por hash, **242 fuentes existentes/publisher verificadas antes de editar docs**, provisioning/publisher/HCL/lock/Dockerfile/producto/tests/spec/plan/004 intactos.

**T-INFRA-007 conserva PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE**: repository/lifecycle reales previos e inventario vacío revalidado, dependencias/retención/contrato/aceptación pendientes. **T-INFRA-008 conserva PARCIAL / REVISIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, con bloqueo operativo **GetAuthorizationToken denegado**; policy adjunta solo informada, permisos efectivos/login/push/digest/scanning/pull/rollback/reproducibilidad/seguridad/ownership004/contrato pendientes. **T-INFRA-025 conserva PARCIAL / VALIDACIÓN LOCAL ACREDITADA, ACEPTACIÓN PENDIENTE**, custodia/cifrado/backup protegido/recovery/dependencias/E025 integral pendientes. **Nueve gates PENDIENTE / NO SATISFECHO**, nueve tareas parciales001–008/025 y otras21 pendientes. Sin aceptación nueva002 GATE-G/H/AUTH-027 integral o producción.

Ninguna imagen/byte ECR/recurso/IAM/Atlas modificado por el agente en§21; no cambios de tarifa/Billing o costo mensual inventado, USD0.10/GB-mes previo como referencia, no Free Tier asumido. Solo task/memoria editados ahora; se conserva publisher JSON no rastreado de§20 sin alterarlo, **0staged / sin commit/push**, HEAD3312d220 íntegro. Evidencia local privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-first-push-20261112-204027\` (gate/respuestas pre-push, metadatos/error de token saneado, hashes y validación), sin credenciales/tokens guardados ni retención/custodia garantizadas por carpeta temporal. **PUBLICACIÓN BLOQUEADA ANTES DE LOGIN / SIN TAG-PUSH-DIGEST-SCANNING-PULL.** Detener para revisión humana, sin nuevas llamadas AWS, reintentos, IAM, cleanup o siguiente incremento automático.

## 22. Checkpoint primera imagen ECR publicada — evidencia posterior informada por responsable

**2026-11-12**, solicitud **GIT CHECKPOINT CONTROLADO / NO NUEVAS OPERACIONES AWS**. Baseline local confirmada **feature/infrastructure**, HEAD **3312d220e3ff68851befc59e9a7cfa516185bdc8**, publisher JSON nuevo/task y memoria modificados, **0 staged**, sin otros cambios. Prechecks git status/diff --check/diff --stat/rev-parse HEAD correctos; diff --check exit0. Se conserva íntegro el intento fallido§21 y su fuente de observación directa, no se reescribe como exitoso ni se repite token/login/push/pull.

**Fuente de los resultados siguientes: responsable, prompt CHECKPOINT PRIMERA IMAGEN ECR PUBLICADA.** Los archivos todavía reflejaban el bloqueo§21 al iniciar este checkpoint; se sincronizan con la publicación posterior comunicada. No se aportaron/inspeccionaron aquí logs nativos adicionales de esa publicación ni se ejecutaron nuevas consultas AWS/Docker: registro de evidencia informada por responsable, **no verificación técnica independiente nueva del agente**. No se inventan la corrección IAM que permitió publicar, permisos efectivos de pull, actor/comandos adicionales u horarios de ejecución distintos del pushedAt comunicado.

| Dato informado y registrado | Valor |
| --- | --- |
| Git SHA fuente | **3312d220e3ff68851befc59e9a7cfa516185bdc8** |
| Repository / región | **enver-dev-api / us-east-1** |
| Tag publicado | **git-3312d220e3ff**; **1 tag publicado**, no latest |
| Docker push | **exit0**, según responsable; no push nuevo en este checkpoint |
| Digest ECR | **sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda** |
| imagePushedAt | **2026-11-12T21:36:45.185000Z** |
| imageSizeInBytes | **101033417bytes**, valor informado de ECR; no confundir con Size local101036277bytes§20 |
| Pull de la misma imagen por digest | **exit0**, según responsable; no pull nuevo/arranque de contenedor en este checkpoint |
| Scanning linux/amd64 | **IN_PROGRESS**, estado informado, no estado final ni imagen limpia |
| Findings/severidades | **PENDIENTES / NO DISPONIBLES** en la evidencia comunicada; **no findings=0** |
| Rollback real | **PENDIENTE**, todavía no hay digest/version anterior publicada |

**Referencia desplegable registrada:** `926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api@sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda`.

Los **3 objetos OCI observados según responsable** corresponden a **índice**, **manifiesto linux/amd64** y **atestación** de la misma publicación; **NO son tres versiones distintas** ni tres tags/pushes autorizados. No se inventan digests de los objetos hijos no comunicados. El digest ECR informado coincide numéricamente con image ID local§20, pero su procedencia se registra como resultado ECR comunicado, no se deriva del ID local como prueba. Scanning IN_PROGRESS aplica a linux/amd64, no se adjudica automáticamente al índice/atestación. Resultado final/findings/aceptación requieren evidencia posterior autorizada.

Primer digest conocido de publicación, sin versión previa compatible para rollback real. Retener tag/manifiestos dependientes; no cleanup, retirar etiquetas, nuevo push o mutabilidad relajada. Pull exit0 informado no demuestra descarga desde host limpio/sin caché, uso runtime A/B o permiso GetDownloadUrlForLayer efectivo; el JSON publisher permanece exactamente con nueve acciones, no se añade ese permiso ni se afirma resuelta su ausencia. Runtime/roles/004/reproducibilidad y seguridad siguen pendientes.

**Revisión local publisher:** [local-ecr-publisher-policy.json](../../infra/bootstrap/local-ecr-publisher-policy.json), mismo SHA256 **4a4feedc8cdd2057b396a40efcde5fb8e8bcdb42dc37df178e911dc6511ffbcd**, JSON válido, **2Allow/9acciones exactas**: GetAuthorizationToken; BatchCheckLayerAvailability/InitiateLayerUpload/UploadLayerPart/CompleteLayerUpload/PutImage/BatchGetImage/DescribeImages/DescribeImageScanFindings. Sin secretos detectados en su contenido, AdministratorAccess/PowerUserAccess/ecr:*/ec2:*/iam:* o permisos provisioning/borrado/lifecycle. Token Resource=* justificado por acción sin ARN; acciones repository y guards cuenta/región/user intactos. Provisioning JSON/HCL/lock/Dockerfile/producto/tests/spec/plan/004 no modificados. Esta revisión no audita IAM remoto ni crea roles/OIDC o credenciales de largo plazo en GitHub.

State/sufijos/backups/.terraform/tfplan/tfvars reales/credenciales Docker **no versionados**; exclusiones verificadas y lock/ejemplo siguen versionables. State/planes preservados por hash, no Terraform ejecutado; se conservan fmt/validate§21 exit0 y post-plan§18 0/0/0 como evidencia histórica fechada, no revalidación actual. Tarifa previa USD0.10/GB-mes conservada, bytes informados no son consumo GB-mes/factura total; no costo mensual/Free Tier inventados.

**T-INFRA-007 PARCIAL / aceptación pendiente**: repository/config previos y publicación informada no aceptan dependencias/retención/contrato. **T-INFRA-008 PARCIAL / publicación, digest y pull acreditados según responsable; scanning final, rollback y aceptación pendientes**; no COMPLETADA, no CI verde o deployment por digest demostrado. **T-INFRA-025 PARCIAL / aceptación pendiente**, protección/recovery/state/dependencias/E025 integral no resueltos. **GATE-I-A PENDIENTE / NO SATISFECHO**, **GATE-I-B PENDIENTE / NO SATISFECHO**, nueve gates abiertos, nueve tareas parciales001–008/025 y otras21 pendientes. Authentication local/002 GATE-G/H/AUTH-027 integral/004/producción intactos.

Checkpoint autorizado únicamente de **infra/bootstrap/local-ecr-publisher-policy.json**, **specs/003-infrastructure/task.md**, **memoria.md**, mensaje **feat(infra): publish first ecr image by digest**. Stage de esos3archivos después de revisión, verificar commit/árbol limpio/0staged al terminar; hash final se comunica al cierre sin autorreferencia dentro del commit. No amend/squash/merge/rebase/push Git.

Evidencia privada local de esta revisión documental/Git: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra4-image-checkpoint-20261112-215724\` (hashes/exclusiones/revisión JSON, resumen de publicación **informada**, revisión Git), no logs nuevos de publicación ni custodia/retención garantizadas por carpeta temporal. **Ninguna operación AWS/Docker/Terraform o cambio IAM por el agente en este checkpoint.** Detener después del commit, sin seguimiento automático de scanning/publicación/workloads/cleanup.

## 23. INFRA-5 — análisis compute/recuperación y detención previa por presupuesto

### 23.1. Autorización, baseline y resultado del precheck

**2026-11-12**, solicitud **INFRA-5 — COMPUTE BASELINE / T-INFRA-009 + T-INFRA-010 / IAM RUNTIME + 2 EC2 + SSM**, BUILD CONTROLADO, PLAN PRIMERO, **NO APPLY**. Se consultan spec/plan/task003, en particular005/009/010 y gates I-C/H/I; se conservan contratos y dependencias. Autorizaba preparar compute.tf/iam-runtime.tf, bootstrap/policies revisables, fmt/check/validate y un intento de plan; EIP solo propuesta y sin IAM aplicado. Incluye condición expresa: **si el diseño amenaza el presupuesto USD50, detener y recomendar alternativa**.

Precheck local real: `git status --untracked-files=all`, `git rev-parse HEAD` y verificaciones de branch/porcelain: **feature/infrastructure**, **25daa836c0571d4c0c02486e49ce435a108f0558**, **árbol limpio/0staged**. Este commit tiene padre3312d220 y preserva publisher/task/memoria de§22; no nuevo stage/commit/push. Perfil/cuenta/user y16recursos se toman de baseline/evidencia histórica, **no STS/inventario fresco**. Terraform1.17.1/provider6.73.0 se conservan como versiones del baseline, no se ejecuta version ni actualización.

**Resultado:** solo análisis previo y registro en este task/memoria. **No compute.tf, iam-runtime.tf, scripts/unit, cambios de variables/outputs/ejemplo, policies compute/IAM nuevas, Terraform fmt/validate/plan o consultas AWS CLI**. Condición de costo activada antes de implementar; no se fabrica un plan o una denegación para completar el formato de salida. Consultas HTTPS públicas a documentación/precios AWS no usan credenciales ni modifican la cuenta. Sin apply/destroy/import, recursos/EIP, cambios IAM, Docker, pruebas o despliegue. No lectura opcional de scanning.

### 23.2. Red y comparación de salida — propuesta, no provisión

Lectura local confirma subnets con **map_public_ip_on_launch=false**, ruta default al IGW y SG backend sin ingress público/22, solo TCP8080 desde SG ALB. El IGW/egreso SG por sí solos **no permiten salida IPv4 de EC2 sin IP pública/NAT**. No se modifica subnet, routing, SG ni se presupone SSM funcional. Diseño candidato: una EC2 por clave a/b, Zone IDs **use1-az1/use1-az2**, nombres us-east-1a/b según evidencia previa de la cuenta; aptitud/capacidad EC2 por AZ no consultadas ahora.

| Opción analizada | Funcionalidad/límites | Costo o pendiente |
| --- | --- | --- |
| A — dos EIP, una por EC2, sin IP automática | Salida IPv4 con IGW/DNS/egreso; Atlas allowlist futura de ambas /32. IP pública no abre SG ni autoriza API/SSH público. La EIP retenida puede reasociarse tras reemplazo. SSM/ECR requieren además agente/rol/red correctos | **USD0.005/IP-h**, **USD7.30/730h** las dos, in-use o idle. Es dependencia004; **solo propuesta**, no recurso HCL/creación/asociación ahora |
| B — interface endpoints privados | Ejemplo ssm + ssmmessages + ecr.api + ecr.dkr en2AZ, con private DNS/SG adecuados; S3 gateway para capas ECR, sin hora/procesamiento de gateway. ec2messages según agente/compatibilidad, otros servicios y bootstrap pueden requerir más | **4×2×USD0.01×730=USD58.40/730h**, más procesamiento inicial **USD0.01/GB**; endpoint adicional en2AZ≈USD14.60/730h. **No resuelve Internet/Atlas público ni repos externos**; Atlas privado/otra salida requiere contrato/costo adicionales, no asumidos |
| C — NAT Gateway | Da salida a hosts privados, puede aportar IP allowlisted, agrega costo/hora/GB y posible inter-AZ; uno es dependencia zonal y dos aumentan costo | **Descartado por prohibición**, no creado ni elegido. Ejemplo público consultado de Ohio USD0.045/h +USD0.045/GB sirve solo como advertencia de orden de costo, **no tarifa us-east-1 acreditada**; cotización regional pendiente si algún día se autoriza reconsiderarlo |

Para el perfil económico vigente se recomienda **A solo si se aprueban ventana/costo/egreso**. Crear hosts sin salida sería preparación inoperante para SSM/Docker, no compute funcional. Futuro bootstrap con EIP debe esperar su asociación antes de instalar/verificar, con espera/reintentos acotados y fallo explícito; evitar ciclo `instance → user_data → EIP → instance`, usando diseño de ENI/asociación o espera aprobado al implementar. No activar API por terminar bootstrap. Sin endpoints, NAT instance, proxy de salida o IPv6 añadidos como atajo.

### 23.3. AMI, capacidad, disco y bootstrap — alternativas revisables, no HCL existente

- **OS/arquitectura propuestos:** Amazon Linux2023 estándar **x86_64** (compatible con linux/amd64 publicada); no ARM, AMI histórica arbitraria o paquete/proveedor añadido. Estrategia mantenible documentada por AWS: `aws_ssm_parameter` sobre **/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64**, después `aws_ami` para verificar owner Amazon, arquitectura, virtualización/EBS y root-device/snapshot. **No AMI ID/version resueltos en la cuenta**. El alias latest/kernel-default cambia; no equivale a reproducibilidad. Antes de aplicar: registrar ID/version/fecha/kernel, revisar y fijar AMI seleccionada para ambos hosts/plan aprobado; actualizaciones por replan/reemplazo autorizado, sin rotación silenciosa. Documentación AWS consultada advierte cambio de kernel-default; no se promete kernel específico sin resolverlo.
- **Candidato de dimensionamiento:** **t3a.small**, AMD x86_64,2vCPU/2GiB, costo menor que t3.small; margen inicial para OS/SSM/Docker/API frente a1GiB. **No capacidad medida/aceptada** ni garantía de latencia/concurrencia. Config existente conserva hashing concurrency2 y presupuestos Register/Login8000ms, Logout4000ms; no se reducen hashing/seguridad para ajustarlos a máquina. Modo CPU **standard propuesto**, evita cargos por surplus Unlimited pero puede throttlear al agotar créditos: exigir prueba de CPU/RSS/créditos/latencia bajo carga antes de aceptación. Unlimited por defecto tiene posible **USD0.05/vCPU-h** adicional Linux según página T3, por eso no se asume costo fijo si se deja default. Disponibilidad de tipo/AZ no verificada.
- **Alternativa no aceptada:** t3a.micro2vCPU/1GiB, más barata pero menor margen; solo seleccionable después de prueba autorizada sin debilitar controles. Precio menor no prueba suficiencia, y no se ejecutan benchmark/backend ahora.
- **EBS candidato:** **gp3 10GiB raíz por host**,2volúmenes, cifrado con clave AWS administrada por defecto, delete_on_termination del root y sin datos críticos; confirmar tamaño mínimo del snapshot AMI antes de fijarlo. 3000IOPS/125MiB/s base sin extras, no snapshots/aprovisionamiento IOPS adicional elegidos. Margen para OS/Docker/imagen≈100MB y logs acotados; no dimensionar usando solo tamaño comprimido ECR. Política futura de rotación/local logs y disco debe impedir llenado, observabilidad central se reserva a INFRA-11.
- **Hardening candidato:** solo SG backend, sin key pair/puerto22, IMDSv2 obligatorio y alcance host, EBS cifrado, sin Docker socket público ni incorporación indiscriminada de usuarios al grupo docker; revisar entrega de credenciales temporales/IMDS a contenedor cuando INFRA-10 defina contrato. No configuración aplicada o validación de hardening real.
- **Bootstrap futuro:** instalar Docker desde repositorio AL2023 aprobado, habilitar Docker/SSM, directorio protegido de aplicación y versión de bootstrap controlada; instalación/release de paquetes trazable antes de aceptación, sin afirmar que `dnf install` variable en el tiempo es reproducible por sí sola. Sin credentials/tokens/URI/JWT/passwords en user_data/state. Cloud-init/systemd de host puede registrar errores saneados; no CloudWatch IAM/agent extra ahora. **Script no creado ni probado**.
- **Systemd futuro de aplicación:** evaluar unidad **no habilitada/no iniciada**, que exija digest/config/secretos runtime válidos antes de cualquier arranque y gestione el contenedor non-root; no escribir unidad ejecutable ficticia ni pull/run en cloud-init. Separar provisioning de selección/despliegue por004, un escritor del digest. El digest real de§22 se conserva como referencia, no como instrucción de autoarranque. No API hasta secrets/Atlas/Redis/authority/readiness/004/CI aprobados.

### 23.4. IAM y responsabilidades — no políticas creadas/adjuntas

| Principal/componente futuro | Propuesta delimitada / pendientes |
| --- | --- |
| Runtime EC2 | Rol propuesto **enver-dev-api-runtime**, trust solo `ec2.amazonaws.com`, instance profile propio, credenciales temporales IMDS, no access keys. **Sin role/profile definido en Terraform ni existente acreditado** |
| ECR consumidor | **ecr:GetAuthorizationToken** Resource=* (acción sin ARN, limitar región/cuenta); **ecr:BatchCheckLayerAvailability**, **ecr:GetDownloadUrlForLayer**, **ecr:BatchGetImage** solo `arn:aws:ecr:us-east-1:926925605666:repository/enver-dev-api`. Sin PutImage/CreateRepository/DeleteRepository ni permisos publisher/provisioning |
| SSM agente | Evaluada **AmazonSSMManagedInstanceCore**, ARN `arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore`, versión pública v2: acciones SSM/inventario/documentos/parámetros +4ssmmessages +6ec2messages, Resource=*. **Más amplia que solo sesión**, incluye GetParameter/GetParameters globales y AWS puede actualizarla; SDD exige mínimo privilegio, no la aprueba por nombre. Preferir evaluar policy custom session-only (`ssm:UpdateInstanceInformation` y4canales ssmmessages) contra agente elegido y llamadas reales, ampliando solo con revisión explícita. **Sin elección/attachment o SSM funcional acreditados** |
| Operador Session Manager | Permisos humanos StartSession/ResumeSession/TerminateSession y límites de documentos/destinos requieren revisión separada; no se conceden a EC2 ni se amplía el usuario local por implementar el agente |
| Terraform compute | Candidato futuro separado `terraform-local-compute-policy.json`: DescribeImages/Instances/InstanceStatus/Volumes y lecturas efectivamente exigidas por provider; RunInstances multirrecurso (AMI/subnet/SG/ENI/volume/instance), TerminateInstances/tagging solo recursos ENVER, región/tags cuando la acción lo soporte. DescribeAddresses solo con EIP en alcance; Allocate/Release/Associate/DisassociateAddress **no se conceden por mera propuesta EIP**. No ec2:* o escritura de networking global. Lista final requiere inspección de llamadas provider y diseño final, **no se presenta policy mínima validada** |
| Resolución AMI por operador | La estrategia SSM requeriría **ssm:GetParameter solo para el parámetro público AL2023 seleccionado**, no permiso de lectura global de parámetros/secretos; DescribeImages para validar sus atributos. No llamadas ni permiso concedido ahora; incluirlo explícitamente en propuesta posterior, no fingir que DescribeImages por sí solo resuelve el alias SSM |
| Terraform IAM | Candidato separado `terraform-local-iam-runtime-policy.json`: lifecycle exacto role/profile/attachment; lecturas y tagging requeridos por provider. Si ECR/custom SSM inline: PutRolePolicy/GetRolePolicy/DeleteRolePolicy adicionales a la lista de solicitud, solo role ENVER; no se omiten y luego se amplía a ciegas. Managed attachment solo ARN explícitamente aprobado. ARN/nombres acotados no constituyen permissions boundary ni validan contenido de policy; revisar riesgo de escalada antes de otorgar CreateRole/PutRolePolicy/PassRole |
| PassRole | Si se autoriza después, **solo `arn:aws:iam::926925605666:role/enver-dev-api-runtime`**, condición **iam:PassedToService=ec2.amazonaws.com**; sin PassRole arbitrario, role existente supuesto o iam:*. Nombre/ARN **propuestos**, no recursos actuales |
| Authority/secretos/logs | Ningún permiso DynamoDB, Secrets/KMS o CloudWatch inventado. Esperar nombres/ARN/contratos exactos INFRA-10/11; no tabla, inicializador o read global de parámetros para suplir secretos. Runtime solo consume lo aprobado, nunca crea/borra authority ni reabre confianza por arrancar |

Se preservan publisher y provisioning ECR existentes sin ampliarlos ni reutilizarlos como role runtime. Usuario Terraform sin permisos compute/IAM según responsable: declaración, **no auditoría efectiva nueva**. No AccessDenied observado ahora porque no se intenta plan/API; acción/recurso/fase denegados **NO APLICA**, no se inventa. Si un futuro plan autorizado deniega acceso: registrar error exacto y detener, sin IAM/reintento/credenciales alternativas automáticos.

### 23.5. Estimación regional pública y condición de detención

Consulta pública **2026-11-12**, capturas principales **22:10:07–22:10:13Z**. Fuentes sin autenticación: [EC2 T3/T3a, Linux us-east-1](https://aws.amazon.com/ec2/instance-types/t3/), [EBS precios](https://aws.amazon.com/ebs/pricing/) y [tabla regional gp3](https://b0.p.awsstatic.com/pricing/2.0/meteredUnitMaps/ec2/USD/current/ebs.json), [VPC precios](https://aws.amazon.com/vpc/pricing/) y [tabla regional IP/endpoints](https://b0.p.awsstatic.com/pricing/2.0/meteredUnitMaps/vpc/USD/current/vpc.json), [PrivateLink](https://aws.amazon.com/privatelink/pricing/), [ALB ejemplo us-east-1](https://aws.amazon.com/elasticloadbalancing/pricing/). EBS rateCode **JG3KUJMBRGHV3N8G.JRTCKXETXF.6YS6EN2CT7**; IP in-use **4GQUNXTFWVSGPUZK.JRTCKXETXF.6YS6EN2CT7**; endpoint **EN2N5TATXE673A3B.JRTCKXETXF.6YS6EN2CT7**.

**730h es escenario comparativo, NO duración aprobada ni reinterpretación de USD50 como mensual.** On-demand Linux/shared, sin Free Tier/crédito descontado, impuestos, reservas/Savings Plans o spot. EBS es referencia mensual de capacidad facturada por tiempo real; proyección a ventanas cortas aproximada. No se consultan Billing, saldo/vigencia/cobertura del crédito o gasto acumulado.

| Partida candidata us-east-1 | Cálculo | USD referencia730h/mes |
| --- | --- | --- |
| Dos EC2 t3a.small | 2 ×0.0188/h ×730h | **27.448** |
| Dos root gp3 10GiB, base | 2 ×10 ×0.08/GB-mes | **1.600** |
| Dos EIP/public IPv4 | 2 ×0.005/h ×730h | **7.300** |
| **Subtotal compute funcional propuesto** | EC2+root+salida EIP | **36.348 ≈36.35**, no total plataforma |
| ALB futuro — solo base hora | 0.0225/h ×730h | **16.425**; LCU variable0.008/LCU-h aparte |
| IPv4 mínimas futuras ALB2AZ | 2 ×0.005/h ×730h | **7.300**, puede haber más direcciones |
| **Subtotal futuro compute + ALB +mínimo IPv4** | Sin LCU ni dependencias restantes | **60.073 ≈60.07** |

No ALB planeado/creado ahora: se incluye **solo para no consumir presupuesto agregado ignorando una obligación posterior de rúbrica**. Falta Redis/tier Atlas, DynamoDB, Secrets/KMS, logs/métricas/alarma, ECR, DNS/dominio, transferencias, actualizaciones, reemplazos solapados y margen. ECR0.10/GB-mes previo es referencia, no consumo/factura medida. Transferencia Internet/inter-AZ/Atlas depende de ruta/volumen; **no se considera gratis ni se inventa USD0**. Servicios AWS misma región pueden tener tratamientos distintos por ruta; estimación integral requiere volumen explícito.

**Detención de seguridad de costo:** compute aislado36.35 es inferior a50, **pero no prueba que se respete el límite total**. En escenario730h el subtotal obligatorio parcial ya excede50, faltan duración y gasto acumulado y no se puede acreditar reserva≥50. No es afirmación de que48h cuesten60 ni de que la arquitectura sea inviable en cualquier ventana; el riesgo es aprobar baseline funcional sin calendario/costo global. Se obedece solicitud§20 y task§8: **DETENER antes de HCL/policies/plan, no continuar porque "no apply" permita ignorar la condición**. USD50 agregado/reserva objetivo conservados.

**Alternativa recomendada, NO aprobada/ejecutada:** demostración acotada, por ejemplo **48h** compute equivalente≈**USD2.39** más plataforma/dependencias/retención/margen, sin escoger duración por el responsable. Planificar/revisar/fijar AMI y bootstrap primero, provisionar justo antes de ventana, evidenciar ambos hosts y shutdown/limpieza separados después de autorización. EBS/EIP/ALB/datos retenidos siguen cobrando aunque EC2 se detenga; no promesa de cero cargos, destroy o reserva garantizada. Alternativa micro: 2t3a.micro≈13.724 EC2 +1.60root +7.30IP =**22.624/730h**, con ALB/IP mínimo **46.349/730h**, deja solo3.651 antes de dependencias/margen y capacidad1GiB sin prueba: **no seleccionada para forzar cumplimiento**. Endpoints4×2AZ ya58.40 sin hosts/Atlas, no alternativa económica aprobada; NAT prohibido. No reducir a una instancia, debilitar hasher o sustituir rúbrica.

Antes de retomar: responsable aprueba ventana/horas y calendario, costo integral/cobertura/consumo acumulado/reserva/margen y responsables de retención/apagado; confirma tamaño/AMI/parches, salida EIP de004 para diseño/plan (sin provisión implícita), contrato SSM y nuevo alcance de preparación/plan. No basta aprobar USD36.35 como "mensual" o asignar políticas por cuenta propia; no auto-reanudación.

### 23.6. Recuperación propuesta T-INFRA-010 — sin ejecución

1. Mantener estado crítico externo (Atlas/Redis/authority), no en root/docker volume de host. Pérdida de host no autoriza vaciar cuotas/revocación, restaurar seguridad antigua, recrear tabla o reabrir authority.
2. Registrar AMI/bootstrap/versión Docker y **misma referencia ECR por digest** para A/B. Referencia conocida sigue `926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api@sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda`, scan final/004/CI/config dependientes; no ejecutar desde bootstrap. Cambiar AMI/paquetes exige revisión del plan y nueva evidencia, no reemplazo silencioso por alias latest.
3. Ante fallo: diagnosticar/inventariar y pedir autorización de reemplazo Terraform **solo del host afectado**, plan revisado sin destroy/replacement de red/ECR/datos; no ejecutar replace/apply en este bloque. Conservar EIP como recurso independiente/reasociable en diseño futuro para no cambiar allowlist Atlas. Si cambia salida, aprobar nueva /32 y validar TLS/conectividad antes de admitir tráfico; no Atlas/0.
4. Esperar salida IPv4/SSM y bootstrap concluido, entregar secretos por mecanismo INFRA-10, verificar fail-closed/readiness y desplegar mismo digest vía004 autorizado. Reintegración/drenaje/targets/health son INFRA-6/12 y requieren evidencia real, no config falsa de API/target group aquí.
5. Sin Auto Scaling. Reemplazar una de solo2EC2 puede dejar temporalmente1host: **no HA/cero downtime garantizados**. Mantener2durante mantenimiento exige capacidad transitoria/costo nuevo autorizado o ventana aceptada; no usar create_before_destroy como promesa de capacidad/egreso/reserva disponible. Probar pérdida/reemplazo/retorno/capacidad/seguridad en incremento autorizado posterior.
6. Rollback real aún pendiente, sin digest anterior publicado conocido; recrear mismo host/artefacto no es prueba de rollback a versión previa. No restore de state viejo ni reaplicación de planes consumidos; custodia/backup/recovery Terraform025 pendientes.

### 23.7. Resultados, estados y detención

- **Archivos nuevos en repositorio: ninguno.** Solo modificados task/memoria para registrar análisis/bloqueo; fuente/HCL/policies/producto/tests/Docker/SDK/spec/plan003/004/gobierno intactos. Evidencia privada: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-cost-review-20261112-221004\` (hashes previos, capturas públicas/precios/cálculos y revisión Git), no logs cloud ni custodia/retención duradera garantizadas.
- **fmt/fmt-check/validate: NO EJECUTADOS** en este bloque por detención anterior a HCL; éxitos anteriores conservados como historia, no verificación actual. **Plan: NO EJECUTADO**; add/change/destroy **NO DISPONIBLE**, no0/0/0 ni número de altas inventado. Resources conceptuales si se retoma: role/profile/policies y2hosts/2roots, EIP solo propuesta; el número de recursos Terraform depende del diseño final, no es plan acreditado.
- **NETWORK14/ECR2 no-op: NO REVALIDADO**, configuración intacta por hash, no inventario AWS nuevo. En futuro plan revisado deben permanecer no-op, sin destroy/replacement inesperados. State/backups/planes existentes conservados por hash; nunca versionados, no plan nuevo ni credenciales guardadas. Lock/ejemplo permanecen versionables.
- **T-INFRA-005 PARCIAL / aceptación pendiente**; sin role/profile/SSM implementados. **T-INFRA-009 y010 PARCIAL / ANÁLISIS PREVIO, BUILD Y PLAN BLOQUEADOS POR COSTO**; no se declaran ejecutados compute/provisión/recovery. **T-INFRA-008 conserva PARCIAL** y evidencia informada de§22, scanning final/rollback/aceptación pendientes. **T-INFRA-025 conserva PARCIAL / aceptación pendiente**, sin validación fresca/aceptación de state.
- **GATE-I-C PENDIENTE / NO SATISFECHO**: no2EC2reales/digest A/B/capacidad/recuperación probados, dependencias I-A/B abiertas. **GATE-I-H/I-I e I-A/I-B igualmente pendientes**, nueve gates abiertos;11tareas parciales001–010/025,19pendientes011–024/026–030. Sin cierre002 GATE-G/H/AUTH-027 integral ni producción/004.
- Revisión local final `git diff --check` y `git status` registrada al cierre; esperado exclusivamente task/memoria modificados, **0staged**, branch/HEAD25daa836 intactos. **Sin stage/commit/push, AWS CLI/IAM mutado, Docker o Terraform.** Detener aquí; ningún nuevo plan/lectura scanning/IAM/apply/cleanup/siguiente incremento automático.

Referencias técnicas consultadas: [AL2023/SSM AMI y política de actualización](https://docs.aws.amazon.com/linux/al2023/ug/ec2.html), [contenido público AmazonSSMManagedInstanceCore](https://docs.aws.amazon.com/aws-managed-policy/latest/reference/AmazonSSMManagedInstanceCore.html). Su lectura no prueba agente/AMI/permisos efectivos ni acepta decisiones pendientes.

## 24. INFRA-5 — ventana48h aprobada, HCL validado y plan parcial bloqueado por IAM

### 24.1. Autorización y baseline conservada

**2026-11-12**, solicitud **APROBACIÓN DE VENTANA TEMPORAL + BUILD/PLAN / NO APPLY**. Aprueba **máximo inicial48h** de compute para evaluación, no infraestructura permanente; **guardrail USD10 acumulados de la ventana** sin nueva aprobación humana y **USD50 AGREGADOS de proyecto**, reserva objetivo50 sobre crédito100 informado. Permite ahora definir2t3a.small/AL2023x86_64/2gp3cifrados10GiB/2EIP para PLAN, role/profile/SSM/pull/bootstrap/recovery y candidatas IAM, fmt/check/validate y un intento de plan; no recursos AWS, apply/destroy/import, IAM remoto o Git stage/commit/push. Una lectura scanning opcional, no polling. El bloqueo presupuestario de preparación§23 se supera por esta autorización explícita, **no se borra su historia** ni se autoriza gasto/aceptación integral.

Precheck real **feature/infrastructure**, HEAD **25daa836c0571d4c0c02486e49ce435a108f0558**, únicamente task/memoria previamente modificados por§23 y **0staged**; sin archivos inesperados. Se preservan esos cambios. Herramientas existentes fuera de PATH heredado, usadas por ruta absoluta sin instalar/cambiar PATH persistente: Terraform **1.17.1/windows_amd64**, `-chdir=infra version -json` confirma AWS **6.73.0**. Introspección local `providers schema -json` confirma primary_network_interface vigente (network_interface de instancia deprecado), sin nuevos providers/init/upgrade. Un error local inicial por Get-FileHash no disponible se resuelve usando SHA256 .NET antes de editar; no fue intento cloud o plan.

### 24.2. Archivos y diseño definido — no recursos creados

**Nuevos5:** [compute.tf](../../infra/compute.tf), [iam-runtime.tf](../../infra/iam-runtime.tf), [compute-host.sh](../../infra/bootstrap/compute-host.sh), [terraform-local-compute-policy.json](../../infra/bootstrap/terraform-local-compute-policy.json), [terraform-local-iam-runtime-policy.json](../../infra/bootstrap/terraform-local-iam-runtime-policy.json). **Modificados4:** variables.tf/outputs.tf y task/memoria; estos dos Markdown ya estaban pendientes. Networking/security/registry/provider/versiones/lock/ejemplo/main, políticas anteriores, SDK/Dockerfile/producto/tests/spec/plan/004/gobierno intactos.

- **Dos EC2 definidas**, clavesa/b en subnets existentes/use1-az1/use1-az2; instance type **t3a.small fijo**, créditoCPU **standard**, no Unlimited/surplus implícito, sin degradar hashing; capacidad/latencia/créditos necesitan prueba posterior. Cada root **gp3 10GiB, encrypted=true,3000IOPS/125MiB/s, delete_on_termination=true**, tags de volumen al RunInstances. No datos críticos locales, snapshots/KMS custom, monitoring detallado o Auto Scaling.
- **Dos ENI persistentes**, solo SG backend existente (TCP8080 desde SG ALB), **dos EIP domain=vpc y dos asociaciones EIP→ENI**, allow_reassociation=false. EC2 usa primary_network_interface precreada: no asignación automática de IPv4 ni cambio a map_public_ip_on_launch=false. Depende de ambas asociaciones/ruta/tabla/IAM inline antes de arrancar; sin ciclo que requiera instancia para obtener salida. ENI conserva IP privada/EIP tras reemplazo de host; interfaz/raíz no son estado crítico de Authentication. Sin NAT/endpoints/ALB/targets/SSH/keypair o ingress nuevo. Red/Atlas/SSM funcionales no probados; /32Atlas posterior necesita aprobación.
- **IMDSv2 obligatorio/hoplimit1**, metadata tags disabled; credenciales temporales para host, no access keys ni credenciales persistentes GitHub. Hop1 impide asumir acceso directo desde contenedor a IMDS; entrega runtime futura debe respetar contrato INFRA-10, no se relaja por conveniencia.
- **AMI:** data SSM público **/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64**, data aws_ami verifica owner Amazon, nombre estándar AL2023, arquitectura x86_64/HVM/rootEBS/available. `compute_ami_id` opcional permite fijar ID verificado para reproducir; null resuelve alias mantenible. **AMI concreta NO RESUELTA por AccessDenied**, no se hardcodea una histórica. Alias/kernel-default cambia: registrar/fijar AMI/version/kernel antes de apply/reproducción, revisar tamaño snapshot mínimo10GiB y parches; la configuración local no demuestra esos atributos reales.
- **Bootstrap host-only**, archivo común SHA trazable: instala únicamente Docker/SSM desde repos bloqueados al release AL2023 de AMI (sin upgrade general/latest), timeout300s/intento,3intentos con pausa15s; fallo explícito. Habilita/verifica servicios Docker/SSM, prepara /opt/enver protegido y marcador/registro de versiones OS/RPM local. Log driver local Docker10m×3 por contenedor futuro, no CloudWatch/grant extra. **No docker login/pull/run, unidad/API/health ficticio, digest o secreto en user_data**; non-root de aplicación se conserva para deployment futuro. AMI/release/paquetes/soporte deben conservarse y verificarse para reproducción: dnf sin versiones RPM explícitas no acredita bit-for-bit ni disponibilidad eterna del repositorio. `bash -n` local exit0, **no script/boot/servicios ejecutados**.
- **Ajuste local posterior al plan fallido:** user_data normaliza CRLF→LF para que autocrlf de Windows no rompa el shebang Linux. Se repiten fmt/check/validate localmente; no nuevo plan ni cambio de candidatas IAM. La salida parcial anterior no es aprobación del HCL final ni autorización de apply.
- **Inputs/outputs:** compute_plan_approved defaultfalse, usado true solo en entorno temporal del plan autorizado; precondiciones limitan baseline proyecto/ambiente/cuenta/región/Zone IDs. No flag equivale a autorización apply, timer48h o capAWS. Outputs solo AMI/IDs/AZ/IPs/role propuestos, no tokens/secretos o health; no contrato de aplicación/digest escrito por Terraform para competir con004.

### 24.3. IAM runtime y candidatas del operador

**Role/profile HCL:** **enver-dev-api-runtime**, path/, trust **ec2.amazonaws.com → sts:AssumeRole**, sin role asumible por GitHub/usuario o access keys. **Dos inline policies separadas**, no managed attachment:

| Runtime | Permisos definidos | Recurso/límites |
| --- | --- | --- |
| enver-dev-ssm-session | **ssm:UpdateInstanceInformation**, **ssmmessages:CreateControlChannel/CreateDataChannel/OpenControlChannel/OpenDataChannel** | Resource=* para registro/canales, RequestedRegion=us-east-1; no lectura parámetros/documentos/inventario, ec2messages o administración global |
| enver-dev-ecr-pull | **ecr:GetAuthorizationToken**, **BatchCheckLayerAvailability/GetDownloadUrlForLayer/BatchGetImage** | Token Resource=* por acción sin ARN con cuenta/región;3acciones solo ARN repository enver-dev-api/región. Sin PutImage/CreateRepository/DeleteRepository/PutLifecyclePolicy |

**AmazonSSMManagedInstanceCore evaluada y NO utilizada**, no ARN attachment ni AttachRolePolicy/DetachRolePolicy en candidatas por no requerirse; evita GetParameter/GetParameters globales del managed§23. SSM reducido es definición revisable para agente moderno/ssmmessages, **no compatibilidad/registro/sesiones verificados**; probar antes de aceptación y detener/revisar si requiere otra acción. Permisos humanos Start/Resume/TerminateSession no se conceden al host ni se amplía operador por inferencia. Ningún grant CloudWatch/Secrets/KMS/DynamoDB runtime: nombres/contratos INFRA-10/11 pendientes, no bootstrap de authority o reapertura de confianza.

**Candidatas locales, NO aplicadas/adjuntas:** compute **9Allow/22acciones distintas/5130caracteres compactos**; IAM **4Allow/21acciones/1742caracteres compactos**, parse JSON válido/sin acciones wildcard. Compute excede límite inline agregado de usuario2048 si se intentara usar así; revisión/asignación humana customer-managed separada y cuotas efectivas pendientes, no se inventa attachment/cupo libre. Resource=* solo lecturas EC2 y necesidades runtime justificadas, no ecr:*/ec2:*/iam:*, AdministratorAccess/PowerUserAccess, policy remota editada o credenciales.

- **Compute:** DescribeImages/Instances/InstanceStatus/InstanceAttribute/InstanceCreditSpecifications/InstanceTypes/Volumes/VolumesModifications/Addresses/NetworkInterfaces/Tags en región/user/cuenta; **ssm:GetParameter solo parámetro público exacto** con ARN sin segmento account como recurso AWS público. RunInstances multirrecurso: subnets/SGbackend exactos del baseline, AMIregional, ENI tagsENVER, instancia t3a.small/IMDSv2/tags, root10GiBgp3cifrado/tags. CreateNetworkInterface/AllocateAddress y lifecycle Terminate/ModifyInstanceCreditSpecification/DeleteNetworkInterface/Associate/Disassociate/ReleaseAddress limitados por cuenta/región/tags/Name. CreateTags solo create-actions previstas y4tagkeys. **No CreateVolume/DeleteVolume separados**, root forma parte RunInstances/Terminate; no SG/VPC/ruta/keypair/AMIadministración. IAM no impone cardinalidad2 ni48h/costo, AMIregional permitida no es proof de ownerOS; HCL filtra Amazon, cambiar permisos/diseño requiere revisión. Estas son propuestas de mínimo alcance, **no simulación IAM/prueba de apply/destroy**; condiciones multi-recurso y rutas provider pendientes de validación humana/cloud.
- **IAM:** lifecycle exacto **role/enver-dev-api-runtime** y **instance-profile/enver-dev-api-runtime**: Create/Get/Delete/Tag/Untag y lecturas concretas de tags/inline/adjuntos/profiles requeridas por provider; inline **PutRolePolicy/GetRolePolicy/DeleteRolePolicy**, Add/RemoveRoleToInstanceProfile. No managedpolicy creation/attachments, ListRoles global, UpdateAssumeRolePolicy o permisos no ejercidos por diseño actual. Fuente provider v6.73.0 role.go confirma ListRolePolicies/ListAttachedRolePolicies incluso con inline separada; se revisaron profile/inline/EIP/schema locales y fuentes públicas. Revisión de mutations/cambios futuros no se acepta por estar Create/Get autorizados.
- **iam:PassRole solo `arn:aws:iam::926925605666:role/enver-dev-api-runtime`**, **iam:PassedToService=ec2.amazonaws.com**, user/cuenta guardados; nunca arbitrario. **PutRolePolicy permite modificar contenido inline de ese role**: limitar ARN no es permissions boundary ni garantía contra escalada al combinar create-policy/passrole/compute; hace falta revisión/custodia del operador antes de asignar. No boundary adicional inventado/creado ni afirmación de aislamiento efectivo de identidades por tener JSON separados.

### 24.4. Validación local y único plan — resultado real

| Comando/acción | Resultado observado |
| --- | --- |
| Terraform version/schema local | **1.17.1/windows_amd64**, lock seleccionado **AWS6.73.0**; sin actualización |
| terraform fmt -recursive infra | **exit0**, formatea compute.tf/outputs.tf |
| terraform fmt -check -recursive infra | **exit0** |
| terraform -chdir=infra validate -no-color | **exit0**, Success! The configuration is valid. |
| JSON parse + revisión acciones | **válidos**,9/22 y4/21, sin acciones wildcard |
| bash -n infra/bootstrap/compute-host.sh | **exit0**, no ejecución del bootstrap |
| Único terraform plan -detailed-exitcode | **exit1**, AccessDeniedException SSM; salida **PARCIAL10add/0change/0destroy**, no plan aceptado/guardado/aplicado |

Plan **2026-11-12T22:58:52.2149764Z →22:59:20.6941352Z**. Comando real `terraform -chdir=infra plan -detailed-exitcode -input=false -lock-timeout=30s -parallelism=1 -no-color`. Entorno de proceso restaurado después: **AWS_PROFILE=enver-infra**, regiónus-east-1, AWS_MAX_ATTEMPTS=1, metadata fallbackdisabled y inputs aprobados project=enver/environment=dev/account926925605666/vpc10.20.0.0/16/newbits8/network_design_approved=true/**compute_plan_approved=true**/common_tags={}; compute_ami_id defaultnull. Sin tfvars real nuevo/credenciales impresas, init o cambios provider; guard allowed_account_ids existente conservado. No STS CLI/inventario IAM independiente.

**Error exacto:** data.aws_ssm_parameter.al2023[0], fase **lectura AMI en plan**, **SSM:GetParameter / iam action ssm:GetParameter**, HTTP400/**AccessDeniedException**, principal **arn:aws:iam::926925605666:user/enver-terraform-local**, recurso **arn:aws:ssm:us-east-1::parameter/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64**, razón **no identity-based policy allows the ssm:GetParameter action**, RequestID **67b7699d-db00-4104-b164-a856c98ed03b**. No significa AMI ausente ni autoriza bypass/pin de ID adivinado. **La acción/recurso ya estaban en candidata compute antes del intento**; no ampliación tras fallo. Un plan/invocación, no reintento del agente; IAM intacto, no otras credenciales. La duración Still reading10s no se presenta como múltiples intentos manuales ni inventario de requests internos SDK.

**Inventario de altas efectivamente visibles en plan parcial:**

| Dirección | Cantidad |
| --- | --- |
| aws_iam_role.api_runtime | 1 |
| aws_iam_instance_profile.api_runtime | 1 |
| aws_iam_role_policy.api_ssm_session / api_ecr_pull | 2 |
| aws_network_interface.api[a,b] | 2 |
| aws_eip.api[a,b] | 2 |
| aws_eip_association.api[a,b] | 2 |
| **Subtotal parcial mostrado** | **10add /0change /0destroy** |
| aws_instance.api[a,b] | **2definidas HCL, NO incorporadas al plan parcial**, AMI bloqueada |

HCL define **12nuevos managed resources** (roots inline no recursos independientes); **no decir "plan12/0/0"**. Sin plan binario nuevo, no apply posible acreditado. No interpretar éxito devalidate como2EC2 listas. **NETWORK14/ECR2 refrescados**,16líneas reales deRefresh sin error en sus lecturas; dataAZ completa, **sin acciones de cambio/reemplazo/destrucción para existentes en salida parcial**. Eso conserva su configuración según evidencia parcial, **no certifica un plan completo16no-op con AMI resuelta**; la comprobación final queda para replan autorizado. No Network/ECRrecreados ni cambio remoto por este intento.

**Tras AccessDenied se detienen operaciones cloud:** sin modificar/adjuntar IAM, reintento de plan o alias alternativo. **Cero consultas de scanning en este turno** (la permitida era opcional); última evidencia linux/amd64 **IN_PROGRESS informada por responsable§22**, no status actual revalidado ni findings=0. T008 conserva pendientes. Solo revisión local/documentación después del fallo; sin AWSCLI/Docker/app/tests/push o servicios nuevos.

### 24.5. Costos48h, guardrails y alcance de la aprobación

Tarifas públicas de§23 consultadas el mismo2026-11-12 se reutilizan, sin Billing/saldo/crédito verificados ni Free Tier: t3a.small0.0188/h, gp3 0.08/GB-mes, IPv4in-use/idle0.005/h; ALB0.0225/h, LCU0.008/LCU-h. **48h máximo aprobado, no730h permanentes**. EBS se prorratea aquí por noviembre30días/720h, distinto de proyección730h de referencia; tiempos reales/redondeos/impuestos/capacidadretenida pueden cambiar factura.

| Partida | Cálculo48h | USD |
| --- | --- | --- |
| 2EC2t3a.small | 2×0.0188×48 | **1.8048** |
| 2rootgp310GiB | 2×10×0.08×48/720 | **0.106667** |
| 2EIP/IPv4 | 2×0.005×48 | **0.4800** |
| **INFRA-5** | Sin transferencia ni dependencias adicionales | **2.391467≈2.39** |
| ALBfuturoINFRA-6 base | 0.0225×48 | **1.0800** |
| Mínimo2IPv4ALB | 2×0.005×48 | **0.4800**, puede haber másIPs |
| LCUvariable: escenario1LCUpromedio | 1×0.008×48 | **0.3840**; no mínimo obligatorio/cap garantizado |
| **ALB48h con escenario1LCU** | No recursosALB en HCL/plan actual | **1.9440**; base+IPsinLCU1.56 |
| **INFRA-5 +ALB escenario1LCU** | Sin otros servicios | **4.335467≈4.34** |
| Bolsa provisional otros servicios | **Supuesto presupuestario, NO cotización Redis/Atlas/etc.** | **2.00** |
| Contingencia provisional | Margen no asignado ni gasto autorizado | **1.00** |
| **Envolvente provisional** | Incluye bolsas anteriores | **7.335467≈7.34** |
| **Margen nominal guardrailUSD10** | 10−7.335467 | **2.664533≈2.66**; sin bolsas,5.664533 |

Proyección **730h solo referencia** INFRA-5 **USD36.348≈36.35**, no presupuesto mensual ni duración aprobada. Costo0directo adicional de ENI/IAM estándar no es factura de plataforma0; transferencia/egresoInternet/Atlas/inter-AZ, ECR/logs/retención/Secrets/DynamoDB/DNS/Redis/Atlas siguen por cotizar según uso. **1LCU es escenario, no reserva/mínimo ni techo**, carga puede aumentarlo; cantidadIPv4ALB puede crecer. Otras dependencias **no se dan cubiertas por bolsaUSD2**. ALB y todos esos servicios necesitan incrementos/aprobaciones aparte.

**Viabilidad provisional: sí hay margen razonable para continuar DISEÑO/PLAN de48h**, no prueba integral del proyecto≤50 ni autorización de gasto. Nominal50−7.335467=42.664533 antes de consumo previo/noincluido; no saldo consultado o crédito garantizado. Antes de solicitar apply: completar costos/consumo acumulados/cobertura/retención/reemplazo y responsables/calendario efectivo; si acumulado ventana amenazaUSD10 o proyectoUSD50/reserva50, **nueva aprobación humana antes**, no eludir excluyendo servicios futuros/retención. **Esta solicitud NO autoriza ningún apply aunque el cálculo esté debajo de10**.

No temporizador, AWSBudget, apagado automático o cap técnico creado. Contar ventana desde provisión autorizada futura, controlar reloj/retención y pedir acciones separadas de parada/limpieza; dejar EC2stop no elimina EBS/EIP/ALB/datos cobrando. Al llegar48h no se autoriza mantener compute permanente ni destruir automáticamente datos; autorización operativa posterior debe prever cierre/cargosresiduales/evidencia antes del plazo. No cleanup ahora.

### 24.6. Recuperación y aceptación pendientes

Runbook§23.6 conservado y concretado: **AMI ID resuelta y fijada** antes de reproducir, mismo archivo bootstrap/release/versiones y configuración externa autorizada; `user_data_replace_on_change=true` obliga revisar reemplazo si cambia. **ENI/EIP son recursos separados persistentes**: reemplazar solo EC2 conserva salida/privada, sin reabrir Atlas/0; si se reemplaza ENI/EIP, revisar plan/allowlist antes de tráfico. Sin create_before_destroy: ENI primaria no puede estar en2hosts simultáneos, reemplazo normalmente deja1temporalmente; mantener2durante mantenimiento requiere capacidad/costo/ventana adicionales autorizados. Sin AutoScaling/SLA/cero downtime prometido.

Ante fallo, diagnóstico y autorización específica →plan exacto de reemplazo host, no red/ECR/datos→bootstrap/SSM/salida verificados→secretos/configINFRA-10→mismo digest por004 manual tras CIexacto→readiness/drenaje/ALBINFRA-6/12 y prueba de retorno. **Nunca reiniciar cuotas/revocaciones/authority, reabrir confianza al boot o restaurar state viejo**. Referencia ECR de§22 se conserva para cuando corresponda, no escrita como autoarranque. Rollback a versión previa sigue sin digestanterior; reproducir host no lo acredita. No se ejecuta falla/reemplazo/recovery/app o prueba cloud ahora.

**T005 PARCIAL / definición local validada, aceptación pendiente**; runtimeIAMsecrets/authority/operadorSSM/004 y pruebas faltan. **T009 PARCIAL / definición validada, plan completo bloqueado IAM/AMI**, sin2EC2reales. **T010 PARCIAL / recovery documentado, prueba/aceptación pendientes**. **T008 PARCIAL** con publicación/digest/pull informados§22 y scanningfinal/rollback/aceptación pendientes. **T025 PARCIAL / validación local acreditada, aceptación pendiente**: plan fallido no acepta custodia/backup/recovery o dependencias. **GATE-I-C PENDIENTE / NO SATISFECHO**, no2hosts/digestA-B/capacidad/recuperación; **los9gates permanecen abiertos**,11tareasparciales/19pendientes. Sin cierres002GATE-G/H/AUTH-027integral o004/producción.

Evidencia privada local `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-build-plan-20261112-222009\` (hashes previos, esquema/versiones, fmt/check/validate, texto/error único plan/análisis y cálculo48h, revisión final), sin tokens/credenciales ni garantía de custodia duradera en temporal. State/backup/planesanteriores se preservan por hash, no state/plan nuevo versionado; lock/ejemplo versionables. `git diff --check`/status/hashes finales se registran al cierre,5nuevos+4modificados esperados,0staged/HEAD25daa836 intacto. **No recursosAWS/IAMmodificado/apply/destroy/import/Docker/Gitstage-commit-push. Detener para revisión humana de la candidata y denegación SSM; ninguna reanudación automática.**

## 25. INFRA-5 — AMI ID resuelta; DescribeImages denegado antes del replan

### 25.1. Alcance y resultados nuevos

**2026-11-12 — VALIDACIÓN CONTROLADA / NO APPLY.** El responsable informa haber agregado manualmente **ENVER-Terraform-AMI-Read**, solo `ssm:GetParameter` sobre el parámetro público exacto. Se autoriza comprobar identidad/AMI, validación local y replan, con detención ante cualquier AccessDenied o cambios network/ECR. No se enumeran políticas efectivas ni se atribuye al agente la modificación IAM. El éxito de GetParameter acredita esa operación, no todos los permisos del operador.

Precheck real: **feature/infrastructure**, HEAD **25daa836c0571d4c0c02486e49ce435a108f0558**, los mismos **5 archivos nuevos/4 modificados/0 staged** de §24. Se preservan cambios anteriores. AWS CLI usa perfil **enver-infra**, región **us-east-1**, un intento por operación (`AWS_MAX_ATTEMPTS=1`, sin credenciales alternativas); entorno temporal restaurado. Sin secretos o credenciales impresos.

| Operación real | Resultado |
| --- | --- |
| aws sts get-caller-identity --profile enver-infra | **exit0**, cuenta926925605666, **arn:aws:iam::926925605666:user/enver-terraform-local** |
| aws ssm get-parameter sobre alias público AL2023 | **exit0**, versión **202**, AMI ID **ami-096ef200a877f36af** |
| LastModifiedDate del parámetro | **2026-11-12T13:40:52.359000-06:00**, equivalente **19:40:52.359Z** |
| ec2 describe-images --image-ids ami-096ef200a877f36af --owners amazon | **exit254 / UnauthorizedOperation**, no atributos devueltos |
| terraform fmt -check -recursive infra | **exit0**, sin formatear/escribir HCL |
| terraform -chdir=infra validate -no-color | **exit0**, Success! The configuration is valid. |
| terraform -chdir=infra version -json | **1.17.1/windows_amd64**, provider seleccionado **6.73.0** |
| Replan / scanning opcional | **No ejecutados**, detención por denegación previa; sin polling |

Nombre exacto leído: **/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-default-x86_64**. Se registra **solo ID resuelta, no AMI completamente validada**: AL2023/x86_64/available/HVM/EBS/Amazon siguen como filtros del HCL y significado del alias, **no atributos observados de esa imagen**. Compatibilidad linux/amd64 esperada por diseño, no confirmada por DescribeImages. No AMI fijada en input ni HCL modificado, no bypass del filtro.

### 25.2. Denegación y mínimo propuesto — no modificación IAM

**2026-11-12T23:12:55.1306021Z–23:12:58.8387069Z**, **UnauthorizedOperation**, CLI **exit254**, acción **ec2:DescribeImages**, principal **arn:aws:iam::926925605666:user/enver-terraform-local**; razón exacta **no identity-based policy allows the ec2:DescribeImages action**. Fase: **validación AMI por AWS CLI, ANTES de terraform plan**. Recurso solicitado: **ami-096ef200a877f36af**, owners=amazon, regiónus-east-1. Recurso Terraform relacionado: **data.aws_ami.api**, no ejecutado en este turno. No error de RunInstances o prueba de permisos de escritura. No se usa el enlace/GetRequestAuthorizationDetails del error, no reintento o credenciales alternativas.

**Mínimo para revisión humana:** únicamente `ec2:DescribeImages`, **Resource="*"** (la acción de lectura no admite restricción a ARN de imagen), condición **aws:RequestedRegion=us-east-1**, y límites de usuario/cuenta del baseline cuando corresponda. No `ec2:*`, RunInstances o IAM admin. La consulta por ID/owner no reemplaza un scope IAM admitido. Esta acción **ya existe** en ReadComputeInApprovedRegion de la candidata compute; **no se propone adjuntar toda la candidata de escritura para desbloquear una lectura**. Sin crear/editar JSON o policy remota. Habilitarla manualmente y autorizar nueva validación/replan son decisiones futuras del responsable, no reanudación automática.

### 25.3. Inventario y seguridad — diseño conservado, sin plan nuevo

**Terraform plan NO ejecutado**, exit code y **add/change/destroy NO DISPONIBLES**. El10/0/0 de §24 es histórico/parcial, no resultado actual ni plan completo. **Network14/ECR2 no-op NO revalidados** en este turno; sin refresh actual, no se afirma0/0/0 o ausencia de drift cloud. No nuevos recursos AWS ni inventario completo de plan para aceptar.

Lectura del HCL existente conserva **12 recursos nuevos definidos**, no planeados ahora: role1/profile1/inline policies2/ENI2/EIP2/asociaciones2/EC2 2. EC2 t3a.small/CPUstandard, use1-az1/use1-az2, gp3 cifrado10GiB por host; ENI/EIP persistentes y backend SG existente, map_public_ip_on_launch=false, 8080 solo SG ALB, sin SSH22 público/NAT. Atributos de AMI y capacidad/servicios requieren evidencia adicional.

Role/profile **enver-dev-api-runtime**, trust ec2.amazonaws.com; SSM reducido UpdateInstanceInformation +4canales ssmmessages, ECR token +BatchCheckLayerAvailability/GetDownloadUrlForLayer/BatchGetImage limitadas al repositorio para las3acciones. Sin push/admin o managed SSM attachment. Docker/SSM bootstrap definido sin tokens/secretos/API auto-start. Son definiciones locales, no permisos efectivos/SSM funcional/health acreditados. `iam:PassRole` de candidata confirmado **solo arn:aws:iam::926925605666:role/enver-dev-api-runtime**, **iam:PassedToService=ec2.amazonaws.com**; no PassRole arbitrario.

**Revisión nueva de candidatas sin modificarlas/aplicarlas:**

| Archivo | Bytes actuales / caracteres compactos | Statements / acciones distintas | Wildcards y scope |
| --- | --- | --- | --- |
| terraform-local-compute-policy.json | **6838 /5130** | **9Allow /22** | **0 acciones wildcard**; Resource=* en lecturasEC2, ARNs imagen/instance/volume/ENI/EIP con sufijos wildcard para provisión revisable; subnets/SGbackend exactos, tags/cuenta/user/región y SSMpublico exacto |
| terraform-local-iam-runtime-policy.json | **2405 /1742** | **4Allow /21** | **0 acciones wildcard, 0 recursos wildcard**; solo role/profile exactos, PassRole condicionado aEC2 |

Acciones compute: `ec2:AllocateAddress`, `AssociateAddress`, `CreateNetworkInterface`, `CreateTags`, `DeleteNetworkInterface`, `DescribeAddresses`, `DescribeImages`, `DescribeInstanceAttribute`, `DescribeInstanceCreditSpecifications`, `DescribeInstances`, `DescribeInstanceStatus`, `DescribeInstanceTypes`, `DescribeNetworkInterfaces`, `DescribeTags`, `DescribeVolumes`, `DescribeVolumesModifications`, `DisassociateAddress`, `ModifyInstanceCreditSpecification`, `ReleaseAddress`, `RunInstances`, `TerminateInstances`; `ssm:GetParameter`. Prefijo ec2 aplica a cada acción EC2 abreviada. Acciones IAM: `AddRoleToInstanceProfile`, `CreateInstanceProfile`, `CreateRole`, `DeleteInstanceProfile`, `DeleteRole`, `DeleteRolePolicy`, `GetInstanceProfile`, `GetRole`, `GetRolePolicy`, `ListAttachedRolePolicies`, `ListInstanceProfilesForRole`, `ListInstanceProfileTags`, `ListRolePolicies`, `ListRoleTags`, `PassRole`, `PutRolePolicy`, `RemoveRoleFromInstanceProfile`, `TagInstanceProfile`, `TagRole`, `UntagInstanceProfile`, `UntagRole`, todas con prefijo `iam:`.

Sin AdministratorAccess/PowerUserAccess, acciones `ssm:*`/`ec2:*`/`iam:*`, ECR push/provisioning o PassRole arbitrario detectados. **No confundir cero acciones wildcard con cero recursos wildcard en compute.** El tamaño compacto compute excede inline user2048; cuotas/asignación siguen para revisión humana. ARN exacto con PutRolePolicy/CreateRole/PassRole **no es permissions boundary** ni elimina escalada de contenido de policy: conservar advertencia §24.3 y custodia separada. JSON válido/revisión local no simulan IAM ni prueban permisos de apply.

### 25.4. Costos, estados y detención

Sin cambios HCL/recursos adicionales: se conserva ventana **máximo48h**, EC2 **USD1.8048**, EBS **0.106667**, IPv4 **0.48**, **INFRA-5≈2.3915**; futuroALB escenario48h **1.944** (LCU variable, no mínimo garantizado), conjunto **4.3355**. Con bolsaotros2+contingencia1: **envolvente7.3355 /margen nominal2.6645 contra10**. USD50 **AGREGADOS**, no mensual; consumo acumulado/servicios/cobertura/reserva no consultados. Margen provisional no certifica guardrail integral ni autoriza apply/gasto. Sin nuevo presupuesto, cap técnico, timer o limpieza; cargos residuales al detener EC2 no desaparecen.

**T005 PARCIAL / aceptación pendiente**, solo lecturaAMI parcialmente desbloqueada; runtime/separación integral sin aceptar. **T009 PARCIAL / definición validada, plan completo bloqueado por IAM/validaciónAMI**; ID conocida, atributos pendientes. **T010 PARCIAL / recovery documentado, prueba y aceptación pendientes**; nada reemplazado. **T008 PARCIAL**, scanfinal/rollback/aceptación pendientes, **cero consultas de scanning**: último IN_PROGRESS informado§22, no estado actual/findings=0. **T025 PARCIAL / validación local acreditada, aceptación pendiente**, sin plan completo/custodia/recovery aceptados. **GATE-I-C PENDIENTE / NO SATISFECHO**, sin dos EC2 reales; los9gates abiertos y11tareasparciales/19pendientes, sin cierres002/004/AUTH-027integral.

Evidencia privada local: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-ami-replan-20261112-231243\`, STS/SSM/denegación y tiempos, fmt-check/validate/version, revisión candidatas y hashes antes/después. Temporal no acredita custodia duradera. Solo **task.md y memoria.md editados en este turno**; los5nuevos/4modificados anteriores permanecen sin stage, HEAD intacto. Revisión final local verifica diff-check, exclusiones, hashes de fuentes/HCL/candidatas/state/backup/planes existentes; sin aplicar/restaurar state o plan histórico. **No apply/destroy/import, IAM remoto modificado, nuevos recursos, Docker, replan/scanning tras denegación o Git stage/commit/push. DETENER.**

## 26. INFRA-5 — plan completo12/0/0; red14/ECR2no-op; sin apply

### 26.1. Autorización, fuentes y validación

**2026-11-12 — REPLAN COMPUTE DESPUÉS DE VALIDAR AMI / VALIDACIÓN CONTROLADA / NO APPLY.** Responsable aporta validación EC2 real de **ami-096ef200a877f36af**: **Amazon Linux2023 2023.12.20261112.0, Linux/UNIX, x86_64, available, ImageOwnerAlias=amazon, Public=true, IMDSv2.0, HVM, rootEBS, kernel6.18**. Se conserva como **evidencia comunicada por el responsable**, no se deduce exclusivamente del alias ni se presenta como nueva ejecución CLI independiente. El permiso DescribeImages se informa habilitado; el plan verifica realmente la lectura de esa AMI por `data.aws_ami.api` con filtros ownerAmazon/nombreAL2023/x86_64/HVM/EBS/available. Campos detallados no impresos por plan (release/kernel/Public/IMDS) mantienen su fuente informada. Historial de denegaciones §§24–25 intacto, bloqueo operativo de plan ahora superado.

Precheck real: **feature/infrastructure**, HEAD **25daa836c0571d4c0c02486e49ce435a108f0558**, mismos5nuevos/4modificados/0staged; sin archivos inesperados. HCL/candidatas/bootstrap no se editan. Herramientas existentes por ruta absoluta, sin instalación/init/upgrade: **Terraform1.17.1/windows_amd64 / AWS6.73.0**. `aws sts get-caller-identity --profile enver-infra` (regiónus-east-1) **exit0**, cuenta **926925605666**, ARN **arn:aws:iam::926925605666:user/enver-terraform-local**; no credenciales expuestas.

`terraform fmt -check -recursive infra` **exit0**; `terraform -chdir=infra validate -no-color` **exit0 / Success! The configuration is valid.** No fmt de escritura. Una ejecución de plan, **2026-11-12T23:36:33.4510140Z–23:36:55.7373288Z**:

```text
terraform -chdir=infra plan -detailed-exitcode -input=false -lock-timeout=30s -parallelism=1 -no-color
Plan: 12 to add, 0 to change, 0 to destroy.
exit code: 2
```

**Exit2 significa plan exitoso con cambios propuestos**, no error ni recursos creados. Sin AccessDenied, replacement o destroy. No archivo binario nuevo guardado, no apply. Logs/resúmenes privados locales, no plan enGit. Un plan exitoso de recursos nuevos **no ejercita permisos IAM/EC2 de creación, PassRole o colisiones con recursos no gestionados**, ni asegura capacidad disponible/costo/futura autorización de apply.

Inputs anteriores conservados mediante entorno temporal restaurado: perfil **enver-infra**, región **us-east-1**, project=enver/environment=dev/account926925605666/vpc10.20.0.0/16/newbits8/common_tags={}/network_design_approved=true/**compute_plan_approved=true**; `compute_ami_id=null` por defecto, mismo comportamiento mantenible anterior, no pin silencioso de input. AWS_MAX_ATTEMPTS=1, metadata credential fallback disabled, sin credenciales alternativas/endpoint custom. DataSSM y dataAMI completan realmente y ambas EC2 usan **ami-096ef200a877f36af**, coincidente con evidencia concreta. Antes de provisión/reproducción, revisar autorización/AMI-ID fijada o plan fresco controlado para que no rote el alias inadvertidamente; este plan no autoriza su aplicación futura.

### 26.2. Inventario REAL de altas y existentes

| Dirección exacta en plan | Alta |
| --- | --- |
| aws_iam_role.api_runtime | 1 |
| aws_iam_instance_profile.api_runtime | 1 |
| aws_iam_role_policy.api_ecr_pull | 1 |
| aws_iam_role_policy.api_ssm_session | 1 |
| aws_network_interface.api["a"] | 1 |
| aws_network_interface.api["b"] | 1 |
| aws_eip.api["a"] | 1 |
| aws_eip.api["b"] | 1 |
| aws_eip_association.api["a"] | 1 |
| aws_eip_association.api["b"] | 1 |
| aws_instance.api["a"] | 1 |
| aws_instance.api["b"] | 1 |
| **Total completo** | **12add /0change /0destroy** |

Dos roots son bloques de instancia, no recursos adicionales independientes. No ALB/targetgroup/NAT/Redis/DynamoDB/Secrets/CloudWatchstack u otro recurso facturable fuera de baseline. **NETWORK14 y ECR2 no-op confirmados para este plan completo**: las16direcciones managed del state fueron refrescadas, cotejadas con las12altas nuevas y sin cambios/drift/replacement/destroy en salida. Incluye VPC1/IGW1/subnets2/routeTable1/route1/asociaciones2/SG2/reglas4=14 y repository/lifecycle=2. No nueva auditoría independiente de toda la cuenta ni garantía de ausencia futura de drift.

| EC2 en plan | AMI /tipo | AZ de esta cuenta /ZoneID | Root |
| --- | --- | --- | --- |
| api["a"] | ami-096ef200a877f36af /t3a.small | **us-east-1a /use1-az1** | **10GiB gp3 cifrado**,3000IOPS/125MiB/s |
| api["b"] | ami-096ef200a877f36af /t3a.small | **us-east-1b /use1-az2** | **10GiB gp3 cifrado**,3000IOPS/125MiB/s |

Arquitectura **x86_64**, compatible con linux/amd64, acreditada por evidencia AMI y filtro del data source exitoso. CPUcredits **standard**, no Unlimited/surplus implícito; rendimiento/capacidad/hashing no probados. Dos ENI persistentes se asignan a subnets existentes/solo **sg-070d06cfebe866427**. Dos EIPvpc y asociaciones `allow_reassociation=false`, sin NAT; EIP→ENI antes del boot, conservar IP al reemplazar EC2 según diseño. Outputs ZoneIDs y plan AZs se cotejan con metadata local de subnets, sin asumir mapeo universal de letras entre cuentas.

`map_public_ip_on_launch=false` se conserva; IP estable viene de EIP, no cambia subnet. Backend8080 solo desde **SG ALB sg-0dacf0df5da30b4f7**; sin CIDR público de backend ni ingreso22/keypair. `associate_public_ip_address`/key_name/atributos derivados aparecen known-after-apply: no se inventan valores observados del host; configuración usa ENI preexistente enRunInstances, no asignación automática o keypair definidos. IMDSv2 required/hop1/tagsexposeddisabled. Egreso/Atlas32/SSM/network readiness reales siguen pendientes, no exposición pública del8080 por tener EIP ni afirmación de app accesible.

### 26.3. Runtime y candidatas — definición no aplicada

Plan contiene role **enver-dev-api-runtime**, profile homónimo, trust **ec2.amazonaws.com→sts:AssumeRole**, dos inline policies. **SSM:** UpdateInstanceInformation +CreateControlChannel/CreateDataChannel/OpenControlChannel/OpenDataChannel de ssmmessages, Resource=* por necesidad/región; sin AmazonSSMManagedInstanceCore attachment. **ECRpull:** GetAuthorizationToken Resource=* con cuenta/región y BatchCheckLayerAvailability/GetDownloadUrlForLayer/BatchGetImage solo ARNrepositoryenver-dev-api. Sin ECRpush/provisioning, IAMadmin/EC2admin, secretos o accesskeys en host. Ninguna sesión/registro SSM o permiso runtime efectivo acreditado por el plan.

BootstrapDocker/SSM aparece en ambas EC2; sin ECRlogin/pull/run, URI/conexiónMongo/Redis, JWT/password/token/accesskeys o autoarranqueAPI. API sigue esperando dependencias/secretos/readiness/004manual; no health ficticio. `user_data_replace_on_change=true`, CRLF→LF conservado, logsDockerlocales acotados. Inspección de indicadores no equivale a auditoría exhaustiva de secretos. Recovery§24 conserva AMI/bootstrap/config externa/mismo digest posterior/estado crítico externo; sin AutoScaling/reemplazo real, dos hosts durante mantenimiento no garantizados. Retención ENI/EIP y pérdida temporal de capacidad requieren prueba/autorización de ejecución posterior.

| Candidata intacta | Tamaño bytes /compactchars | Actions /wildcards /scope |
| --- | --- | --- |
| terraform-local-compute-policy.json | **6838 /5130** | **22acciones concretas /9Allow**,0acciones wildcard; Resource=* enDescribeEC2, ARNwildcards para imagen/instancia/volumen/ENI/EIP, subnets/SG exactos, tags/user/cuenta/región, SSMpublico exacto |
| terraform-local-iam-runtime-policy.json | **2405 /1742** | **21acciones concretas /4Allow**,0acciones/recursos wildcard; role/profile exactos |

Listas exactas de acciones de§25.3 revalidadas localmente sin cambios, guardadas en policy-review.json privado; sin `ssm:*`/`ec2:*`/`iam:*`, AdministratorAccess/PowerUserAccess o ECRpush. **PassRole exclusivamente arn:aws:iam::926925605666:role/enver-dev-api-runtime y iam:PassedToService=ec2.amazonaws.com**. No managedpolicy attachments innecesarios. Recursos wildcard de compute no se presentan como cero wildcards globales. Compute5130compactchars excede inlineuser2048; cuotas/asignación humana y posible customer-managed siguen pendientes, no se concede por el agente. PutRolePolicy/CreateRole/PassRole limitados por nombre no son boundary/garantía contra escalada, mantener revisión humana de contenido. **Ninguna candidata aplicada, adjunta, modificada o simulada; permisos de mutación no validados por el plan.**

### 26.4. Única consulta scanning — ScanNotFoundException, no findings

Después del plan exitoso, **una sola llamada** `ecr describe-image-scan-findings`, repository **enver-dev-api**, perfil/regiónbaseline, `--no-paginate` para evitar solicitudes adicionales, `AWS_MAX_ATTEMPTS=1`; se piden solo status/conteos/fechas/ID. Digest registrado de§22: **sha256:0a0fceb5ab9e71e09db4a1ee7326dc59993d40266f144259f0d02bc6c9802cda**. Resultado **exit254 /ScanNotFoundException**: **Image scan does not exist for the image** con ese digest/repository/registry926925605666. **No AccessDenied**, no status/findings devueltos, no polling/reintento/StartImageScan ni lectura adicional de objetosOCI.

Ese digest es el registrado para despliegue; §22 distingue índice/manifiestoamd64/atestación sin dar digests hijos. **ScanNotFound del digest consultado no demuestra imagen inexistente, cero vulnerabilidades o inexistencia de scan del hijo linux/amd64**. No se inventa digest hijo ni se aplica al índice el IN_PROGRESS comunicado paralinux/amd64. Último estado del manifiesto linux/amd64 continúa **IN_PROGRESS histórico/informado, no revalidado**, resultado final y conteos **NO DISPONIBLES**. Se conserva evidencia de publicación/pull según responsable, sin nueva prueba de ambas operaciones. **Sin más operaciones cloud tras esta consulta.**

### 26.5. Presupuesto, estados y detención

Plan no añade tipos/capacidades facturables fuera de estimación previa. **48h máximo**, EC2 **USD1.8048**, gp3 **0.106667**, dosIPv4 **0.48**→**INFRA-5≈2.3915**; futuroALB **1.944** con escenario1LCUpromedio, conjunto **4.3355**, bolsaotros2+contingencia1→**envolvente7.3355 /margen nominal2.6645 contra guardrail10**. LCU/cantidadIPs/transferencia/retención/otrosservicios/consumo acumulado pueden variar; bolsaotros no cotización integral. **USD50 AGREGADOS**, reservaobjetivo50/crédito100informado, no mensual/FreeTier/Billingverificado. Guardrail respetado **solo por estimación provisional**; reevaluar acumulado/calendario/cobertura/responsables y partidas completas antes de pedir apply, sin cap técnico o timer creado. **Este resultado no autoriza apply/gasto, permanencia más48h o cleanup automático**.

**T005 PARCIAL / definición y plan runtime acreditados, aceptación pendiente**; **T009 PARCIAL / PLAN COMPLETO ACREDITADO, PROVISIÓN Y ACEPTACIÓN PENDIENTES**; **T010 PARCIAL / recovery documentado, prueba y aceptación pendientes**; **T008 PARCIAL / publicación-digest-pull según responsable, scanfinal/rollback/aceptación pendientes**, con consultaScanNotFound precisa; **T025 PARCIAL / VALIDACIÓN LOCAL Y PLAN ACREDITADOS, ACEPTACIÓN PENDIENTE**. Plan completo no acepta custodia/statebackup/recovery ni dependencias. **GATE-I-C PENDIENTE / NO SATISFECHO**, no dosEC2 reales/digestA-B/pruebas; todos9gates abiertos/11tareasparciales/19pendientes. Sin cambios de002GATE-G/H/AUTH-027integral/004/producción.

Evidencia local privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-complete-replan-20261112-233555\`: STS/tiempos/version/fmt-check/validate/plan.txt/plan-result.json/plan-analysis.json/policy-review.json/scanerror y revisiónhashes/Git. No planbinario nuevo, state/backup/planes anteriores no restaurados ni modificados; carpeta temporal no acredita custodia duradera. **Solo task/memoria editados en este turno**, mismos5nuevos/4modificados anteriores,0staged/HEAD25daa836 intacto. Revisión final local verifica hashes de HCL/candidatas/demás fuentes/state/backup/planes, exclusiones/links/diff-check. **No apply/destroy/import, recursosAWS creados, IAM remoto modificado, Docker o Git stage/commit/push. DETENER, sin siguiente incremento automático.**

## 27. INFRA-5 — revisión IAM y preparación de aplicación manual; NO AWS / NO APPLY

### 27.1. Evidencia nueva y alcance de validez

**2026-11-12 — PREPARAR IAM COMPUTE + IAM RUNTIME PARA APPLY / VALIDACIÓN CONTROLADA / NO APPLY.** Se revisan exactamente las dos candidatas existentes contra compute.tf/iam-runtime.tf, sin editarlas; se prepara guía para el responsable humano. Precheck real **feature/infrastructure**, HEAD **25daa836c0571d4c0c02486e49ce435a108f0558**, mismos5archivos nuevos/4modificados/0staged, sin inesperados. Solo task/memoria editados en este turno.

**Ambas válidas como JSON y candidatas de alcance acotado**, no certificación de mínimo privilegio integral, autorización efectiva AWS o capacidad de aplicar. Runtime HCL contiene exclusivamente SSMregistro/canales y ECRpull; no EC2admin/IAMadmin/ECRpush/DeleteRepository/PutImage/AdministratorAccess/PowerUserAccess ni managed attachment extra. Trust EC2, profile y policies inline reales del HCL conservados. Plan previo no prueba llamadas mutantes del provider; no simulación IAM/AccessAnalyzer, inventario IAM efectivo o prueba de permisos de apply en esta entrega.

| Candidata | Tamaño actual bytes /caracteres compactos | Allow /acciones distintas | Tipo/nombre recomendado |
| --- | --- | --- | --- |
| terraform-local-compute-policy.json | **6838 /5130** | **9 /22** | **Customer-managed /ENVER-Terraform-Compute** |
| terraform-local-iam-runtime-policy.json | **2405 /1742** | **4 /21** | **Customer-managed /ENVER-Terraform-IAM-Runtime** |

Compute5130compactchars no cabe en inlineuser2048 agregado; ambas caben individualmente en límite customer-managed6144 sin espacios. IAM1742 podría caber en inline solo si agregado/cuotas lo permiten, pero se recomienda customer-managed por separación, claridad, revisión/versionado/revocación independiente. No se ha verificado cuota libre de attachments o políticas de cuenta/otros grants/boundary/SCP; no declarar disponibilidad o permiso efectivo por tamaño. LímiteIAM excluye whitespace, distinto del tamaño de archivo con formato.

**SHA256 exactos antes de editar documentación:**

- Compute: **e05dd08a4e3505f8dc5e2922e51ee498b99c0feb07d9a1d52634f7e71ad937a0**.
- IAM: **b36c588317ba63ead7317ea4d1ff3de22599be74e5eb2cf83cbb2bb88b1ac7e9**.

### 27.2. Acciones, scope y límites de mínimo privilegio

**Compute,22acciones:** `ec2:DescribeImages`, `ec2:DescribeInstances`, `ec2:DescribeInstanceStatus`, `ec2:DescribeInstanceAttribute`, `ec2:DescribeInstanceCreditSpecifications`, `ec2:DescribeInstanceTypes`, `ec2:DescribeVolumes`, `ec2:DescribeVolumesModifications`, `ec2:DescribeAddresses`, `ec2:DescribeNetworkInterfaces`, `ec2:DescribeTags`, `ec2:RunInstances`, `ec2:TerminateInstances`, `ec2:CreateNetworkInterface`, `ec2:DeleteNetworkInterface`, `ec2:AllocateAddress`, `ec2:AssociateAddress`, `ec2:DisassociateAddress`, `ec2:ReleaseAddress`, `ec2:ModifyInstanceCreditSpecification`, `ec2:CreateTags`, `ssm:GetParameter`.

LecturasEC2 Resource=* por operaciones de consulta, limitadas a **us-east-1/926925605666/enver-terraform-local**; no lectura de secretosSSM, solo parámetro públicoAMIexacto. EscriturasEC2 limitadas por acción/recurso/cuenta/región/usuario/tags/Name: RunInstances multirrecurso con subnets y SGbackend exactos, instancia t3a.small/IMDSv2, raízgp3cifrada10GiB, ENIetiquetadas; Allocate/CreateENI y lifecycle/tagging acotados. EBSraíz se crea con RunInstances y elimina con Terminate/delete_on_termination, no CreateVolume/DeleteVolume o snapshots amplios. **Cero acciones wildcard**, sí Resource=* de lecturas y ARNs con sufijo wildcard para image/instance/volume/ENI/EIP. No autoriza NATGateway/ALB/AutoScaling/RequestSpot/CreateFleet/CreateImage/snapshots/adminSG por sus acciones; no statement nuevo para Spot. Los permisos efectivos resultan de todas las policies, no puede declararse prohibición global desde estas dos Allow.

**IAM,21acciones:** `iam:CreateRole`, `iam:GetRole`, `iam:DeleteRole`, `iam:TagRole`, `iam:UntagRole`, `iam:ListRoleTags`, `iam:ListRolePolicies`, `iam:GetRolePolicy`, `iam:PutRolePolicy`, `iam:DeleteRolePolicy`, `iam:ListAttachedRolePolicies`, `iam:ListInstanceProfilesForRole`, `iam:CreateInstanceProfile`, `iam:GetInstanceProfile`, `iam:DeleteInstanceProfile`, `iam:TagInstanceProfile`, `iam:UntagInstanceProfile`, `iam:ListInstanceProfileTags`, `iam:AddRoleToInstanceProfile`, `iam:RemoveRoleFromInstanceProfile`, `iam:PassRole`.

Solo ARNs exactos **role/enver-dev-api-runtime** e **instance-profile/enver-dev-api-runtime**, usuario/cuenta baseline, tags requeridas al crear. List/Get/Tag/Untag y Delete apoyan lectura/lifecycle de esos recursos del provider, no administradorIAM. HCL usa inline separadas: Put/Get/DeleteRolePolicy necesarios en esa estrategia, no CreatePolicy/AttachRolePolicy/DetachRolePolicy para runtime. **PassRole exclusivamente arn:aws:iam::926925605666:role/enver-dev-api-runtime**, condición **iam:PassedToService=ec2.amazonaws.com**; no role arbitrario ni grants para el operador humano de creación/adjunción de customer-managed. **Cero acciones y recursos wildcard en candidata IAM**.

**Permisos prohibidos directos no detectados** en candidatas/runtime. **Límites y riesgo residual explícitos antes de asignar:**

1. **CreateRole permite definir trust y PutRolePolicy permite contenido inline no restringido por estos JSON.** Un operador con PutRolePolicy+PassRole+EC2 podría ampliar el runtime y acceder a sus credenciales: **riesgo de escalada**, aunque el role sea exacto. La revisión del HCL actual solo confirma que hoy define SSM/pull. No hay boundary inmutable o validación de contenido de inline en estas candidatas. **No afirmar que el operador solo puede conceder SSM/pull**. El responsable debe revisar/aceptar expresamente este riesgo o autorizar un endurecimiento separado **antes de adjuntar**; no se inventa/aplica boundary ni se cambia arquitectura aquí.
2. ARNimagen `ami-*` no fija soloAMIactual/Amazon a nivelIAM; son filtrosHCL. Tags/Name no garantizan cardinalidad2, aislamiento absoluto o impedir reutilización de nombres. ModifyInstanceCreditSpecification permite modificar créditos del host acotado, sin imponer standard como boundaryIAM; HCL y plan sí usan standard. Tampoco estas policies imponen48h,USD10 o techo de factura.
3. Las11lecturasEC2 regionales son familias de inspección/lifecycle candidatas, no auditoría de que cada llamada sea imprescindible en todo camino provider. Compatibilidad exacta/condiciones multirrecurso y lifecycle requieren evidencia adicional; JSONválido no equivale a validación IAM de semántica o simulación exitosa. Sin ampliación preventiva por temor a AccessDenied; detener si aparece en un incremento autorizado.

### 27.3. Guía para aplicación EXCLUSIVAMENTE manual — no ejecutada

**Precondición:** responsable revisa los límites anteriores y decide aceptación o endurecimiento aparte; sin esa decisión, **detener antes de asignar**. Esta guía no afirma que la asignación haya ocurrido ni autoriza recursos/apply.

1. El responsable autorizado entra a consola **cuenta926925605666 →IAM**, con su identidad administrativa humana habitual y MFA donde aplique. No crear accesskeys ni ampliar permisos del usuario Terraform para que se autoadministre. Confirmar destino **usuario enver-terraform-local**, no role runtime, root o grupo genérico.
2. Revisar cuotas de customer-managed policies/attachments y grants existentes manualmente; no quitar otras políticas para hacer espacio automáticamente. Si hay colisión de nombre/versiones/adjuntos, **detener y comparar**, no sobrescribir política existente ni reemplazar defaultversion a ciegas.
3. **IAM→Policies→Create policy→JSON:** copiar exactamente `infra/bootstrap/terraform-local-compute-policy.json` (hash§27.1), revisar acciones/condiciones y validación del editor; nombre **ENVER-Terraform-Compute**. Repetir con archivoIAM/hash y nombre **ENVER-Terraform-IAM-Runtime**. No AWSmanaged/Admin/PowerUser, wizard con recursos ampliados o corrección automática de scope; si el editor reporta errores, conservar mensaje y detener para revisión autorizada.
4. Solo tras decisión humana, **IAM→Users→enver-terraform-local→Permissions→Add permissions→Attach policies directly**: seleccionar exclusivamente las dos customer-managed recién revisadas de esta cuenta, revisar resumen y confirmar manualmente. **NO adjuntar estas policies al runtime role ni crear role/profile/inline runtime manualmente**; esos recursos pertenecen al futuro applyTerraform autorizado. No ejecutar `iam create-policy`, `attach-user-policy` o `put-user-policy` por el agente.
5. Conservar evidencia no sensible: cuenta/usuario, nombres/ARNs/versiones/defaultversion de policies, fechaUTC, decisión y asignaciones reales. No registrar sesión/tokens/accesskeys o screenshots con secretos. Volver al agente con esa evidencia y autorización delimitada para próxima verificación, **sin apply automático**. La propagación IAM/permiso efectivo no se infiere de una captura de creación; verificación posterior debe ser explícita, sin reintentos ciegos o dry-runs de escritura implícitos.
6. Reversión de **asignaciónIAM** si se requiere: responsable revisa y retira manualmente **solo estos dos attachments recién autorizados**, preservando grants ajenos y recursos; no borrar runtime/state o revocar seguridad para resolver un fallo. Retirar permisos no revierte infraestructura/cargos y puede impedir gestión posterior; revisar dependencias antes. No rollback/limpieza ejecutados en este turno.

### 27.4. Validaciones, presupuesto y estado conservados

**Nuevos resultados locales:** `terraform fmt -check -recursive infra` **exit0**; `terraform -chdir=infra validate -no-color` **exit0**, configuración válida. **No plan repetido**, HCL sin cambios. **12add/0change/0destroy, NETWORK14/ECR2no-op de§26 se conservan como baseline HISTÓRICA acreditada**, no nueva verificación AWS/drift o garantía para futuroapply. Sin STS/SSM/EC2/IAM/ECRCLI ni otra API de cuenta, scanning o Terraforminit/apply/destroy/import.

Costos sin cambios: **INFRA-5 USD2.3915/48h**, ALBfuturo **1.944**, conjunto **4.3355**, envolvente **7.3355**, guardrail **USD10** de ventana y **USD50 AGREGADOS** de proyecto, no mensual. Margen nominal2.6645, no presupuesto integral/consumo/crédito/reserva verificadas; ninguna asignaciónIAM o guía autoriza gasto/apply. Ventana máxima48h, sin infraestructura permanente/cap técnico o shutdown implícito.

**T005 PARCIAL / revisión y preparación manual acreditadas, aceptación pendiente**; **T009 PARCIAL / plan completo acreditado, provisión y aceptación pendientes**; **T010 PARCIAL / recovery documentado, prueba y aceptación pendientes**; **GATE-I-C PENDIENTE / NO SATISFECHO**, sin dosEC2 reales. Ninguna tarea/gate cerrados, todos9gates abiertos; T008/scanfinal/rollback y T025custodia/recovery/aceptación conservan§26, sin nuevas evidencias cloud.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-iam-manual-review-20261112-234436\`: hashes previos/revisiónpolicies/validaciones/revisiónfinal local; temporal no acredita custodia duradera. Solo task/memoria editados, mismos5nuevos/4modificados anteriores,0staged/HEAD25daa836. Hashes finales verifican candidatas/HCL/demás fuentes/state/backup/planes intactos, exclusiones y enlaces/diff-check. **CERO cambios o consultas AWS, CERO plan nuevo/apply, IAMadjunto/editado, recursos/Docker, stage/commit/push. DETENER para revisión/decisión humana, sin reanudación automática.**

## 28. INFRA-5 — permissions boundary externa obligatoria; plan bloqueado

### 28.1. Decisión y cambios autorizados

**2026-11-12 — BUILD/SECURITY HARDENING CONTROLADO, NO APPLY / NO AWS AUTOMÁTICO / NO ADJUNTAR POLICIES TODAVÍA.** Responsable no acepta PutRolePolicy+PassRole sin límite independiente y adopta permissions boundary administrada fuera de facultadesTerraform. No cambio de arquitectura runtime ni ampliación a secretos/authority. Precheck real **feature/infrastructure**, HEAD **25daa836c0571d4c0c02486e49ce435a108f0558**, mismos5nuevos/4modificados/0staged previos, trabajo preservado.

**Nuevo:** [runtime-permissions-boundary.json](../../infra/bootstrap/runtime-permissions-boundary.json). **Editados en este turno:** iam-runtime.tf, variables.tf, terraform-local-iam-runtime-policy.json y task/memoria. Compute.tf, computeJSON, networking/ECR/outputs/provider/lock/bootstrap/producto/tests/spec/plan/004/gobierno intactos. El role referencia boundary mediante `permissions_boundary = var.runtime_permissions_boundary_arn`; no resource/data source/policyversionTerraform de la boundary ni import o gestión de su contenido. CreateRole transmite boundary inicial, no requiere permitir luego PutRolePermissionsBoundary; fuente provider6.73.0 confirma uso de CreateRoleInput.PermissionsBoundary.

Variable **runtime_permissions_boundary_arn**, string no secreta, **nullable=false, SIN default**, validación exige cuenta/nombre/path de la referencia esperada. Ausencia impide plan no interactivo; validación no prueba existencia/contenido/version deAWS. **NO hay ARN real creado/comunicado/aprobado en este turno, no se suministra uno ni se ejecuta plan.** Referencia determinística por cuenta acreditada/nombre recomendado/path/:

```text
arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary
```

Es **referencia esperada en artefactos candidatos, NO policy existente acreditada**. No ARN aleatorio o lookup fingido. Si el administrador crea otra ruta/nombre, detener para nueva aprobación/sincronización, no aceptar otra boundary automáticamente. Antes de replan se necesita ARN REAL y aprobación decontenido/version/custodia; antes deapply nueva autorización y plan post-hardening revisado.

### 28.2. Máximo runtime —9acciones,3Allow/3Deny

Boundary **customer-managed recomendada ENVER-Runtime-Permissions-Boundary**, creada/administrada **manualmente por identidad administrativa independiente**, nunca enver-terraform-local. No concede permisos por sí sola: permisos efectivos de identidad se intersectan con boundary; inlinepoliciesruntime siguen necesarias.

| Allow máximo | Recurso/condición | Justificación del contrato actual |
| --- | --- | --- |
| ecr:GetAuthorizationToken | Resource=*; us-east-1/cuenta926925605666 | Autenticación ECR del host; no token embebido |
| ecr:BatchCheckLayerAvailability | Solo ARNrepositoryenver-dev-api/región | Consultar capas de imagen |
| ecr:GetDownloadUrlForLayer | Mismo repository | Descargar capas |
| ecr:BatchGetImage | Mismo repository | Leer manifiesto/imagen |
| ssm:UpdateInstanceInformation | Resource=* /us-east-1 | Registro/información básica del agente |
| ssmmessages:CreateControlChannel | Resource=* /us-east-1 | Crear canal control |
| ssmmessages:CreateDataChannel | Resource=* /us-east-1 | Crear canal datos |
| ssmmessages:OpenControlChannel | Resource=* /us-east-1 | Abrir canal control |
| ssmmessages:OpenDataChannel | Resource=* /us-east-1 | Abrir canal datos |

Las9Allow coinciden estructuralmente con runtimeHCL, **sin ampliar aAmazonSSMManagedInstanceCore/ec2messages/SSMparámetros/documentos/inventario**. Sin IAM/EC2/S3/KMS/DynamoDB/Secrets/ELB/Route53/ACM/admin/ECRwrite/sts:AssumeRole u otro grant fuera de esas9. Resource=* solo donde no hay ARN de acción apropiado; repositoryexacto para3pull. Uso/compatibilidad real del agenteSSM aún no probados.

**Denegaciones explícitas:** (1) `Deny`+`NotAction` con exactamente9acciones/Resource=* niega cualquier otra operación, incluido administrador/ECRwrite/rolechaining; no es un Allowwildcard. (2) Deny3pull con `NotResource`=repositoryexacto bloquea otrosrepos. (3) Deny9acciones si RequestedRegion no esus-east-1. Evitan depender solo de denyimplícito: AWS advierte grants resource-based directos aroleSESSION pueden eludir límites implícitos; denyexplícito sigue prevaleciendo. No `NotPrincipal`Deny. No se afirma aislamiento global de cuenta por este JSON o pruebascloud; acciones/grants finales requieren revisión efectiva antesdeapply.

Tamaños observados: **boundary2566bytes/1917compactchars**,3Allow/3Deny/9accionesAllow; **candidataIAM4673bytes/3448compactchars**,6Allow/3Deny/21accionesAllow; **compute6838bytes/5130compactchars**,9Allow/22acciones intactas. CadaJSON cabe en customer-managed6144sinwhitespace; IAMahora también excede inlineuser2048. Cuotas/semánticaAWS/adjuntos reales no validados.

### 28.3. Candidata del operador endurecida — no adjunta

- **CreateRole exacto** separado deCreateInstanceProfile: Allowrequiere `ArnEquals iam:PermissionsBoundary`=referenciaexacta, tags/user/cuenta originales. **PutRolePolicy** sigue requerido por dosresourcesaws_iam_role_policy y su Allowtambién exige roleexacto+boundaryexacta. UnaDenyCreateRole/PutRolePolicy con `ArnNotEquals` protege contra ausencia/otra boundary incluso anteAllowadicional mientras esa guard esté aplicada. Se verificó soporte real deambasacciones en referenciaAWS; **no** condición aplicada aCreateInstanceProfile, no soportada ahí.
- Lifecycleprofile solo **arn:aws:iam::926925605666:instance-profile/enver-dev-api-runtime**, sinwildcard. Get/DeleteRolePolicy ylecturas/tags/lifecycle conservan scopesanteriores. Allowactions no aumentan:21. Alternativa aPutRolePolicy: admin externo mantiene policiesruntime inmutables yTerraform solo adjuntaARNsexactos bajoBoundary; implica cambiar ownership/HCL/permisos/plan y aprobación aparte, **solo propuesta, no implementada**.
- **Denyexplícito** `iam:PutRolePermissionsBoundary`/`DeleteRolePermissionsBoundary` enroleexacto; noAllow deesasacciones. Terraformpuede crearrole conboundary enCreateRole, **no quitarla/reemplazarla/añadirla posterior para arreglar drift**. Anteboundarymissing/drift, detener yrevisiónadministrativa; noignore_changes/backdoor o ampliaciónIAM.
- **Denyexplícito** `CreatePolicy`, `CreatePolicyVersion`, `SetDefaultPolicyVersion`, `DeletePolicyVersion`, `DeletePolicy`, `TagPolicy`, `UntagPolicy` sobreARNexacto deboundary. NoAllowdeedición/eliminación/versiones ni administraciónIAMgeneral. Impide crear/recrear eseobjetooalterarlo usando esaidentidad cuando guardestéaplicada; contenido/versiones permanecen custodiadosporadministrador, noTerraform.
- **PassRole** conserva únicamente el role exacto y **PassedToService=ec2.amazonaws.com**. Añade `ArnLike iam:AssociatedResourceArn=arn:aws:ec2:*:926925605666:instance/*`. AWS documenta este patrón con región/ID comodín para PassRole usado por AddRoleToInstanceProfile: **no fijar us-east-1 o IDs concretos en esa condición**, causaría evaluación incorrecta. Aporta cuenta/tipo de recurso EC2 sin PassRole arbitrario; no restringe a dos instancias desconocidas. La región de compute sigue acotada por policy EC2. Contexto efectivo/comportamiento provider pendientes de validación cloud autorizada.

ComputeJSON **sinmodificar**, hashpreviamenteacreditado **e05dd08a4e3505f8dc5e2922e51ee498b99c0feb07d9a1d52634f7e71ad937a0** conservado; noIAMmutations/securitypolicychanges/NAT/ALB/AMIcreation/snapshotadmin o nuevosgrants. Su lecturaSSMpublica sigue necesaria paraAMI, nolecturasecretos. Nada concede alusuariocapacidadpara autoasignar o editar customer-managed deoperador.

### 28.4. Revisión estática y custodia independiente pendiente

**Revisión local estilo findingsIAM, NO AccessAnalyzeroficial/simulaciónIAM/pruebadeAWS:**

| Riesgo | Resultado de revisión |
| --- | --- |
| PassRole Resource=* oiam:*Allow | No detectados; ARNroleexacto |
| Resourcewildcardinnecesario | IAMAllowsexactos; condiciónAssociatedResourceArn necesita*documentado; boundary*enaccionesglobales/canales yDenyglobal justificados; computelecturas/ARNs anteriores conservados |
| Permissionsmanagementamplio | PutRolePolicyroleexacto+boundaryrequerida; capruntime9acciones, noAllowadministrativo |
| Versionmanagementboundary | NingúnAllow, Denyexplícito contraeditar/borrar/defaultversion |
| Boundarybypassmissing/removal/replacement | CreateRole/PutRolePolicycondicionados+Denysinboundary, retirada/sustituciónnegadas; protección documentada, **aúnnoaplicada** |
| Resourcepolicy/sessionbypass | BoundaryconDenyfuera9/otrosrepos/otraregión, no soloimplícitos |

Asercionesestructuraleslocales confirman JSONparse,9Allowruntime=9boundary, NotActionexacto,repoDeny, CreateRole/PutRolePolicycondiciones, sinAllowdegestiónboundary yconDeny, PassRole/profileexactos ysinTerraformresourcepolicy. **No certificado formal deIAM o mínimoefectivoglobal.** Crearrole aúnpermiteexpresartrust enrequest; HCLtrustsigueEC2. Boundarylimita permisos, no identidad/custodia deltrust. Revisartrust ypermisosagregados, imposibilidaddeautoadministrarusuariogrupos/policiesguard o asumiradministrador, antesdeasignar. Denydeguard solo protege mientrasasignado: si otrosgrantspermitenretirar/editarlaoguardarcredencialesadmin, elaislamientopuedeeludirse. **Auditoríaefectivadeesosotrosgrants/custodiaindependienterequerida**, no hecha sinAWSautorizado. No seafirma queTerraformyaestélimitado enAWS.

Fuentesoficialesconsultadas públicamente, sincredenciales/APIsdecuenta: [accionesIAM/CreateRole/PutRolePolicy](https://docs.aws.amazon.com/service-authorization/latest/reference/list_iam.html), [PermissionsBoundary/AssociatedResourceArn y excepciónInstanceProfile](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_iam-condition-keys.html), [evaluaciónboundary/grantsdirectossesión](https://docs.aws.amazon.com/IAM/latest/UserGuide/access_policies_boundaries.html), [providerAWS6.73.0 role.go](https://github.com/hashicorp/terraform-provider-aws/blob/v6.73.0/internal/service/iam/role.go). Revisión documental no valida contenidoefectivo delacuentani ejecutaAccessAnalyzer.

**Siguiente paso exclusivamentehumano/autorizaciónposterior:** adminindependiente revisaJSONycustodia, crea manualmente customer-managed **ENVER-Runtime-Permissions-Boundary** path/ usandoarchivocandidato, registraconta/ARNreal/defaultversion/contenido/hash/UTC/identidadadministrativa yverificaqueTerraformnopuedaadministrarla/autoeludirguard. **No attach ahora ENVER-Terraform-Compute o ENVER-Terraform-IAM-Runtime, ni boundary como identitypolicy delusuarioTerraform.** Role/profile siguen deTerraformpara futuroapply; boundary se referenciarácomoboundarydelrole, no resourcegestionado. No seguirautomáticamenteguía§27.3; requiere revisiónactualizada yautorizaciónespecífica. FuturacapacidadSecrets/authority fuera9exigiría nueva revisiónadministrativa deboundary/contratos/aprobación, noampliaciónporTerraform.

### 28.5. Validación, plan bloqueado y estados

**terraform fmt -recursive infra /fmt-check /validate exit0**, configuraciónválida; sininit/upgrade oAWSCLI. **PLAN NOEJECUTADO /BLOQUEADO POR BOUNDARYREAL**, sinvaloresficticios. Elplan§26 **12add/0change/0destroy yNETWORK14/ECR2no-op se rotula PRE-HARDENING**, no plandefinitivoapply ni nuevo no-op. No seafirma elconteodelplanposthardening: boundaryexternanoesresourceTerraform, pero solo unnuevoplanrealauthorizedacreditaráelresultadofinal.

Costo sin cambiocompute: **USD2.3915INFRA-5/48h**, ALBfuturo1.944, conjunto **4.3355**, envolvente **7.3355**, guardrail **USD10**, proyecto **USD50AGREGADOS**/máximo48h, no mensual/capAWS/gastoautorizado. SinBilling/crédito/reserva/consumoacumuladoverificados; hardeninglocalIAMnoaprovisionaservicios.

**T005 PARCIAL / HARDENING IAM PENDIENTE DE APLICACIÓN; T009 PARCIAL / PLAN POST-HARDENING BLOQUEADO POR BOUNDARY; T010 PARCIAL / recoverydocumentado,pruebayaceptaciónpendientes; GATE-I-C PENDIENTE/NOSATISFECHO.** T025PARCIAL/validaciónlocalacreditada,plananteriorPRE-HARDENING/aceptaciónpendiente. Ningúntask/gatecerrado,todos9gatesabiertos/11tareasparciales/19pendientes;T008/scanning/002/004/AUTH-027integralintactos.

Evidencialocalprivada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-boundary-hardening-20261112-235158\` (hashes/revisiónestructural/fmt/check/validate/revisiónfinal), temporalsincustodiaduraderagarantizada. Revisiónfinalpreservafuentesnoautorizadas/compute/state/backup/planesporhash, exclusiones/links/diff-check. Esperado **6nuevos/4modificadosglobales,0staged/HEAD25daa836** (boundaryúniconuevodeesteturno). **SIN AWSconsultadoomodificado/APIsdecuenta/IAMadjunto/plan/apply/destroy/import/Docker/Gitstage-commit-push. DETENER.**

## 29. INFRA-5 — plan POST-HARDENING con ARN real informado;12/0/0, NO APPLY

### 29.1. Fuente de boundary y alcance de autorización

**Ejecución 2026-11-13 UTC**, solicitud **POST-HARDENING PLAN CON PERMISSIONS BOUNDARY REAL / VALIDACIÓN CONTROLADA / NO APPLY**. Responsable confirma creación manual por identidad administrativa independiente, **no enver-terraform-local**, de **ENVER-Runtime-Permissions-Boundary**, ARN real:

```text
arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary
```

**Existencia y custodia acreditadas según esa fuente informada**, no nueva consulta GetPolicy/GetPolicyVersion/CloudTrail ni prueba independiente del contenido/defaultversion/hash remoto o de permisos efectivos del usuario. No se atribuye creación al agente. La autorización actual permite usar el ARN real en plan y supera el bloqueo de input anterior; no presenta el valor de un ARN como prueba técnica de existencia. Revisión de contenido/custodia/otros grants antes de cualquier futura provisión conserva sus pendientes. No administrar boundary mediante Terraform o adjuntar candidatas todavía.

Precheck real: **feature/infrastructure**, HEAD **25daa836c0571d4c0c02486e49ce435a108f0558**, mismos **6 nuevos/4 modificados/0 staged** anteriores, sin archivos inesperados. **Solo task/memoria editados ahora**; HCL/JSON/bootstrap sin cambios por esta entrega. Terraform **1.17.1/windows_amd64**, provider **6.73.0**, sin init/upgrade. `fmt -recursive infra`, `fmt -check -recursive infra`, `-chdir=infra validate -no-color`: **exit0/exit0/exit0**, configuración válida; fmt no produjo cambios de fuente.

### 29.2. Plan real y boundary obligatoria

**Una ejecución**, **2026-11-13T00:10:31.3416803Z–00:10:51.8661754Z**:

```text
terraform -chdir=infra plan -detailed-exitcode -input=false -lock-timeout=30s -parallelism=1 -no-color
Plan: 12 to add, 0 to change, 0 to destroy.
exit code: 2
```

Exit2 es **éxito con altas propuestas**, sin AccessDenied, replacement o destroy. Se pasaron inputs aprobados por entorno de proceso temporal restaurado: **AWS_PROFILE=enver-infra**, regiónus-east-1, project=enver/environment=dev/account926925605666/VPC10.20.0.0/16/newbits8/common_tags={}/network_design_approved=true/compute_plan_approved=true y **TF_VAR_runtime_permissions_boundary_arn=ARN REAL anterior**. `compute_ami_id=null` por defecto, como baseline; dataSSM/dataAMI completan y ambas EC2 usan **ami-096ef200a877f36af**. AWS_MAX_ATTEMPTS=1/fallbackmetadata deshabilitado, sin credenciales alternativas/overrides; no tfvars real nuevo, ARN no secreto ni valor persistido por inventar recurso. No token/connection string/JWT en inputs/logs.

**Verificación directa en el bloque aws_iam_role.api_runtime del plan:** nombre **enver-dev-api-runtime**, trust **ec2.amazonaws.com**, y:

```text
permissions_boundary = "arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"
```

El HCL sigue `permissions_boundary = var.runtime_permissions_boundary_arn`, obligatorio sin default. **Boundary referenciada obligatoriamente EN PLAN, no role/boundary aplicada al role real**. No binario de plan nuevo guardado ni apply; no ARN de instancia/EIP inventado. Plan exitoso no prueba que CreateRole/PassRole/PutRolePolicy/RunInstances estén autorizados, ni capacidad disponible, ausencia de colisión con recursos no gestionados, contenido remoto de boundary o que la identity policy candidata ya esté adjunta. No habilitar apply por exit2. **Plan§26 sigue PRE-HARDENING**, este es el resultado POST-HARDENING, sin autorizar gasto/provisión.

### 29.3. Inventario completo, no-op y seguridad de hosts

| Nuevas direcciones reales | Cantidad |
| --- | --- |
| aws_iam_role.api_runtime | 1 |
| aws_iam_instance_profile.api_runtime | 1 |
| aws_iam_role_policy.api_ecr_pull | 1 |
| aws_iam_role_policy.api_ssm_session | 1 |
| aws_network_interface.api["a"], aws_network_interface.api["b"] | 2 |
| aws_eip.api["a"], aws_eip.api["b"] | 2 |
| aws_eip_association.api["a"], aws_eip_association.api["b"] | 2 |
| aws_instance.api["a"], aws_instance.api["b"] | 2 |
| **Total POST-HARDENING** | **12 add /0 change /0 destroy** |

Boundary es **externa**, no decimotercer recurso Terraform. Raíces EBS inline, no recursos independientes extra. **NETWORK14 y ECR2 no-op confirmados para este plan completo**:16direcciones managed refrescadas, cotejadas con inventario y sin cambio/drift/replacement/destroy en salida. No prueba de toda la cuenta o no-op futuro. No se repite ninguna API tras plan para inventario cloud adicional.

EC2 **2×t3a.small**, CPUstandard; **a=us-east-1a/use1-az1**, **b=us-east-1b/use1-az2** en esta cuenta, misma AMI aprobada **ami-096ef200a877f36af**. DataAMI filtró Amazon/AL2023/x86_64/HVM/EBS/available; detalles de OS/kernel mantienen evidencia informada previa. Cada root **10GiB gp3 cifrado/3000IOPS/125MiB/s/delete_on_termination=true**; IMDSv2 **http_tokens=required/hoplimit1**, metadata tags deshabilitados. No capacidad/hashlatency/arranque/health reales probados.

**2ENI persistentes,2EIP,2asociaciones** en plan; ENI soloSGbackend **sg-070d06cfebe866427** y subnets existentes, EIP→ENI antes del boot. map_public_ip_on_launch=false conservado. Backend ingress **TCP8080 solo SGALB sg-0dacf0df5da30b4f7**, sinCIDR público a8080/SSH22/keypair definido; valores derivados known-after-apply no son observación de instancia real. BootstrapDocker/SSM leído en plan sin accesskeys/JWT/Mongo/Redis/password/ECRtoken, sin dockerlogin/pull/run o autoarranqueEnver.API. No secretos detectados por inspección/indicadores, no auditoría exhaustiva de capas. Sin ALB/Redis/DynamoDB/Secrets/NAT/servicios adicionales o recursos aplicados.

### 29.4. IAM revisado: diseño limitado, eficacia futura no inferida

- Candidata IAM intacta: **CreateRole solo role exacto**, `ArnEquals iam:PermissionsBoundary` exacta; **Deny explícito CreateRole/PutRolePolicy** sin boundary o con otra. CreateProfile y lifecycle limitados al profile ENVER exacto, sin comodín de profiles. No grants de administración del operador por esta entrega.
- **PutRolePolicy** sigue requerido por las dos inline y limitado al role exacto **con boundary exacta**. Máximo de permisos identity-based = **identity policy ∩ permissions boundary**, sujeto a **explicit deny** y demás políticas aplicables. Boundary local de9acciones incorpora también Denyfuera9/otrosrepos/otraregión para grants resource-based a sesiones; no ignorar esa salvedad de evaluación ni dar cap efectivo remoto por comprobado sin comparar versión remota. No permisos efectivos por encima del límite según diseño; runtime efectivo sigue sin desplegar/probar.
- **Sin Allow y con Deny explícito** a PutRolePermissionsBoundary/DeleteRolePermissionsBoundary en role exacto; a CreatePolicy/CreatePolicyVersion/SetDefaultPolicyVersion/DeletePolicyVersion/DeletePolicy/TagPolicy/UntagPolicy sobre boundary exacta. **No poder retirar/editar según candidata**, no certificado de imposibilidad efectiva del usuario actual: candidatas no adjuntas y permisos agregados/custodia independiente no auditados por AWS en este turno. Se conserva revisión de otras vías de auto-administración/retirar guard/usar identidades privilegiadas antes de apply.
- **PassRole solo arn:aws:iam::926925605666:role/enver-dev-api-runtime**, `iam:PassedToService=ec2.amazonaws.com`, `ArnLike iam:AssociatedResourceArn=arn:aws:ec2:*:926925605666:instance/*`. Se reconfirma [documentación oficial](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_iam-condition-keys.html): AddRoleToInstanceProfile exige región/ID comodín en esa condición. Patrón **estáticamente compatible con las EC2 de esta cuenta y ambasAZ**, no se debilita ni sustituye por región fija o IfExists. Ninguna duda técnica pendiente sobre la forma documentada; **plan no evalúa PassRole/AddRoleToInstanceProfile real**, su eficacia requiere ejecución futura autorizada. No IAMsimulator/AccessAnalyzerofficial o grant nuevo.
- Boundary máxima actual sin ampliar: **ECRGetAuthorizationToken +BatchCheckLayerAvailability/GetDownloadUrlForLayer/BatchGetImage** (3pull solo repositoryenver-dev-api); **SSMUpdateInstanceInformation +ssmmessagesCreateControlChannel/CreateDataChannel/OpenControlChannel/OpenDataChannel**. Inline9Allow coincide con límite local, sin ECRwrite/admin/EC2admin/secretos. NoIammanagedattachment extra.

**Futuro DynamoDB Authentication Authority/secretos/config:** no se añade ninguna acción ahora. Boundary actual es intencionalmente de fase compute inicial, **no defecto deINFRA-5**; ampliarla requiere contrato/alcance/revisión humana, edición por identidad administrativa independiente, evidencia de nueva versión y autorización posterior; **nunca enver-terraform-local** o ampliación silenciosa para arrancarAPI. Custodia/admin de boundary fuera deTerraform preservada.

### 29.5. Presupuesto, estados y cierre

No nuevos recursos/capacidades fuera de estimación: **INFRA-5 USD2.3915/48h** (EC2 1.8048/EBS0.106667/IPv40.48), futuroALB48h **1.944**, conjunto **4.3355**, envolvente **7.3355**, guardrail **USD10**, margen nominal **2.6645**, proyecto **USD50 AGREGADOS**, reservaobjetivo50/crédito100informado. No mensual/FreeTier/cap técnico/Billing o consumo acumulado verificados. Costos LCU/tráfico/retención/dependencias variables y bolsas no cotización integral. **Guardrail respetado solo en estimación provisional; no autoriza apply/gasto/permanencia mayor48h o limpieza automática.**

**T005 PARCIAL / HARDENING IAM PENDIENTE DE APLICACIÓN**, plan acreditado y existencia/custodia según responsable, sin role/grants efectivos aceptados. **T009 PARCIAL / PLAN POST-HARDENING ACREDITADO; PROVISIÓN/ACEPTACIÓN PENDIENTES**. **T010 PARCIAL / RECOVERY DOCUMENTADO; PRUEBA/ACEPTACIÓN PENDIENTES**. **T025 PARCIAL / VALIDACIÓN LOCAL Y PLAN POST-HARDENING ACREDITADOS; ACEPTACIÓN PENDIENTE** (statebackup/custodia/recovery/dependencias sin aceptar). **GATE-I-C PENDIENTE / NO SATISFECHO**, sin2EC2reales/digestA-B/pruebas; todos9gatesabiertos/11tareasparciales/19pendientes, sin cierres002/004/AUTH-027integral.

**Sin AccessDenied, permiso faltante no observado en plan**; no ampliar IAM por prever errores decreate. No consulta scanning autorizada ahora: evidencia§26 ScanNotFound conserva digest/alcance, IN_PROGRESS del hijo histórico/informado, sin findings=0 o resultadofinalnuevo. Solo plan como operacióncloud; sin IAMCLI/GetPolicy, simulación, Docker/tests/app/workflows o mutaciones.

Evidencia privada original `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-post-boundary-plan-20261113-000939\`: hashesprevios, revisiónestática, fmt/check/validate/version, plan.txt/plan-result.json/plan-analysis.json; temporal no garantiza custodia duradera. **El corte dejó la revisión final Git/hashes sin acreditar; se completa en §29.6**, sin atribuirla retroactivamente a esta carpeta. No aplicar/restaurar pre-hardening ni nuevos secretos/tfvars. Solo task/memoria editados; mismos6nuevos/4modificadosglobales/0staged/HEAD25daa836. **Sin apply/destroy/import, IAM modificado/adjuntos o EC2/EIP creados, stage/commit/push.**

### 29.6. Recuperación de sesión interrumpida y validación renovada — NO APPLY

**2026-11-13 UTC — CONTINUAR SESIÓN INTERRUMPIDA.** Se inspeccionó antes de editar: branch/HEAD/status/diff-check/stat y diffs solicitados. **feature/infrastructure / 25daa836c0571d4c0c02486e49ce435a108f0558**; **4 modificados** (outputs.tf, variables.tf, task.md, memoria.md), **6 nuevos sin seguimiento** (compute.tf, iam-runtime.tf, compute-host.sh y las tres candidatas JSON compute/IAM/boundary), **0 staged**. Los diffs vacíos de archivos no seguidos no implican archivos vacíos: se leyeron directamente. Sin archivos truncados o errores sintácticos observados; las secciones **§29 y memoria§79 ya estaban escritas**, pero el cierre/revisión final no estaba acreditado. Cambios recuperables, no regenerados/revertidos ni borrados.

Comparación con hashes previos a la sesión cortada: **solo task/memoria cambiaron**; HCL, candidatas, bootstrap, fuentes restantes, **state/backup y planes históricos coinciden por SHA256**. ARN real disponible tanto en estos documentos como en **plan-result.json** anterior: `arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary`. Fuente de existencia/custodia sigue siendo responsable; **sin verificación independiente de policy/version remota o permisos agregados**. No tfvars reales nuevos; ningún ARN inventado o secreto guardado.

Validaciones renovadas **fmt -check -recursive infra**, **-chdir=infra validate -no-color** y **bash --noprofile --norc -n infra/bootstrap/compute-host.sh**: **exit0/exit0/exit0**, Bash nativo de Git, sin ejecutar bootstrap. Terraform1.17.1/windows_amd64 y provider6.73.0 conservados. Sin correcciones HCL/JSON/shell necesarias; revisión estática reafirma variable obligatoria sin default y boundary en role, CreateRole/PutRolePolicy exactos condicionados con Deny explícito, Deny anti-retiro/sustitución/administración y PassRole limitado según§29.4. [Referencia oficial AssociatedResourceArn](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_iam-condition-keys.html) reconfirmada: `arn:aws:ec2:*:926925605666:instance/*`, región/ID comodín necesarios para AddRoleToInstanceProfile, compatible estáticamente; no debilitada, eficacia AWS no probada.

**Un nuevo plan en esta recuperación**, conservando intacto el anterior: **2026-11-13T00:21:46.3476521Z–00:22:11.5023286Z**, mismo comando/inputs temporales restaurados de§29.2, incluido ARN real y perfilenver-infra/us-east-1; **exit2 / 12add / 0change / 0destroy**. Sin AccessDenied, replacements o drift reportado; no permiso faltante observado en plan. **Role incluye boundary exacta**; inventario coincide íntegramente con las12direcciones de§29.3, **NETWORK14 y ECR2 no-op** por reconciliación de16refresh con state y salida completa. No inferir permisos decreate o que las candidatas estén adjuntas. Plan solo texto/evidencia, **sin binario nuevo ni apply**.

Reconfirmado en plan: **2t3a.small/AMI ami-096ef200a877f36af**, **us-east-1a/use1-az1 y us-east-1b/use1-az2**, raíces **10GiB gp3 cifradas**, CPUstandard, IMDSv2required/hop1; **2ENI/2EIP/2asociaciones**. SGbackend solo8080desdeSGALB, sinSSH22/8080públicos, map_public_ip_on_launch=false. BootstrapDocker/SSM sin secretos detectados o APIautostart. Boundary conserva las9accionesECR/SSM y futuros DynamoDB/secretos requieren administración independiente autorizada: **sin ampliar**. Costos§29.5 intactos: **2.3915/48h INFRA-5**, **1.944ALB futuro**, **4.3355conjunto**, **7.3355envolvente**, **USD10guardrail**, **USD50proyecto agregado**; estimaciones, no gasto/credit verification o autorizaciónapply.

**Estados sin cierre:** T005 **PARCIAL / HARDENING IAM PENDIENTE DE APLICACIÓN**; T009 **PARCIAL / PLAN POST-HARDENING ACREDITADO**, provisión/aceptación pendientes; T010 **PARCIAL / RECOVERY DOCUMENTADO**, prueba/aceptación pendientes; T025 **PARCIAL / VALIDACIÓN LOCAL Y PLAN POST-HARDENING ACREDITADOS**, aceptación pendiente. **GATE-I-C y los nueve gates PENDIENTE / NO SATISFECHO**, sin2EC2reales; no aceptación cloud deAUTH.

Consolidación **sin duplicar §29/§79**; solo task/memoria editados. Evidencia privada de recuperación `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-session-recovery-20261113-002038\`: precheck/diff/hashes, validaciones, plan/resultados/análisis y revisiónfinal. Cierre con hashes de fuentes no autorizadas/state/backup/planes preservados, links/exclusiones y **git diff --check exit0**, **4 modificados/6 sin seguimiento/0 staged**, branch/HEAD intactos. Temporal no acredita custodia duradera. **Sin apply/destroy/import, cambios IAM/adjuntos, EC2/EIP u otros recursos creados, stage/commit/push. DETENER.**

## 30. INFRA-5 — replan PRE-APPLY tras adjuntos IAM informados;12/0/0, NO APPLY

### 30.1. Autorización, checkpoint e identidad

**2026-11-13 UTC — REPLAN CON PERMISOS DE APPLY REALES / VALIDACIÓN PRE-APPLY / NO APPLY.** Responsable informa que **ENVER-Terraform-Compute** y **ENVER-Terraform-IAM-Runtime** fueron creadas manualmente desde los dos JSON versionados y adjuntadas **únicamente a enver-terraform-local**. Informa también que **ENVER-Runtime-Permissions-Boundary NO se adjuntó al usuario Terraform**. Se registra **APLICADAS MANUALMENTE SEGÚN RESPONSABLE**, no descubrimiento independiente o mutación del agente. No se enumeran adjuntos, versiones remotas, otros grants/boundaries/SCP o custodia; no simulación IAM/Access Analyzer ni dry-run de escrituras.

Baseline real **feature/infrastructure / f19ca80ef899436bbe1ad724db950fc0dfa629ae**, árbol limpio/0 staged antes de lecturas/plan. Checkpoint anterior autorizado: padre25daa836, diez archivos, **feat(infra): prepare hardened compute baseline**; no nuevo commit en esta entrega. Solo task/memoria editados posteriormente; HCL/policies/bootstrap/lock/spec/plan/producto sin cambios.

Un sondeo inicial no encontró `aws` en PATH y terminó localmente **antes de ejecutar AWS CLI o llamar APIs**. Se usa instalación existente `%LOCALAPPDATA%/Programs/Amazon/AWSCLIV2/aws.exe` ya registrada en memoria, sin instalar, cambiar PATH persistente o credenciales. **Una llamada real** `sts get-caller-identity --profile enver-infra --region us-east-1 --output json --no-cli-pager`: **exit0**, identidad **arn:aws:iam::926925605666:user/enver-terraform-local**, cuenta926925605666 esperada. Sin credenciales publicadas. STS confirma identidad, **no permisos deapply ni adjuntos por nombre**.

### 30.2. Validaciones y plan nuevo real

**fmt -check -recursive infra**, **-chdir=infra validate -no-color**, **bash --noprofile --norc -n infra/bootstrap/compute-host.sh**: **exit0/exit0/exit0**. Terraform **1.17.1/windows_amd64**, provider **6.73.0**, sin init/upgrade/cambios técnicos. Bash solo parsea, no ejecuta bootstrap.

**Un plan nuevo**, **2026-11-13T00:38:03.9212751Z–00:38:23.5648046Z**:

```text
terraform -chdir=infra plan -detailed-exitcode -input=false -lock-timeout=30s -parallelism=1 -no-color
Plan: 12 to add, 0 to change, 0 to destroy.
exit code: 2
```

**Resultado real 12/0/0, 0 replacements, sin AccessDenied o drift reportado.** Perfil enver-infra/regiónus-east-1; inputs temporales restaurados: project=enver/environment=dev/account926925605666/VPC10.20.0.0/16/newbits8/common_tags={}/network_design_approved=true/compute_plan_approved=true/compute_ami_id=null por defecto y **runtime_permissions_boundary_arn=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary**. Sin tfvars reales nuevos, credenciales alternativas o secretos. AWS_MAX_ATTEMPTS=1/metadatafallback deshabilitado. DataSSM/dataAMI completan con AMI aprobada; no consultas adicionales de scanning/IAM/Billing.

**Inventario de altas observado en plan:**

| Direcciones | Cantidad |
| --- | --- |
| aws_iam_role.api_runtime | 1 |
| aws_iam_instance_profile.api_runtime | 1 |
| aws_iam_role_policy.api_ecr_pull | 1 |
| aws_iam_role_policy.api_ssm_session | 1 |
| aws_network_interface.api["a"], aws_network_interface.api["b"] | 2 |
| aws_eip.api["a"], aws_eip.api["b"] | 2 |
| aws_eip_association.api["a"], aws_eip_association.api["b"] | 2 |
| aws_instance.api["a"], aws_instance.api["b"] | 2 |
| **Total** | **12** |

**NETWORK14 y ECR2 no-op**, reconciliando16direcciones managed refrescadas con inventario y plan completo sin modificaciones/replacements/destroy. Boundary externa no es recurso13; roots EBS inline. No ARN/ID/IP de recursos futuros inventados. Plan anterior§26 conserva estado **PRE-HARDENING**;§29 es anterior a estos adjuntos informados, no se reutiliza paraapply.

### 30.3. Boundary, permisos y seguridad reconfirmados

Runtime role **enver-dev-api-runtime**, trustEC2 y **permissions_boundary="arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"** en plan. HCL exige variable obligatoria sin default. Boundary creada/custodiada independientemente según responsable, no contenido/defaultversion/hash remoto verificados en esta entrega. **CreateRole/PutRolePolicy solo role exacto con boundary exacta**, más Deny explícito si falta/difiere. **Put/DeleteRolePermissionsBoundary** y administración/versiones/borrado de boundary **sin Allow y con Deny explícito** en JSON informado adjunto. No afirma imposibilidad global auditada de eludir/retirar la protección: contenido remoto, otros grants y custodia de guard siguen sin revisión efectiva independiente. Si se aplica y conserva el JSON informado, esos Deny prevalecen sobre otros Allow aplicables.

**PassRole solo arn:aws:iam::926925605666:role/enver-dev-api-runtime**, **PassedToService=ec2.amazonaws.com**, `AssociatedResourceArn=arn:aws:ec2:*:926925605666:instance/*`. Patrón documentado§29 compatible estáticamente con AddRoleToInstanceProfile, intacto, sin debilitamiento. **Plan no ejercita CreateRole/PutRolePolicy/PassRole/RunInstances** ni certifica capacidad, cuotas, colisiones con recursos no gestionados o éxito de un futuro apply. Lecturas exitosas prueban solo acceso de las operaciones efectivamente ejecutadas; **permisos efectivos de escritura NO PROBADOS**.

Máximo runtime conserva **nueve acciones ECR/SSM**, pull solo enver-dev-api y regiónus-east-1; identitypolicy∩boundary y explicitDeny prevaleciente según diseño, no grants nuevos DynamoDB/Secrets/config. Ampliación futura requiere revisión humana y administración independiente, nunca Terraform automáticamente.

**2×t3a.small/CPUstandard**, **us-east-1a/use1-az1** y **us-east-1b/use1-az2**, **ami-096ef200a877f36af** filtrada Amazon/AL2023/x86_64/HVM/EBS/available. Cada raíz **10GiB gp3 cifrado/3000IOPS/125MiB/s**, IMDSv2required/hop1. **2ENI/2EIP/2asociaciones**, map_public_ip_on_launch=false, backendSG8080soloSGALB/sinSSH22/8080públicos. User_data Docker/SSM inspeccionado **sin secretos detectados ni autoarranqueEnver.API/login-pull-runECR**. Sin hosts reales o pruebas debootstrap/SSM/egreso/capacidad/health/recovery.

### 30.4. Costo, estados y detención

Estimaciones conservadas **INFRA-5 USD2.3915/48h**, **ALBfuturo1.944**, **conjunto4.3355**, **envolvente7.3355**, **guardrailUSD10** (margennominal2.6645), proyecto **USD50AGREGADOS**, máximo inicial48h/reservaobjetivo50 delcrédito100informado. Sin Billing/créditos/gastoacumulado verificados, FreeTier/mensual/capAWS asumidos o cotización integral nueva. LCU/transferencia/otrosservicios/retención variables. No gasto autorizado por el plan.

**T005 PARCIAL / IAM ADJUNTADO SEGÚN RESPONSABLE; RUNTIME Y VALIDACIÓN EFECTIVA PENDIENTES**; **T009 PARCIAL / PLAN PRE-APPLY POST-HARDENING ACREDITADO; PROVISIÓN/ACEPTACIÓN PENDIENTES**; **T010 PARCIAL / RECOVERY DOCUMENTADO; PRUEBA/ACEPTACIÓN PENDIENTES**; **T025 PARCIAL / VALIDACIÓN LOCAL Y PLAN PRE-APPLY POST-HARDENING ACREDITADOS; ACEPTACIÓN PENDIENTE**. **GATE-I-C PENDIENTE / NO SATISFECHO**, sin2EC2reales, mismo digest/capacidad/recuperación sin verificar. Nueve gates abiertos/11tareasparciales/19pendientes; sin aceptación002/004/AUTH-027integral.

**PRE-APPLY acreditado solo como resultado de planificación tras adjuntos informados**, no aprobación integral de provisión, permisos efectivos de escrituras, seguridad o presupuesto acumulado. No se guardó binario de plan aplicable. Un futuro apply exige autorización humana separada, revisión vigente de plan guardado exacto, contenido/custodia IAM y costos integrales/ventana/retención/responsables de apagado y evidencia de seguridad/state según alcance. **AccessDenied: ninguno; permiso faltante: ninguno observado en plan**, no propuesta de ampliar IAM ni nuevo replan automático.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-attached-iam-preapply-20261113-003559\`: baseline/hashes, incidente local dePATH, STS, revisiónestática/validaciones/version, plan/resultados/análisis/noop y revisiónfinal. Temporal sin custodia duradera garantizada. Cierre: fuentes no autorizadas/state/backup/planes históricos preservados porhash, links/exclusiones/diff-check válidos, **solo task/memoria modificados /0staged /HEADf19ca80 intacto**. **SIN apply/destroy/import, IAM modificado por agente, recursos creados, stage/commit/push. DETENER.**

### 30.5. Plan guardado final y checkpoint documental — NO APPLY

**2026-11-13 UTC.** Solicitud **AUTORIZAR EXCLUSIÓN DE TFPLAN Y CONTINUAR PRE-APPLY / BUILD CONTROLADO**. El intento previo se detuvo correctamente porque `git check-ignore -v infra/tfplan-infra5` devolvió exit1; no generó plan, editó documentos o creó commit. Esta autorización permite únicamente la nueva regla exacta **`/infra/tfplan-infra5`**, sin tocar otras reglas, y el checkpoint de .gitignore/task/memoria. Baseline real **feature/infrastructure / f19ca80ef899436bbe1ad724db950fc0dfa629ae**, solo task/memoria modificados y0staged; se conserva esa documentación previa en este checkpoint.

**Ignore verificado antes de plan:** `.gitignore:94:/infra/tfplan-infra5`, **exit0**, ruta no existente entonces y no tracked. Comparación con copia previa confirma una sola regla añadida, resto intacto. State/backup/tfvars reales/.terraform/.aws/credentials/config/.docker/config.json/.env/secrets.json siguen ignorados en sus rutas convencionales; sin artefactos sensibles tracked, lock/ejemplo siguen versionables. No afirmar exclusión universal de cualquier archivo con nombre arbitrario ni leer/publicar credenciales.

**Fmt-check/validate/bash-n exit0/0/0**, Terraform1.17.1/windows_amd64 y provider6.73.0, sin init/upgrade o ejecución debootstrap. **Una generación nueva** entre **2026-11-13T01:39:28.1561780Z y01:39:44.1908175Z**, con perfilenver-infra/us-east-1 y los inputsbaseline de§30.2 por entorno temporal restaurado; boundary pasada explícitamente por `-var`:

```text
terraform -chdir=infra plan -out=tfplan-infra5 -detailed-exitcode -var="runtime_permissions_boundary_arn=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"
Plan: 12 to add, 0 to change, 0 to destroy.
exit code: 2
```

Archivo **`infra/tfplan-infra5`**, nuevo/no sobrescrito, SHA256 **`ddb6dd34d027b32ae09046128d0acf2132575dc33737bafb91d654691fb997ed`**. **Show humano (`show -no-color tfplan-infra5`) y show-json exit0**, del binario exacto, sin nuevo plan ni APIs de cuenta durante la inspección. JSON completo no publicado/versionado; solo resumen saneado. Lectores locales corrigieron colisión PowerShell de array.Address con propiedad address y decodificación nativaibm850 deUTF8; generaban falsos fallos de cotejo, no cambios del plan/HCL. Enumeración por elemento y lectura UTF8 confirman inventario/user_data exactos; **sin reintento cloud o regeneración del plan**.

**Inventario exacto guardado:** las12direcciones de§30.2, **role1/profile1/inline2/ENI2/EIP2/asociaciones2/EC2 2**. JSON contiene **12create/16no-op/0update/0delete/0replacement**, sin drift reportado. **NETWORK14no-op y ECR2no-op explícitos**; ningún recurso existente cambia. Boundary externa no es recurso adicional y raícesEBS son inline.

**Runtime role en binario:** enver-dev-api-runtime/trustEC2, **permissions_boundary=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary**. InlineSSM/ECR conservan9acciones máximas. Revisión del JSON operador intacto confirma PassRole solo runtimeARN/PassedToServiceec2.amazonaws.com/AssociatedResourceArn`arn:aws:ec2:*:926925605666:instance/*`, sinAllow yconDeny antiPut/DeleteRolePermissionsBoundary y versiones/admin/borradoboundary. Creación/adjuntos/custodia siguen informados por responsable, **no auditoría remota ni prueba de CreateRole/PassRole/RunInstances**, no ampliar boundary ahora.

**Compute guardado:**2t3a.small/CPUstandard/AMI **ami-096ef200a877f36af**, AZ **us-east-1a/use1-az1** y **us-east-1b/use1-az2**, raíces **10GiBgp3cifradas/3000IOPS/125MiB/s**, IMDSv2required/hop1. **2ENI/2EIP/2asociaciones**, ENIsoloSGbackend, map_public_ip_on_launch=false;8080soloSGALBexistente/sinSSH22/8080públicos. Ambos user_data coinciden exactamente con bootstrap revisado normalizandoCRLF: Docker/SSM **sinAWSkeys/sessiontokens/Mongo/Redis/JWT/ECRtokens/passwords detectados ni autoarranqueAPI**, sin dockerlogin/pull/run. No prueba real dehost/health/SSM/egreso/capacidad/recovery.

Costos conservados: **INFRA-5USD2.3915/48h**, **ALBfuturo1.944**, **conjunto4.3355**, **envolvente7.3355**, **guardrailUSD10**, proyecto **USD50AGREGADOS**/máximo48h. Estimaciones no cotización integral/Billing/créditos/acumulados verificados, ni cap técnico o autorización de gasto. Retención/LCU/transferencias/dependencias y responsabilidades de apagado requieren revisión integral previa aapply.

**T005 yT009 PARCIAL / PRE-APPLY ACREDITADO** solo para preparación revisada; runtime/provisión/seguridad efectiva/aceptación pendientes. **T010 PARCIAL**, recovery/prueba/aceptación pendientes; **T025 PARCIAL**, validación local/plan guardado acreditados, custodia/state/backup/recovery/aceptación pendientes. **GATE-I-C PENDIENTE / NO SATISFECHO**, sin dosEC2reales/digest/pruebas; los nueve gates siguen abiertos. Preparación no completa005/009/010/025/027 ni acepta002/004/AUTH-027integral.

**Checkpoint autorizado:** stage exclusivamente **.gitignore**, **specs/003-infrastructure/task.md**, **memoria.md**, revisióncached-check/stat de3archivos y commit **`chore(infra): prepare compute pre-apply`**. El hash final se comunica al cierre y en evidencia privada, sin autorreferencia en el commit; **plan/state/backup/tfvars/credenciales nunca stage/tracked**. Plan y state son artefactos privados locales, ignorar no equivale a custodia/cifrado/backup aceptados.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-saved-plan-checkpoint-20261113-013805\`: baseline/hashes, ignore/validaciones/version, generación/hash, show/análisis selectivo, revisiónprecommit y finalGit. Carpeta temporal no garantiza retención/custodia. Fuentes no autorizadas/state/backup/planes anteriores preservados porhash; plan nuevo conserva suhash durante revisión/checkpoint. Futuroapply exige autorización separada y vigencia/hash exacto revisados, más costo/seguridad/state según alcance; **no permiso implícito por existir archivo aplicable**. **Sin AccessDenied, apply/destroy/import, escriturasAWS/IAM, recursos creados o push. DETENER tras checkpoint, sin siguiente incremento automático.**

## 31. INFRA-5 — apply autorizado PARCIAL; lectura EIP denegada, sin EC2

### 31.1. Autorización y precheck

**2026-11-13 UTC — APPLY CONTROLADO DEL PLAN GUARDADO.** Responsable autoriza exclusivamente aplicar **infra/tfplan-infra5**, hash **ddb6dd34d027b32ae09046128d0acf2132575dc33737bafb91d654691fb997ed**, evaluación temporal48h/guardrailUSD10/proyectoUSD50agregado. No nuevo plan paraapply, segundoapply, destroy/import, IAMmanual/otros recursos/API o stage/commit/push. Verificaciones cloud/post-plan solo después de éxito completo; fallo parcial exige detener y registrar.

Precheck real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, árbol limpio/0staged. **SHA256 exacto coincide**, show humano/JSON exit0 del mismo binario: **12create/16no-op/0update/0delete/0replacement**, NETWORK14/ECR2no-op, inventario12 de§30.2, role/boundary/compute/user_data esperados. Terraform1.17.1/windows_amd64/AWS6.73.0. No plan nuevo previo, init/upgrade o cambios HCL/JSON/.gitignore. Perfilenver-infra/regiónus-east-1 mediante entorno temporal restaurado, sin credenciales alternativas; AWS_MAX_ATTEMPTS1/metadatafallbackoff.

### 31.2. Único apply y fallo parcial real

Comando exacto **`terraform -chdir=infra apply tfplan-infra5`**, **2026-11-13T01:57:56.9337244Z–01:58:16.2718969Z**, duración **19.333s**, **exit1**. **NO hubo Apply complete ni resumen 12added/0changed/0destroyed**. Seis líneas Creationcomplete (role/profile/dosinline/dosENI); las dos EIP fueron asignadas pero falló lectura posterior y quedaron **tainted** enstate. **8nuevas entradas managed**, ninguna modificación/baja de existentes observada; no convertir incremento neto de state en resumen nativo exitoso.

| Denegación confirmada | Detalle |
| --- | --- |
| Acción | **ec2:DescribeAddressesAttribute**; operación API DescribeAddressesAttribute |
| Respuesta | **403 / UnauthorizedOperation**, no identity-based Allow según AWS |
| Principal | arn:aws:iam::926925605666:user/enver-terraform-local |
| Fase | **apply/create EIP, lectura de atributo domain name después de AllocateAddress** |
| aws_eip.api["a"] / compute.tf:48 | arn:aws:ec2:us-east-1:926925605666:elastic-ip/eipalloc-0c297551d49ce9416; RequestID d5e176fd-e4e2-4c62-906f-eef6fd8d5d3d |
| aws_eip.api["b"] / compute.tf:48 | arn:aws:ec2:us-east-1:926925605666:elastic-ip/eipalloc-0b54456ecfc2cdafa; RequestID 91f170b8-7baf-4576-afaf-f3c83638aa96 |

**Detenidas operaciones AWS al retornar el apply fallido**, sin segundoapply, post-plan, inventario CLI, STS/IAM/SSM/Docker o ampliación de permisos. Terraform ya tenía operaciones paralelas en vuelo; no presentarlas como reintentos posteriores del agente. Única propuesta mínima para revisión humana: lectura **ec2:DescribeAddressesAttribute**, conservar límites de cuenta/región/usuario y acotar recursos según soporte IAM verificable antes de redactar/aplicar un Allow. **Sin JSON modificado, policy concedida ni Resource="*" justificado por inferencia**. No otras acciones/ec2:* o bypass de controles. Consultas a referencia pública no acreditaron soporte de recursos; no simulación/APIs adicionales de cuenta.

### 31.3. Inventario local después del fallo — no nueva verificación cloud

Datos derivados únicamente del apply/provider y lectura selectiva del archivo state; **no se ejecutaron state list/output o lecturas AWS de la fase condicionada a éxito**. State **serial27 /24managed /2tainted**, frente a16managed iniciales. Ocho recursos nuevos:

| Dirección | ID / resultado |
| --- | --- |
| aws_iam_role.api_runtime | enver-dev-api-runtime, Creationcomplete; boundary ARN exacto enstate |
| aws_iam_instance_profile.api_runtime | enver-dev-api-runtime, Creationcomplete; role agregado al profile, **sin asociación a EC2** |
| aws_iam_role_policy.api_ecr_pull | enver-dev-api-runtime:enver-dev-ecr-pull, Creationcomplete |
| aws_iam_role_policy.api_ssm_session | enver-dev-api-runtime:enver-dev-ssm-session, Creationcomplete |
| aws_network_interface.api["a"] | **eni-01ecdecf22b187764**, privada **10.20.0.63**, subnet-0120d869dd659e2a1, soloSGbackend |
| aws_network_interface.api["b"] | **eni-049a982368fb2bca8**, privada **10.20.1.99**, subnet-01934348f5d6985aa, soloSGbackend |
| aws_eip.api["a"] | **eipalloc-0c297551d49ce9416 /18.210.153.223**, asignada, **tainted**, sin asociación/ENI enstate |
| aws_eip.api["b"] | **eipalloc-0b54456ecfc2cdafa /44.195.4.217**, asignada, **tainted**, sin asociación/ENI enstate |

**0EC2/0asociacionesEIP/0rootEBS** deesteincremento enstate o logs de creación; no afirmar inexistencia universal en toda la cuenta. No instanceIDs/states/type/AZ/AMI/EBS reales dehosts que no se crearon. Dos t3a.small/AMIaprobada/dosAZ/10GiBgp3/IMDSv2 permanecen **PLANIFICADOS, NO MATERIALIZADOS**. Role/boundary/profile/inline creados segúnprovider/state, no auditoría cloud independiente de IAM o prueba funcional deSSM/ECRpull. **SSM A/B no consultado, Docker A/B no verificado, bootstrap no ejecutado en hosts, Enver.API NO desplegada/iniciada**.

**NETWORK14 y ECR2:** no-op enplan aplicado, mismos16recursos/IDs enstate; estructuras de esos recursos coinciden entre backup ystate actual. Sin operaciones de alteración de red/ECR enlog. **No nueva verificación cloud o post-plan0/0/0 acreditados**. SGbackend enstate conserva únicamente8080desdeSGALB/sinCIDRpúblico/SSH22; no prueba activa de red. No SG/ALB/ACM/Atlas/Redis/DynamoDB/secretos/API configurados por este intento.

### 31.4. State, costos y recuperación pendiente

State ignorado **serial27/24managed**, SHA256 **903066fbe35e43949d96382b2ca00a37760fe98b9671ac0ddc611f8dcd7f7a9c**. Backup nativo actualizado **serial18/16managed** (hash **bb18cf4c99d9e796193330940937c5e8619cc8cc365f83e05c92387dbb1e269f**), no byteidéntico al state previo hashb4497de9…; no custodia cifrada/controlada o recuperación aceptadas. **No restaurar backup de16 para ocultar recursos nuevos**, ni borrar state/plan. Las dosEIPtainted siguen asignadas: tainted **no significa liberadas ni coste0**.

Plan autorizado mantiene suhash, pero está **PARCIALMENTE CONSUMIDO / NO REAPLICAR**: el state ya cambió. La autorización de únicoapply está agotada; no generar ahora plan nuevo o auto-untaint/stateedit/import/reemplazo/destrucción. Las EIPtainted pueden originar propuestas de replacement en un futuro plan: cualquier lectura/reconciliación/recuperación, decisión de conservar/liberar IP y permisos requerirá revisión/autorización separadas; no actuar sobre ello aquí.

Preservar recovery previsto T010: Terraform/AMIcontrolada/bootstrapcomún/ENI-EIPpersistentes/estadocríticoexterno, sin promesa de doshosts durante reemplazo o recuperación ya probada. **No hay API/estado crítico desplegado en estos hosts**, dependencias externas futuras no se dan por operativas. No prueba destructiva ni rollback realizado.

Baseline de evaluación **INFRA-5≈USD2.3915/48h**, ALBfuturo≈1.944/conjunto4.3355/envolvente7.3355/guardrail10/proyecto50AGREGADOS conservada como estimación del incremento completo, **no consumo real tras fallo**. Ahora hay **2IPv4 públicas asignadas sin asociación, potencialmente facturables≈USD0.01/h agregado (≈USD0.48/48h soloIPv4 al precio baseline)**, sin EC2/EBS creados por este intento; Billing/consumo global/crédito no consultados. La ventana de48h iniciada conapply no debería exceder **2026-11-15T01:57:56.9337244Z**; revisión humana pronta de costo/retención/apagado requerida, **sin timer/topeAWS/limpiezaautomática o permiso de liberarIP**.

### 31.5. Estados y cierre por denegación

**T005 PARCIAL / IAM RUNTIME CREADO; VALIDACIÓN Y SSM PENDIENTES**: criterios de separación/minimización/dependencias/SSM no satisfechos integralmente. **T009 PARCIAL / APPLY PARCIAL BLOQUEADO; SIN EC2**. **T010 PARCIAL / RECOVERY DOCUMENTADO; PRUEBA/ACEPTACIÓN PENDIENTES**. **T025 PARCIAL / APPLY PARCIAL REGISTRADO; POST-PLAN/ACEPTACIÓN PENDIENTES**. **GATE-I-C PENDIENTE / NO SATISFECHO**: ni doshosts, mismo digest, capacidad/recuperación o prerrequisitos aceptados; todos9gatesabiertos/11parciales/19pendientes,002/004/AUTH-027integralintactos.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-authorized-apply-20261113-015650\`: prechecks/hash/show, únicoapply/resultados/denegación, resumen selectivo de state e integridad local y revisiónfinal. No secretos/state completo publicados; ubicación temporal no acredita retención/custodia. Solo task/memoria editados, **sin stage/commit/push**, HEAD3730ebe conservado. Fuentes HCL/JSON/bootstrap/.gitignore/lock y planes intactosporhash; state/backup cambiaron legítimamente porapplyparcial, no se afirman preservadoscomoantes. **SIN MÁS AWS/APPLY/POST-PLAN/DESTROY/IMPORT/IAMMANUAL, sin app. DETENER para revisión humana.**

## 32. INFRA-5 — lecturas de recovery; plan bloqueado por exclusión Git

### 32.1. Autorización, baseline y permiso corregido

**2026-11-13 UTC — RECUPERACIÓN DE APPLY PARCIAL, SIN APPLY.** Responsable informa que agregó manualmente `ec2:DescribeAddressesAttribute` al usuario `enver-terraform-local`; no autoriza ampliaciones automáticas. Se permiten lecturas, validaciones locales, un nuevo plan condicionado a exclusión Git y actualizar solo task/memoria. No reutilizar `infra/tfplan-infra5`, modificar taint/state, destruir/importar/liberar EIP o stage/commit/push.

Precheck real: **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, únicamente task/memoria modificados por §31, **0staged/diff-check exit0**. State **serial27/24managed/2EIPtainted**, sin EC2/asociaciones gestionadas del incremento. No se revierte la documentación previa ni se modifica HCL/JSON/bootstrap/.gitignore/lock.

Lectura exacta `aws ec2 describe-addresses-attribute --allocation-ids eipalloc-0c297551d49ce9416 eipalloc-0b54456ecfc2cdafa --attribute domain-name --region us-east-1 --profile enver-infra`, **2026-11-13T02:51:41.1114401Z–02:51:44.8447851Z**, **exit0/sin AccessDenied**. Devuelve **Addresses:[]**: acredita que esta llamada ya no está denegada, no atributos presentes ni por sí sola existencia de EIP. DescribeAddresses separado confirma su existencia. La corrección manual es fuente del responsable; no se audita la versión/hash/contenido exacto de la policy operadora o sus permisos agregados. Perfil/región externos temporales restaurados, máximo1intento CLI, sin credenciales alternativas.

### 32.2. Inventario confirmado sin escrituras

`terraform -chdir=infra state list` **exit0**: **24managed +3data**, desglosados en **NETWORK14/ECR2/IAM4/ENI2/EIP2**. Inspección local selectiva confirma **aws_eip.api["a"] y ["b"] siguen tainted**. No `untaint` ni state completo publicado.

Seis lecturas AWS adicionales, todas **exit0**: **DescribeAddresses, DescribeNetworkInterfaces, GetRole, GetInstanceProfile y GetRolePolicy dos veces**. Junto con la lectura de atributo son siete comandos AWS de solo lectura, **sin AccessDenied en este incremento**.

| Recurso | Observación real AWS |
| --- | --- |
| EIP A | **eipalloc-0c297551d49ce9416 /18.210.153.223**, domainvpc/us-east-1, sin AssociationId/NetworkInterfaceId |
| EIP B | **eipalloc-0b54456ecfc2cdafa /44.195.4.217**, domainvpc/us-east-1, sin AssociationId/NetworkInterfaceId |
| ENI A | **eni-01ecdecf22b187764 /10.20.0.63**, us-east-1a, available/sin attachment, subnet-0120d869dd659e2a1 |
| ENI B | **eni-049a982368fb2bca8 /10.20.1.99**, us-east-1b, available/sin attachment, subnet-01934348f5d6985aa |
| SG de ambas ENI | Solo **sg-070d06cfebe866427** (backend); sin cambios realizados |
| Runtime role | **arn:aws:iam::926925605666:role/enver-dev-api-runtime**, existente |
| Boundary del role | **arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary**, ARN exacto confirmado por GetRole; custodia/contenido/defaultversion remotos no auditados |
| Instance profile | **enver-dev-api-runtime**, contiene únicamente role enver-dev-api-runtime; sin EC2 del incremento asociadas |
| Inline policies | **enver-dev-ecr-pull/enver-dev-ssm-session**, ambas recuperadas por GetRolePolicy; sin validación efectiva de ECRpull/SSM |

### 32.3. Validaciones y detención antes del plan

**`terraform fmt -check -recursive infra`, `terraform -chdir=infra validate -no-color`, `bash -n infra/bootstrap/compute-host.sh`: exit0/0/0.** Bash solo comprueba sintaxis, no ejecuta bootstrap. Sin init/upgrade/instalación/API/SSM/Docker.

**`git check-ignore -v -- infra/tfplan-infra5-recovery`: exit1/sin regla**, ruta inexistente/no tracked. **DETENER antes de crear**, como exige la autorización. No modificación automática de .gitignore, plan alternativo con otro nombre/extensión, ni generación para eludir la condición. **Recovery plan NO creado; comando plan NO ejecutado; exitcode/add/change/destroy/replacements no disponibles.** No show del plan inexistente ni apply de ningún plan.

**No es posible acreditar nuevos no-op IAM/ENI/NETWORK14/ECR2**, acciones de EC2/asociaciones o conservación/replacement de EIP sin el plan real. El taint observado sugiere riesgo de replacement, **no prueba una propuesta concreta ni que su única razón sea taint**. Orden destroy/create, impacto en IP/costos y futuro boot requieren inspección del plan posterior autorizado. Conceptualmente faltan2EC2/2asociaciones, pero **no son un conteo de acciones planificadas acreditado**. HCL conserva2t3a.small/AMIstrategy/dosAZ/10GiBgp3cifrado/IMDSv2/8080soloSGALB/sinSSH; no nueva resoluciónAMI o validación cloud de reglas. El plan consumido §31 sigue **NO REAPLICABLE** aunque conserve el hash.

### 32.4. Costos, estados y límites del cierre

AWS confirma **2IPv4 públicas asignadas sin asociación**, potencialmente facturables **≈USD0.01/h agregado /≈USD0.48por48h soloIPv4** al precio baseline. Billing/costo acumulado/crédito **no consultados**. Estimación de incremento completo≈USD2.3915/48h, guardrailUSD10 y presupuestoUSD50AGREGADOS no son gasto medido ni topes automáticos. No extender por este bloqueo la ventana inicial: máximo registrado **2026-11-15T01:57:56.9337244Z**; responsable debe revisar prontamente retención/costo y continuación. **Sin liberaciónIP/apagado/limpieza automática autorizados**.

**T005 PARCIAL**, IAM existente confirmado, validación integral/SSM pendientes. **T009 PARCIAL / RECOVERY PENDIENTE**, plan bloqueado por ignore. **T010 PARCIAL**, sin recuperación probada. **T025 PARCIAL**, validaciones locales/lecturas acreditadas, recovery plan/post-plan/aceptación pendientes. **GATE-I-C PENDIENTE / NO SATISFECHO**, nueve gates abiertos/11tareasparciales/19pendientes. Sin doshosts/digest/cloudAuth, despliegue API o avances de002/004/AUTH-027integral por inferencia.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery-review-20261113-025116\`: precheck/hashes, lecturaatributo/resultados, statelist, lecturasAWS/resumenselectivo, validaciones, ignoreprecheck y revisiónGit final. Ubicación temporal no acredita custodia duradera. **Solo task/memoria actualizados, sin stage/commit/push ni escriturasAWS/IAM**. State/backup/planes previos conservados; sin alteración de taint. **DETENIDO por ignore: para continuar, autorización específica de exclusión/ruta y nuevo plan; no apply implícito.**

### 32.5. Continuación autorizada — exclusión exacta y recovery plan real, SIN APPLY

**2026-11-13 UTC — AUTORIZAR EXCLUSIÓN DE RECOVERY PLAN Y CONTINUAR RECOVERY PLAN.** Se conserva §32.1–32.4 como evidencia del bloqueo anterior. La nueva autorización permite únicamente añadir la regla exacta en .gitignore, validar, generar/inspeccionar un nuevo plan y sincronizar task/memoria. **No acepta replacements ni autoriza apply, destroy/import/untaint/stateedit/liberación EIP, cambios IAM/escrituras AWS o stage/commit/push.**

**Baseline real:** feature/infrastructure / **3730ebe2cd0c7851bcd827b1f73e355a673db61a**, inicialmente solo task/memoria modificados, 0staged/diff-check exit0. State **24managed/serial27/2EIPtainted**. No se sobrescribe un plan recovery previo: la ruta aún no existía.

**Única regla nueva:** **`/infra/tfplan-infra5-recovery`**, línea95; `/infra/tfplan-infra5` y todas las demás reglas intactas. `git check-ignore -v -- infra/tfplan-infra5-recovery` **exit0/.gitignore:95** antes de crear, no tracked. Sin wildcard nuevo o ampliación a otras rutas. Fmt-check/validate-no-color/bash-n **exit0/0/0**, sin init/upgrade o ejecución de bootstrap. `terraform -chdir=infra state list` **exit0/24managed +3data**: NETWORK14/ECR2/IAM4/ENI2/EIP2, 0EC2/0asociaciones gestionadas del incremento.

**Único nuevo plan:**

```text
terraform -chdir=infra plan -out=tfplan-infra5-recovery -detailed-exitcode -var="runtime_permissions_boundary_arn=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"
```

**2026-11-13T03:01:10.4266796Z–03:01:30.2298220Z**, duración **19.792s**, **exit2 /6add/0change/2destroy**, **2 replacements +4create puros +22no-op**, sin delete puros. Las6altas incluyen las dos recreaciones: incremento neto propuesto4, no seis recursos netos nuevos. **No AccessDenied**. Terraform1.17.1/AWS6.73.0 conservados. Perfil **enver-infra**, región **us-east-1**, entradas aprobadas en entorno temporal restaurado: enver/dev/cuenta926925605666/VPC10.20.0.0/16/newbits8/common_tags{}/network_design_approvedtrue/compute_plan_approvedtrue/compute_ami_idnull, boundary exacta por-var. Sin realtfvars/credenciales alternativas o plan anterior utilizado.

Binario privado **`infra/tfplan-infra5-recovery`**, SHA256 **`f4bfdd7fa8ef316f63cb26b9576c05ce4f67d717ac148fbcd5a2a63c34fa3f7e`**. Show humano y JSON del mismo archivo **exit0/0**; no plan/stdout JSON completo publicados ni stage del binario.

| Recurso | Acción real propuesta | Razón / impacto |
| --- | --- | --- |
| aws_eip.api["a"] | **delete → create, replacement SÍ** | **replace_because_tainted**; humano: is tainted, so must be replaced. Se destruiría eipalloc-0c297551d49ce9416 /18.210.153.223; nueva asignación/IP desconocidas |
| aws_eip.api["b"] | **delete → create, replacement SÍ** | **replace_because_tainted**; mismo texto humano. Se destruiría eipalloc-0b54456ecfc2cdafa /44.195.4.217; nueva asignación/IP desconocidas |
| aws_instance.api["a"] | **create** | EC2 futura con ENI existente eni-01ecdecf22b187764 |
| aws_instance.api["b"] | **create** | EC2 futura con ENI existente eni-049a982368fb2bca8 |
| aws_eip_association.api["a"] | **create** | AllocationId futuro calculado de la EIP reemplazada; ENI existente A |
| aws_eip_association.api["b"] | **create** | AllocationId futuro calculado de la EIP reemplazada; ENI existente B |
| Role/profile/inline2 | **4no-op** | Boundary ARN exacta conservada; no cambio IAM propuesto |
| ENI A/B | **2no-op** | IDs/privadas existentes conservadas |
| NETWORK14 / ECR2 | **14no-op /2no-op** | Sin cambios de infraestructura red/ECR propuestos |

**Razón EIP:** ambas `action_reason=replace_because_tainted`, `replace_paths` sin rutas; domainvpc/tags sin cambios. Terraform atribuye el replacement al **taint**, no señala otro atributo que fuerce sustitución. Esto no implica que untaint sea apropiado o esté autorizado. Orden por EIP **destroy-then-create**, no create_before_destroy. No se acepta perder las IP por el solo hecho de existir el plan; revisión humana pendiente. Si se aplicara, futuras EC2 arrancarían tras sus asociaciones usando nuevas EIP, no las dos públicas actuales; afectaría referencias/allowlists futuras. No Atlas ni aplicación configurados aquí.

**Diferencias de refresh en JSON, separadas de acciones propuestas:** tres entradas `resource_drift/update`: (1) EIP A `ptr_record: null → ""`; (2) EIP B igual; (3) role `inline_policy: [] → [enver-dev-ecr-pull, enver-dev-ssm-session]`. El role sigue **no-op enresource_changes**; sus inline se gestionan por recursos propios también no-op. Son diferencias entre state previo y lectura del provider, compatibles con atributos incompletos del apply parcial; **no prueban por sí solas modificaciones externas nuevas ni son escrituras IAM ejecutadas/propuestas**. No ocultarlas ni afirmar drift0. Refresh del snapshot del plan no alteró state/backup local: hashes conservados.

**Compute inspeccionado en el plan:** dos **t3a.small**, AMI **ami-096ef200a877f36af**, AZ **us-east-1a/b**, raíz **10GiBgp3cifrado/3000IOPS/125MiB/s**, CPUstandard, **IMDSv2required/hop1**, profile enver-dev-api-runtime, ENI prevista porhost, sin key_name. Ambos user_data coinciden exactamente con bootstrap revisado, solo preparaciónDocker/SSM, sin Enver.API/ECRpull/run/secretos. Backend8080 soloSGALB/sinCIDR públicos, **sin SSH22 o8080 públicos**. Esto es evidencia deplan, no doshosts/SSM/Docker/runtime probados.

**Costo:** las dos IPv4 actuales continúan asignadas/tainted y potencialmente facturables **≈USD0.01/h agregado /≈USD0.48por48h soloIPv4**, no Billing consultado. El replacement propuesto liberaría las anteriores y asignaría otras, con costos/tiempos de transición no medidos; no prometer ahorro, coste0 o conservaciónIP. Tipo/cantidad final previstos no cambian la estimación baseline completa≈USD2.3915/48h/guardrailUSD10/proyectoUSD50AGREGADOS. **No se reinicia la ventana original**, máximo **2026-11-15T01:57:56.9337244Z**; costos acumulados, retención y actuación humana siguen pendientes, sin timer/limpieza automática.

**Estados:** T005 **PARCIAL**, IAMno-op no valida runtime/SSM/custodia integral. T009 **PARCIAL / RECOVERY PENDIENTE**, dos replacements EIP **NO ACEPTADOS**. T010 **PARCIAL**, recovery no aplicado/probado. T025 **PARCIAL / RECOVERY PLAN INSPECCIONADO**, post-plan/aceptación integral pendientes. **GATE-I-C PENDIENTE / NO SATISFECHO**, nueve gates abiertos,11parciales/19pendientes;002/004/AUTH-027integral intactos. State sigue **serial27/24managed/2tainted**, no28managed materializados.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery-plan-20261113-025921\`: baseline/hashes, regla/ignore, validaciones/statelist, plan/resultados/show/análisisselectivo/diferenciasrefresh, seguridad/integridad y revisiónGitfinal. Temporal, no custodia duradera aceptada. Fuentes salvo **.gitignore/task/memoria**, state/backup y planes anteriores preservados porhash; binario nuevo ignorado/no tracked. **Sin stage/commit/push, apply/AWS WRITE/IAM modificado/untaint/stateedit o app; HEAD3730ebe intacto. DETENER para revisión humana del plan y sus replacements, sin ejecución implícita.**

### 32.6. Recuperación conservadora autorizada — EIP sin taint; recovery2 bloqueado por ignore

**2026-11-13 UTC — RECUPERACIÓN CONSERVADORA DE EIP TAINTED.** La causa original acreditada fue lectura `ec2:DescribeAddressesAttribute` denegada después de AllocateAddress (§31); el permiso ya funcionaba (§32), y el plan §32.5 proponía replacements por `replace_because_tainted`, sin otra ruta de atributo forzante. La autorización actual permite corregir únicamente esas marcas, tras validar AWS/state. **No autoriza apply ni recreación/liberación de IP, destroy/import, IAM o edición manual de state.**

Precheck real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, solo.gitignore/task/memoria modificados previamente, **0staged/diff-checkexit0**. State **serial27/24managed/2tainted**, ruta recovery2 inexistente. No se modifica .gitignore en este incremento.

**Cinco lecturas AWS exitosas, sin AccessDenied:** STS GetCallerIdentity, DescribeAddresses de A y de B por separado, DescribeAddressesAttribute de ambas con domain-name, DescribeNetworkInterfaces de ambas ENI. Perfil **enver-infra**, región **us-east-1**, identidad exacta **arn:aws:iam::926925605666:user/enver-terraform-local/cuenta926925605666**. EIP consultadas en la cuenta/región esperadas, NetworkBorderGroupus-east-1; sin credenciales alternativas, entorno temporal restaurado. DescribeAddressesAttribute **exit0/Addresses:[]**, permiso ya no denegado; existencia validada por las consultas separadas, no inferida de la lista vacía.

| EIP | AWS y state antes del untaint | Asociación real observada |
| --- | --- | --- |
| A /aws_eip.api["a"] | **eipalloc-0c297551d49ce9416 /18.210.153.223 /domainvpc /tainted enstate** | Sin AssociationId, NetworkInterfaceId, PrivateIpAddress ni InstanceId |
| B /aws_eip.api["b"] | **eipalloc-0b54456ecfc2cdafa /44.195.4.217 /domainvpc /tainted enstate** | Sin AssociationId, NetworkInterfaceId, PrivateIpAddress ni InstanceId |

Tags AWS: **Name=enver-dev-api-a-eip/b-eip, Project=enver, Environment=dev, ManagedBy=Terraform**, coherentes con state/configuración. Ambas allocations/IP/domain coinciden exactamente con state; sin asociación inesperada. ENI **eni-01ecdecf22b187764/10.20.0.63/us-east-1a** y **eni-049a982368fb2bca8/10.20.1.99/us-east-1b** existen, OwnerId926925605666, **available/sin attachment**, soloSGbackend **sg-070d06cfebe866427**. Verificaciones completadas antes de modificar marcas, sin escritura AWS.

**Dos comandos nativos Terraform, uno por dirección exacta**, quoting adaptado a Windows sin cambiar recursos:

| Comando lógico | Intervalo UTC | Exit / resultado |
| --- | --- | --- |
| terraform -chdir=infra untaint 'aws_eip.api["a"]' | **2026-11-13T03:13:00.6595699Z–03:13:05.7015571Z** | **0**, successfully untainted; state **serial28/24managed/1tainted(B)** |
| terraform -chdir=infra untaint 'aws_eip.api["b"]' | **2026-11-13T03:15:52.9323814Z–03:15:58.0940708Z** | **0**, successfully untainted; state **serial29/24managed/0tainted** |

**Pausa de control después de A:** hash inicial sensible al orden difería, por lo que no se ejecutó B inmediatamente. Inspección local de backup nativo/current identifica reordenación de listas **check_results/objects**, sin cambios semánticos de sus resultados; al ordenar solo esas listas en memoria y excluir serial/dosstatus autorizados, backup/current coinciden. **A exit0 no falló ni se repitió**, no se editó/restauró state para resolver el check. Se continuó B solo tras confirmar que todos los atributos/recursos estaban intactos. La misma comprobación semántica pasó después de B; única diferencia funcional: eliminación de dos marcas taint, más serialización/serial nativos de Terraform. Ningún recurso distinto modificado.

**State final:** **serial29/24managed/0tainted**, allocation/IP/domain sin cambios, **0EC2/0asociaciones gestionadas**. SHA256 **2cb2b851babc4a92397bbf10790c7a29f479ad5d3344773f80b62d7b4921e03a**. Backup nativo ahora **serial28/24managed** (A sin taint/B aún tainted), SHA256 **3d14d3caf9372c64b6eba16328bfe9cff415a922fd1581dd393d94461c8005d6**. State/backup cambian legítimamente por los untaint, no se afirma preservación byteprevia; custodia protegida/backup/recuperación integral siguen pendientes. No restaurar un backup que revierta marcas ni editarlo manualmente.

**Bloqueo antes del nuevo plan:** `git check-ignore -v -- infra/tfplan-infra5-recovery2` **exit1/sin regla**. Archivo inexistente/no tracked. **DETENIDO**, sin modificar .gitignore, elegir otra ruta para eludir la condición o ejecutar plan/show de recovery2. **Recovery2 NO creado, sin SHA256/exitcode/add-change-destroy/replacements disponibles.** No acreditar todavía4/0/0/0replacements o EIP/ENI/IAM/NETWORK/ECR no-op nuevos; las altasEC2/asociaciones siguen conceptuales hasta plan real. Los binarios **tfplan-infra5 y tfplan-infra5-recovery** se conservan por hash como históricos, **NO REUTILIZAR**: el primero parcialmente consumido; el segundo ahora obsoleto respecto al state tras untaint.

**Seguridad y runtime:** ausencia públicaSSH22/8080 y compute esperado constan en el plan histórico §32.5, no en un recovery2 inexistente. ENI existentes conservadas, no nuevos hosts/roots/SSM/Docker/API/secretos. No nueva verificación cloud deSG o resoluciónAMI, no afirmar recuperación completa.

**Costo conservado:** baseline INFRA-5 **≈USD2.3915/48h**, incluye las dosIPv4 existentes; su costo parcial estimado sigue **≈USD0.01/h agregado/≈USD0.48por48h soloIPv4**. GuardrailUSD10/presupuestoUSD50AGREGADOS, no Billing/crédito/consumo real consultados. **No reiniciar la ventana**, máximo original **2026-11-15T01:57:56.9337244Z**; sin timer/apagado/liberación deIP o gasto adicional autorizado por corregir marcas.

**Estados:** T005 **PARCIAL**, IAM/runtimeSSM/custodia integral pendientes; T009 **PARCIAL / RECOVERY EN CURSO**, EIP conservadas sin taint, recovery2 bloqueado por ignore; T010 **PARCIAL**, corrección de marcas no equivale a prueba de recuperación dehosts; T025 **PARCIAL**, AWS/state/untaint acreditados, plan/post-plan/aceptación pendientes. **GATE-I-C PENDIENTE / NO SATISFECHO**, nueve gates abiertos/11parciales/19pendientes,002/004/AUTH-027integral intactos.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-conservative-eip-recovery-20261113-031056\`: precheck/hashes, lecturasAWS/comparaciónselectiva, resultadosuntaintA-B/revisiónsemántica/poststate, ignore y revisiónGitfinal. Ubicación temporal no acredita custodia duradera; sin state completo/secretos publicados. En este turno solo task/memoria editados; .gitignore conserva su cambio previo sin ampliación, demás fuentes/planes anteriores intactos porhash. **Sin stage/commit/push, apply/destroy/import/AWS WRITE/IAM modificado o app. DETENER: requiere autorización específica para exclusión de recovery2 y continuación del plan, sin apply implícito.**

### 32.7. Exclusión exacta y recovery2 guardado — 4/0/0, sin replacements ni apply

**2026-11-13 UTC — AUTORIZAR EXCLUSIÓN DE RECOVERY2 Y GENERAR PLAN LIMPIO POST-UNTAINT.** Resuelve únicamente el bloqueo de ignore registrado en §32.6. Autoriza nueva regla exacta, validaciones, plan y evidencia documental. **No apply/destroy/import/taint/untaint adicional, modificación de state/IAM, escrituras AWS o stage/commit/push.** Historial de apply parcial, lectura denegada y untaint conservado; no aprobación retrospectiva de gates.

**Baseline:** feature/infrastructure / **3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cambios previos.gitignore/task/memoria, **0staged/diff-checkexit0**. State **serial29/24managed/0tainted**; allocations conocidas y ruta recovery2 inexistente antes de crear. **Única regla añadida en este turno: `/infra/tfplan-infra5-recovery2`**, línea96; reglas `/infra/tfplan-infra5`, `/infra/tfplan-infra5-recovery` y todas las demás intactas, sin wildcard. `git check-ignore -v -- infra/tfplan-infra5-recovery2` **exit0/.gitignore:96**, antes de generar/no tracked.

**Validaciones reales:** `terraform fmt -check -recursive infra`, `terraform -chdir=infra validate -no-color`, `bash -n infra/bootstrap/compute-host.sh` **exit0/0/0**; sin init/upgrade/instalar/ejecutar bootstrap. `terraform -chdir=infra state list` **exit0/24managed +3data**, NETWORK14/ECR2/IAM4/ENI2/EIP2; no EC2/asociaciones gestionadas aún.

**Único plan nuevo:**

```text
terraform -chdir=infra plan -out=tfplan-infra5-recovery2 -detailed-exitcode -var="runtime_permissions_boundary_arn=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"
```

**2026-11-13T03:31:24.6692164Z–03:31:45.1417059Z**, duración **20.464s**, **exit2 /4add/0change/0destroy/0replacements**. **4create puros/24no-op**, sin actualización, baja o sustitución propuesta. Sin AccessDenied/reintentos o permisos ampliados. Terraform1.17.1/AWS6.73.0 conservados. Perfil **enver-infra/us-east-1**, entradasbaseline aprobadas en entorno temporal restaurado: enver/dev/cuenta926925605666/VPC10.20.0.0/16/newbits8/common_tags{}/network_design_approvedtrue/compute_plan_approvedtrue/compute_ami_idnull, boundary exacta por-var. Sin realtfvars o credenciales alternativas.

**Binario privado `infra/tfplan-infra5-recovery2`**, SHA256 **`cd2a027af2861692b8eeb90174b021660abdb48a5179a49f38a043604c0e79eb`**, ignorado/untracked. Show humano y JSON del mismo binario **exit0/0**, conteo basado en acciones reales, no expectativa conceptual.

| Recurso | Acción real | Identidad / asociación conservada o propuesta |
| --- | --- | --- |
| aws_eip.api["a"] | **no-op** | **eipalloc-0c297551d49ce9416 /18.210.153.223 /vpc**, sin nueva EIP |
| aws_eip.api["b"] | **no-op** | **eipalloc-0b54456ecfc2cdafa /44.195.4.217 /vpc**, sin nueva EIP |
| aws_network_interface.api["a"] | **no-op** | **eni-01ecdecf22b187764 /10.20.0.63** |
| aws_network_interface.api["b"] | **no-op** | **eni-049a982368fb2bca8 /10.20.1.99** |
| Role/profile/inline2 | **4no-op** | enver-dev-api-runtime, boundaryARNexacta y dosinline conservados |
| NETWORK14 / ECR2 | **14no-op /2no-op** | Sin cambios de red/ECR |
| aws_instance.api["a"] /["b"] | **create /create** | Doshosts futuros con ENI A/B existentes |
| aws_eip_association.api["a"] | **create** | **eipalloc-0c297551d49ce9416 ↔ eni-01ecdecf22b187764** |
| aws_eip_association.api["b"] | **create** | **eipalloc-0b54456ecfc2cdafa ↔ eni-049a982368fb2bca8** |

Las asociaciones usan allocations **conocidas y originales**, allow_reassociationfalse; **no nuevas EIP, recreación ni sustitución**. La corrección autorizada de taint en §32.6 queda respaldada por este nuevo plan, pero no se repite ningún untaint. Netofuturo4/28managed es solo propuesta: actual sigue24managed y no se han asociado IP o creado hosts.

**Compute inspeccionado:** dos **t3a.small**, AMI resuelta **ami-096ef200a877f36af**, AZ **us-east-1a/b**, raíces **10GiBgp3cifradas/3000IOPS/125MiB/s**, CPUstandard, **IMDSv2required/hop1**, profileenver-dev-api-runtime, sin key_name, ENI prevista porhost/soloSGbackend. Subnets conservan map_public_ip_on_launchfalse. **Backend8080 solo desde SGALB sg-0dacf0df5da30b4f7; sin SSH22 o8080públicos.** Ambos user_data coinciden con bootstrap revisado, sin secretos detectados, ECRlogin/pull/run o autoarranqueEnver.API; solo prepararDocker/SSM. No prueba runtime/bootstrap/health o seguridad efectiva de hosts que aún no existen.

**Refresh separado de acciones:** JSON conserva **3 entradas resource_drift/update**: EIP A/B `ptr_record` y role `inline_policy`, como §32.5 (null→vacío y listado de dosinline existentes). **Los tres recursos son no-op enresource_changes**, no updates a ejecutar. Diferencias entre snapshot previo y lectura de provider no prueban por sí solas una nueva modificación externa; **no afirmar drift0** por el4/0/0. Sin corrección manual ni persistencia del snapshot refrescado sobre state/backup local.

**Integridad y límites:** state **serial29/24managed/0tainted**, SHA256 **2cb2b851babc4a92397bbf10790c7a29f479ad5d3344773f80b62d7b4921e03a**; backup **serial28**, hash **3d14d3caf9372c64b6eba16328bfe9cff415a922fd1581dd393d94461c8005d6**, y planes previos intactos porhash. Los planes **tfplan-infra5** parcialmenteconsumido y **tfplan-infra5-recovery** obsoleto **NO REUTILIZAR**. Recovery2 es candidato PRE-APPLY, **no autorización de apply** ni garantía de permisos de escritura/capacidad/quotas/custodia/aceptación integral.

**Costo:** baseline INFRA-5 **≈USD2.3915/48h** incluye las dosEIP actuales, parcialIPv4≈USD0.01/hagregado/≈USD0.48por48h. **GuardrailUSD10/proyectoUSD50AGREGADOS**, no Billing/crédito/costoacumulado consultados. **Ventana original NO reiniciada**, máximo **2026-11-15T01:57:56.9337244Z**; revisión humana de retención/costos/actuación sigue pendiente, sin timer/apagado/liberaciónIPautomática. Guardar plan no autoriza nuevos cargos deprovisión.

**Estados:** T005 **PARCIAL**, IAMno-op no valida runtimeSSM/ECRpull/custodia integral. T009 **PARCIAL / RECOVERY2 PRE-APPLY ACREDITADO**, sin doshosts ni aceptación. T010 **PARCIAL**, planificación conservadora/marcas recuperadas no son prueba de recuperación dehosts. T025 **PARCIAL / RECOVERY2 INSPECCIONADO**, post-plan/aceptación pendientes. **GATE-I-C PENDIENTE / NO SATISFECHO**, nueve gates abiertos/11parciales/19pendientes;002/004/AUTH-027integral intactos.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery2-plan-20261113-033003\`: baseline/hashes, regla/ignore, validaciones/statelist, plan/resultados/show/análisisselectivo/integridad y revisiónGitfinal. Temporal, no custodia duradera aceptada. Solo **.gitignore/task/memoria** modificados, demásfuentes/state/backup/planesanteriores intactosporhash, binario nuevo excluido/no tracked. **HEAD3730ebe intacto/0staged/sincommit/push, sinapply/destroy/import/taint/untaint/IAM changes/AWS WRITE o app. DETENER para revisión humana del nuevo plan, sin siguiente incremento automático.**

### 32.8. Apply controlado recovery2 — fallo parcial, asociaciones creadas y EC2 rechazadas

**2026-11-13 UTC — autorización posterior explícita APPLY CONTROLADO RECOVERY2.** Solo aplicar el binario exacto, sin generar un nuevo plan para aplicar ni reutilizar `tfplan-infra5`/`tfplan-infra5-recovery`. Inventario, SSM/Docker y post-plan estaban condicionados a éxito. El fallo parcial activa la detención; no se amplía el alcance para diagnosticar/corregir la cuenta, cambiar tipo EC2 o permisos.

**Pre-check real:** `git branch --show-current` → **feature/infrastructure**; `git rev-parse HEAD` → **3730ebe2cd0c7851bcd827b1f73e355a673db61a**; `git status --short` → únicamente **.gitignore, memoria.md y specs/003-infrastructure/task.md modificados**, **0 staged**. No modificación del índice. STS exit0 acredita cuenta **926925605666**, usuario **enver-terraform-local**, perfil **enver-infra**, región **us-east-1**; sin credenciales alternativas de entorno.

**Hash y plan:** SHA256 calculado **cd2a027af2861692b8eeb90174b021660abdb48a5179a49f38a043604c0e79eb**, coincide exactamente. `terraform -chdir=infra show tfplan-infra5-recovery2` (presentación sin color) y `show -json` **exit0/0**. Acciones gestionadas: **4 create /24 no-op /0 change /0 destroy /0 replacements**. Únicas altas: `aws_instance.api["a"]`, `aws_instance.api["b"]`, `aws_eip_association.api["a"]`, `aws_eip_association.api["b"]`. IAM4/NETWORK14/ECR2/ENI2/EIP2 **no-op**; allocations y ENI originales exactas. No plan nuevo previo al apply.

**Incidencias locales resueltas antes de apply:** `Get-FileHash` no estaba disponible; ese intento no calculó hash y la guarda detuvo el comando antes de show (no fue una discrepancia real del archivo). El cálculo SHA256 con .NET y luego Node confirmó coincidencia. Python no disponible detuvo otro wrapper antes de cualquier acción Terraform/AWS; Node permitió continuar sin instalar herramientas. No se ejecutó ni repitió apply por estas incidencias.

**Única ejecución:**

```text
terraform -chdir=infra apply tfplan-infra5-recovery2
```

**Inicio 2026-11-13T04:28:06.433Z; fin 2026-11-13T04:28:24.409Z; duración17.971s; exit1.** No resumen `Apply complete!`; no declarar4recursos creados ni éxito. Registro nativo confirma **2 altas completadas**, **0 cambios/0 destrucciones registrados**, y **2 errores RunInstances**. **AccessDenied: NO**. No segundo apply ni reintento automático.

| Recurso | Resultado nativo real |
| --- | --- |
| aws_eip_association.api["a"] | Creation complete after2s, **eipassoc-0d50801839a9cae11**; plan exacto vincula **eipalloc-0c297551d49ce9416 /18.210.153.223 ↔ eni-01ecdecf22b187764 /10.20.0.63** |
| aws_eip_association.api["b"] | Creation complete after2s, **eipassoc-0717a47e226e64bda**; plan exacto vincula **eipalloc-0b54456ecfc2cdafa /44.195.4.217 ↔ eni-049a982368fb2bca8 /10.20.1.99** |
| aws_instance.api["a"] | **EC2 RunInstances HTTP400 /InvalidParameterCombination**, RequestID **d7f7a1b0-b923-42cb-abe9-abc16df99300**; sin InstanceId/creación completada |
| aws_instance.api["b"] | **EC2 RunInstances HTTP400 /InvalidParameterCombination**, RequestID **d2a48acb-7be7-4cab-9e50-4bf65ae595ac**; sin InstanceId/creación completada |

Mensaje de AWS para ambas: **“The specified instance type is not eligible for Free Tier. For a list of Free Tier instance types, run 'describe-instance-types' with the filter 'free-tier-eligible=true'.”** No ejecutar esa recomendación automáticamente, inferir el tipo alternativo o atribuir el fallo a IAM/cuotas/estado de Billing sin evidencia. Tipo autorizado **t3a.small**, AMI **ami-096ef200a877f36af**, AZ A/B **us-east-1a/us-east-1b**; son atributos del plan, **no de EC2 reales verificadas**.

**Detención efectiva:** no `state list`, consultas post-apply EC2/EBS/ENI/EIP/SG/IAM, SSM, Docker ni post-plan. **Conteo managed actual NO VERIFICADO** (último inventario previo24, no sustituirlo por28esperado o26inferido). **0 creaciones EC2 completadas según Terraform; conteo AWS posterior no consultado**. Estados/InstanceId/volúmenes/perfil efectivo A/B no disponibles. Raíces10GiBgp3cifradas e IMDSv2required solo inspeccionados en plan, no comprobados en hosts. SG sinSSH22/8080públicos y8080soloSGALB constan en plan/evidencia previa, no revalidados tras fallo. IAM role/profile **enver-dev-api-runtime**, boundary **arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary** y inlineECRpull/SSMmínimo son no-op inspeccionados y lecturas previas, no nueva validación remota. SSM A/B y Docker A/B **NO VALIDADOS**.

**Conservación y alcance:** el apply no ejecutó reemplazo/liberación de allocations EIP ni sustitución ENI, y completó las asociaciones sobre las identidades originales. IAM/NETWORK14/ECR2 sin acciones de cambio en el plan ni modificaciones registradas en apply; **sin post-plan no afirmar drift0 o integridad remota revalidada**. State/backup pueden cambiar legítimamente por las altas parciales; no se editan manualmente ni se afirma que conservan hashes anteriores. **Recovery2 quedó parcialmente consumido: NO REAPLICAR**. Tampoco reutilizar los dos planes históricos; una eventual continuación requiere nueva autorización específica y revisión de baseline/evidencia, sin plan correctivo generado aquí.

**Enver.API NO iniciada por esta ejecución**: ningún deployment, docker run, login/pull o configuración runtime de Atlas/Redis/DynamoDB/JWT/Secrets Manager. No SSH, apertura de puertos, ALB, destroy/import/taint/untaint, modificación IAM o limpieza. La recuperación parcial **NO está completada**; solo se acredita avance de asociaciones sin reemplazar EIP/ENI y fallo explícito del cómputo.

**Costo estimado conservado:** INFRA-5 **≈USD2.3915/48h**, guardrail **USD10**, proyecto **USD50 AGREGADOS**; no Billing/costo acumulado real consultados. Las EIP existentes continúan sin liberarse, parcialIPv4 estimado≈USD0.01/hagregado. **Ventana original NO se reinicia**, máximo **2026-11-15T01:57:56.9337244Z**; no nuevo timer/apagado/cleanup ni aceptación de permanencia/costo real.

**Reevaluación estricta:** **T-INFRA-005 PARCIAL**, mínimo privilegio/separación integral, profile en doshosts y SSM no satisfechos; **T-INFRA-009 PARCIAL / APPLY RECOVERY2 FALLIDO**, sin dosEC2reales verificadas/capacidad/digest; **T-INFRA-010 PARCIAL**, recuperación incompleta, sin prueba destructiva/reemplazo real/runbook aceptado; **T-INFRA-025 PARCIAL / APPLY RECOVERY2 FALLIDO**, sin post-plan ni custodia/backup protegido/recovery/dependencias aceptadas. **GATE-I-C PENDIENTE / NO SATISFECHO**: exige dosEC2/dosAZ/mismo digest/capacidad/recuperación, E007–010/E027–028 y gatesI-A/I-B, no cubiertos por asociaciones. **Nueve gates abiertos;11tareas parciales/19pendientes**. Sin cambios002/004/AUTH-027integral o aceptación productiva.

**Evidencia privada:** `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery2-apply-20261113T042722\`: precheck/hash-manifest, identidadSTS, showJSON del plan autorizado, apply-start/result/stdout/stderr. Temporal, no custodia duradera aceptada ni contenido de state/secretos publicados. Solo task/memoria se editan en esta entrega; .gitignore conserva el cambio previo. Revisión Git final registrada en memoria y sesión. **Sin stage/commit/push. DETENER por fallo parcial, sin continuación técnica automática.**

### 32.9. Corrección local a t3.small — policy real y recovery3 pendientes

**2026-11-13 UTC — BUILD/RECOVERY CONTROLADO, NO APPLY.** Baseline real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cambios previos únicamente.gitignore/task/memoria, **0staged**. Se conserva íntegro el fallo§32.8: dos asociaciones creadas y dosEC2 rechazadas con InvalidParameterCombination por elegibilidad Free Tier, sin AccessDenied. La decisión nueva del responsable sustituye el tipo por **t3.small**, que informa elegible para la cuenta; **x86_64/linux-amd64,2vCPU/2GiB**, sin migración ARM. Suficiencia para baseline es justificación aprobada, no capacidad/latencia medidas ni garantía de costo cero o éxito RunInstances.

**Búsqueda previa:** **42referencias en42líneas**, clasificadas como **HCL operativo1** (`infra/compute.tf:65`), **condición IAM1** (`infra/bootstrap/terraform-local-compute-policy.json:74`), **documentación26** en este task y **14** en memoria. Sin coincidencias en spec/plan003 u otros fuentesinfra revisados. Las referencias documentales son decisiones/estimaciones/planes/fallos históricos, no se sustituyen globalmente ni se alteran binarios previos.

**Cambios exactos autorizados y comprobados:**

| Archivo | Único cambio operativo |
| --- | --- |
| infra/compute.tf | instance_type = **t3.small** en aws_instance.api, antes t3a.small |
| infra/bootstrap/terraform-local-compute-policy.json | Statement Sid **LaunchOnlyTaggedSmallImdsv2Hosts**, Condition.StringEquals.ec2:InstanceType = **t3.small**, antes t3a.small |

El JSON completo actualizado queda preparado en esa ruta para revisión manual; comparación estructural confirma **ningún otro permiso/condición/recurso cambiado**, sin lista ampliada/t3*/ec2:* nuevos. Comparación del HCL confirma solo ese valor, tolerando únicamente finalesCRLF/LF. **No cambios** de AMI/estrategia de resolución (último ID acreditado **ami-096ef200a877f36af**, sin nueva consulta), filtroarquitectura **x86_64**, CPUstandard, AZus-east-1a/b delbaseline, root10GiBgp3cifrado/3000IOPS/125MiBps, IMDSv2required/hop1, ENI/EIP/asociaciones/SG/runtimeIAM/boundary/user_data/bootstrap/red. SG conserva8080soloSGALB, sinSSH22/8080públicos; evidencia local, no comprobación AWS nueva. No afirmar que el alias dinámico de AMI no podrá variar en un futuro refresh: verificar ID exacto en el plan posterior y detener ante cambio no autorizado.

**Acción humana requerida, NO ejecutada:** actualizar manualmente **ENVER-Terraform-Compute** en cuenta926925605666 para que el mismo statement tenga exactamente:

```json
"ec2:InstanceType": "t3.small"
```

Conservar todo lo demás de la policy real, sin agregar tipos/wildcards ni modificar role/profile/boundary. El archivo local contiene la candidata completa, **no una descarga de la policy remota**; comparar antes de aplicar manualmente, preservando cualquier diferencia remota aprobada (por ejemplo la lectura DescribeAddressesAttribute informada en§32), sin sobrescribirla inadvertidamente desde una candidata histórica. **No hay confirmación posterior de actualización real; policy remota no consultada/modificada. Se trata como PENDIENTE, no como cambio AWS acreditado.**

**Validaciones ejecutadas, todas exit0:**

```text
terraform fmt -recursive infra
terraform fmt -check -recursive infra
terraform -chdir=infra validate
bash -n infra/bootstrap/compute-host.sh
```

Validación/sintaxis local no prueba permisos efectivos, elegibilidad de cuenta o funcionamiento cloud. Hashes del restoinfra/lock/bootstrap/runtimeIAM/security/state/backup/tresplanes/.gitignore intactos respecto al inicio de este turno. No instalación/init/upgrade/ejecuciónbootstrap/AWS/SSM/Docker/app.

**DETENIDO ANTES DE PLAN por falta de confirmación manual de policy.** Recovery3 **requerido, NO generado**, sin exitcode/SHA256/show/resultados reales. Tras confirmación, comprobar primero `git check-ignore -v -- infra/tfplan-infra5-recovery3`; si no está ignorado, **detener sin modificar .gitignore automáticamente**. Esta segunda puerta no se evalúa todavía, al no superar la primera. Solo entonces podrá generarse nuevo recovery3 para revisión, **nunca aplicar en este bloque**. No reutilizar tfplan-infra5/recovery/recovery2, parcialmente consumidos u obsoletos.

**Resultado futuro solo conceptual:** **2add/0change/0destroy/0replacements**, únicamente aws_instance.api["a"] y["b"]. Exigir en el plan real **no-op** de EIP A/B, asociaciones A/B **eipassoc-0d50801839a9cae11 /eipassoc-0717a47e226e64bda**, ENI A/B, IAMruntime4, NETWORK14 yECR2; detener ante cualquier acción distinta/no autorizada. Identidades originales conservadas por configuración y ausencia de escrituras, **sin nueva revalidación remota ni no-op acreditados por plan**: EIPA **eipalloc-0c297551d49ce9416 /18.210.153.223 /eni-01ecdecf22b187764**, EIPB **eipalloc-0b54456ecfc2cdafa /44.195.4.217 /eni-049a982368fb2bca8**.

**Estados conservados:** T-INFRA-005 **PARCIAL**, T-INFRA-009 **PARCIAL / RECOVERY PENDIENTE**, T-INFRA-010 **PARCIAL**, T-INFRA-025 **PARCIAL**; **GATE-I-C PENDIENTE / NO SATISFECHO**. No doshosts reales/SSM/Docker/capacidad/mismo digest/prueba destructiva o recuperación integral aceptados. Nuevegatesabiertos/11parciales/19pendientes. Estimación≈USD2.3915/48h de apartados previos corresponde al tipo anterior y **no se traslada como nueva cotización t3.small**, sin Billing/FreeTierahorro acreditados; reevaluación de costo pendiente antes de futura provisión, guardrailUSD10/proyectoUSD50AGREGADOS y ventanaoriginalsinreiniciar conservados.

**Evidencia temporal privada:** `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-t3-small-correction-20261113T043410\`, precheck/clasificaciónreferencias/hashes/copiaspreviasdedosfuentes/resultadosvalidaciones/comparaciónexacta. No state/secretos publicados ni custodia duradera aceptada. Solo compute.tf/candidataCompute/task/memoria editados; .gitignore previo intacto. Revisión Git final registrada en memoria y sesión. **Sin cambios IAM/AWS/apply/destroy/import/liberar EIP/stage/commit/push. DETENER para actualización manual y confirmación, sin plan automático.**

### 32.10. Recovery3: lecturas acreditadas y detención antes del plan por ignore

**2026-11-13 UTC — RECOVERY3 DESPUÉS DE CAMBIO A t3.small, NO APPLY.** El responsable confirma que actualizó manualmente únicamente **ENVER-Terraform-Compute /Sid LaunchOnlyTaggedSmallImdsv2Hosts /ec2:InstanceType=t3.small**, antes t3a.small; no autoriza más cambios IAM. Historial de dosapplyparciales, asociaciones y rechazo InvalidParameterCombination por FreeTier se conserva. Baseline real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cincoarchivosmodificados esperados (.gitignore/compute/candidataCompute/task/memoria), **0staged**, diff-checkexit0/soloavisosLF→CRLF.

**Operativo:** búsquedaeninfra confirma **t3.small** solo en compute.tf:65 y candidataCompute:74, sin t3a.small en fuentesoperativas inspeccionadas. Referencias históricas no sustituidas. **STS exit0**, cuenta926925605666/usuarioenver-terraform-local/perfilenver-infra/us-east-1, sin credencialesalternativas. **GetPolicy** delARN `arn:aws:iam::926925605666:policy/ENVER-Terraform-Compute` **exit254/AccessDenied: iam:GetPolicy no permitido**. No GetPolicyVersion, simulación ni ampliación/solicitud depermisos. **“actualización informada por responsable, no verificada independientemente”**. Se continúa con lecturas/validaciones conforme a excepción autorizada, no se afirma permisoefectivo ni versiónactiva comprobados.

**State real:** `terraform -chdir=infra state list` **exit0**, **26managed+3data, serial34**. Desglose **NETWORK14/ECR2/IAMruntime4/ENI2/EIP2/asociaciones2/EC2 0**. Ambas direcciones `aws_eip_association.api["a"]` y `aws_eip_association.api["b"]` presentes, además de EIP/ENI A/B y role/profile/dosinline. Inspección selectiva local confirma IDs y boundaryexacta; no state completo publicado ni modificado.

**AWS real, solo lectura y exit0:** DescribeAddresses porlasdosallocations, DescribeNetworkInterfaces porlasdosENI, DescribeInstances filtrado **Project=enver/Environment=dev/us-east-1**, estados pending/running/stopping/stopped/shutting-down → **0EC2** (no conteouniversal ni instanciasterminadas). LasENI están **available/sinattachmenthost**, SGbackend **sg-070d06cfebe866427**.

| Host | Allocation/IP conservadas | Association conservada | ENI / privada / AZ |
| --- | --- | --- | --- |
| A | **eipalloc-0c297551d49ce9416 /18.210.153.223**, domainvpc | **eipassoc-0d50801839a9cae11** | **eni-01ecdecf22b187764 /10.20.0.63 /us-east-1a** |
| B | **eipalloc-0b54456ecfc2cdafa /44.195.4.217**, domainvpc | **eipassoc-0717a47e226e64bda** | **eni-049a982368fb2bca8 /10.20.1.99 /us-east-1b** |

**Validaciones:** `terraform fmt -check -recursive infra`, `terraform -chdir=infra validate`, `bash -n infra/bootstrap/compute-host.sh` → **exit0/0/0**. No fmtreescritor/init/upgrade/ejecuciónbootstrap. LecturaHCL conserva **t3.small/CPUstandard, x86_64/linux-amd64/AL2023**, estrategiaAMIintacta/últimoIDacreditado **ami-096ef200a877f36af**, dosAZ, root10GiBgp3cifrado/3000IOPS/125MiBps, IMDSv2required/hop1 y ENIexistentes. Sin nuevaconsultaAMI ni plan para confirmar elIDresueltoactual. SGHCL sinSSH22/8080públicos,8080soloSGALB; reglasAWS no reconsultadas. Revisión de bootstrap no encuentra literales deAWSkeys/sessiontokens/Mongo/Redis/JWT/passwords/ECRtoken ni login/pull/run/autoarranqueEnver.API; soloDocker/SSM. No certificación de secretscanning universal ni ejecuciónruntime.

**Puerta obligatoria:** `git check-ignore -v infra/tfplan-infra5-recovery3` **exit1/sinsalida/sinregla**, archivoinexistente/no tracked. **DETENIDO ANTES DE PLAN**. No modificar .gitignore automáticamente, elegir otra ruta o reutilizar planesanteriores. Se solicita autorización explícita **exclusivamente para añadir `/infra/tfplan-infra5-recovery3`** antes de continuar. **Recovery3 NO creado, SHA256/planexitcode/add-change-destroy/replacements/actions NO DISPONIBLES.** Elresultado **2add/0change/0destroy/0replacements** y solo2EC2 es conceptual, no acreditado. **NO-OP de EIP/asociaciones/ENI/IAMruntime/NETWORK14/ECR2 tampoco acreditado por nuevo plan**, aunque identidades estén verificadas. Ningún apply/destrucción/replacement realizado.

**Costo nominal actualizado, no Billing/beneficioFreeTier:** [AWS T3](https://aws.amazon.com/ec2/instance-types/t3/) consultado2026-11-13 muestra **t3.small USD0.0209/h Linux/Unix/us-east-1**,2vCPU/2GiB. Doshosts×48h → **USD2.0064**; raíces20GiB×USD0.08/GiB-mes×48/720 → **0.106667** (tarifa/prorrateo previo); dosIPv4×USD0.005/h×48 → **0.48** (tarifa previa); **baseline48h≈USD2.5931**, delta **+USD0.2016** respecto a2.3915histórico. CPUstandard conserva posiblethrottling, sin asumirUnlimited/surplus. PosibleFreeTier/créditos sujeto aelegibilidad/cobertura/consumo, **no costo0**, beneficio/cargosreales no consultados. Excluye transferencia/ECR/logs/dependencias/serviciosfuturos/impuestos; no presupuesto integral/cap técnico. **GuardrailUSD10/proyectoUSD50AGREGADOS**, **ventanaoriginal NO reiniciada**, máximo **2026-11-15T01:57:56.9337244Z**. Sin timer/apagado/cleanup.

**Estados:** T-INFRA-005 **PARCIAL**, T-INFRA-009 **PARCIAL / RECOVERY3 PENDIENTE**, T-INFRA-010 **PARCIAL**, T-INFRA-025 **PARCIAL**, **GATE-I-C PENDIENTE / NO SATISFECHO**. Sin dosEC2reales/SSM/Docker/digest/capacidad/recuperación/prueba destructiva o dependenciaaceptados. Nuevegatesabiertos/11parciales/19pendientes;002/004/AUTH-027integral intactos.

Evidenciaprivada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery3-readiness-20261113T044734\` (precheck/hashes/STS/GetPolicydenegado/statelist/resumenselectivo/AWSlecturas/validaciones/ignore/costo), temporal sincustodiaduraderaaceptada. Solo task/memoriaeditadosenesteturno, HCL/candidata/.gitignore/state/backup/planesconservados; no secretos/statecompleto publicados. RevisiónfinalGit/integridad enmemoriaysesión. **NO APPLY/AWS WRITE/IAMchanges/destroy/import/taint/untaint/liberar o desasociarEIP/eliminarENI/stage/commit/push. DETENER por ignore, sincontinuaciónautomática.**

### 32.11. Recovery3 guardado con t3.small — 2/0/0,26no-op, sin apply

**2026-11-13 UTC — AUTORIZACIÓN POSTERIOR EXPLÍCITA:** agregar solo `/infra/tfplan-infra5-recovery3` y generar/inspeccionar plan, sin apply ni cambios IAM/AWS. Baseline verificada **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cincoarchivosmodificadosesperados/0staged/diff-checkexit0. HCL y candidataCompute exactos **t3.small**; historialt3a.small/rechazoFreeTier/dosapplyparciales intacto.

**Ignore:** única línea añadida **/infra/tfplan-infra5-recovery3**, .gitignore:97. Reglas originales /infra/tfplan-infra5, /infra/tfplan-infra5-recovery y /infra/tfplan-infra5-recovery2 y todaslasdemás conservadas; comparación contra copia previa confirma solo esa inserción, sinwildcard. **`git check-ignore -v infra/tfplan-infra5-recovery3` exit0**, antes de crear. Plan antes inexistente/no tracked.

**Validaciones:** fmt-check/validate/bash-n **exit0/0/0**. `terraform -chdir=infra state list` **exit0/26managed+3data**, **serial34/0tainted/0EC2**, EIP2/asociaciones2/ENI2/IAM4/NETWORK14/ECR2. Sin init/upgrade/ejecuciónbootstrap. **“policy AWS actualizada según responsable, no verificada independientemente”**; iam:GetPolicydenegado en§32.10 tratado como no bloqueante, no repetido ni ampliadoIAM.

**Único plan nuevo:**

```text
terraform -chdir=infra plan -out=tfplan-infra5-recovery3 -detailed-exitcode -var="runtime_permissions_boundary_arn=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"
```

**2026-11-13T04:55:35.529Z–04:55:51.321Z /15.786s /exit2**, resultado real **2add/0change/0destroy/0replacements**, **2create puros/26no-op**. Perfilenver-infra/us-east-1, entradasbaselineaprobadas temporales: enver/dev/cuenta926925605666/VPC10.20.0.0/16/newbits8/common_tags{}/network_design_approvedtrue/compute_plan_approvedtrue/compute_ami_idnull(default), boundarypor-var. Sin tfvarsreal/credencialesalternativas o cambiosHCL. **NUEVO AccessDenied: NO**; denegaciónIAMhistórica no equivale a error delplan ni permite inferir escrituras autorizadas.

**Binario privado ignorado:** `infra/tfplan-infra5-recovery3`, SHA256 **`6105cd7c66250f2264a26795e7c6b383882cd9f5a492da3b18e81a83cdc78b5d`**. `terraform show` humano(sincolor) y JSONdelmismobinario **exit0/0**. Revisión inicial local buscó dataAMI enplanned_values, donde no figura; se corrigió lecturaal snapshotrefrescado prior_state delmismoJSON, sin replan ni falloTerraform.

| Dirección / grupo | Acción real | Identidad conservada |
| --- | --- | --- |
| aws_instance.api["a"] | **create** | Host futuroA, ENIAexistente |
| aws_instance.api["b"] | **create** | Host futuroB, ENIBexistente |
| aws_eip.api["a"] | **no-op** | **eipalloc-0c297551d49ce9416 /18.210.153.223** |
| aws_eip.api["b"] | **no-op** | **eipalloc-0b54456ecfc2cdafa /44.195.4.217** |
| aws_eip_association.api["a"] | **no-op** | **eipassoc-0d50801839a9cae11**, allocationA↔ENIA |
| aws_eip_association.api["b"] | **no-op** | **eipassoc-0717a47e226e64bda**, allocationB↔ENIB |
| aws_network_interface.api["a"] | **no-op** | **eni-01ecdecf22b187764 /10.20.0.63** |
| aws_network_interface.api["b"] | **no-op** | **eni-049a982368fb2bca8 /10.20.1.99** |
| IAMruntime4 | **4no-op** | role/profileenver-dev-api-runtime/dosinline y boundaryexacta |
| NETWORK14 /ECR2 | **14no-op /2no-op** | Sin altas/cambios/bajas |

IAM4 son aws_iam_role.api_runtime, aws_iam_instance_profile.api_runtime, aws_iam_role_policy.api_ecr_pull y aws_iam_role_policy.api_ssm_session; **boundary arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary**. No crear/reemplazar/liberar EIP, recrear/desasociar asociaciones, eliminarENI ni modificarIAM/red/ECR.

**Compute inspeccionado enplan:** ambos **t3.small**, AMI **ami-096ef200a877f36af**, dataAMIrefrescada **x86_64/AL2023/al2023-ami-2023.12.20261112.0-kernel-6.18-x86_64**, compatiblelinux/amd64. A **us-east-1a**, B **us-east-1b**; raízporhost **10GiBgp3cifrada/3000IOPS/125MiBps/delete_on_terminationtrue**, CPUstandard, **IMDSv2required/hop1**. ENIA/B originales, únicSGbackendsg-070d06cfebe866427, profileenver-dev-api-runtime, sinkey_name. **SSH22públicoNO/backend8080públicoNO**; únicoingressbackendTCP8080 desdeSGALB **sg-0dacf0df5da30b4f7**, sinCIDR público. Son atributos delplan, no hosts/runtime/seguridad activa acreditados.

**User_data:** ambos coinciden con compute-host.sh normalizandoCRLF/LF. Revisión no detecta AWSkeys/secretkeys/sessiontokens/Mongo/Redis/JWT/passwords/ECRtoken literales; no dockerlogin/pull/run o autoarranqueEnver.API, solo preparaciónDocker/SSM. No secret-scanning universal ni SSM/Docker/health efectivos. **Enver.API NO desplegada/iniciada por esta entrega**.

**Refresh separado de acciones:** **2resource_drift/update**, EIP A/B, desdecamposvacíos a asociacionesyaexistentes: association_id→eipassocA/B, network_interface→ENIA/B, private_ip→10.20.0.63/10.20.1.99, private_dns→ip-10-20-0-63.ec2.internal/ip-10-20-1-99.ec2.internal. **Ambas EIP son no-op enresource_changes**; no updates aejecutar, allocations/IP originales sin cambio. No afirmar drift0 o nuevamodificaciónexterna porinferenciadelsnapshot. Sin persistirrefresh sobrestate/backup.

**Costo nominal48h conservado:** t3.smallUSD0.0209/h×2×48=**2.0064**, EBS≈**0.1067**, IPv4≈**0.48**, total **≈USD2.5931/48h**. ReferenciaAWSdocumentada§32.10; FreeTier **NO descontado**, noBilling/créditos/consumo real. Guardrail **USD10**, proyecto **USD50AGREGADOS**, ventanaoriginalsinreiniciar/máximo **2026-11-15T01:57:56.9337244Z**, sincap técnico/timer/cleanupautomático o gastoautorizadoporplanificar.

**Estados:** T005 **PARCIAL**, T009 **PARCIAL / RECOVERY3 PRE-APPLY**, T010 **PARCIAL**, T025 **PARCIAL**, **GATE-I-C PENDIENTE /NO SATISFECHO**. Plan/hash/no-op no prueban2EC2reales/SSM/Docker/digest/capacidad/recuperaciónintegral/pruebadestructiva/custodia/permisosefectivos. Nuevegatesabiertos/11parciales/19pendientes;002/004/AUTH-027integralintactos.

Evidenciaprivada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery3-plan-20261113T045414\` (baseline/hashes/ignore/validaciones/statelist/planresultados/show/análisisselectivo), temporal sincustodiaduraderaaceptada. Solo.gitignore/task/memoriaeditadosenesteturno, compute/candidata conservan cambiospreviosautorizados. State/backup/planesprevios yrestoinfra intactos; binarionuevoignoradono tracked. RevisiónGit/integridad final enmemoria/sesión. **NO APPLY/destroy/import/taint/untaint/IAMchanges/AWS WRITE/liberarEIP/stage/commit/push. DETENER para revisión humana delbinario, sinprovisiónautomática.**

### 32.12. Apply controlado recovery3 — IDs EC2 asignados, fallo en ModifyInstanceAttribute

**2026-11-13 UTC — autorización explícita para aplicar solo recovery3.** Baseline real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cincoarchivosmodificadosesperados (.gitignore/candidataCompute/compute/task/memoria), **0staged/diff-checkexit0**. SHA256 **6105cd7c66250f2264a26795e7c6b383882cd9f5a492da3b18e81a83cdc78b5d**, coincidenciaexactaantesdeaplicar. Showhumano/JSONexit0/0: **2create/0change/0destroy/0replacements/26no-op**, únicamente `aws_instance.api["a"]` y `aws_instance.api["b"]`. Ningún nuevo plan previo ni reutilización de planesanteriores.

**Único comando apply:**

```text
terraform -chdir=infra apply tfplan-infra5-recovery3
```

**2026-11-13T05:04:11.150Z–05:04:35.647Z /24.491s /exit1**. Perfilenver-infra/us-east-1, sincredencialesalternativas. **FALLO PARCIAL /UnauthorizedOperation /HTTP403**, al configurar **InstanceInitiatedShutdownBehavior**; elHCL ya exigía **stop**. **Acción denegada: ec2:ModifyInstanceAttribute**. Identidad: **arn:aws:iam::926925605666:user/enver-terraform-local**; AWS informa que ningunaidentity-basedpolicy permiteesaacción. No modificarIAM, retiraratributodelHCL, concederwildcards o repetirapply paraeludirlo.

| Host /dirección | ID asignado que aparece en error | Recurso denegado /RequestID |
| --- | --- | --- |
| A /aws_instance.api["a"] | **i-0a66085b7b2411393** | **arn:aws:ec2:us-east-1:926925605666:instance/i-0a66085b7b2411393** /6d8093d9-4d9d-47af-ae6d-d32e39900d81 |
| B /aws_instance.api["b"] | **i-00de11a3f3c1a8aa5** | **arn:aws:ec2:us-east-1:926925605666:instance/i-00de11a3f3c1a8aa5** /480c16b7-368c-489b-98ab-3737a49ab438 |

**Resultado exacto:** dosIDs asignados durante creación, **0 Creation complete**, sinresumenApplycomplete/2added. **No afirmar0EC2existentes ni2recursosTerraformcompletados**: creación/configuración es parcial, conteoAWS/estadofinalnoverificados. **0 cambios/0destrucciones completados registrados**; doserroresModifyInstanceAttribute, AccessDenied/UnauthorizedOperation **SÍ**. Mensajescodificadosdeautorización no se reproducen en documentación ni se intentadecodificar/consultardetallesconAPIsadicionales.

**Detención trasfallo:** no segundointento/apply, state list, EC2/EBS/ENI/EIP/SG/IAM/SSM/Docker, post-plan, correcciónmanualstate/taint o modificacionesIAM. **Conteo managed actual NO VERIFICADO** (últimoprevio26, esperado28 no es inventario real); tampoco serial/taintactuales. State/backup pueden cambiar legítimamente porapplyparcial, no restaurar nipresentarhashesprevioscomoactuales.

**Configuración versus verificación:** t3.small/AMIami-096ef200a877f36af/x86_64/AL2023/us-east-1a-b/10GiBgp3cifrado/IMDSv2required y profileenver-dev-api-runtime constan enplan, **no verificadas enhosts despuésdelapply**. Estadosrunning/LaunchTime/volúmenes/attachmentENI/perfilreal/SG/MetadataOptions **NO DISPONIBLES porinventario**; no completarcamposconvaloresplanificadoscomosifueranreales. EIP A/B allocations/IP/asociaciones y ENI originales permanecían **no-op enplan**, sinreemplazos/liberaciones/desasociacionesregistradas, pero no reconsultadas trasfallo. IAMruntime/NETWORK14/ECR2 sinaccionesdecambioenplan ni modificacionesregistradas; sinpost-plan no afirmar drift0/aceptaciónremotarevalidada.

**SSM A/B y Docker A/B NO VALIDADOS**; ni presenciassm/PingStatus/PlatformName/AgentVersion ni docker--version/is-enabled/is-active obtenidos. NoabrirSSH/8080público, noSSMcorrectivo/IAMautomático. **Enver.API NO iniciada por esta ejecución**, ningún deployment/dockerpull-run/configruntimeAtlas/Redis/DynamoDB/JWT/SecretsManager/ALB. Elbootstrap aprobado no autoarrancaAPI, pero sininspecciónremota noafirmarauditoríadelprocesadoreal.

**Post-plan NO ejecutado**, exitcode/add-change-destroy no disponibles. Laautorizaciónlocondicionabaaléxito, no cumplido. **Recovery3 parcialmente consumido: NO REAPLICAR**; tampocoreutilizarplanesanteriores. Cualquiercontinuación requiere nuevaautorizacióndelimitada y revisión delestado, sin generarnuevoplan o recomendación automática deprivilegios.

**Recuperación aún incompleta:** losdosapplyparcialesprevios, correccióntaintautorizadaanterior y cambiocontroladodetipo quedan trazados; este **tercerapplyparcial** avanzóaIDsEC2sincompletarconfiguración. No destroy/replacement/pruebadestructiva/continuidadreal; **T010 no cerrar**. **T005PARCIAL**, operadorconpermisoinsuficienteparaatributo/SSM/aislamientointegralpendientes; **T009PARCIAL /APPLY RECOVERY3 FALLIDO**, dosIDsnoequivalenadoshostsDockerverificados; **T025PARCIAL**, sinpost-plan/custodia/backup/recovery/dependenciasaceptados. **GATE-I-C PENDIENTE /NO SATISFECHO**: faltanverificaciónhosts/dosAZ/mismodigest/capacidad/recuperación/E007–010/E027–028/prerrequisitosI-A/Baceptados. Nuevegatesabiertos/11parciales/19pendientes,002/004/AUTH-027integralintactos.

**Costo nominal48h≈USD2.5931**, FreeTierNOdescontado, guardrailUSD10/proyectoUSD50AGREGADOS, **ventanaoriginalNOreiniciada/máximo2026-11-15T01:57:56.9337244Z**. NoBillingreal/consumoacumulado/créditosconsultados; laasignacióndeIDs puede implicarEC2activasycargos aunconTerraformfallido, no asumircoste0. No stop/cleanup/liberaciónIP/timerautomáticos.

Evidenciaprivada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery3-apply-20261113T050337\`: precheck/hashmanifiesto/showhumano-JSON/apply-start-result/stdout-stderr, temporal/nocustodiaduraderaaceptada. No secretos/statecompleto/mensajescodificadospublicadosendocumentación. Solo task/memoriaeditadosenesteturno; demásfuentes/planes/.gitignorepreviosconservados, state/backup puedenvariarporapply. **NO reintento/segundoapply/post-plan/IAMchanges/destroy/import/taint/untaint/liberarEIP/stage/commit/push. DETENER porfalloparcial.**

### 32.13. Recuperación post-recovery3: dos EC2 running/tainted; recovery4 bloqueado por ignore

**2026-11-13 UTC — autorización posterior de lecturas y plan condicionado, NO APPLY.** Baseline real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cinco archivos modificados esperados/0staged/diff-checkexit0. Se conserva el fallo ModifyInstanceAttribute HTTP403 de §32.12. El responsable informa que añadió manualmente **ec2:ModifyInstanceAttribute**, recurso **arn:aws:ec2:us-east-1:926925605666:instance/*** y tags Project=enver/Environment=dev/ManagedBy=Terraform en ENVER-Terraform-Compute. **Corrección informada, no verificada independientemente ni ejercitada**: no GetPolicy nuevamente, simulación o llamada de escritura para probarla; DescribeInstanceAttribute solo acredita lectura.

**AWS real, solo lecturas exitosas:** STS, DescribeInstances de los dos IDs, DescribeInstanceAttribute shutdown de A/B, DescribeVolumes, DescribeNetworkInterfaces, DescribeAddresses, GetRole, GetInstanceProfile y DescribeSecurityGroups del backend, **sin nuevo AccessDenied**. Perfil enver-infra/us-east-1, cuenta926925605666/usuarioenver-terraform-local, sin credenciales alternativas.

| Campo real | A | B |
| --- | --- | --- |
| Existe / InstanceId | **Sí /i-0a66085b7b2411393** | **Sí /i-00de11a3f3c1a8aa5** |
| State / tipo / arquitectura | **running /t3.small /x86_64** | **running /t3.small /x86_64** |
| ImageId | **ami-096ef200a877f36af** | **ami-096ef200a877f36af** |
| AZ / privada | **us-east-1a /10.20.0.63** | **us-east-1b /10.20.1.99** |
| ENI original, attached al ID correcto | **eni-01ecdecf22b187764** | **eni-049a982368fb2bca8** |
| LaunchTime devuelto por AWS | **2026-11-13T05:03:37+00:00** | **2026-11-13T05:03:38+00:00** |
| InstanceInitiatedShutdownBehavior | **stop** | **stop** |
| Root VolumeId | **vol-0619ef31bdc16c5ff** | **vol-0e3c5b500af45e847** |
| Root EBS real | **10GiB/gp3/encrypted=true/in-use** | **10GiB/gp3/encrypted=true/in-use** |
| EIP allocation / PublicIp | **eipalloc-0c297551d49ce9416 /18.210.153.223** | **eipalloc-0b54456ecfc2cdafa /44.195.4.217** |
| Association original | **eipassoc-0d50801839a9cae11** | **eipassoc-0717a47e226e64bda** |

Ambos perfiles reales: **arn:aws:iam::926925605666:instance-profile/enver-dev-api-runtime**, con runtime role **enver-dev-api-runtime** y boundary remota verificada **arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary**. SecurityGroups de ambos: únicamente **sg-070d06cfebe866427 /enver-dev-backend**. MetadataOptions de ambos: **State=applied/HttpTokens=required/HttpPutResponseHopLimit=1/HttpEndpoint=enabled/HttpProtocolIpv6=disabled/InstanceMetadataTags=disabled**. Attachments ENI correctos, DeleteOnTermination=false. EIP/asociaciones conservadas y vinculadas a ENI/privada A/B exactas; sin nuevas escrituras.

**Seguridad AWS comprobada:** único ingress backend **TCP8080 desde sg-0dacf0df5da30b4f7**, sin IpRanges/Ipv6Ranges públicos. **SSH22público NO /TCP8080público NO**. No prueba activa de bypass/TLS/egreso fino, SSM o Docker en esta entrega.

**Terraform real:** `terraform -chdir=infra state list` **exit0 /28managed+3data /serial37**. Ambas direcciones `aws_instance.api["a"]` y `aws_instance.api["b"]` presentes con los mismos IDs AWS, **ambas status=tainted**; shutdownbehavior registrado stop. No se eliminaron marcas, importaron recursos, editaron atributos/state o restauraron backups. **Falta reconciliación conservadora del estado tras create fallido**, no demostrar existencia solamente. Taint es un riesgo de replacement que debe revisarse; **sin nuevo plan no acreditar acciones concretas ni asumir updates/no-op/replacements**. El shutdown real ya es stop; el rechazo histórico no demuestra que ese valor siga pendiente en AWS ni que el nuevo permiso funcione. No autoriza untaint, retirar atributos HCL o reemplazar hosts.

**Validaciones:** `terraform fmt -check -recursive infra`, `terraform -chdir=infra validate`, `bash -n infra/bootstrap/compute-host.sh` → **exit0/0/0**, sin ejecutar bootstrap. **`git check-ignore -v infra/tfplan-infra5-recovery4` exit1/sin regla**, archivo inexistente/no tracked. **DETENIDO ANTES DE PLAN**. Solicitar autorización explícita para añadir **únicamente `/infra/tfplan-infra5-recovery4`**, sin editar .gitignore automáticamente. **Recovery4 no creado, sin SHA256/planexitcode/add-change-destroy/replacements/actions EC2 A/B ni nuevos no-op EIP/ENI/asociaciones/IAM/NETWORK14/ECR2 acreditados**. No reutilizar los cuatro planes anteriores; sin apply de ningún plan.

**Costo/tiempo:** EC2 running con LaunchTime real arriba; son horas potencialmente facturables aunque Terraform falló. **Nominal INFRA-5≈USD2.5931/48h**, FreeTier no descontado, **guardrailUSD10/proyectoUSD50AGREGADOS**, no Billing/consumo acumulado real. Ventana original **NO reiniciada**, máximo **2026-11-15T01:57:56.9337244Z**; LaunchTime AWS se registra tal como respondió, separado de tiempos del proceso local. Sin apagado/timer/cleanup ni liberación automática de IP.

**Estados:** T005 **PARCIAL**, profile/boundary acreditados pero SSM/separación/mínimo privilegio integral pendientes; T009 **PARCIAL /DOS EC2 RUNNING, STATE TAINTED Y RECOVERY4 PENDIENTE**, sin Docker/SSM/capacidad/mismo digest/aceptación; T010 **PARCIAL**, reconciliación/prueba formal pendientes; T025 **PARCIAL**, state e inventario acreditados sin plan/custodia/backup/recovery aceptados. **GATE-I-C PENDIENTE /NO SATISFECHO**, dos hosts reales solo cumplen una parte: faltan mismo digest/capacidad/recuperación/E007–010/E027–028/prerrequisitosI-A/B/aceptación. Nueve gates abiertos/11parciales/19pendientes,002/004/AUTH-027integral intactos. No SSM/Docker/app deployment o arranque Enver.API ejecutados aquí.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-post-recovery3-20261113T051207\`: baseline/hashes/lecturasAWS/statelist/resumen selectivo/validaciones/ignore, temporal sin custodia duradera aceptada. Solo task/memoria editados; fuentes/configuración/state/backup/planes/.gitignore conservados. Revisión Git final en memoria/sesión. **Sin apply/destroy/import/state rm/taint/untaint/IAMmodificado/.gitignoremodificado/liberarEIP/stage/commit/push. DETENER por ignore**, sin continuación automática.

### 32.14. Reconciliación conservadora EC2: untaint A/B y recovery4 sin acciones de recursos

**2026-11-13 UTC — autorización explícita condicionada, NO APPLY.** Baseline real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cinco modificados esperados/0staged/diff-checkexit0. Historial de tres applies parciales intacto; causa de los taints EC2: create logró IDs pero provider falló en ModifyInstanceAttribute/shutdown (§32.12). Corrección manual del permiso informada por responsable, **no verificada independientemente ni ejercitada**; no se modifican candidata local o IAM remoto.

**Exclusión exacta:** agregada solo **/infra/tfplan-infra5-recovery4** en .gitignore:98, conservando las reglas anteriores sin wildcard. Check-ignore exit0 antes de verificar y antes del plan; archivo no tracked. No se reutilizan tfplan-infra5/recovery/recovery2/recovery3.

#### Verificación previa a untaint

Lecturas STS/DescribeInstances/DescribeImages/DescribeInstanceCreditSpecifications/DescribeVolumes/DescribeNetworkInterfaces/DescribeAddresses/GetRole/GetInstanceProfile/DescribeSecurityGroups y DescribeInstanceAttribute de shutdown/userData A/B **exit0**, sin denegaciones. `terraform state list` y `state show` de ambas direcciones **exit0**; state inicial **28managed+3data/serial37**, ambas **tainted**. Contenidos user-data/state no impresos íntegramente ni incorporados a Git.

| Atributo real comparado con configuración | A | B |
| --- | --- | --- |
| ID / estado | **i-0a66085b7b2411393 /running** | **i-00de11a3f3c1a8aa5 /running** |
| Tipo / AMI / arquitectura | **t3.small /ami-096ef200a877f36af /x86_64** | **t3.small /ami-096ef200a877f36af /x86_64** |
| AZ / privada | **us-east-1a /10.20.0.63** | **us-east-1b /10.20.1.99** |
| LaunchTime AWS | **2026-11-13T05:03:37+00:00** | **2026-11-13T05:03:38+00:00** |
| Shutdown / monitoring / créditos | **stop /disabled /standard** | **stop /disabled /standard** |
| Name | **enver-dev-api-a** | **enver-dev-api-b** |
| ENI original / attached | **eni-01ecdecf22b187764 /al ID correcto** | **eni-049a982368fb2bca8 /al ID correcto** |
| Root EBS real | **vol-0619ef31bdc16c5ff** | **vol-0e3c5b500af45e847** |
| EIP allocation / pública | **eipalloc-0c297551d49ce9416 /18.210.153.223** | **eipalloc-0b54456ecfc2cdafa /44.195.4.217** |
| Association original | **eipassoc-0d50801839a9cae11** | **eipassoc-0717a47e226e64bda** |

AMI disponible, owner Amazon **137112412989**, nombre **al2023-ami-2023.12.20261112.0-kernel-6.18-x86_64**. Ambos roots **10GiB/gp3/encrypted=true/in-use/3000IOPS/125MiBps**, attachment correcto, DeleteOnTermination=true; tags root Project/Environment/ManagedBy y Name enver-dev-api-a/b-root correctos. Tags de hosts **Project=enver/Environment=dev/ManagedBy=Terraform/Name exacto**, sin extras inesperados. SourceDestCheck **true** en hosts/ENI y state (no explícito en HCL, default normal del provider); ENI DeleteOnTermination=false. IMDSv2 **required/endpointenabled/hop1/metadatatagsdisabled/IPv6disabled**. Sin key_name.

Ambas EC2 con profile real **arn:aws:iam::926925605666:instance-profile/enver-dev-api-runtime**, que contiene role **enver-dev-api-runtime** y boundary remota exacta **arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary**. Solo SG **sg-070d06cfebe866427**; único ingress **TCP8080 desde sg-0dacf0df5da30b4f7**, sin IPv4/IPv6 CIDRs públicos. **SSH público NO/8080 público NO**, no prueba activa de bypass. EIP/asociaciones sobre ENI/privadas originales, sin nueva IP.

**User-data AWS decodificado = archivo bootstrap normalizado CRLF→LF de HCL = representación/hash en state**, ambas SHA256 **c088da3514016f17457850eb74b2c60890f4fe0e2965e7869e8c92e9a94aecff**. No se imprime contenido ni ejecuta bootstrap. Comparados atributos HCL de tipo/AMI/AZ/profile/monitoring/shutdown/metadata/créditos/root/tags/ENI/user-data; no divergencia material configurada.

**Salvedad de state parcial:** root volume_id/device_name/kms_key_id y algunos metadatos calculados estaban vacíos, aunque configuración root y AWS coincidían; no son volúmenes distintos o atributos configurados en conflicto. La primera comprobación local estricta se detuvo al exigir volume_id calculado; se diagnosticó por lectura sin editar state, se completó B y la comparación de configuración antes de actuar. El plan posterior refresca esos metadatos y confirma no-op. No presentar state37/39 como snapshot completamente refrescado de AWS.

#### Dos untaint autorizados y ninguna escritura AWS

```text
terraform -chdir=infra untaint 'aws_instance.api["a"]'
terraform -chdir=infra untaint 'aws_instance.api["b"]'
```

Direcciones pasadas como argumentos literales mediante proceso nativo para evitar pérdida de comillas en PowerShell. **A exit0**, 2026-11-13T05:25:12.615Z–05:25:17.349Z; **B exit0**, 05:25:17.352Z–05:25:22.569Z. Sin reintento ni otros untaint. Post-state **28managed+3data/serial39/backupserial38**, A/B **NO tainted**, IDs originales intactos. State show antes/después idéntico salvo cabecera tainted, statelist sin cambios. Comparación backup38→39 normalizada acredita solo serial/taintB, sin cambios de atributos. Primer comparador estricto posterior señaló exclusivamente orden de check_results/objects; lectura semántica resolvió ese falso positivo sin repetir comandos/editar state. El estado y backup cambian legítimamente por untaint; no restaurarlos.

**Validaciones posteriores:** fmt-check-recursive/validate/bash-n **exit0/0/0**; check-ignore **exit0**. Ninguna instalación/init/upgrade/HCL cambiado.

#### Recovery4 real, no aplicado

Comando autorizado con los mismos inputs temporales: enver/dev/us-east-1/cuenta926925605666/VPC10.20.0.0/16/newbits8/network y compute approvaltrue/common_tags{}, **compute_ami_id sin override usa default null**; ARN boundary exacto por -var. Sin tfvars reales/credenciales alternativas.

```text
terraform -chdir=infra plan -out=tfplan-infra5-recovery4 -detailed-exitcode -var="runtime_permissions_boundary_arn=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary"
```

- **Intento inicial:** 05:27:14.393Z–05:27:31.115Z/16.717s/**exit1**, TF_VAR_compute_ami_id='null' se interpretó erróneamente como string. Sin AccessDenied ni acciones reales propuestas; binario errored preservado en evidencia privada como failed-plan-string-null.bin, hash **bac1033cd063fc6ad6ad286e392b9567ae7a771e4ddbb43a5d8201ccbd5aa3f8**, **NO es el recovery4 final**. Error de invocación del agente, no HCL/AWS; se omite esa variable para usar el default, sin alterar configuración o ejecutar apply.
- **Plan final corregido:** **2026-11-13T05:28:12.034Z–05:28:31.051Z/19.013s/exit2**, sin AccessDenied. **0add/0change/0destroy/0replacements,28no-op**. SHA256 final **bc682c534dfe5a13b267726f3d21744c6a5830654f925ded8c2a0488873d1662**. Show humano/JSON **exit0/0**, errored=false/complete=true.
- **Exit2 explicado, no exit0:** único cambio propuesto de outputs **+compute_instances**, con IDs/privadas/zone_id use1-az1/use1-az2; faltaba por create parcial. Guardarlo requeriría autorización posterior de escritura de state, **no se aplica**. No equivale a create de EC2 ni modificación AWS.
- **Acciones de recursos:** aws_instance.api["a"] **no-op**, ["b"] **no-op**; ENI2/EIP2/asociaciones2/IAMruntime4/NETWORK14/ECR2 **todos no-op**. Ningún update in-place/create/delete/replace propuesto.
- **Refresh separado:** resource_drift incluye EC2A/B (metadatos calculados faltantes: red/IP/estado/root IDs/tags/KMS/CPU/defaults), EIPA/B (instance), asociacionesA/B (instance_id) y ENIA/B (attachment). Son ocho entradas de snapshot actualizado, **no resource_changes/update**. State/backup locales permanecen intactos frente a después de untaint durante ambos planes; refresh/output no persistidos por apply. No afirmar ausencia total de diferencias de snapshot.

#### SSM denegado y detención

Tras plan sin replacements, **una consulta filtrada** `aws ssm describe-instance-information` para ambos IDs, **2026-11-13T05:29:26.515Z–05:29:29.439Z/exit254/AccessDeniedException**. Identidad **arn:aws:iam::926925605666:user/enver-terraform-local**, acción **ssm:DescribeInstanceInformation**, recurso reportado **arn:aws:ssm:us-east-1:926925605666:***, ninguna identity-based policy la permite. **SSM A/B: presencia/PingStatus/PlatformName/AgentVersion NO VERIFICADOS**, no inferir Offline/ausencia del agente o insuficiencia del runtime role. No ampliar IAM/reintentar/abrir SSH ni ejecutar SendCommand. **Docker A/B NO VALIDADO** (sin versión/is-enabled/is-active), condición Online no acreditada.

**Enver.API NO iniciada por el agente**, ningún deploy/dockerpull-run/config runtime/ALB/Redis/Secrets/DynamoDB; sin inspección remota de procesos no afirmar auditoría total del host. **NO APPLY**, ni siquiera para guardar outputs. Planes anteriores no reutilizados, no destroy/import/terminate/taint/state rm/liberar o desasociar EIP/IAMmodificado/stage/commit/push.

**Reevaluación estricta:** **T005PARCIAL**, role/profile/boundary reales, pero SSM/separación y mínimo privilegio integral no aceptados; **T009PARCIAL**, dos hosts correctos/sin taint/recurso no-op, faltan Docker/SSM/capacidad/digest/aceptación; **T010PARCIAL**, reconciliación conservadora real no prueba formal de fallo/reemplazo/drenaje/continuidad; **T025PARCIAL**, validaciones/plan acreditados pero exit2-output/custodia/backup protegido/dependencias/aceptación integral pendientes. **GATE-I-C PENDIENTE /NO SATISFECHO**, definición §3 requiere además mismo digest/capacidad/recuperación/E007–010/E027–028 y prerrequisitosI-A/B, no basta dosrunning/untaint/plan. Once tareas parciales/19pendientes/nueve gates pendientes,002/004/AUTH-027integral intactos.

**Costo nominal≈USD2.5931/48h**, no descuento FreeTier ni Billingreal consultado; EC2 running desde LaunchTime arriba pueden generar consumo. **GuardrailUSD10/proyectoUSD50AGREGADOS**, ventanaoriginalsinreiniciar/máximo **2026-11-15T01:57:56.9337244Z**. Sin timer/apagado/cleanup automático o garantía de coste cero.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-recovery4-20261113T052114\`: baseline/hashes/lecturasAWS/statelist-stateshow/resumenconfig/diagnósticos/untaint/resultados/validaciones/dosintentosplan/show/análisis/denegaciónSSM. Temporal no acredita custodia duradera aceptada. Solo **.gitignore/task/memoria editados**; compute/candidata/policies/bootstrap/demás fuentes y cuatro planes previos preservados. State/backup solo modificados por dos untaint; nuevo recovery4 ignorado/no tracked/no aplicado. Revisión final Git/hash en memoria/sesión. **DETENER para revisión humana, sin IAM/correcciónSSM/guardadooutputs/apply automáticos.**

### 32.15. Validación final SSM/Docker post-reconciliación — sin Terraform ni despliegue

**2026-11-13 UTC — verificación controlada explícita.** Baseline real **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cinco modificados conocidos/0staged/diff-checkexit0. Lectura local state **28managed+3data/serial39/0tainted** y SHA256 recovery4 **bc682c534dfe5a13b267726f3d21744c6a5830654f925ded8c2a0488873d1662** coincidentes. No Terraform ejecutado, nuevo plan, state recovery o cambios de infraestructura.

El responsable informa creación/adjunción manual a **enver-terraform-local** de policy de verificación con DescribeInstanceInformation/SendCommand/GetCommandInvocation/ListCommandInvocations/ListCommands, SendCommand limitado a ambos IDs y AWS-RunShellScript. **Fuente informada, no auditoría independiente del documento/versiones/condiciones IAM.** Se comprueban solo las operaciones efectivamente usadas; no se ejercitan las dos APIs List ni otros targets/documentos. Sin cambios IAM por agente, sin actualización de candidatas locales.

**Lecturas locales/AWS:** STS cuenta926925605666, perfilenver-infra/regiónus-east-1 sin credenciales alternativas; DescribeInstances dirigido a ambos IDs vuelve a confirmar running/t3.small/dosAZ/LaunchTime original. DescribeInstanceInformation filtrado **solo a ambos IDs**, exit0. No nuevo AccessDenied.

| Evidencia real | A | B |
| --- | --- | --- |
| ID | **i-0a66085b7b2411393** | **i-00de11a3f3c1a8aa5** |
| EC2 / AZ | **running /us-east-1a** | **running /us-east-1b** |
| SSM presente / PingStatus | **Sí /Online** | **Sí /Online** |
| PlatformName / PlatformVersion | **Amazon Linux /2023** | **Amazon Linux /2023** |
| AgentVersion | **3.3.5828.0** | **3.3.5828.0** |
| IPAddress SSM | **10.20.0.63** | **10.20.1.99** |
| Docker versión | **25.0.14, build0bab007** | **25.0.14, build0bab007** |
| Docker enabled / active | **enabled /active** | **enabled /active** |
| Run Command Status / ResponseCode | **Success /0** | **Success /0** |
| Contenedores running | **0, nombres vacíos** | **0, nombres vacíos** |
| Servicio Enver.API running detectado | **No** | **No** |

**Un único SendCommand para ambos IDs**, **CommandId `8abf5ba1-a1d6-4ac2-9592-a3200a3b9d81`**, documento **AWS-RunShellScript**, condición Online comprobada antes. Envío local **2026-11-13T05:38:30.577Z–05:38:33.448Z/exit0**, GetCommandInvocation A/B **exit0/0**, consultas terminadas a05:38:51.013Z. Cinco comandos exactos, sin otros comandos remotos:

```text
docker --version
systemctl is-enabled docker
systemctl is-active docker
docker ps --format '{{.Names}}'
systemctl list-units --type=service --state=running
```

Stderr remoto vacío A/B. Las respuestas AWS informan ExecutionStartDateTime **A05:37:46.814Z/B05:37:46.806Z**, elapsedPT0.104S/PT0.1S y End igual al Start; se conservan como metadatos de origen distintos del reloj local de envío, **sin corregirlos, atribuirles orden causal fiable ni certificar sincronización de relojes**. No reenvío/reintento/polling necesario.

**Aplicación:** salida dockerps sin nombres, listas systemd de20servicios sin Enver/dotnet. **Enver.API no detectada como contenedor o servicio running en A/B**, no iniciada por esta intervención. No auditoría de procesos sueltos o todas las formas de ejecución: los comandos autorizados no prueban ausencia absoluta de procesos arbitrarios. Ningún contenedor inesperado. Servicios de sistema registrados en evidencia privada, incluidos **docker/containerd/amazon-ssm-agent y sshd**; sshd activo no equivale a ingress público autorizado, y no se desactiva/modifica. No pull/run/login/stop/rm, instalación, cambio de config o arranque/parada de servicios.

**Seguridad previa conservada como evidencia fechada §32.14**, no nueva reconsulta IAM/SG/user-data: SSH público **NO**, backend8080público **NO**, ingress soloSGALB; runtime role **enver-dev-api-runtime**, boundary exacta aprobada, IMDSv2required, user-data coincidía con bootstrap sin secretos literales detectados/autoarranqueAPI. Fuentes/hash preservados no constituyen nueva auditoría de IAM o garantía exhaustiva de ausencia de secretos. No abrir puertos o SSH para verificar.

**Terraform:** se omite el plan opcional para evitar trabajo innecesario. Evidencia fechada recovery4 conserva **0add/0change/0destroy/0replacements/28no-op** (EC2/EIP/ENI/asociaciones/IAM4/NETWORK14/ECR2); **+outputcompute_instances** es el cambio output-only y explica exit2. Ocho diferencias de refresh descritas en§32.14, no updates propuestos de recursos. **No afirmar nuevo post-plan exit0**, no guardar outputs, aplicar recovery4 o reutilizar planes parcialmente consumidos. State/backup/planes no modificados.

**Recuperación consolidada sin borrar historial:** (1) apply inicial parcial por lecturaEIP denegada; (2) EIP originales conservadas y untaintEIP autorizados; (3) recovery2 crea asociaciones y falla t3a.small/FreeTier; (4) cambio autorizado **t3a.small→t3.small**; (5) recovery3 obtiene dosIDsEC2 pero falla ModifyInstanceAttribute; (6) corrección IAM mínima manual informada, shutdownstop real; (7) comparación AWS/state/HCL y untaintEC2 controlados; (8) recovery4 recurso0/0/0 sinreplacements; (9) nueva policySSM humana informada, **SSMOnline/Dockerfuncional final en ambos**. Son tres applies parciales históricos, **ningún apply en esta validación**. Recuperación conservadora operativa de hosts acreditada, no ensayo de sustitución/HA/fallo de API.

#### Reevaluación por criterios, sin cierres artificiales

- **T-INFRA-005 PARCIAL:** SSMfuncional/operaciones de verificación y profile/boundary previos acreditados. Falta revisión/aceptación integral de mínimo privilegio, separación provisión/deploy/runtime/authority/secretos/contrato004 y dependencias; éxito de un comando no audita todas las policies ni demuestra aislamiento global.
- **T-INFRA-009 PARCIAL /VALIDACIÓN TÉCNICA DE HOSTS ACREDITADA:** todos los checks técnicos de esta solicitud cubiertos: dosrunning/dosAZ/t3.small/ENI-EIPcorrectas/IMDSv2/EBS cifrado de§32.14 más SSM/Docker reales de esta sección. Dimensionamiento **2vCPU/2GiB por host** justificado y aprobado §§23.3/32.9, hashing no debilitado ni cambiado. **No se exige desplegar API/mismo digest/ALB/HTTPS para acreditar baseline de hosts**, ni se presenta Docker como prueba de carga de .NET/hasher. El catálogo conserva PARCIAL exclusivamente por aceptación de sus dependencias **T006/T008** y decisiones **DP-I-02/03** todavía no integrales, sin autorizar su cierre retrospectivo. E009 técnica queda disponible para revisión/aceptación; pruebas de carga/digest y aceptación cloud integral pertenecen a incrementos/gates posteriores.
- **T-INFRA-010 PARCIAL:** recuperación conservadora de estado y hosts operativos demostrada, pero faltan runbook/escenarios de fallo, rollback/drenaje/reemplazo, límites de disponibilidad y prueba formal/aceptación. No se ejecuta ensayo destructivo ni se garantiza mantenimiento con doshosts.
- **T-INFRA-025 PARCIAL:** baseline/readiness y recovery4 previos acreditados, pero custodia/backup protegido/recuperación formal/versiones/dependenciasE001/E002/E025integral no aceptados. SSM/Docker no sustituyen esos criterios. Output-only permanece pendiente sin ser autorización para aplicar.
- **GATE-I-C PENDIENTE /NO SATISFECHO:** dosEC2/dosAZ comprobadas; faltan **mismo digest desplegado, capacidad y recuperación verificadas, E007–010/E027–028 y cierre de GATE-I-A/B**, además de aceptación explícita del responsable según§3. Estos son criterios del gate, no nuevos bloqueos artificiales del checklist técnico de hosts. No se cierra por Docker o SSM, ni se ejecuta API para intentar satisfacerlo.

**11parciales/19pendientes/9gatespendientes**, sin cambios002/004/AUTH-027integral. **Costo nominal≈USD2.5931/48h**, no FreeTier descontado/Billingconsultado, guardrail **USD10**, proyecto **USD50AGREGADOS**. LaunchTimeAWS original **A2026-11-13T05:03:37Z/B05:03:38Z**, hostsrunning pueden generar cargos. Ventanaoriginalsinreiniciar/máximo **2026-11-15T01:57:56.9337244Z**, sin timer/stop/cleanup automático.

Evidencia privada `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-ssm-docker-final-20261113T053800\`: baseline/hashes/state-selectivo/lecturasAWS/SendCommand/GetCommandInvocation/resultados/resumen. Temporal sin custodia duradera aceptada, sin secretos en documentación. Solo **task/memoria editados**, otrasfuentes/infra/.gitignore/state/backup/planes conservados porhash; Git final enmemoria/sesión. **Sin Terraform/plan/apply/destroy/import/IAMmodificado/cambiosconfig/deployment/stage/commit/push. DETENER tras evidencia**, sin siguiente incremento automático.

### 32.16. Checkpoint local post-Compute validado y pendientes exactos de GATE-I-C

**2026-11-13 UTC — autorización posterior de revisión, documentación, stage de cinco archivos y UN commit local**, mensaje exacto **`feat(infra): validate compute baseline`**. Baseline comprobado **feature/infrastructure /3730ebe2cd0c7851bcd827b1f73e355a673db61a**, cinco modificados esperados y **0 staged** al comenzar. No amend/squash/push, nuevas llamadas AWS/IAM/SSM, Docker pull/run, ALB ni nueva provisión. La identificación del checkpoint es el commit que contiene esta sección; SHA y resultado post-commit se entregan en la sesión y evidencia privada, sin segundo commit para autorreferenciarlo.

**Diff revisado de exactamente:** `.gitignore`, `infra/bootstrap/terraform-local-compute-policy.json`, `infra/compute.tf`, `specs/003-infrastructure/task.md`, `memoria.md`. HCL y condición ec2:InstanceType de candidata cambian únicamente **t3a.small→t3.small**; x86_64 y demás configuración conservados. `.gitignore` mantiene plan inicial y recovery/recovery2/recovery3/recovery4; state/backup/planes no se incorporan. La candidata es una **propuesta local, no copia completa de permisos remotos**: no contiene las correcciones informadas DescribeAddressesAttribute/ModifyInstanceAttribute ni la policy separada de verificación SSM. No se sincronizan, amplían ni aplican en este checkpoint. Correcciones humanas documentadas, operaciones realmente acreditadas y revisión IAM integral pendiente permanecen diferenciadas.

**Historia preservada:** primer apply parcial por **ec2:DescribeAddressesAttribute** → conservación/untaint de EIP originales → recovery2/asociaciones y rechazo **t3a.small/Free Tier** → cambio autorizado **t3.small** → recovery3/dos IDs y fallo **ec2:ModifyInstanceAttribute** → corrección IAM mínima manual informada → verificación material AWS/state/HCL y untaint EC2 conservador → recovery4 **0add/0change/0destroy/0replacements/28no-op** → policy SSM manual informada, SSM **Online** y Docker funcional final. Sin borrar fallos, ocultar tres applies parciales o reusar planes consumidos.

**Compute acreditado por evidencia previa fechada §§32.14–32.15, sin nueva consulta cloud:** **28 managed+3 data, serial39, 0 tainted**; A **i-0a66085b7b2411393/us-east-1a**, B **i-00de11a3f3c1a8aa5/us-east-1b**, running/t3.small/AMI **ami-096ef200a877f36af**, AL2023/IMDSv2required/10GiB gp3 cifrado. ENI **eni-01ecdecf22b187764/eni-049a982368fb2bca8**, EIP **18.210.153.223/44.195.4.217** originales; sin SSH/8080 públicos, 8080 solo desde SG ALB. Role **enver-dev-api-runtime**, boundary **ENVER-Runtime-Permissions-Boundary**. SSM A/B Online/agent **3.3.5828.0**, Docker **25.0.14/enabled/active**, CommandId **8abf5ba1-a1d6-4ac2-9592-a3200a3b9d81**, Success/0 ambos. **0 contenedores running y sin servicio Enver.API detectado**; API no desplegada por estos incrementos, no auditoría de procesos arbitrarios. Recovery4 SHA256 **bc682c534dfe5a13b267726f3d21744c6a5830654f925ded8c2a0488873d1662**; exit2 histórico por **+output compute_instances**, no cambios de recursos. No guardar outputs ni nuevo plan.

**Validaciones locales de este checkpoint**, entre **05:46:44.674Z y05:47:08.512Z**, todas **exit0**:

```text
terraform fmt -check -recursive infra
terraform -chdir=infra validate
bash -n infra/bootstrap/compute-host.sh
git diff --check
```

Solo avisos LF→CRLF, sin conversión masiva/init/instalación/bootstrap ejecutado. Stage limitado a los cinco archivos enumerados y condicionado a coherencia/diff-check; cached check/stat y post-commit se registran en evidencia privada/salida. No tocar state/backup/planes. El checkpoint no cierra tareas ni gates: **T005/T009/T010/T025 PARCIAL**, **GATE-I-C PENDIENTE /NO SATISFECHO**, 11parciales/19pendientes/9gatespendientes.

#### Matriz de cierre de GATE-I-C — fuentes §3, catálogo y AC-I-03

| Criterio real / relación | Estado | Evidencia existente | Evidencia faltante | Tarea asociada |
| --- | --- | --- | --- | --- |
| Dos EC2 reales en dos AZ — directo AC-I-03 | Acreditado técnicamente | Dos running/t3.small/AZ distintas, configuración y SSM/Docker §§32.14–15 | Incorporación/aceptación formal de E009 y E027–028, sin volver a negar existencia real | 009, 027–028 |
| Mismo digest .NET 10 en ambos hosts — directo AC-I-03 | Pendiente | ECR/digest/pull informados §22; Docker funcional A/B | Deploy autorizado y atribución del mismo digest inmutable ejecutándose en A/B, CI/commit/digest trazables | 007–009, 027–028 |
| Capacidad sin debilitar hashing — directo AC-I-03 | Parcial | 2vCPU/2GiB por host aprobados/justificados, dos hosts operativos | Capacidad real .NET/hasher demostrada según escenario/ventana aprobados; no sustituirla por docker --version | 009, 028 |
| Recuperación y límites de mantenimiento — directo AC-I-03 | Parcial | Estrategia ENI/EIP persistentes; recuperación conservadora real y hosts funcionales | Runbook/escenarios/rollback/drenaje/reemplazo y límites aceptados; demostración formal autorizada sin prometer cero downtime | 010, 028 |
| Registry y contrato de artefacto — E007/E008 requeridas | Parcial | Repository/lifecycle y configuración verificados; push/digest/pull informados; revisión imagen non-root/.NET | Aceptar E007/retención/dependencias; E008/ownership/contrato004/CI verde/manual/seguridad-reproducibilidad; scanning final (último IN_PROGRESS informado y ScanNotFound posterior sin hallazgos disponibles), rollback por digest | 007–008; dependencias 002/005/025; DP-I-14 |
| Aceptación SG/administración — dependencia de E009 y GATE-I-B | Parcial | Ingress8080 solo SG ALB, no SSH público, SSM funcional | Aceptación T006/matriz acceso directo-denegación, egreso y TLS interno/decisiones pendientes, no exigir ALB desplegado para acreditar hosts | 003–006, 028; DP-I-03/05/09 |
| Revisión IAM integral — prerrequisito GATE-I-B | Parcial | Role/profile/boundary verificados; SSM runtime funcional y verificaciones acotadas exitosas | Mínimo privilegio/separación integral/contratos de permisos y aceptación, sin equiparar policy humana informada con auditoría efectiva | 005; 019–020 según GATE-I-B |
| Custodia/backup/state y baseline aceptada — prerrequisito GATE-I-A | Parcial | Validaciones locales exit0, state/backup ignorados y hashes, recovery4 recursos0/0/0 | Protección/custodia/backup-recuperación y versiones/ambiente/presupuesto/dependenciasE001/E002/E025 aceptados; remoto/versionado/locking antes de uso compartido según T002/026, no migración automática | 001–002, 025; 026 para uso compartido |
| GATE-I-B networking/security completo — prerrequisito expreso | Pendiente | NETWORK14/SG/IAM y SSM acreditados parcialmente | Cierre E003–006, E019–020, E027–028: salidas Atlas, Redis sin acceso público, secretos/authority y pruebas/aceptaciones de ese gate; GATE-I-A previo | 003–006, 019–020, 027–028 |
| Evidencia cloud y aceptación formal — E027/E028 y §3 | Pendiente | Applies incrementales/recovery/hosts trazados, no aceptación integral de 027/028 | Provisión/deploy manual trazables y matriz de pruebas aplicable a AC-I-03 con autorizaciones/prerrequisitos del catálogo (026/004/CI/state protegido); aceptación explícita del responsable de E007–010/E027–028 y gates A/B/C | 026–028; 007–010 |

**Alcance exacto:** ALB/distribución/health de **GATE-I-D** y HTTPS/DNS/ACM de **GATE-I-E** no son nuevos criterios técnicos directos de Compute o AC-I-03. El catálogo T027 sí conserva sus dependencias de dominio/certificado/integración004; no se resuelven ni se inician aquí. Seguridad de Atlas/Redis/secretos/authority aparece en la matriz **por GATE-I-B**, no por inventar un nuevo criterio directo de I-C. La mera persistencia de +outputcompute_instances **no es un requisito de cierre de I-C** ni autoriza apply. API vacía hoy no satisface mismo digest ni capacidad; no se despliega para cerrar artificialmente el gate.

**Costo y límites conservados:** nominal≈USD2.5931/48h, guardrailUSD10/proyectoUSD50agregados, sin descuentoFreeTier/Billing real, ventana original máximo **2026-11-15T01:57:56.9337244Z** sin reiniciar/stop/timer/cleanup. No cambios002/004/AUTH-027integral. Evidencia privada local `C:\Users\Alexander Vega\AppData\Local\Temp\opencode\infra5-compute-checkpoint-20261113T054503\` (diffs/validaciones/hashes/Git); no sustituye custodia duradera aceptada. **Detener tras UN commit local y post-check; NO PUSH/plan/apply/AWS/IAM/ALB/deployment.**

## 33. GATE-I-A — Baseline local, copia restringida y recuperabilidad sin restore operativo

### 33.1. Autorización, baseline e inventario sin secretos

**2026-11-13 UTC — LOCAL SECURITY / ACCEPTANCE** autoriza inventario/Git/hash, state-list local, detección de protección/cifrado sin instalar, copia independiente y prueba aislada, validaciones y edición exclusiva task/memoria. **No autoriza** AWS/IAM, apply/destroy/import/refresh/taint/untaint/statepush, backend remoto/lockingS3-DynamoDB, T026, stage/commit/push o restore real. Baseline real **feature/infrastructure /0d9fdac009c527a8e3bb98a6b0094ae29607871e**, mensaje feat(infra): validate compute baseline, **árbol limpio/0staged** al comenzar. Pre-hashes incluyen fuentesinfra/.terraform/state/backup/planes/.gitignore/spec/plan para comprobar conservación.

Se ejecutan **git status --ignored --short**, **git ls-files** y **git check-ignore**. Inventario por ruta, existencia, tracked, ignored y clasificación, sin leer/imprimir credenciales ni contenido de state. Todos los siguientes existen, **NO tracked /SÍ ignored**:

| Ruta relativa | Clasificación |
| --- | --- |
| infra/.terraform/terraform.tfstate | Metadata sensible del backend local |
| infra/terraform.tfstate | State operativo sensible |
| infra/terraform.tfstate.backup | Backup nativo sensible, NO respaldo actual aceptado |
| infra/enver-network.tfplan | Plan guardado sensible |
| infra/enver-ecr.tfplan | Plan guardado sensible |
| infra/enver-ecr-post.tfplan | Plan guardado sensible |
| infra/infra4-ecr.tfplan | Plan guardado sensible |
| infra/infra4-ecr-replan-20261112-195037.tfplan | Plan guardado sensible |
| infra/tfplan-infra5 | Plan inicial parcialmente consumido, NO reutilizar |
| infra/tfplan-infra5-recovery | Plan histórico de recuperación, NO reutilizar |
| infra/tfplan-infra5-recovery2 | Plan parcialmente consumido, NO reutilizar |
| infra/tfplan-infra5-recovery3 | Plan parcialmente consumido, NO reutilizar |
| infra/tfplan-infra5-recovery4 | Plan histórico output-only, NO aplicar |

**No tfvars operativos/.auto.tfvars detectados.** No archivos nombrados como credenciales/keys/tokens/secretos/PEM/PFX/env detectados eninfra; inventario por nombres no prueba ausencia universal de secretos incrustados. **terraform.tfvars.example y .terraform.lock.hcl SÍ tracked**, apropiadamente versionables; ejemplo no operativo/no secreto y guardfalse, no tfvars real. Ignorar archivos **no cifra ni restringe acceso**.

### 33.2. State real, hashes y detección de protección

Backend **local**, ruta **infra/terraform.tfstate**; sin refresh. **terraform -chdir=infra state list exit0** devuelve **28managed+3data**, serial obtenido de JSON local **39**, **0tainted**. Se registran exclusivamente metadatos/hashes, no valores de recursos ni lineage publicado.

| Archivo | Serial / contenido de inventario | SHA256 |
| --- | --- | --- |
| State operativo | **39 /28managed+3data/0tainted** | **d214e2b172312123f64be213376130cad4e8be25fce36107ebbe258ffd045fff** |
| Backup nativo | **38 /28managed+3data/Btainted** | **23a74f1556b10ade14f509820a2a225d124201b03d52380fd39b5dc458314ea7** |

**ACL de state operativo y backup nativo:** herencia activa, entradas Allow de grupos amplios con lectura detectadas; **NO acceso exclusivo operador/SYSTEM**. No se publican usuarios/SIDs/ACL completas. ACL/contenidos originales no se alteran: los permisos restringidos pedidos para la nueva copia **no corrigen por inferencia los originales**. Requiere acción de custodia delimitada y autorizada antes de afirmar state protegido; no modificar ACL de todo el repositorio silenciosamente.

**Detección de cifrado en orden autorizado:**

1. **A — almacenamiento ya cifrado:** state y destino padre sin atributoEFS; Get-BitLockerVolume disponible, pero consultaC: falla **CimException**, sin certificar VolumeStatus/ProtectionStatus/método. **Cifrado del volumen NO acreditado**, no afirmación de que esté definitivamente sin cifrar. No habilitar/deshabilitarBitLocker/EFS ni administrar claves.
2. **B — 7-Zip:** no disponible enPATH ni ubicación estándar ProgramFiles revisada; no instalación ni búsqueda exhaustiva de todas las carpetas.
3. **C — OpenSSL:** binarioGit disponible; **openssl version exit0, OpenSSL3.5.7**. Sin contraseña/canal seguro provisionados; **no ejecutar cifrado**, no inventar clave ni guardarla en script/comando/log/documentación.

**Bloqueo exacto: “Cifrado requiere intervención segura del operador”.** Solo se detiene esa parte; no ZIP sin cifrado, DPAPI alternativo o contraseña por chat. El fallo de autoload de Get-Acl se resolvió por lectura con APIs.NET de ACL, sin instalar módulos o cambiar configuración.

### 33.3. Copia independiente actual con ACL restringida — NO backup cifrado aceptado

Se crea directorio nuevo **fuera del repositorio**, con DACL protegida desde su creación, propietario operador y únicos Allow **operador actual + SYSTEM**; sin gruposUsers/Everyone/AuthenticatedUsers/Administrators autorizados en esa DACL. No reparsepoint. Se copia **el state39 actual**, nunca backup38, con creación exclusiva y herencia del directorio protegido. Lectura posterior verifica ACL de directorio/archivo; DACL del archivo heredada solo de ese padre, sin otros Allow.

**Ruta conservada**, expresada con variable de perfil para no publicar nombre personal:

```text
%LOCALAPPDATA%\ENVER-State-Custody-20261113T060015-84d56283c2b446bca6c41772ceb4c9b6\terraform-state-serial39.restricted.tfstate
```

Creación **2026-11-13T06:02:10.458Z**, hash **d214e2b172312123f64be213376130cad4e8be25fce36107ebbe258ffd045fff**, idéntico al state39 original. No tracked/no susceptible de gitadd desde este repositorio; **ignored no aplica a ruta externa**, no afirmar protección por gitignore. No permisos públicos por DACL; no garantía contra SYSTEM/operador/administrador que tome posesión o acceso offline al disco. **Copia ACL-restringida SÍ creada; backup definitivo cifrado/protegido integralmente NO acreditado.** ACL sola no satisface cifrado; ruta no temporal del perfil no acredita automáticamente retención, backup off-device o política de custodia aceptada. No se introduce requisito off-device obligatorio distinto del destino/backup protegido aprobado del contrato vigente.

### 33.4. Prueba de recuperabilidad aislada y limpieza selectiva

Ayuda nativa de Terraform1.17.1 confirma **state list -state=statefile**. Una aserción inicial del agente esperaba el texto literal -state=path y se detuvo; diagnóstico de la misma salida demuestra bandera soportada, sin falloTerraform ni copia/restore previo. Incidente conservado en evidencia privada.

Desde la copia restringida39 se recupera **otro archivo temporal** en subdirectorio recovery-test de la misma custodia ACL, fuera deinfra; se verifica ACL operador/SYSTEM. Prueba concluida **2026-11-13T06:03:50.575Z**:

- Archivo legible, **JSON válido, serial39**, lineage coincide con original comparado en memoria/sin publicarlo.
- **SHA256 idéntico** al original39 y copia conservada.
- **terraform -chdir=infra state list -state=<ruta absoluta temporal> exit0**: mismas31direcciones y **28managed+3data** que state-list operativo.
- No overwrite del state real; no refresh/AWS/plan/statepush/import/apply. Comparación hash original igual antes/después.
- **Se eliminan únicamente archivo recuperado y su subdirectorio temporal exactos** creados por la prueba, mediante limpiezafinally; ausencia verificada. No eliminación recursiva amplia, ni state/backup nativos/copia conservada.

**Resultado:** recuperabilidad de **copia ACL-restringida no cifrada** acreditada. **NO prueba descifrado de un backup cifrado** ni restauración operativa/reconciliación cloud. Se conservan state39, backup38, diez planes y copia39restringida. Los logs externos no contienen contenido de state, claves o contraseñas.

### 33.5. Runbook de recuperación — procedimiento, NO ejecutado contra state real

1. Detener y excluir operaciones Terraform concurrentes; confirmar operador, cuenta/ambiente, backend y autorización de la intervención. No suponer exclusión distribuida por lock local.
2. Identificar state operativo y su serial/lineage/versión; registrar hash sin publicar valores. No asumir que39 será siempre el actual.
3. Localizar copia/backup protegido aprobado, verificar acceso/cifrado/retención y hash/manifiesto. La copia actual soloACL **no sustituye** el pendiente de cifrado.
4. Recuperar/descifrar a área temporal restringida fuera deinfra por mecanismo seguro del operador, nunca con contraseña en args/logs; **no reemplazar state real**.
5. Verificar JSON/ambiente/lineage/serial/hash del contenido restaurable y **terraform state list -state=<copia>**. Comparar inventario y antigüedad; rechazar backup38 como si fuera state39 válido equivalente.
6. Si falla una comprobación, **abortar**, aislar candidato y conservar operativo intacto; retirar solo temporales de la prueba, no forzar restore/import/statepush.
7. Exigir **autorización humana explícita antes de cualquier reemplazo real**, con reversión, copia íntegra previa y reconciliación prevista. No restore automático por arranque o error.
8. Después de un restore real autorizado, exigir **terraform plan nuevo con refresh bajo permiso posterior**, revisión de drift/replacements/output-only y coherencia con AWS antes de aceptar o mutar; **no ejecutar ese plan aquí** y no reutilizar recovery4/planes consumidos.
9. Si la validación post-restore falla, detener mutaciones y preservar ambas evidencias. Reversión al state previo solo con autorización humana y comprobación de coherencia con recursos reales; no sobrescribir automáticamente con otro state antiguo ni cambiar estado de seguridadRedis/authority.
10. Limpiar solo temporales autorizados; conservar respaldo protegido, hashes, decisiones y revisión, sin subir state/planes aGitHub.

### 33.6. Consolidación E001/E002/E025 y reevaluación estricta

**E001 consolidada /PARCIAL:** cuenta **AWS ENVER**, región **us-east-1**, ambiente **dev**, proyecto **enver**, owner operativo Terraform **operador autorizado del proyecto** (confirmado por esta solicitud, sin datos personales adicionales). Presupuesto **USD50 agregados**, guardrail operativo **USD10** de evaluación, duración máxima original48h/hito **2026-11-15T01:57:56.9337244Z**, **sin reiniciar ventana**. Compute nominal≈**USD2.5931/48h** = EC2t3.small2.0064+EBS≈0.1067+IPv4≈0.48, **sin descuentoFreeTier** ni consultaBilling/créditos reales. Excluye ECR/transferencia/logs/secretos/servicios futuros/impuestos; VPC/IGW/SG sin cargo directo no hacen gratis toda la plataforma. ALB/Atlas/Redis/authority/backendremoto no autorizados por este cierre. Falta aceptación de estimación integral/cargos aplicables por duración y decisionesE001 restantes; no tratar owner o presupuesto ya confirmados como desconocidos. El crédito no se usa para reducir estimación; cobertura no verificada no equivale a descuento.

**E002 consolidada /PARCIAL:** **terraform -chdir=infra version -json exit0**, Terraform **1.17.1**, AWS **6.73.0**; .terraform.lock.hcl versionado/conchecksums, restricciones sin cambiar. Ejemplo tfvars versionado/no operativo, ningún tfvars operativo detectado. Naming **enver-dev**, tags **Project=enver/Environment=dev/ManagedBy=Terraform** porlocals/provider; cuenta guardada por allowed_account_ids e inputs, precondiciones compute exigenbaselinecuenta/regiónus-east-1/ZoneIDs aprobados. No nueva autenticaciónSTS ni prueba universal de que cualquier recurso futuro herede guards equivalentes. State local y operador único definidos; **protección del operativo/cifrado-backup/retención y aceptación integral pendientes**, no basta copiar fuera deGit.

**E025 — TÉCNICAMENTE ACREDITADO en validaciones; aceptación integral PENDIENTE:** fmt-check **06:03:50.580Z–06:03:50.766Z/exit0**, validate **06:03:50.767Z–06:04:00.952Z/exit0**, bash-n **06:04:00.953Z–06:04:11.269Z/exit0**, diff-check previo documentación **06:04:11.270Z–06:04:11.417Z/exit0**. Version/state-list/copialist tambiénexit0; sin init/install/upgrade/plan/refresh/apply. Verificación de outputs/guards es lectura, no acceso cloud ni aceptación de todaIaCfutura. Diff-check posterior ystatus se registran al cierre.

| Tarea / gate | Estado vigente | Pendiente exacto |
| --- | --- | --- |
| T-INFRA-001 | **PARCIAL /E001 CONSOLIDADA** | Estimación integral/cargos/decisiones aplicables y aceptación explícita restantes; owner/ventana/presupuesto confirmados |
| T-INFRA-002 | **PARCIAL /BASELINE Y COPIA RESTRINGIDA RECUPERABLE ACREDITADAS** | Custodia del state operativo sin ACL amplias, cifrado-backup/retención/recuperación cifrada y aceptación E001/E002 |
| T-INFRA-025 | **PARCIAL /VALIDACIONES TÉCNICAMENTE ACREDITADAS** | Protección/backup integral y dependenciasE001-E002/aceptaciónE025 |
| T-INFRA-026 | **PENDIENTE /NO EJECUTADA** | T001–025 y decisiones posteriores aceptadas; este incremento no ejecuta026 |
| GATE-I-A | **PENDIENTE /NO SATISFECHO** | Persisten bloqueadores técnicos reales anteriores; además aceptación explícita E001/E002/E025. **NO meramente firma pendiente** |

**Remoto NO requerido ahora mientras uso local exclusivo**, no porque custodia local esté ya satisfecha. **Remoto protegido/cifrado/versionado/locking obligatorio antes de colaboración/GitHub/runner**, bootstrap/migración/prueba autorizados aparte; sinS3/DynamoDBlock creados. AuthorityDynamoDB deAuthentication no es lockTerraform. No seguir automáticamente aB/C/ALB/API. Onceparciales/19pendientes/9gatespendientes;002/004/AUTH-027integral intactos.

Evidencia externa ` %LOCALAPPDATA%\Temp\opencode\gate-ia-local-20261113T060015\ ` (inventario/Git/hash/ACL-detección/copia-resumen/recuperación-sinstate/validaciones/diagnósticos). No contiene copia de state y no acredita retención duradera por ser temporal. Copia restringida conservada **solo** en la ruta custodia de§33.3. Solo task/memoria editados enrepositorio; fuentesinfra/state/backup/planes/.gitignore/spec/plan preservados porhash. **Sin AWS/IAM/plan/apply/destroy/import/refresh/taint/untaint/statepush/migración/stage/commit/push. DETENER para revisión**, sin cifrado automático ni cambioACLoperativo implícito.

## 34. GATE-I-A — ACL finales del state; cifrado detenido por canal de passphrase no seguro

### 34.1. Autorización y precheck

**2026-11-13 UTC — LOCAL SECURITY / CUSTODY** autoriza exclusivamente ACL de **infra/terraform.tfstate** y **infra/terraform.tfstate.backup**, cifrado OpenSSL de la copia39 y prueba aislada si existe entrada segura, y task/memoria. Baseline comprobada **feature/infrastructure /0d9fdac009c527a8e3bb98a6b0094ae29607871e**, cambios iniciales esperados solo memoria/task de§33, **0staged**; no sobrescribir trabajo previo. No AWS, IAM, contenido state, owner innecesariamente, directorios padres, backend remoto, plan/apply ni stage/commit/push. T026 no se ejecuta.

**Hashes previos obligatorios y posteriores idénticos:**

| Archivo | Serial | SHA256 antes = después |
| --- | --- | --- |
| infra/terraform.tfstate | **39**,28managed+3data/0tainted | **d214e2b172312123f64be213376130cad4e8be25fce36107ebbe258ffd045fff** |
| infra/terraform.tfstate.backup | **38**,Btainted | **23a74f1556b10ade14f509820a2a225d124201b03d52380fd39b5dc458314ea7** |

Copia39 previa en custodia cotejada con hashstate39; destino.enc inexistente al comenzar. No contenido de state publicado. **whoami exit0** resuelve identidad Windows actual; SID de proceso obtenido dinámicamente para ACL, sin hardcodear cuenta. Nombre personal/máquina no se reproduce en documentación; **OPERADOR** designa esa identidad.

### 34.2. ACL antes/después — cambio de metadatos, no contenido

Antes, ambos archivos tenían herencia activa y las siguientes ACE Allow heredadas:

| Principal | Derechos |
| --- | --- |
| BUILTIN\Administrators | FullControl |
| SYSTEM | FullControl |
| BUILTIN\Users | ReadAndExecute, Synchronize |
| Authenticated Users | Modify, Synchronize |

No ACE Everyone encontrada; no se inventa su eliminación. Con APIs.NET de seguridad Windows se sustituye **solo DACL de esos dos archivos**, se deshabilita herencia sin conservar ACE heredadas y se dejan exactamente:

| Principal | Derechos | Tipo / herencia |
| --- | --- | --- |
| OPERADOR resuelto en ejecución | FullControl | Allow explícita /no heredada |
| SYSTEM | FullControl | Allow explícita /no heredada |

Verificación **2026-11-13T06:14:08.917Z**: **2ACE por archivo**, DACL protegida, ningún otro Allow ni ACE heredada; Users/AuthenticatedUsers/Administrators retirados de DACL, owners **sin cambio** y hashes exactos conservados. Sin owner/SID personal publicado, sin ampliar permisos a nadie más. State/nativo continúan **NO tracked /SÍ ignored** (reglas*.tfstate/*.tfstate.*); no gitadd. **Se modificaron ACL, NO bytes/serial/contenido**. Esto corrige el hallazgo de ACL de§33, no lo borra del historial.

**Límite de alcance:** no se endurece directorio padre infra ni se certifica aislamiento frente a administrador que tome posesión/operador/SYSTEM/acceso offline. Verificar ACL nuevamente después de futuras escrituras/reemplazos Terraform: un archivo nuevo podría heredar permisos del padre. No cambiar directorios/ACL adicionales automáticamente ni confundir esta verificación puntual con cifrado en reposo.

### 34.3. OpenSSL y bloqueo seguro — NO archivo cifrado creado

**openssl version exit0: OpenSSL3.5.7**, binario existenteGit. Mecanismo autorizado para la operación pendiente: **AES-256-CBC, salt aleatorio, PBKDF2 con200000iteraciones y digestSHA256**. No se ejecuta enc/decrypt porque las herramientas del agente no ofrecen un canal interactivo privado que garantice no registrar la passphrase. **No se pide/captura/genera contraseña**, no -pass pass:, variable de contraseña, archivo, script de secreto ni otro cifrado alternativo.

**“Cifrado requiere intervención segura del operador”.** No se crea .enc vacío o se presenta un ZIP/ACL como cifrado. Algoritmo/iteraciones son **parámetros propuestos, no cifrado acreditado**. Destino previsto, todavía inexistente:

```text
%LOCALAPPDATA%\ENVER-State-Custody-20261113T060015-84d56283c2b446bca6c41772ceb4c9b6\terraform-state-serial39-20261113.enc
```

**Comando exacto pendiente para consola PowerShell privada del operador**, fuera del agente y sin transcripción/grabación. Las variables siguientes contienen únicamente rutas, **no passphrase**; OpenSSL debe pedirla directamente con entrada oculta. No ejecutar si la consola registra entradas o si la copia/hash/ACL no coinciden:

```powershell
$custody = Join-Path $env:LOCALAPPDATA 'ENVER-State-Custody-20261113T060015-84d56283c2b446bca6c41772ceb4c9b6'
$source = Join-Path $custody 'terraform-state-serial39.restricted.tfstate'
$encrypted = Join-Path $custody 'terraform-state-serial39-20261113.enc'
if ([IO.File]::Exists($encrypted)) { throw 'Destino existente: detener y revisar, no sobrescribir' }
& 'C:\Program Files\Git\mingw64\bin\openssl.exe' enc -aes-256-cbc -salt -pbkdf2 -iter 200000 -md sha256 -in $source -out $encrypted
if ($LASTEXITCODE -ne 0) { throw 'Cifrado fallido: conservar plaintext y detener' }
```

El directorio custodia existente tiene ACL operador/SYSTEM, pero tras crear.enc se deben **aplicar/verificar explícitamente solo esas dos ACE** también al archivo cifrado antes de aceptarlo. Registrar ruta/tamaño/fecha/SHA256enc sin contraseña. **AES-CBC no es AEAD**: no afirmar autenticación criptográfica del archivo; preservar hash/manifiesto por canal confiable y comprobar hashplaintext esperado. No inventar hash/tamaño/fecha de un .enc no creado ni cambiar el mecanismo exigido.

### 34.4. Descifrado y limpieza — pendientes condicionados, no ejecutar ni simular

No descifrado, JSON/state-list de copia descifrada o temporal de esta prueba ejecutados; esos campos están **NO DISPONIBLES**, no reciclar la prueba de plaintext de§33 como prueba de cifrado. Ningún plaintext temporal nuevo creado, por lo que no hay temporal nuevo que eliminar.

La copia **terraform-state-serial39.restricted.tfstate** **se conserva** con ACL restringida y hash idéntico; su eliminación está autorizada **solo después** de .enc válido/descifrado/hash/JSON/state-list acreditados. Como esa condición no se cumple, borrarla ahora perdería el único respaldo independiente actual comprobado. No tocar state39/nativo38/planes o borrar archivos recursivamente.

### 34.5. Custodia, retención y runbook consolidado

**Owner:** operador autorizado ENVER. **Fuente:** terraform.tfstate serial39/hash indicado. **Backup objetivo:** AES-256-CBC/PBKDF2/SHA256/salt/200000iteraciones, acceso OPERADOR+SYSTEM. **Estado actual:** plaintext restringido conservado, .enc pendiente; no garantía de cifrado aún. **Retención aprobada para esta etapa:** conservar mientras este state sea baseline operativo vigente y hasta existir sucesor validado/protegido; no considerar backup38 sucesor39. No compartir por Git/correo/chat/repositorio. Passphrase solo bajo custodia segura del operador fuera del agente, no dentro del manifiesto o documentación. Remoto diferido hasta **antes de colaboración/GitHub/runner**, no implementado.

Runbook, **procedimiento futuro NO restore real ejecutado**:

1. Detener y excluir operaciones Terraform; verificar ambiente/backend/operador, state actual y serial/lineage/hash, sin asumir que39 siga siempre vigente.
2. Identificar .enc autorizado y verificar su SHA256 contra manifiesto confiable; si falla, abortar, no sobrescribir operativo ni aceptar archivo por nombre.
3. Crear área temporal fuera deinfra con ACL únicamenteOPERADOR+SYSTEM. Descifrar allí en consola privada mediante el mismo OpenSSL y opciones, añadiendo **-d**, invirtiendo -in/-out. Nunca contraseña enargs/archivos/logs.
4. Verificar exitcode, JSON y hashplaintext **d214e2b172312123f64be213376130cad4e8be25fce36107ebbe258ffd045fff** para este respaldo39; comparar lineage/serial sin publicar valores sensibles.
5. Ejecutar **terraform -chdir=infra state list -state=<archivo temporal absoluto>**:28managed+3data y mismas31direcciones del inventario autorizado. No refresh/statepush/import/apply.
6. Solo tras éxito de todas las verificaciones, registrar recuperabilidad/tamaño/hash/fechaenc y eliminar el temporal exacto de descifrado; eliminar copia plaintext custodiada solo con esa evidencia completa y condición de§34.4 satisfecha.
7. Para restauración real exigir **autorización humana posterior** y respaldar/proteger state operativo actual antes de considerar reemplazo. Nunca restore automático ni tomar backup38 como equivalente39.
8. Tras restore autorizado, ejecutar **plan fresco posterior con refresh autorizado/revisión de recursos** y detener si aparecen divergencias; no usar planes consumidos ni aplicar automáticamente.
9. Si falla verificación antes del restore, abortar sin tocar operativo. Si falla validación posterior, detener mutaciones y ejecutar rollback al previo únicamente con autorización/hash/coherenciaAWS comprobados, nunca state antiguo a ciegas ni rollbackRedis/authority/cuotas.
10. Conservar respaldo cifrado/manifiesto protegido según retención; retirar solo temporales exactos de la prueba, sin borrar state/nativo/planes o sucesores válidos.

### 34.6. E001 — estimación delimitada al alcance vigente, no a servicios inventados

CuentaAWSENVER/us-east-1/dev, owneroperador autorizado, **USD50 agregados/guardrailUSD10**, ventana original máximo **2026-11-15T01:57:56.9337244Z** sin reinicio, sin descuentoFreeTier/créditos/Billing asumidos. Se consolida lo conocido por etapa:

| Componente | Estado/evidencia | Estimación aplicable, NO consumo medido |
| --- | --- | --- |
| Dos t3.small | Vigentes según evidencia previa, sin nueva consultaAWS | 2×USD0.0209/h×48h = **USD2.0064** |
| EBS root total20GiB | Vigente | 20×USD0.08/GiB-mes×48/720 ≈ **USD0.1067** |
| Dos IPv4 públicas/EIP | Vigentes; detener EC2 no elimina necesariamente este cargo | 2×USD0.005/h×48h = **USD0.48** |
| VPC/subnets/IGW/rutas/SG e IAM | Baseline existente | Sin cargo recurrente directo previamente identificado; transferencia no incluida |
| ECR repository/imagen | Vigente;101033417bytes y3objetosOCI informados, no volumen total facturable inventado | Tarifa histórica documentadaUSD0.10/GB-mes; **USD0.10×GB-mes facturables**, volumen/retención real no reconsultados |
| Transferencia aplicable | Puede generar cargo por uso real | Volumen/origen/destino/tarifa aplicable y reserva aceptada pendientes; no fijar costo0 ni inventar consumo |
| State/custodia/OpenSSL local | Local, sin nuevo recursoAWS | No nuevo servicioAWS con cargo por este hardening |
| ALB/HTTPS/DNS, Redis, Atlas, authority, logs/secretos y backendremoto | Etapas futuras no creadas por este incremento | Estimaciones y autorizaciones **por etapa pendientes**, no sumarlos como consumo actual ni seleccionarlos aquí |

**Subtotal fijo conocido≈USD2.5931/48h**, no total de factura. Exposición estimada de alcance vigente = subtotal fijo + almacenamientoECR + transferencia aplicable y cargos/impuestos que correspondan. Falta completar/aceptar cotas de variables aplicables y estimación agregada por duración; margen nominal alguardrail no demuestra factura/tope duro. No condicionar E001 a desplegar servicios futuros ni afirmar cotización integral exacta sin datos. **E001PARCIAL**, presupuesto/owner/ventana definidos, aceptación explícita y estimación variable restantes pendientes.

### 34.7. E002/E025 y decisión del gate

**E002 consolidada:** Terraform1.17.1/AWS6.73.0, lock versionado, ejemplo no operativo, sin tfvarsoperativos, namingenver-dev/tagsProjectenver-Environmentdev-ManagedByTerraform y guards cuenta/región conservados; sin cambios de versiones/config. ACL de state/nativo **ahora restringidas y verificadas**, resuelto ese bloqueador. Remoto diferido localexclusivo, obligatorio antes de compartir. Cifrado y recuperabilidad de.enc pendientes, por tanto **E002/T002PARCIAL**, no todo técnicamenteacreditado.

**E025:** fmt-check **06:14:25.483Z–06:14:25.675Z/exit0**, validate **06:14:25.678Z–06:14:36.253Z/exit0**, bash-n **06:14:36.256Z–06:14:46.865Z/exit0**, diff-checkpredocumentación **06:14:46.867Z–06:14:47.070Z/exit0**. Cotejo de hashes postACL y comprobación gitignore/no tracked exitosos. Validaciones **TÉCNICAMENTE ACREDITADAS /ACEPTACIÓN EXPLÍCITA PENDIENTE en ese alcance**; E025integral/T025 **PARCIAL** por dependencias/cifrado-recuperación faltantes. Sin plan/refresh/init/install/apply.

**T001PARCIAL /T002PARCIAL /T025PARCIAL; T026PENDIENTE /NO EJECUTADA; GATE-I-A PENDIENTE /NO SATISFECHO.** Pendiente exacto: ejecución cifrada segura por operador, ACL/metadatos/hash.enc y descifrado/JSON/hash/state-list/limpieza comprobados; completar estimación E001 de variables aplicables y aceptación explícita E001/E002/E025. **No solo firma pendiente**. Retención ya definida; no mantener como desconocidosowner/retención/ACL actuales. Onceparciales/19pendientes/nuevegatespendientes, resto002/004/AUTH-027integral intactos.

Evidencia privada **%LOCALAPPDATA%\Temp\opencode\gate-ia-acl-final-20261113T061331\**: baseline/hashantes, whoami privado, principals/permisos saneados antes-después, resultadoACL/owner-hash, versión/validaciones/ignorados/integridad final. Sin contenido state ni contraseña. Solo dos DACL autorizadas y task/memoria cambiados; hashes de contenido infra/state/nativo/custodia/planes y fuentesprevias conservados. **Sin AWS/IAM/enc/decrypt/restoreoperativo/plan/apply/migración/stage/commit/push. DETENER tras evidencia y comando manual**, no capturar passphrase por chat.

**Cierre local comprobado:** a **06:22:09.589Z**, las **40 entradas** del manifiesto de contenido siguen idénticas; ningún archivo nuevo en infra, copia plaintext39 intacta y .enc ausente, sin temporales de recuperación nuevos. Branch/HEAD originales, solo memoria/task modificados, **0 staged**, state/nativo ignorados y no tracked, **diff-check exit0**; historial íntegro de §33 y memoria§92 preservado. A **06:23:22.600Z**, relectura confirma las dos DACL operativas explícitas y protegidas. Una aserción adicional inicialmente exigió erróneamente herencia deshabilitada a la copia custodiada preexistente: diagnóstico de solo lectura confirma que sus dos ACE OPERADOR/SYSTEM son heredadas de su padre protegido y siguen restringidas. No fue fallo del hardening operativo ni motivó cambios adicionales de ACL; copia y padre conservados.

## 35. GATE-I-A — Verificación posterior a recuperación cifrada y limpieza autorizada

### 35.1. Alcance, procedencia de evidencia y baseline

**2026-11-13 UTC — LOCAL SECURITY / FINAL VALIDATION**, autorización específica del responsable para verificar el .enc esperado, asegurar únicamente su ACL explícita, eliminar los dos plaintext designados tras comprobar .enc/hash, validar localmente y editar solo task/memoria. **Sin AWS/IAM/plan/apply/backend remoto/stage/commit/push**, sin repetir cifrado/descifrado ni acceder a la passphrase. Precheck **06:35:31.029Z**, branch **feature/infrastructure**, HEAD **0d9fdac009c527a8e3bb98a6b0094ae29607871e**, solo task/memoria previamente modificados, **0 staged**; conservar historial y trabajo anterior.

El operador confirma manualmente: backup cifrado creado, descifrado exitoso, hash plaintext39 correcto, JSON/state-list válidos, **31 direcciones / 28 managed + 3 data**; passphrase no compartida ni registrada. Esto se registra como **evidencia humana confirmada**, no como ejecución OpenSSL/state-list del agente. En esta intervención se agregan comprobaciones directas del archivo cifrado y del plaintext restaurado todavía existente antes de su eliminación.

### 35.2. Backup cifrado: existencia, integridad y ACL

| Campo | Evidencia directa |
| --- | --- |
| Ruta | **%LOCALAPPDATA%\ENVER-State-Custody-20261113T060015-84d56283c2b446bca6c41772ceb4c9b6\terraform-state-serial39-20261113.enc** |
| Existe / tamaño | **Sí / 71696 bytes**, mayor que cero |
| SHA256 esperado = observado antes/después de ACL y limpieza | **ED36CCC43BC3C93D22B483F1EC05BD3AFFD6DF65F38F435B8A9411BBF6AEE0F2** |
| Creación UTC informada por filesystem local | **2026-11-13T06:28:38.190Z** |
| Última escritura UTC | **2026-11-13T06:28:38.408Z** |
| ACL anterior | Dos Allow OPERADOR/SYSTEM FullControl heredadas del padre protegido |
| ACL posterior, **06:36:05.087Z** | **Exactamente dos Allow explícitas OPERADOR/SYSTEM FullControl; sin herencia ni otros principals** |
| Contenido / propietario | Hash intacto / propietario no cambiado |

Operador/SID resueltos dinámicamente. No Users, Authenticated Users, Everyone ni Administrators en la DACL final del .enc; no cambio en ACL padres/copia previa/state/nativo. Mecanismo del **procedimiento acordado de §34.3**: OpenSSL3.5.7, AES-256-CBC/salt/PBKDF2/SHA256/**200000 iteraciones**; no inferir parámetros de KDF/algoritmo únicamente a partir de extensión/hash ni presentar una nueva ejecución del agente. CBC no es AEAD: SHA256/manifiesto deben preservarse por canal confiable, sin afirmar autenticación criptográfica del contenedor ni registrar passphrase.

### 35.3. Recuperación y eliminación exacta de plaintext

Se localizó **%TEMP%\ENVER-State-Restore-Test\terraform-state-serial39-restored.tfstate**, no directamente en la raíz de TEMP. Antes de borrar, se comprobó directamente en ese archivo y en la copia custodiada: **JSON válido, serial39, lineage idéntico al operativo sin publicarlo, 28 instancias managed + 3 data** y SHA256 **d214e2b172312123f64be213376130cad4e8be25fce36107ebbe258ffd045fff**. El hash byte a byte idéntico confirma contenido del state39. La ejecución Terraform state-list válida y sus31direcciones provienen de la confirmación humana; no se repite descifrado/state-list ni se copia state a evidencias.

Solo tras comprobar .enc no vacío/hash esperado y esas precondiciones se eliminaron, a **06:38:27.475Z**, exactamente:

1. **%TEMP%\ENVER-State-Restore-Test\terraform-state-serial39-restored.tfstate**.
2. **%LOCALAPPDATA%\ENVER-State-Custody-20261113T060015-84d56283c2b446bca6c41772ceb4c9b6\terraform-state-serial39.restricted.tfstate**.

Ambos ausentes después; directorio de custodia contiene **únicamente terraform-state-serial39-20261113.enc**. No eliminación recursiva ni de directorios/otros archivos. Se conserva directorio temporal vacío, no otro plaintext de esta prueba. Es eliminación lógica de los archivos designados, **no borrado físico seguro, barrido de discos ni garantía sobre copias ajenas**. No se guarda plaintext en registros. Se conservan state operativo, backup nativo y todos los planes.

### 35.4. Operativo, histórico, custodia y recuperación futura

| Archivo conservado | SHA256 directo antes/después | ACL observada |
| --- | --- | --- |
| **infra/terraform.tfstate**, serial39 | **d214e2b172312123f64be213376130cad4e8be25fce36107ebbe258ffd045fff** | OPERADOR+SYSTEM FullControl explícitos, sin herencia/grupos amplios |
| **infra/terraform.tfstate.backup**, serial38 histórico/Btainted | **23a74f1556b10ade14f509820a2a225d124201b03d52380fd39b5dc458314ea7** | Mismo criterio, intacto |

**State real nunca reemplazado**; no bytes, ACL ni propietarios operativos cambiados. Backup38 conserva taintB: **NO es backup actual equivalente ni candidato de recuperación automática**. Hash exacto preserva esta evidencia histórica, no hay refresh/plan/persistencia de output recovery4.

Owner **operador autorizado ENVER**; fuente39/hash conocido, respaldo actual cifrado39 recuperable según evidencia descrita, acceso OPERADOR+SYSTEM. Retener mientras39 sea baseline vigente y hasta sucesor validado/protegido; no compartir por Git/correo/chat/repositorio. Passphrase bajo custodia segura del operador, fuera de registros/agente. Backend remoto protegido/cifrado/versionado/locking **diferido hasta antes de colaboración/GitHub/runner**, no implementado. Runbook §34.5 conservado: para pruebas futuras usar .enc a temporal restringido; no recrear permanentemente copia plaintext ya retirada ni volver a ejecutar el comando de §34.3 cuya fuente ya no existe. Restore real exige autorización nueva, respaldo actual, hashes/JSON/state-list y plan posterior autorizado; nunca apply/rollback automático.

### 35.5. E001 — consolidación sin aceptación artificial de costo integral

Responsable reconfirma **USD50 agregados, guardrailUSD10, owner definido, ≈USD2.5931/48h nominal compute/EBS/IPv4**; us-east-1/dev y ventana original **2026-11-15T01:57:56.9337244Z**, **sin reinicio**, sin FreeTier/créditos descontados ni Billing medido. Desglose fijo, fórmulas ECR/transferencia y exclusiones de §34.6 siguen vigentes. No nueva consulta AWS, tarifa ni volumen inventados.

ALB/HTTPS/DNS/Redis/Atlas/authority/logs/secretos/backend remoto futuros se mantienen como **estimaciones/autorizaciones pendientes por etapa**, no consumo actual. Sin embargo, ECR y transferencia **aplicables al alcance ya vigente** no desaparecen por ser variables: T001 pide estimación fechada/regional y lista de cargos, no solo monto máximo. La confirmación humana de recuperación no aporta cotas/volúmenes/reserva aceptada de esas variables. **E001/T001 PARCIAL**, presupuesto/owner/ventana ya definidos; falta completar o aceptar explícitamente la estimación aplicable con sus supuestos/riesgo y aceptación integral. No exigir desplegar servicios futuros ni investigar Billing fuera de autorización; no usar guardrail como tope técnico de factura.

### 35.6. E002/E025 y decisión estricta de GATE-I-A

**E002 / componente técnico de T002: TÉCNICAMENTE ACREDITADO / ACEPTACIÓN EXPLÍCITA PENDIENTE** para baseline/custodia local. Lectura local `terraform -chdir=infra version -json` **exit0** confirma **Terraform1.17.1/providerAWS6.73.0**. Lockfile y terraform.tfvars.example versionados, ejemplo no operativo, **0 tfvars reales**; naming/tags/guards aprobados previamente y sin cambios. State ACL restringida y backup cifrado recuperable acreditados según procedencias anteriores; remoto diferido con límite explícito. La dependencia T001 y aceptación integral mantienen T002 formalmente **PARCIAL**, no nuevamente bloqueada por cifrado resuelto.

**E025 / componente técnico de T025: TÉCNICAMENTE ACREDITADO / ACEPTACIÓN EXPLÍCITA PENDIENTE** para validación inicial local:

| Comando | Inicio–fin UTC, 2026-11-13 | Exit |
| --- | --- | --- |
| terraform fmt -check -recursive infra | 06:38:49.648Z–06:38:49.850Z | **0** |
| terraform -chdir=infra validate | 06:38:49.855Z–06:39:00.364Z | **0**, configuración válida |
| bash -n infra/bootstrap/compute-host.sh | 06:39:00.368Z–06:39:12.381Z | **0** |
| git diff --check, antes de actualizar documentos | 06:39:12.383Z–06:39:12.624Z | **0**, solo avisos LF/CRLF |

Protección state/nativo/.enc y recuperación/limpieza ya verificadas; aceptación explícita y dependencias pendientes mantienen T025 formalmente **PARCIAL**. No repetir init/install/upgrade/plan/refresh/apply, ni confundir esta validación con T026, drift cloud o toda IaC futura.

**GATE-I-A: PENDIENTE / NO SATISFECHO; aún NO íntegramente TÉCNICAMENTE ACREDITADO ni LISTO SOLO PARA ACEPTACIÓN EXPLÍCITA.** El requisito real de §3 exige E001/E002/E025 y presupuesto/versiones/ambiente/state aprobados. Ya no faltan cifrado, recuperación o limpieza. **Pendiente exacto:** completar/aceptar estimación E001 de variables aplicables al alcance vigente y aceptación explícita integral E001/E002/E025 y del gate. No meramente una firma, ni desconocimiento de owner/presupuesto/retención. T001/T002/T025 conservan PARCIAL con los subalcances técnicos indicados; **T026 PENDIENTE / NO EJECUTADA**, once tareas parciales/19pendientes/nueve gates pendientes. B/C, 002/004 y AUTH-027 integral no cambian.

Evidencia privada **%LOCALAPPDATA%\Temp\opencode\gate-ia-post-encrypted-20261113T063531\**: precheck/manifiesto40entradas, .enc ruta-tamaño-fechas-hash, ACL antes/después saneadas, precondiciones/limpieza exacta sin state, versiones/validaciones/Git/integridad final. No contenido state ni passphrase. Única modificación técnica de metadatos: ACL del .enc; únicas eliminaciones: los dos plaintext autorizados; únicos documentos editados: task/memoria. **Sin AWS/IAM/descifrado repetido/restore operativo/plan/apply/backend remoto/stage/commit/push. DETENER para revisión.**

**Cierre verificado 06:43:15.669Z:** 40 entradas del manifiesto intactas y ningún archivo nuevo en infra; .enc/hash/tamaño conservados, solo .enc en custodia, ambos plaintext ausentes y búsqueda del nombre restaurado en TEMP sin coincidencias. State39/0tainted y nativo38/1tainted intactos; relectura ACL de los tres archivos a06:43:10.328Z confirma exclusivamente OPERADOR+SYSTEM explícitos/sin herencia. Historial task§34/memoria§93 preservado, branch/HEAD originales, **solo task/memoria modificados / 0 staged / diff-check exit0**, state/nativo ignorados y no tracked. Sin commit/push ni cierre artificial de gate.

## 36. GATE-I-A — E001 técnicamente acreditada y paquete listo para aceptación explícita

### 36.1. Alcance y baseline documental

**2026-11-13 UTC — DOCUMENTATION / ACCEPTANCE ONLY**, solicitud expresa del responsable para completar el modelo E001 y preparar aceptación, **no para aceptarla automáticamente**. Precheck **06:47:08.597Z**: **feature/infrastructure / 0d9fdac009c527a8e3bb98a6b0094ae29607871e**, solo memoria/task previamente modificados, **0 staged**. Únicos documentos editables: task/memoria. Se autorizan expresamente los cuatro comandos locales de §36.4; no AWS, IAM, cambios infra/custodia, plan/apply, stage/commit/push ni T026.

Los precios y supuestos siguientes son **modelo documental aplicable comunicado por el responsable en esta solicitud**, no lectura AWS/Billing ni medición de consumo del agente. La publicación/tamaño de imagen conserva su procedencia humana de §22. Trazabilidad E001: INFRA-001–003/033/038, PLD-INFRA-001/020, AC-I-01/15; E002/E025 y su trazabilidad previa se conservan.

### 36.2. Estimación E001: us-east-1/dev, ventana original de evaluación

| Componente actual modelado | Base / fórmula nominal | USD por48h, aproximado |
| --- | --- | --- |
| EC2 | 2t3.small × USD0.0209/h ×48h | **2.0064** |
| EBS | 20GiB × USD0.08/GiB-mes ×48/720h | **0.1067** |
| IPv4 | 2IPv4 × USD0.005/h ×48h | **0.48** |
| Subtotal compute/EBS/IPv4 | Baseline previamente aprobado, sin descontar FreeTier | **2.5931** |
| ECR privado, storage imagen | **101033417bytes /10⁹ =0.101033417GB**; ×USD0.10/GB-mes ×48/720h | **0.0006736 ≈0.0007** |
| ECR us-east-1 → EC2 us-east-1 | Pulls misma región, según modelo de pricing comunicado | **0 esperado** |
| Baseline nominal conocido | 2.5931 +0.0006736 +0 | **2.5937736 ≈2.594** |

Prorrateo documental **mes convencional720h**, no conversión de presupuesto a mensual. ECR es una aproximación proporcional al tamaño de imagen informado, **no medición del total facturable de todas las capas/manifiestos/repositorios o retención**. Los3objetosOCI informados no son3versiones ni3veces ese tamaño por inferencia. Si cambian volumen/retención/artefactos se actualiza la estimación antes de ampliar alcance. Modelo regional fechado, no cotización contractual ni Billing real.

**Transferencia Internet, separada de pulls same-region:** AWS contempla **100GB/mes agregados de DTO a Internet sin cargo para servicios aplicables**, según el modelo comunicado; elegibilidad/cobertura/consumo agregado real **no acreditados aquí**. No descontar esa franquicia ni depender de ella para declarar viable el proyecto. Consumo futuro Internet = **VARIABLE / NO ACREDITADO**, sin inventar GB ni fijar toda transferencia enUSD0. Debe encuadrarse dentro del **guardrail operativoUSD10 de la ventana**, con revisión del responsable antes de nuevas cargas/costos/ampliación; no crear con esta documentación un cap técnico, alarma, timer o apagado automático. Transferencia interregional, Internet u otras rutas/servicios no quedan cubiertos por la afirmación de costo cero de ECR→EC2 same-region.

**Servicios futuros excluidos del consumo actual de este incremento:** ALB, ACM, Redis, DynamoDB, Secrets Manager, CloudWatch adicional, Route53 y Atlas de pago, mientras no estén desplegados/aprobados según evidencia y alcance. Cada uno: **“estimación por etapa pendiente antes de su provisión”**. No inventar despliegue/cargo ni sumarlos arbitrariamente; tampoco afirmar que serían siempre gratuitos. Red/IAM conservan el tratamiento previo de costo recurrente directo, sin ocultar transferencia aplicable.

### 36.3. Presupuesto, duración y ownership

| Control | Valor conservado / interpretación |
| --- | --- |
| Proyecto completo | **USD50 agregados**, no mensuales |
| Guardrail operativo de la ventana | **USD10**, sin enforcement automático acreditado |
| Baseline nominal conocido | **≈USD2.594/48h**, no factura total ni consumo medido |
| Margen nominal contra guardrail | 10−2.594 = **≈USD7.406** |
| Margen nominal contra presupuesto agregado | 50−2.594 = **≈USD47.406** |
| Deadline operativo | **2026-11-15T01:57:56.9337244Z**, ventana original **NO reiniciada ni extendida** |
| Owner operativo Terraform | **operador autorizado ENVER** |
| Owner de presupuesto | **responsable del proyecto** |

Los márgenes son **restas aritméticas del baseline nominal**, no saldo real disponible del proyecto/cuenta: no se han descontado cargos previos/acumulados ni medido variables/impuestos. No garantizan que factura/transferencia se mantengan bajo el umbral. No FreeTier/crédito presumidos, sin nueva bolsa deUSD50/10 ni reinicio de48h. La conservación del deadline no crea timer/stop/cleanup.

**Cada fase nueva con costo exige estimación previa, revisión, autorización y actualización del baseline** antes de provisión. Márgenes, aprobación de una estimación y futura aceptación de I-A **no autorizan automáticamente recursos, duración adicional, apply o deployment**.

### 36.4. E001/E002/E025 preparadas y validaciones locales

**E001 / T-INFRA-001: TÉCNICAMENTE ACREDITADO / ACEPTACIÓN EXPLÍCITA PENDIENTE.** Cuenta/región/ambiente, monto/guardrail, duración/owners y baseline de cargos conocidos están documentados; ECR y transferencia same-region quedan cuantificados según modelo explícito del responsable, Internet queda como variable bajo control presupuestario y etapas futuras separadas. Esta solicitud completa **la estimación y tratamiento del riesgo**, no acredita Billing real ni acepta formalmente E001. El bloqueo técnico indicado en §§34–35 queda superado por información posterior, sin alterar su historial.

**E002 / T-INFRA-002: TÉCNICAMENTE ACREDITADO / ACEPTACIÓN EXPLÍCITA PENDIENTE.** Se mantiene evidencia de §35: Terraform1.17.1/AWSprovider6.73.0, lock/ejemplo versionados, sin tfvars reales, naming/tags/guards y state local protegido, .enc39 recuperable, ACL restringidas y plaintext retirados. No reabrir cifrado/recuperación ni repetirlos; no presentar evidencia previa como prueba nueva de esta intervención. Backend remoto protegido/cifrado/versionado/locking obligatorio **antes de compartir/GitHub/runner**, diferido, sin migración. T001 ya está preparada técnicamente, su aceptación sigue pendiente.

**E025 / T-INFRA-025: TÉCNICAMENTE ACREDITADO / ACEPTACIÓN EXPLÍCITA PENDIENTE**, en validación inicial local. Comandos expresamente autorizados, sin descarga/init/upgrade/refresh/plan/apply:

| Comando | Inicio–fin UTC, 2026-11-13 | Exit |
| --- | --- | --- |
| terraform fmt -check -recursive infra | 06:47:34.521Z–06:47:34.721Z | **0** |
| terraform -chdir=infra validate | 06:47:34.730Z–06:47:45.743Z | **0**, configuración válida |
| bash -n infra/bootstrap/compute-host.sh | 06:47:45.746Z–06:47:57.533Z | **0** |
| git diff --check, antes de actualizar documentos | 06:47:57.536Z–06:47:57.759Z | **0**, solo avisos LF/CRLF |

E025 no acredita cloud, toda IaC futura, drift nuevo, output recovery4 persistido ni T026. Evidencia técnica de dependencias preparada; aceptación humana de cada E continúa pendiente.

### 36.5. GATE-I-A — listo para aceptación, sin cierre automático

**GATE-I-A: TÉCNICAMENTE ACREDITADO / LISTO PARA ACEPTACIÓN EXPLÍCITA.** No queda bloqueo técnico en el alcance de baseline local de §3: E001 estimada con modelo/riesgos explícitos, E002/custodia ya acreditadas, E025 validada realmente. **Cierre formal PENDIENTE / NO SATISFECHO** hasta aceptación expresa del responsable de **E001, E002, E025 y GATE-I-A**; no interpretar esta solicitud de preparación como esa aceptación.

Paquete a aceptar: cuenta/ambiente/versiones; presupuesto agregado/guardrail y estimación con sus límites/riesgo variable; ownership/deadline; custodia/backups/runbook y restricción de uso local; resultados E025. No se registra firma, aprobador o acto de aceptación inexistentes. Tampoco se conceden permisos para tareas posteriores al aceptar este paquete.

Estado de catálogo: **3 tareas técnicamente acreditadas con aceptación pendiente (001/002/025), 8 parciales restantes y19 PENDIENTE / NO EJECUTADA**, ninguna completada por esta solicitud. **T-INFRA-026 PENDIENTE / NO EJECUTADA**. Los9gates siguen sin cierre aceptado; I-A listo para aceptación, B–I pendientes con requisitos propios. Historial recovery4/INFRA-5, 002/004 y AUTH-027 integral intactos.

Evidencia privada **%LOCALAPPDATA%\Temp\opencode\gate-ia-e001-acceptance-20261113T064708\**: precheck/modelo aritmético/procedencia documental/manifiesto40entradas/validaciones e integridad/Git. Sin contenido state, secretos o passphrase. Solo task/memoria editados; no cambios a AWS/IAM/infra/custodia/spec/plan ni stage/commit/push. **DETENER para aceptación explícita humana**, sin iniciar otro incremento.

## 37. GATE-I-A — Aceptación explícita humana y checkpoint local de documentación

### 37.1. Acto de aceptación y autorización delimitada

**2026-11-13 UTC — ACCEPTANCE / DOCUMENTATION / LOCAL GIT.** En su mensaje de esta solicitud, el **responsable del proyecto otorga aceptación humana explícita a E001, E002 y E025**, sobre la evidencia acumulada hasta la baseline actual, y **autoriza cerrar GATE-I-A = SATISFECHO**. No es inferencia a partir de validaciones, una firma criptográfica, ticket o aprobación externa; la fuente es la instrucción expresa del responsable en esta conversación. Fecha/contexto registrados aquí; historial de preparación y bloqueos de §§33–36 intacto.

Precheck **06:55:17.069Z**, **feature/infrastructure / 0d9fdac009c527a8e3bb98a6b0094ae29607871e**, solo task/memoria previamente modificados y **0 staged**. Se autoriza actualizar únicamente esos dos documentos y sus estados, las cuatro validaciones locales, revisar sus diffs y crear **UN commit local** con mensaje exacto **docs(infra): accept gate i-a baseline**. Sin amend/squash/push, AWS/IAM, cambios infra/custodia, plan/apply/refresh/import/state push/destroy ni ejecución de tareas posteriores. El checkpoint incorpora también la evidencia documental acumulada y autorizada en §§33–36, no solo las líneas de este acto.

### 37.2. Evidencia aceptada y alcance de las tareas

| Evidencia / tarea | Estado por aceptación explícita | Alcance aceptado y límites conservados |
| --- | --- | --- |
| **E001 / T-INFRA-001** | **ACEPTADA / COMPLETA / ACEPTADA** | Cuenta ENVER/us-east-1/dev, proyectoUSD50 agregado, guardrailUSD10, baseline≈USD2.594/48h compute/EBS/IPv4/ECR aproximado; pull ECR→EC2 same-region USD0 esperado según modelo documental, Internet VARIABLE/NO ACREDITADO, sin depender de descuentosFreeTier, owners definidos y ventana original |
| **E002 / T-INFRA-002** | **ACEPTADA / COMPLETA / ACEPTADA** | Terraform1.17.1/AWSprovider6.73.0, lock/ejemplo versionados/no operativo, ningún tfvars real detectado, naming enver/dev, tagsProject=enver/Environment=dev/ManagedBy=Terraform, state local y nativo con ACL OPERADOR+SYSTEM, backup cifrado39 recuperable y plaintext retirado |
| **E025 / T-INFRA-025** | **ACEPTADA / COMPLETA / ACEPTADA** | Validación inicial real fmt/validate/bash-n/diff-check exit0 y state/backup protegidos; baseline compute reconciliada y recovery4 histórico recursos0add/0change/0destroy/0replacements. No revisión final de toda IaC/cloud/runtime ni T026 |

Presupuesto y retención no se reinician. **Deadline original 2026-11-15T01:57:56.9337244Z**, sin extensión; cada etapa futura exige estimación/revisión/autorización propia. Márgenes nominales no son saldo real, descuentos ni permiso de gasto adicional. Owner operativo Terraform **operador autorizado ENVER**, owner presupuesto **responsable del proyecto**.

Backup actual aceptado **terraform-state-serial39-20261113.enc**, SHA256 **ED36CCC43BC3C93D22B483F1EC05BD3AFFD6DF65F38F435B8A9411BBF6AEE0F2**; mecanismo aceptado del procedimiento **AES-256-CBC/PBKDF2/200000iteraciones/SHA256/salt**. Recuperabilidad acreditada con fuentes diferenciadas en §35: hashplaintext39 correcto, JSON válido,31direcciones/28managed+3data; copia plaintext de custodia y restaurado temporal eliminados. No contraseña en documentos ni nueva operación de cifrado/descifrado. State39 operativo y nativo38 histórico/Btainted conservados; nativo38 no se convierte en respaldo actual equivalente. Recovery4 mantiene **exit2 por +output compute_instances**, no drift de recursos ni apply para guardar output.

Se acepta custodia local exclusiva con **backend remoto protegido/cifrado/versionado/con locking antes de colaboración/GitHub/runner**, autorización de migración posterior separada. No compartir ni publicar contenido del state por Git, correo, chat o repositorio; la condición previa al uso compartido no autoriza publicar state. La aceptación de T002 no declara backend remoto implementado.

### 37.3. Estados resultantes y siguiente gate

**GATE-I-A = SATISFECHO**, evidencia y aceptación explícita presentes. **E001/E002/E025 ACEPTADAS; T001/T002/T025 COMPLETA / ACEPTADA.** Cierre limitado al criterio real de baseline local, sin relajar otros AC ni aprobar retrospectivamente todos los cambios o dependencias de infraestructura.

**Siguiente gate: GATE-I-B = PENDIENTE / NO SATISFECHO**, con su prerrequisito **GATE-I-A ahora SATISFECHO**. No se ejecuta I-B ni se aceptan E003–006/E019–020/E027–028 por inferencia. Permanecen sus verificaciones/decisiones de seguridad/runtime: Atlas y egreso, TLS interno, revisión IAM integral, Redis privado, secretos/authority y evidencias/aceptación aplicables; el cierre I-A no prueba esas capacidades ni readiness de API.

**T-INFRA-026 = PENDIENTE / NO EJECUTADA**, mantiene dependencias posteriores y revisión final propias. B–I conservan criterios y pendientes; 002/004/AUTH-027 integral intactos. Catálogo: **3 tareas completas/aceptadas, 8 parciales y19 pendientes/no ejecutadas; 1 gate satisfecho y8 pendientes**. No nueva autorización técnica más allá de esta entrega y su commit local.

### 37.4. Validaciones y checkpoint autorizado

Resultados finales de los cuatro comandos autorizados, antes de stage/commit; su éxito no se extrapola a AWS:

| Comando | Inicio–fin UTC, 2026-11-13 | Exit |
| --- | --- | --- |
| terraform fmt -check -recursive infra | 06:57:55.998Z–06:57:56.214Z | **0** |
| terraform -chdir=infra validate | 06:57:56.219Z–06:58:07.900Z | **0**, configuración válida |
| bash -n infra/bootstrap/compute-host.sh | 06:58:07.902Z–06:58:20.008Z | **0** |
| git diff --check, tras actualizar estados | 06:58:20.010Z–06:58:20.226Z | **0**, solo avisos LF/CRLF |

**Revisión efectuada** de **git diff -- specs/003-infrastructure/task.md** y **git diff -- memoria.md**, incluidos los cambios documentales acumulados: únicamente documentación de evidencia, aceptación y estados, sin código/HCL/policy/config/state/plan/secret como archivos del commit. Manifiesto de44entradas de contenido protegido intacto e historial task§36/memoria§95 preservado. Stage únicamente esos dos documentos y exigir cached diff-check exit0 y stat exacto de dos archivos.

El único commit autorizado se realizará tras esas comprobaciones. **SHA/resultado y post-check final se informarán en salida de sesión/evidencia privada**, no mediante auto-referencia imposible en el propio commit, amend o segundo commit. Sin push. Evidencia privada **%LOCALAPPDATA%\Temp\opencode\gate-ia-accepted-checkpoint-20261113T065517\**, precheck/manifiesto44entradas/validaciones/diffs/revisión/index/commit/postcheck, sin state ni secretos. **DETENER al terminar el único commit y sus comprobaciones**, sin iniciar GATE-I-B.

## 38. GATE-I-B / T-INFRA-005 — Compute acreditado y aceptación de excepción SSM/matriz actual

### 38.1. Autorización, baseline y procedencia

**2026-11-13 UTC — DOCUMENTATION / ACCEPTANCE ONLY.** El responsable acepta expresamente en esta conversación la **excepción temporal SSM** y la **matriz actual de responsabilidades** descritas abajo. Autoriza editar únicamente este archivo y [memoria §97](../../memoria.md), ejecutar `terraform fmt -check -recursive infra`, `terraform -chdir=infra validate` y `git diff --check`, y dejar ambos documentos **sin stage, sin commit y sin push**. No autoriza nuevas llamadas AWS/IAM/SSM/ECR, plan, apply, cambio de red, reconciliación de refresh, infraestructura o aplicación.

Precheck documental **2026-11-13T14:45:22.728Z**: **feature/infrastructure / 224ea51c9f4cf3bdd10ff937496f4261c416a8eb**, working tree **clean / 0 staged**; hashes operativos coinciden con §38.3. Deadline original aún **no alcanzado** en ese precheck. Fuentes de los resultados técnicos de §§38.2–38.3: verificaciones anteriores de esta conversación y baseline expresamente ratificada por el responsable, **no nuevas ejecuciones cloud en este bloque**. La aceptación de §37 se conserva; no se reabre GATE-I-A ni se atribuyen al agente cambios IAM humanos.

### 38.2. IAM agregado y remediación Compute ya acreditados

- **Inventario y agregación completos revisados anteriormente:** siete managed policies, tres inline del operador, cero grupos/herencia por grupos y sin permissions boundary en `enver-terraform-local`. La ampliación humana previa de Audit-ReadOnly permitió leer las tres managed antes inaccesibles y resolvió el bloqueo de inventario; no es una autorización actual para ampliar IAM.
- Defaults observados: **Compute v5**, IAM-Runtime v1, SSM-Verification v1, IAM-Audit-ReadOnly v2, Network-Apply v1, Local-ECR-Publisher v1 y Terraform-ECR v2. Revisión agregada **sin escalada IAM identificada fuera del contrato runtime ni bypass de boundary identificado en los grants auditados**; no certificación universal de ausencia de escalada.
- Runtime **enver-dev-api-runtime**: trust solo `ec2.amazonaws.com`, duración máxima3600s, sin managed attachments, inline `enver-dev-ecr-pull` y `enver-dev-ssm-session`. **ENVER-Runtime-Permissions-Boundary v1** efectiva y semánticamente coincidente con candidata local: nueve acciones SSM/ECR, denegaciones explícitas fuera de ese contrato/repositorio/región. Sin grants runtime de IAM/STS AssumeRole/DynamoDB/Secrets/config/KMS/ECR push.
- **ENVER-Terraform-Compute v5 DEFAULT**, v4 conservada para rollback; v5 activada **manualmente por el responsable**, UpdateDate **2026-11-13T07:58:14+00:00**. Comparación estructural anterior: **10 statements / 10 Sids únicos**, exactamente la candidata autorizada reconstruida desde v4, sin diferencias adicionales ni `IfExists`.
- Únicos cambios materiales: AMI limitada a **arn:aws:ec2:us-east-1::image/ami-096ef200a877f36af**; `ModifyOnlyTaggedEnverInstanceAttributes` conserva `instance/*` regional/cuenta, condiciones usuario/región/Project/Environment/ManagedBy y añade **Name=[enver-dev-api-a,enver-dev-api-b]** y **ec2:Attribute/InstanceInitiatedShutdownBehavior=stop**. No IDs de instancia fijados en ese statement; tags/nombres no imponen cardinalidad de dos hosts.
- El intento anterior autorizado del agente de `CreatePolicyVersion` fue **AccessDenied**: no creó ni activó versiones. Runtime SSM, boundary, SSM Verification, ECR y Network no se modificaron por esta remediación. La candidata local `infra/bootstrap/terraform-local-compute-policy.json` conserva desfase histórico: **no es sustituto autoritativo de v5**, no se sincroniza en esta entrega.
- **SimulatePrincipalPolicy: NO EJECUTABLE CON IDENTIDAD ACTUAL / AccessDenied histórico.** No se repite ni se pide ampliación de permisos; no se interpreta como denegación demostrada de EC2 ni se exige como nueva remediación para cerrar Compute. El plan no ejercita mutaciones EC2: la evaluación efectiva de la condition key durante un write no se inventa.
- **Compute remediation: ACREDITADA**, por documento efectivo y plan resource no-op de §38.3. Inventario, agregación, AMI scope, ModifyInstanceAttribute, simulation API y Compute plan **no son bloqueadores pendientes de este incremento**.

### 38.3. Plan nuevo post-v5, refresh y conservación de state

Plan ya ejecutado **2026-11-13T14:37:09.988Z–14:37:27.758Z**, un único plan nuevo con refresh normal, sin `-out`, sin plan binario, sin apply y **sin reutilizar recovery4**. Prechecks anteriores: branch/HEAD actuales, clean/0staged; fmt-check/validate **exit0**; sin tfvars automáticos ni TF_VAR/TF_CLI_ARGS preexistentes. Comando real equivalente registrado (incluida UI JSON para distinguir recursos/outputs):

```text
terraform -chdir=infra plan -json -detailed-exitcode -input=false
  -var='environment=dev'
  -var='aws_region=us-east-1'
  -var='vpc_cidr=10.20.0.0/16'
  -var='public_subnet_newbits=8'
  -var='network_design_approved=true'
  -var='aws_account_id=926925605666'
  -var='runtime_permissions_boundary_arn=arn:aws:iam::926925605666:policy/ENVER-Runtime-Permissions-Boundary'
  -var='compute_ami_id=ami-096ef200a877f36af'
  -var='compute_plan_approved=true'
```

**Detailed exit code=2**, diferencias, no error: **add=0 / change=0 / destroy=0 / replacements=0**. Única diferencia de outputs **+compute_instances**, otros nueve outputs sin cambios según reporte anterior. Clasificación **RESOURCE NO-OP / OUTPUT-ONLY DIFFERENCE**. **Terraform compatibility post-v5: ACREDITADA** bajo esos inputs y para ese plan, no garantía de mutaciones futuras.

**El refresh informó discrepancias respecto al state guardado en ocho recursos relacionados con ENI, EIP, asociaciones EIP e instancias, pero NO produjo acciones correctivas en el plan. Por tanto se acredita resource no-op; NO se afirma zero drift absoluto.** No se reconcilian esas discrepancias en este bloque ni se ejecuta apply para persistir outputs.

| Archivo | SHA256 antes = después del plan y en precheck documental |
| --- | --- |
| `infra/terraform.tfstate` | **d214e2b172312123f64be213376130cad4e8be25fce36107ebbe258ffd045fff** |
| `infra/terraform.tfstate.backup` | **23a74f1556b10ade14f509820a2a225d124201b03d52380fd39b5dc458314ea7** |

State **serial39 / 0tainted**, intacto; backup nativo intacto, histórico38/Btainted, no equivalente de recuperación39. Custodia cifrada aceptada en §37 conservada; no restore, edición manual, passphrase o contenido state en documentación. Postcheck anterior **2026-11-13T14:38:20.361Z**: A **i-0a66085b7b2411393** y B **i-00de11a3f3c1a8aa5**, ambas **running / t3.small / ami-096ef200a877f36af**, clean/0staged, sin lock residual. No se reconsultan AWS ahora.

HCL conserva **compute_ami_id default=null** y **compute_plan_approved default=false**. Solo este plan autorizado usó explícitamente AMI aprobada y true mediante `-var`, sin tfvars ni variables persistentes. Antes de cualquier replace futuro: usar **compute_ami_id=ami-096ef200a877f36af** explícito o **revisar y autorizar nueva AMI + actualizar allowlist IAM** bajo autorización independiente; no modificar HCL ahora. Ningún input autoriza apply/gasto/reemplazo o amplía plazo.

### 38.4. Excepción temporal SSM — ACEPTADA por el responsable

Fuente: **aceptación humana explícita del mensaje de esta entrega**, no inferencia de SSM funcional. Para esta etapa académica, **AWS-RunShellScript utilizado por enver-terraform-local se clasifica como ADMINISTRACIÓN REMOTA CONTROLADA, NO verificación read-only**. Únicos destinos permitidos por la excepción: **i-0a66085b7b2411393 / i-00de11a3f3c1a8aa5**; único documento **AWS-RunShellScript** (`arn:aws:ssm:us-east-1::document/AWS-RunShellScript`). Esto no ejecuta ni autoriza una intervención concreta ahora.

Controles aceptados:

1. Solo **operador autorizado ENVER**, sobre esas dos EC2 A/B.
2. Scripts **revisados y autorizados antes de cada intervención**; no autorización general de comandos.
3. Prohibido **leer secretos, extraer credenciales, copiar tokens, ampliar IAM, modificar boundary o realizar cambios no autorizados**. Prohibiciones operativas; no se presentan como barreras técnicas intrínsecas de RunShellScript.
4. Registrar **propósito, comando/script, ejecución y resultado**, sin datos sensibles.
5. Cambios de red y operaciones destructivas ECR requieren **autorización independiente**; ni este acto ni un script pueden eludirla.
6. **No autoriza terraform apply** ni habilita DynamoDB, Secrets, KMS, Parameter Store config o IAM en runtime; boundary/contrato runtime sin cambios.

Se conserva temporalmente el **SSM runtime actual de cinco acciones con Resource=***, sin introducir restricciones no probadas. La coincidencia con un ejemplo mínimo AWS no prueba que todos sus wildcards sean obligatorios; no se afirma endurecimiento nuevo ni read-only por esa selección. Los permisos del agente runtime y el SendCommand del operador son capacidades distintas; un script administrativo puede actuar en el host, no se confunde la boundary con un sandbox de comandos.

**Vigencia limitada al alcance temporal original**, como máximo **2026-11-15T01:57:56.9337244Z**; sin reiniciar ventana48h ni extender plazo. En precheck de esta documentación el plazo no había sido alcanzado. **Excepción aprobada para el alcance de evaluación originalmente definido; cualquier uso posterior requiere una nueva autorización explícita.** Mantener **USD50 agregados / guardrailUSD10**, sin nuevo gasto, parada/limpieza automática ni permiso de continuidad permanente.

### 38.5. Matriz ACTUAL — ACEPTADA por el responsable

Aceptación humana expresa de responsabilidades y riesgos actuales; no creación de identidades, policies, roles OIDC o contratos futuros.

| Función | Identidad / policy | Responsabilidad y recurso | Riesgo / control / estado |
| --- | --- | --- | --- |
| Terraform provisioning | **enver-terraform-local** | Provisión Terraform autorizada del baseline | Comparte otras funciones operativas; cambios por fase y autorización explícita. Esta aceptación no autoriza provisión nueva |
| ECR publishing | **enver-terraform-local / ENVER-Local-ECR-Publisher** | Publicar artefactos en **enver-dev-api** | Publisher limitado al repository aprobado; no aislamiento efectivo frente a otros grants agregados |
| ECR provisioning / lifecycle | **enver-terraform-local / ENVER-Terraform-ECR** | Repository/lifecycle/rollback autorizado de **enver-dev-api** | Capacidad destructiva sobre repository aprobado; operaciones destructivas con autorización independiente; rollback no probado por poder modificar lifecycle |
| SSM administration | **enver-terraform-local / SSM Verification** | **ADMINISTRACIÓN REMOTA CONTROLADA** A/B mediante documento aprobado | Excepción y controles §38.4; NO read-only, scripts autorizados individualmente |
| Runtime EC2 | **enver-dev-api-runtime / ENVER-Runtime-Permissions-Boundary** | Runtime application host; SSM reducido/ECR pull en contrato auditado | Separación **TÉCNICA / ACREDITADA** respecto a identidad operadora; no prueba de API ejecutándose ni runtime ready |
| Secrets/config | **Identidad operativa NO acreditada todavía** | Entrega/configuración/rotación | **PENDIENTE para fases posteriores**; no secretos concedidos ni runtime ready |
| Authority / DynamoDB | **Identidad/provisión PENDIENTE** | Authority/bootstrap/recuperación futuros | **Runtime DynamoDB DENEGADO actualmente**; no grant/tabla/bootstrap nuevos |
| CI/CD | **Identidad NO acreditada todavía** | Deployment futuro | **OIDC PENDIENTE / contrato004 PENDIENTE**; no workflows/trust/deployment autorizados |

Auditoría ECR anterior: publisher push/query-only no equivale a permisos agregados del usuario; Terraform-ECR permite borrar repository/modificar lifecycle. `IMMUTABLE` sin exclusiones, scanOnPush y AES256 previamente leídos **no protegen retención frente a delete/recreate ni acreditan findings limpios**. Network y ECR sin grants IAM/STS identificados en los documentos revisados; sus mutaciones siguen fuera de esta autorización.

**La separación técnica completa de provisioner / publisher / SSM administrator NO está implementada.** Se acepta una **EXCEPCIÓN OPERATIVA TEMPORAL** para este incremento académico con la misma vigencia/controles de §38.4. La concentración de autoridad operativa no se presenta como escalada IAM demostrada ni como separación técnica completa. **NO satisface automáticamente requisitos futuros de CI/CD, Secrets, Authority o INFRA-024**; spec/plan permanecen intactos.

### 38.6. Reevaluación estricta T-INFRA-005 y gates

Contra el catálogo de §4, T005 exige revisión de mínimo privilegio/separación de roles y runtime sin creación/borrado de tablas ni administración global. Para el **alcance actual** están acreditados inventario/agregación, boundary efectiva/auditoría de escalada, AMI/ModifyInstanceAttribute y compatibilidad post-v5; clasificación SSM, excepción y matriz están **aceptadas por este acto humano**. No queda un bloqueo técnico adicional IAM/Compute identificado en ese alcance bajo la excepción aceptada. No se reabre inventario, simulation API o plan por falta de permisos de simulación.

**T-INFRA-005: TÉCNICAMENTE ACREDITADO / ACEPTACIÓN EXPLÍCITA PENDIENTE, en el alcance actual con excepción temporal.** **NO COMPLETA / ACEPTADA.** El criterio de aceptación de la tarea no establece que aceptar dos componentes de evidencia constituya automáticamente aceptación final de **E(T-INFRA-005)**: falta acto explícito separado de aceptación final de la tarea/evidencia y su alcance excepcional. Se conserva la regla §2 de dependencias aceptadas: **T003/T004, DP-I-08/09 y contrato preliminar004 no se declaran resueltos por esta matriz**. No se certifica el objetivo integral futuro de separación/Secrets/Authority/CI-CD ni se reforma INFRA-024; no se exige implementarlos en esta entrega documental para volver a acreditar Compute.

**GATE-I-A SATISFECHO**, T001/T002/T025 **COMPLETA / ACEPTADA** sin cambios. **GATE-I-B PENDIENTE / NO SATISFECHO**: faltan evidencia/aceptación aplicables de T006 y networking/security, Atlas/egreso/TLS interno, Redis privado, Secrets/config, Authority, E019–020/E027–028 y demás dependencias reales de §3. No cierre por aceptar la excepción, ni ejecución integral E027/E028 inferida de validaciones puntuales.

**T026 PENDIENTE / NO EJECUTADA**, B–I pendientes. Catálogo: **3 completas/aceptadas + 1 técnicamente acreditada en alcance actual con aceptación final pendiente (005) + 7 parciales + 19 pendientes/no ejecutadas**; gates **1 satisfecho + 8 pendientes**. Sin nuevo incremento de autenticación/infra/004, API readiness, digest A/B/capacidad/recuperación formal o aceptación de GATE-I-C/002/AUTH-027 integral.

### 38.7. Validaciones locales y detención documental

Resultados efectivamente obtenidos en esta entrega, **2026-11-13 UTC**:

| Comando | Inicio–fin UTC | Exit / resultado |
| --- | --- | --- |
| `terraform fmt -check -recursive infra` | **14:51:49.526Z–14:51:49.756Z** | **0**, sin modificaciones |
| `terraform -chdir=infra validate` | **14:51:49.757Z–14:52:00.251Z** | **0**, Success! The configuration is valid. |
| `git diff --check` | **14:52:00.251Z–14:52:00.398Z** | **0**, solo avisos LF/CRLF de ambos Markdown |

Comprobación **14:52:01.147Z**: branch/HEAD baseline intactos, solo **memoria.md y specs/003-infrastructure/task.md** modificados, **0 staged**; hashes state/nativo coinciden exactamente con §38.3. Comparación directa con HEAD confirma historial previo de memoria intacto y secciones6–37 de task sin cambios; solo campos vigentes y actualización puntual del catálogo antes de nueva§38. Se verifica nuevamente diff-check después de incorporar estos resultados; salida final en respuesta de sesión.

Ningún comando de esta entrega revalida cloud, ejecuta otro plan o satisface T026. **Sin AWS/IAM/SendCommand, cambio de red/ECR destructivo, apply, stage, commit o push. DETENER al entregar evidencia documental; ninguna intervención, reconciliación o ejecución posterior autorizada implícitamente.**

## 39. GATE-I-B — Aceptación formal E005 / T-INFRA-005 y commit local documental

### 39.1. Acto explícito, fecha y alcance de aceptación

**2026-11-13 UTC — ACCEPTANCE / DOCUMENTATION / LOCAL GIT.** Fuente: mensaje del **responsable del proyecto** titulado **GATE-I-B — CIERRE FORMAL T-INFRA-005 / E005** en esta conversación. El responsable **acepta formalmente la evidencia técnica acumulada E(T-INFRA-005) / E005 y T-INFRA-005 para el alcance actual documentado**, incluidas las excepciones temporales. Este es el acto separado de aceptación final que faltaba en §38.6: **E005 ACEPTADA / T-INFRA-005 COMPLETA / ACEPTADA** con la nomenclatura del catálogo. No firma criptográfica, ticket o aprobación externa inventados, ni cierre deducido solo de pruebas.

Precheck real **2026-11-13T14:55:50.467Z**: **feature/infrastructure / 224ea51c9f4cf3bdd10ff937496f4261c416a8eb**, solo **memoria.md y specs/003-infrastructure/task.md** ya modificados por la entrega anterior, **0 staged**, sin archivos untracked inesperados. Se conserva e incorpora esa documentación acumulada; task§38/memoria§97 mantienen su estado histórico previo a este acto. Solo estos dos Markdown son editables y elegibles para el único commit autorizado. No se reejecuta auditoría cloud ni plan.

La aceptación incluye expresamente:

- **Auditoría IAM integral del alcance actual completada y agregación IAM completa**, boundary runtime efectiva y **ausencia de ruta de escalación IAM identificada** en la revisión documentada; no garantía universal de ausencia de vulnerabilidades.
- **Remediación Compute v5**, AMI exacta aprobada **ami-096ef200a877f36af**, `ModifyInstanceAttribute` restringido y compatibilidad Terraform post-v5 **ACREDITADAS** según §§38.2–38.3. **ENVER-Terraform-Compute v5 DEFAULT / v4 conservada para rollback**, sin nueva modificación o lectura IAM aquí.
- Plan nuevo anterior: **detailed exit2 / 0 add / 0 change / 0 destroy / 0 replacements**, único cambio de outputs **+compute_instances**: **RESOURCE NO-OP / OUTPUT-ONLY DIFFERENCE**; no apply. **Refresh reportó discrepancias en ocho recursos ENI/EIP/asociaciones EIP/instancias, sin producir acciones correctivas en el plan**. No se afirma **zero drift absoluto**, no se reconcilia ni se reabre Compute sin evidencia material nueva.
- **Excepción temporal SSM aceptada**, clasificación **ADMINISTRACIÓN REMOTA CONTROLADA**, matriz de responsabilidades aceptada y **excepción temporal de separación aceptada**; controles y restricciones de §§38.4–38.5 permanecen íntegros.

### 39.2. Excepciones y límites que sobreviven al cierre de E005

**AWS-RunShellScript por enver-terraform-local NO es verificación read-only.** Solo destinos **i-0a66085b7b2411393** y **i-00de11a3f3c1a8aa5**, documento **AWS-RunShellScript**; scripts revisados/autorizados antes de cada intervención, operador autorizado ENVER, registro propósito/script/ejecución/resultado sin sensibles. Prohibido leer secretos, extraer credenciales/copiar tokens, ampliar IAM, modificar boundary o hacer cambios no autorizados. Red y ECR destructivo requieren autorización independiente. Este cierre **no autoriza SendCommand general ni una intervención concreta**.

Matriz conservada: **provisioning / ECR publisher / SSM admin = enver-terraform-local**; **runtime = enver-dev-api-runtime**, boundary **ENVER-Runtime-Permissions-Boundary**. Separación técnica del runtime acreditada; **separación técnica completa provisioner/publisher/SSM = NO implementada**, **excepción operativa temporal = ACEPTADA**, no separación técnica completa por aceptación documental.

Vigencia máxima original **2026-11-15T01:57:56.9337244Z**, no alcanzada en el precheck de este acto, **sin reiniciar ventana48h ni extender plazo**. Excepción aprobada para el alcance de evaluación originalmente definido; cualquier uso posterior requiere una nueva autorización explícita. **USD50 agregados / guardrailUSD10** sin gasto adicional ni automatismos de parada/limpieza. HCL mantiene `compute_ami_id=null` y `compute_plan_approved=false` como defaults; reemplazos futuros requieren input explícito de AMI aprobada o nueva AMI revisada/autorizada+allowlist IAM, con autorización técnica específica.

Esta aceptación es **integral de E005 para el alcance actual documentado con excepciones**, no cumplimiento integral futuro de **INFRA-024**, Secrets/config, Authority/DynamoDB o CI/CD/OIDC. No declara runtime ready, API ejecutándose, bootstrap/entrega de credenciales listos ni dependencias T003/T004/DP-I-08/09/contrato004 satisfechas por inferencia. Se conservan los criterios de origen y pendientes futuros; no se reforma spec/plan ni se exige implementarlos retroactivamente para desconocer el cierre expresamente delimitado. No autorización general de apply, SendCommand, nuevos recursos, red o ECR destructivo.

### 39.3. Estados y siguiente foco sin ejecución automática

| Evidencia / tarea / gate | Estado tras este acto | Límite |
| --- | --- | --- |
| **E005 / E(T-INFRA-005)** | **ACEPTADA** | Evidencia acumulada y excepciones del alcance actual, no certificación universal |
| **T-INFRA-005** | **COMPLETA / ACEPTADA** | Aceptación final expresa del responsable, no cierre de servicios futuros ni separación técnica completa |
| **GATE-I-A / T001 / T002 / T025** | **SATISFECHO / COMPLETA / ACEPTADA** | Aceptación de §37 intacta |
| **GATE-I-B** | **PENDIENTE / NO SATISFECHO** | Restan T006, networking/security, Atlas/egreso/TLS interno, Redis privado, Secrets/config, Authority, E019–020/E027–028 aplicables y demás dependencias reales de §3 |
| **T-INFRA-006** | **PARCIAL / DEFINICIÓN VALIDADA, ACEPTACIÓN PENDIENTE** | **Siguiente foco**, requiere autorización posterior delimitada; no ejecutada ni aceptada por este acto |
| **T-INFRA-026** | **PENDIENTE / NO EJECUTADA** | No se inicia revisión final ni se ejecuta otro incremento |

Catálogo actualizado: **4 completas/aceptadas (001/002/005/025) + 7 parciales + 19 pendientes/no ejecutadas = 30 tareas**; **1 gate satisfecho + 8 pendientes**. Gates B–I, 002/004/AUTH-027 integral y capacidad/recuperación/digest A/B conservan sus condiciones. Secrets/config, Authority/DynamoDB, CI/CD/OIDC, INFRA-024 integral, Atlas/Redis y evidencias aplicables siguen pendientes, no bloqueos IAM/Compute reabiertos.

### 39.4. Validaciones y único commit documental autorizado

Resultados locales efectivos, **2026-11-13 UTC**, sin init/install/upgrade/plan/cloud:

| Comando | Inicio–fin UTC | Exit / resultado |
| --- | --- | --- |
| `terraform fmt -check -recursive infra` | **14:57:43.862Z–14:57:44.058Z** | **0**, sin modificaciones |
| `terraform -chdir=infra validate` | **14:57:44.058Z–14:57:55.272Z** | **0**, Success! The configuration is valid. |
| `git diff --check` | **14:57:55.272Z–14:57:55.432Z** | **0**, solo avisos LF/CRLF de ambos Markdown |

State/nativo conservan los SHA256 de §38.3 en precheck; huella previa de los otros246archivos tracked **0bce43a81746c3a3c315b3b080cd3c3caf6e72a678968056f6f779834ac6feba**, para comparación local, sin publicar contenido state ni secretos. Repetir diff-check tras incorporar estos resultados antes de stage; evidencia final de revisión y commit en salida de sesión.

**Revisión efectuada** de los diffs completos **git diff -- specs/003-infrastructure/task.md** y **git diff -- memoria.md**, incluida la documentación acumulada autorizada de §38 y memoria§97: únicamente evidencia documental, aceptación/estado y límites, no código/config/HCL/policies/state/planes como archivos del commit. Stage exclusivamente de esos dos Markdown, exigir **git diff --cached --check** y **git diff --cached --stat** correctos antes de **UN commit local** con mensaje exacto **docs(infra): accept t-infra-005 iam controls**.

SHA y postcheck real se informarán en la respuesta de sesión, no mediante auto-referencia, amend o segundo commit. Comprobar HEAD nuevo y working tree clean; **sin push, AWS/IAM, SendCommand, plan/apply/destroy/import/state push ni inicio de T006 u otro incremento. DETENER al completar ese único commit y sus comprobaciones.**

## 40. T-INFRA-006 — Decisiones de red/transporte y contratos de dependencias; egreso pendiente de aprobación

### 40.1. Alcance, fuentes y auditoría anterior

**2026-11-13 UTC — DESIGN / DOCUMENTATION ONLY**, solicitud del responsable **GATE-I-B — T-INFRA-006 — DECISIONES DE RED, EGRESO Y TRANSPORTE**. Únicas ediciones autorizadas: este archivo y [memoria §99](../../memoria.md); únicas validaciones Terraform: fmt-check y validate, más git diff --check. **No AWS/IAM/SG/rutas/SendCommand, plan/apply, provisión Atlas/Redis/ALB, stage/commit/push**. Se separan decisiones expresamente aprobadas de propuestas y de implementación observada.

Precheck local **2026-11-13T15:13:53.722Z**: **feature/infrastructure / a16f36e9be46cf12012fd9c7873ea99049aad33b**, **clean / 0 staged**, hashes state/nativo idénticos a §38.3. Plazo original aún no alcanzado. **GATE-I-A SATISFECHO / E005 ACEPTADA / T005 COMPLETA / ACEPTADA** conservados; aceptación humana de §39 no se reabre. No nueva auditoría cloud en este bloque.

Fuente técnica: auditoría read-only anterior de esta conversación, **2026-11-13T15:03–15:07Z**, ratificada por la baseline de esta solicitud. VPC **vpc-060f67e600affd569 / 10.20.0.0/16**, DNS support/hostnames true; subnets A **subnet-0120d869dd659e2a1 / 10.20.0.0/24 / us-east-1a** y B **subnet-01934348f5d6985aa / 10.20.1.0/24 / us-east-1b**, ambas map_public_ip_on_launch=false. Tabla común **rtb-0a80f643c062696f9**: ruta local /16 y **0.0.0.0/0 → igw-06e8461bf7317dc7d**, sin ruta NAT. NACL default **acl-00570ca9a70df2b01** permite todo IPv4 en ambas direcciones mediante regla100, denyfinal32767; no añade segmentación.

EC2 A **i-0a66085b7b2411393 / 10.20.0.63 / 18.210.153.223 / eni-01ecdecf22b187764**, B **i-00de11a3f3c1a8aa5 / 10.20.1.99 / 44.195.4.217 / eni-049a982368fb2bca8**, running/t3.small/AMIaprobada en esa auditoría. Ambas ENI tienen **solo sg-070d06cfebe866427**. Backend ingress **TCP8080 exclusivamente desde sg-0dacf0df5da30b4f7**, sin CIDR público/VPC, self-reference u otro ingreso; egress **todos protocolos/puertos a 0.0.0.0/0**. SG ALB ingress **TCP443 desde 0.0.0.0/0**, egress **TCP8080 a SG backend**. Sin ingreso público22/80/443/8080 o Docker2375/2376 a EC2; **bypass directo por EIP NO permitido por esas reglas**. Ruta/EIP no bastan para declarar servicio públicamente accesible.

No prueba activa de sockets/servicio/firewall del host. No ALB operativo acreditado. Las lecturas `DescribeNatGateways`, `DescribeVpcEndpoints`, `DescribeDhcpOptions` y `DescribeLoadBalancers` fueron denegadas en la auditoría: inventarios **NOT VERIFIED**, no inexistencia probada ni bloqueo IAM nuevo. NAT no es utilizado por las rutas actuales; las EC2 salen directamente por EIP/IGW. Atributos de red comparados con **infra/networking.tf**, **security.tf** y **compute.tf** sin divergencia material identificada, no certificación global de cero drift. Plan Compute anterior conserva recursos0/0/0/0replacements y +outputcompute_instances; sus ocho discrepancias de refresh sin acciones correctivas no se reinterpretan como drift de red nuevo.

### 40.2. Decisión aprobada de transporte y riesgo residual aceptado

| Tramo / decisión | Estado documental | Implementación y límites |
| --- | --- | --- |
| **Cliente → futuro ALB: HTTPS TCP443** | **DISEÑO APROBADO / TLS EXTERNO OBLIGATORIO** | ALB/certificado/dominio/listeners pendientes T011–T014 y gates posteriores; no servicio HTTPS operativo acreditado |
| **Futuro ALB → EC2: HTTP TCP8080, NO HTTPS interno** | **DECISIÓN APROBADA por el responsable para esta etapa; TLS interno NO requerido para este incremento** | No target group/listener/contenedor configurados ahora; aprobación documental, no implementación del enlace |

Fuente de aprobación: instrucción explícita del responsable en esta entrega. Justificación: backend8080 sin exposición pública, ingress exclusivamente SG ALB, denegación Internet→8080, ausencia de ingress CIDR VPC/self, sin listener público directo autorizado por la red y simplificación de operación académica con terminación TLS en futuro ALB. **No se afirma haber inspeccionado listeners del host**.

**Riesgo residual ACEPTADO por el responsable:** tráfico ALB→EC2 **no cifrado a nivel HTTP**, con posible material sensible de solicitudes futuras dentro de ese tramo. **Controles compensatorios:** SG-to-SG, VPC dedicada, backend sin ingress público y control de asociaciones futuras del SG ALB. Los SG restringen acceso, **no cifran payloads**; cualquier ENI asociada al SG ALB podría satisfacer la referencia, por lo que no equivale a identidad intrínseca del servicio ALB. No se promete confidencialidad de aplicación en ese tramo ni aceptación para cualquier etapa futura.

Resuelve **TLS interno/puerto de DP-I-05 en el alcance de T006**; no cierra íntegramente DP-I-05 (forwarding confiable, health/timeouts/draining pendientes T011–T014/T023–T024). No se aceptan forwarded headers arbitrarios ni se modifica aplicación. **TLS hacia Atlas y Redis sigue obligatorio**, no afectado por la decisión HTTP interna. Spec/plan y comentarios HCL anteriores se conservan como antecedentes; esta decisión posterior no reforma requisitos ni configura protocolos por inferencia de una regla TCP.

### 40.3. Egreso observado, contrato objetivo propuesto y excepción NO aceptada

**Actual, acreditado en auditoría:** backend **0.0.0.0/0 / todos los protocolos / todos los puertos**. **PROVISIONAL / EXCESIVO respecto a mínimo privilegio**. No se declara mínimo privilegio ni se modifica. La ausencia de ingress público no limita exfiltración o uso arbitrario de ese egress.

**Contrato objetivo DEFINIDO COMO PROPUESTA:** salida exclusivamente para servicios/destinos requeridos por runtime/bootstrap, con revisión y aprobación antes de implementación:

| Categoría | Destino / transporte objetivo | Condición pendiente |
| --- | --- | --- |
| **A. DNS** | Resolución mediante resolver aprobado; UDP/TCP53 cuando corresponda | DHCP/resolver efectivo y DNS de dependencias por verificar; no inventar resolver/IP ni prometer filtrado DNS por SG que el servicio no soporte |
| **B. HTTPS TCP443** | ECR API, ECR registry, S3 requerido por capas, SSM, ssmmessages y repositorios/bootstrap aprobados | Identificar endpoints reales y necesidades efectivas. Otros endpoints AWS solo si se acreditan; ec2messages/STS no se añaden automáticamente |
| **C. Atlas** | Endpoints y puerto reales del cluster aprobado, TLS y validación de certificado | Aplicar contrato §40.4; no puerto fijo supuesto ni allowlist abierta |
| **D. Redis privado** | Endpoint/puerto reales por red privada dentro de AWS/VPC | Aplicar contrato §40.5; backend→Redis no debe usar Internet |

Un listado de FQDN no se convierte por sí mismo en restricción SG: el mecanismo de control debe revisarse según capacidades reales, destinos dinámicos y DNS. No se seleccionan ahora NAT, endpoints, proxy, firewall o servicios adicionales. Cambios de red futuros requieren autorización independiente, validación y revisión de riesgo de romper ECR/SSM/bootstrap.

**Excepción temporal de egreso: PROPUESTA / NO ACEPTADA.** El mensaje pide **proponer** mantener temporalmente el permiso amplio y exige que la excepción esté explícitamente aprobada; **no contiene un acto inequívoco de aceptación de esa excepción**. No se convierte una propuesta en aprobación ni se extienden las excepciones SSM/separación de E005 al egress.

Términos de la propuesta para aprobación posterior:

1. Solo EC2 A/B y el alcance académico vigente; conservar transitoriamente el egress actual **sin llamarlo mínimo privilegio**.
2. Fin como máximo al deadline original **2026-11-15T01:57:56.9337244Z**, o antes al definirse los endpoints reales Atlas/Redis/ECR/SSM/bootstrap aplicables. **Revisión obligatoria antes de runtime final**; definir contrato/mecanismo de egreso y obtener autorización antes de cualquier cambio, sin remediación automática.
3. **Prohibido añadir usos arbitrarios** o destinos/servicios no autorizados; no permiso de gasto/provisión/continuidad adicional.
4. Sin reiniciar ventana48h ni extender plazo. En precheck documental aún no se había alcanzado; **cualquier uso posterior al alcance/plazo original requiere nueva autorización explícita**. Registrar esta propuesta no autoriza continuidad operativa nueva ni prueba cumplimiento de INFRA-008.
5. Mantener USD50 agregados/guardrailUSD10, controles de secretos y autorizaciones por intervención; sin SG/rutas/IAM cambiados o mecanismos automáticos de parada/limpieza.

**Pendiente concreto:** aprobación explícita del tratamiento de egress —contrato objetivo y excepción delimitada si se desea conservar el permiso amplio—. No se eleva T006 a “solo E006 pendiente” mientras esto no ocurra.

### 40.4. Atlas — contrato de destino DEFINIDO; implementación pendiente

**MongoDB Atlas NO READY**. Para T006 queda definido el **CONTRATO DE DESTINO**: EC2 A/B → **endpoint del cluster aprobado**, TLS **OBLIGATORIO**, validación de certificado **OBLIGATORIA**, DNS/SRV según cluster real y usuario de mínimo privilegio por ambiente/DB. **Puerto PENDIENTE DEL ENDPOINT REAL**; no asumir27017 como contrato definitivo.

Allowlist **CANDIDATA / NO APLICADA NI VERIFICADA**: **18.210.153.223/32** y **44.195.4.217/32**. No usar0.0.0.0/0. Cluster/allowlist/conectividad no acreditados; no existencia o ausencia externa inventadas, no credenciales/connection strings en documentos. Implementación y revisión propia **PENDIENTES T015–T016**, coordinadas con T004/T019 para salida y secretos. Definir este contrato no requiere desplegar Atlas para aceptar el diseño T006, ni fija endpoint/puerto desconocidos silenciosamente.

### 40.5. Redis — contrato de segmentación DEFINIDO; dirección privada APROBADA

**Redis NO READY**. El responsable aprueba como dirección **Redis privado dentro de AWS/VPC**, preferencia documentada **ElastiCache basado en nodos** o **servicio equivalente privado expresamente aprobado**. Esto **no selecciona automáticamente un motor/servicio equivalente**, ni presume compatibilidad, topología, versión, capacidad, HA o costo.

**CONTRATO DE SEGMENTACIÓN DEFINIDO para T006:** sin IP pública, subnets privadas/aprobadas, endpoint privado, acceso solo desde backend/clientes expresamente aprobados, **TLS + autenticación obligatorios**, puerto real definido por servicio aprobado y **sin ruta por Internet para backend→Redis**. No suponer6379/6380 definitivos. Las dos subnets públicas actuales no se declaran subnets privadas Redis ni se alteran.

Implementación/selección/compatibilidad/capacidad/noeviction/fail-closed/recovery pendientes **T017–T018**, con secretos/authority coordinados en T019–T020. Conserva noeviction, fail-closed/503 ante pérdida detectada/confianza incierta, recuperación validada/cuarentena y riesgo residual de pérdida silenciosa de 002; sin fallback local ni aprobación por equivalencia nominal. No se exige ejecutar toda esa continuidad para definir la segmentación T006.

### 40.6. DNS y VPC endpoints

- **DNS support=true / hostnames=true** son evidencia previa de VPC. **DHCP options NOT VERIFIED** por limitación de lectura anterior; **DNS efectivo de dependencias PENDIENTE DE PRUEBAS DE RUNTIME**. No bloqueo IAM ni petición de ampliación de permisos.
- **VPC endpoints NOT VERIFIED**, no definidos en HCL. **FUTURO / OPTIMIZACIÓN DE EGRESO**, posibles para ECR/S3/SSM/ssmmessages si diseño y presupuesto posteriores lo justifican; **no requisito para cerrar T006**, no creados ni seleccionados ahora. La ruta de salida actual sigue EIP/IGW según auditoría, no tráfico medido.

### 40.7. Matriz negativa PREVISTA T028 — NO ejecutada

Fuente de resultados: **expectativas de diseño**, no pruebas activas realizadas. Aplicar las filas EC2 a A y B; conexiones nuevas, no respuestas stateful. Pruebas futuras requieren autorización específica, origen controlado, servicio/escucha conocidos y evidencia que distinga bloqueo de red de servicio ausente o error TLS/auth.

| Origen → destino | Puerto / condición | Resultado esperado | Criterio previsto |
| --- | --- | --- | --- |
| Internet → EC2 A/B | TCP22 | **DENIED** | Sin ingress SSH |
| Internet → EC2 A/B | TCP8080 | **DENIED** | Sin bypass de SG ALB |
| Internet → EC2 A/B | TCP80 | **DENIED** | Sin ingress público directo |
| Internet → EC2 A/B | TCP443 | **DENIED** | HTTPS público solo en futuro ALB |
| ALB SG → backend | HTTP TCP8080 | **ALLOWED** | SG-to-SG; servicio futuro y protocolo aprobados, no ALB operativo actual |
| EC2 A → EC2 B | TCP8080 | **DENIED** | Sin self-reference/CIDR VPC ingress |
| EC2 B → EC2 A | TCP8080 | **DENIED** | Sin self-reference/CIDR VPC ingress |
| Backend → Redis público | Cualquier puerto | **NO DESIGN** | Diseño prohibido/no provisionar destino público; no obligación de crear uno para probar |
| Backend → Redis privado aprobado | Endpoint/puerto/TLS/auth reales | **ALLOWED** futuro | Verificar ruta privada, clientes aprobados, TLS/auth y conectividad |
| Backend → Atlas no allowlisted | Endpoint/puerto reales; origen excluido | **DENIED** futuro | Negativo real de allowlist con demás condiciones válidas; no retirar salidas reales ni mutar Atlas sin permiso específico |
| Backend → Atlas allowlisted/TLS | Endpoint/puerto/usuario reales aprobados | **ALLOWED** futuro | TLS/certificado, DNS, origen permitido y permisos mínimos |

No se ejecuta T028, SendCommand o pruebas de puertos. “NO DESIGN” es prohibición arquitectónica, **no denegación técnica actual acreditada**: egress amplio no bloquea por sí mismo un Redis público arbitrario. La fila ALB usa un emisor autorizado con SG ALB, no habilita asociar ese SG a hosts sin revisión. Un timeout por sí solo no prueba filtrado ni que las cuotas no puedan eludirse a nivel de aplicación.

### 40.8. Reevaluación de T006, dependencias y gates

Contra §4, **ACREDITADOS en alcance auditado:** segmentación SG efectiva, ausencia de ingress público SSH/Docker/backend y bypass directo por controles; E005 y administración SSM previamente aceptadas, sin reabrir IAM. **DECIDIDOS/APROBADOS ahora:** HTTPS443 externo como diseño, HTTP8080 interno con riesgo residual aceptado; dirección Redis privada. **DEFINIDOS:** contrato Atlas, segmentación Redis y matriz prevista T028. Estos contratos no declaran servicios implementados; hostname/puerto finales pendientes de sus tareas.

**T-INFRA-006 conserva PARCIAL / SEGMENTACIÓN ACREDITADA, TLS INTERNO APROBADO; APROBACIÓN DE EGRESO Y E006 PENDIENTES.** No se cumple la condición de que solo reste aceptación humana E006: **egreso objetivo propuesto y excepción temporal NO aceptada**. Se mantiene dependencia original **T004** y resolución/aceptación de aspectos aplicables **DP-I-03/05/09**; T005 ya **COMPLETA / ACEPTADA**, no bloqueo nuevo. No se declara T004 completa, DP-I-05 íntegra o gestor de secretos implementado por estas decisiones; revisar dependencias/alcance al aceptar E006 sin exigir componentes futuros completos para acreditar el diseño.

No son requisitos directos de cierre de diseño T006 **ALB/Atlas/Redis operativos, ACM, DNS público, NACL custom, NAT o VPC endpoints**. Pruebas activas/aceptación cloud corresponden a E027/E028 aplicables y gates posteriores; esta entrega prepara su matriz, no las ejecuta. **E006 aceptación explícita PENDIENTE**, no COMPLETA / ACEPTADA por inferencia de decisiones parciales.

**GATE-I-B PENDIENTE / NO SATISFECHO** por E006 y demás dependencias/evidencias reales, incluidas Atlas/Redis/Secrets/Authority y E019–020/E027–028 aplicables. I-A SATISFECHO, E005 ACEPTADA y T001/T002/T005/T025 COMPLETA / ACEPTADA conservados; T026 PENDIENTE / NO EJECUTADA, B–I pendientes. Catálogo **4 completas/aceptadas + 7 parciales + 19 pendientes = 30**, gates **1 satisfecho + 8 pendientes**. Sin runtime ready, cierre AUTH-027 integral/002/004 o inicio de otro incremento.

### 40.9. Validaciones locales y detención

Resultados locales efectivos, **2026-11-13 UTC**, sin plan ni llamadas AWS:

| Comando | Inicio–fin UTC | Exit / resultado |
| --- | --- | --- |
| `terraform fmt -check -recursive infra` | **15:20:05.773Z–15:20:06.551Z** | **0**, sin modificaciones |
| `terraform -chdir=infra validate` | **15:20:06.551Z–15:20:17.640Z** | **0**, Success! The configuration is valid. |
| `git diff --check` | **15:20:17.640Z–15:20:17.808Z** | **0**, solo avisos LF/CRLF de ambos Markdown |

Comprobación **15:20:18.985Z**: branch/HEAD baseline sin cambios; solo **memoria.md y specs/003-infrastructure/task.md** modificados, **0 staged**. Otros246tracked intactos por huella de contenido; state/nativo mantienen SHA256 de §38.3. Historial previo de memoria y task§§6–39 comparado con HEAD e intacto; solo campos actuales/catálogo y nuevas secciones40/99 modificados. Diff-check se repite tras incorporar resultados, con salida final en respuesta de sesión. Estas validaciones no prueban cloud/T028 ni aceptan egreso/E006.

**DETENER tras documentación y evidencia; sin stage/commit/push, SG/rutas/IAM mutados, AWS, apply o pruebas activas.**

## 41. T-INFRA-006 — Aceptación explícita de egreso temporal; E006 integral pendiente

### 41.1. Acto humano y baseline documental

**2026-11-13 UTC — ACCEPTANCE / DOCUMENTATION ONLY.** Fuente: mensaje explícito del **responsable del proyecto**, **GATE-I-B — T-INFRA-006 — ACEPTACIÓN EXPLÍCITA DE EGRESO TEMPORAL**, en esta conversación. **Excepción temporal de egreso ACEPTADA** para mantener el permiso actual exclusivamente durante la etapa de preparación e integración, bajo los controles y vigencia de este registro. Este acto supera el estado **PROPUESTA / NO ACEPTADA** de §40, que se conserva íntegro como historial; **no acepta automáticamente E006 integral**.

Precheck real **2026-11-13T15:25:02.785Z**: **feature/infrastructure / a16f36e9be46cf12012fd9c7873ea99049aad33b**, cambios esperados únicamente **memoria.md y specs/003-infrastructure/task.md**, **0 staged**. Se preservan los cambios pendientes de §40 y [memoria §99](../../memoria.md), más todo historial anterior. Solo estos dos documentos y fmt-check/validate/diff-check autorizados; **sin AWS/IAM/SG/rutas, SendCommand, plan/apply, stage/commit/push**. Evidencia técnica anterior de §40.1 reutilizada con su fecha y límites, no nueva verificación cloud.

### 41.2. Egreso aceptado temporalmente, justificación y ocho controles

**Permiso actual:** backend **0.0.0.0/0 / todos los protocolos / todos los puertos**. Clasificación conservada: **PROVISIONAL / EXCESIVO respecto a mínimo privilegio**; **NO least privilege ni estado final aprobado**. Aceptar el riesgo temporal no restringe técnicamente destinos ni modifica reglas, y no elimina el riesgo de exfiltración o uso indebido.

**Justificación aceptada:** restringir definitivamente sin conocer los endpoints/destinos reales necesarios para **ECR API, ECR registry, S3 de capas, SSM, ssmmessages, DNS, bootstrap/repositorios autorizados, Atlas y Redis privado** puede romper bootstrap, SSM, descarga de imagen, resolución DNS y conectividad a dependencias. La excepción no sustituye los contratos de esas dependencias ni autoriza servicios arbitrarios.

Controles aceptados expresamente:

1. **No añadir nuevos usos arbitrarios** al egress abierto; solo alcance de preparación/integración autorizado de A/B.
2. **No justificar exposición de ingress** por la existencia de egress amplio.
3. **No habilitar puertos de entrada adicionales**; conservar segmentación SG auditada.
4. **Antes de runtime final, reevaluar y endurecer egress con endpoints reales**, bajo revisión y autorización de cambios; no declarar runtime final listo mientras falte ese control.
5. Cambios de **SG/rutas requieren autorización explícita**; sin remediación automática en este bloque o por vencer el plazo.
6. **Redis privado**, dentro de AWS/VPC, sin dependencia de Internet ni IP pública.
7. **Atlas con endpoint, allowlist y TLS aprobados**; salidas candidatas no equivalen a allowlist aplicada.
8. La excepción **NO autoriza terraform apply, deployment de API, SendCommand, crear Atlas/Redis o ampliar IAM**. Tampoco concede gasto adicional ni aceptación de gates.

**Vigencia:** exclusivamente el alcance original, **hasta definir destinos reales y revisar el egreso, y como máximo 2026-11-15T01:57:56.9337244Z**. Plazo aún no alcanzado en el precheck de esta aceptación. Sin reiniciar ventana48h, extensión automática o nuevo presupuesto: **USD50 agregados / guardrailUSD10** conservados. **Cualquier uso posterior requiere nueva autorización explícita.** No se autoriza alterar SG/rutas ni apagar/destruir automáticamente al alcanzar el hito; si no hay endurecimiento autorizado/validado, no inferir continuidad ni runtime final ready.

### 41.3. Contrato objetivo y contratos relacionados mantenidos

**Contrato de egreso objetivo DEFINIDO como objetivo futuro**, no reglas aplicadas:

| Destino | Alcance objetivo | Estado |
| --- | --- | --- |
| **DNS** | Solo resolver requerido/aprobado, transporte según su contrato real | DNS efectivo pendiente de pruebas runtime; no resolver/IP inventados |
| **HTTPS TCP443** | ECR API/registry, S3 requerido por capas, SSM, ssmmessages y bootstrap/repositorios aprobados | Otros endpoints AWS solo si se acreditan; no habilitar ec2messages/STS por inferencia |
| **Atlas** | Solo endpoint y puerto reales del cluster aprobado, con TLS y validación de certificado | NO READY; implementación T015–T016 pendiente |
| **Redis** | Solo endpoint privado dentro de AWS/VPC y puerto del servicio aprobado, TLS/auth | NO READY; implementación T017–T018 pendiente; sin Internet |

No confundir lista de nombres de destino con filtrado efectivo SG; mecanismo de endurecimiento, endpoints reales y autorización técnica pendientes antes del runtime final. **No aplicar reglas ahora.** DNS VPC support/hostnames true conservados; DHCP options NOT VERIFIED, DNS efectivo pendiente de pruebas, no bloqueo IAM. VPC endpoints NOT VERIFIED/no definidos en HCL, futuro/optimización, no requisito T006.

**Atlas: CONTRATO DE RED DEFINIDO / NO READY**. Allowlist **CANDIDATA / NO APLICADA NI VERIFICADA**: **18.210.153.223/32 / 44.195.4.217/32**, no0.0.0.0/0. TLS y validación de certificado obligatorios; DNS/SRV según cluster, usuario mínimo por ambiente/DB; **puerto pendiente del endpoint real**, no27017 definitivo. Sin creación de cluster, credenciales o conectividad acreditada.

**Redis: CONTRATO PRIVADO DE SEGMENTACIÓN DEFINIDO / NO READY**. Sin IP pública, subnets privadas/aprobadas, acceso solo desde backend/clientes aprobados, endpoint privado y **TLS/auth obligatorios**; **endpoint/puerto pendientes**, no6379/6380 definitivos. Preferencia de §40 conservada sin seleccionar automáticamente servicio equivalente/motor. Implementación/noeviction/fail-closed/recuperación pendientes T017–T018, sin fallback local ni alterar AUTH-027.

**TLS externo HTTPS TCP443 OBLIGATORIO / DISEÑO APROBADO**, futuro ALB/certificado/dominio pendientes T011–T014. **ALB→EC2 HTTP TCP8080 APROBADO para este incremento**, riesgo de tráfico interno no cifrado aceptado y controles SG-to-SG/VPC dedicada/backend sin ingress público/control asociaciones SG ALB conservados. No HTTPS interno requerido en este alcance, no exención TLS Atlas/Redis. Sin servicio ALB/target group o listener desplegados por aceptación documental.

### 41.4. Reevaluación exacta de T006 y separación de aceptación final

Contra §4, la evidencia E(T-INFRA-006) exige revisión SG/egreso, decisión TLS interno y matriz prevista de acceso directo/denegación para028. Para el **alcance actual con excepción temporal** quedan cubiertos:

- Segmentación efectiva y ausencia de ingress público22/80/443/8080/Docker según auditoría previa; Internet→backend por EIP **no permitido por SG** y A↔B nuevas conexiones8080 **denegadas**.
- **SG ALB→backend8080 permitido por reglas**, no servicio operativo ni prueba activa afirmados.
- TLS interno decidido y riesgo aceptado, TLS externo definido, **tratamiento de egreso explícitamente aceptado ahora** y contrato objetivo definido.
- Contrato de destino Atlas y segmentación privada Redis definidos; **matriz negativa prevista §40.7 preparada, NO ejecutada**.

**T-INFRA-006: TÉCNICAMENTE ACREDITADO / ACEPTACIÓN EXPLÍCITA PENDIENTE**, para ese alcance actual con excepción. No se identifica bloqueo técnico propio adicional en ese alcance; **no mantener aprobación de egreso como bloqueo pendiente**. El catálogo no dispone que aceptar un componente de riesgo equivalga a aceptar integralmente **E006**: **falta acto separado de aceptación explícita de E(T-INFRA-006)/tarea y de su alcance excepcional**. **NO COMPLETA / ACEPTADA todavía**.

Se conservan las dependencias originales **T004/T005 y DP-I-03/05/09** para revisión formal de alcance al aceptar E006: **T005 ya COMPLETA / ACEPTADA**; T004 y decisiones integrales no se marcan completadas por inferencia. Esto no introduce un requisito nuevo de desplegar Atlas/Redis/ALB, ACM, DNS público, NAT, NACL custom o endpoints para acreditar el diseño T006. Aspectos futuros de esas decisiones, forwarding y gestor/entrega de secretos siguen sus tareas; no reabren E005/Compute ni constituyen una remediación nueva de este incremento.

**GATE-I-B PENDIENTE / NO SATISFECHO**: aún faltan aceptación E006 y otras evidencias/criterios reales de Atlas/Redis/Secrets/config/Authority, E019–020/E027–028 aplicables y demás requisitos de §3. **GATE-I-A SATISFECHO / E005 ACEPTADA / T001-T002-T005-T025 COMPLETA / ACEPTADA** intactos. T026 PENDIENTE / NO EJECUTADA, B–I pendientes; no runtime ready ni cierre002/AUTH-027 integral/004.

Catálogo vigente: **4 completas/aceptadas + 1 técnicamente acreditada en alcance actual con aceptación integral pendiente (006) + 6 parciales + 19 pendientes = 30 tareas**; gates **1 satisfecho + 8 pendientes**. Detener; ninguna aceptación o ejecución automática posterior.

### 41.5. Validaciones locales y preservación

Resultados locales efectivos, **2026-11-13 UTC**, sin llamadas AWS ni plan:

| Comando | Inicio–fin UTC | Exit / resultado |
| --- | --- | --- |
| `terraform fmt -check -recursive infra` | **15:27:47.629Z–15:27:47.827Z** | **0**, sin modificaciones |
| `terraform -chdir=infra validate` | **15:27:47.828Z–15:27:59.034Z** | **0**, Success! The configuration is valid. |
| `git diff --check` | **15:27:59.034Z–15:27:59.212Z** | **0**, solo avisos LF/CRLF de ambos Markdown |

Comprobación **15:28:00.278Z**: branch/HEAD baseline intactos, únicamente **memoria.md y specs/003-infrastructure/task.md** modificados, **0 staged**. Otros246tracked y state/nativo intactos por hash. Huellas de historia previas a esta entrega confirman task§§6–40 y memoria hasta§99 intactos, incluidos cambios pendientes anteriores; no reescribir sus propuestas/pendientes como si ya hubieran estado aceptados. Diff-check se repite tras registrar estos resultados, con salida final en respuesta de sesión; validaciones locales no prueban runtime ni aceptan E006.

**Sin AWS, plan, apply, SendCommand, SG/rutas/IAM, stage/commit/push. DETENER.**

## 42. Cierre formal E006 / T-INFRA-006 — Aceptación del alcance actual

### 42.1. Autorización expresa y evidencia aceptada

**2026-11-13 UTC — ACCEPTANCE / DOCUMENTATION / LOCAL GIT.** Fuente: mensaje del **responsable del proyecto, GATE-I-B — CIERRE FORMAL E006 / T-INFRA-006**, en esta conversación. Acepta formalmente la evidencia técnica acumulada de **E(T-INFRA-006) / E006** para el alcance actual documentado. **E006 ACEPTADA / T-INFRA-006 COMPLETA / ACEPTADA.** Es el acto separado pendiente en §41; no aceptación inferida, ticket o firma inventados. §§40–41 y [memoria §§99–100](../../memoria.md) se conservan como historial previo.

Precheck real **2026-11-13T15:33:59.108Z**: **feature/infrastructure / a16f36e9be46cf12012fd9c7873ea99049aad33b**; únicamente **memoria.md y specs/003-infrastructure/task.md** modificados, **0 staged**, sin untracked. Otros246tracked y state/nativo intactos por hash. Se autoriza integrar los cambios documentales acumulados de ambos archivos en **exactamente un commit local**, mensaje **`docs(infra): accept t-infra-006 network controls`**; no amend/squash/push. Solo esos dos documentos, validaciones locales y Git autorizado; **sin AWS/IAM/SG/rutas/plan/apply/SendCommand**.

Aceptación expresa de la evidencia anterior de auditoría **2026-11-13 15:03–15:07 UTC** y decisiones §§40–41, sin repetir verificaciones cloud ni convertir revisión de reglas en prueba activa:

| Control / evidencia | Alcance aceptado |
| --- | --- |
| Internet→EC2 TCP22 / 80 / 443 / 8080 | **DENEGADO** para nuevas conexiones por SG; sin ingress público al backend ni administración Docker pública |
| Bypass directo por EIP | **NO permitido por SG** |
| SG ALB→backend TCP8080 | **PERMITIDO por reglas**, no ALB/listener operativo acreditado |
| A→B TCP8080 / B→A TCP8080 | **DENEGADO** para nuevas conexiones por SG |
| TLS externo | **HTTPS443 OBLIGATORIO / definido**; implementación posterior T011–T014 pendiente |
| Transporte interno | **ALB→EC2 HTTP8080 APROBADO para este incremento**, riesgo residual de tráfico interno no cifrado aceptado; SG-to-SG, VPC dedicada, backend sin ingress público y control de asociaciones futuras del SG ALB |
| Egreso / contratos | Excepción temporal aceptada, contrato objetivo de egreso, Atlas y Redis privado definidos según §41 |
| Matriz negativa | **Prevista §40.7, NO ejecutada**; T028 conserva su estado |
| AWS/HCL | Concordancia en atributos de red inspeccionados según §40.1; no certificación universal de drift, conectividad o seguridad |

### 42.2. Límites y obligaciones conservados

**Egreso actual all protocols / all ports / 0.0.0.0/0: PROVISIONAL / EXCESIVO, NO mínimo privilegio.** Excepción temporal **ACEPTADA**, no estado final aprobado. Se mantienen íntegros justificación y ocho controles §41.2: no usos arbitrarios/ingress adicional, revisión y endurecimiento antes de runtime final, SG/rutas solo con autorización, Redis privado, Atlas aprobado y prohibición de actuaciones técnicas por inferencia. Objetivo futuro: DNS requerido; HTTPS443 para ECR API/registry, S3 de capas, SSM, ssmmessages y bootstrap aprobado; Atlas endpoint/puerto reales; Redis endpoint privado. **No aplicado**.

**Deadline original máximo 2026-11-15T01:57:56.9337244Z**, aún no alcanzado en precheck, sin reiniciar ni extender; vigencia limitada además al hito de definición/revisión de destinos de §41.2. **Cualquier uso posterior al plazo requiere nueva autorización explícita.** USD50 agregados/guardrailUSD10 intactos; aceptación documental no concede gasto adicional, continuidad o limpieza automática.

**Atlas NO READY / contrato de red DEFINIDO**: allowlist candidata **18.210.153.223/32 / 44.195.4.217/32**, **NO aplicada**; TLS/validación de certificado obligatorios, puerto pendiente del endpoint real; implementación **T015–T016**. **Redis NO READY / contrato PRIVADO**, sin IP pública ni dependencia de Internet, TLS/auth obligatorios, endpoint/puerto pendientes; implementación **T017–T018**. No infraestructura, credenciales o conectividad implementadas por aceptación.

**DNS support=true / hostnames=true** conservados; **DHCP NOT VERIFIED**, resolución efectiva pendiente; **VPC endpoints NOT VERIFIED / no definidos en HCL**. No bloqueos directos de T006 ni exigencia de NAT/endpoints/NACL custom para aceptar este alcance. Las dependencias originales **T004/T005 y DP-I-03/05/09** se mantienen: T005 aceptada; T004/decisiones integrales no se declaran completas ni aceptadas por inferencia. Obligaciones de servicios reales, forwarding, secretos y controles runtime continúan en sus tareas.

Esta aceptación **NO implica** GATE-I-B satisfecho, Atlas/Redis/Secrets/config/Authority-DynamoDB listos, ALB/ACM/DNS público operativos, T028 ejecutada o egress final endurecido. **NO autoriza apply, despliegue API, SendCommand, AWS/IAM, extensión de plazo o gasto adicional.** No reapertura de E005/Compute, nuevo plan o verificaciones de custodia.

### 42.3. Estado vigente y siguiente foco sin ejecución

**GATE-I-B PENDIENTE / NO SATISFECHO**: E006 ya no es aceptación pendiente; restan Atlas, Redis, Secrets/config, Authority/DynamoDB, E019–020/E027–028 aplicables, runtime readiness y demás criterios reales de §3. **GATE-I-A SATISFECHO / E005 ACEPTADA / T005 COMPLETA / ACEPTADA** conservados. Catálogo: **5 completas/aceptadas (001/002/005/006/025) + 6 parciales + 19 pendientes = 30**; gates **1 satisfecho + 8 pendientes**. T026 **PENDIENTE / NO EJECUTADA**. Sin cierre002/AUTH-027 integral/004.

**Siguiente foco propuesto: Atlas / runtime dependencies**, sujeto a autorización separada con alcance/archivos/comandos/entorno, estimación y límites vigentes. No ejecución automática de T015–T018, pruebas T028, API o CI/CD.

### 42.4. Validaciones y checkpoint local

Validaciones efectivas **2026-11-13 UTC**, sin AWS ni plan:

| Comando | Inicio–fin UTC | Exit / resultado |
| --- | --- | --- |
| `terraform fmt -check -recursive infra` | **15:36:01.097Z–15:36:01.329Z** | **0**, sin modificaciones |
| `terraform -chdir=infra validate` | **15:36:01.330Z–15:36:11.780Z** | **0**, Success! The configuration is valid. |
| `git diff --check` | **15:36:11.780Z–15:36:11.953Z** | **0**, solo avisos LF/CRLF |

**Revisión efectuada de `git diff -- specs/003-infrastructure/task.md` y `git diff -- memoria.md` completos**, incluida documentación acumulada de §§40–41/memoria§§99–100: únicamente evidencia/decisiones/aceptación/estado/límites documentales, no código/configuración/HCL/policies/state/planes. Diff-check se repite tras registrar resultados. Solo estos dos Markdown pueden incorporarse al índice y al commit autorizado; verificar cached check/stat antes y SHA/estado limpio después. El SHA resultante y estado final se comunicarán en la respuesta de sesión, sin amend ni segundo commit para autorreferenciarlo. **Sin AWS, plan/apply, SendCommand o push; DETENER tras el único commit local.**
