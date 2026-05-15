using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Data;

public static class DataConfiguration
{
    public static IServiceCollection AddDataConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
