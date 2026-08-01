using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class JornadaMapper
{
    public static JornadaDTO AJornadaDTO(Jornada jornada)
    {
        return new JornadaDTO
        {
            Numero = jornada.Numero,
            Fecha = jornada.Fecha,
            Partidos = jornada.Partidos.Select(PartidoMapper.APartidoDTO).ToList()
        };
    }

    public static Jornada AJornada(JornadaDTO jornadaDTO)
    {
        Jornada jornada = new Jornada(jornadaDTO.Numero, jornadaDTO.Fecha);
        foreach (PartidoDTO partidoDTO in jornadaDTO.Partidos)
            jornada.AgregarPartido(PartidoMapper.APartido(partidoDTO));
        return jornada;
    }
}
