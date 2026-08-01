using Dominio;

namespace Repositorio;

public interface IEquipoRepositorio
{
    void Agregar(Equipo equipo);
    Equipo? ObtenerPorId(int id);
    List<Equipo> ObtenerTodos();
    void Actualizar(Equipo equipo);
    void Eliminar(int id);
    bool ExisteNombre(string nombre);
    int ContarPorConfederacion(Confederacion confederacion);
}