using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Security_Services; 

// TestClass attribute denotes a class that contains unit tests.
[TestClass]
public class IOTPServiceTests
{
    private ValidationService _validationService;
    private string _connectionString;

    // TestInitialize attribute denotes a method that runs before each test to set up test environments.
    [TestInitialize]
    public async Task Setup()
    {
        // Connection string to the test database
        _connectionString = "Server=DESKTOP-M0GIFNC\\SQLEXPRESS;Database=TestDB;User Id=admin;Password=1234;TrustServerCertificate=true;";

        // Reset the database state before each test
        await ResetDatabaseState();

        // Create an instance of ValidationService with a real database service
        var dbService = new ValidationDBService(_connectionString);
        _validationService = new ValidationService(dbService);
    }

    // Resets the database to a clean state by dropping and recreating tables
    private async Task ResetDatabaseState()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
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

            using (var command = new SqlCommand(resetScript, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    // Test to ensure CreateOTPAsync generates a valid OTP.
    [TestMethod]
    public async Task CreateOTPAsync_GeneratesValidOTP()
    {
        // Arrange: Set up necessary data in the database.
        var userIdentity = "user@example.com";
        var salt = "userSalt";
        await InsertTestUserAsync(userIdentity, salt);

        // Act: Call the method under test.
        var otp = await _validationService.CreateOTPAsync(userIdentity);

        // Assert: Check if the OTP is generated as expected.
        Assert.IsNotNull(otp);
        Assert.AreEqual(8, otp.Length); // Assuming OTP length is 8 characters.
    }

    // Helper method to insert a test user into the database
    private async Task InsertTestUserAsync(string email, string salt)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var insertCommand = $"INSERT INTO UserAccount (Email, OtpSalt, HashedOTP) VALUES ('{email}', '{salt}', 'dummyHash')";
            using (var command = new SqlCommand(insertCommand, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    // Test to check the behavior when the database fails to provide a salt.
    [TestMethod]
    public async Task CreateOTPAsync_WhenGetSaltFails_ThrowsException()
    {
        // Arrange: Set up a user identity that does not exist in the database.
        var userIdentity = "nonexistent@example.com";

        // Act & Assert: Verify that the method throws an exception when the database fails.
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _validationService.CreateOTPAsync(userIdentity));
    }

    // Test to ensure CreateOTPAsync handles database failure during OTP hash insertion.
    [TestMethod]
    public async Task CreateOTPAsync_WhenInsertOTPHashFails_ThrowsException()
    {
        // Arrange
        var brokenConnectionString = "Server=invalid_server;Database=myDataBase;User Id=myUsername;Password=myPassword;";
        var validationDBService = new ValidationDBService(brokenConnectionString);
        var validationService = new ValidationService(validationDBService);

        // Act & Assert
        // The test expects an InvalidOperationException because InsertHashOTPAsync should fail
        // due to the invalid connection string, which should not allow a connection to the database.
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            validationService.CreateOTPAsync("user@example.com"));
    }
}
