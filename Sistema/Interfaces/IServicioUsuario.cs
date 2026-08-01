using Dominio;

namespace Sistema;

public interface IServicioUsuario
{
    void CrearUsuario(Sistema.DTOs.UsuarioDTO usuario, string contrasenia);
    void ModificarUsuario(int usuarioId, Sistema.DTOs.UsuarioDTO usuarioModificado);
    void EliminarUsuario(int usuarioId);
    void AsignarRol(int usuarioId, RolDeUsuario administradorDelSistema);
    void IniciarSesion(string correoElectronico, string contrasenia);
    void CerrarSesion();
    void QuitarRol(int usuarioId, RolDeUsuario rol);
    IReadOnlyList<Sistema.DTOs.UsuarioDTO> ObtenerListaUsuarios();
    IReadOnlyList<RolDeUsuario> ObtenerListaRoles(int usuarioId);
    IReadOnlyList<RolDeUsuario>? ObtenerListaRoles();
    void ModificarContrasenia(string contraseniaActual, string contraseniaNueva);
    Sistema.DTOs.UsuarioDTO ObtenerUsuario(int idUsuario);
    void GenerarContraseniaDefecto(int usuarioId);
}
