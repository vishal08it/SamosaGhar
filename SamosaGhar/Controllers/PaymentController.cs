using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using SamosaGhar.Models;
using System;
using System.Collections.Generic;

namespace SamosaGhar.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly RazorpayClient _client;

        public PaymentController()
        {
            _client = new RazorpayClient("YOUR_RAZORPAY_KEY", "YOUR_RAZORPAY_SECRET");
        }
        [HttpPost("createorder")]
        public IActionResult CreateOrder([FromBody] PaymentRequest paymentRequest)
        {
            if (paymentRequest == null || paymentRequest.Amount <= 0)
            {
                return BadRequest("Invalid payment request.");
            }

            try
            {
                var options = new Dictionary<string, object>
                {
                    { "amount", paymentRequest.Amount * 100 }, 
                    { "currency", "INR" },
                    { "payment_capture", 1 }
                };

                var order = _client.Order.Create(options);

                // Accessing properties using dictionary syntax
                return Ok(new
                {
                    OrderId = order["id"],
                    Amount = order["amount"],
                    Currency = order["currency"]
                });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }
    }
    
}
