using HealthcareAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthcareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All appointment endpoints require login
    public class AppointmentsController : ControllerBase
    {
        // Helper: get current user's username from JWT token
        private string GetCurrentUsername() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        // Helper: get current user's role from JWT token
        private string GetCurrentRole() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // ─────────────────────────────────────────
        // PATIENT ENDPOINTS
        // ─────────────────────────────────────────

        // POST api/appointments/book
        // Only Patients can book a new appointment
        [HttpPost("book")]
        [Authorize(Roles = "Patient")]
        public IActionResult BookAppointment(AppointmentRequest request)
        {
            if (string.IsNullOrEmpty(request.DoctorUsername))
                return BadRequest("Doctor username is required");

            if (request.AppointmentDate < DateTime.UtcNow)
                return BadRequest("Appointment date must be in the future");

            var appointment = new Appointment
            {
                Id = DataStore.Appointments.Count + 1,
                PatientUsername = GetCurrentUsername(),
                DoctorUsername = request.DoctorUsername,
                AppointmentDate = request.AppointmentDate,
                Reason = request.Reason,
                Status = AppointmentStatus.Pending  // new appointments start as Pending
            };

            DataStore.Appointments.Add(appointment);

            return Ok(new { Message = "Appointment booked!", Appointment = appointment });
        }

        // GET api/appointments/my-appointments
        // Patients see only their own appointments
        [HttpGet("my-appointments")]
        [Authorize(Roles = "Patient")]
        public IActionResult GetMyAppointments()
        {
            var username = GetCurrentUsername();

            var myAppointments = DataStore.Appointments
                .Where(a => a.PatientUsername == username)
                .ToList();

            return Ok(myAppointments);
        }

        // DELETE api/appointments/cancel/5
        // Patients can cancel only their OWN appointments
        [HttpDelete("cancel/{id}")]
        [Authorize(Roles = "Patient")]
        public IActionResult CancelAppointment(int id)
        {
            var appointment = DataStore.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            // Security check: patient can't cancel someone else's appointment
            if (appointment.PatientUsername != GetCurrentUsername())
                return Forbid();

            appointment.Status = AppointmentStatus.Canceled;

            return Ok(new { Message = "Appointment canceled" });
        }

        // ─────────────────────────────────────────
        // DOCTOR ENDPOINTS
        // ─────────────────────────────────────────

        // GET api/appointments/doctor-schedule
        // Doctors see all appointments assigned to them
        [HttpGet("doctor-schedule")]
        [Authorize(Roles = "Doctor")]
        public IActionResult GetDoctorSchedule()
        {
            var username = GetCurrentUsername();

            var schedule = DataStore.Appointments
                .Where(a => a.DoctorUsername == username)
                .ToList();

            return Ok(schedule);
        }

        // PUT api/appointments/update-status/5
        // Doctors and Admins can update appointment status (Pending/Confirmed/Canceled)
        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public IActionResult UpdateStatus(int id, UpdateStatusRequest request)
        {
            var appointment = DataStore.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            // Doctor can only update appointments assigned to them
            if (GetCurrentRole() == "Doctor" && appointment.DoctorUsername != GetCurrentUsername())
                return Forbid();

            appointment.Status = request.Status;

            return Ok(new { Message = $"Status updated to: {request.Status}", Appointment = appointment });
        }

        // PUT api/appointments/reschedule/5
        // Doctors can reschedule their own appointments
        [HttpPut("reschedule/{id}")]
        [Authorize(Roles = "Doctor")]
        public IActionResult RescheduleAppointment(int id, AppointmentRequest request)
        {
            var appointment = DataStore.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.DoctorUsername != GetCurrentUsername())
                return Forbid();

            if (request.AppointmentDate < DateTime.UtcNow)
                return BadRequest("New date must be in the future");

            appointment.AppointmentDate = request.AppointmentDate;
            appointment.Reason = request.Reason;

            return Ok(new { Message = "Appointment rescheduled", Appointment = appointment });
        }

        // ─────────────────────────────────────────
        // ADMIN ENDPOINTS
        // ─────────────────────────────────────────

        // GET api/appointments/all
        // Admin can see ALL appointments in the system
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllAppointments()
        {
            return Ok(DataStore.Appointments);
        }

        // DELETE api/appointments/delete/5
        // Admin can permanently delete any appointment
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteAppointment(int id)
        {
            var appointment = DataStore.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            DataStore.Appointments.Remove(appointment);

            return Ok(new { Message = $"Appointment {id} permanently deleted" });
        }
    }
}
