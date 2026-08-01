using Dominio;
using Repositorio;
using Sistema.DTOs;
using Sistema.Interfaces;
using Sistema.Mappers;

namespace Sistema;

public class LogServicio : IServicioLog
{
    private readonly IRepositorioLog _logRepositorio;

    public LogServicio(IRepositorioLog logRepositorio)
    {
        _logRepositorio = logRepositorio;
    }

    public void Registrar(string usuarioEmail, string accion, string detalle)
    {
        LogEntry log = new LogEntry(usuarioEmail, accion, detalle);
        _logRepositorio.Agregar(log);
    }

    public List<LogEntryDTO> ListarLogs()
    {
        return _logRepositorio.ListarTodos().Select(LogEntryMapper.ALogEntryDTO).ToList();
    }
}