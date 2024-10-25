using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplianceMgmt.Api.Infrastructure
{
    public class ComplianceMgmtDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ComplianceMgmtDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
