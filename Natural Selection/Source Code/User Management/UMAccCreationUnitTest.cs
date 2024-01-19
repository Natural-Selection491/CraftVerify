using NUnit.Framework;
namespace CraftVerify.Natural.UserManagement.AccCreateUnTest
{
    public class Tests
    {
        [TestFixture]
        public class ServicesTests
        {
            private Services _services;
            private DateTime _minDate;
            private DateTime _maxDate;
            private string _validFilePath;
            private string _invalidFilePath;
            private string _testFilePath;
            private string _testPepper = "TestPepper";
            private string _tempConfigFilePath;
            private string _tempConfigFilePath2;


            [SetUp]
            public void Setup()
            {
                _services = new Services();
                _minDate = new DateTime(1970, 1, 1); // Assuming these are the bounds used in the method
                _maxDate = DateTime.Now;
                _validFilePath = "validFilePath.txt"; // Replace with a path to an actual file
                _invalidFilePath = "nonExistentFilePath.txt"; // Path to a non-existent file

                // Create a file with sample content for testing
                File.WriteAllText(_validFilePath, "SamplePepperString");
                // Set up a temporary file for testing
                _testFilePath = Path.GetTempFileName();
                File.WriteAllText(_testFilePath, _testPepper);
                // Create a temporary configuration file for testing
                _tempConfigFilePath = Path.GetTempFileName();
                File.WriteAllText(_tempConfigFilePath, "ConnectionString:=TestConnectionString\nTimeout:=30");
                _tempConfigFilePath2 = Path.GetTempFileName();
                File.WriteAllLines(_tempConfigFilePath, new[] {
                "ConnectionString:=TestConnectionString", "Timeout:=30", "EmptyValue:="});

            }

            [Test]
            public void IsNullString_WithNullString_ReturnsTrue()
            {
                // Act
                bool result = _services.IsNullString(null);

                // Assert
                Assert.IsTrue(result, "Expected to return true for null string.");
            }

            [Test]
            public void IsNullString_WithEmptyString_ReturnsTrue()
            {
                // Act
                bool result = _services.IsNullString(string.Empty);

                // Assert
                Assert.IsTrue(result, "Expected to return true for empty string.");
            }

            [Test]
            public void IsNullString_WithNonEmptyString_ReturnsFalse()
            {
                // Act
                bool result = _services.IsNullString("Test String");

                // Assert
                Assert.IsFalse(result, "Expected to return false for non-empty string.");
            }

 //IsValidLength
            [Test]
            public void IsValidLength_NullString_ReturnsFalse()
            {
                bool result = _services.IsValidLength(null, 5, 10);
                Assert.IsFalse(result, "Expected to return false for null string.");
            }

            [Test]
            public void IsValidLength_EmptyString_ReturnsFalse()
            {
                bool result = _services.IsValidLength(string.Empty, 5, 10);
                Assert.IsFalse(result, "Expected to return false for empty string.");
            }

            [Test]
            public void IsValidLength_ShorterThanMinLength_ReturnsFalse()
            {
                bool result = _services.IsValidLength("abc", 5, 10); // "abc" length is 3
                Assert.IsFalse(result, "Expected to return false for string shorter than min length.");
            }

            [Test]
            [TestCase("abcdefghijk", 5, 10)] // 11 characters
            [TestCase("abcdefghijklmnopqrstuvwxyz", 5, 10)] // 26 characters
            public void IsValidLength_LongerThanMaxLength_ReturnsFalse(string email, int minLength, int maxLength)
            {
                bool result = _services.IsValidLength(email, minLength, maxLength);
                Assert.IsFalse(result, $"Expected to return false for string '{email}' longer than max length.");
            }

            [Test]
            [TestCase("abcde", 5, 10)] // Exactly 5 characters
            [TestCase("abcdefghij", 5, 10)] // Exactly 10 characters
            [TestCase("abcdef", 5, 10)] // 6 characters
            public void IsValidLength_WithinValidLengthRange_ReturnsTrue(string email, int minLength, int maxLength)
            {
                bool result = _services.IsValidLength(email, minLength, maxLength);
                Assert.IsTrue(result, $"Expected to return true for string '{email}' within valid length range.");
            }
 //IsValidDigit
            [Test]
            public void IsValidDigit_NullString_ReturnsFalse()
            {
                string pattern = @"^[a-zA-Z0-9]*$";
                bool result = _services.IsValidDigit(null, pattern);
                Assert.IsFalse(result, "Expected to return false for null string.");
            }

            [Test]
            public void IsValidDigit_EmptyString_ReturnsFalse()
            {
                string pattern = @"^[a-zA-Z0-9]*$";
                bool result = _services.IsValidDigit(string.Empty, pattern);
                Assert.IsFalse(result, "Expected to return false for empty string.");
            }

            [Test]
            [TestCase("validEmail123", @"^[a-zA-Z0-9]*$", true)]
            [TestCase("invalidEmail@", @"^[a-zA-Z0-9]*$", false)]
            [TestCase("anotherValidEmail", @"^[a-zA-Z0-9]*$", true)]
            [TestCase("invalid-Email", @"^[a-zA-Z0-9]*$", false)]
            public void IsValidDigit_WithVariousEmailsAndPatterns(string email, string pattern, bool expected)
            {
                bool result = _services.IsValidDigit(email, pattern);
                Assert.AreEqual(expected, result, $"Expected '{expected}' for email '{email}' with pattern '{pattern}'.");
            }

            [Test]
            public void IsValidDigit_WithInvalidPattern_ReturnsFalse()
            {
                string email = "validEmail123";
                string invalidPattern = "[Invalid-Regex";
                Assert.Throws<ArgumentException>(() => _services.IsValidDigit(email, invalidPattern), "Expected an ArgumentException for invalid regex pattern.");
            }

//IsValidPosition
            [Test]
            public void IsValidPosition_NullString_ReturnsFalse()
            {
                bool result = _services.IsValidPosition(null);
                Assert.IsFalse(result, "Expected to return false for null string.");
            }

            [Test]
            public void IsValidPosition_EmptyString_ReturnsFalse()
            {
                bool result = _services.IsValidPosition(string.Empty);
                Assert.IsFalse(result, "Expected to return false for empty string.");
            }

            [Test]
            [TestCase("test@example.com", true)]
            [TestCase("@test.com", false)]
            [TestCase("test@com", false)]
            [TestCase("test@test@test.com", false)]
            [TestCase("test@.com", false)]
            [TestCase("test.com@", false)]
            [TestCase("test@example.com@", false)]
            [TestCase("test@.example.com", false)]
            public void IsValidPosition_WithVariousEmails(string email, bool expected)
            {
                bool result = _services.IsValidPosition(email);
                Assert.AreEqual(expected, result, $"Expected '{expected}' for email '{email}'.");
            }

//GenerateUserID
            [Test]
            public void GenerateUserID_ReturnsPositiveNumber()
            {
                int userID = _services.GenerateUserID(5);
                Assert.Greater(userID, 0, "UserID should be a positive number.");
            }

            [Test]
            public void GenerateUserID_ReturnsNumberWithinRange()
            {
                int idLength = 10;
                int minValue = (int)Math.Pow(10, idLength - 1);
                int maxValue = (int)Math.Pow(10, idLength) - 1;

                int userID = _services.GenerateUserID(10);
                Assert.GreaterOrEqual(userID, minValue, "UserID should be greater or equal to the minimum value.");
                Assert.LessOrEqual(userID, maxValue, "UserID should be less or equal to the maximum value.");
            }

            [Test]
            public void GenerateUserID_ReturnsConsistentLength()
            {
                int idLength = 10;

                int userID = _services.GenerateUserID(10);
                int lengthOfUserID = userID.ToString().Length;

                Assert.AreEqual(idLength, lengthOfUserID, "UserID should have a consistent length of 10 digits.");
            }

            

 //IsValidDate

            [Test]
            public void IsValidDate_BeforeMinDate_ReturnsFalse()
            {
                DateTime dob = new DateTime(1960, 1, 1); // Before min date
                bool result = _services.IsValidDate(dob, _minDate, _maxDate);
                Assert.IsFalse(result, "Expected to return false for date before min date.");
            }

            [Test]
            public void IsValidDate_AfterMaxDate_ReturnsFalse()
            {
                DateTime dob = DateTime.Now.AddDays(1); // After max date
                bool result = _services.IsValidDate(dob, _minDate, _maxDate);
                Assert.IsFalse(result, "Expected to return false for date after max date.");
            }

            [Test]
            public void IsValidDate_WithinValidRange_ReturnsTrue()
            {
                DateTime dob = new DateTime(1980, 1, 1); // Within the valid range
                bool result = _services.IsValidDate(dob, _minDate, _maxDate);
                Assert.IsTrue(result, "Expected to return true for date within valid range.");
            }

            [Test]
            public void IsValidDate_OnMinDate_ReturnsTrue()
            {
                bool result = _services.IsValidDate(_minDate, _minDate, _maxDate);
                Assert.IsTrue(result, "Expected to return true for date equal to min date.");
            }

            [Test]
            public void IsValidDate_OnMaxDate_ReturnsTrue()
            {
                bool result = _services.IsValidDate(_maxDate, _minDate, _maxDate);
                Assert.IsTrue(result, "Expected to return true for date equal to max date.");
            }

 //ReadPepper
            [Test]
            public void ReadPepper_ValidFilePath_ReturnsContent()
            {
                string result = _services.ReadPepper(_validFilePath);
                Assert.AreEqual("SamplePepperString", result, "Expected to return the content of the file.");
            }

            [Test]
            public void ReadPepper_InvalidFilePath_ReturnsEmptyString()
            {
                string result = _services.ReadPepper(_invalidFilePath);
                Assert.AreEqual(string.Empty, result, "Expected to return an empty string for an invalid file path.");
            }


 //IsValidEmail
            [Test]
            public void IsValidEmail_ValidEmail_ReturnsTrue()
            {
                Assert.IsTrue(_services.IsValidEmail("test@example.com"));
                Assert.IsTrue(_services.IsValidEmail("user.name-1234@domain.co.uk"));
            }

            [Test]
            public void IsValidEmail_InvalidEmail_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidEmail("invalid-email"));
                Assert.IsFalse(_services.IsValidEmail("invalid@.com"));
                Assert.IsFalse(_services.IsValidEmail("invalid@domain"));
                Assert.IsFalse(_services.IsValidEmail("@nodomain.com"));
                Assert.IsFalse(_services.IsValidEmail("no@tld"));
            }

            [Test]
            public void IsValidEmail_ShortEmail_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidEmail("a@b.c"));
            }

            [Test]
            public void IsValidEmail_LongEmail_ReturnsFalse()
            {
                string longEmail = new string('a', 65) + "@example.com";
                Assert.IsFalse(_services.IsValidEmail(longEmail));
            }

            [Test]
            public void IsValidEmail_NullOrEmpty_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidEmail(null));
                Assert.IsFalse(_services.IsValidEmail(string.Empty));
            }

 //IsValidSecureAnswers
            [Test]
            public void IsValidSecureAnswers_ValidAnswer_ReturnsTrue()
            {
                Assert.IsTrue(_services.IsValidSecureAnswers("Valid Answer 123."));
                Assert.IsTrue(_services.IsValidSecureAnswers("Another-valid-answer@"));
            }

            [Test]
            public void IsValidSecureAnswers_InvalidAnswer_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidSecureAnswers("No"));
                Assert.IsFalse(_services.IsValidSecureAnswers(new string('a', 51))); // 51 characters long
                Assert.IsFalse(_services.IsValidSecureAnswers("Invalid^&*Characters"));
            }

            [Test]
            public void IsValidSecureAnswers_NullOrEmpty_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidSecureAnswers(null));
                Assert.IsFalse(_services.IsValidSecureAnswers(string.Empty));
            }

            [Test]
            public void IsValidSecureAnswers_SpecialCharacters_ReturnsTrue()
            {
                Assert.IsTrue(_services.IsValidSecureAnswers("Answer with . and -"));
            }

 //HashSHA256
            [Test]
            public void HashSHA256_WithKnownInputs_ReturnsExpectedHash()
            {
                string email = "user@example.com";
                string nullSalt = ""; // Unused parameter
                string pepper = "pepperString";

                string result = _services.HashSHA256(email, nullSalt, pepper);

                // Expected hash for the combined string "user@example.compepperString"
                string expectedHash = "YourExpectedHashHere"; // Replace with the actual expected hash

                Assert.AreEqual(expectedHash, result);
            }

            [Test]
            public void HashSHA256_WithDifferentInputs_ReturnsDifferentHashes()
            {
                string email1 = "user@example.com";
                string email2 = "anotheruser@example.com";
                string nullSalt = ""; // Unused parameter
                string pepper = "pepperString";

                string hash1 = _services.HashSHA256(email1, nullSalt, pepper);
                string hash2 = _services.HashSHA256(email2, nullSalt, pepper);

                Assert.AreNotEqual(hash1, hash2, "Hashes should be different for different inputs.");
            }

            [Test]
            public void HashSHA256_WithEmptyEmailAndPepper_ReturnsConsistentHash()
            {
                string email = "";
                string nullSalt = ""; // Unused parameter
                string pepper = "";

                string result = _services.HashSHA256(email, nullSalt, pepper);
                string expectedHash = "YourExpectedHashForEmptyString"; // Replace with the actual hash

                Assert.AreEqual(expectedHash, result);
            }

//IsValidUserRole
            [Test]
            public void IsValidUserRole_ValidRole_ReturnsTrue()
            {
                Assert.IsTrue(_services.IsValidUserRole("admin"));
                Assert.IsTrue(_services.IsValidUserRole("regular"));
            }

            [Test]
            public void IsValidUserRole_InvalidRole_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidUserRole("adm")); // Too short
                Assert.IsFalse(_services.IsValidUserRole("administrator")); // Too long
                Assert.IsFalse(_services.IsValidUserRole("adm!n")); // Contains invalid character
                Assert.IsFalse(_services.IsValidUserRole(" ")); // Invalid role
            }

            [Test]
            public void IsValidUserRole_NullOrEmpty_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidUserRole(null));
                Assert.IsFalse(_services.IsValidUserRole(string.Empty));
            }

            [Test]
            public void IsValidUserRole_WithSpecialCharacters_ReturnsFalse()
            {
                Assert.IsFalse(_services.IsValidUserRole("admin123"));
                Assert.IsFalse(_services.IsValidUserRole("user-role"));
            }

//CreateEmailPepperHash
            [Test]
            public void CreateEmailPepperHash_WithValidInputs_ReturnsExpectedHash()
            {
                string email = "user@example.com";

                // Expected hash of "user@example.comTestPepper"
                string expectedHash = "ExpectedHashHere"; // Replace with the actual expected hash

                string result = _services.CreateEmailPepperHash(email, _testFilePath);
                Assert.AreEqual(expectedHash, result);
            }

            [Test]
            public void CreateEmailPepperHash_WithInvalidFilePath_ThrowsException()
            {
                string email = "user@example.com";
                string invalidFilePath = "nonexistentpath.txt";

                Assert.Throws<IOException>(() => _services.CreateEmailPepperHash(email, invalidFilePath));
            }

           

 //GetDataFromConfigFile
            [Test]
            public void GetDataFromConfigFile_ExistingVariable_ReturnsValue()
            {
                string result = _services.GetDataFromConfigFile("ConnectionString");
                Assert.AreEqual("TestConnectionString", result);
            }

            [Test]
            public void GetDataFromConfigFile_NonExistingVariable_ThrowsInvalidOperationException()
            {
                Assert.Throws<InvalidOperationException>(
                    () => _services.GetDataFromConfigFile("NonExistingVariable")
                );
            }

            [Test]
            public void GetDataFromConfigFile_NonExistingFile_ThrowsFileNotFoundException()
            {
                string nonExistingFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Assert.Throws<FileNotFoundException>(
                    () => _services.GetDataFromConfigFile("ConnectionString")
                );
            }

            //
            [Test]
            public void ParseConfigFile_ValidFile_ReturnsCorrectDictionary()
            {
                Dictionary<string, string> config = _services.ParseConfigFile(_tempConfigFilePath2);

                Assert.AreEqual(3, config.Count);
                Assert.AreEqual("TestConnectionString", config["ConnectionString"]);
                Assert.AreEqual("30", config["Timeout"]);
                Assert.AreEqual("", config["EmptyValue"]);  // Assuming you want to handle empty values this way
            }

            [Test]
            public void ParseConfigFile_NonExistingFile_ThrowsFileNotFoundException()
            {
                string nonExistingFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Assert.Throws<FileNotFoundException>(() => _services.ParseConfigFile(nonExistingFilePath));
            }


            [TearDown]
            public void TearDown()
            {
                // Clean up the test file
                if (File.Exists(_testFilePath))
                {
                    File.Delete(_testFilePath);
                }

                // Clean up: Delete the test file if it exists
                if (File.Exists(_validFilePath))
                {
                    File.Delete(_validFilePath);
                }
                // Clean up the temporary configuration file
                if (File.Exists(_tempConfigFilePath))
                {
                    File.Delete(_tempConfigFilePath);
                }

                // Clean up the temporary configuration file 2
                if (File.Exists(_tempConfigFilePath2))
                {
                    File.Delete(_tempConfigFilePath2);
                }
            }





        }

        //AccountCreation
        [TestFixture]
        public class AccountCreationTests
        {
            [Test]
            public void Constructor_SetsDefaultValues()
            {
                var account = new AccountCreation();

                Assert.AreEqual("regular", account.userRole);
                Assert.IsTrue(account.userStatus);
                Assert.IsNotNull(account.AdditionalAttributes);
                Assert.AreEqual(0, account.AdditionalAttributes.Count);
                // Testing dateCreate for being recent (within a few seconds of now)
                Assert.Less((DateTime.Now - account.dateCreate).TotalSeconds, 5);
            }

            [Test]
            public void SetAndGet_AdditionalAttributes_ReturnsCorrectValues()
            {
                var account = new AccountCreation();
                account.SetAdditionalAttribute("key1", "value1");
                account.SetAdditionalAttribute("key2", 12345);

                Assert.AreEqual("value1", account.GetAdditionalAttribute("key1"));
                Assert.AreEqual(12345, account.GetAdditionalAttribute("key2"));
                Assert.IsNull(account.GetAdditionalAttribute("nonexistentkey"));
            }

            [Test]
            public void PropertyAssignments_StoreAndRetrieveValues()
            {
                var account = new AccountCreation
                {
                    userID = 1,
                    userHash = "hash",
                    email = "user@example.com",
                    DOB = new DateTime(2000, 1, 1),
                    userStatus = false,
                    secureAnswer1 = "Answer1",
                    secureAnswer2 = "Answer2",
                    secureAnswer3 = "Answer3",
                    securityAnswerSalt = "salt",
                    firstAuthenticationFailTimestamp = new DateTime(2021, 1, 1),
                    failedAuthenticationAttempts = 3
                };

                Assert.AreEqual(1, account.userID);
                Assert.AreEqual("hash", account.userHash);
                
            }

            [Test]
            public void DisplayAccountInfo_WritesExpectedOutputToConsole()
            {
                var account = new AccountCreation
                {
                    userID = 1,
                    email = "test@example.com"
                    
                };
                account.SetAdditionalAttribute("additionalKey", "additionalValue");

                using (var sw = new StringWriter())
                {
                    Console.SetOut(sw);
                    account.DisplayAccountInfo();

                    var result = sw.ToString();
                    Assert.IsTrue(result.Contains("Account Information:"));
                    Assert.IsTrue(result.Contains("userID: 1"));
                    Assert.IsTrue(result.Contains("email: test@example.com"));
                    Assert.IsTrue(result.Contains("additionalKey: additionalValue"));
                    
                }
            }
        }
    }






    
    
}