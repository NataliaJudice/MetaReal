using MetaReal.Application.DTO;

namespace MetaReal.Application.Interfaces
{
    public interface IVendedoresService
    {
        Task<IEnumerable<VendedorDTO>> ObterTodos();

        Task<VendedorDTO?> ObterPorId(Guid id);

        Task<VendedorDTO> Adicionar(VendedorEntradaDTO request);
        Task<VendedorDTO> Editar(Guid id, VendedorEntradaDTO request);

        Task Deletar(Guid id);

    }
}
