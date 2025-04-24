
namespace MedSched.Api.Models;

public class Appointment
{
    public int Id { get; set; }
    public string? PatientName { get; set; }
    public string? HealthcareProfessionalName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }
}

