using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ArabamTR.Models;
using System;

namespace ArabamTR.Data
{
    // Standart DbContext yerine IdentityDbContext entegre ederek veritabanı güvenliğini sağlıyoruz
    public class ArabamTRDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ArabamTRDbContext(DbContextOptions<ArabamTRDbContext> options) : base(options)
        {
        }

        // IdentityDbContext zaten Users tablosunu (AspNetUsers) otomatik yönettiği için ayrıyeten DbSet<User> eklemiyoruz.
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<CarFeature> CarFeatures { get; set; }
        public DbSet<VehicleFeature> VehicleFeatures { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<FakeHistory> FakeHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure VehicleFeature Composite Key
            modelBuilder.Entity<VehicleFeature>()
                .HasKey(vf => new { vf.VehicleId, vf.FeatureId });

            // Configure VehicleFeature Many-to-Many Relationships
            modelBuilder.Entity<VehicleFeature>()
                .HasOne(vf => vf.Vehicle)
                .WithMany(v => v.VehicleFeatures)
                .HasForeignKey(vf => vf.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VehicleFeature>()
                .HasOne(vf => vf.Feature)
                .WithMany(f => f.VehicleFeatures)
                .HasForeignKey(vf => vf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Message double relationship
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Data: Brands
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Volkswagen" },
                new Brand { Id = 2, Name = "BMW" },
                new Brand { Id = 3, Name = "Toyota" }
            );

            // Seed Data: Models
            modelBuilder.Entity<Model>().HasData(
                new Model { Id = 1, Name = "Golf", BrandId = 1 },
                new Model { Id = 2, Name = "Passat", BrandId = 1 },
                new Model { Id = 3, Name = "3 Series", BrandId = 2 },
                new Model { Id = 4, Name = "5 Series", BrandId = 2 },
                new Model { Id = 5, Name = "Corolla", BrandId = 3 },
                new Model { Id = 6, Name = "Yaris", BrandId = 3 }
            );

            // Seed Data: CarFeatures
            modelBuilder.Entity<CarFeature>().HasData(
                new CarFeature { Id = 1, FeatureName = "Sunroof", FeatureType = "Comfort" },
                new CarFeature { Id = 2, FeatureName = "Leather Seats", FeatureType = "Comfort" },
                new CarFeature { Id = 3, FeatureName = "Lane Assist", FeatureType = "Safety" },
                new CarFeature { Id = 4, FeatureName = "Adaptive Cruise Control", FeatureType = "Safety" }
            );

            // Seed Data: FakeHistory for plate inquiries
            modelBuilder.Entity<FakeHistory>().HasData(
                new FakeHistory
                {
                    Id = 1,
                    PlateNumber = "34ABC123",
                    HasDamageRecord = false,
                    DamageAmount = 0.00m,
                    LastKM = 45000,
                    KmHistoryJson = "[{\"Date\":\"2024-01-10\",\"KM\":15000},{\"Date\":\"2025-01-12\",\"KM\":30000},{\"Date\":\"2026-01-15\",\"KM\":45000}]"
                },
                new FakeHistory
                {
                    Id = 2,
                    PlateNumber = "06XYZ999",
                    HasDamageRecord = true,
                    DamageAmount = 120000.00m,
                    LastKM = 185000,
                    KmHistoryJson = "[{\"Date\":\"2022-03-05\",\"KM\":60000},{\"Date\":\"2024-03-08\",\"KM\":120000},{\"Date\":\"2026-03-10\",\"KM\":185000}]"
                }
            );

            // Microsoft Identity Standartlarına Göre Seed Users Yapılandırması
            // Şifreler Identity standartlarında otomatik türetilmiştir. (Password123!)
            var hasher = new PasswordHasher<User>();

            var user1 = new User
            {
                Id = 1,
                UserName = "ahmet@test.com",
                NormalizedUserName = "AHMET@TEST.COM",
                Email = "ahmet@test.com",
                NormalizedEmail = "AHMET@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Ahmet Yilmaz",
                IsEmailConfirmed = true, // Eski property uyumluluğu için tutulmuştur
                Is2FAEnabled = false,
                AccountStatus = "Active",
                Role = "Admin",
                LastActiveDate = new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc)
            };
            user1.PasswordHash = hasher.HashPassword(user1, "Password123!");

            var user2 = new User
            {
                Id = 2,
                UserName = "mehmet@test.com",
                NormalizedUserName = "MEHMET@TEST.COM",
                Email = "mehmet@test.com",
                NormalizedEmail = "MEHMET@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Mehmet Demir",
                IsEmailConfirmed = true,
                Is2FAEnabled = true,
                AccountStatus = "Active",
                Role = "User",
                LastActiveDate = new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc)
            };
            user2.PasswordHash = hasher.HashPassword(user2, "Password123!");

            modelBuilder.Entity<User>().HasData(user1, user2);

            // Seed Vehicles
            modelBuilder.Entity<Vehicle>().HasData(
                new Vehicle
                {
                    Id = 1,
                    Title = "Sahibinden Temiz Golf 8",
                    Description = "Kazası boyası trameri yoktur. Yetkili servis bakımlıdır.",
                    Price = 1250000.00m,
                    KM = 35000,
                    Year = 2022,
                    PlateNumber = "34ABC123",
                    ChassisNumber = "WVWZZZ1K123456789",
                    EngineNumber = "EA211987654",
                    CreatedDate = new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    UserId = 1,
                    ModelId = 1,
                    ImageUrl = "default-car.png" // Başlangıç testi için varsayılan bir görsel adı atadık
                }
            );

            // Seed VehicleFeatures (Link Golf 8 to Sunroof and Adaptive Cruise Control)
            modelBuilder.Entity<VehicleFeature>().HasData(
                new VehicleFeature { VehicleId = 1, FeatureId = 1 },
                new VehicleFeature { VehicleId = 1, FeatureId = 4 }
            );
        }
    }
}
