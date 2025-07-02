// ForgotPasswordRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace sanda.Dtos
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

// ResetPasswordRequestDto.cs
namespace sanda.Dtos
{
    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}