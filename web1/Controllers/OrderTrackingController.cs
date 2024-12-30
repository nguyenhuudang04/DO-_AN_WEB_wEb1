using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web1.Data;
using web1.Models;
using Microsoft.AspNetCore.Authorization;

namespace web1.Controllers
{
    [Authorize]
    public class OrderTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderTrackingController> _logger;

        public OrderTrackingController(
            ApplicationDbContext context,
            ILogger<OrderTrackingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Track(int id)
        {
            var order = await _context.Orders
                .Include(o => o.StatusHistory.OrderByDescending(s => s.Timestamp))
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền xem
            if (!User.IsInRole("Admin") && order.Email != User.Identity?.Name)
            {
                return Forbid();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status, string location, string description)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.StatusHistory)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound();
                }

                // Kiểm tra trạng thái hợp lệ
                if (!IsValidStatus(status))
                {
                    TempData["Error"] = "Invalid status";
                    return RedirectToAction("Track", new { id = orderId });
                }

                var orderStatus = new OrderStatus
                {
                    OrderId = orderId,
                    Status = status,
                    Location = location,
                    Description = description,
                    Timestamp = DateTime.Now
                };

                order.CurrentStatus = status;
                _context.OrderStatuses.Add(orderStatus);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Order status updated successfully";
                return RedirectToAction("Track", new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                TempData["Error"] = "Error updating order status";
                return RedirectToAction("Track", new { id = orderId });
            }
        }

        private bool IsValidStatus(string status)
        {
            var validStatuses = new[]
            {
                "Pending",
                "Processing",
                "Warehouse",
                "Transit",
                "LocalHub",
                "OutForDelivery",
                "Delivered"
            };

            return validStatuses.Contains(status);
        }
    }
} 