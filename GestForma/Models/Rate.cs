using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestForma.Models
{
    public class Rate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRate { get; set; }

        [Required]
        public int ID_Formation { get; set; }

        [ForeignKey("ID_Formation")]
        public Formation? Formation { get; set; }

        [Required]
        public string? ID_User { get; set; }

        [ForeignKey("ID_User")]
        public ApplicationUser? User { get; set; }

        [Required]
        [Range(0, 5)]
        public double ContenuRate { get; set; }

        public bool archivee { get; set; } = false;
    }
}
