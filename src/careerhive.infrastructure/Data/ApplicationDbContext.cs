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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Job>()
            .HasOne(j => j.PostedBy)
            .WithMany(u => u.Jobs)
            .HasForeignKey(j => j.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
