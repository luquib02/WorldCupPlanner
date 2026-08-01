using Dominio;
namespace Sistema.DTOs;

public class EquipoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public Confederacion Confederacion { get; set; }
    public int RankingFIFA { get; set; }
    public int RankingActual { get; set; }
    public string? BanderaBase64 { get; set; }
}
