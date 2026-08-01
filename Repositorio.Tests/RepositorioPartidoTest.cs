using Dominio;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositorio.DataAccess;
using Repositorio.Tests.Helpers;
using System;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(RepositorioPartido))]
public class RepositorioPartidoTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private RepositorioPartido _repo = null!;
    private RepositorioEstadio _repoEstadio = null!;
    private EquipoRepositorio _repoEquipo = null!;
    private const string GrupoPorDefecto = "A";
    private const FasePartido FasePorDefecto = FasePartido.FaseDeGrupos;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repo = new RepositorioPartido(_contexto);
        _repoEstadio = new RepositorioEstadio(_contexto);
        _repoEquipo = new EquipoRepositorio(_contexto);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    private Estadio CrearYPersistirEstadio(string nombre = "Centenario")
    {
        Estadio estadio = new Estadio
        {
            Nombre = nombre,
            Ciudad = "Montevideo",
            Capacidad = 60000
        };
        _repoEstadio.Agregar(estadio);
        return estadio;
    }

    private Equipo CrearYPersistirEquipo(string nombre)
    {
        Equipo equipo = new Equipo(nombre, Confederacion.CONMEBOL, 1500);
        _repoEquipo.Agregar(equipo);
        return equipo;
    }

    private Partido CrearPartidoValido(string grupo = GrupoPorDefecto,
        FasePartido fase = FasePorDefecto)
    {
        return new Partido
        {
            Fecha = new DateTime(2026, 6, 1, 14, 0, 0),
            Estadio = CrearYPersistirEstadio(),
            EquipoLocal = CrearYPersistirEquipo("Uruguay"),
            EquipoVisitante = CrearYPersistirEquipo("Argentina"),
            Grupo = grupo,
            Fase = fase
        };
    }

    [TestMethod]
    public void AgregarPartidoAsignaIdOK()
    {
        Partido partido = CrearPartidoValido();

        _repo.Agregar(partido);

        Assert.IsTrue(partido.Id > 0);
    }

    [TestMethod]
    public void AgregarDosPartidosAsignaIdsDistintosOK()
    {
        Partido p1 = CrearPartidoValido();
        Partido p2 = CrearPartidoValido();

        _repo.Agregar(p1);
        _repo.Agregar(p2);

        Assert.AreNotEqual(p1.Id, p2.Id);
    }

    [TestMethod]
    public void ObtenerPorIdExistenteRetornaPartidoOK()
    {
        Partido partido = CrearPartidoValido();
        _repo.Agregar(partido);

        Partido? encontrado = _repo.ObtenerPorId(partido.Id);

        Assert.IsNotNull(encontrado);
        Assert.AreEqual("A", encontrado.Grupo);
    }

    [TestMethod]
    public void ObtenerPorIdInexistenteRetornaNullOK()
    {
        Assert.IsNull(_repo.ObtenerPorId(999));
    }

    [TestMethod]
    public void ObtenerTodosRetornaListaOK()
    {
        _repo.Agregar(CrearPartidoValido());
        _repo.Agregar(CrearPartidoValido());

        Assert.AreEqual(2, _repo.ObtenerTodos().Count);
    }

    [TestMethod]
    public void ActualizarPartidoExistenteOK()
    {
        Partido partido = CrearPartidoValido();
        _repo.Agregar(partido);

        partido.CargarResultado(2, 1);
        _repo.Actualizar(partido);

        Partido? actualizado = _repo.ObtenerPorId(partido.Id);
        Assert.AreEqual(2, actualizado!.GolesLocal);
        Assert.AreEqual(1, actualizado.GolesVisitante);
    }

    [TestMethod]
    public void ActualizarPartidoInexistenteLanzaExcepcion()
    {
        Partido partido = CrearPartidoValido();
        partido.Id = 99;

        Assert.Throws<InvalidOperationException>(
            () => _repo.Actualizar(partido));
    }

    [TestMethod]
    public void EliminarPartidoExistenteOK()
    {
        Partido partido = CrearPartidoValido();
        _repo.Agregar(partido);

        _repo.Eliminar(partido.Id);

        Assert.IsNull(_repo.ObtenerPorId(partido.Id));
        Assert.AreEqual(0, _repo.ObtenerTodos().Count);
    }

    [TestMethod]
    public void EliminarPartidoInexistenteLanzaExcepcion()
    {
        Assert.Throws<InvalidOperationException>(
            () => _repo.Eliminar(99));
    }

    [TestMethod]
    public void ObtenerPorGrupoOK()
    {
        _repo.Agregar(CrearPartidoValido(grupo: "A"));
        _repo.Agregar(CrearPartidoValido(grupo: "B"));
        _repo.Agregar(CrearPartidoValido(grupo: "A"));

        Assert.AreEqual(2, _repo.ObtenerPorGrupo("A").Count);
        Assert.AreEqual(1, _repo.ObtenerPorGrupo("B").Count);
    }

    [TestMethod]
    public void ObtenerPorFaseOK()
    {
        _repo.Agregar(CrearPartidoValido(fase: FasePartido.FaseDeGrupos));
        _repo.Agregar(CrearPartidoValido(fase: FasePartido.OctavosDeFinal));
        _repo.Agregar(CrearPartidoValido(fase: FasePartido.FaseDeGrupos));

        Assert.AreEqual(2, _repo.ObtenerPorFase(FasePartido.FaseDeGrupos).Count);
        Assert.AreEqual(1, _repo.ObtenerPorFase(FasePartido.OctavosDeFinal).Count);
    }

    [TestMethod]
    public void AgregarPartidoConIncidencia_PersisteTarjetaOK()
    {
        Partido partido = CrearPartidoValido();
        partido.AgregarIncidencia(new TarjetaAmarilla(30, esLocal: true));
        _repo.Agregar(partido);

        Partido? encontrado = _repo.ObtenerPorId(partido.Id);

        Assert.IsNotNull(encontrado);
        Assert.AreEqual(1, encontrado.Incidencias.Count,
            "Debe persistir la tarjeta amarilla");
    }

    [TestMethod]
    public void ObtenerPorId_PartidoConVariasIncidencias_CargaTodasOK()
    {
        Partido partido = CrearPartidoValido();
        partido.AgregarIncidencia(new TarjetaAmarilla(15, esLocal: true));
        partido.AgregarIncidencia(new TarjetaRoja(70, esLocal: false));
        _repo.Agregar(partido);

        Partido? encontrado = _repo.ObtenerPorId(partido.Id);

        Assert.AreEqual(2, encontrado!.Incidencias.Count,
            "Deben cargarse ambas incidencias desde la base de datos");
        Assert.AreEqual(1, encontrado.IncidenciasLocal.Count,
            "Una incidencia local");
        Assert.AreEqual(1, encontrado.IncidenciasVisitante.Count,
            "Una incidencia visitante");
    }

    [TestMethod]
    public void AgregarPartidoConFalloDePersistenciaLanzaDbUpdateException()
    {
        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        RepositorioPartido repoFalla = new RepositorioPartido(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Agregar(CrearPartidoValido()));
    }

    [TestMethod]
    public void ActualizarPartidoConFalloDePersistenciaLanzaDbUpdateException()
    {
        Partido partido = CrearPartidoValido();
        _repo.Agregar(partido);

        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        RepositorioPartido repoFalla = new RepositorioPartido(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Actualizar(partido));
    }

    [TestMethod]
    public void EliminarPartidoConFalloDePersistenciaLanzaDbUpdateException()
    {
        Partido partido = CrearPartidoValido();
        _repo.Agregar(partido);

        using WorldCupPlannerDbContextFalla contextoFalla = WorldCupPlannerDbContextFalla.Crear();
        RepositorioPartido repoFalla = new RepositorioPartido(contextoFalla);

        Assert.Throws<DbUpdateException>(
            () => repoFalla.Eliminar(partido.Id));
    }
}