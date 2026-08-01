using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class EquipoRepositorio : IEquipoRepositorio
{
    private readonly WorldCupPlannerDbContext _contexto;

    public EquipoRepositorio(WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Equipo equipo)
    {
        try
        {
            _contexto.Equipos.Add(equipo);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public Equipo? ObtenerPorId(int id)
    {
        return _contexto.Equipos.FirstOrDefault(e => e.Id == id);
    }

    public List<Equipo> ObtenerTodos()
    {
        return _contexto.Equipos.ToList();
    }

    public void Actualizar(Equipo equipo)
    {
        bool existe = _contexto.Equipos.Any(e => e.Id == equipo.Id);
        if (!existe)
            throw new InvalidOperationException("El equipo no existe.");
        try
        {
            _contexto.Equipos.Update(equipo);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public void Eliminar(int id)
    {
        Equipo? equipo = _contexto.Equipos.FirstOrDefault(e => e.Id == id);
        if (equipo is null)
            throw new InvalidOperationException("El equipo no existe.");
        try
        {
            _contexto.Equipos.Remove(equipo);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public bool ExisteNombre(string nombre)
    {
        return _contexto.Equipos.Any(e => e.Nombre == nombre);
    }

    public int ContarPorConfederacion(Confederacion confederacion)
    {
        return _contexto.Equipos.Count(e => e.Confederacion == confederacion);
    }
}