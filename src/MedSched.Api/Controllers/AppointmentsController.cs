using AutoMapper;
using MedSched.Api.Data;
using MedSched.Api.DTOs;
using MedSched.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MedSched.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly MedSchedContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            MedSchedContext context,
            IMapper mapper,
            ILogger<AppointmentsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        //Get: api/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointmentsAsync()
        {
            try
            {
                var appointments = _mapper.Map<List<GetAppointmentResponse>>(await _context.Appointments.ToListAsync());
                _logger.LogInformation($"GetAppointmentsAsync retrieved {appointments.Count} appointments");
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while retrieving appointments: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        //Get: api/Appointments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetAppointmentResponse>> GetAppointmentByIdAsync(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);

                if (appointment == null)
                {
                    var errorMessage = $"Appointment with Id= {id} not found";
                    _logger.LogWarning(errorMessage);
                    return NotFound(errorMessage);
                }

                _logger.LogInformation($"Retrieved appointment id= {id} details");

                var appointmentRes = _mapper.Map<GetAppointmentResponse>(appointment);

                return Ok(appointmentRes);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while retrieving the appointment with Id= {id}: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        //Post: api/Appointments
        [HttpPost]
        public async Task<ActionResult<CreateAppointmentResponse>> CreateAppointmentAsync(
            AppointmentRequest appointmentReq)
        {
            try
            {
                var appointment = _mapper.Map<Appointment>(appointmentReq);
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                //TODO: add check if scheduale conflict + suggest new times
                var appointmentRes = new CreateAppointmentResponse()
                {
                    AppointmentId = appointment.Id
                };


                _logger.LogInformation($"Appointment with id= {appointment.Id} created successfully");

                return Ok(appointmentRes);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while creating the appointment: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        //Put: api/Appointments/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAppointmentAsync(
            int id,
            [FromBody] AppointmentRequest updateReq
        )
        {
            try
            {
                var existingAppointment = await _context.Appointments.FindAsync(id);
                if (existingAppointment == null)
                {
                    var errorMessage = $"No appointment with id= {id} found for update.";
                    _logger.LogWarning(errorMessage);
                    return NotFound(errorMessage);
                }

                existingAppointment.AppointmentDate = updateReq.AppointmentDate != default ? updateReq.AppointmentDate : existingAppointment.AppointmentDate;
                existingAppointment.Description = !string.IsNullOrEmpty(updateReq.Description) ? updateReq.Description : existingAppointment.Description;
                existingAppointment.Duration = updateReq.Duration != 0 ? updateReq.Duration : existingAppointment.Duration;
                existingAppointment.HealthcareProfessionalName = !string.IsNullOrEmpty(updateReq.HealthcareProfessionalName) ?
                    updateReq.HealthcareProfessionalName :
                    existingAppointment.HealthcareProfessionalName;
                existingAppointment.PatientName = !string.IsNullOrEmpty(updateReq.PatientName) ? updateReq.PatientName : existingAppointment.PatientName;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated appointment with id= {id} details");

                return Ok();
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while updating the appointment with id= {id}: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        //Delete: api/Appointments/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAppointmentAsync(int id)
        {
            try
            {
                var existingAppointment = await _context.Appointments.FindAsync(id);
                if (existingAppointment == null)
                {
                    var errorMessage = $"No appointment with id= {id} found for deletion.";
                    _logger.LogWarning(errorMessage);
                    return NotFound(errorMessage);
                }

                _context.Appointments.Remove(existingAppointment);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted appointment with id= {id}");

                return Ok();
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while deleting the appointment with id= {id}: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }
    }
}
