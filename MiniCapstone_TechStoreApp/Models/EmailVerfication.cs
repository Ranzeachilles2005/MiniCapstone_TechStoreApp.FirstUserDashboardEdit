using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiniCapstone_TechStoreApp.Models
{
    public class EmailVerification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Email { get; set; }
        public string CodeHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastSentAt { get; set; }

        public EmailVerification()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Email = "";
            CodeHash = "";
            Attempts = 0;
            IsUsed = false;
        }
    }
}
