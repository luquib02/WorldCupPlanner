using Dominio;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositorio;
using Sistema.Importacion;
using Sistema.Importacion.FacilidadComprension;

namespace Sistema.Tests;

[TestClass]
public class ImportacionServicioLegibleTest
{
    private Mock<IEquipoRepositorio> _equipoRepoMock;
    private ImportacionServicioLegible _servicio;

    [TestInitialize]
    public void Setup()
    {
        _equipoRepoMock = new Mock<IEquipoRepositorio>();
        _servicio = new ImportacionServicioLegible(_equipoRepoMock.Object);
    }
    
    private const string CsvValido =
        "Nombre,Confederacion,RankingFIFA\n" +
        "Argentina,CONMEBOL,1900\n" +
        "Francia,UEFA,1840\n";

    private const string CsvUnaFila =
        "Nombre,Confederacion,RankingFIFA\n" +
        "Brasil,CONMEBOL,1880\n";
    

    [TestMethod]
    public void ImportarCsvValidoRetornaExitosOK()
    {
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(CsvValido);

        Assert.AreEqual(2, resultado.Exitosos);
        Assert.IsFalse(resultado.TieneErrores);
    }

    [TestMethod]
    public void ImportarCsvValidoAgregaEquiposAlRepositorioOK()
    {
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        _servicio.Importar(CsvValido);

        _equipoRepoMock.Verify(r => r.Agregar(It.IsAny<Equipo>()), Times.Exactly(2));
    }

    [TestMethod]
    public void ImportarCsvUnaFilaRetornaUnExitoOK()
    {
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(CsvUnaFila);

        Assert.AreEqual(1, resultado.Exitosos);
        Assert.AreEqual(0, resultado.Errores.Count);
    }

    [TestMethod]
    public void ImportarCsvConEspaciosEnBlancoCorriosOK()
    {
        string csv = "Nombre , Confederacion , RankingFIFA \n  España , UEFA , 1850 \n";
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(csv);

        Assert.AreEqual(1, resultado.Exitosos);
    }

    [TestMethod]
    public void ImportarCsvIgnoraLineasVaciasOK()
    {
        string csv = "Nombre,Confederacion,RankingFIFA\n\nJapon,AFC,1750\n\n";
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(csv);

        Assert.AreEqual(1, resultado.Exitosos);
    }
    

    [TestMethod]
    public void ImportarCsvSoloEncabezadoSinDatosLanzaExcepcion()
    {
        string csv = "Nombre,Confederacion,RankingFIFA\n";

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Importar(csv));

        Assert.AreEqual("El archivo CSV no contiene datos.", ex.Message);
    }

    [TestMethod]
    public void ImportarCsvVacioLanzaExcepcion()
    {
        string csv = "";

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Importar(csv));

        Assert.AreEqual("El archivo CSV no contiene datos.", ex.Message);
    }

    [TestMethod]
    public void ImportarCsvSinColumnaNombreLanzaExcepcion()
    {
        string csv = "Confederacion,RankingFIFA\nCONMEBOL,1800\n";

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Importar(csv));

        Assert.AreEqual("El archivo CSV no contiene la columna obligatoria: Nombre", ex.Message);
    }

    [TestMethod]
    public void ImportarCsvSinColumnaConfederacionLanzaExcepcion()
    {
        string csv = "Nombre,RankingFIFA\nArgentina,1900\n";

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Importar(csv));

        Assert.AreEqual("El archivo CSV no contiene la columna obligatoria: Confederacion", ex.Message);
    }

    [TestMethod]
    public void ImportarCsvSinColumnaRankingFIFALanzaExcepcion()
    {
        string csv = "Nombre,Confederacion\nArgentina,CONMEBOL\n";

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Importar(csv));

        Assert.AreEqual("El archivo CSV no contiene la columna obligatoria: RankingFIFA", ex.Message);
    }

    [TestMethod]
    public void ImportarCsvConConfederacionInvalidaRegistraErrorOK()
    {
        string csv = "Nombre,Confederacion,RankingFIFA\nEquipoX,INVALIDA,1500\n";
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(csv);

        Assert.AreEqual(0, resultado.Exitosos);
        Assert.AreEqual(1, resultado.Errores.Count);
        Assert.IsTrue(resultado.TieneErrores);
    }

    [TestMethod]
    public void ImportarCsvConRankingNoNumericoRegistraErrorOK()
    {
        string csv = "Nombre,Confederacion,RankingFIFA\nEquipoX,UEFA,noEsNumero\n";
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(csv);

        Assert.AreEqual(0, resultado.Exitosos);
        Assert.AreEqual(1, resultado.Errores.Count);
    }

    [TestMethod]
    public void ImportarCsvConNombreDuplicadoRegistraErrorOK()
    {
        string csv = "Nombre,Confederacion,RankingFIFA\nArgentina,CONMEBOL,1900\n";
        _equipoRepoMock.Setup(r => r.ExisteNombre("Argentina")).Returns(true);

        ResultadoImportacion resultado = _servicio.Importar(csv);

        Assert.AreEqual(0, resultado.Exitosos);
        Assert.AreEqual(1, resultado.Errores.Count);
        Assert.IsTrue(resultado.Errores[0].Contains("Argentina"));
    }

    [TestMethod]
    public void ImportarCsvMezclaExitosYErroresOK()
    {
        string csv =
            "Nombre,Confederacion,RankingFIFA\n" +
            "Argentina,CONMEBOL,1900\n" +
            "Invalido,MADEUP,1500\n" +
            "Francia,UEFA,1840\n";

        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(csv);

        Assert.AreEqual(2, resultado.Exitosos);
        Assert.AreEqual(1, resultado.Errores.Count);
    }

    [TestMethod]
    public void ImportarCsvConRankingFueraDeLimitesRegistraErrorOK()
    {

        string csv = "Nombre,Confederacion,RankingFIFA\nEquipoX,UEFA,100\n";
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        ResultadoImportacion resultado = _servicio.Importar(csv);

        Assert.AreEqual(0, resultado.Exitosos);
        Assert.AreEqual(1, resultado.Errores.Count);
    }

    [TestMethod]
    public void ImportarCsvNoAgregaAlRepositorioCuandoHayErrorEnFila()
    {
        string csv = "Nombre,Confederacion,RankingFIFA\nEquipoX,INVALIDA,1500\n";
        _equipoRepoMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);

        _servicio.Importar(csv);

        _equipoRepoMock.Verify(r => r.Agregar(It.IsAny<Equipo>()), Times.Never);
    }
}
