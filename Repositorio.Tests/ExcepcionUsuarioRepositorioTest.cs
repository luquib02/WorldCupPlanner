using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(ExcepcionUsuarioRepositorio))]
public class ExcepcionUsuarioRepositorioTest
{
    [TestMethod]
    public void UsuarioDuplicado_LanzaExcepcionConMensajeEsperado()
    {
        ExcepcionUsuarioRepositorio ex = Assert.Throws<ExcepcionUsuarioRepositorio>(
            () => ExcepcionUsuarioRepositorio.UsuarioDuplicado());

        Assert.AreEqual("El usuario ya existe en la base de datos.", ex.Message);
    }

    [TestMethod]
    public void UsuarioNoEncontrado_LanzaExcepcionConMensajeEsperado()
    {
        ExcepcionUsuarioRepositorio ex = Assert.Throws<ExcepcionUsuarioRepositorio>(
            () => ExcepcionUsuarioRepositorio.UsuarioNoEncontrado());

        Assert.AreEqual("El usuario no existe en la base de datos.", ex.Message);
    }

    [TestMethod]
    public void UsuarioInvalido_LanzaExcepcionConMensajeEsperado()
    {
        ExcepcionUsuarioRepositorio ex = Assert.Throws<ExcepcionUsuarioRepositorio>(
            () => ExcepcionUsuarioRepositorio.UsuarioInvalido());

        Assert.AreEqual("El usuario es inválido, por favor verifique los datos ingresados.", ex.Message);
    }
}
