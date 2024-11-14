
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;
using System;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using MongoDB.Bson;

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
                // Set the order status to 'Pending' when the order is placed
                order.Status = "Pending";

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

        [HttpGet("neworders")]
        public async Task<IActionResult> GetNewOrders()
        {
            try
            {
                // Filter orders by status 'New'
                var filter = Builders<Order>.Filter.Eq(o => o.Status, "Pending");

                // Get all orders with status 'New'
                var orders = await _orderCollection.Find(filter).ToListAsync();

                // Check if any orders were found
                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { message = "No new orders found." });
                }

                // Return the list of new orders
                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                Console.WriteLine($"Error retrieving new orders: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPatch("process/{orderId}")]
        public async Task<IActionResult> MarkOrderAsProcessed(string orderId)
        {
            try
            {
                // Validate the orderId format
                if (string.IsNullOrEmpty(orderId) || orderId.Length != 5) // Example validation for 5-character orderId
                {
                    return BadRequest(new { message = "Invalid order ID format. The order ID should be 5 characters long." });
                }

                // Build the filter to find the order by its orderId
                var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);

                // Find the order to ensure it exists
                var existingOrder = await _orderCollection.Find(filter).FirstOrDefaultAsync();
                if (existingOrder == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                // Check if the order is already processed/approved
                if (existingOrder.Status == "Accept")
                {
                    return BadRequest(new { message = "Order is already Accept." });
                }

                // Update the order status to 'Accept'
                var update = Builders<Order>.Update.Set(o => o.Status, "Accept");

                // Execute the update in MongoDB
                var result = await _orderCollection.UpdateOneAsync(filter, update);

                // If no document was modified, it means the order wasn't found or already updated
                if (result.ModifiedCount == 0)
                {
                    return NotFound(new { message = "Order not found or already Accept." });
                }

                // Send email notification to the customer about the order acceptance
                SendOrderAcceptedEmail(existingOrder);

                return Ok(new { message = "Order marked as Accept successfully." });
            }
            catch (Exception ex)
            {
                // Log and return the exception message
                Console.WriteLine($"Error processing order: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Send acceptance email to the customer
        private void SendOrderAcceptedEmail(Order order)
        {
            using var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Email"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            // Generate the HTML for each item in the order
            var itemDetails = string.Join("\n", order.Items.Select(item =>
                $@"<div>
            <img src='{item.ImageUrl}' alt='{item.Name}' width='100' />
            <p><strong>Item:</strong> {item.Name}</p>
            <p><strong>Quantity:</strong> {item.Quantity}</p>
            <p><strong>Total Price:</strong> ₹{(item.Price * item.Quantity):F2}</p>
          </div>"
            ));

            // Construct the complete email body
            var emailBody = $@"
        <h2>Hello {order.Address.FullName},</h2>
        <p>Your order has been accepted! Below are your order details:</p>
        {itemDetails}
        <p><strong>Total Amount:</strong> ₹{order.TotalAmount}</p>
        <p>Thank you for choosing Samosa Ghar!<br>Best Regards,<br>Samosa Ghar Team</p>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Email"]),
                Subject = "Order Accepted - Samosa Ghar",
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
                Console.WriteLine($"Failed to send acceptance email to customer: {ex.Message}");
            }
        }

        [HttpPatch("reject/{orderId}")]
        public async Task<IActionResult> RejectOrder(string orderId)
        {
            try
            {
                // Validate the orderId format
                if (string.IsNullOrEmpty(orderId) || orderId.Length != 5) // Example validation for 5-character orderId
                {
                    return BadRequest(new { message = "Invalid order ID format. The order ID should be 5 characters long." });
                }

                // Build the filter to find the order by its orderId
                var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);

                // Find the order to ensure it exists
                var existingOrder = await _orderCollection.Find(filter).FirstOrDefaultAsync();
                if (existingOrder == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                // Check if the order is already rejected
                if (existingOrder.Status == "Reject")
                {
                    return BadRequest(new { message = "Order is already Rejected." });
                }

                // Update the order status to 'Reject'
                var update = Builders<Order>.Update.Set(o => o.Status, "Reject");

                // Execute the update in MongoDB
                var result = await _orderCollection.UpdateOneAsync(filter, update);

                // If no document was modified, it means the order wasn't found or already updated
                if (result.ModifiedCount == 0)
                {
                    return NotFound(new { message = "Order not found or already Rejected." });
                }

                // Send rejection email to the customer
                SendOrderRejectionEmail(existingOrder);

                return Ok(new { message = "Order marked as Rejected successfully." });
            }
            catch (Exception ex)
            {
                // Log and return the exception message
                Console.WriteLine($"Error processing order: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Send rejection email to the customer
        private void SendOrderRejectionEmail(Order order)
        {
            using var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Email"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            // Generate the HTML for each item in the order
            var itemDetails = string.Join("\n", order.Items.Select(item =>
                $@"<div>
            <img src='{item.ImageUrl}' alt='{item.Name}' width='100' />
            <p><strong>Item:</strong> {item.Name}</p>
            <p><strong>Quantity:</strong> {item.Quantity}</p>
            <p><strong>Total Price:</strong> ₹{(item.Price * item.Quantity):F2}</p>
          </div>"
            ));

            // Construct the complete email body
            var emailBody = $@"
        <h2>Hello {order.Address.FullName},</h2>
        <p>We regret to inform you that your order has been rejected. Below are your order details:</p>
        {itemDetails}
        <p><strong>Total Amount:</strong> ₹{order.TotalAmount}</p>
        <p>If you have any questions, please contact our support.<br>Best Regards,<br>Samosa Ghar Team</p>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Email"]),
                Subject = "Order Rejected - Samosa Ghar",
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
                Console.WriteLine($"Failed to send rejection email to customer: {ex.Message}");
            }
        }

        [HttpGet("acceptorder")]
        public async Task<IActionResult> GetAcceptOrder()
        {
            try
            {
                
                var filter = Builders<Order>.Filter.Eq(o => o.Status, "Accept");

                var orders = await _orderCollection.Find(filter).ToListAsync();

                
                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { message = "No Accept order found." });
                }

               
                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                Console.WriteLine($"Error retrieving new orders: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("rejectorder")]
        public async Task<IActionResult> GetRejectOrder()
        {
            try
            {

                var filter = Builders<Order>.Filter.Eq(o => o.Status, "Reject");

                var orders = await _orderCollection.Find(filter).ToListAsync();


                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { message = "No Reject order found." });
                }


                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                Console.WriteLine($"Error retrieving new orders: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPatch("placed/{orderId}")]
        public async Task<IActionResult> MarkPlaced(string orderId)
        {
            try
            {
                // Validate the orderId format
                if (string.IsNullOrEmpty(orderId) || orderId.Length != 5) // Example validation for 5-character orderId
                {
                    return BadRequest(new { message = "Invalid order ID format. The order ID should be 5 characters long." });
                }

                // Build the filter to find the order by its orderId
                var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);

                // Find the order to ensure it exists
                var existingOrder = await _orderCollection.Find(filter).FirstOrDefaultAsync();
                if (existingOrder == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                // Check if the order is already placed
                if (existingOrder.Status == "Placed")
                {
                    return BadRequest(new { message = "Order is already Placed." });
                }

                // Update the order status to 'Placed'
                var update = Builders<Order>.Update.Set(o => o.Status, "Placed");

                // Execute the update in MongoDB
                var result = await _orderCollection.UpdateOneAsync(filter, update);

                // If no document was modified, it means the order wasn't found or already updated
                if (result.ModifiedCount == 0)
                {
                    return NotFound(new { message = "Order not found or already Placed." });
                }

                // Send placed order email to the customer
                SendOrderPlacedEmail(existingOrder);

                return Ok(new { message = "Order marked as Placed successfully." });
            }
            catch (Exception ex)
            {
                // Log and return the exception message
                Console.WriteLine($"Error processing order: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Send placed order email to the customer
        private void SendOrderPlacedEmail(Order order)
        {
            using var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Email"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            // Generate the HTML for each item in the order
            var itemDetails = string.Join("\n", order.Items.Select(item =>
                $@"<div>
            <img src='{item.ImageUrl}' alt='{item.Name}' width='100' />
            <p><strong>Item:</strong> {item.Name}</p>
            <p><strong>Quantity:</strong> {item.Quantity}</p>
            <p><strong>Total Price:</strong> ₹{(item.Price * item.Quantity):F2}</p>
          </div>"
            ));

            // Construct the complete email body
            var emailBody = $@"
        <h2>Hello {order.Address.FullName},</h2>
        <p>Your order has been placed successfully! Below are your order details:</p>
        {itemDetails}
        <p><strong>Total Amount:</strong> ₹{order.TotalAmount}</p>
        <p>Thank you for choosing Samosa Ghar!<br>Best Regards,<br>Samosa Ghar Team</p>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Email"]),
                Subject = "Order Placed - Samosa Ghar",
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
                Console.WriteLine($"Failed to send placed order email to customer: {ex.Message}");
            }
        }

    }
}
