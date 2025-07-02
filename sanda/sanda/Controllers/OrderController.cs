using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sanda.Data;
using sanda.DTOs;
using sanda.Models;
using sanda.Services;

namespace sanda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserDbContext _dbContext;

        public OrderController(IOrderService orderService, UserDbContext dbContext)
        {
            _orderService = orderService;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null || request.UserId <= 0)
                return BadRequest(new { message = "Invalid order data." });

            var order = new Order
            {
                Name = request.Name,
                UserId = request.UserId,
                Comment = request.Comment,
                PhoneNumber = request.PhoneNumber,
                Location = request.Location,
                CategoryName = request.CategoryName,
                ProductId = request.ProductId,
                ServiceId = request.ServiceId,
                ItemImage = request.ItemImage //  <--  إضافة هذا السطر المهم

            };

            // Auto-populate image and item details based on ProductId or ServiceId
            if (request.ProductId.HasValue)
            {
                var product = await _dbContext.Products.FindAsync(request.ProductId.Value);
                if (product != null)
                {
                    order.ItemImage = product.Image;
                    order.CategoryName = product.Category;
                }
            }
            else if (request.ServiceId.HasValue)
            {
                var service = await _dbContext.ServiceItems.FindAsync(request.ServiceId.Value);
                if (service != null)
                {
                    order.ItemImage = service.Image;
                    order.CategoryName = service.Category;
                }
            }

            var data = await _orderService.AddOrder(order, order.UserId);
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var data = await _orderService.GetAllOrdersAsyn();
            var ordersWithImages = data.Select(order => new
            {
                order.OrderId,
                order.Name,
                order.UserName,
                order.Comment,
                order.PhoneNumber,
                order.Location,
                Status = order.Status.ToString(),
                order.CreatedDate,
                order.CategoryName,
                order.ItemImage,
                order.ProductId,
                order.ServiceId
            });

            return Ok(ordersWithImages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByID(id);
            if (order == null) return NotFound();

            var orderDetails = new
            {
                order.OrderId,
                order.Name,
                order.UserName,
                order.Comment,
                order.PhoneNumber,
                order.Location,
                Status = order.Status.ToString(),
                order.CreatedDate,
                order.CategoryName,
                order.ItemImage,
                order.ProductId,
                order.ServiceId
            };

            return Ok(orderDetails);
        }

        [HttpPost("{id},{status}/update-status")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _orderService.GetOrderByID(id);
            if (order == null) return NotFound(new { message = "Order not found." });

            order.Status = status;
            order.StatusLastUpdated = DateTime.UtcNow;

            // Set completion date when order is marked as done
            if (status == OrderStatus.Done)
            {
                order.CompletionDate = DateTime.UtcNow;
            }

            var response = await _orderService.UpdateOrder(order);
            if (!response.Flag) return BadRequest(response.message);

            return Ok(new { message = "Order status updated successfully." });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderService.GetOrderByID(id);
            if (order == null) return NotFound(new { message = "Order not found." });

            // Check if order status is already InProgress or beyond
            if (order.Status >= OrderStatus.InProgress)
            {
                return BadRequest(new { message = "Cannot cancel order - volunteer has already started working on it." });
            }

            var response = await _orderService.DeleteOrder(id);
            if (!response.Flag) return BadRequest(new { message = response.message });

            return Ok(new { message = "Order canceled and removed from system successfully." });
        }

        // New endpoint to manually trigger cleanup for a specific user (optional)
        [HttpPost("cleanup-done-orders/{userId}")]
        public async Task<IActionResult> CleanupDoneOrders(int userId)
        {
            var response = await _orderService.CleanupDoneOrdersForUser(userId);
            if (!response.Flag) return BadRequest(new { message = response.message });

            return Ok(new { message = response.message });
        }

        // New endpoint to get user order count (optional)
        [HttpGet("user/{userId}/count")]
        public async Task<IActionResult> GetUserOrderCount(int userId)
        {
            try
            {
                var count = await _orderService.GetUserOrderCount(userId);
                return Ok(new { userId = userId, totalOrders = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}