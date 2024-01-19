using System.Security.Claims;

public class RoleAuthorizationService : IRoleAuthorization
{
    public ClaimsPrincipal Principal { get; set; }
    private readonly IHashService _hashService;
    private readonly string _fetchedHashPrincipal;

    public RoleAuthorizationService(IHashService hashService)
    {
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
    }

    public async Task<bool> ValidatePrincipal(ClaimsPrincipal principal)
    {
        // Here you would hash the principal and compare it to the stored hash.
        // For this example, let's assume _fetchedHashPrincipal is retrieved from the database.
        var principalHash = await _hashService.HashValueAsync(principal.Identity.Name);
        return principalHash == _fetchedHashPrincipal;
    }

    public bool IsAuthorized(HashSet<string> authRoles)
    {
        if (Principal == null)
        {
            throw new InvalidOperationException("Principal has not been set.");
        }

        // Check if the principal has any of the roles required to access the feature.
        foreach (var role in authRoles)
        {
            if (Principal.IsInRole(role))
            {
                return true; // The user is authorized
            }
        }

        // If none of the roles match, the user is not authorized
        return false;
    }

    // Additional methods such as GetHashPrincipal would be defined here
}
