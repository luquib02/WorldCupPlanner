using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.Excepciones;

namespace Sistema.Tests.Excepcion;

[TestClass]
[TestSubject(typeof(ExcepcionLogServicio))]
public class ExcepcionLogServicioTest
{
    [TestMethod]
    public void EmailInvalidoDevuelveExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionLogServicio excepcion = ExcepcionLogServicio.EmailInvalido();

        Assert.AreEqual("El email del log es inválido.", excepcion.Message);
    }

    [TestMethod]
    public void AccionVaciaDevuelveExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionLogServicio excepcion = ExcepcionLogServicio.AccionVacia();

        Assert.AreEqual("La acción del log es obligatoria.", excepcion.Message);
    }

    [TestMethod]
    public void DetalleVacioDevuelveExcepcionConMensajeCorrespondienteOK()
    {
        ExcepcionLogServicio excepcion = ExcepcionLogServicio.DetalleVacio();

        Assert.AreEqual("El detalle del log es obligatorio.", excepcion.Message);
    }
}
