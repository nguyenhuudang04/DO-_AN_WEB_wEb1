using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web1.Data;
using web1.Models;

namespace web1.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CartSummaryViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string cartId)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                return View(new List<CartItem>());
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            return View(cartItems);
        }
    }
} 