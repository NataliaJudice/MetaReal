using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MetaReal.Application.Interfaces
{
    public interface IMetaRealDbContext
    {
        DbSet<Vendedor> Vendedores { get; }

        DbSet<RegistroVenda> RegistrosVenda { get; }
        DbSet<Usuario> Usuarios { get; }

        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<MetaVenda> MetasVenda { get; }



        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
