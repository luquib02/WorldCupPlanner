using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Apellido).IsRequired().HasMaxLength(200);
        builder.Property(u => u.CorreoElectronico).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FechaNacimiento).IsRequired();
        builder.Property(u => u.HashDeContrasena).IsRequired();
        builder.Ignore(u => u.Roles);
        builder.HasIndex(u => u.CorreoElectronico).IsUnique();
    }
}
