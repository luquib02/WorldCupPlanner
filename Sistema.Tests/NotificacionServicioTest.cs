using System;
using System.Collections.Generic;
using Dominio;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositorio;
using Repositorio.DataAccess;
using Sistema.Interfaces;

namespace Sistema.Tests;

[TestClass]
public class NotificacionServicioTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private IRepositorioNotificacion _repositorioNotificacion = null!;
    private IRepositorioUsuario _repositorioUsuario = null!;
    private ServicioSesion _servicioSesion = null!;
    private NotificacionServicio _notificacionServicio = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repositorioNotificacion = new NotificacionRepositorio(_contexto);
        _repositorioUsuario = new RepositorioUsuario(_contexto);
        _servicioSesion = new ServicioSesion();

        Mock<IRepositorioLog> repositorioLogMock = new Mock<IRepositorioLog>();
        LogServicio logServicio = new LogServicio(repositorioLogMock.Object);

        _notificacionServicio = new NotificacionServicio(
            _repositorioNotificacion,
            _repositorioUsuario,
            _servicioSesion,
            logServicio);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    private void Loguear(Usuario usuario)
    {
        if (_servicioSesion.EstaLogueado)
            _servicioSesion.CerrarSesion();
        _servicioSesion.IniciarSesion(usuario);
    }

    private Usuario CrearYPersistir(string correo, RolDeUsuario rol)
    {
        Usuario usuario = new Usuario
        {
            Nombre = "Nombre",
            Apellido = "Apellido",
            CorreoElectronico = correo,
            FechaNacimiento = new DateOnly(1990, 1, 1)
        };
        usuario.EstablecerContrasenia("Clave123!@");
        usuario.AgregarRol(rol);
        _repositorioUsuario.CrearUsuario(usuario);
        return _repositorioUsuario.ObtenerUsuarioPorCorreo(correo);
    }

    [TestMethod]
    public void Crear_ConVariosPeriodistas_CreaUnaNotificacionEnLaBandejaDeCadaUno()
    {
        Usuario periodistaUno = CrearYPersistir("peri1@sistema.com", RolDeUsuario.Periodista);
        Usuario periodistaDos = CrearYPersistir("peri2@sistema.com", RolDeUsuario.Periodista);

        _notificacionServicio.Crear("Uruguay 2 - 1 Argentina", 1);

        Loguear(periodistaUno);
        Assert.AreEqual(1, _notificacionServicio.ObtenerTodas().Count);
        Loguear(periodistaDos);
        Assert.AreEqual(1, _notificacionServicio.ObtenerTodas().Count);
    }

    [TestMethod]
    public void Crear_NoGeneraNotificacionesParaNoPeriodistas()
    {
        Usuario administrador = CrearYPersistir("admin@sistema.com", RolDeUsuario.AdministradorDelSistema);

        _notificacionServicio.Crear("Uruguay 2 - 1 Argentina", 1);

        Loguear(administrador);
        Assert.AreEqual(0, _notificacionServicio.ObtenerTodas().Count);
    }

    [TestMethod]
    public void ObtenerNoLeidas_RetornaSoloLasDelUsuarioActual()
    {
        Usuario periodista = CrearYPersistir("peri@sistema.com", RolDeUsuario.Periodista);
        _notificacionServicio.Crear("Uruguay 2 - 1 Argentina", 1);

        Loguear(periodista);
        List<Notificacion> noLeidas = _notificacionServicio.ObtenerNoLeidas();

        Assert.AreEqual(1, noLeidas.Count);
        Assert.AreEqual(periodista.Id, noLeidas[0].DestinatarioId);
    }

    [TestMethod]
    public void MarcarComoLeida_DeUnPeriodista_NoAfectaLaBandejaDeLosDemas()
    {
        Usuario periodistaUno = CrearYPersistir("peri1@sistema.com", RolDeUsuario.Periodista);
        Usuario periodistaDos = CrearYPersistir("peri2@sistema.com", RolDeUsuario.Periodista);
        _notificacionServicio.Crear("Uruguay 2 - 1 Argentina", 1);

        Loguear(periodistaUno);
        int idNotificacionUno = _notificacionServicio.ObtenerNoLeidas()[0].Id;
        _notificacionServicio.MarcarComoLeida(idNotificacionUno);

        Assert.AreEqual(0, _notificacionServicio.ObtenerNoLeidas().Count);
        Loguear(periodistaDos);
        Assert.AreEqual(1, _notificacionServicio.ObtenerNoLeidas().Count);
    }

    [TestMethod]
    public void MarcarComoLeida_NotificacionDeOtroUsuario_LanzaExcepcion()
    {
        Usuario periodistaUno = CrearYPersistir("peri1@sistema.com", RolDeUsuario.Periodista);
        Usuario periodistaDos = CrearYPersistir("peri2@sistema.com", RolDeUsuario.Periodista);
        _notificacionServicio.Crear("Uruguay 2 - 1 Argentina", 1);

        Loguear(periodistaUno);
        int idNotificacionUno = _notificacionServicio.ObtenerNoLeidas()[0].Id;

        Loguear(periodistaDos);
        Assert.Throws<DominioException>(() => _notificacionServicio.MarcarComoLeida(idNotificacionUno));
    }

    [TestMethod]
    public void MarcarComoLeida_NotificacionNoExistente_LanzaExcepcion()
    {
        Usuario periodista = CrearYPersistir("peri@sistema.com", RolDeUsuario.Periodista);
        Loguear(periodista);

        Assert.Throws<DominioException>(() => _notificacionServicio.MarcarComoLeida(999));
    }
}
