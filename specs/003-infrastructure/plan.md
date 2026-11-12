# Plan 003 — Infraestructura de ENVER Backend

## 1. Control documental y límites

| Campo | Valor |
| --- | --- |
| Versión / fecha | 0.1.0 / 2026-11-12 |
| Fuente | [spec.md](spec.md), propuesta aprobada y solicitud SDD — MATERIALIZAR 003-INFRASTRUCTURE |
| Estado | PLAN DOCUMENTAL MATERIALIZADO; SIN IMPLEMENTACIÓN AUTORIZADA |
| Catálogo | [task.md](task.md): T-INFRA-001 a T-INFRA-030, PENDIENTE / NO EJECUTADA |
| Gates | GATE-I-A a GATE-I-I, PENDIENTES / NO SATISFECHOS |

Este plan desarrolla cómo abordar los requisitos de 003, no ejecuta ninguna fase. Mantiene la autoridad de la [constitución](../../.specify/memory/constitution.md) y la baseline `c58ea86e3eddff4befa8f61398dd58d55c4ef985`. La aprobación documental previa no concede permisos AWS, Terraform, Docker, GitHub ni cambios de aplicación. No se modifican 001/002 o los documentos de gobierno.

La clasificación **REQUISITO / DECISIÓN PROPUESTA / DECISIÓN PENDIENTE / ESTADO OBSERVADO** tiene el significado de spec §1. Todas las PLD siguientes son **DECISIONES PROPUESTAS de la base documental aprobada**, no capacidades existentes. Las DP de spec §19 conservan su estado; ninguna se resuelve por esta tabla.

## 2. Decisiones trazables

| ID | Decisión propuesta y justificación | Requisitos | Pendientes principales |
| --- | --- | --- | --- |
| PLD-INFRA-001 | AWS y un ambiente académico inicial, presupuesto/ownership antes de cargos. Mantener .NET 10 y alcance Authentication; no Orders | INFRA-001–003, 033, 038 | DP-I-01, 13 |
| PLD-INFRA-002 | EC2 Linux + Docker en dos AZ: dos instancias demostrables, Terraform y control explícito. Operación de host asumida de forma consciente | INFRA-002, 004, 039 | DP-I-02, 15 |
| PLD-INFRA-003 | ECR y un digest inmutable por commit; mismo artefacto en ambas EC2, non-root, sin secretos. Reutilizar/evaluar Dockerfile existente, no duplicarlo | INFRA-005, 006 | DP-I-02, 14 |
| PLD-INFRA-004 | VPC propia, dos AZ, ALB público y Redis privado. Perfil económico: EC2 de salida fija y entrada solo desde SG ALB; evaluar privada/NAT/endpoints antes de fijarlo | INFRA-007–009 | DP-I-03 |
| PLD-INFRA-005 | Sin SSH público; SSM y roles temporales separados para provisión/deploy/runtime/bootstrap, IAM según llamadas reales | INFRA-008, 021, 024 | DP-I-03, 08, 09 |
| PLD-INFRA-006 | ALB y target group `instance`, sin stickiness necesaria; drenaje, checks y fail-open documentados, no reintentar POST automáticamente | INFRA-010, 011 | DP-I-05, 15 |
| PLD-INFRA-007 | ACM con dominio controlado, HTTPS 443 y 80 solo redirect. Resolver TLS interno y proxies antes de publicar | INFRA-012, 013 | DP-I-04, 05, 11 |
| PLD-INFRA-008 | Atlas externo: DB por ambiente, TLS, usuario mínimo, salidas aprobadas y contratos de índices/concerns/ACK de 002 | INFRA-014, 015 | DP-I-06 |
| PLD-INFRA-009 | Evaluar ElastiCache basado en nodos como preferencia; Redis EC2 privada dedicado como alternativa económica explícita, sin prometer HA | INFRA-016–018 | DP-I-07 |
| PLD-INFRA-010 | Preservar fail-closed, logical clock, cuotas, revocación y recuperación coordinada. Probar límites del servicio elegido, no eliminar el riesgo silencioso residual | INFRA-019, 040 | DP-I-07, 15 |
| PLD-INFRA-011 | Provisionar la authority DynamoDB conforme al adapter existente; bootstrap separado, sin reabrir por deploy. No ledger ni sustituto | INFRA-020, 021 | DP-I-08, 09 |
| PLD-INFRA-012 | Preferir Secrets Manager; evaluar Parameter Store SecureString. Entrega externa/rotación coordinada, sin secretos en Terraform, user-data o imagen | INFRA-006, 022, 023 | DP-I-09 |
| PLD-INFRA-013 | CloudWatch logs/métricas/alarmas con privacidad, retención y costos explícitos; agentes solo si se aprueban | INFRA-025, 026 | DP-I-10 |
| PLD-INFRA-014 | `/health` solo liveness; readiness/forwarding pendientes en incremento de aplicación separado. ALB no reemplaza admisión segura | INFRA-011, 013, 027, 028 | DP-I-05, 11 |
| PLD-INFRA-015 | Terraform sencillo por responsabilidades, provider AWS, inputs/outputs tipados y mínimos, naming/tags por ambiente; no fijar versiones sin aprobación | INFRA-029–031, 033 | DP-I-02, 12, 13 |
| PLD-INFRA-016 | State académico local protegido con backup cifrado y un operador; remoto S3/versionado/locking nativo compatible antes de GitHub o uso compartido | INFRA-032 | DP-I-12 |
| PLD-INFRA-017 | Contrato 003→004: CI automático en 004, deployment manual workflow_dispatch tras CI verde del mismo artefacto. Resolver ownership de digest antes de aplicar | INFRA-005, 034 | DP-I-14 |
| PLD-INFRA-018 | Rollback a imagen/config compatibles anteriores, drenaje y salud; no resetear Redis/authority/cuotas ni retirar claves aún necesarias | INFRA-023, 035, 039 | DP-I-07, 14, 15 |
| PLD-INFRA-019 | Validación/revisión IaC transversal, plan/costo antes de apply autorizado, drift/reproducción seguros y sin destroy implícito | INFRA-003, 029, 036, 038 | DP-I-01, 12 |
| PLD-INFRA-020 | Aceptación cloud y evidencia por requisito/tarea/AC/gate; conservar 002 local/test, AUTH-027 y gates pendientes sin cierre retrospectivo | INFRA-002, 037, 038, 040 | DP-I-13, 15 |

### Comparación de cómputo conservada

EC2 + Docker se recomienda por simplicidad académica, dos máquinas demostrables, control explícito y facilidad de evidenciar la rúbrica. ECS/Fargate reduce operación del host, pero exige comparar red/costos y confirmar la interpretación docente de dos tasks. ECS sobre EC2 agrega orquestación y complejidad innecesaria inicialmente. No se afirma que EC2 sea universalmente más económica: aprobar estimación total de ALB, red/salida, IPv4, almacenamiento, servicios y operación.

## 3. Flujo de trabajo y condiciones comunes

1. Resolver DP bloqueantes por escrito y delimitar la autorización de cada incremento.
2. Preparar baseline y validación inicial; ningún recurso se crea al escribir archivos Terraform.
3. Preparar configuración, contratos y runbooks. Las fases son dominios de trabajo, **no una cadena numérica obligatoria**: seguridad/secretos y readiness deben estar listos antes del deployment.
4. Autorizar separadamente la dependencia de aplicación readiness/forwarding, sin cambios productivos implícitos en 003. Reutilizar evidencia aceptada; si requiere cambios en código/tests/Docker, solicitar un incremento específico.
5. Revisar IaC completa y costo; ejecutar apply solo bajo autorización técnica posterior. Un plan aceptado no es autorización de apply.
6. Verificar cloud y registrar evidencia real; ningún gate se cierra solo por configuración o capturas locales.

En todas las fases: preservar trabajo existente, no secretos, no eliminación/recreación de datos por conveniencia, no recursos fuera del presupuesto, no CI/CD implementado en 003. La evidencia faltante no se reemplaza por un resultado esperado. La detención por riesgo, costo, DP o fallo deja la tarea/gate pendiente.

## 4. Fases, dependencias y criterios de avance

| Fase | Trabajo futuro / tareas | Dependencias | Criterio de avance / evidencia | Gates relacionados |
| --- | --- | --- | --- | --- |
| **INFRA-1 Baseline** | Cuenta/región/presupuesto; baseline Terraform/state. T-INFRA-001–002 | Autorización delimitada; DP-I-01, 12, 13 | Ownership, ambiente, costo, versiones y state aprobados; baseline validada mediante T-INFRA-025 | GATE-I-A |
| **INFRA-2 Networking** | VPC/subnets/rutas y salida estable/Atlas. T-INFRA-003–004 | INFRA-1, validación baseline de INFRA-13 | Configuración revisada en dos AZ, salidas y exposición definidas, sin apertura general Atlas | GATE-I-B |
| **INFRA-3 Security/IAM** | Roles/políticas, SG, SSM y transporte. T-INFRA-005–006 | INFRA-1/2; contratos de dependencias | Acciones/recursos mínimos y reglas de ingreso/egreso revisadas; IAM no administra datos por defecto | GATE-I-B |
| **INFRA-4 Registry** | ECR y contrato de artefacto/digest. T-INFRA-007–008 | INFRA-1/3; contrato con 004 | Registry/config definidos, imagen no secreta y ownership de digest acordado. No construir imagen ni workflow aquí | GATE-I-C, GATE-I-H |
| **INFRA-5 Compute** | Dos EC2 y política de recuperación/escalado. T-INFRA-009–010 | INFRA-2/3/4; definiciones de 10/12 antes de admisión pública | IaC expresa dos hosts en dos AZ y operación segura; existencia real se acredita en 14, no al escribir compute.tf | GATE-I-C |
| **INFRA-6 Balanceador** | ALB/targets y contrato de distribución/drenaje/fallos. T-INFRA-011–012 | INFRA-2/3/5; contrato health de 12 | Listeners/target group y escenarios unhealthy/todos-unhealthy revisados; pruebas reales en 14 | GATE-I-D |
| **INFRA-7 HTTPS** | Dominio/DNS/ACM y listeners/TLS. T-INFRA-013–014 | INFRA-2/3/6; dominio aprobado | DNS/certificado/transporte definidos; validez HTTPS solo se acredita tras provisión/verificación en 14 | GATE-I-E |
| **INFRA-8 Atlas** | Integración red/TLS/usuario/DB e índices/ACK. T-INFRA-015–016 | INFRA-2/3; gestor de secretos de 10 | Contratos/configuración compatibles con 002 revisados; inicialización/concurrencia cloud en 14 | GATE-I-F |
| **INFRA-9 Redis** | Selección/capacidad/compatibilidad y continuidad. T-INFRA-017–018 | INFRA-1/2/3; autoridad/secretos de 10 | Opción aprobada y protocolo seguro documentado, sin cambio de semántica; compatibilidad real y fallos se verifican en 14 | GATE-I-F |
| **INFRA-10 Configuración** | Secretos externos/rotación y authority. T-INFRA-019–020 | INFRA-1/3; requisitos 8/9 | Referencias/roles y bootstrap definidos, sin valores en state; ejecución de bootstrap solo en 14 autorizada | GATE-I-F |
| **INFRA-11 Observabilidad** | Logs/métricas y alarmas/retención. T-INFRA-021–022 | INFRA-3; contratos 5/6/8/9/10 | Cobertura, entrega, destinatarios y costos revisados; señales reales y alarma en 14 | GATE-I-G |
| **INFRA-12 Health** | Contrato readiness/proxy y evidencia de dependencia de aplicación. T-INFRA-023–024 | INFRA-6/7/8/9/10/11 para contratos, sin exigir todavía recursos cloud | Dependencia de aplicación autorizada aparte, implementada/probada con evidencia aceptada antes de publicación; sin ello no avanzar a deployment | GATE-I-D, GATE-I-E, GATE-I-F |
| **INFRA-13 Terraform** | Validación baseline transversal y revisión final/plan/costo. T-INFRA-025–026 | 025 tras INFRA-1; 026 tras artefactos de 2–12 | Baseline válida primero; después IaC completa válida, plan revisado y costo aprobado. Ningún apply implícito | GATE-I-A, GATE-I-H |
| **INFRA-14 Cloud** | Provisión/deploy manual autorizados y verificación pública/cross-instance/rollback. T-INFRA-027–028 | 026, dependencia 024 aceptada, artefacto CI verde e integración manual 004 aprobada mediante workflow_dispatch | Recursos reales, dos instancias, HTTPS, dependencias, telemetría y contratos verificados con evidencia saneada | GATE-I-B a GATE-I-I |
| **INFRA-15 Evidencia** | Paquete académico y reproducción/revisión final. T-INFRA-029–030 | INFRA-14; autorizaciones específicas para reproducción/fallos | Evidencia completa, accesible/protegida, trazable y con límites/costos explícitos; revisión del responsable | GATE-I-H, GATE-I-I |

**Distinción de avance:** tareas 003–024 preparan/configuran y verifican contratos antes del cloud; no certifican recursos desplegados. Las comprobaciones runtime de esas fases se acumulan en 027–030 y son necesarias para cerrar gates. T-INFRA-025 valida la baseline temprana; T-INFRA-026 vuelve a validar el conjunto completo antes de apply. No hay dependencia circular que exija cloud para poder revisar el plan previo.

## 5. Secuencia y separación de responsabilidades

Secuencia de dependencias del catálogo (cada tarea contiene su lista exacta en task.md):

```text
001 → 002 → 025 (validación baseline)
              ├─ 003 → 004 → 005 → 006
              ├─ 007 → 008 → 009 → 010
              ├─ 011 → 012; 013 → 014
              ├─ 015 → 016; 017 → 018; 019 → 020
              └─ 021 → 022; 023 → 024
003..024 + 025 → 026 → 027 → 028 → 029 → 030
```

El esquema resume ramas, no sustituye las dependencias cruzadas del catálogo. Puede avanzarse documentalmente/configurarse ramas independientes bajo permisos propios; no autoriza subagentes ni trabajo paralelo no solicitado. La admisión pública espera a dependencias, seguridad, readiness, observabilidad, CI y deployment manual aprobados.

### Propiedad de componentes

- **003:** recursos, red, IAM, referencias a secretos, state, contrato de runtime/health, inputs/outputs y aceptación de infraestructura.
- **002 / incremento de aplicación autorizado:** comportamiento Authentication, implementación readiness/forwarding si falta y pruebas necesarias. No se reabre ni cambia 002 mediante Terraform.
- **004-ci-cd, futura y no creada:** CI automático, quality gates, aprobación manual/workflow_dispatch, deploy y smoke tests. El push/merge no desplegará automáticamente.
- **Operación autorizada:** provisión de valores secretos y bootstrap inicial validado; no user-data con credenciales ni reapertura automática de authority.

## 6. Contratos técnicos de implementación futura

### 6.1. Terraform e inventario

Respetar la estructura futura de spec §15: `main.tf`, `providers.tf`, `versions.tf`, `variables.tf`, `outputs.tf`, `networking.tf`, `security.tf`, `compute.tf`, `registry.tf`, `load-balancer.tf`, `secrets.tf`, `redis.tf`, `authority.tf` y `observability.tf`, todos bajo `infra/` **solo tras autorización futura**. No usar provider Atlas adicional sin decisión. Naming/tags y ambientes se validan antes de asignar recursos.

Inputs de integración: región/ambiente, digest, capacidad mínima, refs de secretos, tabla/region authority, target group/health, dominio/certificado y política de cambio. Outputs: URL, recursos no secretos, ECR y telemetría. Ningún output expone credenciales o JWT. Publicar evidencia no implica publicar state o identificadores internos innecesarios.

### 6.2. State

Inicial: local protegido, backup cifrado y único operador. Antes de GitHub/colaboración: backend remoto protegido/versionado con locking compatible, propuesta S3 con bloqueo nativo. Versiones concretas, bootstrap/migración y recuperación son DP-I-12. No S3 ni DynamoDB de lock creados por este plan. La authority DynamoDB sigue siendo un componente distinto y obligatorio del wiring existente.

### 6.3. Artefacto y deploy manual

004 entregará commit/digest con CI verde y promoción manual `workflow_dispatch`; 003 expondrá destino, permisos y URL. Definir un único escritor del digest/configuración para evitar drift Terraform–CD. No especificar jobs, YAML, triggers automáticos de deployment o acciones concretas en estos documentos.

Deployment futuro: confirmar digest/config/secret refs, bootstrap autorizado y controles de salud; actualizar con drenaje y verificación; retener versión previa. Política de mínimo dos en aceptación y capacidad durante mantenimiento explícita; no prometer continuidad absoluta. Rollback no restaura state Redis antiguo, no vacía cuotas, no reabre authority y no elimina claves verificadoras vigentes. Ante duda de integridad, cerrar admisión y ejecutar recuperación autorizada.

### 6.4. Pruebas y evidencia

Prioridad: seguridad de red y secretos → dos targets/TLS → dependencias/authority → readiness/ALB → Authentication distribuida → fallos/rollback/reproducción. Verificar contratos normales y negativos (no-store, challenge, 429/Retry-After, 503 seguro), sin exigir al ALB que produzca el Problem Details de la API.

Una prueba de revocación debe usar superficie protegida normal ya autorizada, o recibir autorización aparte; Logout es excepción de reconfirmación y no sirve para demostrar rechazo normal. No agregar endpoint productivo para la demostración. Recolectar atribución A/B en telemetría interna saneada y preservar privacidad.

## 7. Gates y política de aceptación

Definiciones y tareas/evidencias requeridas en [task.md §3](task.md#3-gates-formales). Los nueve gates permanecen **PENDIENTES / NO SATISFECHOS**.

- I-A acredita baseline, no autorización de apply.
- I-B a I-G requieren evidencia real de seguridad, cómputo, balanceo, TLS, dependencias y observabilidad; un `.tf` o diagrama no basta.
- I-H requiere IaC revisada/reproducción/rollback y state seguro.
- I-I requiere Authentication pública distribuida y revisión del responsable, tras los anteriores.

La relación con AC-I-01 a AC-I-15 está en spec §18. Si falta la autorización separada para aplicación, dominio, CI/manual deploy, prueba de pérdida o presupuesto, se registra bloqueo y no se cierra el gate. Cerrar 003 en el futuro no cierra automáticamente GATE-G/H de 002 ni AUTH-027 integral.

## 8. Riesgos y decisiones que impiden iniciar ejecución

Aplican DP-I-01 a DP-I-15 y spec §20. Riesgos principales: cargos ALB/IPv4/NAT/Redis/Atlas/logs aun con compute detenido; dependencia única pese a dos AZ; pérdida Redis detectada o silenciosa residual; exposición por SG/forwarding; filtración de state/secrets; falsa readiness; dos escritores del digest; rollback de seguridad; mantenimiento sin capacidad suficiente.

No se fijan servicios gratuitos, tamaños, versiones, dominio, retención, RTO/RPO ni umbrales sin aprobación. Presupuesto/alarma no constituye límite duro. Antes del primer cambio técnico se presentará alcance, archivos/componentes, comandos, entorno, costo estimado, reversión y evidencia esperada. Esta entrega termina con los tres documentos, no con T-INFRA-001 iniciada.

## 9. Revisión documental de coherencia

- 001 permanece histórico y no completado integralmente; no se crean capas/microservicios ni se seleccionan paquetes complementarios.
- 002 mantiene contratos/JWT/cuotas/revocación, authority existente, riesgo residual y cierre exclusivamente LOCAL/TEST REPRESENTATIVO. Evidencia local se referencia, no se reejecuta.
- Liveness existente no se confunde con readiness pendiente ni con continuidad confiable.
- Infraestructura y CI/CD separados; futuro deployment manual tras CI verde, nunca automático por push.
- Recursos descritos como futuros, sin dominios/credenciales inventados ni gates cerrados.
- Solo spec/plan/task de 003 se crean; sin código, infra, Docker, workflows, pruebas, Terraform, AWS o Git stage/commit/push.
