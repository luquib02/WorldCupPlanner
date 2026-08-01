using Dominio;
namespace Sistema.DTOs;

public class PartidoDTO
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public FasePartido Fase { get; set; }
    public string? EtiquetaCruce { get; set; }
    public int? GolesLocal { get; set; }
    public int? GolesVisitante { get; set; }
    public int? GolesPenalesLocal { get; set; }
    public int? GolesPenalesVisitante { get; set; }
    public int? PartidoOrigenLocalId { get; set; }
    public int? PartidoOrigenVisitanteId { get; set; }
    public EstadioDTO? Estadio { get; set; }
    public EquipoDTO? EquipoLocal { get; set; }
    public EquipoDTO? EquipoVisitante { get; set; }
    public string Grupo { get; set; }

    public bool EstaJugado => GolesLocal.HasValue && GolesVisitante.HasValue;
}
