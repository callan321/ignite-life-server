using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IgniteLifeApi.Tests.Infrastructure
{
    public static class PostgresTestDatabaseHelper
    {
        public static string CreateUniqueDatabase(string baseConnectionString, out string dbName)
        {
            dbName = $"ignite_test_{Guid.NewGuid():N}";
            var csb = new NpgsqlConnectionStringBuilder(baseConnectionString)
            {
                Database = dbName
            };

            // Create DB by connecting to postgres
            var adminCsb = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = "postgres" };
            using var adminConn = new NpgsqlConnection(adminCsb.ConnectionString);
            adminConn.Open();

            using var cmd = new NpgsqlCommand($@"CREATE DATABASE ""{dbName}"";", adminConn);
            cmd.ExecuteNonQuery();

            return csb.ToString();
        }

        public static void DropDatabase(string baseConnectionString, string dbName)
        {
            var adminCsb = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = "postgres" };
            using var adminConn = new NpgsqlConnection(adminCsb.ConnectionString);
            adminConn.Open();

            // Terminate existing connections before dropping
            using (var terminateCmd = new NpgsqlCommand($@"
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = '{dbName}' AND pid <> pg_backend_pid();", adminConn))
            {
                terminateCmd.ExecuteNonQuery();
            }

            using var dropCmd = new NpgsqlCommand($@"DROP DATABASE IF EXISTS ""{dbName}"";", adminConn);
            dropCmd.ExecuteNonQuery();
        }

        public static TContext CreateMigratedContext<TContext>(string connectionString)
            where TContext : DbContext
        {
            var options = new DbContextOptionsBuilder<TContext>()
                .UseNpgsql(connectionString)
                .Options;

            var db = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
            db.Database.Migrate();
            return db;
        }
    }
}
