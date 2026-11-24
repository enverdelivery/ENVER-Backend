# Evidencia para revisión manual de baseline de rollback — DEV

Fecha: 2026-11-24. Estado: **PREPARADO PARA REVISIÓN; NO APROBADO**.

## 1. Propósito y alcance

Expediente para una posible aprobación humana nueva de la imagen histórica como
retorno de `enverdelivery/ENVER-Backend`, exclusivamente en `development`.
Clasificación analítica: **MANUAL APPROVAL JUSTIFIABLE**; no constituye aprobación.
No es una aprobación Hardening retrospectiva ni acredita controles no ejecutados.
La existencia de la imagen, un tag, su uso anterior o health satisfactorio no bastan
por sí solos para aprobarla.

Este expediente y su borrador no son entradas de despliegue. Revisar, aprobar o
integrar este PR documental **no autoriza cambiar la baseline operativa, habilitar
flags, ejecutar SSM ni desplegar**. Se requiere una autorización humana explícita
posterior, con alcance delimitado. `approved:false` permanece intacto.

## 2. Identidad de la imagen de retorno

| Dato | Evidencia |
| --- | --- |
| Manifest digest | `sha256:41483906445df7b1ad53de32dfc8b6c416bfd53c17cf043cd5a33476c357a856` |
| Config digest | `sha256:19860cb8d7f8dc35a7183ddd4ee22e8ba5a919148c03652256ec0f4387c74b61` |
| Plataforma | `linux/amd64` |
| Usuario runtime | `1654:1654` |
| ECR | `926925605666.dkr.ecr.us-east-1.amazonaws.com/enver-dev-api` |
| Existencia ECR | YES / ACTIVE, observación de solo lectura de la revisión previa; no se consultó AWS nuevamente en esta entrega |

Se recalculó el SHA-256 de `verified-oci-manifest.json`: coincide exactamente con
el manifest digest indicado. Su descriptor config referencia el config digest.
Se leyó únicamente el miembro `blobs/sha256/19860cb8d7f8dc35a7183ddd4ee22e8ba5a919148c03652256ec0f4387c74b61`
del `image.tar` conservado: sus 5087 bytes coinciden con el hash y tamaño del descriptor.
No se construyó, cargó, ejecutó ni publicó ninguna imagen.

[oci-identity.json](evidence/rollback-baseline-2026-11-24/oci-identity.json)
preserva ambos textos UTF-8 exactos en `manifestUtf8` y `configUtf8`. Para verificar
los digests se hashean esas cadenas como bytes UTF-8, **sin volver a serializar el
objeto OCI**. El hash del JSON contenedor es distinto y se registra en §8.

## 3. Procedencia de las fuentes

- HEAD histórico: `78a5e40ab4fbe6f6c79cc20db994e40d3eb3a2fb`.
- Snapshot de fuentes: `691adba8adc33a8c4640a094f224ffab07f7d32c1f368c20e6a4622eb28f97e7`.
- El árbol de build histórico estaba **dirty**; el config OCI lo declara.
- **HEAD por sí solo no identifica la fuente exacta**. Se requiere el snapshot
  conservado de **104 archivos** para establecer procedencia.
- En esta entrega se comprobaron de nuevo los hashes de esos 104 archivos locales;
  todos coinciden con el manifiesto histórico. No se recompiló la aplicación.
- Contra el `develop` inspeccionado, commit
  `d12e50e1fbaff728f3bd45fdaa6909ddbfd8f095`, las diferencias del contenido de esos
  archivos se limitan a **`.dockerignore`**, tras normalizar CRLF a LF.
  Esto no afirma igualdad de todos los archivos del repositorio ni de los binarios
  de una candidata posterior.

[source-snapshot.json](evidence/rollback-baseline-2026-11-24/source-snapshot.json)
preserva el índice original ordenado, nombres y hashes, comparación por archivo y
texto original de `.dockerignore`. El hash del snapshot es SHA-256 de los bytes
UTF-8 de `JSON.stringify(contextManifest)`, respetando orden de entradas y claves;
no es el hash del archivo contenedor ni un commit Git.

No se copia el contexto completo ni se promete reconstrucción bit a bit del build.
Los contenidos restantes pueden contrastarse con los blobs Git fijados después
de normalizar finales de línea. El contexto completo original permanece temporal:
su conservación futura no está garantizada. Esta limitación no queda resuelta por
registrar sus hashes.

## 4. Evidencia histórica de pruebas

- `proxy-scope-tests-final.trx`: **152 ejecutadas / 152 aprobadas**; 152 resultados
  `UnitTestResult`, todos `Passed`.
- Correspondencia DLL/PDB/fuentes: **PASS histórico**, según `source-audit.json`;
  95 documentos fuente y 6 generados registrados, con todas las fuentes C# selladas
  coincidentes.
- **No se repitieron esas pruebas ni la auditoría de binarios ahora**. Se leyeron
  sus resultados y se recalcularon hashes de los archivos conservados.
- El escaneo histórico de capas no es una aprobación Hardening ni prueba de que
  esta imagen satisfaga la política actual de vulnerabilidades.

Se preservan contadores, resultados de auditoría y referencias con hashes en
[historical-checks.json](evidence/rollback-baseline-2026-11-24/historical-checks.json).
No se copian el TRX íntegro ni sus datos de máquina/usuario.

## 5. Evidencia histórica de despliegue y activación

Los recibos del **2026-11-19** documentan despliegue y activación de la imagen en:

| Nodo | Instancia | Evidencia histórica |
| --- | --- | --- |
| A | `i-0a66085b7b2411393` | Imagen/config MATCH; servicio activo; contenedor RUNNING; health local/privado 200; target healthy |
| B | `i-00de11a3f3c1a8aa5` | Imagen/config MATCH; servicio activo; contenedor RUNNING; health local/privado 200; target healthy |

El recibo final de activación, completado `2026-11-19T04:13:38.249Z`, registra
`registerHttps`, `loginHttps`, `logoutHttps`: **PASS**; `/health`: **200 PASS**;
y ambos targets `healthy`. El digest/config globales coinciden con §2.
Se preservan extractos allowlist de esos resultados, no solicitudes de autenticación,
credenciales, tokens ni datos personales de las pruebas.

**Es evidencia histórica, no verificación actual de instancias.** Los recibos locales
no son atestaciones firmadas: sus hashes fijan los bytes revisados, pero no prueban
por sí solos autenticidad remota ni estado presente.

## 6. Limitaciones y riesgos visibles

| Aspecto | Estado y límite |
| --- | --- |
| Revocación después de logout | **NOT EXECUTED** en el recibo histórico; logout HTTP satisfactorio no prueba revocación distribuida |
| TLS extremo a extremo | **false** |
| CloudFront → ALB | HTTP-only aceptado entonces para DEV/académico; no aceptación para producción ni nueva aceptación implícita |
| Digest actual A/B | **NOT VERIFIED** |
| Journals actuales A/B | **NOT VERIFIED** |
| Hardening histórico para este digest | **NO**, no se encontró aprobación |
| Fuentes históricas | Árbol dirty; requiere snapshot, no solo HEAD |
| Pruebas históricas | 152/152, no repetidas ahora; no sustituyen una evaluación de seguridad actual |
| Retención | Temporales locales pueden desaparecer; no se conserva aquí la imagen, contexto completo, TRX ni recibos íntegros |

El aprobador debe revisar expresamente estos límites. No se convierten controles
`NOT EXECUTED` en `PASS`; tampoco se acredita Mutation Testing, DAST, fuzzing o
cumplimiento integral de seguridad por aprobar una baseline manual.

## 7. Historial GitHub y registro versionado

Inventario de solo lectura capturado `2026-11-24T09:53:09.801Z`:

- 292 artefactos inventariados; 17 artefactos disponibles de aprobación/informes
  CI/Hardening revisados. Sus JSON no contienen el digest de rollback.
- **No se encontró approved-candidate para el digest histórico.**
- **No se encontró aprobación Hardening para el digest histórico.**
- Siete runs Hardening; el único satisfactorio es **45356792502**, intento 1.
  Su aprobación corresponde a la candidata nueva
  `sha256:78ef88839df7b0dadfb6f3f6170b3890a07e6819e4c26e658a94959345262eee`,
  nunca a la imagen de retorno.
- Cero runs del workflow `deploy-dev.yml` en el inventario.
- `memoria.md` del commit
  [`bab1f17728a4212471f4964d994fe42927bb6170`](https://github.com/enverdelivery/ENVER-Backend/blob/bab1f17728a4212471f4964d994fe42927bb6170/memoria.md)
  contiene digest, snapshot y referencia al recibo histórico de activación.

[github-history.json](evidence/rollback-baseline-2026-11-24/github-history.json)
preserva los metadatos de runs y artefactos, nombres, IDs, hashes y el contenido de
la aprobación **de la candidata nueva**, identificado como tal. La ausencia de
evidencia se limita al historial disponible inspeccionado, no demuestra que nunca
existiera ningún archivo externo. La captura API y los extractos son documentación
derivada, no atestaciones firmadas. No se fabricó evidencia ni se reejecutaron runs.

## 8. Preservación y referencias verificables

Los JSON adjuntos son documentación pasiva; ningún despliegue los consume.
Sus hashes vinculan el expediente a los bytes concretos revisables:

| Archivo bajo `evidence/rollback-baseline-2026-11-24/` | SHA-256 de bytes preservados |
| --- | --- |
| [oci-identity.json](evidence/rollback-baseline-2026-11-24/oci-identity.json) | `648bcc9a61436d7190d04883fdca5d0e1f8ba3c2149ab204f5f889b093608f46` |
| [source-snapshot.json](evidence/rollback-baseline-2026-11-24/source-snapshot.json) | `366bc550fa03e282ed60a585a362b547d8e10d5b245bc11d36a569a98f9f2dfc` |
| [historical-checks.json](evidence/rollback-baseline-2026-11-24/historical-checks.json) | `8df823a25aaba79764bbb0804c334791e3d1e6f4d2cafafbbe4f556d282910b8` |
| [github-history.json](evidence/rollback-baseline-2026-11-24/github-history.json) | `5ebfc5143857cdd58f23f3e9d55d3a46f288b6844135372edad9776a08290428` |
| [sources-index.json](evidence/rollback-baseline-2026-11-24/sources-index.json) | `ff7c3dd7d6fb95dfc3ede40df8ebbd1fbd82da9ff9e57e7884a38cdf1b066653` |

`sources-index.json` distingue blobs Git comprometidos y evidencia derivada;
`historical-checks.json.originalSources` identifica ubicaciones originales, tamaños
y SHA-256 de recibos históricos locales; `github-history.json.reviewedReports`
identifica artefactos GitHub y hashes de sus JSON descargados. El hash del ZIP
GitHub y el hash del JSON interno son identidades diferentes, registradas por separado.
No se modifica ningún original. Los documentos derivados no se presentan como copias
íntegras de recibos o como originales de GitHub.

Fuentes ausentes, explícitamente **NOT AVAILABLE FOR PRESERVATION**:
aprobación approved-candidate del rollback, aprobación Hardening del rollback y
recibo actual de digest/journals A/B. No se sustituyen por datos inventados.

### Ubicaciones históricas originales

Raíz local solicitada: `C:\Users\Alexander Vega\AppData\Local\Temp\opencode`.
El nombre de usuario solo identifica la ubicación histórica pedida; no representa
al futuro aprobador. No se preservan identidades personales de pruebas ni secretos.

```text
C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-cloudfront-image-2026-11-19T02-21-08-479Z
C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-cloudfront-disabled-deploy-2026-11-19T03-20-55-396Z
C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-cloudfront-activate-2026-11-19T04-07-19-211Z
C:\Users\Alexander Vega\AppData\Local\Temp\opencode\enver-cloudfront-proxy-review-2026-11-19T02-13-21-840Z\proxy-scope-tests-final.trx
```

Los temporales **no se presumen disponibles indefinidamente**. Los adjuntos pequeños
y sus referencias se proponen para conservación en Git mediante este PR. Binaries,
`image.tar`, capas, logs completos, claves, tokens y material de autenticación no se
copian. Para auditoría que requiera los originales completos será necesaria una
conservación controlada adicional, autorizada y saneada.

## 9. Contrato operativo y cambio futuro mínimo

Contrato inspeccionado en `d12e50e1fbaff728f3bd45fdaa6909ddbfd8f095`:
`scripts/deploy/deploy.mjs`, `scripts/deploy/candidates.mjs`,
`scripts/deploy/rollback-baseline.json`, `.github/workflows/deploy-dev.yml` y
`scripts/ci/policy.json`. Sus hashes de blobs figuran en `sources-index.json`.

Hay dos rutas: candidata aprobada coincidente en `rollbackInventory` o baseline
manual. Esta imagen no satisface la primera. La segunda exige `approved === true`,
`manifestDigest === previousDigest`, repositorio/entorno exactos, `evidenceHash`
de 64 hexadecimales minúsculos, `approvedBy` cadena y `expiresAt` futuro.
El código no exige cadena no vacía ni verifica el contenido referido por el hash;
una aprobación legítima sí requiere identidad real y revisión del expediente.
No se debilitan esos guards ni se aprovechan sus límites sintácticos.

El [borrador separado](rollback-baseline-approval-draft.json) conserva `approved:false`
y placeholders explícitos para identidad y expiración. No se referencia desde código.
Su `evidenceHash` propuesto será SHA-256 de **los bytes exactos de este Markdown**,
que a su vez fija los hashes de los adjuntos. No es el digest OCI ni el snapshot.
No se incluye aquí el hash de este mismo archivo para evitar autorreferencia.

Únicamente después de autorización humana explícita, el cambio mínimo futuro en
la baseline operativa sería mantener schema/digest/repositorio/entorno, pasar
`approved` a `true` y añadir `evidenceHash`, `approvedBy` real y `expiresAt` autorizado.
No copiar el borrador automáticamente ni conservar placeholders en la baseline.
Ese cambio **no se realiza ahora**. La vigencia manual debe decidirla el humano;
el máximo de 168 horas de las candidatas no se impone en esa condición manual.

Un uso posterior sigue condicionado a observar el mismo digest real A/B, ausencia
de recuperación pendiente, existencia ECR y demás gates. El preflight usa SSM para
snapshot; **no se ejecuta ni se autoriza aquí**, tampoco snapshot/verify/finalize.

### Comprobar el hash propuesto sin AWS

Desde la raíz del repositorio, para los bytes de trabajo:

```bash
node -e "const fs=require('node:fs'),c=require('node:crypto');console.log(c.createHash('sha256').update(fs.readFileSync('docs/deploy/rollback-baseline-evidence-2026-11-24.md')).digest('hex'))"
```

Tras el commit, verificar además los bytes Git, sin conversión por PowerShell:

```bash
node -e "const p=require('node:child_process'),c=require('node:crypto');console.log(c.createHash('sha256').update(p.execFileSync('git',['show','HEAD:docs/deploy/rollback-baseline-evidence-2026-11-24.md'])).digest('hex'))"
```

El archivo se prepara en UTF-8 sin BOM y LF. Cambiar texto, finales de línea o
adjuntos requiere revisar de nuevo los hashes y actualizar el borrador antes de
solicitar aprobación. La revisión debe fijar el commit exacto, no una rama mutable.

## 10. Checklist del aprobador humano — PENDIENTE

Todos los puntos siguen sin confirmar:

- [ ] Revisé este expediente y los adjuntos, sus hashes y el commit exacto.
- [ ] Entiendo las limitaciones históricas de §6 y la falta de verificación actual A/B.
- [ ] Apruebo este digest únicamente como retorno manual en `development`.
- [ ] Entiendo que no es aprobación Hardening retrospectiva ni aprobación de producción.
- [ ] Autorizo su uso hasta una expiración futura específica, expresada en UTC.
- [ ] Mi identidad/login GitHub real quedará registrado como `approvedBy`.

La declaración humana debe citar digest completo, entorno, commit revisado,
`evidenceHash` calculado, login GitHub y expiración elegida, y autorizar expresamente
el cambio mínimo posterior de la baseline. No se ha recibido esa declaración.

## 11. Cierre y siguiente gate

Baseline operativa sin modificar; flags sin cambios; sin AWS modificado, SendCommand,
despliegue, reinicios ni cambios de infraestructura. No se ejecutaron builds ni
pruebas históricas. **INITIALIZE = NO; historical total 1. NEVER initialize again.**

**NEXT GATE: HUMAN REVIEW AND EXPLICIT ROLLBACK BASELINE APPROVAL.**
