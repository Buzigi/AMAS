using System;
using AutoMapper;
using MedSched.Api.DTOs;
using MedSched.Api.Models;

namespace MedSched.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AppointmentRequest, Appointment>();
        CreateMap<Appointment, GetAppointmentResponse>();
    }
}
