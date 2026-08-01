using Dominio.Excepciones;
using System.Text.RegularExpressions;

namespace Dominio;

public class LogEntry
{
    private const int _accionMaxLength = 100;
    private const int _detalleMaxLength = 500;

    public int Id { get; set; }  
    public DateTime Timestamp { get; set; }
    public string UsuarioEmail { get; private set; } = string.Empty;
    public string Accion { get; private set; } = string.Empty;
    public string Detalle { get; private set; } = string.Empty;

    protected LogEntry() { }  

    public LogEntry(string usuarioEmail, string accion, string detalle)
    {
        Timestamp = DateTime.Now;
        UsuarioEmail = ValidarYRetornarEmail(usuarioEmail);
        Accion = ValidarYRetornarAccion(accion);
        Detalle = ValidarYRetornarDetalle(detalle);
    }

    private static string ValidarYRetornarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DominioException("El email es obligatorio.");
        string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(email, patron))
            throw new DominioException("El email tiene un formato invalido.");
        return email;
    }

    private static string ValidarYRetornarAccion(string accion)
    {
        if (string.IsNullOrWhiteSpace(accion))
            throw new DominioException("La accion es obligatoria.");
        if (accion.Length > _accionMaxLength)
            throw new DominioException($"La accion no puede superar los {_accionMaxLength} caracteres.");
        return accion;
    }

    private static string ValidarYRetornarDetalle(string detalle)
    {
        if (string.IsNullOrWhiteSpace(detalle))
            throw new DominioException("El detalle es obligatorio.");
        if (detalle.Length > _detalleMaxLength)
            throw new DominioException($"El detalle no puede superar los {_detalleMaxLength} caracteres.");
        return detalle;
    }
}