using System.Collections.Generic;
using System.Threading.Tasks;
using BethanysPieShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BethanysPieShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ShoppingCart _shoppingCart;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(IOrderRepository orderRepository, ShoppingCart shoppingCart, UserManager<IdentityUser> userManager)
        {
            _orderRepository = orderRepository;
            _shoppingCart = shoppingCart;
            _userManager = userManager;
        }

        // GET: Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            var email = user?.Email; // Retrieve the user's email if available

            var order = new Order
            {
                Email = email // Predefine the email value
            };

            return View(order); // Pass the pre-filled order to the view
        }

        // POST: Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            if (_shoppingCart.ShoppingCartItems.Count == 0)
            {
                ModelState.AddModelError("", "Your cart is empty, add some pies first.");
                return View(order);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    order.Email = user.Email; // Ensure the email is still set
                    _orderRepository.CreateOrder(order);
                    _shoppingCart.ClearCart();
                    return RedirectToAction("CheckoutComplete");
                }
            }

            return View(order);
        }

        // GET: Customer Orders
        [HttpGet]
        public async Task<IActionResult> CustomerOrders()
        {
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            var userEmail = user?.Email;

            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user is not found
            }

            // Fetch orders for this user based on their email
            var customerOrders = _orderRepository.GetOrdersByEmail(userEmail);

            return View(customerOrders ?? new List<Order>()); // Pass an empty list if customerOrders is null
        }

        // GET: Order Details
        public IActionResult OrderDetails(int orderId)
        {
            var order = _orderRepository.GetOrderByIdRFPie(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Delete Order
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Ensure the logged-in user can only delete their own orders
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email != order.Email)
            {
                return Forbid(); // Return 403 if the user is trying to delete someone else's order
            }

            // Delete the order
            _orderRepository.DeleteOrderbyobj(order);

            // Redirect to the order list page
            return RedirectToAction("CustomerOrders");
        }

        // GET: Checkout Complete
        public IActionResult CheckoutComplete()
        {
            ViewBag.CheckoutCompleteMessage = "Thanks for your order. You'll soon enjoy our delicious pies!";
            return View();
        }
    }
}
