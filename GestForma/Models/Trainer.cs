using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestForma.Models
{
    public class Trainer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; } // Nom du fichier (facultatif)

        [MaxLength(50)]
        public string? ContentType { get; set; } // Type MIME (facultatif)

        public long? Size { get; set; } // Taille du fichier en octets (facultatif)

        public byte[]? Data { get; set; } // Contenu binaire du fichier (facultatif)

        [MaxLength(100)]
        public string Field { get; set; } = ""; // Domaine du formateur (peut être facultatif)

        public string? Id_user { get; set; } // Identifiant de l'utilisateur associé
        [ForeignKey("Id_user")]
        public ApplicationUser? User { get; set; } // Relation avec l'utilisateur

        public bool archivee { get; set; } = false;


    }
}
