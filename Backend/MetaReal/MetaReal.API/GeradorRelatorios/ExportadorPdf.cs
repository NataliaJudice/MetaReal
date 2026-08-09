using System.Globalization;
using MetaReal.Application.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MetaReal.API.Relatorios;

public static class ExportadorPdf
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private const string Indigo = "#4F46E5";
    private const string Cinza = "#64748B";

    public static byte[] Gerar(RelatorioDTO relatorio)
    {
        var paisagem = relatorio.Colunas.Count > 6;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(paisagem ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontSize(9).FontFamily(Fonts.Calibri));

                page.Header().Element(c => MontarCabecalho(c, relatorio));
                page.Content().PaddingTop(12).Element(c => MontarConteudo(c, relatorio));
                page.Footer().Element(MontarRodape);
            });
        }).GeneratePdf();
    }

    private static void MontarCabecalho(IContainer container, RelatorioDTO relatorio)
    {
        container.Column(coluna =>
        {
            coluna.Item().Row(linha =>
            {
                linha.RelativeItem().Column(c =>
                {
                    c.Item().Text(relatorio.Titulo).FontSize(17).Bold().FontColor(Indigo);
                    c.Item().Text(relatorio.Subtitulo).FontSize(10).FontColor(Cinza);
                });

                linha.ConstantItem(150).AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text("Meta Real").FontSize(10).Bold();
                    c.Item().AlignRight().Text($"Gerado em {relatorio.GeradoEm:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(Cinza);
                });
            });

            coluna.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(Indigo);
        });
    }

    private static void MontarConteudo(IContainer container, RelatorioDTO relatorio)
    {
        container.Column(coluna =>
        {
            if (relatorio.Resumo.Count > 0)
            {
                coluna.Item().PaddingBottom(12).Row(linha =>
                {
                    foreach (var item in relatorio.Resumo)
                    {
                        linha.RelativeItem().PaddingRight(6).Background("#F1F5F9").Padding(8).Column(c =>
                        {
                            c.Item().Text(item.Rotulo).FontSize(7.5f).FontColor(Cinza);
                            c.Item().PaddingTop(2).Text(item.Valor).FontSize(11).Bold();
                        });
                    }
                });
            }

            if (relatorio.Linhas.Count == 0)
            {
                coluna.Item().PaddingTop(30).AlignCenter()
                    .Text("Nenhum dado encontrado para os filtros selecionados.")
                    .FontSize(11).FontColor(Cinza);
                return;
            }

            coluna.Item().Table(tabela =>
            {
                tabela.ColumnsDefinition(definicao =>
                {
                    foreach (var col in relatorio.Colunas)
                    {
                        definicao.RelativeColumn(col.Tipo == TipoColuna.Texto ? 2f : 1.2f);
                    }
                });

                tabela.Header(cabecalho =>
                {
                    foreach (var col in relatorio.Colunas)
                    {
                        cabecalho.Cell()
                            .Background(Indigo).Padding(5)
                            .Alignment(col.Tipo)
                            .Text(col.Titulo).FontColor(Colors.White).Bold().FontSize(8.5f);
                    }
                });

                var indice = 0;
                foreach (var registro in relatorio.Linhas)
                {
                    string fundo = indice++ % 2 == 0 ? "#FFFFFF" : "#F8FAFC";
                    foreach (var col in relatorio.Colunas)
                    {
                        tabela.Cell()
                            .Background(fundo).PaddingVertical(4).PaddingHorizontal(5)
                            .BorderBottom(0.5f).BorderColor("#E2E8F0")
                            .Alignment(col.Tipo)
                            .Text(Formatar(registro.GetValueOrDefault(col.Chave), col.Tipo)).FontSize(8);
                    }
                }
            });
        });
    }

    private static void MontarRodape(IContainer container)
    {
        container.AlignCenter().Text(texto =>
        {
            texto.DefaultTextStyle(t => t.FontSize(8).FontColor(Cinza));
            texto.Span("Página ");
            texto.CurrentPageNumber();
            texto.Span(" de ");
            texto.TotalPages();
        });
    }

    private static IContainer Alignment(this IContainer container, TipoColuna tipo) =>
        tipo == TipoColuna.Texto ? container.AlignLeft() : container.AlignRight();

    private static string Formatar(object? valor, TipoColuna tipo)
    {
        if (valor is null) return "—";

        return tipo switch
        {
            TipoColuna.Moeda => ParaDecimal(valor).ToString("C", PtBr),
            TipoColuna.Percentual => ParaDecimal(valor).ToString("P1", PtBr),
            TipoColuna.Numero => ParaDecimal(valor).ToString("0.##", PtBr),
            _ => valor.ToString() ?? "—"
        };
    }

    private static decimal ParaDecimal(object? valor) => valor switch
    {
        null => 0m,
        decimal d => d,
        int i => i,
        long l => l,
        double db => (decimal)db,
        _ => decimal.TryParse(valor.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m
    };
}
