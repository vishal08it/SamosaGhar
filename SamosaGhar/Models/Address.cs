using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SamosaGhar.Models
{
    public class Address
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // Change to ObjectId

        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Pincode { get; set; }
        public string Flat { get; set; }
        public string Area { get; set; }
        public string Landmark { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
