using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Axiom.Infrastructure.Persistence;

public class AxiomDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AxiomDbContext>
{
    public AxiomDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AXIOM_CONNECTION_STRING")
            ?? "Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AxiomDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AxiomDbContext(optionsBuilder.Options);
    }
}
