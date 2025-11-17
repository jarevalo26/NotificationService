using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.API.Domain.Entities;

namespace NotificationService.API.Domain.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        // Table name
        builder.ToTable("Templates");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.NotificationType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.SubjectTemplate)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.HtmlBodyTemplate)
            .IsRequired();

        builder.Property(t => t.TextBodyTemplate)
            .IsRequired(false);

        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Templates_Name");

        builder.HasIndex(t => t.NotificationType)
            .HasDatabaseName("IX_Templates_Type");
    }
}