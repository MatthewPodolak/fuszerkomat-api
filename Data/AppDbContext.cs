using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Data.Models.Token;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; } = null!;
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Opinion> Opinions => Set<Opinion>();
        public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
        public DbSet<TaskApplication> TaskApplications => Set<TaskApplication>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
               .HasOne(a => a.UserProfile)
               .WithOne(a => a.AppUser)
               .HasForeignKey<UserProfile>(s => s.AppUserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
               .HasOne(a => a.CompanyProfile)
               .WithOne(a => a.AppUser)
               .HasForeignKey<CompanyProfile>(s => s.AppUserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyProfile>()
                .HasOne(c => c.Address)
                .WithOne(a => a.CompanyProfile)
                .HasForeignKey<Address>(a => a.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.CompanyUser)
                .WithMany(u => u.Opinions)
                .HasForeignKey(o => o.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.AuthorUser)
                .WithMany()
                .HasForeignKey(o => o.AuthorUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkTask>()
                .HasMany(a => a.Tags)
                .WithMany(a => a.WorkTasks)
                .UsingEntity(j => j.ToTable("WotkTaskTags"));

            modelBuilder.Entity<WorkTask>()
                .HasOne(w => w.CreatedByUser)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(w => w.CreatedByUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskApplication>()
                .HasOne(a => a.WorkTask)
                .WithMany(t => t.Applications)
                .HasForeignKey(a => a.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskApplication>()
                .HasOne(a => a.CompanyUser)
                .WithMany(u => u.CompanyApplications)
                .HasForeignKey(a => a.CompanyUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.UserId);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.CreatedAtUtc);



            modelBuilder.Entity<TaskApplication>()
                .HasIndex(a => new { a.WorkTaskId, a.CompanyUserId })
                .IsUnique();

            modelBuilder.Entity<Opinion>()
                .HasIndex(o => new { o.AuthorUserId, o.CompanyId })
                .IsUnique();

            modelBuilder.Entity<CompanyProfile>()
                .HasIndex(o => new { o.Email })
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasIndex(o => new { o.Email })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
