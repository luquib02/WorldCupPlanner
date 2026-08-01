using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class EquipoMapperTest
{
    [TestMethod]
    public void AEquipoDTOMapeaTodosLosCamposOK()
    {
        Equipo equipo = new Equipo("Uruguay", Confederacion.CONMEBOL, 1500);
        equipo.Id = 7;
        equipo.ActualizarRankingActual(1450);

        EquipoDTO resultado = EquipoMapper.AEquipoDTO(equipo);

        Assert.AreEqual(7, resultado.Id);
        Assert.AreEqual("Uruguay", resultado.Nombre);
        Assert.AreEqual(Confederacion.CONMEBOL, resultado.Confederacion);
        Assert.AreEqual(1500, resultado.RankingFIFA);
        Assert.AreEqual(1450, resultado.RankingActual);
    }

    [TestMethod]
    public void AEquipoMapeaTodosLosCamposOK()
    {
        EquipoDTO equipoDTO = new EquipoDTO
        {
            Id = 7,
            Nombre = "Uruguay",
            Confederacion = Confederacion.CONMEBOL,
            RankingFIFA = 1500,
            RankingActual = 1450
        };

        Equipo resultado = EquipoMapper.AEquipo(equipoDTO);

        Assert.AreEqual(7, resultado.Id);
        Assert.AreEqual("Uruguay", resultado.Nombre);
        Assert.AreEqual(Confederacion.CONMEBOL, resultado.Confederacion);
        Assert.AreEqual(1500, resultado.RankingFIFA);
        Assert.AreEqual(1450, resultado.RankingActual);
    }

    [TestMethod]
    public void AEquipoSinCambioDeRankingActualUsaRankingFIFAOK()
    {
        EquipoDTO equipoDTO = new EquipoDTO
        {
            Id = 1,
            Nombre = "Argentina",
            Confederacion = Confederacion.CONMEBOL,
            RankingFIFA = 1600,
            RankingActual = 1600
        };

        Equipo resultado = EquipoMapper.AEquipo(equipoDTO);

        Assert.AreEqual(1600, resultado.RankingActual);
    }
}
