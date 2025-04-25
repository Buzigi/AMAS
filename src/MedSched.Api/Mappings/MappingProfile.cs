using System;
using AutoMapper;
using MedSched.Api.DTOs;
using MedSched.Api.Models;

namespace MedSched.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AppointmentRequest, Appointment>()
            .ForMember(dest => dest.AppointmentDate,
                opt => opt.MapFrom(src => DateTime.SpecifyKind(src.AppointmentDate, DateTimeKind.Utc)));

        CreateMap<Appointment, GetAppointmentResponse>()
            .ForMember(dest => dest.AppointmentDate,
                opt => opt.MapFrom(src => DateTime.SpecifyKind(src.AppointmentDate, DateTimeKind.Utc)));
    }
}
