using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sanda.Models;

namespace sanda.Models
{
    public enum OrderStatus
    {

        Pending = 0, // Fixed spelling
        Accepted = 1,
        InProgress = 2,
        Done = 3,
        Cancelled = 4,
    }

    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? InProgressDate { get; set; } // Nullable for flexibility

        public DateTime? CompletionDate { get; set; } // Tracks when the order was marked as done

        public string Name { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string Comment { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [NotMapped]
        public string StatusName => Status.ToString();

        // Geographic location coordinates
        public string Location { get; set; } // Latitude and Longitude



        [Required]
        public string CategoryName { get; set; } = string.Empty;


        public int? ProductId { get; set; }
        public int? ServiceId { get; set; }

        // Store the image URL directly in the order
        public string? ItemImage { get; set; }


        // Track when status changes
        [DataType(DataType.DateTime)]
        public DateTime StatusLastUpdated { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        public int? VolunteerId { get; internal set; }

        public virtual User? User { get; set; }



        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("ServiceId")]
        public virtual ServiceItem? Service { get; set; }
        public string? GenderPreference { get; set; } // "male", "female", or null for no preference

    }
}