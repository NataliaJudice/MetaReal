namespace MetaReal.Application.DTO
{


    public class MetaEntradaDTO
    {
        public Guid VendedorId { get; set; }
        public int Mes { get; set; } // 1-12
        public int Ano { get; set; }
        public decimal ValorMeta { get; set; }
    }
}
