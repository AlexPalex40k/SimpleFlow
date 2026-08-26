using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Models;

namespace SimpleFlow.Data;

/// <summary>
/// Контекст данных SimpleFlow и ASP.NET Core Identity.
/// </summary>
/// <param name="options">Параметры конфигурации контекста базы данных.</param>
public class SimpleFlowContext(DbContextOptions<SimpleFlowContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>
    /// Набор клиентов.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Набор товаров.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Набор строительных проектов.
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// Набор складов.
    /// </summary>
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    /// <summary>
    /// Набор складских остатков.
    /// </summary>
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    
    /// <summary>
    /// Набор складских документов.
    /// </summary>
    public DbSet<InventoryDocument> InventoryDocuments => Set<InventoryDocument>();

    /// <summary>
    /// Набор строк складских документов.
    /// </summary>
    public DbSet<InventoryDocumentLine> InventoryDocumentLines => Set<InventoryDocumentLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SimpleFlowContext).Assembly);
    }

    /// <summary>
    /// Асинхронно сохраняет изменения и автоматически заполняет даты аудита.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Количество записей, изменённых в базе данных.</returns>
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
