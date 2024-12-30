using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
} 