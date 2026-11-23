using Dominio.Entidades;
using Dominio.Enum;
using Infraestructura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PuntoHalcon.Helpers;

namespace PuntoHalcon.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Estudiante")]
[IgnoreAntiforgeryToken]
public class ProgresoApiController : ControllerBase
{
    private readonly PuntoHalconDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly LimiteIntentos _limite;
    public ProgresoApiController(PuntoHalconDbContext db, IConfiguration configuration, LimiteIntentos limite)
    { _db = db; _configuration = configuration; _limite = limite; }

    private int UsuarioActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ObtenerMisProgresos()
    {
        var id = UsuarioActual;
        var rows = await _db.Progresos.Where(p => p.UsuarioId == id)
            .Select(p => new { p.MisionId, Estado = p.Estado.ToString(), p.FechaCompletada }).ToListAsync();
        return Ok(rows);
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> Pendientes()
    {
        var id = UsuarioActual;
        var completadas = _db.Progresos.Where(p => p.UsuarioId == id && p.Estado == EstadoProgreso.Completada).Select(p => p.MisionId);
        var rows = await _db.Misiones.Where(m => !m.EsPropuesta && !completadas.Contains(m.MisionId))
            .Select(m => new { m.MisionId, m.Nombre, m.Descripcion, m.Lugar, Categoria = m.Categoria.ToString(), Dificultad = m.Dificultad.ToString() }).ToListAsync();
        return Ok(rows);
    }

    public record CompletarDto(int MisionId, string Codigo);
    [HttpPost("completar")]
    public async Task<IActionResult> Completar([FromBody] CompletarDto dto)
    {
        // Límite de intentos: evita adivinar códigos probando uno tras otro.
        var clave = $"codigo:{UsuarioActual}";
        if (_limite.Bloqueado(clave, out var espera))
            return StatusCode(StatusCodes.Status429TooManyRequests, new { mensaje = $"Demasiados intentos con código incorrecto. Intenta de nuevo en {espera} minuto(s)." });
        var mision = await _db.Misiones.FirstOrDefaultAsync(m => m.MisionId == dto.MisionId && !m.EsPropuesta);
        if (mision == null) return NotFound(new { mensaje = "La misión no está disponible." });
        // Cada misión tiene su propio código (lo define el administrador). Si una misión antigua no tiene, se usa el de appsettings.
        var esperado = string.IsNullOrWhiteSpace(mision.Codigo) ? (_configuration["Missions:VerificationCode"] ?? "HALCON2026") : mision.Codigo.Trim();
        if (!string.Equals(dto.Codigo?.Trim(), esperado, StringComparison.OrdinalIgnoreCase))
        { _limite.RegistrarFallo(clave); return BadRequest(new { mensaje = "El código de verificación no es correcto." }); }
        _limite.Reiniciar(clave);
        var usuarioId = UsuarioActual;
        var progreso = await _db.Progresos.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.MisionId == dto.MisionId);
        if (progreso?.Estado == EstadoProgreso.Completada)
            return Conflict(new { mensaje = "Ya completaste esta misión." });
        if (progreso == null)
        {
            progreso = new Progreso { UsuarioId = usuarioId, MisionId = dto.MisionId, Estado = EstadoProgreso.Completada, FechaCompletada = DateTime.UtcNow };
            _db.Progresos.Add(progreso);
        }
        else { progreso.Estado = EstadoProgreso.Completada; progreso.FechaCompletada = DateTime.UtcNow; }
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "No se pudo registrar el avance. Es posible que la misión ya estuviera completada." }); }
        return Ok(new { mensaje = "¡Misión completada!", misionId = dto.MisionId });
    }
}
