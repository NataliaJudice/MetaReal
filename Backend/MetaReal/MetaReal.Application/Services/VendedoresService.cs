using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;
using MetaReal.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace MetaReal.Application.Services
{
    public class VendedoresService : IVendedoresService
    {
        private readonly IMetaRealDbContext _context;
        private readonly IAuditoriaService _auditService;

        public VendedoresService(IMetaRealDbContext context, IAuditoriaService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IEnumerable<VendedorDTO>> ObterTodos()
        {
            var vendedores = await _context.Vendedores
                .AsNoTracking()
                .OrderBy(v => v.Nome)
                .ToListAsync();

            return vendedores.Select(v => new VendedorDTO
            {
                Id = v.IdVendedor,
                Nome = v.Nome
            });
        }

        public async Task<VendedorDTO?> ObterPorId(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("O ID informado não pode ser vazio.", nameof(id));
            }

            var vendedor = await _context.Vendedores
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVendedor == id);

            if (vendedor == null)
            {
                return null;
            }

            return new VendedorDTO
            {
                Id = vendedor.IdVendedor,
                Nome = vendedor.Nome
            };

        }

        public async Task<VendedorDTO> Adicionar(VendedorEntradaDTO request)
        {
            if (request == null)
            {

                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                throw new ArgumentException("O nome do vendedor é obrigatório.");

            }
            var vendedor = new Vendedor
            {
                IdVendedor = Guid.NewGuid(),
                Nome = request.Nome.Trim()
            };

            await using var transacao = await _context.BeginTransactionAsync();
            try
            {
                _context.Vendedores.Add(vendedor);
                await _context.SaveChangesAsync();

                await _auditService.Registrar("Vendedor.Criado", "Vendedor", vendedor.IdVendedor.ToString(), $"Nome={vendedor.Nome}");

                await transacao.CommitAsync();
                return new VendedorDTO
                {
                    Id = vendedor.IdVendedor,
                    Nome = vendedor.Nome
                };


            }
            catch
            {

                await transacao.RollbackAsync();
                throw;
            }
        }

        public async Task<VendedorDTO> Editar(Guid id, VendedorEntradaDTO request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                throw new ArgumentException("O nome do vendedor é obrigatório.");
            }

            await using var transacao = await _context.BeginTransactionAsync();
            try
            {
                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdVendedor == id);
                if (vendedor == null)
                {
                    throw new KeyNotFoundException($"Vendedor com ID '{id}' não foi encontrado.");
                }

                var nomeAnterior = vendedor.Nome;
                vendedor.Nome = request.Nome.Trim();
                await _context.SaveChangesAsync();

                await _auditService.Registrar("Vendedor.Editado", "Vendedor", vendedor.IdVendedor.ToString(), $"NomeAnterior={nomeAnterior}; NomeNovo={vendedor.Nome}");

                await transacao.CommitAsync();

                return new VendedorDTO
                {
                    Id = vendedor.IdVendedor,
                    Nome = vendedor.Nome
                };
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }
        public async Task Deletar(Guid id)
        {

            if (id == Guid.Empty)
            {
                throw new ArgumentException("O ID informado não pode ser vazio.", nameof(id));
            }

            await using var transacao = await _context.BeginTransactionAsync();
            try
            {
                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdVendedor == id);
                if (vendedor == null)
                {
                    throw new KeyNotFoundException($"Vendedor com ID '{id}' não foi encontrado.");
                }
                var usuariosVinculados = await _context.Usuarios.Where(u => u.IdVendedor == id).ToListAsync();
                foreach (var usuario in usuariosVinculados)
                {
                    usuario.Ativo = false;
                    usuario.IdVendedor = null;
                }

                var nome = vendedor.Nome;
                _context.Vendedores.Remove(vendedor);
                await _context.SaveChangesAsync();


                await _auditService.Registrar("Vendedor.Excluido", "Vendedor", id.ToString(), $"Nome={nome}");

                await transacao.CommitAsync();
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }
    }
}
