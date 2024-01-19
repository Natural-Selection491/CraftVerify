using System.Data.SqlClient;

public class DeauthenticationDBService : IDeauthenticationDBService
{
    private readonly string _connectionString;

    public DeauthenticationDBService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<bool> DeleteClaimsPrincipalHashAsync(string userIdentity)
    {
        var query = "DELETE FROM ClaimsPrincipalHash WHERE UserIdentity = @UserIdentity";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserIdentity", userIdentity);

        try
        {
            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            // Log and/or handle the exception as needed
            throw new InvalidOperationException("Error deleting ClaimsPrincipalHash.", ex);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}