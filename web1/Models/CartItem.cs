using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        public string CartId { get; set; } = string.Empty;
        
        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
        
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
} 