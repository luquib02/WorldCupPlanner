using Dominio;
using Dominio.Simulacion;
using System;
using System.Collections.Generic;

namespace Sistema
{
    public class SimulacionServicio
    {
        private const int MULTIPLICADOR_SEMILLA = 31;
        private const int SHIFT_ID = 8;
        private const int TARJETAS_AMARILLAS_MAX = 5;
        private const int TARJETAS_ROJAS_MAX = 2;
        private const int MINUTO_MIN = 1;
        private const int MINUTO_MAX = 91;

        public void SimularPartido(Partido partido, ConfiguracionTorneo configuracion, IMotorSimulacion motor)
        {
            if (partido == null) throw new ArgumentNullException(nameof(partido));
            if (configuracion == null) throw new ArgumentNullException(nameof(configuracion));
            if (motor == null) throw new ArgumentNullException(nameof(motor));

            Random rnd = CrearRandomParaPartido(partido, configuracion);
            (int gLocal, int gVisitante) = motor.GenerarMarcador(partido.EquipoLocal, partido.EquipoVisitante, rnd);
            partido.CargarResultado(gLocal, gVisitante);

            if (gLocal == gVisitante && partido.Fase != FasePartido.FaseDeGrupos)
                SimularPenales(partido, rnd);

            GenerarIncidencias(partido, rnd);
        }

        private void SimularPenales(Partido partido, Random rnd)
        {
            int penalesLocal = rnd.Next(3, 8);
            int penalesVisitante;
            do
            {
                penalesVisitante = rnd.Next(3, 8);
            }
            while (penalesVisitante == penalesLocal);

            partido.CargarPenales(penalesLocal, penalesVisitante);
        }

        public void SimularJornada(Jornada jornada, ConfiguracionTorneo configuracion, IMotorSimulacion motor)
        {
            if (jornada == null) throw new ArgumentNullException(nameof(jornada));
            if (configuracion == null) throw new ArgumentNullException(nameof(configuracion));
            if (motor == null) throw new ArgumentNullException(nameof(motor));

            SimularPartidos(jornada.Partidos, configuracion, motor);
        }

        public void SimularFase(Fixture fixture, ConfiguracionTorneo configuracion, IMotorSimulacion motor)
        {
            if (fixture == null) throw new ArgumentNullException(nameof(fixture));
            if (configuracion == null) throw new ArgumentNullException(nameof(configuracion));
            if (motor == null) throw new ArgumentNullException(nameof(motor));

            SimularPartidos(fixture.ObtenerTodosLosPartidos(), configuracion, motor);
        }

        public void SimularTorneo(Fixture fixture, ConfiguracionTorneo configuracion, IMotorSimulacion motor)
        {
            SimularFase(fixture, configuracion, motor);
        }

        private void SimularPartidos(IEnumerable<Partido> partidos, ConfiguracionTorneo configuracion, IMotorSimulacion motor)
        {
            foreach (Partido partido in partidos)
            {
                if (!partido.EstaJugado())
                {
                    SimularPartido(partido, configuracion, motor);
                }
            }
        }

        private Random CrearRandomParaPartido(Partido partido, ConfiguracionTorneo configuracion)
        {
            int semilla = ConstruirSemilla(configuracion.SemillaSimulation, partido.EquipoLocal.Id, partido.EquipoVisitante.Id);
            return new Random(semilla);
        }

        private int ConstruirSemilla(int semillaBase, int idLocal, int idVisitante)
        {
            // Combina la semilla base con los ids del partido en un int estable y legible.
            return unchecked(semillaBase * MULTIPLICADOR_SEMILLA + (idLocal << SHIFT_ID) + idVisitante);
        }

        private void GenerarIncidencias(Partido partido, Random rnd)
        {
            int amarillasLocal     = rnd.Next(0, TARJETAS_AMARILLAS_MAX);
            int amarillasVisitante = rnd.Next(0, TARJETAS_AMARILLAS_MAX);
            int rojasLocal         = rnd.Next(0, TARJETAS_ROJAS_MAX);
            int rojasVisitante     = rnd.Next(0, TARJETAS_ROJAS_MAX);

            for (int i = 0; i < amarillasLocal;     i++) partido.AgregarIncidencia(new TarjetaAmarilla(rnd.Next(MINUTO_MIN, MINUTO_MAX), esLocal: true));
            for (int i = 0; i < amarillasVisitante; i++) partido.AgregarIncidencia(new TarjetaAmarilla(rnd.Next(MINUTO_MIN, MINUTO_MAX), esLocal: false));
            for (int i = 0; i < rojasLocal;         i++) partido.AgregarIncidencia(new TarjetaRoja(rnd.Next(MINUTO_MIN, MINUTO_MAX),     esLocal: true));
            for (int i = 0; i < rojasVisitante;     i++) partido.AgregarIncidencia(new TarjetaRoja(rnd.Next(MINUTO_MIN, MINUTO_MAX),     esLocal: false));
        }
    }
}
