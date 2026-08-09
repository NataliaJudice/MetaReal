using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;

using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MetaReal.Application.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly IMetaRealDbContext _context;

        private readonly IUsuarioAtualService _currentUser;
        public AuditoriaService(IMetaRealDbContext context, IUsuarioAtualService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }
        public async Task Registrar(string acao, string entidade, string? idEntidade, string? detalhes = null)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                IdAuditLog = Guid.NewGuid(),
                IdUsuario = _currentUser.UsuarioId,
                UsuarioNome = _currentUser.Nome,
                Acao = acao,
                Entidade = entidade,
                IdEntidade = idEntidade,
                DataHora = DateTime.UtcNow,
                Detalhes = detalhes,
                Ip = _currentUser.Ip
            });

            await _context.SaveChangesAsync();
        }


        public async Task<AuditoriaPaginadaDTO> ObterPaginado(int pagina, int tamanhoPagina, string? entidade = null)
        {
            var paginaAtual = pagina;
            if (paginaAtual < 1)
            {
                paginaAtual = 1;

            }

            var tamanho = tamanhoPagina;
            if (tamanho < 1)
            {
                tamanho = 20;
            }

            var query = _context.AuditLogs.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(entidade))
            {

                query = query.Where(a => a.Entidade == entidade);
            }

            query = query.OrderByDescending(a => a.DataHora);

            var total = await query.CountAsync();
            var items = await query
                .Skip((paginaAtual - 1) * tamanho)
                .Take(tamanho)
                .Select(a => new AuditoriaDTO
                {
                    Id = a.IdAuditLog,
                    UsuarioNome = a.UsuarioNome,
                    Acao = a.Acao,
                    Entidade = a.Entidade,
                    IdEntidade = a.IdEntidade,
                    DataHora = a.DataHora,
                    Detalhes = a.Detalhes,
                    Ip = a.Ip
                })
                .ToListAsync();

            var totalPaginas = (int)Math.Ceiling(total / (double)tamanho);
            if (totalPaginas == 0)
            {
                totalPaginas = 1;
            }

            var resultado = new AuditoriaPaginadaDTO();
            resultado.TotalRegistros = total;
            resultado.PaginaAtual = paginaAtual;
            resultado.TotalPaginas = totalPaginas;
            resultado.Items = items;

            return resultado;
        }
    }
}
