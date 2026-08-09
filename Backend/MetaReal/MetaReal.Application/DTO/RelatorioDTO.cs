using System.Text.Json.Serialization;
namespace MetaReal.Application.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoColuna
    {
        Texto,
        Moeda,
        Numero,
        Percentual,
        Data
    }

    public class ColunaRelatorio
    {
        public string Chave { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public TipoColuna Tipo { get; set; } = TipoColuna.Texto;
        public ColunaRelatorio() { }


        public ColunaRelatorio(string chave, string titulo, TipoColuna tipo = TipoColuna.Texto)
        {

            Chave = chave;
            Titulo = titulo;
            Tipo = tipo;
        }
    }

    public class ResumoRelatorio
    {


        public string Rotulo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;


        public ResumoRelatorio() { }

        public ResumoRelatorio(string rotulo, string valor)
        {
            Rotulo = rotulo;
            Valor = valor;
        }
    }

    public class RelatorioDTO
    {
        public string Chave { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;

        public string Subtitulo { get; set; } = string.Empty;
        public DateTime GeradoEm { get; set; } = DateTime.Now;
        public List<ColunaRelatorio> Colunas { get; set; } = new();
        public List<Dictionary<string, object?>> Linhas { get; set; } = new();
        public List<ResumoRelatorio> Resumo { get; set; } = new();
    }
}
