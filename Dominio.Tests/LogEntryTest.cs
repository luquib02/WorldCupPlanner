using System;
using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof(LogEntry))]
public class LogEntryTest
{
    private class LogEntryDePrueba : LogEntry { }


    private static LogEntry CrearLogEntryValido(
        string usuarioEmail = "admin@worldcup.com",
        string accion = "AltaEquipo",
        string detalle = "Se creo el equipo Argentina.")
    {
        return new LogEntry(usuarioEmail, accion, detalle);
    }

    [TestMethod]
    public void CrearLogEntryDatosValidosOK()
    {
        LogEntry log = CrearLogEntryValido();

        Assert.AreEqual("admin@worldcup.com", log.UsuarioEmail);
        Assert.AreEqual("AltaEquipo", log.Accion);
        Assert.AreEqual("Se creo el equipo Argentina.", log.Detalle);
    }

    [TestMethod]
    public void CrearLogEntryTimestampSeCargaAutomaticamenteOK()
    {
        DateTime antes = DateTime.Now;

        LogEntry log = CrearLogEntryValido();

        DateTime despues = DateTime.Now;
        Assert.IsTrue(log.Timestamp >= antes && log.Timestamp <= despues);
    }

    [TestMethod]
    public void CrearLogEntryEmailVacioLanzaExcepcion()
    {
        Assert.Throws<DominioException>(
            () => CrearLogEntryValido(usuarioEmail: ""));
    }

    [TestMethod]
    public void CrearLogEntryEmailNuloLanzaExcepcion()
    {
        Assert.Throws<DominioException>(
            () => CrearLogEntryValido(usuarioEmail: null!));
    }

    [TestMethod]
    public void CrearLogEntryEmailSinArrobaLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearLogEntryValido(usuarioEmail: "adminworldcup.com"));

        Assert.AreEqual("El email tiene un formato invalido.", ex.Message);
    }

    [TestMethod]
    public void CrearLogEntryAccionVaciaLanzaExcepcion()
    {
        Assert.Throws<DominioException>(
            () => CrearLogEntryValido(accion: ""));
    }

    [TestMethod]
    public void CrearLogEntryAccionNulaLanzaExcepcion()
    {
        Assert.Throws<DominioException>(
            () => CrearLogEntryValido(accion: null!));
    }

    [TestMethod]
    public void CrearLogEntryAccionMasDe100CharsLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearLogEntryValido(accion: new string('A', 101)));

        Assert.AreEqual("La accion no puede superar los 100 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearLogEntryDetalleVacioLanzaExcepcion()
    {
        Assert.Throws<DominioException>(
            () => CrearLogEntryValido(detalle: ""));
    }

    [TestMethod]
    public void CrearLogEntryDetalleMasDe500CharsLanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => CrearLogEntryValido(detalle: new string('A', 501)));

        Assert.AreEqual("El detalle no puede superar los 500 caracteres.", ex.Message);
    }

    [TestMethod]
    public void CrearLogEntryIdEsCeroAntesDePersistrirOK()
    {
        LogEntry log = CrearLogEntryValido();

        Assert.AreEqual(0, log.Id);
    }

    [TestMethod]
    public void ConstructorProtegido_CreaInstanciaConValoresPorDefectoOK()
    {
        LogEntryDePrueba log = new LogEntryDePrueba();

        Assert.AreEqual(string.Empty, log.UsuarioEmail);
        Assert.AreEqual(string.Empty, log.Accion);
        Assert.AreEqual(string.Empty, log.Detalle);
    }
}