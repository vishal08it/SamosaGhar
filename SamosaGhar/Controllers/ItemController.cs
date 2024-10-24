//using Microsoft.AspNetCore.Mvc;
//using MongoDB.Driver;
//using SamosaGhar.Config;
//using SamosaGhar.Models;
//using System.Collections.Generic;

//namespace SamosaGhar.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ItemController : ControllerBase
//    {
//        private readonly IMongoCollection<Item> _items;

//        public ItemController(MongoDBConfig dbConfig)
//        {
//            _items = dbConfig.GetCollection<Item>("Items");
//        }

//        [HttpGet("list")]
//        public IActionResult GetItems()
//        {
//            var items = _items.Find(_ => true).ToList();
//            return Ok(items);
//        }

//        [HttpPost("add")]
//        public IActionResult AddItem([FromBody] Item newItem)
//        {
//            if (newItem == null)
//            {
//                return BadRequest("Item is null.");
//            }


//            _items.InsertOne(newItem);
//            return Ok(new { message = "Item added successfully", itemId = newItem.Id });
//        }

//    }
//}
//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using MongoDB.Driver;
//using Razorpay.Api;  // Razorpay API namespace
//using SamosaGhar.Config;
//using SamosaGhar.Models;
//using System.Threading.Tasks;

//namespace SamosaGhar.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ItemController : ControllerBase
//    {
//        private readonly IMongoCollection<SamosaGhar.Models.Item> _items;  
//        private readonly Cloudinary _cloudinary;

//        public ItemController(MongoDBConfig dbConfig, IConfiguration config)
//        {
//            _items = dbConfig.GetCollection<SamosaGhar.Models.Item>("Items");  

//            // Initialize Cloudinary using values from appsettings.json
//            CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(  
//                config.GetSection("Cloudinary")["CloudName"],
//                config.GetSection("Cloudinary")["ApiKey"],
//                config.GetSection("Cloudinary")["ApiSecret"]
//            );
//            _cloudinary = new Cloudinary(account);
//        }

//        // GET: api/item/list
//        [HttpGet("list")]
//        public IActionResult GetItems()
//        {
//            var items = _items.Find(_ => true).ToList();
//            return Ok(items);
//        }

//        // POST: api/item/add
//        [HttpPost("add")]
//        public async Task<IActionResult> AddItem([FromForm] IFormFile image, [FromForm] string name, [FromForm] decimal price)
//        {
//            if (image == null || image.Length == 0)
//            {
//                return BadRequest("Image is null or empty.");
//            }

//            // Upload image to Cloudinary
//            var uploadParams = new ImageUploadParams()
//            {
//                File = new FileDescription(image.FileName, image.OpenReadStream())
//            };

//            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

//            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
//            {
//                return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
//            }

//            // Create a new item with the image URL
//            var newItem = new SamosaGhar.Models.Item  // Fully qualify the namespace for Item
//            {
//                Name = name,
//                Price = price,
//                ImageUrl = uploadResult.SecureUrl.ToString() // Store Cloudinary image URL
//            };

//            // Insert the item into MongoDB
//            _items.InsertOne(newItem);
//            return Ok(new { message = "Item added successfully", itemId = newItem.Id });
//        }

//        // DELETE: api/item/{id}
//        [HttpDelete("{id}")]
//        public IActionResult DeleteItem(string id)
//        {
//            var deleteResult = _items.DeleteOne(i => i.Id == id);
//            if (deleteResult.DeletedCount == 0)
//            {
//                return NotFound("Item not found.");
//            }

//            return Ok("Item deleted successfully.");
//        }
//    }
//}
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

            // Initialize Cloudinary using values from appsettings.json
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var account = new Account(
                config[$"Cloudinary:{environment}:CloudName"],
                config[$"Cloudinary:{environment}:ApiKey"],
                config[$"Cloudinary:{environment}:ApiSecret"]
            );
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
