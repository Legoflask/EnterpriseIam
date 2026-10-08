using EnterpriseIam.Core.Entities;
using EnterpriseIam.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseIam.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ExtractedData> ExtractedData => Set<ExtractedData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(250);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(2000);

            entity.HasOne(u => u.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(u => u.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.Roles)
                  .WithMany(r => r.Users)
                  .UsingEntity(j => j.ToTable("UserRoles"));

            // Enforces multi-tenant row isolation across all standard database queries
            entity.HasQueryFilter(u => u.TenantId == _tenantProvider.TenantId);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FileName).IsRequired().HasMaxLength(500);

            // Auto-apply multi-tenant data isolation row filters!
            entity.HasQueryFilter(d => d.TenantId == _tenantProvider.TenantId);
        });

        modelBuilder.Entity<ExtractedData>(entity =>
        {
            entity.ToTable("ExtractedData");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RawText).IsRequired();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Ensure incoming document uploads catch the active client's tenant stamp automatically
        foreach (var entry in ChangeTracker.Entries<Document>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = _tenantProvider.TenantId;
                }
            }
        }

        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = _tenantProvider.TenantId;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}