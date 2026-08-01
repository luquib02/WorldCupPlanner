using System;
using System.Linq;
using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(Bombo))]
public class BomboTest
{
    private Equipo CrearEquipo(string nombre = "Argentina")
    {
        return new Equipo(nombre, Confederacion.CONMEBOL, 1500);
    }

    private Bombo CrearBomboValido(int numero = 1)
    {
        return new Bombo(numero);
    }

    [TestMethod]
    public void CrearBomboValidoOK()
    {
        Bombo bombo = CrearBomboValido(2);

        Assert.AreEqual(2, bombo.Numero);
        Assert.AreEqual(0, bombo.Cantidad());
    }

    [TestMethod]
    public void CrearBomboNumeroMenorA1LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearBomboValido(0));
        Assert.AreEqual("El número de bombo debe estar entre 1 y 4.", ex.Message);
    }

    [TestMethod]
    public void CrearBomboNumeroMayorA4LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearBomboValido(5));
        Assert.AreEqual("El número de bombo debe estar entre 1 y 4.", ex.Message);
    }

    [TestMethod]
    public void AgregarEquipoOK()
    {
        Bombo bombo = CrearBomboValido();
        Equipo equipo = CrearEquipo();

        bombo.Agregar(equipo);

        Assert.AreEqual(1, bombo.Cantidad());
    }

    [TestMethod]
    public void AgregarEquipoNuloLanzaExcepcion()
    {
        Bombo bombo = CrearBomboValido();

        DominioException ex = Assert.Throws<DominioException>(
            () => bombo.Agregar(null!));
        Assert.AreEqual("El equipo es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void AgregarEquipoDuplicadoLanzaExcepcion()
    {
        Bombo bombo = CrearBomboValido();
        Equipo equipo = CrearEquipo("Brasil");
        bombo.Agregar(equipo);

        DominioException ex = Assert.Throws<DominioException>(
            () => bombo.Agregar(equipo));
        Assert.AreEqual("El equipo ya está en el bombo.", ex.Message);
    }

    [TestMethod]
    public void AgregarEquipoSuperaCapacidadLanzaExcepcion()
    {
        Bombo bombo = CrearBomboValido();
        for (int i = 0; i < 12; i++)
        {
            bombo.Agregar(CrearEquipo($"Equipo{i}"));
        }

        DominioException ex = Assert.Throws<DominioException>(
            () => bombo.Agregar(CrearEquipo("Extra")));
        Assert.AreEqual("El bombo no puede contener más de 12 equipos.", ex.Message);
    }

    [TestMethod]
    public void EquiposDevuelveLosEquiposAgregadosOK()
    {
        Bombo bombo = CrearBomboValido();
        Equipo equipo = CrearEquipo("Uruguay");
        bombo.Agregar(equipo);

        Assert.AreEqual(1, bombo.Equipos.Count);
        Assert.IsTrue(bombo.Equipos.Contains(equipo));
    }

    [TestMethod]
    public void ObtenerEquipoOK()
    {
        Bombo bombo = CrearBomboValido();
        Equipo equipo = CrearEquipo("Uruguay");
        bombo.Agregar(equipo);

        Assert.AreEqual(equipo, bombo.ObtenerEquipo(0));
    }

    [TestMethod]
    public void ObtenerEquipoIndiceInvalidoLanzaExcepcion()
    {
        Bombo bombo = CrearBomboValido();
        bombo.Agregar(CrearEquipo("Uruguay"));

        DominioException ex = Assert.Throws<DominioException>(
            () => bombo.ObtenerEquipo(5));
        Assert.AreEqual("Índice fuera de rango.", ex.Message);
    }

    [TestMethod]
    public void ObtenerEquipoIndiceNegativoLanzaExcepcion()
    {
        Bombo bombo = CrearBomboValido();
        bombo.Agregar(CrearEquipo("Uruguay"));

        DominioException ex = Assert.Throws<DominioException>(
            () => bombo.ObtenerEquipo(-1));
        Assert.AreEqual("Índice fuera de rango.", ex.Message);
    }
}
