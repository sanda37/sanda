using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sanda.Data;
using sanda.Models;
using sanda.Services;

public class VolunteerService : IVolunteerService
{
    private readonly UserDbContext _context;

    public VolunteerService(UserDbContext context)
    {
        _context = context;
    }

    // Retrieve all volunteers
    public async Task<IEnumerable<Volunteer>> GetAllVolunteersAsync()
    {
        return await _context.Volunteers.ToListAsync();
    }

    // Retrieve a volunteer by ID
    public async Task<Volunteer> GetVolunteerByIdAsync(int id)
    {
        return await _context.Volunteers.FindAsync(id);
    }

    // Add a new volunteer
    public async Task<sanda.Models.ServiceResponse> AddVolunteerAsync(Volunteer volunteer)
    {
        try
        {
            await _context.Volunteers.AddAsync(volunteer);
            await _context.SaveChangesAsync();
            return new sanda.Models.ServiceResponse(true, "Volunteer added successfully.");
        }
        catch (Exception ex)
        {
            return new sanda.Models.ServiceResponse(false, $"An error occurred: {ex.Message}");
        }
    }

    // Update an existing volunteer
    // Update an existing volunteer
    public async Task<sanda.Models.ServiceResponse> UpdateVolunteerAsync(Volunteer volunteer)
    {
        try
        {
            var existingVolunteer = await _context.Volunteers.FindAsync(volunteer.ID);
            if (existingVolunteer == null)
            {
                return new sanda.Models.ServiceResponse(false, "Volunteer not found.");
            }

            // Update only the fields that are provided (not null or empty)
            if (!string.IsNullOrWhiteSpace(volunteer.FirstName))
                existingVolunteer.FirstName = volunteer.FirstName;

            if (!string.IsNullOrWhiteSpace(volunteer.LastName))
                existingVolunteer.LastName = volunteer.LastName;

            if (!string.IsNullOrWhiteSpace(volunteer.Email))
                existingVolunteer.Email = volunteer.Email;

            if (!string.IsNullOrWhiteSpace(volunteer.PhoneNumber))
                existingVolunteer.PhoneNumber = volunteer.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(volunteer.NationalID))
                existingVolunteer.NationalID = volunteer.NationalID;

            if (!string.IsNullOrWhiteSpace(volunteer.Password))
                existingVolunteer.Password = volunteer.Password;

            if (volunteer.Age > 0)
                existingVolunteer.Age = volunteer.Age;

            if (!string.IsNullOrWhiteSpace(volunteer.Gender))
                existingVolunteer.Gender = volunteer.Gender;

            if (!string.IsNullOrWhiteSpace(volunteer.Address))
                existingVolunteer.Address = volunteer.Address;

            if (!string.IsNullOrWhiteSpace(volunteer.ProfileImage))
                existingVolunteer.ProfileImage = volunteer.ProfileImage;

            if (!string.IsNullOrWhiteSpace(volunteer.NationalIDPath))
                existingVolunteer.NationalIDPath = volunteer.NationalIDPath;

            // Update system fields only if they are explicitly set
            if (volunteer.MaxActiveOrders > 0)
                existingVolunteer.MaxActiveOrders = volunteer.MaxActiveOrders;

            // Don't update CurrentActiveOrders and LastOrderAcceptedDate through regular update
            // These should be managed by the system automatically

            _context.Volunteers.Update(existingVolunteer);
            await _context.SaveChangesAsync();

            return new sanda.Models.ServiceResponse(true, "Volunteer updated successfully.");
        }
        catch (Exception ex)
        {
            return new sanda.Models.ServiceResponse(false, $"An error occurred while updating the volunteer: {ex.Message}");
        }
    }

    // Delete a volunteer by ID
    public async Task<sanda.Models.ServiceResponse> DeleteVolunteerAsync(int id)
    {
        try
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer == null)
            {
                return new sanda.Models.ServiceResponse(false, "Volunteer not found.");
            }

            _context.Volunteers.Remove(volunteer);
            await _context.SaveChangesAsync();

            return new sanda.Models.ServiceResponse(true, "Volunteer deleted successfully.");
        }
        catch (Exception ex)
        {
            return new sanda.Models.ServiceResponse(false, $"An error occurred while deleting the volunteer: {ex.Message}");
        }
    }

    // Retrieve all available orders that can be accepted by volunteers
    public async Task<List<Order>> GetAvailableOrdersAsync()
    {
        return await _context.Orders
            .Where(order => order.Status == OrderStatus.Pending && order.VolunteerId == null)
            .ToListAsync();
    }









    public async Task<List<Order>> GetAvailableOrdersByCategoryAsync(string categoryName)
    {
        return await _context.Orders
            .Where(order => order.Status == OrderStatus.Pending &&
                           order.VolunteerId == null &&
                           order.CategoryName.ToLower() == categoryName.ToLower())
            .ToListAsync();
    }









    // Volunteer accepts an order
    public async Task<sanda.Models.ServiceResponse> AcceptOrderAsync(int volunteerId, int orderId)
    {
        var volunteer = await GetVolunteerByIdAsync(volunteerId);
        if (volunteer == null)
        {
            return new sanda.Models.ServiceResponse(false, "Volunteer not found.");
        }

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.Pending || order.VolunteerId != null)
        {
            return new sanda.Models.ServiceResponse(false, "Order not available for acceptance.");
        }

        // Assign the order to the volunteer
        order.VolunteerId = volunteerId;
        order.Status = OrderStatus.Accepted;

        volunteer.CurrentActiveOrders++; // Keep tracking active orders if needed
        volunteer.LastOrderAcceptedDate = DateTime.Now;

        _context.Orders.Update(order);
        _context.Volunteers.Update(volunteer);
        await _context.SaveChangesAsync();

        return new sanda.Models.ServiceResponse(true, "Order accepted successfully.");
    }

    // Volunteer cancels an order
    public async Task<sanda.Models.ServiceResponse> CancelOrderAsync(int volunteerId, int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return new sanda.Models.ServiceResponse(false, "Order not found.");
        }

        if (order.VolunteerId != volunteerId)
        {
            return new sanda.Models.ServiceResponse(false, "Order not assigned to this volunteer.");
        }

        var volunteer = await GetVolunteerByIdAsync(volunteerId);
        if (volunteer == null)
        {
            return new sanda.Models.ServiceResponse(false, "Volunteer not found.");
        }

        order.VolunteerId = null;
        order.Status = OrderStatus.Pending;
        order.InProgressDate = null;

        if (volunteer.CurrentActiveOrders > 0)
        {
            volunteer.CurrentActiveOrders--;
        }

        _context.Orders.Update(order);
        _context.Volunteers.Update(volunteer);
        await _context.SaveChangesAsync();

        return new sanda.Models.ServiceResponse(true, "Order canceled successfully.");
    }

    public async Task<List<Order>> GetAcceptedOrdersAsync(int volunteerId)
    {
        var volunteer = await GetVolunteerByIdAsync(volunteerId);
        if (volunteer == null)
        {
            throw new Exception("Volunteer not found.");
        }

        return await _context.Orders
            .Where(order => order.VolunteerId == volunteerId && order.Status != OrderStatus.Done)
            .ToListAsync();
    }





    public async Task<List<Order>> GetAcceptedOrdersByCategoryAsync(int volunteerId, string categoryName)
    {
        var volunteer = await GetVolunteerByIdAsync(volunteerId);
        if (volunteer == null)
        {
            throw new Exception("Volunteer not found.");
        }

        return await _context.Orders
            .Where(order => order.VolunteerId == volunteerId &&
                           order.Status != OrderStatus.Done &&
                           order.CategoryName.ToLower() == categoryName.ToLower())
            .ToListAsync();
    }
}