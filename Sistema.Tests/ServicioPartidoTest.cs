using Dominio;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositorio;
using Sistema;
using Sistema.DTOs;
using System;
using System.Collections.Generic;

namespace Sistema.Tests;

[TestClass]
public class PartidoServicioTest
{
    private Mock<IPartidoRepositorio> _partidoRepoMock;
    private Mock<IRepositorioEstadio> _estadioRepoMock;
    private Mock<IRepositorioLog> _logRepoMock;
    private Mock<IEquipoRepositorio> _equipoRepoMock;
    private Mock<IRepositorioNotificacion> _notificacionRepoMock;
    private Mock<IRepositorioUsuario> _usuarioRepoMock;
    private CalculadoraRankingElo _calculadora;
    private PartidoServicio _servicio;

    [TestInitialize]
    public void Setup()
    {
        _partidoRepoMock = new Mock<IPartidoRepositorio>();
        _estadioRepoMock = new Mock<IRepositorioEstadio>();
        _logRepoMock = new Mock<IRepositorioLog>();
        _equipoRepoMock = new Mock<IEquipoRepositorio>();
        LogServicio logServicio = new LogServicio(_logRepoMock.Object);
        _calculadora = new CalculadoraRankingElo();
        _notificacionRepoMock = new Mock<IRepositorioNotificacion>();
        _usuarioRepoMock = new Mock<IRepositorioUsuario>();
        _usuarioRepoMock.Setup(r => r.ObtenerListaUsuarios())
            .Returns(new List<Usuario> { CrearPeriodistaConId(7) });
        NotificacionServicio notificacionServicio = new NotificacionServicio(
            _notificacionRepoMock.Object, _usuarioRepoMock.Object, new ServicioSesion(), logServicio);
        ResultadoPartidoServicio resultadoServicio = new ResultadoPartidoServicio(
            _equipoRepoMock.Object, _partidoRepoMock.Object, logServicio, _calculadora, notificacionServicio);
        _servicio = new PartidoServicio(_partidoRepoMock.Object, _estadioRepoMock.Object, logServicio, resultadoServicio);
    }

    private static Usuario CrearPeriodistaConId(int id)
    {
        Usuario periodista = new Usuario
        {
            Nombre = "Peri",
            Apellido = "Odista",
            CorreoElectronico = "peri@sistema.com",
            FechaNacimiento = new DateOnly(1990, 1, 1)
        };
        periodista.AgregarRol(RolDeUsuario.Periodista);
        typeof(Usuario).GetProperty(nameof(Usuario.Id))!.SetValue(periodista, id);
        return periodista;
    }

    private Equipo CrearEquipo(string nombre) =>
        new Equipo(nombre, Confederacion.CONMEBOL, 1500);

    private Estadio CrearEstadio() =>
        new Estadio { Nombre = "Centenario", Ciudad = "Montevideo", Capacidad = 60000 };

    private Partido CrearPartido()
    {
        return new Partido
        {
            Fecha = DateTime.Now.AddDays(1),
            Estadio = CrearEstadio(),
            EquipoLocal = CrearEquipo("Uruguay"),
            EquipoVisitante = CrearEquipo("Brasil"),
            Grupo = "A",
            Fase = FasePartido.FaseDeGrupos
        };
    }

    [TestMethod]
    public void CrearPartidoValidoOK()
    {
        Partido partido = CrearPartido();

        _servicio.Crear(partido, "admin@test.com");

        _partidoRepoMock.Verify(r => r.Agregar(partido), Times.Once);
    }

    [TestMethod]
    public void CrearPartidoRegistraLogOK()
    {
        Partido partido = CrearPartido();

        _servicio.Crear(partido, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "AltaPartido"
        )), Times.Once);
    }

    [TestMethod]
    public void ModificarFechaPartidoValidoOK()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        DateTime nuevaFecha = new DateTime(2026, 6, 5);
        _servicio.ModificarFecha(1, nuevaFecha, "admin@test.com");

        Assert.AreEqual(nuevaFecha, partido.Fecha);
        _partidoRepoMock.Verify(r => r.Actualizar(partido), Times.Once);
    }

    [TestMethod]
    public void ModificarEstadioPartidoValidoOK()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        Estadio nuevoEstadio = new Estadio { Id = 2, Nombre = "Maracana", Ciudad = "Rio", Capacidad = 80000 };
        _estadioRepoMock.Setup(r => r.BuscarPorId(2)).Returns(nuevoEstadio);

        _servicio.ModificarEstadio(1, 2, "admin@test.com");

        Assert.AreSame(nuevoEstadio, partido.Estadio);
    }

    [TestMethod]
    public void ModificarEstadioInexistenteLanzaExcepcion()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });
        _estadioRepoMock.Setup(r => r.BuscarPorId(99)).Returns((Estadio?)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.ModificarEstadio(1, 99, "admin@test.com"));
        Assert.AreEqual("El estadio no existe.", ex.Message);
    }

    [TestMethod]
    public void CargarResultadoPartidoValidoOK()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        _servicio.CargarResultado(1, 2, 1, "admin@test.com");

        Assert.AreEqual(2, partido.GolesLocal);
        Assert.AreEqual(1, partido.GolesVisitante);
    }

    [TestMethod]
    public void ModificarPartidoInexistenteLanzaExcepcion()
    {
        _partidoRepoMock.Setup(r => r.ObtenerPorId(99)).Returns((Partido?)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.ModificarFecha(99, DateTime.Now, "admin@test.com"));
        Assert.AreEqual("El partido no existe.", ex.Message);
    }

    [TestMethod]
    public void ModificarPartidoConCrucesGeneradosLanzaExcepcion()
    {
        Partido partidoGrupo = CrearPartido();
        partidoGrupo.Id = 1;

        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partidoGrupo);
        _partidoRepoMock.Setup(r => r.ExistenPartidosFueraDeGrupos()).Returns(true);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.ModificarFecha(1, DateTime.Now, "admin@test.com"));
        Assert.AreEqual("No se pueden editar partidos de fase de grupos una vez generados los cruces.", ex.Message);
    }

    [TestMethod]
    public void ListarTodosOK()
    {
        List<Partido> lista = new List<Partido> { CrearPartido(), CrearPartido() };
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(lista);

        List<PartidoDTO> resultado = _servicio.ListarTodos();

        Assert.AreEqual(2, resultado.Count);
    }

    [TestMethod]
    public void ListarPorGrupoOK()
    {
        List<Partido> lista = new List<Partido> { CrearPartido() };
        _partidoRepoMock.Setup(r => r.ObtenerPorGrupo("A")).Returns(lista);

        List<PartidoDTO> resultado = _servicio.ListarPorGrupo("A");

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("A", resultado[0].Grupo);
    }

    [TestMethod]
    public void CargarResultado_VictoriaLocal_ActualizaRankingAmbosEquipos()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        int rankingLocalInicial = partido.EquipoLocal.RankingActual;
        int rankingVisitanteInicial = partido.EquipoVisitante.RankingActual;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        _servicio.CargarResultado(1, 2, 0, "admin@test.com");

        Assert.IsTrue(partido.EquipoLocal.RankingActual > rankingLocalInicial,
            "El equipo local que gana debe subir su ranking");
        Assert.IsTrue(partido.EquipoVisitante.RankingActual < rankingVisitanteInicial,
            "El equipo visitante que pierde debe bajar su ranking");
    }

    [TestMethod]
    public void CargarResultado_EmpateEntreIguales_NoModificaRankings()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        int rankingLocalInicial = partido.EquipoLocal.RankingActual;
        int rankingVisitanteInicial = partido.EquipoVisitante.RankingActual;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        _servicio.CargarResultado(1, 1, 1, "admin@test.com");

        Assert.AreEqual(rankingLocalInicial, partido.EquipoLocal.RankingActual,
            "Con rankings iguales y empate el delta es 0, el ranking no debe cambiar");
        Assert.AreEqual(rankingVisitanteInicial, partido.EquipoVisitante.RankingActual,
            "Con rankings iguales y empate el delta es 0, el ranking no debe cambiar");
    }

    [TestMethod]
    public void CargarResultado_VictoriaLocal_RegistraLogCambioRanking()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        _servicio.CargarResultado(1, 2, 0, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "CambioRanking"
        )), Times.Once, "CargarResultado debe registrar el cambio de ranking en el log de auditoría");
    }

    [TestMethod]
    public void CargarResultado_VictoriaLocal_PersisteRankingDeAmbosEquipos()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        _servicio.CargarResultado(1, 2, 0, "admin@test.com");

        _equipoRepoMock.Verify(r => r.Actualizar(partido.EquipoLocal), Times.Once,
            "El ranking actualizado del equipo local debe persistirse en la carga manual");
        _equipoRepoMock.Verify(r => r.Actualizar(partido.EquipoVisitante), Times.Once,
            "El ranking actualizado del equipo visitante debe persistirse en la carga manual");
    }

    [TestMethod]
    public void CargarResultado_GeneraNotificacion()
    {
        Partido partido = CrearPartido();
        partido.Id = 1;
        _partidoRepoMock.Setup(r => r.ObtenerPorId(1)).Returns(partido);
        _partidoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Partido> { partido });

        _servicio.CargarResultado(1, 2, 0, "admin@test.com");

        _notificacionRepoMock.Verify(r => r.Agregar(It.IsAny<Notificacion>()), Times.Once,
            "Cargar un resultado debe generar una notificación para los periodistas");
    }
}
