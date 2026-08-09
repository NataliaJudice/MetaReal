using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetaReal.Infra.Data.Mappings
{
    public class VendedorConfiguration : IEntityTypeConfiguration<Vendedor>
    {
        public void Configure(EntityTypeBuilder<Vendedor> builder)
        {
            builder.HasKey(x => x.IdVendedor);

            builder.Property(x => x.Nome)
                .IsRequired()
                .HasMaxLength(120);
        }
    }
}
