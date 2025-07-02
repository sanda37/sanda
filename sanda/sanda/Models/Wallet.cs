namespace sanda.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // Foreign key to User
        public User User { get; set; }   // Navigation property
        public decimal Balance { get; set; } // Fixed typo from 'balace'
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}