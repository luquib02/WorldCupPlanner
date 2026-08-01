using Dominio.Excepciones;
using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.Simulacion;
using System.Collections.Generic;

namespace Sistema.Tests.Simulacion;

[TestClass]
public class MotorSimulacionFactoryTest
{
    [TestMethod]
    public void ObtenerPorNombre_Probabilistico_DevuelveMotorProbabilistico()
    {
        IMotorSimulacion motor = MotorSimulacionFactory.ObtenerPorNombre(NombresMotores.Probabilistico);

        Assert.IsInstanceOfType(motor, typeof(MotorProbabilistico),
            "El nombre 'Probabilístico' debe resolver a MotorProbabilistico");
    }

    [TestMethod]
    public void ObtenerPorNombre_AleatorioPuro_DevuelveMotorAleatorioPuro()
    {
        IMotorSimulacion motor = MotorSimulacionFactory.ObtenerPorNombre(NombresMotores.AleatorioPuro);

        Assert.IsInstanceOfType(motor, typeof(MotorAleatorioPuro),
            "El nombre 'Aleatorio Puro' debe resolver a MotorAleatorioPuro");
    }

    [TestMethod]
    public void ObtenerPorNombre_NombreInexistente_LanzaDominioException()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => MotorSimulacionFactory.ObtenerPorNombre("Inexistente"));

        Assert.AreEqual("No existe el motor de simulación 'Inexistente'.", ex.Message,
            "Un nombre de motor inexistente debe lanzar DominioException con mensaje claro");
    }

    [TestMethod]
    public void ObtenerNombresDisponibles_DevuelveAmbosMotores()
    {
        List<string> nombres = MotorSimulacionFactory.ObtenerNombresDisponibles();

        Assert.IsTrue(nombres.Contains(NombresMotores.Probabilistico),
            "Debe incluir el motor Probabilístico");
        Assert.IsTrue(nombres.Contains(NombresMotores.AleatorioPuro),
            "Debe incluir el motor Aleatorio Puro");
        Assert.AreEqual(2, nombres.Count, "Actualmente hay 2 motores disponibles");
    }
}
