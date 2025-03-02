using careerhive.domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace careerhive.infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Job> Jobs { get; set; }
    public DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }
    public DbSet<InvalidToken> InvalidTokens { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Job>()
            .HasOne(j => j.PostedBy)
            .WithMany(u => u.Jobs)
            .HasForeignKey(j => j.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<User>()
            .HasOne(u => u.Subscription)
            .WithOne(s => s.User)
            .HasForeignKey<UserSubscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserSubscription>()
            .HasIndex(s => s.UserId)
            .IsUnique();

        builder.Entity<UserSubscription>()
            .HasIndex(s => s.Email)
            .IsUnique();

        builder.Entity<Job>()
            .HasIndex(j => j.CreatedAt);
        
        builder.Entity<Job>()
            .HasIndex(j => j.Title);

        builder.Entity<Job>()
            .HasIndex(j => j.PostedByUserId);
    }
}
