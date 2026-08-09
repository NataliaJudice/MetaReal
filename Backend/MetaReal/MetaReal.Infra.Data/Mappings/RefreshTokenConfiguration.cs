using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetaReal.Infra.Data.Mappings
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.IdRefreshToken);

            builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(200);
            builder.HasIndex(x => x.TokenHash).IsUnique();

            builder.HasOne(x => x.Usuario)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(x => x.EstaAtivo);
        }
    }
}
