public interface IValidationDBService
{
    Task<string> GetUserSaltAsync(string userIdentity);
    Task<bool> InsertHashOTPAsync(string otpHash, string userIdentity);
}
