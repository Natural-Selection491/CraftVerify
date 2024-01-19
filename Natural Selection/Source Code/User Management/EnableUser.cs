using DataAccessLibraryCraftVerify;
using NaturalSelection.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalSelection.UserManagement.EnableAccount
{
    public class EnableUser
    {
        private SQLServerDAO _sqlServerDAO { get; set; }
        public EnableUser()
        {
            _sqlServerDAO = new SQLServerDAO();
        }

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

        private string CraftEnableSQLCommand(Dictionary<string, object> newValues)
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

        public bool EnableAccount(string userID)
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
                    var connString = GetDataFromConfigFile("ConnectionString");
                    Dictionary<string, object> changeToEnable = new Dictionary<string, object> {
                        { "userID", string.Format("{0}", userID)},
                        { "userStatus", 1 },
                        { "firstAuthenticationFailTimestamp", "NULL"},
                        { "failedAuthenticationAttempts", 0}
                    };
                    var rowsAffected = _sqlServerDAO.InsertAttribute(connString, CraftEnableSQLCommand(changeToEnable));
                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    else { return false; }
                }
            }
            catch (Exception ex2)
            {
                var message = string.Format("Error(CraftSQLCommand): {0}", ex2.ToString());
                Console.WriteLine(message);
                return false;
            }
        }

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
