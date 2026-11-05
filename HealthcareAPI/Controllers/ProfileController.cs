using HealthcareAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HealthcareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints here require a valid JWT token
    public class ProfileController : ControllerBase
    {
        // GET api/profile/me
        // Any logged-in user can view their own profile
        [HttpGet("me")]
        public IActionResult GetMyProfile()
        {
            // Read username from the JWT token
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = DataStore.Users.FirstOrDefault(u => u.Username == username);

            if (user == null) return NotFound("User not found");

            // Return profile info (without password)
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role
            });
        }

        // PUT api/profile/update
        // Any logged-in user can update their own info
        [HttpPut("update")]
        public IActionResult UpdateProfile(UpdateProfileRequest request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = DataStore.Users.FirstOrDefault(u => u.Username == username);

            if (user == null) return NotFound("User not found");

            // Update the allowed fields
            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            return Ok(new { Message = "Profile updated successfully" });
        }

        // PUT api/profile/change-password
        // Any logged-in user can change their own password
        [HttpPut("change-password")]
        public IActionResult ChangePassword(ChangePasswordRequest request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = DataStore.Users.FirstOrDefault(u => u.Username == username);

            if (user == null) return NotFound("User not found");

            // Must verify current password first
            if (HashPassword(request.CurrentPassword) != user.Password)
                return BadRequest("Current password is incorrect");

            if (string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("New password cannot be empty");

            user.Password = HashPassword(request.NewPassword);

            return Ok(new { Message = "Password changed successfully" });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
