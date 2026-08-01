using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class PartidoConfiguration : IEntityTypeConfiguration<Partido>
{
    public void Configure(EntityTypeBuilder<Partido> builder)
    {
        builder.ToTable("Partidos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Fecha).IsRequired();

        builder.Property(p => p.Grupo)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(p => p.Fase)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.EtiquetaCruce)
            .HasMaxLength(10)
            .IsRequired(false);
        
        builder.Property(p => p.GolesLocal).IsRequired(false);
        builder.Property(p => p.GolesVisitante).IsRequired(false);
        builder.Property(p => p.GolesPenalesLocal).IsRequired(false);
        builder.Property(p => p.GolesPenalesVisitante).IsRequired(false);
        
        builder.HasOne(p => p.Estadio)
            .WithMany()
            .HasForeignKey("EstadioId")
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.EquipoLocal)
            .WithMany()
            .HasForeignKey("EquipoLocalId")
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.EquipoVisitante)
            .WithMany()
            .HasForeignKey("EquipoVisitanteId")
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.PartidoOrigenLocal)
            .WithMany()
            .HasForeignKey("PartidoOrigenLocalId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.PartidoOrigenVisitante)
            .WithMany()
            .HasForeignKey("PartidoOrigenVisitanteId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(p => p.IncidenciasLocal);
        builder.Ignore(p => p.IncidenciasVisitante);
    }
}