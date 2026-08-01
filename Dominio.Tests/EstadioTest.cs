using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(Estadio))]
public class EstadioTest
{
    private Estadio CrearEstadioValido(
        string nombre = "Estadio Centenario",
        string ciudad = "Montevideo",
        string? descripcion = null,
        int capacidad = 60000)
    {
        return new Estadio()
        {
            Nombre = nombre,
            Ciudad = ciudad,
            Descripcion = descripcion,
            Capacidad = capacidad
        };
    }
    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        Estadio estadio = CrearEstadioValido();

        estadio.Id = 9;

        Assert.AreEqual(9, estadio.Id);
    }

    [TestMethod]
    public void CrearEstadioDatosMinimosValidosOK()
    {
        Estadio estadio = CrearEstadioValido();
        Assert.AreEqual("Estadio Centenario",estadio.Nombre);
        Assert.AreEqual("Montevideo", estadio.Ciudad);
        Assert.AreEqual(60000, estadio.Capacidad);
        Assert.IsNull(estadio.Descripcion);
    }
    
    [TestMethod]
    public void CrearEstadioConDescripcionOK()
    {
        Estadio estadio = CrearEstadioValido(descripcion: "Estadio principal de Uruguay.");

        Assert.AreEqual("Estadio principal de Uruguay.", estadio.Descripcion);
    }
    [TestMethod]
    public void CrearEstadioNombreExacto80CharsOK()
    {
        Estadio estadio = CrearEstadioValido(nombre: new string('A', 80));

        Assert.AreEqual(80, estadio.Nombre.Length);
    }
    [TestMethod]
    public void CrearEstadioCiudadExacto60CharsOK()
    {
        Estadio estadio = CrearEstadioValido(ciudad: new string('A', 60));

        Assert.AreEqual(60, estadio.Ciudad.Length);
    }
    [TestMethod]
    public void CrearEstadioCapacidadExacta20000OK()
    {
        Estadio estadio = CrearEstadioValido(capacidad: 20000);

        Assert.AreEqual(20000, estadio.Capacidad);
    }

    [TestMethod]
    public void CrearEstadioDescripcionExacto400CharsOK()
    {
        Estadio estadio = CrearEstadioValido(descripcion: new string('A', 400));

        Assert.AreEqual(400, estadio.Descripcion!.Length);
    }
    [TestMethod]
    public void CrearEstadioNombreVacioLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(nombre: ""));

        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 80 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioNombreNuloLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(nombre: null!));

        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 80 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioNombreSoloEspaciosLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(nombre: "   "));

        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 80 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioNombreMasDe80CharsLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(nombre: new string('A', 81)));

        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 80 caracteres.", ex.Message);
    }
    [TestMethod]
    public void CrearEstadioCiudadVaciaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(ciudad: ""));

        Assert.AreEqual("La ciudad es obligatoria y debe tener entre 1 y 60 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioCiudadNulaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(ciudad: null!));

        Assert.AreEqual("La ciudad es obligatoria y debe tener entre 1 y 60 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioCiudadSoloEspaciosLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(ciudad: "   "));

        Assert.AreEqual("La ciudad es obligatoria y debe tener entre 1 y 60 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioCiudadMasDe60CharsLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(ciudad: new string('A', 61)));

        Assert.AreEqual("La ciudad es obligatoria y debe tener entre 1 y 60 caracteres.", ex.Message);
    }
    
    [TestMethod]
    public void CrearEstadioDescripcionMasDe400CharsLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(descripcion: new string('A', 401)));

        Assert.AreEqual("La descripción no puede superar los 400 caracteres.", ex.Message);
    }
    
    [TestMethod]
    public void CrearEstadioCapacidadMenorA20000LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(capacidad: 19999));

        Assert.AreEqual("La capacidad debe ser mayor o igual a 20000 espectadores.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioCapacidadCeroLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(capacidad: 0));

        Assert.AreEqual("La capacidad debe ser mayor o igual a 20000 espectadores.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioCapacidadNegativaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEstadioValido(capacidad: -1));

        Assert.AreEqual("La capacidad debe ser mayor o igual a 20000 espectadores.", ex.Message);
    }
}
