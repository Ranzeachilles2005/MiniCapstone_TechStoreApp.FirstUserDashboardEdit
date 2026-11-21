using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiniCapstone_TechStoreApp.Models
{
    public class UserDB
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        public UserDB()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Username = "";
            Email = "";
            PasswordHash = "";
            Role = "Buyer";
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
