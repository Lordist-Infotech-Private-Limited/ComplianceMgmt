using MySqlConnector;
using System.Data;

namespace ComplianceMgmt.Api.Services
{
    public class MySqlKeepAliveService : BackgroundService
    {
        private MySqlConnection _connection;
        private readonly string _connectionString;

        // Inject IConfiguration to access connection string
        public MySqlKeepAliveService(IConfiguration configuration)
        {
            // Get the connection string from configuration (e.g., appsettings.json)
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_connection == null || _connection.State != ConnectionState.Open)
                    {
                        Console.WriteLine("Re-establishing MySQL connection...");
                        _connection = new MySqlConnection(_connectionString);
                        await _connection.OpenAsync(stoppingToken);
                    }

                    // Lightweight keep-alive query
                    using var command = new MySqlCommand("SELECT 1;", _connection);
                    await command.ExecuteScalarAsync(stoppingToken);

                    Console.WriteLine("Keep-alive successful.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during keep-alive: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Retry after a short delay
                }
            }

            // Clean up connection
            if (_connection?.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }
}
