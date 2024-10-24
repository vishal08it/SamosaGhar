
//using Microsoft.AspNetCore.Mvc;
//using MongoDB.Driver;
//using SamosaGhar.Config;
//using SamosaGhar.Models;

//namespace SamosaGhar.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UserController : ControllerBase
//    {
//        private readonly IMongoCollection<User> _users;

//        public UserController(MongoDBConfig dbConfig)
//        {
//            _users = dbConfig.GetCollection<User>("User"); 
//        }

//        //[HttpPost("login")]
//        //public IActionResult Login([FromBody] User loginUser)
//        //{
//        //    var user = _users.Find(u => u.MobileNumber == loginUser.MobileNumber && u.Password == loginUser.Password).FirstOrDefault();

//        //    if (user == null)
//        //    {
//        //        return Unauthorized(new { message = "Invalid mobile number or password" });
//        //    }

//        //    return Ok(new { message = "Login successful" });
//        //}
//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] User loginUser)
//        {
//            // Validate if the mobile number and password are provided
//            if (string.IsNullOrEmpty(loginUser.MobileNumber) || string.IsNullOrEmpty(loginUser.Password))
//            {
//                return BadRequest(new { message = "Mobile number and password are required" });
//            }

//            // Find the user by mobile number and password using MongoDB's Find method
//            var user = await _users.Find(u => u.MobileNumber == loginUser.MobileNumber && u.Password == loginUser.Password).FirstOrDefaultAsync();

//            // If user is not found, return Unauthorized response
//            if (user == null)
//            {
//                return Unauthorized(new { message = "Invalid mobile number or password" });
//            }

//            // If user is found, return a success response
//            return Ok(new { message = "Login successful", user = user });
//        }


//        [HttpPost("register")]
//        public IActionResult Register([FromBody] User newUser)
//        {
//            var existingUser = _users.Find(u => u.MobileNumber == newUser.MobileNumber).FirstOrDefault();
//            if (existingUser != null)
//            {
//                return Conflict(new { message = "User already exists" });
//            }

//            _users.InsertOne(newUser);
//            return Ok(new { message = "User registered successfully" });
//        }

//    }
//}
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;
using System.Threading.Tasks;

namespace SamosaGhar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;

        public UserController(MongoDBConfig dbConfig)
        {
            _users = dbConfig.GetCollection<User>("User");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            if (string.IsNullOrEmpty(loginUser.MobileNumber) || string.IsNullOrEmpty(loginUser.Password))
            {
                return BadRequest(new { message = "Mobile number and password are required" });
            }

            var user = await _users.Find(u => u.MobileNumber == loginUser.MobileNumber && u.Password == loginUser.Password).FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid mobile number or password" });
            }

            return Ok(new { message = "Login successful", user = user });
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
