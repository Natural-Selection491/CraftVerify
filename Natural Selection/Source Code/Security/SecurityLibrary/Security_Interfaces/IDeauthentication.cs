public interface IDeauthentication
{
    Task<bool> DeauthenticateAsync(string userIdentity);
}