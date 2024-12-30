using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web1.Data;
using web1.Models;
using web1.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace web1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeliveryTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeliveryTrackingController> _logger;

        public DeliveryTrackingController(ApplicationDbContext context, ILogger<DeliveryTrackingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var activeOrders = await _context.Orders
                .Include(o => o.StatusHistory)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Nhóm đơn hàng theo trạng thái
            var groupedOrders = activeOrders
                .GroupBy(o => o.CurrentStatus)
                .Select(g => new OrderGroupViewModel
                {
                    Status = g.Key,
                    Orders = g.ToList(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => GetStatusOrder(g.Status)) // Sắp xếp theo thứ tự trạng thái
                .ToList();

            ViewBag.TotalOrders = activeOrders.Count;
            ViewBag.TotalAmount = activeOrders.Sum(o => o.TotalAmount);
            ViewBag.PendingOrders = activeOrders.Count(o => o.CurrentStatus == "Pending");
            ViewBag.DeliveredOrders = activeOrders.Count(o => o.CurrentStatus == "Delivered");

            return View(groupedOrders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                var location = GetLocationForStatus(status);
                var description = GetDescriptionForStatus(status);

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

                return Json(new { 
                    success = true, 
                    message = "Status updated successfully",
                    newStatus = status,
                    location = location,
                    timestamp = DateTime.Now.ToString("MMM dd, yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return Json(new { success = false, message = "Error updating status" });
            }
        }

        private int GetStatusOrder(string status)
        {
            return status.ToLower() switch
            {
                "pending" => 1,
                "processing" => 2,
                "warehouse" => 3,
                "transit" => 4,
                "localhub" => 5,
                "outfordelivery" => 6,
                "delivered" => 7,
                _ => 99
            };
        }

        private string GetLocationForStatus(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "Order Processing Center",
                "processing" => "Main Warehouse",
                "warehouse" => "Main Warehouse Storage",
                "transit" => "Shipping Partner",
                "localhub" => "Local Distribution Center",
                "outfordelivery" => "Local Delivery Service",
                "delivered" => "Customer Address",
                _ => "Unknown Location"
            };
        }

        private string GetDescriptionForStatus(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "Order has been placed and is being processed",
                "processing" => "Order is being picked and packed",
                "warehouse" => "Order has been packed and ready for shipping",
                "transit" => "Order is in transit to local distribution center",
                "localhub" => "Order arrived at local distribution center",
                "outfordelivery" => "Order is out for delivery to customer",
                "delivered" => "Order has been delivered successfully",
                _ => "Status updated"
            };
        }
    }
} 