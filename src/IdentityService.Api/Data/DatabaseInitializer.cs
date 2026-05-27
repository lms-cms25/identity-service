using Microsoft.EntityFrameworkCore;
using Polly;

namespace IdentityService.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(DataContext db)
    {
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 10,
                sleepDurationProvider: i => TimeSpan.FromSeconds(2)
            );

        await policy.ExecuteAsync(async () =>
        {
            await db.Database.MigrateAsync();
        });
    }
}