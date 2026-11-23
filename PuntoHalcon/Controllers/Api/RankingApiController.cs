using Infraestructura.Data;
using Dominio.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoHalcon.Helpers;
namespace PuntoHalcon.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Estudiante")]
public class RankingApiController : ControllerBase
{
    private readonly PuntoHalconDbContext _db;
    public RankingApiController(PuntoHalconDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var estudiantes = await _db.Usuarios.Where(u => u.Rol == "Estudiante")
            .Select(u => new { u.UsuarioId, u.Nombre, u.Apodo }).ToListAsync();
        var completadas = await _db.Progresos
            .Where(p => p.Estado == EstadoProgreso.Completada && p.UsuarioId > 0)
            .Select(p => new { p.UsuarioId, p.MisionId }).ToListAsync();
        var insignias = await _db.Insignias.Publicadas().ToListAsync();

        var misionesPorUsuario = completadas.GroupBy(p => p.UsuarioId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.MisionId).ToHashSet());

        var filas = estudiantes.Select(u =>
        {
            var hechas = misionesPorUsuario.TryGetValue(u.UsuarioId, out var set) ? set : new HashSet<int>();
            var obtenidas = insignias.Count(i => Logros.Desbloqueada(i, hechas));
            return new
            {
                u.UsuarioId,
                Nombre = string.IsNullOrWhiteSpace(u.Apodo) ? u.Nombre : u.Apodo,
                Misiones = hechas.Count,
                Insignias = obtenidas,
                Puntos = hechas.Count * 100 + obtenidas * 25
            };
        })
        .Where(x => x.Misiones > 0 || x.Insignias > 0)
        .OrderByDescending(x => x.Misiones).ThenByDescending(x => x.Insignias).ThenByDescending(x => x.Puntos)
        .ToList();
        return Ok(filas);
    }
}
