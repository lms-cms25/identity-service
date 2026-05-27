namespace IdentityService.Api.Contracts.Authentication;

public enum AuthenticationStage
{
    EmailPending,
    EmailVerified,
    ProfileIncomplete,
    Authenticated
}
