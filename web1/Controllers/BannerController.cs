using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web1.Data;
using web1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;

namespace web1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string _bannerFolder;
        private readonly ILogger<BannerController> _logger;

        public BannerController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, ILogger<BannerController> logger)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _bannerFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "banners");

            // Tự động tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(_bannerFolder))
            {
                Directory.CreateDirectory(_bannerFolder);
            }

            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banners.OrderBy(b => b.DisplayOrder).ToListAsync();
            return View(banners);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> Create(Banner banner, IFormFile? image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    ModelState.AddModelError("image", "Please select an image file");
                    return View(banner);
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("image", "Only image files (.jpg, .jpeg, .png, .gif) are allowed");
                    return View(banner);
                }

                // Kiểm tra kích thước file
                if (image.Length > 104857600) // 100MB
                {
                    ModelState.AddModelError("image", "File size must be less than 100MB");
                    return View(banner);
                }

                try
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "banners");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    banner.ImagePath = uniqueFileName;
                    banner.IsActive = true;
                    
                    if (banner.DisplayOrder == 0)
                    {
                        var maxOrder = await _context.Banners.MaxAsync(b => (int?)b.DisplayOrder) ?? 0;
                        banner.DisplayOrder = maxOrder + 1;
                    }

                    if (ModelState.IsValid)
                    {
                        _context.Add(banner);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Banner created successfully";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving banner file: {Message}", ex.Message);
                    ModelState.AddModelError("", "Error saving banner file. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner: {Message}", ex.Message);
                ModelState.AddModelError("", "Error creating banner. Please try again.");
            }

            return View(banner);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            // Delete image file
            if (!string.IsNullOrEmpty(banner.ImagePath))
            {
                var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", "banners", banner.ImagePath);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            banner.IsActive = !banner.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int id, int newOrder)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            banner.DisplayOrder = newOrder;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100MB
        [RequestSizeLimit(104857600)] // 100MB
        public async Task<IActionResult> Edit(int id, string title, string description, string link, IFormFile? newImage)
        {
            try
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null)
                {
                    return NotFound();
                }

                if (newImage != null && newImage.Length > 104857600) // 100MB limit
                {
                    TempData["Error"] = "File size must be less than 100MB";
                    return RedirectToAction(nameof(Index));
                }

                banner.Title = title;
                banner.Description = description;
                banner.Link = link;

                if (newImage != null)
                {
                    // Delete old image
                    if (!string.IsNullOrEmpty(banner.ImagePath))
                    {
                        var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", "banners", banner.ImagePath);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "banners");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + newImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await newImage.CopyToAsync(fileStream);
                    }

                    banner.ImagePath = uniqueFileName;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner: {Message}", ex.Message);
                TempData["Error"] = "Error updating banner. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 