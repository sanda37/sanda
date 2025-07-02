using System.ComponentModel.DataAnnotations;

namespace sanda.DTO
{
    public class VerifyTokenRequestDto
    {
        [Required]
        public string Token { get; set; }

    }
}
