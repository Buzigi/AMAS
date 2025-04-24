using System;

namespace MedSched.Api.DTOs;

public class CreateAppointmentRequest
{
    public string? PatientName { get; set; }
    public string? HealthcareProfessionalName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }

}
