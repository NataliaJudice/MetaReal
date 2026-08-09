using MetaReal.Application;
using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaReal.API.Controllers;

[ApiController]
[Route("api/vendas")]
[Authorize]
public class VendasController : ControllerBase
{

    private readonly IVendasService _vendasService;
    public VendasController(IVendasService vendasService)
    {
        _vendasService = vendasService;
    }
    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromQuery] Guid? vendedorId, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim,   [FromQuery] int pagina = 1,[FromQuery] int tamanhoPagina = 10)
    {
       // var limitePagina = 100;
        var registros = await _vendasService.ObterTodos(vendedorId, dataInicio, dataFim, pagina, tamanhoPagina);

        return Ok(new { success = true, data = registros });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var registro = await _vendasService.ObterPorId(id);
            if (registro == null)
            {

                return NotFound(new { success = false, error = "Registro de venda não encontrado." });
            }
            return Ok(new { success = true, data = registro });
        }
        catch (AcessoNegadoException ex)
        {

            return StatusCode(403, new { success = false, error = ex.Message });
        }
    }
    [HttpPost]
    public async Task<IActionResult> Adicionar(RegistroVendaEntradaDTO request)
    {
        try
        {
            var registro = await _vendasService.Adicionar(request);

            return CreatedAtAction(nameof(ObterPorId), new { id = registro.Id }, new { success = true, data = registro });
        }
        catch (AcessoNegadoException ex)
        {
            return StatusCode(403, new { success = false, error = ex.Message });

        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (ConflitoException ex)
        {
            return Conflict(new { success = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }

        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, RegistroVendaEntradaDTO request)
    {
        try
        {
            var registro = await _vendasService.Editar(id, request);
            return Ok(new { success = true, data = registro });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (AcessoNegadoException ex)
        {

            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, error = ex.Message });
        }
        catch (ConflitoException ex)
        {
            return Conflict(new { success = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        try
        {
            await _vendasService.Deletar(id);

            return NoContent();
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
