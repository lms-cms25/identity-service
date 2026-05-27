using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Data;

public static class DataConfiguration
{
    public static IServiceCollection AddDataConfiguration(this IServiceCollection services, IConfiguration configuration)
    {

        var connString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<DataContext>(options => options.UseSqlServer(connString, opt => opt.EnableRetryOnFailure()));
        
        

        return services;
    }
}
