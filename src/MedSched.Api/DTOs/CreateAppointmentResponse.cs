using System;
using MedSched.Api.Models;

namespace MedSched.Api.DTOs;

public class CreateAppointmentResponse
{
    public bool Success { get; set; }

    public int AppointmentId { get; set; }

    public List<(DateTime, int)> SuggestedTimes { get; set; } = new();
}
