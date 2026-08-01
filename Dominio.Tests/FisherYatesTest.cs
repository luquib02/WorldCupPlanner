using Dominio.Helpers;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dominio.Excepciones;

namespace Dominio.Tests.Helpers;

[TestClass]
[TestSubject(typeof(FisherYates))]
public class FisherYatesTest
{
    [TestMethod]
    public void BarajarConMismaSemillaProduceMismoResultadoOK()
    {
        List<int> lista1 = new List<int> { 1, 2, 3, 4, 5 };
        List<int> lista2 = new List<int> { 1, 2, 3, 4, 5 };

        FisherYates.Barajar(lista1, 42);
        FisherYates.Barajar(lista2, 42);

        CollectionAssert.AreEqual(lista1, lista2);
    }
    
    [TestMethod]
    public void BarajarConDistintasSemillasProduceDistintoResultadoOK()
    {
        List<int> lista1 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        List<int> lista2 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        FisherYates.Barajar(lista1, 1);
        FisherYates.Barajar(lista2, 999);

        CollectionAssert.AreNotEqual(lista1, lista2);
    }
    
    [TestMethod]
    public void BarajarMantieneCantidadDeElementosOK()
    {
        List<int> lista = new List<int> { 1, 2, 3, 4, 5 };

        FisherYates.Barajar(lista, 7);

        Assert.AreEqual(5, lista.Count);
    }

    [TestMethod]
    public void BarajarMantieneElementosOK()
    {
        List<int> lista = new List<int> { 1, 2, 3, 4, 5 };

        FisherYates.Barajar(lista, 7);

        Assert.IsTrue(lista.Contains(1));
        Assert.IsTrue(lista.Contains(2));
        Assert.IsTrue(lista.Contains(3));
        Assert.IsTrue(lista.Contains(4));
        Assert.IsTrue(lista.Contains(5));
    }
    [TestMethod]
    public void BarajarListaVaciaNoLanzaExcepcionOK()
    {
        List<int> lista = new List<int>();

        FisherYates.Barajar(lista, 5);

        Assert.AreEqual(0, lista.Count);
    }

    [TestMethod]
    public void BarajarListaConUnElementoNoLaModificaOK()
    {
        List<int> lista = new List<int> { 42 };

        FisherYates.Barajar(lista, 5);

        Assert.AreEqual(42, lista[0]);
    }
    
    [TestMethod]
    public void BarajarListaNulaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => FisherYates.Barajar<int>(null!, 5));
        Assert.AreEqual("La lista a barajar no puede ser nula.", ex.Message);
    }
    
}