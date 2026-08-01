using Dominio.Helpers;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests.Helpers;

[TestClass]
[TestSubject(typeof(Normalizador))]
public class NormalizadorTest
{
    [TestMethod]
    public void NormalizarTextoEnMinusculasOK()
    {
        string resultado = Normalizador.Normalizar("ESTADIO");
        Assert.AreEqual("estadio", resultado);
    }
    
    [TestMethod]
    public void NormalizarTextoSinTildesOK()
    {
        string resultado = Normalizador.Normalizar("Sao Paulo Camboriú");
        Assert.AreEqual("sao paulo camboriu", resultado);
    }
    
    [TestMethod]
    public void NormalizarReemplazaNoAlfanumericosPorEspacioOK()
    {
        string resultado = Normalizador.Normalizar("Estadio-Centenario_2026");
        Assert.AreEqual("estadio centenario 2026", resultado);
    }
    
    [TestMethod]
    public void NormalizarColapsaEspaciosOK()
    {
        string resultado = Normalizador.Normalizar("Estadio   Centenario");
        Assert.AreEqual("estadio centenario", resultado);
    }
    
    [TestMethod]
    public void NormalizarAplicaTrimOK()
    {
        string resultado = Normalizador.Normalizar("  Estadio  ");
        Assert.AreEqual("estadio", resultado);
    }
    
    [TestMethod]
    public void NormalizarTextoNuloRetornaVacioOK()
    {
        string resultado = Normalizador.Normalizar(null!);
        Assert.AreEqual("", resultado);
    }

    [TestMethod]
    public void NormalizarTextoVacioRetornaVacioOK()
    {
        string resultado = Normalizador.Normalizar("");
        Assert.AreEqual("", resultado);
    }
}