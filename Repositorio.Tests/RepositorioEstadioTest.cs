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
[TestSubject(typeof(RepositorioEstadio))]
public class RepositorioEstadioTest
{
    private const string CiudadPorDefecto = "Montevideo";
    private const int CapacidadPorDefecto = 60000;

    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private RepositorioEstadio _repo = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repo = new RepositorioEstadio(_contexto);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    private static Estadio CrearEstadio(string nombre = "Estadio Centenario")
    {
        return new Estadio
        {
            Nombre = nombre,
            Ciudad = CiudadPorDefecto,
            Capacidad = CapacidadPorDefecto
        };
    }

    [TestMethod]
    public void AgregarEstadioAsignaIdOK()
    {
        Estadio estadio = CrearEstadio();

        _repo.Agregar(estadio);

        Assert.IsTrue(estadio.Id > 0);
    }

    [TestMethod]
    public void AgregarDosEstadiosAsignaIdsDistintosOK()
    {
        Estadio e1 = CrearEstadio("A");
        Estadio e2 = CrearEstadio("B");

        _repo.Agregar(e1);
        _repo.Agregar(e2);

        Assert.AreNotEqual(e1.Id, e2.Id);
    }

    [TestMethod]
    public void BuscarPorIdExistenteRetornaEstadioOK()
    {
        Estadio estadio = CrearEstadio();
        _repo.Agregar(estadio);

        Estadio? encontrado = _repo.BuscarPorId(estadio.Id);

        Assert.IsNotNull(encontrado);
        Assert.AreEqual("Estadio Centenario", encontrado.Nombre);
    }

    [TestMethod]
    public void BuscarPorIdInexistenteRetornaNullOK()
    {
        Estadio? resultado = _repo.BuscarPorId(999);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void BuscarPorNombreExistenteRetornaEstadioOK()
    {
        _repo.Agregar(CrearEstadio("Maracana"));

        Estadio? encontrado = _repo.BuscarPorNombre("Maracana");

        Assert.IsNotNull(encontrado);
        Assert.AreEqual("Maracana", encontrado.Nombre);
    }

    [TestMethod]
    public void BuscarPorNombreInexistenteRetornaNullOK()
    {
        Estadio? resultado = _repo.BuscarPorNombre("Wembley");

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ListarTodosRetornaListaOK()
    {
        _repo.Agregar(CrearEstadio("A"));
        _repo.Agregar(CrearEstadio("B"));

        List<Estadio> todos = _repo.ListarTodos();

        Assert.AreEqual(2, todos.Count);
    }

    [TestMethod]
    public void ModificarEstadioExistenteOK()
    {
        Estadio estadio = CrearEstadio("Original");
        _repo.Agregar(estadio);

        estadio.Ciudad = "Buenos Aires";
        _repo.Modificar(estadio);

        Estadio? actualizado = _repo.BuscarPorId(estadio.Id);
        Assert.AreEqual("Buenos Aires", actualizado!.Ciudad);
    }

    [TestMethod]
    public void ModificarEstadioInexistenteLanzaExcepcion()
    {
        Estadio estadio = CrearEstadio();
        estadio.Id = 99;

        Assert.Throws<InvalidOperationException>(
            () => _repo.Modificar(estadio));
    }

    [TestMethod]
    public void EliminarEstadioExistenteOK()
    {
        Estadio estadio = CrearEstadio();
        _repo.Agregar(estadio);

        _repo.Eliminar(estadio.Id);

        Assert.IsNull(_repo.BuscarPorId(estadio.Id));
        Assert.AreEqual(0, _repo.ListarTodos().Count);
    }

    [TestMethod]
    public void EliminarEstadioInexistenteLanzaExcepcion()
    {
        Assert.Throws<InvalidOperationException>(
            () => _repo.Eliminar(99));
    }

    [TestMethod]
    public void AgregarEstadioConFalloDePersistenciaLanzaDbUpdateException()
    {
        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        RepositorioEstadio repoFalla = new RepositorioEstadio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Agregar(CrearEstadio("Maracana")));
    }

    [TestMethod]
    public void ModificarEstadioConFalloDePersistenciaLanzaDbUpdateException()
    {
        Estadio estadio = CrearEstadio();
        _repo.Agregar(estadio);

        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        RepositorioEstadio repoFalla = new RepositorioEstadio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Modificar(estadio));
    }

    [TestMethod]
    public void EliminarEstadioConFalloDePersistenciaLanzaDbUpdateException()
    {
        Estadio estadio = CrearEstadio();
        _repo.Agregar(estadio);

        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        RepositorioEstadio repoFalla = new RepositorioEstadio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Eliminar(estadio.Id));
    }
}