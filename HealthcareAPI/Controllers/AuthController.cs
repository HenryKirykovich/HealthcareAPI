using HealthcareAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HealthcareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _secretKey;

        public AuthController(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:SecretKey"]!;
        }

        // POST api/auth/register
        // Open to everyone - no token needed
        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            // Check required fields
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                return BadRequest("Username and password are required");

            // Check if username already taken
            if (DataStore.Users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists");

            // Only allow valid roles
            if (user.Role != "Admin" && user.Role != "Doctor" && user.Role != "Patient")
                return BadRequest("Role must be: Admin, Doctor, or Patient");

            // Assign ID and hash password
            user.Id = DataStore.Users.Count + 1;
            user.Password = HashPassword(user.Password);

            DataStore.Users.Add(user);

            return Ok(new { Message = $"Welcome, {user.FirstName}! Registered as {user.Role}" });
        }

        // POST api/auth/login
        // Returns JWT token on success
        [HttpPost("login")]
        public IActionResult Login(UserLogin userLogin)
        {
            // Find user by username
            var user = DataStore.Users.FirstOrDefault(u => u.Username == userLogin.Username);

            // Check credentials
            if (user == null || !VerifyPassword(userLogin.Password, user.Password))
                return Unauthorized("Invalid username or password");

            // Create JWT token with user info and role
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),  // role is stored in token
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim("UserId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                Role = user.Role,
                ExpiresIn = "2 hours"
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
