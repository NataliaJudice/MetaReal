using MetaReal.API.Relatorios;
using MetaReal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaReal.API.Controllers;

[ApiController]
[Route("api/relatorios")]
[Authorize]
public class RelatoriosController : ControllerBase
{

    private readonly IRelatoriosService _relatoriosService;

    public RelatoriosController(IRelatoriosService relatoriosService)
    {
        _relatoriosService = relatoriosService;
    }

    [HttpGet("{chave}")]
    public async Task<IActionResult> Obter(string chave, [FromQuery] FiltroRelatorio filtro)
    {
        try
        {

            var relatorio = await _relatoriosService.Gerar(chave, filtro);
            return Ok(new { success = true, data = relatorio });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("{chave}/excel")]
    public async Task<IActionResult> Excel(string chave, [FromQuery] FiltroRelatorio filtro)
    {
        try
        {
            var relatorio = await _relatoriosService.Gerar(chave, filtro);
            var arquivo = ExportadorExcel.Gerar(relatorio);

            return File(arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                MontarNomeArquivo(chave, "xlsx"));
        }

        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("{chave}/pdf")]
    public async Task<IActionResult> Pdf(string chave, [FromQuery] FiltroRelatorio filtro) {
        try
        {
            var relatorio = await _relatoriosService.Gerar(chave, filtro);

            var arquivo = ExportadorPdf.Gerar(relatorio);
            var nome = $"{chave}-{DateTime.Now:yyyy-MM-dd-HHmm}.pdf";

            return File(arquivo, "application/pdf", nome);

        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
    }

    private static string MontarNomeArquivo(string chave, string extensao) {
        return $"{chave}-{DateTime.Now:yyyy-MM-dd-HHmm}.{extensao}";

    }
}
