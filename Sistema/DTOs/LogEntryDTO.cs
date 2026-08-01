namespace Sistema.DTOs;

public class LogEntryDTO
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UsuarioEmail { get; set; }
    public string Accion { get; set; }
    public string Detalle { get; set; }
}
