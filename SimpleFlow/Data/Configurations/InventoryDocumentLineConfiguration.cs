using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleFlow.Models;

namespace SimpleFlow.Data.Configurations;

/// <summary>
/// Конфигурация Entity Framework Core для строк складских документов.
/// </summary>
public class InventoryDocumentLineConfiguration
    : IEntityTypeConfiguration<InventoryDocumentLine>
{
    /// <summary>
    /// Правила хранения строки складского документа.
    /// </summary>
    /// <param name="builder">
    /// Построитель конфигурации сущности.
    /// </param>
    public void Configure(
        EntityTypeBuilder<InventoryDocumentLine> builder)
    {
        builder
            .HasOne(line => line.InventoryDocument)
            .WithMany(document => document.Lines)
            .HasForeignKey(line => line.InventoryDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(line => line.Product)
            .WithMany()
            .HasForeignKey(line => line.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_InventoryDocumentLines_Quantity",
                "[Quantity] > 0");

            table.HasCheckConstraint(
                "CK_InventoryDocumentLines_UnitCost",
                "[UnitCost] >= 0");
        });
    }
}