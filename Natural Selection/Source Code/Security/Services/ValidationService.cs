using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public interface IHashService
{
    Task<string> HashValueAsync(string value);
}

public interface IOTPService
{
    Task<string> CreateOTPAsync();
}

public class ValidationService : IHashService, IOTPService
{
    private readonly string _connectionString;
    private string _userSalt;
    private readonly string _userIdentity;
    private bool _isSaltFetched = false;

    public ValidationService(string connectionString, string userIdentity)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _userIdentity = userIdentity ?? throw new ArgumentNullException(nameof(userIdentity));
        // TODO: Implement logging here to indicate service initialization
    }

    private async Task EnsureSaltIsFetchedAsync()
    {
        if (!_isSaltFetched)
        {
            _userSalt = await GetUserSaltAsync(_userIdentity).ConfigureAwait(false);
            _isSaltFetched = true;
            // TODO: Implement logging to indicate successful salt retrieval
        }
    }

    public async Task<string> CreateOTPAsync()
    {
        await EnsureSaltIsFetchedAsync();

        var otp = GenerateRandomCode();
        var otpHash = await HashValueAsync(otp).ConfigureAwait(false);

        await InsertHashOTPAsync(otpHash).ConfigureAwait(false);

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

    private async Task<string> GetUserSaltAsync(string userIdentity)
    {
        // Retrieve the user-specific salt from the database
        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand("SELECT otpSalt FROM UserAccount WHERE email = @Email", connection);
        command.Parameters.AddWithValue("@Email", userIdentity);

        await connection.OpenAsync();
        var salt = await command.ExecuteScalarAsync() as string;
        await connection.CloseAsync();

        if (string.IsNullOrEmpty(salt))
        {
            // TODO: Log error for missing or empty salt
            throw new InvalidOperationException("User salt not found or is empty.");
        }

        // TODO: Log successful salt retrieval
        return salt;
    }

    public async Task<string> HashValueAsync(string value)
    {
        await EnsureSaltIsFetchedAsync();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_userSalt));
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        byte[] hashBytes = await Task.Run(() => hmac.ComputeHash(valueBytes)).ConfigureAwait(false);

        // TODO: Log hashing operation
        return Convert.ToBase64String(hashBytes);
    }

    private async Task InsertHashOTPAsync(string otpHash)
    {
        // TODO: Consider how user identity is determined for updating the correct record
        await EnsureSaltIsFetchedAsync();

        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand("UPDATE UserAccount SET hashedOTP = @HashedOTP WHERE email = @Email", connection);
        command.Parameters.AddWithValue("@HashedOTP", otpHash);
        command.Parameters.AddWithValue("@Email", _userIdentity);

        await connection.OpenAsync();
        int rowsAffected = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        if (rowsAffected == 0)
        {
            // TODO: Log warning or error for no user account being updated
            throw new InvalidOperationException($"No user account found or updated with email: {_userIdentity}.");
        }

        // TODO: Log successful OTP hash insertion into the database
    }
}



/*

using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public interface IHashService
{
    Task<string> HashValueAsync(string value, string salt);
}

public interface IOTPService
{
    Task<string> CreateOTPAsync(string userIdentity);
}

public class ValidationService : IHashService, IOTPService
{
    private readonly string _connectionString;

    public ValidationService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<string> CreateOTPAsync(string userIdentity)
    {
        // Validate input parameters.
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity must not be null or empty.", nameof(userIdentity));
        }

        try
        {
            var userSalt = await GetUserSaltAsync(userIdentity).ConfigureAwait(false);
            var otp = GenerateRandomCode();
            var otpHash = await HashValueAsync(otp, userSalt).ConfigureAwait(false);

            // Save the hashed OTP in the database.
            await InsertHashOTPAsync(otpHash, userIdentity).ConfigureAwait(false);

            // TODO: Send the OTP via a secure channel (email, SMS, etc.)

            return otp;
        }
        catch (Exception ex)
        {
            // TODO: Log the exception using a logging framework.
            throw new Exception("An error occurred while creating the OTP.", ex);
        }
    }

    private string GenerateRandomCode()
    {
        // Generate a secure random OTP of 8 characters.
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

        return new string(otpChars);
    }

    public async Task<string> GetUserSaltAsync(string userIdentity)
    {
        // Retrieve the user-specific salt from the database.
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT otpSalt FROM UserAccount WHERE email = @Email", connection);
        command.Parameters.AddWithValue("@Email", userIdentity);

        await connection.OpenAsync();
        var salt = await command.ExecuteScalarAsync() as string;
        await connection.CloseAsync();

        if (string.IsNullOrEmpty(salt))
        {
            // TODO: Log the missing salt issue.
            throw new InvalidOperationException("User salt not found or is empty.");
        }

        return salt;
    }

    public async Task<string> HashValueAsync(string value, string salt)
    {
        // Compute the HMACSHA256 hash of the input value using the user-specific salt.
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        byte[] hashBytes = await Task.Run(() => hmac.ComputeHash(valueBytes));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task InsertHashOTPAsync(string otpHash, string userIdentity)
    {
        // Insert the hashed OTP into the database.
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("UPDATE UserAccount SET hashedOTP = @HashedOTP WHERE email = @Email", connection);
        command.Parameters.AddWithValue("@HashedOTP", otpHash);
        command.Parameters.AddWithValue("@Email", userIdentity);

        await connection.OpenAsync();
        int rowsAffected = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        if (rowsAffected == 0)
        {
            // TODO: Log the failed update issue.
            throw new InvalidOperationException($"No user account found or updated with email: {userIdentity}.");
        }
    }
}




using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

public class ValidationService : IHashService, IOTPService
{
    private readonly string _connectionString;

    public ValidationService(string connectionString, string userIdentity)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        // UserIdentity should be validated or used to retrieve UserSalt here
        // For example:
        UserSalt = GetSalt(userIdentity) ?? throw new InvalidOperationException("Salt cannot be null or empty.");
    }

    public string CreateOTP(string userIdentity)
    {
        var otp = GenerateRandomCode();
        var otpHash = HashValue(otp);

        // Consider using a transaction scope if needed
        if (!InsertHashOTP(otpHash, userIdentity))
        {
            throw new InvalidOperationException("Failed to insert OTP hash into the database.");
        }

        // Consider sending the OTP via email or another secure method here

        return otp;
    }

    private string GenerateRandomCode()
    {
        // Consider using a more cryptographically secure method of generating the OTP
        const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        using var random = new RNGCryptoServiceProvider();
        var otpChars = new char[8];

        for (int i = 0; i < otpChars.Length; i++)
        {
            byte[] randomBytes = new byte[1];
            random.GetBytes(randomBytes);
            var index = randomBytes[0] % validCharacters.Length;
            otpChars[i] = validCharacters[index];
        }

        return new string(otpChars);
    }

    private string GetSalt(string userIdentity)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT Salt FROM UserAccounts WHERE Identity = @Identity", connection);
        command.Parameters.AddWithValue("@Identity", userIdentity);

        connection.Open();
        var salt = command.ExecuteScalar() as string;
        connection.Close();

        return salt;
    }

    public string HashValue(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(UserSalt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash);
    }

    private bool InsertHashOTP(string otpHash, string userIdentity)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("UPDATE UserAccounts SET HashedOTP = @HashedOTP WHERE Identity = @Identity", connection);
        command.Parameters.AddWithValue("@HashedOTP", otpHash);
        command.Parameters.AddWithValue("@Identity", userIdentity);

        connection.Open();
        int rowsAffected = command.ExecuteNonQuery();
        connection.Close();

        return rowsAffected > 0;
    }
}

    // Interface definitions (remain unchanged)
    public interface IHashService
    {
        string HashValue(string value);
    }

    public interface IOTPService
    {
        string CreateOTP(string userIdentity);
    }



//============================================================================================
/
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class ValidationService : IHashService, IOTPService
{
    private readonly MyDbContext _dbContext;
    public string UserIdentity { get; private set; }
    private readonly string UserSalt;

    //Constructor to initialize the ValidationService with the user identity and an optional proof
    public ValidationService(string userIdentity)
    {
        UserIdentity = userIdentity;
        Proof = proof;
        //Retrieve the user-specific salt during initialization for HashValue() to use
        UserSalt = GetSaltAsync(userIdentity); 

        //Log
    }

    //Creates OTP, hashes OTP, stores hashed OTP, returns plain OTP to send to user's email
    public string CreateOTP(string userIdentity)
    {
        //Create OTP
        var otp = GenerateRandomCode();

        //Hash the OTP with the user-specific salt
        var otpHash = HashValue(otp);

        //Save the hash in the DB
        InsertHashOTPAsync(otpHash, userIdentity);

        //Log

        //Return the plain OTP to be sent to the user via email
        return otp;
    }


    //OTP is 8 Chars consisting of:
    //A-Z
    //a-z
    //0-9
    private string GenerateRandomCode()
    {
        const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new RNGCryptoServiceProvider();
        var otpChars = new char[8]; //OTP length of 8 characters.

        //Generate a random index for each character in the OTP.
        byte[] randomBytes = new byte[1];
        for (int i = 0; i < otpChars.Length; i++)
        {
            random.GetBytes(randomBytes);
            var index = randomBytes[0] % validCharacters.Length;
            otpChars[i] = validCharacters[index];
        }

        otp = new string(otpChars);

        //log

        return otp;
    }

    private async Task<string> GetSaltAsync(string userIdentity)
    {
        var salt = await _dbContext.UserAccounts
            .AsNoTracking() //Use AsNoTracking if you do not intend to update the entity in the same context.
            .Where(u => u.identity == userIdentity)
            .Select(u => u.Salt)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(salt))
        {
            throw new InvalidOperationException("User account not found or salt is not set.");
        }

        //log
        return salt;
    }


    public string HashValue(string value)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(UserSalt)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            //log
            return Convert.ToBase64String(hash);
        }
    }

    //Async insert hash OTP into userAccount table
    private async Task<bool> InsertHashOTPAsync(string otpHash, string userIdentity)
    {
        //Insert the hash into the database.
        var userAccount = await _dbContext.UserAccounts.SingleOrDefaultAsync(u => u.identity == userIdentity);
        if (userAccount != null)
        {
            userAccount.hashedOTP = otpHash;
            await _dbContext.SaveChangesAsync();
            //Log
            return true;
        }
        //Log
        return false;
    }

}
*/