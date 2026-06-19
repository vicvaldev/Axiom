using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfStartupService : IStartupService
{
    private readonly AxiomDbContext _context;

    public EfStartupService(AxiomDbContext context)
    {
        _context = context;
    }

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
