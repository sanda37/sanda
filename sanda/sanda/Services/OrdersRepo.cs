using Microsoft.EntityFrameworkCore;
using sanda.Data;
using sanda.Models;
using sanda.Services;


namespace sanda.Services
{
    public record ServiceResponse(bool Flag, string message);

    public class OrdersRepo : IOrderService
    {
        private readonly UserDbContext _context;

        public OrdersRepo(UserDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersAsyn()
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching orders", ex);
            }
        }

        public async Task<Order> GetOrderByID(int orderId)
        {
            try
            {
                return await _context.Orders.AsNoTracking().Include(o => o.User)
                    .FirstOrDefaultAsync(order => order.OrderId == orderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Order with ID {orderId} not found.", ex);
            }
        }

        public async Task<ServiceResponse> AddOrder(Order order, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ServiceResponse(false, "User not found.");
            }

            order.UserId = user.Id;
            order.UserName = $"{user.FirstName} {user.LastName}"; // جلب اسم المستخدم تلقائيًا
            order.Location ??= "Unknown Location";
            order.CreatedDate = DateTime.UtcNow;
            order.StatusLastUpdated = DateTime.UtcNow;
            order.StartDate = DateTime.Now;

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Check if user has 20 or more orders and cleanup done orders
                var userOrderCount = await GetUserOrderCount(userId);
                if (userOrderCount >= 20)
                {
                    await CleanupDoneOrdersForUser(userId);
                }

                return new ServiceResponse(true, "Order created successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                // Get the inner exception details
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                return new ServiceResponse(false, $"Database error: {innerException}");
            }
            catch (Exception ex)
            {
                // Get full exception details including inner exceptions
                var fullMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    fullMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                return new ServiceResponse(false, $"Failed to create order: {fullMessage}");
            }
        }

        public async Task<ServiceResponse> UpdateOrder(Order order)
        {
            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return new ServiceResponse(true, "Order updated successfully.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Failed to update order: {ex.Message}");
            }
        }

        public async Task<ServiceResponse> UpdateStatus(int orderId, OrderStatus status)
        {
            var order = await GetOrderByID(orderId);
            if (order == null)
            {
                return new ServiceResponse(false, $"Order with ID {orderId} not found.");
            }

            order.Status = status;
            order.StatusLastUpdated = DateTime.UtcNow; // تحديث وقت آخر تعديل للحالة

            // Set completion date when order is marked as done
            if (status == OrderStatus.Done)
            {
                order.CompletionDate = DateTime.UtcNow;
            }

            return await UpdateOrder(order);
        }

        public async Task<List<Order>> GetOrdersInProgressAsync()
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.User)
                    .Where(order => order.Status == OrderStatus.InProgress)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching in-progress orders", ex);
            }
        }

        public async Task<List<Order>> GetOrdersByStatus(int userId, OrderStatus status, bool excludeStatus = false)
        {
            try
            {
                var query = _context.Orders.AsNoTracking().Where(order => order.UserId == userId);
                if (excludeStatus)
                {
                    query = query.Where(order => order.Status != status);
                }
                else
                {
                    query = query.Where(order => order.Status == status);
                }
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching orders for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponse> MarkOrderAsDone(int id)
        {
            var order = await GetOrderByID(id);
            if (order == null)
            {
                return new ServiceResponse(false, "Order not found.");
            }

            if (order.Status != OrderStatus.Done)
            {
                return new ServiceResponse(false, "Order must be marked as 'Done' before deletion.");
            }

            try
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return new ServiceResponse(true, "Order marked as done and deleted successfully.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Failed to delete order: {ex.Message}");
            }
        }

        // New DeleteOrder method for hard delete
        public async Task<ServiceResponse> DeleteOrder(int orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return new ServiceResponse(false, "Order not found.");
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                return new ServiceResponse(true, "Order deleted successfully.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Failed to delete order: {ex.Message}");
            }
        }

        // New method to get user order count
        public async Task<int> GetUserOrderCount(int userId)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .CountAsync(order => order.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting order count for user {userId}: {ex.Message}", ex);
            }
        }

        // New method to cleanup done orders for user
        public async Task<ServiceResponse> CleanupDoneOrdersForUser(int userId)
        {
            try
            {
                // Get all done orders for this user, ordered by completion date (oldest first)
                var doneOrders = await _context.Orders
                    .Where(order => order.UserId == userId && order.Status == OrderStatus.Done)
                    .OrderBy(order => order.CompletionDate ?? order.StatusLastUpdated)
                    .ToListAsync();

                if (doneOrders.Any())
                {
                    // Remove all done orders
                    _context.Orders.RemoveRange(doneOrders);
                    await _context.SaveChangesAsync();

                    return new ServiceResponse(true, $"Cleaned up {doneOrders.Count} done orders for user {userId}.");
                }

                return new ServiceResponse(true, "No done orders found to cleanup.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Failed to cleanup done orders for user {userId}: {ex.Message}");
            }
        }
    }
}



//// NEW: Get orders by category
//public async Task<List<Order>> GetOrdersByCategory(string categoryName)
//{
//    try
//    {
//        return await _context.Orders
//            .AsNoTracking()
//            .Where(order => order.CategoryName.ToLower() == categoryName.ToLower())
//            .Select(order => new Order
//            {
//                OrderId = order.OrderId,
//                Name = order.Name,
//                UserName = order.UserName,
//                Comment = order.Comment,
//                PhoneNumber = order.PhoneNumber,
//                Location = order.Location,
//                Status = order.Status,
//                CategoryName = order.CategoryName,
//                StatusLastUpdated = order.StatusLastUpdated,
//                CreatedDate = order.CreatedDate
//            })
//            .OrderByDescending(order => order.CreatedDate)
//            .ToListAsync();
//    }
//    catch (Exception ex)
//    {
//        throw new Exception($"Error fetching orders for category {categoryName}", ex);
//    }
//}

//// NEW: Get orders by category and status
//public async Task<List<Order>> GetOrdersByCategoryAndStatus(string categoryName, OrderStatus status)
//{
//    try
//    {
//        return await _context.Orders
//            .AsNoTracking()
//            .Where(order => order.CategoryName.ToLower() == categoryName.ToLower() && order.Status == status)
//            .Select(order => new Order
//            {
//                OrderId = order.OrderId,
//                Name = order.Name,
//                UserName = order.UserName,
//                Comment = order.Comment,
//                PhoneNumber = order.PhoneNumber,
//                Location = order.Location,
//                Status = order.Status,
//                CategoryName = order.CategoryName,
//                StatusLastUpdated = order.StatusLastUpdated,
//                CreatedDate = order.CreatedDate
//            })
//            .OrderByDescending(order => order.CreatedDate)
//            .ToListAsync();
//    }
//    catch (Exception ex)
//    {
//        throw new Exception($"Error fetching orders for category {categoryName} with status {status}", ex);
//    }
//}

//// NEW: Get all available categories
//public async Task<List<string>> GetAllCategories()
//{
//    try
//    {
//        return await _context.Orders
//            .AsNoTracking()
//            .Select(order => order.CategoryName)
//            .Distinct()
//            .Where(category => !string.IsNullOrEmpty(category))
//            .OrderBy(category => category)
//            .ToListAsync();
//    }
//    catch (Exception ex)
//    {
//        throw new Exception("Error fetching categories", ex);
//    }
//}