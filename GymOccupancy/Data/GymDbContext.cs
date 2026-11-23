using GymOccupancy.Models;
using Microsoft.EntityFrameworkCore;

namespace GymOccupancy.Data;

public class GymDbContext : DbContext
{
    public GymDbContext(DbContextOptions<GymDbContext> options): base(options)
    {
    }

    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<VisitEvent> VisitEvents => Set<VisitEvent>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Club>().HasKey(club => club.ClubId);
        modelBuilder.Entity<VisitEvent>().HasKey(visit => visit.EventId);

        modelBuilder.Entity<UserAccount>().Property(user => user.Username).HasMaxLength(100);
        modelBuilder.Entity<UserAccount>().HasIndex(user => user.Username).IsUnique();

    }
}