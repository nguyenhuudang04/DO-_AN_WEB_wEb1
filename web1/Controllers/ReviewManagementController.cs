using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using web1.Data;
using web1.Models;

namespace web1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReviewManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewManagementController> _logger;

        public ReviewManagementController(
            ApplicationDbContext context,
            ILogger<ReviewManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                review.IsApproved = true;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Review approved successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review");
                TempData["Error"] = "Error approving review.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Review rejected and removed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review");
                TempData["Error"] = "Error rejecting review.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 