using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class LogEntryMapperTest
{
    [TestMethod]
    public void ALogEntryDTOMapeaTodosLosCamposOK()
    {
        LogEntry logEntry = new LogEntry("admin@test.com", "AltaEquipo", "Se creo Argentina.");
        logEntry.Id = 7;

        LogEntryDTO resultado = LogEntryMapper.ALogEntryDTO(logEntry);

        Assert.AreEqual(7, resultado.Id);
        Assert.AreEqual(logEntry.Timestamp, resultado.Timestamp);
        Assert.AreEqual("admin@test.com", resultado.UsuarioEmail);
        Assert.AreEqual("AltaEquipo", resultado.Accion);
        Assert.AreEqual("Se creo Argentina.", resultado.Detalle);
    }

    [TestMethod]
    public void ALogEntryMapeaTodosLosCamposOK()
    {
        LogEntryDTO dto = new LogEntryDTO
        {
            Id = 7,
            Timestamp = new System.DateTime(2026, 6, 12),
            UsuarioEmail = "admin@test.com",
            Accion = "AltaEquipo",
            Detalle = "Se creo Argentina."
        };

        LogEntry resultado = LogEntryMapper.ALogEntry(dto);

        Assert.AreEqual(7, resultado.Id);
        Assert.AreEqual(new System.DateTime(2026, 6, 12), resultado.Timestamp);
        Assert.AreEqual("admin@test.com", resultado.UsuarioEmail);
        Assert.AreEqual("AltaEquipo", resultado.Accion);
        Assert.AreEqual("Se creo Argentina.", resultado.Detalle);
    }
}
