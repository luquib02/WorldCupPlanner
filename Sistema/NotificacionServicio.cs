using System.Collections.Generic;
using System.Linq;
using Dominio;
using Dominio.Excepciones;
using Repositorio;
using Sistema.Interfaces;

namespace Sistema;

public class NotificacionServicio
{
    private readonly IRepositorioNotificacion _repositorioNotificacion;
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IServicioSesion _servicioSesion;
    private readonly LogServicio _logServicio;

    public NotificacionServicio(
        IRepositorioNotificacion repositorioNotificacion,
        IRepositorioUsuario repositorioUsuario,
        IServicioSesion servicioSesion,
        LogServicio logServicio)
    {
        _repositorioNotificacion = repositorioNotificacion;
        _repositorioUsuario = repositorioUsuario;
        _servicioSesion = servicioSesion;
        _logServicio = logServicio;
    }

    // Una notificación se reparte a la bandeja de cada periodista (un registro por
    // destinatario), de modo que el estado "leída" es individual: que un periodista
    // marque la suya no afecta las de los demás.
    public void Crear(string mensaje, int partidoId)
    {
        foreach (Usuario periodista in ObtenerPeriodistas())
        {
            Notificacion notificacion = new Notificacion(mensaje, partidoId, periodista.Id);
            _repositorioNotificacion.Agregar(notificacion);
        }
    }

    public List<Notificacion> ObtenerNoLeidas()
    {
        return _repositorioNotificacion.ObtenerNoLeidas(_servicioSesion.ObtenerUsuarioActual().Id);
    }

    public List<Notificacion> ObtenerTodas()
    {
        return _repositorioNotificacion.ObtenerTodas(_servicioSesion.ObtenerUsuarioActual().Id);
    }

    public void MarcarComoLeida(int id)
    {
        Usuario usuarioActual = _servicioSesion.ObtenerUsuarioActual();
        Notificacion notificacion = ObtenerNotificacionOLanzarExcepcion(id);
        if (notificacion.DestinatarioId != usuarioActual.Id)
            throw new DominioException("La notificación no pertenece al usuario.");
        notificacion.MarcarComoLeida();
        _repositorioNotificacion.Actualizar(notificacion);
        _logServicio.Registrar(usuarioActual.CorreoElectronico, "LecturaNotificacion",
            $"Se marcó como leída la notificación Id={id}.");
    }

    private IEnumerable<Usuario> ObtenerPeriodistas()
    {
        return _repositorioUsuario.ObtenerListaUsuarios()
            .Where(u => u.Roles.Contains(RolDeUsuario.Periodista));
    }

    private Notificacion ObtenerNotificacionOLanzarExcepcion(int id)
    {
        Notificacion? notificacion = _repositorioNotificacion.ObtenerPorId(id);
        if (notificacion == null)
            throw new DominioException("La notificación no existe.");
        return notificacion;
    }
}