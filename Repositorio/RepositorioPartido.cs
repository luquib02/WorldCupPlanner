using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class RepositorioPartido : IPartidoRepositorio
{
    private readonly WorldCupPlannerDbContext _contexto;

    public RepositorioPartido(WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Partido partido)
    {
        try
        {
            _contexto.Partidos.Add(partido);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public Partido? ObtenerPorId(int id)
    {
        return _contexto.Partidos
            .Include(p => p.Estadio)
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Include(p => p.PartidoOrigenLocal)
            .Include(p => p.PartidoOrigenVisitante)
            .Include(p => p.Incidencias)
            .FirstOrDefault(p => p.Id == id);
    }

    public List<Partido> ObtenerTodos()
    {
        return _contexto.Partidos
            .Include(p => p.Estadio)
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Include(p => p.Incidencias)
            .ToList();
    }

    public void Actualizar(Partido partido)
    {
        bool existe = _contexto.Partidos.Any(p => p.Id == partido.Id);
        if (!existe)
            throw new InvalidOperationException("El partido no existe.");
        try
        {
            _contexto.Partidos.Update(partido);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public void Eliminar(int id)
    {
        Partido? partido = _contexto.Partidos.FirstOrDefault(p => p.Id == id);
        if (partido is null)
            throw new InvalidOperationException("El partido no existe.");
        try
        {
            _contexto.Partidos.Remove(partido);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public List<Partido> ObtenerPorGrupo(string grupo)
    {
        return _contexto.Partidos
            .Include(p => p.Estadio)
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Include(p => p.Incidencias)
            .Where(p => p.Grupo == grupo)
            .ToList();
    }

    public List<Partido> ObtenerPorFase(FasePartido fase)
    {
        return _contexto.Partidos
            .Include(p => p.Estadio)
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Include(p => p.Incidencias)
            .Where(p => p.Fase == fase)
            .ToList();
    }

    public bool ExistenPartidosFueraDeGrupos()
    {
        return _contexto.Partidos.Any(p => p.Fase != FasePartido.FaseDeGrupos);
    }
}