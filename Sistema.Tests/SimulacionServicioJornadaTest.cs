using Dominio;
using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Sistema.Tests
{
    [TestClass]
    public class SimulacionServicioJornadaTest
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

        private Equipo CrearEquipo(string nombre, int id, int ranking)
        {
            Equipo e = new Equipo(nombre, Confederacion.UEFA, ranking);
            e.Id = id;
            return e;
        }

        [TestMethod]
        public void SimularJornada_SimulaTodosLosPartidos()
        {
            Jornada jornada = new Jornada(1, DateTime.Now);
            Equipo e1 = CrearEquipo("E1", 1, 2000);
            Equipo e2 = CrearEquipo("E2", 2, 1900);
            Equipo e3 = CrearEquipo("E3", 3, 1800);
            Equipo e4 = CrearEquipo("E4", 4, 1700);

            Partido p1 = new Partido { Estadio = CrearEstadio(), EquipoLocal = e1, EquipoVisitante = e2 };
            Partido p2 = new Partido { Estadio = CrearEstadio(), EquipoLocal = e3, EquipoVisitante = e4 };
            Partido p3 = new Partido { Estadio = CrearEstadio(), EquipoLocal = e1, EquipoVisitante = e3 };

            jornada.AgregarPartido(p1);
            jornada.AgregarPartido(p2);
            jornada.AgregarPartido(p3);

            ConfiguracionTorneo config = ConfiguracionTorneo.PorDefecto() with { SemillaSimulation = 42 };

            _servicio.SimularJornada(jornada, config, _motor);

            Assert.IsTrue(p1.EstaJugado());
            Assert.IsTrue(p2.EstaJugado());
            Assert.IsTrue(p3.EstaJugado());
        }
    }
}
