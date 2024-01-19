public interface IProfileModification
{
    bool UpdateUserProfile(string userId, string newBio, byte[] newProfilePhoto);
    bool ValidateBio(string newBio);
    bool ValidateProfilePhoto(byte[] newProfilePhoto);
    bool SaveProfileChangesToDatabase(string userId, string updatedProfileData);
    void LogProfileUpdateActivity(string userId, string updateDetails);
}

// Class implementing profile modification functionalities.
public class UserModification : IProfileModification
{
    // Unique identifier of the user.
    public string UserID { get; set; }
    // URL or path of the user's profile picture.
    public string ProfilePicture { get; set; }
    // Biography or description of the user.
    public string Bio { get; set; }

    // Implementation of updating the user's profile.
    public bool UpdateUserProfile(string userId, string newBio, byte[] newProfilePhoto)
    {
        if (!ValidateBio(newBio) || !ValidateProfilePhoto(newProfilePhoto))
        {
            return false;
        }
        
        // Assume UpdateDatabase is a method that updates the user profile in the database.
        if (!SaveProfileChangesToDatabase(userId, newBio))
        {
            LogProfileUpdateActivity(userId, "Failed to update profile.");
            return false;
        }

        LogProfileUpdateActivity(userId, "Profile updated successfully.");
        return true;
    }

    // Implementation of bio validation.
    public bool ValidateBio(string newBio)
    {
        // Check bio length and content
        // The bio should be less than or equal to 200 words
        int bioWordCount = newBio.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        bool isValid = bioWordCount <= 200;
        if (!isValid)
        {
            LogProfileUpdateActivity(UserID, "Bio validation failed");
        }
        return isValid;
    }

    // Implementation of profile photo validation.
    public bool ValidateProfilePhoto(byte[] newProfilePhoto)
    {
        // Check photo format and size
        // Assume that the photo is a JPEG/PNG/HEIF format if the byte array starts with specific magic numbers.
        // JPEG starts with 0xFFD8, PNG starts with 0x89504E47, HEIF starts with 'ftyp'
        bool isValidFormat = false;
        if (newProfilePhoto.Length >= 3)
        {
            // Check for JPEG
            if (newProfilePhoto[0] == 0xFF && newProfilePhoto[1] == 0xD8)
            {
                isValidFormat = true;
            }
            // Check for PNG
            else if (newProfilePhoto[0] == 0x89 && newProfilePhoto[1] == 'P' && newProfilePhoto[2] == 'N' && newProfilePhoto[3] == 'G')
            {
                isValidFormat = true;
            }
            // Check for HEIF
            else if (newProfilePhoto[0] == 'f' && newProfilePhoto[1] == 't' && newProfilePhoto[2] == 'y' && newProfilePhoto[3] == 'p')
            {
                isValidFormat = true;
            }
        }

        bool isValidSize = newProfilePhoto.Length < 5 * 1024 * 1024; // Less than 5MB
        bool isValid = isValidFormat && isValidSize;
        
        if (!isValid)
        {
            LogProfileUpdateActivity(UserID, "Photo validation failed");
        }
        return isValid;
    }

    // Implementation of saving profile changes to the database.
    public bool SaveProfileChangesToDatabase(string userId, string updatedProfileData)
    {
        bool updateSuccess = false;
        
        // Define your connection string directly (not recommended for production code)
        string connectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

        // SQL command to update the user profile
        string sqlCommandText = "UPDATE UserProfile SET Bio = @Bio WHERE UserID = @UserID";

        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = new SqlCommand(sqlCommandText, connection))
                {
                    // Assuming 'updatedProfileData' is just the new bio for simplicity.
                    // If it's more complex, you'd need to parse it and add more parameters.
                    command.Parameters.AddWithValue("@Bio", updatedProfileData);
                    command.Parameters.AddWithValue("@UserID", userId);

                    int rowsAffected = command.ExecuteNonQuery();
                    updateSuccess = rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                LogProfileUpdateActivity(userId, $"Database update failed: {ex.Message}");
            }
        }

        if (!updateSuccess)
        {
            LogProfileUpdateActivity(userId, "Database update failed");
        }

        return updateSuccess;
    }

    // Implementation of logging profile update activities.
    public void LogProfileUpdateActivity(string userId, string updateDetails)
    {
        // Assuming a Logger class with a method Log(string) is available
        Logger.Log($"UserID: {userId}, UpdateDetails: {updateDetails}");
    }
}