using IdentityService.Api.Data;
using IdentityService.Api.Endpoints;
using IdentityService.Api.Identity;
using IdentityService.Api.Security;
using Scalar.AspNetCore;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddCorsConfiguration(builder.Configuration);


builder.Services.AddDataConfiguration(builder.Configuration);
builder.Services.AddIdentityConfiguration(builder.Configuration);

var app = builder.Build();


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

