using sanda.Models;

public class Product
{
    public int Id { get; set; }  // Auto-incremented in DB
    public string Name { get; set; }
    public string? Image { get; set; } // URL or base64 string
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public string Category { get; set; }
    //public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

}