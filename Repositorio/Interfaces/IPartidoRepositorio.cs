using Dominio;

namespace Repositorio;

public interface IPartidoRepositorio
{
    void Agregar(Partido partido);
    Partido? ObtenerPorId(int id);
    List<Partido> ObtenerTodos();
    void Actualizar(Partido partido);
    void Eliminar(int id);
    List<Partido> ObtenerPorGrupo(string grupo);
    List<Partido> ObtenerPorFase(FasePartido fase);
    bool ExistenPartidosFueraDeGrupos();
}