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

        // //Get: api/Appointments
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<GetAppointmentResponse>>> GetAppointmentsAsync()
        // {
        //     return await _context.Appointments.ToListAsync();
        // }

        // //Get: api/Appointments
        // [HttpGet("{id}")]
        // public async Task<ActionResult<GetAppointmentResponse>> GetAppointmentByIdAsync(int id)
        // {
        //     var appointment = await _context.Appointments.Where(a => a.Id == id).FirstOrDefaultAsync();

        //     if (appointment == null)
        //     {
        //         return NotFound($"Appointment with Id {id} not found");
        //     }

        //     return Ok(appointment);
        // }

        //Post: api/Appointments
        [HttpPost]
        public async Task<ActionResult<CreateAppointmentResponse>> CreateAppointmentAsync(
            CreateAppointmentRequest appointmentReq)
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
    }
}
