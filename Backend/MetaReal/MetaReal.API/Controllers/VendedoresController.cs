using MetaReal.Application;
using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MetaReal.API.Controllers;

[ApiController]
[Route("api/vendedores")]
[Authorize]
public class VendedoresController : ControllerBase
{

    private readonly IVendedoresService _vendedoresService;
    private readonly IVendasService _vendasService;

    public VendedoresController(IVendedoresService vendedoresService, IVendasService vendasService)
    {
        _vendedoresService = vendedoresService;
        _vendasService = vendasService;
    }

    private static string MontarNomeExibicao(string nome, bool ativo)
    {
        if (!ativo)
        {
            return nome + " (inativo)";
        }

        return nome;
    }

    [HttpGet]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> ObterTodos()
    {
        var lista = await _vendedoresService.ObterTodos();

        return Ok(new { success = true, data = lista });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {

        try
        {
            var vendedor = await _vendedoresService.ObterPorId(id);
            if (vendedor == null)
            {
                return NotFound(new { success = false, error = "Vendedor não encontrado." });
            }

            return Ok(new { success = true, data = vendedor });
        }


        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> Adicionar(VendedorEntradaDTO request)
    {
        try
        {
            var vendedor = await _vendedoresService.Adicionar(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = vendedor.Id }, new { success = true, data = vendedor });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> Editar(Guid id, VendedorEntradaDTO request)
    {
        try
        {
            var vendedor = await _vendedoresService.Editar(id, request);

            return Ok(new { success = true, data = vendedor });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        try
        {
            await _vendedoresService.Deletar(id);

            return NoContent();
        }

        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/perfil")]
    public async Task<IActionResult> ObterPerfil( Guid id, [FromQuery] DateTime? dataInicio,[FromQuery] DateTime? dataFim, [FromQuery] int pagina = 1)
    {
        try
        {
            var perfil = await _vendasService.ObterPerfilVendedor(id, dataInicio, dataFim, pagina, 10);

            return Ok(new { success = true, data = perfil });
        }
        catch (AcessoNegadoException ex)
        {
            return StatusCode(403, new { success = false, error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
    }
}
