using System.ComponentModel.DataAnnotations;
namespace Dominio.Entidades;
public class Evidencia
{
    public int EvidenciaId { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    [Required, StringLength(30)] public string Categoria { get; set; } = "Academica";
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [StringLength(250)] public string Descripcion { get; set; } = string.Empty;
    [StringLength(100)] public string Institucion { get; set; } = string.Empty;
    [StringLength(80)] public string Tipo { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public string ArchivoRuta { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
