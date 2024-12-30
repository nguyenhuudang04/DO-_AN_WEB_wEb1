using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web1.Data;
using web1.Models;
using web1.ViewModels;

namespace web1.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReviewController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ReviewController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ReviewController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int productId, int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", 
                    new { returnUrl = Url.Action("Create", "Review", new { productId, orderId }) });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng đã mua sản phẩm này chưa
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Email == user.Email);

            if (order == null || !order.OrderDetails.Any(od => od.ProductId == productId))
            {
                TempData["Error"] = "You can only review products you have purchased.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Kiểm tra xem đã đánh giá chưa
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && 
                                        r.UserId == user.Id && 
                                        r.OrderId == orderId);
            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this product.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            ViewBag.ProductName = product.Name;
            return View(new ReviewViewModel { ProductId = productId, OrderId = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var product = await _context.Products.FindAsync(model.ProductId);
                    ViewBag.ProductName = product?.Name;
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", 
                        new { returnUrl = Url.Action("Create", "Review", new { productId = model.ProductId, orderId = model.OrderId }) });
                }

                // Kiểm tra quyền đánh giá
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == model.OrderId && o.Email == user.Email);

                if (order == null || !order.OrderDetails.Any(od => od.ProductId == model.ProductId))
                {
                    TempData["Error"] = "You can only review products you have purchased.";
                    return RedirectToAction("Details", "Product", new { id = model.ProductId });
                }

                // Kiểm tra đã đánh giá chưa
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == model.ProductId && 
                                            r.UserId == user.Id && 
                                            r.OrderId == model.OrderId);
                if (existingReview != null)
                {
                    TempData["Error"] = "You have already reviewed this product.";
                    return RedirectToAction("Details", "Product", new { id = model.ProductId });
                }

                var review = new Review
                {
                    ProductId = model.ProductId,
                    OrderId = model.OrderId,
                    UserId = user.Id,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now
                };

                // Lưu review
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Không cần refresh session và sign in nữa
                TempData["Success"] = "Thank you for your review! Continue shopping.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                TempData["Error"] = "Error saving your review. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
} 