namespace IdentityService.Api.Contracts.Auth;

public enum AuthStage
{
    EmailPending,
    AwaitingProfileCompletion,
    Authenticated
}
