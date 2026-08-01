using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dominio.Excepciones;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
namespace Dominio.Tests;
using System;


[TestClass]
public class EquipoTest
{
    private class EquipoDePrueba : Equipo { }

    private Equipo CrearEquipoValido(
        string nombre = "Uruguay",
        Confederacion confederacion = Confederacion.CONMEBOL,
        int rankingFIFA = 1500)
    {
        return new Equipo(nombre, confederacion, rankingFIFA);
    }

    [TestMethod]
    public void Constructor_DatosValidos_CreaEquipo()
    {
        Equipo equipo = CrearEquipoValido();
        Assert.AreEqual("Uruguay", equipo.Nombre);
        Assert.AreEqual(Confederacion.CONMEBOL, equipo.Confederacion);
        Assert.AreEqual(1500, equipo.RankingFIFA);
        Assert.AreEqual(1500, equipo.RankingActual);
    }
    
    [TestMethod]
    public void Constructor_NombreNulo_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEquipoValido(nombre: null!));
        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 60 caracteres.", ex.Message);
    }
    [TestMethod]
    public void Constructor_NombreVacio_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEquipoValido(nombre: ""));
        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 60 caracteres.", ex.Message);
    }
    
    [TestMethod]
    public void Constructor_NombreVacioConEspacios_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEquipoValido(nombre: "   "));
        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 60 caracteres.", ex.Message);
    }
    [TestMethod]
    public void Constructor_NombreSuperaLongitudMaxima_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEquipoValido(nombre: new string('A', 61)));
        Assert.AreEqual("El nombre es obligatorio y debe tener entre 1 y 60 caracteres.", ex.Message);
    }
    [TestMethod]
    public void Constructor_NombreCon60Caracteres_NoLanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido(nombre: new string('A', 60));
        Assert.AreEqual(60, equipo.Nombre.Length);
    }
    [TestMethod]
    public void Constructor_RankingFIFAMenorA300_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEquipoValido(rankingFIFA: 299));
        Assert.AreEqual("El RankingFIFA debe estar entre 300 y 2500.", ex.Message);
    }
    [TestMethod]
    public void Constructor_RankingFIFAMayorA2500_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearEquipoValido(rankingFIFA: 2501));
        Assert.AreEqual("El RankingFIFA debe estar entre 300 y 2500.", ex.Message);
    }
    [TestMethod]
    public void Constructor_RankingFIFAEn300_NoLanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido(rankingFIFA: 300);
        Assert.AreEqual(300, equipo.RankingFIFA);
    }
    [TestMethod]
    public void Constructor_RankingFIFAEn2500_NoLanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido(rankingFIFA: 2500);
        Assert.AreEqual(2500, equipo.RankingFIFA);
    }
    [TestMethod]
    public void ActualizarRankingActual_ConValorValido_ActualizaRankingActual()
    {
        Equipo equipo = CrearEquipoValido();
        equipo.ActualizarRankingActual(1600);
        Assert.AreEqual(1600, equipo.RankingActual);
    }
    [TestMethod]
    public void ActualizarRankingActual_ConValorValido_NoModificaRankingFIFA()
    {
        Equipo equipo = CrearEquipoValido();
        equipo.ActualizarRankingActual(1600);
        Assert.AreEqual(1500, equipo.RankingFIFA);
    }
    [TestMethod]
    public void ActualizarRankingActual_ConValorFueraDeRango_LanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => equipo.ActualizarRankingActual(299));
        Assert.AreEqual("El RankingFIFA debe estar entre 300 y 2500.", ex.Message);
    }
    [TestMethod]
    public void ActualizarRankingActual_ConValorMayorA2500_LanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido();
        DominioException ex = Assert.Throws<DominioException>(
            () => equipo.ActualizarRankingActual(2501));
        Assert.AreEqual("El RankingFIFA debe estar entre 300 y 2500.", ex.Message);
    }
    [TestMethod]
    public void Constructor_ConfederacionInvalida_LanzaExcepcion()
    {
        Assert.Throws<DominioException>(() => CrearEquipoValido(confederacion: (Confederacion)999));
    }

    [TestMethod]
    public void BanderaBase64_SinEstablecer_EsNula()
    {
        Equipo equipo = CrearEquipoValido();

        Assert.IsNull(equipo.BanderaBase64,
            "La bandera debe ser null si nunca se llamó a EstablecerBandera");
    }

    [TestMethod]
    public void EstablecerBandera_StringBase64Valido_GuardaBandera()
    {
        Equipo equipo = CrearEquipoValido();
        string bandera = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });

        equipo.EstablecerBandera(bandera);

        Assert.AreEqual(bandera, equipo.BanderaBase64,
            "La bandera debe guardarse después de llamar a EstablecerBandera");
    }

    [TestMethod]
    public void EstablecerBandera_StringVacio_LanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido();

        DominioException ex = Assert.Throws<DominioException>(
            () => equipo.EstablecerBandera(string.Empty));
        Assert.AreEqual("La bandera no puede estar vacía.", ex.Message);
    }

    [TestMethod]
    public void EstablecerBandera_StringEspacios_LanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido();

        DominioException ex = Assert.Throws<DominioException>(
            () => equipo.EstablecerBandera("   "));
        Assert.AreEqual("La bandera no puede estar vacía.", ex.Message);
    }

    [TestMethod]
    public void EstablecerBandera_Null_LanzaExcepcion()
    {
        Equipo equipo = CrearEquipoValido();

        DominioException ex = Assert.Throws<DominioException>(
            () => equipo.EstablecerBandera(null!));
        Assert.AreEqual("La bandera no puede estar vacía.", ex.Message);
    }

    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        Equipo equipo = CrearEquipoValido();

        equipo.Id = 7;

        Assert.AreEqual(7, equipo.Id);
    }

    [TestMethod]
    public void ConstructorProtegido_CreaInstanciaConValoresPorDefectoOK()
    {
        EquipoDePrueba equipo = new EquipoDePrueba();

        Assert.AreEqual(string.Empty, equipo.Nombre);
        Assert.AreEqual(0, equipo.RankingFIFA);
        Assert.AreEqual(0, equipo.RankingActual);
    }

    [TestMethod]
    public void EstablecerBandera_DosVeces_SobreescribeBandera()
    {
        Equipo equipo = CrearEquipoValido();
        string banderaOriginal = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        string banderaNueva = Convert.ToBase64String(new byte[] { 4, 5, 6 });

        equipo.EstablecerBandera(banderaOriginal);
        equipo.EstablecerBandera(banderaNueva);

        Assert.AreEqual(banderaNueva, equipo.BanderaBase64,
            "Llamar EstablecerBandera dos veces debe sobreescribir la bandera anterior");
    }
}