using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web1.Data;
using web1.Models;

namespace web1.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetCartId()
        {
            var cartId = HttpContext.Session.GetString("CartId");
            if (cartId == null)
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartId", cartId);
            }
            return cartId;
        }

        public async Task<IActionResult> Index()
        {
            var cartId = GetCartId();
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            try
            {
                var cartId = GetCartId();
                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    return NotFound();
                }

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);

                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        CartId = cartId,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    };
                    _context.CartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity += quantity;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Product added to cart successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                TempData["Error"] = "Error adding product to cart.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 