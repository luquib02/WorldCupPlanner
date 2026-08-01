using System;
using System.Collections.Generic;
using Dominio;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositorio.DataAccess;
using Repositorio.Tests.Helpers;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(NotificacionRepositorio))]
public class RepositorioNotificacionTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private NotificacionRepositorio _repo = null!;
    private int ID_NOTIFICACION_INEXISTENTE = 999;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repo = new NotificacionRepositorio(_contexto);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    private const int DESTINATARIO_POR_DEFECTO = 1;

    private static Notificacion CrearNotificacion(string mensaje = "Resultado simulado", int partidoId = 1, int destinatarioId = DESTINATARIO_POR_DEFECTO)
        => new Notificacion(mensaje, partidoId, destinatarioId);

    [TestMethod]
    public void AgregarNotificacionAsignaIdOK()
    {
        Notificacion notificacion = CrearNotificacion();

        _repo.Agregar(notificacion);

        Assert.IsTrue(notificacion.Id > 0);
    }

    [TestMethod]
    public void ObtenerPorIdExistenteRetornaNotificacionOK()
    {
        Notificacion notificacion = CrearNotificacion("Partido jugado", 5);
        _repo.Agregar(notificacion);

        Notificacion? encontrada = _repo.ObtenerPorId(notificacion.Id);

        Assert.IsNotNull(encontrada);
        Assert.AreEqual("Partido jugado", encontrada.Mensaje);
        Assert.AreEqual(5, encontrada.PartidoId);
    }

    [TestMethod]
    public void ObtenerPorIdInexistenteRetornaNullOK()
    {
        Assert.IsNull(_repo.ObtenerPorId(999));
    }

    [TestMethod]
    public void ObtenerTodasRetornaTodasLasNotificacionesOK()
    {
        _repo.Agregar(CrearNotificacion("Primer mensaje", 1));
        _repo.Agregar(CrearNotificacion("Segundo mensaje", 2));

        List<Notificacion> todas = _repo.ObtenerTodas(DESTINATARIO_POR_DEFECTO);

        Assert.AreEqual(2, todas.Count);
    }

    [TestMethod]
    public void ObtenerNoLeidasNoIncluyeLasMarcadasComoLeidasOK()
    {
        Notificacion leida = CrearNotificacion("Leida", 1);
        leida.MarcarComoLeida();
        Notificacion noLeida = CrearNotificacion("No leida", 2);
        _repo.Agregar(leida);
        _repo.Agregar(noLeida);

        List<Notificacion> noLeidas = _repo.ObtenerNoLeidas(DESTINATARIO_POR_DEFECTO);

        Assert.AreEqual(1, noLeidas.Count);
        Assert.AreEqual("No leida", noLeidas[0].Mensaje);
    }

    [TestMethod]
    public void ObtenerTodasSoloRetornaLasDelDestinatarioOK()
    {
        _repo.Agregar(CrearNotificacion("Para periodista 1", 1, destinatarioId: 1));
        _repo.Agregar(CrearNotificacion("Para periodista 2", 1, destinatarioId: 2));

        List<Notificacion> delUno = _repo.ObtenerTodas(1);

        Assert.AreEqual(1, delUno.Count);
        Assert.AreEqual("Para periodista 1", delUno[0].Mensaje);
    }

    [TestMethod]
    public void ObtenerNoLeidasDeUnDestinatarioNoSeAfectaPorLecturaDeOtroOK()
    {
        Notificacion delUno = CrearNotificacion("Misma noticia", 1, destinatarioId: 1);
        Notificacion delDos = CrearNotificacion("Misma noticia", 1, destinatarioId: 2);
        _repo.Agregar(delUno);
        _repo.Agregar(delDos);

        delUno.MarcarComoLeida();
        _repo.Actualizar(delUno);

        Assert.AreEqual(0, _repo.ObtenerNoLeidas(1).Count);
        Assert.AreEqual(1, _repo.ObtenerNoLeidas(2).Count);
    }

    [TestMethod]
    public void ActualizarNotificacionExistenteMarcaComoLeidaOK()
    {
        Notificacion notificacion = CrearNotificacion("Pendiente", 1);
        _repo.Agregar(notificacion);

        notificacion.MarcarComoLeida();
        _repo.Actualizar(notificacion);

        Notificacion? actualizada = _repo.ObtenerPorId(notificacion.Id);
        Assert.IsNotNull(actualizada);
        Assert.IsTrue(actualizada.Leida);
    }

    [TestMethod]
    public void ActualizarNotificacionInexistenteLanzaExcepcion()
    {
        Notificacion notificacion = CrearNotificacion("Inexistente", 1);
        notificacion.Id = ID_NOTIFICACION_INEXISTENTE;

        Assert.Throws<InvalidOperationException>(
            () => _repo.Actualizar(notificacion));
    }

    [TestMethod]
    public void AgregarNotificacionConFalloDePersistenciaLanzaDbUpdateException()
    {
        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        NotificacionRepositorio repoFalla = new NotificacionRepositorio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Agregar(CrearNotificacion()));
    }

    [TestMethod]
    public void ActualizarNotificacionConFalloDePersistenciaLanzaDbUpdateException()
    {
        Notificacion notificacion = CrearNotificacion("Pendiente", 1);
        _repo.Agregar(notificacion);

        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        NotificacionRepositorio repoFalla = new NotificacionRepositorio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Actualizar(notificacion));
    }
}