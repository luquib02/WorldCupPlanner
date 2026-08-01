using Dominio;
using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sistema.Tests
{
    [TestClass]
    public class SimulacionServicioTorneoTest
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
        public void SimularTorneo_NoActualizaRankingActual()
        {
            var equipo1 = new Equipo("Eq1", Confederacion.CONMEBOL, 1800) { Id = 1 };
            var equipo2 = new Equipo("Eq2", Confederacion.CONMEBOL, 1700) { Id = 2 };
            var equipo3 = new Equipo("Eq3", Confederacion.UEFA, 1600) { Id = 3 };
            var equipo4 = new Equipo("Eq4", Confederacion.UEFA, 1550) { Id = 4 };

            int r1Antes = equipo1.RankingActual;
            int r2Antes = equipo2.RankingActual;
            int r3Antes = equipo3.RankingActual;
            int r4Antes = equipo4.RankingActual;

            var estadio = CrearEstadio();

            var partido1 = new Partido { Estadio = estadio, EquipoLocal = equipo1, EquipoVisitante = equipo2 };
            var partido2 = new Partido { Estadio = estadio, EquipoLocal = equipo3, EquipoVisitante = equipo4 };

            var fixture = new Fixture(42);
            fixture.AgregarPartidoEliminatoria(partido1);
            fixture.AgregarPartidoEliminatoria(partido2);

            var config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 999 };

            _servicio.SimularTorneo(fixture, config, _motor);

            Assert.AreEqual(r1Antes, equipo1.RankingActual);
            Assert.AreEqual(r2Antes, equipo2.RankingActual);
            Assert.AreEqual(r3Antes, equipo3.RankingActual);
            Assert.AreEqual(r4Antes, equipo4.RankingActual);
        }
    }
}
