# Axiom Documentation Index

## Project Overview

Axiom is a KnowledgeOps and Operational Continuity Platform built with Clean Architecture in .NET 10. It provides CLI-driven management of knowledge entries, case records, technical components, and component dependencies with dual persistence (JSON files / EF Core SQL Server).

## Axiom.Domain

### Namespace: `Axiom.Domain.Enums`

| Type | Kind | Summary |
|------|------|---------|
| `ComponentType` | Enum | Database, Schema, Table, View, StoredProcedure, Api, ApiEndpoint, ETL, SchedulerJob, AdobeFlow, Queue, File, Service, ExternalSystem, Unknown |
| `TargetEnvironment` | Enum | DEV, QA, UAT, PROD, Unknown |
| `Criticality` | Enum | Low, Medium, High, Critical, Unknown |
| `DependencyType` | Enum | ReadsFrom, WritesTo, Calls, Populates, Triggers, Consumes, Produces, Imports, Exports, Schedules, AuthenticatesAgainst, SynchronizesWith, Unknown |
| `DependencyStatus` | Enum | Active, Deprecated, Disabled, Unknown |
| `DependencyTraceEventType` | Enum | Created, Updated, Validated, Failed, Deprecated, IssueLinked, KnowledgeLinked, RitmLinked, ChangeLinked, NoteAdded, Unknown |

### Namespace: `Axiom.Domain.Entities`

| Type | Kind | Summary |
|------|------|---------|
| `TechnicalComponent` | Class | Componente técnico con Name, TechnicalName (único), ComponentType, Environment, Criticality, Description. FK → System. |
| `SystemComponent` | Class | Join table many-to-many entre System y TechnicalComponent. Unique index (SystemId, ComponentId). |
| `ComponentDependency` | Class | Dependencia dirigida entre componentes. SourceComponent → TargetComponent con DependencyType, Criticality, Status. |
| `DependencyTraceEvent` | Class | Evento de trazabilidad inmutable. FK → ComponentDependency con cascade delete. |

### Namespace: `Axiom.Domain.Exceptions`

| Type | Kind | Summary |
|------|------|---------|
| `DomainException` | Abstract Class | Base exception class for domain-level exceptions. |

## Axiom.Application

### Namespace: `Axiom.Application.Interfaces`

| Type | Kind | Summary |
|------|------|---------|
| `ITechnicalComponentRepository` | Interface | SaveAsync, GetByIdAsync, GetBySystemIdAsync, GetAllAsync, DeleteAsync |
| `IComponentDependencyRepository` | Interface | SaveAsync, GetByIdAsync, GetByComponentIdAsync, GetImpactedByComponentAsync, DeleteAsync |
| `IDependencyTraceEventRepository` | Interface | AddAsync, GetByDependencyIdAsync |
| `ISystemComponentRepository` | Interface | SaveAsync |

### Namespace: `Axiom.Application.Commands`

| Type | Kind | Summary |
|------|------|---------|
| `CreateTechnicalComponentCommand` | Record | Creates a new TechnicalComponent (MediatR IRequest\<TechnicalComponent\>) |
| `CreateSystemComponentCommand` | Record | Creates a new SystemComponent join entry |
| `CreateComponentDependencyCommand` | Record | Creates a new ComponentDependency |
| `CreateDependencyTraceEventCommand` | Record | Creates a new DependencyTraceEvent |

### Namespace: `Axiom.Application.Queries`

| Type | Kind | Summary |
|------|------|---------|
| `ListComponentsBySystemQuery` | Record | Returns all TechnicalComponents for a given SystemId |
| `GetComponentByIdQuery` | Record | Returns a single TechnicalComponentDto by ComponentId |
| `ListDependenciesByComponentQuery` | Record | Returns all outgoing dependencies of a component |
| `ListImpactedComponentsQuery` | Record | Returns all incoming dependencies (impact) on a component |

### Namespace: `Axiom.Application.Handlers`

| Type | Kind | Summary |
|------|------|---------|
| `CreateTechnicalComponentHandler` | Class | Handles CreateTechnicalComponentCommand |
| `CreateSystemComponentHandler` | Class | Handles CreateSystemComponentCommand |
| `CreateComponentDependencyHandler` | Class | Handles CreateComponentDependencyCommand |
| `CreateDependencyTraceEventHandler` | Class | Handles CreateDependencyTraceEventCommand |
| `ListComponentsBySystemHandler` | Class | Handles ListComponentsBySystemQuery |
| `GetComponentByIdHandler` | Class | Handles GetComponentByIdQuery |
| `ListDependenciesByComponentHandler` | Class | Handles ListDependenciesByComponentQuery |
| `ListImpactedComponentsHandler` | Class | Handles ListImpactedComponentsQuery |

### Namespace: `Axiom.Application.Validators`

| Type | Kind | Summary |
|------|------|---------|
| `CreateTechnicalComponentValidator` | Class | Name NotEmpty MaxLength(200), TechnicalName NotEmpty MaxLength(100), SystemId > 0 |
| `CreateComponentDependencyValidator` | Class | SourceComponentId not empty, TargetComponentId not empty, Source ≠ Target |
| `CreateDependencyTraceEventValidator` | Class | DependencyId not empty, Description NotEmpty |
| `CreateSystemComponentValidator` | Class | SystemId > 0, ComponentId not empty |

### Namespace: `Axiom.Application`

| Type | Kind | Summary |
|------|------|---------|
| `DependencyInjection` | Static Class | Extension method `AddApplication()` to register MediatR and FluentValidation. |

## Axiom.Infrastructure

### Namespace: `Axiom.Infrastructure.Persistence`

| Type | Kind | Summary |
|------|------|---------|
| `EfTechnicalComponentRepository` | Class | EF Core implementation of ITechnicalComponentRepository |
| `EfSystemComponentRepository` | Class | EF Core implementation of ISystemComponentRepository |
| `EfComponentDependencyRepository` | Class | EF Core implementation of IComponentDependencyRepository |
| `EfDependencyTraceEventRepository` | Class | EF Core implementation of IDependencyTraceEventRepository |

## Axiom.Cli

### File: `Program.cs`

| Section | Summary |
|---------|---------|
| Root | Axiom CLI entry point with `--help` support. |
| `component add` | Creates a new technical component (--name, --technical-name, --type, --environment, --criticality, --system-id, --description) |
| `component list` | Lists components by system (--system-id) |
| `component show <id>` | Shows detailed info for a component |
| `dependency add` | Creates a new dependency (--source, --target, --type, --criticality, --status, --description) |
| `dependency list` | Lists outgoing dependencies (--component) |
| `dependency impact` | Lists incoming dependencies (--component) |
| `dependency trace` | Registers a trace event (--dependency, --event-type, --description, optional metadata) |

## Tests

### `Axiom.Domain.Tests`

| File | Type | Summary |
|------|------|---------|
| `TechnicalComponentTests.cs` | Test Class | 9 tests covering constructor validation, defaults, and Update method |
| `SystemComponentTests.cs` | Test Class | 6 tests covering system-component association |
| `ComponentDependencyTests.cs` | Test Class | 6 tests covering dependency creation and validation |
| `DependencyTraceEventTests.cs` | Test Class | 5 tests covering trace event creation and immutability |

### `Axiom.Application.Tests`

| File | Type | Summary |
|------|------|---------|
| `CreateTechnicalComponentHandlerTests.cs` | Test Class | Verifies handler creates and persists a component |
| `CreateComponentDependencyHandlerTests.cs` | Test Class | Verifies handler creates and persists a dependency |
| `CreateDependencyTraceEventHandlerTests.cs` | Test Class | Verifies handler creates and persists a trace event |
| `ListComponentsBySystemHandlerTests.cs` | Test Class | Verifies handler returns components by system |
| `ListDependenciesByComponentHandlerTests.cs` | Test Class | Verifies handler returns dependencies by component |
| `ListImpactedComponentsHandlerTests.cs` | Test Class | Verifies handler returns impacted components |

### `Axiom.Integration.Tests`

| File | Type | Summary |
|------|------|---------|
| `EfTechnicalComponentRepositoryTests.cs` | Test Class | 5 e2e tests for CRUD + GetBySystemId + GetAll |
| `EfComponentDependencyRepositoryTests.cs` | Test Class | 6 e2e tests for CRUD + GetByComponentId + GetImpactedByComponent |
| `EfDependencyTraceEventRepositoryTests.cs` | Test Class | 3 e2e tests for Add + list ordered + empty result |

## Build & Test

```bash
dotnet build               # Build all projects
dotnet test                # Run all tests (61 tests)
```
