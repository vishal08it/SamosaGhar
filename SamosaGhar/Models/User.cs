using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SamosaGhar.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        public string MobileNumber { get; set; }
        public string Password { get; set; }

     
        public User()
        {
            Id = Guid.NewGuid().ToString(); 
        }
    }
}

