using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.DAL
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

                entity.Property(x => x.Cost)
                    .HasColumnType("decimal(18,2)");

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ================= TRANSACTION =================
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Travel)
                    .WithMany(tr => tr.Transactions)
                    .HasForeignKey(t => t.TravelId)
                    .OnDelete(DeleteBehavior.Cascade);
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