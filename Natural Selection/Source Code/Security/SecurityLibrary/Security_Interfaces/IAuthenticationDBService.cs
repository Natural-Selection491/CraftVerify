using System;
using System.Threading.Tasks;

public interface IAuthenticationDBService
{
    Task<bool> CheckUserIdentityExistsAsync(string userIdentity);
    Task<bool> CheckUserNotAlreadyAuthenticatedAsync(string userIdentity);
    Task<bool> CheckUserStatusEnabledAsync(string userIdentity);
    Task<bool> CheckFirstAuthenticationFailTimestampNullAsync(string userIdentity);
    Task<bool> CheckFirstAuthenticationFailTimestampNeedsResetAsync(string userIdentity);
    Task<int> GetFailedAuthenticationAttemptsAsync(string userIdentity);
    Task<bool> ResetAuthenticationFailuresAsync(string userIdentity);
    Task<bool> CheckOTPNeedsResetAsync(string userIdentity);
    Task<bool> ResetOTPAsync(string userIdentity);
    Task<string> GetOTPHashAsync(string userIdentity);
    Task<string> GetUserProfileRoleAsync(string userIdentity);
    Task<bool> InsertHashPrincipalAsync(string userIdentity, string principalHash);
    Task<bool> InsertFirstAuthenticationFailTimestampAsync(string userIdentity, DateTime currentTimestamp);
    Task<bool> IncrementFailedAuthenticationAttemptsAsync(string userIdentity);
}
