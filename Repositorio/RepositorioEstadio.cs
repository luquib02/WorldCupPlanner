using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class RepositorioEstadio : IRepositorioEstadio
{
    private readonly WorldCupPlannerDbContext _contexto;

    public RepositorioEstadio(WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Estadio estadio)
    {
        try
        {
            _contexto.Estadios.Add(estadio);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public Estadio? BuscarPorId(int id)
    {
        return _contexto.Estadios.FirstOrDefault(e => e.Id == id);
    }

    public Estadio? BuscarPorNombre(string nombre)
    {
        return _contexto.Estadios.FirstOrDefault(e => e.Nombre == nombre);
    }

    public List<Estadio> ListarTodos()
    {
        return _contexto.Estadios.ToList();
    }

    public void Modificar(Estadio estadio)
    {
        bool existe = _contexto.Estadios.Any(e => e.Id == estadio.Id);
        if (!existe)
            throw new InvalidOperationException("El estadio no existe.");

        try
        {
            _contexto.Estadios.Update(estadio);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public void Eliminar(int id)
    {
        Estadio? estadio = _contexto.Estadios.FirstOrDefault(e => e.Id == id);
        if (estadio is null)
            throw new InvalidOperationException("El estadio no existe.");

        try
        {
            _contexto.Estadios.Remove(estadio);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }
}