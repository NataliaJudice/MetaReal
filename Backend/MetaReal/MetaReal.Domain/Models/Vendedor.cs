using System;
using System.Collections.Generic;

namespace MetaReal.Domain.Models
{
    public class Vendedor
    {
        public Guid IdVendedor { get; set; }
        public string Nome { get; set; }
        public ICollection<RegistroVenda> RegistrosVenda { get; set; }
    }
}
