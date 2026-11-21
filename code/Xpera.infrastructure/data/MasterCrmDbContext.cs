using Xpera.domain.entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xpera.infrastructure.data
{
    public class MasterCrmDbContext : IdentityDbContext
    {
        public DbSet<Company> Companies => Set<Company>();

        public DbSet<CompanyDatabase> CompanyDatabases => Set<CompanyDatabase>();

        public DbSet<Plan> Plans => Set<Plan>();

        public DbSet<Subscription> Subscriptions => Set<Subscription>();

        public DbSet<Device> Devices { get; set; }

        // Which store each username belongs to, so sign-in needs no store code.
        public DbSet<CompanyUser> CompanyUsers => Set<CompanyUser>();

        public MasterCrmDbContext(DbContextOptions<MasterCrmDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Company>(entity =>
            {
                entity.HasKey(x => x.CompanyId);

                entity.Property(x => x.CompanyCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.CompanyName)
                    .HasMaxLength(200)
                    .IsRequired();
                
                entity.HasIndex(x => x.CompanyCode)
                    .IsUnique();

            });

            builder.Entity<CompanyDatabase>(entity =>
            {
                entity.HasKey(x => x.CompanyDatabaseId);

                entity.Property(x => x.ServerName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.DatabaseName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasOne(x => x.Company)
                      .WithMany()
                      .HasForeignKey(x => x.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Device>(entity =>
            {
                entity.HasKey(x => x.DeviceId);

                entity.Property(x => x.DeviceCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DeviceName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasOne(x => x.Company)
                      .WithMany(x => x.Devices)
                      .HasForeignKey(x => x.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.CompanyId, x.DeviceCode })
                    .IsUnique();
            });

            builder.Entity<CompanyUser>(entity =>
            {
                entity.HasKey(x => x.CompanyUserId);

                entity.Property(x => x.Username)
                    .HasMaxLength(40)
                    .IsRequired();

                // One username = one person in all of xpera.
                entity.HasIndex(x => x.Username)
                    .IsUnique();

                entity.HasOne(x => x.Company)
                      .WithMany()
                      .HasForeignKey(x => x.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Plan>(entity =>
            {
                entity.HasKey(x => x.PlanCode);

                entity.Property(x => x.PlanCode)
                    .HasMaxLength(1);

                entity.Property(x => x.PlanName)
                    .HasMaxLength(40)
                    .IsRequired();

                entity.Property(x => x.MonthlyFee)
                    .HasPrecision(10, 2);

                entity.Property(x => x.Includes)
                    .HasMaxLength(200)
                    .IsRequired();
            });

            builder.Entity<Subscription>(entity =>
            {
                entity.HasKey(x => x.SubscriptionId);

                entity.Property(x => x.MonthlyFee)
                    .HasPrecision(10, 2);

                entity.Property(x => x.StartedOn)
                    .HasColumnType("date");

                entity.Property(x => x.RenewsOn)
                    .HasColumnType("date");

                entity.HasOne(x => x.Company)
                      .WithMany()
                      .HasForeignKey(x => x.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Plan)
                      .WithMany()
                      .HasForeignKey(x => x.PlanCode)
                      .OnDelete(DeleteBehavior.Restrict);
            });


        }
    }
}
