using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
} 