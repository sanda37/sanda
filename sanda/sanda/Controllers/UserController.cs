using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sanda.Models;
using sanda.Data;
using System.Net.Mail;
using System.Net;

namespace sanda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;

        public UserController(UserDbContext context)
        {
            _context = context;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already registered." });
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                NationalId = request.NationalId,
                Email = request.Email,
                Password = request.Password, // Store password as plain text
                Age = request.Age,
                Gender = request.Gender,
                Address = request.Address,
                ProfilePicturePath = request.ProfilePicturePath,
                HasMobilityDisability = request.HasMobilityDisability,
                DisabilityProofPath = request.DisabilityProofPath
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Check if password is null or empty
            if (string.IsNullOrEmpty(user.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Direct string comparison instead of hash verification
            if (user.Password != request.Password)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            return Ok(new { message = "Login successful!", userId = user.Id });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] sanda.Models.ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return NotFound(new { message = "Email not found" });
            }

            if (user.ResetCode != request.OtpCode || user.TokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired OTP code." });
            }

            user.Password = request.NewPassword; // Store new password as plain text
            user.ResetCode = null;
            user.TokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("orders/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId && o.Status != OrderStatus.Done)
                .Select(order => new
                {
                    order.OrderId,
                    order.Name,
                    order.UserName,
                    order.Comment,
                    order.PhoneNumber,
                    order.Location,
                    order.CategoryName,
                    Status = order.Status.ToString(),
                    order.CreatedDate,
                    // NEW: Include image and item details
                    order.ItemImage,
                    order.ProductId,
                    order.ServiceId
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.PhoneNumber,
                    u.NationalId,
                    u.Email,
                    u.Age,
                    u.Gender,
                    u.Address,
                    u.ProfilePicturePath,
                    u.HasMobilityDisability,
                    u.DisabilityProofPath
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        [HttpPost("upload-profile-picture/{id}")]
        public async Task<IActionResult> UploadProfilePicture(int id, [FromForm] IFormFile file)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Invalid file." });
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePicturePath = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile picture uploaded successfully.", profilePicturePath = user.ProfilePicturePath });
        }

        [HttpPut("update-profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.Age = request.Age ?? user.Age;
            user.Gender = request.Gender ?? user.Gender;
            user.Address = request.Address ?? user.Address;
            user.ProfilePicturePath = request.ProfilePicturePath ?? user.ProfilePicturePath;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] sanda.Models.ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "Email not found" });
            }

            Random rnd = new Random();
            int otpCode = rnd.Next(100000, 999999);

            user.ResetCode = otpCode.ToString();
            user.TokenExpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            bool emailSent = SendOtpEmail(user.Email, otpCode);

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
        //[HttpGet("{userId}/favorites")]
        //public async Task<IActionResult> GetUserFavoriteServices(int userId)
        //{
        //    var favorites = await _context.FavoriteServices
        //        .Where(f => f.UserId == userId)
        //        .Include(f => f.Service)
        //        .Select(f => new
        //        {
        //            f.Id,
        //            f.ServiceId,
        //            ServiceName = f.Service.Name,
        //            ServiceImage = f.Service.Image,
        //            ServicePrice = f.Service.Price,
        //            ServiceCategory = f.Service.Category,
        //            f.AddedDate
        //        })
        //        .ToListAsync();

        //    return Ok(favorites);
        //}

        //[HttpPost("{userId}/favorites/{serviceId}")]
        //public async Task<IActionResult> AddServiceToFavorites(int userId, int serviceId)
        //{
        //    // Check if user exists
        //    var user = await _context.Users.FindAsync(userId);
        //    if (user == null)
        //    {
        //        return NotFound(new { message = "User not found." });
        //    }

        //    // Check if service exists
        //    var service = await _context.ServiceItems.FindAsync(serviceId);
        //    if (service == null)
        //    {
        //        return NotFound(new { message = "Service not found." });
        //    }

        //    // Check if already favorited
        //    var existingFavorite = await _context.FavoriteServices
        //        .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceId == serviceId);

        //    if (existingFavorite != null)
        //    {
        //        return BadRequest(new { message = "Service is already in favorites." });
        //    }

        //    var favorite = new FavoriteService
        //    {
        //        UserId = userId,
        //        ServiceId = serviceId
        //    };

        //    _context.FavoriteServices.Add(favorite);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Service added to favorites successfully.", favoriteId = favorite.Id });
        //}

        //[HttpDelete("{userId}/favorites/{favoriteId}")]
        //public async Task<IActionResult> RemoveServiceFromFavorites(int userId, int favoriteId)
        //{
        //    var favorite = await _context.FavoriteServices
        //        .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

        //    if (favorite == null)
        //    {
        //        return NotFound(new { message = "Favorite not found." });
        //    }

        //    _context.FavoriteServices.Remove(favorite);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Service removed from favorites successfully." });
        //}

        //[HttpGet("{userId}/favorites/check/{serviceId}")]
        //public async Task<IActionResult> CheckIfServiceIsFavorite(int userId, int serviceId)
        //{
        //    var isFavorite = await _context.FavoriteServices
        //        .AnyAsync(f => f.UserId == userId && f.ServiceId == serviceId);

        //    return Ok(new { isFavorite });
        //}
    }


}