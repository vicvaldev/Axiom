# Axiom CLI — Instrucciones para el Agente

Eres un asistente que tiene acceso a la base de conocimiento operacional (Knowledge) y al registro de casos/incidentes (Cases) de la organización a través del CLI de Axiom.

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

Busca entradas de conocimiento cuyo título, descripción, contenido o tags contengan el texto indicado (búsqueda sin distinción de mayúsculas/minúsculas).

**Ejemplos de uso:**

| Pregunta del usuario | Comando a ejecutar |
|---|---|
| "Busca si tenemos documentación sobre IIS" | `.\axiom knowledge search IIS` |
| "Hay algún troubleshooting de errores 500?" | `.\axiom knowledge search "error 500"` |
| "Qué sabemos sobre reinicio de servidores?" | `.\axiom knowledge search reinicio` |
| "Busca en la base de conocimientos si tenemos errores relacionados al IIS en el EAI 11478" | `.\axiom knowledge search IIS` (luego buscas también `.\axiom knowledge search "EAI 11478"` o `.\axiom case show <guid>` si el ID es de un caso) |

**Output:** Una tabla con columnas `Id`, `Title`, `System`, `Type`, `Status`.

```
┌──────────┬───────────────────────────┬────────────┬─────────────────┬────────┐
│ Id       │ Title                     │ System     │ Type            │ Status │
├──────────┼───────────────────────────┼────────────┼─────────────────┼────────┤
│ 177ed8be │ Cómo reiniciar el         │ IIS-Server │ Troubleshooting │ Draft  │
│          │ servidor IIS              │            │                 │        │
└──────────┴───────────────────────────┴────────────┴─────────────────┴────────┘
```

**Interpretación:** Si la tabla tiene filas, existían entradas relacionadas. Si solo muestra los encabezados (sin filas), no se encontraron resultados.

El `Id` (primeros 8 caracteres) identifica cada entrada. Se puede usar el GUID completo para ver el detalle.

---

### 2. Ver detalle de una entrada de conocimiento

```
.\axiom knowledge show <guid>
```

**Ejemplo:**
```
.\axiom knowledge show 177ed8be-6ec1-49f6-8439-8164aa2ea180
```

**Output:** Un panel con: Title, Description, System, Type, Status, Author, Tags, Created, Updated, Content.

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

Lista todas las entradas de conocimiento no eliminadas. Misma tabla que `search`.

---

### 4. Ver detalle de un caso/incidente

```
.\axiom case show <guid>
```

**Ejemplo:**
```
.\axiom case show 74fc9278-5376-440d-9d72-38b68d5ff3de
```

**Output:** Un panel con: System, Problem, Analysis, Resolution, Lessons Learned, RITM ID, Change ID, Status, Created.

Usa este comando cuando el usuario pregunte por un caso específico o cuando tengas un GUID de caso.

**Si el GUID no existe:**
```
Case record not found.
```

---

### 5. Crear una entrada de conocimiento

```
.\axiom knowledge create --title "<título>" --content "<contenido>" --system "<sistema>" --author "<autor>" [--description "<descripción>"] [--tags "<tag1>,<tag2>"] [--type <Tipo>]
```

| Opción | Requerido | Descripción |
|---|---|---|
| `--title` | Sí | Título de la entrada |
| `--content` | Sí | Contenido principal |
| `--system` | Sí | Sistema o aplicación asociada |
| `--author` | Sí | Nombre del autor |
| `--description` | No | Descripción corta |
| `--tags` | No | Tags separados por coma. Ej: `"iis,reinicio"` |
| `--type` | No | Tipo: `Documentation`, `Runbook`, `Troubleshooting`, `Reference`, `Tutorial`, `Other` |

**Output:**
```
Knowledge entry created: 177ed8be-6ec1-49f6-8439-8164aa2ea180
  Title: Cómo reiniciar IIS
  System: IIS-Server
  Type: Troubleshooting
  Status: Draft
```

El GUID se usa para referenciar la entrada después.

---

### 6. Crear un caso/incidente

```
.\axiom case create --system "<sistema>" --problem "<problema>" [--analysis "<análisis>"] [--resolution "<resolución>"] [--lessons "<lecciones>"] [--ritm-id "<id>"] [--change-id "<id>"]
```

| Opción | Requerido | Descripción |
|---|---|---|
| `--system` | Sí | Sistema asociado |
| `--problem` | Sí | Descripción del problema |
| `--analysis` | No | Análisis de causa raíz |
| `--resolution` | No | Pasos de resolución |
| `--lessons` | No | Lecciones aprendidas |
| `--ritm-id` | No | ID del RITM (ej. `RITM001234`) |
| `--change-id` | No | ID del cambio (ej. `CHG005678`) |

**Output:**
```
Case record created: 74fc9278-5376-440d-9d72-38b68d5ff3de
  System: CRM-App
  Problem: Error 500 al cargar módulo de facturación
  Status: Open
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

### "Qué casos tenemos registrados?"

Actualmente solo se puede ver un caso individual por GUID. No hay comando para listar todos los casos.

### "Tenemos algún caso sobre el sistema X?"

No hay búsqueda por sistema para casos. Si el usuario tiene un GUID de caso, usa `.\axiom case show <guid>`. Si no, no hay forma de buscar casos actualmente.

---

## Consideraciones importantes

1. **Búsqueda sin resultados**: Si `knowledge search` devuelve una tabla vacía (solo encabezados), significa que no hay entradas que coincidan con la búsqueda
2. **GUID no encontrado**: Si `knowledge show` o `case show` no encuentran el GUID, el mensaje es `not found` en rojo
3. **IDs truncados**: En las tablas de listado, el `Id` muestra solo los primeros 8 caracteres del GUID. Para operaciones como `show`, usa el GUID completo
4. **Tags en la búsqueda**: La búsqueda también encuentra coincidencias dentro de los tags de cada entrada
5. **Case list no disponible**: No hay comando para listar casos. Solo se puede ver un caso si se conoce su GUID
6. **Errores de conexión**: Si la base de datos no está disponible, el CLI mostrará un error de SQL Server
