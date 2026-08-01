using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class FixtureRepositorio : IFixtureRepositorio
{
    private readonly WorldCupPlannerDbContext _contexto;

    public FixtureRepositorio(WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Guardar(Fixture fixture)
    {
        if (fixture.Id == 0)
            _contexto.Fixtures.Add(fixture);
        _contexto.SaveChanges();
    }

    public Fixture? ObtenerActual()
    {
        return _contexto.Fixtures
            .AsSplitQuery()
            .Include(f => f.Grupos)
                .ThenInclude(g => g.Equipos)
            .Include(f => f.Grupos)
                .ThenInclude(g => g.Jornadas)
                    .ThenInclude(j => j.Partidos)
                        .ThenInclude(p => p.EquipoLocal)
            .Include(f => f.Grupos)
                .ThenInclude(g => g.Jornadas)
                    .ThenInclude(j => j.Partidos)
                        .ThenInclude(p => p.EquipoVisitante)
            .Include(f => f.Grupos)
                .ThenInclude(g => g.Jornadas)
                    .ThenInclude(j => j.Partidos)
                        .ThenInclude(p => p.Estadio)
            .Include(f => f.PartidosEliminatorias)
                .ThenInclude(p => p.EquipoLocal)
            .Include(f => f.PartidosEliminatorias)
                .ThenInclude(p => p.EquipoVisitante)
            .Include(f => f.PartidosEliminatorias)
                .ThenInclude(p => p.Estadio)
            .FirstOrDefault();
    }

    public bool Existe()
    {
        return _contexto.Fixtures.Any();
    }
}
