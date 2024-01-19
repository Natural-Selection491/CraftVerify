public class DeauthenticationService : IDeauthentication
{
    private readonly IDeauthenticationDBService _deauthenticateDBService;

    public DeauthenticationService(IDeauthenticationDBService deauthenticateDBService)
    {
        _deauthenticateDBService = deauthenticateDBService ?? throw new ArgumentNullException(nameof(deauthenticateDBService));
    }

    public async Task<bool> DeauthenticateAsync(string userIdentity)
    {
        // Deauthenticate deletes the ClaimsPrincipalHash where UserIdentity matches
        return await _deauthenticateDBService.DeleteClaimsPrincipalHashAsync(userIdentity);
    }
}