using Axiom.Application.Interfaces;

namespace Axiom.Cli.Helpers;

internal static class Resolvers
{
    public static long? ResolveSystemId(long id, string? eai, IReferenceDataService references, bool json)
    {
        if (id > 0 && !string.IsNullOrWhiteSpace(eai))
        {
            CliOutput.WriteError("Use either --system-id or --system-eai, not both.", json);
            return null;
        }

        if (id > 0)
            return id;

        if (string.IsNullOrWhiteSpace(eai))
        {
            CliOutput.WriteError("A system reference is required. Use --system-id or --system-eai.", json);
            return null;
        }

        var system = references.FindSystemByEaiAsync(eai).Result;
        if (system is null)
        {
            CliOutput.WriteError($"System EAI not found: {eai}", json);
            return null;
        }

        return system.SystemId;
    }

    public static long? ResolveKnowledgeTypeId(long id, string? code, IReferenceDataService references, bool json)
    {
        if (id > 0 && !string.IsNullOrWhiteSpace(code))
        {
            CliOutput.WriteError("Use either --type-id or --type-code, not both.", json);
            return null;
        }

        if (id > 0)
            return id;

        if (string.IsNullOrWhiteSpace(code))
        {
            CliOutput.WriteError("A knowledge type reference is required. Use --type-id or --type-code.", json);
            return null;
        }

        var type = references.FindKnowledgeTypeByCodeAsync(code).Result;
        if (type is null)
        {
            CliOutput.WriteError($"Knowledge type code not found: {code}", json);
            return null;
        }

        return type.Id;
    }

    public static int? ResolveKnowledgeStateId(int id, string? code, IReferenceDataService references, bool json)
    {
        if (id > 0 && !string.IsNullOrWhiteSpace(code))
        {
            CliOutput.WriteError("Use either --state-id or --state-code, not both.", json);
            return null;
        }

        if (id > 0)
            return id;

        if (string.IsNullOrWhiteSpace(code))
        {
            CliOutput.WriteError("A knowledge state reference is required. Use --state-id or --state-code.", json);
            return null;
        }

        var state = references.FindKnowledgeStateByCodeAsync(code).Result;
        if (state is null)
        {
            CliOutput.WriteError($"Knowledge state code not found: {code}", json);
            return null;
        }

        return (int)state.Id;
    }

    public static int? ResolveIssueStateId(int id, string? code, IReferenceDataService references, bool json)
    {
        if (id > 0 && !string.IsNullOrWhiteSpace(code))
        {
            CliOutput.WriteError("Use either --state-id or --state-code, not both.", json);
            return null;
        }

        if (id > 0)
            return id;

        if (string.IsNullOrWhiteSpace(code))
        {
            CliOutput.WriteError("An issue state reference is required. Use --state-id or --state-code.", json);
            return null;
        }

        var state = references.FindIssueStateByCodeAsync(code).Result;
        if (state is null)
        {
            CliOutput.WriteError($"Issue state code not found: {code}", json);
            return null;
        }

        return (int)state.Id;
    }

    public static Guid? ResolveUserId(Guid id, string? email, IReferenceDataService references, bool json)
    {
        if (id != Guid.Empty && !string.IsNullOrWhiteSpace(email))
        {
            CliOutput.WriteError("Use either --created-by or --created-by-email, not both.", json);
            return null;
        }

        if (id != Guid.Empty)
            return id;

        if (string.IsNullOrWhiteSpace(email))
        {
            CliOutput.WriteError("A user reference is required. Use --created-by or --created-by-email.", json);
            return null;
        }

        var user = references.FindUserByEmailAsync(email).Result;
        if (user is null)
        {
            CliOutput.WriteError($"User email not found: {email}", json);
            return null;
        }

        return user.UserId;
    }
}
