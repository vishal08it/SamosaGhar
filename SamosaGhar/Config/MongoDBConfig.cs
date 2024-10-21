using MongoDB.Driver;

namespace SamosaGhar.Config
{
    public class MongoDBConfig
    {
        private readonly IMongoDatabase _database;

        // Constructor to accept connection string and database name
        public MongoDBConfig(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString); 
            _database = client.GetDatabase(databaseName);   
        }

        // Generic method to get a collection from the database
        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name); 
        }
    }
}
