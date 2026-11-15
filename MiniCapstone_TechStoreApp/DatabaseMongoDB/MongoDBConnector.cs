using Microsoft.VisualBasic.ApplicationServices;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace MiniCapstone_TechStoreApp.DatabaseMongoDB
{
    public static class MongoDBConnector
    {
        private static readonly string connectionString =
            Environment.GetEnvironmentVariable(
                "MINICAPSTONE_MONGODB_CONNECTIONSTRING"
            ) ?? throw new InvalidOperationException(
                "MongoDB connection string not found!"
            );

        private static readonly MongoClient client = new MongoClient(connectionString);

        public static IMongoDatabase Database = client.GetDatabase("TechStoreDB");

        public static IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }

        public static void TestDB(KryptonTextBox textBox)
        {
            try
            {
                StringBuilder output = new StringBuilder();

                // Get all databases
                var databases = MongoDBConnector.client
                    .ListDatabaseNames()
                    .ToList();

                output.AppendLine("========== MONGODB DATABASE TEST ==========");
                output.AppendLine();

                foreach (string databaseName in databases)
                {
                    output.AppendLine("DATABASE: " + databaseName);
                    output.AppendLine("-------------------------------------------");

                    var database = MongoDBConnector.client
                        .GetDatabase(databaseName);

                    // Get all collections
                    var collections = database
                        .ListCollectionNames()
                        .ToList();

                    if (collections.Count == 0)
                    {
                        output.AppendLine("  No collections found.");
                        output.AppendLine();
                        continue;
                    }

                    foreach (string collectionName in collections)
                    {
                        output.AppendLine();
                        output.AppendLine("  COLLECTION: " + collectionName);
                        output.AppendLine("  --------------------------------");

                        var collection = database
                            .GetCollection<BsonDocument>(collectionName);

                        // Retrieve all documents
                        var documents = collection
                            .Find(FilterDefinition<BsonDocument>.Empty)
                            .ToList();

                        output.AppendLine(
                            "  Total Documents: " + documents.Count
                        );

                        if (documents.Count == 0)
                        {
                            output.AppendLine("  No documents found.");
                            continue;
                        }
                    }

                    output.AppendLine();
                }

                output.AppendLine("========== END OF DATABASE TEST ==========");

                textBox.Text = output.ToString();
            }
            catch (Exception ex)
            {
                textBox.Text = "MONGODB ERROR:\r\n\r\n" + ex;
            }
        }
    }
}
