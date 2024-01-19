
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.IO;


namespace CraftVerify.Natural.UserManagement
{
    public class Services
    {

        public string GetDataFromConfigFile(string variable)
        {
            // Set the full path to the config file
            var configFilePath = @"C:\Users\vanan\teamSynology\CECS 491A\Milestone_2_analysis\config.local.txt";

            //Check if the config file is exist
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException("The configuration file was not found.", configFilePath);
            }
            // Read the config file
            var config = ParseConfigFile(configFilePath);
            // Get the variable from the config file
            string v1 = config.TryGetValue(variable, out var connStr) ? connStr : string.Empty;

            if (string.IsNullOrEmpty(v1))
            {
                throw new InvalidOperationException(" This variable is not found in the configuration file.");
            }
            return v1;
        }

        // Read Config file and get value after := 
        public Dictionary<string, string> ParseConfigFile(string filePath)
        {
            var config = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(new[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    config[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return config;
        }


        // Copied the code from Professor's email and changed it to fit 2 table insertions
        public bool InsertAccountToDatabase(AccountCreation account)
        {
            // Hardcoded the queries and parameters
            // Because I think not much change gonna apply through out the life span of the program
            // I have made dynamic queries and parameters but it can slow down the process unnecessary
            var sqlUserAccount = @"
            INSERT INTO UserAccount (userID, email, userHash, userStatus, dateCreate, secureAnswer1, secureAnswer2, secureAnswer3, securityAnswerSalt)
            VALUES (@UserID, @Email, @UserHash, @UserStatus, @DateCreate, @SecureAnswer1, @SecureAnswer2, @SecureAnswer3, @SecurityAnswerSalt)";

            var sqlUserProfile = @"
            INSERT INTO UserProfile (userID, userHash, profileUserRole, profileDOB)
            VALUES (@UserID, @UserHash, @ProfileUserRole, @ProfileDOB)";

            // Create parameter sets for each SQL statement
            var userAccountParams = new HashSet<SqlParameter>
        {
            new SqlParameter("@UserID", account.userID),
            new SqlParameter("@Email", account.email),
            new SqlParameter("@UserHash", account.userHash),
            new SqlParameter("@UserStatus", account.userStatus),
            new SqlParameter("@DateCreate", account.dateCreate),
            new SqlParameter("@SecureAnswer1", account.secureAnswer1),
            new SqlParameter("@SecureAnswer2", account.secureAnswer2),
            new SqlParameter("@SecureAnswer3", account.secureAnswer3),
            new SqlParameter("@SecurityAnswerSalt", account.securityAnswerSalt),

        };

            var userProfileParams = new HashSet<SqlParameter>
        {
            new SqlParameter("@UserID", account.userID),
            new SqlParameter("@UserHash", account.userHash),
            new SqlParameter("@ProfileUserRole", account.userRole),
            new SqlParameter("@ProfileDOB", account.DOB)

        };
            string cnnstring = GetDataFromConfigFile("ConnectionString");
            if(cnnstring == null) 
            {
                return false;
            }
            // Execute both inserts in a transaction
            using (SqlConnection conn = new SqlConnection(cnnstring))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert into UserAccount first
                        using (var command = new SqlCommand(sqlUserAccount, conn, transaction))
                        {
                            command.CommandType = System.Data.CommandType.Text;
                            foreach (var parameter in userAccountParams)
                            {
                                command.Parameters.Add(parameter);
                            }
                            command.ExecuteNonQuery();
                        }

                        // Insert into UserProfile later
                        using (var command = new SqlCommand(sqlUserProfile, conn, transaction))
                        {
                            command.CommandType = System.Data.CommandType.Text;
                            foreach (var parameter in userProfileParams)
                            {
                                command.Parameters.Add(parameter);
                            }
                            command.ExecuteNonQuery();
                        }

                        // Commit transaction if both inserts succeed
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        // Roll back if there is an error
                        transaction.Rollback();
                        throw;
                        
                    }
                }
            }


        }


        #region for dynamic query and sqlParameter if later needed

        /*
        public static string BuildInsertQuery(object obj, string tableName, string attributesEndsWith)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();

            // Filter out properties that should be included in the SQL statement
            properties = properties.Where(p => ShouldIncludeInSql(p, attributesEndsWith)).ToArray();

            var columnNames = properties.Select(p => p.Name);
            var parameterNames = properties.Select(p => "@" + p.Name);

            var columns = string.Join(", ", columnNames);
            var parameters = string.Join(", ", parameterNames);

            return $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
        }

        private static bool ShouldIncludeInSql(PropertyInfo propertyInfo, string endsWith)
        {
            // Attributes with string endsWith will be included
            return propertyInfo.Name.EndsWith("endsWith");
        
        }

        public static HashSet<SqlParameter> BuildSqlParameters(object obj, string attributesEndsWith)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();

            // Filter out properties that should be included in the SQL statement
            properties = properties.Where(p => ShouldIncludeInSql(p, attributesEndsWith)).ToArray();

            var parameters = new HashSet<SqlParameter>();
            foreach (var property in properties)
            {
                parameters.Add(new SqlParameter("@" + property.Name, property.GetValue(obj) ?? DBNull.Value));
            }

            return parameters;
        }

        */
        #endregion

        public bool IsValidEmail(string email)
        {
            /*
             •	Valid email consists of:
                i.	Minimum of 3 characters
                ii.	Must be in the format: <valid_characters>@<valid_characters>
                iii.	a-z (case insensitive)
                iv.	0-9
                v.	May have special characters 
                    1.	.
                    2.	-
             */

            // allowedEmailLength = 64
            // emailMinLength = 8 if we use email as username
            // allowedEmailPattern  should be = @"^[a-zA-Z0-9@.-]*$"



            return (!IsNullString(email) && IsValidLength(email, 8, 64) && IsValidDigit(email, @"^[a-zA-Z0-9@.-]*$") && IsValidPosition(email));
        }



        public bool IsValidSecureAnswers(string answer)
        {
            

            // allowedLength = 50
            // MinLength = 3 if we use email as username
            // allowedPattern  should be = @"^[a-zA-Z0-9@. -]*$"



            return (!IsNullString(answer) && IsValidLength(answer, 3, 50) && IsValidDigit(answer, @"^[a-zA-Z0-9@. -]*$"));
        }

        public bool IsValidDate(DateTime dobString, DateTime minDate, DateTime maxDate)
        {
            /*
             Valid date of births begins January 1st, 1970 and ends current date.
             minDate = DateTime(1970, 1, 1);
             maxDate = DateTime.Now;
         
             */
           
            
                return (dobString >= minDate && dobString <= maxDate);           // Check if the parsed date falls within the valid range
                                                     // If parsing fails, the input is not a valid date
        }

        public bool IsNullString(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public bool IsValidLength(string email, int minLength, int maxLength)
        {
            if (IsNullString(email))
            {
                return false; // null string is considered invalid
            }

            int length = email.Length;
            return length >= minLength && length <= maxLength;
        }

        public bool IsValidDigit(string email, string allowedEmailPattern)
        {
            if (IsNullString(email))
            {
                return false; // Invalid if the email is null or empty
            }

            return Regex.IsMatch(email, allowedEmailPattern);
        }

        public bool IsValidPosition(string email)
        {
            if (IsNullString(email))
            {
                return false; // Invalid if the email is null or empty
            }

            int atIndex = email.IndexOf('@');

            // Check if '@' is not at the start, not at the end, and occurs only once
            return atIndex > 0 && atIndex < email.Length - 1 && email.LastIndexOf('@') == atIndex;
        }

        public int GenerateUserID(int numDigit)
        {
            byte[] randomNumber = new byte[4]; // int is 4 bytes
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomNumber);
            }

            // Convert byte array to an int
            int value = BitConverter.ToInt32(randomNumber, 0);

            // Ensure a positive number that's within the range of 10 digits
            
            int minValue = (int)Math.Pow(10, numDigit - 1);
            int maxValue = (int)Math.Pow(10, numDigit) - 1;

            return Math.Abs(value % (maxValue - minValue)) + minValue;
        }

        public string ReadPepper(string filePath)
        {
            try
            {
                // Assuming the file contains only the pepper string
                return File.ReadAllText(filePath);
            }
            catch (IOException e)
            {
                // Handle or log the exception as needed
                Console.WriteLine("An error occurred while reading the file: " + e.Message);
                return string.Empty; // Or handle the error as appropriate
            }
        }

        public string HashSHA256(string email, string nullSalt, string pepper)
        {
            // Combine the email and pepper. The nullSalt is not used.
            string combinedString = email + nullSalt + pepper;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute the hash of the combined string.
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedString));

                // Convert the byte array to a hexadecimal string.
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public string CreateEmailPepperHash(string email, string filePath)
        {
            return HashSHA256(email, "", ReadPepper(filePath));
        }

        public bool IsValidUserRole(string roleString)
        {
            // role would be "admin" or "regular" length from 5 to 10 with @"^[a-zA-Z0-9]*$" as pattern
            return (!IsNullString(roleString) && IsValidLength(roleString, 5, 10) && IsValidDigit(roleString, @"^[a-zA-Z0-9]*$"));
        }

        



    }
}
