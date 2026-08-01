using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(PosicionEquipo))]
public class PosicionEquipoTest
{
    private Equipo CrearEquipo(string nombre = "Argentina")
    {
        return new Equipo(nombre, Confederacion.CONMEBOL, 1500);
    }

    [TestMethod]
    public void CrearPosicionEquipoValidaOK()
    {
        Equipo equipo = CrearEquipo();
        PosicionEquipo posicion = new PosicionEquipo(equipo);

        Assert.AreEqual(equipo, posicion.Equipo);
        Assert.AreEqual(0, posicion.Puntos);
        Assert.AreEqual(0, posicion.PartidosJugados);
        Assert.AreEqual(0, posicion.Ganados);
        Assert.AreEqual(0, posicion.Empatados);
        Assert.AreEqual(0, posicion.Perdidos);
        Assert.AreEqual(0, posicion.GolesAFavor);
        Assert.AreEqual(0, posicion.GolesEnContra);
        Assert.AreEqual(0, posicion.DiferenciaGoles);
    }

    [TestMethod]
    public void CrearPosicionEquipoNuloLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => new PosicionEquipo(null!));
        Assert.AreEqual("El equipo es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void RegistrarVictoriaOK()
    {
        PosicionEquipo posicion = new PosicionEquipo(CrearEquipo());
        posicion.RegistrarResultado(3, 1);

        Assert.AreEqual(3, posicion.Puntos);
        Assert.AreEqual(1, posicion.PartidosJugados);
        Assert.AreEqual(1, posicion.Ganados);
        Assert.AreEqual(0, posicion.Empatados);
        Assert.AreEqual(0, posicion.Perdidos);
        Assert.AreEqual(3, posicion.GolesAFavor);
        Assert.AreEqual(1, posicion.GolesEnContra);
        Assert.AreEqual(2, posicion.DiferenciaGoles);
    }

    [TestMethod]
    public void RegistrarEmpateOK()
    {
        PosicionEquipo posicion = new PosicionEquipo(CrearEquipo());
        posicion.RegistrarResultado(1, 1);

        Assert.AreEqual(1, posicion.Puntos);
        Assert.AreEqual(1, posicion.PartidosJugados);
        Assert.AreEqual(0, posicion.Ganados);
        Assert.AreEqual(1, posicion.Empatados);
        Assert.AreEqual(0, posicion.Perdidos);
        Assert.AreEqual(0, posicion.DiferenciaGoles);
    }

    [TestMethod]
    public void RegistrarDerrotaOK()
    {
        PosicionEquipo posicion = new PosicionEquipo(CrearEquipo());
        posicion.RegistrarResultado(0, 2);

        Assert.AreEqual(0, posicion.Puntos);
        Assert.AreEqual(1, posicion.PartidosJugados);
        Assert.AreEqual(0, posicion.Ganados);
        Assert.AreEqual(0, posicion.Empatados);
        Assert.AreEqual(1, posicion.Perdidos);
        Assert.AreEqual(-2, posicion.DiferenciaGoles);
    }

    [TestMethod]
    public void RegistrarVariosResultadosAcumulaCorrectamenteOK()
    {
        PosicionEquipo posicion = new PosicionEquipo(CrearEquipo());
        posicion.RegistrarResultado(2, 0);
        posicion.RegistrarResultado(1, 1);
        posicion.RegistrarResultado(0, 3);

        Assert.AreEqual(4, posicion.Puntos);
        Assert.AreEqual(3, posicion.PartidosJugados);
        Assert.AreEqual(1, posicion.Ganados);
        Assert.AreEqual(1, posicion.Empatados);
        Assert.AreEqual(1, posicion.Perdidos);
        Assert.AreEqual(3, posicion.GolesAFavor);
        Assert.AreEqual(4, posicion.GolesEnContra);
        Assert.AreEqual(-1, posicion.DiferenciaGoles);
    }

    [TestMethod]
    public void RegistrarResultadoGolesNegativosLanzaExcepcion()
    {
        PosicionEquipo posicion = new PosicionEquipo(CrearEquipo());
        DominioException ex = Assert.Throws<DominioException>(
            () => posicion.RegistrarResultado(-1, 0));
        Assert.AreEqual("Los goles no pueden ser negativos.", ex.Message);
    }

    [TestMethod]
    public void RegistrarResultadoGolesEnContraNegativosLanzaExcepcion()
    {
        PosicionEquipo posicion = new PosicionEquipo(CrearEquipo());
        DominioException ex = Assert.Throws<DominioException>(
            () => posicion.RegistrarResultado(0, -1));
        Assert.AreEqual("Los goles no pueden ser negativos.", ex.Message);
    }
}
