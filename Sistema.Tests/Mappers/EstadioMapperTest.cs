using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema.Tests.Mappers;

[TestClass]
public class EstadioMapperTest
{
    [TestMethod]
    public void AEstadioDTOMapeaTodosLosCamposOK()
    {
        Estadio estadio = new Estadio
        {
            Id = 3,
            Nombre = "Maracana",
            Ciudad = "Rio de Janeiro",
            Descripcion = "Estadio historico",
            Capacidad = 78000
        };

        EstadioDTO resultado = EstadioMapper.AEstadioDTO(estadio);

        Assert.AreEqual(3, resultado.Id);
        Assert.AreEqual("Maracana", resultado.Nombre);
        Assert.AreEqual("Rio de Janeiro", resultado.Ciudad);
        Assert.AreEqual("Estadio historico", resultado.Descripcion);
        Assert.AreEqual(78000, resultado.Capacidad);
    }

    [TestMethod]
    public void AEstadioMapeaTodosLosCamposOK()
    {
        EstadioDTO estadioDTO = new EstadioDTO
        {
            Id = 3,
            Nombre = "Maracana",
            Ciudad = "Rio de Janeiro",
            Descripcion = "Estadio historico",
            Capacidad = 78000
        };

        Estadio resultado = EstadioMapper.AEstadio(estadioDTO);

        Assert.AreEqual(3, resultado.Id);
        Assert.AreEqual("Maracana", resultado.Nombre);
        Assert.AreEqual("Rio de Janeiro", resultado.Ciudad);
        Assert.AreEqual("Estadio historico", resultado.Descripcion);
        Assert.AreEqual(78000, resultado.Capacidad);
    }
}
