using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class GrupoMapper
{
    public static GrupoDTO AGrupoDTO(Grupo grupo)
    {
        return new GrupoDTO
        {
            Etiqueta = grupo.Etiqueta,
            Equipos = grupo.Equipos.Select(EquipoMapper.AEquipoDTO).ToList(),
            Jornadas = grupo.Jornadas.Select(JornadaMapper.AJornadaDTO).ToList()
        };
    }

    public static Grupo AGrupo(GrupoDTO grupoDTO)
    {
        Grupo grupo = new Grupo(grupoDTO.Etiqueta);
        foreach (EquipoDTO equipoDTO in grupoDTO.Equipos)
            grupo.AgregarEquipo(EquipoMapper.AEquipo(equipoDTO));
        foreach (JornadaDTO jornadaDTO in grupoDTO.Jornadas)
            grupo.AgregarJornada(JornadaMapper.AJornada(jornadaDTO));
        return grupo;
    }
}
