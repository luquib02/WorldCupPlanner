using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class JornadaConfiguration : IEntityTypeConfiguration<Jornada>
{
    public void Configure(EntityTypeBuilder<Jornada> builder)
    {
        builder.ToTable("Jornadas");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).ValueGeneratedOnAdd();

        builder.Property(j => j.Numero).IsRequired();
        builder.Property(j => j.Fecha).IsRequired();

        builder.HasMany(j => j.Partidos)
            .WithOne()
            .HasForeignKey("JornadaId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
