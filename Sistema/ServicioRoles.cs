using Dominio;
using Sistema.Interfaces;

namespace Sistema.Servicios;

public class ServicioRoles : IServicioRoles
{
    private readonly IServicioSesion _servicioSesion;
    private readonly IServicioUsuario _servicioUsuario;

    public ServicioRoles(IServicioSesion servicioSesion, IServicioUsuario servicioUsuario)
    {
        _servicioSesion = servicioSesion;
        _servicioUsuario = servicioUsuario;
    }

    public string[] ObtenerRolesDisponibles()
    {
        return Enum.GetNames(typeof(RolDeUsuario));
    }

    public bool EsAdmin()
    {
        return TienePermiso("AdministradorDelSistema");
    }

    public bool EsEditor()
    {
        return TienePermiso("Editor");
    }

    public bool EsPeriodista()
    {
        return TienePermiso("Periodista");
    }

    public bool TienePermiso(string roleName)
    {
        try
        {
            if (!_servicioSesion.EstaLogueado) return false;
            if (!Enum.TryParse<RolDeUsuario>(roleName, out var rol))
                return false;
            _servicioSesion.TienePermiso(rol);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void AsignarRolPorNombre(int usuarioId, string roleName)
    {
        if (!Enum.TryParse<RolDeUsuario>(roleName, out var rol))
            throw new ArgumentException("Rol inválido", nameof(roleName));
        _servicioUsuario.AsignarRol(usuarioId, rol);
    }

    public void QuitarRolPorNombre(int usuarioId, string roleName)
    {
        if (!Enum.TryParse<RolDeUsuario>(roleName, out var rol))
            throw new ArgumentException("Rol inválido", nameof(roleName));
        _servicioUsuario.QuitarRol(usuarioId, rol);
    }
}