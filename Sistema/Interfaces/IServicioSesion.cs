using Dominio;

namespace Sistema.Interfaces;

public interface IServicioSesion
{
    public void IniciarSesion(Usuario usuario);
    public bool EstaLogueado { get; }
    public Usuario? UsuarioActual { get; }
    public Usuario ObtenerUsuarioActual();
    public void CerrarSesion();
    public void TienePermiso(RolDeUsuario rol);
    void ActualizarUsuarioActual(Usuario usuario);
}