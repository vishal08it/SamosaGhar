
using MongoDB.Driver;

namespace SamosaGhar.Config
{
    public class MongoDBConfig
    {
        private readonly IMongoDatabase _database;

        public MongoDBConfig(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
