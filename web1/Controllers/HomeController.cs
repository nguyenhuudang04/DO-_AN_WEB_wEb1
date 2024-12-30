using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using web1.Data;
using web1.Models;
using web1.Helpers;

namespace web1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private const int PageSize = 9;// Số sản phẩm trên mỗi trang

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string searchString, 
            int? categoryId, 
            decimal? minPrice, 
            decimal? maxPrice,
            int? page)
        {
            try
            {
                // Get banners
                ViewBag.Banners = await _context.Banners
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.DisplayOrder)
                    .ToListAsync();

                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Reviews)
                    .AsQueryable();

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(p => p.Name.Contains(searchString));
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                // Sắp xếp sản phẩm
                query = query.OrderByDescending(p => p.Id);

                int pageNumber = page ?? 1;
                var products = await PaginatedList<Product>.CreateAsync(query, pageNumber, PageSize);

                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.CurrentFilter = searchString;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.CurrentPage = pageNumber;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View("Error");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
