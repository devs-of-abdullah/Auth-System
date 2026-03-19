using Business;
using Business.Interfaces;
using Business.Interfaces;
using Business.Services;
using Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
         Environment.GetEnvironmentVariable("DefaultConnection") 
         ?? configuration.GetConnectionString("DefaultConnection")
         ?? throw new Exception("Database connection string not found.");

        services.AddHttpContextAccessor();

        services.AddDbContext<AppDbContext>(options => {
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Data"));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService,TokenService>();
        services.AddTransient<IEmailService, EmailService>();

        services.AddControllers()
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                x.JsonSerializerOptions.WriteIndented = true;
            });

        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

            options.AddPolicy("RestrictedCors", policy =>
            {
                var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else if (!isProduction)
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else
                {
                    throw new InvalidOperationException("AllowedOrigins must be configured in Production environment.");
                }
            });
        });

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        return services;
    }
}
