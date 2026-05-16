namespace IdentityService.Api.Dtos;

public record CheckUserStatusRequest(string Email);

public record CheckUserStatusResult(bool Succeeded, bool IsVerified, string? Email = null, string? Error = null)
{
    public static CheckUserStatusResult Verified(string email) => new(true, true, email, null);

    public static CheckUserStatusResult NotVerified(string email, string errorMessage) => new(true, false, email, errorMessage);
    public static CheckUserStatusResult NotFound(string email, string errorMessage) => new(false, false, email, errorMessage);

}
