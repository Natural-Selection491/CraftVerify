using System.Security.Claims;
using System.Threading.Tasks;

public interface IRoleAuthorizationDBService
{
    Task<string> GetHashedPrincipalAsync(string userIdentity);
}
