using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using sanda.Models;
using sanda.Services;
using System.ComponentModel.DataAnnotations;
using sanda.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using sanda.Data;
using sanda.DTO;
using System.Net.Mail;
using System.Net;

[Route("api/[controller]")]
[ApiController]
public class VolunteerController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly IVolunteerService _volunteerService;
    private readonly IVolunteerBalanceService _balanceService;

    public VolunteerController(
        UserDbContext context,
        IVolunteerService volunteerService,
        IVolunteerBalanceService balanceService)
    {
        _context = context;
        _volunteerService = volunteerService;
        _balanceService = balanceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllVolunteers()
    {
        try
        {
            var volunteers = await _volunteerService.GetAllVolunteersAsync();
            if (volunteers == null || !volunteers.Any())
            {
                return NotFound(new { message = "No volunteers found." });
            }
            return Ok(volunteers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving volunteers.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVolunteerById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        var volunteer = await _volunteerService.GetVolunteerByIdAsync(id);
        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        return Ok(volunteer);
    }

    [HttpPost]
    public async Task<IActionResult> AddVolunteer([FromBody] Volunteer volunteer)
    {
        if (volunteer == null)
        {
            return BadRequest(new { message = "Invalid volunteer data." });
        }

        // التحقق من عدم اختيار الخدمتين معًا
        if (volunteer.Nursing && volunteer.PhysicalTherapy)
        {
            return BadRequest(new { message = "يمكنك اختيار خدمة واحدة فقط (تمريض أو علاج طبيعي)" });
        }

        var response = await _volunteerService.AddVolunteerAsync(volunteer);
        if (!response.Flag)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == request.Email);
        if (volunteer == null || string.IsNullOrEmpty(volunteer.Password) || volunteer.Password != request.Password)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(new { message = "Login successful!", volunteerId = volunteer.ID });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] sanda.Models.ResetPasswordRequest request)
    {
        var Volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == request.Email);

        if (Volunteer == null)
        {
            return NotFound(new { message = "Email not found" });
        }

        if (Volunteer.ResetCode != request.OtpCode || Volunteer.TokenExpiry < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Invalid or expired OTP code." });
        }

        Volunteer.Password = request.NewPassword; // Store new password as plain text
        Volunteer.ResetCode = null;
        Volunteer.TokenExpiry = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully." });
    }



    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] sanda.Models.ForgotPasswordRequest request)
    {
        var Volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == request.Email);
        if (Volunteer == null)
        {
            return NotFound(new { message = "Email not found" });
        }

        Random rnd = new Random();
        int otpCode = rnd.Next(100000, 999999);

        Volunteer.ResetCode = otpCode.ToString();
        Volunteer.TokenExpiry = DateTime.UtcNow.AddMinutes(10);
        await _context.SaveChangesAsync();

        bool emailSent = SendOtpEmail(Volunteer.Email, otpCode);

        if (!emailSent)
        {
            return StatusCode(500, new { message = "Failed to send OTP email" });
        }

        return Ok(new { message = "OTP sent to your email." });
    }

    private bool SendOtpEmail(string toEmail, int otpCode)
    {
        try
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);

            mail.From = new MailAddress("your-email@gmail.com");
            mail.To.Add(toEmail);
            mail.Subject = "Your Password Reset OTP Code";
            mail.Body = $"Your OTP Code is: {otpCode}. It is valid for 10 minutes.";
            mail.IsBodyHtml = true;

            smtp.Credentials = new NetworkCredential("your-email@gmail.com", "your-email-password");
            smtp.EnableSsl = true;
            smtp.Send(mail);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }




    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVolunteer(int id, [FromBody] UpdateVolunteerRequest request)
    {
        if (request == null || id <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID or data." });
        }

        // التحقق من عدم اختيار الخدمتين معًا
        if (request.Nursing && request.PhysicalTherapy)
        {
            return BadRequest(new { message = "يمكنك اختيار خدمة واحدة فقط (تمريض أو علاج طبيعي)" });
        }

        var existingVolunteer = await _volunteerService.GetVolunteerByIdAsync(id);
        if (existingVolunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        var volunteerToUpdate = new Volunteer
        {
            ID = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            NationalID = request.NationalID,
            Email = request.Email,
            Password = request.Password,
            Age = request.Age ?? 0,
            Gender = request.Gender,
            Address = request.Address,
            ProfileImage = request.ProfileImage,
            NationalIDPath = request.NationalIDPath,
            MaxActiveOrders = request.MaxActiveOrders ?? 0,
            Nursing = request.Nursing,
            PhysicalTherapy = request.PhysicalTherapy
        };

        var response = await _volunteerService.UpdateVolunteerAsync(volunteerToUpdate);
        if (!response.Flag)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(new { message = response.Message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVolunteer(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        var response = await _volunteerService.DeleteVolunteerAsync(id);
        if (!response.Flag)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(new { message = response.Message });
    }

    [HttpGet("{volunteerId}/available-orders")]
    public async Task<IActionResult> GetAvailableOrders(int volunteerId)
    {
        if (volunteerId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        var volunteer = await _volunteerService.GetVolunteerByIdAsync(volunteerId);
        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        var allOrders = await _volunteerService.GetAvailableOrdersAsync();
        if (allOrders == null)
        {
            return Ok(new List<object>());
        }

        var filteredOrders = allOrders.Where(order =>
        {
            // First check gender preference
            if (!string.IsNullOrEmpty(order.GenderPreference) &&
                order.GenderPreference.ToLower() != volunteer.Gender.ToLower())
            {
                return false;
            }

            // Then check specializations
            if (order.CategoryName.Equals("Nursing", StringComparison.OrdinalIgnoreCase))
            {
                return volunteer.Nursing;
            }

            if (order.CategoryName.Equals("PhysicalTherapy", StringComparison.OrdinalIgnoreCase))
            {
                return volunteer.PhysicalTherapy;
            }

            // Show all other categories
            return true;
        }).ToList();

        var formattedOrders = filteredOrders.Select(order => new
        {
            order.OrderId,
            order.Name,
            order.UserName,
            order.PhoneNumber,
            order.Comment,
            order.Location,
            order.CategoryName,
            Status = order.Status.ToString(),
            order.CreatedDate,
            order.ItemImage,
            order.ProductId,
            order.ServiceId,
            order.GenderPreference
        });

        return Ok(formattedOrders);
    }

    // ... بقية الأكواد بدون تغيير ...
    [HttpPost("{volunteerId}/accept-order/{orderId}")]
    public async Task<IActionResult> AcceptOrder(int volunteerId, int orderId)
    {
        if (volunteerId <= 0 || orderId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID or order ID." });
        }

        var volunteer = await _volunteerService.GetVolunteerByIdAsync(volunteerId);
        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        var response = await _volunteerService.AcceptOrderAsync(volunteerId, orderId);
        if (!response.Flag)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(new { message = response.Message });
    }

    [HttpPost("{volunteerId}/cancel-order/{orderId}")]
    public async Task<IActionResult> CancelOrder(int volunteerId, int orderId)
    {
        if (volunteerId <= 0 || orderId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID or order ID." });
        }

        var volunteer = await _volunteerService.GetVolunteerByIdAsync(volunteerId);
        if (volunteer == null)
        {
            return NotFound(new { message = "Volunteer not found." });
        }

        var response = await _volunteerService.CancelOrderAsync(volunteerId, orderId);
        if (!response.Flag)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(new { message = response.Message });
    }

    [HttpGet("{volunteerId}/accepted-orders")]
    public async Task<IActionResult> GetAcceptedOrders(int volunteerId)
    {
        if (volunteerId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        var orders = await _volunteerService.GetAcceptedOrdersAsync(volunteerId);
        if (orders == null)
        {
            return Ok(new List<object>());
        }

        var formattedOrders = orders.Select(order => new
        {
            order.OrderId,
            order.Name,
            order.UserName,
            order.PhoneNumber,
            order.Comment,
            order.Location,
            order.CategoryName,
            Status = order.Status.ToString(),
            order.CreatedDate,
            order.ItemImage,
            order.ProductId,
            order.ServiceId
        });

        return Ok(formattedOrders);
    }

    #region Balance Operations
    [HttpGet("{volunteerId}/balance")]
    public async Task<IActionResult> GetBalance(int volunteerId)
    {
        if (volunteerId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        try
        {
            var balance = await _balanceService.GetBalanceAsync(volunteerId);
            return Ok(new { volunteerId, balance });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{volunteerId}/deposit")]
    public async Task<IActionResult> Deposit(
        int volunteerId,
        [FromBody] BalanceOperationRequest request)
    {
        if (volunteerId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        try
        {
            var volunteer = await _balanceService.DepositAsync(volunteerId, request.Amount);
            return Ok(new
            {
                volunteerId = volunteer.ID,
                newBalance = volunteer.Balance,
                message = "Deposit successful"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{volunteerId}/withdraw")]
    public async Task<IActionResult> Withdraw(
        int volunteerId,
        [FromBody] BalanceOperationRequest request)
    {
        if (volunteerId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        try
        {
            var volunteer = await _balanceService.WithdrawAsync(volunteerId, request.Amount);
            return Ok(new
            {
                volunteerId = volunteer.ID,
                newBalance = volunteer.Balance,
                message = "Withdrawal successful"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{volunteerId}/can-withdraw")]
    public async Task<IActionResult> CanWithdraw(
        int volunteerId,
        [FromBody] BalanceOperationRequest request)
    {
        if (volunteerId <= 0)
        {
            return BadRequest(new { message = "Invalid volunteer ID." });
        }

        try
        {
            var canWithdraw = await _balanceService.CanWithdrawAsync(volunteerId, request.Amount);
            return Ok(new
            {
                canWithdraw,
                message = canWithdraw ? "Sufficient funds" : "Insufficient funds"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    #endregion
}