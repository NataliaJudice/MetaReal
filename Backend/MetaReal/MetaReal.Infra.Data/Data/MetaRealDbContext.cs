using MetaReal.Application.Interfaces;
using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MetaReal.Infra.Data.Data
{
    public class MetaRealDbContext : DbContext, IMetaRealDbContext
    {
        public MetaRealDbContext(DbContextOptions<MetaRealDbContext> options)
           : base(options)
        { }

        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<RegistroVenda> RegistrosVenda { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MetaVenda> MetasVenda { get; set; }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            Database.BeginTransactionAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetaRealDbContext).Assembly);
        }
    }
}
