using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;

namespace SamosaGhar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;

        // Use constructor injection to get MongoDBConfig
        public UserController(MongoDBConfig dbConfig)
        {
            _users = dbConfig.GetCollection<User>("Users");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            var user = _users.Find(u => u.MobileNumber == loginUser.MobileNumber && u.Password == loginUser.Password).FirstOrDefault();

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid mobile number or password" });
            }

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User newUser)
        {
            var existingUser = _users.Find(u => u.MobileNumber == newUser.MobileNumber).FirstOrDefault();
            if (existingUser != null)
            {
                return Conflict(new { message = "User already exists" });
            }

            _users.InsertOne(newUser);
            return Ok(new { message = "User registered successfully" });
        }
    }
}
