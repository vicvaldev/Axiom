# Axiom Documentation Index

## Project Overview

Axiom is a KnowledgeOps and Operational Continuity Platform built with Clean Architecture in .NET 10. It provides CLI-driven management of knowledge entries and case records with dual persistence (JSON files / EF Core SQL Server).

## Axiom.Domain

### Namespace: `Axiom.Domain.Enums`

| Type | Kind | Summary |
|------|------|---------|
| `KnowledgeStatus` | Enum | Lifecycle status of a knowledge entry (Draft, Published, Archived, Deprecated). |
| `KnowledgeType` | Enum | Categorization of knowledge content (Documentation, Runbook, Troubleshooting, Reference, Tutorial, Other). |
| `CaseStatus` | Enum | State of a case record (Open, InProgress, Resolved, Closed). |

### Namespace: `Axiom.Domain.ValueObjects`

| Type | Kind | Summary |
|------|------|---------|
| `RitmId` | readonly record struct | RITM (Request Item) identifier value object with custom JSON converter. |
| `RitmIdJsonConverter` | Class | JSON converter for RitmId; falls back to "unknown" on null/empty. |
| `SystemName` | readonly record struct | System or application name value object with custom JSON converter. |
| `SystemNameJsonConverter` | Class | JSON converter for SystemName; falls back to "unknown" on null/empty. |
| `KnowledgeStatusValue` | readonly record struct | Wraps the KnowledgeStatus enum as a value object with JSON converter. |
| `KnowledgeStatusValueJsonConverter` | Class | JSON converter for KnowledgeStatusValue; falls back to Draft on parse failure. |

### Namespace: `Axiom.Domain.Entities`

| Type | Kind | Summary |
|------|------|---------|
| `KnowledgeEntry` | Class | Knowledge entry entity with Title, Content, System, Tags, Author, Type, Status. |
| `CaseRecord` | Class | Case record entity with Problem, Analysis, Resolution, RitmId, ChangeId, Status. |

### Namespace: `Axiom.Domain.Exceptions`

| Type | Kind | Summary |
|------|------|---------|
| `DomainException` | Abstract Class | Base exception class for domain-level exceptions. |

## Axiom.Application

### Namespace: `Axiom.Application.Interfaces`

| Type | Kind | Summary |
|------|------|---------|
| `IKnowledgeRepository` | Interface | Contract for persisting/retrieving KnowledgeEntry records (Save, GetById, Search, GetAll, Delete). |
| `ICaseRepository` | Interface | Contract for persisting/retrieving CaseRecord entities (Save, GetById, GetAll). |

### Namespace: `Axiom.Application.Commands`

| Type | Kind | Summary |
|------|------|---------|
| `CreateKnowledgeCommand` | Record | Command to create a new knowledge entry (MediatR `IRequest<KnowledgeEntry>`). |
| `CreateCaseCommand` | Record | Command to create a new case record (MediatR `IRequest<CaseRecord>`). |

### Namespace: `Axiom.Application.Queries`

| Type | Kind | Summary |
|------|------|---------|
| `ListKnowledgeQuery` | Record | Query to retrieve all knowledge entries. |
| `GetKnowledgeByIdQuery` | Record | Query to retrieve a knowledge entry by ID. |
| `SearchKnowledgeQuery` | Record | Query to search knowledge entries by free-text. |
| `GetCaseByIdQuery` | Record | Query to retrieve a case record by ID. |

### Namespace: `Axiom.Application.Handlers`

| Type | Kind | Summary |
|------|------|---------|
| `CreateKnowledgeHandler` | Class | Handles CreateKnowledgeCommand: creates KnowledgeEntry and persists it. |
| `CreateCaseHandler` | Class | Handles CreateCaseCommand: creates CaseRecord and persists it. |
| `ListKnowledgeHandler` | Class | Handles ListKnowledgeQuery: retrieves all knowledge entries. |
| `GetKnowledgeByIdHandler` | Class | Handles GetKnowledgeByIdQuery: retrieves a single entry by ID. |
| `SearchKnowledgeHandler` | Class | Handles SearchKnowledgeQuery: searches entries by query text. |
| `GetCaseByIdHandler` | Class | Handles GetCaseByIdQuery: retrieves a single case record by ID. |

### Namespace: `Axiom.Application.Validators`

| Type | Kind | Summary |
|------|------|---------|
| `CreateKnowledgeValidator` | Class | FluentValidation rules for CreateKnowledgeCommand. |
| `CreateCaseValidator` | Class | FluentValidation rules for CreateCaseCommand. |

### Namespace: `Axiom.Application`

| Type | Kind | Summary |
|------|------|---------|
| `DependencyInjection` | Static Class | Extension method `AddApplication()` to register MediatR and FluentValidation. |

## Axiom.Infrastructure

### Namespace: `Axiom.Infrastructure.Data`

| Type | Kind | Summary |
|------|------|---------|
| `JsonDataOptions` | Class | Configuration options for JSON file paths (KnowledgeFilePath, CasesFilePath). |
| `JsonKnowledgeRepository` | Class | JSON file-backed implementation of IKnowledgeRepository. |
| `JsonCaseRepository` | Class | JSON file-backed implementation of ICaseRepository. |

### Namespace: `Axiom.Infrastructure`

| Type | Kind | Summary |
|------|------|---------|
| `DependencyInjection` | Static Class | Extension method `AddInfrastructure()` to register JSON repositories. |

## Axiom.Cli

### File: `Program.cs`

| Section | Summary |
|---------|---------|
| Root | Axiom CLI entry point with `--help` support. |
| `knowledge create` | Creates a new knowledge entry (--title, --description, --content, --system, --tags, --author, --type). |
| `knowledge list` | Lists all knowledge entries in a table. |
| `knowledge show <id>` | Shows detailed info for a single knowledge entry. |
| `knowledge search <query>` | Searches entries by free-text query. |
| `case create` | Creates a new case record (--system, --problem, --analysis, --resolution, --lessons, --ritm-id, --change-id). |
| `case show <id>` | Shows detailed info for a single case record. |

## Tests

### `Axiom.Domain.Tests`

| File | Type | Summary |
|------|------|---------|
| `Entities/KnowledgeEntryTests.cs` | Test Class | 8 tests covering constructor validation, defaults, and Update method. |
| `Entities/CaseRecordTests.cs` | Test Class | 3 tests covering construction and status updates. |

### `Axiom.Application.Tests`

| File | Type | Summary |
|------|------|---------|
| `Handlers/CreateKnowledgeHandlerTests.cs` | Test Class | Verifies handler creates and persists a knowledge entry. |
| `Handlers/CreateCaseHandlerTests.cs` | Test Class | Verifies handler creates and persists a case record. |
| `Handlers/ListKnowledgeHandlerTests.cs` | Test Class | Verifies handler returns all entries from repository. |

### `Axiom.Integration.Tests`

| File | Type | Summary |
|------|------|---------|
| `JsonRepositoryTests.cs` | Test Class | 4 integration tests for JSON repository round-trip, search, and delete operations. |

## CLI Usage

```bash
# Knowledge commands
dotnet run -- knowledge create --system "<guid>" --title "<str>" --content "<str>" [--description] [--tags] [--type]
dotnet run -- knowledge list
dotnet run -- knowledge show <guid>
dotnet run -- knowledge search <query>

# Case commands
dotnet run -- case create --system "<guid>" --problem "<str>" [--title] [--analysis] [--resolution] [--lessons] [--ritm-id] [--change-id]
dotnet run -- case show <guid>
```

## Build & Test

```bash
dotnet build               # Build all projects
dotnet test                # Run all tests (26 tests)
```
