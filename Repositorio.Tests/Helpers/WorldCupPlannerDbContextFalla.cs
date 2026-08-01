using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio.Tests.Helpers;

public class WorldCupPlannerDbContextFalla : WorldCupPlannerDbContext
{
    public WorldCupPlannerDbContextFalla(DbContextOptions<WorldCupPlannerDbContext> options)
        : base(options)
    {
    }

    public override int SaveChanges()
    {
        throw new DbUpdateException("Error simulado al guardar cambios.");
    }

    public static WorldCupPlannerDbContextFalla Crear()
    {
        DbContextOptionsBuilder<WorldCupPlannerDbContext> optionsBuilder = new();
        optionsBuilder.UseInMemoryDatabase(InMemoryDbContextFactory.NombreBaseDeDatos);
        return new WorldCupPlannerDbContextFalla(optionsBuilder.Options);
    }
}
