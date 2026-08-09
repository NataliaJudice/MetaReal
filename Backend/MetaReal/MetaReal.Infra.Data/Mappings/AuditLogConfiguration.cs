using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetaReal.Infra.Data.Mappings
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(x => x.IdAuditLog);

            builder.Property(x => x.Acao).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Entidade).IsRequired().HasMaxLength(100);
            builder.Property(x => x.IdEntidade).HasMaxLength(64);
            builder.Property(x => x.Detalhes).HasMaxLength(1000);
            builder.Property(x => x.Ip).HasMaxLength(64);

            builder.HasIndex(x => x.DataHora);
        }
    }
}
