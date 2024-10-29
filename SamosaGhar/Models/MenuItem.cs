using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SamosaGhar.Models
{
    public class MenuItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Image { get; set; }
    }
}
