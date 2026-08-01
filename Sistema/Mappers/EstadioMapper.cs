using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class EstadioMapper
{
    public static EstadioDTO AEstadioDTO(Estadio estadio)
    {
        return new EstadioDTO
        {
            Id = estadio.Id,
            Nombre = estadio.Nombre,
            Ciudad = estadio.Ciudad,
            Descripcion = estadio.Descripcion,
            Capacidad = estadio.Capacidad
        };
    }

    public static Estadio AEstadio(EstadioDTO estadioDTO)
    {
        Estadio estadio = new Estadio
        {
            Id = estadioDTO.Id,
            Nombre = estadioDTO.Nombre,
            Ciudad = estadioDTO.Ciudad,
            Descripcion = estadioDTO.Descripcion,
            Capacidad = estadioDTO.Capacidad
        };
        return estadio;
    }
}
