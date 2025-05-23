using System;
using MedSched.Api.Models;

namespace MedSched.Api.DTOs;

public class CreateAppointmentResponse
{
    public bool Success { get; set; }
    public List<SuggestedTimeResponse> SuggestedTimes { get; set; } = new();
}
