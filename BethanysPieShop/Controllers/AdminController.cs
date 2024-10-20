using Microsoft.AspNetCore.Mvc;
using BethanysPieShop.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BethanysPieShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IPieRepository _pieRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;  // Logger dependency
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment; // Inject the hosting environment
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Constructor injection of dependencies
        public AdminController(IPieRepository pieRepository, IOrderRepository orderRepository, AppDbContext context, ILogger<AdminController> logger, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _pieRepository = pieRepository;
            _orderRepository = orderRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _logger = logger;  // Assigning the logger
            _webHostEnvironment = webHostEnvironment; // Initialize in the constructor
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Index page for Admin, could be a dashboard or general landing
        public IActionResult Index()
        {
            return View();
        }

        // Manage Pies: list all pies
        public async Task<IActionResult> ManagePies(string searchQuery)
        {
            var pies = _pieRepository.AllPies;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                // If a search query is provided, filter pies by name or price
                pies = await _pieRepository.SearchPiesAsync(searchQuery);
                ViewBag.SearchQuery = searchQuery;  // Pass the search query back to the view
            }
            else
            {
                // If no search query, return all pies
                pies = await _pieRepository.GetAllPiesAsync();
            }

            return View(pies);
        }

        // Manage Orders: list all orders
        public async Task<IActionResult> ManageOrders(string searchQuery)
        {
            var orders = _orderRepository.AllOrders;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                // If a search query is provided, filter orders by OrderId or Customer Name
                orders = await _orderRepository.SearchOrdersAsync(searchQuery);
                ViewBag.SearchQuery = searchQuery;  // Pass the search query back to the view
            }
            else
            {
                // If no search query, return all orders
                orders = await _orderRepository.GetAllOrdersAsync();
            }
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> EditPie(int pieId)
        {
            // Fetch the pie by ID from the repository
            var pie = await _pieRepository.GetPieByIdAsync(pieId);

            // Check if the pie exists
            if (pie == null)
            {
                _logger.LogWarning($"Pie with ID {pieId} not found.");
                return NotFound();
            }

            // Fetch categories for the dropdown
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", pie.CategoryId);

            return View(pie);  // Pass the pie object to the view
        }
        [HttpPost]
        public async Task<IActionResult> EditPie(int pieId, Pie pie, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Fetch the existing pie from the database
                var existingPie = await _context.Pies.FirstOrDefaultAsync(p => p.PieId == pieId);

                // Check if the pie exists
                if (existingPie == null)
                {
                    _logger.LogWarning($"Pie with ID {pieId} not found.");
                    return NotFound();
                }

                // Handle image upload if a new image is provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    _logger.LogInformation($"Uploading new image for Pie ID {pieId} to {filePath}");

                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(existingPie.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingPie.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            _logger.LogInformation($"Deleting old image at {oldImagePath}");
                            System.IO.File.Delete(oldImagePath);
                        }
                        else
                        {
                            _logger.LogWarning($"Old image not found at {oldImagePath}");
                        }
                    }

                    // Save the new image
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Update both ImageUrl and ImageThumbnailUrl to point to the new image
                    var newImageUrl = "/images/" + uniqueFileName;
                    existingPie.ImageUrl = newImageUrl;
                    existingPie.ImageThumbnailUrl = newImageUrl; // Set the thumbnail URL to the same as the image URL
                    _logger.LogInformation($"New image URL set for Pie ID {pieId}: {existingPie.ImageUrl}");
                }
                else
                {
                    _logger.LogWarning($"No image file provided for Pie ID {pieId}");
                }

                // Update other pie properties
                existingPie.Name = pie.Name;
                existingPie.ShortDescription = pie.ShortDescription;
                existingPie.LongDescription = pie.LongDescription;
                existingPie.AllergyInformation = pie.AllergyInformation;
                existingPie.Price = pie.Price;
                existingPie.IsPieOfTheWeek = pie.IsPieOfTheWeek;
                existingPie.InStock = pie.InStock;
                existingPie.CategoryId = pie.CategoryId;

                // Save the changes to the database
                _logger.LogInformation($"Saving changes for Pie ID {pieId}");
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Pie with ID {pieId} updated successfully.");

                // Add success message to TempData
                TempData["success"] = "Pie updated successfully!";
                return RedirectToAction("ManagePies", "Admin");
            }
            else
            {
                // If model is invalid, return the view with error messages
                _logger.LogWarning($"Model state is invalid for Pie ID {pieId}");

                // Fetch categories again and return the view with the pie model
                var categories = await _context.Categories.ToListAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", pie.CategoryId);
                return View(pie);
            }
        }

        [HttpGet]
        public IActionResult DeletePie(int pieId)
        {
            var pie = _pieRepository.GetPieById(pieId);
            if (pie == null)
            {
                return NotFound();  // If the pie doesn't exist, return 404
            }
            return View(pie);  // Return the pie to the delete confirmation view
        }

        [HttpPost, ActionName("DeletePie")]  // Use the ActionName attribute to link this method with DeletePie
        public IActionResult DeletePieConfirmed(int pieId)
        {
            var pie = _pieRepository.GetPieById(pieId);
            if (pie != null)
            {
                _pieRepository.DeletePie(pieId);  // Delete the pie from the repository
                return RedirectToAction("ManagePies");  // Redirect to the pie management page
            }
            return NotFound();  // If the pie doesn't exist anymore, return 404
        }


        [HttpGet]
        public IActionResult AddPie()
        {
            // Fetch categories for the dropdown
            var categories = _categoryRepository.GetAllCategoriesAsync().Result;
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            // Return the view with an empty Pie object for the form
            return View(new Pie());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPie(Pie pie, IFormFile imageFile)
        {
            // No validation on ImageUrl, we're handling file uploads separately
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("ImageUrl", "Image is required.");
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the new image
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Set the image URLs
                    pie.ImageUrl = "/images/" + uniqueFileName;
                    pie.ImageThumbnailUrl = "/images/" + uniqueFileName;
                }

                // Add the new pie to the database
                _context.Pies.Add(pie);
                await _context.SaveChangesAsync();

                TempData["success"] = "Pie added successfully!";
                return RedirectToAction("ManagePies", "Admin");
            }

            // If the model is invalid, reload the categories and return the view
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", pie.CategoryId);
            return View(pie);
        }


        public IActionResult DeleteOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        [HttpGet]
        public IActionResult DeleteOrderConfirmed(int orderId)
        {

            var order = _orderRepository.GetOrderById(orderId);
            if (order != null)
            {
                _orderRepository.DeleteOrder(orderId);
                return RedirectToAction("ManageOrders");
            }
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        [Route("Admin/EditOrder/{orderId}")]
        public IActionResult EditOrder(int orderId)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
            {
                _logger.LogWarning($"Order with ID {orderId} not found.");
                return NotFound();
            }

            ViewBag.Statuses = new SelectList(new List<string> { "Pending", "Completed", "Cancelled" }, order.Status);
            return View(order);
        }

        [HttpPost]
        [Route("Admin/EditOrder/{orderId}")]
        public IActionResult EditOrder(Order order, int orderId)
        {
            _logger.LogInformation($"Received order ID: {orderId}");

            // Remove validation for irrelevant fields
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("AddressLine1");
            ModelState.Remove("AddressLine2");
            ModelState.Remove("ZipCode");
            ModelState.Remove("City");
            ModelState.Remove("Country");
            ModelState.Remove("PhoneNumber");
            ModelState.Remove("Email");

            if (ModelState.IsValid)
            {
                var existingOrder = _orderRepository.GetOrderById(orderId);

                if (existingOrder == null)
                {
                    _logger.LogWarning($"Order with ID {order.OrderId} not found.");
                    return NotFound();
                }

                // Update the status and save changes
                existingOrder.Status = order.Status;

                try
                {
                    _orderRepository.UpdateOrder(existingOrder);
                    _context.SaveChanges();
                    _logger.LogInformation($"Order {order.OrderId} updated successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating order {order.OrderId}: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error updating order");
                }

                return RedirectToAction("ManageOrders");
            }

            ViewBag.Statuses = new SelectList(new List<string> { "Pending", "Completed", "Cancelled" }, order.Status);
            return View(order);
        }


        [HttpGet]
        public IActionResult AddCategory()
        {

            return View();
        }

        // POST: Admin/AddCategory
        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddCategoryAsync(category);  // You need to implement this method
                return RedirectToAction("ManageCategories");
            }
            return View(category);
        }

        // GET: Admin/ManageCategories
        public async Task<IActionResult> ManageCategories(string searchQuery)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync(); // Fetch categories from the repository
           
            if (!string.IsNullOrEmpty(searchQuery))
            {
                // If search query is provided, filter the categories by name
                categories = await _categoryRepository.SearchCategoriesAsync(searchQuery);
                ViewBag.SearchQuery = searchQuery;  // Pass the search query back to the view
            }
            else
            {
                // If no search query, return all categories
                categories = await _categoryRepository.GetAllCategoriesAsync();
            }
            return View(categories);
        }

        // GET: Admin/DeleteCategory
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        [Route("Admin/DeleteCategoryConfirmed")]
        public async Task<IActionResult> DeleteCategoryConfirmed(int categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category != null)
            {
                await _categoryRepository.DeleteCategoryAsync(categoryId);
                return RedirectToAction("ManageCategories");
            }

            return NotFound();
        }
        public async Task<IActionResult> ManageAdmins()
        {
            var users = _userManager.Users.ToList();
            var model = new ManageAdminViewModel
            {
                Users = users,
                Admins = (await _userManager.GetUsersInRoleAsync("Admin")).Select(u => u.Email).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction("ManageAdmins");
        }



    }
}
