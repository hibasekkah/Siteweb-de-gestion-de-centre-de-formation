using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestForma.Models
{
    public class CommentairesEntiers
    {
        // Clé primaire
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCommentaire { get; set; }

        // Contenu du commentaire
        [Required]
        [MaxLength(2000)]
        public string? ContenuCommentaire { get; set; }

        // Clé étrangère pour l'utilisateur
        // Ajout de l'attribut pour s'assurer que la relation est obligatoire
        public string? Id_User { get; set; }

        [ForeignKey("Id_User")]
        public ApplicationUser? User { get; set; }

        public bool archivee { get; set; } = false;
    }
}
