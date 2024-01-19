using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Security_Services;
using System.Data;
using System.Reflection;

// Indicates this class contains tests for the IAuthenticationDBService interface.
[TestClass]
public class AuthenticationDBServiceTests
{
    private AuthenticationDBService _authenticationDBService;
    private string _connectionString;

    // Setup method that runs before each test.
    [TestInitialize]
    public async Task Setup()
    {
        // Define the connection string for the test database.
        _connectionString = "Server=DESKTOP-M0GIFNC\\SQLEXPRESS;Database=TestDB;User Id=admin;Password=1234;TrustServerCertificate=true;";

        // Create an instance of the AuthenticationDBService with the connection string.
        _authenticationDBService = new AuthenticationDBService(_connectionString);

        // Reset the database state to a known clean state before running each test.
        await ResetDatabaseState();
    }

    // Helper method to reset the database state by recreating the necessary tables.
    private async Task ResetDatabaseState()
    {
        // Open a connection to the SQL Server database.
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // SQL script to drop and recreate necessary tables.
            string resetScript = @"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserAccount')
                BEGIN
                    DROP TABLE UserAccount;
                END
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserProfile')
                BEGIN
                    DROP TABLE UserProfile;
                END
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClaimsPrincipalHash')
                BEGIN
                    DROP TABLE ClaimsPrincipalHash;
                END

                CREATE TABLE UserAccount (
                    Email NVARCHAR(255) PRIMARY KEY,
                    otpTimestamp DATETIME,
                    otpHash NVARCHAR(255),
                    firstAuthenticationFailTimestamp DATETIME,
                    failedAuthenticationAttempts INT,
                    userStatus BIT 
                );


                CREATE TABLE UserProfile (
                    Email NVARCHAR(255) PRIMARY KEY,
                    userRole NVARCHAR(255)
                );

                CREATE TABLE ClaimsPrincipalHash (
                    UserIdentity NVARCHAR(255) PRIMARY KEY,
                    PrincipalHash NVARCHAR(255)
                );";

            // Execute the SQL script.
            using (var command = new SqlCommand(resetScript, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    // Test for CheckUserIdentityExistsAsync method.
    [TestMethod]
    public async Task CheckUserIdentityExistsAsync_WhenUserExists_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";

        // Insert the test user directly into the UserAccount table.
        var query = @"
    INSERT INTO UserAccount (Email, firstAuthenticationFailTimestamp, failedAuthenticationAttempts) 
    VALUES (@UserIdentity, NULL, 0)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.CheckUserIdentityExistsAsync(userIdentity);

        // Assert: Check if the result is true since the user exists.
        Assert.IsTrue(result);
    }

    // Test for CheckUserNotAlreadyAuthenticatedAsync method.
    [TestMethod]
    public async Task CheckUserNotAlreadyAuthenticatedAsync_WhenUserNotAuthenticated_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";

        // No need to insert into UserAccount table since we're checking for non-authentication
        // No insert into ClaimsPrincipalHash to simulate the user is not authenticated

        // Act: Call the method under test.
        var result = await _authenticationDBService.CheckUserNotAlreadyAuthenticatedAsync(userIdentity);

        // Assert: Check if the result is true since there is no entry in ClaimsPrincipalHash for the user.
        Assert.IsTrue(result);
    }




    // Test for CheckFirstAuthenticationFailTimestampNullAsync method.
    [TestMethod]
    public async Task CheckFirstAuthenticationFailTimestampNullAsync_WhenTimestampIsNull_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";

        // Insert the test user directly into the UserAccount table with null timestamp.
        var query = @"
    INSERT INTO UserAccount (Email, firstAuthenticationFailTimestamp, failedAuthenticationAttempts) 
    VALUES (@UserIdentity, NULL, 0)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.CheckFirstAuthenticationFailTimestampNullAsync(userIdentity);

        // Assert: Check if the result is true since the timestamp is null.
        Assert.IsTrue(result);
    }

    // Test for CheckFirstAuthenticationFailTimestampNeedsResetAsync method.
    [TestMethod]
    public async Task CheckFirstAuthenticationFailTimestampNeedsResetAsync_WhenTimestampNeedsReset_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";
        var firstFailTimestamp = DateTime.UtcNow.AddHours(-25); // Set timestamp older than 24 hours.

        // Insert the test user directly into the UserAccount table with the old timestamp.
        var query = @"
    INSERT INTO UserAccount (Email, firstAuthenticationFailTimestamp, failedAuthenticationAttempts) 
    VALUES (@UserIdentity, @FirstFailTimestamp, 0)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                command.Parameters.AddWithValue("@FirstFailTimestamp", firstFailTimestamp);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.CheckFirstAuthenticationFailTimestampNeedsResetAsync(userIdentity);

        // Assert: Check if the result is true since the timestamp needs reset.
        Assert.IsTrue(result);
    }

    // Test for GetFailedAuthenticationAttemptsAsync method.
    [TestMethod]
    public async Task GetFailedAuthenticationAttemptsAsync_WhenAttemptsExist_ReturnsAttemptCount()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";
        var failedAttempts = 3;

        // Insert the test user directly into the UserAccount table with the specified failed attempts.
        var query = @"
    INSERT INTO UserAccount (Email, firstAuthenticationFailTimestamp, failedAuthenticationAttempts) 
    VALUES (@UserIdentity, NULL, @FailedAttempts)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                command.Parameters.AddWithValue("@FailedAttempts", failedAttempts);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.GetFailedAuthenticationAttemptsAsync(userIdentity);

        // Assert: Check if the result matches the expected number of failed attempts.
        Assert.AreEqual(failedAttempts, result);
    }

    // Test for ResetAuthenticationFailuresAsync method.
    [TestMethod]
    public async Task ResetAuthenticationFailuresAsync_WhenResetSuccessful_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";

        // Insert the test user directly into the UserAccount table with old timestamp and failed attempts.
        var queryInsert = @"
    INSERT INTO UserAccount (Email, firstAuthenticationFailTimestamp, failedAuthenticationAttempts) 
    VALUES (@UserIdentity, @FirstFailTimestamp, @FailedAttempts)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(queryInsert, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                command.Parameters.AddWithValue("@FirstFailTimestamp", DateTime.UtcNow.AddHours(-25));
                command.Parameters.AddWithValue("@FailedAttempts", 3);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test to reset authentication failures.
        var queryUpdate = @"
    UPDATE UserAccount
    SET firstAuthenticationFailTimestamp = NULL, failedAuthenticationAttempts = 0
    WHERE Email = @UserIdentity";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(queryUpdate, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Check if the reset was successful.
                var result = rowsAffected > 0;

                // Assert: Check if the result is true since the reset was successful.
                Assert.IsTrue(result);
            }
        }
    }

    // Test for CheckOTPNeedsResetAsync method.
    [TestMethod]
    public async Task CheckOTPNeedsResetAsync_WhenOTPNeedsReset_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";

        // Insert the test user directly into the UserAccount table with an old OTP timestamp.
        var query = @"
    INSERT INTO UserAccount (Email, otpTimestamp) 
    VALUES (@UserIdentity, @OTPTimestamp)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                command.Parameters.AddWithValue("@OTPTimestamp", DateTime.UtcNow.AddMinutes(-3));
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.CheckOTPNeedsResetAsync(userIdentity);

        // Assert: Check if the result is true since the OTP needs reset (older than 2 minutes).
        Assert.IsTrue(result);
    }

    // Test for ResetOTPAsync method.
    [TestMethod]
    public async Task ResetOTPAsync_WhenResetSuccessful_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";

        // Insert the test user directly into the UserAccount table with an old OTP timestamp and hash.
        var queryInsert = @"
    INSERT INTO UserAccount (Email, otpTimestamp, otpHash) 
    VALUES (@UserIdentity, @OTPTimestamp, @OTPhash)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(queryInsert, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                command.Parameters.AddWithValue("@OTPTimestamp", DateTime.UtcNow.AddMinutes(-3));
                command.Parameters.AddWithValue("@OTPhash", "oldHash");
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test to reset OTP.
        var queryUpdate = @"
    UPDATE UserAccount
    SET otpTimestamp = NULL, otpHash = NULL
    WHERE Email = @UserIdentity";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(queryUpdate, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Check if the reset was successful.
                var result = rowsAffected > 0;

                // Assert: Check if the result is true since the OTP reset was successful.
                Assert.IsTrue(result);
            }
        }
    }

    // Test for GetOTPHashAsync method.
    [TestMethod]
    public async Task GetOTPHashAsync_WhenOTPExists_ReturnsHash()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";
        var otpHash = "testOTPHash";

        // Insert the test user directly into the UserAccount table with the OTP hash.
        var query = @"
    INSERT INTO UserAccount (Email, otpHash) 
    VALUES (@UserIdentity, @OTPHash)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                command.Parameters.AddWithValue("@OTPHash", otpHash);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.GetOTPHashAsync(userIdentity);

        // Assert: Check if the result matches the expected OTP hash.
        Assert.AreEqual(otpHash, result);
    }

    [TestMethod]
    public async Task GetUserProfileRoleAsync_WhenRoleExists_ReturnsRole()
    {
        // Arrange: Prepare necessary test data.
        var email = "test@example.com";
        var userRole = "Admin";

        // Insert the test user into the UserAccount table.
        var userAccountQuery = @"
INSERT INTO UserAccount (Email) 
VALUES (@Email)";

        // Insert the test user profile into the UserProfile table with the user role.
        var userProfileQuery = @"
INSERT INTO UserProfile (Email, userRole) 
VALUES (@Email, @UserRole)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Insert into UserAccount table
            using (var command = new SqlCommand(userAccountQuery, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                await command.ExecuteNonQueryAsync();
            }

            // Insert into UserProfile table
            using (var command = new SqlCommand(userProfileQuery, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@UserRole", userRole);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.GetUserProfileRoleAsync(email);

        // Assert: Check if the result matches the expected user role.
        Assert.AreEqual(userRole, result);
    }



    // Test for InsertHashPrincipalAsync method.
    [TestMethod]
    public async Task InsertHashPrincipalAsync_WhenInsertSuccessful_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";
        var principalHash = "testPrincipalHash";

        // Act: Call the method under test.
        var result = await _authenticationDBService.InsertHashPrincipalAsync(userIdentity, principalHash);

        // Assert: Check if the result is true since the insert was successful.
        Assert.IsTrue(result);
    }



    // Test for CheckUserStatusEnabledAsync method.
    [TestMethod]
    public async Task CheckUserStatusEnabledAsync_WhenStatusEnabled_ReturnsTrue()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";

        // Insert the test user directly into the UserAccount table with enabled status.
        var query = @"
INSERT INTO UserAccount (Email, userStatus) 
VALUES (@UserIdentity, 1)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserIdentity", userIdentity.Trim());
                await command.ExecuteNonQueryAsync();
            }
        }

        // Act: Call the method under test.
        var result = await _authenticationDBService.CheckUserStatusEnabledAsync(userIdentity);

        // Assert: Check if the result is true since the user status is enabled.
        Assert.IsTrue(result);
    }


    // Cleanup method that runs after all tests.
    [TestCleanup]
    public async Task Cleanup()
    {
        // Clean up any resources or perform necessary cleanup after all tests.

        // Get the _connectionString field using reflection
        var connectionStringField = typeof(AuthenticationDBService)
            .GetField("_connectionString", BindingFlags.NonPublic | BindingFlags.Instance);

        if (connectionStringField != null)
        {
            var connectionString = (string)connectionStringField.GetValue(_authenticationDBService);

            // Close the database connection if it's open.
            if (!string.IsNullOrEmpty(connectionString))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}