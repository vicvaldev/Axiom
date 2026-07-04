using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Servicio de inicialización y siembra de datos de demostración para la aplicación Axiom.
/// Implementa el patrón "buscar o crear" (find-or-create) para evitar duplicados
/// en la creación de usuarios, sistemas, tipos de conocimiento, estados y datos de demo.
/// </summary>
public class EfStartupService : IStartupService
{
    private readonly AxiomDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del servicio con el contexto de base de datos especificado.
    /// </summary>
    /// <param name="context">Contexto de Entity Framework Core que expone las tablas del esquema Axiom.</param>
    public EfStartupService(AxiomDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea un nuevo usuario o retorna el existente si ya hay uno con el mismo correo electrónico.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico única del usuario.</param>
    /// <param name="name">Nombre completo del usuario.</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// La entidad <see cref="User"/> recién creada o la existente si ya estaba registrada.
    /// </returns>
    public async Task<User> CreateUserAsync(string email, string name, CancellationToken ct)
    {
        var existing = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null)
        {
            return existing;
        }

        var user = new User(email, name);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    /// <summary>
    /// Crea un nuevo sistema o retorna el existente si ya hay uno con el mismo código EAI.
    /// </summary>
    /// <param name="eai">Código EAI único del sistema (máximo 20 caracteres).</param>
    /// <param name="name">Nombre descriptivo del sistema (máximo 200 caracteres).</param>
    /// <param name="ownerUserId">Identificador del usuario propietario del sistema.</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// La entidad <see cref="AxiomSystem"/> recién creada o la existente si ya estaba registrada.
    /// </returns>
    public async Task<AxiomSystem> CreateSystemAsync(string eai, string name, Guid ownerUserId, CancellationToken ct)
    {
        var existing = await _context.Systems
            .FirstOrDefaultAsync(s => s.EAI == eai, ct);
        if (existing is not null)
        {
            return existing;
        }

        var system = new AxiomSystem(eai, name, ownerUserId);
        _context.Systems.Add(system);
        await _context.SaveChangesAsync(ct);
        return system;
    }

    /// <summary>
    /// Crea un nuevo tipo de conocimiento o retorna el existente si ya hay uno con el mismo código.
    /// </summary>
    /// <param name="code">Código único del tipo de conocimiento (ej. "RUNBOOK", "TROUBLESHOOTING").</param>
    /// <param name="name">Nombre descriptivo del tipo de conocimiento.</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// La entidad <see cref="KnowledgeType"/> recién creada o la existente si ya estaba registrada.
    /// </returns>
    public async Task<KnowledgeType> CreateKnowledgeTypeAsync(string code, string name, CancellationToken ct)
    {
        var existing = await _context.KnowledgeTypes
            .FirstOrDefaultAsync(t => t.Code == code, ct);
        if (existing is not null)
        {
            return existing;
        }

        var type = new KnowledgeType(code, name);
        _context.KnowledgeTypes.Add(type);
        await _context.SaveChangesAsync(ct);
        return type;
    }

    /// <summary>
    /// Crea un nuevo estado de incidencia o retorna el existente si ya hay uno con el mismo código.
    /// </summary>
    /// <param name="code">Código único del estado de incidencia (ej. "OPEN", "RESOLVED", "CLOSED").</param>
    /// <param name="name">Nombre descriptivo del estado de incidencia.</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// La entidad <see cref="IssueState"/> recién creada o la existente si ya estaba registrada.
    /// </returns>
    public async Task<IssueState> CreateIssueStateAsync(string code, string name, CancellationToken ct)
    {
        var existing = await _context.IssueStates
            .FirstOrDefaultAsync(s => s.Code == code, ct);
        if (existing is not null)
        {
            return existing;
        }

        var state = new IssueState(code, name);
        _context.IssueStates.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }

    /// <summary>
    /// Crea un nuevo estado de conocimiento o retorna el existente si ya hay uno con el mismo código.
    /// </summary>
    /// <param name="code">Código único del estado de conocimiento (ej. "DRAFT", "PUBLISHED", "ARCHIVED").</param>
    /// <param name="name">Nombre descriptivo del estado de conocimiento.</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// La entidad <see cref="KnowledgeState"/> recién creada o la existente si ya estaba registrada.
    /// </returns>
    public async Task<KnowledgeState> CreateKnowledgeStateAsync(string code, string name, CancellationToken ct)
    {
        var existing = await _context.KnowledgeStates
            .FirstOrDefaultAsync(s => s.Code == code, ct);
        if (existing is not null)
        {
            return existing;
        }

        var state = new KnowledgeState(code, name);
        _context.KnowledgeStates.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }

    /// <summary>
    /// Siembra datos de demostración en la base de datos: crea usuarios, sistemas,
    /// tipos de conocimiento, estados (incidencia y conocimiento), incidencias de ejemplo
    /// y entradas de conocimiento relacionadas.
    /// Todos los métodos internos usan la estrategia "buscar o crear" para garantizar
    /// la idempotencia de la operación.
    /// </summary>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// Un <see cref="DemoSeedResultDto"/> con el conteo de entidades creadas
    /// (usuarios, sistemas, tipos, estados, incidencias y conocimientos).
    /// </returns>
    public async Task<DemoSeedResultDto> SeedDemoDataAsync(CancellationToken ct)
    {
        var victor = await CreateUserAsync("victor.valdivia.dev@gmail.com", "Victor Valdivia", ct);
        var agent = await CreateUserAsync("ops.agent@axiom.local", "Axiom Ops Agent", ct);

        var portal = await CreateSystemAsync("EAI001", "Portal Clientes", victor.UserId, ct);
        await CreateSystemAsync("EAI002", "Gestor de Cartera", victor.UserId, ct);
        var pagos = await CreateSystemAsync("EAI003", "Integracion Pagos", agent.UserId, ct);

        var runbook = await CreateKnowledgeTypeAsync("RUNBOOK", "Runbook", ct);
        var troubleshooting = await CreateKnowledgeTypeAsync("TROUBLESHOOTING", "Troubleshooting", ct);
        var reference = await CreateKnowledgeTypeAsync("REFERENCE", "Referencia", ct);

        await CreateIssueStateAsync("OPEN", "Abierto", ct);
        await CreateIssueStateAsync("IN_PROGRESS", "En progreso", ct);
        var resolved = await CreateIssueStateAsync("RESOLVED", "Resuelto", ct);
        var closed = await CreateIssueStateAsync("CLOSED", "Cerrado", ct);

        await CreateKnowledgeStateAsync("DRAFT", "Borrador", ct);
        var published = await CreateKnowledgeStateAsync("PUBLISHED", "Publicado", ct);
        await CreateKnowledgeStateAsync("ARCHIVED", "Archivado", ct);

        var loginIssue = await FindOrCreateIssueAsync(
            "INC0001001",
            "RITM0002001",
            "Portal Clientes no permite iniciar sesion",
            portal.SystemId,
            "Usuarios reportan error 500 al autenticar contra Portal Clientes.",
            "El pool de conexiones hacia el proveedor de identidad quedo saturado despues de un despliegue.",
            "Se reciclo el servicio de autenticacion y se aumento temporalmente el limite de conexiones.",
            resolved.StateId,
            agent.UserId,
            ct);

        var pagosIssue = await FindOrCreateIssueAsync(
            "INC0001002",
            "RITM0002002",
            "Integracion Pagos con reintentos acumulados",
            pagos.SystemId,
            "Mensajes de conciliacion quedan en estado pendiente y se acumulan reintentos.",
            "El endpoint externo respondio timeout durante la ventana nocturna.",
            "Se reprocesaron los mensajes pendientes y se valido conciliacion con negocio.",
            closed.StateId,
            agent.UserId,
            ct);

        await FindOrCreateKnowledgeAsync(
            "Runbook reinicio controlado Portal Clientes",
            "Pasos operativos para reiniciar componentes del Portal Clientes sin afectar sesiones activas.",
            "1. Validar incidentes abiertos.\n2. Drenar trafico del nodo afectado.\n3. Reiniciar el servicio web.\n4. Verificar login con usuario de prueba.\n5. Retornar el nodo al balanceador.",
            portal.SystemId,
            agent.UserId,
            runbook.TypeId,
            published.StateId,
            loginIssue.IssueId,
            ["portal-clientes", "login", "runbook"],
            ct);

        await FindOrCreateKnowledgeAsync(
            "Troubleshooting error 500 en autenticacion",
            "Guia para diagnosticar errores 500 durante login en Portal Clientes.",
            "Revisar health checks del proveedor de identidad, saturacion del pool de conexiones y logs de autenticacion. Si el error coincide con timeouts recurrentes, reciclar el componente y levantar seguimiento con plataforma.",
            portal.SystemId,
            victor.UserId,
            troubleshooting.TypeId,
            published.StateId,
            loginIssue.IssueId,
            ["troubleshooting", "autenticacion", "incidente"],
            ct);

        await FindOrCreateKnowledgeAsync(
            "Referencia operativa Integracion Pagos",
            "Datos de soporte y criterios para reprocesar mensajes de pagos.",
            "Los mensajes pendientes se revisan por correlacion de negocio, estado tecnico y ventana de conciliacion. No reprocesar mensajes duplicados sin validacion previa con operaciones.",
            pagos.SystemId,
            agent.UserId,
            reference.TypeId,
            published.StateId,
            pagosIssue.IssueId,
            ["pagos", "conciliacion", "referencia"],
            ct);

        return new DemoSeedResultDto
        {
            Users = 2,
            Systems = 3,
            KnowledgeTypes = 3,
            IssueStates = 4,
            KnowledgeStates = 3,
            Issues = 2,
            Knowledges = 3
        };
    }

    /// <summary>
    /// Busca una incidencia por su número de incidente o número RITM; si no existe, la crea.
    /// </summary>
    /// <param name="incidentNumber">Número de incidente (único, nullable).</param>
    /// <param name="ritmNumber">Número RITM (único, nullable).</param>
    /// <param name="summary">Resumen descriptivo de la incidencia.</param>
    /// <param name="systemId">Identificador del sistema asociado.</param>
    /// <param name="problem">Descripción del problema reportado.</param>
    /// <param name="analysis">Análisis técnico o diagnóstico realizado.</param>
    /// <param name="resolution">Resolución o acción correctiva aplicada.</param>
    /// <param name="stateId">Identificador del estado de la incidencia.</param>
    /// <param name="createdByUserId">Identificador del usuario que crea la incidencia.</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>La entidad <see cref="Issue"/> existente o la recién creada.</returns>
    private async Task<Issue> FindOrCreateIssueAsync(
        string incidentNumber,
        string ritmNumber,
        string summary,
        long systemId,
        string problem,
        string analysis,
        string resolution,
        int stateId,
        Guid createdByUserId,
        CancellationToken ct)
    {
        var existing = await _context.Issues
            .FirstOrDefaultAsync(i => i.IncidentNumber == incidentNumber || i.RitmNumber == ritmNumber, ct);
        if (existing is not null)
        {
            return existing;
        }

        var issue = new Issue(summary, systemId, problem, stateId, createdByUserId, analysis, resolution, ritmNumber, incidentNumber);
        _context.Issues.Add(issue);
        await _context.SaveChangesAsync(ct);
        return issue;
    }

    /// <summary>
    /// Busca un conocimiento por su título; si no existe, lo crea junto con sus etiquetas asociadas.
    /// </summary>
    /// <param name="title">Título del conocimiento (se usa como criterio de unicidad).</param>
    /// <param name="summary">Resumen o descripción breve del conocimiento.</param>
    /// <param name="content">Contenido detallado del conocimiento.</param>
    /// <param name="systemId">Identificador del sistema asociado.</param>
    /// <param name="createdByUserId">Identificador del usuario creador.</param>
    /// <param name="knowledgeTypeId">Identificador del tipo de conocimiento.</param>
    /// <param name="knowledgeStateId">Identificador del estado de conocimiento.</param>
    /// <param name="issueId">Identificador opcional de la incidencia relacionada.</param>
    /// <param name="tags">Lista de nombres de etiquetas a asociar (se crean si no existen).</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>La entidad <see cref="Knowledge"/> existente o la recién creada.</returns>
    private async Task<Knowledge> FindOrCreateKnowledgeAsync(
        string title,
        string summary,
        string content,
        long systemId,
        Guid createdByUserId,
        long knowledgeTypeId,
        int knowledgeStateId,
        Guid? issueId,
        IReadOnlyList<string> tags,
        CancellationToken ct)
    {
        var existing = await _context.Knowledges
            .FirstOrDefaultAsync(k => k.Title == title, ct);
        if (existing is not null)
        {
            return existing;
        }

        var knowledge = new Knowledge(title, summary, content, systemId, createdByUserId, knowledgeTypeId, knowledgeStateId, issueId);
        foreach (var tagName in tags)
        {
            var tag = await FindOrCreateTagAsync(tagName, ct);
            knowledge.KnowledgeKnowledgeTags.Add(new KnowledgeKnowledgeTag(knowledge.KnowledgeId, tag.KnowledgeTagId));
        }

        _context.Knowledges.Add(knowledge);
        await _context.SaveChangesAsync(ct);
        return knowledge;
    }

    /// <summary>
    /// Busca una etiqueta por su nombre; si no existe, la crea.
    /// </summary>
    /// <param name="tagName">Nombre de la etiqueta (único).</param>
    /// <param name="ct">Token de cancelación para la operación asincrónica.</param>
    /// <returns>La entidad <see cref="KnowledgeTag"/> existente o la recién creada.</returns>
    private async Task<KnowledgeTag> FindOrCreateTagAsync(string tagName, CancellationToken ct)
    {
        var existing = await _context.KnowledgeTags
            .FirstOrDefaultAsync(t => t.TagName == tagName, ct);
        if (existing is not null)
        {
            return existing;
        }

        var tag = new KnowledgeTag(tagName);
        _context.KnowledgeTags.Add(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }
}
