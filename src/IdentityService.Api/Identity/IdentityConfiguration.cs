using Azure.Messaging.ServiceBus;
using IdentityService.Api.Abstractions;
using IdentityService.Api.Data;
using IdentityService.Api.Security;
using IdentityService.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Api.Identity;

public static class IdentityConfiguration
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("AzureServiceBus");
            return new ServiceBusClient(connectionString);
        });

        services.AddDataProtection(); //eftersom .AddDefaultTokenProviders(); använder denna.


        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT Configuration is missing");

        services.AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders();


        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = [jwtOptions.Issuer],
                ValidateAudience = true,
                ValidAudiences = [jwtOptions.Audience],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwtOptions.GetSigningKey(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = JwtClaimTypes.Email,
                RoleClaimType = JwtClaimTypes.Role 
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        services.AddScoped<JwtTokenService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<RefreshTokenHasher>();
        services.AddScoped<IAuthService, AuthenticationService>();


        return services;
    }
}
