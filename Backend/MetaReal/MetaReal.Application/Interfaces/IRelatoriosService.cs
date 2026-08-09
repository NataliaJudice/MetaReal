using MetaReal.Application.DTO;

namespace MetaReal.Application.Interfaces
{
    public class FiltroRelatorio
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public Guid? VendedorId { get; set; }
        public string? Agrupamento { get; set; }
    }

    public interface IRelatoriosService
    {
        Task<RelatorioDTO> Gerar(string chave, FiltroRelatorio filtro);
    }
}
