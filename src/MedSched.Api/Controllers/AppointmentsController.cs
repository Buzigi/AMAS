using AutoMapper;
using MedSched.Api.Data;
using MedSched.Api.DTOs;
using MedSched.Api.Interfaces;
using MedSched.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedSched.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        //Get: api/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointmentsAsync()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Get: api/Appointments/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetAppointmentResponse>> GetAppointmentByIdAsync(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

                if (appointment == null)
                {
                    return NotFound($"Appointment with Id= {id} not found");
                }

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Get: api/Appointments/{hcName}
        [HttpGet("{hcName}")]
        public async Task<ActionResult<GetAppointmentResponse>> GetAppointmentByHCProfessionalAsync(string hcName)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByHCProfessionalAsync(hcName);

                if (appointments == null)
                {
                    return NotFound($"No appointment for healthcare professional {hcName}");
                }

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Post: api/Appointments
        [HttpPost]
        public async Task<ActionResult<CreateAppointmentResponse>> CreateAppointmentAsync(
            AppointmentRequest appointmentReq)
        {
            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(appointmentReq);

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
                var success = await _appointmentService.UpdateAppointmentAsync(id, updateReq);

                if (!success)
                {
                    return NotFound($"No appointment with id= {id} found for update.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Delete: api/Appointments/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAppointmentAsync(int id)
        {
            try
            {
                var success = await _appointmentService.DeleteAppointmentAsync(id);

                if (!success)
                {
                    return NotFound($"No appointment with id= {id} found for deletion.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
