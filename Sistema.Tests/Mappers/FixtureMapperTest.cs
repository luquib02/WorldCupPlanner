using Dominio;
using Dominio.Simulacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;
using Sistema.Tests.Helpers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class FixtureMapperTest
{
    [TestMethod]
    public void AFixtureDTOMapeaGruposYPartidosEliminatoriosOK()
    {
        Fixture fixture = new FixtureBuilder()
            .ConPartidosEliminatorios(FixtureBuilder.CrearDieciseisavosJugados())
            .Construir();

        FixtureDTO resultado = FixtureMapper.AFixtureDTO(fixture);

        Assert.AreEqual(fixture.Id, resultado.Id);
        Assert.AreEqual(fixture.FechaGeneracion, resultado.FechaGeneracion);
        Assert.AreEqual(fixture.SemillaUtilizada, resultado.SemillaUtilizada);
        Assert.AreEqual(fixture.CrucesGenerados, resultado.CrucesGenerados);
        Assert.AreEqual(fixture.Grupos.Count, resultado.Grupos.Count);
        Assert.AreEqual(fixture.PartidosEliminatorias.Count, resultado.PartidosEliminatorias.Count);
        Assert.AreEqual("A", resultado.Grupos[0].Etiqueta);
    }

    [TestMethod]
    public void AFixtureMapeaGruposYPartidosEliminatoriosOK()
    {
        Fixture original = new FixtureBuilder()
            .ConPartidosEliminatorios(FixtureBuilder.CrearDieciseisavosJugados())
            .Construir();
        FixtureDTO dto = FixtureMapper.AFixtureDTO(original);

        Fixture resultado = FixtureMapper.AFixture(dto);

        Assert.AreEqual(original.SemillaUtilizada, resultado.SemillaUtilizada);
        Assert.AreEqual(original.Grupos.Count, resultado.Grupos.Count);
        Assert.AreEqual(original.PartidosEliminatorias.Count, resultado.PartidosEliminatorias.Count);
        Assert.AreEqual("A", resultado.Grupos[0].Etiqueta);
    }

    [TestMethod]
    public void AFixtureRespetaCrucesGeneradosOK()
    {
        FixtureDTO dto = new FixtureDTO { SemillaUtilizada = 42, CrucesGenerados = true };

        Fixture resultado = FixtureMapper.AFixture(dto);

        Assert.IsTrue(resultado.CrucesGenerados);
    }

    [TestMethod]
    public void AFixtureDTOMapeaNombreMotorSimulacionOK()
    {
        Fixture fixture = new Fixture(42, NombresMotores.AleatorioPuro);

        FixtureDTO resultado = FixtureMapper.AFixtureDTO(fixture);

        Assert.AreEqual(NombresMotores.AleatorioPuro, resultado.NombreMotorSimulacion);
    }

    [TestMethod]
    public void AFixtureMapeaNombreMotorSimulacionOK()
    {
        FixtureDTO dto = new FixtureDTO { SemillaUtilizada = 42, NombreMotorSimulacion = NombresMotores.AleatorioPuro };

        Fixture resultado = FixtureMapper.AFixture(dto);

        Assert.AreEqual(NombresMotores.AleatorioPuro, resultado.NombreMotorSimulacion);
    }
}
