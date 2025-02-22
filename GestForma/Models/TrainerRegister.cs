using System.ComponentModel.DataAnnotations;

namespace GestForma.Models
{
    public class TrainerRegister
    {
        // Champs provenant de ApplicationUser
        [Required]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        public string Phone { get; set; } = "";

        // For the Trainer
        [Required]
        public string Field { get; set; } = "";
        [Required]
        public IFormFile ProfileImage { get; set; }
    }
}
