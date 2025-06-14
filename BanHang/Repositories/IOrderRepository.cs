using BanHang.Models;
using Microsoft.AspNetCore.Mvc;
using static BanHang.Models.Order;

namespace BanHang.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task<Order> GetByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        Task<string> GenerateOrderNumberAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetOrderCountAsync();
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
    }
}
