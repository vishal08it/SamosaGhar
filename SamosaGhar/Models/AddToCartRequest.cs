namespace SamosaGhar.Models
{
    public class AddToCartRequest
    {
        public string MobileNumber { get; set; }
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
