# Plan 007 — Entrega y asignación rescatista

**v0.1.0 · Borrador para revisión · Desarrollador B.** Fuentes: [spec.md](spec.md), [constitución canónica](../../.specify/memory/constitution.md). No autoriza ejecución técnica.

## 1. Objetivo
Entregar el listado de asignaciones y las cinco transiciones de entrega de spec §3, sin otro agregado.

## 2. Arquitectura reutilizada
MVC, DI, identidad activa y política normal de [005](../005-users-profile/spec.md); consultas, estados y proyecciones de [006](../006-orders/spec.md). Sin Redis nuevo ni locks distribuidos.

## 3. Estructura propuesta
`DeliveryController` bajo `/api/v1/orders` y módulo de aplicación de Delivery; consumir operaciones específicas de Order y la consulta de elegibilidad, sin repositorio paralelo que pueda reemplazar Order.

## 4. Persistencia
Mismo Order e índice de asignado de 006. Cada transición registra actor e instante UTC del paso junto a estado y `updatedAt`; `accept` fija también el asignado. Sin colección Delivery.

## 5. Flujo de endpoints
- `assigned-to-me`: filtrar por `sub` asignado y paginar resúmenes, incluidos completados.
- `accept`: Publicado → Aceptado; `start-delivery`: Aceptado → En camino; `arrive`: En camino → Llegué; `request-completion`: Llegué → Esperando confirmación; `confirm-completion`: Esperando confirmación → Completado.
- Acción → autorizar → actualizar con filtro condicional → confirmar resultado o reconocer reenvío → `200` con detalle vigente de 006. Llegada manual, sin GPS.

## 6. Autorización
Cuenta activa y Bearer normal en todas las rutas. Accept exige elegibilidad de servidor y actor distinto del propietario, también se revalida elegibilidad en el reenvío. Listado y tres pasos intermedios se limitan al asignado. Confirmación solo por el propietario. Nunca usar `activeRole` como permiso.

## 7. Validación
UUID en ruta, sin cuerpo en transiciones, sin actor/estado/posición/fecha suministrados. Listado comparte límites y cursor de 006, acotado al ámbito asignado.

## 8. Concurrencia e idempotencia
**ALTO RIESGO:** filtro atómico de accept por ID, Publicado, asignado nulo y propietario distinto; los demás pasos filtran ID, estado esperado y actor (asignado o dueño). Compite con cancelación en el mismo documento. Sin reemplazo incondicional. Si no se confirma la mutación, comprobar visibilidad y marca persistida: solo el mismo actor aún autorizado recibe éxito de reenvío sin mutar, aunque el estado haya avanzado. No generar otra clave ni atribuir éxito al perdedor; si no se confirma, 503.

## 9. Errores
Base 005 y spec §8: 403 por falta de elegibilidad o autoprestación en pedido visible, 404 si no es visible o no existe, 409 por estado incompatible en pedido visible, 503 por incertidumbre no resuelta. No devolver estado privado al resolver una carrera.

## 10. Pruebas mínimas
Autorización por actor, listado propio, pasos fuera de orden, dueño que confirma y repetición sin escritura. Dos carreras Mongo críticas: accept/accept y accept/cancel. Probar accept posterior a cancelación sin mutación.

## 11. Secuencia
Al estabilizar Order con A: contratos, aplicación y tests con elegibilidad controlada → listado y CAS → endpoints y carreras con 006. Publicar temprano a 009 la garantía de asignación y `Completado` terminal.

## 12. Riesgos y decisiones abiertas
**ALTO RIESGO:** un ganador, no saltar estados, no confundir modo activo con permiso. Acordar fuente real de elegibilidad y qué ocurre con entregas en curso al retirarla (spec §6, propuesta). Esto no impide desarrollar contra una fuente de pruebas; no habilitar accepts reales sin aprobación.

## 13. Definición de terminado
Seis rutas y AC-DEL-01–03 acreditados; actas de decisión de elegibilidad, reenvío y carreras comprobadas antes de aceptar el flujo real. Sin nueva infraestructura ni cambios a Auth.
