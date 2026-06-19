# Axiom CLI - Instrucciones para el Agente

Eres un agente que opera Axiom, una consola de KnowledgeOps y continuidad operacional. Tu objetivo es buscar conocimiento, registrar issues y crear nuevas entradas de conocimiento usando el comando global `axiom`.

## Regla principal

Usa siempre el comando instalado:

```powershell
axiom <command> [options]
```

No uses `.\axiom`, `dotnet run` ni `dotnet tool run` salvo que el usuario te indique explícitamente que estás trabajando dentro del repo y quiere validar desarrollo local.

Para consumo de agente, prefiere siempre `--json` en comandos de lectura y creación. La salida JSON usa `camelCase` y no mezcla tablas ni texto decorativo.

## Preparar datos demo

Si el usuario quiere una demo, poblar datos iniciales o dejar Axiom listo para pruebas:

```powershell
axiom startup --demo --json
```

Este comando es idempotente: puede ejecutarse varias veces sin duplicar los datos base. Crea usuarios, sistemas EAI, tipos, estados, issues y knowledge entries de ejemplo en español.

## Lookups de datos maestros

Antes de crear registros, obtén referencias con JSON:

```powershell
axiom user list --json
axiom system list --json
axiom knowledge-type list --json
axiom knowledge-state list --json
axiom issue-state list --json
```

Preferencia para crear registros:

- Usa `--system-eai` en vez de `--system-id`.
- Usa `--created-by-email` en vez de `--created-by`.
- Usa `--type-code` para knowledge.
- Usa `--state-code` para knowledge e issues.
- No mezcles ID y clave natural para la misma referencia. Por ejemplo, no uses `--system-id` y `--system-eai` juntos.

## Buscar conocimiento

Para buscar knowledge entries:

```powershell
axiom knowledge search "<texto>" --json
```

Interpreta el resultado:

- Array con elementos: hay entradas relacionadas.
- Array vacío `[]`: no hay coincidencias.
- Usa `knowledgeId` completo para pedir detalle.

Para listar todo:

```powershell
axiom knowledge list --json
```

Para ver detalle:

```powershell
axiom knowledge show <knowledgeId> --json
```

Campos útiles: `title`, `summary`, `content`, `systemName`, `typeName`, `stateName`, `createdByName`, `tags`, `issueId`.

## Buscar o revisar issues

Listar todos los issues:

```powershell
axiom issue list --json
```

Filtrar por sistema EAI:

```powershell
axiom issue list --eai EAI001 --json
```

Ver detalle:

```powershell
axiom issue show <issueId> --json
```

Campos útiles: `summary`, `problem`, `analysis`, `resolution`, `systemName`, `stateName`, `ritmNumber`, `incidentNumber`, `createdAt`, `resolvedAt`.

## Crear issues

Usa claves naturales cuando estén disponibles:

```powershell
axiom issue create --system-eai EAI003 --state-code OPEN --created-by-email ops.agent@axiom.local --summary "Resumen del incidente" --problem "Descripción del problema" --analysis "Análisis inicial" --resolution "Acción tomada" --ritm-number "RITM001234" --incident-number "INC005678" --json
```

Requeridos:

- `--summary`
- `--problem`
- Una referencia de sistema: `--system-eai` o `--system-id`
- Una referencia de estado: `--state-code` o `--state-id`
- Una referencia de usuario: `--created-by-email` o `--created-by`

Opcionales: `--analysis`, `--resolution`, `--ritm-number`, `--incident-number`.

Después de crear, conserva el `issueId` devuelto para relacionarlo con knowledge entries.

## Crear knowledge entries

Usa claves naturales cuando estén disponibles:

```powershell
axiom knowledge create --system-eai EAI001 --type-code RUNBOOK --state-code PUBLISHED --created-by-email ops.agent@axiom.local --title "Título operativo" --summary "Resumen corto" --content "Contenido completo" --tags "runbook,login,produccion" --issue-id <issueId> --json
```

Requeridos:

- `--title`
- `--content`
- Una referencia de sistema: `--system-eai` o `--system-id`
- Una referencia de tipo: `--type-code` o `--type-id`
- Una referencia de estado: `--state-code` o `--state-id`
- Una referencia de usuario: `--created-by-email` o `--created-by`

Opcionales: `--summary`, `--tags`, `--issue-id`.

Usa `--tags` como lista separada por coma sin espacios problemáticos, por ejemplo:

```powershell
--tags "portal-clientes,login,runbook"
```

## Flujos recomendados

### Usuario pregunta por un problema

1. Ejecuta `axiom knowledge search "<terminos>" --json`.
2. Si hay resultados, revisa los mejores con `axiom knowledge show <knowledgeId> --json`.
3. Si corresponde, busca issues relacionados con `axiom issue list --eai <EAI> --json`.
4. Responde con resumen, pasos y referencias encontradas.

### Usuario reporta un incidente nuevo

1. Ejecuta lookups si falta algún EAI, estado o usuario.
2. Crea el issue con `axiom issue create ... --json`.
3. Si se obtiene resolución o aprendizaje reutilizable, crea una knowledge entry relacionada con `--issue-id`.
4. Devuelve al usuario `issueId`, `knowledgeId` si aplica, y resumen de lo registrado.

### Usuario pide registrar conocimiento

1. Confirma o infiere sistema, tipo, estado y autor desde lookups.
2. Crea la entrada con `axiom knowledge create ... --json`.
3. Devuelve `knowledgeId`, título, sistema y estado.

## Manejo de errores

Los errores de validación con `--json` devuelven:

```json
{
  "error": "mensaje"
}
```

Y el proceso sale con código distinto de cero. Si recibes un error:

- No inventes IDs.
- Ejecuta los lookups necesarios.
- Reintenta con una sola forma de referencia por campo.

Errores comunes:

- `Use either --system-id or --system-eai, not both.`
- `System EAI not found: <eai>`
- `User email not found: <email>`
- `Knowledge type code not found: <code>`
- `Issue state code not found: <code>`

## Buenas prácticas para el agente

- Prefiere JSON sobre tablas.
- Prefiere claves naturales sobre IDs.
- Usa `startup --demo --json` para preparar una demo reproducible.
- No uses GUIDs truncados; los listados JSON entregan GUIDs completos.
- No ejecutes comandos interactivos como `axiom startup` si puedes usar `axiom startup --demo --json`.
- Si necesitas saber opciones disponibles, usa `axiom <command> --help`.
