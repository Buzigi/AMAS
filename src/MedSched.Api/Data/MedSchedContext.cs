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

    public DbSet<Appointment> appointments { get; set; }
}
