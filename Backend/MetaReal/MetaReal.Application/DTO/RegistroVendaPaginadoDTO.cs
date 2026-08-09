namespace MetaReal.Application.DTO
{
    public class RegistroVendaPaginadoDTO
    {
        public int TotalRegistros { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public decimal? ValorTotal { get; set; }
        public int? TotalVendas { get; set; }
        public int? TotalAtendimentos { get; set; }
        public decimal? ConversaoMedia { get; set; }
        public IEnumerable<RegistroVendaDTO> Items { get; set; } = Enumerable.Empty<RegistroVendaDTO>();
       
    }
}
