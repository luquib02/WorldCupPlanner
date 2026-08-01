namespace Dominio.Clasificacion;

public class ClasificadorFifa : IClasificador
{
    public List<PosicionEquipo> Ordenar(List<PosicionEquipo> tabla)
    {
        return tabla
            .OrderByDescending(p => p.Puntos)
            .ThenByDescending(p => p.DiferenciaGoles)
            .ThenByDescending(p => p.GolesAFavor)
            .ToList();
    }
}
