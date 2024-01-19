using DataAccessLibraryCraftVerify;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace NaturalSelection.UserManagement.AccountRecovery.Tests
{
    [TestFixture]
    public class RecoverAccountTests
    {
        private RecoverAccount _recoverAccount;

        [SetUp]
        public void Setup()
        {
            _recoverAccount = new RecoverAccount();
            
        }

        [Test]
        public void RecoverUserAccountTool_ValidUserId_ReturnsTrue()
        {
            // Arrange
            var validUserId = "1277320943";

            // Act
            var result = _recoverAccount.RecoverUserAccountTool(validUserId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void RecoverUserAccountTool_InvalidUserId_ReturnsFalse()
        {
            // Arrange
            var invalidUserId = "someInvalidUserId";

            // Act
            var result = _recoverAccount.RecoverUserAccountTool(invalidUserId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetDataFromConfigFile_ValidVariable_ReturnsValue()
        {
            // Arrange
            var variable = "ConnectionString";

            // Act
            var result = _recoverAccount.GetDataFromConfigFile(variable);

            // Assert
            var check = (!(string.IsNullOrEmpty(result)));
            Assert.IsNotEmpty(result);
            Assert.IsNotNull(result);
        }


        [Test]
        public void ParseConfigFile_ValidFilePath_ReturnsDictionary()
        {
            // Arrange
            var filePath = @"C:\Users\vankh\source\repos\CraftVerify.NatrualSelection.UserManagement\config.local.txt";

            // Act
            var result = _recoverAccount.ParseConfigFile(filePath);

            // Assert
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void CraftRecoverySQLCommand_ValidData_ReturnsValidQuery()
        {
            // Arrange
            var data = new Dictionary<string, object> { { "userID", "123" }, { "userStatus", 1 } };

            // Act
            var result = _recoverAccount.CraftRecoverySQLCommand(data);

            // Assert
            StringAssert.Contains("UPDATE UserAccount SET", result);
        }

        [Test]
        public void IsUserIDValid_ValidUserId_ReturnsTrue()
        {
            // Arrange
            var validUserId = "1277320943";

            // Act
            var result = _recoverAccount.IsUserIDValid(validUserId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsUserIDValid_NullUserId_ReturnsFalse()
        {
            // Arrange
            var invalidUserId = "";

            // Act
            var result = _recoverAccount.IsUserIDValid(invalidUserId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUserIDValid_ShortUserId_ReturnsFalse()
        {
            // Arrange
            var invalidUserId = "324234";

            // Act
            var result = _recoverAccount.IsUserIDValid(invalidUserId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUserIDValid_NonIntegerUserId_ReturnsFalse()
        {
            // Arrange
            var invalidUserId = "23adff12df";

            // Act
            var result = _recoverAccount.IsUserIDValid(invalidUserId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUserIDValid_EmptySpacesUserId_ReturnsFalse()
        {
            // Arrange
            var invalidUserId = "          ";

            // Act
            var result = _recoverAccount.IsUserIDValid(invalidUserId);

            // Assert
            Assert.IsFalse(result);
        }
    }


}
