using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Fábrica de contexto de base de datos utilizada exclusivamente en tiempo de diseño por las herramientas
/// de EF Core (migraciones, scaffolding, etc.).
/// Implementa <see cref="IDesignTimeDbContextFactory{TContext}"/> para permitir que comandos como
/// <c>dotnet ef migrations add</c> o <c>dotnet ef database update</c> creen una instancia de
/// <see cref="AxiomDbContext"/> sin necesidad de que la aplicación esté en ejecución.
/// </summary>
/// <remarks>
/// La cadena de conexión se obtiene de la variable de entorno <c>AXIOM_CONNECTION_STRING</c>.
/// Si la variable no está definida, se utiliza una cadena de conexión por defecto que apunta a
/// una instancia local de SQL Server con la base de datos <c>AXIOM</c> y autenticación integrada.
/// </remarks>
public class AxiomDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AxiomDbContext>
{
    /// <summary>
    /// Crea una nueva instancia de <see cref="AxiomDbContext"/> configurada para SQL Server,
    /// leyendo la cadena de conexión desde la variable de entorno <c>AXIOM_CONNECTION_STRING</c>
    /// o utilizando un valor predeterminado local.
    /// </summary>
    /// <param name="args">
    /// Argumentos de línea de comandos proporcionados por la herramienta de EF Core.
    /// No se utilizan en esta implementación; se incluyen para cumplir con el contrato de la interfaz.
    /// </param>
    /// <returns>Una instancia configurada de <see cref="AxiomDbContext"/> lista para usar.</returns>
    public AxiomDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AXIOM_CONNECTION_STRING")
            ?? "Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AxiomDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AxiomDbContext(optionsBuilder.Options);
    }
}
