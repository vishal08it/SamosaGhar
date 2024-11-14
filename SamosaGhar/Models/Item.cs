
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SamosaGhar.Models
{
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }  // New field for storing the image URL
    }
}
