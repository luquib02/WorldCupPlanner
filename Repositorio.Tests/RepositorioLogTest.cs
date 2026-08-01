using Dominio;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositorio.DataAccess;
using Repositorio.Tests.Helpers;
using System;
using System.Collections.Generic;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(RepositorioLog))]
public class RepositorioLogTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private RepositorioLog _repo = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repo = new RepositorioLog(_contexto);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    private static LogEntry CrearLogEntry(string accion = "AltaEquipo")
        => new LogEntry("admin@test.com", accion, "Se realizó la acción.");

    [TestMethod]
    public void AgregarLogPersisteOK()
    {
        _repo.Agregar(CrearLogEntry());

        Assert.AreEqual(1, _repo.ListarTodos().Count);
    }

    [TestMethod]
    public void AgregarDosLogsPersisteOK()
    {
        _repo.Agregar(CrearLogEntry("AltaEquipo"));
        _repo.Agregar(CrearLogEntry("AltaEstadio"));

        Assert.AreEqual(2, _repo.ListarTodos().Count);
    }

    [TestMethod]
    public void ListarTodosVacioRetornaListaVaciaOK()
    {
        Assert.AreEqual(0, _repo.ListarTodos().Count);
    }

    [TestMethod]
    public void ListarTodosRetornaCopiaNoReferenciaOK()
    {
        _repo.Agregar(CrearLogEntry());

        List<LogEntry> lista1 = _repo.ListarTodos();
        List<LogEntry> lista2 = _repo.ListarTodos();

        Assert.AreNotSame(lista1, lista2);
    }

    [TestMethod]
    public void AgregarLogAsignaIdOK()
    {
        LogEntry log = CrearLogEntry();

        _repo.Agregar(log);

        Assert.IsTrue(log.Id > 0);
    }

    [TestMethod]
    public void ObtenerPorRangoIncluyeSoloLogsDentroDelRangoOK()
    {
        LogEntry dentroDeRango = CrearLogEntry();
        LogEntry fueraDeRango = CrearLogEntry();
        _repo.Agregar(dentroDeRango);
        _repo.Agregar(fueraDeRango);
        DateTime desde = dentroDeRango.Timestamp.AddMinutes(-1);
        DateTime hasta = dentroDeRango.Timestamp.AddMinutes(1);
        fueraDeRango.Timestamp = hasta.AddDays(1);
        _contexto.SaveChanges();

        List<LogEntry> resultado = _repo.ObtenerPorRango(desde, hasta);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual(dentroDeRango.Id, resultado[0].Id);
    }

    [TestMethod]
    public void ObtenerPorRangoSinLogsEnRangoRetornaListaVaciaOK()
    {
        LogEntry log = CrearLogEntry();
        _repo.Agregar(log);

        List<LogEntry> resultado = _repo.ObtenerPorRango(
            log.Timestamp.AddDays(-2), log.Timestamp.AddDays(-1));

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void AgregarLogConFalloDePersistenciaLanzaDbUpdateException()
    {
        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        RepositorioLog repoFalla = new RepositorioLog(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Agregar(CrearLogEntry()));
    }
}