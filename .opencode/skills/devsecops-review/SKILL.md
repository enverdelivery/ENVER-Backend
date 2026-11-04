---
name: devsecops-review
description: Guía de revisión documental de riesgos OWASP API Security y requisitos DevSecOps de ENVER, sin escaneos, pruebas ni configuración técnica.
metadata:
  opencode/autoinvoke: false
---

# Revisión documental de seguridad y DevSecOps

## Propósito

Establecer una pauta de análisis documental de riesgos y controles futuros de seguridad para ENVER Backend con ASP.NET Core y .NET 10. No constituye auditoría activa, certificación de seguridad ni implantación de CI/CD.

## Cuándo utilizarla

Solo ante autorización explícita para revisar riesgos, requisitos de seguridad o estrategia DevSecOps por lectura. No utilizarla para ejecutar escáneres, consultar servicios del proyecto, validar credenciales ni desplegar herramientas.

## Relación con constitution.md

La [constitución canónica](../../../.specify/memory/constitution.md) establece los requisitos de seguridad desde el diseño, pruebas futuras, DevSecOps, resiliencia y autorización. El [constitution.md de la raíz](../../../constitution.md) es solo su referencia. Esta guía ayuda a revisar esos requisitos sin declarar controles implementados ni cambiar las reglas.

Las rutas textuales se refieren a la raíz del proyecto; los enlaces Markdown parten de esta carpeta.

## Procedimiento documental

Los pasos se describen para una futura revisión autorizada y no se ejecutan al crear este archivo.

1. Confirmar autorización, objetivo, alcance documental y permisos. Leer primero `.specify/memory/constitution.md`, `AGENTS.md`, `memoria.md` y `README.md`.
2. Revisar exclusivamente fuentes permitidas y, si se autorizaron, los archivos base de solución, proyecto, arranque y contenedor. No buscar secretos ni acceder a almacenes de credenciales.
3. Identificar riesgos usando OWASP API Security como referencia: autorización por objeto, propiedad y función, autenticación, consumo de recursos, flujos sensibles, solicitudes a servicios externos, configuración, inventario y consumo de API de terceros.
4. Examinar documentalmente HTTPS y su terminación en balanceador, gestión y rotación de secretos, validación de entradas, mínimo privilegio, errores y exposición de información. Para JWT, señalar validación de firma, algoritmo, emisor, audiencia y vigencia como aspectos que deberán especificarse.
5. Relacionar NoSQL, dos instancias en nube, balanceador, Redis distribuido, caché, paginación, índices, rate limiting, logging, observabilidad y versionamiento con riesgos futuros de acceso, recursos, aislamiento, estado compartido y evolución. Considerar concurrencia, idempotencia y fallos sin diseñar funcionalidades de negocio.
6. Identificar necesidades futuras de pruebas unitarias, integración y seguridad, análisis estático, dependencias, secretos, contenedores y trazabilidad de artefactos. Mantener pendientes herramientas, umbrales, entornos, permisos de entrega, aprobaciones y reversión.
7. Priorizar los riesgos en la respuesta según impacto y evidencia disponible, distinguiendo riesgo potencial, hecho observado, control requerido y decisión pendiente. Indicar que no hubo comprobaciones técnicas.

## Reglas y restricciones

- Solo lectura y respuesta documental en español; no crear reportes en archivos ni modificar documentos sin una autorización independiente.
- No ejecutar comandos, escáneres, solicitudes a endpoints, pruebas, compilaciones, instalaciones, migraciones, canalizaciones ni despliegues.
- No generar código, configuraciones de seguridad, infraestructura, workflows o funcionalidades de ENVER.
- No crear `spec.md`, `plan.md`, `task.md` ni sus equivalentes, ni modificar código, configuración o permisos de agentes.
- No invocar subagentes u otras skills ni eludir límites de acceso. Ante un conflicto de alcance, detener la acción afectada y comunicarlo.
- Si una lectura permitida revela un secreto, no reproducirlo; señalar solo ubicación y riesgo de forma segura.
- No afirmar que una dependencia es vulnerable sin evidencia suficiente, ni certificar seguridad por la presencia de middleware o del Dockerfile.
- No seleccionar proveedor cloud, motor NoSQL, librerías, versiones complementarias o herramientas DevSecOps.

## Criterios de aceptación documental

- Cada riesgo identifica fuente, impacto, grado de evidencia, requisito relacionado y recomendación documental.
- Las referencias a OWASP se relacionan con riesgos concretos o se marcan como aspectos pendientes de análisis.
- Se distinguen controles exigidos de controles observados, sin dar por acreditada su eficacia.
- No se exponen datos sensibles ni se atribuyen verificaciones técnicas inexistentes.
- La estrategia futura de CI/CD incluye necesidades de aprobación y mínimo privilegio, sin configurar servicios.
- El resultado enumera limitaciones, decisiones pendientes y ausencia de ejecución técnica.

## Relación con agentes y compatibilidad

Afinidad temática con `security-reviewer`; no implica invocarlo. Los cinco subagentes existentes deniegan cargar skills y conservan sus permisos. Solo un agente con permiso `skill` y autorización explícita podrá cargar esta guía y realizar la revisión documental solicitada.

Frontmatter basado en la [guía de skills de OpenCode V2](https://opencode.ai/v2/docs/skills), para la versión instalada previamente comprobada `2.0.23`. `metadata.opencode/autoinvoke: false` evita su anuncio al modelo; no es una denegación de carga explícita. El ID es `devsecops-review`. No se realiza prueba de carga ni ejecución.
