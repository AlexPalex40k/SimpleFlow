using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleFlow.Models;

namespace SimpleFlow.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(x => x.TaxNumber).IsUnique().HasFilter("[TaxNumber] IS NOT NULL");
    }
}
