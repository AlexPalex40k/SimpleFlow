using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Models;

namespace SimpleFlow.Data;

public class SimpleFlowContext(DbContextOptions<SimpleFlowContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SimpleFlowContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAtUtc = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAtUtc = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
