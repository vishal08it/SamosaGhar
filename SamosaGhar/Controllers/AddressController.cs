using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using SamosaGhar.Config;
using SamosaGhar.Models;

namespace SamosaGhar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IMongoCollection<Address> _addressCollection;

        public AddressController(MongoDBConfig mongoDBConfig)
        {
            _addressCollection = mongoDBConfig.GetCollection<Address>("Addresses");
        }

        // Add a new address
        [HttpPost("add")]
        public async Task<IActionResult> AddAddress([FromBody] Address newAddress)
        {
            if (newAddress == null || string.IsNullOrEmpty(newAddress.UserId))
            {
                return BadRequest("Invalid input. UserId must be provided.");
            }

            try
            {
                await _addressCollection.InsertOneAsync(newAddress);
                // After insertion, newAddress.Id will have the generated Id from MongoDB
                return Ok(new { message = "Address added successfully.", addressId = newAddress.Id.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("updateByUserId")]
        public async Task<IActionResult> UpdateAddressByUserId([FromBody] Address updatedAddress)
        {
            if (updatedAddress == null || string.IsNullOrEmpty(updatedAddress.UserId))
            {
                return BadRequest("Updated address and UserId must be provided.");
            }

            var filter = Builders<Address>.Filter.Eq(a => a.UserId, updatedAddress.UserId);
            var update = Builders<Address>.Update
                .Set(a => a.FullName, updatedAddress.FullName)
                .Set(a => a.Mobile, updatedAddress.Mobile)
                .Set(a => a.Pincode, updatedAddress.Pincode)
                .Set(a => a.Flat, updatedAddress.Flat)
                .Set(a => a.Area, updatedAddress.Area)
                .Set(a => a.Landmark, updatedAddress.Landmark)
                .Set(a => a.City, updatedAddress.City)
                .Set(a => a.State, updatedAddress.State);

            try
            {
                var result = await _addressCollection.UpdateManyAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return NotFound("No addresses found for the provided UserId.");
                }

                return Ok(new { message = "Addresses updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        // Get all addresses for a user
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAddresses(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId must be provided.");
            }

            try
            {
                var addresses = await _addressCollection.Find(a => a.UserId == userId).ToListAsync();
                if (addresses == null || addresses.Count == 0)
                {
                    return NotFound("No addresses found for this user.");
                }

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Delete an address by Id
        //[HttpDelete("{id}/{userId}")]
        //public async Task<IActionResult> DeleteAddress(string id, string userId)
        //{
        //    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
        //    {
        //        return BadRequest("Address Id and UserId must be provided.");
        //    }

        //    if (!ObjectId.TryParse(id, out var objectId)) // Convert string id to ObjectId
        //    {
        //        return BadRequest("Invalid Address Id format.");
        //    }

        //    try
        //    {
        //        var filter = Builders<Address>.Filter.And(
        //            Builders<Address>.Filter.Eq(a => a.Id, objectId),
        //            Builders<Address>.Filter.Eq(a => a.UserId, userId)
        //        );

        //        var result = await _addressCollection.DeleteOneAsync(filter);

        //        if (result.DeletedCount == 0)
        //        {
        //            return NotFound("Address not found or user does not have permission to delete this address.");
        //        }

        //        return Ok(new { message = "Address deleted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
    }
}
