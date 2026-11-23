using Dominio.Enum;
namespace Dominio.Entidades;
public class Progreso
{
    public int ProgresoId { get; set; }
    public int MisionId { get; set; }
    public int UsuarioId { get; set; }
    public EstadoProgreso Estado { get; set; } = EstadoProgreso.Pendiente;
    public DateTime? FechaCompletada { get; set; }
    public Mision? Mision { get; set; }
    public Usuario? Usuario { get; set; }
}
