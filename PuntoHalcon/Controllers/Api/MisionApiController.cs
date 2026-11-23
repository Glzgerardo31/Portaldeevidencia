using Aplicacion.Interfaces;
using Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace PuntoHalcon.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MisionApiController : ControllerBase
    {
        private readonly IMisionService _misionService;
        private readonly PuntoHalconDbContext _db;

        public MisionApiController(IMisionService misionService, PuntoHalconDbContext db)
        {
            _misionService = misionService;
            _db = db;
        }



        // Misiones publicadas para el estudiante autenticado.
        [HttpGet("disponibles")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> ObtenerDisponibles()
        {
            var rows = await _db.Misiones.Where(m => !m.EsPropuesta)
                .OrderBy(m => m.MisionId)
                .Select(m => new { m.MisionId, m.Nombre, m.Descripcion, m.Lugar, Categoria = m.Categoria.ToString(), Dificultad = m.Dificultad.ToString() })
                .ToListAsync();
            return Ok(rows);
        }

        // ====================================================
        // GET: api/MisionApi
        // ====================================================

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<Mision>>> ObtenerTodos()
        {
            var lista = await _misionService.ObtenerTodosAsync();

            return Ok(lista);
        }


        // ====================================================
        // GET: api/MisionApi/5
        // ====================================================

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Mision>> ObtenerPorId(int id)
        {
            var mision = await _misionService.ObtenerPorIdAsync(id);

            if (mision == null)
            {
                return NotFound();
            }

            return Ok(mision);
        }


        // ====================================================
        // POST: api/MisionApi
        // ====================================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Agregar(Mision mision)
        {
            await _misionService.AgregarAsync(mision);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = mision.MisionId },
                mision
            );
        }


        // ====================================================
        // PUT: api/MisionApi/5
        // ====================================================

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Actualizar(int id, Mision mision)
        {
            if (id != mision.MisionId)
            {
                return BadRequest(
                    "El Id de la URL no coincide con el Id de la misión."
                );
            }

            var existente = await _misionService.ObtenerPorIdAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            await _misionService.ActualizarAsync(mision);

            return NoContent();
        }


        // ====================================================
        // DELETE: api/MisionApi/5
        // ====================================================

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var mision = await _misionService.ObtenerPorIdAsync(id);

            if (mision == null)
            {
                return NotFound();
            }

            await _misionService.EliminarAsync(id);

            return NoContent();
        }
    }
}
