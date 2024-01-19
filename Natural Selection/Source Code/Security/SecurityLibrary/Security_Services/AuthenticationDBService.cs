using System.Data.SqlClient;

public class AuthenticationDBService : IAuthenticationDBService
{
    private readonly string _connectionString;

    public AuthenticationDBService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<bool> CheckUserIdentityExistsAsync(string userIdentity)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to check if the user identity exists in the database
        var query = "SELECT COUNT(1) FROM UserAccount WHERE email = @UserIdentity";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error checking user identity existence.", ex);
        }
    }

    public async Task<bool> CheckUserNotAlreadyAuthenticatedAsync(string email)
    {
        // SQL command to check if the user has an active authentication
        var query = @"
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM ClaimsPrincipalHash
            WHERE UserIdentity = @Email AND PrincipalHash IS NOT NULL
        ) THEN CAST(0 AS BIT) 
        ELSE CAST(1 AS BIT) 
    END";

        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email); // Ensure the parameter name matches the SQL query

        try
        {
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return (bool)result; // If result is 0 (FALSE), then the user is considered not authenticated
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error checking user authentication status.", ex);
        }
    }



    public async Task<bool> CheckUserStatusEnabledAsync(string userIdentity)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to check if the user's account status is enabled (assuming 'true' is enabled)
        var query = "SELECT userStatus FROM UserAccount WHERE email = @UserIdentity";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());

            var result = await command.ExecuteScalarAsync();
            // If result is not null and true, then the user status is enabled
            return result != null && (bool)result;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error checking user status.", ex);
        }
    }

    public async Task<bool> CheckFirstAuthenticationFailTimestampNullAsync(string userIdentity)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to check if the firstAuthenticationFailTimestamp is null for the user
        var query = "SELECT firstAuthenticationFailTimestamp FROM UserAccount WHERE email = @UserIdentity";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());

            var result = await command.ExecuteScalarAsync();
            // If result is DBNull, then the timestamp is null, indicating no failed attempts
            return result == DBNull.Value;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error checking authentication fail timestamp.", ex);
        }
    }

    public async Task<bool> CheckFirstAuthenticationFailTimestampNeedsResetAsync(string userIdentity)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to get the first authentication fail timestamp for the user
        var query = "SELECT firstAuthenticationFailTimestamp FROM UserAccount WHERE email = @UserIdentity";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());

            var result = await command.ExecuteScalarAsync();

            if (result == DBNull.Value)
            {
                // If the result is DBNull, no timestamp is set, and no reset is needed
                return false;
            }

            var firstFailTimestamp = (DateTime)result;
            var timeDifference = DateTime.UtcNow - firstFailTimestamp;

            // Check if the difference is greater than 24 hours
            return timeDifference.TotalHours > 24;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error checking if authentication fail timestamp needs reset.", ex);
        }
    }

    public async Task<int> GetFailedAuthenticationAttemptsAsync(string userIdentity)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to get the number of failed authentication attempts for the user
        var query = "SELECT failedAuthenticationAttempts FROM UserAccount WHERE email = @UserIdentity";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());

            var result = await command.ExecuteScalarAsync();
            // Return the number of failed attempts. If the result is DBNull, return 0.
            return result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error retrieving failed authentication attempts.", ex);
        }
    }

    public async Task<bool> ResetAuthenticationFailuresAsync(string userIdentity)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to reset authentication failure information for the user
        var query = @"
            UPDATE UserAccount 
            SET firstAuthenticationFailTimestamp = NULL, 
                failedAuthenticationAttempts = 0 
            WHERE email = @UserIdentity";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());

            int rowsAffected = await command.ExecuteNonQueryAsync();
            // Check if any rows were affected. If not, the update may not have been necessary.
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error resetting authentication failures.", ex);
        }
    }

    public async Task<bool> CheckOTPNeedsResetAsync(string userIdentity)
    {
        // SQL command to check if the OTP timestamp is older than 2 minutes
        var query = @"
            SELECT 
                CASE 
                    WHEN DATEDIFF(minute, OtpTimestamp, GETUTCDATE()) > 2 THEN 1 
                    ELSE 0 
                END 
            FROM UserAccount WHERE email = @UserIdentity";

        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserIdentity", userIdentity);

        try
        {
            await connection.OpenAsync();
            var needsReset = (int)await command.ExecuteScalarAsync();
            return needsReset == 1;
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException("Database operation failed during OTP reset check.", ex);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<bool> ResetOTPAsync(string userIdentity)
    {
        // SQL command to reset OTP (set otpTimestamp and otpHash to null)
        var query = "UPDATE UserAccount SET otpTimestamp = NULL, otpHash = NULL WHERE email = @UserIdentity";

        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserIdentity", userIdentity);

        try
        {
            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0; // Returns true if one or more rows were updated.
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error resetting OTP.", ex);
        }
    }

    public async Task<string> GetOTPHashAsync(string userIdentity)
    {
        // SQL command to retrieve OTP hash for the user
        var query = "SELECT otpHash FROM UserAccount WHERE email = @UserIdentity";

        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserIdentity", userIdentity);

        try
        {
            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            // If result is not DBNull, convert to string and return, else return null
            return result != DBNull.Value ? result.ToString() : null;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error retrieving OTP hash.", ex);
        }
    }

    public async Task<string> GetUserProfileRoleAsync(string email)
    {
        var query = @"
SELECT UP.userRole 
FROM UserProfile UP
INNER JOIN UserAccount UA ON UP.Email = UA.Email 
WHERE UA.Email = @Email";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        try
        {
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            // Check if the result is null before trying to call .ToString()
            if (result != null && result != DBNull.Value)
            {
                return result.ToString();
            }
            else
            {
                // Handle the case where the user role is not found
                return null; // or throw an appropriate exception if necessary
            }
        }
        catch (Exception ex)
        {
            // Log the exception details and rethrow or handle as appropriate
            throw new InvalidOperationException("Error retrieving user role.", ex);
        }
    }



    public async Task<bool> InsertHashPrincipalAsync(string userIdentity, string principalHash)
    {
        // SQL command to insert principal hash into the database
        var query = "INSERT INTO ClaimsPrincipalHash (UserIdentity, PrincipalHash) VALUES (@UserIdentity, @PrincipalHash)";

        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserIdentity", userIdentity);
        command.Parameters.AddWithValue("@PrincipalHash", principalHash);

        try
        {
            await connection.OpenAsync();

            int rowsAffected = await command.ExecuteNonQueryAsync();
            // Check if any rows were affected by the insert operation
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error inserting principal hash.", ex);
        }
    }

    public async Task<bool> InsertFirstAuthenticationFailTimestampAsync(string userIdentity, DateTime currentTimestamp)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to insert the first authentication fail timestamp for the user
        var query = @"
            UPDATE UserAccount 
            SET firstAuthenticationFailTimestamp = @CurrentTimestamp 
            WHERE email = @UserIdentity AND firstAuthenticationFailTimestamp IS NULL";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
            command.Parameters.AddWithValue("@CurrentTimestamp", currentTimestamp);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            // Check if any rows were affected. If not, the update may not have been necessary.
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error inserting first authentication fail timestamp.", ex);
        }
    }

    public async Task<bool> IncrementFailedAuthenticationAttemptsAsync(string userIdentity)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            throw new ArgumentException("User identity cannot be null or whitespace.", nameof(userIdentity));
        }

        // SQL command to increment the failed authentication attempts for the user
        var query = @"
            UPDATE UserAccount 
            SET failedAuthenticationAttempts = failedAuthenticationAttempts + 1 
            WHERE email = @UserIdentity";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());

            int rowsAffected = await command.ExecuteNonQueryAsync();
            // Check if any rows were affected. If not, the update may not have been necessary.
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            // Log the exception details and rethrow a custom exception or handle it accordingly
            throw new InvalidOperationException("Error incrementing failed authentication attempts.", ex);
        }
    }
}