using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestForma.Models
{
    public class ApplicationUser:IdentityUser
    {
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = "";
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = "";
        public string Address { get; set; } = "";
        public int Age { get; set; } 

        public DateTime CreatedAt { get; set; }

        public bool archivee { get; set; } = false;
    }
}
