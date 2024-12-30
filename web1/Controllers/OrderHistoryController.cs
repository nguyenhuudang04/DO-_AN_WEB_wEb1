using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web1.Data;
using web1.Models;
using Microsoft.AspNetCore.Identity;

namespace web1.Controllers
{
    [Authorize]
    public class OrderHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderHistoryController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.Email == user.Email)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Reorder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra sản phẩm còn tồn tại không
            foreach (var item in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    TempData["Error"] = $"Some products are no longer available.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Thêm sản phẩm vào gi��� hàng
            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartId", cartId);
            }

            foreach (var item in order.OrderDetails)
            {
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == item.ProductId);

                if (cartItem != null)
                {
                    cartItem.Quantity += item.Quantity;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cartId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    };
                    _context.CartItems.Add(cartItem);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Items have been added to your cart.";
            return RedirectToAction("Index", "Cart");
        }
    }
} 