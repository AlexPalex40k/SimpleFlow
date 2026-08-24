using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleFlow.Models;

namespace SimpleFlow.Data.Configurations;

/// <summary>
/// Конфигурация Entity Framework Core для складских остатков.
/// </summary>
public class InventoryBalanceConfiguration : IEntityTypeConfiguration<InventoryBalance>
{
    /// <summary>
    /// Правила хранения складского остатка.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder
            .HasIndex(balance => new
            {
                balance.ProductId,
                balance.WarehouseId
            })
            .IsUnique();

        builder
            .HasOne(balance => balance.Product)
            .WithMany(product => product.InventoryBalances)
            .HasForeignKey(balance => balance.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(balance => balance.Warehouse)
            .WithMany(warehouse => warehouse.InventoryBalances)
            .HasForeignKey(balance => balance.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(balance => balance.RowVersion)
            .IsRowVersion();

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_InventoryBalances_QuantityOnHand",
                "[QuantityOnHand] >= 0");

            table.HasCheckConstraint(
                "CK_InventoryBalances_QuantityReserved",
                "[QuantityReserved] >= 0");

            table.HasCheckConstraint(
                "CK_InventoryBalances_ReservedNotGreaterThanOnHand",
                "[QuantityReserved] <= [QuantityOnHand]");

            table.HasCheckConstraint(
                "CK_InventoryBalances_AverageCost",
                "[AverageCost] >= 0");
        });
    }
}