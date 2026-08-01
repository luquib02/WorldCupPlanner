using System;
using System.Linq;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dominio.Tests;

[TestClass]
public class PruebasDeUsuario
{
    // Note 2: Variables privadas de instancia reutilizadas en todos los tests (Test Fixtures)
    // Esto permite compartir datos de configuración sin duplicar código entre pruebas
    private string _nombre;
    private string _apellido;
    private string _correoElectronico;
    private DateOnly _fechaNacimiento;
    private string _hashDeContrasenia;

    // Note 3: [TestInitialize] ejecuta este método antes de CADA test individual
    // Garantiza un estado limpio (freshness) para cada prueba evitando efectos secundarios
    [TestInitialize]
    public void ConfiguracionInicial()
    {
        // Note 4: Inicializar datos válidos en la configuración permite a otros tests
        // simplemente usar valores válidos sin tener que replantearlos en cada método
        _nombre = "Ana";
        _apellido = "Perez";
        _correoElectronico = "ana.perez@mail.com";
        _fechaNacimiento = new DateOnly(1995, 12, 15);
        // Note 4b: Contraseña que cumple todos los requisitos: 8+ caracteres, mayúscula, minúscula, número, especial
        _hashDeContrasenia = "SecurePass123@";
    }
    
    // Note 5: Test de "happy path" o caso exitoso (prueba que el código funciona correctamente)
    // Verifica que se pueden asignar datos válidos y recuperarlos sin errores
    [TestMethod]
    public void CrearUsuario_Y_Verificar_DatosValidos()
    {
        Usuario usuario = new Usuario();
        usuario.Nombre = _nombre;
        usuario.Apellido = _apellido;
        usuario.CorreoElectronico = _correoElectronico;
        usuario.FechaNacimiento = _fechaNacimiento;
        usuario.EstablecerContrasenia(_hashDeContrasenia);
        usuario.AgregarRol(RolDeUsuario.Editor);
        //Usuario.AsignarId(usuario);
        
        // Note 6: Assert (afirmación) verifica que el comportamiento coincida con lo esperado
        // Si alguna condición es falsa, el test falla inmediatamente
        Assert.IsTrue(usuario.Id >= 0);
        Assert.AreEqual(_nombre, usuario.Nombre);
        Assert.AreEqual(_apellido, usuario.Apellido);
        Assert.AreEqual(_correoElectronico, usuario.CorreoElectronico);
        Assert.AreEqual(_fechaNacimiento, usuario.FechaNacimiento);
        // Note 23: Verificamos que la contraseña se puede verificar correctamente con BCrypt
        Assert.IsTrue(usuario.VerificarContrasenia(_hashDeContrasenia));
        Assert.AreEqual(RolDeUsuario.Editor, usuario.Roles.ElementAt(0));
    }

    // Tests para validación de Nombre
    // Note 7: Assert.Throws<T>() verifica que el código lanza la excepción correcta
    // El lambda se ejecuta y si lanza la excepción esperada, el test pasa
    [TestMethod]
    public void NombreVacio_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        // Note 8: Assert.Throws retorna la excepción, permitiendo verificar su mensaje
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.Nombre = "");
        Assert.AreEqual("El nombre no puede estar vacio", ex.Message);
    }

    [TestMethod]
    public void NombreConEspacios_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        // Note 9: Si solo queremos verificar que se lanzó la excepción, sin verificar el mensaje
        Assert.Throws<ExcepcionUsuario>(() => usuario.Nombre = "   ");
    }

    // Tests para validación de Apellido
    // Note 10: Patrón repetido para Apellido: esto ilustra test coverage consistente
    // Varios inputs inválidos (vacío, espacios) prueban robustez de la validación
    [TestMethod]
    public void ApellidoVacio_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.Apellido = "");
        Assert.AreEqual("El apellido no puede estar vacio", ex.Message);
    }

    [TestMethod]
    public void ApellidoConEspacios_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        Assert.Throws<ExcepcionUsuario>(() => usuario.Apellido = "   ");
    }

    // Tests para validación de Correo Electrónico
    // Note 11: Validación de email requiere múltiples casos: sin @, sin dominio, solo @
    // Esto es "boundary testing": probar los límites de lo válido vs inválido
    [TestMethod]
    public void CorreoInvalido_SinArroba_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.CorreoElectronico = "correo-sin-arroba.com");
        Assert.AreEqual("El correo electrónico es inválido", ex.Message);
    }

    [TestMethod]
    public void CorreoInvalido_SinDominio_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        Assert.Throws<ExcepcionUsuario>(() => usuario.CorreoElectronico = "correo@");
    }

    [TestMethod]
    public void CorreoInvalido_SoloArroba_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        Assert.Throws<ExcepcionUsuario>(() => usuario.CorreoElectronico = "@dominio.com");
    }

    // Note 12: Test de caso válido: asegura que emails correctos SÍ se acepten
    // Esto previene falsos positivos donde se rechazarían todos los emails
    [TestMethod]
    public void CorreoValido_SeAsignaCorrectamente()
    {
        Usuario usuario = new Usuario();
        usuario.CorreoElectronico = _correoElectronico;
        
        Assert.AreEqual(_correoElectronico, usuario.CorreoElectronico);
    }

    // Tests para validación de Fecha de Nacimiento
    // Note 13: Validar fechas es crítico: futuras no tiene sentido, hoy es válido, pasadas también
    // El test FechaNacimientoFutura verifica la lógica de negocio: "no puede ser futuro"
    [TestMethod]
    public void FechaNacimientoFutura_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => 
            usuario.FechaNacimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
        Assert.AreEqual("La fecha de nacimiento no puede ser en el futuro", ex.Message);
    }

    // Note 14: DateOnly.FromDateTime() convierte DateTime a DateOnly (sin hora)
    // Esto es importante porque nacimiento es un concepto de "día", no de "momento exacto"
    [TestMethod]
    public void FechaNacimientoHoy_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.FechaNacimiento = DateOnly.FromDateTime(DateTime.Now);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now), usuario.FechaNacimiento);
    }

    [TestMethod]
    public void FechaNacimientoPasada_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.FechaNacimiento = _fechaNacimiento;
        Assert.AreEqual(_fechaNacimiento, usuario.FechaNacimiento);
    }

    #region Pruebas de roles

    // Tests para validación de Roles - AgregarRol
    // Note 15: Roles son una relación de colección: un usuario puede tener múltiples roles
    // Los tests verifican que se agreguen, se verifiquen, y no se dupliquen
    [TestMethod]
    public void AgregarRol_YVerificarQueExiste()
    {
        Usuario usuario = new Usuario();
        usuario.AgregarRol(RolDeUsuario.Editor);
        
        // Note 16: Usar .Contains() en la colección de Roles (que es ReadOnly)
        // Verifica que el rol se agregó correctamente sin duplicados
        Assert.IsTrue(usuario.Roles.Contains(RolDeUsuario.Editor));
    }

    // Note 17: Test de duplicados: agregar el mismo rol dos veces debe lanzar excepción
    // Esto previene lógica incorrecta donde los roles se duplicarían sin límite
    [TestMethod]
    public void AgregarRolDuplicado_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.AgregarRol(RolDeUsuario.Editor);
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.AgregarRol(RolDeUsuario.Editor));
        Assert.AreEqual("El rol ya existe en la lista de roles del usuario", ex.Message);
    }

    [TestMethod]
    public void AgregarVariosRoles_VerificaMultiples()
    {
        Usuario usuario = new Usuario();
        usuario.AgregarRol(RolDeUsuario.Editor);
        usuario.AgregarRol(RolDeUsuario.AdministradorDelSistema);
        
        // Note 18: Assert.AreEqual(count, ...) verifica que ambos roles se agregaron (count = 2)
        // Luego verificamos individualmente que cada rol esté presente (.Contains)
        Assert.AreEqual(2, usuario.Roles.Count);
        Assert.IsTrue(usuario.Roles.Contains(RolDeUsuario.Editor));
        Assert.IsTrue(usuario.Roles.Contains(RolDeUsuario.AdministradorDelSistema));
    }

    // Tests para validación de Roles - QuitarRol
    // Note 19: Quitar un rol que no existe debe lanzar excepción (no es operación silenciosa)
    // Esto previene bugs donde la lógica esperaría que el rol fue removido pero no lo fue
    [TestMethod]
    public void QuitarRolQueNoExiste_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.QuitarRol(RolDeUsuario.Editor));
        Assert.AreEqual("El rol no existe en la lista de roles del usuario", ex.Message);
    }

    [TestMethod]
    public void QuitarRolExistente_LoElimina()
    {
        Usuario usuario = new Usuario();
        usuario.AgregarRol(RolDeUsuario.Editor);
        usuario.QuitarRol(RolDeUsuario.Editor);
        
        // Note 20: Verificar dos cosas: (1) que el rol no está (.IsFalse), (2) que count = 0
        // Esto confirma que la operación fue exitosa completamente, no parcial
        Assert.IsFalse(usuario.Roles.Contains(RolDeUsuario.Editor));
        Assert.AreEqual(0, usuario.Roles.Count);
    }
    
    [TestMethod]
    public void QuitarUnRolDeMultiples_MantieneOtros()
    {
        Usuario usuario = new Usuario();
        usuario.AgregarRol(RolDeUsuario.Editor);
        usuario.AgregarRol(RolDeUsuario.AdministradorDelSistema);
        usuario.QuitarRol(RolDeUsuario.Editor);
        
        Assert.IsFalse(usuario.Roles.Contains(RolDeUsuario.Editor));
        Assert.IsTrue(usuario.Roles.Contains(RolDeUsuario.AdministradorDelSistema));
        Assert.AreEqual(1, usuario.Roles.Count);
    }

    [TestMethod]
    public void AgregarRolPeriodista()
    {
        Usuario usuario = new Usuario();
        usuario.AgregarRol(RolDeUsuario.Periodista);
        
        Assert.IsTrue(usuario.Roles.Contains(RolDeUsuario.Periodista));
    }

    #endregion
    
    
    [TestMethod]
    public void EstablecerContrasenia_YVerificar_Exitoso()
    {
        Usuario usuario = new Usuario();
        // Note 25: EstablecerContrasenia hashea la contraseña internamente con BCrypt
        // La contraseña debe cumplir todos los requisitos: 8+, mayúscula, minúscula, número, especial
        usuario.EstablecerContrasenia("SecurePass123@");
        
        // Note 26: VerificarContrasenia compara la contraseña en texto plano con el hash BCrypt
        Assert.IsTrue(usuario.VerificarContrasenia("SecurePass123@"));
    }

    [TestMethod]
    public void VerificarContraseniaIncorrecta_RetornaFalso()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Correct123@Pass");
        
        // Note 27: Si la contraseña no coincide, VerificarContrasenia retorna false
        Assert.IsFalse(usuario.VerificarContrasenia("Incorrect123@Pass"));
    }

    [TestMethod]
    public void EstablecerContraseniaVacia_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.EstablecerContrasenia(""));
        Assert.AreEqual("La contraseña no puede estar vacía", ex.Message);
    }

    [TestMethod]
    public void EstablecerContraseniaConEspacios_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        Assert.Throws<ExcepcionUsuario>(() => usuario.EstablecerContrasenia("   "));
    }

    [TestMethod]
    public void VerificarContraseniaVacia_RetornaFalso()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid123@Pass");
        
        // Note 29: Intentar verificar con una contraseña vacía siempre retorna false
        // Esto previene bypasses de seguridad
        Assert.IsFalse(usuario.VerificarContrasenia(""));
    }

    [TestMethod]
    public void DosContraseniasDiferentes_GeneranHashesDiferentes()
    {
        Usuario usuario1 = new Usuario();
        Usuario usuario2 = new Usuario();
        
        usuario1.EstablecerContrasenia("SecurePass123@");
        usuario2.EstablecerContrasenia("SecurePass123@");
        
        Assert.AreNotEqual(usuario1.HashDeContrasena, usuario2.HashDeContrasena);
        Assert.IsTrue(usuario1.VerificarContrasenia("SecurePass123@"));
        Assert.IsTrue(usuario2.VerificarContrasenia("SecurePass123@"));
    }

    // Tests para validación de requisitos de contraseña
    // Note 37: Validar cada requisito por separado es importante para debugging
    // Si un test falla, se sabe exactamente cuál requisito no cumple
    [TestMethod]
    public void ContraseniaMenorA8Caracteres_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.EstablecerContrasenia("Pass12@"));
        Assert.AreEqual("La contraseña debe tener al menos 8 caracteres", ex.Message);
    }

    [TestMethod]
    public void ContraseniaExactamente8Caracteres_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        // Note 38: 8 caracteres es el límite inferior aceptable
        usuario.EstablecerContrasenia("Pass12@a");
        Assert.IsTrue(usuario.VerificarContrasenia("Pass12@a"));
    }

    [TestMethod]
    public void ContraseniaSinMayuscula_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.EstablecerContrasenia("lowercase123@"));
        Assert.AreEqual("La contraseña debe incluir al menos una letra mayúscula (A-Z)", ex.Message);
    }

    [TestMethod]
    public void ContraseniaSinMinuscula_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.EstablecerContrasenia("UPPERCASE123@"));
        Assert.AreEqual("La contraseña debe incluir al menos una letra minúscula (a-z)", ex.Message);
    }

    [TestMethod]
    public void ContraseniaSinNumero_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.EstablecerContrasenia("NoNumeros@abc"));
        Assert.AreEqual("La contraseña debe incluir al menos un número (0-9)", ex.Message);
    }

    [TestMethod]
    public void ContraseniaSinCaracterEspecial_LanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        var ex = Assert.Throws<ExcepcionUsuario>(() => usuario.EstablecerContrasenia("NoSpecial123"));
        Assert.AreEqual("La contraseña debe incluir al menos un carácter especial (ej: @, #, $, %, !, *, +, etc.)", ex.Message);
    }

    [TestMethod]
    public void ContraseniaConTodosRequisitos_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        // Note 39: Contraseña que cumple: 8+, mayúscula, minúscula, número, especial
        usuario.EstablecerContrasenia("ValidPass123#");
        Assert.IsTrue(usuario.VerificarContrasenia("ValidPass123#"));
    }

    [TestMethod]
    public void ContraseniaValidaConMultiplesEspeciales_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        // Note 40: Acepta múltiples caracteres especiales
        usuario.EstablecerContrasenia("Strong$Pass99#");
        Assert.IsTrue(usuario.VerificarContrasenia("Strong$Pass99#"));
    }

    [TestMethod]
    public void ContraseniaValidaConArroba_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid@Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid@Pass123"));
    }

    [TestMethod]
    public void ContraseniaValidaConAlmohadilla_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid#Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid#Pass123"));
    }

    [TestMethod]
    public void ContraseniaValidaConDolar_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid$Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid$Pass123"));
    }

    [TestMethod]
    public void ContraseniaValidaConPorcentaje_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid%Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid%Pass123"));
    }

    [TestMethod]
    public void ContraseniaValidaConPunto_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid.Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid.Pass123"));
    }

    [TestMethod]
    public void ContraseniaValidaConExclamacion_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid!Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid!Pass123"));
    }

    [TestMethod]
    public void ContraseniaValidaConAsterisco_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid*Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid*Pass123"));
    }

    [TestMethod]
    public void ContraseniaValidaConMas_NoLanzaExcepcion()
    {
        Usuario usuario = new Usuario();
        usuario.EstablecerContrasenia("Valid+Pass123");
        Assert.IsTrue(usuario.VerificarContrasenia("Valid+Pass123"));
    }
    
}
