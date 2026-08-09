using MetaReal.Application.DTO;
using MetaReal.Application;
using MetaReal.Application.Interfaces;
using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MetaReal.Application.Services
{
    public class MetaService : IMetaService
    {
        private readonly IMetaRealDbContext _context;

        private readonly IUsuarioAtualService _currentUser;

        private readonly INotificador _notificador;

        public MetaService(IMetaRealDbContext context, IUsuarioAtualService currentUser, INotificador notificador)
        {
            _context = context;

            _currentUser = currentUser;
            _notificador = notificador;
        }

        public async Task<MetaProgressoDTO> DefinirMeta(MetaEntradaDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.ValorMeta <= 0)
                throw new ArgumentException("O valor da meta deve ser maior que zero.");
            if (request.Mes < 1 || request.Mes > 12)
                throw new ArgumentException("Mês inválido.");

            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdVendedor == request.VendedorId);
            if (vendedor == null)
                throw new KeyNotFoundException($"Vendedor com ID '{request.VendedorId}' não foi encontrado.");

            var metaExistente = await _context.MetasVenda.FirstOrDefaultAsync(m =>
                m.IdVendedor == request.VendedorId && m.Mes == request.Mes && m.Ano == request.Ano);

            MetaVenda meta;
            if (metaExistente == null)
            {
                meta = new MetaVenda
                {

                    IdMetaVenda = Guid.NewGuid(),
                    IdVendedor = request.VendedorId,
                    Mes = request.Mes,
                    Ano = request.Ano,
                    ValorMeta = request.ValorMeta,
                    CriadoEm = DateTime.UtcNow
                };
                _context.MetasVenda.Add(meta);

            }
            else
            {
                meta = metaExistente;

                meta.ValorMeta = request.ValorMeta;
                meta.AtualizadoEm = DateTime.UtcNow;
                meta.NotificadoConclusao = false;
            }

            await _context.SaveChangesAsync();

            var inicioMes = new DateTime(request.Ano, request.Mes, 1);
            var fimExclusivo = inicioMes.AddMonths(1);
            var valorAtual = await _context.RegistrosVenda.AsNoTracking()
                .Where(r => r.IdVendedor == request.VendedorId && r.Data >= inicioMes && r.Data < fimExclusivo)
                .SumAsync(r => r.ValorTotalVendas);

            var mesReferencia = new DateTime(request.Ano, request.Mes, 1);
            await _notificador.NotificarVendedor(
                request.VendedorId,
                "MetaDefinida",
                $"Sua meta de {mesReferencia:MMMM/yyyy} foi definida: {request.ValorMeta:C}.");

            return new MetaProgressoDTO
            {
                Id = meta.IdMetaVenda,
                VendedorId = meta.IdVendedor,
                VendedorNome = vendedor.Nome,
                Mes = meta.Mes,
                Ano = meta.Ano,
                ValorMeta = meta.ValorMeta,
                ValorAtual = valorAtual
            };
        }

        public async Task<List<MetaProgressoDTO>> DefinirMetaParaTodos(MetaLoteEntradaDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.ValorMeta <= 0)
                throw new ArgumentException("O valor da meta deve ser maior que zero.");
            if (request.Mes < 1 || request.Mes > 12)
                throw new ArgumentException("Mês inválido.");
            var vendedores = await _context.Vendedores.OrderBy(v => v.Nome).ToListAsync();
            if (vendedores.Count == 0)
                return new List<MetaProgressoDTO>();

            var metasExistentes = await _context.MetasVenda
                .Where(m => m.Mes == request.Mes && m.Ano == request.Ano)
                .ToListAsync();

            var metasAplicadas = new Dictionary<Guid, MetaVenda>();
            foreach (var vendedor in vendedores)
            {
                var existente = metasExistentes.FirstOrDefault(m => m.IdVendedor == vendedor.IdVendedor);
                MetaVenda meta;
                if (existente == null)
                {
                    meta = new MetaVenda
                    {

                        IdMetaVenda = Guid.NewGuid(),
                        IdVendedor = vendedor.IdVendedor,
                        Mes = request.Mes,
                        Ano = request.Ano,
                        ValorMeta = request.ValorMeta,
                        CriadoEm = DateTime.UtcNow
                    };
                    _context.MetasVenda.Add(meta);
                }
                else
                {
                    meta = existente;
                    meta.ValorMeta = request.ValorMeta;

                    meta.AtualizadoEm = DateTime.UtcNow;

                    meta.NotificadoConclusao = false;
                }
                metasAplicadas[vendedor.IdVendedor] = meta;
            }

            await _context.SaveChangesAsync();
            var inicioMes = new DateTime(request.Ano, request.Mes, 1);
            var fimExclusivo = inicioMes.AddMonths(1);

            var somas = await _context.RegistrosVenda.AsNoTracking()
                .Where(r => r.Data >= inicioMes && r.Data < fimExclusivo)
                .GroupBy(r => r.IdVendedor)
                .Select(g => new { VendedorId = g.Key, Total = g.Sum(r => r.ValorTotalVendas) })
                .ToDictionaryAsync(x => x.VendedorId, x => x.Total);
            var mesReferencia = new DateTime(request.Ano, request.Mes, 1);
            foreach (var vendedor in vendedores)
            {
                await _notificador.NotificarVendedor(
                    vendedor.IdVendedor,
                    "MetaDefinida",
                    $"Sua meta de {mesReferencia:MMMM/yyyy} foi definida: {request.ValorMeta:C}.");
            }

            return vendedores.Select(v =>
            {
                var meta = metasAplicadas[v.IdVendedor];
                somas.TryGetValue(v.IdVendedor, out var valorAtual);

                return new MetaProgressoDTO
                {

                    Id = meta.IdMetaVenda,
                    VendedorId = v.IdVendedor,
                    VendedorNome = v.Nome,
                    Mes = request.Mes,
                    Ano = request.Ano,
                    ValorMeta = request.ValorMeta,
                    ValorAtual = valorAtual
                };
            }).ToList();
        }
        public async Task<MetaProgressoDTO> ObterProgresso(Guid vendedorId, int? mes, int? ano)
        {
            if (!_currentUser.EhGerente && vendedorId != _currentUser.VendedorId)
                throw new AcessoNegadoException("Você não tem permissão para acessar a meta de outro vendedor.");

            var vendedor = await _context.Vendedores.AsNoTracking().FirstOrDefaultAsync(v => v.IdVendedor == vendedorId);
            if (vendedor == null)
                throw new KeyNotFoundException($"Vendedor com ID '{vendedorId}' não foi encontrado.");
            var mesAlvo = (mes != null && mes.Value >= 1 && mes.Value <= 12) ? mes.Value : DateTime.UtcNow.Month;
            var anoAlvo = (ano != null && ano.Value > 0) ? ano.Value : DateTime.UtcNow.Year;

            var meta = await _context.MetasVenda.AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdVendedor == vendedorId && m.Mes == mesAlvo && m.Ano == anoAlvo);
            var inicioMes = new DateTime(anoAlvo, mesAlvo, 1);
            var fimExclusivo = inicioMes.AddMonths(1);
            var valorAtual = await _context.RegistrosVenda.AsNoTracking()
                .Where(r => r.IdVendedor == vendedorId && r.Data >= inicioMes && r.Data < fimExclusivo)
                .SumAsync(r => r.ValorTotalVendas);

            if (meta != null)
            {
                return new MetaProgressoDTO
                {

                    Id = meta.IdMetaVenda,
                    VendedorId = meta.IdVendedor,
                    VendedorNome = vendedor.Nome,
                    Mes = meta.Mes,
                    Ano = meta.Ano,
                    ValorMeta = meta.ValorMeta,
                    ValorAtual = valorAtual
                };
            }
            return new MetaProgressoDTO
            {
                Id = null,
                VendedorId = vendedorId,
                VendedorNome = vendedor.Nome,
                Mes = mesAlvo,
                Ano = anoAlvo,
                ValorMeta = 0m,
                ValorAtual = valorAtual

            };
        }

        public async Task<List<MetaProgressoDTO>> ObterProgressoGeral(int? mes, int? ano)
        {
            var mesAlvo = (mes != null && mes.Value >= 1 && mes.Value <= 12) ? mes.Value : DateTime.UtcNow.Month;
            var anoAlvo = (ano != null && ano.Value > 0) ? ano.Value : DateTime.UtcNow.Year;

            var vendedores = await _context.Vendedores.AsNoTracking().OrderBy(v => v.Nome).ToListAsync();
            var metas = await _context.MetasVenda.AsNoTracking()
                .Where(m => m.Mes == mesAlvo && m.Ano == anoAlvo)
                .ToListAsync();

            var inicioMes = new DateTime(anoAlvo, mesAlvo, 1);
            var fimExclusivo = inicioMes.AddMonths(1);
            var somasPorVendedor = await _context.RegistrosVenda.AsNoTracking()
                .Where(r => r.Data >= inicioMes && r.Data < fimExclusivo)
                .GroupBy(r => r.IdVendedor)
                .Select(g => new { VendedorId = g.Key, Total = g.Sum(r => r.ValorTotalVendas) })
                .ToDictionaryAsync(x => x.VendedorId, x => x.Total);

            return vendedores.Select(v =>
            {
                var meta = metas.FirstOrDefault(m => m.IdVendedor == v.IdVendedor);
                somasPorVendedor.TryGetValue(v.IdVendedor, out var valorAtual);

                if (meta != null)
                {
                    return new MetaProgressoDTO
                    {
                        Id = meta.IdMetaVenda,
                        VendedorId = meta.IdVendedor,
                        VendedorNome = v.Nome,
                        Mes = meta.Mes,
                        Ano = meta.Ano,
                        ValorMeta = meta.ValorMeta,
                        ValorAtual = valorAtual
                    };
                }

                return new MetaProgressoDTO
                {
                    Id = null,
                    VendedorId = v.IdVendedor,
                    VendedorNome = v.Nome,
                    Mes = mesAlvo,
                    Ano = anoAlvo,
                    ValorMeta = 0m,
                    ValorAtual = valorAtual
                };
            }).ToList();
        }

        public async Task<List<MetaProgressoDTO>> ObterHistorico(Guid vendedorId, int meses)
        {
            if (!_currentUser.EhGerente && vendedorId != _currentUser.VendedorId)
                throw new AcessoNegadoException("Você não tem permissão para acessar a meta de outro vendedor.");

            var vendedor = await _context.Vendedores.AsNoTracking().FirstOrDefaultAsync(v => v.IdVendedor == vendedorId);
            if (vendedor == null)
                throw new KeyNotFoundException($"Vendedor com ID '{vendedorId}' não foi encontrado.");

            var quantidade = meses;
            if (quantidade < 1) quantidade = 1;
            if (quantidade > 24) quantidade = 24;
            var hoje = DateTime.UtcNow;
            var primeiroDoMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
            var inicioIntervalo = primeiroDoMesAtual.AddMonths(-(quantidade - 1));
            var fimIntervaloExclusivo = primeiroDoMesAtual.AddMonths(1);

            var somasPorMes = await _context.RegistrosVenda.AsNoTracking()
                .Where(r => r.IdVendedor == vendedorId && r.Data >= inicioIntervalo && r.Data < fimIntervaloExclusivo)
                .GroupBy(r => new { r.Data.Year, r.Data.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(r => r.ValorTotalVendas) })
                .ToListAsync();
            var metas = await _context.MetasVenda.AsNoTracking()
                .Where(m => m.IdVendedor == vendedorId)
                .ToListAsync();

            var historico = new List<MetaProgressoDTO>();
            for (var i = 0; i < quantidade; i++)
            {

                var referencia = primeiroDoMesAtual.AddMonths(-i);

                var somaDoMes = somasPorMes
                    .FirstOrDefault(s => s.Year == referencia.Year && s.Month == referencia.Month);

                var valorAtual = somaDoMes?.Total ?? 0m;
                var meta = metas.FirstOrDefault(m => m.Mes == referencia.Month && m.Ano == referencia.Year);

                if (meta != null)
                {
                    historico.Add(new MetaProgressoDTO
                    {
                        Id = meta.IdMetaVenda,
                        VendedorId = meta.IdVendedor,
                        VendedorNome = vendedor.Nome,
                        Mes = meta.Mes,
                        Ano = meta.Ano,
                        ValorMeta = meta.ValorMeta,
                        ValorAtual = valorAtual
                    });
                }
                else
                {

                    historico.Add(new MetaProgressoDTO
                    {
                        Id = null,
                        VendedorId = vendedorId,
                        VendedorNome = vendedor.Nome,
                        Mes = referencia.Month,
                        Ano = referencia.Year,
                        ValorMeta = 0m,
                        ValorAtual = valorAtual
                    });
                }
            }

            return historico;

        }

        public async Task VerificarConclusao(Guid vendedorId, DateTime data)
        {


            var meta = await _context.MetasVenda
                .FirstOrDefaultAsync(m => m.IdVendedor == vendedorId && m.Mes == data.Month && m.Ano == data.Year);

            if (meta == null || meta.NotificadoConclusao || meta.ValorMeta <= 0)
                return;

            var inicioMes = new DateTime(data.Year, data.Month, 1);
            var fimExclusivo = inicioMes.AddMonths(1);
            var valorAtual = await _context.RegistrosVenda.AsNoTracking()
                .Where(r => r.IdVendedor == vendedorId && r.Data >= inicioMes && r.Data < fimExclusivo)
                .SumAsync(r => r.ValorTotalVendas);


            if (valorAtual < meta.ValorMeta)
                return;

            meta.NotificadoConclusao = true;
            await _context.SaveChangesAsync();

            var vendedor = await _context.Vendedores.AsNoTracking().FirstOrDefaultAsync(v => v.IdVendedor == vendedorId);
            var nome = vendedor?.Nome ?? "Um vendedor";

            await _notificador.NotificarGerentes(
                "MetaConcluida",
                $"{nome} bateu a meta de {data:MMMM/yyyy}! 🎉");
        }
    }
}
