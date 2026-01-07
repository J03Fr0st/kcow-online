using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kcow.Infrastructure.Data;

/// <summary>
/// Application database context for EF Core with SQLite.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<ClassGroup> ClassGroups => Set<ClassGroup>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
