using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(UsuarioRolRecord))]
public class UsuarioRolRecordTest
{
    [TestMethod]
    public void AsignarYLeerPropiedadesOK()
    {
        UsuarioRolRecord registro = new UsuarioRolRecord
        {
            Id = 5,
            UsuarioId = 10,
            Rol = "Editor"
        };

        Assert.AreEqual(5, registro.Id);
        Assert.AreEqual(10, registro.UsuarioId);
        Assert.AreEqual("Editor", registro.Rol);
    }
}
