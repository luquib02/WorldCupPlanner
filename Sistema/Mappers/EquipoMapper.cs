using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class EquipoMapper
{
    public static EquipoDTO AEquipoDTO(Equipo equipo)
    {
        return new EquipoDTO
        {
            Id = equipo.Id,
            Nombre = equipo.Nombre,
            Confederacion = equipo.Confederacion,
            RankingFIFA = equipo.RankingFIFA,
            RankingActual = equipo.RankingActual,
            BanderaBase64 = equipo.BanderaBase64
        };
    }

    public static Equipo AEquipo(EquipoDTO equipoDTO)
    {
        Equipo equipo = new Equipo(equipoDTO.Nombre, equipoDTO.Confederacion, equipoDTO.RankingFIFA);
        equipo.Id = equipoDTO.Id;
        if (equipoDTO.RankingActual != equipoDTO.RankingFIFA)
            equipo.ActualizarRankingActual(equipoDTO.RankingActual);
        if (!string.IsNullOrEmpty(equipoDTO.BanderaBase64))
            equipo.EstablecerBandera(equipoDTO.BanderaBase64);
        return equipo;
    }
}
