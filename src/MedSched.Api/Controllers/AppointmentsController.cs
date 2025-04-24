using AutoMapper;
using MedSched.Api.Data;
using MedSched.Api.DTOs;
using MedSched.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedSched.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly MedSchedContext _context;
        private readonly IMapper _mapper;

        public AppointmentsController(MedSchedContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //Get: api/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointmentsAsync()
        {
            var appointments = _mapper.Map<List<GetAppointmentResponse>>(await _context.Appointments.ToListAsync());
            return Ok(appointments);
        }

        //Get: api/Appointments
        [HttpGet("{id}")]
        public async Task<ActionResult<GetAppointmentResponse>> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound($"Appointment with Id {id} not found");
            }

            var appointmentRes = _mapper.Map<GetAppointmentResponse>(appointment);

            return Ok(appointmentRes);
        }

        //Post: api/Appointments
        [HttpPost]
        public async Task<ActionResult<CreateAppointmentResponse>> CreateAppointmentAsync(
            AppointmentRequest appointmentReq)
        {
            var appointment = _mapper.Map<Appointment>(appointmentReq);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            //TODO: add check if scheduale conflict + suggest new times
            var appointmentRes = new CreateAppointmentResponse()
            {
                AppointmentId = appointment.Id
            };
            return Ok(appointmentRes);
        }

        //Put: api/Appointments/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAppointment(
            int id,
            [FromBody] AppointmentRequest updateReq
        )
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
            {
                return NotFound($"No appointment with ID {id}");
            }

            _mapper.Map(updateReq, existingAppointment);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency error occurred while updating the appointment.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while updating the appointment: {ex.Message}");
            }
        }

        //Delete: api/Appointments/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAppointmentAsync(int id)
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
            {
                return NotFound($"No appointment with ID {id}");
            }

            _context.Appointments.Remove(existingAppointment);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while deleting the appointment: {ex.Message}");
            }
        }
    }
}
