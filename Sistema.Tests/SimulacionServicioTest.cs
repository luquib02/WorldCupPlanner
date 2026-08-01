using Dominio;
using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace Sistema.Tests
{
    [TestClass]
    public class SimulacionServicioTest
    {
        private SimulacionServicio _servicio;
        private IMotorSimulacion _motor;

        [TestInitialize]
        public void Setup()
        {
            _servicio = new SimulacionServicio();
            _motor = new MotorProbabilistico();
        }

        private Estadio CrearEstadio() => new Estadio { Nombre = "E", Ciudad = "C", Capacidad = 30000 };

        [TestMethod]
        public void SimularPartido_GeneraMarcadorValido()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };

            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 12345 };

            _servicio.SimularPartido(partido, config, _motor);

            Assert.IsTrue(partido.EstaJugado());
            Assert.IsTrue(partido.GolesLocal >= 0);
            Assert.IsTrue(partido.GolesVisitante >= 0);
        }

        [TestMethod]
        public void SimularPartido_DiferenteSemilla_ProduceResultadosDistintos()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido1 = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var partido2 = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };

            var config1 = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 1 };
            var config2 = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 2 };

            _servicio.SimularPartido(partido1, config1, _motor);
            _servicio.SimularPartido(partido2, config2, _motor);

            Assert.IsTrue(partido1.GolesLocal != partido2.GolesLocal || partido1.GolesVisitante != partido2.GolesVisitante);
        }

        [TestMethod]
        public void SimularPartido_MismoSeed_ResultadoDeterminista()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 10 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1700) { Id = 11 };

            var partido1 = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var partido2 = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };

            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 777 };

            _servicio.SimularPartido(partido1, config, _motor);
            _servicio.SimularPartido(partido2, config, _motor);

            Assert.AreEqual(partido1.GolesLocal, partido2.GolesLocal);
            Assert.AreEqual(partido1.GolesVisitante, partido2.GolesVisitante);
        }

        [TestMethod]
        public void SimularPartido_NoActualizaRankingActual()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 1900) { Id = 20 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1600) { Id = 21 };

            int rLocalAntes = local.RankingActual;
            int rVisitAntes = visitante.RankingActual;

            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 999 };

            _servicio.SimularPartido(partido, config, _motor);

            Assert.AreEqual(rLocalAntes, local.RankingActual);
            Assert.AreEqual(rVisitAntes, visitante.RankingActual);
        }

        [TestMethod]
        public void SimularPartido_GeneraIncidenciasEnElPartido()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1700) { Id = 2 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 1 };
            IMotorSimulacion motorAleatorio = new MotorAleatorioPuro();

            _servicio.SimularPartido(partido, config, motorAleatorio);

            Assert.IsTrue(partido.Incidencias.Count > 0,
                "La simulación debe generar al menos una incidencia en el partido");
        }

        [TestMethod]
        public void SimularPartido_TodasLasIncidenciasSonTarjetasConMinutoValido()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 5 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1700) { Id = 6 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 1 };
            IMotorSimulacion motorAleatorio = new MotorAleatorioPuro();

            _servicio.SimularPartido(partido, config, motorAleatorio);

            Assert.IsTrue(
                partido.Incidencias.All(i => i is TarjetaAmarilla || i is TarjetaRoja),
                "Todas las incidencias deben ser tarjetas amarillas o rojas");
            Assert.IsTrue(
                partido.Incidencias.All(i => i.MinutoJuego >= 1 && i.MinutoJuego <= 90),
                "El minuto de juego debe estar entre 1 y 90");
        }

        [TestMethod]
        public void SimularPartido_IncidenciasMismaSemillaResultadoDeterminista()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 1800) { Id = 10 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1700) { Id = 11 };
            var partido1 = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var partido2 = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 777 };

            _servicio.SimularPartido(partido1, config, _motor);
            _servicio.SimularPartido(partido2, config, _motor);

            Assert.AreEqual(partido1.Incidencias.Count, partido2.Incidencias.Count,
                "Con la misma semilla el número de incidencias debe ser idéntico");
            Assert.AreEqual(
                partido1.Incidencias.Count(i => i is TarjetaAmarilla),
                partido2.Incidencias.Count(i => i is TarjetaAmarilla),
                "Con la misma semilla la cantidad de tarjetas amarillas debe ser igual");
        }

        [TestMethod]
        public void SimularPartido_PartidoNulo_LanzaArgumentNullException()
        {
            var config = ConfiguracionTorneo.PorDefecto();

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularPartido(null!, config, _motor));
        }

        [TestMethod]
        public void SimularPartido_ConfiguracionNula_LanzaArgumentNullException()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularPartido(partido, null!, _motor));
        }

        [TestMethod]
        public void SimularPartido_MotorNulo_LanzaArgumentNullException()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var config = ConfiguracionTorneo.PorDefecto();

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularPartido(partido, config, null!));
        }

        [TestMethod]
        public void SimularPartido_EmpateEnFaseEliminatoria_GeneraPenales()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido
            {
                Estadio = CrearEstadio(),
                EquipoLocal = local,
                EquipoVisitante = visitante,
                Fase = FasePartido.OctavosDeFinal
            };
            var config = ConfiguracionTorneo.PorDefecto();

            var motorMock = new Mock<IMotorSimulacion>();
            motorMock
                .Setup(m => m.GenerarMarcador(local, visitante, It.IsAny<Random>()))
                .Returns((1, 1));

            _servicio.SimularPartido(partido, config, motorMock.Object);

            Assert.AreEqual(1, partido.GolesLocal);
            Assert.AreEqual(1, partido.GolesVisitante);
            Assert.IsNotNull(partido.GolesPenalesLocal);
            Assert.IsNotNull(partido.GolesPenalesVisitante);
            Assert.AreNotEqual(partido.GolesPenalesLocal, partido.GolesPenalesVisitante);
        }

        [TestMethod]
        public void SimularPartido_EmpateEnFaseDeGrupos_NoGeneraPenales()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido
            {
                Estadio = CrearEstadio(),
                EquipoLocal = local,
                EquipoVisitante = visitante,
                Fase = FasePartido.FaseDeGrupos
            };
            var config = ConfiguracionTorneo.PorDefecto();

            var motorMock = new Mock<IMotorSimulacion>();
            motorMock
                .Setup(m => m.GenerarMarcador(local, visitante, It.IsAny<Random>()))
                .Returns((1, 1));

            _servicio.SimularPartido(partido, config, motorMock.Object);

            Assert.IsNull(partido.GolesPenalesLocal);
            Assert.IsNull(partido.GolesPenalesVisitante);
        }

        [TestMethod]
        public void SimularJornada_JornadaNula_LanzaArgumentNullException()
        {
            var config = ConfiguracionTorneo.PorDefecto();

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularJornada(null!, config, _motor));
        }

        [TestMethod]
        public void SimularJornada_ConfiguracionNula_LanzaArgumentNullException()
        {
            var jornada = new Jornada(1, new DateTime(2026, 6, 1));

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularJornada(jornada, null!, _motor));
        }

        [TestMethod]
        public void SimularJornada_MotorNulo_LanzaArgumentNullException()
        {
            var jornada = new Jornada(1, new DateTime(2026, 6, 1));
            var config = ConfiguracionTorneo.PorDefecto();

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularJornada(jornada, config, null!));
        }

        [TestMethod]
        public void SimularJornada_SimulaTodosLosPartidosNoJugados()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var jornada = new Jornada(1, new DateTime(2026, 6, 1));
            jornada.AgregarPartido(partido);
            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 5 };

            _servicio.SimularJornada(jornada, config, _motor);

            Assert.IsTrue(partido.EstaJugado());
        }

        [TestMethod]
        public void SimularJornada_NoResimulaPartidosYaJugados()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            partido.CargarResultado(3, 1);
            var jornada = new Jornada(1, new DateTime(2026, 6, 1));
            jornada.AgregarPartido(partido);
            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 5 };

            _servicio.SimularJornada(jornada, config, _motor);

            Assert.AreEqual(3, partido.GolesLocal);
            Assert.AreEqual(1, partido.GolesVisitante);
        }

        [TestMethod]
        public void SimularFase_FixtureNulo_LanzaArgumentNullException()
        {
            var config = ConfiguracionTorneo.PorDefecto();

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularFase(null!, config, _motor));
        }

        [TestMethod]
        public void SimularFase_ConfiguracionNula_LanzaArgumentNullException()
        {
            var fixture = new Fixture(0);

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularFase(fixture, null!, _motor));
        }

        [TestMethod]
        public void SimularFase_MotorNulo_LanzaArgumentNullException()
        {
            var fixture = new Fixture(0);
            var config = ConfiguracionTorneo.PorDefecto();

            Assert.Throws<ArgumentNullException>(
                () => _servicio.SimularFase(fixture, config, null!));
        }

        [TestMethod]
        public void SimularFase_SimulaTodosLosPartidosDelFixture()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partidoGrupo = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var jornada = new Jornada(1, new DateTime(2026, 6, 1));
            jornada.AgregarPartido(partidoGrupo);
            var grupo = new Grupo("A");
            grupo.AgregarJornada(jornada);

            var localElim = new Equipo("LocalElim", Confederacion.UEFA, 1900) { Id = 3 };
            var visitanteElim = new Equipo("VisitElim", Confederacion.UEFA, 1600) { Id = 4 };
            var partidoEliminatoria = new Partido
            {
                Estadio = CrearEstadio(),
                EquipoLocal = localElim,
                EquipoVisitante = visitanteElim,
                Fase = FasePartido.OctavosDeFinal
            };

            var fixture = new Fixture(0);
            fixture.AgregarGrupo(grupo);
            fixture.AgregarPartidoEliminatoria(partidoEliminatoria);

            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 5 };

            _servicio.SimularFase(fixture, config, _motor);

            Assert.IsTrue(partidoGrupo.EstaJugado());
            Assert.IsTrue(partidoEliminatoria.EstaJugado());
        }

        [TestMethod]
        public void SimularTorneo_DelegaEnSimularFaseYSimulaLosPartidos()
        {
            var local = new Equipo("Local", Confederacion.UEFA, 2000) { Id = 1 };
            var visitante = new Equipo("Visit", Confederacion.UEFA, 1500) { Id = 2 };
            var partido = new Partido { Estadio = CrearEstadio(), EquipoLocal = local, EquipoVisitante = visitante };
            var jornada = new Jornada(1, new DateTime(2026, 6, 1));
            jornada.AgregarPartido(partido);
            var grupo = new Grupo("A");
            grupo.AgregarJornada(jornada);

            var fixture = new Fixture(0);
            fixture.AgregarGrupo(grupo);

            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 5 };

            _servicio.SimularTorneo(fixture, config, _motor);

            Assert.IsTrue(partido.EstaJugado());
        }
    }
}
