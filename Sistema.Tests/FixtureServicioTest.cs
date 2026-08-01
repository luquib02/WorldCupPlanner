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

namespace Sistema.Tests;

[TestClass]
public class FixtureServicioTest
{
    private Mock<IFixtureRepositorio> _fixtureRepoMock;
    private Mock<IEquipoRepositorio> _equipoRepoMock;
    private Mock<IRepositorioEstadio> _estadioRepoMock;
    private Mock<IRepositorioLog> _logRepoMock;
    private Mock<IPartidoRepositorio>  _partidoMock; 
    private FixtureServicio _servicio;

    [TestInitialize]
    public void Setup()
    {
        _fixtureRepoMock = new Mock<IFixtureRepositorio>();
        _equipoRepoMock = new Mock<IEquipoRepositorio>();
        _estadioRepoMock = new Mock<IRepositorioEstadio>();
        _logRepoMock = new Mock<IRepositorioLog>();
        _partidoMock = new Mock<IPartidoRepositorio>();  
        LogServicio logServicio = new LogServicio(_logRepoMock.Object);
        Mock<IRepositorioNotificacion> notificacionRepoMock = new Mock<IRepositorioNotificacion>();
        Mock<IRepositorioUsuario> usuarioRepoMock = new Mock<IRepositorioUsuario>();
        usuarioRepoMock.Setup(r => r.ObtenerListaUsuarios()).Returns(new List<Usuario>());
        NotificacionServicio notificacionServicio = new NotificacionServicio(
            notificacionRepoMock.Object, usuarioRepoMock.Object, new ServicioSesion(), logServicio);
        ResultadoPartidoServicio resultadoServicio = new ResultadoPartidoServicio(
            _equipoRepoMock.Object, _partidoMock.Object, logServicio, new CalculadoraRankingElo(), notificacionServicio);
        _servicio = new FixtureServicio(
            _fixtureRepoMock.Object,
            _equipoRepoMock.Object,
            _estadioRepoMock.Object,
            _partidoMock.Object,
            logServicio,
            new SimulacionServicio(),
            resultadoServicio);
    }

    private List<Equipo> Crear48EquiposValidos()
    {
        List<Equipo> equipos = new List<Equipo>();
        Confederacion[] confederaciones = {
            Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA,
            Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA,
            Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA,
            Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA, Confederacion.UEFA,
            Confederacion.CONMEBOL, Confederacion.CONMEBOL, Confederacion.CONMEBOL,
            Confederacion.CONMEBOL, Confederacion.CONMEBOL, Confederacion.CONMEBOL, Confederacion.CONMEBOL,
            Confederacion.CONCACAF, Confederacion.CONCACAF, Confederacion.CONCACAF,
            Confederacion.CONCACAF, Confederacion.CONCACAF, Confederacion.CONCACAF, Confederacion.CONCACAF,
            Confederacion.CAF, Confederacion.CAF, Confederacion.CAF, Confederacion.CAF,
            Confederacion.CAF, Confederacion.CAF, Confederacion.CAF, Confederacion.CAF, Confederacion.CAF,
            Confederacion.AFC, Confederacion.AFC, Confederacion.AFC, Confederacion.AFC,
            Confederacion.AFC, Confederacion.AFC, Confederacion.AFC, Confederacion.AFC,
            Confederacion.OFC
        };

        for (int i = 0; i < 48; i++)
        {
            Equipo equipo = new Equipo($"Equipo{i + 1:D2}", confederaciones[i], 2500 - i * 40);
            equipo.Id = i + 1;
            equipos.Add(equipo);
        }
        return equipos;
    }

    private List<Estadio> Crear4Estadios()
    {
        return new List<Estadio>
        {
            CrearEstadio(1, "Estadio Alfa"),
            CrearEstadio(2, "Estadio Beta"),
            CrearEstadio(3, "Estadio Gamma"),
            CrearEstadio(4, "Estadio Delta")
        };
    }

    private Estadio CrearEstadio(int id, string nombre)
    {
        Estadio e = new Estadio { Nombre = nombre, Ciudad = "Ciudad", Capacidad = 50000 };
        e.Id = id;
        return e;
    }

    private ConfiguracionTorneo ConfigDefault() => ConfiguracionTorneo.PorDefecto();
    
    [TestMethod]
    public void GenerarFixtureConMenosDe48EquiposLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(new List<Equipo>());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarFixture(ConfigDefault(), "admin@test.com"));
        Assert.AreEqual("Se necesitan exactamente 48 equipos para generar el fixture.", ex.Message);
    }
    
    [TestMethod]
    public void GenerarFixtureConMenosDe4EstadiosLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(new List<Estadio> { CrearEstadio(1, "Solo") });

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarFixture(ConfigDefault(), "admin@test.com"));
        Assert.AreEqual("Se necesitan al menos 4 estadios para generar el fixture.", ex.Message);
    }
    
    [TestMethod]
    public void GenerarFixtureCuandoYaExisteLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(true);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarFixture(ConfigDefault(), "admin@test.com"));
        Assert.AreEqual("Ya existe un fixture generado.", ex.Message);
    }
    
    [TestMethod]
    public void GenerarFixtureCrea12GruposOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        FixtureDTO fixture = _servicio.GenerarFixture(ConfigDefault(), "admin@test.com");

        Assert.AreEqual(12, fixture.Grupos.Count);
    }
    
    [TestMethod]
    public void GenerarFixtureCadaGrupoTiene4EquiposOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        FixtureDTO fixture = _servicio.GenerarFixture(ConfigDefault(), "admin@test.com");

        foreach (GrupoDTO g in fixture.Grupos)
            Assert.AreEqual(4, g.Equipos.Count);
    }
    
    [TestMethod]
    public void GenerarFixtureCadaGrupoTiene6PartidosOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        FixtureDTO fixture = _servicio.GenerarFixture(ConfigDefault(), "admin@test.com");

        foreach (GrupoDTO g in fixture.Grupos)
            Assert.AreEqual(6, g.Jornadas.SelectMany(j => j.Partidos).Count());
    }
    
    [TestMethod]
    public void GenerarFixtureGenera72PartidosOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        FixtureDTO fixture = _servicio.GenerarFixture(ConfigDefault(), "admin@test.com");

        int totalPartidos = fixture.Grupos.Sum(g => g.Jornadas.SelectMany(j => j.Partidos).Count());
        Assert.AreEqual(72, totalPartidos);
    }
    
    [TestMethod]
    public void GenerarFixtureGuardaEnRepositorioOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        _servicio.GenerarFixture(ConfigDefault(), "admin@test.com");

        _fixtureRepoMock.Verify(r => r.Guardar(It.IsAny<Fixture>()), Times.Once);
    }
    
    [TestMethod]
    public void GenerarFixtureRegistraLogOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        _servicio.GenerarFixture(ConfigDefault(), "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "GeneracionFixture" && l.UsuarioEmail == "admin@test.com"
        )), Times.Once);
    }
    
    [TestMethod]
    public void GenerarFixtureMismaSemillaProduceMismoOrdenOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());
        ConfiguracionTorneo config = ConfigDefault() with { SemillaFixture = 42 };

        FixtureDTO f1 = _servicio.GenerarFixture(config, "admin@test.com");

        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        FixtureDTO f2 = _servicio.GenerarFixture(config, "admin@test.com");

        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.AreEqual(
                    f1.Grupos[i].Equipos[j].Nombre,
                    f2.Grupos[i].Equipos[j].Nombre);
            }
        }
    }
    
    [TestMethod]
    public void ObtenerFixtureCuandoNoExisteLanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns((Fixture?)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.ObtenerFixture());
        Assert.AreEqual("No existe un fixture generado.", ex.Message);
    }

    [TestMethod]
    public void EstaFaseGruposCompletaConPartidosSinJugarEsFalsoOK()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        Assert.IsFalse(_servicio.EstaFaseGruposCompleta());
    }

    [TestMethod]
    public void EstaFaseGruposCompletaConTodosLosPartidosJugadosEsTrueOK()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().ConResultadosCargados().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        Assert.IsTrue(_servicio.EstaFaseGruposCompleta());
    }

    [TestMethod]
    public void ObtenerTablaPosicionesDeGrupoInexistenteLanzaExcepcion()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().ConResultadosCargados().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.ObtenerTablaPosiciones("Z"));
        Assert.AreEqual("No existe el grupo Z.", ex.Message);
    }

    [TestMethod]
    public void ObtenerTablaPosicionesDevuelve4EquiposOrdenadosOK()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().ConResultadosCargados().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        List<PosicionEquipoDTO> tabla = _servicio.ObtenerTablaPosiciones("A");

        Assert.AreEqual(4, tabla.Count);
        for (int i = 0; i < tabla.Count - 1; i++)
            Assert.IsTrue(tabla[i].Puntos >= tabla[i + 1].Puntos);
    }

    [TestMethod]
    public void SimularGrupoDeGrupoInexistenteLanzaExcepcion()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.SimularGrupo("Z", ConfigDefault(), "user@test.com"));
        Assert.AreEqual("No existe el grupo Z.", ex.Message);
    }

    [TestMethod]
    public void SimularGrupoDejaTodosLosPartidosDelGrupoJugadosOK()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        FixtureDTO resultado = _servicio.SimularGrupo("A", ConfigDefault(), "user@test.com");

        GrupoDTO grupoA = resultado.Grupos.First(g => g.Etiqueta == "A");
        Assert.IsTrue(grupoA.Jornadas.SelectMany(j => j.Partidos).All(p => p.EstaJugado));
    }

    [TestMethod]
    public void SimularFaseDeGruposDejaTodosLosPartidosJugadosOK()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        FixtureDTO resultado = _servicio.SimularFaseDeGrupos(ConfigDefault(), "user@test.com");

        Assert.IsTrue(resultado.Grupos.SelectMany(g => g.Jornadas).SelectMany(j => j.Partidos).All(p => p.EstaJugado));
    }

    [TestMethod]
    public void GenerarFixture_MotorAleatorioPuro_PersisteNombreMotorEnFixtureOK()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        FixtureDTO fixture = _servicio.GenerarFixture(ConfigDefault(), "admin@test.com", NombresMotores.AleatorioPuro);

        Assert.AreEqual(NombresMotores.AleatorioPuro, fixture.NombreMotorSimulacion);
    }

    [TestMethod]
    public void GenerarFixture_MotorInexistente_LanzaExcepcion()
    {
        _fixtureRepoMock.Setup(r => r.Existe()).Returns(false);
        _equipoRepoMock.Setup(r => r.ObtenerTodos()).Returns(Crear48EquiposValidos());
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(Crear4Estadios());

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.GenerarFixture(ConfigDefault(), "admin@test.com", "Inexistente"));
        Assert.AreEqual("No existe el motor de simulación 'Inexistente'.", ex.Message);
    }

    [TestMethod]
    public void SimularGrupo_UsaMotorDelFixtureOK()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().ConMotor(NombresMotores.AleatorioPuro).Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        FixtureDTO resultado = _servicio.SimularGrupo("A", ConfigDefault(), "user@test.com");

        GrupoDTO grupoA = resultado.Grupos.First(g => g.Etiqueta == "A");
        Assert.IsTrue(grupoA.Jornadas.SelectMany(j => j.Partidos).All(p => p.EstaJugado));
    }

    [TestMethod]
    public void ObtenerMotoresDisponiblesDevuelveAmbosMotoresOK()
    {
        List<string> motores = _servicio.ObtenerMotoresDisponibles();

        Assert.AreEqual(2, motores.Count);
        Assert.IsTrue(motores.Contains(NombresMotores.Probabilistico));
        Assert.IsTrue(motores.Contains(NombresMotores.AleatorioPuro));
    }

    [TestMethod]
    public void SimularGrupo_PersisteCadaPartidoSimulado()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        _servicio.SimularGrupo("A", ConfigDefault(), "user@test.com");

        _partidoMock.Verify(r => r.Actualizar(It.IsAny<Partido>()), Times.AtLeastOnce,
            "Cada partido simulado debe persistirse con Actualizar");
    }

    [TestMethod]
    public void SimularGrupo_ActualizaRankingEquiposPostSimulacion()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        _servicio.SimularGrupo("A", ConfigDefault(), "user@test.com");

        _equipoRepoMock.Verify(r => r.Actualizar(It.IsAny<Equipo>()), Times.AtLeastOnce,
            "El ranking de los equipos debe actualizarse y persistirse luego de la simulación");
    }

    [TestMethod]
    public void SimularGrupo_RegistraLogDeSimulacion()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        _servicio.SimularGrupo("A", ConfigDefault(), "user@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "SimulacionGrupo" && l.UsuarioEmail == "user@test.com")),
            Times.Once,
            "Debe registrarse un log de auditoría con la simulación del grupo");
    }

    [TestMethod]
    public void SimularGrupo_PartidoJugado_RegistraLogCambioRanking()
    {
        Fixture fixture = new Sistema.Tests.Helpers.FixtureBuilder().ConMotor(NombresMotores.AleatorioPuro).Construir();
        _fixtureRepoMock.Setup(r => r.ObtenerActual()).Returns(fixture);

        _servicio.SimularGrupo("A", ConfigDefault(), "user@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "CambioRanking"
        )), Times.AtLeastOnce, "SimularGrupo debe registrar el cambio de ranking por cada partido jugado");
    }
}