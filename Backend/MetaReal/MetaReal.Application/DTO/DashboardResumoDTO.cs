using System.Text.Json.Serialization;

namespace MetaReal.Application.DTO
{
    public class DashboardResumoDTO
    {

        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        public int TotalVendedores { get; set; }

        public int TotalRegistros { get; set; }

        public decimal SomaPretasMistas { get; set; }
        public decimal SomaGarantia { get; set; }
        public decimal SomaCrediarioDujuca { get; set; }

        public int SomaQuantAtendimento { get; set; }
        public int SomaNumVendas { get; set; }
        public decimal SomaValorTotalVendas { get; set; }

        public decimal PercentualServicoGeral => SomaValorTotalVendas > 0 ? SomaGarantia / SomaValorTotalVendas : 0m;
        public decimal AproveitamentoGeral => SomaQuantAtendimento > 0 ? (decimal)SomaNumVendas / SomaQuantAtendimento : 0m;
        [JsonPropertyName("evolucaoTemporal")]
        public List<EvolucaoTemporalDTO> EvolucaoTemporal { get; set; }
        public List<RankingVendedorDTO> Ranking { get; set; } = new();
    }
}
