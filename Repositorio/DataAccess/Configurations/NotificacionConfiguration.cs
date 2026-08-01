using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedOnAdd();
        builder.Property(n => n.Mensaje).IsRequired().HasMaxLength(500);
        builder.Property(n => n.Timestamp).IsRequired();
        builder.Property(n => n.PartidoId).IsRequired();
        builder.Property(n => n.DestinatarioId).IsRequired();
        builder.Property(n => n.Leida).IsRequired();
        builder.HasOne<Partido>()
            .WithMany()
            .HasForeignKey(n => n.PartidoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(n => n.DestinatarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
