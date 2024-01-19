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
        public int InsertAttribute(string connString, string sqlcommand)
        {
            // Synchronous implementation
            // You can keep the existing synchronous method signatures in the interface
            return Task.Run(() => InsertAttributeAsync(connString, sqlcommand)).Result;
        }

        public ICollection<object>? GetAttribute(string connString, string sqlcommand)
        {
            // Synchronous implementation
            // You can keep the existing synchronous method signatures in the interface
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
