// DAO for ValidationService

using MySql.Data.MySqlClient;

public class ValidationDBService : IValidationDBService
{
    private readonly string _connectionString;

    public ValidationDBService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<string> GetUserSaltAsync(string userIdentity)
    {
        // Retrieve the user-specific salt from the database
        using MySqlConnection connection = new MySqlConnection(_connectionString);
        var command = new MySqlCommand("SELECT otpSalt FROM UserAccount WHERE email = @Email", connection);
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

    public async Task<bool> InsertHashOTPAsync(string otpHash, string userIdentity)
    {
        try
        {
            using MySqlConnection connection = new MySqlConnection(_connectionString);
            MySqlCommand command = new MySqlCommand("UPDATE UserAccount SET hashedOTP = @HashedOTP WHERE email = @Email", connection);
            command.Parameters.AddWithValue("@HashedOTP", otpHash);
            command.Parameters.AddWithValue("@Email", userIdentity);

            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            if (rowsAffected == 0)
            {
                // TODO: Log warning for no user account being updated
                return false;
            }

            // TODO: Log successful OTP hash insertion into the database
            return true;
        }
        catch (MySqlException ex)
        {
            // TODO: Log SQL exception
            return false;
        }
    }
}
