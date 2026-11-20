using Microsoft.VisualBasic.ApplicationServices;
using MiniCapstone_TechStoreApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniCapstone_TechStoreApp.DatabaseMongoDB
{
    public class MongoDBConnector
    {
        private IMongoDatabase database;
        public MongoDBConnector()
        {
            string connectionString = Environment.GetEnvironmentVariable("MINICAPSTONE_MONGODB_CONNECTIONSTRING");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("MongoDB connection string is missing.");
            }

            MongoClient client = new MongoClient(connectionString);

            database = client.GetDatabase("TechStoreDB");
        }

        public IMongoCollection<UserDB> Users
        {
            get 
            { 
                return database.GetCollection<UserDB>("Users"); 
            }
        }

        public IMongoCollection<EmailVerification> EmailVerifications
        {
            get
            {
                return database.GetCollection<EmailVerification>("EmailVerifications");
            }
        }

        public async Task CreateIndexesAsync()
        {
            // Prevent duplicate user email addresses.
            CreateIndexOptions userEmailOptions =
                new CreateIndexOptions { Unique = true };

            IndexKeysDefinition<UserDB> userEmailIndex =
                Builders<UserDB>.IndexKeys.Ascending(x => x.Email);

            await Users.Indexes.CreateOneAsync(
                new CreateIndexModel<UserDB>(
                    userEmailIndex,
                    userEmailOptions));

            // One verification document per email address.
            CreateIndexOptions verificationOptions =
                new CreateIndexOptions { Unique = true };

            IndexKeysDefinition<EmailVerification> verificationIndex =
                Builders<EmailVerification>.IndexKeys.Ascending(
                    x => x.Email);

            await EmailVerifications.Indexes.CreateOneAsync(
                new CreateIndexModel<EmailVerification>(
                    verificationIndex,
                    verificationOptions));

            // Automatically remove expired verification documents.
            IndexKeysDefinition<EmailVerification> expiryIndex =
                Builders<EmailVerification>.IndexKeys.Ascending(
                    x => x.ExpiresAt);

            CreateIndexOptions expiryOptions =
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };

            await EmailVerifications.Indexes.CreateOneAsync(
                new CreateIndexModel<EmailVerification>(
                    expiryIndex,
                    expiryOptions));
        }
    }
}
