using EventManagementSystem.Models;
using EventManagementSystem.Models.UserRelated;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AccountModel>(options)
{
    public DbSet<EventModel> Events { get; set; }
    public DbSet<GuestModel> Guests { get; set; }
    public DbSet<VenueModel> Venues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventModel>()
            .HasKey(e => e.EventId);
        
        modelBuilder.Entity<EventModel>()
            .Property(e => e.EventDate)
            .HasColumnType("date");

        modelBuilder.Entity<EventModel>()
            .Property(e => e.RegisteredAt)
            .HasColumnType("date");
        
        
        modelBuilder.Entity<GuestModel>()
            .HasKey(g => g.GuestId);
        
        
        modelBuilder.Entity<VenueModel>()
            .HasKey(v => v.VenueId);
        
    }
}