using Ejecutable.Components;
using Dominio;
using Repositorio;
using Sistema;
using Sistema.Interfaces;
using Sistema.Servicios;
using Sistema.Importacion;
using Sistema.Importacion.FacilidadComprension;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<WorldCupPlannerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IFixtureRepositorio, FixtureRepositorio>();
builder.Services.AddScoped<IImportacionServicio, ImportacionServicioLegible>();

builder.Services.AddScoped<IRepositorioEstadio, RepositorioEstadio>();
builder.Services.AddScoped<IEquipoRepositorio, EquipoRepositorio>();
builder.Services.AddScoped<IRepositorioLog, RepositorioLog>();
builder.Services.AddScoped<IPartidoRepositorio, RepositorioPartido>();
builder.Services.AddScoped<IRepositorioNotificacion, NotificacionRepositorio>();


builder.Services.AddSingleton<IServicioSesion, ServicioSesion>();

builder.Services.AddScoped<LogServicio>();
builder.Services.AddScoped<IServicioLog>(sp => sp.GetRequiredService<LogServicio>());

builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();

builder.Services.AddSingleton<CalculadoraRankingElo>();
builder.Services.AddScoped<ResultadoPartidoServicio>();
builder.Services.AddScoped<EstadioServicio>();
builder.Services.AddScoped<EquipoServicio>();
builder.Services.AddScoped<PartidoServicio>();
builder.Services.AddScoped<FixtureServicio>();
builder.Services.AddScoped<CrucesServicio>();
builder.Services.AddScoped<SimulacionServicio>();
builder.Services.AddScoped<IExportacionServicio, ExportacionServicio>();
builder.Services.AddScoped<NotificacionServicio>();
builder.Services.AddScoped<IServicioRoles, ServicioRoles>();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    WorldCupPlannerDbContext dbContext = scope.ServiceProvider
        .GetRequiredService<WorldCupPlannerDbContext>();
    dbContext.Database.Migrate();
}
    
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IRepositorioUsuario>();
    var todos = repo.ObtenerListaUsuarios();
    bool adminExiste = todos.Any(u => u.Roles.Contains(RolDeUsuario.AdministradorDelSistema));
    if (!adminExiste)
    {
        var admin = new Usuario();
        admin.Nombre = "Admin";
        admin.Apellido = "Sistema";
        admin.CorreoElectronico = "admin@sistema.com";
        admin.FechaNacimiento = new DateOnly(2000, 1, 1);
        admin.EstablecerContrasenia("Admin123!@");
        admin.AgregarRol(RolDeUsuario.AdministradorDelSistema);
        Console.WriteLine($"Id antes de guardar: {admin.Id}");  
        repo.CrearUsuario(admin);

        var adminEditor = new Usuario();
        adminEditor.Nombre = "Editor";
        adminEditor.Apellido = "Sistema";
        adminEditor.CorreoElectronico = "editor@sistema.com";
        adminEditor.FechaNacimiento = new DateOnly(2000, 1, 1);
        adminEditor.EstablecerContrasenia("Edit123!@");
        adminEditor.AgregarRol(RolDeUsuario.Editor);
        repo.CrearUsuario(adminEditor);
        
        var periodista = new Usuario();
        periodista.Nombre = "Periodista";
        periodista.Apellido = "Sistema";
        periodista.CorreoElectronico = "periodista@sistema.com";
        periodista.FechaNacimiento = new DateOnly(2000, 1, 1);
        periodista.EstablecerContrasenia("Periodista123!@");
        periodista.AgregarRol(RolDeUsuario.Periodista);
        repo.CrearUsuario(periodista);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();