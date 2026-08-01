using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class LogEntryConfiguration : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("Logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();

        builder.Property(l => l.Timestamp).IsRequired();

        builder.Property(l => l.UsuarioEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.Accion)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Detalle)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(l => l.Timestamp);
    }
}