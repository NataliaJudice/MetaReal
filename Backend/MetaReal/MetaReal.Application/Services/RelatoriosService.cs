using System.Globalization;
using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;

using MetaReal.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace MetaReal.Application.Services
{
    public class RelatoriosService : IRelatoriosService
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        private readonly IMetaRealDbContext _context;
        private readonly IUsuarioAtualService _usuarioAtual;

        public RelatoriosService(IMetaRealDbContext context, IUsuarioAtualService usuarioAtual)
        {
            _context = context;
            _usuarioAtual = usuarioAtual;
        }

        public Task<RelatorioDTO> Gerar(string chave, FiltroRelatorio filtro)
        {
            if (!_usuarioAtual.EhGerente)
            {
                filtro.VendedorId = _usuarioAtual.VendedorId ?? Guid.Empty;
            }


            return chave switch
            {

                "desempenho-vendedor" => DesempenhoPorVendedor(filtro),
                "cumprimento-metas" => CumprimentoDeMetas(filtro),
                "evolucao-vendas" => EvolucaoDeVendas(filtro),
                "produtividade-conversao" => ProdutividadeEConversao(filtro),
                "consistencia-lancamentos" => ConsistenciaDeLancamentos(filtro),
                "garantias-servico" => GarantiasEServico(filtro),
                "crediario-dujuca" => CrediarioDujuca(filtro),
                _ => throw new KeyNotFoundException($"Relatório '{chave}' não existe.")
            };
        }

        private async Task<RelatorioDTO> DesempenhoPorVendedor(FiltroRelatorio filtro)
        {
            var query = _context.RegistrosVenda.AsNoTracking().Include(r => r.Vendedor).AsQueryable();
            var vid = filtro.VendedorId ?? Guid.Empty;
            if (vid != Guid.Empty) query = query.Where(r => r.IdVendedor == vid);
            if (filtro.DataInicio.HasValue) query = query.Where(r => r.Data >= filtro.DataInicio.Value.Date);
            if (filtro.DataFim.HasValue) query = query.Where(r => r.Data < filtro.DataFim.Value.Date.AddDays(1));
            var registros = await query.ToListAsync();

            var linhas = registros
                .GroupBy(r => r.IdVendedor)
                .Select(g =>
                {
                    var valor = g.Sum(r => r.ValorTotalVendas);
                    var vendas = g.Sum(r => r.NumVendas);
                    var atendimentos = g.Sum(r => r.QuantAtendimento);
                    var conversao = atendimentos > 0 ? (decimal)vendas / atendimentos : 0m;
                    var ticket = vendas > 0 ? valor / vendas : 0m;

                    return new
                    {
                        Nome = g.First().Vendedor?.Nome ?? "—",
                        Valor = valor,
                        Vendas = vendas,
                        Atendimentos = atendimentos,
                        Conversao = conversao,
                        Ticket = ticket,
                        Garantia = g.Sum(r => r.Garantia),
                        Crediario = g.Sum(r => r.CrediarioDujuca)
                    };
                })
                .OrderByDescending(x => x.Valor)
                .ToList();

            var totalGeral = linhas.Sum(l => l.Valor);

            var totalDeVendas = linhas.Sum(l => l.Vendas);
            var ticketMedioGeral = totalDeVendas > 0 ? totalGeral / totalDeVendas : 0m;
            var colunas = new List<ColunaRelatorio>
            {
                new ColunaRelatorio("posicao", "#", TipoColuna.Numero),
                new ColunaRelatorio("vendedor", "Vendedor"),
                new ColunaRelatorio("valorTotal", "Valor Total", TipoColuna.Moeda),
                new ColunaRelatorio("participacao", "Participação", TipoColuna.Percentual),
                new ColunaRelatorio("vendas", "Vendas", TipoColuna.Numero),
                new ColunaRelatorio("atendimentos", "Atendimentos", TipoColuna.Numero),
                new ColunaRelatorio("conversao", "Conversão", TipoColuna.Percentual),
                new ColunaRelatorio("ticketMedio", "Ticket Médio", TipoColuna.Moeda),
                new ColunaRelatorio("garantia", "Garantia", TipoColuna.Moeda),
                new ColunaRelatorio("crediario", "Crediário", TipoColuna.Moeda)
            };

            var linhasDoRelatorio = linhas.Select((l, i) =>
            {
                var participacao = totalGeral > 0 ? l.Valor / totalGeral : 0m;
                var linha = new Dictionary<string, object?>
                {

                    ["posicao"] = i + 1,
                    ["vendedor"] = l.Nome,
                    ["valorTotal"] = l.Valor,
                    ["participacao"] = participacao,
                    ["vendas"] = l.Vendas,
                    ["atendimentos"] = l.Atendimentos,
                    ["conversao"] = l.Conversao,
                    ["ticketMedio"] = l.Ticket,
                    ["garantia"] = l.Garantia,
                    ["crediario"] = l.Crediario
                };
                return linha;
            }).ToList();

            var queryPeriodo = _context.RegistrosVenda.AsNoTracking().AsQueryable();
            if (vid != Guid.Empty) queryPeriodo = queryPeriodo.Where(r => r.IdVendedor == vid);

            var temReg = queryPeriodo.Any();
            var pInicio = filtro.DataInicio?.Date ?? (temReg ? queryPeriodo.Min(r => r.Data).Date : DateTime.Today);
            var pFim = filtro.DataFim?.Date ?? (temReg ? queryPeriodo.Max(r => r.Data).Date : DateTime.Today);
            var descPeriodo = $"{pInicio:dd/MM/yyyy} a {pFim:dd/MM/yyyy}";
            if (vid != Guid.Empty)
            {

                var nomeVend = _context.Vendedores.AsNoTracking().Where(v => v.IdVendedor == vid).Select(v => v.Nome).FirstOrDefault();
                if (!string.IsNullOrEmpty(nomeVend)) descPeriodo += $" · {nomeVend}";
            }

            return new RelatorioDTO
            {
                Chave = "desempenho-vendedor",
                Titulo = "Desempenho por Vendedor",
                Subtitulo = descPeriodo,
                Colunas = colunas,
                Linhas = linhasDoRelatorio,
                Resumo = new List<ResumoRelatorio>
                {
                    new ResumoRelatorio("Faturamento total", totalGeral.ToString("C", PtBr)),
                    new ResumoRelatorio("Vendedores no período", linhas.Count.ToString()),
                    new ResumoRelatorio("Total de vendas", totalDeVendas.ToString()),
                    new ResumoRelatorio("Ticket médio geral", ticketMedioGeral.ToString("C", PtBr))
                }
            };
        }

        //  esse bloco de resolver o periodo tá copiado em todos os relatorios daqui pra baixo,
        // uma hora eu extraio
        private async Task<RelatorioDTO> CumprimentoDeMetas(FiltroRelatorio filtro)
        {
            var queryPeriodo = _context.RegistrosVenda.AsNoTracking().AsQueryable();

            var vid = filtro.VendedorId ?? Guid.Empty;
            if (vid != Guid.Empty) queryPeriodo = queryPeriodo.Where(r => r.IdVendedor == vid);
            var temReg = queryPeriodo.Any();
            var pInicio = filtro.DataInicio?.Date ?? (temReg ? queryPeriodo.Min(r => r.Data).Date : DateTime.Today);
            var pFim = filtro.DataFim?.Date ?? (temReg ? queryPeriodo.Max(r => r.Data).Date : DateTime.Today);

            var metasQuery = _context.MetasVenda.AsNoTracking().AsQueryable();
            if (vid != Guid.Empty) metasQuery = metasQuery.Where(m => m.IdVendedor == vid);
            var metas = await metasQuery.ToListAsync();

            var primeiroMes = new DateTime(pInicio.Year, pInicio.Month, 1);
            var ultimoMes = new DateTime(pFim.Year, pFim.Month, 1);
            metas = metas.Where(m => new DateTime(m.Ano, m.Mes, 1) >= primeiroMes && new DateTime(m.Ano, m.Mes, 1) <= ultimoMes).ToList();

            var regQuery = _context.RegistrosVenda.AsNoTracking().Include(r => r.Vendedor).AsQueryable();
            if (vid != Guid.Empty) regQuery = regQuery.Where(r => r.IdVendedor == vid);
            if (filtro.DataInicio.HasValue) regQuery = regQuery.Where(r => r.Data >= filtro.DataInicio.Value.Date);
            if (filtro.DataFim.HasValue) regQuery = regQuery.Where(r => r.Data < filtro.DataFim.Value.Date.AddDays(1));

            var registros = await regQuery.ToListAsync();
            var vendedores = await _context.Vendedores.AsNoTracking().ToDictionaryAsync(v => v.IdVendedor, v => v.Nome);

            var realizadoPorMes = registros
                .GroupBy(r => new { r.IdVendedor, r.Data.Year, r.Data.Month })
                .ToDictionary(g => (g.Key.IdVendedor, g.Key.Year, g.Key.Month), g => g.Sum(r => r.ValorTotalVendas));


            var linhas = metas
                .OrderByDescending(m => m.Ano).ThenByDescending(m => m.Mes)
                .ThenBy(m => vendedores.TryGetValue(m.IdVendedor, out var n) ? n : "")
                .Select(m =>
                {
                    realizadoPorMes.TryGetValue((m.IdVendedor, m.Ano, m.Mes), out var realizado);
                    var perc = m.ValorMeta > 0 ? realizado / m.ValorMeta : 0m;
                    var situacao = realizado >= m.ValorMeta ? "Bateu" : "Não bateu";

                    return new Dictionary<string, object?>
                    {
                        ["competencia"] = $"{m.Mes:00}/{m.Ano}",
                        ["vendedor"] = vendedores.TryGetValue(m.IdVendedor, out var nome) ? nome : "—",
                        ["meta"] = m.ValorMeta,
                        ["realizado"] = realizado,
                        ["percentual"] = perc,
                        ["diferenca"] = realizado - m.ValorMeta,
                        ["situacao"] = situacao
                    };
                }).ToList();

            var bateram = linhas.Count(l => (string?)l["situacao"] == "Bateu");
            var taxaDeCumprimento = linhas.Count > 0 ? (decimal)bateram / linhas.Count : 0m;

            var descPeriodo = $"{pInicio:dd/MM/yyyy} a {pFim:dd/MM/yyyy}";
            if (vid != Guid.Empty && vendedores.TryGetValue(vid, out var nomeV)) descPeriodo += $" · {nomeV}";

            return new RelatorioDTO
            {
                Chave = "cumprimento-metas",
                Titulo = "Cumprimento de Metas",
                Subtitulo = descPeriodo,
                Colunas = new List<ColunaRelatorio>
                {
                    new ColunaRelatorio("competencia", "Competência"),
                    new ColunaRelatorio("vendedor", "Vendedor"),
                    new ColunaRelatorio("meta", "Meta", TipoColuna.Moeda),
                    new ColunaRelatorio("realizado", "Realizado", TipoColuna.Moeda),
                    new ColunaRelatorio("percentual", "Atingido", TipoColuna.Percentual),
                    new ColunaRelatorio("diferenca", "Diferença", TipoColuna.Moeda),
                    new ColunaRelatorio("situacao", "Situação")
                },
                Linhas = linhas,
                Resumo = new List<ResumoRelatorio>
                {
                    new ResumoRelatorio("Metas no período", linhas.Count.ToString()),
                    new ResumoRelatorio("Metas batidas", bateram.ToString()),
                    new ResumoRelatorio("Taxa de cumprimento", taxaDeCumprimento.ToString("P1", PtBr)),
                    new ResumoRelatorio("Soma das metas", linhas.Sum(l => (decimal)(l["meta"] ?? 0m)).ToString("C", PtBr))

                }
            };

        }

        private async Task<RelatorioDTO> EvolucaoDeVendas(FiltroRelatorio filtro)
        {

            var query = _context.RegistrosVenda.AsNoTracking().Include(r => r.Vendedor).AsQueryable();
            var vid = filtro.VendedorId ?? Guid.Empty;
            if (vid != Guid.Empty) query = query.Where(r => r.IdVendedor == vid);
            if (filtro.DataInicio.HasValue) query = query.Where(r => r.Data >= filtro.DataInicio.Value.Date);
            if (filtro.DataFim.HasValue) query = query.Where(r => r.Data < filtro.DataFim.Value.Date.AddDays(1));
            var registros = await query.ToListAsync();
            var porMes = string.Equals(filtro.Agrupamento, "mes", StringComparison.OrdinalIgnoreCase);

            var grupos = registros
                .GroupBy(r => porMes ? new DateTime(r.Data.Year, r.Data.Month, 1) : r.Data.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Periodo = porMes ? g.Key.ToString("MM/yyyy") : g.Key.ToString("dd/MM/yyyy"),
                    Valor = g.Sum(r => r.ValorTotalVendas),
                    Vendas = g.Sum(r => r.NumVendas),
                    Atendimentos = g.Sum(r => r.QuantAtendimento)
                }).ToList();

            var total = grupos.Sum(g => g.Valor);

            var melhor = grupos.OrderByDescending(g => g.Valor).FirstOrDefault();
            var mediaPorPeriodo = grupos.Count > 0 ? total / grupos.Count : 0m;

            var titulo = porMes ? "Evolução de Vendas (mensal)" : "Evolução de Vendas (diária)";

            var tituloColuna = porMes ? "Mês" : "Data";

            var rotuloContagem = porMes ? "Meses com registro" : "Dias com registro";

            var linhas = grupos.Select(g =>
            {
                var conversao = g.Atendimentos > 0 ? (decimal)g.Vendas / g.Atendimentos : 0m;
                var ticketMedio = g.Vendas > 0 ? g.Valor / g.Vendas : 0m;

                return new Dictionary<string, object?>
                {
                    ["periodo"] = g.Periodo,
                    ["valor"] = g.Valor,
                    ["vendas"] = g.Vendas,
                    ["atendimentos"] = g.Atendimentos,
                    ["conversao"] = conversao,
                    ["ticketMedio"] = ticketMedio
                };
            }).ToList();

            var temReg = query.Any();
            var pInicio = filtro.DataInicio?.Date ?? (temReg ? query.Min(r => r.Data).Date : DateTime.Today);
            var pFim = filtro.DataFim?.Date ?? (temReg ? query.Max(r => r.Data).Date : DateTime.Today);
            var descPeriodo = $"{pInicio:dd/MM/yyyy} a {pFim:dd/MM/yyyy}";
            if (vid != Guid.Empty)
            {
                var nomeV = _context.Vendedores.AsNoTracking().Where(v => v.IdVendedor == vid).Select(v => v.Nome).FirstOrDefault();
                if (!string.IsNullOrEmpty(nomeV)) descPeriodo += $" · {nomeV}";
            }

            return new RelatorioDTO
            {
                Chave = "evolucao-vendas",
                Titulo = titulo,
                Subtitulo = descPeriodo,
                Colunas = new List<ColunaRelatorio>
                {
                    new ColunaRelatorio("periodo", tituloColuna),
                    new ColunaRelatorio("valor", "Valor Vendido", TipoColuna.Moeda),
                    new ColunaRelatorio("vendas", "Vendas", TipoColuna.Numero),
                    new ColunaRelatorio("atendimentos", "Atendimentos", TipoColuna.Numero),
                    new ColunaRelatorio("conversao", "Conversão", TipoColuna.Percentual),
                    new ColunaRelatorio("ticketMedio", "Ticket Médio", TipoColuna.Moeda)
                },
                Linhas = linhas,
                Resumo = new List<ResumoRelatorio>
                {
                    new ResumoRelatorio("Total do período", total.ToString("C", PtBr)),
                    new ResumoRelatorio(rotuloContagem, grupos.Count.ToString()),
                    new ResumoRelatorio("Média por período", mediaPorPeriodo.ToString("C", PtBr)),
                    new ResumoRelatorio("Melhor período", melhor != null ? $"{melhor.Periodo} ({melhor.Valor.ToString("C", PtBr)})" : "—")

                }

            };
        }
        private async Task<RelatorioDTO> ProdutividadeEConversao(FiltroRelatorio filtro)
        {

            var query = _context.RegistrosVenda.AsNoTracking().Include(r => r.Vendedor).AsQueryable();
            var vid = filtro.VendedorId ?? Guid.Empty;
            if (vid != Guid.Empty) query = query.Where(r => r.IdVendedor == vid);
            if (filtro.DataInicio.HasValue) query = query.Where(r => r.Data >= filtro.DataInicio.Value.Date);
            if (filtro.DataFim.HasValue) query = query.Where(r => r.Data < filtro.DataFim.Value.Date.AddDays(1));
            var registros = await query.ToListAsync();

            var linhas = registros
                .GroupBy(r => r.IdVendedor)
                .Select(g =>
                {
                    var atendimentos = g.Sum(r => r.QuantAtendimento);
                    var vendas = g.Sum(r => r.NumVendas);
                    var valor = g.Sum(r => r.ValorTotalVendas);
                    var dias = g.Select(r => r.Data.Date).Distinct().Count();
                    var conversao = atendimentos > 0 ? (decimal)vendas / atendimentos : 0m;
                    var ticket = vendas > 0 ? valor / vendas : 0m;

                    var atendPorDia = dias > 0 ? (decimal)atendimentos / dias : 0m;

                    return new
                    {
                        Nome = g.First().Vendedor?.Nome ?? "—",
                        Atendimentos = atendimentos,
                        Vendas = vendas,
                        Perdidos = atendimentos - vendas,
                        Conversao = conversao,
                        Ticket = ticket,
                        AtendPorDia = atendPorDia,
                        Dias = dias
                    };
                })
                .OrderByDescending(x => x.Conversao)
                .ToList();

            var totalAtend = linhas.Sum(l => l.Atendimentos);
            var totalVendas = linhas.Sum(l => l.Vendas);

            var conversaoGeral = totalAtend > 0 ? (decimal)totalVendas / totalAtend : 0m;

            var linhasDoRelatorio = linhas.Select(l => new Dictionary<string, object?>
            {
                ["vendedor"] = l.Nome,
                ["atendimentos"] = l.Atendimentos,
                ["vendas"] = l.Vendas,
                ["perdidos"] = l.Perdidos,
                ["conversao"] = l.Conversao,
                ["ticketMedio"] = l.Ticket,
                ["diasTrabalhados"] = l.Dias,
                ["atendPorDia"] = Math.Round(l.AtendPorDia, 1)
            }).ToList();

            var temReg = query.Any();
            var pInicio = filtro.DataInicio?.Date ?? (temReg ? query.Min(r => r.Data).Date : DateTime.Today);

            var pFim = filtro.DataFim?.Date ?? (temReg ? query.Max(r => r.Data).Date : DateTime.Today);
            var descPeriodo = $"{pInicio:dd/MM/yyyy} a {pFim:dd/MM/yyyy}";
            if (vid != Guid.Empty)
            {
                var nomeV = _context.Vendedores.AsNoTracking().Where(v => v.IdVendedor == vid).Select(v => v.Nome).FirstOrDefault();
                if (!string.IsNullOrEmpty(nomeV)) descPeriodo += $" · {nomeV}";
            }

            return new RelatorioDTO
            {
                Chave = "produtividade-conversao",
                Titulo = "Produtividade e Conversão",
                Subtitulo = descPeriodo,
                Colunas = new List<ColunaRelatorio>
                {

                    new ColunaRelatorio("vendedor", "Vendedor"),
                    new ColunaRelatorio("atendimentos", "Atendimentos", TipoColuna.Numero),
                    new ColunaRelatorio("vendas", "Vendas", TipoColuna.Numero),
                    new ColunaRelatorio("perdidos", "Não converteram", TipoColuna.Numero),
                    new ColunaRelatorio("conversao", "Taxa de Conversão", TipoColuna.Percentual),
                    new ColunaRelatorio("ticketMedio", "Ticket Médio", TipoColuna.Moeda),
                    new ColunaRelatorio("diasTrabalhados", "Dias com registro", TipoColuna.Numero),
                    new ColunaRelatorio("atendPorDia", "Atend./dia", TipoColuna.Numero)
                },
                Linhas = linhasDoRelatorio,
                Resumo = new List<ResumoRelatorio>
                {
                    new ResumoRelatorio("Atendimentos totais", totalAtend.ToString()),
                    new ResumoRelatorio("Vendas totais", totalVendas.ToString()),
                    new ResumoRelatorio("Conversão geral", conversaoGeral.ToString("P1", PtBr)),
                    new ResumoRelatorio("Oportunidades perdidas", (totalAtend - totalVendas).ToString())
                }
            };
        }

        private async Task<RelatorioDTO> ConsistenciaDeLancamentos(FiltroRelatorio filtro)
        {
            var queryPeriodo = _context.RegistrosVenda.AsNoTracking().AsQueryable();
            var vid = filtro.VendedorId ?? Guid.Empty;
            if (vid != Guid.Empty) queryPeriodo = queryPeriodo.Where(r => r.IdVendedor == vid);
            var temReg = queryPeriodo.Any();
            var pInicio = filtro.DataInicio?.Date ?? (temReg ? queryPeriodo.Min(r => r.Data).Date : DateTime.Today);
            var pFim = filtro.DataFim?.Date ?? (temReg ? queryPeriodo.Max(r => r.Data).Date : DateTime.Today);
            var diasEsperados = new List<DateTime>();

            for (var d = pInicio; d <= pFim; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Sunday) diasEsperados.Add(d);
            }

            var regQuery = _context.RegistrosVenda.AsNoTracking().Include(r => r.Vendedor).AsQueryable();
            if (vid != Guid.Empty) regQuery = regQuery.Where(r => r.IdVendedor == vid);
            if (filtro.DataInicio.HasValue) regQuery = regQuery.Where(r => r.Data >= filtro.DataInicio.Value.Date);
            if (filtro.DataFim.HasValue) regQuery = regQuery.Where(r => r.Data < filtro.DataFim.Value.Date.AddDays(1));
            var registros = await regQuery.ToListAsync();

            var vendQuery = _context.Vendedores.AsNoTracking().AsQueryable();
            if (vid != Guid.Empty) vendQuery = vendQuery.Where(v => v.IdVendedor == vid);
            var vendedores = await vendQuery.OrderBy(v => v.Nome).ToListAsync();

            var linhas = vendedores.Select(v =>
            {
                var lancados = registros.Where(r => r.IdVendedor == v.IdVendedor).Select(r => r.Data.Date).ToHashSet();
                var faltantes = diasEsperados.Where(d => !lancados.Contains(d)).ToList();
                var cobertura = diasEsperados.Count > 0 ? (decimal)lancados.Count / diasEsperados.Count : 0m;

                var textoFaltantes = "—";

                if (faltantes.Count > 0)
                {

                    textoFaltantes = string.Join(", ", faltantes.Take(12).Select(d => d.ToString("dd/MM")));
                    if (faltantes.Count > 12) textoFaltantes += $" (+{faltantes.Count - 12})";
                }

                return new Dictionary<string, object?>
                {

                    ["vendedor"] = v.Nome,
                    ["diasEsperados"] = diasEsperados.Count,
                    ["diasLancados"] = lancados.Count,
                    ["diasFaltantes"] = faltantes.Count,
                    ["cobertura"] = cobertura,
                    ["datasFaltantes"] = textoFaltantes
                };
            }).ToList();

            var totalFaltantes = linhas.Sum(l => (int)(l["diasFaltantes"] ?? 0));
            var vendedoresEmDia = linhas.Count(l => (int)(l["diasFaltantes"] ?? 0) == 0);

            var descPeriodo = $"{pInicio:dd/MM/yyyy} a {pFim:dd/MM/yyyy}";
            if (vid != Guid.Empty)
            {
                var nomeV = vendedores.FirstOrDefault(v => v.IdVendedor == vid)?.Nome;
                if (!string.IsNullOrEmpty(nomeV)) descPeriodo += $" · {nomeV}";
            }

            return new RelatorioDTO
            {
                Chave = "consistencia-lancamentos",
                Titulo = "Consistência de Lançamentos",
                Subtitulo = $"{descPeriodo} · dias úteis (seg-sáb)",
                Colunas = new List<ColunaRelatorio>
                {
                    new ColunaRelatorio("vendedor", "Vendedor"),
                    new ColunaRelatorio("diasEsperados", "Dias esperados", TipoColuna.Numero),
                    new ColunaRelatorio("diasLancados", "Dias lançados", TipoColuna.Numero),
                    new ColunaRelatorio("diasFaltantes", "Em falta", TipoColuna.Numero),
                    new ColunaRelatorio("cobertura", "Cobertura", TipoColuna.Percentual),
                    new ColunaRelatorio("datasFaltantes", "Datas sem lançamento")
                },
                Linhas = linhas,
                Resumo = new List<ResumoRelatorio>
                {
                    new ResumoRelatorio("Dias úteis no período", diasEsperados.Count.ToString()),
                    new ResumoRelatorio("Vendedores avaliados", linhas.Count.ToString()),
                    new ResumoRelatorio("Lançamentos em falta", totalFaltantes.ToString()),
                    new ResumoRelatorio("Vendedores em dia", vendedoresEmDia.ToString())
                }
            };
        }
        private async Task<RelatorioDTO> GarantiasEServico(FiltroRelatorio filtro)
        {
            var query = _context.RegistrosVenda.AsNoTracking().Include(r => r.Vendedor).AsQueryable();
            var vid = filtro.VendedorId ?? Guid.Empty;
            if (vid != Guid.Empty) query = query.Where(r => r.IdVendedor == vid);
            if (filtro.DataInicio.HasValue) query = query.Where(r => r.Data >= filtro.DataInicio.Value.Date);
            if (filtro.DataFim.HasValue) query = query.Where(r => r.Data < filtro.DataFim.Value.Date.AddDays(1));
            var registros = await query.ToListAsync();


            var linhas = registros
                .GroupBy(r => r.IdVendedor)
                .Select(g =>
                {
                    var garantia = g.Sum(r => r.Garantia);

                    var valor = g.Sum(r => r.ValorTotalVendas);
                    var vendas = g.Sum(r => r.NumVendas);
                    var percServico = valor > 0 ? garantia / valor : 0m;
                    var porVenda = vendas > 0 ? garantia / vendas : 0m;
                    return new
                    {
                        Nome = g.First().Vendedor?.Nome ?? "—",
                        Garantia = garantia,
                        Valor = valor,
                        PercServico = percServico,
                        PorVenda = porVenda
                    };
                })
                .OrderByDescending(x => x.Garantia)
                .ToList();
            var totalGarantia = linhas.Sum(l => l.Garantia);

            var totalValor = linhas.Sum(l => l.Valor);
            var percServicoGeral = totalValor > 0 ? totalGarantia / totalValor : 0m;
            var melhorServico = linhas.Count > 0 ? linhas.OrderByDescending(l => l.PercServico).First().Nome : "—";

            var linhasDoRelatorio = linhas.Select(l => new Dictionary<string, object?>
            {
                ["vendedor"] = l.Nome,
                ["garantia"] = l.Garantia,
                ["valorTotal"] = l.Valor,
                ["percServico"] = l.PercServico,
                ["garantiaPorVenda"] = l.PorVenda
            }).ToList();
            var temReg = query.Any();

            var pInicio = filtro.DataInicio?.Date ?? (temReg ? query.Min(r => r.Data).Date : DateTime.Today);
            var pFim = filtro.DataFim?.Date ?? (temReg ? query.Max(r => r.Data).Date : DateTime.Today);
            var descPeriodo = $"{pInicio:dd/MM/yyyy} a {pFim:dd/MM/yyyy}";
            if (vid != Guid.Empty)
            {
                var nomeV = _context.Vendedores.AsNoTracking().Where(v => v.IdVendedor == vid).Select(v => v.Nome).FirstOrDefault();
                if (!string.IsNullOrEmpty(nomeV)) descPeriodo += $" · {nomeV}";
            }

            return new RelatorioDTO
            {
                Chave = "garantias-servico",
                Titulo = "Garantias e % Serviço",
                Subtitulo = descPeriodo,
                Colunas = new List<ColunaRelatorio>
                {
                    new ColunaRelatorio("vendedor", "Vendedor"),
                    new ColunaRelatorio("garantia", "Garantia", TipoColuna.Moeda),
                    new ColunaRelatorio("valorTotal", "Valor Vendido", TipoColuna.Moeda),
                    new ColunaRelatorio("percServico", "% Serviço", TipoColuna.Percentual),
                    new ColunaRelatorio("garantiaPorVenda", "Garantia por venda", TipoColuna.Moeda)
                },
                Linhas = linhasDoRelatorio,
                Resumo = new List<ResumoRelatorio>
                {
                    new ResumoRelatorio("Garantia total", totalGarantia.ToString("C", PtBr)),
                    new ResumoRelatorio("% Serviço geral", percServicoGeral.ToString("P1", PtBr)),
                    new ResumoRelatorio("Faturamento no período", totalValor.ToString("C", PtBr)),
                    new ResumoRelatorio("Melhor % serviço", melhorServico)
                }
            };
        }

        private async Task<RelatorioDTO> CrediarioDujuca(FiltroRelatorio filtro)
        {
            var query = _context.RegistrosVenda.AsNoTracking().Include(r => r.Vendedor).AsQueryable();
            var vid = filtro.VendedorId ?? Guid.Empty;
            if (vid != Guid.Empty) query = query.Where(r => r.IdVendedor == vid);
            if (filtro.DataInicio.HasValue) query = query.Where(r => r.Data >= filtro.DataInicio.Value.Date);
            if (filtro.DataFim.HasValue) query = query.Where(r => r.Data < filtro.DataFim.Value.Date.AddDays(1));
            var registros = await query.ToListAsync();

            var linhas = registros
                .GroupBy(r => r.IdVendedor)
                .Select(g =>
                {
                    var crediario = g.Sum(r => r.CrediarioDujuca);
                    var valor = g.Sum(r => r.ValorTotalVendas);
                    var participacao = valor > 0 ? crediario / valor : 0m;

                    return new
                    {
                        Nome = g.First().Vendedor?.Nome ?? "—",
                        Crediario = crediario,
                        Valor = valor,
                        Participacao = participacao,
                        DiasComCrediario = g.Count(r => r.CrediarioDujuca > 0)
                    };
                })
                .OrderByDescending(x => x.Crediario)
                .ToList();

            var totalCrediario = linhas.Sum(l => l.Crediario);
            var totalValor = linhas.Sum(l => l.Valor);
            var participacaoGeral = totalValor > 0 ? totalCrediario / totalValor : 0m;

            var linhasDoRelatorio = linhas.Select(l => new Dictionary<string, object?>
            {
                ["vendedor"] = l.Nome,
                ["crediario"] = l.Crediario,
                ["valorTotal"] = l.Valor,
                ["participacao"] = l.Participacao,
                ["diasComCrediario"] = l.DiasComCrediario
            }).ToList();

            var temReg = query.Any();

            var pInicio = filtro.DataInicio?.Date ?? (temReg ? query.Min(r => r.Data).Date : DateTime.Today);
            var pFim = filtro.DataFim?.Date ?? (temReg ? query.Max(r => r.Data).Date : DateTime.Today);
            var descPeriodo = $"{pInicio:dd/MM/yyyy} a {pFim:dd/MM/yyyy}";
            if (vid != Guid.Empty)
            {

                var nomeV = _context.Vendedores.AsNoTracking().Where(v => v.IdVendedor == vid).Select(v => v.Nome).FirstOrDefault();
                if (!string.IsNullOrEmpty(nomeV)) descPeriodo += $" · {nomeV}";
            }
            return new RelatorioDTO
            {
                Chave = "crediario-dujuca",
                Titulo = "Crediário Dujuca",
                Subtitulo = descPeriodo,
                Colunas = new List<ColunaRelatorio>
                {
                    new ColunaRelatorio("vendedor", "Vendedor"),
                    new ColunaRelatorio("crediario", "Crediário", TipoColuna.Moeda),
                    new ColunaRelatorio("valorTotal", "Valor Vendido", TipoColuna.Moeda),
                    new ColunaRelatorio("participacao", "% do faturamento", TipoColuna.Percentual),
                    new ColunaRelatorio("diasComCrediario", "Dias com crediário", TipoColuna.Numero)
                },
                Linhas = linhasDoRelatorio,
                Resumo = new List<ResumoRelatorio>
                {
                    new ResumoRelatorio("Crediário total", totalCrediario.ToString("C", PtBr)),
                    new ResumoRelatorio("% do faturamento", participacaoGeral.ToString("P1", PtBr)),
                    new ResumoRelatorio("Faturamento no período", totalValor.ToString("C", PtBr)),
                    new ResumoRelatorio("Fora do crediário", (totalValor - totalCrediario).ToString("C", PtBr))
                }
            };

        }
    }
}
