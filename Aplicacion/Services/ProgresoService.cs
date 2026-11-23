using Aplicacion.Interfaces;
using Dominio.Enum;
using Dominio.Entidades;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class ProgresoService : IProgresoService
    {
        private readonly IProgresoRepository _progresoRepository;

        public ProgresoService(IProgresoRepository progresoRepository)
        {
            _progresoRepository = progresoRepository;
        }

        public async Task<List<Progreso>> ObtenerTodosAsync()
        {
            return await _progresoRepository.ObtenerTodosAsync();
        }

        public async Task<Progreso?> ObtenerPorIdAsync(int id)
        {
            return await _progresoRepository.ObtenerPorIdAsync(id);
        }

        public async Task AgregarAsync(Progreso progreso)
        {
            if (progreso.Estado == EstadoProgreso.Completada && progreso.FechaCompletada == null)
            {
                progreso.FechaCompletada = DateTime.UtcNow;
            }

            await _progresoRepository.AgregarAsync(progreso);
        }

        public async Task ActualizarAsync(Progreso progreso)
        {
            if (progreso.Estado == EstadoProgreso.Completada && progreso.FechaCompletada == null)
            {
                progreso.FechaCompletada = DateTime.UtcNow;
            }

            await _progresoRepository.ActualizarAsync(progreso);
        }

        public async Task EliminarAsync(int id)
        {
            await _progresoRepository.EliminarAsync(id);
        }
    }
}
