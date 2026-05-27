namespace IdentityService.Api.Dtos.Requests;

public sealed record CompleteProfileRequest(string Email, string FirstName, string LastName, string Password, string ComparePassword, bool AcceptTerms);
