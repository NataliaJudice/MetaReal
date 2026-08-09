using MetaReal.Application;
using MetaReal.Application.DTO;
using MetaReal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaReal.API.Controllers;

[ApiController]
[Route("api/metas")]
[Authorize]
public class MetasController : ControllerBase
{
    private readonly IMetaService _metaService;

    public MetasController(IMetaService metaService)
    {
        _metaService = metaService;
    }

    [HttpPost]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> DefinirMeta(MetaEntradaDTO request)
    {
        try
        {
            var progresso = await _metaService.DefinirMeta(request);
            return Ok(new { success = true, data = progresso });
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

    [HttpPost("lote")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> DefinirMetaParaTodos(MetaLoteEntradaDTO request)
    {
        try
        {
            var progressos = await _metaService.DefinirMetaParaTodos(request);

            return Ok(new { success = true, data = progressos });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("vendedor/{vendedorId:guid}")]
    public async Task<IActionResult> ObterProgresso(Guid vendedorId, [FromQuery] int? mes, [FromQuery] int? ano)
    {
        try
        {
            var progresso = await _metaService.ObterProgresso(vendedorId, mes, ano);

            return Ok(new { success = true, data = progresso });
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

    [HttpGet("vendedor/{vendedorId:guid}/historico")]
    public async Task<IActionResult> ObterHistorico(Guid vendedorId, [FromQuery] int meses = 6)
    {
        try
        {
            var historico = await _metaService.ObterHistorico(vendedorId, meses);

            return Ok(new { success = true, data = historico });
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

    [HttpGet]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> ObterProgressoGeral([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var hoje = DateTime.Now;

        var progresso = await _metaService.ObterProgressoGeral(mes, ano);
        return Ok(new { success = true, data = progresso });
    }

}
