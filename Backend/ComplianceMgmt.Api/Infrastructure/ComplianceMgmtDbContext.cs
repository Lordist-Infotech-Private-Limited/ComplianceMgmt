using MySql.Data.MySqlClient;
using System.Data;

namespace ComplianceMgmt.Api.Infrastructure
{
    public class ComplianceMgmtDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _defaultConnectionString;
        public ComplianceMgmtDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_defaultConnectionString);
        }

        public IDbConnection CreateClientConnection(string serverIp, string dbName, string userName, string password)
        {
            var connectionString = $"Server={serverIp};Database={dbName};User Id={userName};Password={password};Pooling=true;";
            return new MySqlConnection(connectionString);
        }
    }
}
