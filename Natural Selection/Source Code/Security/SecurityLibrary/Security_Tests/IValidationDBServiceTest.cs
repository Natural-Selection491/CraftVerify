using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Security_Services; // Include the namespace where ValidationDBService is defined.

// Indicates this class contains tests for the IValidationDBService interface.
[TestClass]
public class IValidationDBServiceTests
{
    private ValidationDBService _validationDBService;
    private string _connectionString;

    // Setup method that runs before each test.
    [TestInitialize]
    public async Task Setup()
    {
        // Define the connection string for the test database.
        _connectionString = "Server=DESKTOP-M0GIFNC\\SQLEXPRESS;Database=TestDB;User Id=admin;Password=1234;TrustServerCertificate=true;";

        // Create an instance of the ValidationDBService with the connection string.
        _validationDBService = new ValidationDBService(_connectionString);

        // Reset the database state to a known clean state before running each test.
        await ResetDatabaseState();
    }

    // Method to reset the database state by recreating the UserAccount table.
    private async Task ResetDatabaseState()
    {
        // Open a connection to the SQL Server database.
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // SQL script to drop the UserAccount table if it exists and then create it.
            string resetScript = @"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserAccount')
                BEGIN
                    DROP TABLE UserAccount;
                END
                CREATE TABLE UserAccount (
                    Id INT PRIMARY KEY IDENTITY,
                    Email NVARCHAR(255) NOT NULL,
                    OtpSalt NVARCHAR(255) NOT NULL,
                    HashedOTP NVARCHAR(255) NOT NULL
                );";

            // Execute the SQL script.
            using (var command = new SqlCommand(resetScript, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    // Test to ensure GetUserSaltAsync returns the expected salt for a given user.
    [TestMethod]
    public async Task GetUserSaltAsync_ReturnsExpectedSalt()
    {
        // Arrange: Prepare necessary test data.
        var userIdentity = "test@example.com";
        var expectedSalt = "expectedSalt";

        // Insert test data into the database.
        await InsertTestUserAsync(userIdentity, expectedSalt);

        // Act: Call the method under test.
        var result = await _validationDBService.GetUserSaltAsync(userIdentity);

        // Assert: Check if the result matches the expected salt.
        Assert.AreEqual(expectedSalt, result);
    }

    // Helper method to insert a test user into the database.
    private async Task InsertTestUserAsync(string email, string salt)
    {
        // Open a connection to the database.
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // SQL command to insert a test user into the UserAccount table.
            var insertCommand = $"INSERT INTO UserAccount (Email, OtpSalt, HashedOTP) VALUES ('{email}', '{salt}', 'dummyHash')";

            // Execute the SQL command.
            using (var command = new SqlCommand(insertCommand, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    [TestMethod]
    public async Task GetUserSaltAsync_WhenDatabaseFails_ThrowsException()
    {
        // Arrange: Set up a user identity that does not exist in the database.
        var userIdentity = "nonexistent@example.com";

        // Act & Assert: Verify that an exception is thrown for a non-existent user.
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _validationDBService.GetUserSaltAsync(userIdentity));
    }

    [TestMethod]
    public async Task InsertHashOTPAsync_SuccessfullyInsertsData()
    {
        // Arrange: Insert a user into the database.
        var userIdentity = "test@example.com";
        var otpHash = "otpHash";
        await InsertTestUserAsync(userIdentity, "userSalt");

        // Act: Call the method under test.
        var result = await _validationDBService.InsertHashOTPAsync(otpHash, userIdentity);

        // Assert: Check that the method returned true, indicating success.
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task InsertHashOTPAsync_WhenInsertFails_ReturnsFalse()
    {
        // Arrange: Use a user identity that does not exist in the database.
        var userIdentity = "nonexistent@example.com";
        var otpHash = "otpHash";

        // Act: Call the method under test.
        var result = await _validationDBService.InsertHashOTPAsync(otpHash, userIdentity);

        // Assert: Check that the method returned false, indicating failure.
        Assert.IsFalse(result);
    }
    [TestMethod]
    public async Task InsertHashOTPAsync_WhenDatabaseFails_ThrowsException()
    {
        // Arrange: Temporarily break the connection string to simulate a database failure.
        var brokenValidationDBService = new ValidationDBService("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;\r\n");

        // Act & Assert: This should throw an SqlException due to the incorrect connection string.
        await Assert.ThrowsExceptionAsync<SqlException>(() =>
            brokenValidationDBService.InsertHashOTPAsync("otpHash", "anyUserIdentity"));
    }
}
