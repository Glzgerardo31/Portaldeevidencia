using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    // Tarjeta de la vista "Mis insignias".
    public class Insignia
    {
        public int InsigniaId { get; set; }

        // Cuenta estudiante que envió la propuesta (null para contenido inicial/admin).
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Misión cuyo cumplimiento desbloquea la insignia.
        // Es null cuando no depende de una sola misión (por ejemplo, "Conquistador del campus").
        public int? MisionId { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(120, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;

        // Texto pequeño sobre el título: "PRIMER LOGRO", "CULTURA", "CIENCIA"...
        [Required]
        [StringLength(30)]
        public string Etiqueta { get; set; } = string.Empty;

        // Emoji que se muestra en la tarjeta.
        [Required]
        [StringLength(10)]
        public string Icono { get; set; } = string.Empty;

        // true cuando espera revisión del administrador.
        public bool EsPropuesta { get; set; }

        public Mision? Mision { get; set; }
    }
}
