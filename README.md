# Axiom — KnowledgeOps & Operational Continuity Platform

> Plataforma de Conocimiento Operacional y Continuidad.
> Construido con .NET 10, Clean Architecture, CQRS/MediatR.

---

## 1. Arquitectura

### Clean Architecture (3 capas + entrypoint)

| Capa | Proyecto | Dependencias | Propósito |
|---|---|---|---|
| **Domain** | `Axiom.Domain` | Ninguna | Entidades, Value Objects, Enums, Excepciones |
| **Application** | `Axiom.Application` | Domain | Casos de uso CQRS, validación FluentValidation, interfaces de repositorio |
| **Infrastructure** | `Axiom.Infrastructure` | Application + Domain | Persistencia EF Core + SQL Server, repositorios, migraciones |
| **Entrypoint** | `Axiom.Cli` | Application + Infrastructure | CLI con System.CommandLine + Spectre.Console + MediatR |

### Stack principal

- **.NET 10** (`net10.0`) — sin `global.json` ni `Directory.Build.props`
- **MediatR 12.5** — CQRS in-process
- **FluentValidation 12** — validación de comandos
- **System.CommandLine 2** — parser de CLI
- **Spectre.Console 0.57** — UI en terminal (tablas, paneles)
- **EF Core 10 + SQL Server** — persistencia principal
- **xUnit + FluentAssertions + NSubstitute + Coverlet** — tests

---

## 2. Ejecución

### Wrapper script (recomendado)

```powershell
.\axiom <command> [args]
```

El script `axiom.ps1` resuelve automáticamente la ruta del proyecto.

### Directamente con dotnet

```powershell
dotnet run --project src\Axiom.Cli -- <command> [args]
```

### Variable de entorno

La conexión a BD se lee de `AXIOM_CONNECTION_STRING`. Si no está definida, se usa:

```
Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;
```

---

## 3. Comandos CLI

### `knowledge create`

Crea una entrada de conocimiento. **Status siempre `Draft`**, **todos los campos con validación de dominio**.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--title` | Sí | `string` | Título (max 500 chars) |
| `--content` | Sí | `string` | Contenido principal |
| `--system` | Sí | `string` | Sistema/aplicación asociada (max 200 chars) |
| `--author` | Sí | `string` | Autor (max 200 chars) |
| `--description` | No | `string` | Descripción corta |
| `--tags` | No | `string` | Tags separados por coma. Ej: `"iis,reinicio,produccion"` |
| `--type` | No | `enum` | `Documentation`, `Runbook`, `Troubleshooting`, `Reference`, `Tutorial`, `Other` |

**Output:**
```
Knowledge entry created: <guid>
  Title: <title>
  System: <system>
  Type: <type>
  Status: Draft
```

### `knowledge list`

Lista todas las entradas de conocimiento no eliminadas en una tabla.

```powershell
.\axiom knowledge list
```

Columnas: `Id` (8 chars), `Title`, `System`, `Type`, `Status`, `Updated`.

### `knowledge show <guid>`

Muestra detalle completo de una entrada por GUID.

```powershell
.\axiom knowledge show 177ed8be-6ec1-49f6-8439-8164aa2ea180
```

Panel con: Title, Description, System, Type, Status, Author, Tags, Created, Updated, Content.

Si no existe: `Knowledge entry not found.`

### `knowledge search <query>`

Busca entradas por texto en **Title**, **Description**, **Content** y **Tags** (case-insensitive).

```powershell
.\axiom knowledge search "IIS"
```

Misma tabla que `list`.

### `case create`

Crea un registro de caso/incidente. **Status siempre `Open`**.

| Opción | Requerido | Tipo | Descripción |
|---|---|---|---|
| `--system` | Sí | `string` | Sistema asociado (max 200 chars) |
| `--problem` | Sí | `string` | Descripción del problema |
| `--analysis` | No | `string` | Análisis de causa raíz |
| `--resolution` | No | `string` | Pasos de resolución |
| `--lessons` | No | `string` | Lecciones aprendidas |
| `--ritm-id` | No | `string` | ID RITM (ej. `RITM001234`) |
| `--change-id` | No | `string` | ID de cambio (ej. `CHG005678`) |

**Output:**
```
Case record created: <guid>
  System: <system>
  Problem: <problem>
  Status: Open
```

### `case show <guid>`

Muestra detalle completo de un caso por GUID.

```powershell
.\axiom case show 74fc9278-5376-440d-9d72-38b68d5ff3de
```

Panel con: System, Problem, Analysis, Resolution, Lessons Learned, RITM ID, Change ID, Status, Created.

Si no existe: `Case record not found.`

---

## 4. Capa de Dominio

### Entidades

#### `KnowledgeEntry`

| Propiedad | Tipo | Default / Notas |
|---|---|---|
| `Id` | `Guid` | Generado en constructor |
| `Title` | `string` | Requerido |
| `Description` | `string` | `""` si null |
| `Content` | `string` | Requerido |
| `System` | `SystemName` | Value Object |
| `Tags` | `List<string>` | Lista vacía si null |
| `CreatedAt` | `DateTime` | UTC |
| `UpdatedAt` | `DateTime` | UTC, refrescado en `Update()` |
| `Author` | `string` | `"unknown"` si null |
| `Type` | `KnowledgeType` | Enum |
| `Status` | `KnowledgeStatusValue` | Value Object |

Métodos: `Update(title, description, content, system, tags, author, type)` actualiza todo y refresca `UpdatedAt`.

#### `CaseRecord`

| Propiedad | Tipo | Default / Notas |
|---|---|---|
| `Id` | `Guid` | Generado en constructor |
| `RitmId` | `RitmId?` | Nullable Value Object |
| `ChangeId` | `string?` | Nullable string |
| `System` | `SystemName` | Value Object |
| `Problem` | `string` | Requerido |
| `Analysis` | `string` | `""` si null |
| `Resolution` | `string` | `""` si null |
| `LessonsLearned` | `string` | `""` si null |
| `CreatedAt` | `DateTime` | UTC |
| `Status` | `CaseStatus` | `Open` por defecto |

Métodos: `UpdateStatus(CaseStatus)` cambia el estado.

### Value Objects (`readonly record struct` con `JsonConverter`)

| VO | Propiedad | Fallback JSON |
|---|---|---|
| `SystemName` | `string Value` | `"unknown"` si null/vacío |
| `RitmId` | `string Value` | `"unknown"` si null/vacío |
| `KnowledgeStatusValue` | `KnowledgeStatus Value` | `Draft` si no se puede parsear |

### Enums

| Enum | Valores |
|---|---|
| `KnowledgeStatus` | `Draft`, `Published`, `Archived`, `Deprecated` |
| `KnowledgeType` | `Documentation`, `Runbook`, `Troubleshooting`, `Reference`, `Tutorial`, `Other` |
| `CaseStatus` | `Open`, `InProgress`, `Resolved`, `Closed` |

### Soft-delete

Las entidades tienen propiedad `DeletedAt` (DateTime?). Las queries aplican `HasQueryFilter(x => x.DeletedAt == null)` para excluir registros eliminados lógicamente.

---

## 5. Capa de Aplicación (CQRS)

### Commands

| Command | Handler | Retorna |
|---|---|---|
| `CreateKnowledgeCommand` | `CreateKnowledgeHandler` | `KnowledgeEntry` |
| `CreateCaseCommand` | `CreateCaseHandler` | `CaseRecord` |

### Queries

| Query | Handler | Retorna |
|---|---|---|
| `ListKnowledgeQuery` | `ListKnowledgeHandler` | `IEnumerable<KnowledgeEntry>` |
| `GetKnowledgeByIdQuery` | `GetKnowledgeByIdHandler` | `KnowledgeEntry?` |
| `SearchKnowledgeQuery` | `SearchKnowledgeHandler` | `IEnumerable<KnowledgeEntry>` |
| `GetCaseByIdQuery` | `GetCaseByIdHandler` | `CaseRecord?` |

### Validadores (FluentValidation)

| Validador | Reglas |
|---|---|
| `CreateKnowledgeValidator` | `Title`: NotEmpty, MaxLength(500); `Content`: NotEmpty; `System`: NotEmpty, MaxLength(200); `Author`: NotEmpty, MaxLength(200) |
| `CreateCaseValidator` | `System`: NotEmpty, MaxLength(200); `Problem`: NotEmpty |

---

## 6. Capa de Infraestructura

### Persistencia activa: EF Core + SQL Server

El CLI registra `AddInfrastructureEF(connectionString)` al iniciar, lo que configura `AxiomDbContext` con SQL Server.

- **DbContext**: `AxiomDbContext` con `DbSet<KnowledgeEntry>` y `DbSet<CaseRecord>`
- **Repositorios**: `EfKnowledgeRepository` y `EfCaseRepository` implementan las interfaces del dominio
- **Migraciones**: `src/Axiom.Infrastructure/Migrations/` — `InitialCreate` ya aplicada
- **Value Converters**: `TagsConverter` (List\<string> ↔ string), `KnowledgeStatusValueConverter` (enum ↔ byte), `SystemNameConverter`, `RitmIdConverter`
- **Soft-delete**: `HasQueryFilter(x => x.DeletedAt == null)` en ambas entidades

### Design-time factory

`AxiomDesignTimeDbContextFactory` lee `AXIOM_CONNECTION_STRING` del entorno (fallback: `Server=localhost;Database=AXIOM;...`) para comandos de migración.

### Repositorios JSON (legacy/alternativa)

Los repositorios JSON (`JsonKnowledgeRepository`, `JsonCaseRepository`) existen pero **no están registrados** por defecto en el CLI. Se activan con `AddInfrastructure()`.

Configuración en sección `"JsonData"`:

| Opción | Default |
|---|---|
| `KnowledgeFilePath` | `data/knowledge.json` |
| `CasesFilePath` | `data/cases.json` |

---

## 7. Tests

| Proyecto | Cantidad |
|---|---|
| `Axiom.Domain.Tests` | 11 tests (entidades, value objects) |
| `Axiom.Application.Tests` | 3 tests (handlers con NSubstitute) |
| `Axiom.Integration.Tests` | 4 tests (JSON repos roundtrip, search, delete) |

```bash
dotnet test                              # Todos los tests
dotnet test tests/Axiom.Application.Tests # Proyecto específico
dotnet test --filter "KnowledgeRepository_ShouldRoundTripEntry"
dotnet test --collect:"XPlat Code Coverage"
```

---

## 8. Archivos de documentación para agentes

| Archivo | Propósito |
|---|---|
| `AGENTS.md` | Guía del proyecto para el agente (estructura, stack, convenciones, git) |
| `instruction.md` | Instrucciones detalladas del CLI para consumo por agentes (comandos, opciones, tipos, enums, edge cases, ejemplos) |

---

## 9. Build

```bash
dotnet build                          # Compila todo
dotnet build src\Axiom.Cli            # Solo el CLI
dotnet run --project src\Axiom.Cli -- knowledge list
.\axiom knowledge list                # Con wrapper script
```
