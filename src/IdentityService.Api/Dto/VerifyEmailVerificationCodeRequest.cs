namespace IdentityService.Api.Dto;

public record VerifyEmailVerificationCodeRequest(string Email, string Code);
