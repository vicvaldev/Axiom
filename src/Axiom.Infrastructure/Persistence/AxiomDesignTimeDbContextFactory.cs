using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Fábrica de contexto de base de datos utilizada exclusivamente en tiempo de diseño por las herramientas
/// de EF Core (migraciones, scaffolding, etc.).
/// Implementa <see cref="IDesignTimeDbContextFactory{TContext}"/> para permitir que comandos como
/// <c>dotnet ef migrations add</c> o <c>dotnet ef database update</c> creen una instancia de
/// <see cref="AxiomDbContext"/> sin necesidad de que la aplicación esté en ejecución.
/// </summary>
/// <remarks>
/// La cadena de conexión se lee desde <c>appsettings.json</c> utilizando la clave
/// <c>ConnectionStrings:Axiom</c>. Si el archivo o la clave no existen, se produce un error
/// intencional para forzar la configuración explícita.
/// </remarks>
public class AxiomDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AxiomDbContext>
{
    /// <summary>
    /// Crea una nueva instancia de <see cref="AxiomDbContext"/> configurada para SQL Server,
    /// leyendo la cadena de conexión desde <c>appsettings.json</c>.
    /// </summary>
    /// <param name="args">
    /// Argumentos de línea de comandos proporcionados por la herramienta de EF Core.
    /// No se utilizan en esta implementación; se incluyen para cumplir con el contrato de la interfaz.
    /// </param>
    /// <returns>Una instancia configurada de <see cref="AxiomDbContext"/> lista para usar.</returns>
    public AxiomDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Axiom")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'Axiom' en appsettings.json. " +
                "Agrega la sección ConnectionStrings:Axiom.");

        var optionsBuilder = new DbContextOptionsBuilder<AxiomDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AxiomDbContext(optionsBuilder.Options);
    }
}
