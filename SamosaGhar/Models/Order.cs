using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SamosaGhar.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
         public ObjectId Id { get; set; } // MongoDB will automatically generate this field

        public string UserId { get; set; } // UserId (Mobile number of the user)
        public List<OrderItem> Items { get; set; } // List of order items
        public Address Address { get; set; } // Shipping address, which also contains UserId
        public string PaymentMethod { get; set; } // Payment method (e.g., "Credit Card", "Cash")
        public decimal TotalAmount { get; set; } // Total amount for the order
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Order date
    }

    public class OrderItem
    {
        public string Name { get; set; } // Item name
        public int Quantity { get; set; } // Item quantity
        public decimal Price { get; set; } // Price of the item
        public string ImageUrl { get; set; } // Item image URL
    }
}
