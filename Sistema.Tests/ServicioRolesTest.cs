using System;
using Dominio;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sistema.Interfaces;
using Sistema.Servicios;

namespace Sistema.Tests;

[TestClass]
[TestSubject(typeof(ServicioRoles))]
public class ServicioRolesTest
{
    private Mock<IServicioSesion> _servicioSesionMock = null!;
    private Mock<IServicioUsuario> _servicioUsuarioMock = null!;
    private ServicioRoles _servicioRoles = null!;

    [TestInitialize]
    public void Setup()
    {
        _servicioSesionMock = new Mock<IServicioSesion>();
        _servicioUsuarioMock = new Mock<IServicioUsuario>();
        _servicioRoles = new ServicioRoles(_servicioSesionMock.Object, _servicioUsuarioMock.Object);
    }

    [TestMethod]
    public void ObtenerRolesDisponiblesDevuelveTodosLosValoresDelEnumOK()
    {
        string[] roles = _servicioRoles.ObtenerRolesDisponibles();

        CollectionAssert.AreEquivalent(
            new[] { "AdministradorDelSistema", "Editor", "Periodista" },
            roles);
    }

    [TestMethod]
    public void TienePermisoSinUsuarioLogueadoDevuelveFalse()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(false);

        Assert.IsFalse(_servicioRoles.TienePermiso("AdministradorDelSistema"));
    }

    [TestMethod]
    public void TienePermisoConRolInvalidoDevuelveFalse()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);

        Assert.IsFalse(_servicioRoles.TienePermiso("RolQueNoExiste"));
    }

    [TestMethod]
    public void TienePermisoConSesionSinPermisoDevuelveFalse()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.AdministradorDelSistema))
            .Throws(new InvalidOperationException());

        Assert.IsFalse(_servicioRoles.TienePermiso("AdministradorDelSistema"));
    }

    [TestMethod]
    public void TienePermisoConUsuarioLogueadoYRolValidoDevuelveTrue()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.Editor));

        Assert.IsTrue(_servicioRoles.TienePermiso("Editor"));
    }

    [TestMethod]
    public void EsAdminConPermisoDeAdministradorDevuelveTrue()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.AdministradorDelSistema));

        Assert.IsTrue(_servicioRoles.EsAdmin());
    }

    [TestMethod]
    public void EsAdminSinPermisoDeAdministradorDevuelveFalse()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.AdministradorDelSistema))
            .Throws(new InvalidOperationException());

        Assert.IsFalse(_servicioRoles.EsAdmin());
    }

    [TestMethod]
    public void EsEditorConPermisoDeEditorDevuelveTrue()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.Editor));

        Assert.IsTrue(_servicioRoles.EsEditor());
    }

    [TestMethod]
    public void EsEditorSinPermisoDeEditorDevuelveFalse()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.Editor))
            .Throws(new InvalidOperationException());

        Assert.IsFalse(_servicioRoles.EsEditor());
    }

    [TestMethod]
    public void EsPeriodistaConPermisoDePeriodistaDevuelveTrue()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.Periodista));

        Assert.IsTrue(_servicioRoles.EsPeriodista());
    }

    [TestMethod]
    public void EsPeriodistaSinPermisoDePeriodistaDevuelveFalse()
    {
        _servicioSesionMock.Setup(s => s.EstaLogueado).Returns(true);
        _servicioSesionMock.Setup(s => s.TienePermiso(RolDeUsuario.Periodista))
            .Throws(new InvalidOperationException());

        Assert.IsFalse(_servicioRoles.EsPeriodista());
    }

    [TestMethod]
    public void AsignarRolPorNombreConRolValidoLlamaAlServicioUsuarioOK()
    {
        _servicioRoles.AsignarRolPorNombre(1, "Editor");

        _servicioUsuarioMock.Verify(s => s.AsignarRol(1, RolDeUsuario.Editor), Times.Once);
    }

    [TestMethod]
    public void AsignarRolPorNombreConRolInvalidoLanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => _servicioRoles.AsignarRolPorNombre(1, "RolQueNoExiste"));
    }

    [TestMethod]
    public void QuitarRolPorNombreConRolValidoLlamaAlServicioUsuarioOK()
    {
        _servicioRoles.QuitarRolPorNombre(1, "Periodista");

        _servicioUsuarioMock.Verify(s => s.QuitarRol(1, RolDeUsuario.Periodista), Times.Once);
    }

    [TestMethod]
    public void QuitarRolPorNombreConRolInvalidoLanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => _servicioRoles.QuitarRolPorNombre(1, "RolQueNoExiste"));
    }
}
