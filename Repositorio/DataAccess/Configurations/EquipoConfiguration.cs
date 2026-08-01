using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class EquipoConfiguration : IEntityTypeConfiguration<Equipo>
{
    public void Configure(EntityTypeBuilder<Equipo> builder)
    {
        builder.ToTable("Equipos");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(e => e.Confederacion)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.RankingFIFA).IsRequired();
        builder.Property(e => e.RankingActual).IsRequired();

        builder.Property(e => e.BanderaBase64)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.HasIndex(e => e.Nombre).IsUnique();
    }
}