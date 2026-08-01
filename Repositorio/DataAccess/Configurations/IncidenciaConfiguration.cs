using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class IncidenciaConfiguration : IEntityTypeConfiguration<Incidencia>
{
    public void Configure(EntityTypeBuilder<Incidencia> builder)
    {
        builder.ToTable("Incidencias");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.MinutoJuego).IsRequired();
        builder.Property(i => i.EsLocal).IsRequired();

        builder.HasDiscriminator<string>("Tipo")
            .HasValue<TarjetaAmarilla>("TarjetaAmarilla")
            .HasValue<TarjetaRoja>("TarjetaRoja");

        builder.HasOne<Partido>()
            .WithMany(p => p.Incidencias)
            .HasForeignKey("PartidoId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
