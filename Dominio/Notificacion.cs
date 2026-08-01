using Dominio.Excepciones;

namespace Dominio;

public class Notificacion
{
    private const int _longitudMaximaMensaje = 500;

    public int Id { get; set; }
    public DateTime Timestamp { get; private set; }
    public string Mensaje { get; private set; }
    public int PartidoId { get; private set; }
    public int DestinatarioId { get; private set; }
    public bool Leida { get; private set; }

    protected Notificacion() { }

    public Notificacion(string mensaje, int partidoId, int destinatarioId)
    {
        ValidarMensaje(mensaje);
        ValidarPartidoId(partidoId);
        ValidarDestinatarioId(destinatarioId);
        Mensaje = mensaje;
        PartidoId = partidoId;
        DestinatarioId = destinatarioId;
        Timestamp = DateTime.Now;
        Leida = false;
    }

    public void MarcarComoLeida()
    {
        Leida = true;
    }

    private void ValidarMensaje(string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
            throw new DominioException("El mensaje es obligatorio.");
        if (mensaje.Length > _longitudMaximaMensaje)
            throw new DominioException($"El mensaje no puede superar los {_longitudMaximaMensaje} caracteres.");
    }

    private void ValidarPartidoId(int partidoId)
    {
        if (partidoId <= 0)
            throw new DominioException("El partido es obligatorio.");
    }

    private void ValidarDestinatarioId(int destinatarioId)
    {
        if (destinatarioId <= 0)
            throw new DominioException("El destinatario es obligatorio.");
    }
}