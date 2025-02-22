using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestForma.Models
{
    public class Actualite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdActualite { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Titre { get; set; }
        [Required]
        public string? Description { get; set; }

        
        [MaxLength(255)]
        public string? FileName { get; set; } // Nom de l'image

        
        [MaxLength(50)]
        public string? ContentType { get; set; } // Type MIME (ex. "image/png", "image/jpeg")

        
        public long? Size { get; set; } // Taille de l'image en octets

        public byte[]? Data { get; set; } // Contenu binaire de l'image

        public bool archivee { get; set; } = false;

    }
}
