using Dominio;
using Dominio.Excepciones;
using Moq;
using Repositorio;
using Sistema;
using Sistema.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Sistema.Tests;

[TestClass]
public class LogServicioTest
{
    private Mock<IRepositorioLog> _logRepoMock;
    private LogServicio _servicio;

    [TestInitialize]
    public void Setup()
    {
        _logRepoMock = new Mock<IRepositorioLog>();
        _servicio = new LogServicio(_logRepoMock.Object);
    }

    [TestMethod]
    public void RegistrarLogValidoPersisteOK()
    {
        _servicio.Registrar("admin@test.com", "AltaEquipo", "Se creo Argentina.");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.UsuarioEmail == "admin@test.com"
                 && l.Accion == "AltaEquipo"
                 && l.Detalle == "Se creo Argentina."
        )), Times.Once);
    }

    [TestMethod]
    public void RegistrarLogAsignaTimestampAutomaticoOK()
    {
        DateTime antes = DateTime.Now;

        _servicio.Registrar("admin@test.com", "AltaEstadio", "Se creo Centenario.");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Timestamp >= antes && l.Timestamp <= DateTime.Now
        )), Times.Once);
    }

    [TestMethod]
    public void RegistrarLogConEmailInvalidoLanzaExcepcionDeDominioOK()
    {
        Assert.Throws<DominioException>(
            () => _servicio.Registrar("", "AltaEquipo", "Detalle"));
    }

    [TestMethod]
    public void RegistrarLogConAccionVaciaLanzaExcepcionDeDominioOK()
    {
        Assert.Throws<DominioException>(
            () => _servicio.Registrar("admin@test.com", "", "Detalle"));
    }

    [TestMethod]
    public void RegistrarLogConDetalleVacioLanzaExcepcionDeDominioOK()
    {
        Assert.Throws<DominioException>(
            () => _servicio.Registrar("admin@test.com", "Accion", ""));
    }

    [TestMethod]
    public void ListarLogsRetornaListaDelRepoOK()
    {
        List<LogEntry> logsEsperados = new List<LogEntry>
        {
            new LogEntry("admin@test.com", "AltaEquipo", "Detalle 1"),
            new LogEntry("admin@test.com", "AltaEstadio", "Detalle 2")
        };
        _logRepoMock.Setup(r => r.ListarTodos()).Returns(logsEsperados);

        List<LogEntryDTO> resultado = _servicio.ListarLogs();

        Assert.AreEqual(2, resultado.Count);
    }

    [TestMethod]
    public void ListarLogsVacioRetornaListaVaciaOK()
    {
        _logRepoMock.Setup(r => r.ListarTodos()).Returns(new List<LogEntry>());

        List<LogEntryDTO> resultado = _servicio.ListarLogs();

        Assert.AreEqual(0, resultado.Count);
    }
}