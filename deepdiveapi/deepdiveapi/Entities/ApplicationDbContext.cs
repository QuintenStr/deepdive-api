using deepdiveapi.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace deepdiveapi.Entities
{
    /// <summary>
    /// Represents the database context of the application, integrating Identity framework functionality.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationDbContext"/> with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by a DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet for Refresh Tokens.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Identity Roles.
        /// </summary>
        public DbSet<IdentityRole> IdentityRoles { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for User Register Documents.
        /// </summary>
        public DbSet<UserRegisterDocument> UserRegisterDocuments { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Registration Requests.
        /// </summary>
        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Password Resets.
        /// </summary>
        public DbSet<PasswordReset> PasswordResets { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Excursions.
        /// </summary>
        public DbSet<Excursion> Excursions { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Excursion Participants.
        /// </summary>
        public DbSet<ExcursionParticipant> ExcursionParticipants { get; set; }

        /// <summary>
        /// Configures the schema needed for the identity framework and sets up entity relationships and properties.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Custom configurations and filters
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

            // Soft delete fix
            modelBuilder.Entity<User>()
                .Property(u => u.BirthDate)
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v));

            // Configurations for entity relationships
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(rt => rt.RefreshTokens)
                .HasForeignKey(rt => rt.UserIdFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRegisterDocument>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserRegisterDocuments)
                .HasForeignKey(ud => ud.UserIdFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistrationRequest>()
                .HasOne(rr => rr.User)
                .WithOne(u => u.RegistrationRequest)
                .HasForeignKey<RegistrationRequest>(rr => rr.UserIdFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordReset>()
                .HasOne(pwd => pwd.User)
                .WithMany(u => u.PasswordResets)
                .HasForeignKey(pwd => pwd.UserIdFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Excursion>()
                .HasOne(rt => rt.CreatedByUser)
                .WithMany(rt => rt.Excursions)
                .HasForeignKey(rt => rt.CreatedByUserFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExcursionParticipant>()
                .HasOne(ep => ep.Excursion)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.ExcursionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExcursionParticipant>()
                .HasOne(ep => ep.User)
                .WithMany(u => u.ParticipatingInExcursions)
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<ExcursionParticipant>()
                .HasIndex(ep => new { ep.ExcursionId, ep.UserId })
                .IsUnique();
        }
    }
}