using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestForma.Models
{
    public class Inscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Inscription { get; set; }

        [Required]
        public string? ID_User { get; set; }

        [ForeignKey("ID_User")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int ID_Formation { get; set; }

        [ForeignKey("ID_Formation")]
        public Formation? Formation { get; set; }

        public bool Paiement { get; set; } = false;
        public bool Fin { get; set; } = false;
        public bool Certificat { get; set; } = false;

        public bool archivee { get; set; } = false;

    }
}
