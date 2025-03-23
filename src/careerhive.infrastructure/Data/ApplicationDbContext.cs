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
    public DbSet<SavedJob> SavedJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Job>()
            .HasOne(j => j.PostedBy)
            .WithMany(u => u.Jobs)
            .HasForeignKey(j => j.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Job>()
            .HasIndex(j => j.CreatedAt)
            .HasDatabaseName("IX_Jobs_CreatedAt");

        builder.Entity<Job>()
            .HasIndex(j => j.Title)
            .HasDatabaseName("IX_Jobs_Title");

        builder.Entity<Job>()
            .HasIndex(j => j.PostedByUserId)
            .HasDatabaseName("IX_Jobs_PostedByUserId");

        builder.Entity<SavedJob>()
            .HasKey(j => j.Id);

        builder.Entity<SavedJob>()
        .HasIndex(j => j.SavedByUserId)
        .HasDatabaseName("IX_SavedJobs_SavedByUserId");

        builder.Entity<SavedJob>()
        .HasIndex(j => j.JobId)
        .HasDatabaseName("IX_SavedJobs_JobId");

        builder.Entity<SavedJob>()
        .HasIndex(j => new { j.SavedByUserId, j.JobId })
        .HasDatabaseName("IX_SavedJobs_User_Job");

        builder.Entity<User>()
            .HasOne(u => u.Subscription)
            .WithOne(s => s.User)
            .HasForeignKey<UserSubscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserSubscription>()
            .HasIndex(s => s.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserSubscription_UserId");

        builder.Entity<UserSubscription>()
            .HasIndex(s => s.Email)
            .IsUnique()
            .HasDatabaseName("IX_UserSubscription_Email");

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_User_Email");

        builder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_User_UserName");
    }
}
