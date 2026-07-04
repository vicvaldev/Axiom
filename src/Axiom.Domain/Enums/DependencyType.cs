namespace Axiom.Domain.Enums;

public enum DependencyType
{
    ReadsFrom,
    WritesTo,
    Calls,
    Populates,
    Triggers,
    Consumes,
    Produces,
    Imports,
    Exports,
    Schedules,
    AuthenticatesAgainst,
    SynchronizesWith,
    Unknown
}
