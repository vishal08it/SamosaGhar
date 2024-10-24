    //using Microsoft.AspNetCore.Mvc;
    //using Razorpay.Api;
    //using SamosaGhar.Models;
    //using System;
    //using System.Collections.Generic;

    //namespace SamosaGhar.Controllers
    //{
    //    [Route("api/[controller]")]
    //    [ApiController]
    //    public class PaymentController : Controller
    //    {
    //        private readonly RazorpayClient _client;

    //        public PaymentController()
    //        {
    //            _client = new RazorpayClient("YOUR_RAZORPAY_KEY", "YOUR_RAZORPAY_SECRET");
    //        }

    //        [HttpPost("createorder")]
    //        public IActionResult CreateOrder([FromBody] PaymentRequest paymentRequest)
    //        {
    //            if (paymentRequest == null || paymentRequest.Amount <= 0)
    //            {
    //                return BadRequest("Invalid payment request.");
    //            }

    //            try
    //            {
    //                var options = new Dictionary<string, object>
    //                {
    //                    { "amount", paymentRequest.Amount * 100 },  // Convert Rupees to Paise (Razorpay accepts in paise)
    //                    { "currency", "INR" },
    //                    { "payment_capture", 1 }
    //                };

    //                // Create the order using Razorpay API
    //                var razorpayOrder = _client.Order.Create(options);

    //                // Map the Razorpay order to your custom Order class
    //                var order = new Models.Order
    //                {
    //                    OrderId = razorpayOrder["id"].ToString(),
    //                    Amount = (int)(paymentRequest.Amount),  // Keep it in Rupees for your response
    //                    Currency = razorpayOrder["currency"].ToString(),
    //                    Status = razorpayOrder["status"].ToString()
    //                };

    //                return Ok(order);
    //            }
    //            catch (Exception ex)
    //            {
    //                // Log the exception (consider using a logging framework)
    //                return StatusCode(500, "An error occurred while creating the order: " + ex.Message);
    //            }
    //        }
    //    }
    //}
    //using Microsoft.AspNetCore.Mvc;
    //using Razorpay.Api;
    //using SamosaGhar.Models;
    //using System;
    //using System.Collections.Generic;

    //namespace SamosaGhar.Controllers
    //{
    //    [Route("api/[controller]")]
    //    [ApiController]
    //    public class PaymentController : Controller
    //    {
    //        private readonly RazorpayClient _client;

    //        public PaymentController()
    //        {
    //            _client = new RazorpayClient("YOUR_RAZORPAY_KEY", "YOUR_RAZORPAY_SECRET");
    //        }

    //        [HttpPost("createorder")]
    //        public IActionResult CreateOrder([FromBody] PaymentRequest paymentRequest)
    //        {
    //            if (paymentRequest == null || paymentRequest.Amount <= 0)
    //            {
    //                return BadRequest("Invalid payment request.");
    //            }

    //            try
    //            {
    //                var options = new Dictionary<string, object>
    //                {
    //                    { "amount", paymentRequest.Amount * 100 },  // Convert Rupees to Paise (Razorpay accepts in paise)
    //                    { "currency", "INR" },
    //                    { "payment_capture", 1 }
    //                };

    //                var razorpayOrder = _client.Order.Create(options);

    //                var order = new Order
    //                {
    //                    OrderId = razorpayOrder["id"].ToString(),
    //                    Amount = (int)(paymentRequest.Amount),  // Keep it in Rupees for your response
    //                    Currency = razorpayOrder["currency"].ToString(),
    //                    Status = razorpayOrder["status"].ToString()
    //                };

    //                return Ok(order);
    //            }
    //            catch (Exception ex)
    //            {
    //                return StatusCode(500, "An error occurred while creating the order: " + ex.Message);
    //            }
    //        }
    //    }
    //}
