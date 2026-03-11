using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new ServiceId(value));

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.DurationMinutes).IsRequired();
        builder.Property(s => s.Price).HasPrecision(18, 2);
        builder.Property(s => s.Description).HasColumnType("text");
        
        builder.HasMany(s => s.Providers)
            .WithMany(p => p.Services) // якщо в Provider є List<Service>
            .UsingEntity(j => j.ToTable("provider_services"));
    }
}