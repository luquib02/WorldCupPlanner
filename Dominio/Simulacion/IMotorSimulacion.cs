using System;

namespace Dominio.Simulacion;

public interface IMotorSimulacion
{
    string Nombre { get; }

    (int golesLocal, int golesVisitante) GenerarMarcador(Equipo local, Equipo visitante, Random random);
}
