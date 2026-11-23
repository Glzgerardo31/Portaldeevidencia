using Dominio.Entidades;
using Dominio.Enum;
using Infraestructura.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace PuntoHalcon.Controllers;

// Un estudiante propone UNA misión y, junto con ella, la insignia que se obtiene al completarla.
// El administrador revisa las dos cosas juntas, puede modificarlas y agrega el código de la misión.
[Route("api/propuestas")]
[ApiController]
[Authorize(Roles = "Estudiante")]
[IgnoreAntiforgeryToken]
public class PropuestasController : ControllerBase
{
    private readonly PuntoHalconDbContext _db;
    public PropuestasController(PuntoHalconDbContext db) => _db = db;

    [HttpPost("mision")]
    public async Task<IActionResult> Mision([FromBody] PropuestaDto dto)
    {
        var nombre = (dto.Nombre ?? "").Trim();
        var descripcion = (dto.Descripcion ?? "").Trim();
        var lugar = (dto.Lugar ?? "").Trim();
        var insigniaNombre = (dto.InsigniaNombre ?? "").Trim();
        var icono = string.IsNullOrWhiteSpace(dto.InsigniaIcono) ? "🏅" : dto.InsigniaIcono.Trim();

        // Los límites coinciden con las columnas de la base de datos (antes un nombre largo provocaba error 500).
        if (nombre.Length < 3 || nombre.Length > 40) return BadRequest(new { mensaje = "El nombre de la misión debe tener entre 3 y 40 caracteres." });
        if (descripcion.Length < 10 || descripcion.Length > 120) return BadRequest(new { mensaje = "La descripción debe tener entre 10 y 120 caracteres." });
        if (lugar.Length < 3 || lugar.Length > 40) return BadRequest(new { mensaje = "El lugar debe tener entre 3 y 40 caracteres." });
        if (insigniaNombre.Length < 3 || insigniaNombre.Length > 60) return BadRequest(new { mensaje = "El nombre de la insignia debe tener entre 3 y 60 caracteres." });
        if (icono.Length > 10) return BadRequest(new { mensaje = "El icono de la insignia no es válido." });

        var categoria = dto.Categoria switch
        {
            "Biblioteca" => CategoriaMision.Biblioteca,
            "Cultura" => CategoriaMision.Cultura,
            "Tecnología" or "Tecnologia" => CategoriaMision.Tecnologia,
            "Deportes" => CategoriaMision.Deportes,
            "Naturaleza" => CategoriaMision.Naturaleza,
            "Otra" or "Comunidad" => CategoriaMision.Otra,
            _ => (CategoriaMision?)null
        };
        if (categoria == null) return BadRequest(new { mensaje = "Selecciona una categoría válida." });

        var dificultad = dto.Dificultad switch
        {
            "Fácil" or "Facil" => DificultadMision.Facil,
            "Media" => DificultadMision.Media,
            "Difícil" or "Dificil" => DificultadMision.Dificil,
            _ => (DificultadMision?)null
        };
        if (dificultad == null) return BadRequest(new { mensaje = "Selecciona la dificultad." });

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var etiqueta = categoria.Value == CategoriaMision.Tecnologia ? "TECNOLOGÍA" : categoria.Value.ToString().ToUpperInvariant();

        var mision = new Mision
        {
            UsuarioId = usuarioId, Nombre = nombre, Descripcion = descripcion, Categoria = categoria.Value,
            Lugar = lugar, Dificultad = dificultad.Value, EsPropuesta = true, FechaCreacion = DateTime.UtcNow
        };
        // La insignia queda ligada a la misión; el código lo asigna el administrador al revisarla.
        var insignia = new Insignia
        {
            UsuarioId = usuarioId, Mision = mision, Nombre = insigniaNombre,
            Descripcion = $"Completa la misión «{nombre}».", Etiqueta = etiqueta, Icono = icono,
            EsPropuesta = true
        };
        _db.Misiones.Add(mision);
        _db.Insignias.Add(insignia);
        try { await _db.SaveChangesAsync(); } // una sola transacción: se guardan las dos o ninguna
        catch (DbUpdateException) { return StatusCode(500, new { mensaje = "No se pudo guardar la propuesta. Intenta de nuevo." }); }

        return Ok(new { mensaje = "Propuesta enviada para revisión." });
    }

    // Estado de las propuestas del estudiante: en revisión, aceptada o rechazada (con la nota del administrador).
    [HttpGet("mias")]
    public async Task<IActionResult> Mias()
    {
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var filas = await _db.Misiones.Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaCreacion)
            .Select(m => new
            {
                m.MisionId,
                m.Nombre,
                Insignia = m.Insignias.Select(i => i.Nombre).FirstOrDefault(),
                Estado = !m.EsPropuesta ? "Aceptada" : (m.Rechazada ? "Rechazada" : "EnRevision"),
                Nota = m.NotaAdmin
            }).ToListAsync();
        return Ok(filas);
    }

    public record PropuestaDto(string? Nombre, string? Descripcion, string? Categoria, string? Lugar, string? Dificultad, string? InsigniaNombre, string? InsigniaIcono);
}
