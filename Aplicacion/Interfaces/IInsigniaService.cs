using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IInsigniaService
    {
        Task<List<Insignia>> ObtenerTodosAsync();

        Task<Insignia?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Insignia insignia);

        Task ActualizarAsync(Insignia insignia);

        Task EliminarAsync(int id);
    }
}
