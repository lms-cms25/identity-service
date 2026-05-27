namespace IdentityService.Api.Dtos;

public record VerifyEmailVerificationCodeRequest(string Email, string Code);
