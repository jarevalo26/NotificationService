using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.API.Domain.Entities;

namespace NotificationService.API.Domain.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Table name
        builder.ToTable("Notifications");

        // Primary key
        builder.HasKey(n => n.Id);

        // Properties
        builder.Property(n => n.RecipientEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.RecipientName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Subject)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(n => n.HtmlBody)
            .IsRequired();

        builder.Property(n => n.TextBody)
            .IsRequired(false);

        builder.Property(n => n.NotificationType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(n => n.SentAt)
            .IsRequired(false);

        builder.Property(n => n.SendAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(n => n.EventId)
            .IsRequired();

        builder.Property(n => n.ParticipantId)
            .IsRequired();

        builder.Property(n => n.SendGridMessageId)
            .HasMaxLength(100)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(n => n.RecipientEmail)
            .HasDatabaseName("IX_Notifications_RecipientEmail");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("IX_Notifications_Status");

        builder.HasIndex(n => new { n.CreatedAt, n.Status })
            .HasDatabaseName("IX_Notifications_CreatedAt_Status");
    }
}