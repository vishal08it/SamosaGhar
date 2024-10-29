//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using System;

//namespace SamosaGhar.Models
//{
//    public class User
//    {
//        [BsonId]
//        [BsonRepresentation(BsonType.String)]
//        public string Id { get; set; }

//        public string MobileNumber { get; set; }
//        public string Password { get; set; }


//        public User()
//        {
//            Id = Guid.NewGuid().ToString(); 
//        }
//    }
//}

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

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("MobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        public User()
        {
            // Generate a unique Id when creating a new User
            Id = Guid.NewGuid().ToString();
        }
    }
}
