using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace web1.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public double GetAverageRating()
        {
            if (Reviews == null || !Reviews.Any(r => r.IsApproved))
                return 0;
            
            return Math.Round(Reviews.Where(r => r.IsApproved).Average(r => r.Rating), 1);
        }

        public int GetTotalReviews()
        {
            if (Reviews == null)
                return 0;
            
            return Reviews.Count(r => r.IsApproved);
        }
    }
} 