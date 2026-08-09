using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetaReal.Infra.Data.Mappings
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(x => x.IdUsuario);

            builder.Property(x => x.Nome).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
            builder.Property(x => x.SenhaHash).IsRequired();
            builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(x => x.Email).IsUnique();

            builder.HasOne(x => x.Vendedor)
                .WithMany()
                .HasForeignKey(x => x.IdVendedor)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
