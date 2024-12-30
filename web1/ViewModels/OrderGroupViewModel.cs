using web1.Models;

namespace web1.ViewModels
{
    public class OrderGroupViewModel
    {
        public string Status { get; set; } = string.Empty;
        public List<Order> Orders { get; set; } = new List<Order>();
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 