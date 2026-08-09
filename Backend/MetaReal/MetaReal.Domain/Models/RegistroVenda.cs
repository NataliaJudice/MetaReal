using System;

namespace MetaReal.Domain.Models
{
    public class RegistroVenda
    {
        public Guid IdRegistroVenda { get; set; }
        public DateTime Data { get; set; }
        public decimal PretasMistas { get; set; }
        public decimal Garantia { get; set; }
        public decimal CrediarioDujuca { get; set; }
        public int QuantAtendimento { get; set; }
        public int NumVendas { get; set; }
        public decimal ValorTotalVendas { get; set; }
        public Guid IdVendedor { get; set; }
        public Vendedor Vendedor { get; set; }
    }
}
