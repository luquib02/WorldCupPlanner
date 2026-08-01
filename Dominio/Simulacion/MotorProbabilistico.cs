using System;

namespace Dominio.Simulacion;

public class MotorProbabilistico : IMotorSimulacion
{
    private const double PROBABILIDAD_BASE = 0.5;
    private const double FACTOR_RANKING = 0.5;
    private const double PROBABILIDAD_MAXIMA = 0.95;
    private const double PROBABILIDAD_MINIMA = 0.05;
    private const double UMBRAL_EMPATE = 0.1;
    private const int GOLES_GANADOR_MIN = 1;
    private const int GOLES_GANADOR_MAX = 4;   // exclusivo: el ganador marca entre 1 y 3
    private const int GOLES_EMPATE_MAX = 3;    // exclusivo: el empate va de 0 a 2

    public string Nombre => NombresMotores.Probabilistico;

    public (int golesLocal, int golesVisitante) GenerarMarcador(Equipo local, Equipo visitante, Random random)
    {
        double diff = local.RankingActual - visitante.RankingActual;
        double denom = Math.Max(1, local.RankingActual + visitante.RankingActual);
        double probLocal = PROBABILIDAD_BASE + (diff / denom) * FACTOR_RANKING;
        probLocal = Math.Min(PROBABILIDAD_MAXIMA, Math.Max(PROBABILIDAD_MINIMA, probLocal));

        double roll = random.NextDouble();

        if (roll < probLocal - UMBRAL_EMPATE)
        {
            (int golesGanador, int golesPerdedor) = GenerarMarcadorDecisivo(random);
            return (golesGanador, golesPerdedor);
        }

        if (roll > probLocal + UMBRAL_EMPATE)
        {
            (int golesGanador, int golesPerdedor) = GenerarMarcadorDecisivo(random);
            return (golesPerdedor, golesGanador);
        }

        int golesEmpate = random.Next(0, GOLES_EMPATE_MAX);
        return (golesEmpate, golesEmpate);
    }

    // El ganador siempre marca estrictamente más goles que el perdedor, de modo que el
    // marcador nunca contradiga al resultado sorteado (no produce empates ni resultados invertidos).
    private static (int golesGanador, int golesPerdedor) GenerarMarcadorDecisivo(Random random)
    {
        int golesGanador = random.Next(GOLES_GANADOR_MIN, GOLES_GANADOR_MAX);
        int golesPerdedor = random.Next(0, golesGanador);
        return (golesGanador, golesPerdedor);
    }
}
