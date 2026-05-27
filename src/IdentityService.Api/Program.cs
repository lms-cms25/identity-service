using IdentityService.Api.Data;
using IdentityService.Api.Endpoints;
using IdentityService.Api.Identity;
using IdentityService.Api.Security;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Polly;
using IdentityService.Api.Services;
using IdentityService.Api.Abstractions;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddCorsConfiguration(builder.Configuration);



builder.Services.AddHttpClient<IVerificationService, VerificationService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Services:VerificationApi"]!);
});
builder.Services.AddDataConfiguration(builder.Configuration);
builder.Services.AddIdentityConfiguration(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//lägg till AzureServiceBus

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

    var policy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(20, i => TimeSpan.FromSeconds(3));

    await policy.ExecuteAsync(async () =>
    {
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await db.Database.MigrateAsync();
        });
    });

    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Documentation for Identity";
        options.Theme = ScalarTheme.Default;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    app.MapGet("/", () => Results.Redirect("/scalar"));
}

app.UseCors("Frontend");
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthenticationEndpoints();
app.MapAccountAdministrationEndpoints();

//app.MapGet("/api/test-auth", (ClaimsPrincipal user) =>
//{
//    return Results.Ok(user.Claims.Select(c => new { c.Type, c.Value }));
//}).RequireAuthorization();




app.Run();

