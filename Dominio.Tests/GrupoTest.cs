using System;
using System.Collections.Generic;
using System.Linq;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Dominio.Tests;

[TestClass]
public class GrupoTest
{
    private class GrupoDePrueba : Grupo { }

    private Equipo CrearEquipo(string nombre, Confederacion confederacion = Confederacion.CONMEBOL)
    {
        return new Equipo(nombre, confederacion, 1500);
    }

    private Grupo CrearGrupoValido(string etiqueta = "A")
    {
        return new Grupo(etiqueta);
    }

    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        Grupo grupo = CrearGrupoValido();

        grupo.Id = 4;

        Assert.AreEqual(4, grupo.Id);
    }

    [TestMethod]
    public void ConstructorProtegido_CreaInstanciaConValoresPorDefectoOK()
    {
        GrupoDePrueba grupo = new GrupoDePrueba();

        Assert.AreEqual(string.Empty, grupo.Etiqueta);
        Assert.AreEqual(0, grupo.Equipos.Count);
        Assert.AreEqual(0, grupo.Jornadas.Count);
    }

    [TestMethod]
    public void CrearGrupoValidoOK()
    {
        Grupo grupo = CrearGrupoValido("B");

        Assert.AreEqual("B", grupo.Etiqueta);
        Assert.AreEqual(0, grupo.Equipos.Count);
        Assert.AreEqual(0, grupo.Jornadas.Count);
    }
    [TestMethod]
    public void CrearGrupoEtiquetaInvalidaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearGrupoValido("Z"));
        Assert.AreEqual("La etiqueta del grupo debe ser una letra entre A y L.", ex.Message);
    }
    [TestMethod]
    public void CrearGrupoEtiquetaNulaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearGrupoValido(null!));
        Assert.AreEqual("La etiqueta del grupo debe ser una letra entre A y L.", ex.Message);
    }
    [TestMethod]
    public void AgregarEquipoOK()
    {
        Grupo grupo = CrearGrupoValido();
        Equipo equipo = CrearEquipo("Argentina");

        grupo.AgregarEquipo(equipo);

        Assert.AreEqual(1, grupo.Equipos.Count);
    }
    [TestMethod]
    public void AgregarEquipoNuloLanzaExcepcion()
    {
        Grupo grupo = CrearGrupoValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => grupo.AgregarEquipo(null!));
        Assert.AreEqual("El equipo es obligatorio.", ex.Message);
    }
    [TestMethod]
    public void AgregarEquipoDuplicadoLanzaExcepcion()
    {
        Grupo grupo = CrearGrupoValido();
        Equipo equipo = CrearEquipo("Argentina");
        grupo.AgregarEquipo(equipo);

        DominioException ex = Assert.Throws<DominioException>(
            () => grupo.AgregarEquipo(equipo));
        Assert.AreEqual("El equipo ya está en el grupo.", ex.Message);
    }
    [TestMethod]
    public void AgregarMasDe4EquiposLanzaExcepcion()
    {
        Grupo grupo = CrearGrupoValido();
        grupo.AgregarEquipo(CrearEquipo("E1"));
        grupo.AgregarEquipo(CrearEquipo("E2"));
        grupo.AgregarEquipo(CrearEquipo("E3"));
        grupo.AgregarEquipo(CrearEquipo("E4"));

        DominioException ex = Assert.Throws<DominioException>(
            () => grupo.AgregarEquipo(CrearEquipo("E5")));
        Assert.AreEqual("El grupo no puede contener más de 4 equipos.", ex.Message);
    }
    [TestMethod]
    public void ContieneEquiposMismaConfederacionOK()
    {
        Grupo grupo = CrearGrupoValido();
        grupo.AgregarEquipo(CrearEquipo("Argentina", Confederacion.CONMEBOL));
        grupo.AgregarEquipo(CrearEquipo("Brasil", Confederacion.CONMEBOL));
        grupo.AgregarEquipo(CrearEquipo("Francia", Confederacion.UEFA));

        Assert.AreEqual(2, grupo.ContieneEquiposMismaConfederacion(Confederacion.CONMEBOL));
        Assert.AreEqual(1, grupo.ContieneEquiposMismaConfederacion(Confederacion.UEFA));
        Assert.AreEqual(0, grupo.ContieneEquiposMismaConfederacion(Confederacion.AFC));
    }
    
    [TestMethod]
    public void ObtenerPartidosOK()
    {
        Grupo grupo = CrearGrupoValido();
        Jornada j1 = new Jornada(1, new DateTime(2026, 6, 1));
        Jornada j2 = new Jornada(2, new DateTime(2026, 6, 4));

        Estadio estadio = new Estadio { Nombre = "E", Ciudad = "C", Capacidad = 20000 };
        Partido p1 = new Partido
        {
            Fecha = new DateTime(2026, 6, 1),
            Estadio = estadio,
            EquipoLocal = CrearEquipo("L1"),
            EquipoVisitante = CrearEquipo("V1"),
            Grupo = "A",
            Fase = FasePartido.FaseDeGrupos
        };
        Partido p2 = new Partido
        {
            Fecha = new DateTime(2026, 6, 4),
            Estadio = estadio,
            EquipoLocal = CrearEquipo("L2"),
            EquipoVisitante = CrearEquipo("V2"),
            Grupo = "A",
            Fase = FasePartido.FaseDeGrupos
        };
        j1.AgregarPartido(p1);
        j2.AgregarPartido(p2);
        grupo.AgregarJornada(j1);
        grupo.AgregarJornada(j2);

        List<Partido> partidos = grupo.ObtenerPartidos();

        Assert.AreEqual(2, partidos.Count);
        Assert.IsTrue(partidos.Contains(p1));
        Assert.IsTrue(partidos.Contains(p2));
    }
    [TestMethod]
    public void ObtenerTablaPosicionesSinPartidosJugadosOK()
    {
        Grupo grupo = CrearGrupoValido();
        grupo.AgregarEquipo(CrearEquipo("E1"));
        grupo.AgregarEquipo(CrearEquipo("E2"));

        List<PosicionEquipo> tabla = grupo.ObtenerTablaPosiciones();

        Assert.AreEqual(2, tabla.Count);
        Assert.IsTrue(tabla.All(p => p.Puntos == 0));
    }
    [TestMethod]
    public void ObtenerTablaPosicionesConClasificadorCalculaPuntosYOrdenaOK()
    {
        Grupo grupo = CrearGrupoValido();
        Equipo argentina = CrearEquipo("Argentina");
        Equipo brasil = CrearEquipo("Brasil");
        grupo.AgregarEquipo(argentina);
        grupo.AgregarEquipo(brasil);

        Estadio estadio = new Estadio { Nombre = "E", Ciudad = "C", Capacidad = 20000 };
        Partido partido = new Partido
        {
            Fecha = new DateTime(2026, 6, 1),
            Estadio = estadio,
            EquipoLocal = argentina,
            EquipoVisitante = brasil,
            Grupo = "A",
            Fase = FasePartido.FaseDeGrupos
        };
        partido.CargarResultado(2, 0);
        Jornada jornada = new Jornada(1, new DateTime(2026, 6, 1));
        jornada.AgregarPartido(partido);
        grupo.AgregarJornada(jornada);

        List<PosicionEquipo> tabla = grupo.ObtenerTablaPosiciones(new Dominio.Clasificacion.ClasificadorFifa());

        Assert.AreEqual(2, tabla.Count);
        Assert.AreEqual(argentina, tabla[0].Equipo);
        Assert.AreEqual(3, tabla[0].Puntos);
        Assert.AreEqual(2, tabla[0].GolesAFavor);
        Assert.AreEqual(brasil, tabla[1].Equipo);
        Assert.AreEqual(0, tabla[1].Puntos);
    }
    [TestMethod]
    public void ObtenerTablaPosicionesConClasificador_PartidoNoJugado_SeOmiteDeLaTablaOK()
    {
        Grupo grupo = CrearGrupoValido();
        Equipo argentina = CrearEquipo("Argentina");
        Equipo brasil = CrearEquipo("Brasil");
        grupo.AgregarEquipo(argentina);
        grupo.AgregarEquipo(brasil);

        Estadio estadio = new Estadio { Nombre = "E", Ciudad = "C", Capacidad = 20000 };
        Partido partidoNoJugado = new Partido
        {
            Fecha = new DateTime(2026, 6, 1),
            Estadio = estadio,
            EquipoLocal = argentina,
            EquipoVisitante = brasil,
            Grupo = "A",
            Fase = FasePartido.FaseDeGrupos
        };
        Jornada jornada = new Jornada(1, new DateTime(2026, 6, 1));
        jornada.AgregarPartido(partidoNoJugado);
        grupo.AgregarJornada(jornada);

        List<PosicionEquipo> tabla = grupo.ObtenerTablaPosiciones(new Dominio.Clasificacion.ClasificadorFifa());

        Assert.AreEqual(2, tabla.Count);
        Assert.IsTrue(tabla.All(p => p.Puntos == 0),
            "El partido no jugado no debe afectar los puntos de la tabla");
    }

    [TestMethod]
    public void AgregarMasDe3JornadasLanzaExcepcion()
    {
        Grupo grupo = CrearGrupoValido();
        grupo.AgregarJornada(new Jornada(1, new DateTime(2026, 6, 1)));
        grupo.AgregarJornada(new Jornada(2, new DateTime(2026, 6, 4)));
        grupo.AgregarJornada(new Jornada(3, new DateTime(2026, 6, 7)));

        DominioException ex = Assert.Throws<DominioException>(
            () => grupo.AgregarJornada(new Jornada(1, new DateTime(2026, 6, 10))));
        Assert.AreEqual("El grupo no puede contener más de 3 jornadas.", ex.Message);
    }
}