using System;
using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class JornadaMapperTest
{
    [TestMethod]
    public void AJornadaDTOMapeaNumeroFechaYPartidosOK()
    {
        Estadio estadio = new Estadio { Nombre = "E", Ciudad = "C", Capacidad = 20000 };
        Equipo local = new Equipo("Argentina", Confederacion.CONMEBOL, 1500);
        Equipo visitante = new Equipo("Brasil", Confederacion.CONMEBOL, 1500);
        Jornada jornada = new Jornada(1, new DateTime(2026, 6, 1));
        Partido partido = new Partido
        {
            Fecha = new DateTime(2026, 6, 1),
            Estadio = estadio,
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = "A",
            Fase = FasePartido.FaseDeGrupos
        };
        jornada.AgregarPartido(partido);

        JornadaDTO resultado = JornadaMapper.AJornadaDTO(jornada);

        Assert.AreEqual(1, resultado.Numero);
        Assert.AreEqual(new DateTime(2026, 6, 1), resultado.Fecha);
        Assert.AreEqual(1, resultado.Partidos.Count);
        Assert.AreEqual("Argentina", resultado.Partidos[0].EquipoLocal!.Nombre);
    }

    [TestMethod]
    public void AJornadaMapeaNumeroFechaYPartidosOK()
    {
        EquipoDTO local = new EquipoDTO { Nombre = "Argentina", Confederacion = Confederacion.CONMEBOL, RankingFIFA = 1500, RankingActual = 1500 };
        EquipoDTO visitante = new EquipoDTO { Nombre = "Brasil", Confederacion = Confederacion.CONMEBOL, RankingFIFA = 1500, RankingActual = 1500 };
        EstadioDTO estadio = new EstadioDTO { Nombre = "E", Ciudad = "C", Capacidad = 20000 };
        PartidoDTO partidoDTO = new PartidoDTO
        {
            Fecha = new DateTime(2026, 6, 1),
            Fase = FasePartido.FaseDeGrupos,
            Grupo = "A",
            Estadio = estadio,
            EquipoLocal = local,
            EquipoVisitante = visitante
        };
        JornadaDTO jornadaDTO = new JornadaDTO
        {
            Numero = 1,
            Fecha = new DateTime(2026, 6, 1),
            Partidos = new System.Collections.Generic.List<PartidoDTO> { partidoDTO }
        };

        Jornada resultado = JornadaMapper.AJornada(jornadaDTO);

        Assert.AreEqual(1, resultado.Numero);
        Assert.AreEqual(new DateTime(2026, 6, 1), resultado.Fecha);
        Assert.AreEqual(1, resultado.Partidos.Count);
        Assert.AreEqual("Argentina", resultado.Partidos[0].EquipoLocal!.Nombre);
    }
}
