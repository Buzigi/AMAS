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

    public async Task<IEnumerable<GetAppointmentResponse>?> GetAllAppointmentsAsync()
    {
        try
        {
            var appointments = _mapper.Map<List<GetAppointmentResponse>>(await _context.Appointments.ToListAsync());

            if (appointments == null || appointments.Count() == 0)
            {
                _logger.LogInformation("No appointments found");
                return null;
            }

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
                var errorMessage = $"Appointment with id = {id} not found";
                _logger.LogWarning(errorMessage);
                return null;
            }

            _logger.LogInformation($"Retrieved appointment id = {id} details");

            var appointmentRes = _mapper.Map<GetAppointmentResponse>(appointment);

            return appointmentRes;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while retrieving the appointment with id = {id}: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    public async Task<IEnumerable<GetAppointmentResponse>?> GetAppointmentsByHCProfessionalAsync(string hcName)
    {
        try
        {
            var appointments = await _context.Appointments.Where(a => a.HealthcareProfessionalName == hcName).ToListAsync();

            if (appointments == null || appointments.Count() == 0)
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
                var suggestedTimes = await SuggestNewTimesAsync(
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
                Success = true
            };

            _logger.LogInformation($"Appointment with id = {appointment.Id} created successfully");

            return appointmentRes;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while creating the appointment: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    public async Task<CreateAppointmentResponse> UpdateAppointmentAsync(int id, AppointmentRequest updateReq)
    {
        try
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
            {
                _logger.LogWarning($"No appointment with id = {id} found for update.");
                return new CreateAppointmentResponse() { Success = false };
            }

            updateReq.HealthcareProfessionalName = string.Compare(updateReq.HealthcareProfessionalName, "All") != 0 ?
                updateReq.HealthcareProfessionalName :
                existingAppointment.HealthcareProfessionalName;
            updateReq.AppointmentDate = updateReq.AppointmentDate != default ?
                DateTime.SpecifyKind(updateReq.AppointmentDate, DateTimeKind.Utc) :
                existingAppointment.AppointmentDate;
            updateReq.Duration = updateReq.Duration != 0 ? updateReq.Duration : existingAppointment.Duration;

            var hasConflict = await HasSchedConflictAsync(
                updateReq.HealthcareProfessionalName,
                updateReq.AppointmentDate,
                updateReq.Duration,
                id);
            if (hasConflict)
            {
                var suggestedTimes = await SuggestNewTimesAsync(
                    updateReq.HealthcareProfessionalName,
                    updateReq.AppointmentDate,
                    updateReq.Duration,
                    id);

                return new CreateAppointmentResponse()
                {
                    Success = false,
                    SuggestedTimes = suggestedTimes
                };
            }

            existingAppointment.AppointmentDate = updateReq.AppointmentDate;
            existingAppointment.Description = !string.IsNullOrEmpty(updateReq.Description) ? updateReq.Description : existingAppointment.Description;
            existingAppointment.Duration = updateReq.Duration;
            existingAppointment.HealthcareProfessionalName = updateReq.HealthcareProfessionalName;
            existingAppointment.PatientName = !string.IsNullOrEmpty(updateReq.PatientName) ? updateReq.PatientName : existingAppointment.PatientName;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Updated appointment with id = {id} details");

            return new CreateAppointmentResponse() { Success = true };
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while updating the appointment with id = {id}: {ex.Message}";
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
                var errorMessage = $"No appointment with id = {id} found for deletion.";
                _logger.LogWarning(errorMessage);
                return false;
            }

            _context.Appointments.Remove(existingAppointment);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted appointment with id = {id}");

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while deleting the appointment with id = {id}: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new Exception(errorMessage);
        }
    }

    /// <summary>
    /// Checks if there is a scheduling conflict for a given healthcare professional, date, and duration.
    /// </summary>
    /// <param name="hcProfName">The name of the healthcare professional.</param>
    /// <param name="wantedDate">The desired appointment date and time.</param>
    /// <param name="duration">The duration of the appointment in minutes.</param>
    /// <param name="id">The ID of the appointment to exclude from the conflict check (optional).</param>
    /// <returns>True if there is a scheduling conflict, otherwise false.</returns>
    private async Task<bool> HasSchedConflictAsync(string hcProfName, DateTime wantedDate, int duration, int id = 0)
    {
        var endDate = wantedDate.AddMinutes(duration);

        // Does {hcProfName} has an appointment that starts before the wanted appointment finises, 
        // and ends after the wanted appointment starts?
        //In case of an update, exclude the updated appointment from the list
        return await _context.Appointments.AnyAsync(a =>
        (hcProfName == "All" || a.HealthcareProfessionalName == hcProfName) &&
        a.AppointmentDate < endDate &&
        wantedDate < a.AppointmentDate.AddMinutes(a.Duration) &&
        (id == 0 || a.Id != id));
    }

    /// <summary>
    /// Suggests new available times for an appointment based on the desired date, duration, and healthcare professional.
    /// </summary>
    /// <param name="hcProfName">The name of the healthcare professional.</param>
    /// <param name="wantedDate">The desired appointment date and time.</param>
    /// <param name="duration">The duration of the appointment in minutes.</param>
    /// <param name="id">The ID of the appointment to exclude from the suggestions (optional).</param>
    /// <returns>A list of suggested times for the appointment.</returns>
    private async Task<List<SuggestedTimeResponse>> SuggestNewTimesAsync(string hcProfName, DateTime wantedDate, int duration, int id = 0)
    {
        var suggestions = new List<SuggestedTimeResponse>();

        // Set number of new times suggestions
        int maxSuggestions = int.Parse(_config["NumberOfMeetingSuggestions"] ?? "4");

        // Only suggest time from original request date or 5 minutes from now (the farthest of them)
        var suggestionsStart = wantedDate > DateTime.UtcNow.AddMinutes(5) ? wantedDate : DateTime.UtcNow.AddMinutes(5);

        // Get all appointment for the {hcProfName} that ends after the wanted date and time.
        //In case of an update, exclude the updated appointment from the list
        var scheduled = await _context.Appointments
            .Where(a =>
                (hcProfName == "All" || a.HealthcareProfessionalName == hcProfName) &&
                a.AppointmentDate.AddMinutes(a.Duration) >= suggestionsStart &&
                (id == 0 || a.Id != id))
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();


        foreach (var appointment in scheduled)
        {
            // While there is an available space for a new meeting
            var availableDuration = appointment.AppointmentDate - suggestionsStart;
            while (availableDuration >= TimeSpan.FromMinutes(duration))
            {
                // Add the suggested meeting to the list
                suggestions.Add(new SuggestedTimeResponse()
                {
                    AppointmentStart = suggestionsStart,
                    Duration = duration
                });
                // If max number of suggestion reached, return the list
                if (suggestions.Count == maxSuggestions)
                {
                    return suggestions;
                }
                //Update time to check to be after the already suggested time, then update the available space
                suggestionsStart = suggestionsStart.AddMinutes(duration);
                availableDuration = appointment.AppointmentDate - suggestionsStart;
            }
            // When there is no more space before the next meeting, update time to check to be at end of scheduled meeting
            suggestionsStart = appointment.AppointmentDate.AddMinutes(appointment.Duration);
        }

        // When there are no more meetings, add times in {duration} steps to list
        while (suggestions.Count < maxSuggestions)
        {
            suggestions.Add(new SuggestedTimeResponse()
            {
                AppointmentStart = suggestionsStart,
                Duration = duration
            });
            suggestionsStart = suggestionsStart.AddMinutes(duration);
        }

        return suggestions;
    }
}
