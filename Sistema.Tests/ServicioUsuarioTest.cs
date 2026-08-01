using System;
using System.Collections.Generic;
using System.Linq;
using Dominio;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositorio;
using Repositorio.DataAccess;
using Sistema.Interfaces;
using Sistema.DTOs;
using Moq;

namespace Sistema.Tests;

[TestClass]
[TestSubject(typeof(ServicioUsuario))]
public class ServicioUsuarioTest
{
    private IServicioUsuario _servicioUsuario;
    private IServicioSesion _servicioSesion;
    private IRepositorioUsuario _repositorioUsuario;
    private IServicioLog _servicioLog;
    private List<LogEntry> _logCapturados = null!;
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repositorioUsuario = new RepositorioUsuario(_contexto);
        _servicioSesion = new ServicioSesion();
        
        _logCapturados = new List<LogEntry>();
        var logRepoMock = new Mock<IRepositorioLog>();
        logRepoMock
            .Setup(r => r.Agregar(It.IsAny<LogEntry>()))
            .Callback<LogEntry>(l => _logCapturados.Add(l));
        logRepoMock
            .Setup(r => r.ListarTodos())
            .Returns(() => _logCapturados);
        _servicioLog = new LogServicio(logRepoMock.Object);

        _servicioUsuario = new ServicioUsuario(_repositorioUsuario, _servicioSesion, _servicioLog);

        Usuario usuarioAdministradorSesion = CrearUsuarioAdministrador();
        _repositorioUsuario.CrearUsuario(usuarioAdministradorSesion);
        _servicioSesion.IniciarSesion(usuarioAdministradorSesion);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    private static Usuario CrearUsuarioAdministrador()
    {
        Usuario usuario = new Usuario();
        usuario.Nombre = "Admin";
        usuario.Apellido = "Sistema";
        usuario.CorreoElectronico = "admin@sistema.com";
        usuario.FechaNacimiento = new DateOnly(2000, 10, 4);
        usuario.EstablecerContrasenia("Admin123!@");
        usuario.AgregarRol(RolDeUsuario.AdministradorDelSistema);

        return usuario;
    }

    private static Usuario CrearUsuarioEditor()
    {
        Usuario usuario = new()
        {
            Nombre = "Editor",
            Apellido = "Sistema",
            CorreoElectronico = "editor@sistema.com",
            FechaNacimiento = new DateOnly(1998, 5, 21)
        };
        usuario.EstablecerContrasenia("Editor123!@");
        usuario.AgregarRol(RolDeUsuario.Editor);
        return usuario;
    }

    private Usuario CrearUsuarioEditorPersistido()
    {
        Usuario usuario = CrearUsuarioEditor();
        _servicioUsuario.CrearUsuario(UsuarioDTO.convertirAUsuarioDTO(usuario), "Editor123!@");
        return _repositorioUsuario.ObtenerUsuarioPorCorreo(usuario.CorreoElectronico);
    }

    [TestMethod]
    public void NuevoServicioUsuario_SeInicializaCorrectamente()
    {
        Assert.IsNotNull(_servicioUsuario);
    }

    [TestMethod]
    public void CrearUsuario_UsuarioValido_CreaUsuario()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();

        Assert.IsTrue(_repositorioUsuario.ExisteUsuario(usuario.Id));
    }

    [TestMethod]
    public void ModificarUsuario_UsuarioExistente_ModificaUsuario()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();

        Usuario usuarioModificado = new Usuario();
        usuarioModificado.Nombre = "Administrador";
        usuarioModificado.Apellido = "Principal";
        usuarioModificado.CorreoElectronico = "administrador@sistema.com";
        usuarioModificado.FechaNacimiento = new DateOnly(1995, 5, 15);
        usuarioModificado.EstablecerContrasenia("Admin123!@");
        usuarioModificado.AgregarRol(RolDeUsuario.AdministradorDelSistema);

        _servicioUsuario.ModificarUsuario(usuario.Id, UsuarioDTO.convertirAUsuarioDTO(usuarioModificado));

        Usuario usuarioObtenido = _repositorioUsuario.ObtenerUsuarioPorId(usuario.Id);
        Assert.AreEqual("Administrador", usuarioObtenido.Nombre);
        Assert.AreEqual("Principal", usuarioObtenido.Apellido);
        Assert.AreEqual("administrador@sistema.com", usuarioObtenido.CorreoElectronico);
        Assert.AreEqual(new DateOnly(1995, 5, 15), usuarioObtenido.FechaNacimiento);
    }

    [TestMethod]
    public void ModificarUsuario_CambiandoDatosPersonales_ConservaElHashDeContrasenia()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        string contraseniaOriginal = "Editor123!@";

        UsuarioDTO datosModificados = UsuarioDTO.convertirAUsuarioDTO(usuario);
        datosModificados.Apellido = "ApellidoNuevo";

        _servicioUsuario.ModificarUsuario(usuario.Id, datosModificados);

        Usuario usuarioObtenido = _repositorioUsuario.ObtenerUsuarioPorId(usuario.Id);
        Assert.AreEqual("ApellidoNuevo", usuarioObtenido.Apellido, "El dato modificado debe persistirse");
        Assert.IsTrue(usuarioObtenido.VerificarContrasenia(contraseniaOriginal),
            "Modificar datos personales no debe borrar el hash de la contraseña");
    }

    [TestMethod]
    public void EliminarUsuario_UsuarioExistente_EliminaUsuario()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();

        _servicioUsuario.EliminarUsuario(usuario.Id);

        Assert.IsFalse(_repositorioUsuario.ExisteUsuario(usuario.Id));
    }

    [TestMethod]
    public void EliminarUsuario_UsuarioInexistente_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionUsuarioRepositorio>(() => _servicioUsuario.EliminarUsuario(CrearUsuarioEditor().Id));
    }

    [TestMethod]
    public void AsignarRol_UsuarioExistente_AsignaRol()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();

        _servicioUsuario.AsignarRol(usuario.Id, RolDeUsuario.AdministradorDelSistema);

        Usuario usuarioObtenido = _repositorioUsuario.ObtenerUsuarioPorId(usuario.Id);
        Assert.IsTrue(usuarioObtenido.Roles.Contains(RolDeUsuario.AdministradorDelSistema) && usuarioObtenido.Roles.Contains(RolDeUsuario.Editor));
    }
    
    [TestMethod]
    public void IniciarSesion_UsuarioEditor_EmailYContraseniaCorrectos()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        _servicioUsuario.CerrarSesion();
        _servicioUsuario.IniciarSesion(usuario.CorreoElectronico, "Editor123!@");
        Assert.IsTrue(_servicioSesion.EstaLogueado);
        Assert.AreEqual(usuario.Id, _servicioSesion.ObtenerUsuarioActual().Id);
    }

    [TestMethod]
    public void IniciarSesion_UsuarioCredencialesIncorrectas_LanzaExcepcion()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        _servicioUsuario.CerrarSesion();
        string contraseniaIncorrecta = "Eeditor123!@";
        Assert.Throws<ExcepcionServicioUsuario>(() => _servicioUsuario.IniciarSesion(usuario.CorreoElectronico, contraseniaIncorrecta));
    }
    
    [TestMethod]
    public void QuitarRol_UsuarioExistentel()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();

        _servicioUsuario.QuitarRol(usuario.Id, RolDeUsuario.Editor);
        Usuario usuarioObtenido = _repositorioUsuario.ObtenerUsuarioPorId(usuario.Id);
        Assert.IsFalse(usuarioObtenido.Roles.Contains(RolDeUsuario.Editor));
    }
    
    [TestMethod]
    public void QuitarRol_UsuarioSinPermisos_LanzaExcepcion()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        _servicioUsuario.CerrarSesion();
        _servicioUsuario.IniciarSesion(usuario.CorreoElectronico, "Editor123!@");
        Assert.Throws<ExcepcionSesion>(() => _servicioUsuario.QuitarRol(usuario.Id, RolDeUsuario.Editor));
    }
    
    [TestMethod]
    public void QuitarRol_UsuarioInexistente_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionUsuarioRepositorio>(() => _servicioUsuario.QuitarRol(CrearUsuarioEditor().Id, RolDeUsuario.Editor));
    }
    
    [TestMethod]
    public void ObtenerUsuarios_DevuelveListaDeUsuarios()
    {
        CrearUsuarioEditorPersistido();
        
       Assert.AreEqual(2, _servicioUsuario.ObtenerListaUsuarios().Count); 
    }

    [TestMethod]
    public void ObtenerRoles_Usuario_SiendoAdministrador()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        IReadOnlyList<RolDeUsuario> rolEnSistema = _servicioUsuario.ObtenerListaRoles(usuario.Id); 
        Assert.AreEqual(usuario.Roles.Count, rolEnSistema.Count);
        Assert.IsTrue(rolEnSistema.All(r => usuario.Roles.Contains(r)));
    }
    
    [TestMethod]
    public void ObtenerRolesDeUnUsuario_SinPermisos_LanzaExcepcion()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        _servicioUsuario.CerrarSesion();
        _servicioUsuario.IniciarSesion(usuario.CorreoElectronico, "Editor123!@");
        Assert.Throws<ExcepcionSesion>(() => _servicioUsuario.ObtenerListaRoles(usuario.Id));
    }

    [TestMethod]
    public void ObtenerMisRoles_SinSerAdministrador()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        _servicioUsuario.CerrarSesion();
        _servicioUsuario.IniciarSesion(usuario.CorreoElectronico, "Editor123!@");
        IReadOnlyList<RolDeUsuario> misRoles = _servicioUsuario.ObtenerListaRoles();
        Assert.IsTrue(misRoles.All(r => usuario.Roles.Contains(r)));
    }
    
    [TestMethod]
    public void ObtenerMisRoles_SiendoAdministrador()
    {
        IReadOnlyList<RolDeUsuario> misRoles = _servicioUsuario.ObtenerListaRoles();
        Assert.IsTrue(misRoles.All(r => _servicioSesion.UsuarioActual.Roles.Contains(r)));
    }

    [TestMethod]
    public void ModificarContraseña_UsuarioAdministrador()
    {
        string contraseniaActual = "Admin123!@";
        string contrasenianueva = "Testeando123!@#";
        _servicioUsuario.ModificarContrasenia(contraseniaActual, contrasenianueva);
        Usuario usuarioModificado = _repositorioUsuario.ObtenerUsuarioPorId(_servicioSesion.UsuarioActual.Id);
        Assert.IsTrue(usuarioModificado.VerificarContrasenia(contrasenianueva));
    }

    [TestMethod]
    public void ModificarContrasenia_ContraseniaActualIncorrecta_LanzaExcepcionYNoCambia()
    {
        string contraseniaActualCorrecta = "Admin123!@";
        string contraseniaActualIncorrecta = "Incorrecta999!@";
        string contraseniaNueva = "Testeando123!@#";

        Assert.Throws<ExcepcionServicioUsuario>(
            () => _servicioUsuario.ModificarContrasenia(contraseniaActualIncorrecta, contraseniaNueva));

        Usuario usuarioSinCambios = _repositorioUsuario.ObtenerUsuarioPorId(_servicioSesion.UsuarioActual.Id);
        Assert.IsTrue(usuarioSinCambios.VerificarContrasenia(contraseniaActualCorrecta),
            "Si la contraseña actual es incorrecta, la contraseña no debe cambiar");
        Assert.IsFalse(usuarioSinCambios.VerificarContrasenia(contraseniaNueva));
    }

    [TestMethod]
    public void ObtenerUsuario_SiendoAdministrador()
    {
        Usuario usuarioEditor = CrearUsuarioEditorPersistido();
        var dto = _servicioUsuario.ObtenerUsuario(usuarioEditor.Id);
        Assert.AreEqual(usuarioEditor.Nombre, dto.Nombre);
        Assert.AreEqual(usuarioEditor.Apellido, dto.Apellido);
        Assert.AreEqual(usuarioEditor.CorreoElectronico, dto.CorreoElectronico);
    }
    
    [TestMethod]
    public void ObtenerUsuario_SinPermisos_LanzaExcepcion()
    {
        int idAdmin = _servicioSesion.UsuarioActual.Id;
        Usuario usuarioEditor = CrearUsuarioEditorPersistido();
        _servicioUsuario.CerrarSesion();
        _servicioUsuario.IniciarSesion(usuarioEditor.CorreoElectronico, "Editor123!@");
        Assert.Throws<ExcepcionSesion>(() => _servicioUsuario.ObtenerUsuario(idAdmin));
    }

    [TestMethod]
    public void GenerarContraseniaDefecto_AdministradorGeneraContraseniaParaUsuarioExistente_CambiaContrasenia()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();
        string contraseniaOriginal = "Editor123!@";

        _servicioUsuario.GenerarContraseniaDefecto(usuario.Id);

        Usuario usuarioModificado = _repositorioUsuario.ObtenerUsuarioPorId(usuario.Id);
        Assert.IsFalse(usuarioModificado.VerificarContrasenia(contraseniaOriginal));
        Assert.IsTrue(usuarioModificado.VerificarContrasenia("Sistema123!@"));
    }

    [TestMethod]
    public void AltaDeUsuario_SeRegistraLog()
    {
        CrearUsuarioEditorPersistido();
        Assert.AreEqual(1, _servicioLog.ListarLogs().Count);
        Assert.AreEqual("Alta de Usuario", _servicioLog.ListarLogs().First().Accion);
    }

    [TestMethod]
    public void EliminarUsuario_SeRegistraLog()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();

        _servicioUsuario.EliminarUsuario(usuario.Id);

        Assert.IsTrue(_servicioLog.ListarLogs().Any(l => l.Accion == "Baja de Usuario"),
            "Eliminar un usuario debe registrarse en el log de auditoría");
    }

    [TestMethod]
    public void AsignarRol_SeRegistraLog()
    {
        Usuario usuario = CrearUsuarioEditorPersistido();

        _servicioUsuario.AsignarRol(usuario.Id, RolDeUsuario.AdministradorDelSistema);

        Assert.IsTrue(_servicioLog.ListarLogs().Any(l => l.Accion == "Asignacion de Rol"),
            "Asignar un rol debe registrarse en el log de auditoría");
    }

}
