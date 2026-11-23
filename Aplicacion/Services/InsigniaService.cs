using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class InsigniaService : IInsigniaService
    {
        private readonly IInsigniaRepository _insigniaRepository;

        public InsigniaService(IInsigniaRepository insigniaRepository)
        {
            _insigniaRepository = insigniaRepository;
        }

        public async Task<List<Insignia>> ObtenerTodosAsync()
        {
            return await _insigniaRepository.ObtenerTodosAsync();
        }

        public async Task<Insignia?> ObtenerPorIdAsync(int id)
        {
            return await _insigniaRepository.ObtenerPorIdAsync(id);
        }

        public async Task AgregarAsync(Insignia insignia)
        {
            await _insigniaRepository.AgregarAsync(insignia);
        }

        public async Task ActualizarAsync(Insignia insignia)
        {
            await _insigniaRepository.ActualizarAsync(insignia);
        }

        public async Task EliminarAsync(int id)
        {
            await _insigniaRepository.EliminarAsync(id);
        }
    }
}
