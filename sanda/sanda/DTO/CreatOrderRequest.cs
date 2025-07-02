namespace sanda.DTOs
{
    public class CreateOrderRequest
    {
        public string Name { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string CategoryName { get; set; }
        public int? ProductId { get; set; }
        public int? ServiceId { get; set; }
        public string ItemImage { get; set; }
    }
}