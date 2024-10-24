////using SamosaGhar.Config;

////var builder = WebApplication.CreateBuilder(args);

////// Add appsettings.json
////builder.Configuration
////    .SetBasePath(Directory.GetCurrentDirectory())
////    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
////    .AddEnvironmentVariables();

////// Add services to the container
////builder.Services.AddControllers();

////// Register MongoDBConfig as a singleton service
////builder.Services.AddSingleton(sp =>
////{
////    var configSection = builder.Configuration.GetSection("MongoDB");
////    string connectionString = configSection["ConnectionString"];
////    string databaseName = configSection["DatabaseName"];
////    return new MongoDBConfig(connectionString, databaseName);
////});

////// Add CORS configuration
////builder.Services.AddCors(options =>
////{
////    options.AddPolicy("AllowReactApp",
////        policy =>
////        {
////            policy.WithOrigins("http://localhost:3000")  // Allow requests from the React app
////                  .AllowAnyHeader()                      // Allow any headers
////                  .AllowAnyMethod();                     // Allow any HTTP methods (GET, POST, etc.)
////        });
////});

////// Set the application to listen on port 7200
////builder.WebHost.UseUrls("http://localhost:7200");

////var app = builder.Build();

////// Enable CORS
////app.UseCors("AllowReactApp");

////// Configure the HTTP request pipeline
////app.UseHttpsRedirection();
////app.UseAuthorization();

////app.MapControllers();

////app.MapGet("/", () => Results.Content(
////    "<h1 style='text-align:center;color:green;'>Welcome to Samosa Ghar</h1>",
////    "text/html"
////));

////app.Run();
//using SamosaGhar.Config;

//var builder = WebApplication.CreateBuilder(args);

//// Add appsettings.json configuration
//builder.Configuration
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddEnvironmentVariables();

//// Add services to the container
//builder.Services.AddControllers();

//// Register MongoDBConfig as a singleton service
//builder.Services.AddSingleton(sp =>
//{
//    var configSection = builder.Configuration.GetSection("MongoDB");
//    string connectionString = configSection["ConnectionString"];
//    string databaseName = configSection["DatabaseName"];
//    return new MongoDBConfig(connectionString, databaseName);
//});

//// Add CORS configuration to allow the frontend React app to make API requests
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp",
//        policy =>
//        {
//            policy.WithOrigins("http://localhost:3000")  // Allow requests from the React app
//                  .AllowAnyHeader()                      // Allow any headers
//                  .AllowAnyMethod();                     // Allow any HTTP methods (GET, POST, etc.)
//        });
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

//// Run the app
//app.Run();
//using SamosaGhar.Config;

//var builder = WebApplication.CreateBuilder(args);

//// Add appsettings.json configuration
//builder.Configuration
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddEnvironmentVariables();

//// Add services to the container
//builder.Services.AddControllers();

//// Register MongoDBConfig as a singleton service
//builder.Services.AddSingleton(sp =>
//{
//    var configSection = builder.Configuration.GetSection("MongoDB");
//    string connectionString = configSection["ConnectionString"];
//    string databaseName = configSection["DatabaseName"];
//    return new MongoDBConfig(connectionString, databaseName);
//});

//// CORS configuration
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp",
//        policy =>
//        {
//            // Allow requests based on the environment
//            if (builder.Environment.IsDevelopment())
//            {
//                policy.WithOrigins("http://localhost:3000")  // Local React app
//                      .AllowAnyHeader()
//                      .AllowAnyMethod();
//            }
//            else
//            {
//                policy.WithOrigins("https://samosagharreact.vercel.app/")  // Live React app URL
//                      .AllowAnyHeader()
//                      .AllowAnyMethod();
//            }
//        });
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

//// Run the app
//app.Run();
//public class Startup
//{
//    public IConfiguration Configuration { get; }

//    public Startup(IConfiguration configuration)
//    {
//        Configuration = configuration;
//    }

//    public void ConfigureServices(IServiceCollection services)
//    {
//        services.AddControllers();
//        services.AddSingleton<MongoDBConfig>(); // Register MongoDBConfig as a singleton
//    }

//    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//    {
//        if (env.IsDevelopment())
//        {
//            app.UseDeveloperExceptionPage();
//        }

//        app.UseRouting();
//        app.UseAuthorization();
//        app.UseEndpoints(endpoints =>
//        {
//            endpoints.MapControllers();
//        });
//    }
//}
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllers();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
//else
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseRouting();
//app.UseAuthorization();

//// Define a default route to the Home Controller
//app.MapGet("/", () => Results.Redirect("/home"));

//app.MapControllers(); // Ensure controllers are mapped

//app.Run();
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SamosaGhar.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<MongoDBConfig>(); // Register MongoDBConfig

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Map the root URL to redirect to the home page
app.MapGet("/", () => Results.Redirect("/home"));

app.MapControllers(); // Ensure controllers are mapped

app.Run();
