using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class FixtureMapper
{
    public static FixtureDTO AFixtureDTO(Fixture fixture)
    {
        return new FixtureDTO
        {
            Id = fixture.Id,
            FechaGeneracion = fixture.FechaGeneracion,
            Grupos = fixture.Grupos.Select(GrupoMapper.AGrupoDTO).ToList(),
            PartidosEliminatorias = fixture.PartidosEliminatorias.Select(PartidoMapper.APartidoDTO).ToList(),
            CrucesGenerados = fixture.CrucesGenerados,
            SemillaUtilizada = fixture.SemillaUtilizada,
            NombreMotorSimulacion = fixture.NombreMotorSimulacion
        };
    }

    public static Fixture AFixture(FixtureDTO fixtureDTO)
    {
        Fixture fixture = new Fixture(fixtureDTO.SemillaUtilizada, fixtureDTO.NombreMotorSimulacion);
        fixture.Id = fixtureDTO.Id;
        foreach (GrupoDTO grupoDTO in fixtureDTO.Grupos)
            fixture.AgregarGrupo(GrupoMapper.AGrupo(grupoDTO));
        foreach (PartidoDTO partidoDTO in fixtureDTO.PartidosEliminatorias)
            fixture.AgregarPartidoEliminatoria(PartidoMapper.APartido(partidoDTO));
        if (fixtureDTO.CrucesGenerados)
            fixture.MarcarCrucesGenerados();
        return fixture;
    }
}
