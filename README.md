# Axiom — Funcionalidades y Detalles del Proyecto

> Plataforma de Conocimiento Operacional y Continuidad (KnowledgeOps)
> Construido con .NET 10, Clean Architecture, CQRS/MediatR

---

## 1. Arquitectura

### Clean Architecture (3 capas + entrypoint)

| Capa | Proyecto | Dependencias | Propósito |
|---|---|---|---|
| **Domain** | `Axiom.Domain` | Ninguna | Entidades, Value Objects, Enums, Excepciones |
| **Application** | `Axiom.Application` | Domain | Casos de uso CQRS (Commands/Queries/Handlers), Validación (FluentValidation), Interfaces de repositorio |
| **Infrastructure** | `Axiom.Infrastructure` | Application + Domain | Persistencia JSON, configuración |
| **Entrypoint** | `Axiom.Cli` | Application + Infrastructure | CLI con System.CommandLine + Spectre.Console + MediatR |

### Stack principal

- **.NET 10** (`net10.0`)
- **MediatR 14** — CQRS in-process
- **FluentValidation 12** — validación de comandos
- **System.CommandLine 2** — parser de CLI
- **Spectre.Console 0.57** — UI bonita en terminal (tablas, paneles)
- **xUnit + FluentAssertions + NSubstitute** — tests

---

## 2. Comandos CLI

### `knowledge create`

Crea una entrada de conocimiento.

| Opción | Requerido | Descripción |
|---|---|---|
| `--title` | Sí | Título del conocimiento |
| `--content` | Sí | Contenido principal |
| `--system` | Sí | Sistema/aplicación asociada |
| `--author` | Sí | Autor |
| `--description` | No | Descripción corta |
| `--tags` | No | Tags separados por coma |
| `--type` | No | Tipo enum: `Documentation`, `Runbook`, `Troubleshooting`, `Reference`, `Tutorial`, `Other` (default: `Documentation`) |

### `knowledge list`

Lista todas las entradas de conocimiento en una tabla.

### `knowledge show <id>`

Muestra detalle de una entrada por GUID.

### `knowledge search <query>`

Busca entradas por texto en título, descripción, contenido y tags (case-insensitive).

### `case create`

Crea un registro de caso/incidente.

| Opción | Requerido | Descripción |
|---|---|---|
| `--system` | Sí | Sistema asociado |
| `--problem` | Sí | Descripción del problema |
| `--analysis` | No | Análisis de causa raíz |
| `--resolution` | No | Pasos de resolución |
| `--lessons` | No | Lecciones aprendidas |
| `--ritm-id` | No | ID RITM (ej. RITM12345) |
| `--change-id` | No | ID de cambio (ej. CHG67890) |

### `case show <id>`

Muestra detalle de un caso por GUID.

---

## 3. Capa de Dominio

### Entidades

#### `KnowledgeEntry`
- `Id` (Guid), `Title`, `Description`, `Content`, `System` (SystemName), `Tags` (List<string>), `CreatedAt`, `UpdatedAt`, `Author`, `Type` (KnowledgeType), `Status` (KnowledgeStatusValue)
- Validación: lanza `ArgumentException` si `Title` o `Content` están vacíos
- Valores por defecto: `Description` = `""`, `Tags` = lista vacía, `Author` = `"unknown"`
- Método `Update()`: modifica propiedades y actualiza `UpdatedAt`

#### `CaseRecord`
- `Id` (Guid), `RitmId` (RitmId?), `ChangeId` (string?), `System` (SystemName), `Problem`, `Analysis`, `Resolution`, `LessonsLearned`, `CreatedAt`, `Status` (CaseStatus)
- Validación: lanza `ArgumentException` si `Problem` está vacío
- Valores por defecto: `Analysis`/`Resolution`/`Lessons` = `""`, `Status` = `Open`
- Método `UpdateStatus(CaseStatus)`: cambia el estado

### Value Objects (`readonly record struct` con `JsonConverter`)

| VO | Propiedad | Fallback en JSON |
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

### Excepciones

- `DomainException` (abstracta) — base para excepciones de dominio

---

## 4. Capa de Aplicación (CQRS)

### Commands

| Command | Handler | Retorna |
|---|---|---|
| `CreateKnowledgeCommand` | `CreateKnowledgeHandler` | `KnowledgeEntry` |
| `UpdateKnowledgeCommand` | `UpdateKnowledgeHandler` | `KnowledgeEntry?` |
| `DeleteKnowledgeCommand` | `DeleteKnowledgeHandler` | `bool` |
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

## 5. Capa de Infraestructura

### Repositorios JSON

Ambos repositorios usan el mismo patrón:
- Leen todo el archivo JSON en memoria (`List<T>`)
- Mutan en memoria y escriben el archivo completo
- Serialización: `WriteIndented = true`, `PropertyNamingPolicy = CamelCase`
- Si el archivo no existe, retornan lista vacía (sin errores)
- Crean el directorio automáticamente al guardar

#### `JsonKnowledgeRepository : IKnowledgeRepository`

| Método | Comportamiento |
|---|---|
| `SaveAsync` | Busca por `Id`; si existe actualiza, si no agrega |
| `GetByIdAsync` | `FirstOrDefault` por `Id` |
| `GetAllAsync` | Retorna todas las entradas |
| `SearchAsync` | Match case-insensitive en `Title`, `Description`, `Content` o cualquier `Tag` |
| `DeleteAsync` | Remueve por `Id` |

#### `JsonCaseRepository : ICaseRepository`

| Método | Comportamiento |
|---|---|
| `SaveAsync` | Busca por `Id`; si existe actualiza, si no agrega |
| `GetByIdAsync` | `FirstOrDefault` por `Id` |
| `GetAllAsync` | Retorna todos los registros |

### Configuración (`JsonDataOptions`)

- Sección: `"JsonData"`
- `KnowledgeFilePath`: default `"data/knowledge.json"`
- `CasesFilePath`: default `"data/cases.json"`

### DI Registration

```csharp
// Application
services.AddApplication()     // MediatR + FluentValidation

// Infrastructure
services.AddInfrastructure()  // JsonKnowledgeRepository + JsonCaseRepository (Scoped)
```

---

## 6. Tests

| Proyecto | Archivos | Tests | Tema |
|---|---|---|---|
| `Axiom.Domain.Tests` | 4 | 11 | Entidades y Value Objects |
| `Axiom.Application.Tests` | 3 | 3 | Handlers (CreateKnowledge, CreateCase, ListKnowledge) |
| `Axiom.Integration.Tests` | 1 | 4 | Repositorios JSON (roundtrip, search, delete) |
| **Total** | **8** | **18** | |

### Framework: xUnit + FluentAssertions + NSubstitute + Coverlet

### Lo que cubren los tests

- **Domain**: Validación de constructores (`ArgumentException`), defaults (null tags → lista vacía), método `Update()`, igualdad de Value Objects, cambio de estado
- **Application**: Handlers crean entidades, llaman al repositorio, retornan datos correctos
- **Integration**: Persistencia y recuperación JSON en directorios temporales (con `IDisposable` cleanup)

---

## 7. Datos Semilla

- `data/knowledge.json` — 1 entrada de ejemplo
- `data/cases.json` — 1 caso de ejemplo

---

## 8. Comandos de Build y Test

```bash
dotnet build                          # Compila todo
dotnet test                           # Ejecuta todos los tests
dotnet test --filter "KnowledgeRepository_ShouldRoundTripEntry"  # Test específico
dotnet test tests/Axiom.Application.Tests                         # Proyecto específico
dotnet test --collect:"XPlat Code Coverage"                       # Con cobertura
```

---

## 9. Funcionalidades No Implementadas (Planeadas)

Lo siguiente está documentado en `AGENTS.md` como deseado pero **no existe en el código**:

| Funcionalidad | Estado |
|---|---|
| Persistencia con EF Core + SQL Server | ❌ No implementado |
| Migraciones de EF Core | ❌ No implementado |
| Método `AddInfrastructureEF(connectionString)` | ❌ No implementado |
| `DeletedAt` (soft-delete) + `HasQueryFilter` | ❌ No implementado |
| `ChangeId` como Value Object | ❌ No implementado |
| Eventos de dominio (`Domain/Events/`) | ❌ Placeholder vacío |
| DTOs (`Application/DTOs/`) | ❌ Placeholder vacío |
| Servicios de aplicación (`Application/Services/`) | ❌ Placeholder vacío |
| `appsettings.json` | ❌ No existe |

---

## 10. Detalles Técnicos Adicionales

- **Serialización JSON**: Los Value Objects usan `JsonConverter` personalizado para poder deserializar desde strings planos en JSON
- **Constructores privados**: Las entidades tienen constructores privados sin parámetros con `[JsonConstructor]` para JSON deserialization
- **Sin `global.json` / `Directory.Build.props`**: Cada project define su propio `TargetFramework`
- **Cobertura**: Coverlet configurado vía `coverlet.collector` en cada proyecto de test
- **Sin dependencias externas**: Los tests de integración usan `Path.GetTempPath()` sin fixtures ni servicios externos
