using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Contexto de base de datos principal de la aplicación Axiom.
/// Expone los conjuntos de entidades (<see cref="DbSet{TEntity}"/>) para cada tabla del modelo
/// y aplica las configuraciones de asignación (Fluent API) descubiertas en el ensamblado de infraestructura.
/// </summary>
/// <remarks>
/// Este contexto se utiliza tanto en tiempo de ejecución (a través de la inyección de dependencias)
/// como en tiempo de diseño (mediante <see cref="AxiomDesignTimeDbContextFactory"/> para migraciones de EF Core).
/// Todas las consultas de solo lectura deben usar <c>AsNoTracking()</c> para mejorar el rendimiento.
/// Las navegaciones entre entidades se cargan explícitamente con <c>Include</c> / <c>ThenInclude</c>;
/// no se emplea carga diferida (<c>LazyLoading</c>) ni propiedades virtuales.
/// </remarks>
public class AxiomDbContext : DbContext
{
    /// <summary>
    /// Conjunto de entidades de usuarios del sistema.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Conjunto de entidades de sistemas registrados en la plataforma.
    /// </summary>
    public DbSet<AxiomSystem> Systems => Set<AxiomSystem>();

    /// <summary>
    /// Conjunto de entidades de etiquetas asociables a conocimientos.
    /// </summary>
    public DbSet<KnowledgeTag> KnowledgeTags => Set<KnowledgeTag>();

    /// <summary>
    /// Conjunto de entidades de tipos de conocimiento (catálogo).
    /// </summary>
    public DbSet<KnowledgeType> KnowledgeTypes => Set<KnowledgeType>();

    /// <summary>
    /// Conjunto de entidades de estados posibles para incidencias (catálogo).
    /// </summary>
    public DbSet<IssueState> IssueStates => Set<IssueState>();

    /// <summary>
    /// Conjunto de entidades de estados posibles para conocimientos (catálogo).
    /// </summary>
    public DbSet<KnowledgeState> KnowledgeStates => Set<KnowledgeState>();

    /// <summary>
    /// Conjunto de entidades de conocimientos (artículos, guías, documentación).
    /// </summary>
    public DbSet<Knowledge> Knowledges => Set<Knowledge>();

    /// <summary>
    /// Conjunto de entidades de incidencias (tickets, problemas, solicitudes).
    /// </summary>
    public DbSet<Issue> Issues => Set<Issue>();

    /// <summary>
    /// Conjunto de entidades de la tabla intermedia muchos-a-muchos entre conocimientos y etiquetas.
    /// </summary>
    public DbSet<KnowledgeKnowledgeTag> KnowledgeKnowledgeTags => Set<KnowledgeKnowledgeTag>();

    /// <summary>
    /// Conjunto de entidades de componentes técnicos asociados a sistemas.
    /// </summary>
    public DbSet<TechnicalComponent> TechnicalComponents => Set<TechnicalComponent>();

    /// <summary>
    /// Conjunto de entidades de asociaciones muchos-a-muchos entre sistemas y componentes técnicos.
    /// </summary>
    public DbSet<SystemComponent> SystemComponents => Set<SystemComponent>();

    /// <summary>
    /// Conjunto de entidades de dependencias dirigidas entre componentes técnicos.
    /// </summary>
    public DbSet<ComponentDependency> ComponentDependencies => Set<ComponentDependency>();

    /// <summary>
    /// Conjunto de entidades de eventos de trazabilidad de dependencias.
    /// </summary>
    public DbSet<DependencyTraceEvent> DependencyTraceEvents => Set<DependencyTraceEvent>();

    /// <summary>
    /// Inicializa una nueva instancia del <see cref="AxiomDbContext"/> con las opciones de configuración especificadas.
    /// </summary>
    /// <param name="options">Opciones de configuración del contexto, incluyendo la cadena de conexión y proveedor de base de datos.</param>
    public AxiomDbContext(DbContextOptions<AxiomDbContext> options) : base(options) { }

    /// <summary>
    /// Configura el modelo de entidades aplicando todas las implementaciones de <see cref="IEntityTypeConfiguration{TEntity}"/>
    /// contenidas en el ensamblado de infraestructura.
    /// </summary>
    /// <param name="modelBuilder">Instancia del <see cref="ModelBuilder"/> utilizada para configurar las entidades.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AxiomDbContext).Assembly);
    }
}
