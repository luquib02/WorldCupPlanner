using System.Collections.Generic;
using Dominio;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositorio;
using Sistema;
using Sistema.DTOs;

namespace Sistema.Tests;

[TestClass]
public class EquipoServicioTest
{
    private Mock<IEquipoRepositorio> _repositorioMock;
    private Mock<IRepositorioLog> _logRepoMock;
    private EquipoServicio _equipoServicio;

    [TestInitialize]
    public void Setup()
    {
        _repositorioMock = new Mock<IEquipoRepositorio>();
        _logRepoMock = new Mock<IRepositorioLog>();
        LogServicio logServicio = new LogServicio(_logRepoMock.Object);
        _equipoServicio = new EquipoServicio(_repositorioMock.Object, logServicio);
    }

    private Equipo CrearEquipoValido(
        string nombre = "Uruguay",
        Confederacion confederacion = Confederacion.CONMEBOL,
        int rankingFIFA = 1500)
    {
        return new Equipo(nombre, confederacion, rankingFIFA);
    }

    private EquipoDTO CrearEquipoDTOValido(
        string nombre = "Uruguay",
        Confederacion confederacion = Confederacion.CONMEBOL,
        int rankingFIFA = 1500)
    {
        return new EquipoDTO
        {
            Nombre = nombre,
            Confederacion = confederacion,
            RankingFIFA = rankingFIFA,
            RankingActual = rankingFIFA
        };
    }

    [TestMethod]
    public void AgregarEquipoConDatosValidosOK()
    {
        EquipoDTO equipoDTO = CrearEquipoDTOValido();

        _equipoServicio.Agregar(equipoDTO, "admin@test.com");

        _repositorioMock.Verify(r => r.Agregar(It.Is<Equipo>(e => e.Nombre == "Uruguay")), Times.Once);
    }

    [TestMethod]
    public void AgregarEquipoRegistraLogOK()
    {
        EquipoDTO equipoDTO = CrearEquipoDTOValido();

        _equipoServicio.Agregar(equipoDTO, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "AltaEquipo" && l.UsuarioEmail == "admin@test.com"
        )), Times.Once);
    }

    [TestMethod]
    public void AgregarEquipoNombreDuplicadoLanzaExcepcion()
    {
        EquipoDTO equipoDTO = CrearEquipoDTOValido();
        _repositorioMock.Setup(r => r.ExisteNombre("Uruguay")).Returns(true);

        DominioException ex = Assert.Throws<DominioException>(
            () => _equipoServicio.Agregar(equipoDTO, "admin@test.com"));
        Assert.AreEqual("Ya existe un equipo con ese nombre.", ex.Message);
    }

    [TestMethod]
    public void AgregarEquipoCupoConfederacionExcedidoLanzaExcepcion()
    {
        EquipoDTO equipoDTO = CrearEquipoDTOValido(confederacion: Confederacion.CONMEBOL);
        _repositorioMock.Setup(r => r.ExisteNombre(It.IsAny<string>())).Returns(false);
        _repositorioMock.Setup(r => r.ContarPorConfederacion(Confederacion.CONMEBOL)).Returns(7);

        DominioException ex = Assert.Throws<DominioException>(
            () => _equipoServicio.Agregar(equipoDTO, "admin@test.com"));
        Assert.AreEqual("Se ha alcanzado el cupo maximo para la confederacion CONMEBOL.", ex.Message);
    }

    [TestMethod]
    public void ObtenerPorIdEquipoExistenteRetornaEquipoOK()
    {
        Equipo equipo = CrearEquipoValido();
        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns(equipo);

        EquipoDTO resultado = _equipoServicio.ObtenerPorId(1);

        Assert.AreEqual(equipo.Nombre, resultado.Nombre);
        Assert.AreEqual(equipo.Confederacion, resultado.Confederacion);
    }

    [TestMethod]
    public void ObtenerPorIdEquipoNoExistenteLanzaExcepcion()
    {
        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns((Equipo?)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _equipoServicio.ObtenerPorId(1));
        Assert.AreEqual("No se encontro el equipo con id 1.", ex.Message);
    }

    [TestMethod]
    public void ObtenerTodosRetornaListaDeEquiposOK()
    {
        List<Equipo> equipos = new List<Equipo>
        {
            CrearEquipoValido("Uruguay"),
            CrearEquipoValido("Argentina")
        };
        _repositorioMock.Setup(r => r.ObtenerTodos()).Returns(equipos);

        List<EquipoDTO> resultado = _equipoServicio.ObtenerTodos();

        Assert.AreEqual(2, resultado.Count);
    }

    [TestMethod]
    public void EliminarEquipoExistenteOK()
    {
        Equipo equipo = CrearEquipoValido();
        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns(equipo);

        _equipoServicio.Eliminar(1, "admin@test.com");

        _repositorioMock.Verify(r => r.Eliminar(1), Times.Once);
    }

    [TestMethod]
    public void EliminarEquipoRegistraLogOK()
    {
        Equipo equipo = CrearEquipoValido();
        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns(equipo);

        _equipoServicio.Eliminar(1, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "EliminacionEquipo"
        )), Times.Once);
    }

    [TestMethod]
    public void EliminarEquipoNoExistenteLanzaExcepcion()
    {
        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns((Equipo?)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _equipoServicio.Eliminar(1, "admin@test.com"));
        Assert.AreEqual("No se encontro el equipo con id 1.", ex.Message);
    }

    [TestMethod]
    public void ActualizarEquipoValidoOK()
    {
        Equipo equipo = CrearEquipoValido();
        equipo.Id = 1;
        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns(equipo);
        _repositorioMock.Setup(r => r.ObtenerTodos()).Returns(new List<Equipo> { equipo });

        EquipoDTO equipoDTO = CrearEquipoDTOValido();
        equipoDTO.Id = 1;

        _equipoServicio.Actualizar(equipoDTO, "admin@test.com");

        _repositorioMock.Verify(r => r.Actualizar(It.Is<Equipo>(e => e.Id == 1 && e.Nombre == "Uruguay")), Times.Once);
    }

    [TestMethod]
    public void ActualizarEquipoRegistraLogOK()
    {
        Equipo equipo = CrearEquipoValido();
        equipo.Id = 1;
        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns(equipo);
        _repositorioMock.Setup(r => r.ObtenerTodos()).Returns(new List<Equipo> { equipo });

        EquipoDTO equipoDTO = CrearEquipoDTOValido();
        equipoDTO.Id = 1;

        _equipoServicio.Actualizar(equipoDTO, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "EdicionEquipo"
        )), Times.Once);
    }

    [TestMethod]
    public void ActualizarEquipoNombreDuplicadoConOtroIdLanzaExcepcion()
    {
        Equipo existente = CrearEquipoValido("Uruguay");
        existente.Id = 1;
        Equipo otro = CrearEquipoValido("Argentina");
        otro.Id = 2;

        _repositorioMock.Setup(r => r.ObtenerPorId(1)).Returns(existente);
        _repositorioMock.Setup(r => r.ObtenerTodos()).Returns(new List<Equipo> { existente, otro });

        EquipoDTO modificado = CrearEquipoDTOValido("Argentina");
        modificado.Id = 1;

        DominioException ex = Assert.Throws<DominioException>(
            () => _equipoServicio.Actualizar(modificado, "admin@test.com"));
        Assert.AreEqual("Ya existe un equipo con ese nombre.", ex.Message);
    }
}