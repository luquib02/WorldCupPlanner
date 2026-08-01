using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class PartidoMapper
{
    public static PartidoDTO APartidoDTO(Partido partido)
    {
        return new PartidoDTO
        {
            Id = partido.Id,
            Fecha = partido.Fecha,
            Fase = partido.Fase,
            EtiquetaCruce = partido.EtiquetaCruce,
            GolesLocal = partido.GolesLocal,
            GolesVisitante = partido.GolesVisitante,
            GolesPenalesLocal = partido.GolesPenalesLocal,
            GolesPenalesVisitante = partido.GolesPenalesVisitante,
            PartidoOrigenLocalId = partido.PartidoOrigenLocal?.Id,
            PartidoOrigenVisitanteId = partido.PartidoOrigenVisitante?.Id,
            Estadio = partido.Estadio != null ? EstadioMapper.AEstadioDTO(partido.Estadio) : null,
            EquipoLocal = partido.EquipoLocal != null ? EquipoMapper.AEquipoDTO(partido.EquipoLocal) : null,
            EquipoVisitante = partido.EquipoVisitante != null ? EquipoMapper.AEquipoDTO(partido.EquipoVisitante) : null,
            Grupo = partido.Grupo
        };
    }

    public static Partido APartido(PartidoDTO partidoDTO)
    {
        Partido partido = new Partido();
        partido.Id = partidoDTO.Id;
        partido.Fecha = partidoDTO.Fecha;
        partido.Fase = partidoDTO.Fase;
        partido.EtiquetaCruce = partidoDTO.EtiquetaCruce;
        partido.Grupo = partidoDTO.Grupo;
        if (partidoDTO.Estadio != null)
            partido.Estadio = EstadioMapper.AEstadio(partidoDTO.Estadio);
        if (partidoDTO.EquipoLocal != null)
            partido.EquipoLocal = EquipoMapper.AEquipo(partidoDTO.EquipoLocal);
        if (partidoDTO.EquipoVisitante != null)
            partido.EquipoVisitante = EquipoMapper.AEquipo(partidoDTO.EquipoVisitante);
        if (partidoDTO.GolesLocal.HasValue && partidoDTO.GolesVisitante.HasValue)
            partido.CargarResultado(partidoDTO.GolesLocal.Value, partidoDTO.GolesVisitante.Value);
        if (partidoDTO.GolesPenalesLocal.HasValue && partidoDTO.GolesPenalesVisitante.HasValue)
            partido.CargarPenales(partidoDTO.GolesPenalesLocal.Value, partidoDTO.GolesPenalesVisitante.Value);
        return partido;
    }
}
