using System;
using Dominio;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositorio.DataAccess;
using Repositorio.Tests.Helpers;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(EquipoRepositorio))]
public class RepositorioEquipoTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private EquipoRepositorio _repo = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repo = new EquipoRepositorio(_contexto);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }
    private const int RankingPorDefecto = 1500;
    private static Equipo CrearEquipo(string nombre = "Uruguay",
        Confederacion conf = Confederacion.CONMEBOL)
        => new Equipo(nombre, conf, RankingPorDefecto);

    [TestMethod]
    public void AgregarEquipoAsignaIdOK()
    {
        Equipo equipo = CrearEquipo();

        _repo.Agregar(equipo);

        Assert.IsTrue(equipo.Id > 0);
    }

    [TestMethod]
    public void AgregarDosEquiposAsignaIdsDistintosOK()
    {
        Equipo e1 = CrearEquipo("Uruguay");
        Equipo e2 = CrearEquipo("Argentina");

        _repo.Agregar(e1);
        _repo.Agregar(e2);

        Assert.AreNotEqual(e1.Id, e2.Id);
    }

    [TestMethod]
    public void ObtenerPorIdExistenteRetornaEquipoOK()
    {
        Equipo equipo = CrearEquipo();
        _repo.Agregar(equipo);

        Equipo? encontrado = _repo.ObtenerPorId(equipo.Id);

        Assert.IsNotNull(encontrado);
        Assert.AreEqual("Uruguay", encontrado.Nombre);
    }

    [TestMethod]
    public void ObtenerPorIdInexistenteRetornaNullOK()
    {
        Assert.IsNull(_repo.ObtenerPorId(999));
    }

    [TestMethod]
    public void ObtenerTodosRetornaListaOK()
    {
        _repo.Agregar(CrearEquipo("Uruguay"));
        _repo.Agregar(CrearEquipo("Argentina"));

        Assert.AreEqual(2, _repo.ObtenerTodos().Count);
    }

    [TestMethod]
    public void ActualizarEquipoExistentePersisteCambiosOK()
    {
        Equipo equipo = CrearEquipo("Uruguay");
        _repo.Agregar(equipo);
        equipo.ActualizarRankingActual(1600);

        _repo.Actualizar(equipo);

        Equipo? actualizado = _repo.ObtenerPorId(equipo.Id);
        Assert.IsNotNull(actualizado);
        Assert.AreEqual(1600, actualizado.RankingActual);
    }

    [TestMethod]
    public void ActualizarEquipoInexistenteLanzaExcepcion()
    {
        Equipo equipo = CrearEquipo();
        equipo.Id = 99;

        Assert.Throws<InvalidOperationException>(
            () => _repo.Actualizar(equipo));
    }

    [TestMethod]
    public void EliminarEquipoExistenteOK()
    {
        Equipo equipo = CrearEquipo();
        _repo.Agregar(equipo);

        _repo.Eliminar(equipo.Id);

        Assert.IsNull(_repo.ObtenerPorId(equipo.Id));
        Assert.AreEqual(0, _repo.ObtenerTodos().Count);
    }

    [TestMethod]
    public void EliminarEquipoInexistenteLanzaExcepcion()
    {
        Assert.Throws<InvalidOperationException>(
            () => _repo.Eliminar(99));
    }

    [TestMethod]
    public void AgregarEquipoConFalloDePersistenciaLanzaDbUpdateException()
    {
        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        EquipoRepositorio repoFalla = new EquipoRepositorio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Agregar(CrearEquipo("Brasil")));
    }

    [TestMethod]
    public void ActualizarEquipoConFalloDePersistenciaLanzaDbUpdateException()
    {
        Equipo equipo = CrearEquipo("Uruguay");
        _repo.Agregar(equipo);

        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        EquipoRepositorio repoFalla = new EquipoRepositorio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Actualizar(equipo));
    }

    [TestMethod]
    public void EliminarEquipoConFalloDePersistenciaLanzaDbUpdateException()
    {
        Equipo equipo = CrearEquipo("Uruguay");
        _repo.Agregar(equipo);

        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        EquipoRepositorio repoFalla = new EquipoRepositorio(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Eliminar(equipo.Id));
    }

    [TestMethod]
    public void ExisteNombreDevuelveTrueOK()
    {
        _repo.Agregar(CrearEquipo("Uruguay"));

        Assert.IsTrue(_repo.ExisteNombre("Uruguay"));
    }

    [TestMethod]
    public void ExisteNombreDevuelveFalseOK()
    {
        Assert.IsFalse(_repo.ExisteNombre("Uruguay"));
    }

    [TestMethod]
    public void ContarPorConfederacionOK()
    {
        _repo.Agregar(CrearEquipo("Uruguay", Confederacion.CONMEBOL));
        _repo.Agregar(CrearEquipo("Argentina", Confederacion.CONMEBOL));
        _repo.Agregar(CrearEquipo("Espana", Confederacion.UEFA));

        Assert.AreEqual(2, _repo.ContarPorConfederacion(Confederacion.CONMEBOL));
        Assert.AreEqual(1, _repo.ContarPorConfederacion(Confederacion.UEFA));
    }
}