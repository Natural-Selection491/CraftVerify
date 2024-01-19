public interface IDeauthenticationDBService
{
    Task<bool> DeleteClaimsPrincipalHashAsync(string userIdentity);
}