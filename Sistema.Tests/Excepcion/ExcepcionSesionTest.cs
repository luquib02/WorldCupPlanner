using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sistema.Tests.Excepcion;

[TestClass]
[TestSubject(typeof(ExcepcionSesion))]
public class ExcepcionSesionTest
{
    [TestMethod]
    public void NoEstaLogueadoLanzaExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionSesion excepcion = Assert.Throws<ExcepcionSesion>(
            () => ExcepcionSesion.NoEstaLogueado());

        Assert.AreEqual("El usuario no inició sesión", excepcion.Message);
    }

    [TestMethod]
    public void UsuarioYaLogueadoLanzaExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionSesion excepcion = Assert.Throws<ExcepcionSesion>(
            () => ExcepcionSesion.UsuarioYaLogueado());

        Assert.AreEqual("El usuario ya tiene la sesión iniciada", excepcion.Message);
    }

    [TestMethod]
    public void SesionNoIniciadaLanzaExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionSesion excepcion = Assert.Throws<ExcepcionSesion>(
            () => ExcepcionSesion.SesionNoIniciada());

        Assert.AreEqual("No hay ningún usuario con sesión abierta.", excepcion.Message);
    }

    [TestMethod]
    public void SinPermisoLanzaExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionSesion excepcion = Assert.Throws<ExcepcionSesion>(
            () => ExcepcionSesion.SinPermiso());

        Assert.AreEqual("El usuario no tiene permisos los permisos necesarios.", excepcion.Message);
    }
}
