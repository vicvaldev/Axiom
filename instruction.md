# Axiom CLI — Instrucciones para el Agente

Eres un asistente que tiene acceso a la base de conocimiento operacional (Knowledge) y al registro de incidencias/Issues de la organización a través del CLI de Axiom.

## Cómo ejecutar comandos

```powershell
.\axiom <comando> [argumentos]
```

El CLI corre sobre .NET 10 y se conecta a SQL Server. No necesita compilación ni setup adicional.

---

## Comandos disponibles

### 1. Buscar en la base de conocimiento

```
.\axiom knowledge search <texto>
```

Busca entradas de conocimiento cuyo título, resumen o contenido contengan el texto indicado.

**Output:** Una tabla con columnas `Id`, `Title`, `System`, `Type`, `State`, `Tags`, `Updated`.

**Interpretación:** Si la tabla tiene filas, existían entradas relacionadas. Si solo muestra los encabezados (sin filas), no se encontraron resultados.

El `Id` (primeros 8 caracteres) identifica cada entrada. Se puede usar el GUID completo para ver el detalle.

---

### 2. Ver detalle de una entrada de conocimiento

```
.\axiom knowledge show <guid>
```

**Output:** Un panel con: Title, Summary, System, Type, State, Created By, Tags, Version, Created, Updated, Content.

Usa este comando cuando el agente necesite leer el contenido completo de una entrada.

**Si el GUID no existe:**
```
Knowledge entry not found.
```

---

### 3. Listar todas las entradas de conocimiento

```
.\axiom knowledge list
```

Lista todas las entradas de conocimiento. Misma tabla que `search`.

---

### 4. Ver detalle de un Issue

```
.\axiom issue show <guid>
```

**Output:** Un panel con: Summary, System, State, RITM, Incident, Created By, Created, Resolved, Problem, Analysis, Resolution.

Usa este comando cuando el usuario pregunte por un Issue específico o cuando tengas un GUID de Issue.

**Si el GUID no existe:**
```
Issue not found.
```

---

### 5. Listar Issues

```
.\axiom issue list
```

Lista todos los Issues en una tabla con columnas: `Id`, `Summary`, `System`, `State`, `RITM`, `Incident`, `Created`.

---

### 6. Crear una entrada de conocimiento

```
.\axiom knowledge create --system-id <id> --title "<título>" --content "<contenido>" --created-by <userId> --type-id <id> --state-id <id> [--summary "<resumen>"] [--tags "<tag1>,<tag2>"] [--issue-id <guid>]
```

| Opción | Requerido | Descripción |
|---|---|---|
| `--system-id` | Sí | ID del sistema asociado (long) |
| `--title` | Sí | Título de la entrada |
| `--content` | Sí | Contenido principal |
| `--created-by` | Sí | GUID del usuario creador |
| `--type-id` | Sí | ID del tipo de conocimiento (long) |
| `--state-id` | Sí | ID del estado de conocimiento (int) |
| `--summary` | No | Resumen corto (máx 200 caracteres) |
| `--tags` | No | Tags separados por coma. Ej: `iis,reinicio` |
| `--issue-id` | No | GUID del Issue asociado (opcional) |

**Output:**
```
Knowledge created: 177ed8be-6ec1-49f6-8439-8164aa2ea180
  Title: Cómo reiniciar IIS
  System ID: 1
  Type ID: 2
  State ID: 1
```

El GUID se usa para referenciar la entrada después.

---

### 7. Crear un Issue

```
.\axiom issue create --system-id <id> --summary "<resumen>" --problem "<problema>" --state-id <id> --created-by <userId> [--analysis "<análisis>"] [--resolution "<resolución>"] [--ritm-number "<número>"] [--incident-number "<número>"]
```

| Opción | Requerido | Descripción |
|---|---|---|
| `--system-id` | Sí | ID del sistema asociado (long) |
| `--summary` | Sí | Resumen del Issue |
| `--problem` | Sí | Descripción del problema |
| `--state-id` | Sí | ID del estado del Issue (int) |
| `--created-by` | Sí | GUID del usuario creador |
| `--analysis` | No | Análisis de causa raíz |
| `--resolution` | No | Pasos de resolución |
| `--ritm-number` | No | Número de RITM (ej. `RITM001234`) |
| `--incident-number` | No | Número de Incidente (ej. `INC005678`) |

**Output:**
```
Issue created: 74fc9278-5376-440d-9d72-38b68d5ff3de
  Summary: Error 500 al cargar módulo de facturación
  System ID: 1
  State ID: 1
```

---

## Flujos típicos para el agente

### "Busca información sobre X"

1. Ejecuta `.\axiom knowledge search <X>`
2. Si hay resultados, revísalos. Si alguno parece relevante, ejecuta `.\axiom knowledge show <guid>` para leerlo completo
3. Responde al usuario con lo que encontraste

### "Registra esto en la base de conocimiento"

1. Ejecuta `.\axiom knowledge create` con los datos proporcionados por el usuario
2. Confirma al usuario el GUID y los datos creados

### "Qué Issues tenemos registrados?"

1. Ejecuta `.\axiom issue list` para ver todos los Issues

### "Tenemos algún Issue sobre el sistema X?"

```
.\axiom issue list --eai <código_eai>
```

Lista Issues filtrados por el código EAI del sistema. Ej: `.\axiom issue list --eai EAI001`.

Para buscar Issues relacionados a un tema concreto, usa `.\axiom knowledge search <término>` ya que los Knowledge suelen estar vinculados a Issues.

---

## Consideraciones importantes

1. **Búsqueda sin resultados**: Si `knowledge search` devuelve una tabla vacía (solo encabezados), significa que no hay entradas que coincidan con la búsqueda
2. **GUID no encontrado**: Si `knowledge show` o `issue show` no encuentran el GUID, el mensaje es `not found` en rojo
3. **IDs truncados**: En las tablas de listado, el `Id` muestra solo los primeros 8 caracteres del GUID. Para operaciones como `show`, usa el GUID completo
4. **IDs de referencia**: Los comandos `create` usan IDs numéricos para System, Type y State. Estos deben obtenerse consultando las tablas de referencia correspondientes
5. **Errores de conexión**: Si la base de datos no está disponible, el CLI mostrará un error de SQL Server
