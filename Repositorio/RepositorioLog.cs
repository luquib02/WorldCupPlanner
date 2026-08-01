using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class RepositorioLog : IRepositorioLog
{
    private readonly WorldCupPlannerDbContext _contexto;

    public RepositorioLog(WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(LogEntry log)
    {
        try
        {
            _contexto.Logs.Add(log);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public List<LogEntry> ListarTodos()
    {
        return _contexto.Logs.ToList();
    }
    
    public List<LogEntry> ObtenerPorRango(DateTime desde, DateTime hasta)
    {
        return _contexto.Logs
            .Where(l => l.Timestamp >= desde && l.Timestamp <= hasta)
            .OrderBy(l => l.Timestamp)
            .ToList();
    }
}