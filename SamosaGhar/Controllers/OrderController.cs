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
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;
using System;
using System.Threading.Tasks;

namespace SamosaGhar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<CartItem> _cartCollection;

        public OrderController(MongoDBConfig mongoDBConfig)
        {
            _orderCollection = mongoDBConfig.GetCollection<Order>("Orders");
            _cartCollection = mongoDBConfig.GetCollection<CartItem>("CartItems");
        }

        // Place Order and Delete Cart Items
        [HttpPost("placeorder")]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            // Ensure that the UserId and Address.UserId are provided
            if (order == null || string.IsNullOrEmpty(order.UserId) || order.Address == null || string.IsNullOrEmpty(order.Address.UserId))
            {
                return BadRequest("Invalid order data. UserId and Address.UserId are required.");
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

                return Ok(new { message = "Order placed successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error placing order: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
