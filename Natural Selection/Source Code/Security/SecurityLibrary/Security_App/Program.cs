using System;
using Security_Services; // Assuming Security_Services is the correct namespace.

namespace Security_App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Security App Test Driver");

            // The connection string should be provided; this is just a placeholder.
            string connectionString = "Server=DESKTOP-M0GIFNC\\SQLEXPRESS;Database=TestDB;User Id=admin;Password=1234;TrustServerCertificate=true;";

            // Create an instance of your database service implementation.
            var dbService = new ValidationDBService(connectionString);

            // Create an instance of ValidationService with the dbService instance.
            var validationService = new ValidationService(dbService);

            // Example usage of validationService.
            // Replace "example_user_identity" with a valid user identity for your tests.
            var userIdentity = "example_user_identity";
            try
            {
                // Run some methods from your services to ensure they are working as expected.
                var otp = validationService.CreateOTPAsync(userIdentity).GetAwaiter().GetResult();
                Console.WriteLine($"OTP: {otp}");

                // Assuming you have a valid value to hash and a user identity.
                var hashedValue = validationService.HashValueAsync("value_to_hash", userIdentity).GetAwaiter().GetResult();
                Console.WriteLine($"Hashed Value: {hashedValue}");
            }
            catch (Exception ex)
            {
                // If an exception is thrown, write the message to the console.
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Tests completed. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
