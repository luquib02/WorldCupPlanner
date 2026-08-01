using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class EstadioConfiguration : IEntityTypeConfiguration<Estadio>
{
    public void Configure(EntityTypeBuilder<Estadio> builder)
    {
        // configuración de Estadio
    }
}