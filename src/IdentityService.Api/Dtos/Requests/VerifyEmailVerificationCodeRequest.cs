namespace IdentityService.Api.Dtos.Requests;

public record VerifyEmailVerificationCodeRequest(string Email, string Code);
