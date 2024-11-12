
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
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;


namespace SamosaGhar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly EmailSettings _emailSettings;

        public UserController(MongoDBConfig dbConfig ,IOptions<EmailSettings> emailSettings)
        {
            _users = dbConfig.GetCollection<User>("User");
            _emailSettings = emailSettings.Value;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginUser)
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

        //[HttpPost("register")]
        //public IActionResult Register([FromBody] User newUser)
        //{
        //    var existingUser = _users.Find(u => u.MobileNumber == newUser.MobileNumber).FirstOrDefault();
        //    if (existingUser != null)
        //    {
        //        return Conflict(new { message = "User already exists" });
        //    }

        //    _users.InsertOne(newUser);
        //    return Ok(new { message = "User registered successfully" });
        //}
        


[HttpPost("register")]
    public IActionResult Register([FromBody] User newUser)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(newUser.Name) ||
            string.IsNullOrEmpty(newUser.MobileNumber) ||
            string.IsNullOrEmpty(newUser.Email) ||
            string.IsNullOrEmpty(newUser.Password))
        {
            return BadRequest(new { message = "All fields (Name, Phone, Email, Password) are required." });
        }

        // Check if user already exists
        var existingUser = _users.Find(u => u.MobileNumber == newUser.MobileNumber).FirstOrDefault();
        if (existingUser != null)
        {
            return Conflict(new { message = "User already exists" });
        }

        // Insert new user into the database
        _users.InsertOne(newUser);

            //Send email to the newly registered user
            try
            {
                SendRegistrationEmail(newUser.Email, newUser.Name, newUser.Password, newUser.MobileNumber);
            }
            catch (Exception ex)
            {
                // Handle email sending failure, but still return success for user registration
                return Ok(new { message = "User registered successfully, but failed to send email.", error = ex.Message });
            }

            //Return success response
            return Ok(new { message = "User registered successfully and check email ." });
           // return Ok(new { message = "User registered successfully ." });
        }

        private void SendRegistrationEmail(string userEmail, string userName, string userPassword, string userMobileNumber)
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.Port,
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
                EnableSsl = true,
            };

            // Email content
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email),
                Subject = "Registration Successful - Samosa Ghar",
                Body = $"Hello {userName},\n\nThank you for registering at Samosa Ghar!\n" +
                       $"Your login details are as follows:\n\n" +
                       $"Mobile Number: {userMobileNumber}\n" +
                       $"Password: {userPassword}\n\n" +
                       "We recommend changing your password after your first login.\n\n" +
                       "Best Regards,\nSamosa Ghar Team",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(userEmail);

            // Send email with error handling
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new Exception("Failed to send email.", ex);
            }
        }

    }
}
