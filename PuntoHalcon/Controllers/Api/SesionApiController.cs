using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace PuntoHalcon.Controllers.Api;

// Uso de HttpContext.Session: recuerda la última categoría elegida en "Explorar" mientras dure la sesión.
// Solo se guarda un texto de una lista permitida (nada sensible). Al cerrar sesión se limpia y vuelve a "Todas".
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Estudiante")]
[IgnoreAntiforgeryToken]
public class SesionApiController : ControllerBase
{
    private const string ClaveFiltro = "filtroCategoria";
    private static readonly string[] Permitidas = { "Todas", "Biblioteca", "Cultura", "Tecnología", "Deportes", "Naturaleza" };

    [HttpGet("filtro")]
    public IActionResult Obtener() => Ok(new { categoria = HttpContext.Session.GetString(ClaveFiltro) ?? "Todas" });

    public record FiltroDto(string? Categoria);

    [HttpPost("filtro")]
    public IActionResult Guardar([FromBody] FiltroDto dto)
    {
        var categoria = (dto.Categoria ?? "").Trim();
        if (!Permitidas.Contains(categoria)) return BadRequest(new { mensaje = "Categoría no válida." });
        HttpContext.Session.SetString(ClaveFiltro, categoria);
        return Ok(new { categoria });
    }
}
