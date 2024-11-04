using MongoDB.Bson;

namespace SamosaGhar.Models
{
    public class CartItemResponse
    {
        public string Id { get; set; }
        public string ItemName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
