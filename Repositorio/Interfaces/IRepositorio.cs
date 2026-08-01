namespace Repositorio;

public interface IRepositorio<T>
{
    void Agregar(T entidad);
    List<T> ListarTodos();
    T? BuscarPorId(int id);
    void Modificar(T entidad);
    void Eliminar(int id);
}