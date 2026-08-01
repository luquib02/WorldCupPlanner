using System;
using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using Dominio;
namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(Partido))]
public class PartidoTest
{
    private Equipo CrearEquipoValido(string nombre = "Argentina")
    {
        return new Equipo(nombre, Confederacion.CONMEBOL, 1500);
    }

    private Estadio CrearEstadioValido()
    {
        return new Estadio()
        {
            Nombre = "Estadio Centenario",
            Ciudad = "Montevideo",
            Capacidad = 60000
        };
    }

    private Partido CrearPartidoValido(
        DateTime? fecha = null,
        Estadio? estadio = null,
        Equipo? local = null,
        Equipo? visitante = null,
        string grupo = "A",
        FasePartido fase = FasePartido.FaseDeGrupos)
    {
        return new Partido()
        {
            Fecha = fecha ?? DateTime.Now.AddDays(1),
            Estadio = estadio ?? CrearEstadioValido(),
            EquipoLocal = local ?? CrearEquipoValido("Argentina"),
            EquipoVisitante = visitante ?? CrearEquipoValido("Brasil"),
            Grupo = grupo,
            Fase = fase
        };
    }

    [TestMethod]
    public void CrearPartidoDatosValidosOK()
    {
        Partido partido = CrearPartidoValido();

        Assert.IsNotNull(partido.Estadio);
        Assert.IsNotNull(partido.EquipoLocal);
        Assert.IsNotNull(partido.EquipoVisitante);
        Assert.AreEqual("A", partido.Grupo);
        Assert.IsFalse(partido.EstaJugado());
        Assert.IsNull(partido.GolesLocal);
        Assert.IsNull(partido.GolesVisitante);
    }

    [TestMethod]
    public void CrearPartidoFechaValidaOK()
    {
        DateTime fecha = new DateTime(2026, 6, 1);
        Partido partido = CrearPartidoValido(fecha: fecha);

        Assert.AreEqual(fecha, partido.Fecha);
    }

    [TestMethod]
    public void CrearPartidoEstadioNuloLanzaExcepcion()
    {
        Partido partido = new Partido();
        DominioException ex = Assert.Throws<DominioException>(
            () => partido.Estadio = null!);
        Assert.AreEqual("El estadio es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void CrearPartidoEquipoLocalNuloLanzaExcepcion()
    {
        Partido partido = new Partido();
        DominioException ex = Assert.Throws<DominioException>(
            () => partido.EquipoLocal = null!);
        Assert.AreEqual("El equipo local es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void CrearPartidoEquipoVisitanteNuloLanzaExcepcion()
    {
        Partido partido = new Partido();
        DominioException ex = Assert.Throws<DominioException>(
            () => partido.EquipoVisitante = null!);
        Assert.AreEqual("El equipo visitante es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void CrearPartidoEquiposIgualesLanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido("Argentina");
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearPartidoValido(local: equipo, visitante: equipo));
        Assert.AreEqual("El equipo local y visitante no pueden ser el mismo.", ex.Message);
    }

    [TestMethod]
    public void CrearPartidoGrupoValidoOK()
    {
        foreach (string grupo in new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" })
        {
            Partido partido = CrearPartidoValido(grupo: grupo);
            Assert.AreEqual(grupo, partido.Grupo);
        }
    }

    [TestMethod]
    public void CrearPartidoGrupoInvalidoLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearPartidoValido(grupo: "Z"));
        Assert.AreEqual("El grupo debe ser una letra entre A y L.", ex.Message);
    }

    [TestMethod]
    public void CrearPartidoGrupoVacioLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearPartidoValido(grupo: ""));
        Assert.AreEqual("El grupo debe ser una letra entre A y L.", ex.Message);
    }

    [TestMethod]
    public void CrearPartidoGrupoNuloLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearPartidoValido(grupo: null!));
        Assert.AreEqual("El grupo debe ser una letra entre A y L.", ex.Message);
    }

    [TestMethod]
    public void CargarResultadoValidoOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(2, 1);

        Assert.AreEqual(2, partido.GolesLocal);
        Assert.AreEqual(1, partido.GolesVisitante);
        Assert.IsTrue(partido.EstaJugado());
    }

    [TestMethod]
    public void CargarResultadoEmpateOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(0, 0);

        Assert.IsTrue(partido.EsEmpate());
        Assert.IsTrue(partido.EstaJugado());
    }

    [TestMethod]
    public void CargarResultadoGolesNegativosLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => partido.CargarResultado(-1, 0));
        Assert.AreEqual("Los goles no pueden ser negativos.", ex.Message);
    }

    [TestMethod]
    public void CargarResultadoGolesVisitanteNegativosLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => partido.CargarResultado(0, -1));
        Assert.AreEqual("Los goles no pueden ser negativos.", ex.Message);
    }

    [TestMethod]
    public void ObtenerVencedorLocalGanaOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(3, 1);

        Assert.AreEqual(partido.EquipoLocal, partido.ObtenerVencedor());
    }

    [TestMethod]
    public void ObtenerVencedorVisitanteGanaOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(0, 2);

        Assert.AreEqual(partido.EquipoVisitante, partido.ObtenerVencedor());
    }

    [TestMethod]
    public void ObtenerVencedorSinResultadoLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => partido.ObtenerVencedor());
        Assert.AreEqual("El partido no ha sido jugado.", ex.Message);
    }

    [TestMethod]
    public void ObtenerPerdedorOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(3, 0);

        Assert.AreEqual(partido.EquipoVisitante, partido.ObtenerPerdedor());
    }

    [TestMethod]
    public void ObtenerPerdedorSinResultadoLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => partido.ObtenerPerdedor());
        Assert.AreEqual("El partido no ha sido jugado.", ex.Message);
    }
    
    [TestMethod]
    public void CrearPartidoEtiquetaCruceOpcionalOK()
    {
        Partido partido = CrearPartidoValido();
        Assert.IsNull(partido.EtiquetaCruce);
    }

    [TestMethod]
    public void CrearPartidoPartidoOrigenLocalOpcionalOK()
    {
        Partido partido = CrearPartidoValido();
        Assert.IsNull(partido.PartidoOrigenLocal);
    }

    [TestMethod]
    public void CrearPartidoPartidoOrigenVisitanteOpcionalOK()
    {
        Partido partido = CrearPartidoValido();
        Assert.IsNull(partido.PartidoOrigenVisitante);
    }

    [TestMethod]
    public void CrearPartidoPenalesOpcionalesOK()
    {
        Partido partido = CrearPartidoValido();
        Assert.IsNull(partido.GolesPenalesLocal);
        Assert.IsNull(partido.GolesPenalesVisitante);
    }

    [TestMethod]
    public void PenalesNegativosLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        Assert.Throws<DominioException>(
            () => partido.CargarPenales(-1, 0));
    }

    [TestMethod]
    public void PenalesEmpatados_LanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        Assert.Throws<DominioException>(
            () => partido.CargarPenales(0, 0));
    }
    
    [TestMethod]
    public void CrearPartidoConOrigenesAsignadosOK()
    {
        Partido origenLocal = CrearPartidoValido();
        Partido origenVisitante = CrearPartidoValido();
        Partido partido = CrearPartidoValido();

        partido.PartidoOrigenLocal = origenLocal;
        partido.PartidoOrigenVisitante = origenVisitante;

        Assert.AreEqual(origenLocal, partido.PartidoOrigenLocal);
        Assert.AreEqual(origenVisitante, partido.PartidoOrigenVisitante);
    }

    [TestMethod]
    public void ValidarPartidoValidoNoLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        partido.Validar();
    }

    [TestMethod]
    public void AgregarIncidencia_TarjetaAmarilla_SeAgregaAIncidenciasLocalesOK()
    {
        Partido partido = CrearPartidoValido();
        TarjetaAmarilla tarjeta = new TarjetaAmarilla(30, esLocal: true);

        partido.AgregarIncidencia(tarjeta);

        Assert.AreEqual(1, partido.IncidenciasLocal.Count,
            "La tarjeta amarilla local debe aparecer en IncidenciasLocal");
        Assert.AreEqual(0, partido.IncidenciasVisitante.Count,
            "No debe haber incidencias visitante");
    }

    [TestMethod]
    public void AgregarIncidencia_TarjetaRoja_SeAgregaAIncidenciasVisitanteOK()
    {
        Partido partido = CrearPartidoValido();
        TarjetaRoja tarjeta = new TarjetaRoja(60, esLocal: false);

        partido.AgregarIncidencia(tarjeta);

        Assert.AreEqual(0, partido.IncidenciasLocal.Count,
            "No debe haber incidencias locales");
        Assert.AreEqual(1, partido.IncidenciasVisitante.Count,
            "La tarjeta roja visitante debe aparecer en IncidenciasVisitante");
    }

    [TestMethod]
    public void AgregarIncidencia_Nula_LanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();

        DominioException ex = Assert.Throws<DominioException>(
            () => partido.AgregarIncidencia(null!));
        Assert.AreEqual("La incidencia no puede ser nula.", ex.Message);
    }

    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        Partido partido = CrearPartidoValido();

        partido.Id = 15;

        Assert.AreEqual(15, partido.Id);
    }

    [TestMethod]
    public void CargarPenalesValidoOK()
    {
        Partido partido = CrearPartidoValido();

        partido.CargarPenales(4, 2);

        Assert.AreEqual(4, partido.GolesPenalesLocal);
        Assert.AreEqual(2, partido.GolesPenalesVisitante);
    }

    [TestMethod]
    public void EsEmpateSinResultadoLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();

        DominioException ex = Assert.Throws<DominioException>(
            () => partido.EsEmpate());
        Assert.AreEqual("El partido no ha sido jugado.", ex.Message);
    }

    [TestMethod]
    public void ObtenerVencedorEmpateConPenalesDefinidos_DevuelveGanadorPorPenalesOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(1, 1);
        partido.CargarPenales(4, 2);

        Assert.AreEqual(partido.EquipoLocal, partido.ObtenerVencedor());
    }

    [TestMethod]
    public void ObtenerVencedorEmpateSinPenalesLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(1, 1);

        DominioException ex = Assert.Throws<DominioException>(
            () => partido.ObtenerVencedor());
        Assert.AreEqual("El partido terminó empatado.", ex.Message);
    }

    [TestMethod]
    public void ObtenerPerdedorVisitanteGanaOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(0, 2);

        Assert.AreEqual(partido.EquipoLocal, partido.ObtenerPerdedor());
    }

    [TestMethod]
    public void ObtenerPerdedorEmpateConPenalesDefinidos_DevuelvePerdedorPorPenalesOK()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(1, 1);
        partido.CargarPenales(4, 2);

        Assert.AreEqual(partido.EquipoVisitante, partido.ObtenerPerdedor());
    }

    [TestMethod]
    public void ObtenerPerdedorEmpateSinPenalesLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        partido.CargarResultado(1, 1);

        DominioException ex = Assert.Throws<DominioException>(
            () => partido.ObtenerPerdedor());
        Assert.AreEqual("El partido terminó empatado.", ex.Message);
    }

    [TestMethod]
    public void ValidarSinEstadioLanzaExcepcion()
    {
        Partido partido = new Partido();

        DominioException ex = Assert.Throws<DominioException>(
            () => partido.Validar());
        Assert.AreEqual("El estadio es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void ValidarSinEquipoLocalLanzaExcepcion()
    {
        Partido partido = new Partido { Estadio = CrearEstadioValido() };

        DominioException ex = Assert.Throws<DominioException>(
            () => partido.Validar());
        Assert.AreEqual("El equipo local es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void ValidarSinEquipoVisitanteLanzaExcepcion()
    {
        Partido partido = new Partido
        {
            Estadio = CrearEstadioValido(),
            EquipoLocal = CrearEquipoValido("Argentina")
        };

        DominioException ex = Assert.Throws<DominioException>(
            () => partido.Validar());
        Assert.AreEqual("El equipo visitante es obligatorio.", ex.Message);
    }

    [TestMethod]
    public void AgregarIncidencia_MultiplesTarjetas_FiltranCorrectamenteOK()
    {
        Partido partido = CrearPartidoValido();

        partido.AgregarIncidencia(new TarjetaAmarilla(10, esLocal: true));
        partido.AgregarIncidencia(new TarjetaAmarilla(25, esLocal: true));
        partido.AgregarIncidencia(new TarjetaRoja(80, esLocal: false));

        Assert.AreEqual(2, partido.IncidenciasLocal.Count,
            "Deben contarse solo las incidencias del equipo local");
        Assert.AreEqual(1, partido.IncidenciasVisitante.Count,
            "Debe contarse solo la incidencia del equipo visitante");
    }
}
