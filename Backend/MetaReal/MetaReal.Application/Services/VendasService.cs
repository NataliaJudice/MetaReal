using MetaReal.Application.DTO;
using MetaReal.Application;
using MetaReal.Application.Interfaces;
using MetaReal.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MetaReal.Application.Services
{

    public class VendasService : IVendasService
    {
        private readonly IMetaRealDbContext _context;
        private readonly IUsuarioAtualService _currentUser;

        private readonly IAuditoriaService _auditService;
        private readonly IMetaService _metaService;

        public VendasService(
            IMetaRealDbContext context,
            IUsuarioAtualService currentUser,
            IAuditoriaService auditService,
            IMetaService metaService)
        {
            _context = context;
            _currentUser = currentUser;

            _auditService = auditService;
            _metaService = metaService;
        }

        public async Task<RegistroVendaPaginadoDTO> ObterTodos(
            Guid? vendedorId,
            DateTime? dataInicio,
            DateTime? dataFim,
            int pagina,
            int tamanhoPagina)
        {
            if (!_currentUser.EhGerente)
            {
                vendedorId = _currentUser.VendedorId ?? Guid.Empty;
            }


            var paginaAtual = pagina < 1 ? 1 : pagina;
            var tamanho = tamanhoPagina < 1 ? 10 : tamanhoPagina;
            var queryFiltrada = _context.RegistrosVenda.AsNoTracking().AsQueryable();
            if (vendedorId.HasValue && vendedorId.Value != Guid.Empty)
                queryFiltrada = queryFiltrada.Where(r => r.IdVendedor == vendedorId.Value);
            if (dataInicio.HasValue)
                queryFiltrada = queryFiltrada.Where(r => r.Data >= dataInicio.Value.Date);
            if (dataFim.HasValue)
                queryFiltrada = queryFiltrada.Where(r => r.Data <= dataFim.Value.Date);

            // os Sum são em cima da query inteira, antes do Skip/Take. no começo eu somava só os
            // items da página e os totais do topo da tela ficavam menores que o da planilha
            var totalRegistros = await queryFiltrada.CountAsync();
            var valorTotal = await queryFiltrada.SumAsync(r => r.ValorTotalVendas);
            var totalVendas = await queryFiltrada.SumAsync(r => r.NumVendas);
            var totalAtendimentos = await queryFiltrada.SumAsync(r => r.QuantAtendimento);

            // TODO ordenar por Data e mais alguma coisa, quando dois registros caem no mesmo dia
            // eles trocam de lugar entre uma chamada e outra e a paginação repete linha
            var items = await queryFiltrada
                .Include(r => r.Vendedor)
                .OrderByDescending(r => r.Data)
                .Skip((paginaAtual - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync();
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanho);
            if (totalPaginas == 0) totalPaginas = 1;
            var conversaoMedia = totalAtendimentos > 0 ? (decimal)totalVendas / totalAtendimentos : 0m;

            return new RegistroVendaPaginadoDTO
            {
                TotalRegistros = totalRegistros,
                PaginaAtual = paginaAtual,
                TotalPaginas = totalPaginas,
                ValorTotal = valorTotal,
                TotalVendas = totalVendas,
                TotalAtendimentos = totalAtendimentos,
                ConversaoMedia = conversaoMedia,
                Items = items.Select(r => new RegistroVendaDTO
                {
                    Id = r.IdRegistroVenda,
                    Data = r.Data,
                    PretasMistas = r.PretasMistas,
                    Garantia = r.Garantia,
                    CrediarioDujuca = r.CrediarioDujuca,
                    QuantAtendimento = r.QuantAtendimento,
                    NumVendas = r.NumVendas,
                    ValorTotalVendas = r.ValorTotalVendas,
                    VendedorId = r.IdVendedor,
                    VendedorNome = r.Vendedor?.Nome ?? string.Empty
                })
            };
        }

        public async Task<RegistroVendaDTO?> ObterPorId(Guid id)
        {
            var registro = await _context.RegistrosVenda
                .AsNoTracking()
                .Include(r => r.Vendedor)
                .FirstOrDefaultAsync(r => r.IdRegistroVenda == id);
            if (registro == null) return null;

            if (!_currentUser.EhGerente && registro.IdVendedor != _currentUser.VendedorId)
                throw new AcessoNegadoException("Você não tem permissão para acessar dados de outro vendedor.");

            return new RegistroVendaDTO
            {
                Id = registro.IdRegistroVenda,
                Data = registro.Data,
                PretasMistas = registro.PretasMistas,
                Garantia = registro.Garantia,
                CrediarioDujuca = registro.CrediarioDujuca,
                QuantAtendimento = registro.QuantAtendimento,
                NumVendas = registro.NumVendas,
                ValorTotalVendas = registro.ValorTotalVendas,
                VendedorId = registro.IdVendedor,
                VendedorNome = registro.Vendedor?.Nome ?? string.Empty
            };
        }

        public async Task<RegistroVendaDTO> Adicionar(RegistroVendaEntradaDTO request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (!_currentUser.EhGerente)
            {
                if (_currentUser.VendedorId == null)
                    throw new AcessoNegadoException("Seu usuário não está vinculado a nenhum vendedor.");

                request.VendedorId = _currentUser.VendedorId.Value;
            }

            if (request.VendedorId == Guid.Empty)
                throw new ArgumentException("O registro de venda deve estar associado a um vendedor válido.");

            if (request.NumVendas > request.QuantAtendimento)
                throw new InvalidOperationException("O número de vendas não pode ser maior que a quantidade de atendimentos.");
            await using var transacao = await _context.BeginTransactionAsync();
            try
            {
                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdVendedor == request.VendedorId);
                if (vendedor == null)
                    throw new KeyNotFoundException($"Vendedor com ID '{request.VendedorId}' não foi encontrado.");

                var jaExiste = await _context.RegistrosVenda
                    .AnyAsync(r => r.IdVendedor == request.VendedorId && r.Data.Date == request.Data.Date);
                if (jaExiste)
                    throw new ConflitoException($"Já existe um registro de venda para '{vendedor.Nome}' em {request.Data:dd/MM/yyyy}.");
                var registro = new RegistroVenda
                {
                    IdRegistroVenda = Guid.NewGuid(),
                    Data = request.Data.Date,
                    PretasMistas = request.PretasMistas,
                    Garantia = request.Garantia,
                    CrediarioDujuca = request.CrediarioDujuca,
                    QuantAtendimento = request.QuantAtendimento,
                    NumVendas = request.NumVendas,
                    ValorTotalVendas = request.ValorTotalVendas,
                    IdVendedor = request.VendedorId
                };

                _context.RegistrosVenda.Add(registro);
                await _context.SaveChangesAsync();

                await _auditService.Registrar(
                    "RegistroVenda.Criado",
                    "RegistroVenda",
                    registro.IdRegistroVenda.ToString(),
                    $"Vendedor={vendedor.Nome}; Data={registro.Data:yyyy-MM-dd}; ValorTotalVendas={registro.ValorTotalVendas}");

                await transacao.CommitAsync();

                // esse catch vazio é de propósito. a venda já foi commitada acima, se a checagem
                // da meta explodir aqui não dá pra desfazer nada mesmo, e o vendedor ia tomar erro
                // numa venda que na verdade salvou certinho
                try
                {
                    await _metaService.VerificarConclusao(registro.IdVendedor, registro.Data);
                }
                catch
                {
                }

                return new RegistroVendaDTO
                {
                    Id = registro.IdRegistroVenda,
                    Data = registro.Data,
                    PretasMistas = registro.PretasMistas,
                    Garantia = registro.Garantia,
                    CrediarioDujuca = registro.CrediarioDujuca,
                    QuantAtendimento = registro.QuantAtendimento,
                    NumVendas = registro.NumVendas,
                    ValorTotalVendas = registro.ValorTotalVendas,
                    VendedorId = registro.IdVendedor,
                    VendedorNome = vendedor.Nome
                };
            }
            catch (DbUpdateException)
            {
                await transacao.RollbackAsync();
                throw new ConflitoException("Não foi possível salvar o registro de venda: dado duplicado ou inconsistente.");
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }

        public async Task<RegistroVendaDTO> Editar(Guid id, RegistroVendaEntradaDTO request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await using var transacao = await _context.BeginTransactionAsync();
            try
            {
                var registro = await _context.RegistrosVenda.FirstOrDefaultAsync(r => r.IdRegistroVenda == id);

                if (registro == null)
                    throw new KeyNotFoundException($"Registro de venda com ID '{id}' não foi encontrado.");

                if (!_currentUser.EhGerente && registro.IdVendedor != _currentUser.VendedorId)
                    throw new AcessoNegadoException("Você não tem permissão para acessar dados de outro vendedor.");

                if (!_currentUser.EhGerente)
                {
                    request.VendedorId = registro.IdVendedor;
                }

                if (request.VendedorId == Guid.Empty)
                    throw new ArgumentException("O registro de venda deve estar associado a um vendedor válido.");

                if (request.NumVendas > request.QuantAtendimento)
                    throw new InvalidOperationException("O número de vendas não pode ser maior que a quantidade de atendimentos.");

                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdVendedor == request.VendedorId);
                if (vendedor == null)
                    throw new KeyNotFoundException($"Vendedor com ID '{request.VendedorId}' não foi encontrado.");
                var jaExiste = await _context.RegistrosVenda
                    .AnyAsync(r => r.IdVendedor == request.VendedorId && r.Data.Date == request.Data.Date && r.IdRegistroVenda != id);
                if (jaExiste)
                    throw new ConflitoException($"Já existe um registro de venda para '{vendedor.Nome}' em {request.Data:dd/MM/yyyy}.");
                registro.Data = request.Data.Date;
                registro.PretasMistas = request.PretasMistas;

                registro.Garantia = request.Garantia;
                registro.CrediarioDujuca = request.CrediarioDujuca;
                registro.QuantAtendimento = request.QuantAtendimento;
                registro.NumVendas = request.NumVendas;

                registro.ValorTotalVendas = request.ValorTotalVendas;
                registro.IdVendedor = request.VendedorId;
                await _context.SaveChangesAsync();

                await _auditService.Registrar(
                    "RegistroVenda.Editado",
                    "RegistroVenda",
                    registro.IdRegistroVenda.ToString(),
                    $"Vendedor={vendedor.Nome}; Data={registro.Data:yyyy-MM-dd}; ValorTotalVendas={registro.ValorTotalVendas}");

                await transacao.CommitAsync();

                try
                {
                    await _metaService.VerificarConclusao(registro.IdVendedor, registro.Data);
                }
                catch
                {
                }

                return new RegistroVendaDTO
                {
                    Id = registro.IdRegistroVenda,
                    Data = registro.Data,
                    PretasMistas = registro.PretasMistas,
                    Garantia = registro.Garantia,
                    CrediarioDujuca = registro.CrediarioDujuca,
                    QuantAtendimento = registro.QuantAtendimento,
                    NumVendas = registro.NumVendas,
                    ValorTotalVendas = registro.ValorTotalVendas,
                    VendedorId = registro.IdVendedor,
                    VendedorNome = vendedor.Nome

                };
            }
            catch (DbUpdateException)
            {
                await transacao.RollbackAsync();
                throw new ConflitoException("Não foi possível salvar o registro de venda: dado duplicado ou inconsistente.");
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }

        public async Task Deletar(Guid id)
        {
            await using var transacao = await _context.BeginTransactionAsync();
            try
            {
                var registro = await _context.RegistrosVenda.Include(r => r.Vendedor).FirstOrDefaultAsync(r => r.IdRegistroVenda == id);
                if (registro == null)
                    throw new KeyNotFoundException($"Registro de venda com ID '{id}' não foi encontrado.");

                if (!_currentUser.EhGerente && registro.IdVendedor != _currentUser.VendedorId)
                    throw new AcessoNegadoException("Você não tem permissão para acessar dados de outro vendedor.");

                var nomeDoVendedor = registro.Vendedor?.Nome ?? string.Empty;
                var detalhes = $"Vendedor={nomeDoVendedor}; Data={registro.Data:yyyy-MM-dd}; ValorTotalVendas={registro.ValorTotalVendas}";

                _context.RegistrosVenda.Remove(registro);
                await _context.SaveChangesAsync();

                await _auditService.Registrar("RegistroVenda.Excluido", "RegistroVenda", id.ToString(), detalhes);

                await transacao.CommitAsync();
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }

        public async Task<DashboardResumoDTO> ObterResumoGeral(DateTime? dataInicio, DateTime? dataFim)
        {
            var vendedores = await _context.Vendedores.AsNoTracking().OrderBy(v => v.Nome).ToListAsync();

            var query = _context.RegistrosVenda.AsNoTracking().AsQueryable();
            if (dataInicio.HasValue) query = query.Where(r => r.Data >= dataInicio.Value.Date);
            if (dataFim.HasValue) query = query.Where(r => r.Data <= dataFim.Value.Date);
            var registros = await query.ToListAsync();

            var somaValorTotalVendasGeral = registros.Sum(r => r.ValorTotalVendas);

            var agrupadoPorVendedor = registros
                .GroupBy(r => r.IdVendedor)
                .ToDictionary(g => g.Key, g => new
                {
                    ValorTotalVendas = g.Sum(r => r.ValorTotalVendas),
                    NumVendas = g.Sum(r => r.NumVendas),
                    QuantAtendimento = g.Sum(r => r.QuantAtendimento)
                });

            var ranking = vendedores
                .Select(v =>
                {
                    var linha = new RankingVendedorDTO
                    {
                        VendedorId = v.IdVendedor,
                        Nome = v.Nome,
                        ValorTotalVendas = 0m,
                        NumVendas = 0,
                        QuantAtendimento = 0
                    };

                    if (agrupadoPorVendedor.TryGetValue(v.IdVendedor, out var totais))
                    {
                        linha.ValorTotalVendas = totais.ValorTotalVendas;
                        linha.NumVendas = totais.NumVendas;
                        linha.QuantAtendimento = totais.QuantAtendimento;
                    }

                    linha.ParticipacaoPercentual = somaValorTotalVendasGeral > 0
                        ? Math.Round(linha.ValorTotalVendas / somaValorTotalVendasGeral * 100, 2)
                        : 0m;

                    return linha;
                })
                .OrderByDescending(r => r.ValorTotalVendas)
                .ToList();
            for (var i = 0; i < ranking.Count; i++)
            {
                ranking[i].Posicao = i + 1;
            }
            var evolucaoTemporal = registros
                .GroupBy(r => r.Data.ToString("yyyy-MM-dd"))
                .OrderBy(g => g.Key)
                .Select(g => new EvolucaoTemporalDTO
                {
                    Periodo = DateTime.Parse(g.Key).ToString("dd/MM/yyyy"),
                    ValorTotalVendas = g.Sum(r => r.ValorTotalVendas)
                })
                .ToList();

            return new DashboardResumoDTO
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalVendedores = vendedores.Count,
                TotalRegistros = registros.Count,
                SomaPretasMistas = registros.Sum(r => r.PretasMistas),
                SomaGarantia = registros.Sum(r => r.Garantia),
                SomaCrediarioDujuca = registros.Sum(r => r.CrediarioDujuca),
                SomaQuantAtendimento = registros.Sum(r => r.QuantAtendimento),
                SomaNumVendas = registros.Sum(r => r.NumVendas),
                SomaValorTotalVendas = somaValorTotalVendasGeral,
                Ranking = ranking,
                EvolucaoTemporal = evolucaoTemporal
            };
        }

        public async Task<PerfilVendedorDTO> ObterPerfilVendedor(
            Guid vendedorId,
            DateTime? dataInicio,
            DateTime? dataFim,
            int pagina,
            int tamanhoPagina)
        {
            if (!_currentUser.EhGerente && vendedorId != _currentUser.VendedorId)
                throw new AcessoNegadoException("Você não tem permissão para acessar dados de outro vendedor.");

            var vendedor = await _context.Vendedores.AsNoTracking().FirstOrDefaultAsync(v => v.IdVendedor == vendedorId);
            if (vendedor == null)
                throw new KeyNotFoundException($"Vendedor com ID '{vendedorId}' não foi encontrado.");

            var paginaAtual = pagina < 1 ? 1 : pagina;
            var tamanho = tamanhoPagina < 1 ? 10 : tamanhoPagina;

            var query = _context.RegistrosVenda.AsNoTracking().AsQueryable();
            query = query.Where(r => r.IdVendedor == vendedorId);
            if (dataInicio.HasValue) query = query.Where(r => r.Data >= dataInicio.Value.Date);
            if (dataFim.HasValue) query = query.Where(r => r.Data <= dataFim.Value.Date);
            var registros = await query.OrderByDescending(r => r.Data).ToListAsync();

            var totalPaginas = (int)Math.Ceiling(registros.Count / (double)tamanho);
            if (totalPaginas == 0) totalPaginas = 1;

            var paginaAtualItens = registros
                .Skip((paginaAtual - 1) * tamanho)
                .Take(tamanho)
                .Select(r => new RegistroVendaDTO
                {
                    Id = r.IdRegistroVenda,
                    Data = r.Data,
                    PretasMistas = r.PretasMistas,
                    Garantia = r.Garantia,
                    CrediarioDujuca = r.CrediarioDujuca,
                    QuantAtendimento = r.QuantAtendimento,
                    NumVendas = r.NumVendas,
                    ValorTotalVendas = r.ValorTotalVendas,
                    VendedorId = r.IdVendedor,
                    VendedorNome = vendedor.Nome
                })
                .ToList();

            return new PerfilVendedorDTO
            {
                Vendedor = new VendedorDTO
                {
                    Id = vendedor.IdVendedor,
                    Nome = vendedor.Nome
                },
                DataInicio = dataInicio,
                DataFim = dataFim,
                SomaPretasMistas = registros.Sum(r => r.PretasMistas),
                SomaGarantia = registros.Sum(r => r.Garantia),
                SomaCrediarioDujuca = registros.Sum(r => r.CrediarioDujuca),
                SomaQuantAtendimento = registros.Sum(r => r.QuantAtendimento),
                SomaNumVendas = registros.Sum(r => r.NumVendas),
                SomaValorTotalVendas = registros.Sum(r => r.ValorTotalVendas),
                TotalRegistrosPeriodo = registros.Count,
                PaginaAtual = paginaAtual,
                TotalPaginas = totalPaginas,
                Registros = paginaAtualItens
            };
        }
    }
}
