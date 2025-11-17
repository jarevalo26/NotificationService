using Microsoft.EntityFrameworkCore;
using NotificationService.API.Domain.Configurations;
using NotificationService.API.Domain.Entities;

namespace NotificationService.API.Infrastructure.Persistences;

public class NotificationsDbContext: DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
    }
}