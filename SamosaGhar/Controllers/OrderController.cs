//using Microsoft.AspNetCore.Mvc;
//using MongoDB.Driver;
//using SamosaGhar.Config;
//using SamosaGhar.Models;
//using System;
//using System.Threading.Tasks;

//namespace SamosaGhar.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class OrderController : ControllerBase
//    {
//        private readonly IMongoCollection<Order> _orderCollection;

//        public OrderController(MongoDBConfig mongoDBConfig)
//        {
//            _orderCollection = mongoDBConfig.GetCollection<Order>("Orders");
//        }

//        [HttpPost("placeorder")]
//        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
//        {
//            // Ensure that the UserId and Address.UserId are provided
//            if (order == null || string.IsNullOrEmpty(order.UserId) || order.Address == null || string.IsNullOrEmpty(order.Address.UserId))
//            {
//                return BadRequest("Invalid order data. UserId and Address.UserId are required.");
//            }

//            try
//            {
//                // Insert the order into the database
//                await _orderCollection.InsertOneAsync(order);

//                return Ok(new { message = "Order placed successfully!" });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }
//    }

//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;
using System;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace SamosaGhar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<CartItem> _cartCollection;
        private readonly IConfiguration _configuration;
        public OrderController(MongoDBConfig mongoDBConfig, IConfiguration configuration)
        {
            _orderCollection = mongoDBConfig.GetCollection<Order>("Orders");
            _cartCollection = mongoDBConfig.GetCollection<CartItem>("CartItems");
            _configuration = configuration;
        }
        [HttpPost("placeorder")]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            // Validate the required fields for the order
            if (string.IsNullOrEmpty(order.UserId) ||
                order.Address == null || string.IsNullOrEmpty(order.Address.UserId))
            {
                return BadRequest(new { message = "Invalid order data. UserId and Address.UserId are required." });
            }

            try
            {
                // Insert the order into the database
                await _orderCollection.InsertOneAsync(order);

                // After successfully placing the order, remove the user's cart items
                var deleteFilter = Builders<CartItem>.Filter.Eq(c => c.UserId, order.UserId);
                var deleteResult = await _cartCollection.DeleteManyAsync(deleteFilter);

                if (deleteResult.DeletedCount > 0)
                {
                    Console.WriteLine($"Successfully deleted {deleteResult.DeletedCount} cart items for user {order.UserId}");
                }
                else
                {
                    Console.WriteLine("No cart items found to delete.");
                }

                // Send email to the newly placed order's user
                try
                {
                    SendOrderConfirmationEmail(order);
                }
                catch (Exception ex)
                {
                    // Handle email sending failure, but still return success for order placement
                    return Ok(new { message = "Order placed successfully, but failed to send email.", error = ex.Message });
                }

                // Send email to admin about the new order
                try
                {
                    SendAdminOrderNotificationEmail(order);
                }
                catch (Exception ex)
                {
                    // Handle admin email sending failure, but still proceed
                    Console.WriteLine($"Failed to send admin email: {ex.Message}");
                }

                // Return success response
                return Ok(new { message = "Order placed successfully. Check your email for confirmation." });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error placing order: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Send confirmation email to the user
        private void SendOrderConfirmationEmail(Order order)
        {
            using var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Email"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            // Construct the email body
            var itemDetails = string.Join("\n", order.Items.Select(item =>
                $"<div><img src='{item.ImageUrl}' alt='{item.Name}' width='100' />" +
                $"<p><strong>Item:</strong> {item.Name}</p>" +
                $"<p><strong>Quantity:</strong> {item.Quantity}</p>" +
                $"<p><strong>Total Price:</strong> ₹{(item.Price * item.Quantity).ToString("F2")}</p></div>"
            ));

            var emailBody = $@"
<h2>Hello {order.Address.FullName},</h2>
<p>Thank you for your order! Below are your order details:</p>
{itemDetails}
<p><strong>Total Amount:</strong> ₹{order.TotalAmount}</p>
<p>Thank you for choosing Samosa Ghar!<br>Best Regards,<br>Samosa Ghar Team
";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Email"]),
                Subject = "Order Confirmation - Samosa Ghar",
                Body = emailBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(order.Email);

            // Send email with error handling
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new Exception("Failed to send confirmation email.", ex);
            }
        }

        // Send order notification email to admin
        private void SendAdminOrderNotificationEmail(Order order)
        {
            using var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Email"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            // Construct the email body for admin
            var adminItemDetails = string.Join("\n", order.Items.Select(item =>
                $"<div><img src='{item.ImageUrl}' alt='{item.Name}' width='100' />" +
                $"<p><strong>Item:</strong> {item.Name}</p>" +
                $"<p><strong>Quantity:</strong> {item.Quantity}</p>" +
                $"<p><strong>Total Price:</strong> ₹{(item.Price * item.Quantity).ToString("F2")}</p></div>"
            ));

            // Include user address in the email
            var userAddress = order.Address != null
                ? $@"
            <h3>User Address:</h3>
            <p></strong> {order.Address.FullName} {order.Address.Flat} {order.Address.Area} {order.Address.Landmark} {order.Address.City} {order.Address.State} {order.Address.Pincode}</p>
            <p><strong>Mobile:</strong> {order.Address.Mobile}</p>"
            
                : "<p>No address provided.</p>";

            var adminEmailBody = $@"
<h2>New Order Placed</h2>
<p>A new order has been placed by {order.UserId}. Here are the order details:</p>
{adminItemDetails}
<p><strong>Total Amount:</strong> ₹{order.TotalAmount}</p>
{userAddress}
<p>Please process the order at the earliest.<br>Best Regards,<br>Samosa Ghar Team
";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Email"]),
                Subject = "New Order Notification - Samosa Ghar",
                Body = adminEmailBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(_configuration["EmailSettings:Email"]); // Admin's email

            // Send email with error handling
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new Exception("Failed to send admin notification email.", ex);
            }
        }



        // Place Order and Delete Cart Items
        //[HttpPost("placeorder")]
        //public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        //{
        //    // Ensure that the UserId and Address.UserId are provided
        //    if (order == null || string.IsNullOrEmpty(order.UserId) || order.Address == null || string.IsNullOrEmpty(order.Address.UserId))
        //    {
        //        return BadRequest("Invalid order data. UserId and Address.UserId are required.");
        //    }

        //    try
        //    {
        //        // Insert the order into the database
        //        await _orderCollection.InsertOneAsync(order);

        //        // After successfully placing the order, remove the user's cart items
        //        var deleteFilter = Builders<CartItem>.Filter.Eq(c => c.UserId, order.UserId);
        //        var deleteResult = await _cartCollection.DeleteManyAsync(deleteFilter);

        //        if (deleteResult.DeletedCount > 0)
        //        {
        //            Console.WriteLine($"Successfully deleted {deleteResult.DeletedCount} cart items for user {order.UserId}");
        //        }
        //        else
        //        {
        //            Console.WriteLine("No cart items found to delete.");
        //        }

        //        return Ok(new { message = "Order placed successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        Console.WriteLine($"Error placing order: {ex.Message}");
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        // Get Order History for a User
        [HttpGet("orderhistory/{userId}")]
        public async Task<IActionResult> GetOrderHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId (mobile number) is required.");
            }

            try
            {
                // Query orders by UserId (mobile number)
                var filter = Builders<Order>.Filter.Eq(o => o.UserId, userId);
                var orders = await _orderCollection.Find(filter).ToListAsync();

                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { message = "No orders found for the given UserId." });
                }

                return Ok(orders);  // Return the list of orders
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving order history: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
