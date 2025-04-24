using MedSched.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Register DB Context
builder.Services.AddDbContext<MedSchedContext>(opt =>
    opt.UseInMemoryDatabase("MedSchedDB"));

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
