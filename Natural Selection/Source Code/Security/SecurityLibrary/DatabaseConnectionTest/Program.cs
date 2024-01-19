using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DatabaseConnectionTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Database Connection and Setup Test Application");

            string connectionString = "Server=DESKTOP-M0GIFNC\\SQLEXPRESS;Database=TestDB;User Id=admin;Password=1234;TrustServerCertificate=true;";

            try
            {
                // Set up database (create table if not exists)
                await SetupDatabaseAsync(connectionString);

                Console.WriteLine("Database setup is complete.");

                // Test database connection
                await TestDatabaseConnectionAsync(connectionString);

                Console.WriteLine("Database connection successful.");

                // Test database update
                await TestDatabaseUpdateAsync(connectionString);

                Console.WriteLine("Database update successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static async Task SetupDatabaseAsync(string connectionString)
        {
            string createTableCommandText = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserAccount')
                BEGIN
                    CREATE TABLE UserAccount (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        Email NVARCHAR(255) NOT NULL,
                        OtpSalt NVARCHAR(255) NOT NULL,
                        HashedOTP NVARCHAR(255) NOT NULL,
                        LastLoginDate DATETIME NULL
                    )
                END";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(createTableCommandText, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("UserAccount table has been checked/created.");
                }
            }
        }

        static async Task TestDatabaseConnectionAsync(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Successfully connected to the database.");
            }
        }

        static async Task TestDatabaseUpdateAsync(string connectionString)
        {
            // This command assumes that a row with Id 1 exists. Adjust it as needed.
            string updateCommandText = "UPDATE UserAccount SET LastLoginDate = GETDATE() WHERE Id = 1";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(updateCommandText, connection))
                {
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        // Optional: Insert a test row if the update affects 0 rows
                        string insertCommandText = "INSERT INTO UserAccount (Email, OtpSalt, HashedOTP) VALUES ('test@example.com', 'salt', 'hashedOtp')";
                        using (var insertCommand = new SqlCommand(insertCommandText, connection))
                        {
                            await insertCommand.ExecuteNonQueryAsync();
                        }

                        // Attempt the update again after inserting the test row
                        rowsAffected = await command.ExecuteNonQueryAsync();
                    }

                    Console.WriteLine($"Rows affected by the update operation: {rowsAffected}");
                }
            }
        }
    }
}
