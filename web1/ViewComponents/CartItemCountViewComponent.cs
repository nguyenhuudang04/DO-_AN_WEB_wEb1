using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web1.Data;

namespace web1.ViewComponents
{
    public class CartItemCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CartItemCountViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
            {
                return View(0);
            }

            var count = await _context.CartItems
                .Where(c => c.CartId == cartId)
                .SumAsync(c => c.Quantity);

            return View(count);
        }
    }
} 