
//using MongoDB.Driver;

//namespace SamosaGhar.Config
//{
//    public class MongoDBConfig
//    {
//        private readonly IMongoDatabase _database;

//        public MongoDBConfig(string connectionString, string databaseName)
//        {
//            var client = new MongoClient(connectionString);
//            _database = client.GetDatabase(databaseName);
//        }

//        public IMongoCollection<T> GetCollection<T>(string name)
//        {
//            return _database.GetCollection<T>(name);
//        }
//    }
//}
//using MongoDB.Driver;

//namespace SamosaGhar.Config
//{
//    public class MongoDBConfig
//    {
//        private readonly IMongoDatabase _database;

//        public MongoDBConfig(string connectionString, string databaseName)
//        {
//            var client = new MongoClient(connectionString);
//            _database = client.GetDatabase(databaseName);
//        }

//        public IMongoCollection<T> GetCollection<T>(string name)
//        {
//            return _database.GetCollection<T>(name);
//        }
//    }
//}

//using Microsoft.Extensions.Configuration;
//using MongoDB.Driver;

//namespace SamosaGhar.Config
//{
//    public class MongoDBConfig
//    {
//        private readonly IMongoDatabase _database;

//        public MongoDBConfig(IConfiguration configuration)
//        {
//            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

//            var connectionString = configuration[$"MongoDB:{environment}:ConnectionString"];
//            var databaseName = configuration[$"MongoDB:{environment}:DatabaseName"];

//            var client = new MongoClient(connectionString);
//            _database = client.GetDatabase(databaseName);
//        }

//        public IMongoCollection<T> GetCollection<T>(string name)
//        {
//            return _database.GetCollection<T>(name);
//        }
//    }
//}
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace SamosaGhar.Config
{
    public class MongoDBConfig
    {
        private readonly IMongoDatabase _database;

        public MongoDBConfig(IConfiguration config, IWebHostEnvironment env)
        {
            string connectionString;
            string databaseName;

            if (env.IsDevelopment())
            {
                connectionString = config.GetSection("MongoDB:Development:ConnectionString").Value;
                databaseName = config.GetSection("MongoDB:Development:DatabaseName").Value;
            }
            else
            {
                connectionString = config.GetSection("MongoDB:Production:ConnectionString").Value;
                databaseName = config.GetSection("MongoDB:Production:DatabaseName").Value;
            }

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}

