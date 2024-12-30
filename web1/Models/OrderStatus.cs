using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
} 