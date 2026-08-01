namespace Sistema.DTOs;

public class GrupoDTO
{
    public string Etiqueta { get; set; }
    public List<EquipoDTO> Equipos { get; set; } = new();
    public List<JornadaDTO> Jornadas { get; set; } = new();
}
