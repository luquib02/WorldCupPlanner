using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dominio.Tests.Simulacion;

[TestClass]
public class MotorAleatorioPuroTest
{
    private readonly MotorAleatorioPuro _motor = new MotorAleatorioPuro();

    private Equipo CrearEquipo(string nombre, int ranking) =>
        new Equipo(nombre, Confederacion.UEFA, ranking);

    [TestMethod]
    public void Nombre_DevuelveAleatorioPuro()
    {
        Assert.AreEqual("Aleatorio Puro", _motor.Nombre,
            "El motor aleatorio puro debe identificarse con el nombre 'Aleatorio Puro'");
    }

    [TestMethod]
    public void GenerarMarcador_GolesEnRangoValido()
    {
        Equipo local = CrearEquipo("Local", 1800);
        Equipo visitante = CrearEquipo("Visitante", 1600);

        (int golesLocal, int golesVisitante) = _motor.GenerarMarcador(local, visitante, new Random(123));

        Assert.IsTrue(golesLocal >= 0 && golesLocal <= 4, "Los goles del local deben estar entre 0 y 4");
        Assert.IsTrue(golesVisitante >= 0 && golesVisitante <= 4, "Los goles del visitante deben estar entre 0 y 4");
    }

    [TestMethod]
    public void GenerarMarcador_MismaSemilla_ResultadoDeterminista()
    {
        Equipo local = CrearEquipo("Local", 1800);
        Equipo visitante = CrearEquipo("Visitante", 1600);

        (int golesLocal1, int golesVisitante1) = _motor.GenerarMarcador(local, visitante, new Random(777));
        (int golesLocal2, int golesVisitante2) = _motor.GenerarMarcador(local, visitante, new Random(777));

        Assert.AreEqual(golesLocal1, golesLocal2, "Con la misma semilla el resultado debe ser determinista");
        Assert.AreEqual(golesVisitante1, golesVisitante2, "Con la misma semilla el resultado debe ser determinista");
    }

    [TestMethod]
    public void GenerarMarcador_NoDependeDelRankingDeLosEquipos()
    {
        Equipo localRankingAlto = CrearEquipo("Local", 2500);
        Equipo visitanteRankingBajo = CrearEquipo("Visitante", 300);
        Equipo localRankingBajo = CrearEquipo("Local2", 300);
        Equipo visitanteRankingAlto = CrearEquipo("Visitante2", 2500);

        (int golesLocal1, int golesVisitante1) = _motor.GenerarMarcador(localRankingAlto, visitanteRankingBajo, new Random(555));
        (int golesLocal2, int golesVisitante2) = _motor.GenerarMarcador(localRankingBajo, visitanteRankingAlto, new Random(555));

        Assert.AreEqual(golesLocal1, golesLocal2,
            "El motor aleatorio puro no debe usar el ranking de los equipos para el marcador");
        Assert.AreEqual(golesVisitante1, golesVisitante2,
            "El motor aleatorio puro no debe usar el ranking de los equipos para el marcador");
    }
}
