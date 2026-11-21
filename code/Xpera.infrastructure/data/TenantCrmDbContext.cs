using Xpera.domain.entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xpera.infrastructure.data
{
    // One store's database (a tenant): Xpera_TenantA, Xpera_TenantB, Xpera_TenantC.
    // Every store database has the same tables; CompanyDatabases in the Master says which database is whose.
    public class TenantCrmDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<Sale> Sales => Set<Sale>();

        public DbSet<SaleItem> SaleItems => Set<SaleItem>();

        public DbSet<Payment> Payments => Set<Payment>();

        public DbSet<Inquiry> Inquiries => Set<Inquiry>();

        public DbSet<Complaint> Complaints => Set<Complaint>();

        public DbSet<Feedback> Feedback => Set<Feedback>();

        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        public TenantCrmDbContext(DbContextOptions<TenantCrmDbContext> options) : base(options) { }

        // ---------- cloud sync flags ----------

        // Set to true for a save that should NOT count as "not yet in the cloud", like the failed-login counter
        // going up and back to 0 at every sign-in (that is not store data a person entered).
        public bool KeepSyncFlags { get; set; }

        // Every save goes through here (the app, the API, the minimal APIs), so no screen can forget:
        // any row that is new or changed in a table with IsSynced becomes "not yet synced".
        // Only the cloud backup sets it back to true (CloudBackup, with plain SQL after the copy).
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            MarkChangesNotSynced();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            MarkChangesNotSynced();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void MarkChangesNotSynced()
        {
            if (KeepSyncFlags) return;
            foreach (var entry in ChangeTracker.Entries<ISyncTracked>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    entry.Entity.IsSynced = false;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // "Today" in SQL Server, used as the default for every date column.
            const string today = "CAST(GETDATE() AS DATE)";

            // UC-01 staff accounts
            builder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.UserId);

                entity.Property(x => x.FullName)
                    .HasMaxLength(80)
                    .IsRequired();

                entity.Property(x => x.Username)
                    .HasMaxLength(40)
                    .IsRequired();

                entity.HasIndex(x => x.Username)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(32)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Active");

                entity.Property(x => x.FailedLogins)
                    .HasDefaultValue(0);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Users_Role", "Role IN (N'Admin', N'Cashier', N'Staff')");
                    t.HasCheckConstraint("CK_Users_Status", "Status IN (N'Active', N'Locked')");
                });
            });

            // UC-02 products (no stock counts, only available or not)
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.ProductId);

                entity.Property(x => x.Name)
                    .HasMaxLength(80)
                    .IsRequired();

                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.Property(x => x.Category)
                    .HasMaxLength(40)
                    .IsRequired();

                entity.Property(x => x.Price)
                    .HasPrecision(10, 2);

                entity.Property(x => x.IsAvailable)
                    .HasDefaultValue(true)
                    .HasSentinel(false);

                entity.ToTable(t => t.HasCheckConstraint("CK_Products_Price", "Price >= 0"));
            });

            // UC-03 customers
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(x => x.CustomerId);

                entity.Property(x => x.FullName)
                    .HasMaxLength(80)
                    .IsRequired();

                entity.Property(x => x.Contact)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.UsuallyBuys)
                    .HasMaxLength(80)
                    .HasDefaultValue("");

                entity.Property(x => x.CreatedOn)
                    .HasColumnType("date")
                    .HasDefaultValueSql(today);
            });

            // UC-04 sales and the products in each sale
            builder.Entity<Sale>(entity =>
            {
                entity.HasKey(x => x.SaleId);

                entity.Property(x => x.ReceiptNo)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(x => x.ReceiptNo)
                    .IsUnique();

                entity.Property(x => x.SaleDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql(today);

                entity.Property(x => x.PaymentType)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.Total)
                    .HasPrecision(10, 2);

                entity.Property(x => x.DueDate)
                    .HasColumnType("date");

                entity.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.CashierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(x => x.Items)
                      .WithOne()
                      .HasForeignKey(x => x.SaleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Sales_PaymentType", "PaymentType IN (N'Cash', N'Credit')");
                    t.HasCheckConstraint("CK_Sales_Total", "Total >= 0");
                });
            });

            builder.Entity<SaleItem>(entity =>
            {
                entity.HasKey(x => x.SaleItemId);

                entity.Property(x => x.UnitPrice)
                    .HasPrecision(10, 2);

                entity.HasOne<Product>()
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("CK_SaleItems_Quantity", "Quantity > 0"));
            });

            // UC-05 payments toward what a customer owes
            builder.Entity<Payment>(entity =>
            {
                entity.HasKey(x => x.PaymentId);

                entity.Property(x => x.Amount)
                    .HasPrecision(10, 2);

                entity.Property(x => x.PaidOn)
                    .HasColumnType("date")
                    .HasDefaultValueSql(today);

                entity.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.CashierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("CK_Payments_Amount", "Amount > 0"));
            });

            // UC-06 inquiries (one about a product that is not available waits for it)
            builder.Entity<Inquiry>(entity =>
            {
                entity.HasKey(x => x.InquiryId);

                entity.Property(x => x.InquiryDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql(today);

                entity.Property(x => x.Question)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(30)
                    .HasDefaultValue("Open");

                entity.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Product>()
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("CK_Inquiries_Status",
                    "Status IN (N'Open', N'In progress', N'Waiting for product', N'Resolved')"));
            });

            // UC-07 complaints and warranty claims
            builder.Entity<Complaint>(entity =>
            {
                entity.HasKey(x => x.ComplaintId);

                entity.Property(x => x.ComplaintDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql(today);

                entity.Property(x => x.Details)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Type)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Open");

                entity.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Complaints_Type", "Type IN (N'Complaint', N'Warranty claim')");
                    t.HasCheckConstraint("CK_Complaints_Status", "Status IN (N'Open', N'In progress', N'Resolved')");
                });
            });

            // UC-08 feedback (1 to 5 stars and a comment)
            builder.Entity<Feedback>(entity =>
            {
                entity.HasKey(x => x.FeedbackId);

                entity.Property(x => x.FeedbackDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql(today);

                entity.Property(x => x.Comment)
                    .HasMaxLength(300)
                    .HasDefaultValue("");

                entity.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("CK_Feedback_Rating", "Rating BETWEEN 1 AND 5"));
            });

            // The Activity log: one line per thing done in the store (not a use case of its own; it shows
            // which records are already in the cloud). TableName + RecordId point at the record it is about.
            builder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(x => x.ActivityLogId);

                entity.Property(x => x.LoggedAt)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(x => x.Action)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.TableName)
                    .HasMaxLength(40);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.TableName, x.RecordId });
            });

            // IsSynced on the 8 tables with records of their own (see ISyncTracked; SaleItems has none).
            // New rows start as false: "not in the cloud yet". Rows from the seed scripts too.
            foreach (var table in builder.Model.GetEntityTypes().Where(e => typeof(ISyncTracked).IsAssignableFrom(e.ClrType)).ToList())
            {
                builder.Entity(table.ClrType).Property(nameof(ISyncTracked.IsSynced))
                    .HasDefaultValue(false);
            }
        }
    }
}
