using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SamosaGhar.Config;
using SamosaGhar.Models;

namespace SamosaGhar.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly IMongoCollection<CartItem> _cartItems;

        
        public CartController(MongoDBConfig dbConfig)
        {
            _cartItems = dbConfig.GetCollection<CartItem>("CartItems");
        }

        [HttpPost("add")]
        public IActionResult AddToCart([FromBody] CartItem cartItem)
        {
            
            cartItem.Id = Guid.NewGuid().ToString(); 

            _cartItems.InsertOne(cartItem);
            return Ok(new { message = "Item added to cart" });
        }

        [HttpGet("get")]
        public IActionResult GetCartItems()
        {
            var cartItems = _cartItems.Find(_ => true).ToList();
            return Ok(cartItems);
        }

        [HttpDelete("clear")]
        public IActionResult ClearCart()
        {
            _cartItems.DeleteMany(_ => true);
            return Ok(new { message = "Cart cleared" });
        }

    }
}

