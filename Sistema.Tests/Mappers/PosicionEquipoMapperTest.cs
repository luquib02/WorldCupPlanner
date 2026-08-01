using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class PosicionEquipoMapperTest
{
    [TestMethod]
    public void APosicionEquipoDTOMapeaTodosLosCamposOK()
    {
        Equipo equipo = new Equipo("Argentina", Confederacion.CONMEBOL, 1500);
        PosicionEquipo posicion = new PosicionEquipo(equipo);
        posicion.RegistrarResultado(2, 0);

        PosicionEquipoDTO resultado = PosicionEquipoMapper.APosicionEquipoDTO(posicion);

        Assert.AreEqual("Argentina", resultado.Equipo.Nombre);
        Assert.AreEqual(3, resultado.Puntos);
        Assert.AreEqual(1, resultado.PartidosJugados);
        Assert.AreEqual(1, resultado.Ganados);
        Assert.AreEqual(0, resultado.Empatados);
        Assert.AreEqual(0, resultado.Perdidos);
        Assert.AreEqual(2, resultado.GolesAFavor);
        Assert.AreEqual(0, resultado.GolesEnContra);
        Assert.AreEqual(2, resultado.DiferenciaGoles);
    }

    [TestMethod]
    public void APosicionEquipoMapeaElEquipoOK()
    {
        EquipoDTO equipoDTO = new EquipoDTO
        {
            Id = 1,
            Nombre = "Brasil",
            Confederacion = Confederacion.CONMEBOL,
            RankingFIFA = 1700,
            RankingActual = 1700
        };
        PosicionEquipoDTO posicionDTO = new PosicionEquipoDTO { Equipo = equipoDTO };

        PosicionEquipo resultado = PosicionEquipoMapper.APosicionEquipo(posicionDTO);

        Assert.AreEqual("Brasil", resultado.Equipo.Nombre);
        Assert.AreEqual(0, resultado.Puntos);
    }
}
