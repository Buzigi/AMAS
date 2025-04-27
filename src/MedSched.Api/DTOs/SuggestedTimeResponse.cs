using System;

namespace MedSched.Api.DTOs;

public class SuggestedTimeResponse
{
    public DateTime AppointmentStart { get; set; }
    public int Duration { get; set; }
}
