namespace Dominio.Clasificacion;

public interface IClasificador
{
    List<PosicionEquipo> Ordenar(List<PosicionEquipo> tabla);
}
