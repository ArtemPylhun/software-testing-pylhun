using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");
        builder.HasKey(p => p.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new ProviderId(value));

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Specialization).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Specialization);

        builder.Property(p => p.Email).IsRequired().HasMaxLength(255);

        builder.Property(p => p.StartWorkingHours).IsRequired();
        builder.Property(p => p.EndWorkingHours).IsRequired();

        builder.Metadata.FindNavigation(nameof(Provider.Appointments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}