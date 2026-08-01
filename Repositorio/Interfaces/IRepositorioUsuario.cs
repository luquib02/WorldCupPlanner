using Dominio;

namespace Repositorio;

public interface IRepositorioUsuario
{
    public void CrearUsuario(Usuario unUsuario);
    public bool ExisteUsuario(int idUsuario);
    public Usuario ObtenerUsuarioPorId(int idUsuario);
    public Usuario ObtenerUsuarioPorCorreo(string correo);
    public void ModificarUsuario(int usuarioAModificar, Usuario usuarioModificado);
    public void ActualizarContrasenia(int usuarioId, string hashDeContrasena);
    public void EliminarUsuario(int idUsuario);
    public bool ExisteUsuarioPorCorreo(string correo);
    public IReadOnlyList<Usuario> ObtenerListaUsuarios();
}