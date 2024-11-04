using Microsoft.AspNetCore.Mvc;
using SamosaGhar.Config;
using SamosaGhar.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using System;
using MongoDB.Bson;

namespace SamosaGhar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly MongoDBConfig _mongoDBConfig;

        public CartController(MongoDBConfig mongoDBConfig)
        {
            _mongoDBConfig = mongoDBConfig;
        }

        [HttpPost("addtocart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.MobileNumber) ||
                string.IsNullOrEmpty(request.ItemId) ||
                request.Quantity <= 0)
            {
                return BadRequest("Invalid input. Mobile number, item ID, and quantity must be provided.");
            }

            try
            {
                var userCollection = _mongoDBConfig.GetCollection<User>("User");
                var itemCollection = _mongoDBConfig.GetCollection<Item>("Items");
                var cartCollection = _mongoDBConfig.GetCollection<CartItem>("CartItems");

                var user = await userCollection.Find(u => u.MobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var item = await itemCollection.Find(i => i.Id == request.ItemId).FirstOrDefaultAsync();
                if (item == null)
                {
                    return NotFound("Item not found.");
                }

                var totalPrice = item.Price * request.Quantity; // Calculate total price

                var filter = Builders<CartItem>.Filter.Where(c => c.UserId == request.MobileNumber && c.ItemId == request.ItemId);
                var update = Builders<CartItem>.Update
                    .Set(c => c.ItemName, item.Name)
                    .Set(c => c.Price, item.Price)
                    .Set(c => c.ImageUrl, item.ImageUrl)
                    .Inc(c => c.Quantity, request.Quantity)
                    .Set(c => c.TotalPrice, totalPrice) // Set total price
                    .SetOnInsert(c => c.AddedDate, DateTime.UtcNow);

                var options = new UpdateOptions { IsUpsert = true };

                await cartCollection.UpdateOneAsync(filter, update, options);

                return Ok(new { message = "Item added to cart successfully", itemId = request.ItemId, quantity = request.Quantity });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpGet("countitems/{mobileNumber}")]
        public async Task<IActionResult> CountItemsInCart(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber))
            {
                return BadRequest("Mobile number must be provided.");
            }

            try
            {
                var cartCollection = _mongoDBConfig.GetCollection<CartItem>("CartItems");

                // Count total items for the specified user
                var cartItems = await cartCollection
                    .Find(c => c.UserId.Equals(mobileNumber, StringComparison.OrdinalIgnoreCase))
                    .ToListAsync();

                int totalItems = cartItems.Sum(c => c.Quantity); // Sum the quantities

                return Ok(new { totalItems });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting items: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("getcartitems/{mobileNumber}")]
        public async Task<IActionResult> GetCartItems(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber))
            {
                return BadRequest("Mobile number must be provided.");
            }

            try
            {
                var cartCollection = _mongoDBConfig.GetCollection<CartItem>("CartItems");

                var cartItems = await cartCollection
                    .Find(c => c.UserId == mobileNumber)
                    .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                {
                    return NotFound("No items found in the cart.");
                }

                var response = cartItems.Select(c => new CartItemResponse
                {
                    Id = c.Id.ToString(),  // This now holds the MongoDB document ID (_id)
                    ItemName = c.ItemName,
                    ImageUrl = c.ImageUrl,
                    Price = c.Price,
                    Quantity = c.Quantity,
                    TotalPrice = c.TotalPrice // Ensure this property is available in your CartItem class
                }).ToList();

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpDelete("removefromcart/{id}/{mobileNumber}")]
        public async Task<IActionResult> RemoveFromCart(string id, string mobileNumber)
        {
            // Validate input
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(mobileNumber))
            {
                return BadRequest("Item ID and mobile number must be provided.");
            }

            try
            {
                var cartCollection = _mongoDBConfig.GetCollection<CartItem>("CartItems");

                // Try to convert the string ID to ObjectId
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    return BadRequest("Invalid item ID format.");
                }

                // Log the ID and mobile number
                Console.WriteLine($"Attempting to remove item with ID: {objectId} for user with mobile number: {mobileNumber}");

                // Create filter for the item to delete
                var filter = Builders<CartItem>.Filter.And(
                    Builders<CartItem>.Filter.Eq(c => c.Id, objectId),
                    Builders<CartItem>.Filter.Eq(c => c.UserId, mobileNumber)
                );

                // Perform the deletion
                var result = await cartCollection.DeleteOneAsync(filter);

                // Log the result of the deletion attempt
                Console.WriteLine($"Deletion result: {result.DeletedCount} items deleted.");

                if (result.DeletedCount == 0)
                {
                    return NotFound("Item not found in cart.");
                }

                return Ok(new { message = "Item removed from cart successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }





    }
}
