namespace Sistema.Interfaces;

public interface IServicioRoles
{
    string[] ObtenerRolesDisponibles();
    bool EsAdmin();
    bool EsEditor();
    bool EsPeriodista();
    bool TienePermiso(string roleName);
    void AsignarRolPorNombre(int usuarioId, string roleName);
    void QuitarRolPorNombre(int usuarioId, string roleName);
}