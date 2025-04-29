using AutoMapper;
using MedSched.Api.Data;
using MedSched.Api.DTOs;
using MedSched.Api.Models;
using MedSched.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Moq;

namespace MedSched.Api.UnitTests;

public class AppointmentServiceTests
{
    private readonly MedSchedContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<AppointmentService>> _mockLogger;
    private readonly AppointmentService _appointmentService;

    #region setup
    public AppointmentServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        SetupMockMapper();

        _mockLogger = new Mock<ILogger<AppointmentService>>();

        _mockConfig = new Mock<IConfiguration>();

        var dbContextOptions = new DbContextOptionsBuilder<MedSchedContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new MedSchedContext(dbContextOptions);

        _appointmentService = new AppointmentService(
            _context,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockConfig.Object);
    }

    private void SetupTestData()
    {
        _context.Appointments.RemoveRange(_context.Appointments);
        _context.Appointments.Add(new Appointment
        {
            Id = 1,
            PatientName = "test",
            HealthcareProfessionalName = "dr-test1",
            AppointmentDate = DateTime.UtcNow,
            Duration = 30,
            Description = "Checkup"
        });
        _context.Appointments.Add(new Appointment
        {
            Id = 2,
            PatientName = "test",
            HealthcareProfessionalName = "dr-test2",
            AppointmentDate = DateTime.UtcNow.AddHours(2),
            Duration = 30,
            Description = "Checkup2"
        });
        _context.Appointments.Add(new Appointment
        {
            Id = 3,
            PatientName = "test",
            HealthcareProfessionalName = "dr-test1",
            AppointmentDate = DateTime.UtcNow.AddMinutes(90),
            Duration = 30,
            Description = "Checkup3"
        });
        _context.SaveChanges();
    }

    private void ClearTestData()
    {
        _context.Appointments.RemoveRange(_context.Appointments);
        _context.SaveChanges();

    }

    private void SetupMockMapper()
    {
        _mockMapper.Setup(m => m.Map<List<GetAppointmentResponse>>(It.IsAny<List<Appointment>>()))
                    .Returns((List<Appointment> appointments) =>
                        appointments.Select(a => new GetAppointmentResponse
                        {
                            Id = a.Id,
                            PatientName = a.PatientName,
                            HealthcareProfessionalName = a.HealthcareProfessionalName,
                            AppointmentDate = a.AppointmentDate,
                            Duration = a.Duration,
                            Description = a.Description
                        }).ToList());

        _mockMapper.Setup(m => m.Map<GetAppointmentResponse>(It.IsAny<Appointment>()))
            .Returns((Appointment appointment) => new GetAppointmentResponse
            {
                Id = appointment.Id,
                PatientName = appointment.PatientName,
                HealthcareProfessionalName = appointment.HealthcareProfessionalName,
                AppointmentDate = appointment.AppointmentDate,
                Duration = appointment.Duration,
                Description = appointment.Description
            });

        _mockMapper.Setup(m => m.Map<Appointment>(It.IsAny<AppointmentRequest>()))
            .Returns((AppointmentRequest appointmentReq) => new Appointment
            {
                PatientName = appointmentReq.PatientName,
                HealthcareProfessionalName = appointmentReq.HealthcareProfessionalName,
                AppointmentDate = appointmentReq.AppointmentDate,
                Duration = appointmentReq.Duration,
                Description = appointmentReq.Description
            });
    }

    #endregion

    #region GetAllAppointmentsAsync

    [Fact]
    public async Task GetAllAppointmentsAsync_WhenAppointmentsExist_ReturnsAppointments()
    {
        // Arrange
        SetupTestData();

        // Act
        var result = await _appointmentService.GetAllAppointmentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() == 3);
    }

    [Fact]
    public async Task GetAllAppointmentsAsync_NoAppointmentsExist_ReturnsEmptyList()
    {
        // Arrange
        ClearTestData();

        // Act
        var result = await _appointmentService.GetAllAppointmentsAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAppointmentsAsync_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        _mockMapper.Setup(m => m.Map<List<GetAppointmentResponse>>(It.IsAny<List<Appointment>>()))
            .Throws(new Exception("Mapping failed"));

        // Act && Asert
        var ex = await Assert.ThrowsAsync<Exception>(() => _appointmentService.GetAllAppointmentsAsync());

        // Assert
        Assert.Contains("An error occurred while retrieving appointments", ex.Message);
    }

    #endregion

    #region GetAppointmentByIdAsync

    [Fact]
    public async Task GetAppointmentByIdAsync_WhenAppointmentExists_ReturnsAppointment()
    {
        // Arrange
        SetupTestData();

        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_WhenAppointmentDoesNotExist_ReturnsNull()
    {
        // Arrange
        ClearTestData();

        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        SetupTestData();
        _mockMapper.Setup(m => m.Map<GetAppointmentResponse>(It.IsAny<Appointment>()))
            .Throws(new Exception("Mapping failed"));

        // Act && Asert
        var ex = await Assert.ThrowsAsync<Exception>(() => _appointmentService.GetAppointmentByIdAsync(1));

        // Assert
        Assert.Contains("An error occurred while retrieving the appointment", ex.Message);
    }

    #endregion

    #region GetAppointmentsByHCProfessionalAsync

    [Fact]
    public async Task GetAppointmentsByHCProfessionalAsync_WhenAppointmentExists_ReturnsAppointment()
    {
        // Arrange
        SetupTestData();

        // Act
        var result = await _appointmentService.GetAppointmentsByHCProfessionalAsync("dr-test1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAppointmentsByHCProfessionalAsync_WhenAppointmentDoesNotExist_ReturnsNull()
    {
        // Arrange
        ClearTestData();

        // Act
        var result = await _appointmentService.GetAppointmentsByHCProfessionalAsync("dr-test1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAppointmentsByHCProfessionalAsync_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        SetupTestData();
        _mockMapper.Setup(m => m.Map<List<GetAppointmentResponse>>(It.IsAny<List<Appointment>>()))
            .Throws(new Exception("Mapping failed"));

        // Act && Asert
        var ex = await Assert.ThrowsAsync<Exception>(() => _appointmentService.GetAppointmentsByHCProfessionalAsync("dr-test1"));

        // Assert
        Assert.Contains("An error occurred while retrieving appointments", ex.Message);
    }

    #endregion

    #region CreateAppointmentAsync

    [Fact]
    public async Task CreateAppointmentAsync_WhenNoConflict_CreatesSuccessfully()
    {
        // Arrange
        SetupTestData();
        var request = new AppointmentRequest
        {
            HealthcareProfessionalName = "Dr. Test",
            AppointmentDate = DateTime.UtcNow.AddHours(1),
            Duration = 15
        };

        // Act
        var result = await _appointmentService.CreateAppointmentAsync(request);

        // Assert
        Assert.True(result.Success);
    }

    [Theory]
    [InlineData(110, 15)]
    [InlineData(140, 30)]
    [InlineData(110, 60)]
    [InlineData(10, 120)]
    public async Task CreateAppointmentAsync_WhenConflictExists_ReturnsUnscheduledSuggestedTimes(int timeToAdd, int duration)
    {
        // Arrange
        SetupTestData();
        var request = new AppointmentRequest
        {
            HealthcareProfessionalName = "dr-test2",
            AppointmentDate = DateTime.UtcNow.AddMinutes(timeToAdd),
            Duration = duration
        };

        // Act
        var result = await _appointmentService.CreateAppointmentAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.SuggestedTimes);
        Assert.Equal(4, result.SuggestedTimes.Count);

        // Act
        var success = true;

        foreach (var time in result.SuggestedTimes)
        {
            var res = await _appointmentService.CreateAppointmentAsync(new AppointmentRequest()
            {

                HealthcareProfessionalName = "dr-test2",
                AppointmentDate = time.AppointmentStart,
                Duration = time.Duration
            });
            success &= res.Success;
        }

        // Assert

        Assert.True(success);
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        _mockMapper.Setup(m => m.Map<Appointment>(It.IsAny<AppointmentRequest>()))
            .Throws(new Exception("Mapping failed"));

        // Act && Asert
        var ex = await Assert.ThrowsAsync<Exception>(() => _appointmentService.CreateAppointmentAsync(new AppointmentRequest()
        {
            HealthcareProfessionalName = "Dr. Test",
            AppointmentDate = DateTime.UtcNow.AddHours(1),
            Duration = 15
        }));

        // Assert
        Assert.Contains("An error occurred while creating the appointment", ex.Message);
    }

    #endregion

    #region UpdateAppointmentAsync


    [Theory]
    [InlineData("1.1.2030 17:30", 15, "All", "", "")]
    [InlineData("", 25, "dr-test3", "testP", "Exam")]
    public async Task UpdateAppointmentAsync_WhenAppointmentExistsNoConflicts_UpdatesSuccessfully(
        string date,
        int duration,
        string hcProf,
        string patient,
        string desc)
    {
        // Arrange
        SetupTestData();

        var appointment = await _context.Appointments.Where(a => a.Id == 1).FirstOrDefaultAsync();

        var update = new AppointmentRequest
        {
            AppointmentDate = !string.IsNullOrEmpty(date) ? DateTime.Parse(date) : default(DateTime),
            Duration = duration,
            HealthcareProfessionalName = hcProf,
            PatientName = patient,
            Description = desc
        };

        // Act
        var result = await _appointmentService.UpdateAppointmentAsync(1, update);

        // Assert
        Assert.True(result.Success);

        var updated = await _context.Appointments.Where(a => a.Id == 1).FirstOrDefaultAsync();

        Assert.Equal(!string.IsNullOrEmpty(date) ? DateTime.Parse(date) : appointment?.AppointmentDate, updated?.AppointmentDate);
        Assert.Equal(duration != default ? duration : appointment?.Duration, updated?.Duration);
        Assert.Equal(string.Compare(hcProf, "All") != 0 ? hcProf : appointment?.HealthcareProfessionalName, updated?.HealthcareProfessionalName);
        Assert.Equal(!string.IsNullOrEmpty(patient) ? patient : appointment?.PatientName, updated?.PatientName);
        Assert.Equal(!string.IsNullOrEmpty(desc) ? desc : appointment?.Description, updated?.Description);
    }

    [Fact]
    public async Task UpdateAppointmentAsync_WhenAppointmentExistsWithConflicts_UpdatesSuccessfully()
    {
        // Arrange
        SetupTestData();
        int id = 3;

        var appointment = await _context.Appointments.Where(a => a.Id == id).FirstOrDefaultAsync();

        var update = new AppointmentRequest
        {
            AppointmentDate = DateTime.UtcNow
        };

        // Act
        var result = await _appointmentService.UpdateAppointmentAsync(id, update);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.SuggestedTimes);

    }

    [Fact]
    public async Task UpdateAppointmentAsync_WhenAppointmentDoesNotExist_ReturnsFalse()
    {
        // Arrange
        SetupTestData();

        // Act
        var result = await _appointmentService.UpdateAppointmentAsync(6, new AppointmentRequest());

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.SuggestedTimes);
    }

    [Fact]
    public async Task UpdateAppointmentAsync_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        SetupTestData();

        _mockLogger
            .Setup(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
            .Throws(new Exception("Logger failure"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _appointmentService.UpdateAppointmentAsync(7, new AppointmentRequest()));

        // Assert
        Assert.Contains("An error occurred while updating the appointment", ex.Message);
    }

    #endregion

    #region SuggestNewTimesAsync

    [Theory]
    [InlineData(new int[] { 0, 200, 240 }, 0, 30, new int[] { 30, 60, 90, 120 })]
    [InlineData(new int[] { 0, 200, 240 }, 15, 30, new int[] { 30, 60, 90, 120 })]
    [InlineData(new int[] { 15, 200, 240 }, 0, 30, new int[] { 45, 75, 105, 135 })]
    [InlineData(new int[] { 0, 30, 200 }, 0, 30, new int[] { 60, 90, 120, 150 })]
    [InlineData(new int[] { 15, 400, 440 }, 0, 60, new int[] { 45, 105, 165, 225 })]
    [InlineData(new int[] { 0, 35, 70 }, 0, 30, new int[] { 100, 130, 160, 190 })]
    [InlineData(new int[] { 0, 75, 195 }, 0, 45, new int[] { 30, 105, 150, 225 })]
    public async Task CreateAppointmentAsync_WhenConflictExists_CorrectSuggestionOfTimes(
        int[] schedTimes,
        int wantedTime,
        int wantedDuration,
        int[] suggestedTimes)
    {
        // Arrange
        ClearTestData();
        var now = DateTime.UtcNow;
        var request1 = new AppointmentRequest
        {
            AppointmentDate = now.AddMinutes(schedTimes[0]),
            Duration = 30
        };
        var request2 = new AppointmentRequest
        {
            AppointmentDate = now.AddMinutes(schedTimes[1]),
            Duration = 30
        };
        var request3 = new AppointmentRequest
        {
            AppointmentDate = now.AddMinutes(schedTimes[2]),
            Duration = 30
        };

        var wanted = new AppointmentRequest
        {
            AppointmentDate = now.AddMinutes(wantedTime),
            Duration = wantedDuration
        };

        _ = await _appointmentService.CreateAppointmentAsync(request1);
        _ = await _appointmentService.CreateAppointmentAsync(request2);
        _ = await _appointmentService.CreateAppointmentAsync(request3);

        // Act
        var result = await _appointmentService.CreateAppointmentAsync(wanted);

        // Assert

        Assert.False(result.Success);
        Assert.Equal(4, result.SuggestedTimes.Count);
        Assert.Equal(suggestedTimes[0], (result.SuggestedTimes[0].AppointmentStart - now).TotalMinutes);
        Assert.Equal(suggestedTimes[1], (result.SuggestedTimes[1].AppointmentStart - now).TotalMinutes);
        Assert.Equal(suggestedTimes[2], (result.SuggestedTimes[2].AppointmentStart - now).TotalMinutes);
        Assert.Equal(suggestedTimes[3], (result.SuggestedTimes[3].AppointmentStart - now).TotalMinutes);
    }

    #endregion

    #region DeleteAppointmentAsync

    [Fact]
    public async Task DeleteAppointmentAsync_WhenAppointmentExists_DeletesSuccessfully()
    {
        // Arrange
        SetupTestData();

        // Act
        var result = await _appointmentService.DeleteAppointmentAsync(1);

        // Assert
        Assert.True(result);

        var afterDeletion = await _context.Appointments.ToListAsync();
        Assert.Equal(2, afterDeletion.Count);
    }

    [Fact]
    public async Task DeleteAppointmentAsync_WhenAppointmentDoesNotExist_ReturnsFalse()
    {
        // Arrange
        ClearTestData();

        // Act
        var result = await _appointmentService.DeleteAppointmentAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAppointmentAsync_WhenExceptionThrown_ThrowsException()
    {
        // Arrange
        _mockLogger
            .Setup(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
            .Throws(new Exception("Logger failure"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _appointmentService.DeleteAppointmentAsync(1));

        // Assert
        Assert.Contains("An error occurred while deleting the appointment", ex.Message);
    }
    #endregion
}