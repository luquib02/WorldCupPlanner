using Dominio;

namespace Repositorio;

public interface IRepositorioLog
{
    void Agregar(LogEntry log);
    List<LogEntry> ListarTodos();
    List<LogEntry> ObtenerPorRango(DateTime desde, DateTime hasta);
    
}