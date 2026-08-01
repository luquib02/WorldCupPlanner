using Dominio;

namespace Repositorio;

public interface IRepositorioNotificacion
{
    void Agregar(Notificacion notificacion);
    List<Notificacion> ObtenerNoLeidas(int destinatarioId);
    List<Notificacion> ObtenerTodas(int destinatarioId);
    Notificacion? ObtenerPorId(int id);
    void Actualizar(Notificacion notificacion);
}