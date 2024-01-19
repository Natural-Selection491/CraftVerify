//dotnet add package MySql.Data
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace DataAccessLibraryCraftVerify
{
    public class MySQLDAO : IReadOnlyDAO, IWriteOnlyDAO
    {
          public string GetDataFromConfigFile(string variable)
        {
            // Set the full path to the config file
            var configFilePath = @"C:\Users\vanan\teamSynology\CECS 491A\Milestone_2_analysis\config.local.txt"; // change to your location here

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
        public int InsertAttribute( string sqlcommand)
        {
            // Synchronous implementation
            // You can keep the existing synchronous method signatures in the interface
            string connString = GetDataFromConfigFile("Connection String");
            return Task.Run(() => InsertAttributeAsync(connString, sqlcommand)).Result;
        }

        public ICollection<object>? GetAttribute(string sqlcommand)
        {
            // Synchronous implementation
            // You can keep the existing synchronous method signatures in the interface
            string connString = GetDataFromConfigFile("Connection String");
            return Task.Run(() => GetAttributeAsync(connString, sqlcommand)).Result;
        }
        public async Task<int> InsertAttributeAsync(string connString, string sqlcommand)
        {
            #region Validate Arguments
            if (connString == null)
            {
                throw new ArgumentNullException();
            }
            if (sqlcommand == null)
            {
                throw new ArgumentNullException();
            }
            #endregion
            int rowsaffected = 0;
            using (MySqlConnection connection = new MySqlConnection(connString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted))
                {
                    using (MySqlCommand command = new MySqlCommand(sqlcommand, connection, transaction))
                    {
                        try
                        {
                            rowsaffected += await command.ExecuteNonQueryAsync();
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }

            }
            return rowsaffected;
        }

        public async Task<ICollection<object>?> GetAttributeAsync(string connString, string sqlcommand)
        {
            #region Validate arguments
            if (connString == null)
            {
                throw new ArgumentNullException();
            }
            if (sqlcommand == null)
            {
                throw new ArgumentNullException();
            }
            #endregion
            DbDataReader? read = null;
            ICollection<object>? attributevalue = null;
            using (MySqlConnection connection = new MySqlConnection(connString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted))
                {
                    using (MySqlCommand command = new MySqlCommand(sqlcommand, connection, transaction))
                    {
                        using (read = await command.ExecuteReaderAsync())
                        {
                            while (await read.ReadAsync())
                            {
                                var values = new object[read.FieldCount];
                                read.GetValues(values);
                                attributevalue.Add(values);
                            }
                        }
                    }
                }
            }
            return attributevalue;
        }

    }
}
