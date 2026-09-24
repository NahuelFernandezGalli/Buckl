using Buckl.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Persistence;

/// <summary>EF Core session over the Buckl schema. It maps persistence records, never domain
/// aggregates (ADR-0023); repositories convert between the two.</summary>
public sealed class BucklDbContext : DbContext
{
    public BucklDbContext(DbContextOptions<BucklDbContext> options)
        : base(options)
    {
    }

    public DbSet<GarmentRecord> Garments => Set<GarmentRecord>();

    public DbSet<ProductRecord> Products => Set<ProductRecord>();

    public DbSet<UserRecord> Users => Set<UserRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BucklDbContext).Assembly);
    }
}
