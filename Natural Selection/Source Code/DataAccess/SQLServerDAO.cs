using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace DataAccessLibraryCraftVerify
{
    public class SQLServerDAO : IReadOnlyDAO, IWriteOnlyDAO
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
            using (SqlConnection connection = new SqlConnection(connString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted) as SqlTransaction)
                {
                    using (SqlCommand command = new SqlCommand(sqlcommand, connection, transaction))
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

        public async Task<List<object>?> GetAttributeAsync(string connString, string sqlcommand)
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
            SqlDataReader? read = null;
            List<object>? attributevalue = new List<object>();
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                using (var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted) as SqlTransaction)
                {
                    using (SqlCommand command = new SqlCommand(sqlcommand, connection, transaction))
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
