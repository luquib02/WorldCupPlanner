using Dominio;
using Sistema.Interfaces;

namespace Sistema;

public class ServicioSesion : IServicioSesion
{
    private Usuario? _usuarioActual;

    public Usuario? UsuarioActual => _usuarioActual;

    public bool EstaLogueado => _usuarioActual is not null;
    
    public void IniciarSesion(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (EstaLogueado)
            throw ExcepcionSesion.UsuarioYaLogueado();

        _usuarioActual = usuario;
    }
    
    public void CerrarSesion()
    {
        _usuarioActual = null;
    }
    
    public Usuario ObtenerUsuarioActual()
    {
        return !EstaLogueado ? throw ExcepcionSesion.SesionNoIniciada() : _usuarioActual!;
    }

    public void TienePermiso(RolDeUsuario rol)
    {
        Usuario usuarioActual = _usuarioActual ?? throw ExcepcionSesion.NoEstaLogueado();
        if (!usuarioActual.Roles.Contains(rol))
        {
            throw ExcepcionSesion.SinPermiso();
        }
    }
    public void ActualizarUsuarioActual(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        _usuarioActual = usuario;
    }
}
