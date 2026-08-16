using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleFlow.Models;

namespace SimpleFlow.Data.Configurations;

/// <summary>
/// Конфигурация Entity Framework Core для клиентов.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <summary>
    /// Настраивает модель и индексы клиента.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(x => x.TaxNumber).IsUnique().HasFilter("[TaxNumber] IS NOT NULL");
    }
}
