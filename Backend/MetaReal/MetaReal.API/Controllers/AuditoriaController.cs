using System.Text;
using MetaReal.Application.Interfaces;
using MetaReal.Application.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaReal.API.Controllers;


[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "Gerente")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditService;

    public AuditoriaController(IAuditoriaService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, [FromQuery] string? entidade = null) {

        

        var resultado = await _auditService.ObterPaginado(pagina, tamanhoPagina, entidade);
        return Ok(new { success = true, data = resultado });
    }
}
