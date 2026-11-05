namespace HealthcareAPI.Models
{
    // Represents a registered user in the system
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // stored as SHA256 hash
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Role determines what this user can do:
        // "Admin"   - can see and manage everything
        // "Doctor"  - can manage their own appointments
        // "Patient" - can book and view their own appointments
        public string Role { get; set; } = "Patient";
    }
}
