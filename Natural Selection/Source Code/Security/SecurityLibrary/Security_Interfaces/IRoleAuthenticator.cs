using System.Security.Claims;

public interface IRoleAuthenticator
{
    ClaimsPrincipal AuthenticateAsync(AuthenticationRequest authRequest);
}