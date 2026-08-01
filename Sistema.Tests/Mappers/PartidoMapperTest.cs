using System;
using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class PartidoMapperTest
{
    private Equipo CrearEquipo(string nombre) =>
        new Equipo(nombre, Confederacion.CONMEBOL, 1500);

    private Estadio CrearEstadio() =>
        new Estadio { Id = 5, Nombre = "Centenario", Ciudad = "Montevideo", Capacidad = 60000 };

    [TestMethod]
    public void APartidoDTOMapeaTodosLosCamposOK()
    {
        Partido partido = new Partido
        {
            Id = 10,
            Fecha = new DateTime(2026, 6, 12),
            Fase = FasePartido.FaseDeGrupos,
            EtiquetaCruce = "C1",
            Estadio = CrearEstadio(),
            EquipoLocal = CrearEquipo("Uruguay"),
            EquipoVisitante = CrearEquipo("Brasil"),
            Grupo = "A"
        };
        partido.CargarResultado(2, 1);

        PartidoDTO resultado = PartidoMapper.APartidoDTO(partido);

        Assert.AreEqual(10, resultado.Id);
        Assert.AreEqual(new DateTime(2026, 6, 12), resultado.Fecha);
        Assert.AreEqual(FasePartido.FaseDeGrupos, resultado.Fase);
        Assert.AreEqual("C1", resultado.EtiquetaCruce);
        Assert.AreEqual("A", resultado.Grupo);
        Assert.AreEqual(2, resultado.GolesLocal);
        Assert.AreEqual(1, resultado.GolesVisitante);
        Assert.IsTrue(resultado.EstaJugado);
        Assert.AreEqual("Uruguay", resultado.EquipoLocal!.Nombre);
        Assert.AreEqual("Brasil", resultado.EquipoVisitante!.Nombre);
        Assert.AreEqual("Centenario", resultado.Estadio!.Nombre);
    }

    [TestMethod]
    public void APartidoMapeaTodosLosCamposOK()
    {
        PartidoDTO partidoDTO = new PartidoDTO
        {
            Id = 10,
            Fecha = new DateTime(2026, 6, 12),
            Fase = FasePartido.FaseDeGrupos,
            EtiquetaCruce = "C1",
            GolesLocal = 2,
            GolesVisitante = 1,
            Grupo = "A",
            Estadio = EstadioMapper.AEstadioDTO(CrearEstadio()),
            EquipoLocal = EquipoMapper.AEquipoDTO(CrearEquipo("Uruguay")),
            EquipoVisitante = EquipoMapper.AEquipoDTO(CrearEquipo("Brasil"))
        };

        Partido resultado = PartidoMapper.APartido(partidoDTO);

        Assert.AreEqual(10, resultado.Id);
        Assert.AreEqual(new DateTime(2026, 6, 12), resultado.Fecha);
        Assert.AreEqual(FasePartido.FaseDeGrupos, resultado.Fase);
        Assert.AreEqual("C1", resultado.EtiquetaCruce);
        Assert.AreEqual("A", resultado.Grupo);
        Assert.AreEqual(2, resultado.GolesLocal);
        Assert.AreEqual(1, resultado.GolesVisitante);
        Assert.AreEqual("Uruguay", resultado.EquipoLocal.Nombre);
        Assert.AreEqual("Brasil", resultado.EquipoVisitante.Nombre);
        Assert.AreEqual("Centenario", resultado.Estadio.Nombre);
    }

    [TestMethod]
    public void PartidoDTOEstaJugadoFalsoSinResultadoOK()
    {
        PartidoDTO partidoDTO = new PartidoDTO { Grupo = "A" };

        Assert.IsFalse(partidoDTO.EstaJugado);
    }
}
