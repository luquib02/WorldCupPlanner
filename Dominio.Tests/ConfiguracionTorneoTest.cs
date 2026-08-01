using System;
using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(ConfiguracionTorneo))]
public class ConfiguracionTorneoTest
{
    private ConfiguracionTorneo CrearConfiguracionValida(
        int semillaFixture = 1,
        int semillaCompletar = 2,
        int semillaCrucesFase = 3,
        int semillaSimulation = 4,
        DateTime? fechaInicio = null,
        int maxPartidosPorDia = 3,
        int separacionEntreFechas = 3)
    {
        return new ConfiguracionTorneo(
            semillaFixture,
            semillaCompletar,
            semillaCrucesFase,
            semillaSimulation,
            fechaInicio ?? new DateTime(2026, 6, 1),
            maxPartidosPorDia,
            separacionEntreFechas);
    }

    [TestMethod]
    public void CrearConfiguracionValidaOK()
    {
        ConfiguracionTorneo config = CrearConfiguracionValida();

        Assert.AreEqual(1, config.SemillaFixture);
        Assert.AreEqual(2, config.SemillaCompletar);
        Assert.AreEqual(3, config.SemillaCrucesFase);
        Assert.AreEqual(4, config.SemillaSimulation);
        Assert.AreEqual(new DateTime(2026, 6, 1), config.FechaInicioTorneo);
        Assert.AreEqual(3, config.MaxPartidosPorDia);
        Assert.AreEqual(3, config.SeparacionEntreFechas);
    }

    [TestMethod]
    public void CrearConfiguracionMaxPartidosPorDiaCeroLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearConfiguracionValida(maxPartidosPorDia: 0));
        Assert.AreEqual("MaxPartidosPorDia debe ser mayor a 0.", ex.Message);
    }

    [TestMethod]
    public void CrearConfiguracionMaxPartidosPorDiaNegativoLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearConfiguracionValida(maxPartidosPorDia: -1));
        Assert.AreEqual("MaxPartidosPorDia debe ser mayor a 0.", ex.Message);
    }

    [TestMethod]
    public void CrearConfiguracionSeparacionEntreFechasCeroLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearConfiguracionValida(separacionEntreFechas: 0));
        Assert.AreEqual("SeparacionEntreFechas debe ser mayor a 0.", ex.Message);
    }

    [TestMethod]
    public void CrearConfiguracionSeparacionEntreFechasNegativaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearConfiguracionValida(separacionEntreFechas: -1));
        Assert.AreEqual("SeparacionEntreFechas debe ser mayor a 0.", ex.Message);
    }

    [TestMethod]
    public void ConfiguracionPorDefectoOK()
    {
        ConfiguracionTorneo config = ConfiguracionTorneo.PorDefecto();

        Assert.AreEqual(new DateTime(2026, 6, 1), config.FechaInicioTorneo);
        Assert.AreEqual(3, config.MaxPartidosPorDia);
        Assert.AreEqual(3, config.SeparacionEntreFechas);
    }

    [TestMethod]
    public void ConfiguracionesIgualesSonEquivalentesOK()
    {
        ConfiguracionTorneo a = CrearConfiguracionValida();
        ConfiguracionTorneo b = CrearConfiguracionValida();

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void ConfiguracionesDistintasNoSonEquivalentesOK()
    {
        ConfiguracionTorneo a = CrearConfiguracionValida(semillaFixture: 1);
        ConfiguracionTorneo b = CrearConfiguracionValida(semillaFixture: 99);

        Assert.AreNotEqual(a, b);
    }
}
