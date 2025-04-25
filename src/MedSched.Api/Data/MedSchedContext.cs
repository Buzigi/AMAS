using System;
using MedSched.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MedSched.Api.Data;

public class MedSchedContext : DbContext
{
    public MedSchedContext(DbContextOptions<MedSchedContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(e => e.AppointmentDate)
                .HasColumnType("timestamp with time zone");
        });
    }
}
