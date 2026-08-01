using Dominio;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(CalculadoraRankingElo))]
public class CalculadoraRankingEloTest
{
    private CalculadoraRankingElo _calculadora = null!;

    [TestInitialize]
    public void Setup()
    {
        _calculadora = new CalculadoraRankingElo();
    }

    [TestMethod]
    public void CalcularNuevoRanking_EjemploObligatorioEquipoDebilGanaEnGrupos_Retorna1220()
    {
        int resultado = _calculadora.CalcularNuevoRanking(1200, 1500, 1.0, FasePartido.FaseDeGrupos);

        Assert.AreEqual(1220, resultado,
            "Ejemplo del obligatorio: Equipo B (1200) gana a A (1500) en grupos debe obtener 1220");
    }

    [TestMethod]
    public void CalcularNuevoRanking_EquipoFuertePierdeEnGrupos_Retorna1480()
    {
        int resultado = _calculadora.CalcularNuevoRanking(1500, 1200, 0.0, FasePartido.FaseDeGrupos);

        Assert.AreEqual(1480, resultado,
            "Equipo A (1500) pierde ante B (1200) en grupos debe obtener 1480");
    }

    [TestMethod]
    public void CalcularNuevoRanking_EmpateEntreIguales_MantieneRanking()
    {
        int resultado = _calculadora.CalcularNuevoRanking(1500, 1500, 0.5, FasePartido.FaseDeGrupos);

        Assert.AreEqual(1500, resultado,
            "Con rankings iguales y empate la probabilidad es 0.5, delta es 0, ranking no cambia");
    }

    [TestMethod]
    public void CalcularNuevoRanking_EmpateEquipoDebilContraFuerte_SubeRanking()
    {
        int resultado = _calculadora.CalcularNuevoRanking(1200, 1500, 0.5, FasePartido.FaseDeGrupos);

        Assert.IsTrue(resultado > 1200,
            "El equipo débil que empata con uno más fuerte debe subir ranking");
    }

    [TestMethod]
    public void CalcularNuevoRanking_VictoriaEnEliminatorias_AplicaMultiplicador1punto5()
    {
        int resultadoGrupos = _calculadora.CalcularNuevoRanking(1200, 1500, 1.0, FasePartido.FaseDeGrupos);
        int resultadoEliminatoria = _calculadora.CalcularNuevoRanking(1200, 1500, 1.0, FasePartido.OctavosDeFinal);

        Assert.AreEqual(1230, resultadoEliminatoria,
            "En eliminatorias el multiplicador 1.5 debe dar 1230 con los mismos parámetros que dan 1220 en grupos");
        Assert.IsTrue(resultadoEliminatoria > resultadoGrupos,
            "El mismo resultado en eliminatorias debe sumar más que en fase de grupos");
    }

    [TestMethod]
    public void CalcularNuevoRanking_TodasLasFasesEliminatorias_UsanMismoMultiplicador()
    {
        int resultadoOctavos = _calculadora.CalcularNuevoRanking(1200, 1500, 1.0, FasePartido.OctavosDeFinal);
        int resultadoCuartos = _calculadora.CalcularNuevoRanking(1200, 1500, 1.0, FasePartido.CuartosDeFinal);
        int resultadoSemifinal = _calculadora.CalcularNuevoRanking(1200, 1500, 1.0, FasePartido.Semifinal);
        int resultadoFinal = _calculadora.CalcularNuevoRanking(1200, 1500, 1.0, FasePartido.Final);

        Assert.AreEqual(resultadoOctavos, resultadoCuartos, "Todas las fases eliminatorias deben usar multiplicador 1.5");
        Assert.AreEqual(resultadoOctavos, resultadoSemifinal, "Todas las fases eliminatorias deben usar multiplicador 1.5");
        Assert.AreEqual(resultadoOctavos, resultadoFinal, "Todas las fases eliminatorias deben usar multiplicador 1.5");
    }

    [TestMethod]
    public void CalcularNuevoRanking_ResultadoCalculadoSuperaMaximo_Retorna2500()
    {
        // Equipo en 2490 gana a uno levemente más fuerte (2500): delta positivo supera el techo
        int resultado = _calculadora.CalcularNuevoRanking(2490, 2500, 1.0, FasePartido.FaseDeGrupos);

        Assert.AreEqual(2500, resultado, "El ranking no puede superar el máximo de 2500");
    }

    [TestMethod]
    public void CalcularNuevoRanking_ResultadoCalculadoBajaDeMinimo_Retorna300()
    {
        // Equipo en 310 pierde ante uno levemente más débil (300): cae por debajo del piso
        int resultado = _calculadora.CalcularNuevoRanking(310, 300, 0.0, FasePartido.FaseDeGrupos);

        Assert.AreEqual(300, resultado, "El ranking no puede bajar del mínimo de 300");
    }

    [TestMethod]
    public void CalcularNuevoRanking_VictoriaEquipoFuerteContraDebil_IncrementoPequenio()
    {
        int resultado = _calculadora.CalcularNuevoRanking(1500, 1200, 1.0, FasePartido.FaseDeGrupos);

        Assert.AreEqual(1510, resultado,
            "El equipo fuerte (1500) que gana al débil (1200) sube solo 10 puntos porque la victoria era esperada");
    }
}
