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
    private readonly IConfiguration _config;

    public AppointmentService(
            MedSchedContext context,
            IMapper mapper,
            ILogger<AppointmentService> logger,
            IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _config = configuration;
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
            var hasConflict = await HasSchedConflictAsync(
                appointmentReq.HealthcareProfessionalName,
                appointmentReq.AppointmentDate,
                appointmentReq.Duration);
            if (hasConflict)
            {
                var suggestedTimes = await SuggestNewTimes(
                    appointmentReq.HealthcareProfessionalName,
                    appointmentReq.AppointmentDate,
                    appointmentReq.Duration);

                return new CreateAppointmentResponse()
                {
                    Success = false,
                    SuggestedTimes = suggestedTimes
                };
            }

            var appointment = _mapper.Map<Appointment>(appointmentReq);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
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

    private async Task<bool> HasSchedConflictAsync(string hcProfName, DateTime wantedDate, int duration)
    {
        var endDate = wantedDate.AddMinutes(duration);

        return await _context.Appointments.AnyAsync(a =>
        (hcProfName == "All" || a.HealthcareProfessionalName == hcProfName) &&
        a.AppointmentDate < endDate &&
        wantedDate < a.AppointmentDate.AddMinutes(a.Duration));
    }

    private async Task<List<SuggestedTimeResponse>> SuggestNewTimes(string hcProfName, DateTime wantedDate, int duration)
    {
        var suggestions = new List<SuggestedTimeResponse>();

        int maxSuggestions = int.Parse(_config["NumberOfMeetingSuggestions"] ?? "4");

        var suggestionsStart = wantedDate.AddDays(-1) > DateTime.Now ? wantedDate.AddDays(-1) : DateTime.Now;

        var scheduled = await _context.Appointments
            .Where(a =>
                (hcProfName == "All" || a.HealthcareProfessionalName == hcProfName) &&
                a.AppointmentDate >= suggestionsStart)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();


        foreach (var appointment in scheduled)
        {
            var availableDuration = appointment.AppointmentDate - suggestionsStart;
            if (availableDuration >= new TimeSpan(duration))
            {
                suggestions.Add(new SuggestedTimeResponse()
                {
                    AppointmentStart = suggestionsStart,
                    Duration = duration
                });

                if (suggestions.Count == maxSuggestions)
                {
                    return suggestions;
                }
            }

            suggestionsStart = suggestionsStart.AddMinutes(duration);
        }

        while (suggestions.Count < maxSuggestions)
        {
            suggestions.Add(new SuggestedTimeResponse()
            {
                AppointmentStart = suggestionsStart,
                Duration = duration
            });
            suggestionsStart.AddMinutes(duration);
        }

        return suggestions;
    }
}
