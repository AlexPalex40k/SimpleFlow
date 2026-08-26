using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleFlow.Models;

namespace SimpleFlow.Data.Configurations;

/// <summary>
/// Конфигурация Entity Framework Core для складских документов.
/// </summary>
public class InventoryDocumentConfiguration
    : IEntityTypeConfiguration<InventoryDocument>
{
    /// <summary>
    /// Правила хранения складского документа.
    /// </summary>
    /// <param name="builder">
    /// Построитель конфигурации сущности.
    /// </param>
    public void Configure(
        EntityTypeBuilder<InventoryDocument> builder)
    {
        builder
            .HasIndex(document => document.DocumentNumber)
            .IsUnique();

        builder
            .Property(document => document.DocumentDate)
            .HasColumnType("date");

        builder
            .HasOne(document => document.SourceWarehouse)
            .WithMany()
            .HasForeignKey(document => document.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(document => document.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(document => document.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}