// DAO for ValidationService

using System.Data;
using System.Data.SqlClient;

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
        using SqlConnection connection = new SqlConnection(_connectionString);
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

    public async Task<bool> InsertHashOTPAsync(string otpHash, string userIdentity)
    {
        SqlConnection connection = null;

        try
        {
            connection = new SqlConnection(_connectionString);
            SqlCommand command = new SqlCommand("UPDATE UserAccount SET hashedOTP = @HashedOTP WHERE email = @Email", connection);
            command.Parameters.AddWithValue("@HashedOTP", otpHash);
            command.Parameters.AddWithValue("@Email", userIdentity);

            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                // Log warning for no user account being updated
                // Return false to indicate that the update was not successful.
                return false;
            }

            // Log successful OTP hash insertion into the database
            return true;
        }
        catch (SqlException)
        {
            // Rethrow the SqlException to maintain the original stack trace.
            throw;
        }
        finally
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

}
