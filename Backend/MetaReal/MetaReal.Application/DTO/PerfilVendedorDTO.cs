namespace MetaReal.Application.DTO
{
    public class PerfilVendedorDTO
    {
        public VendedorDTO Vendedor { get; set; } = new();

        public DateTime? DataInicio { get; set; }


        public DateTime? DataFim { get; set; }
        public decimal SomaPretasMistas { get; set; }
        public decimal SomaGarantia { get; set; }
        public decimal SomaCrediarioDujuca { get; set; }
        public int SomaQuantAtendimento { get; set; }
        public int SomaNumVendas { get; set; }
        public decimal SomaValorTotalVendas { get; set; }

        public decimal PercentualServico => SomaValorTotalVendas > 0 ? SomaGarantia / SomaValorTotalVendas : 0m;
        public decimal Aproveitamento => SomaQuantAtendimento > 0 ? (decimal)SomaNumVendas / SomaQuantAtendimento : 0m;

        public int TotalRegistrosPeriodo { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public List<RegistroVendaDTO> Registros { get; set; } = new();
    }
}
