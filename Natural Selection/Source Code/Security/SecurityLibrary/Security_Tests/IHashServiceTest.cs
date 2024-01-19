using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Security_Services;

// TestClass attribute denotes a class that contains unit tests.
[TestClass]
public class IHashServiceTest
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

    // A test method to ensure HashValueAsync returns the correct hash for valid inputs.
    [TestMethod]
    public async Task HashValueAsync_WithValidInput_ReturnsExpectedHash()
    {
        // Arrange: Setting up the test data and expectations.
        var userIdentity = "test@example.com";
        var valueToHash = "password";
        var salt = "someSalt";

        // Insert test data into the database
        await InsertTestUserAsync(userIdentity, salt);

        var expectedHash = GetExpectedHash(valueToHash, salt);

        // Act: Calling the method under test.
        var result = await _validationService.HashValueAsync(valueToHash, userIdentity);

        // Assert: Verifying that the result matches the expected hash.
        Assert.AreEqual(expectedHash, result);
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

    // Helper method to calculate the expected hash value for testing.
    private string GetExpectedHash(string value, string salt)
    {
        // Using HMACSHA256 to create a hash of the value with the provided salt.
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);

        // Returning the computed hash as a base64 string.
        return Convert.ToBase64String(hmac.ComputeHash(valueBytes));
    }

    // A test method to check if HashValueAsync throws an ArgumentNullException for null values.
    [TestMethod]
    public async Task HashValueAsync_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange: Setting up a user identity for the test.
        var userIdentity = "test@example.com";

        // Act & Assert: Verifying that calling HashValueAsync with a null value throws ArgumentNullException.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            _validationService.HashValueAsync(null, userIdentity));
    }

    // A test method to ensure HashValueAsync throws an exception when the database service fails.
    [TestMethod]
    public async Task HashValueAsync_WhenDatabaseFails_ThrowsException()
    {
        // Arrange: Setting up test data with a user identity that does not exist in the database.
        var userIdentity = "nonexistent@example.com";
        var valueToHash = "password";

        // Act & Assert: Verifying that HashValueAsync throws an exception when the database fails to find the user.
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _validationService.HashValueAsync(valueToHash, userIdentity));
    }

}
