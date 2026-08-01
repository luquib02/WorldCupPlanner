using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRolRecord>
{
    public void Configure(EntityTypeBuilder<UsuarioRolRecord> builder)
    {
        builder.ToTable("UsuarioRoles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.Rol).IsRequired().HasMaxLength(100);
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => new { r.UsuarioId, r.Rol }).IsUnique();
    }
}
