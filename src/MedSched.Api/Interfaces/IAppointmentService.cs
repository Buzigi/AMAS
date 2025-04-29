using System;
using MedSched.Api.DTOs;

namespace MedSched.Api.Interfaces;

public interface IAppointmentService
{
    /// <summary>
    /// Retrieves all appointments from the database.
    /// </summary>
    /// <returns>A list of appointments or null if no appointments are found.</returns>
    Task<IEnumerable<GetAppointmentResponse>?> GetAllAppointmentsAsync();

    /// <summary>
    /// Retrieves a specific appointment by its ID.
    /// </summary>
    /// <param name="id">The ID of the appointment to retrieve.</param>
    /// <returns>The appointment details or null if not found.</returns>
    Task<GetAppointmentResponse?> GetAppointmentByIdAsync(int id);

    /// <summary>
    /// Retrieves appointments for a specific healthcare professional.
    /// </summary>
    /// <param name="hcName">The name of the healthcare professional.</param>
    /// <returns>A list of appointments or null if no appointments are found.</returns>
    Task<IEnumerable<GetAppointmentResponse>?> GetAppointmentsByHCProfessionalAsync(string hcName);

    /// <summary>
    /// Creates a new appointment in the database.
    /// </summary>
    /// <param name="appointmentReq">The appointment request details.</param>
    /// <returns>A response indicating success or failure, with new suggested times if there is a conflict.</returns>
    Task<CreateAppointmentResponse> CreateAppointmentAsync(AppointmentRequest appointmentReq);

    /// <summary>
    /// Updates an existing appointment in the database.
    /// </summary>
    /// <param name="id">The ID of the appointment to update.</param>
    /// <param name="updateReq">The updated appointment details.</param>
    /// <returns>A response indicating success or failure, with suggested times if there is a conflict.</returns>
    Task<CreateAppointmentResponse> UpdateAppointmentAsync(int id, AppointmentRequest updateReq);

    /// <summary>
    /// Deletes an appointment from the database.
    /// </summary>
    /// <param name="id">The ID of the appointment to delete.</param>
    /// <returns>True if the deletion was successful, otherwise false.</returns>
    Task<bool> DeleteAppointmentAsync(int id);
}
