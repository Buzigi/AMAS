using System;
using AutoMapper;
using MedSched.Api.Data;
using MedSched.Api.DTOs;
using MedSched.Api.Interfaces;
using MedSched.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MedSched.Api.Services;

public class AppointmentService : IAppointmentService
{
    private readonly MedSchedContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
            MedSchedContext context,
            IMapper mapper,
            ILogger<AppointmentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<GetAppointmentResponse>> GetAllAppointmentsAsync()
    {
        try
        {
            var appointments = _mapper.Map<List<GetAppointmentResponse>>(await _context.Appointments.ToListAsync());
            _logger.LogInformation($"Retrieved {appointments.Count} appointments");
            return appointments;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while retrieving appointments: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    public async Task<GetAppointmentResponse?> GetAppointmentByIdAsync(int id)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                var errorMessage = $"Appointment with Id= {id} not found";
                _logger.LogWarning(errorMessage);
                return null;
            }

            _logger.LogInformation($"Retrieved appointment id= {id} details");

            var appointmentRes = _mapper.Map<GetAppointmentResponse>(appointment);

            return appointmentRes;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while retrieving the appointment with Id= {id}: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    public async Task<IEnumerable<GetAppointmentResponse>?> GetAppointmentsByHCProfessionalAsync(string hcName)
    {
        try
        {
            var appointments = await _context.Appointments.Where(a => a.HealthcareProfessionalName == hcName).ToListAsync();

            if (appointments == null)
            {
                var errorMessage = $"No appointment for healthcare professional {hcName}";
                _logger.LogWarning(errorMessage);
                return null;
            }

            _logger.LogInformation($"Retrieved {appointments.Count} for healthcare professional {hcName}");

            var appointmentsRes = _mapper.Map<List<GetAppointmentResponse>>(appointments);

            return appointmentsRes;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while retrieving appointments for healthcare professional {hcName}: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    public async Task<CreateAppointmentResponse> CreateAppointmentAsync(AppointmentRequest appointmentReq)
    {
        try
        {
            var appointment = _mapper.Map<Appointment>(appointmentReq);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            //TODO: add check if scheduale conflict + suggest new times
            var appointmentRes = new CreateAppointmentResponse()
            {
                AppointmentId = appointment.Id,
                Success = true
            };

            _logger.LogInformation($"Appointment with id= {appointment.Id} created successfully");

            return appointmentRes;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while creating the appointment: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    public async Task<bool> UpdateAppointmentAsync(int id, AppointmentRequest updateReq)
    {
        try
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
            {
                _logger.LogWarning($"No appointment with id= {id} found for update.");
                return false;
            }

            existingAppointment.AppointmentDate = updateReq.AppointmentDate != default ?
                DateTime.SpecifyKind(updateReq.AppointmentDate, DateTimeKind.Utc) : //Compatability with Postgres definitions
                existingAppointment.AppointmentDate;
            existingAppointment.Description = !string.IsNullOrEmpty(updateReq.Description) ? updateReq.Description : existingAppointment.Description;
            existingAppointment.Duration = updateReq.Duration != 0 ? updateReq.Duration : existingAppointment.Duration;
            existingAppointment.HealthcareProfessionalName = !string.IsNullOrEmpty(updateReq.HealthcareProfessionalName) ?
                updateReq.HealthcareProfessionalName :
                existingAppointment.HealthcareProfessionalName;
            existingAppointment.PatientName = !string.IsNullOrEmpty(updateReq.PatientName) ? updateReq.PatientName : existingAppointment.PatientName;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Updated appointment with id= {id} details");

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while updating the appointment with id= {id}: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        try
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
            {
                var errorMessage = $"No appointment with id= {id} found for deletion.";
                _logger.LogWarning(errorMessage);
                return false;
            }

            _context.Appointments.Remove(existingAppointment);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted appointment with id= {id}");

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while deleting the appointment with id= {id}: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }
}
