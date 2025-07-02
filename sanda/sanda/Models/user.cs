    namespace sanda.Models
{
    public class User
    {
        public int Id { get; set; }
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
        public string DisabilityProofPath { get; set; } = string.Empty;
        public string? ResetCode { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public List<ServiceItem> Services { get; set; } = new();
        public List<FavoriteService> FavoriteServices { get; set; } = new();
        public string? PasswordResetToken { get; internal set; }
        public DateTime? ResetTokenExpires { get; internal set; }
        public DateTime? UpdatedAt { get; internal set; }
        public Wallet Wallet { get; set; } = new Wallet();

    }
}