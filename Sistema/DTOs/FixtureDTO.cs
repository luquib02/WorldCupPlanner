using Dominio.Simulacion;

namespace Sistema.DTOs;

public class FixtureDTO
{
    public int Id { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public List<GrupoDTO> Grupos { get; set; } = new();
    public List<PartidoDTO> PartidosEliminatorias { get; set; } = new();
    public bool CrucesGenerados { get; set; }
    public int SemillaUtilizada { get; set; }
    public string NombreMotorSimulacion { get; set; } = NombresMotores.Probabilistico;
}
