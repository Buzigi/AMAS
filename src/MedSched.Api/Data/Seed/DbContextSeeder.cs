using System.Text.Json;
using MedSched.Api.DTOs;
using MedSched.Api.Interfaces;

namespace MedSched.Api.Data.Seed;

public class DbContextSeeder
{
    private readonly MedSchedContext _context;
    private readonly IAppointmentService _appointmentService;
    private readonly string _seedFilePath;
    private readonly ILogger<DbContextSeeder> _logger;

    public DbContextSeeder(
        MedSchedContext context,
        IAppointmentService appointmentService,
        string seedFilePath,
        ILogger<DbContextSeeder> logger)
    {
        _context = context;
        _appointmentService = appointmentService;
        _seedFilePath = seedFilePath;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Database already seeded
        if (_context.Appointments.Any())
        {
            return;
        }

        try
        {
            // Read the seed_data.json file
            var seedDataJson = await File.ReadAllTextAsync(_seedFilePath);
            var seedData = JsonSerializer.Deserialize<List<AppointmentRequest>>(seedDataJson, new JsonSerializerOptions
            {
                // JSON and model deffer in casing
                PropertyNameCaseInsensitive = true
            });

            if (seedData != null)
            {
                foreach (var appointment in seedData)
                {
                    _ = _appointmentService.CreateAppointmentAsync(appointment);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error seeding database: {ex.Message}");
        }
    }
}
