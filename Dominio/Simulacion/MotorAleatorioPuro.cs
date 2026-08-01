using System;

namespace Dominio.Simulacion;

public class MotorAleatorioPuro : IMotorSimulacion
{
    private const int GOLES_MIN = 0;
    private const int GOLES_MAX = 5;

    public string Nombre => NombresMotores.AleatorioPuro;

    public (int golesLocal, int golesVisitante) GenerarMarcador(Equipo local, Equipo visitante, Random random)
    {
        int golesLocal = random.Next(GOLES_MIN, GOLES_MAX);
        int golesVisitante = random.Next(GOLES_MIN, GOLES_MAX);
        return (golesLocal, golesVisitante);
    }
}
