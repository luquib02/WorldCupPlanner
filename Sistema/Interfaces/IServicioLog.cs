using Sistema.DTOs;

namespace Sistema.Interfaces;

public interface IServicioLog
{
    public void Registrar(string usuarioEmail, string accion, string detalle);
    public List<LogEntryDTO> ListarLogs();
}
