using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sanda.Data;
using sanda.DTO;
using sanda.Dtos;
using sanda.Models;
using sanda.Services;

namespace sanda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IMailingService _mailingService;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            UserDbContext context,
            IMailingService mailingService,
            ILogger<AuthController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _mailingService = mailingService;
            _logger = logger;
            _environment = environment;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid email format" });
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                var volunteer = await _context.Volunteers
                    .FirstOrDefaultAsync(v => v.Email == dto.Email);

                if (user == null && volunteer == null)
                {
                    // Security best practice: don't reveal if user exists
                    return Ok(new { message = "If an account exists, a reset link has been sent." });
                }

                var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
                var tokenExpiry = DateTime.UtcNow.AddHours(24);

                if (user != null)
                {
                    user.PasswordResetToken = token;
                    user.ResetTokenExpires = tokenExpiry;
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (volunteer != null)
                {
                    volunteer.PasswordResetToken = token;
                    volunteer.ResetTokenExpires = tokenExpiry;
                    volunteer.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Send email with proper logging
                var emailSent = await _mailingService.SendPasswordResetEmailAsync(dto.Email, token);

                if (!emailSent)
                {
                    _logger.LogError("Failed to send password reset email to {Email}", dto.Email);
                    // In development, return more information
                    if (_environment.IsDevelopment())
                    {
                        return StatusCode(500, new
                        {
                            message = "Password reset token generated but email failed to send.",
                            token = token // Only include in development for testing
                        });
                    }
                }

                return Ok(new { message = "If an account exists, a reset link has been sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPassword for email {Email}", dto.Email);

                if (_environment.IsDevelopment())
                {
                    return StatusCode(500, new
                    {
                        message = "An error occurred while processing your request.",
                        details = ex.Message
                    });
                }

                return StatusCode(500, new
                {
                    message = "An error occurred while processing your request."
                });
            }
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return BadRequest(new { message = "New password and confirmation don't match." });
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.PasswordResetToken == dto.Token &&
                        u.ResetTokenExpires > DateTime.UtcNow);

                var volunteer = await _context.Volunteers
                    .FirstOrDefaultAsync(v =>
                        v.PasswordResetToken == dto.Token &&
                        v.ResetTokenExpires > DateTime.UtcNow);

                if (user == null && volunteer == null)
                {
                    _logger.LogWarning("Invalid password reset token attempt: {Token}", dto.Token);
                    return BadRequest(new { message = "Invalid or expired token." });
                }

                // تم إزالة عملية التشفير هنا
                var plainPassword = dto.NewPassword;

                if (user != null)
                {
                    user.Password = plainPassword; // حفظ كلمة المرور كما هي
                    user.PasswordResetToken = null;
                    user.ResetTokenExpires = null;
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;

                    _logger.LogInformation("Password reset for user {Email}", user.Email);
                }
                else if (volunteer != null)
                {
                    volunteer.Password = plainPassword; // حفظ كلمة المرور كما هي
                    volunteer.PasswordResetToken = null;
                    volunteer.ResetTokenExpires = null;
                    volunteer.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(volunteer).State = EntityState.Modified;

                    _logger.LogInformation("Password reset for volunteer {Email}", volunteer.Email);
                }

                var changes = await _context.SaveChangesAsync();

                if (changes == 0)
                {
                    _logger.LogError("Password reset failed - no changes saved to database");
                    return StatusCode(500, new { message = "Failed to update password." });
                }

                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed for token {Token}", dto.Token);
                return StatusCode(500, new
                {
                    message = "An error occurred while resetting your password.",
                    detail = _environment.IsDevelopment() ? ex.Message : null
                });
            }
        }

        // Optional: Add endpoint to verify token validity
        [HttpPost("verify-reset-token")]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyTokenRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            try
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.PasswordResetToken == dto.Token && u.ResetTokenExpires > DateTime.UtcNow);

                var volunteerExists = await _context.Volunteers
                    .AnyAsync(v => v.PasswordResetToken == dto.Token && v.ResetTokenExpires > DateTime.UtcNow);

                var isValid = userExists || volunteerExists;

                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset token {Token}", dto.Token);
                return StatusCode(500, new { message = "An error occurred verifying the token." });
            }
        }
    }
}