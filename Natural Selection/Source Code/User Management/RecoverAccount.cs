using DataAccessLibraryCraftVerify;
using System.Diagnostics;
using System.Linq;
namespace NaturalSelection.UserManagement.AccountRecovery
{
    public class RecoverAccount
    {
        private SQLServerDAO _sqlServerDAO = new SQLServerDAO();

        /// <summary>
        /// This Is The Constructor for RecoverAccount and create a new SQLServerDAO
        /// </summary>
        public RecoverAccount()
        {
        }

        /// <summary>
        /// This Method Is From Professor Vatanak Vong For Retrieving Information From Config File
        /// </summary>
        /// <param name="variable">The string represent the name of information.</param>
        /// <returns>The Value Of The Variable From Config File</returns>
        public string GetDataFromConfigFile(string variable)
        {
            // Set the full path to the config file
            var configFilePath = @"C:\Users\vankh\source\repos\CraftVerify.NatrualSelection.UserManagement\config.local.txt";

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

        /// <summary>
        /// This Method Is From Professor Vatanak Vong For Retrieving Information And Getting Value After := From Config File 
        /// </summary>
        /// <param name="filePath">The File Path of The Text File</param>
        /// <returns>Return The Key and Value Of The Stored Information</returns>
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

        /// <summary>
        /// This Method Use SQLServerDAO To Disable The User
        /// </summary>
        /// <param name="filePath">The File Path of The Text File</param>
        /// <returns>Return Whether The USer Is Successfully Recovered</returns>
        public bool RecoverUserAccountTool(string userID)
        {
            try
            {
                
                if (!IsUserIDValid(userID))
                {
                    Console.WriteLine("Invalid User Hash Format");
                    return false;
                }
                else
                {
                    Stopwatch stopwatch = new Stopwatch();
                    var connString = GetDataFromConfigFile("ConnectionString");
                    Dictionary<string, object> recoverColumnValue = new Dictionary<string, object> {
                        { "userID", string.Format("{0}", userID)},
                        { "userStatus", 1 }
                    };
                    stopwatch.Start();
                    var rowsAffected = _sqlServerDAO.InsertAttribute(connString, CraftRecoverySQLCommand(recoverColumnValue));
                    stopwatch.Stop();
                    TimeSpan tsCompleteRecovery = stopwatch.Elapsed;
                    if ((rowsAffected > 0) && (tsCompleteRecovery.TotalSeconds <= 3))
                    {
                        Console.WriteLine("Account Recovered Successfully!");
                        Console.WriteLine("Please Contact The System Admin If You Did Not Get Access After Recovery");
                        return true;
                    }
                    else if ((rowsAffected > 0) && (tsCompleteRecovery.TotalSeconds < 3))
                    {
                        stopwatch.Restart();
                        stopwatch.Start();
                        Console.WriteLine("Recover Takes More Than 3 Second To Complete!");
                        stopwatch.Stop();
                        TimeSpan tsDisplayMessage = stopwatch.Elapsed;
                        if (tsDisplayMessage.TotalSeconds <= 3)
                        {
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Error Message Takes More Than 3 Second To Display");
                            return false;
                        }
                    }
                    else { return false; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error(RecoverAccount): {ex}");
                return false;
            }


        }

        /// <summary>
        /// This Method Is For Crafting The Query Command For Disabling User 
        /// </summary>
        /// <param name="newValues">The Values To Update The UserAccount Table</param>
        /// <returns>Return The SQL Query For Disable User</returns>
        public string CraftRecoverySQLCommand(Dictionary<string, object> newValues)
        {
            try
            {
                if (!(newValues != null && newValues.Keys.All(key => key is string)))
                {
                    throw new ArgumentException("Null or Invalid Dictionary");
                }
                else
                {
                    var whereCommand = "WHERE ";
                    string temp;
                    var finalCommand = "UPDATE UserAccount SET ";
                    for (int i = 1; i < newValues.Count; i++)
                    {

                        temp = string.Format("{0} = {1}", newValues.ElementAt(i).Key, newValues.ElementAt(i).Value);
                        finalCommand = string.Concat(finalCommand, temp);
                        if ((i > 0) && (i < newValues.Count - 1))
                        {
                            temp = string.Concat(", ");
                            finalCommand = string.Concat(finalCommand, temp);
                        }
                        else if (i == newValues.Count - 1)
                        {
                            temp = string.Concat(" ");
                            finalCommand = string.Concat(finalCommand, temp);
                        }
                    }
                    temp = string.Format("{0} = {1}", newValues.ElementAt(0).Key, newValues.ElementAt(0).Value);
                    finalCommand = string.Concat(finalCommand, whereCommand, temp);
                    return finalCommand;
                }

            }
            catch (Exception ex)
            {
                string message = string.Format("Error(CraftSQLCommand): {0}", ex.ToString());
                Console.WriteLine(message);
                return message;
            }
        }

        /// <summary>
        /// This Method Is For Checking If The userID is Valid(WIP)
        /// </summary>
        /// <param name="userID">The ID Of The User To Disable</param>
        /// <returns>Return True or False For userID Validity</returns>
        public bool IsUserIDValid(string userID)
        {
            try
            {
                Console.WriteLine(userID);
                if ((!(string.IsNullOrEmpty(userID))) && (userID.Length == 10) && (userID.All(char.IsDigit)))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid User Hash Format");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error(UserHashCheck): " + ex.ToString());
                return false;
            }
        }
    }
}
