using MedSched.Api.Converters;
using MedSched.Api.Data;
using MedSched.Api.Data.Seed;
using MedSched.Api.Interfaces;
using MedSched.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register Serilog
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
);

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5176")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register PostgresSQL
builder.Services.AddDbContext<MedSchedContext>(opt =>
{
    var environment = builder.Environment.EnvironmentName;
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Use InMemory database for Development or Docker (or as default)
    if (environment == "Development" || environment == "Docker" || dbUrl == null)
    {
        opt.UseInMemoryDatabase("MedSchedDb");
    }
    // Use Postgres in [dbUrl] in Production
    else
    {
        var connectionString = ConnectionStringConverter.ConvertDatabaseUrlToConnectionString(dbUrl);
        opt.UseNpgsql(connectionString);
    }
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register own services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddControllers();

var app = builder.Build();

//Always allow Swagger (available in Production too)
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

// Enable CORS
app.UseCors();

// When environment is Development or Docker
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    // Disable https locally or in Docker
    app.UseHttpsRedirection();

    // Seed the database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<MedSchedContext>();
        var appointmentService = services.GetRequiredService<IAppointmentService>();
        var logger = services.GetRequiredService<ILogger<DbContextSeeder>>();
        var env = services.GetRequiredService<IWebHostEnvironment>();

        var seeder = new DbContextSeeder(
            context,
            appointmentService,
            Path.Combine(env.ContentRootPath, "Data/Seed/seed_data.json"),
            logger);
        await seeder.SeedAsync();
    }
}
// When environment is Production
else
{
    // Apply migrations automatically to create DB and tables in Production
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MedSchedContext>();
        context.Database.Migrate();
    }
}

app.MapControllers();

app.Run();
