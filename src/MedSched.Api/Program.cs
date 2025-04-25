using MedSched.Api.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SQLitePCL;

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

// Initialize SQLitePCL batteries
Batteries.Init();

//Register DB Context
builder.Services.AddDbContext<MedSchedContext>(opt =>
    opt.UseSqlite("Data Source=Data/appointments.db"));


Directory.CreateDirectory("Data"); // ensure folder exists

//Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
