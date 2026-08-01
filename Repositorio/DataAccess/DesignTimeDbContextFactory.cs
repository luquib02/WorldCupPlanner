using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Repositorio.DataAccess;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WorldCupPlannerDbContext>
{
    public WorldCupPlannerDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<WorldCupPlannerDbContext> options =
            new DbContextOptionsBuilder<WorldCupPlannerDbContext>()
                .UseSqlServer(
                    "Server=localhost,1433;Database=WorldCupPlanner;User Id=sa;Password=Mundial2026!;TrustServerCertificate=True;")
                .Options;

        return new WorldCupPlannerDbContext(options);
    }
}