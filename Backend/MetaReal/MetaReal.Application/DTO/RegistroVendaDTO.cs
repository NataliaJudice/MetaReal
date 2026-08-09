namespace MetaReal.Application.DTO
{
    public class RegistroVendaDTO
    {
        public Guid Id { get; set; }
        public DateTime Data { get; set; }
        public decimal PretasMistas { get; set; }
        public decimal Garantia { get; set; }
        public decimal CrediarioDujuca { get; set; }
        public int QuantAtendimento { get; set; }
        public int NumVendas { get; set; }
        public decimal ValorTotalVendas { get; set; }
        public Guid VendedorId { get; set; }
        public string VendedorNome { get; set; } = string.Empty;

        public decimal PercentualServico => ValorTotalVendas > 0 ? Garantia / ValorTotalVendas : 0m;


        public decimal Aproveitamento => QuantAtendimento > 0 ? (decimal)NumVendas / QuantAtendimento : 0m;
    }
}
