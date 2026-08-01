using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class FixtureConfiguration : IEntityTypeConfiguration<Fixture>
{
    public void Configure(EntityTypeBuilder<Fixture> builder)
    {
        builder.ToTable("Fixtures");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();

        builder.Property(f => f.SemillaUtilizada).IsRequired();
        builder.Property(f => f.FechaGeneracion).IsRequired();
        builder.Property(f => f.CrucesGenerados).IsRequired();
        builder.Property(f => f.NombreMotorSimulacion)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(f => f.Grupos)
            .WithOne()
            .HasForeignKey("FixtureId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(f => f.Grupos).HasField("_grupos");

        builder.HasMany(f => f.PartidosEliminatorias)
            .WithOne()
            .HasForeignKey("FixtureEliminatoriasId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Navigation(f => f.PartidosEliminatorias).HasField("_partidosEliminatorias");
    }
}
