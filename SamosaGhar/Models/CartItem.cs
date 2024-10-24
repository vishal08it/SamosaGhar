using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SamosaGhar.Models
{
    public class CartItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        
        public CartItem()
        {
            Id = Guid.NewGuid().ToString(); 
        }
    }
}
