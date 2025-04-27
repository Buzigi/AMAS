using System;
using MedSched.Api.DTOs;

namespace MedSched.Api.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<GetAppointmentResponse>> GetAllAppointmentsAsync();
    Task<GetAppointmentResponse?> GetAppointmentByIdAsync(int id);
    Task<IEnumerable<GetAppointmentResponse>?> GetAppointmentsByHCProfessionalAsync(string hcName);
    Task<CreateAppointmentResponse> CreateAppointmentAsync(AppointmentRequest appointmentReq);
    Task<CreateAppointmentResponse> UpdateAppointmentAsync(int id, AppointmentRequest updateReq);
    Task<bool> DeleteAppointmentAsync(int id);
}
