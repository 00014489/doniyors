using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doniyors.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Doniyors.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
            
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<TypeUser> TypeUsers => Set<TypeUser>();
        public DbSet<Travel> Travels => Set<Travel>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<TravelImage> TravelImages => Set<TravelImage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= USER =================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.FirstName)
                    .HasMaxLength(100)
                    .IsRequired()
                    .HasDefaultValue(string.Empty);

                // Telegram CDN URLs are well under this; the cap is a sanity bound.
                entity.Property(x => x.PhotoUrl)
                    .HasMaxLength(512);

                entity.Property(x => x.QrToken)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasIndex(x => x.TgUserId).IsUnique();
                entity.HasIndex(x => x.QrToken).IsUnique();

                entity.Property(x => x.Points)
                    .HasDefaultValue(0);

                entity.Property(x => x.TypeUserId)
                    .HasDefaultValue(3);

                entity.Property(x => x.RegisteredAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Soft delete: disabled users stay in the table with Status = false.
                entity.Property(x => x.Status)
                    .HasDefaultValue(true);

                entity.HasOne(x => x.TypeUser)
                    .WithMany(x => x.Users)
                    .HasForeignKey(x => x.TypeUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= TYPE USER =================
            modelBuilder.Entity<TypeUser>(entity =>
            {
                entity.ToTable("TypeUsers");

                entity.HasKey(x => x.Id);

                // Name (role name)
                entity.Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                // Better than CURRENT_TIMESTAMP in PostgreSQL for consistency in migrations
                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Prevent duplicate roles
                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.HasData(
                    new TypeUser { Id = 1, Name = "SuperAdmin", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    new TypeUser { Id = 2, Name = "Admin", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    new TypeUser { Id = 3, Name = "User", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                );
            });

            // ================= TRAVEL =================
            modelBuilder.Entity<Travel>(entity =>
            {
                entity.ToTable("Travels");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .HasMaxLength(255)
                    .IsRequired();

                // Free text, so generous — but bounded, since it is rendered
                // straight into the adventure detail page.
                entity.Property(x => x.Description)
                    .HasMaxLength(4000)
                    .IsRequired()
                    .HasDefaultValue(string.Empty);

                entity.Property(x => x.Cost)
                    .HasColumnType("decimal(18,2)");

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Soft delete: disabled travels stay in the table with Status = false.
                entity.Property(x => x.Status)
                    .HasDefaultValue(true);
            });

             modelBuilder.Entity<TravelImage>(entity =>
            {
                entity.ToTable("TravelImages");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.ContentType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Data)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Cover image first.
                entity.HasIndex(x => new { x.TravelId, x.SortOrder });

                entity.HasOne(x => x.Travel)
                    .WithMany(x => x.Images)
                    .HasForeignKey(x => x.TravelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================= TRANSACTION =================
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Soft delete: voided transactions stay in the table with Status = false.
                entity.Property(x => x.Status)
                    .HasDefaultValue(true);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Optional: manual points adjustments have no adventure.
                entity.HasOne(t => t.Travel)
                    .WithMany(tr => tr.Transactions)
                    .HasForeignKey(t => t.TravelId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.ToTable("UserSessions");

                entity.HasKey(x => x.UserId);

                entity.Property(x => x.Step)
                    .HasConversion<string>()
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.DataJson)
                    .HasColumnType("jsonb")
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(x => x.User)
                    .WithOne(x => x.Session)
                    .HasForeignKey<UserSession>(x => x.UserId)
                    .HasPrincipalKey<User>(x => x.TgUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}