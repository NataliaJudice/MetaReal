namespace MetaReal.Application.DTO
{


    public class RankingVendedorDTO
    {
        public int Posicao { get; set; }
        public Guid VendedorId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorTotalVendas { get; set; }
        public int NumVendas { get; set; }
        public int QuantAtendimento { get; set; }

        public decimal Aproveitamento => QuantAtendimento > 0 ? (decimal)NumVendas / QuantAtendimento : 0m;
        public decimal ParticipacaoPercentual { get; set; }
    }
}
