using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Models
{
    public interface IOrderRepository
    {
        IEnumerable<Order> AllOrders { get; }
        void CreateOrder(Order order);
        void DeleteOrder(int orderId);  // Add 
        Order GetOrderById(int orderId);  // Add this method signature
        void UpdateOrder(Order order);  // Method for updating the order
        public void Save();
        Task<IEnumerable<Order>> SearchOrdersAsync(string searchQuery);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        void DeleteOrderbyobj(Order order); // Add this method
        IEnumerable<Order> GetOrdersByEmail(string email); // Add this method to fetch orders by user email
        Order GetOrderByIdRFPie(int orderId);

    }
}
