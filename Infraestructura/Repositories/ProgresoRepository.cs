using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class ProgresoRepository : IProgresoRepository
    {
        private readonly PuntoHalconDbContext _context;

        public ProgresoRepository(PuntoHalconDbContext context)
        {
            _context = context;
        }

        public async Task ActualizarAsync(Progreso progreso)
        {
            _context.Progresos.Update(progreso);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarAsync(Progreso progreso)
        {
            await _context.Progresos.AddAsync(progreso);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var progreso = await _context.Progresos.FirstOrDefaultAsync(x => x.ProgresoId == id);
            if (progreso != null)
            {
                _context.Progresos.Remove(progreso);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Progreso?> ObtenerPorIdAsync(int id)
        {
            return await _context.Progresos.AsNoTracking().FirstOrDefaultAsync(x => x.ProgresoId == id);
        }

        public async Task<List<Progreso>> ObtenerTodosAsync()
        {
            return await _context.Progresos.OrderBy(x => x.ProgresoId).ToListAsync();
        }
    }
}
