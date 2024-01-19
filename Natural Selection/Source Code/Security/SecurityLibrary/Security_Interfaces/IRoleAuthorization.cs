using System.Security.Claims;

public interface IRoleAuthorization
{
    ClaimsPrincipal Principal { get; set; }
    bool IsAuthorizedAsync(HashSet<string> authRoles);
}