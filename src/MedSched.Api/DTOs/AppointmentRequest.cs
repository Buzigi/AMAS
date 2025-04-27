using System;
using System.ComponentModel.DataAnnotations;

namespace MedSched.Api.DTOs;

public class AppointmentRequest
{
    public string? PatientName { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "HealthcareProfessionalName cannot be empty.")]
    public string HealthcareProfessionalName { get; set; } = "All";
    public DateTime AppointmentDate { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }

}
