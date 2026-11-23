using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class MisionRepository : IMisionRepository
    {
        private readonly PuntoHalconDbContext _context;

        public MisionRepository(PuntoHalconDbContext context)
        {
            _context = context;
        }

        public async Task ActualizarAsync(Mision mision)
        {
            _context.Misiones.Update(mision);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarAsync(Mision mision)
        {
            await _context.Misiones.AddAsync(mision);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var mision = await _context.Misiones.FirstOrDefaultAsync(x => x.MisionId == id);
            if (mision != null)
            {
                _context.Misiones.Remove(mision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Mision?> ObtenerPorIdAsync(int id)
        {
            return await _context.Misiones.AsNoTracking().FirstOrDefaultAsync(x => x.MisionId == id);
        }

        public async Task<List<Mision>> ObtenerTodosAsync()
        {
            return await _context.Misiones.OrderBy(x => x.MisionId).ToListAsync();
        }
    }
}
