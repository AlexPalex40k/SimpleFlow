using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleFlow.Models;

namespace SimpleFlow.Data.Configurations;

/// <summary>
/// Конфигурация Entity Framework Core для проектов.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <summary>
    /// Правила хранения проекта и связи с клиентом.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasIndex(project => project.ProjectNumber).IsUnique();

        builder.Property(project => project.StartDate).HasColumnType("date");
        builder.Property(project => project.PlannedEndDate).HasColumnType("date");
        builder.Property(project => project.ActualEndDate).HasColumnType("date");

        builder
            .HasOne(project => project.Customer)
            .WithMany(customer => customer.Projects)
            .HasForeignKey(project => project.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
