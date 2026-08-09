using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetaReal.Infra.Data.Mappings
{
    public class RegistroVendaConfiguration : IEntityTypeConfiguration<RegistroVenda>
    {
        public void Configure(EntityTypeBuilder<RegistroVenda> builder)
        {
            builder.HasKey(x => x.IdRegistroVenda);

            builder.HasIndex(x => new { x.IdVendedor, x.Data }).IsUnique();

            builder.HasOne(x => x.Vendedor)
                .WithMany(x => x.RegistrosVenda)
                .HasForeignKey(x => x.IdVendedor)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.PretasMistas).HasPrecision(18, 2);
            builder.Property(x => x.Garantia).HasPrecision(18, 2);
            builder.Property(x => x.CrediarioDujuca).HasPrecision(18, 2);
            builder.Property(x => x.ValorTotalVendas).HasPrecision(18, 2);
        }
    }
}
