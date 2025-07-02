using System.Collections.Generic;
using System.Threading.Tasks;
using sanda.Models;
using sanda.Services;

public interface IVolunteerService
{
    // Existing methods
    Task<IEnumerable<Volunteer>> GetAllVolunteersAsync();
    Task<Volunteer> GetVolunteerByIdAsync(int id);
    Task<sanda.Models.ServiceResponse> AddVolunteerAsync(Volunteer volunteer);
    Task<sanda.Models.ServiceResponse> UpdateVolunteerAsync(Volunteer volunteer);
    Task<sanda.Models.ServiceResponse> DeleteVolunteerAsync(int id);

    // New methods
    Task<List<Order>> GetAvailableOrdersAsync(); // Get all orders that can be accepted
    Task<sanda.Models.ServiceResponse> AcceptOrderAsync(int volunteerId, int orderId); // Accept an order
    Task<sanda.Models.ServiceResponse> CancelOrderAsync(int volunteerId, int orderId); // Cancel an accepted order
    Task<List<Order>> GetAcceptedOrdersAsync(int volunteerId); // Get all orders currently accepted by the volunteer
    //Task<bool> CheckActiveOrdersAsync(int volunteerId); // للتحقق من الطلبات النشطة
    Task<List<Order>> GetAvailableOrdersByCategoryAsync(string categoryName); // Get available orders by category
    Task<List<Order>> GetAcceptedOrdersByCategoryAsync(int volunteerId, string categoryName); // Get accepted orders by category for volunteer

}
