using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class NotificacionRepositorio : IRepositorioNotificacion
{
    private readonly WorldCupPlannerDbContext _contexto;

    public NotificacionRepositorio(WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Notificacion notificacion)
    {
        try
        {
            _contexto.Notificaciones.Add(notificacion);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public List<Notificacion> ObtenerNoLeidas(int destinatarioId)
    {
        return _contexto.Notificaciones
            .Where(n => n.DestinatarioId == destinatarioId && !n.Leida)
            .OrderByDescending(n => n.Timestamp)
            .ToList();
    }

    public List<Notificacion> ObtenerTodas(int destinatarioId)
    {
        return _contexto.Notificaciones
            .Where(n => n.DestinatarioId == destinatarioId)
            .OrderByDescending(n => n.Timestamp)
            .ToList();
    }

    public Notificacion? ObtenerPorId(int id)
    {
        return _contexto.Notificaciones.FirstOrDefault(n => n.Id == id);
    }

    public void Actualizar(Notificacion notificacion)
    {
        bool existe = _contexto.Notificaciones.Any(n => n.Id == notificacion.Id);
        if (!existe)
            throw new InvalidOperationException("La notificación no existe.");
        try
        {
            _contexto.Notificaciones.Update(notificacion);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }
}