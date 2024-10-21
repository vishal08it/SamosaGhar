using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SamosaGhar.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register MongoDBConfig as a singleton service
builder.Services.AddSingleton(sp =>
{
    var configSection = builder.Configuration.GetSection("MongoDB");
    string connectionString = configSection["ConnectionString"];
    string databaseName = configSection["DatabaseName"];

    return new MongoDBConfig(connectionString, databaseName);
});

// Set the server URL to localhost:7200
builder.WebHost.UseUrls("http://localhost:7200");

var app = builder.Build();

// Configure the HTTP request pipeline

// Redirect base URL to the welcome message
app.MapGet("/", () => Results.Content(
    "<h1 style='text-align:center;color:green;'>Welcome to Samosa Ghar</h1>",
    "text/html"
));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
