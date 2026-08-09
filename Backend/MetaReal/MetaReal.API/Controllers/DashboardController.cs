using MetaReal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaReal.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Gerente")]
public class DashboardController : ControllerBase
{
    private readonly IVendasService _vendasService;
    //private readonly int _diasPadrao = 30;

    public DashboardController(IVendasService vendasService)
    {
        _vendasService = vendasService;
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        var resumo = await _vendasService.ObterResumoGeral(dataInicio, dataFim);

        var resposta = new { success = true, data = resumo };

        return Ok(resposta);
    }
}
