using Microsoft.AspNetCore.Mvc;
using web1.Services;
using web1.Data;
using web1.ViewModels;
using web1.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace web1.Controllers
{
    public class PayPalController : Controller
    {
        private readonly PayPalService _paypalService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayPalController> _logger;

        public PayPalController(
            PayPalService paypalService,
            ApplicationDbContext context,
            ILogger<PayPalController> logger)
        {
            _paypalService = paypalService;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentViewModel model)
        {
            try
            {
                if (model == null || model.TotalAmount <= 0)
                {
                    return BadRequest("Invalid payment data");
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var payment = _paypalService.CreatePayment(
                    model.TotalAmount,
                    $"{baseUrl}/PayPal/Success",
                    $"{baseUrl}/PayPal/Cancel"
                );

                if (payment == null || string.IsNullOrEmpty(payment.id))
                {
                    _logger.LogError("Failed to create PayPal payment");
                    return BadRequest("Failed to create payment");
                }

                // Lưu thông tin thanh toán vào session
                HttpContext.Session.SetString("PaymentId", payment.id);
                HttpContext.Session.SetString("PaymentInfo", JsonSerializer.Serialize(model));

                var approvalUrl = payment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;
                if (string.IsNullOrEmpty(approvalUrl))
                {
                    _logger.LogError("No approval URL in PayPal response");
                    return BadRequest("Invalid PayPal response");
                }

                return Json(new { redirectUrl = approvalUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment");
                return BadRequest("Error processing payment");
            }
        }

        public async Task<IActionResult> Success(string paymentId, string PayerID)
        {
            try
            {
                var sessionPaymentId = HttpContext.Session.GetString("PaymentId");
                var paymentInfo = JsonSerializer.Deserialize<PaymentViewModel>(
                    HttpContext.Session.GetString("PaymentInfo"));

                if (string.IsNullOrEmpty(sessionPaymentId) || paymentInfo == null)
                {
                    return BadRequest();
                }

                var payment = _paypalService.ExecutePayment(paymentId, PayerID);

                // Tạo đơn hàng sau khi thanh toán thành công
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    FullName = paymentInfo.FullName,
                    Email = paymentInfo.Email,
                    PhoneNumber = paymentInfo.PhoneNumber,
                    Address = paymentInfo.Address,
                    TotalAmount = paymentInfo.TotalAmount,
                    CurrentStatus = "pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Thêm chi tiết đơn hàng
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CartId == paymentInfo.CartId)
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                }

                // Xóa giỏ hàng
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                // Xóa session
                HttpContext.Session.Remove("PaymentId");
                HttpContext.Session.Remove("PaymentInfo");
                HttpContext.Session.Remove("CartId");

                TempData["Success"] = "Payment completed successfully!";
                return RedirectToAction("Confirmation", "Order", new { id = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PayPal payment");
                TempData["Error"] = "Error processing payment";
                return RedirectToAction("Index", "Cart");
            }
        }

        public IActionResult Cancel()
        {
            TempData["Error"] = "Payment was cancelled";
            return RedirectToAction("Index", "Cart");
        }
    }
} 