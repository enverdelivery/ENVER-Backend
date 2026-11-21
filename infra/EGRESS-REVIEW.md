# Revisión de egreso — FIX PIPELINE, 2026-11-19

## Finding y autorización

`AVD-AWS-0104` / `aws-vpc-no-public-egress-sgr` (CRITICAL) es un **finding válido**, no un falso positivo. `aws_vpc_security_group_egress_rule.backend_initial` en `security.tf` permitía protocolo `-1` a `0.0.0.0/0`: cualquier puerto/protocolo/destino IPv4. No era necesario para las dependencias conocidas de ENVER.

El responsable respondió explícitamente **«Aprobar excepción acotada»** a la propuesta de limitar HCL a TCP443, TCP27017 y Valkey TCP6379 al SG del cache, con excepción exacta de la regla para los dos recursos públicos, owner responsable ENVER, caducidad **2026-12-19** y revisión obligatoria antes de apply. No se ha ejecutado plan cloud/apply ni modificado AWS. No se presenta esta aprobación de fuente como una restricción ya activa en las instancias.

## Fuente corregida y excepciones precisas

| Recurso | Egreso | Justificación y alcance | Regla exceptuada |
|---|---|---|---|
| `aws_vpc_security_group_egress_rule.backend_https` | TCP443 → `0.0.0.0/0` | AWS APIs (SSM/ssmmessages, ECR, Secrets Manager, DynamoDB y capas ECR/S3) usan endpoints públicos dinámicos en la arquitectura existente. No se inventan CIDRs fijos, prefix lists regionales ni endpoints privados. No permite todos los protocolos. | Solo `AVD-AWS-0104` en este bloque |
| `aws_vpc_security_group_egress_rule.backend_atlas` | TCP27017 → `0.0.0.0/0` | MongoDB Atlas SRV/TLS; los destinos de nodos pueden cambiar y no hay CIDRs/PrivateLink/peering aprobados. No se leen secretos ni congela una resolución DNS efímera como allowlist. Atlas documenta TCP27017 para conectividad de clusters; cualquier SRV con otro puerto exige revisión, no ampliar a un rango por defecto. | Solo `AVD-AWS-0104` en este bloque |
| `aws_vpc_security_group_egress_rule.backend_cache` | TCP6379 → `aws_security_group.cache` | Puerto existente en `elasticache.tf` / `elasticache-v2.tf` y `RuntimeMapping.cs`; TLS y credenciales siguen obligatorios. | Ninguna |

**Owner de ambas excepciones:** responsable ENVER. **Vencimiento de ambas:** 2026-12-19, sin renovación automática. Trivy consume anotaciones inline por bloque con `:exp:`; no existe ignore global de regla/directorio/severidad. El resumen saneado identifica las dos excepciones declaradas y el número de excepciones que la herramienta efectivamente reporta. Trivy0.61.1 omite los resultados ignorados inline incluso con `--include-non-failures` (no se inventan dos entradas EXCEPTION); se conserva fuente/policy y se valida el alcance quitando solo esas anotaciones en una copia temporal. Un recurso nuevo similar no queda cubierto. El helper verifica también vencimiento por UTC antes del scan; no depende solo del parser de anotaciones.

El resolver AmazonProvidedDNS de la VPC se trata por AWS fuera del filtrado normal de SG; no requiere abrir UDP/TCP53 a Internet. IMDSv2 permanece link-local y no se sustituye por un endpoint arbitrario. No se añade HTTP80, SSH, Redis público, nuevos protocolos, red, servicios o infraestructura. La regla ALB→backend permanece SG→SG/8080.

## Riesgo residual y revisión obligatoria

Restringir puertos **no limita dominios de destino** ni evita exfiltración por HTTPS/27017. TLS/validación de certificados, RBAC/IAM y allowlist de origen Atlas no equivalen a un firewall de egreso. La excepción es temporal DEV/académica, no seguridad de producción ni aceptación end-to-end TLS.

Revisar antes de cualquier plan/apply autorizado, cambio de dependencias/SRV/puertos, incorporación de endpoints privados o a más tardar el vencimiento. Comprobar entonces los destinos/puertos efectivos de Atlas y bootstrap de la AMI (dnf); no se inspeccionó red viva para inferirlo. Validar que los paquetes del bootstrap estén disponibles por HTTPS antes de afectar una instancia. La aprobación aquí no autoriza ampliar reglas por suposición ni prometer bootstrap/runtime probado con el HCL nuevo.

La eliminación de `backend_initial` y creación de reglas específicas exige revisar el plan futuro y orden de actualización para no cortar SSM/runtime ni abrir una ventana innecesaria. No usar un bloque `moved` para disfrazar un cambio de política; no tocar state. Si al vencimiento no hay una solución aprobada, **el gate debe bloquear**; no renovar el ignore automáticamente.
