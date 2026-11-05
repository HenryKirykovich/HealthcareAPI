namespace HealthcareAPI.Models
{
    // Possible statuses for an appointment
    public enum AppointmentStatus
    {
        Pending,    // just created, waiting for confirmation
        Confirmed,  // doctor confirmed the appointment
        Canceled    // canceled by patient, doctor, or admin
    }

    // Represents a healthcare appointment
    public class Appointment
    {
        public int Id { get; set; }

        // Who booked this appointment (Patient's username)
        public string PatientUsername { get; set; } = string.Empty;

        // Which doctor this appointment is with (Doctor's username)
        public string DoctorUsername { get; set; } = string.Empty;

        // When the appointment is scheduled
        public DateTime AppointmentDate { get; set; }

        // Why the patient is coming (e.g., "Annual checkup")
        public string Reason { get; set; } = string.Empty;

        // Current status - starts as Pending
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    }

    // Used when creating or updating an appointment (no Id or Status needed)
    public class AppointmentRequest
    {
        public string DoctorUsername { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // Used when updating appointment status
    public class UpdateStatusRequest
    {
        public AppointmentStatus Status { get; set; }
    }
}
