using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Security_Services;
using SecurityLibrary;

[TestClass]
public class AuthenticationServiceTests
{
    private AuthenticationService _authenticationService;
    private string _connectionString;
    private AuthenticationRequest _testAuthRequest;

    [TestInitialize]
    public async Task Setup()
    {
        _connectionString = "YourConnectionStringHere";
        await ResetDatabaseState();

        // Assume these are the actual services you have that implement the respective interfaces.
        var dbService = new AuthenticationDBService(_connectionString);
        var hashService = new ActualHashService(); // Replace with your actual HashService implementation
        var otpService = new ActualOTPService();   // Replace with your actual OTPService implementation

        _authenticationService = new AuthenticationService(dbService, hashService, otpService);

        // Initialize a test AuthenticationRequest object
        _testAuthRequest = new AuthenticationRequest
        {
            UserIdentity = "testuser@example.com",
            Proof = "test_otp"
        };

        // Insert a test user and set up the test environment as needed
        // ...
    }

    private async Task ResetDatabaseState()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            // Replace with your actual SQL script to reset the database
            var resetScript = "SQL Script to Reset Database";
            using (var command = new SqlCommand(resetScript, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    [TestMethod]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsClaimsPrincipal()
    {
        // Arrange: Insert test data into the database...

        // Act
        var result = await _authenticationService.AuthenticateAsync(_testAuthRequest);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ClaimsPrincipal));
    }

    [TestMethod]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        // Arrange: Modify _testAuthRequest to have invalid credentials

        // Act
        var result = await _authenticationService.AuthenticateAsync(_testAuthRequest);

        // Assert
        Assert.IsNull(result);
    }

    // Additional test methods...

    [TestCleanup]
    public async Task Cleanup()
    {
        // Optional cleanup code...
    }
}
