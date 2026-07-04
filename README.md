# Axiom — KnowledgeOps & Operational Continuity Platform

> Plataforma de Conocimiento Operacional y Continuidad.
> Construido con .NET 10, Clean Architecture, CQRS/MediatR, EF Core + SQL Server.

---

## 1. Arquitectura

### Clean Architecture (3 capas + entrypoint)

| Capa | Proyecto | Dependencias | Propósito |
|---|---|---|---|
| **Domain** | `Axiom.Domain` | Ninguna | Entidades (9), Value Objects, Excepciones |
| **Application** | `Axiom.Application` | Domain | Casos de uso CQRS (11 commands, 5 queries, 17 handlers), validación FluentValidation, interfaces de repositorio, DTOs de proyección |
| **Infrastructure** | `Axiom.Infrastructure` | Application + Domain | Persistencia EF Core + SQL Server, migraciones, configuraciones por entidad, repositorios, startup service |
| **Entrypoint** | `Axiom.Cli` | Application + Infrastructure | CLI con System.CommandLine + Spectre.Console + MediatR |

### Stack principal

- **.NET 10** (`net10.0`) — sin `global.json` ni `Directory.Build.props`
- **MediatR 12.5** — CQRS in-process
- **FluentValidation 12** — validación de comandos
- **System.CommandLine 2** — parser de CLI
- **Spectre.Console 0.57** — UI en terminal (tablas, paneles, prompts interactivos)
- **EF Core 10 + SQL Server** — persistencia principal
- **xUnit + FluentAssertions + NSubstitute + Coverlet** — tests

---

## 2. Instalación y ejecución

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local o remoto) — opcional si se usa solo el almacén JSON local

### Variable de entorno

La conexión a BD se lee de `AXIOM_CONNECTION_STRING`. Si no está definida, se usa:

```
Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;
```

### Instalación como dotnet tool global (recomendado)

Axiom está pensado para usarse como herramienta de consola instalada con
`dotnet tool`. Desde el repo, empaqueta el CLI e instálalo:

```bash
dotnet pack src/Axiom.Cli/Axiom.Cli.csproj -c Release
dotnet tool install --global Axiom.Cli --add-source artifacts/packages --version 1.2.0
```

Para actualizar una instalación existente:

```bash
dotnet tool update --global Axiom.Cli --add-source artifacts/packages --version 1.2.0
```

Una vez instalado globalmente, usa directamente el comando `axiom`:

```bash
axiom startup --demo
axiom knowledge search "login" --json
axiom issue list --json
axiom issue create --system-eai EAI003 --state-code OPEN --created-by-email ops.agent@axiom.local --summary "Incidente" --problem "Detalle" --json
```

### Alternativa: herramienta local (development)

El repo incluye un manifest de dotnet tools para uso local sin instalación global:

```bash
dotnet pack src/Axiom.Cli/Axiom.Cli.csproj -c Release
dotnet tool restore --add-source artifacts/packages
```

Luego ejecuta comandos con `dotnet axiom`:

```bash
dotnet axiom startup --demo
dotnet axiom knowledge list
```

### Alternativa: ejecución directa con `dotnet run`

Sin necesidad de empaquetar ni instalar:

```bash
dotnet run --project src/Axiom.Cli -- startup --demo
dotnet run --project src/Axiom.Cli -- knowledge list
dotnet run --project src/Axiom.Cli -- knowledge search "IIS" --json
```

### Almacén JSON local (offline-first)

Cuando la base de datos no está disponible, Axiom guarda los datos en un
almacén JSON local (`~/.axiom/store/`). Todos los comandos de lectura y
escritura detectan automáticamente la caída de BD y operan contra este
almacén, mostrando un aviso `[yellow]DB unavailable[/]`. No requiere
configuración adicional.

---

## 3. Comandos CLI

### `startup` — Asistente interactivo

Inicializa los datos maestros necesarios para operar Axiom.

**Modo interactivo (sin flags):** Guía al usuario paso a paso para crear:
1. Usuarios (Email + Name)
2. Sistemas (EAI + Name + Owner)
3. Tipos de conocimiento (Code + Name)
4. Estados de issue (Code + Name)
5. Estados de conocimiento (Code + Name)

```bash
axiom startup
```

**Modo demo (`--demo`):** Carga un set idempotente de datos de demostración
sin interacción. Crea usuarios, sistemas EAI, estados, tipos, issues y
knowledge entries de Operaciones TI en español. Puede ejecutarse varias
veces sin duplicar los datos base.

```bash
axiom startup --demo
axiom startup --demo --json
```

### Salida JSON y lookups para agentes

Los comandos de lectura y creación soportan `--json` para salida machine-readable:

```powershell
axiom knowledge list --json
axiom knowledge search "login" --json
axiom issue list --json
axiom issue show <guid> --json
```

Datos maestros:

```powershell
axiom user list --json
axiom system list --json
axiom knowledge-type list --json
axiom knowledge-state list --json
axiom issue-state list --json
```

`knowledge create`, `knowledge update`, `issue create`, `issue update` y los comandos de actualización de datos maestros aceptan IDs o claves naturales:

```powershell
axiom knowledge create --system-eai EAI001 --type-code RUNBOOK --state-code PUBLISHED --created-by-email ops.agent@axiom.local --title "Runbook" --content "Contenido" --json
axiom knowledge update <guid> --system-eai EAI001 --type-code RUNBOOK --state-code PUBLISHED --title "Runbook v2" --content "Actualizado" --json
axiom issue create --system-eai EAI003 --state-code OPEN --created-by-email ops.agent@axiom.local --summary "Incidente" --problem "Detalle" --json
axiom issue update <guid> --system-eai EAI003 --state-code RESOLVED --summary "Incidente" --problem "Detalle" --resolution "Solucionado" --json

Actualización de datos maestros:
```powershell
axiom user update <guid> --email "nuevo@email.com" --name "Nuevo Nombre" --json
axiom system update <id> --eai EAI001 --name "Nuevo Sistema" --owner-email "owner@email.com" --json
axiom knowledge-type update <id> --code RUNBOOK --name "Runbook" --json
axiom knowledge-state update <id> --code PUBLISHED --name "Publicado" --json
axiom issue-state update <id> --code RESOLVED --name "Resuelto" --json
axiom knowledge-tag update <id> --name "nuevo-tag" --json
```

### `knowledge create`

Crea una entrada de conocimiento. **VersionNumber siempre 1**, FK a datos maestros existentes.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--title` | Sí | `string` | Título (max 500 chars) |
| `--content` | Sí | `string` | Contenido principal |
| `--summary` | No | `string` | Resumen corto |
| `--system-id` | Sí* | `long` | ID del sistema asociado |
| `--system-eai` | Sí* | `string` | Código EAI del sistema asociado |
| `--type-id` | Sí* | `long` | ID del tipo de conocimiento |
| `--type-code` | Sí* | `string` | Código del tipo de conocimiento |
| `--state-id` | Sí* | `int` | ID del estado de conocimiento |
| `--state-code` | Sí* | `string` | Código del estado de conocimiento |
| `--created-by` | Sí* | `guid` | ID del usuario creador |
| `--created-by-email` | Sí* | `string` | Email del usuario creador |
| `--tags` | No | `string` | Tags separados por coma. Ej: `"iis,reinicio,produccion"` |
| `--issue-id` | No | `guid` | ID del issue relacionado |
| `--json` | No | `bool` | Devuelve salida machine-readable |
| `--wizard` | No | `bool` | Lanza asistente interactivo |

`*` Debe usarse una sola forma por referencia: ID o clave natural, no ambas.

**Output:**
```
Knowledge entry created: <guid>
  Title: <title>
  Version: 1
```

**Modo interactivo (`--wizard`):** Guía al usuario paso a paso con menús
de selección para elegir usuario, sistema, tipo y estado; luego solicita
título, resumen, contenido, tags e issue ID. Opcionalmente compatible con
`--json`.

### `knowledge update <guid>`

Actualiza una entrada de conocimiento existente. Incrementa `VersionNumber` automáticamente.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--title` | Sí | `string` | Título (max 500 chars) |
| `--content` | Sí | `string` | Contenido principal |
| `--summary` | No | `string` | Resumen corto |
| `--system-id` | Sí* | `long` | ID del sistema asociado |
| `--system-eai` | Sí* | `string` | Código EAI del sistema asociado |
| `--type-id` | Sí* | `long` | ID del tipo de conocimiento |
| `--type-code` | Sí* | `string` | Código del tipo de conocimiento |
| `--state-id` | Sí* | `int` | ID del estado de conocimiento |
| `--state-code` | Sí* | `string` | Código del estado de conocimiento |
| `--tags` | No | `string` | Tags separados por coma. Reemplaza los existentes |
| `--issue-id` | No | `guid` | ID del issue relacionado |
| `--json` | No | `bool` | Devuelve salida machine-readable |

`*` Debe usarse una sola forma por referencia: ID o clave natural, no ambas.

**Output:**
```
Knowledge updated: <guid>
  Title: <title>
  Version: <N+1>
```

Si el GUID no existe: `Knowledge entry not found.`
Si la BD no está disponible, actualiza en el almacén JSON local.

### `knowledge list`

Lista todas las entradas de conocimiento activas en una tabla.

```powershell
axiom knowledge list
```

Columnas: `Id` (8 chars), `Title`, `System`, `Type`, `State`, `Version`, `Updated`.

### `knowledge show <guid>`

Muestra detalle completo de una entrada por GUID.

```bash
axiom knowledge show 177ed8be-6ec1-49f6-8439-8164aa2ea180
```

Panel con: Title, Summary, Content, System, Type, State, Author, Tags, Version, IssueId, Created, Updated.

Si no existe: `Knowledge entry not found.`

Si la BD no está disponible, lee desde el almacén JSON local y muestra
`(local store)` en el encabezado.

### `knowledge search <query>`

Busca entradas por texto en **Title**, **Summary** y **Content** (case-insensitive).

```powershell
axiom knowledge search "IIS"
```

Misma tabla que `list`.

### `issue create`

Crea un registro de issue/incidencia. FK a datos maestros existentes.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--summary` | Sí | `string` | Resumen (max 200 chars) |
| `--problem` | Sí | `string` | Descripción del problema |
| `--system-id` | Sí* | `long` | ID del sistema asociado |
| `--system-eai` | Sí* | `string` | Código EAI del sistema asociado |
| `--state-id` | Sí* | `int` | ID del estado de issue |
| `--state-code` | Sí* | `string` | Código del estado de issue |
| `--created-by` | Sí* | `guid` | ID del usuario creador |
| `--created-by-email` | Sí* | `string` | Email del usuario creador |
| `--analysis` | No | `string` | Análisis de causa raíz |
| `--resolution` | No | `string` | Pasos de resolución |
| `--ritm-number` | No | `string` | Número RITM (único nullable) |
| `--incident-number` | No | `string` | Número de incidencia (único nullable) |
| `--json` | No | `bool` | Devuelve salida machine-readable |
| `--wizard` | No | `bool` | Lanza asistente interactivo |

`*` Debe usarse una sola forma por referencia: ID o clave natural, no ambas.

**Output:**
```
Issue created: <guid>
  Summary: <summary>
  State: <state>
```

**Modo interactivo (`--wizard`):** Similar a `knowledge create --wizard`,
con menús de selección para usuario, sistema y estado; campos opcionales
para analysis, resolution, RITM e incident number.

### `issue list`

Lista todos los issues activos en una tabla. Opcionalmente filtra por código EAI del sistema.

```powershell
axiom issue list
axiom issue list --eai EAI001
```

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--eai` | No | `string` | Código EAI del sistema para filtrar |

Columnas: `Id` (8 chars), `Summary`, `System`, `State`, `RITM`, `Incident`, `Created`.

### `issue show <guid>`

Muestra detalle completo de un issue por GUID.

```bash
axiom issue show 74fc9278-5376-440d-9d72-38b68d5ff3de
```

Panel con: Summary, Problem, Analysis, Resolution, System, State, RITM, Incident, CreatedBy, Created, ResolvedAt.

Si no existe: `Issue not found.`
Si la BD no está disponible, lee desde el almacén JSON local y muestra
`(local store)` en el encabezado.

### `issue update <guid>`

Actualiza un issue existente.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--summary` | Sí | `string` | Resumen (max 200 chars) |
| `--problem` | Sí | `string` | Descripción del problema |
| `--system-id` | Sí* | `long` | ID del sistema asociado |
| `--system-eai` | Sí* | `string` | Código EAI del sistema asociado |
| `--state-id` | Sí* | `int` | ID del estado de issue |
| `--state-code` | Sí* | `string` | Código del estado de issue |
| `--analysis` | No | `string` | Análisis de causa raíz |
| `--resolution` | No | `string` | Pasos de resolución |
| `--ritm-number` | No | `string` | Número RITM |
| `--incident-number` | No | `string` | Número de incidencia |
| `--json` | No | `bool` | Devuelve salida machine-readable |

`*` Debe usarse una sola forma por referencia: ID o clave natural, no ambas.

**Output:**
```
Issue updated: <guid>
  Summary: <summary>
```

Si el GUID no existe: `Issue not found.`
Si la BD no está disponible, actualiza en el almacén JSON local.

### `issue delete <id>`

Elimina un issue por GUID.

```bash
axiom issue delete 74fc9278-5376-440d-9d72-38b68d5ff3de
```

| Argumento | Requerido | Tipo | Descripción |
|---|---|---|---|
| `id` | Sí | `guid` | GUID del issue |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Issue deleted: <guid>
```

Si no existe: `Issue not found.`
Si la BD no está disponible, elimina del almacén JSON local.

### `user update <id>`

Actualiza un usuario existente.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--email` | Sí | `string` | Nuevo email |
| `--name` | Sí | `string` | Nuevo nombre |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
User updated: <guid>
  Email: <email>
  Name: <name>
```

### `user create`

Crea un nuevo usuario.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--email` | Sí | `string` | Email del usuario |
| `--name` | Sí | `string` | Nombre del usuario |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
User created: <guid>
  Email: <email>
  Name: <name>
```

Si la BD no está disponible, guarda en el almacén JSON local.

### `user delete <id>`

Elimina un usuario por GUID.

```bash
axiom user delete 74fc9278-5376-440d-9d72-38b68d5ff3de
```

| Argumento | Requerido | Tipo | Descripción |
|---|---|---|---|
| `id` | Sí | `guid` | GUID del usuario |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
User deleted: <guid>
```

Si no existe: `User not found.`
Si la BD no está disponible, elimina del almacén JSON local.

### `system update <id>`

Actualiza un sistema existente.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--eai` | Sí | `string` | Código EAI |
| `--name` | Sí | `string` | Nombre del sistema |
| `--owner-id` | Sí* | `guid` | ID del usuario propietario |
| `--owner-email` | Sí* | `string` | Email del usuario propietario |
| `--json` | No | `bool` | Devuelve salida machine-readable |

`*` Debe usarse una sola forma: ID o email, no ambas.

**Output:**
```
System updated: <id>
  EAI: <eai>
  Name: <name>
  Owner: <ownerUserId>
```

### `system create`

Crea un nuevo sistema.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--eai` | Sí | `string` | Código EAI (max 20 chars) |
| `--name` | Sí | `string` | Nombre del sistema |
| `--owner-id` | Sí* | `guid` | ID del usuario propietario |
| `--owner-email` | Sí* | `string` | Email del usuario propietario |
| `--json` | No | `bool` | Devuelve salida machine-readable |

`*` Debe usarse una sola forma: ID o email, no ambas.

**Output:**
```
System created: <id>
  EAI: <eai>
  Name: <name>
  Owner: <ownerUserId>
```

Si la BD no está disponible, guarda en el almacén JSON local.

### `system delete <id>`

Elimina un sistema por ID numérico.

```bash
axiom system delete 1
```

| Argumento | Requerido | Tipo | Descripción |
|---|---|---|---|
| `id` | Sí | `long` | ID del sistema |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
System deleted: <id>
```

Si no existe: `System not found.`
Si la BD no está disponible, elimina del almacén JSON local.

### `knowledge-type update <id>`

Actualiza un tipo de conocimiento.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--code` | Sí | `string` | Código del tipo |
| `--name` | Sí | `string` | Nombre del tipo |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge type updated: <id>
  Code: <code>
  Name: <name>
```

### `knowledge-type create`

Crea un nuevo tipo de conocimiento.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--code` | Sí | `string` | Código del tipo |
| `--name` | Sí | `string` | Nombre del tipo |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge type created: <id>
  Code: <code>
  Name: <name>
```

Si la BD no está disponible, guarda en el almacén JSON local.

### `knowledge-type delete <id>`

Elimina un tipo de conocimiento por ID numérico.

```bash
axiom knowledge-type delete 1
```

| Argumento | Requerido | Tipo | Descripción |
|---|---|---|---|
| `id` | Sí | `long` | ID del tipo de conocimiento |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge type deleted: <id>
```

Si no existe: `Knowledge type not found.`
Si la BD no está disponible, elimina del almacén JSON local.

### `knowledge-state update <id>`

Actualiza un estado de conocimiento.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--code` | Sí | `string` | Código del estado |
| `--name` | Sí | `string` | Nombre del estado |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge state updated: <id>
  Code: <code>
  Name: <name>
```

### `knowledge-state create`

Crea un nuevo estado de conocimiento.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--code` | Sí | `string` | Código del estado |
| `--name` | Sí | `string` | Nombre del estado |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge state created: <id>
  Code: <code>
  Name: <name>
```

Si la BD no está disponible, guarda en el almacén JSON local.

### `knowledge-state delete <id>`

Elimina un estado de conocimiento por ID numérico.

```bash
axiom knowledge-state delete 1
```

| Argumento | Requerido | Tipo | Descripción |
|---|---|---|---|
| `id` | Sí | `int` | ID del estado de conocimiento |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge state deleted: <id>
```

Si no existe: `Knowledge state not found.`
Si la BD no está disponible, elimina del almacén JSON local.

### `issue-state update <id>`

Actualiza un estado de issue.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--code` | Sí | `string` | Código del estado |
| `--name` | Sí | `string` | Nombre del estado |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Issue state updated: <id>
  Code: <code>
  Name: <name>
```

### `issue-state create`

Crea un nuevo estado de issue.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--code` | Sí | `string` | Código del estado |
| `--name` | Sí | `string` | Nombre del estado |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Issue state created: <id>
  Code: <code>
  Name: <name>
```

Si la BD no está disponible, guarda en el almacén JSON local.

### `issue-state delete <id>`

Elimina un estado de issue por ID numérico.

```bash
axiom issue-state delete 1
```

| Argumento | Requerido | Tipo | Descripción |
|---|---|---|---|
| `id` | Sí | `int` | ID del estado de issue |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Issue state deleted: <id>
```

Si no existe: `Issue state not found.`
Si la BD no está disponible, elimina del almacén JSON local.

### `knowledge-tag list`

Lista todos los tags de conocimiento.

```powershell
axiom knowledge-tag list
axiom knowledge-tag list --json
```

Columnas: `Id`, `Name`.

### `knowledge-tag update <id>`

Actualiza un tag de conocimiento.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--name` | Sí | `string` | Nuevo nombre del tag |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge tag updated: <id>
  Name: <tagName>
```

### `knowledge-tag create`

Crea un nuevo tag de conocimiento.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--name` | Sí | `string` | Nombre del tag |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge tag created: <id>
  Name: <tagName>
```

Si la BD no está disponible, guarda en el almacén JSON local.

### `knowledge-tag delete <id>`

Elimina un tag de conocimiento por ID numérico.

```bash
axiom knowledge-tag delete 1
```

| Argumento | Requerido | Tipo | Descripción |
|---|---|---|---|
| `id` | Sí | `long` | ID del tag |
| `--json` | No | `bool` | Devuelve salida machine-readable |

**Output:**
```
Knowledge tag deleted: <id>
```

Si no existe: `Knowledge tag not found.`
Si la BD no está disponible, elimina del almacén JSON local.

---

## 4. Modelo de Datos (9 entidades)

### Entidades del dominio

| Entidad | PK | FK | Notas |
|---|---|---|---|
| `User` | `UserId` (Guid) | — | `Email` único, `Name` |
| `AxiomSystem` | `SystemId` (long, identity) | `OwnerUserId` → User | `EAI`(20), `Name`(200) |
| `KnowledgeType` | `TypeId` (long, identity) | — | `Code` único, `Name`(200) |
| `KnowledgeState` | `StateId` (int, identity) | — | `Code` único, `Name`(200) |
| `IssueState` | `StateId` (int, identity) | — | `Code` único, `Name`(200) |
| `KnowledgeTag` | `KnowledgeTagId` (long, identity) | — | `TagName`(100) único |
| `Knowledge` | `KnowledgeId` (Guid) | `SystemId`, `CreatedByUserId`, `KnowledgeTypeId`, `KnowledgeStateId`, `IssueId` (nullable) | `Title`, `Summary`, `Content`, `VersionNumber` |
| `Issue` | `IssueId` (Guid) | `SystemId`, `StateId`, `CreatedByUserId` | `Summary`, `Problem`, `Analysis`, `Resolution`, `RitmNumber`/`IncidentNumber` (únicos nullables), `ResolvedAt` |
| `KnowledgeKnowledgeTag` | Compuesta (`KnowledgeId`+`KnowledgeTagId`) | Ambos FK | Join table many-to-many |

### Relaciones principales

- `User` → `AxiomSystem` (1:N), `Issue` (1:N), `Knowledge` (1:N)
- `AxiomSystem` → `Issue` (1:N), `Knowledge` (1:N)
- `IssueState` → `Issue` (1:N)
- `KnowledgeState` → `Knowledge` (1:N)
- `KnowledgeType` → `Knowledge` (1:N)
- `Issue` → `Knowledge` (1:N, nullable FK)
- `Knowledge` ↔ `KnowledgeTag` (N:M via `KnowledgeKnowledgeTag`)

### Soft-delete

Las entidades `Knowledge` e `Issue` tienen propiedad `DeletedAt` (DateTime?). Las queries aplican `HasQueryFilter(x => x.DeletedAt == null)` en el DbContext.

---

## 5. Capa de Aplicación (CQRS)

### Commands

| Command | Handler | Retorna |
|---|---|---|---|
| `CreateKnowledgeCommand` | `CreateKnowledgeHandler` | `Knowledge` |
| `UpdateKnowledgeCommand` | `UpdateKnowledgeHandler` | `Knowledge?` |
| `DeleteKnowledgeCommand` | `DeleteKnowledgeHandler` | `bool` |
| `CreateIssueCommand` | `CreateIssueHandler` | `Issue` |
| `UpdateIssueCommand` | `UpdateIssueHandler` | `Issue?` |
| `DeleteIssueCommand` | `DeleteIssueHandler` | `bool` |
| `CreateUserCommand` | `CreateUserHandler` | `User` |
| `UpdateUserCommand` | `UpdateUserHandler` | `User?` |
| `DeleteUserCommand` | `DeleteUserHandler` | `bool` |
| `CreateSystemCommand` | `CreateSystemHandler` | `AxiomSystem` |
| `UpdateSystemCommand` | `UpdateSystemHandler` | `AxiomSystem?` |
| `DeleteSystemCommand` | `DeleteSystemHandler` | `bool` |
| `CreateKnowledgeTypeCommand` | `CreateKnowledgeTypeHandler` | `KnowledgeType` |
| `UpdateKnowledgeTypeCommand` | `UpdateKnowledgeTypeHandler` | `KnowledgeType?` |
| `DeleteKnowledgeTypeCommand` | `DeleteKnowledgeTypeHandler` | `bool` |
| `CreateKnowledgeStateCommand` | `CreateKnowledgeStateHandler` | `KnowledgeState` |
| `UpdateKnowledgeStateCommand` | `UpdateKnowledgeStateHandler` | `KnowledgeState?` |
| `DeleteKnowledgeStateCommand` | `DeleteKnowledgeStateHandler` | `bool` |
| `CreateIssueStateCommand` | `CreateIssueStateHandler` | `IssueState` |
| `UpdateIssueStateCommand` | `UpdateIssueStateHandler` | `IssueState?` |
| `DeleteIssueStateCommand` | `DeleteIssueStateHandler` | `bool` |
| `CreateKnowledgeTagCommand` | `CreateKnowledgeTagHandler` | `KnowledgeTag` |
| `UpdateKnowledgeTagCommand` | `UpdateKnowledgeTagHandler` | `KnowledgeTag?` |
| `DeleteKnowledgeTagCommand` | `DeleteKnowledgeTagHandler` | `bool` |

### Queries

| Query | Handler | Retorna |
|---|---|---|
| `ListKnowledgeQuery` | `ListKnowledgeHandler` | `IEnumerable<KnowledgeDto>` |
| `GetKnowledgeByIdQuery` | `GetKnowledgeByIdHandler` | `Knowledge?` |
| `SearchKnowledgeQuery` | `SearchKnowledgeHandler` | `IEnumerable<KnowledgeDto>` |
| `ListIssuesQuery` | `ListIssuesHandler` | `IEnumerable<IssueDto>` |
| `GetIssueByIdQuery` | `GetIssueByIdHandler` | `Issue?` |

### DTOs (proyecciones de solo lectura)

- `KnowledgeDto` — `KnowledgeId`, `Title`, `Summary`, `SystemName`, `Tags`, `TypeName`, `StateName`, `CreatedByName`, `VersionNumber`, `UpdatedAt`
- `IssueDto` — `IssueId`, `Summary`, `SystemName`, `StateName`, `RitmNumber`, `IncidentNumber`, `CreatedAt`, `ResolvedAt`

### Validadores (FluentValidation)

| Validador | Reglas principales |
|---|---|
| `CreateKnowledgeValidator` | `Title`: NotEmpty, MaxLength(500); `Content`: NotEmpty; `SystemId` > 0; `CreatedByUserId` not empty; `KnowledgeTypeId` > 0; `KnowledgeStateId` > 0 |
| `CreateIssueValidator` | `Summary`: NotEmpty, MaxLength(200); `Problem`: NotEmpty; `SystemId` > 0; `CreatedByUserId` not empty; `StateId` > 0 |

### Interfaces de repositorio

| Interfaz | Métodos |
|---|---|---|
| `IKnowledgeRepository` | `SaveAsync`, `GetByIdAsync`, `SearchAsync`, `GetAllAsync`, `DeleteAsync` |
| `IIssueRepository` | `SaveAsync`, `GetByIdAsync`, `GetAllAsync`, `GetByEaiAsync`, `DeleteAsync` |
| `ITagRepository` | `FindOrCreateAsync(string)` |
| `IUserRepository` | `GetByIdAsync`, `SaveAsync`, `DeleteAsync` |
| `ISystemRepository` | `GetByIdAsync`, `SaveAsync`, `DeleteAsync` |
| `IKnowledgeTypeRepository` | `GetByIdAsync`, `SaveAsync`, `DeleteAsync` |
| `IKnowledgeStateRepository` | `GetByIdAsync`, `SaveAsync`, `DeleteAsync` |
| `IIssueStateRepository` | `GetByIdAsync`, `SaveAsync`, `DeleteAsync` |
| `IKnowledgeTagRepository` | `GetByIdAsync`, `SaveAsync`, `GetAllAsync`, `DeleteAsync` |
| `IStartupService` | `CreateUserAsync`, `CreateSystemAsync`, `CreateKnowledgeTypeAsync`, `CreateIssueStateAsync`, `CreateKnowledgeStateAsync`, `SeedDemoDataAsync` |
| `IReferenceDataService` | Listado y resolución de usuarios, sistemas, tipos y estados por claves naturales |

---

## 6. Capa de Infraestructura

### Persistencia: EF Core + SQL Server

- **DbContext**: `AxiomDbContext` con 9 `DbSet`s y configuraciones vía `IEntityTypeConfiguration<T>`
- **Configuraciones**: 8 archivos en `Persistence/Configurations/` — una por entidad (PKs, FKs, indexes, tipos, delete behavior)
- **Repositorios**: `EfKnowledgeRepository`, `EfIssueRepository`, `EfTagRepository`, `EfUserRepository`, `EfSystemRepository`, `EfKnowledgeTypeRepository`, `EfKnowledgeStateRepository`, `EfIssueStateRepository`, `EfKnowledgeTagRepository`, `EfStartupService`
- **Migraciones**: `src/Axiom.Infrastructure/Migrations/` — `InitialCreate` ya aplicada (EF Core 10.0.9)
- **FK delete behavior**: `Restrict` para la mayoría, `Cascade` para join table `KnowledgeKnowledgeTags`, `SetNull` para Knowledge → Issue

### Design-time factory

`AxiomDesignTimeDbContextFactory` lee `AXIOM_CONNECTION_STRING` del entorno (fallback: `Server=localhost;Database=AXIOM;...`) para comandos de migración.

---

## 7. Tests

| Proyecto | Tests |
|---|---|
| `Axiom.Domain.Tests` | 10 tests (entidades Knowledge e Issue) |
| `Axiom.Application.Tests` | 3 tests (handlers con NSubstitute) |
| `Axiom.Integration.Tests` | 15 tests (EF Core InMemory — startup service, reference data service, repositorios Knowledge e Issue) |

```bash
dotnet test                              # Todos los tests
dotnet test tests/Axiom.Application.Tests # Proyecto específico
dotnet test --filter "ShouldRoundTripEntry"
dotnet test --collect:"XPlat Code Coverage"
```

Tests de integración usan proveedor InMemory de EF Core con datos maestros seed (User, System, KnowledgeType, KnowledgeState, IssueState). No requieren SQL Server ni variables de entorno.

---

## 8. Documentación adicional

| Archivo | Propósito |
|---|---|
| `AGENTS.md` | Guía del proyecto para el agente (estructura, stack, convenciones, git) |
| `docs.md` | Instrucciones detalladas del CLI para consumo por agentes (comandos, opciones, tipos, enums, edge cases, ejemplos) |
| `data/tables.md` | Definiciones de tablas (columnas, PKs, FKs, constraints) |
| `data/diagram.md` | Diagrama ER en Mermaid |

---

## 9. Build

```bash
dotnet build                          # Compila todo
dotnet build src/Axiom.Cli            # Solo el CLI
dotnet pack src/Axiom.Cli/Axiom.Cli.csproj -c Release
axiom knowledge list                  # Uso recomendado con dotnet tool global
```
