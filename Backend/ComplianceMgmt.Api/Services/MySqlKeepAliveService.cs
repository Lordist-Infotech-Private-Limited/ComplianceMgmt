namespace ComplianceMgmt.Api.Services
{
    using Microsoft.Extensions.Hosting;
    using MySqlConnector;
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public class MySqlKeepAliveService : BackgroundService
    {
        private readonly string _connectionString = "Server=mysql5050.site4now.net;Database=db_a927ee_comlian;User Id=a927ee_comlian;Password=P@ssw0rd;Pooling=true;Keepalive=60;ConnectionTimeout=60;DefaultCommandTimeout=1200;MinPoolSize=5;MaxPoolSize=50;";
        private MySqlConnection _connection;

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
