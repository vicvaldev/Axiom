# Axiom — Agent Guide

## Project structure (Clean Architecture)

```
Axiom.Domain/        — Entities, ValueObjects, Exceptions. Zero dependencies.
Axiom.Application/   — CQRS (MediatR), DTOs, Interfaces, Validators (FluentValidation). Depends on Domain.
Axiom.Infrastructure/— Persistence (EF Core SQL Server), Configurations, Migrations. Depends on Application + Domain.
Axiom.Cli/           — Entrypoint (exe), System.CommandLine + Spectre.Console + MediatR.
tests/               — *Domain.Tests, *Application.Tests, *Integration.Tests (xUnit).
data/                — diagram.md, tables.md (documentation only).
```

## Stack

- **.NET 10** (`net10.0`) — no `global.json`, no `Directory.Build.props`
- MediatR 12.5, FluentValidation 12, EF Core 10 + SQL Server
- CLI: System.CommandLine 2 + Spectre.Console 0.57
- Tests: xUnit + FluentAssertions + NSubstitute (mocks) + Coverlet (coverage)

## Database model (9 entities)

| Table | PK | Notes |
|-------|----|-------|
| `Users` | `UserId` (GUID) | `Email` (unique), `Name` |
| `Systems` | `SystemId` (long, identity) | `EAI`(20), `Name`(200), FK→Users |
| `KnowledgeTags` | `KnowledgeTagId` (long, identity) | `TagName`(100, unique) |
| `KnowledgeTypes` | `TypeId` (long, identity) | `Code`(unique), `Name`(200) |
| `IssueStates` | `StateId` (int, identity) | `Code`(unique), `Name`(200) |
| `KnowledgeStates` | `StateId` (int, identity) | `Code`(unique), `Name`(200) |
| `Knowledges` | `KnowledgeId` (GUID) | FKs→ Systems, Users, KnowledgeTypes, KnowledgeStates, Issues (nullable) |
| `Issues` | `IssueId` (GUID) | FKs→ Systems, IssueStates, Users; `RitmNumber`/`IncidentNumber` (unique nullable) |
| `KnowledgeKnowledgeTags` | Compuesta (KnowledgeId, KnowledgeTagId) | Many-to-many join |

## Build & test (from repo root)

```bash
dotnet build               # build all projects
dotnet test                # run all tests
dotnet test tests/Axiom.Application.Tests  # single test project
dotnet test --filter "ShouldRoundTripEntry"  # single test
```

No special order required. Tests are self-contained (no external services needed).

## CLI commands

```
dotnet run -- knowledge create --system-id <long> --title <str> --content <str> [--summary] [--type-id] [--state-id] [--tags <t1,t2>] [--issue-id]
dotnet run -- knowledge list
dotnet run -- knowledge show <guid>
dotnet run -- knowledge search <query>

dotnet run -- issue create --system-id <long> --summary <str> --problem <str> [--analysis] [--resolution] [--ritm-number] [--incident-number] [--state-id]
dotnet run -- issue list
dotnet run -- issue show <guid>
```

## Persistence

- **EF Core only** (SQL Server via `AddInfrastructure(connectionString)`).
- Design-time factory reads `AXIOM_CONNECTION_STRING` env var (fallback: `Server=localhost;Database=AXIOM;...`).
- Migrations at `src/Axiom.Infrastructure/Persistence/Migrations/`.

## Domain conventions

- Private setters with domain constructors (validation in ctors).
- Navigation properties: explicit `.Include()` / `.ThenInclude()` — no lazy loading, no virtual.
- Collections: `HashSet<T>` initialized with `= new HashSet<T>()`.
- All queries use `AsNoTracking()` for read operations.
- DTO projections for list/search responses (no navigation properties exposed).
- Nullable FKs (`IssueId`, `RitmNumber`, `IncidentNumber`, `ResolvedAt`) allowed where applicable.
- FK delete behavior: `Restrict` for most, `Cascade` for join table, `SetNull` for Knowledge→Issue.

## Testing conventions

- Application tests mock `IKnowledgeRepository`, `IIssueRepository`, `ITagRepository` with NSubstitute.
- Integration tests use EF Core InMemory provider with seeded reference data (User, System, KnowledgeType, KnowledgeState, IssueState).
- No `appsettings.json` or environment setup required for tests.
- Coverage: `coverlet.collector` (already referenced, run with `dotnet test --collect:"XPlat Code Coverage"`).

## Git

Tienes permiso para ejecutar comandos de git para commits, push y
auditoría. **NO puedes ejecutar `git reset`** (ninguna variante).

### Flujo para commits

1. `git status` + `git diff` + `git log --oneline -10` para inspeccionar.
2. `git add <files>` para stagear solo los archivos intencionados.
3. `git commit -m "mensaje"` con mensaje descriptivo y conciso acorde al
   estilo del repo.
4. `git push` cuando se solicite.

### Herramientas de auditoría permitidas

`git log`, `git reflog`, `git blame`, `git diff`, `git show`, `git status`.

### Restricciones

- No hacer `git reset` (ninguna variante: `--soft`, `--hard`, `--mixed`,
  `HEAD~N`, etc.).
- No hacer `git push --force` ni `git push -f`.
- No modificar config de git, no saltar hooks, no usar `-i`.
- No crear commits vacíos.
- Solo commit/push cuando el usuario lo solicite explícitamente.
