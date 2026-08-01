using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sistema.Tests.Excepcion;

[TestClass]
[TestSubject(typeof(ExcepcionServicioUsuario))]
public class ExcepcionServicioUsuarioTest
{
    [TestMethod]
    public void ContraseniaIncorrectaLanzaExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionServicioUsuario excepcion = Assert.Throws<ExcepcionServicioUsuario>(
            () => ExcepcionServicioUsuario.ContraseniaIncorrecta());

        Assert.AreEqual("Contraseña incorrecta.", excepcion.Message);
    }
}
