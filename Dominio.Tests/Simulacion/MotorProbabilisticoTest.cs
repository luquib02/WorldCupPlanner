using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dominio.Tests.Simulacion;

[TestClass]
public class MotorProbabilisticoTest
{
    private readonly MotorProbabilistico _motor = new MotorProbabilistico();

    private Equipo CrearEquipo(string nombre, int ranking) =>
        new Equipo(nombre, Confederacion.UEFA, ranking);

    [TestMethod]
    public void Nombre_DevuelveProbabilistico()
    {
        Assert.AreEqual("Probabilístico", _motor.Nombre,
            "El motor probabilístico debe identificarse con el nombre 'Probabilístico'");
    }

    [TestMethod]
    public void GenerarMarcador_GolesSiempreNoNegativos()
    {
        Equipo local = CrearEquipo("Local", 1800);
        Equipo visitante = CrearEquipo("Visitante", 1600);

        (int golesLocal, int golesVisitante) = _motor.GenerarMarcador(local, visitante, new Random(123));

        Assert.IsTrue(golesLocal >= 0, "Los goles del local no pueden ser negativos");
        Assert.IsTrue(golesVisitante >= 0, "Los goles del visitante no pueden ser negativos");
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
    public void GenerarMarcador_SemillaDistinta_PuedeProducirResultadoDistinto()
    {
        Equipo local = CrearEquipo("Local", 1800);
        Equipo visitante = CrearEquipo("Visitante", 1600);

        (int golesLocal1, int golesVisitante1) = _motor.GenerarMarcador(local, visitante, new Random(1));
        (int golesLocal2, int golesVisitante2) = _motor.GenerarMarcador(local, visitante, new Random(2));

        Assert.IsTrue(golesLocal1 != golesLocal2 || golesVisitante1 != golesVisitante2,
            "Distintas semillas deberian producir distintos resultados");
    }

    [TestMethod]
    public void GenerarMarcador_ResultadoDecisivo_GanadorMarcaMasQuePerdedor()
    {
        // Local fuertemente favorito: la franja decisiva-local (roll < probLocal - umbral) es amplia.
        Equipo local = CrearEquipo("Local", 2500);
        Equipo visitante = CrearEquipo("Visitante", 300);
        int casosDecisivos = 0;

        for (int semilla = 0; semilla < 200; semilla++)
        {
            // El primer consumo del Random en GenerarMarcador es el "roll": lo reproducimos para
            // quedarnos solo con semillas que caen con seguridad en la franja decisiva a favor del local.
            double rollEsperado = new Random(semilla).NextDouble();
            if (rollEsperado >= 0.5) continue;

            (int golesLocal, int golesVisitante) = _motor.GenerarMarcador(local, visitante, new Random(semilla));

            casosDecisivos++;
            Assert.IsTrue(golesLocal > golesVisitante,
                $"En un resultado decisivo el ganador debe marcar más goles (semilla {semilla}: {golesLocal}-{golesVisitante})");
        }

        Assert.IsTrue(casosDecisivos > 0,
            "El escenario debe cubrir al menos un resultado decisivo a favor del local");
    }

    [TestMethod]
    public void GenerarMarcador_RankingsIguales_PuedeProducirEmpateOK()
    {
        Equipo local = CrearEquipo("Local", 1500);
        Equipo visitante = CrearEquipo("Visitante", 1500);
        bool huboEmpate = false;

        for (int semilla = 0; semilla < 50; semilla++)
        {
            (int golesLocal, int golesVisitante) = _motor.GenerarMarcador(local, visitante, new Random(semilla));
            if (golesLocal == golesVisitante)
                huboEmpate = true;
        }

        Assert.IsTrue(huboEmpate, "Con rankings iguales debe poder producirse un resultado empatado");
    }
}
