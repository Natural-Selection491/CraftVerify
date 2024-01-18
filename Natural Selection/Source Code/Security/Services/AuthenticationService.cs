
using ValidationService;
using System.Security.Claims;
using System;
using System.Data.SqlClient;

public class AuthenticationService : IRoleAuthenticator
{ 
	public bool hasError;
	public bool canRetry;
	public ClaimsPrincipal principal;
	public string principalHash;

	public ClaimsPrincipal Authenticate(AuthenticationRequest authRequest)
	{
		//set AuthenticateionRequest variables
		userIdentity = authRequest.UserIdentity
		proof? = authRequest.Proof
		VS = ValidationService(UserIdentity = userIdentity, Proof = proof)

		// TLDR: validate input, check exists, check not authenticated
		// If Submitted userIdentity is not null and in a valid format and If Submitted userIdentity exists in email column in DB UserAccount table and If Submitted userIdentity is not already authenticated
		if (ValidateUserIdentityInput(UserIdentity) & CheckUserIdentityExists(UserIdentity) & CheckUserNotAlreadyAuthenticated(UserIdentity)) is true:
		{
			continue
		}
		else:
		{
			A system message displays “Invalid security credentials provided. Retry again or contact system administrator”

			hasError = true

			return false

		}

		// If submitted userIdentity account userStatus is 1 inside UserAccount table inside DB
		if CheckUserStatusEnabled(UserIdentity):
		{
			continue

		}
		else:
		{
			A system message displays “Account is disabled. Perform account recovery first or contact system administrator”


			canRetry = false

			return false
		}

		//check to see if user has not failed before 
		//check the User Account table in the DB to see if FirstAuthenticationFailTimestamp is null
		if CheckFirstAuthenticationFailTimestampNull(UserIdentity) is true:
		{
			//early exit 
			continue

		}
		//check if user authentication failures needs reset 
		else:
		{
			// Check if firstAuthenticationFailTimestamp in DB userAccount table is greater than 24 hours from current timeStamp and failedAuthenticationAttempts less than 3
			if (CheckFirstAuthenticationFailTimestampNeedsReset(UserIdentity) and GetFailedAuthenticationAttempts(UserIdentity) < 3):
			{
				// Set firstAuthenticationFailTimestamp to null and set failedAuthenticationAttempts to 0 in UserAccount table in sql Database
				ResetAuthenticationFailures(userIdentity)

				// set canRetry to true
				hasError = false
				canRetry = true

				continue
			}
			else:
			{
				continue
			}
		}


		// Check if OTP expired by comparing otpTimestamp in DB with the current timestamp
		if CheckOTPExpired(userIdentity) is true:
		{
			// set otpTimestamp and otpHash to null
			ResetOTP(userIdentity)
			continue
		}

		//checkOTPCreation
		// If otpHash is null in DB create OTP
		otpHash = GetOTPHash(userIdenity)

		if otpHash is null:
		{
			VS.CreateOTP(UserIdentity)

			return true
		}
		//otpHash exists
		else:
		{
			// ValidateOTP hashes the proof and compares that value to the otpHash in the UserAccount table inside the database 
			if otpHash == VS.HashValue(proof) is true:
			{
				//Set firstAuthenticationFailTimestamp to null and set failedAuthenticationAttempts to 0 in UserAccount table in sql Database
				ResetAuthenticationFailures(userIdentity)


				//create claims principal using userRole from DB userAcc table, userIdentity
				appPrincipal = new ClaimsPrincipal using microsoft.system.security.claimsprincipal
					//get userRole from userProfile table in DB
					userRole = GetUserProfileRole(userIdentity)

					claims = new Claim(ClaimTypes.Role.ToString(), userRole)
					//appPrincipal.AddIdentity(userIdentity)
					identity = userIdentity

				//create hash principal
				principalHash = VS.HashValue(appPrincipal)

				//insert hashPrincipal into db ClaimsPrincipalHash table
				InsertHashPrincipal(userIdentity, principalHash)

				return appPrincipal
			}

			// Else If ValidateOTP fails 
			else:
			{
				// If this is first failed authentication attempt
				//if firstAuthenticationFailTimestamp is null in UserAccount table in sql Database
				if CheckFirstAuthenticationFailTimestampNull(UserIdentity) is true:
				{
					//insert firstAuthenticationFailTimestamp in UserAccount table in sql Database to the current timestamp
					currentTimestamp = new timestamp()

					InsertFirstAuthenticationFailTimestamp(userIdentity, CurrentTimestamp)

					// set failedAuthenticationAttempts to 1 in sql Database
					IncrementFailedAuthenticationAttempts(UserIdentity)

					// set canRetry to true
					hasError = true
					canRetry = true

					// For each failed attempt, the account undergoing authentication and the IP address that initiated the authentication request will be recorded.

				}

				// Else user has already failed an authentication before then increment failedAuthenticationAttempts in DB UserAccount table by 1
				else
				{
					// Increment failedAuthenticationAttempts to add 1 in sql Database
					IncrementFailedAuthenticationAttempts(UserIdentity)

					// For each failed attempt, the account undergoing authentication and the IP address that initiated the authentication request will be recorded.

					// If user has reached max failed authentication attempts call AccountDisable from the imported UserManagement Library
					if (GetFailedAuthenticationAttempts(UserIdentity) >= 3)
					{
						//call AccountDisable from the imported UserManagement Library
						UserManagement.DisableAccount(GetUserHash(UserIdentity))

						//set canRetry to false
						hasError = true
						canRetry = false

					}
				}
			}
		}
	}

	// Submitted userIdentity is not null and in a valid format, return true
	private bool ValidateUserIdentityInput(string userIdentity)
	{
		/*Valid usernames consist of:
		 * Minimum of 8 characters
		 * a-z (case insensitive)
		 * 0-9
		 * Allow the following special characters:
		 * .
		 * –
		 * @ */
	}

	// Check the UserAccount table in the DB where userIdentity = email exists in email column, return true
	private bool CheckUserIdentityExists(string userIdentity)
	{

	}

	// Check the ClaimsPrincipalHash table in the DB where userIdentity = identity if it doesnt exist, return true
	private bool CheckUserNotAlreadyAuthenticated(string userIdentity)
	{

	}

	// Check the UserAccount table in the DB where userIdentity = email, if userStatus is 1, return true
	private bool CheckUserStatusEnabled(string userIdentity)
	{

	}

	// Check the UserAccount table in the DB where userIdentity = email, if firstAuthenticationFailTimestamp is null, returm true
	private bool CheckFirstAuthenticationFailTimestampNull(string userIdentity)
	{

	}

	// Check the UserAccount table in the DB where userIdentity = email, if current Timestmap is greater than 24 hours from firstAuthenticationFailTimestamp, return true
	private bool CheckFirstAuthenticationFailTimestampNeedsReset(string userIdentity)
	{
		current = new timestamp()
		otpTimestamp = GetFirstAuthenticationFailTimestamp(userIdentity)

		// if current Timestmap is greater than 24 hours from firstAuthenticationFailTimestamp, return true

	}

	// from UserAccount table in DB return firstAuthenticationFailTimestamp where userIdentity = email
	private timestamp GetFirstAuthenticationFailTimestamp(string userIdentity)
	{
		return firstAuthenticationFailTimestamp
	}

	// From the UserAccount table in DB where userIdentity = email return number of failedAuthenticationAttempts
	private int GetFailedAuthenticationAttempts(string userIdentity)
	{
		return failedAuthenticationAttempts
	}

	// Set firstAuthenticationFailTimestamp to null and set failedAuthenticationAttempts to 0 in UserAccount table inside DB, return true
	private bool ResetAuthenticationFailures(string userIdentity)
	{

	}

	// Check if OTP expired by comparing otpTimestamp in DB UserAccount table with the current timestamp, if the current timestamp is greater than 2 minutes, return true
	private bool CheckOTPExpired(string userIdentity)
	{
		current = new timestamp()
		otpTimestamp = GetOTPTimestamp(userIdentity)

		//if current is greater than otpTimestamp by 2 minutes then return true

	}

	// from UserAccount table in DB return otpTimestamp where userIdentity = email
	private timestamp GetOTPTimestamp(string userIdentity)
	{
		return otpTimestamp
	}

	// set otpTimestamp and otpHash to null inside UserAccount table in DB where userIdentity = email
	private bool ResetOTP(string userIdentity)
	{

	}

	// From UserAccount table in DB return otpHash where userIdentity = email,
	private bool GetOTPHash(string userIdentity)
	{

	}

	// From UserProfile table in DB, where userID = userID where email in UserAccount = userIdentity, return userRole
	private string GetUserProfileRole(string userIdentity)
	{
		return userProfileRole

	}

	// Insert into ClaimsPrincipalHash table in DB identity and principalHash column, return true
	private bool InsertHashPrincipal(string userIdentity, string principalHash)
	{

	}	

	// Insert SSinto UserAccount table in DB, where userIdentity = email, the passed timestamp into firstAuthenticationFailTimestamp, return true
	private bool InsertFirstAuthenticationFailTimestamp(string userIdentity, DateTime currentTimestamp)
	{

	}

	// From the UserAccount table in the DB, where userIdentity = email, increment failedAuthenticationAttempts by 1, return true
	private bool IncrementFailedAuthenticationAttempts(string userIdentity)
	{

	}

}

