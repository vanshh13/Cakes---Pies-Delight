using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Models
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly ShoppingCart _shoppingCart;

        public OrderRepository(AppDbContext appDbContext, ShoppingCart shoppingCart)
        {
            _appDbContext = appDbContext;
            _shoppingCart = shoppingCart;
        }
        public async Task<IEnumerable<Order>> SearchOrdersAsync(string searchQuery)
        {
            return await _appDbContext.Orders
                .Where(o => o.OrderId.ToString().Contains(searchQuery)
                         || (o.FirstName + " " + o.LastName).Contains(searchQuery))
                .ToListAsync();
        }
        public IEnumerable<Order> AllOrders
        {
            get { return _appDbContext.Orders.ToList(); }  // Fetching all orders from the database
        }

        public Order GetOrderById(int orderId)
        {
            return _appDbContext.Orders.FirstOrDefault(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _appDbContext.Orders.ToListAsync();
        }

        public void CreateOrder(Order order)
        {
            order.OrderPlaced = DateTime.Now;

            // First, save the order to the database
            _appDbContext.Orders.Add(order);
            _appDbContext.SaveChanges(); // Save to generate OrderId

            var shoppingCartItems = _shoppingCart.ShoppingCartItems;

            // Now, we can safely create order details
            foreach (var shoppingCartItem in shoppingCartItems)
            {
                var orderDetail = new OrderDetail()
                {
                    Amount = shoppingCartItem.Amount,
                    PieId = shoppingCartItem.Pie.PieId,
                    OrderId = order.OrderId, // This will now be valid
                    Price = shoppingCartItem.Pie.Price
                };

                _appDbContext.OrderDetails.Add(orderDetail);
            }

            // Save the order details
            _appDbContext.SaveChanges();
        }

        public void DeleteOrder(int orderId)
        {
            var order = _appDbContext.Orders.FirstOrDefault(o => o.OrderId == orderId);  // Fetch the order from the database
            if (order != null)
            {
                _appDbContext.Orders.Remove(order);  // Remove the order from the database
                _appDbContext.SaveChanges();  // Save changes to the database
            }
        }

        public void UpdateOrder(Order order)
        {
            _appDbContext.Orders.Update(order);
        }
        public void Save()
        {
            _appDbContext.SaveChanges();  // This is required to persist the changes
        }
        public IEnumerable<Order> GetOrdersByEmail(string email)
        {
            return _appDbContext.Orders
                                .Where(o => o.Email == email) // Fetch orders by user's email
                                .Include(o => o.OrderDetails); // Include order details if needed
        }
        public void DeleteOrderbyobj(Order order)
        {
            _appDbContext.Orders.Remove(order);
            _appDbContext.SaveChanges();
        }
        public Order GetOrderByIdRFPie(int orderId)
        {
            return _appDbContext.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Pie) // Include the Pie information for each OrderDetail
                .FirstOrDefault(o => o.OrderId == orderId);
        }
    }
}
