using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;
using System.Threading.Tasks;

namespace SamosaGhar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IMongoCollection<Item> _items;
        private readonly Cloudinary _cloudinary;

        public ItemController(MongoDBConfig dbConfig, IConfiguration config)
        {
            _items = dbConfig.GetCollection<Item>("Items");

            // Fetch Cloudinary config directly
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            // Check if Cloudinary configuration is valid
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing. Please check your appsettings.json.");
            }

            // Initialize Cloudinary
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        // GET: api/item/list
        [HttpGet("list")]
        public IActionResult GetItems()
        {
            var items = _items.Find(_ => true).ToList();
            return Ok(items);
        }

        // POST: api/item/add
        [HttpPost("add")]
        public async Task<IActionResult> AddItem([FromForm] IFormFile image, [FromForm] string name, [FromForm] decimal price)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Image is null or empty.");
            }

            // Upload image to Cloudinary
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(image.FileName, image.OpenReadStream())
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
            }

            // Create a new item with the image URL
            var newItem = new Item
            {
                Name = name,
                Price = price,
                ImageUrl = uploadResult.SecureUrl.ToString() // Store Cloudinary image URL
            };

            // Insert the item into MongoDB
            _items.InsertOne(newItem);
            return Ok(new { message = "Item added successfully", itemId = newItem.Id });
        }

        // DELETE: api/item/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteItem(string id)
        {
            var deleteResult = _items.DeleteOne(i => i.Id == id);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound("Item not found.");
            }

            return Ok("Item deleted successfully.");
        }
    }
}
