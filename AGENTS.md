# Axiom — Agent Guide

## Project structure (Clean Architecture)

```
Axiom.Domain/        — Entities, Enums, ValueObjects, Exceptions. Zero dependencies.
Axiom.Application/   — CQRS (MediatR), Interfaces, Validators (FluentValidation). Depends on Domain.
Axiom.Infrastructure/— Persistence (EF Core SQL Server + JSON files), Data, Migrations. Depends on Application + Domain.
Axiom.Cli/           — Entrypoint (exe), System.CommandLine + Spectre.Console + MediatR.
tests/               — *Domain.Tests, *Application.Tests, *Integration.Tests (xUnit).
data/                — Default JSON file storage: cases.json, knowledge.json.
```

## Stack

- **.NET 10** (`net10.0`) — no `global.json`, no `Directory.Build.props`
- MediatR 14, FluentValidation 12, EF Core 10 + SQL Server
- CLI: System.CommandLine 2 + Spectre.Console 0.57
- Tests: xUnit + FluentAssertions + NSubstitute (mocks) + Coverlet (coverage)

## Build & test (from repo root)

```bash
dotnet build               # build all projects
dotnet test                # run all tests
dotnet test tests/Axiom.Application.Tests  # single test project
dotnet test --filter "KnowledgeRepository_ShouldRoundTripEntry"  # single test
```

No special order required. Tests are self-contained (no external services needed).

## CLI commands

```
dotnet run -- knowledge create --system-id <guid> --title <str> --content <str> [--description] [--tags] [--type]
dotnet run -- knowledge list
dotnet run -- knowledge show <guid>
dotnet run -- knowledge search <query>
dotnet run -- case create --system-id <guid> --problem <str> [--title] [--analysis] [--resolution] [--lessons] [--ritm-id] [--change-id]
dotnet run -- case show <guid>
```

`--type` accepts enum names: `Procedure`, `Incident`, `LessonLearned`, `Change`, `Troubleshooting`.

## Dual persistence

- **Default**: JSON files at `data/knowledge.json` and `data/cases.json` (paths from `JsonDataOptions` config section `"JsonData"`).
- **Opt-in EF Core**: call `AddInfrastructureEF(connectionString)` (available but not wired by default). Design-time factory reads `AXIOM_CONNECTION_STRING` env var (fallback: `Server=localhost;Database=AXIOM;...`).
- EF Migrations exist at `src/Axiom.Infrastructure/Persistence/Migrations/`.

## Domain conventions

- Private setters + `[JsonConstructor]` for entity deserialization.
- Value objects: `readonly record struct` with custom `JsonConverter` (`RitmId`, `ChangeId`, `SystemName`, `KnowledgeStatusValue`).
- Soft-delete via `DeletedAt` column + `HasQueryFilter(x => x.DeletedAt == null)`.
- Enums stored as `byte` (`tinyint` in SQL Server).
- EF uses `ValueConverter` for knowledge status; other value objects stored as strings in JSON repos, as strings in EF via converters.

## Testing conventions

- JSON repository integration tests use `Path.GetTempPath()` temp dirs with `IDisposable` cleanup (no fixtures needed).
- Application tests mock `IKnowledgeRepository` / `ICaseRepository` with NSubstitute.
- No `appsettings.json` or environment setup required for tests.
- Coverage: `coverlet.collector` (already referenced, run with `dotnet test --collect:"XPlat Code Coverage"`).
