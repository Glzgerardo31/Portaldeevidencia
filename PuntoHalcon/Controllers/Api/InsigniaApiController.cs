using Aplicacion.Interfaces;
using Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;
using Dominio.Enum;
using PuntoHalcon.Helpers;
using System.Security.Claims;

namespace PuntoHalcon.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsigniaApiController : ControllerBase
    {
        private readonly IInsigniaService _insigniaService;
        private readonly PuntoHalconDbContext _db;

        public InsigniaApiController(IInsigniaService insigniaService, PuntoHalconDbContext db)
        {
            _insigniaService = insigniaService;
            _db = db;
        }



        // Insignias publicadas con el estado de desbloqueo de la cuenta que consulta.
        [HttpGet("disponibles")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> ObtenerDisponibles()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var completadas = await _db.Progresos
                .Where(p => p.UsuarioId == usuarioId && p.Estado == EstadoProgreso.Completada)
                .Select(p => p.MisionId).ToListAsync();
            var insignias = await _db.Insignias.Publicadas().OrderBy(i => i.InsigniaId).ToListAsync();
            var resultado = insignias.Select(i => new
            {
                i.InsigniaId, i.MisionId, i.Nombre, i.Descripcion, i.Etiqueta, i.Icono,
                Desbloqueada = Logros.Desbloqueada(i, completadas)
            });
            return Ok(resultado);
        }

        // ====================================================
        // GET: api/InsigniaApi
        // ====================================================

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<Insignia>>> ObtenerTodos()
        {
            var lista = await _insigniaService.ObtenerTodosAsync();

            return Ok(lista);
        }


        // ====================================================
        // GET: api/InsigniaApi/5
        // ====================================================

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Insignia>> ObtenerPorId(int id)
        {
            var insignia = await _insigniaService.ObtenerPorIdAsync(id);

            if (insignia == null)
            {
                return NotFound();
            }

            return Ok(insignia);
        }


        // ====================================================
        // POST: api/InsigniaApi
        // ====================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Agregar(Insignia insignia)
        {
            await _insigniaService.AgregarAsync(insignia);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = insignia.InsigniaId },
                insignia
            );
        }


        // ====================================================
        // PUT: api/InsigniaApi/5
        // ====================================================

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Actualizar(int id, Insignia insignia)
        {
            if (id != insignia.InsigniaId)
            {
                return BadRequest(
                    "El Id de la URL no coincide con el Id de la insignia."
                );
            }

            var existente = await _insigniaService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            await _insigniaService.ActualizarAsync(insignia);

            return NoContent();
        }


        // ====================================================
        // DELETE: api/InsigniaApi/5
        // ====================================================

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var insignia = await _insigniaService.ObtenerPorIdAsync(id);

            if (insignia == null)
            {
                return NotFound();
            }

            await _insigniaService.EliminarAsync(id);

            return NoContent();
        }
    }
}
