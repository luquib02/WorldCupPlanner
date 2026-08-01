using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class PosicionEquipoMapper
{
    public static PosicionEquipoDTO APosicionEquipoDTO(PosicionEquipo posicion)
    {
        return new PosicionEquipoDTO
        {
            Equipo = EquipoMapper.AEquipoDTO(posicion.Equipo),
            Puntos = posicion.Puntos,
            PartidosJugados = posicion.PartidosJugados,
            Ganados = posicion.Ganados,
            Empatados = posicion.Empatados,
            Perdidos = posicion.Perdidos,
            GolesAFavor = posicion.GolesAFavor,
            GolesEnContra = posicion.GolesEnContra,
            DiferenciaGoles = posicion.DiferenciaGoles
        };
    }

    public static PosicionEquipo APosicionEquipo(PosicionEquipoDTO posicionDTO)
    {
        // El dominio solo permite construir la posicion a partir del equipo;
        // las estadisticas se recalculan con RegistrarResultado, no se asignan directamente.
        return new PosicionEquipo(EquipoMapper.AEquipo(posicionDTO.Equipo));
    }
}
