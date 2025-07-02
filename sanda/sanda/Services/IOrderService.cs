using sanda.Models;
namespace sanda.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsyn();
        Task<Order> GetOrderByID(int orderId);
        Task<ServiceResponse> UpdateOrder(Order order);
        Task<ServiceResponse> AddOrder(Order order, int userId); // تعديل لجلب اسم المستخدم تلقائيًا
        Task<ServiceResponse> UpdateStatus(int orderId, OrderStatus status); // تعديل لجعل الحالة نوع OrderStatus
        Task<ServiceResponse> MarkOrderAsDone(int id);
        Task<List<Order>> GetOrdersByStatus(int userId, OrderStatus status, bool excludeStatus);
        Task<List<Order>> GetOrdersInProgressAsync(); // Add this new method



        Task<ServiceResponse> DeleteOrder(int orderId); // New method for hard delete
        Task<ServiceResponse> CleanupDoneOrdersForUser(int userId); // New method for cleanup
        Task<int> GetUserOrderCount(int userId); // New method to get user order count
    }
}