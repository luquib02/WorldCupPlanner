namespace Repositorio;
using Dominio;

public interface IRepositorioEstadio
{
    void Agregar(Estadio estadio);
    Estadio? BuscarPorId(int id);
    Estadio? BuscarPorNombre(string nombre);
    List<Estadio> ListarTodos();
    void Modificar(Estadio estadio);
    void Eliminar(int id);
}