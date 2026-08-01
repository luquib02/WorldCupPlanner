using System;
using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
namespace Dominio.Tests;
[TestClass]
public class JornadaTest
{
    private class JornadaDePrueba : Jornada { }


    private Estadio CrearEstadio()
    {
        return new Estadio
        {
            Nombre = "Estadio Centenario",
            Ciudad = "Montevideo",
            Capacidad = 60000
        };
    }
    private Partido CrearPartido(string local = "Argentina", string visitante = "Brasil")
    {
        return new Partido
        {
            Fecha = new DateTime(2026, 6, 1),
            Estadio = CrearEstadio(),
            EquipoLocal = new Equipo(local, Confederacion.CONMEBOL, 1500),
            EquipoVisitante = new Equipo(visitante, Confederacion.CONMEBOL, 1400),
            Grupo = "A",
            Fase = FasePartido.FaseDeGrupos
        };
    }
    private Jornada CrearJornadaValida(int numero = 1, DateTime? fecha = null)
    {
        return new Jornada(numero, fecha ?? new DateTime(2026, 6, 1));
    }
    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        Jornada jornada = CrearJornadaValida();

        jornada.Id = 8;

        Assert.AreEqual(8, jornada.Id);
    }

    [TestMethod]
    public void ConstructorProtegido_CreaInstanciaConValoresPorDefectoOK()
    {
        JornadaDePrueba jornada = new JornadaDePrueba();

        Assert.AreEqual(0, jornada.Partidos.Count);
    }

    [TestMethod]
    public void CrearJornadaValidaOK()
    {
        DateTime fecha = new DateTime(2026, 6, 1);
        Jornada jornada = CrearJornadaValida(2, fecha);
        Assert.AreEqual(2, jornada.Numero);
        Assert.AreEqual(fecha, jornada.Fecha);
        Assert.AreEqual(0, jornada.Partidos.Count);
    }
    
    [TestMethod]
    public void CrearJornadaNumeroMenorA1LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearJornadaValida(0));
        Assert.AreEqual("El número de jornada debe estar entre 1 y 3.", ex.Message);
    }
    [TestMethod]
    public void CrearJornadaNumeroMayorA3LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearJornadaValida(4));
        Assert.AreEqual("El número de jornada debe estar entre 1 y 3.", ex.Message);
    }
    
    [TestMethod]
    public void AgregarPartidoOK()
    {
        Jornada jornada = CrearJornadaValida();
        Partido partido = CrearPartido();

        jornada.AgregarPartido(partido);

        Assert.AreEqual(1, jornada.Partidos.Count);
        Assert.AreEqual(partido, jornada.Partidos[0]);
    }
    
    [TestMethod]
    public void AgregarPartidoNuloLanzaExcepcion()
    {
        Jornada jornada = CrearJornadaValida();
        DominioException ex = Assert.Throws<DominioException>(
            () => jornada.AgregarPartido(null!));
        Assert.AreEqual("El partido es obligatorio.", ex.Message);
    }
    
    [TestMethod]
    public void AgregarPartidoDuplicadoLanzaExcepcion()
    {
        Jornada jornada = CrearJornadaValida();
        Partido partido = CrearPartido();
        jornada.AgregarPartido(partido);

        DominioException ex = Assert.Throws<DominioException>(
            () => jornada.AgregarPartido(partido));
        Assert.AreEqual("El partido ya está en la jornada.", ex.Message);
    }
    
    [TestMethod]
    public void AgregarMultiplesPartidosOK()
    {
        Jornada jornada = CrearJornadaValida();
        Partido p1 = CrearPartido("Argentina", "Brasil");
        Partido p2 = CrearPartido("Uruguay", "Chile");

        jornada.AgregarPartido(p1);
        jornada.AgregarPartido(p2);

        Assert.AreEqual(2, jornada.Partidos.Count);
    }
}