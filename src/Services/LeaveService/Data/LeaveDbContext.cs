using Microsoft.EntityFrameworkCore;
using LeaveService.Models;
using Shared.Enums;


namespace LeaveService.Data;

public class LeaveDbContext : DbContext
{
    public LeaveDbContext(DbContextOptions<LeaveDbContext> options) : base(options) { }

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.ToTable("leave_types");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(x => x.Name)
                .IsRequired()
                .HasColumnType("varchar(100)");

            entity.Property(x => x.Description)
                .HasColumnType("varchar(500)");

            entity.Property(x => x.DefaultDays)
                .HasColumnType("integer");

            entity.Property(x => x.IsActive)
                .HasColumnType("boolean")
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("leave_requests");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(x => x.EmployeeId)
                .HasColumnType("uuid")
                .IsRequired();

            entity.Property(x => x.EmployeeName)
                .IsRequired()
                .HasColumnType("varchar(200)");

            entity.Property(x => x.EmployeeEmail)
                .IsRequired()
                .HasColumnType("varchar(255)");

            entity.Property(x => x.LeaveTypeId)
                .HasColumnType("uuid")
                .IsRequired();

            entity.Property(x => x.StartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EndDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.TotalDays)
                .HasColumnType("integer");

            entity.Property(x => x.Reason)
                .HasColumnType("varchar(1000)");

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasColumnType("varchar(50)")
                .HasDefaultValue(LeaveStatus.Pending);

            entity.Property(x => x.ReviewedBy)
                .HasColumnType("uuid");

            entity.Property(x => x.RejectionReason)
                .HasColumnType("varchar(1000)");

            entity.Property(x => x.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.Property(x => x.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.HasOne(x => x.LeaveType)
                .WithMany(lt => lt.LeaveRequests)
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveType>().HasData(
            new LeaveType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Annual Leave",
                Description = "Yearly paid leave entitlement",
                DefaultDays = 20,
                IsActive = true,
                CreatedAt = new DateTime(2026, 01, 01, 00, 00, 00, DateTimeKind.Utc)
            },
            new LeaveType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "Sick Leave",
                Description = "Medical and illness related leave",
                DefaultDays = 10,
                IsActive = true,
                CreatedAt = new DateTime(2026, 01, 01, 00, 00, 00, DateTimeKind.Utc)
            },
            new LeaveType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "Casual Leave",
                Description = "Short notice personal leave",
                DefaultDays = 5,
                IsActive = true,
                CreatedAt = new DateTime(2026, 01, 01, 00, 00, 00, DateTimeKind.Utc)
            },
            new LeaveType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Name = "Maternity Leave",
                Description = "Leave for maternity",
                DefaultDays = 90,
                IsActive = true,
                CreatedAt = new DateTime(2026, 01, 01, 00, 00, 00, DateTimeKind.Utc)
            },
            new LeaveType
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Name = "Paternity Leave",
                Description = "Leave for paternity",
                DefaultDays = 10,
                IsActive = true,
                CreatedAt = new DateTime(2026, 01, 01, 00, 00, 00, DateTimeKind.Utc)
            }
        );
    }
}
