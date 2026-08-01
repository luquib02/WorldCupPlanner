using Microsoft.EntityFrameworkCore;

namespace Repositorio.DataAccess;

public class InMemoryDbContextFactory
{
    public const string NombreBaseDeDatos = "WorldCupPlannerTestingDB";

    public WorldCupPlannerDbContext CreateDbContext()
    {
        DbContextOptionsBuilder<WorldCupPlannerDbContext> optionsBuilder =
            new DbContextOptionsBuilder<WorldCupPlannerDbContext>();

        optionsBuilder.UseInMemoryDatabase(NombreBaseDeDatos);

        return new WorldCupPlannerDbContext(optionsBuilder.Options);
    }
}