using System.ComponentModel.DataAnnotations;
namespace Dominio.Entidades;
public class Usuario
{
    public int UsuarioId { get; set; }
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(160)] public string Correo { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Rol { get; set; } = "Estudiante";
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    [StringLength(30)] public string Apodo { get; set; } = string.Empty;
    [StringLength(25)] public string Telefono { get; set; } = string.Empty;
    [StringLength(30)] public string CURP { get; set; } = string.Empty;
    [StringLength(30)] public string INE { get; set; } = string.Empty;
    public string ActaNacimientoRuta { get; set; } = string.Empty;
    public ICollection<Evidencia> Evidencias { get; set; } = new List<Evidencia>();
}
