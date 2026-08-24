using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace KhayelitshaLibraryApp
{
    internal static class DatabaseHelper
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["LibraryDB"]?.ConnectionString
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=KhayelitshaLibraryDB;Integrated Security=True;TrustServerCertificate=True;";

        public static string ConnectionInfo => ConnectionString;

        public static SqlConnection GetConnection() => new(ConnectionString);

        public static bool TestConnection(out string errorMessage)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var command = new SqlCommand("SELECT DB_NAME()", connection);
                command.ExecuteScalar();
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            connection.Open();
            return ExecuteNonQuery(connection, null, query, parameters);
        }

        public static int ExecuteNonQuery(SqlConnection connection, SqlTransaction? transaction,
            string query, params SqlParameter[] parameters)
        {
            using var command = new SqlCommand(query, connection, transaction);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);
            return command.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            connection.Open();
            return command.ExecuteScalar();
        }

        public static object? ExecuteScalar(SqlConnection connection, SqlTransaction? transaction,
            string query, params SqlParameter[] parameters)
        {
            using var command = new SqlCommand(query, connection, transaction);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);
            return command.ExecuteScalar();
        }

        public static void ExecuteTransaction(Action<SqlConnection, SqlTransaction> action)
        {
            using var connection = GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                action(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static bool HasActiveLoan(int copyId)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM Loan WHERE CopyID = @CopyID AND ReturnDate IS NULL",
                new SqlParameter("@CopyID", copyId));
            return Convert.ToInt32(result) > 0;
        }

        public static bool MemberHasLoans(int memberId)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM Loan WHERE MemberID = @MemberID",
                new SqlParameter("@MemberID", memberId));
            return Convert.ToInt32(result) > 0;
        }
    }
}
