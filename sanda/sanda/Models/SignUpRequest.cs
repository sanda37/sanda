using System.ComponentModel.DataAnnotations;

namespace sanda.Models
{
    public class SignUpRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public bool HasMobilityDisability { get; set; }

        [RequireIfMobilityDisability]
        public string? DisabilityProofPath { get; set; } = string.Empty;// رابط صورة الإثبات
    }
}