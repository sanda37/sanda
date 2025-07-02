// IMailingService.cs
namespace sanda.Services
{
    public interface IMailingService
    {
        Task SendEmailAsync(string mailto, string subject, string body, IList<IFormFile> attachments = null);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken); // Changed to return bool
    }
}