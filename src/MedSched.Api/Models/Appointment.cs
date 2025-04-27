using System.ComponentModel.DataAnnotations;

namespace MedSched.Api.Models;

public class Appointment
{
    public int Id { get; set; }
    public string? PatientName { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "HealthcareProfessionalName cannot be empty.")]
    public string HealthcareProfessionalName { get; set; } = "All";
    public DateTime AppointmentDate { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }
}

