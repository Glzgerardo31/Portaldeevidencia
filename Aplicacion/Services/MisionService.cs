using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class MisionService : IMisionService
    {
        private readonly IMisionRepository _misionRepository;

        public MisionService(IMisionRepository misionRepository)
        {
            _misionRepository = misionRepository;
        }

        public async Task<List<Mision>> ObtenerTodosAsync()
        {
            return await _misionRepository.ObtenerTodosAsync();
        }

        public async Task<Mision?> ObtenerPorIdAsync(int id)
        {
            return await _misionRepository.ObtenerPorIdAsync(id);
        }

        public async Task AgregarAsync(Mision mision)
        {
            // Toda misión que llega por la API viene del formulario "Proponer una misión".
            mision.EsPropuesta = true;

            // El front end ya manda fechaCreacion; si no viniera, se pone aquí.
            if (mision.FechaCreacion == default)
            {
                mision.FechaCreacion = DateTime.UtcNow;
            }

            await _misionRepository.AgregarAsync(mision);
        }

        public async Task ActualizarAsync(Mision mision)
        {
            await _misionRepository.ActualizarAsync(mision);
        }

        public async Task EliminarAsync(int id)
        {
            await _misionRepository.EliminarAsync(id);
        }
    }
}
