using Dominio;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositorio.DataAccess;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(FixtureRepositorio))]
public class RepositorioFixtureTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private FixtureRepositorio _repo = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repo = new FixtureRepositorio(_contexto);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    [TestMethod]
    public void GuardarYObtenerFixtureOK()
    {
        Fixture fixture = new Fixture(42);

        _repo.Guardar(fixture);

        Fixture? recuperado = _repo.ObtenerActual();
        Assert.IsNotNull(recuperado);
        Assert.AreEqual(fixture.SemillaUtilizada, recuperado.SemillaUtilizada);
    }

    [TestMethod]
    public void ObtenerActualSinFixtureRetornaNullOK()
    {
        Assert.IsNull(_repo.ObtenerActual());
    }

    [TestMethod]
    public void ExisteSinFixtureRetornaFalseOK()
    {
        Assert.IsFalse(_repo.Existe());
    }

    [TestMethod]
    public void ExisteConFixtureRetornaTrueOK()
    {
        _repo.Guardar(new Fixture(42));

        Assert.IsTrue(_repo.Existe());
    }
}
