namespace Sistema.DTOs;

public class JornadaDTO
{
    public int Numero { get; set; }
    public DateTime Fecha { get; set; }
    public List<PartidoDTO> Partidos { get; set; } = new();
}
