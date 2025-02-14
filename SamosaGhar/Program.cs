
//using CloudinaryDotNet;
//using SamosaGhar.Config;
//using SamosaGhar.Models;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container
//builder.Services.AddControllers();

//// Add EmailSettings configuration from appsettings.json
//builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

//// Register other services like MongoDB and Cloudinary as usual
//var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
//var cloudinaryAccount = new Account(
//    cloudinaryConfig["CloudName"],
//    cloudinaryConfig["ApiKey"],
//    cloudinaryConfig["ApiSecret"]
//);
//builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

//// Additional services and configurations
//builder.Services.AddSingleton<MongoDBConfig>(sp =>
//{
//    var configSection = builder.Configuration.GetSection("MongoDB");
//    string connectionString = configSection["ConnectionString"];
//    string databaseName = configSection["DatabaseName"];
//    return new MongoDBConfig(connectionString, databaseName);
//});

//// Configure CORS, etc.
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp", policy =>
//    {
//        policy.WithOrigins("http://localhost:3000", "https://samosagharreact.vercel.app")
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

//// Set the application to listen on port 7200
//builder.WebHost.UseUrls("http://localhost:7200");

//var app = builder.Build();

//// Enable CORS
//app.UseCors("AllowReactApp");

//// Configure the HTTP request pipeline
//app.UseHttpsRedirection();
//app.UseAuthorization();

//app.MapControllers();

//// Welcome message at root URL
//app.MapGet("/", () => Results.Content(
//    "<h1 style='text-align:center;color:green;'>Welcome to Samosa Ghar</h1>",
//    "text/html"
//));

//app.Run();
using CloudinaryDotNet;
using Microsoft.Extensions.Logging;
using SamosaGhar.Config;
using SamosaGhar.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Load EmailSettings from appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure Cloudinary
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
var cloudinaryAccount = new Account(
    cloudinaryConfig["CloudName"],
    cloudinaryConfig["ApiKey"],
    cloudinaryConfig["ApiSecret"]
);
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

// Configure MongoDB
builder.Services.AddSingleton<MongoDBConfig>(sp =>
{
    var configSection = builder.Configuration.GetSection("MongoDB");
    string connectionString = configSection["ConnectionString"];
    string databaseName = configSection["DatabaseName"];
    return new MongoDBConfig(connectionString, databaseName);
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "https://samosagharreact.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Enable logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Force HTTPS in hosted environments
builder.WebHost.UseUrls("http://localhost:7200"); // Change port as needed

var app = builder.Build();

// Middleware pipeline
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseStaticFiles();      // Serve static files if needed
app.UseCors("AllowAllOrigins");
app.UseAuthorization();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

// Map endpoints
app.MapControllers();

// Root welcome message
app.MapGet("/", () => Results.Content(
    "<h1 style='text-align:center;color:green;'>Welcome to Samosa Ghar</h1>",
    "text/html"
));

app.Run();
