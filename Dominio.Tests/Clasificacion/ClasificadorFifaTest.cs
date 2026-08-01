using System.Collections.Generic;
using Dominio.Clasificacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests.Clasificacion;

[TestClass]
public class ClasificadorFifaTest
{
    private Equipo CrearEquipo(string nombre)
    {
        return new Equipo(nombre, Confederacion.CONMEBOL, 1500);
    }

    [TestMethod]
    public void OrdenarOrdenaPorPuntosDescendenteOK()
    {
        PosicionEquipo posicionA = new PosicionEquipo(CrearEquipo("A"));
        PosicionEquipo posicionB = new PosicionEquipo(CrearEquipo("B"));
        posicionB.RegistrarResultado(2, 0);
        List<PosicionEquipo> tabla = new List<PosicionEquipo> { posicionA, posicionB };

        List<PosicionEquipo> resultado = new ClasificadorFifa().Ordenar(tabla);

        Assert.AreEqual(posicionB, resultado[0]);
        Assert.AreEqual(posicionA, resultado[1]);
    }

    [TestMethod]
    public void OrdenarConPuntosIgualesDesempataPorDiferenciaDeGolesOK()
    {
        PosicionEquipo posicionA = new PosicionEquipo(CrearEquipo("A"));
        PosicionEquipo posicionB = new PosicionEquipo(CrearEquipo("B"));
        posicionA.RegistrarResultado(3, 1);
        posicionB.RegistrarResultado(1, 0);
        List<PosicionEquipo> tabla = new List<PosicionEquipo> { posicionB, posicionA };

        List<PosicionEquipo> resultado = new ClasificadorFifa().Ordenar(tabla);

        Assert.AreEqual(posicionA, resultado[0]);
        Assert.AreEqual(posicionB, resultado[1]);
    }

    [TestMethod]
    public void OrdenarConPuntosYDiferenciaIgualesDesempataPorGolesAFavorOK()
    {
        PosicionEquipo posicionA = new PosicionEquipo(CrearEquipo("A"));
        PosicionEquipo posicionB = new PosicionEquipo(CrearEquipo("B"));
        posicionA.RegistrarResultado(3, 1);
        posicionB.RegistrarResultado(2, 0);
        List<PosicionEquipo> tabla = new List<PosicionEquipo> { posicionB, posicionA };

        List<PosicionEquipo> resultado = new ClasificadorFifa().Ordenar(tabla);

        Assert.AreEqual(posicionA, resultado[0]);
        Assert.AreEqual(posicionB, resultado[1]);
    }
}
