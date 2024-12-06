using MySqlConnector;

namespace ComplianceMgmt.Api.Extensions
{
    public static class MySqlRetryHelper
    {
        public static async Task ExecuteWithRetryAsync(Func<Task> action, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    await action();
                    return;
                }
                catch (MySqlException ex) when (ex.IsTransient)
                {
                    Console.WriteLine($"Transient error: {ex.Message}. Retrying...");
                    retryCount++;
                    await Task.Delay(2000); // Wait before retrying
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Non-retryable error: {ex.Message}");
                    throw;
                }
            }
        }
    }

}
