# Axiom — KnowledgeOps & Operational Continuity Platform

> Plataforma de Conocimiento Operacional y Continuidad.
> Construido con .NET 10, Clean Architecture, CQRS/MediatR, EF Core + SQL Server.

---

## 1. Arquitectura

### Clean Architecture (3 capas + entrypoint)

| Capa | Proyecto | Dependencias | Propósito |
|---|---|---|---|
| **Domain** | `Axiom.Domain` | Ninguna | Entidades (9), Value Objects, Excepciones |
| **Application** | `Axiom.Application` | Domain | Casos de uso CQRS (4 commands, 5 queries, 9 handlers), validación FluentValidation, interfaces de repositorio, DTOs de proyección |
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

### Dotnet tool global (recomendado)

Axiom está pensado para usarse como herramienta de consola instalada con
`dotnet tool`. Desde el repo, primero empaqueta el CLI y luego instálalo en el
perfil del usuario:

```powershell
dotnet pack .\src\Axiom.Cli\Axiom.Cli.csproj -c Release
dotnet tool install --global Axiom.Cli --add-source .\artifacts\packages --version 0.1.0-mvp.5
```

Si ya está instalado, actualiza con:

```powershell
dotnet tool update --global Axiom.Cli --add-source .\artifacts\packages --version 0.1.0-mvp.5
```

Una vez instalado, el uso recomendado es llamar directamente a `axiom`:

```powershell
axiom startup --demo
axiom knowledge search "login" --json
axiom issue list --json
axiom issue create --system-eai EAI003 --state-code OPEN --created-by-email ops.agent@axiom.local --summary "Incidente" --problem "Detalle" --json
```

### Alternativa local de desarrollo

El repo también incluye manifest de dotnet tools para uso local:

```powershell
dotnet pack .\src\Axiom.Cli\Axiom.Cli.csproj -c Release
dotnet tool restore --add-source .\artifacts\packages
```

Para ejecutar comandos, usa la instalación global recomendada y llama `axiom`.

### Variable de entorno

La conexión a BD se lee de `AXIOM_CONNECTION_STRING`. Si no está definida, se usa:

```
Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;
```

---

## 3. Comandos CLI

### `startup` — Asistente interactivo

Guía al usuario paso a paso para crear datos maestros iniciales:
1. Crear usuario (Email + Name)
2. Crear sistema (EAI + Name)
3. Crear tipos de conocimiento (Documentation, Runbook, Troubleshooting, Reference, Tutorial, Other)
4. Crear estados de issue (Open, InProgress, Resolved, Closed)
5. Crear estados de conocimiento (Draft, Published, Archived, Deprecated)

```powershell
axiom startup
```

Para demos con agentes, se puede cargar un set idempotente no interactivo:

```powershell
axiom startup --demo
axiom startup --demo --json
```

El seed demo crea usuarios, sistemas EAI, estados, tipos, issues y knowledge
entries de Operaciones TI en español. Puede ejecutarse varias veces sin duplicar
los datos base.

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

`knowledge create` e `issue create` aceptan IDs o claves naturales:

```powershell
axiom knowledge create --system-eai EAI001 --type-code RUNBOOK --state-code PUBLISHED --created-by-email ops.agent@axiom.local --title "Runbook" --content "Contenido" --json
axiom issue create --system-eai EAI003 --state-code OPEN --created-by-email ops.agent@axiom.local --summary "Incidente" --problem "Detalle" --json
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

`*` Debe usarse una sola forma por referencia: ID o clave natural, no ambas.

**Output:**
```
Knowledge entry created: <guid>
  Title: <title>
  Version: 1
```

### `knowledge list`

Lista todas las entradas de conocimiento activas en una tabla.

```powershell
axiom knowledge list
```

Columnas: `Id` (8 chars), `Title`, `System`, `Type`, `State`, `Version`, `Updated`.

### `knowledge show <guid>`

Muestra detalle completo de una entrada por GUID.

```powershell
axiom knowledge show 177ed8be-6ec1-49f6-8439-8164aa2ea180
```

Panel con: Title, Summary, Content, System, Type, State, Author, Tags, Version, IssueId, Created, Updated.

Si no existe: `Knowledge entry not found.`

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

`*` Debe usarse una sola forma por referencia: ID o clave natural, no ambas.

**Output:**
```
Issue created: <guid>
  Summary: <summary>
  State: <state>
```

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

```powershell
axiom issue show 74fc9278-5376-440d-9d72-38b68d5ff3de
```

Panel con: Summary, Problem, Analysis, Resolution, System, State, RITM, Incident, CreatedBy, Created, ResolvedAt.

Si no existe: `Issue not found.`

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
|---|---|---|
| `CreateKnowledgeCommand` | `CreateKnowledgeHandler` | `Knowledge` |
| `UpdateKnowledgeCommand` | `UpdateKnowledgeHandler` | `Knowledge?` |
| `DeleteKnowledgeCommand` | `DeleteKnowledgeHandler` | `bool` |
| `CreateIssueCommand` | `CreateIssueHandler` | `Issue` |

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
|---|---|
| `IKnowledgeRepository` | `SaveAsync`, `GetByIdAsync`, `SearchAsync`, `GetAllAsync`, `DeleteAsync` |
| `IIssueRepository` | `SaveAsync`, `GetByIdAsync`, `GetAllAsync` |
| `ITagRepository` | `FindOrCreateAsync(string)` |
| `IStartupService` | `CreateUserAsync`, `CreateSystemAsync`, `CreateKnowledgeTypeAsync`, `CreateIssueStateAsync`, `CreateKnowledgeStateAsync`, `SeedDemoDataAsync` |
| `IReferenceDataService` | Listado y resolución de usuarios, sistemas, tipos y estados por claves naturales |

---

## 6. Capa de Infraestructura

### Persistencia: EF Core + SQL Server

- **DbContext**: `AxiomDbContext` con 9 `DbSet`s y configuraciones vía `IEntityTypeConfiguration<T>`
- **Configuraciones**: 8 archivos en `Persistence/Configurations/` — una por entidad (PKs, FKs, indexes, tipos, delete behavior)
- **Repositorios**: `EfKnowledgeRepository`, `EfIssueRepository`, `EfTagRepository`, `EfStartupService`
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
dotnet build src\Axiom.Cli            # Solo el CLI
dotnet pack .\src\Axiom.Cli\Axiom.Cli.csproj -c Release
axiom knowledge list                  # Uso recomendado con dotnet tool global
```
