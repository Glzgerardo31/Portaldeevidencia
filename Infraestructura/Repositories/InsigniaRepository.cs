using Dominio.Entidades;
using Dominio.Interfaces;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories
{
    public class InsigniaRepository : IInsigniaRepository
    {
        private readonly PuntoHalconDbContext _context;

        public InsigniaRepository(PuntoHalconDbContext context)
        {
            _context = context;
        }

        public async Task ActualizarAsync(Insignia insignia)
        {
            _context.Insignias.Update(insignia);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarAsync(Insignia insignia)
        {
            await _context.Insignias.AddAsync(insignia);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var insignia = await _context.Insignias.FirstOrDefaultAsync(x => x.InsigniaId == id);
            if (insignia != null)
            {
                _context.Insignias.Remove(insignia);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Insignia?> ObtenerPorIdAsync(int id)
        {
            return await _context.Insignias.AsNoTracking().FirstOrDefaultAsync(x => x.InsigniaId == id);
        }

        public async Task<List<Insignia>> ObtenerTodosAsync()
        {
            return await _context.Insignias.OrderBy(x => x.InsigniaId).ToListAsync();
        }
    }
}
