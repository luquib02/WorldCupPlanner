using System;
using Dominio;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.Interfaces;

namespace Sistema.Tests;

[TestClass]
[TestSubject(typeof(ServicioSesion))]
public class ServicioSesionTest
{
    private IServicioSesion _sesion;
    private Usuario _usuario;

    private static Usuario CrearUsuarioValido()
    {
        Usuario usuario = new Usuario();
        usuario.Nombre = "Admin";
        usuario.Apellido = "Sistema";
        usuario.CorreoElectronico = "admin@sistema.com";
        usuario.FechaNacimiento = new DateOnly(1999, 10, 4);
        usuario.EstablecerContrasenia("Admin123!@");
        usuario.AgregarRol(RolDeUsuario.AdministradorDelSistema);
        return usuario;
    }

    [TestInitialize]
    public void Setup()
    {
        _sesion = new ServicioSesion();
        _usuario = CrearUsuarioValido();
    }


    [TestMethod]
    public void NuevoServicio_NoEstaLogueado()
    {
        Assert.IsFalse(_sesion.EstaLogueado);
        Assert.IsNull(_sesion.UsuarioActual);
    }

    [TestMethod]
    public void NoEstaLogueado_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionSesion>(() => ExcepcionSesion.NoEstaLogueado());
    }

    [TestMethod]
    public void IniciarSesion_UsuarioValido()
    {
        _sesion.IniciarSesion(_usuario);
    }

    [TestMethod]
    public void IniciarSesion_SesionYaActiva_LanzaExcepcion()
    {
        _sesion.IniciarSesion(_usuario);

        Assert.Throws<ExcepcionSesion>(() => _sesion.IniciarSesion(_usuario));
    }

    [TestMethod]
    public void CerrarSesion_ConSesionActiva_CierraSesion()
    {
        _sesion.IniciarSesion(_usuario);

        _sesion.CerrarSesion();

        Assert.IsFalse(_sesion.EstaLogueado);
        Assert.IsNull(_sesion.UsuarioActual);
    }

    [TestMethod]
    public void CerrarSesion_SinSesionActiva_NoFalla()
    {
        _sesion.CerrarSesion();

        Assert.IsFalse(_sesion.EstaLogueado);
    }

    [TestMethod]
    public void ObtenerUsuarioActual_ConSesionActiva()
    {
        _sesion.IniciarSesion(_usuario);
        Assert.AreEqual(_usuario, _sesion.ObtenerUsuarioActual());
    }

    [TestMethod]
    public void ObtenerUsuarioActual_SinSesionActiva_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionSesion>(() => _sesion.ObtenerUsuarioActual());
    }

    [TestMethod]
    public void ObtenerUsuarioActual_UsuarioEditor_NoTienePermisoAdministrador()
    {
        _usuario.AgregarRol(RolDeUsuario.Editor);
        _usuario.QuitarRol(RolDeUsuario.AdministradorDelSistema);
        _sesion.IniciarSesion(_usuario);
        Assert.Throws<ExcepcionSesion>(() => _sesion.TienePermiso(RolDeUsuario.AdministradorDelSistema));
    }

    [TestMethod]
    public void TienePermiso_SinUsuarioLogueado_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionSesion>(() => _sesion.TienePermiso(RolDeUsuario.AdministradorDelSistema));
    }

    [TestMethod]
    public void TienePermiso_UsuarioConRol_NoLanzaExcepcion()
    {
        _sesion.IniciarSesion(_usuario);

        _sesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
    }

    [TestMethod]
    public void ActualizarUsuarioActual_UsuarioValido_EstableceUsuarioActual()
    {
        _sesion.ActualizarUsuarioActual(_usuario);

        Assert.AreEqual(_usuario, _sesion.UsuarioActual);
        Assert.IsTrue(_sesion.EstaLogueado);
    }

}
