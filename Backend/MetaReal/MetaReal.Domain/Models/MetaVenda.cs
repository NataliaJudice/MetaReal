using System;

namespace MetaReal.Domain.Models
{
    public class MetaVenda
    {
        public Guid IdMetaVenda { get; set; }
        public Guid IdVendedor { get; set; }
        public Vendedor Vendedor { get; set; }

        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal ValorMeta { get; set; }

        public bool NotificadoConclusao { get; set; }

        public DateTime CriadoEm { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }
}
