using System;
using System.Collections.Generic;
using Dominio;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositorio;
using Sistema;
using Sistema.Interfaces;

namespace Sistema.Tests;

[TestClass]
public class ExportacionServicioTest
{
    private Mock<IRepositorioLog> _repositorioLogMock;
    private Mock<IFixtureRepositorio> _fixtureRepositorioMock;
    private ExportacionServicio _exportacionServicio;

    [TestInitialize]
    public void Setup()
    {
        _repositorioLogMock = new Mock<IRepositorioLog>();
        _fixtureRepositorioMock = new Mock<IFixtureRepositorio>();
        Mock<IRepositorioLog> logRepoMock = new Mock<IRepositorioLog>();
        LogServicio logServicio = new LogServicio(logRepoMock.Object);
        _exportacionServicio = new ExportacionServicio(
            _repositorioLogMock.Object,
            _fixtureRepositorioMock.Object,
            logServicio);
    }

    private List<LogEntry> CrearLogs()
    {
        return new List<LogEntry>
        {
            new LogEntry(
                usuarioEmail: "admin@sistema.com",
                accion: "AltaEquipo",
                detalle: "Se creó el equipo Uruguay"),
            new LogEntry(
                usuarioEmail: "editor@sistema.com",
                accion: "ModificacionPartido",
                detalle: "Se modificó el partido 1")
        };
    }

    [TestMethod]
    public void ExportarLogsCsv_ConLogsEnRango_RetornaByteArrayNoVacio()
    {
        DateTime desde = new DateTime(2026, 6, 1);
        DateTime hasta = new DateTime(2026, 6, 3);
        _repositorioLogMock
            .Setup(r => r.ObtenerPorRango(desde, hasta.AddDays(1).AddSeconds(-1)))
            .Returns(CrearLogs());

        byte[] resultado = _exportacionServicio.ExportarLogsCsv(desde, hasta, "test@test.com");

        Assert.IsTrue(resultado.Length > 0);
    }
    [TestMethod]
    public void ExportarLogsCsv_SinLogsEnRango_RetornaSoloEncabezado()
    {
        DateTime desde = new DateTime(2026, 6, 1);
        DateTime hasta = new DateTime(2026, 6, 3);
        _repositorioLogMock
            .Setup(r => r.ObtenerPorRango(desde, hasta.AddDays(1).AddSeconds(-1)))
            .Returns(new List<LogEntry>());

        byte[] resultado = _exportacionServicio.ExportarLogsCsv(desde, hasta, "test@test.com");
        string contenido = System.Text.Encoding.UTF8.GetString(resultado);

        Assert.IsTrue(contenido.Contains("Timestamp,Usuario,Accion,Detalle"));
        Assert.AreEqual(2, contenido.Split('\n').Length);
    }
    [TestMethod]
    public void ExportarLogsXlsx_ConLogsEnRango_RetornaByteArrayNoVacio()
    {
        DateTime desde = new DateTime(2026, 6, 1);
        DateTime hasta = new DateTime(2026, 6, 3);
        _repositorioLogMock
            .Setup(r => r.ObtenerPorRango(desde, hasta.AddDays(1).AddSeconds(-1)))
            .Returns(CrearLogs());

        byte[] resultado = _exportacionServicio.ExportarLogsXlsx(desde, hasta, "test@test.com");

        Assert.IsTrue(resultado.Length > 0);
    }
[TestMethod]
public void ExportarFixtureCsv_SinFixture_LanzaExcepcion()
{
    _fixtureRepositorioMock
        .Setup(r => r.ObtenerActual())
        .Returns((Fixture?)null);

    bool lanzoExcepcion = false;
    try
    {
        _exportacionServicio.ExportarFixtureCsv("test@test.com");
    }
    catch (DominioException)
    {
        lanzoExcepcion = true;
    }
    Assert.IsTrue(lanzoExcepcion);
}
[TestMethod]
public void ExportarFixtureCsv_ConFixture_RetornaByteArrayNoVacio()
{
    Fixture fixture = CrearFixtureConPartido();
    _fixtureRepositorioMock
        .Setup(r => r.ObtenerActual())
        .Returns(fixture);

    byte[] resultado = _exportacionServicio.ExportarFixtureCsv("test@test.com");

    Assert.IsTrue(resultado.Length > 0);
}

private Fixture CrearFixtureConPartido()
{
    Fixture fixture = new Fixture(42);
    Grupo grupo = new Grupo("A");
    Equipo local = new Equipo("Uruguay", Confederacion.CONMEBOL, 1500);
    Equipo visitante = new Equipo("Argentina", Confederacion.CONMEBOL, 1800);
    Estadio estadio = new Estadio
    {
        Nombre = "MetLife",
        Ciudad = "Nueva York",
        Capacidad = 82500
    };
    grupo.AgregarEquipo(local);
    grupo.AgregarEquipo(visitante);
    grupo.AgregarEquipo(new Equipo("Brasil", Confederacion.CONMEBOL, 1700));
    grupo.AgregarEquipo(new Equipo("Chile", Confederacion.CONMEBOL, 1300));

    Jornada jornada = new Jornada(1, new DateTime(2026, 6, 1));
    Partido partido = new Partido
    {
        Fecha = new DateTime(2026, 6, 1, 14, 0, 0),
        Estadio = estadio,
        EquipoLocal = local,
        EquipoVisitante = visitante,
        Grupo = "A",
        Fase = FasePartido.FaseDeGrupos
    };
    jornada.AgregarPartido(partido);
    grupo.AgregarJornada(jornada);
    fixture.AgregarGrupo(grupo);
    return fixture;
}
[TestMethod]
public void ExportarFixtureXlsx_ConFixture_RetornaByteArrayNoVacio()
{
    Fixture fixture = CrearFixtureConPartido();
    _fixtureRepositorioMock
        .Setup(r => r.ObtenerActual())
        .Returns(fixture);

    byte[] resultado = _exportacionServicio.ExportarFixtureXlsx("test@test.com");

    Assert.IsTrue(resultado.Length > 0);
}
}