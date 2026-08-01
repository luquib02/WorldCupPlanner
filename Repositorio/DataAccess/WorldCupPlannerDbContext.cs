using System.Reflection;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Repositorio.DataAccess;

public class WorldCupPlannerDbContext : DbContext
{
    public DbSet<Estadio> Estadios { get; set; } = null!;
    public DbSet<Partido> Partidos { get; set; } = null!;
    public DbSet<Equipo> Equipos { get; set; } = null!;
    public DbSet<LogEntry> Logs { get; set; } = null!;
    public DbSet<Incidencia> Incidencias { get; set; } = null!;
    public DbSet<Notificacion> Notificaciones { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    internal DbSet<UsuarioRolRecord> UsuarioRoles { get; set; } = null!;
    public DbSet<Fixture> Fixtures { get; set; } = null!;
    public DbSet<Grupo> Grupos { get; set; } = null!;
    public DbSet<Jornada> Jornadas { get; set; } = null!;

    public WorldCupPlannerDbContext(DbContextOptions<WorldCupPlannerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}