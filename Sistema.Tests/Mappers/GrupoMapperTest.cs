using System;
using System.Collections.Generic;
using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class GrupoMapperTest
{
    [TestMethod]
    public void AGrupoDTOMapeaEtiquetaEquiposYJornadasOK()
    {
        Grupo grupo = new Grupo("A");
        Equipo equipo = new Equipo("Argentina", Confederacion.CONMEBOL, 1500);
        grupo.AgregarEquipo(equipo);
        grupo.AgregarJornada(new Jornada(1, new DateTime(2026, 6, 1)));

        GrupoDTO resultado = GrupoMapper.AGrupoDTO(grupo);

        Assert.AreEqual("A", resultado.Etiqueta);
        Assert.AreEqual(1, resultado.Equipos.Count);
        Assert.AreEqual("Argentina", resultado.Equipos[0].Nombre);
        Assert.AreEqual(1, resultado.Jornadas.Count);
        Assert.AreEqual(1, resultado.Jornadas[0].Numero);
    }

    [TestMethod]
    public void AGrupoMapeaEtiquetaEquiposYJornadasOK()
    {
        EquipoDTO equipoDTO = new EquipoDTO { Nombre = "Brasil", Confederacion = Confederacion.CONMEBOL, RankingFIFA = 1700, RankingActual = 1700 };
        JornadaDTO jornadaDTO = new JornadaDTO { Numero = 1, Fecha = new DateTime(2026, 6, 1), Partidos = new List<PartidoDTO>() };
        GrupoDTO grupoDTO = new GrupoDTO
        {
            Etiqueta = "B",
            Equipos = new List<EquipoDTO> { equipoDTO },
            Jornadas = new List<JornadaDTO> { jornadaDTO }
        };

        Grupo resultado = GrupoMapper.AGrupo(grupoDTO);

        Assert.AreEqual("B", resultado.Etiqueta);
        Assert.AreEqual(1, resultado.Equipos.Count);
        Assert.AreEqual("Brasil", resultado.Equipos[0].Nombre);
        Assert.AreEqual(1, resultado.Jornadas.Count);
        Assert.AreEqual(1, resultado.Jornadas[0].Numero);
    }
}
