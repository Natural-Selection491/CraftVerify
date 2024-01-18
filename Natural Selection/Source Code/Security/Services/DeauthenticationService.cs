using System.Security.Claims;

public class DeauthenticationService : IRoleDeauthenticate
{
    public bool HasError;
    public bool CanRetry;
    public ClaimsPrincipal Principal;
    public bool Deauthenticate(string UserIdentity)
    {
        //deauthenticate deletes the ClaimsPrincipalHash table where UserIdentity = identity 
    }
}