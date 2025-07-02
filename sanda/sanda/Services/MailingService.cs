using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MimeKit;
using sanda.Settings;
using sanda.Dtos;

namespace sanda.Services
{
    public class MailingService : IMailingService
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MailingService> _logger;

        public MailingService(
            IOptions<MailSettings> mailSettings,
            IWebHostEnvironment env,
            ILogger<MailingService> logger)
        {
            _mailSettings = mailSettings.Value;
            _env = env;
            _logger = logger;
        }

        public async Task SendEmailAsync(string mailto, string subject, string body, IList<IFormFile> attachments = null)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Email);
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));
            email.To.Add(MailboxAddress.Parse(mailto));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            if (attachments != null)
            {
                foreach (var item in attachments)
                {
                    if (item.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await item.CopyToAsync(ms);
                        builder.Attachments.Add(item.FileName, ms.ToArray(), ContentType.Parse(item.ContentType));
                    }
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);
                await smtp.SendAsync(email);
                _logger.LogInformation($"Email sent to {mailto}");
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "SMTP authentication failed");
                throw new Exception($"SMTP authentication failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string token)
        {
            try
            {
                var resetLink = $"https://yourapp.com/reset-password?token={token}";
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sanda App", _mailSettings.Email));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Password Reset Request";

                message.Body = new TextPart("html")
                {
                    Text = $"Please reset your password by <a href='{resetLink}'>clicking here</a>"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

       
    }
}