using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;
using System;
using System.Threading.Tasks;

namespace SamosaGhar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationsController(MongoDBConfig mongoDBConfig)
        {
            // Make sure to properly initialize the _notifications collection
            //ar database = dbConfig.GetDatabase(); // Assuming GetDatabase() returns the actual MongoDB database instance
            _notifications = mongoDBConfig.GetCollection<Notification>("Notifications"); // Ensure this collection is assigned
        }

        // 1. Create Notification for a New Order
        [HttpPost("create")]
        public async Task<IActionResult> CreateNotification([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return BadRequest(new { message = "Notification data is required" });
            }

            if (string.IsNullOrEmpty(notification.UserId) || string.IsNullOrEmpty(notification.Message))
            {
                return BadRequest(new { message = "UserId and Message are required" });
            }

            // Initialize other properties if necessary
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;  // Default value for IsRead if not provided

            try
            {
                // Ensure _notifications is not null and has been initialized
                if (_notifications == null)
                {
                    throw new NullReferenceException("Notifications collection is not initialized.");
                }

                // Save notification to MongoDB - MongoDB will generate the _id
                await _notifications.InsertOneAsync(notification);

                return Ok(new { message = "Notification created successfully", notification });
            }
            catch (NullReferenceException ex)
            {
                // Catch specific NullReferenceException and log or handle it
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                // Catch other exceptions
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }



        // 2. Get Unread Notifications Count
        [HttpGet("count")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            var unreadCount = await _notifications.CountDocumentsAsync(n => !n.IsRead);
            return Ok(new { unreadCount });
        }

        // 3. Get All Notifications (Admin can view all notifications)
        //[HttpGet]
        //public async Task<IActionResult> GetAllNotifications()
        //{
        //    var notifications = await _notifications.Find(_ => true).ToListAsync();
        //    return Ok(notifications);
        //}

        //// 4. Mark Notification as Read
        //[HttpPut("read/{id}")]
        //public async Task<IActionResult> MarkNotificationAsRead(string id)
        //{
        //    //var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
        //    var update = Builders<Notification>.Update.Set(n => n.IsRead, true);

        //    var result = await _notifications.UpdateOneAsync(filter, update);

        //    if (result.ModifiedCount == 0)
        //    {
        //        return NotFound(new { message = "Notification not found" });
        //    }

        //    return Ok(new { message = "Notification marked as read" });
        //}
    }
}
