using Dominio;
using Dominio.Excepciones;
using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositorio;
using Sistema;
using Sistema.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using Sistema.Tests.Helpers;

namespace Sistema.Tests;

[TestClass]
public class CrucesServicioTest
{
    private Mock<IFixtureRepositorio> _fixtureRepoMock;
    private Mock<IPartidoRepositorio> _partidoRepoMock;
    private Mock<IRepositorioEstadio> _estadioRepoMock;
    private Mock<IRepositorioLog> _logRepoMock;
    private Mock<IEquipoRepositorio> _equipoRepoMock;
    private CrucesServicio _servicio;

    [TestInitialize]
    public void Setup()
    {
        _fixtureRepoMock = new Mock<IFixtureRepositorio>();
        _partidoRepoMock = new Mock<IPartidoRepositorio>();
        _estadioRepoMock = new Mock<IRepositorioEstadio>();
        _logRepoMock = new Mock<IRepositorioLog>();
        _equipoRepoMock = new Mock<IEquipoRepositorio>();
        LogServicio logServicio = new LogServicio(_logRepoMock.Object);
        Mock<IRepositorioNotificacion> notificacionRepoMock = new Mock<IRepositorioNotificacion>();
        Mock<IRepositorioUsuario> usuarioRepoMock = new Mock<IRepositorioUsuario>();
        usuarioRepoMock.Setup(r => r.ObtenerListaUsuarios()).Returns(new List<Usuario>());
        NotificacionServicio notificacionServicio = new NotificacionServicio(
            notificacionRepoMock.Object, usuarioRepoMock.Object, new ServicioSesion(), logServicio);
        ResultadoPartidoServicio resultadoServicio = new ResultadoPartidoServicio(
            _equipoRepoMock.Object, _partidoRepoMock.Object, logServicio, new CalculadoraRankingElo(), notificacionServicio);
        _servicio = new CrucesServicio(
            _fixtureRepoMock.Object,
            _partidoRepoMock.Object,
            _estadioRepoMock.Object,
            logServicio,
            new SimulacionServicio(),
            resultadoServicio);
    }
    

    private List<Estadio> UnEstadio() => new List<Estadio>
    {
        new Estadio { Nombre = "Estadio A", Ciudad = "Ciudad", Capacidad = 50000 }
    };

    private List<Estadio> DosEstadios() => new List<Estadio>
    {
        new Estadio { Nombre = "Estadio A", Ciudad = "Ciudad", Capacidad = 50000 },
        new Estadio { Nombre = "Estadio B", Ciudad = "Ciudad", Capacidad = 60000 }
    };

    private Fixture FixtureConGruposCompletos() =>
        new FixtureBuilder().ConResultadosCargados().Construir();

    private Fixture FixtureConDieciseisavosJugados()
    {
        Fixture fixture = new FixtureBuilder()
            .ConResultadosCargados()
            .ConPartidosEliminatorios(FixtureBuilder.CrearDieciseisavosJugados())
            .Construir();
        fixture.MarcarCrucesGenerados();
        return fixture;
    }

    private Fixture FixtureConOctavosJugados()
    {
        List<Partido> eliminatorios = new List<Partido>();
        eliminatorios.AddRange(FixtureBuilder.CrearDieciseisavosJugados());
        eliminatorios.AddRange(FixtureBuilder.CrearOctavosJugados());
        return new FixtureBuilder()
            .ConResultadosCargados()
            .ConPartidosEliminatorios(eliminatorios)
            .Construir();
    }

    private Fixture FixtureConCuartosJugados()
    {
        List<Partido> eliminatorios = new List<Partido>();
        eliminatorios.AddRange(FixtureBuilder.CrearDieciseisavosJugados());
        eliminatorios.AddRange(FixtureBuilder.CrearOctavosJugados());
        eliminatorios.AddRange(FixtureBuilder.CrearCuartosJugados());
        return new FixtureBuilder()
            .ConResultadosCargados()
            .ConPartidosEliminatorios(eliminatorios)
            .Construir();
    }

    private Fixture FixtureConSemifinalesJugadas()
    {
        List<Partido> eliminatorios = new List<Partido>();
        eliminatorios.AddRange(FixtureBuilder.CrearDieciseisavosJugados());
        eliminatorios.AddRange(FixtureBuilder.CrearOctavosJugados());
        eliminatorios.AddRange(FixtureBuilder.CrearCuartosJugados());
        eliminatorios.AddRange(FixtureBuilder.CrearSemifinalesJugadas());
        return new FixtureBuilder()
            .ConResultadosCargados()
            .ConPartidosEliminatorios(eliminatorios)
            .Construir();
    }

    private static string ObtenerGrupoDeEquipo(Fixture fixture, Equipo equipo)
    {
        foreach (Grupo grupo in fixture.Grupos)
            if (grupo.Equipos.Contains(equipo))
                return grupo.Etiqueta;
        return "X";
    }

    [TestMethod]
    public void GenerarDieciseisavosSinFixtureLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns((Fixture)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com"));

        Assert.AreEqual("No existe un fixture generado.", ex.Message);
    }

    [TestMethod]
    public void GenerarDieciseisavosFaseGruposIncompletaLanzaExcepcion()
    {
        Fixture fixture = new Fixture(42);
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com"));

        Assert.AreEqual("La fase de grupos no esta completa.", ex.Message);
    }

    [TestMethod]
    public void GenerarDieciseisavosCrucesYaGeneradosLanzaExcepcion()
    {
        Fixture fixture = new FixtureBuilder()
            .ConResultadosCargados()
            .Construir();
        fixture.MarcarCrucesGenerados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com"));

        Assert.AreEqual("Los cruces ya fueron generados.", ex.Message);
    }

    [TestMethod]
    public void GenerarDieciseisavosCrea16PartidosOK()
    {
        Fixture fixture = new FixtureBuilder().ConResultadosCargados().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(new List<Estadio>
        {
            new Estadio { Nombre = "Estadio A", Ciudad = "Ciudad", Capacidad = 50000 }
        });

        _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com");

        Assert.AreEqual(16, fixture.PartidosEliminatorias.Count);
    }

    [TestMethod]
    public void GenerarDieciseisavosMarcaCrucesGeneradosOK()
    {
        Fixture fixture = new FixtureBuilder().ConResultadosCargados().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(new List<Estadio>
        {
            new Estadio { Nombre = "Estadio A", Ciudad = "Ciudad", Capacidad = 50000 }
        });

        _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com");

        Assert.IsTrue(fixture.CrucesGenerados);
    }

    [TestMethod]
    public void GenerarDieciseisavosRegistraLogOK()
    {
        Fixture fixture = new FixtureBuilder().ConResultadosCargados().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(new List<Estadio>
        {
            new Estadio { Nombre = "Estadio A", Ciudad = "Ciudad", Capacidad = 50000 }
        });

        _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "GeneracionCruces" && l.UsuarioEmail == "admin@test.com"
        )), Times.Once);
    }

    [TestMethod]
    public void GenerarDieciseisavosSinEstadiosLanzaExcepcion()
    {
        Fixture fixture = FixtureConGruposCompletos();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(new List<Estadio>());

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com"));

        Assert.AreEqual("No hay estadios disponibles.", ex.Message);
    }

    [TestMethod]
    public void GenerarDieciseisavosNingunPartidoEnfrentaEquiposDelMismoGrupoOK()
    {
        Fixture fixture = FixtureConGruposCompletos();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarDieciseisavos(ConfiguracionTorneo.PorDefecto(), "admin@test.com");

        foreach (Partido partido in fixture.PartidosEliminatorias)
        {
            string grupoLocal = ObtenerGrupoDeEquipo(fixture, partido.EquipoLocal);
            string grupoVisitante = ObtenerGrupoDeEquipo(fixture, partido.EquipoVisitante);
            Assert.AreNotEqual(grupoLocal, grupoVisitante,
                $"El partido {partido.EtiquetaCruce} enfrenta equipos del mismo grupo {grupoLocal}.");
        }
    }
    

    [TestMethod]
    public void GenerarOctavosSinFixtureLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns((Fixture)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarOctavos("admin@test.com"));

        Assert.AreEqual("No existe un fixture generado.", ex.Message);
    }

    [TestMethod]
    public void GenerarOctavosSinDieciseisavosJugadosLanzaExcepcion()
    {
        Fixture fixture = FixtureConGruposCompletos();
        fixture.MarcarCrucesGenerados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarOctavos("admin@test.com"));

        Assert.AreEqual("Los dieciseisavos no estan completos.", ex.Message);
    }

    [TestMethod]
    public void GenerarOctavosYaGeneradosLanzaExcepcion()
    {
        List<Partido> eliminatorios = new List<Partido>();
        eliminatorios.AddRange(FixtureBuilder.CrearDieciseisavosJugados());
        eliminatorios.AddRange(FixtureBuilder.CrearOctavosJugados());
        Fixture fixture = new FixtureBuilder()
            .ConResultadosCargados()
            .ConPartidosEliminatorios(eliminatorios)
            .Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarOctavos("admin@test.com"));

        Assert.AreEqual("Los octavos ya fueron generados.", ex.Message);
    }

    [TestMethod]
    public void GenerarOctavosCrea8PartidosOK()
    {
        Fixture fixture = FixtureConDieciseisavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarOctavos("admin@test.com");

        int octavos = fixture.PartidosEliminatorias.ToList()
            .Count(p => p.Fase == FasePartido.OctavosDeFinal);
        Assert.AreEqual(8, octavos);
    }

    [TestMethod]
    public void GenerarOctavosEtiquetasCorrectasOK()
    {
        Fixture fixture = FixtureConDieciseisavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarOctavos("admin@test.com");

        for (int i = 1; i <= 8; i++)
        {
            bool existe = fixture.PartidosEliminatorias.ToList()
                .Any(p => p.EtiquetaCruce == $"C{i}");
            Assert.IsTrue(existe, $"Falta el partido C{i}.");
        }
    }

    [TestMethod]
    public void GenerarOctavosRegistraLogOK()
    {
        Fixture fixture = FixtureConDieciseisavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarOctavos("admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "GeneracionCruces")), Times.Once);
    }

    [TestMethod]
    public void GenerarCuartosSinFixtureLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns((Fixture)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarCuartos("admin@test.com"));

        Assert.AreEqual("No existe un fixture generado.", ex.Message);
    }

    [TestMethod]
    public void GenerarCuartosSinOctavosJugadosLanzaExcepcion()
    {
        Fixture fixture = FixtureConDieciseisavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarCuartos("admin@test.com"));

        Assert.AreEqual("Los octavos no estan completos.", ex.Message);
    }

    [TestMethod]
    public void GenerarCuartosYaGeneradosLanzaExcepcion()
    {
        Fixture fixture = FixtureConOctavosJugados();
        foreach (Partido p in FixtureBuilder.CrearCuartosJugados())
            fixture.AgregarPartidoEliminatoria(p);
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarCuartos("admin@test.com"));

        Assert.AreEqual("Los cuartos ya fueron generados.", ex.Message);
    }

    [TestMethod]
    public void GenerarCuartosCrea4PartidosOK()
    {
        Fixture fixture = FixtureConOctavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarCuartos("admin@test.com");

        int cuartos = fixture.PartidosEliminatorias.ToList()
            .Count(p => p.Fase == FasePartido.CuartosDeFinal);
        Assert.AreEqual(4, cuartos);
    }

    [TestMethod]
    public void GenerarCuartosEtiquetasCorrectasOK()
    {
        Fixture fixture = FixtureConOctavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarCuartos("admin@test.com");

        for (int i = 1; i <= 4; i++)
        {
            bool existe = fixture.PartidosEliminatorias.ToList()
                .Any(p => p.EtiquetaCruce == $"D{i}");
            Assert.IsTrue(existe, $"Falta el partido D{i}.");
        }
    }

    [TestMethod]
    public void GenerarCuartosRegistraLogOK()
    {
        Fixture fixture = FixtureConOctavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarCuartos("admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "GeneracionCruces")), Times.Once);
    }

    [TestMethod]
    public void GenerarSemifinalesSinFixtureLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns((Fixture)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarSemifinales("admin@test.com"));

        Assert.AreEqual("No existe un fixture generado.", ex.Message);
    }

    [TestMethod]
    public void GenerarSemifinalesSinCuartosJugadosLanzaExcepcion()
    {
        Fixture fixture = FixtureConOctavosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarSemifinales("admin@test.com"));

        Assert.AreEqual("Los cuartos no estan completos.", ex.Message);
    }

    [TestMethod]
    public void GenerarSemifinalesYaGeneradasLanzaExcepcion()
    {
        Fixture fixture = FixtureConCuartosJugados();
        foreach (Partido p in FixtureBuilder.CrearSemifinalesJugadas())
            fixture.AgregarPartidoEliminatoria(p);
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarSemifinales("admin@test.com"));

        Assert.AreEqual("Las semifinales ya fueron generadas.", ex.Message);
    }

    [TestMethod]
    public void GenerarSemifinalesCrea2PartidosOK()
    {
        Fixture fixture = FixtureConCuartosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarSemifinales("admin@test.com");

        int semis = fixture.PartidosEliminatorias.ToList()
            .Count(p => p.Fase == FasePartido.Semifinal);
        Assert.AreEqual(2, semis);
    }

    [TestMethod]
    public void GenerarSemifinalesEtiquetasCorrectasOK()
    {
        Fixture fixture = FixtureConCuartosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarSemifinales("admin@test.com");

        Assert.IsTrue(fixture.PartidosEliminatorias.ToList().Any(p => p.EtiquetaCruce == "S1"));
        Assert.IsTrue(fixture.PartidosEliminatorias.ToList().Any(p => p.EtiquetaCruce == "S2"));
    }

    [TestMethod]
    public void GenerarSemifinalesRegistraLogOK()
    {
        Fixture fixture = FixtureConCuartosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarSemifinales("admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "GeneracionCruces")), Times.Once);
    }
    

    [TestMethod]
    public void GenerarTercerPuestoYFinalSinFixtureLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns((Fixture)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarTercerPuestoYFinal("admin@test.com"));

        Assert.AreEqual("No existe un fixture generado.", ex.Message);
    }

    [TestMethod]
    public void GenerarTercerPuestoYFinalSinSemifinalesJugadasLanzaExcepcion()
    {
        Fixture fixture = FixtureConCuartosJugados();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarTercerPuestoYFinal("admin@test.com"));

        Assert.AreEqual("Las semifinales no estan completas.", ex.Message);
    }

    [TestMethod]
    public void GenerarTercerPuestoYFinalYaGeneradaLanzaExcepcion()
    {
        Fixture fixture = FixtureConSemifinalesJugadas();
        Equipo e1 = new Equipo("E1", Confederacion.UEFA, 1800);
        Equipo e2 = new Equipo("E2", Confederacion.CONMEBOL, 1500);
        Partido final = new Partido
        {
            Fecha = new DateTime(2026, 7, 20),
            Estadio = new Estadio { Nombre = "Est", Ciudad = "C", Capacidad = 20000 },
            EquipoLocal = e1,
            EquipoVisitante = e2,
            Grupo = "X",
            Fase = FasePartido.Final,
            EtiquetaCruce = "F"
        };
        fixture.AgregarPartidoEliminatoria(final);
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarTercerPuestoYFinal("admin@test.com"));

        Assert.AreEqual("La final ya fue generada.", ex.Message);
    }

    [TestMethod]
    public void GenerarTercerPuestoYFinalCrea2PartidosOK()
    {
        Fixture fixture = FixtureConSemifinalesJugadas();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(DosEstadios());

        _servicio.GenerarTercerPuestoYFinal("admin@test.com");

        bool hayTercerPuesto = fixture.PartidosEliminatorias.ToList()
            .Any(p => p.Fase == FasePartido.TercerPuesto);
        bool hayFinal = fixture.PartidosEliminatorias.ToList()
            .Any(p => p.Fase == FasePartido.Final);
        Assert.IsTrue(hayTercerPuesto, "Falta el partido de tercer puesto.");
        Assert.IsTrue(hayFinal, "Falta la final.");
    }

    [TestMethod]
    public void GenerarTercerPuestoYFinalEtiquetasCorrectasOK()
    {
        Fixture fixture = FixtureConSemifinalesJugadas();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(DosEstadios());

        _servicio.GenerarTercerPuestoYFinal("admin@test.com");

        Assert.IsTrue(fixture.PartidosEliminatorias.ToList().Any(p => p.EtiquetaCruce == "TP"));
        Assert.IsTrue(fixture.PartidosEliminatorias.ToList().Any(p => p.EtiquetaCruce == "F"));
    }

    [TestMethod]
    public void GenerarTercerPuestoYFinalConUnEstadioReutilizaElMismoOK()
    {
        Fixture fixture = FixtureConSemifinalesJugadas();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(UnEstadio());

        _servicio.GenerarTercerPuestoYFinal("admin@test.com");

        Partido tercerPuesto = fixture.PartidosEliminatorias.ToList()
            .First(p => p.Fase == FasePartido.TercerPuesto);
        Partido finalPartido = fixture.PartidosEliminatorias.ToList()
            .First(p => p.Fase == FasePartido.Final);

        Assert.AreEqual("Estadio A", tercerPuesto.Estadio.Nombre);
        Assert.AreEqual("Estadio A", finalPartido.Estadio.Nombre);
    }

    [TestMethod]
    public void GenerarTercerPuestoYFinalRegistraLogOK()
    {
        Fixture fixture = FixtureConSemifinalesJugadas();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(DosEstadios());

        _servicio.GenerarTercerPuestoYFinal("admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "GeneracionCruces")), Times.Once);
    }

    [TestMethod]
    public void SimularFase_PersisteCadaPartidoSimulado_LlamaActualizarPartidoOK()
    {
        Equipo local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 1 };
        Equipo visitante = new Equipo("Visit", Confederacion.CONMEBOL, 1500) { Id = 2 };
        Partido partido = new Partido
        {
            Id = 101,
            Fecha = new DateTime(2026, 7, 1, 16, 0, 0),
            Estadio = new Estadio { Nombre = "Estadio", Ciudad = "Ciudad", Capacidad = 20000 },
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = "X",
            Fase = FasePartido.DieciseisavosDeFinal,
            EtiquetaCruce = "A1"
        };
        Fixture fixture = new FixtureBuilder()
            .ConMotor(NombresMotores.AleatorioPuro)
            .ConPartidosEliminatorios(new List<Partido> { partido })
            .Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        _servicio.SimularFase(FasePartido.DieciseisavosDeFinal, ConfiguracionTorneo.PorDefecto(), "user@test.com");

        _partidoRepoMock.Verify(r => r.Actualizar(It.IsAny<Partido>()), Times.AtLeastOnce);
    }

    [TestMethod]
    public void SimularFase_RegistraLogDeSimulacion_OKConEmailYAccion()
    {
        Equipo local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 1 };
        Equipo visitante = new Equipo("Visit", Confederacion.CONMEBOL, 1500) { Id = 2 };
        Partido partido = new Partido
        {
            Id = 102,
            Fecha = new DateTime(2026, 7, 1, 16, 0, 0),
            Estadio = new Estadio { Nombre = "Estadio", Ciudad = "Ciudad", Capacidad = 20000 },
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = "X",
            Fase = FasePartido.DieciseisavosDeFinal,
            EtiquetaCruce = "A1"
        };
        Fixture fixture = new FixtureBuilder()
            .ConMotor(NombresMotores.AleatorioPuro)
            .ConPartidosEliminatorios(new List<Partido> { partido })
            .Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        _servicio.SimularFase(FasePartido.DieciseisavosDeFinal, ConfiguracionTorneo.PorDefecto(), "editor@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "SimulacionFase" && l.UsuarioEmail == "editor@test.com"
        )), Times.Once);
    }

    [TestMethod]
    public void SimularFase_UsaMotorDelFixtureOK()
    {
        Equipo local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 1 };
        Equipo visitante = new Equipo("Visit", Confederacion.CONMEBOL, 1500) { Id = 2 };
        Partido partido = new Partido
        {
            Id = 103,
            Fecha = new DateTime(2026, 7, 1, 16, 0, 0),
            Estadio = new Estadio { Nombre = "Estadio", Ciudad = "Ciudad", Capacidad = 20000 },
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = "X",
            Fase = FasePartido.DieciseisavosDeFinal,
            EtiquetaCruce = "A1"
        };

        Fixture fixture = new FixtureBuilder()
            .ConMotor(NombresMotores.AleatorioPuro)
            .ConPartidosEliminatorios(new List<Partido> { partido })
            .Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        FixtureDTO resultado = _servicio.SimularFase(FasePartido.DieciseisavosDeFinal, ConfiguracionTorneo.PorDefecto(), "user@test.com");

        PartidoDTO partidoResultado = resultado.PartidosEliminatorias.First(p => p.EtiquetaCruce == "A1");
        Assert.IsTrue(partidoResultado.EstaJugado);
    }

    [TestMethod]
    public void SimularFase_PartidoJugado_RegistraLogCambioRanking()
    {
        Equipo local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 1 };
        Equipo visitante = new Equipo("Visit", Confederacion.CONMEBOL, 1500) { Id = 2 };
        Partido partido = new Partido
        {
            Id = 104,
            Fecha = new DateTime(2026, 7, 1, 16, 0, 0),
            Estadio = new Estadio { Nombre = "Estadio", Ciudad = "Ciudad", Capacidad = 20000 },
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = "X",
            Fase = FasePartido.DieciseisavosDeFinal,
            EtiquetaCruce = "A1"
        };
        Fixture fixture = new FixtureBuilder()
            .ConMotor(NombresMotores.AleatorioPuro)
            .ConPartidosEliminatorios(new List<Partido> { partido })
            .Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        _servicio.SimularFase(FasePartido.DieciseisavosDeFinal, ConfiguracionTorneo.PorDefecto(), "editor@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "CambioRanking"
        )), Times.Once, "SimularFase debe registrar el cambio de ranking por cada partido jugado");
    }
}
