using System;
using Dominio;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
public class NotificacionTest
{
    private class NotificacionDePrueba : Notificacion { }


    [TestMethod]
    public void Constructor_DatosValidos_CreaNotificacion()
    {
        Notificacion notificacion = new Notificacion("Resultado cargado: Uruguay 2-1 Argentina", 1, 7);

        Assert.AreEqual("Resultado cargado: Uruguay 2-1 Argentina", notificacion.Mensaje);
        Assert.AreEqual(1, notificacion.PartidoId);
        Assert.AreEqual(7, notificacion.DestinatarioId);
        Assert.IsFalse(notificacion.Leida);
        Assert.IsTrue(notificacion.Timestamp <= DateTime.Now);
    }
    [TestMethod]
    public void Constructor_MensajeNulo_LanzaExcepcion()
    {
        bool lanzoExcepcion = false;
        try
        {
            Notificacion notificacion = new Notificacion(null!, 1, 7);
        }
        catch (DominioException)
        {
            lanzoExcepcion = true;
        }
        Assert.IsTrue(lanzoExcepcion);
    }
    [TestMethod]
    public void Constructor_MensajeVacio_LanzaExcepcion()
    {
        bool lanzoExcepcion = false;
        try
        {
            Notificacion notificacion = new Notificacion("", 1, 7);
        }
        catch (DominioException)
        {
            lanzoExcepcion = true;
        }
        Assert.IsTrue(lanzoExcepcion);
    }
    [TestMethod]
    public void Constructor_PartidoIdInvalido_LanzaExcepcion()
    {
        bool lanzoExcepcion = false;
        try
        {
            Notificacion notificacion = new Notificacion("Resultado cargado", 0, 7);
        }
        catch (DominioException)
        {
            lanzoExcepcion = true;
        }
        Assert.IsTrue(lanzoExcepcion);
    }
    [TestMethod]
    public void Constructor_DestinatarioIdInvalido_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => new Notificacion("Resultado cargado", 1, 0));
        Assert.AreEqual("El destinatario es obligatorio.", ex.Message);
    }
    [TestMethod]
    public void Constructor_MensajeMasDe500Caracteres_LanzaExcepcion()
    {
        DominioException ex = Assert.Throws<DominioException>(
            () => new Notificacion(new string('A', 501), 1, 7));
        Assert.AreEqual("El mensaje no puede superar los 500 caracteres.", ex.Message);
    }

    [TestMethod]
    public void Id_AsignaYDevuelveElValorAsignadoOK()
    {
        Notificacion notificacion = new Notificacion("Resultado cargado", 1, 7);

        notificacion.Id = 6;

        Assert.AreEqual(6, notificacion.Id);
    }

    [TestMethod]
    public void ConstructorProtegido_CreaInstanciaOK()
    {
        NotificacionDePrueba notificacion = new NotificacionDePrueba();

        Assert.AreEqual(0, notificacion.Id);
    }

    [TestMethod]
    public void MarcarComoLeida_NotificacionNoLeida_MarcaComoLeida()
    {
        Notificacion notificacion = new Notificacion("Resultado cargado", 1, 7);

        notificacion.MarcarComoLeida();

        Assert.IsTrue(notificacion.Leida);
    }
    [TestMethod]
    public void MarcarComoLeida_NotificacionYaLeida_PermaneceLeida()
    {
        Notificacion notificacion = new Notificacion("Resultado cargado", 1, 7);
        notificacion.MarcarComoLeida();

        notificacion.MarcarComoLeida();

        Assert.IsTrue(notificacion.Leida);
    }
    
}