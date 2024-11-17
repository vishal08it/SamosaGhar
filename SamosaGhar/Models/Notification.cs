using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SamosaGhar.Models
{
    public class Notification
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }




}
