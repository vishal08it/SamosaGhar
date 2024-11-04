using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SamosaGhar.Models
{
    public class CartItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime AddedDate { get; set; }
    }
}