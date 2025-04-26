using MedSched.Api.Converters;
using MedSched.Api.Data;
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

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register PostgresSQL
builder.Services.AddDbContext<MedSchedContext>(opt =>
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var connectionString = dbUrl != null ?
        ConnectionStringConverter.ConvertDatabaseUrlToConnectionString(dbUrl) :
        builder.Configuration.GetConnectionString("PostgresConnection");
    opt.UseNpgsql(connectionString);
});

//Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddControllers();


var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MedSchedContext>();
    context.Database.Migrate();
}

//Always allow Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.MapControllers();
app.Run();
