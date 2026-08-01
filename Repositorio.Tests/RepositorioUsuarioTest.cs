using System;
using System.Linq;
using Dominio;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositorio.DataAccess;

namespace Repositorio.Tests;

[TestClass]
[TestSubject(typeof(RepositorioUsuario))]
public class RepositorioUsuarioTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private WorldCupPlannerDbContext _contexto = null!;
    private RepositorioUsuario _repo = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _contexto = _contextFactory.CreateDbContext();
        _repo = new RepositorioUsuario(_contexto);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    private Usuario CrearUsuarioValido(string correo = "test@test.com")
    {
        var usuario = new Usuario();
        usuario.Nombre = "Juan";
        usuario.Apellido = "Perez";
        usuario.CorreoElectronico = correo;
        usuario.FechaNacimiento = new DateOnly(1990, 1, 1);
        usuario.EstablecerContrasenia("Pass123!@");
        return usuario;
    }

    [TestMethod]
    public void CrearUsuario_UsuarioValido_PersisteDatos()
    {
        var usuario = CrearUsuarioValido();

        _repo.CrearUsuario(usuario);

        var obtenido = _repo.ObtenerUsuarioPorId(usuario.Id);
        Assert.AreEqual("Juan", obtenido.Nombre);
        Assert.AreEqual("test@test.com", obtenido.CorreoElectronico);
    }

    [TestMethod]
    public void ObtenerUsuarioPorCorreo_UsuarioPersistido_RetornaConRoles()
    {
        var usuario = CrearUsuarioValido();
        usuario.AgregarRol(RolDeUsuario.Periodista);
        _repo.CrearUsuario(usuario);

        var obtenido = _repo.ObtenerUsuarioPorCorreo("test@test.com");

        Assert.IsTrue(obtenido.Roles.Contains(RolDeUsuario.Periodista));
    }

    [TestMethod]
    public void CrearUsuario_CorreoDuplicado_LanzaExcepcion()
    {
        var usuario1 = CrearUsuarioValido("dup@test.com");
        var usuario2 = CrearUsuarioValido("dup@test.com");
        _repo.CrearUsuario(usuario1);

        Assert.Throws<ExcepcionUsuarioRepositorio>(() => _repo.CrearUsuario(usuario2));
    }

    [TestMethod]
    public void EliminarUsuario_UsuarioExistente_NoEstaEnLista()
    {
        var usuario = CrearUsuarioValido();
        _repo.CrearUsuario(usuario);
        int id = usuario.Id;

        _repo.EliminarUsuario(id);

        Assert.IsFalse(_repo.ExisteUsuario(id));
    }

    [TestMethod]
    public void ModificarUsuario_CambiaRol_PersisteCambio()
    {
        var usuario = CrearUsuarioValido();
        _repo.CrearUsuario(usuario);

        var cargado = _repo.ObtenerUsuarioPorId(usuario.Id);
        cargado.AgregarRol(RolDeUsuario.Editor);
        _repo.ModificarUsuario(cargado.Id, cargado);

        var actualizado = _repo.ObtenerUsuarioPorId(cargado.Id);
        Assert.IsTrue(actualizado.Roles.Contains(RolDeUsuario.Editor));
    }

    [TestMethod]
    public void ObtenerUsuarioPorId_UsuarioInexistente_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionUsuarioRepositorio>(() => _repo.ObtenerUsuarioPorId(999));
    }

    [TestMethod]
    public void ObtenerListaUsuarios_ConUsuariosPersistidos_RetornaTodosConRoles()
    {
        var usuario1 = CrearUsuarioValido("uno@test.com");
        usuario1.AgregarRol(RolDeUsuario.Editor);
        var usuario2 = CrearUsuarioValido("dos@test.com");
        _repo.CrearUsuario(usuario1);
        _repo.CrearUsuario(usuario2);

        var lista = _repo.ObtenerListaUsuarios();

        Assert.AreEqual(2, lista.Count);
        Assert.IsTrue(lista.First(u => u.CorreoElectronico == "uno@test.com").Roles.Contains(RolDeUsuario.Editor));
    }

    [TestMethod]
    public void ObtenerUsuarioPorId_DesdeOtroContexto_CargaRolesPersistidos()
    {
        var usuario = CrearUsuarioValido();
        usuario.AgregarRol(RolDeUsuario.Periodista);
        _repo.CrearUsuario(usuario);

        using var otroContexto = _contextFactory.CreateDbContext();
        var otroRepo = new RepositorioUsuario(otroContexto);

        var obtenido = otroRepo.ObtenerUsuarioPorId(usuario.Id);

        Assert.IsTrue(obtenido.Roles.Contains(RolDeUsuario.Periodista));
    }
}
