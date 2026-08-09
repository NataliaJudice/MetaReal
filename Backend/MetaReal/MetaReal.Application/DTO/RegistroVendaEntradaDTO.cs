namespace MetaReal.Application.DTO
{
    public class RegistroVendaEntradaDTO
    {
        public DateTime Data { get; set; }
        public Guid VendedorId { get; set; }
        public decimal PretasMistas { get; set; }
        public decimal Garantia { get; set; }

        public decimal CrediarioDujuca { get; set; }
        public int QuantAtendimento { get; set; }
        public int NumVendas { get; set; }
        public decimal ValorTotalVendas { get; set; }
    }
}
