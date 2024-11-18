

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
        var existingUser = _users.Find(u => u.MobileNumber == newUser.MobileNumber && u.Email==newUser.Email).FirstOrDefault();
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
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            if (string.IsNullOrEmpty(resetPasswordRequest.NewPassword) ||
                (string.IsNullOrEmpty(resetPasswordRequest.Email) && string.IsNullOrEmpty(resetPasswordRequest.MobileNumber)))
            {
                return BadRequest(new { message = "New Password is required, and either Email or Mobile Number must be provided." });
            }

            // Build query filter for email or mobile number
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(u => u.Email, resetPasswordRequest.Email),
                Builders<User>.Filter.Eq(u => u.MobileNumber, resetPasswordRequest.MobileNumber)
            );

            // Find user
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound(new { message = "User not found with the provided Email or Mobile Number." });
            }

            // Update the password in the database (plain text)
            var update = Builders<User>.Update.Set(u => u.Password, resetPasswordRequest.NewPassword);
            await _users.UpdateOneAsync(filter, update);

            // Send email after password reset
            try
            {
                SendPasswordResetEmail(user.Email, user.Name, user.MobileNumber);
            }
            catch (Exception ex)
            {
                // Handle email sending failure but still return success for password reset
                return Ok(new { message = "Password reset successful, but failed to send email notification.", error = ex.Message });
            }

            return Ok(new { message = "Password reset successful. Please check your email for confirmation." });
        }


        // Email function for password reset
        private void SendPasswordResetEmail(string userEmail, string userName, string MobileNumber)
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.Port,
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email),
                Subject = "Password Reset Confirmation - Samosa Ghar",
                Body = $"Hello {userName},\n\nYour password for the mobile number {MobileNumber} has been successfully reset.\n\n" +
                       "If you did not request this change, please contact our support team immediately.\n\n" +
                       "Best Regards,\nSamosa Ghar Team",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(userEmail);

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
