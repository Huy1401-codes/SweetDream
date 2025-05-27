using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SweetDream.Models
{
    public class Account : IdentityUser<int>
    {
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; } = string.Empty;
        public string AccountImage { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public bool? Active { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public bool Disable { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
