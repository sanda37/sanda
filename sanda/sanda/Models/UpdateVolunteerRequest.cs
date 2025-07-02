using System.ComponentModel.DataAnnotations;

namespace sanda.Models
{
    public class UpdateVolunteerRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NationalID { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? ProfileImage { get; set; }
        public string? NationalIDPath { get; set; }
        public int? MaxActiveOrders { get; set; }
        public bool PhysicalTherapy { get; internal set; }
        public bool Nursing { get; internal set; }
    }
}