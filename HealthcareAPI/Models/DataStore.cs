namespace HealthcareAPI.Models
{
    // Simple in-memory data storage shared across all controllers
    // In a real app this would be replaced with a database (e.g., Entity Framework + SQL Server)
    public static class DataStore
    {
        public static List<User> Users { get; } = new List<User>();
        public static List<Appointment> Appointments { get; } = new List<Appointment>();
    }
}
