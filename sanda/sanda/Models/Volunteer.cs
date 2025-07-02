using sanda.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ServiceProcess;


public class Volunteer
{
    [Key]
    public int ID { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required, Phone]
    public string PhoneNumber { get; set; }

    [Required, StringLength(14, MinimumLength = 14)]
    public string NationalID { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; } // 
    [Required, Range(18, 100)]
    public int Age { get; set; }
    [Required]
    public string Gender { get; set; }
    public string Address { get; set; }
    public string? ProfileImage { get; set; }
    public string? NationalIDPath { get; set; }
    public virtual ICollection<Order> AcceptedOrders { get; set; } = new List<Order>();
    public int MaxActiveOrders { get; set; } = 3;
    public int CurrentActiveOrders { get; set; } = 0;
    public DateTime? LastOrderAcceptedDate { get; set; }
    public DateTime? UpdatedAt { get; internal set; } = DateTime.UtcNow;
    public string? PasswordResetToken { get; internal set; }
    public DateTime? ResetTokenExpires { get; internal set; }
    public decimal Balance { get; set; } = 0;
    public ServiceType? SelectedService { get; set; }
    public bool Nursing { get; set; } = false;
    public bool PhysicalTherapy { get; set; } = false;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ResetCode { get; set; }
    public DateTime? TokenExpiry { get; set; }


}

public enum ServiceType
{
    None = 0,
    Nursing = 1,
    PhysicalTherapy = 2
}