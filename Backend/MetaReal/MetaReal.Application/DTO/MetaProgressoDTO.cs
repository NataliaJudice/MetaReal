namespace MetaReal.Application.DTO
{
    public class MetaProgressoDTO
    {
        public Guid? Id { get; set; }
        public Guid VendedorId { get; set; }
        public string VendedorNome { get; set; } = string.Empty;
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal ValorMeta { get; set; }
        public decimal ValorAtual { get; set; }

        public decimal PercentualConcluido => ValorMeta > 0 ? ValorAtual / ValorMeta : 0m;
        public bool Concluida => ValorMeta > 0 && ValorAtual >= ValorMeta;
    }
}
