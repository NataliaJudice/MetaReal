using ClosedXML.Excel;
using MetaReal.Application.DTO;

namespace MetaReal.API.Relatorios;

public static class ExportadorExcel
{
    public static byte[] Gerar(RelatorioDTO relatorio)
    {
        using var workbook = new XLWorkbook();
        var aba = workbook.Worksheets.Add("Relatório");
        var totalColunas = Math.Max(relatorio.Colunas.Count, 1);
        var linha = 1;

        aba.Cell(linha, 1).Value = relatorio.Titulo;
        aba.Range(linha, 1, linha, totalColunas).Merge();
        aba.Cell(linha, 1).Style.Font.SetBold().Font.SetFontSize(16);
        linha++;

        aba.Cell(linha, 1).Value = relatorio.Subtitulo;
        aba.Range(linha, 1, linha, totalColunas).Merge();
        aba.Cell(linha, 1).Style.Font.SetItalic().Font.SetFontColor(XLColor.Gray);
        linha++;

        aba.Cell(linha, 1).Value = $"Gerado em {relatorio.GeradoEm:dd/MM/yyyy HH:mm}";
        aba.Range(linha, 1, linha, totalColunas).Merge();
        aba.Cell(linha, 1).Style.Font.SetFontSize(9).Font.SetFontColor(XLColor.Gray);
        linha += 2;

        if (relatorio.Resumo.Count > 0)
        {
            foreach (var item in relatorio.Resumo)
            {
                aba.Cell(linha, 1).Value = item.Rotulo;
                aba.Cell(linha, 1).Style.Font.SetBold();
                aba.Cell(linha, 2).Value = item.Valor;
                linha++;
            }
            linha++;
        }

        var linhaCabecalho = linha;
        for (var c = 0; c < relatorio.Colunas.Count; c++)
        {
            var celula = aba.Cell(linha, c + 1);
            celula.Value = relatorio.Colunas[c].Titulo;
            celula.Style.Font.SetBold().Font.SetFontColor(XLColor.White);
            celula.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#4F46E5"));
            celula.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }
        linha++;

        foreach (var registro in relatorio.Linhas)
        {
            for (var c = 0; c < relatorio.Colunas.Count; c++)
            {
                var coluna = relatorio.Colunas[c];
                var celula = aba.Cell(linha, c + 1);
                var valor = registro.GetValueOrDefault(coluna.Chave);

                switch (coluna.Tipo)
                {
                    case TipoColuna.Moeda:
                        celula.Value = ParaDecimal(valor);
                        celula.Style.NumberFormat.Format = "R$ #,##0.00";
                        break;
                    case TipoColuna.Percentual:
                        celula.Value = ParaDecimal(valor);
                        celula.Style.NumberFormat.Format = "0.0%";
                        break;
                    case TipoColuna.Numero:
                        celula.Value = ParaDecimal(valor);
                        break;
                    case TipoColuna.Data:
                        celula.Value = valor?.ToString() ?? "";
                        break;
                    default:
                        celula.Value = valor?.ToString() ?? "";
                        break;
                }
            }
            linha++;
        }

        if (relatorio.Linhas.Count > 0)
        {
            aba.Range(linhaCabecalho, 1, linha - 1, totalColunas).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            aba.Range(linhaCabecalho, 1, linha - 1, totalColunas).Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);
        }

        aba.Columns().AdjustToContents();
        aba.SheetView.FreezeRows(linhaCabecalho);

        using var memoria = new MemoryStream();
        workbook.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static decimal ParaDecimal(object? valor) => valor switch
    {
        null => 0m,
        decimal d => d,
        int i => i,
        long l => l,
        double db => (decimal)db,
        _ => decimal.TryParse(valor.ToString(), out var parsed) ? parsed : 0m
    };
}
