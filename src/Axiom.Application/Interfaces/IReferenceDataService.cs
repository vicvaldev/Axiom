using Axiom.Application.Dtos;

namespace Axiom.Application.Interfaces;

public interface IReferenceDataService
{
    Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SystemDto>> ListSystemsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeStatesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReferenceCodeDto>> ListIssueStatesAsync(CancellationToken ct = default);
    Task<UserDto?> FindUserByEmailAsync(string email, CancellationToken ct = default);
    Task<SystemDto?> FindSystemByEaiAsync(string eai, CancellationToken ct = default);
    Task<ReferenceCodeDto?> FindKnowledgeTypeByCodeAsync(string code, CancellationToken ct = default);
    Task<ReferenceCodeDto?> FindKnowledgeStateByCodeAsync(string code, CancellationToken ct = default);
    Task<ReferenceCodeDto?> FindIssueStateByCodeAsync(string code, CancellationToken ct = default);
}
