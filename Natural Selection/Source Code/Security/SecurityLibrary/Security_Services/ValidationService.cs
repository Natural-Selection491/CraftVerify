using System.Security.Cryptography;
using System.Text;
namespace Security_Services;
public class ValidationService : IHashService, IOTPService
{
    private readonly IValidationDBService _databaseService;
    private string _userSalt;
    private bool _isSaltFetched = false;

    public ValidationService(IValidationDBService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        // TODO: Implement logging here to indicate service initialization
    }

    private async Task EnsureSaltIsFetchedAsync(string userIdentity)
    {
        if (!_isSaltFetched)
        {
            // Exception handling and logging should be implemented.
            try
            {
                _userSalt = await _databaseService.GetUserSaltAsync(userIdentity).ConfigureAwait(false);
                _isSaltFetched = true;
            }
            catch (Exception ex)
            {
                // TODO: Implement logging for the exception
                throw; // Rethrow the exception to be handled higher up the call stack.
            }
        }
    }

    public async Task<string> CreateOTPAsync(string userIdentity)
    {
        await EnsureSaltIsFetchedAsync(userIdentity);

        var otp = GenerateRandomCode();
        var otpHash = await HashValueAsync(otp, userIdentity).ConfigureAwait(false); // Pass both parameters

        var isInserted = await _databaseService.InsertHashOTPAsync(otpHash, userIdentity).ConfigureAwait(false);
        if (!isInserted)
        {
            throw new InvalidOperationException("Failed to insert OTP hash.");
        }
        // TODO: Implement OTP transmission logic to send OTP to the user via a secure channel (email, SMS, etc.)
        // TODO: Implement logging for OTP creation and storage

        return otp;
    }

    private string GenerateRandomCode()
    {
        // Generate a secure random OTP
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        using var random = new RNGCryptoServiceProvider();
        var otpChars = new char[8];
        byte[] randomBytes = new byte[1];

        for (int i = 0; i < otpChars.Length; i++)
        {
            random.GetBytes(randomBytes);
            var index = randomBytes[0] % validChars.Length;
            otpChars[i] = validChars[index];
        }

        // TODO: Implement logging for OTP code generation
        return new string(otpChars);
    }

    public async Task<string> HashValueAsync(string value, string userIdentity)
    {
        await EnsureSaltIsFetchedAsync(userIdentity);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_userSalt));
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        byte[] hashBytes = hmac.ComputeHash(valueBytes);

        return Convert.ToBase64String(hashBytes);
    }
}