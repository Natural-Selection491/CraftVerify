public interface IOTPService
{
    Task<string> CreateOTPAsync(string userIdentity);
}
