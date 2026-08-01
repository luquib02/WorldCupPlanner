using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(Incidencia))]
public class IncidenciaTest
{
    [TestMethod]
    public void CrearTarjetaAmarilla_MinutoValido_OK()
    {
        TarjetaAmarilla tarjeta = new TarjetaAmarilla(45, esLocal: true);

        Assert.AreEqual(45, tarjeta.MinutoJuego, "El minuto debe guardarse correctamente");
        Assert.IsTrue(tarjeta.EsLocal, "EsLocal debe ser true");
    }

    [TestMethod]
    public void CrearTarjetaAmarilla_MinutoCero_OK()
    {
        TarjetaAmarilla tarjeta = new TarjetaAmarilla(0, esLocal: true);

        Assert.AreEqual(0, tarjeta.MinutoJuego, "El minuto cero es válido");
    }

    [TestMethod]
    public void CrearTarjetaAmarilla_MinutoNegativo_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => new TarjetaAmarilla(-1, esLocal: true));
        Assert.AreEqual("El minuto de juego no puede ser negativo.", ex.Message);
    }

    [TestMethod]
    public void CrearTarjetaAmarilla_EsVisitante_GuardaEsLocalFalseOK()
    {
        TarjetaAmarilla tarjeta = new TarjetaAmarilla(30, esLocal: false);

        Assert.IsFalse(tarjeta.EsLocal, "EsLocal debe ser false para el equipo visitante");
    }

    [TestMethod]
    public void CrearTarjetaRoja_MinutoValido_OK()
    {
        TarjetaRoja tarjeta = new TarjetaRoja(80, esLocal: false);

        Assert.AreEqual(80, tarjeta.MinutoJuego, "El minuto debe guardarse correctamente");
        Assert.IsFalse(tarjeta.EsLocal, "EsLocal debe ser false");
    }

    [TestMethod]
    public void CrearTarjetaRoja_MinutoNegativo_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => new TarjetaRoja(-5, esLocal: false));
        Assert.AreEqual("El minuto de juego no puede ser negativo.", ex.Message);
    }

    [TestMethod]
    public void CrearTarjetaRoja_EsLocal_GuardaEsLocalTrueOK()
    {
        TarjetaRoja tarjeta = new TarjetaRoja(55, esLocal: true);

        Assert.IsTrue(tarjeta.EsLocal, "EsLocal debe ser true para el equipo local");
    }

    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        TarjetaAmarilla tarjeta = new TarjetaAmarilla(10, esLocal: true);

        tarjeta.Id = 3;

        Assert.AreEqual(3, tarjeta.Id);
    }

    [TestMethod]
    public void TarjetaAmarilla_ConstructorSinParametros_CreaInstanciaOK()
    {
        TarjetaAmarilla tarjeta = new TarjetaAmarilla();

        Assert.AreEqual(0, tarjeta.MinutoJuego);
        Assert.IsFalse(tarjeta.EsLocal);
    }

    [TestMethod]
    public void TarjetaRoja_ConstructorSinParametros_CreaInstanciaOK()
    {
        TarjetaRoja tarjeta = new TarjetaRoja();

        Assert.AreEqual(0, tarjeta.MinutoJuego);
        Assert.IsFalse(tarjeta.EsLocal);
    }
}
