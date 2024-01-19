using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class RoleAuthorizationService
{
    public bool HasError { get; private set; }
    public bool CanRetry { get; private set; }
    public ClaimsPrincipal Principal? { get; private set; }
    public string FetchedHashPrincipal { get; private set; }

    private readonly ValidationService _validationService;

    public RoleAuthorizationService(ValidationService validationService)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    // Method to validate the principal's hash with the fetched hash from the database
    public async Task<bool> ValidatePrincipalAsync(ClaimsPrincipal principal)
    {
        Principal = principal ?? throw new ArgumentNullException(nameof(principal));
        try
        {
            FetchedHashPrincipal = await _validationService.HashedOTPFetchingAsync(Principal.Identity.Name);
            if (FetchedHashPrincipal == null)
            {
                _logger.LogError("No hash found for the provided principal identity.");
                HasError = true;
                return false;
            }

            // Simulate the hash of the principal and compare it with the fetched hash
            var principalHash = _validationService.HashValue(Principal.Identity.Name);
            if (principalHash == FetchedHashPrincipal)
            {
                _logger.LogInformation("Principal is valid.");
                return true;
            }
            else
            {
                _logger.LogWarning("Principal is not valid.");
                HasError = true;
                CanRetry = true; // Assuming a retry is possible in case of failure
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while validating the principal.");
            HasError = true;
            CanRetry = false; // Assuming no retry on exception
            return false;
        }
    }

    // Method to check if the principal has the required roles
    public bool IsAuthorized(HashSet<string> authRoles)
    {
        if (Principal == null)
        {
            _logger.LogError("ClaimsPrincipal is not set.");
            HasError = true;
            return false;
        }

        foreach (var role in authRoles)
        {
            if (Principal.IsInRole(role))
            {
                return true;
            }
        }

        _logger.LogWarning("The principal does not have the required roles.");
        return false;
    }
}

// Note: ValidationService class must be defined in your codebase with the method HashedOTPFetchingAsync
// and any other methods/properties necessary for this class to function properly.
