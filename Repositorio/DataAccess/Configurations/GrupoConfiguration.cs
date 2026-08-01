using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class GrupoConfiguration : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> builder)
    {
        builder.ToTable("Grupos");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedOnAdd();

        builder.Property(g => g.Etiqueta)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasIndex("FixtureId", "Etiqueta").IsUnique();

        builder.HasMany(g => g.Equipos)
            .WithMany()
            .UsingEntity(j =>
            {
                j.ToTable("GrupoEquipos");
                j.HasIndex("EquiposId").IsUnique();
            });

        builder.HasMany(g => g.Jornadas)
            .WithOne()
            .HasForeignKey("GrupoId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
