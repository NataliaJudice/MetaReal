namespace MetaReal.Application.DTO
{
    public class AuditoriaPaginadaDTO
    {
        public int TotalRegistros { get; set; }
        public int PaginaAtual { get; set; }

        public int TotalPaginas { get; set; }
        public IEnumerable<AuditoriaDTO> Items { get; set; } = Enumerable.Empty<AuditoriaDTO>();

    }
}
