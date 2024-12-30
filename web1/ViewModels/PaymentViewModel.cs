using System.Collections.Generic;
using web1.Models;

public class PaymentViewModel
{
    public string CartId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CartItem> CartItems { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
} 