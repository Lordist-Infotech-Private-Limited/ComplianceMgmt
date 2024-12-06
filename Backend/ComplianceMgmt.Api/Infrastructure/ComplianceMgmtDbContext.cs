using MySqlConnector;
using Polly;
using Polly.Retry;
using System.Data;

namespace ComplianceMgmt.Api.Infrastructure
{
    public class ComplianceMgmtDbContext(IConfiguration configuration)
    {
        private readonly string _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        private readonly AsyncRetryPolicy _retryPolicy = Policy
                .Handle<MySqlException>() // Retry on MySQL exceptions
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

        /// <summary>
        /// Creates a new MySQL connection with a retry policy.
        /// </summary>
        public async Task<IDbConnection> CreateDefaultConnectionAsync()
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var connection = new MySqlConnection(_defaultConnectionString);
                await connection.OpenAsync();
                return connection;
            });
        }

        /// <summary>
        /// Creates a new MySQL connection for the secondary database.
        /// </summary>
        public async Task<IDbConnection> CreateSecondaryConnectionAsync(string serverIp, string dbName, string userName, string password)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var connectionString = $"Server={serverIp};Database={dbName};User Id={userName};Password={password};Allow Zero Datetime=True;Convert Zero Datetime=True;";
                var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                return connection;
            });
        }
    }
}
