using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DataTesting.UnitTest.Helpers
{
    public interface IDatabaseHelper<T>
    {
        /// <summary>
        /// Load all csv file inside a database table
        /// </summary>
        /// <param name="table">Name of table</param>
        /// <param name="data">DataTable containing data of csv data file</param>
        /// <returns>If loaded will return true</returns>
        bool LoadData(string table, DataTable data);

        /// <summary>
        /// Execute Rollback command if this class is injected
        /// </summary>
        /// <returns>True if rollback ocurred</returns>
        bool Rollback();

        /// <summary>
        /// Get information about SQL Connection (Singleton Injection)
        /// </summary>
        /// <returns>Sql Connection object</returns>
        SqlConnection GetInfoSqlConnection();

        /// <summary>
        /// Get information about SQL Transaction (Singleton Injection)
        /// </summary>
        /// <returns>Sql Transaction object</returns>
        SqlTransaction GetInfoSqlTransaction();

        /// <summary>
        /// Execute a database stored procedure that will return a list of results
        /// </summary>
        /// <param name="procedure">Name of stored procedure</param>
        /// <param name="parameters">Parameters to execute procedure</param>
        /// <returns>Typed list of database results as expected</returns>
        IEnumerable<T> ExecuteReader(string procedure, object parameters = null);

        /// <summary>
        /// Execute a database stored procedure that will return an integer (insert, update or delete commands)
        /// </summary>
        /// <param name="procedure">Name of stored procedure</param>
        /// <param name="parameters">Parameters to execute procedure</param>
        /// <returns>The number of rows affected</returns>
        int Execute(string procedure, object parameters = null);

        /// <summary>
        /// Execute a SQL Command that will not return results (commands like SET IDENTITY_INSERT table ON)
        /// </summary>
        /// <param name="command">SQL Command</param>
        /// <param name="parameters">Parameters to execute the command</param>
        /// <returns>The number of rows affected</returns>
        int ExecuteSql(string command, object parameters = null);

        /// <summary>
        /// Execute a SQL Command on database and type the result as expected
        /// </summary>
        /// <param name="command">SQL Command</param>
        /// <param name="parameters">Parameters to execute the command</param>
        /// <returns>Typed list of results as expected</returns>
        IEnumerable<T> SelectManually(string command, object parameters = null);

        /// <summary>
        /// Execute a SQL Command that will result one line and one column (commands like SELECT SCOPE_IDENTITY())
        /// </summary>
        /// <param name="command">SQL Command</param>
        /// <param name="parameters">Parameters to execute the command</param>
        /// <returns>Expected integer value from the command</returns>
        int SelectManuallyScalar(string command, object parameters = null);
    }


    /// <summary>
    /// This class implemnts database access and trasaction using Dapper.
    /// </summary>
    /// <typeparam name="T">Type of object expected of database execution expected</typeparam>
    public class DatabaseHelper<T> : IDatabaseHelper<T>
    {
        private readonly SqlConnection connection;
        private readonly SqlTransaction transaction;

        /// <summary>
        /// On injected will configure database connection and transaction
        /// </summary>
        public DatabaseHelper()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["database"].ConnectionString);
            if (connection.State != ConnectionState.Open)
                connection.Open();

            if (transaction == null)
                transaction = connection.BeginTransaction();

        }

        public bool LoadData(string table, DataTable data)
        {
            var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction);
            sqlBulkCopy.DestinationTableName = table;
            sqlBulkCopy.WriteToServer(data);
            return true;
        }

        public IEnumerable<T> ExecuteReader(string procedure, object parameters = null)
        {
            return connection.Query<T>(procedure, parameters, transaction, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        public int Execute(string procedure, object parameters = null)
        {
            return connection.Execute(procedure, parameters, transaction, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        public int ExecuteSql(string command, object parameters = null)
        {
            return connection.Execute(command, parameters, transaction, commandTimeout: 0, commandType: CommandType.Text);
        }

        public int SelectManuallyScalar(string command, object parameters = null)
        {
            return Convert.ToInt32(connection.ExecuteScalar(command, parameters, transaction, commandTimeout: 0, commandType: CommandType.Text));
        }

        public IEnumerable<T> SelectManually(string command, object parameters = null)
        {
            return connection.Query<T>(command, parameters, transaction, commandTimeout: 0, commandType: CommandType.Text);
        }

        public SqlConnection GetInfoSqlConnection()
        {
            return connection;
        }

        public SqlTransaction GetInfoSqlTransaction()
        {
            return transaction;
        }

        public bool Rollback()
        {
            if (connection.State == ConnectionState.Open && transaction != null) { 
                transaction.Rollback();
                return true;
            }

            return false;
        }
    }
}
