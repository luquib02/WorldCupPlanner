using System;
using System.Collections.Generic;
using Dominio.Excepciones;
using Dominio.Simulacion;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(Fixture))]
public class FixtureTest
{
    private class FixtureDePrueba : Fixture { }


    private Equipo CrearEquipo(string nombre)
    {
        return new Equipo(nombre, Confederacion.CONMEBOL, 1500);
    }

    private Estadio CrearEstadio()
    {
        return new Estadio { Nombre = "E", Ciudad = "C", Capacidad = 20000 };
    }

    private Partido CrearPartidoJugado(Equipo local, Equipo visitante, int gl, int gv, string grupo = "A")
    {
        Partido p = new Partido
        {
            Fecha = new DateTime(2026, 6, 1),
            Estadio = CrearEstadio(),
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = grupo,
            Fase = FasePartido.FaseDeGrupos
        };
        p.CargarResultado(gl, gv);
        return p;
    }

    private Grupo CrearGrupoCon6PartidosJugados(string etiqueta = "A")
    {
        Grupo grupo = new Grupo(etiqueta);
        Equipo e1 = CrearEquipo($"{etiqueta}1");
        Equipo e2 = CrearEquipo($"{etiqueta}2");
        Equipo e3 = CrearEquipo($"{etiqueta}3");
        Equipo e4 = CrearEquipo($"{etiqueta}4");
        grupo.AgregarEquipo(e1);
        grupo.AgregarEquipo(e2);
        grupo.AgregarEquipo(e3);
        grupo.AgregarEquipo(e4);

        Jornada j1 = new Jornada(1, new DateTime(2026, 6, 1));
        j1.AgregarPartido(CrearPartidoJugado(e1, e4, 1, 0, etiqueta));
        j1.AgregarPartido(CrearPartidoJugado(e2, e3, 2, 1, etiqueta));

        Jornada j2 = new Jornada(2, new DateTime(2026, 6, 4));
        j2.AgregarPartido(CrearPartidoJugado(e1, e3, 1, 1, etiqueta));
        j2.AgregarPartido(CrearPartidoJugado(e2, e4, 0, 0, etiqueta));

        Jornada j3 = new Jornada(3, new DateTime(2026, 6, 7));
        j3.AgregarPartido(CrearPartidoJugado(e1, e2, 2, 2, etiqueta));
        j3.AgregarPartido(CrearPartidoJugado(e3, e4, 1, 0, etiqueta));

        grupo.AgregarJornada(j1);
        grupo.AgregarJornada(j2);
        grupo.AgregarJornada(j3);
        return grupo;
    }

    private Fixture CrearFixtureValido(int semilla = 42)
    {
        return new Fixture(semilla);
    }

    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        Fixture fixture = CrearFixtureValido();

        fixture.Id = 11;

        Assert.AreEqual(11, fixture.Id);
    }

    [TestMethod]
    public void ConstructorProtegido_CreaInstanciaConValoresPorDefectoOK()
    {
        FixtureDePrueba fixture = new FixtureDePrueba();

        Assert.AreEqual(string.Empty, fixture.NombreMotorSimulacion);
        Assert.AreEqual(0, fixture.Grupos.Count);
        Assert.AreEqual(0, fixture.PartidosEliminatorias.Count);
        Assert.IsFalse(fixture.CrucesGenerados);
    }

    [TestMethod]
    public void CrearFixtureValidoOK()
    {
        DateTime antes = DateTime.Now;
        Fixture fixture = CrearFixtureValido(42);
        DateTime despues = DateTime.Now;

        Assert.AreEqual(42, fixture.SemillaUtilizada);
        Assert.IsFalse(fixture.CrucesGenerados);
        Assert.AreEqual(0, fixture.Grupos.Count);
        Assert.AreEqual(0, fixture.PartidosEliminatorias.Count);
        Assert.IsTrue(fixture.FechaGeneracion >= antes && fixture.FechaGeneracion <= despues);
    }

    [TestMethod]
    public void CrearFixtureSinMotor_UsaProbabilisticoPorDefectoOK()
    {
        Fixture fixture = CrearFixtureValido(42);

        Assert.AreEqual(NombresMotores.Probabilistico, fixture.NombreMotorSimulacion);
    }

    [TestMethod]
    public void CrearFixtureConMotorAleatorioPuro_GuardaNombreMotorOK()
    {
        Fixture fixture = new Fixture(42, NombresMotores.AleatorioPuro);

        Assert.AreEqual(NombresMotores.AleatorioPuro, fixture.NombreMotorSimulacion);
    }

    [TestMethod]
    public void AgregarGrupoOK()
    {
        Fixture fixture = CrearFixtureValido();
        Grupo grupo = new Grupo("A");

        fixture.AgregarGrupo(grupo);

        Assert.AreEqual(1, fixture.Grupos.Count);
    }

    [TestMethod]
    public void AgregarGrupoNuloLanzaExcepcion()
    {
        Fixture fixture = CrearFixtureValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => fixture.AgregarGrupo(null!));
        Assert.AreEqual("El grupo es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void AgregarMasDe12GruposLanzaExcepcion()
    {
        Fixture fixture = CrearFixtureValido();
        string[] etiquetas = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
        foreach (string et in etiquetas)
            fixture.AgregarGrupo(new Grupo(et));

        DominioException ex = Assert.Throws<DominioException>(
            () => fixture.AgregarGrupo(new Grupo("A")));
        Assert.AreEqual("El fixture no puede contener más de 12 grupos.", ex.Message);
    }

    [TestMethod]
    public void EstaFaseGruposCompletaFalsoSiNoTodosLosGruposJugaronOK()
    {
        Fixture fixture = CrearFixtureValido();
        fixture.AgregarGrupo(CrearGrupoCon6PartidosJugados("A"));

        Assert.IsFalse(fixture.EstaFaseGruposCompleta());
    }

    [TestMethod]
    public void EstaFaseGruposCompletaTrueCon12GruposJugadosOK()
    {
        Fixture fixture = CrearFixtureValido();
        string[] etiquetas = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
        foreach (string et in etiquetas)
            fixture.AgregarGrupo(CrearGrupoCon6PartidosJugados(et));

        Assert.IsTrue(fixture.EstaFaseGruposCompleta());
    }

    [TestMethod]
    public void PuedeGenerarCrucesFalsoSiFaseGruposIncompletaOK()
    {
        Fixture fixture = CrearFixtureValido();
        fixture.AgregarGrupo(CrearGrupoCon6PartidosJugados("A"));

        Assert.IsFalse(fixture.PuedeGenerarCruces());
    }

    [TestMethod]
    public void PuedeGenerarCrucesTrueSiFaseCompletaYNoGeneradosOK()
    {
        Fixture fixture = CrearFixtureValido();
        string[] etiquetas = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
        foreach (string et in etiquetas)
            fixture.AgregarGrupo(CrearGrupoCon6PartidosJugados(et));

        Assert.IsTrue(fixture.PuedeGenerarCruces());
    }

    [TestMethod]
    public void PuedeGenerarCrucesFalsoSiYaGeneradosOK()
    {
        Fixture fixture = CrearFixtureValido();
        string[] etiquetas = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
        foreach (string et in etiquetas)
            fixture.AgregarGrupo(CrearGrupoCon6PartidosJugados(et));
        fixture.MarcarCrucesGenerados();

        Assert.IsFalse(fixture.PuedeGenerarCruces());
        Assert.IsTrue(fixture.CrucesGenerados);
    }

    [TestMethod]
    public void ObtenerTodosLosPartidosOK()
    {
        Fixture fixture = CrearFixtureValido();
        fixture.AgregarGrupo(CrearGrupoCon6PartidosJugados("A"));
        fixture.AgregarGrupo(CrearGrupoCon6PartidosJugados("B"));

        Partido eliminatorio = new Partido
        {
            Fecha = new DateTime(2026, 7, 1),
            Estadio = CrearEstadio(),
            EquipoLocal = CrearEquipo("X"),
            EquipoVisitante = CrearEquipo("Y"),
            Grupo = "A",
            Fase = FasePartido.OctavosDeFinal
        };
        fixture.AgregarPartidoEliminatoria(eliminatorio);

        List<Partido> todos = fixture.ObtenerTodosLosPartidos();
        
        Assert.AreEqual(13, todos.Count);
    }
}
