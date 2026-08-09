using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetaReal.Infra.Data.Mappings
{
    public class MetaVendaConfiguration : IEntityTypeConfiguration<MetaVenda>
    {
        public void Configure(EntityTypeBuilder<MetaVenda> builder)
        {
            builder.HasKey(x => x.IdMetaVenda);

            builder.Property(x => x.ValorMeta).HasPrecision(18, 2);

            builder.HasIndex(x => new { x.IdVendedor, x.Mes, x.Ano }).IsUnique();

            builder.HasOne(x => x.Vendedor)
                .WithMany()
                .HasForeignKey(x => x.IdVendedor)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
