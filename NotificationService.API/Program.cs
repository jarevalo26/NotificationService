using Microsoft.EntityFrameworkCore;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Infrastructure.BackgroundServices;
using NotificationService.API.Infrastructure.Configurations;
using NotificationService.API.Infrastructure.Persistences;
using NotificationService.API.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ---------------------------------------
// Swagger / OpenAPI
// ---------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Notifications API", 
        Version = "v1",
        Description = "API para gestión de notificaciones y envío de emails"
    });
});

// ---------------------------------------
// Configuración de SQL Server
// ---------------------------------------
var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection");
builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------------------------------------
// Configuración de los Setings
// ---------------------------------------
builder.Services.Configure<SendGridSettings>(
    builder.Configuration.GetSection("SendGrid"));

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// ---------------------------------------
// Configuración de DI para Application Services
// ---------------------------------------
builder.Services.AddScoped<ISendNotificationService, SendNotificationService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<ISendGridEmailService, SendGridEmailService>();
builder.Services.AddHostedService<RetryNotificationsService>();

// ---------------------------------------
// CORS
// ---------------------------------------
var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(corsOrigins ?? [])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ---------------------------------------
// Migraciones automáticas + Seed de datos
// ---------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<NotificationsDbContext>();
        context.Database.Migrate();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notifications API v1");
        c.RoutePrefix = string.Empty;
    });  
}

app.UseHttpsRedirection();
app.UseCors("AllowClient");
app.UseAuthorization();
app.MapControllers();

app.Run();