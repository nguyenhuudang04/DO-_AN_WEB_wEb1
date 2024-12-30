using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public DateTime RegisterDate { get; set; } = DateTime.Now;
    }
} 