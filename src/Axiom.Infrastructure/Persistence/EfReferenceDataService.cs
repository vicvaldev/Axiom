using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Servicio de datos de referencia para tablas catálogo (usuarios, sistemas,
/// tipos de conocimiento, estados de conocimiento y estados de incidencia).
/// Todas las consultas se realizan sin seguimiento de cambios (<c>AsNoTracking</c>)
/// y retornan objetos DTO en lugar de entidades de dominio.
/// </summary>
public class EfReferenceDataService : IReferenceDataService
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del servicio con el contexto de base de datos especificado.
    /// </summary>
    /// <param name="context">Contexto de Entity Framework Core que expone las tablas del esquema Axiom.</param>
    public EfReferenceDataService(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la lista completa de usuarios ordenados por correo electrónico.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Lista de objetos <see cref="UserDto"/> con identificador, correo y nombre.</returns>
    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                Name = u.Name
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// Obtiene la lista completa de sistemas ordenados por código EAI,
    /// incluyendo el nombre del propietario.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Lista de objetos <see cref="SystemDto"/> con identificador, EAI, nombre y propietario.</returns>
    public async Task<IReadOnlyList<SystemDto>> ListSystemsAsync(CancellationToken ct = default)
    {
        return await _context.Systems
            .AsNoTracking()
            .Include(s => s.Owner)
            .OrderBy(s => s.EAI)
            .Select(s => new SystemDto
            {
                SystemId = s.SystemId,
                EAI = s.EAI,
                Name = s.Name,
                OwnerUserId = s.OwnerUserId,
                OwnerName = s.Owner.Name
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// Obtiene la lista de tipos de conocimiento ordenados por código.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Lista de objetos <see cref="ReferenceCodeDto"/> con identificador, código y nombre.</returns>
    public async Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeTypesAsync(CancellationToken ct = default)
    {
        return await _context.KnowledgeTypes
            .AsNoTracking()
            .OrderBy(t => t.Code)
            .Select(t => new ReferenceCodeDto
            {
                Id = t.TypeId,
                Code = t.Code,
                Name = t.Name
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// Obtiene la lista de estados de conocimiento ordenados por código.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Lista de objetos <see cref="ReferenceCodeDto"/> con identificador, código y nombre.</returns>
    public async Task<IReadOnlyList<ReferenceCodeDto>> ListKnowledgeStatesAsync(CancellationToken ct = default)
    {
        return await _context.KnowledgeStates
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// Obtiene la lista de estados de incidencia ordenados por código.
    /// </summary>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Lista de objetos <see cref="ReferenceCodeDto"/> con identificador, código y nombre.</returns>
    public async Task<IReadOnlyList<ReferenceCodeDto>> ListIssueStatesAsync(CancellationToken ct = default)
    {
        return await _context.IssueStates
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// Busca un usuario por su dirección de correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario a buscar (coincidencia exacta).</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="UserDto"/> si existe un usuario con el correo especificado; <c>null</c> en caso contrario.
    /// </returns>
    public async Task<UserDto?> FindUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                Name = u.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Busca un sistema por su código EAI, incluyendo el nombre del propietario.
    /// </summary>
    /// <param name="eai">Código EAI del sistema a buscar (coincidencia exacta).</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="SystemDto"/> si existe un sistema con el EAI especificado; <c>null</c> en caso contrario.
    /// </returns>
    public async Task<SystemDto?> FindSystemByEaiAsync(string eai, CancellationToken ct = default)
    {
        return await _context.Systems
            .AsNoTracking()
            .Include(s => s.Owner)
            .Where(s => s.EAI == eai)
            .Select(s => new SystemDto
            {
                SystemId = s.SystemId,
                EAI = s.EAI,
                Name = s.Name,
                OwnerUserId = s.OwnerUserId,
                OwnerName = s.Owner.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Busca un tipo de conocimiento por su código.
    /// </summary>
    /// <param name="code">Código del tipo de conocimiento (ej. "RUNBOOK", "TROUBLESHOOTING").</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="ReferenceCodeDto"/> si existe un tipo con el código especificado; <c>null</c> en caso contrario.
    /// </returns>
    public async Task<ReferenceCodeDto?> FindKnowledgeTypeByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.KnowledgeTypes
            .AsNoTracking()
            .Where(t => t.Code == code)
            .Select(t => new ReferenceCodeDto
            {
                Id = t.TypeId,
                Code = t.Code,
                Name = t.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Busca un estado de conocimiento por su código.
    /// </summary>
    /// <param name="code">Código del estado de conocimiento (ej. "DRAFT", "PUBLISHED", "ARCHIVED").</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="ReferenceCodeDto"/> si existe un estado con el código especificado; <c>null</c> en caso contrario.
    /// </returns>
    public async Task<ReferenceCodeDto?> FindKnowledgeStateByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.KnowledgeStates
            .AsNoTracking()
            .Where(s => s.Code == code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Busca un estado de incidencia por su código.
    /// </summary>
    /// <param name="code">Código del estado de incidencia (ej. "OPEN", "IN_PROGRESS", "RESOLVED", "CLOSED").</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="ReferenceCodeDto"/> si existe un estado con el código especificado; <c>null</c> en caso contrario.
    /// </returns>
    public async Task<ReferenceCodeDto?> FindIssueStateByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.IssueStates
            .AsNoTracking()
            .Where(s => s.Code == code)
            .Select(s => new ReferenceCodeDto
            {
                Id = s.StateId,
                Code = s.Code,
                Name = s.Name
            })
            .FirstOrDefaultAsync(ct);
    }
}
