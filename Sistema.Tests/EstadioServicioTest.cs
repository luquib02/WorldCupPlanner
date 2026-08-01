using Dominio;
using Dominio.Excepciones;
using Moq;
using Repositorio;
using Sistema;
using Sistema.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Sistema.Tests;

[TestClass]
public class EstadioServicioTest
{
    private Mock<IRepositorioEstadio> _estadioRepoMock;
    private Mock<IRepositorioLog> _logRepoMock;
    private EstadioServicio _servicio;

    [TestInitialize]
    public void Setup()
    {
        _estadioRepoMock = new Mock<IRepositorioEstadio>();
        _logRepoMock = new Mock<IRepositorioLog>();
        LogServicio logServicio = new LogServicio(_logRepoMock.Object);
        _servicio = new EstadioServicio(_estadioRepoMock.Object, logServicio);
    }

    private Estadio CrearEstadio(string nombre = "Estadio Centenario")
    {
        return new Estadio
        {
            Nombre = nombre,
            Ciudad = "Montevideo",
            Capacidad = 60000
        };
    }

    private EstadioDTO CrearEstadioDTO(string nombre = "Estadio Centenario")
    {
        return new EstadioDTO
        {
            Nombre = nombre,
            Ciudad = "Montevideo",
            Capacidad = 60000
        };
    }

    [TestMethod]
    public void CrearEstadioValidoOK()
    {
        EstadioDTO estadioDTO = CrearEstadioDTO();
        _estadioRepoMock.Setup(r => r.BuscarPorNombre("Estadio Centenario"))
            .Returns((Estadio)null!);

        _servicio.Crear(estadioDTO, "admin@test.com");

        _estadioRepoMock.Verify(r => r.Agregar(It.Is<Estadio>(e => e.Nombre == "Estadio Centenario")), Times.Once);
    }

    [TestMethod]
    public void CrearEstadioRegistraLogOK()
    {
        EstadioDTO estadioDTO = CrearEstadioDTO();
        _estadioRepoMock.Setup(r => r.BuscarPorNombre("Estadio Centenario"))
            .Returns((Estadio)null!);

        _servicio.Crear(estadioDTO, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "AltaEstadio" && l.UsuarioEmail == "admin@test.com"
        )), Times.Once);
    }

    [TestMethod]
    public void CrearEstadioNombreDuplicadoLanzaExcepcion()
    {
        Estadio existente = CrearEstadio("Maracana");
        _estadioRepoMock.Setup(r => r.BuscarPorNombre("Maracana"))
            .Returns(existente);

        EstadioDTO nuevo = CrearEstadioDTO("Maracana");

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Crear(nuevo, "admin@test.com"));
        Assert.AreEqual("Ya existe un estadio con el nombre 'Maracana'.", ex.Message);
    }

    [TestMethod]
    public void CrearEstadioNombreDuplicadoNoPersisteOK()
    {
        _estadioRepoMock.Setup(r => r.BuscarPorNombre("Maracana"))
            .Returns(CrearEstadio("Maracana"));

        try { _servicio.Crear(CrearEstadioDTO("Maracana"), "admin@test.com"); }
        catch (DominioException) { }

        _estadioRepoMock.Verify(r => r.Agregar(It.IsAny<Estadio>()), Times.Never);
    }

    [TestMethod]
    public void ModificarEstadioValidoOK()
    {
        Estadio existente = CrearEstadio("Centenario");
        existente.Id = 1;
        _estadioRepoMock.Setup(r => r.BuscarPorId(1)).Returns(existente);
        _estadioRepoMock.Setup(r => r.BuscarPorNombre("Centenario Renovado"))
            .Returns((Estadio)null!);

        EstadioDTO modificado = CrearEstadioDTO("Centenario Renovado");
        modificado.Id = 1;

        _servicio.Modificar(modificado, "admin@test.com");

        _estadioRepoMock.Verify(r => r.Modificar(It.Is<Estadio>(e => e.Id == 1 && e.Nombre == "Centenario Renovado")), Times.Once);
    }

    [TestMethod]
    public void ModificarEstadioRegistraLogOK()
    {
        Estadio existente = CrearEstadio("Centenario");
        existente.Id = 1;
        _estadioRepoMock.Setup(r => r.BuscarPorId(1)).Returns(existente);
        _estadioRepoMock.Setup(r => r.BuscarPorNombre("Centenario"))
            .Returns(existente);

        EstadioDTO modificado = CrearEstadioDTO("Centenario");
        modificado.Id = 1;

        _servicio.Modificar(modificado, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "EdicionEstadio"
        )), Times.Once);
    }

    [TestMethod]
    public void ModificarEstadioInexistenteLanzaExcepcion()
    {
        _estadioRepoMock.Setup(r => r.BuscarPorId(99)).Returns((Estadio)null!);

        EstadioDTO modificado = CrearEstadioDTO();
        modificado.Id = 99;

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Modificar(modificado, "admin@test.com"));
        Assert.AreEqual("El estadio no existe.", ex.Message);
    }

    [TestMethod]
    public void ModificarEstadioNombreDuplicadoConOtroIdLanzaExcepcion()
    {
        Estadio existente = CrearEstadio("Centenario");
        existente.Id = 1;
        Estadio otro = CrearEstadio("Maracana");
        otro.Id = 2;

        _estadioRepoMock.Setup(r => r.BuscarPorId(1)).Returns(existente);
        _estadioRepoMock.Setup(r => r.BuscarPorNombre("Maracana")).Returns(otro);

        EstadioDTO modificado = CrearEstadioDTO("Maracana");
        modificado.Id = 1;

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Modificar(modificado, "admin@test.com"));
        Assert.AreEqual("Ya existe un estadio con el nombre 'Maracana'.", ex.Message);
    }

    [TestMethod]
    public void EliminarEstadioExistenteOK()
    {
        Estadio existente = CrearEstadio();
        existente.Id = 1;
        _estadioRepoMock.Setup(r => r.BuscarPorId(1)).Returns(existente);

        _servicio.Eliminar(1, "admin@test.com");

        _estadioRepoMock.Verify(r => r.Eliminar(1), Times.Once);
    }

    [TestMethod]
    public void EliminarEstadioRegistraLogOK()
    {
        Estadio existente = CrearEstadio();
        existente.Id = 1;
        _estadioRepoMock.Setup(r => r.BuscarPorId(1)).Returns(existente);

        _servicio.Eliminar(1, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "EliminacionEstadio"
        )), Times.Once);
    }

    [TestMethod]
    public void EliminarEstadioInexistenteLanzaExcepcion()
    {
        _estadioRepoMock.Setup(r => r.BuscarPorId(99)).Returns((Estadio)null!);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Eliminar(99, "admin@test.com"));
        Assert.AreEqual("El estadio no existe.", ex.Message);
    }

    [TestMethod]
    public void ObtenerPorIdExistenteRetornaEstadioOK()
    {
        Estadio existente = CrearEstadio();
        existente.Id = 1;
        _estadioRepoMock.Setup(r => r.BuscarPorId(1)).Returns(existente);

        EstadioDTO resultado = _servicio.ObtenerPorId(1);

        Assert.AreEqual(existente.Id, resultado.Id);
        Assert.AreEqual(existente.Nombre, resultado.Nombre);
        Assert.AreEqual(existente.Ciudad, resultado.Ciudad);
        Assert.AreEqual(existente.Capacidad, resultado.Capacidad);
    }

    [TestMethod]
    public void ObtenerPorIdInexistenteLanzaExcepcion()
    {
        _estadioRepoMock.Setup(r => r.BuscarPorId(99)).Returns((Estadio)null!);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.ObtenerPorId(99));
        Assert.AreEqual("El estadio no existe.", ex.Message);
    }

    [TestMethod]
    public void ListarTodosRetornaListaOK()
    {
        List<Estadio> lista = new List<Estadio> { CrearEstadio("A"), CrearEstadio("B") };
        _estadioRepoMock.Setup(r => r.ListarTodos()).Returns(lista);

        List<EstadioDTO> resultado = _servicio.ListarTodos();

        Assert.AreEqual(2, resultado.Count);
    }
}
