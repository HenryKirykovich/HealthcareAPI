namespace HealthcareAPI.Models
{
    // Used only for login requests - just username and password
    public class UserLogin
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
