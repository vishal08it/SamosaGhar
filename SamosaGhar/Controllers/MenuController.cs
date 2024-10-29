using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SamosaGhar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMongoCollection<MenuItem> _menuItems;
        private readonly Cloudinary _cloudinary;

        public MenuController(MongoDBConfig dbConfig, Cloudinary cloudinary)
        {
            _menuItems = dbConfig.GetCollection<MenuItem>("Menu");
            _cloudinary = cloudinary;
        }

        // POST: api/menu/add
        [HttpPost("add")]
        public async Task<IActionResult> AddMenuItem([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Image is null or empty.");
            }

            // Upload image to Cloudinary
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, image.OpenReadStream())
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
            }

            // Create a new menu item with the image URL
            var newItem = new MenuItem
            {
                Image = uploadResult.SecureUrl.ToString() // Store Cloudinary image URL
            };

            // Insert the menu item into the "menu" collection
            _menuItems.InsertOne(newItem);
            return Ok(new { message = "Menu item added successfully", itemId = newItem.Id });
        }

        // GET: api/menu
        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            var items = await _menuItems.Find(_ => true).ToListAsync();
            return Ok(items);
        }

        // GET: api/menu/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuItemById(string id)
        {
            var item = await _menuItems.Find(i => i.Id == id).FirstOrDefaultAsync();
            if (item == null)
            {
                return NotFound("Menu item not found.");
            }
            return Ok(item);
        }

        // DELETE: api/menu/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteMenuItem(string id)
        {
            var deleteResult = _menuItems.DeleteOne(i => i.Id == id);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound("Menu item not found.");
            }

            return Ok("Menu item deleted successfully.");
        }
    }
}
